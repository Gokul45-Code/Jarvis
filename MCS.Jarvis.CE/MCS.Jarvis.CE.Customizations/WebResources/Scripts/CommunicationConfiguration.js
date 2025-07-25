function checkChannel(executionContext) {
    "use strict";
    var formContext = executionContext.getFormContext();
    var channel = formContext.getAttribute("jarvis_channel").getValue();
    var communicTemplate = formContext.getAttribute("jarvis_communicationtemplate").getValue();
    if (communicTemplate !== null && channel !== null) {
        var communicTemplateID = communicTemplate[0].id.toString().replace('{', '').replace('}', '');
        Xrm.WebApi.retrieveRecord("jarvis_communicationtemplate", "" + communicTemplateID + "", "?$select=jarvis_emailtemplateforeignkey").then(
            function success(result) {
                //console.log(result);
                // Columns
                var jarvis_emailtemplateforeignkey = result["jarvis_emailtemplateforeignkey"]; // Text
                Xrm.WebApi.retrieveMultipleRecords("template", "?$select=jarvis_channel,jarvis_foreignkey,title&$filter=jarvis_foreignkey eq '" + jarvis_emailtemplateforeignkey + "'").then(
                    function success(results) {
                        //console.log(results);
                        for (var i = 0; i < results.entities.length; i++) {
                            var result = results.entities[i];
                            // Columns

                            var jarvis_channel = result["jarvis_channel"]; // Choice
                            var jarvis_channel_formatted = result["jarvis_channel@OData.Community.Display.V1.FormattedValue"];
                            var jarvis_foreignkey = result["jarvis_foreignkey"]; // Text
                            var title = result["title"]; // Text
                            if (jarvis_channel !== channel) {
                                Xrm.Navigation.openErrorDialog({ message: "Please select a template made for this communication channel" }).then(
                                    function (success) {
                                        //console.log(success);        
                                    },
                                    function (error) {
                                        //console.log(error);
                                    });
                            }
                            else {
                                //formContext.getAttribute("jarvis_name").setValue(title);
                                formContext.getAttribute("jarvis_emailtemplateforeignkey").setValue(jarvis_foreignkey);
                            }
                        }
                    },
                    function (error) {
                        //console.log(error.message);
                    }
                )
            },
            function (error) {
                //console.log(error.message);
            }
        );
    }
    else {
        //formContext.getAttribute("jarvis_name").setValue(null);
        formContext.getAttribute("jarvis_emailtemplateforeignkey").setValue(null);
    }

}
