var instanceid = 0;

document.querySelectorAll('.file-input').forEach(input => {
    input.addEventListener('change', function () {
        const uploader = this.closest('.uploader');
        const previewContainer = uploader.querySelector('.preview-container');

        Array.from(this.files).forEach(file => {
            const col = document.createElement('div');
            col.className = 'col-6 mt-3';
            const preview = document.createElement('div');
            preview.className = 'preview-item';

            // Remove Button
            const removeBtn = document.createElement('button');
            removeBtn.className = 'remove-btn';
            removeBtn.innerHTML = '&times;';
            removeBtn.onclick = () => {
                col.remove();
            };

            preview.appendChild(removeBtn);

            // Image Preview
            if (file.type.startsWith('image/')) {
                const img = document.createElement('img');
                img.src = URL.createObjectURL(file);
                preview.appendChild(img);
            } else {
                const icon = document.createElement('div');
                icon.style.height = '120px';
                icon.style.display = 'flex';
                icon.style.alignItems = 'center';
                icon.style.justifyContent = 'center';
                icon.style.fontSize = '50px';
                icon.innerHTML = '📄';
                preview.appendChild(icon);
            }

            // File Name
            const name = document.createElement('div');
            name.className = 'mt-2 small';
            name.innerText = file.name;
            preview.appendChild(name);
            col.appendChild(preview);
            previewContainer.appendChild(col);
        });
    });
});

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
    ////    $("#ddlIndustry").val($("#ddlIndustry option:eq(1)").val());
    ////    $("#ddlCompanySize").val($("#ddlCompanySize option:eq(1)").val());
    ////    $("#ddlCountry").val($("#ddlCountry option:eq(1)").val());
    ////    $("#ddlContractType").val($("#ddlContractType option:eq(1)").val());
    ////    $("#ddlaccOwner").val($("#ddlaccOwner option:eq(1)").val());
    ////    $("#ddlsupOwner").val($("#ddlsupOwner option:eq(1)").val());
    ////}, 2000);

    //GetCOBDetailByInstanceId(instanceid);

    var clientId = $("#hdnClientId").val();
    if (clientId && clientId != "" && clientId != "0") {
        ////alert('Edit Mode');
        $("#hdnAction").val('Edit');

        GetClientById(clientId);
    }

    var action = $("#hdnAction").val();
    if (action === 'Add') {
        $("#docSection").show();
    } else {
        $("#docSection").hide();
    }


}

