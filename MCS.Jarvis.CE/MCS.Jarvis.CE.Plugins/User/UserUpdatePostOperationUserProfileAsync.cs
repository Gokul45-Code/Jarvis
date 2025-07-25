// <copyright file="UserUpdatePostOperationUserProfileAsync.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MCS.Jarvis.CE.Plugins
{
    using System;
    using global::Plugins;
    using MCS.Jarvis.CE.BusinessProcessesShared.User;
    using Microsoft.Xrm.Sdk;

    /// <summary>
    /// User Update Post Operation User Profile Async.
    /// </summary>
    public class UserUpdatePostOperationUserProfileAsync : PluginBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserUpdatePostOperationUserProfileAsync"/> class.
        /// </summary>
        public UserUpdatePostOperationUserProfileAsync()
            : base(typeof(UserUpdatePostOperationUserProfileAsync))
        {
        }

        /// <summary>
        /// Execute CRM Plugin.
        /// </summary>
        /// <param name="localcontext">local context.</param>
        /// <exception cref="InvalidPluginExecutionException">Invalid Plugin Execution.</exception>
        public override void ExecuteCrmPlugin(LocalPluginContext localcontext)
        {
            ITracingService traceService = localcontext.TracingService;
            traceService.Trace("Start GOPPreOperationSync:" + DateTime.UtcNow.ToString("o", System.Globalization.CultureInfo.InvariantCulture));
            try
            {
                IPluginExecutionContext context = localcontext.PluginExecutionContext;
                IOrganizationService orgService = localcontext.OrganizationService;
                if (context.Depth > 1)
                {
                    return;
                }

                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                {
                    if (context.MessageName.ToUpper() == "UPDATE")
                    {
                        Entity userUpdate = (Entity)context.InputParameters["Target"];
                        Entity userImg = (Entity)context.PreEntityImages["PreUserImage"];
                        UserOperation userOperationInstance = new UserOperation();
                        userOperationInstance.UserUpdateOperation(userUpdate, userImg, orgService, traceService);
                    }
                }
            }
            catch (Exception ex)
            {
                traceService.Trace(ex.Message);
                throw new InvalidPluginExecutionException("Error in GOP Operations " + ex.Message + string.Empty);
            }
        }
    }
}
