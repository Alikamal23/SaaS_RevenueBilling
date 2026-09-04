var filespath = [];

$(document).ready(function () {
    toggleDiv();

    GetCategories();
    $('#correctanswer_feedback').hide();
    $('#correctanswer_yes').hide();
    $('#btn_upload').click(UploadDocument);

    $('#questiondiv').on('change', '.form-check-input', function () {
        var answerType = $(this).attr('asnwertype-id');
        var radios_ans = document.querySelectorAll('#answerdiv input[type="radio"]');
        var radios_fed = document.querySelectorAll('#feedbackdiv input[type="radio"]');

        if ($(this).is(':checked')) {
            if (answerType == '2') {
                $('#correctanswer_yes').show();
            }

            if (answerType == '3') {
                $('#correctanswer_feedback').show();
            }

        }
        else {

            if (answerType == '2') {
                $('#correctanswer_yes').hide();
                radios_ans.forEach(radio => {
                    radio.checked = false;
                });

            }

            if (answerType == '3') {
                $('#correctanswer_feedback').hide();
                radios_fed.forEach(radio => {
                    radio.checked = false;
                });
            }

        }
    });

    $('#btn_save').click(InsertModel);
    $('#btn_update').click(UpdateModel);
    $('#btn_update').hide().prop('disabled', true);
    $('#btn_cancel').click(ClearFields);

    $('#tbl_cwmform').on('click', '.btn.first-btn', function () {
        var modelid = $(this).closest('tr').find('td:first').text();
        //alert('hello: ' + boardid);
        EditModel(modelid);
    });

    $('a[data-bs-toggle="tab"]').on('shown.bs.tab', function () {
        toggleDiv();
    });


});

