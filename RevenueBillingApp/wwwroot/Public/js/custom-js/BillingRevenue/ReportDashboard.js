let clientReportData = [];
let clientCurrentPage = 1;
const clientRowsPerPage = 10;

let contractReportData = [];
let contractCurrentPage = 1;
const contractRowsPerPage = 10;

let invoiceReportData = [];
let invoiceCurrentPage = 1;
const invoiceRowsPerPage = 10;

let invoiceDueReportData = [];
let invoiceDueCurrentPage = 1;
const invoiceDueRowsPerPage = 10;

let currentReportData = [];
let currentReportType = "";

// Generic pagination state — ek object mein sab kuch
var reportPagination = {
    ClientList: { data: [], page: 1, rowsPerPage: 10 },
    ContractList: { data: [], page: 1, rowsPerPage: 10 },
    InvoiceList: { data: [], page: 1, rowsPerPage: 10 },
    InvoiceDue: { data: [], page: 1, rowsPerPage: 10 }
};


$(document).ready(function () {
    GetClientInfoDDL();
    GetIndustryDDL();

    LoadReportParameters("ClientList");

    $("#ddlReportType").change(function () {
        var reportType = $(this).val();

        ////alert(reportType);

        // Reset previous report
        $("#reportResult").html("");
        $("#reportResult").hide();

        // Reset common export data
        currentReportData = [];
        currentReportType = "";

        LoadReportParameters(reportType);
    });

    $("#btnViewReport").click(function () {
        ViewSelectedReport();

        // Jahan bhi result final render ho raha hai (RenderClientReport/RenderContractReport/etc ke baad), yeh add karo:
        $("#reportFilterCard").hide();
        $("#reportResult").show();
        $("#btnBackToParameters").show();

    });

    // NAYA: Back button handler
    $(document).on("click", "#btnBackToParameters", function () {
        $("#reportFilterCard").show();
        $("#reportResult").hide().empty();
        $("#btnBackToParameters").hide();
    });

    $(document).on(
        'click',
        '.client-pagination-btn',
        function () {

            if ($(this).prop('disabled')) {
                return;
            }

            var page = parseInt(
                $(this).attr('data-page')
            );

            changeClientReportPage(page);
        }
    );


    $(document).on(
        'click',
        '.contract-pagination-btn',
        function () {

            if ($(this).prop('disabled')) {
                return;
            }


            var page = parseInt(
                $(this).attr('data-page')
            );


            changeContractReportPage(page);
        }
    );


    $(document).on(
        'click',
        '.invoice-pagination-btn',
        function () {

            if ($(this).prop('disabled')) {
                return;
            }


            var page =
                parseInt(
                    $(this).attr('data-page')
                );


            changeInvoiceReportPage(page);

        }
    );


    $(document).on(
        'click',
        '.invoice-due-pagination-btn',
        function () {

            if ($(this).prop('disabled')) {
                return;
            }


            var page =
                parseInt(
                    $(this).attr('data-page')
                );


            changeInvoiceDueReportPage(page);

        }
    );


    $(document).on('click', '.report-pagination-btn', function () {
        if ($(this).prop('disabled')) return;

        var page = parseInt($(this).attr('data-page'));
        var reportType = $(this).attr('data-report');

        var renderMap = {
            ClientList: renderClientReportPage,
            ContractList: renderContractReportPage,
            InvoiceList: renderInvoiceReportPage,
            InvoiceDue: renderInvoiceDueReportPage
        };

        changeReportPage(reportType, page, renderMap[reportType]);
    });





});

function LoadReportParameters(reportType) {
    var html = "";

    ////console.log("Report Type:", reportType);

    // Remove old Export Excel checkbox if it exists
    $("#reportParameters")
        .find("#chkExportExcel")
        .closest(".form-check")
        .remove();

    if (reportType === "ClientList") {

        html = GetClientListParameterHtml();

    }
    else {
        console.log("Other Report");

        html = GetClientOnlyParameterHtml();
    }

    $("#reportParameters").html(html);

    if (reportType === "ClientList") {
        FillIndustryDropdown("#ddlReportIndustry");

    }
    else {
        FillClientDropdown("#ddlReportClient");
    }
}

function GetClientListParameterHtml() {

    return `
        <div class="row g-3">

            <div class="col-md-6">

                <label class="form-label fw-bold">
                    Order By
                </label>

                <div class="form-check">
                    <input class="form-check-input"
                           type="radio"
                           name="orderBy"
                           value="ClientName"
                           checked>

                    <label class="form-check-label">
                        Client Name
                    </label>
                </div>

                <div class="form-check">
                    <input class="form-check-input"
                           type="radio"
                           name="orderBy"
                           value="IndustryName">

                    <label class="form-check-label">
                        Industry Name
                    </label>
                </div>

                <div class="form-check">
                    <input class="form-check-input"
                           type="radio"
                           name="orderBy"
                           value="CountryName">

                    <label class="form-check-label">
                        Country Name
                    </label>
                </div>

                <div class="form-check">
                    <input class="form-check-input"
                           type="radio"
                           name="orderBy"
                           value="OnboardingStartDate">

                    <label class="form-check-label">
                        Onboarding Start Date
                    </label>
                </div>

            </div>


            <div class="col-md-6">

                <label class="form-label fw-bold">
                    Industry
                </label>

                <select class="form-select"
                        id="ddlReportIndustry">

                    <option value="0">
                        Please select
                    </option>

                </select>

            </div>

        </div>
    `;
}

function GetClientOnlyParameterHtml() {

    return `
        <div class="row">

            <div class="col-md-6">

                <label class="form-label fw-bold">
                    Client
                </label>

                <select class="form-select"
                        id="ddlReportClient">

                    <option value="0">
                        Please select
                    </option>

                </select>

            </div>

        </div>

    `;
}


function GetClientInfoDDL() {

    new APICALL(
        GetGlobalURL('Base', 'GetClientInfoDDL'),
        'GET',
        '',
        true
    ).FETCH((result, error) => {

        if (error)
            return;

        if (result.data != null) {

            window.clientDDLData = result.data;

        }

    });

}

