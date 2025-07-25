//-----------------------------------------------------------------------
// <copyright file="CaseResolutionAsyncTest.cs" company="Microsoft">
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
    /// CaseResolutionAsync Test.
    /// </summary>
    [TestClass]
    public class CaseResolutionAsyncTest : UnitTestBase
    {
        /// <summary>
        /// Positive Scenario.
        /// </summary>
        [TestMethod]
        public void CaseResolutionAsyncTestUpdate()
        {
            //// Setting Input Parameters.
            Entity incidentReso = new Entity("incidentresolutions");
            incidentReso.Id = Guid.NewGuid();
            incidentReso["incidentid"] = new EntityReference("incident", Guid.NewGuid());
            incidentReso["resolutiontypecode"] = new OptionSetValue(1000);

            var postImg = new Entity("incidentresolutions");
            postImg.Attributes["incidentid"] = new EntityReference("incident", Guid.NewGuid());

            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", incidentReso),
               };
            //// Setting Post Entity Image.
            this.PluginExecutionContext.PostEntityImagesGet = () =>
            {
                return new EntityImageCollection
                 {
                        { "PostImage", postImg },
                 };
            };
            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "incident")
                {
                    Entity inc = new Entity("incident");
                    inc.Id = guid;
                    inc["casetypecode"] = new OptionSetValue(2);
                    return inc;
                }

                return null;
            };
            string secureString = $"{{\r\n  \"functionAppUrl\": \"https://test.azure-api.net/\",\r\n  \"client_id\": \"{Guid.Empty}\",\r\n  \"client_secret\": \"{Guid.Empty}\",\r\n  \"scope\":\"api://{Guid.Empty}/.default\",\r\n  \"tenantId\": \"{Guid.Empty}\",\r\n  \"tokenUrl\":\"https://login.microsoftonline.com/{{0}}/oauth2/token\",\r\n\"api-key\":\"{Guid.Empty}\"\r\n}}";
            CaseResolutionAsync plugin = new CaseResolutionAsync("test", secureString);
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Negative scenario where secure and unsecure strings are not provided.
        /// </summary>
        [TestMethod]

        public void CaseResolutionAsyncTestNoSecureUnsecureStrings()
        {
            // Creating the plugin instance without providing secure and unsecure strings.
            Action createPlugin = () => new CaseResolutionAsync(null, null);

            Assert.ThrowsException<InvalidPluginExecutionException>(createPlugin);

            // The expected exception (InvalidPluginExecutionException) will be thrown, indicating the absence of secure and unsecure strings.
        }
    }
}
