var instanceid = 0;
$(document).ready(function () {
    instanceid = GetParameterValues('instanceid') ?? 0;
    LoadViewMyRequest(instanceid);

    $("#intiateform").on("click", ".SaveBtn", function () {
        SaveData();
    });

    $("#amount, #tax").on("input", function () {
        CalculateTotal();
    });

});
async function LoadViewMyRequest(instanceid) {
    FillDropdowns_OnLoad();

    ////setbydefaultvalues_fortesting();

    ////setTimeout(function () {
    ////    $("#ddlClient").val($("#ddlClient option:eq(1)").val());
    ////    $("#ddlContract").val($("#ddlClient option:eq(1)").val());
    ////    $("#ddlBillingStatus").val($("#ddlBillingStatus option:eq(1)").val());
    ////}, 2000);


    var rbillingId = $("#hdnRBillingId").val();
    if (rbillingId && rbillingId != "" && rbillingId != "0") {
        ////alert('Edit Mode');
        $("#hdnAction").val('Edit');

        GetRecurringBillingById(rbillingId);
    }

}
function ValidateAll() {

    $(".is-invalid").removeClass("is-invalid");

    // Client *
    if (!$("#ddlClient").val() || $("#ddlClient").val() == "0") {
        setInvalid("#ddlClient", "Please select Client!");
        return false;
    }

    // Contract *
    if (!$("#ddlContract").val() || $("#ddlContract").val() == "0") {
        setInvalid("#ddlContract", "Please select Contract!");
        return false;
    }

    // Amount *
    if (!$("#amount").val() || parseFloat($("#amount").val()) <= 0) {
        setInvalid("#amount", "Please enter valid Amount!");
        return false;
    }

    // Tax *
    if (!$("#tax").val() || parseFloat($("#tax").val()) < 0) {
        setInvalid("#tax", "Please enter Tax!");
        return false;
    }

    // Due Date *
    if (!$("#due_date").val()) {
        setInvalid("#due_date", "Please select Due Date!");
        return false;
    }

    // Status *
    if (!$("#ddlBillingStatus").val() || $("#ddlBillingStatus").val() == "0") {
        setInvalid("#ddlBillingStatus", "Please select Status!");
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
            var rawRBillingId = $("#hdnRBillingId").val();
            var rbillingId = (!rawRBillingId || isNaN(rawRBillingId)) ? 0 : parseInt(rawRBillingId);

            var recurringbillingInfo = {
                workflow: "COB",
                instanceid: 0,
                rbilling_id: rbillingId,

                client_id: $("#ddlClient").val(),
                contract_id: $("#ddlContract").val(),

                // Billing Period
                bill_start_date: $("#bill_start_date").val(),
                bill_end_date: $("#bill_end_date").val(),

                // Financial Summary
                amount: $("#amount").val(),
                tax: $("#tax").val(),
                total_amount: $("#total").val(),

                due_date: $("#due_date").val(),
                bill_status_id: $("#ddlBillingStatus").val(),

                userid: 1
            };

            // API call using JSON instead of form-urlencoded
            new APICALL(GetGlobalURL('Base', 'SaveRecurringBillingForm'), 'POST', JSON.stringify(recurringbillingInfo), true, false)
                .FETCH((result, error) => {
                    //console.log(result);

                    if (result && result.status === 'success') {
                        DoEmptyFields();

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
function GetRecurringBillingById(rbillingId) {
    new APICALL(GetGlobalURL('Base', 'GetRecurringBillingById') + '?rbillingId=' + rbillingId, 'GET', '', false).FETCH((result, error) => {

        if (result) {
            if (result.data != null && result.data.length > 0) {

                try {
                    const data = result.data[0];
                    console.log(data);

                    // Billing Id
                    $("#hdnRBillingId").val(data.rbilling_id);

                    // Dropdowns (wait until dropdowns loaded)
                    setTimeout(() => {
                        $("#ddlClient").val(String(data.client_id)).trigger('change');
                        $("#ddlContract").val(String(data.contract_id)).trigger('change');
                        $("#ddlBillingType").val(String(data.billing_type_id)).trigger('change');
                        $("#ddlBillingFreqRecurring").val(String(data.billing_freq_id)).trigger('change');
                    }, 100);

                    // Billing Period
                    if (data.bill_start_date) {
                        $("#bill_start_date").val(data.bill_start_date.split('T')[0]);
                    }

                    if (data.bill_end_date) {
                        $("#bill_end_date").val(data.bill_end_date.split('T')[0]);
                    }

                    // Financial Summary
                    $("#amount").val(data.amount);
                    $("#tax").val(data.tax);
                    $("#total").val(data.total_amount);

                    if (data.due_date) {
                        $("#due_date").val(data.due_date.split('T')[0]);
                    }

                    // Status dropdown
                    setTimeout(() => {
                        $("#ddlBillingStatus")
                            .val(String(data.bill_status_id))
                            .trigger('change');
                    }, 120);


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
function GetContractInfoDDL() {
    new APICALL(GetGlobalURL('Base', 'GetContractInfoDDL'), 'GET', '', true).FETCH((result, error) => {
        if (result) {
            if (result.data != null) {
                window.allUsers = result.data;

                var ddl = $('#ddlContract');
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
function GetAllBillingStatus_DDL() {
    new APICALL(GetGlobalURL('Base', 'GetBillingStatusDDL'), 'GET', '', true).FETCH((result, error) => {
        if (result) {
            if (result.data != null) {
                window.allUsers = result.data;

                var ddl = $('#ddlBillingStatus');
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
    GetContractInfoDDL();
    GetAllBillingStatus_DDL();
}
function DoEmptyFields() {
    // Client Info
    $("#ddlClient").val('0');
    $("#ddlContract").val('0');

    // Billing Period
    $("#bill_start_date").val('');
    $("#bill_end_date").val('');

    // Financial Summary
    $("#amount").val('');
    $("#tax").val('');
    $("#total").val('');
    $("#due_date").val('');
    $("#ddlBillingStatus").val('0');

}
function setInvalid(selector, message) {
    $(selector).addClass("is-invalid");

    Swal.fire({
        icon: 'warning',
        text: message,
        confirmButtonColor: "#61affe"
    });
}
function CalculateTotal() {

    var amount = parseFloat($("#amount").val()) || 0;
    var tax = parseFloat($("#tax").val()) || 0;

    var total = amount + ((amount * tax) / 100);

    $("#total").val(total.toFixed(2));
}

/* for development testing */
function setbydefaultvalues_fortesting() {
    // Client Info
    $("#ddlClient").prop('selectedIndex', 1);
    $("#ddlContract").prop('selectedIndex', 1);

    // Billing Period
    $("#bill_start_date").val("2026-07-01");
    $("#bill_end_date").val("2026-07-31");

    // Financial Summary
    $("#amount").val("1000");
    $("#tax").val("15");

    // Auto calculate total
    CalculateTotal();

    $("#due_date").val("2026-08-05");
    $("#ddlBillingStatus").prop('selectedIndex', 1);

    console.log("Default testing values loaded.");

}
