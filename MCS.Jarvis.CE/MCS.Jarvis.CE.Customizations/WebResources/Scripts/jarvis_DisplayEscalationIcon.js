var Jarvis = Jarvis || {};

Jarvis.escalation =
{

    displayIcon: function (rowData, userLCID) {
		"use strict";
        var str = JSON.parse(rowData);
        var coldata = str.isescalated;
        var imgName = "";
        var tooltip = "Escalated";
        switch (coldata.toUpperCase()) {
            case "YES":
                imgName = "jarvis_escalationIcon";
                break;

            case "NO":
                imgName = "";
                tooltip = "";
                break;

            default:
                imgName = "";
                tooltip = "";
                break;
        }
        var resultarray = [imgName, tooltip];
        return resultarray;

    }
}