// <copyright file="PickReleaseQueueItem.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace MCS.Jarvis.CE.Plugins.Actions
{
    using System;
    using System.ServiceModel;
    using Microsoft.Crm.Sdk.Messages;
    using Microsoft.Xrm.Sdk;
    using global::Plugins;

    /// <summary>
    /// Pick Release Queue Item.
    /// </summary>
    public class PickReleaseQueueItem : PluginBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PickReleaseQueueItem"/> class.
        /// Pick Release Queue Item:Constructor.
        /// </summary>
        public PickReleaseQueueItem()
            : base(typeof(PickReleaseQueueItem))
        {
        }

        /// <summary>
        /// Assign Case worker to Queue Worked by.
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
                PickRelease pickRelease = new PickRelease();
                tracingService.Trace("AssignWorkertoQueueItem Started");
                pickRelease.AssignWorkertoQueueItem(context, service, tracingService);
            }
            catch (InvalidPluginExecutionException oex)
            {
                tracingService.Trace(oex.Message);
                tracingService.Trace(oex.StackTrace);
                throw new InvalidPluginExecutionException(oex.Message + propName);
            }
        }
    }
}
