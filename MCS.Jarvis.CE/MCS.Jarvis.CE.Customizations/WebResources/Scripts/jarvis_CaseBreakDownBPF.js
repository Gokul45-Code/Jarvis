//function checkStageChangedAuto(executionContext) {
//    "use strict";
//    var formContext = executionContext.getFormContext();
//    //formContext.data.process.addOnStageChange(stageOnChange);
//    formContext.data.process.addOnStageChange(caseBPFValidation);
//}


// function autoRefresh(executionContext) {
//     "use strict";
//     var formContext = executionContext.getFormContext();
//     var activeStageName = getStageData(formContext);
//     //console.log(activeStageName.toLowerCase());
//    // if (activeStageName.toLowerCase() === 'pass out') {
//   //      formContext.data.refresh(save).then(successCallback, errorCallback);
//     //}
// }

//function showTabsForCaseReopened(executionContext) {
//    "use strict";
//    var formContext = executionContext.getFormContext();
//    var caseStatus = formContext.getAttribute("statuscode").getValue();
//    if (caseStatus === 85 || caseStatus === 5) {
//        formContext.getControl("casetypecode").setDisabled(true);
//        var showTabsArr = ["tab_20", "tab_closure", "tab_15", "tab_timeline", "tab_summary", "tab_GOP"];
//        showHideTabs(formContext, showTabsArr, true);
//        var hideTabsArr = ["tab_22", "tab_Query", "tab_register", "tab_validate", "tab_collect", "tab_share", "tab_9", "tab_passout", "tab_scheduler", "tab_waitingforrepair", "tab_repairstart",
//            "tab_repairongoing", "tab_repairfinished", "towingRental"];
//        showHideTabs(formContext, hideTabsArr, false);
//    }

//}

//function checkStageChangedAutoExpertForm(executionContext) {
//    "use strict";
//    var formContext = executionContext.getFormContext();
//    formContext.data.process.addOnStageChange(caseBPFStatusSetForExpert);
//}
//var results = null;
//var formContext = null
//async function onLoad(e) {
//    "use strict";
//    formContext = e.getFormContext();
//    formContext.data.process.addOnPreProcessStatusChange(handleStageMovement);
//    results = await retrieveRecords(formContext);
//    var gridContext = formContext.getControl("Subgrid_new_24");
//    if (gridContext !== null) {
//        gridContext.addOnLoad(functionTriggeredgetOnSubgridRefresh)
//    }
    //var activeStageName = getStageData();
    //if (activeStageName !== null && activeStageName.toLowerCase() === 'case opening') {
    //if (formContext.ui.tabs.get("tab_20") !== null) {
    //formContext.ui.tabs.get("tab_20").setVisible(false);
    //}
    //}
//}
//async function functionTriggeredgetOnSubgridRefresh() {
//    "use strict";
//    setTimeout(get, 1000);
//    //results = await retrieveRecords(formContext);
//}
//async function get() {
//    "use strict";
//    await retrieveRecords(formContext);
//}
//async function handleStageMovement(e) {
//    "use strict";
//    // get the event arguments

//    if (results.entities.length > 0) {
//        e.getEventArgs().preventDefault();
//        var msg = "One or more GOP+ request(s) is still open in the current case. Please check the GOP+ request(s) and update the JED before proceeding.";
//        Xrm.Navigation.openAlertDialog(msg);
//    }
//}

//async function retrieveRecords(formContext) {
//    "use strict";
//    if (formContext.ui.getFormType() !== 1) {
//        var recordId = formContext.data.entity.getId();
//        results = await Xrm.WebApi.retrieveMultipleRecords("jarvis_jobenddetails", "?$filter=(_jarvis_incident_value eq " + recordId + " and jarvis_gopcheckrd eq false)");
//        //console.log(results);
//    }
//    return results;
//}

// async function caseBPFValidation(executionContext) {
//     "use strict";
//     var formContext = executionContext.getFormContext();
//     //console.log("Case On Load");
//     var formType = formContext.ui.getFormType();
//     //alert(formType);
//     if (formType !== 4) {//Read-Only Or Disabled
//         var activeStageName = getStageData(formContext);
//         //alert(activeStageName);
//         if (activeStageName !== null && activeStageName !== undefined) {
//             if (formContext.getControl("header_process_jarvis_totalgoplimitinapproved") !== null)
//                 formContext.getControl("header_process_jarvis_totalgoplimitinapproved").setDisabled(true);
//             if (formContext.getControl("header_process_jarvis_totalcurrencyinapproved") !== null)
//                 formContext.getControl("header_process_jarvis_totalcurrencyinapproved").setDisabled(true);
//             if (formContext.getControl("header_process_jarvis_actualcausefault") !== null)
//                 formContext.getControl("header_process_jarvis_actualcausefault").setDisabled(true);
//             if (formContext.getControl("header_process_jarvis_eta") !== null)
//                 formContext.getControl("header_process_jarvis_eta").setDisabled(true);
//             if (formContext.getControl("header_process_jarvis_etc") !== null)
//                 formContext.getControl("header_process_jarvis_etc").setDisabled(true);
//             if (formContext.getControl("header_process_jarvis_ata") !== null)
//                 formContext.getControl("header_process_jarvis_ata").setDisabled(true);
//             if (formContext.getControl("header_process_jarvis_atc") !== null)
//                 formContext.getControl("header_process_jarvis_atc").setDisabled(true);
//             if (formContext.getControl("header_process_jarvis_totalpassoutamount") !== null)
//                 formContext.getControl("header_process_jarvis_totalpassoutamount").setDisabled(true);
//             if (formContext.getControl("header_process_jarvis_totalpassoutcurrency") !== null)//jarvis_customerinformed
//                 formContext.getControl("header_process_jarvis_totalpassoutcurrency").setDisabled(true);
//                 if (formContext.getControl("header_process_jarvis_customerinformed") !== null)
//                 {//jarvis_customerinformed getAttribute().getValue()
//                     var cInformed = formContext.getControl("header_process_jarvis_customerinformed").getAttribute().getValue();
//                     if(cInformed===true)//Yes
//                     {
//                         formContext.getControl("header_process_jarvis_customerinformed").setDisabled(true);
//                     }

