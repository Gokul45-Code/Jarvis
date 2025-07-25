// <copyright file="CaseClosureAutomation.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace MCS.Jarvis.CE.Plugins.Actions
{
    using System;
    using global::Plugins;
    using MCS.Jarvis.CE.BusinessProcessesShared.Helpers;
    using MCS.Jarvis.CE.BusinessProcessShared.CaseMonitor;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;

    /// <summary>
    /// CaseClosureAutomation.
    /// </summary>
    public class CaseClosureAutomation : PluginBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CaseClosureAutomation"/> class.
        /// CaseClosureAutomation:Constructor.
        /// </summary>
        public CaseClosureAutomation()
            : base(typeof(ComposeCommunicationTemplate))
        {
        }

        /// <summary>
        /// Compose Email Body for Template.
        /// </summary>
        /// <param name="localcontext">local context.</param>
        /// <exception cref="InvalidPluginExecutionException">Exception thrown.</exception>
        public override void ExecuteCrmPlugin(LocalPluginContext localcontext)
        {
            // Extract the tracing service for use in debugging sandboxed plug-ins.
            ITracingService tracingService =
                localcontext.TracingService;

            // Obtain the execution context from the service provider.
            IPluginExecutionContext context = localcontext.PluginExecutionContext;

            try
            {
                tracingService.Trace("CaseClosureAutomation Action started");

                if (context.InputParameters.Contains("CaseID") && context.InputParameters["CaseID"] != null)
                {
                    IOrganizationService service = localcontext.OrganizationService;
                    Guid incidentId = new Guid((string)context.InputParameters["CaseID"]);

                    // Parent Case
                    if (incidentId != Guid.Empty)
                    {
                        tracingService.Trace("Set Customer Informed");
                        this.SetCustomerInformed(service, incidentId);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException(ex.InnerException.Message);
            }
        }

        /// <summary>
        /// Set Customer Informed if :
        /// at least one jarvis job end details status reason = "has been sent" for each pass out
        /// AND all email activity are “closed”
        /// AND “Case Status” = “Case Closure”.
        /// </summary>
        /// <param name="orgService">service Instance.</param>
        /// <param name="caseID">Case ID.</param>
        public void SetCustomerInformed(IOrganizationService orgService, Guid caseID)
        {
            // CaseActivePassouts
            bool isJEDopen = false;
            EntityCollection passouts = orgService.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.CaseActivePassouts, caseID)));
            if (passouts.Entities.Count > 0)
            {
                foreach (var item in passouts.Entities)
                {
                    EntityCollection jed = orgService.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getHasBeenSentJEDs, caseID, item.Id)));
                    if (jed.Entities.Count > 0)
                    {
                        Guid jedID = Guid.Empty;
                        foreach (var jItem in jed.Entities)
                        {
                            EntityCollection completedEmails = orgService.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getCaseEmailsCompleted, jItem.Id)));
                            if (completedEmails.Entities.Count == 0)
                            {
                                isJEDopen = true;
                            }
                        }
                    }
                    else
                    {
                        isJEDopen = true;
                    }
                }
            }

            if (isJEDopen == false)
            {
                Entity parentCase = new Entity("incident");
                parentCase.Id = caseID;

                CaseMonitorProcess operation = new CaseMonitorProcess();
                string fucomments = "Pass ATC,Pass JED,Chase JEDS,Chase Diagnose";
                operation.AutomateCloseMonitorActions(parentCase, fucomments, 1, fucomments, orgService);

                parentCase["jarvis_customerinformed"] = true;
                orgService.Update(parentCase);
            }
        }
    }
}
