// <copyright file="GopPostOperationAsync.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MCS.Jarvis.CE.Plugins
{
    using System;
    using System.Globalization;
    using global::Plugins;
    using MCS.Jarvis.CE.BusinessProcessesShared.Helpers;
    using Microsoft.Xrm.Sdk;
    using Newtonsoft.Json;
    using static MCS.Jarvis.CE.BusinessProcessesShared.Helpers.Constants;

    /// <summary>
    /// Post Operation Async Register as Sync and with Mapping Field as Post Image.
    /// </summary>
    public class GopPostOperationAsync : PluginBase
    {
        /// <summary>
        /// Secure string.
        /// </summary>
        private readonly string secureString;

        /// <summary>
        /// unsecure string.
        /// </summary>
        private readonly string unsecureString;

        /// <summary>
        /// Initializes a new instance of the <see cref="GopPostOperationAsync"/> class.
        /// GOP Post Operation Constructor.
        /// </summary>
        /// <param name="unsecureString">unsecure String.</param>
        /// <param name="secureString">secure String.</param>
        /// <exception cref="InvalidPluginExecutionException">Invalid Plugin Execution Exception.</exception>
        public GopPostOperationAsync(string unsecureString, string secureString)
            : base(typeof(GopPostOperationAsync))
        {
            if (string.IsNullOrEmpty(secureString) && string.IsNullOrWhiteSpace(unsecureString))
            {
                throw new InvalidPluginExecutionException("Secure and unsecure strings are required for this plugin to execute.");
            }

            this.secureString = secureString;
            this.unsecureString = unsecureString;
        }

        /// <summary>
        /// Execute Post Operation Plugin.
        /// </summary>
        /// <param name="localcontext">local context.</param>
        public override void ExecuteCrmPlugin(LocalPluginContext localcontext)
        {
            ITracingService traceService = localcontext.TracingService;
            traceService.Trace("Start GOPPostOperationAsync:" + DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));
            try
            {
                IPluginExecutionContext context = localcontext.PluginExecutionContext;
                IOrganizationService orgService = localcontext.OrganizationService;
                if (context.Depth > 2)
                {
                    traceService.Trace("Depth is greater than 2.");
                    return;
                }

                if (context.InputParameters.Contains(General.Target) && context.InputParameters[General.Target] is Entity entity && context.PostEntityImages["PostImage"] != null)
                {
                    traceService.Trace("Enter into Create after condition");
                    IOrganizationService service = localcontext.AdminOrganizationService;
                    Entity targetEntity = entity;
                    bool isMercuriusUser = CrmHelper.CheckUserIsMercurius(context.UserId, traceService, service);
                    bool isSystemUser = CrmHelper.CheckUserIsSystem(context.UserId, traceService, service);
                    traceService.Trace($"IsMercuriusUserCheck: {isMercuriusUser} and IsSystemUserCheck: {isSystemUser}");
                    Entity gopImg = context.PostEntityImages["PostImage"];

                    dynamic gopImp = gopImg.Attributes;
                    string jsonObject = JsonConvert.SerializeObject(gopImp) ?? string.Empty;

                    // Target Atrributes Value.
                    dynamic attribute = targetEntity.Attributes;
                    string targetJson = JsonConvert.SerializeObject(attribute) ?? string.Empty;
                    traceService.Trace($"Target Attributes : {targetJson}");
                    EntityReference incident = (EntityReference)gopImg.Attributes[Casecontact.jarvisIncident];
                    traceService.Trace("Got Image");
                    if (gopImg != null && gopImg.Attributes.Contains(Gop.Source) && gopImg.Attributes.Contains(Gop.GopApproval) && gopImg.Attributes.Contains(Gop.requestType) && incident != null && !string.IsNullOrEmpty(incident.Id.ToString()))
                    {
                        bool isCaseTypeBreakdown = CrmHelper.CheckCaseTypeIsBreakdown(incident.Id, traceService, orgService);
                        OptionSetValue gopApproved = (OptionSetValue)gopImg.Attributes[Gop.GopApproval];
                        OptionSetValue requestType = (OptionSetValue)gopImg.Attributes[Gop.requestType];
                        OptionSetValue source = (OptionSetValue)gopImg.Attributes[Gop.Source];
                        OptionSetValue paymentType = (OptionSetValue)gopImg.Attributes[Gop.PaymentType];
                        decimal? totalLimitIn = gopImg.Attributes.Contains(Gop.totalLimitIn) ? (decimal)gopImg.Attributes[Gop.totalLimitIn] : (decimal?)null;
                        decimal? totalLimitOut = gopImg.Attributes.Contains(Gop.totalLimitOut) ? (decimal)gopImg.Attributes[Gop.totalLimitOut] : (decimal?)null;
                        string contact = gopImg.Attributes.Contains(Gop.contact) ? (string)gopImg.Attributes[Gop.contact] : string.Empty;
                        decimal? gopLimitIn = gopImg.Attributes.Contains(Gop.jarvis_goplimitin) ? (decimal)gopImg.Attributes[Gop.jarvis_goplimitin] : (decimal?)null;
                        decimal? gopLimitOut = gopImg.Attributes.Contains(Gop.jarvis_goplimitout) ? (decimal)gopImg.Attributes[Gop.jarvis_goplimitout] : (decimal?)null;
                        int? stateCode = gopImg.Attributes.Contains("statecode") ? ((OptionSetValue)gopImg.Attributes["statecode"]).Value : (int?)null;
                        int? statusCode = gopImg.Attributes.Contains("statuscode") ? ((OptionSetValue)gopImg.Attributes["statuscode"]).Value : (int?)null;
                        bool? isDealerCopied = gopImg.Attributes.Contains("jarvis_isdealercopied") ? (bool?)gopImg.Attributes["jarvis_isdealercopied"] : false;
                        EntityReference triggeredBy = gopImg.Attributes.Contains("jarvis_triggeredby") ? (EntityReference)gopImg.Attributes["jarvis_triggeredby"] : null;
                        traceService.Trace("Case Type eq Breakdown " + isCaseTypeBreakdown.ToString() + " and incident id is " + incident.Id);

                        if (context.MessageName.ToString().ToUpper() == PluginMessage.Create.ToUpper() && isCaseTypeBreakdown)
                        {
                            traceService.Trace("CREATE Plugin");
                            //// target.Gop.MercuriusId does not contain,PostImg.contains(approved,jarvis_source_ and requesttype)
                            if (!targetEntity.Attributes.Contains(Gop.jarvis_mercuriusgopid) && !isMercuriusUser && gopApproved.Value == (int)GopApproval.Approved && requestType.Value == (int)GopRequestType.GOP_HD
                                && source.Value == (int)Source.Jarvis && totalLimitIn.HasValue && totalLimitOut.HasValue && !string.IsNullOrEmpty(contact))
                            {
                                ////Approved = Yes and RequestType = GOP_HD, Gop.Source == "Jarvis" && Mercuriusgopid not present && totoallimit not null && totallimitout not null and contact not null
                                traceService.Trace("Mercuriusgopid not present && Approved = Yes and RequestType = GOP, Gop.Source == Jarvis && totoallimit not null && totallimitout not null and contact not null");
                                CrmHelper.JarvisToFunction(gopImg, jsonObject, traceService, this.unsecureString.ToString(), this.secureString.ToString());
                                traceService.Trace("ADD GOP IN Triggered"); ////ADD GOP
                            }

                            traceService.Trace("Retrieve Comment and gopReason for creation");
                            string comment = entity.Attributes.Contains(Gop.Comment) ? (string)entity.Attributes[Gop.Comment] : string.Empty;
                            string gopReason = entity.Attributes.Contains(Gop.GopReason) ? (string)entity.Attributes[Gop.GopReason] : string.Empty;

                            if (!targetEntity.Attributes.Contains(Gop.jarvis_mercuriusgopid) && !isMercuriusUser && source.Value == (int)Source.Jarvis
                                && ((requestType.Value == (int)GopRequestType.GOP_HD && gopLimitIn.HasValue && string.IsNullOrEmpty(comment)) || (requestType.Value == (int)GopRequestType.GOP_RD && gopLimitOut.HasValue && string.IsNullOrEmpty(gopReason))))
                            {
                                if (paymentType.Value == (int)PaymentType.CreditCard && gopApproved.Value == (int)GopApproval.Approved)
                                {
                                    traceService.Trace("Payment Type credit card");
                                    traceService.Trace("GopHd.Comment does not contains or GopRd.gopReason does not contain");
                                    traceService.Trace("Mercuriusgopid not present && Gop.Source == Jarvis && (requestType = GOP_HD && gopLimitIN has Value || requestType = GOP_RD && gopLimitOut has value ");
                                    CrmHelper.JarvisToFunction(gopImg, jsonObject, traceService, requestType.Value.ToString(), this.secureString.ToString());  ////ADD GOP+
                                    traceService.Trace("ADD GOP HD or ADD GOP RD Triggered");
                                }
                                else if (paymentType.Value != (int)PaymentType.CreditCard)
                                {
                                    traceService.Trace("Payment Type not creditcard");
                                    traceService.Trace("GopHd.Comment does not contains or GopRd.gopReason does not contain");
                                    traceService.Trace("Mercuriusgopid not present && Gop.Source == Jarvis && (requestType = GOP_HD && gopLimitIN has Value || requestType = GOP_RD && gopLimitOut has value ");
                                    CrmHelper.JarvisToFunction(gopImg, jsonObject, traceService, requestType.Value.ToString(), this.secureString.ToString());  ////ADD GOP+
                                    traceService.Trace("ADD GOP HD or ADD GOP RD Triggered");
                                }
                            }
                        }

                        if (context.MessageName.ToUpper() == PluginMessage.Update.ToUpper())
                        {
                            bool isCheckPrePostImage = true;
                            Entity preImage;
                            traceService.Trace($"preCompareImage : {context.PreEntityImages.Contains("ComparePreImage")}");
                            traceService.Trace($"postCompareImage : {context.PostEntityImages.Contains("ComparePostImage")}");
                            if (context.PreEntityImages["ComparePreImage"] != null && context.PostEntityImages["ComparePostImage"] != null)
                            {
                                preImage = context.PreEntityImages["ComparePreImage"];
                                traceService.Trace($"CheckPrePostImage Method Started.");
                                isCheckPrePostImage = CrmHelper.CheckPrePostImage(context.PreEntityImages["ComparePreImage"], context.PostEntityImages["ComparePostImage"], traceService);
                                traceService.Trace($"CheckPrePostImage: {isCheckPrePostImage}");
                            }
                            else
                            {
                                preImage = null;
                            }

                            // check  GOP approved and status is Inactive + isCheckPrePostImage [Cancel GOP] + isMercuriusUser check + IsSystemUser and CaseBreakdownCheck.
                            if (gopApproved.Value == (int)GopApproval.Approved && requestType.Value == (int)GopRequestType.GOP_HD && stateCode.HasValue && stateCode == 1 && statusCode.HasValue && statusCode == 2 && isCheckPrePostImage && !isMercuriusUser && isCaseTypeBreakdown && !isSystemUser)
                            {
                                CrmHelper.JarvisToFunction(gopImg, jsonObject, traceService, this.unsecureString.ToString(), this.secureString.ToString());    ////ADD GOP
                                traceService.Trace(" Approved = Yes && Statecode = 1 && StatusCode = 2");
                                traceService.Trace("ADD GOP Triggered.");
                            }
                            else if (gopApproved.Value != (int)GopApproval.Approved && requestType.Value == (int)GopRequestType.GOP_HD && stateCode.HasValue && stateCode == 1 && statusCode.HasValue && statusCode == 334030001 && (isDealerCopied.HasValue && !isDealerCopied.Value) && isCheckPrePostImage && !isMercuriusUser && isCaseTypeBreakdown)
                            {
                                CrmHelper.JarvisToFunction(gopImg, jsonObject, traceService, this.unsecureString.ToString(), this.secureString.ToString());    ////ADD GOP
                                traceService.Trace(" Approved = No && Statecode = 1 && StatusCode = 334030001 && ParentGOP.GopDealer not same to CopiedGOP.GopDealer");
                                traceService.Trace("ADD GOP Triggered.");
                            }
                            else if (gopApproved.Value == (int)GopApproval.Approved && requestType.Value == (int)GopRequestType.GOP_HD
                              && totalLimitIn.HasValue && totalLimitOut.HasValue && !string.IsNullOrEmpty(contact) && !isMercuriusUser && isCaseTypeBreakdown
                              && stateCode.HasValue && stateCode == 0 && isCheckPrePostImage)
                            {
                                traceService.Trace("Update Plugin");
                                traceService.Trace("Update + Approved + Comment contains or does not contains data??");
                                traceService.Trace("Gop.RequestType = GOP HD and Approved = Yes && totalLiminIn not null && totalLimitOut not null and contact not null.");
                                CrmHelper.JarvisToFunction(gopImg, jsonObject, traceService, this.unsecureString.ToString(), this.secureString.ToString());    ////ADD GOP
                                traceService.Trace("ADD GOP IN Triggered.");

                                traceService.Trace("Checking comment logic for GOP HD");
                                if (gopImg.Attributes.Contains(Gop.Comment) && !string.IsNullOrEmpty((string)gopImg.Attributes[Gop.Comment]) && gopImg.Attributes.Contains(Gop.Translationstatuscomment))
                                {
                                    traceService.Trace("GopHd.comment contains data, preImage comment trnas contains data, post image trans contians data");
                                    OptionSetValue postTranslationStatus = (OptionSetValue)gopImg.Attributes[Gop.Translationstatuscomment];
                                    if (triggeredBy != null && (postTranslationStatus.Value == (int)TranslationStatus.NotStarted || postTranslationStatus.Value == (int)TranslationStatus.Completed))
                                    {
                                        bool isTriggeredByMercurius = CrmHelper.CheckUserIsMercurius(triggeredBy.Id, traceService, service);
                                        if (!isTriggeredByMercurius)
                                        {
                                            traceService.Trace("preImage trans status is Inprogress and post image trans status is completed or not started and triggeredBy not Mercurius.");
                                            CrmHelper.JarvisToFunction(gopImg, jsonObject, traceService, requestType.Value.ToString(), this.secureString.ToString());  ////ADD GOP+
                                            traceService.Trace("ADD GOP+HD Triggered.");
                                        }
                                    }
                                    else
                                    {
                                        traceService.Trace("preImage trans status is Inprogress and post image trans status is completed or not started and triggeredBy not Mercurius.");
                                        CrmHelper.JarvisToFunction(gopImg, jsonObject, traceService, requestType.Value.ToString(), this.secureString.ToString());  ////ADD GOP+
                                        traceService.Trace("ADD GOP+HD Triggered.");
                                    }
                                }
                                else
                                {
                                    traceService.Trace("GopHd Approved without any comment.");
                                    CrmHelper.JarvisToFunction(gopImg, jsonObject, traceService, requestType.Value.ToString(), this.secureString.ToString());  ////ADD GOP+
                                    traceService.Trace("ADD GOP+HD Triggered.");
                                }
                            }
                            else if ((entity.Attributes.Contains(Gop.Translationstatuscomment) || entity.Attributes.Contains(Gop.Translationstatusgopreason)) && !isMercuriusUser && isCaseTypeBreakdown)
                            {
                                traceService.Trace("Update Plugin");
                                if (requestType.Value == (int)GopRequestType.GOP_HD && gopImg.Attributes.Contains(Gop.Comment) && !string.IsNullOrEmpty((string)gopImg.Attributes[Gop.Comment]) && preImage.Attributes.Contains(Gop.Translationstatuscomment) && gopImg.Attributes.Contains(Gop.Translationstatuscomment))
                                {
                                    traceService.Trace("GopHd.comment contains data, preImage comment trnas contains data, post image trans contians data");
                                    OptionSetValue preTranslationStatus = (OptionSetValue)preImage.Attributes[Gop.Translationstatuscomment];
                                    OptionSetValue postTranslationStatus = (OptionSetValue)gopImg.Attributes[Gop.Translationstatuscomment];
                                    if (preTranslationStatus.Value == (int)TranslationStatus.InProgress && (postTranslationStatus.Value == (int)TranslationStatus.NotStarted || postTranslationStatus.Value == (int)TranslationStatus.Completed))
                                    {
                                        if (paymentType.Value == (int)PaymentType.CreditCard && gopApproved.Value == (int)GopApproval.Approved)
                                        {
                                            traceService.Trace("Payment Type is CreditCard");
                                            if (triggeredBy != null && preTranslationStatus.Value == (int)TranslationStatus.InProgress && (postTranslationStatus.Value == (int)TranslationStatus.NotStarted || postTranslationStatus.Value == (int)TranslationStatus.Completed))
                                            {
                                                bool isTriggeredByMercurius = CrmHelper.CheckUserIsMercurius(triggeredBy.Id, traceService, service);
                                                if (!isTriggeredByMercurius)
                                                {
                                                    traceService.Trace("preImage trans status is Inprogress and post image trans status is completed or not started and triggeredBy not merecurius.");
                                                    CrmHelper.JarvisToFunction(gopImg, jsonObject, traceService, requestType.Value.ToString(), this.secureString.ToString());  ////ADD GOP+
                                                    traceService.Trace("ADD GOP+HD Triggered.");
                                                }
                                            }
                                            else
                                            {
                                                traceService.Trace("preImage trans status is Inprogress and post image trans status is completed or not started and triggeredBy not Mercurius.");
                                                CrmHelper.JarvisToFunction(gopImg, jsonObject, traceService, requestType.Value.ToString(), this.secureString.ToString());  ////ADD GOP+
                                                traceService.Trace("ADD GOP+HD Triggered.");
                                            }
                                        }
                                        else if (paymentType.Value != (int)PaymentType.CreditCard)
                                        {
                                            if (triggeredBy != null && preTranslationStatus.Value == (int)TranslationStatus.InProgress && (postTranslationStatus.Value == (int)TranslationStatus.NotStarted || postTranslationStatus.Value == (int)TranslationStatus.Completed))
                                            {
                                                bool isTriggeredByMercurius = CrmHelper.CheckUserIsMercurius(triggeredBy.Id, traceService, service);
                                                if (!isTriggeredByMercurius)
                                                {
                                                    traceService.Trace("preImage trans status is Inprogress and post image trans status is completed or not started and triggeredBy not merecurius.");
                                                    CrmHelper.JarvisToFunction(gopImg, jsonObject, traceService, requestType.Value.ToString(), this.secureString.ToString());  ////ADD GOP+
                                                    traceService.Trace("ADD GOP+HD Triggered.");
                                                }
                                            }
                                            else
                                            {
                                                traceService.Trace("preImage trans status is Inprogress and post image trans status is completed or not started and triggeredBy not Mercurius.");
                                                CrmHelper.JarvisToFunction(gopImg, jsonObject, traceService, requestType.Value.ToString(), this.secureString.ToString());  ////ADD GOP+
                                                traceService.Trace("ADD GOP+HD Triggered.");
                                            }
                                        }
                                    }
                                }
                                else if (requestType.Value == (int)GopRequestType.GOP_RD && gopImg.Attributes.Contains(Gop.GopReason) && !string.IsNullOrEmpty((string)gopImg.Attributes[Gop.GopReason]) && preImage.Attributes.Contains(Gop.Translationstatusgopreason) && gopImg.Attributes.Contains(Gop.Translationstatusgopreason))
                                {
                                    traceService.Trace("GopRD.gopReason contains data, preImage gopReason trnas contains data, post image gopReason contians data");
                                    OptionSetValue preTranslationStatus = (OptionSetValue)preImage.Attributes[Gop.Translationstatusgopreason];
                                    OptionSetValue postTranslationStatus = (OptionSetValue)gopImg.Attributes[Gop.Translationstatusgopreason];
                                    if (preTranslationStatus.Value == (int)TranslationStatus.InProgress && (postTranslationStatus.Value == (int)TranslationStatus.NotStarted || postTranslationStatus.Value == (int)TranslationStatus.Completed))
                                    {
                                        if (paymentType.Value == (int)PaymentType.CreditCard && gopApproved.Value == (int)GopApproval.Approved)
                                        {
                                            traceService.Trace("Payment Type is CreditCard");
                                            if (triggeredBy != null && preTranslationStatus.Value == (int)TranslationStatus.InProgress && (postTranslationStatus.Value == (int)TranslationStatus.NotStarted || postTranslationStatus.Value == (int)TranslationStatus.Completed))
                                            {
                                                bool isTriggeredByMercurius = CrmHelper.CheckUserIsMercurius(triggeredBy.Id, traceService, service);
                                                if (!isTriggeredByMercurius)
                                                {
                                                    traceService.Trace("preImage trans status is Inprogress and post image trans status is completed or not started and triggeredBy not merecurius.");
                                                    CrmHelper.JarvisToFunction(gopImg, jsonObject, traceService, requestType.Value.ToString(), this.secureString.ToString());  ////ADD GOP+
                                                    traceService.Trace("ADD GOP+RD Triggered.");
                                                }
                                            }
                                            else
                                            {
                                                traceService.Trace("preImage trans status is Inprogress and post image trans status is completed or not started and triggeredBy not Mercurius.");
                                                CrmHelper.JarvisToFunction(gopImg, jsonObject, traceService, requestType.Value.ToString(), this.secureString.ToString());  ////ADD GOP+
                                                traceService.Trace("ADD GOP+HD Triggered.");
                                            }
                                        }
                                        else if (paymentType.Value != (int)PaymentType.CreditCard)
                                        {
                                            if (triggeredBy != null && preTranslationStatus.Value == (int)TranslationStatus.InProgress && (postTranslationStatus.Value == (int)TranslationStatus.NotStarted || postTranslationStatus.Value == (int)TranslationStatus.Completed))
                                            {
                                                bool isTriggeredByMercurius = CrmHelper.CheckUserIsMercurius(triggeredBy.Id, traceService, service);
                                                if (!isTriggeredByMercurius)
                                                {
                                                    traceService.Trace("preImage trans status is Inprogress and post image trans status is completed or not started and triggeredBy not merecurius.");
                                                    CrmHelper.JarvisToFunction(gopImg, jsonObject, traceService, requestType.Value.ToString(), this.secureString.ToString());  ////ADD GOP+
                                                    traceService.Trace("ADD GOP+RD Triggered.");
                                                }
                                            }
                                            else
                                            {
                                                traceService.Trace("preImage trans status is Inprogress and post image trans status is completed or not started and triggeredBy not Mercurius.");
                                                CrmHelper.JarvisToFunction(gopImg, jsonObject, traceService, requestType.Value.ToString(), this.secureString.ToString());  ////ADD GOP+
                                                traceService.Trace("ADD GOP+HD Triggered.");
                                            }
                                        }
                                    }
                                }
                            }
                            ////588609
                            if (gopApproved.Value == (int)GopApproval.Approved && requestType.Value == (int)GopRequestType.GOP_HD && isMercuriusUser && isCheckPrePostImage)
                            {
                                CrmHelper.JarvisToFunction(gopImg, jsonObject, traceService, this.unsecureString.ToString(), this.secureString.ToString());
                                traceService.Trace("ADD GOP IN Triggered For Eservice/Mercurius.");
                            }
                        }
                    }
                }
            }
            catch (InvalidPluginExecutionException pex)
            {
                traceService.Trace(pex.Message);
                traceService.Trace(pex.StackTrace);
            }
            finally
            {
                traceService.Trace("End GOPPostOperationAsync:" + DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));
            }
        }
    }
}
