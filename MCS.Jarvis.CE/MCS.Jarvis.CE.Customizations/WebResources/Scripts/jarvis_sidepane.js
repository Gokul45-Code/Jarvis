async function onCaseLoad(executionContext) {
    var formContext = executionContext.getFormContext();
    var formType = formContext.ui.getFormType();
    if (formType !== 1) {
        var workspaceAppName = "msdyn_CustomerServiceWorkspace";
        var globalContext = Xrm.Utility.getGlobalContext();
        var app = await globalContext.getCurrentAppProperties();
        if (app !== null) {
            var currentAppName = app.uniqueName;
            if (currentAppName === workspaceAppName) {
                Microsoft.AppRuntime.Sessions.addOnAfterSessionSwitch(OnsessionSwitch);
                var recordId = formContext.data.entity.getId();
                await openTimelinePaneOnLoad(recordId);
                Xrm.App.sidePanes.state = 0;
            }
        }
    }
    disableFormSelector(formContext);
}

function openTimelinePaneOnSessionSwitch(recordId) {
    var timelineformId = "973A5E16-F4F5-4FDF-A5B9-7939A6562166";
    var timelinePane = Xrm.App.sidePanes.getPane("Timeline");
    if (timelinePane != null) {
        timelinePane.title = "Timeline"
        timelinePane.state = 0;
        timelinePane.navigate({
            pageType: "entityrecord",
            entityName: "incident",
            entityId: recordId,
            formId: timelineformId,
        });
    }
    else {
        Xrm.App.sidePanes.createPane({
            title: "Timeline",
            imageSrc: "WebResources/msdyn_notes_icon.svg",
            hideHeader: true,
            canClose: true,
            width: 340,
            paneId: "Timeline",
            state: 0,
        }).then((pane) => {
            Xrm.App.sidePanes.state = 0;
            pane.navigate({
                pageType: "entityrecord",
                entityName: "incident",
                entityId: recordId,
                formId: timelineformId,
            });
        });
    }
}

async function openTimelinePaneOnLoad(recordId) {
    //await getCaseTitle(recordId);
    var timelineformId = "973A5E16-F4F5-4FDF-A5B9-7939A6562166";
    var timelinePane = Xrm.App.sidePanes.getPane("Timeline");
    if (timelinePane != null) {
        timelinePane.title = "Timeline"
        //timelinePane.state = 0;
        timelinePane.navigate({
            pageType: "entityrecord",
            entityName: "incident",
            entityId: recordId,
            formId: timelineformId,
        });
    }
    else {
        Xrm.App.sidePanes.createPane({
            title: "Timeline",
            imageSrc: "WebResources/msdyn_notes_icon.svg",
            hideHeader: true,
            canClose: true,
            width: 340,
            paneId: "Timeline",
            // state: 0,
        }).then((pane) => {
            pane.navigate({
                pageType: "entityrecord",
                entityName: "incident",
                entityId: recordId,
                formId: timelineformId,
            });
        });
    }
    //Xrm.App.sidePanes.state = 0;
}

function HideRibbonAndBPF(executionContext) {
    var formContext = executionContext.getFormContext();
    formContext.ui.headerSection.setCommandBarVisible(false);
    formContext.ui.process.setVisible(false);
    formContext.ui.headerSection.setBodyVisible(false);
    formContext.ui.headerSection.setTabNavigatorVisible(false);
}

async function OnsessionSwitch(executionContext) {

    const currentSession = Microsoft.AppRuntime.Sessions.getFocusedSession();
    var sessionContext = currentSession.getContext();
    var currentTab = Xrm.App.sessions.getFocusedSession().tabs.getFocusedTab();

    var currentURL = currentTab.currentUrl;
    const url = new URL(currentURL);
    var activeEntityId = url.searchParams.get('id');
    var activeEntityName = url.searchParams.get('etn');


    if (activeEntityId !== null && activeEntityName === "incident") {
        await openTimelinePaneOnLoad(activeEntityId);
        Xrm.App.sidePanes.state = 0;
    }
    else {

        var timelinePane = Xrm.App.sidePanes.getPane("Timeline");
        if (timelinePane != null) {
            timelinePane.close();
        }
    }
}

function disableFormSelector(formContext) {
    formContext.ui.formSelector.items.forEach(
        function (f) {
            f.setVisible(false);
        }
    );
}


async function getCaseTitle(recordId) {
    var results = await Xrm.WebApi.retrieveRecord("incident", recordId, "?$select=title")
    return results;
}