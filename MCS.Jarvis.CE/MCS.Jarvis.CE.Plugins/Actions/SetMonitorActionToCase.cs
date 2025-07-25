// <copyright file="SetMonitorActionToCase.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace MCS.Jarvis.CE.Plugins.Actions
{
    using System;
    using global::Plugins;
    using MCS.Jarvis.CE.BusinessProcessShared.CaseMonitor;
    using Microsoft.Xrm.Sdk;

    /// <summary>
    /// Set MonitorAction To Case.
    /// </summary>
    public class SetMonitorActionToCase : PluginBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SetMonitorActionToCase"/> class.
        /// </summary>
        public SetMonitorActionToCase()
            : base(typeof(SetMonitorActionToCase))
        {
        }

        /// <summary>
        /// Execute Crm Plugin.
        /// </summary>
        /// <param name="localcontext">Local Context.</param>
        /// <exception cref="InvalidPluginExecutionException">Invalid plugin execution exception.</exception>
        public override void ExecuteCrmPlugin(LocalPluginContext localcontext)
        {
            // Extract the tracing service for use in debugging sandboxed plug-ins.
            ITracingService tracingService =
                localcontext.TracingService;

            // Obtain the execution context from the service provider.
            IPluginExecutionContext context = localcontext.PluginExecutionContext;
            string incidentId = string.Empty;
            bool actionFlag = false;
            try
            {
                if (context.InputParameters.Contains("IncidentId") && context.InputParameters["IncidentId"] != null && context.InputParameters.Contains("ActionFlag"))
                {
                    IOrganizationService adminService = localcontext.AdminOrganizationService;
                    incidentId = (string)context.InputParameters["IncidentId"];
                    actionFlag = (bool)context.InputParameters["ActionFlag"];
                    tracingService.Trace($"Started : {incidentId}");
                    tracingService.Trace($"IncidentGuid : {Guid.Parse(incidentId)}");
                    tracingService.Trace($"ActionFlag : {actionFlag}");
                    Entity incident = new Entity("incident", Guid.Parse(incidentId));
                    CaseMonitorProcess monitorProcess = new CaseMonitorProcess();
                    monitorProcess.UpdateNextMOForCase(adminService, incident, tracingService, actionFlag);
                }
            }
            catch (InvalidPluginExecutionException oex)
            {
                tracingService.Trace(oex.Message);
                tracingService.Trace(oex.StackTrace);
                throw new InvalidPluginExecutionException($"SetMonitorActionToCase: {incidentId} :" + oex.Message);
            }
            finally
            {
                tracingService.Trace($"Exit SetMonitorActionToCase: {incidentId}");
            }
        }
    }
}