//                 }
//             if (activeStageName.toLowerCase() === 'case opening') {
//                 //console.log(activeStageName.toLowerCase());
//                 var caseType = formContext.getAttribute("casetypecode").getValue();
//                 formContext.getControl("casetypecode").setDisabled(false);
//                 if (caseType === 3)//Query
//                 {
//                     var showTabsArr = ["tab_22", "tab_Query", "tab_register", "tab_validate", "tab_collect", "tab_share", "tab_9", "tab_passout", "tab_scheduler", "tab_waitingforrepair", "tab_repairstart",
//                         "tab_repairongoing", "tab_repairfinished", "tab_closure", "tab_15", "tab_timeline", "tab_summary", "towingRental", "tab_GOP"];
//                     showHideTabs(formContext, showTabsArr, true);
//                     var hideTabsArr = ["Contacts", "tab_20", "tab_register", "tab_validate", "tab_collect", "tab_share", "tab_9", "tab_passout", "tab_scheduler", "tab_waitingforrepair", "tab_repairstart",
//                         "tab_repairongoing", "tab_repairfinished", "tab_closure", "tab_15", "tab_timeline", "tab_summary", "towingRental", "tab_GOP"];
//                     showHideTabs(formContext, hideTabsArr, false);
//                 }
//                 else {
//                     var showTabsArr = ["tab_22", "tab_Query", "tab_register", "tab_validate", "tab_collect", "tab_share", "tab_9", "tab_passout", "tab_scheduler", "tab_waitingforrepair", "tab_repairstart",
//                         "tab_repairongoing", "tab_repairfinished", "tab_closure", "tab_15", "tab_timeline", "tab_summary", "towingRental", "tab_GOP"];
//                     showHideTabs(formContext, showTabsArr, true);
//                     var hideTabsArr = ["tab_20", "tab_22", "tab_Query", "tab_9", "tab_passout", "tab_scheduler", "tab_waitingforrepair", "tab_repairstart",
//                         "tab_repairongoing", "tab_repairfinished", "tab_closure", "tab_15", "tab_timeline", "tab_summary", "towingRental", "tab_GOP"];
//                     showHideTabs(formContext, hideTabsArr, false);
//                 }//Breakdown


//             }
//             else if (activeStageName.toLowerCase() === 'guarantee of payment') {
//                 //console.log(activeStageName.toLowerCase());
//                 formContext.ui.clearFormNotification("VEHICLE");
//                 formContext.getControl("casetypecode").setDisabled(true);
//                 var showTabsArr = ["tab_20", "tab_register", "tab_validate", "tab_collect", "tab_share", "tab_9", "tab_passout", "tab_scheduler", "tab_waitingforrepair", "tab_repairstart",
//                     "tab_repairongoing", "tab_repairfinished", "tab_closure", "tab_15", "tab_timeline", "tab_summary", "towingRental", "tab_GOP"];
//                 showHideTabs(formContext, showTabsArr, true);
//                 var hideTabsArr = ["tab_22", "tab_Query", "tab_register", "tab_validate", "tab_collect", "tab_share", "tab_passout", "tab_scheduler", "tab_waitingforrepair", "tab_repairstart",
//                     "tab_repairongoing", "tab_repairfinished", "tab_closure", "towingRental", "tab_GOP"];
//                 showHideTabs(formContext, hideTabsArr, false);
//                 if (formContext.getControl("header_process_jarvis_totalgoplimitinapproved") !== null)
//                     formContext.getControl("header_process_jarvis_totalgoplimitinapproved").setDisabled(true);
//                 if (formContext.getControl("header_process_jarvis_totalcurrencyinapproved") !== null)
//                     formContext.getControl("header_process_jarvis_totalcurrencyinapproved").setDisabled(true);

