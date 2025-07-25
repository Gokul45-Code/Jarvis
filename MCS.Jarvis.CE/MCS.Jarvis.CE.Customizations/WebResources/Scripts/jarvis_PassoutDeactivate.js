var Jarvis = Jarvis || {};

Jarvis.PassOutConst = {
	message: "Do you want to cancel selected Pass Out? Please consider possible costs involved with the RD before cancelling the Pass Out(s).\nThis action will change the status of selected Pass Out to Cancelled.",
	entityControl: "entity_control",
	inactive: 1,
	cancelled: 2,
	claerAmount: 0,
	title: "Confirmation Pass Out cancellation"
};

// Framing Notification Message to Dispaly on Click of Cancel.
var confirmStrings = { text: Jarvis.PassOutConst.message, title: Jarvis.PassOutConst.title, confirmButtonLabel: "Yes", cancelButtonLabel: "No" };
var confirmOptions = { height: 150, width: 500 };

var data = {};
data.statecode = Jarvis.PassOutConst.inactive; // State
data.statuscode = Jarvis.PassOutConst.cancelled; //Status
data.jarvis_goplimitout = Jarvis.PassOutConst.claerAmount; //Amount

Jarvis.PassOutRibbon = {

	//Cancel Pasout Button Logic for the FormControl
	cancelPassOutOnForm: function (entityName, entityId, primaryControl) {
		Xrm.Navigation.openConfirmDialog(confirmStrings, confirmOptions).then(
			function (success) {
				if (success.confirmed) {
					if (entityId.length > 0) {
						for (var i = 0; i < entityId.length; i++) {
							Xrm.WebApi.updateRecord(entityName, entityId[i], data).then
								(
									function success(result) {
										primaryControl.data.refresh(false);
									},
									function (error) {
									});

						}

					}
				}
			});
	},

	//Cancel Passout Button Logic for the SubgridControl
	cancelPassOutOnSubGrid: function (entityName, entityId, selectedControl, primaryControl) {

		Xrm.Navigation.openConfirmDialog(confirmStrings, confirmOptions).then(
			function (success) {
				if (success.confirmed) {
					if (entityId.length > 0) {
						for (var i = 0; i < entityId.length; i++) {
							Xrm.WebApi.updateRecord(entityName, entityId[i], data).then
								(
									function success(result) {
										if (selectedControl._controlName != Jarvis.PassOutConst.entityControl) {
											primaryControl.getControl(selectedControl._controlName).refresh();
										}
										else {
											primaryControl.data.refresh(false);
										}

									},
									function (error) {
									});

						}

					}
				}
			});
	}

}
