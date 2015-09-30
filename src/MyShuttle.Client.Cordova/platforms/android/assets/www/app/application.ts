﻿/// <reference path="./config.ts" />

// For an introduction to the Blank template, see the following documentation:
// http://go.microsoft.com/fwlink/?LinkID=397704
// To debug code on page load in Ripple or on Android devices/emulators: launch your app, set breakpoints, 
// and then run "window.location.reload()" in the JavaScript Console.
(function () {
    //Start by defining the main module and adding the module dependencies
    angular.module(ApplicationConfiguration.applicationName, ApplicationConfiguration.applicationModuleVendorDependencies);

    document.addEventListener('deviceready', onDeviceReady.bind(this), false);

    //$(document).ready(function() {
    //    angular.bootstrap(document, [ApplicationConfiguration.applicationName]);
    //});

    function onDeviceReady() {
        // Handle the Cordova pause and resume events
        document.addEventListener('pause', onPause.bind(this), false);
        document.addEventListener('resume', onResume.bind(this), false);
        
        // Then init the app
        angular.bootstrap(document, [ApplicationConfiguration.applicationName]);
    };

    function onPause() {
        // TODO: This application has been suspended. Save application state here.
    };

    function onResume() {
        // TODO: This application has been reactivated. Restore application state here.
    };
})();