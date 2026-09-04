var wfcode = 0;
$(document).ready(function () {
    wfcode = GetParameterValues('wfcode') ?? "0";
    GetMyRequest(wfcode);
});


function GetMyRequest(wfcode) {
    new APICALL(GetGlobalURL('Base', 'MyApproval') + '?wfcode=' + wfcode, 'GET', '', true).FETCH((result, error) => {

        if (result) {

            $('#myApproval tbody').html('');
            if (result.data != null) {
                roles = result.data;
                $.each(result.data, function (i, option) {
                    var viewBtn = '<td><div class="btn-group btn-group-sm"><a class="avatar-text avatar-md EditUser" title="View" href="' + option.viewpage +'"><i class="fa-solid fa-eye"></i></a></div></td>';
                    $('#myApproval tbody').append(
                        '<tr id="rowid-' + i + '">' +
                        '<td>' + option.docid + '</td>' +
                        '<td>' + option.currentstep + '</td>' +
                        '<td>' + option.lastupdatedby + '</td>' +
                        '<td>' + option.lastupdatedon + '</td>' +
                        '<td>' + option.createdby + '</td>' +
                        viewBtn +
                        '</tr>'
                    );
                });
            }
            $('#workflow-master').DataTable();
            HideLoader('workflowDiv');
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
