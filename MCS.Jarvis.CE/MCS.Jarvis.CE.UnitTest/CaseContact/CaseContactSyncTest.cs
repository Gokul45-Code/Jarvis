//-----------------------------------------------------------------------
// <copyright file="CaseContactSyncTest.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace MCS.Jarvis.CE.UnitTest
{
    using System;
    using System.Collections.Generic;
    using MCS.Jarvis.CE.Plugins.CaseContact;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.Xrm.Sdk;
    using static MCS.Jarvis.CE.BusinessProcessesShared.Helpers.Constants;

    /// <summary>
    /// CaseContactSync Test.
    /// </summary>
    [TestClass]
    public class CaseContactSyncTest : UnitTestBase
    {
        /// <summary>
        /// CaseContactSync UpdateMessage ShouldUpdateIncidentCaller.
        /// </summary>
        [TestMethod]
        public void CaseContactSync_UpdateMessage_ShouldUpdateIncident_Caller()
        {
            var caseContact = new Entity("jarvis_contact");
            caseContact.Attributes[Casecontact.jarvisFirstname] = "John";
            caseContact.Attributes[Casecontact.jarvisLastname] = "Doe";
            caseContact.Attributes[Casecontact.jarvisMobilephone] = "XY123";
            caseContact.Attributes[Casecontact.jarvisPreferredlanguage] = "ENG";
            caseContact.Attributes[Casecontact.jarvisRole] = new OptionSetValue((int)Casecontact.callerMethodofContact);

            Entity caseContactPostImg = new Entity("jarvis_contact");
            caseContactPostImg.Attributes["jarvis_incident"] = new EntityReference("incident", Guid.NewGuid());
            caseContactPostImg.Attributes[Casecontact.jarvisIsManualUpdate] = true;
            caseContactPostImg.Attributes[Casecontact.jarvisFirstname] = "John";
            caseContactPostImg.Attributes[Casecontact.jarvisLastname] = "Doe";
            caseContactPostImg.Attributes[Casecontact.jarvisMobilephone] = "XY123";
            caseContactPostImg.Attributes[Casecontact.jarvisPreferredlanguage] = "ENG";
            caseContactPostImg.Attributes["jarvis_casecontacttype"] = new OptionSetValue(334030000);
            caseContactPostImg.Attributes[Casecontact.jarvisRole] = new OptionSetValue((int)Casecontact.callerMethodofContact);

            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
            {
                new KeyValuePair<string, object>("Target", caseContact),
            };
            this.PluginExecutionContext.MessageNameGet = () => "Update";

            this.PluginExecutionContext.PostEntityImagesGet = () => new EntityImageCollection { new KeyValuePair<string, Entity>("PostImage", caseContactPostImg) };

            CaseConatctSync plugin = new CaseConatctSync();

            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Case Contact Sync Update Message Should Update Incident Driver.
        /// </summary>
        [TestMethod]
        public void CaseContactSync_UpdateMessage_ShouldUpdateIncident_Driver()
        {
            var caseContact = new Entity("jarvis_contact");
            caseContact.Attributes[Casecontact.jarvisFirstname] = "John";
            caseContact.Attributes[Casecontact.jarvisLastname] = "Doe";
            caseContact.Attributes[Casecontact.jarvisMobilephone] = "XY123";
            caseContact.Attributes[Casecontact.jarvisPreferredlanguage] = "ENG";
            caseContact.Attributes[Casecontact.jarvisRole] = new OptionSetValue((int)Casecontact.callerMethodofContact);

            var caseContactPostImg = new Entity("jarvis_contact");
            caseContactPostImg.Attributes["jarvis_incident"] = new EntityReference("incident", Guid.NewGuid());
            caseContactPostImg.Attributes[Casecontact.jarvisIsManualUpdate] = true;
            caseContactPostImg.Attributes[Casecontact.jarvisFirstname] = "John";
            caseContactPostImg.Attributes[Casecontact.jarvisLastname] = "Doe";
            caseContactPostImg.Attributes[Casecontact.jarvisMobilephone] = "XY123";
            caseContactPostImg.Attributes[Casecontact.jarvisPreferredlanguage] = "ENG";
            caseContactPostImg.Attributes["jarvis_casecontacttype"] = new OptionSetValue(334030001);
            caseContactPostImg.Attributes[Casecontact.jarvisRole] = new OptionSetValue((int)Casecontact.callerMethodofContact);

            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
            {
                new KeyValuePair<string, object>("Target", caseContact),
            };
            this.PluginExecutionContext.MessageNameGet = () => "Update";

            this.PluginExecutionContext.PostEntityImagesGet = () => new EntityImageCollection { new KeyValuePair<string, Entity>("PostImage", caseContactPostImg) };

            CaseConatctSync plugin = new CaseConatctSync();

            plugin.Execute(this.ServiceProvider);
        }
    }
}