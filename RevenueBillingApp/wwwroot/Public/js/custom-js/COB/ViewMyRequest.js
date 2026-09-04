var instanceid = 0;
$(document).ready(function () {
    instanceid = GetParameterValues('instanceid') ?? 0;
    LoadViewMyRequest(instanceid);
    getWorkflowLog(instanceid);
    document.addEventListener('click', function (e) {
        const card = e.target.closest('.flip-card');
        if (card && document.body.contains(card)) {
            card.classList.toggle('hover');
        }
    });
});
async function LoadViewMyRequest(instanceid) {
    FillDropdowns_OnLoad();

    //await GetFormsByInstanceId($('#workflow').val(), instanceid, "appendForm", "View");
    GetCOBDetailByInstanceId(instanceid);
}
function GetCOBDetailByInstanceId(instanceid) {
    new APICALL(GetGlobalURL('Base', 'GetCOBDetailByInstanceId') + '?instanceid=' + instanceid, 'GET', '', false).FETCH((result, error) => {

        if (result) {
            if (result.data != null && result.data.length > 0) {

                try {
                    const data = result.data;

                    console.log(data);

                    //$('#Department').val(data[0].dept ?? '');
                    //$('#Purpose').val(data[0].purpose ?? '');
                    //$('#EstimatedAmount').val(data[0].estimatedamount ?? '');
                    //let date = new Date(data[0].requiredby);
                    //let formatted = date.toISOString().split('T')[0];
                    //$('#RequiredBy').val(formatted);
                    //$('#Priority').val(data[0].priority ?? '');
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
    });
}
function GetAllContractTypeBasic_DDL() {
    new APICALL(GetGlobalURL('Base', 'GetContractTypeBasicDDL'), 'GET', '', true).FETCH((result, error) => {
        if (result) {
            if (result.data != null) {
                window.allUsers = result.data;

                var ddl = $('#ddlbasicContractType');
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
function GetAllBillingFrequency_DDL() {
    new APICALL(GetGlobalURL('Base', 'GetBillingFrequencyDDL'), 'GET', '', true).FETCH((result, error) => {
        if (result) {
            if (result.data != null) {
                window.allUsers = result.data;

                var ddl = $('#ddlBillingFreq');
                populateDropdowns(ddl, result.data, null, 0);

                var ddl = $('#ddlBillingFreqRecurring');
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
    GetAllCompanySize_DDL();
    GetAllCountry_DDL();
    GetAllContractType_DDL();
    GetAllAccountOwner_DDL();
    GetAllSupportOwner_DDL();
    GetAllContractTypeBasic_DDL();
    GetAllBillingFrequency_DDL();
    GetAllPaymentTerms_DDL();
    GetAllBillingType_DDL();
    GetAllBillingStatus_DDL();

}
