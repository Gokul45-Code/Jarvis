// Copyright (c) Microsoft Corporation. All rights reserved.

class Utility {

    //// To open related record for Case,Contact,BusinessPartner and CaseContact based on incoming call number.
    static openRelatedRecord(number) {

        return new Promise(function (resolve, reject) {
            if (!number) {
                return reject("number is empty");
            }

            var input = {
                templateName: "msdyn_ContactList",
                templateParameters: {
                    // searchText: "{$filter=telephone1 eq '" + number+ "'}"
                    searchText: number
                },
                isFocused: true
            }
            Microsoft.CIFramework.createTab(input).then((tabId) => {
                console.log("created tab with id " + tabId);
                resolve({ value: tabId, isNameFound: true });
            }, (error) => {
                console.log(error);
                return resolve("Related Record is not opened");
            });
        });
    };
    //// Update Session Caller Name render and Open If only one Contact Form exists.
    static openContactRecord(number, searchOnly, sessionBag, sessionId, recordId) {
        debugger;
        var recordAvailable = false;
        return new Promise(function (resolve, reject) {
            if (!number) {
                return reject("number is empty");
            }
            sessionBag._number = number;
            //In this sample, we search all 'contact' records
            log("Trying to find name of caller " + number + " with searchOnly=" + searchOnly);
            var query = "?$select=fullname,_jarvis_language_value&$expand=parentcustomerid_account($select=name),jarvis_Language($select=jarvis_languageid,jarvis_name)&$filter=";   //In this sample, we are retrieving the 'fullname' attribute of the record
            if (recordId) { //oData query to retrieve a specific record
                query += "contactid eq " + recordId;
            }
            else {  //oData query to search all records for current phone number
                query += "(contains(mobilephone,'" + encodeURIComponent(number) + "') or contains(company,'" + encodeURIComponent(number) + "'))";
            }
            Microsoft.CIFramework.searchAndOpenRecords("contact", query, true).then(
                function (valStr) {    //We got the CRM contact record for our search query
                    try {
                        //log("contact DATA");
                        //log(valStr);
                        debugger;
                        let val = JSON.parse(valStr);
                        if (typeof (val) == "object" && !(val[1] == undefined)) {
                            if (sessionBag) {
                                sessionBag.recordAvailable = true;
                                sessionBag._name = val[0].fullname != null ? val[0].fullname : null;
                                sessionBag._contactid = val[0].contactid;
                                sessionBag.contactrecordAvailable = true;
                                if (val[0].parentcustomerid_account) {
                                    sessionBag._companyName = val[0].parentcustomerid_account.name != null ? val[0].parentcustomerid_account.name : null;
                                    sessionBag._companyId = val[0].parentcustomerid_account.accountid;
                                }
                                if (val[0].jarvis_Language) {
                                    sessionBag._languageName = val[0].jarvis_Language.jarvis_name != null ? val[0].jarvis_Language.jarvis_name : null;
                                    sessionBag._languageId = val[0].jarvis_Language.jarvis_languageid;
                                }
                                log("The caller name is " + val[0].fullname);
                                sessionBag.renderCallerName();
                            }
                        }
                        else if (typeof (val) == "object" && val[1] == undefined && !(val[0] == undefined)) {// Open Single Record
                            openEntityRecord("msdyn_entityrecord", "contact", val[0].contactid);
                            if (sessionBag) {
                                //Record the fullname and CRM record id
                                sessionBag._name = val[0].fullname != null ? val[0].fullname : null;
                                sessionBag._contactid = val[0].contactid;
                                sessionBag.contactrecordAvailable = true;
                                if (val[0].parentcustomerid_account) {
                                    sessionBag._companyName = val[0].parentcustomerid_account.name != null ? val[0].parentcustomerid_account.name : null;
                                    sessionBag._companyId = val[0].parentcustomerid_account.accountid;
                                }
                                if (val[0].jarvis_Language) {
                                    sessionBag._languageName = val[0].jarvis_Language.jarvis_name != null ? val[0].jarvis_Language.jarvis_name : null;
                                    sessionBag._languageId = val[0].jarvis_Language.jarvis_languageid;
                                }
                                log("The caller name is " + val[0].fullname);
                                sessionBag.renderCallerName();
                            }
                        }
                        else {
                            sessionBag.contactrecordAvailable = false;
                            sessionBag.renderCallerName();
                        }
                        debugger;
                        sessionslist.sessionmap.set(sessionId, sessionBag);
                        resolve({ value: val[0].fullname, isNameFound: true });
                    }
                    catch (e) {
                        log("Unable to find caller name- Exception: " + e);
                        resolve({ value: number, isNameFound: false });
                    }
                }
            )
                .catch(function (reason) {
                    if (!reason) {
                        reason = "Unknown Reason";
                    }
                    log("Couldn't retrieve caller name because " + reason.toString());
                    resolve({ value: number, isNameFound: false });
                });
        });
    }
    //// Open If only one Business Partner record exists
    static openBusinessPartnerRecord(number, searchOnly, sessionBag, recordId) {
        //In this sample, we search all 'Account' records
        return new Promise(function (resolve, reject) {
            if (!number) {
                return reject("number is empty");
            }
            var query = "?$select=name,accountid&$filter=contains(telephone1, '" + encodeURIComponent(number) + "')";
            Microsoft.CIFramework.searchAndOpenRecords("account", query, true).then(
                function (valStr) {    //We got the CRM contact record for our search query
                    try {
                        let val = JSON.parse(valStr);
                        log("Account DATA----------------------------------------");
                        log(valStr);
                        if (!typeof (val) == "object" && !(val[1] == undefined)) {
                            sessionBag.recordAvailable = true;
                        }
                        else if (typeof (val) == "object" && val[1] == undefined && !(val[0] == undefined)) {// Open Single Record
                            openEntityRecord("msdyn_entityrecord", "account", val[0].accountid);
                            sessionBag.accountrecordAvailable = true;
                        }
                        else
                            sessionBag.accountrecordAvailable = false;
                        resolve({ value: val[0].name, isNameFound: true });
                    }
                    catch (e) {
                        log("Unable to find BusinessParter name- Exception: " + e);
                        resolve({ value: number, isNameFound: false });
                    }
                }
            ).catch(function (reason) {
                if (!reason) {
                    reason = "Unknown Reason";
                }
                log("Couldn't retrieve BusinessParter because " + reason.toString());
                resolve({ value: number, isNameFound: false });
            });

        });

    }
    //// Open If only one Case record exists
    static openCaseRecord(number, searchOnly, sessionBag, recordId) {
        return new Promise(function (resolve, reject) {
            if (!number) {
                return reject("number is empty");
            }

            var query = "?$select=title,incidentid,jarvis_callerphone,jarvis_driverphone&$filter=((contains(jarvis_callerphone,'" + encodeURIComponent(number) + "') or contains(jarvis_driverphone,'" + encodeURIComponent(number) + "') or contains(jarvis_callerfixedphone,'" + encodeURIComponent(number) + "')) and statecode eq 0)";
            Microsoft.CIFramework.searchAndOpenRecords("incident", query, true).then(
                function (valStr) {    //We got the CRM contact record for our search query
                    try {
                        let val = JSON.parse(valStr);
                        log("Case DATA-------------------------------------------------");
                        log(valStr);
                        if (typeof (val) == "object" && !(val[1] == undefined)) {
                            sessionBag.recordAvailable = true;
                        }
                        else if (typeof (val) == "object" && val[1] == undefined && !(val[0] == undefined)) {// Open Single Record
                            openEntityRecord("msdyn_entityrecord", "incident", val[0].incidentid);
                            sessionBag.caserecordAvailable = true;
                        }
                        else {
                            sessionBag.caserecordAvailable = false;
                        }

                        // if No Record Available Create New Case

                        resolve({ value: val[0].title, isNameFound: true });
                    }
                    catch (e) {
                        log("Unable to find Incident name- Exception: " + e);
                        resolve({ value: number, isNameFound: false });
                    }
                }
            ).catch(function (reason) {
                if (!reason) {
                    reason = "Unknown Reason";
                }
                log("Couldn't retrieve BusinessParter because " + reason.toString());
                resolve({ value: number, isNameFound: false });
            });
        });



    }
    //// Open If only one Case Contact record exists
    static openCaseContactRecord(number, searchOnly, sessionBag, recordId) {
        return new Promise(function (resolve, reject) {
            if (!number) {
                return reject("number is empty");
            }

            var query = "?$select=jarvis_firstname,jarvis_lastname,jarvis_businesspartner,jarvis_casecontactid,_jarvis_incident_value,jarvis_mobilephone,jarvis_phone&$expand=jarvis_PreferredLanguage($select=jarvis_languageid,jarvis_name)&$filter=(contains(jarvis_mobilephone,'" + encodeURIComponent(number) + "') or contains(jarvis_phone,'" + encodeURIComponent(number) + "'))";
            Microsoft.CIFramework.searchAndOpenRecords("jarvis_casecontact", query, true).then(
                function (valStr) {
                    try {
                        let val = JSON.parse(valStr);
                        let fullName = "";
                        log("Case Contact DATA-------------------------------------------------");
                        log(valStr);
                        if (typeof (val) == "object" && !(val[1] == undefined)) {
                            if (sessionBag) {
                                sessionBag.recordAvailable = true;
                                sessionBag._multipleCaseConAvailable = true;
                                sessionBag.caseContactRecordAvailable = true;
                                sessionBag._caseContactName = (val[0].jarvis_firstname != null && val[0].jarvis_firstname != undefined) || (val[0].jarvis_lastname != null && val[0].jarvis_lastname != undefined) ? ((val[0].jarvis_firstname != null ? val[0].jarvis_firstname : "") + " " + (val[0].jarvis_lastname != null ? val[0].jarvis_lastname : "")) : null;
                                sessionBag._caseContactId = val[0].jarvis_casecontactid;
                                sessionBag._caseContactCompanyName = val[0].jarvis_businesspartner != null || val[0].jarvis_businesspartner != undefined ? val[0].jarvis_businesspartner : null;
                                if (val[0].jarvis_PreferredLanguage) {
                                    sessionBag._caseConLanguageName = (val[0].jarvis_PreferredLanguage.jarvis_name != null || val[0].jarvis_PreferredLanguage.jarvis_name != undefined) ? val[0].jarvis_PreferredLanguage.jarvis_name : null;
                                    sessionBag._caseContactLanguageId = val[0].jarvis_PreferredLanguage.jarvis_languageid;
                                }
                                sessionBag.renderCallerName();
                            }
                        }
                        else if (typeof (val) == "object" && val[1] == undefined && !(val[0] == undefined)) {// Open Single Record
                            openEntityRecord("msdyn_entityrecord", "jarvis_casecontact", val[0].jarvis_casecontactid);
                            if (sessionBag) {
                                //Record the fullname and CRM record id
                                sessionBag._multipleCaseConAvailable = false;
                                sessionBag.caseContactRecordAvailable = true;
                                sessionBag._caseContactName = (val[0].jarvis_firstname != null && val[0].jarvis_firstname != undefined) || (val[0].jarvis_lastname != null && val[0].jarvis_lastname != undefined) ? ((val[0].jarvis_firstname != null ? val[0].jarvis_firstname : "") + " " + (val[0].jarvis_lastname != null ? val[0].jarvis_lastname : "")) : null;
                                sessionBag._caseContactId = val[0].jarvis_casecontactid;
                                sessionBag._caseContactCompanyName = val[0].jarvis_businesspartner != null || val[0].jarvis_businesspartner != undefined ? val[0].jarvis_businesspartner : null;
                                if (val[0].jarvis_PreferredLanguage) {
                                    sessionBag._caseConLanguageName = (val[0].jarvis_PreferredLanguage.jarvis_name != null || val[0].jarvis_PreferredLanguage.jarvis_name != undefined) ? val[0].jarvis_PreferredLanguage.jarvis_name : null;
                                    sessionBag._caseContactLanguageId = val[0].jarvis_PreferredLanguage.jarvis_languageid;
                                }
                                sessionBag.renderCallerName();
                            }
                        }
                        else {
                            sessionBag.caseContactRecordAvailable = false;
                            sessionBag.renderCallerName();
                        }

                        resolve({ value: val[0].title, isNameFound: true });
                    }
                    catch (e) {
                        log("Unable to find Case Contact name- Exception: " + e);
                        resolve({ value: number, isNameFound: false });
                    }
                }
            ).catch(function (reason) {
                if (!reason) {
                    reason = "Unknown Reason";
                }
                log("Couldn't retrieve Case Contact because " + reason.toString());
                resolve({ value: number, isNameFound: false });
            });
        });



    }
};

