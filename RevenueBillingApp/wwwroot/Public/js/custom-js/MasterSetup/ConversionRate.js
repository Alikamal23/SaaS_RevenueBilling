var users = {};
var Clientmanagement = {};

$(document).ready(function () {
    GetAllCurrency_DDL();

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

    SetTodayDate('txtConvDate');

});


/* View GRID */
function ViewData() {
    ShowLoader('UserMasterDiv');
    UTILITY.CheckSession((data_) => {
        if (data_) {
            new APICALL(GetGlobalURL('Base', 'GetConversionRate'), 'GET', '', true).FETCH((result, error) => {
                if (result) {
                    $("#user-master").dataTable().fnDestroy();
                    $('#user-master tbody').html('');

                    //console.log(result);

                    if (result.data != null) {
                        users = result.data;
                        $.each(result.data, function (i, option) {
                            var primaryID = option.id

                            var UpdateBtn = (data_[0].AllowUpdate == true) ? '<td><button class="avatar-text avatar-md EditData" data-value="' + primaryID + '" data-value1="' + i + '" type="button" data-bs-toggle="tooltip" data-bs-placement="bottom" title="" data-bs-original-title="Edit" aria-label="Edit"><i class="feather-edit-2 edit-icon"></i></button></td>' : '';
                            var DeleteBtn = (data_[0].AllowDelete == true) ? '<td><button class="avatar-text avatar-md DeleteData" data-value="' + primaryID + '" type="button" data-bs-toggle="tooltip" data-bs-placement="bottom" title="" data-bs-original-title="Delete" aria-label="Delete"><i class="feather-trash delete-icon"></i></button></td>' : '';

                            $('#user-master tbody').append(
                                '<tr id="rowid-' + i + '">' +
                                '<td>' + option.currency_code + '</td>' +
                                '<td>' + formatDate(option.conversion_date) + '</td>' +
                                '<td>' + formatAmount(option.conversion_rate) + '</td>' +
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

/* populate DDL */
function GetAllCurrency_DDL() {
    new APICALL(GetGlobalURL('Base', 'GetCurrencyDDL'), 'GET', '', true).FETCH((result, error) => {
        if (result) {
            if (result.data != null) {
                window.allUsers = result.data;

                var ddl = $('#ddlCurrency');
                populateDropdowns(ddl, result.data, null, 1);
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
function populateDropdowns(ddl, users, selectedManagerId = null, customfield) {
    ddl.empty().append('<option value="0" selected>Please select</option>');

    $.each(users, function (i, option) {
        if (option.ddlvalue != selectedManagerId) {

            if (customfield == 0) {
                ddl.append(
                    `<option value="${option.ddlvalue}">${option.ddltext}</option>`
                );
            } else {
                ddl.append(
                    `<option value="${option.ddlvalue}" data-customfield="${option.customfield}">${option.ddltext}</option>`
                );
            }
        }
    });
}


/* CRUD Operations */
function SaveData() {
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
                id: $('#EditID').val() ? parseInt($('#EditID').val()) : 0,
                currency_id: $('#ddlCurrency').val(),
                conversion_date: $('#txtConvDate').val(),
                conversion_rate: $('#conv_rate').val(),
                //IsActive: $('#IsActive').is(':checked'),
                createdby: '1'
            }

            // API call using JSON instead of form-urlencoded
            new APICALL(GetGlobalURL('Base', 'SaveConversionRate'), 'POST', JSON.stringify(data), true, false, 'application/json; charset=utf-8')
                .FETCH((result, error) => {
                    HideLoader('UserMasterDiv');

                    if (error) {
                        Swal.fire({
                            icon: 'error',
                            title: 'Error',
                            text: error.data?.responseText || 'Something went wrong.'
                        });
                        return;
                    }

                    if (!result || !result.data || result.data.length == 0) {
                        Swal.fire({
                            icon: 'error',
                            title: 'Error',
                            text: 'No data returned.'
                        });
                        return;
                    }


                    var row = result.data[0];

                    if (row.Status == 0) {
                        Swal.fire({
                            icon: 'warning',
                            title: 'Duplicate Record',
                            text: row.Message
                        });
                        return;
                    }

                    ViewData();
                    DoEmptyFields();

                    Swal.fire({
                        icon: 'success',
                        title: 'Success',
                        text: row.Message
                    });


                });
        }
    });
}
function EditData(ID, index) {
    DoEmptyFields();

    var row = users[index]; // <-- define row
    ////var IsActive = users[index].IsActive;
    ////console.log(row);

    $('#EditID').val(ID);
    ////alert(ID);

    $('#ddlCurrency').val(row.currency_id);

    var conv_date = row.conversion_date.split('T')[0];

    $('#txtConvDate').val(conv_date);
    $('#conv_rate').val(row.conversion_rate);

    //$('#IsActive').prop('checked', IsActive);

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

            var formData = {
                id: $('#EditID').val() ? parseInt($('#EditID').val()) : 0,
                currency_id: $('#ddlCurrency').val(),
                conversion_date: $('#txtConvDate').val(),
                conversion_rate: $('#conv_rate').val(),
                createdby: '1'
            };

            fetch(GetGlobalURL('Base', 'EditConversionRate'), {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(formData)
            })
                .then(async res => {

                    const data = await res.json();

                    HideLoader('UserMasterDiv');

                    // Duplicate Record
                    if (data[0].Status == 0) {
                        Swal.fire({
                            icon: 'warning',
                            title: 'Duplicate Record',
                            text: data[0].Message
                        });
                        return;
                    }

                    // Success
                    if (res.ok) {
                        ViewData();
                        DoEmptyFields();

                        Swal.fire({
                            icon: 'success',
                            title: 'Success...',
                            text: data[0].Message
                        });
                    }
                    else {

                        Swal.fire({
                            icon: 'error',
                            title: 'Error...',
                            text: data || 'Unknown error'
                        });

                    }

                })
                .catch(err => {

                    HideLoader('UserMasterDiv');

                    console.error(err);

                    Swal.fire({
                        icon: 'error',
                        title: 'Error...',
                        text: 'Something went wrong while updating!'
                    });

                });

        }
    });
}
function DeleteData(ID) {
    var DeleteData = JSON.stringify({ id: ID });

    Swal.fire({
        title: 'Do you want to save the changes?',
        showDenyButton: true,
        showCancelButton: false,
        confirmButtonText: 'Ok',
        denyButtonText: 'Cancel',
    }).then((result) => {
        if (result.isConfirmed) {
            ShowLoader('UserMasterDiv');

            new APICALL(GetGlobalURL('Base', 'DeleteConversionRate'), 'POST', DeleteData, true).FETCH((result, error) => {
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

    var currency = $("#ddlCurrency").val();
    var conv_rate = $("#conv_rate").val();

    if (currency == null || currency == '0' || !currency.trim()) {
        Swal.fire({
            icon: 'warning',
            dangerMode: true,
            text: 'Please select Currency!',
            confirmButtonColor: "#61affe"
        });
        return false;
    }


    if (conv_rate == null || conv_rate == 'string' || !conv_rate.trim()) {
        Swal.fire({
            icon: 'warning',
            dangerMode: true,
            text: 'Please enter Conversion Rate!',
            confirmButtonColor: "#61affe"
        });
        return false;
    }


    return isValid;
}
function DoClear() {
    $('#UserID').val('');
    $('#EditID').val('');

    ////$('#IsActive').prop('checked', true);

    $('#ddlCurrency').val('0');
    $('#txtConvDate').val(nuill);
    $('#conv_rate').val('');

    $('.updatebutton').addClass('d-none');
    $('.savebutton').removeClass('d-none');
}
function DoEmptyFields() {
    $('#UserID').val('');
    $('#EditID').val('');

    $('#ddlCurrency').val('0');
    $('#txtConvDate').val(null);
    $('#conv_rate').val('');

    ////$('#IsActive').prop('checked', true);

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
function formatAmount(amount) {
    if (amount == null || amount === "" || isNaN(amount))
        return "0.00";

    return Number(amount).toLocaleString("en-US", {
        minimumFractionDigits: 2,
        maximumFractionDigits: 2
    });
}
function formatDate(dateString) {
    if (!dateString) return "";

    var date = new Date(dateString);

    if (isNaN(date.getTime()))
        return "";

    var day = String(date.getDate()).padStart(2, "0");
    var month = String(date.getMonth() + 1).padStart(2, "0");
    var year = date.getFullYear();

    return day + "-" + month + "-" + year;
}
