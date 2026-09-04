var users = {};
var Clientmanagement = {};

$(document).ready(function () {
    ViewData('0');

    // Set opening date to today's date
    SetTodayDate("OpeningDate");

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
function ViewData(categoryCode) {
    ShowLoader('UserMasterDiv');
    UTILITY.CheckSession((data_) => {
        if (data_) {
            new APICALL(GetGlobalURL('Base', 'GetParty?CategoryCode=' + categoryCode), 'GET', '', true).FETCH((result, error) => {
                if (result) {
                    $("#user-master").dataTable().fnDestroy();
                    $('#user-master tbody').html('');

                    //console.log(result);

                    if (result.data != null) {
                        users = result.data;
                        $.each(result.data, function (i, option) {
                            var primaryID = option.PartyId

                            // Create Active/Inactive badge
                            var activeBadge = option.IsActive
                                ? '<span class="badge bg-success">Active</span>'
                                : '<span class="badge bg-danger">Inactive</span>';

                            var partyType = option.CategoryCode === 1 ? 'Customer' : 'Supplier';

                            var UpdateBtn = (data_[0].AllowUpdate == true) ? '<td><button class="avatar-text avatar-md EditData" data-value="' + primaryID + '" data-value1="' + i + '" type="button" data-bs-toggle="tooltip" data-bs-placement="bottom" title="" data-bs-original-title="Edit" aria-label="Edit"><i class="feather-edit-2 edit-icon"></i></button></td>' : '';
                            var DeleteBtn = (data_[0].AllowDelete == true) ? '<td><button class="avatar-text avatar-md DeleteData" data-value="' + primaryID + '" type="button" data-bs-toggle="tooltip" data-bs-placement="bottom" title="" data-bs-original-title="Delete" aria-label="Delete"><i class="feather-trash delete-icon"></i></button></td>' : '';

                            var OpeningDate = formatDate(option.OpeningDate);

                            $('#user-master tbody').append(
                                '<tr id="rowid-' + i + '">' +
                                '<td>' + option.PartyName + '</td>' +
                                '<td>' + option.Contact + '</td>' +
                                '<td>' + option.Address + '</td>' +
                                '<td>' + option.Phone1 + '</td>' +
                                '<td>' + option.Phone2 + '</td>' +
                                '<td>' + option.Cell + '</td>' +
                                '<td>' + option.City + '</td>' +
                                '<td>' + option.PartyGroup + '</td>' +
                                '<td>' + option.NTNNo + '</td>' +
                                '<td>' + option.GSTNo + '</td>' +
                                '<td>' + option.OpeningBalance + '</td>' +
                                '<td>' + OpeningDate + '</td>' +
                                '<td>' + partyType + '</td>' +
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
                PartyId: $('#EditID').val() === "" ? 0 : parseInt($('#EditID').val()),
                PartyCode: $('#EditID').val() === "" ? 0 : parseInt($('#EditID').val()),
                PartyName: $('#PartyName').val(),
                Contact: $('#Contact').val(),
                Address: $('#Address').val(),
                Phone1: $('#Phone1').val(),
                Phone2: $('#Phone2').val(),
                Cell: $('#Cell').val(),
                Fax: $('#Fax').val(),
                Email: $('#Email').val(),
                City: $('#City').val(),
                PartyGroup: $('#PartyGroup').val(),
                NTNNo: $('#NTNNo').val(),
                GSTNo: $('#GSTNo').val(),
                Remarks: $('#Remarks').val(),
                CategoryCode: $('#CategoryCode').val(),
                GLCode: $('#GLCode').val(),
                // float fields (empty -> 0)
                Commission: toFloat($('#Commission').val()),
                VanSharing: toFloat($('#VanSharing').val()),
                GST: toFloat($('#GST').val()),
                SST: toFloat($('#SST').val()),
                WHT: toFloat($('#WHT').val()),
                OpeningBalance: $('#OpeningBalance').val(),
                OpeningDate: $('#OpeningDate').val(),
                PaymentTerm: $('#PaymentTerm').val(),
                IsActive: $('#IsActive').is(':checked'),
                CreatedBy: 'superadmin',
                BranchId: 1
            };

            // Convert to JSON string
            var jsonData = JSON.stringify(formData);

            // API call using JSON instead of form-urlencoded
            new APICALL(GetGlobalURL('Base', 'SaveParty'), 'POST', jsonData, true, false, 'application/json')
                .FETCH((result, error) => {

                    if (result && result.status === 'success') {
                        ViewData('0');
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

    $('#PartyName').val(row.PartyName);
    $('#Contact').val(row.Contact);
    $('#Address').val(row.Address);
    $('#Phone1').val(row.Phone1);
    $('#Phone2').val(row.Phone2);
    $('#Cell').val(row.Cell);
    $('#Fax').val(row.Fax);
    $('#Email').val(row.Email);

    $('#City').val(row.City);
    $('#PartyGroup').val(row.PartyGroup);
    $('#NTNNo').val(row.NTNNo);
    $('#GSTNo').val(row.GSTNo);

    $('#Remarks').val(row.Remarks);

    $('#Commission').val(row.Commission);
    $('#VanSharing').val(row.VanSharing);
    $('#GST').val(row.GST);
    $('#SST').val(row.SST);
    $('#WHT').val(row.WHT);

    $('#OpeningBalance').val(row.OpeningBalance);
    $('#OpeningDate').val(row.OpeningDate);
    $('#PaymentTerm').val(row.PaymentTerm);

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
                PartyId: $('#EditID').val() === "" ? 0 : parseInt($('#EditID').val()),
                PartyCode: $('#EditID').val() === "" ? 0 : parseInt($('#EditID').val()),
                PartyName: $('#PartyName').val(),
                Contact: $('#Contact').val(),
                Address: $('#Address').val(),
                Phone1: $('#Phone1').val(),
                Phone2: $('#Phone2').val(),
                Cell: $('#Cell').val(),
                Fax: $('#Fax').val(),
                Email: $('#Email').val(),
                City: $('#City').val(),
                PartyGroup: $('#PartyGroup').val(),
                NTNNo: $('#NTNNo').val(),
                GSTNo: $('#GSTNo').val(),
                Remarks: $('#Remarks').val(),
                CategoryCode: $('#CategoryCode').val(),
                GLCode: $('#GLCode').val(),
                // float fields (empty -> 0)
                Commission: toFloat($('#Commission').val()),
                VanSharing: toFloat($('#VanSharing').val()),
                GST: toFloat($('#GST').val()),
                SST: toFloat($('#SST').val()),
                WHT: toFloat($('#WHT').val()),
                OpeningBalance: $('#OpeningBalance').val(),
                OpeningDate: $('#OpeningDate').val(),
                PaymentTerm: $('#PaymentTerm').val(),
                IsActive: $('#IsActive').is(':checked'),
                CreatedBy: 'superadmin',
                BranchId: 1
            };

            // Convert object to JSON string
            var jsonData = JSON.stringify(formData);

            // Use fetch instead of APICALL to ensure proper JSON binding
            fetch(GetGlobalURL('Base', 'EditParty'), {
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
                        ViewData('0');
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
    var DeleteData = JSON.stringify({ PartyId: ID });

    Swal.fire({
        title: 'Do you want to save the changes?',
        showDenyButton: true,
        showCancelButton: false,
        confirmButtonText: 'Ok',
        denyButtonText: 'Cancel',
    }).then((result) => {
        if (result.isConfirmed) {
            ShowLoader('UserMasterDiv');

            new APICALL(GetGlobalURL('Base', 'DeleteParty'), 'POST', DeleteData, true).FETCH((result, error) => {
                var isDeleted = result.data;

                if (isDeleted) {
                    ViewData('0');

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

    var PartyName = $("#PartyName").val();

    if (PartyName == null || PartyName == 'string' || !PartyName.trim()) {
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

    $('#PartyName').val('');
    $('#Contact').val('');
    $('#Address').val('');
    $('#Phone1').val('');
    $('#Phone2').val('');
    $('#Cell').val('');
    $('#Fax').val('');
    $('#Email').val('');

    $('#City').val('');
    $('#PartyGroup').val('');
    $('#NTNNo').val('');
    $('#GSTNo').val('');

    $('#Remarks').val('');

    $('#Commission').val('');
    $('#VanSharing').val('');
    $('#GST').val('');
    $('#SST').val('');
    $('#WHT').val('');

    $('#OpeningBalance').val('0');
    // Set today's date dynamically
    SetTodayDate("OpeningDate");
    $('#PaymentTerm').val('Cash');

    $('#IsActive').prop('checked', true);

    $('.updatebutton').addClass('d-none');
    $('.savebutton').removeClass('d-none');
}
function DoEmptyFields() {
    $('#UserID').val('');
    $('#EditID').val('');

    $('#PartyName').val('');
    $('#Contact').val('');
    $('#Address').val('');
    $('#Phone1').val('');
    $('#Phone2').val('');
    $('#Cell').val('');
    $('#Fax').val('');
    $('#Email').val('');

    $('#City').val('');
    $('#PartyGroup').val('');
    $('#NTNNo').val('');
    $('#GSTNo').val('');

    $('#Remarks').val('');

    $('#Commission').val('');
    $('#VanSharing').val('');
    $('#GST').val('');
    $('#SST').val('');
    $('#WHT').val('');

    $('#OpeningBalance').val('0');
    // Set today's date dynamically
    SetTodayDate("OpeningDate");
    $('#PaymentTerm').val('Cash');

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