class SessionList {
    constructor() {
        this.sessionmap = new Map();
    }
}

class SessionInfo {
    constructor() {
        this._name = null;   //Current caller name by searching all contact records based on phone number
        this._number = null;
        this.caserecordAvailable = false;
        this.accountrecordAvailable = false;
        this.contactrecordAvailable = false;//Current phone number
        this.caseContactRecordAvailable = false;
        this._multipleContactAvailable = false;
        this._multipleCaseConAvailable = false;
        this._companyName = null;
        this._companyId = null;
        this._languageId = null;
        this._languageName = null;
        this._caseContactId = null;
        this._caseContactName = null;
        this._caseContactCompanyName = null;
        this._caseContactCompanyId = null;
        this._caseContactLanguageId = null;
        this._caseConLanguageName = null;
        this.siteId = null;
        this.userId = null;
        this.ExactCallingPartyNumber = null;
    }

    get name() {
        if (this._name) {
            return this._name;
        }
        else {
            return this._number;
        }
    }

    /* Display the current caller's name. if the name is not available, display the phone number */
    renderCallerName() {
        var fn = this.name;

        if (this.contactrecordAvailable && (this._name != null || this._name != undefined)) {
            $(".callerName").text(this._name);
            fn = this._name;
        }
        else if (!this.contactrecordAvailable && this.caseContactRecordAvailable && (this._caseContactName != null || this._caseContactName != undefined)) {
            $(".callerName").text(this._caseContactName.trim());
            fn = this._caseContactName.trim();
        }
        else {
            $(".callerName").text("Unknown");
        }
        ////Setting Company Name at Widget
        if (this.contactrecordAvailable && (this._companyName != null || this._companyName != undefined)) {
            $(".companyName").text(this._companyName.trim());
        }
        else if (!this.contactrecordAvailable && this.caseContactRecordAvailable && (this._caseContactCompanyName != null || this._caseContactCompanyName != undefined)) {
            $(".companyName").text(this._caseContactCompanyName);
        }
        else {
            $(".companyName").text("Unknown");
        }

        if (!this.name) {
            fn = "Unknown(" + this._number + ")";
        }

        var input = {
            "sessionId": this.sessionId,
            "customer": fn
        };
        Microsoft.CIFramework.setSessionTitle(input);
        var sp = fn.split(" ");
        $(".callerInit").text(sp[0][0] + (sp[1] ? sp[1][0] : sp[0][1]));
    }


