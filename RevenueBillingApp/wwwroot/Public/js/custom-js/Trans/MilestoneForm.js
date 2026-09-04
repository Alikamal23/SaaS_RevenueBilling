var instanceid = 0;
var deliverables = [];

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
    ////    $("#ddlClient").val($("#ddlClient option:eq(1)").val());
    ////    $("#ddlContract").val($("#ddlContract option:eq(1)").val());
    ////    $("#ddlInternalOwner").val($("#ddlInternalOwner option:eq(1)").val());
    ////    $("#ddlClientApprover").val($("#ddlClientApprover option:eq(1)").val());
    ////}, 2000);

    //GetCOBDetailByInstanceId(instanceid);

    var milestoneId = $("#hdnMilestoneId").val();
    if (milestoneId && milestoneId != "" && milestoneId != "0") {
        ////alert('Edit Mode');
        $("#hdnAction").val('Edit');

        GetMilestoneById(milestoneId);
    }

    var action = $("#hdnAction").val();
    if (action === 'Add') {
        $("#docUpload").show();
    } else {
        $("#docUpload").hide();
    }

}

$('.multi-field-wrapper').each(function () {
    var $wrapper = $('.multi-fields', this);

    $(".add-field", $(this)).click(function () {
        var totalRows = $('.multi-field', $wrapper).length;

        // Max 5 rows allowed
        if (totalRows >= 5) {
            alert("Maximum 5 rows allowed.");
            return false;
        }

        $('.multi-field:first', $wrapper)
            .clone(true)
            .appendTo($wrapper)
            .find('input, select, textarea')
            .val('');
    });

    $('.multi-field .remove-field', $wrapper).click(function () {
        var totalRows = $('.multi-field', $wrapper).length;

        // Keep at least 1 row
        if (totalRows > 1) {
            $(this).closest('.multi-field').remove();
        }
        else {
            alert("At least one row is required.");
        }
    });

});

