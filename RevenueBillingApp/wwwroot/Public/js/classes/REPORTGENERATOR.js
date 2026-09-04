function FillReportTableWithTotal(TblReport, data, ColCountForSum) {

    var columns = [];
    $.each(data[0], function (key, value) {
        var my_item = {};
        my_item.title = key;
        my_item.data = key;
        columns.push(my_item);

    });

    if ($.fn.DataTable.isDataTable("#TblReport")) {
        $('#TblReport').DataTable().clear().destroy();
        $('#TblReport').empty();
        $('#TblReport').html('');
    }

    $('#TblReport').DataTable({
        'paging': false,
        "lengthMenu": [[100, 200, 400, 800, -1], [100, 200, 400, 800, "All"]],
        "pageLength": 100,
        'lengthChange': false,
        'searching': false,
        'ordering': false,
        'info': false,
        'autoWidth': false,
        "destroy": true,
        "columns":columns,
        "data": data,
        dom: 'Bfrtip',
        buttons: [
            'copyHtml5',
            'excelHtml5',
            'csvHtml5',
            'pdfHtml5',
            'print'
        ]
    });

    var LastColumns = ColCountForSum;
    var TotalColumns = columns.length;
    var ColSpanCount = (TotalColumns - LastColumns);

    var TrFooter = '<tfoot>';
    TrFooter = TrFooter + '<th class="text-end" colspan="' + ColSpanCount + '">Total:</th>';

    for (i = 0; i <= LastColumns - 1; i++) {
        var ColIndex = (ColSpanCount + i);
        TrFooter = TrFooter + '<th class="text-end"><span id="f' + columns[ColIndex].data + '">0</span></th>';
    }
    $('#TblReport').append(TrFooter);
    $('#TblReport > tbody  > tr').each(function (index, tr) {
        var currentRow = $(this).closest("tr");
        for (i = 0; i <= LastColumns - 1; i++) {
            var ColIndex = (ColSpanCount + i);
            var val2 = currentRow.find("td:eq(" + ColIndex + ")").text();
            if (val2 != "") {
                var val1 = $('#f' + columns[ColIndex].data).text();
                val1 = (parseInt(val1) + parseInt(val2));
                $('#f' + columns[ColIndex].data).text(val1);
            }
        }
    });
    TrFooter = TrFooter + '</tfoot>';
}

function FillReportTable(TblReport, data) {

    var columns = [];

    $('#' + TblReport).html('');
    if ($.fn.DataTable.isDataTable('#' + TblReport)) {
        $('#' + TblReport).DataTable().clear().destroy();
        $('#' + TblReport).empty('');
        $('#' + TblReport).html('');
    }

    if (data != [] && data != undefined && data.length > 0) {

        $.each(data[0], function (key, value) {
            var my_item = {};
            my_item.title = key;
            my_item.data = key;
            columns.push(my_item);

        });

        if (data.length > 0) {
            $('#' + TblReport).DataTable({
                'paging': true,
                "lengthMenu": [[50, 100, 200, 400, 800, -1], [50, 100, 200, 400, 800, "All"]],
                "pageLength": 50,
                'lengthChange': false,
                'searching': false,
                'ordering': false,
                'info': false,
                'autoWidth': false,
                "destroy": true,

                "columns":
                    columns
                ,
                "data": data,
                dom: 'Bfrtip',
                buttons: [
                    'copyHtml5',
                    'excelHtml5',
                    'csvHtml5',
                    'pdfHtml5',
                    'print'
                ]
            });
        }
        else {
            $('#' + TblReport).html('<tr><th class="text-center cblack" align="center">No record found</th></tr>');
        }
    }
    else {
        $('#' + TblReport).html('<tr><th class="text-center cblack" align="center">No record found</th></tr>');
    }
}