var Jarvis = Jarvis || {};
Jarvis.MAConst = {
    message: "There is an open GOP request. Do you want to close this Monitor Action?",
    entityControl: "entity_control",
    inactive: 1,
    cancelled: 2,
    title: "Confirmation Monitor Action completion"
};

// Framing Notification Message to Dispaly on Click of Cancel.
var confirmStrings = { text: Jarvis.MAConst.message, title: Jarvis.MAConst.title, confirmButtonLabel: "Yes", cancelButtonLabel: "No" };
var confirmOptions = { height: 150, width: 500 };

Jarvis.caseMonitorActionTime = {

    validate: function (executionContext) {
        "use strict";

        let fieldVal = executionContext.getFormContext().getAttribute("jarvis_followuptime").getValue();
        if (fieldVal.replace(':', '').length === 3) {
            fieldVal = '0' + fieldVal;
        }
        if (fieldVal.search(':') === -1) {
            fieldVal = fieldVal.slice(0, 2) + ":" + fieldVal.slice(2);
        }
        executionContext.getFormContext().getAttribute("jarvis_followuptime").setValue(fieldVal);
        const regex = new RegExp(/^([01]\d|2[0-3]):[0-5][0-9]$/);

        if (regex.test(fieldVal) === false) {
            var alertStrings = { confirmButtonLabel: "OK", text: "Please enter valid time formats hhmm OR hh:mm", title: "Alert" };
            var alertOptions = { height: 120, width: 260 };
            Xrm.Navigation.openAlertDialog(alertStrings, alertOptions).then(
                function (success) {

                },
                function (error) {

                }
            );
            executionContext.getFormContext().getAttribute("jarvis_followuptime").setValue(null);
        }



    }

}

Jarvis.caseMonitorActionDate =
{

    updateDate: function (executionContext) {
        "use strict";
        var formContext = executionContext.getFormContext();
        let today = new Date();
        var date = formContext.getAttribute('actualstart').getValue();
        if (date === null) {
            executionContext.getFormContext().getAttribute("actualstart").setValue(today);
        }
    },

    PastDateValidation: function (executionContext) {
        "use strict";
        var formContext = executionContext.getFormContext();
        var today = new Date();
        var date = formContext.getAttribute('actualstart').getValue();
        var existingDate = new Date(date);
        if (date !== null) {
            if (new Date(today.getFullYear(), today.getMonth(), today.getDate()) > new Date(existingDate.getFullYear(), existingDate.getMonth(), existingDate.getDate())) {
                var alertStrings = { confirmButtonLabel: "OK", text: "Can't select past date.", title: "Alert" };
                var alertOptions = { height: 120, width: 260 };
                Xrm.Navigation.openAlertDialog(alertStrings, alertOptions).then(
                    function (success) {

                    },
                    function (error) {

                    }
                );
                executionContext.getFormContext().getAttribute("actualstart").setValue(null);
            }
        }
    }

}

