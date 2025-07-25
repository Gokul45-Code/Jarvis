// <copyright file="CasePreferredAgentSync.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace MCS.Jarvis.CE.Plugins.Case_Preferred_Agent
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.ServiceModel;
    using System.Text;
    using System.Threading.Tasks;
    using global::Plugins;
    using MCS.Jarvis.CE.BusinessProcessShared.CaseMonitor;
    using MCS.Jarvis.CE.BusinessProcessShared.CasePreferredAgent;
    using MCS.Jarvis.CE.Commons;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;

    /// <summary>
    /// Case Preferred Agent Sync.
    /// </summary>
    public class CasePreferredAgentSync : PluginBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CasePreferredAgentSync"/> class.
        /// CasePreferredAgentSync.
        /// </summary>
        public CasePreferredAgentSync()
            : base(typeof(CasePreferredAgentSync))
        {
        }

        /// <summary>
        /// Execute CRM Plugin.
        /// </summary>
        /// <param name="localcontext">local context.</param>
        /// <exception cref="InvalidPluginExecutionException">Invalid Plugin Execution Exception.</exception>
        public override void ExecuteCrmPlugin(LocalPluginContext localcontext)
        {
            // Extract the tracing service for use in debugging sandboxed plug-ins.
            ITracingService tracingService =
                localcontext.TracingService;

            // Obtain the execution context from the service provider.
            IPluginExecutionContext context = localcontext.PluginExecutionContext;

            // Organization Service
            IOrganizationService orgservice = localcontext.OrganizationService;
            Guid initiatingUserID = context.UserId;
            IOrganizationService adminservice = localcontext.AdminOrganizationService;

            try
            {
#pragma warning disable SA1123 // Do not place regions within elements
                #region Create

                if (context.MessageName.ToUpper() == "CREATE" && context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                {
                    Entity casePreferredAgent = (Entity)context.InputParameters["Target"];
                    Guid caseId = ((EntityReference)casePreferredAgent.Attributes["jarvis_case"]).Id;
                    if (caseId != Guid.Empty)
                    {
                        CasePreferredAgent cPA = new CasePreferredAgent();
                        cPA.CreateCasePreferredAgent(initiatingUserID, casePreferredAgent, caseId, orgservice, adminservice);
                    }
                }
#pragma warning restore SA1123 // Do not place regions within elements

                #endregion

#pragma warning disable SA1123 // Do not place regions within elements
                #region Delete

                if (context.MessageName.ToUpper() == "DELETE" && context.InputParameters.Contains("Target") && context.InputParameters["Target"] is EntityReference)
                {
                    Entity casePreferredAgentRetrive = context.PreEntityImages["PreImage"];
                    Guid caseId = ((EntityReference)casePreferredAgentRetrive.Attributes["jarvis_case"]).Id;
                    if (casePreferredAgentRetrive.Attributes.Contains("jarvis_case") && casePreferredAgentRetrive.Attributes["jarvis_case"] != null)
                    {
                        CasePreferredAgent cPA = new CasePreferredAgent();
                        cPA.DeleteCasePreferredAgent(initiatingUserID, casePreferredAgentRetrive, caseId, orgservice, adminservice);
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
