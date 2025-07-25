var Jarvis = Jarvis || {};
var MERCURIUS = "UNASSIGNED";
// Framing Notification Message to Dispaly on Click of Cancel.
var confirmStrings = { text: "There is an open GOP request.?Do you want to close this Monitor Action?", title: "Confirmation Monitor Action completion", confirmButtonLabel: "Yes", cancelButtonLabel: "No" };
var confirmOptions = { height: 150, width: 500 };
Jarvis.CaseRibbon = {

    pickCaseAction: function (selectedItemId, entityTypeName, selectedControl) {
        var caseId;
        if (selectedItemId != null) {
            caseId = selectedItemId.toString().slice(1, 37);

        }
        else if (selectedControl != null) {
            caseId = caseId = selectedControl._entityReference.id.guid;
        }
        //var caseId = selectedItemId.toString().slice(1, 37);

        var userSettings = Xrm.Utility.getGlobalContext().userSettings;

        var currentUserId = userSettings.userId.toString().slice(1, 37);

        Xrm.WebApi.retrieveRecord("incident", caseId, "?$select=_ownerid_value,casetypecode").then(
            function success(result) {
                var caseWorker = result["_ownerid_value"];
                var ownerid = result["_ownerid_value"]; // Owner
                var caseWorkerLabel = result["_ownerid_value@OData.Community.Display.V1.FormattedValue"];
                var ownerid_lookuplogicalname = result["_ownerid_value@Microsoft.Dynamics.CRM.lookuplogicalname"];
                var casetypecode = result["casetypecode"]; // Choice
                if (casetypecode != 3) {
                    if (caseWorker.toUpperCase() !== currentUserId.toUpperCase() && casetypecode != 3) {

                        confirmStrings = { text: "The Item will be assigned to you to work on.", subTitle: "You have selected 1 item", title: "Pick", cancelButtonLabel: "Cancel", confirmButtonLabel: "Pick" };
                        var confirmOptions = { height: 200, width: 450 };

                        Xrm.Navigation.openConfirmDialog(confirmStrings, confirmOptions).then(
                            function (success) {
                                if (success.confirmed) {

                                    //Xrm.WebApi.retrieveMultipleRecords("systemuser", "?$select=domainname&$filter=contains(domainname,'MERCURIUS') and isdisabled eq false").then(
                                    //function success(results) {

                                    //if (results && results.entities.length > 0) {
                                    //var result = results.entities[0];
                                    // Columns
                                    //var mercuriusId = result["systemuserid"]; // Guid   

                                    if (!caseWorkerLabel.toUpperCase().includes(MERCURIUS)) {
                                        Jarvis.CaseRibbon.showErrorNotification("You can only pick cases assigned to Default User...");
                                    }
                                    else {

                                        var data =
                                        {
                                            "ownerid@odata.bind": "/systemusers(" + currentUserId + ")"
                                        }

                                        Xrm.WebApi.updateRecord(entityTypeName.toString(), caseId, data).then(
                                            function success(result) {

                                                Jarvis.CaseRibbon.showSuccessNotification("Case has been successfully picked by you...");
                                                if (selectedControl._controlName !== undefined) {
                                                    selectedControl.refresh();
                                                }
                                                else {
                                                    selectedControl.ui.refresh();
                                                }

                                                //gridContext.refresh();
                                            },
                                            function (error) {

                                                Jarvis.CaseRibbon.showErrorNotification("Case could not be picked from the queue...");

                                            }
                                        );

                                    }



                                    //}
                                    //});


                                }

                            });


                    }
                    else {
                        Jarvis.CaseRibbon.showErrorNotification("The case is already assigned to you...");
                    }
                }
                else {
                    //Jarvis.CaseRibbon.showErrorNotification("Functionality not applicable for Cases of Type Query");
                }

            });
    },

    noReleaseCaseAction: function (selectedItemId, entityTypeName, selectedControl) {

        var caseId;
        if (selectedItemId != null) {
            caseId = selectedItemId.toString().slice(1, 37);

        }
        else if (selectedControl != null) {
            caseId = selectedControl._entityReference.id.guid;
        }

        // var caseId = selectedItemId.toString().slice(1, 37);

        var userSettings = Xrm.Utility.getGlobalContext().userSettings;

        // update the user id to Mercurious User

        var currentUserId = userSettings.userId.toString().slice(1, 37);  // "cea66b08-49ad-ed11-aad0-0022489caba0";

        Xrm.WebApi.retrieveRecord("incident", caseId, "?$select=_ownerid_value,casetypecode,statuscode").then(
            function success(result) {
                var caseWorker = result["_ownerid_value"];
                var casetypecode = result["casetypecode"]; // Choice
                var caseStatus = result["statuscode"]; // Choice
                if (casetypecode != 3) {
                    if (caseWorker.toUpperCase() === currentUserId.toUpperCase() && casetypecode != 3 && caseStatus != 90) {

                        var confirmStrings = { text: "The Item(s) will be assigned to Queue owner for other members to pick up", subTitle: "Do you want to release the selected Item?", title: "Release Queue Item", cancelButtonLabel: "Cancel", confirmButtonLabel: "Release" };
                        var confirmOptions = { height: 200, width: 450 };

                        Xrm.Navigation.openConfirmDialog(confirmStrings, confirmOptions).then(
                            function (success) {
                                if (success.confirmed) {

                                    Xrm.WebApi.retrieveMultipleRecords("systemuser", "?$select=domainname&$filter=contains(domainname,'" + MERCURIUS + "') and isdisabled eq false").then(
                                        function success(results) {

                                            if (results && results.entities.length > 0) {
                                                var result = results.entities[0];
                                                // Columns
                                                var mercuriusId = result["systemuserid"]; // Guid

                                                var data =
                                                {
                                                    "ownerid@odata.bind": "/systemusers(" + mercuriusId + ")"
                                                    //"routecase": true // Bool
                                                }

                                                Xrm.WebApi.updateRecord(entityTypeName.toString(), caseId, data).then(
                                                    function success(result) {

                                                        Jarvis.CaseRibbon.showSuccessNotification("Case has been successfully released to queue...");
                                                        if (selectedControl._controlName !== undefined) {
                                                            selectedControl.refresh();
                                                        }
                                                        else {
                                                            selectedControl.ui.refresh();
                                                        }
                                                        //gridContext.refresh();
                                                    },
                                                    function (error) {

                                                        Jarvis.CaseRibbon.showErrorNotification("Case could not be released to the Queue...");
                                                    }
                                                );

                                            } else {
                                                alert("Please enter the correct value for Country");
                                            }
                                        },
                                        function (error) {
                                        }
                                    );


                                }


                            });

                    }
                    else {
                        Jarvis.CaseRibbon.showErrorNotification("You can only release Case assigned to you...");

                    }
                }
                else {
                    //Jarvis.CaseRibbon.showErrorNotification("Functionality not applicable for Cases of Type Query");
                }

            }
        );
    },

    showSuccessNotification(successMessage) {

        var notificationSuccess =
        {
            type: 2,
            level: 1, //success
            message: successMessage, //"You can only release Case assigned to you..."
            showCloseButton: true,
        }

        Xrm.App.addGlobalNotification(notificationSuccess).then(
            function success(result) {

            },
            function (error) {

            }
        );

    },
    releaseCaseAction: function (selectedItemId, entityTypeName, selectedControl) {

        var caseId;
        if (selectedItemId != null) {
            caseId = selectedItemId.toString().slice(1, 37);

        }
        else if (selectedControl != null) {
            caseId = caseId = selectedControl._entityReference.id.guid;
        }

        //var caseId = selectedItemId.toString().slice(1, 37);
        var entityType = entityTypeName;
        var selectedControlName = selectedControl;
        var userSettings = Xrm.Utility.getGlobalContext().userSettings;

        // update the user id to Mercurious User

        var currentUserId = userSettings.userId.toString().slice(1, 37);  // "cea66b08-49ad-ed11-aad0-0022489caba0";

        Xrm.WebApi.retrieveRecord("incident", caseId, "?$select=_ownerid_value,casetypecode,statuscode,statecode").then(
            function success(result) {
                var caseWorker = result["_ownerid_value"];
                var casetypecode = result["casetypecode"]; // Choice
                var caseStatus = result["statuscode"]; // Choice
                var stateCode = result["statecode"]; // Choice
                if (stateCode == 0) {


                    if (caseWorker.toUpperCase() === currentUserId.toUpperCase()) {

                        var confirmStrings = { text: "The Item(s) will be assigned to Queue owner for other members to pick up", subTitle: "Do you want to release the selected Item?", title: "Release Queue Item", cancelButtonLabel: "Cancel", confirmButtonLabel: "Release" };
                        var confirmOptions = { height: 200, width: 450 };

                        Xrm.Navigation.openConfirmDialog(confirmStrings, confirmOptions).then(
                            function (success) {
                                if (success.confirmed) {//checkForOpenMonitorActions
                                    Jarvis.CaseRibbon.checkForOpenMonitorActions(caseId, casetypecode, entityType, selectedControlName);

                                }


                            });

                    }
                    else {
                        Jarvis.CaseRibbon.showErrorNotification("You can only release Case assigned to you...");

                    }
                }
                else {
                    Jarvis.CaseRibbon.showErrorNotification("Only Active Cases can be released");
                }
            }
        );
    },

    showErrorNotification(errorMessage) {

        var notificationSuccess =
        {
            type: 2,
            level: 2, //error
            message: errorMessage, //"You can only release Case assigned to you..."
            showCloseButton: true,
        }

        Xrm.App.addGlobalNotification(notificationSuccess).then(
            function success(result) {

            },
            function (error) {

            }
        );

    },
    checkForOpenMonitorActions(incident, casetypecode, entityTypeName, selectedControl) {
        var entityType = entityTypeName;
        var selectedControlName = selectedControl;
        Xrm.WebApi.retrieveMultipleRecords("jarvis_casemonitoraction", "?$filter=(_regardingobjectid_value eq " + incident + " and statuscode eq 1)").then(
            function success(results) {
                console.log(results);
                if (results == null || results.entities.length == 0) {
                    var parameters = {};
                    var caseLookup = new Array();
                    var regardingItem = new Object();
                    regardingItem.id = incident;
                    //regardingItem.name = caseName;
                    regardingItem.entityType = 'incident';
                    caseLookup[0] = regardingItem;
                    parameters["regardingobjectid"] = caseLookup;
                    parameters["actualstart"] = Date.now();
                    var pageInput = {
                        pageType: "entityrecord",
                        entityName: "jarvis_casemonitoraction",
                        data: parameters
                    };
                    var navigationOptions = {
                        target: 2,
                        height: { value: 100, unit: "%" },
                        width: { value: 70, unit: "%" },
                        position: 1
                    };
                    Xrm.Navigation.navigateTo(pageInput, navigationOptions).then(
                        function success(result) {
                            console.log("Record created with ID: " + result.savedEntityReference[0].id +
                                " Name: " + result.savedEntityReference[0].name)
                            // Handle dialog closed
                            if (casetypecode != 3) {
                                Jarvis.CaseRibbon.assignOwnerShip(incident, entityType, selectedControlName);
                            }


                        },
                        function error() {
                            // Handle errors
                        }
                    );
                }
                else {
                    if (casetypecode != 3) {
                        Jarvis.CaseRibbon.assignOwnerShip(incident, entityType, selectedControlName);
                    }
                }

            },
            function (error) {
                console.log(error.message);
            }
        );
    },
    assignOwnerShip(incident, entityTypeName, selectedControl) {
        Xrm.WebApi.retrieveMultipleRecords("systemuser", "?$select=domainname&$filter=contains(domainname,'" + MERCURIUS + "') and isdisabled eq false").then(
            function success(results) {

                if (results && results.entities.length > 0) {
                    var result = results.entities[0];
                    // Columns
                    var mercuriusId = result["systemuserid"]; // Guid

                    var data =
                    {
                        "ownerid@odata.bind": "/systemusers(" + mercuriusId + ")"
                        //"routecase": true
                    }
                    //data.jarvis_isreleased = true;
                    Xrm.WebApi.updateRecord(entityTypeName.toString(), incident, data).then(
                        function success(result) {

                            Jarvis.CaseRibbon.showSuccessNotification("Case has been successfully released to queue...");
                            if (selectedControl._controlName !== undefined) {
                                selectedControl.refresh();
                            }
                            else {
                                selectedControl.ui.refresh();
                            }
                            //gridContext.refresh();
                        },
                        function (error) {

                            Jarvis.CaseRibbon.showErrorNotification("Case could not be released to the Queue...");
                        }
                    );

                } else {
                    alert("Please enter the correct value for Country");
                }
            },
            function (error) {
            }
        );
    },
    gMapsRedirect(incident, selectedControl) {
        var caseId = incident.toString().slice(1, 37);
        var sourceID = selectedControl.getControl("jarvis_sourceid").getAttribute()?.getValue();
        Xrm.WebApi.retrieveMultipleRecords("jarvis_configurationjarvis", "?$select=jarvis_gmapsurl&$filter=jarvis_gmapsurl ne null").then(
            function success(results) {
                console.log(results);
                for (var i = 0; i < results.entities.length; i++) {
                    var result = results.entities[i];
                    // Columns
                    var jarvis_configurationjarvisid = result["jarvis_configurationjarvisid"]; // Guid
                    var gmapsurl = result["jarvis_gmapsurl"]; // Text
                    if (sourceID != null && sourceID != "") {

                        Xrm.Navigation.openUrl(gmapsurl + sourceID);

                    }
                    else {
                        Xrm.Navigation.openUrl(gmapsurl);

                    }
                }
            },
            function (error) {
                console.log(error.message);
            }
        );

    },
    closeMonitorActionSubgrid: async function (selectedItemId, primaryControl, PrimaryItemId) {
        var actions = selectedItemId;
        var chaseGOPMAs = [];
        var isChaseGOPSelected = false;
        var caseId = PrimaryItemId;
        if (PrimaryItemId.length > 0) {
            caseId = PrimaryItemId[0]?.replace('{', '').replace('}', '');
        }
        for (i = 0; i < actions.length; i++) {
            if (actions[i]?.Name.toUpperCase().includes("CHASE GOP")) {
                isChaseGOPSelected = true;
                chaseGOPMAs.push(actions[i]?.Id);
            }
            else {
                await Jarvis.CaseRibbon.closeMonitorAction(actions[i]?.Id, primaryControl);
            }
        }
        if (isChaseGOPSelected) {

            Jarvis.CaseRibbon.checkOpenGOP(caseId, chaseGOPMAs, primaryControl, Jarvis.CaseRibbon.openConfirmDialogBox, false);

        }
        else {
            for (i = 0; i < actions.length; i++) {
                await Jarvis.CaseRibbon.closeMonitorAction(actions[i]?.Id, primaryControl);
            }
        }

        primaryControl.getControl("Subgrid_new_70").refresh();
    },
    closeMonitorAction: async function (selectedItemId, primaryControl) {
        var data = {
            "statecode": 1,
            "statuscode": 2
        }
        // update the record
        await Xrm.WebApi.updateRecord("jarvis_casemonitoraction", selectedItemId, data);

    },
    openConfirmDialogBox: function (entityId, primaryControl, isform = true) {
        Xrm.Navigation.openConfirmDialog(confirmStrings, confirmOptions).then(
            function (success) {
                if (success.confirmed) {
                    var data = {
                        "statecode": 1,
                        "statuscode": 2
                    }
                    for (var i = 0; i < entityId.length; i++) {
                        entityId[i] = entityId[i].replace('{', '').replace('}', '');
                        Xrm.WebApi.updateRecord("jarvis_casemonitoraction", entityId[i], data).then
                            (
                                function success(result) {
                                    if (isform)
                                        primaryControl.data.refresh(false);
                                    else {
                                        primaryControl.getControl("Subgrid_new_70").refresh();
                                    }
                                },
                                function (error) {
                                });
                    }


                }

            });
    },

    checkOpenGOP: function (caseId, entityId, primaryControl, sucessCallBack, isform = true) {
        var currentId = entityId;
        if (entityId.length > 0) {
            currentId = entityId[0]?.replace('{', '').replace('}', '');
        }
        var fetchXml = `<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
	<entity name='jarvis_gop'>
	  <attribute name='jarvis_gopid' />
	  <attribute name='jarvis_name' />
	  <attribute name='createdon' />
	  <attribute name='jarvis_requesttype' />
	  <attribute name='modifiedon' />
	  <attribute name='jarvis_incident' />
	  <attribute name='jarvis_approved' />
	  <attribute name='jarvis_totallimitincurrency' />
	  <attribute name='jarvis_totallimitin' />
	  <order attribute='modifiedon' descending='true' />
	  <filter type='and'>
		<condition attribute='jarvis_incident' operator='eq'  uitype='incident' value='{`+ caseId + `}' />
		<condition attribute='jarvis_gopapproval' operator='eq' value='334030001' />
		<condition attribute='jarvis_relatedgop' operator='null' />
		<condition attribute='statecode' operator='eq' value='0' />
	  </filter>
	</entity>
  </fetch>`;

        fetchXml = "?fetchXml=" + encodeURIComponent(fetchXml);
        Xrm.WebApi.retrieveMultipleRecords("jarvis_gop", fetchXml).then(
            function success(result) {
                if (result.entities.length > 0) {
                    sucessCallBack(entityId, primaryControl, isform);
                }
                else {
                    var data = {
                        "statecode": 1,
                        "statuscode": 2
                    }
                    Xrm.WebApi.updateRecord("jarvis_casemonitoraction", currentId, data);
                }
            },
            function (error) {
                var data = {
                    "statecode": 1,
                    "statuscode": 2
                }
                Xrm.WebApi.updateRecord("jarvis_casemonitoraction", currentId, data);

            }
        );
    }

}