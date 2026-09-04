var users = {};
var Clientmanagement = {};

$(document).ready(function () {
    ViewData();

    GetParty_DDL(2); //only supplier

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
            new APICALL(GetGlobalURL('Base', 'GetProduct'), 'GET', '', true).FETCH((result, error) => {
                if (result) {
                    $("#user-master").dataTable().fnDestroy();
                    $('#user-master tbody').html('');

                    //console.log(result);

                    if (result.data != null) {
                        users = result.data;
                        $.each(result.data, function (i, option) {
                            var primaryID = option.ProductId

                            // Create Active/Inactive badge
                            var activeBadge = option.IsActive
                                ? '<span class="badge bg-success">Active</span>'
                                : '<span class="badge bg-danger">Inactive</span>';

                            var productType = option.ProductType;

                            var UpdateBtn = (data_[0].AllowUpdate == true) ? '<td><button class="avatar-text avatar-md EditData" data-value="' + primaryID + '" data-value1="' + i + '" type="button" data-bs-toggle="tooltip" data-bs-placement="bottom" title="" data-bs-original-title="Edit" aria-label="Edit"><i class="feather-edit-2 edit-icon"></i></button></td>' : '';
                            var DeleteBtn = (data_[0].AllowDelete == true) ? '<td><button class="avatar-text avatar-md DeleteData" data-value="' + primaryID + '" type="button" data-bs-toggle="tooltip" data-bs-placement="bottom" title="" data-bs-original-title="Delete" aria-label="Delete"><i class="feather-trash delete-icon"></i></button></td>' : '';

                            var OpeningDate = formatDate(option.OpeningDate);

                            $('#user-master tbody').append(
                                '<tr id="rowid-' + i + '">' +
                                '<td>' + option.ProductName + '</td>' +
                                '<td>' + option.Category + '</td>' +
                                '<td>' + option.Specification + '</td>' +
                                '<td>' + option.Unit + '</td>' +
                                '<td>' + option.CP + '</td>' +
                                '<td>' + option.SP + '</td>' +
                                '<td>' + option.ReOrderQty + '</td>' +
                                '<td>' + option.Packing + '</td>' +
                                '<td>' + productType + '</td>' +
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

    //var a = $('#partyId').val();
    //alert(a);

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

            var data = {
                ProductId: $('#EditID').val() ? parseInt($('#EditID').val()) : 0,
                ProductName: $('#ProductName').val(),
                Category: $('#Category').val(),
                Specification: $('#Specification').val(),
                Unit: $('#Unit').val(),
                CP: toFloat($('#CP').val()),
                SP: toFloat($('#SP').val()),
                ReOrderQty: $('#ReOrderQty').val() === "" ? 0 : parseInt($('#ReOrderQty').val()),
                Packing: toFloat($('#Packing').val()),
                ProductType: $('#productType').val(),
                PartyId: $('#partyId').val() ? parseInt($('#partyId').val()) : null,
                IsActive: $('#IsActive').is(':checked'),
                CreatedBy: 'superadmin',
                BranchId: 1
            }

            // API call using JSON instead of form-urlencoded
                new APICALL(GetGlobalURL('Base', 'SaveProduct'), 'POST', JSON.stringify(data), true, false, 'application/json; charset=utf-8')
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

    var row = users[index]; // <-- define row
    var IsActive = users[index].IsActive;

    $('#EditID').val(ID);
    ////alert(ID);

    $('#ProductName').val(row.ProductName);
    $('#Category').val(row.Category);
    $('#Specification').val(row.Specification);
    $('#Unit').val(row.Unit);

    $('#CP').val(row.CP);
    $('#SP').val(row.SP);

    $('#ReOrderQty').val(row.ReOrderQty);
    $('#Packing').val(row.Packing);

    $('#productType').val(row.ProductType);
    $('#partyId').val(row.PartyId);

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
                ProductId: $('#EditID').val() ? parseInt($('#EditID').val()) : 0,
                ProductName: $('#ProductName').val(),
                Category: $('#Category').val(),
                Specification: $('#Specification').val(),
                Unit: $('#Unit').val(),
                CP: toFloat($('#CP').val()),
                SP: toFloat($('#SP').val()),
                ReOrderQty: $('#ReOrderQty').val() === "" ? 0 : parseInt($('#ReOrderQty').val()),
                Packing: toFloat($('#Packing').val()),
                ProductType: $('#productType').val(),
                PartyId: $('#partyId').val() ? parseInt($('#partyId').val()) : null,
                IsActive: $('#IsActive').is(':checked'),
                CreatedBy: 'superadmin',
                BranchId: 1
            };

            // Convert object to JSON string
            var jsonData = JSON.stringify(formData);

            // Use fetch instead of APICALL to ensure proper JSON binding
            fetch(GetGlobalURL('Base', 'EditProduct'), {
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
    var DeleteData = JSON.stringify({ ProductId: ID });

    Swal.fire({
        title: 'Do you want to save the changes?',
        showDenyButton: true,
        showCancelButton: false,
        confirmButtonText: 'Ok',
        denyButtonText: 'Cancel',
    }).then((result) => {
        if (result.isConfirmed) {
            ShowLoader('UserMasterDiv');

            new APICALL(GetGlobalURL('Base', 'DeleteProduct'), 'POST', DeleteData, true).FETCH((result, error) => {
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

/* Populate DROP DOWN DDL */
function GetParty_DDL(CategoryCode) {
    new APICALL(GetGlobalURL('Base', 'GetPartyDDL?CategoryCode=' + CategoryCode), 'GET', '', true).FETCH((result, error) => {

        if (result) {
            if (result.data != null) {
                $.each(result.data, function (i, option) {
                    $('#partyId').append(
                        '<option value="' + option.PartyId + '">' + option.PartyName + '</option>'
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

/* Common functions */
function ValidateAll() {
    var isValid = true;

    var ProductName = $("#ProductName").val();
    var PartyName = $("#partyId").val();

    if (ProductName == null || ProductName == 'string' || !ProductName.trim()) {
        Swal.fire({
            icon: 'warning',
            dangerMode: true,
            text: 'Please enter Product Name!',
            confirmButtonColor: "#61affe"
        });
        return false;
    }

    if (PartyName == null || PartyName == '0' || !PartyName.trim()) {
        Swal.fire({
            icon: 'warning',
            dangerMode: true,
            text: 'Please enter Party Name!',
            confirmButtonColor: "#61affe"
        });
        return false;
    }

    return isValid;
}
function DoClear() {
    $('#UserID').val('');
    $('#EditID').val('');

    $('#ProductName').val('');
    $('#Category').val('');
    $('#Specification').val('');
    $('#Unit').val('');
    $('#CP').val('');
    $('#SP').val('');
    $('#ReOrderQty').val('');
    $('#Packing').val('');
    //$('#productType').val('');
    $('#partyId').val('');
    $('#IsActive').prop('checked', true);

    $('.updatebutton').addClass('d-none');
    $('.savebutton').removeClass('d-none');
}
function DoEmptyFields() {
    $('#UserID').val('');
    $('#EditID').val('');

    $('#ProductName').val('');
    $('#Category').val('');
    $('#Specification').val('');
    $('#Unit').val('');
    $('#CP').val('');
    $('#SP').val('');
    $('#ReOrderQty').val('');
    $('#Packing').val('');
    //$('#productType').val('');
    $('#partyId').val('');
    $('#IsActive').prop('checked', true);


    $('.updatebutton').addClass('d-none');
    $('.savebutton').removeClass('d-none');
}
function SetTodayDate(controlId) {
    let today = new Date();
    let yyyy = today.getFullYear();
    let mm = String(today.getMonth() + 1).padStart(2, '0');
    let dd = String(today.getDate()).padStart(2, '0');

    $('#' + controlId).val(`${yyyy}-${mm}-${dd}`);
}
function toFloat(val) {
    return val === "" || val === null ? 0 : parseFloat(val);
}
// Format function
function formatDate(dateString) {
    if (!dateString) return '';
    var date = new Date(dateString);
    var day = ("0" + date.getDate()).slice(-2);
    var month = ("0" + (date.getMonth() + 1)).slice(-2);
    var year = date.getFullYear();
    return day + '-' + month + '-' + year;  // format: dd-MM-yyyy
}
