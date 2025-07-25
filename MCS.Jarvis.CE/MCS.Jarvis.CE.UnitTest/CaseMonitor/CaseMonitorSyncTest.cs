//-----------------------------------------------------------------------
// <copyright file="CaseMonitorSyncTest.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace MCS.Jarvis.CE.UnitTest
{
    using System;
    using System.Collections.Generic;
    using MCS.Jarvis.CE.BusinessProcessesShared.Helpers;
    using MCS.Jarvis.CE.Plugins.CaseMonitor;
    using Microsoft.Crm.Sdk.Messages;
    using Microsoft.Crm.Sdk.Messages.Fakes;
    using Microsoft.QualityTools.Testing.Fakes;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;
    using static MCS.Jarvis.CE.BusinessProcessesShared.Helpers.Constants;

    /// <summary>
    /// CaseMonitorSync Test.
    /// </summary>
    [TestClass]
    public class CaseMonitorSyncTest : UnitTestBase
    {
        /// <summary>
        /// Positive scenario for CaseMonitorSync Create Scenario.
        /// </summary>
        [TestMethod]
        public void CaseMonitorTestPreCreateHighNoType()
        {
            var monitor = new Entity("incident");
            monitor.Id = Guid.NewGuid();
            monitor[Incident.caseOriginCode] = new OptionSetValue(2);
            monitor[Incident.casetypecode] = new OptionSetValue(2);
            monitor.Attributes[CaseMonitor.regardingobjectid] = new EntityReference("incident", monitor.Id);
            monitor["prioritycode"] = new OptionSetValue(0);
            monitor["actualstart"] = DateTime.Now;
            monitor["jarvis_followuptime"] = "12:00";
            monitor["subject"] = "Test Test";
            monitor["createdon"] = DateTime.Now;

            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", monitor),
               };
            this.PluginExecutionContext.MessageNameGet = () => "Create";
            this.PluginExecutionContext.StageGet = () => 20;

            // Setting service retrieve.
            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "incident")
                {
                    Entity inc = new Entity("incident");
                    inc.Id = guid;
                    inc["casetypecode"] = new OptionSetValue(2);
                    inc["statecode"] = new OptionSetValue(0);
                    inc[Incident.caseStatus] = new OptionSetValue(1);
                    inc["isescalated"] = true;
                    inc["routecase"] = true;
                    inc["ownerid"] = new EntityReference("users", Guid.NewGuid());
                    inc["jarvis_caselocation"] = new OptionSetValue(1);
                    inc["jarvis_caseserviceline"] = new EntityReference("jarivs_serviceline", Guid.NewGuid());
                    return inc;
                }

                return null;
            };

            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();
                if (query.GetType().Name == "QueryExpression")
                {
                    if ((query as QueryExpression).EntityName.Contains("usersettings"))
                    {
                        Entity userSettings = new Entity("usersettings");
                        userSettings["timezonecode"] = 105;
                        userSettings["localeid"] = Guid.NewGuid();
                        userSettings.Id = Guid.NewGuid();
                        result.Entities.Add(userSettings);
                    }
                }

                if (query.GetType().Name == "FetchExpression")
                {
                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_country'>"))
                    {
                        Entity country = new Entity("jarvis_country", Guid.NewGuid());
                        country["jarvis_name"] = "UK";
                        country["jarvis_iso2countrycode"] = "UK";
                        result.Entities.Add(country);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_language'>"))
                    {
                        Entity country = new Entity("jarvis_language", Guid.NewGuid());
                        country["jarvis_name"] = "English";
                        result.Entities.Add(country);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_language'>"))
                    {
                        Entity country = new Entity("jarvis_language", Guid.NewGuid());
                        country["jarvis_name"] = "English";
                        result.Entities.Add(country);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_integrationconfiguration'>"))
                    {
                        Entity intConfig = new Entity("jarvis_integrationconfiguration", Guid.NewGuid());
                        intConfig["jarvis_integrationname"] = "English";
                        intConfig["jarvis_integrationcode"] = "English";
                        intConfig["jarvis_integrationmapping"] = "English";
                        result.Entities.Add(intConfig);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='incident'>"))
                    {
                        Entity inc = new Entity("jarvis_integrationconfiguration", Guid.NewGuid());
                        inc["jarvis_preferredvasoperator"] = "English";
                        inc["title"] = "English";
                        inc[CaseNotification.Case2AccountAccountid] = new AliasedValue("accounts", "name", Guid.NewGuid());
                        inc[CaseNotification.Case2AccountName] = new AliasedValue("accounts", "name", "Test");

                        result.Entities.Add(inc);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_casepreferredagent'>"))
                    {
                        Entity casePref = new Entity("jarvis_casepreferredagent", Guid.NewGuid());
                        casePref["jarvis_preferredvasoperator"] = new EntityReference("users", Guid.NewGuid());
                        result.Entities.Add(casePref);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_casemonitoraction'>"))
                    {
                        Entity caseMonitor = new Entity("jarvis_casemonitoraction", Guid.NewGuid());
                        caseMonitor["subject"] = "Test";
                        caseMonitor["prioritycode"] = new OptionSetValue(0);
                        caseMonitor["jarvis_monitorsortorder"] = 1;
                        caseMonitor["jarvis_followuptimestamp"] = DateTime.Now;
                        caseMonitor["jarvis_followuplanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
                        caseMonitor["jarvis_followupcountry"] = new EntityReference("jarvis_country", Guid.NewGuid());
                        caseMonitor["createdby"] = new EntityReference("systemusers", Guid.NewGuid());
                        result.Entities.Add(caseMonitor);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_monitorskill'>"))
                    {
                        Entity skill = new Entity("jarvis_monitorskill", Guid.NewGuid());
                        skill["jarvis_preferredvasoperator"] = new EntityReference("users", Guid.NewGuid());
                        result.Entities.Add(skill);
                    }

                    if ((query as FetchExpression).Query.Contains(" <entity name='jarvis_configurationjarvis'>"))
                    {
                        Entity config = new Entity("jarvis_configurationjarvis", Guid.NewGuid());
                        config[MCS.Jarvis.CE.Plugins.Constants.JarvisConfiguration.Automationreleasecase] = new OptionSetValue(1);
                        result.Entities.Add(config);
                    }
                }

                return result;
            };
            CaseMonitorSync plugin = new CaseMonitorSync();
            this.Service.ExecuteOrganizationRequest = (request) =>
            {
                LocalTimeFromUtcTimeResponse response = new LocalTimeFromUtcTimeResponse();
                return response;
            };
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Positive scenario for CaseMonitorSync Create Scenario.
        /// </summary>
        [TestMethod]
        public void CaseMonitorTestPreCreateWithMediumActionType()
        {
            var monitor = new Entity("incident");
            monitor.Id = Guid.NewGuid();
            monitor[Incident.caseOriginCode] = new OptionSetValue(2);
            monitor[Incident.casetypecode] = new OptionSetValue(2);
            monitor.Attributes[CaseMonitor.regardingobjectid] = new EntityReference("incident", monitor.Id);
            monitor["prioritycode"] = new OptionSetValue(1);
            monitor["actualstart"] = DateTime.Now;
            monitor["subject"] = "Test Test";
            monitor["createdon"] = DateTime.Now;
            monitor["jarvis_actiontype"] = "test";

            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", monitor),
               };
            this.PluginExecutionContext.MessageNameGet = () => "Create";
            this.PluginExecutionContext.StageGet = () => 20;

            // Setting service retrieve.
            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "incident")
                {
                    Entity inc = new Entity("incident");
                    inc.Id = guid;
                    inc["casetypecode"] = new OptionSetValue(2);
                    inc["statecode"] = new OptionSetValue(0);
                    inc[Incident.caseStatus] = new OptionSetValue(10);
                    inc["isescalated"] = true;
                    inc["routecase"] = true;
                    inc["ownerid"] = new EntityReference("users", Guid.NewGuid());
                    inc["jarvis_caselocation"] = new OptionSetValue(1);
                    inc["jarvis_caseserviceline"] = new EntityReference("jarivs_serviceline", Guid.NewGuid());
                    return inc;
                }

                return null;
            };

            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();
                if (query.GetType().Name == "QueryExpression")
                {
                    if ((query as QueryExpression).EntityName.Contains("usersettings"))
                    {
                        Entity userSettings = new Entity("usersettings");
                        userSettings["timezonecode"] = 105;
                        userSettings["localeid"] = Guid.NewGuid();
                        userSettings.Id = Guid.NewGuid();
                        result.Entities.Add(userSettings);
                    }
                }

                if (query.GetType().Name == "FetchExpression")
                {
                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_country'>"))
                    {
                        Entity country = new Entity("jarvis_country", Guid.NewGuid());
                        country["jarvis_name"] = "UK";
                        country["jarvis_iso2countrycode"] = "UK";
                        result.Entities.Add(country);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_language'>"))
                    {
                        Entity country = new Entity("jarvis_language", Guid.NewGuid());
                        country["jarvis_name"] = "English";
                        result.Entities.Add(country);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_language'>"))
                    {
                        Entity country = new Entity("jarvis_language", Guid.NewGuid());
                        country["jarvis_name"] = "English";
                        result.Entities.Add(country);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_integrationconfiguration'>"))
                    {
                        Entity intConfig = new Entity("jarvis_integrationconfiguration", Guid.NewGuid());
                        intConfig["jarvis_integrationname"] = "English";
                        intConfig["jarvis_integrationcode"] = "English";
                        intConfig["jarvis_integrationmapping"] = "English";
                        result.Entities.Add(intConfig);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='incident'>"))
                    {
                        Entity inc = new Entity("jarvis_integrationconfiguration", Guid.NewGuid());
                        inc["jarvis_preferredvasoperator"] = "English";
                        inc["title"] = "English";
                        inc[CaseNotification.Case2AccountAccountid] = new AliasedValue("accounts", "name", Guid.NewGuid());
                        inc[CaseNotification.Case2AccountName] = new AliasedValue("accounts", "name", "Test");

                        result.Entities.Add(inc);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_casepreferredagent'>"))
                    {
                        Entity casePref = new Entity("jarvis_casepreferredagent", Guid.NewGuid());
                        casePref["jarvis_preferredvasoperator"] = new EntityReference("users", Guid.NewGuid());
                        result.Entities.Add(casePref);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_casemonitoraction'>"))
                    {
                        Entity caseMonitor = new Entity("jarvis_casemonitoraction", Guid.NewGuid());
                        caseMonitor["subject"] = "Test";
                        caseMonitor["prioritycode"] = new OptionSetValue(0);
                        caseMonitor["jarvis_monitorsortorder"] = 1;
                        caseMonitor["jarvis_followuptimestamp"] = DateTime.Now;
                        caseMonitor["jarvis_followuplanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
                        caseMonitor["jarvis_followupcountry"] = new EntityReference("jarvis_country", Guid.NewGuid());
                        caseMonitor["createdby"] = new EntityReference("systemusers", Guid.NewGuid());
                        result.Entities.Add(caseMonitor);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_monitorskill'>"))
                    {
                        Entity skill = new Entity("jarvis_monitorskill", Guid.NewGuid());
                        skill["jarvis_preferredvasoperator"] = new EntityReference("users", Guid.NewGuid());
                        result.Entities.Add(skill);
                    }

                    if ((query as FetchExpression).Query.Contains(" <entity name='jarvis_configurationjarvis'>"))
                    {
                        Entity config = new Entity("jarvis_configurationjarvis", Guid.NewGuid());
                        config[MCS.Jarvis.CE.Plugins.Constants.JarvisConfiguration.Automationreleasecase] = new OptionSetValue(1);
                        result.Entities.Add(config);
                    }
                }

                return result;
            };
            CaseMonitorSync plugin = new CaseMonitorSync();

            this.Service.ExecuteOrganizationRequest = (request) =>
            {
                LocalTimeFromUtcTimeResponse response = new LocalTimeFromUtcTimeResponse();
                return response;
            };
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Positive scenario for CaseMonitorSync Create Scenario.
        /// </summary>
        [TestMethod]
        public void CaseMonitorTestPreCreateWithType10()
        {
            var monitor = new Entity("incident");
            monitor.Id = Guid.NewGuid();
            monitor[Incident.caseOriginCode] = new OptionSetValue(2);
            monitor[Incident.casetypecode] = new OptionSetValue(2);
            monitor.Attributes[CaseMonitor.regardingobjectid] = new EntityReference("incident", monitor.Id);
            monitor["prioritycode"] = new OptionSetValue(1);
            monitor["actualstart"] = DateTime.Now;
            monitor["subject"] = "Test Test";
            monitor["createdon"] = DateTime.Now;

            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", monitor),
               };
            this.PluginExecutionContext.MessageNameGet = () => "Create";
            this.PluginExecutionContext.StageGet = () => 20;

            // Setting service retrieve.
            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "incident")
                {
                    Entity inc = new Entity("incident");
                    inc.Id = guid;
                    inc["casetypecode"] = new OptionSetValue(2);
                    inc["statecode"] = new OptionSetValue(0);
                    inc[Incident.caseStatus] = new OptionSetValue(10);
                    inc["isescalated"] = true;
                    inc["routecase"] = true;
                    inc["ownerid"] = new EntityReference("users", Guid.NewGuid());
                    inc["jarvis_caselocation"] = new OptionSetValue(1);
                    inc["jarvis_caseserviceline"] = new EntityReference("jarivs_serviceline", Guid.NewGuid());
                    return inc;
                }

                return null;
            };

            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();
                if (query.GetType().Name == "QueryExpression")
                {
                    if ((query as QueryExpression).EntityName.Contains("usersettings"))
                    {
                        Entity userSettings = new Entity("usersettings");
                        userSettings["timezonecode"] = 105;
                        userSettings["localeid"] = Guid.NewGuid();
                        userSettings.Id = Guid.NewGuid();
                        result.Entities.Add(userSettings);
                    }
                }

                if (query.GetType().Name == "FetchExpression")
                {
                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_country'>"))
                    {
                        Entity country = new Entity("jarvis_country", Guid.NewGuid());
                        country["jarvis_name"] = "UK";
                        country["jarvis_iso2countrycode"] = "UK";
                        result.Entities.Add(country);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_language'>"))
                    {
                        Entity country = new Entity("jarvis_language", Guid.NewGuid());
                        country["jarvis_name"] = "English";
                        result.Entities.Add(country);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_language'>"))
                    {
                        Entity country = new Entity("jarvis_language", Guid.NewGuid());
                        country["jarvis_name"] = "English";
                        result.Entities.Add(country);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_integrationconfiguration'>"))
                    {
                        Entity intConfig = new Entity("jarvis_integrationconfiguration", Guid.NewGuid());
                        intConfig["jarvis_integrationname"] = "English";
                        intConfig["jarvis_integrationcode"] = "English";
                        intConfig["jarvis_integrationmapping"] = "English";
                        result.Entities.Add(intConfig);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='incident'>"))
                    {
                        Entity inc = new Entity("jarvis_integrationconfiguration", Guid.NewGuid());
                        inc["jarvis_preferredvasoperator"] = "English";
                        inc["title"] = "English";
                        inc[CaseNotification.Case2AccountAccountid] = new AliasedValue("accounts", "name", Guid.NewGuid());
                        inc[CaseNotification.Case2AccountName] = new AliasedValue("accounts", "name", "Test");

                        result.Entities.Add(inc);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_casepreferredagent'>"))
                    {
                        Entity casePref = new Entity("jarvis_casepreferredagent", Guid.NewGuid());
                        casePref["jarvis_preferredvasoperator"] = new EntityReference("users", Guid.NewGuid());
                        result.Entities.Add(casePref);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_casemonitoraction'>"))
                    {
                        Entity caseMonitor = new Entity("jarvis_casemonitoraction", Guid.NewGuid());
                        caseMonitor["subject"] = "Test";
                        caseMonitor["prioritycode"] = new OptionSetValue(0);
                        caseMonitor["jarvis_monitorsortorder"] = 1;
                        caseMonitor["jarvis_followuptimestamp"] = DateTime.Now;
                        caseMonitor["jarvis_followuplanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
                        caseMonitor["jarvis_followupcountry"] = new EntityReference("jarvis_country", Guid.NewGuid());
                        caseMonitor["createdby"] = new EntityReference("systemusers", Guid.NewGuid());
                        result.Entities.Add(caseMonitor);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_monitorskill'>"))
                    {
                        Entity skill = new Entity("jarvis_monitorskill", Guid.NewGuid());
                        skill["jarvis_preferredvasoperator"] = new EntityReference("users", Guid.NewGuid());
                        result.Entities.Add(skill);
                    }

                    if ((query as FetchExpression).Query.Contains(" <entity name='jarvis_configurationjarvis'>"))
                    {
                        Entity config = new Entity("jarvis_configurationjarvis", Guid.NewGuid());
                        config[MCS.Jarvis.CE.Plugins.Constants.JarvisConfiguration.Automationreleasecase] = new OptionSetValue(1);
                        result.Entities.Add(config);
                    }
                }

                return result;
            };
            CaseMonitorSync plugin = new CaseMonitorSync();

            this.Service.ExecuteOrganizationRequest = (request) =>
            {
                LocalTimeFromUtcTimeResponse response = new LocalTimeFromUtcTimeResponse();
                return response;
            };
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Positive scenario for CaseMonitorSync Create Scenario.
        /// </summary>
        [TestMethod]
        public void CaseMonitorTestPreCreateWithType20()
        {
            var monitor = new Entity("incident");
            monitor.Id = Guid.NewGuid();
            monitor[Incident.caseOriginCode] = new OptionSetValue(2);
            monitor[Incident.casetypecode] = new OptionSetValue(2);
            monitor.Attributes[CaseMonitor.regardingobjectid] = new EntityReference("incident", monitor.Id);
            monitor["prioritycode"] = new OptionSetValue(1);
            monitor["actualstart"] = DateTime.Now;
            monitor["subject"] = "Test Test";
            monitor["createdon"] = DateTime.Now;

            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", monitor),
               };
            this.PluginExecutionContext.MessageNameGet = () => "Create";
            this.PluginExecutionContext.StageGet = () => 20;

            // Setting service retrieve.
            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "incident")
                {
                    Entity inc = new Entity("incident");
                    inc.Id = guid;
                    inc["casetypecode"] = new OptionSetValue(2);
                    inc["statecode"] = new OptionSetValue(0);
                    inc[Incident.caseStatus] = new OptionSetValue(20);
                    inc["isescalated"] = true;
                    inc["routecase"] = true;
                    inc["ownerid"] = new EntityReference("users", Guid.NewGuid());
                    inc["jarvis_caselocation"] = new OptionSetValue(1);
                    inc["jarvis_caseserviceline"] = new EntityReference("jarivs_serviceline", Guid.NewGuid());
                    return inc;
                }

                return null;
            };

            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();
                if (query.GetType().Name == "QueryExpression")
                {
                    if ((query as QueryExpression).EntityName.Contains("usersettings"))
                    {
                        Entity userSettings = new Entity("usersettings");
                        userSettings["timezonecode"] = 105;
                        userSettings["localeid"] = Guid.NewGuid();
                        userSettings.Id = Guid.NewGuid();
                        result.Entities.Add(userSettings);
                    }
                }

                if (query.GetType().Name == "FetchExpression")
                {
                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_country'>"))
                    {
                        Entity country = new Entity("jarvis_country", Guid.NewGuid());
                        country["jarvis_name"] = "UK";
                        country["jarvis_iso2countrycode"] = "UK";
                        result.Entities.Add(country);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_language'>"))
                    {
                        Entity country = new Entity("jarvis_language", Guid.NewGuid());
                        country["jarvis_name"] = "English";
                        result.Entities.Add(country);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_language'>"))
                    {
                        Entity country = new Entity("jarvis_language", Guid.NewGuid());
                        country["jarvis_name"] = "English";
                        result.Entities.Add(country);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_integrationconfiguration'>"))
                    {
                        Entity intConfig = new Entity("jarvis_integrationconfiguration", Guid.NewGuid());
                        intConfig["jarvis_integrationname"] = "English";
                        intConfig["jarvis_integrationcode"] = "English";
                        intConfig["jarvis_integrationmapping"] = "English";
                        result.Entities.Add(intConfig);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='incident'>"))
                    {
                        Entity inc = new Entity("jarvis_integrationconfiguration", Guid.NewGuid());
                        inc["jarvis_preferredvasoperator"] = "English";
                        inc["title"] = "English";
                        inc[CaseNotification.Case2AccountAccountid] = new AliasedValue("accounts", "name", Guid.NewGuid());
                        inc[CaseNotification.Case2AccountName] = new AliasedValue("accounts", "name", "Test");

                        result.Entities.Add(inc);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_casepreferredagent'>"))
                    {
                        Entity casePref = new Entity("jarvis_casepreferredagent", Guid.NewGuid());
                        casePref["jarvis_preferredvasoperator"] = new EntityReference("users", Guid.NewGuid());
                        result.Entities.Add(casePref);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_casemonitoraction'>"))
                    {
                        Entity caseMonitor = new Entity("jarvis_casemonitoraction", Guid.NewGuid());
                        caseMonitor["subject"] = "Test";
                        caseMonitor["prioritycode"] = new OptionSetValue(0);
                        caseMonitor["jarvis_monitorsortorder"] = 1;
                        caseMonitor["jarvis_followuptimestamp"] = DateTime.Now;
                        caseMonitor["jarvis_followuplanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
                        caseMonitor["jarvis_followupcountry"] = new EntityReference("jarvis_country", Guid.NewGuid());
                        caseMonitor["createdby"] = new EntityReference("systemusers", Guid.NewGuid());
                        result.Entities.Add(caseMonitor);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_monitorskill'>"))
                    {
                        Entity skill = new Entity("jarvis_monitorskill", Guid.NewGuid());
                        skill["jarvis_preferredvasoperator"] = new EntityReference("users", Guid.NewGuid());
                        result.Entities.Add(skill);
                    }

                    if ((query as FetchExpression).Query.Contains(" <entity name='jarvis_configurationjarvis'>"))
                    {
                        Entity config = new Entity("jarvis_configurationjarvis", Guid.NewGuid());
                        config[MCS.Jarvis.CE.Plugins.Constants.JarvisConfiguration.Automationreleasecase] = new OptionSetValue(1);
                        result.Entities.Add(config);
                    }
                }

                return result;
            };
            CaseMonitorSync plugin = new CaseMonitorSync();

            this.Service.ExecuteOrganizationRequest = (request) =>
            {
                LocalTimeFromUtcTimeResponse response = new LocalTimeFromUtcTimeResponse();
                return response;
            };
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Positive scenario for CaseMonitorSync Create Scenario.
        /// </summary>
        [TestMethod]
        public void CaseMonitorTestPreCreateWithType30()
        {
            var monitor = new Entity("incident");
            monitor.Id = Guid.NewGuid();
            monitor[Incident.caseOriginCode] = new OptionSetValue(2);
            monitor[Incident.casetypecode] = new OptionSetValue(2);
            monitor.Attributes[CaseMonitor.regardingobjectid] = new EntityReference("incident", monitor.Id);
            monitor["prioritycode"] = new OptionSetValue(1);
            monitor["actualstart"] = DateTime.Now;
            monitor["subject"] = "Test Test";
            monitor["createdon"] = DateTime.Now;

            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", monitor),
               };
            this.PluginExecutionContext.MessageNameGet = () => "Create";
            this.PluginExecutionContext.StageGet = () => 20;

            // Setting service retrieve.
            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "incident")
                {
                    Entity inc = new Entity("incident");
                    inc.Id = guid;
                    inc["casetypecode"] = new OptionSetValue(2);
                    inc["statecode"] = new OptionSetValue(0);
                    inc[Incident.caseStatus] = new OptionSetValue(30);
                    inc["isescalated"] = true;
                    inc["routecase"] = true;
                    inc["ownerid"] = new EntityReference("users", Guid.NewGuid());
                    inc["jarvis_caselocation"] = new OptionSetValue(1);
                    inc["jarvis_caseserviceline"] = new EntityReference("jarivs_serviceline", Guid.NewGuid());
                    return inc;
                }

                return null;
            };

            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();
                if (query.GetType().Name == "QueryExpression")
                {
                    if ((query as QueryExpression).EntityName.Contains("usersettings"))
                    {
                        Entity userSettings = new Entity("usersettings");
                        userSettings["timezonecode"] = 105;
                        userSettings["localeid"] = Guid.NewGuid();
                        userSettings.Id = Guid.NewGuid();
                        result.Entities.Add(userSettings);
                    }
                }

                if (query.GetType().Name == "FetchExpression")
                {
                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_country'>"))
                    {
                        Entity country = new Entity("jarvis_country", Guid.NewGuid());
                        country["jarvis_name"] = "UK";
                        country["jarvis_iso2countrycode"] = "UK";
                        result.Entities.Add(country);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_language'>"))
                    {
                        Entity country = new Entity("jarvis_language", Guid.NewGuid());
                        country["jarvis_name"] = "English";
                        result.Entities.Add(country);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_language'>"))
                    {
                        Entity country = new Entity("jarvis_language", Guid.NewGuid());
                        country["jarvis_name"] = "English";
                        result.Entities.Add(country);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_integrationconfiguration'>"))
                    {
                        Entity intConfig = new Entity("jarvis_integrationconfiguration", Guid.NewGuid());
                        intConfig["jarvis_integrationname"] = "English";
                        intConfig["jarvis_integrationcode"] = "English";
                        intConfig["jarvis_integrationmapping"] = "English";
                        result.Entities.Add(intConfig);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='incident'>"))
                    {
                        Entity inc = new Entity("jarvis_integrationconfiguration", Guid.NewGuid());
                        inc["jarvis_preferredvasoperator"] = "English";
                        inc["title"] = "English";
                        inc[CaseNotification.Case2AccountAccountid] = new AliasedValue("accounts", "name", Guid.NewGuid());
                        inc[CaseNotification.Case2AccountName] = new AliasedValue("accounts", "name", "Test");

                        result.Entities.Add(inc);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_casepreferredagent'>"))
                    {
                        Entity casePref = new Entity("jarvis_casepreferredagent", Guid.NewGuid());
                        casePref["jarvis_preferredvasoperator"] = new EntityReference("users", Guid.NewGuid());
                        result.Entities.Add(casePref);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_casemonitoraction'>"))
                    {
                        Entity caseMonitor = new Entity("jarvis_casemonitoraction", Guid.NewGuid());
                        caseMonitor["subject"] = "Test";
                        caseMonitor["prioritycode"] = new OptionSetValue(0);
                        caseMonitor["jarvis_monitorsortorder"] = 1;
                        caseMonitor["jarvis_followuptimestamp"] = DateTime.Now;
                        caseMonitor["jarvis_followuplanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
                        caseMonitor["jarvis_followupcountry"] = new EntityReference("jarvis_country", Guid.NewGuid());
                        caseMonitor["createdby"] = new EntityReference("systemusers", Guid.NewGuid());
                        result.Entities.Add(caseMonitor);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_monitorskill'>"))
                    {
                        Entity skill = new Entity("jarvis_monitorskill", Guid.NewGuid());
                        skill["jarvis_preferredvasoperator"] = new EntityReference("users", Guid.NewGuid());
                        result.Entities.Add(skill);
                    }

                    if ((query as FetchExpression).Query.Contains(" <entity name='jarvis_configurationjarvis'>"))
                    {
                        Entity config = new Entity("jarvis_configurationjarvis", Guid.NewGuid());
                        config[MCS.Jarvis.CE.Plugins.Constants.JarvisConfiguration.Automationreleasecase] = new OptionSetValue(1);
                        result.Entities.Add(config);
                    }
                }

                return result;
            };
            CaseMonitorSync plugin = new CaseMonitorSync();

            this.Service.ExecuteOrganizationRequest = (request) =>
            {
                LocalTimeFromUtcTimeResponse response = new LocalTimeFromUtcTimeResponse();
                return response;
            };
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Positive scenario for CaseMonitorSync Create Scenario.
        /// </summary>
        [TestMethod]
        public void CaseMonitorTestPreCreateWithTypeGOP()
        {
            var monitor = new Entity("incident");
            monitor.Id = Guid.NewGuid();
            monitor[Incident.caseOriginCode] = new OptionSetValue(2);
            monitor[Incident.casetypecode] = new OptionSetValue(2);
            monitor.Attributes[CaseMonitor.regardingobjectid] = new EntityReference("incident", monitor.Id);
            monitor["prioritycode"] = new OptionSetValue(2);
            monitor["actualstart"] = DateTime.Now;
            monitor["subject"] = "GOP+";
            monitor["createdon"] = DateTime.Now;

            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", monitor),
               };
            this.PluginExecutionContext.MessageNameGet = () => "Create";
            this.PluginExecutionContext.StageGet = () => 20;

            // Setting service retrieve.
            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "incident")
                {
                    Entity inc = new Entity("incident");
                    inc.Id = guid;
                    inc["casetypecode"] = new OptionSetValue(2);
                    inc["statecode"] = new OptionSetValue(0);
                    inc[Incident.caseStatus] = new OptionSetValue(60);
                    inc["isescalated"] = true;
                    inc["routecase"] = true;
                    inc["ownerid"] = new EntityReference("users", Guid.NewGuid());
                    inc["jarvis_caselocation"] = new OptionSetValue(1);
                    inc["jarvis_caseserviceline"] = new EntityReference("jarivs_serviceline", Guid.NewGuid());
                    return inc;
                }

                return null;
            };

            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();
                if (query.GetType().Name == "QueryExpression")
                {
                    if ((query as QueryExpression).EntityName.Contains("usersettings"))
                    {
                        Entity userSettings = new Entity("usersettings");
                        userSettings["timezonecode"] = 105;
                        userSettings["localeid"] = Guid.NewGuid();
                        userSettings.Id = Guid.NewGuid();
                        result.Entities.Add(userSettings);
                    }
                }

                if (query.GetType().Name == "FetchExpression")
                {
                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_country'>"))
                    {
                        Entity country = new Entity("jarvis_country", Guid.NewGuid());
                        country["jarvis_name"] = "UK";
                        country["jarvis_iso2countrycode"] = "UK";
                        result.Entities.Add(country);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_language'>"))
                    {
                        Entity country = new Entity("jarvis_language", Guid.NewGuid());
                        country["jarvis_name"] = "English";
                        result.Entities.Add(country);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_language'>"))
                    {
                        Entity country = new Entity("jarvis_language", Guid.NewGuid());
                        country["jarvis_name"] = "English";
                        result.Entities.Add(country);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_integrationconfiguration'>"))
                    {
                        Entity intConfig = new Entity("jarvis_integrationconfiguration", Guid.NewGuid());
                        intConfig["jarvis_integrationname"] = "English";
                        intConfig["jarvis_integrationcode"] = "English";
                        intConfig["jarvis_integrationmapping"] = "English";
                        result.Entities.Add(intConfig);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='incident'>"))
                    {
                        Entity inc = new Entity("jarvis_integrationconfiguration", Guid.NewGuid());
                        inc["jarvis_preferredvasoperator"] = "English";
                        inc["title"] = "English";
                        inc[CaseNotification.Case2AccountAccountid] = new AliasedValue("accounts", "name", Guid.NewGuid());
                        inc[CaseNotification.Case2AccountName] = new AliasedValue("accounts", "name", "Test");

                        result.Entities.Add(inc);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_casepreferredagent'>"))
                    {
                        Entity casePref = new Entity("jarvis_casepreferredagent", Guid.NewGuid());
                        casePref["jarvis_preferredvasoperator"] = new EntityReference("users", Guid.NewGuid());
                        result.Entities.Add(casePref);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_casemonitoraction'>"))
                    {
                        Entity caseMonitor = new Entity("jarvis_casemonitoraction", Guid.NewGuid());
                        caseMonitor["subject"] = "Test";
                        caseMonitor["prioritycode"] = new OptionSetValue(0);
                        caseMonitor["jarvis_monitorsortorder"] = 1;
                        caseMonitor["jarvis_followuptimestamp"] = DateTime.Now;
                        caseMonitor["jarvis_followuplanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
                        caseMonitor["jarvis_followupcountry"] = new EntityReference("jarvis_country", Guid.NewGuid());
                        caseMonitor["createdby"] = new EntityReference("systemusers", Guid.NewGuid());
                        result.Entities.Add(caseMonitor);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_monitorskill'>"))
                    {
                        Entity skill = new Entity("jarvis_monitorskill", Guid.NewGuid());
                        skill["jarvis_preferredvasoperator"] = new EntityReference("users", Guid.NewGuid());
                        result.Entities.Add(skill);
                    }

                    if ((query as FetchExpression).Query.Contains(" <entity name='jarvis_configurationjarvis'>"))
                    {
                        Entity config = new Entity("jarvis_configurationjarvis", Guid.NewGuid());
                        config[MCS.Jarvis.CE.Plugins.Constants.JarvisConfiguration.Automationreleasecase] = new OptionSetValue(1);
                        result.Entities.Add(config);
                    }
                }

                return result;
            };
            CaseMonitorSync plugin = new CaseMonitorSync();

            this.Service.ExecuteOrganizationRequest = (request) =>
            {
                LocalTimeFromUtcTimeResponse response = new LocalTimeFromUtcTimeResponse();
                return response;
            };
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Positive scenario for CaseMonitorSync Create Scenario.
        /// </summary>
        [TestMethod]
        public void CaseMonitorTestPreCreateWithTypeDESC()
        {
            var monitor = new Entity("incident");
            monitor.Id = Guid.NewGuid();
            monitor[Incident.caseOriginCode] = new OptionSetValue(2);
            monitor[Incident.casetypecode] = new OptionSetValue(2);
            monitor.Attributes[CaseMonitor.regardingobjectid] = new EntityReference("incident", monitor.Id);
            monitor["prioritycode"] = new OptionSetValue(2);
            monitor["actualstart"] = DateTime.Now;
            monitor["subject"] = "DESC";
            monitor["createdon"] = DateTime.Now;

            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", monitor),
               };
            this.PluginExecutionContext.MessageNameGet = () => "Create";
            this.PluginExecutionContext.StageGet = () => 20;

            // Setting service retrieve.
            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "incident")
                {
                    Entity inc = new Entity("incident");
                    inc.Id = guid;
                    inc["casetypecode"] = new OptionSetValue(2);
                    inc["statecode"] = new OptionSetValue(0);
                    inc[Incident.caseStatus] = new OptionSetValue(60);
                    inc["isescalated"] = true;
                    inc["routecase"] = true;
                    inc["ownerid"] = new EntityReference("users", Guid.NewGuid());
                    inc["jarvis_caselocation"] = new OptionSetValue(1);
                    inc["jarvis_caseserviceline"] = new EntityReference("jarivs_serviceline", Guid.NewGuid());
                    return inc;
                }

                return null;
            };

            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();
                if (query.GetType().Name == "QueryExpression")
                {
                    if ((query as QueryExpression).EntityName.Contains("usersettings"))
                    {
                        Entity userSettings = new Entity("usersettings");
                        userSettings["timezonecode"] = 105;
                        userSettings["localeid"] = Guid.NewGuid();
                        userSettings.Id = Guid.NewGuid();
                        result.Entities.Add(userSettings);
                    }
                }

                if (query.GetType().Name == "FetchExpression")
                {
                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_country'>"))
                    {
                        Entity country = new Entity("jarvis_country", Guid.NewGuid());
                        country["jarvis_name"] = "UK";
                        country["jarvis_iso2countrycode"] = "UK";
                        result.Entities.Add(country);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_language'>"))
                    {
                        Entity country = new Entity("jarvis_language", Guid.NewGuid());
                        country["jarvis_name"] = "English";
                        result.Entities.Add(country);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_language'>"))
                    {
                        Entity country = new Entity("jarvis_language", Guid.NewGuid());
                        country["jarvis_name"] = "English";
                        result.Entities.Add(country);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_integrationconfiguration'>"))
                    {
                        Entity intConfig = new Entity("jarvis_integrationconfiguration", Guid.NewGuid());
                        intConfig["jarvis_integrationname"] = "English";
                        intConfig["jarvis_integrationcode"] = "English";
                        intConfig["jarvis_integrationmapping"] = "English";
                        result.Entities.Add(intConfig);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='incident'>"))
                    {
                        Entity inc = new Entity("jarvis_integrationconfiguration", Guid.NewGuid());
                        inc["jarvis_preferredvasoperator"] = "English";
                        inc["title"] = "English";
                        inc[CaseNotification.Case2AccountAccountid] = new AliasedValue("accounts", "name", Guid.NewGuid());
                        inc[CaseNotification.Case2AccountName] = new AliasedValue("accounts", "name", "Test");

                        result.Entities.Add(inc);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_casepreferredagent'>"))
                    {
                        Entity casePref = new Entity("jarvis_casepreferredagent", Guid.NewGuid());
                        casePref["jarvis_preferredvasoperator"] = new EntityReference("users", Guid.NewGuid());
                        result.Entities.Add(casePref);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_casemonitoraction'>"))
                    {
                        Entity caseMonitor = new Entity("jarvis_casemonitoraction", Guid.NewGuid());
                        caseMonitor["subject"] = "Test";
                        caseMonitor["prioritycode"] = new OptionSetValue(0);
                        caseMonitor["jarvis_monitorsortorder"] = 1;
                        caseMonitor["jarvis_followuptimestamp"] = DateTime.Now;
                        caseMonitor["jarvis_followuplanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
                        caseMonitor["jarvis_followupcountry"] = new EntityReference("jarvis_country", Guid.NewGuid());
                        caseMonitor["createdby"] = new EntityReference("systemusers", Guid.NewGuid());
                        result.Entities.Add(caseMonitor);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_monitorskill'>"))
                    {
                        Entity skill = new Entity("jarvis_monitorskill", Guid.NewGuid());
                        skill["jarvis_preferredvasoperator"] = new EntityReference("users", Guid.NewGuid());
                        result.Entities.Add(skill);
                    }

                    if ((query as FetchExpression).Query.Contains(" <entity name='jarvis_configurationjarvis'>"))
                    {
                        Entity config = new Entity("jarvis_configurationjarvis", Guid.NewGuid());
                        config[MCS.Jarvis.CE.Plugins.Constants.JarvisConfiguration.Automationreleasecase] = new OptionSetValue(1);
                        result.Entities.Add(config);
                    }
                }

                return result;
            };
            CaseMonitorSync plugin = new CaseMonitorSync();

            this.Service.ExecuteOrganizationRequest = (request) =>
            {
                LocalTimeFromUtcTimeResponse response = new LocalTimeFromUtcTimeResponse();
                return response;
            };
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Positive scenario for CaseMonitorSync Create Scenario.
        /// </summary>
        [TestMethod]
        public void CaseMonitorTestPreCreateWithMediumType()
        {
            var monitor = new Entity("incident");
            monitor.Id = Guid.NewGuid();
            monitor[Incident.caseOriginCode] = new OptionSetValue(2);
            monitor[Incident.casetypecode] = new OptionSetValue(2);
            monitor.Attributes[CaseMonitor.regardingobjectid] = new EntityReference("incident", monitor.Id);
            monitor["prioritycode"] = new OptionSetValue(1);
            monitor["actualstart"] = DateTime.Now;
            monitor["createdon"] = DateTime.Now;

            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", monitor),
               };
            this.PluginExecutionContext.MessageNameGet = () => "Create";
            this.PluginExecutionContext.StageGet = () => 20;

            // Setting service retrieve.
            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "incident")
                {
                    Entity inc = new Entity("incident");
                    inc.Id = guid;
                    inc["casetypecode"] = new OptionSetValue(2);
                    inc["statecode"] = new OptionSetValue(0);
                    inc[Incident.caseStatus] = new OptionSetValue(70);
                    inc["isescalated"] = false;
                    inc["routecase"] = true;
                    inc["ownerid"] = new EntityReference("users", Guid.NewGuid());
                    inc["jarvis_caselocation"] = new OptionSetValue(1);
                    inc["jarvis_caseserviceline"] = new EntityReference("jarivs_serviceline", Guid.NewGuid());
                    return inc;
                }

                return null;
            };

            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();
                if (query.GetType().Name == "QueryExpression")
                {
                    if ((query as QueryExpression).EntityName.Contains("usersettings"))
                    {
                        Entity userSettings = new Entity("usersettings");
                        userSettings["timezonecode"] = 105;
                        userSettings["localeid"] = Guid.NewGuid();
                        userSettings.Id = Guid.NewGuid();
                        result.Entities.Add(userSettings);
                    }
                }

                if (query.GetType().Name == "FetchExpression")
                {
                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_country'>"))
                    {
                        Entity country = new Entity("jarvis_country", Guid.NewGuid());
                        country["jarvis_name"] = "UK";
                        country["jarvis_iso2countrycode"] = "UK";
                        result.Entities.Add(country);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_language'>"))
                    {
                        Entity country = new Entity("jarvis_language", Guid.NewGuid());
                        country["jarvis_name"] = "English";
                        result.Entities.Add(country);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_language'>"))
                    {
                        Entity country = new Entity("jarvis_language", Guid.NewGuid());
                        country["jarvis_name"] = "English";
                        result.Entities.Add(country);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_integrationconfiguration'>"))
                    {
                        Entity intConfig = new Entity("jarvis_integrationconfiguration", Guid.NewGuid());
                        intConfig["jarvis_integrationname"] = "English";
                        intConfig["jarvis_integrationcode"] = "English";
                        intConfig["jarvis_integrationmapping"] = "English";
                        result.Entities.Add(intConfig);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='incident'>"))
                    {
                        Entity inc = new Entity("jarvis_integrationconfiguration", Guid.NewGuid());
                        inc["jarvis_preferredvasoperator"] = "English";
                        inc["title"] = "English";
                        inc[CaseNotification.Case2AccountAccountid] = new AliasedValue("accounts", "name", Guid.NewGuid());
                        inc[CaseNotification.Case2AccountName] = new AliasedValue("accounts", "name", "Test");

                        result.Entities.Add(inc);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_casepreferredagent'>"))
                    {
                        Entity casePref = new Entity("jarvis_casepreferredagent", Guid.NewGuid());
                        casePref["jarvis_preferredvasoperator"] = new EntityReference("users", Guid.NewGuid());
                        result.Entities.Add(casePref);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_casemonitoraction'>"))
                    {
                        Entity caseMonitor = new Entity("jarvis_casemonitoraction", Guid.NewGuid());
                        caseMonitor["subject"] = "Test";
                        caseMonitor["prioritycode"] = new OptionSetValue(0);
                        caseMonitor["jarvis_monitorsortorder"] = 1;
                        caseMonitor["jarvis_followuptimestamp"] = DateTime.Now;
                        caseMonitor["jarvis_followuplanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
                        caseMonitor["jarvis_followupcountry"] = new EntityReference("jarvis_country", Guid.NewGuid());
                        caseMonitor["createdby"] = new EntityReference("systemusers", Guid.NewGuid());
                        result.Entities.Add(caseMonitor);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_monitorskill'>"))
                    {
                        Entity skill = new Entity("jarvis_monitorskill", Guid.NewGuid());
                        skill["jarvis_preferredvasoperator"] = new EntityReference("users", Guid.NewGuid());
                        result.Entities.Add(skill);
                    }

                    if ((query as FetchExpression).Query.Contains(" <entity name='jarvis_configurationjarvis'>"))
                    {
                        Entity config = new Entity("jarvis_configurationjarvis", Guid.NewGuid());
                        config[MCS.Jarvis.CE.Plugins.Constants.JarvisConfiguration.Automationreleasecase] = new OptionSetValue(1);
                        result.Entities.Add(config);
                    }
                }

                return result;
            };
            CaseMonitorSync plugin = new CaseMonitorSync();

            this.Service.ExecuteOrganizationRequest = (request) =>
            {
                LocalTimeFromUtcTimeResponse response = new LocalTimeFromUtcTimeResponse();
                return response;
            };
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Positive scenario for CaseMonitorSync Create Scenario.
        /// </summary>
        [TestMethod]
        public void CaseMonitorTestPreCreateWithStandardType()
        {
            var monitor = new Entity("incident");
            monitor.Id = Guid.NewGuid();
            monitor[Incident.caseOriginCode] = new OptionSetValue(2);
            monitor[Incident.casetypecode] = new OptionSetValue(2);
            monitor.Attributes[CaseMonitor.regardingobjectid] = new EntityReference("incident", monitor.Id);
            monitor["prioritycode"] = new OptionSetValue(2);
            monitor["actualstart"] = DateTime.Now;
            monitor["createdon"] = DateTime.Now;

            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", monitor),
               };
            this.PluginExecutionContext.MessageNameGet = () => "Create";
            this.PluginExecutionContext.StageGet = () => 20;

            // Setting service retrieve.
            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "incident")
                {
                    Entity inc = new Entity("incident");
                    inc.Id = guid;
                    inc["casetypecode"] = new OptionSetValue(2);
                    inc["statecode"] = new OptionSetValue(0);
                    inc[Incident.caseStatus] = new OptionSetValue(70);
                    inc["isescalated"] = false;
                    inc["routecase"] = true;
                    inc["ownerid"] = new EntityReference("users", Guid.NewGuid());
                    inc["jarvis_caselocation"] = new OptionSetValue(1);
                    inc["jarvis_caseserviceline"] = new EntityReference("jarivs_serviceline", Guid.NewGuid());
                    return inc;
                }

                return null;
            };

            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();
                if (query.GetType().Name == "QueryExpression")
                {
                    if ((query as QueryExpression).EntityName.Contains("usersettings"))
                    {
                        Entity userSettings = new Entity("usersettings");
                        userSettings["timezonecode"] = 105;
                        userSettings["localeid"] = Guid.NewGuid();
                        userSettings.Id = Guid.NewGuid();
                        result.Entities.Add(userSettings);
                    }
                }

                if (query.GetType().Name == "FetchExpression")
                {
                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_country'>"))
                    {
                        Entity country = new Entity("jarvis_country", Guid.NewGuid());
                        country["jarvis_name"] = "UK";
                        country["jarvis_iso2countrycode"] = "UK";
                        result.Entities.Add(country);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_language'>"))
                    {
                        Entity country = new Entity("jarvis_language", Guid.NewGuid());
                        country["jarvis_name"] = "English";
                        result.Entities.Add(country);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_language'>"))
                    {
                        Entity country = new Entity("jarvis_language", Guid.NewGuid());
                        country["jarvis_name"] = "English";
                        result.Entities.Add(country);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_integrationconfiguration'>"))
                    {
                        Entity intConfig = new Entity("jarvis_integrationconfiguration", Guid.NewGuid());
                        intConfig["jarvis_integrationname"] = "English";
                        intConfig["jarvis_integrationcode"] = "English";
                        intConfig["jarvis_integrationmapping"] = "English";
                        result.Entities.Add(intConfig);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='incident'>"))
                    {
                        Entity inc = new Entity("jarvis_integrationconfiguration", Guid.NewGuid());
                        inc["jarvis_preferredvasoperator"] = "English";
                        inc["title"] = "English";
                        inc[CaseNotification.Case2AccountAccountid] = new AliasedValue("accounts", "name", Guid.NewGuid());
                        inc[CaseNotification.Case2AccountName] = new AliasedValue("accounts", "name", "Test");

                        result.Entities.Add(inc);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_casepreferredagent'>"))
                    {
                        Entity casePref = new Entity("jarvis_casepreferredagent", Guid.NewGuid());
                        casePref["jarvis_preferredvasoperator"] = new EntityReference("users", Guid.NewGuid());
                        result.Entities.Add(casePref);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_casemonitoraction'>"))
                    {
                        Entity caseMonitor = new Entity("jarvis_casemonitoraction", Guid.NewGuid());
                        caseMonitor["subject"] = "Test";
                        caseMonitor["prioritycode"] = new OptionSetValue(2);
                        caseMonitor["jarvis_monitorsortorder"] = 1;
                        caseMonitor["jarvis_followuptimestamp"] = DateTime.Now;
                        caseMonitor["jarvis_followuplanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
                        caseMonitor["jarvis_followupcountry"] = new EntityReference("jarvis_country", Guid.NewGuid());
                        caseMonitor["createdby"] = new EntityReference("systemusers", Guid.NewGuid());
                        result.Entities.Add(caseMonitor);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_monitorskill'>"))
                    {
                        Entity skill = new Entity("jarvis_monitorskill", Guid.NewGuid());
                        skill["jarvis_preferredvasoperator"] = new EntityReference("users", Guid.NewGuid());
                        result.Entities.Add(skill);
                    }

                    if ((query as FetchExpression).Query.Contains(" <entity name='jarvis_configurationjarvis'>"))
                    {
                        Entity config = new Entity("jarvis_configurationjarvis", Guid.NewGuid());
                        config[MCS.Jarvis.CE.Plugins.Constants.JarvisConfiguration.Automationreleasecase] = new OptionSetValue(1);
                        result.Entities.Add(config);
                    }
                }

                return result;
            };
            CaseMonitorSync plugin = new CaseMonitorSync();

            this.Service.ExecuteOrganizationRequest = (request) =>
            {
                LocalTimeFromUtcTimeResponse response = new LocalTimeFromUtcTimeResponse();
                return response;
            };
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Positive scenario for CaseMonitorSync Create Scenario.
        /// </summary>
        [TestMethod]
        public void CaseMonitorTestPreCreateWithEscalate()
        {
            var monitor = new Entity("incident");
            monitor.Id = Guid.NewGuid();
            monitor[Incident.caseOriginCode] = new OptionSetValue(2);
            monitor[Incident.casetypecode] = new OptionSetValue(2);
            monitor.Attributes[CaseMonitor.regardingobjectid] = new EntityReference("incident", monitor.Id);
            monitor["prioritycode"] = new OptionSetValue(2);
            monitor["actualstart"] = DateTime.Now;
            monitor["createdon"] = DateTime.Now;

            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", monitor),
               };
            this.PluginExecutionContext.MessageNameGet = () => "Create";
            this.PluginExecutionContext.StageGet = () => 20;

            // Setting service retrieve.
            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "incident")
                {
                    Entity inc = new Entity("incident");
                    inc.Id = guid;
                    inc["casetypecode"] = new OptionSetValue(2);
                    inc["statecode"] = new OptionSetValue(0);
                    inc[Incident.caseStatus] = new OptionSetValue(70);
                    inc["isescalated"] = true;
                    inc["routecase"] = true;
                    inc["ownerid"] = new EntityReference("users", Guid.NewGuid());
                    inc["jarvis_caselocation"] = new OptionSetValue(1);
                    inc["jarvis_caseserviceline"] = new EntityReference("jarivs_serviceline", Guid.NewGuid());
                    return inc;
                }

                return null;
            };

            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();
                if (query.GetType().Name == "QueryExpression")
                {
                    if ((query as QueryExpression).EntityName.Contains("usersettings"))
                    {
                        Entity userSettings = new Entity("usersettings");
                        userSettings["timezonecode"] = 105;
                        userSettings["localeid"] = Guid.NewGuid();
                        userSettings.Id = Guid.NewGuid();
                        result.Entities.Add(userSettings);
                    }
                }

                if (query.GetType().Name == "FetchExpression")
                {
                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_country'>"))
                    {
                        Entity country = new Entity("jarvis_country", Guid.NewGuid());
                        country["jarvis_name"] = "UK";
                        country["jarvis_iso2countrycode"] = "UK";
                        result.Entities.Add(country);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_language'>"))
                    {
                        Entity country = new Entity("jarvis_language", Guid.NewGuid());
                        country["jarvis_name"] = "English";
                        result.Entities.Add(country);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_language'>"))
                    {
                        Entity country = new Entity("jarvis_language", Guid.NewGuid());
                        country["jarvis_name"] = "English";
                        result.Entities.Add(country);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_integrationconfiguration'>"))
                    {
                        Entity intConfig = new Entity("jarvis_integrationconfiguration", Guid.NewGuid());
                        intConfig["jarvis_integrationname"] = "English";
                        intConfig["jarvis_integrationcode"] = "English";
                        intConfig["jarvis_integrationmapping"] = "English";
                        result.Entities.Add(intConfig);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='incident'>"))
                    {
                        Entity inc = new Entity("jarvis_integrationconfiguration", Guid.NewGuid());
                        inc["jarvis_preferredvasoperator"] = "English";
                        inc["title"] = "English";
                        inc[CaseNotification.Case2AccountAccountid] = new AliasedValue("accounts", "name", Guid.NewGuid());
                        inc[CaseNotification.Case2AccountName] = new AliasedValue("accounts", "name", "Test");

                        result.Entities.Add(inc);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_casepreferredagent'>"))
                    {
                        Entity casePref = new Entity("jarvis_casepreferredagent", Guid.NewGuid());
                        casePref["jarvis_preferredvasoperator"] = new EntityReference("users", Guid.NewGuid());
                        result.Entities.Add(casePref);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_casemonitoraction'>"))
                    {
                        Entity caseMonitor = new Entity("jarvis_casemonitoraction", Guid.NewGuid());
                        caseMonitor["subject"] = "Test";
                        caseMonitor["prioritycode"] = new OptionSetValue(0);
                        caseMonitor["jarvis_monitorsortorder"] = 1;
                        caseMonitor["jarvis_followuptimestamp"] = DateTime.Now;
                        caseMonitor["jarvis_followuplanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
                        caseMonitor["jarvis_followupcountry"] = new EntityReference("jarvis_country", Guid.NewGuid());
                        caseMonitor["createdby"] = new EntityReference("systemusers", Guid.NewGuid());
                        result.Entities.Add(caseMonitor);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_monitorskill'>"))
                    {
                        Entity skill = new Entity("jarvis_monitorskill", Guid.NewGuid());
                        skill["jarvis_preferredvasoperator"] = new EntityReference("users", Guid.NewGuid());
                        result.Entities.Add(skill);
                    }

                    if ((query as FetchExpression).Query.Contains(" <entity name='jarvis_configurationjarvis'>"))
                    {
                        Entity config = new Entity("jarvis_configurationjarvis", Guid.NewGuid());
                        config[MCS.Jarvis.CE.Plugins.Constants.JarvisConfiguration.Automationreleasecase] = new OptionSetValue(1);
                        result.Entities.Add(config);
                    }
                }

                return result;
            };
            CaseMonitorSync plugin = new CaseMonitorSync();

            this.Service.ExecuteOrganizationRequest = (request) =>
            {
                LocalTimeFromUtcTimeResponse response = new LocalTimeFromUtcTimeResponse();
                return response;
            };
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Negative scenario for CaseMonitorSync Create Scenario.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidPluginExecutionException))]
        public void CaseMonitorTestPreCreateNoActiveCase()
        {
            var monitor = new Entity("incident");
            monitor.Id = Guid.NewGuid();
            monitor[Incident.caseOriginCode] = new OptionSetValue(2);
            monitor[Incident.casetypecode] = new OptionSetValue(2);
            monitor.Attributes[CaseMonitor.regardingobjectid] = new EntityReference("incident", monitor.Id);
            monitor["prioritycode"] = new OptionSetValue(0);
            monitor["actualstart"] = DateTime.Now;
            monitor["jarvis_followuptime"] = "12:00";
            monitor["subject"] = "Test Test";
            monitor["createdon"] = DateTime.Now;

            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", monitor),
               };
            this.PluginExecutionContext.MessageNameGet = () => "Create";
            this.PluginExecutionContext.StageGet = () => 20;

            // Setting service retrieve.
            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "incident")
                {
                    Entity inc = new Entity("incident");
                    inc.Id = guid;
                    inc["casetypecode"] = new OptionSetValue(2);
                    inc["statecode"] = new OptionSetValue(1);
                    inc[Incident.caseStatus] = new OptionSetValue(1);
                    inc["isescalated"] = true;
                    inc["routecase"] = true;
                    inc["ownerid"] = new EntityReference("users", Guid.NewGuid());
                    inc["jarvis_caselocation"] = new OptionSetValue(1);
                    inc["jarvis_caseserviceline"] = new EntityReference("jarivs_serviceline", Guid.NewGuid());
                    return inc;
                }

                return null;
            };
            CaseMonitorSync plugin = new CaseMonitorSync();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Positive scenario for CaseMonitorSync Post Create Scenario.
        /// </summary>
        [TestMethod]
        public void CaseMonitorTestPostCreate()
        {
            var monitor = new Entity("incident");
            monitor.Id = Guid.NewGuid();
            monitor[Incident.caseOriginCode] = new OptionSetValue(2);
            monitor[Incident.casetypecode] = new OptionSetValue(2);
            monitor.Attributes[CaseMonitor.regardingobjectid] = new EntityReference("incident", monitor.Id);
            monitor["prioritycode"] = new OptionSetValue(2);
            monitor["actualstart"] = DateTime.Now;
            monitor["createdon"] = DateTime.Now;

            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", monitor),
               };
            this.PluginExecutionContext.MessageNameGet = () => "Create";
            this.PluginExecutionContext.StageGet = () => 40;
            this.PluginExecutionContext.PostEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PostImage", monitor },
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
                    inc["statecode"] = new OptionSetValue(0);
                    inc[Incident.caseStatus] = new OptionSetValue(70);
                    inc["isescalated"] = true;
                    inc["routecase"] = true;
                    inc["ownerid"] = new EntityReference("users", Guid.NewGuid());
                    inc["jarvis_caselocation"] = new OptionSetValue(1);
                    inc["jarvis_caseserviceline"] = new EntityReference("jarivs_serviceline", Guid.NewGuid());
                    return inc;
                }

                return null;
            };

            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();

                if (query.GetType().Name == "FetchExpression")
                {
                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_casemonitoraction'>"))
                    {
                        Entity caseMonitor = new Entity("jarvis_casemonitoraction", Guid.NewGuid());
                        caseMonitor["subject"] = "Test";
                        caseMonitor["prioritycode"] = new OptionSetValue(1);
                        caseMonitor["jarvis_monitorsortorder"] = 1;
                        caseMonitor["jarvis_followuptimestamp"] = DateTime.Now;
                        caseMonitor["jarvis_followuplanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
                        caseMonitor["jarvis_followupcountry"] = new EntityReference("jarvis_country", Guid.NewGuid());
                        caseMonitor["createdby"] = new EntityReference("systemusers", Guid.NewGuid());
                        result.Entities.Add(caseMonitor);
                    }
                }

                return result;
            };
            CaseMonitorSync plugin = new CaseMonitorSync();

            this.Service.ExecuteOrganizationRequest = (request) =>
            {
                LocalTimeFromUtcTimeResponse response = new LocalTimeFromUtcTimeResponse();
                return response;
            };
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Positive scenario for CaseMonitorSync Delete Scenario.
        /// </summary>
        [TestMethod]
        public void CaseMonitorSyncTestDelete()
        {
            var preImg = new Entity("incident");
            preImg.Attributes[CaseMonitor.regardingobjectid] = new EntityReference("incident", Guid.NewGuid());

            // Set the input parameters
            var incident = new EntityReference("casemonitor", Guid.NewGuid());

            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", incident),
               };
            this.PluginExecutionContext.MessageNameGet = () => "Delete";
            this.PluginExecutionContext.StageGet = () => 20;

            this.PluginExecutionContext.PreEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PreImage", preImg },
                };
            };

            CaseMonitorSync plugin = new CaseMonitorSync();

            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Positive scenario for CaseMonitorSync Update Scenario.
        /// </summary>
        [TestMethod]
        public void CaseMonitorSyncTestPreUpdate()
        {
            var incident = new Entity("incident");
            incident.Id = Guid.NewGuid();
            incident[Incident.caseOriginCode] = new OptionSetValue(2);
            incident[Incident.casetypecode] = new OptionSetValue(2);
            incident.Attributes["subject"] = "test";
            incident.Attributes["prioritycode"] = new OptionSetValue(2);
            incident.Attributes["actualstart"] = DateTime.UtcNow;
            incident.Attributes["jarvis_followuptime"] = "11:22";
            var preImg = new Entity("incident");
            preImg.Attributes[CaseMonitor.regardingobjectid] = new EntityReference("incident", incident.Id);
            preImg.Attributes["subject"] = "test";
            preImg.Attributes["prioritycode"] = new OptionSetValue(2);
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", incident),
               };
            this.PluginExecutionContext.MessageNameGet = () => "Update";
            this.PluginExecutionContext.StageGet = () => 20;

            this.PluginExecutionContext.PreEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PreImage", preImg },
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

            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();
                if (query.GetType().Name == "QueryExpression")
                {
                    if ((query as QueryExpression).EntityName.Contains("usersettings"))
                    {
                        Entity usersetting = new Entity("usersettings");
                        usersetting["localeid"] = 105;
                        usersetting["timezonecode"] = 105;
                        usersetting.Id = Guid.NewGuid();
                        result.Entities.Add(usersetting);
                    }
                }

                return result;
            };

            CaseMonitorSync plugin = new CaseMonitorSync();

            this.Service.ExecuteOrganizationRequest = (request) =>
            {
                LocalTimeFromUtcTimeResponse response = new LocalTimeFromUtcTimeResponse();
                return response;
            };
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Positive scenario for CaseMonitorSync Update Scenario.
        /// </summary>
        [TestMethod]
        public void CaseMonitorSyncTestPreUpdateWithActualDate()
        {
            var incident = new Entity("incident");
            incident.Id = Guid.NewGuid();
            incident[Incident.caseOriginCode] = new OptionSetValue(2);
            incident[Incident.casetypecode] = new OptionSetValue(2);
            incident.Attributes["subject"] = "test";
            incident.Attributes["prioritycode"] = new OptionSetValue(2);
            var preImg = new Entity("incident");
            preImg.Attributes[CaseMonitor.regardingobjectid] = new EntityReference("incident", incident.Id);
            preImg.Attributes["subject"] = "test";
            preImg.Attributes["prioritycode"] = new OptionSetValue(2);
            preImg.Attributes["actualstart"] = DateTime.UtcNow;
            preImg.Attributes["jarvis_followuptime"] = "11:22";
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", incident),
               };
            this.PluginExecutionContext.MessageNameGet = () => "Update";
            this.PluginExecutionContext.StageGet = () => 20;

            this.PluginExecutionContext.PreEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PreImage", preImg },
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
            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();
                if (query.GetType().Name == "QueryExpression")
                {
                    if ((query as QueryExpression).EntityName.Contains("usersettings"))
                    {
                        Entity usersetting = new Entity("usersettings");
                        usersetting["localeid"] = 105;
                        usersetting["timezonecode"] = 105;
                        usersetting.Id = Guid.NewGuid();
                        result.Entities.Add(usersetting);
                    }
                }

                return result;
            };
            CaseMonitorSync plugin = new CaseMonitorSync();

            this.Service.ExecuteOrganizationRequest = (request) =>
            {
                LocalTimeFromUtcTimeResponse response = new LocalTimeFromUtcTimeResponse();
                return response;
            };
            plugin.Execute(this.ServiceProvider);
        }
    }
}