//             }
//             else if (activeStageName.toLowerCase() === 'pass out') {
//                 //console.log(activeStageName.toLowerCase());
//                 formContext.getControl("casetypecode").setDisabled(true);
//                 var showTabsArr = ["tab_20", "tab_register", "tab_validate", "tab_collect", "tab_share", "tab_9", "tab_passout", "tab_scheduler", "tab_waitingforrepair", "tab_repairstart",
//                     "tab_repairongoing", "tab_repairfinished", "tab_closure", "tab_15", "tab_timeline", "tab_summary", "towingRental", "tab_GOP"];
//                 showHideTabs(formContext, showTabsArr, true);
//                 var hideTabsArr = ["tab_22", "tab_Query", "tab_register", "tab_validate", "tab_collect", "tab_share", "tab_9", "tab_waitingforrepair", "tab_repairstart",
//                     "tab_repairongoing", "tab_repairfinished", "tab_closure", "tab_GOP"];
//                 showHideTabs(formContext, hideTabsArr, false);
//                 if (formContext.getControl("header_process_jarvis_totalpassoutamount") !== null)
//                     formContext.getControl("header_process_jarvis_totalpassoutamount").setDisabled(true);
//                 if (formContext.getControl("header_process_jarvis_totalpassoutcurrency") !== null)
//                     formContext.getControl("header_process_jarvis_totalpassoutcurrency").setDisabled(true);
//                 var currentDateTime = new Date();
//                 //formContext.getAttribute("jarvis_canvasdate").setValue(currentDateTime);

//             }
//             else if (activeStageName.toLowerCase() === 'eta technician') {
//                 //console.log(activeStageName.toLowerCase());
//                 formContext.getControl("casetypecode").setDisabled(true);
//                 var showTabsArr = ["tab_20", "tab_register", "tab_validate", "tab_collect", "tab_share", "tab_9", "tab_passout", "tab_scheduler", "tab_waitingforrepair", "tab_repairstart",
//                     "tab_repairongoing", "tab_repairfinished", "tab_closure", "tab_15", "tab_timeline", "tab_summary", "towingRental", "tab_GOP"];
//                 showHideTabs(formContext, showTabsArr, true);
//                 var hideTabsArr = ["tab_22", "tab_Query", "tab_register", "tab_validate", "tab_collect", "tab_share", "tab_9", "tab_passout", "tab_scheduler", "tab_repairstart",
//                     "tab_repairongoing", "tab_repairfinished", "tab_closure", "tab_15", "towingRental", "tab_GOP"];
//                 showHideTabs(formContext, hideTabsArr, false);
//                 if (formContext.getControl("header_process_jarvis_eta") !== null)
//                     formContext.getControl("header_process_jarvis_eta").setDisabled(true);
//                 if (formContext.getControl("header_process_jarvis_etavalidation") !== null)
//                     formContext.getControl("header_process_jarvis_etavalidation").setDisabled(true);

//             }
//             else if (activeStageName.toLowerCase() === 'waiting for repair start') {
//                 formContext.getControl("casetypecode").setDisabled(true);
//                 var showTabsArr = ["tab_20", "tab_register", "tab_validate", "tab_collect", "tab_share", "tab_9", "tab_passout", "tab_scheduler", "tab_waitingforrepair", "tab_repairstart",
//                     "tab_repairongoing", "tab_repairfinished", "tab_closure", "tab_15", "tab_timeline", "tab_summary", "towingRental", "tab_GOP"];
//                 showHideTabs(formContext, showTabsArr, true);
//                 var hideTabsArr = ["tab_22", "tab_Query", "tab_register", "tab_validate", "tab_collect", "tab_share", "tab_9", "tab_passout", "tab_scheduler", "tab_waitingforrepair",
//                     "tab_repairongoing", "tab_repairfinished", "tab_closure", "tab_15", "towingRental", "tab_GOP"];
//                 showHideTabs(formContext, hideTabsArr, false);
//                 if (formContext.getControl("header_process_jarvis_ata") !== null)
//                     formContext.getControl("header_process_jarvis_ata").setDisabled(true);
//                 if (formContext.getControl("header_process_jarvis_atavalidation") !== null)
//                     formContext.getControl("header_process_jarvis_atavalidation").setDisabled(true);

//             }

//             else if (activeStageName.toLowerCase() === 'repair ongoing') {
//                 formContext.getControl("casetypecode").setDisabled(true);
//                 var showTabsArr = ["tab_20", "tab_repairongoing", "tab_15", "tab_timeline", "tab_summary"];
//                 showHideTabs(formContext, showTabsArr, true);
//                 var hideTabsArr = ["tab_22", "tab_Query", "tab_register", "tab_validate", "tab_collect", "tab_share", "tab_9", "tab_passout", "tab_scheduler", "tab_waitingforrepair", "tab_repairstart",
//                     "tab_repairfinished", "tab_closure", "towingRental", "tab_GOP"];
//                 showHideTabs(formContext, hideTabsArr, false);


//             }
//             else if (activeStageName.toLowerCase() === 'repair finished') {
//                 formContext.getControl("casetypecode").setDisabled(true);
//                 var showTabsArr = ["tab_20", "tab_repairfinished", "tab_15", "tab_timeline", "tab_summary"];
//                 showHideTabs(formContext, showTabsArr, true);
//                 var hideTabsArr = ["tab_22", "tab_Query", "tab_register", "tab_validate", "tab_collect", "tab_share", "tab_9", "tab_passout", "tab_scheduler", "tab_waitingforrepair", "tab_repairstart",
//                     "tab_repairongoing", "tab_closure", "towingRental", "tab_GOP"];
//                 showHideTabs(formContext, hideTabsArr, false);


