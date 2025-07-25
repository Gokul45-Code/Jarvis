
HideCustomer360Section = function (executionContext, oneTimeCustomerFlag) {
    "use strict";
    const formContext = executionContext.getFormContext();
    let sectioncustomer360 = formContext.ui.tabs.get("tab_29").sections.get("tab_validate_section_12");
    if (sectioncustomer360 == null) { return };
    if (sectioncustomer360) {

        if (!oneTimeCustomerFlag || oneTimeCustomerFlag == null) {
            sectioncustomer360.setVisible(true);
        }
        else {
            sectioncustomer360.setVisible(false);
        }
    }

}

//Update Onetimecustomer based onn customer id
UpdateOneTimeCustomer = function (executionContext) {
    "use strict";
    const formContext = executionContext.getFormContext();
    let field = formContext.getAttribute("customerid");
    let onetimefield = formContext.getAttribute("jarvis_onetimecustomeryesno");
    let oneTimeFlag = null;

    if (field != null) {
        var value = field.getValue();
        if (value) {
            let record_id = field.getValue()[0].id;
            record_id = record_id.replace('{', '').replace('}', '');
            Xrm.WebApi.retrieveRecord("account", record_id, "?$select=jarvis_onetimecustomer").then(
                function success(result) {

                    if (onetimefield != null) {
                        oneTimeFlag = result.jarvis_onetimecustomer;
                        formContext.getAttribute("jarvis_onetimecustomeryesno").setValue(result.jarvis_onetimecustomer);
                        formContext.getAttribute("jarvis_onetimecustomeryesno").fireOnChange();
                        HideCustomer360Section(executionContext, oneTimeFlag);
                    }
                }
            );
        }
        else if (onetimefield != null) {

            oneTimeFlag = false;
            formContext.getAttribute("jarvis_onetimecustomeryesno").setValue(oneTimeFlag);
            formContext.getAttribute("jarvis_onetimecustomeryesno").fireOnChange();
            HideCustomer360Section(executionContext, oneTimeFlag)
        }
    }

}

//Get CDB Customer for one time customer
UpdateCDBCustomerDetails = function (executionContext) {
    //    "use strict";
    caseCntrls = {
        SourceFields: [
            {
                TargetField: "jarvis_onetimecustomername",
                Fieldtype: "Text",
                SourceField: "name"
            },
            {
                TargetField: "jarvis_onetimecustomerstreet1",
                Fieldtype: "Text",
                SourceField: "address1_line1"
            },
            {
                TargetField: "jarvis_onetimecustomerstreet2",
                Fieldtype: "Text",
                SourceField: "address1_line2"
            },
            {
                TargetField: "jarvis_onetimecustomerzip",
                Fieldtype: "Text",
                SourceField: "address1_postalcode"
            },
            {
                TargetField: "jarvis_onetimecustomercity",
                Fieldtype: "Text",
                SourceField: "address1_city"
            },
            {
                TargetField: "jarvis_onetimecustomervat",
                Fieldtype: "Text",
                SourceField: "jarvis_vatid"
            },
            {
                TargetField: "jarvis_onetimecustomercountry",
                Fieldtype: "Lookup",
                SourceField: "_jarvis_address1_country_value",
                DatasetAttribute: "_jarvis_address1_country_value"
            },
            {
                TargetField: "jarvis_onetimecustomerlanguage",
                Fieldtype: "Lookup",
                SourceField: "_jarvis_language_value",
                DatasetAttribute: "_jarvis_language_value"
            }
        ]
    };
    let formContext = executionContext.getFormContext();
    let field = formContext.getAttribute("customerid");
    if (field != null) {
        var value = field.getValue();
        if (value) {
            let record_id = field.getValue()[0].id;
            record_id = record_id.replace('{', '').replace('}', '');
            Xrm.WebApi.retrieveRecord("account", record_id, "?$select=name,address1_city,_jarvis_address1_country_value,address1_line1,address1_line2,address1_postalcode,_jarvis_language_value,jarvis_vatid,jarvis_onetimecustomer").then(
                function success(result) {
                    if (result != null) {
                        if (!result.jarvis_onetimecustomer) {
                            caseCntrls.SourceFields.forEach(SourceField => {
                                var data = result[SourceField.SourceField];
                                if (data) {
                                    if (SourceField.Fieldtype === "Lookup") {
                                        var lookup = new Array();
                                        lookup[0] = new Object();
                                        lookup[0].id = data;
                                        lookup[0].name = result[SourceField.DatasetAttribute + "@OData.Community.Display.V1.FormattedValue"];
                                        lookup[0].entityType = result[SourceField.DatasetAttribute + "@Microsoft.Dynamics.CRM.lookuplogicalname"];
                                        formContext.getAttribute(SourceField.TargetField).setValue(lookup);
                                    }
                                    else {
                                        formContext.getAttribute(SourceField.TargetField).setValue(result[SourceField.SourceField]);
                                    }
                                }
                            });
                        }
                    }
                },
            );
        }
    }
}

