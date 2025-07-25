var Jarvis = Jarvis || {};

Jarvis.passout = {

    notEnoughGOPAmount: false,
    dateIsFuture: false,
    caseAvailableAmount: null,
    caseAvailableCurrency: null,

    PassOutOnRDChange: function (executionContext) {
        "use strict";
        let formContext = executionContext.getFormContext();
        let repairingdealer = formContext.getAttribute("jarvis_repairingdealer")?.getValue();
        let gopCurrency = formContext.getAttribute("transactioncurrencyid")?.getValue();
        if (repairingdealer == null) { return };
        let repairingdealerid = repairingdealer[0].id.slice(1, -1);
        Xrm.WebApi.retrieveRecord("account", repairingdealerid, "?$select=_jarvis_currency_value").then(
            function success(result) {
                console.log(result);
                let jarvis_currency = result["_jarvis_currency_value"]; // Lookup
                let jarvis_currency_formatted = result["_jarvis_currency_value@OData.Community.Display.V1.FormattedValue"];
                let jarvis_currency_lookuplogicalname = result["_jarvis_currency_value@Microsoft.Dynamics.CRM.lookuplogicalname"];
                if (jarvis_currency == null) { return };
                let jarvisCurrency = [
                    {
                        id: jarvis_currency,
                        name: jarvis_currency_formatted,
                        entityType: jarvis_currency_lookuplogicalname
                    }
                ];
                formContext.getAttribute("transactioncurrencyid").setValue(jarvisCurrency);
                Jarvis.passout.OnCurrencyUpdate(executionContext);
                Jarvis.passout.onPassOutCurrencyChange(executionContext);
            },
            function (error) {
                console.log(error.message);
            }
        );
    },
    PassOutOnChangeOfGOPOUT: function (executionContext) {
        "use strict";
        let formContext = executionContext.getFormContext();
        var formType = formContext.ui.getFormType();
        let jarvisCase = formContext.getAttribute("jarvis_incident")?.getValue();
        if (jarvisCase === null || jarvisCase === "") {
            return;
        }
        let jarvisPassOutId = formContext.data.entity.getId().slice(1, -1)
        let jarvisCaseId = jarvisCase[0].id.slice(1, -1);
        let gopLimitOut = formContext.getAttribute("jarvis_goplimitout")?.getValue();
        let passoutcurrency = null;

        let passoutcurrencyvalue = formContext.getAttribute("transactioncurrencyid")?.getValue();
        if (passoutcurrencyvalue)
            passoutcurrency = passoutcurrencyvalue[0].id;
        if (passoutcurrency) {
            passoutcurrency = passoutcurrency.replace('{', '').replace('}', '');
        }

        formContext.ui.clearFormNotification("ERR_PassOutAmountValidation");
        formContext.ui.clearFormNotification("WAR_PassOutCurrencyValidation");
        let passoutSent = formContext.getAttribute("jarvis_passoutaccepted")?.getValue();
        if ((!passoutSent && gopLimitOut != null )|| (formType==1 && gopLimitOut != null && gopLimitOut>0)) {
            if (Jarvis.passout.caseAvailableCurrency && Jarvis.passout.caseAvailableAmount) {
                if (passoutcurrency && Jarvis.passout.caseAvailableCurrency) {
                    let conversionrate = 0;
                    if (passoutcurrency.toUpperCase() == Jarvis.passout.caseAvailableCurrency.toUpperCase()) {
                        var totalPassoutAmount = gopLimitOut * 1;
                        if (totalPassoutAmount > Jarvis.passout.caseAvailableAmount) {
                            Jarvis.passout.notEnoughGOPAmount = true;
                            formContext.ui.setFormNotification("Not enough GOP available Amount", "ERROR", "ERR_PassOutAmountValidation");//statuscode
                            //executionContext.getEventArgs().preventDefault();
                        }
                        else
                            Jarvis.passout.notEnoughGOPAmount = false;
                    }
                    else {
                        Xrm.WebApi.retrieveMultipleRecords("jarvis_exchangerate", "?$select=jarvis_value&$filter=(_jarvis_basecurrency_value eq " + passoutcurrency + " and _jarvis_countercurrency_value eq " + Jarvis.passout.caseAvailableCurrency + ")").then(
                            function success(result) {
                                if (result.entities.length > 0) {
                                    for (let row of result.entities) {
                                        if (row["jarvis_value"]) {
                                            var conversionrate = row["jarvis_value"];
                                            var totalPassoutAmount = gopLimitOut * conversionrate;
                                            if (totalPassoutAmount > Jarvis.passout.caseAvailableAmount) {
                                                Jarvis.passout.notEnoughGOPAmount = true;
                                                formContext.ui.setFormNotification("Not enough GOP available Amount", "ERROR", "ERR_PassOutAmountValidation");//statuscode
                                                //executionContext.getEventArgs().preventDefault();
                                            }
                                            else
                                                Jarvis.passout.notEnoughGOPAmount = false;
                                        }
                                        else {
                                            formContext.ui.setFormNotification("Conversion rate not available for Passout Currency and Case Available Amount Curreny ", "WARNING", "WAR_PassOutCurrencyValidation");
                                        }
                                    }
                                }
                                else {
                                    formContext.ui.setFormNotification("Conversion rate not available for Passout Currency and Case Available Amount Curreny ", "WARNING", "WAR_PassOutCurrencyValidation");
                                }
                            },
                            function (error) {
                                console.log(error.message);
                            }
                        );
                    }
                }
            }
            else {
                Xrm.WebApi.retrieveRecord("incident", jarvisCaseId, "?$select=jarvis_totalgoplimitoutapproved,_jarvis_totalgoplimitoutapprovedcurrency_value,jarvis_totalpassoutamount,_jarvis_totalpassoutcurrency_value,jarvis_restgoplimitout,_jarvis_totalrestcurrencyout_value").then(
                    function success(result) {
                        let jarvis_totalgoplimitoutapproved = result["jarvis_totalgoplimitoutapproved"]; // Decimal

                        if (result["jarvis_restgoplimitout"])
                            Jarvis.passout.caseAvailableAmount = result["jarvis_restgoplimitout"];
                        else
                            Jarvis.passout.caseAvailableAmount = 0;

                        //let jarvis_totalrestcurrencyout = null
                        if (result["_jarvis_totalrestcurrencyout_value"])
                            Jarvis.passout.caseAvailableCurrency = result["_jarvis_totalrestcurrencyout_value"];
                        else
                            Jarvis.passout.caseAvailableCurrency = passoutcurrency;


                        // Get exchange rate of passout currency and GOp avialble amount
                        if (passoutcurrency && Jarvis.passout.caseAvailableCurrency) {
                            // let conversionrate = Jarvis.passout.currencyExchange(passoutcurrency,jarvis_totalrestcurrencyout);
                            let conversionrate = 0;
                            if (passoutcurrency.toUpperCase() == Jarvis.passout.caseAvailableCurrency.toUpperCase()) {
                                var totalPassoutAmount = gopLimitOut * 1;
                                if (totalPassoutAmount > Jarvis.passout.caseAvailableAmount) {
                                    Jarvis.passout.notEnoughGOPAmount = true;
                                    formContext.ui.setFormNotification("Not enough GOP available Amount", "ERROR", "ERR_PassOutAmountValidation");//statuscode
                                    //executionContext.getEventArgs().preventDefault();
                                }
                                else
                                    Jarvis.passout.notEnoughGOPAmount = false;
                            }
                            else {
                                Xrm.WebApi.retrieveMultipleRecords("jarvis_exchangerate", "?$select=jarvis_value&$filter=(_jarvis_basecurrency_value eq " + passoutcurrency + " and _jarvis_countercurrency_value eq " + Jarvis.passout.caseAvailableCurrency + ")").then(
                                    function success(result) {
                                        if (result.entities.length > 0) {
                                            for (let row of result.entities) {
                                                if (row["jarvis_value"]) {
                                                    var conversionrate = row["jarvis_value"];
                                                    var totalPassoutAmount = gopLimitOut * conversionrate;
                                                    if (totalPassoutAmount > Jarvis.passout.caseAvailableAmount) {
                                                        Jarvis.passout.notEnoughGOPAmount = true;
                                                        formContext.ui.setFormNotification("Not enough GOP available Amount", "ERROR", "ERR_PassOutAmountValidation");//statuscode
                                                        //executionContext.getEventArgs().preventDefault();
                                                    }
                                                    else
                                                        Jarvis.passout.notEnoughGOPAmount = false;
                                                }
                                                else {
                                                    formContext.ui.setFormNotification("Conversion rate not available for Passout Currency and Case Available Amount Curreny ", "WARNING", "WAR_PassOutCurrencyValidation");
                                                }
                                            }
                                        }
                                        else {
                                            formContext.ui.setFormNotification("Conversion rate not available for Passout Currency and Case Available Amount Curreny ", "WARNING", "WAR_PassOutCurrencyValidation");
                                        }
                                    },
                                    function (error) {
                                        console.log(error.message);
                                    }
                                );
                            }
                        }

                    },
                    function (error) {
                        console.log(error.message);
                    }
                );
            }
        }
    },
    onPassoutSave: function (executionContext) {
        "use strict";
        let formContext = executionContext.getFormContext();
        var formType = formContext.ui.getFormType();
        let passoutSent = formContext.getAttribute("jarvis_passoutaccepted")?.getValue();
        if (formType === 1 ||!passoutSent) {
            if (Jarvis.passout.notEnoughGOPAmount) {

                formContext.ui.setFormNotification("Not enough GOP available Amount", "ERROR", "ERR_PassOutAmountValidation");//statuscode
                executionContext.getEventArgs().preventDefault();

            }
            else {
                if (Jarvis.passout.caseAvailableCurrency && Jarvis.passout.caseAvailableAmount) {
                    let passoutcurrency = null;

                    let passoutcurrencyvalue = formContext.getAttribute("transactioncurrencyid")?.getValue();
                    if (passoutcurrencyvalue)
                        passoutcurrency = passoutcurrencyvalue[0].id;
                    if (passoutcurrency) {
                        passoutcurrency = passoutcurrency.replace('{', '').replace('}', '');
                    }
                    let gopLimitOut = formContext.getAttribute("jarvis_goplimitout")?.getValue();
                    if (passoutcurrency && Jarvis.passout.caseAvailableCurrency) {
                        let conversionrate = 0;
                        if (passoutcurrency.toUpperCase() == Jarvis.passout.caseAvailableCurrency.toUpperCase()) {
                            var totalPassoutAmount = gopLimitOut * 1;
                            if (totalPassoutAmount > Jarvis.passout.caseAvailableAmount) {
                                Jarvis.passout.notEnoughGOPAmount = true;
                                formContext.ui.setFormNotification("Not enough GOP available Amount", "ERROR", "ERR_PassOutAmountValidation");//statuscode
                                executionContext.getEventArgs().preventDefault();
                            }
                            else
                                Jarvis.passout.notEnoughGOPAmount = false;
                        }
                    }
                }
                Jarvis.passout.OnCurrencyUpdate(executionContext);
                formContext.ui.clearFormNotification("SubmitForApproval");
            }
        }
        let fieldControl = formContext.getControl("statuscode");
        let headerfieldControl = formContext.getControl("header_statuscode");
        let fieldValue = formContext.getAttribute("statuscode")?.getValue();
        if (fieldControl && fieldValue && fieldValue == 334030001) {
            formContext.getAttribute("jarvis_passoutaccepted").setValue(true)
        }
        if (fieldControl && fieldValue && headerfieldControl && (fieldValue == 334030002 || fieldValue == 334030003))// 
        {
            fieldControl.setDisabled(false);
            headerfieldControl.setDisabled(false);

        }
        else if (fieldControl && fieldValue && headerfieldControl && fieldValue == 334030001) {
            fieldControl.setDisabled(true);
            headerfieldControl.setDisabled(true);
        }

    },

    LockField: function (executionContext) {
        "use strict";
        let formContext = executionContext.getFormContext();
        const allControls = formContext.ui.controls.get();
        let statusField = formContext.getControl("statuscode");
        let formType = formContext.ui.getFormType();
        if (formType != 1) {
            if (statusField != null) {
                //Remove Has been Sent on change and Failed
                formContext.getControl("statuscode")?.removeOption(334030002);
                formContext.getControl("statuscode")?.removeOption(334030003);

                var statusValue = formContext.getAttribute("statuscode")?.getValue()
                if (statusValue == 334030001) {
                    formContext.getControl("jarvis_communicationfailedreason")?.setVisible(false);
                    formContext.getAttribute("jarvis_communicationfailedreason")?.setValue(null);
                    allControls.forEach((control) => {
                        // Get the field name of the current control
                        const fieldName = control.getName();

                        if (fieldName !== "statuscode" && fieldName !== "header_statuscode" && fieldName !== "jarvis_etatime" && fieldName !== "jarvis_atatime" && fieldName !== "jarvis_etctime" && fieldName !== "jarvis_atctime" &&
                            fieldName !== "jarvis_etadate" && fieldName !== "jarvis_atadate" && fieldName !== "jarvis_etcdate" && fieldName !== "jarvis_atcdate" && fieldName !== "jarvis_reason") {
                            // Disable the control to lock the field
                            control.setDisabled(true);
                        }
                        else {
                            control.setDisabled(false);
                        }
                    });
                }
                else if (statusValue == 334030002) {
                    formContext.getControl("jarvis_communicationfailedreason")?.setVisible(false);
                    formContext.getAttribute("jarvis_communicationfailedreason")?.setValue(null);
                    allControls.forEach((control) => {
                        // Get the field name of the current control
                        const fieldName = control.getName();
                        if (fieldName !== "statuscode" && fieldName !== "header_statuscode" && fieldName !== "jarvis_etatime" && fieldName !== "jarvis_atatime" && fieldName !== "jarvis_etctime" && fieldName !== "jarvis_atctime" &&
                            fieldName !== "jarvis_etadate" && fieldName !== "jarvis_atadate" && fieldName !== "jarvis_etcdate" && fieldName !== "jarvis_atcdate" && fieldName !== "jarvis_reason") {
                            // Disable the control to lock the field
                            control.setDisabled(true);
                        }
                        else {
                            control.setDisabled(false);
                        }
                    });
                }
                else if (statusValue == 334030003) {
                    formContext.getControl("jarvis_communicationfailedreason")?.setVisible(true);
                    allControls.forEach((control) => {
                        // Get the field name of the current control
                        const fieldName = control.getName();
                        if (fieldName !== "statuscode" && fieldName !== "header_statuscode") {
                            // Disable the control to lock the field
                            control.setDisabled(true);
                        }
                        else {
                            control.setDisabled(false);
                        }
                    });
                }
                else if (statusValue == 1) {
                    formContext.getControl("jarvis_communicationfailedreason")?.setVisible(false);
                    formContext.getAttribute("jarvis_communicationfailedreason")?.setValue(null);
                    let passoutSent = formContext.getAttribute("jarvis_passoutaccepted")?.getValue();
                    allControls.forEach((control) => {
                        const fieldName = control.getName();
                        if (fieldName !== "jarvis_casegopoutavailableamount") {
                            // Enable the control to lock the field
                            control.setDisabled(false);
                        }
                        if (passoutSent && (fieldName == "jarvis_goplimitout" || fieldName == "transactioncurrencyid")) {
                            control.setDisabled(true);
                        }

                    });
                }
            }
        }


    },
    LockRecordOnLoad: function (executionContext) {
        "use strict";
        let formContext = executionContext.getFormContext();
        let statusField = formContext.getControl("statuscode");
        const allControls = formContext.ui.controls.get();
        let formType = formContext.ui.getFormType();
        // if Form type is Not create 
        if (formType != 1) {
            if (statusField != null)
                var statusValue = formContext.getAttribute("statuscode")?.getValue()
            //status is Has been sent
            if (statusValue == 334030002) {
                ////643487 - Remove Failed Option.
                formContext.getControl("statuscode")?.removeOption(334030003);
                allControls.forEach((control) => {
                    // Get the field name of the current control
                    const fieldName = control.getName();
                    if (fieldName !== "statuscode" && fieldName !== "header_statuscode" && fieldName !== "jarvis_etatime" && fieldName !== "jarvis_atatime" && fieldName !== "jarvis_etctime" && fieldName !== "jarvis_atctime" &&
                        fieldName !== "jarvis_etadate" && fieldName !== "jarvis_atadate" && fieldName !== "jarvis_etcdate" && fieldName !== "jarvis_atcdate" && fieldName !== "jarvis_reason") {
                        // Disable the control to lock the field
                        control.setDisabled(true);
                    }
                    else {
                        control.setDisabled(false);

                    }
                });
            }
            //status is Send
            else if (statusValue == 334030001) {
                ////643487 - Remove Failed Option.
                formContext.getControl("statuscode")?.removeOption(334030003);
                allControls.forEach((control) => {
                    const fieldName = control.getName();
                    // Disable the control to lock the field
                    if (fieldName !== "jarvis_etatime" && fieldName !== "jarvis_atatime" && fieldName !== "jarvis_etctime" && fieldName !== "jarvis_atctime" &&
                        fieldName !== "jarvis_etadate" && fieldName !== "jarvis_atadate" && fieldName !== "jarvis_etcdate" && fieldName !== "jarvis_atcdate" && fieldName !== "jarvis_reason") {
                        // Disable the control to lock the field
                        control.setDisabled(true);
                    }
                    // control.setDisabled(true);
                    //Remove HasBeen sent option 
                    formContext.getControl("statuscode")?.removeOption(334030002);

                });

            }
            ////643487 - Status is Failed
            else if (statusValue == 334030003) {
                allControls.forEach((control) => {
                    const fieldName = control.getName();
                    // Disable the control to lock the field
                    if (fieldName !== "statuscode" && fieldName !== "header_statuscode") {
                        // Disable the control to lock the field
                        control.setDisabled(true);
                    }
                    // control.setDisabled(true);
                    //Remove HasBeen sent option 
                    formContext.getControl("statuscode")?.removeOption(334030002);
                    formContext.getControl("jarvis_communicationfailedreason")?.setVisible(true);

                });
            }
            //status is Draft
            else if (statusValue == 1) {
                let passoutSent = formContext.getAttribute("jarvis_passoutaccepted")?.getValue();
                ////643487 - Remove Failed Option.
                formContext.getControl("statuscode")?.removeOption(334030003);
                allControls.forEach((control) => {

                    const fieldName = control.getName();
                    if (fieldName !== "jarvis_casegopoutavailableamount") {
                        // Enable the control to lock the field
                        control.setDisabled(false);
                    }
                    if (passoutSent && (fieldName == "jarvis_goplimitout" || fieldName == "transactioncurrencyid")) {
                        control.setDisabled(true);
                    }
                    //Remove HasBeen sent option
                    formContext.getControl("statuscode")?.removeOption(334030002);
                });

            }

        }
        // if Form type is create
        else if (formType == 1) {
            //Remove Has Been sent option
            formContext.getControl("statuscode")?.removeOption(334030002);
            ////643487 - Remove Failed Option.
            formContext.getControl("statuscode")?.removeOption(334030003);
        }
    },
    ETAAppointment: function (executionContext) {
        "use strict";
        let formContext = executionContext.getFormContext();
        let formType = formContext.ui.getFormType();
        if (formType == 1) {
            let incident = formContext.getAttribute("jarvis_incident")?.getValue();
            if (incident == null) { return };
            let incidentid = incident[0].id.slice(1, -1);
            Xrm.WebApi.retrieveRecord("incident", incidentid, "?$select=jarvis_etatimeappointment,jarvis_etadateappointment,_jarvis_dealerappointment_value").then(
                function success(result) {

                    let dealerappointment = result["_jarvis_dealerappointment_value"]; // Lookup
                    let dealerappointment_formatted = result["_jarvis_dealerappointment_value@OData.Community.Display.V1.FormattedValue"];
                    let dealerappointment_lookuplogicalname = result["_jarvis_dealerappointment_value@Microsoft.Dynamics.CRM.lookuplogicalname"];
                    if (dealerappointment != null) {
                        let jarvis_Repairingdealer = [
                            {
                                id: dealerappointment,
                                name: dealerappointment_formatted,
                                entityType: dealerappointment_lookuplogicalname
                            }
                        ];
                        formContext.getAttribute("jarvis_repairingdealer").setValue(jarvis_Repairingdealer);
                        Jarvis.passout.PassOutOnRDChange(executionContext);
                    }
                    if (result.jarvis_etatimeappointment != null) {
                        formContext.getAttribute("jarvis_etatime").setValue(result.jarvis_etatimeappointment)
                    }
                    if (result.jarvis_etadateappointment != null) {
                        formContext.getAttribute("jarvis_etadate").setValue(new Date(result.jarvis_etadateappointment))
                    }
                    if (result.jarvis_etatimeappointment != null || result.jarvis_etadateappointment != null) {
                        ////Jarvis.passout.populateDateTime(formContext, "jarvis_etatime", "jarvis_etadate", "jarvis_eta");
                    }
                }

            );

        }

    },


    OnETATimeChange: function (executionContext) {
        "use strict";
        let formContext = executionContext.getFormContext();
        let ETATime = formContext.getAttribute("jarvis_etatime");
        if (ETATime == null) { return };
        let fieldVal = formContext.getAttribute("jarvis_etatime").getValue();
        const positiveIntegersRegex = new RegExp(/^0\d{3}$|^[1-9]\d{3}$/);
        const timeRegex = new RegExp(/^(0\d|1\d|2[0-3])[0-5]\d$/);
        if (ETATime.getValue() === null) {
            formContext.getAttribute("jarvis_eta").setValue(null);
        }
        if (!positiveIntegersRegex.test(fieldVal)) {
            Xrm.Navigation.openAlertDialog("Please enter valid time format hhmm");
            executionContext.getFormContext().getAttribute("jarvis_etatime").setValue(null);
            return;
        }

        if (positiveIntegersRegex.test(fieldVal) && !timeRegex.test(fieldVal)) {
            Xrm.Navigation.openAlertDialog("Please enter ETA Time within a valid range between 0000 and 2359");
            executionContext.getFormContext().getAttribute("jarvis_etatime").setValue(null);
            return;
        }
        ////Jarvis.passout.populateDateTime(formContext, "jarvis_etatime", "jarvis_etadate", "jarvis_eta");
        //let incident = formContext.getAttribute("jarvis_incident")?.getValue();
        //if (incident == null) { return };
        //let data = { "jarvis_etatimeappointment": ETATime.getValue() };
        //let incidentid = incident[0].id.slice(1, -1);
        //Xrm.WebApi.updateRecord("incident", incidentid, data);


    },
    OnETADateChange: function (executionContext) {
        "use strict";
        let formContext = executionContext.getFormContext();
        Jarvis.passout.populateDateTime(formContext, "jarvis_etatime", "jarvis_etadate", "jarvis_eta");
        let ETADate = formContext.getAttribute("jarvis_etadate");
        // if (ETADate == null) { return };
        // let incident = formContext.getAttribute("jarvis_incident")?.getValue();
        // if (incident == null) { return };
        // let data = { "jarvis_etadateappointment": new Date(ETADate.getValue()) };
        // let incidentid = incident[0].id.slice(1, -1);
        // Xrm.WebApi.updateRecord("incident", incidentid, data);


    },
    OnATAtimeChange: function (executionContext) {
        "use strict";
        let formContext = executionContext.getFormContext();
        var timefield = "jarvis_atatime";
        let ETATime = formContext.getAttribute(timefield);
        if (ETATime == null) { return };
        let fieldVal = formContext.getAttribute(timefield).getValue();
        const positiveIntegersRegex = new RegExp(/^0\d{3}$|^[1-9]\d{3}$/);
        const timeRegex = new RegExp(/^(0\d|1\d|2[0-3])[0-5]\d$/);
        if (!positiveIntegersRegex.test(fieldVal)) {
            Xrm.Navigation.openAlertDialog("Please enter valid time format hhmm");
            executionContext.getFormContext().getAttribute(timefield).setValue(null);
            return;
        }

        if (positiveIntegersRegex.test(fieldVal) && !timeRegex.test(fieldVal)) {
            Xrm.Navigation.openAlertDialog("Please enter ATA Time within a valid range between 0000 and 2359");
            executionContext.getFormContext().getAttribute(timefield).setValue(null);
            return;
        }
        ////Jarvis.passout.populateDateTime(formContext, timefield, "jarvis_atadate", "jarvis_ata");
        // let etaTime = formContext.getAttribute("jarvis_etadate")?.getValue();
        // if (etaTime == null)
        // {
        //     ataDate=formContext.getAttribute("jarvis_atadate")?.getValue();
        //     formContext.getAttribute("jarvis_etatime").setValue(fieldVal);
        //     formContext.getAttribute("jarvis_etadate").setValue(ataDate);
        //     formContext.getAttribute("jarvis_etatime").fireOnChange();
        // }

    },
    OnATADateChange: function (executionContext) {
        "use strict";
        let formContext = executionContext.getFormContext();
        let aTADate = formContext.getAttribute("jarvis_atadate");
        var aTADateFieldControl = formContext.getControl("jarvis_atadate");
        var currentDateTime = new Date();
        //ATA date cannot be future date logic
        if (aTADate.getValue() !== null && aTADate.getValue() > currentDateTime) {
            //aTADateFieldControl.setNotification("ATA cannot be in the future.", "ERR_ATAFutureDate");
        }
        else {
            // aTADateFieldControl.clearNotification("ERR_ATAFutureDate");
        }
        Jarvis.passout.populateDateTime(formContext, "jarvis_atatime", "jarvis_atadate", "jarvis_ata");

    },
    OnETCtimeChange: function (executionContext) {
        "use strict";
        let formContext = executionContext.getFormContext();
        var timefield = "jarvis_etctime";
        let ETATime = formContext.getAttribute(timefield);
        if (ETATime == null) { return };
        let fieldVal = formContext.getAttribute(timefield).getValue();
        const positiveIntegersRegex = new RegExp(/^0\d{3}$|^[1-9]\d{3}$/);
        const timeRegex = new RegExp(/^(0\d|1\d|2[0-3])[0-5]\d$/);
        if (!positiveIntegersRegex.test(fieldVal)) {
            Xrm.Navigation.openAlertDialog("Please enter valid time format hhmm");
            executionContext.getFormContext().getAttribute(timefield).setValue(null);
            return;
        }
        if (positiveIntegersRegex.test(fieldVal) && !timeRegex.test(fieldVal)) {
            Xrm.Navigation.openAlertDialog("Please enter ETC Time within a valid range between 0000 and 2359");
            executionContext.getFormContext().getAttribute(timefield).setValue(null);
            return;
        }
        ////Jarvis.passout.populateDateTime(formContext, timefield, "jarvis_etcdate", "jarvis_etc");

    },
    OnETCDateChange: function (executionContext) {
        "use strict";
        let formContext = executionContext.getFormContext();
        Jarvis.passout.populateDateTime(formContext, "jarvis_etctime", "jarvis_etcdate", "jarvis_etc");

    },
    OnATCtimeChange: function (executionContext) {
        "use strict";
        let formContext = executionContext.getFormContext();
        var timefield = "jarvis_atctime";
        let ETATime = formContext.getAttribute(timefield);
        if (ETATime == null) { return };
        let fieldVal = formContext.getAttribute(timefield).getValue();
        const positiveIntegersRegex = new RegExp(/^0\d{3}$|^[1-9]\d{3}$/);
        const timeRegex = new RegExp(/^(0\d|1\d|2[0-3])[0-5]\d$/);
        if (!positiveIntegersRegex.test(fieldVal)) {
            Xrm.Navigation.openAlertDialog("Please enter valid time format hhmm");
            executionContext.getFormContext().getAttribute(timefield).setValue(null);
            return;
        }
        if (positiveIntegersRegex.test(fieldVal) && !timeRegex.test(fieldVal)) {
            Xrm.Navigation.openAlertDialog("Please enter ATC Time within a valid range between 0000 and 2359");
            executionContext.getFormContext().getAttribute(timefield).setValue(null);
            return;
        }
        ////Jarvis.passout.populateDateTime(formContext, timefield, "jarvis_atcdate", "jarvis_atc");
        // let etcTime = formContext.getAttribute("jarvis_etctime")?.getValue();
        // if (etcTime == null)
        // {
        //     atcDate=formContext.getAttribute("jarvis_atcdate")?.getValue();
        //     formContext.getAttribute("jarvis_etctime").setValue(fieldVal);
        //     formContext.getAttribute("jarvis_etcdate").setValue(atcDate);
        //     formContext.getAttribute("jarvis_etctime").fireOnChange();
        // 

    },
    OnATCDateChange: function (executionContext) {
        "use strict";
        let formContext = executionContext.getFormContext();
        let aTCDate = formContext.getAttribute("jarvis_atcdate");
        var aTCDateFieldControl = formContext.getControl("jarvis_atcdate");
        var currentDateTime = new Date();
        //ATC date cannot be future date logic
        if (aTCDate.getValue() !== null && aTCDate.getValue() > currentDateTime) {
            //formContext.ui.setFormNotification("ATC cannot be in the future.", "ERROR", "ERR_ATCFutureDate");
            // Jarvis.passout.dateIsFuture = true;
            //aTCDateFieldControl.setNotification("ATC cannot be in the future.", "ERR_ATCFutureDate");
        }
        else {
            // aTCDateFieldControl.clearNotification("ERR_ATCFutureDate");
            //formContext.ui.clearFormNotification("ERR_ATCFutureDate");
            Jarvis.passout.dateIsFuture = false;
        }

        Jarvis.passout.populateDateTime(formContext, "jarvis_atctime", "jarvis_atcdate", "jarvis_atc");

    },
    populateDateTime: function (formContext, timeField, dateField, dateTimefield) {
        "use strict";
        let ETAfield = formContext.getAttribute(dateField);
        let timefield = formContext.getAttribute(timeField);
        let ETADateTimefield = formContext.getAttribute(dateTimefield);
        if (ETAfield != null && ETADateTimefield != null && timefield != null) {
            let fieldVal = formContext.getAttribute(timeField).getValue();
            let ETADate = formContext.getAttribute(dateField).getValue();
            if (ETADate != null && fieldVal != null) {
                let hh = '00';
                let mm = '00';
                if (fieldVal != null) {
                    hh = fieldVal.slice(0, 2);
                    mm = fieldVal.slice(2, 4);
                }
                var day = ETADate.getDate();
                var month = ETADate.getMonth();
                var year = ETADate.getFullYear();
                var date = new Date(year, month, day, hh, mm, 0, 0);
                //
                //var date =  formContext.getAttribute("jarvis_eta").getValue() 
                var userSettings = Xrm.Utility.getGlobalContext().userSettings;
                let offsetUserSetting = userSettings.getTimeZoneOffsetMinutes();
                if (offsetUserSetting) {
                    var now_utc = Date.UTC(date.getUTCFullYear(), date.getUTCMonth(), date.getUTCDate(),
                        date.getUTCHours(), date.getUTCMinutes(), date.getUTCSeconds());
                    var dt = new Date(now_utc);
                    var utcDate = new Date(dt.toLocaleString('en-US', { timeZone: "UTC" }));
                    var timezonediff = dt.getTime() - utcDate.getTime();
                    var offsetmyTimeZone = timezonediff / 60000;



                    if (offsetmyTimeZone > offsetUserSetting) {
                        var offsetToCalculate = Math.abs(offsetmyTimeZone) - Math.abs(offsetUserSetting);
                        dt.setTime(dt.getTime() + (Math.abs(offsetToCalculate) * 60000));
                    }
                    else {
                        var offsetToCalculate = Math.abs(offsetUserSetting) - Math.abs(offsetmyTimeZone);
                        dt.setTime(dt.getTime() - (Math.abs(offsetToCalculate) * 60000));
                    }
                    formContext.getAttribute(dateTimefield).setValue(dt);
                }
                //
                else {
                    formContext.getAttribute(dateTimefield).setValue(date);
                }

            }
        }
    },

    SetDateFields: function (executionContext) {
        "use strict";
        let passoutCntrls = ["jarvis_etadate", "jarvis_atadate", "jarvis_etcdate", "jarvis_atcdate"];
        let formContext = executionContext.getFormContext();
        let source = formContext.getAttribute("jarvis_source_")?.getValue();

        passoutCntrls.forEach(SourceField => {
            let field = formContext.getAttribute(SourceField);
            if (field) {
                let fieldVal = formContext.getAttribute(SourceField).getValue();
                if (fieldVal == null) {
                    let jarvisCase = formContext.getAttribute("jarvis_incident")?.getValue();
                    var timezoneCode = 105;
                    if (jarvisCase !== null && jarvisCase !== undefined) {
                        let jarvisCaseId = jarvisCase[0].id.slice(1, -1);
                        Xrm.WebApi.retrieveRecord("incident", jarvisCaseId, "?$select=jarvis_timezone").then(
                            function success(result) {
                                if (result !== null) {
                                    timezoneCode = result["jarvis_timezone"];

                                    Xrm.WebApi.retrieveMultipleRecords("timezonedefinition", "?$select=standardname,bias&$filter=timezonecode eq " + timezoneCode).then(
                                        function success(results) {
                                            if (results.entities.length > 0) {
                                                var result = results.entities[0];
                                                var timezoneBias = result["bias"];
                                                if (timezoneBias != null) {
                                                    var dt = new Date();
                                                    var utcDate = new Date(dt.toLocaleString('en-US', { timeZone: "UTC" }));
                                                    var localDate = Jarvis.passout.getLocalDateByTimezoneBias(utcDate, timezoneBias);
                                                    formContext.getAttribute(SourceField).setValue(localDate);
                                                }
                                            }
                                        }
                                    );
                                }
                            }
                        );
                    }
                }
            }
        });
    },

    currencyExchange: function (baseCurrencyId, targetCurrencyId) {
        "use strict";
        if (baseCurrencyId == targetCurrencyId)
            return 1
        else {
            Xrm.WebApi.retrieveMultipleRecords("jarvis_exchangerate", "?$select=jarvis_value&$filter=(_jarvis_basecurrency_value eq " + baseCurrencyId + " and _jarvis_countercurrency_value eq " + targetCurrencyId + ")").then(
                function success(result) {
                    if (result.entities.length > 0) {
                        for (let row of result.entities) {
                            if (row["jarvis_value"]) {
                                var conversionrate = row["jarvis_value"];
                                return conversionrate;
                            }
                            else
                                return 0;
                        }
                    }
                    else
                        return 0;
                },
                function (error) {
                    console.log(error.message);
                }
            );
        }
    },
    OnCurrencyUpdate: function (executionContext) {
        "use strict";
        let formContext = executionContext.getFormContext();
        let jarvisCase = formContext.getAttribute("jarvis_incident")?.getValue();
        let jarvisPassOutId = formContext.data.entity.getId().slice(1, -1)
        let jarvisCaseId = jarvisCase[0].id.slice(1, -1);
        let gopLimitOut = formContext.getAttribute("jarvis_goplimitout")?.getValue();
        let passoutcurrency = null;
        let passoutcurrencyvalue = formContext.getAttribute("transactioncurrencyid")?.getValue();
        if (passoutcurrencyvalue)
            passoutcurrency = passoutcurrencyvalue[0].id;
        if (passoutcurrency) {
            passoutcurrency = passoutcurrency.replace('{', '').replace('}', '');
        }
        formContext.ui.clearFormNotification("ERR_PassOutAmountValidation");
        formContext.ui.clearFormNotification("WAR_PassOutCurrencyValidation");
        let passoutSent = formContext.getAttribute("jarvis_passoutaccepted")?.getValue();
        if (jarvisPassOutId == "" || jarvisPassOutId == null) {
            if (Jarvis.passout.caseAvailableCurrency && Jarvis.passout.caseAvailableAmount) {
                if (passoutcurrency && Jarvis.passout.caseAvailableCurrency) {
                    let conversionrate = 0;
                    if (passoutcurrency.toUpperCase() == Jarvis.passout.caseAvailableCurrency.toUpperCase()) {
                        var totalPassoutAmount = gopLimitOut * 1;
                        if (totalPassoutAmount > Jarvis.passout.caseAvailableAmount) {
                            Jarvis.passout.notEnoughGOPAmount = true;
                            formContext.ui.setFormNotification("Not enough GOP available Amount", "ERROR", "ERR_PassOutAmountValidation");//statuscode
                            //executionContext.getEventArgs().preventDefault();
                        }
                        else
                            Jarvis.passout.notEnoughGOPAmount = false;
                    }
                    else {
                        if (!passoutSent) {
                            Xrm.WebApi.retrieveMultipleRecords("jarvis_exchangerate", "?$select=jarvis_value&$filter=(_jarvis_basecurrency_value eq " + passoutcurrency + " and _jarvis_countercurrency_value eq " + Jarvis.passout.caseAvailableCurrency + ")").then(
                                function success(result) {
                                    if (result.entities.length > 0) {
                                        for (let row of result.entities) {
                                            if (row["jarvis_value"]) {
                                                var conversionrate = row["jarvis_value"];
                                                var totalPassoutAmount = gopLimitOut * conversionrate;
                                                if (totalPassoutAmount > Jarvis.passout.caseAvailableAmount) {
                                                    Jarvis.passout.notEnoughGOPAmount = true;
                                                    formContext.ui.setFormNotification("Not enough GOP available Amount", "ERROR", "ERR_PassOutAmountValidation");//statuscode
                                                    // executionContext.getEventArgs().preventDefault();
                                                }
                                                else
                                                    Jarvis.passout.notEnoughGOPAmount = false;
                                            }
                                            else {
                                                formContext.ui.setFormNotification("Conversion rate not available for Passout Currency and Case Available Amount Curreny ", "WARNING", "WAR_PassOutCurrencyValidation");
                                            }
                                        }
                                    }
                                    else {
                                        formContext.ui.setFormNotification("Conversion rate not available for Passout Currency and Case Available Amount Curreny ", "WARNING", "WAR_PassOutCurrencyValidation");
                                    }
                                },
                                function (error) {
                                    console.log(error.message);
                                }
                            );
                        }
                    }
                }
            }
            else {
                Xrm.WebApi.retrieveRecord("incident", jarvisCaseId, "?$select=jarvis_totalgoplimitoutapproved,_jarvis_totalgoplimitoutapprovedcurrency_value,jarvis_totalpassoutamount,_jarvis_totalpassoutcurrency_value,jarvis_restgoplimitout,_jarvis_totalrestcurrencyout_value").then(
                    function success(result) {
                        let jarvis_totalgoplimitoutapproved = result["jarvis_totalgoplimitoutapproved"]; // Decimal
                        if (result["jarvis_restgoplimitout"])
                            Jarvis.passout.caseAvailableAmount = result["jarvis_restgoplimitout"];
                        else
                            Jarvis.passout.caseAvailableAmount = 0;

                        // let jarvis_totalrestcurrencyout = null
                        if (result["_jarvis_totalrestcurrencyout_value"])
                            Jarvis.passout.caseAvailableCurrency = result["_jarvis_totalrestcurrencyout_value"];

                        else
                            Jarvis.passout.caseAvailableCurrency = passoutcurrency;


                        // Get exchange rate of passout currency and GOp avialble amount
                        if (passoutcurrency && Jarvis.passout.caseAvailableCurrency) {
                            // let conversionrate = Jarvis.passout.currencyExchange(passoutcurrency,jarvis_totalrestcurrencyout);
                            let conversionrate = 0;
                            if (passoutcurrency.toUpperCase() == Jarvis.passout.caseAvailableCurrency.toUpperCase()) {
                                var totalPassoutAmount = gopLimitOut * 1;
                                if (totalPassoutAmount > Jarvis.passout.caseAvailableAmount)
                                    Jarvis.passout.notEnoughGOPAmount = true;
                                else
                                    Jarvis.passout.notEnoughGOPAmount = false;
                            }
                            else {
                                Xrm.WebApi.retrieveMultipleRecords("jarvis_exchangerate", "?$select=jarvis_value&$filter=(_jarvis_basecurrency_value eq " + passoutcurrency + " and _jarvis_countercurrency_value eq " + Jarvis.passout.caseAvailableCurrency + ")").then(
                                    function success(result) {
                                        if (result.entities.length > 0) {
                                            for (let row of result.entities) {
                                                if (row["jarvis_value"]) {
                                                    var conversionrate = row["jarvis_value"];
                                                    var totalPassoutAmount = gopLimitOut * conversionrate;
                                                    if (totalPassoutAmount > Jarvis.passout.caseAvailableAmount)
                                                        Jarvis.passout.notEnoughGOPAmount = true;
                                                    else
                                                        Jarvis.passout.notEnoughGOPAmount = false;
                                                }
                                                else {
                                                    formContext.ui.setFormNotification("Conversion rate not available for Passout Currency and Case Available Amount Curreny ", "WARNING", "WAR_PassOutCurrencyValidation");
                                                }
                                            }
                                        }
                                        else {
                                            formContext.ui.setFormNotification("Conversion rate not available for Passout Currency and Case Available Amount Curreny ", "WARNING", "WAR_PassOutCurrencyValidation");
                                        }
                                    },
                                    function (error) {
                                        console.log(error.message);
                                    }
                                );
                            }
                        }

                    },
                    function (error) {
                        console.log(error.message);
                    }
                );
            }
        }
    },
    SetPaymentType: function (executionContext) {
        "use strict";
        let formContext = executionContext.getFormContext();
        formContext.getAttribute("jarvis_paymenttype").setRequiredLevel("required");
        let formType = formContext.ui.getFormType();
        let jarvisCase = formContext.getAttribute("jarvis_incident")?.getValue();

        if (formType == 1 && jarvisCase !== null) {
            let jarvisCaseId = jarvisCase[0].id.slice(1, -1);
            var fetchXml = `<fetch version="1.0" output-format="xml-platform" mapping="logical" distinct="false">
            <entity name="jarvis_gop">
              <attribute name="jarvis_gopid" />
              <attribute name="jarvis_name" />
              <attribute name="jarvis_paymenttype" />
              <attribute name='modifiedon' />
              <order attribute='modifiedon' descending='true' />
              <filter type="and">
                <condition attribute="jarvis_gopapproval" operator="eq" value="334030001" />
                <condition attribute="jarvis_incident" operator="eq" uitype="incident" value="{`+ jarvisCaseId + `}" />
                <condition attribute='jarvis_relatedgop' operator='null' />
                <condition attribute="statecode" operator="eq" value="0" />
              </filter>
            </entity>
          </fetch>`;


            fetchXml = "?fetchXml=" + encodeURIComponent(fetchXml);
            Xrm.WebApi.retrieveMultipleRecords("jarvis_gop", fetchXml).then(
                function success(result) {
                    if (result.entities.length > 0) {
                        if (result.entities[0]["jarvis_paymenttype"]) {
                            var paymentType = result.entities[0]["jarvis_paymenttype"];
                            formContext.getAttribute("jarvis_paymenttype").setValue(paymentType);
                        }
                    }
                },
                function (error) {
                    console.log(error.message);
                    // handle error conditions
                }
            );
        }


    },

    OnSaveDateChange: function (executionContext) {
        "use strict";
        let formContext = executionContext.getFormContext();
        if (Jarvis.passout.dateIsFuture) {
            executionContext.getEventArgs().preventDefault();

        }
    },

    onLoad: function (executionContext) {
        "use strict";
        Jarvis.passout.ETAAppointment(executionContext);
        Jarvis.passout.LockRecordOnLoad(executionContext);
        Jarvis.passout.SetDateFields(executionContext);
        Jarvis.passout.SetPaymentType(executionContext);
    },

    onPassOutCurrencyChange: function (executionContext) {
        const formContext = executionContext.getFormContext();
        ////const repairingDealer = formContext.getAttribute("jarvis_repairingdealer").getValue();
        const incident = formContext.getAttribute("jarvis_incident")?.getValue();
        if (incident != null && incident != undefined) {
            let incidentId = incident[0].id;
            incidentId = incidentId.replace('{', '').replace('}', '');
            const passOutCurrency = formContext.getAttribute("transactioncurrencyid")?.getValue();
            if (passOutCurrency != null && passOutCurrency != undefined) {
                let passOutCurrencyId = passOutCurrency[0].id;
                passOutCurrencyId = passOutCurrencyId.replace('{', '').replace('}', '');
                Xrm.WebApi.retrieveRecord("incident", incidentId, "?$select=jarvis_restgoplimitout,_jarvis_totalrestcurrencyout_value").then(
                    function success(result) {
                        const jarvis_restgoplimitout = result["jarvis_restgoplimitout"]; // Decimal
                        const jarvis_totalrestcurrencyout = result["_jarvis_totalrestcurrencyout_value"]; // Lookup 
                        if (jarvis_restgoplimitout != null && jarvis_restgoplimitout != 0) {
                            if (jarvis_totalrestcurrencyout != null && jarvis_totalrestcurrencyout != undefined) {
                                Jarvis.passout.exchangeRate(formContext, jarvis_totalrestcurrencyout, passOutCurrencyId, jarvis_restgoplimitout, "jarvis_casegopoutavailableamount");
                            }
                            else {
                                formContext.getAttribute("jarvis_casegopoutavailableamount").setValue(null);
                                //// formContext.ui.setFormNotification("Case Available GOP OUT Amount Currency is not present at Case level.", "WARNING", "WAR_AvailableGOPOUTAmountCurrencyValidation");
                            }
                        }
                        else {
                            formContext.getAttribute("jarvis_casegopoutavailableamount").setValue(jarvis_restgoplimitout);
                        }
                    },
                    function (error) {
                        //// Handle the error if it required.
                    }
                );
            }
            else {
                formContext.getAttribute("jarvis_casegopoutavailableamount")?.setValue(null);
            }

        }
    },

    exchangeRate: function (formContext, baseCurrency, counterCurrency, jarvis_restgoplimitout, field) {
        let result = Xrm.WebApi.retrieveMultipleRecords("jarvis_exchangerate", "?$select=jarvis_value&$filter=(_jarvis_basecurrency_value eq " + baseCurrency + " and _jarvis_countercurrency_value eq " + counterCurrency + ")").then(
            function success(result) {
                if (result.entities.length > 0) {
                    for (let row of result.entities) {
                        if (row["jarvis_value"]) {
                            jarvis_restgoplimitout = jarvis_restgoplimitout * row["jarvis_value"];
                            formContext.getAttribute(field)?.setValue(jarvis_restgoplimitout);
                        }
                        else {
                            formContext.getAttribute(field)?.setValue(null);
                            //// formContext.ui.setFormNotification("Conversion rate not available for GOP Limit In/Out Currency and Case Available Amount Currency.", "WARNING", "WAR_GOPOutCurrencyValidation");
                        }
                    }
                }
                else {
                    formContext.getAttribute(field)?.setValue(null);
                    //// formContext.ui.setFormNotification("Conversion rate not available for GOP Limit In/Out Currency and Case Available Amount Currency.", "WARNING", "WAR_GOPOutCurrencyValidation");
                }
            },
            function (error) {
                //// Handle the error if it required.
            }
        );
    },

    getLocalDateByTimezoneBias: function getLocalDateByTimezoneBias(utcDate, timezoneBias) {
        // Create a Date object from the UTC date string
        // var date = new Date(utcDate);

        // Convert the bias from minutes to milliseconds
        var biasInMilliseconds = timezoneBias * 60000;

        // Adjust the date by the bias to get the local date
        var localDate = new Date(utcDate.getTime() - biasInMilliseconds);

        return localDate;
    }

}