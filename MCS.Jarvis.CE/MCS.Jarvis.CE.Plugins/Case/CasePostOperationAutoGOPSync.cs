//-----------------------------------------------------------------------
// <copyright file="CasePostOperationAutoGOPSync.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace MCS.Jarvis.CE.Plugins
{
    using System;
    using System.Globalization;
    using System.ServiceModel;
    using global::Plugins;
    using MCS.Jarvis.CE.BusinessProcessesShared.Helpers;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;
    using static MCS.Jarvis.CE.BusinessProcessesShared.Helpers.Constants;

    /// <summary>
    /// CasePostOperationAutoGOPSync --> To initiate GOP Automation based on case stage.
    /// </summary>
    public class CasePostOperationAutoGOPSync : PluginBase
    {
        /// <summary>
        /// secure string.
        /// </summary>
        private readonly string secureString;

        /// <summary>
        /// unsecure string.
        /// </summary>
        private readonly string unSecureString;

        /// <summary>
        /// Initializes a new instance of the <see cref="CasePostOperationAutoGOPSync"/> class.
        /// Case Post Operation Sync.
        /// </summary>
        /// <param name="unsecureParamString">unsecure String.</param>
        /// <param name="secureParamString">secure String.</param>
        public CasePostOperationAutoGOPSync(string unsecureParamString, string secureParamString)
            : base(typeof(CasePostOperationAutoGOPSync))
        {
            this.secureString = secureParamString;
            this.unSecureString = unsecureParamString;
        }

        /// <summary>
        /// Execute Post Operation Plugin.
        /// </summary>
        /// <param name="localcontext">local context.</param>
        public override void ExecuteCrmPlugin(LocalPluginContext localcontext)
        {
            ITracingService traceService = localcontext.TracingService;
            traceService.Trace("Start CasePostOperationAutoGOPSync:" + DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));
            try
            {
                IPluginExecutionContext context = localcontext.PluginExecutionContext;
                IOrganizationService orgService = localcontext.OrganizationService;
                IOrganizationService service = localcontext.AdminOrganizationService;
                bool autoGopEnabled = CrmHelper.GetAutomationConfig(service, JarvisConfiguration.Automationinitialgop, traceService);
                if (autoGopEnabled)
                {
                    if (context.MessageName.ToUpper() == PluginMessage.Update.ToUpper() && context.InputParameters.Contains(General.Target) && context.InputParameters[General.Target] is Entity entity && context.PostEntityImages["PostImage"] != null)
                    {
                        traceService.Trace("Enter into Update after condition Check for GOP");
                        Entity targetEntity = entity;
                        Entity incidentPre = (Entity)context.PostEntityImages["PostImage"];
                        if (string.IsNullOrEmpty(this.secureString) && string.IsNullOrWhiteSpace(this.secureString))
                        {
                            throw new InvalidPluginExecutionException("Secure strings are required for this plugin to execute.");
                        }

                        if (targetEntity.Attributes.Contains("statuscode") && targetEntity.Attributes["statuscode"] != null)
                        {
                            OptionSetValue statusCode = (OptionSetValue)targetEntity.Attributes["statuscode"];
                            OptionSetValue caseOrginCode = new OptionSetValue();
                            EntityReference homeDealerValue = null;
                            bool? blackList = null;
                            if (targetEntity.Attributes.Contains("caseorigincode") && targetEntity.Attributes["caseorigincode"] != null)
                            {
                                caseOrginCode = (OptionSetValue)targetEntity.Attributes["caseorigincode"];
                            }
                            else if (incidentPre.Attributes.Contains("caseorigincode") && incidentPre.Attributes["caseorigincode"] != null)
                            {
                                caseOrginCode = (OptionSetValue)incidentPre.Attributes["caseorigincode"];
                            }

                            traceService.Trace($"Before HomeDealer: {incidentPre.Attributes.Contains(Incident.HomeDealer)}");
                            if (incidentPre.Attributes.Contains(Incident.HomeDealer) && incidentPre.Attributes[Incident.HomeDealer] != null)
                            {
                                homeDealerValue = (EntityReference)incidentPre.Attributes[Incident.HomeDealer];
                                if (homeDealerValue != null)
                                {
                                    var homeDealer = orgService.Retrieve(homeDealerValue.LogicalName, homeDealerValue.Id, new ColumnSet(Accounts.Blacklist));
                                    traceService.Trace($"Entered in Home Dealer Details With Blacklist");
                                    if (homeDealer != null && homeDealer.Attributes.Contains(Accounts.Blacklist))
                                    {
                                        blackList = (bool?)homeDealer.Attributes[Accounts.Blacklist];
                                    }
                                }
                            }

                            EntityCollection existingGOPs = orgService.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getGOPsForCase, targetEntity.Id)));
                            if (existingGOPs.Entities.Count < 1 && !(blackList != null && blackList == true))
                            {
                                if (statusCode.Value == 20 && (caseOrginCode.Value == 1 || (caseOrginCode.Value == 334030002 && incidentPre.Attributes.Contains(Incident.CallerRole) && (((OptionSetValue)incidentPre.Attributes[Incident.CallerRole]).Value == 4 || ((OptionSetValue)incidentPre.Attributes[Incident.CallerRole]).Value == 3))))
                                {
                                    CrmHelper.JarvisToGopFunction(targetEntity, string.Empty, traceService, string.Empty, this.secureString.ToString());
                                }
                            }
                        }
                    }
                }
            }
            catch (InvalidPluginExecutionException oex)
            {
                traceService.Trace(oex.Message);
                traceService.Trace(oex.StackTrace);
                ////No Error Message Throwing Implictly to avoid blocking user because of integration.
            }
            finally
            {
                traceService.Trace("End PassOutTransPostOperationAsync:" + DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));
                ////No Error Message Throwing Implictly to avoid blocking user because of integration.
            }
        }
    }
}