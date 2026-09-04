var instanceid = 0;
const options = {
    margin: [0.5, 0, 0.5, 0],
    filename: 'filename.pdf',
    image: {
        type: 'jpeg',
        quality: 0.98
    },
    html2canvas: {
        scale: 2
    },
    jsPDF: {
        unit: 'in',
        format: 'a4',
        orientation: 'landscape' //'portrait'
    },
    pagebreak: { avoid: '.avoid-break' }
}

var obj;
let reportData = [];
let currentPage = 1;
const rowsPerPage = 10;

$(document).ready(function () {
    var ddl = $("#ddlDueClient");

    ddl.select2({
        width: "100%"
    });

    GetClientInfoDDL();

    $("#btnViewDueInvoice").click(function () {
        var clientId = $("#ddlDueClient").val() || "0";
        var monthVal = $("#txtDueMonth").val();   // "2026-08" format

        var fromDate = monthVal ? monthVal + "-01" : null;   // "2026-08-01"

        var payload = {
            ClientId: clientId,
            FromDate: fromDate
        };

        new APICALL(GetGlobalURL('Base', 'GetNextUnpaidInvoiceDue'), 'POST', JSON.stringify(payload), true)
            .FETCH((result, error) => {

                if (error) {
                    Swal.fire({
                        icon: 'info',
                        title: 'No Data',
                        text: 'No unpaid invoices found for the selected filters.'
                    });
                    return;
                }

                var data = result.data || [];
                var clientName = clientId == "0" ? "All Clients" : $("#ddlDueClient option:selected").text();
                var monthLabel = monthVal ? FormatMonthLabel(monthVal) : "All Months";

                RenderNextUnpaidInvoiceDue(data, clientName, monthLabel);

                $("#reportFilterCard").hide();
                $("#reportResult").show();
                $("#btnBackToParameters").show();
            });
    });

    $(document).on('click', '.pagination-btn', function () {
        var page = parseInt($(this).data('page'));

        changePage(page);
    });

    // Back button
    $(document).on('click', '#btnBackToParameters', function () {
        $("#reportFilterCard").show();
        $("#reportResult").hide().empty();
        $("#btnBackToParameters").hide();
        reportData = [];
        currentPage = 1;
    });

    // Export Excel
    $(document).on('click', '#btnExportDueInvoiceExcel', function () {
        ExportDueInvoiceExcel();
    });

});


function GetClientInfoDDL() {
    new APICALL(GetGlobalURL('Base', 'GetClientInfoDDL'), 'GET', '', true).FETCH((result, error) => {
        if (result) {
            if (result.data != null) {
                window.allUsers = result.data;

                var ddl = $('#ddlDueClient');
                populateDropdowns(ddl, result.data, null, 0);
            }
        }
        if (error) {
            Swal.fire({
                icon: 'error',
                title: 'Error...',
                text: error.data.responseText,
                footer: ''
            });
        }

    });
}

/* Populate Dropdowns */
function populateDropdowns(ddl, users, selectedManagerId = null, customfield) {
    ddl.empty().append('<option value="0" selected>Please select</option>');

    $.each(users, function (i, option) {
        if (option.ddlvalue != selectedManagerId) {

            if (customfield == 0) {
                ddl.append(
                    `<option value="${option.ddlvalue}">${option.ddltext}</option>`
                );
            } else {
                ddl.append(
                    `<option value="${option.ddlvalue}" data-customfield="${option.customfield}">${option.ddltext}</option>`
                );
            }
        }
    });
}