function toggleDiv() {
    if ($('#cwmform').hasClass('active')) {
        $('#divtable').show();
    } else {
        $('#divtable').hide();
    }
}
function GETCWMFORMS() {
    new APICALL(GetGlobalURL('Base', 'GetCWMFoms'), 'GET', '', true).FETCH((result, error) => {
        if (result) {
            if (result.data != null && result.data.length > 0) {
                $('#tbl_cwmform').DataTable().destroy();
                $('#tbl_cwmform tbody').empty();
                var sr = 0;
                $.each(result.data, function (i, option) {
                    sr++;
                    $('#tbl_cwmform tbody').append(
                        '<tr>' +
                        '<td>' + sr + '</td>' +
                        '<td>' + option.category_type + '</td>' +
                        '<td>' + option.company_standard + '</td>' +
                        '<td>' + option.standard + '</td>' +
                        '<td>' + option.guideline + '</td>' +
                        '<td>' + option.answers_type + '</td>' +
                        '<td><button type="button" onclick="EditModel(' + option.id + ')" class="btn first-btn"><i class="bx bxs-pencil"></i></button></td>' +
                        '</tr>'
                    );

                });

                $('#tbl_cwmform').DataTable();

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
function GetCategories() {
    new APICALL(GetGlobalURL('Base', 'GetCategories_CWM'), 'GET', '', true).FETCH((result, error) => {
        if (result) {
            if (result.data != null && result.data.length > 0) {
                $('#ddl_category').empty(); // Clear existing options
                $('#ddl_category_form').empty(); // Clear existing options
                $('#ddl_category').append('<option selected="true" value="0"> Select Category</option>'); // Add default option
                $('#ddl_category_form').append('<option selected="true" value="0"> Select Category</option>'); // Add default option

                $.each(result.data, function (i, option) {
                    $('#ddl_category_form').append(
                        `<option value="${option.id}"> ${option.category_type}</option>`
                    );

                    $('#ddl_category').append(
                        `<option value="${option.id}"> ${option.category_type}</option>`
                    );
                });

                GETCWMFORMS();
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
function UploadDocument() {
    var formData = new FormData();
    var files = $('#uploader')[0].files;

    if (files.length > 0) {
        if ($('#ddl_category').val() > 0) {
            formData.append('files', files[0]);

            new APICALL(GetGlobalURL('FileUpload', 'UploadModelDocument'), 'POST', formData, true, true).FETCH((result, error) => {
                if (result) {
                    filespath = result.data;

                    var Model = JSON.stringify({
                        category_type: $('#ddl_category').val(),
                        FilePath: filespath[0]
                    });

                    new APICALL(GetGlobalServiceURL('Base', 'UploadImage'), 'POST', Model, true).FETCH((success, error) => {
                        {
                            if (success) {
                                ClearFields();
                                Swal.fire({
                                    icon: 'success',
                                    title: 'Success...',
                                    text: 'File uploaded successfully!',
                                    footer: ''
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
                        }
                    });
                }

                if (error) {
                    Swal.fire({
                        icon: 'warning',
                        title: 'Warning...',
                        text: error.data.responseText,
                    });
                }

            });

        }
        else {
            Swal.fire({
                icon: 'warning',
                title: 'Warning...',
                text: 'Please select category first!',
                footer: ''
            });

        }

    }
    else {

        Swal.fire({
            icon: 'warning',
            title: 'Warning...',
            text: 'Please select a file first!',
            footer: ''
        });

        return false;
    }
}
function ClearFields() {
    $('#ddl_category').val('0');
    $('#ddl_category_form').val('0');
    $('#uploader').val('');
    $('#cwmform').find('input[type="checkbox"],input[type="radio"],input[type="file"],input[type="text"], textarea').val('');
    $('#cwmform').find('input[type="file"],input[type="text"] ,input[type="radio"],select,textarea, input[type="checkbox"]').prop('disabled', false);
    filespath = [];
    $('#btn_update').hide().prop('disabled', true);
    $('#btn_save').show().prop('disabled', false);

    $('#chk_evaluation').prop('checked', false);
    $('#chk_yes').prop('checked', false);
    $('#chk_feed').prop('checked', false);
    $('#chk_remarks').prop('checked', false);
    $('#chk_status').prop('checked', false);

    $('#image_ck_6').prop('checked', false);
    $('#audio_ck_6').prop('checked', false);
    $('#video_ck_6').prop('checked', false);
    $('#contact_no_ck_6').prop('checked', false);

    $('#correctanswer_yes').hide();
    $('#correctanswer_feedback').hide();
}
function InsertModel() {
    var answer_type = [];
    var attachment_type = [];
    var correctAnswer_yes = "";
    var correctAnswer_feed = "";

    var category_type = $('#ddl_category_form').val();
    var companystandard = $('#txt_companystandard').val();
    var standard = $('#txt_standard').val();
    var guideline = $('#guideline').val();

    if (category_type == 0) {
        Swal.fire({
            icon: 'warning',
            title: 'Warning...',
            text: 'Please select category type, It is a required field!',
            footer: ''
        });
        return false;
    }

    if (companystandard == '' || companystandard == null) {
        Swal.fire({
            icon: 'warning',
            title: 'Warning...',
            text: 'Please enter company standard, It is a required field!',
            footer: ''
        });
        return false;
    }

    if (standard == '' || standard == null) {
        Swal.fire({
            icon: 'warning',
            title: 'Warning...',
            text: 'Please enter company standard, It is a required field!',
            footer: ''
        });
        return false;
    }

    const checkbox_ev = document.querySelector(`[asnwertype-name="Evaluation"]`);
    const checkbox_yes = document.querySelector(`[asnwertype-name="Yes/No"]`);
    const checkbox_feed = document.querySelector(`[asnwertype-name="Feedback"]`);
    const checkbox_remarks = document.querySelector(`[checkbox="remarks"]`);
    const checkbox_status = document.querySelector(`[checkbox="status"]`);
    const checkboxes = document.querySelectorAll('.attachementdiv input[type="checkbox"]');

    if (!checkbox_ev.checked && !checkbox_yes.checked && !checkbox_feed.checked) {
        Swal.fire({
            icon: 'warning',
            title: 'Warning...',
            text: 'Please select atleast one answer type, It is a required field!',
            footer: ''
        });
        return false;
    }

    if (checkbox_ev.checked) {
        answer_type.push(1);
    }
    if (checkbox_yes.checked) {
        const radioButtons = document.getElementsByName('correct_answer_yes');

        for (let i = 0; i < radioButtons.length; i++) {
            if (radioButtons[i].checked) {
                // If a radio button is checked, set its value to selectedValue
                correctAnswer_yes = radioButtons[i].value;
                break; // Exit the loop since we found the checked radio button
            }
        }

        answer_type.push(2);
    }
    if (checkbox_feed.checked) {
        const radioButtons = document.getElementsByName('correct_answer_feed');

        for (let i = 0; i < radioButtons.length; i++) {
            if (radioButtons[i].checked) {
                // If a radio button is checked, set its value to selectedValue
                correctAnswer_feed = radioButtons[i].value;
                break; // Exit the loop since we found the checked radio button
            }
        }
        answer_type.push(3);
    }

    checkboxes.forEach(checkbox => {
        if (checkbox.checked) {
            // If a checkbox is checked, add its value to the array
            attachment_type.push(checkbox.value);
        }
    });

    var answertypes = answer_type.join(',');
    var remarks = checkbox_remarks.checked;
    var attachments = attachment_type.join(',');
    var c_yes = correctAnswer_yes;
    var c_fed = correctAnswer_feed;

    var Model = JSON.stringify({
        category_type: category_type,
        companystandard: companystandard,
        standard: standard,
        answertypes: answertypes,
        remarks: remarks,
        guideline: guideline,
        attachments: attachments,
        c_yes: c_yes,
        c_fed: c_fed,
        correctAnswer_yes: correctAnswer_yes,
        correctAnswer_feed: correctAnswer_feed,
        status: checkbox_status.checked
    });

    new APICALL(GetGlobalURL('Base', 'InsertCWMForm'), 'POST', Model, true).FETCH((result, error) => {
        if (result) {
            //ResetControls();
            Swal.fire({
                icon: 'success',
                title: 'Success...',
                text: 'Model Inserted Successfully!',
                footer: ''
            });
            ClearFields();
            //GETKPIFORMS();
        }
        else {
            Swal.fire({
                icon: 'error',
                title: 'Error...',
                text: 'Unable to Model Board',
                footer: ''
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
function EditModel(modelid) {
    new APICALL(GetGlobalURL('Base', 'GetCWMModel?modelid=' + modelid), 'GET', '', true).FETCH((result, error) => {
        if (result) {
            if (result.data != null && result.data.length > 0) {
                $('#hd_modelid').val(modelid);
                $('#hd_formid').val(result.data[0].id);

                $('#ddl_category_form').val(result.data[0].cateogrytype_id).prop('disabled', true);
                $('#txt_companystandard').val(result.data[0].company_standard).prop('disabled', true);
                $('#txt_standard').val(result.data[0].standard).prop('disabled', true);
                $('#guideline').val(result.data[0].guideline);
                result.data[0].remarks == true ? $('#chk_remarks').prop('checked', true) : $('#chk_remarks').prop('checked', false);
                result.data[0].isactive == true ? $('#chk_status').prop('checked', true) : $('#chk_status').prop('checked', false);

                var answers = JSON.parse(result.data[0].Answer);
                var attachments = JSON.parse(result.data[0].Attachment_types);

                //$('#btn_update').show().prop('disabled', false);
                //$('#btn_save').hide().prop('disabled', true);

                $('#btn_update').removeClass('d-none').show().prop('disabled', false);
                $('#btn_save').addClass('d-none').hide().prop('disabled', true);

                $.each(answers, function (i, option) {
                    if (option.answer_type == 1) {
                        $('#chk_evaluation').prop('checked', true);
                    }

                    if (option.answer_type == 2) {
                        $('#chk_yes').prop('checked', true);
                        $('#correctanswer_yes').show();

                        $('#c_yes').prop('checked', false);
                        $('#c_no').prop('checked', false);

                        if (option.correct_answer = 1) {
                            $('#c_yes').prop('checked', true);
                        }
                        else {
                            $('#c_no').prop('checked', true);

                        }

                    }

                    if (option.answer_type == 3) {
                        $('#chk_feed').prop('checked', true);

                        $('#correctanswer_feedback').show();

                        $('#c_good').prop('checked', false);
                        $('#c_fair').prop('checked', false);
                        $('#c_bad').prop('checked', false);

                        if (option.correct_answer = 1) {
                            $('#c_good').prop('checked', true);
                        }
                        else if (option.correct_answer = 2) {
                            $('#c_fair').prop('checked', false);

                        }
                        else {
                            $('#c_no').prop('checked', true);

                        }
                    }

                });

                if (attachments != null) {
                    $.each(attachments, function (i, option) {
                        if (option.attachment_type == 1) {
                            $('#image_ck_6').prop('checked', true);
                        }
                        if (option.attachment_type == 2) {
                            $('#audio_ck_6').prop('checked', true);
                        }
                        if (option.attachment_type == 3) {
                            $('#video_ck_6').prop('checked', true);
                        }
                        if (option.attachment_type == 4) {
                            $('#contact_no_ck_6').prop('checked', true);
                        }

                    });

                }

                $('#cwmformmodel').find('input').not('#chk_status').prop('disabled', true);
                $('#cwmformmodel').find('select, textarea').prop('disabled', true);

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
function UpdateModel() {
    var answer_type = [];
    var attachment_type = [];
    var correctAnswer_yes = "";
    var correctAnswer_feed = "";

    var category_type = $('#ddl_category_form').val();
    var companystandard = $('#txt_companystandard').val();
    var standard = $('#txt_standard').val();
    var guideline = $('#guideline').val();

    if (category_type == 0) {
        Swal.fire({
            icon: 'warning',
            title: 'Warning...',
            text: 'Please select category type, It is a required field!',
            footer: ''
        });
        return false;
    }

    if (companystandard == '' || companystandard == null) {
        Swal.fire({
            icon: 'warning',
            title: 'Warning...',
            text: 'Please enter company standard, It is a required field!',
            footer: ''
        });
        return false;
    }

    if (standard == '' || standard == null) {
        Swal.fire({
            icon: 'warning',
            title: 'Warning...',
            text: 'Please enter company standard, It is a required field!',
            footer: ''
        });
        return false;
    }

    const checkbox_ev = document.querySelector(`[asnwertype-name="Evaluation"]`);
    const checkbox_yes = document.querySelector(`[asnwertype-name="Yes/No"]`);
    const checkbox_feed = document.querySelector(`[asnwertype-name="Feedback"]`);
    const checkbox_remarks = document.querySelector(`[checkbox="remarks"]`);
    const checkbox_status = document.querySelector(`[checkbox="status"]`);
    const checkboxes = document.querySelectorAll('.attachementdiv input[type="checkbox"]');

    if (!checkbox_ev.checked && !checkbox_yes.checked && !checkbox_feed.checked) {
        Swal.fire({
            icon: 'warning',
            title: 'Warning...',
            text: 'Please select atleast one answer type, It is a required field!',
            footer: ''
        });
        return false;
    }

    if (checkbox_ev.checked) {
        answer_type.push(1);
    }
    if (checkbox_yes.checked) {
        const radioButtons = document.getElementsByName('correct_answer_yes');

        for (let i = 0; i < radioButtons.length; i++) {
            if (radioButtons[i].checked) {
                // If a radio button is checked, set its value to selectedValue
                correctAnswer_yes = radioButtons[i].value;
                break; // Exit the loop since we found the checked radio button
            }
        }
        answer_type.push(2);
    }
    if (checkbox_feed.checked) {
        const radioButtons = document.getElementsByName('correct_answer_feed');

        for (let i = 0; i < radioButtons.length; i++) {
            if (radioButtons[i].checked) {
                // If a radio button is checked, set its value to selectedValue
                correctAnswer_feed = radioButtons[i].value;
                break; // Exit the loop since we found the checked radio button
            }
        }

        answer_type.push(3);
    }

    checkboxes.forEach(checkbox => {
        if (checkbox.checked) {
            // If a checkbox is checked, add its value to the array
            attachment_type.push(checkbox.value);
        }
    });


    var answertypes = answer_type.join(',');
    var remarks = checkbox_remarks.checked;
    var attachments = attachment_type.join(',');
    var c_yes = correctAnswer_yes;
    var c_fed = correctAnswer_feed;

    var Model = JSON.stringify({
        form_id: $('#hd_formid').val(),
        model_id: $('#hd_modelid').val(),
        category_type: category_type,
        companystandard: companystandard,
        standard: standard,
        answertypes: answertypes,
        remarks: remarks,
        guideline: guideline,
        attachments: attachments,
        c_yes: c_yes,
        c_fed: c_fed,
        correctAnswer_yes: correctAnswer_yes,
        correctAnswer_feed: correctAnswer_feed,
        status: checkbox_status.checked
    });


    new APICALL(GetGlobalURL('Base', 'UpdateCWMForm'), 'POST', Model, true).FETCH((result, error) => {
        if (result) {
            //ResetControls();
            Swal.fire({
                icon: 'success',
                title: 'Success...',
                text: 'Model Updated Successfully!',
                footer: ''
            });

            ClearFields();
            //GETKPIFORMS();
        }
        else {
            Swal.fire({
                icon: 'error',
                title: 'Error...',
                text: 'Unable to Update Model',
                footer: ''
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
