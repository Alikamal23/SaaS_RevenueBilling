var voucherList = {};
var voucherDetailList = {};
var Clientmanagement = {};
let totalDrAmount = 0;
let totalCrAmount = 0;
var existingSerialNo = 1;
var sno = 1;

$(document).ready(function () {
    ViewData();

    GetVoucherTypeDDL("2"); //Journal Voucher

    //var ddl = $("#ddlCrHead");
    //GetHeadDDL('1', ddl, 'Head');

    GetJobDDL();

    var date = new Date();
    var currentDate = date.toISOString().slice(0, 10);
    setDateFields(currentDate);

    $('#ddlVType').change(function () {
        var selectedValue = $(this).val();

        GetMaxVoucherID(selectedValue, function (id) {
            $("#txtVCode").val(id);   // Set returned value
        });
    });

    var ddl = $("#ddlHead");
    GetHeadDDL('-1', ddl, 'Head');


    // Bind radio button change
    $('input[name="headType"]').on('change', function () {
        var selectedValue = $('input[name="headType"]:checked').val(); // gets Head, Party (Cust/Supp) or Employee
        GetHeadDDL('0', ddl, selectedValue);
    });

    // Bind change event to update hidden field
    $("#ddlCrHead").off('change').on('change', function () {
        var selectedOption = $(this).find(':selected');
        var category = selectedOption.data('category');
        $('#txtMasterCategoryId').val(category || '');
    });

    //var appSetting = {
    //    defaultCustomerCategoryId: '1',
    //    defaultSupplierCategoryId: '2'
    //};

    // Bind change event to update hidden field
    $("#ddlHead").off('change').on('change', function () {
        var selectedOption = $(this).find(':selected');
        var selectedHeadType = $('input[name="headType"]:checked').val(); // e.g., "Head", "Vendor", "Donor"
        var category = '';

        category = selectedOption.data('category') || '';

        //if (selectedHeadType === 'Head') {
        //    category = selectedOption.data('category') || '';
        //} else if (selectedHeadType === 'Party') {
        //    category = appSetting.defaultCustomerCategoryId;
        //} else if (selectedHeadType === 'Employee') {
        //    category = appSetting.defaultSupplierCategoryId;
        //}

        $('#txtDetailCategoryId').val(category);
    });


    $("#masterform").validate();

    $('#SaveBtn').on('click', function () {
        SaveData();
    });

    $('#UpdateBtn').on('click', function () {
        UpdateData();
    });

    $('#ClearBtn').on('click', function () {
        DoEmptyFields();

        CloseModal('myModal');
    });

    $('#btnAddNew').on('click', function () {
        $('#myModalLabel').html('Add Journal Voucher');

        DoEmptyFields();

        // Footer buttons
        $('#SaveBtn').removeClass('d-none');    // Show Save
        $('#UpdateBtn').addClass('d-none');     // Hide Update
        $('#ClearBtn').removeClass('d-none');   // Show Cancel

        OpenModal('myModal');
    });

    // Add in Grid..    
    $('#btnAddAB').on('click', function () {
        AddToGrid();
    });

    $(document).on('click', '#DetailSectionID .btnRemoveRow', function () {
        var row = $(this).closest('tr');
        row.remove();

        // Recalculate total
        totalDrAmount = 0;
        totalCrAmount = 0;
        $("#DetailSectionID tbody tr").each(function () {
            var amountStrDR = $(this).find("td").eq(9).text().replace(/,/g, '');
            var amount_dr = parseFloat(amountStrDR) || 0;

            var amountStrCR = $(this).find("td").eq(10).text().replace(/,/g, '');
            var amount_cr = parseFloat(amountStrCR) || 0;

            totalDrAmount += amount_dr;
            totalCrAmount += amount_cr;
        });

        $("#txtTotalDrAmount").val(totalDrAmount);
        $("#txtTotalCrAmount").val(totalCrAmount);

        // Update serial numbers
        $("#DetailSectionID tbody tr").each(function (i, tr) {
            $(tr).find("td").eq(0).text(i + 1);
        });

        sno = $("#DetailSectionID tbody tr").length + 1;
    });

});


