// <copyright file="AnnotationPostOperation.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace MCS.Jarvis.CE.IntegrationPlugin
{
    using System;
    using System.Globalization;
    using global::Plugins;
    using MCS.Jarvis.CE.BusinessProcessesShared.Helpers;
    using Microsoft.Xrm.Sdk;
    using Newtonsoft.Json;
    using static MCS.Jarvis.CE.BusinessProcessesShared.Helpers.Constants;

    /// <summary>
    /// Annotation Post Create
    /// Create - Async
    /// Update - Sync Execution order 2
    /// Mapping Attribute as Post Image.
    /// </summary>
    public class AnnotationPostOperation : PluginBase
    {
        /// <summary>
        /// secure string.
        /// </summary>
        private readonly string secureString;

        /// <summary>
        /// unsecure string.
        /// </summary>
        private readonly string unsecureString;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnnotationPostOperation"/> class.
        /// AnnotationPostCreate Constructor.
        /// </summary>
        /// <param name="unsecureString">unsecure String.</param>
        /// <param name="secureString">secure String.</param>
        /// <exception cref="InvalidPluginExecutionException">Invalid Plugin Execution Exception.</exception>
        public AnnotationPostOperation(string unsecureString, string secureString)
            : base(typeof(AnnotationPostOperation))
        {
            if (string.IsNullOrEmpty(secureString) && string.IsNullOrWhiteSpace(unsecureString))
            {
                throw new InvalidPluginExecutionException("Secure strings are required for this plugin to execute.");
            }

            this.secureString = secureString;
            this.unsecureString = unsecureString;
        }

        /// <summary>
        /// Execute Annotation Post Create Plugin.
        /// </summary>
        /// <param name="localcontext">local context.</param>
        /// <exception cref="InvalidPluginExecutionException">Invalid Plugin Execution Exception.</exception>
        public override void ExecuteCrmPlugin(LocalPluginContext localcontext)
        {
            // Extract the tracing service for use in debugging sandboxed plug-ins.
            ITracingService traceService = localcontext.TracingService;
            traceService.Trace("Start AnnotationPostCreate:" + DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));
            try
            {
                // Obtain the execution context from the service provider.
                IPluginExecutionContext context = localcontext.PluginExecutionContext;
                IOrganizationService orgService = localcontext.OrganizationService;

                // region CREATE/Update
                if (context.InputParameters.Contains(General.Target) && context.InputParameters[General.Target] is Entity)
                {
                    traceService.Trace("Enter into Create/update after condition ");
                    IOrganizationService service = localcontext.AdminOrganizationService;
                    bool isMercuriusUser = CrmHelper.CheckUserIsMercurius(context.UserId, traceService, service);
                    traceService.Trace($"Mercurius User : {isMercuriusUser}");

                    if (context.MessageName.ToString().ToUpper() == PluginMessage.Create.ToUpper() || (context.MessageName.ToString().ToUpper() == PluginMessage.Update.ToUpper() && context.PostEntityImages["PostImage"] != null))
                    {
                        traceService.Trace("Inside Create/Update AnnotationPost Step");
                        Entity annotationPostImage = context.PostEntityImages["PostImage"];
                        traceService.Trace("Check RegardingObject ID is present pr not for Note activity");
                        if (annotationPostImage != null && annotationPostImage.Attributes.Contains(Notes.regardingObjectId) && annotationPostImage.Attributes.Contains(Notes.isdocument))
                        {
                            traceService.Trace("Checked RegardingObject ID is present for Note activity");
                            dynamic annotationTransData = annotationPostImage.Attributes;
                            string jsonObject = JsonConvert.SerializeObject(annotationTransData) ?? string.Empty;
                            traceService.Trace($"Entity Data : {jsonObject}");
                            EntityReference incident = (EntityReference)annotationPostImage.Attributes[Notes.regardingObjectId];
                            bool isDocument = (bool)annotationPostImage.Attributes[Notes.isdocument];

                            if (incident != null && incident.LogicalName.ToUpper() == Incident.logicalName.ToUpper() && !string.IsNullOrEmpty(incident.Id.ToString()))
                            {
                                traceService.Trace("Case Monitor Action Activity having Incident as Regarding Object");
                                bool isCaseTypeBreakdown = CrmHelper.CheckCaseTypeIsBreakdown(incident.Id, traceService, orgService);
                                traceService.Trace("Case Type eq Breakdown " + isCaseTypeBreakdown.ToString() + " and incident id is " + incident.Id);
                                //// IsCaseType == Breakdown, Calling user is not by Mercurius user and IsDocument Available against the note.
                                if (isCaseTypeBreakdown && !isMercuriusUser && isDocument)
                                {
                                    traceService.Trace("Case Type eq Breakdown, CallingUser ne mercurius user and IsDcoument avaliable");
                                    CrmHelper.JarvisToFunction(annotationPostImage, jsonObject, traceService, this.unsecureString.ToString(), this.secureString.ToString());
                                }
                            }
                        }
                    }
                }

                // endregion
            }
            catch (InvalidPluginExecutionException pex)
            {
                traceService.Trace(pex.Message);
                traceService.Trace(pex.StackTrace);
                ////No Error Message Throwing Implictly to avoid blocking user because of integration.
            }
            finally
            {
                traceService.Trace("End AnnotationPostCreate:" + DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));

                // No Error Message Throwing Implictly to avoid blocking user because of integration.
            }
        }
    }
}
