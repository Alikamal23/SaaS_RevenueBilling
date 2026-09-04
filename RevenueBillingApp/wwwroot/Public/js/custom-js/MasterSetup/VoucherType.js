var users = {};
var Clientmanagement = {};

$(document).ready(function () {
    ViewData();

    //$("#masterform").validate();

    //$('#SaveBtn').on('click', function () {
    //    SaveData();
    //});

    //$('#UpdateBtn').on('click', function () {
    //    UpdateData();
    //});

    //$('#ClearBtn').on('click', function () {
    //    DoClear();
    //});
});


/* View GRID */
function ViewData() {
    ShowLoader('UserMasterDiv');
    UTILITY.CheckSession((data_) => {
        if (data_) {
            new APICALL(GetGlobalURL('Base', 'GetVoucherType'), 'GET', '', true).FETCH((result, error) => {
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


                            //var UpdateBtn = (data_[0].AllowUpdate == true) ? '<td><button class="avatar-text avatar-md EditData" data-value="' + primaryID + '" data-value1="' + i + '" type="button" data-bs-toggle="tooltip" data-bs-placement="bottom" title="" data-bs-original-title="Edit" aria-label="Edit"><i class="feather-edit-2 edit-icon"></i></button></td>' : '';
                            //var DeleteBtn = (data_[0].AllowDelete == true) ? '<td><button class="avatar-text avatar-md DeleteData" data-value="' + primaryID + '" type="button" data-bs-toggle="tooltip" data-bs-placement="bottom" title="" data-bs-original-title="Delete" aria-label="Delete"><i class="feather-trash delete-icon"></i></button></td>' : '';

                            $('#user-master tbody').append(
                                '<tr id="rowid-' + i + '">' +
                                '<td>' + option.Code + '</td>' +
                                '<td>' + option.Description + '</td>' +
                                '<td>' + option.VNature + '</td>' +
                                '<td>' + activeBadge + '</td>' +
                                //UpdateBtn +
                                //DeleteBtn +
                                '</tr>'
                            );
                        });
                        //$('.EditData').on('click', function () {
                        //    EditData(this.attributes["data-value"].value, this.attributes["data-value1"].value);
                        //});

                        //$('.DeleteData').on('click', function () {
                        //    DeleteData(this.attributes["data-value"].value);
                        //});

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
//function SaveData() {
//    //if (!$('#masterform').valid()) {
//    //    return false;
//    //}

//    //var a = $('#partyId').val();
//    //alert(a);

//    var res = ValidateAll();
//    if (res == false) {
//        return false;
//    }

//    Swal.fire({
//        title: 'Do you want to save the changes?',
//        showDenyButton: true,
//        showCancelButton: false,
//        confirmButtonText: 'Ok',
//        denyButtonText: 'Cancel',
//    }).then((result) => {
//        if (result.isConfirmed) {
//            ShowLoader('UserMasterDiv');

//            var data = {
//                CurrencyId: $('#EditID').val() ? parseInt($('#EditID').val()) : 0,
//                CurrencyCode: $('#CurrencyCode').val(),
//                CurrencyDesc: $('#CurrencyDesc').val(),
//                CurrencyAbbr: $('#CurrencyAbbr').val(),
//                CurrencyRate: toFloat($('#CurrencyRate').val()),
//                DefaultCurrency: $('#DefaultCurrency').is(':checked'),
//                IsActive: $('#IsActive').is(':checked'),
//                CreatedBy: 'superadmin',
//                BranchId: 1
//            }

//            // API call using JSON instead of form-urlencoded
//            new APICALL(GetGlobalURL('Base', 'SaveCurrency'), 'POST', JSON.stringify(data), true, false, 'application/json; charset=utf-8')
//                .FETCH((result, error) => {

//                    if (result && result.status === 'success') {
//                        ViewData();
//                        DoEmptyFields();

//                        Swal.fire({
//                            icon: 'success',
//                            title: 'Success...',
//                            text: 'Saved Successfully!'
//                        });
//                        return;
//                    }

//                    // ERROR block
//                    if (error && error.status && error.status !== 200) {
//                        Swal.fire({
//                            icon: 'error',
//                            title: 'Error...',
//                            text: error.data?.responseText || 'Something went wrong!'
//                        });
//                        return;
//                    }
//                });
//        }
//    });
//}
//function EditData(ID, index) {
//    DoEmptyFields();

//    var row = users[index]; // <-- define row
//    var IsActive = users[index].IsActive;
//    var DefaultCurrency = users[index].DefaultCurrency;

//    $('#EditID').val(ID);
//    ////alert(ID);

//    $('#CurrencyCode').val(row.CurrencyCode);
//    $('#CurrencyDesc').val(row.CurrencyDesc);
//    $('#CurrencyAbbr').val(row.CurrencyAbbr);

//    $('#CurrencyRate').val(row.CurrencyRate);
//    $('#DefaultCurrency').prop('checked', DefaultCurrency);

//    $('#IsActive').prop('checked', IsActive);


//    if ($('.updatebutton').hasClass('d-none')) {
//        $('.savebutton').toggleClass('d-none');
//        $('.updatebutton').toggleClass('d-none');
//    }

//}
//function UpdateData() {
//    var res = ValidateAll();
//    if (res == false) {
//        return false;
//    }

//    Swal.fire({
//        title: 'Do you want to update the record?',
//        showDenyButton: true,
//        showCancelButton: false,
//        confirmButtonText: 'Ok',
//        denyButtonText: 'Cancel',
//    }).then((result) => {
//        if (result.isConfirmed) {
//            ShowLoader('UserMasterDiv');

//            // Build JSON object manually
//            var formData = {
//                CurrencyId: $('#EditID').val() ? parseInt($('#EditID').val()) : 0,
//                CurrencyCode: $('#CurrencyCode').val(),
//                CurrencyDesc: $('#CurrencyDesc').val(),
//                CurrencyAbbr: $('#CurrencyAbbr').val(),
//                CurrencyRate: toFloat($('#CurrencyRate').val()),
//                DefaultCurrency: $('#DefaultCurrency').is(':checked'),
//                IsActive: $('#IsActive').is(':checked'),
//                CreatedBy: 'superadmin',
//                BranchId: 1
//            };

//            // Convert object to JSON string
//            var jsonData = JSON.stringify(formData);

//            // Use fetch instead of APICALL to ensure proper JSON binding
//            fetch(GetGlobalURL('Base', 'EditCurrency'), {
//                method: 'POST',
//                headers: {
//                    'Content-Type': 'application/json'
//                },
//                body: jsonData
//            })
//                .then(async res => {
//                    const data = await res.json();
//                    //console.log('Server response:', data);

//                    if (res.ok) {
//                        ViewData();
//                        DoEmptyFields();

//                        Swal.fire({
//                            icon: 'success',
//                            title: 'Success...',
//                            text: 'Updated Successfully!'
//                        });
//                    } else {
//                        Swal.fire({
//                            icon: 'error',
//                            title: 'Error...',
//                            text: 'Update failed: ' + (data || 'Unknown error')
//                        });
//                    }

//                    HideLoader('UserMasterDiv');
//                })
//                .catch(err => {
//                    console.error('Fetch error:', err);
//                    Swal.fire({
//                        icon: 'error',
//                        title: 'Error...',
//                        text: 'Something went wrong while updating!'
//                    });
//                    HideLoader('UserMasterDiv');
//                });
//        }
//    });
//}
//function DeleteData(ID) {
//    var DeleteData = JSON.stringify({ CurrencyId: ID });

//    Swal.fire({
//        title: 'Do you want to save the changes?',
//        showDenyButton: true,
//        showCancelButton: false,
//        confirmButtonText: 'Ok',
//        denyButtonText: 'Cancel',
//    }).then((result) => {
//        if (result.isConfirmed) {
//            ShowLoader('UserMasterDiv');

//            new APICALL(GetGlobalURL('Base', 'DeleteCurrency'), 'POST', DeleteData, true).FETCH((result, error) => {
//                var isDeleted = result.data;

//                if (isDeleted) {
//                    ViewData();

//                    Swal.fire({
//                        icon: 'success',
//                        title: 'Deleted!',
//                        text: 'Record deleted successfully.'
//                    });
//                    return;
//                }

//                // ERROR
//                if (error && error.status && error.status !== 200) {
//                    Swal.fire({
//                        icon: 'error',
//                        title: 'Error...',
//                        text: error.data?.responseText || 'Something went wrong while deleting.'
//                    });
//                    return;
//                }

//            });
//        }
//    });
//}


/* Common functions */
//function ValidateAll() {
//    var isValid = true;

//    var CurrencyCode = $("#CurrencyCode").val();
//    var CurrencyDesc = $("#CurrencyDesc").val();

//    if (CurrencyCode == null || CurrencyCode == '0' || !CurrencyCode.trim()) {
//        Swal.fire({
//            icon: 'warning',
//            dangerMode: true,
//            text: 'Please enter Currency Name!',
//            confirmButtonColor: "#61affe"
//        });
//        return false;
//    }


//    if (CurrencyDesc == null || CurrencyDesc == 'string' || !CurrencyDesc.trim()) {
//        Swal.fire({
//            icon: 'warning',
//            dangerMode: true,
//            text: 'Please enter Currency Desc!',
//            confirmButtonColor: "#61affe"
//        });
//        return false;
//    }


//    return isValid;
//}
//function DoClear() {
//    $('#UserID').val('');
//    $('#EditID').val('');

//    $('#DefaultCurrency').prop('checked', true);
//    $('#IsActive').prop('checked', true);

//    $('#CurrencyDesc').val('');
//    $('#CurrencyCode').val('');
//    $('#CurrencyDesc').val('');
//    $('#CurrencyAbbr').val('');
//    $('#CurrencyRate').val('');


//    $('.updatebutton').addClass('d-none');
//    $('.savebutton').removeClass('d-none');
//}
//function DoEmptyFields() {
//    $('#UserID').val('');
//    $('#EditID').val('');

//    $('#CurrencyDesc').val('');
//    $('#CurrencyCode').val('');
//    $('#CurrencyDesc').val('');
//    $('#CurrencyAbbr').val('');
//    $('#CurrencyRate').val('');

//    $('#DefaultCurrency').prop('checked', true);

//    $('#IsActive').prop('checked', true);


//    $('.updatebutton').addClass('d-none');
//    $('.savebutton').removeClass('d-none');
//}
//function SetTodayDate(controlId) {
//    let today = new Date();
//    let yyyy = today.getFullYear();
//    let mm = String(today.getMonth() + 1).padStart(2, '0');
//    let dd = String(today.getDate()).padStart(2, '0');

//    $('#' + controlId).val(`${yyyy}-${mm}-${dd}`);
//}
//function toFloat(val) {
//    return val === "" || val === null ? 0 : parseFloat(val);
//}
//// Format function
//function formatDate(dateString) {
//    if (!dateString) return '';
//    var date = new Date(dateString);
//    var day = ("0" + date.getDate()).slice(-2);
//    var month = ("0" + (date.getMonth() + 1)).slice(-2);
//    var year = date.getFullYear();
//    return day + '-' + month + '-' + year;  // format: dd-MM-yyyy
//}
