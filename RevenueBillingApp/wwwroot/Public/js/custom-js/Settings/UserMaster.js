var users = {};
var Clientmanagement = {};
var AdUsers = [];

$(document).ready(function () {
    GetCities();
    //GetUserType_DDL();
    GetRoles_DDL();
    GetUsers();
    
    $('#RoleID').on('change', function (e) {

        var selected_value = $(this).val();

        if (selected_value !== '') {

            GetMapping(selected_value);
        }
        else {
            $('#FormID').html('<option value="">Select Default Form</option>');
        }
    });

    $("#usermasterform").validate();

    $('#SaveBtn').on('click', function () {
        SaveUser();
    });

    $('#UpdateBtn').on('click', function () {
        UpdateUser();
    });


    $('#AdUserBtn').on('click', function () {
        FetchAdUsers();
    });

    $('#ClearBtn').on('click', function () {
        DoClear();
    });
    $('#CloseModalBtn').on('click', function () {
        CloseModal();
    });
});

function GetCities() {
    new APICALL(GetGlobalURL('Base', 'GetAllCities'), 'GET', '', true).FETCH((result, error) => {

        if (result) {
            if (result.data != null) {
                $.each(result.data, function (i, option) {
                    $('#CityID').append(
                        '<option value="' + option.ID + '" required>' + option.City + '</option>'
                    );
                });
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
function GetRoles_DDL() {
    new APICALL(GetGlobalURL('Base', 'GetUserRoles'), 'GET', '', true).FETCH((result, error) => {

        if (result) {
            if (result.data != null) {
                $.each(result.data, function (i, option) {
                    $('#RoleID').append(
                        '<option value="' + option.RoleID + '">' + option.RoleName + '</option>'
                    );
                });
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
function GetForms_DDL() {
    new APICALL(GetGlobalURL('Base', 'GetForms'), 'GET', '', true).FETCH((result, error) => {

        if (result) {
            if (result.data != null) {
                $.each(result.data, function (i, option) {
                    try {
                        $('#FormID').append(
                            '<option value="' + option.FormID + '">' + option.FormDisplayName + '</option>'
                        );
                    }
                    catch (ex) {
                        alert(ex);
                    }
                });
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
function GetUsers() {
    ShowLoader('UserMasterDiv');
    UTILITY.CheckSession((data_) => {

        if (data_) {
            new APICALL(GetGlobalURL('Base', 'GetUsers'), 'GET', '', true).FETCH((result, error) => {

                if (result) {
                    $("#user-master").dataTable().fnDestroy();
                    $('#user-master tbody').html('');

                    if (result.data != null) {
                        users = result.data;
                        $.each(result.data, function (i, option) {
                            var isActiveCell = option.IsActive == true ? '<td><input type="checkbox" class="role-active" disabled checked="checked"></td>' : '<td><input type="checkbox" class="role-active" disabled></td>'
                            var UpdateBtn = (data_[0].AllowUpdate == true) ? '<td><button class="avatar-text avatar-md EditUser" data-value="' + option.UserID + '" data-value1="' + i + '" type="button" data-bs-toggle="tooltip" data-bs-placement="bottom" title="" data-bs-original-title="Edit" aria-label="Edit"><i class="feather-edit-2 edit-icon"></i></button></td>' : '';
                            var DeleteBtn = (data_[0].AllowDelete == true) ? '<td><button class="avatar-text avatar-md DeleteUser" data-value="' + option.UserID + '" type="button" data-bs-toggle="tooltip" data-bs-placement="bottom" title="" data-bs-original-title="Delete" aria-label="Delete"><i class="feather-trash delete-icon"></i></button></td>' : '';

                            var FormName = "";
                            try {
                                FormName = option.FormName.split('/')[1];
                            } catch (ex) {
                                alert(ex);
                            }
                            $('#user-master tbody').append(
                                '<tr id="rowid-' + i + '">' +
                                '<td>' + option.UserName + '</td>' +
                                '<td>' + option.UserEmail + '</td>' +
                                '<td>' + option.RoleName + '</td>' +
                                '<td>' + FormName + '</td>' +
                                '<td>' + (option.City != null ? option.City : '-') + '</td>' +
                                '<td>' + option.MobileNumber + '</td>' +
                                isActiveCell +
                                UpdateBtn +
                                DeleteBtn +
                                '</tr>'
                            );
                        });
                        $('.EditUser').on('click', function () {
                            EditUser(this.attributes["data-value"].value, this.attributes["data-value1"].value);
                        });

                        $('.DeleteUser').on('click', function () {
                            DeleteUser(this.attributes["data-value"].value);
                        });

                    }
                    $('#user-master').DataTable();
                    HideLoader('UserMasterDiv');
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
function EditUser(UserID, index) {
    DoEmptyFields();

    var CityID = users[index].CityID;
    var UserName = users[index].UserName;
    var UserEmail = users[index].UserEmail;
    var RoleID = users[index].RoleID;
    var FormID = users[index].DefaultFormID;
    var UserTypeID = users[index].TypeID;
    var MobileNumber = users[index].MobileNumber;
    var IsActive = users[index].IsActive;
    var Designation = users[index].Designation;
    var Department = users[index].Department;
    var DomainName = users[index].DomainName;
    var IsAdUser = users[index].IsAdUser;
    var LoginName = users[index].LoginName;

    $('#UserID').val(UserID);
    $('#CityID').val(CityID);
    $('#UserName').val(UserName);
    $('#UserEmail').val(UserEmail);
    $('#RoleID').val(RoleID).change();
    //$('#UserTypeID').val(UserTypeID).change();
    $('#MobileNumber').val(MobileNumber);
    $('#IsActive').prop('checked', IsActive);
    $('#UserID').val(UserID);
    $('#Designation').val(Designation);
    $('#Department').val(Department);
    $('#DomainName').val(DomainName);
    $('#LoginName').val(LoginName);
    $('#IsAdUser').prop('checked', IsAdUser);
    $('#FormID').val(FormID).change();
    EnableDisableFields(IsAdUser);

    if ($('.updatebutton').hasClass('d-none')) {
        $('.savebutton').toggleClass('d-none');
        $('.updatebutton').toggleClass('d-none');
    }

}
function DoClear() {
    $('#UserID').val('');
    $('#UserName').val('');
    $('#UserEmail').val('');
    $('#MobileNumber').val('');
    $('#Designation').val('');
    $('#Department').val('');
    $('#LoginName').val('');
    $('#RoleID').val('').change();
    $('#FormID').val('').change();
    $('#UserTypeID').val('').change();
    $('#IsActive').prop('checked', false);
    $('.updatebutton').addClass('d-none');
    $('.savebutton').removeClass('d-none');
}
function UpdateUser() {
    if (!$('#usermasterform').valid()) {
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
            ShowLoader('UserMasterDiv');
            var Data = $('#usermasterform').serialize();
            new APICALL(GetGlobalURL('Base', 'EditUsers'), 'POST', Data, true, false, 'application/x-www-form-urlencoded').FETCH((result, error) => {

                if (result) {
                    GetUsers();
                    DoEmptyFields();

                    Swal.fire({
                        icon: 'success',
                        title: 'Success...',
                        text: 'User Updated Successfully!',
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
                HideLoader('UserMasterDiv');
            });
        }
    });

}
function SaveUser() {
    if (!$('#usermasterform').valid()) {
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
            ShowLoader('UserMasterDiv');
            var Data = $('#usermasterform').serialize();
            new APICALL(GetGlobalURL('Base', 'SaveUsers'), 'POST', Data, true, false, 'application/x-www-form-urlencoded').FETCH((result, error) => {

                if (result) {
                    if (result.data != null) {

                        GetUsers();
                        DoEmptyFields();

                    }
                    HideLoader('UserMasterDiv');

                    Swal.fire({
                        icon: 'success',
                        title: 'Success...',
                        text: 'User Saved Successfully!',
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
    });
}
function DoEmptyFields() {
    $('#UserID').val('');
    $('#UserName').val('');
    $('#UserEmail').val('');
    $('#MobileNumber').val('');
    $('#RoleID').val('').change();
    $('#FormID').val('').change();
    $('#UserTypeID').val('').change();
    $('#IsActive').prop('checked', false);
    $('#LoginName').val('');
    $('#DomainName').val('');
    $('#Designation').val('');
    $('#Department').val('');
    $('#IsAdUser').prop('checked', false);
    EnableDisableFields(false);
    $('.updatebutton').addClass('d-none');
    $('.savebutton').removeClass('d-none');

}
function DeleteUser(UserID) {

    var DeleteData = JSON.stringify({
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
            ShowLoader('UserMasterDiv');
            new APICALL(GetGlobalURL('Base', 'DeleteUsers'), 'POST', DeleteData, true).FETCH((result, error) => {

                if (result) {
                    GetUsers();

                    Swal.fire({
                        icon: 'success',
                        title: 'Success...',
                        text: 'User Deleted Successfully!',
                        footer: ''
                    });

                    HideLoader('UserMasterDiv');
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
function GetMapping(RoleID) {
    new APICALL(GetGlobalURL('Base', 'GetMapping?RoleId=' + RoleID), 'GET', '', false).FETCH((result, error) => {

        if (result) {
            $('#FormID').html('<option value="">Select Default Form</option>');
            if (result.data != null) {
                $.each(result.data, function (i, option) {
                    try {
                        $('#FormID').append(
                            '<option value="' + option.FormID + '">' + option.FormDisplayName + '</option>'
                        );
                    }
                    catch (ex) {
                        alert(ex);
                    }
                });
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
function FetchAdUsers() {
    ShowLoader('ADUserMasterDiv');
    new APICALL(GetGlobalURL('Base', 'FetchAdUsers'), 'GET', '', true).FETCH((result, error) => {

        if (result) {
            $('#ModalActiveDirectoryUsers').modal('show');
            $('#TblActiveDirectoryUsers').DataTable().clear().destroy();
            $('#TblActiveDirectoryUsers tbody').empty();
            if (result.data != null) {
                AdUsers = result.data;
                $.each(result.data, function (i, option) {
                    $('#TblActiveDirectoryUsers tbody').append(
                        '<tr id="rowid-' + i + '">' +
                        '<td>' + option.UserName + '</td>' +
                        '<td>' + option.UserID + '</td>' +
                        '<td>' + option.EmailID + '</td>' +
                        '<td>' + option.Designation + '</td>' +
                        '<td>' + option.Department + '</td>' +
                        '<td><button class="btn btn-sm btn-orange btn-block SelectAdUser" data-value="' + i + '" type="button"><i class="bx bx-expand"></i></button></td>' +
                        '</tr>'
                    );
                });
                $('.SelectAdUser').on('click', function () {
                    SelectAdUser(this.attributes["data-value"].value);
                });

            }
            $('#TblActiveDirectoryUsers').DataTable();
        }
        if (error) {

            Swal.fire({
                icon: 'error',
                title: 'Error...',
                text: error.data.responseText,
                footer: ''
            });
        }
        HideLoader('ADUserMasterDiv');
    });
}
function SelectAdUser(index) {
    $('#UserName').val('');
    $('#UserEmail').val('');
    $('#LoginName').val('');
    $('#DomainName').val('');
    $('#Designation').val('');
    $('#Department').val('');
    $('#UserTypeID').val('').change();
    $('#MobileNumber').val('');
    $('#IsAdUser').prop('checked', false);

    var UserName = AdUsers[index].UserName;
    var UserEmail = AdUsers[index].EmailID;
    var LoginName = AdUsers[index].UserID;
    var DomainName = AdUsers[index].DomainName;
    var Designation = AdUsers[index].Designation;
    var Department = AdUsers[index].Department;
    var MobileNumber = AdUsers[index].TelephoneNo;

    $('#UserName').val(UserName);
    $('#UserEmail').val(UserEmail);
    $('#LoginName').val(LoginName);
    $('#DomainName').val(DomainName);
    $('#Designation').val(Designation);
    $('#Department').val(Department);
    $('#UserTypeID').val(1).change();
    $('#MobileNumber').val(MobileNumber);
    $('#IsAdUser').prop('checked', true);

    EnableDisableFields(true);

    $('#ModalActiveDirectoryUsers').modal('hide');
}
function EnableDisableFields(action) {

    $("#UserName").removeAttr('readonly');
    $("#UserEmail").removeAttr('readonly');
    $("#LoginName").removeAttr('readonly');
    $("#UserTypeID").removeAttr('readonly');
    $("#UserName").css("background-color", "white");
    $("#UserEmail").css("background-color", "white");
    $("#LoginName").css("background-color", "white");

    if (action == true) {

        $("#UserName").attr('readonly', 'readonly');
        $("#UserEmail").attr('readonly', 'readonly');
        $("#LoginName").attr('readonly', 'readonly');
        $("#UserTypeID").attr('readonly', 'readonly');
        $("#UserName").css("background-color", "#ced4da");
        $("#UserEmail").css("background-color", "#ced4da");
        $("#LoginName").css("background-color", "#ced4da");
    }
}
function CloseModal() {
    $('#ModalActiveDirectoryUsers').modal('hide');
}