updateImageFromServiceLine = function (serviceLineId, productId, formContext) {
    "use strict";
    Xrm.WebApi.retrieveRecord("jarvis_serviceline", serviceLineId).then(
        function success(result) {
            var serviceLineImage = result["jarvis_servicelineicon"];
            if (serviceLineImage != null) {
                var record = {};
                record.entityimage = serviceLineImage;
                Xrm.WebApi.updateRecord("incident", productId, record).then(

                    function success(result) {

                        var updatedId = result.id;
                        //Jarvis.Helper.refreshPage(formContext);
                        //console.log(updatedId);

                    },

                    function (error) {

                        //console.log(error.message);

                    }

                );
            }

        },
        function (error) {
            //console.log(error.message);
        }
    );

}
function onFieldChange(executionContext) {
    caseCntrls = {
        SourceFields: [

            {
                TargetField: "jarvis_homedealer",
                SourceTable: "jarvis_vehicle",
                Fieldtype: "Lookup",
                Field: "jarvis_updatedhomedealer",
                DatasetAttribute: "_jarvis_updatedhomedealer_value"
            },
            {
                TargetField: "customerid",
                SourceTable: "jarvis_vehicle",
                Fieldtype: "Lookup",
                Field: "jarvis_updatedusingcustomer",
                DatasetAttribute: "_jarvis_updatedusingcustomer_value"
            },
            {
                TargetField: "jarvis_caseserviceline",
                SourceTable: "jarvis_vehicle",
                Fieldtype: "Related",
                Field: "jarvis_brandid",
                DatasetAttribute: "_jarvis_brandid_value",
                subTable: "jarvis_brand",
                SubDatasetAttribute: "_jarvis_serviceline_value"
            },
            {
                TargetField: "jarvis_mileageunit",
                SourceTable: "jarvis_vehicle",
                Fieldtype: "Related",
                Field: "jarvis_countryofoperation",
                DatasetAttribute: "_jarvis_countryofoperation_value",
                subTable: "jarvis_country",
                SubDatasetAttribute: "_jarvis_mileageunit_value"
            },
            {
                TargetField: "jarvis_registrationnumber",
                SourceTable: "jarvis_vehicle",
                Fieldtype: "Text",
                Field: "jarvis_updatedregistrationnumber",
                DatasetAttribute: "jarvis_updatedregistrationnumber"
            }
        ]
    }

    const formContext = executionContext.getFormContext();
    let field = formContext.getAttribute("jarvis_vehicle");
    // Access the field on the form
    caseCntrls.SourceFields.forEach(SourceField => {
        if (field != null) {
            var value = field.getValue();
            if (value) {
                let record_id = field.getValue()[0].id;
                record_id = record_id.replace('{', '').replace('}', '');
                GetRelatedData(executionContext,
                    record_id,
                    SourceField.SourceTable,
                    SourceField.Fieldtype,
                    SourceField.DatasetAttribute,
                    SourceField.TargetField,
                    SourceField.subTable,
                    SourceField.SubDatasetAttribute);
            }
            else {
                if (executionContext.getFormContext().getAttribute(SourceField.TargetField) != null)
                    executionContext.getFormContext().getAttribute(SourceField.TargetField).setValue(null);
            }
        }
    });



    function GetRelatedData(executionContext, record_id, targetEntity, Fieldtype, DatasetAttribute, Field, subTable, SubDatasetAttribute) {
        const formContext = executionContext.getFormContext();
        Xrm.WebApi.retrieveRecord(targetEntity, record_id).then(
            function success(result) {
                if (result[DatasetAttribute]) {
                    if (Fieldtype == "Lookup") {
                        var lookup = new Array();
                        lookup[0] = new Object();
                        lookup[0].id = result[DatasetAttribute];
                        lookup[0].name = result[DatasetAttribute + "@OData.Community.Display.V1.FormattedValue"];
                        lookup[0].entityType = result[DatasetAttribute + "@Microsoft.Dynamics.CRM.lookuplogicalname"];
                        formContext.getAttribute(Field).setValue(lookup);
                        if (Field == "customerid") {
                            UpdateOneTimeCustomer(executionContext);
                            UpdateCDBCustomerDetails(executionContext);
                        }
                    }
                    else if (Fieldtype == "Related") {
                        Xrm.WebApi.retrieveRecord(subTable, result[DatasetAttribute]).then(
                            function success(result) {
                                if (result[SubDatasetAttribute]) {
                                    var lookup = new Array();
                                    lookup[0] = new Object();
                                    lookup[0].id = result[SubDatasetAttribute];
                                    lookup[0].name = result[SubDatasetAttribute + "@OData.Community.Display.V1.FormattedValue"];
                                    lookup[0].entityType = result[SubDatasetAttribute + "@Microsoft.Dynamics.CRM.lookuplogicalname"];
                                    formContext.getAttribute(Field).setValue(lookup);
                                    if (Field == "jarvis_caseserviceline") {
                                        var productId = Xrm.Page.data.entity.getId();
                                        if (productId != null)
                                            updateImageFromServiceLine(lookup[0].id, productId, formContext);
                                    }

                                }
                            });
                    }
                    else {
                        if (formContext.getAttribute(Field) != null)
                            formContext.getAttribute(Field).setValue(result[DatasetAttribute]);
                    }
                }

            }
        );
    }



}

