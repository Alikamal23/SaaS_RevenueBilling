// ********* DATE FORMAT ********************
function FormatDate(dateString) {
    if (!dateString)
        return "";

    var dt = new Date(dateString);

    if (isNaN(dt))
        return "";

    return ("0" + dt.getDate()).slice(-2) + "-" +
        ("0" + (dt.getMonth() + 1)).slice(-2) + "-" +
        dt.getFullYear();
}
function FormatMonthYear(dateString) {
    if (!dateString) return "";
    var dt = new Date(dateString);
    if (isNaN(dt)) return "";
    var months = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];
    return months[dt.getMonth()] + " " + dt.getFullYear();
}
function FormatMonthDay(dateString) {
    if (!dateString) return "";
    var dt = new Date(dateString);
    if (isNaN(dt)) return "";
    var months = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];
    return months[dt.getMonth()] + " " + dt.getDate();
}


// ********* AMOUNT FORMAT ********************
function FormatAmount(amount, showDecimals = true) {
    amount = amount?.toString().replace(/,/g, '');

    if (amount == null || amount === "" || isNaN(amount))
        return showDecimals ? "0.00" : "0";

    return Number(amount).toLocaleString("en-US", {
        minimumFractionDigits: showDecimals ? 2 : 0,
        maximumFractionDigits: showDecimals ? 2 : 0
    });
}
function UnFormatAmount(value) {
    if (!value) return 0;

    return value.toString().replace(/,/g, "");
}



// ********* NUMBER CONVERT TO WORDS ********************
function ConvertNumberToWords(num) {
    var ones = ['', 'One', 'Two', 'Three', 'Four', 'Five', 'Six', 'Seven', 'Eight', 'Nine', 'Ten', 'Eleven', 'Twelve', 'Thirteen', 'Fourteen', 'Fifteen', 'Sixteen', 'Seventeen', 'Eighteen', 'Nineteen'];
    var tens = ['', '', 'Twenty', 'Thirty', 'Forty', 'Fifty', 'Sixty', 'Seventy', 'Eighty', 'Ninety'];

    num = Math.floor(num);
    if (num === 0) return 'Zero';

    function convertLessThanOneThousand(n) {
        var str = '';
        if (n >= 100) {
            str += ones[Math.floor(n / 100)] + ' Hundred ';
            n %= 100;
        }
        if (n >= 20) {
            str += tens[Math.floor(n / 10)] + ' ';
            n %= 10;
        }
        if (n > 0) {
            str += ones[n] + ' ';
        }
        return str;
    }

    var words = '';
    var units = ['', 'Thousand', 'Million', 'Billion'];
    var unitIdx = 0;

    while (num > 0) {
        var rem = num % 1000;
        if (rem > 0) {
            words = convertLessThanOneThousand(rem) + units[unitIdx] + ' ' + words;
        }
        num = Math.floor(num / 1000);
        unitIdx++;
    }
    return words.trim() + ' Only';
}


