// <copyright file="PassoutPostOperationAsync.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
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
    /// Pass out Post Operation Async --> Register as Sync and with Mapping Field as Post Image.
    /// </summary>
    public class PassoutPostOperationAsync : PluginBase
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
        /// Initializes a new instance of the <see cref="PassoutPostOperationAsync"/> class.
        /// PassOut Post Operation Constructor.
        /// </summary>
        /// <param name="unsecureString">unsecure String.</param>
        /// <param name="secureString">secure String.</param>
        /// <exception cref="InvalidPluginExecutionException">Invalid Plugin Execution Exception.</exception>
        public PassoutPostOperationAsync(string unsecureString, string secureString)
            : base(typeof(PassoutPostOperationAsync))
        {
            if (string.IsNullOrEmpty(secureString) && string.IsNullOrWhiteSpace(unsecureString))
            {
                throw new InvalidPluginExecutionException("Secure strings are required for this plugin to execute.");
            }

            this.secureString = secureString;
            this.unsecureString = unsecureString;
        }

        /// <summary>
        /// Execute Pass out Post Operation Plugin.
        /// </summary>
        /// <param name="localcontext">local context.</param>
        public override void ExecuteCrmPlugin(LocalPluginContext localcontext)
        {
            ITracingService traceService = localcontext.TracingService;
            traceService.Trace("Start PassoutPostOperationAsync:" + DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));
            try
            {
                IPluginExecutionContext context = localcontext.PluginExecutionContext;
                IOrganizationService orgService = localcontext.AdminOrganizationService;
                traceService.Trace("Plugin Execution Started");

                if (context.InputParameters.Contains(General.Target) && context.InputParameters[General.Target] is Entity entity)
                {
                    Entity targetEntity = entity;
                    bool isMercuriusUser = CrmHelper.CheckUserIsMercurius(context.UserId, traceService, orgService);
                    traceService.Trace($"IsMercuriusUserCheck: {isMercuriusUser}");

                    Entity passOutImg = context.PostEntityImages["PostImage"];
                    if (passOutImg != null && passOutImg.Attributes.Contains(Casecontact.jarvisIncident) && passOutImg.Attributes.Contains(PassOut.Statuscode))
                    {
                        EntityReference incident = (EntityReference)passOutImg.Attributes[Casecontact.jarvisIncident];
                        OptionSetValue passOutStatus = (OptionSetValue)passOutImg.Attributes[PassOut.Statuscode];
                        if (incident != null && !string.IsNullOrEmpty(incident.Id.ToString()) && passOutStatus != null)
                        {
                            bool isCaseTypeBreakdown = CrmHelper.CheckCaseTypeIsBreakdown(incident.Id, traceService, orgService);
                            traceService.Trace("Case Type eq Breakdown " + isCaseTypeBreakdown.ToString() + " and incident id is " + incident.Id);

                            // region UPDATE
                            if (context.MessageName.ToString().ToUpper() == PluginMessage.Update.ToUpper() && !isMercuriusUser && isCaseTypeBreakdown)
                            {
                                if (passOutStatus.Value != (int)PassOutStatus.Cancelled)
                                {
                                    traceService.Trace("Enter into PassOut Update scenarios");
                                    bool isCheckPrePostImage = true;
                                    traceService.Trace($"preCompareImage : {context.PreEntityImages.Contains("ComparePreImage")}");
                                    traceService.Trace($"postCompareImage : {context.PostEntityImages.Contains("ComparePostImage")}");
                                    if (context.PreEntityImages["ComparePreImage"] != null && context.PostEntityImages["ComparePostImage"] != null)
                                    {
                                        traceService.Trace($"CheckPrePostImage Method Started.");
                                        isCheckPrePostImage = CrmHelper.CheckPrePostImage(context.PreEntityImages["ComparePreImage"], context.PostEntityImages["ComparePostImage"], traceService);
                                        traceService.Trace($"CheckPrePostImage: {isCheckPrePostImage}");
                                    }

                                    if (isCheckPrePostImage)
                                    {
                                        //// Sending Jarvis.Event.PassOut only on Send.
                                        if (passOutStatus.Value == (int)PassOutStatus.ToBeSent)
                                        {
                                            if (targetEntity.Attributes.Contains(PassOut.Statuscode) || targetEntity.Attributes.Contains(PassOut.JarvisContact) || targetEntity.Attributes.Contains(PassOut.TransactionCurrencyId) || targetEntity.Attributes.Contains(PassOut.JarvisDescription)
                                                || targetEntity.Attributes.Contains(PassOut.JarvisGopLimitOut) || targetEntity.Attributes.Contains(PassOut.JarvisPaymentType) || targetEntity.Attributes.Contains(PassOut.JarvisRepairingDealer))
                                            {
                                                traceService.Trace($"PassOut Started");
                                                dynamic passOutData = passOutImg.Attributes;
                                                traceService.Trace($"Modified On For PassOut : {passOutImg.Attributes[PassOut.Modifiedon]}");
                                                string jsonObject = JsonConvert.SerializeObject(passOutData) ?? string.Empty;
                                                CrmHelper.JarvisToFunction(targetEntity, jsonObject, traceService, this.unsecureString.ToString(), this.secureString.ToString());
                                                traceService.Trace($"PassOut Completed");
                                            }
                                        }

                                        //// Sending TimeStamp Updates either on Send or Has been Sent.
                                        if (passOutStatus.Value == (int)PassOutStatus.Sent)
                                        {
                                            //// Sending Jarvis.Event.ETAUpdate
                                            if (targetEntity.Attributes.Contains(PassOut.JarvisEta) || (targetEntity.Attributes.Contains(PassOut.Statuscode) && passOutStatus.Value == (int)PassOutStatus.Sent && passOutImg.Attributes.Contains(PassOut.JarvisEta)))
                                            {
                                                traceService.Trace($"PassOut ETA Started");

                                                dynamic passOutData = passOutImg.Attributes;
                                                string jsonObject = JsonConvert.SerializeObject(passOutData) ?? string.Empty;
                                                CrmHelper.JarvisToFunction(targetEntity, jsonObject, traceService, TransType.PassOutETA, this.secureString.ToString());
                                                traceService.Trace($"PassOut ETA Completed");
                                            }

                                            //// Sending Jarvis.Event.ATAUpdate
                                            if (targetEntity.Attributes.Contains(PassOut.JarvisAta) || (targetEntity.Attributes.Contains(PassOut.Statuscode) && passOutStatus.Value == (int)PassOutStatus.Sent && passOutImg.Attributes.Contains(PassOut.JarvisAta)))
                                            {
                                                traceService.Trace($"PassOut ATA Started");
                                                dynamic passOutData = passOutImg.Attributes;
                                                string jsonObject = JsonConvert.SerializeObject(passOutData) ?? string.Empty;
                                                CrmHelper.JarvisToFunction(targetEntity, jsonObject, traceService, TransType.PassOutATA, this.secureString.ToString());
                                                traceService.Trace($"PassOut ATA Completed");
                                            }

                                            //// Sending Jarvis.Event.ETCUpdate
                                            if (targetEntity.Attributes.Contains(PassOut.JarvisEtc) || (targetEntity.Attributes.Contains(PassOut.Statuscode) && passOutStatus.Value == (int)PassOutStatus.Sent && passOutImg.Attributes.Contains(PassOut.JarvisEtc)))
                                            {
                                                if (passOutImg.Attributes.Contains(PassOut.JarvisAta) && passOutImg.Attributes[PassOut.JarvisAta] != null)
                                                {
                                                    traceService.Trace($"PassOut ETC Started");
                                                    dynamic passOutData = passOutImg.Attributes;
                                                    string jsonObject = JsonConvert.SerializeObject(passOutData) ?? string.Empty;
                                                    CrmHelper.JarvisToFunction(targetEntity, jsonObject, traceService, TransType.PassOutETC, this.secureString.ToString());
                                                    traceService.Trace($"PassOut ETC Completed");
                                                }
                                            }

                                            //// Sending Jarvis.Event.ATCUpdate
                                            if (targetEntity.Attributes.Contains(PassOut.JarvisAtc) || (targetEntity.Attributes.Contains(PassOut.Statuscode) && passOutStatus.Value == (int)PassOutStatus.Sent && passOutImg.Attributes.Contains(PassOut.JarvisAtc)))
                                            {
                                                traceService.Trace($"PassOut ATC Started");
                                                dynamic passOutData = passOutImg.Attributes;
                                                string jsonObject = JsonConvert.SerializeObject(passOutData) ?? string.Empty;
                                                CrmHelper.JarvisToFunction(targetEntity, jsonObject, traceService, TransType.PassOutATC, this.secureString.ToString());
                                                traceService.Trace($"PassOut ATC Completed");
                                            }
                                        }
                                    }
                                }
                                else if (passOutStatus.Value == (int)PassOutStatus.Cancelled)
                                {
                                    traceService.Trace($"PassOut Cancel Started");
                                    dynamic passOutData = passOutImg.Attributes;
                                    traceService.Trace($"Modified On For PassOut : {passOutImg.Attributes[PassOut.Modifiedon]}");
                                    string jsonObject = JsonConvert.SerializeObject(passOutData) ?? string.Empty;
                                    CrmHelper.JarvisToFunction(targetEntity, jsonObject, traceService, this.unsecureString.ToString(), this.secureString.ToString());
                                    traceService.Trace($"PassOut Cancel Completed");
                                }
                            }
                            else if (context.MessageName.ToString().ToUpper() == PluginMessage.Update.ToUpper() && isCaseTypeBreakdown && targetEntity.Attributes.Contains(PassOut.JarvisEta))
                            {
                                traceService.Trace("Enter into GPS and ETA logic.");
                                if (passOutStatus.Value == (int)PassOutStatus.ToBeSent || passOutStatus.Value == (int)PassOutStatus.Sent)
                                {
                                    traceService.Trace("Passout status checked.");
                                    traceService.Trace($"PreImage of GPS ETA {context.PreEntityImages["ComparePreImage"].Attributes.Contains(PassOut.JarvisGpsETA)} && postImage of ETA {passOutImg.Attributes.Contains(PassOut.JarvisEta)}");
                                    if (context.PreEntityImages["ComparePreImage"] != null && context.PreEntityImages["ComparePreImage"].Attributes.Contains(PassOut.JarvisGpsETA) && passOutImg.Attributes.Contains(PassOut.JarvisEta))
                                    {
                                        OptionSetValue releaseCase = CrmHelper.GetReleaseAutomationConfig(orgService, "jarvis_automationreleasecase", traceService);
                                        if (releaseCase != null && releaseCase.Value == (int)3)
                                        {
                                            traceService.Trace("This is a onecaseonly release case behaviour");
                                            DateTime gpsETA = context.PreEntityImages["ComparePreImage"].GetAttributeValue<DateTime>(PassOut.JarvisGpsETA);
                                            DateTime eta = passOutImg.GetAttributeValue<DateTime>(PassOut.JarvisEta);
                                            traceService.Trace($"GPS ETA: {context.PreEntityImages["ComparePreImage"].GetAttributeValue<DateTime>(PassOut.JarvisGpsETA)}  ETA{passOutImg.GetAttributeValue<DateTime>(PassOut.JarvisEta)}  Difference {gpsETA - eta}");
                                            TimeSpan ts = gpsETA - eta;
                                            double diff = Math.Abs(ts.TotalMinutes);
                                            if (diff > 15)
                                            {
                                                traceService.Trace($"PassOut ETA Started");
                                                dynamic passOutData = passOutImg.Attributes;
                                                string jsonObject = JsonConvert.SerializeObject(passOutData) ?? string.Empty;
                                                CrmHelper.JarvisToFunction(targetEntity, jsonObject, traceService, TransType.PassOutETA, this.secureString.ToString());
                                                traceService.Trace($"PassOut ETA Completed");
                                            }
                                        }
                                    }
                                }
                            }
                            else if (context.MessageName.ToString().ToUpper() == PluginMessage.Create.ToUpper() && isCaseTypeBreakdown && !isMercuriusUser)
                            {
                                // region CREATE
                                DateTime modifiedOn = (DateTime)entity.Attributes[PassOut.Modifiedon];
                                traceService.Trace("Enter into Create of PassOut");
                                dynamic passOutData = passOutImg.Attributes;
                                traceService.Trace($"Modified On For PassOut : {passOutImg.Attributes[PassOut.Modifiedon]}");
                                string jsonObject = JsonConvert.SerializeObject(passOutData) ?? string.Empty;
                                if (passOutImg != null && passOutImg.Attributes.Contains(PassOut.Source)
                                    && passOutImg.Attributes.Contains(PassOut.Statuscode))
                                {
                                    traceService.Trace("Status Code contains data,mercuriuspassoutid does not contain,source contains data");
                                    OptionSetValue source = (OptionSetValue)passOutImg.Attributes[PassOut.Source];
                                    OptionSetValue statusCode = (OptionSetValue)passOutImg.Attributes[PassOut.Statuscode];
                                    if (source.Value == (int)Source.Jarvis && statusCode.Value == (int)PassOutStatus.ToBeSent)
                                    {
                                        traceService.Trace("Status Code is to be sent and source is jarvis for create");
                                        CrmHelper.JarvisToFunction(passOutImg, jsonObject, traceService, this.unsecureString.ToString(), this.secureString.ToString());

                                        //// Sending Jarvis.Event.ETAUpdate
                                        if (targetEntity.Attributes.Contains(PassOut.JarvisEta))
                                        {
                                            //Do Not Send ETA on Passout Create - US #681793
                                            traceService.Trace($"PassOut ETA Started");
                                           // CrmHelper.JarvisToFunction(targetEntity, jsonObject, traceService, TransType.PassOutETA, this.secureString.ToString());
                                            traceService.Trace($"PassOut ETA Completed");
                                        }

                                        //// Sending Jarvis.Event.ATAUpdate
                                        if (targetEntity.Attributes.Contains(PassOut.JarvisAta))
                                        {
                                            //Do Not Send ATA on Passout Create - US #681793
                                            traceService.Trace($"PassOut ATA Started");
                                            // CrmHelper.JarvisToFunction(targetEntity, jsonObject, traceService, TransType.PassOutATA, this.secureString.ToString());
                                            traceService.Trace($"PassOut ATA Completed");
                                        }

                                        //// Sending Jarvis.Event.ETCUpdate
                                        if (targetEntity.Attributes.Contains(PassOut.JarvisEtc))
                                        {
                                            if (passOutImg.Attributes.Contains(PassOut.JarvisAta) && passOutImg.Attributes[PassOut.JarvisAta] != null)
                                            {
                                                //Do Not Send ETC on Passout Create - US #681793
                                                traceService.Trace($"PassOut ETC Started");
                                               // CrmHelper.JarvisToFunction(targetEntity, jsonObject, traceService, TransType.PassOutETC, this.secureString.ToString());
                                                traceService.Trace($"PassOut ETC Completed");
                                            }
                                        }

                                        //// Sending Jarvis.Event.ATCUpdate
                                        if (targetEntity.Attributes.Contains(PassOut.JarvisAtc))
                                        {
                                            //Do Not Send ATC on Passout Create - US #681793
                                            traceService.Trace($"PassOut ATC Started");
                                            // CrmHelper.JarvisToFunction(targetEntity, jsonObject, traceService, TransType.PassOutATC, this.secureString.ToString());
                                            traceService.Trace($"PassOut ATC Completed");
                                        }
                                    }
                                }
                            }

                            // endregion
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
                traceService.Trace("End PassoutPostOperationAsync:" + DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));

                // No Error Message Throwing Implictly to avoid blocking user because of integration.
            }
        }
    }
}