/* View GRID */
function ViewData() {
    ShowLoader('UserMasterDiv');
    UTILITY.CheckSession((data_) => {
        if (data_) {
            new APICALL(GetGlobalURL('Base', 'GetVoucherMaster?Nature=2'), 'GET', '', true).FETCH((result, error) => {
                if (result) {
                    $("#user-master").dataTable().fnDestroy();
                    $('#user-master tbody').empty();

                    //console.log(result);

                    if (result.data != null) {
                        voucherList = result.data.Master;        // Master list
                        voucherDetailList = result.data.Detail;  // Detail list

                        // --------- Attach Details to each Master ---------
                        voucherList.forEach(master => {
                            master.DetailList = voucherDetailList.filter(d => d.FK_VoucherID === master.Id);
                        });

                        // --------- Bind Master to Table ----------
                        $.each(voucherList, function (i, option) {

                            var UpdateBtn = '<td><button class="avatar-text avatar-md EditData" ' +
                                'data-value="' + option.Id + '" data-value1="' + i + '">' +
                                '<i class="feather-edit-2 edit-icon"></i></button></td>';

                            var DeleteBtn = '<td><button class="avatar-text avatar-md DeleteData" ' +
                                'data-value="' + option.Id + '">' +
                                '<i class="feather-trash delete-icon"></i></button></td>';

                            $('#user-master tbody').append(
                                `<tr id="rowid-${i}">
                                    <td>${option.VType}</td>
                                    <td>${option.VCode}</td>
                                    <td>${formatDate(option.VDate)}</td>
                                    <td>${option.HeadName}</td>
                                    <td>${option.Amount}</td>
                                    <td>${option.HeadName}</td>
                                    <td>${option.Amount}</td>
                                    <td>${option.Narration}</td>
                                    ${UpdateBtn}
                                    ${DeleteBtn}
                                </tr>`
                            );
                        });

                        // attach events
                        $('.EditData').on('click', function () {
                            EditData(
                                this.getAttribute("data-value"),
                                this.getAttribute("data-value1")
                            );
                        });

                        $('.DeleteData').on('click', function () {
                            DeleteData(this.getAttribute("data-value"));
                        });

                    }
                    $('#user-master').DataTable();
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

/* CRUD Operations */
function SaveData() {
    var detailSections = new Array();

    var res = ValidateAll();
    if (res == false) {
        return false;
    }

    Swal.fire({
        title: 'Do you want to save the changes?',
        showDenyButton: true,
        showCancelButton: false,
        confirmButtonText: 'Ok',
        denyButtonText: 'Cancel',
    }).then((result) => {
        if (result.isConfirmed) {
            ShowLoader('UserMasterDiv');

            $("#DetailSectionID tbody tr").each(function () {
                var row = $(this);
                var detailSection = {};

                detailSection.VType = $("#ddlVType").val();
                detailSection.VCode = parseInt($("#txtVCode").val());
                detailSection.VNo = '';
                detailSection.CategoryCode = parseInt(row.find("td").eq(1).html());
                detailSection.HeadCode = parseInt(row.find("td").eq(2).html());
                detailSection.Remarks = row.find("td").eq(4).html();
                detailSection.HeadType = row.find("td").eq(5).html();
                detailSection.JobNo = row.find("td").eq(6).html();
                detailSection.BillNo = '';
                detailSection.ChqNo = row.find("td").eq(7).html();
                detailSection.ChqDate = FixDate(row.find("td").eq(8).text().trim());
                detailSection.ClearDate = null;
                detailSection.ReturnDate = null;
                detailSection.DebitAmount = parseFloat(row.find("td").eq(9).html().replace(/,/g, '')); 
                detailSection.CreditAmount = parseFloat(row.find("td").eq(10).html().replace(/,/g, '')); 
                detailSections.push(detailSection);
            });

            if ($("#ddlVType").val() == 'JV') {
                cashType = '-';
            } else {
                cashType = '-';
            }

            var amountStrDr = $("#txtTotalDrAmount").val().replace(/,/g, ''); // Remove commas
            var amount_dr = parseFloat(amountStrDr) || 0;

            var amountStrCr = $("#txtTotalCrAmount").val().replace(/,/g, ''); // Remove commas
            var amount_cr = parseFloat(amountStrCr) || 0;

            var data = {
                Id: parseInt($("#PK_HiddenID").val() == "" ? 0 : $("#PK_HiddenID").val()),
                VType: $("#ddlVType").val(),
                VCode: parseInt($("#txtVCode").val()),
                VDate: $("#txtVDate").val(),
                VNo: '',
                Folio: 'JV',  //for Journal Voucher
                ModeOfPayment: '',
                BankName: '',
                Chq_PO_Draft_No: '',
                CashType: cashType,
                Narration: $("#txtNarration").val(),
                DCCategory: 0,
                DCCode: 0,
                Amount: amount_dr,
                DetailSection: detailSections,
                FK_InstanceID: 0,
                IsActive: $('#IsActive').is(':checked'),
                CreatedBy: 'superadmin',
                BranchId: 1
            }

            new APICALL(GetGlobalURL('Base', 'SaveVoucher'), 'POST', JSON.stringify(data), true, false, 'application/json; charset=utf-8')
                .FETCH((result, error) => {

                    if (result && result.status === 'success') {
                        CloseModal('myModal');

                        ViewData();

                        DoEmptyFields();
                        Swal.fire({
                            icon: 'success',
                            title: 'Success...',
                            text: 'Saved Successfully!'
                        });
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
function EditData(ID, index) {
    $('#myModalLabel').html('Edit Journal Voucher');

    DoEmptyFields();
    var row = voucherList[index];

    $('#EditID').val(ID);
    $('#txtPrimaryField').val(ID);

    $('#txtVtype').val(row.VType);
    $('#txtVcode').val(row.VCode);


    // Master
    $('#ddlVType').val(row.VType);
    $('#txtVCode').val(row.VCode);
    $('#txtVDate').val(FixDate(row.VDate));

    //$('#ddlCrHead').val(row.DCCode);
    //$('#txtMasterCategoryId').val(row.DCCategory);

    $('#txtTotalDrAmount').val(row.Amount);
    $('#txtTotalCrAmount').val(row.Amount);

    $('#txtNarration').val(row.Narration);

    // -------- DETAIL FIELDS --------
    var tbody = $("#DetailSectionID tbody");
    tbody.empty();

    // Fill each detail row
    var snoLocal = 1;
    var totalDrAmountLocal = 0;
    var totalCrAmountLocal = 0;
    if (row.DetailList && row.DetailList.length > 0) {
        row.DetailList.forEach(d => {
            var headName = d.HeadName || '';
            var jobNo = d.JobNo || '';
            var chqDate = d.ChqDate ? FixDate(d.ChqDate) : '';

            var debitAmount = parseFloat(d.DebitAmount) || 0;
            totalDrAmountLocal += debitAmount;

            var creditAmount = parseFloat(d.CreditAmount) || 0;
            totalCrAmountLocal += creditAmount;

            var headType = d.HeadType || 'Head';

            var removeButton = "<button type='button' class='btn btn-sm btn-danger btnRemoveRow'>" +
                "<i class='fa fa-minus-square'></i></button>";

            var tr = `<tr>
                <td class='text-center'>${snoLocal}</td>
                <td class='text-center' hidden>${d.CategoryCode}</td>
                <td class='text-center'>${d.HeadCode}</td>
                <td class='text-center'>${headName}</td>
                <td class='text-center'>${d.Remarks}</td>
                <td class='text-center'>${headType}</td>
                <td class='text-center'>${jobNo}</td>
                <td class='text-center'>${d.ChqNo || ''}</td>
                <td class='text-center'>${chqDate}</td>
                <td class='text-center'>${debitAmount}</td>
                <td class='text-center'>${creditAmount}</td>
                <td>
                    ${removeButton}
                </td>
            </tr>`;

            tbody.append(tr);
            snoLocal++;
        });
    }

    // Update global totals
    totalDrAmount = totalDrAmountLocal;
    totalCrAmount = totalCrAmountLocal;
    sno = snoLocal;
    $("#txtTotalDrAmount").val(totalDrAmount);
    $("#txtTotalCrAmount").val(totalCrAmount);


    // -------- SWITCH BUTTONS --------
    // Footer buttons
    $('#SaveBtn').addClass('d-none');      // Hide Save
    $('#UpdateBtn').removeClass('d-none'); // Show Update
    $('#ClearBtn').removeClass('d-none');  // Show Cancel


    // -------- OPEN MODAL --------
    OpenModal('myModal');

}
function UpdateData() {
    var detailSections = new Array();

    var res = ValidateAll();
    if (res == false) {
        return false;
    }

    Swal.fire({
        title: 'Do you want to update the voucher?',
        showDenyButton: true,
        showCancelButton: false,
        confirmButtonText: 'Ok',
        denyButtonText: 'Cancel',
    }).then((result) => {
        if (result.isConfirmed) {
            ShowLoader('UserMasterDiv');

            // ----------------------
            // BUILD DETAIL ARRAY
            // ----------------------
            $("#DetailSectionID tbody tr").each(function () {
                var row = $(this);
                var detailSection = {};

                detailSection.VType = $("#ddlVType").val();
                detailSection.VCode = parseInt($("#txtVCode").val());
                detailSection.VNo = '';
                detailSection.CategoryCode = parseInt(row.find("td").eq(1).html());
                detailSection.HeadCode = parseInt(row.find("td").eq(2).html());
                detailSection.Remarks = row.find("td").eq(4).html();
                detailSection.HeadType = row.find("td").eq(5).html();
                detailSection.JobNo = row.find("td").eq(6).html();
                detailSection.BillNo = '';
                detailSection.ChqNo = row.find("td").eq(7).html();
                detailSection.ChqDate = FixDate(row.find("td").eq(8).text().trim());
                detailSection.ClearDate = null;
                detailSection.ReturnDate = null;
                detailSection.DebitAmount = parseFloat(row.find("td").eq(9).html().replace(/,/g, ''));
                detailSection.CreditAmount = 0;

                detailSections.push(detailSection);
            });

            // Cash / Bank logic
            var cashType = ($("#ddlVType").val() == 'JV') ? '' : '';

            var amountStrDr = $("#txtTotalDrAmount").val().replace(/,/g, '');
            var amount_dr = parseFloat(amountStrDr) || 0;

            var amountStrCr = $("#txtTotalCrAmount").val().replace(/,/g, '');
            var amount_cr = parseFloat(amountStrCr) || 0;

            // -----------------------------------------
            // BUILD MAIN UPDATE OBJECT
            // -----------------------------------------
            var data = {
                Id: parseInt($("#PK_HiddenID").val()),      // UPDATE means Id always > 0
                VType: $("#ddlVType").val(),
                VCode: parseInt($("#txtVCode").val()),
                VDate: $("#txtVDate").val(),
                VNo: '',
                Folio: '',
                ModeOfPayment: cashType,
                BankName: '',
                Chq_PO_Draft_No: '',
                CashType: cashType,
                Narration: $("#txtNarration").val(),
                DCCategory: 0,
                DCCode: 0,
                Amount: amount_dr,
                DetailSection: detailSections,
                FK_InstanceID: 0,
                IsActive: $('#IsActive').is(':checked'),
                UpdatedBy: 'superadmin',
                BranchId: 1
            };

            // -----------------------------------------
            // FINAL API CALL — SAME AS SAVE
            // BUT BACKEND pe SP voucher UPDATE karega
            // and DETAIL DELETE + INSERT
            // -----------------------------------------
            new APICALL(GetGlobalURL('Base', 'SaveVoucher'), 'POST', JSON.stringify(data), true, false, 'application/json; charset=utf-8')
                .FETCH((result, error) => {

                    if (result && result.status === 'success') {
                        CloseModal('myModal');
                        ViewData();
                        DoEmptyFields();

                        Swal.fire({
                            icon: 'success',
                            title: 'Updated!',
                            text: 'Voucher Updated Successfully!'
                        });
                        return;
                    }

                    // ERROR
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
function DeleteData(ID) {
    var DeleteData = JSON.stringify({ VoucherId: ID });

    Swal.fire({
        title: 'Do you want to save the changes?',
        showDenyButton: true,
        showCancelButton: false,
        confirmButtonText: 'Ok',
        denyButtonText: 'Cancel',
    }).then((result) => {
        if (result.isConfirmed) {
            ShowLoader('UserMasterDiv');

            new APICALL(GetGlobalURL('Base', 'DeleteVoucher'), 'POST', DeleteData, true).FETCH((result, error) => {
                var isDeleted = result.data;

                if (isDeleted) {
                    ViewData();

                    Swal.fire({
                        icon: 'success',
                        title: 'Deleted!',
                        text: 'Record deleted successfully.'
                    });
                    return;
                }

                // ERROR
                if (error && error.status && error.status !== 200) {
                    Swal.fire({
                        icon: 'error',
                        title: 'Error...',
                        text: error.data?.responseText || 'Something went wrong while deleting.'
                    });
                    return;
                }

            });
        }
    });
}

// Detail Section Add to Grid ....
function validateDetailSection() {
    var isValid = true;

    if ($('#ddlHead').val().trim() == "-1") {
        $('#ddlHead').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#ddlHead').css('border-color', 'lightgrey');
    }

    if ($('#txtRemarks').val().trim() == "") {
        $('#txtRemarks').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#txtRemarks').css('border-color', 'lightgrey');
    }

    if ($('#txtDrAmount').val().trim() == "" && $('#txtCrAmount').val().trim() == "") {
        $('#txtDrAmount').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#txtDrAmount').css('border-color', 'lightgrey');
    }

    return isValid;
}
function AddToGrid() {
    var res = validateDetailSection();
    if (res == false) {
        return false;
    }

    // First check if a <tbody> tag exists, add one if not
    if ($("#DetailSectionID tbody").length == 0) {
        $("#DetailSectionID").append("<tbody></tbody>");
    }


    var ddlJobValue = $("#ddlJob").val();                 // value e.g. "0", "101", "202"
    var ddlJobText = $("#ddlJob option:selected").text(); // text e.g. "Please select", "Job-101"

    var txtCategory = $("#txtDetailCategoryId").val();
    var headCode = $("#ddlHead").val();
    var ddlHead = $("#ddlHead option:selected").text();
    var txtRemarks = $("#txtRemarks").val();
    var optHeadType = $('input[name="headType"]:checked').val();

    // If value = 0 → Empty save karo
    ddlJobText = (ddlJobValue === "0") ? "" : ddlJobText;

    var txtChqNo = $("#txtChqNo").val();
    var txtChqDate = $("#txtChqDate").val();
    txtChqDate = txtChqDate ? txtChqDate : ''; // Display empty if empty
    var txtDrAmount = parseFloat($("#txtDrAmount").val()) || 0;
    var txtCrAmount = parseFloat($("#txtCrAmount").val()) || 0;

    var removeButton = "<button type='button' class='btn btn-sm btn-danger btnRemoveRow'>" +
        "<i class='fa fa-minus-square'></i></button>";

    $("#DetailSectionID tbody").append("<tr>" +
        "<td class='text-center'>" + sno + "</td>" +
        "<td class='text-center' hidden>" + txtCategory + "</td>" +
        "<td class='text-center'>" + headCode + "</td>" +
        "<td class='text-center'>" + ddlHead + "</td>" +
        "<td class='text-center'>" + txtRemarks + "</td>" +
        "<td class='text-center'>" + optHeadType + "</td>" +
        "<td class='text-center'>" + ddlJobText + "</td>" +
        "<td class='text-center'>" + txtChqNo + "</td>" +
        "<td class='text-center'>" + txtChqDate + "</td>" +
        "<td class='text-center'>" + txtDrAmount + "</td>" +
        "<td class='text-center'>" + txtCrAmount + "</td>" +
        "<td>" + removeButton + "</td>" +
        "</tr>");

    // Increment the serial number for the next entry
    sno++;
    totalDrAmount += txtDrAmount;
    $("#txtTotalDrAmount").val(totalDrAmount);

    totalCrAmount += txtCrAmount;
    $("#txtTotalCrAmount").val(totalCrAmount);


    //-------------
    $("#ddlHead").val(null);
    $("#txtRemarks").val(null);
    $("#txtChqNo").val(null);
    $("#txtChqDate").val(null);
    $("#txtDrAmount").val(null);
}
function removeButton(ctl) {
    //$(ctl).parents("tr").remove();

    var row = $(ctl).closest("tr");
    row.remove();

    // Recalculate total from scratch
    totalAmount = 0;
    $("#DetailSectionID tbody tr").each(function () {
        var amountStr = $(this).find("td").eq(8).text().replace(/,/g, '');
        var amount = parseFloat(amountStr) || 0;
        totalAmount += amount;
    });

    $("#txtTotalDrAmount").val(totalAmount.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 }));

    // Update serial numbers
    $("#DetailSectionID tbody tr").each(function (i, tr) {
        $(tr).find("td").eq(0).text(i + 1);
    });

    // Update sno
    sno = $("#DetailSectionID tbody tr").length + 1;

}
function getVoucherDetailsInEdit(vtype, vcode) {
    new APICALL(
        GetGlobalURL('Account', 'GetVoucherDetailsInEdit/' + vtype + '/' + vcode), 'GET', '', true).FETCH((result, error) => {

            if (result) {
                var varData = result.data;
                if (!varData || varData.length === 0) return;

                // MASTER FIELDS
                $("#txtPrimaryField").val(varData[0].Id);
                $("#txtVtype").val(varData[0].VType);
                $("#txtVcode").val(varData[0].VCode);

                $("#ddlVType").val(varData[0].VType);
                $("#txtVCode").val(varData[0].VCode);

                // DATE
                var fullDate = new Date(varData[0].VDate);
                var month = ("0" + (fullDate.getMonth() + 1)).slice(-2);
                var day = ("0" + fullDate.getDate()).slice(-2);
                var formattedDate = fullDate.getFullYear() + "-" + month + "-" + day;
                $("#txtVDate").val(formattedDate);

                // OTHER FIELDS
                $("#txtMasterCategoryId").val(varData[0].DCCategory);
                $("#ddlCrHead").val(varData[0].DCCode).change();
                $("#txtTotalDrAmount").val(varData[0].Amount);
                $("#txtNarration").val(varData[0].Narration);

                // DETAILS GRID
                populateDetailGrid(varData || []);
            }

            if (error) {
                Swal.fire({
                    icon: 'error',
                    title: 'Error...',
                    text: error.data?.responseText || 'API Error',
                    footer: ''
                });
            }

        });
}
function populateDetailGrid(details) {
    //console.log("populateDetailGrid....");
    //console.log(details);

    var tbody = $('#DetailSectionID tbody');
    tbody.empty();  // Clear existing rows

    if (mode === 'edit') {
        if (!details || details.length === 0) {
            // Optional: show a 'No data' message
            var emptyRow = '<tr><td colspan="5" class="text-center">No Voucher history available</td></tr>';
            tbody.append(emptyRow);
            return;
        }
    }

    // Populate grid with data
    $.each(details, function (index, detail) {
        //var removeButton = "<button type='button' class='btn btn-sm' style='background: #eb1d24;' onclick='deleteRow(this,'"+detail.id+"');'><i class='fa fa-minus-square' style='font-size: 20px; color: white;' aria-hidden='true'></i></button>";
        var removeButton = "<button type='button' class='btn btn-sm' style='background: #eb1d24;' onclick='removeButton(this);'><i class='fa fa-minus-square' style='font-size: 20px; color: white;' aria-hidden='true'></i></button>";
        var row = '<tr>' +
            '<td>' + (existingSerialNo) + '</td>' +
            '<td style="display: none;">' + (detail.CategoryCode || '') + '</td>' +
            '<td>' + (detail.HeadCode || '') + '</td>' +
            '<td>' + (detail.HeadName || '') + '</td>' +
            '<td>' + (detail.Remarks || '') + '</td>' +
            '<td>' + (detail.HeadType || '') + '</td>' +
            '<td>' + (detail.ChqNo || '') + '</td>' +
            '<td>' + (detail.ChqDate ? formatDate(detail.ChqDate) : '') + '</td>' +  // null-safe
            '<td>' + Number(detail.DebitAmount || 0).toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 }) + '</td>' + // formatted amount
            "<td>" + removeButton + "</td>" +
            '</tr>';
        tbody.append(row);
        existingSerialNo++;
    });
    sno = existingSerialNo;
}
// end of Detail section *************


/* Populate DROP DOWN DDL */
function GetVoucherTypeDDL(nature) {
    new APICALL(GetGlobalURL('Base', 'GetVoucherTypeDDL?Nature=' + nature), 'GET', '', true).FETCH((result, error) => {
        if (result) {
            if (result.data && Array.isArray(result.data)) {
                //console.log('test...');

                var dropdown = $("#ddlVType");
                dropdown.empty();
                dropdown.append('<option value="0" selected>Please select</option>');

                $.each(result.data, function (i, item) {
                    dropdown.append(
                        '<option value="' + item.Code + '">' + item.Code + '</option>'
                    );
                });
            }
        }

        if (error) {
            Swal.fire({
                icon: 'error',
                title: 'Error...',
                text: error.data?.responseText || 'API Error',
                footer: ''
            });
        }

    });
}
function GetMaxVoucherID(vtype, callback) {
    new APICALL(GetGlobalURL('Base', 'GetMaxVoucherID?VType=' + vtype), 'GET', '', true).FETCH((result, error) => {
        //console.log(result);

        if (result) {
            if (result.data && Array.isArray(result.data)) {
                callback(result.data[0].MaxCode);  // return value here
                return;
            }
        }

        if (error) {
            Swal.fire({
                icon: 'error',
                title: 'Error...',
                text: error.data?.responseText || 'API Error',
                footer: ''
            });
            callback(0);
        }

    });
}
function GetHeadDDL(cb, dropdown, headType) {
    new APICALL(
        GetGlobalURL('Base', 'GetHeadDDL?CB=' + cb + '&HeadType=' + headType), 'GET', '', true).FETCH((result, error) => {

            if (result) {
                if (result.data && Array.isArray(result.data)) {

                    dropdown.empty();  // dropdown is element passed in parameter
                    dropdown.append('<option value="0" selected>Please select</option>');

                    $.each(result.data, function (i, item) {
                        dropdown.append(
                            '<option value="' + item.HeadCode + '" data-category="' + item.CategoryCode + '">' +
                            item.HeadName +
                            '</option>'
                        );

                    });
                }
            }

            if (error) {
                Swal.fire({
                    icon: 'error',
                    title: 'Error...',
                    text: error.data?.responseText || 'API Error',
                    footer: ''
                });
            }

        });

}
function GetJobDDL() {
    new APICALL(GetGlobalURL('Base', 'GetJobDDL'), 'GET', '', true).FETCH((result, error) => {
        if (result) {
            if (result.data != null) {
                $.each(result.data, function (i, option) {
                    $('#ddlJob').append(
                        '<option value="' + option.JobOrderId + '">' + option.JobNo + '</option>'
                    );
                });
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

/* Common functions */
function ValidateAll() {
    var isValid = true;

    var ddlVType = $("#ddlVType").val();
    //var ddlCrHead = $("#ddlCrHead").val();
    var txtVDate = $("#txtVDate").val();

    if (ddlVType == null || ddlVType == 'string' || !ddlVType.trim()) {
        Swal.fire({
            icon: 'warning',
            dangerMode: true,
            text: 'Please enter VType!',
            confirmButtonColor: "#61affe"
        });
        return false;
    }

    //if (ddlCrHead == null || ddlCrHead == '0' || !ddlCrHead.trim()) {
    //    Swal.fire({
    //        icon: 'warning',
    //        dangerMode: true,
    //        text: 'Please enter Credit Head!',
    //        confirmButtonColor: "#61affe"
    //    });
    //    return false;
    //}

    if (txtVDate == null || txtVDate == 'string' || !txtVDate.trim()) {
        Swal.fire({
            icon: 'warning',
            dangerMode: true,
            text: 'Please enter Voucher Date!',
            confirmButtonColor: "#61affe"
        });
        return false;
    }


    return isValid;
}
function DoEmptyFields() {
    $('#UserID').val('');
    $('#EditID').val('');

    $('#ddlVType').val('0');
    $('#txtVCode').val('');
    $('#txtVDate').val(null);
    //$('#ddlCrHead').val('0');
    //$('#txtMasterCategoryId').val('');
    $('#txtDrAmount').val('');
    $('#txtCrAmount').val('');
    $('#txtNarration').val('');
    //$('#IsActive').prop('checked', true);

    $("#DetailSectionID tbody").empty();

    $('.updatebutton').addClass('d-none');
    $('.savebutton').removeClass('d-none');
}
function SetTodayDate(controlId) {
    let today = new Date();
    let yyyy = today.getFullYear();
    let mm = String(today.getMonth() + 1).padStart(2, '0');
    let dd = String(today.getDate()).padStart(2, '0');

    $('#' + controlId).val(`${yyyy}-${mm}-${dd}`);
}
function toFloat(val) {
    return val === "" || val === null ? 0 : parseFloat(val);
}
function FixDate(value) {
    if (!value || value === "" || value === "-") return null;

    // Try converting to ISO
    let d = new Date(value);
    if (isNaN(d.getTime())) return null;

    return d.toISOString().split("T")[0]; // yyyy-MM-dd
}
function formatDate(dateStr) {
    if (!dateStr) return "";
    let d = new Date(dateStr);
    if (isNaN(d)) return dateStr;
    return d.getFullYear() + "-" +
        String(d.getMonth() + 1).padStart(2, '0') + "-" +
        String(d.getDate()).padStart(2, '0');
}

// Format function
function setDateFields(currentDate) {
    document.getElementById('txtVDate').value = currentDate
    //document.getElementById('txtChqDate').value = currentDate
}
function formatDate(dateString) {
    if (!dateString) return '';
    var date = new Date(dateString);
    var day = ("0" + date.getDate()).slice(-2);
    var month = ("0" + (date.getMonth() + 1)).slice(-2);
    var year = date.getFullYear();
    return day + '-' + month + '-' + year;  // format: dd-MM-yyyy
}
function OpenModal(modelId) {
    const modalElement = document.getElementById(modelId);
    const modalInstance = bootstrap.Modal.getOrCreateInstance(modalElement);
    modalInstance.show();
}
function CloseModal(modelId) {
    const modalElement = document.getElementById(modelId);

    let modalInstance = bootstrap.Modal.getInstance(modalElement);
    if (!modalInstance) {
        modalInstance = bootstrap.Modal.getOrCreateInstance(modalElement);
    }

    modalInstance.hide();
}
