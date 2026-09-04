$(document).ready(function () {
    GetAllUsers_DDL();
    GetOrganizationChart();
    //GetOrganizationHierarchy();

    $('#SaveBtn').on('click', function () {
        SaveOrganizationChart();
    });
    $('#UpdateBtn').on('click', function () {
        UpdateOrgChart();
    });
    $("#employeeid").select2({
        placeholder: "Select Employee(s)",
        allowClear: true
    });
});
$(document).on('change', '#managerid', function () {
    const selectedManagerId = $(this).val();
    
    populateDropdowns(window.allUsers, selectedManagerId);
});
const modals = document.querySelectorAll('.modal');
modals.forEach(modal => {
    modal.addEventListener('show.bs.modal', (e) => {
        const openedModals = document.querySelectorAll('.modal.show');
        openedModals.forEach(openModal => {
            openModal.style.zIndex = 1040 + (10 * (openedModals.length + 1)); 
        });

    });

    modal.addEventListener('hidden.bs.modal', () => {
        const backdrop = document.querySelector('.modal-backdrop');
        if (document.querySelectorAll('.modal.show').length === 0 && backdrop) {
            backdrop.remove(); 
        }
    });
});
function GetAllUsers_DDL() {
    new APICALL(GetGlobalURL('Base', 'GetAllUsers'), 'GET', '', true).FETCH((result, error) => {

        if (result) {
            if (result.data != null) {
               
                window.allUsers = result.data;

                populateDropdowns(result.data);
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

function SaveOrganizationChart() {
    
    Swal.fire({
        title: 'Do you want to save the changes?',
        showDenyButton: true,
        showCancelButton: false,
        confirmButtonText: 'Ok',
        denyButtonText: 'Cancel',
    }).then((result) => {
        if (result.isConfirmed) {
            $('#s-active').val($('#s-active').prop('checked'));
            $('#isedit').val('0');
            var Data = $('#organizationchart').serialize();

            new APICALL(GetGlobalURL('Base', 'SaveOrganizationChart'), 'POST', Data, true, false, 'application/x-www-form-urlencoded').FETCH((result, error) => {

                if (result) {
                    ResetControls();
                    GetOrganizationChart();
                    Swal.fire({
                        icon: 'success',
                        title: 'Success...',
                        text: 'Chart Saved Successfully!',
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

function GetOrganizationChart() {

            new APICALL(GetGlobalURL('Base', 'GetOrganizationChart'), 'GET', '', true).FETCH((result, error) => {

                if (result) {
                    if ($.fn.DataTable.isDataTable('#orgchartmodalTable')) {
                        $('#orgchartmodalTable').DataTable().clear().destroy();
                    }
                    if (result.data != null) {
                        
                        $.each(result.data, function (i, option) {
                           
                            var viewBtn = '<td><div class="btn-group btn-group-sm"><a class="avatar-text avatar-md ViewUser" data-bs-toggle="modal" data-bs-target="#orgchartmodal" data-value="' + option.managerid + '" title="View"><i class="fa-solid fa-eye"></i></a></div></td>';
                            var UpdateBtn = '<td><button class="btn btn-sm btn-white btn-block EditOrgChart" data-value="' + option.managerid + '" type="button"><i class="bx bxs-edit"></i></button></td>' ;

                            $('#organizationcharttable tbody').append(
                                '<tr id="rowid-' + option.id + '">' +
                                
                                //'<td>' + option.organizationcode + '</td>' +
                                '<td>' + option.organizationname + '</td>' +
                                '<td>' + option.managername + '</td>' +
                                //'<td>' + option.employeename + '</td>' +
                                //'<td>' + formatDateTime(option.createddatetime) + '</td>' +
                                viewBtn +
                                UpdateBtn +
                                '</tr>'
                            );
                        });
                        $('.ViewUser').on('click', function () {
                            GetEmployeeListByManagerID(this.attributes["data-value"].value);
                        });
                        $('.EditOrgChart').on('click', function () {
                            EditOrgChart(this.attributes["data-value"].value);
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

function formatDateTime(dateStr) {
    if (!dateStr) return '';

    const date = new Date(dateStr);

    const day = String(date.getDate()).padStart(2, '0');
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const year = date.getFullYear();

    const hours = String(date.getHours()).padStart(2, '0');
    const minutes = String(date.getMinutes()).padStart(2, '0');

    return `${day}-${month}-${year} ${hours}:${minutes}`;
}



function populateDropdowns(users, selectedManagerId = null) {
    $('#managerid').empty().append('<option value="">-- Select Manager --</option>');
    $('#employeeid').empty().append('<option value="">-- Select Employee --</option>');

    $.each(users, function (i, option) {
        $('#managerid').append(
            `<option value="${option.userid}">${option.username}</option>`
        );
    });

   
    if (selectedManagerId) {
        $('#managerid').val(selectedManagerId);
    }

    $.each(users, function (i, option) {
        if (option.userid != selectedManagerId) {
            $('#employeeid').append(
                `<option value="${option.userid}">${option.username}</option>`
            );
        }
    });
}


function ResetControls() {

    $('#organizationname').val('');
    $('#managerid').val('');
    $('#employeeid').val('').trigger('change');
    $('#s-active').val('');



    if ($('.savebutton').hasClass('d-none')) {
        $('.updatebutton').toggleClass('d-none');
        $('.savebutton').toggleClass('d-none');
    }
}

function GetEmployeeListByManagerID(managerid) {

    new APICALL(GetGlobalURL('Base', 'GetEmployeeListByManagerID?managerid=' + managerid), 'GET', '', true).FETCH((result, error) => {

                $('#orgchartmodalTable tbody').empty();
            if (result.data != null) {
                $('#orgchartmodahead').text('Manager' + ' ' + result.data[0].managername)
                $.each(result.data, function (i, option) {

                  
                    $('#orgchartmodalTable tbody').append(
                        '<tr id="rowid-' + option.employeeid + '">' +
                        '<td>' + option.employeename + '</td>' +
                        '</tr>'
                    );
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



function EditOrgChart(managerid) {
    new APICALL(GetGlobalURL('Base', 'GetEmployeeListByManagerID?managerid=' + managerid), 'GET', '', true).FETCH((result, error) => {

        if (result) {

            if (result.data.length > 0) {

                var orgname = result.data[0].organizationname;
                var managerid = result.data[0].managerid;
                //var employeeids = result.data[0].employeeids;
                var isactive = result.data[0].isactive;
                const employeeids = result.data.map(row => row.employeeid);

                $('#organizationname').val(orgname);
                $('#managerid').val(managerid);
                $('#employeeid').val(employeeids).trigger('change');
                $('#s-active').prop('checked', isactive);

                if ($('.updatebutton').hasClass('d-none')) {
                    $('.savebutton').toggleClass('d-none');
                    $('.updatebutton').toggleClass('d-none');
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


function UpdateOrgChart() {
    $('#organizationcharttable').DataTable().clear().destroy();
    Swal.fire({
        title: 'Do you want to save the changes?',
        showDenyButton: true,
        showCancelButton: false,
        confirmButtonText: 'Ok',
        denyButtonText: 'Cancel',
    }).then((result) => {
        if (result.isConfirmed) {
            $('#s-active').val($('#s-active').prop('checked'));
            $('#isedit').val('1');
            var Data = $('#organizationchart').serialize();

            new APICALL(GetGlobalURL('Base', 'SaveOrganizationChart'), 'POST', Data, true, false, 'application/x-www-form-urlencoded').FETCH((result, error) => {

                if (result) {
                    GetOrganizationChart();
                    ResetControls();


                    Swal.fire({
                        icon: 'success',
                        title: 'Success...',
                        text: 'Organization Chart Updated Successfully!',
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


