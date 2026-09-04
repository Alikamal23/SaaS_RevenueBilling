var users = {};
var Clientmanagement = {};

$(document).ready(function () {
    ViewData();

    $("#masterform").validate();

    $('#SaveBtn').on('click', function () {
        SaveData();
    });

    $('#UpdateBtn').on('click', function () {
        UpdateData();
    });

    $('#ClearBtn').on('click', function () {
        DoClear();
    });
});


/* View GRID */
function ViewData() {
    ShowLoader('UserMasterDiv');
    UTILITY.CheckSession((data_) => {
        if (data_) {
            new APICALL(GetGlobalURL('Base', 'GetManufacturer'), 'GET', '', true).FETCH((result, error) => {
                if (result) {
                    $("#user-master").dataTable().fnDestroy();
                    $('#user-master tbody').html('');

                    //console.log(result);

                    if (result.data != null) {
                        users = result.data;
                        $.each(result.data, function (i, option) {
                            var primaryID = option.Id

                            // Create Active/Inactive badge
                            var activeBadge = option.IsActive
                                ? '<span class="badge bg-success">Active</span>'
                                : '<span class="badge bg-danger">Inactive</span>';

                            var UpdateBtn = (data_[0].AllowUpdate == true) ? '<td><button class="avatar-text avatar-md EditData" data-value="' + primaryID + '" data-value1="' + i + '" type="button" data-bs-toggle="tooltip" data-bs-placement="bottom" title="" data-bs-original-title="Edit" aria-label="Edit"><i class="feather-edit-2 edit-icon"></i></button></td>' : '';
                            var DeleteBtn = (data_[0].AllowDelete == true) ? '<td><button class="avatar-text avatar-md DeleteData" data-value="' + primaryID + '" type="button" data-bs-toggle="tooltip" data-bs-placement="bottom" title="" data-bs-original-title="Delete" aria-label="Delete"><i class="feather-trash delete-icon"></i></button></td>' : '';

                            $('#user-master tbody').append(
                                '<tr id="rowid-' + i + '">' +
                                '<td>' + option.Name + '</td>' +
                                '<td>' + activeBadge + '</td>' +
                                UpdateBtn +
                                DeleteBtn +
                                '</tr>'
                            );
                        });
                        $('.EditData').on('click', function () {
                            EditData(this.attributes["data-value"].value, this.attributes["data-value1"].value);
                        });

                        $('.DeleteData').on('click', function () {
                            DeleteData(this.attributes["data-value"].value);
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

/* CRUD Operations */
function SaveData() {
    //if (!$('#masterform').valid()) {
    //    return false;
    //}

    var res = ValidateAll();
    if (res == false) {
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

            // Convert form data to a JSON object
            var formData = {
                Id: $('#EditID').val() === "" ? 0 : parseInt($('#EditID').val()),
                Name: $('#Name').val(),
                IsActive: $('#IsActive').is(':checked')
            };

            // Convert to JSON string
            var jsonData = JSON.stringify(formData);

            // API call using JSON instead of form-urlencoded
            new APICALL(GetGlobalURL('Base', 'SaveManufacturer'), 'POST', jsonData, true, false, 'application/json')
                .FETCH((result, error) => {

                    if (result && result.status === 'success') {
                        ViewData();
                        DoEmptyFields();

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
function EditData(ID, index) {
    DoEmptyFields();

    var Name = users[index].Name;
    var IsActive = users[index].IsActive;

    $('#EditID').val(ID);
    ////alert(ID);

    $('#Name').val(Name);
    $('#IsActive').prop('checked', IsActive);

    if ($('.updatebutton').hasClass('d-none')) {
        $('.savebutton').toggleClass('d-none');
        $('.updatebutton').toggleClass('d-none');
    }

}
function UpdateData() {
    var res = ValidateAll();
    if (res == false) {
        return false;
    }

    Swal.fire({
        title: 'Do you want to update the record?',
        showDenyButton: true,
        showCancelButton: false,
        confirmButtonText: 'Ok',
        denyButtonText: 'Cancel',
    }).then((result) => {
        if (result.isConfirmed) {
            ShowLoader('UserMasterDiv');

            // Build JSON object manually
            var formData = {
                Id: $('#EditID').val() === "" ? 0 : parseInt($('#EditID').val()),
                Name: $('#Name').val(),
                IsActive: $('#IsActive').is(':checked')
            };

            // Convert object to JSON string
            var jsonData = JSON.stringify(formData);

            // Use fetch instead of APICALL to ensure proper JSON binding
            fetch(GetGlobalURL('Base', 'EditManufacturer'), {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: jsonData
            })
                .then(async res => {
                    const data = await res.json();
                    //console.log('Server response:', data);

                    if (res.ok) {
                        ViewData();
                        DoEmptyFields();

                        Swal.fire({
                            icon: 'success',
                            title: 'Success...',
                            text: 'Updated Successfully!'
                        });
                    } else {
                        Swal.fire({
                            icon: 'error',
                            title: 'Error...',
                            text: 'Update failed: ' + (data || 'Unknown error')
                        });
                    }

                    HideLoader('UserMasterDiv');
                })
                .catch(err => {
                    console.error('Fetch error:', err);
                    Swal.fire({
                        icon: 'error',
                        title: 'Error...',
                        text: 'Something went wrong while updating!'
                    });
                    HideLoader('UserMasterDiv');
                });
        }
    });
}
function DeleteData(ID) {
    var DeleteData = JSON.stringify({ Id: ID });

    Swal.fire({
        title: 'Do you want to save the changes?',
        showDenyButton: true,
        showCancelButton: false,
        confirmButtonText: 'Ok',
        denyButtonText: 'Cancel',
    }).then((result) => {
        if (result.isConfirmed) {
            ShowLoader('UserMasterDiv');

            new APICALL(GetGlobalURL('Base', 'DeleteManufacturer'), 'POST', DeleteData, true).FETCH((result, error) => {
                var isDeleted = result.data;

                if (isDeleted) {
                    ViewData();

                    Swal.fire({
                        icon: 'success',
                        title: 'Deleted!',
                        text: 'Record deleted successfully.'
                    });
                    return;
                }

                // ERROR
                if (error && error.status && error.status !== 200) {
                    Swal.fire({
                        icon: 'error',
                        title: 'Error...',
                        text: error.data?.responseText || 'Something went wrong while deleting.'
                    });
                    return;
                }

            });
        }
    });
}

/* Common functions */
function ValidateAll() {
    var isValid = true;

    var Name = $("#Name").val();

    if (Name == null || Name == 'string' || !Name.trim()) {
        Swal.fire({
            icon: 'warning',
            dangerMode: true,
            text: 'Please enter Manufacturer Desc!',
            confirmButtonColor: "#61affe"
        });
        return false;
    }

    return isValid;
}
function DoClear() {
    $('#UserID').val('');
    $('#EditID').val('');
    $('#Name').val('');
    $('#IsActive').prop('checked', false);
    $('.updatebutton').addClass('d-none');
    $('.savebutton').removeClass('d-none');
}
function DoEmptyFields() {
    $('#UserID').val('');
    $('#EditID').val('');
    $('#Name').val('');
    $('#IsActive').prop('checked', false);
    $('.updatebutton').addClass('d-none');
    $('.savebutton').removeClass('d-none');
}
