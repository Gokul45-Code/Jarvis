// <copyright file="CaseMonitorPostOperation.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace MCS.Jarvis.IntegrationPlugin
{
    using System;
    using System.Globalization;
    using MCS.Jarvis.CE.BusinessProcessesShared.Helpers;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;
    using Newtonsoft.Json;
    using Plugins;
    using static MCS.Jarvis.CE.BusinessProcessesShared.Helpers.Constants;

    /// <summary>
    /// Case Monitor Post Operation.
    /// </summary>
    public class CaseMonitorPostOperation : PluginBase
    {
        /// <summary>
        /// IncidentPostOperationAsync --> Register as Sync and with Mapping Field as Post Image.
        /// </summary>
        private readonly string secureString;

        /// <summary>
        /// unSecure String.
        /// </summary>
        private readonly string unSecureString;

        /// <summary>
        /// Initializes a new instance of the <see cref="CaseMonitorPostOperation"/> class.
        /// Job End Details Translation Constructor.
        /// </summary>
        /// <param name="unsecureString">unsecure String.</param>
        /// <param name="secureString">secure String.</param>
        /// <exception cref="InvalidPluginExecutionException">Invalid Plugin Execution Exception.</exception>
        public CaseMonitorPostOperation(string unsecureString, string secureString)
            : base(typeof(CaseMonitorPostOperation))
        {
            if (string.IsNullOrEmpty(secureString) && string.IsNullOrWhiteSpace(unsecureString))
            {
                throw new InvalidPluginExecutionException("Secure strings are required for this plugin to execute.");
            }

            this.secureString = secureString;
            this.unSecureString = unsecureString;
        }

        /// <summary>
        /// Execute Job End Details Translation Plugin.
        /// </summary>
        /// <param name="localcontext">local context.</param>
        /// <exception cref="InvalidPluginExecutionException">Invalid Plugin Execution Exception.</exception>
        public override void ExecuteCrmPlugin(LocalPluginContext localcontext)
        {
            ////Extract the tracing service for use in debugging sandboxed plug-ins.
            ITracingService traceService = localcontext.TracingService;
            traceService.Trace("Start CaseMonitorPostOperation:" + DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));
            try
            {
                // Obtain the execution context from the service provider.
                IPluginExecutionContext context = localcontext.PluginExecutionContext;
                IOrganizationService orgService = localcontext.OrganizationService;
                //// No Need Mercurius Check Because Check has been handled by Mercurius user or System user.
                if (context.InputParameters.Contains(General.Target) && context.InputParameters[General.Target] is Entity entity)
                {
                    bool isCheckPrePostImage = true;
                    traceService.Trace("Enter into Create/update after condition ");
                    if (context.MessageName.ToString().ToUpper() == PluginMessage.Update.ToUpper() && context.PostEntityImages["PostImage"] != null)
                    {
                        traceService.Trace("Inside Create/Update Case Monitor Action Step");
                        Entity caseImg = context.PostEntityImages["PostImage"];
                        traceService.Trace($"preCompareImage : {context.PreEntityImages.Contains("ComparePreImage")}");
                        traceService.Trace($"postCompareImage : {context.PostEntityImages.Contains("ComparePostImage")}");
                        if (context.PreEntityImages["ComparePreImage"] != null && context.PostEntityImages["ComparePostImage"] != null)
                        {
                            traceService.Trace($"CheckPrePostImage Method Started.");
                            isCheckPrePostImage = CrmHelper.CheckPrePostImage(context.PreEntityImages["ComparePreImage"], context.PostEntityImages["ComparePostImage"], traceService);
                            traceService.Trace($"CheckPrePostImage: {isCheckPrePostImage}");
                        }

                        if (caseImg != null && caseImg.Attributes.Contains(CaseMonitor.jarvis_fulinknew) && caseImg.Attributes.Contains(Incident.casetypecode) && caseImg.Attributes[Incident.casetypecode] != null)
                        {
                            traceService.Trace("Check FulinkNew (CaseMonintorAction Record is present or not.");
                            dynamic jobEndTransData = caseImg.Attributes;
                            string jsonObject = JsonConvert.SerializeObject(jobEndTransData) ?? string.Empty;
                            traceService.Trace($"Entity Data : {jsonObject}");
                            EntityReference caseMonitorActionRef = (EntityReference)caseImg.Attributes[CaseMonitor.jarvis_fulinknew];
                            traceService.Trace("Incident having the latest Case Monitor Action Activity.");
                            OptionSetValue caseTypeBreakdown = (OptionSetValue)caseImg.Attributes[Incident.casetypecode];
                            traceService.Trace("Case Type eq Breakdown " + caseTypeBreakdown.Value.ToString() + " and incident id is " + caseImg.Id);

                            //// IsCaseType == Breakdown, Calling user is not by Mercurius user and System User and CaseMonitorAction Source == Jarvis(OneCase).

                            if (caseTypeBreakdown.Value == (int)CaseTypeCode.Breakdown && caseMonitorActionRef != null)
                            {
                                Entity caseMontiorAction = orgService.Retrieve(caseMonitorActionRef.LogicalName, caseMonitorActionRef.Id, new ColumnSet(CaseMonitor.jarvis_source));
                                OptionSetValue source = (OptionSetValue)caseMontiorAction.Attributes[CaseMonitor.jarvis_source];
                                if (source != null && source.Value == (int)Source.Jarvis && isCheckPrePostImage)
                                {
                                    traceService.Trace("Case Type eq Breakdown, and CaseMonitorAction.Source == Jarvis");
                                    CrmHelper.JarvisToFunction(caseImg, jsonObject, traceService, this.unSecureString.ToString(), this.secureString.ToString());
                                }
                            }
                        }
                    }
                }
            }
            catch (InvalidPluginExecutionException pex)
            {
                traceService.Trace(pex.Message);
                traceService.Trace(pex.StackTrace);
                ////No Error Message Throwing Implictly to avoid blocking user because of integration.
            }
            finally
            {
                traceService.Trace("End JobEndDetailsTransPostOperationAsync:" + DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));
                ////No Error Message Throwing Implictly to avoid blocking user because of integration.
            }
        }
    }
}
