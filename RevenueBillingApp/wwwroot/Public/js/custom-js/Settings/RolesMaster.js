var roles = {};
$(document).ready(function () {
    GetRoles();
    $("#rolesmasterform").validate();
    $('#SaveBtn').on('click', function () {
        SaveRole();
    });

    $('#UpdateBtn').on('click', function () {
        UpdateRole();
    });

});

function GetRoles() {
    ShowLoader('RolesMasterDiv');

    UTILITY.CheckSession((data_) => {

        if (data_) {

            new APICALL(GetGlobalURL('Base', 'GetRoles'), 'GET', '', true).FETCH((result, error) => {

                if (result) {

                    $('#role-master tbody').html('');
                    if (result.data != null) {
                        roles = result.data;
                        $.each(result.data, function (i, option) {
                            var ActiveCell = option.IsActive == true ? '<td><input type="checkbox" class="role-active checkboxsize" disabled checked="checked"></td>' : '<td><input type="checkbox" class="role-active checkboxsize" disabled></td>'
                            var UpdateBtn = (data_[0].AllowUpdate == true) ? '<td><button class="avatar-text avatar-md EditRole" data-value="' + option.RoleID + '" type="button" data-bs-toggle="tooltip" data-bs-placement="bottom" title="" data-bs-original-title="Edit"><i class="feather-edit-2 edit-icon"></i></button></td>' : '';
                            var DeleteBtn = (data_[0].AllowDelete == true) ? '<td><button class="avatar-text avatar-md DeleteRole"  data-value="' + option.RoleID + '" type="button" data-bs-toggle="tooltip" data-bs-placement="bottom" title="" data-bs-original-title="Delete"><i class="feather-trash delete-icon"></i></button></td>' : '';
                         
                            $('#role-master tbody').append(
                                '<tr id="rowid-' + i + '">' +
                                '<td>' + option.RoleName + '</td>' +
                                ActiveCell +
                                UpdateBtn +
                                DeleteBtn +
                                '</tr>'
                            );
                        });
                        $('.EditRole').on('click', function () {
                            EditRole(this.attributes["data-value"].value);
                        });

                        $('.DeleteRole').on('click', function () {
                            DeleteRole(this.attributes["data-value"].value);
                        });

                    }
                    $('#role-master').DataTable();
                    HideLoader('RolesMasterDiv');
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
function EditRole(RoleID) {

    new APICALL(GetGlobalURL('Base', 'GetRolesByID?RoleID=' + RoleID), 'GET', '', true).FETCH((result, error) => {

        if (result) {

            if (result.data.length > 0) {

                var RoleName = result.data[0].RoleName;
                var IsActive = result.data[0].IsActive;

                $('#s-rolename').val(RoleName);
                $('#s-active').prop('checked', IsActive);
                $('#s-roleid').val(RoleID);

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
function UpdateRole() {
    if (!$('#rolesmasterform').valid()) {
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
            ShowLoader('RolesMasterDiv');
            var Data = $('#rolesmasterform').serialize();

            new APICALL(GetGlobalURL('Base', 'EditRoles'), 'POST', Data, true, false, 'application/x-www-form-urlencoded').FETCH((result, error) => {

                if (result) {
                    GetRoles();
                    ResetControls();


                    Swal.fire({
                        icon: 'success',
                        title: 'Success...',
                        text: 'Role Updated Successfully!',
                        footer: ''
                    });

                    HideLoader('RolesMasterDiv');
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
function SaveRole() {
    if (!$('#rolesmasterform').valid()) {
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
            ShowLoader('RolesMasterDiv');
            var Data = $('#rolesmasterform').serialize();

            new APICALL(GetGlobalURL('Base', 'SaveRoles'), 'POST', Data, true, false, 'application/x-www-form-urlencoded').FETCH((result, error) => {

                if (result) {
                    GetRoles();
                    ResetControls();

                    Swal.fire({
                        icon: 'success',
                        title: 'Success...',
                        text: 'Role Saved Successfully!',
                        footer: ''
                    });

                    HideLoader('RolesMasterDiv');
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
function DeleteRole(RoleID) {

    var UserID = window.UserID;
    var DeleteData = JSON.stringify({
        RoleID: RoleID,
        UserID: UserID
    });

    Swal.fire({
        title: 'Do you want to save the changes?',
        showDenyButton: true,
        showCancelButton: false,
        confirmButtonText: 'Ok',
        denyButtonText: 'Cancel',
    }).then((result) => {
        if (result.isConfirmed) {
            ShowLoader('RolesMasterDiv');
            new APICALL(GetGlobalURL('Base', 'DeleteRoles'), 'POST', DeleteData, true).FETCH((result, error) => {

                if (result) {
                    GetRoles();
                    ResetControls();

                    Swal.fire({
                        icon: 'success',
                        title: 'Success...',
                        text: 'Role Deleted Successfully!',
                        footer: ''
                    });

                    HideLoader('RolesMasterDiv');
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

    $('#s-rolename').val('');
    $('#s-active').prop('checked', false)
    $('#AllowInsert').prop('checked', false);
    $('#AllowUpdate').prop('checked', false);
    $('#AllowDelete').prop('checked', false);
    $('#s-roleid').val('');
    $('.savebutton').removeClass('d-none');
    $('.updatebutton').addClass('d-none');
}