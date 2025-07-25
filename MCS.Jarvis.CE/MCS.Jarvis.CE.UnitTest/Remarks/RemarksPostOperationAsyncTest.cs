// -----------------------------------------------------------------------
// <copyright file="RemarksPostOperationAsyncTest.cs" company="Microsoft">
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
    /// Remarks PostOperationAsyncTest.
    /// </summary>
    [TestClass]
    public class RemarksPostOperationAsyncTest : UnitTestBase
    {
        /// <summary>
        /// Positive Scenario but due to object is null we will get Expected Exception.
        /// </summary>
        [TestMethod]
        public void RemarksPostOperationAsyncTestUpdate()
        {
            //// Setting Input Parameters.
            Entity incident = new Entity("incident");
            incident.Id = Guid.NewGuid();
            incident[Incident.caseOriginCode] = new OptionSetValue(2);
            incident[Incident.casetypecode] = new OptionSetValue(2);
            incident.Attributes[Remark.remarkSource] = new OptionSetValue((int)PostSource.ManualPost);
            incident.Attributes[Remark.regardingField] = new EntityReference("incident", incident.Id);

            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", incident),
               };
            this.PluginExecutionContext.MessageNameGet = () => "Create";
            this.PluginExecutionContext.StageGet = () => 40;

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
            RemarksPostOperationAsync plugin = new RemarksPostOperationAsync("test", secureString);
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Negative scenario where secure and unsecure strings are not provided.
        /// </summary>
        [TestMethod]
        public void RemarksPostOperationAsyncTestNoSecureUnsecureStrings()
        {
            // Creating the plugin instance without providing secure and unsecure strings.
            Action createPlugin = () => new RemarksPostOperationAsync(null, null);

            Assert.ThrowsException<InvalidPluginExecutionException>(createPlugin);

            // The expected exception (InvalidPluginExecutionException) will be thrown, indicating the absence of secure and unsecure strings.
        }
    }
}