function FillClientDropdown(selector) {

    var ddl = $(selector);

    ddl.empty();

    ddl.append(
        '<option value="0">Please select</option>'
    );

    $.each(window.clientDDLData || [], function (i, opt) {

        ddl.append(`
            <option value="${opt.ddlvalue}">
                ${opt.ddltext}
            </option>
        `);

    });

    ddl.select2({
        width: "100%"
    });

}


function GetIndustryDDL() {

    new APICALL(
        GetGlobalURL('Base', 'GetIndustryDDL'),
        'GET',
        '',
        true
    ).FETCH((result, error) => {

        if (error)
            return;

        if (result.data != null) {

            window.industryDDLData = result.data;

        }

    });

}

function FillIndustryDropdown(selector) {

    var ddl = $(selector);

    ddl.empty();

    ddl.append(
        '<option value="0">Please select</option>'
    );

    $.each(window.industryDDLData || [], function (i, opt) {

        ddl.append(`
            <option value="${opt.ddlvalue}">
                ${opt.ddltext}
            </option>
        `);

    });

    ddl.select2({
        width: "100%"
    });

}

function ViewSelectedReport() {
    var reportType = $("#ddlReportType").val();
    var container = $("#reportResult");

    container.show();

    if (reportType === "ClientList") {
        LoadClientReport();

    }
    else if (reportType === "ContractList") {
        LoadContractReport();

    }
    else if (reportType === "InvoiceList") {
        LoadInvoiceReport();

    }
    else if (reportType === "InvoiceDue") {
        LoadInvoiceDueReport();

    }

}

function LoadClientReport() {

    var orderBy =
        $("input[name='orderBy']:checked").val();

    var industryId =
        $("#ddlReportIndustry").val();

    var industryName =
        $("#ddlReportIndustry option:selected").text();

    // Agar Please select hai to report mein All Industries show hoga
    if (!industryId || industryId === "0") {
        industryName = "All Industries";
    }

    var showExcel =
        $("#chkExportExcel").is(":checked");


    var obj = {

        industry_id: industryId,

        industry_name: industryName,

        FromDate: null,

        ToDate: null,

        OrderBy: orderBy,

        ShowExcel: showExcel

    };


    $("#reportResult").html(`
        <div class="text-center p-4">
            <i class="fas fa-spinner fa-spin"></i>
            Loading report...
        </div>
    `);


    new APICALL(
        GetGlobalURL('Base', 'GetClientReport'),
        'POST',
        JSON.stringify(obj),
        true
    ).FETCH((result, error) => {

        if (error) {

            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: error.data.responseText
            });

            return;
        }


        if (result && result.data) {

            // obj bhi RenderClientReport ko pass karna hai
            RenderClientReport(result.data, obj);

        }

    });

}

function RenderClientReport(data, obj) {

    clientReportData = data || [];
    currentReportData = clientReportData;   // ✅ yeh line add karo
    currentReportType = "ClientList";
    clientCurrentPage = 1;
    

    var html = `

        <div class="report-wrapper">

            <div class="report-header">

                <div class="report-title-section">

                    <div class="report-title">
                        LIST OF ALL CLIENTS
                    </div>

                    <div class="report-subtitle">
                        Client Information Report
                    </div>

                </div>

            </div>


            <div class="report-parameters ml-2">

                <div class="parameters-title
                            d-flex
                            justify-content-between
                            align-items-center
                            ms-2">

                    <span>
                        Report Parameters
                    </span>

                    <div class="report-export-buttons m-1">

                        <button type="button"
                                id="btnExportExcel"
                                class="btn btn-sm btn-outline-success">

                            <i class="fas fa-file-excel"></i>

                        </button>

                        <!--
                        <button type="button"
                                id="btnExportPDF"
                                class="btn btn-sm btn-outline-danger">

                            <i class="fas fa-file-pdf"></i>

                        </button>
                        -->

                    </div>

                </div>


                <div class="row ms-2">

                    <div class="col-md-4">

                        <div class="parameter-label">
                            Industry Name
                        </div>

                        <div class="parameter-value">
                           ${obj.industry_name}
                        </div>

                    </div>


                    <div class="col-md-4">

                        <div class="parameter-label">
                            Order By
                        </div>

                        <div class="parameter-value">
                            ${getOrderByText(obj.OrderBy)}
                        </div>

                    </div>


                    <div class="col-md-4">

                        <div class="parameter-label">
                            Report Date
                        </div>

                        <div class="parameter-value">
                            ${formatDate(new Date())}
                        </div>

                    </div>

                </div>

            </div>


            <div class="report-data">

                <div class="table-responsive">

                    <table class="table report-table">

                        <thead>

                            <tr>
                                <th>S#</th>
                                <th>Client Name</th>
                                <th>Client Ref#</th>
                                <th>Industry</th>
                                <th>Company Size</th>
                                <th>Website</th>
                                <th>NTN #</th>
                                <th>Contact Name</th>
                                <th>Contact Email</th>
                                <th>Contact Phone</th>
                                <th>Country</th>
                                <th>Onboarding Start Date</th>
                            </tr>

                        </thead>

                        <tbody id="clientReportBody">

                        </tbody>

                    </table>

                </div>

                <!-- PAGINATION -->
                <div id="clientReportPagination"
                     class="d-flex justify-content-center align-items-center mt-3 mb-3">
                </div>


            </div>

        </div>

    `;


    $("#reportResult").html(html);


    //var tbody = $("#clientReportBody");

    //$.each(data, function (i, item) {

    //    tbody.append(`

    //        <tr>

    //            <td>${i + 1}</td>

    //            <td>${item.client_name ?? ''}</td>

    //            <td>${item.client_refno ?? ''}</td>

    //            <td>${item.IndustryName ?? ''}</td>

    //            <td>${item.CompanySizeName ?? ''}</td>

    //            <td>${item.website ?? ''}</td>

    //            <td>${item.tax_registration_no ?? ''}</td>

    //            <td>${item.primary_contact_name ?? ''}</td>

    //            <td>${item.primary_contact_email ?? ''}</td>

    //            <td>${item.primary_contact_phone ?? ''}</td>

    //            <td>${item.CountryName ?? ''}</td>

    //            <td>${formatDate(item.onboarding_start_date)}</td>

    //        </tr>

    //    `);

    //});

    renderClientReportPage();


    // Automatically scroll to report
    $('html, body').animate({

        scrollTop: $("#reportResult").offset().top - 20

    }, 500);

}


