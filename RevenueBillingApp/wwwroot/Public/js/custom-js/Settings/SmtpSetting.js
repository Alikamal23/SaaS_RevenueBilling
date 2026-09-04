$(document).ready(function () {
    ////GetSmtpGrid();
    SmtpSettingGet();

    $('#Smtptogglepassword').on('click', function (e) {
        var type = $('#SmtpPassword').attr('type') === 'password' ? 'text' : 'password';
        $('#SmtpPassword').attr('type', type);
        $('#Smtptogglepassword i').toggleClass('bx-show');
        $('#Smtptogglepassword i').toggleClass('bx-hide');
    });

    $('#UpdateBtn').on('click', function () {
        UpdateSmpt();
    });

});

function SmtpSettingGet() {
    new APICALL(GetGlobalURL('Base', 'SmtpSettingGet'), 'GET', '', true).FETCH((result, error) => {
        if (result) {
            if (result.data != null) {
                var SSLvalue = result.data[0].EnableSSL;
                $('#Smtp').val(result.data[0].Smtp);
                $('#SenderEmailID').val(result.data[0].SenderEmailID);
                $('#SmtpPassword').val('');
                $('#SmtpPort').val(result.data[0].SmtpPort);
                $('#DisplayName').val(result.data[0].DisplayName);

                if (SSLvalue == true) {
                    $('#EnableSSL').prop('checked', true);
                }
                else {
                    $('#EnableSSL').val('').prop('checked', false);
                }

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
function UpdateSmpt() {
    if (!$('#smtpform').valid()) {
        return false;
    }

    var PkId = 1;
    var Smtp = $("#Smtp").val();
    var SenderEmailID = $("#SenderEmailID").val();
    var SmtpPassword = $("#SmtpPassword").val();
    var SmtpPort = $("#SmtpPort").val();
    var DisplayName = $("#DisplayName").val();
    var EnableSSL = $("#EnableSSL").prop("checked");

    var model = new Object();
    model.ID = PkId;
    model.Smtp = Smtp;
    model.SenderEmailID = SenderEmailID;
    model.SmtpPassword = SmtpPassword;
    model.SmtpPort = SmtpPort;
    model.DisplayName = DisplayName;
    model.EnableSSL = EnableSSL;
    model.UserID = window.UserID;

    Swal.fire({
        title: 'Do you want to save the changes?',
        showDenyButton: true,
        showCancelButton: false,
        confirmButtonText: 'Ok',
        denyButtonText: 'Cancel',
    }).then((result) => {
        if (result.isConfirmed) {

            new APICALL(GetGlobalURL('Base', 'SmtpSettingUpdate'), 'POST', JSON.stringify(model), true).FETCH((result, error) => {
                if (result) {
                    Swal.fire({
                        icon: 'success',
                        title: 'Success...',
                        text: 'SMTP setting Saved Successfully!',
                        footer: ''
                    });
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
function GetSmtpGrid() {
    ShowLoader('SmtpMasterDiv');

    UTILITY.CheckSession((data_) => {
        if (data_) {

            if ($.fn.DataTable.isDataTable('#role-master')) {
                $('#role-master').DataTable().destroy();
            }

            $('#role-master tbody').html('');

            new APICALL(GetGlobalURL('Base', 'GetSmtpGrid'), 'GET', '', true).FETCH((result, error) => {
                if (error) {
                    Swal.fire({
                        icon: 'error',
                        title: 'Error',
                        text: error.data.responseText
                    });
                    HideLoader('SmtpMasterDiv');
                    return;
                }

                if (result && result.data != null) {
                    $.each(result.data, function (i, option) {

                        var SSL = option.EnableSSL
                            ? '<input type="checkbox" disabled checked class="checkboxsize"/>'
                            : '<input type="checkbox" disabled class="checkboxsize"/>';

                        var DefaultBtn = "";

                        if (option.Active) {

                            DefaultBtn = '<button class="btn btn-success btn-sm" disabled>' +
                                '<i class="feather-check"></i> Default</button>';

                        }
                        else {

                            DefaultBtn = '<button class="btn btn-primary btn-sm SetDefaultSMTP" data-id="' + option.ID + '">' +
                                '<i class="feather-star"></i> Set Default</button>';
                        }

                        $('#role-master tbody').append(
                            '<tr>' +
                            '<td>' + option.Smtp + '</td>' +
                            '<td>' + option.SenderEmailID + '</td>' +
                            '<td>********</td>' +
                            '<td>' + option.SmtpPort + '</td>' +
                            '<td>' + option.DisplayName + '</td>' +
                            '<td class="text-center">' + SSL + '</td>' +
                            '<td>' + DefaultBtn + '</td>' +
                            '</tr>'
                        );

                    });

                    $('#role-master').DataTable();

                    $('.SetDefaultSMTP').click(function () {
                        var id = $(this).data('id');
                        SetDefaultSMTP(id);
                    });

                }

                HideLoader('SmtpMasterDiv');

            });

        }

    });

}
function SetDefaultSMTP(id) {
    //alert(id);

    Swal.fire({
        title: 'Set Default SMTP?',
        text: 'Only one SMTP account can remain active.',
        icon: 'question',
        showCancelButton: true,
        confirmButtonText: 'Yes'
    }).then((result) => {
        if (result.isConfirmed) {
            var obj = {
                ID: id
            };

            new APICALL(GetGlobalURL('Base', 'SetDefaultSMTP'), 'POST', obj, true).FETCH((result, error) => {
                if (error) {
                    Swal.fire('Error', error.data.responseText, 'error');
                    return;
                }

                Swal.fire('Success', 'Default SMTP updated successfully.', 'success');

                GetSmtpGrid();

            });

        }

    });

}
