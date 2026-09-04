var clients = {};

$(document).ready(function () {
    ViewGrid();
    ViewInvoiceGrid();

    // Tab Change
    $('button[data-bs-toggle="pill"]').on('shown.bs.tab', function (e) {

        var target = $(e.target).attr("id");

        if (target === "pills-all-tab") {
            $("#pageTitle").text("Client List");
            $("#pageDescription").text("Create the client record + define contract basics");
        }
        else if (target === "pills-new-tab") {
            $("#pageTitle").text("Invoice List");
            $("#pageDescription").text("View and manage client invoices");
        }
    });

});
function ViewGrid() {
    ShowLoader('UserMasterDiv');
    UTILITY.CheckSession((data_) => {
        if (data_) {
            new APICALL(GetGlobalURL('Base', 'GetAllClientList'), 'GET', '', true).FETCH((result, error) => {
                if (result) {
                    $("#user-master").dataTable().fnDestroy();
                    $('#user-master tbody').html('');

                    var primaryID = 0;
                    var rowIndex = 0;
                    var clientID = 0;
                    var contractID = 0;

                    //console.log(result);
                    if (result.data != null) {
                        clients = result.data;

                        $.each(result.data, function (i, option) {
                            var primaryID = option.client_id;

                            clientID = option.client_id;
                            contractID = option.contract_id;

                            var completed = parseInt(option.completedtabs || 0);
                            var total = parseInt(option.totaltabs || 6);
                            var progress = Math.round(parseFloat(option.progresspercent || 0));

                            // Force 100% if all tabs completed
                            if (completed >= total) {
                                completed = total;
                                progress = 100;
                            }

                            // Default Values
                            var progressColor = "bg-danger";
                            var statusText = "Getting Started";

                            // Status & Color
                            if (progress >= 100) {
                                progressColor = "bg-success";
                                statusText = "Completed";
                            }
                            else if (progress >= 75) {
                                progressColor = "bg-primary";
                                statusText = "Almost Complete";
                            }
                            else if (progress >= 40) {
                                progressColor = "bg-warning";
                                statusText = "In Progress";
                            }

                            // Progress HTML
                            var ProgressBar =
                                '<td class="text-center">' +
                                    '<div class="progress-title">' +
                                        completed + ' / ' + total + ' Tabs Completed' +
                                    '</div>' +

                                    '<div class="progress client-progress">' +
                                        '<div class="progress-bar ' + progressColor + '"' +
                                        ' role="progressbar"' +
                                        ' aria-valuenow="' + progress + '"' +
                                        ' aria-valuemin="0"' +
                                        ' aria-valuemax="100"' +
                                        ' style="width:' + progress + '%;">' +
                                            progress + '%' +
                                        '</div>' +
                                    '</div>' +

                                    '<div class="progress-status">' +
                                        statusText +
                                    '</div>' +
                                '</td>';

                            /*ProgressBar = '<td>-</td>'*/
                            var PrintBtn = '<td><button class="avatar-text avatar-md ViewPrint" data-value="' + primaryID + '" data-value1="' + i + '" type="button"><i class="feather-printer"></i></button> '

                            var UpdateBtn = (data_[0].AllowUpdate == true)
                                ? '<button class="avatar-text avatar-md EditData"' +
                                ' data-value="' + primaryID + '"' +
                                ' data-value1="' + i + '"' +
                                ' data-clientid="' + clientID + '"' +
                                ' data-contractid="' + contractID + '"' +
                                ' type="button">' +
                                '<i class="feather-edit-2 edit-icon"></i></button></td>'
                                : '';

                            $('#user-master tbody').append(
                                '<tr id="rowid-' + i + '">' +
                                    /*'<td>' + option.client_id + '</td>' +*/
                                '<td>' + (option.client_name || '') + '</td>' +
                                '<td>' + (option.contract_refno || '') + '</td>' +
                                '<td>' + (option.contractType || '') + '</td>' +
                                '<td>' + (option.billingFrequency || '') + '</td>' +
                                '<td>' + (option.contractperiod || '') + '</td>' +
                                '<td>' + (FormatAmount(option.valuepricing, true) || '') + '</td>' +
                                '<td>' + (option.renewaltype || '') + '</td>' +
                                '<td>' + FormatDate(option.createdon) + '</td>' +
                                    ProgressBar +
                                    PrintBtn +
                                    UpdateBtn +
                                '</tr>'
                            );
                        });

                        $('.ViewPrint').on('click', function () {
                            primaryID = $(this).data('value');
                            rowIndex = $(this).data('value1');

                            ViewPrint(primaryID, rowIndex);
                        });

                        $('.EditData').on('click', function () {
                            primaryID = $(this).data('value');
                            rowIndex = $(this).data('value1');
                            clientID = $(this).data('clientid');
                            contractID = $(this).data('contractid');

                            EditData(primaryID, rowIndex, clientID, contractID);
                        });

                    }
                    $('#user-master').DataTable({
                        autoWidth: false,
                        responsive: true
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
function ViewPrint(primaryID, rowIndex) {
    ////alert('View Print: ' + 'clientid: ' + primaryID);
    window.open('/BillingRevenue/rpt_ClientInformation?ClientId=' + primaryID, '_blank');
}
function EditData(primaryID, rowIndex, clientid, contractid) {
    contractid = contractid ?? 0;      // ES2020
    // Ya:
    // contractid = contractid || 0;

    var url = '/BillingRevenue/ClientOnboarding';
    var qs = '?ClientId=' + clientid + '&ContractId=' + contractid + '&type=editfromgrid';
    window.location.href = url + qs;
}
function ViewInvoiceGrid() {
    ShowLoader('UserMasterDiv');

    UTILITY.CheckSession((data_) => {
        if (data_) {

            new APICALL(GetGlobalURL('Base', 'GetAllInvoicesGrid'), 'GET', '', true)
                .FETCH((result, error) => {

                    if (error) {
                        Swal.fire({
                            icon: 'error',
                            title: 'Error...',
                            text: error.data.responseText
                        });
                        HideLoader("UserMasterDiv");
                        return;
                    }

                    if (result) {
                        $("#inv_table").dataTable().fnDestroy();
                        $("#inv_table tbody").html("");

                        $.each(result.data, function (i, option) {
                            // Invoice Date
                            var invoiceDate = "";
                            if (option.invoice_date) {
                                var d = new Date(option.invoice_date);
                                invoiceDate =
                                    ("0" + d.getDate()).slice(-2) + "-" +
                                    ("0" + (d.getMonth() + 1)).slice(-2) + "-" +
                                    d.getFullYear();
                            }

                            // Due Date
                            var dueDate = "";
                            if (option.due_date) {
                                var d = new Date(option.due_date);
                                dueDate =
                                    ("0" + d.getDate()).slice(-2) + "-" +
                                    ("0" + (d.getMonth() + 1)).slice(-2) + "-" +
                                    d.getFullYear();
                            }

                            // Created On (SP doesn't return createdon)
                            var createdOn = invoiceDate;

                            // Invoice Status Badge
                            var invoiceStatus = "";

                            if (option.invoice_status_code == 1)
                                invoiceStatus = '<span class="badge bg-warning">Pending</span>';
                            else if (option.invoice_status_code == 2)
                                invoiceStatus = '<span class="badge bg-success">Paid</span>';
                            else
                                invoiceStatus = option.invoice_status;

                            // Payment Status Badge
                            var paymentStatus = "";

                            if (option.payment_status == "Paid")
                                paymentStatus = '<span class="badge bg-success">Paid</span>';
                            else
                                paymentStatus = '<span class="badge bg-danger">Unpaid</span>';

                            var action = '';

                            action += '<button class="avatar-text avatar-md ViewBtn" ' +
                                'data-id="' + option.invoice_id + '" ' +
                                'type="button">' +
                                '<i class="feather-eye"></i>' +
                                '</button>';

                            action += '<button class="avatar-text avatar-md DownloadBtn" ' +
                                'data-id="' + option.invoice_id + '" ' +
                                'type="button">' +
                                '<i class="feather-download"></i>' +
                                '</button>';

                            action += '<button class="avatar-text avatar-md EmailBtn" ' +
                                'data-id="' + option.invoice_id + '" ' +
                                'type="button">' +
                                '<i class="feather-mail"></i>' +
                                '</button>';

                            $('#inv_table tbody').append(
                                '<tr>' +
                                    '<td>' + (option.invoice_no || '') + '</td>' +
                                    '<td>' + (option.client_name || '') + '</td>' +
                                    '<td>' + (option.contract_refno || '') + '</td>' +
                                    '<td>' + (option.contract_type || '') + '</td>' +
                                    '<td>' + invoiceDate + '</td>' +
                                    '<td>' + dueDate + '</td>' +
                                    '<td class="text-end">' +
                                        parseFloat(FormatAmount(option.total_amount, true) || 0).toLocaleString() +
                                    '</td>' +
                                    '<td>' + invoiceStatus + '</td>' +
                                    '<td>' + paymentStatus + '</td>' +
                                    '<td>' + createdOn + '</td>' +
                                    '<td class="text-end">' +
                                        action +
                                    '</td>' +
                                '</tr>'
                            );

                        });

                        $("#inv_table").DataTable({
                            autoWidth: false,
                            responsive: true,
                            order: [[0, "desc"]]
                        });


                        // ===============================
                        // BUTTON EVENTS
                        // ===============================

                        // View
                        $("#inv_table").off("click", ".ViewBtn").on("click", ".ViewBtn", function (e) {
                            e.preventDefault();
                            e.stopPropagation();

                            var invoiceId = $(this).data("id");
                            ViewInvoice(invoiceId);
                        });

                        // Download
                        $("#inv_table").off("click", ".DownloadBtn").on("click", ".DownloadBtn", function (e) {
                            e.preventDefault();
                            e.stopPropagation();

                            var invoiceId = $(this).data("id");
                            DownloadInvoice(invoiceId);
                        });

                        // Email
                        $("#inv_table").off("click", ".EmailBtn").on("click", ".EmailBtn", function (e) {
                            e.preventDefault();
                            e.stopPropagation();

                            var invoiceId = $(this).data("id");
                            EmailInvoice(invoiceId);
                        });


                        HideLoader("UserMasterDiv");
                    }

                });

        }

    });

}
