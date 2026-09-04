$(document).ready(function () {
    Clearfield();
    GetDealerTypes();

    $('#correctanswer_feedback').hide();
    $('#correctanswer_yes').hide();

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

    $('#btn_save').click(Insert_Question);
    $('#btn_update').click(UpdateQuestion);
    $('#btn_update').hide().prop('disabled', true);
    $('#btn_cancel').click(Clearfield);


    $('#tbl_kpiform').on('click', '.btn.first-btn', function () {
        var questionid = $(this).closest('tr').find('td:first').text();
        //alert('hello: ' + questionid);
        EditQuestion(questionid);
    });

});

function GETKPIFORMS() {
    new APICALL(GetGlobalURL('Base', 'GetKPIFoms'), 'GET', '', true).FETCH((result, error) => {
        if (result) {
            if (result.data != null && result.data.length > 0) {
                $('#tbl_kpiform').DataTable().destroy();
                $('#tbl_kpiform tbody').empty();
                var sr = 0;
                $.each(result.data, function (i, option) {
                    sr++;
                    $('#tbl_kpiform tbody').append(
                        '<tr>' +
                        '<td>' + sr + '</td>' +
                        '<td>' + option.dealer_type + '</td>' +
                        '<td>' + option.question_type + '</td>' +
                        '<td>' + option.question + '</td>' +
                        '<td>' + option.guideline + '</td>' +
                        '<td>' + option.answers_type + '</td>' +
                        '<td><button type="button" onclick="EditQuestion(' + option.id + ')" class="btn first-btn"><i class="bx bxs-pencil"></i></button></td>' +

                        '</tr>'
                    );

                });

                $('#tbl_kpiform').DataTable();
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
function GetDealerTypes() {
    new APICALL(GetGlobalURL('Base', 'GetDealerTypes'), 'GET', '', true).FETCH((result, error) => {
        if (result) {
            if (result.data != null && result.data.length > 0) {
                $('#ddl_dealertype').empty(); // Clear existing options
                $('#ddl_dealertype').append('<option selected="true" value="0"> Select Dealer Type</option>'); // Add default option

                $.each(result.data, function (i, option) {
                    $('#ddl_dealertype').append(
                        `<option value="${option.id}"> ${option.dealer_type}</option>`
                    );
                });

                GetQuestionTypes();
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
function GetQuestionTypes() {
    new APICALL(GetGlobalURL('Base', 'GetQuestionTypes'), 'GET', '', true).FETCH((result, error) => {
        if (result) {
            if (result.data != null && result.data.length > 0) {
                $('#ddl_questiontype').empty(); // Clear existing options
                $('#ddl_questiontype').append('<option selected="true" value="0"> Select Question Type</option>'); // Add default option

                $.each(result.data, function (i, option) {
                    $('#ddl_questiontype').append(
                        `<option value="${option.id}"> ${option.question_type}</option>`
                    );
                });

                GETKPIFORMS();
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
function GetAnswerTypes() {
    new APICALL(GetGlobalURL('Base', 'GetAnswerTypes'), 'GET', '', true).FETCH((result, error) => {
        if (result) {
            if (result.data != null) {
                $('#questiondiv').append('<div id="main-container" class="col-md-6 mt-2 answer_types"> <label for="answer" class="form-label w-100">Answer Type:</label > ');

                var html = '';
                $.each(result.data, function (i, option) {
                    html += '<div class="form-check form-check-inline">';
                    html += '<input class="form-check-input asnwertype-name="' + option.answers_type + '"   asnwertype-Id=' + option.id + '" type="checkbox"  value="' + option.id + '">';
                    html += '<label class="form-check-label" for="rating_check_2">' + option.answers_type + '</label></div>';
                });

                // Append the generated HTML to the main container
                $('#main-container').append(html);

                // Close the main container div
                $('#questiondiv').append('</div>');
            }
        }
    });
}
function Insert_Question() {
    const checkedAnswerTypes = [];

    var answer_type = [];
    var attachment_type = [];

    var correctAnswer_yes = "";
    var correctAnswer_feed = "";

    var dealer_type = $('#ddl_dealertype').val();
    var question_type = $('#ddl_questiontype').val();
    var question = $('#txtquestion').val();
    var guideline = $('#guideline').val();


    if (dealer_type == 0) {
        Swal.fire({
            icon: 'warning',
            title: 'Warning...',
            text: 'Please select dealer type, It is a required field!',
            footer: ''
        });
        return false;
    }

    if (question_type == 0) {
        Swal.fire({
            icon: 'warning',
            title: 'Warning...',
            text: 'Please select question type, It is a required field!',
            footer: ''
        });
        return false;
    }

    if (question == '' || question == null) {
        Swal.fire({
            icon: 'warning',
            title: 'Warning...',
            text: 'Please enter question, It is a required field!',
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
        dealer_type: dealer_type,
        question_type: question_type,
        question: question,
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


    new APICALL(GetGlobalURL('Base', 'InsertKPIForm'), 'POST', Model, true).FETCH((result, error) => {
        if (result) {
            //ResetControls();
            Swal.fire({
                icon: 'success',
                title: 'Success...',
                text: 'Question Inserted Successfully!',
                footer: ''
            });

            Clearfield();
            GETKPIFORMS();
        }
        else {
            Swal.fire({
                icon: 'error',
                title: 'Error...',
                text: 'Unable to Insert Question',
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
function Clearfield() {
    $('#kpiform').find('input:not([type="checkbox"]):not([type="radio"]), select, textarea').val('');
    $('#kpiform').find('input[type="file"],input[type="text"] ,input[type="radio"],select,textarea, input[type="checkbox"]').prop('disabled', false);

    $('#btn_update').hide().prop('disabled', true);
    $('#btn_save').show().prop('disabled', false);

    $('#chk_evaluation').prop('checked', false);
    $('#chk_yes').prop('checked', false);
    $('#chk_feed').prop('checked', false);

    $('#image_ck_6').prop('checked', false);
    $('#audio_ck_6').prop('checked', false);
    $('#video_ck_6').prop('checked', false);
    $('#contact_no_ck_6').prop('checked', false);

    $('#correctanswer_yes').hide();
    $('#correctanswer_feedback').hide();
}
function EditQuestion(questionid) {
    new APICALL(GetGlobalURL('Base', 'GetKPIQuestion?questionid=' + questionid), 'GET', '', true).FETCH((result, error) => {
        if (result) {
            if (result.data != null && result.data.length > 0) {
                Clearfield();

                $('#hd_questionid').val(questionid);
                $('#hd_formid').val(result.data[0].id);

                $('#ddl_dealertype').val(result.data[0].dealer_typeid).prop('disabled', true);
                $('#ddl_questiontype').val(result.data[0].questiontype_id).prop('disabled', true);
                $('#txtquestion').val(result.data[0].question).prop('disabled', true);
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

                $('#kpiform').find('input').not('#chk_status').prop('disabled', true);
                $('#kpiform').find('select, textarea').prop('disabled', true);

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
function UpdateQuestion() {
    const checkedAnswerTypes = [];

    var answer_type = [];
    var attachment_type = [];

    var correctAnswer_yes = "";
    var correctAnswer_feed = "";

    var dealer_type = $('#ddl_dealertype').val();
    var question_type = $('#ddl_questiontype').val();
    var question = $('#txtquestion').val();
    var guideline = $('#guideline').val();
    var question_id = $('#hd_questionid').val();
    var form_id = $('#hd_formid').val();


    if (dealer_type == 0) {
        Swal.fire({
            icon: 'warning',
            title: 'Warning...',
            text: 'Please select dealer type, It is a required field!',
            footer: ''
        });

        return false;

    }

    if (question_type == 0) {
        Swal.fire({
            icon: 'warning',
            title: 'Warning...',
            text: 'Please select question type, It is a required field!',
            footer: ''
        });

        return false;

    }

    if (question == '' || question == null) {
        Swal.fire({
            icon: 'warning',
            title: 'Warning...',
            text: 'Please enter question, It is a required field!',
            footer: ''
        });

        return false;

    }


    const checkbox_ev = document.querySelector(`[asnwertype-name="Evaluation"]`);
    const checkbox_yes = document.querySelector(`[asnwertype-name="Yes/No"]`);
    const checkbox_feed = document.querySelector(`[asnwertype-name="Feedback"]`);
    const checkbox_remarks = document.querySelector(`[checkbox="remarks"]`);
    const checkbox_status = document.querySelector(`[checkbox="status"]`); // ✅ Add this
    const checkboxes = document.querySelectorAll('.attachementdiv input[type="checkbox"]');

    if (!checkbox_ev.checked && !checkbox_yes.checked && !checkbox_yes.feed) {
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
        form_id: form_id,
        question_id: question_id,
        dealer_type: dealer_type,
        question_type: question_type,
        question: question,
        answertypes: answertypes,
        remarks: checkbox_remarks.checked,
        status: checkbox_status.checked,
        guideline: guideline,
        attachments: attachments,
        c_yes: c_yes,
        c_fed: c_fed,
        correctAnswer_yes: correctAnswer_yes,
        correctAnswer_feed: correctAnswer_feed
    });


    new APICALL(GetGlobalURL('Base', 'UpdateKPIForm'), 'POST', Model, true).FETCH((result, error) => {
        if (result) {
            //ResetControls();
            Swal.fire({
                icon: 'success',
                title: 'Success...',
                text: 'Question Updated Successfully!',
                footer: ''
            });

            Clearfield();
            GETKPIFORMS();
        }
        else {
            Swal.fire({
                icon: 'error',
                title: 'Error...',
                text: 'Unable to Update Question',
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