InsideEscalationFilter = function (executionContext) {
    "use strict";
    let FilteredUserIds = [];
    const formContext = executionContext.getFormContext();
    let jarvisCountry = formContext.getAttribute("jarvis_country")?.getValue();
    let jarvisserviceline = formContext.getAttribute("jarvis_caseserviceline")?.getValue();
    let isescalated = formContext.getAttribute("isescalated")?.getValue();

    // use the filtered user Id to filter Subgrid
    let gridContext = formContext.getControl("Subgrid_new_72");

    var GridFetchXml = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>"
        + "<entity name='systemuser'>"
        + "<attribute name='fullname' />"
        + "<attribute name='systemuserid' />"
        + "<attribute name='internalemailaddress' />"
        + "<order attribute='fullname' descending='false' />"
        + "<filter type='and'><condition attribute='systemuserid' operator='in'>";

    if (isescalated && jarvisCountry != null && jarvisserviceline != null) {
        jarvisCountry = jarvisCountry[0].id;
        jarvisserviceline = jarvisserviceline[0].id;
        var fetchXml = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>"
            + "<entity name='systemuser'>"
            + "<attribute name='fullname' />"
            + "<attribute name='systemuserid' />"
            + "<order attribute='fullname' descending='false' />"
            + "<filter type='and'><condition attribute='isdisabled' operator='eq' value='0' /></filter>"
            + "<link-entity name='jarvis_escalationuser' from='jarvis_user' to='systemuserid' link-type='inner' alias='ab'>"
            + "<filter type='and'>"
            + "<condition attribute='jarvis_serviceline' operator='eq'  uitype='jarvis_serviceline' value='" + jarvisserviceline + "' />"
            + "<condition attribute='jarvis_country' operator='eq'  uitype='jarvis_country' value='" + jarvisCountry + "' />"
            + "</filter>"
            + "</link-entity>"
            + "</entity>"
            + "</fetch>";

        fetchXml = "?fetchXml=" + encodeURIComponent(fetchXml);
        Xrm.WebApi.retrieveMultipleRecords("systemuser", fetchXml).then(
            function success(result) {
                if (result.entities.length > 0) {
                    for (let row of result.entities) {
                        let userId = row["systemuserid"]; // Currency
                        GridFetchXml = GridFetchXml + "<value uitype='systemuser'>" + userId + "</value>";
                    }

                    GridFetchXml = GridFetchXml + "</condition>"
                    GridFetchXml = GridFetchXml + "</filter>"
                    GridFetchXml = GridFetchXml + "</entity>"
                    GridFetchXml = GridFetchXml + "</fetch>";

                    gridContext.setFilterXml(GridFetchXml);
                    // gridContext.getGrid().setParameter("fetchXml", GridFetchXml);
                    formContext.getControl("Subgrid_new_72").refresh();
                }
                else {
                    fetchXml = fetchXml + "<value uitype='systemuser'>d1d68bdcfb104dc7a5ea404444272362</value>";
                    fetchXml = fetchXml + "</condition>"
                    fetchXml = fetchXml + "</filter>"
                    fetchXml = fetchXml + "</entity>"
                    fetchXml = fetchXml + "</fetch>";
                    gridContext.setFilterXml(fetchXml);
                    formContext.getControl("Subgrid_new_72").refresh();

                }
            },
            function (error) {
                //console.log(error.message);
                // handle error conditions
            }
        );

    }
    else {
        GridFetchXml = GridFetchXml + "<value uitype='systemuser'>d1d68bdcfb104dc7a5ea404444272362</value>";
        GridFetchXml = GridFetchXml + "</condition>"
        GridFetchXml = GridFetchXml + "</filter>"
        GridFetchXml = GridFetchXml + "</entity>"
        GridFetchXml = GridFetchXml + "</fetch>";

        gridContext.setFilterXml(GridFetchXml);
        formContext.getControl("Subgrid_new_72").refresh();

    }
}

