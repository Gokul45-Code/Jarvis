// <copyright file="CaseTranslationPostOperationAsync.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace MCS.Jarvis.CE.Plugins
{
    using System;
    using System.Drawing;
    using System.Globalization;
    using System.ServiceModel;
    using global::Plugins;
    using MCS.Jarvis.CE.BusinessProcessesShared.Helpers;
    using Microsoft.Xrm.Sdk;
    using Newtonsoft.Json;
    using static MCS.Jarvis.CE.BusinessProcessesShared.Helpers.Constants;

    /// <summary>
    /// CaseTranslationPostOperationAsync --> Register as Sync and with Mapping Field as Post Image.
    /// </summary>
    public class CaseTranslationPostOperationAsync : PluginBase
    {
        /// <summary>
        /// secure String.
        /// </summary>
        private readonly string secureString;

        /// <summary>
        /// unSecure String.
        /// </summary>
        private readonly string unSecureString;

        /// <summary>
        /// Initializes a new instance of the <see cref="CaseTranslationPostOperationAsync"/> class.
        /// CaseTranslation (ReportFault) Operation Constructor.
        /// </summary>
        /// <param name="unsecureString">unsecure String.</param>
        /// <param name="secureString">secure String.</param>
        /// <exception cref="InvalidPluginExecutionException">Invalid Plugin Execution Exception.</exception>
        public CaseTranslationPostOperationAsync(string unsecureString, string secureString)
            : base(typeof(CaseTranslationPostOperationAsync))
        {
            if (string.IsNullOrEmpty(secureString) && string.IsNullOrWhiteSpace(unsecureString))
            {
                throw new InvalidPluginExecutionException("Secure strings are required for this plugin to execute.");
            }

            this.secureString = secureString;
            this.unSecureString = unsecureString;
        }

        /// <summary>
        /// Execute CaseTranslation (ReportFault) Post Operation Plugin.
        /// </summary>
        /// <param name="localcontext">local context.</param>
        public override void ExecuteCrmPlugin(LocalPluginContext localcontext)
        {
            ITracingService traceService = localcontext.TracingService;
            traceService.Trace("Start CaseTranslationPostOperationAsync:" + DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));
            try
            {
                traceService.Trace("Plugin Execution Started");
                IPluginExecutionContext context = localcontext.PluginExecutionContext;
                IOrganizationService orgService = localcontext.OrganizationService;
                //// No Need Mercurius Check Because Check has been handled in Power Automate Translation level.

#pragma warning disable SA1123 // Do not place regions within elements
                #region CREATE/UPDATE
                if (context.InputParameters.Contains(General.Target) && context.InputParameters[General.Target] is Entity entity)
                {
                    Entity targetEntity = new Entity();
                    bool isCheckPrePostImage = true;
                    if (context.MessageName.ToString().ToUpper() == PluginMessage.Create.ToUpper())
                    {
                        traceService.Trace("Inside Create case translation Step");
                        targetEntity = entity;
                    }
                    else if (context.MessageName.ToString().ToUpper() == PluginMessage.Update.ToUpper() && context.PostEntityImages["PostImage"] != null)
                    {
                        traceService.Trace("Inside Update case translation Step");
                        traceService.Trace($"preCompareImage : {context.PreEntityImages.Contains("ComparePreImage")}");
                        traceService.Trace($"postCompareImage : {context.PostEntityImages.Contains("ComparePostImage")}");
                        targetEntity = context.PostEntityImages["PostImage"];
                        if (context.PreEntityImages["ComparePreImage"] != null && context.PostEntityImages["ComparePostImage"] != null)
                        {
                            traceService.Trace($"CheckPrePostImage Method Started.");
                            isCheckPrePostImage = CrmHelper.CheckPrePostImage(context.PreEntityImages["ComparePreImage"], context.PostEntityImages["ComparePostImage"], traceService);
                            traceService.Trace($"CheckPrePostImage: {isCheckPrePostImage}");
                        }
                    }

                    Entity caseTransImg = context.PostEntityImages["PostImage"];
                    if (caseTransImg != null && caseTransImg.Attributes.Contains(Casecontact.jarvisIncident))
                    {
                        dynamic caseTransData = caseTransImg.Attributes;
                        string jsonObject = JsonConvert.SerializeObject(caseTransData) ?? string.Empty;
                        EntityReference incident = (EntityReference)caseTransImg.Attributes[Casecontact.jarvisIncident];
                        if (targetEntity.Attributes.Contains(CaseTranslation.Source) && targetEntity.Attributes[CaseTranslation.Source] != null && incident != null && !string.IsNullOrEmpty(incident.Id.ToString()))
                        {
                            traceService.Trace("After Target/PostImage Check,Source contains data");
                            bool isCaseTypeBreakdown = CrmHelper.CheckCaseTypeIsBreakdown(incident.Id, traceService, orgService);
                            traceService.Trace("Case Type eq Breakdown " + isCaseTypeBreakdown.ToString() + " and incident id is " + incident.Id);
                            OptionSetValue source = (OptionSetValue)targetEntity.Attributes[CaseTranslation.Source];
                            //// Souce is Equal to Jarvis then Sent to Mercurius, Not required any other check.
                            //// check Description contains in Target.
                            if (targetEntity.Attributes.Contains(CaseTranslation.Description))
                            {
                                traceService.Trace("Description contains in target/postImage");
                                if (source.Value == (int)Source.Jarvis && isCaseTypeBreakdown && isCheckPrePostImage)
                                {
                                    traceService.Trace("Source eq Jarvis, IsCaseTypeBreakdown is true and IsCheckPrePostImage is true");
                                    CrmHelper.JarvisToFunction(targetEntity, jsonObject, traceService, this.unSecureString.ToString(), this.secureString.ToString());
                                }
                            }
                        }
                    }
                }
#pragma warning restore SA1123 // Do not place regions within elements
                #endregion
            }
            catch (InvalidPluginExecutionException pex)
            {
                traceService.Trace(pex.Message);
                traceService.Trace(pex.StackTrace);
            }
            finally
            {
                traceService.Trace("End CaseTranslationPostOperationAsync:" + DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));
                ////No Error Message Throwing Implictly to avoid blocking user because of integration.
            }
        }
    }
}
