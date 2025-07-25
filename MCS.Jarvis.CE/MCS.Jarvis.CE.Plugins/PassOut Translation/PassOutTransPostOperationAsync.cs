// <copyright file="PassOutTransPostOperationAsync.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MCS.Jarvis.CE.Plugins.PassOutTrans
{
    using System;
    using System.Globalization;
    using global::Plugins;
    using MCS.Jarvis.CE.BusinessProcessesShared.Helpers;
    using Microsoft.Xrm.Sdk;
    using Newtonsoft.Json;
    using static MCS.Jarvis.CE.BusinessProcessesShared.Helpers.Constants;

    /// <summary>
    /// PassOutTransPostOperationAsync --> Register as Sync and with Mapping Field as Post Image.
    /// </summary>
    public class PassOutTransPostOperationAsync : PluginBase
    {
        /// <summary>
        /// secure String.
        /// </summary>
        private readonly string secureString;

        /// <summary>
        /// un Secure String.
        /// </summary>
        private readonly string unsecureString;

        /// <summary>
        /// Initializes a new instance of the <see cref="PassOutTransPostOperationAsync"/> class.
        /// PassOut Translation Post Operation Constructor.
        /// </summary>
        /// <param name="unsecureString">unsecure String.</param>
        /// <param name="secureString">secure String.</param>
        /// <exception cref="InvalidPluginExecutionException">Invalid Plugin Execution Exception.</exception>
        public PassOutTransPostOperationAsync(string unsecureString, string secureString)
            : base(typeof(PassOutTransPostOperationAsync))
        {
            if (string.IsNullOrEmpty(secureString) && string.IsNullOrWhiteSpace(unsecureString))
            {
                throw new InvalidPluginExecutionException("Secure strings are required for this plugin to execute.");
            }

            this.secureString = secureString;
            this.unsecureString = unsecureString;
        }

        /// <summary>
        /// Execute Pass out Translation Post Operation Plugin.
        /// </summary>
        /// <param name="localcontext">local context.</param>
        public override void ExecuteCrmPlugin(LocalPluginContext localcontext)
        {
            ITracingService traceService = localcontext.TracingService;
            traceService.Trace("Start PassOutTransPostOperationAsync:" + DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));
            try
            {
                IPluginExecutionContext context = localcontext.PluginExecutionContext;
                IOrganizationService orgService = localcontext.AdminOrganizationService;

                // No Need Mercurius Check Because Check has been handled in Power Automate Translation level.
                // region CREATE/UPDATE
                if (context.InputParameters.Contains(General.Target) && context.InputParameters[General.Target] is Entity entity)
                {
                    Entity targetEntity = new Entity();
                    bool isCheckPrePostImage = true;

                    if (context.MessageName.ToString().ToUpper() == PluginMessage.Create.ToUpper())
                    {
                        traceService.Trace("Inside Create PassOut Translation Step");
                        targetEntity = entity;
                    }
                    else if (context.MessageName.ToString().ToUpper() == PluginMessage.Update.ToUpper() && context.PostEntityImages["PostImage"] != null)
                    {
                        traceService.Trace("Inside Update PassOut Translation Step");
                        targetEntity = context.PostEntityImages["PostImage"];
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

                    Entity passOutTransImg = context.PostEntityImages["PostImage"];
                    if (passOutTransImg != null && passOutTransImg.Attributes.Contains(Casecontact.jarvisCase))
                    {
                        dynamic passOutTransData = passOutTransImg.Attributes;
                        string jsonObject = JsonConvert.SerializeObject(passOutTransData) ?? string.Empty;
                        EntityReference incident = (EntityReference)passOutTransImg.Attributes[Casecontact.jarvisCase];

                        if (targetEntity.Attributes.Contains(PassOutTranslation.Source) && targetEntity.Attributes[PassOutTranslation.Source] != null && incident != null && !string.IsNullOrEmpty(incident.Id.ToString()))
                        {
                            traceService.Trace("After Target/PostImage Check,Source contains data");
                            bool isCaseTypeBreakdown = CrmHelper.CheckCaseTypeIsBreakdown(incident.Id, traceService, orgService);
                            traceService.Trace("Case Type eq Breakdown " + isCaseTypeBreakdown.ToString() + " and incident id is " + incident.Id);
                            OptionSetValue source = (OptionSetValue)targetEntity.Attributes[PassOutTranslation.Source];
                            //// Souce is Equal to Jarvis then Sent to Mercurius, Not required any other check.
                            //// Check DelayedETA contains in Translation.
                            if (targetEntity.Attributes.Contains(PassOutTranslation.EtaReason))
                            {
                                traceService.Trace("Delayed ETA reason contains data");
                                if (source.Value == (int)Source.Jarvis && isCaseTypeBreakdown && isCheckPrePostImage)
                                {
                                    traceService.Trace("Source eq Jarvis");
                                    CrmHelper.JarvisToFunction(targetEntity, jsonObject, traceService, this.unsecureString.ToString(), this.secureString.ToString());
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
            }
            finally
            {
                traceService.Trace("End PassOutTransPostOperationAsync:" + DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));

                // No Error Message Throwing Implictly to avoid blocking user because of integration.
            }
        }
    }
}