    /* Update the current name. If we only have a phone number, initiate a lookup into CRM */
    set name(value) {
        if (!value) {
            this._name = this._number = null;
            return;
        }
        if (value.startsWith("+")) {    //We have a phone number but no name
            this._number = value;
            //Utility.openContactRecord(this._number, true, this);
            log("Initiated number lookup");
        }
        else {
            this._name = value;
        }
    }

};

//// CreateCase Method on Click on CreateCase button.
function createCase() {

    Microsoft.CIFramework.getFocusedSession().then((sessionId) => {
        if (sessionslist.sessionmap) {
            var currentSession = sessionslist.sessionmap.get(sessionId);
            var languageId = null;
            var languageName = null;
            var callerCompanyId = null;
            var callerCompanyName = null;
            var callerName = null;
            if (currentSession != null || currentSession != undefined) {
                var number = currentSession._number;
                if (!number) {
                    return reject("number is empty");
                }
                if (currentSession.contactrecordAvailable) {
                    if (currentSession._languageId != null && currentSession._languageName != null) {
                        languageId = currentSession._languageId;
                        languageName = currentSession._languageName;
                    }
                    if (currentSession._companyId != null && currentSession._companyName != null) {
                        callerCompanyId = currentSession._companyId;
                        callerCompanyName = currentSession._companyName;
                    }
                    if (currentSession._name != null && currentSession._name != undefined && currentSession._name.toString().toUpperCase() != "UNKNOWN") {
                        callerName = currentSession._name;
                    }
                }
                else if (!currentSession.contactrecordAvailable && currentSession._caseContactLanguageId != null && currentSession._caseConLanguageName != null) {
                    languageId = currentSession._caseContactLanguageId;
                    languageName = currentSession._caseConLanguageName;

                    if (currentSession._caseContactName != null && currentSession._caseContactName.trim().toUpperCase() != "UNKNOWN") {
                        callerName = currentSession._caseContactName.trim();
                    }
                }

                var input = {
                    templateName: "msdyn_entityrecord",
                    templateParameters: {
                        entityName: "incident",
                        entityId: "",
                        data: {
                            "jarvis_callernameargus": callerName,
                            "jarvis_callerphone": currentSession.ExactCallingPartyNumber,
                            "jarvis_callerlanguage": languageId,
                            "jarvis_callerlanguagename": languageName,
                            "jarvis_callercompany": callerCompanyId,
                            "jarvis_callercompanyname": callerCompanyName
                        }
                    },
                    isFocused: true
                }
                Microsoft.CIFramework.createTab(input);
            }
        }
    });

}

