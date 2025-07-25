// <copyright file="CaseResolutionAsync.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace MCS.Jarvis.CE.Plugins
{
    using System;
    using System.Globalization;
    using global::Plugins;
    using MCS.Jarvis.CE.BusinessProcessesShared.Helpers;
    using Microsoft.Xrm.Sdk;
    using Newtonsoft.Json;

    /// <summary>
    /// Case Resolution Async.
    /// </summary>
    public class CaseResolutionAsync : PluginBase
    {
        /// <summary>
        /// secure String.
        /// </summary>
        private readonly string secureString;

        /// <summary>
        /// unSecure String.
        /// </summary>
        private readonly string unSecureString;

        /// <summary>
        /// Initializes a new instance of the <see cref="CaseResolutionAsync"/> class.
        /// CaseResolutionAsync.
        /// </summary>
        /// <param name="unsecureString">unsecure String.</param>
        /// <param name="secureString">secure String.</param>
        /// <exception cref="InvalidPluginExecutionException">Invalid Plugin Execution Exception.</exception>
        public CaseResolutionAsync(string unsecureString, string secureString)
            : base(typeof(CaseResolutionAsync))
        {
            if (string.IsNullOrEmpty(secureString) && string.IsNullOrWhiteSpace(unsecureString))
            {
                throw new InvalidPluginExecutionException("Secure and unsecure strings are required for this plugin to execute.");
            }

            this.secureString = secureString;
            this.unSecureString = unsecureString;
        }

        /// <summary>
        /// Execute CRM Plugin.
        /// </summary>
        /// <param name="localcontext">Local Context.</param>
        /// <exception cref="InvalidPluginExecutionException">Invalid Plugin Execution Exception.</exception>
        public override void ExecuteCrmPlugin(LocalPluginContext localcontext)
        {
            ////Extract the tracing service for use in debugging sandboxed plug-ins.
            ITracingService tracingService =
                localcontext.TracingService;

            // Obtain the execution context from the service provider.
            IPluginExecutionContext context = localcontext.PluginExecutionContext;

            ITracingService traceService = localcontext.TracingService;
            traceService.Trace("Start Case Resolution operation:" + DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));

            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                Entity caseResolution = (Entity)context.InputParameters["Target"];
                string jsonObject = null;

                try
                {
                    if (caseResolution.Attributes.Contains("incidentid") && caseResolution.Attributes["incidentid"] != null && context.PostEntityImages["PostImage"] != null
                        && caseResolution.Attributes.Contains("resolutiontypecode") && caseResolution.Attributes["resolutiontypecode"] != null)
                    {
                        IOrganizationService orgService = localcontext.OrganizationService;
                        IOrganizationService service = localcontext.AdminOrganizationService;
                        EntityReference incident = (EntityReference)caseResolution.Attributes["incidentid"];
                        OptionSetValue resolutionType = (OptionSetValue)caseResolution.Attributes["resolutiontypecode"];
                        bool isCaseTypeBreakdown = CrmHelper.CheckCaseTypeIsBreakdown(incident.Id, traceService, orgService);
                        traceService.Trace("Case Type eq Breakdown " + isCaseTypeBreakdown.ToString() + " and incident id is " + incident.Id);
                        bool isMercuriusUser = CrmHelper.CheckUserIsMercurius(context.UserId, tracingService, service);
                        tracingService.Trace($"IsMercuriusUserCheck: {isMercuriusUser}");
                        Entity caseResolutionImg = context.PostEntityImages["PostImage"];
                        if (caseResolutionImg != null)
                        {
                            dynamic caseResolutionData = caseResolutionImg.Attributes;
                            jsonObject = JsonConvert.SerializeObject(caseResolutionData) ?? string.Empty;
                        }

                        if (isCaseTypeBreakdown && !isMercuriusUser && resolutionType.Value == 1000)
                        {
                            ////Framing Case entity Based on Case Resolution Data
                            Entity caseToUpdate = new Entity(incident.LogicalName);
                            caseToUpdate.Id = incident.Id;

                            tracingService.Trace("Incident Contains Data");
                            CrmHelper.JarvisToFunction(caseToUpdate, jsonObject, tracingService, this.unSecureString.ToString(), this.secureString.ToString());
                        }
                    }
                }
                catch (InvalidPluginExecutionException ex)
                {
                    throw new InvalidPluginExecutionException("Error in Resolution Operations " + ex.Message + string.Empty);
                }
            }
        }
    }
}
