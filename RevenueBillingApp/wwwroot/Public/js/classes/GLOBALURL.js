class GLOBALURL {
    constructor() {

    }

    static GetGlobalSignalRURL() {
        var LocalUrl = 'localhost:7201'; //view
        FinalUrl = GetProtocolURL() + LocalUrl + "/signalr";
        return FinalUrl;
    }

    static GetGlobalServiceURL(pServiceName, pMethodName) {
        var LocalUrl = 'localhost:7113'; // apiurl
        FinalUrl = GetProtocolURL() + LocalUrl + "/api/" + pServiceName + "/" + pMethodName;
        return FinalUrl;
    }

    static GetGlobalURL(pControllerName, pMethodName) {
        var LocalUrl = 'localhost:7163';
        FinalUrl = GetProtocolURL() + LocalUrl + "/" + pControllerName + "/" + pMethodName;
        return FinalUrl;
    }

    static GetProtocolURL() {
        var FinalProtocol = "";
        if (document.location.protocol === 'https:') {
            FinalProtocol = 'https://';
        }
        else {
            FinalProtocol = 'http://';
        }
        return FinalProtocol;
    }
}