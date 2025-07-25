var Jarvis = Jarvis || {};
Jarvis.CCC = {
    onLoad: function (executionContext) {
        "use strict";
        Jarvis.CCC.populateRD(executionContext);
        Jarvis.CCC.populateHD(executionContext);
    },

    // function RetrieveStorageValue(executionContext) {
    // 	"use strict";
    //     var formContext = executionContext.getFormContext();
    //     var repairingDealer = localStorage.getItem("RepaingDealer");
    //     var homeDealer = localStorage.getItem("HomeDealer");
    //     var repairingDealerName = localStorage.getItem("RepaingDealerName");
    //     var homeDealerName = localStorage.getItem("HomeDealerName");
    //     var arr = new Array();
    //     arr[0] = new Object();
    //     if (repairingDealer !== null && repairingDealer !== "") {
    //         arr[0].id = repairingDealer;
    //         arr[0].name = repairingDealerName;
    //         arr[0].entityType = "jarvis_passout";
    //         formContext.getAttribute("jarvis_repairingdealerpassout").setValue(arr);
    //     }

    //     if (homeDealer !== null && homeDealer != "") {
    //         arr = new Array();
    //         arr[0] = new Object();
    //         arr[0].id = homeDealer;
    //         arr[0].name = homeDealerName;
    //         arr[0].entityType = "account";
    //         formContext.getAttribute("jarvis_homedealer").setValue(arr);
    //     }
    //     //localStorage.removeItem("RepaingDealer");
    //     //localStorage.removeItem("HomeDealer");
    // }
    populateHD: function (executionContext) {
        "use strict";
        var formContext = executionContext.getFormContext(); //jarvis_incident
        var incident = formContext.getAttribute("jarvis_incident").getValue();
        var hd = formContext.getAttribute("jarvis_homedealer").getValue();
        var rd = formContext.getAttribute("jarvis_repairingdealer").getValue();
        if (hd !== null || rd !== null) {

        }
        else {
            if (incident !== null) {
                var parentCaseID = incident[0].id.toString().replace('{', '').replace('}', '');
                Xrm.WebApi.retrieveRecord("incident", "" + parentCaseID + "", "?$select=_jarvis_homedealer_value").then(
                    function success(result) {
                        //console.log(result);
                        // Columns
                        var incidentid = result["incidentid"]; // Guid
                        var jarvis_homedealer = result["_jarvis_homedealer_value"]; // Lookup
                        if (jarvis_homedealer !== null) {
                            var hdLookup = [];   // Creating a new lookup Array
                            hdLookup[0] = {};    // new Object
                            hdLookup[0].id = jarvis_homedealer;  // GUID of the lookup id
                            hdLookup[0].name = result["_jarvis_homedealer_value@OData.Community.Display.V1.FormattedValue"];// Name of the lookup
                            hdLookup[0].entityType = result["_jarvis_homedealer_value@Microsoft.Dynamics.CRM.lookuplogicalname"]; // Entity Type of the lookup entity
                            formContext.getAttribute("jarvis_homedealer").setValue(hdLookup);
                        }

                    },
                    function (error) {
                        //console.log(error.message);
                    }
                );
            }
        }


    },
    populateRD: function (executionContext) {
        "use strict";
        var formContext = executionContext.getFormContext(); //jarvis_incident
        var incident = formContext.getAttribute("jarvis_incident").getValue();
        var rd = formContext.getAttribute("jarvis_repairingdealer").getValue();
        var hd = formContext.getAttribute("jarvis_homedealer").getValue();
        var parentCaseID = incident[0].id.toString().replace('{', '').replace('}', '');
        if (rd !== null || hd !== null) {

        }
        else {
            if (incident !== null) {
                Xrm.WebApi.retrieveMultipleRecords("jarvis_passout", "?$select=_jarvis_repairingdealer_value,jarvis_name&$filter=_jarvis_incident_value eq " + parentCaseID + "").then(
                    function success(results) {
                        //console.log(results);
                        if (results.entities.length === 1) {
                            var result = results.entities[0];
                            // Columns
                            var jarvis_passoutid = result["jarvis_passoutid"]; // Guid
                            var jarvis_name = result["jarvis_name"];
                            var jarvis_repairingdealer = result["_jarvis_repairingdealer_value"]; // Lookup
                            var jarvis_repairingdealer_formatted = result["_jarvis_repairingdealer_value@OData.Community.Display.V1.FormattedValue"];
                            var jarvis_repairingdealer_lookuplogicalname = result["_jarvis_repairingdealer_value@Microsoft.Dynamics.CRM.lookuplogicalname"];
                            var rdLookup = [];   // Creating a new lookup Array
                            rdLookup[0] = {};    // new Object
                            rdLookup[0].id = jarvis_repairingdealer;  // GUID of the lookup id
                            rdLookup[0].name = jarvis_repairingdealer_formatted;// Name of the lookup
                            rdLookup[0].entityType = jarvis_repairingdealer_lookuplogicalname; // Entity Type of the lookup entity
                            formContext.getAttribute("jarvis_repairingdealer").setValue(rdLookup);

                        }

                    },
                    function (error) {
                        //console.log(error.message);
                    }
                );
            }
        }

    }
}
