var table;

$(document).ready(function () {

    toggleDynamicType();
    GetWorkflowStepAction();
    GetDynamicFunction();

    $("#workflowstepsActionform").validate();
    $('#SaveBtn').on('click', function () {
        SaveWorkflowStepAction();
    });

    $('#UpdateBtn').on('click', function () {
        UpdateWorkflowStepAction();
    });
    $('input[name="assignmentType"]').on('change', function () {
        toggleDynamicType();
    });
  
    GetWorkflowCode();
});


function GetWorkflowStep(workflowid) {
    ShowLoader('workflowstepsActionDiv');
    new APICALL(GetGlobalURL('Base', 'GetWorkflowStepbyWorkflowId?workflowid=' + workflowid), 'GET', '', false).FETCH((result, error) => {

        if (result) {
            $.each(result.data, function (i, option)
            {
                $('#s-workflownextstepid').append(
                    '<option value="' + option.id + '" required>' + option.workflowstep + '</option>'
                );
            });

            $.each(result.data, function (i, option) {
                $('#s-workflowstepid').append(
                    '<option value="' + option.id + '" required>' + option.workflowstep + '</option>'
                );
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
function GetWorkflowStepAction() {
    ShowLoader('workflowstepsActionDiv');

    UTILITY.CheckSession((data_) => {

        if (data_) {

            new APICALL(GetGlobalURL('Base', 'GetWorkflowStepAction'), 'GET', '', true).FETCH((result, error) => {

                if (result) {


                    if (table) {
                        table.clear();
                        table.destroy();

                    }

                    if (result.data != null) {
                        roles = result.data;
                        $.each(result.data, function (i, option) {
                            var ActiveCell = option.isactive == true ? '<td><input type="checkbox" class="role-active checkboxsize" disabled checked="checked"></td>' : '<td><input type="checkbox" class="role-active checkboxsize" disabled></td>'
                            var IsSaveCell = option.issave == true ? '<td><input type="checkbox" class="role-active checkboxsize" disabled checked="checked"></td>' : '<td><input type="checkbox" class="role-active checkboxsize" disabled></td>'
                            var IsMoveCell = option.ismove == true ? '<td><input type="checkbox" class="role-active checkboxsize" disabled checked="checked"></td>' : '<td><input type="checkbox" class="role-active checkboxsize" disabled></td>'
                            var UpdateBtn = (data_[0].AllowUpdate == true) ? '<td><button class="btn btn-sm btn-white btn-block EditWorkflowStepAction" data-value="' + option.id + '" type="button"><i class="bx bxs-edit"></i></button></td>' : '';
                            var DeleteBtn = (data_[0].AllowDelete == true) ? '<td><button class="btn btn-sm btn-white btn-block DeleteWorkflowStepAction" data-value="' + option.id + '" type="button"><i class="bx bxs-trash"></i></button></td>' : '';

                            $('#workflowstepsaction-master tbody').append(
                                '<tr id="rowid-' + option.id + '">' +
                                '<td>' + option.id + '</td>' +
                                '<td>' + option.actionname + '</td>' +
                                '<td>' + option.workflowstep + '</td>' +
                                '<td>' + option.NEXTSTEP + '</td>' +
                                '<td>' + formatDateTime(option.createddatetime) + '</td>' +
                                '<td>' + option.createdby + '</td>' +
                                IsSaveCell +
                                IsMoveCell +
                                ActiveCell +
                                UpdateBtn +
                                DeleteBtn +
                                '</tr>'
                            );
                        });
                        $('.EditWorkflowStepAction').on('click', function () {
                            EditWorkflowStepAction(this.attributes["data-value"].value);
                        });
                        $('.DeleteWorkflowStepAction').on('click', function () {
                            DeleteWorkflowStepAction(this.attributes["data-value"].value);
                        });


                    }

                    table = $('#workflowstepsaction-master').DataTable({
                        columnDefs: [
                            { targets: 0, visible: false },
                        ],
                        select: true
                    });

                    table.draw();


                    HideLoader('workflowstepsActionDiv');
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
function EditWorkflowStepAction(actionid) {
    new APICALL(GetGlobalURL('Base', 'GetWorkflowStepsActionByID?workflowstepid=' + actionid), 'GET', '', true).FETCH((result, error) => {

        if (result) {

            if (result.data.length > 0) {
                console.log(result.data);
                debugger;
                var id = result.data[0].id;
                var actionname = result.data[0].actionname;
                var stepid = result.data[0].stepid;
                var nextstepid = result.data[0].nextstepid;
                var isactive = result.data[0].isactive;
                var ismove = result.data[0].ismove;
                var issave = result.data[0].issave;
                var workflowid = result.data[0].workflowid
                var assignmenttype = result.data[0].assignmenttype
                var dynamicfunction = result.data[0].dynamicfunction
                var actiontype = result.data[0].actiontype

                

                $('#Workflowcode').val(workflowid).trigger('change');
                $('#s-workflowstepActionid').val(id);
                $('#s-workflowstepaction').val(actionname);
                $('#s-workflowstepid').val(stepid);
                $('#s-workflownextstepid').val(nextstepid);
                $('#s-active').prop('checked', isactive);
                $('#ismove').prop('checked', ismove);
                $('#issave').prop('checked', issave);
                $('#actiontype').val(actiontype);
                $('input[name="assignmentType"][value="' + assignmenttype + '"]').prop('checked', true).trigger('change');
                $('#nexttype').val(dynamicfunction);

               

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
function UpdateWorkflowStepAction() {
    if (!$('#workflowstepsActionform').valid()) {
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
            ShowLoader('workflowstepsActionDiv');

            $('#workflowstepsActionform #s-active').val($('#workflowstepsActionform #s-active').prop('checked'));

            var Data = $('#workflowstepsActionform').serialize();

            new APICALL(GetGlobalURL('Base', 'UpdateWorkflowStepAction'), 'POST', Data, true, false, 'application/x-www-form-urlencoded').FETCH((result, error) => {

                if (result) {
                    GetWorkflowStepAction();
                    ResetControls();


                    Swal.fire({
                        icon: 'success',
                        title: 'Success...',
                        text: 'Action Updated Successfully!',
                        footer: ''
                    });

                    HideLoader('workflowstepsActionDiv');
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
function SaveWorkflowStepAction() {
    if (!$('#workflowstepsActionform').valid()) {
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
            ShowLoader('workflowstepsActionDiv');

            $('#workflowstepsActionform #s-active').val($('#workflowstepsActionform #s-active').prop('checked'));
            $('#workflowstepsActionform #ismove').val($('#workflowstepsActionform #ismove').prop('checked'));
            $('#workflowstepsActionform #issave').val($('#workflowstepsActionform #issave').prop('checked'));

            var Data = $('#workflowstepsActionform').serialize();

            new APICALL(GetGlobalURL('Base', 'SaveWorkflowStepAction'), 'POST', Data, true, false, 'application/x-www-form-urlencoded').FETCH((result, error) => {

                if (result) {
                    GetWorkflowStepAction();
                    ResetControls();

                    Swal.fire({
                        icon: 'success',
                        title: 'Success...',
                        text: 'Action Saved Successfully!',
                        footer: ''
                    });

                    HideLoader('workflowstepsActionDiv');
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

function DeleteWorkflowStepAction(workflowstepid) {

    var DeleteData = JSON.stringify({
        id: workflowstepid
    });
    Swal.fire({
        title: 'Do you want to save the changes?',
        showDenyButton: true,
        showCancelButton: false,
        confirmButtonText: 'Ok',
        denyButtonText: 'Cancel',
    }).then((result) => {
        if (result.isConfirmed) {
            ShowLoader('workflowstepsActionDiv');
            new APICALL(GetGlobalURL('Base', 'DeleteWorkflowStepAction'), 'POST', DeleteData, true).FETCH((result, error) => {

                if (result) {
                    GetWorkflowStepAction();

                    Swal.fire({
                        icon: 'success',
                        title: 'Success...',
                        text: 'Action Deleted Successfully!',
                        footer: ''
                    });

                    HideLoader('workflowstepsActionDiv');
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

    $('#s-workflowstepActionid').val('');
    $('#s-workflowstepaction').val('');
    $('#s-workflowstepid').val('');
    $('#s-workflownextstepid').val('');
    
    $('#s-active').prop('checked', false)
    $('#AllowInsert').prop('checked', false);
    $('#AllowUpdate').prop('checked', false);
    $('#AllowDelete').prop('checked', false);
    $('.savebutton').removeClass('d-none');
    $('.updatebutton').addClass('d-none');
    $('#issave').prop('checked', false)
    $('#Workflowcode').val('');
    $('#nexttype').val('');
    $('#staticRole').prop('checked', true);
    toggleDynamicType();
}

function GetWorkflowCode() {
    ShowLoader('workflowstepsActionDiv');

    
            new APICALL(GetGlobalURL('Base', 'GetWorkflowCode'), 'GET', '', true).FETCH((result, error) => {

                if (result) {
                   
                    $.each(result.data, function (i, option) {
                        $('#Workflowcode').append(
                            '<option value="' + option.id + '" required>' + option.workflowcode + '</option>'
                        );
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


$('#Workflowcode').on('change', function () {
    var selectedValue = $(this).val();
    $('#s-workflowstepid').empty();
    $('#s-workflowstepid').append('<option value="">Select Workflow Step</option>');
    $('#s-workflownextstepid').empty();
    $('#s-workflownextstepid').append('<option value="">Select Next Step</option>');
    GetWorkflowStep(selectedValue);
    
    if (selectedValue !== '') {
        $('#s-workflowstepid, #s-workflownextstepid').removeAttr('disabled');
       
    } else {
        $('#s-workflowstepid, #s-workflownextstepid').attr('disabled', true);
       
    }
});



function toggleDynamicType() {
    const dynamicRadio = document.getElementById('dynamicRole');
    const nextTypeDropdown = document.getElementById('nexttype').parentElement;

    if (dynamicRadio.checked) {
        nextTypeDropdown.style.display = 'block';
    } else {
        nextTypeDropdown.style.display = 'none';
    }
}


function GetDynamicFunction() {
    $('#nexttype').empty();
    $('#nexttype').append('<option value="">Select Dynamic Type</option>');
    new APICALL(GetGlobalURL('Base', 'GetDynamicFunction'), 'GET', '', false).FETCH((result, error) => {

        if (result) {
            $.each(result.data, function (i, option) {
                
                $('#nexttype').append(
                    '<option value="' + option.name + '" required>' + option.displayname + '</option>'
                );
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