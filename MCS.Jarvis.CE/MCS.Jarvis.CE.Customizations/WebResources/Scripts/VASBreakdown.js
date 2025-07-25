function showHideTabs(formContext, tabArr, isVisible) {
    "use strict";
    for (var ii = 0; ii < tabArr.length; ii++) {
        formContext.ui.tabs.get(tabArr[ii])?.setVisible(isVisible);
    }
}

function makeFieldsMandatory(formContext, fieldArr, isMandatory) {
    "use strict";
    for (var ii = 0; ii < fieldArr.length; ii++) {
        formContext.getAttribute(fieldArr[ii]).setRequiredLevel(isMandatory);//
    }
}

function getStageData(executionContext) {
    "use strict";
    var formContext = executionContext.getFormContext();

    var activeStageId, activeStageName;

    //Get the current active stage of the process
    var activeStage = formContext.data.process.getActiveStage();

    if (activeStage != null && activeStage != undefined) {
        //Get the ID of the current stage
        activeStageId = activeStage.getId();

        //Get the Name of the current stage
        activeStageName = activeStage.getName();

    }


    return activeStageName;
}

async function caseBPFValidation(executionContext) {
    "use strict";
    var formContext = executionContext.getFormContext();
    console.log("Case On Load");
    var formType = formContext.ui.getFormType();
    //alert(formType);
    if (formType !== null) {//Read-Only Or Disabled
        var activeStageName = getStageData(executionContext);
        //alert(activeStageName);
        if (activeStageName !== null && activeStageName !== undefined) {
            if (formContext.getControl("header_process_jarvis_totalgoplimitinapproved") !== null)
                formContext.getControl("header_process_jarvis_totalgoplimitinapproved").setDisabled(true);
            if (formContext.getControl("header_process_jarvis_totalcurrencyinapproved") !== null)
                formContext.getControl("header_process_jarvis_totalcurrencyinapproved").setDisabled(true);
            if (formContext.getControl("header_process_jarvis_actualcausefault") !== null)
                formContext.getControl("header_process_jarvis_actualcausefault").setDisabled(true);
            if (formContext.getControl("header_process_jarvis_eta") !== null)
                formContext.getControl("header_process_jarvis_eta").setDisabled(true);
            if (formContext.getControl("header_process_jarvis_etc") !== null)
                formContext.getControl("header_process_jarvis_etc").setDisabled(true);
            if (formContext.getControl("header_process_jarvis_ata") !== null)
                formContext.getControl("header_process_jarvis_ata").setDisabled(true);
            if (formContext.getControl("header_process_jarvis_atc") !== null)
                formContext.getControl("header_process_jarvis_atc").setDisabled(true);
            if (formContext.getControl("header_process_jarvis_totalpassoutamount") !== null)
                formContext.getControl("header_process_jarvis_totalpassoutamount").setDisabled(true);
            if (formContext.getControl("header_process_jarvis_totalpassoutcurrency") !== null)//jarvis_customerinformed
                formContext.getControl("header_process_jarvis_totalpassoutcurrency").setDisabled(true);
            if (formContext.getControl("header_process_jarvis_automationcriteriamet") !== null)//jarvis_customerinformed
                formContext.getControl("header_process_jarvis_automationcriteriamet").setDisabled(true);
            if (formContext.getControl("header_process_jarvis_customerinformed") !== null) {//jarvis_customerinformed getAttribute().getValue()
                var cInformed = formContext.getControl("header_process_jarvis_customerinformed").getAttribute().getValue();
                if (cInformed == true)//Yes
                {
                    formContext.getControl("header_process_jarvis_customerinformed").setDisabled(true);
                }

            }
            var hideTabsArr = ["tab_31"];
            showHideTabs(formContext, hideTabsArr, false);
            if (activeStageName.toLowerCase() === 'case opening') {
                console.log(activeStageName.toLowerCase());
                var caseType = formContext.getAttribute("casetypecode").getValue();
                formContext.getControl("casetypecode").setDisabled(false);
                if (caseType == 3)//Query
                {
                    var showTabsArr = ["tab_20", "tab_27", "tab_26", "tab_25", "tab_28", "tab_30", "tab_29", "tab_22", "tab_Query", "tab_register", "tab_collect", "tab_share", "tab_9", "tab_passout", "tab_scheduler", "tab_repairongoing", "tab_repairfinished", "tab_15", "tab_timeline", "tab_summary", "towingRental", "tab_GOP", "Contacts", "tab_MonitorActions"];
                    showHideTabs(formContext, showTabsArr, true);
                    var hideTabsArr = ["tab_20", "tab_27", "tab_26", "tab_25", "tab_28", "tab_30", "tab_29", "tab_register", "tab_collect", "tab_share", "tab_9", "tab_passout", "tab_scheduler", "tab_repairongoing", "tab_repairfinished", "tab_15", "tab_timeline", "tab_summary", "towingRental", "tab_GOP"];
                    showHideTabs(formContext, hideTabsArr, false);
                }
                else {
                    var showTabsArr = ["tab_31", "tab_20", "tab_27", "tab_26", "tab_25", "tab_28", "tab_30", "tab_29", "tab_22", "tab_Query", "tab_register", "tab_collect", "tab_share", "tab_9", "tab_passout", "tab_scheduler", "tab_repairongoing", "tab_repairfinished", "tab_15", "tab_timeline", "tab_summary", "towingRental", "tab_GOP", "Contacts", "tab_MonitorActions"];
                    showHideTabs(formContext, showTabsArr, true);
                    var hideTabsArr = ["tab_20", "tab_27", "tab_26", "tab_25", "tab_28", "tab_22", "tab_Query", "tab_9", "tab_passout", "tab_scheduler", "tab_repairongoing", "tab_repairfinished", "tab_timeline", "tab_summary", "towingRental", "tab_GOP"];
                    showHideTabs(formContext, hideTabsArr, false);
                }//Breakdown


            }
            else if (activeStageName.toLowerCase() === 'guarantee of payment') {
                console.log(activeStageName.toLowerCase());
                formContext.ui.clearFormNotification("VEHICLE");
                formContext.getControl("casetypecode").setDisabled(true);
                var showTabsArr = ["tab_31", "tab_20", "tab_27", "tab_26", "tab_25", "tab_28", "tab_30", "tab_29", "tab_22", "tab_Query", "tab_register", "tab_collect", "tab_share", "tab_9", "tab_passout", "tab_scheduler", "tab_repairongoing", "tab_repairfinished", "tab_15", "tab_timeline", "tab_summary", "towingRental", "tab_GOP", "Contacts", "tab_MonitorActions"];
                showHideTabs(formContext, showTabsArr, true);
                var hideTabsArr = ["tab_30", "tab_29", "tab_22", "tab_Query", "tab_register", "tab_collect", "tab_share", "tab_scheduler", "towingRental", "tab_GOP"];
                showHideTabs(formContext, hideTabsArr, false);
                if (formContext.getControl("header_process_jarvis_totalgoplimitinapproved") !== null)
                    formContext.getControl("header_process_jarvis_totalgoplimitinapproved").setDisabled(true);
                if (formContext.getControl("header_process_jarvis_totalcurrencyinapproved") !== null)
                    formContext.getControl("header_process_jarvis_totalcurrencyinapproved").setDisabled(true);

            }
            else if (activeStageName.toLowerCase() === 'pass out') {
                console.log(activeStageName.toLowerCase());
                formContext.getControl("casetypecode").setDisabled(true);
                var showTabsArr = ["tab_31", "tab_20", "tab_27", "tab_26", "tab_25", "tab_28", "tab_30", "tab_29", "tab_22", "tab_Query", "tab_register", "tab_collect", "tab_share", "tab_9", "tab_passout", "tab_scheduler", "tab_repairongoing", "tab_repairfinished", "tab_15", "tab_timeline", "tab_summary", "towingRental", "tab_GOP", "Contacts", "tab_MonitorActions"];
                showHideTabs(formContext, showTabsArr, true);
                var hideTabsArr = ["tab_30", "tab_29", "tab_22", "tab_Query", "tab_register", "tab_collect", "tab_share", "tab_GOP"];
                showHideTabs(formContext, hideTabsArr, false);
                if (formContext.getControl("header_process_jarvis_totalpassoutamount") !== null)
                    formContext.getControl("header_process_jarvis_totalpassoutamount").setDisabled(true);
                if (formContext.getControl("header_process_jarvis_totalpassoutcurrency") !== null)
                    formContext.getControl("header_process_jarvis_totalpassoutcurrency").setDisabled(true);
                var currentDateTime = new Date();
                //formContext.getAttribute("jarvis_canvasdate").setValue(currentDateTime);

            }
            else if (activeStageName.toLowerCase() === 'eta technician') {
                console.log(activeStageName.toLowerCase());
                formContext.getControl("casetypecode").setDisabled(true);
                var showTabsArr = ["tab_31", "tab_20", "tab_27", "tab_26", "tab_25", "tab_28", "tab_30", "tab_29", "tab_22", "tab_Query", "tab_register", "tab_collect", "tab_share", "tab_9", "tab_passout", "tab_scheduler", "tab_repairongoing", "tab_repairfinished", "tab_15", "tab_timeline", "tab_summary", "towingRental", "tab_GOP", "Contacts", "tab_MonitorActions"];
                showHideTabs(formContext, showTabsArr, true);
                var hideTabsArr = ["tab_30", "tab_29", "tab_22", "tab_Query", "tab_register", "tab_collect", "tab_share", "tab_GOP"];
                showHideTabs(formContext, hideTabsArr, false);
                if (formContext.getControl("header_process_jarvis_eta") !== null)
                    formContext.getControl("header_process_jarvis_eta").setDisabled(true);
                if (formContext.getControl("header_process_jarvis_etavalidation") !== null)
                    formContext.getControl("header_process_jarvis_etavalidation").setDisabled(true);

            }
            else if (activeStageName.toLowerCase() === 'waiting for repair start') {
                formContext.getControl("casetypecode").setDisabled(true);
                var showTabsArr = ["tab_31", "tab_20", "tab_27", "tab_26", "tab_25", "tab_28", "tab_30", "tab_29", "tab_22", "tab_Query", "tab_register", "tab_collect", "tab_share", "tab_9", "tab_passout", "tab_scheduler", "tab_repairongoing", "tab_repairfinished", "tab_15", "tab_timeline", "tab_summary", "towingRental", "tab_GOP", "Contacts", "tab_MonitorActions"];
                showHideTabs(formContext, showTabsArr, true);
                var hideTabsArr = ["tab_30", "tab_29", "tab_22", "tab_Query", "tab_register", "tab_collect", "tab_share", "tab_GOP"];
                showHideTabs(formContext, hideTabsArr, false);
                if (formContext.getControl("header_process_jarvis_ata") !== null)
                    formContext.getControl("header_process_jarvis_ata").setDisabled(true);
                if (formContext.getControl("header_process_jarvis_atavalidation") !== null)
                    formContext.getControl("header_process_jarvis_atavalidation").setDisabled(true);

            }

            else if (activeStageName.toLowerCase() === 'repair ongoing') {
                formContext.getControl("casetypecode").setDisabled(true);
                var showTabsArr = ["tab_31", "tab_20", "tab_27", "tab_26", "tab_25", "tab_28", "tab_30", "tab_29", "tab_22", "tab_Query", "tab_register", "tab_collect", "tab_share", "tab_9", "tab_passout", "tab_scheduler", "tab_repairongoing", "tab_repairfinished", "tab_15", "tab_timeline", "tab_summary", "towingRental", "tab_GOP", "Contacts", "tab_MonitorActions"];
                showHideTabs(formContext, showTabsArr, true);
                var hideTabsArr = ["tab_30", "tab_29", "tab_22", "tab_Query", "tab_register", "tab_collect", "tab_share", "tab_GOP"];
                showHideTabs(formContext, hideTabsArr, false);


            }
            else if (activeStageName.toLowerCase() === 'repair finished') {
                formContext.getControl("casetypecode").setDisabled(true);
                var showTabsArr = ["tab_31", "tab_20", "tab_27", "tab_26", "tab_25", "tab_28", "tab_30", "tab_29", "tab_22", "tab_Query", "tab_register", "tab_collect", "tab_share", "tab_9", "tab_passout", "tab_scheduler", "tab_repairongoing", "tab_repairfinished", "tab_15", "tab_timeline", "tab_summary", "towingRental", "tab_GOP", "Contacts", "tab_MonitorActions"];
                showHideTabs(formContext, showTabsArr, true);
                var hideTabsArr = ["tab_30", "tab_29", "tab_22", "tab_Query", "tab_register", "tab_collect", "tab_share", "tab_GOP"];
                showHideTabs(formContext, hideTabsArr, false);


            }

            else if (activeStageName.toLowerCase() === 'repair summary') {
                formContext.getControl("casetypecode").setDisabled(true);
                var showTabsArr = ["tab_31", "tab_20", "tab_27", "tab_26", "tab_25", "tab_28", "tab_30", "tab_29", "tab_22", "tab_Query", "tab_register", "tab_collect", "tab_share", "tab_9", "tab_passout", "tab_scheduler", "tab_repairongoing", "tab_repairfinished", "tab_15", "tab_timeline", "tab_summary", "towingRental", "tab_GOP", "Contacts", "tab_MonitorActions"];
                showHideTabs(formContext, showTabsArr, true);
                var hideTabsArr = ["tab_30", "tab_29", "tab_22", "tab_Query", "tab_register", "tab_collect", "tab_share", "tab_GOP"];
                showHideTabs(formContext, hideTabsArr, false);


            }
            else if (activeStageName.toLowerCase() === 'case closure') {
                formContext.getControl("casetypecode").setDisabled(true);
                formContext.getControl("header_process_jarvis_automationcriteriamet").setDisabled(true);
                var showTabsArr = ["tab_31", "tab_20", "tab_27", "tab_26", "tab_25", "tab_28", "tab_30", "tab_29", "tab_22", "tab_Query", "tab_register", "tab_collect", "tab_share", "tab_9", "tab_passout", "tab_scheduler", "tab_repairongoing", "tab_repairfinished", "tab_15", "tab_timeline", "tab_summary", "towingRental", "tab_GOP", "Contacts", "tab_MonitorActions"];
                showHideTabs(formContext, showTabsArr, true);
                var hideTabsArr = ["tab_30", "tab_29", "tab_22", "tab_Query", "tab_register", "tab_collect", "tab_share", "tab_GOP"];
                showHideTabs(formContext, hideTabsArr, false);
                if (results === null) {
                    results = await retrieveRecords(formContext);
                }
                if (caseMonitors === null) {
                    caseMonitors = await retrieveCaseMonitorRecords(formContext);
                }
                if (passOuts === null) {
                    passOuts = await retrievePassOutRecords(formContext);
                }
                if (activeGops === null) {
                    activeGops = await retrieveGOPRecords(formContext);
                }
            }
            else if (activeStageName.toLowerCase() === 'ongoing') {
                formContext.getControl("casetypecode").setDisabled(true);
                var showTabsArr = ["tab_20", "tab_27", "tab_26", "tab_25", "tab_28", "tab_30", "tab_29", "tab_22", "tab_Query", "tab_register", "tab_collect", "tab_share", "tab_9", "tab_passout", "tab_scheduler", "tab_repairongoing", "tab_repairfinished", "tab_15", "tab_timeline", "tab_summary", "towingRental", "tab_GOP", "Contacts", "tab_MonitorActions"];
                showHideTabs(formContext, showTabsArr, true);
                var hideTabsArr = ["tab_20", "tab_27", "tab_26", "tab_25", "tab_28", "tab_30", "tab_29", "tab_register", "tab_collect", "tab_share", "tab_9", "tab_passout", "tab_scheduler", "tab_repairongoing", "tab_repairfinished", "tab_15", "tab_timeline", "tab_summary", "towingRental", "tab_GOP"];
                showHideTabs(formContext, hideTabsArr, false);

            }
            else if (activeStageName.toLowerCase() === 'solved') {
                var showTabsArr = ["tab_20", "tab_27", "tab_26", "tab_25", "tab_28", "tab_30", "tab_29", "tab_22", "tab_Query", "tab_register", "tab_collect", "tab_share", "tab_9", "tab_passout", "tab_scheduler", "tab_repairongoing", "tab_repairfinished", "tab_15", "tab_timeline", "tab_summary", "towingRental", "tab_GOP", "Contacts", "tab_MonitorActions"];
                showHideTabs(formContext, showTabsArr, true);
                var hideTabsArr = ["tab_20", "tab_27", "tab_26", "tab_25", "tab_28", "tab_30", "tab_29", "tab_register", "tab_collect", "tab_share", "tab_9", "tab_passout", "tab_scheduler", "tab_repairongoing", "tab_repairfinished", "tab_15", "tab_timeline", "tab_summary", "towingRental", "tab_GOP"];
                showHideTabs(formContext, hideTabsArr, false);

            }
            else if (activeStageName.toLowerCase() === 'credit to hd') {
                var showTabsArr = ["tab_20", "tab_27", "tab_26", "tab_25", "tab_28", "tab_30", "tab_29", "tab_22", "tab_Query", "tab_register", "tab_collect", "tab_share", "tab_9", "tab_passout", "tab_scheduler", "tab_repairongoing", "tab_repairfinished", "tab_15", "tab_timeline", "tab_summary", "towingRental", "tab_GOP", "Contacts", "tab_MonitorActions"];
                showHideTabs(formContext, showTabsArr, true);
                var hideTabsArr = ["tab_20", "tab_27", "tab_26", "tab_25", "tab_28", "tab_30", "tab_29", "tab_register", "tab_collect", "tab_share", "tab_9", "tab_passout", "tab_scheduler", "tab_repairongoing", "tab_repairfinished", "tab_15", "tab_timeline", "tab_summary", "towingRental", "tab_GOP"];
                showHideTabs(formContext, hideTabsArr, false);

            }
            else if (activeStageName.toLowerCase() === 'closed') {
                var showTabsArr = ["tab_20", "tab_27", "tab_26", "tab_25", "tab_28", "tab_30", "tab_29", "tab_22", "tab_Query", "tab_register", "tab_collect", "tab_share", "tab_9", "tab_passout", "tab_scheduler", "tab_repairongoing", "tab_repairfinished", "tab_15", "tab_timeline", "tab_summary", "towingRental", "tab_GOP", "Contacts", "tab_MonitorActions"];
                showHideTabs(formContext, showTabsArr, true);
                var hideTabsArr = ["tab_20", "tab_27", "tab_26", "tab_25", "tab_28", "tab_30", "tab_29", "tab_register", "tab_collect", "tab_share", "tab_9", "tab_passout", "tab_scheduler", "tab_repairongoing", "tab_repairfinished", "tab_15", "tab_timeline", "tab_summary", "towingRental", "tab_GOP"];
                showHideTabs(formContext, hideTabsArr, false);

            }
            //#94752-case escalation
            if (activeStageName.toLowerCase() != 'case opening') {
                console.log(activeStageName.toLowerCase());
                var caseType = formContext.getAttribute("casetypecode").getValue();
                if (caseType == 2)//Query
                {
                    var showTabsArr = ["tab_25"];
                    showHideTabs(formContext, showTabsArr, true);
                }

            }

        }
        else {
            //alert('inside else');
            var hideTabsArr = ["tab_20", "tab_22", "tab_Query", "tab_register", "tab_validate", "tab_collect", "tab_share", "tab_9", "tab_passout", "tab_scheduler",
                "tab_repairongoing", "tab_repairfinished", "tab_15", "tab_summary", "towingRental", "tab_GOP"];
            showHideTabs(formContext, hideTabsArr, false);
        }
    }


}

