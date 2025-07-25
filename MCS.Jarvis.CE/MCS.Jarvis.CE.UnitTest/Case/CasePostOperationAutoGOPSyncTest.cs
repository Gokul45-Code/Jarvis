// <copyright file="CasePostOperationAutoGOPSyncTest.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MCS.Jarvis.CE.UnitTest
{
    using System;
    using System.Collections.Generic;
    using MCS.Jarvis.CE.Plugins;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;
    using static MCS.Jarvis.CE.BusinessProcessesShared.Helpers.Constants;

    /// <summary>
    /// Case Post operation Auto GOP Sync test.
    /// </summary>
    [TestClass]
    public class CasePostOperationAutoGOPSyncTest : UnitTestBase
    {
        /// <summary>
        /// Positive scenario for CasePostOperationAutoGOPSyncTest Update Scenario.
        /// </summary>
        [TestMethod]
        public void CasePostOperationAutoGOPSyncTestUpdate()
        {
            Entity incident = new Entity("incident");
            incident.Id = Guid.Empty;
            incident["statuscode"] = new OptionSetValue(20);
            incident["caseorigincode"] = new OptionSetValue(334030002);
            incident["jarvis_mercuriusstatus"] = new OptionSetValue(200);
            incident["customerid"] = new EntityReference("account", Guid.NewGuid());

            Entity postEntityImage = new Entity("incident");
            postEntityImage["customerid"] = new EntityReference("account", Guid.NewGuid());
            postEntityImage[Incident.HomeDealer] = new EntityReference("account", Guid.NewGuid());
            postEntityImage.Attributes[Incident.CallerRole] = new OptionSetValue(4);
            //// Setting Input Parameters.
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", incident),
               };
            this.PluginExecutionContext.MessageNameGet = () => "Update";
            this.PluginExecutionContext.StageGet = () => 40;
            //// Setting Post and Pre Image
            this.PluginExecutionContext.PostEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PostImage", postEntityImage },
                };
            };

            // Setting service retrieve.
            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "account")
                {
                    Entity account = new Entity("account");
                    account.Id = guid;
                    account["jarvis_language"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    account["jarvis_address1_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    account[Accounts.Blacklist] = false;
                    return account;
                }

                return null;
            };
            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();
                if (query.GetType().Name == "FetchExpression")
                {
                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_configurationjarvis'>"))
                    {
                        Entity configResult = new Entity("jarvis_configurationjarvis");
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationinitialgop"] = true;
                        configResult["createdon"] = DateTime.UtcNow;
                        configResult.Id = Guid.NewGuid();
                        result.Entities.Add(configResult);
                    }
                }

                return result;
            };
            string secureString = $"{{\r\n  \"functionAppUrl\": \"https://test.azure-api.net/\",\r\n  \"client_id\": \"{Guid.Empty}\",\r\n  \"client_secret\": \"{Guid.Empty}\",\r\n  \"scope\":\"api://{Guid.Empty}/.default\",\r\n  \"tenantId\": \"{Guid.Empty}\",\r\n  \"tokenUrl\":\"https://login.microsoftonline.com/{{0}}/oauth2/token\",\r\n\"api-key\":\"{Guid.Empty}\"\r\n}}";
            CasePostOperationAutoGOPSync plugin = new CasePostOperationAutoGOPSync("test", secureString);

            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Positive scenario for CasePostOperationAutoGOPSyncTest Update Scenario.
        /// </summary>
        [TestMethod]
        public void CasePostOperationAutoGOPSyncTestUpdatePostImg()
        {
            Entity incident = new Entity("incident");
            incident.Id = Guid.Empty;
            incident["statuscode"] = new OptionSetValue(20);
            incident["jarvis_mercuriusstatus"] = new OptionSetValue(200);
            incident["customerid"] = new EntityReference("account", Guid.NewGuid());

            Entity postEntityImage = new Entity("incident");
            postEntityImage["caseorigincode"] = new OptionSetValue(1);
            postEntityImage["customerid"] = new EntityReference("account", Guid.NewGuid());
            postEntityImage[Incident.HomeDealer] = new EntityReference("account", Guid.NewGuid());
            postEntityImage.Attributes[Incident.CallerRole] = new OptionSetValue(3);
            //// Setting Input Parameters.
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", incident),
               };
            this.PluginExecutionContext.MessageNameGet = () => "Update";
            this.PluginExecutionContext.StageGet = () => 40;
            //// Setting Post and Pre Image
            this.PluginExecutionContext.PostEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PostImage", postEntityImage },
                };
            };

            // Setting service retrieve.
            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "account")
                {
                    Entity account = new Entity("account");
                    account.Id = guid;
                    account["jarvis_language"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    account["jarvis_address1_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    account[Accounts.Blacklist] = false;
                    return account;
                }

                return null;
            };
            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();
                if (query.GetType().Name == "FetchExpression")
                {
                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_configurationjarvis'>"))
                    {
                        Entity configResult = new Entity("jarvis_configurationjarvis");
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationinitialgop"] = true;
                        configResult["createdon"] = DateTime.UtcNow;
                        configResult.Id = Guid.NewGuid();
                        result.Entities.Add(configResult);
                    }
                }

                return result;
            };
            string secureString = $"{{\r\n  \"functionAppUrl\": \"https://test.azure-api.net/\",\r\n  \"client_id\": \"{Guid.Empty}\",\r\n  \"client_secret\": \"{Guid.Empty}\",\r\n  \"scope\":\"api://{Guid.Empty}/.default\",\r\n  \"tenantId\": \"{Guid.Empty}\",\r\n  \"tokenUrl\":\"https://login.microsoftonline.com/{{0}}/oauth2/token\",\r\n\"api-key\":\"{Guid.Empty}\"\r\n}}";
            CasePostOperationAutoGOPSync plugin = new CasePostOperationAutoGOPSync("test", secureString);

            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Positive scenario for CasePostOperationAutoGOPSyncTest Update Scenario.
        /// </summary>
        [TestMethod]
        public void CasePostOperationAutoGOPSyncTestUpdateException()
        {
            Entity incident = new Entity("incident");
            incident.Id = Guid.Empty;
            incident["statuscode"] = new OptionSetValue(20);
            incident["caseorigincode"] = new OptionSetValue(334030002);
            incident["jarvis_mercuriusstatus"] = new OptionSetValue(200);
            incident["customerid"] = new EntityReference("account", Guid.NewGuid());

            Entity postEntityImage = new Entity("incident");
            postEntityImage["customerid"] = new EntityReference("account", Guid.NewGuid());
            postEntityImage[Incident.HomeDealer] = new EntityReference("account", Guid.NewGuid());
            postEntityImage.Attributes[Incident.CallerRole] = new OptionSetValue(4);
            //// Setting Input Parameters.
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", incident),
               };
            this.PluginExecutionContext.MessageNameGet = () => "Update";
            this.PluginExecutionContext.StageGet = () => 40;
            //// Setting Post and Pre Image
            this.PluginExecutionContext.PostEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PostImage", postEntityImage },
                };
            };

            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();
                if (query.GetType().Name == "FetchExpression")
                {
                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_configurationjarvis'>"))
                    {
                        Entity configResult = new Entity("jarvis_configurationjarvis");
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationinitialgop"] = true;
                        configResult["createdon"] = DateTime.UtcNow;
                        configResult.Id = Guid.NewGuid();
                        result.Entities.Add(configResult);
                    }
                }

                return result;
            };
            CasePostOperationAutoGOPSync plugin = new CasePostOperationAutoGOPSync("test", null);

            plugin.Execute(this.ServiceProvider);
        }
    }
}