function renderClientReportPage() {

    var tbody = $("#clientReportBody");

    tbody.empty();

    if (!clientReportData || clientReportData.length === 0) {

        tbody.append(`
            <tr>
                <td colspan="12" class="text-center">
                    No Data To Display
                </td>
            </tr>
        `);

        $("#clientReportPagination").hide();

        return;
    }

    $("#clientReportPagination").show();

    var start =
        (clientCurrentPage - 1) * clientRowsPerPage;

    var end =
        Math.min(
            start + clientRowsPerPage,
            clientReportData.length
        );


    for (var i = start; i < end; i++) {

        var item = clientReportData[i];

        tbody.append(`

            <tr>

                <td>${i + 1}</td>

                <td>${item.client_name ?? ''}</td>

                <td>${item.client_refno ?? ''}</td>

                <td>${item.IndustryName ?? ''}</td>

                <td>${item.CompanySizeName ?? ''}</td>

                <td>${item.website ?? ''}</td>

                <td>${item.tax_registration_no ?? ''}</td>

                <td>${item.primary_contact_name ?? ''}</td>

                <td>${item.primary_contact_email ?? ''}</td>

                <td>${item.primary_contact_phone ?? ''}</td>

                <td>${item.CountryName ?? ''}</td>

                <td>${formatDate(item.onboarding_start_date)}</td>

            </tr>

        `);
    }

    createClientReportPagination();
}

function createClientReportPagination() {

    var totalPages = Math.ceil(
        clientReportData.length / clientRowsPerPage
    );

    var html = '';


    // Previous
    html += `
        <button type="button"
                class="btn btn-sm btn-outline-primary me-1 client-pagination-btn"
                data-page="${clientCurrentPage - 1}"
                ${clientCurrentPage === 1 ? 'disabled' : ''}>
            Previous
        </button>
    `;


    // Page Numbers
    for (var i = 1; i <= totalPages; i++) {

        html += `
            <button type="button"
                    class="btn btn-sm ${i === clientCurrentPage
                ? 'btn-primary'
                : 'btn-outline-primary'
            } me-1 client-pagination-btn"
                    data-page="${i}">
                ${i}
            </button>
        `;
    }


    // Next
    html += `
        <button type="button"
                class="btn btn-sm btn-outline-primary client-pagination-btn"
                data-page="${clientCurrentPage + 1}"
                ${clientCurrentPage === totalPages ? 'disabled' : ''}>
            Next
        </button>
    `;


    $("#clientReportPagination").html(html);
}

function changeClientReportPage(page) {

    var totalPages = Math.ceil(
        clientReportData.length / clientRowsPerPage
    );

    if (page < 1 || page > totalPages) {
        return;
    }

    clientCurrentPage = page;

    renderClientReportPage();
}


function getOrderByText(orderBy) {

    switch (orderBy) {

        case "ClientName":
            return "Client Name";

        case "IndustryName":
            return "Industry Name";

        case "CountryName":
            return "Country Name";

        case "OnboardingStartDate":
            return "Onboarding Start Date";

        default:
            return "Client Name";
    }
}


function formatDate(receivedDate) {
    var fullDatestart = new Date(receivedDate);
    var twoDigitMonthstart = (fullDatestart.getMonth() + 1) + "";
    if (twoDigitMonthstart.length == 1)
        twoDigitMonthstart = "0" + twoDigitMonthstart;

    var twoDigitDatestart = fullDatestart.getDate() + "";
    if (twoDigitDatestart.length == 1)
        twoDigitDatestart = "0" + twoDigitDatestart;

    return fullDatestart.getFullYear() + "-" + twoDigitMonthstart + "-" + twoDigitDatestart;
}

function formatAmount(amount) {

    if (amount == null || amount === "" || isNaN(amount))
        return "0.00";

    return Number(amount).toLocaleString("en-US", {
        minimumFractionDigits: 2,
        maximumFractionDigits: 2
    });
}


function setReportParameters(obj) {

    // ============================
    // Industry
    // ============================

    var industryName = obj.industry_name || "All Industries";

    if (obj.industry_id === "0" || !obj.industry_id) {
        industryName = "All Industries";
    }


    // ============================
    // Order By
    // ============================

    var orderBy = obj.OrderBy || "ClientName";

    switch (orderBy) {

        case "ClientName":
            orderBy = "Client Name";
            break;

        case "IndustryName":
            orderBy = "Industry Name";
            break;

        case "CountryName":
            orderBy = "Country Name";
            break;

        case "OnboardingStartDate":
            orderBy = "Onboarding Start Date";
            break;

        default:
            break;
    }


    // ============================
    // Report Date
    // ============================

    var reportDate = formatDate(new Date());


    $("#reportIndustry").text(industryName);

    $("#reportOrderBy").text(orderBy);

    $("#reportDate").text(reportDate);

    $("#generatedDate").text(reportDate);
}



//======================================

function LoadContractReport() {

    var clientId =
        $("#ddlReportClient").val();

    var clientName =
        $("#ddlReportClient option:selected").text();

    if (!clientId || clientId === "0") {
        clientName = "All Clients";
    }

    var showExcel =
        $("#chkExportExcel").is(":checked");


    var obj = {

        ClientId: clientId,

        ClientName: clientName,

        ShowExcel: showExcel
    };


    $("#reportResult").html(`
        <div class="text-center p-4">
            <i class="fas fa-spinner fa-spin"></i>
            Loading contract report...
        </div>
    `);


    new APICALL(
        GetGlobalURL('Base', 'GetClientContractReport'),
        'POST',
        JSON.stringify(obj),
        true
    ).FETCH((result, error) => {

        if (error) {

            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: error.data.responseText
            });

            return;
        }


        if (result && result.data) {
            RenderContractReport(
                result.data,
                obj
            );

        }

    });

}

