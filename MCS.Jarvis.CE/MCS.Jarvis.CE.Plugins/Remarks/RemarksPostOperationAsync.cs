// <copyright file="RemarksPostOperationAsync.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MCS.Jarvis.CE.Plugins
{
    using System;
    using System.Globalization;
    using global::Plugins;
    using MCS.Jarvis.CE.BusinessProcessesShared.Helpers;
    using Microsoft.Xrm.Sdk;
    using static MCS.Jarvis.CE.BusinessProcessesShared.Helpers.Constants;

    /// <summary>
    /// Pass Out Translation Post Operation Async --> Register as Sync and with Mapping Field as Post Image.
    /// </summary>
    public class RemarksPostOperationAsync : PluginBase
    {
        /// <summary>
        /// Secure string.
        /// </summary>
        private readonly string secureString;

        /// <summary>
        /// unsecure string.
        /// </summary>
        private readonly string unsecureString;

        /// <summary>
        /// Initializes a new instance of the <see cref="RemarksPostOperationAsync"/> class.
        /// Remark Post Operation Constructor.
        /// </summary>
        /// <param name="unsecureString">unsecure String.</param>
        /// <param name="secureString">secure String.</param>
        /// <exception cref="InvalidPluginExecutionException">Invalid Plugin Execution Exception.</exception>
        public RemarksPostOperationAsync(string unsecureString, string secureString)
            : base(typeof(RemarksPostOperationAsync))
        {
            if (string.IsNullOrEmpty(secureString) && string.IsNullOrWhiteSpace(unsecureString))
            {
                throw new InvalidPluginExecutionException("Secure and unsecure strings are required for this plugin to execute.");
            }

            this.secureString = secureString;
            this.unsecureString = unsecureString;
        }

        /// <summary>
        /// Execute Remark Post Operation Plugin.
        /// </summary>
        /// <param name="localcontext">local context.</param>
        public override void ExecuteCrmPlugin(LocalPluginContext localcontext)
        {
            ITracingService traceService = localcontext.TracingService;
            traceService.Trace("Start RemarksPostOperationAsync:" + DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));
            try
            {
                IPluginExecutionContext context = localcontext.PluginExecutionContext;
                IOrganizationService orgService = localcontext.OrganizationService;
                Entity target = (context.InputParameters != null && context.InputParameters.Contains(General.Target) && context.InputParameters[General.Target] is Entity entity) ? entity : null;
                bool isMercuriusUser = CrmHelper.CheckUserIsMercurius(context.UserId, traceService, orgService);
                traceService.Trace($"IsMercuriusUserCheck: {isMercuriusUser}");
                if (target != null && context.MessageName.ToString().ToUpper() == PluginMessage.Create.ToUpper() && !isMercuriusUser)
                {
                    traceService.Trace("Inside Create Post Step");
                    if (target.Attributes.Contains(Remark.remarkSource) && target.Attributes[Remark.remarkSource] != null
                     && target.Attributes.Contains(Remark.regardingField)
                      && target.Attributes[Remark.regardingField] != null)
                    {
                        traceService.Trace("After Target Check,Source contains data and regardingobjectid contains data");
                        EntityReference referenceInc = (EntityReference)target.Attributes[Remark.regardingField];
                        OptionSetValue postSourceCode = (OptionSetValue)target.Attributes[Remark.remarkSource];
                        if (postSourceCode.Value == (int)PostSource.ManualPost && referenceInc.LogicalName == Incident.logicalName)
                        {
                            bool isCaseTypeBreakdown = CrmHelper.CheckCaseTypeIsBreakdown(referenceInc.Id, traceService, orgService);
                            traceService.Trace("Source eq manual and regardingobjectid is incident");
                            traceService.Trace("Case Type eq Breakdown " + isCaseTypeBreakdown.ToString() + " and incident id is " + referenceInc.Id);
                            if (isCaseTypeBreakdown)
                            {
                                CrmHelper.JarvisToFunction(target, string.Empty, traceService, this.unsecureString.ToString(), this.secureString.ToString());
                            }
                        }
                    }
                }
            }
            catch (InvalidPluginExecutionException pex)
            {
                traceService.Trace(pex.Message);
                traceService.Trace(pex.StackTrace);
            }
            finally
            {
                traceService.Trace("End RemarksPostOperationAsync:" + DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));

                // No Error Message Throwing Implictly to avoid blocking user because of integration.
            }
        }
    }
}
