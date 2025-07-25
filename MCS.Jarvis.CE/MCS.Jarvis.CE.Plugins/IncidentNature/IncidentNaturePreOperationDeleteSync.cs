// <copyright file="IncidentNaturePreOperationDeleteSync.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MCS.Jarvis.CE.Plugins.IncidentNature
{
    using System;
    using System.Globalization;
    using global::Plugins;
    using MCS.Jarvis.CE.BusinessProcessShared.Case;
    using Microsoft.Xrm.Sdk;

    /// <summary>
    /// Incident Nature Pre Operation Delete Sync.
    /// </summary>
    public class IncidentNaturePreOperationDeleteSync : PluginBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IncidentNaturePreOperationDeleteSync"/> class.
        /// </summary>
        public IncidentNaturePreOperationDeleteSync()
            : base(typeof(IncidentNaturePreOperationDeleteSync))
        {
        }

        /// <summary>
        /// Execute CRM Plugin.
        /// </summary>
        /// <param name="localcontext">local context.</param>
        /// <exception cref="InvalidPluginExecutionException">Invalid Plugin Execution Exception.</exception>
        /// <exception cref="Exception">Exception details.</exception>
        public override void ExecuteCrmPlugin(LocalPluginContext localcontext)
        {
            ITracingService traceService = localcontext.TracingService;
            traceService.Trace("Start IncidentNaturePreOperationDeleteSync:" + DateTime.UtcNow.ToString("o", System.Globalization.CultureInfo.InvariantCulture));
            try
            {
                IPluginExecutionContext context = localcontext.PluginExecutionContext;
                IOrganizationService orgService = localcontext.OrganizationService;

                // region Delete
                if (context.MessageName.ToUpper() == "DELETE")
                {
                    if (context.InputParameters.Contains("Relationship"))
                    {
                        // Get the Relationship name for which this plugin fired
                        string relationshipName = ((Relationship)context.InputParameters["Relationship"]).SchemaName;
                    }
                }

                // endregion
                // region Associate/Disassociate
                if (context.MessageName.ToLower() == "associate" || context.MessageName.ToLower() == "disassociate")
                {
                    Incidentnature incidentnature = new Incidentnature();
                    incidentnature.AssociateIncidentNatureFromSubgridToMultiselect(orgService, context);
                }

                // endregion
            }
            catch (InvalidPluginExecutionException pex)
            {
                traceService.Trace(pex.Message);
                traceService.Trace(pex.StackTrace);
                throw new InvalidPluginExecutionException("Error in IncidentNaturePreOperationDeleteSync " + pex.Message);
            }
            finally
            {
                traceService.Trace("End IncidentNaturePreOperationDeleteSync:" + DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));

                // No Error Message Throwing Implictly to avoid blocking user because of integration.
            }
        }
    }
}
