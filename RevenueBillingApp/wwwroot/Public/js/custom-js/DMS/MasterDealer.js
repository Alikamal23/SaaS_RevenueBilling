var requestid = '';
var imagepath = '';

$(document).ready(function () {
    GetZones();

    $('#ddl_zone').change(GetUsernames);
    $('#ddl_dealertype').change(GetDealers);

    $('#chk_zone').change(function () {

        if ($(this).is(':checked')) {
            $('#ddl_zone').val('0');
            $('#ddl_zone').prop('disabled', true);
            GetUsernames();
        }
        else {
            $('#ddl_zone').prop('disabled', false);
            $('#ddl_username').empty();
            $('#ddl_username').append('<option selected="true" value="0"> Select Username</option>');
        }

    });

    $('#chk_dealertype').change(function () {
        if ($(this).is(':checked')) {
            $('#ddl_dealertype').val('0');
            $('#ddl_dealertype').prop('disabled', true);
            GetDealers();
        }
        else {
            $('#ddl_dealertype').prop('disabled', false);
            $('#ddl_dealers').empty();
            $('#ddl_dealers').append('<option selected="true" value="0"> Select Dealer</option>');
        }

    });

    $('#ddl_dealers').change(GetDealerInfo);

    $('#div_dealerinfo').hide();
    $('#btn_update').hide();
    $('#btn_save').show();

    $('#btn_view').click(ShowImage);

    $('#btn_save').click(AssignDealer);

    $('#ddl_username').select2();

});

function GetZones() {
    new APICALL(GetGlobalURL('Base', 'GetZones'), 'GET', '', true).FETCH((result, error) => {
        if (result) {
            if (result.data != null && result.data.length > 0) {

                $('#ddl_zone').empty(); // Clear existing options
                $('#ddl_zone').append('<option selected="true" value="0"> Select Zone</option>');

                $.each(result.data, function (i, option) {
                    $('#ddl_zone').append(
                        `<option value="${option.id}"> ${option.zone}</option>`
                    );
                });

                GetDealerTypes();

            }
            else {
                $('#ddl_zone').empty(); // Clear existing options
                $('#ddl_zone').append('<option selected="true" value="0"> Select Zone</option>');
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
function GetUsernames() {
    var zone = 0;
    if ($('#ddl_zone').val() > 0) {
        zone = $('#ddl_zone').val();
    }

    new APICALL(GetGlobalURL('Base', 'GetUserNames?zoneid=' + zone), 'GET', '', true).FETCH((result, error) => {
        if (result) {
            if (result.data != null && result.data.length > 0) {
                $('#ddl_username').empty(); // Clear existing options
                $('#ddl_username').append('<option  value="0"> Select Username</option>');

                $.each(result.data, function (i, option) {
                    $('#ddl_username').append(
                        `<option value="${option.userid}"> ${option.username}</option>`
                    );
                });

                $('#ddl_username').select2();
            }
            else {
                $('#ddl_username').empty(); // Clear existing options
                $('#ddl_username').append('<option selected="true" value="0"> Select Username</option>');
                $('#ddl_username').select2();
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
function GetDealerTypes() {
    new APICALL(GetGlobalURL('Base', 'GetDealerTypes_MasterDealer'), 'GET', '', true).FETCH((result, error) => {
        if (result) {
            if (result.data != null && result.data.length > 0) {
                $('#ddl_dealertype').empty(); // Clear existing options
                $('#ddl_dealertype').append('<option selected="true" value="0"> Select Dealer Type</option>'); // Add default option

                $.each(result.data, function (i, option) {
                    $('#ddl_dealertype').append(
                        `<option value="${option.id}"> ${option.dealer_type}</option>`
                    );
                });
            }
            else {
                $('#ddl_dealertype').empty(); // Clear existing options
                $('#ddl_dealertype').append('<option selected="true" value="0"> Select Dealer Type</option>'); // Add default option
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




//++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

function GetDealers() {
    var dealertypeid = 0;

    if ($('#ddl_dealertype').val() > 0) {
        dealertypeid = $('#ddl_dealertype').val();
    }

    new APICALL(GetGlobalURL('Base', 'GetDealers?dealertype=' + dealertypeid), 'GET', '', true).FETCH((result, error) => {
        if (result) {
            if (result.data != null && result.data.length > 0) {
                $('#ddl_dealers').empty(); // Clear existing options
                $('#ddl_dealers').append('<option selected="true" value="0"> Select Dealer</option>');

                $.each(result.data, function (i, option) {
                    $('#ddl_dealers').append(
                        `<option value="${option.id}"> ${option.Dealership_name}</option>`
                    );
                });
            }
            else {
                $('#ddl_dealers').empty(); // Clear existing options
                $('#ddl_dealers').append('<option selected="true" value="0"> Select Dealer</option>');
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
function GetDealerInfo() {
    if ($('#ddl_dealers').val() > 0) {
        new APICALL(GetGlobalURL('Base', 'GetDealerInfo?dealerid=' + $('#ddl_dealers').val()), 'GET', '', true).FETCH((result, error) => {
            if (result) {
                if (result.data != null && result.data.length > 0) {
                    $('#lbl_cnic').html(result.data[0].cnic);
                    $('#lbl_address').html(result.data[0].address);
                    $('#lbl_contact').html(result.data[0].dealer_contactno);
                    $('#lbl_city').html(result.data[0].city);

                    imagepath = result.data[0].imagepath;

                    $('#div_dealerinfo').show();
                }
                else {
                    $('#lbl_cnic').html('');
                    $('#lbl_address').html('');
                    $('#lbl_contact').html('');
                    $('#lbl_city').html('');
                    $('#div_dealerinfo').hide();
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
    else {
        $('#div_dealerinfo').hide();

    }

}
function ShowImage() {
    var filePath = GetViewURL() + imagepath;
    window.open(filePath, '_blank');
}
function clearfields() {
    $('#div_dealerinfo').hide();
    $('#ddl_dealers').empty();
    $('#ddl_username').empty().select2();
    $('#ddl_zone').val('0').prop("disabled", false);;
    $('#ddl_dealertype').val('0').prop("disabled", false);
    $('#datepicker').val('');

    $("#chk_zone").prop("checked", false);
    $("#chk_dealertype").prop("checked", false);
}
function AssignDealer() {
    var values = $('#ddl_username').val();

    if (values.length > 0) {
        if ($('#ddl_dealers').val() > 0) {
            if ($('#datepicker').val() != null && $('#datepicker').val() != '') {
                var userids = values.join(",");

                var Data = JSON.stringify({
                    userids: userids,
                    dealerid: $('#ddl_dealers').val(),
                    monthofdealer: $('#datepicker').val()
                });

                new APICALL(GetGlobalURL('Base', 'AssignDealer'), 'POST', Data, true).FETCH((result, error) => {
                    debugger;
                    if (result) {
                        Swal.fire({
                            icon: 'success',
                            title: 'Success...',
                            text: 'Dealer Assigned Successfully!',
                            footer: ''
                        });
                        clearfields();
                    }
                });

            }
            else {
                Swal.fire({
                    icon: 'warning',
                    title: 'Warning...',
                    text: 'Please select month first!',
                    footer: ''
                });
                return false;
            }
        }
        else {
            Swal.fire({
                icon: 'warning',
                title: 'Warning...',
                text: 'Please select dealer first!',
                footer: ''
            });
            return false;

        }
    }
    else {
        Swal.fire({
            icon: 'warning',
            title: 'Warning...',
            text: 'Please select atleast one username!',
            footer: ''
        });
        return false;

    }

}
