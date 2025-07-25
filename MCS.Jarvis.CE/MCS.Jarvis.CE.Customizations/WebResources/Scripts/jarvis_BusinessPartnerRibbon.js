var Jarvis = Jarvis || {};

var VasAdvisor = "BD2426CB-4E5B-ED11-9562-000D3ABA62E8";

Jarvis.BPRibbon = {
    displayActviate: function () {
        "use strict";
        var userSettings = Xrm.Utility.getGlobalContext().userSettings
        var userRole = userSettings.roles;
        var hasRole = true;
        if (userRole._collection !== null) {
            userRole.forEach(function (item) {
                if (item.id.toUpperCase() === VasAdvisor) {
                    hasRole = false;
                    return hasRole;
                }
            });
        }
        return hasRole;
    }
}