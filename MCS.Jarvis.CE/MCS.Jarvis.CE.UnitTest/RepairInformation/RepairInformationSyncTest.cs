// <copyright file="RepairInformationSyncTest.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MCS.Jarvis.CE.UnitTest
{
    using System;
    using System.Collections.Generic;
    using MCS.Jarvis.CE.Plugins.RepairInformation;
    using Microsoft.Crm.Sdk.Messages;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;

    /// <summary>
    /// Repair Information Sync Test.
    /// </summary>
    [TestClass]
    public class RepairInformationSyncTest : UnitTestBase
    {
        /// <summary>
        /// Repair Information Sync Execute Create Test.
        /// </summary>
        [TestMethod]
        public void RepairInformationSyncExecuteCreateTest()
        {
            var repairInfo = new Entity("jarvis_repairinformations");
            repairInfo.Id = Guid.NewGuid();
            repairInfo["jarvis_incident"] = new EntityReference("incident", Guid.NewGuid());
            repairInfo.Attributes["jarvis_repairingdealerpassout"] = new EntityReference("jarvis_passout", Guid.NewGuid());
            repairInfo.Attributes["jarvis_partsinformation"] = "test";
            repairInfo.Attributes["jarvis_repairinformation"] = "test";
            repairInfo.Attributes["jarvis_towingrental"] = "test";
            repairInfo.Attributes["jarvis_warrantyinformation"] = "test";
            repairInfo.Attributes["jarvis_translationstatuspartsinformation"] = new OptionSetValue(334030000);
            repairInfo.Attributes["jarvis_translationstatusrepairinformation"] = new OptionSetValue(334030000);
            repairInfo.Attributes["jarvis_translationstatustowinginformation"] = new OptionSetValue(334030000);
            repairInfo.Attributes["jarvis_translationstatuswarrantyinformation"] = new OptionSetValue(334030000);
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", repairInfo),
               };
            this.PluginExecutionContext.MessageNameGet = () => "CREATE";
            this.PluginExecutionContext.StageGet = () => 40;

            // Setting service retrieve.
            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "jarvis_passout")
                {
                    Entity passout = new Entity("jarvis_passout");
                    passout.Id = Guid.NewGuid();
                    passout["jarvis_repairingdealer"] = new EntityReference("account", Guid.NewGuid());
                    return passout;
                }

                if (entityName == "incident")
                {
                    Entity inc = new Entity("incident");
                    inc.Id = guid;
                    inc["statuscode"] = new OptionSetValue(50);
                    inc["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
                    return inc;
                }

                if (entityName == "account")
                {
                    Entity account = new Entity("account");
                    account.Id = guid;
                    account["jarvis_language"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    account["jarvis_address1_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    return account;
                }

                if (entityName == "jarvis_country")
                {
                    Entity country = new Entity("jarvis_country");
                    country.Id = guid;
                    country["jarvis_iso2countrycode"] = "NL";
                    return country;
                }

                if (entityName == "jarvis_language")
                {
                    Entity language = new Entity("jarvis_language");
                    language.Id = guid;
                    language["jarvis_iso3languagecode6392t"] = "DEU";
                    return language;
                }

                if (entityName == "jarvis_repairinformations")
                {
                    Entity repairInfoRecord = new Entity("jarvis_repairinformations");
                    repairInfoRecord.Id = Guid.NewGuid();
                    repairInfoRecord["jarvis_incident"] = new EntityReference("incident", Guid.NewGuid());
                    repairInfoRecord.Attributes["jarvis_repairingdealerpassout"] = new EntityReference("jarvis_passout", Guid.NewGuid());
                    repairInfoRecord.Attributes["jarvis_partsinformation"] = "test";
                    repairInfoRecord.Attributes["jarvis_translationstatuspartsinformation"] = new OptionSetValue(334030000);
                    repairInfoRecord.Attributes["jarvis_translationstatusrepairinformation"] = new OptionSetValue(334030000);
                    repairInfoRecord.Attributes["jarvis_translationstatustowinginformation"] = new OptionSetValue(334030000);
                    repairInfoRecord.Attributes["jarvis_translationstatuswarrantyinformation"] = new OptionSetValue(334030000);
                    return repairInfoRecord;
                }

                return null;
            };
            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();
                if (query.GetType().Name == "FetchExpression")
                {
                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_passout'>"))
                    {
                        Entity passoutResult = new Entity("jarvis_passout");
                        passoutResult["jarvis_name"] = "testpassout";
                        passoutResult["createdon"] = DateTime.UtcNow;
                        passoutResult.Id = Guid.NewGuid();
                        result.Entities.Add(passoutResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_repairinformation'>"))
                    {
                        Entity repairInfoResult = new Entity("jarvis_repairinformation");
                        repairInfoResult["jarvis_name"] = "testpassout";
                        repairInfoResult["createdon"] = DateTime.UtcNow;
                        repairInfoResult.Id = Guid.NewGuid();
                        result.Entities.Add(repairInfoResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_configurationjarvis'>"))
                    {
                        Entity configResult = new Entity("jarvis_configurationjarvis");
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationmonitoraction"] = true;
                        configResult["createdon"] = DateTime.UtcNow;
                        configResult.Id = Guid.NewGuid();
                        result.Entities.Add(configResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_casemonitoraction'>"))
                    {
                        Entity casemonitoraction = new Entity("jarvis_casemonitoraction");
                        casemonitoraction["subject"] = true;
                        casemonitoraction["statuscode"] = new OptionSetValue(2);
                        casemonitoraction.Id = Guid.NewGuid();
                        result.Entities.Add(casemonitoraction);
                    }
                }

                return result;
            };
            this.Service.ExecuteOrganizationRequest = processInstanceRequest =>
            {
                return new LocalTimeFromUtcTimeResponse();
            };
            RepairInformationSync plugin = new RepairInformationSync();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Repair Information Sync Execute Update Test.
        /// </summary>
        [TestMethod]
        public void RepairInformationSyncExecuteUpdateTest()
        {
            var repairInfo = new Entity("jarvis_repairinformations");
            repairInfo.Id = Guid.NewGuid();
            repairInfo["jarvis_incident"] = new EntityReference("incident", Guid.NewGuid());
            repairInfo.Attributes["jarvis_repairingdealerpassout"] = new EntityReference("jarvis_passout", Guid.NewGuid());
            repairInfo.Attributes["jarvis_partsinformation"] = "test";
            repairInfo.Attributes["jarvis_translationstatuspartsinformation"] = new OptionSetValue(334030000);
            var postImg = new Entity("jarvis_repairinformations");
            postImg.Attributes["jarvis_incident"] = new EntityReference("incident", Guid.NewGuid());
            postImg.Attributes["jarvis_partsinformation"] = "test";
            postImg.Attributes["jarvis_repairinformation"] = "test";
            postImg.Attributes["jarvis_towingrental"] = "test";
            postImg.Attributes["jarvis_warrantyinformation"] = "test";
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", repairInfo),
               };
            this.PluginExecutionContext.MessageNameGet = () => "UPDATE";
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
                    inc["statuscode"] = new OptionSetValue(50);
                    inc["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
                    return inc;
                }

                if (entityName == "account")
                {
                    Entity account = new Entity("account");
                    account.Id = guid;
                    account["jarvis_language"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    account["jarvis_address1_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    return account;
                }

                if (entityName == "jarvis_language")
                {
                    Entity language = new Entity("jarvis_language");
                    language.Id = guid;
                    language["jarvis_iso3languagecode6392t"] = "DEU";
                    return language;
                }

                if (entityName == "jarvis_repairinformations")
                {
                    Entity repairInfoRecord = new Entity("jarvis_repairinformations");
                    repairInfoRecord.Id = Guid.NewGuid();
                    repairInfoRecord["jarvis_incident"] = new EntityReference("incident", Guid.NewGuid());
                    repairInfoRecord.Attributes["jarvis_repairingdealerpassout"] = new EntityReference("jarvis_passout", Guid.NewGuid());
                    repairInfoRecord.Attributes["jarvis_partsinformation"] = "test";
                    repairInfo.Attributes["jarvis_repairinformation"] = "test";
                    repairInfo.Attributes["jarvis_towingrental"] = "test";
                    repairInfo.Attributes["jarvis_warrantyinformation"] = "test";
                    repairInfoRecord.Attributes["jarvis_translationstatuspartsinformation"] = new OptionSetValue(334030000);
                    repairInfoRecord.Attributes["jarvis_translationstatusrepairinformation"] = new OptionSetValue(334030000);
                    repairInfoRecord.Attributes["jarvis_translationstatustowinginformation"] = new OptionSetValue(334030000);
                    repairInfoRecord.Attributes["jarvis_translationstatuswarrantyinformation"] = new OptionSetValue(334030000);
                    return repairInfoRecord;
                }

                return null;
            };
            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();
                if (query.GetType().Name == "FetchExpression")
                {
                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_passout'>"))
                    {
                        Entity passoutResult = new Entity("jarvis_passout");
                        passoutResult["jarvis_name"] = "testpassout";
                        passoutResult["createdon"] = DateTime.UtcNow;
                        passoutResult.Id = Guid.NewGuid();
                        result.Entities.Add(passoutResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_configurationjarvis'>"))
                    {
                        Entity configResult = new Entity("jarvis_configurationjarvis");
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["createdon"] = DateTime.UtcNow;
                        configResult.Id = Guid.NewGuid();
                        result.Entities.Add(configResult);
                    }
                }

                return result;
            };
            RepairInformationSync plugin = new RepairInformationSync();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Repair Information Sync Execute Exception Test.
        /// </summary>
        [ExpectedException(typeof(NullReferenceException))]
        [TestMethod]
        public void RepairInformationSyncExecuteExceptionTest()
        {
            var repairInfo = new Entity("jarvis_repairinformations");
            repairInfo.Id = Guid.NewGuid();
            repairInfo["jarvis_incident"] = new EntityReference("incident", Guid.NewGuid());
            repairInfo.Attributes["jarvis_repairingdealerpassout"] = new EntityReference("jarvis_passout", Guid.NewGuid());

            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", repairInfo),
               };
            this.PluginExecutionContext.MessageNameGet = () => "UPDATE";
            this.PluginExecutionContext.StageGet = () => 40;

            RepairInformationSync plugin = new RepairInformationSync();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Negative scenario.
        /// </summary>
        [ExpectedException(typeof(InvalidPluginExecutionException))]
        [TestMethod]
        public void RepairInformationSyncTestException()
        {
            RepairInformationSync plugin = new RepairInformationSync();
            plugin.Execute(null);
        }
    }
}
