$(document).ready(function () {
    // Default active tab: Download
    $('#tabDownload').addClass('active');
    $('#download').addClass('show active');
    $('#upload').removeClass('show active');


    setDateFields();
    LoadZone_DDL();

    $('#btnDownloadExcel').click(function () {
        handleExcelAction('D'); // D = Download
    });

    $('#btnUploadExcel').click(function () {
        handleExcelAction('U'); // U = Upload
    });

    // Change button color dynamically when switching tabs
    $('a[data-bs-toggle="pill"]').on('shown.bs.tab', function (e) {
        const target = $(e.target).attr('href'); // #download or #upload
        if (target === '#upload') {
            $('#btnUploadExcel').removeClass('btn-primary').addClass('btn-danger');
        } else if (target === '#download') {
            $('#btnDownloadExcel').removeClass('btn-danger').addClass('btn-primary');
        }
    });

});

function setDateFields() {
    const date = new Date();
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const formattedMonth = `${year}-${month}`;

    document.getElementById('dp_DownloadDate').value = formattedMonth;
    document.getElementById('dp_UploadDate').value = formattedMonth;
}
function LoadZone_DDL() {
    new APICALL(GetGlobalURL('Base', 'LoadZone_DDL'), 'GET', '', true).FETCH((result, error) => {
        if (result) {
            if (result.data != null && result.data.length > 0) {
                $('#ddl_zone').empty(); // Clear existing options
                $('#ddl_zone').append('<option selected="true" value="0"> Select Zone</option>'); // Add default option

                $.each(result.data, function (i, option) {
                    $('#ddl_zone').append(
                        `<option value="${option.UserId}"> ${option.UserName}</option>`
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


function validateFields(tab) {
    if (tab === 'D') {
        const month = $('#dp_DownloadDate').val();
        const zone = $('#ddl_zone').val();

        if (!month) {
            Swal.fire({ icon: 'warning', text: 'Please select a calendar month.' });
            return false;
        }

        if (zone === '0' || !zone) {
            Swal.fire({ icon: 'warning', text: 'Please select a zone.' });
            return false;
        }

    } else if (tab === 'U') {
        const month = $('#dp_UploadDate').val();
        const file = $('#file_uploader').val();

        if (!month) {
            Swal.fire({ icon: 'warning', text: 'Please select a calendar month.' });
            return false;
        }

        if (!file) {
            Swal.fire({ icon: 'warning', text: 'Please select a file to upload.' });
            return false;
        }
    }

    return true;
}

// Unified Excel Action Handler
function handleExcelAction(actionType) {
    // Validate inputs before continuing
    if (!validateFields(actionType)) {
        return;
    }

    var monthVal = (actionType === 'D') ? $('#dp_DownloadDate').val() : $('#dp_UploadDate').val();
    var zoneId = $('#ddl_zone').val();

    // Additional check for upload (require file)
    if (actionType === 'U') {
        var fileVal = $('#file_uploader').val();
        if (!fileVal) {
            Swal.fire({ icon: 'warning', text: 'Please select an Excel file to upload.' });
            return;
        }
        // Perform actual upload logic here if needed
        console.log('Uploading file: ', fileVal); // Placeholder
    }

    callExcelApi(monthVal, actionType, zoneId);
}

// Reusable API Call Function
function callExcelApi(monthVal, type, zoneId) {
    var [year, month] = monthVal.split('-').map(Number);

    var requestObj = {
        Monthd: month,
        Year: year,
        Zone: parseInt(zoneId),
        type: type,
        ShowExcel: true,
        ShowLogs: false
    };

    if (type === 'D') {
        $.ajax({
            type: "POST",
            url: keys + 'Base/DownloadExcel',
            data: JSON.stringify(requestObj),
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (response) {
                console.log("SUCCESS RESPONSE:", response);

                // All Excel headers as per your attached file + new Frequency column
                const headers = [
                    "SPOID",
                    "TerritoryID",
                    "Dealer Code",
                    "Dealer Name",
                    "Contact Person Name",
                    "Main Group",
                    "Type",
                    "Address",
                    "City",
                    "Mobile Number",
                    "Frequency",   // <-- new column
                    "KOL",
                    "Status",
                    "Potential",
                    "Propensity",
                    "Flag",
                    "SPO Name",
                    "Brand",
                    "Market_Name"
                ];


                // Build hidden HTML table
                let tableHtml = '<table id="pdfDatatable" style="display:none">';

                // Insert NOTE row at the very top (before header row)
                tableHtml += `<tr><td colspan="19" style="font-weight:bold; color:#000; background-color:#FFFF99;">
                                NOTE: N= New Dealer U= Update Dealer D= Delete Dealer C=Continue (Flag must be provided, Otherwise System will not accept the record)
                                </td></tr>`;

                // Now add headers
                tableHtml += '<thead><tr>';
                headers.forEach(h => { tableHtml += `<th>${h}</th>`; });
                tableHtml += '</tr></thead><tbody>';


                if (response.statusCode === 200 && response.data && response.data.length > 0) {
                    console.log('Download success');

                    // Map API data into each column — keep blank if not available
                    response.data.forEach(item => {
                        tableHtml += `<tr>
                        <td>${item.SPOID || ''}</td>
                        <td>${item.TerritoryID || ''}</td>
                        <td>${item.DealerCode || ''}</td>
                        <td>${item.DealerName || ''}</td>
                        <td>${item.ContactPersonName || ''}</td>
                        <td>${item.MainGroup || ''}</td>
                        <td>${item.Type || ''}</td>
                        <td>${item.Address || ''}</td>
                        <td>${item.City || ''}</td>
                        <td>${item.MobileNumber || ''}</td>
                        <td>${item.Frequency || ''}</td> 
                        <td>${item.KOL || ''}</td>
                        <td>${item.STATUS || ''}</td>
                        <td>${item.Potential || ''}</td>
                        <td>${item.Propensity || ''}</td>
                        <td>${item.Flag || ''}</td>
                        <td>${item.SPONAME || ''}</td>
                        <td>${item.Brand || ''}</td>
                        <td>${item.Market_Name || ''}</td>
                    </tr>`;
                    });
                } else {
                    ////Blank Excel (no data)
                    ////tableHtml += '<tr><td colspan="19" style="text-align:center;">No Data</td></tr>';
                    tableHtml += '';
                }

                tableHtml += '</tbody></table>';

                // Remove old table if exists
                $('#pdfDatatable').remove();

                // Append new table to body
                $('body').append(tableHtml);


                // Convert monthVal (YYYY-MM) → "October2025"
                const [year, month] = monthVal.split('-').map(Number);
                const monthNames = [
                    "January", "February", "March", "April", "May", "June",
                    "July", "August", "September", "October", "November", "December"
                ];
                const monthName = monthNames[month - 1];
                const formattedName = `DealerList-${monthName}${year}`;


                // Use your general export function
                exportTableToExcel('pdfDatatable', formattedName);

                // Clear form after successful download
                ClearDealerForm();

            },
            error: function () {
                console.log("❌ API Error", status, error);
                console.log("Response Text:", xhr.responseText);
                Swal.fire({ icon: 'error', text: 'An error occurred during the request.' });
            },
            async: false
        });
    }

    // Future-proof: handle 'U' (upload) logic here if backend API exists
    // upload logic here ...
    if (type === 'U') {
        const fileInput = document.getElementById('file_uploader');
        const file = fileInput.files[0];
        if (!file) {
            Swal.fire({ icon: 'warning', text: 'Please select a file to upload.' });
            return;
        }

        const [year, month] = monthVal.split('-').map(Number);

        // Build FormData because we’re sending a file + JSON
        const formData = new FormData();
        formData.append("Monthd", month);
        formData.append("Year", year);
        formData.append("Zone", parseInt(zoneId));
        formData.append("type", "U");
        formData.append("ShowExcel", true);
        formData.append("ShowLogs", false);
        formData.append("file", file); // Attach Excel file

        $.ajax({
            url: keys + 'SPODealer/UploadExcel',
            type: "POST",
            data: formData,
            processData: false, // Important for FormData
            contentType: false, // Important for FormData
            success: function (response) {
                if (response.statusCode === 200) {
                    Swal.fire({
                        icon: 'success',
                        title: 'Upload Successful!',
                        text: response.responseMsg || 'File processed successfully.'
                    }).then(() => {
                        ClearDealerForm();
                    });

                } else {
                    Swal.fire({
                        icon: 'error',
                        title: 'Upload Failed',
                        text: response.responseMsg || 'Something went wrong!'
                    });
                }
            },
            error: function (xhr, status, error) {
                console.error("Upload error:", xhr.responseText);
                Swal.fire({
                    icon: 'error',
                    title: 'Error!',
                    text: 'Error during upload request.'
                });
            }
        });

    }


}
function formatDate(dateValue) {
    if (!dateValue) return '';
    const d = new Date(dateValue);
    const year = d.getFullYear();
    const month = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');
    const hours = String(d.getHours()).padStart(2, '0');
    const minutes = String(d.getMinutes()).padStart(2, '0');
    return `${year}-${month}-${day}T${hours}:${minutes}`;
}
function ClearDealerForm() {
    // Clear the date pickers - reset to current month
    const date = new Date();
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const formattedMonth = `${year}-${month}`;

    $('#dp_DownloadDate').val(formattedMonth);
    $('#dp_UploadDate').val(formattedMonth);

    // Reset zone dropdown to default ("Select Zone")
    $('#ddl_zone').val('0');

    // Clear file input
    $('#file_uploader').val('');

    // Optionally, remove generated table if any
    $('#pdfDatatable').remove();
}
function exportTableToExcel(tableId, filename = '') {
    const table = document.getElementById(tableId);
    if (table && table.rows.length > 1) {
        const ws = XLSX.utils.table_to_sheet(table);
        const wb = XLSX.utils.book_new();
        XLSX.utils.book_append_sheet(wb, ws, 'Sheet1');

        // Hide the first column (SPOID)
        ws['!cols'] = ws['!cols'] || [];
        ws['!cols'][0] = { hidden: true }; // Hide first column (index 0)

        XLSX.writeFile(wb, filename ? `${filename}.xlsx` : 'DealerList.xlsx');
    } else {
        Swal.fire({
            icon: 'error',
            title: 'Oops...',
            text: 'No Data Available to Export',
        });
    }
}
