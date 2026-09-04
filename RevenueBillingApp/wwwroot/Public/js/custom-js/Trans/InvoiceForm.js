var instanceid = 0;

document.querySelectorAll('.file-input').forEach(input => {
    input.addEventListener('change', function () {
        const uploader = this.closest('.uploader');
        const previewContainer = uploader.querySelector('.preview-container');

        Array.from(this.files).forEach(file => {
            const col = document.createElement('div');
            col.className = 'col-6 mt-3';
            const preview = document.createElement('div');
            preview.className = 'preview-item';

            // Remove Button
            const removeBtn = document.createElement('button');
            removeBtn.className = 'remove-btn';
            removeBtn.innerHTML = '&times;';
            removeBtn.onclick = () => {
                col.remove();
            };

            preview.appendChild(removeBtn);

            // Image Preview
            if (file.type.startsWith('image/')) {
                const img = document.createElement('img');
                img.src = URL.createObjectURL(file);
                preview.appendChild(img);
            } else {
                const icon = document.createElement('div');
                icon.style.height = '120px';
                icon.style.display = 'flex';
                icon.style.alignItems = 'center';
                icon.style.justifyContent = 'center';
                icon.style.fontSize = '50px';
                icon.innerHTML = '📄';
                preview.appendChild(icon);
            }

            // File Name
            const name = document.createElement('div');
            name.className = 'mt-2 small';
            name.innerText = file.name;
            preview.appendChild(name);
            col.appendChild(preview);
            previewContainer.appendChild(col);
        });
    });
});

$(document).ready(function () {
    instanceid = GetParameterValues('instanceid') ?? 0;
    LoadViewMyRequest(instanceid);

    $("#intiateform").on("click", ".SaveBtn", function () {
        SaveData();
    });

    $("#btnAddRow").on("click", function () {
        if (!ValidateLastRow())
            return;

        var row = `
        <tr>
            <td>
                <input type="text" class="form-control item_desc" placeholder="Item description">
            </td>

            <td>
                <input type="number" class="form-control form-control-sm qty text-end" value="1">
            </td>

            <td>
                <input type="number" class="form-control form-control-sm unit_price text-end" value="0">
            </td>

            <td class="amount text-end align-middle">0.00</td>

            <td>
                <input type="number" class="form-control form-control-sm tax_percent text-end" value="0">
            </td>

            <td class="tax_amount text-end align-middle">0.00</td>

            <td class="total_amount text-end align-middle fw-semibold">0.00</td>

            <td class="text-center">
                <button type="button" class="btn btn-danger btn-sm btnRemoveRow">
                    <i class="feather-trash-2"></i>
                </button>
            </td>
        </tr>`;

        $("#tbltbody").append(row);

        //ToggleDeleteButtons();
    });

    $(document).on("input", ".qty, .unit_price, .tax_percent", function () {
        var row = $(this).closest("tr");

        var qty = parseFloat(row.find(".qty").val()) || 0;
        var price = parseFloat(row.find(".unit_price").val()) || 0;
        var tax_percent = parseFloat(row.find(".tax_percent").val()) || 0;

        var amount = qty * price;
        var taxAmount = amount * tax_percent / 100;
        var total = amount + taxAmount;

        row.find(".amount").text(amount.toFixed(2));
        row.find(".tax_amount").text(taxAmount.toFixed(2));
        row.find(".total_amount").text(total.toFixed(2));

        CalculateInvoiceTotals();
    });

    $(document).on("click", ".btnRemoveRow", function () {
        if ($("#tbltbody tr").length == 1) {
            Swal.fire({
                icon: 'warning',
                text: 'At least one invoice item is required.'
            });
            return;
        }
        $(this).closest("tr").remove();

        CalculateInvoiceTotals();

        //ToggleDeleteButtons();
    });

    //ToggleDeleteButtons();

});

