var instanceid = 0;
$(document).ready(function () {
    instanceid = GetParameterValues('instanceid') ?? 0;
    LoadViewMyRequest(instanceid);

    $("#intiateform").on("click", ".SaveBtn", function () {
        SaveData();
    });

    // Initially disable
    $("#ddlBillingFreqRecurring").prop("disabled", true);

    $("#ddlBillingType").on("change", function () {
        if ($(this).val() == "1") { //Recurring
            $("#ddlBillingFreqRecurring").prop("disabled", false);
        } else {
            $("#ddlBillingFreqRecurring").prop("disabled", true).val("0");
        }
    });


});
async function LoadViewMyRequest(instanceid) {
    FillDropdowns_OnLoad();

    ////setbydefaultvalues_fortesting();

    ////setTimeout(function () {
    ////    $("#ddlClient").val($("#ddlClient option:eq(1)").val());
    ////    $("#ddlContract").val($("#ddlClient option:eq(1)").val());
    ////    $("#ddlBillingType").val($("#ddlBillingType option:eq(1)").val());
    ////    $("#ddlBillingFreqRecurring").val($("#ddlBillingFreqRecurring option:eq(1)").val());
    ////}, 2000);


    var billingId = $("#hdnBillingId").val();
    if (billingId && billingId != "" && billingId != "0") {
        ////alert('Edit Mode');
        $("#hdnAction").val('Edit');

        GetBillingById(billingId);
    }

}

function ValidateAll() {
    $(".is-invalid").removeClass("is-invalid");

    // Client
    if (!$("#ddlClient").val() || $("#ddlClient").val() == "0") {
        setInvalid("#ddlClient", "Please select Client!");
        return false;
    }

    // Contract
    if (!$("#ddlContract").val() || $("#ddlContract").val() == "0") {
        setInvalid("#ddlContract", "Please select Contract!");
        return false;
    }

    // Billing Type
    if (!$("#ddlBillingType").val() || $("#ddlBillingType").val() == "0") {
        setInvalid("#ddlBillingType", "Please select Billing Type!");
        return false;
    }

    // Recurring Frequency (only if Billing Type = Recurring)
    var billingTypeText = $("#ddlBillingType option:selected").text().trim().toLowerCase();

    if (billingTypeText === "Recurring".toLowerCase()) {

        if (!$("#ddlBillingFreqRecurring").val() ||
            $("#ddlBillingFreqRecurring").val() == "0") {

            setInvalid("#ddlBillingFreqRecurring",
                "Please select Billing Frequency!");
            return false;
        }
    }

    // Next Billing Date
    if (!$("#next_billing_date").val()) {
        setInvalid("#next_billing_date", "Please select Next Billing Date!");
        return false;
    }

    // Billing Method
    if (!$("input[name='bill_method']:checked").length) {
        alert("Please select Billing Method!");
        return false;
    }

    // Delivery Method
    if (!$("input[name='delivery_method']:checked").length) {
        alert("Please select Invoice Delivery Method!");
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
            var rawBillingId = $("#hdnBillingId").val();
            var billingId = (!rawBillingId || isNaN(rawBillingId)) ? 0 : parseInt(rawBillingId);

            var billingInfo = {
                workflow: "COB",
                instanceid: 0,
                billing_id: billingId,

                client_id: $("#ddlClient").val(),
                contract_id: $("#ddlContract").val(),

                billing_type_id: $("#ddlBillingType").val(),
                billing_freq_id: $("#ddlBillingFreqRecurring").val(),
                next_billing_date: $("#next_billing_date").val(),

                billing_method: $("input[name='bill_method']:checked").val(),
                delivery_method: $("input[name='delivery_method']:checked").val(),

                userid: 1
            };

            // API call using JSON instead of form-urlencoded
            new APICALL(GetGlobalURL('Base', 'SaveBillingForm'), 'POST', JSON.stringify(billingInfo), true, false)
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
function GetBillingById(billingId) {
    new APICALL(GetGlobalURL('Base', 'GetBillingById') + '?billingId=' + billingId, 'GET', '', false).FETCH((result, error) => {

        if (result) {
            if (result.data != null && result.data.length > 0) {

                try {
                    const data = result.data[0];
                    console.log(data);

                    // Billing Id
                    $("#hdnBillingId").val(data.billing_id);

                    // Dropdowns (wait until dropdowns loaded)
                    setTimeout(() => {
                        $("#ddlClient").val(String(data.client_id)).trigger('change');
                        $("#ddlContract").val(String(data.contract_id)).trigger('change');
                        $("#ddlBillingType").val(String(data.billing_type_id)).trigger('change');
                        $("#ddlBillingFreqRecurring").val(String(data.billing_freq_id)).trigger('change');
                    }, 100);

                    // Billing Dates
                    if (data.next_billing_date) {
                        $("#next_billing_date").val(
                            data.next_billing_date.split('T')[0]
                        );
                    }

                    // Billing Method (Auto / Manual)
                    $("input[name='bill_method'][value='" + data.billing_method + "']")
                        .prop("checked", true);

                    // Invoice Delivery Method (Email / Portal / Manual)
                    $("input[name='delivery_method'][value='" + data.delivery_method + "']")
                        .prop("checked", true);


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
function GetAllBillingType_DDL() {
    new APICALL(GetGlobalURL('Base', 'GetBillingTypeDDL'), 'GET', '', true).FETCH((result, error) => {
        if (result) {
            if (result.data != null) {
                window.allUsers = result.data;

                var ddl = $('#ddlBillingType');
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
function GetAllBillingFrequency_DDL() {
    new APICALL(GetGlobalURL('Base', 'GetBillingFrequencyDDL'), 'GET', '', true).FETCH((result, error) => {
        if (result) {
            if (result.data != null) {
                window.allUsers = result.data;

                var ddl = $('#ddlBillingFreqRecurring');
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
    GetAllBillingType_DDL();
    GetAllBillingFrequency_DDL();
}
function DoEmptyFields() {
    // Client Info
    $("#ddlClient").val('0');
    $("#ddlContract").val('0');

    // Core Billing Details
    $("#ddlBillingType").val('0');
    $("#ddlBillingFreqRecurring").val('0');
    $("#next_billing_date").val('');

    // Invoicing Preferences
    $("input[name='bill_method']").prop("checked", false);
    $("input[name='delivery_method']").prop("checked", false);

    // Disable recurring frequency by default
    $("#ddlBillingFreqRecurring").prop("disabled", true);

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
    // Client Info
    $("#ddlClient").prop('selectedIndex', 1);
    $("#ddlContract").prop('selectedIndex', 1);

    // Core Billing Details
    $("#ddlBillingType").prop('selectedIndex', 1);
    $("#ddlBillingFreqRecurring").prop('selectedIndex', 1);
    $("#next_billing_date").val("2026-07-01");

    // Invoicing Preferences
    $("#rdoAuto").prop("checked", true);
    $("#rdoDeliveryEmail").prop("checked", true);

    console.log("Default testing values loaded.");

}
