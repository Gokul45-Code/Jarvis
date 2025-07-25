//-----------------------------------------------------------------------
// <copyright file="PassOutTransPostOperationAsyncTest.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace MCS.Jarvis.CE.UnitTest
{
    using System;
    using System.Collections.Generic;
    using MCS.Jarvis.CE.Plugins.PassOutTrans;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.Xrm.Sdk;
    using static MCS.Jarvis.CE.BusinessProcessesShared.Helpers.Constants;

    /// <summary>
    /// Pass Out Trans Post Operation Async Test.
    /// </summary>
    [TestClass]
    public class PassOutTransPostOperationAsyncTest : UnitTestBase
    {
        /// <summary>
        /// Positive scenario for Pass Out Trans Post Operation Async Create Scenario.
        /// </summary>
        [TestMethod]
        public void PassOutTransPostOperationAsyncTestCreate()
        {
            var incident = new Entity("incident");
            incident.Id = Guid.NewGuid();
            incident[Incident.caseOriginCode] = new OptionSetValue(2);
            incident[Incident.casetypecode] = new OptionSetValue(2);
            incident.Attributes[PassOutTranslation.Source] = new OptionSetValue((int)Source.Jarvis);

            var postImg = new Entity("jarvis_passout");
            postImg.Attributes[Casecontact.jarvisCase] = new EntityReference("incident", incident.Id);

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
                        { "PostImage", postImg },
                 };
            };

            // Setting service retrieve.
            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "systemuser")
                {
                    Entity user = new Entity("systemuser");
                    user.Id = Guid.NewGuid();
                    user["fullname"] = "Mercurius";
                    return user;
                }

                if (entityName == "incident")
                {
                    Entity inc = new Entity("incident");
                    inc.Id = guid;
                    inc["casetypecode"] = new OptionSetValue(2);
                    return inc;
                }

                return null;
            };

            PassOutTransPostOperationAsync plugin = new PassOutTransPostOperationAsync("test", "test");

            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Positive scenario for PassOutTransPostOperationAsync Update Scenario
        /// Post Image is not null.
        /// </summary>
        [TestMethod]
        public void PassOutTransPostOperationAsyncTestUpdate()
        {
            var incident = new Entity("incident");
            incident.Id = Guid.NewGuid();
            incident[Incident.caseOriginCode] = new OptionSetValue(2);
            incident[Incident.casetypecode] = new OptionSetValue(2);

            var postImg = new Entity("jarvis_passout");
            postImg.Attributes[Casecontact.jarvisCase] = new EntityReference("incident", incident.Id);
            postImg.Attributes[PassOutTranslation.Source] = new OptionSetValue((int)Source.Jarvis);
            postImg[PassOutTranslation.EtaReason] = "test";
            Entity preImage = new Entity("jarvis_passout");
            preImage.Id = Guid.NewGuid();
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", postImg),
               };

            this.PluginExecutionContext.MessageNameGet = () => "Update";
            this.PluginExecutionContext.StageGet = () => 40;

            //// Setting Post Entity Image.
            this.PluginExecutionContext.PostEntityImagesGet = () =>
            {
                return new EntityImageCollection
                 {
                        { "PostImage", postImg },
                        { "ComparePostImage", postImg },
                 };
            };
            this.PluginExecutionContext.PreEntityImagesGet = () => new EntityImageCollection
            {
                 { "ComparePreImage", preImage },
            };

            // Setting service retrieve.
            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "systemuser")
                {
                    Entity user = new Entity("systemuser");
                    user.Id = Guid.NewGuid();
                    user["fullname"] = "Mercurius";
                    return user;
                }

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
            PassOutTransPostOperationAsync plugin = new PassOutTransPostOperationAsync("test", secureString);

            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Negative scenario where secure and unsecure strings are not provided.
        /// </summary>
        [TestMethod]
        public void PassOutTransPostOperationAsyncTestNoSecureUnsecureStrings()
        {
            // Creating the plugin instance without providing secure and unsecure strings.
            Action createPlugin = () => new PassOutTransPostOperationAsync(null, null);

            Assert.ThrowsException<InvalidPluginExecutionException>(createPlugin);

            // The expected exception (InvalidPluginExecutionException) will be thrown, indicating the absence of secure and unsecure strings.
        }
    }
}
