//-----------------------------------------------------------------------
// <copyright file="JobEndDetailsTransPostOperationTest.cs" company="Microsoft">
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
    /// Job End Details Trans Post Operation Test.
    /// </summary>
    [TestClass]
    public class JobEndDetailsTransPostOperationTest : UnitTestBase
    {
        /// <summary>
        /// Positive scenario for JobEndDetailsTransPostOperation Create Scenario.
        /// </summary>
        [TestMethod]
        public void JobEndDetailsTransPostOperationTestCreate()
        {
            var jedTranslation = new Entity("jarvis_jobenddetailstranslations");
            jedTranslation.Id = Guid.NewGuid();
            jedTranslation[Incident.caseOriginCode] = new OptionSetValue(2);
            jedTranslation[Incident.casetypecode] = new OptionSetValue(2);
            jedTranslation.Attributes[PassOutTranslation.Source] = new OptionSetValue((int)Source.Jarvis);
            jedTranslation.Attributes[JobEndTranslation.ActualCauseFault] = "test";
            jedTranslation.Attributes[JobEndTranslation.TemporaryRepair] = "test";

            var postImg = new Entity("jarvis_jobenddetailstranslations");
            postImg.Attributes[Casecontact.jarvisCase] = new EntityReference("incident", Guid.NewGuid());

            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", jedTranslation),
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
            string secureString = $"{{\r\n  \"functionAppUrl\": \"https://test.azure-api.net/\",\r\n  \"client_id\": \"{Guid.Empty}\",\r\n  \"client_secret\": \"{Guid.Empty}\",\r\n  \"scope\":\"api://{Guid.Empty}/.default\",\r\n  \"tenantId\": \"{Guid.Empty}\",\r\n  \"tokenUrl\":\"https://login.microsoftonline.com/{{0}}/oauth2/token\",\r\n\"api-key\":\"{Guid.Empty}\"\r\n}}";
            JobEndDetailsTransPostOperation plugin = new JobEndDetailsTransPostOperation("test", secureString);

            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Positive scenario for JobEndDetailsTransPostOperation Update Scenario
        /// Post Image is not null.
        /// </summary>
        [TestMethod]
        public void JobEndDetailsTransPostOperationTestUpdate()
        {
            var jedTranslation = new Entity("jarvis_jobenddetailstranslations");
            jedTranslation.Id = Guid.NewGuid();
            jedTranslation[Incident.caseOriginCode] = new OptionSetValue(2);
            jedTranslation[Incident.casetypecode] = new OptionSetValue(2);
            jedTranslation.Attributes[PassOutTranslation.Source] = new OptionSetValue((int)Source.Jarvis);
            jedTranslation.Attributes[JobEndTranslation.ActualCauseFault] = "test";
            jedTranslation.Attributes[JobEndTranslation.TemporaryRepair] = "test";

            var postImg = new Entity("jarvis_jobenddetailstranslations");
            postImg.Attributes[Casecontact.jarvisCase] = new EntityReference("incident", Guid.NewGuid());
            postImg.Attributes[PassOutTranslation.Source] = new OptionSetValue((int)Source.Jarvis);
            postImg.Attributes[JobEndTranslation.ActualCauseFault] = "test";
            postImg.Attributes[JobEndTranslation.TemporaryRepair] = "test";
            var preImg = new Entity("jarvis_jobenddetailstranslations");
            preImg.Attributes[Casecontact.jarvisCase] = new EntityReference("incident", Guid.NewGuid());
            preImg.Attributes[PassOutTranslation.Source] = new OptionSetValue((int)Source.Jarvis);

            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", jedTranslation),
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
            JobEndDetailsTransPostOperation plugin = new JobEndDetailsTransPostOperation("test", secureString);

            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Positive scenario for JobEndDetailsTransPostOperation Update Scenario
        /// Post Image is not null.
        /// </summary>
        [TestMethod]
        public void JobEndDetailsTransPostOperationTestUpdateText()
        {
            var jedTranslation = new Entity("jarvis_jobenddetailstranslations");
            jedTranslation.Id = Guid.NewGuid();
            jedTranslation[Incident.caseOriginCode] = new OptionSetValue(2);
            jedTranslation[Incident.casetypecode] = new OptionSetValue(2);
            jedTranslation.Attributes[PassOutTranslation.Source] = new OptionSetValue((int)Source.Jarvis);
            jedTranslation.Attributes[JobEndTranslation.ActualCauseFault] = "test";
            jedTranslation.Attributes[JobEndTranslation.TemporaryRepair] = "test";

            var postImg = new Entity("jarvis_jobenddetailstranslations");
            postImg.Attributes[Casecontact.jarvisCase] = new EntityReference("incident", Guid.NewGuid());
            postImg.Attributes[PassOutTranslation.Source] = new OptionSetValue((int)Source.Jarvis);
            postImg.Attributes[JobEndTranslation.ActualCauseFault] = "test";
            postImg.Attributes[JobEndTranslation.TemporaryRepair] = "test";
            var preImg = new Entity("jarvis_jobenddetailstranslations");
            preImg.Attributes[Casecontact.jarvisCase] = new EntityReference("incident", Guid.NewGuid());
            preImg.Attributes[PassOutTranslation.Source] = new OptionSetValue((int)Source.Jarvis);
            preImg.Attributes[JobEndTranslation.ActualCauseFault] = "testPre";
            preImg.Attributes[JobEndTranslation.TemporaryRepair] = "testPre";
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", jedTranslation),
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
            JobEndDetailsTransPostOperation plugin = new JobEndDetailsTransPostOperation("test", secureString);

            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Negative scenario where secure and unsecure strings are not provided.
        /// </summary>
        [TestMethod]

        // [ExpectedException(typeof(InvalidPluginExecutionException))]
        public void JobEndDetailsTransPostOperationTestNoSecureUnsecureStrings()
        {
            // Creating the plugin instance without providing secure and unsecure strings.
            Action createPlugin = () => new JobEndDetailsTransPostOperation(null, null);
            Assert.ThrowsException<InvalidPluginExecutionException>(createPlugin);
        }
    }
}
