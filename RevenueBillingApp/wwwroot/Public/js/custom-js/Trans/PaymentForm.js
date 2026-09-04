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

});
async function LoadViewMyRequest(instanceid) {
    FillDropdowns_OnLoad();

    ////setbydefaultvalues_fortesting();

    ////setTimeout(function () {
    ////    $("#ddlClient").val($("#ddlClient option:eq(1)").val());
    ////    $("#ddlInvoiceNo").val($("#ddlInvoiceNo option:eq(1)").val());
    ////}, 2000);


    var paymentId = $("#hdnPaymentId").val();
    if (paymentId && paymentId != "" && paymentId != "0") {
        ////alert('Edit Mode');
        $("#hdnAction").val('Edit');

        GetPaymentById(paymentId);
    }

    var action = $("#hdnAction").val();
    if (action === 'Add') {
        $("#docSection").show();
    } else {
        $("#docSection").hide();
    }

    // Page load par
    togglePaymentFields();

    // Radio change par
    $("input[name='pmode']").change(function () {
        togglePaymentFields();
    });

    $("#ddlInvoiceNo").change(function () {
        let invoiceId = $(this).val();

        if (invoiceId && invoiceId != "0") {
            LoadInvoiceDetail(invoiceId);
        }
        else {
            $("#tblInvoiceDetail tbody").html("");
        }
    });

}
function ValidateAll() {
    $(".is-invalid").removeClass("is-invalid");

    if (!$("#ddlClient").val() || $("#ddlClient").val() == "0") {
        setInvalid("#ddlClient", "Please select Client!");
        return false;
    }

    if (!$("#ddlInvoiceNo").val() || $("#ddlInvoiceNo").val() == "0") {
        setInvalid("#ddlInvoiceNo", "Please select Invoice!");
        return false;
    }

    if (!$("#amount_received").val() || parseFloat($("#amount_received").val()) <= 0) {
        setInvalid("#amount_received", "Please enter Amount Received!");
        return false;
    }

    // Cheque Validation
    if ($("#rdoChq").is(":checked")) {

        if (!$("#chq_no").val()?.trim()) {
            setInvalid("#chq_no", "Please enter Cheque No!");
            return false;
        }

        if (!$("#bank_name").val()?.trim()) {
            setInvalid("#bank_name", "Please enter Bank Name!");
            return false;
        }

        if (!$("#value_date").val()) {
            setInvalid("#value_date", "Please select Value Date!");
            return false;
        }
    }

    // Bank Transfer Validation
    if ($("#rdoBankTransfer").is(":checked")) {

        if (!$("#ref_no").val()?.trim()) {
            setInvalid("#ref_no", "Please enter Reference Number!");
            return false;
        }

        if (!$("#transfer_date").val()) {
            setInvalid("#transfer_date", "Please select Transfer Date!");
            return false;
        }
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

/* Save ClientForm */
function SaveData() {
    var res = ValidateAll();
    if (res == false) {
        return false;
    }

    Swal.fire({
        title: 'Do you want to save the changes?',
        showDenyButton: true,
        showCancelButton: true,
        confirmButtonText: 'Ok',
        denyButtonText: 'Cancel',
    }).then((result) => {
        if (result) {
            var formData = new FormData();
            formData.append("workflow", "COB");
            formData.append("instanceid", 0);

            var rawPaymentId = $("#hdnPaymentId").val();
            ////alert(rawPaymentId);
            var paymentId = (!rawPaymentId || isNaN(rawPaymentId)) ? 0 : parseInt(rawPaymentId);
            ////alert(paymentId);

            formData.append("payment_id", paymentId);

            formData.append("client_id", $("#ddlClient").val() || 0);
            formData.append("invoice_id", $("#ddlInvoiceNo").val() || 0);

            formData.append("amount_received", $("#amount_received").val() || 0);

            formData.append("payment_mode", $("input[name='pmode']:checked").val() || "");

            // Cheque / Bank details
            formData.append("chq_no", $("#chq_no").val() || "");
            formData.append("bank_name", $("#bank_name").val() || "");
            formData.append("value_date", $("#value_date").val() || "");

            formData.append("userid", 1);


            if ($("#receipt_upload")[0].files.length > 0)
                formData.append("receipt_upload", $("#receipt_upload")[0].files[0]);


            ////new APICALL(GetGlobalURL('Base', 'SavePaymentForm'), 'POST', formData, true, true, false, 'multipart/form-data')
            new APICALL(GetGlobalURL('Base', 'SavePaymentForm'), 'POST', formData, true, true)
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

/* Fill Payment Form in EDIT Mode */
function GetPaymentById(paymentId) {
    new APICALL(GetGlobalURL('Base', 'GetPaymentFormById') + '?paymentId=' + paymentId, 'GET', '', false).FETCH((result, error) => {
        if (result) {
            if (result.data != null && result.data.length > 0) {
                try {
                    const data = result.data[0];
                    console.log(data);

                    setTimeout(() => {
                        // 1. Set Client first
                        $('#ddlClient')
                            .val(String(data.client_id))
                            .trigger('change');

                        // 2. WAIT for invoice dropdown to populate
                        let waitInvoice = setInterval(() => {

                            if ($("#ddlInvoiceNo option[value='" + data.invoice_id + "']").length > 0) {

                                $('#ddlInvoiceNo')
                                    .val(String(data.invoice_id))
                                    .trigger('change');

                                clearInterval(waitInvoice);

                                // 3. Load grid after invoice is confirmed
                                LoadInvoiceDetail(data.invoice_id);
                            }

                        }, 100);

                    }, 200);

                    $('#amount_received').val(data.amount_received ?? 0);

                    // Payment Mode radio
                    if (data.payment_mode) {
                        $("input[name='pmode'][value='" + data.payment_mode + "']")
                            .prop('checked', true)
                            .trigger('change');
                    }

                    // Cheque / Bank
                    $('#chq_no').val(data.chq_no ?? '');
                    $('#bank_name').val(data.bank_name ?? '');

                    if (data.value_date) {
                        $('#value_date').val(data.value_date.split('T')[0]);
                    }

                    // Receipt file (optional display only)
                    if (data.receipt_upload) {
                        $('#receiptPreview').html(
                            '<a target="_blank" href="/uploadedfiles/PaymentDocs/' +
                            data.receipt_upload +
                            '">View Receipt</a>'
                        );
                    }

                    // Hidden ID
                    $('#hdnPaymentId').val(data.payment_id);
                    $("#hdnAction").val('Edit');
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
function GetInvoiceDDL() {
    new APICALL(GetGlobalURL('Base', 'GetInvoiceDDL'), 'GET', '', true).FETCH((result, error) => {
        if (result) {
            if (result.data != null) {
                window.allUsers = result.data;

                var ddl = $('#ddlInvoiceNo');
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
    GetInvoiceDDL();
}
function DoEmptyFields() {
    // Main Information
    $("#ddlClient").val('0').trigger('change');
    $("#ddlInvoiceNo").val('0').trigger('change');

    // Payment Information
    $("#amount_received").val('');

    // Payment Mode
    $("#rdoCash").prop("checked", true);

    // Cheque Details
    $("#chq_no").val('');
    $("#bank_name").val('');
    $("#value_date").val('');

    // Bank Transfer Details
    $("#ref_no").val('');
    $("#transfer_date").val('');

    // File Upload
    $("#doc_upload").val('');
    $(".preview-container").html('');

    // Trigger payment mode UI
    $("input[name='pmode']").trigger("change");

}
function setInvalid(selector, message) {
    $(selector).addClass("is-invalid");

    Swal.fire({
        icon: 'warning',
        text: message,
        confirmButtonColor: "#61affe"
    });
}
function togglePaymentFields() {
    if ($("#rdoCash").is(":checked")) {
        $("#chq_no, #bank_name, #value_date").prop("disabled", true).val("");
    }
    else {
        $("#chq_no, #bank_name, #value_date").prop("disabled", false);
    }
}
function TogglePaymentMode() {
    // not used

    if ($("#rdoCash").is(":checked")) {

        $("#check").addClass("d-none");
        $("#bank").addClass("d-none");

        $("#chq_no,#bank_name,#value_date,#ref_no,#transfer_date")
            .prop("disabled", true);

    }
    else if ($("#rdoChq").is(":checked")) {

        $("#check").removeClass("d-none");
        $("#bank").addClass("d-none");

        $("#chq_no,#bank_name,#value_date")
            .prop("disabled", false);

        $("#ref_no,#transfer_date")
            .prop("disabled", true);
    }
    else if ($("#rdoBankTransfer").is(":checked")) {

        $("#check").addClass("d-none");
        $("#bank").removeClass("d-none");

        $("#chq_no,#bank_name,#value_date")
            .prop("disabled", true);

        $("#ref_no,#transfer_date")
            .prop("disabled", false);
    }
}
function LoadInvoiceDetail(invoiceId) {
    new APICALL(GetGlobalURL('Base', 'GetInvoiceDetailByInvoiceId') + '?invoiceId=' + invoiceId,
        'GET',
        '',
        false
    ).FETCH((result, error) => {
        if (result) {
            let html = '';

            let subTotal = 0;
            let taxTotal = 0;
            let grandTotal = 0;

            if (result.data != null && result.data.length > 0) {
                $.each(result.data, function (i, item) {

                    let amount = parseFloat(item.amount ?? 0);
                    let taxAmount = parseFloat(item.tax_amount ?? 0);
                    let totalAmount = parseFloat(item.total_amount ?? 0);

                    subTotal += amount;
                    taxTotal += taxAmount;
                    grandTotal += totalAmount;

                    html += `
                        <tr>
                            <td>${item.item_desc ?? ''}</td>
                            <td class="text-end">${item.quantity ?? 0}</td>
                            <td class="text-end">${item.unit_price ?? 0}</td>
                            <td class="text-end">${item.amount ?? 0}</td>
                            <td class="text-end">${item.tax_percent ?? 0}</td>
                            <td class="text-end">${item.tax_amount ?? 0}</td>
                            <td class="text-end">${item.total_amount ?? 0}</td>
                        </tr>`;
                });
            }
            else {
                html = `
                    <tr>
                        <td colspan="7" class="text-center">
                            No Record Found
                        </td>
                    </tr>`;
            }

            $("#tblInvoiceDetail tbody").html(html);

            // 🔥 Fill Footer Totals
            $("#lblSubTotal").text(subTotal.toFixed(2));
            $("#lblTaxTotal").text(taxTotal.toFixed(2));
            $("#lblGrandTotal").text(grandTotal.toFixed(2));
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

/* for development testing */
function setbydefaultvalues_fortesting() {
    // Select first available records
    $("#ddlClient").prop('selectedIndex', 1).trigger('change');
    $("#ddlInvoiceNo")
        .prop('selectedIndex', 1)
        .trigger('change');   // 🔥 IMPORTANT

    // Amount
    $("#amount_received").val("10000");

    // Payment Mode
    $("#rdoChq").prop("checked", true);

    // Cheque Details
    $("#chq_no").val("CHQ-123456");
    $("#bank_name").val("HBL");
    $("#value_date").val("2025-08-15");

    // Bank Transfer Details
    $("#ref_no").val("TXN-987654");
    $("#transfer_date").val("2025-08-15");

    $("input[name='pmode']").trigger("change");

    console.log("Payment Form default testing values loaded.");

}
