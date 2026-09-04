//$(document).ready(function () {

//    $('.nav-link.parent').click(function () {
//        $(this).find('.toggleClassParent').toggleClass('bx-chevron-right');
//        $(this).find('.toggleClassParent').toggleClass('bx-chevron-down');
//    });

//    $('.js-example-basic-single').select2();
//});

//document.addEventListener("DOMContentLoaded", function (event) {

//    const showNavbar = (toggleId, navId, bodyId, headerId) => {
//        const toggle = document.getElementById(toggleId),
//            nav = document.getElementById(navId),
//            bodypd = document.getElementById(bodyId),
//            headerpd = document.getElementById(headerId)

//        if (localStorage.getItem('nav') == 'show') {
//            if (!nav.classList.contains('show')) {
//                nav.classList.add('show');
//            }
//        }

//        if (localStorage.getItem('toggle') == "") {
//            toggle.classList.add('bx-toggle-left');
//        }
//        else if (localStorage.getItem('toggle') == 'bx-toggle-right') {
//            toggle.classList.add('bx-toggle-left');
//            toggle.classList.remove('bx-toggle-right');
//        }
//        else if(localStorage.getItem('toggle') == 'bx-toggle-left') {
//            toggle.classList.remove('bx-toggle-left');
//            toggle.classList.add('bx-toggle-right');

//        }

//        if (localStorage.getItem('bodypd') == 'body-pd') {
//            if (!bodypd.classList.contains('body-pd')) {
//                bodypd.classList.add('body-pd');
//            }
//        }
//        // Validate that all variables exist
//        if (toggle && nav && bodypd && headerpd) {
//            toggle.addEventListener('click', () => {
//                // show navbar
//                nav.classList.toggle('show');

//                // change icon
//                toggle.classList.toggle('bx-toggle-right');
//                toggle.classList.toggle('bx-toggle-left');

//                // add padding to body
//                bodypd.classList.toggle('body-pd')
//                // add padding to header

//                if (nav.classList.contains('show')) {
//                    window.localStorage.setItem('nav', 'show');
//                }
//                else {
//                    window.localStorage.setItem('nav', '');
//                }

//                if (toggle.classList.contains('bx-toggle-right')) {
//                    window.localStorage.setItem('toggle', 'bx-toggle-right');
//                }
//                else {
//                    window.localStorage.setItem('toggle', 'bx-toggle-left');
//                }

//                if (bodypd.classList.contains('body-pd')) {
//                    window.localStorage.setItem('bodypd', 'body-pd');
//                }
//                else {
//                    window.localStorage.setItem('bodypd', '');
//                }
//            });
//        }
//    }

//    showNavbar('header-toggle', 'nav-bar', 'body-pd', 'header')

//});

function GetGlobalSignalRURL() {

    var LocalUrl = 'localhost:7201'; //view

    FinalUrl = GetProtocolURL() + LocalUrl + "/signalr";

    return FinalUrl;
}

function GetGlobalServiceURL(pServiceName, pMethodName) {

    var LocalUrl = 'localhost:7163'; // apiurl

    FinalUrl = GetProtocolURL() + LocalUrl + "/api/" + pServiceName + "/" + pMethodName;

    return FinalUrl;
}

function GetGlobalURL(pControllerName, pMethodName) {

    var LocalUrl = 'localhost:7201';

    FinalUrl = GetProtocolURL() + LocalUrl + "/" + pControllerName + "/" + pMethodName;

    return FinalUrl;
}

function GetProtocolURL() {

    var FinalProtocol = "";

    if (document.location.protocol === 'https:') {
        FinalProtocol = 'https://';
    }
    else {
        FinalProtocol = 'http://';
    }

    return FinalProtocol;
}
function ShowLoader(divid) {
    $('#' + divid + ' .loader-wrapper').removeClass('d-none');
}

function HideLoader(divid) {
    $('#' + divid + ' .loader-wrapper').addClass('d-none');
}
function removeToast(id) {
    $('.c_toast_' + id).remove();
}