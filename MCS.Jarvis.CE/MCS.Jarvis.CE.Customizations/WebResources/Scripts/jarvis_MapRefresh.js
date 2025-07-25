var Jarvis = Jarvis || {};

Jarvis.MapRefresh = {

    refreshMap: function (executionContext) {
		"use strict";
        let formContext = executionContext.getFormContext();
        let mapNameArr = ["WebResource_MapControlBing", "WebResource_MapControlBingSummary", "WebResource_MapControlBingValidate"];
        Jarvis.MapRefresh.executeRefreshMap(formContext, mapNameArr);
    },

    executeRefreshMap: function (formContext, mapNameArr) {
		"use strict";
        for (let row of mapNameArr) {
            let webResourceControl = formContext.getControl(row);
            if (webResourceControl && webResourceControl.getObject()) {
                let src = webResourceControl.getObject().src;
                webResourceControl.getObject().src = "about:blank";
                webResourceControl.getObject().src = src;
            }
        }
    }

}