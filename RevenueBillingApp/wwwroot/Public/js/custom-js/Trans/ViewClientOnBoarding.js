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
            new APICALL(GetGlobalURL('Base', 'GetViewAllClientForm'), 'GET', '', true).FETCH((result, error) => {
                if (result) {
                    $("#user-master").dataTable().fnDestroy();
                    $('#user-master tbody').html('');

                    //console.log(result);
                    if (result.data != null) {
                        users = result.data;

                        $.each(result.data, function (i, option) {
                            var primaryID = option.client_id;

                            var highValueClient = option.high_value_client
                                ? '<span class="badge bg-success">Yes</span>'
                                : '<span class="badge bg-secondary">No</span>';

                            var ContractBtn = option.contract_upload
                                ? '<td><a class="btn btn-sm btn-primary" target="_blank" href="/uploadedfiles/ClientDocs/' + option.contract_upload + '">Contract</a></td>'
                                : '<td>-</td>';

                            var NdaBtn = option.nda_upload
                                ? '<td><a class="btn btn-sm btn-info" target="_blank" href="/uploadedfiles/ClientDocs/' + option.nda_upload + '">NDA</a></td>'
                                : '<td>-</td>';

                            var ProposalBtn = option.proposal_upload
                                ? '<td><a class="btn btn-sm btn-success" target="_blank" href="/uploadedfiles/ClientDocs/' + option.proposal_upload + '">Proposal</a></td>'
                                : '<td>-</td>';


                            var UpdateBtn = (data_[0].AllowUpdate == true)
                                ? '<td><button class="avatar-text avatar-md EditData" data-value="' + primaryID + '" data-value1="' + i + '" type="button"><i class="feather-edit-2 edit-icon"></i></button></td>'
                                : '';

                            var DeleteBtn = (data_[0].AllowDelete == true)
                                ? '<td><button class="avatar-text avatar-md DeleteData" data-value="' + primaryID + '" type="button"><i class="feather-trash delete-icon"></i></button></td>'
                                : '';

                            $('#user-master tbody').append(
                                '<tr id="rowid-' + i + '">' +
                                /*'<td>' + option.client_id + '</td>' +*/
                                '<td>' + (option.client_name || '') + '</td>' +
                                '<td>' + (option.website || '') + '</td>' +
                                '<td>' + (option.billing_address || '') + '</td>' +
                                '<td>' + (option.tax_reg_no || '') + '</td>' +
                                '<td>' + (option.primary_contact_name || '') + '</td>' +
                                '<td>' + (option.primary_contact_email || '') + '</td>' +
                                '<td>' + (option.primary_contact_contact || '') + '</td>' +
                                '<td>' + highValueClient + '</td>' +
                                '<td>' + (option.priority_level || '') + '</td>' +
                                ContractBtn +
                                NdaBtn +
                                ProposalBtn +

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
        "/Trans/ClientOnBoarding?ClientId=" + id;
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

            new APICALL(GetGlobalURL('Base', 'DeleteClientForm'), 'POST', DeleteData, true).FETCH((result, error) => {
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