//// Trigger on Session Swtich and Setting Caller Name and Company Name.
function onSessionSwitchHandler(sessionParam) {
    //Hide communciation Panel
    //setPanelMode(2);
    //Microsoft.CIFramework.setClickToAct(true);
    //Microsoft.CIFramework.addHandler("onclicktoact", clickToActHandler);
    Microsoft.CIFramework.setWidth(300);
    if (sessionParam == null || sessionParam == undefined) {
        return;
    }
    Microsoft.CIFramework.getFocusedSession().then(
        function success(result) {
            debugger;
            console.log(result);
            var currentSessionId = result;
            console.log(this.sessionslist.sessionmap);
            if (this.sessionslist.sessionmap) {
                var currentSessionDetail = sessionslist.sessionmap.get(currentSessionId);
                if ((currentSessionDetail != null || currentSessionDetail != undefined)) {
                    ////Setting Caller Name at Widget
                    if (currentSessionDetail.contactrecordAvailable && (currentSessionDetail._name != null || currentSessionDetail._name != undefined)) {
                        console.log(currentSessionDetail._name);
                        $(".callerName").text(currentSessionDetail._name);
                    }
                    else if (!currentSessionDetail.contactrecordAvailable && (currentSessionDetail._caseContactName != null || currentSessionDetail._caseContactName != undefined)) {
                        console.log(currentSessionDetail._caseContactName);
                        $(".callerName").text(currentSessionDetail._caseContactName.trim());
                    }
                    else {
                        $(".callerName").text("Unknown");
                    }
                    ////Setting Company Name at Widget
                    if (currentSessionDetail.contactrecordAvailable && (currentSessionDetail._companyName != null || currentSessionDetail._companyName != undefined)) {
                        console.log(currentSessionDetail._companyName);
                        $(".companyName").text(currentSessionDetail._companyName);
                    }
                    else if (!currentSessionDetail.contactrecordAvailable && (currentSessionDetail._caseContactCompanyName != null || currentSessionDetail._caseContactCompanyName != undefined)) {
                        console.log(currentSessionDetail._caseContactCompanyName);
                        $(".companyName").text(currentSessionDetail._caseContactCompanyName);
                    }
                    else {
                        $(".companyName").text("Unknown");
                    }
                    return;
                }
                else {
                    $(".callerName").text("Unknown");
                    $(".companyName").text("Unknown");
                    return;
                }
            }
            else {
                $(".callerName").text("Unknown");
                $(".companyName").text("Unknown");
                return;
            }
            // perform operations on session id value
        },
        function (error) {
            console.log(error.message);
            // handle error conditions
        }
    );
}

