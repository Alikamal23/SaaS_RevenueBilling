var instanceid = 0;
var isSaving = false;
var skipContractSave = false;

$(document).on("change", ".upload-file", function () {
    var fileName = this.files.length > 0
        ? this.files[0].name
        : "PDF, DOCX, DOC";

    var text = $(this)
        .closest(".upload-box")
        .find(".file-text");

    if (this.files.length > 0) {
        text.html('<i class="fa fa-check text-success"></i> ' + fileName);
        text.removeClass("text-muted");
        text.css("color", "green");
    }
    else {
        text.text("PDF, DOCX, DOC");
        text.addClass("text-muted");
        text.css("color", "");
    }
});

$(document).ready(function () {
    FillDropdowns_OnLoad();

    $("#client-onboarding").steps({
        headerTag: "div.step-title",
        bodyTag: "section.step-body",
        transitionEffect: "slideLeft",
        autoFocus: false,
        enableAllSteps: false,   // <-- Change this
        enablePagination: true,
        onStepChanging: function (event, currentIndex, newIndex) {
            if (skipContractSave) {
                skipContractSave = false;
                return true;
            }
            
            // Previous Step always allowed
            if (newIndex < currentIndex)
                return true;

            if (!ValidateSteps(currentIndex))
                return false;

            // Contract -> Invoice
            if (currentIndex == 1 && newIndex == 2) {
                if (isSaving)
                    return false;

                isSaving = true;

                SaveData(1, function (success) {
                    isSaving = false;

                    if (!success)
                        return;

                    GetInvoiceGrid(
                        $("#hdnClientId").val(),
                        $("#hdnContractId").val()
                    );


                    EnableStep(2);
                    skipContractSave = true;

                    setTimeout(function () {
                        $("#client-onboarding").steps("next");

                        SetDefaultInvoiceDates();
                        SetDefaultInvoiceAmounts();
                        LoadMaxInvoiceID();

                        // Grid ya Form — decide karo
                        DecideInvoiceView(
                            $("#hdnClientId").val(),
                            $("#hdnContractId").val()
                        );


                        FillInvoiceForEditManual(
                            $("#hdnClientId").val(),
                            $("#hdnContractId").val()
                        );

                        FillPaymentForEdit(
                            $("#hdnClientId").val(),
                            $("#hdnContractId").val()
                        );

                        GetARExceptionGrid(
                            $("#hdnClientId").val(),
                            $("#hdnContractId").val()
                        );

                        GetRenewalGrid(
                            $("#hdnClientId").val(),
                            $("#hdnContractId").val()
                        );

                        //DisableContractTab(true);

                    }, 100);

                }, false);

                return false;
            }


            if ($("#hdnPaymentId").val() == 0) {
                $("#amount_received").val('');
            }


            // Invoice -> Payment
            if (currentIndex == 2 && newIndex == 3) {

                var params = new URLSearchParams(window.location.search);
                var type = params.get("type");

                // Edit Mode - Invoice already exists
                if (type === "editfromgrid") {
                    $("#paymentGrid").show();

                    GetUnPaidInvoiceList(
                        $("#hdnClientId").val(),
                        $("#hdnContractId").val()
                    );

                    FillPaymentForEdit(
                        $("#hdnClientId").val(),
                        $("#hdnContractId").val()
                    );

                    GetARExceptionGrid(
                        $("#hdnClientId").val(),
                        $("#hdnContractId").val()
                    );

                    GetRenewalGrid(
                        $("#hdnClientId").val(),
                        $("#hdnContractId").val()
                    );


                    EnableStep(3);
                    return true;
                }


                // ============================
                // NAYA: Sirf tab invoice save karo jab Manual Form active ho
                // ============================
                var isManualFormActive = !$("#manualGrid").hasClass("d-none");

                if (!isManualFormActive) {
                    // Grid dikh rahi hai — koi naya invoice save nahi karna, seedha Payment pe jao
                    GetUnPaidInvoiceList(
                        $("#hdnClientId").val(),
                        $("#hdnContractId").val()
                    );

                    EnableStep(3);
                    skipContractSave = true;

                    setTimeout(function () {
                        $("#client-onboarding").steps("next");
                        /*$("#paymentGrid").hide();*/

                        FillPaymentForEdit(
                            $("#hdnClientId").val(),
                            $("#hdnContractId").val()
                        );
                    }, 100);

                    return false;
                }


                // Add Mode — Manual Form active hai, invoice save karo
                if (isSaving)
                    return false;

                isSaving = true;

                SaveData(2, function (success) {
                    isSaving = false;

                    if (!success)
                        return;

                    GetUnPaidInvoiceList(
                        $("#hdnClientId").val(),
                        $("#hdnContractId").val()
                    );

                    EnableStep(3);

                    skipContractSave = true;

                    setTimeout(function () {
                        $("#client-onboarding").steps("next");

                        //$("#paymentGrid").hide();

                        FillPaymentForEdit(
                            $("#hdnClientId").val(),
                            $("#hdnContractId").val()
                        );

                    }, 100);

                }, false);

                return false;
            }


            // Payment -> AR Exception
            if (currentIndex == 3 && newIndex == 4) {

                    //if (!ValidateSteps(4))
                    //    return false;

                    if (isSaving)
                        return false;

                    isSaving = true;


                    SaveData(3, function (success) {
                        isSaving = false;

                        if (!success)
                            return;


                        EnableStep(4);

                        skipContractSave = true;

                        GetOverdueInvoices(
                            $("#hdnClientId").val(),
                            $("#hdnContractId").val()
                        );

                        GetARExceptionGrid(
                            $("#hdnClientId").val(),
                            $("#hdnContractId").val()
                        );

                        
                        setTimeout(function () {
                            $("#client-onboarding").steps("next");


                        }, 100);

                    }, false);

                return false;
            }

            // AR Exception -> Renewal
            if (currentIndex == 4 && newIndex == 5) {

                //if (!ValidateSteps(5))
                //    return false;

                if (isSaving)
                    return false;

                isSaving = true;

                SaveData(4, function (success) {
                    isSaving = false;
                    if (!success)
                        return;

                    EnableStep(5);

                    GetRenewalGrid(
                        $("#hdnClientId").val(),
                        $("#hdnContractId").val()
                    );

                    skipContractSave = true;

                    setTimeout(function () {
                        $("#client-onboarding").steps("next");
                    }, 100);

                }, false);

                return false;
            }

            EnableStep(newIndex);

            return true;

        },
        onFinishing: function (event, currentIndex) {
            //return ValidateSteps(7);

            //if (!ValidateSteps(6))
            //    return false;

            ////if (!ValidateSteps(currentIndex))
            ////    return false;

            if (isSaving)
                return false;

            isSaving = true;

            SaveData(5, function (success) {
                isSaving = false;

                if (!success)
                    return;

                Swal.fire({
                    icon: "success",
                    title: "Saved",
                    text: "Client Onboarding Completed Successfully."
                }).then(function () {
                    // Reload Page
                    location.reload();

                    ////ya agar list page pe bhejna ho
                    //window.location.href = "/BillingRevenue/ClientList";
                });

            }, false);

            return false;   // Wizard ko khud finish na karne do

        },
        onFinished: function (event, currentIndex) {
            // submit / save logic here
        }
    });


    initFileUploaders();
    initMultiFieldGroups();
    initPaymentModeToggle();   // Event bind + initial state
    toggleClient();
    toggleContractType();
    toggleAutoManualInvoice();

    $("input[name='new_existing']").change(function () {
        toggleClient();
    });

    $("#ddlbasicContractType").change(function () {
        toggleContractType();
    });

    //if contractType : Custom --> Milestone
    $("#m_taxper").keypress(function (e) {
        if (e.which == 13) {
            e.preventDefault();

            calculateMilestoneAmount();
            $("#noMilestone").focus();
        }

    });

    $("#noMilestone").on("input", function () {
        this.value = this.value.replace(/[^0-9]/g, "");

        if (parseInt(this.value) > 12) {
            this.value = 12;
        }
    });

    $("#contract_value, #discount, #tax").keypress(function (e) {
        if (e.which != 13)
            return;

        e.preventDefault();

        switch (this.id) {

            case "contract_value":
                $("#discount").focus();
                break;

            case "discount":
                $("#tax").focus();
                break;

            case "tax":
                calculateSaaSTotal();
                break;
        }
    });

    $("#contract_value, #discount, #tax").on("input", function () {
        calculateSaaSTotal();
    });

    $("#noMilestone").keypress(function (e) {
        if (e.which == 13) {
            e.preventDefault();
            generateMilestoneGrid();
        }
    });

    $("#noMilestone").on("input", function () {
        var value = parseInt($(this).val()) || 0;

        if (value > 12) {
            $(this).val(12);

            Swal.fire({
                icon: "warning",
                title: "Maximum Limit",
                text: "You can create a maximum of 12 milestones."
            });
        }

        generateMilestoneGrid();
    });

    $("#projectValue, #m_discountper, #m_taxper").keypress(function (e) {
        if (e.which != 13) return;
        e.preventDefault();

        switch (this.id) {
            case "projectValue":
                $("#m_discountper").focus();
                break;

            case "m_discountper":
                $("#m_taxper").focus();
                break;

            case "m_taxper":
                $("#noMilestone").focus();
                break;
        }
    });

    $("#projectValue, #m_discountper, #m_taxper").on("input", function () {
        calculateMilestoneAmount();

        // Agar milestone already enter hai to grid bhi update ho
        if ($("#noMilestone").val() > 0) {
            generateMilestoneGrid();
        }
    });

    $(document).on("input", ".milestoneAmount", function () {
        calculateGrandTotal();
    });

    // searching by mobile no format / masking 923001234567
    $(document).on("input", "#primary_contact_phone", function () {
        //let value = this.value.replace(/\D/g, "");
        let value = normalizePakMobile(this.value);

        if (value.length > 12)
            value = value.substring(0, 12);

        if (value.length > 12)
            value = value.substring(0, 12);

        if (value.length > 5)
            this.value = value.replace(/^(\d{2})(\d{3})(\d{0,7}).*/, "$1-$2-$3");
        else if (value.length > 2)
            this.value = value.replace(/^(\d{2})(\d+)/, "$1-$2");
        else
            this.value = value;



    });


    var clientSearchTimer = null;
    $(document)
        .off("input.clientExistingSearch", "#clientSearchValue")
        .on("input.clientExistingSearch", "#clientSearchValue", function () {

            clearTimeout(clientSearchTimer);

            var searchValue = $(this).val().trim();

            // ==============================
            // Empty / less than 2 characters
            // ==============================
            if (searchValue.length < 2) {

                $("#existingClientResults")
                    .stop(true, true)
                    .slideUp(200, function () {
                        $(this).addClass("d-none");
                    });

                $("#existingClientGrid tbody").empty();

                $("#hdnClientId").val("");
                $("#hdnAction").val("Add");

                return;
            }

            // ==============================
            // Search after user stops typing
            // ==============================
            clientSearchTimer = setTimeout(function () {

                FillExistingClient(searchValue);

            }, 250);

        });


    $(document).on("input", "#client_name", function () {
        $("#con_clientname").val($(this).val());
        $("#invTab_con_clientname").val($(this).val());
        $("#paym_con_clientname").val($(this).val());
        $("#ar_con_clientname").val($(this).val());
        $("#renew_con_clientname").val($(this).val());
    });

    $(document).on("input", "#ddlbasicContractType", function () {
        $("#invTab_contracttype").val($(this).val());
        $("#invTab_contracttype").prop("disabled", true);

        $("#paym_contracttype").val($(this).val());
        $("#paym_contracttype").prop("disabled", true);

        $("#ar_contracttype").val($(this).val());
        $("#ar_contracttype").prop("disabled", true);

        $("#renew_contracttype").val($(this).val());
        $("#renew_contracttype").prop("disabled", true);
    });

    $("#SaveBtn").click(function () {
        var currentStep = $("#client-onboarding").steps("getCurrentIndex");

        if (!ValidateSteps(currentStep))
            return;

        if (currentStep == 2 && $("#ddlbasicContractType").val() == "2" && $("hdnInvoiceEditMode").val() == 0) {
            // Milestone
            if ($("#ddlInvoiceMilestone").val() == "" ||
                $("#ddlInvoiceMilestone").val() == null ||
                $("#ddlInvoiceMilestone").val() == "0") {

                Swal.fire({
                    icon: "warning",
                    title: "Validation",
                    text: "Please select Milestone."
                });

                $("#ddlInvoiceMilestone").focus();
                return false;
            }
        }

        Swal.fire({
            title: 'Do you want to save changes?',
            icon: 'question',
            showCancelButton: true,
            confirmButtonText: 'Save'
        }).then((r) => {
            if (!r.value)
                return;

            SaveData(currentStep, null, true);

        });

    });

    //////---------start------------------------------------------
    FileInfo();
    //////---------end---------------------------------------------


    //////---------start------------------------------------------
    PopulateFieldsOnEdit();
    //////---------end---------------------------------------------


    $(document).on("keydown", ".milestoneAmount, .milestoneDate", function (e) {
        if ($("#hdnContractEditMode").val() == "1") {
            e.preventDefault();
            return false;
        }
    });

    // invoice Tab Line Detail Grid
    $("#btnAddRow").on("click", function () {
        if (!ValidateLastRowInvoice())
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

        //toggleDeleteButtons();
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
    });

    $("input[name='inv_method']").change(function () {
        toggleAutoManualInvoice();
    });


    $(document).off("click", ".EditInvoice");

    $(document).on("click", ".EditInvoice", function () {
        $("#hdnInvoiceEditMode").val("1");
        $("#hdnInvoiceId").val($(this).data("id"));
        $("#txtInvoiceNo").val($(this).data("no"));

        var d = $(this).data("date");
        var due = $(this).data("due");
        $("#txtInvoiceDate").val(d ? String(d).split("T")[0] : "");
        $("#txtInvoiceDueDate").val(due ? String(due).split("T")[0] : "");

        $("#invGrossAmount").val($(this).data("gross"));
        $("#invDiscountPercent").val($(this).data("disc"));
        $("#invTaxPercent").val($(this).data("tax"));

        $("#hdnInvoiceMilestoneNo").val($(this).data("milestone") || "");
        $("#hdnInvoiceBillingPeriod").val($(this).data("billingperiod") || "");

        // Contract type ke mutabik Discount/Tax enable/disable
        var contractType = $("#ddlbasicContractType").val();
        if (contractType == "2") {
            $("#invDiscountPercent").prop("disabled", true);
            $("#invTaxPercent").prop("disabled", true);
            $("#milestoneSelectWrapper").addClass("d-none");   // edit mode mein milestone switch na ho
        } else {
            $("#invDiscountPercent").prop("disabled", false);
            $("#invTaxPercent").prop("disabled", false);
        }

        $("#hdnInvoiceMilestoneNo").val($(this).data("milestone") || "");
        $("#hdnInvoiceBillingPeriod").val($(this).data("billingperiod") || "");

        calculateInvoiceAmount();   // Invoice Amount / Tax Amount / Total sab turant recalc

        $("#rdoManualInvoice").prop("checked", true).trigger("change");
        toggleAutoManualInvoice();

        $("#autoGrid").addClass("d-none");
        $("#manualGrid").removeClass("d-none");
        $("#btnBackInvoiceGrid").show();

    });

    $("#btnBackInvoiceGrid").click(function () {

        // Hide Manual Form
        $("#manualGrid").addClass("d-none");

        // Show Invoice List
        $("#autoGrid").removeClass("d-none");

        // Hide Back Button
        $("#btnBackInvoiceGrid").hide();

        // Exit Edit Mode
        $("#hdnInvoiceEditMode").val("0");
        $("#hdnInvoiceId").val("0");

        // Optional
        //ClearInvoiceForm();

    });

    $(document).on("input", "#invGrossAmount, #invBaseAmount, #invDiscountPercent, #invTaxPercent", function () {
        calculateInvoiceAmount();
    });

    $("#btnAddNewInvoice").click(function () {
        $("#hdnInvoiceId").val(0);
        $("#hdnInvoiceEditMode").val("0");

        $("#autoGrid").addClass("d-none");
        $("#manualGrid").removeClass("d-none");
        $("#btnBackInvoiceGrid").show();   // ab grid maujood hai, Back se wapis ja sakte

        LoadMaxInvoiceID();
        SetDefaultInvoiceAmounts();
    });

    //++******* start Preview Document on Modal Form
    PreviewDocument();
    //++******* end Preview Document on Modal Form


});

function PopulateFieldsOnEdit() {
    ////---------------------------------------------------------
    var params = new URLSearchParams(window.location.search);

    var type = params.get("type");
    var clientId = params.get("ClientId");
    var contractId = params.get("ContractId");

    if (type == "editfromgrid") {
        $("#paymentGrid").hide();      // Hide Payment Grid
        $("#hdnAction").val("Edit");

        ////hide add new button always
        //$("#btnAddNewInvoice").addClass("d-none");

        //DisableContractTab(true);

        $("#CustomerRadioBoxBlock").hide();
        $("#primary_contact_phone").prop("disabled", true);
        $("#ddlbasicContractType").prop("disabled", true);
        $("#invTab_contracttype").prop("disabled", true);
        $("#paym_contracttype").prop("disabled", true);
        $("#ar_contracttype").prop("disabled", true);
        $("#renew_contracttype").prop("disabled", true);

        if (contractId == 0) {
            LoadMaxContractID();
            $("#ddlbasicContractType").prop("disabled", false);
            $("#invTab_contracttype").prop("disabled", false);
            $("#paym_contracttype").prop("disabled", false);
            $("#ar_contracttype").prop("disabled", false);
            $("#renew_contracttype").prop("disabled", false);

            // Wizard Steps
            EnableStep(0);   // Client
            EnableStep(1);   // Contract

            // Disable remaining steps
            DisableStep(2);   // Invoice
            DisableStep(3);   // Payment
            DisableStep(4);   // AR
            DisableStep(5);   // Renewal

        }

        //FillDropdowns_OnLoad();

        FillClientContractForEdit(clientId, contractId);
        FillInvoiceForEditManual(clientId, contractId);
        FillPaymentForEdit(clientId, contractId);

        GetARExceptionGrid(clientId, contractId);
        GetRenewalGrid(clientId, contractId);


        if ($("#ddlbasicContractType").val() == "1") {
            //show add new button on SaaS Contract
            $("#btnAddNewInvoice").removeClass("d-none");

        } else { //Custom Milestone
            //hide add new button on Custom Contract
            $("#btnAddNewInvoice").addClass("d-none");
        }


        /*GetInvoiceGrid(clientId, contractId);*/
        GetUnPaidInvoiceList(clientId, contractId);

        // NAYA: Grid ya Form decide karo
        DecideInvoiceView(clientId, contractId);

        setTimeout(function () {
            $("#hdnAction").val("Edit");
        }, 200);

        // Wizard Steps
        //EnableStep(0);   // Client
        EnableStep(1);   // Contract

        //$("#rdoAutoInvoice").prop("checked", true).trigger("change");
        //toggleAutoManualInvoice();

    }
    else {
        $("#paymentGrid").show();      // show Payment Grid

        //DisableContractTab(false);

        $("#CustomerRadioBoxBlock").show();
        $("#primary_contact_phone").prop("disabled", false);
        $("#ddlbasicContractType").prop("disabled", false);
        $("#invTab_contracttype").prop("disabled", false);
        $("#paym_contracttype").prop("disabled", false);
        $("#ar_contracttype").prop("disabled", false);
        $("#renew_contracttype").prop("disabled", false);

        AutoGenerateID();

        clientId = $("#hdnClientId").val();
        contractId = $("#hdnContractId").val();

        if (parseInt(clientId) > 0) {
            FillClientContractForEdit(clientId, contractId);
            FillInvoiceForEditManual(clientId, contractId);
            FillPaymentForEdit(clientId, contractId);

            GetInvoiceGrid(clientId, contractId);
            GetUnPaidInvoiceList(clientId, contractId);

            GetARExceptionGrid(clientId, contractId);
            GetRenewalGrid(clientId, contractId);

        }
        else {
            LoadMaxInvoiceID();
        }

        //DisableStep(4);
        //DisableStep(5);

        $("#rdoAutoInvoice").prop("checked", true).prop("disabled", false).trigger("change");
        toggleAutoManualInvoice();

    }

}

