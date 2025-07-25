// <copyright file="PickRelease.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace MCS.Jarvis.CE.Plugins.Actions
{
    using System;
    using Microsoft.Crm.Sdk.Messages;
    using Microsoft.Xrm.Sdk;

    /// <summary>
    /// Pick Release Queue Item.
    /// </summary>
    public class PickRelease
    {
        /// <summary>
        /// Responsible for assigning worker to queue item.
        /// </summary>
        /// <param name="context">target entity context.</param>
        /// <param name="service">Org Service.</param>
        /// <param name="tracingService">Tracing Service.</param>
        public void AssignWorkertoQueueItem(IPluginExecutionContext context, IOrganizationService service, ITracingService tracingService)
        {
            if (context.InputParameters.Contains("QueueItemID") && context.InputParameters["QueueItemID"] != null
                    && context.InputParameters.Contains("UserID") && context.InputParameters["UserID"] != null
                    && context.InputParameters.Contains("PickOrRelease") && context.InputParameters["PickOrRelease"] != null)
            {
                bool pickOrRelease = (bool)context.InputParameters["PickOrRelease"];
                if (pickOrRelease)
                {
                    PickFromQueueRequest queueItem = new PickFromQueueRequest();
                    Guid.TryParse(context.InputParameters["QueueItemID"].ToString(), out Guid queueitemGuid);
                    queueItem.QueueItemId = queueitemGuid;
                    Guid.TryParse(context.InputParameters["UserID"].ToString(), out Guid useritemGuid);
                    queueItem.WorkerId = useritemGuid;
                    service.Execute(queueItem);
                    tracingService.Trace("AssignWorkertoQueueItem Ended");
                }
            }
        }
    }
}