function checkStageChangedAuto(executionContext) {
    "use strict";
    var formContext = executionContext.getFormContext();
    //formContext.data.process.addOnStageChange(stageOnChange);
    formContext.data.process.addOnStageChange(caseBPFValidation);
}


function autoRefresh(executionContext) {
    "use strict";
    var formContext = executionContext.getFormContext();
    var activeStageName = getStageData(executionContext);
    console.log(activeStageName.toLowerCase());
    // if (activeStageName.toLowerCase() === 'pass out') {
    //      formContext.data.refresh(save).then(successCallback, errorCallback);
    //}
}

function showTabsForCaseReopened(executionContext) {
    "use strict";
    var formContext = executionContext.getFormContext();
    var caseStatus = formContext.getAttribute("statuscode").getValue();
    var caseType = formContext.getAttribute("casetypecode").getValue();
    if (caseType == 3)//Query
    {
        if (caseStatus == 85 || caseStatus == 5) {
            var showTabsArr = ["tab_20", "tab_27", "tab_26", "tab_25", "tab_28", "tab_30", "tab_29", "tab_22", "tab_Query", "tab_register", "tab_collect", "tab_share", "tab_9", "tab_passout", "tab_scheduler", "tab_repairongoing", "tab_repairfinished", "tab_15", "tab_timeline", "tab_summary", "towingRental", "tab_GOP", "Contacts", "tab_MonitorActions"];
            showHideTabs(formContext, showTabsArr, true);
            var hideTabsArr = ["tab_20", "tab_27", "tab_26", "tab_25", "tab_28", "tab_30", "tab_29", "tab_register", "tab_collect", "tab_share", "tab_9", "tab_passout", "tab_scheduler", "tab_repairongoing", "tab_repairfinished", "tab_15", "tab_timeline", "tab_summary", "towingRental", "tab_GOP"];
            showHideTabs(formContext, hideTabsArr, false);
        }
    }
    else {
        if (caseStatus == 85 || caseStatus == 5) {
            formContext.getControl("casetypecode").setDisabled(true);
            var showTabsArr = ["tab_31", "tab_20", "tab_27", "tab_26", "tab_25", "tab_28", "tab_30", "tab_29", "tab_22", "tab_Query", "tab_register", "tab_collect", "tab_share", "tab_9", "tab_passout", "tab_scheduler", "tab_repairongoing", "tab_repairfinished", "tab_15", "tab_timeline", "tab_summary", "towingRental", "tab_GOP", "Contacts", "tab_MonitorActions"];
            showHideTabs(formContext, showTabsArr, true);
            var hideTabsArr = ["tab_30", "tab_29", "tab_22", "tab_Query", "tab_register", "tab_collect", "tab_share", "tab_GOP"];
            showHideTabs(formContext, hideTabsArr, false);
        }
    }


}