// invoice Tab Line Detail Grid START ******
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
function toggleDeleteButtons() {
    if ($("#tbltbody tr").length == 1) {
        $(".btnRemoveRow").hide();
    }
    else {
        $(".btnRemoveRow").show();
    }
}
function toggleAutoManualInvoice() {
    if ($("#rdoAutoInvoice").is(":checked")) {
        $("#manualGrid").addClass("d-none");
        $("#autoGrid").removeClass("d-none");
    }
    else if ($("#rdoManualInvoice").is(":checked")) {
        $("#manualGrid").removeClass("d-none");
        $("#autoGrid").addClass("d-none");
    }
}
// invoice Tab - END here ************

function initFileUploaders() {
    $(document).on('change', '.file-input', function () {
        const $input = $(this);
        const uploader = $input.closest('.uploader');
        const $previewContainer = uploader.find('.preview-container');

        Array.from(this.files).forEach(file => {
            const $col = $('<div class="col-6 mt-3"></div>');
            const $preview = $('<div class="preview-item"></div>');

            const $removeBtn = $('<button type="button" class="remove-btn">&times;</button>');
            $removeBtn.on('click', function () {
                $col.remove();
            });
            $preview.append($removeBtn);

            if (file.type.startsWith('image/')) {
                const $img = $('<img>').attr('src', URL.createObjectURL(file));
                $preview.append($img);
            } else {
                const $icon = $('<div></div>').css({
                    height: '120px',
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    fontSize: '50px'
                }).html('📄');
                $preview.append($icon);
            }

            const $name = $('<div class="mt-2 small"></div>').text(file.name);
            $preview.append($name);

            $col.append($preview);
            $previewContainer.append($col);
        });

        $input.val('');
    });
}
function initMultiFieldGroups() {
    $('.multi-field-wrapper').each(function () {
        const $wrapper = $(this);
        const $fields = $wrapper.find('.multi-fields');

        $wrapper.on('click', '.add-field', function () {
            const $clone = $fields.find('.multi-field').first().clone(true);
            $clone.find('input').val('');
            $fields.append($clone);
            $clone.find('input').focus();
        });

        $wrapper.on('click', '.remove-field', function () {
            if ($fields.find('.multi-field').length > 1) {
                $(this).closest('.multi-field').remove();
            }
        });
    });
}
function initPaymentModeToggle() {
    $(document).on('change', 'input[name="payment_mode"]', togglePaymentFields);

    // Initial page load state
    togglePaymentFields();
}
function togglePaymentFields() {
    const $refRow = $('#payment_reference_fields');

    if ($('#pm_cash').is(':checked')) {
        $refRow.addClass('d-none');

    }
    else if ($('#pm_chq').is(':checked')) {
        $refRow.removeClass('d-none');

        $('#lblRefNo').text('Cheque No');
        $('#lblRefDate').text('Cheque Date');

    }
    else if ($('#pm_bank').is(':checked')) {
        $refRow.removeClass('d-none');

        $('#lblRefNo').text('Reference No');
        $('#lblRefDate').text('Transfer Date');

    }

}
function toggleClient() {
    var type = new URLSearchParams(window.location.search).get("type");

    if (type == "editfromgrid") {
        return;
    }

    if ($("#ExistingClient").is(":checked")) {
        //$("#existing_client").show();
        $("#new_client").show();

        //search div
        $("#search_client").removeClass("d-none").show();

        setClientInfoState(false);
        $("#hdnAction").val('Edit');
        $("#primary_contact_phone").prop("disabled", true);

        //set default field is mobile
        $("#clientSearchBy").val('primary_contact_phone');
        $("#client_mobileNo").focus();
    }
    else if ($("#NewClient").is(":checked")) {
        $("#new_client").show();

        ClearAllFields();

        //search div
        $("#search_client").hide();
        $("#search_client").addClass("d-none").hide();

        setClientInfoState(true);

        $("#primary_contact_phone").prop("disabled", false);

        LoadMaxClientID();
        $("#hdnAction").val('Add');

    }
}
function toggleContractType() {
    var contractType = $("#ddlbasicContractType option:selected").text().trim().toLowerCase();
    // Agar value se check karna ho to:
    // var contractType = $("#ddlbasicContractType").val();

    if (contractType == "saas") {
        $("#saas").removeClass("d-none");
        $("#custom").addClass("d-none");

        $("#docSection").removeClass("d-none");

        // Billing Type = Recurring
        $("#ddlBillingType")
            .val("1")
            .prop("disabled", true);

        $("#total_saas_amount").prop("disabled", true);

    }
    else if (contractType == "custom") {
        $("#custom").removeClass("d-none");
        $("#saas").addClass("d-none");

        $("#docSection").removeClass("d-none");

        // Billing Type = Milestone
        $("#ddlBillingType2")
            .val("2")
            .prop("disabled", true);

        $("#final_amount").prop("disabled", true);

    }
    else {
        $("#saas").addClass("d-none");
        $("#custom").addClass("d-none");

        $("#docSection").addClass("d-none");

        $("#ddlBillingType")
            .val("0")
            .prop("disabled", false);

        $("#ddlBillingType2")
            .val("0")
            .prop("disabled", false);

        $("#total_saas_amount").prop("disabled", true);
        $("#final_amount").prop("disabled", true);

    }
}
function InitPaymentGridEvents() {
    // Select All
    $(document).off('change', '#chkAllInvoice').on('change', '#chkAllInvoice', function () {
        $('.chkInvoice').prop('checked', $(this).is(':checked'));
        CalculatePaymentAmount();
    });

    // Single Checkbox
    $(document).off('change', '.chkInvoice').on('change', '.chkInvoice', function () {
        $('#chkAllInvoice').prop(
            'checked',
            $('.chkInvoice').length == $('.chkInvoice:checked').length
        );

        CalculatePaymentAmount();
    });
}
function CalculatePaymentAmount() {
    var total = 0;

    $('.chkInvoice:checked').each(function () {
        total += parseFloat($(this).data('amount')) || 0;
    });

    $('#amount_received').val(total.toFixed(2));
}
function GetSelectedInvoices() {
    var invoices = [];

    $('.chkInvoice:checked').each(function () {
        invoices.push(parseInt($(this).val()));
    });

    return invoices;
}
function calculateSaaSTotal() {
    var contract = parseFloat($("#contract_value").val()) || 0;
    var discountPer = parseFloat($("#discount").val()) || 0;
    var taxPer = parseFloat($("#tax").val()) || 0;

    var discountAmt = (contract * discountPer) / 100;
    var afterDiscount = contract - discountAmt;

    var taxAmt = (afterDiscount * taxPer) / 100;
    var total = afterDiscount + taxAmt;

    $("#total_saas_amount").val(FormatAmount(total.toFixed(2)));
}
function calculateInvoiceAmount() {
    var gross = parseFloat($("#invGrossAmount").val()) || 0;
    var discPercent = parseFloat($("#invDiscountPercent").val()) || 0;
    var taxPercent = parseFloat($("#invTaxPercent").val()) || 0;

    var discAmt = gross * discPercent / 100;
    var netAmount = gross - discAmt;          // yeh hai invoice_amount

    var taxAmt = netAmount * taxPercent / 100;
    var total = netAmount + taxAmt;

    $("#invNetAmount").val(netAmount.toFixed(2));
    $("#invTaxAmount").val(taxAmt.toFixed(2));
    $("#txtInvoiceAmount").val(total.toFixed(2));
}
function calculateMilestoneAmount() {
    var projectValue = parseFloat($("#projectValue").val()) || 0;
    var discountPer = parseFloat($("#m_discountper").val()) || 0;
    var taxPer = parseFloat($("#m_taxper").val()) || 0;

    var afterDiscount = projectValue - (projectValue * discountPer / 100);
    var finalAmount = afterDiscount + (afterDiscount * taxPer / 100);

    $("#final_amount").val(FormatAmount(finalAmount.toFixed(2)));
}
function generateMilestoneGrid() {
    var finalAmount = parseFloat(UnFormatAmount($("#final_amount").val())) || 0;
    var milestone = parseInt($("#noMilestone").val()) || 0;

    if (milestone <= 0)
        return;

    if (milestone > 12) {
        Swal.fire({
            icon: "warning",
            title: "Invalid Milestones",
            text: "Maximum 12 milestones are allowed."
        });

        $("#noMilestone").val(12).focus();
        milestone = 12;
    }

    var amount = finalAmount / milestone;

    $("#tblMilestone tbody").html("");

    //for (var i = 1; i <= milestone; i++) {
    //    $("#tblMilestone tbody").append(`
    //        <tr>
    //            <td>Milestone ${i}</td>
    //            <td>
    //                <input type="number" class="text-end form-control form-control-sm milestoneAmount" value="${amount.toFixed(2)}">
    //            </td>
    //            <td>
    //                <input type="date" class="form-control form-control-sm">
    //            </td>
    //        </tr>
    //    `);
    //}

    for (var i = 1; i <= milestone; i++) {
        $("#tblMilestone tbody").append(`
            <tr data-milestone-no="${i}" data-status="Pending">
                <td>Milestone ${i}</td>
                <td>
                    <input type="number" class="text-end form-control form-control-sm milestoneAmount" value="${amount.toFixed(2)}">
                </td>
                <td>
                    <input type="date" class="form-control form-control-sm milestoneDate">
                </td>
            </tr>
        `);
    }

    calculateGrandTotal();

    // Agar Contract Edit Mode hai to naye controls bhi disable kar do
    if ($("#hdnContractEditMode").val() == "1") {
        console.log('line reached.... means edit mode contract');

        $("#tblMilestone")
            .find("input, select, textarea, button")
            .prop("disabled", true)
            .prop("readonly", true);
    }

}
function calculateGrandTotal() {
    var total = 0;

    $(".milestoneAmount").each(function () {
        total += parseFloat($(this).val()) || 0;
    });

    $("#txtGrandTotal").val(total.toFixed(2));
}
function setClientInfoState(enable) {
    $("#clientInfoSection")
        .find("input, select, textarea")
        .prop("disabled", !enable);

    $("#client_refno").prop("disabled", true);

}
function ClearForm(formId) {
    var form = $(formId)[0];

    if (!form)
        return;

    // Reset normal controls
    form.reset();

    // Select2
    $(formId).find("select").trigger("change");

    // Remove validation
    $(formId).find(".is-invalid").removeClass("is-invalid");

    // Clear file preview
    $(formId).find(".preview-container").empty();

    // File inputs
    $(formId).find("input[type='file']").val("");

}

// +++++++ ******** start existing client search by name or mobile number **********************
function FillExistingClient(searchValue) {
    console.log("Searching Client:", searchValue);

    new APICALL(
        GetGlobalURL('Base', 'FillExistingClient')
        + '?searchValue=' + encodeURIComponent(searchValue),
        'GET',
        '',
        false
    ).FETCH((result, error) => {
        //console.log("API RESULT:", result);
        //console.log("API ERROR:", error);

        if (error) {
            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: error.data.responseText
            });
            return;
        }

        var tbody = $("#existingClientGrid tbody");

        //console.log("tbody found:", tbody.length);

        tbody.empty();

        if (!result || !result.data || result.data.length === 0) {
            console.log("NO CLIENT FOUND");

            //$("#existingClientResults").addClass("d-none");
            $("#existingClientResults")
                .stop(true, true)
                .slideUp(200, function () {
                    $(this).addClass("d-none");
                });

            $("#hdnClientId").val("");
            $("#hdnAction").val("Add");

            return;
        }

        //console.log("CLIENTS FOUND:", result.data.length);
        $("#existingClientCount").text(result.data.length);

        $.each(result.data, function (index, data) {
            var row = `
                <tr class="existing-client-row" data-client-index="${index}" style="cursor:pointer;">
                    <td>${data.client_refno ?? '-'}</td>
                    <td>
                        <strong>${data.client_name ?? '-'}</strong>
                    </td>
                    <td>${data.primary_contact_name ?? '-'}</td>
                    <td>${data.primary_contact_phone ?? '-'}</td>
                    <td>${data.primary_contact_email ?? '-'}</td>
                </tr>
            `;

            tbody.append(row);
        });

        $("#existingClientGrid").data("clients", result.data);

        // Show container
        $("#existingClientResults")
            .removeClass("d-none")
            .show();


        // Show accordion body
        var body = $("#existingClientBody");

        body.stop(true, true).slideDown(250);

        if (!body.is(":visible")) {
            body
                .stop(true, true)
                .slideDown(250);

            $("#existingClientToggleIcon")
                .removeClass("fa-chevron-down")
                .addClass("fa-chevron-up");
        }



    });
}
function LoadExistingClient(data) {
    try {

        // ==========================================
        // Client Information
        // ==========================================

        $('#client_refno').val(data.client_refno ?? '');
        $('#client_name').val(data.client_name ?? '');
        $('#website').val(data.website ?? '');
        $('#billing_address').val(data.billing_address ?? '');
        $('#tax_registration_no').val(data.tax_registration_no ?? '');


        // ==========================================
        // Client Name - Other Tabs
        // ==========================================

        $("#con_clientname").val($('#client_name').val());
        $("#invTab_con_clientname").val($('#client_name').val());
        $("#paym_con_clientname").val($('#client_name').val());
        $("#ar_con_clientname").val($('#client_name').val());
        $("#renew_con_clientname").val($('#client_name').val());


        // ==========================================
        // Contacts
        // ==========================================

        $('#primary_contact_name').val(data.primary_contact_name ?? '');
        $('#primary_contact_email').val(data.primary_contact_email ?? '');
        $('#primary_contact_phone').val(data.primary_contact_phone ?? '');

        $('#txtAccOwner').val(data.account_owner ?? '');


        // ==========================================
        // Business Information
        // ==========================================

        $('#high_value_client')
            .prop('checked', data.high_value_client === true);


        // ==========================================
        // Priority
        // ==========================================

        $('input[name="priority_level"][value="' + data.priority_level + '"]')
            .prop('checked', true);


        // ==========================================
        // Dropdowns
        // ==========================================

        $('#ddlIndustry')
            .val(data.industry_id)
            .trigger('change');

        $('#ddlCompanySize')
            .val(data.company_size_id)
            .trigger('change');

        $('#ddlCountry')
            .val(data.country_id)
            .trigger('change');

        $('#ddlContractType')
            .val(data.contract_type_id)
            .trigger('change');

        $('#ddlsupOwner')
            .val(data.support_owner_id)
            .trigger('change');


        waitForDropdownsAndSet(data);


        // ==========================================
        // Dates
        // ==========================================

        if (data.onboarding_start_date)
            $('#onboarding_start_date')
                .val(data.onboarding_start_date.split('T')[0]);

        if (data.onboarding_completion_date)
            $('#onboarding_completion_date')
                .val(data.onboarding_completion_date.split('T')[0]);

        if (data.billing_start_date)
            $('#billing_start_date')
                .val(data.billing_start_date.split('T')[0]);

        if (data.contract_start_date)
            $('#contract_start_date')
                .val(data.contract_start_date.split('T')[0]);

        if (data.contract_end_date)
            $('#contract_end_date')
                .val(data.contract_end_date.split('T')[0]);


        // ==========================================
        // Existing NDA
        // ==========================================
        if (data.nda_upload) {
            $("#nda_existing_file").html(
                '<a href="/uploadedfiles/ClientDocs/' +
                data.nda_upload +
                '" target="_blank">' +
                data.nda_upload +
                '</a>'
            );

            $("#hdnNDAFile").val(data.nda_upload);

        }
        else {
            $("#nda_existing_file").html("");
            $("#hdnNDAFile").val("");

        }

        // ==========================================
        // Existing Tax Verification
        // ==========================================
        if (data.tax_verify_upload) {
            $("#tax_existing_file").html(
                '<a href="/uploadedfiles/ClientDocs/' +
                data.tax_verify_upload +
                '" target="_blank">' +
                data.tax_verify_upload +
                '</a>'
            );

            $("#hdnTaxFile").val(data.tax_verify_upload);

        }
        else {

            $("#tax_existing_file").html("");
            $("#hdnTaxFile").val("");

        }


        // ==========================================
        // Hidden Fields
        // ==========================================
        $('#hdnClientId').val(data.id);
        $("#hdnAction").val('Edit');


        // ==========================================
        // Move to next step
        // ==========================================
        EnableStep(1);
        $("#client-onboarding").steps("next");


        // ==========================================
        // Existing Client State
        // ==========================================
        setClientInfoState(true);
        $("#primary_contact_phone").prop("disabled", true);
        $("#client_name").focus();

    }
    catch (ex) {
        ////console.error(ex);
        Swal.fire({
            icon: 'error',
            title: 'Error',
            text: 'Unable to load client information.'
        });

    }
}

$(document).on("click", ".existing-client-row", function () {
    var index = $(this).data("client-index");

    var clients = $("#existingClientGrid").data("clients");

    if (!clients || !clients[index]) {
        return;
    }

    var selectedClient = clients[index];

    //// Clear search box
    //$("#clientSearchValue").val("");

    // Hide Existing Client Results smoothly
    $("#existingClientResults")
        .stop(true, true)
        .slideUp(250, function () {
            $(this).addClass("d-none");
        });


    /*alert(selectedClient);*/

    // Load client
    LoadExistingClient(selectedClient);

});

$("#existingClientHeader").click(function () {

    var body = $("#existingClientBody");
    var icon = $("#existingClientToggleIcon");

    if (body.is(":visible")) {

        body.stop(true, true).slideUp(250, function () {

            icon
                .removeClass("fa-chevron-up")
                .addClass("fa-chevron-down");

        });

    }
    else {

        body.stop(true, true).slideDown(250, function () {

            icon
                .removeClass("fa-chevron-down")
                .addClass("fa-chevron-up");

        });

    }

});

