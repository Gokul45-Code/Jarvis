// <copyright file="CaseConatctSync.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace MCS.Jarvis.CE.Plugins.CaseContact
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.ServiceModel;
    using System.Text;
    using System.Threading.Tasks;
    using global::Plugins;
    using MCS.Jarvis.CE.BusinessProcessShared.CaseContact;
    using MCS.Jarvis.CE.BusinessProcessShared.CasePreferredAgent;
    using MCS.Jarvis.CE.Plugins.Case_Preferred_Agent;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;

    /// <summary>
    /// Case Contact Sync.
    /// </summary>
    public class CaseConatctSync : PluginBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CaseConatctSync"/> class.
        /// CaseContactSync.
        /// </summary>
        public CaseConatctSync()
            : base(typeof(CaseConatctSync))
        {
        }

        /// <summary>
        /// Execute CRM Plugin.
        /// </summary>
        /// <param name="localcontext">local context.</param>
        /// <exception cref="InvalidPluginExecutionException">throw Exception.</exception>
        public override void ExecuteCrmPlugin(LocalPluginContext localcontext)
        {
            // Extract the tracing service for use in debugging sandboxed plug-ins.
            ITracingService tracingService =
                localcontext.TracingService;

            // Obtain the execution context from the service provider.
            IPluginExecutionContext context = localcontext.PluginExecutionContext;

            // Organization Service
            IOrganizationService orgservice = localcontext.OrganizationService;
            IOrganizationService adminservice = localcontext.AdminOrganizationService;

            try
            {
#pragma warning disable SA1123 // Do not place regions within elements
                #region Update

                if (context.MessageName.ToUpper() == "UPDATE" && context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                {
                    Entity caseContact = (Entity)context.InputParameters["Target"];
                    Entity caseContactRetrieve = context.PostEntityImages["PostImage"];
                    if (caseContactRetrieve.Attributes.Contains("jarvis_incident") && caseContactRetrieve.Attributes["jarvis_incident"] != null)
                    {
                        Guid caseId = ((EntityReference)caseContactRetrieve.Attributes["jarvis_incident"]).Id;
                        if (caseId != Guid.Empty)
                        {
                            Entity incidentRetrieve = adminservice.Retrieve("incident", caseId, new ColumnSet("jarvis_callerphonenumbertype"));
                            CaseContactHelper caseContactHelper = new CaseContactHelper();
                            tracingService.Trace("Case Contact operation to update Incident");
                            caseContactHelper.UpdateIncident(caseContact, caseContactRetrieve, incidentRetrieve, orgservice, tracingService);
                        }
                    }
                }
#pragma warning restore SA1123 // Do not place regions within elements

                #endregion
            }
            catch (InvalidPluginExecutionException ex)
            {
                tracingService.Trace(ex.Message);
                tracingService.Trace(ex.StackTrace);
                throw new InvalidPluginExecutionException("Error in Case Contact Operations " + ex.Message);
            }
        }
    }
}
