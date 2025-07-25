var Jarvis = Jarvis || {};

Jarvis.GOPConst = {
	message: "Do you want to cancel the GOP?\n This action will change the status of the GOP to Cancelled.",
	entityControl: "entity_control",
	inactive: 1,
	cancelled: 2,
	title: "Confirmation GOP cancellation"
};

// Framing Notification Message to Dispaly on Click of Cancel.
var confirmStrings = { text: Jarvis.GOPConst.message, title: Jarvis.GOPConst.title, confirmButtonLabel: "Yes", cancelButtonLabel: "No" };
var confirmOptions = { height: 150, width: 500 };

var data = {};
data.statecode = Jarvis.GOPConst.inactive; // State
data.statuscode = Jarvis.GOPConst.cancelled; //Status
data.jarvis_goplimitin = 0;

function checkLatestApprovedGOP(caseId, entityId, entityName, primaryControl, selectedControl, sucessCallBack, isform = true) {
	var approvedGOP;
	var fetchXml = `<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
	<entity name='jarvis_gop'>
	  <attribute name='jarvis_gopid' />
	  <attribute name='jarvis_name' />
	  <attribute name='createdon' />
	  <attribute name='jarvis_requesttype' />
	  <attribute name='modifiedon' />
	  <attribute name='jarvis_incident' />
	  <attribute name='jarvis_gopapproval' />
	  <attribute name='jarvis_totallimitincurrency' />
	  <attribute name='jarvis_totallimitin' />
	  <order attribute='jarvis_gopapprovaltime' descending='true' />
	  <order attribute='modifiedon' descending='true' />
	  <filter type='and'>
		<condition attribute='jarvis_incident' operator='eq'  uitype='incident' value='{`+ caseId + `}' />
		<condition attribute='jarvis_gopapproval' operator='eq' value='334030001' />
		<condition attribute='jarvis_relatedgop' operator='null' />
		<condition attribute='statecode' operator='eq' value='0' />
	  </filter>
	</entity>
  </fetch>`;

	fetchXml = "?fetchXml=" + encodeURIComponent(fetchXml);
	Xrm.WebApi.retrieveMultipleRecords("jarvis_gop", fetchXml).then(
		function success(result) {
			if (result.entities.length > 0) {
				approvedGOP = result.entities[0];
				if (entityId.length > 0) {
					for (var i = 0; i < entityId.length; i++) {
						entityId[i] = entityId[i].replace('{', '').replace('}', '');
						if (approvedGOP["jarvis_gopid"].toUpperCase() == entityId[i].toUpperCase()) {
							sucessCallBack(entityId[i], entityName, primaryControl, selectedControl, isform);
							break;
						}
						else
							Xrm.Navigation.openAlertDialog("Only the latest Approved GOP can be cancelled");
					}
				}

			}
		},
		function (error) {
			console.log(error.message);

		}
	);
}

function openConfirmDialogBox(entityId, entityName, primaryControl, selectedControl, isform = true) {
	Xrm.Navigation.openConfirmDialog(confirmStrings, confirmOptions).then(
		function (success) {
			if (success.confirmed) {

				Xrm.WebApi.updateRecord(entityName, entityId, data).then
					(
						function success(result) {
							if (isform)
								primaryControl.data.refresh(false);
							else {
								if (selectedControl._controlName != Jarvis.GOPConst.entityControl) {
									primaryControl.getControl(selectedControl._controlName).refresh();
								}
								else {
									primaryControl.data.refresh(false);
								}
							}
						},
						function (error) {
						});


			}
		});
}

Jarvis.GOPRibbon = {

	//Cancel GOP Button Logic for the FormControl
	cancelGOPonForm: function (entityName, entityId, primaryControl) {
		var caseId = primaryControl.getAttribute("jarvis_incident")?.getValue()[0]["id"];
		caseId = caseId.replace('{', '').replace('}', '');
		checkLatestApprovedGOP(caseId, entityId, entityName, primaryControl, primaryControl, openConfirmDialogBox);

	},

	//Cancel GOP Button Logic for the SubgridControl
	cancelGOPonSubGrid: function (entityName, entityId, selectedControl, primaryControl) {
		var caseId = selectedControl._formContext._data?._formContext?._entityReference?.id?.guid;
		checkLatestApprovedGOP(caseId, entityId, entityName, primaryControl, selectedControl, openConfirmDialogBox, false);

	}



}