//             }

//             else if (activeStageName.toLowerCase() === 'repair summary') {
//                 formContext.getControl("casetypecode").setDisabled(true);
//                 var showTabsArr = ["tab_20", "tab_repairfinished", "tab_15", "tab_timeline", "tab_summary"];
//                 showHideTabs(formContext, showTabsArr, true);
//                 var hideTabsArr = ["tab_22", "tab_Query", "tab_register", "tab_validate", "tab_collect", "tab_share", "tab_9", "tab_passout", "tab_scheduler", "tab_waitingforrepair", "tab_repairstart",
//                     "tab_repairongoing", "tab_closure", "towingRental", "tab_GOP"];
//                 showHideTabs(formContext, hideTabsArr, false);


//             }
//             else if (activeStageName.toLowerCase() === 'case closure') {
//                 formContext.getControl("casetypecode").setDisabled(true);
//                 var showTabsArr = ["tab_closure", "tab_15", "tab_timeline", "tab_summary","tab_20","tab_GOP"];
//                 showHideTabs(formContext, showTabsArr, true);
//                 var hideTabsArr = ["tab_22", "tab_Query", "tab_register", "tab_validate", "tab_collect", "tab_share", "tab_9", "tab_passout", "tab_scheduler", "tab_waitingforrepair", "tab_repairstart",
//                     "tab_repairongoing", "tab_repairfinished", "towingRental"];
//                 showHideTabs(formContext, hideTabsArr, false);
//                 if (results === null) {
//                     results = await retrieveRecords(formContext);
//                 }
//             }
//             else if (activeStageName.toLowerCase() === 'ongoing') {
//                 formContext.getControl("casetypecode").setDisabled(true);
//                 var showTabsArr = ["tab_22", "tab_Query", "tab_register", "tab_validate", "tab_collect", "tab_share", "tab_9", "tab_passout", "tab_scheduler", "tab_waitingforrepair", "tab_repairstart",
//                     "tab_repairongoing", "tab_repairfinished", "tab_closure", "tab_15", "tab_timeline", "tab_summary", "towingRental", "tab_GOP"];
//                 showHideTabs(formContext, showTabsArr, true);
//                 var hideTabsArr = ["tab_20", "tab_register", "tab_validate", "tab_collect", "tab_share", "tab_9", "tab_passout", "tab_scheduler", "tab_waitingforrepair", "tab_repairstart",
//                     "tab_repairongoing", "tab_repairfinished", "tab_closure", "tab_15", "tab_timeline", "tab_summary", "towingRental", "tab_GOP"];
//                 showHideTabs(formContext, hideTabsArr, false);

//             }
//             else if (activeStageName.toLowerCase() === 'solved') {
//                 var showTabsArr = ["tab_22", "tab_Query", "tab_register", "tab_validate", "tab_collect", "tab_share", "tab_9", "tab_passout", "tab_scheduler", "tab_waitingforrepair", "tab_repairstart",
//                     "tab_repairongoing", "tab_repairfinished", "tab_closure", "tab_15", "tab_timeline", "tab_summary", "towingRental", "tab_GOP"];
//                 showHideTabs(formContext, showTabsArr, true);
//                 var hideTabsArr = ["tab_20", "tab_register", "tab_validate", "tab_collect", "tab_share", "tab_9", "tab_passout", "tab_scheduler", "tab_waitingforrepair", "tab_repairstart",
//                     "tab_repairongoing", "tab_repairfinished", "tab_closure", "tab_15", "tab_timeline", "tab_summary", "towingRental", "tab_GOP"];
//                 showHideTabs(formContext, hideTabsArr, false);

//             }
//             else if (activeStageName.toLowerCase() === 'credit to hd') {
//                 var showTabsArr = ["tab_22", "tab_Query", "tab_register", "tab_validate", "tab_collect", "tab_share", "tab_9", "tab_passout", "tab_scheduler", "tab_waitingforrepair", "tab_repairstart",
//                     "tab_repairongoing", "tab_repairfinished", "tab_closure", "tab_15", "tab_timeline", "tab_summary", "towingRental", "tab_GOP"];
//                 showHideTabs(formContext, showTabsArr, true);
//                 var hideTabsArr = ["tab_20", "tab_register", "tab_validate", "tab_collect", "tab_share", "tab_9", "tab_passout", "tab_scheduler", "tab_waitingforrepair", "tab_repairstart",
//                     "tab_repairongoing", "tab_repairfinished", "tab_closure", "tab_15", "tab_timeline", "tab_summary", "towingRental", "tab_GOP"];
//                 showHideTabs(formContext, hideTabsArr, false);

