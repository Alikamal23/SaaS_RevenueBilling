var instanceid = 0;
var currentTab = "";
$(document).ready(function () {
    instanceid = GetParameterValues('instanceid') ?? 0;
    LoadViewMyRequest(instanceid);
    $("#intiateform").on("click", ".savebutton", function () {
        mynextsetp(this);
    });
    document.addEventListener('click', function (e) {
        const card = e.target.closest('.flip-card');
        if (card && document.body.contains(card)) {
            card.classList.toggle('hover');
        }
    });

});
async function LoadViewMyRequest(instanceid) {
    FillDropdowns_OnLoad();

    //await GetFormsByInstanceId($('#workflow').val(), instanceid, "appendForm", "intiate");
    GetCOBDetailByInstanceId(instanceid);

    getWorkflowLog(instanceid);
    GetWorkflowAction($('#workflow').val(), instanceid, "intiateform");

    // Tab Navigation Initialize
    InitializeTabNavigation();

    updateActionButtons();

}

/* Save by workflow */
function mynextsetp(btn) {
    var save = $(btn).attr('data-save') === "true";
    var move = $(btn).attr('data-move') === "true";
    var actionId = $(btn).attr('data-id');
    var assignmenttype = $(btn).attr('data-assignmenttype');
    var dynamicfunction = $(btn).attr('data-dynamicfunction');

    var _api = getApiByTab(currentTab);

    if (!validateActiveTab(currentTab)) {
        return;
    }

    if (save && move) {
        Swal.fire({
            title: 'Do you want to save?',
            showDenyButton: true,
            showCancelButton: true,
            confirmButtonText: 'Yes',
            denyButtonText: 'No',
        }).then((result) => {
            if (result && result.dismiss != 'cancel') {
                var Data = $('#intiateform').serialize() + '&instanceid=' + encodeURIComponent(instanceid);
                //new APICALL(GetGlobalURL('Base', 'SaveCOBInitiateRequest'), 'POST', Data, false, false, 'application/x-www-form-urlencoded').FETCH((result, error) => {
                new APICALL(GetGlobalURL('Base', _api), 'POST', Data, false, false, 'application/x-www-form-urlencoded').FETCH((result, error) => {
                    if (result) {
                        var workflowMove = {
                            instanceid: result.data[0].instanceid,
                            actionid: actionId,
                            dynamicfunction: dynamicfunction,
                            assignmenttype: assignmenttype,
                            comment: $("#remarks").val()
                        };
                        var urlEncodedData = Object.keys(workflowMove)
                            .map(key => encodeURIComponent(key) + '=' + encodeURIComponent(workflowMove[key] ?? ''))
                            .join('&');

                        new APICALL(GetGlobalURL('Base', 'MoveWorkflow'), 'POST', urlEncodedData, false, false, 'application/x-www-form-urlencoded').FETCH((result, error) => {
                            if (result) {
                                Swal.fire({
                                    icon: 'success',
                                    title: 'Success...',
                                    text: 'Workflow moved successfully!',
                                    timer: 2000,
                                    showConfirmButton: false
                                }).then(() => {
                                    const defaultForm = document.getElementById("defaultform").value;
                                    window.location.href = "/" + defaultForm;
                                });
                            }
                            if (error) {
                                Swal.fire({
                                    icon: 'error',
                                    title: 'Error...',
                                    text: error.data.responseText,
                                });
                            }
                        });

                    }
                    if (error) {
                        Swal.fire({
                            icon: 'error',
                            title: 'Error...',
                            text: error.data.responseText,
                        });
                    }
                });
            }
        });

    }
    else if (save) {
        SaveInitiateRequest();
    }
    else if (move) {
        MoveMyRequest(actionId, dynamicfunction, assignmenttype);
    }
}
function SaveInitiateRequest() {
    if (!ValidateInitiateTab()) {
        return;
    }

    Swal.fire({
        title: 'Do you want to save?',
        showDenyButton: true,
        showCancelButton: true,
        confirmButtonText: 'Yes',
        denyButtonText: 'No',
    }).then((result) => {
        if (result.isConfirmed) {
            var Data = $('#intiateform').serialize();

            //Data.push(
            //    { name: "contract_upload", value: $("#doc").val().split("\\").pop() },
            //    { name: "nda_upload", value: $("#nda").val().split("\\").pop() },
            //    { name: "proposal_upload", value: $("#proposal").val().split("\\").pop() }
            //);

            //var formData = new FormData();

            //formData.append("contract", $("#doc")[0].files[0]);
            //formData.append("nda", $("#nda")[0].files[0]);
            //formData.append("proposal", $("#proposal")[0].files[0]);

            //console.log($('#intiateform').serialize());

            new APICALL(GetGlobalURL('Base', 'SaveCOBInitiateRequest'), 'POST', Data, false, false, 'application/x-www-form-urlencoded').FETCH((result, error) => {
                if (result) {
                    Swal.fire({
                        icon: 'success',
                        title: 'Success...',
                        text: 'Saved Successfully!',
                        timer: 2000,
                        showConfirmButton: false
                    }).then(() => {
                        const defaultForm = document.getElementById("defaultform").value;
                        window.location.href = "/" + defaultForm;
                    });
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
    });
}
function MoveMyRequest(actionId, dynamicfunction, assignmenttype) {
    Swal.fire({
        title: 'Do you want to move the workflow?',
        showDenyButton: true,
        showCancelButton: false,
        confirmButtonText: 'Yes',
        denyButtonText: 'No',
    }).then((result) => {
        if (result.isConfirmed) {
            var workflowMove = {
                instanceid: instanceid,
                actionid: actionId,
                dynamicfunction: dynamicfunction,
                assignmenttype: assignmenttype,
                comment: $("#remarks").val()
            };
            var urlEncodedData = Object.keys(workflowMove)
                .map(key => encodeURIComponent(key) + '=' + encodeURIComponent(workflowMove[key] ?? ''))
                .join('&');

            new APICALL(GetGlobalURL('Base', 'MoveWorkflow'), 'POST', urlEncodedData, false, false, 'application/x-www-form-urlencoded').FETCH((result, error) => {
                if (result) {
                    Swal.fire({
                        icon: 'success',
                        title: 'Success...',
                        text: 'Workflow moved successfully!',
                        timer: 2000, 
                        showConfirmButton: false
                    }).then(() => {
                        const defaultForm = document.getElementById("defaultform").value;
                        window.location.href = "/" + defaultForm;
                    });
                }
                if (error) {
                    Swal.fire({
                        icon: 'error',
                        title: 'Error...',
                        text: error.data.responseText,
                    });
                }
            });
        }
    });
}

function GetCOBDetailByInstanceId(instanceid) {
    new APICALL(GetGlobalURL('Base', 'GetCOBDetailByInstanceId') + '?instanceid=' + instanceid, 'GET', '', false).FETCH((result, error) => {

        if (result) {
            if (result.data != null && result.data.length > 0 ) {

                try {
                    const data = result.data;

                    console.log(data);

                    //$('#Department').val(data[0].dept ?? '');
                    //$('#Purpose').val(data[0].purpose ?? '');
                    //$('#EstimatedAmount').val(data[0].estimatedamount ?? '');
                    //let date = new Date(data[0].requiredby);
                    //let formatted = date.toISOString().split('T')[0];
                    //$('#RequiredBy').val(formatted);
                    //$('#Priority').val(data[0].priority ?? '');
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
    $('#Department').val('');
    $('#Purpose').val('');
    $('#EstimatedAmount').val('');
    $('#RequiredBy').val('');
    $('#Priority').val('');
    $('#bloombergcode').val('');
}


/* Tab Navigation Next & Prev */
function InitializeTabNavigation() {

    // Next Button
    $(document).on('click', '.btn-next', function () {

        var currentTab = $('#myTab .nav-link.active');
        var nextTab = currentTab.closest('.nav-item').next().find('.nav-link');

        if (nextTab.length) {
            bootstrap.Tab.getOrCreateInstance(nextTab[0]).show();
        }
    });

    // Previous Button
    $(document).on('click', '.btn-prev', function () {

        var currentTab = $('#myTab .nav-link.active');
        var prevTab = currentTab.closest('.nav-item').prev().find('.nav-link');

        if (prevTab.length) {
            bootstrap.Tab.getOrCreateInstance(prevTab[0]).show();
        }
    });

    // Tab change hone par buttons update
    $('#myTab .nav-link').on('shown.bs.tab', function () {
        UpdateTabButtons();
        UpdateActiveTabTitle();   // ✅ add this
    });

    UpdateTabButtons();
    UpdateActiveTabTitle();
}
function UpdateTabButtons() {

    var tabs = $('#myTab .nav-link');
    var activeTab = $('#myTab .nav-link.active');

    var activeIndex = tabs.index(activeTab);
    var totalTabs = tabs.length;

    // Sab buttons enable
    $('.btn-prev').prop('disabled', false);
    $('.btn-next').prop('disabled', false);

    // Current active pane
    var currentPane = $(activeTab.data('bs-target'));

    // First Tab
    if (activeIndex === 0) {
        currentPane.find('.btn-prev').prop('disabled', true);
    }

    // Last Tab
    if (activeIndex === totalTabs - 1) {
        currentPane.find('.btn-next').prop('disabled', true);
    }
}
function updateActionButtons() {
    const totalTabs = $(".tab-pane").length;
    const activeIndex = $(".tab-pane.active").index() + 1;

    const isLastTab = (activeIndex === totalTabs);

    // Save / Submit buttons dynamically injected hain
    $("#actionId .savebutton, #actionId .submitbutton").each(function () {
        if (isLastTab) {
            $(this).prop("disabled", false).removeClass("disabled");
        } else {
            $(this).prop("disabled", true).addClass("disabled");
        }
    });
}

/* Validations Tab wise */
function ValidateInitiateTab() {

    if ($("#client_name").val() == "") {
        Swal.fire('Validation', 'Client Name is required.', 'warning');
        $("#client_name").focus();
        return false;
    }

    if ($("#ddlIndustry").val() == "0" || $("#ddlIndustry").val() == null) {
        Swal.fire('Validation', 'Please select Industry.', 'warning');
        $("#ddlIndustry").focus();
        return false;
    }

    if ($("#ddlCompanySize").val() == "0" || $("#ddlCompanySize").val() == null) {
        Swal.fire('Validation', 'Please select Company Size.', 'warning');
        $("#ddlCompanySize").focus();
        return false;
    }

    if ($("#txtBillingAddress").val() == "") {
        Swal.fire('Validation', 'Billing Address is required.', 'warning');
        $("#txtBillingAddress").focus();
        return false;
    }

    if ($("#ddlCountry").val() == "0" || $("#ddlCountry").val() == null) {
        Swal.fire('Validation', 'Please select Country.', 'warning');
        $("#ddlCountry").focus();
        return false;
    }

    if ($("#txtPrimaryContactName").val() == "") {
        Swal.fire('Validation', 'Primary Contact Name is required.', 'warning');
        $("#txtPrimaryContactName").focus();
        return false;
    }

    if ($("#txtPrimaryContactEmail").val() == "") {
        Swal.fire('Validation', 'Primary Contact Email is required.', 'warning');
        $("#txtPrimaryContactEmail").focus();
        return false;
    }

    //var email = $("#txtPrimaryContactEmail").val();
    //var emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

    //if (!emailPattern.test(email)) {
    //    Swal.fire('Validation', 'Please enter a valid Email Address.', 'warning');
    //    $("#txtPrimaryContactEmail").focus();
    //    return false;
    //}

    if ($("#txtPrimaryContactPhone").val() == "") {
        Swal.fire('Validation', 'Primary Contact Phone is required.', 'warning');
        $("#txtPrimaryContactPhone").focus();
        return false;
    }

    if ($("#ddlContractType").val() == "0" || $("#ddlContractType").val() == null) {
        Swal.fire('Validation', 'Please select Contract Type.', 'warning');
        $("#ddlContractType").focus();
        return false;
    }

    if ($("#ddlaccOwner").val() == "0" || $("#ddlaccOwner").val() == null) {
        Swal.fire('Validation', 'Please select Account Owner.', 'warning');
        $("#ddlaccOwner").focus();
        return false;
    }

    if ($("#ddlsupOwner").val() == "0" || $("#ddlsupOwner").val() == null) {
        Swal.fire('Validation', 'Please select Support Owner.', 'warning');
        $("#ddlsupOwner").focus();
        return false;
    }

    if ($("#onboarding_start_date").val() == "") {
        Swal.fire('Validation', 'Please select Onboarding Start Date.', 'warning');
        return false;
    }

    if ($("#onboarding_completion_date").val() == "") {
        Swal.fire('Validation', 'Please select Onboarding Completion Date.', 'warning');
        return false;
    }

    if ($("#billing_start_date").val() == "") {
        Swal.fire('Validation', 'Please select Billing Start Date.', 'warning');
        return false;
    }

    if ($("#contract_start_date").val() == "") {
        Swal.fire('Validation', 'Please select Contract Start Date.', 'warning');
        return false;
    }

    if ($("#contract_end_date").val() == "") {
        Swal.fire('Validation', 'Please select Contract End Date.', 'warning');
        return false;
    }

    //if ($("#ddlPriorityLevel").val() == "" || $("#ddlPriorityLevel").val() == null) {
    //    Swal.fire('Validation', 'Please select Priority Level.', 'warning');
    //    return false;
    //}

    return true;
}
function validateContract() {

    if ($("#contract_type").val() == "0") {
        Swal.fire('Validation', 'Contract Type required', 'warning');
        return false;
    }

    //if ($("#contract_start_date").val() == "") {
    //    Swal.fire('Validation', 'Start Date required', 'warning');
    //    return false;
    //}

    return true;
}

function UpdateActiveTabTitle() {
    var activeTab = $('#myTab .nav-link.active');
    var title = activeTab.text().trim();

    $("#activeTabTitle").text(title);
    currentTab = title;
}
function getApiByTab(tabName) {
    switch (tabName) {
        case "Client Onboarding":
            return "SaveCOBInitiateRequest";

        case "Contract":
            return "SaveContractInfo";

        case "Billing":
            return "SaveBillingProfile";

        case "Recurring Billing":
            return "SaveRecurringBilling";

        case "Milestone":
            return "SaveMilestone";

        case "Milestone Completion":
            return "SaveMilestoneCompletion";

        case "Invoice":
            return "SaveInvoice";

        case "Payment":
            return "SavePayment";

        case "AR Exception":
            return "SaveARException";

        case "Renewal":
            return "SaveRenewal";

        default:
            return "";
    }
}
function validateActiveTab(tabName) {
    switch (tabName) {
        case "Client Onboarding":
            return ValidateInitiateTab();

        case "Contract":
            return validateContract();

        //case "Billing":
        //    return validateBilling();

        default:
            Swal.fire("Validation", "No validation defined for this tab", "warning");
            return false;
    }
}


/* Direct Save */ // now not used direct
function SaveData() {
    //var res = ValidateAll();
    //if (res == false) {
    //    return false;
    //}

    Swal.fire({
        title: 'Do you want to save the changes?',
        showDenyButton: true,
        showCancelButton: true,
        confirmButtonText: 'Ok',
        denyButtonText: 'Cancel',
    }).then((result) => {
        if (result) {
            ShowLoader('UserMasterDiv');

            var data = {
                Id: 1,
                Name: 'alikamal'
            }

            // API call using JSON instead of form-urlencoded
            new APICALL(GetGlobalURL('Base', 'SaveClientonBoarding'), 'POST', JSON.stringify(data), true, false, 'application/json; charset=utf-8')
                .FETCH((result, error) => {

                    if (result && result.status === 'success') {
                        ////ViewData();
                        ////DoEmptyFields();

                        Swal.fire({
                            icon: 'success',
                            title: 'Success...',
                            text: 'Saved Successfully!'
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


// ++++++++++++++++File upload system+++++++++++++++++++++++++++

$(".btnFileUploader").on('click', function () {
    var description = $("#txtFiledescription").val();
    var file = $("#fileInput").val();

    if (!file || !file.trim()) {
        Swal.fire({
            icon: 'warning',
            text: 'Please Select the file!'
        });
        return false;
    }

    if (!description || description === 'string' || !description.trim()) {
        Swal.fire({
            icon: 'warning',
            text: 'Please Fill File Description!'
        });
        return false;
    }

    var fileUpload = $("#fileInput").get(0);
    var files = fileUpload.files;

    var formData = new FormData();

    for (var i = 0; i < files.length; i++) {
        var newName =
            'COB_' +
            instanceid +
            '_' +
            getCurrentDateTime() +
            '.' +
            files[i].name.split('.').pop();

        formData.append("files", files[i], newName);
    }

    var enCodeDescrpiton = encodeURIComponent(description);
    var UserID = 1;

    var url = "UploadToFileSystem/" +
        "-" + "/" +
        enCodeDescrpiton + "/" +
        UserID + "/" +
        instanceid;

    new APICALL(
        GetGlobalURL('Base', url),
        'POST',
        formData,
        false
    ).FETCH((result, error) => {
        if (result) {
            Swal.fire({
                icon: 'success',
                text: 'File Uploaded Successfully'
            });

            $("#fileInput").val('');
            $(".custom-file-label").text('Choose file');
            $("#txtFiledescription").val('');

            //GetUpladedFiles();
        }

        if (error) {
            Swal.fire({
                icon: 'error',
                title: 'Error...',
                text: error?.data?.responseText || 'Upload failed'
            });
        }
    });

});

function GetUpladedFiles() {
    new APICALL(
        GetGlobalURL('Workflow', 'WorkFlow/GetUploadedFiles/' + instanceid),
        'POST',
        '',
        true
    ).FETCH((result, error) => {
        if (result) {
            GetUpladedFilesGrid(result);
        }

        if (error) {
            console.log(error);

            Swal.fire({
                icon: 'error',
                title: 'Error...',
                text: error?.data?.responseText || 'Something went wrong'
            });
        }
    });
}
function GetUpladedFilesGrid(datasource) {
    var data = datasource.data;

    $('#gridContainerFiles').DataTable().destroy();
    $("#gridContainerFiles").DataTable({
        "responsive": true,
        "autoWidth": false,
        "aaData": data,
        "columns": [

            { data: "fullfilename" },
            { data: "description" },
            { data: "userName" },
            { data: "createdOn" },
            {
                "orderable": false,
                "data": null,
                render: function (data, type, row) {
                    if (type === 'display') {
                        var column;
                        if (data.user_ID == user_ID) {
                            column = '<div class="btn-group btn-group-sm">' +
                                '<a class="avatar-text avatar-md me-2 btnViewClass-' + data.id + '" onclick="return downloadFunctionWithLoader(\'' + data.id + '\',\'btnViewClass-' + data.id + '\');" data-bs-toggle="tooltip data-bs-placement="bottom" title="View"><i class="feather-eye view-icon"></i></a>' +
                                '<a class="avatar-text avatar-md" onclick="DeleteBAFile(\'' + data.id + '\',\'' + data.fullfilename + '\')" data-bs-toggle="tooltip data-bs-placement="bottom" title="Delete"><i class="feather-trash delete-icon"></i></a>' +
                                '</div >'
                        }
                        else {
                            column = '<div class="btn-group btn-group-sm">' +
                                '<a class="avatar-text avatar-md me-2 btnViewClass-' + data.id + '" onclick="return downloadFunctionWithLoader(\'' +
                                data.id + '\',\'btnViewClass-' + data.id + '\');" data-bs-toggle="tooltip data-bs-placement="bottom" title="View"><i class="feather-eye view-icon"></i></a>' +
                                '</div >'
                        }
                        var html = column;
                        return html;
                    }
                    return data;
                },
            },
        ],
        "columnDefs": [{
            "targets": 'no-sort',
            "orderable": false,
        },
        {
            "targets": 3,
            "render": function (data, type, row, meta) {

                return moment(data).format('DD-MMM-YYYY hh:mm:ss');
            },
        },
        ]
    });
}
function getCurrentDateTime() {
    var dt = new Date();
    return (`${dt.getDate().toString().padStart(2, '0')}-${(dt.getMonth() + 1).toString().padStart(2, '0')}-${dt.getFullYear().toString().padStart(4, '0')}_${dt.getHours().toString().padStart(2, '0')}-${dt.getMinutes().toString().padStart(2, '0')}-${dt.getSeconds().toString().padStart(2, '0')}`).toString();
}

// ===============file upload end here===========================
