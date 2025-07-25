// <copyright file="CaseCloseAndCreateMonitorActions.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace MCS.Jarvis.CE.Plugins.Actions
{
    using System;
    using System.Linq;
    using System.ServiceModel;
    using global::Plugins;
    using MCS.Jarvis.CE.BusinessProcessShared.CaseMonitor;
    using MCS.Jarvis.CE.Commons;
    using Microsoft.Crm.Sdk.Messages;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;

    /// <summary>
    /// CaseCloseAndCreateMonitorActions when Case HD = RD and has one pass out .
    /// </summary>
    public class CaseCloseAndCreateMonitorActions : PluginBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CaseCloseAndCreateMonitorActions"/> class.
        /// CaseCloseAndCreateMonitorActions:Constructor.
        /// </summary>
        public CaseCloseAndCreateMonitorActions()
            : base(typeof(CaseCloseAndCreateMonitorActions))
        {
        }

        /// <summary>
        /// Get Monitor Actions Related to Case and close them .
        /// </summary>
        /// <param name="localcontext"> local context.</param>
        /// <exception cref="InvalidPluginExecutionException">Invalid Plugin Execution Exception.</exception>
        public override void ExecuteCrmPlugin(LocalPluginContext localcontext)
        {
            // Extract the tracing service for use in debugging sandboxed plug-ins.
            ITracingService tracingService =
                localcontext.TracingService;

            // Obtain the execution context from the service provider.
            IPluginExecutionContext context = localcontext.PluginExecutionContext;

            string propName = string.Empty;
            ResourcesPayload resourcePayload = new ResourcesPayload();
            try
            {
                IOrganizationService service = localcontext.OrganizationService;
                CaseMonitorProcess operations = new CaseMonitorProcess();
                Guid initiatingUserID = context.UserId;
                DateTime duedate;
                string atatime = "00:00";
                string actionTarget = string.Empty;
                tracingService.Trace("CaseCloseAndCreateMonitorActions for HD=RD Started");

                // Check If Incident Id available
                if (context.InputParameters.Contains("IncidentId") && context.InputParameters["IncidentId"] != null)
                {
                    // Close existing Monitor actions
                    Guid.TryParse(context.InputParameters["IncidentId"].ToString(), out Guid incidentId);
                    Entity incident = service.Retrieve(Constants.Incident.IncidentValue, incidentId, new ColumnSet(true));
                    if (context.InputParameters.Contains("actionTarget") && context.InputParameters["actionTarget"] != null)
                    {
                        actionTarget = context.InputParameters["actionTarget"].ToString();
                    }

                    EntityCollection caseHDRDMonitorActionItems = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getHDRDMonitorActions, incidentId)));
                    EntityCollection casePassouts = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.CaseAllPassouts, incidentId)));
                    Entity passout = casePassouts.Entities.FirstOrDefault();

                    if (incident != null)
                    {
                        // Cloase all Monitor Actions and Create High Priority Monitor Action with "YYY YY Case auto close HD=RD"
                        if (actionTarget == "CLoseMonitorActions" && caseHDRDMonitorActionItems.Entities.Count() == 0)
                        {
                            operations.CloseMonitorActions(incident, service, tracingService);
                            int timeZoneCode = 105;
                            if (incident.Attributes.Contains("jarvis_timezone") && incident.Attributes["jarvis_timezone"] != null)
                            {
                                timeZoneCode = (int)incident.Attributes["jarvis_timezone"];
                            }

                            // Find The ATA for case passout , and add 24 hhrs with due date
                            if (casePassouts.Entities.Count() > 0)
                            {
                                if (incident.Attributes.Contains("jarvis_ata") && incident["jarvis_ata"] != null)
                                {
                                    duedate = (DateTime)incident.Attributes["jarvis_ata"];

                                    // DateTime localDate = this.RetrieveLocalTimeFromUTCTime(service, (DateTime)passout.Attributes["jarvis_ata"], timeZoneCode);
                                    duedate = duedate.AddDays(1);
                                }
                                else
                                {
                                    duedate = DateTime.UtcNow.AddDays(1);
                                }

                                if (passout.Attributes.Contains("jarvis_atatime") && passout["jarvis_atatime"] != null)
                                {
                                    atatime = (string)passout.Attributes["jarvis_atatime"];
                                }
                            }
                            else
                            {
                                duedate = DateTime.UtcNow.AddDays(1);
                            }

                            if (atatime.Length == 4 && !atatime.Contains(":"))
                            {
                                var hr = atatime.Substring(0, 2);
                                var mm = atatime.Substring(2, 2);
                                atatime = hr + ':' + mm;
                            }

                            // Create New Monitor no Steering monitor Action
                            Entity caseMonitorAction = new Entity(CaseMonitorAction.EntityLogicalName);
                            EntityCollection getMonitorActionsForCase = service.RetrieveMultiple(new FetchExpression(string.Format(MCS.Jarvis.CE.Plugins.Constants.FetchXmls.getCountryMO, "YY")));
                            EntityCollection getLanguageforMOCase = service.RetrieveMultiple(new FetchExpression(string.Format(MCS.Jarvis.CE.Plugins.Constants.FetchXmls.getNonSteeringLanguageMO, "YYY")));
                            if (getMonitorActionsForCase.Entities.Count > 0)
                            {
                                caseMonitorAction["jarvis_followupcountry"] = new EntityReference(getMonitorActionsForCase.Entities[0].LogicalName, getMonitorActionsForCase.Entities[0].Id);
                            }

                            if (getLanguageforMOCase.Entities.Count > 0)
                            {
                                caseMonitorAction["jarvis_followuplanguage"] = new EntityReference(getLanguageforMOCase.Entities[0].LogicalName, getLanguageforMOCase.Entities[0].Id);
                            }

                            // Urgency High
                            caseMonitorAction.Attributes.Add(CaseMonitorAction.Attributes.ActionType, new OptionSetValue(1));

                            // High
                            caseMonitorAction.Attributes.Add(CaseMonitorAction.Attributes.PriorityCode, new OptionSetValue(0));
                            caseMonitorAction.Attributes.Add(CaseMonitorAction.Attributes.Subject, "YYY YY Case auto close HD=RD");
                            caseMonitorAction.Attributes.Add(CaseMonitorAction.Attributes.Followuptime, atatime);
                            caseMonitorAction.Attributes.Add(CaseMonitorAction.Attributes.FollowupDate, duedate);
                            caseMonitorAction.Attributes.Add("jarvis_followuptimestamp", duedate);
                            operations.CreateMonitorProcess(initiatingUserID, caseMonitorAction, incident, service, service, tracingService, true, duedate);
                        }
                        else if (actionTarget == "CLoseCase" && casePassouts.Entities.Count() == 1 && incident.Attributes.Contains("statuscode") && ((OptionSetValue)incident.Attributes["statuscode"]).Value != 90 && ((OptionSetValue)incident.Attributes["statuscode"]).Value != 5)
                        {
                            // Create JED
                            EntityCollection mileageUnits = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getMileageUnits)));
                            Entity mileageunit = mileageUnits.Entities.FirstOrDefault();
                            int mileage = 0;
                            if (incident.Attributes.Contains("jarvis_mileage") && incident.Attributes["jarvis_mileage"] != null)
                            {
                                mileage = (int)incident.Attributes["jarvis_mileage"];
                            }

                            Entity jed = new Entity(JobEndDetails.EntityLogicalName);

                            // Send
                            jed.Attributes.Add(JobEndDetails.Attributes.Statuscode, new OptionSetValue(334030001));

                            // High
                            jed.Attributes.Add(JobEndDetails.Attributes.RelatedCase, incident.ToEntityReference());
                            jed.Attributes.Add(JobEndDetails.Attributes.RepairingDealer, passout.ToEntityReference());
                            jed.Attributes.Add(JobEndDetails.Attributes.ActualCauseFault, "Repair details unavailable, please contact your dealer for more information");
                            jed.Attributes.Add(JobEndDetails.Attributes.Mileage, new decimal(mileage));
                            if (incident.Attributes.Contains("jarvis_mileageunit") && incident.Attributes["jarvis_mileageunit"] != null)
                            {
                                EntityReference caseMileageUnit = (EntityReference)incident.Attributes["jarvis_mileageunit"];
                                jed.Attributes.Add(JobEndDetails.Attributes.MileageUnit, caseMileageUnit);
                            }
                            else
                            {
                                jed.Attributes.Add(JobEndDetails.Attributes.MileageUnit, new EntityReference(mileageunit.LogicalName, mileageunit.Id));
                            }

                            service.Create(jed);

                            // Close Case
                            int timeZoneCode = 105;
                            if (incident.Attributes.Contains("jarvis_timezone") && incident.Attributes["jarvis_timezone"] != null)
                            {
                                timeZoneCode = (int)incident.Attributes["jarvis_timezone"];
                            }

                            string atctime = string.Empty;

                            // DateTime actualStart = this.RetrieveLocalTimeFromUTCTime(service, DateTime.UtcNow, timeZoneCode);
                            Entity updatePassout = new Entity(passout.LogicalName, passout.Id);
                            // updatePassout.Attributes["jarvis_atcdate"] = DateTime.UtcNow;
                            // atctime = DateTime.UtcNow.ToString("hhmm");
                            // updatePassout.Attributes["jarvis_atctime"] = atctime;
                            updatePassout.Attributes["jarvis_atc"] = DateTime.UtcNow;
                            service.Update(updatePassout);
                            if (incident != null)
                            {
                                operations.CloseMonitorActions(incident, service, tracingService);
                            }

                            // Close Case
                            CasePostOperationSync casePostoperation = new CasePostOperationSync();
                            casePostoperation.ExecuteBPF(incident.ToEntityReference(), Constants.Incident.BpfStage9, service, tracingService);
                            Entity updateCase = new Entity(Constants.Incident.IncidentValue, incidentId);
                            updateCase.Attributes["jarvis_customerinformed"] = true;
                            service.Update(updateCase);
                        }
                    }
                }
            }
            catch (InvalidOperationException oex)
            {
                tracingService.Trace(oex.Message);
                tracingService.Trace(oex.StackTrace);
            }
            catch (FaultException<OrganizationServiceFault> fex)
            {
                tracingService.Trace(fex.Message);
                tracingService.Trace(fex.StackTrace);
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException(ex.Message + propName);
            }
        }

        /// <summary>
        /// Retrieve Local Time From UTC Time.
        /// </summary>
        /// <param name="service">Org service.</param>
        /// <param name="utcTime">UTC Time.</param>
        /// <param name="timeZoneCode">time Zone Code.</param>
        /// <returns>returns local time.</returns>
        public DateTime RetrieveLocalTimeFromUTCTime(IOrganizationService service, DateTime utcTime, int? timeZoneCode)
        {
            if (!timeZoneCode.HasValue)
            {
                return utcTime;
            }

            var request = new LocalTimeFromUtcTimeRequest
            {
                TimeZoneCode = timeZoneCode.Value,
                UtcTime = utcTime.ToUniversalTime(),
            };

            var response = (LocalTimeFromUtcTimeResponse)service.Execute(request);

            return response.LocalTime;
        }
    }
}
