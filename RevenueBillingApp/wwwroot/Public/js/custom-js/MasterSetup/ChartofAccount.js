var users = {};
var Clientmanagement = {};

$(document).ready(function () {
    ViewData();

    GetFillFinancialCategoryDDL($("#ddlFCL2"));
    GetFillFinancialCategoryDDL($("#ddlFCL3"));
    GetFillFinancialCategoryDDL($("#ddlFC"));


    //for Modal 2
    $('#ddlFCL2').change(function () {
        var selectedValue = $(this).val();
        var selectedText = $('#ddlFCL2 option:selected').text();
        var prefix = selectedText.substring(0, 2);

        var nature = "";
        if (prefix == "01" || prefix == "02") {
            nature = "Debit";
        } else if (["03", "04", "05"].includes(prefix)) {
            nature = "Credit";
        }

        $("#txtNatureL2").val(nature);

        // ⬇⬇ FIX — USE CALLBACK
        GetAutoGLCode('2', prefix, function (glCode) {
            $("#txtGLCodeL2").val(glCode);
        });



    });

    //for Modal 3
    $('#ddlFCL3').change(function () {
        var selectedValue = $(this).val();
        var selectedText = $('#ddlFCL3 option:selected').text();
        var prefix = selectedText.substring(0, 2);

        $('#txtGLCodeAccountL3').val(null);
        $('#txtGLCodeL3').val(null);

        var nature = "";
        if (prefix == "01" || prefix == "02") {
            nature = "Debit";
        } else if (["03", "04", "05"].includes(prefix)) {
            nature = "Credit";
        }
        // 
        $("#txtNatureL3").val(nature);

        //For Modal3
        var ddl = $("#ddlAccountL3");
        GetFillDropdown('2', selectedText, ddl);
    });

    $('#ddlAccountL3').on('change', function () {
        var glcode = $(this).find(':selected').data('glcode') || '';
        $('#txtGLCodeAccountL3').val(glcode); // set in textbox

        //$("#txtGLCodeL3").val(GetAutoGLCode('3', glcode));
        GetAutoGLCode('3', glcode, function (glCode2) {
            $("#txtGLCodeL3").val(glCode2);
        });

    });


    //for Modal 4
    $('#ddlFC').change(function () {
        var selectedValue = $(this).val();
        var selectedText = $('#ddlFC option:selected').text();
        var prefix = selectedText.substring(0, 2);

        $('#txtGLCodeA').val(null);
        $('#txtGLCodeB').val(null);
        $('#txtGLCodeC').val(null);

        var nature = "";
        if (prefix == "01" || prefix == "02") {
            nature = "Debit";
        } else if (["03", "04", "05"].includes(prefix)) {
            nature = "Credit";
        }
        // 
        $("#txtNature").val(nature);

        //For Modal4
        var ddl = $("#ddlAccount");
        GetFillDropdown('2', selectedText, ddl);
    });

    $('#ddlAccount').on('change', function () {
        var accountId = $('#ddlAccount').val();
        var selectedText = $('#ddlFC option:selected').text();
        var glcode = $(this).find(':selected').data('glcode') || '';
        $('#txtGLCodeA').val(glcode); // set in textbox

        $('#txtGLCodeB').val(null);
        $('#txtGLCodeC').val(null);

        //For Modal4
        var ddl = $("#ddlCategory");
        GetFillCategory(accountId, ddl);
    });

    $('#ddlCategory').on('change', function () {
        var glcode = $(this).find(':selected').data('glcode') || '';
        $('#txtGLCodeB').val(glcode); // set in textbox

        //$("#txtGLCodeC").val(GetAutoGLCode('4', glcode));
        GetAutoGLCode('4', glcode, function (glCode3) {
            $("#txtGLCodeC").val(glCode3);
        });

    });

    //+++++++++++++++++++++++++++

    $(document).on('click', '.btnSaveL2', function (e) {
        e.preventDefault();

        if (ValidateAll_Level2() == false) {
            return false;
        }

        SaveData_Level2();
    });


    $(document).on('click', '.btnSaveL3', function (e) {
        e.preventDefault();

        if (ValidateAll_Level3() == false) {
            return false;
        }

        SaveData_Level3();
    });


    //++++++++++++++++++++++++++

    $("#masterform").validate();

    $('#SaveBtn').on('click', function () {
        SaveData();
    });

    $('#UpdateBtn').on('click', function () {
        UpdateData();
    });

    $('#btnAddNew').on('click', function () {
        ////FetchAdUsers();
        OpenModal('myModal');
    });

    $('#btnLevel2').on('click', function () {
        ////FetchAdUsers();
        OpenModal('myL2Modal');
    });
    $('#btnLevel3').on('click', function () {
        ////FetchAdUsers();
        OpenModal('myL3Modal');
    });



    $('#ClearBtn').on('click', function () {
        DoClear();
    });

    $('.closebtn').on('click', function () {
        CloseModal('myModal');

        CloseModal('myL2Modal');
        CloseModal('myL3Modal');
    });

});


