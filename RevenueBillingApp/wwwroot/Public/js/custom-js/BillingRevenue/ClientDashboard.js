var instanceid = 0;

$(document).ready(function () {
    $("#ddlClient").select2({
        placeholder: "Please select client",
        width: "100%"
    });

    AutoAlertMessage();

    GetClientInfoDDL();

    $("#ShowBtn").click(function () {
        var clientId = $("#ddlClient").val();
        var clientName = $("#ddlClient option:selected").text();

        if (clientId == "0") {
            Swal.fire({
                icon: "warning",
                title: "Client Required",
                text: "Please select a client."
            });
            return;
        }


        // Loading Popup
        Swal.fire({
            title: "Loading Client Dashboard...",
            html: `
            <b>${clientName}</b><br>
            Please wait while we are loading the dashboard.
        `,
            allowOutsideClick: false,
            allowEscapeKey: false,
            showConfirmButton: false,
            didOpen: () => {
                Swal.showLoading();

                document.querySelector(".swal2-container")
                    .classList.add("loading-backdrop");
            }
        });


        // Immediately update client name
        $("#selectedClientName").text(clientName);

        // Hide selection
        $("#clientSelectArea").addClass("d-none");

        // IMPORTANT: remove d-none first
        $("#changeClient").removeClass("d-none");


        // Demo: 10 sec delay == 10000
        // Demo: 5 sec delay == 5000
        setTimeout(function () {
            Swal.close();

            // Dashboard Data Load
            LoadClientDashboard(clientId);

        }, 5000);


    });

    $("#btnClientDetails").click(function () {
        var clientId = $("#ddlClient").val();

        if (clientId == "0") {
            Swal.fire({ icon: "warning", title: "Please select client first." });
            return;
        }

        var d = window.currentClientData;
        //console.log('before: ' + d);

        if (!d) {
            Swal.fire({ icon: "warning", title: "Please load dashboard first (click Show)." });
            return;
        }
        //console.log('after: ' + d);

        $("#txtClientName").val(d.client_name);
        $("#txtIndustry").val(d.industry_name);
        $("#txtTaxNo").val(d.tax_registration_no);
        $("#txtCountry").val(d.country_name);
        $("#txtPrimaryContact").val(d.primary_contact_name);
        $("#txtEmail").val(d.primary_contact_email);
        $("#txtPhone").val(d.primary_contact_phone);
        $("#txtAddress").val(d.billing_address);

        $("#ClientDetailsModal").modal("show");

    });

    $("#changeClient").click(function (e) {
        e.preventDefault();

        $("#clientSelectArea").removeClass("d-none");
        $("#changeClient").addClass("d-none");
    });

    $(document).on("click", ".view-dashboard-item", function (e) {
        e.preventDefault();

        var type = $(this).data("type");
        var id = $(this).data("id");

        //console.log("View Type:", type);
        //console.log("View ID:", id);

        switch (type) {
            case "invoice":
                ViewInvoice(id);
                break;
            case "contract":
                ViewContract(id);
                break;
            case "milestone":
                ViewMilestone(id);
                break;
            default:
                console.warn("Unknown dashboard item type:", type);
                break;
        }

    });

});

function AutoAlertMessage() {
    var messageElement = document.getElementById("autoInvoiceMessage");

    if (!messageElement) {
        return;
    }

    var successMessage = messageElement.getAttribute("data-success");
    var errorMessage = messageElement.getAttribute("data-error");

    if (successMessage) {
        Swal.fire({
            icon: "success",
            title: "Success",
            text: successMessage,
            timer: 3000,
            showConfirmButton: false,
            allowOutsideClick: false
        });

    }

    if (errorMessage) {
        Swal.fire({
            icon: "error",
            title: "Error",
            text: errorMessage,
            showConfirmButton: true,
            allowOutsideClick: false
        });
    }

}