function RenderContractReport(data, obj) {
    contractReportData = data || [];
    currentReportType = "ContractList";
    contractCurrentPage = 1;

    var html = `

        <div class="report-wrapper">

            <div class="report-header">

                <div class="report-title-section">

                    <div class="report-title">
                        CLIENT CONTRACT LIST
                    </div>

                    <div class="report-subtitle">
                        Client Contract Information Report
                    </div>

                </div>

            </div>


            <div class="report-parameters ml-2">

                <div class="parameters-title
                            d-flex
                            justify-content-between
                            align-items-center
                            ms-2">

                    <span>
                        Report Parameters
                    </span>

                    <div class="report-export-buttons m-1">

                        <button type="button"
                                id="btnExportExcel"
                                class="btn btn-sm btn-outline-success">

                            <i class="fas fa-file-excel"></i>

                        </button>

                        <!--
                        <button type="button"
                                id="btnExportPDF"
                                class="btn btn-sm btn-outline-danger">

                            <i class="fas fa-file-pdf"></i>

                        </button>
                        -->

                    </div>

                </div>


                <div class="row ms-2">

                    <div class="col-md-4">

                        <div class="parameter-label">
                            Client Name
                        </div>

                        <div class="parameter-value">
                            ${obj.ClientName}
                        </div>

                    </div>


                    <div class="col-md-4">

                        <div class="parameter-label">
                            Report Date
                        </div>

                        <div class="parameter-value">
                            ${formatDate(new Date())}
                        </div>

                    </div>

                </div>

            </div>


            <div class="report-data">

                <div class="table-responsive">

                    <table class="table report-table">

                        <thead>

                            <tr>
                                <th>S#</th>
                                <th>Client Name</th>
                                <th>Client Ref#</th>
                                <th>Contract Ref#</th>
                                <th>Contract Type</th>
                                <th>Billing Start Date</th>
                                <th>Billing End Date</th>
                                <th>Billing Frequency</th>
                                <th>Contract Value</th>
                                <th>Discount %</th>
                                <th>Tax %</th>
                                <th>Total SaaS Amount</th>
                                <th>Project Name</th>
                                <th>Project Value</th>
                                <th>Milestones</th>
                                <th>Auto Renew</th>
                            </tr>

                        </thead>

                        <tbody id="contractReportBody">
                        </tbody>

                    </table>

                </div>


                <div id="contractReportPagination"
                     class="d-flex justify-content-center align-items-center mt-3 mb-3">
                </div>

            </div>

        </div>

    `;


    $("#reportResult").html(html);


    renderContractReportPage();


    $('html, body').animate({

        scrollTop: $("#reportResult").offset().top - 20

    }, 500);

}

function renderContractReportPage() {

    var tbody = $("#contractReportBody");

    tbody.empty();


    if (!contractReportData ||
        contractReportData.length === 0) {

        tbody.append(`
            <tr>
                <td colspan="16" class="text-center">
                    No Data To Display
                </td>
            </tr>
        `);

        $("#contractReportPagination").hide();

        return;
    }


    $("#contractReportPagination").show();


    var start =
        (contractCurrentPage - 1) *
        contractRowsPerPage;


    var end =
        Math.min(
            start + contractRowsPerPage,
            contractReportData.length
        );


    for (var i = start; i < end; i++) {

        var item = contractReportData[i];

        tbody.append(`

            <tr>

                <td>${i + 1}</td>

                <td>${item.client_name ?? ''}</td>

                <td>${item.client_refno ?? ''}</td>

                <td>${item.contract_refno ?? ''}</td>

                <td>${item.contractType1 ?? ''}</td>

                <td>${formatDate(item.bill_start_date)}</td>

                <td>${formatDate(item.bill_end_date)}</td>

                <td>${item.billingFrequency ?? ''}</td>

                <td>${item.contract_value ?? ''}</td>

                <td>${item.discount_percent ?? ''}</td>

                <td>${item.tax_percent ?? ''}</td>

                <td>${item.total_saas_amount ?? ''}</td>

                <td>${item.project_name ?? ''}</td>

                <td>${item.project_value ?? ''}</td>

                <td>${item.milestones ?? ''}</td>

                <td class="text-center">
                    ${item.auto_renew ? 'Yes' : 'No'}
                </td>

            </tr>

        `);
    }


    createContractReportPagination();
}

function createContractReportPagination() {

    var totalPages = Math.ceil(
        contractReportData.length /
        contractRowsPerPage
    );

    var html = '';


    html += `
        <button type="button"
                class="btn btn-sm btn-outline-primary me-1 contract-pagination-btn"
                data-page="${contractCurrentPage - 1}"
                ${contractCurrentPage === 1 ? 'disabled' : ''}>
            Previous
        </button>
    `;


    for (var i = 1; i <= totalPages; i++) {

        html += `
            <button type="button"
                    class="btn btn-sm ${i === contractCurrentPage
                ? 'btn-primary'
                : 'btn-outline-primary'
            } me-1 contract-pagination-btn"
                    data-page="${i}">
                ${i}
            </button>
        `;
    }


    html += `
        <button type="button"
                class="btn btn-sm btn-outline-primary contract-pagination-btn"
                data-page="${contractCurrentPage + 1}"
                ${contractCurrentPage === totalPages ? 'disabled' : ''}>
            Next
        </button>
    `;


    $("#contractReportPagination").html(html);
}

function changeContractReportPage(page) {

    var totalPages = Math.ceil(
        contractReportData.length /
        contractRowsPerPage
    );


    if (page < 1 || page > totalPages) {
        return;
    }


    contractCurrentPage = page;

    renderContractReportPage();
}

//=========================================

