var users = {};

$(document).ready(function () {
    ViewData();

    $('.EditData').on('click', function () {
        EditData($(this).attr('data-value'));
    });

});
function ViewData() {
    ShowLoader('UserMasterDiv');
    UTILITY.CheckSession((data_) => {
        if (data_) {
            new APICALL(GetGlobalURL('Base', 'GetViewAllPaymentForm'), 'GET', '', true).FETCH((result, error) => {
                if (result) {
                    $("#user-master").dataTable().fnDestroy();
                    $('#user-master tbody').html('');

                    //console.log(result);
                    if (result.data != null) {
                        users = result.data;

                        $.each(result.data, function (i, option) {
                            var primaryID = option.payment_id;

                            var highValueClient = option.high_value_client
                                ? '<span class="badge bg-success">Yes</span>'
                                : '<span class="badge bg-secondary">No</span>';

                            var ReceiptBtn = option.receipt_upload
                                ? '<td><a class="btn btn-sm btn-primary" target="_blank" href="/uploadedfiles/PaymentDocs/' + option.receipt_upload + '">Receipt</a></td>'
                                : '<td>-</td>';

                            var UpdateBtn = (data_[0].AllowUpdate == true)
                                ? '<td><button class="avatar-text avatar-md EditData" data-value="' + primaryID + '" data-value1="' + i + '" type="button"><i class="feather-edit-2 edit-icon"></i></button></td>'
                                : '';

                            var DeleteBtn = (data_[0].AllowDelete == true)
                                ? '<td><button class="avatar-text avatar-md DeleteData" data-value="' + primaryID + '" type="button"><i class="feather-trash delete-icon"></i></button></td>'
                                : '';

                            $('#user-master tbody').append(
                                '<tr id="rowid-' + i + '">' +
                                    '<td>' + (option.client_name || '') + '</td>' +
                                    '<td>' + (option.invoice_name || '') + '</td>' +
                                    '<td class="text-end">' + (option.amount_received || 0) + '</td>' +
                                    '<td>' + (option.payment_mode || '') + '</td>' +
                                    '<td>' + (option.chq_no || '-') + '</td>' +
                                    '<td>' + (option.bank_name || '-') + '</td>' +
                                    '<td>' + (formatDate(option.value_date) || '-') + '</td>' +
                                    ReceiptBtn +
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
                    $('#user-master').DataTable({
                        scrollX: true,
                        autoWidth: false
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
function EditData(id) {
    window.location.href =
        "/Trans/PaymentForm?PaymentId=" + id;
}
function DeleteData(ID) {
    var DeleteData = JSON.stringify({ PrimaryID: ID });

    Swal.fire({
        title: 'Do you want to save the changes?',
        showDenyButton: true,
        showCancelButton: false,
        confirmButtonText: 'Ok',
        denyButtonText: 'Cancel',
    }).then((result) => {
        if (result.isConfirmed) {
            ShowLoader('UserMasterDiv');

            new APICALL(GetGlobalURL('Base', 'DeletePaymentForm'), 'POST', DeleteData, true).FETCH((result, error) => {
                console.log(result);

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
// Format function
function formatDate(dateString) {
    if (!dateString) return '';
    var date = new Date(dateString);
    var day = ("0" + date.getDate()).slice(-2);
    var month = ("0" + (date.getMonth() + 1)).slice(-2);
    var year = date.getFullYear();
    return day + '-' + month + '-' + year;  // format: dd-MM-yyyy
}

//function formatDate2(dateString, format) {
//    if (!dateString) return '';

//    var date = new Date(dateString);
//    if (isNaN(date.getTime())) return '';

//    var day = ("0" + date.getDate()).slice(-2);
//    var monthIndex = date.getMonth();
//    var year = date.getFullYear();

//    var monthsShort = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];

//    if (format === 'DD-MM-YYYY') {
//        return day + '-' + ("0" + (monthIndex + 1)).slice(-2) + '-' + year;
//    }

//    // default DD-MMM-YYYY
//    return day + '-' + monthsShort[monthIndex] + '-' + year;
//}
