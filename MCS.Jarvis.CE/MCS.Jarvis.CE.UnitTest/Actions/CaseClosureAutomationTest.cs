// <copyright file="CaseClosureAutomationTest.cs" company="Microsoft">
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
    /// Case Closure Automation Test.
    /// </summary>
    [TestClass]
    public class CaseClosureAutomationTest : UnitTestBase
    {
        /// <summary>
        /// Case Closure for Open JEDs.
        /// </summary>
        [TestMethod]
        public void CaseClosureWithOpenJED()
        {
            var incident = new Entity("incident");
            incident.Id = Guid.NewGuid();
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("CaseID", incident.Id.ToString()),
               };

            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();
                if (query.GetType().Name == "FetchExpression")
                {
                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_passout'>"))
                    {
                        Entity passoutResult = new Entity("jarvis_passout");
                        passoutResult["statuscode"] = new OptionSetValue(334030002);
                        passoutResult["statecode"] = new OptionSetValue(0);
                        passoutResult["jarvis_incident"] = new EntityReference("incident", Guid.NewGuid());
                        passoutResult.Id = Guid.NewGuid();
                        passoutResult["jarvis_name"] = "Pass Out";
                        passoutResult["createdon"] = DateTime.UtcNow;
                        passoutResult["jarvis_repairingdealer"] = new EntityReference("account", Guid.NewGuid());
                        passoutResult["jarvis_goplimitout"] = Convert.ToDecimal(12);
                        passoutResult["transactioncurrencyid"] = new EntityReference("transactioncurrencies", Guid.NewGuid());
                        passoutResult["modifiedon"] = DateTime.UtcNow;
                        result.Entities.Add(passoutResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_jobenddetails'>"))
                    {
                        Entity jedResult = new Entity("jarvis_jobenddetails");
                        jedResult["statuscode"] = new OptionSetValue(334030002);
                        jedResult["jarvis_repairingdealerpassout"] = new EntityReference("jarvis_passout", Guid.NewGuid());
                        jedResult["jarvis_incident"] = new EntityReference("incident", Guid.NewGuid());
                        jedResult.Id = Guid.NewGuid();
                        result.Entities.Add(jedResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='email'>"))
                    {
                        Entity email = new Entity("email");
                        email["statuscode"] = new OptionSetValue(3);
                        email["jarvis_relatedjobenddetail"] = new EntityReference("jarvis_jobenddetails", Guid.NewGuid());
                        email.Id = Guid.NewGuid();
                        result.Entities.Add(email);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_casemonitoraction'>"))
                    {
                        Entity casemonitoraction = new Entity("jarvis_casemonitoraction");
                        casemonitoraction["statuscode"] = new OptionSetValue(1);
                        casemonitoraction["subject"] = "Pass ATC";
                        casemonitoraction["regardingobjectid"] = new EntityReference("incident", Guid.NewGuid());
                        casemonitoraction.Id = Guid.NewGuid();
                        result.Entities.Add(casemonitoraction);
                    }
                }

                return result;
            };

            CaseClosureAutomation plugin = new CaseClosureAutomation();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Case Closure for Closed JEDs.
        /// </summary>
        [TestMethod]
        public void CaseClosureWithCloseJED()
        {
            var incident = new Entity("incident");
            incident.Id = Guid.NewGuid();
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
                   {
                       new KeyValuePair<string, object>("CaseID", incident.Id.ToString()),
                   };

            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();
                if (query.GetType().Name == "FetchExpression")
                {
                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_casemonitoraction'>"))
                    {
                        Entity casemonitoraction = new Entity("jarvis_casemonitoraction");
                        casemonitoraction["statuscode"] = new OptionSetValue(1);
                        casemonitoraction["subject"] = "Pass ATC";
                        casemonitoraction["regardingobjectid"] = new EntityReference("incident", Guid.NewGuid());
                        casemonitoraction.Id = Guid.NewGuid();
                        result.Entities.Add(casemonitoraction);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_passout'>"))
                    {
                        Entity passoutResult = new Entity("jarvis_passout");
                        passoutResult["statuscode"] = new OptionSetValue(334030002);
                        passoutResult["statecode"] = new OptionSetValue(0);
                        passoutResult["jarvis_incident"] = new EntityReference("incident", Guid.NewGuid());
                        passoutResult.Id = Guid.NewGuid();
                        result.Entities.Add(passoutResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_jobenddetails'>"))
                    {
                        Entity jedResult = new Entity("jarvis_jobenddetails");
                        jedResult["statuscode"] = new OptionSetValue(334030002);
                        jedResult["jarvis_repairingdealerpassout"] = new EntityReference("jarvis_passout", Guid.NewGuid());
                        jedResult["jarvis_incident"] = new EntityReference("incident", Guid.NewGuid());
                        jedResult.Id = Guid.NewGuid();
                        result.Entities.Add(jedResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='email'>"))
                    {
                        Entity email = new Entity("email");
                        email["jarvis_relatedjobenddetail"] = new EntityReference("jarvis_jobenddetails", Guid.NewGuid());
                        email.Id = Guid.NewGuid();
                        result.Entities.Add(email);
                    }
                }

                return result;
            };

            CaseClosureAutomation plugin = new CaseClosureAutomation();
            plugin.Execute(this.ServiceProvider);
        }
    }
}
