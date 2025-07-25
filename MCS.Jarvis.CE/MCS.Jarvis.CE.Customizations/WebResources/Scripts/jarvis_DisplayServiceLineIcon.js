function displayIconTooltip(rowData, userLCID) {
    var str = JSON.parse(rowData);
    var coldata = str?.jarvis_caseserviceline_Value?.Name;
    var imgName = "";
    var tooltip = str?.title;
    switch (coldata?.toUpperCase()) {
        case "VOLVO ACTION SERVICE":
            imgName = "jarvis_VolvoActionService";
            break;

        case "RENAULT 24/7":
            imgName = "jarvis_renault247";
            break;

        case "MACK":
            imgName = "jarvis_Mack";
            break;

        case "VOLVO PENTA":
            imgName = "jarvis_Penta";
            break;

        default:
            imgName = "";
            tooltip = "";
            break;
    }
    var resultarray = [imgName, tooltip];
    return resultarray;
}