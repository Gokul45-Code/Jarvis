// <Copyright file="RemarksPreCreateSync.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace MCS.Jarvis.CE.Plugins.Remarks
{
    using System;
    using System.Globalization;
    using global::Plugins;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;

    /// <summary>
    /// RemarksPreCreateSync.
    /// </summary>
    public class RemarksPreCreateSync : PluginBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RemarksPreCreateSync"/> class.
        /// RemarksPreCreateSync.
        /// </summary>
        public RemarksPreCreateSync()
            : base(typeof(RemarksPreCreateSync))
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
                    Entity post = (Entity)context.InputParameters["Target"];
                    IOrganizationService service = localcontext.OrganizationService;
                    if (post.Attributes.Contains("source") && post.Attributes["source"] != null)
                    {
                        OptionSetValue source = (OptionSetValue)post.Attributes["source"];
                        if (source.Value == 2) // Manual Post
                        {
                            if (post.Attributes.Contains("regardingobjectid") && post.Attributes["regardingobjectid"] != null)
                            {
                                int incrementNumber = 1;
                                EntityReference incident = (EntityReference)post.Attributes["regardingobjectid"];
                                EntityCollection getPostsForCase = service.RetrieveMultiple(new FetchExpression(string.Format(MCS.Jarvis.CE.Plugins.Constants.FetchXmls.getPostsForCase, incident.Id)));
                                if (getPostsForCase.Entities.Count > 0)
                                {
                                    incrementNumber = getPostsForCase.Entities.Count + 1;
                                }

                                if (post.Attributes.Contains("largetext") && post.Attributes["largetext"] != null)
                                {
                                    string subject = (string)post.Attributes["largetext"];
                                    subject = "P" + incrementNumber.ToString() + ":" + " " + subject;
                                    post["largetext"] = subject;
                                }
                                else
                                {
                                    post["largetext"] = "P" + incrementNumber.ToString();
                                }
                            }
                        }
                    }
                }
                catch (InvalidPluginExecutionException ex)
                {
                    tracingService.Trace(ex.Message);
                    tracingService.Trace(ex.StackTrace);
                    throw new Exception("Error in PostPreOperationSync " + ex.Message);
                }
                finally
                {
                    tracingService.Trace("End PostPreOperationSync:" + DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));

                    // No Error Message Throwing Implictly to avoid blocking user because of integration.
                }
            }
        }
    }
}
