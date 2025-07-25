// <copyright file="SetMonitorActionToCaseTest.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace MCS.Jarvis.CE.UnitTest
{
    using System;
    using System.Collections.Generic;
    using MCS.Jarvis.CE.Plugins.Actions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;
    using static MCS.Jarvis.CE.BusinessProcessesShared.Helpers.Constants;

    /// <summary>
    /// Set MonitorAction To Case Test.
    /// </summary>
    [TestClass]
    public class SetMonitorActionToCaseTest : UnitTestBase
    {
        /// <summary>
        /// Positive Scenario to Set Monitor Action to Case.
        /// </summary>
        [TestMethod]
        public void SetMonitorActionToCaseMethodCurrentHigh()
        {
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("IncidentId", Guid.NewGuid().ToString()),
               };

            EntityReference caseMonitorAction = new EntityReference("jarvis_casemonitoraction", Guid.NewGuid());

            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "incident")
                {
                    Entity inc = new Entity("incident");
                    inc.Id = guid;
                    inc["casetypecode"] = new OptionSetValue(2);
                    inc["statecode"] = new OptionSetValue(0);
                    inc[Incident.caseStatus] = new OptionSetValue(80);
                    inc["isescalated"] = true;
                    inc["routecase"] = true;
                    inc["ownerid"] = new EntityReference("users", Guid.NewGuid());
                    inc["jarvis_caselocation"] = new OptionSetValue(1);
                    inc["jarvis_caseserviceline"] = new EntityReference("jarivs_serviceline", Guid.NewGuid());
                    inc["jarvis_fulinknew"] = caseMonitorAction;
                    return inc;
                }

                return null;
            };

            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();

                if (query.GetType().Name == "FetchExpression")
                {
                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_country'>"))
                    {
                        Entity country = new Entity("jarvis_country", Guid.NewGuid());
                        country["jarvis_name"] = "YY";
                        country["jarvis_iso2countrycode"] = "YY";
                        result.Entities.Add(country);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_language'>"))
                    {
                        Entity country = new Entity("jarvis_language", Guid.NewGuid());
                        country["jarvis_name"] = "English";
                        result.Entities.Add(country);
                    }

                    if ((query as FetchExpression).Query.Contains("<condition attribute='jarvis_monitorcurrentupcoming' operator='like' value='%Current%' />"))
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


                    if ((query as FetchExpression).Query.Contains("<entity name='incident'>"))
                    {
                        Entity inc = new Entity("jarvis_integrationconfiguration", Guid.NewGuid());
                        inc["jarvis_preferredvasoperator"] = "English";
                        inc["title"] = "English";
                        inc[CaseNotification.Case2AccountAccountid] = new AliasedValue("accounts", "name", Guid.NewGuid());
                        inc[CaseNotification.Case2AccountName] = new AliasedValue("accounts", "name", "Test");

                        result.Entities.Add(inc);
                    }
                }

                return result;
            };
            SetMonitorActionToCase plugin = new SetMonitorActionToCase();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Positive Scenario to Set Monitor Action to Case.
        /// </summary>
        [TestMethod]
        public void SetMonitorActionToCaseMethodUpcomingMedium()
        {
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("IncidentId", Guid.NewGuid().ToString()),
               };

            EntityReference caseMonitorAction = new EntityReference("jarvis_casemonitoraction", Guid.NewGuid());

            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "incident")
                {
                    Entity inc = new Entity("incident");
                    inc.Id = guid;
                    inc["casetypecode"] = new OptionSetValue(2);
                    inc["statecode"] = new OptionSetValue(0);
                    inc[Incident.caseStatus] = new OptionSetValue(80);
                    inc["isescalated"] = true;
                    inc["routecase"] = true;
                    inc["ownerid"] = new EntityReference("users", Guid.NewGuid());
                    inc["jarvis_caselocation"] = new OptionSetValue(1);
                    inc["jarvis_caseserviceline"] = new EntityReference("jarivs_serviceline", Guid.NewGuid());
                    inc["jarvis_fulinknew"] = caseMonitorAction;
                    return inc;
                }

                return null;
            };

            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();

                if (query.GetType().Name == "FetchExpression")
                {
                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_country'>"))
                    {
                        Entity country = new Entity("jarvis_country", Guid.NewGuid());
                        country["jarvis_name"] = "YY";
                        country["jarvis_iso2countrycode"] = "YY";
                        result.Entities.Add(country);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_language'>"))
                    {
                        Entity country = new Entity("jarvis_language", Guid.NewGuid());
                        country["jarvis_name"] = "English";
                        result.Entities.Add(country);
                    }

                    if ((query as FetchExpression).Query.Contains("<condition attribute='jarvis_monitorcurrentupcoming' operator='like' value='%Upcoming%' />"))
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

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_monitorskill'>"))
                    {
                        Entity skill = new Entity("jarvis_monitorskill", Guid.NewGuid());
                        skill["jarvis_preferredvasoperator"] = new EntityReference("users", Guid.NewGuid());
                        result.Entities.Add(skill);
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
                }

                return result;
            };
            SetMonitorActionToCase plugin = new SetMonitorActionToCase();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Positive Scenario to Set Monitor Action to Case.
        /// </summary>
        [TestMethod]
        public void SetMonitorActionToCaseMethodUpcomingStd()
        {
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("IncidentId", Guid.NewGuid().ToString()),
               };

            EntityReference caseMonitorAction = new EntityReference("jarvis_casemonitoraction", Guid.NewGuid());

            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "incident")
                {
                    Entity inc = new Entity("incident");
                    inc.Id = guid;
                    inc["casetypecode"] = new OptionSetValue(2);
                    inc["statecode"] = new OptionSetValue(0);
                    inc[Incident.caseStatus] = new OptionSetValue(80);
                    inc["isescalated"] = true;
                    inc["routecase"] = true;
                    inc["ownerid"] = new EntityReference("users", Guid.NewGuid());
                    inc["jarvis_caselocation"] = new OptionSetValue(1);
                    inc["jarvis_caseserviceline"] = new EntityReference("jarivs_serviceline", Guid.NewGuid());
                    inc["jarvis_fulinknew"] = caseMonitorAction;
                    return inc;
                }

                return null;
            };

            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();

                if (query.GetType().Name == "FetchExpression")
                {
                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_country'>"))
                    {
                        Entity country = new Entity("jarvis_country", Guid.NewGuid());
                        country["jarvis_name"] = "YY";
                        country["jarvis_iso2countrycode"] = "YY";
                        result.Entities.Add(country);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_language'>"))
                    {
                        Entity country = new Entity("jarvis_language", Guid.NewGuid());
                        country["jarvis_name"] = "English";
                        result.Entities.Add(country);
                    }

                    if ((query as FetchExpression).Query.Contains("<condition attribute='jarvis_monitorcurrentupcoming' operator='like' value='%Upcoming%' />"))
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

                    if ((query as FetchExpression).Query.Contains("<entity name='incident'>"))
                    {
                        Entity inc = new Entity("jarvis_integrationconfiguration", Guid.NewGuid());
                        inc["jarvis_preferredvasoperator"] = "English";
                        inc["title"] = "English";
                        inc[CaseNotification.Case2AccountAccountid] = new AliasedValue("accounts", "name", Guid.NewGuid());
                        inc[CaseNotification.Case2AccountName] = new AliasedValue("accounts", "name", "Test");

                        result.Entities.Add(inc);
                    }
                }

                return result;
            };
            SetMonitorActionToCase plugin = new SetMonitorActionToCase();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Positive Scenario to Set Monitor Action to Case.
        /// </summary>
        [TestMethod]
        public void SetMonitorActionToCaseMethodCloseCase()
        {
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("IncidentId", Guid.NewGuid().ToString()),
               };

            EntityReference caseMonitorAction = new EntityReference("jarvis_casemonitoraction", Guid.NewGuid());

            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "incident")
                {
                    Entity inc = new Entity("incident");
                    inc.Id = guid;
                    inc["casetypecode"] = new OptionSetValue(2);
                    inc["statecode"] = new OptionSetValue(0);
                    inc[Incident.caseStatus] = new OptionSetValue(90);
                    inc["isescalated"] = true;
                    inc["routecase"] = true;
                    inc["ownerid"] = new EntityReference("users", Guid.NewGuid());
                    inc["jarvis_caselocation"] = new OptionSetValue(1);
                    inc["jarvis_caseserviceline"] = new EntityReference("jarivs_serviceline", Guid.NewGuid());
                    inc["jarvis_fulinknew"] = caseMonitorAction;
                    return inc;
                }

                return null;
            };

            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();

                if (query.GetType().Name == "FetchExpression")
                {
                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_country'>"))
                    {
                        Entity country = new Entity("jarvis_country", Guid.NewGuid());
                        country["jarvis_name"] = "Country Not Found";
                        country["jarvis_iso2countrycode"] = "YY";
                        result.Entities.Add(country);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_language'>"))
                    {
                        Entity country = new Entity("jarvis_language", Guid.NewGuid());
                        country["jarvis_name"] = "Dummy Language";
                        result.Entities.Add(country);
                    }

                }

                return result;
            };
            SetMonitorActionToCase plugin = new SetMonitorActionToCase();
            plugin.Execute(this.ServiceProvider);
        }
    }
}
