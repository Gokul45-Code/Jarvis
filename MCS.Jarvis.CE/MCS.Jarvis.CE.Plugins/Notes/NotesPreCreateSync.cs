// <Copyright file="NotesPreCreateSync.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace MCS.Jarvis.CE.Plugins.Notes
{
    using System;
    using System.Globalization;
    using global::Plugins;
    using MCS.Jarvis.CE.BusinessProcessShared.CaseMonitor;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;

    /// <summary>
    /// NotesPreCreateSync.
    /// </summary>
    public class NotesPreCreateSync : PluginBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotesPreCreateSync"/> class.
        /// Notes Post Create Sync.
        /// </summary>
        public NotesPreCreateSync()
            : base(typeof(NotesPreCreateSync))
        {
        }

        /// <summary>
        /// Execute CRM Plugin.
        /// </summary>
        /// <param name="localcontext">local context.</param>
        /// <exception cref="Exception">Exception details.</exception>
        public override void ExecuteCrmPlugin(LocalPluginContext localcontext)
        {
            // Extract the tracing service for use in debugging sandboxed plug-ins.
            ITracingService tracingService =
                localcontext.TracingService;

            // Obtain the execution context from the service provider.
            IPluginExecutionContext context = localcontext.PluginExecutionContext;

            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                try
                {
                    Entity annotation = (Entity)context.InputParameters["Target"];
                    IOrganizationService service = localcontext.OrganizationService;

                    if (annotation.Attributes.Contains("objectid") && annotation.Attributes["objectid"] != null)
                    {
                        int incrementNumber = 1;
                        EntityReference incident = (EntityReference)annotation.Attributes["objectid"];
                        EntityCollection getNotesForCase = service.RetrieveMultiple(new FetchExpression(string.Format(MCS.Jarvis.CE.Plugins.Constants.FetchXmls.getNotesForCase, incident.Id)));
                        if (getNotesForCase.Entities.Count > 0)
                        {
                            incrementNumber = getNotesForCase.Entities.Count + 1;
                        }

                        if (annotation.Attributes.Contains("subject") && annotation.Attributes["subject"] != null)
                        {
                            string subject = (string)annotation.Attributes["subject"];
                            subject = "N" + incrementNumber.ToString() + ":" + " " + subject;
                            annotation["subject"] = subject;
                        }
                        else
                        {
                            annotation["subject"] = "N" + incrementNumber.ToString();
                        }
                    }
                }
                catch (InvalidPluginExecutionException ex)
                {
                    tracingService.Trace(ex.Message);
                    tracingService.Trace(ex.StackTrace);
                    throw new Exception("Error in NotesPreOperationSync " + ex.Message);
                }
                finally
                {
                    tracingService.Trace("End NotesPreOperationSync:" + DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));

                    // No Error Message Throwing Implictly to avoid blocking user because of integration.
                }
            }
        }
    }
}
