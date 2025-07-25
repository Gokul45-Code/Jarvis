var Jarvis = Jarvis || {};

Jarvis.bookableResourceCustomization = {

    validate: function (executionContext) {
        "use strict";
        let GopCntrls = ["jarvis_from", "jarvis_to", "jarvis_sortorder"]
        const regex = new RegExp(/^([01]\d|2[0-3]):?([0-5]\d)$/);
        const timeRegex = new RegExp(/^(0\d|1\d|2[0-3])[0-5]\d$/);
        GopCntrls.forEach(SourceField => {
            let fieldVal = executionContext.getFormContext().getAttribute(SourceField).getValue();
            if (SourceField !== "jarvis_sortorder") {
                if (fieldVal) {
                    if (regex.test(fieldVal) === false && timeRegex.test(fieldVal) === false) {
                        var alertStrings = { confirmButtonLabel: "OK", text: "Please enter valid time format hh:mm/hhmm", title: "Alert" };
                        var alertOptions = { height: 120, width: 260 };
                        Xrm.Navigation.openAlertDialog(alertStrings, alertOptions).then(
                            function (success) {

                            },
                            function (error) {

                            }
                        );
                        executionContext.getFormContext().getAttribute(SourceField).setValue(null);
                    }
                    Jarvis.bookableResourceCustomization.validateTime(executionContext);
                }
            }
            else {
                let weekday = executionContext.getFormContext().getAttribute("jarvis_weekday").getValue();
                executionContext.getFormContext().getAttribute("jarvis_sortorder").setValue(weekday);
            }
        });
    },

    validateTime: function (executionContext) {
        "use strict";
        let formContext = executionContext.getFormContext();
        let from = formContext.getAttribute("jarvis_from").getValue();
        let to = formContext.getAttribute("jarvis_to").getValue();
        if (new Date('1/1/1999 ' + from) > new Date('1/1/1999 ' + to)) {
            var alertStrings = { confirmButtonLabel: "OK", text: "From Duration cannot be greater than To Duration", title: "Alert" };
            var alertOptions = { height: 120, width: 260 };
            Xrm.Navigation.openAlertDialog(alertStrings, alertOptions).then(
                function (success) {

                },
                function (error) {

                }
            );
            formContext.getAttribute("jarvis_to").setValue(null);
        }
    }

}