// ================================================
// RENDER — Report ka poora HTML banata hai (header/params + table skeleton)
// ================================================
function RenderNextUnpaidInvoiceDue(data, clientName, monthLabel) {
    reportData = data || [];
    currentPage = 1;

    //console.log(reportData);
    //console.log(reportData[0].invoice_id);

    var html = `
        <div class="report-wrapper">
            <div class="report-title-bar text-center py-4">
                <h4 class="mb-1">NEXT UNPAID INVOICE DUE</h4>
                <p class="mb-0">Client Due Invoice Report</p>
            </div>
            <div class="card-body">
                <div class="d-flex justify-content-between align-items-start mb-3">
                    <div class="d-flex" style="gap: 40px;">
                        <div>
                            <p class="text-muted mb-1">Client Name</p>
                            <p class="fw-bold">${clientName}</p>
                        </div>
                        <div>
                            <p class="text-muted mb-1">Month</p>
                            <p class="fw-bold">${monthLabel}</p>
                        </div>
                        <div>
                            <p class="text-muted mb-1">Report Date</p>
                            <p class="fw-bold">${FormatDateDisplay(new Date())}</p>
                        </div>
                    </div>
                    <button type="button" class="btn btn-outline-success" id="btnExportDueInvoiceExcel">
                        <i class="feather-download me-1"></i>Excel
                    </button>
                </div>

                <div class="table-responsive">
                    <table class="table table-bordered mb-0">
                        <thead>
                            <tr>
                                <th>S#</th><th>Client Name</th><th>Invoice No</th><th>Contract Ref#</th>
                                <th>Contract Type</th><th>Billing Period</th>
                                <th>Invoice Date</th><th>Due Date</th><th>Invoice Amount</th>
                                <th>Total Amount</th><th>Invoice Status</th><th>Payment Status</th><th>Actions</th>
                            </tr>
                        </thead>
                        <tbody id="dueInvoiceTbody"></tbody>
                    </table>
                </div>

                <div class="report-footer-pagination mt-3">
                    <p class="text-muted small mb-0">
                        Generated on: ${FormatDateDisplay(new Date())} &nbsp; Mettis Global | Confidential
                    </p>

                    <div id="duePaginationContainer"></div>
                </div>
            </div>
        </div>
    `;

    $("#reportResult").html(html);

    renderReportPage();
    createPagination();
}

// ================================================
// PAGINATION — ek page ka data table body mein daalta hai
// ================================================
function renderReportPage() {
    var start = (currentPage - 1) * rowsPerPage;
    var end = start + rowsPerPage;
    var pageData = reportData.slice(start, end);

    var rows = "";

    if (pageData.length === 0) {
        rows = `<tr><td colspan="14" class="text-center text-muted py-3">No unpaid invoices found.</td></tr>`;
    } else {
        $.each(pageData, function (i, r) {

            //console.log(reportData);
            //console.log(reportData[0].invoice_id);
            //console.log(r.invoice_id);

            // =========================
            // Invoice Status
            // =========================

            var invoiceStatus = "";

            switch (r.invoice_status) {
                case "Pending":
                    invoiceStatus =
                        '<span class="badge bg-warning text-dark">Pending</span>';
                    break;

                case "Paid":
                    invoiceStatus =
                        '<span class="badge bg-success">Paid</span>';
                    break;

                case "Over Due":
                    invoiceStatus =
                        '<span class="badge bg-danger">Over Due</span>';
                    break;

                case "Cancelled":
                    invoiceStatus =
                        '<span class="badge bg-secondary">Cancelled</span>';
                    break;

                default:
                    invoiceStatus =
                        r.invoice_status ?? '';
                    break;

            }


            // =========================
            // Payment Status
            // =========================

            var paymentStatus = "";

            if (r.payment_status == "Paid") {
                paymentStatus =
                    '<span class="badge bg-success">Paid</span>';

            }
            else {
                paymentStatus =
                    '<span class="badge bg-danger">Unpaid</span>';

            }

            var action = '';
            var primaryKey;

            primaryKey = parseInt(r.invoice_id);
            ////console.log(primaryKey);

            //action += '<button type="button" class="btn btn-sm btn-info ViewInvoice1 me-1" ';
            //action += 'data-id="' + primaryKey + '" ';
            //action += 'data-row="' + i + '">';
            //action += '<i class="fa fa-eye"></i></button>';

            //action += '<button type="button" class="btn btn-sm btn-success DownloadInvoice1 me-1" ';
            //action += 'data-id="' + primaryKey + '">';
            //action += '<i class="fa fa-download"></i></button>';

            action += '<button type="button" class="btn btn-sm btn-info invoice-action-btn ViewInvoice1 me-1" ';
            action += 'data-id="' + primaryKey + '" ';
            action += 'data-row="' + i + '">';
            action += '<i class="fa fa-eye"></i></button>';

            action += '<button type="button" class="btn btn-sm btn-success invoice-action-btn DownloadInvoice1 me-1" ';
            action += 'data-id="' + primaryKey + '">';
            action += '<i class="fa fa-download"></i></button>';


            //action += '<button type="button" class="btn btn-sm btn-primary EmailInvoice1" ';
            //action += 'data-id="' + primaryKey + '">';
            //action += '<i class="fa fa-envelope"></i></button>';


            rows += `
                <tr>
                    <td>${start + i + 1}</td>
                    <td>${r.client_name || ''}</td>
                    <td>${r.invoice_no || ''}</td>
                    <td>${r.contract_refno || ''}</td>
                    <td>${r.contract_type || ''}</td>
                    <td>${FormatDateDisplay(r.billing_period)}</td>
                    <td>${FormatDateDisplay(r.invoice_date)}</td>
                    <td>${FormatDateDisplay(r.due_date)}</td>
                    <td>${parseFloat(r.invoice_amount || 0).toLocaleString()}</td>
                    <td>${parseFloat(r.total_amount || 0).toLocaleString()}</td>
                    <td>${invoiceStatus || ''}</td>
                    <td>${paymentStatus || ''}</td>
                    <td>${action}</td>
                </tr>`;
        });


        // ================================================
        // INVOICE ACTION BUTTONS
        // ================================================

        // View Invoice
        $(document).off("click", ".ViewInvoice1").on("click", ".ViewInvoice1", function (e) {
            e.preventDefault();
            e.stopPropagation();

            var invoiceId = $(this).data("id");
            var rowIndex = $(this).data("row");
            //console.log("View Invoice:", invoiceId, rowIndex);
            ViewInvoice(invoiceId, rowIndex);
        });


        // Download Invoice
        $(document).off("click", ".DownloadInvoice1").on("click", ".DownloadInvoice1", function (e) {
            e.preventDefault();
            e.stopPropagation();

            var invoiceId = $(this).data("id");
            //console.log("Download Invoice:", invoiceId);
            DownloadInvoice(invoiceId);
        });


        // Email Invoice
        $(document).off("click", ".EmailInvoice1").on("click", ".EmailInvoice1", function (e) {
            e.preventDefault();
            e.stopPropagation();

            var invoiceId = $(this).data("id");
            //console.log("Email Invoice:", invoiceId);
            EmailInvoice(invoiceId);
        });


    }

    $("#dueInvoiceTbody").html(rows);
}