function ValidateAll() {

    $(".is-invalid").removeClass("is-invalid");

    // Client
    if ($("#ddlClient").val() == "0" || !$("#ddlClient").val()) {
        setInvalid("#ddlClient", "Please select Client!");
        return false;
    }

    if ($("#ddlContract").val() == "0" || !$("#ddlContract").val()) {
        setInvalid("#ddlContract", "Please select Contract!");
        return false;
    }

    // Milestone Details
    if (!$("#milestone_name").val()?.trim()) {
        setInvalid("#milestone_name", "Please enter Milestone Name!");
        return false;
    }

    if (!$("#milestone_amount").val()?.trim()) {
        setInvalid("#milestone_amount", "Please enter Milestone Amount!");
        return false;
    }

    if (!$("#exp_completion_date").val()) {
        setInvalid("#exp_completion_date", "Please select Expected Completion Date!");
        return false;
    }

    // Team & Approval
    if ($("#ddlInternalOwner").val() == "0" || !$("#ddlInternalOwner").val()) {
        setInvalid("#ddlInternalOwner", "Please select Internal Owner!");
        return false;
    }

    if ($("#ddlClientApprover").val() == "0" || !$("#ddlClientApprover").val()) {
        setInvalid("#ddlClientApprover", "Please select Client Approver!");
        return false;
    }

    // Client Approval
    if (!$("input[name='question']:checked").length) {
        setInvalid("input[name='question']", "Please select Client Approval!");
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

            formData.append("workflow", "MIL");
            formData.append("instanceid", 0);

            var rawMilestoneId = $("#hdnMilestoneId").val();
            var milestoneId = (!rawMilestoneId || isNaN(rawMilestoneId)) ? 0 : parseInt(rawMilestoneId);

            formData.append("milestone_id", milestoneId);
            formData.append("client_id", $("#ddlClient").val());
            formData.append("contract_id", $("#ddlContract").val());
            formData.append("milestone_name", $("#milestone_name").val());
            formData.append("milestone_amount", $("#milestone_amount").val());
            formData.append("exp_completion_date", $("#exp_completion_date").val());
            formData.append("completion_date", $("#completion_date").val());
            formData.append("milestone_desc", $("#milestone_desc").val());
            formData.append("internal_owner_id", $("#ddlInternalOwner").val());
            formData.append("client_approver_id", $("#ddlClientApprover").val());

            //formData.append("deliverables_list", $("#deliverables_list").val());
            $(".multi-fields .multi-field input").each(function () {
                var val = $(this).val().trim();

                if (val !== "") {
                    deliverables.push(val);
                }
            });
            formData.append("deliverables_list", deliverables.join(","));

            formData.append("dependencies", $("#dependencies").val());
            formData.append("client_approval", $("input[name='question']:checked").val());
            formData.append("client_comments", $("#client_comments").val());
            formData.append("qa_comments", $("#qa_comments").val());
            formData.append("userid", 1);

            // =========================
            // FILE UPLOAD
            // =========================
            var fileInput = document.querySelector(".file-input");

            if (fileInput && fileInput.files && fileInput.files.length > 0) {
                formData.append("docUpload", fileInput.files[0]);
            }


            //// API call using JSON instead of form-urlencoded
            ////new APICALL(GetGlobalURL('Base', 'SaveClientonBoarding'), 'POST', formData, true, true, false, 'multipart/form-data')
            new APICALL(GetGlobalURL('Base', 'SaveMilestone'), 'POST', formData, true, true)
                .FETCH((result, error) => {
                    //console.log('aaaaa');
                    //console.log(result);
                    //console.log(error);
                    //console.log(result.data.status);
                    //console.log(result.data.message);

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
function GetMilestoneById(milestoneId) {
    new APICALL(GetGlobalURL('Base', 'GetMilestoneById') + '?milestoneId=' + milestoneId, 'GET', '', false).FETCH((result, error) => {

        if (result) {
            if (result.data != null && result.data.length > 0) {

                try {
                    const data = result.data[0];
                    console.log("MILESTONE DATA:", data);

                    // =========================
                    // BASIC MILESTONE INFO
                    // =========================
                    $("#milestone_name").val(data.milestone_name ?? '');
                    $("#milestone_amount").val(data.milestone_amount ?? '');
                    $("#milestone_desc").val(data.milestone_desc ?? '');

                    $("#dependencies").val(data.dependencies ?? '');
                    $("#client_comments").val(data.client_comments ?? '');
                    $("#qa_comments").val(data.qa_comments ?? '');

                    // =========================
                    // DROPDOWNS
                    // =========================
                    setTimeout(() => {
                        $("#ddlClient").val(String(data.client_id)).trigger("change");
                        $("#ddlContract").val(String(data.contract_id)).trigger("change");
                        $("#ddlInternalOwner").val(String(data.internal_owner_id)).trigger("change");
                        $("#ddlClientApprover").val(String(data.client_approver_id)).trigger("change");
                    }, 100);

                    // =========================
                    // DATES
                    // =========================
                    if (data.exp_completion_date) {
                        $("#exp_completion_date").val(data.exp_completion_date.split('T')[0]);
                    }

                    if (data.completion_date) {
                        $("#completion_date").val(data.completion_date.split('T')[0]);
                    }

                    // =========================
                    // CLIENT APPROVAL RADIO
                    // =========================
                    if (data.client_approval) {
                        $(`input[name='question'][value='${data.client_approval}']`).prop("checked", true);
                    }

                    // =========================
                    // DELIVERABLES (multi fields)
                    // =========================
                    if (data.deliverables_list) {

                        let items = data.deliverables_list.split(',');

                        let wrapper = $(".multi-fields");
                        wrapper.empty();

                        items.forEach((item, index) => {

                            let html = `
                            <div class="multi-field">
                                <input type="text" class="form-control" value="${item.trim()}" />
                                <button type="button" class="remove-field">X</button>
                            </div>
                        `;

                            wrapper.append(html);
                        });
                    }

                    // =========================
                    // FILE (optional preview)
                    // =========================
                    if (data.docUpload) {
                        $("#existingFile").html(
                            `<a href="${data.docUpload}" target="_blank">View Document</a>`
                        );
                    }

                    // Hidden ID
                    $("#hdnInstanceId").val(data.instanceid);

                    $('#hdnMilestoneId').val(data.milestone_id);
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
    GetClientInfoDDL();
    GetContractInfoDDL();
    GetAllBillingStatus_DDL();
}
function DoEmptyFields() {
    // Client Info
    $("#ddlClient").val('0');
    $("#ddlContract").val('0');

    // Milestone Details
    $("#milestone_name").val('');
    $("#milestone_amount").val('');
    $("#exp_completion_date").val('');
    $("#completion_date").val('');
    $("#milestone_desc").val('');

    // Team & Approval
    $("#ddlInternalOwner").val('0');
    $("#ddlClientApprover").val('0');

    // Deliverables
    $("#deliverables_list").val('');

    // Dependencies
    $("#dependencies").val('');

    // Evidence Upload
    $(".file-input").val('');
    $(".preview-container").empty();

    // Client Approval
    $("#rdoYes").prop("checked", false);
    $("#rdoNo").prop("checked", false);

    $("#client_comments").val('');

    // QA
    $("#qa_comments").val('');
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

    // Milestone Details
    $("#milestone_name").val("UI Design Approval");
    $("#milestone_amount").val("50000");
    $("#exp_completion_date").val("2025-08-15");
    $("#completion_date").val("2025-08-14");
    $("#milestone_desc").val("Initial UI/UX design approval by client.");

    // Team & Approval
    $("#ddlInternalOwner").prop('selectedIndex', 1);
    $("#ddlClientApprover").prop('selectedIndex', 1);

    // Deliverables
    $("#deliverables_list").val("Homepage Design");

    // Dependencies
    $("#dependencies").val("Client feedback and branding assets");

    // Client Approval
    $("#rdoYes").prop("checked", true);

    $("#client_comments").val("Approved with minor changes.");

    // QA
    $("#qa_comments").val("QA verified and approved.");

    console.log("Default testing values loaded.");
}