Jarvis.caseMonitorSave = {
    isFUformatvalid: true,
    retrieveRecords: async function (formContext, entityname, entityColumn, searchColumn, searchValue) {
        "use strict";
        var results = await Xrm.WebApi.retrieveMultipleRecords(entityname, "?$select=" + entityColumn + "&$filter=(" + searchColumn + " eq '" + searchValue + "' and statecode eq 0)");

        return results;
    },
    validateFUComment: async function (executionContext) {
        "use strict";
        Jarvis.caseMonitorSave.isFUformatvalid = true;
        var formContext = executionContext.getFormContext();
        formContext.ui.clearFormNotification("ERR_MonitorLangCode");
        let followupComment = executionContext.getFormContext().getAttribute("subject").getValue();
        var continuesave = true;
        if (followupComment !== null && followupComment !== "") {
            var charlist = followupComment.split(' ');
            if (charlist.length > 2) {
                var langCode = charlist[0];
                var countrycode = charlist[1];
                continuesave = false;

                //Check if country code has special characters
                const regex = new RegExp(/^[a-z0-9]+$/i);

                if (regex.test(countrycode) === false) {
                    formContext.ui.setFormNotification("Please provide a valid Language (ISO-3) and Country (ISO-2) in the follow Up Comment.", "ERROR", "ERR_MonitorLangCode");
                    Jarvis.caseMonitorSave.isFUformatvalid = false;
                }
                else {
                    // Check any country matching
                    var results = await Jarvis.caseMonitorSave.retrieveRecords(formContext, "jarvis_country", "jarvis_iso2countrycode", "jarvis_iso2countrycode", countrycode);
                    if (results.entities.length < 1) {
                        formContext.ui.setFormNotification("Please provide a valid Language (ISO-3) and Country (ISO-2) in the follow Up Comment.", "ERROR", "ERR_MonitorLangCode");
                        Jarvis.caseMonitorSave.isFUformatvalid = false;
                    }
                }

                //Check if language code has special characters
                if (regex.test(langCode) === false) {
                    formContext.ui.setFormNotification("Please provide a valid Language (ISO-3) and Country (ISO-2) in the follow Up Comment.", "ERROR", "ERR_MonitorLangCode");
                    Jarvis.caseMonitorSave.isFUformatvalid = false;
                }
                else {
                    // Check any Language matching
                    var results2 = await Jarvis.caseMonitorSave.retrieveRecords(formContext, "jarvis_language", "jarvis_iso3languagecode6392t", "jarvis_iso3languagecode6392t", langCode);
                    if (results2.entities.length < 1) {
                        formContext.ui.setFormNotification("Please provide a valid Language (ISO-3) and Country (ISO-2) in the follow Up Comment.", "ERROR", "ERR_MonitorLangCode");
                        Jarvis.caseMonitorSave.isFUformatvalid = false;
                    }
                }
            }
            else {
                Jarvis.caseMonitorSave.isFUformatvalid = false;

            }

        }
        else {
            Jarvis.caseMonitorSave.isFUformatvalid = false;
        }
    },
    validate: async function (executionContext) {
        "use strict";
        var iscountryLanguageAvailable = true;
        var formContext = executionContext.getFormContext();
        formContext.ui.clearFormNotification("ERR_MonitorLangCode");

        if (!Jarvis.caseMonitorSave.isFUformatvalid) {
            formContext.ui.setFormNotification("Please provide a valid Language (ISO-3) and Country (ISO-2) in the follow Up Comment.", "ERROR", "ERR_MonitorLangCode");
            executionContext.getEventArgs().preventDefault();
        }
    }

}
function openConfirmDialogBox(entityId, entityName, primaryControl, selectedControl, isform = true) {
    Xrm.Navigation.openConfirmDialog(confirmStrings, confirmOptions).then(
        function (success) {
            if (success.confirmed) {
                var data = {
                    "statecode": 1,
                    "statuscode": 2
                }
                Xrm.WebApi.updateRecord("jarvis_casemonitoraction", entityId, data).then
                    (
                        function success(result) {
                            if (isform)
                                primaryControl.data.refresh(false);
                            else {
                                if (selectedControl._controlName != Jarvis.MAConst.entityControl) {
                                    primaryControl.getControl(selectedControl._controlName).refresh();
                                }
                                else {
                                    primaryControl.data.refresh(false);
                                }
                            }
                        },
                        function (error) {
                        });


            }

        });
}
Jarvis.caseMonitorComplete = {
    Completemonitor: function (entityName, entityId, primaryControl) {
        "use strict";
        var currentId = entityId;
        if (entityId.length > 0) {
            currentId = entityId[0]?.replace('{', '').replace('}', '');
        }
        var caseId = primaryControl.getAttribute("regardingobjectid")?.getValue()[0]["id"];
        //var caseId = primaryControl._data._formContext?._data?.getAttribute("jarvis_incident")?.getValue()[0]["id"];;
        caseId = caseId.replace('{', '').replace('}', '');
        var monitorActionname = primaryControl.getAttribute("subject")?.getValue();
        if (monitorActionname != null && monitorActionname.toUpperCase().includes("CHASE GOP")) {
            Jarvis.caseMonitorComplete.checkOpenGOP(caseId, entityId, entityName, primaryControl, primaryControl, openConfirmDialogBox);
        }
        else {
            var data = {
                "statecode": 1,
                "statuscode": 2
            }
            Xrm.WebApi.updateRecord("jarvis_casemonitoraction", currentId, data);
        }
    },

    checkOpenGOP: function (caseId, entityId, entityName, primaryControl, selectedControl, sucessCallBack, isform = true) {
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
	  <attribute name='jarvis_gopapproval' />
	  <attribute name='jarvis_totallimitincurrency' />
	  <attribute name='jarvis_totallimitin' />
	  <order attribute='modifiedon' descending='true' />
	  <filter type='and'>
		<condition attribute='jarvis_incident' operator='eq'  uitype='incident' value='{`+ caseId + `}' />
		<condition attribute='jarvis_gopapproval' operator='eq' value='334030000' />
		<condition attribute='jarvis_relatedgop' operator='null' />
		<condition attribute='statecode' operator='eq' value='0' />
	  </filter>
	</entity>
  </fetch>`;

        fetchXml = "?fetchXml=" + encodeURIComponent(fetchXml);
        Xrm.WebApi.retrieveMultipleRecords("jarvis_gop", fetchXml).then(
            function success(result) {
                if (result.entities.length > 0) {
                    if (entityId.length > 0) {
                        for (var i = 0; i < entityId.length; i++) {
                            entityId[i] = entityId[i].replace('{', '').replace('}', '');
                            sucessCallBack(entityId[i], entityName, primaryControl, selectedControl, isform);
                            break;
                        }
                    }

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

    /* openConfirmDialogBox: function (entityId, entityName, primaryControl, selectedControl, isform = true) {
         Xrm.Navigation.openConfirmDialog(confirmStrings, confirmOptions).then(
             function (success) {
                 if (success.confirmed) {
                     var data = {
                         "statecode": 1,
                         "statuscode": 2
                     }
                     Xrm.WebApi.updateRecord("jarvis_casemonitoraction", entityId, data).then
                         (
                             function success(result) {
                                 if (isform)
                                     primaryControl.data.refresh(false);
                                 else {
                                     if (selectedControl._controlName != Jarvis.MAConst.entityControl) {
                                         primaryControl.getControl(selectedControl._controlName).refresh();
                                     }
                                     else {
                                         primaryControl.data.refresh(false);
                                     }
                                 }
                             },
                             function (error) {
                             });
 
 
                 }
             });
     }*/

}
