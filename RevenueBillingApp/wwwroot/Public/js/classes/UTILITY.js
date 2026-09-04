class UTILITY {
    constructor() {

    }

    //#region Static Methods
    static NumericInput(input) {
        var numericKeys = '0123456789';

        // restricts input to numeric keys 0-9
        input.addEventListener('keypress', function (e) {
            var event = e || window.event;
            var target = event.target;

            if (event.charCode == 0) {
                return;
            }

            if (-1 == numericKeys.indexOf(event.key)) {
                // Could notify the user that 0-9 is only acceptable input.
                event.preventDefault();
                return;
            }
        });
    }

    static AddCommas(x) {
        if (!isNaN(x)) {
            return x.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",");
        }
        else {
            return "";
        }
    }

    static RemoveCommas(input) {
        var amount_ = $(input).val().replace(/[,.]/g, '');
        $(input).val(amount_);
    }

    static toLocaleDateString(date) {
        var Date_ = date.getFullYear() + "-" + (((date.getMonth() + 1) < 10) ? "0" + (date.getMonth() + 1) : (date.getMonth() + 1)) + "-" + (((date.getDate()) < 10) ? "0" + (date.getDate()) : (date.getDate()));
        return Date_
    };

    static toLocaleDateTimeString(date) {
        var Date_ = date.getFullYear() + "-" + (((date.getMonth() + 1) < 10) ? "0" + (date.getMonth() + 1) : (date.getMonth() + 1)) + "-" + (((date.getDate()) < 10) ? "0" + (date.getDate()) : (date.getDate()));
        var hourFormat_A = (date.getHours() >= 12) ? 'PM' : 'AM';
        var hourFormat_H = ((date.getHours() > 12) ? date.getHours() - 12 : date.getHours())
        var Time_ = ((hourFormat_H < 10) ? "0" + hourFormat_H : hourFormat_H) + ":" + ((date.getMinutes() < 10) ? "0" + date.getMinutes() : date.getMinutes()) + ":" + ((date.getSeconds() < 10) ? "0" + date.getSeconds() : date.getSeconds()) + " " + hourFormat_A;
        return Date_ + " " + Time_;
    };

    static CheckSession(callback) {

        new APICALL(GetGlobalURL('Base', 'GetSessionForHtml?RouteValues=' + location.pathname), 'GET', '', true).FETCH((result, error) => {

            if (result) {

                if (result.data != null) {

                    callback(result.data);
                }
            }
        });
    }

    static _AccessEventHandler_RolesMapping(elements) {
        $(elements).change(function () {

            var trelement = $(this).closest('tr')[0];
            var view_checked = $(trelement).children('td')[2].getElementsByClassName('View')[0].checked;
            var insert_checked = $(trelement).children('td')[3].getElementsByClassName('Insert')[0].checked;
            var update_checked = $(trelement).children('td')[4].getElementsByClassName('Update')[0].checked;
            var delete_checked = $(trelement).children('td')[5].getElementsByClassName('Delete')[0].checked;

            if (insert_checked && !view_checked) {
                var Insert = $(trelement).children('td')[3].getElementsByClassName('Insert')[0];
                var Update = $(trelement).children('td')[4].getElementsByClassName('Update')[0];
                var Delete = $(trelement).children('td')[5].getElementsByClassName('Delete')[0];
                $(Insert).prop('checked', false);
                $(Update).prop('checked', false);
                $(Delete).prop('checked', false);
                return;
            }

            if (update_checked && !view_checked) {
                var Insert = $(trelement).children('td')[3].getElementsByClassName('Insert')[0];
                var Update = $(trelement).children('td')[4].getElementsByClassName('Update')[0];
                var Delete = $(trelement).children('td')[5].getElementsByClassName('Delete')[0];
                $(Insert).prop('checked', false);
                $(Update).prop('checked', false);
                $(Delete).prop('checked', false);
                return;
            }

            if (delete_checked && !view_checked) {
                var Insert = $(trelement).children('td')[3].getElementsByClassName('Insert')[0];
                var Update = $(trelement).children('td')[4].getElementsByClassName('Update')[0];
                var Delete = $(trelement).children('td')[5].getElementsByClassName('Delete')[0];
                $(Insert).prop('checked', false);
                $(Update).prop('checked', false);
                $(Delete).prop('checked', false);
                return;
            }

            if (!view_checked) {
                var Insert = $(trelement).children('td')[3].getElementsByClassName('Insert')[0];
                var Update = $(trelement).children('td')[4].getElementsByClassName('Update')[0];
                var Delete = $(trelement).children('td')[5].getElementsByClassName('Delete')[0];
                $(Insert).prop('checked', false);
                $(Update).prop('checked', false);
                $(Delete).prop('checked', false);
                return;
            }
        });

    }

    static _Percentage(value) {
        return Number(value * 100).toFixed(2) + "%";
    }
}