//             }
//             else if (activeStageName.toLowerCase() === 'closed') {
//                 var showTabsArr = ["tab_22", "tab_Query", "tab_register", "tab_validate", "tab_collect", "tab_share", "tab_9", "tab_passout", "tab_scheduler", "tab_waitingforrepair", "tab_repairstart",
//                     "tab_repairongoing", "tab_repairfinished", "tab_closure", "tab_15", "tab_timeline", "tab_summary", "towingRental", "tab_GOP"];
//                 showHideTabs(formContext, showTabsArr, true);
//                 var hideTabsArr = ["tab_20", "tab_register", "tab_validate", "tab_collect", "tab_share", "tab_9", "tab_passout", "tab_scheduler", "tab_waitingforrepair", "tab_repairstart",
//                     "tab_repairongoing", "tab_repairfinished", "tab_closure", "tab_15", "tab_timeline", "tab_summary", "towingRental", "tab_GOP"];
//                 showHideTabs(formContext, hideTabsArr, false);

//             }
//             //#94752-case escalation
//             if (activeStageName.toLowerCase() !== 'case opening') {
//                 //console.log(activeStageName.toLowerCase());
//                 var caseType = formContext.getAttribute("casetypecode").getValue();
//                 if (caseType === 2)//Query
//                 {
//                     var showTabsArr = [ "tab_25"];
//                 showHideTabs(formContext, showTabsArr, true);
//                 }

//             }

//         }
//         else {
//             //alert('inside else');
//             var hideTabsArr = ["tab_20", "tab_22", "tab_Query", "tab_register", "tab_validate", "tab_collect", "tab_share", "tab_9", "tab_passout", "tab_scheduler", "tab_waitingforrepair", "tab_repairstart",
//                 "tab_repairongoing", "tab_repairfinished", "tab_closure", "tab_15", "tab_timeline", "tab_summary", "towingRental", "tab_GOP"];
//             showHideTabs(formContext, hideTabsArr, false);
//         }
//     }


// }

//Status Set For Expert Form
// async function caseBPFStatusSetForExpert(executionContext) {
//     "use strict";
//     var formContext = executionContext.getFormContext();
//     //console.log("Case On Load");
//     var formType = formContext.ui.getFormType();
//     if (formType !== 4) {//Read-Only Or Disabled
//         var activeStageName = getStageData(formContext);
//         //alert(activeStageName);
//         if (activeStageName !== null && activeStageName !== undefined) {

//             if (activeStageName.toLowerCase() === 'case opening') {
//                 //console.log(activeStageName.toLowerCase());
//                 formContext.getAttribute("statuscode").setValue(10);
//                 if (formContext.getAttribute("jarvis_mercuriusstatus").getValue() === null) {
//                     formContext.getAttribute("jarvis_mercuriusstatus").setValue(100);
//                 }
//                 formContext.data.entity.save();
//             }
//             else if (activeStageName.toLowerCase() === 'guarantee of payment') {
//                 //console.log(activeStageName.toLowerCase());
//                 formContext.getAttribute("statuscode").setValue(20);
//                 formContext.getAttribute("jarvis_mercuriusstatus").setValue(200);
//                 formContext.data.entity.save();
//             }
//             else if (activeStageName.toLowerCase() === 'pass out') {
//                 //console.log(activeStageName.toLowerCase());
//                 formContext.getAttribute("statuscode").setValue(30);
//                 formContext.getAttribute("jarvis_mercuriusstatus").setValue(300);
//                 formContext.data.entity.save();
//             }
//             else if (activeStageName.toLowerCase() === 'waiting for repair start') {
//                 //console.log(activeStageName.toLowerCase());
//                 formContext.getAttribute("statuscode").setValue(40);
//                 formContext.getAttribute("jarvis_mercuriusstatus").setValue(400);
//                 formContext.data.entity.save();
//                 //makeFieldsMandatory(fieldlist, "required");
//             }
//             else if (activeStageName.toLowerCase() === 'repair start') {
//                 formContext.getAttribute("statuscode").setValue(50);
//                 formContext.getAttribute("jarvis_mercuriusstatus").setValue(500);
//                 formContext.data.entity.save();
//             }

//             else if (activeStageName.toLowerCase() === 'repair ongoing') {
//                 formContext.getAttribute("statuscode").setValue(60);
//                 formContext.getAttribute("jarvis_mercuriusstatus").setValue(600);
//                 formContext.data.entity.save();
//             }
//             else if (activeStageName.toLowerCase() === 'repair finished') {
//                 formContext.getAttribute("statuscode").setValue(70);
//                 formContext.getAttribute("jarvis_mercuriusstatus").setValue(700);
//                 formContext.data.entity.save();
//             }

//             else if (activeStageName.toLowerCase() === 'repair summary') {
//                 formContext.getAttribute("statuscode").setValue(80);
//                 formContext.getAttribute("jarvis_mercuriusstatus").setValue(800);
//                 formContext.data.entity.save();
//             }
//             else if (activeStageName.toLowerCase() === 'case closure') {
//                 formContext.getAttribute("statuscode").setValue(90);
//                 formContext.getAttribute("jarvis_mercuriusstatus").setValue(900);
//                 formContext.data.entity.save();
//                 if (results === null) {
//                     results = await retrieveRecords(bpfArguments, formContext);
//                 }
//             }
//         }

