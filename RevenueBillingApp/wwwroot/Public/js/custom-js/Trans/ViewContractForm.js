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
            new APICALL(GetGlobalURL('Base', 'GetViewAllContractForm'), 'GET', '', true).FETCH((result, error) => {
                if (result) {
                    $("#user-master").dataTable().fnDestroy();
                    $('#user-master tbody').html('');

                    //console.log(result);
                    if (result.data != null) {
                        users = result.data;

                        $.each(result.data, function (i, option) {
                            var primaryID = option.contract_id;

                            var UpdateBtn = (data_[0].AllowUpdate == true)
                                ? '<td><button class="avatar-text avatar-md EditData" data-value="' + primaryID + '" data-value1="' + i + '" type="button"><i class="feather-edit-2 edit-icon"></i></button></td>'
                                : '';

                            var DeleteBtn = (data_[0].AllowDelete == true)
                                ? '<td><button class="avatar-text avatar-md DeleteData" data-value="' + primaryID + '" type="button"><i class="feather-trash delete-icon"></i></button></td>'
                                : '';

                            $('#user-master tbody').append(
                                '<tr id="rowid-' + i + '">' +
                                /*'<td>' + option.contract_id + '</td>' +*/
                                '<td>' + (option.client_name || '') + '</td>' +
                                '<td>' + (option.contract_type || '') + '</td>' +
                                '<td>' + (option.contract_value || '') + '</td>' +
                                '<td>' + (option.currency_name || '') + '</td>' +
                                '<td>' + (option.project_name || '') + '</td>' +
                                '<td>' + (option.project_value || '0') + '</td>' +
                                '<td>' + (option.no_of_milestones || '0') + '</td>' +
                                '<td>' + (option.paymentterms_name || '') + '</td>' +
                                '<td>' + (option.penalty_terms || '') + '</td>' +
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
        "/Trans/ContractForm?ContractId=" + id;
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

            new APICALL(GetGlobalURL('Base', 'DeleteContractForm'), 'POST', DeleteData, true).FETCH((result, error) => {
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
