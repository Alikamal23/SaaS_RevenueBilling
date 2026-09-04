
let myTask, overDueTask, myRequest, overdueRequest, myApproval, percentageRound;
$(document).ready(function () {

    createMainSlides();
    LoadRows(GetMyTasksByUserId, 'My Tasks');
    $(".task-progress-1").circleProgress({
        max: 100,
        value: CalculatePercentage(myTask, overDueTask),
        textFormat: function () {
            return CalculatePercentage(myTask, overDueTask)
        }
    }), $(".task-progress-2").circleProgress({
        max: 100,
        value: CalculatePercentage(overDueTask, myTask),
        textFormat: function () {
            return CalculatePercentage(overDueTask, myTask)
        }
    }), $(".task-progress-3").circleProgress({
        max: 100,
        value: CalculatePercentage(myRequest, overdueRequest),
        textFormat: function () {
            return CalculatePercentage(myRequest, overdueRequest)
        }
    }), $(".task-progress-4").circleProgress({
        max: 100,
        value: CalculatePercentage(overdueRequest, myRequest),
        textFormat: function () {
            return CalculatePercentage(overdueRequest, myRequest)
        }
    }), $(".task-progress-5").circleProgress({
        max: 100,
        value: CalculatePercentage(myApproval, 0),
        textFormat: function () {
            return CalculatePercentage(myApproval, 0)
        }
    })

});

var globalPk_Id;

function createMainSlides() {
    $('#item-container').html('');

    var slideNames = ['My Tasks', 'Overdue Tasks', 'My Requests', 'Overdue Requests', 'My Approvals'];
    var classes = ['my-tasks', 'overdue', 'requests', 'overdue-tasks', 'approvals'];
    const funcsArr = [GetMyTasksByUserId, GetMyOverdueTasksByUserId, GetMyRequestsByUserId, GetMyOverdueRequestsByUserId, GetMyApprovalsDashboard];

    const funcMap = {
        'GetMyTasksByUserId': GetMyTasksByUserId,
        'GetMyOverdueTasksByUserId': GetMyOverdueTasksByUserId,
        'GetMyRequestsByUserId': GetMyRequestsByUserId,
        'GetMyOverdueRequestsByUserId': GetMyOverdueRequestsByUserId,
        'GetMyApprovalsDashboard': GetMyApprovalsDashboard,
    };

    var mainSlides = '';
    for (let i = 0; i < funcsArr.length; i++) {

        const func = funcsArr[i];

        var nestedSlides = func();

        switch (slideNames[i]) {
            case "My Tasks":
                myTask = nestedSlides.length;
                break;
            case "Overdue Tasks":
                overDueTask = nestedSlides.length;
                break;
            case "My Requests":
                myRequest = nestedSlides.length;
                break;
            case "Overdue Requests":
                overdueRequest = nestedSlides.length;
                break;
            case "My Approvals":
                myApproval = nestedSlides.length;
                break;
            default:
                break;
        }

        mainSlides +=
            `<div class="row">
                <div class='col'>
                    <div class="card overflow-hidden bg-color-${i % 5}">
                        <div class="card-body">
                            <div class="hstack justify-content-between">
                                <div>
                                    <i class="feather-users fs-20"></i>
                                </div>
                                <div class="text-end">
                                    <a class="custom-btn load-rows-btn" 
                                       data-func="${func.name}" 
                                       data-slide-name="${slideNames[i]}"
                                       data-bs-toggle="tooltip" data-bs-placement="bottom" title="View">
                                        <i class="feather-eye"></i>
                                    </a>
                                </div>
                            </div>
                            <h5 class="fs-4 text-reset mt-4 mb-1">
                                <span class="counter">${nestedSlides.length}</span>
                            </h5>
                            <div class="fs-16 text-reset fw-bold">${slideNames[i]}</div>
                        </div>
                    </div>
                </div>
            </div>`;
    }

    $('#item-container').append(mainSlides);

    document.querySelectorAll('.load-rows-btn').forEach(button => {
        button.addEventListener('click', () => {
            const funcName = button.getAttribute('data-func');
            const slideName = button.getAttribute('data-slide-name');
            const func = funcMap[funcName];
            if (func) {
                LoadRows(func, slideName);
            } else {
                console.error('Function not found for:', funcName);
            }
        });
    });
}

function GetMyTasksByUserId() {
    let slides = '';

    new APICALL(GetGlobalURL('Base', 'GetDailyTasks'), 'GET', '', false).FETCH((result, error) => {

        if (result) {
            try {
                if (result.data) {
                    slides = renderNestedSlides(result.data);
                }
            } catch (ex) {
                console.error('Exception while rendering slides:', ex);
            }
        }

        if (error) {
            console.error('API error:', error);
        }
    });

    return slides;
}

