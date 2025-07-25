//-----------------------------------------------------------------------
// <copyright file="fs_Helper.js" company="Microsoft">
//     This file has copirights
// </copyright>
//-----------------------------------------------------------------------

/*jslint browser:true,maxlen:500,white:true,for:true,this:true,single:true*/
/*global Xrm,XMLHttpRequest,console,parent,break,fieldName,value,window,escape*/
/*property
    executeUpdateSync,namespace,Helper,checkIfNullOrUndefined,getUserId,Utility,getGlobalContext,userSettings,userId,toString,replace,getFormContext,Page,getRibbonContext,disableAllFieldsFromList,disableAllFieldsFromList
    data,entity,attributes,forEach,getControl,getName,indexOf,setDisabled,disableAllFieldsExceptExclusionList,setEnableDisableAllFieldsExceptExclusionList,checkFieldIsPresent,getAttribute,checkFieldHasValue,getValue,getFieldValue,getOptionSetFieldValue
    Text,getText,Value,getLookUpFieldValue,id,name,entityType,ui,quickForms,getQuickFormFieldValue,getQuickFormLookUpFieldValue,setFieldNotification,setNotification,get,setVisible,tabs,setDisplayState,getOptionsetTextKey,getDateTextKey,setRequestHeaders,setRequestHeader
    clearFieldNotification,clearNotification,setFieldValue,setValue,setOptionSetValueByText,getOptions,length,text,value,setLookupFieldValue,setFieldsRequiredLevel,setRequiredLevel,setFieldsEnableDisable
    formatDate,getFullYear,substr,getMonth,getDate,getHours,getMinutes,getDatesDifferenceInDays,getTime,round,compareDates,setFieldsVisibility,setTabsExpandCollapse,setTabsVisibility,setSectionsVisibility,executeFetchRecordByIdSync,executeCustomActionAsync
    sections,formatPhone,translateMask,translatePhoneLetter,charAt,toUpperCase,getContext,parent,Xrm,context,getClientUrl,getWebAPIPath,getErrorMessage,response,parse,error,message,prepareErrorLog,type,stacktrace,getOptionSetTextKey
    innererror,responseText,status,statusText,errorHandler,stringParameterCheck,callbackParameterCheck,getAliasKey,getAliasTextKey,getLookupIdKey,getLookupTextKey,key,open,onreadystatechange,statuscode,result,errorlog,errorMessage,send,stringify,getResponseHeader,entityid
    executeDeleteSync,readyState,statusCode,errorLog,executeCreateSync,executeCreateAsync,executeUpdateAsync,executeCustomActionSync,executeCustomActionASync,executeFetchXmlSync,executeFetchXmlSyncMoreRecords,push,executeFetchByQuerySync,
*/

var Jarvis = Jarvis || { namespace: true };
Jarvis.Helper = Jarvis.Helper || { namespace: true };

