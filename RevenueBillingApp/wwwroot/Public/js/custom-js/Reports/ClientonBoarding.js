var users = {};

$(document).ready(function () {
    //$("#btnPDF").click(function () {
    //    window.print();
    //});

    $('#btnGetReport').on('click', function () {
        //var res = ValidateAll();
        //if (res == false) {
        //    return false;
        //}

        GetReport();
    });

    $("#btnPDF").click(function () {
        $(".no-print").hide();

        html2pdf()
            .set({
                margin: 0.3,
                filename: 'Client_OnBoarding_Report.pdf',
                image: { type: 'jpeg', quality: 1 },
                html2canvas: { scale: 2 },
                jsPDF: {
                    unit: 'in',
                    format: 'a4',
                    orientation: 'portrait'
                }
            })
            .from(document.querySelector(".report-container"))
            .save()
            .then(function () {
                $(".no-print").show();
            });

    });


});

function DoEmptyFields() {
    //$('#UserID').val('');
    //$('#EditID').val('');

    ////$("#pkID").val(null);
    ////$("#ddlFC").val("-1");
    ////$("#txtNature").val(null);
    ////$("#ddlAccount").val("-1");
    ////$("#txtGLCodeA").val(null);
    ////$("#ddlCategory").val("-1");
    ////$("#txtGLCodeB").val(null);
    ////$("#txtNameL4").val(null);
    ////$("#txtGLCodeC").val(null);

}
function GetReport() {
    var queryParams = [];

    //var companyId = $('#chkCompany').is(':checked')
    //    ? $('#ddlCompany').val()?.join(",")
    //    : null;

    //var category = $('#chkCategory').is(':checked')
    //    ? $('#ddlCategory').val()?.join(",")
    //    : null;

    //var productId = $('#chkProduct').is(':checked')
    //    ? $('#ddlProduct').val()?.join(",")
    //    : null;

    //// Construct query string accordingly
    //if (companyId) queryParams.push("CompanyId=" + encodeURIComponent(companyId));
    //if (category) queryParams.push("Category=" + encodeURIComponent(category));
    //if (productId) queryParams.push("ProductId=" + encodeURIComponent(productId));

    //if ($('#chkExpiryRange').is(':checked')) {
    //    var fromExpiryDate = $('#txtFromExpiryDate').val();
    //    var toExpiryDate = $('#txtToExpiryDate').val();

    //    if (fromExpiryDate) queryParams.push("FromExpiryDate=" + fromExpiryDate);
    //    if (toExpiryDate) queryParams.push("ToExpiryDate=" + toExpiryDate);
    //}

    var blnExcel = $('#chkShowExcel').is(':checked') ? "true" : "false";
    queryParams.push("ShowExcel=" + blnExcel);

    var queryString = "?" + queryParams.join("&");
    var url = "/Reports/rpt_ClientonBoarding" + queryString;

    // Open the URL in a new tab
    window.open(url, "_newtab");

}
function setDateFields(currentDate) {
    //document.getElementById('txtFromExpiryDate').value = currentDate
    //document.getElementById('txtToExpiryDate').value = currentDate
}
function ValidateAll() {
    var isValid = true;

    // Validate Item if checkbox is checked
    if ($("#chkCompany").is(":checked")) {
        var company = $("#ddlCompany").val();
        if (!company || company === "-1") {
            Swal.fire({
                icon: 'warning',
                text: 'Please select Company Name!',
                confirmButtonColor: "#61affe"
            });
            return false;
        }
    }


    if ($("#chkCategory").is(":checked")) {
        var category = $("#ddlCategory").val();
        if (!category || category === "-1") {
            Swal.fire({
                icon: 'warning',
                text: 'Please select Category!',
                confirmButtonColor: "#61affe"
            });
            return false;
        }
    }

    if ($("#chkProduct").is(":checked")) {
        var product = $("#ddlProduct").val();
        if (!product || product === "-1") {
            Swal.fire({
                icon: 'warning',
                text: 'Please select Product!',
                confirmButtonColor: "#61affe"
            });
            return false;
        }
    }


    if ($("#chkExpiryRange").is(":checked")) {
        var fromexpiry = $("#txtFromExpiryDate").val();
        var toexpiry = $("#txtToExpiryDate").val();
        if (!fromexpiry || !fromexpiry.trim()) {
            Swal.fire({
                icon: 'warning',
                text: 'Please select From Expiry Date!',
                confirmButtonColor: "#61affe"
            });
            return false;
        }
        if (!toexpiry || !toexpiry.trim()) {
            Swal.fire({
                icon: 'warning',
                text: 'Please select To Expiry Date!',
                confirmButtonColor: "#61affe"
            });
            return false;
        }
    }



    return isValid;
}