/* Client DDL populate */
function GetClientInfoDDL() {
    new APICALL(GetGlobalURL('Base', 'GetClientInfoDDL'), 'GET', '', true).FETCH((result, error) => {
        if (result) {
            if (result.data != null) {
                window.allUsers = result.data;

                var ddl = $('#ddlClient');
                populateDropdowns(ddl, result.data, null, 0);
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
function populateDropdowns(ddl, users, selectedManagerId = null, customfield) {
    ddl.empty();

    ddl.append('<option value="0">Please select</option>');
    $.each(users, function (i, option) {
        if (option.ddlvalue != selectedManagerId) {

            if (customfield == 0) {
                ddl.append(
                    `<option value="${option.ddlvalue}">${option.ddltext}</option>`
                );
            } else {
                ddl.append(
                    `<option value="${option.ddlvalue}" data-customfield="${option.customfield}">${option.ddltext}</option>`
                );
            }
        }
    });
}

/* Client Dashboard */
function LoadClientDashboard(clientId) {
    //console.log("Dashboard Loaded For Client : " + clientId);

    new APICALL(GetGlobalURL('Base', 'GetClientDashboard') + '?clientId=' + clientId, 'GET', '', true)
        .FETCH((result, error) => {
            if (error) {
                Swal.fire({
                    icon: 'error',
                    title: 'Error',
                    text: error.data ? error.data.responseText : 'Unable to load dashboard.'
                });
                return;
            }

            var d = result.data;
            console.log("Dashboard Response:", d);
            console.log("Client Data:", d.Table[0]);

            // Header + KPI Cards
            $(".page-header-title h4").text(d.Table[0].client_name);


            // =====================================
            // HIDE CLIENT SELECTION
            // =====================================
            $("#clientSelectArea").addClass("d-none");

            $("#changeClient")
                .removeClass("d-none")
                .show();


            $("[data-kpi='total-ar']").text(FormatAmount(d.Table[0].total_ar, false));
            $("[data-kpi='active-contracts']").text(d.Table[0].active_contracts_count);
            $("[data-kpi='pending-milestones']").text(d.Table[0].pending_milestones_count);
            $("[data-kpi='current-balance']").text(FormatAmount(d.Table[0].current_balance, false));


            $("#primary_contact_name").html(d.Table[0].primary_contact_name);
            $("#tax_registration_no").html(d.Table[0].tax_registration_no);
            $("#support_owner").html(d.Table[0].support_owner);
            $("#billing_address").html(d.Table[0].billing_address);


            // Store client object globally for modal reuse
            window.currentClientData = d.Table[0];


            // ---- Active Contracts ----
            FillActiveContracts(d.Table1 || []);

            // ---- Milestone Progress ----
            FillMilestoneProgress(d.Table2 || []);

            // ---- Recent Billing Activities ----
            FillRecentBillingActivity(d.Table3 || []);

            // ---- Fill Contract Grid ----
            FillContractGrid(d.Table4 || []);

            // ---- Fill Invoice History Grid ----
            FillInvoiceHistory(d.Table3 || []);

            // ---- Fill Milestone Grid ----
            FillMilestoneGrid(d.Table2 || []);

        });
}


/* Dashboard Card Overview Tab */
function FillActiveContracts(contracts) {
    var $container = $("#activeContractsList");
    $container.empty();

    if (contracts.length == 0) {
        $container.html('<p class="text-muted mb-0">No active contracts found.</p>');
        return;
    }

    $.each(contracts, function (i, c) {
        var typeLabel = c.contracttype || (c.basic_contract_type == 1 ? "SaaS" : "Custom");
        var amount = parseFloat(c.contract_amount || 0).toLocaleString();
        var displayName = c.basic_contract_type == 2 ? (c.project_name || "-") : (c.frequencyname || "Recurring") + " Subscription";

        var statusHtml = "";
        if (c.contract_status == "Active") {
            statusHtml = `<span class="text-success fw-bold">${c.bill_end_date ? "Renews " + FormatMonthYear(c.bill_end_date) : "Active"}</span>`;
        } else if (c.contract_status == "In Implementation") {
            statusHtml = `<span class="text-primary fw-bold">In Implementation</span>`;
        } else if (c.contract_status == "Expired") {
            statusHtml = `<span class="text-danger fw-bold">Expired</span>`;
        } else {
            statusHtml = `<span class="text-muted fw-bold">${c.contract_status || ""}</span>`;
        }

        var amountLabel = c.basic_contract_type == 1
            ? `$${amount} / ${(c.frequencyname || "period").toLowerCase()}`
            : `$${amount} Total`;

        $container.append(`
            <div class="d-flex mb-2 justify-content-between">
                <span class="text-uppercase fw-semibold">${c.contract_refno}</span>
                <span class="badge bg-secondary text-uppercase">${typeLabel}</span>
            </div>
            <h6 class="text-uppercase">${displayName}</h6>
            <div class="d-flex mb-2 justify-content-between">
                <span class="fw-semibold">${amountLabel}</span>
                ${statusHtml}
            </div>
            ${i < contracts.length - 1 ? '<hr>' : ''}
        `);
    });
}
function FillMilestoneProgress(milestones) {
    var $container = $("#milestoneProgressList");
    $container.empty();

    if (milestones.length == 0) {
        $("#milestoneContractRef").text("-");
        $container.html('<p class="text-muted mb-0">No milestones found.</p>');
        return;
    }

    $("#milestoneContractRef").text(milestones[0].contract_refno);

    $.each(milestones, function (i, m) {
        var dotClass = "bg-secondary";
        var statusText = "";

        if (m.status == "Invoiced") {
            dotClass = "bg-success";
            statusText = "Completed";
        } else {
            var dueDate = new Date(m.due_date);
            var today = new Date();
            if (dueDate < today) {
                dotClass = "bg-danger";
                statusText = "Overdue • Due " + FormatMonthDay(m.due_date);
            } else {
                dotClass = "bg-primary";
                statusText = "Due " + FormatMonthDay(m.due_date) + " • Pending";
            }
        }

        $container.append(`
            <div class="workflow-step">
                <div class="step-number ${dotClass} text-white"></div>
                <div class="step-label">
                    <p class="mb-0 fw-semibold">${m.milestone_name}</p>
                    <span class="font-12 text-muted">${statusText}</span>
                </div>
            </div>
        `);
    });
}


/* Dashboard Grid */
function FillRecentBillingActivity(invoices) {
    var $tbody = $("#recentBillingBody");

    $tbody.empty();

    if (!invoices || invoices.length === 0) {
        $tbody.html(`
            <tr>
                <td colspan="5" class="text-center text-muted py-4">
                    No recent billing activity found.
                </td>
            </tr>
        `);

        return;
    }

    $.each(invoices, function (i, inv) {
        var invoiceId = inv.invoice_id || "0";
        var invoiceNo = inv.invoice_no || inv.invoice_refno || inv.invoice_id || "-";

        var invoiceDate = inv.invoice_date
            ? FormatDate(inv.invoice_date)
            : "-";

        var amount = FormatAmount(
            inv.total_amount || inv.amount || 0,
            false
        );

        var status = inv.payment_status || inv.status || "Pending";

        var statusHtml = "";

        if (status == "Paid" || status == 1) {

            statusHtml = `
                <span class="badge bg-success">
                    Paid
                </span>
            `;

        }
        else if (status == "Overdue") {

            statusHtml = `
                <span class="badge bg-danger">
                    Overdue
                </span>
            `;

        }
        else if (status == "Draft") {

            statusHtml = `
                <span class="badge bg-secondary">
                    Draft
                </span>
            `;

        }
        else {

            statusHtml = `
                <span class="badge bg-warning">
                    ${status}
                </span>
            `;
        }


        $tbody.append(`
            <tr>

                <td>
                    <span class="fw-semibold">
                        ${invoiceNo}
                    </span>
                </td>

                <td>
                    ${invoiceDate}
                </td>

                <td>
                    ${amount}
                </td>

                <td>
                    ${statusHtml}
                </td>

                <td>
                    <div class="hstack gap-2 justify-content-center">
                        <a href="#"
                           class="avatar-text avatar-md view-dashboard-item"
                           data-type="invoice"
                           data-id="${invoiceId}">
                               <i class="feather feather-eye"></i> 
                        </a>

                    </div>
                </td>

            </tr>
        `);
    });
}
function FillContractGrid(contracts) {
    var $tbody = $("#contractGridBody");

    $tbody.empty();

    if (!contracts || contracts.length === 0) {

        $tbody.html(`
            <tr>
                <td colspan="6" class="text-center text-muted py-4">
                    No active or pending contracts found.
                </td>
            </tr>
        `);

        return;
    }

    $.each(contracts, function (i, c) {

        // -----------------------------
        // Contract Type
        // -----------------------------
        var typeHtml = "";

        if (c.basic_contract_type == 1) {

            typeHtml = `
                <span class="badge bg-primary">
                    SAAS
                </span>
            `;

        }
        else if (c.basic_contract_type == 2) {

            typeHtml = `
                <span class="badge bg-secondary">
                    CUSTOM
                </span>
            `;

        }
        else {

            typeHtml = `
                <span class="badge bg-secondary">
                    OTHER
                </span>
            `;
        }


        // -----------------------------
        // Status
        // -----------------------------
        var statusClass = "text-muted";

        if (c.contract_status == "Active") {

            statusClass = "text-success";

        }
        else if (c.contract_status == "Pending") {

            statusClass = "text-warning";

        }
        else if (c.contract_status == "Expired") {

            statusClass = "text-danger";
        }


        // -----------------------------
        // Contract Amount
        // -----------------------------
        var amount = FormatAmount(
            c.contract_amount || 0,
            false
        );


        // -----------------------------
        // Contract Dates
        // -----------------------------
        var startDate = c.contract_start_date
            ? FormatDate(c.contract_start_date)
            : "-";

        var endDate = c.contract_end_date
            ? FormatDate(c.contract_end_date)
            : "-";


        // -----------------------------
        // Row
        // -----------------------------
        $tbody.append(`
            <tr>

                <td>
                    <span class="fw-semibold">
                        ${c.contract_refno || "-"}
                    </span>
                </td>

                <td>
                    ${typeHtml}
                </td>

                <td>
                    <span class="${statusClass} fw-bold">
                        ${c.contract_status || "-"}
                    </span>
                </td>

                <td>
                    ${amount}
                </td>

                <td>
                    ${startDate} - ${endDate}
                </td>

                <td>
                    <div class="hstack gap-2 justify-content-center">

                        <a href="#"
                           class="avatar-text avatar-md view-dashboard-item"
                           data-type="contract"
                           data-id="${c.id}">
                                <i class="feather feather-eye"></i>
                        </a>

                    </div>
                </td>

            </tr>
        `);
    });
}
function FillInvoiceHistory(invoices) {
    var $tbody = $("#invoiceHistoryBody");

    $tbody.empty();

    if (!invoices || invoices.length === 0) {

        $tbody.html(`
            <tr>
                <td colspan="6" class="text-center text-muted py-4">
                    No invoice history found.
                </td>
            </tr>
        `);

        return;
    }

    $.each(invoices, function (i, inv) {
        // Invoice ID
        var invoiceId = inv.invoice_id || "0";
        var invoiceNo = inv.invoice_no || inv.invoice_refno || inv.invoice_id || "-";

        // Invoice Date
        var invoiceDate = inv.invoice_date
            ? FormatDate(inv.invoice_date)
            : "-";


        // Due Date
        var dueDate = inv.due_date
            ? FormatDate(inv.due_date)
            : "-";


        // Amount
        var amount = FormatAmount(
            inv.total_amount || inv.amount || 0,
            false
        );


        // Status
        var status =
            inv.payment_status ||
            inv.status ||
            "Pending";


        var statusHtml = "";


        if (status == "Paid" || status == 1) {

            statusHtml = `
                <span class="badge rounded-pill bg-soft-success text-success">
                    Paid
                </span>
            `;

        }
        else if (status == "Overdue") {

            statusHtml = `
                <span class="badge rounded-pill bg-soft-danger text-danger">
                    Overdue
                </span>
            `;

        }
        else if (status == "Draft") {

            statusHtml = `
                <span class="badge rounded-pill bg-secondary">
                    Draft
                </span>
            `;

        }
        else {

            statusHtml = `
                <span class="badge rounded-pill bg-soft-warning text-warning">
                    ${status}
                </span>
            `;
        }


        // Row
        $tbody.append(`
            <tr>

                <td>
                    <span class="fw-semibold">
                        ${invoiceNo}
                    </span>
                </td>

                <td>
                    ${invoiceDate}
                </td>

                <td class="${status == "Overdue" ? "text-danger fw-bold" : ""}">
                    ${dueDate}
                </td>

                <td>
                    ${amount}
                </td>

                <td>
                    ${statusHtml}
                </td>

                <td>
                    <div class="hstack gap-2 justify-content-center">
                        <a href="#"
                           class="avatar-text avatar-md view-dashboard-item"
                           data-type="invoice"
                           data-id="${invoiceId}">
                                <i class="feather feather-eye"></i>
                        </a>

                    </div>
                </td>

            </tr>
        `);
    });
}
function FillMilestoneGrid(milestones) {
    var $tbody = $("#milestoneGridBody");

    $tbody.empty();

    if (!milestones || milestones.length === 0) {

        $tbody.html(`
            <tr>
                <td colspan="5" class="text-center text-muted py-4">
                    No milestones found.
                </td>
            </tr>
        `);

        return;
    }

    $.each(milestones, function (i, m) {
        // -----------------------------
        // Milestone Name
        // -----------------------------
        var milestoneName = m.milestone_name || "-";


        // -----------------------------
        // Deliverables / Remarks
        // -----------------------------
        var deliverables = m.remarks || "-";


        // -----------------------------
        // Due Date
        // -----------------------------
        var dueDate = m.due_date
            ? FormatDate(m.due_date)
            : "-";


        // -----------------------------
        // Status
        // -----------------------------
        var status = m.status || "Pending";

        var statusHtml = "";


        if (status == "Invoiced") {

            statusHtml = `
                <span class="badge bg-soft-success text-success rounded-pill">
                    Completed
                </span>
            `;

        }
        else if (status == "Pending") {

            statusHtml = `
                <span class="badge bg-soft-primary text-primary rounded-pill">
                    Pending
                </span>
            `;

        }
        else if (status == "Blocked") {

            statusHtml = `
                <span class="badge bg-soft-danger text-danger rounded-pill">
                    Blocked
                </span>
            `;

        }
        else if (status == "In Progress") {

            statusHtml = `
                <span class="badge bg-soft-primary text-primary rounded-pill">
                    In Progress
                </span>
            `;

        }
        else {

            statusHtml = `
                <span class="badge bg-secondary rounded-pill">
                    ${status}
                </span>
            `;
        }


        // -----------------------------
        // Row
        // -----------------------------
        $tbody.append(`
            <tr>

                <td>
                    <span class="fw-semibold">
                        ${milestoneName}
                    </span>
                </td>

                <td>
                    ${deliverables}
                </td>

                <td>
                    ${dueDate}
                </td>

                <td>
                    ${statusHtml}
                </td>

                <td>
                    <div class="hstack gap-2 justify-content-center">

                        <a href="#"
                           class="avatar-text avatar-md view-dashboard-item"
                           data-type="milestone"
                           data-id="${m.milestone_id}">
                                <i class="feather feather-eye"></i>
                        </a>

                    </div>
                </td>

            </tr>
        `);
    });
}


/* Dashboard Grid VIEW BUTTON */
function ViewInvoice(invoiceId) {
    console.log("View Invoice ID:", invoiceId);
}
function ViewContract(contractId) {
    console.log("View Contract ID:", contractId);
}
function ViewMilestone(milestoneId) {
    console.log("View Milestone ID:", milestoneId);
}