// +++++++ ********************* end *****************************************

function waitForDropdownsAndSet(data) {
    var attempts = 0;
    var maxAttempts = 30;   // 30 x 100ms = 3 second max wait

    var interval = setInterval(function () {
        attempts++;

        var industryReady = $('#ddlIndustry option').length > 1;
        var companyReady = $('#ddlCompanySize option').length > 1;
        var countryReady = $('#ddlCountry option').length > 1;
        var supOwnerReady = $('#ddlsupOwner option').length > 1;

        if ((industryReady && companyReady && countryReady && supOwnerReady) || attempts >= maxAttempts) {
            clearInterval(interval);

            $('#ddlIndustry').val(String(data.industry_id)).trigger('change');
            $('#ddlCompanySize').val(String(data.company_size_id)).trigger('change');
            $('#ddlCountry').val(String(data.country_id)).trigger('change');
            $('#ddlsupOwner').val(String(data.support_owner_id)).trigger('change');
        }
    }, 100);
}
function GetLatestContractByClient(clientId) {
    new APICALL(
        GetGlobalURL('Base', 'GetLatestContractByClient') + '?clientId=' + clientId,
        'GET',
        '',
        false
    ).FETCH((result, error) => {
        if (error) {
            Swal.fire({
                icon: 'error',
                text: error.data.responseText
            });
            return;
        }

        if (result.data.Table && result.data.Table.length > 0) {
            var contractId = result.data.Table[0].contract_id;
            $("#hdnContractId").val(contractId);

            FillClientContractForEdit(clientId, contractId);
            FillInvoiceForEditManual(clientId, contractId);
            FillPaymentForEdit(clientId, contractId);

            // Contract Type dropdowns disable
            $("#ddlbasicContractType").prop("disabled", true);
            $("#invTab_contracttype").prop("disabled", true);
            $("#paym_contracttype").prop("disabled", true);


            if ($("#hdnPaymentId").val() == '' || parseInt($("#hdnPaymentId").val()) == 0) {
                $("#paymentGrid").show();
            } else {
                $("#paymentGrid").hide();
            }

            EnableStep(1);
            $("#client-onboarding").steps("next");

        }
        else {
            // ============================
            // NAYA: Koi Contract nahi mila — New Contract banane ke liye khula chhodo
            // ============================
            $("#hdnContractId").val(0);

            // Contract type dropdown editable rahe (naya contract banega)
            $("#ddlbasicContractType").prop("disabled", false);
            $("#invTab_contracttype").prop("disabled", false);
            $("#paym_contracttype").prop("disabled", false);

            // Contract Ref No prefill karo (naya reference number)
            LoadMaxContractID();

            // Contract fields ko clean rakho (agar pehle koi purana data reh gaya ho)
            $("#contract_value, #discount, #tax, #total_saas_amount").val("");
            $("#projectname, #projectValue, #m_discountper, #m_taxper, #final_amount, #noMilestone").val("");
            $("#tblMilestone tbody").empty();
            $("#saas").addClass("d-none");
            $("#custom").addClass("d-none");
            $("#docSection").addClass("d-none");
            $("#ddlbasicContractType").val("0").trigger("change");

            EnableStep(1);
            $("#client-onboarding").steps("next");
        }


    });

}
function SetDefaultInvoiceDates() {
    var today = new Date();

    // Invoice Date
    var invoiceDate = new Date(today);

    // Due Date = Today + 15 Days
    var dueDate = new Date(today);
    dueDate.setDate(dueDate.getDate() + 15);

    // Format YYYY-MM-DD
    var formatDate = function (date) {
        var year = date.getFullYear();
        var month = String(date.getMonth() + 1).padStart(2, '0');
        var day = String(date.getDate()).padStart(2, '0');
        return year + '-' + month + '-' + day;
    };

    $("#txtInvoiceDate").val(formatDate(invoiceDate));
    $("#txtInvoiceDueDate").val(formatDate(dueDate));

    //// ============================
    //// Default Manual Invoice
    //// ============================
    //$("#rdoManualInvoice")
    //    .prop("checked", true)
    //    .trigger("change");      // agar change event hai to fire hoga

    //// Dono radio disable
    //$("input[name='inv_method']").prop("disabled", true);

}
function SetDefaultInvoiceAmounts() {
    var contractType = $("#ddlbasicContractType").val();

    if (contractType == "1") {
        $("#milestoneSelectWrapper").addClass("d-none");
        $("#invDiscountPercent").prop("disabled", false);
        $("#invTaxPercent").prop("disabled", false);

        var contractValue = parseFloat($("#contract_value").val()) || 0;
        var noOfPeriods = GetNoOfBillingPeriods();
        var perPeriodGross = contractValue / noOfPeriods;

        $("#invGrossAmount").val(perPeriodGross.toFixed(2));   // ✅ ab per-period amount
        $("#invDiscountPercent").val($("#discount").val() || 0);
        $("#invTaxPercent").val($("#tax").val() || 0);

        //$("#invGrossAmount").val($("#contract_value").val() || 0);
        //$("#invDiscountPercent").val($("#discount").val() || 0);
        //$("#invTaxPercent").val($("#tax").val() || 0);
    }
    else {
        $("#milestoneSelectWrapper").removeClass("d-none");
        $("#invDiscountPercent").prop("disabled", true);
        $("#invTaxPercent").prop("disabled", true);

        PopulateMilestoneDropdown();
        $("#invDiscountPercent").val(0);
        $("#invTaxPercent").val(0);
    }

    calculateInvoiceAmount();
}
function GetNoOfBillingPeriods() {
    var startDate = new Date($("#bill_start_date").val());
    var endDate = new Date($("#bill_end_date").val());
    var freqId = parseInt($("#ddlBillingFreq").val()) || 1;

    if (isNaN(startDate) || isNaN(endDate) || endDate <= startDate)
        return 1;

    // Total months between start and end (inclusive)
    var totalMonths = (endDate.getFullYear() - startDate.getFullYear()) * 12
        + (endDate.getMonth() - startDate.getMonth()) + 1;

    //id	frequencyname
    //1	Monthly
    //2	Quarterly
    //3	Semi - Annual
    //4	Annual

    var intervalMonths = 1;
    switch (freqId) {
        case 1: intervalMonths = 1; break;    // Monthly
        case 2: intervalMonths = 3; break;    // Quarterly
        case 3: intervalMonths = 6; break;    // Semi Annual
        case 4: intervalMonths = 12; break;   // Annual
    }

    //var periods = Math.round(totalMonths / intervalMonths);

    // 🔴 PEHLE: var periods = Math.round(totalMonths / intervalMonths);
    // ✅ AB: SP jaisa integer-division (truncate/floor) — dono consistent rahenge
    var periods = Math.floor(totalMonths / intervalMonths);

    return periods > 0 ? periods : 1;
}
function DecideInvoiceView(clientId, contractId) {
    new APICALL(GetGlobalURL('Base', 'GetAllInvoicesGrid') + '?client_id=' + clientId + "&contract_id=" + contractId, 'GET', '', true)
        .FETCH((result, error) => {
            if (error) return;

            var hasInvoices = result.data && result.data.length > 0;

            if (hasInvoices) {
                // Existing invoices hain — Grid dikhao
                $("#manualGrid").addClass("d-none");
                $("#autoGrid").removeClass("d-none");
                GetInvoiceGrid(clientId, contractId);
            }
            else {
                // Koi invoice nahi — seedha Form kholo, Contract se prefill karo
                OpenNewInvoiceForm();

                // always grid show ...
                $("#manualGrid").addClass("d-none");
                $("#autoGrid").removeClass("d-none");

                //////hide add new button
                ////$("#btnAddNewInvoice").addClass("d-none");

            }

            if ($("#ddlbasicContractType").val() == "1") {
                //show add new button on SaaS Contract
                $("#btnAddNewInvoice").removeClass("d-none");

            } else { //Custom Milestone
                //hide add new button on Custom Contract
                $("#btnAddNewInvoice").addClass("d-none");
            }


        });
}
function OpenNewInvoiceForm() {
    $("#hdnInvoiceId").val(0);
    $("#hdnInvoiceEditMode").val("0");
    $("#autoGrid").addClass("d-none");
    $("#manualGrid").removeClass("d-none");
    $("#btnBackInvoiceGrid").hide();   // koi grid nahi hai jahan wapis jaana ho

    LoadMaxInvoiceID();   // yeh iske baad call ho
    SetDefaultInvoiceAmounts();   // Contract se Gross/Disc/Tax prefill
}

// ++ for Edit from Grid Client , Contract & Contract Milestones
function FillClientContractForEdit(clientId, contractId) {
    new APICALL(GetGlobalURL('Base', 'FillClientContractForEdit')
        + '?clientId=' + clientId
        + '&contractId=' + contractId,
        'GET',
        '',
        false
    ).FETCH((result, error) => {
        if (error) {
            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: error.data.responseText
            });
            return;
        }

        if (result && result.data) {
            var data = result.data;

            var client = (data.Table && data.Table.length > 0) ? data.Table[0] : null;
            var contract = (data.Table1 && data.Table1.length > 0) ? data.Table1[0] : null;
            var milestones = (data.Table2 && data.Table2.length > 0) ? data.Table2 : [];

            //-----------------------
            // Hidden IDs
            //-----------------------
            $("#hdnClientId").val(client ? client.id : 0);
            $("#hdnContractId").val(contract ? contract.id : 0);
            $("#hdnAction").val("Edit");


            //-----------------------
            // STEP-1 Client
            //-----------------------
            $("#client_refno").val(client?.client_refno || "");

            $("#client_name").val(client.client_name);
            $("#con_clientname").val(client.client_name);

            $("#website").val(client.website);
            $("#billing_address").val(client.billing_address);
            $("#tax_registration_no").val(client.tax_registration_no);

            $("#primary_contact_name").val(client.primary_contact_name);
            $("#primary_contact_email").val(client.primary_contact_email);
            $("#primary_contact_phone").val(client.primary_contact_phone);

            $("#txtAccOwner").val(client.account_owner);

            $("input[name='priority_level'][value='" + client.priority_level + "']")
                .prop("checked", true);


            // Dropdowns
            $('#ddlIndustry').val(client.industry_id).trigger('change');
            $('#ddlCompanySize').val(client.company_size_id).trigger('change');
            $('#ddlCountry').val(client.country_id).trigger('change');
            $('#ddlsupOwner').val(client.support_owner_id).trigger('change');

            //-----------------------
            // Dates
            //-----------------------
            if (client.onboarding_start_date)
                $("#onboarding_start_date")
                    .val(client.onboarding_start_date.split('T')[0]);


            //-----------------------
            // Existing Files (Client Docs)
            //-----------------------
            if (client.nda_upload) {
                $("#nda_existing_file").html(
                    '<a href="/uploadedfiles/ClientDocs/' + client.nda_upload +
                    '" target="_blank">' + client.nda_upload + '</a>'
                );

                $("#hdnNDAFile").val(client.nda_upload);
            } else {
                $("#nda_existing_file").html("");
                $("#hdnNDAFile").val("");
            }

            if (client.tax_verify_upload) {
                $("#tax_existing_file").html(
                    '<a href="/uploadedfiles/ClientDocs/' + client.tax_verify_upload +
                    '" target="_blank">' + client.tax_verify_upload + '</a>'
                );

                $("#hdnTaxFile").val(client.tax_verify_upload);
            } else {
                $("#tax_existing_file").html("");
                $("#hdnTaxFile").val("");
            }


            //-----------------------
            // STEP-2 Contract
            //-----------------------
            if ($("#hdnContractId").val() > 0) {
                $("#contract_no").val(contract.contract_refno);
                $("#invTab_contract_no").val(contract.contract_refno);
                $("#con_clientname").val(client.client_name);
                $("#invTab_con_clientname").val(client.client_name);
                $("#paym_contract_no").val(contract.contract_refno);
                $("#paym_con_clientname").val(client.client_name);
                $("#ar_con_clientname").val(client.client_name);
                $("#ar_contract_no").val(contract.contract_refno);
                $("#renew_contract_no").val(contract.contract_refno);
                $("#renew_con_clientname").val(client.client_name);

                $("#bill_start_date").val(contract.bill_start_date?.split('T')[0]);
                $("#bill_end_date").val(contract.bill_end_date?.split('T')[0]);

                $("#contract_value").val(contract.contract_value);
                $("#discount").val(contract.discount_percent);
                $("#tax").val(contract.tax_percent);
                $("#total_saas_amount").val(FormatAmount(contract.total_saas_amount));

                $("#projectname").val(contract.project_name);

                $("#projectValue").val(contract.project_value);
                $("#m_discountper").val(contract.m_discount);
                $("#m_taxper").val(contract.m_tax_percent);
                //final_amount

                //$("#final_amount").val(contract.total_milestone_amount);
                $("#final_amount").val(FormatAmount(contract.total_milestone_amount));


                $("#noMilestone").val(contract.no_of_milestones);

                $("#remarks").val(contract.penalty_terms);
                $("#supphours").val(contract.support_hours);

                $("#auto_renew").prop("checked",
                    contract.auto_renew == true ||
                    contract.auto_renew == 1 ||
                    contract.auto_renew == "1"
                );

                setTimeout(function () {
                    $("#ddlbasicContractType").val(contract.basic_contract_type).trigger("change");
                    $("#ddlBillingFreq").val(contract.bill_freq_id).trigger("change");
                    $("#ddlBillingType").val(contract.bill_type_id).trigger("change");
                    $("#ddlCurrency").val(contract.currency_id).trigger("change");
                    $("#ddlBillingCycle").val(contract.billing_cycle).trigger("change");
                    $("#ddlPaymentTerms").val(contract.payment_term_id).trigger("change");

                    //invoice Tab contracttype
                    $("#invTab_contracttype").val(contract.basic_contract_type).trigger("change");
                    $("#paym_contracttype").val(contract.basic_contract_type).trigger("change");
                    $("#ar_contracttype").val(contract.basic_contract_type).trigger("change");
                    $("#renew_contracttype").val(contract.basic_contract_type).trigger("change");
                }, 200);

                //-----------------------
                // Existing Files (Contract Docs)
                //-----------------------
                // Contract
                if (contract.doc1_upload) {
                    $("#doc1_existing_file").html(
                        '<a href="/uploadedfiles/ContractDocs/' + contract.doc1_upload +
                        '" target="_blank">' + contract.doc1_upload + '</a>'
                    );

                    $("#hdndoc1File").val(contract.doc1_upload);
                } else {
                    $("#doc1_existing_file").html("");
                    $("#hdndoc1File").val("");
                }

                // Proposal
                if (contract.doc2_upload) {
                    $("#doc2_existing_file").html(
                        '<a href="/uploadedfiles/ContractDocs/' + contract.doc2_upload +
                        '" target="_blank">' + contract.doc2_upload + '</a>'
                    );

                    $("#hdndoc2File").val(contract.doc2_upload);
                } else {
                    $("#doc2_existing_file").html("");
                    $("#hdndoc2File").val("");
                }

                // Scope
                if (contract.doc3_upload) {
                    $("#doc3_existing_file").html(
                        '<a href="/uploadedfiles/ContractDocs/' + contract.doc3_upload +
                        '" target="_blank">' + contract.doc3_upload + '</a>'
                    );

                    $("#hdndoc3File").val(contract.doc3_upload);
                } else {
                    $("#doc3_existing_file").html("");
                    $("#hdndoc3File").val("");
                }

                // SLA
                if (contract.doc4_upload) {
                    $("#doc4_existing_file").html(
                        '<a href="/uploadedfiles/ContractDocs/' + contract.doc4_upload +
                        '" target="_blank">' + contract.doc4_upload + '</a>'
                    );

                    $("#hdndoc4File").val(contract.doc4_upload);
                } else {
                    $("#doc4_existing_file").html("");
                    $("#hdndoc4File").val("");
                }

                //-----------------------
                // Milestones
                //-----------------------
                if (milestones != null)
                    BindMilestones(milestones);

                EnableCompletedSteps();

            } 
        }

    });

}
function BindMilestones(data) {
    $("#tblMilestone tbody").empty();

    var grandTotal = 0;
    $.each(data, function (i, item) {
        var amount = Number(item.milestone_amount || 0);
        grandTotal += amount;

        //var dueDate = "";
        //if (item.due_date)
        //    dueDate = item.due_date.split('T')[0];

        //$("#tblMilestone tbody").append(`
        //    <tr>
        //        <td>
        //            ${item.milestone_name}
        //        </td>
        //        <td>
        //            <input type="number"
        //                   class="text-end form-control form-control-sm milestoneAmount"
        //                   value="${amount.toFixed(2)}">
        //        </td>
        //        <td>
        //            <input type="date"
        //                   class="form-control form-control-sm milestoneDate"
        //                   value="${dueDate}">
        //        </td>
        //    </tr>
        //`);

        var dueDate = item.due_date ? item.due_date.split('T')[0] : "";
        var milestoneNo = item.milestone_no || (i + 1);
        var status = item.status || "Pending";

        $("#tblMilestone tbody").append(`
            <tr data-milestone-no="${milestoneNo}" data-status="${status}">
                <td>${item.milestone_name}</td>
                <td>
                    <input type="number" class="text-end form-control form-control-sm milestoneAmount" value="${amount.toFixed(2)}">
                </td>
                <td>
                    <input type="date" class="form-control form-control-sm milestoneDate" value="${dueDate}">
                </td>
            </tr>
        `);

    });

    $("#txtGrandTotal").val(grandTotal.toFixed(2));

    // Agar Contract Edit Mode hai to naye controls bhi disable kar do
    if ($("#hdnContractEditMode").val() == "1") {
        $("#tblMilestone")
            .find("input, select, textarea, button")
            .prop("disabled", true)
            .prop("readonly", true);
    }

}
function PopulateMilestoneDropdown() {
    var $ddl = $("#ddlInvoiceMilestone");
    $ddl.empty();

    $("#tblMilestone tbody tr").each(function () {
        var status = $(this).data("status");
        var milestoneNo = $(this).data("milestone-no");
        var name = $(this).find("td:eq(0)").text().trim();
        var amount = $(this).find(".milestoneAmount").val();
        var dueDate = $(this).find(".milestoneDate").val();

        if (status == "Pending") {
            $ddl.append(`<option value="${milestoneNo}" data-amount="${amount}" data-due="${dueDate}">${name}</option>`);
        }
    });

    $ddl.trigger("change");
}

$(document).on("change", "#ddlInvoiceMilestone", function () {
    var selected = $(this).find("option:selected");
    $("#invGrossAmount").val(selected.data("amount") || 0);
    $("#hdnInvoiceMilestoneNo").val($(this).val());
    $("#hdnInvoiceBillingPeriod").val(selected.data("due") || "");

    // Milestone ke liye hamesha 0 aur disabled rakho
    $("#invDiscountPercent").prop("disabled", true).val(0);
    $("#invTaxPercent").prop("disabled", true).val(0);

    calculateInvoiceAmount();
});

