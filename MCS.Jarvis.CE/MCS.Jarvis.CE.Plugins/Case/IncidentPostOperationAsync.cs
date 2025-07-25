//-----------------------------------------------------------------------
// <copyright file="IncidentPostOperationAsync.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

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
    /// IncidentPostOperationAsync --> Register as Sync and with Mapping Field as Post Image.
    /// </summary>
    public class IncidentPostOperationAsync : PluginBase
    {
        /// <summary>
        /// secure string.
        /// </summary>
        private readonly string secureString;

        /// <summary>
        /// unsecure string.
        /// </summary>
        private readonly string unSecureString;

        /// <summary>
        /// Initializes a new instance of the <see cref="IncidentPostOperationAsync"/> class.
        /// Incident Post Operation Constructor.
        /// </summary>
        /// <param name="unsecureString">unsecure String.</param>
        /// <param name="secureString">secure String.</param>
        /// <exception cref="InvalidPluginExecutionException">Invalid Plugin Execution Exception.</exception>
        public IncidentPostOperationAsync(string unsecureString, string secureString)
            : base(typeof(IncidentPostOperationAsync))
        {
            if (string.IsNullOrEmpty(secureString) && string.IsNullOrWhiteSpace(unsecureString))
            {
                throw new InvalidPluginExecutionException("Secure and unsecure strings are required for this plugin to execute.");
            }

            this.secureString = secureString;
            this.unSecureString = unsecureString;
        }

        /// <summary>
        ///  Execute Incident Post Operation Plugin.
        /// </summary>
        /// <param name="localcontext">Local Context.</param>
        public override void ExecuteCrmPlugin(LocalPluginContext localcontext)
        {
            ITracingService tracingService = localcontext.TracingService;
            tracingService.Trace("Start IncidentPostOperationAsync:" + DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));
            try
            {
                IPluginExecutionContext context = localcontext.PluginExecutionContext;

                if (context.InputParameters.Contains(General.Target) && context.InputParameters[General.Target] is Entity entity)
                {
                    IOrganizationService service = localcontext.AdminOrganizationService;
                    Entity targetEntity = entity;
                    bool isMercuriusUser = CrmHelper.CheckUserIsMercurius(context.UserId, tracingService, service);
                    tracingService.Trace($"IsMercuriusUserCheck: {isMercuriusUser}");
                    Entity incidentImg = context.PostEntityImages["PostImage"];
                    if (incidentImg != null)
                    {
                        dynamic incImg = incidentImg.Attributes;
                        string jsonObject = JsonConvert.SerializeObject(incImg) ?? string.Empty;

                        if (context.MessageName.ToString().ToUpper() == PluginMessage.Create.ToUpper() && this.unSecureString.ToUpper() == TransType.CreateCase.ToUpper())
                        {
                            if (targetEntity.Attributes.Contains(Incident.caseOriginCode) && targetEntity.Attributes[Incident.caseOriginCode] != null && targetEntity.Attributes.Contains(Incident.casetypecode) && targetEntity.Attributes[Incident.casetypecode] != null)
                            {
                                OptionSetValue caseOrginCode = (OptionSetValue)targetEntity.Attributes[Incident.caseOriginCode];
                                OptionSetValue casetypecode = (OptionSetValue)targetEntity.Attributes[Incident.casetypecode];
                                tracingService.Trace(casetypecode.Value.ToString());
                                ////CaseOriginCode is not equal to Mercurius Phone ,Eservice Web,Email - Mercurius,Fax - Mercurius and Telematics - Mercurius
                                if (caseOrginCode.Value != (int)CaseOriginCode.Phone_Mercurius && caseOrginCode.Value != (int)CaseOriginCode.Web_Eservice && caseOrginCode.Value != (int)CaseOriginCode.Email_Mercurius
                                    && caseOrginCode.Value != (int)CaseOriginCode.Fax_Mercurius && caseOrginCode.Value != (int)CaseOriginCode.Telematics_Mercurius && casetypecode.Value == (int)CaseTypeCode.Breakdown)
                                {
                                    CrmHelper.JarvisToFunction(targetEntity, jsonObject, tracingService, this.unSecureString.ToString(), this.secureString.ToString());
                                }
                            }
                        }
                        else if (context.MessageName.ToString().ToUpper() == PluginMessage.Update.ToUpper() && incidentImg.Attributes.Contains(Incident.casetypecode)
                            && incidentImg.Attributes[Incident.casetypecode] != null && incidentImg.Attributes.Contains(Incident.caseStatus) && incidentImg.Attributes[Incident.caseStatus] != null)
                        {
                            bool isCheckPrePostImage = true;
                            tracingService.Trace($"preCompareImage : {context.PreEntityImages.Contains("ComparePreImage")}");
                            tracingService.Trace($"postCompareImage : {context.PostEntityImages.Contains("ComparePostImage")}");
                            if (context.PreEntityImages["ComparePreImage"] != null && context.PostEntityImages["ComparePostImage"] != null)
                            {
                                tracingService.Trace($"CheckPrePostImage Method Started.");
                                isCheckPrePostImage = CrmHelper.CheckPrePostImage(context.PreEntityImages["ComparePreImage"], context.PostEntityImages["ComparePostImage"], tracingService);
                                tracingService.Trace($"CheckPrePostImage: {isCheckPrePostImage}");
                            }

                            OptionSetValue caseStatus = (OptionSetValue)incidentImg.Attributes[Incident.caseStatus];
                            OptionSetValue casetypecode = (OptionSetValue)incidentImg.Attributes[Incident.casetypecode];
                            tracingService.Trace(caseStatus.Value.ToString());
                            ////CaseTypeCode is Breakdown and CheckPrePostImage and Not ForceClosed.
                            if (casetypecode.Value == (int)CaseTypeCode.Breakdown && isCheckPrePostImage && !isMercuriusUser && caseStatus.Value != 1000 && this.unSecureString.ToUpper() == TransType.UpdateCase.ToUpper())
                            {
                                CrmHelper.JarvisToFunction(targetEntity, jsonObject, tracingService, this.unSecureString.ToString(), this.secureString.ToString());
                            }
                            else if (casetypecode.Value == (int)CaseTypeCode.Breakdown && isCheckPrePostImage && isMercuriusUser && caseStatus.Value == 5 && this.unSecureString.ToUpper() == TransType.UpdateCase.ToUpper())
                            {
                                CrmHelper.JarvisToFunction(targetEntity, jsonObject, tracingService, this.unSecureString.ToString(), this.secureString.ToString());
                            }

                            if (casetypecode.Value == (int)CaseTypeCode.Breakdown && isCheckPrePostImage && caseStatus.Value == 5 && this.unSecureString.ToUpper() == TransType.CSISCase.ToUpper())
                            {
                                CrmHelper.JarvisToFunction(targetEntity, jsonObject, tracingService, this.unSecureString.ToString(), this.secureString.ToString());
                            }

                            tracingService.Trace(casetypecode.Value.ToString());
                        }
                    }
                }
            }
            catch (InvalidPluginExecutionException pex)
            {
                tracingService.Trace(pex.Message);
                tracingService.Trace(pex.StackTrace);
            }
            finally
            {
                tracingService.Trace("End IncidentPostOperationAsync:" + DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));
            }
        }
    }
}