function LoadInvoiceReport() {

    var clientId =
        $("#ddlReportClient").val();

    var clientName =
        $("#ddlReportClient option:selected").text();

    if (!clientId || clientId === "0") {
        clientName = "All Clients";
    }

    var showExcel =
        $("#chkExportExcel").is(":checked");


    var obj = {

        ClientId: clientId,

        ClientName: clientName,

        ShowExcel: showExcel

    };


    $("#reportResult").html(`
        <div class="text-center p-4">
            <i class="fas fa-spinner fa-spin"></i>
            Loading invoice report...
        </div>
    `);


    new APICALL(
        GetGlobalURL('Base', 'GetClientInvoiceReport'),
        'POST',
        JSON.stringify(obj),
        true
    ).FETCH((result, error) => {

        if (error) {

            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: error.data.responseText
            });

            return;
        }


        if (result && result.data) {

            RenderInvoiceReport(
                result.data,
                obj
            );

        }

    });

}

function RenderInvoiceReport(data, obj) {

    invoiceReportData = data || [];
    invoiceCurrentPage = 1;

    // Common export data
    currentReportData = invoiceReportData;
    currentReportType = "InvoiceList";


    var html = `

        <div class="report-wrapper">

            <div class="report-header">

                <div class="report-title-section">

                    <div class="report-title">
                        CLIENT INVOICE LIST
                    </div>

                    <div class="report-subtitle">
                        Client Invoice Information Report
                    </div>

                </div>

            </div>


            <div class="report-parameters ml-2">

                <div class="parameters-title
                            d-flex
                            justify-content-between
                            align-items-center
                            ms-2">

                    <span>
                        Report Parameters
                    </span>


                    <div class="report-export-buttons m-1">

                        <button type="button"
                                id="btnExportExcel"
                                class="btn btn-sm btn-outline-success">

                            <i class="fas fa-file-excel"></i>

                        </button>

                        <!--
                        <button type="button"
                                id="btnExportPDF"
                                class="btn btn-sm btn-outline-danger">

                            <i class="fas fa-file-pdf"></i>

                        </button>
                        -->

                    </div>

                </div>


                <div class="row ms-2">

                    <div class="col-md-4">

                        <div class="parameter-label">
                            Client Name
                        </div>

                        <div class="parameter-value">
                            ${obj.ClientName}
                        </div>

                    </div>


                    <div class="col-md-4">

                        <div class="parameter-label">
                            Report Date
                        </div>

                        <div class="parameter-value">
                            ${formatDate(new Date())}
                        </div>

                    </div>

                </div>

            </div>


            <div class="report-data">

                <div class="table-responsive">

                    <table class="table report-table">

                        <thead>

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

                        </thead>


                        <tbody id="invoiceReportBody">
                        </tbody>

                    </table>

                </div>


                <div id="invoiceReportPagination"
                     class="d-flex justify-content-center align-items-center mt-3 mb-3">
                </div>

            </div>

        </div>

    `;


    $("#reportResult").html(html);


    renderInvoiceReportPage();


    $('html, body').animate({

        scrollTop:
            $("#reportResult").offset().top - 20

    }, 500);

}

function renderInvoiceReportPage() {

    var tbody =
        $("#invoiceReportBody");

    tbody.empty();


    if (!invoiceReportData ||
        invoiceReportData.length === 0) {

        tbody.append(`

            <tr>

                <td colspan="17"
                    class="text-center">

                    No Data To Display

                </td>

            </tr>

        `);

        $("#invoiceReportPagination").hide();

        return;
    }


    $("#invoiceReportPagination").show();


    var start =
        (invoiceCurrentPage - 1) *
        invoiceRowsPerPage;


    var end =
        Math.min(
            start + invoiceRowsPerPage,
            invoiceReportData.length
        );


    for (var i = start; i < end; i++) {

        var item =
            invoiceReportData[i];


        tbody.append(`

            <tr>

                <td class="text-center">
                    ${i + 1}
                </td>

                <td>
                    ${item.client_name ?? ''}
                </td>

                <td>
                    ${item.invoice_no ?? ''}
                </td>

                <td>
                    ${item.contract_refno ?? ''}
                </td>

                <td>
                    ${item.contractType1 ?? ''}
                </td>

                <td class="text-center">
                    ${item.milestone_no ?? ''}
                </td>

                <td>
                    ${item.billing_period ?? ''}
                </td>

                <td>
                    ${formatDate(item.invoice_date)}
                </td>

                <td>
                    ${formatDate(item.due_date)}
                </td>

                <td class="text-end">
                    ${formatAmount(item.invoice_amount)}
                </td>

                <td class="text-end">
                    ${formatAmount(item.tax_amount)}
                </td>

                <td class="text-end">
                    ${formatAmount(item.total_amount)}
                </td>

                <td>
                    ${item.invoice_status ?? ''}
                </td>

                <td>
                    ${item.payment_status ?? ''}
                </td>

                <td>
                    ${item.invoice_type ?? ''}
                </td>

                <td>
                    ${item.po_no ?? ''}
                </td>

                <td>
                    ${item.remarks ?? ''}
                </td>

            </tr>

        `);

    }


    createInvoiceReportPagination();

}

function createInvoiceReportPagination() {

    var totalPages =
        Math.ceil(
            invoiceReportData.length /
            invoiceRowsPerPage
        );


    var html = '';


    html += `

        <button type="button"
                class="btn btn-sm btn-outline-primary me-1 invoice-pagination-btn"
                data-page="${invoiceCurrentPage - 1}"
                ${invoiceCurrentPage === 1 ? 'disabled' : ''}>

            Previous

        </button>

    `;


    for (var i = 1; i <= totalPages; i++) {

        html += `

            <button type="button"
                    class="btn btn-sm ${i === invoiceCurrentPage
                ? 'btn-primary'
                : 'btn-outline-primary'
            } me-1 invoice-pagination-btn"
                    data-page="${i}">

                ${i}

            </button>

        `;

    }


    html += `

        <button type="button"
                class="btn btn-sm btn-outline-primary invoice-pagination-btn"
                data-page="${invoiceCurrentPage + 1}"
                ${invoiceCurrentPage === totalPages ? 'disabled' : ''}>

            Next

        </button>

    `;


    $("#invoiceReportPagination")
        .html(html);

}

function changeInvoiceReportPage(page) {

    var totalPages =
        Math.ceil(
            invoiceReportData.length /
            invoiceRowsPerPage
        );


    if (page < 1 || page > totalPages) {
        return;
    }


    invoiceCurrentPage = page;


    renderInvoiceReportPage();

}