function FillInvoiceForEditManual(clientId, contractId) {
    new APICALL(GetGlobalURL('Base', 'FillInvoiceForEditManual') + '?clientId=' + clientId + '&contractId=' + contractId, 'GET', '', false)
        .FETCH((result, error) => {

            if (error) {
                Swal.fire({
                    icon: 'error',
                    title: 'Error',
                    text: error.data.responseText
                });
                return;
            }

            // yahan data fill hoga
            var invoices = result.data.Table[0] ?? [];

            //-------------------------
            // Header Invoice Tab Manual
            //-------------------------
            if (invoices != null) {
                $("#hdnInvoiceId").val(invoices.invoice_id);
                $("#txtInvoiceNo").val(invoices.invoice_no);

                //-----------------------
                // Dates
                //-----------------------
                if (invoices.invoice_date)
                    $("#txtInvoiceDate")
                        .val(invoices.invoice_date.split('T')[0]);

                if (invoices.due_date)
                    $("#txtInvoiceDueDate")
                        .val(invoices.due_date.split('T')[0]);

            }
        });

}
function FillPaymentForEdit(clientId, contractId) {
    new APICALL(GetGlobalURL('Base', 'FillPaymentForEdit') + '?clientId=' + clientId + '&contractId=' + contractId, 'GET', '', false)
        .FETCH((result, error) => {

            if (error) {
                Swal.fire({
                    icon: 'error',
                    title: 'Error',
                    text: error.data.responseText
                });
                return;
            }

            // yahan data fill hoga
            var invoices = result.data.Table ?? [];
            var invoiceSingle = result.data.Table?.[0] ?? null;

            var payment = result.data.Table1?.[0] ?? null;
            var paymentDetail = result.data.Table2 ?? [];


            ////-------------------------
            //// Header Invoice Tab Manual
            ////-------------------------
            //if (invoices != null && invoices.length > 0) {
            //    $("#hdnInvoiceId").val(invoiceSingle.invoice_id);
            //    $("#txtInvoiceNo").val(invoiceSingle.invoice_no);

            //    //-----------------------
            //    // Dates
            //    //-----------------------
            //    if (invoiceSingle.invoice_date)
            //        $("#txtInvoiceDate")
            //            .val(invoiceSingle.invoice_date.split('T')[0]);

            //    if (invoiceSingle.due_date)
            //        $("#txtInvoiceDueDate")
            //            .val(invoiceSingle.due_date.split('T')[0]);

            //}

            //-------------------------
            // Header Payment Tab
            //-------------------------
            if (payment != null) {

                $("#hdnPaymentId").val(payment.payment_id);

                $("#amount_received").val(payment.amount_received);

                $("input[name='payment_mode'][value='" + payment.payment_mode + "']")
                    .prop("checked", true);

                $("#txtReferenceNo").val(payment.ref_no);
                $("#txtBankName").val(payment.bank_name);

                if (payment.value_date)
                    $("#txtReferenceDate").val(payment.value_date.split('T')[0]);

                $("#hdnpaymentslip").val(payment.receipt_upload);

                if (payment.receipt_upload) {

                    $("#payment_slip_existing_file").html(
                        '<a href="/uploadedfiles/PaymentDocs/' +
                        payment.receipt_upload +
                        '" target="_blank">' +
                        payment.receipt_upload +
                        '</a>'
                    );

                }
                else {
                    $("#payment_slip_existing_file").html("");

                }


            }

            //-------------------------
            // Selected Invoice List
            //-------------------------
            var selectedInvoices = [];

            $.each(paymentDetail, function (i, row) {
                selectedInvoices.push(parseInt(row.invoice_id));
            });

            //-------------------------
            // DataTable Destroy
            //-------------------------
            if ($.fn.DataTable.isDataTable("#tblPaymentGrid")) {
                $("#tblPaymentGrid").DataTable().clear().destroy();
            }


            $("#tblPaymentDtl").empty();
            //-------------------------
            // Grid
            //-------------------------
            $.each(invoices, function (i, item) {

                var checked = "";
                var disabled = "";

                if (selectedInvoices.includes(parseInt(item.invoice_id))) {
                    checked = "checked";
                }
                else if (item.payment_status == 1) {
                    disabled = "disabled";
                }

                $("#tblPaymentDtl").append(
                    '<tr>' +
                    '<td class="text-center">' +
                    '<input type="checkbox" class="chkInvoice"' +
                    ' value="' + item.invoice_id + '"' +
                    ' data-amount="' + item.total_amount + '"' +
                    ' ' + checked +
                    ' ' + disabled + '>' +
                    '</td>' +
                    '<td>' + (item.invoice_no || '') + '</td>' +
                    '<td>' + FormatDate(item.invoice_date) + '</td>' +
                    '<td>' + FormatDate(item.due_date) + '</td>' +
                    '<td>' + (item.contract_refno || '') + '</td>' +
                    '<td>' + (item.milestone_name || '-') + '</td>' +
                    '<td class="text-end">' +
                    parseFloat(item.total_amount || 0).toLocaleString() +
                    '</td>' +
                    '</tr>'
                );

            });

            $("#tblPaymentGrid").DataTable({
                destroy: true,
                autoWidth: false,
                responsive: true,
                language: {
                    emptyTable: "No Invoice Found"
                }
            });

            InitPaymentGridEvents();
    });

}

// +++++++ validate START here +++++++++
//function ValidateSteps(step) {
//    // Client Tab
//    if (step == 0) {
//        if (!ValidateClient()) {
//            $("#client-onboarding").steps("setStep", 0);   // Client Tab
//            return false;
//        }
//        EnableStep(1);
//    }

//    // Contract Tab
//    else if (step == 1) {
//        if (!ValidateClient()) {
//            $("#client-onboarding").steps("setStep", 0);   // Client Tab
//            return false;
//        }

//        if (!ValidateContract()) {
//            $("#client-onboarding").steps("setStep", 1);   // Contract Tab
//            return false;
//        }

//        EnableStep(2);
//        EnableStep(3);

//    }

//    // Payment/Invoice Tab
//    else if (step == 2) {
//        if (!ValidateInvoice()) {
//            $("#client-onboarding").steps("setStep", 2);   // Invoice Tab
//            return false;
//        }

//        EnableStep(3);
//        return true;
//    }

//    // Payment Tab
//    else if (step == 3) {
//        if (!ValidatePayment()) {
//            $("#client-onboarding").steps("setStep", 3);
//            return false;
//        }

//        EnableStep(4);

//    }

//    // ARException
//    else if (step == 4) {
//        if (!ValidateARException()) {
//            $("#client-onboarding").steps("setStep", 4);
//            return false;
//        }

//        EnableStep(5);
//    }

//    // Renewal Last Tab
//    else if (step == 5) {
//        if (!ValidateRenewal()) {
//            $("#client-onboarding").steps("setStep", 5);
//            return false;
//        }

//        EnableStep(6);

//    }

//    // if cursor is read this then its means OK...
//    return true;

//}

