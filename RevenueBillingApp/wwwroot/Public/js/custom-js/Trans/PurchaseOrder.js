var users = {};
var purchaseDetails = {};
var Clientmanagement = {};
let totalAmount = 0;
var existingSerialNo = 1;
var sno = 1;

$(document).ready(function () {
    ViewData();

    GetParty_DDL(2); //only supplier
    GetWarehouse_DDL(); //get all warehouse

    GetProduct_DDL(); //get all product
    GetJobDDL(); //get all jobOrder

    var date = new Date();
    var currentDate = date.toISOString().slice(0, 10);
    setDateFields(currentDate);

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
        $('#myModalLabel').html('Add New Purchase Order');

        DoEmptyFields();

        GetMaxID('Purchase_Order', function (id) {
            $("#txtPOId").val(id);
        });

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
        totalAmount = 0;
        $("#DetailSectionID tbody tr").each(function () {
            var amountStr = $(this).find("td").eq(6).text().replace(/,/g, '');
            var amount = parseFloat(amountStr) || 0;
            totalAmount += amount;
        });

        $("#txtTotalAmount").val(totalAmount);

        // Update serial numbers
        $("#DetailSectionID tbody tr").each(function (i, tr) {
            $(tr).find("td").eq(0).text(i + 1);
        });

        sno = $("#DetailSectionID tbody tr").length + 1;
    });

});


