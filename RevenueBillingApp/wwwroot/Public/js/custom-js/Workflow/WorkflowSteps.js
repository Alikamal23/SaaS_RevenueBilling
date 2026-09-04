var table;

$(document).ready(function () {

    GetWorkflow();
    GetRoles()
    GetApprovalType();
    GetWorkflowStep();
    $("#workflowstepsform").validate();
    $('#SaveBtn').on('click', function () {
        SaveWorkflowStep();
    });

    $('#UpdateBtn').on('click', function () {
        UpdateWorkflowStep();
    });

});

const modals = document.querySelectorAll('.modal');
modals.forEach(modal => {
    modal.addEventListener('show.bs.modal', (e) => {
        const openedModals = document.querySelectorAll('.modal.show');
        openedModals.forEach(openModal => {
            openModal.style.zIndex = 1040 + (10 * (openedModals.length + 1)); // Adjust stacking
        });
        
    });

    modal.addEventListener('hidden.bs.modal', () => {
        const backdrop = document.querySelector('.modal-backdrop');
        if (document.querySelectorAll('.modal.show').length === 0 && backdrop) {
            backdrop.remove(); // Clean up backdrops if no modals are left open
        }
    });
});



function GetWorkflow() {
    ShowLoader('workflowDiv');

    UTILITY.CheckSession((data_) => {

        if (data_) {

            new APICALL(GetGlobalURL('Base', 'GetWorkflow'), 'GET', '', true).FETCH((result, error) => {

                if (result)
                {
                    $.each(result.data, function (i, option) {
                        $('#s-workflow').append(
                            '<option value="' + option.id + '" required>' + option.workflowname + '</option>'
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
    });
}
function GetRoles() {
    ShowLoader('RolesMasterDiv');

    UTILITY.CheckSession((data_) => {

        if (data_) {

            new APICALL(GetGlobalURL('Base', 'GetRoles'), 'GET', '', true).FETCH((result, error) => {

                if (result) {
                    $('#s-RoleID').append('<option value="">Select Role</option>');
                    $.each(result.data, function (i, option) {
                        $('#s-RoleID').append(
                            '<option value="' + option.RoleID + '">' + option.RoleName + '</option>'
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
    });
}
function GetApprovalType() {
   
    UTILITY.CheckSession((data_) => {

        if (data_) {

            new APICALL(GetGlobalURL('Base', 'GetApprovalType'), 'GET', '', true).FETCH((result, error) => {

                if (result) {
                    $.each(result.data, function (i, option) {
                        $('#s-approvaltypeid').append(
                            '<option value="' + option.id + '" required>' + option.approvaltype + '</option>'
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
    });
}
function GetWorkflowStep() {
    ShowLoader('workflowstepsDiv');

    UTILITY.CheckSession((data_) => {

        if (data_) {

            new APICALL(GetGlobalURL('Base', 'GetWorkflowStep'), 'GET', '', true).FETCH((result, error) => {

                if (result) {


                    if (table) {
                        table.clear();
                        table.destroy();

                    }

                    if (result.data != null) {
                        roles = result.data;
                        $.each(result.data, function (i, option) {
                            var ActiveCell = option.isactive == true ? '<td><input type="checkbox" class="role-active checkboxsize" disabled checked="checked"></td>' : '<td><input type="checkbox" class="role-active checkboxsize" disabled></td>'
                            var UpdateBtn = (data_[0].AllowUpdate == true) ? '<td><button class="btn btn-sm btn-white btn-block EditWorkflowStep" data-value="' + option.id + '" type="button"><i class="bx bxs-edit"></i></button></td>' : '';
                            var DeleteBtn = (data_[0].AllowDelete == true) ? '<td><button class="btn btn-sm btn-white btn-block DeleteWorkflowStep" data-value="' + option.id + '" type="button"><i class="bx bxs-trash"></i></button></td>' : '';
                            var Section = (data_[0].AllowUpdate == true) ? '<td><button data-workflow="' + option.workflowid + '" data-step="' + option.id + '" class="btn btn-sm btn-white btn-block EditSectionPermission" data-bs-toggle="modal" data-bs-target="#sectionPermissionModal" data-value="' + option.id + '" type="button"><i class="bx bxs-edit"></i></button></td>' : '';
                            $('#workflowsteps-master tbody').append(
                                '<tr id="rowid-' + option.id + '">' +
                                '<td>' + option.id + '</td>' +
                                '<td>' + option.sortid + '</td>' +
                                '<td>' + option.workflowname + '</td>' +
                                '<td>' + option.workflowstep + '</td>' +
                                '<td>' + (option.RoleName == null ? "N/A" : option.RoleName) + '</td>'+
                                '<td>' + option.approvaltype + '</td>' +
                                '<td>' + option.sla + '</td>' +
                                '<td>' + formatDateTime(option.createddatetime ?? '') + '</td>' +
                                '<td>' + option.createdby + '</td>' +
                                '<td>' + formatDateTime(option.editdatetime ?? '') + '</td>' +
                                '<td>' + (option.editdby ?? '') + '</td>' +
                                Section +
                                ActiveCell +
                                UpdateBtn +
                                DeleteBtn +
                                '</tr>'
                            );
                        });
                        $('.EditWorkflowStep').on('click', function () {
                            EditWorkflowStep(this.attributes["data-value"].value);
                        });
                        $('.DeleteWorkflowStep').on('click', function () {
                            DeleteWorkflowStep(this.attributes["data-value"].value);
                        });
                        $('.EditSectionPermission').on('click', function () {
                            const workflowid = this.attributes["data-workflow"].value;
                            const workflowstepid = this.attributes["data-step"].value;

                            GetSectionPermission(workflowid, workflowstepid);
                        });

                    }

                    table = $('#workflowsteps-master').DataTable({
                        columnDefs: [
                            { targets: 0, visible: false },
                        ],
                        select: true
                    });

                    table.draw();


                    HideLoader('workflowstepsDiv');
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
function EditWorkflowStep(workflowid) {
    new APICALL(GetGlobalURL('Base', 'GetWorkflowStepsByID?workflowstepid=' + workflowid), 'GET', '', true).FETCH((result, error) => {

        if (result) {

            if (result.data.length > 0) {

                var id = result.data[0].id;
                var workflowid = result.data[0].workflowid;
                var workflowstep = result.data[0].workflowstep;
                var RoleID = result.data[0].RoleID;
                var approvaltypeid = result.data[0].approvaltypeid;
                var sla = result.data[0].sla;
                var sortid = result.data[0].sortid;
                var isactive = result.data[0].isactive;

                $('#s-workflowstepid').val(id);
                $('#s-workflow').val(workflowid);
                $('#s-workflowstep').val(workflowstep);
                $('#s-RoleID').val(RoleID);
                $('#s-approvaltypeid').val(approvaltypeid);
                $('#s-sla').val(sla);
                $('#s-sortid').val(sortid);
                $('#s-active').prop('checked', isactive);

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
function UpdateWorkflowStep() {
    if (!$('#workflowstepsform').valid()) {
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
            ShowLoader('workflowstepsDiv');

            $('#workflowstepsform #s-active').val($('#workflowstepsform #s-active').prop('checked'));

            var Data = $('#workflowstepsform').serialize();

            new APICALL(GetGlobalURL('Base', 'UpdateWorkflowStep'), 'POST', Data, true, false, 'application/x-www-form-urlencoded').FETCH((result, error) => {

                if (result) {
                    GetWorkflowStep();
                    ResetControls();


                    Swal.fire({
                        icon: 'success',
                        title: 'Success...',
                        text: 'Step Updated Successfully!',
                        footer: ''
                    });

                    HideLoader('workflowstepsDiv');
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
function SaveWorkflowStep() {
    if (!$('#workflowstepsform').valid()) {
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
            ShowLoader('workflowstepsDiv');

            $('#workflowstepsform #s-active').val($('#workflowstepsform #s-active').prop('checked'));

            var Data = $('#workflowstepsform').serialize();

            new APICALL(GetGlobalURL('Base', 'SaveWorkflowStep'), 'POST', Data, true, false, 'application/x-www-form-urlencoded').FETCH((result, error) => {

                if (result) {
                    GetWorkflowStep();
                    ResetControls();

                    Swal.fire({
                        icon: 'success',
                        title: 'Success...',
                        text: 'Step Saved Successfully!',
                        footer: ''
                    });

                    HideLoader('workflowstepsDiv');
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

function DeleteWorkflowStep(workflowstepid) {

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
            ShowLoader('workflowstepsDiv');
            new APICALL(GetGlobalURL('Base', 'DeleteWorkflowStep'), 'POST', DeleteData, true).FETCH((result, error) => {

                if (result) {
                    GetWorkflowStep();

                    Swal.fire({
                        icon: 'success',
                        title: 'Success...',
                        text: 'Workflow Step Deleted Successfully!',
                        footer: ''
                    });

                    HideLoader('workflowstepsDiv');
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

    $('#s-workflow').val('');
    $('#s-workflowstep').val('');
    $('#s-RoleID').val('');
    $('#s-approvaltypeid').val('');
    $('#s-workflowstepid').val('');
    $('#s-sla').val('');
    $('#s-sortid').val('');
    $('#s-active').prop('checked', false)
    $('#AllowInsert').prop('checked', false);
    $('#AllowUpdate').prop('checked', false);
    $('#AllowDelete').prop('checked', false);
    $('.savebutton').removeClass('d-none');
    $('.updatebutton').addClass('d-none');
}


function GetSectionPermission(workflowid,stepid) {
    new APICALL(GetGlobalURL('Base', 'GetSectionPermission?workflowid=' + workflowid + '&stepid=' + stepid) , 'GET', '', true).FETCH((result, error) => {

        if (result) {
            $('#sectionPermissionTable').DataTable().clear().destroy();
            $('#sectionPermissionTable tbody').empty();

            if (result.data != null) {
               
                $.each(result.data, function (i, option) {
                    $('#sectionPermissionTable tbody').append(
                        '<tr id="rowid-' + option.id + '">' +
                        '<td>' + option.workflowname + '</td>' +
                        '<td>' + option.workflowstep + '</td>' +
                        '<td>' + option.sectionname + '</td>' +
                        '<td>' +
                        '<div class="form-check form-switch ">' +
                        '<input  type="checkbox" class="form-check-input performVisibleTask" data-value="' + option.id + '" role="switch" id="visible-' + option.id + '" ' +
                        (!!option.isvisible ? 'checked' : '') + ' />' + 
                        '</div>' +
                        '</td>' +
                        '<td>' +
                        '<div class="form-check form-switch">' +
                        '<input class="form-check-input performEnableTask" data-value="' + option.id + '" type="checkbox" role="switch" id="enable-' + option.id + '" ' +
                        (!!option.isenable ? 'checked' : '') + ' />' + 
                        '</div>' +
                        '</td>' +
                        '</tr>'
                    );
                });

                $('.performVisibleTask').on('click', function () {
                    const sectionpermission = this.attributes["data-value"].value;
                    performVisibleTask(sectionpermission);
                });
                $('.performEnableTask').on('click', function () {
                    const sectionpermission = this.attributes["data-value"].value;
                    PerformEnableTask(sectionpermission);
                });
            }
            $('#sectionPermissionTable').DataTable();
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




function performVisibleTask(sectionpermissionid) {
    const isChecked = $('#visible-' + sectionpermissionid).is(':checked');
    var sectionpermissionid = JSON.stringify({
        sectionpermissionid: sectionpermissionid,
        isvisible: isChecked ? 1 : 0
    });
            new APICALL(GetGlobalURL('Base', 'PerformVisibleTask'), 'POST', sectionpermissionid, true).FETCH((result, error) => {

                if (result) {
                    
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

function PerformEnableTask(sectionpermissionid) {
    const isChecked = $('#enable-' + sectionpermissionid).is(':checked');
    var sectionpermissionid = JSON.stringify({
        sectionpermissionid: sectionpermissionid,
        isenable: isChecked ? 1 : 0
    });
    new APICALL(GetGlobalURL('Base', 'PerformEnableTask'), 'POST', sectionpermissionid, true).FETCH((result, error) => {

        if (result) {

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