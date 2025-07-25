// <copyright file="ForcedTranslationActionTest.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace MCS.Jarvis.CE.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Controls;
    using MCS.Jarvis.CE.Plugins;
    using MCS.Jarvis.CE.Plugins.Actions;
    using Microsoft.Crm.Sdk.Messages;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;
    using static MCS.Jarvis.CE.BusinessProcessesShared.Helpers.Constants;

    /// <summary>
    /// Forced Translation Action Test.
    /// </summary>
    [TestClass]
    public class ForcedTranslationActionTest : UnitTestBase
    {
        /// <summary>
        /// Forced Translation Op1.
        /// </summary>
        [TestMethod]
        public void ForcedTranslationOp1Case()
        {
            var incident = new Entity("incident");
            incident.Id = Guid.NewGuid();
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Operation", 1),
                   new KeyValuePair<string, object>("EntityName", "CASE"),
                   new KeyValuePair<string, object>("IncidentId", Guid.NewGuid().ToString()),
                   new KeyValuePair<string, object>("TargetEntityId", Guid.NewGuid().ToString()),
                   new KeyValuePair<string, object>("TriggeredById", Guid.NewGuid().ToString()),
               };
            this.PluginExecutionContext.OutputParametersGet = () => new ParameterCollection();

            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "incident")
                {
                    Entity inc = new Entity("incident");
                    inc.Id = guid;
                    inc["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
                    inc["customerid"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    return inc;
                }

                if (entityName == "account")
                {
                    Entity account = new Entity("account");
                    account.Id = guid;
                    account["jarvis_language"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    return account;
                }

                if (entityName == "jarvis_language")
                {
                    Entity language = new Entity("jarvis_language");
                    language.Id = guid;
                    language["jarvis_iso2languagecode6391"] = "FRE";
                    return language;
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
                        Entity configuration = new Entity("jarvis_configurationjarvis");
                        configuration["jarvis_automationtranslation"] = true;
                        result.Entities.Add(configuration);
                    }
                }

                return result;
            };

            ForcedTranslationAction plugin = new ForcedTranslationAction();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Forced Translation Op1.
        /// </summary>
        [TestMethod]
        public void ForcedTranslationOp1GOP()
        {
            var incident = new Entity("incident");
            incident.Id = Guid.NewGuid();
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Operation", 1),
                   new KeyValuePair<string, object>("EntityName", "GOP"),
                   new KeyValuePair<string, object>("IncidentId", Guid.NewGuid().ToString()),
                   new KeyValuePair<string, object>("TargetEntityId", Guid.NewGuid().ToString()),
                   new KeyValuePair<string, object>("TriggeredById", Guid.NewGuid().ToString()),
               };

            this.PluginExecutionContext.OutputParametersGet = () => new ParameterCollection();

            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "incident")
                {
                    Entity inc = new Entity("incident");
                    inc.Id = guid;
                    inc["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
                    inc["customerid"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    return inc;
                }

                if (entityName == "account")
                {
                    Entity account = new Entity("account");
                    account.Id = guid;
                    account["jarvis_language"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    return account;
                }

                if (entityName == "jarvis_language")
                {
                    Entity language = new Entity("jarvis_language");
                    language.Id = guid;
                    language["jarvis_iso2languagecode6391"] = "FRE";
                    return language;
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
                        Entity configuration = new Entity("jarvis_configurationjarvis");
                        configuration["jarvis_automationtranslation"] = true;
                        result.Entities.Add(configuration);
                    }
                }

                return result;
            };

            ForcedTranslationAction plugin = new ForcedTranslationAction();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Forced Translation Op1.
        /// </summary>
        [TestMethod]
        public void ForcedTranslationOp1PO()
        {
            var incident = new Entity("incident");
            incident.Id = Guid.NewGuid();
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Operation", 1),
                   new KeyValuePair<string, object>("EntityName", "PASSOUT"),
                   new KeyValuePair<string, object>("IncidentId", Guid.NewGuid().ToString()),
                   new KeyValuePair<string, object>("TargetEntityId", Guid.NewGuid().ToString()),
                   new KeyValuePair<string, object>("TriggeredById", Guid.NewGuid().ToString()),
               };
            this.PluginExecutionContext.OutputParametersGet = () => new ParameterCollection();

            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "incident")
                {
                    Entity inc = new Entity("incident");
                    inc.Id = guid;
                    inc["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
                    inc["customerid"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    return inc;
                }

                if (entityName == "account")
                {
                    Entity account = new Entity("account");
                    account.Id = guid;
                    account["jarvis_language"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    return account;
                }

                if (entityName == "jarvis_language")
                {
                    Entity language = new Entity("jarvis_language");
                    language.Id = guid;
                    language["jarvis_iso2languagecode6391"] = "FRE";
                    return language;
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
                        Entity configuration = new Entity("jarvis_configurationjarvis");
                        configuration["jarvis_automationtranslation"] = true;
                        result.Entities.Add(configuration);
                    }
                }

                return result;
            };

            ForcedTranslationAction plugin = new ForcedTranslationAction();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Forced Translation Op1.
        /// </summary>
        [TestMethod]
        public void ForcedTranslationOp1JED()
        {
            var incident = new Entity("incident");
            incident.Id = Guid.NewGuid();
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Operation", 1),
                   new KeyValuePair<string, object>("EntityName", "JED"),
                   new KeyValuePair<string, object>("IncidentId", Guid.NewGuid().ToString()),
                   new KeyValuePair<string, object>("TargetEntityId", Guid.NewGuid().ToString()),
                   new KeyValuePair<string, object>("TriggeredById", Guid.NewGuid().ToString()),
               };
            this.PluginExecutionContext.OutputParametersGet = () => new ParameterCollection();

            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "incident")
                {
                    Entity inc = new Entity("incident");
                    inc.Id = guid;
                    inc["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
                    inc["customerid"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    return inc;
                }

                if (entityName == "account")
                {
                    Entity account = new Entity("account");
                    account.Id = guid;
                    account["jarvis_language"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    return account;
                }

                if (entityName == "jarvis_language")
                {
                    Entity language = new Entity("jarvis_language");
                    language.Id = guid;
                    language["jarvis_iso2languagecode6391"] = "FRE";
                    return language;
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
                        Entity configuration = new Entity("jarvis_configurationjarvis");
                        configuration["jarvis_automationtranslation"] = true;
                        result.Entities.Add(configuration);
                    }
                }

                return result;
            };

            ForcedTranslationAction plugin = new ForcedTranslationAction();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Forced Translation Op1.
        /// </summary>
        [TestMethod]
        public void ForcedTranslationOp1Repair()
        {
            var incident = new Entity("incident");
            incident.Id = Guid.NewGuid();
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Operation", 1),
                   new KeyValuePair<string, object>("EntityName", "REPAIR INFO"),
                   new KeyValuePair<string, object>("IncidentId", Guid.NewGuid().ToString()),
                   new KeyValuePair<string, object>("TargetEntityId", Guid.NewGuid().ToString()),
                   new KeyValuePair<string, object>("TriggeredById", Guid.NewGuid().ToString()),
               };

            this.PluginExecutionContext.OutputParametersGet = () => new ParameterCollection();

            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "incident")
                {
                    Entity inc = new Entity("incident");
                    inc.Id = guid;
                    inc["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
                    inc["customerid"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    return inc;
                }

                if (entityName == "account")
                {
                    Entity account = new Entity("account");
                    account.Id = guid;
                    account["jarvis_language"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    return account;
                }

                if (entityName == "jarvis_language")
                {
                    Entity language = new Entity("jarvis_language");
                    language.Id = guid;
                    language["jarvis_iso2languagecode6391"] = "FRE";
                    return language;
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
                        Entity configuration = new Entity("jarvis_configurationjarvis");
                        configuration["jarvis_automationtranslation"] = true;
                        result.Entities.Add(configuration);
                    }
                }

                return result;
            };

            ForcedTranslationAction plugin = new ForcedTranslationAction();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Forced Translation Op2.
        /// </summary>
        [TestMethod]
        public void ForcedTranslationOp2Case()
        {
            var incident = new Entity("incident");
            incident.Id = Guid.NewGuid();
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Operation", 2),
                   new KeyValuePair<string, object>("EntityName", "CASE"),
                   new KeyValuePair<string, object>("IncidentId", Guid.NewGuid().ToString()),
                   new KeyValuePair<string, object>("TargetEntityId", Guid.NewGuid().ToString()),
                   new KeyValuePair<string, object>("TriggeredById", Guid.NewGuid().ToString()),
               };

            this.PluginExecutionContext.OutputParametersGet = () => new ParameterCollection();

            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "incident")
                {
                    Entity inc = new Entity("incident");
                    inc.Id = guid;
                    inc["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
                    inc["customerid"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    return inc;
                }

                if (entityName == "account")
                {
                    Entity account = new Entity("account");
                    account.Id = guid;
                    account["jarvis_language"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    return account;
                }

                if (entityName == "jarvis_language")
                {
                    Entity language = new Entity("jarvis_language");
                    language.Id = guid;
                    language["jarvis_iso2languagecode6391"] = "FRE";
                    return language;
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
                        Entity configuration = new Entity("jarvis_configurationjarvis");
                        configuration["jarvis_automationtranslation"] = true;
                        result.Entities.Add(configuration);
                    }
                }

                return result;
            };

            ForcedTranslationAction plugin = new ForcedTranslationAction();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Forced Translation Op2.
        /// </summary>
        [TestMethod]
        public void ForcedTranslationOp2CaseGOP()
        {
            var incident = new Entity("incident");
            incident.Id = Guid.NewGuid();
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Operation", 2),
                   new KeyValuePair<string, object>("EntityName", "CASE.GOP"),
                   new KeyValuePair<string, object>("IncidentId", Guid.NewGuid().ToString()),
                   new KeyValuePair<string, object>("TargetEntityId", Guid.NewGuid().ToString()),
                   new KeyValuePair<string, object>("TriggeredById", Guid.NewGuid().ToString()),
               };

            this.PluginExecutionContext.OutputParametersGet = () => new ParameterCollection();

            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "incident")
                {
                    Entity inc = new Entity("incident");
                    inc.Id = guid;
                    inc["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
                    inc["customerid"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    return inc;
                }

                if (entityName == "account")
                {
                    Entity account = new Entity("account");
                    account.Id = guid;
                    account["jarvis_language"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    return account;
                }

                if (entityName == "jarvis_language")
                {
                    Entity language = new Entity("jarvis_language");
                    language.Id = guid;
                    language["jarvis_iso2languagecode6391"] = "FRE";
                    return language;
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
                        Entity configuration = new Entity("jarvis_configurationjarvis");
                        configuration["jarvis_automationtranslation"] = true;
                        result.Entities.Add(configuration);
                    }
                }

                return result;
            };

            ForcedTranslationAction plugin = new ForcedTranslationAction();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Forced Translation Op2.
        /// </summary>
        [TestMethod]
        public void ForcedTranslationOp2CasePO()
        {
            var incident = new Entity("incident");
            incident.Id = Guid.NewGuid();
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Operation", 2),
                   new KeyValuePair<string, object>("EntityName", "CASE.PASSOUT"),
                   new KeyValuePair<string, object>("IncidentId", Guid.NewGuid().ToString()),
                   new KeyValuePair<string, object>("TargetEntityId", Guid.NewGuid().ToString()),
                   new KeyValuePair<string, object>("TriggeredById", Guid.NewGuid().ToString()),
               };

            this.PluginExecutionContext.OutputParametersGet = () => new ParameterCollection();

            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "incident")
                {
                    Entity inc = new Entity("incident");
                    inc.Id = guid;
                    inc["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
                    inc["customerid"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    return inc;
                }

                if (entityName == "account")
                {
                    Entity account = new Entity("account");
                    account.Id = guid;
                    account["jarvis_language"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    return account;
                }

                if (entityName == "jarvis_language")
                {
                    Entity language = new Entity("jarvis_language");
                    language.Id = guid;
                    language["jarvis_iso2languagecode6391"] = "FRE";
                    return language;
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
                        Entity configuration = new Entity("jarvis_configurationjarvis");
                        configuration["jarvis_automationtranslation"] = true;
                        result.Entities.Add(configuration);
                    }
                }

                return result;
            };

            ForcedTranslationAction plugin = new ForcedTranslationAction();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Forced Translation Op2.
        /// </summary>
        [TestMethod]
        public void ForcedTranslationOp2CaseJED()
        {
            var incident = new Entity("incident");
            incident.Id = Guid.NewGuid();
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Operation", 2),
                   new KeyValuePair<string, object>("EntityName", "CASE.JED"),
                   new KeyValuePair<string, object>("IncidentId", Guid.NewGuid().ToString()),
                   new KeyValuePair<string, object>("TargetEntityId", Guid.NewGuid().ToString()),
                   new KeyValuePair<string, object>("TriggeredById", Guid.NewGuid().ToString()),
               };

            this.PluginExecutionContext.OutputParametersGet = () => new ParameterCollection();

            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "incident")
                {
                    Entity inc = new Entity("incident");
                    inc.Id = guid;
                    inc["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
                    inc["customerid"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    return inc;
                }

                if (entityName == "account")
                {
                    Entity account = new Entity("account");
                    account.Id = guid;
                    account["jarvis_language"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    return account;
                }

                if (entityName == "jarvis_language")
                {
                    Entity language = new Entity("jarvis_language");
                    language.Id = guid;
                    language["jarvis_iso2languagecode6391"] = "FRE";
                    return language;
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
                        Entity configuration = new Entity("jarvis_configurationjarvis");
                        configuration["jarvis_automationtranslation"] = true;
                        result.Entities.Add(configuration);
                    }
                }

                return result;
            };

            ForcedTranslationAction plugin = new ForcedTranslationAction();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Forced Translation Op2.
        /// </summary>
        [TestMethod]
        public void ForcedTranslationOp2CaseRepair()
        {
            var incident = new Entity("incident");
            incident.Id = Guid.NewGuid();
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Operation", 2),
                   new KeyValuePair<string, object>("EntityName", "CASE.REPAIR INFO"),
                   new KeyValuePair<string, object>("IncidentId", Guid.NewGuid().ToString()),
                   new KeyValuePair<string, object>("TargetEntityId", Guid.NewGuid().ToString()),
                   new KeyValuePair<string, object>("TriggeredById", Guid.NewGuid().ToString()),
               };

            this.PluginExecutionContext.OutputParametersGet = () => new ParameterCollection();

            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "incident")
                {
                    Entity inc = new Entity("incident");
                    inc.Id = guid;
                    inc["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
                    inc["customerid"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    return inc;
                }

                if (entityName == "account")
                {
                    Entity account = new Entity("account");
                    account.Id = guid;
                    account["jarvis_language"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    return account;
                }

                if (entityName == "jarvis_language")
                {
                    Entity language = new Entity("jarvis_language");
                    language.Id = guid;
                    language["jarvis_iso2languagecode6391"] = "FRE";
                    return language;
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
                        Entity configuration = new Entity("jarvis_configurationjarvis");
                        configuration["jarvis_automationtranslation"] = true;
                        result.Entities.Add(configuration);
                    }
                }

                return result;
            };

            ForcedTranslationAction plugin = new ForcedTranslationAction();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Forced Translation Op2.
        /// </summary>
        [TestMethod]
        public void ForcedTranslationOp2GOP()
        {
            var incident = new Entity("incident");
            incident.Id = Guid.NewGuid();
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Operation", 2),
                   new KeyValuePair<string, object>("EntityName", "GOP"),
                   new KeyValuePair<string, object>("IncidentId", Guid.NewGuid().ToString()),
                   new KeyValuePair<string, object>("TargetEntityId", Guid.NewGuid().ToString()),
                   new KeyValuePair<string, object>("TriggeredById", Guid.NewGuid().ToString()),
               };

            this.PluginExecutionContext.OutputParametersGet = () => new ParameterCollection();

            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "incident")
                {
                    Entity inc = new Entity("incident");
                    inc.Id = guid;
                    inc["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
                    inc["customerid"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    return inc;
                }

                if (entityName == "account")
                {
                    Entity account = new Entity("account");
                    account.Id = guid;
                    account["jarvis_language"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    return account;
                }

                if (entityName == "jarvis_language")
                {
                    Entity language = new Entity("jarvis_language");
                    language.Id = guid;
                    language["jarvis_iso2languagecode6391"] = "FRE";
                    return language;
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
                        Entity configuration = new Entity("jarvis_configurationjarvis");
                        configuration["jarvis_automationtranslation"] = true;
                        result.Entities.Add(configuration);
                    }
                }

                return result;
            };

            ForcedTranslationAction plugin = new ForcedTranslationAction();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Forced Translation Op2.
        /// </summary>
        [TestMethod]
        public void ForcedTranslationOp2PO()
        {
            var incident = new Entity("incident");
            incident.Id = Guid.NewGuid();
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Operation", 2),
                   new KeyValuePair<string, object>("EntityName", "PASSOUT"),
                   new KeyValuePair<string, object>("IncidentId", Guid.NewGuid().ToString()),
                   new KeyValuePair<string, object>("TargetEntityId", Guid.NewGuid().ToString()),
                   new KeyValuePair<string, object>("TriggeredById", Guid.NewGuid().ToString()),
               };

            this.PluginExecutionContext.OutputParametersGet = () => new ParameterCollection();

            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "incident")
                {
                    Entity inc = new Entity("incident");
                    inc.Id = guid;
                    inc["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
                    inc["customerid"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    return inc;
                }

                if (entityName == "account")
                {
                    Entity account = new Entity("account");
                    account.Id = guid;
                    account["jarvis_language"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    return account;
                }

                if (entityName == "jarvis_language")
                {
                    Entity language = new Entity("jarvis_language");
                    language.Id = guid;
                    language["jarvis_iso2languagecode6391"] = "FRE";
                    return language;
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
                        Entity configuration = new Entity("jarvis_configurationjarvis");
                        configuration["jarvis_automationtranslation"] = true;
                        result.Entities.Add(configuration);
                    }
                }

                return result;
            };

            ForcedTranslationAction plugin = new ForcedTranslationAction();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Forced Translation Op2.
        /// </summary>
        [TestMethod]
        public void ForcedTranslationOp2JED()
        {
            var incident = new Entity("incident");
            incident.Id = Guid.NewGuid();
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Operation", 2),
                   new KeyValuePair<string, object>("EntityName", "JED"),
                   new KeyValuePair<string, object>("IncidentId", Guid.NewGuid().ToString()),
                   new KeyValuePair<string, object>("TargetEntityId", Guid.NewGuid().ToString()),
                   new KeyValuePair<string, object>("TriggeredById", Guid.NewGuid().ToString()),
               };

            this.PluginExecutionContext.OutputParametersGet = () => new ParameterCollection();

            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "incident")
                {
                    Entity inc = new Entity("incident");
                    inc.Id = guid;
                    inc["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
                    inc["customerid"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    return inc;
                }

                if (entityName == "account")
                {
                    Entity account = new Entity("account");
                    account.Id = guid;
                    account["jarvis_language"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    return account;
                }

                if (entityName == "jarvis_language")
                {
                    Entity language = new Entity("jarvis_language");
                    language.Id = guid;
                    language["jarvis_iso2languagecode6391"] = "FRE";
                    return language;
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
                        Entity configuration = new Entity("jarvis_configurationjarvis");
                        configuration["jarvis_automationtranslation"] = true;
                        result.Entities.Add(configuration);
                    }
                }

                return result;
            };

            ForcedTranslationAction plugin = new ForcedTranslationAction();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Forced Translation Op3.
        /// </summary>
        [TestMethod]
        public void ForcedTranslationOp3Case()
        {
            var incident = new Entity("incident");
            incident.Id = Guid.NewGuid();
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Operation", 3),
                   new KeyValuePair<string, object>("EntityName", "CASE"),
                   new KeyValuePair<string, object>("IncidentId", Guid.NewGuid().ToString()),
                   new KeyValuePair<string, object>("TargetEntityId", Guid.NewGuid().ToString()),
                   new KeyValuePair<string, object>("TriggeredById", Guid.NewGuid().ToString()),
               };

            this.PluginExecutionContext.OutputParametersGet = () => new ParameterCollection();
            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "incident")
                {
                    Entity inc = new Entity("incident");
                    inc.Id = guid;
                    inc["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
                    inc["customerid"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    return inc;
                }

                if (entityName == "account")
                {
                    Entity account = new Entity("account");
                    account.Id = guid;
                    account["jarvis_language"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    return account;
                }

                if (entityName == "jarvis_language")
                {
                    Entity language = new Entity("jarvis_language");
                    language.Id = guid;
                    language["jarvis_iso2languagecode6391"] = "FRE";
                    return language;
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
                        Entity configuration = new Entity("jarvis_configurationjarvis");
                        configuration["jarvis_automationtranslation"] = true;
                        result.Entities.Add(configuration);
                    }
                }

                return result;
            };

            ForcedTranslationAction plugin = new ForcedTranslationAction();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Forced Translation Op3.
        /// </summary>
        [TestMethod]
        public void ForcedTranslationOp3GOP()
        {
            var incident = new Entity("incident");
            incident.Id = Guid.NewGuid();
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Operation", 3),
                   new KeyValuePair<string, object>("EntityName", "GOP"),
                   new KeyValuePair<string, object>("IncidentId", Guid.NewGuid().ToString()),
                   new KeyValuePair<string, object>("TargetEntityId", Guid.NewGuid().ToString()),
                   new KeyValuePair<string, object>("TriggeredById", Guid.NewGuid().ToString()),
               };

            this.PluginExecutionContext.OutputParametersGet = () => new ParameterCollection();
            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "incident")
                {
                    Entity inc = new Entity("incident");
                    inc.Id = guid;
                    inc["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
                    inc["customerid"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    return inc;
                }

                if (entityName == "account")
                {
                    Entity account = new Entity("account");
                    account.Id = guid;
                    account["jarvis_language"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    return account;
                }

                if (entityName == "jarvis_language")
                {
                    Entity language = new Entity("jarvis_language");
                    language.Id = guid;
                    language["jarvis_iso2languagecode6391"] = "FRE";
                    return language;
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
                        Entity configuration = new Entity("jarvis_configurationjarvis");
                        configuration["jarvis_automationtranslation"] = true;
                        result.Entities.Add(configuration);
                    }
                }

                return result;
            };

            ForcedTranslationAction plugin = new ForcedTranslationAction();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Forced Translation Op3.
        /// </summary>
        [TestMethod]
        public void ForcedTranslationOp3PO()
        {
            var incident = new Entity("incident");
            incident.Id = Guid.NewGuid();
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Operation", 3),
                   new KeyValuePair<string, object>("EntityName", "PASSOUT"),
                   new KeyValuePair<string, object>("IncidentId", Guid.NewGuid().ToString()),
                   new KeyValuePair<string, object>("TargetEntityId", Guid.NewGuid().ToString()),
                   new KeyValuePair<string, object>("TriggeredById", Guid.NewGuid().ToString()),
               };

            this.PluginExecutionContext.OutputParametersGet = () => new ParameterCollection();
            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "incident")
                {
                    Entity inc = new Entity("incident");
                    inc.Id = guid;
                    inc["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
                    inc["customerid"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    return inc;
                }

                if (entityName == "account")
                {
                    Entity account = new Entity("account");
                    account.Id = guid;
                    account["jarvis_language"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    return account;
                }

                if (entityName == "jarvis_language")
                {
                    Entity language = new Entity("jarvis_language");
                    language.Id = guid;
                    language["jarvis_iso2languagecode6391"] = "FRE";
                    return language;
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
                        Entity configuration = new Entity("jarvis_configurationjarvis");
                        configuration["jarvis_automationtranslation"] = true;
                        result.Entities.Add(configuration);
                    }
                }

                return result;
            };

            ForcedTranslationAction plugin = new ForcedTranslationAction();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Forced Translation Op3.
        /// </summary>
        [TestMethod]
        public void ForcedTranslationOp3Repair()
        {
            var incident = new Entity("incident");
            incident.Id = Guid.NewGuid();
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Operation", 3),
                   new KeyValuePair<string, object>("EntityName", "REPAIR INFO"),
                   new KeyValuePair<string, object>("IncidentId", Guid.NewGuid().ToString()),
                   new KeyValuePair<string, object>("TargetEntityId", Guid.NewGuid().ToString()),
                   new KeyValuePair<string, object>("TriggeredById", Guid.NewGuid().ToString()),
               };

            this.PluginExecutionContext.OutputParametersGet = () => new ParameterCollection();
            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "incident")
                {
                    Entity inc = new Entity("incident");
                    inc.Id = guid;
                    inc["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
                    inc["customerid"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    return inc;
                }

                if (entityName == "account")
                {
                    Entity account = new Entity("account");
                    account.Id = guid;
                    account["jarvis_language"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    return account;
                }

                if (entityName == "jarvis_language")
                {
                    Entity language = new Entity("jarvis_language");
                    language.Id = guid;
                    language["jarvis_iso2languagecode6391"] = "FRE";
                    return language;
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
                        Entity configuration = new Entity("jarvis_configurationjarvis");
                        configuration["jarvis_automationtranslation"] = true;
                        result.Entities.Add(configuration);
                    }
                }

                return result;
            };

            ForcedTranslationAction plugin = new ForcedTranslationAction();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Forced Translation Op3.
        /// </summary>
        [TestMethod]
        public void ForcedTranslationOp3JED()
        {
            var incident = new Entity("incident");
            incident.Id = Guid.NewGuid();
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Operation", 3),
                   new KeyValuePair<string, object>("EntityName", "JED"),
                   new KeyValuePair<string, object>("IncidentId", Guid.NewGuid().ToString()),
                   new KeyValuePair<string, object>("TargetEntityId", Guid.NewGuid().ToString()),
                   new KeyValuePair<string, object>("TriggeredById", Guid.NewGuid().ToString()),
               };

            this.PluginExecutionContext.OutputParametersGet = () => new ParameterCollection();
            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "incident")
                {
                    Entity inc = new Entity("incident");
                    inc.Id = guid;
                    inc["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
                    inc["customerid"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    return inc;
                }

                if (entityName == "account")
                {
                    Entity account = new Entity("account");
                    account.Id = guid;
                    account["jarvis_language"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    return account;
                }

                if (entityName == "jarvis_language")
                {
                    Entity language = new Entity("jarvis_language");
                    language.Id = guid;
                    language["jarvis_iso2languagecode6391"] = "FRE";
                    return language;
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
                        Entity configuration = new Entity("jarvis_configurationjarvis");
                        configuration["jarvis_automationtranslation"] = true;
                        result.Entities.Add(configuration);
                    }
                }

                return result;
            };

            ForcedTranslationAction plugin = new ForcedTranslationAction();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Forced Translation Op4.
        /// </summary>
        [TestMethod]
        public void ForcedTranslationOp4Case()
        {
            var incident = new Entity("incident");
            incident.Id = Guid.NewGuid();
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Operation", 4),
                   new KeyValuePair<string, object>("EntityName", "CASE"),
                   new KeyValuePair<string, object>("IncidentId", Guid.NewGuid().ToString()),
                   new KeyValuePair<string, object>("TargetEntityId", Guid.NewGuid().ToString()),
                   new KeyValuePair<string, object>("TriggeredById", Guid.NewGuid().ToString()),
               };
            this.PluginExecutionContext.OutputParametersGet = () => new ParameterCollection();
            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "incident")
                {
                    Entity inc = new Entity("incident");
                    inc.Id = guid;
                    inc["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
                    inc["customerid"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    return inc;
                }

                if (entityName == "account")
                {
                    Entity account = new Entity("account");
                    account.Id = guid;
                    account["jarvis_language"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    return account;
                }

                if (entityName == "jarvis_language")
                {
                    Entity language = new Entity("jarvis_language");
                    language.Id = guid;
                    language["jarvis_iso2languagecode6391"] = "FRE";
                    return language;
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
                        Entity configuration = new Entity("jarvis_configurationjarvis");
                        configuration["jarvis_automationtranslation"] = true;
                        result.Entities.Add(configuration);
                    }
                }

                return result;
            };

            ForcedTranslationAction plugin = new ForcedTranslationAction();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Forced Translation Op5.
        /// </summary>
        [TestMethod]
        public void ForcedTranslationOp5Case()
        {
            var incident = new Entity("incident");
            incident.Id = Guid.NewGuid();
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Operation", 5),
                   new KeyValuePair<string, object>("EntityName", "CASE"),
                   new KeyValuePair<string, object>("IncidentId", Guid.NewGuid().ToString()),
                   new KeyValuePair<string, object>("TargetEntityId", Guid.NewGuid().ToString()),
                   new KeyValuePair<string, object>("TriggeredById", Guid.NewGuid().ToString()),
               };
            this.PluginExecutionContext.OutputParametersGet = () => new ParameterCollection();
            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "incident")
                {
                    Entity inc = new Entity("incident");
                    inc.Id = guid;
                    inc["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
                    inc["customerid"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    return inc;
                }

                if (entityName == "account")
                {
                    Entity account = new Entity("account");
                    account.Id = guid;
                    account["jarvis_language"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    return account;
                }

                if (entityName == "jarvis_language")
                {
                    Entity language = new Entity("jarvis_language");
                    language.Id = guid;
                    language["jarvis_iso2languagecode6391"] = "FRE";
                    return language;
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
                        Entity configuration = new Entity("jarvis_configurationjarvis");
                        configuration["jarvis_automationtranslation"] = true;
                        result.Entities.Add(configuration);
                    }
                }

                return result;
            };

            ForcedTranslationAction plugin = new ForcedTranslationAction();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Forced Translation Op5.
        /// </summary>
        [TestMethod]
        public void ForcedTranslationOp5GOP()
        {
            var incident = new Entity("incident");
            incident.Id = Guid.NewGuid();
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Operation", 5),
                   new KeyValuePair<string, object>("EntityName", "GOP"),
                   new KeyValuePair<string, object>("IncidentId", Guid.NewGuid().ToString()),
                   new KeyValuePair<string, object>("TargetEntityId", Guid.NewGuid().ToString()),
                   new KeyValuePair<string, object>("TriggeredById", Guid.NewGuid().ToString()),
               };
            this.PluginExecutionContext.OutputParametersGet = () => new ParameterCollection();
            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "incident")
                {
                    Entity inc = new Entity("incident");
                    inc.Id = guid;
                    inc["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
                    inc["customerid"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    return inc;
                }

                if (entityName == "account")
                {
                    Entity account = new Entity("account");
                    account.Id = guid;
                    account["jarvis_language"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    return account;
                }

                if (entityName == "jarvis_language")
                {
                    Entity language = new Entity("jarvis_language");
                    language.Id = guid;
                    language["jarvis_iso2languagecode6391"] = "FRE";
                    return language;
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
                        Entity configuration = new Entity("jarvis_configurationjarvis");
                        configuration["jarvis_automationtranslation"] = true;
                        result.Entities.Add(configuration);
                    }
                }

                return result;
            };

            ForcedTranslationAction plugin = new ForcedTranslationAction();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Forced Translation Op5.
        /// </summary>
        [TestMethod]
        public void ForcedTranslationOp5PO()
        {
            var incident = new Entity("incident");
            incident.Id = Guid.NewGuid();
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Operation", 5),
                   new KeyValuePair<string, object>("EntityName", "PASSOUT"),
                   new KeyValuePair<string, object>("IncidentId", Guid.NewGuid().ToString()),
                   new KeyValuePair<string, object>("TargetEntityId", Guid.NewGuid().ToString()),
                   new KeyValuePair<string, object>("TriggeredById", Guid.NewGuid().ToString()),
               };
            this.PluginExecutionContext.OutputParametersGet = () => new ParameterCollection();
            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "incident")
                {
                    Entity inc = new Entity("incident");
                    inc.Id = guid;
                    inc["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
                    inc["customerid"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    return inc;
                }

                if (entityName == "account")
                {
                    Entity account = new Entity("account");
                    account.Id = guid;
                    account["jarvis_language"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    return account;
                }

                if (entityName == "jarvis_language")
                {
                    Entity language = new Entity("jarvis_language");
                    language.Id = guid;
                    language["jarvis_iso2languagecode6391"] = "FRE";
                    return language;
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
                        Entity configuration = new Entity("jarvis_configurationjarvis");
                        configuration["jarvis_automationtranslation"] = true;
                        result.Entities.Add(configuration);
                    }
                }

                return result;
            };

            ForcedTranslationAction plugin = new ForcedTranslationAction();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Forced Translation Op5.
        /// </summary>
        [TestMethod]
        public void ForcedTranslationOp5JED()
        {
            var incident = new Entity("incident");
            incident.Id = Guid.NewGuid();
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Operation", 5),
                   new KeyValuePair<string, object>("EntityName", "JED"),
                   new KeyValuePair<string, object>("IncidentId", Guid.NewGuid().ToString()),
                   new KeyValuePair<string, object>("TargetEntityId", Guid.NewGuid().ToString()),
                   new KeyValuePair<string, object>("TriggeredById", Guid.NewGuid().ToString()),
               };
            this.PluginExecutionContext.OutputParametersGet = () => new ParameterCollection();
            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "incident")
                {
                    Entity inc = new Entity("incident");
                    inc.Id = guid;
                    inc["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
                    inc["customerid"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    return inc;
                }

                if (entityName == "account")
                {
                    Entity account = new Entity("account");
                    account.Id = guid;
                    account["jarvis_language"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    return account;
                }

                if (entityName == "jarvis_language")
                {
                    Entity language = new Entity("jarvis_language");
                    language.Id = guid;
                    language["jarvis_iso2languagecode6391"] = "FRE";
                    return language;
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
                        Entity configuration = new Entity("jarvis_configurationjarvis");
                        configuration["jarvis_automationtranslation"] = true;
                        result.Entities.Add(configuration);
                    }
                }

                return result;
            };

            ForcedTranslationAction plugin = new ForcedTranslationAction();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Forced Translation Op5.
        /// </summary>
        [TestMethod]
        public void ForcedTranslationOp5Repair()
        {
            var incident = new Entity("incident");
            incident.Id = Guid.NewGuid();
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Operation", 5),
                   new KeyValuePair<string, object>("EntityName", "REPAIR INFO"),
                   new KeyValuePair<string, object>("IncidentId", Guid.NewGuid().ToString()),
                   new KeyValuePair<string, object>("TargetEntityId", Guid.NewGuid().ToString()),
                   new KeyValuePair<string, object>("TriggeredById", Guid.NewGuid().ToString()),
               };
            this.PluginExecutionContext.OutputParametersGet = () => new ParameterCollection();
            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "incident")
                {
                    Entity inc = new Entity("incident");
                    inc.Id = guid;
                    inc["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
                    inc["customerid"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    return inc;
                }

                if (entityName == "account")
                {
                    Entity account = new Entity("account");
                    account.Id = guid;
                    account["jarvis_language"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    return account;
                }

                if (entityName == "jarvis_language")
                {
                    Entity language = new Entity("jarvis_language");
                    language.Id = guid;
                    language["jarvis_iso2languagecode6391"] = "FRE";
                    return language;
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
                        Entity configuration = new Entity("jarvis_configurationjarvis");
                        configuration["jarvis_automationtranslation"] = true;
                        result.Entities.Add(configuration);
                    }
                }

                return result;
            };

            ForcedTranslationAction plugin = new ForcedTranslationAction();
            plugin.Execute(this.ServiceProvider);
        }
    }
}