function GetMyOverdueTasksByUserId() {
    let slides = '';

    new APICALL(GetGlobalURL('Base', 'GetDailyOverdueTasks'), 'GET', '', false).FETCH((result, error) => {

        if (result) {
            try {
                if (result.data) {
                    slides = renderNestedSlides(result.data);
                }
            } catch (ex) {
                console.error('Exception while rendering slides:', ex);
            }
        }

        if (error) {
            console.error('API error:', error);
        }
    });

    return slides;
}

function GetMyRequestsByUserId() {
    let slides = '';

    new APICALL(GetGlobalURL('Base', 'GetDailyMyRequests'), 'GET', '', false).FETCH((result, error) => {

        if (result) {
            try {
                if (result.data) {
                    slides = renderNestedSlides(result.data);
                }
            } catch (ex) {
                console.error('Exception while rendering slides:', ex);
            }
        }

        if (error) {
            console.error('API error:', error);
        }
    });

    return slides;
}
function GetMyOverdueRequestsByUserId() {
    let slides = '';

    new APICALL(GetGlobalURL('Base', 'GetDailyOverdueMyRequests'), 'GET', '', false).FETCH((result, error) => {

        if (result) {
            try {
                if (result.data) {
                    slides = renderNestedSlides(result.data);
                }
            } catch (ex) {
                console.error('Exception while rendering slides:', ex);
            }
        }

        if (error) {
            console.error('API error:', error);
        }
    });

    return slides;
}
function GetMyApprovalsDashboard() {
    let slides = '';

    new APICALL(GetGlobalURL('Base', 'GetDailyMyApprovals'), 'GET', '', false).FETCH((result, error) => {

        if (result) {
            try {
                if (result.data) {
                    slides = renderNestedSlides(result.data);
                }
            } catch (ex) {
                console.error('Exception while rendering slides:', ex);
            }
        }

        if (error) {
            console.error('API error:', error);
        }
    });

    return slides;
}



function renderNestedSlides(data) {
    var slides = '';
    for (var i = 0; i < data.length; i++) {
        var slide = ` <div class="item--nested">
                <div class="row">
                    <div class="col-12">
                        <p class="mb-0 wf-name">Doc ID</p>
                        <p class="mb-0 wf-status">: `+ data[i].id + `</p>
                    </div>
                    <div class="col-12">
                        <p class="mb-0 wf-name">Workflow</p>
                        <p class="mb-0 wf-status">: `+ data[i].wfcode + `</p>
                    </div>
                    <div class="col-12">
                        <p class="mb-0 wf-name">Status</p>
                        <p class="mb-0 wf-status">: `+ data[i].status + `</p>
                    </div>
                </div>
            </div>`;

        slides += slide;
    }

    return { slides: slides, length: data.length, data: data };
}

function LoadRows(apiFucntion, heading) {

    const func = apiFucntion;

    var nestedSlides = func();

    var data2 = nestedSlides.data;
    
    $('#gridTaskContainer').DataTable().destroy();

    $("#gridTaskContainer").DataTable({
        "responsive": true,
        "autoWidth": false,
        "order": [],
        "aaData": data2,
        "columns": [

            { data: "id" },
            { data: "wfcode" },
            { data: "createdby" },
            { data: "editdate" },
            {
                "orderable": false,
                "data": null,
                render: function (data, type, row) {

                    if (type === 'display') {

                        var column1 =
                            "<span class='badge bg-soft-success text-success'>" + data.status +"</span>"

                        return column1;
                    }
                    return data;
                },
            },
            {
                "orderable": false,
                "data": null,

                render: function (data, type, row) {

                    if (type === 'display') {

                        var column = '<td><div class="btn-group btn-group-sm"><a class="avatar-text avatar-md EditUser" title="View" href="' + data.viewpage + '"><i class="fa-solid fa-eye"></i></a></div></td>';

                        return column;
                    }
                    return data;
                },
            },
        ],
        "columnDefs": [{
            "targets": 'no-sort',
            "orderable": false,
        },
        {
            "targets": 3,
            "render": function (data, type, row, meta) {
                if (!data) return '';
                const m = moment(data, 'DD-MM-YYYY'); // <-- tell moment the format
                return m.isValid() ? m.format('DD-MM-YYYY  hh:mm:ss a') : data;
            }
        },


        ]
    });


    $('.card-header').html('<h5 class="card-title">' + heading + '</h5>');

    initializeTooltips();
}

function initializeTooltips() {
    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.forEach(tooltipTriggerEl => {
        const existingTooltip = bootstrap.Tooltip.getInstance(tooltipTriggerEl);
        if (existingTooltip) {
            existingTooltip.dispose();
        }
        new bootstrap.Tooltip(tooltipTriggerEl);
    });
}

function CalculatePercentage(firstVal, secondVal) {
    if (firstVal == 0 && secondVal == 0) {
        percentageRound = 0;
        return 0 + "%";
    }

    var sum = firstVal + secondVal;
    var percentage = (firstVal / sum) * 100;
    percentageRound = Math.round(percentage);
    return Math.round(percentage) + "%";
}