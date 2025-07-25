var Jarvis = Jarvis || {};

Jarvis.PreferredContactMethod = {
    Any: 1,
    SMS: 334030001,
    Email: 2,
    Phone: 3
}
Jarvis.PreferredContactMethod_Contact = {
    Any: 334030000,
    SMS: 334030001,
    Email: 334030002,
    Calling: 334030003
}

Jarvis.CaseContact = {

    existingContactOnChange: function (executionContext) {
        let formContext = executionContext.getFormContext();
        let existingContact = formContext.getAttribute("jarvis_existingcontact")?.getValue();

        if (!existingContact) {
            return;
        }

        let existingContactId = existingContact[0].id.slice(1, -1);
        Xrm.WebApi.retrieveRecord("contact", existingContactId, "?$select=accountrolecode,_parentcustomerid_value,company,emailaddress1,firstname,_jarvis_title_value,lastname,_jarvis_language_value,mobilephone,jarvis_parentaccounttype,preferredcontactmethodcode&$expand=parentcustomerid_account($select=jarvis_accounttype)").then(function success(result) {
            formContext.getAttribute("jarvis_firstname").setValue(result["firstname"]);
            formContext.getAttribute("jarvis_firstname").fireOnChange();
            formContext.getAttribute("jarvis_lastname").setValue(result["lastname"]);
            formContext.getAttribute("jarvis_lastname").fireOnChange();
            formContext.getAttribute("jarvis_jobtitle").setValue(result["_jarvis_title_value@OData.Community.Display.V1.FormattedValue"]);
            formContext.getAttribute("jarvis_businesspartner").setValue(result["_parentcustomerid_value@OData.Community.Display.V1.FormattedValue"]);
            formContext.getAttribute("jarvis_businesspartnertype").setValue(result["jarvis_parentaccounttype"]);
            formContext.getAttribute("jarvis_preferredlanguage").setValue(result["_jarvis_language_value"]);
            formContext.getAttribute("jarvis_phone").setValue(result["company"]);
            formContext.getAttribute("jarvis_mobilephone").setValue(result["mobilephone"]);
            formContext.getAttribute("jarvis_email").setValue(result["emailaddress1"]);

            let preferredContactMethod = result["preferredcontactmethodcode"];
            switch (preferredContactMethod) {
                case 1:
                    formContext.getAttribute("jarvis_preferredmethodofcontact").setValue(Jarvis.PreferredContactMethod_Contact.Any);
                    break;
                case 3:
                    formContext.getAttribute("jarvis_preferredmethodofcontact").setValue(Jarvis.PreferredContactMethod_Contact.Calling);
                    break;
                case 2:
                    formContext.getAttribute("jarvis_preferredmethodofcontact").setValue(Jarvis.PreferredContactMethod_Contact.Email);
                    break;
                case 334030001:
                    formContext.getAttribute("jarvis_preferredmethodofcontact").setValue(Jarvis.PreferredContactMethod_Contact.SMS);
                    break;
                default:
                    formContext.getAttribute("jarvis_preferredmethodofcontact").setValue(preferredContactMethod);
                    break;
            }
            formContext.getAttribute("jarvis_preferredmethodofcontact").fireOnChange();
        }, function (error) {
            console.log(error.message);
        });
    },

    firstnameOnChange: function (executionContext) {
        let formContext = executionContext.getFormContext();
        Jarvis.CaseContact.populateCaseContactName(formContext);
    },

    lastnameOnChange: function (executionContext) {
        let formContext = executionContext.getFormContext();
        Jarvis.CaseContact.populateCaseContactName(formContext);
    },

    populateCaseContactName: function (formContext) {
        let firstName = formContext.getAttribute("jarvis_firstname")?.getValue();
        let lastName = formContext.getAttribute("jarvis_lastname")?.getValue();

        firstName = firstName ? firstName : "";
        lastName = lastName ? lastName : "";

        let name = firstName + " " + lastName;

        formContext.getAttribute("jarvis_name").setValue(name);
    },

    SetManualUpdate: function (executionContext) {
        var formContext = executionContext.getFormContext();
        let manualUpdate = formContext.getAttribute("jarvis_ismanualupdate");
        let caseConatactType = formContext.getAttribute("jarvis_casecontacttype");
        var fieldToDirty = ["jarvis_callerlanguage", "jarvis_casecontacttype", "jarvis_driverlanguage", "jarvis_email", "jarvis_firstname", "jarvis_phone", "jarvis_lastname", "jarvis_mobilephone", "jarvis_name", "jarvis_role"];
        var dirtyCheck = Jarvis.CaseContact.checkIsDirty(formContext, fieldToDirty);
        if (dirtyCheck) {
            if (manualUpdate == null) { return };
            if (manualUpdate.getValue() == null || manualUpdate.getValue() == false && caseConatactType.getValue() != null) {
                formContext.getAttribute("jarvis_ismanualupdate").setValue(true);
            }
        }
        else {
            formContext.getAttribute("jarvis_ismanualupdate")?.setValue(false);
        }
    },

    checkIsDirty: function (formContext, fieldArr) {
        for (var ii = 0; ii < fieldArr.length; ii++) {
            var isDirty = formContext.getAttribute(fieldArr[ii])?.getIsDirty();//
            if (isDirty != null && isDirty != undefined) {
                if (isDirty) {
                    return true;
                }
            }
        }
        return false;
    }
}