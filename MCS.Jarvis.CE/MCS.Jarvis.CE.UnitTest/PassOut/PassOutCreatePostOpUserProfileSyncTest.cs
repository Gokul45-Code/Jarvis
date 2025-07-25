//-----------------------------------------------------------------------
// <copyright file="PassOutCreatePostOpUserProfileSyncTest.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace MCS.Jarvis.CE.UnitTest
{
    using System;
    using System.Collections.Generic;
    using MCS.Jarvis.CE.Plugins;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.Xrm.Sdk;

    /// <summary>
    /// PassOutCreatePostOpUserProfileSync Test.
    /// </summary>
    [TestClass]
    public class PassOutCreatePostOpUserProfileSyncTest : UnitTestBase
    {
        /// <summary>
        /// Positive scenario with user for PassOutCreatePostOpUserProfileSync Create Scenario
        /// Contact Should not be bull.
        /// </summary>
        [TestMethod]
        public void PassOutCreatePostOpUserProfileSyncTestCreate()
        {
            var passOutObject = new Entity("jarvis_passout");
            passOutObject[MCS.Jarvis.CE.Plugins.PassOut.Attributes.Case] = new EntityReference("incident", Guid.NewGuid());
            passOutObject[MCS.Jarvis.CE.Plugins.PassOut.Attributes.BusinessPartner] = new EntityReference("businesspartner", Guid.NewGuid());

            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
            {
                new KeyValuePair<string, object>("Target", passOutObject),
            };
            this.PluginExecutionContext.MessageNameGet = () => "Create";
            this.PluginExecutionContext.StageGet = () => 40;

            // Setting service retrieve.
            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "incident")
                {
                    var caseObject = new Entity("incident");
                    caseObject.Id = guid;
                    caseObject[Case.Attributes.Location] = new OptionSetValue(334030001);
                    caseObject[Case.Attributes.ServiceLine] = new EntityReference("caseserviceline", Guid.NewGuid());
                    caseObject[Case.Attributes.CaseType] = new OptionSetValue(2);
                    return caseObject;
                }

                if (entityName == "account")
                {
                    var businessPartner = new Entity("account");
                    businessPartner.Id = guid;
                    businessPartner[BusinessPartner.Atrributes.Country] = new EntityReference("country", Guid.NewGuid());
                    businessPartner[BusinessPartner.Atrributes.BusinessPartnerCountry] = new EntityReference("country", Guid.NewGuid());
                    return businessPartner;
                }

                return null;
            };

            Plugins.PassOutCreatePostOpUserProfileSync plugin = new Plugins.PassOutCreatePostOpUserProfileSync();

            plugin.Execute(this.ServiceProvider);
        }
    }
}