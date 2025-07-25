// <copyright file="GOPPreOperationSync.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MCS.Jarvis.CE.Plugins
{
    using System;
    using global::Plugins;
    using MCS.Jarvis.CE.BusinessProcessesShared.GOP;
    using Microsoft.Xrm.Sdk;

    /// <summary>
    /// GOP Pre Operation Sync --> Register as Sync and with Mapping Field as Post Image.
    /// </summary>
    public class GOPPreOperationSync : PluginBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GOPPreOperationSync"/> class.
        /// GOP Post Operation Constructor.
        /// </summary>
        public GOPPreOperationSync()
            : base(typeof(GOPPreOperationSync))
        {
        }

        /// <summary>
        /// Execute Post Operation Plugin.
        /// </summary>
        /// <param name="localcontext">local context.</param>
        public override void ExecuteCrmPlugin(LocalPluginContext localcontext)
        {
            ITracingService traceService = localcontext.TracingService;
            IOrganizationService adminservice = localcontext.AdminOrganizationService;
            traceService.Trace("Start GOPPreOperationSync:" + DateTime.UtcNow.ToString("o", System.Globalization.CultureInfo.InvariantCulture));
            try
            {
                IPluginExecutionContext context = localcontext.PluginExecutionContext;
                IOrganizationService orgService = localcontext.OrganizationService;

                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                {
                    // region Pre-Create
                    if (context.Depth > 3)
                    {
                        return;
                    }

                    if (context.MessageName.ToUpper() == "CREATE")
                    {
                        Entity gop = (Entity)context.InputParameters["Target"];
                        Entity gopImg = (Entity)context.InputParameters["Target"];
                        GOPCalculation gopBusinessInstance = new GOPCalculation();
                        gopBusinessInstance.GopCreateOperations(gop, gopImg, orgService, traceService, adminservice);
                    }

                    // endregion
                    if (context.MessageName.ToUpper() == "UPDATE")
                    {
                        if (context.Depth > 1)
                        {
                            return;
                        }

                        Entity gop = (Entity)context.InputParameters["Target"];
                        Entity gopImg = (Entity)context.PreEntityImages["PreGOPImage"];
                        GOPCalculation gopBusinessInstance = new GOPCalculation();
                        gopBusinessInstance.GopUpdateOperations(gop, gopImg, orgService, traceService, adminservice);
                    }
                }
            }
            catch (InvalidPluginExecutionException ex)
            {
                traceService.Trace(ex.Message);
                throw new InvalidPluginExecutionException("Error in GOP Operations " + ex.Message + string.Empty);
            }
        }
    }
}