OutsideEscalationFilter = function (executionContext) {
    "use strict";
    const formContext = executionContext.getFormContext();
    let jarvis_homedealer = formContext.getAttribute("jarvis_homedealer")?.getValue();
    let isescalated = formContext.getAttribute("isescalated")?.getValue();
    let jarvisCaseId = Xrm.Page.data.entity.getId();
    let RepairingDealers = [];
    if (jarvis_homedealer != null) {
        RepairingDealers = [jarvis_homedealer[0].id.replace('{', '').replace('}', '')];
    }
    if (jarvisCaseId != null && isescalated) {
        jarvisCaseId = jarvisCaseId.replace('{', '').replace('}', '');
        Xrm.WebApi.retrieveMultipleRecords("jarvis_passout", "?$select=_jarvis_repairingdealer_value&$filter=(_jarvis_incident_value eq " + jarvisCaseId + " and statecode eq 0)").then(
            function success(results) {
                for (let row of results.entities) {
                    let repDealer = row["_jarvis_repairingdealer_value"]; // Currency
                    RepairingDealers.push(repDealer);
                }
                var gridContext = formContext.getControl("Subgrid_new_73");

                var GridFetchXml = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>"
                    + "<entity name='contact'>"
                    + "<attribute name='fullname' />"
                    + "<attribute name='contactid' />"
                    + "<attribute name='mobilephone' />"
                    + "<attribute name='jarvis_title' />"
                    + "<attribute name='emailaddress1' />"
                    + "<attribute name='jarvis_department' />"
                    + "<attribute name='jarvis_contacttimetype' />"
                    + "<attribute name='jarvis_sortorder' />"
                    + "<order attribute='fullname' descending='false' />"
                    + "<filter type='and'><condition attribute='contactid' operator='in'>";
                // Query Contacts to get the Ids of contacts to be filtered
                var FetchXml = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>"
                    + "<entity name='contact'>"
                    + "<attribute name='fullname' />"
                    + "<attribute name='contactid' />"
                    + "<order attribute='fullname' descending='false' />"
                    + "<link-entity name='jarvis_marketcompany_dealer' from='contactid' to='contactid' visible='false' intersect='true'>"
                    + "<link-entity name='account' from='accountid' to='accountid' alias='ao'>"
                    + "<link-entity name='account' from='jarvis_marketcompany' to='accountid' link-type='inner' alias='bb'>"
                    + "<filter type='and'>"
                    + "<condition attribute='accountid' operator='in'>";
                for (let repDealer of RepairingDealers) {
                    FetchXml = FetchXml + "<value uitype='account'>{" + repDealer + "}</value>";
                }
                FetchXml = FetchXml + "</condition>"
                    + "</filter>"
                    + "</link-entity>"
                    + "</link-entity>"
                    + "</link-entity>"
                    + "</entity>"
                    + "</fetch>";
                FetchXml = "?fetchXml=" + encodeURIComponent(FetchXml);
                Xrm.WebApi.retrieveMultipleRecords("contact", FetchXml).then(
                    function success(result) {
                        if (result.entities.length > 0) {
                            for (let row of result.entities) {
                                let userId = row["contactid"]; // Currency
                                GridFetchXml = GridFetchXml + "<value uitype='contact'>" + userId + "</value>";
                            }

                            GridFetchXml = GridFetchXml + "</condition>"
                            GridFetchXml = GridFetchXml + "</filter>"
                            GridFetchXml = GridFetchXml + "</entity>"
                            GridFetchXml = GridFetchXml + "</fetch>";

                            gridContext.setFilterXml(GridFetchXml);
                            formContext.getControl("Subgrid_new_73").refresh();
                        }
                    },
                    function (error) {
                        //console.log(error.message);
                        // handle error conditions
                    }
                );

                //gridContext.setFilterXml(FetchXml);
                //gridContext.getGrid().setParameter("fetchXml", FetchXml);
                formContext.getControl("Subgrid_new_73").refresh();
            },
            function (error) {
                //console.log(error.message);
            }
        );
    }

}

