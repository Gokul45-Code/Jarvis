var Jarvis = Jarvis || {};

Jarvis.GOPHDUnApprovedCopyConst = {
    text: "Only the last GOP record for this GOP Dealer can be copied, please select the last unapproved GOP record."
}

Jarvis.GOPHDApprovedCopyConst = {
    text: "Only the last GOP record for this GOP Dealer can be copied, please select the last approved GOP record."
};

Jarvis.GOPRDUnApprovedCopyConst = {
    text: "Only the last GOP record for this RD can be copied, please select the last unapproved GOP record."
};

Jarvis.GOPRDApprovedCopyConst = {
    text: "Only the last GOP record for this RD can be copied, please select the last approved GOP record."
};

Jarvis.GOPCopyRibbon = {

    //Copy GOP Button Logic for the FormControl
    copyGOPonForm: function (entityName, entityId, primaryControl) {
        "use strict";
        var caseId = primaryControl.getAttribute("jarvis_incident")?.getValue();
        if (caseId != null && caseId != undefined) {
            caseId = caseId[0]["id"]?.replace('{', '').replace('}', '');
        }
        //var isApproved = primaryControl.getAttribute("jarvis_approved")?.getValue();
        var isApproved = primaryControl.getAttribute("jarvis_gopapproval")?.getValue();
        var gopReqType = primaryControl.getAttribute("jarvis_requesttype")?.getValue();
        var jarvis_paymenttype = primaryControl.getAttribute("jarvis_paymenttype")?.getValue();
        var gopId = entityId[0]?.replace('{', '').replace('}', '');
        var repairingDealer = primaryControl.getAttribute("jarvis_repairingdealer")?.getValue();
        if (repairingDealer != null && repairingDealer != undefined) {
            repairingDealer = repairingDealer[0]["id"]?.replace('{', '').replace('}', '');
        }
        var dealer = primaryControl.getAttribute("jarvis_dealer")?.getValue();
        if (dealer != null && dealer != undefined) {
            dealer = dealer[0]["id"]?.replace('{', '').replace('}', '');
        }
        if (isApproved !== null && isApproved !== undefined && gopReqType !== null && gopReqType !== undefined
            && ((dealer != null && dealer != undefined) || (repairingDealer != null && repairingDealer != undefined))) {
            Jarvis.GOPCopyRibbon.checkLatestGop(caseId, isApproved, gopReqType, gopId, dealer, repairingDealer, jarvis_paymenttype);
        }
        else {
            throw "No Condition matched for approved,requestType,GopDealer or RepairingDealer";
        }
    },

    //Copy GOP Button Logic for the SubgridControl
    copyGOPonSubGrid: function (entityName, entityId, selectedControl, primaryControl) {
        "use strict";
        try {
            var caseId = selectedControl._formContext._data?._formContext?._entityReference?.id?.guid;
            var gopId = entityId[0];
            Xrm.WebApi.retrieveRecord("jarvis_gop", gopId, "?$select=_jarvis_incident_value,jarvis_approved,jarvis_gopapproval,_jarvis_dealer_value,jarvis_requesttype,_jarvis_repairingdealer_value,jarvis_paymenttype").then(
                function success(result) {
                    //var jarvis_approved = result["jarvis_approved"];
                    var jarvis_approved = result["jarvis_gopapproval"];
                    var jarvis_dealer = result["_jarvis_dealer_value"];
                    var jarvis_requesttype = result["jarvis_requesttype"];
                    var jarvis_paymenttype = result["jarvis_paymenttype"];
                    var jarvis_repairingdealer = result["_jarvis_repairingdealer_value"];
                    caseId = result["_jarvis_incident_value"];
                    if (jarvis_approved !== null && jarvis_approved !== undefined && jarvis_requesttype !== null && jarvis_requesttype !== undefined && ((jarvis_dealer !== null && jarvis_dealer !== undefined) || (jarvis_repairingdealer !== null && jarvis_repairingdealer !== undefined))) {
                        Jarvis.GOPCopyRibbon.checkLatestGop(caseId, jarvis_approved, jarvis_requesttype, gopId, jarvis_dealer, jarvis_repairingdealer, jarvis_paymenttype);
                    }
                    else {
                        throw "No Condition matched for approved,requestType,GopDealer or RepairingDealer";
                    }
                },
                function (error) {
                    throw error;

                }
            );
        }
        catch (error) {
            Xrm.Navigation.openErrorDialog(error).then(function (success) {
            }, function (error) {

            });
        }
    },

    // CheckCopyGop For Latest Approved/UnApproved HD and Approved/UnApproved RD logic.
    checkLatestGop: function (caseId, approved, requestType, gopId, gopDealerId, gopRepairingDealerId, jarvis_paymenttype) {
        "use strict";
        try {
            var retrievedGOP;
            var fetchXml = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>"
                + "<entity name='jarvis_gop'>"
                + "<attribute name='jarvis_gopid' />"
                + "<attribute name='modifiedon' />"
                + "<order attribute='jarvis_gopapprovaltime' descending='true' />"
                + "<order attribute='modifiedon' descending='true' />"
                + "<filter type='and'>"
                + "<condition attribute='jarvis_gopapproval' operator='eq' value='" + approved + "' />"
                + "<condition attribute='jarvis_relatedgop' operator='null' />"
                + "<condition attribute='jarvis_incident' operator='eq' value='" + caseId + "' />"
                + "<condition attribute='jarvis_requesttype' operator='eq' value='" + requestType + "' />"
                + "<condition attribute='statecode' operator='eq' value='0' />";
            if (requestType === 334030001 && gopDealerId !== null && gopDealerId !== undefined) {
                fetchXml += "<condition attribute='jarvis_dealer' operator='eq' value='" + gopDealerId + "' />";
            }
            else if (requestType === 334030002 && gopRepairingDealerId !== null && gopRepairingDealerId !== undefined) {
                fetchXml += "<condition attribute='jarvis_repairingdealer' operator='eq' value='" + gopRepairingDealerId + "' />";
            }
            fetchXml += "</filter>"
                + "</entity>"
                + "</fetch>";

            fetchXml = "?fetchXml=" + encodeURIComponent(fetchXml);

            Xrm.WebApi.retrieveMultipleRecords("jarvis_gop", fetchXml).then(
                function success(results) {
                    if (results.entities.length > 0) {
                        var approvedGOP = results.entities[0];
                        if (approvedGOP["jarvis_gopid"].toUpperCase() === gopId.toUpperCase()) {
                            if (approved == 334030001) {
                                var fetchXml2 = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>"
                                    + "<entity name='jarvis_gop'>"
                                    + "<attribute name='jarvis_gopid' />"
                                    + "<attribute name='modifiedon' />"
                                    + "<order attribute='jarvis_gopapprovaltime' descending='true' />"
                                    + "<filter type='and'>"
                                    + "<condition attribute='jarvis_gopapproval' operator='eq' value='334030000' />"
                                    + "<condition attribute='jarvis_relatedgop' operator='null' />"
                                    + "<condition attribute='jarvis_incident' operator='eq' value='" + caseId + "' />"
                                    + "<condition attribute='jarvis_requesttype' operator='eq' value='" + requestType + "' />"
                                    + "<condition attribute='statecode' operator='eq' value='0' />";
                                if (requestType === 334030001 && gopDealerId !== null && gopDealerId !== undefined) {
                                    fetchXml2 += "<condition attribute='jarvis_dealer' operator='eq' value='" + gopDealerId + "' />";
                                }
                                else if (requestType === 334030002 && gopRepairingDealerId !== null && gopRepairingDealerId !== undefined) {
                                    fetchXml2 += "<condition attribute='jarvis_dealer' operator='eq' value='" + gopDealerId + "' />";
                                    fetchXml2 += "<condition attribute='jarvis_repairingdealer' operator='eq' value='" + gopRepairingDealerId + "' />";
                                }
                                fetchXml2 += "</filter>"
                                    + "</entity>"
                                    + "</fetch>";

                                fetchXml2 = "?fetchXml=" + encodeURIComponent(fetchXml2);

                                Xrm.WebApi.retrieveMultipleRecords("jarvis_gop", fetchXml2).then(
                                    function success(results) {
                                        if (results.entities.length > 0) {
                                            Xrm.Navigation.openAlertDialog("The GOP cannot be copied as there is a pending GOP request for this dealer.");
                                        }
                                        else {//// Open Form
                                            var entityFormOptions = {
                                                "entityName": "jarvis_gop",
                                                "createFromEntity": {
                                                    "entityType": "jarvis_gop",
                                                    "id": gopId
                                                },
                                                "navbar": "entity",
                                            };
                                            var formParameters = {};
                                            if (approved) {
                                                formParameters = {
                                                    "jarvis_parentgopid": gopId,
                                                    "jarvis_gopapproval": 334030000,
                                                    "statuscode": 20,
                                                    "jarvis_contact": null

                                                }
                                            } else if (!approved) {
                                                formParameters = {
                                                    "jarvis_parentgopid": gopId,
                                                    "jarvis_gopapproval": 334030000,
                                                    "jarvis_contact": null
                                                }
                                            }
                                            if (requestType === 334030001) {
                                                if (jarvis_paymenttype == 334030002) {
                                                    formParameters["jarvis_goplimitin"] = null;
                                                    formParameters["jarvis_creditcardgopinbooking"] = null;
                                                }
                                                else {
                                                    formParameters["jarvis_goplimitout"] = null;

                                                }
                                            }
                                            else if (requestType === 334030002 || jarvis_paymenttype == 334030002) {
                                                formParameters["jarvis_goplimitin"] = null;
                                                formParameters["jarvis_creditcardgopinbooking"] = null;
                                            }

                                            Xrm.Navigation.openForm(entityFormOptions, formParameters).then(successCallback, errorCallback);
                                        }
                                    });
                            }
                            else {
                                //// Open Form
                                var entityFormOptions = {
                                    "entityName": "jarvis_gop",
                                    "createFromEntity": {
                                        "entityType": "jarvis_gop",
                                        "id": gopId
                                    },
                                    "navbar": "entity",
                                };
                                var formParameters = {};
                                if (approved === 334030001) {
                                    formParameters = {
                                        "jarvis_parentgopid": gopId,
                                        //"jarvis_approved": false,
                                        "jarvis_gopapproval": 334030000,
                                        "statuscode": 20,
                                        "jarvis_contact": null

                                    }
                                } else if (approved === 334030000) {
                                    formParameters = {
                                        "jarvis_parentgopid": gopId,
                                        //"jarvis_approved": false,
                                        "jarvis_gopapproval": 334030000,
                                        "jarvis_contact": null
                                    }
                                }
                                else if (approved === 334030002) {
                                    formParameters = {
                                        "jarvis_parentgopid": gopId,
                                        //"jarvis_approved": false,
                                        "jarvis_gopapproval": 334030000,
                                        "jarvis_contact": null
                                    }
                                }
                                if (requestType === 334030001) {
                                    if (jarvis_paymenttype == 334030002) {
                                        formParameters["jarvis_goplimitin"] = null;
                                        formParameters["jarvis_creditcardgopinbooking"] = null;
                                    }
                                    else {
                                        formParameters["jarvis_goplimitout"] = null;

                                    }
                                }
                                else if (requestType === 334030002 || jarvis_paymenttype == 334030002) {
                                    formParameters["jarvis_goplimitin"] = null;
                                    formParameters["jarvis_creditcardgopinbooking"] = null;
                                }

                                Xrm.Navigation.openForm(entityFormOptions, formParameters).then(successCallback, errorCallback);
                            }
                        }
                        else {
                            //// Open Alert Dialog Based on Request Type and Approved.
                            //// HD and UnApproved.
                            if (requestType === 334030001 && approved === 334030000) {
                                Xrm.Navigation.openAlertDialog(Jarvis.GOPHDUnApprovedCopyConst);
                            }
                            else if (requestType === 334030001 && approved === 334030001) {
                                Xrm.Navigation.openAlertDialog(Jarvis.GOPHDApprovedCopyConst);
                            }
                            else if (requestType === 334030002 && approved === 334030000) {
                                Xrm.Navigation.openAlertDialog(Jarvis.GOPRDUnApprovedCopyConst);
                            }
                            else if (requestType === 334030002 && approved === 334030001) {
                                Xrm.Navigation.openAlertDialog(Jarvis.GOPRDApprovedCopyConst);
                            }
                        }
                    }
                },
                function (error) {
                    throw error;
                }
            );
        }
        catch (error) {
            Xrm.Navigation.openErrorDialog(error).then(function (success) {
            }, function (error) {

            });
        }
    },

}