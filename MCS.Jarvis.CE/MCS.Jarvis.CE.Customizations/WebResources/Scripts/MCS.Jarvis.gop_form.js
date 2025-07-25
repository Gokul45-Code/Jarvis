var existingGOP = false;
var existingCCGOP = false;
var caseAvailableGOP = 0;
var RDLimitOUT = 0;
var isPreventRequired = false;
var isBlacklistedCustomer = false;
var volvoPayAutomationEnabled = false;
var isStairCasefeeIncluded = false;
var isvolvoPayFeeIncluded = false;
var dealerCountry = null;
//var confirmStrings = { text: "Confirm the send out of the Payment request to customer?", title: "Confirmation Of Payment", confirmButtonLabel: "Yes", cancelButtonLabel: "No" };
var confirmOptions = { height: 150, width: 500 };
function onDealerSelect(executionContext) {
    GopCntrls = {
        Fields: {
            Dealer: "jarvis_dealer",
            RepairingDealer: "jarvis_repairingdealer",
            GopINCurrency: "jarvis_gopincurrency",
            GopOutCurrency: "jarvis_totallimitincurrency",

        }
    }
    // Access the field on the form
    var formContext = executionContext.getFormContext();
    var field = executionContext.getFormContext().getAttribute(GopCntrls.Fields.Dealer);
    var casefield = executionContext.getFormContext().getAttribute("jarvis_incident");
    var parentGop = executionContext.getFormContext().getAttribute("jarvis_parentgop")?.getValue();
    var formType = formContext.ui.getFormType();
    var existingLimitIn = executionContext.getFormContext().getAttribute("jarvis_goplimitin")?.getValue();
    var requestType = executionContext.getFormContext().getAttribute("jarvis_requesttype")?.getValue();
    existingGOP = false;
    if (field != null && casefield != null && formContext.getAttribute("jarvis_incident").getValue() != null) {
        var value = field.getValue();
        if (value) {
            var record_id = field.getValue()[0].id;
            record_id = record_id.replace('{', '').replace('}', '');
            //GetRelatedData(executionContext, record_id, "account", "Lookup", "_jarvis_currency_value", "jarvis_gopincurrency");
            // GetRelatedData(executionContext, record_id, "account", "Lookup", "_jarvis_currency_value", "jarvis_totallimitincurrency");
            let paymentType = executionContext.getFormContext().getAttribute("jarvis_paymenttype")?.getValue();
            if (paymentType !== 334030002) // Credit Card
            {
                GetRelatedData(executionContext, record_id, "account", "Lookup", "_jarvis_currency_value", "jarvis_gopincurrency");
                GetRelatedData(executionContext, record_id, "account", "Lookup", "_jarvis_currency_value", "jarvis_totallimitincurrency");
            }
            //else {
            // GetRelatedData(executionContext, record_id, "account", "Lookup", "_jarvis_currency_value", "jarvis_gopoutcurrency");
            // GetRelatedData(executionContext, record_id, "account", "Lookup", "_jarvis_currency_value", "jarvis_totallimitoutcurrency");
            // }
            var caseId = executionContext.getFormContext().getAttribute("jarvis_incident").getValue()[0].id;
            caseId = caseId.replace('{', '').replace('}', '');
            //let paymentType = executionContext.getFormContext().getAttribute("jarvis_paymenttype")?.getValue();
            if (paymentType !== 334030002) // Credit Card
            {
                var fetchXml = `<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
        <entity name='jarvis_gop'>
        <attribute name='jarvis_totallimitin' />
        <attribute name='jarvis_paymenttype' />
          <order attribute='jarvis_gopapprovaltime' descending='true' />
          <filter type='and'>
            <condition attribute='jarvis_incident' operator='eq'  uitype='incident' value='{`+ caseId + `}' />
            <condition attribute='jarvis_requesttype' operator='in'>
              <value>334030001</value>
              <value>334030002</value>
            </condition>
            <condition attribute='jarvis_gopapproval' operator='eq' value='334030001' />
            <condition attribute='jarvis_relatedgop' operator='null' />
            <condition attribute='jarvis_dealer' operator='eq' uitype='account' value='{`+ record_id + `}' />
            <condition attribute='statecode' operator='eq' value='0' />
          </filter>
        </entity>
      </fetch>`;

                fetchXml = "?fetchXml=" + encodeURIComponent(fetchXml);
                Xrm.WebApi.retrieveMultipleRecords("jarvis_gop", fetchXml).then(
                    function success(result) {
                        let paymentType = executionContext.getFormContext().getAttribute("jarvis_paymenttype")?.getValue();
                        if (result.entities.length > 0) {
                            var approvedGOP = result.entities[0];
                            ////208314 Do not set Existing HD Approved goplimitin for copiedRecord
                            if (approvedGOP["jarvis_totallimitin"] && parentGop == null) {
                                executionContext.getFormContext().getAttribute("jarvis_goplimitin").setValue(approvedGOP["jarvis_totallimitin"]);
                                executionContext.getFormContext().getAttribute("jarvis_goplimitout").setValue(null);

                            }
                            if (approvedGOP["jarvis_paymenttype"]) {
                                executionContext.getFormContext().getAttribute("jarvis_paymenttype").setValue(approvedGOP["jarvis_paymenttype"]);
                                if (paymentType == null || (paymentType != null && paymentType != approvedGOP["jarvis_paymenttype"])) {
                                    executionContext.getFormContext().getAttribute("jarvis_paymenttype").fireOnChange();
                                }
                                /* else if (approvedGOP["jarvis_paymenttype"] == 334030002) // Credit Card
                                 {
                                     calculateBookingAmount(executionContext, "");
                                 }*/
                            }

                        }
                        /*else {
                            if (paymentType == 334030002) // Credit Card
                            {
                                calculateBookingAmount(executionContext, "");
                            }
                        }*/
                    },
                    function (error) {
                        console.log(error.message);
                        // handle error conditions
                    }
                );

                if (!requestType || requestType == 334030001) {
                    // #589879-Restrict creation of duplicate GOPs
                    var parentGop = formContext.getAttribute("jarvis_parentgop").getValue();
                    if (parentGop == null && formType === 1) {
                        var fetchXml = `<fetch version="1.0" output-format="xml-platform" mapping="logical" distinct="false">
            <entity name="jarvis_gop">
            <attribute name="jarvis_gopid" />
            <attribute name="jarvis_name" />
            <attribute name="createdon" />
            <order attribute="jarvis_name" descending="false" />
            <filter type="and">
                <condition attribute="jarvis_incident" operator="eq" uitype="incident" value="{`+ caseId + `}" />
                <condition attribute="jarvis_requesttype" operator="eq" value="334030001" />
                <condition attribute='statecode' operator='eq' value='0' />
                <condition attribute='jarvis_relatedgop' operator='null' />
                <condition attribute='jarvis_dealer' operator='eq' uitype='account' value='{`+ record_id + `}' />`;
                        //if (parentGop != null) {
                        //    fetchXml = fetchXml + `<condition attribute='jarvis_gopapproval' operator='eq' value='0' />`;
                        //}
                        fetchXml = fetchXml + `</filter>
            </entity>
        </fetch>`;


                        fetchXml = "?fetchXml=" + encodeURIComponent(fetchXml);
                        Xrm.WebApi.retrieveMultipleRecords("jarvis_gop", fetchXml).then(
                            function success(result) {
                                if (result.entities.length > 0) {
                                    existingGOP = true;
                                }
                                else {
                                    existingGOP = false;
                                }
                            },
                            function (error) {
                                console.log(error.message);
                                existingGOP = false;
                            }
                        );
                    }
                }
                else if (requestType == 334030002) {
                    // #589879-Restrict creation of duplicate GOPs
                    var gopDealer = executionContext.getFormContext().getAttribute(GopCntrls.Fields.RepairingDealer)?.getValue();
                    if (gopDealer) {
                        var gopDealerid = gopDealer[0].id;
                        gopDealerid = gopDealerid.replace('{', '').replace('}', '');

                        if (formType === 1 && gopDealerid != null && parentGop == null) {
                            var fetchXml = `<fetch version="1.0" output-format="xml-platform" mapping="logical" distinct="false">
                <entity name="jarvis_gop">
                <attribute name="jarvis_gopid" />
                <attribute name="jarvis_name" />
                <attribute name="createdon" />
                <order attribute="jarvis_name" descending="false" />
                <filter type="and">
                    <condition attribute="jarvis_incident" operator="eq" uitype="incident" value="{`+ caseId + `}" />
                    <condition attribute="jarvis_requesttype" operator="eq" value="334030002" />
                    <condition attribute='statecode' operator='eq' value='0' />
                    <condition attribute='jarvis_relatedgop' operator='null' />
                    <condition attribute='jarvis_dealer' operator='eq' uitype='account' value='{`+ record_id + `}' />
                    <condition attribute='jarvis_repairingdealer' operator='eq'  value='{`+ gopDealerid + `}' />`;
                            //if (parentGop != null) {
                            //        fetchXml = fetchXml + `<condition attribute = 'jarvis_gopapproval' operator = 'eq' value = '0' /> `;
                            //    }
                            fetchXml = fetchXml + `</filter>
            </entity >
        </fetch >`;

                            fetchXml = "?fetchXml=" + encodeURIComponent(fetchXml);
                            Xrm.WebApi.retrieveMultipleRecords("jarvis_gop", fetchXml).then(
                                function success(result) {
                                    if (result.entities.length > 0) {
                                        existingGOP = true;
                                    }
                                    else {
                                        existingGOP = false;
                                    }
                                },
                                function (error) {
                                    console.log(error.message);
                                    existingGOP = false;
                                }
                            );
                        }
                    }
                }
            }// If Not credit card ends Here
            else {
                var parentGop = formContext.getAttribute("jarvis_parentgop").getValue();
                if (parentGop == null && formType === 1) {
                    var fetchXml = `<fetch version="1.0" output-format="xml-platform" mapping="logical" distinct="false">
                    <entity name="jarvis_gop">
                    <attribute name="jarvis_gopid" />
                    <attribute name="jarvis_name" />
                    <attribute name="createdon" />
                    <order attribute="jarvis_name" descending="false" />
                    <filter type="and">
                    <condition attribute="jarvis_incident" operator="eq" uitype="incident" value="{`+ caseId + `}" />
                    <condition attribute="jarvis_gopapproval" operator="eq" value="334030001" />
                    <condition attribute="jarvis_paymenttype" operator="eq" value="334030002" />
                    <condition attribute='statecode' operator='eq' value='0' />
                    <condition attribute='jarvis_relatedgop' operator='null' />
                    <condition attribute='jarvis_dealer' operator='ne' uitype='account' value='{`+ record_id + `}' />`;
                    fetchXml = fetchXml + `</filter>
                        </entity>
                    </fetch>`;


                    fetchXml = "?fetchXml=" + encodeURIComponent(fetchXml);
                    Xrm.WebApi.retrieveMultipleRecords("jarvis_gop", fetchXml).then(
                        function success(result) {
                            if (result.entities.length > 0) {
                                Xrm.Navigation.openAlertDialog("Please take the same GOP Dealer as the already approved GOP for payment type credit card, or cancel the Approved GOP Dealer to use a new one for credit card purpose");
                                executionContext.getFormContext().getAttribute("jarvis_dealer").setValue(null);
                            }
                        },
                        function (error) {
                            console.log(error.message)
                        }
                    );
                }
            }
        }
        else {
            executionContext.getFormContext().getAttribute(GopCntrls.Fields.GopINCurrency).setValue(null);
            executionContext.getFormContext().getAttribute(GopCntrls.Fields.GopOutCurrency).setValue(null);
        }

    }




}
function onRepairingDealerSelect(executionContext) {
    GopCntrls = {
        Fields: {
            Dealer: "jarvis_repairingdealer",
            GopINCurrency: "jarvis_gopoutcurrency",
            GopOutCurrency: "jarvis_totallimitoutcurrency",

        }
    }
    // Access the field on the form
    var formContext = executionContext.getFormContext();
    var field = executionContext.getFormContext().getAttribute("jarvis_repairingdealer");
    var parentGop = executionContext.getFormContext().getAttribute("jarvis_parentgop")?.getValue();
    var formType = formContext.ui.getFormType();
    let paymentType = executionContext.getFormContext().getAttribute("jarvis_paymenttype")?.getValue();
    //var requestType = executionContext.getFormContext().getAttribute("jarvis_requesttype")?.getValue();
    if (field != null) {
        var value = field.getValue();
        if (value) {
            var record_id = field.getValue()[0].id;
            record_id = record_id.replace('{', '').replace('}', '');

            GetRelatedData(executionContext, record_id, "jarvis_passout", "Lookup", "_transactioncurrencyid_value", "jarvis_gopoutcurrency");
            GetRelatedData(executionContext, record_id, "jarvis_passout", "Lookup", "_transactioncurrencyid_value", "jarvis_totallimitoutcurrency");

            var gopLimitOut = executionContext.getFormContext().getAttribute("jarvis_goplimitout")?.getValue();

            // if (!gopLimitOut) {
            executionContext.getFormContext().getAttribute("jarvis_goplimitin").setValue(null);
            //IF only one Repairing Dealer available in the pass out, this one need to be prepopulated ELSE do not populate (from pass out)
            var caseId = executionContext.getFormContext().getAttribute("jarvis_incident").getValue()[0].id;
            caseId = caseId.replace('{', '').replace('}', '');
            var fetchXml = `<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
            <entity name='jarvis_passout'>
                <attribute name='jarvis_passoutid' />
                <attribute name='jarvis_goplimitout' />
                <attribute name='jarvis_paymenttype' />
            <order attribute='modifiedon' descending='true' />
                <filter type='and'>
                <condition attribute='jarvis_incident' operator='eq' uitype='incident' value='{`+ caseId + `}' />
                <condition attribute='statecode' operator='eq' value='0' />
                <condition attribute='jarvis_passoutid' operator='eq'  value='{`+ record_id + `}' />
                </filter>
            </entity>
            </fetch>`;

            fetchXml = "?fetchXml=" + encodeURIComponent(fetchXml);
            Xrm.WebApi.retrieveMultipleRecords("jarvis_passout", fetchXml).then(
                function success(result) {
                    if (result.entities.length > 0) {
                        var approvedGOP = result.entities[0];
                        ////208314 Do not set Existing HD Approved goplimitout for copiedRecord
                        if (approvedGOP["jarvis_goplimitout"] && parentGop == null) {
                            executionContext.getFormContext().getAttribute("jarvis_goplimitout").setValue(approvedGOP["jarvis_goplimitout"]);

                        }
                        if (approvedGOP["jarvis_paymenttype"]) {
                            executionContext.getFormContext().getAttribute("jarvis_paymenttype").setValue(approvedGOP["jarvis_paymenttype"]);
                            executionContext.getFormContext().getAttribute("jarvis_paymenttype").fireOnChange();
                        }

                    }
                },
                function (error) {
                    console.log(error.message);
                    // handle error conditions
                }
            );
            // #589879-Restrict creation of duplicate GOPs
            var gopDealer = executionContext.getFormContext().getAttribute("jarvis_dealer")?.getValue();
            if (gopDealer) {
                var gopDealerid = gopDealer[0].id;
                gopDealerid = gopDealerid.replace('{', '').replace('}', '');

                if (formType === 1 && gopDealerid != null && parentGop == null && paymentType !== 334030002) {
                    var fetchXml = `<fetch version="1.0" output-format="xml-platform" mapping="logical" distinct="false">
                            <entity name="jarvis_gop">
                            <attribute name="jarvis_gopid" />
                            <attribute name="jarvis_name" />
                            <attribute name="createdon" />
                            <order attribute="jarvis_name" descending="false" />
                            <filter type="and">
                                <condition attribute="jarvis_incident" operator="eq" uitype="incident" value="{`+ caseId + `}" />
                                <condition attribute="jarvis_requesttype" operator="eq" value="334030002" />
                                <condition attribute='statecode' operator='eq' value='0' />
                                <condition attribute='jarvis_relatedgop' operator='null' />
                                <condition attribute='jarvis_dealer' operator='eq' uitype='account' value='{`+ gopDealerid + `}' />
                                <condition attribute='jarvis_repairingdealer' operator='eq'  value='{`+ record_id + `}' />`;
                    //if (parentGop != null) {
                    //    fetchXml = fetchXml + `<condition attribute = 'jarvis_gopapproval' operator = 'eq' value = '0' /> `;
                    //}
                    fetchXml = fetchXml + `</filter>
            </entity >
        </fetch >`;

                    fetchXml = "?fetchXml=" + encodeURIComponent(fetchXml);
                    Xrm.WebApi.retrieveMultipleRecords("jarvis_gop", fetchXml).then(
                        function success(result) {
                            if (result.entities.length > 0) {
                                existingGOP = true;
                            }
                            else {
                                existingGOP = false;
                            }
                        },
                        function (error) {
                            console.log(error.message);
                            existingGOP = false;
                        }
                    );
                }
            }
        }
        else {
            executionContext.getFormContext().getAttribute(GopCntrls.Fields.GopINCurrency).setValue(null);
            executionContext.getFormContext().getAttribute(GopCntrls.Fields.GopOutCurrency).setValue(null);
        }


    }




}
function onRequestTypeChange(executionContext) {
    var formContext = executionContext.getFormContext();
    var requestType = executionContext.getFormContext().getAttribute("jarvis_requesttype")?.getValue();
    var currentGOPID = executionContext.getFormContext().data.entity.getId();
    var fetchXml = "";
    var caseDetails = formContext.getAttribute("jarvis_incident")?.getValue();
    if (caseDetails !== null && caseDetails[0].id !== null) {
        var caseId = caseDetails[0].id;
        caseId = caseId.replace('{', '').replace('}', '');
        var jarvis_paymenttype = formContext.getAttribute("jarvis_paymenttype")?.getValue();
        if (requestType == 334030002) // GOP+RD
        {

            //IF only one Repairing Dealer available in the pass out, this one need to be prepopulated ELSE do not populate (from pass out)
            var fetchXml = `<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                <entity name='jarvis_passout'>
                  <attribute name='jarvis_passoutid' />
                  <attribute name='jarvis_goplimitout' />
                  <attribute name='createdon' />
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
                    if (result.entities.length == 1) {
                        var approvedGOP = result.entities[0];
                        if (approvedGOP["jarvis_goplimitout"]) {
                            executionContext.getFormContext().getAttribute("jarvis_goplimitout").setValue(approvedGOP["jarvis_goplimitout"]);

                        }
                        if (approvedGOP["jarvis_passoutid"]) {
                            var lookup = new Array();
                            lookup[0] = new Object();
                            lookup[0].id = approvedGOP["jarvis_passoutid"];
                            lookup[0].name = approvedGOP["jarvis_name"];
                            lookup[0].entityType = "jarvis_passout";
                            executionContext.getFormContext().getAttribute("jarvis_repairingdealer").setValue(lookup);

                            onRepairingDealerSelect(executionContext);
                        }

                    }
                },
                function (error) {
                    console.log(error.message);
                    // handle error conditions
                }
            );

            // 573285-Autopopulate the GOP OUT value AND Payment type  from the last Pass Out of the Repairing Dealer AND the GOP Dealer from the last approved GOP
            Xrm.WebApi.retrieveMultipleRecords("jarvis_gop", "?$filter=(statecode eq 0 and jarvis_gopapproval eq 334030001 and _jarvis_relatedgop_value eq null and _jarvis_incident_value eq " + caseId + " )&$orderby= modifiedon desc").then(
                function success(result) {
                    if (result.entities.length > 0) {
                        var approvedGOP = result.entities[0];
                        if (approvedGOP["_jarvis_dealer_value"]) {
                            var lookup = new Array();
                            lookup[0] = new Object();
                            lookup[0].id = approvedGOP["_jarvis_dealer_value"];
                            lookup[0].name = approvedGOP["_jarvis_dealer_value@OData.Community.Display.V1.FormattedValue"];
                            lookup[0].entityType = approvedGOP["_jarvis_dealer_value@Microsoft.Dynamics.CRM.lookuplogicalname"];
                            formContext.getAttribute("jarvis_dealer").setValue(lookup);
                            onDealerSelect(executionContext);
                            var parentGop = executionContext.getFormContext().getAttribute("jarvis_parentgop")?.getValue();
                            if (parentGop == null && jarvis_paymenttype != 334030002) {
                                existingGOP = true;
                            }
                            else {
                                existingGOP = false;
                            }
                        }

                    }
                },
                function (error) {
                    console.log(error.message);
                    // handle error conditions
                }
            );

        }
        if (jarvis_paymenttype == 334030002) {
            onPaymentTypeChange(executionContext);
        }
    }
}
ShowDealerNotFoundGOPNotification = function (executionContext) {
    var level = "WARNING";
    var uniqueId = "DEALERNOTFOUND";
    let formContext = executionContext.getFormContext();
    let createdBy = formContext.getAttribute("createdby")?.getValue();
    let dealerName = formContext.getAttribute("jarvis_dealer")?.getValue();
    formContext.ui.clearFormNotification(uniqueId);
    if (dealerName !== null && dealerName[0].name !== null && dealerName[0].name !== undefined && dealerName[0].name.includes("DEALER NOT FOUND")) {
        var msg = "Dealer not available in OneCase or Mercurius - Contact system support";
        formContext.ui.setFormNotification(msg, level, uniqueId);
    }
    else {
        formContext.ui.clearFormNotification(uniqueId);
    }
}

function onFormLoad(executionContext) {
    AddPreSearchToLookup(executionContext);
    caseCntrls = {
        SourceFields: [

            {
                TargetField: "jarvis_homedealer",
                //RecordId : formContext.getAttribute("jarvis_incident")?.getValue(),
                SourceTable: "incident",
                Fieldtype: "Lookup",
                Field: "jarvis_homedealer",
                DatasetAttribute: "_jarvis_homedealer_value" // _jarvis_country_value
            }

        ]
    }
    var fieldArr = ["jarvis_gopapproval"];
    var formContext = executionContext.getFormContext();
    lockFields(formContext, fieldArr, false);
    var casevalue = formContext.getAttribute("jarvis_incident")?.getValue();
    if (casevalue && casevalue.length > 0) {
        var caseId = casevalue[0].id;
        caseId = caseId.replace('{', '').replace('}', '');
        var countryId;
        var existingHD = formContext.getAttribute("jarvis_dealer")?.getValue();
        var gopId = Xrm.Page.data.entity.getId();
        if (!gopId) {
            Xrm.WebApi.retrieveRecord("incident", caseId).then(
                function success(result) {
                    if (result["_jarvis_homedealer_value"]) {
                        var lookup = new Array();
                        lookup[0] = new Object();
                        lookup[0].id = result["_jarvis_homedealer_value"];
                        lookup[0].name = result["_jarvis_homedealer_value@OData.Community.Display.V1.FormattedValue"];
                        lookup[0].entityType = result["_jarvis_homedealer_value@Microsoft.Dynamics.CRM.lookuplogicalname"];
                        if (existingHD == null)
                            formContext.getAttribute("jarvis_dealer").setValue(lookup);
                        onDealerSelect(executionContext);
                        ShowDealerNotFoundGOPNotification(executionContext);
                    }
                    if (result["_jarvis_country_value"]) {
                        countryId = result["_jarvis_country_value"];
                    }
                    if (result["jarvis_restgoplimitout"]) {
                        caseAvailableGOP = result["jarvis_restgoplimitout"];
                    }

                }
            );

            var requestType = executionContext.getFormContext().getAttribute("jarvis_requesttype")?.getValue();
            if (requestType == 334030000)// GOP
            {
                if (countryId) {
                    GetRelatedData(executionContext, countryId, "jarvis_country", "Lookup", "_transactioncurrencyid_value", "jarvis_totallimitoutcurrency");
                }
                var fetchXml = `<fetch version="1.0" output-format="xml-platform" mapping="logical" distinct="false">
            <entity name="jarvis_gop">
            <attribute name="jarvis_gopid" />
            <attribute name="jarvis_name" />
            <attribute name="createdon" />
            <order attribute="jarvis_name" descending="false" />
            <filter type="and">
                <condition attribute="jarvis_incident" operator="eq" uitype="incident" value="{`+ caseId + `}" />
                <condition attribute="jarvis_requesttype" operator="eq" value="334030000" />
            </filter>
            </entity>
        </fetch>`;

                fetchXml = "?fetchXml=" + encodeURIComponent(fetchXml);
                Xrm.WebApi.retrieveMultipleRecords("jarvis_gop", fetchXml).then(
                    function success(result) {
                        if (result.entities.length > 0) {
                            existingGOP = true;
                            //executionContext.getFormContext().getAttribute("jarvis_goplimitin").setValue(null);
                            //alert("A GOP can be only created once. Please change the type of the GOP to GOP+");
                        }
                    },
                    function (error) {
                        console.log(error.message);
                        // handle error conditions
                    }
                );


            }
            else if (requestType == 334030001)// GOP+HD
            {// Insert the Total GOP Limit IN from the last approved GOP+ of type
                //Total Limit OUT Currency (From case Location Country)
                var parentGop = executionContext.getFormContext().getAttribute("jarvis_parentgop")?.getValue();
                if (countryId) {
                    GetRelatedData(executionContext, countryId, "jarvis_country", "Lookup", "_transactioncurrencyid_value", "jarvis_totallimitoutcurrency");

                }
                var gopDealerValue = executionContext.getFormContext().getAttribute("jarvis_dealer").getValue();
                var existingLimitIn = executionContext.getFormContext().getAttribute("jarvis_goplimitin")?.getValue();
                if (gopDealerValue && !existingLimitIn) {
                    var gopDealer = gopDealerValue[0].id;
                    gopDealer = gopDealer.replace('{', '').replace('}', '');
                    var fetchXml = `<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                <entity name='jarvis_gop'>
                <attribute name='jarvis_totallimitin' />
                <order attribute='jarvis_gopapprovaltime' descending='true' />
                <filter type='and'>
                    <condition attribute='jarvis_incident' operator='eq'  uitype='incident' value='{`+ caseId + `}' />
                    <condition attribute='jarvis_requesttype' operator='in'>
                    <value>334030001</value>
                    <value>334030002</value>
                    </condition>
                    <condition attribute='jarvis_gopapproval' operator='eq' value='334030001' />
                    <condition attribute='jarvis_relatedgop' operator='null' />
                    <condition attribute='jarvis_dealer' operator='eq' uitype='account' value='{`+ gopDealer + `}' />
                    <condition attribute='statecode' operator='eq' value='0' />
                </filter>
                </entity>
            </fetch>`;

                    fetchXml = "?fetchXml=" + encodeURIComponent(fetchXml);
                    Xrm.WebApi.retrieveMultipleRecords("jarvis_gop", fetchXml).then(
                        function success(result) {
                            if (result.entities.length > 0) {
                                var approvedGOP = result.entities[0];
                                ////208314 Do not set Existing HD Approved totallimitin for copiedRecord
                                if (approvedGOP["jarvis_totallimitin"] && parentGop == null)
                                    executionContext.getFormContext().getAttribute("jarvis_goplimitin").setValue(approvedGOP["jarvis_totallimitin"]);
                            }
                        },
                        function (error) {
                            console.log(error.message);
                            // handle error conditions
                        }
                    );
                }

            }
            else if (requestType == 334030002) // GOP+RD
            {
                lockFields(formContext, fieldArr, true);
                var existingrepairingDealer = executionContext.getFormContext().getAttribute("jarvis_repairingdealer")?.getValue();
                var existingLimitIn = executionContext.getFormContext().getAttribute("jarvis_goplimitout")?.getValue();
                if (existingrepairingDealer == null && !existingLimitIn) {
                    //IF only one Repairing Dealer available in the pass out, this one need to be prepopulated ELSE do not populate (from pass out)
                    var fetchXml = `<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                <entity name='jarvis_passout'>
                <attribute name='jarvis_passoutid' />
                <attribute name='jarvis_goplimitout' />
                <attribute name='createdon' />
                <attribute name='jarvis_name' />
                <attribute name='modifiedon' />
                <order attribute='modifiedon' descending='true' />
                <filter type='and'>
                <condition attribute='jarvis_incident' operator='eq' uitype='incident' value='{`+ caseId + `}' />
                <condition attribute='statecode' operator='eq' value='0' />
                </filter>
                </entity>
                </fetch>`;

                    fetchXml = "?fetchXml=" + encodeURIComponent(fetchXml);
                    Xrm.WebApi.retrieveMultipleRecords("jarvis_passout", fetchXml).then(
                        function success(result) {
                            if (result.entities.length == 1) {
                                var approvedGOP = result.entities[0];
                                if (approvedGOP["jarvis_goplimitout"]) {
                                    executionContext.getFormContext().getAttribute("jarvis_goplimitout").setValue(approvedGOP["jarvis_goplimitout"]);

                                }
                                if (approvedGOP["jarvis_passoutid"]) {
                                    var lookup = new Array();
                                    lookup[0] = new Object();
                                    lookup[0].id = approvedGOP["jarvis_passoutid"];
                                    lookup[0].name = approvedGOP["jarvis_name"];
                                    lookup[0].entityType = "jarvis_passout";
                                    executionContext.getFormContext().getAttribute("jarvis_repairingdealer").setValue(lookup);

                                    onRepairingDealerSelect(executionContext);
                                }

                            }
                        },
                        function (error) {
                            console.log(error.message);
                            // handle error conditions
                        }
                    );
                }
                else {
                    executionContext.getFormContext().getAttribute("jarvis_gopoutcurrency")?.fireOnChange();
                }
            }
        }
    }
}
function GetRelatedData(executionContext, record_id, targetEntity, Fieldtype, DatasetAttribute, Field) {
    var formContext = executionContext.getFormContext();
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
                    ////213392 -- Trigger onLoad.
                    if (Field == "jarvis_gopincurrency" || Field == "jarvis_gopoutcurrency") {
                        formContext.getAttribute(Field).fireOnChange();
                    }
                }
                else {
                    formContext.getAttribute(Field).setValue(result[DatasetAttribute]);
                }
                if (targetEntity == "account" && result["_jarvis_address1_country_value"]) {
                    dealerCountry = result["_jarvis_address1_country_value"];
                }
                //if (Field == "jarvis_vat") {
                //    calculateBookingAmount(executionContext, "");

                //}

            }

        }
    );
}
function GOPOnSave(executionContext) {
    var formContext = executionContext.getFormContext();
    var gopApproval = formContext.getAttribute("jarvis_gopapproval")?.getValue();
    var isApproved = false;
    if (gopApproval !== null && gopApproval === 334030001) {
        isApproved = true;
    }
    var fieldArr = ["jarvis_gopapproval", "jarvis_contact"];
    var requestType = formContext.getAttribute("jarvis_requesttype")?.getValue();
    var totalLimitOut = formContext.getAttribute("jarvis_totallimitout")?.getValue();
    var totalLimitOut = formContext.getAttribute("jarvis_totallimitout")?.getValue();
    if (totalLimitOut) {
        totalLimitOut = formContext.getAttribute("jarvis_totallimitout")?.getValue();
    }
    else if (formContext.getAttribute("jarvis_goplimitout")?.getValue()) {
        totalLimitOut = formContext.getAttribute("jarvis_goplimitout")?.getValue();
    }
    else {
        totalLimitOut = formContext.getAttribute("jarvis_goplimitin")?.getValue();
    }
    var incidentId = formContext.getAttribute("jarvis_incident")?.getValue()[0].id.slice(1, -1);
    var jarvis_restgoplimitout;
    formContext.ui.clearFormNotification("ERR_GOPAmountValidation");
    formContext.ui.clearFormNotification("ERR_GOPCreateValidation");
    var relatedGopfield = formContext.getAttribute("jarvis_relatedgop")?.getValue();
    var jarvis_requesttype = formContext.getAttribute("jarvis_requesttype")?.getValue();
    var jarvis_paymenttype = formContext.getAttribute("jarvis_paymenttype")?.getValue();
    var jarvis_dealer = formContext.getAttribute("jarvis_dealer")?.getValue();
    var jarvis_repairingdealer = formContext.getAttribute("jarvis_repairingdealer")?.getValue();
    var jarvis_goplimitin = formContext.getAttribute("jarvis_goplimitin")?.getValue();
    var jarvis_gopincurrency = formContext.getAttribute("jarvis_gopincurrency")?.getValue();
    var jarvis_goplimitout = formContext.getAttribute("jarvis_goplimitout")?.getValue();
    var jarvis_gopoutcurrency = formContext.getAttribute("jarvis_gopoutcurrency")?.getValue();
    var mandatorycheckfailed = false;
    var gopApproval = formContext.getAttribute("jarvis_gopapproval")?.getValue();



    if (existingCCGOP) {
        isApproved = false;
        Xrm.Navigation.openAlertDialog("Only one GOP can be sent at a time to Volvo Pay. If you wish to send this GOP to Volvo Pay, please either decline or request approval for the existing GOP.");
        executionContext.getEventArgs().preventDefault();
    }
    else if (existingGOP) {
        isApproved = false;
        Xrm.Navigation.openAlertDialog("Copy the GOP HD/RD already existing for this Dealer. Only create a new request when involving a new Dealer");
        executionContext.getEventArgs().preventDefault();
    }

    if (gopApproval !== null && gopApproval === 334030002) {
        formContext.getAttribute("jarvis_contact").setValue("Cancelled from OneCase");
    }
    // Check mandatory fields
    if (!jarvis_requesttype || !jarvis_paymenttype || !jarvis_dealer) {
        mandatorycheckfailed = true;
    }
    if (jarvis_requesttype && (requestType == 334030002 || (jarvis_paymenttype == 334030002))) {
        if ((!jarvis_goplimitout || jarvis_goplimitout == 0) || !jarvis_gopoutcurrency) {
            mandatorycheckfailed = true;
        }
        if (requestType == 334030002 && !jarvis_repairingdealer) {
            mandatorycheckfailed = true;
        }
    }
    else {
        if ((!jarvis_goplimitin || jarvis_goplimitin == 0) || !jarvis_gopincurrency) {
            mandatorycheckfailed = true;
        }
    }
    if (mandatorycheckfailed) {
        executionContext.getEventArgs().preventDefault();
    }
    else {
        // Lock fields
        var paymentType = executionContext.getFormContext().getAttribute("jarvis_paymenttype")?.getValue();
        //Is Approve or Payment Type is Credit Card
        if (isApproved == true || relatedGopfield || (paymentType == 334030002 && gopApproval != 334030000)) {
            lockFields(formContext, fieldArr, true);
        }
        else {
            lockFields(formContext, fieldArr, false);
        }
        var requestType = formContext.getAttribute("jarvis_requesttype")?.getValue();
        if (requestType == 334030001) {
            var field = formContext.getAttribute("jarvis_relatedgop");
            if (field != null) {
                var value = field.getValue();
                if (value) {
                    //formContext.ui.setFormNotification("This GOP cannot be edited, go to the corresponding GOP+ RD Request to update it", "WARNING", "ERR_SHADOWGOP");
                    var fieldArr = ["jarvis_gopapproval"];
                    lockFields(formContext, fieldArr, true);
                    //executionContext.getEventArgs().preventDefault();
                }
            }
        }
    }

}

function GopLimitOutCaseOnLoad(executionContext) {
    let formContext = executionContext.getFormContext();
    let gridContext = formContext.getControl("Subgrid_new_34");
    formContext.ui.clearFormNotification("ERR_GOPAmountValidation");
    let gopGridOnloadFunction = function () {
        caseAvailableGOP = formContext.getAttribute("jarvis_restgoplimitout").getValue();

    };
    gridContext.addOnLoad(gopGridOnloadFunction);
}
function GOPSubgridOnSave(executionContext) {
    var formContext = executionContext.getFormContext();
    var gopApproval = formContext.getAttribute("jarvis_gopapproval")?.getValue();
    var isApproved = false;
    if (gopApproval !== null && gopApproval === 334030001) {
        isApproved = true;
    }
    var fieldArr = ["jarvis_gopapproval"];
    var requestType = formContext.getAttribute("jarvis_requesttype")?.getValue();
    var totalLimitOut = formContext.getAttribute("jarvis_totallimitout")?.getValue();
    if (totalLimitOut) {
        totalLimitOut = formContext.getAttribute("jarvis_totallimitout")?.getValue();
    }
    else if (formContext.getAttribute("jarvis_goplimitout")?.getValue()) {
        totalLimitOut = formContext.getAttribute("jarvis_goplimitout")?.getValue();
    }
    else {
        totalLimitOut = formContext.getAttribute("jarvis_goplimitin")?.getValue();
    }
    var incidentId = Xrm.Utility.getPageContext().input.entityId.replace('{', '').replace('}', '');
    //if (isApproved == true && (requestType == 334030001 || requestType == 334030002) && (caseAvailableGOP > 0 && totalLimitOut < caseAvailableGOP)) {
    //if (isApproved == true && requestType == 334030001 && (caseAvailableGOP > 0 && totalLimitOut < caseAvailableGOP)) {

    //    isApproved = false;
    //    formContext.ui.setFormNotification("Please use the available Amount on the case before creating a new GOP + request", "ERROR", "ERR_GOPAmountValidation");
    //    executionContext.getEventArgs().preventDefault();
    //}
    /*if (isApproved == true && requestType == 334030002) {
        if (formContext.getAttribute("jarvis_goplimitout")?.getValue()) {
            totalLimitOut = formContext.getAttribute("jarvis_goplimitout")?.getValue();
        }
        if (caseAvailableGOP > 0 && totalLimitOut < caseAvailableGOP) {
            isApproved = false;
            formContext.ui.setFormNotification("Please use the available Amount on the case before creating a new GOP + request", "ERROR", "ERR_GOPAmountValidation");
            executionContext.getEventArgs().preventDefault();
        }
    }*/
}

function lockFields(formContext, fieldArr, readOnly) {
    "use strict";

    if (fieldArr.length === 0) {
        return;
    }

    for (var ii = 0; ii < fieldArr.length; ii++) {
        var fieldControl = formContext.getControl(fieldArr[ii]);
        if (fieldControl) {
            fieldControl.setDisabled(readOnly);
        }
    }
}
function onMainFormLoad(executionContext) {

    var formContext = executionContext.getFormContext();

    //// Nullify Related Gop While Copy Gop and Lock Gop Request Type.
    var parentGop = formContext.getAttribute("jarvis_parentgop").getValue();
    var formType = formContext.ui.getFormType();
    if (parentGop !== null && formType === 1) {
        formContext.getAttribute("jarvis_relatedgop").setValue(null);
        formContext.getControl("jarvis_requesttype").setDisabled(true);
    }

    var incident = formContext.getAttribute("jarvis_incident").getValue();
    if (incident != null) {
        var incidentId = formContext.getAttribute("jarvis_incident").getValue()[0].id;
        incidentId = incidentId.replace('{', '').replace('}', '');
        var countryId;

        Xrm.WebApi.retrieveRecord("incident", incidentId, "?$select=jarvis_restgoplimitout").then(
            function success(result) {
                caseAvailableGOP = result["jarvis_restgoplimitout"];
            },
        );
    }

    // UserStory-91008  GOP + request RD to HD
    var requestType = formContext.getAttribute("jarvis_requesttype")?.getValue();
    if (requestType == 334030001) {
        var field = formContext.getAttribute("jarvis_relatedgop");
        if (field != null) {
            var value = field.getValue();
            if (value) {
                formContext.ui.setFormNotification("This GOP cannot be edited, go to the corresponding GOP+ RD Request to update it", "WARNING", "ERR_SHADOWGOP");
                var fieldArr = ["jarvis_gopapproval"];
                lockFields(formContext, fieldArr, true);
                //executionContext.getEventArgs().preventDefault();
            }
        }
    }
    var paymentType = executionContext.getFormContext().getAttribute("jarvis_paymenttype")?.getValue();
    var fetchXml = "";
    //let formType = formContext.ui.getFormType();
    if (paymentType == 334030002) // Credit Card
    {
        var attribute = formContext.getAttribute("jarvis_dealer")
        attribute.controls.forEach(control => control.addPreSearch(filterCustomerAccounts))
        var gopApproval = formContext.getAttribute("jarvis_gopapproval")?.getValue();

        if (gopApproval !== null && gopApproval != 334030002) {
            formContext.getControl("jarvis_gopapproval")?.removeOption(334030002);
            formContext.getControl("jarvis_gopapproval")?.addOption({ text: "Declined", value: 334030002 });
        }
        if (gopApproval !== null && gopApproval != 334030001) {
            formContext.getControl("jarvis_gopapproval")?.removeOption(334030001);
        }
        if (formType === 2) {
            let gopId = formContext.data.entity.getId().slice(1, -1);
            var volvoPaymentStatus = formContext.getAttribute("jarvis_volvopaypaymentrequestsent")?.getValue();
            var volvoPaymentStatusReceived = formContext.getAttribute("jarvis_volvopaypaymentstatusreceived")?.getValue();
            var bookingAmount = formContext.getAttribute("jarvis_creditcardgopinbooking")?.getValue();
            if (bookingAmount != null && parseFloat(bookingAmount) <= 0 && volvoPaymentStatus == 334030000 && gopApproval === 334030000) {
                if (parseFloat(bookingAmount) < 0) {
                    Xrm.Navigation.openAlertDialog("The booking amount is negative. A decrease in the Limit OUT is not possible using the credit card payment type. Please proceed by either copying or declining this GOP.");
                }
                //var confirmStrings = {
                //    text: "The booking amount is negative. A decrease in the Limit OUT is not possible using the credit card payment type. Please proceed by either copying or declining this GOP.", title: "Negative Booking Amount", confirmButtonLabel: "Copy", cancelButtonLabel: "Decline"
                //};
                //Xrm.Navigation.openConfirmDialog(confirmStrings, confirmOptions).then(
                //    function (success) {
                //        if (success.confirmed) {
                //        }
                //        else if (success.confirmed==false) {
                //        }
                //    });
                return;
            }
            else if (volvoPaymentStatus != null && volvoPaymentStatus == 334030000 && gopApproval === 334030000) {
                openConfirmDialogBox(gopId, formContext);
            }
            else if ((volvoPaymentStatus != null && volvoPaymentStatus == 334030003) || (volvoPaymentStatusReceived != null && volvoPaymentStatusReceived == 334030003)) {
                Xrm.Navigation.openAlertDialog("The Payment request is failed due to invalid Language/Customer Email Address/unforeseen issues , please create a copy of the GOP with valid details");
            }
        }
        if (gopApproval == 334030000 && formType === 2) {

            //disable approver name
            let attributeToDisable = formContext.getAttribute("jarvis_contact").controls.get(0);
            attributeToDisable.setDisabled(false);
            //disable approver name
            let approvalToDisable = formContext.getAttribute("jarvis_gopapproval").controls.get(0);
            approvalToDisable.setDisabled(false);
        }
        else {
            //disable approver name
            let attributeToDisable = formContext.getAttribute("jarvis_contact").controls.get(0);
            attributeToDisable.setDisabled(true);
            //disable approver name
            let approvalToDisable = formContext.getAttribute("jarvis_gopapproval").controls.get(0);
            approvalToDisable.setDisabled(true);
        }

    }
    else {
        var attribute = formContext.getAttribute("jarvis_dealer");
        attribute.controls.forEach(control => control.removePreSearch(filterCustomerAccounts));
        formContext.getControl("jarvis_gopapproval")?.removeOption(334030002);
    }

}
function LockRecordOnLoad(executionContext) {
    let formContext = executionContext.getFormContext();
    let fieldControl = formContext.getControl("statuscode");
    let gopControl = formContext.getControl("jarvis_gopapproval");
    var paymentType = executionContext.getFormContext().getAttribute("jarvis_paymenttype")?.getValue();
    let headerfieldControl = formContext.getControl("header_statuscode");
    let formType = formContext.ui.getFormType();
    const allControls = formContext.ui.controls.get();
    if (fieldControl && gopControl && headerfieldControl) {
        let statusCode = formContext.getAttribute("statuscode")?.getValue();
        let gopApproval = formContext.getAttribute("jarvis_gopapproval")?.getValue();
        if (formType != 1) {
            if (gopApproval != null && statusCode != null) {
                if (gopApproval == 334030001 && statusCode === 20) {
                    allControls.forEach((control) => {
                        // Disable the control to lock the field
                        control.setDisabled(true);
                    });
                }
                //// 643487 - Added Failed
                else if (gopApproval == 334030001 && (statusCode === 1 || statusCode === 30 || statusCode === 334030002)) {
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
                ////208314 Lock all fields if approve = No and not lock approved and contact
                else if (gopApproval == 334030000) {
                    allControls.forEach((control) => {
                        // Disable the control to lock the field
                        var controlName = control.getName();
                        if (controlName !== "jarvis_gopapproval" && controlName !== "jarvis_contact") {
                            control.setDisabled(true);
                        }
                        else {
                            control.setDisabled(false);
                        }
                    });
                }


            }

            //Payment Type is Credit Card
            if ((paymentType === 334030002 && gopApproval != 334030000)) {
                var fieldArr = ["jarvis_gopapproval", "jarvis_contact"];
                lockFields(formContext, fieldArr, true);
            }

            //Payment Type is Credit Card and goApproval pending, lock approver name
            //  if ((paymentType === 334030002 && gopApproval === 334030000)) {
            //   var fieldArr = ["jarvis_contact"];
            //   lockFields(formContext, fieldArr, true);
            // }

        }
        else if (formType == 1) {

            if (gopApproval != null) {
                fieldControl.setDisabled(true);
                headerfieldControl.setDisabled(true);
                ////208314 Lock all fields if approve = No and not lock approved and contact
                if (paymentType != 334030002) {
                    gopControl.setDisabled(false);
                }
                else {
                    gopControl.setDisabled(true);
                }

            }
            else {
                fieldControl.setDisabled(false);
                headerfieldControl.setDisabled(false);
            }
        }

    }

}
function LockFieldOnSave(executionContext) {
    let formContext = executionContext.getFormContext();
    let fieldControl = formContext.getControl("statuscode");
    let gopControl = formContext.getControl("jarvis_gopapproval");
    let headerfieldControl = formContext.getControl("header_statuscode");
    let formType = formContext.ui.getFormType();
    var requestType = formContext.getAttribute("jarvis_requesttype")?.getValue();
    const allControls = formContext.ui.controls.get();
    if (fieldControl && gopControl && headerfieldControl) {
        let statusCode = formContext.getAttribute("statuscode")?.getValue();
        let gopApproval = formContext.getAttribute("jarvis_gopapproval")?.getValue();
        var relatedGopfield = formContext.getAttribute("jarvis_relatedgop")?.getValue();
        //create
        if (formType == 1) {
            var relatedGopfield = formContext.getAttribute("jarvis_relatedgop")?.getValue();
            var jarvis_requesttype = formContext.getAttribute("jarvis_requesttype")?.getValue();
            var jarvis_paymenttype = formContext.getAttribute("jarvis_paymenttype")?.getValue();
            var jarvis_dealer = formContext.getAttribute("jarvis_dealer")?.getValue();
            var jarvis_repairingdealer = formContext.getAttribute("jarvis_repairingdealer")?.getValue();
            var jarvis_goplimitin = formContext.getAttribute("jarvis_goplimitin")?.getValue();
            var jarvis_gopincurrency = formContext.getAttribute("jarvis_gopincurrency")?.getValue();
            var jarvis_goplimitout = formContext.getAttribute("jarvis_goplimitout")?.getValue();
            var jarvis_gopoutcurrency = formContext.getAttribute("jarvis_gopoutcurrency")?.getValue();
            var mandatorycheckfailed = false;


            // Check mandatory fields
            if (!jarvis_requesttype || !jarvis_paymenttype || !jarvis_dealer) {
                mandatorycheckfailed = true;
            }
            if (jarvis_requesttype && (requestType == 334030002 || (jarvis_paymenttype == 334030002))) {
                if ((!jarvis_goplimitout || jarvis_goplimitout == 0) || !jarvis_gopoutcurrency) {
                    mandatorycheckfailed = true;

                }
                if (requestType == 334030002 && !jarvis_repairingdealer) {
                    mandatorycheckfailed = true;
                }
            }
            else {
                if ((!jarvis_goplimitin || jarvis_goplimitin == 0) || !jarvis_gopincurrency) {
                    mandatorycheckfailed = true;
                }
            }
            if (mandatorycheckfailed) {
                Xrm.Navigation.openAlertDialog("Please enter the required fields.");
                executionContext.getEventArgs().preventDefault();
            }
            else {
                if (gopApproval != null && statusCode != null) {
                    if (gopApproval == 334030001 && statusCode === 20) {
                        allControls.forEach((control) => {
                            // Disable the control to lock the field
                            control.setDisabled(true);
                        });
                    }
                    else if (gopApproval == 334030001 && (statusCode === 30 || statusCode === 334030002)) {
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
                    else if (gopApproval == 334030001 && statusCode === 1) {
                        allControls.forEach((control) => {
                            // Get the field name of the current control
                            const fieldName = control.getName();
                            if (fieldName !== "statuscode" && fieldName !== "header_statuscode" && fieldName !== "jarvis_totallimitin" &&
                                fieldName !== "jarvis_totallimitincurrency" && fieldName !== "jarvis_totallimitout" && fieldName !== "jarvis_totallimitoutcurrency") {
                                // Disable the control to lock the field
                                control.setDisabled(false);
                            }
                            else {
                                control.setDisabled(true);
                            }
                        });
                    }
                    ////208314 Lock all fields if approve = No and not lock approved and contact
                    else if (gopApproval == 334030000) {
                        allControls.forEach((control) => {
                            // Disable the control to lock the field
                            var controlName = control.getName();
                            if (controlName !== "jarvis_gopapproval" && controlName !== "jarvis_contact") {
                                control.setDisabled(true);
                            }
                            else {
                                control.setDisabled(false);
                            }
                        });
                    }
                }
            }
        }
        else if (formType != 1) {
            if (gopApproval != null && statusCode != null) {
                if (gopApproval == 334030001 && statusCode === 20) {
                    allControls.forEach((control) => {
                        // Disable the control to lock the field
                        control.setDisabled(true);
                    });
                }
                else if (gopApproval == 334030001 && statusCode === 1) {
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
                else if (gopApproval == 334030001 && statusCode === 30) {
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
                ////208314 Lock all fields if approve = No and not lock approved and contact
                else if (gopApproval == 334030000 && relatedGopfield == null) {
                    allControls.forEach((control) => {
                        // Disable the control to lock the field
                        var controlName = control.getName();
                        if (controlName !== "jarvis_gopapproval" && controlName !== "jarvis_contact") {
                            control.setDisabled(true);
                        }
                        else {
                            control.setDisabled(false);
                        }
                    });
                }
            }
        }

    }
}
function LockGopGridFields(executionContext) {
    let formContext = executionContext.getFormContext();
    if (formContext) {
        ////208314 Lock all fields if approve = No and not lock approved and contact
        //let arrFields = ["jarvis_totallimitincurrency", "jarvis_totallimitoutcurrency", "jarvis_totallimitin", "jarvis_totallimitout", "jarvis_goplimitout"];
        let arrUnlockFields = ["jarvis_gopapproval", "jarvis_contact"];
        let objEntity = formContext.data.entity;
        /*        objEntity.attributes.forEach(function (attribute, i) {
                    if (arrUnlockFields.indexOf(attribute.getName()) > -1) {
                        let attributeToEnable = attribute.controls.get(0);
                        attributeToEnable.setDisabled(false);
                    }
                    else {
                        let attributeToDisable = attribute.controls.get(0);
                        attributeToDisable.setDisabled(true);
                    }
                }
                )*/
        var paymentType = formContext.getAttribute("jarvis_paymenttype")?.getValue();
        if (paymentType == 334030002) // Credit Card
        {
            var gopApproval = formContext.getAttribute("jarvis_gopapproval")?.getValue();

            if (gopApproval !== null && gopApproval == 334030000) {
                formContext.getControl("jarvis_gopapproval")?.removeOption(334030001);
                formContext.getControl("jarvis_gopapproval")?.removeOption(334030002);
                formContext.getControl("jarvis_gopapproval")?.addOption({ text: "Declined", value: 334030002 });
                objEntity.attributes.forEach(function (attribute, i) {
                    if (arrUnlockFields.indexOf(attribute.getName()) > -1) {
                        let attributeToEnable = attribute.controls.get(0);
                        attributeToEnable.setDisabled(false);
                    }
                });
            }
            else {
                objEntity.attributes.forEach(function (attribute, i) {
                    if (arrUnlockFields.indexOf(attribute.getName()) > -1) {
                        let attributeToEnable = attribute.controls.get(0);
                        attributeToEnable.setDisabled(true);
                    }
                });
            }


        }
        else {
            formContext.getControl("jarvis_gopapproval")?.removeOption(334030002);
        }
    };

}
//let isBlacklistedCustomer = false;
function OnLoadGetBlacklistedDealer(executionContext) {
    let dealer = null;
    let formContext = executionContext.getFormContext();

    if (formContext.getControl("jarvis_dealer") !== null && formContext.getAttribute("jarvis_dealer").getValue() !== null && formContext.ui.getFormType() == 2) {
        dealer = formContext.getAttribute("jarvis_dealer").getValue();
        Xrm.WebApi.retrieveRecord("account", dealer[0].id, "?$select=name,jarvis_blacklisted").then(
            function success(result) {
                var accountid = result["accountid"];
                dealerName = result["name"];
                isBlacklistedCustomer = result["jarvis_blacklisted"];

            }

        );
    }
}
function OnSaveBlacklistDealerDiplay(executionContext) {
    let formContext = executionContext.getFormContext();
    if (isBlacklistedCustomer && formContext.ui.getFormType() === 2) {
        executionContext.getEventArgs().preventDefault();
        var alertStrings = { confirmButtonLabel: "Yes", text: "You cannot save the GOP because the Home Dealer is blacklisted, please change the GOP Dealer.", title: "Dealer Blacklisted" };
        var alertOptions = { height: 120, width: 260 };
        Xrm.Navigation.openAlertDialog(alertStrings, alertOptions).then(
            function (success) {
            },
            function (error) {
            }
        );
    }
    /* else if (formContext.ui.getFormType() === 1)
     {
         let customer = null;
 
         if (formContext.getControl("jarvis_dealer") !== null && formContext.getAttribute("jarvis_dealer").getValue() !== null) {
             customer = formContext.getAttribute("jarvis_dealer").getValue();
             if (!isPreventRequired) {
                 executionContext.getEventArgs().preventDefault();
             }
             Xrm.WebApi.retrieveRecord("account", customer[0].id, "?$select=name,jarvis_blacklisted").then(
                 function success(result) {
                     var accountid = result["accountid"];
                     dealerName = result["name"];
                     isBlacklistedCustomer = result["jarvis_blacklisted"];
                     if (isBlacklistedCustomer) {
                         var alertStrings = { confirmButtonLabel: "Yes", text: "You cannot save the GOP because the Home Dealer is blacklisted, please change the GOP Dealer.", title: "Dealer Blacklisted" };
                         Xrm.Navigation.openAlertDialog(alertStrings, alertOptions).then(
                             function (success) {
                             },
                             function (error) {
                             }
                         );
 
                     }
                     else {
                         isPreventRequired = true;
                         formContext.data.save().then(
 function () {
 //alert("Successfully Saved!");
 },
 function () {
 //alert("Failed while saving!");
 });
                     }
                 }
 
             );
 
         }
 
     }*/

}

function exchangeRate(basecurrency, basecurrencyValue, targetcurrency, targetcurrencyValue) {
    var baseCurrencyName = basecurrencyValue;
    if (basecurrencyValue == "") {
        baseCurrencyName = basecurrency[0]?.name;
        var basecurrencyValue = basecurrency[0]?.id;
        if (basecurrencyValue != null) {
            basecurrencyValue = basecurrencyValue.replace('{', '').replace('}', '');
        }
    }
    var targetcurrencyName = targetcurrencyValue;
    if (targetcurrencyValue == "") {
        targetcurrencyName = targetcurrency[0]?.name;
        targetcurrencyValue = targetcurrency[0]?.id;
        if (targetcurrencyValue != null) {
            targetcurrencyValue = targetcurrencyValue.replace('{', '').replace('}', '');
        }
    }
    Xrm.WebApi.retrieveMultipleRecords("jarvis_exchangerate", "?$select=jarvis_value&$filter=(_jarvis_basecurrency_value eq " + basecurrencyValue + " and _jarvis_countercurrency_value eq " + targetcurrencyValue + ")").then(
        function success(result) {
            if (result.entities.length > 0) {
                for (let row of result.entities) {
                    if (row["jarvis_value"]) {
                        var conversionrate = parseFloat(row["jarvis_value"]);
                        return conversionrate;
                    }
                    else {
                        formContext.ui.setFormNotification("Conversion rate not available for " + baseCurrencyName + " and " + targetcurrencyName, "WARNING", "WAR_BaseCurrencyValidation");
                        return 1;
                    }
                }
            }
            else {
                formContext.ui.setFormNotification("Conversion rate not available for " + baseCurrencyName + " and " + targetcurrencyName, "WARNING", "WAR_BaseCurrencyValidation");
                return 1;
            }
        },
        function (error) {
            return 1;
        }
    );

}


function calculateBookingAmount(executionContext, caseobj) {
    var formContext = executionContext.getFormContext();
    var gopLimitOut = formContext.getAttribute("jarvis_goplimitout")?.getValue();
    var gopLimitOutCurrency = formContext.getAttribute("jarvis_gopoutcurrency")?.getValue();
    var bookingCurrency = formContext.getAttribute("jarvis_creditcardincurrency")?.getValue();
    var VATpercent = formContext.getAttribute("jarvis_vat")?.getValue();
    var serviceLineid = "";
    var calculatedAmount = 0;
    var paymentType = formContext.getAttribute("jarvis_paymenttype")?.getValue();

    var caseId = "";
    var exchangerate = 1;
    /*    if (paymentType == 334030002) // Credit Card
        {
    
            var field = executionContext.getFormContext().getAttribute(GopCntrls.Fields.Dealer);
            if (field != null && field.getValue() != null) {
                var record_id = field.getValue()[0].id;
                record_id = record_id.replace('{', '').replace('}', '');
                if (gopLimitOut != null && VATpercent != null && bookingCurrency != null && gopLimitOutCurrency != null) {
                    caseId = formContext.getAttribute("jarvis_incident").getValue()[0].id;
                    caseId = caseId.replace('{', '').replace('}', '');
                    var basecurrencyValue = gopLimitOutCurrency[0]?.id;
                    if (basecurrencyValue != null) {
                        basecurrencyValue = basecurrencyValue.replace('{', '').replace('}', '');
                    }
                    var targetcurrencyValue = bookingCurrency[0]?.id;
                    if (targetcurrencyValue != null) {
                        targetcurrencyValue = targetcurrencyValue.replace('{', '').replace('}', '');
                    }
                    var matchedstaircasefee = 45;
                    var baseStairCaseCurrency = targetcurrencyValue;
                    Xrm.WebApi.retrieveMultipleRecords("jarvis_gop", "?$select=_jarvis_dealer_value,jarvis_paymenttype,jarvis_gopapproval&$filter=(_jarvis_incident_value eq " + caseId + "  and statecode eq 0)&$orderby= modifiedon asc").then(
                        function success(result) {
                            if (result !== null && result.entities.length > 0) {
    
                                for (let row of result.entities) {
                                    if (row["_jarvis_dealer_value"] && row["_jarvis_dealer_value"].toUpperCase() == record_id.toUpperCase()) {
                                        isStairCasefeeIncluded = true;
    
                                    }
                                    if (row["jarvis_paymenttype"] && row["jarvis_paymenttype"] == 334030002 && row["jarvis_gopapproval"] == 334030001) {
                                        isvolvoPayFeeIncluded = true;
    
                                    }
    
                                }
                            }
                            //});
                            Xrm.WebApi.retrieveRecord("incident", caseId, "?$select=jarvis_totalbookingamountinclvat,_jarvis_totalcreditcardpaymentamountcurrency_value,_jarvis_caseserviceline_value&$expand=customerid_account($select=_jarvis_language_value)").then(
                                function success(result) {
                                    serviceLineid = result["_jarvis_caseserviceline_value"];
                                    var volvopayExchangerate = 1;
                                    var stiarcasefeeExchangeRate = 1;
                                    var bookingcuurencyexchange =1;
                                    var approvedcreditCardAmount = 0;
                                    var approvedcreditCardAmountCurrency = targetcurrencyValue;
                                    if (result["jarvis_totalbookingamountinclvat"]) {
                                        approvedcreditCardAmount = result["jarvis_totalbookingamountinclvat"];
                                    }
                                    if (result["_jarvis_totalcreditcardpaymentamountcurrency_value"]) {
                                        approvedcreditCardAmountCurrency = result["_jarvis_totalcreditcardpaymentamountcurrency_value"];
                                    }
                                    // 1. Add GOp Limit Out in Booking Currency
                                    // var exchangerate = exchangeRate(gopLimitOutCurrency, "", bookingCurrency, "");
                                    Xrm.WebApi.retrieveMultipleRecords("jarvis_exchangerate", "?$select=jarvis_value&$filter=(_jarvis_basecurrency_value eq " + basecurrencyValue + " and _jarvis_countercurrency_value eq " + targetcurrencyValue + ")").then(
                                        function success(exchangelist) {
                                            if (exchangelist.entities.length > 0 && exchangelist.entities[0] != null && exchangelist.entities[0]["jarvis_value"]) {
                                                exchangerate = exchangelist.entities[0]["jarvis_value"];
                                                calculatedAmount = gopLimitOut * exchangerate;
                                            }
                                            //});
    
                                            // GetStairCase Fee and Volvo pay fee from service Line
                                            Xrm.WebApi.retrieveRecord("jarvis_serviceline", serviceLineid, "?$select=jarvis_servicefee,jarvis_volvopayfee,_jarvis_volvopayfeecurrency_value,_transactioncurrencyid_value").then(
                                                function success(serviceLineobj) {
                                                    if (serviceLineobj["_jarvis_volvopayfeecurrency_value"]) {
                                                        // 2. Add Volvo fee
                                                        if ( serviceLineobj["jarvis_volvopayfee"] != null && serviceLineobj["_jarvis_volvopayfeecurrency_value"] != null) {
                                                            //exchangerate = exchangeRate("", serviceLineobj["_transactioncurrencyid_value"], bookingCurrency, "");
                                                            Xrm.WebApi.retrieveMultipleRecords("jarvis_exchangerate", "?$select=jarvis_value&$filter=(_jarvis_basecurrency_value eq " + serviceLineobj["_jarvis_volvopayfeecurrency_value"] + " and _jarvis_countercurrency_value eq " + targetcurrencyValue + ")").then(
                                                                function success(exchangelist) {
                                                                    if (serviceLineobj["_jarvis_volvopayfeecurrency_value"].toUpperCase() != targetcurrencyValue.toUpperCase() && exchangelist.entities.length > 0 && exchangelist.entities[0] != null && exchangelist.entities[0]["jarvis_value"]) {
                                                                        volvopayExchangerate = exchangelist.entities[0]["jarvis_value"];
                                                                        
                                                                    }
                                                                    // 2. Add Staircase fee
                                                                    if ( dealerCountry != null && serviceLineid != null) {
                                                                        var fetchXml = `<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                                                                  <entity name='jarvis_staircasefee'>
                                                                                    <attribute name='jarvis_staircasefeeid' />
                                                                                    <attribute name='jarvis_name' />
                                                                                    <attribute name='createdon' />
                                                                                    <attribute name='jarvis_staircasefee' />
                                                                                    <attribute name='transactioncurrencyid' />
                                                                                    <order attribute='jarvis_name' descending='false' />
                                                                                    <filter type='and'>
                                                                                      <condition attribute='jarvis_country' operator='eq' uitype='jarvis_country' value='{`+ dealerCountry +`}' />
                                                                                      <condition attribute='jarvis_serviceline' operator='eq' uitype='jarvis_serviceline' value='{`+ serviceLineid +`}' />
                                                                                      <condition attribute='statecode' operator='eq' value='0' />
                                                                                    </filter>
                                                                                  </entity>
                                                                                </fetch>`;
    
                                                                        fetchXml = "?fetchXml=" + encodeURIComponent(fetchXml);
    
                                                                        Xrm.WebApi.retrieveMultipleRecords("jarvis_staircasefee", fetchXml).then(
                                                                            function success(result) {
                                                                                if (result.entities.length > 0 && result.entities[0]) {
                                                                                    matchedstaircasefee = result.entities[0]["jarvis_staircasefee"];
                                                                                    baseStairCaseCurrency = result.entities[0]["transactioncurrencyid"];
                                                                                }
                                                                                Xrm.WebApi.retrieveMultipleRecords("jarvis_exchangerate", "?$select=jarvis_value&$filter=(_jarvis_basecurrency_value eq " + baseStairCaseCurrency + " and _jarvis_countercurrency_value eq " + targetcurrencyValue + ")").then(
                                                                                    function success(exchangelist) {
                                                                                        if (baseStairCaseCurrency.toUpperCase() != targetcurrencyValue.toUpperCase() && exchangelist.entities.length > 0 && exchangelist.entities[0] != null && exchangelist.entities[0]["jarvis_value"]) {
                                                                                            stiarcasefeeExchangeRate = exchangelist.entities[0]["jarvis_value"];
                                                                                            
                                                                                        }
                                                                                        // 3. Add the vat %
                                                                                        VATpercent = VATpercent[0]?.id;
                                                                                        if (VATpercent != null) {
                                                                                            VATpercent = VATpercent.replace('{', '').replace('}', '');
                                                                                            Xrm.WebApi.retrieveRecord("jarvis_vat", VATpercent, "?$select=jarvis_vat").then(
                                                                                                function success(vatobj) {
                                                                                                    if (vatobj["jarvis_vat"]) {
                                                                                                        // Add Vat fee
                                                                                                        if (vatobj["jarvis_vat"] != null) 
                                                                                                        {
                                                                                                            if(approvedcreditCardAmountCurrency.toUpperCase() != targetcurrencyValue.toUpperCase())
                                                                                                            {// Different currency
                                                                                                            //---Reduce already approved booked amount 
                                                                                                            Xrm.WebApi.retrieveMultipleRecords("jarvis_exchangerate", "?$select=jarvis_value&$filter=(_jarvis_basecurrency_value eq " + approvedcreditCardAmountCurrency + " and _jarvis_countercurrency_value eq " + targetcurrencyValue + ")").then(
                                                                                                                function success(exchangelist) {
                                                                                                                    if (approvedcreditCardAmountCurrency.toUpperCase() != targetcurrencyValue.toUpperCase() && exchangelist.entities.length > 0 && exchangelist.entities[0] != null && exchangelist.entities[0]["jarvis_value"]) {
                                                                                                                         bookingcuurencyexchange = exchangelist.entities[0]["jarvis_value"];
                                                                                                                        // Reduce Previous Approved booking amount
                                                                                                                        if (approvedcreditCardAmount != null && approvedcreditCardAmount > 0) {
                                                                                                                            calculatedAmount = calculatedAmount - (parseFloat(approvedcreditCardAmount) * bookingcuurencyexchange);
                                                                                                                            if (calculatedAmount < 0) {
                                                                                                                                Xrm.Navigation.openAlertDialog("The Limit Out provided is not sufficient , please a provide amount above the aggregated booking amount of " + approvedcreditCardAmount.toString());
                                                                                                                                return;
                                                                                                                            }
                                                                                                                        }
                                                                                                                    }
                                                                                                                    // Add stair case fee
                                                                                                                    if (!isStairCasefeeIncluded && matchedstaircasefee) {
                                                                                                                        calculatedAmount = calculatedAmount + (parseFloat(matchedstaircasefee) * stiarcasefeeExchangeRate);
                                                                                                                    }
                                                                                                                    // Add Volvo Pay fee
                                                                                                                    if (!isvolvoPayFeeIncluded && serviceLineobj["jarvis_volvopayfee"]) {
                                                                                                                        calculatedAmount = calculatedAmount + (parseFloat(serviceLineobj["jarvis_volvopayfee"]) * volvopayExchangerate);
                                                                                                                    }
                                                                                                                    calculatedAmount = calculatedAmount * (1 + (parseFloat(vatobj["jarvis_vat"]) * 0.01));
                                                                                                                    formContext.getAttribute("jarvis_creditcardgopinbooking").setValue(calculatedAmount);
                                                                                                                });
                                                                                                            }
                                                                                                            else
                                                                                                            {   // Same currency 
                                                                                                                if (approvedcreditCardAmount != null && approvedcreditCardAmount > 0) {
                                                                                                                    calculatedAmount = calculatedAmount - (parseFloat(approvedcreditCardAmount) * bookingcuurencyexchange);
                                                                                                                    if (calculatedAmount < 0) {
                                                                                                                        Xrm.Navigation.openAlertDialog("The Limit Out provided is not sufficient , please provide amount above the aggregated booking amount of " + approvedcreditCardAmount.toString());
                                                                                                                        return;
                                                                                                                    }
                                                                                                                }
                                                                                                                // Add stair case fee
                                                                                                                if (!isStairCasefeeIncluded && matchedstaircasefee) {
                                                                                                                    calculatedAmount = calculatedAmount + (parseFloat(matchedstaircasefee) * stiarcasefeeExchangeRate);
                                                                                                                }
                                                                                                                // Add Volvo Pay fee
                                                                                                                if (!isvolvoPayFeeIncluded && serviceLineobj["jarvis_volvopayfee"]) {
                                                                                                                    calculatedAmount = calculatedAmount + (parseFloat(serviceLineobj["jarvis_volvopayfee"]) * volvopayExchangerate);
                                                                                                                }
                                                                                                                calculatedAmount = calculatedAmount * (1 + (parseFloat(vatobj["jarvis_vat"]) * 0.01));
                                                                                                                formContext.getAttribute("jarvis_creditcardgopinbooking").setValue(calculatedAmount);
    
                                                                                                            }
                                                                                                        }
                                                                                                    }
                                                                                                },
                                                                                                function (error) {
                                                                                                    console.log(error.message);
                                                                                                    // handle error conditions
                                                                                                }
                                                                                            );
                                                                                        }
                                                                                    }); // 3. Add the vat %
                                                                            });// End fetch StaircaseFee
                                                                    }
                                                                    else {
                                                                        // 3. Add the vat %
                                                                        VATpercent = VATpercent[0]?.id;
                                                                        if (VATpercent != null) {
                                                                            VATpercent = VATpercent.replace('{', '').replace('}', '');
                                                                            Xrm.WebApi.retrieveRecord("jarvis_vat", VATpercent, "?$select=jarvis_vat").then(
                                                                                function success(vatobj) {
                                                                                    if (vatobj["jarvis_vat"]) {
                                                                                        // Add Vat fee
                                                                                        if (vatobj["jarvis_vat"] != null) {
                                                                                            //---Reduce already approved booked amount 
                                                                                            if(approvedcreditCardAmountCurrency.toUpperCase() != targetcurrencyValue.toUpperCase())
                                                                                            {   // Get exchange value if both have different currencies
                                                                                                Xrm.WebApi.retrieveMultipleRecords("jarvis_exchangerate", "?$select=jarvis_value&$filter=(_jarvis_basecurrency_value eq " + approvedcreditCardAmountCurrency + " and _jarvis_countercurrency_value eq " + targetcurrencyValue + ")").then(
                                                                                                    function success(exchangelist) {
                                                                                                        if ( exchangelist.entities.length > 0 && exchangelist.entities[0] != null && exchangelist.entities[0]["jarvis_value"]) {
                                                                                                            var bookingcuurencyexchange = exchangelist.entities[0]["jarvis_value"];
                                                                                                            // Reduce Previous Approved booking amount
                                                                                                            if (approvedcreditCardAmount != null && approvedcreditCardAmount > 0) {
                                                                                                                calculatedAmount = calculatedAmount - (parseFloat(approvedcreditCardAmount) * bookingcuurencyexchange);
                                                                                                                if (calculatedAmount < 0) {
                                                                                                                    Xrm.Navigation.openAlertDialog("The Limit Out provided is not sufficient , please provide amount above the aggregated booking amount of " + approvedcreditCardAmount.toString());
                                                                                                                    return;
                                                                                                                }
                                                                                                            }
                                                                                                        }
                                                                                                        calculatedAmount = calculatedAmount * (1 + (parseFloat(vatobj["jarvis_vat"]) * 0.01));
                                                                                                        formContext.getAttribute("jarvis_creditcardgopinbooking").setValue(calculatedAmount);
                                                                                                    }
                                                                                                );
                                                                                            }
                                                                                            else
                                                                                            {
                                                                                                if (approvedcreditCardAmount != null && approvedcreditCardAmount > 0) {
                                                                                                    calculatedAmount = calculatedAmount - (parseFloat(approvedcreditCardAmount) * bookingcuurencyexchange);
                                                                                                    if (calculatedAmount < 0) {
                                                                                                        Xrm.Navigation.openAlertDialog("The Limit Out provided is not sufficient , please provide amount above the aggregated booking amount of " + approvedcreditCardAmount.toString());
                                                                                                        return;
                                                                                                    }
                                                                                                }
                                                                                                calculatedAmount = calculatedAmount * (1 + (parseFloat(vatobj["jarvis_vat"]) * 0.01));
                                                                                                formContext.getAttribute("jarvis_creditcardgopinbooking").setValue(calculatedAmount);
                                                                                            }
                                                                                        }
                                                                                    }
                                                                                },
                                                                                function (error) {
                                                                                    console.log(error.message);
                                                                                    // handle error conditions
                                                                                }
                                                                            );
                                                                        }
                                                                    }
    
    
                                                                }); // 2. Add Volvo pay fee
    
                                                        }
                                                        else {
                                                            // 3. Add the vat %
                                                            VATpercent = VATpercent[0]?.id;
                                                            if (VATpercent != null) {
                                                                VATpercent = VATpercent.replace('{', '').replace('}', '');
                                                                Xrm.WebApi.retrieveRecord("jarvis_vat", VATpercent, "?$select=jarvis_vat").then(
                                                                    function success(vatobj) {
                                                                        if (vatobj["jarvis_vat"]) {
                                                                            // Add Vat fee
                                                                            if (vatobj["jarvis_vat"] != null) {
                                                                                //---Reduce already approved booked amount 
                                                                                if(approvedcreditCardAmountCurrency.toUpperCase() != targetcurrencyValue.toUpperCase())
                                                                                {   // Get exchange value if both have different currencies
                                                                                    Xrm.WebApi.retrieveMultipleRecords("jarvis_exchangerate", "?$select=jarvis_value&$filter=(_jarvis_basecurrency_value eq " + approvedcreditCardAmountCurrency + " and _jarvis_countercurrency_value eq " + targetcurrencyValue + ")").then(
                                                                                        function success(exchangelist) {
                                                                                            if ( exchangelist.entities.length > 0 && exchangelist.entities[0] != null && exchangelist.entities[0]["jarvis_value"]) {
                                                                                                var bookingcuurencyexchange = exchangelist.entities[0]["jarvis_value"];
                                                                                                // Reduce Previous Approved booking amount
                                                                                                if (approvedcreditCardAmount != null && approvedcreditCardAmount > 0) {
                                                                                                    calculatedAmount = calculatedAmount - (parseFloat(approvedcreditCardAmount) * bookingcuurencyexchange);
                                                                                                    if (calculatedAmount < 0) {
                                                                                                        Xrm.Navigation.openAlertDialog("The Limit Out provided is not sufficient , please a provide amount above the aggregated booking amount of " + approvedcreditCardAmount.toString());
                                                                                                        return;
                                                                                                    }
                                                                                                }
                                                                                            }
                                                                                            calculatedAmount = calculatedAmount * (1 + (parseFloat(vatobj["jarvis_vat"]) * 0.01));
                                                                                            formContext.getAttribute("jarvis_creditcardgopinbooking").setValue(calculatedAmount);
                                                                                        }
                                                                                    );
                                                                                }
                                                                                else
                                                                                {
                                                                                    if (approvedcreditCardAmount != null && approvedcreditCardAmount > 0) {
                                                                                        calculatedAmount = calculatedAmount - (parseFloat(approvedcreditCardAmount) * bookingcuurencyexchange);
                                                                                        if (calculatedAmount < 0) {
                                                                                            Xrm.Navigation.openAlertDialog("The Limit Out provided is not sufficient , please a provide amount above the aggregated booking amount of " + approvedcreditCardAmount.toString());
                                                                                            return;
                                                                                        }
                                                                                    }
                                                                                    calculatedAmount = calculatedAmount * (1 + (parseFloat(vatobj["jarvis_vat"]) * 0.01));
                                                                                    formContext.getAttribute("jarvis_creditcardgopinbooking").setValue(calculatedAmount);
                                                                                }
                                                                            }
                                                                        }
                                                                    },
                                                                    function (error) {
                                                                        console.log(error.message);
                                                                        // handle error conditions
                                                                    }
                                                                );
                                                            }
                                                        }
                                                    }
                                                });
                                        }); //1. Add GOp Limit Out in Booking Currency
    
    
                                },
                                function (error) {
                                }
                            );
    
                        });
    
                    formContext.ui.clearFormNotification("BOOKINGVAT");
                }
                else {
                    formContext.ui.setFormNotification("Please enter valid Limitout,LimitOut Currency, Case: Service Line, Booking Currency and VAT for Booking amount calculation.", "WARNING", "BOOKINGVAT");
                }
            }
    
        }*/
}


function onPaymentTypeChange(executionContext) {
    var formContext = executionContext.getFormContext();
    var paymentType = executionContext.getFormContext().getAttribute("jarvis_paymenttype")?.getValue();
    var requestType = executionContext.getFormContext().getAttribute("jarvis_requesttype")?.getValue();
    var fetchXml = "";
    var formType = formContext.ui.getFormType();
    if (paymentType == 334030002) // Credit Card
    {
        var attribute = formContext.getAttribute("jarvis_dealer")
        attribute.controls.forEach(control => control.addPreSearch(filterCustomerAccounts))
        var gopApproval = formContext.getAttribute("jarvis_gopapproval")?.getValue();
        //Add decline option
        if (gopApproval !== null && gopApproval != 334030002) {
            formContext.getControl("jarvis_gopapproval")?.removeOption(334030002);
            formContext.getControl("jarvis_gopapproval")?.addOption({ text: "Declined", value: 334030002 });
        }
        if (gopApproval !== null && gopApproval != 334030001) {
            formContext.getControl("jarvis_gopapproval")?.removeOption(334030001);
        }

        //disable approver name
        let attributeToDisable = formContext.getAttribute("jarvis_contact").controls.get(0);
        attributeToDisable.setDisabled(true);

        formContext.getAttribute("jarvis_dealer").setValue(null);
        var caseId = executionContext.getFormContext().getAttribute("jarvis_incident").getValue()[0].id;
        caseId = caseId.replace('{', '').replace('}', '');
        // Set The limit Out Currency to Null
        if (requestType != 334030002) {
            formContext.getAttribute("jarvis_gopoutcurrency").setValue(null);
            formContext.getAttribute("jarvis_totallimitoutcurrency").setValue(null);
            formContext.getAttribute("jarvis_totallimitincurrency").setValue(null);
        }
        if (caseId !== null) {
            Xrm.WebApi.retrieveRecord("incident", caseId, "?$select=_jarvis_country_value,_customerid_value,jarvis_registrationnumber,_jarvis_brand_value,_jarvis_onetimecustomercountry_value,_jarvis_caseserviceline_value&$expand=customerid_account($select=_jarvis_language_value)").then(
                function success(result) {
                    // Columns
                    var incidentid = result["incidentid"]; // Guid
                    var jarvis_country = result["_jarvis_country_value"]; // Lookup
                    var jarvis_country_formatted = result["_jarvis_country_value@OData.Community.Display.V1.FormattedValue"];
                    var jarvis_country_lookuplogicalname = result["_jarvis_country_value@Microsoft.Dynamics.CRM.lookuplogicalname"];
                    var customerid = result["_customerid_value"]; // Customer
                    var customerid_formatted = result["_customerid_value@OData.Community.Display.V1.FormattedValue"];
                    var customerid_lookuplogicalname = result["_customerid_value@Microsoft.Dynamics.CRM.lookuplogicalname"];
                    var jarvis_registrationnumber = result["jarvis_registrationnumber"]; // Text
                    var messageToDisplay = jarvis_registrationnumber + " " + jarvis_country_formatted;
                    formContext.getAttribute("jarvis_creditcardmessage").setValue(messageToDisplay);
                    var brand = result["_jarvis_brand_value"]; // Brand
                    var CustomerCountry = result["_jarvis_onetimecustomercountry_value"];
                    var serviceLine = result["_jarvis_caseserviceline_value"];

                    // Many To One Relationships
                    if (result.hasOwnProperty("customerid_account") && result["customerid_account"] !== null) {
                        var custLang = new Array();
                        custLang[0] = new Object();
                        custLang[0].id = result["customerid_account"]["_jarvis_language_value"];
                        custLang[0].name = result["customerid_account"]["_jarvis_language_value@OData.Community.Display.V1.FormattedValue"];
                        custLang[0].entityType = result["customerid_account"]["_jarvis_language_value@Microsoft.Dynamics.CRM.lookuplogicalname"];
                        if (custLang[0].id !== null) {
                            formContext.getAttribute("jarvis_creditcardlanguage").setValue(custLang);
                        }
                        else {
                            Xrm.WebApi.retrieveMultipleRecords("jarvis_language", "?$select=jarvis_languageid,jarvis_name&$filter=jarvis_name eq 'English'").then(
                                function success(results) {
                                    if (results.entities.length > 0) {
                                        var result = results.entities[0];

                                        var jarvis_languageid = result["jarvis_languageid"]; // Guid
                                        var jarvis_name = result["jarvis_name"]; // Text
                                        var custLangEng = new Array();
                                        custLangEng[0] = new Object();
                                        custLangEng[0].id = result["jarvis_languageid"];
                                        custLangEng[0].name = result["jarvis_name"];
                                        custLangEng[0].entityType = "jarvis_language";
                                        formContext.getAttribute("jarvis_creditcardlanguage").setValue(custLangEng);


                                    }
                                },
                                function (error) {
                                }
                            );

                        }
                        // Auto Populate Dealer
                        if (brand != null || CustomerCountry != null) {
                            //IF only one Repairing Dealer available in the pass out, this one need to be prepopulated ELSE do not populate (from pass out)
                            var fetchXml = `<fetch version="1.0" output-format="xml-platform" mapping="logical" distinct="true">
                              <entity name="account">
                                <attribute name="name" />
                                <attribute name="accountid" />
                                <attribute name="address1_composite" />
                                <attribute name="jarvis_dealerbrand" />
                                <attribute name="jarvis_country" />
                                <attribute name="accountnumber" />
                                <attribute name="msdyn_taxexemptnumber" />
                                <attribute name="jarvis_source" />
                                <attribute name="jarvis_responsableunitid" />
                                <attribute name="jarvis_accounttype" />
                                <order attribute="name" descending="false" />
                                <filter type="and">
                                  <condition attribute="jarvis_volvopay" operator="eq" value="1" />
                                  <condition attribute="statecode" operator="eq" value="0" />`;
                            if (CustomerCountry != null) {
                                fetchXml = fetchXml + `<condition attribute="jarvis_address1_country" operator="eq" uitype="jarvis_country" value="{` + CustomerCountry + `}" />`;
                            }
                            fetchXml = fetchXml + `</filter>
                                <link-entity name="jarvis_businesspartnerbrands" from="jarvis_businesspartner" to="accountid" link-type="inner" alias="ad">
                                  <filter type="and">
                                    <condition attribute="jarvis_brand" operator="eq"  uitype="jarvis_brand" value="{`+ brand + `}" />
                                  </filter>
                                </link-entity>
                              </entity>
                            </fetch>`;

                            fetchXml = "?fetchXml=" + encodeURIComponent(fetchXml);
                            Xrm.WebApi.retrieveMultipleRecords("account", fetchXml).then(
                                function success(result) {
                                    if (result.entities.length > 0) {
                                        var matchedDealer = result.entities[0];

                                        if (matchedDealer["accountid"]) {
                                            var lookup = new Array();
                                            lookup[0] = new Object();
                                            lookup[0].id = matchedDealer["accountid"];
                                            lookup[0].name = matchedDealer["name"];
                                            lookup[0].entityType = "account";
                                            executionContext.getFormContext().getAttribute("jarvis_dealer").setValue(lookup);
                                            onDealerSelect(executionContext);
                                        }

                                    }
                                },
                                function (error) {
                                    console.log(error.message);
                                    // handle error conditions
                                }
                            );
                        }
                        // End Auto populate dealer

                        // Auto Populate Channel
                        if (serviceLine != null || CustomerCountry != null) {
                            //IF only one Repairing Dealer available in the pass out, this one need to be prepopulated ELSE do not populate (from pass out)
                            var fetchXml = `<fetch version="1.0" output-format="xml-platform" mapping="logical" distinct="true">
                              <entity name="jarvis_channel">
                                <attribute name="jarvis_channelid" />
                                <attribute name="jarvis_name" />
                                <attribute name="createdon" />
                                <order attribute="jarvis_name" descending="false" />
                                <filter type="and">
                                  <condition attribute="statecode" operator="eq" value="0" />
                                  <condition attribute="jarvis_serviceline" operator="eq" uitype="jarvis_serviceline" value="{` + serviceLine + `}" />
                                </filter>
                                <link-entity name="jarvis_channel_jarvis_country" from="jarvis_channelid" to="jarvis_channelid" visible="false" intersect="true">
                                  <link-entity name="jarvis_country" from="jarvis_countryid" to="jarvis_countryid" alias="ai">
                                    <filter type="and">
                                      <condition attribute="jarvis_countryid" operator="eq" uitype="jarvis_country" value="{` + CustomerCountry + `}" />
                                    </filter>
                                  </link-entity>
                                </link-entity>
                              </entity>
                            </fetch>`;

                            fetchXml = "?fetchXml=" + encodeURIComponent(fetchXml);
                            Xrm.WebApi.retrieveMultipleRecords("jarvis_channel", fetchXml).then(
                                function success(result) {
                                    if (result.entities.length > 0) {
                                        var matchedChannel = result.entities[0];

                                        if (matchedChannel["jarvis_channelid"]) {
                                            var lookup = new Array();
                                            lookup[0] = new Object();
                                            lookup[0].id = matchedChannel["jarvis_channelid"];
                                            lookup[0].name = matchedChannel["jarvis_name"];
                                            lookup[0].entityType = "jarvis_channel";
                                            executionContext.getFormContext().getAttribute("jarvis_channel").setValue(lookup);
                                            onChannelChange(executionContext);
                                        }

                                    }
                                },
                                function (error) {
                                    console.log(error.message);
                                    // handle error conditions
                                }
                            );
                        }
                        // End Auto populate dealer------

                    }

                    // calculateBookingAmount(executionContext, result);
                },
                function (error) {
                }
            );

            // check credit card existing
            // if (!requestType || requestType == 334030001) {
            // #589879-Restrict creation of duplicate GOPs
            if (formType === 1) {
                var fetchXml = `<fetch version="1.0" output-format="xml-platform" mapping="logical" distinct="false">
                                    <entity name="jarvis_gop">
                                    <attribute name="jarvis_gopid" />
                                    <attribute name="jarvis_name" />
                                    <attribute name="createdon" />
                                    <order attribute="jarvis_name" descending="false" />
                                    <filter type="and">
                                        <condition attribute="jarvis_incident" operator="eq" uitype="incident" value="{`+ caseId + `}" />
                                        <condition attribute="jarvis_paymenttype" operator="eq" value="334030002" />
                                        <condition attribute='statecode' operator='eq' value='0' />
                                        <condition attribute='jarvis_volvopaypaymentrequestsent' operator="ne" value="334030000" />
                                        <condition attribute='jarvis_gopapproval' operator="eq" value="334030000" />`;
                fetchXml = fetchXml + `</filter>
                                    </entity>
                                </fetch>`;


                fetchXml = "?fetchXml=" + encodeURIComponent(fetchXml);
                Xrm.WebApi.retrieveMultipleRecords("jarvis_gop", fetchXml).then(
                    function success(result) {
                        if (result.entities.length > 0) {
                            existingCCGOP = true;
                        }
                        else {
                            existingCCGOP = false;
                        }
                    },
                    function (error) {
                        console.log(error.message);
                        existingCCGOP = false;
                    }
                );
            }
            //}
        }
    }
    else {
        existingCCGOP = false;
        var attribute = formContext.getAttribute("jarvis_dealer");
        attribute.controls.forEach(control => control.removePreSearch(filterCustomerAccounts));
        var gopApproval = formContext.getAttribute("jarvis_gopapproval")?.getValue();
        //Add Approved option
        if (gopApproval !== null && gopApproval != 334030001) {
            formContext.getControl("jarvis_gopapproval")?.removeOption(334030002);
            formContext.getControl("jarvis_gopapproval")?.removeOption(334030001);
            formContext.getControl("jarvis_gopapproval")?.addOption({ text: "Approved", value: 334030001 });
        }

        let attributeToDisable = formContext.getAttribute("jarvis_contact").controls.get(0);
        attributeToDisable.setDisabled(false);
    }
    formContext.ui.refreshRibbon();
}

function filterCustomerAccounts(executionContext) {
    //var control = executionContext.getEventSource();
    var customerAccountFilter = "<filter type='and'><condition attribute='jarvis_volvopay' operator='eq' value='true'/></filter>";
    executionContext.getFormContext().getControl("jarvis_dealer").addCustomFilter(customerAccountFilter, "account");
}

function onChannelChange(executionContext) {
    var formContext = executionContext.getFormContext();
    var paymentType = executionContext.getFormContext().getAttribute("jarvis_paymenttype")?.getValue();
    var requestType = executionContext.getFormContext().getAttribute("jarvis_requesttype")?.getValue();
    if (executionContext.getFormContext().getAttribute("jarvis_channel").getValue() != null) {
        var channelId = executionContext.getFormContext().getAttribute("jarvis_channel").getValue()[0].id;
        var fetchXml = "";
        if (paymentType == 334030002) // Credit Card
        {
            record_id = channelId.replace('{', '').replace('}', '');
            GetRelatedData(executionContext, record_id, "jarvis_channel", "Lookup", "_jarvis_currency_value", "jarvis_creditcardincurrency");
            GetRelatedData(executionContext, record_id, "jarvis_channel", "Lookup", "_jarvis_vat_value", "jarvis_vat");
            //calculateBookingAmount(executionContext, "");
            if (requestType != 334030002) {
                GetRelatedData(executionContext, record_id, "jarvis_channel", "Lookup", "_jarvis_currency_value", "jarvis_gopoutcurrency");
                GetRelatedData(executionContext, record_id, "jarvis_channel", "Lookup", "_jarvis_currency_value", "jarvis_totallimitoutcurrency");
            }
        }
    }
}

function GopHideDeclineCaseOnLoad(executionContext) {
    let formContext = executionContext.getFormContext();
    let gridContext = formContext.getControl("Subgrid_new_34");
    var paymentType = formContext.getAttribute("jarvis_paymenttype")?.getValue();
    if (paymentType == 334030002) // Credit Card
    {
        var gopApproval = formContext.getAttribute("jarvis_gopapproval")?.getValue();

        if (gopApproval !== null && gopApproval != 334030002) {
            formContext.getControl("jarvis_gopapproval").removeOption(334030002);
            formContext.getControl("jarvis_gopapproval").addOption({ text: "Declined", value: 334030002 });
        }
        if (gopApproval !== null && gopApproval != 334030001) {
            formContext.getControl("jarvis_gopapproval").removeOption(334030001);
        }

    }
    else {
        formContext.getControl("jarvis_gopapproval").removeOption(334030002);
    }
}

function onLimitInChange(executionContext) {
    const formContext = executionContext.getFormContext();
    ////const homeDealer = formContext.getAttribute("jarvis_dealer").getValue();
    const incident = formContext.getAttribute("jarvis_incident")?.getValue();
    if (incident != null && incident != undefined) {
        let incidentId = incident[0].id;
        incidentId = incidentId.replace('{', '').replace('}', '');

        //// GOP Type is RD
        const gopType = formContext.getAttribute("jarvis_requesttype")?.getValue();
        if (gopType != null && gopType != undefined && gopType == 334030002) {
            return;
        }

        const gopLimitIn = formContext.getAttribute("jarvis_gopincurrency")?.getValue();
        if (gopLimitIn != null && gopLimitIn != undefined) {
            formContext.getAttribute("jarvis_casegopoutavailableamounthdcurrency")?.setValue(gopLimitIn);
            let gopLimitInId = gopLimitIn[0].id;
            gopLimitInId = gopLimitInId.replace('{', '').replace('}', '');
            Xrm.WebApi.retrieveRecord("incident", incidentId, "?$select=jarvis_restgoplimitout,_jarvis_totalrestcurrencyout_value").then(
                function success(result) {
                    console.log(result);
                    const jarvis_restgoplimitout = result["jarvis_restgoplimitout"]; // Decimal
                    const jarvis_totalrestcurrencyout = result["_jarvis_totalrestcurrencyout_value"]; // Lookup
                    if (jarvis_restgoplimitout != null && jarvis_restgoplimitout != 0) {
                        if (jarvis_totalrestcurrencyout != null && jarvis_totalrestcurrencyout != undefined) {
                            exchangeRateCaseAvailable(formContext, jarvis_totalrestcurrencyout, gopLimitInId, jarvis_restgoplimitout, "jarvis_casegopoutavailableamounthd");
                        }
                        else {
                            formContext.ui.setFormNotification("Case Available GOP OUT Amount Currency is not present at Case level.", "WARNING", "WAR_AvailableGOPOUTAmountCurrencyValidation");
                        }
                    }
                    else {
                        formContext.getAttribute("jarvis_casegopoutavailableamounthd")?.setValue(jarvis_restgoplimitout);
                    }
                },
                function (error) {
                    console.log(error.message);
                }
            );
        }
    }
}
function onLimitOutChange(executionContext) {
    const formContext = executionContext.getFormContext();
    ////const repairingDealer = formContext.getAttribute("jarvis_repairingdealer").getValue();
    const incident = formContext.getAttribute("jarvis_incident")?.getValue();
    if (incident != null && incident != undefined) {
        let incidentId = incident[0].id;
        incidentId = incidentId.replace('{', '').replace('}', '');

        //// GOP Type is HD
        const gopType = formContext.getAttribute("jarvis_requesttype")?.getValue();
        if (gopType != null && gopType != undefined && gopType == 334030001) {
            return;
        }
        const gopLimitOut = formContext.getAttribute("jarvis_gopoutcurrency")?.getValue();
        if (gopLimitOut != null && gopLimitOut != undefined) {
            formContext.getAttribute("jarvis_casegopoutavailableamountrdcurrency")?.setValue(gopLimitOut);
            let gopLimitOutId = gopLimitOut[0].id;
            gopLimitOutId = gopLimitOutId.replace('{', '').replace('}', '');
            Xrm.WebApi.retrieveRecord("incident", incidentId, "?$select=jarvis_restgoplimitout,_jarvis_totalrestcurrencyout_value").then(
                function success(result) {
                    const jarvis_restgoplimitout = result["jarvis_restgoplimitout"]; // Decimal
                    const jarvis_totalrestcurrencyout = result["_jarvis_totalrestcurrencyout_value"]; // Lookup
                    if (jarvis_restgoplimitout != null && jarvis_restgoplimitout != 0) {
                        if (jarvis_totalrestcurrencyout != null && jarvis_totalrestcurrencyout != undefined) {
                            exchangeRateCaseAvailable(formContext, jarvis_totalrestcurrencyout, gopLimitOutId, jarvis_restgoplimitout, "jarvis_casegopoutavailableamountrd");
                        }
                        else {
                            //// formContext.ui.setFormNotification("Case Available GOP OUT Amount Currency is not present at Case level.", "WARNING", "WAR_AvailableGOPOUTAmountCurrencyValidation");
                        }
                    }
                    else {
                        formContext.getAttribute("jarvis_casegopoutavailableamountrd")?.setValue(jarvis_restgoplimitout);
                    }
                },
                function (error) {
                    //// Handle the error if it required.
                }
            );
        }
    }
}
function exchangeRateCaseAvailable(formContext, baseCurrency, counterCurrency, jarvis_restgoplimitout, field) {
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
}

function OnLoadGetvolvoPayAutomationEnabled(executionContext) {
    Xrm.WebApi.retrieveMultipleRecords("jarvis_configurationjarvis", "?$select=jarvis_volvopayautomation&$filter=jarvis_volvopayautomation ne null").then(
        function success(results) {
            for (var i = 0; i < results.entities.length; i++) {
                var result = results.entities[i];
                // Columns
                var jarvis_configurationjarvisid = result["jarvis_configurationjarvisid"]; // Guid
                var jarvis_volvopayautomation = result["jarvis_volvopayautomation"]; // Text
                if (jarvis_volvopayautomation != null && jarvis_configurationjarvisid != null && jarvis_volvopayautomation) {

                    volvoPayAutomationEnabled = true;

                }

            }
        }
    );

    var formContext = executionContext.getFormContext();
    var dealer = executionContext.getFormContext().getAttribute("jarvis_dealer")?.getValue();
    if (dealer) {
        var gopDealerid = dealer[0].id;
        gopDealerid = gopDealerid.replace('{', '').replace('}', '');
        Xrm.WebApi.retrieveRecord("account", gopDealerid, "?$select=_jarvis_address1_country_value").then(
            function success(result) {
                if (result["_jarvis_address1_country_value"]) {
                    dealerCountry = result["_jarvis_address1_country_value"];
                }

            }

        );
    }
}

function FilterHDLookup(executionContext) {
    var dealerFilter = "<filter type='and'>" +
        "<condition attribute='jarvis_accounttype' operator='in'>" +
        "<value>334030001</value>" +
        "<value>334030003</value>" +
        "<value>334030002</value>" +
        "</condition>" +
        "</filter>";
    executionContext.getFormContext().getControl("jarvis_dealer").addCustomFilter(dealerFilter);
}

function AddPreSearchToLookup(executionContext) {
    var formContext = executionContext.getFormContext();
    var homeDealerLookup = formContext.getAttribute("jarvis_dealer");
    if (homeDealerLookup != null) {
        homeDealerLookup.controls.forEach(control => control.addPreSearch(FilterHDLookup));
    }
}

function ShowHideStatusCode(executionContext) {
    var formContext = executionContext.getFormContext();
    var statusCodeControl = formContext.getControl("statuscode");
    let statusCode = formContext.getAttribute("statuscode")?.getValue();
    if (statusCode === 334030002) {
        formContext.getControl("jarvis_communicationfailedreason")?.setVisible(true);
        statusCodeControl.removeOption(30);
    }
    else {
        statusCodeControl?.removeOption(334030002);
        formContext.getControl("jarvis_communicationfailedreason")?.setVisible(false);
        formContext.getAttribute("jarvis_communicationfailedreason")?.setValue(null);
    }
    /*var paymentType = formContext.getAttribute("jarvis_paymenttype")?.getValue();
    if (paymentType == 334030002 ) // Credit Card
    {
        var gopApproval = formContext.getAttribute("jarvis_gopapproval")?.getValue();

        if (gopApproval !== null && gopApproval == 334030002) {
            formContext.getAttribute("statecode")?.setValue(1);
        }
    }*/
}

function openConfirmDialogBox(entityId, formContext) {
    var bookingAmount = formContext.getAttribute("jarvis_creditcardgopinbooking")?.getValue();
    var customerEmailAddress = formContext.getAttribute("jarvis_creditcardemailaddress")?.getValue();
    var bookingCurrency = "";
    var bookingCurrencylookupObj = formContext.getAttribute("jarvis_creditcardincurrency")?.getValue();
    if (bookingCurrencylookupObj !== null && bookingCurrencylookupObj !== undefined) {
        bookingCurrency = " " + bookingCurrencylookupObj[0].name;
    }
    if (bookingAmount && customerEmailAddress) {
        var confirmStrings = {
            text: "Confirm the send out of the Payment request of " + bookingAmount.toString() + bookingCurrency.toString() + " to customer?", title: "Confirmation Of Payment", confirmButtonLabel: "Confirm", cancelButtonLabel: "Decline"
        };
        // var confirmStrings = {
        //  text: "Confirm the send out of the Payment request of " + bookingAmount.toString() +" to customer?", title: "Confirmation Of //Payment", confirmButtonLabel: "Confirm", cancelButtonLabel: "Decline" };
        Xrm.Navigation.openConfirmDialog(confirmStrings, confirmOptions).then(
            function (success) {
                if (success.confirmed) {
                    var data = {
                        "jarvis_volvopaypaymentrequestsent": 334030001
                    }
                    Xrm.WebApi.updateRecord("jarvis_gop", entityId, data).then
                        (
                            function success(result) {
                                formContext.data.refresh(true);
                            },
                            function (error) {
                            }
                        );



                }
                else if (success.confirmed == false) {

                    var userSettings = Xrm.Utility.getGlobalContext().userSettings;
                    var currentUserId = userSettings.userId.toString().slice(1, 37);
                    Xrm.WebApi.retrieveRecord("systemuser", currentUserId, "?$select=fullname").then(
                        function success(result) {
                            var currentUser = "Manually declined by " + result["fullname"] + ", before send out to Volvo Pay";
                            var data = {
                                "jarvis_gopapproval": 334030002,
                                "jarvis_contact": currentUser
                            }
                            Xrm.WebApi.updateRecord("jarvis_gop", entityId, data).then
                                (
                                    function success(result) {
                                        formContext.data.refresh(true);
                                        //disable approver name
                                        let attributeToDisable = formContext.getAttribute("jarvis_contact").controls.get(0);
                                        attributeToDisable.setDisabled(true);
                                        //disable approver name
                                        let approvalToDisable = formContext.getAttribute("jarvis_gopapproval").controls.get(0);
                                        approvalToDisable.setDisabled(true);
                                    },
                                    function (error) {
                                    });

                        });
                }

            });
    }
}

function lockField(executionContext) {
    "use strict";
    var formContext = executionContext.getFormContext();

    var fieldArr = ["jarvis_channel", "jarvis_creditcardlanguage", "jarvis_creditcardmessage", "jarvis_creditcardemailaddress"];
    var formType = formContext.ui.getFormType();
    if (formType != 1) {
        for (var ii = 0; ii < fieldArr.length; ii++) {
            var fieldControl = formContext.getControl(fieldArr[ii]);
            if (fieldControl) {
                if (formContext.getAttribute(fieldArr[ii])?.getValue() != null) {
                    fieldControl.setDisabled(true);
                }
                else {
                    fieldControl.setDisabled(false);
                }
            }
        }
    }



}

function saveCloseButtonVisibility(primaryControl) {
    var formContext = primaryControl;
    let paymentType = formContext.getAttribute("jarvis_paymenttype")?.getValue();
    if (paymentType !== 334030002) // Credit Card
    {
        return true;
    }
    else {
        return false;
    }
}

/*LoadParentCase = function (executionContext) {
    var casefield = executionContext.getFormContext().getAttribute("jarvis_incident").getValue();
    let casefieldId = casefield[0].id.slice(1, -1);
    var globalContext = Xrm.Utility.getGlobalContext();
    var mdaAppName;
    globalContext.getCurrentAppName().then(
        function successCallback(appName) {

            mdaAppName = appName;
            if (mdaAppName === "Workspace App") {
                var currentTab = Microsoft.Apm.getFocusedSession().getFocusedTab();
                var session = Microsoft.Apm.getFocusedSession();
                session.getAllTabs().forEach(id => {
                    var tab = session.getTab(id);
                    if (tab.tabId !== currentTab.tabId) {
                        Microsoft.Apm.refreshTab(tab.tabId);
                    }
                });
            }

        }, function errorCallback() {

            alert('Error');

    });
    setTimeout(LoadParentCase, 9000);
}*/