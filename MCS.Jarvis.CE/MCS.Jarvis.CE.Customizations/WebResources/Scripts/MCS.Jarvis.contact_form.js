var Jarvis = Jarvis || {};


Jarvis.Contact = {


    openCase: function (selectedItemId, entityTypeName, selectedControl) {
        "use strict";
        if (selectedControl.entityReference.id !== null && selectedControl.entityReference.id !== "") {
            var caseId = selectedControl.entityReference.id.replace('{', '').replace('}', '');


            Xrm.WebApi.retrieveRecord("contact", caseId).then(
                function success(result) {
                    var entityFormOptions = {};
                    entityFormOptions["entityName"] = "incident";

                    // Set default values for the Contact form
                    var formParameters = {};
                    if (result["mobilephone"] !== null && result["mobilephone"] !== "") {
                        formParameters["jarvis_callerphone"] = result["mobilephone"];
                    }
                    else if (result["company"] !== null && result["company"] !== "") {
                        formParameters["jarvis_callerphone"] = result["company"];
                    }
                    if (result["_parentcustomerid_value"] !== null && result["_parentcustomerid_value"] !== "") {
                        formParameters["jarvis_callercompanytype"] = "account";
                        formParameters["jarvis_callercompanyname"] = result["_parentcustomerid_value@OData.Community.Display.V1.FormattedValue"];
                        formParameters["jarvis_callercompany"] = result["_parentcustomerid_value"];
                    }
                    if (result["jarvis_parentaccounttype"] === 334030000) {
                        formParameters["jarvis_callerrole@OData.Community.Display.V1.FormattedValue"] = "Customer";
                        formParameters["jarvis_callerrole"] = result["jarvis_parentaccounttype"];
                    }
                    formParameters["jarvis_callernameargus"] = result["fullname"];
                    if (result["_jarvis_language_value"] !== null) {
                        formParameters["jarvis_callerlanguage"] = result["_jarvis_language_value"];
                        formParameters["jarvis_callerlanguagename"] = result["_jarvis_language_value@OData.Community.Display.V1.FormattedValue"];
                        formParameters["jarvis_callerlanguagetype"] = "jarvis_language";
                    }


                    // Open the form.
                    Xrm.Navigation.openForm(entityFormOptions, formParameters).then(
                        function (success) {
                            //console.log(success);
                        },
                        function (error) {
                            //console.log(error);
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
                    //console.log(success);
                },
                function (error) {
                    //console.log(error);
                });
        }

    }
}