async function LoadViewMyRequest(instanceid) {
    FillDropdowns_OnLoad();

    var invoiceId = $("#hdnInvoiceId").val();
    if (invoiceId && invoiceId != "" && invoiceId != "0") {
        ////alert('Edit Mode');
        $("#hdnAction").val('Edit');

        GetInvoiceById(invoiceId);
    }

    var action = $("#hdnAction").val();
    if (action === 'Add') {
        $("#docSection").show();
    } else {
        $("#docSection").hide();
    }

    setInvoiceDates("#invoice_date", "#due_date", 10);

}
function ValidateAll() {
    // clear previous errors first
    $(".is-invalid").removeClass("is-invalid");

    if (!$("#invoice_no").val()?.trim()) {
        setInvalid("#invoice_no", "Please enter Invoice No!");
        //$("#invoice_no").focus();
        return false;
    }

    if (!$("#ddlClient").val()?.trim()) {
        setInvalid("#ddlClient", "Please select Client!");
        //$("#ddlClient").focus();
        return false;
    }

    if (!$("#invoice_date").val()?.trim()) {
        setInvalid("#invoice_date", "Please enter Invoice Date!");
        //$("#invoice_date").focus();
        return false;
    }

    if (!$("#ddlBillingType").val()?.trim()) {
        setInvalid("#ddlBillingType", "Please select Billing Type!");
        //$("#ddlBillingType").focus();
        return false;
    }


    //+++++++++++++++++ Item Detail Validation ++++++++++++++++++++++++++

    /* Invoice Grid Validation */
    if ($("#tbltbody tr").length == 0) {
        ShowValidationMessage("Please add at least one invoice item!");
        return false;
    }

    var isGridValid = true;

    $("#tbltbody tr").each(function (index) {
        var row = $(this);

        var itemDesc = row.find(".item_desc").val()?.trim();
        var qty = parseFloat(row.find(".qty").val()) || 0;
        var unitPrice = parseFloat(row.find(".unit_price").val()) || 0;

        if (!itemDesc) {
            row.find(".item_desc").addClass("is-invalid");
            ShowValidationMessage("Please enter Item Description in Row " + (index + 1));
            isGridValid = false;
            return false;
        }

        if (qty <= 0) {
            row.find(".qty").addClass("is-invalid");
            ShowValidationMessage("Quantity must be greater than zero in Row " + (index + 1));
            isGridValid = false;
            return false;
        }

        if (unitPrice <= 0) {
            row.find(".unit_price").addClass("is-invalid");
            ShowValidationMessage("Unit Price must be greater than zero in Row " + (index + 1));
            isGridValid = false;
            return false;
        }
    });

    if (!isGridValid) {
        return false;
    }

    //++++++++++++++++++++++++++ end here +++++++++++++++++++++++++++++++

    if (!$("#ddlContract").val()?.trim()) {
        setInvalid("#ddlContract", "Please select Contract!");
        //$("#ddlContract").focus();
        return false;
    }


    return true;
}
function ValidateLastRow() {
    var lastRow = $("#tbltbody tr:last");

    if (lastRow.length == 0)
        return true;

    var itemDesc = lastRow.find(".item_desc").val().trim();
    var qty = parseFloat(lastRow.find(".qty").val()) || 0;

    if (itemDesc == "") {
        Swal.fire({
            icon: 'warning',
            text: 'Please enter Item Description first.'
        });

        lastRow.find(".item_desc").focus();
        return false;
    }

    if (qty <= 0) {
        Swal.fire({
            icon: 'warning',
            text: 'Quantity must be greater than zero.'
        });

        lastRow.find(".qty").focus();
        return false;
    }

    return true;
}
function ShowValidationMessage(message) {
    Swal.fire({
        icon: 'warning',
        text: message,
        confirmButtonColor: "#61affe"
    });
}