function ValidateSteps(step) {
    // Client Tab
    if (step == 0) {
        if (!ValidateClient()) {
            return false;
        }
        EnableStep(1);
    }
    // Contract Tab
    else if (step == 1) {
        if (!ValidateClient()) {
            return false;
        }
        if (!ValidateContract()) {
            return false;
        }
        EnableStep(2);
        EnableStep(3);
    }
    // Invoice Tab
    else if (step == 2) {
        if (!ValidateInvoice()) {
            return false;
        }
        EnableStep(3);
        return true;
    }
    // Payment Tab
    else if (step == 3) {
        if (!ValidatePayment()) {
            return false;
        }
        EnableStep(4);
    }
    // AR Exception
    else if (step == 4) {
        if (!ValidateARException()) {
            return false;
        }
        EnableStep(5);
    }
    // Renewal Last Tab
    else if (step == 5) {
        if (!ValidateRenewal()) {
            return false;
        }
        EnableStep(6);
    }

    // Agar yahan tak aaya, matlab sab OK hai
    return true;
}
function ValidateClient() {
    $(".is-invalid").removeClass("is-invalid");

    // Client Information
    if (!$("#client_name").val().trim()) {
        setInvalid("#client_name", "Please enter Client Name");
        return false;
    }

    if ($("#ddlIndustry").val() == '0') {
        setInvalid("#ddlIndustry", "Please select Industry");
        return false;
    }

    if ($("#ddlCompanySize").val() == '0') {
        setInvalid("#ddlCompanySize", "Please select Company Size");
        return false;
    }

    var website = $("#website").val().trim();
    if (!website) {
        setInvalid("#website", "Please enter Website");
        return false;
    }

    if (!isValidWebsite(website)) {
        setInvalid("#website", "Website format should be like https://www.abc.com");
        return false;
    }

    ////var ntn = $("#tax_registration_no").val().replace(/\D/g, "");
    var ntn = $("#tax_registration_no").val();
    if (!ntn) {
        setInvalid("#tax_registration_no", "Please enter Tax Registration No");
        return false;
    }

    ////if (!isValidNTN(ntn)) {
    ////    setInvalid("#tax_registration_no", "NTN format should be 1234567-8");
    ////    return false;
    ////}

    if ($("#ddlCountry").val() == '0') {
        setInvalid("#ddlCountry", "Please select Country");
        return false;
    }

    if (!$("#billing_address").val().trim()) {
        setInvalid("#billing_address", "Please enter Billing Address");
        return false;
    }

    // Contact Information
    if (!$("#primary_contact_name").val().trim()) {
        setInvalid("#primary_contact_name", "Please enter Primary Contact Name");
        return false;
    }

    if (!$("#primary_contact_email").val().trim()) {
        setInvalid("#primary_contact_email", "Please enter Email");
        return false;
    }

    // Email Format
    var email = $("#primary_contact_email").val().trim();
    if (!email) {
        setInvalid("#primary_contact_email", "Please enter Email");
        return false;
    }

    if (!isValidEmail(email)) {
        setInvalid("#primary_contact_email", "Please enter a valid Email");
        return false;
    }

    // Mobile Format
    var mobile = $("#primary_contact_phone").val().replace(/\D/g, "");
    if (!mobile) {
        setInvalid("#primary_contact_phone", "Please enter Mobile Number");
        return false;
    }

    if (!isValidMobile(mobile)) {
        setInvalid("#primary_contact_phone", "Mobile format should be 923001234567");
        return false;
    }

    // Internal Details
    if (!$("#txtAccOwner").val().trim()) {
        setInvalid("#txtAccOwner", "Please enter Account Owner");
        return false;
    }

    if ($("#ddlsupOwner").val() == '0') {
        setInvalid("#ddlsupOwner", "Please select Support Owner");
        return false;
    }

    if (!$("#onboarding_start_date").val()) {
        setInvalid("#onboarding_start_date", "Please select Onboarding Date");
        return false;
    }

    // Required Documents (Only Add Mode)
    if ($("#hdnClientId").val() == "0") {

        if ($("#nda_upload")[0].files.length == 0) {
            Swal.fire("Validation", "Please upload NDA Document", "warning");
            return false;
        }

        if ($("#tax_verify_upload")[0].files.length == 0) {
            Swal.fire("Validation", "Please upload Tax Verification Document", "warning");
            return false;
        }
    }

    if (!$("input[name='priority_level']:checked").length) {
        Swal.fire("Validation", "Please select Priority Level", "warning");
        return false;
    }

    return true;
}
function ValidateContract() {
    $(".is-invalid").removeClass("is-invalid");

    // Common Validation
    if ($("#ddlbasicContractType").val() == '0') {
        setInvalid("#ddlbasicContractType", "Please select Contract Type");
        return false;
    }

    var contractType = parseInt($("#ddlbasicContractType").val());
    var contractTypeText = $("#ddlbasicContractType option:selected")
        .text()
        .trim()
        .toLowerCase();

    //=========================================
    // SaaS Contract
    //=========================================
    if (contractType == 1) {

        if ($("#ddlBillingFreq").val() == '0') {
            setInvalid("#ddlBillingFreq", "Please select Billing Frequency");
            return false;
        }

        if (!$("#bill_start_date").val()) {
            setInvalid("#bill_start_date", "Please select Contract Start Date");
            return false;
        }

        if (!$("#bill_end_date").val()) {
            setInvalid("#bill_end_date", "Please select Contract End Date");
            return false;
        }

        // Start Date should be less than End Date
        var startDate = new Date($("#bill_start_date").val());
        var endDate = new Date($("#bill_end_date").val());

        if (startDate >= endDate) {
            setInvalid("#bill_end_date", "Contract End Date should be greater than Contract Start Date");
            return false;
        }



        if ($("#ddlBillingCycle").val() == '0') {
            setInvalid("#ddlBillingCycle", "Please select Billing Cycle");
            return false;
        }

        if ($("#ddlCurrency").val() == '0') {
            setInvalid("#ddlCurrency", "Please select Currency");
            return false;
        }

        if (!$("#contract_value").val().trim()) {
            setInvalid("#contract_value", "Please enter Contract Amount");
            return false;
        }

        if (parseFloat($("#contract_value").val()) <= 0) {
            setInvalid("#contract_value", "Contract Amount should be greater than zero");
            return false;
        }

        // ==========================
        // Discount % Validation
        // ==========================
        var discountPercent = ($("#discount").val() || "").trim();

        if (discountPercent !== "") {

            var discount = parseFloat(discountPercent);

            if (isNaN(discount) || discount < 0 || discount > 100) {
                setInvalid("#discount", "Discount % should be between 0 and 100");
                return false;
            }
        }

        // ==========================
        // Tax % Validation
        // ==========================
        var taxPercent = ($("#tax").val() || "").trim();

        if (taxPercent !== "") {

            var tax = parseFloat(taxPercent);

            if (isNaN(tax) || tax < 0 || tax > 100) {
                setInvalid("#tax", "Tax % should be between 0 and 100");
                return false;
            }
        }


    }

    //=========================================
    // Custom Project
    //=========================================
    else if (contractType == 2) {

        if ($("#projectname").val() == '') {
            setInvalid("#projectname", "Please enter Project Name");
            return false;
        }

        if ($("#ddlPaymentTerms").val() == '0') {
            setInvalid("#ddlPaymentTerms", "Please select Payment Terms");
            return false;
        }

        if ($("#projectValue").val() == '') {
            setInvalid("#projectValue", "Please enter Project Value");
            return false;
        }

        if (parseFloat($("#projectValue").val()) <= 0) {
            setInvalid("#projectValue", "Project Value should be greater than zero");
            return false;
        }

        // ==========================
        // Discount % Validation
        // ==========================
        var discountPercent = ($("#m_discountper").val() || "").trim();

        if (discountPercent !== "") {

            var discount = parseFloat(discountPercent);

            if (isNaN(discount) || discount < 0 || discount > 100) {
                setInvalid("#m_discountper", "Discount % should be between 0 and 100");
                return false;
            }
        }

        // ==========================
        // Tax % Validation
        // ==========================
        var taxPercent = ($("#m_taxper").val() || "").trim();

        if (taxPercent !== "") {

            var tax = parseFloat(taxPercent);

            if (isNaN(tax) || tax < 0 || tax > 100) {
                setInvalid("#m_taxper", "Tax % should be between 0 and 100");
                return false;
            }
        }

        //final_amount
        if (parseFloat(UnFormatAmount($("#final_amount").val())) <= 0) {
            setInvalid("#final_amount", "Final Amount should be greater than zero");
            return false;
        }


        if ($("#noMilestone").val() == '') {
            setInvalid("#noMilestone", "Please enter Number of Milestones");
            return false;
        }

        if ($("#tblMilestone tbody tr").length == 0) {
            Swal.fire("Validation", "Please add Milestones", "warning");
            return false;
        }

        var isValid = true;
        var totalMilestoneAmount = 0;
        var previousDueDate = null;

        $("#tblMilestone tbody tr").each(function (index) {
            var name = $(this).find("td:eq(0)").text().trim();
            var amount = ($(this).find(".milestoneAmount").val() || "").trim();
            var dueDate = ($(this).find("input[type='date']").val() || "").trim();

            if (name === "") {
                Swal.fire("Validation", "Please enter milestone name.", "warning");
                isValid = false;
                return false;
            }

            if (amount === "" || parseFloat(amount) <= 0) {
                Swal.fire("Validation", "Please enter Milestone Amount", "warning");
                isValid = false;
                return false;
            }

            if (dueDate === "") {
                Swal.fire("Validation", "Please select Due Date", "warning");
                isValid = false;
                return false;
            }

            // Total Amount
            totalMilestoneAmount += parseFloat(amount);

            // Due Date Sequence Validation
            var currentDueDate = new Date(dueDate);

            if (previousDueDate != null && currentDueDate <= previousDueDate) {
                Swal.fire(
                    "Validation",
                    "Milestone " + (index + 1) + " Due Date should be greater than previous milestone Due Date.",
                    "warning"
                );
                isValid = false;
                return false;
            }

            previousDueDate = currentDueDate;

        });

        if (!isValid)
            return false;

        var projectValue = parseFloat(UnFormatAmount($("#final_amount").val())) || 0;
        var grandTotal = parseFloat(UnFormatAmount($("#txtGrandTotal").val())) || 0;

        // OR
        // var grandTotal = totalMilestoneAmount;

        if (grandTotal != projectValue) {
            Swal.fire(
                "Validation",
                "Project Value and Milestone Grand Total should be equal.",
                "warning"
            );
            return false;
        }

    }

    // Validate only Add New ....
    if ($("#hdnContractId").val() == "0") {
        // ==========================
        // Required Contract Documents
        // ==========================
        if ($("#doc1_upload")[0].files.length == 0) {
            Swal.fire("Validation", "Please upload Signed Contract.", "warning");
            return false;
        }

        //if ($("#doc2_upload")[0].files.length == 0) {
        //    Swal.fire("Validation", "Please upload Approved Proposal.", "warning");
        //    return false;
        //}

        //if ($("#doc3_upload")[0].files.length == 0) {
        //    Swal.fire("Validation", "Please upload Project Scope Document.", "warning");
        //    return false;
        //}

        //if ($("#doc4_upload")[0].files.length == 0) {
        //    Swal.fire("Validation", "Please upload SLA Document.", "warning");
        //    return false;
        //}
    }

    //if (!$("#remarks").val().trim()) {
    //    setInvalid("#remarks", "Please enter Penalty Terms");
    //    return false;
    //}

    //if (!$("#supphours").val().trim()) {
    //    setInvalid("#supphours", "Please enter Support Hours");
    //    return false;
    //}

    // Default Payment Mode = Cash
    if ($("input[name='payment_mode']:checked").length === 0) {
        $("#pm_cash").prop("checked", true).trigger("change");
    }

    return true;
}
function ValidateInvoice() {
    // Sirf Manual Invoice pe validation hogi
    if (!$("#rdoManualInvoice").is(":checked"))
        return true;

    // Invoice Date
    if ($("#txtInvoiceDate").val() == "") {
        Swal.fire({
            icon: "warning",
            title: "Validation",
            text: "Please select Invoice Date."
        });

        $("#txtInvoiceDate").focus();
        return false;
    }

    // Due Date
    if ($("#txtInvoiceDueDate").val() == "") {
        Swal.fire({
            icon: "warning",
            title: "Validation",
            text: "Please select Due Date."
        });

        $("#txtInvoiceDueDate").focus();
        return false;
    }

    // Due Date >= Invoice Date
    var invoiceDate = new Date($("#txtInvoiceDate").val());
    var dueDate = new Date($("#txtInvoiceDueDate").val());

    if (dueDate < invoiceDate) {
        Swal.fire({
            icon: "warning",
            title: "Validation",
            text: "Due Date cannot be earlier than Invoice Date."
        });

        $("#txtInvoiceDueDate").focus();
        return false;
    }

    // Milestone — SIRF Custom contract (type=2) ke liye check karo
    var contractType = $("#ddlbasicContractType").val();
    
    ////$("hdnInvoiceEditMode").val()

    if (contractType == "2" && $("hdnInvoiceEditMode").val() == 0) {
        // Milestone
        if ($("#ddlInvoiceMilestone").val() == "" ||
            $("#ddlInvoiceMilestone").val() == null ||
            $("#ddlInvoiceMilestone").val() == "0") {

            Swal.fire({
                icon: "warning",
                title: "Validation",
                text: "Please select Milestone."
            });

            $("#ddlInvoiceMilestone").focus();
            return false;
        }
    }

    return true;
}
function ValidatePayment() {
    $(".is-invalid").removeClass("is-invalid");

    var params = new URLSearchParams(window.location.search);
    var type = params.get("type");

    if (type == "editfromgrid") {

    } else {
        var invoiceList = GetSelectedInvoices();
        if (invoiceList.length == 0) {
            Swal.fire("Warning", "Please select at least one invoice.", "warning");
            return false;
        } 
    }

    //// Ab dono mode (Add + editfromgrid) mein check ho
    //var invoiceList = GetSelectedInvoices();
    //if (invoiceList.length == 0) {
    //    Swal.fire("Warning", "Please select at least one invoice.", "warning");
    //    return false;
    //}

    if ($("#amount_received").val() == "" || parseInt($("#amount_received").val()) == 0) {
        $("#amount_received").addClass("is-invalid").focus();
        Swal.fire("Warning", "Please enter Amount Received.", "warning");
        return false;
    }


    var paymentMode = $("input[name='payment_mode']:checked").val();
    if (!paymentMode) {
        Swal.fire("Warning", "Please select Payment Mode.", "warning");
        return false;
    }

    // If not Cash then validate banking fields
    if (paymentMode !== "Cash") {

        if ($("#txtReferenceNo").val().trim() == "") {
            $("#txtReferenceNo").addClass("is-invalid").focus();
            Swal.fire("Warning", "Please enter Reference No.", "warning");
            return false;
        }

        if ($("#txtBankName").val().trim() == "") {
            $("#txtBankName").addClass("is-invalid").focus();
            Swal.fire("Warning", "Please enter Bank Name.", "warning");
            return false;
        }

        if ($("#txtReferenceDate").val().trim() == "") {
            $("#txtReferenceDate").addClass("is-invalid").focus();
            Swal.fire("Warning", "Please select Reference Date.", "warning");
            return false;
        }
    }


    //// Required Documents (Only Add Mode)
    //if ($("#hdnPaymentId").val() == "0") {
    //    if ($("#payment_slip_upload")[0].files.length == 0) {
    //        Swal.fire("Validation", "Please upload Receipt Document", "warning");
    //        return false;
    //    }
    //}

    ////var invoiceList = GetSelectedInvoices();
    ////if (invoiceList.length == 0) {
    ////    Swal.fire("Warning", "Please select at least one invoice.", "warning");
    ////    return false;
    ////} 

    return true;
}
function ValidateLastRowInvoice() {
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
function ValidateARException() {
    $(".is-invalid").removeClass("is-invalid");

    //if (!$("#client_name").val().trim()) {
    //    setInvalid("#client_name", "Please enter Client Name");
    //    return false;
    //}

    return true;
}
function ValidateRenewal() {
    var isFormActive = !$("#renewFormView").hasClass("d-none");
    if (!isFormActive) return true;   // form khula hi nahi, skip

    if (!$("#ddlRenewalTerm").val()) {
        Swal.fire({ icon: "warning", title: "Validation", text: "Please select Renewal Term." });
        return false;
    }
    if (!$("#renew_start_date").val() || !$("#renew_end_date").val()) {
        Swal.fire({ icon: "warning", title: "Validation", text: "Please select Start and End Date." });
        return false;
    }
    if (!$("#renew_contract_value").val() || parseFloat($("#renew_contract_value").val()) <= 0) {
        Swal.fire({ icon: "warning", title: "Validation", text: "Please enter Contract Value." });
        return false;
    }
    return true;
}



// ++++ Regex function ++++++++
function isValidNTN(ntn) {
    // NTN Format: 1234567-8
    var regex = /^\d{8}$/;
    return regex.test(ntn);
}
function isValidMobile(mobile) {
    // Pakistan Mobile Format: 923001234567
    var regex = /^92\d{10}$/;
    return regex.test(mobile);
}

function normalizePakMobile(rawValue) {
    let digits = rawValue.replace(/\D/g, "");

    // Agar leading "0" hai (03XXXXXXXXX — 11 digits), to "0" ko "92" se replace karo
    if (digits.length > 0 && digits.charAt(0) === "0") {
        digits = "92" + digits.substring(1);
    }

    // Agar user ne bina "0" ya "92" ke seedha "3XXXXXXXXX" (10 digit) type kar diya
    else if (digits.length > 0 && digits.charAt(0) !== "9" && digits.length <= 10) {
        digits = "92" + digits;
    }

    return digits;
}
function isValidEmail(email) {
    // Email
    var regex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return regex.test(email);
}
function isValidWebsite(url) {
    // Website
    // Accepts:
    // https:///www.abc.com
    // http:///abc.com
    // https:///abc.pk
    var regex = /^(https?:\/\/)?(www\.)?[a-zA-Z0-9-]+(\.[a-zA-Z]{2,})+([\/?#].*)?$/i;
    return regex.test(url);
}

// ---------------------------
// +++++++ validate END here +++++++++


// ******* Save Data START here *********
function SaveData(step, callback, showMessage) {
    SaveClient(step, callback, showMessage);
}
function SaveClient(step, callback, showMessage) {
    var formData = new FormData();
    AppendClient(formData);

    SaveToServer("BR_SaveClientonBoarding", formData, function (result) {
            var clientId = result.data[0].id;
            $("#hdnClientId").val(clientId);

            if (step >= 1) {
                SaveContract(step, callback, showMessage);
            }
            else {
                // NAYA: sirf Client tab save ho raha ho to yahin message dikhao
                if (showMessage) {
                    Swal.fire("Saved", "Saved Successfully", "success");
                }

                if (callback)
                    callback(true);
            }
        },
        // Error
        function () {
            if (callback)
                callback(false);

        }
    );
}
function SaveContract(step, callback, showMessage) {
    var formData = new FormData();

    AppendContract(formData);

    SaveToServer("BR_SaveContractForm", formData, function (result) {
        var contractId = result.data.data[0].id;
        $("#hdnContractId").val(contractId);

        // ============================
        // NAYA: Sirf tab invoice save karo jab Manual Form actually active ho
        // (naya invoice bana rahe ho ya edit kar rahe ho)
        // ============================
        var isManualFormActive = !$("#manualGrid").hasClass("d-none");
        var shouldSaveInvoice = (step >= 2) && isManualFormActive;

        if (shouldSaveInvoice) {
            SaveInvoice(step, callback, showMessage);
        }
        else if (step >= 3) {
            SavePayment(step, callback, showMessage);
        }
        else {
            if (showMessage) {
                Swal.fire("Saved", "Saved Successfully", "success");
            }

            if (callback)
                callback(true);
        }

    },
        // Error
        function () {
            if (callback)
                callback(false);

        }
    );

}
function SaveInvoice(step, callback, showMessage) {
    var formData = new FormData();
    var wasEditMode = $("#hdnInvoiceEditMode").val() == "1";   // pehle capture karo

    //// Milestone
    //if ($("#ddlInvoiceMilestone").val() == "" ||
    //    $("#ddlInvoiceMilestone").val() == null ||
    //    $("#ddlInvoiceMilestone").val() == "0") {

    //    Swal.fire({
    //        icon: "warning",
    //        title: "Validation",
    //        text: "Please select Milestone."
    //    });

    //    $("#ddlInvoiceMilestone").focus();
    //    return false;
    //}

    AppendInvoice(formData);

    SaveToServer("BR_SaveInvoideForm", formData, function (result) {
            var invoiceId = result.data.data[0].id;
            $("#hdnInvoiceId").val(invoiceId);

            // NAYA: Milestone ko locally "Invoiced" mark karo (sirf Custom contract)
            // taake dropdown agli dafa isay exclude kare
            if ($("#ddlbasicContractType").val() == "2") {
                var milestoneNo = $("#hdnInvoiceMilestoneNo").val();
                $("#tblMilestone tbody tr[data-milestone-no='" + milestoneNo + "']")
                    .attr("data-status", "Invoiced");
            }


            $("#hdnInvoiceEditMode").val("0");
            $("#txtInvoiceAmount").prop("disabled", true);

            // Grid refresh — updated amount turant dikhe
            GetInvoiceGrid($("#hdnClientId").val(), $("#hdnContractId").val());


            // NAYA: Chahe Add ho ya Edit, hamesha wapis Grid pe switch karo
            // (purana "if (wasEditMode)" condition hata diya)
            $("#manualGrid").addClass("d-none");
            $("#autoGrid").removeClass("d-none");
            $("#btnBackInvoiceGrid").hide();


            if (step >= 3) {
                SavePayment(step, callback, showMessage);
            }
            else {

                //if (wasEditMode) {   // ✅ ab captured value use ho raha hai
                //    $("#manualGrid").addClass("d-none");
                //    $("#autoGrid").removeClass("d-none");
                //    $("#btnBackInvoiceGrid").hide();
                //}

                if (showMessage) {
                    Swal.fire("Saved", "Saved Successfully", "success");
                }

                if (callback)
                    callback(true);
            }

        },
        function () {
            if (callback)
                callback(false);

     });

}
function SavePayment(step, callback, showMessage) {
    var formData = new FormData();
    AppendPayment(formData);

    SaveToServer("BR_SavePaymentForm", formData, function (result) {
        $("#hdnPaymentId").val(result.data.data[0].id);

        if (step >= 4) {
            SaveARExceptionInfo(step, callback, showMessage);
        } else {
            if (showMessage) Swal.fire("Saved", "Saved Successfully", "success");
            if (callback) callback(true);
        }
    }, function () {
        if (callback) callback(false);
    });

}
function SaveARExceptionInfo(step, callback, showMessage) {
    // Agar user Add New form pe hi nahi gaya (kabhi kholi nahi), skip kar do
    var isFormActive = !$("#arFormView").hasClass("d-none");

    if (!isFormActive) {
        if (step >= 5) {
            SaveRenewalInfo(step, callback, showMessage);
        } else {
            if (showMessage) Swal.fire("Saved", "Saved Successfully", "success");
            if (callback) callback(true);
        }
        return;
    }

    if ($("#ddlAROverdueInvoice").val() == "0" || !$("#ddlAROverdueInvoice").val()) {
        if (step >= 5) {
            SaveRenewalInfo(step, callback, showMessage);
        } else {
            if (callback) callback(true);
        }
        return;
    }

    var formData = new FormData();
    AppendARException(formData);

    SaveToServer("BR_SaveARException", formData, function (result) {
        $("#hdnARExceptionId").val(result.data.data[0].id);

        $("#arFormView").addClass("d-none");
        $("#arGridView").removeClass("d-none");
        GetARExceptionGrid($("#hdnClientId").val(), $("#hdnContractId").val());

        if (step >= 5) {
            SaveRenewalInfo(step, callback, showMessage);
        } else {
            if (showMessage)
                Swal.fire("Saved", "Saved Successfully", "success");

            if (callback)
                callback(true);

        }
    }, function () {
        if (callback) callback(false);
    });
}
function SaveRenewalInfo(step, callback, showMessage) {
    var isFormActive = !$("#renewFormView").hasClass("d-none");

    if (!isFormActive) {
        if (showMessage) Swal.fire("Saved", "Saved Successfully", "success");
        if (callback) callback(true);
        return;
    }

    if (!ValidateRenewal()) {
        if (callback) callback(false);
        return;
    }

    var formData = new FormData();
    AppendRenewal(formData);

    SaveToServer("BR_SaveRenewalForm", formData, function (result) {
        $("#hdnRenewId").val(result.data.data[0].id);

        $("#renewFormView").addClass("d-none");
        $("#renewGridView").removeClass("d-none");
        GetRenewalGrid($("#hdnClientId").val(), $("#hdnContractId").val());

        if (showMessage) Swal.fire("Saved", "Saved Successfully", "success");
        if (callback) callback(true);
    }, function () {
        if (callback) callback(false);
    });
}


function AppendClient(formData) {
    var rawClientId = $("#hdnClientId").val();
    var clientId = (!rawClientId || isNaN(rawClientId)) ? 0 : parseInt(rawClientId);

    // Basic Client Information
    formData.append("workflow", 'COB');
    formData.append("id", clientId);
    formData.append("client_name", $("#client_name").val() || "");
    formData.append("client_refno", $("#client_refno").val() || "");
    formData.append("industry_id", $("#ddlIndustry").val() || "");
    formData.append("company_size_id", $("#ddlCompanySize").val() || "");
    // Website
    formData.append("website", $("#website").val().trim().toLowerCase());

    // NTN (1234567-8 -> 12345678)
    formData.append("tax_registration_no", $("#tax_registration_no").val().replace(/-/g, "").trim());

    formData.append("country_id", $("#ddlCountry").val() || "");
    formData.append("billing_address", $("#billing_address").val() || "");

    // Contact Information
    formData.append("primary_contact_name", $("#primary_contact_name").val() || "");

    // Email
    formData.append("primary_contact_email", $("#primary_contact_email").val().trim().toLowerCase());

    // Mobile (92-300-1234567 -> 923001234567)
    formData.append("primary_contact_phone", $("#primary_contact_phone").val().replace(/\D/g, ""));

    ////Screen Value	        Database Value
    ////92-300-1234567	        923001234567
    ////1234567-8	            12345678
    ////Info@ABC.COM	        info@abc.com
    ////HTTPS:/WWW.ABC.COM	    https:/www.abc.com


    // Internal Meta
    formData.append("contract_type_id", 0);
    formData.append("account_owner", $("#txtAccOwner").val() || "");
    formData.append("support_owner_id", $("#ddlsupOwner").val() || "");
    formData.append("onboarding_start_date", $("#onboarding_start_date").val() || "");
    formData.append("high_value_client", $("#highvalue").is(":checked"));
    formData.append("priority_level", $("input[name='priority_level']:checked").val() || "");

    formData.append("userid", 1);
    formData.append("Action", $("#hdnAction").val());

    var action = $("#hdnAction").val();
    ////alert(action);

    // File Uploads (Keys MUST exactly match Model properties)
    var ndaFileInput = $("#nda_upload")[0];
    if (ndaFileInput && ndaFileInput.files.length > 0) {
        formData.append("nda_upload", ndaFileInput.files[0]);
    }

    var taxFileInput = $("#tax_verify_upload")[0];
    if (taxFileInput && taxFileInput.files.length > 0) {
        formData.append("tax_verify", taxFileInput.files[0]); // Changed key to 'tax_verify'
    }


    // Existing filenames bhi hamesha bhej do
    formData.append("nda_filename", $("#hdnNDAFile").val());
    formData.append("tax_verify_filename", $("#hdnTaxFile").val());


    //set clientid into hiddenfield to store next table contract...
    $("#hdnClientId").val(clientId);

}
function AppendContract(formData) {
    // =========================
    // Build Milestone List From Table
    // =========================
    var MilestoneList = [];

    $("#tblMilestone tbody tr").each(function (index) {
        var row = $(this);

        MilestoneList.push({
            milestone_no: index + 1,
            milestone_name: row.find("td:eq(0)").text().trim(),
            milestone_amount: parseFloat(row.find(".milestoneAmount").val()) || 0,
            due_date: row.find("td:eq(2) input").val(),
            remarks: "",
            status: "Pending"
        });

    });

    var rawContractId = $("#hdnContractId").val();
    var contractId = (!rawContractId || isNaN(rawContractId)) ? 0 : parseInt(rawContractId);

    formData.append("workflow", "COB");
    formData.append("instanceid", 0);
    formData.append("id", contractId);
    formData.append("contract_refno", $("#contract_no").val() || "");
    formData.append("client_id", $("#hdnClientId").val());

    var contractType = $("#ddlbasicContractType").val()

    formData.append("basic_contract_type", contractType);

    // SaaS / Custom Billing Type
    if (contractType == "1") { // SaaS
        //SaaS fields
        formData.append("bill_type_id", $("#ddlBillingType").val());
        formData.append("bill_freq_id", $("#ddlBillingFreq").val());
        formData.append("bill_start_date", $("#bill_start_date").val());
        formData.append("bill_end_date", $("#bill_end_date").val());
        formData.append("billing_cycle", $("#ddlBillingCycle").val());
        formData.append("currency_id", $("#ddlCurrency").val());
        formData.append("contract_value", $("#contract_value").val() || 0);
        formData.append("discount_percent", $("#discount").val() || 0);
        formData.append("tax_percent", $("#tax").val() || 0);
        formData.append("total_saas_amount", UnFormatAmount($("#total_saas_amount").val()) || 0);
        formData.append("auto_renew", $("#auto_renew").is(":checked"));

        //Custom Fields
        formData.append("project_name", "-");
        formData.append("no_of_milestones", 0);
        formData.append("payment_term_id", 0);
        formData.append("project_value", $("#projectValue").val() || 0);
        formData.append("m_discount", $("#m_discountper").val() || 0);
        formData.append("m_tax_percent", $("#m_taxper").val() || 0);
        formData.append("total_milestone_amount", UnFormatAmount($("#final_amount").val()) || 0);

    } else { //Custom

        //SaaS fields
        formData.append("bill_type_id", $("#ddlBillingType2").val());
        formData.append("bill_freq_id", 0);
        formData.append("bill_start_date", "NULL");
        formData.append("bill_end_date", "NULL");
        formData.append("billing_cycle", 0);

        //if custom milestone then default currency is PKR=1
        formData.append("currency_id", 1);
        formData.append("contract_value", $("#contract_value").val() || 0);
        formData.append("discount_percent", $("#discount").val() || 0);
        formData.append("tax_percent", $("#tax").val() || 0);
        formData.append("total_saas_amount", UnFormatAmount($("#total_saas_amount").val()) || 0);
        formData.append("auto_renew", $("#auto_renew").is(":checked"));

        //Custom Fields
        formData.append("project_name", $("#projectname").val());
        formData.append("no_of_milestones", $("#noMilestone").val());
        formData.append("payment_term_id", $("#ddlPaymentTerms").val());
        formData.append("project_value", $("#projectValue").val() || 0);
        formData.append("m_discount", $("#m_discountper").val() || 0);
        formData.append("m_tax_percent", $("#m_taxper").val() || 0);
        formData.append("total_milestone_amount", UnFormatAmount($("#final_amount").val()) || 0);

        formData.append("Milestones", JSON.stringify(MilestoneList));

    }

    formData.append("penalty_terms", $("#remarks").val() || "NULL");
    formData.append("support_hours", $("#supphours").val() || "NULL");
    formData.append("userid", 1);


    var params = new URLSearchParams(window.location.search);
    var type = params.get("type");
    ////alert(type);

    if (type === 'editfromgrid') {
        if (contractId == 0) {
            formData.append("Action", "Add");
        } else {
            formData.append("Action", "Edit");
        }
    } else {
        formData.append("Action", "Add");
    }


    // Signed Contract
    var doc1FileInput = $("#doc1_upload")[0];
    if (doc1FileInput && doc1FileInput.files.length > 0) {
        formData.append("doc1_upload", doc1FileInput.files[0]);
    }

    // Approved Proposal
    var doc2FileInput = $("#doc2_upload")[0];
    if (doc2FileInput && doc2FileInput.files.length > 0) {
        formData.append("doc2_upload", doc2FileInput.files[0]);
    }

    // Project Scope
    var doc3FileInput = $("#doc3_upload")[0];
    if (doc3FileInput && doc3FileInput.files.length > 0) {
        formData.append("doc3_upload", doc3FileInput.files[0]);
    }

    // SLA
    var doc4FileInput = $("#doc4_upload")[0];
    if (doc4FileInput && doc4FileInput.files.length > 0) {
        formData.append("doc4_upload", doc4FileInput.files[0]);
    }


    // Existing filenames hamesha bhejo
    formData.append("doc1_filename", $("#hdndoc1File").val());
    formData.append("doc2_filename", $("#hdndoc2File").val());
    formData.append("doc3_filename", $("#hdndoc3File").val());
    formData.append("doc4_filename", $("#hdndoc4File").val());

}
function AppendInvoice(formData) {
    var rawInvoiceId = $("#hdnInvoiceId").val();
    var invoiceId = (!rawInvoiceId || isNaN(rawInvoiceId)) ? 0 : parseInt(rawInvoiceId);
    var isInvoiceEdit = $("#hdnInvoiceEditMode").val() == "1";

    formData.append("workflow", "COB");
    formData.append("instanceid", 0);
    formData.append("invoice_id", invoiceId);
    formData.append("invoice_no", $("#txtInvoiceNo").val() || "");
    formData.append("client_id", $("#hdnClientId").val() || 0);
    formData.append("contract_id", $("#hdnContractId").val() || 0);

    var contractType = $("#ddlbasicContractType").val();
    formData.append("contract_type", contractType);

    // Milestone info
    if (contractType == "1") {
        formData.append("milestone_no", "NULL");
        // Contract ki fixed bill_start_date ki jagah, is invoice ka apna Invoice Date use karo
        formData.append("billing_period", $("#txtInvoiceDate").val() || $("#bill_start_date").val());
    }
    else {
        formData.append("milestone_no", $("#hdnInvoiceMilestoneNo").val() || "");
        formData.append("billing_period", $("#hdnInvoiceBillingPeriod").val() || "");

        //var milestoneNo = "", dueDate = "";
        //$("#tblMilestone tbody tr").each(function (index) {
        //    milestoneNo = index + 1;
        //    dueDate = $(this).find("td:eq(2) input").val();
        //    return false;
        //});
        //formData.append("milestone_no", milestoneNo);
        //formData.append("billing_period", dueDate);
    }

    // ============================
    // Amount Breakdown — common
    // ============================
    var gross = parseFloat($("#invGrossAmount").val()) || 0;
    var discPercent = parseFloat($("#invDiscountPercent").val()) || 0;
    var taxPercent = parseFloat($("#invTaxPercent").val()) || 0;

    var discAmt = gross * discPercent / 100;
    var netAmount = gross - discAmt;
    var taxAmt = netAmount * taxPercent / 100;
    var totalAmt = netAmount + taxAmt;

    formData.append("gross_amount", gross.toFixed(2));
    formData.append("discount_percent", discPercent);
    formData.append("tax_percent", taxPercent);
    formData.append("invoice_amount", netAmount.toFixed(2));
    formData.append("tax_amount", taxAmt.toFixed(2));
    formData.append("total_amount", totalAmt.toFixed(2));

    formData.append("invoice_type", $("#rdoManualInvoice").is(":checked") ? "Manual" : "Auto");
    formData.append("invoice_date", $("#txtInvoiceDate").val());
    formData.append("due_date", $("#txtInvoiceDueDate").val());
    formData.append("invoice_status", "1");
    formData.append("payment_status", "0");
    formData.append("remarks", isInvoiceEdit ? "Invoice Amount Updated" : "Manual Generate Invoice");
    formData.append("PONo", $("#txtInvoicePONo").val());
    formData.append("userid", 1);
    

    var params = new URLSearchParams(window.location.search);
    var type = params.get("type");

    if (isInvoiceEdit || type === "editfromgrid") {
        formData.append("Action", invoiceId == 0 ? "Add" : "Edit");
    } else {
        formData.append("Action", "Add");
    }
}
function AppendPayment(formData) {
    var PaymentList = [];

    ////invoice_amount: parseFloat($("#amount_received").val()) || 0,

    $("#tblPaymentDtl tr").each(function () {
        var chk = $(this).find(".chkInvoice");

        if (chk.is(":checked")) {
            PaymentList.push({
                invoice_id: chk.val(),
                invoice_amount: chk.data("amount"),   // FIX: apna invoice amount, poora amount_received nahi
                remarks: ""
            });
        }

    });

    var rawPaymentId = $("#hdnPaymentId").val();
    var paymentId = (!rawPaymentId || isNaN(rawPaymentId)) ? 0 : parseInt(rawPaymentId);

    formData.append("workflow", "COB");
    formData.append("payment_id", paymentId);
    formData.append("instanceid", 0);

    formData.append("client_id", $("#hdnClientId").val());
    formData.append("amount_received", $("#amount_received").val());
    formData.append("payment_mode", $("input[name='payment_mode']:checked").val());

    formData.append("ref_no", $("#txtReferenceNo").val());
    formData.append("bank_name", $("#txtBankName").val());
    formData.append("value_date", $("#txtReferenceDate").val());
    formData.append("userid", 1);

    var params = new URLSearchParams(window.location.search);
    var type = params.get("type");

    if (type === "editfromgrid") {
        formData.append("Action", paymentId == 0 ? "Add" : "Edit");
    }
    else {
        formData.append("Action", "Add");
    }


    // Receipt Upload
    var doc1FileInput = $("#payment_slip_upload")[0];
    if (doc1FileInput && doc1FileInput.files.length > 0) {
        formData.append("receipt_file", doc1FileInput.files[0]);
    }

    // Existing filenames hamesha bhejo
    formData.append("receipt_upload", $("#hdnpaymentslip").val());


    formData.append("PaymentDtl", JSON.stringify(PaymentList));

}
function AppendARException(formData) {
    formData.append("workflow", "COB");   
    formData.append("exception_id", $("#hdnARExceptionId").val() || 0);
    formData.append("client_id", $("#hdnClientId").val());
    formData.append("invoice_id", $("#ddlAROverdueInvoice").val());
    formData.append("instanceid", 0);
    formData.append("amount", $("#ar_amount").val() || 0);
    formData.append("days_overdue", $("#ar_days_overdue").val() || 0);
    formData.append("risk_category", $("input[name='ar-risk']:checked").val() || "Medium");
    formData.append("escalation_level", $("input[name='ar-escalation']:checked").val() || "Sales");
    formData.append("notes", $("#ar_notes").val() || "");
    formData.append("userid", 1);
    formData.append("createdby", 1);
    formData.append("Action", $("#hdnARExceptionId").val() == "0" ? "Add" : "Edit");
}
function AppendRenewal(formData) {
    formData.append("workflow", "COB");   
    formData.append("renew_id", $("#hdnRenewId").val() || 0);
    formData.append("client_id", $("#hdnClientId").val());
    formData.append("contract_id", $("#hdnContractId").val());
    formData.append("instanceid", 0);
    formData.append("renewal_term", $("#ddlRenewalTerm").val());
    formData.append("old_end_date", $("#renew_old_end_date").val() || "");
    formData.append("contract_value", $("#renew_contract_value").val() || 0);
    formData.append("discount_percent", $("#renew_discount_percent").val() || 0);
    formData.append("tax_percent", $("#renew_tax_percent").val() || 0);
    formData.append("final_amount", $("#renew_final_amount").val() || 0);
    formData.append("currency_id", $("#ddlCurrency").val() || "");
    formData.append("bill_freq_id", $("#ddlBillingFreq").val() || "");
    formData.append("billing_cycle", $("#ddlBillingCycle").val() || "");
    formData.append("start_date", $("#renew_start_date").val());
    formData.append("end_date", $("#renew_end_date").val());
    formData.append("renewal_status", $("#renew_status").val());
    formData.append("auto_renew", $("#renew_auto_renew").is(":checked"));

    formData.append("currency_id", $("#hdnRenewCurrencyId").val() || "");
    formData.append("bill_freq_id", $("#hdnRenewBillFreqId").val() || "");
    formData.append("billing_cycle", $("#hdnRenewBillingCycle").val() || "");

    var fileInput = document.getElementById("renew_doc_upload");
    if (fileInput && fileInput.files.length > 0) {
        formData.append("renewal_doc_file", fileInput.files[0]);   // ⚠️ naam confirm karo neeche
    }

    formData.append("userid", 1);
}

//--------------------------------
function SaveToServer(apiName, formData, successCallback = null, errorCallback = null) {
    new APICALL(GetGlobalURL('Base', apiName), 'POST', formData, true, true)
        .FETCH((result, error) => {
            // Success
            if (result && result.status === "success") {
                if (typeof successCallback === "function") {
                    successCallback(result);
                }

                return;
            }

            // Remove previous validation
            $(".is-invalid").removeClass("is-invalid");

            let msg = "Something went wrong";
            let field = "";
            let title = "Error";
            let icon = "warning";

            // API Success object
            if (result?.message) {
                msg = result.message;
                field = result.field || "";
                title = result.title || title;
            }
            else if (error?.responseJSON) {
                msg = error.responseJSON.message || msg;
                field = error.responseJSON.field || "";
                title = error.responseJSON.title || title;
            }
            else if (error?.data?.responseJSON) {
                msg = error.data.responseJSON.message || msg;
                field = error.data.responseJSON.field || "";
                title = error.data.responseJSON.title || title;
            }
            else if (error?.data?.responseText) {
                msg = error.data.responseText;
            }
            else if (error?.responseText) {
                msg = error.responseText;
            }
            else if (error?.message) {
                msg = error.message;
            }

            // Highlight duplicate field
            if (field) {
                $("#" + field)
                    .addClass("is-invalid")
                    .focus();
            }

            Swal.fire({
                icon: icon,
                title: title,
                text: msg
            }).then(() => {
                //console.log(">>> After Swal");
            });

            // <<< IMPORTANT
            if (typeof errorCallback === "function")
                errorCallback(error, result);

        });

}
// ******* Save Data END here *********


// Load Max ClientID, ContractID
function LoadMaxClientID() {
    ////console.trace("LoadMaxClientID CALLED");

    new APICALL(GetGlobalURL('Base', 'LoadMaxClientID'), 'GET', '', true).FETCH((result, error) => {
        if (result) {
            if (result.data != null && result.data.length > 0) {
                $("#client_refno").val(result.data[0].ReferenceNo);
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
function LoadMaxContractID() {
    new APICALL(GetGlobalURL('Base', 'LoadMaxContractID'), 'GET', '', true).FETCH((result, error) => {
        if (result) {
            if (result.data != null && result.data.length > 0) {
                $("#contract_no").val(result.data[0].ReferenceNo);

                $("#invTab_contract_no").val(result.data[0].ReferenceNo);
                $("#invTab_con_clientname").val($("#con_clientname").val());
                $("#invTab_contracttype").val($("#ddlbasicContractType").val());

                $("#paym_contract_no").val(result.data[0].ReferenceNo);

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
function LoadMaxInvoiceID() {
    new APICALL(GetGlobalURL('Base', 'LoadMaxInvoiceID'), 'GET', '', true).FETCH((result, error) => {
        if (result) {
            // Sirf tab skip karo jab koi EXISTING invoice edit ho rahi ho
            if ($("#hdnInvoiceEditMode").val() == "1") {
                return;
            }

            if (result.data != null && result.data.length > 0) {
                console.log('dsdasdasdas');
                console.log(result.data);


                $("#hdnInvoiceId").val(0);

                ////console.log('ref No: ' + result.data[0].ReferenceNo);
                $("#txtInvoiceNo").val(result.data[0].ReferenceNo);

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

// get Milestone Detail in Save
function GetMilestones() {
    var milestones = [];

    $("#tblMilestone tbody tr").each(function (i) {
        milestones.push({
            milestone_id: $(this).find(".milestone_id").val() || 0,
            milestone_no: i + 1,
            milestone_name: $(this).find(".milestone_name").val(),
            milestone_amount: $(this).find(".milestone_amount").val(),
            due_date: $(this).find(".due_date").val(),
            remarks: $(this).find(".remarks").val(),
            status: $(this).find(".status").val()
        });
    });
    return milestones;
}

// get Invoice Grid on Load
function GetInvoiceGrid(client_id = 0, contract_id = 0) {
    ShowLoader('InvoiceDiv');
    new APICALL(GetGlobalURL('Base', 'GetAllInvoicesGrid') + '?client_id=' + client_id + "&contract_id=" + contract_id,  'GET', '', true)
            .FETCH((result, error) => {
                if ($.fn.DataTable.isDataTable("#tblInvoiceGrid")) {
                    $("#tblInvoiceGrid").DataTable().clear().destroy();
                }

                $("#tblInvDtl").empty();

                if (result) {
                    var invoiceId = 0;
                    var rowIndex = 0;

                    if (result.data && result.data.length > 0) {
                        $.each(result.data, function (i, option) {
                            invoiceId = option.invoice_id;

                            // Invoice Status Badge
                            var invoiceStatus = "";

                            switch (option.invoice_status) {
                                case "Pending":
                                    invoiceStatus = '<span class="badge bg-warning text-dark">Pending</span>';
                                    break;
                                case "Paid":
                                    invoiceStatus = '<span class="badge bg-success">Paid</span>';
                                    break;
                                case "Over Due":
                                    invoiceStatus = '<span class="badge bg-danger">Over Due</span>';
                                    break;
                                case "Cancelled":
                                    invoiceStatus = '<span class="badge bg-secondary">Cancelled</span>';
                                    break;
                                default:
                                    invoiceStatus = option.invoice_status;
                                    break;
                            }

                            // Payment Badge
                            var paymentStatus = "";
                            if (option.payment_status == "Paid")
                                paymentStatus = '<span class="badge bg-success">Paid</span>';
                            else
                                paymentStatus = '<span class="badge bg-danger">Unpaid</span>';

                            // Action Buttons
                            var action = '';

                            // Edit — sirf Unpaid invoice pe
                            if (option.payment_status != "Paid") {
                                action += '<button type="button" class="btn btn-sm btn-warning EditInvoice me-1" ';
                                action += 'data-id="' + option.invoice_id + '" ';
                                action += 'data-no="' + option.invoice_no + '" ';
                                action += 'data-date="' + option.invoice_date + '" ';
                                action += 'data-due="' + option.due_date + '" ';
                                action += 'data-gross="' + (option.gross_amount || 0) + '" ';
                                action += 'data-disc="' + (option.discount_percent || 0) + '" ';
                                action += 'data-tax="' + (option.tax_percent || 0) + '" ';
                                action += 'data-milestone="' + (option.milestone_no || '') + '" ';      
                                action += 'data-billingperiod="' + (option.billing_period || '') + '">';
                                action += '<i class="fa fa-edit"></i>';
                                action += '</button>';
                            } //else { //==Paid

                                action += '<button type"button" class="btn btn-sm btn-info ViewInvoice1 me-1" ';
                                action += 'data-id="' + invoiceId + '" ';
                                action += 'data-row="' + i + '">';
                                action += '<i class="fa fa-eye"></i></button>';

                                action += '<button type"button" class="btn btn-sm btn-success DownloadInvoice1 me-1" ';
                                action += 'data-id="' + invoiceId + '">';
                                action += '<i class="fa fa-download"></i></button>';

                                action += '<button type"button" class="btn btn-sm btn-primary EmailInvoice1" ';
                                action += 'data-id="' + invoiceId + '">';
                                action += '<i class="fa fa-envelope"></i></button>';
                            //}

                            $("#tblInvDtl").append(
                                '<tr>' +
                                '<td>' + (i + 1) + '</td>' +
                                '<td>' + (option.invoice_no || '') + '</td>' +
                                '<td>' + (option.client_name || '') + '</td>' +
                                '<td>' + (option.contract_refno || '') + '</td>' +
                                '<td>' + (option.contract_type || '') + '</td>' +
                                '<td>' + (option.milestone_name || '-') + '</td>' +
                                '<td>' + FormatDate(option.invoice_date) + '</td>' +
                                '<td>' + FormatDate(option.due_date) + '</td>' +
                                '<td>' + (option.currency_code || '-') + '</td>' +
                                '<td class="text-end">' + parseFloat(option.total_amount || 0).toLocaleString() + '</td>' +
                                '<td class="text-end">' + parseFloat(option.total_PKR_amount || 0).toLocaleString() + '</td>' +
                                '<td>' + invoiceStatus + '</td>' +
                                '<td>' + paymentStatus + '</td>' +
                                '<td class="text-center">' + action + '</td>' +
                                '</tr>'

                            );

                        });

                        // <-- AFTER APPENDING ROWS
                        $("#tblInvoiceGrid").DataTable({
                            destroy: true,
                            autoWidth: false,
                            responsive: true,
                            language: {
                                emptyTable: "No Invoice Found"
                            }
                        });


                        // View
                        $(".ViewInvoice1").off().on("click", function (e) {
                            e.preventDefault();
                            e.stopPropagation();

                            invoiceId = $(this).data("id");
                            rowIndex = $(this).data("row");
                            ViewInvoice(invoiceId, rowIndex);
                        });

                        // Download
                        $(".DownloadInvoice1").off().on("click", function (e) {
                            e.preventDefault();
                            e.stopPropagation();

                            invoiceId = $(this).data("id");
                            DownloadInvoice(invoiceId);
                        });

                        // Email
                        $(".EmailInvoice1").off().on("click", function (e) {
                            e.preventDefault();
                            e.stopPropagation();

                            invoiceId = $(this).data("id");
                            EmailInvoice(invoiceId);
                        });

                    }
                    else {
                        $("#tblInvDtl").html(
                            '<tr><td colspan="13" class="text-center text-muted">No Invoice Found</td></tr>'
                        );

                    }

                    HideLoader('InvoiceDiv');

                }

                if (error) {
                    HideLoader('InvoiceDiv');
                    Swal.fire({
                        icon: 'error',
                        title: 'Error...',
                        text: error.data.responseText
                    });
                }
            });


}

// get Invoice in Payment tab
function GetUnPaidInvoiceList(client_id = 0, contract_id = 0) {
    new APICALL(GetGlobalURL('Base', 'GetAllUnPaidInvoiceList') + '?client_id=' + client_id + "&contract_id=" + contract_id, 'GET', '', true)
        .FETCH((result, error) => {
            if (error) {
                Swal.fire({
                    icon: 'error',
                    title: 'Error...',
                    text: error.data.responseText
                });
                return;
            }

            if ($.fn.DataTable.isDataTable("#tblPaymentGrid")) {
                $("#tblPaymentGrid").DataTable().clear().destroy();
            }

            $("#tblPaymentDtl").empty();

            if (result) {
                var paymentId = 0;

                if (result.data && result.data.length > 0) {
                    $.each(result.data, function (i, option) {
                        paymentId = option.payment_id;

                        $("#tblPaymentDtl").append(
                            '<tr>' +
                            '<td class="text-center">' +
                            '<input type="checkbox" class="chkInvoice"' +
                            ' value="' + option.invoice_id + '"' +
                            ' data-amount="' + option.total_amount + '">' +
                            '</td>' +

                            '<td>' + (option.invoice_no || '') + '</td>' +
                            '<td>' + FormatDate(option.invoice_date) + '</td>' +
                            '<td>' + FormatDate(option.due_date) + '</td>' +
                            '<td>' + (option.contract_refno || '') + '</td>' +
                            '<td>' + (option.milestone_name || '-') + '</td>' +
                            '<td class="text-end">' +
                                parseFloat(option.total_amount || 0).toLocaleString() +
                            '</td>' +
                            '</tr>'
                        );

                    });

                    $("#tblPaymentGrid").DataTable({
                        destroy: true,
                        autoWidth: false,
                        responsive: true,
                        language: {
                            emptyTable: "No Unpaid Invoice Found"
                        }
                    });


                }
                else {
                    $("#tblPaymentDtl").append(
                        '<tr><td colspan="7" class="text-center text-muted">No Unpaid Invoice Found</td></tr>'
                    );

                }

                InitPaymentGridEvents();

            }
        });


}

/* All DDL populate */
function GetAllIndustry_DDL() {
    new APICALL(GetGlobalURL('Base', 'GetIndustryDDL'), 'GET', '', true).FETCH((result, error) => {
        if (result) {
            if (result.data != null) {
                window.allUsers = result.data;

                var ddl = $('#ddlIndustry');
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
        GetAllCompanySize_DDL();
    });
}
function GetAllCompanySize_DDL() {
    new APICALL(GetGlobalURL('Base', 'GetCompanySizeDDL'), 'GET', '', true).FETCH((result, error) => {
        if (result) {
            if (result.data != null) {
                window.allUsers = result.data;

                var ddl = $('#ddlCompanySize');
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
        GetAllCountry_DDL();
    });
}
function GetAllCountry_DDL() {
    new APICALL(GetGlobalURL('Base', 'GetCountryDDL'), 'GET', '', true).FETCH((result, error) => {
        if (result) {
            if (result.data != null) {
                window.allUsers = result.data;

                var ddl = $('#ddlCountry');
                populateDropdowns(ddl, result.data, null, 1);
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
        GetAllContractType_DDL();
    });
}
function GetAllContractType_DDL() {
    new APICALL(GetGlobalURL('Base', 'GetContractTypeDDL'), 'GET', '', true).FETCH((result, error) => {
        if (result) {
            if (result.data != null) {
                window.allUsers = result.data;

                var ddl = $('#ddlContractType');
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
        GetAllAccountOwner_DDL();
    });
}
function GetAllAccountOwner_DDL() {
    new APICALL(GetGlobalURL('Base', 'GetAccountOwnerDDL'), 'GET', '', true).FETCH((result, error) => {
        if (result) {
            if (result.data != null) {
                window.allUsers = result.data;

                var ddl = $('#ddlaccOwner');
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
        GetAllSupportOwner_DDL();
    });
}
function GetAllSupportOwner_DDL() {
    new APICALL(GetGlobalURL('Base', 'GetSupportOwnerDDL'), 'GET', '', true).FETCH((result, error) => {
        if (result) {
            if (result.data != null) {
                window.allUsers = result.data;

                var ddl = $('#ddlsupOwner');
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
        GetAllContractTypeBasic_DDL();
    });
}
function GetAllContractTypeBasic_DDL() {
    new APICALL(GetGlobalURL('Base', 'GetContractTypeBasicDDL'), 'GET', '', true).FETCH((result, error) => {
        if (result) {
            if (result.data != null) {
                window.allUsers = result.data;

                var ddl = $('#ddlbasicContractType');
                populateDropdowns(ddl, result.data, null, 0);

                var ddl2 = $('#invTab_contracttype');
                populateDropdowns(ddl2, result.data, null, 0);

                var ddl3 = $('#paym_contracttype');
                populateDropdowns(ddl3, result.data, null, 0);

                var ddl4 = $('#ar_contracttype');
                populateDropdowns(ddl4, result.data, null, 0);

                var ddl5 = $('#renew_contracttype');
                populateDropdowns(ddl5, result.data, null, 0);
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
        GetAllBillingFrequency_DDL();
    });
}
function GetAllBillingFrequency_DDL() {
    new APICALL(GetGlobalURL('Base', 'GetBillingFrequencyDDL'), 'GET', '', true).FETCH((result, error) => {
        if (result) {
            if (result.data != null) {
                window.allUsers = result.data;

                var ddl = $('#ddlBillingFreq');
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
        GetAllPaymentTerms_DDL();
    });
}
function GetAllPaymentTerms_DDL() {
    new APICALL(GetGlobalURL('Base', 'GetPaymentTermsDDL'), 'GET', '', true).FETCH((result, error) => {
        if (result) {
            if (result.data != null) {
                window.allUsers = result.data;

                var ddl = $('#ddlPaymentTerms');
                populateDropdowns(ddl, result.data, null, 1);
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
        GetAllBillingType_DDL();
    });
}
function GetAllBillingType_DDL() {
    new APICALL(GetGlobalURL('Base', 'GetBillingTypeDDL'), 'GET', '', true).FETCH((result, error) => {
        if (result) {
            if (result.data != null) {
                window.allUsers = result.data;

                var ddl = $('#ddlBillingType');
                populateDropdowns(ddl, result.data, null, 0);

                var ddl = $('#ddlBillingType2');
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
        GetAllBillingStatus_DDL();
    });
}
function GetAllBillingStatus_DDL() {
    new APICALL(GetGlobalURL('Base', 'GetBillingStatusDDL'), 'GET', '', true).FETCH((result, error) => {
        if (result) {
            if (result.data != null) {
                window.allUsers = result.data;

                var ddl = $('#ddlBillingStatus');
                populateDropdowns(ddl, result.data, null, 0);

                var ddl = $('#ddlInternalOwner');
                populateDropdowns(ddl, result.data, null, 0);

                var ddl = $('#ddlClientApprover');
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
        GetAllCurrency_DDL();
    });
}
function GetAllCurrency_DDL() {
    new APICALL(GetGlobalURL('Base', 'GetCurrencyDDL'), 'GET', '', true).FETCH((result, error) => {
        if (result) {
            if (result.data != null) {
                window.allUsers = result.data;

                var ddl = $('#ddlCurrency');
                populateDropdowns(ddl, result.data, null, 1);
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
        PopulateFieldsOnEdit();
        //GetClientInfoDDL();
    });
}
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
    GetAllIndustry_DDL();
    ////GetAllCompanySize_DDL();
    ////GetAllCountry_DDL();
    ////GetAllContractType_DDL();
    ////GetAllAccountOwner_DDL();
    ////GetAllSupportOwner_DDL();
    ////GetAllContractTypeBasic_DDL();
    ////GetAllBillingFrequency_DDL();
    ////GetAllPaymentTerms_DDL();
    ////GetAllBillingType_DDL();
    ////GetAllBillingStatus_DDL();
    ////GetAllCurrency_DDL();

// +++++++++++++ old code
//    GetAllIndustry_DDL();
//    GetAllCompanySize_DDL();
//    GetAllCountry_DDL();
//    GetAllContractType_DDL();
//    GetAllAccountOwner_DDL();
//    GetAllSupportOwner_DDL();
//    GetAllContractTypeBasic_DDL();
//    GetAllBillingFrequency_DDL();
//    GetAllPaymentTerms_DDL();
//    GetAllBillingType_DDL();
//    GetAllBillingStatus_DDL();
//    GetAllCurrency_DDL();
}
function ClearAllFields() {
    // Hidden Fields
    $("#hdnClientId").val("0");
    $("#hdnAction").val("Add");
    $("#hdnContractId").val("0");

    // Text, Number, Email, URL, Date
    $("input[type='text'], input[type='number'], input[type='email'], input[type='url'], input[type='date']").val("");

    // Textarea
    $("textarea").val("");

    // Dropdowns
    $("select").prop("selectedIndex", 0).trigger("change");

    // Radio Buttons
    $("input[type='radio']").prop("checked", false);
    $("#NewClient").prop("checked", true);   // Default

    // Checkboxes
    $("input[type='checkbox']").prop("checked", false);

    // File Uploads
    $("input[type='file']").val("");
    $(".file-text").text("PDF, DOCX, DOC");
    $("small[id$='_existing_file']").text("");

    // Disabled Fields (Reference Numbers etc.)
    $("#client_refno").val("");
    $("#contract_no").val("");
    $("#con_clientname").val("");
    $("#invTab_con_clientname").val("");
    $("#ar_con_clientname").val("");
    $("#renew_con_clientname").val("");

    $("#total_saas_amount").val("");
    $("#total_saas_amount").prop("disabled", true);

    $("#final_amount").val("");
    $("#txtGrandTotal").val("");
    $("#remarks").val("");
    $("#supphours").val("");

    // Hide Sections
    $("#search_client").addClass("d-none");
    $("#saas").addClass("d-none");
    $("#custom").addClass("d-none");
    $("#docSection").addClass("d-none");

    // Milestone Table
    $("#tblMilestone tbody").empty();

}

/* Max IDs */
function AutoGenerateID() {
    LoadMaxClientID();
    LoadMaxContractID();
    LoadMaxInvoiceID();
}
function setInvalid(selector, message) {
    $(selector).addClass("is-invalid");

    Swal.fire({
        icon: 'warning',
        text: message,
        confirmButtonColor: "#61affe"
    });
}

/* Enable Tab when all fields are valid or fill */
function EnableStep(index) {
    var $step = $("#client-onboarding .steps ul li").eq(index);

    $step.removeClass("disabled");
    $step.attr("aria-disabled", "false");

    $step.find("a").css({
        "pointer-events": "auto",
        "cursor": "pointer",
        "opacity": "1"
    });
}
function EnableCompletedSteps() {
    // Step-1 Client -- First Tab always enable
    EnableStep(0);

    // Step-2 Contract
    if (parseInt($("#hdnClientId").val()) > 0)
        EnableStep(0);

    // Step-3 Invoice
    if (parseInt($("#hdnContractId").val()) > 0) {
        EnableStep(2); //--Invoice Tab
        EnableStep(3); //--Payment Tab
    }

    // Step-4 Payment
    if (parseInt($("#hdnInvoiceId").val()) > 0)
        EnableStep(3); //--Payment Tab

    // Step-5 AR Exception
    if (parseInt($("#hdnPaymentId").val()) > 0)
        EnableStep(4); //--ARException Tab

    // Step-6 Renewal Tab
    if (parseInt($("#hdnARExceptionId").val()) > 0)
        EnableStep(5); //--Renewal Tab

}
function DisableStep(index) {
    var $step = $("#client-onboarding .steps ul li").eq(index);

    $step.addClass("disabled");
    $step.attr("aria-disabled", "true");

    // click bhi disable
    $step.find("a").css({
        "pointer-events": "none",
        "cursor": "default",
        "opacity": "0.5"
    });
}
function DisableContractTab(disable = true) {
    $("#form-contract")
        .find("input, select, textarea, button")
        .prop("disabled", disable);

    $("#tblMilestone")
        .find("input, select, textarea, button")
        .prop("disabled", true)
        .prop("readonly", true);

    // Hidden flag
    $("#hdnContractEditMode").val(disable ? "1" : "0");

    // Ye fields waise bhi read-only rehni chahiye
    $("#contract_no").prop("disabled", true);
    $("#con_clientname").prop("disabled", true);
    $("#hdnContractId").prop("disabled", true);
}

pdfjsLib.GlobalWorkerOptions.workerSrc = "/Public/pdfjs/pdf.worker.js";

async function PreviewPDF(file) {
    $("#pdfPreviewContainer").show();
    $("#excelPreviewContainer").hide();
    $("#docxPreviewContainer").hide();

    // Clear previous PDF pages
    $("#pdfPreviewContainer").empty();

    try {

        const arrayBuffer = await file.arrayBuffer();

        const pdf = await pdfjsLib.getDocument({
            data: arrayBuffer
        }).promise;

        // Pehle modal show karo
        $("#DocumentPreviewModal").modal("show");

        // Modal render hone ka wait
        await new Promise(resolve => setTimeout(resolve, 200));

        const container = document.getElementById("pdfPreviewContainer");

        const containerWidth = container.clientWidth;

        if (!containerWidth) {
            throw new Error("PDF preview container width is 0.");
        }

        // Render all PDF pages
        for (let pageNumber = 1; pageNumber <= pdf.numPages; pageNumber++) {

            const page = await pdf.getPage(pageNumber);

            const originalViewport = page.getViewport({
                scale: 1
            });

            // Modal width ke according scale
            const scale = containerWidth / originalViewport.width;

            const viewport = page.getViewport({
                scale: scale
            });

            // Dynamic canvas
            const canvas = document.createElement("canvas");

            canvas.className = "pdf-page";

            canvas.width = viewport.width;
            canvas.height = viewport.height;

            canvas.style.width = "100%";
            canvas.style.height = "auto";
            canvas.style.display = "block";
            canvas.style.marginBottom = "15px";

            container.appendChild(canvas);

            const context = canvas.getContext("2d");

            await page.render({
                canvasContext: context,
                viewport: viewport
            }).promise;
        }

    }
    catch (error) {

        console.error("PDF Preview Error:", error);

        $("#DocumentPreviewModal").modal("hide");

        Swal.fire({
            icon: "error",
            title: "PDF Preview Error",
            text: "Unable to preview PDF file."
        });
    }
}
async function PreviewExcel(file) {

    $("#pdfPreviewContainer").hide();
    $("#excelPreviewContainer").show();
    $("#docxPreviewContainer").hide();

    const arrayBuffer = await file.arrayBuffer();

    const workbook = XLSX.read(arrayBuffer, {
        type: "array"
    });

    let html = "";

    workbook.SheetNames.forEach(function (sheetName) {

        const worksheet = workbook.Sheets[sheetName];

        html += `
            <h6 class="fw-bold mt-3 mb-2">
                ${sheetName}
            </h6>
        `;

        html += XLSX.utils.sheet_to_html(worksheet, {
            editable: false
        });
    });

    $("#excelPreviewContainer").html(html);

    $("#DocumentPreviewModal").modal("show");
}
async function PreviewDocx(file) {
    $("#pdfPreviewContainer").hide();
    $("#excelPreviewContainer").hide();
    $("#docxPreviewContainer").show();

    const arrayBuffer = await file.arrayBuffer();

    const result = await mammoth.convertToHtml({
        arrayBuffer: arrayBuffer
    });

    $("#docxPreviewContainer").html(result.value);

    $("#DocumentPreviewModal").modal("show");
}
function PreviewDocument() {
    //+++++++++++++++++++++++++++++++++++++++++++++++++++++++
    let ndaFile = null;
    let taxFile = null;

    let doc1File = null;
    let doc2File = null;
    let doc3File = null;
    let doc4File = null;

    $("#nda_upload").change(function () {
        ndaFile = this.files[0];

        if (!ndaFile) return;
        $("#nda_actions").show();
    });


    $("#previewNDA").click(function () {
        if (!ndaFile)
            return;

        const extension = ndaFile.name
            .substring(ndaFile.name.lastIndexOf("."))
            .toLowerCase();

        if (extension === ".pdf") {
            PreviewPDF(ndaFile);
        }
        else if (extension === ".xlsx" || extension === ".xls") {
            PreviewExcel(ndaFile);
        }
        else if (extension === ".docx") {
            PreviewDocx(ndaFile);
        }
        else if (extension === ".doc") {
            Swal.fire({
                icon: "info",
                title: "Preview Not Available",
                text: "DOC format preview is not supported. Please upload DOCX instead."
            });
        }
        else {
            Swal.fire({
                icon: "warning",
                title: "Preview Not Available",
                text: "Preview is not supported for this file type."
            });
        }

    });


    //+++++++++++++++++++++++++++++++++++++++++++++++++++++++
    $("#tax_verify_upload").change(function () {
        taxFile = this.files[0];

        if (!taxFile) return;
        $("#tax_actions").show();
    });


    $("#previewtax").click(function () {
        if (!taxFile)
            return;

        const extension = taxFile.name
            .substring(taxFile.name.lastIndexOf("."))
            .toLowerCase();

        if (extension === ".pdf") {
            PreviewPDF(taxFile);
        }
        else if (extension === ".xlsx" || extension === ".xls") {
            PreviewExcel(taxFile);
        }
        else if (extension === ".docx") {
            PreviewDocx(taxFile);
        }
        else if (extension === ".doc") {
            Swal.fire({
                icon: "info",
                title: "Preview Not Available",
                text: "DOC format preview is not supported. Please upload DOCX instead."
            });
        }
        else {
            Swal.fire({
                icon: "warning",
                title: "Preview Not Available",
                text: "Preview is not supported for this file type."
            });
        }

    });


    //+++++++++++++++++++++++++++++++++++++++++++++++++++++++
    $("#doc1_upload").change(function () {
        doc1File = this.files[0];

        if (!doc1File) return;
        $("#doc1_actions").show();
    });


    $("#previewdoc1").click(function () {
        if (!doc1File)
            return;

        const extension = doc1File.name
            .substring(doc1File.name.lastIndexOf("."))
            .toLowerCase();

        if (extension === ".pdf") {
            PreviewPDF(doc1File);
        }
        else if (extension === ".xlsx" || extension === ".xls") {
            PreviewExcel(doc1File);
        }
        else if (extension === ".docx") {
            PreviewDocx(doc1File);
        }
        else if (extension === ".doc") {
            Swal.fire({
                icon: "info",
                title: "Preview Not Available",
                text: "DOC format preview is not supported. Please upload DOCX instead."
            });
        }
        else {
            Swal.fire({
                icon: "warning",
                title: "Preview Not Available",
                text: "Preview is not supported for this file type."
            });
        }

    });


    //+++++++++++++++++++++++++++++++++++++++++++++++++++++++
    $("#doc2_upload").change(function () {
        doc2File = this.files[0];

        if (!doc2File) return;
        $("#doc2_actions").show();
    });


    $("#previewdoc2").click(function () {
        if (!doc2File)
            return;

        const extension = doc2File.name
            .substring(doc2File.name.lastIndexOf("."))
            .toLowerCase();

        if (extension === ".pdf") {
            PreviewPDF(doc2File);
        }
        else if (extension === ".xlsx" || extension === ".xls") {
            PreviewExcel(doc2File);
        }
        else if (extension === ".docx") {
            PreviewDocx(doc2File);
        }
        else if (extension === ".doc") {
            Swal.fire({
                icon: "info",
                title: "Preview Not Available",
                text: "DOC format preview is not supported. Please upload DOCX instead."
            });
        }
        else {
            Swal.fire({
                icon: "warning",
                title: "Preview Not Available",
                text: "Preview is not supported for this file type."
            });
        }

    });


    //+++++++++++++++++++++++++++++++++++++++++++++++++++++++
    $("#doc3_upload").change(function () {
        doc3File = this.files[0];

        if (!doc3File) return;
        $("#doc3_actions").show();
    });


    $("#previewdoc3").click(function () {
        if (!doc3File)
            return;

        const extension = doc3File.name
            .substring(doc3File.name.lastIndexOf("."))
            .toLowerCase();

        if (extension === ".pdf") {
            PreviewPDF(doc3File);
        }
        else if (extension === ".xlsx" || extension === ".xls") {
            PreviewExcel(doc3File);
        }
        else if (extension === ".docx") {
            PreviewDocx(doc3File);
        }
        else if (extension === ".doc") {
            Swal.fire({
                icon: "info",
                title: "Preview Not Available",
                text: "DOC format preview is not supported. Please upload DOCX instead."
            });
        }
        else {
            Swal.fire({
                icon: "warning",
                title: "Preview Not Available",
                text: "Preview is not supported for this file type."
            });
        }

    });


    //+++++++++++++++++++++++++++++++++++++++++++++++++++++++
    $("#doc4_upload").change(function () {
        doc4File = this.files[0];

        if (!doc4File) return;
        $("#doc4_actions").show();
    });


    $("#previewdoc4").click(function () {
        if (!doc4File)
            return;

        const extension = doc4File.name
            .substring(doc4File.name.lastIndexOf("."))
            .toLowerCase();

        if (extension === ".pdf") {
            PreviewPDF(doc4File);
        }
        else if (extension === ".xlsx" || extension === ".xls") {
            PreviewExcel(doc4File);
        }
        else if (extension === ".docx") {
            PreviewDocx(doc4File);
        }
        else if (extension === ".doc") {
            Swal.fire({
                icon: "info",
                title: "Preview Not Available",
                text: "DOC format preview is not supported. Please upload DOCX instead."
            });
        }
        else {
            Swal.fire({
                icon: "warning",
                title: "Preview Not Available",
                text: "Preview is not supported for this file type."
            });
        }

    });


}
function FileInfo() {
    $("#nda_upload").on("change", function () {
        if (this.files.length > 0) {
            $("#nda_existing_file").html("").hide();

            var file = this.files[0];
            var fileSizeMB = (file.size / (1024 * 1024)).toFixed(2);

            $("#nda_file_size").text(fileSizeMB + " MB");
            $("#nda_file_info").show();
        }
    });

    $("#tax_verify_upload").on("change", function () {
        if (this.files.length > 0) {
            $("#tax_existing_file").html("").hide();

            var file = this.files[0];
            var fileSizeMB = (file.size / (1024 * 1024)).toFixed(2);

            $("#tax_file_size").text(fileSizeMB + " MB");
            $("#tax_file_info").show();
        }
    });

    //+++++++++++++++++++++++++++++
    $("#doc1_upload").on("change", function () {
        if (this.files.length > 0) {
            $("#doc1_existing_file").html("").hide();

            var file = this.files[0];
            var fileSizeMB = (file.size / (1024 * 1024)).toFixed(2);

            $("#doc1_file_size").text(fileSizeMB + " MB");
            $("#doc1_file_info").show();
        }
    });


    $("#doc2_upload").on("change", function () {
        if (this.files.length > 0) {
            $("#doc2_existing_file").html("").hide();

            var file = this.files[0];
            var fileSizeMB = (file.size / (1024 * 1024)).toFixed(2);

            $("#doc2_file_size").text(fileSizeMB + " MB");
            $("#doc2_file_info").show();
        }
    });


    $("#doc3_upload").on("change", function () {
        if (this.files.length > 0) {
            $("#doc3_existing_file").html("").hide();

            var file = this.files[0];
            var fileSizeMB = (file.size / (1024 * 1024)).toFixed(2);

            $("#doc3_file_size").text(fileSizeMB + " MB");
            $("#doc3_file_info").show();
        }
    });

    $("#doc4_upload").on("change", function () {
        if (this.files.length > 0) {
            $("#doc4_existing_file").html("").hide();

            var file = this.files[0];
            var fileSizeMB = (file.size / (1024 * 1024)).toFixed(2);

            $("#doc4_file_size").text(fileSizeMB + " MB");
            $("#doc4_file_info").show();
        }
    });

}



//++++++++++++ Last Two Tabs ARException and Renewal ++++++++++
//==============================================================
// Grid load karo
// ================================
function GetARExceptionGrid(clientId, contractId) {
    new APICALL(GetGlobalURL('Base', 'GetARExceptionGrid') + '?clientId=' + clientId + '&contractId=' + contractId, 'GET', '', true)
        .FETCH((result, error) => {
            if (error) return;

            var rows = "";
            var data = result.data || [];

            if (data.length === 0) {
                rows = `<tr><td colspan="8" class="text-center text-muted">No AR exception cases found.</td></tr>`;
            } else {
                $.each(data, function (i, r) {
                    rows += `
                        <tr>
                            <td>${r.invoice_no}</td>
                            <td>${FormatDateDisplay(r.due_date)}</td>
                            <td>${parseFloat(r.amount || 0).toLocaleString()}</td>
                            <td>${r.days_overdue}</td>
                            <td><span class="badge bg-${r.risk_category === 'High' ? 'danger' : r.risk_category === 'Medium' ? 'warning' : 'success'}">${r.risk_category}</span></td>
                            <td>${r.escalation_level}</td>
                            <td>${r.notes || ''}</td>
                            <td>
                                <button type="button" class="btn btn-sm btn-warning EditARException"
                                    data-id="${r.exception_id}" data-invoice="${r.invoice_id}"
                                    data-invoiceno="${r.invoice_no}" data-amount="${r.amount}"
                                    data-overdue="${r.days_overdue}" data-risk="${r.risk_category}"
                                    data-escalation="${r.escalation_level}" data-notes="${r.notes || ''}">
                                    <i class="fa fa-edit"></i>
                                </button>
                            </td>
                        </tr>`;
                });
            }

            $("#tblARException tbody").html(rows);
        });
}
function GetRenewalGrid(clientId, contractId) {
    new APICALL(GetGlobalURL('Base', 'GetRenewalGrid') + '?clientId=' + clientId + '&contractId=' + contractId, 'GET', '', true)
        .FETCH((result, error) => {
            if (error) return;

            var rows = "";
            var data = result.data || [];
            var approvedRenewal = null;   // ✅ yeh line add karo

            if (data.length === 0) {
                rows = `<tr><td colspan="6" class="text-center text-muted">No renewal requests found.</td></tr>`;
            } else {
                $.each(data, function (i, r) {
                    var statusBadge = r.renewal_status === "Approved" ? "success"
                        : r.renewal_status === "Rejected" ? "danger"
                            : r.renewal_status === "Completed" ? "primary"
                                : "warning";

                    if (r.renewal_status === "Approved" && !approvedRenewal) {
                        approvedRenewal = r;   // sabse pehli Approved renewal pakar lo
                    }

                    // ✅ Sirf tab Edit button banao jab status "Completed" na ho
                    var actionHtml = "";
                    if (r.renewal_status !== "Completed") {
                        actionHtml = `
                            <button type="button" class="btn btn-sm btn-warning EditRenewal"
                                data-id="${r.renew_id}" data-term="${r.renewal_term}"
                                data-oldend="${r.old_end_date || ''}" data-start="${r.start_date}"
                                data-end="${r.end_date}" data-value="${r.contract_value}"
                                data-disc="${r.discount_percent}" data-tax="${r.tax_percent}"
                                data-status="${r.renewal_status}" data-autorenew="${r.auto_renew}">
                                <i class="fa fa-edit"></i>
                            </button>`;
                    } else {
                        //actionHtml = `<span class="text-muted small">—</span>`;

                        actionHtml = `
                            <button type="button" class="btn btn-sm btn-warning EditRenewal"
                                data-id="${r.renew_id}" data-term="${r.renewal_term}"
                                data-oldend="${r.old_end_date || ''}" data-start="${r.start_date}"
                                data-end="${r.end_date}" data-value="${r.contract_value}"
                                data-disc="${r.discount_percent}" data-tax="${r.tax_percent}"
                                data-status="${r.renewal_status}" data-autorenew="${r.auto_renew}">
                                <i class="fa fa-edit"></i>
                            </button>`;

                    }

                    rows += `
                        <tr>
                            <td>${r.renewal_term}</td>
                            <td>${FormatDateDisplay(r.start_date)}</td>
                            <td>${FormatDateDisplay(r.end_date)}</td>
                            <td>${parseFloat(r.final_amount || 0).toLocaleString()}</td>
                            <td><span class="badge bg-${statusBadge}">${r.renewal_status}</span></td>
                            <td>${actionHtml}</td>
                        </tr>`;
                });
            }

            $("#tblRenewal tbody").html(rows);

            // Convert to Contract button — sirf Approved renewal mile to dikhao
            if (approvedRenewal) {
                $("#btnConvertToContract").removeClass("d-none").attr("data-id", approvedRenewal.renew_id);
            } else {
                $("#btnConvertToContract").addClass("d-none").attr("data-id", "");
            }


        });
}


// ================================
// Overdue invoices (Add New ke dropdown ke liye)
// ================================
function GetOverdueInvoices(clientId, contractId) {
    new APICALL(GetGlobalURL('Base', 'GetOverdueInvoices') + '?clientId=' + clientId + '&contractId=' + contractId, 'GET', '', true)
        .FETCH((result, error) => {
            if (error) return;

            var $ddl = $("#ddlAROverdueInvoice");
            $ddl.empty().append('<option value="0">Select Invoice</option>');

            if (result.data && result.data.length > 0) {
                $.each(result.data, function (i, inv) {
                    $ddl.append(
                        `<option value="${inv.invoice_id}" data-amount="${inv.total_amount}" data-overdue="${inv.days_overdue}">${inv.invoice_no} (${inv.days_overdue} days overdue)</option>`
                    );
                });
            }
        });
}

$(document).on("change", "#ddlAROverdueInvoice", function () {
    var selected = $(this).find("option:selected");

    $("#ar_invoice").val(selected.text() || "");
    $("#ar_amount").val(selected.data("amount") || "");
    $("#ar_days_overdue").val(selected.data("overdue") || "");
});

// ================================
// Add New / Edit / Back navigation
// ================================
$(document).on("click", "#btnAddNewARException", function () {
    $("#hdnARExceptionId").val(0);
    $("#ddlAROverdueInvoice").prop("disabled", false);
    $("#ar_amount, #ar_days_overdue").val("");
    $("#ar_notes").val("");
    $("#risk-medium").prop("checked", true);
    $("#esc-sales").prop("checked", true);

    GetOverdueInvoices($("#hdnClientId").val(), $("#hdnContractId").val());

    $("#arGridView").addClass("d-none");
    $("#arFormView").removeClass("d-none");
});

$(document).on("click", ".EditARException", function () {
    $("#hdnARExceptionId").val($(this).data("id"));

    var $ddl = $("#ddlAROverdueInvoice");
    $ddl.empty().append(`<option value="${$(this).data('invoice')}">${$(this).data('invoiceno')}</option>`);
    $ddl.prop("disabled", true);   // Edit mode mein invoice change na ho

    $("#ar_amount").val($(this).data("amount"));
    $("#ar_days_overdue").val($(this).data("overdue"));
    $("#ar_notes").val($(this).data("notes"));

    var risk = $(this).data("risk");
    $("input[name='ar-risk'][value='" + risk + "']").prop("checked", true);

    var escalation = $(this).data("escalation");
    $("input[name='ar-escalation'][value='" + escalation + "']").prop("checked", true);

    $("#arGridView").addClass("d-none");
    $("#arFormView").removeClass("d-none");
});

$(document).on("click", "#btnBackARGrid", function () {
    $("#arFormView").addClass("d-none");
    $("#arGridView").removeClass("d-none");
    GetARExceptionGrid($("#hdnClientId").val(), $("#hdnContractId").val());
});


$(document).on("click", "#btnAddNewRenewal", function () {
    $("#hdnRenewId").val(0);
    $("#ddlRenewalTerm").val("");
    $("#renew_start_date, #renew_end_date").val("");
    $("#renew_status").val("Pending");
    $("#renew_auto_renew").prop("checked", false);

    var contractCurrencyId = $("#ddlCurrency").val();   // Contract tab ka currency
    var contractBillFreq = $("#ddlBillingFreq").val();
    var contractBillCycle = $("#ddlBillingCycle").val();

    $("#hdnRenewCurrencyId").val(contractCurrencyId);
    $("#hdnRenewBillFreqId").val(contractBillFreq);
    $("#hdnRenewBillingCycle").val(contractBillCycle);

    var contractType = $("#ddlbasicContractType").val();   // 1=SaaS, 2=Custom

    if (contractType == "1") {
        // SaaS
        $("#renew_contract_value").val($("#contract_value").val() || 0);
        $("#renew_discount_percent").val($("#discount").val() || 0);
        $("#renew_tax_percent").val($("#tax").val() || 0);
        $("#renew_old_end_date").val($("#bill_end_date").val() || "");
    }
    else {
        // Custom/Milestone
        $("#renew_contract_value").val($("#projectValue").val() || 0);
        $("#renew_discount_percent").val($("#m_discountper").val() || 0);
        $("#renew_tax_percent").val($("#m_taxper").val() || 0);

        var lastMilestoneDate = "";
        $("#tblMilestone tbody tr").each(function () {
            var d = $(this).find(".milestoneDate").val();
            if (d) lastMilestoneDate = d;
        });
        $("#renew_old_end_date").val(lastMilestoneDate);
    }

    calculateRenewalFinalAmount();
    UpdateRenewalLabels();

    $("#renewGridView").addClass("d-none");
    $("#renewFormView").removeClass("d-none");
});

function UpdateRenewalLabels() {
    var contractType = $("#ddlbasicContractType").val();
    var label = contractType == "2" ? "Project Value:" : "Contract Value:";
    $("#renewContractValueLabel").text(label);
}

$(document).on("click", ".EditRenewal", function () {
    $("#hdnRenewId").val($(this).data("id"));
    $("#ddlRenewalTerm").val($(this).data("term"));
    $("#renew_old_end_date").val($(this).data("oldend") ? String($(this).data("oldend")).split('T')[0] : "");
    $("#renew_start_date").val(String($(this).data("start")).split('T')[0]);
    $("#renew_end_date").val(String($(this).data("end")).split('T')[0]);
    $("#renew_contract_value").val($(this).data("value"));
    $("#renew_discount_percent").val($(this).data("disc"));
    $("#renew_tax_percent").val($(this).data("tax"));
    $("#renew_status").val($(this).data("status"));
    $("#renew_auto_renew").prop("checked", $(this).data("autorenew") == true || $(this).data("autorenew") == "true");

    calculateRenewalFinalAmount();

    $("#renewGridView").addClass("d-none");
    $("#renewFormView").removeClass("d-none");
});

$(document).on("click", "#btnBackRenewGrid", function () {
    $("#renewFormView").addClass("d-none");
    $("#renewGridView").removeClass("d-none");
    GetRenewalGrid($("#hdnClientId").val(), $("#hdnContractId").val());
});

function calculateRenewalFinalAmount() {
    var value = parseFloat($("#renew_contract_value").val()) || 0;
    var disc = parseFloat($("#renew_discount_percent").val()) || 0;
    var tax = parseFloat($("#renew_tax_percent").val()) || 0;

    var afterDisc = value - (value * disc / 100);
    var final = afterDisc + (afterDisc * tax / 100);

    $("#renew_final_amount").val(final.toFixed(2));
}
function FormatDateDisplay(dateVal) {
    if (!dateVal) return "";
    var d = new Date(dateVal);
    if (isNaN(d)) return "";
    var dd = String(d.getDate()).padStart(2, '0');
    var mm = String(d.getMonth() + 1).padStart(2, '0');
    return dd + "-" + mm + "-" + d.getFullYear();
}

$(document).on("input", "#renew_contract_value, #renew_discount_percent, #renew_tax_percent", calculateRenewalFinalAmount);

$(document).on("change", "#ddlRenewalTerm", function () {
    var term = $(this).val();
    var baseDate = $("#renew_old_end_date").val() ? new Date($("#renew_old_end_date").val()) : new Date();

    if (!term) return;

    var endDate = new Date(baseDate);
    if (term == "6 Months") endDate.setMonth(endDate.getMonth() + 6);
    else if (term == "1 Year") endDate.setFullYear(endDate.getFullYear() + 1);
    else if (term == "2 Years") endDate.setFullYear(endDate.getFullYear() + 2);

    $("#renew_start_date").val(baseDate.toISOString().split('T')[0]);
    $("#renew_end_date").val(endDate.toISOString().split('T')[0]);
});

//////$(document).on("click", "#btnConvertToContract", function () {
//////    var renewId = $(this).attr("data-id");

//////    if (!renewId) {
//////        Swal.fire({ icon: "warning", title: "No Approved Renewal", text: "Koi Approved renewal nahi mili." });
//////        return;
//////    }

//////    Swal.fire({
//////        title: "Convert to New Contract?",
//////        text: "Yeh renewal ko naya contract banayega. Continue?",
//////        icon: "question",
//////        showCancelButton: true
//////    }).then((r) => {
//////        if (!r.value) return;

//////        new APICALL(GetGlobalURL('Base', 'ConvertRenewalToContract') + '?renewId=' + renewId, 'POST', '', true)
//////            .FETCH((result, error) => {
//////                if (error) {
//////                    Swal.fire({ icon: 'error', title: 'Error', text: error.data.responseText });
//////                    return;
//////                }

//////                Swal.fire({
//////                    icon: "success",
//////                    title: "Contract Created",
//////                    text: "Naya contract successfully ban gaya."
//////                }).then(() => {
//////                    var newContractId = result.data.data[0].new_contract_id;
//////                    var clientId = result.data.data[0].client_id;
//////                    window.location.href = "/BillingRevenue/ClientOnboarding?ClientId=" + clientId + "&ContractId=" + newContractId + "&type=editfromgrid";
//////                });
//////            });
//////    });
//////});