onChangeEscalation = function (executionContext) {
    "use strict";
    let field = formContext.getAttribute("isescalated");
    if (field != null) {
        var isescalated = field?.getValue();
        if (isescalated) {
            formContext.ui.setFormNotification("This Case is currently escalated", "WARNING", "ERR_ISESCALATED");
        }
        else {
            formContext.ui.clearFormNotification("ERR_ISESCALATED");
        }
    }

}

TimeValidionMessage = function (time) {
    "use strict";
    var hours = parseInt(time.substring(0, 2), 10);
    var minutes = parseInt(time.substring(2, 4), 10);
    if (hours < 0 || hours > 23 || minutes < 0 || minutes > 59) {
        return "“Please enter ETA Time within a valid range between 0000 and 2359”";
    }

    const regex = new RegExp(/^([01]\d|2[0-3])[0-5][0-9]$/);
    if (!regex.test(time)) {
        return "“Please enter valid format ‘hhmm’ on ETA Time”";
    }


    return null;
}




TimeValidationETA = function (executionContext) {
    "use strict";
    const formContext = executionContext.getFormContext();
    let ETATime = formContext.getAttribute("jarvis_etatimeappointment")?.getValue()

    if (ETATime != null) {
        let message = TimeValidionMessage(ETATime);
        if (message != null) {
            alert(message);
            formContext.getAttribute("jarvis_etatimeappointment").setValue(null);
        }
    }


}

SetETADate = function (executionContext) {
    "use strict";
    const formContext = executionContext.getFormContext();
    let assistanceType = formContext.getAttribute("jarvis_assistancetype").getValue();
    if (assistanceType != null) {
        if (assistanceType == 334030002 || assistanceType == 334030003) {
            let today = new Date();
            let date = formContext.getAttribute('jarvis_etadateappointment').getValue();
            if (date == null) {
                executionContext.getFormContext().getAttribute("jarvis_etadateappointment").setValue(today);
            }

        }
    }
}

