// <copyright file="CaseCloseAndCreateMonitorActionsTest.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace MCS.Jarvis.CE.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.Security.Cryptography.Xml;
    using System.Windows.Controls;
    using MCS.Jarvis.CE.Plugins;
    using MCS.Jarvis.CE.Plugins.Actions;
    using Microsoft.Crm.Sdk.Messages;
    using Microsoft.IdentityModel.Claims;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;
    using static MCS.Jarvis.CE.BusinessProcessesShared.Helpers.Constants;

    /// <summary>
    /// Case Close And Create Monitor Actions Test.
    /// </summary>
    [TestClass]
    public class CaseCloseAndCreateMonitorActionsTest : UnitTestBase
    {
        /// <summary>
        /// Case Close And Create MA.
        /// </summary>
        [TestMethod]
        public void CloseMATest()
        {
            var incident = new Entity("incident");
            incident.Id = Guid.NewGuid();
            incident["jarvis_timezone"] = 105;
            incident["jarvis_ata"] = DateTime.UtcNow;
            Guid userId = Guid.NewGuid();
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("IncidentId", incident.Id),
                   new KeyValuePair<string, object>("UserID", userId),
                   new KeyValuePair<string, object>("actionTarget", "CLoseMonitorActions"),
               };

            // Setting service retrieve.
            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "incident")
                {
                    Entity inc = new Entity("incident");
                    inc.Id = guid;
                    inc["statuscode"] = new OptionSetValue(30);
                    inc["jarvis_ata"] = DateTime.Now;
                    inc["jarvis_timezone"] = 105;
                    return inc;
                }

                return null;
            };

            this.Service.RetrieveMultipleQueryBase = query =>
          {
              var result = new EntityCollection();
              if (query.GetType().Name == "FetchExpression")
              {
                  //if ((query as FetchExpression).Query.Contains("<entity name='jarvis_casemonitoraction'>"))
                  //{
                  //    Entity caseMonitor = new Entity("jarvis_casemonitoraction", Guid.NewGuid());
                  //    caseMonitor["subject"] = "Test";
                  //    caseMonitor["prioritycode"] = new OptionSetValue(0);
                  //    caseMonitor["jarvis_monitorsortorder"] = 1;
                  //    caseMonitor["jarvis_followuptimestamp"] = DateTime.Now;
                  //    caseMonitor["jarvis_followuplanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
                  //    caseMonitor["jarvis_followupcountry"] = new EntityReference("jarvis_country", Guid.NewGuid());
                  //    caseMonitor["createdby"] = new EntityReference("systemusers", Guid.NewGuid());
                  //    result.Entities.Add(caseMonitor);
                  //}

                  if ((query as FetchExpression).Query.Contains("<entity name='jarvis_passout'>"))
                  {
                      Entity passoutResult = new Entity("jarvis_passout");
                      passoutResult["jarvis_atadate"] = DateTime.UtcNow;
                      passoutResult["jarvis_atatime"] = "0300";
                      passoutResult.Id = Guid.NewGuid();
                      result.Entities.Add(passoutResult);
                  }

                  if ((query as FetchExpression).Query.Contains("<entity name='jarvis_language'>"))
                  {
                      Entity language = new Entity("jarvis_language");
                      language.Id = Guid.NewGuid();
                      //language["jarvis_iso2languagecode6391"] = "FRE";
                      result.Entities.Add(language);
                  }
              }

              return result;
          };

            this.Service.ExecuteOrganizationRequest = processInstanceRequest =>
            {
                return new LocalTimeFromUtcTimeResponse();
            };

            CaseCloseAndCreateMonitorActions plugin = new CaseCloseAndCreateMonitorActions();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Case Close And Create MA.
        /// </summary>
        [TestMethod]
        ////[ExpectedException(typeof(InvalidPluginExecutionException))]
        public void CloseCaseTest()
        {
            var incident = new Entity("incident");
            incident.Id = Guid.NewGuid();
            Guid userId = Guid.NewGuid();
            incident["statuscode"] = new OptionSetValue(10);
            incident["incidentid"] = new EntityReference("incident", Guid.NewGuid());
            incident["caseorigincode"] = new OptionSetValue(2);
            incident["casetypecode"] = new OptionSetValue(2);
            incident["jarvis_mercuriusstatus"] = new OptionSetValue(200);
            incident["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
            incident["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incident["customerid"] = new EntityReference("account", Guid.NewGuid());
            incident["jarvis_registrationnumber"] = "123";
            incident["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["description"] = "This is for testing";
            incident["jarvis_driverlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incident["jarvis_translationstatusreportedfault"] = new OptionSetValue(334030000);
            incident["jarvis_translationstatuslocation"] = new OptionSetValue(334030000);
            incident["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["jarvis_translationstatuscustomerexpectations"] = new OptionSetValue(334030000);
            incident["jarvis_location"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["jarvis_customerexpectations"] = "test";
            incident["ownerid"] = new EntityReference { LogicalName = "system_user", Name = "test" };
            incident["jarvis_sourceid"] = 232323232;
            incident["mileage"] = 30;
            incident["jarvis_mileageunit"] = new EntityReference("jarvis_mileages", Guid.NewGuid());
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("IncidentId", incident.Id),
                   new KeyValuePair<string, object>("UserID", userId),
                   new KeyValuePair<string, object>("actionTarget", "CLoseCase"),
               };

            // Setting service retrieve.
            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "incident")
                {
                    Entity inc = new Entity("incident");
                    inc.Id = guid;
                    inc["statuscode"] = new OptionSetValue(30);
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
                        Entity casemonitoraction = new Entity("jarvis_casemonitoraction");
                        casemonitoraction.Id = Guid.NewGuid();
                        result.Entities.Add(casemonitoraction);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_passout'>"))
                    {
                        Entity passoutResult = new Entity("jarvis_passout");
                        passoutResult["jarvis_atadate"] = DateTime.UtcNow;
                        passoutResult["jarvis_atatime"] = "0300";
                        passoutResult.Id = Guid.NewGuid();
                        result.Entities.Add(passoutResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_mileage'>"))
                    {
                        Entity mileage = new Entity("jarvis_mileage");
                        mileage.Id = Guid.NewGuid();
                        result.Entities.Add(mileage);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_vasbreakdownprocess'>"))
                    {
                        Entity vasprocess = new Entity("jarvis_vasbreakdownprocess");
                        vasprocess["businessprocessflowinstanceid"] = Guid.NewGuid();
                        vasprocess["bpf_name"] = "test";
                        vasprocess["bpf_incidentid"] = Guid.NewGuid();
                        vasprocess["activestageid"] = Guid.NewGuid();
                        vasprocess["statecode"] = new OptionSetValue(0);
                        vasprocess["createdon"] = DateTime.UtcNow;
                        vasprocess["createdby"] = "test";
                        vasprocess.Id = Guid.NewGuid();
                        result.Entities.Add(vasprocess);
                    }
                }

                return result;
            };

            this.Service.ExecuteOrganizationRequest = processInstanceRequest =>
            {
                return new LocalTimeFromUtcTimeResponse();
            };

            this.Service.ExecuteOrganizationRequest = processInstanceRequest =>
            {
                if (processInstanceRequest.RequestName.Contains("RetrieveProcessInstances"))
                {
                    return new RetrieveProcessInstancesResponse()
                    {
                        Results = new ParameterCollection
                {
                    new KeyValuePair<string, object>("Processes", new EntityCollection(new List<Entity> { incident })),
                },
                    };
                }

                if (processInstanceRequest.RequestName.Contains("RetrieveActivePath"))
                {
                    Entity activepath = new Entity("activepath");
                    activepath["stagename"] = "Pass Out";
                    activepath["processstageid"] = Guid.NewGuid();
                    return new RetrieveActivePathResponse()
                    {
                        Results = new ParameterCollection
                {
                    new KeyValuePair<string, object>("ProcessStages", new EntityCollection(new List<Entity> { activepath })),
                },
                    };
                }

                return null;
            };

            CaseCloseAndCreateMonitorActions plugin = new CaseCloseAndCreateMonitorActions();
            plugin.Execute(this.ServiceProvider);
        }
    }
}