//     }
// }

// function getStageData(formContext) {
//     "use strict";

//     var activeStageId, activeStageName;

//     //Get the current active stage of the process
//     var activeStage = formContext.data.process.getActiveStage();

//     if (activeStage !== null && activeStage !== undefined) {
//         //Get the ID of the current stage
//         activeStageId = activeStage.getId();

//         //Get the Name of the current stage
//         activeStageName = activeStage.getName();

//     }


//     return activeStageName;
// }

// function showHideTabs(formContext, tabArr, isVisible) {
//     "use strict";
//     for (var ii = 0; ii < tabArr.length; ii++) {
//         formContext.ui.tabs.get(tabArr[ii]).setVisible(isVisible);
//     }
// }

// function makeFieldsMandatory(formContext, fieldArr, isMandatory) {
//     "use strict";
//     for (var ii = 0; ii < fieldArr.length; ii++) {
//         formContext.getAttribute(fieldArr[ii]).setRequiredLevel(isMandatory);//
//     }
// }

// function hideDriverSection(executionContext) {
//     "use strict";
//     var formContext = executionContext.getFormContext();
//     if (formContext.getAttribute("jarvis_callerrole").getValue() === 2) {
//         formContext.ui.tabs.get("tab_register").sections.get("tab_register_section_5").setVisible(false);
//         //alert(formContext.ui.tabs.get("tab_register").sections.get("tab_register_section_5").getVisible())
//     }
//     else {
//         formContext.ui.tabs.get("tab_register").sections.get("tab_register_section_5").setVisible(true);
//     }
// }

//function showTowingTab(primaryControl) {
//    "use strict";
//    var formContext = primaryControl;
//    var currentDateTime = new Date();
//    formContext.getAttribute("jarvis_canvasdate").setValue(currentDateTime);
//    formContext.data.entity.save();
//    formContext.ui.tabs.get("towingRental").setVisible(true);
//    formContext.ui.tabs.get("towingRental").setFocus();


//}

//function showAssignTab(primaryControl) {
//    "use strict";
//    var formContext = primaryControl;
//    formContext.ui.tabs.get("assignRD").setVisible(true);
//    formContext.ui.tabs.get("assignRD").setFocus();
//}

//function populateSubject(executionContext) {
//    "use strict";
//    var formContext = executionContext.getFormContext();
//    var resolutionType = formContext.getAttribute("resolutiontypecode").getValue();
//    if (resolutionType !== 1000) {
//        formContext.getAttribute("subject").setValue("Standard Resolved");
//    }

//}

// async function checkOpenCasesForVehicle(executionContext, tabName) {
//     "use strict";
//     try {
//         var formContext = executionContext.getFormContext();
//         formContext.ui.clearFormNotification("VEHICLE");
//         var formContext = executionContext.getFormContext();
//         var vehicle = formContext.getAttribute("jarvis_vehicle").getValue();
//         //var tabSelc = formContext.ui.tabs.get();
//         //var tName = tabSelc.getName();
//         //if (tabName !== null)
//         //  alert(tabName);
//         if (vehicle !== null && vehicle[0] !== null) {
//             var vehicleID = vehicle[0].id.toString().replace('{', '').replace('}', '');
//             var activeStageName = getStageData(formContext);
//             var uniqueId = "VEHICLE";
//             if ((activeStageName === null || activeStageName === undefined || (activeStageName.toLowerCase() === 'case opening' && (formContext.ui.getFormType() === 1 || tabName === null))) || (tabName !== null && (tabName === "Register" || tabName === "Validate"))) {
//                 var incidents = await retrieveRecordsofIncident(vehicleID);
//                 //console.log(incidents);
//                 if (incidents !== null && incidents.entities !== null && incidents.entities.length > 0) {
//                     var msg = "This vehicle already has a case open. Please check the list of active cases for this vehicle before proceeding further.";
//                     var level = "WARNING";
//                     var tabObj = formContext.ui.tabs.get("tab_register");
//                     if (formContext.ui.getFormType() === 1) {
//                         formContext.ui.setFormNotification(msg, level, uniqueId);
//                     }
//                     else {
//                         if (tabObj.getDisplayState() === "expanded")
//                             formContext.ui.setFormNotification(msg, level, uniqueId);
//                         else {
//                             var tabObj = formContext.ui.tabs.get("tab_validate");
//                             if (tabObj.getDisplayState() === "expanded")
//                                 formContext.ui.setFormNotification(msg, level, uniqueId);
//                         }
//                     }
//                 }
//             }
//             else if (tabName !== null && tabName !== "Register" && tabName !== "Validate") {
//                 formContext.ui.clearFormNotification();
//             }
//         }
//         else {
//             formContext.ui.clearFormNotification();
//         }
//     }
//     catch (error) {
//         Xrm.Navigation.openAlertDialog(error.message);
//     }

// }

