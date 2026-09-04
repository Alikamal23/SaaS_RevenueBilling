var roles = {};

$(document).ready(function () {

    GetWorkflow();
    $("#workflowform").validate();
    $('#SaveBtn').on('click', function () {
        SaveWorkflow();
    });

    $('#UpdateBtn').on('click', function () {
        UpdateWorkflow();
    });

});

function GetWorkflow() {
    ShowLoader('workflowDiv');

    UTILITY.CheckSession((data_) => {

        if (data_) {

            new APICALL(GetGlobalURL('Base', 'GetWorkflow'), 'GET', '', true).FETCH((result, error) => {

                if (result) {

                    $('#workflow-master tbody').html('');
                    if (result.data != null) {
                        roles = result.data;
                        $.each(result.data, function (i, option) {
                            var ActiveCell = option.isactive == true ? '<td><input type="checkbox" class="role-active checkboxsize" disabled checked="checked"></td>' : '<td><input type="checkbox" class="role-active checkboxsize" disabled></td>'
                            var UpdateBtn = (data_[0].AllowUpdate == true) ? '<td><button class="avatar-text avatar-md EditWorkflow" data-value="' + option.id + '" type="button" data-bs-toggle="tooltip" data-bs-placement="bottom" title="" data-bs-original-title="Edit"><i class="feather-edit-2 edit-icon"></i></button></td>' : '';
                         
                            $('#workflow-master tbody').append(
                                '<tr id="rowid-' + i + '">' +
                                '<td>' + (option.workflowcode ?? '') + '</td>' +
                                '<td>' + (option.workflowname ?? '') + '</td>' +
                                '<td>' + formatDateTime(option.createddatetime ?? '') + '</td>' +
                                '<td>' + (option.createdby ?? '') + '</td>' +
                                '<td>' + formatDateTime(option.editdatetime ?? '') + '</td>' +
                                '<td>' + (option.editdby ?? '') + '</td>' +
                                ActiveCell +
                                UpdateBtn +
                                '</tr>'
                            );
                        });
                        $('.EditWorkflow').on('click', function () {
                            EditWorkflow(this.attributes["data-value"].value);
                        });


                    }
                    $('#workflow-master').DataTable();
                    HideLoader('workflowDiv');
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

function EditWorkflow(workflowid) {

    new APICALL(GetGlobalURL('Base', 'GetWorkflowByID?workflowid=' + workflowid), 'GET', '', true).FETCH((result, error) => {

        if (result) {

            if (result.data.length > 0) {

                
                var workflowname = result.data[0].workflowname;
                var workflowcode = result.data[0].workflowcode;
                var formpageurl = result.data[0].mainformurl;
                var viewpageurl = result.data[0].viewpageurl;
                var taskpageurl = result.data[0].mytaskpageurl;
                var requestpageurl = result.data[0].myrequestpageurl;
                var isactive = result.data[0].isactive;
               

                $('#s-workflowname').val(workflowname);
                $('#s-active').prop('checked', isactive);
                $('#s-workflowid').val(workflowid);
                $('#s-workflowcode').val(workflowcode);
                $('#formpageurl').val(formpageurl);
                $('#viewpageurl').val(viewpageurl);
                $('#taskpageurl').val(taskpageurl);
                $('#requestpageurl').val(requestpageurl);

                if ($('.updatebutton').hasClass('d-none')) {
                    $('.savebutton').toggleClass('d-none');
                    $('.updatebutton').toggleClass('d-none');
                }
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

function UpdateWorkflow() {
    if (!$('#workflowform').valid()) {
        return false;
    }
    Swal.fire({
        title: 'Do you want to save the changes?',
        showDenyButton: true,
        showCancelButton: false,
        confirmButtonText: 'Ok',
        denyButtonText: 'Cancel',
    }).then((result) => {
        if (result.isConfirmed) {
            ShowLoader('workflowDiv');

            $('#workflowform #s-active').val($('#workflowform #s-active').prop('checked'));

            var Data = $('#workflowform').serialize();

            new APICALL(GetGlobalURL('Base', 'EditWorkflow'), 'POST', Data, true, false, 'application/x-www-form-urlencoded').FETCH((result, error) => {

                if (result) {
                    GetWorkflow();
                    ResetControls();


                    Swal.fire({
                        icon: 'success',
                        title: 'Success...',
                        text: 'Workflow Updated Successfully!',
                        footer: ''
                    });

                    HideLoader('workflowDiv');
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

function SaveWorkflow() {
    if (!$('#workflowform').valid()) {
        return false;
    }
    Swal.fire({
        title: 'Do you want to save the changes?',
        showDenyButton: true,
        showCancelButton: false,
        confirmButtonText: 'Ok',
        denyButtonText: 'Cancel',
    }).then((result) => {
        if (result.isConfirmed) {
            ShowLoader('workflowDiv');

            $('#workflowform #s-active').val($('#workflowform #s-active').prop('checked'));

            var Data = $('#workflowform').serialize();

            new APICALL(GetGlobalURL('Base', 'SaveWorkflow'), 'POST', Data, true, false, 'application/x-www-form-urlencoded').FETCH((result, error) => {

                if (result) {
                    GetWorkflow();
                    ResetControls();

                    Swal.fire({
                        icon: 'success',
                        title: 'Success...',
                        text: 'Workflow Saved Successfully!',
                        footer: ''
                    });

                    HideLoader('workflowDiv');
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

function ResetControls() {

    $('#s-workflowname').val('');
    $('#s-active').prop('checked', false)
    $('#AllowInsert').prop('checked', false);
    $('#AllowUpdate').prop('checked', false);
    $('#AllowDelete').prop('checked', false);
    $('#s-workflowid').val('');
    $('.savebutton').removeClass('d-none');
    $('.updatebutton').addClass('d-none');
    $('#s-workflowcode').val('');
    $('#formpageurl').val('');
    $('#viewpageurl').val('');
    $('#taskpageurl').val('');
    $('#requestpageurl').val('');

}


function formatDateTime(dateStr) {
    if (!dateStr) return '';

    const date = new Date(dateStr);

    const day = String(date.getDate()).padStart(2, '0');
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const year = date.getFullYear();

    const hours = String(date.getHours()).padStart(2, '0');
    const minutes = String(date.getMinutes()).padStart(2, '0');

    return `${day}-${month}-${year} ${hours}:${minutes}`;
}