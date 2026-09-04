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
    ////    $("#ddlClient").val($("#ddlClient option:eq(1)").val());
    ////    $("#ddlContract").val($("#ddlContract option:eq(1)").val());
    ////}, 2000);

    var renewId = $("#hdnRenewId").val();
    if (renewId && renewId != "" && renewId != "0") {
        ////alert('Edit Mode');
        $("#hdnAction").val('Edit');

        GetRenewalFormById(renewId);
    }

    var action = $("#hdnAction").val();
    if (action === 'Add') {
        $("#docSection").show();
    } else {
        $("#docSection").hide();
    }

}

function ValidateAll() {

    $(".is-invalid").removeClass("is-invalid");

    if (!$("#ddlClient").val() || $("#ddlClient").val() == "0") {
        setInvalid("#ddlClient", "Please select Client!");
        return false;
    }

    if (!$("#ddlContract").val() || $("#ddlContract").val() == "0") {
        setInvalid("#ddlContract", "Please select Contract!");
        return false;
    }

    if (!$("#renewal_term").val()?.trim()) {
        setInvalid("#renewal_term", "Please enter Renewal Term!");
        return false;
    }

    if (!$("#contract_value").val()?.trim()) {
        setInvalid("#contract_value", "Please enter Contract Value!");
        return false;
    }

    if (!$("#discount").val()?.trim()) {
        setInvalid("#discount", "Please enter Discount!");
        return false;
    }

    if (!$("#start_date").val()) {
        setInvalid("#start_date", "Please select Start Date!");
        return false;
    }

    if (!$("#end_date").val()) {
        setInvalid("#end_date", "Please select End Date!");
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

            var rawRenewId = $("#hdnRenewId").val();
            ////alert(rawRenewId);
            var renewId = (!rawRenewId || isNaN(rawRenewId)) ? 0 : parseInt(rawRenewId);
            ////alert(renewId);

            formData.append("workflow", "COB");
            formData.append("instanceid", 0);

            var renewId = $("#hdnRenewId").val() || 0;
            formData.append("renew_id", renewId);

            formData.append("client_id", $("#ddlClient").val());
            formData.append("contract_id", $("#ddlContract").val());

            formData.append("renewal_term", $("#renewal_term").val());
            formData.append("contract_value", $("#contract_value").val());
            formData.append("discount", $("#discount").val());

            formData.append("start_date", $("#start_date").val());
            formData.append("end_date", $("#end_date").val());

            formData.append("userid", 1);


            if ($("#doc_upload")[0].files.length > 0)
                formData.append("doc_upload", $("#doc_upload")[0].files[0]);


            //// API call using JSON instead of form-urlencoded
            ////new APICALL(GetGlobalURL('Base', 'SaveRenewalForm'), 'POST', formData, true, true, false, 'multipart/form-data')
            new APICALL(GetGlobalURL('Base', 'SaveRenewalForm'), 'POST', formData, true, true)
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

/* Fill Client Form in EDIT Mode */
function GetRenewalFormById(renewId) {
    new APICALL(GetGlobalURL('Base', 'GetRenewalFormById') + '?renewId=' + renewId, 'GET', '', false).FETCH((result, error) => {

        if (result) {
            if (result.data != null && result.data.length > 0) {

                try {
                    const data = result.data[0];
                    console.log(data);

                    // =========================
                    // Hidden Keys
                    // =========================
                    $("#hdnRenewId").val(data.renew_id);
                    $("#hdnAction").val("Edit");

                    // =========================
                    // Dropdowns
                    // =========================
                    setTimeout(() => {
                        $("#ddlClient").val(String(data.client_id)).trigger("change");
                        $("#ddlContract").val(String(data.contract_id)).trigger("change");
                    }, 100);

                    // =========================
                    // Basic Info
                    // =========================
                    $("#renewal_term").val(data.renewal_term ?? '');
                    $("#contract_value").val(data.contract_value ?? 0);
                    $("#discount").val(data.discount ?? 0);

                    // =========================
                    // Dates (SAFE)
                    // =========================
                    if (data.start_date) {
                        $("#start_date").val(data.start_date.split('T')[0]);
                    }

                    if (data.end_date) {
                        $("#end_date").val(data.end_date.split('T')[0]);
                    }

                    // =========================
                    // Document (optional preview)
                    // =========================
                    if (data.doc_upload) {
                        $("#docPreview").html(
                            '<a class="btn btn-sm btn-primary" target="_blank" href="/uploadedfiles/RenewalDocs/' +
                            data.doc_upload + '">View Document</a>'
                        );
                    }

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
}
function DoEmptyFields() {
    $("#hdnRenewId").val("0");

    $("#ddlClient").val("0").trigger("change");
    $("#ddlContract").val("0").trigger("change");

    $("#renewal_term").val("");
    $("#contract_value").val("");
    $("#discount").val("");

    $("#start_date").val("");
    $("#end_date").val("");

    $("#doc_upload").val("");

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
    $("#ddlClient").prop('selectedIndex', 1).trigger("change");

    setTimeout(function () {
        $("#ddlContract").prop('selectedIndex', 1);
    }, 300);

    $("#renewal_term").val("1 Year Renewal");
    $("#contract_value").val("500000");
    $("#discount").val("10");

    $("#start_date").val("2025-08-01");
    $("#end_date").val("2026-08-01");

    console.log("Renewal default test values loaded.");

}
