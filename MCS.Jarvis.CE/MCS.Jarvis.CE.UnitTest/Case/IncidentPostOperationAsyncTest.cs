//-----------------------------------------------------------------------
// <copyright file="IncidentPostOperationAsyncTest.cs" company="Microsoft">
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
    using static MCS.Jarvis.CE.BusinessProcessesShared.Helpers.Constants;

    /// <summary>
    /// IncidentPostOperationAsync Test.
    /// </summary>
    [TestClass]
    public class IncidentPostOperationAsyncTest : UnitTestBase
    {
        /// <summary>
        /// Positive Scenario with Create.
        /// </summary>
        [TestMethod]
        public void IncidentPostOperationAsyncTestCreate()
        {
            //// Setting Input Parameters.
            Entity incident = new Entity("incident");
            incident.Id = Guid.NewGuid();
            incident[Incident.caseOriginCode] = new OptionSetValue(2);
            incident[Incident.casetypecode] = new OptionSetValue(2);

            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", incident),
               };
            this.PluginExecutionContext.MessageNameGet = () => "Create";
            this.PluginExecutionContext.StageGet = () => 40;

            //// Setting Post Entity Image.
            this.PluginExecutionContext.PostEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PostImage", incident },
                };
            };
            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "systemuser")
                {
                    Entity user = new Entity("systemuser");
                    user.Id = Guid.NewGuid();
                    user["fullname"] = "Mercurius";
                }

                return null;
            };
            string secureString = $"{{\r\n  \"functionAppUrl\": \"https://test.azure-api.net/\",\r\n  \"client_id\": \"{Guid.Empty}\",\r\n  \"client_secret\": \"{Guid.Empty}\",\r\n  \"scope\":\"api://{Guid.Empty}/.default\",\r\n  \"tenantId\": \"{Guid.Empty}\",\r\n  \"tokenUrl\":\"https://login.microsoftonline.com/{{0}}/oauth2/token\",\r\n\"api-key\":\"{Guid.Empty}\"\r\n}}";
            IncidentPostOperationAsync plugin = new IncidentPostOperationAsync("test", secureString);
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Positive Scenario for Update.
        /// </summary>
        [TestMethod]
        public void IncidentPostOperationAsyncTestUpdate()
        {
            //// Setting Input Parameters.
            Entity incident = new Entity("incident");
            incident.Id = Guid.NewGuid();
            incident[Incident.caseOriginCode] = new OptionSetValue(2);
            incident[Incident.casetypecode] = new OptionSetValue(2);

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
                        { "PostImage", incident },
                };
            };
            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "systemuser")
                {
                    Entity user = new Entity("systemuser");
                    user.Id = Guid.NewGuid();
                    user["fullname"] = "Mercurius";
                }

                return null;
            };
            IncidentPostOperationAsync plugin = new IncidentPostOperationAsync("test", "test");
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Negative scenario where secure and unsecure strings are not provided.
        /// </summary>
        [TestMethod]
        public void IncidentPostOperationAsyncTestNoSecureUnsecureStrings()
        {
            // Creating the plugin instance without providing secure and unsecure strings.
            Action createPlugin = () => new IncidentPostOperationAsync(null, null);

            Assert.ThrowsException<InvalidPluginExecutionException>(createPlugin);

            // The expected exception (InvalidPluginExecutionException) will be thrown, indicating the absence of secure and unsecure strings.
        }
    }
}
