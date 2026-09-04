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

//var obj = JSON.parse(sessionStorage.getItem("obj"));
var obj;
var qs;
let reportData = [];
let currentPage = 1;
const rowsPerPage = 10;

$('document').ready(function () {
    qs = getQueryParams();
    //console.log(qs);

    var showExcel = (qs.ShowExcel === 'true') ? true : false;

    obj = {
        ClientId: qs.ClientId,
        ClientName: qs.ClientName,
        FromDate: qs.FromDate ? qs.FromDate : null,
        ToDate: qs.ToDate ? qs.ToDate : null,
        OrderBy: qs.OrderBy,
        ShowExcel: showExcel
    };

    //console.log(obj);
    Show_Report_Preview(obj);

    $(document).on('click', '.pagination-btn', function () {

        var page = parseInt($(this).data('page'));

        changePage(page);

    });

    $('#btnExportExcel').on('click', function () {
        ExportReportExcel();
    });

    $('#btnExportPDF').on('click', function () {
        ExportReportPDF();
    });

});

function Show_Report_Preview(obj) {
    setTableHeaders();
    setReportParameters();   // <-- ADD THIS

    new APICALL(GetGlobalURL('Base', 'GetInvoiceDueReport'), 'POST', JSON.stringify(obj), true).FETCH((result, error) => {
        if (error) {
            Swal.fire({
                icon: 'error',
                title: 'Error...',
                text: error.data.responseText,
                footer: ''
            });
            return;
        }

        if (result && result.data) {
            ////console.log(result.data);

            CreateMultiSelectionTable(result.data);

            if (obj.ShowExcel) {
                new JSONTOEXCEL().Export(result.data, "Client Due Invoice Report", "Report", []);

            }
        }

    });
}
function setReportParameters() {

    // ClientName
    var clientName = "All Clients";

    if (qs.ClientId && qs.ClientId != "0") {
        // Agar report form se industry name query string mein aa raha hai
        clientName = qs.ClientName || qs.ClientName || "Selected Client";
    }

    //// Order By
    //var orderBy = qs.OrderBy || "ClientName";

    //switch (orderBy) {
    //    case "ClientName":
    //        orderBy = "Client Name";
    //        break;

    //    case "IndustryName":
    //        orderBy = "Industry Name";
    //        break;

    //    case "CountryName":
    //        orderBy = "Country Name";
    //        break;

    //    case "OnboardingStartDate":
    //        orderBy = "Onboarding Start Date";
    //        break;

    //    default:
    //        orderBy = orderBy;
    //}

    // Report Date
    var reportDate = formatDate(new Date());

    $("#reportClient").text(clientName);
    //$("#reportOrderBy").text(orderBy);
    $("#reportDate").text(reportDate);
    $("#generatedDate").text(reportDate);
}
function CreateMultiSelectionTable(data) {
    reportData = data;
    currentPage = 1;

    renderPage();
}
function renderPage() {
    $('#tblChecklist').empty();

    if (reportData.length == 0) {
        $('#tblChecklist').append(
            '<tr><td colspan="12" class="text-center">No Data To Display</td></tr>'
        );

        $('#pagination').hide();
        return;
    }

    $('#pagination').show();

    var start = (currentPage - 1) * rowsPerPage;
    var end = Math.min(start + rowsPerPage, reportData.length);

    for (var i = start; i < end; i++) {
        appendRow(reportData[i], i + 1);
    }

    createPagination();
}
function createPagination() {

    var totalPages = Math.ceil(reportData.length / rowsPerPage);

    var html = '';

    html += `
        <button type="button"
                class="btn btn-sm btn-outline-primary me-1 pagination-btn"
                data-page="${currentPage - 1}"
                ${currentPage == 1 ? 'disabled' : ''}>
            Previous
        </button>
    `;

    for (var i = 1; i <= totalPages; i++) {

        html += `
            <button type="button"
                    class="btn btn-sm ${i == currentPage ? 'btn-primary' : 'btn-outline-primary'} me-1 pagination-btn"
                    data-page="${i}">
                ${i}
            </button>
        `;
    }

    html += `
        <button type="button"
                class="btn btn-sm btn-outline-primary pagination-btn"
                data-page="${currentPage + 1}"
                ${currentPage == totalPages ? 'disabled' : ''}>
            Next
        </button>
    `;

    $('#pagination').html(html);
}
function changePage(page) {

    var totalPages = Math.ceil(reportData.length / rowsPerPage);

    if (page < 1 || page > totalPages) {
        return;
    }

    currentPage = page;

    renderPage();
}
function setTableHeaders() {

    $("#tableHeaders").html(`
        <tr>
            <th>S#</th>
            <th>Client Name</th>
            <th>Invoice No</th>
            <th>Contract Ref#</th>
            <th>Contract Type</th>
            <th>Milestone No</th>
            <th>Billing Period</th>
            <th>Invoice Date</th>
            <th>Due Date</th>
            <th>Invoice Amount</th>
            <th>Tax Amount</th>
            <th>Total Amount</th>
            <th>Invoice Status</th>
            <th>Payment Status</th>
            <th>Invoice Type</th>
            <th>PO No</th>
            <th>Remarks</th>
        </tr>
    `);
}
function appendRow(data, index) {

    // Invoice Status Badge
    var invoiceStatus = "";

    switch (data.invoice_status) {
        case "Pending":
            invoiceStatus = '<span class="badge bg-warning text-dark">Pending</span>';
            break;

        case "Paid":
            invoiceStatus = '<span class="badge bg-success">Paid</span>';
            break;

        case "Over Due":
            invoiceStatus = '<span class="badge bg-danger">Over Due</span>';
            break;

        case "Cancelled":
            invoiceStatus = '<span class="badge bg-secondary">Cancelled</span>';
            break;

        default:
            invoiceStatus = data.invoice_status ?? '';
            break;
    }


    // Payment Status Badge
    var paymentStatus = "";

    if (data.payment_status == "Paid") {
        paymentStatus = '<span class="badge bg-success">Paid</span>';
    }
    else {
        paymentStatus = '<span class="badge bg-danger">Unpaid</span>';
    }

    var row = `
        <tr class="avoid-break">

            <td class="text-center">${index}</td>

            <td>${data.client_name ?? ''}</td>

            <td>${data.invoice_no ?? ''}</td>

            <td>${data.contract_refno ?? ''}</td>

            <td>${data.contract_type ?? ''}</td>

            <td class="text-center">${data.milestone_no ?? ''}</td>

            <td>${formatDate(data.billing_period) ?? ''}</td>

            <td>${formatDate(data.invoice_date)}</td>

            <td>${formatDate(data.due_date)}</td>

            <td class="text-end">
                ${formatAmount(data.invoice_amount)}
            </td>

            <td class="text-end">
                ${formatAmount(data.tax_amount)}
            </td>

            <td class="text-end">
                ${formatAmount(data.total_amount)}
            </td>

            <td>${invoiceStatus}</td>

            <td>${paymentStatus}</td>

            <td>${data.invoice_type ?? ''}</td>

            <td>${data.PONo ?? ''}</td>

            <td>${data.remarks ?? ''}</td>

        </tr>
    `;

    $('#tblChecklist').append(row);
}
function formatDate(receivedDate) {

    if (!receivedDate)
        return '';

    var fullDate = new Date(receivedDate);

    if (isNaN(fullDate.getTime()))
        return '';

    var month = String(fullDate.getMonth() + 1).padStart(2, '0');
    var day = String(fullDate.getDate()).padStart(2, '0');
    var year = fullDate.getFullYear();

    return day + "-" + month + "-" + year;
}
function formatAmount(amount) {

    if (amount == null || amount === "" || isNaN(amount))
        return "0.00";

    return Number(amount).toLocaleString("en-US", {
        minimumFractionDigits: 2,
        maximumFractionDigits: 2
    });
}
function getQueryParams() {
    var params = {};
    var queryString = window.location.search.substring(1); // Get the query string without the "?"

    if (queryString) {
        var queryParams = queryString.split("&"); // Split into key-value pairs
        queryParams.forEach(function (param) {
            var pair = param.split("="); // Split key and value
            var key = decodeURIComponent(pair[0]);
            var value = decodeURIComponent(pair[1]);
            params[key] = value; // Add key-value pair to the object
        });
    }
    return params;
}
function ExportReportExcel() {

    if (!reportData || reportData.length === 0) {
        Swal.fire({
            icon: 'warning',
            title: 'No Data',
            text: 'There is no data to export.'
        });
        return;
    }

    new JSONTOEXCEL().Export(
        reportData,
        "Client Report",
        "Client Report",
        []
    );
}
function ExportReportPDF() {

    if (!reportData || reportData.length === 0) {
        Swal.fire({
            icon: 'warning',
            title: 'No Data',
            text: 'There is no data available to export.'
        });
        return;
    }

    if (typeof html2pdf === 'undefined') {
        Swal.fire({
            icon: 'error',
            title: 'PDF Error',
            text: 'PDF library is not loaded.'
        });
        return;
    }

    var element = document.querySelector('.report-wrapper');

    if (!element) {
        Swal.fire({
            icon: 'error',
            title: 'PDF Error',
            text: 'Report section not found.'
        });
        return;
    }

    // Current page save karo
    var oldPage = currentPage;

    // PDF ke liye temporarily ALL records show karo
    currentPage = 1;

    $('#tblChecklist').empty();

    for (var i = 0; i < reportData.length; i++) {
        appendRow(reportData[i], i + 1);
    }

    // Pagination hide
    $('#pagination').hide();

    // Export buttons hide
    $('.report-export-buttons').hide();

    var pdfOptions = {
        margin: [0.35, 0.2, 0.45, 0.2],

        filename: 'Client Report.pdf',

        image: {
            type: 'jpeg',
            quality: 0.98
        },

        html2canvas: {
            scale: 2,
            useCORS: true,
            backgroundColor: '#ffffff'
        },

        jsPDF: {
            unit: 'in',
            format: 'a4',
            orientation: 'landscape'
        },

        pagebreak: {
            mode: ['css', 'legacy'],
            avoid: ['tr']
        }
    };

    html2pdf()
        .set(pdfOptions)
        .from(element)
        .save()
        .then(function () {

            // Buttons wapas
            $('.report-export-buttons').show();

            // Pagination / screen table wapas
            currentPage = oldPage;
            renderPage();

        })
        .catch(function (error) {

            console.error('PDF Error:', error);

            // UI restore
            $('.report-export-buttons').show();

            currentPage = oldPage;
            renderPage();

            Swal.fire({
                icon: 'error',
                title: 'PDF Error',
                text: 'Unable to generate PDF.'
            });
        });
}
