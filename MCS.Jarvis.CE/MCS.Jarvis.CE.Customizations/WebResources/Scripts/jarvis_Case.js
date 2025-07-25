var Jarvis = Jarvis || {};

Jarvis.Case = {
    isPreventRequired: false,
    isBlacklistedCustomer: false,
    populateCountry: function (executionContext) {
        let formContext = executionContext.getFormContext();
        let countryRegionName = formContext.getAttribute("jarvis_countryregion")?.getValue();

        if (!countryRegionName) {
            formContext.getAttribute("jarvis_country").setValue(null);
            return;
        }

        Xrm.WebApi.retrieveMultipleRecords("jarvis_country", "?$select=jarvis_countryid,jarvis_name,transactioncurrencyid&$filter=jarvis_name eq '" + countryRegionName + "'").then(
            function success(results) {
                console.log(results);
                if (results.entities.length > 0) {
                    let result = results.entities[0];
                    // Columns
                    let jarvis_countryid = result["jarvis_countryid"]; // Guid
                    let jarvis_name = result["jarvis_name"]; // Text

                    let countryData = [
                        {
                            id: jarvis_countryid,
                            name: jarvis_name,
                            entityType: "jarvis_country"
                        }
                    ];
                    formContext.getAttribute("jarvis_country").setValue(countryData);
                    let DatasetAttribute = "_transactioncurrencyid_value";
                    Xrm.WebApi.retrieveRecord("jarvis_country", jarvis_countryid).then(
                        function success(result) {
                            if (result[DatasetAttribute]) {

                                let lookup = new Array();
                                lookup[0] = new Object();
                                lookup[0].id = result[DatasetAttribute];
                                lookup[0].name = result[DatasetAttribute + "@OData.Community.Display.V1.FormattedValue"];
                                lookup[0].entityType = result[DatasetAttribute + "@Microsoft.Dynamics.CRM.lookuplogicalname"];
                                formContext.getAttribute("jarvis_totalrestcurrencyout").setValue(lookup);
                                formContext.getAttribute("jarvis_totalgoplimitoutapprovedcurrency").setValue(lookup);
                                formContext.getAttribute("jarvis_totalcurrencyout").setValue(lookup);

                            }

                        }
                    );
                } else {
                    alert("Please enter the correct value for Country");
                }
            },
            function (error) {
                console.log(error.message);
            }
        );
    },

    populateGOPCurrency: function (executionContext) {
        let formContext = executionContext.getFormContext();
        let countryRegionName = formContext.getAttribute("jarvis_country")?.getValue();

        if (!countryRegionName) {
            return;
        }

        Xrm.WebApi.retrieveMultipleRecords("jarvis_country", "?$select=jarvis_countryid,jarvis_name,transactioncurrencyid&$filter=jarvis_countryid eq '" + countryRegionName + "'").then(
            function success(results) {
                console.log(results);
                if (results.entities.length > 0) {
                    let result = results.entities[0];
                    // Columns
                    let jarvis_countryid = result["jarvis_countryid"]; // Guid
                    let jarvis_name = result["jarvis_name"]; // Text
                    let gopCurrency = result["transactioncurrencyid"];


                } else {
                    alert("Please enter the correct value for Country");
                }
            },
            function (error) {
                console.log(error.message);
            }
        );
    },

    SetLocalStorage: async function (executionContext) {
        localStorage.removeItem("RepaingDealer");
        localStorage.removeItem("HomeDealer");
        localStorage.removeItem("RepaingDealerName");
        localStorage.removeItem("HomeDealerName");
        let formContext = executionContext.getFormContext();
        if (formContext.ui.getFormType() !== 1) {
            let recordId = formContext.data.entity.getId().replace('{', '').replace('}', '');
            formContext = executionContext.getFormContext();
            let repairingDealer = null;
            let homeDealer = null;
            if (formContext.getControl("jarvis_homedealer") !== null && formContext.getAttribute("jarvis_homedealer").getValue() !== null) {
                homeDealer = formContext.getAttribute("jarvis_homedealer").getValue();
            }
            let passedouts = await Xrm.WebApi.retrieveMultipleRecords("jarvis_passout", "?$filter=_jarvis_incident_value eq " + recordId + " &$orderby= createdon asc");
            if (passedouts !== null && passedouts.entities !== null) {
                repairingDealer = passedouts.entities[0];
            }
            if (repairingDealer !== null && repairingDealer !== undefined) {
                localStorage.setItem("RepaingDealer", "{" + repairingDealer.jarvis_passoutid + "}");
                localStorage.setItem("RepaingDealerName", repairingDealer.jarvis_name);
            }
            else {
                localStorage.setItem("RepaingDealer", "");
                localStorage.setItem("RepaingDealerName", "");
            }

            if (homeDealer !== null) {
                localStorage.setItem("HomeDealer", homeDealer[0].id);
                localStorage.setItem("HomeDealerName", homeDealer[0].name);
            }
            else {
                localStorage.setItem("HomeDealer", null);
            }
            if (formContext.getControl("jarvis_incidentnature") !== null && formContext.getAttribute("jarvis_incidentnature").getValue() == "null") {
                formContext.getAttribute("jarvis_incidentnature").setValue("[]");
            }
        }

    },
    OnLoadDealerBlacklistDisplay: function (executionContext) {
        let formContext = executionContext.getFormContext();
        let homeDealer = null;
        let customer = null;
        let isBlackListed = false;
        var responsableUnitid;
        let dealerName = null;
        let address1_city = null;
        let arrFields = ["jarvis_homedealer", "customerid"];
        arrFields.forEach(DisplayMessage);

        async function DisplayMessage(item) {
            if (formContext.getControl(item) !== null && formContext.getAttribute(item).getValue() !== null) {
                homeDealer = formContext.getAttribute(item).getValue();

                let result = await Xrm.WebApi.retrieveRecord("account", homeDealer[0].id, "?$select=name,address1_city,jarvis_blacklisted,accountnumber,jarvis_responsableunitid").then(
                    function success(result) {
                        var accountid = result["accountid"];
                        dealerName = result["name"];
                        address1_city = result["address1_city"] ? " " + result["address1_city"] : " ";
                        isBlackListed = result["jarvis_blacklisted"];
                        responsableUnitid = result["jarvis_responsableunitid"] ? result["jarvis_responsableunitid"] + " " : " ";

                        if (isBlackListed && item === "jarvis_homedealer") {
                            formContext.ui.setFormNotification("Home Dealer " + responsableUnitid + dealerName + address1_city + " is blacklisted by VAS", "ERROR", "ERR_AccountBlacklisted");

                        }
                        if (isBlackListed && item === "customerid") {
                            formContext.ui.setFormNotification("Customer " + dealerName + " is blacklisted", "ERROR", "ERR_CustomerBlacklisted");

                        }


                    }
                );
            }
            else {
                formContext.ui.clearFormNotification("ERR_CustomerBlacklisted");
                formContext.ui.clearFormNotification("ERR_AccountBlacklisted");
            }
        }
    },



    OnSaveCustomerBlacklistDisplay: function (executionContext) {
        let formContext = executionContext.getFormContext();
        let customer = null;

        /* if (formContext.getControl("customerid") !== null && formContext.getAttribute("customerid").getValue() !== null && formContext.ui.getFormType() === 1) {
             customer = formContext.getAttribute("customerid").getValue();
             if (!Jarvis.Case.isPreventRequired) {
                 executionContext.getEventArgs().preventDefault();
             }
             Xrm.WebApi.retrieveRecord("account", customer[0].id, "?$select=name,jarvis_blacklisted").then(
                 function success(result) {
                     var accountid = result["accountid"];
                     dealerName = result["name"];
                     Jarvis.Case.isBlacklistedCustomer = result["jarvis_blacklisted"];
                     if (Jarvis.Case.isBlacklistedCustomer) {
                         var alertStrings = { confirmButtonLabel: "Yes", text: "You cannot save the case because the customer is blacklisted, please inform customer that VAS cannot assist.", title: "Customer Blacklisted" };
                         var alertOptions = { height: 120, width: 260 };
                         Xrm.Navigation.openAlertDialog(alertStrings, alertOptions).then(
                             function (success) {
                             },
                             function (error) {
                             }
                         );
 
                     }
                     else {
                         Jarvis.Case.isPreventRequired = true;
                         formContext.data.save();
                     }
                 }
                  
             );
 
         }*/
    }
}