function onSessionSwitchHandler2(sessionParam) {

}

// Programatically set the panel mode using CIF
function setPanelMode(mode) {
    Microsoft.CIFramework.setMode(mode).then(function (val) {
        log("Successfully set the panel mode " + val);
    }).catch(function (reason) {
        log("Failed to set mode due to: " + reason);
    });
}

// Activity log
function log(message) {
    message = new Date().toString() + " " + message;
    console.log(message);
}

//Create Session
function createagentsession(msgJson) {
    var callingpartynumber = msgJson.CallingPartyNumber;
    var callid = msgJson.CallID;
    var callednumber = msgJson.CalledNumber;
    var instance = msgJson.Instance;
    var serviceroup = msgJson.ServiceGroup;
    var siteId = msgJson.siteId;
    var userId = msgJson.userId;
    var inputBag = {
        "templateName": "sample_CallSessionTemplate", "templateParameters": { customer: callingpartynumber }
    };
    Microsoft.CIFramework.createSession(inputBag).then(
        (sessionId) => {
            var sessionPh = new SessionInfo();
            sessionPh.userId = userId;
            sessionPh.siteId = siteId;
            sessionPh.ExactCallingPartyNumber = callingpartynumber;
            if (callingpartynumber) {
                let hasLeadingPlus = /^\+/.test(callingpartynumber);
                let hasLeadingDoubleZeros = /^00/.test(callingpartynumber);
                let hasLeadingSingleZero = /^0/.test(callingpartynumber);

                if (hasLeadingPlus) {
                    callingpartynumber = callingpartynumber.replace(/^\+/g, '');
                }
                else if (hasLeadingDoubleZeros) {
                    callingpartynumber = callingpartynumber.replace(/^00/, '');
                }
                else if (hasLeadingSingleZero) {
                    callingpartynumber = callingpartynumber.replace(/^0/, '');
                }
            }

            // Replacing + --> * , Leading '0' --> * and Leading '00' --> *
            // Open Related Records;
            //let searchNumber = '*' + callingpartynumber;
            Utility.openRelatedRecord(callingpartynumber);
            //// Open Related Record if having only one record matched.
            Utility.openContactRecord(callingpartynumber, false, sessionPh, sessionId);
            Utility.openBusinessPartnerRecord(callingpartynumber, false, sessionPh);
            Utility.openCaseRecord(callingpartynumber, false, sessionPh);
            Utility.openCaseContactRecord(callingpartynumber, false, sessionPh);
            ////End of Open Related Record.
            //setPanelMode(2);

        },
        (error) => {
        });
}

