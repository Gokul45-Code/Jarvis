// <copyright file="JobEndDetailsTransPostOperation.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace MCS.Jarvis.IntegrationPlugin
{
    using System;
    using System.Globalization;
    using MCS.Jarvis.CE.BusinessProcessesShared.Helpers;
    using Microsoft.Xrm.Sdk;
    using Newtonsoft.Json;
    using Plugins;
    using static MCS.Jarvis.CE.BusinessProcessesShared.Helpers.Constants;

    /// <summary>
    /// Incident Post Operation Async --> Register as Sync and with Mapping Field as Post Image.
    /// </summary>
    public class JobEndDetailsTransPostOperation : PluginBase
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
        /// Initializes a new instance of the <see cref="JobEndDetailsTransPostOperation"/> class.
        /// Job End Details Translation Constructor.
        /// </summary>
        /// <param name="unsecureString">unsecure String.</param>
        /// <param name="secureString">secure eString.</param>
        /// <exception cref="InvalidPluginExecutionException">Invalid Plugin Execution Exception.</exception>
        public JobEndDetailsTransPostOperation(string unsecureString, string secureString)
            : base(typeof(JobEndDetailsTransPostOperation))
        {
            if (string.IsNullOrEmpty(secureString) && string.IsNullOrWhiteSpace(unsecureString))
            {
                throw new InvalidPluginExecutionException("Secure strings are required for this plugin to execute.");
            }

            this.secureString = secureString;
            this.unsecureString = unsecureString;
        }

        /// <summary>
        /// Execute Job End Details Translation Plugin.
        /// </summary>
        /// <param name="localcontext">local context.</param>
        /// <exception cref="InvalidPluginExecutionException">Invalid Plugin Execution Exception.</exception>
        public override void ExecuteCrmPlugin(LocalPluginContext localcontext)
        {
            // Extract the tracing service for use in debugging sandboxed plug-ins.
            ITracingService traceService = localcontext.TracingService;
            traceService.Trace("Start JobEndDetailsTransPostOperationAsync:" + DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));
            try
            {
                // Obtain the execution context from the service provider.
                IPluginExecutionContext context = localcontext.PluginExecutionContext;
                IOrganizationService orgService = localcontext.OrganizationService;

                // No Need Mercurius Check Because Check has been handled in Power Automate Translation level.
                // region CREATE/UPDATE
                if (context.InputParameters.Contains(General.Target) && context.InputParameters[General.Target] is Entity entity)
                {
                    Entity targetEntity = new Entity();
                    Entity preImage = new Entity();
                    Entity postImage = new Entity();
                    bool isCheckPrePostImage = true;
                    if (context.MessageName.ToString().ToUpper() == PluginMessage.Create.ToUpper())
                    {
                        traceService.Trace("Inside Create Job End Detail Translation Step");
                        targetEntity = entity;
                    }
                    else if (context.MessageName.ToString().ToUpper() == PluginMessage.Update.ToUpper() && context.PostEntityImages["PostImage"] != null)
                    {
                        traceService.Trace("Inside Update Job End details Translation Step");
                        traceService.Trace($"preCompareImage : {context.PreEntityImages.Contains("ComparePreImage")}");
                        traceService.Trace($"postCompareImage : {context.PostEntityImages.Contains("ComparePostImage")}");
                        targetEntity = context.PostEntityImages["PostImage"];
                        if (context.PreEntityImages["ComparePreImage"] != null && context.PostEntityImages["ComparePostImage"] != null)
                        {
                            traceService.Trace($"CheckPrePostImage Method Started.");
                            preImage = context.PreEntityImages["ComparePreImage"];
                            postImage = context.PostEntityImages["ComparePostImage"];
                            isCheckPrePostImage = CrmHelper.CheckPrePostImage(preImage, postImage, traceService);
                            traceService.Trace($"CheckPrePostImage: {isCheckPrePostImage}");
                        }
                    }

                    Entity jobEndTransImg = context.PostEntityImages["PostImage"];
                    if (jobEndTransImg != null && jobEndTransImg.Attributes.Contains(Casecontact.jarvisCase))
                    {
                        dynamic jobEndTransData = jobEndTransImg.Attributes;
                        string jsonObject = JsonConvert.SerializeObject(jobEndTransData) ?? string.Empty;
                        EntityReference incident = (EntityReference)jobEndTransImg.Attributes[Casecontact.jarvisCase];

                        if (targetEntity.Attributes.Contains(JobEndTranslation.Source) && targetEntity.Attributes[JobEndTranslation.Source] != null && incident != null && !string.IsNullOrEmpty(incident.Id.ToString()))
                        {
                            bool isCaseTypeBreakdown = CrmHelper.CheckCaseTypeIsBreakdown(incident.Id, traceService, orgService);
                            traceService.Trace("Case Type eq Breakdown " + isCaseTypeBreakdown.ToString() + " and incident id is " + incident.Id);
                            traceService.Trace("After Target/PostImage Check,Source contains data");
                            OptionSetValue source = (OptionSetValue)targetEntity.Attributes[JobEndTranslation.Source];
                            //// Souce is Equal to Jarvis then Sent to Mercurius, Not required any other check.
                            if (source.Value == (int)Source.Jarvis && isCaseTypeBreakdown && isCheckPrePostImage)
                            {
                                traceService.Trace("Source eq Jarvis");
                                if (context.MessageName.ToString().ToUpper() == PluginMessage.Create.ToUpper())
                                {
                                    if (entity.Attributes.Contains(JobEndTranslation.ActualCauseFault) || entity.Attributes.Contains(JobEndTranslation.TemporaryRepair))
                                    {
                                        traceService.Trace("Inside Create JobEndTranslation Step");
                                        CrmHelper.JarvisToFunction(targetEntity, jsonObject, traceService, this.unsecureString.ToString(), this.secureString.ToString(), TranslationType.Create);
                                    }
                                }
                                else if (context.MessageName.ToString().ToUpper() == PluginMessage.Update.ToUpper())
                                {
                                    if (entity.Attributes.Contains(JobEndTranslation.ActualCauseFault))
                                    {
                                        if (preImage.Attributes.Contains(JobEndTranslation.ActualCauseFault) && !preImage.GetAttributeValue<string>(JobEndTranslation.ActualCauseFault).Equals(postImage.GetAttributeValue<string>(JobEndTranslation.ActualCauseFault)))
                                        {
                                            traceService.Trace("Actual Fault Cause Updated");
                                            CrmHelper.JarvisToFunction(targetEntity, jsonObject, traceService, this.unsecureString.ToString(), this.secureString.ToString(), TranslationType.ActualFault);
                                        }
                                        else if (!preImage.Attributes.Contains(JobEndTranslation.ActualCauseFault))
                                        {
                                            traceService.Trace("First Actual Fault Cause Updated");
                                            CrmHelper.JarvisToFunction(targetEntity, jsonObject, traceService, this.unsecureString.ToString(), this.secureString.ToString(), TranslationType.ActualFault);
                                        }
                                    }

                                    if (entity.Attributes.Contains(JobEndTranslation.TemporaryRepair))
                                    {
                                        if (preImage.Attributes.Contains(JobEndTranslation.TemporaryRepair) && !preImage.GetAttributeValue<string>(JobEndTranslation.TemporaryRepair).Equals(postImage.GetAttributeValue<string>(JobEndTranslation.TemporaryRepair)))
                                        {
                                            traceService.Trace("Temporary Repair Updated");
                                            CrmHelper.JarvisToFunction(targetEntity, jsonObject, traceService, this.unsecureString.ToString(), this.secureString.ToString(), TranslationType.TempRepair);
                                        }
                                        else if (!preImage.Attributes.Contains(JobEndTranslation.TemporaryRepair))
                                        {
                                            traceService.Trace("First Temporary Repair Updated");
                                            CrmHelper.JarvisToFunction(targetEntity, jsonObject, traceService, this.unsecureString.ToString(), this.secureString.ToString(), TranslationType.TempRepair);
                                        }
                                    }
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
                traceService.Trace("End JobEndDetailsTransPostOperationAsync:" + DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));

                // No Error Message Throwing Implictly to avoid blocking user because of integration.
            }
        }
    }
}