////Deprecated as this js moved to VasBreakdown.
//async function retrieveRecordsofIncident(vehicleID) {
//    "use strict";
//    var incidents = await Xrm.WebApi.retrieveMultipleRecords("incident", "?$filter=(statecode eq 0 and _jarvis_vehicle_value eq " + vehicleID + ")");
//    return incidents;
//}

function onSave(executionContext) {
    "use strict";
    var formContext = executionContext.getFormContext();
    formContext.ui.clearFormNotification();
}

// function lockCaseType(executionContext) {
//     "use strict";
//     var formContext = executionContext.getFormContext();
//     //console.log("Case On Load");
//     var formType = formContext.ui.getFormType();
//     //alert(formType);
//     if (formType !== 4) {//Read-Only Or Disabled
//         var activeStageName = getStageData(formContext);
//         //alert(activeStageName);
//         if (activeStageName !== null && activeStageName !== undefined) {
//             if (activeStageName.toLowerCase() === 'case opening') {
//                 //console.log(activeStageName.toLowerCase());
//                 var caseType = formContext.getAttribute("casetypecode").getValue();
//                 formContext.getControl("casetypecode").setDisabled(false);
//                 if (caseType === 3)//Query
//                 {
//                     var showTabsArr = ["tab_22", "tab_Query", "tab_register", "tab_validate", "tab_collect", "tab_share", "tab_9", "tab_passout", "tab_scheduler", "tab_waitingforrepair", "tab_repairstart",
//                         "tab_repairongoing", "tab_repairfinished", "tab_closure", "tab_15", "tab_timeline", "tab_summary", "towingRental", "tab_GOP"];
//                     showHideTabs(formContext, showTabsArr, true);
//                     var hideTabsArr = ["Contacts", "tab_20", "tab_register", "tab_validate", "tab_collect", "tab_share", "tab_9", "tab_passout", "tab_scheduler", "tab_waitingforrepair", "tab_repairstart",
//                         "tab_repairongoing", "tab_repairfinished", "tab_closure", "tab_15", "tab_timeline", "tab_summary", "towingRental", "tab_GOP"];
//                     showHideTabs(formContext, hideTabsArr, false);
//                 }
//                 else {
//                     var showTabsArr = ["tab_22", "tab_Query", "tab_register", "tab_validate", "tab_collect", "tab_share", "tab_9", "tab_passout", "tab_scheduler", "tab_waitingforrepair", "tab_repairstart",
//                         "tab_repairongoing", "tab_repairfinished", "tab_closure", "tab_15", "tab_timeline", "tab_summary", "towingRental", "tab_GOP"];
//                     showHideTabs(formContext, showTabsArr, true);
//                     var hideTabsArr = ["tab_22", "tab_Query", "tab_9", "tab_passout", "tab_scheduler", "tab_waitingforrepair", "tab_repairstart",
//                         "tab_repairongoing", "tab_repairfinished", "tab_closure", "tab_15", "tab_timeline", "tab_summary", "towingRental", "tab_GOP"];
//                     showHideTabs(formContext, hideTabsArr, false);
//                 }//Breakdown


//             }
//             else {
//                 formContext.getControl("casetypecode").setDisabled(true);
//                 // if (caseType == 2)//Query
//                 // {
//                 //     var showTabsArr = [ "tab_25"];
//                 // showHideTabs(formContext, showTabsArr, true);
//                 // }
//             }
//         }
//     }
// }

/*function retrieveParentCaseDetails(executionContext) {
    "use strict";
    var formContext = executionContext.getFormContext();
    var parentCase = formContext.getAttribute("parentcaseid").getValue();
    if (parentCase !== null && parentCase[0] !== null) {
        var parentCaseID = parentCase[0].id.toString().replace('{', '').replace('}', '');
        Xrm.WebApi.retrieveRecord("incident", "" + parentCaseID + "", "?$select=_customerid_value,_jarvis_homedealer_value,_jarvis_caseserviceline_value,_jarvis_vehicle_value").then(
            function success(result) {
                //console.log(result);
                // Columns

                var jarvis_homedealer = result["_jarvis_homedealer_value"]; // Lookup
                if (jarvis_homedealer !== null) {
                    var hdLookup = [];   // Creating a new lookup Array
                    hdLookup[0] = {};    // new Object
                    hdLookup[0].id = jarvis_homedealer;  // GUID of the lookup id
                    hdLookup[0].name = result["_jarvis_homedealer_value@OData.Community.Display.V1.FormattedValue"]; // Name of the lookup
                    hdLookup[0].entityType = result["_jarvis_homedealer_value@Microsoft.Dynamics.CRM.lookuplogicalname"]; // Entity Type of the lookup entity
                    formContext.getAttribute("jarvis_homedealer").setValue(hdLookup);
                }
                var jarvis_caseserviceline = result["_jarvis_caseserviceline_value"]; // Lookup
                if (jarvis_caseserviceline !== null) {
                    var csLookup = [];   // Creating a new lookup Array
                    csLookup[0] = {};    // new Object
                    csLookup[0].id = jarvis_caseserviceline;
                    csLookup[0].name = result["_jarvis_caseserviceline_value@OData.Community.Display.V1.FormattedValue"]; // Lookup // Name of the lookup
                    csLookup[0].entityType = result["_jarvis_caseserviceline_value@Microsoft.Dynamics.CRM.lookuplogicalname"];// Entity Type of the lookup entity
                    formContext.getAttribute("jarvis_caseserviceline").setValue(csLookup);
                }
                var jarvis_vehicle = result["_jarvis_vehicle_value"]; // Lookup
                if (jarvis_vehicle !== null) {
                    var vhLookup = [];
                    vhLookup[0] = {};
                    vhLookup[0].id = jarvis_vehicle;
                    vhLookup[0].name = result["_jarvis_vehicle_value@OData.Community.Display.V1.FormattedValue"]; // Name of the lookup
                    vhLookup[0].entityType = result["_jarvis_vehicle_value@Microsoft.Dynamics.CRM.lookuplogicalname"]; // Entity Type of the lookup entity
                    formContext.getAttribute("jarvis_vehicle").setValue(vhLookup);
                }
                var customerid = result["_customerid_value"];
                if (customerid !== null) {
                    var cusLookup = [];
                    cusLookup[0] = {};
                    cusLookup[0].id = result["_customerid_value"]; // Customer
                    cusLookup[0].name = result["_customerid_value@OData.Community.Display.V1.FormattedValue"];
                    cusLookup[0].entityType = result["_customerid_value@Microsoft.Dynamics.CRM.lookuplogicalname"];
                    formContext.getAttribute("customerid").setValue(cusLookup);
                }
            },
            function (error) {
                //console.log(error.message);
            }
        );
    }
    else {
        formContext.getAttribute("jarvis_homedealer").setValue(null);
        formContext.getAttribute("jarvis_caseserviceline").setValue(null);
        formContext.getAttribute("jarvis_vehicle").setValue(null);
        formContext.getAttribute("customerid").setValue(null);
    }

}*/

