class APICALL {

    constructor(URL, METHOD, DATA, ASYNC, FILEUPLOAD, CONTENTTYPE, LOADER) {
        this.URL = URL;
        this.METHOD = METHOD;
        this.DATA = DATA;
        this.ASYNC = ASYNC;
        this.FILEUPLOAD = (FILEUPLOAD == undefined) ? false : FILEUPLOAD;
        this.CONTENTTYPE = (CONTENTTYPE == undefined) ? 'application/json' : CONTENTTYPE;
        this.LOADER = (LOADER == undefined) ? false : LOADER;
    }

    FETCH(CALLBACK) {
    
        var URL = this.URL;
        var METHOD = this.METHOD;
        var DATA = this.DATA;
        var ASYNC = this.ASYNC;
        var FILEUPLOAD = (this.FILEUPLOAD == undefined) ? false : FILEUPLOAD;
        var CONTENTTYPE = (this.CONTENTTYPE == undefined) ? 'application/json' : CONTENTTYPE;
        var LOADER = (this.LOADER == undefined) ? false : LOADER;

        $('.loader-wrapper').removeClass('d-none');

        if (this.METHOD == "POST" && !this.FILEUPLOAD) {
            $.ajax({
                url: this.URL,
                type: this.METHOD,
                async: this.ASYNC,
                data: this.DATA,
                contentType: this.CONTENTTYPE,
                success: function (data, status) {
                    var obj = {
                        data: data,
                        status: status
                    }

                    CALLBACK(obj, null);
                    $('.loader-wrapper').addClass('d-none');
                },
                error: function (err, status) {
                        var obj = {
                            data: err,
                            status: status
                        }

                    CALLBACK(null, obj);
                    $('.loader-wrapper').addClass('d-none');
                }
            });
        }

        if (this.METHOD == "GET" && !this.FILEUPLOAD) {
            $.ajax({
                url: this.URL,
                type: this.METHOD,
                async: this.ASYNC,
                success: function (data, status) {
                    var obj = {
                        data: data,
                        status: status
                    }

                    CALLBACK(obj, null);
                    $('.loader-wrapper').addClass('d-none');
                },
                error: function (err, status) {
                        var obj = {
                            data: err,
                            status: status
                        }
                    CALLBACK(null, obj);
                    $('.loader-wrapper').addClass('d-none');
                }
            });
        }

        if (this.METHOD == "POST" && this.FILEUPLOAD) {
            //console.log(this.DATA)
            $.ajax({
                url: this.URL,
                type: this.METHOD,
                async: this.ASYNC,
                data: this.DATA,
                contentType: false,
                processData: false,
                success: function (data, status) {
                    var obj = {
                        data: data,
                        status: status
                    }

                    CALLBACK(obj, null);
                    $('.loader-wrapper').addClass('d-none');
                },
                error: function (err, status) {
                        var obj = {
                            data: err,
                            status: status
                        }
                    CALLBACK(null, obj);
                    $('.loader-wrapper').addClass('d-none');
                }
            });
        }

        this.URL = undefined;
        this.METHOD = undefined;
        this.DATA = undefined;
        this.ASYNC = undefined;

    }

    static RECALL(URL, METHOD, DATA, ASYNC, FILEUPLOAD, CONTENTTYPE, CALLBACK) {
    
        if (METHOD == "POST" && !FILEUPLOAD) {
            $.ajax({
                url: URL,
                type: METHOD,
                async: ASYNC,
                data: DATA,
                contentType: CONTENTTYPE,
                success: function (data, status) {
                    var obj = {
                        data: data,
                        status: status
                    }

                    CALLBACK(obj, null);
                },
                error: function (err, status) {

                }
            });
        }

        if (METHOD == "GET" && !FILEUPLOAD) {
            $.ajax({
                url: URL,
                type: METHOD,
                async: ASYNC,
                success: function (data, status) {
                    var obj = {
                        data: data,
                        status: status
                    }

                    CALLBACK(obj, null);
                },
                error: function (err, status) {
                }
            });
        }

        if (METHOD == "POST" && FILEUPLOAD) {
            $.ajax({
                url: URL,
                type: METHOD,
                async: ASYNC,
                data: DATA,
                contentType: false,
                processData: false,
                success: function (data, status) {
                    var obj = {
                        data: data,
                        status: status
                    }

                    CALLBACK(obj, null);
                },
                error: function (err, status) {

                }
            });
        }
    }
}