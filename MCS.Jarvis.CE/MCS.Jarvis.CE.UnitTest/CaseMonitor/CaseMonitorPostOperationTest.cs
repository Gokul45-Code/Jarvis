//-----------------------------------------------------------------------
// <copyright file="CaseMonitorPostOperationTest.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace MCS.Jarvis.CE.UnitTest
{
    using System;
    using System.Collections.Generic;
    using MCS.Jarvis.IntegrationPlugin;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.Xrm.Sdk;
    using static MCS.Jarvis.CE.BusinessProcessesShared.Helpers.Constants;

    /// <summary>
    /// CaseMonitorPostOperation Test.
    /// </summary>
    [TestClass]
    public class CaseMonitorPostOperationTest : UnitTestBase
    {
        /// <summary>
        /// Positive scenario for CaseMonitorPostOperation Create Scenario
        /// Contact Should not be bull.
        /// </summary>
        [TestMethod]
        public void CaseMonitorPostOperationTestCreate()
        {
            var incident = new Entity("incident");
            incident.Id = Guid.NewGuid();
            incident[Incident.caseOriginCode] = new OptionSetValue(2);
            incident[Incident.casetypecode] = new OptionSetValue(2);
            incident["jarvis_fucomment"] = "Test";
            incident["jarvis_futimestamp"] = DateTime.Now;
            incident["prioritycode"] = new OptionSetValue(0);

            var postImg = new Entity("incident");
            postImg.Attributes[CaseMonitor.regardingobjectid] = new EntityReference("incident", incident.Id);
            postImg[Incident.caseOriginCode] = new OptionSetValue(2);
            postImg[Incident.casetypecode] = new OptionSetValue(2);
            postImg["jarvis_fucomment"] = "Test";
            postImg["jarvis_futimestamp"] = DateTime.Now;
            postImg["prioritycode"] = new OptionSetValue(0);
            postImg[CaseMonitor.jarvis_fulinknew] = new EntityReference("jarvis_casemonitor", Guid.NewGuid());

            Entity comparePostImg = new Entity("incident");
            comparePostImg.Id = Guid.NewGuid();
            comparePostImg["jarvis_fucomment"] = "Test";
            comparePostImg["jarvis_futimestamp"] = DateTime.Now;
            comparePostImg["prioritycode"] = new OptionSetValue(0);

            Entity comparePreImg = new Entity("incident");
            comparePreImg.Id = Guid.NewGuid();
            comparePreImg["jarvis_fucomment"] = "Test";
            comparePreImg["jarvis_futimestamp"] = DateTime.Now;
            comparePreImg["prioritycode"] = new OptionSetValue(1);
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", incident),
               };
            this.PluginExecutionContext.MessageNameGet = () => "Update";
            this.PluginExecutionContext.StageGet = () => 40;

            //// Setting Post Entity Image.
            this.PluginExecutionContext.PostEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PostImage", postImg },
                        { "ComparePostImage", comparePostImg },
                };
            };

            this.PluginExecutionContext.PreEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                       { "ComparePreImage", comparePreImg },
                };
            };

            // Setting service retrieve.
            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "jarvis_casemonitor")
                {
                    Entity caseMonitor = new Entity("jarvis_casemonitor");
                    caseMonitor.Id = Guid.NewGuid();
                    caseMonitor[CaseMonitor.jarvis_source] = new OptionSetValue((int)Source.Jarvis);
                    return caseMonitor;
                }

                return null;
            };

            string secureString = $"{{\r\n  \"functionAppUrl\": \"https://test.azure-api.net/\",\r\n  \"client_id\": \"{Guid.Empty}\",\r\n  \"client_secret\": \"{Guid.Empty}\",\r\n  \"scope\":\"api://{Guid.Empty}/.default\",\r\n  \"tenantId\": \"{Guid.Empty}\",\r\n  \"tokenUrl\":\"https://login.microsoftonline.com/{{0}}/oauth2/token\",\r\n\"api-key\":\"{Guid.Empty}\"\r\n}}";
            CaseMonitorPostOperation plugin = new CaseMonitorPostOperation("test", secureString);
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Negative scenario where secure and unsecure strings are not provided.
        /// </summary>
        [TestMethod]

        // [ExpectedException(typeof(InvalidPluginExecutionException))]
        public void CaseMonitorPostOperationTestNoSecureUnsecureStrings()
        {
            // Creating the plugin instance without providing secure and unsecure strings.
            Action createPlugin = () => new CaseMonitorPostOperation(null, null);

            Assert.ThrowsException<InvalidPluginExecutionException>(createPlugin);
        }
    }
}
