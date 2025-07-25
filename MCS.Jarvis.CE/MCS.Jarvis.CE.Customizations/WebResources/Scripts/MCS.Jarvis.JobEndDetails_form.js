var Jarvis = Jarvis || {};


Jarvis.JobEndDetailsform = {

    onFieldChange: function (executionContext) {
        "use strict";
        const cntrls = {
            SourceFields: [

                {
                    TargetField: "jarvis_mileage",
                    SourceTable: "incident",
                    Fieldtype: "Text",
                    Field: "jarvis_mileage",
                    DatasetAttribute: "jarvis_mileage"
                },
                {
                    TargetField: "jarvis_mileageunit",
                    SourceTable: "incident",
                    Fieldtype: "Lookup",
                    Field: "jarvis_mileageunit",
                    DatasetAttribute: "_jarvis_mileageunit_value"
                }
            ]
        }

        const formContext = executionContext.getFormContext();
        const field = formContext.getAttribute("jarvis_incident");
        // Access the field on the form
        cntrls.SourceFields.forEach(SourceField => {
            if (field !== null) {
                const value = field.getValue();
                if (value) {
                    let record_id = field.getValue()[0].id;
                    record_id = record_id.replace('{', '').replace('}', '');
                    const targetField = formContext.getAttribute(SourceField.TargetField).getValue();
                    if (targetField !== null) {
                        record_id = null;
                    }
                    else {
                        Jarvis.JobEndDetailsform.GetRelatedData(executionContext,
                            record_id,
                            SourceField.SourceTable,
                            SourceField.Fieldtype,
                            SourceField.DatasetAttribute,
                            SourceField.TargetField);
                    }
                }
                else {
                    executionContext.getFormContext().getAttribute(SourceField.TargetField).setValue(null);
                }
            }
        });
    },
    GetRelatedData: function (executionContext, record_id, targetEntity, Fieldtype, DatasetAttribute, Field) {
        "use strict";
        const formContext = executionContext.getFormContext();
        Xrm.WebApi.retrieveRecord(targetEntity, record_id).then(
            function success(result) {
                if (result[DatasetAttribute]) {
                    if (Fieldtype === "Lookup") {
                        const lookup = new Array();
                        lookup[0] = new Object();
                        lookup[0].id = result[DatasetAttribute];
                        lookup[0].name = result[DatasetAttribute + "@OData.Community.Display.V1.FormattedValue"];
                        lookup[0].entityType = result[DatasetAttribute + "@Microsoft.Dynamics.CRM.lookuplogicalname"];
                        formContext.getAttribute(Field).setValue(lookup);
                    }
                    else {
                        formContext.getAttribute(Field).setValue(result[DatasetAttribute]);
                    }
                }

            }
        );
    },
    populatePassoutRD: function (executionContext) {
        "use strict";
        const formContext = executionContext.getFormContext();
        if (formContext.ui.getFormType() === 1) {
            let fetchXml = "";
            let caseId = formContext.getAttribute("jarvis_incident")?.getValue()[0]?.id;
            if (caseId !== null && caseId !== undefined) {
                caseId = caseId.replace('{', '').replace('}', '');

                fetchXml = `<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
        <entity name='jarvis_passout'>
          <attribute name='jarvis_passoutid' />
          <attribute name='jarvis_name' />
          <attribute name='modifiedon' />
          <order attribute='modifiedon' descending='true' />
          <filter type='and'>
          <filter type='and'>
            <condition attribute='jarvis_incident' operator='eq' uitype='incident' value='{`+ caseId + `}' />
            <condition attribute='statecode' operator='eq' value='0' />
          </filter>
              <filter type='or'>
			<condition attribute='statuscode' operator='eq' value='334030002' />
			<condition attribute='statuscode' operator='eq' value='334030001' />
         </filter>
        </filter>
        </entity>
      </fetch>`;

                fetchXml = "?fetchXml=" + encodeURIComponent(fetchXml);
                Xrm.WebApi.retrieveMultipleRecords("jarvis_passout", fetchXml).then(
                    function success(result) {
                        if (result.entities.length === 1) {
                            const passout = result.entities[0];
                            if (passout["jarvis_passoutid"]) {
                                let lookup = new Array();
                                lookup[0] = new Object();
                                lookup[0].id = passout["jarvis_passoutid"];
                                lookup[0].name = passout["jarvis_name"];
                                lookup[0].entityType = "jarvis_passout";
                                executionContext.getFormContext().getAttribute("jarvis_repairingdealerpassout").setValue(lookup);

                            }

                        }
                    },
                    function (error) {

                    }
                );
            }
        }
    },
    onJEDSave: function (executionContext) {
        "use strict";
        const formContext = executionContext.getFormContext();
        const fieldControl = formContext.getControl("statuscode");
        const headerfieldControl = formContext.getControl("header_statuscode");
        const fieldValue = formContext.getAttribute("statuscode")?.getValue();
        ////643487 - Unlock If status is Failed.
        if (fieldControl && fieldValue && headerfieldControl && (fieldValue === 334030002 || fieldValue === 334030003))// 
        {
            fieldControl.setDisabled(false);
            headerfieldControl.setDisabled(false);
        }
        else if (fieldControl && fieldValue && headerfieldControl && fieldValue === 334030001) {
            fieldControl.setDisabled(true);
            headerfieldControl.setDisabled(true);
        }

    },

    onJEDLoad: function (executionContext) {
        "use strict";
        Jarvis.JobEndDetailsform.populatePassoutRD(executionContext);
    },
    LockField: function (executionContext) {
        "use strict";
        const formContext = executionContext.getFormContext();
        const allControls = formContext.ui.controls.get();
        const statusField = formContext.getControl("statuscode");
        const formType = formContext.ui.getFormType();
        if (formType !== 1) {
            if (statusField !== null) {
                //Remove Has been Sent on change
                formContext.getControl("statuscode")?.removeOption(334030002);
                ////643487 - Hide Failed
                formContext.getControl("statuscode")?.removeOption(334030003);
                const statusValue = formContext.getAttribute("statuscode")?.getValue();
                if (statusValue && statusValue === 334030001) {
                    formContext.getControl("jarvis_communicationfailedreason")?.setVisible(false);
                    formContext.getAttribute("jarvis_communicationfailedreason")?.setValue(null);
                    allControls.forEach((control) => {
                        // Get the field name of the current control
                        const fieldName = control.getName();

                        if (fieldName !== "statuscode" && fieldName !== "header_statuscode") {
                            // Disable the control to lock the field
                            control.setDisabled(true);
                        }
                    });
                }
                else if (statusValue === 334030000) {
                    formContext.getControl("jarvis_communicationfailedreason")?.setVisible(false);
                    formContext.getAttribute("jarvis_communicationfailedreason")?.setValue(null);
                    allControls.forEach((control) => {
                        // Enable the control to lock the field
                        //// 569973
                        const fieldName = control.getName();
                        if (fieldName !== "jarvis_mileage" && fieldName !== "jarvis_mileageunit" && fieldName !== "jarvis_repairontrailer") {
                            control.setDisabled(false);
                        }
                        else {
                            control.setDisabled(true);
                        }

                    });

                }
                //// Status is Failed
                else if (statusValue === 334030003) {
                    formContext.getControl("jarvis_communicationfailedreason")?.setVisible(true);
                    allControls.forEach((control) => {
                        // Get the field name of the current control
                        const fieldName = control.getName();

                        if (fieldName !== "statuscode" && fieldName !== "header_statuscode") {
                            // Disable the control to lock the field
                            control.setDisabled(true);
                        }
                    });
                    //Remove HasBeen sent option
                    formContext.getControl("statuscode")?.removeOption(334030002);
                }
            }
        }

    },
    LockRecordOnLoad: function (executionContext) {
        "use strict";
        const formContext = executionContext.getFormContext();
        const statusField = formContext.getControl("statuscode");
        const allControls = formContext.ui.controls.get();
        const formType = formContext.ui.getFormType();
        let statusValue = 0;
        if (formType !== 1) {
            if (statusField !== null) {
                statusValue = formContext.getAttribute("statuscode")?.getValue()
            }

            if (statusValue === 334030002) {
                formContext.getControl("jarvis_communicationfailedreason")?.setVisible(false);
                allControls.forEach((control) => {
                    // Get the field name of the current control
                    const fieldName = control.getName();

                    if (fieldName !== "statuscode" && fieldName !== "header_statuscode") {
                        // Disable the control to lock the field
                        control.setDisabled(true);
                    }
                });
                ////643487 - Hide Failed
                formContext.getControl("statuscode")?.removeOption(334030003);
            }
            //status is send
            else if (statusValue === 334030001) {
                formContext.getControl("jarvis_communicationfailedreason")?.setVisible(false);
                allControls.forEach((control) => {

                    // Disable the control to lock the field
                    control.setDisabled(true);
                    //Remove HasBeen sent option
                    formContext.getControl("statuscode")?.removeOption(334030002);
                });
                ////643487 - Hide Failed
                formContext.getControl("statuscode")?.removeOption(334030003);
            }
            //status is Draft
            else if (statusValue === 334030000) {
                formContext.getControl("jarvis_communicationfailedreason")?.setVisible(false);
                allControls.forEach((control) => {
                    const fieldName = control.getName();
                    //// 569973
                    if (fieldName !== "jarvis_mileage" && fieldName !== "jarvis_mileageunit" && fieldName !== "jarvis_repairontrailer") {
                        // Enable the control to lock the field
                        control.setDisabled(false);
                    }
                    else {
                        control.setDisabled(true);
                    }
                    //Remove HasBeen sent option
                    formContext.getControl("statuscode")?.removeOption(334030002);
                });
                ////643487 - Hide Failed
                formContext.getControl("statuscode")?.removeOption(334030003);
            }
            //// Status is Failed
            else if (statusValue === 334030003) {
                formContext.getControl("jarvis_communicationfailedreason")?.setVisible(true);
                allControls.forEach((control) => {
                    // Get the field name of the current control
                    const fieldName = control.getName();

                    if (fieldName !== "statuscode" && fieldName !== "header_statuscode") {
                        // Disable the control to lock the field
                        control.setDisabled(true);
                    }
                });
                //Remove HasBeen sent option
                formContext.getControl("statuscode")?.removeOption(334030002);
            }
        }
        // if Form type is create
        else if (formType === 1) {
            //Remove Has Been sent option
            formContext.getControl("statuscode")?.removeOption(334030002);
            ////643487 - Hide Failed
            formContext.getControl("statuscode")?.removeOption(334030003);
        }

    },

    //// 569973
    LockJEDGridField: function (executionContext) {
        "use strict";
        const formContext = executionContext.getFormContext();
        if (formContext) {
            const arrUnlockFields = ["jarvis_mileage", "jarvis_mileageunit", "jarvis_repairontrailer"];
            const objEntity = formContext.data.entity;
            objEntity.attributes.forEach(function (attribute) {
                if (arrUnlockFields.indexOf(attribute.getName()) > -1) {
                    const attributeToEnable = attribute.controls.get(0);
                    attributeToEnable.setDisabled(true);
                }
            }
            )
        };
    }
}