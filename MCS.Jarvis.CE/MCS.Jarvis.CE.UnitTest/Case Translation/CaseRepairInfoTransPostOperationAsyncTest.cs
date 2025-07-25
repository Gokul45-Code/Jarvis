//-----------------------------------------------------------------------
// <copyright file="CaseRepairInfoTransPostOperationAsyncTest.cs" company="Microsoft">
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
    /// CaseRepairInfoTransPostOperationAsync Test.
    /// </summary>
    [TestClass]
    public class CaseRepairInfoTransPostOperationAsyncTest : UnitTestBase
    {
        /// <summary>
        /// Positive scenario for CaseRepairInfoTransPostOperationAsync Create Scenario.
        /// </summary>
        [TestMethod]
        public void CaseRepairInfoTransPostOperationAsyncTestCreate()
        {
            var repairInfoTrans = new Entity("jarvis_repairinformationtranslations");
            repairInfoTrans.Id = Guid.NewGuid();
            repairInfoTrans[Incident.caseOriginCode] = new OptionSetValue(2);
            repairInfoTrans[Incident.casetypecode] = new OptionSetValue(2);
            repairInfoTrans.Attributes[RepairInfoTranslation.Source] = new OptionSetValue((int)Source.Jarvis);
            repairInfoTrans.Attributes[RepairInfoTranslation.RepairInformation] = "test";
            repairInfoTrans.Attributes[RepairInfoTranslation.PartsInformation] = "test";
            repairInfoTrans.Attributes[RepairInfoTranslation.TowingRental] = "test";
            repairInfoTrans.Attributes[RepairInfoTranslation.WarrantyInformation] = "test";

            var postImg = new Entity("jarvis_repairinformationtranslations");
            postImg.Attributes[Casecontact.jarvisCase] = new EntityReference("incident", Guid.NewGuid());

            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", repairInfoTrans),
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
            CaseRepairInfoTransPostOperationAsync plugin = new CaseRepairInfoTransPostOperationAsync("test", secureString);

            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Positive scenario for CaseRepairInfoTransPostOperationAsync Update Scenario
        /// Post Image is not null.
        /// </summary>
        [TestMethod]
        public void CaseRepairInfoTransPostOperationAsyncTestUpdate()
        {
            var repairInfoTrans = new Entity("jarvis_repairinformationtranslations");
            repairInfoTrans.Id = Guid.NewGuid();
            repairInfoTrans[Incident.caseOriginCode] = new OptionSetValue(2);
            repairInfoTrans[Incident.casetypecode] = new OptionSetValue(2);
            repairInfoTrans.Attributes[RepairInfoTranslation.Source] = new OptionSetValue((int)Source.Jarvis);
            repairInfoTrans.Attributes[RepairInfoTranslation.RepairInformation] = "test";
            repairInfoTrans.Attributes[RepairInfoTranslation.PartsInformation] = "test";
            repairInfoTrans.Attributes[RepairInfoTranslation.TowingRental] = "test";
            repairInfoTrans.Attributes[RepairInfoTranslation.WarrantyInformation] = "test";

            var postImg = new Entity("jarvis_repairinformationtranslations");
            postImg.Attributes[Casecontact.jarvisCase] = new EntityReference("jarvis_repairinformationtranslations", Guid.NewGuid());
            postImg.Attributes[RepairInfoTranslation.Source] = new OptionSetValue((int)Source.Jarvis);

            var preImg = new Entity("jarvis_repairinformationtranslations");
            preImg.Attributes[Casecontact.jarvisCase] = new EntityReference("jarvis_repairinformationtranslations", Guid.NewGuid());
            preImg.Attributes[RepairInfoTranslation.Source] = new OptionSetValue((int)Source.Jarvis);
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", repairInfoTrans),
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
            CaseRepairInfoTransPostOperationAsync plugin = new CaseRepairInfoTransPostOperationAsync("test", secureString);

            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Positive scenario for CaseRepairInfoTransPostOperationAsync Update Scenario
        /// Post Image is not null.
        /// </summary>
        [TestMethod]
        public void CaseRepairInfoTransPostOperationAsyncTestUpdateComments()
        {
            var repairInfoTrans = new Entity("jarvis_repairinformationtranslations");
            repairInfoTrans.Id = Guid.NewGuid();
            repairInfoTrans[Incident.caseOriginCode] = new OptionSetValue(2);
            repairInfoTrans[Incident.casetypecode] = new OptionSetValue(2);
            repairInfoTrans.Attributes[RepairInfoTranslation.Source] = new OptionSetValue((int)Source.Jarvis);
            repairInfoTrans.Attributes[RepairInfoTranslation.RepairInformation] = "test";
            repairInfoTrans.Attributes[RepairInfoTranslation.PartsInformation] = "test";
            repairInfoTrans.Attributes[RepairInfoTranslation.TowingRental] = "test";
            repairInfoTrans.Attributes[RepairInfoTranslation.WarrantyInformation] = "test";

            var postImg = new Entity("jarvis_repairinformationtranslations");
            postImg.Attributes[Casecontact.jarvisCase] = new EntityReference("jarvis_repairinformationtranslations", Guid.NewGuid());
            postImg.Attributes[RepairInfoTranslation.Source] = new OptionSetValue((int)Source.Jarvis);
            postImg.Attributes[RepairInfoTranslation.RepairInformation] = "test";
            postImg.Attributes[RepairInfoTranslation.PartsInformation] = "test";
            postImg.Attributes[RepairInfoTranslation.TowingRental] = "test";
            postImg.Attributes[RepairInfoTranslation.WarrantyInformation] = "test";

            var preImg = new Entity("jarvis_repairinformationtranslations");
            preImg.Attributes[Casecontact.jarvisCase] = new EntityReference("jarvis_repairinformationtranslations", Guid.NewGuid());
            preImg.Attributes[RepairInfoTranslation.Source] = new OptionSetValue((int)Source.Jarvis);
            preImg.Attributes[RepairInfoTranslation.RepairInformation] = "test1";
            preImg.Attributes[RepairInfoTranslation.PartsInformation] = "test1";
            preImg.Attributes[RepairInfoTranslation.TowingRental] = "test1";
            preImg.Attributes[RepairInfoTranslation.WarrantyInformation] = "test1";
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", repairInfoTrans),
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
            CaseRepairInfoTransPostOperationAsync plugin = new CaseRepairInfoTransPostOperationAsync("test", secureString);

            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Negative scenario where secure and unsecure strings are not provided.
        /// </summary>
        [TestMethod]

        // [ExpectedException(typeof(InvalidPluginExecutionException))]
        public void CaseRepairInfoTransPostOperationAsyncTestNoSecureUnsecureStrings()
        {
            // Creating the plugin instance without providing secure and unsecure strings.
            Action createPlugin = () => new CaseRepairInfoTransPostOperationAsync(null, null);

            Assert.ThrowsException<InvalidPluginExecutionException>(createPlugin);

            // The expected exception (InvalidPluginExecutionException) will be thrown, indicating the absence of secure and unsecure strings.
        }
    }
}