/* Populate DROP DOWN DDL */
function GetParty_DDL(CategoryCode) {
    new APICALL(GetGlobalURL('Base', 'GetPartyDDL?CategoryCode=' + CategoryCode), 'GET', '', true).FETCH((result, error) => {

        if (result) {
            if (result.data != null) {
                $.each(result.data, function (i, option) {
                    $('#ddlParty').append(
                        '<option value="' + option.PartyId + '">' + option.PartyName + '</option>'
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
function GetWarehouse_DDL() {
    new APICALL(GetGlobalURL('Base', 'GetWarehouseDDL'), 'GET', '', true).FETCH((result, error) => {

        if (result) {
            if (result.data != null) {
                $.each(result.data, function (i, option) {
                    $('#ddlWarehouse').append(
                        '<option value="' + option.WarehouseId + '">' + option.WarehouseDesc + '</option>'
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
function GetProduct_DDL() {
    new APICALL(GetGlobalURL('Base', 'GetProductDDL'), 'GET', '', true).FETCH((result, error) => {

        if (result) {
            if (result.data != null) {
                $.each(result.data, function (i, option) {
                    $('#ddlProduct').append(
                        '<option value="' + option.ProductId + '">' + option.ProductName + '</option>'
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


/* View GRID */
function ViewData() {
    ShowLoader('UserMasterDiv');
    UTILITY.CheckSession((data_) => {
        if (data_) {
            new APICALL(GetGlobalURL('Base', 'GetPurchaseOrder'), 'GET', '', true).FETCH((result, error) => {
                if (result) {
                    $("#user-master").dataTable().fnDestroy();
                    $('#user-master tbody').empty();

                    console.log('dsdsdds......');
                    console.log(result);

                    if (result.data != null) {
                        users = result.data.Master;
                        purchaseDetails = result.data.Detail ?? []; // IMPORTANT

                        // --------- Bind Master to Table ----------
                        $.each(users, function (i, option) {

                            var UpdateBtn = '<td><button class="avatar-text avatar-md EditData" ' +
                                'data-value="' + option.Id + '" data-value1="' + i + '">' +
                                '<i class="feather-edit-2 edit-icon"></i></button></td>';

                            var DeleteBtn = '<td><button class="avatar-text avatar-md DeleteData" ' +
                                'data-value="' + option.Id + '">' +
                                '<i class="feather-trash delete-icon"></i></button></td>';

                            $('#user-master tbody').append(
                                `<tr id="rowid-${i}">
                                    <td>${option.POId}</td>
                                    <td>${formatDate(option.OrderDate)}</td>
                                    <td>${option.Terms_Cond}</td>
                                    <td>${option.QuoteNo}</td>
                                    <td>${formatDate(option.QuoteDate)}</td>
                                    <td>${option.Remarks}</td>
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
function GetMaxID(table, callback) {
    new APICALL(GetGlobalURL('Base', 'GetMaxID?table=' + table), 'GET', '', true).FETCH((result, error) => {
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

                detailSection.POId = parseInt($("#txtPOId").val());
                detailSection.ProductId = parseInt(row.find("td").eq(1).text());

                detailSection.JobNo = row.find("td").eq(3).text().trim(); // Job 1 / Job 2
                detailSection.OQty = parseInt(row.find("td").eq(4).text());

                detailSection.Rate = parseFloat(row.find("td").eq(5).text());

                // ⭐ Business rule
                detailSection.RQty = detailSection.OQty;

                detailSection.TotalAmount = parseFloat(
                    row.find("td").eq(6).text().replace(/,/g, '')
                );

                detailSections.push(detailSection);
            });

            var amountStr = $("#txtTotalAmount").val().replace(/,/g, ''); // Remove commas
            var amount = parseFloat(amountStr) || 0;

            var data = {
                Id: parseInt($("#PK_HiddenID").val() == "" ? 0 : $("#PK_HiddenID").val()),
                POId: parseInt($("#txtPOId").val()),
                OrderDate: $("#txtOrderDate").val(),
                RefNo: '-',
                PartyId: $("#ddlParty").val(),
                WarehouseId: $("#ddlWarehouse").val(),
                DeliveryDate: $("#txtDeliveryDate").val(),
                Remarks: $("#txtRemarks").val(),
                Terms_Cond: $("#txtTerms").val(),
                QuoteNo: $("#txtQuoteNo").val(),
                QuoteDate: $("#txtQuoteDate").val(),
                TotalAmount: amount,
                Cancel: false,
                Clear: false,
                InstanceId: 0,
                BranchId: 1,
                DetailSection: detailSections,
                ////IsActive: $('#IsActive').is(':checked'),
                CreatedBy: 1,
                //CreatedOn: null,
                UpdatedBy: 0,
                //UpdatedOn: null,
                EditID: 1
            }
            console.log(data);

            new APICALL(GetGlobalURL('Base', 'SavePurchaseOrder'), 'POST', JSON.stringify(data), true, false, 'application/json; charset=utf-8')
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
function EditData(Id, index) {

    // ---------- Modal title ----------
    $('#myModalLabel').html('Edit Purchase Order');

    // ---------- Clear old data ----------
    DoEmptyFields('#frm-Idea');

    // ---------- MASTER DATA ----------
    var master = users[index];

    // ---------- DETAIL DATA ----------
    var details = purchaseDetails.filter(x => x.POId == master.POId);

    // ---------- SET MASTER FIELDS ----------
    $('#PK_HiddenID').val(master.Id);
    $('#txtPOId').val(master.POId);

    $('#txtOrderDate').val(FixDate(master.OrderDate));
    $('#ddlParty').val(master.PartyId);
    $('#ddlWarehouse').val(master.WarehouseId);
    $('#txtDeliveryDate').val(FixDate(master.DeliveryDate));

    $('#txtRemarks').val(master.Remarks);
    $('#txtTerms').val(master.Terms_Cond);
    $('#txtQuoteNo').val(master.QuoteNo);
    $('#txtQuoteDate').val(FixDate(master.QuoteDate));


    // ---------- DETAIL GRID ----------
    var tbody = $('#DetailSectionID tbody');
    tbody.empty();
    var totalAmount = 0;
    var sno = 1;

    // 🔹 AJAX call to get details fresh from server
    new APICALL(GetGlobalURL('Base', 'GetPurchaseOrderById') + '?POId=' + master.POId, 'GET', '', true)
        .FETCH((result, error) => {

            if (error) {
                Swal.fire({
                    icon: 'error',
                    title: 'Error fetching details',
                    text: error.data?.responseText || 'Something went wrong'
                });
                return;
            }

            if (result && result.data && result.data.Detail) { // ✅ Use .Detail
                var details = result.data.Detail;

                $.each(details, function (i, d) {

                    var amount = parseFloat(d.TotalAmount) || (parseFloat(d.OQty) * parseFloat(d.Rate));
                    totalAmount += amount;

                    tbody.append(`
                    <tr>
                        <td class="text-center">${sno}</td>
                        <td class="text-center">${d.ProductId}</td>
                        <td class="text-center">${d.ProductName || ''}</td>
                        <td class="text-center">${d.JobNo || ''}</td>
                        <td class="text-center">${d.OQty}</td>
                        <td class="text-center">${d.Rate}</td>
                        <td class="text-center">${amount}</td>
                        <td>
                            <button type="button" class="btn btn-sm btn-danger btnRemoveRow">
                                <i class="fa fa-minus-square"></i>
                            </button>
                        </td>
                    </tr>
                `);

                    sno++;
                });

                $('#txtTotalAmount').val(totalAmount);
            }
        });

    // ---------- BUTTON TOGGLE ----------
    $('#SaveBtn').addClass('d-none');
    $('#UpdateBtn').removeClass('d-none');

    // ---------- OPEN MODAL ----------
    OpenModal('myModal');
}
function UpdateData() {
    var detailSections = [];

    var res = ValidateAll();
    if (!res) return;

    Swal.fire({
        title: 'Do you want to update this Purchase Order?',
        showDenyButton: true,
        confirmButtonText: 'Ok',
        denyButtonText: 'Cancel',
    }).then((result) => {

        if (result.isConfirmed) {

            ShowLoader('UserMasterDiv');

            // ---------- DETAILS ----------
            $("#DetailSectionID tbody tr").each(function () {

                var row = $(this);

                var detail = {
                    POId: parseInt($("#txtPOId").val()),
                    ProductId: parseInt(row.find("td").eq(1).text()),
                    JobNo: row.find("td").eq(3).text(),
                    OQty: parseInt(row.find("td").eq(4).text()),
                    Rate: parseFloat(row.find("td").eq(5).text()),
                    RQty: parseInt(row.find("td").eq(4).text()),
                    TotalAmount: parseFloat(row.find("td").eq(6).text().replace(/,/g, ''))
                };

                detailSections.push(detail);
            });

            var amount = parseFloat($("#txtTotalAmount").val().replace(/,/g, '')) || 0;

            // ---------- MASTER ----------
            var data = {
                Id: parseInt($("#PK_HiddenID").val()),
                POId: parseInt($("#txtPOId").val()),
                OrderDate: $("#txtOrderDate").val(),
                RefNo: '-',
                PartyId: $("#ddlParty").val(),
                WarehouseId: $("#ddlWarehouse").val(),
                DeliveryDate: $("#txtDeliveryDate").val(),
                Remarks: $("#txtRemarks").val(),
                Terms_Cond: $("#txtTerms").val(),
                QuoteNo: $("#txtQuoteNo").val(),
                QuoteDate: $("#txtQuoteDate").val(),
                TotalAmount: amount,
                Cancel: false,
                Clear: false,
                InstanceId: 0,
                BranchId: 1,
                DetailSection: detailSections,
                CreatedBy: 1,
                UpdatedBy: 1,
                EditID: 1
            };

            // ---------- API ----------
            new APICALL(
                GetGlobalURL('Base', 'SavePurchaseOrder'),
                'POST',
                JSON.stringify(data),
                true,
                false,
                'application/json; charset=utf-8'
            ).FETCH((result, error) => {
                if (result && result.status === 'success') {
                    CloseModal('myModal');
                    ViewData();
                    DoEmptyFields('#masterform');

                    Swal.fire({
                        icon: 'success',
                        title: 'Updated!',
                        text: 'Purchase Order Updated Successfully'
                    });
                }

                if (error) {
                    Swal.fire({
                        icon: 'error',
                        title: 'Error',
                        text: error.data?.responseText || 'Update failed'
                    });
                }
            });
        }
    });
}
function DeleteData(ID) {
    var DeleteData = JSON.stringify({ Id: ID });

    Swal.fire({
        title: 'Do you want to save the changes?',
        showDenyButton: true,
        showCancelButton: false,
        confirmButtonText: 'Ok',
        denyButtonText: 'Cancel',
    }).then((result) => {
        if (result.isConfirmed) {
            ShowLoader('UserMasterDiv');

            new APICALL(GetGlobalURL('Base', 'DeletePurchaseOrder'), 'POST', DeleteData, true).FETCH((result, error) => {
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

    if ($('#ddlProduct').val().trim() == "-1") {
        $('#ddlProduct').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#ddlProduct').css('border-color', 'lightgrey');
    }


    if ($('#txtQty').val().trim() == "") {
        $('#txtQty').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#txtQty').css('border-color', 'lightgrey');
    }

    if ($('#txtRate').val().trim() == "") {
        $('#txtRate').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#txtRate').css('border-color', 'lightgrey');
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

    var productCode = $("#ddlProduct").val();
    var ddlProduct = $("#ddlProduct option:selected").text();

    // If value = 0 → Empty save karo
    ddlJobText = (ddlJobValue === "0") ? "" : ddlJobText;

    var txtQty = parseFloat($("#txtQty").val());
    var txtRate = parseFloat($("#txtRate").val());
    var txtAmount = parseFloat(txtQty * txtRate);

    var removeButton = "<button type='button' class='btn btn-sm btn-danger btnRemoveRow'>" +
        "<i class='fa fa-minus-square'></i></button>";

    $("#DetailSectionID tbody").append("<tr>" +
        "<td class='text-center'>" + sno + "</td>" +
        "<td class='text-center'>" + productCode + "</td>" +
        "<td class='text-center'>" + ddlProduct + "</td>" +
        "<td class='text-center'>" + ddlJobText + "</td>" +
        "<td class='text-center'>" + txtQty + "</td>" +
        "<td class='text-center'>" + txtRate + "</td>" +
        "<td class='text-center'>" + txtAmount + "</td>" +
        "<td>" + removeButton + "</td>" +
        "</tr>");

    // Increment the serial number for the next entry
    sno++;
    totalAmount += txtAmount;
    $("#txtTotalAmount").val(totalAmount);

    //-------------
    $("#ddlProduct").val(null);
    $("#txtQty").val(null);
    $("#txtRate").val(null);
    $("#txtAmount").val(null);
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

    $("#txtTotalAmount").val(totalAmount.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 }));

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
                $("#txtTotalAmount").val(varData[0].Amount);
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
            '<td>' + (detail.ProductCode || '') + '</td>' +
            '<td>' + (detail.ProductName || '') + '</td>' +
            '<td>' + (detail.Qty || '') + '</td>' +
            '<td>' + (detail.Rate || '') + '</td>' +
            '<td>' + Number(detail.Amount || 0).toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 }) + '</td>' + // formatted amount
            "<td>" + removeButton + "</td>" +
            '</tr>';
        tbody.append(row);
        existingSerialNo++;
    });
    sno = existingSerialNo;
}
// end of Detail section *************


/* Common functions */
function ValidateAll() {
    var isValid = true;

    var ddlParty = $("#ddlParty").val();
    var ddlWarehouse = $("#ddlWarehouse").val();
    var txtOrderDate = $("#txtOrderDate").val();

    if (ddlParty == null || ddlParty == 'string' || !ddlParty.trim()) {
        Swal.fire({
            icon: 'warning',
            dangerMode: true,
            text: 'Please enter Party!',
            confirmButtonColor: "#61affe"
        });
        return false;
    }

    if (ddlWarehouse == null || ddlWarehouse == '0' || !ddlWarehouse.trim()) {
        Swal.fire({
            icon: 'warning',
            dangerMode: true,
            text: 'Please enter Warehouse!',
            confirmButtonColor: "#61affe"
        });
        return false;
    }

    if (txtOrderDate == null || txtOrderDate == 'string' || !txtOrderDate.trim()) {
        Swal.fire({
            icon: 'warning',
            dangerMode: true,
            text: 'Please enter Order Date!',
            confirmButtonColor: "#61affe"
        });
        return false;
    }


    return isValid;
}
function DoEmptyFields(formSelector) {
    $('#UserID').val('');
    $('#EditID').val('');

    //$('#ddlVType').val('0');
    //$('#txtVCode').val('');
    //$('#txtVDate').val(null);
    //$('#ddlCrHead').val('0');
    //$('#txtMasterCategoryId').val('');
    //$('#txtTotalAmount').val('');
    //$('#txtNarration').val('');
    ////$('#IsActive').prop('checked', true);

    //$("#DetailSectionID tbody").empty();

    //$('.updatebutton').addClass('d-none');
    //$('.savebutton').removeClass('d-none');


    var $form = $(formSelector);

    // Text, hidden, number, date inputs clear
    $form.find('input[type="text"], input[type="hidden"], input[type="number"], input[type="date"]').val('');

    // Select dropdowns reset (0 ya first option)
    $form.find('select').val('0');

    // Textarea clear
    $form.find('textarea').val('');

    // Table body empty (agar form ke andar ho)
    $form.find('tbody').empty();

    // Buttons toggle (agar form ke andar hain)
    $form.find('.updatebutton').addClass('d-none');
    $form.find('.savebutton').removeClass('d-none');


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
    document.getElementById('txtOrderDate').value = currentDate
    document.getElementById('txtDeliveryDate').value = currentDate
    document.getElementById('txtQuoteDate').value = currentDate
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