function createPagination() {
    var totalPages = Math.ceil(reportData.length / rowsPerPage);
    var html = '';

    if (totalPages <= 1) {
        $("#duePaginationContainer").html('');
        return;
    }

    html += `<button type="button" class="btn btn-sm btn-outline-primary me-1 pagination-btn"
                data-page="${currentPage - 1}" ${currentPage === 1 ? 'disabled' : ''}>Previous</button>`;

    for (var i = 1; i <= totalPages; i++) {
        html += `<button type="button" class="btn btn-sm ${i === currentPage ? 'btn-primary' : 'btn-outline-primary'} me-1 pagination-btn"
                    data-page="${i}">${i}</button>`;
    }

    html += `<button type="button" class="btn btn-sm btn-outline-primary pagination-btn"
                data-page="${currentPage + 1}" ${currentPage === totalPages ? 'disabled' : ''}>Next</button>`;

    $("#duePaginationContainer").html(html);
}

function changePage(page) {
    var totalPages = Math.ceil(reportData.length / rowsPerPage);
    if (page < 1 || page > totalPages) return;

    currentPage = page;
    renderReportPage();
    createPagination();
}

// ================================================
// HELPERS
// ================================================
function FormatMonthLabel(monthVal) {
    var parts = monthVal.split("-");
    var months = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];
    return months[parseInt(parts[1]) - 1] + " " + parts[0];
}

function FormatDateDisplay(dateVal) {
    if (!dateVal) return "";
    var d = new Date(dateVal);
    if (isNaN(d)) return "";
    var dd = String(d.getDate()).padStart(2, '0');
    var mm = String(d.getMonth() + 1).padStart(2, '0');
    return dd + "-" + mm + "-" + d.getFullYear();
}

// ================================================
// EXCEL EXPORT (SheetJS ya server-side, jo bhi pattern chal raha ho)
// ================================================
function ExportDueInvoiceExcel() {
    if (!reportData || reportData.length === 0) {
        Swal.fire({ icon: 'warning', title: 'No Data', text: 'There is no data to export.' });
        return;
    }

    var exportData = reportData.map(function (r, i) {
        return {
            "S#": i + 1,
            "Client Name": r.client_name || '',
            "Invoice No": r.invoice_no || '',
            "Contract Ref#": r.contract_refno || '',
            "Contract Type": r.contract_type || '',
            "Billing Period": FormatDateDisplay(r.billing_period),
            "Invoice Date": FormatDateDisplay(r.invoice_date),
            "Due Date": FormatDateDisplay(r.due_date),
            "Invoice Amount": r.invoice_amount || 0,
            "Total Amount": r.total_amount || 0,
            "Invoice Status": r.invoice_status || '',
            "Payment Status": r.payment_status || ''
        };
    });

    var ws = XLSX.utils.json_to_sheet(exportData);
    var wb = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(wb, ws, "Next Unpaid Invoice Due");
    XLSX.writeFile(wb, "NextUnpaidInvoiceDue_" + FormatDateDisplay(new Date()) + ".xlsx");
}
