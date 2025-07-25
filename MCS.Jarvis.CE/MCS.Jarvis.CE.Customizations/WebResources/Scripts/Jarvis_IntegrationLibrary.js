var Jarvis = Jarvis || {};

Jarvis.Integration = {
    TriggeredByOnSave: function (executionContext) {
        "use strict";
        var userId = Xrm.Utility.getGlobalContext().userSettings.userId;
        const formContext = executionContext.getFormContext();

        var userEntityReference = [
            {
                id: userId,
                entityType: "systemuser"
            }
        ];
        if (userEntityReference !== null && userEntityReference !== undefined) {
            var triggeredByField = formContext.getAttribute("jarvis_triggeredby");
            if (triggeredByField !== null && triggeredByField !== undefined) {
                triggeredByField.setValue(userEntityReference);
            }
        }
    }
}