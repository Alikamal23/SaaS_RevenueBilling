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
    ////    $("#ddlbasicContractType").val($("#ddlbasicContractType option:eq(1)").val());
    ////    $("#ddlCurrency").val($("#ddlCurrency option:eq(1)").val());
    ////    $("#ddlBillingFreq").val($("#ddlBillingFreq option:eq(1)").val());
    ////    $("#ddlBillingCycle").val($("#ddlBillingCycle option:eq(1)").val());
    ////    $("#ddlPaymentTerms").val($("#ddlPaymentTerms option:eq(1)").val());
    ////}, 2000);

    //GetCOBDetailByInstanceId(instanceid);

    var contractId = $("#hdnContractId").val();
    if (contractId && contractId != "" && contractId != "0") {
        ////alert('Edit Mode');
        $("#hdnAction").val('Edit');

        GetContractById(contractId);
    }

}

function ValidateAll() {
    // clear previous errors
    $(".is-invalid").removeClass("is-invalid");

    // Client
    if (!$("#ddlClient").val() || $("#ddlClient").val() == "0") {
        setInvalid("#ddlClient", "Please select Client!");
        return false;
    }

    // Contract Type
    if (!$("#ddlbasicContractType").val() || $("#ddlbasicContractType").val() == "0") {
        setInvalid("#ddlbasicContractType", "Please select Contract Type!");
        return false;
    }

    // Contract Value
    if (!$("#contract_value").val()?.trim()) {
        setInvalid("#contract_value", "Please enter Contract Value!");
        return false;
    }

    // Currency
    if (!$("#ddlCurrency").val() || $("#ddlCurrency").val() == "0") {
        setInvalid("#ddlCurrency", "Please select Currency!");
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
            var rawContractId = $("#hdnContractId").val();
            var contractId = (!rawContractId || isNaN(rawContractId)) ? 0 : parseInt(rawContractId);

            var contractInfo = {
                workflow: "COB",
                instanceid: 0,
                contract_id: contractId,

                client_id: $("#ddlClient").val(),
                basic_contract_type: $("#ddlbasicContractType").val(),
                contract_value: $("#contract_value").val(),
                currency_id: $("#ddlCurrency").val(),
                discount_percent: $("#discount").val(),
                tax_percent: $("#tax").val(),

                bill_freq_id: $("#ddlBillingFreq").val(),
                bill_start_date: $("#bill_start_date").val(),
                bill_end_date: $("#bill_end_date").val(),
                billing_cycle: $("#ddlBillingCycle").val(),
                renewal_terms_months: $("#renewal_terms_months").val(),
                auto_renew: $("#auto_renew").is(":checked"),

                project_name: $("#projectname").val(),
                project_value: $("#projectvalue").val(),
                no_of_milestones: $("#milestones").val(),

                payment_term_id: $("#ddlPaymentTerms").val(),
                penalty_terms: $("#remarks").val(),
                support_hours: $("#supphours").val(),

                userid: 1
            };

            // API call using JSON instead of form-urlencoded
            new APICALL(GetGlobalURL('Base', 'SaveContractForm'), 'POST', JSON.stringify(contractInfo), true, false)
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
function GetContractById(contractId) {
    new APICALL(GetGlobalURL('Base', 'GetContractById') + '?contractId=' + contractId, 'GET', '', false).FETCH((result, error) => {

        if (result) {
            if (result.data != null && result.data.length > 0) {

                try {
                    const data = result.data[0];
                    console.log(data);

                    // Contract Id
                    $("#hdnContractId").val(data.contract_id);

                    // Dropdowns (wait until dropdowns loaded)
                    setTimeout(() => {
                        $("#ddlClient").val(String(data.client_id)).trigger('change');
                        $("#ddlbasicContractType").val(String(data.basic_contract_type)).trigger('change');
                        $("#ddlCurrency").val(String(data.currency_id)).trigger('change');
                        $("#ddlBillingFreq").val(String(data.bill_freq_id)).trigger('change');
                        $("#ddlPaymentTerms").val(String(data.payment_term_id)).trigger('change');

                        // Billing Cycle dropdown (1-31)
                        $("#ddlBillingCycle").val(String(data.billing_cycle));

                    }, 100);

                    // Contract Basics
                    $("#contract_value").val(data.contract_value ?? '');
                    $("#discount").val(data.discount_percent ?? '');
                    $("#tax").val(data.tax_percent ?? '');

                    // Billing Dates
                    if (data.bill_start_date) {
                        $("#bill_start_date").val(
                            data.bill_start_date.split('T')[0]
                        );
                    }

                    if (data.bill_end_date) {
                        $("#bill_end_date").val(
                            data.bill_end_date.split('T')[0]
                        );
                    }

                    // Renewal
                    // month input expects yyyy-MM
                    if (data.renewal_terms_months) {

                        const currentYear = new Date().getFullYear();

                        $("#renewal_terms_months").val(
                            currentYear + "-" +
                            String(data.renewal_terms_months).padStart(2, '0')
                        );
                    }

                    $("#auto_renew").prop(
                        "checked",
                        data.auto_renew == 1 ||
                        data.auto_renew === true
                    );

                    // Project Section
                    $("#projectname").val(data.project_name ?? '');
                    $("#projectvalue").val(data.project_value ?? '');
                    $("#milestones").val(data.no_of_milestones ?? '');

                    // Payment / Support
                    $("#remarks").val(data.penalty_terms ?? '');
                    $("#supphours").val(data.support_hours ?? '');

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
function GetAllContractTypeBasic_DDL() {
    new APICALL(GetGlobalURL('Base', 'GetContractTypeBasicDDL'), 'GET', '', true).FETCH((result, error) => {
        if (result) {
            if (result.data != null) {
                window.allUsers = result.data;

                var ddl = $('#ddlbasicContractType');
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

                var ddl = $('#ddlBillingFreq');
                populateDropdowns(ddl, result.data, null, 0);

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
function GetAllPaymentTerms_DDL() {
    new APICALL(GetGlobalURL('Base', 'GetPaymentTermsDDL'), 'GET', '', true).FETCH((result, error) => {
        if (result) {
            if (result.data != null) {
                window.allUsers = result.data;

                var ddl = $('#ddlPaymentTerms');
                populateDropdowns(ddl, result.data, null, 1);
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
function GetAllCurrency_DDL() {
    new APICALL(GetGlobalURL('Base', 'GetCurrencyDDL'), 'GET', '', true).FETCH((result, error) => {
        if (result) {
            if (result.data != null) {
                window.allUsers = result.data;

                var ddl = $('#ddlCurrency');
                populateDropdowns(ddl, result.data, null, 1);
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
    GetAllContractTypeBasic_DDL();
    GetAllBillingFrequency_DDL();
    GetAllPaymentTerms_DDL();
    GetAllCurrency_DDL();
}
function DoEmptyFields() {
    // Client Info
    $("#ddlClient").val('0');

    // Contract Basics
    $("#ddlbasicContractType").val('0');
    $("#contract_value").val('');
    $("#ddlCurrency").val('0');
    $("#discount").val('');
    $("#tax").val('');

    // SaaS Specific
    $("#ddlBillingFreq").val('0');
    $("#bill_start_date").val('');
    $("#bill_end_date").val('');
    $("#ddlBillingCycle").val('0');
    $("#renewal_terms_months").val('');
    $("#auto_renew").prop("checked", false);

    // Project Specific
    $("#projectname").val('');
    $("#projectvalue").val('');
    $("#milestones").val('');
    $("#ddlPaymentTerms").val('0');
    $("#remarks").val('');
    $("#supphours").val('');
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

    // Contract Basics
    $("#ddlbasicContractType").prop('selectedIndex', 1);
    $("#contract_value").val("50000");
    $("#ddlCurrency").prop('selectedIndex', 1);
    $("#discount").val("10");
    $("#tax").val("15");

    // SaaS Specific
    $("#ddlBillingFreq").prop('selectedIndex', 1);
    $("#bill_start_date").val("2025-08-01");
    $("#bill_end_date").val("2026-07-31");
    $("#ddlBillingCycle").val("15");
    $("#renewal_terms_months").val("2026-08");
    $("#auto_renew").prop("checked", true);

    // Project Specific
    $("#projectname").val("CRM Implementation Project");
    $("#projectvalue").val("75000");
    $("#milestones").val("5");
    $("#ddlPaymentTerms").prop('selectedIndex', 1);
    $("#remarks").val("5% penalty on delayed payments.");
    $("#supphours").val("Mon-Fri 9AM-6PM");

    console.log("Default testing values loaded.");
}