Jarvis.Helper = {
    ///<summary>
    /// check if input is null or undefined
    ///</summary>
    checkIfNullOrUndefined: function (obj) {
        "use strict";
        if ((obj === undefined) || (obj === null)) {
            return true;
        }
        return false;
    },
    ///<summary>
    /// get Current User Id
    ///</summary>
    getUserId: function () {
        "use strict";
        return Xrm.Utility.getGlobalContext().userSettings.userId.toString().replace("{", "").replace("}", "");
    },
    ///<summary>
    /// get Form context
    ///</summary>
    getFormContext: function (executionContext) {
        "use strict";
        var result = null;
        if (!Jarvis.Helper.checkIfNullOrUndefined(executionContext)) {
            var formContext = executionContext.getFormContext();
            if (!Jarvis.Helper.checkIfNullOrUndefined(formContext)) {
                result = formContext;
            }
        }
        return result;
    },
    ///<summary>
    /// get Ribbon Context
    ///</summary>
    getRibbonContext: function (primaryControl) {
        "use strict";
        var result = null;
        if (!Jarvis.Helper.checkIfNullOrUndefined(primaryControl)) {
            result = primaryControl;
        }
        return result;
    },
    ///<summary>
    /// disable all fields on form mentioned in exclusion list - To Remove
    ///</summary>
    disableAllFieldsFromList: function (formContext, disableList) {
        "use strict";
        formContext.data.entity.attributes.forEach(function (attribute) {
            var control = formContext.getControl(attribute.getName());
            var fieldName = attribute.getName();
            if (disableList.indexOf(fieldName) !== -1 && !Jarvis.Helper.checkIfNullOrUndefined(control)) {
                control.setDisabled(true);
            }
        });
    },
    ///<summary>
    /// disable all fields on form except fields mentioned in exclusion list - To Remove
    ///</summary>
    disableAllFieldsExceptExclusionList: function (formContext, exclusionList) {
        "use strict";
        formContext.data.entity.attributes.forEach(function (attribute) {
            var control = formContext.getControl(attribute.getName());
            var fieldName = attribute.getName();
            if (exclusionList.indexOf(fieldName) === -1 && !Jarvis.Helper.checkIfNullOrUndefined(control)) {
                control.setDisabled(true);
            }
        });
    },
    ///<summary>
    /// set enable disable all fields on form except fields mentioned in exclusion list
    ///</summary>
    setEnableDisableAllFieldsExceptExclusionList: function (formContext, exclusionList, toDisable) {
        "use strict";
        formContext.data.entity.attributes.forEach(function (attribute) {
            var control = formContext.getControl(attribute.getName());
            var fieldName = attribute.getName();
            if (exclusionList.indexOf(fieldName) === -1 && !Jarvis.Helper.checkIfNullOrUndefined(control)) {
                control.setDisabled(toDisable);
            }
        });
    },

    ///<summary>
    /// set required level for all fields on form except fields mentioned in exclusion list
    ///</summary>
    setRequiredLevelAllFieldsExceptExclusionList: function (formContext, exclusionList, validateRequiredLevel, setRequiredlevel) {
        "use strict";
        formContext.data.entity.attributes.forEach(function (attribute) {
            if (attribute.getRequiredLevel() === validateRequiredLevel) {
                var control = formContext.getControl(attribute.getName());
                var fieldName = attribute.getName();
                if (exclusionList.indexOf(fieldName) === -1 && !Jarvis.Helper.checkIfNullOrUndefined(control)) {
                    formContext.getAttribute(fieldName).setRequiredLevel(setRequiredlevel);
                }
            }
        });
    },

    ///<summary>
    /// check if field is present on form
    ///</summary>
    checkFieldIsPresent: function (formContext, fieldName) {
        "use strict";
        return !Jarvis.Helper.checkIfNullOrUndefined(formContext.getAttribute(fieldName));
    },
    ///<summary>
    /// check if field has value
    ///</summary>
    checkFieldHasValue: function (formContext, fieldName) {
        "use strict";
        return (Jarvis.Helper.checkFieldIsPresent(formContext, fieldName) && !Jarvis.Helper.checkIfNullOrUndefined(formContext.getAttribute(fieldName).getValue()));
    },
    ///<summary>
    /// get field value
    ///</summary>
    getFieldValue: function (formContext, fieldName) {
        "use strict";
        var result = null;
        if (Jarvis.Helper.checkFieldHasValue(formContext, fieldName)) {
            result = formContext.getAttribute(fieldName).getValue();
        }
        return result;
    },
    ///<summary>
    /// get option set field value
    ///</summary>
    getOptionSetFieldValue: function (formContext, fieldName) {
        "use strict";
        var result = null;
        if (Jarvis.Helper.checkFieldHasValue(formContext, fieldName)) {
            result = {};
            result.Text = formContext.getAttribute(fieldName).getText();
            result.Value = formContext.getAttribute(fieldName).getValue();
        }
        return result;
    },
    ///<summary>
    /// set look up field value
    ///</summary>
    getLookUpFieldValue: function (formContext, fieldName) {
        "use strict";
        var result = null;
        var lookUpControl = null;
        if (Jarvis.Helper.checkFieldHasValue(formContext, fieldName)) {
            lookUpControl = formContext.getAttribute(fieldName).getValue();
        }
        if (lookUpControl !== null && lookUpControl[0] !== null) {
            result = {};
            result.id = lookUpControl[0].id;
            result.name = lookUpControl[0].name;
            result.entityType = lookUpControl[0].entityType;
        }
        return result;
    },
    ///<summary>
    /// set value of field within quick view form
    ///</summary>
    getQuickFormFieldValue: function (formContext, formName, fieldName) {
        "use strict";
        var quickViewControl = formContext.ui.quickForms.get(formName).getControl(fieldName);
        return quickViewControl.getAttribute().getValue();
    },
    ///<summary>
    /// set value of look up field within quick view form
    ///</summary>
    getQuickFormLookUpFieldValue: function (formContext, formName, fieldName) {
        "use strict";
        var result = {};
        var quickViewControl = formContext.ui.quickForms.get(formName).getControl(fieldName);
        var lookupField = quickViewControl.getAttribute().getValue();
        if (lookupField !== null && lookupField[0] !== null) {
            result.id = lookupField[0].id;
            result.name = lookupField[0].name;
            result.entityType = lookupField[0].entityType;
        }
        return result;
    },
    ///<summary>
    /// set visibility of fields
    ///</summary>
    setFieldNotification: function (formContext, controlName, message) {
        "use strict";
        formContext.getControl(controlName).setNotification(message);
    },
    ///<summary>
    /// clear field level notification
    ///</summary>
    clearFieldNotification: function (formContext, controlName) {
        "use strict";
        formContext.getControl(controlName).clearNotification();
    },
    ///<summary>
    /// set field value
    ///</summary>
    setFieldValue: function (formContext, fieldName, value) {
        "use strict";
        if (Jarvis.Helper.checkFieldIsPresent(formContext, fieldName)) {
            formContext.getAttribute(fieldName).setValue(value);
        }
    },
    ///<summary>
    /// set option set by option text
    ///</summary>
    setOptionSetValueByText: function (formContext, fieldName, optionText) {
        "use strict";
        if (Jarvis.Helper.checkFieldIsPresent(formContext, fieldName)) {
            var i = 0;
            var options = formContext.getAttribute(fieldName).getOptions();
            for (i = 0; i < options.length; i += 1) {
                if (options[i].text === optionText) {
                    formContext.getAttribute(fieldName).setValue(options[i].value);
                }
            }
        }
    },
    ///<summary>
    /// set look up field value
    ///</summary>
    setLookupFieldValue: function (formContext, fieldName, entityType, id, name) {
        "use strict";
        if (Jarvis.Helper.checkFieldIsPresent(formContext, fieldName)) {
            var lookupReference = [];
            lookupReference[0] = {};
            lookupReference[0].id = id;
            lookupReference[0].entityType = entityType;
            lookupReference[0].name = name;
            formContext.getAttribute(fieldName).setValue(lookupReference);
        }
    },
    ///<summary>
    /// set requirement level of fields
    ///</summary>
    setFieldsRequiredLevel: function (formContext, listFieldNames, requirementLevel) {
        "use strict";
        if (listFieldNames !== null && listFieldNames.length > 0) {
            var i = 0;
            for (i = 0; i < listFieldNames.length; i += 1) {
                if (Jarvis.Helper.checkFieldIsPresent(formContext, listFieldNames[i])) {
                    formContext.getAttribute(listFieldNames[i]).setRequiredLevel(requirementLevel);
                }
            }
        }
    },
    ///<summary>
    /// set enable disable of fields
    ///</summary>
    setFieldsEnableDisable: function (formContext, listFieldNames, isDisable) {
        "use strict";
        if (listFieldNames !== null && listFieldNames.length > 0) {
            var i = 0;
            for (i = 0; i < listFieldNames.length; i += 1) {
                if (Jarvis.Helper.checkFieldIsPresent(formContext, listFieldNames[i])) {
                    formContext.getControl(listFieldNames[i]).setDisabled(isDisable);
                }
            }
        }
    },

    ///<summary>
    /// set Section enable or disable of fields
    ///</summary>
    setSectionDisabled: function (formContext, tabName, sectionName, disablestatus) {
        "use strict";
        var section = formContext.ui.tabs.get(tabName).sections.get(sectionName);
        var controls = section.controls.get();
        var controlsLength = controls.length;
        for (var i = 0; i < controlsLength; i++) {
            controls[i].setDisabled(disablestatus)
        }
    },

    ///<summary>
    /// set visibility of fields
    ///</summary>
    setFieldsVisibility: function (formContext, listFieldNames, isVisible) {
        "use strict";
        if (listFieldNames !== null && listFieldNames.length > 0) {
            var i = 0;
            for (i = 0; i < listFieldNames.length; i += 1) {
                if (Jarvis.Helper.checkFieldIsPresent(formContext, listFieldNames[i])) {
                    formContext.getControl(listFieldNames[i]).setVisible(isVisible);
                }
            }
        }
    },
    ///<summary>
    /// set expand/collapse Tabs
    ///</summary>
    setTabsExpandCollapse: function (formContext, listTabNames, displayState) {
        "use strict";
        if (listTabNames !== null && listTabNames.length > 0) {
            var i = 0;
            for (i = 0; i < listTabNames.length; i += 1) {
                if (!Jarvis.Helper.checkIfNullOrUndefined(formContext.ui.tabs.get(listTabNames[i]))) {
                    formContext.ui.tabs.get(listTabNames[i]).setDisplayState(displayState);
                }
            }
        }
    },
    ///<summary>
    /// set visibility of Tabs
    ///</summary>
    setTabsVisibility: function (formContext, listTabNames, isVisible) {
        "use strict";
        if (listTabNames !== null && listTabNames.length > 0) {
            var i = 0;
            for (i = 0; i < listTabNames.length; i += 1) {
                if (!Jarvis.Helper.checkIfNullOrUndefined(formContext.ui.tabs.get(listTabNames[i]))) {
                    formContext.ui.tabs.get(listTabNames[i]).setVisible(isVisible);
                }
            }
        }
    },
    ///<summary>
    /// set visibility of Tab's Sections
    ///</summary>
    setSectionsVisibility: function (formContext, tabName, listSectionNames, isVisible) {
        "use strict";
        if (listSectionNames !== null && listSectionNames.length > 0) {
            var i = 0;
            for (i = 0; i < listSectionNames.length; i += 1) {
                if (!Jarvis.Helper.checkIfNullOrUndefined(formContext.ui.tabs.get(tabName))) {
                    if (!Jarvis.Helper.checkIfNullOrUndefined(formContext.ui.tabs.get(tabName).sections.get(listSectionNames[i]))) {
                        formContext.ui.tabs.get(tabName).sections.get(listSectionNames[i]).setVisible(isVisible);
                    }
                }
            }
        }
    },
    ///<summary>
    /// format date to string as per format
    ///</summary>
    formatDate: function (date, format) {
        "use strict";
        var result = format.toString();
        var yyyy = date.getFullYear().toString();
        var yy = yyyy.substr(2);
        result = result.replace("yyyy", yyyy);
        result = result.replace("yy", yy);

        var M = (date.getMonth() + 1).toString();
        var MM = M;
        if (M < 10) {
            MM = "0" + M;
        }
        result = result.replace("MM", MM);
        result = result.replace("M", M);

        var d = date.getDate().toString();
        var dd = d;
        if (d < 10) {
            dd = "0" + d;
        }
        result = result.replace("dd", dd);
        result = result.replace("d", d);

        var H = date.getHours().toString();
        var HH = H;
        if (H < 10) {
            HH = "0" + H;
        }
        result = result.replace("HH", HH);
        result = result.replace("H", H);

        var hours = date.getHours();
        var ampm = hours >= 12 ? "PM" : "AM";
        hours = hours % 12;
        hours = (hours === 0 && ampm === "PM") ? 12 : hours; // the hour '0' should be '12'
        var h = hours.toString();
        var hh = h;
        if (h < 10) {
            hh = "0" + h;
        }
        result = result.replace("hh", hh);
        result = result.replace("h", h);
        result = result.replace("ap", ampm);

        var m = date.getMinutes().toString();
        var mm = m;
        if (m < 10) {
            mm = "0" + m;
        }
        result = result.replace("mm", mm);
        result = result.replace("m", m);

        var s = date.getHours().toString();
        var ss = s;
        if (s < 10) {
            ss = "0" + s;
        }
        result = result.replace("ss", ss);
        result = result.replace("s", s);
        return result;
    },
    ///<summary>
    /// get no of days difference between two date only values
    ///</summary>
    getDatesDifferenceInDays: function (date1, date2) {
        "use strict";
        var date1_dt = new Date(date1.getFullYear(), date1.getMonth(), date1.getDate(), 0, 0, 0, 0);
        var date2_dt = new Date(date2.getFullYear(), date2.getMonth(), date2.getDate(), 0, 0, 0, 0);
        //Get 1 day in milliseconds
        var one_day = 1000 * 60 * 60 * 24;

        // Convert both dates to milliseconds
        var date1_ms = date1_dt.getTime();
        var date2_ms = date2_dt.getTime();

        // Calculate the difference in milliseconds
        var difference_ms = date2_ms - date1_ms;

        // Convert back to days and return
        return Math.round(difference_ms / one_day);
    },
    ///<summary>
    /// compare two date only values
    ///</summary>
    compareDates: function (date1, date2) {
        "use strict";
        var result = null;
        var date1_dt = new Date(date1.getFullYear(), date1.getMonth(), date1.getDate(), 0, 0, 0, 0);
        var date2_dt = new Date(date2.getFullYear(), date2.getMonth(), date2.getDate(), 0, 0, 0, 0);
        if (date1_dt.getTime() < date2_dt.getTime()) {
            result = -1;
        } else if (date1_dt.getTime() > date2_dt.getTime()) {
            result = 1;
        } else if (date1_dt.getTime() === date2_dt.getTime()) {
            result = 0;
        }
        return result;
    },


    ///<summary>
    /// get context object.
    ///</summary>
    ///<returns>Context</returns>
    getContext: function () {
        "use strict";
        var oContext = null;
        if (Xrm !== undefined) {
            oContext = Xrm.Utility.getGlobalContext();
        } else {
            throw new Error("Context is not available.");
        }
        return oContext;
    },

    ///<summary>
    /// get server URL from the context
    ///</summary>
    ///<returns>String</returns>
    getClientUrl: function () {
        "use strict";
        return Jarvis.Helper.getContext().getClientUrl();
    },

    ///<summary>
    /// get path to the REST endpoint.
    ///</summary>
    ///<returns>String</returns>
    getWebAPIPath: function () {
        "use strict";
        return Jarvis.Helper.getClientUrl() + "/api/data/v9.0/";
    },

    ///<summary>
    /// get Error message from request
    ///</summary>
    ///<param name="req" type="XMLHttpRequest">
    /// The XMLHttpRequest response that returned an error.
    ///</param>
    ///<returns>Error message</returns>
    getErrorMessage: function (req) {
        "use strict";
        var errorText = "";
        try {
            if (!Jarvis.Helper.checkIfNullOrUndefined(req) && !Jarvis.Helper.checkIfNullOrUndefined(req.response)) {
                var jsonResponse = JSON.parse(req.response);
                if (!Jarvis.Helper.checkIfNullOrUndefined(jsonResponse) && !Jarvis.Helper.checkIfNullOrUndefined(jsonResponse.error)) {
                    errorText = jsonResponse.error.message;
                }
            }
        } catch (ignore) {
            errorText = "Error";
        }
        return errorText;
    },

    ///<summary>
    /// get Error Log for failed request
    ///</summary>
    ///<param name="req" type="XMLHttpRequest">
    /// The XMLHttpRequest response that returned an error.
    ///</param>
    ///<returns>Error text</returns>
    prepareErrorLog: function (req) {
        "use strict";
        var errorText = "";
        try {
            if (!Jarvis.Helper.checkIfNullOrUndefined(req) && !Jarvis.Helper.checkIfNullOrUndefined(req.response)) {
                var jsonResponse = JSON.parse(req.response);
                if (!Jarvis.Helper.checkIfNullOrUndefined(jsonResponse) && !Jarvis.Helper.checkIfNullOrUndefined(jsonResponse.error)) {
                    var errorObj = jsonResponse.error;
                    errorText += errorObj.message;
                    if (!Jarvis.Helper.checkIfNullOrUndefined(errorObj.innererror)) {
                        errorObj = errorObj.innererror;
                        if (!Jarvis.Helper.checkIfNullOrUndefined(errorObj.message)) {
                            errorText += "\nInner Message: " + errorObj.message;
                        }
                        if (!Jarvis.Helper.checkIfNullOrUndefined(errorObj.type)) {
                            errorText += "\nInner Type: " + errorObj.type;
                        }
                        if (!Jarvis.Helper.checkIfNullOrUndefined(errorObj.stacktrace)) {
                            errorText += "\nInner stacktrace: " + errorObj.stacktrace;
                        }
                    }
                }
            }
        } catch (ignore) {
            errorText = req.responseText;
        }
        var errormessage = "Error : " + req.status + ": " + req.statusText + ": " + errorText;
        return errormessage;
    },

    ///<summary>
    /// handle failed request and return custom Error object for failed request
    ///</summary>
    ///<param name="req" type="XMLHttpRequest">
    /// The XMLHttpRequest response that returned an error.
    ///</param>
    ///<returns>Error</returns>
    errorHandler: function (req) {
        "use strict";
        var errormessage = Jarvis.Helper.prepareErrorLog(req);
        return new Error(errormessage);
    },

    ///<summary>
    /// check whether parameter is of type string
    ///</summary>
    ///<param name="parameter" type="String">
    /// The string parameter to check;
    ///</param>
    ///<param name="message" type="String">
    /// The error message text to include when the error is thrown.
    ///</param>
    stringParameterCheck: function (parameter, message) {
        "use strict";
        if (typeof parameter !== "string") {
            throw new Error(message);
        }
    },

    ///<summary>
    /// check whether parameter is of type function
    ///</summary>
    ///<param name="callbackParameter" type="Function">
    /// The callback parameter to check;
    ///</param>
    ///<param name="message" type="String">
    /// The error message text to include when the error is thrown.
    ///</param>
    callbackParameterCheck: function (callbackParameter, message) {
        "use strict";
        if (typeof callbackParameter !== "function") {
            throw new Error(message);
        }
    },

    ///<summary>
    /// get the alias attribute Key
    ///</summary>
    getAliasKey: function (attributeName, alias) {
        "use strict";
        //return alias + "_x002e_" + attributeName; // V8.2
        return alias + "." + attributeName;
    },

    ///<summary>
    /// get the alias attribute Text Key
    ///</summary>
    getAliasTextKey: function (attributeName, alias) {
        "use strict";
        return alias + "." + attributeName + "@OData.Community.Display.V1.FormattedValue";
    },

    ///<summary>
    /// get the lookup id attribute name
    ///</summary>
    getLookupIdKey: function (attributeName) {
        "use strict";
        return "_" + attributeName + "_value";
    },

    ///<summary>
    /// get the lookup text attribute name
    ///</summary>
    getLookupTextKey: function (attributeName) {
        "use strict";
        return "_" + attributeName + "_value@OData.Community.Display.V1.FormattedValue";
    },

    ///<summary>
    /// get the optionset text attribute name
    ///</summary>
    getOptionsetTextKey: function (attributeName) {
        "use strict";
        return attributeName + "@OData.Community.Display.V1.FormattedValue";
    },

    ///<summary>
    /// get the Date field text attribute name
    ///</summary>
    getDateTextKey: function (attributeName) {
        "use strict";
        return attributeName + "@OData.Community.Display.V1.FormattedValue";
    },

    ///<summary>
    /// Set request headers
    ///</summary>
    setRequestHeaders: function (req, includePrefer, customHeaders) {
        "use strict";
        req.setRequestHeader("Accept", "application/json");
        req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
        req.setRequestHeader("OData-MaxVersion", "4.0");
        req.setRequestHeader("OData-Version", "4.0");
        if (includePrefer === true) {
            req.setRequestHeader("Prefer", "odata.include-annotations=\"*\"");
        }
        if (!Jarvis.Helper.checkIfNullOrUndefined(customHeaders) && customHeaders.length > 0) {
            var i = 0;
            for (i = 0; i < customHeaders.length; i += 1) {
                req.setRequestHeader(customHeaders[i].key, customHeaders[i].value);
            }
        }
    },

    ///<summary>
    /// execute Delete Request Sync
    ///</summary>
    executeDeleteSync: function (entityId, entityCollectionName, customHeaders) {
        "use strict";
        var response = null;
        if (!Jarvis.Helper.checkIfNullOrUndefined(entityId)) {
            response = {};
            Jarvis.Helper.stringParameterCheck(entityId, "Jarvis.Helper.executeDeleteSync requires the entityId parameter of type string.");
            Jarvis.Helper.stringParameterCheck(entityCollectionName, "Jarvis.Helper.executeDeleteSync requires the entityCollectionName parameter of type string.");

            entityId = entityId.replace("{", "").replace("}", "");
            var query = entityCollectionName + "(" + entityId + ")";

            var req = new XMLHttpRequest();
            req.open("DELETE", Jarvis.Helper.getWebAPIPath() + query, false);
            Jarvis.Helper.setRequestHeaders(req, false, customHeaders);
            req.onreadystatechange = function () {
                if (this.readyState === 4) {
                    req.onreadystatechange = null;
                    switch (this.status) {
                        case 204:
                            response.statusCode = 204;
                            response.result = null;
                            break;
                        default:
                            response.statusCode = this.status;
                            response.errorLog = Jarvis.Helper.prepareErrorLog(this);
                            response.errorMessage = Jarvis.Helper.getErrorMessage(this);
                    }
                }
            };
            req.send();
        }
        return response;
    },

    ///<summary>
    /// execute Create Request Sync
    ///</summary>
    executeCreateSync: function (entity, entityCollectionName, customHeaders) {
        "use strict";
        var response = null;
        if (!Jarvis.Helper.checkIfNullOrUndefined(entity)) {
            response = {};
            Jarvis.Helper.stringParameterCheck(entityCollectionName, "Jarvis.Helper.executeCreateSync requires the entityCollectionName parameter of type string.");
            var query = entityCollectionName;
            var req = new XMLHttpRequest();
            req.open("POST", Jarvis.Helper.getWebAPIPath() + query, false);
            Jarvis.Helper.setRequestHeaders(req, false, customHeaders);
            req.onreadystatechange = function () {
                if (this.readyState === 4) {
                    req.onreadystatechange = null;
                    switch (this.status) {
                        case 204:
                            response.statusCode = 204;
                            response.result = null;
                            break;
                        default:
                            response.statusCode = this.status;
                            response.errorLog = Jarvis.Helper.prepareErrorLog(this);
                            response.errorMessage = Jarvis.Helper.getErrorMessage(this);
                    }
                }
            };
            req.send(JSON.stringify(entity));
            var responseHeader = req.getResponseHeader("OData-EntityId");
            if (!Jarvis.Helper.checkIfNullOrUndefined(responseHeader)) {
                var entityId = responseHeader.replace(Jarvis.Helper.getWebAPIPath() + query, "");
                response.entityid = entityId.substr(1, entityId.length - 2);
            }
        }
        return response;
    },

    ///<summary>
    /// execute Create Request Async
    ///</summary>
    executeCreateAsync: function (entity, entityCollectionName, customHeaders, successCallback, errorCallback) {
        "use strict";
        if (!Jarvis.Helper.checkIfNullOrUndefined(entity)) {
            Jarvis.Helper.stringParameterCheck(entityCollectionName, "Jarvis.Helper.executeCreateAsync requires the entityCollectionName parameter of type string.");
            Jarvis.Helper.callbackParameterCheck(successCallback, "Jarvis.Helper.executeCreateAsync requires successCallback of type function.");
            Jarvis.Helper.callbackParameterCheck(errorCallback, "Jarvis.Helper.executeCreateAsync requires errorCallback of type function.");
            var query = entityCollectionName;
            var req = new XMLHttpRequest();
            req.open("POST", Jarvis.Helper.getWebAPIPath() + query, true);
            Jarvis.Helper.setRequestHeaders(req, false, customHeaders);
            req.onreadystatechange = function () {
                if (this.readyState === 4) {
                    req.onreadystatechange = null;
                    var response = {};
                    switch (this.status) {
                        case 204:
                            response.statusCode = 204;
                            response.result = null;
                            successCallback(response);
                            break;
                        default:
                            response.statusCode = this.status;
                            response.errorLog = Jarvis.Helper.prepareErrorLog(this);
                            response.errorMessage = Jarvis.Helper.getErrorMessage(this);
                            errorCallback(response);
                    }
                }
            };
            req.send(JSON.stringify(entity));
        }
    },

    ///<summary>
    /// execute Update Request Sync
    ///</summary>
    executeUpdateSync: function (entity, entityId, entityCollectionName, customHeaders) {
        "use strict";
        var response = null;
        if (!Jarvis.Helper.checkIfNullOrUndefined(entity)) {
            response = {};
            Jarvis.Helper.stringParameterCheck(entityId, "Jarvis.Helper.executeUpdateSync requires the entityId parameter of type string.");
            Jarvis.Helper.stringParameterCheck(entityCollectionName, "Jarvis.Helper.executeUpdateSync requires the entityCollectionName parameter of type string.");

            entityId = entityId.replace("{", "").replace("}", "");
            var query = entityCollectionName + "(" + entityId + ")";

            var req = new XMLHttpRequest();
            req.open("PATCH", Jarvis.Helper.getWebAPIPath() + query, false);
            Jarvis.Helper.setRequestHeaders(req, false, customHeaders);
            req.onreadystatechange = function () {
                if (this.readyState === 4) {
                    req.onreadystatechange = null;
                    switch (this.status) {
                        case 204:
                            response.statusCode = 204;
                            response.result = null;
                            break;
                        default:
                            response.statusCode = this.status;
                            response.errorLog = Jarvis.Helper.prepareErrorLog(this);
                            response.errorMessage = Jarvis.Helper.getErrorMessage(this);
                    }
                }
            };
            req.send(JSON.stringify(entity));
        }
        return response;
    },

    ///<summary>
    /// execute Update Request Async
    ///</summary>
    executeUpdateAsync: function (entity, entityId, entityCollectionName, successCallback, errorCallback, customHeaders) {
        "use strict";
        if (!Jarvis.Helper.checkIfNullOrUndefined(entity)) {
            Jarvis.Helper.stringParameterCheck(entityId, "Jarvis.Helper.executeUpdateAsync requires the entityId parameter of type string.");
            Jarvis.Helper.stringParameterCheck(entityCollectionName, "Jarvis.Helper.executeUpdateAsync requires the entityCollectionName parameter of type string.");
            Jarvis.Helper.callbackParameterCheck(successCallback, "Jarvis.Helper.executeUpdateAsync requires successCallback of type function.");
            Jarvis.Helper.callbackParameterCheck(errorCallback, "Jarvis.Helper.executeUpdateAsync requires errorCallback of type function.");

            entityId = entityId.replace("{", "").replace("}", "");
            var query = entityCollectionName + "(" + entityId + ")";

            var req = new XMLHttpRequest();
            req.open("PATCH", Jarvis.Helper.getWebAPIPath() + query, true);
            Jarvis.Helper.setRequestHeaders(req, false, customHeaders);
            req.onreadystatechange = function () {
                if (this.readyState === 4) {
                    req.onreadystatechange = null;
                    var response = {};
                    switch (this.status) {
                        case 204:
                            response.statusCode = 204;
                            response.result = null;
                            successCallback(response);
                            break;
                        default:
                            response.statusCode = this.status;
                            response.errorLog = Jarvis.Helper.prepareErrorLog(this);
                            response.errorMessage = Jarvis.Helper.getErrorMessage(this);
                            errorCallback(response);
                    }
                }
            };
            req.send(JSON.stringify(entity));
        }
    },

    ///<summary>
    /// execute Custom Action Sync
    ///</summary>
    executeCustomActionSync: function (entityId, entityCollectionName, actionName, body, isGlobleAction) {
        "use strict";
        var response = {};
        var query = "";
        Jarvis.Helper.stringParameterCheck(actionName, "Jarvis.Helper.executeCustomActionSync requires the actionName parameter is a string.");
        if (!Jarvis.Helper.checkIfNullOrUndefined(isGlobleAction) && isGlobleAction !== true) {
            Jarvis.Helper.stringParameterCheck(entityId, "Jarvis.Helper.executeCustomActionSync requires the entityId parameter of type string.");
            Jarvis.Helper.stringParameterCheck(entityCollectionName, "Jarvis.Helper.executeCustomActionSync requires the entityCollectionName parameter of type string.");
            entityId = entityId.replace("{", "").replace("}", "");
            query = entityCollectionName + "(" + entityId + ")/Microsoft.Dynamics.CRM." + actionName;
        } else {
            query = actionName;
        }
        var req = new XMLHttpRequest();
        req.open("POST", Jarvis.Helper.getWebAPIPath() + query, false);
        Jarvis.Helper.setRequestHeaders(req, false, null);
        req.onreadystatechange = function () {
            if (this.readyState === 4) {
                req.onreadystatechange = null;
                var jsonResponse = null;
                switch (this.status) {
                    case 200:
                        response.statusCode = 200;
                        if (!Jarvis.Helper.checkIfNullOrUndefined(this.response)) {
                            jsonResponse = JSON.parse(this.response);
                            if (!Jarvis.Helper.checkIfNullOrUndefined(jsonResponse)) {
                                response.result = jsonResponse;
                            }
                        }
                        break;
                    case 204:
                        response.statusCode = 204;
                        response.result = null;
                        break;
                    default:
                        response.statusCode = this.status;
                        response.errorLog = Jarvis.Helper.prepareErrorLog(this);
                        response.errorMessage = Jarvis.Helper.getErrorMessage(this);
                }
            }
        };
        if (Jarvis.Helper.checkIfNullOrUndefined(body)) {
            body = {};
        }
        req.send(JSON.stringify(body));
        return response;
    },

    ///<summary>
    /// execute Custom Action Async
    ///</summary>
    executeCustomActionAsync: function (entityId, entityCollectionName, actionName, body, isGlobleAction, successCallback, errorCallback) {
        "use strict";
        var query = "";
        Jarvis.Helper.stringParameterCheck(actionName, "Jarvis.Helper.executeCustomActionAsync requires the actionName parameter is a string.");
        Jarvis.Helper.callbackParameterCheck(successCallback, "Jarvis.Helper.executeCustomActionAsync requires successCallback of type function.");
        Jarvis.Helper.callbackParameterCheck(errorCallback, "Jarvis.Helper.executeCustomActionAsync requires errorCallback of type function.");
        query = "";
        if (!Jarvis.Helper.checkIfNullOrUndefined(isGlobleAction) && isGlobleAction !== true) {
            Jarvis.Helper.stringParameterCheck(entityId, "Jarvis.Helper.executeCustomActionAsync requires the entityId parameter of type string.");
            Jarvis.Helper.stringParameterCheck(entityCollectionName, "Jarvis.Helper.executeCustomActionAsync requires the entityCollectionName parameter of type string.");
            entityId = entityId.replace("{", "").replace("}", "");
            query = entityCollectionName + "(" + entityId + ")/Microsoft.Dynamics.CRM." + actionName;
        } else {
            query = actionName;
        }
        var req = new XMLHttpRequest();
        req.open("POST", Jarvis.Helper.getWebAPIPath() + query, true);
        Jarvis.Helper.setRequestHeaders(req, false, null);
        req.onreadystatechange = function () {
            if (this.readyState === 4) {
                req.onreadystatechange = null;
                var response = {};
                var jsonResponse = null;
                switch (this.status) {
                    case 200:
                        response.statusCode = 200;
                        if (!Jarvis.Helper.checkIfNullOrUndefined(this.response)) {
                            jsonResponse = JSON.parse(this.response);
                            if (!Jarvis.Helper.checkIfNullOrUndefined(jsonResponse)) {
                                response.result = jsonResponse;
                            }
                        }
                        successCallback(response);
                        break;
                    case 204:
                        response.statusCode = 204;
                        response.result = null;
                        successCallback(response);
                        break;
                    default:
                        response.statusCode = this.status;
                        response.errorLog = Jarvis.Helper.prepareErrorLog(this);
                        response.errorMessage = Jarvis.Helper.getErrorMessage(this);
                        errorCallback(response);
                }
            }
        };
        if (Jarvis.Helper.checkIfNullOrUndefined(body)) {
            body = {};
        }
        req.send(JSON.stringify(body));
    },

    ///<summary>
    /// execute Fetch Xml Sync
    ///</summary>
    executeFetchXmlSync: function (entitySet, fetchXml, errorCallback) {
        "use strict";
        Jarvis.Helper.stringParameterCheck(entitySet, "Jarvis.Helper.executeFetchXmlSync requires entitySet parameter of type string.");
        Jarvis.Helper.stringParameterCheck(fetchXml, "Jarvis.Helper.executeFetchXmlSync requires fetchXml parameter of type string.");
        Jarvis.Helper.callbackParameterCheck(errorCallback, "Jarvis.Helper.executeFetchXmlSync requires errorCallback of type function.");
        var results = [];
        var fetch = escape(fetchXml);
        var req = new XMLHttpRequest();
        req.open("GET", Jarvis.Helper.getWebAPIPath() + entitySet + "?fetchXml=" + fetch, false);
        Jarvis.Helper.setRequestHeaders(req, true, null);
        req.onreadystatechange = function () {
            if (this.readyState === 4) {
                req.onreadystatechange = null;
                if (this.status === 200) {
                    if (!Jarvis.Helper.checkIfNullOrUndefined(this.response)) {
                        var jsonResponse = JSON.parse(this.response);
                        if (!Jarvis.Helper.checkIfNullOrUndefined(jsonResponse) && !Jarvis.Helper.checkIfNullOrUndefined(jsonResponse.value)) {
                            results = jsonResponse.value;
                        }
                    }
                } else {
                    results = errorCallback(Jarvis.Helper.errorHandler(this));
                }
            }
        };
        req.send();
        return results;
    },

    ///<summary>
    /// execute Fetch Xml Sync 5000+ Records
    ///</summary>
    executeFetchXmlSyncMoreRecords: function (entitySet, fetchXml, errorCallback) {
        "use strict";
        Jarvis.Helper.stringParameterCheck(entitySet, "Jarvis.Helper.executeFetchXmlSync requires entitySet parameter of type string.");
        Jarvis.Helper.stringParameterCheck(fetchXml, "Jarvis.Helper.executeFetchXmlSync requires fetchXml parameter of type string.");
        Jarvis.Helper.callbackParameterCheck(errorCallback, "Jarvis.Helper.executeFetchXmlSync requires errorCallback of type function.");
        var pageCount = 5000;
        var results = [];
        var pageNumber = 0;
        var pagingcookie = "";
        fetchXml = fetchXml.replace(/"/g, "'");
        var fetchXmlPaging = null;
        var fetch = null;
        var req = null;
        while (pagingcookie !== "end") {
            pageNumber += 1;
            fetchXmlPaging = fetchXml.replace("<fetch ", "<fetch no-lock='true' page='" + pageNumber.toString() + "' count='" + pageCount.toString() + "' paging-cookie='" + pagingcookie + "' ");
            fetch = escape(fetchXmlPaging);
            req = new XMLHttpRequest();
            req.open("GET", Jarvis.Helper.getWebAPIPath() + entitySet + "?fetchXml=" + fetch, false);
            Jarvis.Helper.setRequestHeaders(req, true, null);
            req.onreadystatechange = function () {
                if (this.readyState === 4) {
                    req.onreadystatechange = null;
                    pagingcookie = "end";
                    if (this.status === 200) {
                        if (!Jarvis.Helper.checkIfNullOrUndefined(this.response)) {
                            var jsonResponse = JSON.parse(this.response);
                            if (!Jarvis.Helper.checkIfNullOrUndefined(jsonResponse) && !Jarvis.Helper.checkIfNullOrUndefined(jsonResponse.value) && jsonResponse.value.length > 0) {
                                var itemIndex = 0;
                                for (itemIndex = 0; itemIndex < jsonResponse.value.length; itemIndex += 1) {
                                    results.push(jsonResponse.value[itemIndex]);
                                }
                            }
                            pagingcookie = jsonResponse["@Microsoft.Dynamics.CRM.fetchxmlpagingcookie"];
                            if (Jarvis.Helper.checkIfNullOrUndefined(pagingcookie) || pagingcookie === "") {
                                pagingcookie = "end";
                            } else {
                                pagingcookie = pagingcookie.substr(pagingcookie.indexOf('"') + 1);
                                pagingcookie = pagingcookie.substr(pagingcookie.indexOf('"') + 1);
                                pagingcookie = pagingcookie.substr(pagingcookie.indexOf('"') + 1);
                                pagingcookie = pagingcookie.substr(0, pagingcookie.indexOf('"'));
                                pagingcookie = decodeURIComponent(pagingcookie);
                                pagingcookie = decodeURIComponent(pagingcookie);
                                pagingcookie = pagingcookie.replace(/"/g, "&quot;");
                                pagingcookie = pagingcookie.replace(/</g, "&lt;");
                                pagingcookie = pagingcookie.replace(/>/g, "&gt;");
                            }
                        }
                    } else {
                        results = errorCallback(Jarvis.Helper.errorHandler(this));
                    }
                }
            };
            req.send();
        }
        return results;
    },

    ///<summary>
    /// execute Fetch By Query Sync
    ///</summary>
    executeFetchByQuerySync: function (entitySet, selectQuery, filterQuery, errorCallback, queryEndPart) {
        "use strict";
        Jarvis.Helper.stringParameterCheck(entitySet, "Jarvis.Helper.executeFetchByQuerySync requires entitySet parameter of type string.");
        Jarvis.Helper.stringParameterCheck(selectQuery, "Jarvis.Helper.executeFetchByQuerySync requires selectQuery parameter of type string.");
        Jarvis.Helper.stringParameterCheck(filterQuery, "Jarvis.Helper.executeFetchByQuerySync requires filterQuery parameter of type string.");
        Jarvis.Helper.callbackParameterCheck(errorCallback, "Jarvis.Helper.executeFetchByQuerySync requires errorCallback of type function.");
        var results = [];
        var query = "";
        if (selectQuery !== null && selectQuery !== "") {
            if (query === "") {
                query += "?$select=" + selectQuery;
            } else {
                query += "&$select=" + selectQuery;
            }
        }
        if (filterQuery !== null && filterQuery !== "") {
            if (query === "") {
                query += "?$filter=" + filterQuery;
            } else {
                query += "&$filter=" + filterQuery;
            }
        }
        if (queryEndPart !== null && queryEndPart !== "") {
            if (query === "") {
                query += "?" + queryEndPart;
            } else {
                query += "&" + queryEndPart;
            }
        }
        var req = new XMLHttpRequest();
        req.open("GET", Jarvis.Helper.getWebAPIPath() + entitySet + query, false);
        Jarvis.Helper.setRequestHeaders(req, true, null);
        req.onreadystatechange = function () {
            if (this.readyState === 4) {
                req.onreadystatechange = null;
                if (this.status === 200) {
                    if (!Jarvis.Helper.checkIfNullOrUndefined(this.response)) {
                        var jsonResponse = JSON.parse(this.response);
                        if (!Jarvis.Helper.checkIfNullOrUndefined(jsonResponse) && !Jarvis.Helper.checkIfNullOrUndefined(jsonResponse.value)) {
                            results = jsonResponse.value;
                        }
                    }
                } else {
                    results = errorCallback(Jarvis.Helper.errorHandler(this));
                }
            }
        };
        req.send();
        return results;
    },

    executeEntitySetByExpand: function (entitySet, entityId, expand, errorCallback) {
        "use strict";
        Jarvis.Helper.stringParameterCheck(entitySet, "Jarvis.Helper.executeFetchByQuerySync requires entitySet parameter of type string.");
        Jarvis.Helper.stringParameterCheck(entityId, "Jarvis.Helper.executeFetchByQuerySync requires entityId parameter of type string.");
        Jarvis.Helper.stringParameterCheck(expand, "Jarvis.Helper.executeFetchByQuerySync requires expand parameter of type string.");
        Jarvis.Helper.callbackParameterCheck(errorCallback, "Jarvis.Helper.executeFetchByQuerySync requires errorCallback of type function.");
        var results = [];
        var query = "";
        if (entityId !== null && entityId !== "") {
            query += "(" + entityId + ")";
        }
        if (expand !== null && expand !== "") {
            query += "?$expand=" + expand;
        }

        var req = new XMLHttpRequest();
        req.open("GET", Jarvis.Helper.getWebAPIPath() + entitySet + query, false);
        Jarvis.Helper.setRequestHeaders(req, true, null);
        req.onreadystatechange = function () {
            if (this.readyState === 4) {
                req.onreadystatechange = null;
                if (this.status === 200) {
                    if (!Jarvis.Helper.checkIfNullOrUndefined(this.response)) {
                        var jsonResponse = JSON.parse(this.response);
                        if (!Jarvis.Helper.checkIfNullOrUndefined(jsonResponse)) {
                            results = jsonResponse;
                        }
                    }
                } else {
                    results = errorCallback(Jarvis.Helper.errorHandler(this));
                }
            }
        };
        req.send();
        return results;
    },

    ///<summary>
    /// execute Fetch record by Id Sync
    ///</summary>
    executeFetchRecordByIdSync: function (entitySet, recordId, select, errorCallback) {
        "use strict";
        Jarvis.Helper.stringParameterCheck(entitySet, "Jarvis.Helper.executeFetchRecordByIdSync requires entitySet parameter of type string.");
        Jarvis.Helper.stringParameterCheck(recordId, "Jarvis.Helper.executeFetchRecordByIdSync requires recordId parameter of type string.");
        Jarvis.Helper.stringParameterCheck(select, "Jarvis.Helper.executeFetchRecordByIdSync requires select parameter of type string.");
        Jarvis.Helper.callbackParameterCheck(errorCallback, "Jarvis.Helper.executeFetchRecordByIdSync requires errorCallback of type function.");
        var result = {};

        var req = new XMLHttpRequest();
        req.open("GET", Jarvis.Helper.getWebAPIPath() + entitySet + "(" + recordId.replace("{", "").replace("}", "") + ")?$select=" + select, false);
        Jarvis.Helper.setRequestHeaders(req, true, null);
        req.onreadystatechange = function () {
            if (this.readyState === 4) {
                req.onreadystatechange = null;
                if (this.status === 200) {
                    var jsonResponse = JSON.parse(this.response);
                    if (!Jarvis.Helper.checkIfNullOrUndefined(jsonResponse)) {
                        result = jsonResponse;
                    }
                } else {
                    result = errorCallback(Jarvis.Helper.errorHandler(this));
                }
            }
        };
        req.send();
        return result;
    },

    ///
    executeSubmitForApproval: function (formContext, regardingEntityName, regardingEntityType, bidLevel, performanceLevel, ApprovalLevelType, AlertErrorInAction, PleaseWaitMessage, SubmitForApprovalMessage) {
        "use strict";
        var isNotificationRemoved = formContext.ui.clearFormNotification("SubmitForApproval");
        if (isNotificationRemoved) {
            formContext.ui.setFormNotification(PleaseWaitMessage, "INFO", "SubmitForApproval");
        } else {
            formContext.ui.setFormNotification(PleaseWaitMessage, "INFO", "SubmitForApproval");
            var approvalType = null;
            var formType = formContext.ui.getFormType();
            if (formType !== 1) {
                var recordId = formContext.data.entity.getId().replace("{", "").replace("}", "");
                var entityName = formContext.data.entity.getEntityName();
                approvalType = ApprovalLevelType;
                if (approvalType != null) {
                    formContext.ui.setFormNotification(PleaseWaitMessage, "INFO", "SubmitForApproval");
                    var recordId = formContext.data.entity.getId().replace("{", "").replace("}", "");
                    formContext.data.save().then(function () {
                        var parameters = {};
                        parameters.inputData = "{ \"Submit\": \"Request\", \"RegardingObjectLogicalName\":\"" + entityName + "\", \"RegardingObjectId\":\"" + recordId + "\", \"ApprovalType\":\"" + approvalType + "\" }";
                        Jarvis.Helper.executeCustomActionAsync(null, null, "Jarvis_SubmitApproval", parameters, true,
                            function (response) {
                                var results = response.result;

                                formContext.ui.clearFormNotification("SubmitForApproval");
                                var entityFormOptions = {};
                                var entityId = formContext.data.entity.getId().replace("{", "").replace("}", "");
                                entityFormOptions["entityName"] = formContext.data.entity.getEntityName();
                                entityFormOptions["entityId"] = entityId;

                                // Open the form.
                                var alertStrings = { confirmButtonLabel: "OK", text: SubmitForApprovalMessage, title: "Title" };
                                var alertOptions = { height: 120, width: 260 };
                                Xrm.Navigation.openAlertDialog(alertStrings, alertOptions).then(
                                function (success) {
                                    Jarvis.Helper.refreshPage(formContext);
                                },
                                function (error) { Jarvis.Helper.getConsoleErrorMessage(error); });
                            },
                            function (error) {
                                formContext.ui.clearFormNotification("SubmitForApproval");
                                var alertStrings = { confirmButtonLabel: "Yes", text: AlertErrorInAction + error.errorMessage, title: "Title" };
                                var alertOptions = { height: 120, width: 260 };
                                Xrm.Navigation.openAlertDialog(alertStrings, alertOptions).then(
                                   function (success) { Jarvis.Helper.getMessage("Alert dialog closed"); },
                                   function (error) { Jarvis.Helper.getConsoleErrorMessage(error); });

                                var entityFormOptions = {};
                                var entityId = formContext.data.entity.getId().replace("{", "").replace("}", "");
                                entityFormOptions["entityName"] = formContext.data.entity.getEntityName();
                                entityFormOptions["entityId"] = entityId;

                                // Open the form.
                                Xrm.Navigation.openForm(entityFormOptions).then(
                                    function (success) {
                                        Jarvis.Helper.getMessage(success);
                                    },
                                    function (error) {
                                        Jarvis.Helper.getConsoleErrorMessage(error);
                                    });
                            });
                    },
                        function (error) {
                            Jarvis.Helper.getConsoleErrorMessage(error);
                        });
                }
            }
        }
    },

    getUserMessage: function (name) {
        "use strict";
        if (name !== null && name !== undefined) {
            return Xrm.Utility.getResourceString("Jarvis_/resx/UserMessage", name);
        }
    },

    getUserRoles: function (userId) {
        "use strict";
        if (sessionStorage.getItem("User_" + userId) !== null) {
            return JSON.parse(sessionStorage.getItem("User_" + userId))
        }

        var fetchXml = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>" +
            "<entity name='role'>" +
            "<attribute name='name' />" +
            "<attribute name='businessunitid' />" +
            "<attribute name='roleid' />" +
            "<order attribute='name' descending='false' />" +
            "<link-entity name='systemuserroles' from='roleid' to='roleid' visible='false' intersect='true'>" +
            "<link-entity name='systemuser' from='systemuserid' to='systemuserid' alias='aa'>" +
            "<filter type='and'>" +
            "<condition attribute='systemuserid' operator='eq' value='" + userId + "' />" +
            "</filter>" +
            "</link-entity>" +
            "</link-entity>" +
            "</entity>" +
            "</fetch>";
        var results = Jarvis.Helper.executeFetchXmlSync("roles", fetchXml, function errorCallback(error) { Jarvis.Helper.getConsoleErrorMessage(error); });
        sessionStorage.setItem("User_" + userId, JSON.stringify(results));
        return results;
    },

    checkUserHasSecurityRole: function (userId, roleNameLists) {
        "use strict";
        var isUserExist = false;
        var rolelist = Jarvis.Helper.getUserRoles(userId);
        if (rolelist !== null && rolelist !== undefined && rolelist.length > 0) {
            rolelist.forEach(function (item, index) {
                if (roleNameLists.includes(item.name)) {
                    isUserExist = true;
                }
            });
        }

        return isUserExist;
    },

    getUserTeams: function (userId) {
        "use strict";
        if (sessionStorage.getItem("Team_" + userId) !== null) {
            return JSON.parse(sessionStorage.getItem("Team_" + userId))
        }
        var fetchXml = "<fetch distinct='true' mapping='logical' output-format='xml-platform' version='1.0'>" +
                            "<entity name='team'>" +
                            "<attribute name='name'/>" +
                            "<attribute name='businessunitid'/>" +
                            "<attribute name='teamid'/>" +
                            "<attribute name='teamtype'/>" +
                            "<order descending='false' attribute='name'/>" +
                             "<filter type='and'>" +
                             "<condition attribute='teamtype' operator='eq' value='0' />" +
                             "</filter>" +
                                "<link-entity name='teammembership' intersect='true' visible='false' to='teamid' from='teamid'>" +
                                   "<link-entity name='systemuser' to='systemuserid' from='systemuserid' alias='ab'>" +
                                        "<filter type='and'>" +
                                            "<condition attribute='systemuserid' operator='eq' value='" + userId + "' />" +
                                        "</filter>" +
                                    "</link-entity>" +
                                "</link-entity>" +
                            "</entity>" +
                            "</fetch>";
        var results = Jarvis.Helper.executeFetchXmlSync("teams", fetchXml, function errorCallback(error) { Jarvis.Helper.getConsoleErrorMessage(error); });
        sessionStorage.setItem("Team_" + userId, JSON.stringify(results));
        return results;
    },

    checkUserIsTeamMember: function (userId, teamNameLists) {
        "use strict";
        var isUserExist = false;
        var teamsList = Jarvis.Helper.getUserTeams(userId);
        if (teamsList !== null && teamsList.length > 0) {
            teamsList.forEach(function (item, index) {
                if (teamNameLists.includes(item.name)) {
                    isUserExist = true;
                }
            });
        }

        return isUserExist;
    },

    refreshPage: function (formContext) {
        "use strict";
        var entityFormOptions = {};
        entityFormOptions["entityName"] = formContext.data.entity.getEntityName();
        entityFormOptions["entityId"] = formContext.data.entity.getId().replace("{", "").replace("}", "");

        // Open the form.
        Xrm.Navigation.openForm(entityFormOptions).then(
        function (success) {
            Jarvis.Helper.getMessage(success);
        },
        function (error) {
            Jarvis.Helper.getConsoleErrorMessage(error);
        });
    },

    getConsoleErrorMessage: function (error) {
        "use strict";
        if (error != null) {
            console.log(error.message);
        }
    },

    getMessage: function (message) {
        "use strict";
        console.log(message);
    },

}