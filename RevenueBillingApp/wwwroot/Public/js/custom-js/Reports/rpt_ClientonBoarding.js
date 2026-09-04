var users = {};

$(document).ready(function () {
    //$("#btnPDF").click(function () {
    //    window.print();
    //});

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