function GetInvoicePDFRow(data, index) {

    return `

        <tr>

            <td>${index}</td>

            <td>${data.client_name ?? ''}</td>

            <td>${data.invoice_no ?? ''}</td>

            <td>${data.contract_refno ?? ''}</td>

            <td>${data.contractType1 ?? ''}</td>

            <td>${data.milestone_no ?? ''}</td>

            <td>${data.billing_period ?? ''}</td>

            <td>${formatDate(data.invoice_date)}</td>

            <td>${formatDate(data.due_date)}</td>

            <td>${formatAmount(data.invoice_amount)}</td>

            <td>${formatAmount(data.tax_amount)}</td>

            <td>${formatAmount(data.total_amount)}</td>

            <td>${data.invoice_status ?? ''}</td>

            <td>${data.payment_status ?? ''}</td>

            <td>${data.invoice_type ?? ''}</td>

            <td>${data.po_no ?? ''}</td>

            <td>${data.remarks ?? ''}</td>

        </tr>

    `;

}



//++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

function LoadInvoiceDueReport() {

    var clientId =
        $("#ddlReportClient").val();

    var clientName =
        $("#ddlReportClient option:selected").text();

    if (!clientId || clientId === "0") {
        clientName = "All Clients";
    }

    var showExcel =
        $("#chkExportExcel").is(":checked");


    var obj = {

        ClientId: clientId,

        ClientName: clientName,

        FromDate: null,

        ToDate: null,

        OrderBy: null,

        ShowExcel: showExcel

    };


    $("#reportResult").html(`
        <div class="text-center p-4">
            <i class="fas fa-spinner fa-spin"></i>
            Loading due invoice report...
        </div>
    `);


    new APICALL(
        GetGlobalURL('Base', 'GetInvoiceDueReport'),
        'POST',
        JSON.stringify(obj),
        true
    ).FETCH((result, error) => {

        if (error) {

            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: error.data.responseText
            });

            return;
        }


        if (result && result.data) {

            RenderInvoiceDueReport(
                result.data,
                obj
            );

        }

    });

}


function RenderInvoiceDueReport(data, obj) {
    invoiceDueReportData = data || [];
    invoiceDueCurrentPage = 1;

    // Common Export
    currentReportData = invoiceDueReportData;
    currentReportType = "InvoiceDue";


    var html = `
        <div class="report-wrapper">
            <div class="report-header">
                <div class="report-title-section">
                    <div class="report-title">
                        INVOICE DUE REPORT
                    </div>
                    <div class="report-subtitle">
                        Client Due Invoice Report
                    </div>
                </div>
            </div>

            <div class="report-parameters ml-2">
                <div class="parameters-title
                            d-flex
                            justify-content-between
                            align-items-center
                            ms-2">
                    <span>
                        Report Parameters
                    </span>

                    <div class="report-export-buttons m-1">
                        <button type="button"
                                id="btnExportExcel"
                                class="btn btn-sm btn-outline-success">

                            <i class="fas fa-file-excel"></i>
                        </button>

                        <!--
                        <button type="button"
                                id="btnExportPDF"
                                class="btn btn-sm btn-outline-danger">

                            <i class="fas fa-file-pdf"></i>

                        </button>
                        -->

                    </div>

                </div>


                <div class="row ms-2">

                    <div class="col-md-4">

                        <div class="parameter-label">
                            Client Name
                        </div>

                        <div class="parameter-value">
                            ${obj.ClientName}
                        </div>

                    </div>


                    <div class="col-md-4">

                        <div class="parameter-label">
                            Report Date
                        </div>

                        <div class="parameter-value">
                            ${formatDate(new Date())}
                        </div>

                    </div>

                </div>

            </div>


            <div class="report-data">

                <div class="table-responsive">

                    <table class="table report-table">

                        <thead>

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

                        </thead>


                        <tbody id="invoiceDueReportBody">
                        </tbody>

                    </table>

                </div>


                <div id="invoiceDueReportPagination"
                     class="d-flex justify-content-center align-items-center mt-3 mb-3">
                </div>

            </div>

        </div>

    `;


    $("#reportResult").html(html);


    renderInvoiceDueReportPage();


    $('html, body').animate({

        scrollTop:
            $("#reportResult").offset().top - 20

    }, 500);

}

function renderInvoiceDueReportPage() {

    var tbody =
        $("#invoiceDueReportBody");

    tbody.empty();


    if (!invoiceDueReportData ||
        invoiceDueReportData.length === 0) {

        tbody.append(`

            <tr>

                <td colspan="17"
                    class="text-center">

                    No Data To Display

                </td>

            </tr>

        `);

        $("#invoiceDueReportPagination").hide();

        return;
    }


    $("#invoiceDueReportPagination").show();


    var start =
        (invoiceDueCurrentPage - 1) *
        invoiceDueRowsPerPage;


    var end =
        Math.min(
            start + invoiceDueRowsPerPage,
            invoiceDueReportData.length
        );


    for (var i = start; i < end; i++) {

        appendInvoiceDueRow(
            invoiceDueReportData[i],
            i + 1
        );

    }


    createInvoiceDueReportPagination();

}

function appendInvoiceDueRow(data, index) {

    // =========================
    // Invoice Status
    // =========================

    var invoiceStatus = "";

    switch (data.invoice_status) {

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
                data.invoice_status ?? '';

            break;

    }


    // =========================
    // Payment Status
    // =========================

    var paymentStatus = "";

    if (data.payment_status == "Paid") {

        paymentStatus =
            '<span class="badge bg-success">Paid</span>';

    }
    else {

        paymentStatus =
            '<span class="badge bg-danger">Unpaid</span>';

    }


    var row = `

        <tr class="avoid-break">

            <td class="text-center">
                ${index}
            </td>


            <td>
                ${data.client_name ?? ''}
            </td>


            <td>
                ${data.invoice_no ?? ''}
            </td>


            <td>
                ${data.contract_refno ?? ''}
            </td>


            <td>
                ${data.contract_type ?? ''}
            </td>


            <td class="text-center">
                ${data.milestone_no ?? ''}
            </td>


            <td>
                ${formatDate(data.billing_period)}
            </td>


            <td>
                ${formatDate(data.invoice_date)}
            </td>


            <td>
                ${formatDate(data.due_date)}
            </td>


            <td class="text-end">
                ${formatAmount(data.invoice_amount)}
            </td>


            <td class="text-end">
                ${formatAmount(data.tax_amount)}
            </td>


            <td class="text-end">
                ${formatAmount(data.total_amount)}
            </td>


            <td>
                ${invoiceStatus}
            </td>


            <td>
                ${paymentStatus}
            </td>


            <td>
                ${data.invoice_type ?? ''}
            </td>


            <td>
                ${data.PONo ?? ''}
            </td>


            <td>
                ${data.remarks ?? ''}
            </td>

        </tr>

    `;


    $("#invoiceDueReportBody").append(row);

}

