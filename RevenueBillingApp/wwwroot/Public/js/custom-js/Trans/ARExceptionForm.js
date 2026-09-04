var instanceid = 0;
$(document).ready(function () {
    instanceid = GetParameterValues('instanceid') ?? 0;
    LoadViewMyRequest(instanceid);

    $("#intiateform").on("click", ".SaveBtn", function () {
        SaveData();
    });

});
async function LoadViewMyRequest(instanceid) {
    FillDropdowns_OnLoad();

    ////setbydefaultvalues_fortesting();

    ////setTimeout(function () {
    ////    $("#ddlClient").val($("#ddlClient option:eq(1)").val());
    ////    $("#ddlInvoiceNo").val($("#ddlInvoiceNo option:eq(1)").val());
    ////}, 2000);

    var exceptionId = $("#hdnExceptionId").val();
    if (exceptionId && exceptionId != "" && exceptionId != "0") {
        ////alert('Edit Mode');
        $("#hdnAction").val('Edit');

        GetExceptionById(exceptionId);
    }

}

function ValidateAll() {
    $(".is-invalid").removeClass("is-invalid");

    // Client
    if (!$("#ddlClient").val() || $("#ddlClient").val() == "0") {
        setInvalid("#ddlClient", "Please select Client!");
        return false;
    }

    // Invoice No
    if (!$("#ddlInvoiceNo").val() || $("#ddlInvoiceNo").val() == "0") {
        setInvalid("#ddlInvoiceNo", "Please select Invoice No!");
        return false;
    }

    // Amount
    if ($("#amount").val() == "" || parseFloat($("#amount").val()) <= 0) {
        setInvalid("#amount", "Please enter valid Amount!");
        return false;
    }

    // Risk Category
    if (!$("input[name='rdoRisk']:checked").length) {
        ShowValidationMessage("Please select Risk Category!");
        return false;
    }

    // Escalation Level
    if (!$("input[name='rdoEscalation']:checked").length) {
        ShowValidationMessage("Please select Escalation Level!");
        return false;
    }

    return true;
}
function ShowValidationMessage(message) {
    Swal.fire({
        icon: 'warning',
        text: message,
        confirmButtonColor: "#61affe"
    });
}

/* Save ClientForm */
function SaveData() {
    var res = ValidateAll();
    if (res == false) {
        return false;
    }

    Swal.fire({
        title: 'Do you want to save the changes?',
        showDenyButton: true,
        showCancelButton: true,
        confirmButtonText: 'Ok',
        denyButtonText: 'Cancel',
    }).then((result) => {
        if (result) {
            var rawExceptionId = $("#hdnExceptionId").val();
            var exceptionId = (!rawExceptionId || isNaN(rawExceptionId)) ? 0 : parseInt(rawExceptionId);

            var exceptionInfo = {
                workflow: "COB",
                instanceid: 0,
                exception_id: exceptionId,
                client_id: $("#ddlClient").val(),
                invoice_id: $("#ddlInvoiceNo").val(),
                amount: $("#amount").val(),
                days_overdue: $("#days_overdue").val(),
                risk_category: $("input[name='rdoRisk']:checked").val(),
                escalation_level: $("input[name='rdoEscalation']:checked").val(),
                notes: $("#notes").val(),
                userid: 1
            };

            // API call using JSON instead of form-urlencoded
            new APICALL(GetGlobalURL('Base', 'SaveARExceptionForm'), 'POST', JSON.stringify(exceptionInfo), true, false)
                .FETCH((result, error) => {
                    //console.log(result);

                    if (result && result.status === 'success') {
                        var action = $("#hdnAction").val();
                        var message = '';

                        if (action === 'Add') {
                            message = 'Saved Successfully!';
                        } else {
                            message = 'Update Successfully!';
                        }

                        Swal.fire({
                            icon: 'success',
                            title: 'Success...',
                            text: message
                        });

                        DoEmptyFields();

                        return;
                    }

                    // ERROR block
                    if (error && error.status && error.status !== 200) {
                        Swal.fire({
                            icon: 'error',
                            title: 'Error...',
                            text: error.data?.responseText || 'Something went wrong!'
                        });
                        return;
                    }
                });



        }
    });
}

/* Fill Contract Form in EDIT Mode */
function GetExceptionById(exceptionId) {
    new APICALL(GetGlobalURL('Base', 'GetARExceptionById') + '?exceptionId=' + exceptionId, 'GET', '', false).FETCH((result, error) => {

        if (result) {
            if (result.data != null && result.data.length > 0) {

                try {
                    const data = result.data[0];
                    console.log(data);

                    // Exception Id
                    $("#hdnExceptionId").val(data.exception_id);

                    // Dropdowns (wait until dropdowns loaded)
                    setTimeout(() => {
                        $("#ddlClient").val(String(data.client_id)).trigger('change');
                        $("#ddlInvoiceNo").val(String(data.invoice_id)).trigger('change');
                    }, 100);    

                    $("#amount").val(data.amount ?? '0');
                    $("#days_overdue").val(data.days_overdue ?? '0');

                    $("input[name='rdoRisk'][value='" + data.risk_category + "']").prop("checked", true);
                    $("input[name='rdoEscalation'][value='" + data.escalation_level + "']").prop("checked", true);

                    $("#notes").val(data.notes ?? '');

                    // Edit Mode
                    $("#hdnAction").val("Edit");

                }
                catch (ex) {
                    console.error(ex);
                };

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

/* All DDL populate */
function GetClientInfoDDL() {
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
function GetInvoiceDDL() {
    new APICALL(GetGlobalURL('Base', 'GetInvoiceDDL'), 'GET', '', true).FETCH((result, error) => {
        if (result) {
            if (result.data != null) {
                window.allUsers = result.data;

                var ddl = $('#ddlInvoiceNo');
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
function FillDropdowns_OnLoad() {
    GetClientInfoDDL();
    GetInvoiceDDL();
}
function DoEmptyFields() {
    $("#hdnAction").val("Add");
    $("#hdnExceptionId").val("0");

    // Dropdowns
    $("#ddlClient").val("0").trigger("change");
    $("#ddlInvoiceNo").empty().append('<option value="0">Select Invoice</option>');

    // Inputs
    $("#amount").val("");
    $("#days_overdue").val("");
    $("#notes").val("");

    // Default Risk Category = Medium
    $("#riskMedium").prop("checked", true);

    // Default Escalation = Finance
    $("#rdoFinance").prop("checked", true);

    $(".is-invalid").removeClass("is-invalid");
}
function setInvalid(selector, message) {
    $(selector).addClass("is-invalid");

    Swal.fire({
        icon: 'warning',
        text: message,
        confirmButtonColor: "#61affe"
    });
}

/* for development testing */
function setbydefaultvalues_fortesting() {
    // Client
    $("#ddlClient").prop("selectedIndex", 1).trigger("change");

    // Invoice
    setTimeout(function () {
        $("#ddlInvoiceNo").prop("selectedIndex", 1);
    }, 500);

    // Amount
    $("#amount").val("250000");

    // Days Overdue
    $("#days_overdue").val("45");

    // Risk Category
    $("#riskHigh").prop("checked", true);

    // Escalation Level
    $("#rdoCEO").prop("checked", true);

    // Notes
    $("#notes").val("Customer payment is overdue for more than 45 days. Escalated for management review.");

    console.log("Default testing values loaded.");
}
