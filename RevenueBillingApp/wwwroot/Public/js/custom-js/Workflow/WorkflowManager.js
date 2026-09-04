var roles = {};
var data = [];
var lines = [];
$(document).ready(function () {
   
});
function GetFormsByInstanceId(wfcode, instanceid, FormId, formName) {
    return new Promise((resolve, reject) => {
        ShowLoader('workflowDiv');

        UTILITY.CheckSession((data_) => {
            if (data_) {
                new APICALL(GetGlobalURL('Base', 'GetFormsByInstanceId?wfcode=' + wfcode + '&instanceid=' + instanceid +'&formName=' + formName), 'GET', '', false)
                    .FETCH((result, error) => {
                        if (result) {
                            let htmlArr = JSON.parse(result.data);
                            if (htmlArr != null && htmlArr.length > 0) {
                                htmlArr.forEach(function (item) {
                                    if (item.sectionhtml) {
                                        let tempDiv = document.createElement('div');
                                        tempDiv.innerHTML = item.sectionhtml;
                                        let wrapper = document.createElement('div');
                                        wrapper.appendChild(tempDiv);
                                        document.getElementById(FormId).appendChild(wrapper);
                                        const isEnable = item?.isenable ?? item?.p?.[0]?.isenable;
                                        if (isEnable === false) {
                                            wrapper.querySelectorAll('input, select, textarea, button').forEach(el => {
                                                el.disabled = true;
                                            });
                                        }
                                    }
                                });
                            }
                            resolve(); // Finished
                        } else {
                            reject("Error loading form sections");
                        }
                    });
            } else {
                reject("Session invalid");
            }
        });
    });
}

function GetWorkflowAction(wfcode, instanceid,FormId) {
    ShowLoader('workflowDiv');

    UTILITY.CheckSession((data_) => {

        if (data_) {

            new APICALL(GetGlobalURL('Base', 'GetCurrentWorkflowAction?wfcode=' + wfcode + '&instanceid=' + instanceid), 'GET', '', false).FETCH((result, error) => {

                if (result) {
                    let wf_Step_actions = JSON.parse(result.data);
                    if (wf_Step_actions != null) {
                        $("#" + FormId).find('#actionId').html("")
                        const bootstrapClasses = ['btn-primary', 'btn-secondary', 'btn-success', 'btn-info', 'btn-warning', 'btn-danger', 'btn-light', 'btn-dark'];

                        $(wf_Step_actions).each(function (index, row) {
                            const btnClass = bootstrapClasses[index % bootstrapClasses.length];

                            $("#" + FormId).find('#actionId').append(
                                '<button class="btn ' + btnClass + ' btn-sm savebutton me-2 mb-2" type="button" data-assignmenttype="' + row.assignmenttype + '" data-dynamicfunction="' + row.dynamicfunction + '" data-save="' + row.issave + '" data-move="' + row.ismove + '" data-id="' + row.id + '">' +
                                '<i class="feather-save me-2"></i>' + row.actionname +
                                '</button>'
                            );
                        });
                    }
                    HideLoader('workflowDiv');
                }
            });
        }
    });
}

function getWorkflowLog(instanceid) {
    UTILITY.CheckSession((data_) => {
        if (data_) {
            new APICALL(GetGlobalURL('Base', 'GetWorkflowLog?instanceid=' + instanceid), 'GET', '', false).FETCH((result, error) => {
                if (result) {
                    if (result.data != "") {
                        data = JSON.parse(result.data);
                        generateSteps(data);
                    }
                }
            });
        }
    });
}
function GetParameterValues(param) {
    var url = window.location.href.slice(window.location.href.indexOf('?') + 1).split('&');
    for (var i = 0; i < url.length; i++) {
        var urlparam = url[i].split('=');
        if (urlparam[0] == param) {
            return urlparam[1];
        }
    }
}
function generateSteps(data) {
    var card = '';
    for (let i = 0; i < data.length; i++) {
        const currentElement = data[i];
        const nextElement = data[i + 1]; 
        var stage = "Sequence No " + currentElement.id;
        var taskPerform = currentElement.assignedTo;
        if (currentElement.stepTitle == "Rejected") {
            if (nextElement != undefined) {
                taskPerform = "Reinitiated  By " + currentElement.assignedTo;
            }
            else {
                taskPerform = '-'
            }
        }

        card += '<div class="history-card" id="history-card-' + currentElement.id + '">' +
            '<div class="flip-card" data-id="' + currentElement.id + '">' + 
            '<div class="flip-card-inner">' +
            '<div class="flip-card-front ' + currentElement.status + '">' +
            '<div class="card-front__tp card-front_' + currentElement.status + ' ">' +
            '<i class="fa-solid ' + currentElement.icon + '"></i>' +
            '<p class="card-front__heading">' + currentElement.stepTitle + '</p>' +
            '<p class="card-front__text-price">' + taskPerform + '</p>' +
            '</div>' +
            '<div class="card-front__bt">' +
            '<p class="card-front__text-view card-front__text-view--camping"> ' + stage + '</p>' +
            '</div>' +
            '</div>' +
            '<div class="flip-card-back ' + currentElement.status + '">' +
            '<p class="inside-page__heading inside-page__heading--camping">Assigned On</p>' +
            '<p class="inside-page__text">' + currentElement.assignedOn + '</p>' +
            '<p class="inside-page__heading inside-page__heading--camping">Performed On</p>' +
            '<p class="inside-page__text">' + currentElement.completedOn + '</p>' +
            '<p class="inside-page__heading inside-page__heading--camping">Comments</p>' +
            '<p class="inside-page__text">' + currentElement.comments + '</p>' +
            '</div>' +
            '</div>' +
            '</div>' +
            '</div>';
    };
    $('.flow-container').html(card);
}



function drawLines(length) {
    for (let i = 1; i < length; i++) {

        var start = document.getElementById('history-card-' + i);
        var end = document.getElementById('history-card-' + (i + 1));

        var startSocket = 'right';
        var endSocket = 'left';

        if (start.getBoundingClientRect().top != end.getBoundingClientRect().top) {
            startSocket = 'bottom';
            endSocket = 'top';
        }

        lines.push(new LeaderLine(start, end, {
            color: '#00a650', size: 2, path: 'grid',
            startSocket: startSocket,
            endSocket: endSocket,
            startSocketGravity: 10,
            endSocketGravity: 10,
        }));
    }
}

function renderLines() {
    lines.forEach(line => {
        line.remove();
        lines = [];
    });

    if ($('.flow-container').height() > 0) {
        drawLines(data.length);
    }
}

elem = $(".main-content")[0];
let resizeObserver = new ResizeObserver(() => {
    renderLines();
});
resizeObserver.observe(elem);