function createInvoiceDueReportPagination() {

    var totalPages =
        Math.ceil(
            invoiceDueReportData.length /
            invoiceDueRowsPerPage
        );


    var html = '';


    html += `

        <button type="button"
                class="btn btn-sm btn-outline-primary me-1 invoice-due-pagination-btn"
                data-page="${invoiceDueCurrentPage - 1}"
                ${invoiceDueCurrentPage === 1 ? 'disabled' : ''}>

            Previous

        </button>

    `;


    for (var i = 1; i <= totalPages; i++) {

        html += `

            <button type="button"
                    class="btn btn-sm ${i === invoiceDueCurrentPage
                ? 'btn-primary'
                : 'btn-outline-primary'
            } me-1 invoice-due-pagination-btn"
                    data-page="${i}">

                ${i}

            </button>

        `;

    }


    html += `

        <button type="button"
                class="btn btn-sm btn-outline-primary invoice-due-pagination-btn"
                data-page="${invoiceDueCurrentPage + 1}"
                ${invoiceDueCurrentPage === totalPages ? 'disabled' : ''}>

            Next

        </button>

    `;


    $("#invoiceDueReportPagination")
        .html(html);

}

function changeInvoiceDueReportPage(page) {

    var totalPages =
        Math.ceil(
            invoiceDueReportData.length /
            invoiceDueRowsPerPage
        );


    if (page < 1 || page > totalPages) {
        return;
    }


    invoiceDueCurrentPage = page;


    renderInvoiceDueReportPage();

}


function GetInvoiceDuePDFRow(data, index) {

    var invoiceStatus = "";

    switch (data.invoice_status) {

        case "Pending":
            invoiceStatus = "Pending";
            break;

        case "Paid":
            invoiceStatus = "Paid";
            break;

        case "Over Due":
            invoiceStatus = "Over Due";
            break;

        case "Cancelled":
            invoiceStatus = "Cancelled";
            break;

        default:
            invoiceStatus = data.invoice_status ?? '';
            break;
    }


    var paymentStatus =
        data.payment_status == "Paid"
            ? "Paid"
            : "Unpaid";


    return `

        <tr>

            <td>${index}</td>

            <td>${data.client_name ?? ''}</td>

            <td>${data.invoice_no ?? ''}</td>

            <td>${data.contract_refno ?? ''}</td>

            <td>${data.contract_type ?? ''}</td>

            <td>${data.milestone_no ?? ''}</td>

            <td>${formatDate(data.billing_period)}</td>

            <td>${formatDate(data.invoice_date)}</td>

            <td>${formatDate(data.due_date)}</td>

            <td>${formatAmount(data.invoice_amount)}</td>

            <td>${formatAmount(data.tax_amount)}</td>

            <td>${formatAmount(data.total_amount)}</td>

            <td>${invoiceStatus}</td>

            <td>${paymentStatus}</td>

            <td>${data.invoice_type ?? ''}</td>

            <td>${data.PONo ?? ''}</td>

            <td>${data.remarks ?? ''}</td>

        </tr>

    `;

}




//++++++++++++++++++++++++++++++++++++++++++++++++++++++++


$(document).on('click', '#btnExportExcel', function () {
    ExportReportExcel();
});

$(document).on('click', '#btnExportPDF', function () {
    ExportReportPDF();
});



