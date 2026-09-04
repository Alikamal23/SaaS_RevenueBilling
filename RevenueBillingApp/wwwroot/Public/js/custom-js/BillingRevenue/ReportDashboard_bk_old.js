$(document).ready(function () {
    // Client dropdowns — 3 alag jagah reuse hoga
    GetClientInfoDDL(["#ddlContractListClient", "#ddlInvoiceListClient", "#ddlInvoiceDueClient"]);

    // Industry dropdown — sirf Client List modal ke liye
    GetIndustryDDL("#ddlClientListIndustry");

    // ---- Modal 1: Client List ----
    $("#btnViewClientListReport").click(function () {
        var orderBy = $("input[name='clOrderBy']:checked").val();
        var industryId = $("#ddlClientListIndustry").val();
        var industryName = $("#ddlClientListIndustry option:selected").text();
        var showExcel = $("#chkClientListExcel").is(":checked");

        var queryParams = [];

        queryParams.push("OrderBy=" + encodeURIComponent(orderBy));
        queryParams.push("industry_id=" + encodeURIComponent(industryId));
        queryParams.push("industry_name=" + encodeURIComponent(industryName));
        queryParams.push("ShowExcel=" + showExcel);

        var queryString = "?" + queryParams.join("&");

        var url = "/BillingRevenue/rpt_ClientReport" + queryString;

        window.open(url, "_blank");
    });

    // ---- Modal 2: Client Contract List ----
    $("#btnViewContractListReport").click(function () {
        var clientId = $("#ddlContractListClient").val();
        var clientName = $("#ddlContractListClient option:selected").text();

        //if (clientId == "0") {
        //    Swal.fire({
        //        icon: "warning",
        //        title: "Please select a client."
        //    });
        //    return;
        //}

        var showExcel = $("#chkContractListExcel").is(":checked");

        var queryParams = [];

        queryParams.push("ClientId=" + encodeURIComponent(clientId));
        queryParams.push("ClientName=" + encodeURIComponent(clientName));
        queryParams.push("ShowExcel=" + showExcel);

        var url = "/BillingRevenue/rpt_ClientContractReport?" + queryParams.join("&");

        window.open(url, "_blank");
    });

    // ---- Modal 3: Client Invoice List ----
    $("#btnViewInvoiceListReport").click(function () {
        var clientId = $("#ddlInvoiceListClient").val();
        var clientName = $("#ddlInvoiceListClient option:selected").text();

        //if (clientId == "0") {
        //    Swal.fire({
        //        icon: "warning",
        //        title: "Please select a client."
        //    });
        //    return;
        //}

        var showExcel = $("#chkInvoiceListExcel").is(":checked");

        var queryParams = [];

        queryParams.push("ClientId=" + encodeURIComponent(clientId));
        queryParams.push("ClientName=" + encodeURIComponent(clientName));
        queryParams.push("ShowExcel=" + showExcel);

        var url = "/BillingRevenue/rpt_ClientInvoiceReport?" + queryParams.join("&");

        window.open(url, "_blank");
    });

    // ---- Modal 4: Invoice Due Report ----
    $("#btnViewInvoiceDueReport").click(function () {
        var clientId = $("#ddlInvoiceDueClient").val();
        var clientName = $("#ddlInvoiceDueClient option:selected").text();

        //if (clientId == "0") {
        //    Swal.fire({
        //        icon: "warning",
        //        title: "Please select a client."
        //    });
        //    return;
        //}

        var showExcel = $("#chkInvoiceDueExcel").is(":checked");

        var queryParams = [];

        queryParams.push("ClientId=" + encodeURIComponent(clientId));
        queryParams.push("ClientName=" + encodeURIComponent(clientName));
        queryParams.push("ShowExcel=" + showExcel);

        var url = "/BillingRevenue/rpt_InvoiceDueReport?" + queryParams.join("&");

        window.open(url, "_blank");
    });



    $(document).on('shown.bs.modal', '.modal', function () {

        $(this).find(
            '#ddlContractListClient, #ddlInvoiceListClient, #ddlInvoiceDueClient, #ddlClientListIndustry'
        ).each(function () {

            if ($(this).hasClass("select2-hidden-accessible")) {
                return;
            }

            $(this).select2({
                width: "100%",
                dropdownParent: $(this).closest('.modal')
            });

        });

    });



});

// Client dropdown — ek dafa API call, saare dropdowns populate
function GetClientInfoDDL(targetSelectors) {
    new APICALL(GetGlobalURL('Base', 'GetClientInfoDDL'), 'GET', '', true).FETCH((result, error) => {
        if (error) return;

        if (result.data != null) {
            $.each(targetSelectors, function (i, selector) {
                var ddl = $(selector);
                ddl.empty().append('<option value="0">Please select</option>');

                $.each(result.data, function (j, opt) {
                    ddl.append(`<option value="${opt.ddlvalue}">${opt.ddltext}</option>`);
                });
            });
        }
    });
}

// Industry dropdown
function GetIndustryDDL(targetSelector) {
    new APICALL(GetGlobalURL('Base', 'GetIndustryDDL'), 'GET', '', true).FETCH((result, error) => {
        if (error) return;

        if (result.data != null) {
            var ddl = $(targetSelector);
            ddl.empty().append('<option value="0">Please select</option>');

            $.each(result.data, function (i, opt) {
                ddl.append(`<option value="${opt.ddlvalue}">${opt.ddltext}</option>`);
            });
        }
    });
}