function checkStageChangedAutoExpertForm(executionContext) {
    "use strict";
    var formContext = executionContext.getFormContext();
    formContext.data.process.addOnStageChange(caseBPFStatusSetForExpert);
}
var results = null; var caseMonitors = null; var passOuts = null; var activeGops = null;
var formContext = null
async function onLoad(e) {
    formContext = e.getFormContext();
    AddPreSearchToLookupCase(e);
    // formContext.data.process.addOnPreProcessStatusChange(handleStageMovement);
    var activeStageName = getStageData(e);
    if (activeStageName.toLowerCase() === 'case opening') {
        // formContext.data.process.addOnPreProcessStatusChange(handleStageMovement);
        formContext.data.process.addOnPreStageChange(handleStageMovement);
    }
    results = await retrieveRecords(formContext);
    caseMonitors = await retrieveCaseMonitorRecords(formContext);
    passOuts = await retrievePassOutRecords(formContext);
    activeGops = await retrieveGOPRecords(formContext);

    ////476631 - UI Change
    //// Set Caller Phone Number TYPE as Mandatory
    if (formContext.ui.getFormType() == 1 || (formContext.ui.getFormType() == 2 && activeStageName?.toLowerCase() === 'case opening')) {
        formContext.getAttribute("jarvis_callerphonenumbertype")?.setRequiredLevel("required");
    } else {
        formContext.getAttribute("jarvis_callerphonenumbertype")?.setRequiredLevel("recommended");
    }
    //var gridContext = formContext.getControl("Subgrid_new_24");
    //if (gridContext !== null) {
    //gridContext.addOnLoad(functionTriggeredgetOnSubgridRefresh)
    //}
    //var activeStageName = getStageData();
    //if (activeStageName !== null && activeStageName.toLowerCase() === 'case opening') {
    //if (formContext.ui.tabs.get("tab_20") !== null) {
    //formContext.ui.tabs.get("tab_20").setVisible(false);
    //}
    //}
    let creditCardRequested = formContext.getAttribute("jarvis_totalrequestedccamount")?.getValue();
    if (creditCardRequested) {
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
async function functionTriggeredgetOnSubgridRefresh() {
    setTimeout(get, 1000);
    //results = await retrieveRecords(formContext);
}
async function get() {
    await retrieveRecords(formContext);
}
async function handleStageMovement(e) {
    var formContext = e.getFormContext();
    var mileage = formContext.getAttribute("jarvis_mileage").getValue();
    var caseType = formContext.getAttribute("casetypecode").getValue();
    if ((mileage === null || mileage === 0) && caseType === 2) {
        e.getEventArgs().preventDefault();
        var msg = "Please add mileage details.";
        Xrm.Navigation.openAlertDialog(msg);
    }
    // get the event arguments
    //var formContext = e.getFormContext();
    //var caseStatus = formContext.getAttribute("statuscode").getValue();
    //if (caseStatus === 5 || caseStatus === 90) {
    //    if (results.entities.length > 0) {
    //        e.getEventArgs().preventDefault();
    //        var msg = "One or more GOP+ request(s) is still open in the current case. Please check the GOP+ request(s) and update the JED before proceeding.";
    //        Xrm.Navigation.openAlertDialog(msg);
    //    }
    // if (caseMonitors.entities.length > 0) {
    // e.getEventArgs().preventDefault();
    // var msg = "Please complete the monitor action before closing the case.";
    // Xrm.Navigation.openAlertDialog(msg);
    // }
    //    if (passOuts.entities.length > 0) {
    //        e.getEventArgs().preventDefault();
    //        var msg = "Please fulfilled the missing timestamp on Pass Out before closing the case.";
    //        Xrm.Navigation.openAlertDialog(msg);
    //    }
    //    if (activeGops.entities.length > 0) {
    //        e.getEventArgs().preventDefault();
    //        var msg = "One or more GOP request(s) is still open in the current case. Please check the GOP request(s) and update the GOP before proceeding.";
    //        Xrm.Navigation.openAlertDialog(msg);
    //    }
    //}
}

//Retrive Job end Details where GOP check is false
async function retrieveRecords(formContext) {
    if (formContext.ui.getFormType() == 2) {
        var recordId = formContext.data.entity.getId();
        results = await Xrm.WebApi.retrieveMultipleRecords("jarvis_jobenddetails", "?$filter=(_jarvis_incident_value eq " + recordId + " and jarvis_gopcheckrd eq false)");
        console.log(results);
    }
    return results;
}

//Retrive Case Monitor action which is not complete
async function retrieveCaseMonitorRecords(formContext) {
    if (formContext.ui.getFormType() == 2) {
        var recordId = formContext.data.entity.getId();
        caseMonitors = await Xrm.WebApi.retrieveMultipleRecords("jarvis_casemonitoraction", "?$filter=(_regardingobjectid_value eq " + recordId + " and statuscode ne 2)");
        console.log(caseMonitors);
    }
    return caseMonitors;
}

//Retrive Pass out where ETA field is null
async function retrievePassOutRecords(formContext) {
    if (formContext.ui.getFormType() == 2) {
        var recordId = formContext.data.entity.getId();
        passOuts = await Xrm.WebApi.retrieveMultipleRecords("jarvis_passout", "?$filter=((jarvis_ata eq null or jarvis_atc eq null) and (_jarvis_incident_value eq " + recordId + " and statecode eq 0))");

        console.log(passOuts);
    }
    return passOuts;
}

//Retrive GOP where gop is error 
async function retrieveGOPRecords(formContext) {
    if (formContext.ui.getFormType() == 2) {
        var recordId = formContext.data.entity.getId();
        activeGops = await Xrm.WebApi.retrieveMultipleRecords("jarvis_gop", "?$select=jarvis_approved,_jarvis_incident_value&$filter=(_jarvis_incident_value eq " + recordId + " and jarvis_approved eq false)")
        console.log(activeGops);
    }
    return activeGops;
}

//Status Set For Expert Form
async function caseBPFStatusSetForExpert(executionContext) {
    "use strict";
    var formContext = executionContext.getFormContext();
    console.log("Case On Load");
    var formType = formContext.ui.getFormType();
    if (formType !== 4) {//Read-Only Or Disabled
        var activeStageName = getStageData(executionContext);
        //alert(activeStageName);
        if (activeStageName != null && activeStageName !== undefined) {

            if (activeStageName.toLowerCase() === 'case opening') {
                console.log(activeStageName.toLowerCase());
                formContext.getAttribute("statuscode").setValue(10);
                if (formContext.getAttribute("jarvis_mercuriusstatus").getValue() == null) {
                    formContext.getAttribute("jarvis_mercuriusstatus").setValue(100);
                }
                formContext.data.entity.save();
            }
            else if (activeStageName.toLowerCase() === 'guarantee of payment') {
                console.log(activeStageName.toLowerCase());
                formContext.getAttribute("statuscode").setValue(20);
                formContext.getAttribute("jarvis_mercuriusstatus").setValue(200);
                formContext.data.entity.save();
            }
            else if (activeStageName.toLowerCase() === 'pass out') {
                console.log(activeStageName.toLowerCase());
                formContext.getAttribute("statuscode").setValue(30);
                formContext.getAttribute("jarvis_mercuriusstatus").setValue(300);
                formContext.data.entity.save();
            }
            else if (activeStageName.toLowerCase() === 'waiting for repair start') {
                console.log(activeStageName.toLowerCase());
                formContext.getAttribute("statuscode").setValue(40);
                formContext.getAttribute("jarvis_mercuriusstatus").setValue(400);
                formContext.data.entity.save();
                //makeFieldsMandatory(fieldlist, "required");
            }
            else if (activeStageName.toLowerCase() === 'repair start') {
                formContext.getAttribute("statuscode").setValue(50);
                formContext.getAttribute("jarvis_mercuriusstatus").setValue(500);
                formContext.data.entity.save();
            }

            else if (activeStageName.toLowerCase() === 'repair ongoing') {
                formContext.getAttribute("statuscode").setValue(60);
                formContext.getAttribute("jarvis_mercuriusstatus").setValue(600);
                formContext.data.entity.save();
            }
            else if (activeStageName.toLowerCase() === 'repair finished') {
                formContext.getAttribute("statuscode").setValue(70);
                formContext.getAttribute("jarvis_mercuriusstatus").setValue(700);
                formContext.data.entity.save();
            }

            else if (activeStageName.toLowerCase() === 'repair summary') {
                formContext.getAttribute("statuscode").setValue(80);
                formContext.getAttribute("jarvis_mercuriusstatus").setValue(800);
                formContext.data.entity.save();
            }
            else if (activeStageName.toLowerCase() === 'case closure') {
                formContext.getAttribute("statuscode").setValue(90);
                formContext.getAttribute("jarvis_mercuriusstatus").setValue(900);
                formContext.data.entity.save();
                if (results === null) {
                    results = await retrieveRecords(bpfArguments, formContext);
                }
                if (caseMonitors === null) {
                    caseMonitors = await retrieveCaseMonitorRecords(bpfArguments, formContext);
                }
                if (passOuts === null) {
                    passOuts = await retrievePassOutRecords(bpfArguments, formContext);
                }
                if (activeGops === null) {
                    activeGops = await retrieveGOPRecords(bpfArguments, formContext);
                }
            }
        }

    }
}



function hideDriverSection(executionContext) {
    //"use strict";
    //var formContext = executionContext.getFormContext();
    //if (formContext.getAttribute("jarvis_callerrole").getValue() === 2) {
    //    formContext.ui.tabs.get("tab_register").sections.get("tab_register_section_5")?.setVisible(false);
    //    //alert(formContext.ui.tabs.get("tab_register").sections.get("tab_register_section_5").getVisible())
    //}
    //else {
    //    formContext.ui.tabs.get("tab_register").sections.get("tab_register_section_5")?.setVisible(true);
    //}
}

function showTowingTab(primaryControl) {
    "use strict";
    var formContext = primaryControl;
    var currentDateTime = new Date();
    formContext.getAttribute("jarvis_canvasdate").setValue(currentDateTime);
    formContext.data.entity.save();
    formContext.ui.tabs.get("towingRental")?.setVisible(true);
    formContext.ui.tabs.get("towingRental")?.setFocus();


}

function showAssignTab(primaryControl) {
    "use strict";
    var formContext = primaryControl;
    formContext.ui.tabs.get("assignRD")?.setVisible(true);
    formContext.ui.tabs.get("assignRD")?.setFocus();
}

function populateSubject(executionContext) {
    "use strict";
    var formContext = executionContext.getFormContext();
    var resolutionType = formContext.getAttribute("resolutiontypecode").getValue();
    if (resolutionType != 1000) {
        formContext.getAttribute("subject").setValue("Standard Resolved");
    }

}

function CheckFormIsDirty() {
    alert(Xrm.Page.data.entity.getDataXml());
}
ShowDealerNotFoundNotification = function (executionContext) {
    var level = "WARNING";
    var uniqueId = "DEALERNOTFOUND";
    let formContext = executionContext.getFormContext();
    let createdBy = formContext.getAttribute("createdby")?.getValue();
    let dealerName = formContext.getAttribute("jarvis_homedealer")?.getValue();
    formContext.ui.clearFormNotification(uniqueId);
    if (dealerName !== null && dealerName[0].name !== null && dealerName[0].name != undefined && dealerName[0].name.includes("DEALER NOT FOUND")) {
        var msg = "Dealer not available in OneCase or Mercurius - Contact system support";
        formContext.ui.setFormNotification(msg, level, uniqueId);
    }
    else {
        formContext.ui.clearFormNotification(uniqueId);
    }
}
async function checkOpenCasesForVehicle(executionContext, tabName) {
    "use strict";
    try {
        var formContext = executionContext.getFormContext();
        formContext.ui.clearFormNotification("VEHICLE");
        var formContext = executionContext.getFormContext();
        var vehicle = formContext.getAttribute("jarvis_vehicle").getValue();
        var recordId = formContext.data.entity.getId();
        //var tabSelc = formContext.ui.tabs.get();
        //var tName = tabSelc.getName();
        //if (tabName !== null)
        //  alert(tabName);
        if (vehicle !== null && vehicle[0] !== null) {
            var vehicleID = vehicle[0].id.toString().replace('{', '').replace('}', '');
            var activeStageName = getStageData(executionContext);
            var uniqueId = "VEHICLE";
            if ((activeStageName === null || activeStageName === undefined || (activeStageName.toLowerCase() === 'case opening' && (formContext.ui.getFormType() === 1 || tabName == null))) || (tabName !== null && (tabName === "Register" || tabName === "Validate"))) {
                var incidents = await retrieveRecordsofIncident(vehicleID, recordId);
                console.log(incidents);
                if (incidents !== null && incidents.entities !== null && incidents.entities.length > 0) {
                    var msg = "This vehicle already has a case open. Please check the list of active cases for this vehicle before proceeding further.";
                    var level = "WARNING";
                    var tabObj = formContext.ui.tabs.get("tab_register");
                    if (formContext.ui.getFormType() === 1) {
                        formContext.ui.setFormNotification(msg, level, uniqueId);
                    }
                    else {
                        if (tabObj.getDisplayState() === "expanded")
                            formContext.ui.setFormNotification(msg, level, uniqueId);
                        //else {
                        //var tabObj = formContext.ui.tabs.get("tab_validate");
                        //if (tabObj.getDisplayState() === "expanded")
                        //formContext.ui.setFormNotification(msg, level, uniqueId);
                        //}
                    }
                }
            }
            else if (tabName !== null && tabName !== "Register" && tabName !== "Validate") {
                formContext.ui.clearFormNotification();
            }
            ShowDealerNotFoundNotification(executionContext);
        }
        else {
            formContext.ui.clearFormNotification();
        }
    }
    catch (error) {
        Xrm.Navigation.openAlertDialog(error.message);
    }

}

async function retrieveRecordsofIncident(vehicleID, recordId) {
    var incidents;
    if (recordId !== "" && recordId !== null && recordId !== undefined) {
        recordId = recordId.toString().replace('{', '').replace('}', '');
        incidents = await Xrm.WebApi.retrieveMultipleRecords("incident", "?$filter=(statecode eq 0 and _jarvis_vehicle_value eq " + vehicleID + " and incidentid ne " + recordId + ")");
    }
    else {
        incidents = await Xrm.WebApi.retrieveMultipleRecords("incident", "?$filter=(statecode eq 0 and _jarvis_vehicle_value eq " + vehicleID + ")")
    }
    return incidents;
}

function onSave(executionContext) {
    "use strict";
    var formContext = executionContext.getFormContext();
    formContext.ui.clearFormNotification();
    ShowDealerNotFoundNotification(executionContext);
    DisableTimezone(executionContext);

    var activeStageName = getStageData(executionContext);
    ////476631 - UI Change
    //// Set Caller Phone Number TYPE as Mandatory
    if (formContext.ui.getFormType() == 1 || (formContext.ui.getFormType() == 2 && activeStageName?.toLowerCase() === 'case opening')) {
        formContext.getAttribute("jarvis_callerphonenumbertype")?.setRequiredLevel("required");
    } else {
        formContext.getAttribute("jarvis_callerphonenumbertype")?.setRequiredLevel("recommended");
    }
}

function lockCaseType(executionContext) {
    "use strict";
    var formContext = executionContext.getFormContext();
    console.log("Case On Load");
    var formType = formContext.ui.getFormType();
    //alert(formType);
    if (formType !== 4) {//Read-Only Or Disabled
        var activeStageName = getStageData(executionContext);
        //alert(activeStageName);
        if (activeStageName !== null && activeStageName !== undefined) {
            if (activeStageName.toLowerCase() === 'case opening') {
                console.log(activeStageName.toLowerCase());
                var caseType = formContext.getAttribute("casetypecode").getValue();
                formContext.getControl("casetypecode").setDisabled(false);
                if (caseType == 3)//Query
                {
                    var showTabsArr = ["tab_20", "tab_27", "tab_26", "tab_25", "tab_28", "tab_30", "tab_29", "tab_22", "tab_Query", "tab_register", "tab_collect", "tab_share", "tab_9", "tab_passout", "tab_scheduler", "tab_repairongoing", "tab_repairfinished", "tab_15", "tab_timeline", "tab_summary", "towingRental", "tab_GOP", "Contacts", "tab_MonitorActions"];
                    showHideTabs(formContext, showTabsArr, true);
                    var hideTabsArr = ["tab_20", "tab_27", "tab_26", "tab_25", "tab_28", "tab_30", "tab_29", "tab_register", "tab_collect", "tab_share", "tab_9", "tab_passout", "tab_scheduler", "tab_repairongoing", "tab_repairfinished", "tab_15", "tab_timeline", "tab_summary", "towingRental", "tab_GOP"];
                    showHideTabs(formContext, hideTabsArr, false);
                }
                else {
                    var showTabsArr = ["tab_20", "tab_27", "tab_26", "tab_25", "tab_28", "tab_30", "tab_29", "tab_22", "tab_Query", "tab_register", "tab_collect", "tab_share", "tab_9", "tab_passout", "tab_scheduler", "tab_repairongoing", "tab_repairfinished", "tab_15", "tab_timeline", "tab_summary", "towingRental", "tab_GOP", "Contacts", "tab_MonitorActions"];
                    showHideTabs(formContext, showTabsArr, true);
                    var hideTabsArr = ["tab_20", "tab_27", "tab_26", "tab_25", "tab_28", "tab_22", "tab_Query", "tab_9", "tab_passout", "tab_scheduler", "tab_repairongoing", "tab_repairfinished", "tab_timeline", "tab_summary", "towingRental", "tab_GOP"];
                    showHideTabs(formContext, hideTabsArr, false);
                }//Breakdown


            }
            else {
                formContext.getControl("casetypecode").setDisabled(true);
                // if (caseType == 2)//Query
                // {
                //     var showTabsArr = [ "tab_25"];
                // showHideTabs(formContext, showTabsArr, true);
                // }
            }
        }
    }
}

function retrieveParentCaseDetails(executionContext) {
    "use strict";
    var formContext = executionContext.getFormContext();
    var parentCase = formContext.getAttribute("parentcaseid").getValue();
    if (parentCase !== null && parentCase[0] !== null) {
        var parentCaseID = parentCase[0].id.toString().replace('{', '').replace('}', '');
        Xrm.WebApi.retrieveRecord("incident", "" + parentCaseID + "", "?$select=_customerid_value,_jarvis_homedealer_value,_jarvis_caseserviceline_value,_jarvis_vehicle_value").then(
            function success(result) {
                console.log(result);
                // Columns

                var jarvis_homedealer = result["_jarvis_homedealer_value"]; // Lookup
                if (jarvis_homedealer != null) {
                    var hdLookup = [];   // Creating a new lookup Array
                    hdLookup[0] = {};    // new Object
                    hdLookup[0].id = jarvis_homedealer;  // GUID of the lookup id
                    hdLookup[0].name = result["_jarvis_homedealer_value@OData.Community.Display.V1.FormattedValue"]; // Name of the lookup
                    hdLookup[0].entityType = result["_jarvis_homedealer_value@Microsoft.Dynamics.CRM.lookuplogicalname"]; // Entity Type of the lookup entity
                    formContext.getAttribute("jarvis_homedealer").setValue(hdLookup);
                }
                var jarvis_caseserviceline = result["_jarvis_caseserviceline_value"]; // Lookup
                if (jarvis_caseserviceline != null) {
                    var csLookup = [];   // Creating a new lookup Array
                    csLookup[0] = {};    // new Object
                    csLookup[0].id = jarvis_caseserviceline;
                    csLookup[0].name = result["_jarvis_caseserviceline_value@OData.Community.Display.V1.FormattedValue"]; // Lookup // Name of the lookup
                    csLookup[0].entityType = result["_jarvis_caseserviceline_value@Microsoft.Dynamics.CRM.lookuplogicalname"];// Entity Type of the lookup entity
                    formContext.getAttribute("jarvis_caseserviceline").setValue(csLookup);
                }
                var jarvis_vehicle = result["_jarvis_vehicle_value"]; // Lookup
                if (jarvis_vehicle != null) {
                    var vhLookup = [];
                    vhLookup[0] = {};
                    vhLookup[0].id = jarvis_vehicle;
                    vhLookup[0].name = result["_jarvis_vehicle_value@OData.Community.Display.V1.FormattedValue"]; // Name of the lookup
                    vhLookup[0].entityType = result["_jarvis_vehicle_value@Microsoft.Dynamics.CRM.lookuplogicalname"]; // Entity Type of the lookup entity
                    formContext.getAttribute("jarvis_vehicle").setValue(vhLookup);
                }
                var customerid = result["_customerid_value"];
                if (customerid != null) {
                    var cusLookup = [];
                    cusLookup[0] = {};
                    cusLookup[0].id = result["_customerid_value"]; // Customer
                    cusLookup[0].name = result["_customerid_value@OData.Community.Display.V1.FormattedValue"];
                    cusLookup[0].entityType = result["_customerid_value@Microsoft.Dynamics.CRM.lookuplogicalname"];
                    formContext.getAttribute("customerid").setValue(cusLookup);
                }
            },
            function (error) {
                console.log(error.message);
            }
        );
    }
    else {
        formContext.getAttribute("jarvis_homedealer").setValue(null);
        formContext.getAttribute("jarvis_caseserviceline").setValue(null);
        formContext.getAttribute("jarvis_vehicle").setValue(null);
        formContext.getAttribute("customerid").setValue(null);
    }

}

//Refresh
function CaseQuickCreateFormLoad() //add this function onload form
{
    var subGrid = window.parent.document.getElementById("Subgrid_new_34")
    if (subGrid !== null) {
        if (subGrid.control)
            subGrid.control.add_onRefresh(fnOnRefresh)
        else
            setTimeout(CaseQuickCreateFormLoad, 500);
    } else {
        setTimeout(CaseQuickCreateFormLoad, 500);
    }
}

function fnOnRefresh() {
    setTimeout(function () {
        Xrm.Page.ui.controls.get("Subgrid_new_34").refresh();
    }, 2000) //after 2 sec refresh subgrid
}

function enableForceClose(executionContext) {
    "use strict";
    var flag = true;
    var formContext = executionContext;
    var caseStatus = formContext.getAttribute("statuscode").getValue();
    if (caseStatus != 90 || caseStatus != 1000) {

        flag = false;

    }
    return flag;
}

function enableReactivateButton(executionContext) {
    "use strict";
    var flag = false;
    var formContext = executionContext;
    var caseStatus = formContext.getAttribute("statuscode").getValue();
    if (caseStatus != 1000) {

        flag = true;

    }
    return flag;
}

function clearVehicleNotification(executionContext) {
    "use strict";
    var formContext = executionContext.getFormContext();
    var activeStageName = getStageData(executionContext);
    if (activeStageName !== null && activeStageName !== undefined) {
        if (activeStageName.toLowerCase() === 'case opening') {
            formContext.ui.clearFormNotification("VEHICLE");
        }

    }

}

SetAdditionalLocation = function (executionContext) {
    let formContext = executionContext.getFormContext();
    let RepairingDealer = formContext.getAttribute("jarvis_dealerappointment");
    let assistanceType = formContext.getAttribute("jarvis_assistancetype")?.getValue();
    let location = formContext.getAttribute("jarvis_location")?.getValue();
    let createdBy = formContext.getAttribute("createdby")?.getValue();
    let etaTime = formContext.getAttribute("jarvis_etatimeappointment")?.getValue();
    if (assistanceType !== null && assistanceType === 334030002 || assistanceType === 334030003) {
        ////349955 - OnUpdate of Case (RepairDealer,ETATime,ETADate) to check CreatedBy is Mercurius and location Not Null
        if (createdBy !== null && createdBy[0].name !== null && createdBy[0].name.toUpperCase().indexOf("MERCURIUS") !== -1 && location !== null) {
            return;
        }
        if (RepairingDealer != null) {
            let RDvalue = RepairingDealer.getValue();
            if (RDvalue) {
                let record_id = RepairingDealer.getValue()[0].id;
                record_id = record_id.replace('{', '').replace('}', '');
                Xrm.WebApi.retrieveRecord("account", record_id, "?$select=jarvis_responsableunitid,name").then(
                    function success(result) {
                        var timezonelabel = formContext.getAttribute("jarvis_timezonelabel").getValue();
                        if (formContext.getAttribute("jarvis_location") != null && formContext.getAttribute("jarvis_etatimeappointment") != null && formContext.getAttribute("jarvis_etadateappointment") != null) {
                            if (formContext.getAttribute("jarvis_etatimeappointment").getValue() != null && formContext.getAttribute("jarvis_etadateappointment").getValue() != null && timezonelabel != null) {
                                var dateTimeValue = formContext.getAttribute("jarvis_etadateappointment")?.getValue();
                                var datevalue = { year: 'numeric', month: '2-digit', day: '2-digit' };
                                var formattedDate = dateTimeValue.toLocaleDateString(undefined, datevalue);
                                var responisbleUnitid = result.jarvis_responsableunitid != null ? result.jarvis_responsableunitid : "";
                                var dealername = result.name != null ? result.name : "";
                                var isoFormatDate = formatDate(dateTimeValue, etaTime);

                                formContext.getAttribute("jarvis_location").controls.forEach(
                                    function (control, i) {
                                        formContext.getAttribute("jarvis_location").setValue(
                                            "Responsible Unit ID: " + responisbleUnitid +
                                            "\nDealer Name: " + dealername +
                                            // "\nETA: " + etaTime + " " + formattedDate + " " + timezonelabel);
                                            "\nETA: " + isoFormatDate + " " + timezonelabel);
                                    });
                            }
                        }
                    }
                );
            }
        }
    }
}

LockAppointmentfields = async function (executionContext) {
    var formContext = executionContext.getFormContext();
    let assistanceType = formContext.getAttribute("jarvis_assistancetype")?.getValue();
    let appointmentRD = formContext.getAttribute("jarvis_dealerappointment");
    let appointmentETADate = formContext.getAttribute("jarvis_etadateappointment");
    let appointmentETATime = formContext.getAttribute("jarvis_etatimeappointment");
    var formType = formContext.ui.getFormType();
    if (formType !== 4) {
        //var activeStageName = getStageData(executionContext);
        //alert(activeStageName);
        var isPassOutCreated = false;
        var passOuts = await getPassOuts(formContext);
        if (passOuts !== null && passOuts.entities !== null && passOuts.entities.length > 0) {
            isPassOutCreated = true;
        }
        //if (activeStageName !== null && activeStageName !== undefined) {
        //if (activeStageName.toLowerCase() !== 'case opening' && activeStageName.toLowerCase() !== 'guarantee of payment') 
        if (isPassOutCreated) {
            if (assistanceType != null) {
                if (assistanceType === 334030002 || assistanceType === 334030003) {
                    if (appointmentRD && appointmentETADate && appointmentETATime) {
                        repairingdealer = formContext.getAttribute("jarvis_dealerappointment")?.getValue();
                        var fieldList = [appointmentRD, appointmentETADate, appointmentETATime];
                        if (repairingdealer != null) {
                            fieldList.forEach(function (appointmentfields) {
                                appointmentfields.controls.forEach(
                                    function (control, i) {
                                        control.setDisabled(true);
                                    });

                            });

                        }
                        else {
                            fieldList.forEach(function (appointmentfields) {
                                appointmentfields.controls.forEach(
                                    function (control, i) {
                                        control.setDisabled(false);
                                    });

                            });
                        }
                    }
                }
            }
        }
        //}
    }
}

function categoryOnChange(executionContext) {
    "use strict";
    clearSubCategory(executionContext, "jarvis_querycategory", "jarvis_querysubcategory");
}

function alertCategoryOnChange(executionContext) {
    "use strict";
    clearSubCategory(executionContext, "jarvis_escalationmaincategory", "jarvis_escalationsubcategory");
}

function clearSubCategory(executionContext, category, subCategory) {
    "use strict";
    var formContext = executionContext.getFormContext();
    var category = formContext.getAttribute(category).getValue();
    if (category !== null) {
        formContext.getAttribute(subCategory).setValue(null);
    }
}

function SetTimezoneByCountry(executionContext) {
    "use strict";
    var formContext = executionContext.getFormContext();
    var activeStageName = getStageData(executionContext);
    // if (activeStageName !== null && activeStageName !== undefined) {
    //    if (activeStageName.toLowerCase() === 'case opening') {
    var country = formContext.getAttribute("jarvis_country").getValue();
    var timezone = formContext.getAttribute("jarvis_timezone").getValue();
    if (country !== null) {
        var countryID = country[0].id.toString().replace('{', '').replace('}', '');
        Xrm.WebApi.retrieveRecord("jarvis_country", countryID, "?$select=jarvis_timezone").then(
            function success(result) {
                var defaultTimezoneByCountry = result["jarvis_timezone"];
                var defaultTimezone = 105;
                if (defaultTimezoneByCountry !== null) {
                    formContext.getAttribute("jarvis_timezone").setValue(defaultTimezoneByCountry);
                }
                else {
                    formContext.getAttribute("jarvis_timezone").setValue(defaultTimezone);
                }
                SetTimezoneLabel(executionContext);
            }
        );
    }
    //  }
    // }
}

function SetTimezoneLabel(executionContext) {
    "use strict";
    var formContext = executionContext.getFormContext();
    var timezone = formContext.getAttribute("jarvis_timezone").getValue();
    var userinterfacename = '';
    var timezoneLabel = '';
    if (timezone !== null) {
        Xrm.WebApi.retrieveMultipleRecords("timezonedefinition", "?$select=userinterfacename&$filter=timezonecode eq " + timezone).then(
            function success(results) {
                for (var i = 0; i < results.entities.length; i++) {
                    var result = results.entities[i];
                    userinterfacename = result["userinterfacename"];
                    if (userinterfacename != null) {
                        timezoneLabel = userinterfacename.substring(0, 11);
                        formContext.getAttribute("jarvis_timezonelabel").setValue(timezoneLabel);
                        SetAdditionalLocation(executionContext);
                    }
                }
            }
        );
    }
    else {
        formContext.getAttribute("jarvis_timezonelabel").setValue(null);
        SetAdditionalLocation(executionContext);
    }

}

function DisableTimezone(executionContext) {
    "use strict";
    var formContext = executionContext.getFormContext();
    formContext.getControl("jarvis_timezonelabel").setVisible(false);
    var activeStageName = getStageData(executionContext);
    if (activeStageName !== null && activeStageName !== undefined) {
        if (activeStageName.toLowerCase() === 'case opening') {
            formContext.getControl("jarvis_timezone").setDisabled(false);
        }
    }
    SetEntityTypeForCustomer(executionContext);
}

function SetEntityTypeForCustomer(executionContext) {
    var formContext = executionContext.getFormContext();
    formContext.getControl("header_process_customerid").setEntityTypes(["account"]);
}

function FilterHDLookup(executionContext) {
    var dealerFilter = "<filter type='and'>" +
        "<condition attribute='jarvis_accounttype' operator='in'>" +
        "<value>334030001</value>" +
        "<value>334030003</value>" +
        "<value>334030002</value>" +
        "</condition>" +
        "</filter>";
    var homeDealerLookup = executionContext.getFormContext().getControl("jarvis_homedealer");
    if (homeDealerLookup != null) {
        //executionContext.getFormContext().getControl("jarvis_homedealer").addCustomFilter(dealerFilter);
    }
}

function FilterCustomerLookup(executionContext) {
    var customerFilter = "<filter type='and'>" +
        "<condition attribute='jarvis_accounttype' operator='eq' value='334030000'/>" +
        "</filter>";
    var customerLookup = executionContext.getFormContext().getControl("customerid");
    if (customerLookup != null) {
        executionContext.getFormContext().getControl("customerid").addCustomFilter(customerFilter);
    }

}

function FilterHDBPFLookup(executionContext) {
    var dealerFilter = "<filter type='and'>" +
        "<condition attribute='jarvis_accounttype' operator='in'>" +
        "<value>334030001</value>" +
        "<value>334030003</value>" +
        "<value>334030002</value>" +
        "</condition>" +
        "</filter>";
    var homedealerBPFControl = executionContext.getFormContext().getControl("header_process_jarvis_homedealer");
    if (homedealerBPFControl != null) {
        executionContext.getFormContext().getControl("header_process_jarvis_homedealer").addCustomFilter(dealerFilter);
    }
}

function FilterCustomerBPFLookup(executionContext) {
    var customerFilter = "<filter type='and'>" +
        "<condition attribute='jarvis_accounttype' operator='eq' value='334030000'/>" +
        "</filter>";
    var customerBPFControl = executionContext.getFormContext().getControl("header_process_customerid");
    if (customerBPFControl != null) {
        executionContext.getFormContext().getControl("header_process_customerid").addCustomFilter(customerFilter);
    }
}

function AddPreSearchToLookupCase(executionContext) {
    var formContext = executionContext.getFormContext();
    var homeDealerLookup = formContext.getAttribute("jarvis_homedealer");
    if (homeDealerLookup != null) {
        // homeDealerLookup.controls.forEach(control => control.addPreSearch(FilterHDLookup));
    }
    var homedealerBPFControl = executionContext.getFormContext().getControl("header_process_jarvis_homedealer");
    if (homedealerBPFControl != null) {
        executionContext.getFormContext().getControl("header_process_jarvis_homedealer").addPreSearch(FilterHDBPFLookup);
    }
    var customerBPFControl = executionContext.getFormContext().getControl("header_process_customerid");
    if (customerBPFControl != null) {
        executionContext.getFormContext().getControl("header_process_customerid").addPreSearch(FilterCustomerBPFLookup);
    }
    var customerLookup = formContext.getAttribute("customerid");
    if (customerLookup != null) {
        customerLookup.controls.forEach(control => control.addPreSearch(FilterCustomerLookup));
    }
}

async function getPassOuts(formContext) {
    var passOuts = null;
    var formType = formContext.ui.getFormType();
    if (formType !== 1) {
        var recordId = formContext.data.entity.getId();
        passOuts = await Xrm.WebApi.retrieveMultipleRecords("jarvis_passout", "?$filter=(_jarvis_incident_value eq " + recordId + ")");
    }
    return passOuts;
}

function formatDate(dateToFormat, timeToFormat) {
    let day = dateToFormat.getDate().toString().padStart(2, '0');
    let month = (dateToFormat.getMonth() + 1).toString().padStart(2, '0');
    let year = dateToFormat.getFullYear();
    let hours = timeToFormat.substring(0, 2);
    let minutes = timeToFormat.substring(2, 4);

    return hours + ':' + minutes + ' ' + year + '-' + month + '-' + day;
}
