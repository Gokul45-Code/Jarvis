var Jarvis = Jarvis || {};
Jarvis.UserRibbon = {

    repairProfile: function (selectedItemId, entityTypeName, selectedControl) {
		"use strict";
        var data = {};
        data.jarvis_repair = true;
        var confirmStrings = { text: "Do you want to update the case user profiles?\n This action will update all the cases associated to the user", title: "Confirmation Case User Profile Repair", confirmButtonLabel: "Yes", cancelButtonLabel: "No" };
        var confirmOptions = { height: 150, width: 500 };

        Xrm.Navigation.openConfirmDialog(confirmStrings, confirmOptions).then(
            function (success) {
                if (success.confirmed) {
                    
                            Xrm.WebApi.updateRecord(entityTypeName, selectedItemId, data).then
                                (
                                    function success(result) {
                                        },
                                    function (error) {
                                    });
    
                        
                }
            });

    }

}