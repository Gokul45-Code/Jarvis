function populateSubject(executionContext) {
    "use strict";
    var formContext = executionContext.getFormContext();
    var resolutionType = formContext.getAttribute("resolutiontypecode").getValue();
    if (resolutionType !== 1000) {
        formContext.getAttribute("subject").setValue("Standard Resolved");
        var fieldlist = ["jarvis_serviceline", "jarvis_closuredescription", "jarvis_closuretype", "jarvis_casestatus"]
        makeFieldVisible(formContext, fieldlist, false);
        makeFieldsMandatory(formContext, fieldlist, "none");
    }
    else {
        formContext.getAttribute("subject").setValue(null);
        var fieldlist = ["jarvis_serviceline", "jarvis_closuredescription", "jarvis_closuretype", "jarvis_casestatus"]
        makeFieldVisible(formContext, fieldlist, true);
        makeFieldsMandatory(formContext, fieldlist, "required");
    }

}

function populateResolution(executionContext) {
    "use strict";
    var formContext = executionContext.getFormContext();
    var descriptionlookup = formContext.getAttribute("jarvis_closuredescription").getValue();
    if (descriptionlookup !== null) {
        var description = descriptionlookup[0].name;
        formContext.getAttribute("subject").setValue(description);

    }
    else {

        formContext.getAttribute("subject").setValue(null);

    }

}


function updateCaseForceClosedStatus(executionContext) {
    "use strict";
    var formContext = executionContext.getFormContext();//
    var record = {};
    record.statuscode = 1000; // Status
    record.statecode = 1; // State
    record.jarvis_mercuriusstatus = 900; // Mercurius Status
    var incident = formContext.getAttribute("incidentid").getValue();
    var parentCaseID = incident[0].id.toString().replace('{', '').replace('}', '');
    var resolutionType = formContext.getAttribute("resolutiontypecode").getValue();
    if (parentCaseID !== null) {
        if (resolutionType === 1000)//Force Closed
        {
            Xrm.WebApi.updateRecord("incident", "" + parentCaseID + "", record).then(
                function success(result) {
                    var updatedId = result.id;
                    //console.log(updatedId);
                },
                function (error) {
                    //console.log(error.message);
                }
            );
        }

    }

}

function makeFieldsMandatory(formContext, fieldArr, isMandatory) {
    "use strict";
    for (var ii = 0; ii < fieldArr.length; ii++) {
        formContext.getAttribute(fieldArr[ii]).setRequiredLevel(isMandatory);//
    }
}

function makeFieldVisible(formContext, fieldArr, isVisible) {
    "use strict";
    for (var ii = 0; ii < fieldArr.length; ii++) {
        formContext.getControl(fieldArr[ii]).setVisible(isVisible);//
    }
}

function populateSubject(executionContext) {
    "use strict";
    var formContext = executionContext.getFormContext();
    var resolutionType = formContext.getAttribute("resolutiontypecode").getValue();
    if (resolutionType !== 1000) {
        formContext.getAttribute("subject").setValue("Standard Resolved");
        var fieldlist = ["jarvis_serviceline", "jarvis_closuredescription", "jarvis_closuretype", "jarvis_casestatus"]
        makeFieldVisible(formContext, fieldlist, false);
        makeFieldsMandatory(formContext, fieldlist, "none");
    }
    else {
        formContext.getAttribute("subject").setValue(null);
        var fieldlist = ["jarvis_serviceline", "jarvis_closuredescription", "jarvis_closuretype", "jarvis_casestatus"]
        makeFieldVisible(formContext, fieldlist, true);
        makeFieldsMandatory(formContext, fieldlist, "required");
    }

}


function retrieveIncidentStatus(executionContext) {
    "use strict";
    var formContext = executionContext.getFormContext();//
    var incident = formContext.getAttribute("incidentid").getValue();
    var statecode = formContext.getAttribute("statecode").getValue();
    if (incident !== null && statecode !== 1) {
        var parentCaseID = incident[0].id.toString().replace('{', '').replace('}', '');
        Xrm.WebApi.retrieveRecord("incident", "" + parentCaseID + "", "?$select=statuscode", "_jarvis_caseserviceline_value").then(
            function success(result) {
                //console.log(result);
                // Columns
                var incidentid = result["incidentid"]; // Guid
                var statuscode = result["statuscode"]; // Status
                var statuscode_formatted = result["statuscode@OData.Community.Display.V1.FormattedValue"];
                var jarvis_caseserviceline = result["_jarvis_caseserviceline_value"]; // Lookup
                var jarvis_caseserviceline_formatted = result["_jarvis_caseserviceline_value@OData.Community.Display.V1.FormattedValue"];
                var jarvis_caseserviceline_lookuplogicalname = result["_jarvis_caseserviceline_value@Microsoft.Dynamics.CRM.lookuplogicalname"];
                if (statuscode !== 90 && statuscode !== 85) {
                    formContext.getAttribute("resolutiontypecode").setValue(1000);
                    if (jarvis_caseserviceline !== null) {
                        var vhLookup = [];
                        vhLookup[0] = {};
                        vhLookup[0].id = jarvis_caseserviceline;
                        vhLookup[0].name = jarvis_caseserviceline_formatted; // Name of the lookup
                        vhLookup[0].entityType = jarvis_caseserviceline_lookuplogicalname; // Entity Type of the lookup entity
                        formContext.getAttribute("jarvis_serviceline").setValue(vhLookup);
                    }

                }
            },
            function (error) {
                //console.log(error.message);
            }
        );
    }

}


