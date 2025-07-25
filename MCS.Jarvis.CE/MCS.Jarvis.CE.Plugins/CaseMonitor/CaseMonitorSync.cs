// <copyright file="CaseMonitorSync.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace MCS.Jarvis.CE.Plugins.CaseMonitor
{
    using System;
    using global::Plugins;
    using MCS.Jarvis.CE.BusinessProcessesShared.Helpers;
    using MCS.Jarvis.CE.BusinessProcessShared.CaseMonitor;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;

    /// <summary>
    /// Case Monitor Sync.
    /// </summary>
    public class CaseMonitorSync : PluginBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CaseMonitorSync"/> class.
        /// CaseMonitorSync:Constructor.
        /// </summary>
        public CaseMonitorSync()
            : base(typeof(CaseMonitorSync))
        {
        }

        /// <summary>
        /// Execute Plugin Logic for CaseMonitorSync.
        /// </summary>
        /// <param name="localcontext">local Context.</param>
        /// <exception cref="InvalidPluginExecutionException">throw Exception.</exception>
        public override void ExecuteCrmPlugin(LocalPluginContext localcontext)
        {
            // Extract the tracing service for use in debugging sandboxed plug-ins.
            ITracingService tracingService =
                localcontext.TracingService;

            // Obtain the execution context from the service provider.
            IPluginExecutionContext context = localcontext.PluginExecutionContext;

            // Organization Service
            IOrganizationService service = localcontext.OrganizationService;
            Guid initiatingUserID = context.UserId;
            IOrganizationService adminservice = localcontext.AdminOrganizationService;

            CaseMonitorProcess operations = new CaseMonitorProcess();
            try
            {
#pragma warning disable SA1123 // Do not place regions within elements
                #region Create

                if (context.Stage == 20 && context.MessageName.ToUpper() == "CREATE")
                {
                    tracingService.Trace("Case monitor action pre create");
                    if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                    {
                        Entity monitor = (Entity)context.InputParameters["Target"];
                        if (monitor.Attributes.Contains("regardingobjectid") && monitor.Attributes["regardingobjectid"] != null)
                        {
                            EntityReference incidentReference = (EntityReference)monitor.Attributes["regardingobjectid"];
                            Entity incident = service.Retrieve(incidentReference.LogicalName, incidentReference.Id, new ColumnSet("statecode", "statuscode", "isescalated", "routecase", "jarvis_country"));
                            if (incident != null)
                            {
                                if (incident.Attributes.Contains("statecode") && incident.Attributes["statecode"] != null)
                                {
                                    OptionSetValue statecode = (OptionSetValue)incident.Attributes["statecode"];
                                    if (statecode.Value != 0)
                                    {
                                        throw new InvalidPluginExecutionException("You can’t create Monitor Actions for closed Cases");
                                    }
                                    else
                                    {
                                        operations.CreateMonitorProcess(initiatingUserID, monitor, incident, service, adminservice, localcontext.TracingService);
                                        ////operations.UpdateNextMOForCase(service, incident, localcontext.TracingService);
                                    }
                                }
                            }

                            OptionSetValue monitorActionSource = new OptionSetValue();
                            monitorActionSource = CrmHelper.GetReleaseAutomationConfig(service, MCS.Jarvis.CE.Plugins.Constants.JarvisConfiguration.Automationreleasecase, tracingService);
                            if (monitorActionSource.Value == 1 || monitorActionSource.Value == 2)
                            {
                                operations.CloseMonitorActions(incident, service, tracingService);
                            }
                        }
                    }
                }
#pragma warning restore SA1123 // Do not place regions within elements

                #endregion

#pragma warning disable SA1123 // Do not place regions within elements
                #region Create - Post

                if (context.Stage == 40 && context.MessageName.ToUpper() == "CREATE")
                {
                    tracingService.Trace("Case monitor action post create");
                    if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                    {
                        Entity monitor = (Entity)context.InputParameters["Target"];
                        Entity monitorRetrieve = (Entity)context.PostEntityImages["PostImage"];
                        if (monitorRetrieve.Attributes.Contains("regardingobjectid") && monitorRetrieve.Attributes["regardingobjectid"] != null)
                        {
                            EntityReference incidentReference = (EntityReference)monitorRetrieve.Attributes["regardingobjectid"];
                            Entity incident = service.Retrieve(incidentReference.LogicalName, incidentReference.Id, new ColumnSet("statecode", "statuscode", "isescalated"));
                            if (incident != null)
                            {
                                operations.UpdateNextMOForCase(service, incident, localcontext.TracingService);
                            }
                        }
                    }
                }
#pragma warning restore SA1123 // Do not place regions within elements

                #endregion

#pragma warning disable SA1123 // Do not place regions within elements
                #region Delete

                if (context.Stage == 20 && context.MessageName.ToUpper() == "DELETE")
                {
                    if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is EntityReference)
                    {
                        EntityReference monitor = (EntityReference)context.InputParameters["Target"];
                        Entity monitorRetrieve = (Entity)context.PreEntityImages["PreImage"];
                        if (monitorRetrieve.Attributes.Contains("regardingobjectid") && monitorRetrieve.Attributes["regardingobjectid"] != null)
                        {
                            EntityReference incident = (EntityReference)monitorRetrieve.Attributes["regardingobjectid"];
                            operations.DeleteMonitorProcess(monitor, incident, service);
                        }
                    }
                }
#pragma warning restore SA1123 // Do not place regions within elements

                #endregion

#pragma warning disable SA1123 // Do not place regions within elements
                #region Update - Post
                if (context.MessageName.ToUpper() == "UPDATE" && context.Stage == 40)
                {
                    if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                    {
                        Entity monitorRetrieve = (Entity)context.PostEntityImages["PostImage"];
                        if (monitorRetrieve.Attributes.Contains("regardingobjectid") && monitorRetrieve.Attributes["regardingobjectid"] != null)
                        {
                            EntityReference incidentReference = (EntityReference)monitorRetrieve.Attributes["regardingobjectid"];
                            if (incidentReference != null)
                            {
                                Entity incident = service.Retrieve(incidentReference.LogicalName, incidentReference.Id, new ColumnSet("jarvis_caseserviceline", "casetypecode", "jarvis_caselocation"));
                                operations.UpdateMonitorProcess(initiatingUserID, monitorRetrieve, incidentReference, service, adminservice);
                                operations.UpdateNextMOForCase(service, incident, localcontext.TracingService);
                            }
                        }
                    }
                }
#pragma warning restore SA1123 // Do not place regions within elements

                #endregion

#pragma warning disable SA1123 // Do not place regions within elements
                #region Update -Pre
                if (context.MessageName.ToUpper() == "UPDATE" && context.Stage == 20)
                {
                    if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                    {
                        Entity monitor = (Entity)context.InputParameters["Target"];
                        Entity monitorRetrieve = (Entity)context.PreEntityImages["PreImage"];
                        if (monitorRetrieve.Attributes.Contains("regardingobjectid") && monitorRetrieve.Attributes["regardingobjectid"] != null)
                        {
                            EntityReference incidentReference = (EntityReference)monitorRetrieve.Attributes["regardingobjectid"];
                            if (incidentReference != null)
                            {
                                Entity incident = service.Retrieve(incidentReference.LogicalName, incidentReference.Id, new ColumnSet("statuscode", "isescalated", "jarvis_country"));
                                string subject = string.Empty;
                                DateTime actualStart;
                                string followupTime = string.Empty;
                                OptionSetValue urgency = new OptionSetValue();
                                if (monitorRetrieve.Attributes.Contains("subject") && monitorRetrieve.Attributes["subject"] != null)
                                {
                                    subject = (string)monitorRetrieve.Attributes["subject"];
                                }

                                if (monitor.Attributes.Contains("subject") && monitor.Attributes["subject"] != null)
                                {
                                    subject = (string)monitor.Attributes["subject"];
                                }

                                if (monitorRetrieve.Attributes.Contains("prioritycode") && monitorRetrieve.Attributes["prioritycode"] != null)
                                {
                                    urgency = (OptionSetValue)monitorRetrieve.Attributes["prioritycode"];
                                }

                                if (monitor.Attributes.Contains("prioritycode") && monitor.Attributes["prioritycode"] != null)
                                {
                                    urgency = (OptionSetValue)monitor.Attributes["prioritycode"];
                                }

                                if (monitor.Attributes.Contains("actualstart") && monitor.Attributes["actualstart"] != null)
                                {
                                    actualStart = (DateTime)monitor.Attributes["actualstart"];
                                }
                                else
                                {
                                    actualStart = (DateTime)monitorRetrieve.Attributes["actualstart"];
                                }

                                if (monitor.Attributes.Contains("jarvis_followuptime") && monitor.Attributes["jarvis_followuptime"] != null)
                                {
                                    followupTime = (string)monitor.Attributes["jarvis_followuptime"];
                                }
                                else
                                {
                                    followupTime = (string)monitorRetrieve.Attributes["jarvis_followuptime"];
                                }

                                operations.UpdateMonitorActionTypeProcess(monitor, urgency, subject, incident, service, adminservice, actualStart, followupTime, tracingService);
                            }
                        }
                    }
                }
#pragma warning restore SA1123 // Do not place regions within elements
                #endregion
            }
            catch (InvalidPluginExecutionException ex)
            {
                tracingService.Trace(ex.Message);
                tracingService.Trace(ex.StackTrace);
                throw new InvalidPluginExecutionException("Error in Case Monitor Operations " + ex.Message);
            }
        }
    }
}
