//-----------------------------------------------------------------------
// <copyright file="CaseTranslationPostOperationAsyncTest.cs" company="Microsoft">
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
    /// CaseTranslationPostOperationAsync Test.
    /// </summary>
    [TestClass]
    public class CaseTranslationPostOperationAsyncTest : UnitTestBase
    {
        /// <summary>
        /// Positive scenario for CaseTranslationPostOperationAsync Create Scenario.
        /// </summary>
        [TestMethod]
        public void CaseTranslationPostOperationAsyncTestCreate()
        {
            var incident = new Entity("incident");
            incident.Id = Guid.NewGuid();
            incident[Incident.caseOriginCode] = new OptionSetValue(2);
            incident[Incident.casetypecode] = new OptionSetValue(2);
            incident.Attributes[CaseTranslation.Source] = new OptionSetValue((int)Source.Jarvis);

            var postImg = new Entity("incident");
            postImg.Attributes[Casecontact.jarvisIncident] = new EntityReference("incident", incident.Id);

            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", incident),
               };

            this.PluginExecutionContext.MessageNameGet = () => "Create";

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
                if (entityName == "incident")
                {
                    Entity inc = new Entity("incident");
                    inc.Id = guid;
                    inc["casetypecode"] = new OptionSetValue(2);
                    return inc;
                }

                return null;
            };

            CaseTranslationPostOperationAsync plugin = new CaseTranslationPostOperationAsync("test", "test");

            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Positive scenario for CaseTranslationPostOperationAsync Update Scenario
        /// Post Image is not null.
        /// </summary>
        [TestMethod]
        public void CaseTranslationPostOperationAsyncTestUpdate()
        {
            var incident = new Entity("incident");
            incident.Id = Guid.NewGuid();
            incident[Incident.caseOriginCode] = new OptionSetValue(2);
            incident[Incident.casetypecode] = new OptionSetValue(2);

            var postImg = new Entity("incident");
            postImg.Attributes[Casecontact.jarvisIncident] = new EntityReference("incident", incident.Id);
            postImg.Attributes[CaseTranslation.Source] = new OptionSetValue((int)Source.Jarvis);
            var preImg = new Entity("incident");
            preImg.Attributes[Casecontact.jarvisIncident] = new EntityReference("incident", incident.Id);
            preImg.Attributes[CaseTranslation.Source] = new OptionSetValue((int)Source.Jarvis);

            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", incident),
               };

            this.PluginExecutionContext.MessageNameGet = () => "Update";

            //// Setting Post Entity Image.
            this.PluginExecutionContext.PostEntityImagesGet = () =>
            {
                return new EntityImageCollection
                 {
                        { "PostImage", postImg },
                        { "ComparePostImage", postImg },
                 };
            };
            //// Setting Pre Entity Image.
            this.PluginExecutionContext.PreEntityImagesGet = () =>
            {
                return new EntityImageCollection
                 {
                        { "ComparePreImage", preImg },
                 };
            };

            // Setting service retrieve.
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
            CaseTranslationPostOperationAsync plugin = new CaseTranslationPostOperationAsync("test", secureString);

            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Negative scenario where secure and unsecure strings are not provided.
        /// </summary>
        [TestMethod]

        // [ExpectedException(typeof(InvalidPluginExecutionException))]
        public void CaseTranslationPostOperationAsyncTestNoSecureUnsecureStrings()
        {
            // CaseTranslationPostOperationAsync plugin = new CaseTranslationPostOperationAsync(null, null);
            Action createPlugin = () => new CaseTranslationPostOperationAsync(null, null);
            Assert.ThrowsException<InvalidPluginExecutionException>(createPlugin);

            // The expected exception (InvalidPluginExecutionException) will be thrown, indicating the absence of secure and unsecure strings.
        }
    }
}
