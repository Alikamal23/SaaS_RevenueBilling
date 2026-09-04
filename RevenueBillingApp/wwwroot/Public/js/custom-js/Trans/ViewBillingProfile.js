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
            new APICALL(GetGlobalURL('Base', 'GetViewAllBillingForm'), 'GET', '', true).FETCH((result, error) => {
                if (result) {
                    $("#user-master").dataTable().fnDestroy();
                    $('#user-master tbody').html('');

                    //console.log(result);
                    if (result.data != null) {
                        users = result.data;

                        $.each(result.data, function (i, option) {
                            var primaryID = option.billing_id;

                            var UpdateBtn = (data_[0].AllowUpdate == true)
                                ? '<td><button class="avatar-text avatar-md EditData" data-value="' + primaryID + '" data-value1="' + i + '" type="button"><i class="feather-edit-2 edit-icon"></i></button></td>'
                                : '';

                            var DeleteBtn = (data_[0].AllowDelete == true)
                                ? '<td><button class="avatar-text avatar-md DeleteData" data-value="' + primaryID + '" type="button"><i class="feather-trash delete-icon"></i></button></td>'
                                : '';

                            var billingfreq = '';
                            if (option.billing_freq_id === '0')
                                billingfreq = '-'
                            else 
                                billingfreq = option.bfreq_name;


                            $('#user-master tbody').append(
                                '<tr id="rowid-' + i + '">' +
                                /*'<td>' + option.billing_id + '</td>' +*/
                                '<td>' + (option.client_name || '') + '</td>' +
                                '<td>' + (option.contract_name || '') + '</td>' +
                                '<td>' + (option.billing_type || '') + '</td>' +
                                '<td>' + (billingfreq || '') + '</td>' +
                                '<td>' + (formatDate(option.next_billing_date) || '') + '</td>' +
                                '<td>' + (option.billing_method || '') + '</td>' +
                                '<td>' + (option.delivery_method || '') + '</td>' +
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
        "/Trans/BillingProfile?BillingId=" + id;
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

            new APICALL(GetGlobalURL('Base', 'DeleteBillingForm'), 'POST', DeleteData, true).FETCH((result, error) => {
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
