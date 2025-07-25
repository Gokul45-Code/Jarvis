// <copyright file="SetMonitorActionToCase.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace MCS.Jarvis.CE.Plugins.Actions
{
    using System;
    using global::Plugins;
    using MCS.Jarvis.CE.BusinessProcessesShared.Helpers;
    using MCS.Jarvis.CE.BusinessProcessShared.TranslationProcessAction;
    using Microsoft.Xrm.Sdk;
    using static MCS.Jarvis.CE.BusinessProcessesShared.Helpers.Constants;

    /// <summary>
    /// Forced Translation Action.
    /// </summary>
    public class ForcedTranslationAction : PluginBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ForcedTranslationAction"/> class.
        /// </summary>
        public ForcedTranslationAction()
            : base(typeof(ForcedTranslationAction))
        {
        }

        /// <summary>
        /// Execute CRM Plugin.
        /// </summary>
        /// <param name="localcontext">context.</param>
        /// <exception cref="InvalidPluginExecutionException">Plugin Execution Execption.</exception>
        public override void ExecuteCrmPlugin(LocalPluginContext localcontext)
        {
            // Extract the tracing service for use in debugging sandboxed plug-ins.
            ITracingService tracingService =
                localcontext.TracingService;

            // Obtain the execution context from the service provider.
            IPluginExecutionContext context = localcontext.PluginExecutionContext;
            int operation = 0;
            string entityName = string.Empty;
            try
            {
                if (context.InputParameters.Contains("Operation") && context.InputParameters["Operation"] != null && context.InputParameters.Contains("EntityName") && context.InputParameters["EntityName"] != null &&
                    context.InputParameters.Contains("IncidentId") && context.InputParameters["IncidentId"] != null && context.InputParameters.Contains("TargetEntityId") && context.InputParameters["TargetEntityId"] != null &&
                    context.InputParameters.Contains("TriggeredById") && context.InputParameters["TriggeredById"] != null)
                {
                    IOrganizationService adminService = localcontext.AdminOrganizationService;
                    operation = (int)context.InputParameters["Operation"];
                    entityName = (string)context.InputParameters["EntityName"];
                    string incidentId = (string)context.InputParameters["IncidentId"];
                    string targetEntity = (string)context.InputParameters["TargetEntityId"];
                    string triggeredBy = (string)context.InputParameters["TriggeredById"];
                    bool isAutomation = false;
                    isAutomation = CrmHelper.GetAutomationConfig(adminService, MCS.Jarvis.CE.Plugins.Constants.JarvisConfiguration.Automationtranslation, tracingService);
                    tracingService.Trace($"Started for Operation : {operation} and Entity : {entityName}");
                    if (isAutomation && operation > 0 && !string.IsNullOrEmpty(incidentId) && !string.IsNullOrEmpty(triggeredBy))
                    {
                        TranslationProcessAction translationProcessAction = new TranslationProcessAction();
                        Entity incident = new Entity(Incident.logicalName, Guid.Parse(incidentId));
                        if (!string.IsNullOrEmpty(entityName) && !string.IsNullOrEmpty(targetEntity))
                        {
                            switch (operation)
                            {
                                case 1:
                                    {
                                        context.OutputParameters["NeedTranslate"] = translationProcessAction.ForceHDCustomerCountryTranslation(adminService, tracingService, entityName, incident, Guid.Parse(targetEntity), Guid.Parse(triggeredBy));

                                        break;
                                    }

                                case 2:
                                    {
                                        context.OutputParameters["NeedTranslate"] = translationProcessAction.ForceCaseContactsTranslation(adminService, tracingService, entityName, incident, Guid.Parse(targetEntity), Guid.Parse(triggeredBy));

                                        break;
                                    }

                                case 3:
                                    {
                                        context.OutputParameters["NeedTranslate"] = translationProcessAction.ForcePassOutsTranslation(adminService, tracingService, entityName, incident, Guid.Parse(targetEntity), Guid.Parse(triggeredBy));

                                        break;
                                    }

                                case 4:
                                    {
                                        context.OutputParameters["NeedTranslate"] = translationProcessAction.ForceCaseQueryTranslation(adminService, tracingService, entityName, incident, Guid.Parse(targetEntity), Guid.Parse(triggeredBy));
                                        break;
                                    }

                                case 5:
                                    {
                                        context.OutputParameters["NeedTranslate"] = translationProcessAction.ForceGOPsTranslation(adminService, tracingService, entityName, incident, Guid.Parse(targetEntity), Guid.Parse(triggeredBy));
                                        break;
                                    }
                                default:
                                    {
                                        break;
                                    }
                            }
                        }
                    }
                }
                else
                {
                    tracingService.Trace($"ForcedTranslationAction: Input parameter : Operation/entityName/incident/targetEntity/triggeredBy are Missing");
                }
            }
            catch (InvalidPluginExecutionException oex)
            {
                tracingService.Trace(oex.Message);
                tracingService.Trace(oex.StackTrace);
                throw new InvalidPluginExecutionException($"ForcedTranslationAction: for Operation : {operation} and Entity : {entityName} :" + oex.Message);
            }
            finally
            {
                tracingService.Trace($"Exit ForcedTranslationAction: {operation}  and Entity :  {entityName}");
            }
        }
    }
}
