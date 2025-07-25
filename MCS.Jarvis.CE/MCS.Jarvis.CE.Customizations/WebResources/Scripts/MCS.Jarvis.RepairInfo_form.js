var Jarvis = Jarvis || {};


Jarvis.RepairInfoform = {

    populatePassoutRD: function (executionContext) {
        "use strict";
        var formContext = executionContext.getFormContext();
        if (formContext.ui.getFormType() === 1) {
            var fetchXml = "";
            var caseId = formContext.getAttribute("jarvis_incident").getValue()[0].id;
            caseId = caseId.replace('{', '').replace('}', '');

            var fetchXml = `<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
        <entity name='jarvis_passout'>
          <attribute name='jarvis_passoutid' />
          <attribute name='jarvis_name' />
          <attribute name='modifiedon' />
          <order attribute='modifiedon' descending='true' />
          <filter type='and'>
          <filter type='and'>
            <condition attribute='jarvis_incident' operator='eq' uitype='incident' value='{`+ caseId + `}' />
            <condition attribute='statecode' operator='eq' value='0' />
          </filter>
            <filter type='or'>
			<condition attribute='statuscode' operator='eq' value='334030002' />
			<condition attribute='statuscode' operator='eq' value='334030001' />
          </filter>
         </filter>
        </entity>
      </fetch>`;

            fetchXml = "?fetchXml=" + encodeURIComponent(fetchXml);
            Xrm.WebApi.retrieveMultipleRecords("jarvis_passout", fetchXml).then(
                function success(result) {
                    if (result.entities.length === 1) {
                        var passout = result.entities[0];
                        if (passout["jarvis_passoutid"]) {
                            var lookup = new Array();
                            lookup[0] = new Object();
                            lookup[0].id = passout["jarvis_passoutid"];
                            lookup[0].name = passout["jarvis_name"];
                            lookup[0].entityType = "jarvis_passout";
                            executionContext.getFormContext().getAttribute("jarvis_repairingdealerpassout").setValue(lookup);

                        }

                    }
                },
                function (error) {

                }
            );
        }

    },
    onRepairInfoLoad: function (executionContext) {
        "use strict";
        Jarvis.RepairInfoform.populatePassoutRD(executionContext);
    }
}