/* View GRID */
function ViewData() {
    ShowLoader('UserMasterDiv');
    UTILITY.CheckSession((data_) => {
        if (data_) {
            new APICALL(GetGlobalURL('Base', 'GetChartofAccount'), 'GET', '', true).FETCH((result, error) => {
                if (result) {
                    $("#user-master").dataTable().fnDestroy();
                    $('#user-master tbody').html('');

                    //console.log(result);

                    if (result.data != null) {
                        users = result.data;
                        $.each(result.data, function (i, option) {
                            var primaryID = option.HeadID

                            // Create Active/Inactive badge
                            var activeBadge = option.IsActive
                                ? '<span class="badge bg-success">Active</span>'
                                : '<span class="badge bg-danger">Inactive</span>';

                            //var UpdateBtn = (data_[0].AllowUpdate == true) ? '<td><button class="avatar-text avatar-md EditData" data-value="' + primaryID + '" data-value1="' + i + '" type="button" data-bs-toggle="tooltip" data-bs-placement="bottom" title="" data-bs-original-title="Edit" aria-label="Edit"><i class="feather-edit-2 edit-icon"></i></button></td>' : '';
                            //var DeleteBtn = (data_[0].AllowDelete == true) ? '<td><button class="avatar-text avatar-md DeleteData" data-value="' + primaryID + '" type="button" data-bs-toggle="tooltip" data-bs-placement="bottom" title="" data-bs-original-title="Delete" aria-label="Delete"><i class="feather-trash delete-icon"></i></button></td>' : '';

                            $('#user-master tbody').append(
                                '<tr id="rowid-' + i + '">' +
                                '<td>' + option.HeadName + '</td>' +
                                '<td>' + option.GLCode + '</td>' +
                                '<td>' + option.CB + '</td>' +
                                '<td>' + option.Category + '</td>' +
                                '<td>' + option.Account + '</td>' +
                                '<td>' + option.FC + '</td>' +
                                '<td>' + activeBadge + '</td>' +
                                //UpdateBtn +
                                //DeleteBtn +
                                '</tr>'
                            );
                        });
                        //$('.EditData').on('click', function () {
                        //    EditData(this.attributes["data-value"].value, this.attributes["data-value1"].value);
                        //});

                        //$('.DeleteData').on('click', function () {
                        //    DeleteData(this.attributes["data-value"].value);
                        //});

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

// ++++++ SaveData of Modal Level2, Level3
function SaveData_Level2() {
    var action = $("#modalActionL2").val();

    Swal.fire({
        title: 'Do you want to save the changes?',
        showDenyButton: true,
        showCancelButton: false,
        confirmButtonText: 'Ok',
        denyButtonText: 'Cancel',
    }).then((result) => {

        if (result.isConfirmed) {
            var formData = {
                Id: $("#pkIDL2").val() === "" ? 0 : parseInt($("#pkIDL2").val()),
                Name: $("#txtNameL2").val(),
                GLCode: $("#txtGLCodeL2").val(),
                FC: $('#ddlFCL2 option:selected').text(),
                FC_Nature: $("#txtNatureL2").val(),
                IsActive: 1,
                CreatedBy: 1,
                BranchId: 1,
                Action: action
            };

            var jsonData = JSON.stringify(formData);

            new APICALL(GetGlobalURL('Base', 'SaveDataLevel2'), 'POST', jsonData, true, false, 'application/json')
                .FETCH((result, error) => {

                    if (result && result.status === "success") {
                        Swal.fire({
                            icon: 'success',
                            text: action === "Add" ? 'Level II added successfully!' : 'Level II updated successfully!'
                        });

                        clearFormFields(2);
                        CloseModal('myL2Modal');
                        return;
                    }

                    if (error && error.status && error.status !== "success") {
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
function SaveData_Level3() {
    var action = $("#modalActionL3").val();

    Swal.fire({
        title: 'Do you want to save the changes?',
        showDenyButton: true,
        showCancelButton: false,
        confirmButtonText: 'Ok',
        denyButtonText: 'Cancel',
    }).then((result) => {

        if (result.isConfirmed) {
            var formData = {
                Id: $("#pkIDL3").val() === "" ? 0 : parseInt($("#pkIDL3").val()),
                Name: $("#txtNameL3").val(),
                GLCode: $("#txtGLCodeL3").val(),
                FK_AccountId: $("#ddlAccountL3").val(),
                IsActive: 1,
                CreatedBy: 1,
                BranchId: 1,
                Action: action
            };

            var jsonData = JSON.stringify(formData);

            new APICALL(GetGlobalURL('Base', 'SaveDataLevel3'), 'POST', jsonData, true, false, 'application/json')
                .FETCH((result, error) => {
                    if (result && result.status === "success") {
                        Swal.fire({
                            icon: 'success',
                            text: action === "Add" ? 'Level III added successfully!' : 'Level III updated successfully!'
                        });

                        clearFormFields(3);
                        CloseModal('myL3Modal');
                        return;
                    }

                    if (error && error.status && error.status !== "success") {
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
function SaveData() {
    //if (!$('#masterform').valid()) {
    //    return false;
    //}

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

            // Convert form data to a JSON object
            var formData = {
                Id: parseInt($("#pkID").val() == "" ? 0 : $("#pkID").val()),
                Name: $("#txtNameL4").val() || "",
                GLCode: $("#txtGLCodeC").val() || "",
                CB: $("#chkForCB").prop('checked') ? '1' : '0',
                FK_CategoryId: parseInt($("#ddlCategory").val()) || 0,
                CreatedBy: 1,                      // Must be int
                IsActive: $('#IsActive').is(':checked'),
                Action: $("#modalAction").val() || "",
                BranchId: 1,
                EditID: "superadmin"               // optional
            };

            // Convert to JSON string
            var jsonData = JSON.stringify(formData);

            // API call using JSON instead of form-urlencoded
            new APICALL(GetGlobalURL('Base', 'SaveDataLevel4'), 'POST', jsonData, true, false, 'application/json')
                .FETCH((result, error) => {

                    if (result && result.status === 'success') {
                        ViewData();
                        DoEmptyFields();
                        clearFormFields(4);

                        Swal.fire({
                            icon: 'success',
                            title: 'Success...',
                            text: 'Saved Successfully!'
                        });
                        return;
                    }

                    // ERROR block
                    if (error && error.status && error.status !== "success") {
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
    DoEmptyFields();

    var row = users[index]; // <-- define row
    var IsActive = users[index].IsActive;

    $('#EditID').val(ID);
    ////alert(ID);

    $('#PartyName').val(row.PartyName);
    $('#Contact').val(row.Contact);
    $('#Address').val(row.Address);
    $('#Phone1').val(row.Phone1);
    $('#Phone2').val(row.Phone2);
    $('#Cell').val(row.Cell);
    $('#Fax').val(row.Fax);
    $('#Email').val(row.Email);

    $('#City').val(row.City);
    $('#PartyGroup').val(row.PartyGroup);
    $('#NTNNo').val(row.NTNNo);
    $('#GSTNo').val(row.GSTNo);

    $('#Remarks').val(row.Remarks);

    $('#Commission').val(row.Commission);
    $('#VanSharing').val(row.VanSharing);
    $('#GST').val(row.GST);
    $('#SST').val(row.SST);
    $('#WHT').val(row.WHT);

    $('#OpeningBalance').val(row.OpeningBalance);
    $('#OpeningDate').val(row.OpeningDate);
    $('#PaymentTerm').val(row.PaymentTerm);

    $('#IsActive').prop('checked', IsActive);

    if ($('.updatebutton').hasClass('d-none')) {
        $('.savebutton').toggleClass('d-none');
        $('.updatebutton').toggleClass('d-none');
    }

}
function UpdateData() {
    var res = ValidateAll();
    if (res == false) {
        return false;
    }

    Swal.fire({
        title: 'Do you want to update the record?',
        showDenyButton: true,
        showCancelButton: false,
        confirmButtonText: 'Ok',
        denyButtonText: 'Cancel',
    }).then((result) => {
        if (result.isConfirmed) {
            ShowLoader('UserMasterDiv');

            // Build JSON object manually
            var formData = {
                PartyId: $('#EditID').val() === "" ? 0 : parseInt($('#EditID').val()),
                PartyCode: $('#EditID').val() === "" ? 0 : parseInt($('#EditID').val()),
                PartyName: $('#PartyName').val(),
                Contact: $('#Contact').val(),
                Address: $('#Address').val(),
                Phone1: $('#Phone1').val(),
                Phone2: $('#Phone2').val(),
                Cell: $('#Cell').val(),
                Fax: $('#Fax').val(),
                Email: $('#Email').val(),
                City: $('#City').val(),
                PartyGroup: $('#PartyGroup').val(),
                NTNNo: $('#NTNNo').val(),
                GSTNo: $('#GSTNo').val(),
                Remarks: $('#Remarks').val(),
                CategoryCode: $('#CategoryCode').val(),
                GLCode: $('#GLCode').val(),
                // float fields (empty -> 0)
                Commission: toFloat($('#Commission').val()),
                VanSharing: toFloat($('#VanSharing').val()),
                GST: toFloat($('#GST').val()),
                SST: toFloat($('#SST').val()),
                WHT: toFloat($('#WHT').val()),
                OpeningBalance: $('#OpeningBalance').val(),
                OpeningDate: $('#OpeningDate').val(),
                PaymentTerm: $('#PaymentTerm').val(),
                IsActive: $('#IsActive').is(':checked'),
                CreatedBy: 'superadmin',
                BranchId: 1
            };

            // Convert object to JSON string
            var jsonData = JSON.stringify(formData);

            // Use fetch instead of APICALL to ensure proper JSON binding
            fetch(GetGlobalURL('Base', 'EditParty'), {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: jsonData
            })
                .then(async res => {
                    const data = await res.json();
                    //console.log('Server response:', data);

                    if (res.ok) {
                        ViewData();
                        DoEmptyFields();

                        Swal.fire({
                            icon: 'success',
                            title: 'Success...',
                            text: 'Updated Successfully!'
                        });
                    } else {
                        Swal.fire({
                            icon: 'error',
                            title: 'Error...',
                            text: 'Update failed: ' + (data || 'Unknown error')
                        });
                    }

                    HideLoader('UserMasterDiv');
                })
                .catch(err => {
                    console.error('Fetch error:', err);
                    Swal.fire({
                        icon: 'error',
                        title: 'Error...',
                        text: 'Something went wrong while updating!'
                    });
                    HideLoader('UserMasterDiv');
                });
        }
    });
}
function DeleteData(ID) {
    var DeleteData = JSON.stringify({ PartyId: ID });

    Swal.fire({
        title: 'Do you want to save the changes?',
        showDenyButton: true,
        showCancelButton: false,
        confirmButtonText: 'Ok',
        denyButtonText: 'Cancel',
    }).then((result) => {
        if (result.isConfirmed) {
            ShowLoader('UserMasterDiv');

            new APICALL(GetGlobalURL('Base', 'DeleteParty'), 'POST', DeleteData, true).FETCH((result, error) => {
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


/* Populate DROP DOWN DDL */
function GetFillFinancialCategoryDDL(dropdown) {
    new APICALL(GetGlobalURL('Base', 'GetFillFinancialCategoryDDL'), 'GET', '', true).FETCH((result, error) => {

        if (result) {
            if (result.data != null) {
                $.each(result.data, function (i, option) {
                    $(dropdown).append(
                        '<option value="' + option.Id + '">' + option.Name + '</option>'
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
function GetAutoGLCode(level, FC, callback) {
    let url = GetGlobalURL('Base', 'GetAutoGLCode') + `?Level=${level}&FC=${FC}`;

    new APICALL(url, 'GET', '', true).FETCH((result, error) => {
        if (result) {
            if (result.data && result.data.length > 0) {
                callback(result.data[0].AutoGeneratedGLCode);
            } else {
                callback("");
            }
        }

        if (error) {
            console.log(error);

            Swal.fire({
                icon: 'error',
                title: 'Error...',
                text: error.data?.responseText || 'Something went wrong',
            });

            callback("");
        }
    });
}
function GetFillDropdown(level, FC, dropdown) {
    let url = GetGlobalURL('Base', 'GetFillDropdown') + `?Level=${level}&FC=${FC}`;
    new APICALL(url, 'GET', '', true).FETCH((result, error) => {
        if (result) {
            if (result.data && Array.isArray(result.data)) {
                dropdown.empty();
                dropdown.append('<option value="-1" selected>Please select</option>');

                result.data.forEach(item => {
                    dropdown.append(
                        `<option value="${item.Id}" data-glcode="${item.GLCode}">
                            ${item.Name}
                        </option>`
                    );
                });
            }
        }

        if (error) {
            console.log(error);

            Swal.fire({
                icon: 'error',
                title: 'Error...',
                text: error.data?.responseText || "Something went wrong",
            });
        }
    });
}
function GetFillCategory(accountId, dropdown) {
    let url = GetGlobalURL('Base', 'GetFillCategory') + `?accountId=${accountId}`;
    new APICALL(url, 'GET', '', true).FETCH((result, error) => {
        if (result) {
            if (result.data && Array.isArray(result.data)) {
                dropdown.empty();
                dropdown.append('<option value="-1" selected>Please select</option>');

                result.data.forEach(item => {
                    dropdown.append(
                        `<option value="${item.Id}" data-glcode="${item.GLCode || ''}">
                            ${item.Name}
                        </option>`
                    );
                });
            }
        }

        if (error) {
            console.log(error);

            Swal.fire({
                icon: 'error',
                title: 'Error...',
                text: error.data?.responseText || 'Something went wrong',
            });
        }

    });
}

//function FetchAdUsers() {
//    ShowLoader('ADUserMasterDiv');
//    new APICALL(GetGlobalURL('Base', 'FetchAdUsers'), 'GET', '', true).FETCH((result, error) => {
//        if (result) {
//            OpenModal();
//            $('#TblActiveDirectoryUsers').DataTable().clear().destroy();
//            $('#TblActiveDirectoryUsers tbody').empty();
//            if (result.data != null) {
//                AdUsers = result.data;
//                $.each(result.data, function (i, option) {
//                    $('#TblActiveDirectoryUsers tbody').append(
//                        '<tr id="rowid-' + i + '">' +
//                        '<td>' + option.UserName + '</td>' +
//                        '<td>' + option.UserID + '</td>' +
//                        '<td>' + option.EmailID + '</td>' +
//                        '<td>' + option.Designation + '</td>' +
//                        '<td>' + option.Department + '</td>' +
//                        '<td><button class="btn btn-sm btn-orange btn-block SelectAdUser" data-value="' + i + '" type="button"><i class="bx bx-expand"></i></button></td>' +
//                        '</tr>'
//                    );
//                });
//                $('.SelectAdUser').on('click', function () {
//                    SelectAdUser(this.attributes["data-value"].value);
//                });

//            }
//            $('#TblActiveDirectoryUsers').DataTable();
//        }
//        if (error) {

//            Swal.fire({
//                icon: 'error',
//                title: 'Error...',
//                text: error.data.responseText,
//                footer: ''
//            });
//        }
//        HideLoader('ADUserMasterDiv');
//    });
//}


/* Common functions */

// ++++++ Validation of Modal Level2
function ValidateAll_Level2() {
    var isValid = true;

    var ddlFCL2 = $("#ddlFCL2");
    var txtNameL2 = $("#txtNameL2");

    if (ddlFCL2.val() == "-1" || ddlFCL2.val() == 'string' || !ddlFCL2.val().trim()) {
        Swal.fire({
            icon: 'warning',
            dangerMode: true,
            text: 'Please select Financial Category!'
        });
        ddlFCL2.focus();
        return false;
    }

    if (txtNameL2.val() == null || txtNameL2.val() == 'string' || !txtNameL2.val().trim()) {
        Swal.fire({
            icon: 'warning',
            dangerMode: true,
            text: 'Please Enter Level II Name!'
        });
        txtNameL2.focus();
        return false;
    }

    return isValid;
}
function ValidateAll_Level3() {
    var isValid = true;

    var ddlFCL3 = $("#ddlFCL3").val();
    var ddlAccountL3 = $("#ddlAccountL3").val();
    var txtNameL3 = $("#txtNameL3").val();

    if (ddlFCL3 == null || ddlFCL3 == 'string' || !ddlFCL3.trim()) {
        Swal.fire({
            icon: 'warning',
            dangerMode: true,
            text: 'Please select Financial Category!'
        });
        return false;
    }

    if (ddlAccountL3 == null || ddlAccountL3 == 'string' || !ddlAccountL3.trim()) {
        Swal.fire({
            icon: 'warning',
            dangerMode: true,
            text: 'Please Select Level II Account!'
        });
        return false;
    }

    if (txtNameL3 == null || txtNameL3 == 'string' || !txtNameL3.trim()) {
        Swal.fire({
            icon: 'warning',
            dangerMode: true,
            text: 'Please Enter Level III Name!'
        });
        return false;
    }

    return isValid;
}
function ValidateAll() {
    var isValid = true;

    var ddlFC = $("#ddlFC").val();
    var ddlAccount = $("#ddlAccount").val();
    var ddlCategory = $("#ddlCategory").val();
    var txtNameL4 = $("#txtNameL4").val();

    if (ddlAccount == null || ddlAccount == 'string' || !ddlAccount.trim()) {
        Swal.fire({
            icon: 'warning',
            dangerMode: true,
            text: 'Please Select Account Name!'
        });
        return false;
    }

    if (ddlCategory == null || ddlCategory == 'string' || !ddlCategory.trim()) {
        Swal.fire({
            icon: 'warning',
            dangerMode: true,
            text: 'Please Select Category Name!'
        });
        return false;
    }


    if (txtNameL4 == null || txtNameL4 == 'string' || !txtNameL4.trim()) {
        Swal.fire({
            icon: 'warning',
            dangerMode: true,
            text: 'Please Enter Level IV Name!'
        });
        return false;
    }

    return isValid;
}
function DoClear() {
    $('#UserID').val('');
    $('#EditID').val('');

    $('#PartyName').val('');
    $('#Contact').val('');
    $('#Address').val('');
    $('#Phone1').val('');
    $('#Phone2').val('');
    $('#Cell').val('');
    $('#Fax').val('');
    $('#Email').val('');

    $('#City').val('');
    $('#PartyGroup').val('');
    $('#NTNNo').val('');
    $('#GSTNo').val('');

    $('#Remarks').val('');

    $('#Commission').val('');
    $('#VanSharing').val('');
    $('#GST').val('');
    $('#SST').val('');
    $('#WHT').val('');

    $('#OpeningBalance').val('0');
    // Set today's date dynamically
    SetTodayDate("OpeningDate");
    $('#PaymentTerm').val('Cash');

    $('#IsActive').prop('checked', true);

    $('.updatebutton').addClass('d-none');
    $('.savebutton').removeClass('d-none');
}
function DoEmptyFields() {
    $('#UserID').val('');
    $('#EditID').val('');

    //$("#pkID").val(null);
    //$("#ddlFC").val("-1");
    //$("#txtNature").val(null);
    //$("#ddlAccount").val("-1");
    //$("#txtGLCodeA").val(null);
    //$("#ddlCategory").val("-1");
    //$("#txtGLCodeB").val(null);
    //$("#txtNameL4").val(null);
    //$("#txtGLCodeC").val(null);

    $('#txtNature').val('');
    $('#ddlAccount').val('');
    $('#txtGLCodeA').val('');
    $('#ddlCategory').val('');
    $('#txtGLCodeB').val('');
    $('#txtNameL4').val('');
    $('#txtGLCodeC').val('');

    $('#IsActive').prop('checked', true);

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
// Format function
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
function clearFormFields(modal) {
    if (modal == 2) {
        $("#pkIDL2").val(null);
        $("#ddlFCL2").val("-1");
        $("#txtNatureL2").val(null);
        $("#txtNameL2").val(null);
        $("#txtGLCodeL2").val(null);

    } else if (modal == 3) {
        $("#pkIDL3").val(null);
        $("#ddlFCL3").val("-1");
        $("#ddlAccountL3").val("-1");
        $("#txtNatureL3").val(null);
        $("#txtNameL3").val(null);
        $("#txtGLCodeL3").val(null);
        $("#txtGLCodeAccountL3").val(null);

    } else { //Level4
        $("#pkID").val(null);
        $("#ddlFC").val("-1");
        $("#txtNature").val(null);
        $("#ddlAccount").val("-1");
        $("#txtGLCodeA").val(null);
        $("#ddlCategory").val("-1");
        $("#txtGLCodeB").val(null);
        $("#txtNameL4").val(null);
        $("#txtGLCodeC").val(null);
    }
}
