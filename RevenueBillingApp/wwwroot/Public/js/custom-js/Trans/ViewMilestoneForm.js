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
            new APICALL(GetGlobalURL('Base', 'GetViewAllMilestoneForm'), 'GET', '', true).FETCH((result, error) => {
                if (result) {
                    $("#user-master").dataTable().fnDestroy();
                    $('#user-master tbody').html('');

                    //console.log(result);
                    if (result.data != null) {
                        users = result.data;

                        $.each(result.data, function (i, option) {
                            var primaryID = option.milestone_id;

                            // =========================
                            // DOCUMENT LINK
                            // =========================
                            var DocBtn = option.docUpload
                                ? `<a class="btn btn-sm btn-primary" target="_blank" 
                                    href="/uploadedfiles/MilestoneDocs/${option.docUpload}">
                                    View File
                                   </a>`
                                : '-';

                            // =========================
                            // ACTION BUTTONS
                            // =========================
                            var UpdateBtn = (data_[0].AllowUpdate == true)
                                ? `<button class="avatar-text avatar-md EditData"
                                        data-value="${primaryID}"
                                        data-value1="${i}">
                                        <i class="feather-edit-2 edit-icon"></i>
                                   </button>`
                                : '';

                            var DeleteBtn = (data_[0].AllowDelete == true)
                                ? `<button class="avatar-text avatar-md DeleteData"
                                        data-value="${primaryID}">
                                        <i class="feather-trash delete-icon"></i>
                                   </button>`
                                : '';

                            // =========================
                            // TABLE ROW
                            // =========================
                            $('#user-master tbody').append(
                                `<tr id="rowid-${i}">
                                    <td>${option.client_name || ''}</td>
                                    <td>${option.contract_name || ''}</td>

                                    <td>${option.milestone_name || ''}</td>
                                    <td>${option.milestone_amount || 0}</td>

                                    <td>${option.exp_completion_date ? option.exp_completion_date.split('T')[0] : ''}</td>
                                    <td>${option.completion_date ? option.completion_date.split('T')[0] : ''}</td>

                                    <td>${option.internal_owner || ''}</td>
                                    <td>${option.client_approval || ''}</td>

                                    <td>${option.client_comments}</td>
                                    <td>${option.qa_comments}</td>

                                    <td>${DocBtn}</td>

                                    <td>${UpdateBtn}</td>
                                    <td>${DeleteBtn}</td>

                                </tr>`
                            );
                        });

                        // =========================
                        // EVENTS
                        // =========================
                        $('.EditData').on('click', function () {
                            EditData(
                                this.getAttribute("data-value"),
                                this.getAttribute("data-value1")
                            );
                        });

                        $('.DeleteData').on('click', function () {
                            DeleteData(this.getAttribute("data-value"));
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
        "/Trans/MilestoneForm?MilestoneId=" + id;
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

            new APICALL(GetGlobalURL('Base', 'DeleteMilestoneForm'), 'POST', DeleteData, true).FETCH((result, error) => {
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