/* Save InvoiceForm */
function SaveData() {
    var res = ValidateAll();
    if (res == false) {
        return false;
    }

    var formData = new FormData();
    var invoiceItems = [];

    $("#tbltbody tr").each(function (index) {
        var row = $(this);

        formData.append("invoice_details[" + index + "].item_desc",
            row.find(".item_desc").val());

        formData.append("invoice_details[" + index + "].quantity",
            parseFloat(row.find(".qty").val()) || 0);

        formData.append("invoice_details[" + index + "].unit_price",
            parseFloat(row.find(".unit_price").val()) || 0);

        formData.append("invoice_details[" + index + "].tax_percent",
            parseFloat(row.find(".tax_percent").val()) || 0);

        // ✅ FIXED HERE
        formData.append("invoice_details[" + index + "].amount",
            parseFloat(row.find(".amount").text()) || 0);

        formData.append("invoice_details[" + index + "].tax_amount",
            parseFloat(row.find(".tax_amount").text()) || 0);

        formData.append("invoice_details[" + index + "].total_amount",
            parseFloat(row.find(".total_amount").text()) || 0);

    });

    Swal.fire({
        title: 'Do you want to save the changes?',
        showDenyButton: true,
        showCancelButton: true,
        confirmButtonText: 'Ok',
        denyButtonText: 'Cancel',
    }).then((result) => {
        if (result) {
            formData.append("workflow", "COB");
            formData.append("instanceid", 0);

            formData.append("invoice_id", $("#hdnInvoiceId").val() || 0);
            formData.append("invoice_no", $("#invoice_no").val());

            formData.append("invoice_date", $("#invoice_date").val());
            formData.append("due_date", $("#due_date").val());

            formData.append("client_Id", $("#ddlClient").val());
            formData.append("contract_Id", $("#ddlContract").val());

            formData.append("billing_type_id", $("#ddlBillingType").val());
            formData.append("rbilling_Id", $("#ddlRecurringBilling").val());
            formData.append("milestone_Id", $("#ddlMilestone").val());

            formData.append("subtotal", $("#lblSubTotal").val());
            formData.append("taxtotal", $("#lblTaxTotal").val());
            formData.append("grandtotal", $("#lblGrandTotal").val());

            formData.append("userid", 1);

            //// IMPORTANT: DETAIL GRID
            //formData.append("invoiceItems", JSON.stringify(invoiceItems));

            if ($("#inv_pdf")[0].files.length > 0)
                formData.append("attach1_upload", $("#inv_pdf")[0].files[0]);

            if ($("#support_pdf")[0].files.length > 0)
                formData.append("attach2_upload", $("#support_pdf")[0].files[0]);


            //// API call using JSON instead of form-urlencoded
            ////new APICALL(GetGlobalURL('Base', 'SaveInvoiceForm'), 'POST', formData, true, true, false, 'multipart/form-data')
            new APICALL(GetGlobalURL('Base', 'SaveInvoiceForm'), 'POST', formData, true, true)
                .FETCH((result, error) => {
                    //console.log('aaaaa');
                    //console.log(result);
                    //console.log(error);

                    //console.log(result.data.status);
                    //console.log(result.data.message);

                    if (result.data.status === 'error') {
                        Swal.fire({
                            icon: 'error',
                            title: 'Error...',
                            text: result.data.message
                        });
                        return;
                    }

                    if (result && result.status === 'success') {
                        var action = $("#hdnAction").val();
                        var message = '';

                        if (action === 'Add') {
                            message = 'Saved Successfully!';
                        } else {
                            message = 'Update Successfully!';
                        }

                        Swal.fire({
                            icon: 'success',
                            title: 'Success...',
                            text: message
                        });

                        DoEmptyFields();

                        return;
                    }

                    // ERROR block
                    if (error && error.status && error.status !== 200) {
                        Swal.fire({
                            icon: 'error',
                            title: 'Error...',
                            text: error.data?.responseText || 'Something went wrong!'
                        });
                        return;
                    }
                });



        }
    });
}

