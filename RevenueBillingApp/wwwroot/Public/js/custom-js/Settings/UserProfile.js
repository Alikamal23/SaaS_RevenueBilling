$(document).ready(function () {
    $("#profilemasterform").validate();
});

$(document).on("change", ".uploadProfileInput", function () {
    var triggerInput = this;
    var currentImg = $(this).closest(".pic-holder").find(".pic").attr("src");
    var holder = $(this).closest(".pic-holder");
    var wrapper = $(this).closest(".profile-pic-wrapper");
    $(wrapper).find('[role="alert"]').remove();
    triggerInput.blur();
    var files = !!this.files ? this.files : [];
    if (!files.length || !window.FileReader) {
        return;
    }
    if (/^image/.test(files[0].type)) {
        // only image file
        var reader = new FileReader(); // instance of the FileReader
        reader.readAsDataURL(files[0]); // read the local file

        reader.onloadend = function () {
            $(holder).addClass("uploadInProgress");
            $(holder).find(".pic").attr("src", this.result);
        };
    } else {
        $(wrapper).append(
            '<div class="alert alert-danger d-inline-block p-2 small" role="alert">Please choose the valid image.</div>'
        );
        setTimeout(() => {
            $(wrapper).find('role="alert"').remove();
        }, 3000);
    }
});

$(document).on("click", ".savebutton", function () {
    if (!$('#profilemasterform').valid()) {
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
            ShowLoader('ProfileMasterDiv');
            if (window.FormData !== undefined) {

                var fileData = new FormData($('#profilemasterform')[0]);

                new APICALL(GetGlobalURL('Base', 'UpdateProfile'), 'POST', fileData, true, true).FETCH((result, error) => {

                    if (result) {
                        Swal.fire({
                            icon: 'success',
                            title: 'Success...',
                            text: 'Please re-login to view changes!',
                            footer: ''
                        });
                        HideLoader('ProfileMasterDiv');
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
                alert("FormData is not supported.");
                HideLoader('ProfileMasterDiv');
            }
        }
    });
});