//// OnClick Call button.
function clickToActHandler(paramStr) {
    return new Promise(function (resolve, reject) {
        try {
            let params = JSON.parse(paramStr);
            getphonePrefix(params);
            //Make the call
            resolve(true);
        }
        catch (error) {
            reject(error);
        }
    });
}

//// To show Error Notification for connection established.
function showErrorNotification(msg) {
    var msgnotification =
    {
        type: 2,
        level: 3, //error
        message: msg,
        showCloseButton: true
    }

    Microsoft.CIFramework.showGlobalNotification(msgnotification).then(
        function success(result) {
            //window.setTimeout(function () {
            //    Microsoft.CIFramework.clearGlobalNotification(result);
            //}, 60000);
        },
        function (error) {
            console.log(error.message);
        });
}

//// To show Notification for connection established.
function showToastNotification(msgtitle, msgbody, userid, icontype) {
    systemuserid = userid.replace("{", "").replace("}", "");
    var data = {
        "title": msgtitle,
        "body": msgbody,
        "ownerid@odata.bind": "/systemusers(" + systemuserid + ")",
        "icontype": icontype,
        "toasttype": 200000000,
        "ttlinseconds": 60
    }

    var jsonData = JSON.stringify(data);
    Microsoft.CIFramework.createRecord("appnotification", jsonData).then(
        function success(result) {
        },
        function (error) {
            console.log(error.message);
        }
    );
}

//// To open Entity Record for  Case,Contact,BusinessPartner and CaseContact if only one exists
function openEntityRecord(templateName, entityName, entityId) {
    var input = {
        templateName: templateName,
        templateParameters: {
            entityName: entityName,
            entityId: entityId
        }
        // isFocused: true
    }
    //Microsoft.CIFramework.requestFocusSession(sessionId)
    Microsoft.CIFramework.createTab(input);
}

