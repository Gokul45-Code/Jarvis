// <copyright file="CaseRepairInfoTransPostOperationAsync.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace MCS.Jarvis.CE.Plugins
{
    using System;
    using System.Globalization;
    using System.ServiceModel;
    using global::Plugins;
    using MCS.Jarvis.CE.BusinessProcessesShared.Helpers;
    using Microsoft.Xrm.Sdk;
    using Newtonsoft.Json;
    using static MCS.Jarvis.CE.BusinessProcessesShared.Helpers.Constants;

    /// <summary>
    /// CaseRepairInfoTransPostOperationAsync --> Register as Sync and with Mapping Field as Post Image.
    /// </summary>
    public class CaseRepairInfoTransPostOperationAsync : PluginBase
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
        /// Initializes a new instance of the <see cref="CaseRepairInfoTransPostOperationAsync"/> class.
        ///  Repair Info Translation Constructor.
        /// </summary>
        /// <param name="unsecureString">UnSecure String.</param>
        /// <param name="secureString">Secure String.</param>
        /// <exception cref="InvalidPluginExecutionException">Invalid Plugin Execution Exception.</exception>
        public CaseRepairInfoTransPostOperationAsync(string unsecureString, string secureString)
            : base(typeof(CaseRepairInfoTransPostOperationAsync))
        {
            if (string.IsNullOrEmpty(secureString) && string.IsNullOrWhiteSpace(unsecureString))
            {
                throw new InvalidPluginExecutionException("Secure strings are required for this plugin to execute.");
            }

            this.secureString = secureString;
            this.unSecureString = unsecureString;
        }

        /// <summary>
        ///  Execute Repair Info Post Operation Plugin.
        /// </summary>
        /// <param name="localcontext">Local Context.</param>
        public override void ExecuteCrmPlugin(LocalPluginContext localcontext)
        {
            ITracingService traceService = localcontext.TracingService;
            traceService.Trace("Start CaseRepairInfoTransPostOperationAsync:" + DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));
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
                    Entity preImage = new Entity();
                    Entity postImage = new Entity();
                    bool isCheckPrePostImage = true;
                    if (context.MessageName.ToString().ToUpper() == PluginMessage.Create.ToUpper())
                    {
                        traceService.Trace("Inside Create case translation Step");
                        targetEntity = entity;
                    }
                    else if (context.MessageName.ToString().ToUpper() == PluginMessage.Update.ToUpper() && context.PostEntityImages["PostImage"] != null)
                    {
                        traceService.Trace("Inside Update RepairInfoTranslation Step");
                        traceService.Trace($"preCompareImage : {context.PreEntityImages.Contains("ComparePreImage")}");
                        traceService.Trace($"postCompareImage : {context.PostEntityImages.Contains("ComparePostImage")}");
                        targetEntity = context.PostEntityImages["PostImage"];
                        traceService.Trace($"RepairInfoTransId: {targetEntity} ");
                        if (context.PreEntityImages["ComparePreImage"] != null && context.PostEntityImages["ComparePostImage"] != null)
                        {
                            traceService.Trace($"CheckPrePostImage Method Started.");
                            preImage = context.PreEntityImages["ComparePreImage"];
                            postImage = context.PostEntityImages["ComparePostImage"];
                            isCheckPrePostImage = CrmHelper.CheckPrePostImage(preImage, postImage, traceService);
                            traceService.Trace($"CheckPrePostImage: {isCheckPrePostImage}");
                        }
                    }

                    Entity repairInfoTransImg = context.PostEntityImages["PostImage"];
                    if (repairInfoTransImg != null && repairInfoTransImg.Attributes.Contains(Casecontact.jarvisCase))
                    {
                        dynamic repairInfoTransData = repairInfoTransImg.Attributes;
                        string jsonObject = JsonConvert.SerializeObject(repairInfoTransData) ?? string.Empty;
                        EntityReference incident = (EntityReference)repairInfoTransImg.Attributes[Casecontact.jarvisCase];
                        if (targetEntity.Attributes.Contains(RepairInfoTranslation.Source) && targetEntity.Attributes[RepairInfoTranslation.Source] != null && incident != null && !string.IsNullOrEmpty(incident.Id.ToString()))
                        {
                            traceService.Trace("After Target/PostImage Check,Source contains data");
                            bool isCaseTypeBreakdown = CrmHelper.CheckCaseTypeIsBreakdown(incident.Id, traceService, orgService);
                            traceService.Trace("Case Type eq Breakdown " + isCaseTypeBreakdown.ToString() + " and incident id is " + incident.Id);
                            OptionSetValue source = (OptionSetValue)targetEntity.Attributes[RepairInfoTranslation.Source];

                            //// Souce is Equal to Jarvis then Sent to Mercurius, Not required any other check.
                            if (source.Value == (int)Source.Jarvis && isCaseTypeBreakdown && isCheckPrePostImage)
                            {
                                traceService.Trace("Source eq Jarvis");
                                if (context.MessageName.ToString().ToUpper() == PluginMessage.Create.ToUpper())
                                {
                                    if (entity.Attributes.Contains(RepairInfoTranslation.RepairInformation) || entity.Attributes.Contains(RepairInfoTranslation.PartsInformation) || entity.Attributes.Contains(RepairInfoTranslation.TowingRental) || entity.Attributes.Contains(RepairInfoTranslation.WarrantyInformation))
                                    {
                                        traceService.Trace("Inside Create RepairInfoTranslation Step");
                                        CrmHelper.JarvisToFunction(targetEntity, jsonObject, traceService, this.unSecureString.ToString(), this.secureString.ToString(), TranslationType.Create.ToUpper());
                                    }
                                }
                                else if (context.MessageName.ToString().ToUpper() == PluginMessage.Update.ToUpper())
                                {
                                    if (entity.Attributes.Contains(RepairInfoTranslation.RepairInformation) || entity.Attributes.Contains(RepairInfoTranslation.PartsInformation))
                                    {
                                        if ((preImage.Attributes.Contains(RepairInfoTranslation.RepairInformation) && !preImage.GetAttributeValue<string>(RepairInfoTranslation.RepairInformation).Equals(postImage.GetAttributeValue<string>(RepairInfoTranslation.RepairInformation)))
                                            || (preImage.Attributes.Contains(RepairInfoTranslation.PartsInformation) && !preImage.GetAttributeValue<string>(RepairInfoTranslation.PartsInformation).Equals(postImage.GetAttributeValue<string>(RepairInfoTranslation.PartsInformation))))
                                        {
                                            traceService.Trace("RepairInfo/PartsInfo Updated");
                                            CrmHelper.JarvisToFunction(targetEntity, jsonObject, traceService, this.unSecureString.ToString(), this.secureString.ToString(), TranslationType.RepairInfo.ToUpper());
                                        }
                                        else if (!preImage.Attributes.Contains(RepairInfoTranslation.RepairInformation) || !preImage.Attributes.Contains(RepairInfoTranslation.PartsInformation))
                                        {
                                            traceService.Trace("First RepairInfo/PartsInfo Updated");
                                            CrmHelper.JarvisToFunction(targetEntity, jsonObject, traceService, this.unSecureString.ToString(), this.secureString.ToString(), TranslationType.RepairInfo.ToUpper());
                                        }
                                    }

                                    if (entity.Attributes.Contains(RepairInfoTranslation.TowingRental))
                                    {
                                        if (preImage.Attributes.Contains(RepairInfoTranslation.TowingRental) && !preImage.GetAttributeValue<string>(RepairInfoTranslation.TowingRental).Equals(postImage.GetAttributeValue<string>(RepairInfoTranslation.TowingRental)))
                                        {
                                            traceService.Trace("TowingRental Updated");
                                            CrmHelper.JarvisToFunction(targetEntity, jsonObject, traceService, this.unSecureString.ToString(), this.secureString.ToString(), TranslationType.TowingInfo.ToUpper());
                                        }
                                        else if (!preImage.Attributes.Contains(RepairInfoTranslation.TowingRental))
                                        {
                                            traceService.Trace("First TowingRental Updated");
                                            CrmHelper.JarvisToFunction(targetEntity, jsonObject, traceService, this.unSecureString.ToString(), this.secureString.ToString(), TranslationType.TowingInfo.ToUpper());
                                        }
                                    }

                                    if (entity.Attributes.Contains(RepairInfoTranslation.WarrantyInformation))
                                    {
                                        if (preImage.Attributes.Contains(RepairInfoTranslation.WarrantyInformation) && !preImage.GetAttributeValue<string>(RepairInfoTranslation.WarrantyInformation).Equals(postImage.GetAttributeValue<string>(RepairInfoTranslation.WarrantyInformation)))
                                        {
                                            traceService.Trace("Warranty Updated");
                                            CrmHelper.JarvisToFunction(targetEntity, jsonObject, traceService, this.unSecureString.ToString(), this.secureString.ToString(), TranslationType.Warranty.ToUpper());
                                        }
                                        else if (!preImage.Attributes.Contains(RepairInfoTranslation.WarrantyInformation))
                                        {
                                            traceService.Trace("First Warranty Updated");
                                            CrmHelper.JarvisToFunction(targetEntity, jsonObject, traceService, this.unSecureString.ToString(), this.secureString.ToString(), TranslationType.Warranty.ToUpper());
                                        }
                                    }
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
                traceService.Trace("End CaseRepairInfoTransPostOperationAsync:" + DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));
                ////No Error Message Throwing Implictly to avoid blocking user because of integration.
            }
        }
    }
}