// ********* VIEW, DOWNLOAD AND EMAIL BUTTONS CODE (GENERIC) ********************
function BuildInvoiceHtml(invoice, milestones) {
    milestones = milestones || [];

    var rows = "";
    var grandTotal = 0;

    //console.log(invoice);
    //console.log(milestones);

    // Currency ke mutabik variables set karna (PKR vs Foreign Currency)
    var currId = parseInt(invoice.currency_id || 0);
    var currCode = invoice.currency_code;
    var convRate = parseFloat(invoice.conversion_rate || 0);
    var isPKR = (currCode === "PKR");


    var grossAmt = parseFloat(invoice.gross_amount || 0);
    var discPercent = parseFloat(invoice.discount_percent || 0);
    var discAmt = parseFloat(grossAmt * discPercent / 100 || 0);

    var grossAftDisc = parseFloat(grossAmt - discAmt || 0);

    var taxPercent = parseFloat(invoice.tax_percent || 0);
    var taxAmt = parseFloat(grossAftDisc * taxPercent / 100 || 0);

    var NET_AMT = parseFloat(grossAftDisc + taxAmt || 0);

    // Agar USD hai to rate conversion_rate hoga, agar PKR hai to 1
    var rate = isPKR ? 1 : parseFloat(invoice.conversion_rate || 1);


    var temp = grossAmt.toLocaleString();
    if (currCode === "PKR") {
        temp = parseFloat(invoice.gross_amount || 0);
    } else {
        temp = parseFloat(invoice.invoice_amount || 0);
    }


    // "Line Total" column ke liye — YAHAN rate ek hi dafa multiply hoga:
    //var lineTotal = grossAmountCurr * rate;   // 600 × 290.23 = 174,139.74 (yeh sahi hai, PKR equivalent)
    var lineTotal = grossAmt;


    // total in PKR
    var discAmountPKR = parseFloat(discAmt);
    var subTotalPKR = grossAmt - discAmountPKR;   // NOTE: neeche bug bhi fix kar raha hoon
    var taxAmountPKR = parseFloat(subTotalPKR * taxPercent / 100);
    var finalAmountPKR = grossAmt - discAmountPKR + taxAmountPKR;

    // SaaS / No Milestone
    if (milestones.length === 0) {
        var qty = 1;

        rows += `
        <tr>
            <td>MG Link</td>
            <td class="inv-center">${temp.toLocaleString()}</td>
            <td class="inv-right">${rate}</td>
            <td class="inv-right">${lineTotal.toLocaleString()}</td>
        </tr>

        <tr>
            <td style="height: 35px; border: 1px solid #999;"></td>
            <td style="border: 1px solid #999;"></td>
            <td style="border: 1px solid #999;"></td>
            <td style="border: 1px solid #999;"></td>
        </tr>
        <tr>
            <td style="height: 35px; border: 1px solid #999;"></td>
            <td style="border: 1px solid #999;"></td>
            <td style="border: 1px solid #999;"></td>
            <td style="border: 1px solid #999;"></td>
        </tr>
        <tr>
            <td style="height: 35px; border: 1px solid #999;"></td>
            <td style="border: 1px solid #999;"></td>
            <td style="border: 1px solid #999;"></td>
            <td style="border: 1px solid #999;"></td>
        </tr>
        `;

        //grandTotal = finalAmount; // Final payable amount
        grandTotal = finalAmountPKR; //final amount in PKR after disc minus and Tax Plus
    }
    else {
        $.each(milestones, function (i, m) {
            //var amount = parseFloat(m.milestone_amount || 0);
            var amount = parseFloat(lineTotal || 0);

            rows += `
            <tr>
                <td>
                ${m.milestone_name}
                </td>

                <td class="inv-center">
                ${FormatDate(m.due_date)}
                </td>

                <td class="inv-right">
                ${amount.toLocaleString()}
                </td>
            </tr>`;

            grandTotal += amount;
        });


        // Total 6 rows dikhani hain
        var blankRows = 6 - milestones.length;

        if (blankRows < 0)
            blankRows = 0;

        for (var i = 0; i < blankRows; i++) {
            rows += `
                <tr>
                    <td style="height:35px;">&nbsp;</td>
                    <td></td>
                    <td></td>
                </tr>`;
        }


    }

    var amountInWords = ConvertNumberToWords(grandTotal);
    console.log(amountInWords);

    var currencyLabel = invoice.currency_code || "PKR";
    console.log(currencyLabel);

    //${invoice.client_name || ''}
    return `
        <div id="invoicePDF">

            <!-- ================= HEADER ================= -->
            <table class="inv-table inv-no-border inv-header-table">
                <tr>
                    <td colspan="2" style="padding:0; border:none;">
                        <img src="/Public/image/report_header.png"
                             class="inv-report-header">
                    </td>
                </tr>

                <tr>
                    <!-- CLIENT -->
                    <td class="inv-header-left">
                        <div class="inv-company-name">
                            
                        </div>
                    </td>

                    <!-- INVOICE INFO -->
                    <td class="inv-header-right">

                        <div class="inv-title">
                            SALES TAX INVOICE
                        </div>

                        <table class="inv-table inv-info">
                            <tr>
                                <td>Date</td>
                                <td>${FormatDate(invoice.invoice_date)}</td>
                            </tr>

                            <tr>
                                <td>Invoice #</td>
                                <td>${invoice.invoice_no || '-'}</td>
                            </tr>

                            <tr>
                                <td>PO #</td>
                                <td>${invoice.po_no || '-'}</td>
                            </tr>

                            <tr>
                                <td>Due Date</td>
                                <td>${FormatDate(invoice.due_date)}</td>
                            </tr>
                        </table>

                    </td>
                </tr>
            </table>


            <!-- ================= BILL TO / CONTRACT ================= -->
            <table class="inv-details">
                <tr>

                    <!-- BILL TO -->
                    <td class="inv-box bill-to-box">

                        <div class="inv-box-title">
                            BILL TO
                        </div>

                        <div class="inv-box-body">

                            <div class="inv-client-name">
                                ${invoice.client_name || '-'}
                            </div>

                            <div class="inv-client-details">

                                <div class="inv-detail-row">
                                    <span class="inv-detail-label">NTN</span>
                                    <span>${invoice.tax_registration_no || '-'}</span>
                                </div>

                                <div class="inv-detail-row">
                                    <span class="inv-detail-label">Address</span>
                                    <span>${invoice.billing_address || '-'}</span>
                                </div>

                                <div class="inv-detail-row">
                                    <span class="inv-detail-label">Contact</span>
                                    <span>${invoice.primary_contact_name || '-'}</span>
                                </div>

                                <div class="inv-detail-row">
                                    <span class="inv-detail-label">Email</span>
                                    <span>${invoice.primary_contact_email || '-'}</span>
                                </div>

                                <div class="inv-detail-row">
                                    <span class="inv-detail-label">Phone</span>
                                    <span>${invoice.primary_contact_phone || '-'}</span>
                                </div>

                            </div>

                        </div>

                    </td>


                    <!-- CONTRACT DETAILS -->
                    <td class="inv-box contract-box">

                        <div class="inv-box-title">
                            CONTRACT DETAILS
                        </div>

                        <div class="inv-box-body">

                            <div class="inv-client-details">

                                <div class="inv-detail-row">
                                    <span class="inv-detail-label">Contract Ref</span>
                                    <span>${invoice.contract_refno || '-'}</span>
                                </div>

                                <div class="inv-detail-row">
                                    <span class="inv-detail-label">Project</span>
                                    <span>${invoice.project_name || '-'}</span>
                                </div>

                                <div class="inv-detail-row">
                                    <span class="inv-detail-label">Contract Type</span>
                                    <span>${invoice.contract_type || '-'}</span>
                                </div>

                            </div>

                        </div>

                    </td>

                </tr>
            </table>


            <!-- ================= ITEMS ================= -->
            <div class="inv-items-wrapper">

                <table class="inv-table inv-items">

                    <thead>

                        ${milestones.length === 0
            ?
            `
                            <tr>
                                <th class="desc-col">
                                    Description
                                </th>

                                <th class="qty-col">
                                    ${invoice.currency_code}/${invoice.frequencyname}
                                </th>

                                <th class="rate-col">
                                    ${invoice.currency_code} Rate
                                </th>

                                <th class="amount-col">
                                    Line Total
                                </th>
                            </tr>
                            `
            :
            `
                            <tr>
                                <th style="width:60%">
                                    Milestone
                                </th>

                                <th style="width:20%">
                                    Due Date
                                </th>

                                <th style="width:20%">
                                    Amount
                                </th>
                            </tr>
                            `
        }

                    </thead>

                    <tbody>
                        ${rows}
                    </tbody>

                </table>

            </div>


            <!-- ================= NOTES + SUMMARY ================= -->
            <table class="inv-bottom-row">

                <tr>

                    <!-- NOTES -->
                    <td class="inv-notes-cell">

                        <div class="inv-notes">

                            <div class="inv-notes-title">
                                IMPORTANT NOTES
                            </div>

                            <div class="inv-notes-body">

                                <strong>
                                    Mettis Global (Pvt.) Ltd.
                                </strong><br>

                                1-19-20311-714-131716<br>

                                Habib Metropolitan Bank Ltd.<br>

                                PK59MPBL0119027140131716<br>

                                NTN : 7114843-1

                            </div>

                        </div>

                    </td>


                    <!-- SUMMARY -->
                    <td class="inv-summary-cell">

                        <table class="inv-summary-box">

                            <tr>
                                <td class="label">
                                    Sub Total
                                </td>

                                <td class="amount">
                                    ${lineTotal.toLocaleString()}
                                </td>
                            </tr>

                            <tr>
                                <td class="label">
                                    Discount (${discPercent}%)
                                </td>

                                <td class="amount">
                                    ${discAmountPKR.toLocaleString()}
                                </td>
                            </tr>

                            <tr>
                                <td class="label">
                                    Sales Tax (${taxPercent}%)
                                </td>

                                <td class="amount">
                                    ${taxAmountPKR.toLocaleString()}
                                </td>
                            </tr>

                            <tr class="grand">

                                <td>
                                    TOTAL (PKR)
                                </td>

                                <td class="amount">
                                    ${finalAmountPKR.toLocaleString()}
                                </td>

                            </tr>

                        </table>

                    </td>

                </tr>

            </table>


            <!-- ================= FOOTER NOTE ================= -->

            <div class="inv-footer-note">
                Kindly deduct withholding tax @ 4% of the invoice amount
                and provide the tax challan within 15 days.
            </div>


            <!-- ================= FOOTER IMAGE ================= -->

            <div class="inv-footer-image">

                <img src="/Public/image/report_footer.png"
                     class="footer-img">

            </div>

        </div>
    `;
}
function ViewInvoice(invoiceId) {
    ShowInvoiceLoader();   // ✅ replace

    new APICALL(GetGlobalURL('Base', 'FillInvoiceForPrint') + "?invoiceId=" + invoiceId, "GET", "", false)
        .FETCH((result, error) => {

            if (error) {
                HideInvoiceLoader();   // ✅ replace
                Swal.fire({
                    icon: "error",
                    title: "Error",
                    text: error.data ? error.data.responseText : "Something went wrong"
                });
                return;
            }

            if (result && result.data) {
                var invoice = result.data.Table[0];
                var milestones = result.data.Table1 || [];

                var html = BuildInvoiceHtml(invoice, milestones);

                GenerateInvoicePdf(html, invoice.invoice_no, false);
            } else {
                HideInvoiceLoader();   // ✅ replace
            }

        });

}
function DownloadInvoice(invoiceId) {
    ShowInvoiceLoader();

    new APICALL(GetGlobalURL('Base', 'FillInvoiceForPrint') + "?invoiceId=" + invoiceId, "GET", "", false)
        .FETCH((result, error) => {
            if (error) {
                HideInvoiceLoader();
                Swal.fire({
                    icon: "error",
                    title: "Error",
                    text: error.data ? error.data.responseText : "Something went wrong"
                });
                return;
            }

            if (result && result.data) {
                var invoice = result.data.Table[0];
                var milestones = result.data.Table1 || [];

                var html = BuildInvoiceHtml(invoice, milestones);

                GenerateInvoicePdf(html, invoice.invoice_no, true);

            } else {
                HideInvoiceLoader();
            }

        });

}
function EmailInvoice(invoiceId) {
    Swal.fire({
        title: "Send Invoice?",
        text: "Do you want to send this invoice via email?",
        icon: "question",
        showCancelButton: true
    }).then((r) => {
        if (r.value) {
            ShowInvoiceLoader();

            // 1. Pehle Invoice ka Data fetch karein takki HTML/PDF ban sake
            new APICALL(GetGlobalURL('Base', 'FillInvoiceForPrint') + "?invoiceId=" + invoiceId, "GET", "", false)
                .FETCH((result, error) => {
                    if (error) {
                        HideInvoiceLoader();
                        Swal.fire({ icon: "error", title: "Error", text: error.data.responseText });
                        return;
                    }

                    if (result && result.data) {
                        var invoice = result.data.Table[0];
                        var milestones = result.data.Table1 || [];
                        var html = BuildInvoiceHtml(invoice, milestones);

                        //// 2. PDF Blob generate karein (Yahan aapko apni library ke mutabik convert karna hoga)
                        var element = document.createElement("div");
                        element.style.position = "fixed";
                        element.style.left = "-9999px";
                        element.style.top = "0";
                        element.style.width = "894px";
                        element.style.background = "#fff";
                        element.style.boxSizing = "border-box";
                        element.innerHTML = html;

                        document.body.appendChild(element);

                        setTimeout(function () {

                            html2canvas(element, {
                                scale: 2,
                                useCORS: true,
                                logging: false
                            }).then(function (canvas) {
                                //console.log(canvas.width);
                                //console.log(canvas.height);

                                var imgData = canvas.toDataURL("image/png");

                                var PDFClass = typeof jsPDF !== 'undefined'
                                    ? jsPDF
                                    : (window.jspdf && window.jspdf.jsPDF ? window.jspdf.jsPDF : null);

                                var pdf = new PDFClass("p", "mm", "a4");

                                var pageWidth = pdf.internal.pageSize.width;
                                var pageHeight = pdf.internal.pageSize.height;

                                //console.log("Canvas:", canvas.width, canvas.height);
                                //console.log("PDF:", pageWidth, pageHeight);

                                //console.log(pageWidth);
                                //console.log(pageHeight);

                                pdf.addImage(imgData, "PNG", 0, 0, pageWidth, pageHeight);

                                var pdfBlob = pdf.output("blob");

                                var formData = new FormData();
                                formData.append("invoiceId", invoiceId);
                                formData.append("pdfFile", pdfBlob, "Invoice_" + invoice.invoice_no + ".pdf");

                                $.ajax({
                                    url: GetGlobalURL('Base', 'EmailInvoice'),
                                    type: "POST",
                                    data: formData,
                                    processData: false,
                                    contentType: false,
                                    success: function () {
                                        document.body.removeChild(element);
                                        HideInvoiceLoader();   // ✅ replace

                                        Swal.fire(
                                            "Success",
                                            "Invoice emailed successfully.",
                                            "success"
                                        );
                                    },
                                    error: function () {
                                        document.body.removeChild(element);
                                        HideInvoiceLoader();   // ✅ replace

                                        Swal.fire(
                                            "Error",
                                            "Unable to send email.",
                                            "error"
                                        );
                                    }
                                });

                            });

                        }, 1000);



                    } else {
                        HideInvoiceLoader();   // ✅ replace
                    }
                });
        }
    });
}
function GenerateInvoicePdf(html, fileName, isDownload) {
    var element = document.createElement("div");

    element.style.position = "fixed";
    element.style.left = "-9999px";
    element.style.top = "0";
    element.style.width = "894px"; // A4 width
    element.style.background = "#fff";
    element.style.boxSizing = "border-box";
    element.innerHTML = html;

    document.body.appendChild(element);

    // Browser ko UI paint karne ka waqt dene ke liye chota sa delay
    setTimeout(function () {
        html2canvas(element, {
            scale: 2,
            useCORS: true,
            logging: false
        }).then(function (canvas) {

            var imgData = canvas.toDataURL("image/png");

            var PDFClass = typeof jsPDF !== 'undefined'
                ? jsPDF
                : (window.jspdf && window.jspdf.jsPDF ? window.jspdf.jsPDF : null);

            if (!PDFClass) {
                console.error("jsPDF library not found on page.");
                document.body.removeChild(element);
                HideInvoiceLoader();   // ✅ replace
                return;
            }

            var pdf = new PDFClass("p", "mm", "a4");

            var pageWidth = pdf.internal.pageSize.getWidth
                ? pdf.internal.pageSize.getWidth()
                : pdf.internal.pageSize.width;

            var pageHeight = (canvas.height * pageWidth) / canvas.width;

            pdf.addImage(imgData, "PNG", 0, 0, pageWidth, pageHeight);

            if (isDownload) {
                pdf.save(fileName + ".pdf");
            } else {
                var blob = pdf.output("blob");
                var url = URL.createObjectURL(blob);
                window.open(url, "_blank");
            }

            document.body.removeChild(element);

            // Ab PDF process ho gayi hai, toh loader band karein
            //HideLoader("body");
            HideInvoiceLoader();   // ✅ replace

        }).catch(function (err) {
            console.error("PDF generation failed:", err);
            if (document.body.contains(element)) {
                document.body.removeChild(element);
            }

            // Error par bhi loader hide hona zaroori hai
            //HideLoader("body");
            HideInvoiceLoader();   // ✅ replace

        });
    }, 100);
}

// ************* ++++++++++++++++ end of View, Download & Email common functions ************ +++++++++++++++++++++


// ********* GENERIC INVOICE LOADER (self-contained, koi page-specific div ki zaroorat nahi) ********************
function ShowInvoiceLoader() {
    if ($("#genericInvoiceLoader").length === 0) {
        $("body").append(`
            <div id="genericInvoiceLoader" style="
                position: fixed;
                inset: 0;
                background: rgba(255,255,255,0.65);
                backdrop-filter: blur(4px);
                z-index: 999999;
                display: flex;
                align-items: center;
                justify-content: center;
                flex-direction: column;
            ">
                <div class="spinner-border text-primary" style="width:3.5rem;height:3.5rem;" role="status"></div>
                <h6 class="mt-3 text-dark">Processing Invoice...</h6>
            </div>
        `);
    } else {
        $("#genericInvoiceLoader").show();
    }
}

function HideInvoiceLoader() {
    $("#genericInvoiceLoader").fadeOut(150, function () {
        $(this).remove();
    });
}
