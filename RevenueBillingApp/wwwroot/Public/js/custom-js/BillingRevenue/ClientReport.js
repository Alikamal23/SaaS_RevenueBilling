var instanceid = 0;

$(document).ready(function () {
    GetAllIndustry_DDL();

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
    //var ddlIndustry = $("#ddlIndustry").val();
    //if (!ddlIndustry || ddlIndustry === "0") {
    //    Swal.fire({
    //        icon: 'warning',
    //        text: 'Please select Industry Name!',
    //        confirmButtonColor: "#61affe"
    //    });
    //    return false;
    //}


    return isValid;
}
function GetAllIndustry_DDL() {
    new APICALL(GetGlobalURL('Base', 'GetIndustryDDL'), 'GET', '', true).FETCH((result, error) => {
        if (result) {
            if (result.data != null) {
                window.allUsers = result.data;

                var ddl = $('#ddlIndustry');
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


    var industry_id = $('#ddlIndustry').val();
    var industry_name = $('#ddlIndustry option:selected').text();

    // Construct query string accordingly
    if (industry_id) queryParams.push("industry_id=" + encodeURIComponent(industry_id));
    if (industry_name) queryParams.push("industry_name=" + encodeURIComponent(industry_name));

    // Order By
    var orderBy = "";

    if ($("#client_name").is(":checked")) {
        orderBy = "ClientName";
    }
    else if ($("#industry_name").is(":checked")) {
        orderBy = "IndustryName";
    }
    else if ($("#country_name").is(":checked")) {
        orderBy = "CountryName";
    }
    else if ($("#OnboardingstartDate").is(":checked")) {
        orderBy = "OnboardingStartDate";
    }

    queryParams.push("OrderBy=" + encodeURIComponent(orderBy));



    var blnExcel = $('#chkShowExcel').is(':checked') ? "true" : "false";
    queryParams.push("ShowExcel=" + blnExcel);

    var queryString = "?" + queryParams.join("&");
    var url = "/BillingRevenue/rpt_ClientReport" + queryString;

    // Open the URL in a new tab
    window.open(url, "_newtab");

}