onLoadSetImage = function (executionContext) {
    "use strict";
    const formContext = executionContext.getFormContext();
    let field = formContext.getAttribute("entityimage");
    let ServiceLine = formContext.getAttribute("jarvis_caseserviceline");
    let productId = Xrm.Page.data.entity.getId();


    if (field == null && ServiceLine != null && productId != null) {
        let serviceLineobj = ServiceLine?.getValue();

        if (serviceLineobj != null) {
            let serviceLineId = serviceLineobj[0].id;
            serviceLineId = serviceLineId.replace('{', '').replace('}', '');
            // Xrm.WebApi.retrieveRecord("incident", "" + productId + "", "?$select=statuscode", "jarvis_caseserviceline","_jarvis_caseserviceline_value@OData.Community.Display.V1.FormattedValue").then(
            // Copy from Case Service Line
            updateImageFromServiceLine(serviceLineId, productId, formContext);

        }

    }
}

IncidentNatureNotification = function (executionContext) {
    "use strict";
    const formContext = executionContext.getFormContext();

    let incidentNatureField = formContext.getControl("jarvis_incidentnature");
    if (incidentNatureField) {
        let incidentNatureData = incidentNatureField.getValue();
        if (incidentNatureData != null) {
            let incidentNatureValue = incidentNatureData.toString().toLowerCase();
            //Banner for Electric
            if (incidentNatureValue.includes("3ab23ec9-3ed9-ed11-a7c7-0022489d0994")) {
                formContext.ui.setFormNotification("“Electric truck - certified electric dealer required” ", "ERROR", "Electric_1");
            }
            else {
                formContext.ui.clearFormNotification("Electric_1");
            }
            //Banner for LNG
            if (incidentNatureValue.includes("403cad9e-44d9-ed11-a7c7-000d3ab1495c")) {
                formContext.ui.setFormNotification("“LNG vehicle - special tools/routines may be needed depending on initial diagnosis”", "ERROR", "LNG_1");
            }
            else {
                formContext.ui.clearFormNotification("LNG_1");
            }
            //Banner for CNG
            if (incidentNatureValue.includes("e441d9f0-c4d9-ed11-a7c7-000d3ad9bde0")) {
                formContext.ui.setFormNotification("“CNG vehicle - special tools/routines may be needed depending on initial diagnosis”", "ERROR", "CNG_1");
            }
            else {
                formContext.ui.clearFormNotification("CNG_1");
            }
        }
    }

}

checkContactIsDirty = function (formContext, fieldArr) {
    "use strict";
    for (var ii = 0; ii < fieldArr.length; ii++) {
        var isDirty = formContext.getData().getEntity().attributes.get(fieldArr[ii])?.getIsDirty();//
        if (isDirty != null && isDirty != undefined) {
            if (isDirty) {
                return true;
            }
        }
    }
    return false;
}
ismanualUpdate = function (executionContext) {
    "use strict";
    const formContext = executionContext.getFormContext();
    let manualField = formContext.getData().getEntity().attributes.get("jarvis_ismanualupdate");
    let fieldToDirty = ["jarvis_callerlanguage", "jarvis_casecontacttype", "jarvis_driverlanguage", "jarvis_email", "jarvis_firstname", "jarvis_phone", "jarvis_lastname", "jarvis_mobilephone", "jarvis_name", "jarvis_role"];
    var dirtyCheck = checkContactIsDirty(formContext, fieldToDirty);

    if (dirtyCheck) {
        manualField?.setValue(true);
    }
    else {
        manualField?.setValue(false);
    }
}


Lockmanual = function (executionContext) {
    "use strict";
    const formContext = executionContext.getFormContext();
    let column = formContext.getData().getEntity().attributes.getByName("jarvis_ismanualupdate");
    if (column) {
        // Disable the column
        column.controls.forEach(function (control) {
            control.setDisabled(true);
        });
    }
}

RefreshOnSave = function (executionContext) {
    "use strict";
    const formContext = executionContext.getFormContext();

    formContext.data.refresh(true);
    // Xrm.Utility.openEntityForm(Xrm.Page.data.entity.getEntityName(), Xrm.Page.data.entity.getId());
}

LockGopColumns = function (executionContext) {
    "use strict";
    const formContext = executionContext.getFormContext();
    let isApproved = formContext.getData().getEntity().attributes.get("jarvis_approved")?.getValue();
    if (isApproved == true) {
        let allColumns = formContext.getData().getEntity().attributes.get();
        allColumns.forEach(function (fields) {
            fields.controls.forEach(function (control) {
                if (fields.getName() != "statuscode") {
                    control.setDisabled(true);
                }
                else {
                    control.setDisabled(false);
                }
            });
        });
    }
}



