var users = {};
var Clientmanagement = {};
var CurrentMaster = "";
var _Title = "";
var MasterConfig = {
    Industry: {
        title: "Industry",
        getUrl: "GetIndustryDDL",
        saveUrl: "SaveIndustry",
        updateUrl: "UpdateIndustry"
    },

    CompanySize: {
        title: "Company Size",
        getUrl: "GetCompanySizeDDL",
        saveUrl: "SaveCompanySize",
        updateUrl: "UpdateCompanySize"
    },

    Country: {
        title: "Country",
        getUrl: "GetCountryDDL",
        saveUrl: "SaveCountry",
        updateUrl: "UpdateCountry"
    },

    ContractType: {
        title: "Contract Type",
        getUrl: "GetContractTypeDDL",
        saveUrl: "SaveContractType",
        updateUrl: "UpdateContractType"
    },

    AccountOwner: {
        title: "Account Owner",
        getUrl: "GetAccountOwnerDDL",
        saveUrl: "SaveAccountOwner",
        updateUrl: "UpdateAccountOwner"
    },

    SupportOwner: {
        title: "Support Owner",
        getUrl: "GetSupportOwnerDDL",
        saveUrl: "SaveSupportOwner",
        updateUrl: "UpdateSupportOwner"
    },
    
    ContractTypeBasic: {
        title: "Contract Type",
        getUrl: "GetContractTypeBasicDDL",
        saveUrl: "SaveContractTypeBasic",
        updateUrl: "UpdateContractTypeBasic"
    },

    BillingFrequency: {
        title: "Billing Frequency",
        getUrl: "GetBillingFrequencyDDL",
        saveUrl: "SaveBillingFrequency",
        updateUrl: "UpdateBillingFrequency"
    },

    PaymentTerms: {
        title: "Payment Terms",
        getUrl: "GetPaymentTermsDDL",
        saveUrl: "SavePaymentTerms",
        updateUrl: "UpdatePaymentTerms"
    },

    BillingType: {
        title: "Billing Type",
        getUrl: "GetBillingTypeDDL",
        saveUrl: "SaveBillingType",
        updateUrl: "UpdateBillingType"
    },

    BillingStatus: {
        title: "Billing Status",
        getUrl: "GetBillingStatusDDL",
        saveUrl: "SaveBillingStatus",
        updateUrl: "UpdateBillingStatus"
    }
};

$(document).ready(function () {
    _Title = "Industry";
    LoadMaster("Industry");

    $(".master-tab").click(function () {
        CurrentMaster = $(this).data("master");
        _Title = $(this).data("title");

        LoadMaster(CurrentMaster);
    });

    ////$("#masterform").validate();

    $(document).on('click', '.btnAddNew', function (e) {
        $("#EditID").val(0);
        $("#txtName").val('');

        $("#myModalLabel").text("Add New " + _Title);

        var myModal = new bootstrap.Modal(document.getElementById('myModal'));
        myModal.show();
    });

    $('#SaveBtn').on('click', function () {
        SaveData();
    });

    $(document).on('click', '.EditData', function () {
        $("#EditID").val(
            $(this).attr("data-id")
        );

        $("#txtName").val(
            $(this).attr("data-name")
        );

        $("#myModalLabel").text(
            "Edit " + CurrentMaster
        );

        $("#myModal").modal('show');

    });

    $('#UpdateBtn').on('click', function () {
        UpdateData();
    });

    $('#ClearBtn').on('click', function () {
        DoClear();
    });

    $('a[data-bs-toggle="tab"]').on('shown.bs.tab', function (e) {
        let master = $(e.target).data('master');
        let title = $(e.target).data('title');

        CurrentMaster = master;

        $("#MasterTitle").text(title + " Setup");

        LoadMaster(CurrentMaster);
    });


});


function openModalForAddNew() {
    ////clear textboxes...
    //clearFormFields();

    //// Reset modal fields
    //$("#chkIsActive").prop('checked', true);

    //$("#myModalLabel").text("Add New Product");  // Set the modal title
    //$("#modalAction").val("Add");  // Action will be "Add" for new items
    //$('#myModal').modal('show');  // Open the modal

    ////$("#txtName").prop("disabled", false);
}