function updateCaseForceClosedStatus(executionContext) {
    "use strict";
    var formContext = executionContext.getFormContext();//
    var record = {};
    record.statuscode = 1000; // Status
    record.statecode = 1; // State
    record.jarvis_mercuriusstatus = 900; // Mercurius Status
    var incident = formContext.getAttribute("incidentid").getValue();
    var parentCaseID = incident[0].id.toString().replace('{', '').replace('}', '');
    var resolutionType = formContext.getAttribute("resolutiontypecode").getValue();
    if (parentCaseID !== null) {
        if (resolutionType === 1000)//Force Closed
        {
            Xrm.WebApi.updateRecord("incident", "" + parentCaseID + "", record).then(
                function success(result) {
                    var updatedId = result.id;
                    //console.log(updatedId);
                },
                function (error) {
                    //console.log(error.message);
                }
            );
        }

    }

}

function makeFieldsMandatory(formContext, fieldArr, isMandatory) {
    "use strict";
    for (var ii = 0; ii < fieldArr.length; ii++) {
        formContext.getAttribute(fieldArr[ii]).setRequiredLevel(isMandatory);//
    }
}

function makeFieldVisible(formContext, fieldArr, isVisible) {
    "use strict";
    for (var ii = 0; ii < fieldArr.length; ii++) {
        formContext.getControl(fieldArr[ii]).setVisible(isVisible);//
    }
}


function populateCaseStatus(executionContext) {
    "use strict";
    var formContext = executionContext.getFormContext();//
    var serviceline = formContext.getAttribute("jarvis_serviceline").getValue();
    var incident = formContext.getAttribute("incidentid").getValue();
    var statecode = formContext.getAttribute("statecode").getValue();
    if (serviceline !== null && incident !== null && statecode !== 1) {
        var parentCaseID = incident[0].id.toString().replace('{', '').replace('}', '');
        var servicelineID = serviceline[0].id.toString().replace('{', '').replace('}', '');
        Xrm.WebApi.retrieveMultipleRecords("incident", "?$select=statuscode&$filter=incidentid eq '" + parentCaseID + "'").then(
            function success(result) {
                //console.log(result);
                // Columns
                var incidentid = result.entities[0]["incidentid"]; // Guid
                var statuscode = result.entities[0]["statuscode"]; // Status
                var statuscode_formatted = result.entities[0]["statuscode@OData.Community.Display.V1.FormattedValue"];//statuscode@OData.Community.Display.V1.FormattedValue
                Xrm.WebApi.retrieveMultipleRecords("jarvis_casestatus", "?$select=_jarvis_serviceline_value&$filter=(jarvis_name eq '" + statuscode_formatted + "' and _jarvis_serviceline_value eq '" + servicelineID + "')").then(
                    function success(results) {
                        //console.log(results);
                        for (var i = 0; i < results.entities.length; i++) {
                            var result = results.entities[i];
                            // Columns
                            var jarvis_casestatusid = result["jarvis_casestatusid"]; // Guid
                            var jarvis_serviceline = result["_jarvis_serviceline_value"]; // Lookup
                            var jarvis_serviceline_formatted = result["_jarvis_serviceline_value@OData.Community.Display.V1.FormattedValue"];
                            var jarvis_serviceline_lookuplogicalname = result["_jarvis_serviceline_value@Microsoft.Dynamics.CRM.lookuplogicalname"];
                            var jarvis_name = result["jarvis_name"]; // Text
                            var vhLookup = [];
                            vhLookup[0] = {};
                            vhLookup[0].id = jarvis_casestatusid;
                            vhLookup[0].name = jarvis_name; // Name of the lookup
                            vhLookup[0].entityType = "jarvis_casestatus"; // Entity Type of the lookup entity
                            formContext.getAttribute("jarvis_casestatus").setValue(vhLookup);
                            if (statuscode !== 90) {
                                formContext.getAttribute("resolutiontypecode").setValue(1000);
                            }
                            else {
                                var fieldlist = ["jarvis_serviceline", "jarvis_closuredescription", "jarvis_closuretype", "jarvis_casestatus"]
                                makeFieldVisible(formContext, fieldlist, false);
                                formContext.getAttribute("subject").setValue("Standard Resolved");
                            }
                        }
                    },
                    function (error) {
                        //console.log(error.message);
                    }
                );
            },
            function (error) {
                //console.log(error.message);
            }
        );

    }

}

function populateResolution(executionContext) {
    "use strict";
    var formContext = executionContext.getFormContext();
    var descriptionlookup = formContext.getAttribute("jarvis_closuredescription").getValue();
    if (descriptionlookup !== null) {
        var description = descriptionlookup[0].name;
        formContext.getAttribute("subject").setValue(description);

    }
    else {

        formContext.getAttribute("subject").setValue(null);

    }

}

function clearDescription(executionContext) {
    "use strict";
    var formContext = executionContext.getFormContext();
    var closureType = formContext.getAttribute("jarvis_closuretype").getValue();
    if (closureType !== null) {
        formContext.getAttribute("jarvis_closuredescription").setValue(null);
    }
}