//Refresh
//function CaseQuickCreateFormLoad() //add this function onload form
//{
//    "use strict";
//    var subGrid = window.parent.document.getElementById("Subgrid_new_34")
//    if (subGrid !== null) {
//        if (subGrid.control)
//            subGrid.control.add_onRefresh(fnOnRefresh)
//        else
//            setTimeout(CaseQuickCreateFormLoad, 500);
//    } else {
//        setTimeout(CaseQuickCreateFormLoad, 500);
//    }
//}


//function enableForceClose(executionContext) {
//    "use strict";
//    var flag = true;
//    var formContext = executionContext;
//    var caseStatus = formContext.getAttribute("statuscode").getValue();
//    if (caseStatus !== 90 || caseStatus !== 1000) {

//        flag = false;

//    }
//    return flag;
//}

//function enableReactivateButton(executionContext) {
//    "use strict";
//    var flag = false;
//    var formContext = executionContext;
//    var caseStatus = formContext.getAttribute("statuscode").getValue();
//    if (caseStatus !== 1000) {

//        flag = true;

//    }
//    return flag;
//}

// function clearVehicleNotification(executionContext) {
//     "use strict";
//     var formContext = executionContext.getFormContext();
//     var activeStageName = getStageData(formContext);
//     if (activeStageName !== null && activeStageName !== undefined) {
//         if (activeStageName.toLowerCase() === 'case opening') {
//             formContext.ui.clearFormNotification("VEHICLE");
//         }

//     }

// }

// SetAdditionalLocation =function(executionContext)
// {
// 	 "use strict";
//     let formContext = executionContext.getFormContext();
//     var formType = formContext.ui.getFormType();
//     if (formType !== 4)
//     {
//     var activeStageName = getStageData(formContext);
//         //alert(activeStageName);
//          if (activeStageName !== null && activeStageName !== undefined)
//             {
//                 if (activeStageName.toLowerCase() === 'case opening')
//                 {
//                 let RepairingDealer = formContext.getAttribute("jarvis_dealerappointment");
//                 if (RepairingDealer !== null)
//                     {
//                         let RDvalue = RepairingDealer.getValue();
//                         if (RDvalue)
//                         {
//                             let record_id = RepairingDealer.getValue()[0].id;
//                             record_id = record_id.replace('{', '').replace('}', '');
//                             Xrm.WebApi.retrieveRecord("account",record_id, "?$select=accountnumber,name").then(
//                                 function success(result) {

//                                 if (formContext.getAttribute("jarvis_location") !== null && formContext.getAttribute("jarvis_etatimeappointment")!==null && formContext.getAttribute("jarvis_etadateappointment"))
//                                     {
//                                         if(formContext.getAttribute("jarvis_etatimeappointment").getValue() !== null &&  formContext.getAttribute("jarvis_etadateappointment").getValue() !== null)
//                                         {
//                                             formContext.getAttribute("jarvis_location").setValue(
//                                             "Dealer ID: "+result.accountnumber+
//                                             "\nDealer Name: "+result.name+
//                                             "\nETA: "+formContext.getAttribute("jarvis_etatimeappointment").getValue()+" "+formContext.getAttribute("jarvis_etadateappointment").getValue());
//                                         }
//                                     }
//                                 }
//                             );
//                         }
//                     }
//                  }
//         }

//     }

// }