//// Phone Prefix logic for Outbound call.
function getphonePrefix(phoneControl) {
    var entityName = phoneControl.entityLogicalName;
    var controlName = phoneControl.name;
    var entityId = phoneControl.entityId;
    var phNo = phoneControl.value;

    var siteId = null;
    var prefix = null;
    Microsoft.CIFramework.getEnvironment().then(function (res) {
        var crmUserId = JSON.parse(res).userId;
        var query = "?$select=_siteid_value&$filter=(systemuserid eq " + crmUserId + ")";
        Microsoft.CIFramework.searchAndOpenRecords("systemuser", query, true).then(
            function (result) {
                var resp = JSON.parse(result);
                if (resp[0] != null) {
                    var siteId = resp[0]._siteid_value;
                }
                if (entityName == "incident") {
                    var incidentquery = "?$select=_jarvis_callercompany_value,_jarvis_homedealer_value,_jarvis_caseserviceline_value,jarvis_callerphone,jarvis_driverphone&$filter=(incidentid eq " + entityId + ")";
                    Microsoft.CIFramework.searchAndOpenRecords(entityName, incidentquery, true).then(
                        function (result) {
                            var resp = JSON.parse(result);
                            if (resp[0] != null) {
                                var callerCompany = resp[0]._jarvis_callercompany_value;
                                var homeDealer = resp[0]._jarvis_homedealer_value;
                                var serviceLine = resp[0]._jarvis_caseserviceline_value;
                                if (controlName == "jarvis_callerphone") {
                                    if (callerCompany != null) {
                                        phNo = resp[0].jarvis_callerphone;
                                        var icompanyquery = "?$select=_jarvis_country_value,_jarvis_address1_country_value&$filter=(accountid eq " + callerCompany + ")";
                                        Microsoft.CIFramework.searchAndOpenRecords("account", icompanyquery, true).then(
                                            function (result) {
                                                var resp = JSON.parse(result);
                                                if (resp[0] != null) {
                                                    var country = resp[0]._jarvis_country_value;
                                                    if (resp[0]._jarvis_country_value == null) {
                                                        country = resp[0]._jarvis_address1_country_value;
                                                    }
                                                    if (country != null) {
                                                        var prefixQuery = "?$select=jarvis_prefix&$filter=(_jarvis_serviceline_value eq " + serviceLine + ") and (_jarvis_site_value eq " + siteId + ") and (_jarvis_country_value eq " + country + ")";
                                                        Microsoft.CIFramework.searchAndOpenRecords("jarvis_phoneprefix", prefixQuery, true).then(
                                                            function (result) {
                                                                var resp = JSON.parse(result);
                                                                if (resp[0] != null && resp[0].jarvis_prefix != undefined && resp[0].jarvis_prefix != null) {
                                                                    placeCallWithPrefix(resp[0].jarvis_prefix, phNo);
                                                                }
                                                                else {
                                                                    placeCallWithoutPrefix(phNo);
                                                                }
                                                            });
                                                    }
                                                    else {
                                                        placeCallWithoutPrefix(phNo);
                                                    }
                                                }
                                                else {
                                                    placeCallWithoutPrefix(phNo);
                                                }
                                            });
                                    }
                                    else {
                                        placeCallWithoutPrefix(phNo);
                                    }
                                }
                                else if (controlName == "jarvis_driverphone") {
                                    if (homeDealer != null) {
                                        var icompanyquery = "?$select=_jarvis_country_value,_jarvis_address1_country_value&$filter=(accountid eq " + homeDealer + ")";
                                        var country = null;
                                        phNo = resp[0].jarvis_driverphone;
                                        Microsoft.CIFramework.searchAndOpenRecords("account", icompanyquery, true).then(
                                            function (result) {
                                                var resp = JSON.parse(result);
                                                if (resp[0] != null) {
                                                    if (resp[0]._jarvis_country_value != null) {
                                                        country = resp[0]._jarvis_country_value;
                                                    }
                                                    else
                                                        country = resp[0]._jarvis_address1_country_value;
                                                    if (country != null) {
                                                        var prefixQuery = "?$select=jarvis_prefix&$filter=(_jarvis_serviceline_value eq " + serviceLine + ") and (_jarvis_site_value eq " + siteId + ") and (_jarvis_country_value eq " + country + ")";
                                                        Microsoft.CIFramework.searchAndOpenRecords("jarvis_phoneprefix", prefixQuery, true).then(
                                                            function (result) {
                                                                var resp = JSON.parse(result);
                                                                if (resp[0] != null && resp[0].jarvis_prefix != undefined && resp[0].jarvis_prefix != null) {
                                                                    placeCallWithPrefix(resp[0].jarvis_prefix, phNo);
                                                                }
                                                                else {
                                                                    placeCallWithoutPrefix(phNo);
                                                                }
                                                            });
                                                    }
                                                    else {
                                                        placeCallWithoutPrefix(phNo);
                                                    }
                                                }
                                                else {
                                                    placeCallWithoutPrefix(phNo);
                                                }
                                            });
                                    }
                                    else {
                                        placeCallWithoutPrefix(phNo);
                                    }
                                }
                            }
                            else {
                                placeCallWithoutPrefix(phNo);
                            }
                        });
                }

                else if (entityName == "jarvis_casecontact") {
                    var casecontactQuery = "?$select=_jarvis_incident_value,jarvis_mobilephone,jarvis_phone,jarvis_role&$filter=(jarvis_casecontactid eq " + entityId + ")";
                    Microsoft.CIFramework.searchAndOpenRecords(entityName, casecontactQuery, true).then(
                        function (result) {
                            var resp = JSON.parse(result);
                            if (resp[0] != null && resp[0]._jarvis_incident_value != null && resp[0].jarvis_mobilephone != null && resp[0].jarvis_role != null) {
                                var incidentSourceId = resp[0]._jarvis_incident_value;
                                var role = resp[0].jarvis_role;
                                if (controlName == "jarvis_mobilephone") {
                                    phNo = resp[0].jarvis_mobilephone;
                                }
                                else if (controlName == "jarvis_phone") {
                                    phNo = resp[0].jarvis_phone;
                                }
                                var incidentquery = "?$select=_jarvis_callercompany_value,_jarvis_homedealer_value,_jarvis_caseserviceline_value&$filter=(incidentid eq " + incidentSourceId + ")";
                                Microsoft.CIFramework.searchAndOpenRecords("incident", incidentquery, true).then(
                                    function (result) {
                                        var resp = JSON.parse(result);
                                        if (resp[0] != null) {
                                            var callerCompany = resp[0]._jarvis_callercompany_value;
                                            var homeDealer = resp[0]._jarvis_homedealer_value;
                                            var serviceLine = resp[0]._jarvis_caseserviceline_value
                                            if (callerCompany != null || homeDealer != null) {
                                                if (role == 334030001) {
                                                    accountFilter = callerCompany;
                                                }
                                                else {
                                                    accountFilter = homeDealer;
                                                }
                                                var icompanyquery = "?$select=_jarvis_country_value,_jarvis_address1_country_value&$filter=(accountid eq " + accountFilter + ")";
                                                Microsoft.CIFramework.searchAndOpenRecords("account", icompanyquery, true).then(
                                                    function (result) {
                                                        var resp = JSON.parse(result);
                                                        if (resp[0] != null) {
                                                            var country = resp[0]._jarvis_country_value;
                                                            if (resp[0]._jarvis_country_value == null) {
                                                                country = resp[0]._jarvis_address1_country_value;
                                                            }
                                                            if (country != null) {
                                                                var prefixQuery = "?$select=jarvis_prefix&$filter=(_jarvis_serviceline_value eq " + serviceLine + ") and (_jarvis_site_value eq " + siteId + ") and (_jarvis_country_value eq " + country + ")";
                                                                Microsoft.CIFramework.searchAndOpenRecords("jarvis_phoneprefix", prefixQuery, true).then(
                                                                    function (result) {
                                                                        var resp = JSON.parse(result);
                                                                        if (resp[0] != null && resp[0].jarvis_prefix != undefined && resp[0].jarvis_prefix != null) {
                                                                            placeCallWithPrefix(resp[0].jarvis_prefix, phNo);
                                                                        }
                                                                        else {
                                                                            placeCallWithoutPrefix(phNo);
                                                                        }
                                                                    });
                                                            }
                                                            else {
                                                                placeCallWithoutPrefix(phNo);
                                                            }
                                                        }
                                                        else {
                                                            placeCallWithoutPrefix(phNo);
                                                        }
                                                    });
                                            }
                                            else {
                                                placeCallWithoutPrefix(phNo);
                                            }
                                        }
                                        else {
                                            placeCallWithoutPrefix(phNo);
                                        }
                                    });
                            }
                            else {
                                placeCallWithoutPrefix(phNo);
                            }

                        });
                }

                else {
                    placeCallWithoutPrefix(phNo);
                }
            });
    });


}

//// Place Call By Removing Space, + --> 00 and Adding Prefix if extracted
function placeCallWithPrefix(prefix, phNo) {
    phNo = phNo.replace(/\s/g, "");
    phNo = phNo.replace('+', '00');
    callto = prefix + phNo;
    window.open("tel:" + callto);
}
//// Place Call By Removing Space, + --> 00
function placeCallWithoutPrefix(phNo) {

    phNo = phNo.replace(/\s/g, "");
    phNo = phNo.replace('+', '00');
    //callto = '0' + phNo;
    callto = phNo;
    window.open("tel:" + callto);

}

// Initializes the CIF handlers 
$(function () {
    sessionslist = new SessionList();
    Microsoft.CIFramework.setClickToAct(true);
    Microsoft.CIFramework.addHandler("onclicktoact", clickToActHandler);
    Microsoft.CIFramework.addHandler("onSessionSwitched", onSessionSwitchHandler);
});

