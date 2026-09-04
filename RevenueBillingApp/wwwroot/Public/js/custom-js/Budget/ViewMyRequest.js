var instanceid = 0;
$(document).ready(function () {
    instanceid = GetParameterValues('instanceid') ?? 0;
    LoadViewMyRequest(instanceid);
    getWorkflowLog(instanceid);
    document.addEventListener('click', function (e) {
        const card = e.target.closest('.flip-card');
        if (card && document.body.contains(card)) {
            card.classList.toggle('hover');
        }
    });
});
async function LoadViewMyRequest(instanceid) {
    await GetFormsByInstanceId($('#workflow').val(), instanceid, "appendForm", "View");
    GetBudgetDetailByInstanceId(instanceid);
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
