// <copyright file="CaseResolutionSync.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace MCS.Jarvis.CE.Plugins
{
    using global::Plugins;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;

    /// <summary>
    /// Case Resolution Sync.
    /// </summary>
    public class CaseResolutionSync : PluginBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CaseResolutionSync"/> class.
        /// CaseResolutionSync.
        /// </summary>
        public CaseResolutionSync()
            : base(typeof(CaseResolutionSync))
        {
        }

        /// <summary>
        /// Execute CRM Plugin.
        /// </summary>
        /// <param name="localcontext">local context.</param>
        /// <exception cref="InvalidPluginExecutionException">exception thrown.</exception>
        public override void ExecuteCrmPlugin(LocalPluginContext localcontext)
        {
            // Extract the tracing service for use in debugging sandboxed plug-ins.
            ITracingService tracingService =
                localcontext.TracingService;

            // Obtain the execution context from the service provider.
            IPluginExecutionContext context = localcontext.PluginExecutionContext;

            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                tracingService.Trace("Entered");
                Entity caseResolution = (Entity)context.InputParameters["Target"];

                try
                {
                    if (caseResolution.Attributes.Contains("resolutiontypecode") && caseResolution.Attributes["resolutiontypecode"] != null)
                    {
                        IOrganizationService service = localcontext.OrganizationService;
                        OptionSetValue resolutionType = (OptionSetValue)caseResolution.Attributes["resolutiontypecode"];

                        // Force Closed
                        if (resolutionType.Value == 1000 || resolutionType.Value == 5)
                        {
                            if (caseResolution.Attributes.Contains("incidentid") && caseResolution.Attributes["incidentid"] != null)
                            {
                                EntityReference incident = (EntityReference)caseResolution.Attributes["incidentid"];
                                Entity parentCase = service.Retrieve(incident.LogicalName, incident.Id, new ColumnSet("casetypecode"));
                                OptionSetValue caseTypeCode = new OptionSetValue();

                                // Update Case
                                Entity caseToUpdate = new Entity(incident.LogicalName);
                                caseToUpdate.Id = incident.Id;
                                caseToUpdate["jarvis_mercuriusstatus"] = new OptionSetValue(900);
                                if (parentCase.Attributes.Contains("casetypecode") && parentCase.Attributes["casetypecode"] != null)
                                {
                                    caseTypeCode = (OptionSetValue)parentCase.Attributes["casetypecode"];
                                }

                                if (caseTypeCode.Value == 2)
                                {
                                    EntityCollection user = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getUnAssignedUser)));
                                    if (user.Entities.Count > 0)
                                    {
                                        caseToUpdate["ownerid"] = user.Entities[0].ToEntityReference();
                                    }
                                }

                                service.Update(caseToUpdate);
                            }
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
