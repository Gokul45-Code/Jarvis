var Jarvis = Jarvis || {};

Jarvis.BusinessPartnerType = {
    Customer: 334030000,
    Dealer: 334030001,
    Partner: 334030002,
    Market: 334030003
};
Jarvis.BusinessPartner = {
    businessTypeOnChange: function (executionContext) {
        "use strict";
        let formContext = executionContext.getFormContext();

        let fieldArr = [];
        let businessPatnerType = formContext.getAttribute("jarvis_accounttype")?.getValue();

        if (!businessPatnerType) {
            return;
        }

        switch (businessPatnerType) {
            case Jarvis.BusinessPartnerType.Customer:
                fieldArr = ["jarvis_source", "accountnumber", "name", "parentaccountid", "description",
                    "jarvis_externallanguage", "telephone1", "emailaddress1", "fax", "websiteurl",
                    "address1_addresstypecode", "address1_name", "address1_line1", "address1_line2", "address1_line3", "address1_postalcode", "address1_city", "address1_stateorprovince", "jarvis_address1_country", "address1_latitude", "address1_longitude"];
                formContext.ui.setFormNotification("Business Partner of type Customer and Dealer cannot be created", "ERROR", "ERR_BusinesspartnerType");
                //executionContext.getEventArgs().preventDefault();
                break;
            case Jarvis.BusinessPartnerType.Dealer:
                fieldArr = ["jarvis_source", "accountnumber", "name", "parentaccountid",
                    "jarvis_externallanguage", "telephone1", "emailaddress1", "fax", "websiteurl",
                    "address1_addresstypecode", "address1_name", "address1_line1", "address1_line2", "address1_line3", "address1_postalcode", "address1_city", "address1_stateorprovince", "jarvis_address1_country", "address1_latitude", "address1_longitude",
                    "jarvis_country", "jarvis_vatid",
                    "address2_addresstypecode"]
                formContext.ui.setFormNotification("Business Partner of type Customer and Dealer cannot be created", "ERROR", "ERR_BusinesspartnerType");
                //executionContext.getEventArgs().preventDefault();
                break;
            case Jarvis.BusinessPartnerType.Partner:
                fieldArr = ["address2_addresstypecode"]
                formContext.ui.clearFormNotification("ERR_BusinesspartnerType");
                break;
            case Jarvis.BusinessPartnerType.Market:
                fieldArr = ["address2_addresstypecode"]
                formContext.ui.clearFormNotification("ERR_BusinesspartnerType");
                break;
        }

        if (fieldArr.length > 0) {
            Jarvis.BusinessPartner.lockFields(formContext, fieldArr, true);
        }
    },
    HideBusinessPartnerType: function (executionContext) {
        "use strict";
        let formContext = executionContext.getFormContext();
        let formType = formContext.ui.getFormType();
        let businessPatnerType = formContext.getControl("jarvis_accounttype");
        formContext.ui.clearFormNotification("WAR_BusinesspartnerType");
        if (formType === 1 && businessPatnerType !== null) {
            formContext.ui.setFormNotification("Business Partner of type Customer and Dealer cannot be created", "WARNING", "WAR_BusinesspartnerType");
            businessPatnerType.removeOption(334030000);
            businessPatnerType.removeOption(334030001);
        }
        else {
            businessPatnerType.setDisabled(true);
        }
    },

    lockFields: function (formContext, fieldArr, readOnly) {
        "use strict";

        if (fieldArr.length === 0) {
            return;
        }

        for (let row of fieldArr) {
            let fieldControl = formContext.getControl(row);
            if (fieldControl) {
                fieldControl.setDisabled(readOnly);
            }
        }
    },

    validateTime: function (executionContext) {
        "use strict";
        let formContext = executionContext.getFormContext();
        let from = formContext.getAttribute("jarvis_temporarydealerinformationvalidfrom").getValue();
        let to = formContext.getAttribute("jarvis_temporarydealerinformationvaliduntil").getValue();
        if (to != null && from != null) {
            if (from > to) {
                let msg = "Temporary Dealer Information Valid until needs to be after valid from";
                let myControl = formContext.getControl("jarvis_temporarydealerinformationvaliduntil");
                myControl.addNotification({
                    messages: [msg],
                    notificationLevel: 'ERROR'

                });

            }
            else {
                formContext.getControl("jarvis_temporarydealerinformationvaliduntil").clearNotification();
            }
        }
        else {
            formContext.getControl("jarvis_temporarydealerinformationvaliduntil").clearNotification();
        }

    },

    openCase: function (selectedItemId, entityTypeName, selectedControl) {
        if (selectedControl.entityReference.id != null && selectedControl.entityReference.id != "") {
            var caseId = selectedControl.entityReference.id.replace('{', '').replace('}', '');


            Xrm.WebApi.retrieveRecord("account", caseId).then(
                function success(result) {
                    var entityFormOptions = {};
                    entityFormOptions["entityName"] = "incident";

                    // Set default values for the Contact form
                    var formParameters = {};
                    formParameters["jarvis_callerphone"] = result["telephone1"];
                    //formParameters["_jarvis_callercompany_value@Microsoft.Dynamics.CRM.lookuplogicalname"] = "account";
                    //formParameters["_jarvis_callercompany_value@OData.Community.Display.V1.FormattedValue"] = result["name"];
                    //formParameters["_jarvis_callercompany_value"] = caseId;
                    formParameters["jarvis_callercompanytype"] = "account";
                    formParameters["jarvis_callercompanyname"] = result["name"];
                    formParameters["jarvis_callercompany"] = caseId;
                    if (result["jarvis_accounttype"] == 334030000) {
                        formParameters["jarvis_callerrole@OData.Community.Display.V1.FormattedValue"] = "Customer";
                        formParameters["jarvis_callerrole"] = result["jarvis_accounttype"];
                    }
                    if (result["_jarvis_language_value"] != null) {
                        formParameters["jarvis_callerlanguage"] = result["_jarvis_language_value"];
                        formParameters["jarvis_callerlanguagename"] = result["_jarvis_language_value@OData.Community.Display.V1.FormattedValue"];
                        formParameters["jarvis_callerlanguagetype"] = "jarvis_language";
                    }


                    // Open the form.
                    Xrm.Navigation.openForm(entityFormOptions, formParameters).then(
                        function (success) {
                            console.log(success);
                        },
                        function (error) {
                            console.log(error);
                        });
                }
            );
        }
        else {
            var entityFormOptions = {};
            entityFormOptions["entityName"] = "incident";
            var formParameters = {};
            // Open the form.
            Xrm.Navigation.openForm(entityFormOptions, formParameters).then(
                function (success) {
                    console.log(success);
                },
                function (error) {
                    console.log(error);
                });
        }

    },

    saveBusinessPartner: function (executionContext) {
        "use strict";
        let formContext = executionContext.getFormContext();
        let formType = formContext.ui.getFormType();
        if (formType === 1) {
            let fieldArr = [];
            let businessPatnerType = formContext.getAttribute("jarvis_accounttype")?.getValue();

            if (!businessPatnerType) {
                return;
            }

            switch (businessPatnerType) {
                case Jarvis.BusinessPartnerType.Customer:
                    executionContext.getEventArgs().preventDefault();
                    break;
                case Jarvis.BusinessPartnerType.Dealer:
                    executionContext.getEventArgs().preventDefault();
                    break;

            }
        }
    }
}