UpdateCountryOnChangeRD = function (executionContext) {
    "use strict";
    const formContext = executionContext.getFormContext();
    let repairingDealer = formContext.getAttribute("jarvis_dealerappointment").getValue();
    let assistantType = formContext.getAttribute("jarvis_assistancetype").getValue();
    let caseStatus = formContext.getAttribute("statuscode").getValue();
    //formContext.getControl("header_process_jarvis_customerinformed").getAttribute().getValue()
    if (repairingDealer !== null && (assistantType === 334030002 || assistantType === 334030003) && caseStatus === 10) {
        var repairingDealerId = repairingDealer[0].id.replace('{', '').replace('}', '');
        Xrm.WebApi.retrieveRecord("account", repairingDealerId, "?$select=_jarvis_address1_country_value").then(
            function success(result) {
                var accountid = result["accountid"]; // Guid
                var jarvis_address1_country = result["_jarvis_address1_country_value"]; // Lookup
                var jarvis_address1_country_formatted = result["_jarvis_address1_country_value@OData.Community.Display.V1.FormattedValue"];
                var jarvis_address1_country_lookuplogicalname = result["_jarvis_address1_country_value@Microsoft.Dynamics.CRM.lookuplogicalname"];

                if (jarvis_address1_country !== null) {
                    var countryPopulate = new Array();
                    countryPopulate[0] = new Object();
                    countryPopulate[0].id = jarvis_address1_country;
                    countryPopulate[0].name = jarvis_address1_country_formatted;
                    countryPopulate[0].entityType = jarvis_address1_country_lookuplogicalname;
                    formContext.getAttribute("jarvis_country").setValue(countryPopulate);
                    formContext.getAttribute("jarvis_country").fireOnChange();
                }
            },
            function (error) {
                //console.log(error.message);
            }
        );
    }
}
OnAssistanceChange = function (executionContext) {
    "use strict";
    const formContext = executionContext.getFormContext();
    let repairingDealer = formContext.getAttribute("jarvis_dealerappointment")?.getValue();
    let assistanceType = formContext.getAttribute("jarvis_assistancetype")?.getValue();
    let caseStatus = formContext.getAttribute("statuscode").getValue();
    if (repairingDealer !== null && assistanceType !== null && (assistanceType === 334030000 || assistanceType === 334030001)) {
        let repairingDealer = formContext.getAttribute("jarvis_dealerappointment").setValue(null);
    }

    if (repairingDealer !== null && caseStatus === 10 && assistanceType !== null && (assistanceType === 334030000 || assistanceType === 334030001)) {
        if (formContext.getAttribute("jarvis_country")?.getValue() !== null) {
            formContext.getAttribute("jarvis_country").setValue(null);
        }
    }
    else if (repairingDealer !== null && caseStatus === 10 && assistanceType !== null && (assistanceType === 334030002 || assistanceType === 334030003)) {
        UpdateCountryOnChangeRD(executionContext);
    }
}

HideCreditCardFields = function (executionContext) {
    "use strict";
    const formContext = executionContext.getFormContext();
    let creditCardRequested = formContext.getAttribute("jarvis_totalrequestedccamount")?.getValue();
    if (creditCardRequested && creditCardRequested > 0) {
        formContext.getControl("jarvis_totalrequestedccamount")?.setVisible(true);
        formContext.getControl("jarvis_totalcreditcardpaymentamountcurrency")?.setVisible(true);
        formContext.getControl("jarvis_totalbookedamountinclvat")?.setVisible(true);
        formContext.getControl("jarvis_totalcreditcardrequestedamountcurreny")?.setVisible(true);
    }
    else {
        formContext.getControl("jarvis_totalrequestedccamount")?.setVisible(false);
        formContext.getControl("jarvis_totalcreditcardpaymentamountcurrency")?.setVisible(false);
        formContext.getControl("jarvis_totalbookedamountinclvat")?.setVisible(false);
        formContext.getControl("jarvis_totalcreditcardrequestedamountcurreny")?.setVisible(false);

    }

}