/* Fill Invoice Form in EDIT Mode */
function GetInvoiceById(invoiceId) {
    new APICALL(GetGlobalURL('Base', 'GetInvoiceById') + '?invoiceId=' + invoiceId, 'GET', '', false).FETCH((result, error) => {

        if (result) {
            if (result.data != null && result.data.length > 0) {

                try {
                    const data = result.data[0];
                    console.log(data);

                    // Invoice Info
                    $("#invoice_no").val(data.invoice_no || '');

                    if (data.invoice_date) {
                        $("#invoice_date").val(data.invoice_date.split('T')[0]);
                    }

                    if (data.due_date) {
                        $("#due_date").val(data.due_date.split('T')[0]);
                    }

                    // Totals
                    $("#subtotal").val(data.subtotal || 0);
                    $("#taxtotal").val(data.taxtotal || 0);
                    $("#grandtotal").val(data.grandtotal || 0);

                    // Hidden Invoice Id
                    $("#hdnInvoiceId").val(data.invoice_id);

                    // Dropdowns
                    setTimeout(() => {
                        $("#ddlClient")
                            .val(String(data.client_Id))
                            .trigger('change');

                        $("#ddlContract")
                            .val(String(data.contract_Id))
                            .trigger('change');

                        $("#ddlBillingType")
                            .val(String(data.billing_type_id))
                            .trigger('change');

                        $("#ddlRecurringBilling")
                            .val(String(data.rbilling_Id))
                            .trigger('change');

                        $("#ddlMilestone")
                            .val(String(data.milestone_Id))
                            .trigger('change');

                    }, 100);

                    // Hidden ID
                    $('#hdnInvoiceId').val(data.invoice_id);
                    $("#hdnAction").val('Edit');

                    // Load Detail Grid
                    LoadInvoiceDetails(data.invoice_id);

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

/* All DDL populate */
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
function GetContractInfoDDL() {
    new APICALL(GetGlobalURL('Base', 'GetContractInfoDDL'), 'GET', '', true).FETCH((result, error) => {
        if (result) {
            if (result.data != null) {
                window.allUsers = result.data;

                var ddl = $('#ddlContract');
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
function GetAllBillingType_DDL() {
    new APICALL(GetGlobalURL('Base', 'GetBillingTypeDDL'), 'GET', '', true).FETCH((result, error) => {
        if (result) {
            if (result.data != null) {
                window.allUsers = result.data;

                var ddl = $('#ddlBillingType');
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
function GetAllRecurringBilling_DDL() {
    new APICALL(GetGlobalURL('Base', 'GetRecurringBillingDDL'), 'GET', '', true).FETCH((result, error) => {
        if (result) {
            if (result.data != null) {
                window.allUsers = result.data;

                var ddl = $('#ddlRecurringBilling');
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
function GetAllMilestone_DDL() {
    new APICALL(GetGlobalURL('Base', 'GetMilestoneDDL'), 'GET', '', true).FETCH((result, error) => {
        if (result) {
            if (result.data != null) {
                window.allUsers = result.data;

                var ddl = $('#ddlMilestone');
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

/* Populate Dropdowns */
function populateDropdowns(ddl, users, selectedManagerId = null, customfield) {
    ddl.empty().append('<option value="0" selected>Please select</option>');

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
function FillDropdowns_OnLoad() {
    GetClientInfoDDL();
    GetContractInfoDDL();
    GetAllBillingType_DDL();
    GetAllRecurringBilling_DDL();
    GetAllMilestone_DDL();
}
function DoEmptyFields() {
    // Hidden Fields
    $("#hdnInvoiceId").val("0");
    $("#hdnAction").val("Add");

    // Invoice Details
    $("#invoice_no").val('');
    $("#ddlClient").val('0');
    $("#invoice_date").val('');
    $("#due_date").val('');
    $("#ddlBillingType").val('0');

    // References
    $("#ddlRecurringBilling").val('0');
    $("#ddlMilestone").val('0');
    $("#ddlContract").val('0');

    // Attachments
    $("#inv_pdf").val('');
    $("#support_pdf").val('');

    // File Previews
    $(".preview-container").empty();

    // Reset Grid
    $("#tbltbody").html(`
        <tr>
            <td>
                <input type="text" class="form-control item_desc" placeholder="Item description">
            </td>
            <td>
                <input type="number" class="form-control form-control-sm qty text-end" value="1">
            </td>
            <td>
                <input type="number" class="form-control form-control-sm unit_price text-end" value="0">
            </td>
            <td class="amount text-end align-middle">0.00</td>
            <td>
                <input type="number" class="form-control form-control-sm tax_percent text-end" value="0">
            </td>
            <td class="tax_amount text-end align-middle">0.00</td>
            <td class="total_amount text-end align-middle fw-semibold">0.00</td>
            <td class="text-center">
                <button type="button" class="btn btn-danger btn-sm btnRemoveRow">
                    <i class="feather-trash-2"></i>
                </button>
            </td>
        </tr>
    `);

    // Totals
    $("#lblSubTotal").text("0.00");
    $("#lblTaxTotal").text("0.00");
    $("#lblGrandTotal").text("0.00");

    // Remove Validation Classes
    $(".is-invalid").removeClass("is-invalid");
}
function setInvalid(selector, message) {
    $(selector).addClass("is-invalid");

    Swal.fire({
        icon: 'warning',
        text: message,
        confirmButtonColor: "#61affe"
    });
}
function CalculateInvoiceTotals() {
    var subTotal = 0;
    var taxTotal = 0;
    var grandTotal = 0;

    $("#tbltbody tr").each(function () {
        var row = $(this);

        var qty = parseFloat(row.find(".qty").val()) || 0;
        var price = parseFloat(row.find(".unit_price").val()) || 0;
        var tax_percent = parseFloat(row.find(".tax_percent").val()) || 0;

        var amount = qty * price;
        var taxAmount = amount * tax_percent / 100;
        var total = amount + taxAmount;

        subTotal += amount;
        taxTotal += taxAmount;
        grandTotal += total;
    });

    $("#lblSubTotal").text(subTotal.toFixed(2));
    $("#lblTaxTotal").text(taxTotal.toFixed(2));
    $("#lblGrandTotal").text(grandTotal.toFixed(2));
}
function ToggleDeleteButtons() {
    if ($("#tbltbody tr").length == 1) {
        $(".btnRemoveRow").hide();
    }
    else {
        $(".btnRemoveRow").show();
    }
}
function setInvoiceDates(invoiceSelector, dueSelector, dueDays = 10) {
    const today = new Date();

    // format YYYY-MM-DD (required for input[type=date])
    const formatDate = (date) => {
        return date.toISOString().split('T')[0];
    };

    // set invoice date = today
    const invoiceDate = formatDate(today);
    $(invoiceSelector).val(invoiceDate);

    // set due date = today + N days
    const dueDate = new Date();
    dueDate.setDate(dueDate.getDate() + dueDays);

    $(dueSelector).val(formatDate(dueDate));
}
function LoadInvoiceDetails(invoiceId) {
    new APICALL(
        GetGlobalURL('Base', 'GetInvoiceDetailByInvoiceId') +
        '?invoiceId=' + invoiceId,
        'GET',
        '',
        false
    ).FETCH((result, error) => {

        if (result && result.data) {

            $("#tbltbody").empty();

            $.each(result.data, function (i, item) {

                var row = `
                <tr>

                    <td>
                        <input type="text"
                               class="form-control item_desc"
                               value="${item.item_desc || ''}">
                    </td>

                    <td>
                        <input type="number"
                               class="form-control form-control-sm qty text-end"
                               value="${item.quantity || 0}">
                    </td>

                    <td>
                        <input type="number"
                               class="form-control form-control-sm unit_price text-end"
                               value="${item.unit_price || 0}">
                    </td>

                    <td class="amount text-end align-middle">
                        ${item.amount || 0}
                    </td>

                    <td>
                        <input type="number"
                               class="form-control form-control-sm tax_percent text-end"
                               value="${item.tax_percent || 0}">
                    </td>

                    <td class="tax_amount text-end align-middle">
                        ${item.tax_amount || 0}
                    </td>

                    <td class="total_amount text-end align-middle fw-semibold">
                        ${item.total_amount || 0}
                    </td>

                    <td class="text-center">
                        <button type="button"
                                class="btn btn-danger btn-sm btnRemoveRow">
                            <i class="feather-trash-2"></i>
                        </button>
                    </td>

                </tr>`;

                $("#tbltbody").append(row);
            });

            CalculateInvoiceTotals();
        }

        if (error) {
            console.log(error);
        }

    });
}
