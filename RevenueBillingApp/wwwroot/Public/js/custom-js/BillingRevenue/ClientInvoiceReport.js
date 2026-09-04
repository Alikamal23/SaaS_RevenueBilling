var instanceid = 0;

$(document).ready(function () {
    $("#ddlClient").select2({
        placeholder: "Please select client",
        width: "100%"
    });

    GetAllClient_DDL();

    $('#btnGetReport').on('click', function () {
        var res = ValidateAll();
        if (res == false) {
            return false;
        }

        GetReport();
    });

});

function ValidateAll() {
    var isValid = true;

    ////Validate Item if checkbox is checked
    //var clientId = $("#ddlClient").val();
    //if (!clientId || clientId === "0") {
    //    Swal.fire({
    //        icon: 'warning',
    //        text: 'Please select Client!',
    //        confirmButtonColor: "#61affe"
    //    });
    //    return false;
    //}


    return isValid;
}
function GetAllClient_DDL() {
    new APICALL(GetGlobalURL('Base', 'GetClientInfoDDL'), 'GET', '', true).FETCH((result, error) => {
        if (result) {
            if (result.data != null) {
                window.allUsers = result.data;

                var ddl = $('#ddlClient');
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
function GetReport() {
    var queryParams = [];

    var clientId = $('#ddlClient').val();
    var client_name = $('#ddlClient option:selected').text();


    // Construct query string accordingly
    if (clientId) queryParams.push("ClientId=" + encodeURIComponent(clientId));
    if (client_name) queryParams.push("ClientName=" + encodeURIComponent(client_name));


    var blnExcel = $('#chkShowExcel').is(':checked') ? "true" : "false";
    queryParams.push("ShowExcel=" + blnExcel);

    var queryString = "?" + queryParams.join("&");
    var url = "/BillingRevenue/rpt_ClientInvoiceReport" + queryString;

    // Open the URL in a new tab
    window.open(url, "_newtab");

}
