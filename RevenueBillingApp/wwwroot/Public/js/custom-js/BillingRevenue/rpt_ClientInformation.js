$(document).ready(function () {
    var ClientId = new URLSearchParams(window.location.search).get("ClientId");
    ////alert(ClientId);

    SetPrintDate();

    LoadReport(ClientId);

    $('#btnPrint').on('click', PrintReport);

    $('#btnPDF').on('click', DownloadPDF);

});

function SetPrintDate() {
    var today = new Date();

    var options = {
        day: '2-digit',
        month: 'short',
        year: 'numeric'
    };

    $('#PrintDate').text(today.toLocaleDateString('en-GB', options));

}
function PrintReport() {
    window.print();
}
function LoadReport(clientId) {
    new APICALL(GetGlobalURL('Base', 'LoadClientReport') + '?clientId=' + clientId, 'GET', '', true)
        .FETCH((result, error) => {

            if (result && result.data) {
                //console.log('inside if block ....');
                //console.log(result);

                //console.log(client);
                //console.log(contracts);
                //console.log(milestones);

                var client = result.data.Table;
                var contracts = result.data.Table1;
                var milestones = result.data.Table2;

                BindClient(client);
                BindContracts(contracts, milestones);

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
function BindClient(client) {
    if (client.length == 0)
        return;

    var c = client[0];

    $('#ClientName').text(c.client_name);
    $('#ClientRefNo').text(c.client_refno);
    $('#Industry').text(c.IndustryName);
    $('#CompanySize').text(c.CompanySizeName);
    $('#Website').text(c.website);
    $('#Country').text(c.countryname);
    $('#BillingAddress').text(c.billing_address);
    $('#PrimaryContact').text(c.primary_contact_name);
    $('#PrimaryPhone').text(c.primary_contact_phone);
    $('#PrimaryEmail').text(c.primary_contact_email);
    $('#TaxRegistration').text(c.tax_registration_no);
    $('#Priority').text(c.priority_level);
    $('#HighValue').text(c.high_value_client ? "Yes" : "No");

}
function BindContracts(contracts, milestones) {
    $('#ContractContainer').html('');

    $.each(contracts, function (i, contract) {
        var html = '';

        html += '<div class="card report-card mt-4">';
        html += '<div class="card-header">';
        html += 'Contract #' + (i + 1);

        if (contract.basic_contract_type == 1)
            html += ' (SaaS)';
        else
            html += ' (Custom)';

        html += '</div>';

        html += '<div class="card-body">';
        html += '<table class="table table-bordered">';
        html += '<tr>';
        html += '<th>Contract Ref#</th>';
        html += '<td>' + contract.contract_refno + '</td>';
        html += '<th>Project Name</th>';
        html += '<td>' + (contract.project_name || '') + '</td>';
        html += '</tr>';

        html += '<tr>';
        html += '<th>Billing Frequency</th>';
        html += '<td>' + contract.BillingFrequency + '</td>';
        html += '<th>Currency</th>';
        html += '<td>' + contract.CurrencyName + '</td>';
        html += '</tr>';

        html += '<tr>';
        html += '<th>Contract Value</th>';
        html += '<td>' + contract.contract_value + '</td>';
        html += '<th>Auto Renew</th>';
        html += '<td>' + (contract.auto_renew ? 'Yes' : 'No') + '</td>';
        html += '</tr>';

        html += '</table>';

        // Custom Contract
        if (contract.basic_contract_type == 2) {
            html += '<h5 class="mt-3">Milestone Details</h5>';
            html += '<table class="table table-bordered table-striped">';
            html += '<thead>';
            html += '<tr>';
            html += '<th>#</th>';
            html += '<th>Milestone</th>';
            html += '<th>Due Date</th>';
            html += '<th>Amount</th>';
            html += '<th>Status</th>';
            html += '</tr>';
            html += '</thead>';
            html += '<tbody>';

            $.each(milestones, function (j, m) {
                if (m.contract_id == contract.contract_id) {
                    html += '<tr>';
                    html += '<td>' + m.milestone_no + '</td>';
                    html += '<td>' + m.milestone_name + '</td>';
                    html += '<td>' + FormatDate(m.due_date) + '</td>';
                    html += '<td>' + FormatAmount(m.milestone_amount) + '</td>';
                    html += '<td>' + m.status + '</td>';
                    html += '</tr>';
                }
            });

            html += '</tbody>';
            html += '</table>';
        }

        html += '</div>';
        html += '</div>';

        $('#ContractContainer').append(html);

    });

}
function FormatDate(date) {
    if (!date)
        return '';

    var d = new Date(date);

    if (isNaN(d.getTime()))
        return '';

    return d.toLocaleDateString('en-GB', {
        day: '2-digit',
        month: 'short',
        year: 'numeric'
    });

}
function FormatAmount(amount) {

    if (amount == null || amount === '')
        return '0.00';

    return Number(amount).toLocaleString('en-US', {
        minimumFractionDigits: 2,
        maximumFractionDigits: 2
    });

}
function DownloadPDF() {
    var element = document.querySelector('.container-fluid');

    var opt = {
        margin: 0.3,
        filename: 'Client_Report.pdf',
        image: {
            type: 'jpeg',
            quality: 1
        },
        html2canvas: {
            scale: 2,
            useCORS: true
        },
        jsPDF: {
            unit: 'in',
            format: 'a4',
            orientation: 'portrait'
        }
    };

    html2pdf().set(opt).from(element).save();

}