function ValidateAll_Old() {
    if ($("#client_name").val() == null || $("#client_name").val().trim() == "") {
        ShowValidationMessage("Please enter Client Name!");
        ////$("#client_name").focus();
        return false;
    }

    if ($("#website").val() == null || $("#website").val().trim() == "") {
        ShowValidationMessage("Please enter Website!");
        return false;
    }

    if ($("#billing_address").val() == null || $("#billing_address").val().trim() == "") {
        ShowValidationMessage("Please enter Billing Address!");
        return false;
    }

    if ($("#tax_registration_no").val() == null || $("#tax_registration_no").val().trim() == "") {
        ShowValidationMessage("Please enter Tax Registration No!");
        return false;
    }

    if ($("#primary_contact_name").val() == null || $("#primary_contact_name").val().trim() == "") {
        ShowValidationMessage("Please enter Primary Contact Name!");
        return false;
    }

    if ($("#primary_contact_email").val() == null || $("#primary_contact_email").val().trim() == "") {
        ShowValidationMessage("Please enter Primary Contact Email!");
        return false;
    }

    // Email Format Validation
    var email = $("#primary_contact_email").val().trim();
    var emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

    if (!emailRegex.test(email)) {
        ShowValidationMessage("Please enter a valid Email Address!");
        return false;
    }

    if ($("#primary_contact_phone").val() == null || $("#primary_contact_phone").val().trim() == "") {
        ShowValidationMessage("Please enter Primary Contact Phone!");
        return false;
    }

    if ($("#ddlContractType").val() == null || $("#ddlContractType").val() == "0") {
        ShowValidationMessage("Please select Contract Type!");
        return false;
    }

    //// High Value Client Mandatory
    //if (!$("#highvalue").is(":checked")) {
    //    ShowValidationMessage("Please mark High Value Client!");
    //    return false;
    //}

    // Priority Level Mandatory
    if (!$("input[name='priority_level']:checked").length) {
        ShowValidationMessage("Please select Priority Level!");
        return false;
    }

    return true;
}
function ValidateAll() {
    // clear previous errors first
    $(".is-invalid").removeClass("is-invalid");

    if (!$("#client_name").val()?.trim()) {
        setInvalid("#client_name", "Please enter Client Name!");
        //$("#client_name").focus();
        return false;
    }

    if (!$("#website").val()?.trim()) {
        setInvalid("#website", "Please enter Website!");
        //$("#website").focus();
        return false;
    }

    if (!$("#billing_address").val()?.trim()) {
        setInvalid("#billing_address", "Please enter Billing Address!");
        //$("#billing_address").focus();
        return false;
    }

    if (!$("#tax_registration_no").val()?.trim()) {
        setInvalid("#tax_registration_no", "Please enter Tax Registration No!");
        //$("#tax_registration_no").focus();
        return false;
    }

    if (!$("#primary_contact_name").val()?.trim()) {
        setInvalid("#primary_contact_name", "Please enter Primary Contact Name!");
        //$("#primary_contact_name").focus();
        return false;
    }

    if (!$("#primary_contact_email").val()?.trim()) {
        setInvalid("#primary_contact_email", "Please enter Email!");
        //$("#primary_contact_email").focus();
        return false;
    }

    if (!$("#primary_contact_phone").val()?.trim()) {
        setInvalid("#primary_contact_phone", "Please enter Phone!");
        //$("#primary_contact_phone").focus();
        return false;
    }

    if (!$("#ddlContractType").val()) {
        setInvalid("#ddlContractType", "Please select Contract Type!");
        //$("#ddlContractType").focus();
        return false;
    }

    //if (!$("#highvalue").is(":checked")) {
    //    setInvalid("#highvalue", "Please mark High Value Client!");
    //    return false;
    //}

    if (!$("input[name='priority_level']:checked").length) {
        setInvalid("input[name='priority_level']", "Please select Priority Level!");
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
            var formData = new FormData();
            formData.append("workflow", "COB");
            formData.append("instanceid", 0);

            var rawClientId = $("#hdnClientId").val();
            ////alert(rawClientId);
            var clientId = (!rawClientId || isNaN(rawClientId)) ? 0 : parseInt(rawClientId);
            ////alert(clientId);

            formData.append("client_id", clientId);
            formData.append("client_name", $("#client_name").val());
            formData.append("industry_id", $("#ddlIndustry").val());
            formData.append("company_size_id", $("#ddlCompanySize").val());
            formData.append("website", $("#website").val());
            formData.append("tax_registration_no", $("#tax_registration_no").val());
            formData.append("country_id", $("#ddlCountry").val());
            formData.append("billing_address", $("#billing_address").val());

            formData.append("primary_contact_name", $("#primary_contact_name").val());
            formData.append("primary_contact_email", $("#primary_contact_email").val());
            formData.append("primary_contact_phone", $("#primary_contact_phone").val());

            formData.append("contract_type_id", $("#ddlContractType").val());
            formData.append("account_owner_id", $("#ddlaccOwner").val());
            formData.append("support_owner_id", $("#ddlsupOwner").val());

            formData.append("onboarding_start_date", $("#onboarding_start_date").val());
            formData.append("onboarding_completion_date", $("#onboarding_completion_date").val());
            formData.append("billing_start_date", $("#billing_start_date").val());
            formData.append("contract_start_date", $("#contract_start_date").val());
            formData.append("contract_end_date", $("#contract_end_date").val());

            formData.append("high_value_client", $("#highvalue").is(":checked"));
            formData.append("priority_level", $("input[name='priority_level']:checked").val());

            formData.append("userid", 1);


            if ($("#doc")[0].files.length > 0)
                formData.append("contract_upload", $("#doc")[0].files[0]);

            if ($("#nda")[0].files.length > 0)
                formData.append("nda_upload", $("#nda")[0].files[0]);

            if ($("#proposal")[0].files.length > 0)
                formData.append("proposal_upload", $("#proposal")[0].files[0]);


            new APICALL(GetGlobalURL('Base', 'SaveClientonBoarding'), 'POST', formData, true, true)
                .FETCH((result, error) => {

                    const xhr = error?.data; // 👈 actual jqXHR is here

                    let res = {};

                    try {
                        res = xhr?.responseJSON || JSON.parse(xhr?.responseText || '{}');
                    } catch (e) {
                        res = {};
                    }

                    // now check message safely
                    if (res.message === "DuplicateNameCount") {
                        Swal.fire({
                            icon: 'warning',
                            title: 'Duplicate Client',
                            text: 'Client name already exists!'
                        });
                        return;
                    }

                    Swal.fire({
                        icon: 'error',
                        title: 'Error',
                        text: res.message || 'Something went wrong'
                    });


                    if (result.data.status === 'error') {
                        Swal.fire({
                            icon: 'error',
                            title: 'Error...',
                            text: result.data.message
                        });
                        return;
                    }

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

/* Fill Client Form in EDIT Mode */
function GetClientById(clientId) {
    new APICALL(GetGlobalURL('Base', 'GetClientById') + '?clientId=' + clientId, 'GET', '', false).FETCH((result, error) => {

        if (result) {
            if (result.data != null && result.data.length > 0) {

                try {
                    const data = result.data[0];

                    console.log(data);

                    // Client Information
                    $('#client_name').val(data.client_name ?? '');
                    $('#website').val(data.website ?? '');
                    $('#billing_address').val(data.billing_address ?? '');
                    $('#tax_registration_no').val(data.tax_registration_no ?? '');

                    // Contacts
                    $('#primary_contact_name').val(data.primary_contact_name ?? '');
                    $('#primary_contact_email').val(data.primary_contact_email ?? '');
                    $('#primary_contact_phone').val(data.primary_contact_phone ?? '');

                    // Business Information
                    $('#high_value_client').prop('checked', data.high_value_client === true);
                    $('#priority_level').val(data.priority_level ?? '');

                    //// Dropdowns
                    ////$('#ddlIndustry').val(data.industry_id).trigger('change');
                    ////$('#ddlCompanySize').val(data.company_size_id).trigger('change');
                    ////$('#ddlCountry').val(data.country_id).trigger('change');
                    ////$('#ddlContractType').val(data.contract_type_id).trigger('change');
                    ////$('#ddlaccOwner').val(data.account_owner_id).trigger('change');
                    ////$('#ddlsupOwner').val(data.support_owner_id).trigger('change');

                    // Dropdowns wait for dropdown render
                    setTimeout(() => {
                        $('#ddlIndustry').val(String(data.industry_id)).trigger('change');
                        $('#ddlCompanySize').val(String(data.company_size_id)).trigger('change');
                        $('#ddlCountry').val(String(data.country_id)).trigger('change');
                        $('#ddlContractType').val(String(data.contract_type_id)).trigger('change');
                        $('#ddlaccOwner').val(String(data.account_owner_id)).trigger('change');
                        $('#ddlsupOwner').val(String(data.support_owner_id)).trigger('change');
                    }, 100); // 👈 important


                    // Dates
                    if (data.onboarding_start_date) {
                        $('#onboarding_start_date').val(
                            data.onboarding_start_date.split('T')[0]
                        );
                    }

                    if (data.onboarding_completion_date) {
                        $('#onboarding_completion_date').val(
                            data.onboarding_completion_date.split('T')[0]
                        );
                    }

                    if (data.billing_start_date) {
                        $('#billing_start_date').val(
                            data.billing_start_date.split('T')[0]
                        );
                    }

                    if (data.contract_start_date) {
                        $('#contract_start_date').val(
                            data.contract_start_date.split('T')[0]
                        );
                    }

                    if (data.contract_end_date) {
                        $('#contract_end_date').val(
                            data.contract_end_date.split('T')[0]
                        );
                    }



                    // Hidden ID
                    $('#hdnClientId').val(data.client_id);
                    $("#hdnAction").val('Edit');
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
function GetAllCompanySize_DDL() {
    new APICALL(GetGlobalURL('Base', 'GetCompanySizeDDL'), 'GET', '', true).FETCH((result, error) => {
        if (result) {
            if (result.data != null) {
                window.allUsers = result.data;

                var ddl = $('#ddlCompanySize');
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
function GetAllCountry_DDL() {
    new APICALL(GetGlobalURL('Base', 'GetCountryDDL'), 'GET', '', true).FETCH((result, error) => {
        if (result) {
            if (result.data != null) {
                window.allUsers = result.data;

                var ddl = $('#ddlCountry');
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
function GetAllContractType_DDL() {
    new APICALL(GetGlobalURL('Base', 'GetContractTypeDDL'), 'GET', '', true).FETCH((result, error) => {
        if (result) {
            if (result.data != null) {
                window.allUsers = result.data;

                var ddl = $('#ddlContractType');
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
function GetAllAccountOwner_DDL() {
    new APICALL(GetGlobalURL('Base', 'GetAccountOwnerDDL'), 'GET', '', true).FETCH((result, error) => {
        if (result) {
            if (result.data != null) {
                window.allUsers = result.data;

                var ddl = $('#ddlaccOwner');
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
function GetAllSupportOwner_DDL() {
    new APICALL(GetGlobalURL('Base', 'GetSupportOwnerDDL'), 'GET', '', true).FETCH((result, error) => {
        if (result) {
            if (result.data != null) {
                window.allUsers = result.data;

                var ddl = $('#ddlsupOwner');
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
function GetAllBillingStatus_DDL() {
    new APICALL(GetGlobalURL('Base', 'GetBillingStatusDDL'), 'GET', '', true).FETCH((result, error) => {
        if (result) {
            if (result.data != null) {
                window.allUsers = result.data;

                var ddl = $('#ddlBillingStatus');
                populateDropdowns(ddl, result.data, null, 0);

                var ddl = $('#ddlInternalOwner');
                populateDropdowns(ddl, result.data, null, 0);

                var ddl = $('#ddlClientApprover');
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
    GetAllIndustry_DDL();
    GetAllCompanySize_DDL();
    GetAllCountry_DDL();
    GetAllContractType_DDL();
    GetAllAccountOwner_DDL();
    GetAllSupportOwner_DDL();
    GetAllContractTypeBasic_DDL();
    GetAllBillingFrequency_DDL();
    GetAllPaymentTerms_DDL();
    GetAllBillingType_DDL();
    GetAllBillingStatus_DDL();

}
function DoEmptyFields() {
    // Client Information
    $("#client_name").val('');
    $("#website").val('');
    $("#tax_registration_no").val('');
    $("#billing_address").val('');

    // DDLs (ensure options loaded first)
    $("#ddlIndustry").val('0');
    $("#ddlCompanySize").val('0');
    $("#ddlCountry").val('0');

    // Primary Contact
    $("#primary_contact_name").val('');
    $("#primary_contact_email").val('');
    $("#primary_contact_phone").val('');

    // Commercial Setup
    $("#ddlContractType").val('0');
    $("#ddlaccOwner").val('0');
    $("#ddlsupOwner").val('0');

    // Dates
    $("#onboarding_start_date").val(null);
    $("#onboarding_completion_date").val(null);
    $("#billing_start_date").val(null);
    $("#contract_start_date").val(null);
    $("#contract_end_date").val(null);

    // Internal Administration
    $("#highvalue").prop("checked", false);

    // Priority
    $("#prio-high").prop("checked", false);
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
    // Client Information
    $("#client_name").val("ABC Client");
    $("#website").val("https://www.abcclient.com");
    $("#tax_registration_no").val("NTN-1234567-8");
    $("#billing_address").val("Test Billing Address, Karachi Pakistan");

    // DDLs (ensure options loaded first)
    $("#ddlIndustry").prop('selectedIndex', 1);
    $("#ddlCompanySize").prop('selectedIndex', 1);
    $("#ddlCountry").prop('selectedIndex', 1);

    // Primary Contact
    $("#primary_contact_name").val("John Smith");
    $("#primary_contact_email").val("john.smith@test.com");
    $("#primary_contact_phone").val("03001234567");

    // Commercial Setup
    $("#ddlContractType").prop('selectedIndex', 1);
    $("#ddlaccOwner").prop('selectedIndex', 1);
    $("#ddlsupOwner").prop('selectedIndex', 1);

    // Dates
    $("#onboarding_start_date").val("2025-08-01");
    $("#onboarding_completion_date").val("2025-08-15");
    $("#billing_start_date").val("2025-08-16");
    $("#contract_start_date").val("2025-08-16");
    $("#contract_end_date").val("2026-08-15");

    // Internal Administration
    $("#highvalue").prop("checked", true);

    // Priority
    $("#prio-high").prop("checked", true);

    console.log("Default testing values loaded.");
}