function ExportReportExcel() {

    if (!currentReportData ||
        currentReportData.length === 0) {
        Swal.fire({
            icon: 'warning',
            title: 'No Data',
            text: 'There is no data to export.'
        });
        return;
    }

    var reportName = getCurrentReportName();

    new JSONTOEXCEL().Export(
        currentReportData,
        reportName,
        reportName,
        []
    );
}
function ExportReportPDF() {
    if (!currentReportData || currentReportData.length === 0) {
        Swal.fire({
            icon: 'warning',
            title: 'No Data',
            text: 'There is no data to export.'
        });
        return;
    }

    // =========================================
    // SAVE CURRENT PAGE
    // =========================================

    var oldPage = getCurrentPageNumber();

    // PDF ke liye first page
    setCurrentPageNumber(1);

    // First page render
    RerenderCurrentReportPage();


    // =========================================
    // WAIT FOR DOM RENDER
    // =========================================

    setTimeout(function () {
        // IMPORTANT:
        // Rerender ke BAAD wrapper dobara select karo
        var sourceElement = document.querySelector('.report-wrapper');

        if (!sourceElement) {
            setCurrentPageNumber(oldPage);
            RerenderCurrentReportPage();

            Swal.fire({
                icon: 'error',
                title: 'PDF Error',
                text: 'Report wrapper not found.'
            });
            return;
        }

        // =========================================
        // CLONE
        // =========================================
        var pdfElement = sourceElement.cloneNode(true);
        pdfElement.classList.add('pdf-report-clone');


        pdfElement.style.marginTop = '0';
        pdfElement.style.paddingTop = '0';

        // =========================================
        // REMOVE BUTTONS
        // =========================================
        $(pdfElement)
            .find('.report-export-buttons')
            .remove();


        // =========================================
        // REMOVE PAGINATION
        // =========================================

        $(pdfElement)
            .find('.report-pagination')
            .remove();

        $(pdfElement)
            .find('.pagination')
            .remove();




        // =========================================
        // APPEND CLONE
        // =========================================
        document.body.appendChild(pdfElement);


        // =========================================
        // PDF OPTIONS
        // =========================================
        var pdfOptions = {
            margin: [0.15, 0.15, 0.20, 0.15],
            filename: 'Client Report.pdf',
            image: {
                type: 'jpeg',
                quality: 0.98
            },

            html2canvas: {
                scale: 2,
                useCORS: true,
                backgroundColor: '#ffffff',
                logging: false
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


        // =========================================
        // GENERATE PDF
        // =========================================

        html2pdf()
            .set(pdfOptions)
            .from(pdfElement)
            .save()
            .then(function () {
                // Clone remove
                $(pdfElement).remove();

                // Original report restore
                setCurrentPageNumber(oldPage);
                RerenderCurrentReportPage();
            })

            .catch(function (error) {
                console.error('PDF Error:', error);

                // Clone remove
                $(pdfElement).remove();

                // Original report restore
                setCurrentPageNumber(oldPage);
                RerenderCurrentReportPage();

                Swal.fire({
                    icon: 'error',
                    title: 'PDF Error',
                    text: 'Unable to generate PDF.'
                });
            });

    }, 500);
}

function getCurrentPageNumber() {
    switch (currentReportType) {
        case "ClientList": return clientCurrentPage;
        case "ContractList": return contractCurrentPage;
        case "InvoiceList": return invoiceCurrentPage;
        case "InvoiceDue": return invoiceDueCurrentPage;
    }
}

function setCurrentPageNumber(page) {
    switch (currentReportType) {
        case "ClientList": clientCurrentPage = page; break;
        case "ContractList": contractCurrentPage = page; break;
        case "InvoiceList": invoiceCurrentPage = page; break;
        case "InvoiceDue": invoiceDueCurrentPage = page; break;
    }
}

function RerenderCurrentReportPage() {
    switch (currentReportType) {
        case "ClientList": renderClientReportPage(); break;
        case "ContractList": renderContractReportPage(); break;
        case "InvoiceList": renderInvoiceReportPage(); break;
        case "InvoiceDue": renderInvoiceDueReportPage(); break;
    }
}



function getCurrentReportName() {

    switch (currentReportType) {

        case "ClientList":
            return "Client Report";

        case "ContractList":
            return "Client Contract Report";

        case "InvoiceList":
            return "Client Invoice Report";

        case "InvoiceDue":
            return "Client Due Invoice Report";

        default:
            return "Report";
    }
}


function GetReportPDFRow(data, index) {

    switch (currentReportType) {

        case "ClientList":

            return GetClientPDFRow(data, index);


        case "ContractList":

            return GetContractPDFRow(data, index);


        case "InvoiceList":

            return GetInvoicePDFRow(data, index);


        case "InvoiceDue":

            return GetInvoiceDuePDFRow(data, index);


        default:

            return "";
    }
}

function GetClientPDFRow(data, index) {

    return `
        <tr>

            <td>${index}</td>

            <td>${data.client_name ?? ''}</td>

            <td>${data.client_refno ?? ''}</td>

            <td>${data.IndustryName ?? ''}</td>

            <td>${data.CompanySizeName ?? ''}</td>

            <td>${data.website ?? ''}</td>

            <td>${data.tax_registration_no ?? ''}</td>

            <td>${data.primary_contact_name ?? ''}</td>

            <td>${data.primary_contact_email ?? ''}</td>

            <td>${data.primary_contact_phone ?? ''}</td>

            <td>${data.CountryName ?? ''}</td>

            <td>${formatDate(data.onboarding_start_date)}</td>

        </tr>
    `;
}

function GetContractPDFRow(data, index) {

    return `
        <tr>

            <td>${index}</td>

            <td>${data.client_name ?? ''}</td>

            <td>${data.client_refno ?? ''}</td>

            <td>${data.contract_refno ?? ''}</td>

            <td>${data.contractType1 ?? ''}</td>

            <td>${formatDate(data.bill_start_date)}</td>

            <td>${formatDate(data.bill_end_date)}</td>

            <td>${data.billingFrequency ?? ''}</td>

            <td>${formatAmount(data.contract_value)}</td>

            <td>${formatAmount(data.discount_percent)}</td>

            <td>${formatAmount(data.tax_percent)}</td>

            <td>${formatAmount(data.total_saas_amount)}</td>

            <td>${data.project_name ?? ''}</td>

            <td>${formatAmount(data.project_value)}</td>

            <td>${data.no_of_milestones ?? 0}</td>

            <td>${data.auto_renew ? 'Yes' : 'No'}</td>

        </tr>
    `;
}

function createReportPagination(reportType, containerId, cssClass) {
    var state = reportPagination[reportType];
    var totalPages = Math.ceil(state.data.length / state.rowsPerPage);
    var html = '';

    html += `<button type="button" class="btn btn-sm btn-outline-primary me-1 ${cssClass}"
                data-page="${state.page - 1}" data-report="${reportType}"
                ${state.page === 1 ? 'disabled' : ''}>Previous</button>`;

    for (var i = 1; i <= totalPages; i++) {
        html += `<button type="button" class="btn btn-sm ${i === state.page ? 'btn-primary' : 'btn-outline-primary'} me-1 ${cssClass}"
                    data-page="${i}" data-report="${reportType}">${i}</button>`;
    }

    html += `<button type="button" class="btn btn-sm btn-outline-primary ${cssClass}"
                data-page="${state.page + 1}" data-report="${reportType}"
                ${state.page === totalPages ? 'disabled' : ''}>Next</button>`;

    $("#" + containerId).html(html);
}

function changeReportPage(reportType, page, rerenderFn) {
    var state = reportPagination[reportType];
    var totalPages = Math.ceil(state.data.length / state.rowsPerPage);

    if (page < 1 || page > totalPages) return;

    state.page = page;
    rerenderFn();
}


function GetClientOnlyParameterHtml() {
    return `
        <div class="row">
            <div class="col-md-6">
                <label class="form-label fw-bold">Client</label>
                <select class="form-select" id="ddlReportClient">
                    <option value="0">Please select</option>
                </select>
            </div>
        </div>
    `;
}

