var instanceid = 0;
$(document).ready(function () {
    instanceid = GetParameterValues('instanceid') ?? 0;
    LoadViewMyRequest(instanceid);
    $("#intiateform").on("click", ".savebutton", function () {
        mynextsetp(this);
    });
    document.addEventListener('click', function (e) {
        const card = e.target.closest('.flip-card');
        if (card && document.body.contains(card)) {
            card.classList.toggle('hover');
        }
    });
});
async function LoadViewMyRequest(instanceid) {
    await GetFormsByInstanceId($('#workflow').val(), instanceid, "appendForm", "intiate");
    GetAllUsers_DDL();
    GetBudgetDetailByInstanceId(instanceid);
    getWorkflowLog(instanceid);
    GetWorkflowAction($('#workflow').val(), instanceid, "intiateform");
    $("#btnAddSelfAssign").on("click", function () {
        SaveSelfAssignUser();
    });
    GetSelfAutoAssignedGridData();
}

function mynextsetp(btn) {
    var save = $(btn).attr('data-save') === "true";
    var move = $(btn).attr('data-move') === "true";
    var actionId = $(btn).attr('data-id');
    var assignmenttype = $(btn).attr('data-assignmenttype');
    var dynamicfunction = $(btn).attr('data-dynamicfunction');

    if (save && move) {
        Swal.fire({
            title: 'Do you want to save?',
            showDenyButton: true,
            showCancelButton: false,
            confirmButtonText: 'Yes',
            denyButtonText: 'No',
        }).then((result) => {
            if (result.isConfirmed) {
                var Data = $('#intiateform').serialize() + '&instanceid=' + encodeURIComponent(instanceid);
                new APICALL(GetGlobalURL('Base', 'SaveInitiateRequest'), 'POST', Data, false, false, 'application/x-www-form-urlencoded').FETCH((result, error) => {
                    if (result) {
                        var workflowMove = {
                            instanceid: result.data[0].instanceid,
                            actionid: actionId,
                            dynamicfunction: dynamicfunction,
                            assignmenttype: assignmenttype,
                            comment: $("#remarks").val()
                        };
                        var urlEncodedData = Object.keys(workflowMove)
                            .map(key => encodeURIComponent(key) + '=' + encodeURIComponent(workflowMove[key] ?? ''))
                            .join('&');

                        new APICALL(GetGlobalURL('Base', 'MoveWorkflow'), 'POST', urlEncodedData, false, false, 'application/x-www-form-urlencoded').FETCH((result, error) => {
                            if (result) {
                                Swal.fire({
                                    icon: 'success',
                                    title: 'Success...',
                                    text: 'Workflow moved successfully!',
                                    timer: 2000,
                                    showConfirmButton: false
                                }).then(() => {
                                    const defaultForm = document.getElementById("defaultform").value;
                                    window.location.href = "/" + defaultForm;
                                });
                            }
                            if (error) {
                                Swal.fire({
                                    icon: 'error',
                                    title: 'Error...',
                                    text: error.data.responseText,
                                });
                            }
                        });

                    }
                    if (error) {
                        Swal.fire({
                            icon: 'error',
                            title: 'Error...',
                            text: error.data.responseText,
                        });
                    }
                });
            }
        });

    }
    else if (save) {
        SaveInitiateRequest();
    }
    else if (move) {
        MoveMyRequest(actionId, dynamicfunction, assignmenttype);
    }
}
function SaveInitiateRequest() {


    Swal.fire({
        title: 'Do you want to save?',
        showDenyButton: true,
        showCancelButton: false,
        confirmButtonText: 'Yes',
        denyButtonText: 'No',
    }).then((result) => {
        if (result.isConfirmed) {

            var Data = $('#intiateform').serialize();
            new APICALL(GetGlobalURL('Base', 'SaveInitiateRequest'), 'POST', Data, false, false, 'application/x-www-form-urlencoded').FETCH((result, error) => {

                if (result) {
                    Swal.fire({
                        icon: 'success',
                        title: 'Success...',
                        text: 'Saved Successfully!',
                        timer: 2000,
                        showConfirmButton: false
                    }).then(() => {
                        const defaultForm = document.getElementById("defaultform").value;
                        window.location.href = "/" + defaultForm;
                    });
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
function MoveMyRequest(actionId, dynamicfunction, assignmenttype) {

    Swal.fire({
        title: 'Do you want to move the workflow?',
        showDenyButton: true,
        showCancelButton: false,
        confirmButtonText: 'Yes',
        denyButtonText: 'No',
    }).then((result) => {
        if (result.isConfirmed) {
            var workflowMove = {
                instanceid: instanceid,
                actionid: actionId,
                dynamicfunction: dynamicfunction,
                assignmenttype: assignmenttype,
                comment: $("#remarks").val()
            };
            var urlEncodedData = Object.keys(workflowMove)
                .map(key => encodeURIComponent(key) + '=' + encodeURIComponent(workflowMove[key] ?? ''))
                .join('&');

            new APICALL(GetGlobalURL('Base', 'MoveWorkflow'), 'POST', urlEncodedData, false, false, 'application/x-www-form-urlencoded').FETCH((result, error) => {
                if (result) {
                    Swal.fire({
                        icon: 'success',
                        title: 'Success...',
                        text: 'Workflow moved successfully!',
                        timer: 2000, 
                        showConfirmButton: false
                    }).then(() => {
                        const defaultForm = document.getElementById("defaultform").value;
                        window.location.href = "/" + defaultForm;
                    });
                }
                if (error) {
                    Swal.fire({
                        icon: 'error',
                        title: 'Error...',
                        text: error.data.responseText,
                    });
                }
            });
        }
    });
}
function GetBudgetDetailByInstanceId(instanceid) {
    new APICALL(GetGlobalURL('Base', 'GetBudgetDetailByInstanceId') + '?instanceid=' + instanceid, 'GET', '', false).FETCH((result, error) => {

        if (result) {
            if (result.data != null && result.data.length > 0 ) {

                try {
                    const data = result.data;

                    $('#Department').val(data[0].dept ?? '');
                    $('#Purpose').val(data[0].purpose ?? '');
                    $('#EstimatedAmount').val(data[0].estimatedamount ?? '');
                    let date = new Date(data[0].requiredby);
                    let formatted = date.toISOString().split('T')[0];
                    $('#RequiredBy').val(formatted);
                    $('#Priority').val(data[0].priority ?? '');
                }
                catch (ex) {
                    console.error(ex);
                };

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

function DoEmptyFields() {
    $('#Department').val('');
    $('#Purpose').val('');
    $('#EstimatedAmount').val('');
    $('#RequiredBy').val('');
    $('#Priority').val('');
    $('#bloombergcode').val('');
}
function GetAllUsers_DDL() {
    new APICALL(GetGlobalURL('Base', 'GetAllUsers'), 'GET', '', true).FETCH((result, error) => {

        if (result) {
            if (result.data != null) {

                window.allUsers = result.data;

                populateDropdowns(result.data);
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
function populateDropdowns(users, selectedManagerId = null) {
    $('#userid').empty().append('<option value="">-- Select User --</option>');

    $.each(users, function (i, option) {
        if (option.userid != selectedManagerId) {
            $('#userid').append(
                `<option value="${option.userid}">${option.username}</option>`
            );
        }
    });
}

function SaveSelfAssignUser() {
    Swal.fire({
        title: 'Do you want to save?',
        showDenyButton: true,
        showCancelButton: false,
        confirmButtonText: 'Yes',
        denyButtonText: 'No',
    }).then((result) => {
        if (result.isConfirmed) {

            var Data = $('#selfAssignedForm').serialize() + '&instanceid=' + encodeURIComponent(instanceid);
            new APICALL(GetGlobalURL('Base', 'SaveSelfAssignUser'), 'POST', Data, false, false, 'application/x-www-form-urlencoded').FETCH((result, error) => {

                if (result) {
                    GetSelfAutoAssignedGridData();
                    Swal.fire({
                        icon: 'success',
                        title: 'Success...',
                        text: 'Saved Successfully!',
                        timer: 2000,
                        showConfirmButton: false
                    })
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

function GetSelfAutoAssignedGridData() {

        new APICALL(GetGlobalURL('Base', 'GetSelfAssignUser') + '?instanceid=' + instanceid, 'GET', '', false).FETCH((result, error) => {
        if (result) {
            if (result.data != null) {
                populateSelfAutoAssignedGrid(result.data);
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

function populateSelfAutoAssignedGrid(datasource) {


    $('#gridSelfAutoAssigned').DataTable().destroy();
    $("#gridSelfAutoAssigned").DataTable({
        "responsive": true,
        "autoWidth": false,
        "aaData": datasource,
        "columns": [
            { data: "orderby" },
            { data: "UserName" },
            {
                "orderable": false,
                "data": null,
                render: function (data, type, row) {
                    if (type === 'display') {
                        var column = '<div class="btn-group btn-group-sm">' +
                            '<a class="btn btn-danger delete-btn" data-id="' + data.id + '" data-instanceid="' + data.instanceid +'" id="deletebtn"><i class="fas fa-trash-alt pr-2"></i></a>' +
                            '</div >'
                        var html = column;
                        return html;
                    }
                    return data;
                },
            },
        ]
    });

}

$(document).on('click', '.delete-btn', function () {
    var id = $(this).data('id');
    var instanceid = $(this).data('instanceid');
    Swal.fire({
        title: 'Are you sure?',
        text: "You won't be able to revert this!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes, delete it!'
    }).then((result) => {
        if (result.isConfirmed) {
            new APICALL(GetGlobalURL('Base', 'DeleteSelfAssignUser') + '?primaryid=' + id + '&instanceid=' + instanceid, 'GET', '', false)
                .FETCH((response, error) => {
                    if (response) {
                        if (response.data && response.status == 'success') {
                            Swal.fire(
                                'Deleted!',
                                'The user has been deleted successfully.',
                                'success'
                            );
                            var table = $('#gridSelfAutoAssigned').DataTable();
                            table.row($(this).closest('tr')).remove().draw();
                        } else {
                            Swal.fire(
                                'Failed!',
                                response.data.message || 'There was an issue while deleting the user.',
                                'error'
                            );
                        }
                    } else if (error) {
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
});