/* View GRID */
function LoadMaster(masterType) {
    CurrentMaster = masterType;

    ShowLoader('UserMasterDiv');

    new APICALL(GetGlobalURL('Base', MasterConfig[masterType].getUrl), 'GET', '', true).FETCH((result, error) => {
        if (result) {
            if ($.fn.DataTable && $.fn.DataTable.isDataTable('#user-master')) {
                $('#user-master').DataTable().destroy();
            }
            $("#user-master tbody").html('');

            $.each(result.data, function (i, option) {
                var EditBtn =
                    `<td>
                    <button
                       class="avatar-text avatar-md EditData"
                       data-id="${option.ddlvalue}"
                       data-name="${option.ddltext}">
                       <i class="feather-edit-2 edit-icon"></i>
                    </button>
                </td>`;

                EditBtn = "";

                $('#user-master tbody').append(
                    `<tr>
                        <td>${option.ddlvalue}</td>
                        <td>${option.ddltext}</td>
                        ${EditBtn}
                    </tr>`
                );
            });

            // ensure DOM updated before DataTable init
            setTimeout(function () {
                table.DataTable();
            }, 0);

            HideLoader('UserMasterDiv');
        }
    });
}

/* CRUD Operations */
function SaveData() {
    //var res = ValidateAll();
    //if (res == false) {
    //    return false;
    //}

    var data = {
        ID: $("#EditID").val(),
        Name: $("#txtName").val()
    };

    var url = "";
    if ($("#EditID").val() == "0")
        url = MasterConfig[CurrentMaster].saveUrl;
    else
        url = MasterConfig[CurrentMaster].updateUrl;


    Swal.fire({
        title: 'Do you want to save the changes?',
        showDenyButton: true,
        showCancelButton: true,
        confirmButtonText: 'Ok',
        denyButtonText: 'Cancel',
    }).then((result) => {
        if (result.isConfirmed) {
            ShowLoader('UserMasterDiv');

            // API call using JSON instead of form-urlencoded
            new APICALL(GetGlobalURL('Base', url), 'POST', JSON.stringify(data), true, false, 'application/json').FETCH((result, error) => {
                if (result) {
                    $("#myModal").modal('hide');

                    LoadMaster(CurrentMaster);

                    Swal.fire({
                        icon: 'success',
                        title: 'Success',
                        text: 'Saved Successfully'
                    });

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

    $('#ProductName').val(row.ProductName);
    $('#Category').val(row.Category);
    $('#Specification').val(row.Specification);
    $('#Unit').val(row.Unit);

    $('#CP').val(row.CP);
    $('#SP').val(row.SP);

    $('#ReOrderQty').val(row.ReOrderQty);
    $('#Packing').val(row.Packing);

    $('#productType').val(row.ProductType);
    $('#partyId').val(row.PartyId);

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
                ProductId: $('#EditID').val() ? parseInt($('#EditID').val()) : 0,
                ProductName: $('#ProductName').val(),
                Category: $('#Category').val(),
                Specification: $('#Specification').val(),
                Unit: $('#Unit').val(),
                CP: toFloat($('#CP').val()),
                SP: toFloat($('#SP').val()),
                ReOrderQty: $('#ReOrderQty').val() === "" ? 0 : parseInt($('#ReOrderQty').val()),
                Packing: toFloat($('#Packing').val()),
                ProductType: $('#productType').val(),
                PartyId: $('#partyId').val() ? parseInt($('#partyId').val()) : null,
                IsActive: $('#IsActive').is(':checked'),
                CreatedBy: 'superadmin',
                BranchId: 1
            };

            // Convert object to JSON string
            var jsonData = JSON.stringify(formData);

            // Use fetch instead of APICALL to ensure proper JSON binding
            fetch(GetGlobalURL('Base', 'EditProduct'), {
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

/* Common functions */
function ValidateAll() {
    var isValid = true;

    var ProductName = $("#ProductName").val();
    var PartyName = $("#partyId").val();

    if (ProductName == null || ProductName == 'string' || !ProductName.trim()) {
        Swal.fire({
            icon: 'warning',
            dangerMode: true,
            text: 'Please enter Product Name!',
            confirmButtonColor: "#61affe"
        });
        return false;
    }

    if (PartyName == null || PartyName == '0' || !PartyName.trim()) {
        Swal.fire({
            icon: 'warning',
            dangerMode: true,
            text: 'Please enter Party Name!',
            confirmButtonColor: "#61affe"
        });
        return false;
    }

    return isValid;
}
function DoClear() {
    $('#UserID').val('');
    $('#EditID').val('');

    $('#Name').val('');

    //$('#IsActive').prop('checked', true);

    $('.updatebutton').addClass('d-none');
    $('.savebutton').removeClass('d-none');
}
