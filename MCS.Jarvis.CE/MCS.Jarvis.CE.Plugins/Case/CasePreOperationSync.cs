//-----------------------------------------------------------------------
// <copyright file="CasePreOperationSync.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace MCS.Jarvis.CE.Plugins
{
    using System;
    using System.Linq;
    using System.Text.RegularExpressions;
    using MCS.Jarvis.CE.BusinessProcessesShared;
    using MCS.Jarvis.CE.BusinessProcessesShared.Helpers;
    using MCS.Jarvis.CE.BusinessProcessShared.Case;
    using MCS.Jarvis.CE.BusinessProcessShared.CaseMonitor;
    using MCS.jarvis.CE.BusinessProcessShared.VASBreakdownProcess;
    using Microsoft.Crm.Sdk.Messages;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;
    using static MCS.Jarvis.CE.BusinessProcessesShared.Helpers.Constants;

    /// <summary>
    /// Initializes a new instance of the <see cref="CasePreOperationSync"/> class.
    /// </summary>
    public class CasePreOperationSync : IPlugin
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
        /// Initializes a new instance of the <see cref="CasePreOperationSync"/> class.
        /// Case Pre Operation Sync.
        /// </summary>
        /// <param name="unsecureParamString">unsecure String.</param>
        /// <param name="secureParamString">secure String.</param>
        public CasePreOperationSync(string unsecureParamString, string secureParamString)
        {
            this.secureString = secureParamString;
            this.unSecureString = unsecureParamString;
        }

        /// <summary>
        /// Execute method.
        /// </summary>
        /// <param name="serviceProvider">service Provider.</param>
        /// <exception cref="InvalidPluginExecutionException">Exception details.</exception>
        public void Execute(IServiceProvider serviceProvider)
        {
            // Extract the tracing service for use in debugging sandboxed plug-ins.
            ITracingService tracingService =
                (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            // Obtain the execution context from the service provider.
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            if (context.Depth > 4)
            {
                return;
            }

            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                Entity incident = (Entity)context.InputParameters["Target"];

                try
                {
                    if (serviceProvider != null)
                    {
                        IOrganizationServiceFactory orgfactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                        IOrganizationService orgService = orgfactory.CreateOrganizationService(context.UserId);
                        IOrganizationService adminService = orgfactory.CreateOrganizationService(null);

                        if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                        {
                            Entity targetEntity = (Entity)context.InputParameters["Target"];

                            if (context.MessageName.ToString().ToUpper() == "UPDATE")
                            {
                                Entity incidentPre = (Entity)context.PreEntityImages["PreImage"];
                                OptionSetValue caseStatusCode = new OptionSetValue();
                                OptionSetValue caseTypeCode = new OptionSetValue();
                                if (targetEntity.Attributes.Contains("statuscode") && targetEntity.Attributes["statuscode"] != null)
                                {
                                    caseStatusCode = (OptionSetValue)targetEntity.Attributes["statuscode"];
                                    tracingService.Trace("Case Status" + caseStatusCode.Value);
                                }
                                else if (incidentPre.Attributes.Contains("statuscode") && incidentPre.Attributes["statuscode"] != null)
                                {
                                    caseStatusCode = (OptionSetValue)incidentPre.Attributes["statuscode"];
                                    tracingService.Trace("Case Status" + caseStatusCode.Value);
                                }

                                // Set customer Informed
                                if (!targetEntity.Attributes.Contains("jarvis_customerinformed") && targetEntity.Attributes.Contains("statuscode"))
                                {
                                    // To check if user manually did not select customer informed
                                    CaseOperations caseOperationsSet = new CaseOperations();
                                    tracingService.Trace("Invoke SetCustomerInformed");
                                    OptionSetValue releaseConfig = new OptionSetValue();
                                    releaseConfig = CrmHelper.GetReleaseAutomationConfig(orgService, MCS.Jarvis.CE.Plugins.Constants.JarvisConfiguration.Automationreleasecase, tracingService);
                                    // caseOperationsSet.SetCustomerInformed(orgService, tracingService, targetEntity, caseStatusCode, releaseConfig.Value);
                                }

#pragma warning disable SA1123 // Do not place regions within elements
                                #region Dummy Dealer
                                if (targetEntity.Attributes.Contains("modifiedby") && targetEntity.Attributes["modifiedby"] != null)
                                {
                                    EntityReference modifiedByID = (EntityReference)targetEntity.Attributes["modifiedby"];
                                    string modifiedBy = string.Empty;
                                    EntityCollection systemuser = orgService.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getModifiedUser, modifiedByID.Id)));
                                    if (systemuser != null && systemuser.Entities.Count > 0)
                                    {
                                        modifiedBy = (string)systemuser.Entities[0].Attributes["fullname"];
                                        if (modifiedBy.ToUpperInvariant().Contains("MERCURIUS"))
                                        {
                                            if (targetEntity.Attributes.Contains("jarvis_homedealer") && targetEntity.Attributes["jarvis_homedealer"] != null)
                                            {
                                                EntityReference homeDealer = (EntityReference)targetEntity.Attributes["jarvis_homedealer"];
                                                if (incidentPre.Attributes.Contains("jarvis_homedealer") && incidentPre.Attributes["jarvis_homedealer"] != null)
                                                {
                                                    EntityReference prehomeDealer = (EntityReference)incidentPre.Attributes["jarvis_homedealer"];
                                                    if (prehomeDealer.Id != homeDealer.Id)
                                                    {
                                                        Entity businessPartner = orgService.Retrieve(homeDealer.LogicalName, homeDealer.Id, new ColumnSet("jarvis_responsableunitid"));
                                                        if (businessPartner.Attributes.Contains("jarvis_responsableunitid") && businessPartner.Attributes["jarvis_responsableunitid"] != null)
                                                        {
                                                            string respUnitID = (string)businessPartner.Attributes["jarvis_responsableunitid"];
                                                            if (respUnitID.ToUpperInvariant().Contains("DUMMY"))
                                                            {
                                                                targetEntity["jarvis_homedealer"] = prehomeDealer;
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
#pragma warning restore SA1123 // Do not place regions within elements

                                #endregion

                                // Check if Customer Informed
#pragma warning disable SA1123 // Do not place regions within elements
                                #region Customer Informed - Automation

                                if (targetEntity.Attributes.Contains("jarvis_customerinformed") && targetEntity.Attributes["jarvis_customerinformed"] != null)
                                {
                                    bool customerInformed = (bool)targetEntity.Attributes["jarvis_customerinformed"];
                                    if (customerInformed && caseStatusCode.Value == 90)
                                    {
                                        OptionSetValue releaseConfig = new OptionSetValue();
                                        releaseConfig = CrmHelper.GetReleaseAutomationConfig(orgService, MCS.Jarvis.CE.Plugins.Constants.JarvisConfiguration.Automationreleasecase, tracingService);
                                        if (releaseConfig.Value == 1)
                                        {
                                            targetEntity["jarvis_automationcriteriamet"] = true;
                                            targetEntity["jarvis_mercuriusstatus"] = new OptionSetValue(900);
                                            targetEntity["statuscode"] = new OptionSetValue(5);
                                            targetEntity["statecode"] = new OptionSetValue(1);
                                            if (incidentPre.Attributes.Contains("jarvis_firstclosed") && incidentPre.Attributes["jarvis_firstclosed"] != null)
                                            {
                                                // targetEntity["jarvis_firstclosed"] = DateTime.UtcNow;
                                            }
                                            else
                                            {
                                                targetEntity["jarvis_firstclosed"] = DateTime.UtcNow;
                                            }
                                        }
                                        else
                                        {
                                            EntityCollection monitorActions = orgService.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getOpenMO, targetEntity.Id)));
                                            EntityCollection passouts = orgService.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getOpenPassOuts, targetEntity.Id)));
                                            EntityCollection gop = orgService.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getUnApprovedGOP, targetEntity.Id)));
                                            if ((monitorActions.Entities.Count <= 0 && passouts.Entities.Count <= 0 && gop.Entities.Count <= 0) || (incidentPre.Contains(Constants.Incident.JarvisHDRD) && (bool)incidentPre.Attributes[Constants.Incident.JarvisHDRD]))
                                            {
                                                targetEntity["jarvis_automationcriteriamet"] = true;
                                                targetEntity["jarvis_mercuriusstatus"] = new OptionSetValue(900);
                                                targetEntity["statuscode"] = new OptionSetValue(5);
                                                targetEntity["statecode"] = new OptionSetValue(1);
                                                if (incidentPre.Attributes.Contains("jarvis_firstclosed") && incidentPre.Attributes["jarvis_firstclosed"] != null)
                                                {
                                                    // targetEntity["jarvis_firstclosed"] = DateTime.UtcNow;
                                                }
                                                else
                                                {
                                                    targetEntity["jarvis_firstclosed"] = DateTime.UtcNow;
                                                }
                                            }
                                            else
                                            {
                                                targetEntity["jarvis_automationcriteriamet"] = false;
                                            }
                                        }
                                    }
                                }
#pragma warning restore SA1123 // Do not place regions within elements

                                #endregion

#pragma warning disable SA1123 // Do not place regions within elements
                                #region Set Route Case
                                if (incidentPre.Attributes.Contains("casetypecode") && incidentPre.Attributes["casetypecode"] != null)
                                {
                                    caseTypeCode = (OptionSetValue)incidentPre.Attributes["casetypecode"];
                                }
#pragma warning restore SA1123 // Do not place regions within elements

                                OptionSetValue caseState = new OptionSetValue();
                                if (incidentPre.Attributes.Contains("statecode") && incidentPre.Attributes["statecode"] != null)
                                {
                                    caseState = (OptionSetValue)incidentPre.Attributes["statecode"];
                                }

                                if (targetEntity.Attributes.Contains("statecode") && targetEntity.Attributes["statecode"] != null)
                                {
                                    caseState = (OptionSetValue)targetEntity.Attributes["statuscode"];
                                }

                                if ((targetEntity.Attributes.Contains("ownerid") && caseTypeCode.Value == 2) || (targetEntity.Attributes.Contains("jarvis_fumonitorskill") && targetEntity.Attributes["jarvis_fumonitorskill"] != null))
                                {
                                    EntityReference fumonitorskills = new EntityReference();
                                    OptionSetValue caseStatus = new OptionSetValue();
                                    DateTime? futimestamp = null;
                                    if (incidentPre.Attributes.Contains("statuscode") && incidentPre.Attributes["statuscode"] != null)
                                    {
                                        caseStatus = (OptionSetValue)incidentPre.Attributes["statuscode"];
                                    }

                                    if (targetEntity.Attributes.Contains("statuscode") && targetEntity.Attributes["statuscode"] != null)
                                    {
                                        caseStatus = (OptionSetValue)targetEntity.Attributes["statuscode"];
                                    }

                                    // #453158 - Case Steering once timestamp reached - Design Document
                                    if (targetEntity.Attributes.Contains("jarvis_fumonitorskill") && targetEntity.Attributes["jarvis_fumonitorskill"] != null)
                                    {
                                        fumonitorskills = (EntityReference)targetEntity.Attributes["jarvis_fumonitorskill"];
                                    }
                                    else if (incidentPre.Attributes.Contains("jarvis_fumonitorskill") && incidentPre.Attributes["jarvis_fumonitorskill"] != null)
                                    {
                                        fumonitorskills = (EntityReference)incidentPre.Attributes["jarvis_fumonitorskill"];
                                    }

                                    if (targetEntity.Attributes.Contains("jarvis_futimestamp") && targetEntity.Attributes["jarvis_futimestamp"] != null)
                                    {
                                        futimestamp = (DateTime)targetEntity.Attributes["jarvis_futimestamp"];
                                    }
                                    else if (incidentPre.Attributes.Contains("jarvis_futimestamp") && incidentPre.Attributes["jarvis_futimestamp"] != null)
                                    {
                                        futimestamp = (DateTime)incidentPre.Attributes["jarvis_futimestamp"];
                                    }

                                    if (caseStatus.Value != 90)
                                    {
                                        if (fumonitorskills != null && futimestamp != null)
                                        {
                                            targetEntity["routecase"] = true;
                                        }
                                        else
                                        {
                                            targetEntity["routecase"] = false;
                                        }
                                    }
                                }

                                if (caseState.Value != 0 && caseTypeCode.Value == 2)
                                {
                                    targetEntity["routecase"] = false;
                                    targetEntity["jarvis_fumonitorskill"] = null;
                                    targetEntity["jarvis_fucomment"] = null;
                                    targetEntity["jarvis_futimestamp"] = null;
                                    EntityCollection caseQueueItems = orgService.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getQueueItemOfcase, targetEntity.Id)));
                                    if (caseQueueItems.Entities.Count > 0)
                                    {
                                        RemoveFromQueueRequest queueItem = new RemoveFromQueueRequest();
                                        queueItem.QueueItemId = caseQueueItems.Entities.FirstOrDefault().Id;
                                        adminService.Execute(queueItem);
                                    }
                                }
                                #endregion

                                // Mercurius Status Changes
#pragma warning disable SA1123 // Do not place regions within elements
                                #region Mercurius Status Change

                                if (targetEntity.Attributes.Contains("jarvis_mercuriusstatus") && targetEntity.Attributes["jarvis_mercuriusstatus"] != null)
                                {
                                    OptionSetValue mercStatus = (OptionSetValue)targetEntity.Attributes["jarvis_mercuriusstatus"];

                                    if (mercStatus.Value == 0)
                                    {
                                        targetEntity["statuscode"] = new OptionSetValue(10);
                                        this.UpdateTimeStamp(targetEntity, mercStatus.Value);
                                    }

                                    if (mercStatus.Value == 100)
                                    {
                                        targetEntity["statuscode"] = new OptionSetValue(20);
                                        if (incidentPre.Attributes.Contains("jarvis_casestatusupdate") && incidentPre.Attributes["jarvis_casestatusupdate"] != null)
                                        {
                                            targetEntity["jarvis_canvasdate"] = (DateTime)incidentPre.Attributes["jarvis_casestatusupdate"];
                                        }

                                        this.UpdateTimeStamp(targetEntity, mercStatus.Value);
                                    }

                                    if (mercStatus.Value == 200)
                                    {
                                        targetEntity["statuscode"] = new OptionSetValue(30);
                                        if (incidentPre.Attributes.Contains("jarvis_casestatusupdate") && incidentPre.Attributes["jarvis_casestatusupdate"] != null)
                                        {
                                            // jarvis_canvasdate
                                            targetEntity["jarvis_canvasdate"] = (DateTime)incidentPre.Attributes["jarvis_casestatusupdate"];
                                        }

                                        this.UpdateTimeStamp(targetEntity, mercStatus.Value);
                                    }

                                    if (mercStatus.Value == 300)
                                    {
                                        targetEntity["statuscode"] = new OptionSetValue(40);
                                        if (incidentPre.Attributes.Contains("jarvis_casestatusupdate") && incidentPre.Attributes["jarvis_casestatusupdate"] != null)
                                        {
                                            // jarvis_canvasdate
                                            targetEntity["jarvis_canvasdate"] = (DateTime)incidentPre.Attributes["jarvis_casestatusupdate"];
                                        }

                                        this.UpdateTimeStamp(targetEntity, mercStatus.Value);
                                    }

                                    if (mercStatus.Value == 400)
                                    {
                                        targetEntity["statuscode"] = new OptionSetValue(50);
                                        if (incidentPre.Attributes.Contains("jarvis_casestatusupdate") && incidentPre.Attributes["jarvis_casestatusupdate"] != null)
                                        {
                                            // jarvis_canvasdate
                                            targetEntity["jarvis_canvasdate"] = (DateTime)incidentPre.Attributes["jarvis_casestatusupdate"];
                                        }

                                        this.UpdateTimeStamp(targetEntity, mercStatus.Value);
                                    }

                                    if (mercStatus.Value == 500)
                                    {
                                        targetEntity["statuscode"] = new OptionSetValue(60);
                                        if (incidentPre.Attributes.Contains("jarvis_casestatusupdate") && incidentPre.Attributes["jarvis_casestatusupdate"] != null)
                                        {
                                            // jarvis_canvasdate
                                            targetEntity["jarvis_canvasdate"] = (DateTime)incidentPre.Attributes["jarvis_casestatusupdate"];
                                        }

                                        this.UpdateTimeStamp(targetEntity, mercStatus.Value);
                                    }

                                    if (mercStatus.Value == 600)
                                    {
                                        targetEntity["statuscode"] = new OptionSetValue(70);
                                        if (incidentPre.Attributes.Contains("jarvis_casestatusupdate") && incidentPre.Attributes["jarvis_casestatusupdate"] != null)
                                        {
                                            // jarvis_canvasdate
                                            targetEntity["jarvis_canvasdate"] = (DateTime)incidentPre.Attributes["jarvis_casestatusupdate"];
                                        }

                                        this.UpdateTimeStamp(targetEntity, mercStatus.Value);
                                    }

                                    if (mercStatus.Value == 700)
                                    {
                                        targetEntity["statuscode"] = new OptionSetValue(80);
                                        if (incidentPre.Attributes.Contains("jarvis_casestatusupdate") && incidentPre.Attributes["jarvis_casestatusupdate"] != null)
                                        {
                                            targetEntity["jarvis_canvasdate"] = (DateTime)incidentPre.Attributes["jarvis_casestatusupdate"];
                                        }

                                        this.UpdateTimeStamp(targetEntity, mercStatus.Value);
                                    }

                                    if (mercStatus.Value == 800)
                                    {
                                        targetEntity["statuscode"] = new OptionSetValue(90);
                                        if (incidentPre.Attributes.Contains("jarvis_casestatusupdate") && incidentPre.Attributes["jarvis_casestatusupdate"] != null)
                                        {
                                            // jarvis_canvasdate
                                            targetEntity["jarvis_canvasdate"] = (DateTime)incidentPre.Attributes["jarvis_casestatusupdate"];
                                        }

                                        this.UpdateTimeStamp(targetEntity, mercStatus.Value);
                                    }

                                    if (mercStatus.Value == 900)
                                    {
                                        if (targetEntity.Attributes.Contains("jarvis_customerinformed"))
                                        {
                                            bool customerInformed = (bool)targetEntity["jarvis_customerinformed"];
                                            if (!customerInformed)
                                            {
                                                targetEntity["jarvis_customerinformed"] = true;
                                            }
                                        }
                                        else
                                        {
                                            targetEntity["jarvis_customerinformed"] = true;
                                        }

                                        if (targetEntity.Attributes.Contains("statecode"))
                                        {
                                            OptionSetValue status = (OptionSetValue)targetEntity["statecode"];
                                            if (status.Value != 1)
                                            {
                                                targetEntity["statecode"] = new OptionSetValue(1);
                                            }
                                        }
                                        else
                                        {
                                            targetEntity["statecode"] = new OptionSetValue(1);
                                        }

                                        if (targetEntity.Attributes.Contains("statuscode"))
                                        {
                                            OptionSetValue statusCode = (OptionSetValue)targetEntity["statuscode"];
                                            if (statusCode.Value != 5)
                                            {
                                                targetEntity["statuscode"] = new OptionSetValue(5);
                                            }
                                        }
                                        else
                                        {
                                            targetEntity["statuscode"] = new OptionSetValue(5);
                                        }

                                        if (incidentPre.Attributes.Contains("jarvis_casestatusupdate") && incidentPre.Attributes["jarvis_casestatusupdate"] != null)
                                        {
                                            // jarvis_canvasdate
                                            targetEntity["jarvis_canvasdate"] = (DateTime)incidentPre.Attributes["jarvis_casestatusupdate"];
                                        }

                                        this.UpdateTimeStamp(targetEntity, mercStatus.Value);
                                    }
                                }
#pragma warning restore SA1123 // Do not place regions within elements

                                #endregion

                                // FirstClosed
#pragma warning disable SA1123 // Do not place regions within elements
                                #region First Closed

                                if (targetEntity.Attributes.Contains("statecode") && targetEntity.Attributes["statecode"] != null)
                                {
                                    OptionSetValue statecode = (OptionSetValue)targetEntity.Attributes["statecode"];
                                    if (statecode.Value == 1)
                                    {
                                        // Resolved
                                        EntityCollection caseRsolution = orgService.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getCaseResolution, targetEntity.Id)));
                                        if (caseRsolution.Entities.Count != 0)
                                        {
                                            if (caseRsolution.Entities[0].Attributes.Contains("resolutiontypecode") && caseRsolution.Entities[0].Attributes["resolutiontypecode"] != null)
                                            {
                                                OptionSetValue resolutionType = (OptionSetValue)caseRsolution.Entities[0].Attributes["resolutiontypecode"];
                                                if (resolutionType.Value == 1000)
                                                {
                                                    // ForceClosed
                                                    targetEntity["statuscode"] = new OptionSetValue(1000);
                                                    targetEntity["jarvis_mercuriusstatus"] = new OptionSetValue(900);
                                                }
                                            }
                                        }

                                        if (incidentPre.Attributes.Contains("jarvis_firstclosed") && incidentPre.Attributes["jarvis_firstclosed"] != null)
                                        {
                                            //// targetEntity["jarvis_firstclosed"] = DateTime.UtcNow;
                                        }
                                        else
                                        {
                                            targetEntity["jarvis_firstclosed"] = DateTime.UtcNow;
                                            targetEntity["jarvis_automationcriteriamet"] = true;
                                        }
                                    }
                                    else
                                    {
                                        if (incidentPre.Attributes.Contains("statecode") && incidentPre.Attributes["statecode"] != null)
                                        {
                                            OptionSetValue preStatus = (OptionSetValue)incidentPre.Attributes["statecode"];
                                            if (statecode.Value != preStatus.Value)
                                            {
                                                // Reopened
                                                VASBreakDownProcess vasBreakdown = new VASBreakDownProcess();
                                                targetEntity["jarvis_mercuriusstatus"] = new OptionSetValue(850);
                                                targetEntity["statuscode"] = new OptionSetValue(85);
                                                targetEntity["jarvis_reopenedreason"] = "Reopened from Jarvis";
                                                vasBreakdown.setStateBPF(targetEntity, orgService, 1, 0);
                                            }
                                        }
                                    }
                                }
#pragma warning restore SA1123 // Do not place regions within elements

                                #endregion

                                // 110619
#pragma warning disable SA1123 // Do not place regions within elements
                                #region Update Brand and Code

                                if (targetEntity.Attributes.Contains("jarvis_vehicle") && targetEntity.Attributes["jarvis_vehicle"] != null)
                                {
                                    IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                                    IOrganizationService service = serviceFactory.CreateOrganizationService(null);
                                    EntityReference vehicle = (EntityReference)targetEntity.Attributes["jarvis_vehicle"];
                                    Entity vehicleEntity = service.Retrieve(vehicle.LogicalName, vehicle.Id, new ColumnSet("jarvis_brandid"));
                                    if (vehicleEntity.Attributes.Contains("jarvis_brandid") && vehicleEntity.Attributes["jarvis_brandid"] != null)
                                    {
                                        targetEntity["jarvis_brand"] = (EntityReference)vehicleEntity.Attributes["jarvis_brandid"];

                                        Entity brand = service.Retrieve(((EntityReference)vehicleEntity.Attributes["jarvis_brandid"]).LogicalName, ((EntityReference)vehicleEntity.Attributes["jarvis_brandid"]).Id, new ColumnSet("jarvis_mercuriusbrandcode"));
                                        if (brand.Attributes.Contains("jarvis_mercuriusbrandcode") && brand.Attributes["jarvis_mercuriusbrandcode"] != null)
                                        {
                                            targetEntity["jarvis_mercuriusbrand"] = (string)brand.Attributes["jarvis_mercuriusbrandcode"];
                                        }
                                    }
                                }
#pragma warning restore SA1123 // Do not place regions within elements

                                #endregion

#pragma warning disable SA1123 // Do not place regions within elements
                                #region Update Case Location (National/International/Global)
                                CaseOperations caseOperations = new CaseOperations();
#pragma warning restore SA1123 // Do not place regions within elements
                                caseOperations.UpdateCaseLocationAndGOPCurrency(orgService, tracingService, targetEntity, incidentPre);

                                #endregion

#pragma warning disable SA1123 // Do not place regions within elements
                                #region Preferred Caller Language
                                if (targetEntity.Attributes.Contains("jarvis_callerlanguage") && targetEntity.Attributes["jarvis_callerlanguage"] != null)
                                {
                                    EntityReference callerPreferredLanguage = (EntityReference)targetEntity.Attributes["jarvis_callerlanguage"];
                                    bool isSupported = false;
                                    Entity callerLanguage = orgService.Retrieve(callerPreferredLanguage.LogicalName, callerPreferredLanguage.Id, new ColumnSet("jarvis_vassupportedlanguage", "jarvis_vasstandardlanguage"));
                                    if (callerLanguage.Attributes.Contains("jarvis_vasstandardlanguage") && callerLanguage.Attributes["jarvis_vasstandardlanguage"] != null)
                                    {
                                        isSupported = (bool)callerLanguage.Attributes["jarvis_vasstandardlanguage"];
                                        if (!isSupported)
                                        {
                                            if (callerLanguage.Attributes.Contains("jarvis_vassupportedlanguage") && callerLanguage.Attributes["jarvis_vassupportedlanguage"] != null)
                                            {
                                                targetEntity["jarvis_callerlanguagesupported"] = (EntityReference)callerLanguage.Attributes["jarvis_vassupportedlanguage"];
                                            }
                                            else
                                            {
                                                targetEntity["jarvis_callerlanguagesupported"] = (EntityReference)targetEntity.Attributes["jarvis_callerlanguage"];
                                            }
                                        }
                                        else
                                        {
                                            targetEntity["jarvis_callerlanguagesupported"] = (EntityReference)targetEntity.Attributes["jarvis_callerlanguage"];
                                        }
                                    }
                                    else
                                    {
                                        targetEntity["jarvis_callerlanguagesupported"] = (EntityReference)targetEntity.Attributes["jarvis_callerlanguage"];
                                    }
                                }
#pragma warning restore SA1123 // Do not place regions within elements
                                #endregion

#pragma warning disable SA1123 // Do not place regions within elements
                                #region Preferred Driver Language
                                if (targetEntity.Attributes.Contains("jarvis_driverlanguage") && targetEntity.Attributes["jarvis_driverlanguage"] != null)
                                {
                                    EntityReference callerPreferredLanguage = (EntityReference)targetEntity.Attributes["jarvis_driverlanguage"];
                                    bool isSupported = false;
                                    Entity callerLanguage = orgService.Retrieve(callerPreferredLanguage.LogicalName, callerPreferredLanguage.Id, new ColumnSet("jarvis_vassupportedlanguage", "jarvis_vasstandardlanguage"));
                                    if (callerLanguage.Attributes.Contains("jarvis_vasstandardlanguage") && callerLanguage.Attributes["jarvis_vasstandardlanguage"] != null)
                                    {
                                        isSupported = (bool)callerLanguage.Attributes["jarvis_vasstandardlanguage"];
                                        if (!isSupported)
                                        {
                                            if (callerLanguage.Attributes.Contains("jarvis_vassupportedlanguage") && callerLanguage.Attributes["jarvis_vassupportedlanguage"] != null)
                                            {
                                                targetEntity["jarvis_driverlanguagesupported"] = (EntityReference)callerLanguage.Attributes["jarvis_vassupportedlanguage"];
                                            }
                                            else
                                            {
                                                targetEntity["jarvis_driverlanguagesupported"] = (EntityReference)targetEntity.Attributes["jarvis_driverlanguage"];
                                            }
                                        }
                                        else
                                        {
                                            targetEntity["jarvis_driverlanguagesupported"] = (EntityReference)targetEntity.Attributes["jarvis_driverlanguage"];
                                        }
                                    }
                                    else
                                    {
                                        targetEntity["jarvis_driverlanguagesupported"] = (EntityReference)targetEntity.Attributes["jarvis_driverlanguage"];
                                    }
                                }
#pragma warning restore SA1123 // Do not place regions within elements
                                #endregion

                                // Auto Stage Movement
                                bool isAutomation = false;
                                isAutomation = CrmHelper.GetAutomationConfig(orgService, MCS.Jarvis.CE.Plugins.Constants.JarvisConfiguration.Automationcasestatuschange, tracingService);
                                if (isAutomation)
                                {
#pragma warning disable SA1123 // Do not place regions within elements
                                    #region AutoStageMovement

#pragma warning disable SA1123 // Do not place regions within elements
                                    #region Waiting for Repair

                                    if ((targetEntity.Attributes.Contains("jarvis_eta") && targetEntity.Attributes["jarvis_eta"] != null) || (targetEntity.Attributes.Contains("jarvis_ata") && targetEntity.Attributes["jarvis_ata"] != null))
                                    {
#pragma warning disable SA1123 // Do not place regions within elements
                                        #region Close Monitor Actions

                                        CaseMonitorProcess operation = new CaseMonitorProcess();
#pragma warning restore SA1123 // Do not place regions within elements
                                        string fucomments = "Pass ETA,Chase ETA";
                                        operation.AutomateCloseMonitorActions(incidentPre, fucomments, 1, fucomments, orgService);

                                        #endregion
                                        ////updating stage even if ata has value and not eta
                                        if (incidentPre.Attributes.Contains("statuscode") && incidentPre.Attributes["statuscode"] != null)
                                        {
                                            OptionSetValue status = (OptionSetValue)incidentPre.Attributes["statuscode"];
                                            if (status.Value == 40)
                                            {
                                                // ETA Technician
                                                targetEntity["statuscode"] = new OptionSetValue(50); // Waiting for Repair
                                                targetEntity["jarvis_mercuriusstatus"] = new OptionSetValue(400); // Waiting for Repair
                                                targetEntity["jarvis_casestatusupdate"] = DateTime.UtcNow; ////560406
                                            }
                                        }
                                    }
#pragma warning restore SA1123 // Do not place regions within elements
#pragma warning restore SA1123 // Do not place regions within elements

                                    #endregion

#pragma warning disable SA1123 // Do not place regions within elements
                                    #region Repair Ongoing
                                    if ((targetEntity.Attributes.Contains("jarvis_ata") && targetEntity.Attributes["jarvis_ata"] != null) || (incidentPre.Attributes.Contains("jarvis_ata") && incidentPre.Attributes["jarvis_ata"] != null))
                                    {
                                        if (incidentPre.Attributes.Contains("statuscode") && incidentPre.Attributes["statuscode"] != null)
                                        {
                                            OptionSetValue status = (OptionSetValue)incidentPre.Attributes["statuscode"];
                                            if (status.Value == 50)
                                            {
                                                // Waiting for Repair
                                                targetEntity["statuscode"] = new OptionSetValue(60); // Repair Ongoing
                                                targetEntity["jarvis_mercuriusstatus"] = new OptionSetValue(500); // Repair Ongoing
                                                targetEntity["jarvis_casestatusupdate"] = DateTime.UtcNow; ////560406
                                            }
                                        }
                                    }
#pragma warning restore SA1123 // Do not place regions within elements

                                    #endregion

#pragma warning disable SA1123 // Do not place regions within elements
                                    #region Repair finished

                                    if (incidentPre.Attributes.Contains("statuscode") && incidentPre.Attributes["statuscode"] != null)
                                    {
                                        OptionSetValue status = (OptionSetValue)incidentPre.Attributes["statuscode"];
                                        if (status.Value == 60)
                                        {
                                            // Repair Ongoing
                                            if ((targetEntity.Attributes.Contains("jarvis_etc") && targetEntity.Attributes["jarvis_etc"] != null) || (targetEntity.Attributes.Contains("jarvis_atc") && targetEntity.Attributes["jarvis_atc"] != null) || (incidentPre.Attributes.Contains("jarvis_etc") && incidentPre.Attributes["jarvis_etc"] != null) || (incidentPre.Attributes.Contains("jarvis_atc") && incidentPre.Attributes["jarvis_atc"] != null))
                                            {
                                                tracingService.Trace("Setting Status as Repair finished if ATC has value");
                                                targetEntity["statuscode"] = new OptionSetValue(70); // Repair Finished
                                                targetEntity["jarvis_mercuriusstatus"] = new OptionSetValue(600); // Repair Finished
                                                targetEntity["jarvis_casestatusupdate"] = DateTime.UtcNow; ////560406
                                            }
                                        }
                                    }
#pragma warning restore SA1123 // Do not place regions within elements
                                    #endregion

#pragma warning disable SA1123 // Do not place regions within elements
                                    #region Repair Summary

                                    if (incidentPre.Attributes.Contains("statuscode") && incidentPre.Attributes["statuscode"] != null)
                                    {
                                        OptionSetValue status = (OptionSetValue)incidentPre.Attributes["statuscode"];
                                        if (status.Value == 70)
                                        {
                                            // Repair Finished
                                            if ((targetEntity.Attributes.Contains("jarvis_atc") && targetEntity.Attributes["jarvis_atc"] != null) || (incidentPre.Attributes.Contains("jarvis_atc") && incidentPre.Attributes["jarvis_atc"] != null))
                                            {
                                                targetEntity["statuscode"] = new OptionSetValue(80); // Repair Summary
                                                targetEntity["jarvis_mercuriusstatus"] = new OptionSetValue(700); // Repair Summary
                                                targetEntity["jarvis_casestatusupdate"] = DateTime.UtcNow; ////560406
                                            }
                                        }
                                    }
#pragma warning restore SA1123 // Do not place regions within elements
                                    #endregion

#pragma warning disable SA1123 // Do not place regions within elements
                                    #region Case Closure

                                    if (incidentPre.Attributes.Contains("statuscode") && incidentPre.Attributes["statuscode"] != null)
                                    {
                                        OptionSetValue status = (OptionSetValue)incidentPre.Attributes["statuscode"];
                                        if (status.Value == 80)
                                        {
                                            // Repair Finished
                                            if ((targetEntity.Attributes.Contains("jarvis_actualcausefault") && targetEntity.Attributes["jarvis_actualcausefault"] != null) || (incidentPre.Attributes.Contains("jarvis_actualcausefault") && incidentPre.Attributes["jarvis_actualcausefault"] != null))
                                            {
                                                targetEntity["statuscode"] = new OptionSetValue(90); // Case Closure
                                                targetEntity["jarvis_mercuriusstatus"] = new OptionSetValue(800); // Case Closure
                                                targetEntity["jarvis_casestatusupdate"] = DateTime.UtcNow;
                                            }
                                        }
                                    }
#pragma warning restore SA1123 // Do not place regions within elements
                                    #endregion

                                    #endregion
                                }

#pragma warning disable SA1123 // Do not place regions within elements
                                #region Lattitude Longitude - WorkOrder Changes
                                if (targetEntity.Attributes.Contains("jarvis_latitude") || targetEntity.Attributes.Contains("jarvis_longitude"))
                                {
                                    ////IOrganizationService adminService = orgfactory.CreateOrganizationService(null);
                                    decimal latitude = 0;
                                    decimal longitude = 0;
                                    if (targetEntity.Attributes.Contains("jarvis_latitude") && targetEntity.Attributes["jarvis_latitude"] != null)
                                    {
                                        latitude = (decimal)targetEntity.Attributes["jarvis_latitude"];
                                    }
                                    else if (incidentPre.Attributes.Contains("jarvis_latitude") && incidentPre.Attributes["jarvis_latitude"] != null)
                                    {
                                        latitude = (decimal)incidentPre.Attributes["jarvis_latitude"];
                                    }

                                    if (targetEntity.Attributes.Contains("jarvis_longitude") && targetEntity.Attributes["jarvis_longitude"] != null)
                                    {
                                        longitude = (decimal)targetEntity.Attributes["jarvis_longitude"];
                                    }
                                    else if (incidentPre.Attributes.Contains("jarvis_longitude") && incidentPre.Attributes["jarvis_longitude"] != null)
                                    {
                                        longitude = (decimal)incidentPre.Attributes["jarvis_longitude"];
                                    }

                                    EntityCollection retriveWO = adminService.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getWorkOrders, incidentPre.Id)));
                                    if (retriveWO.Entities.Count == 1)
                                    {
                                        Entity workOrder = new Entity("msdyn_workorder");
                                        workOrder.Id = retriveWO.Entities[0].Id;
                                        workOrder["msdyn_latitude"] = Convert.ToDouble(latitude);
                                        workOrder["msdyn_longitude"] = Convert.ToDouble(longitude);
                                        adminService.Update(workOrder);
                                    }
                                }
#pragma warning restore SA1123 // Do not place regions within elements
                                #endregion

#pragma warning disable SA1123 // Do not place regions within elements
                                #region Average Eta Calculation
                                if ((targetEntity.Attributes.Contains(Incident.JarvisCountry) || targetEntity.Attributes.Contains(Incident.casetypecode) || targetEntity.Attributes.Contains(Incident.JarvisAssistancetype)) && incidentPre != null && incidentPre.Attributes.Contains(Incident.CreatedOn) &&
                                    incidentPre.Attributes[Incident.CreatedOn] != null)
                                {
                                    EntityReference countryReference = null;
                                    bool isBreakdown = false;
                                    bool isBreakdownImmediate = false;
                                    if (targetEntity.Attributes.Contains(Incident.JarvisCountry))
                                    {
                                        if (targetEntity.Attributes[Incident.JarvisCountry] != null)
                                        {
                                            countryReference = targetEntity.GetAttributeValue<EntityReference>(Incident.JarvisCountry);
                                        }
                                    }
                                    else if (incidentPre.Attributes.Contains(Incident.JarvisCountry) && incidentPre.Attributes[Incident.JarvisCountry] != null)
                                    {
                                        countryReference = incidentPre.GetAttributeValue<EntityReference>(Incident.JarvisCountry);
                                    }

                                    if (targetEntity.Attributes.Contains(Incident.casetypecode))
                                    {
                                        if (targetEntity.Attributes[Incident.casetypecode] != null && ((OptionSetValue)targetEntity.Attributes[Incident.casetypecode]).Value == (int)CaseTypeCode.Breakdown)
                                        {
                                            isBreakdown = true;
                                        }
                                    }
                                    else if (incidentPre.Attributes.Contains(Incident.casetypecode) && incidentPre.Attributes[Incident.casetypecode] != null && ((OptionSetValue)incidentPre.Attributes[Incident.casetypecode]).Value == (int)CaseTypeCode.Breakdown)
                                    {
                                        isBreakdown = true;
                                    }

                                    if (targetEntity.Attributes.Contains(Incident.JarvisAssistancetype))
                                    {
                                        if (targetEntity.Attributes[Incident.JarvisAssistancetype] != null && ((OptionSetValue)targetEntity.Attributes[Incident.JarvisAssistancetype]).Value == (int)AssistanceType.Breakdown_immediate)
                                        {
                                            isBreakdownImmediate = true;
                                        }
                                    }
                                    else if (incidentPre.Attributes.Contains(Incident.JarvisAssistancetype) && incidentPre.Attributes[Incident.JarvisAssistancetype] != null && ((OptionSetValue)incidentPre.Attributes[Incident.JarvisAssistancetype]).Value == (int)AssistanceType.Breakdown_immediate)
                                    {
                                        isBreakdownImmediate = true;
                                    }

                                    if (countryReference != null && isBreakdown && isBreakdownImmediate)
                                    {
                                        Entity country = orgService.Retrieve(countryReference.LogicalName, countryReference.Id, new ColumnSet(JarvisCountry.JarvisAverageetaduration));
                                        tracingService.Trace("Incident country - " + countryReference.Id.ToString());
                                        ////Update Case
                                        if (country != null && country.Contains(JarvisCountry.JarvisAverageetaduration) && country.Attributes[JarvisCountry.JarvisAverageetaduration] != null)
                                        {
                                            tracingService.Trace("Country Average ETA - " + country.Attributes[JarvisCountry.JarvisAverageetaduration].ToString());
                                            tracingService.Trace("Case created on - " + DateTime.Parse(incidentPre.Attributes[Incident.CreatedOn].ToString()));
                                            targetEntity[Incident.JarvisAverageEta] = DateTime.Parse(incidentPre.Attributes[Incident.CreatedOn].ToString()).AddHours(double.Parse(country.Attributes[JarvisCountry.JarvisAverageetaduration].ToString()));
                                            tracingService.Trace("Case Average ETA - " + targetEntity.Attributes[Incident.JarvisAverageEta].ToString());
                                        }
                                        else
                                        {
                                            targetEntity[Incident.JarvisAverageEta] = null;
                                        }
                                    }
                                    else
                                    {
                                        targetEntity[Incident.JarvisAverageEta] = null;
                                    }
                                }
#pragma warning restore SA1123 // Do not place regions within elements
                                #endregion

#pragma warning disable SA1123 // Do not place regions within elements
                                #region AddIncidentNatureSubgriddata
                                Incidentnature incident_nat = new Incidentnature();
                                if (targetEntity.Attributes.Contains("jarvis_incidentnature") && targetEntity.Attributes["jarvis_incidentnature"] != null) ////|| (targetEntity.Attributes.Contains("jarvis_vehicle") && targetEntity.Attributes["jarvis_vehicle"] != null)
                                {
                                    tracingService.Trace("Entered AddIncidentNatureSubgriddata of Case");
                                    tracingService.Trace("Incident Nature Associate");
                                    tracingService.Trace("KS: Incident Nature Update");
                                    incident_nat.AssociateIncidentNaturetoCaseMultiselect(targetEntity, incidentPre, orgService, context.MessageName.ToUpper());
                                    incident_nat.AssociateKnowledgeSearch(incident, incidentPre, orgService, tracingService);
                                    //// if (targetEntity.Attributes.Contains("jarvis_vehicle") && targetEntity.Attributes["jarvis_vehicle"] != null)
                                    //// {
                                    //// EntityReference vehicle = (EntityReference)targetEntity.Attributes["jarvis_vehicle"];
                                    ////    if (incidentPre.Attributes.Contains("jarvis_vehicle") && incidentPre.Attributes["jarvis_vehicle"] != null)
                                    ////    {
                                    ////        EntityReference prevehicle = (EntityReference)incidentPre.Attributes["jarvis_vehicle"];
                                    ////        if (prevehicle.Id != vehicle.Id)
                                    ////        {
                                    ////            tracingService.Trace("Entered AddIncidentNatureSubgriddata of Case");
                                    ////            tracingService.Trace("Incident Nature Associate");
                                    ////            incident_nat.AssociateIncidentNaturetoCaseMultiselect(targetEntity, incidentPre, orgService, context.MessageName.ToUpper());
                                    ////        }
                                    ////    }
                                    ////}
                                }

                                if (targetEntity.Attributes.Contains("jarvis_vehicle") && targetEntity.Attributes["jarvis_vehicle"] != null)
                                {
                                    EntityReference vehicle = (EntityReference)targetEntity.Attributes["jarvis_vehicle"];
                                    if (!incidentPre.Attributes.Contains("jarvis_vehicle") && vehicle != null)
                                    {
                                        tracingService.Trace("Entered AddIncidentNatureSubgriddata of Case");
                                        tracingService.Trace("Incident Nature Associate");
                                        tracingService.Trace("KS: On Vehicle Update");
                                        incident_nat.AssociateIncidentNaturetoCaseMultiselect(targetEntity, incidentPre, orgService, context.MessageName.ToUpper());
                                        incident_nat.AssociateKnowledgeSearch(incident, incidentPre, orgService, tracingService);
                                    }
                                    else if (incidentPre.Attributes.Contains("jarvis_vehicle") && incidentPre.Attributes["jarvis_vehicle"] != null)
                                    {
                                        EntityReference prevehicle = (EntityReference)incidentPre.Attributes["jarvis_vehicle"];
                                        if (prevehicle.Id != vehicle.Id)
                                        {
                                            tracingService.Trace("Entered AddIncidentNatureSubgriddata of Case");
                                            tracingService.Trace("Incident Nature Associate");
                                            tracingService.Trace("KS: On Vehicle Update");
                                            incident_nat.AssociateIncidentNaturetoCaseMultiselect(targetEntity, incidentPre, orgService, context.MessageName.ToUpper());
                                            incident_nat.AssociateKnowledgeSearch(incident, incidentPre, orgService, tracingService);
                                        }
                                    }
                                }

                                tracingService.Trace($"KS: check: {targetEntity.Attributes.Contains("jarvis_country")}");
                                if (targetEntity.Attributes.Contains("jarvis_country"))
                                {
                                    EntityReference caseCountry = (EntityReference)targetEntity.Attributes["jarvis_country"];
                                    if ((!incidentPre.Attributes.Contains("jarvis_country") && caseCountry != null) || caseCountry == null)
                                    {
                                        tracingService.Trace("KS: On Country Update");
                                        incident_nat.AssociateKnowledgeSearch(incident, incidentPre, orgService, tracingService);
                                    }
                                    else if (incidentPre.Attributes.Contains("jarvis_country") && incidentPre.Attributes["jarvis_country"] != null)
                                    {
                                        EntityReference preCaseCountry = (EntityReference)incidentPre.Attributes["jarvis_country"];
                                        if (preCaseCountry.Id != caseCountry.Id)
                                        {
                                            tracingService.Trace("KS: On Country Update");
                                            incident_nat.AssociateKnowledgeSearch(incident, incidentPre, orgService, tracingService);
                                        }
                                    }
                                }

                                if (targetEntity.Attributes.Contains("jarvis_homedealer"))
                                {
                                    EntityReference caseHomeDealer = (EntityReference)targetEntity.Attributes["jarvis_homedealer"];
                                    if ((!incidentPre.Attributes.Contains("jarvis_homedealer") && caseHomeDealer != null) || caseHomeDealer == null)
                                    {
                                        tracingService.Trace("KS: On HomeDealer Update");
                                        incident_nat.AssociateKnowledgeSearch(incident, incidentPre, orgService, tracingService);
                                    }
                                    else if (incidentPre.Attributes.Contains("jarvis_homedealer") && incidentPre.Attributes["jarvis_homedealer"] != null)
                                    {
                                        EntityReference preCaseHomeDealer = (EntityReference)incidentPre.Attributes["jarvis_homedealer"];
                                        if (preCaseHomeDealer.Id != caseHomeDealer.Id)
                                        {
                                            tracingService.Trace("KS: On HomeDealer Update");
                                            incident_nat.AssociateKnowledgeSearch(incident, incidentPre, orgService, tracingService);
                                        }
                                    }
                                }

                                if (targetEntity.Attributes.Contains("casetypecode") && targetEntity.Attributes["casetypecode"] != null)
                                {
                                    OptionSetValue caseType = (OptionSetValue)targetEntity.Attributes["casetypecode"];

                                    if (incidentPre.Attributes.Contains("casetypecode") && incidentPre.Attributes["casetypecode"] != null)
                                    {
                                        OptionSetValue precaseType = (OptionSetValue)incidentPre.Attributes["casetypecode"];
                                        if (precaseType.Value != caseType.Value)
                                        {
                                            tracingService.Trace("KS: On Case Type Update");
                                            incident_nat.AssociateKnowledgeSearch(incident, incidentPre, orgService, tracingService);
                                        }
                                    }
                                }

                                if (targetEntity.Attributes.Contains("parentcaseid") && targetEntity.Attributes["parentcaseid"] != null)
                                {
                                    EntityReference parentCase = (EntityReference)targetEntity.Attributes["parentcaseid"];
                                    if (!incidentPre.Attributes.Contains("parentcaseid") && parentCase != null)
                                    {
                                        tracingService.Trace("KS: On Parent Case Update");
                                        incident_nat.AssociateKnowledgeSearch(incident, incidentPre, orgService, tracingService);
                                    }
                                    else if (incidentPre.Attributes.Contains("parentcaseid") && incidentPre.Attributes["parentcaseid"] != null)
                                    {
                                        EntityReference preParentCase = (EntityReference)incidentPre.Attributes["parentcaseid"];
                                        if (preParentCase.Id != parentCase.Id)
                                        {
                                            tracingService.Trace("KS: On Parent Case Update");
                                            incident_nat.AssociateKnowledgeSearch(incident, incidentPre, orgService, tracingService);
                                        }
                                    }
                                }

                                if (targetEntity.Attributes.Contains("isescalated") || targetEntity.Attributes.Contains("jarvis_escalationmaincategory"))
                                {
                                    tracingService.Trace("KS: On Escalate and Escalation Category Update");
                                    incident_nat.AssociateKnowledgeSearch(incident, incidentPre, orgService, tracingService);
                                }
#pragma warning restore SA1123 // Do not place regions within elements
                                #endregion

                                ////Set Time zone
                                EntityReference countryPre = new EntityReference();
                                if (incidentPre.Attributes.Contains("jarvis_country") && incidentPre.Attributes["jarvis_country"] != null)
                                {
                                    countryPre = (EntityReference)incidentPre.Attributes["jarvis_country"];
                                }

                                if (targetEntity.Attributes.Contains("jarvis_country") && targetEntity.Attributes["jarvis_country"] != null)
                                {
                                    OptionSetValue incidentStatus = new OptionSetValue();
                                    if (incidentPre.Attributes.Contains("statuscode") && incidentPre.Attributes["statuscode"] != null)
                                    {
                                        incidentStatus = (OptionSetValue)incidentPre.Attributes["statuscode"];
                                    }

                                    EntityReference country = (EntityReference)targetEntity.Attributes["jarvis_country"];
                                    if (country != null && countryPre != null)
                                    {
                                        this.SetTimeZoneFields(orgService, tracingService, targetEntity);
                                    }
                                    else
                                    {
                                        this.SetTimeZoneFields(orgService, tracingService, targetEntity);
                                    }
                                }

                                this.SetCaseRegistrationShadow(orgService, tracingService, targetEntity);
                            }

                            if (context.MessageName.ToString().ToUpper() == "CREATE")
                            {
#pragma warning disable SA1123 // Do not place regions within elements
                                #region if Customer Blacklisted
                                if (targetEntity.Attributes.Contains("customerid") && targetEntity.Attributes["customerid"] != null && targetEntity.Attributes.Contains("createdby") && targetEntity.Attributes["createdby"] != null)
                                {
                                    EntityReference createdBy = (EntityReference)targetEntity.Attributes["createdby"];
                                    Entity userFullname = orgService.Retrieve(createdBy.LogicalName, createdBy.Id, new ColumnSet("fullname"));
                                    if (userFullname != null && userFullname.Attributes.Contains("fullname"))
                                    {
                                        string fullName = (string)userFullname.Attributes["fullname"];
                                        if (!fullName.ToUpper().Contains("MERCURIUS"))
                                        {
                                            tracingService.Trace("is mercurius user" + fullName.ToUpper().Contains("MERCURIUS"));
                                            EntityReference caseCustomer = (EntityReference)targetEntity.Attributes["customerid"];
                                            Entity jarvisDealer = orgService.Retrieve(caseCustomer.LogicalName, caseCustomer.Id, new ColumnSet("jarvis_blacklisted"));
                                            if (jarvisDealer != null && jarvisDealer.Attributes.Contains("jarvis_blacklisted") && (bool)jarvisDealer.Attributes["jarvis_blacklisted"])
                                            {
                                                throw new InvalidPluginExecutionException("You cannot save the case because the customer is blacklisted, please inform customer that VAS cannot assist.");
                                            }
                                        }
                                    }
                                }
#pragma warning restore SA1123 // Do not place regions within elements

                                #endregion

#pragma warning disable SA1123 // Do not place regions within elements
                                #region Set Route Case
                                targetEntity["routecase"] = false;
#pragma warning restore SA1123 // Do not place regions within elements
                                #endregion

                                // 110619
#pragma warning disable SA1123 // Do not place regions within elements
                                #region Update Brand and Code

                                if (targetEntity.Attributes.Contains("jarvis_vehicle") && targetEntity.Attributes["jarvis_vehicle"] != null)
                                {
                                    EntityReference vehicle = (EntityReference)targetEntity.Attributes["jarvis_vehicle"];
                                    Entity vehicleEntity = orgService.Retrieve(vehicle.LogicalName, vehicle.Id, new ColumnSet("jarvis_brandid"));
                                    if (vehicleEntity.Attributes.Contains("jarvis_brandid") && vehicleEntity.Attributes["jarvis_brandid"] != null)
                                    {
                                        // jarvis_brand
                                        targetEntity["jarvis_brand"] = (EntityReference)vehicleEntity.Attributes["jarvis_brandid"];

                                        Entity brand = orgService.Retrieve(((EntityReference)vehicleEntity.Attributes["jarvis_brandid"]).LogicalName, ((EntityReference)vehicleEntity.Attributes["jarvis_brandid"]).Id, new ColumnSet("jarvis_mercuriusbrandcode"));
                                        if (brand.Attributes.Contains("jarvis_mercuriusbrandcode") && brand.Attributes["jarvis_mercuriusbrandcode"] != null)
                                        {
                                            targetEntity["jarvis_mercuriusbrand"] = (string)brand.Attributes["jarvis_mercuriusbrandcode"];
                                        }
                                    }
                                }
#pragma warning restore SA1123 // Do not place regions within elements

                                #endregion

#pragma warning disable SA1123 // Do not place regions within elements
                                #region Query - GOP Validity

                                if (targetEntity.Attributes.Contains("casetypecode") && targetEntity.Attributes["casetypecode"] != null)
                                {
                                    OptionSetValue caseType = (OptionSetValue)targetEntity.Attributes["casetypecode"];
                                    if (caseType.Value == 3)
                                    {
                                        // Query
                                        if (targetEntity.Attributes.Contains("parentcaseid") && targetEntity.Attributes["parentcaseid"] != null)
                                        {
                                            EntityReference parentCase = (EntityReference)targetEntity.Attributes["parentcaseid"];
                                            EntityCollection gop = orgService.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getLastModGOP, parentCase.Id)));

                                            if (gop.Entities.Count > 0)
                                            {
                                                DateTime modifiedOn = (DateTime)gop.Entities[0].Attributes["modifiedon"];
                                                modifiedOn = modifiedOn.AddDays(60);
                                                targetEntity["jarvis_gopvalidity"] = modifiedOn;
                                            }
                                        }

                                        targetEntity["statuscode"] = new OptionSetValue(110);
                                    }

                                    this.UpdateTimeStamp(targetEntity, 1);
                                }
#pragma warning restore SA1123 // Do not place regions within elements

                                #endregion

#pragma warning disable SA1123 // Do not place regions within elements
                                #region Update Case Location (National/International/Global) And GOP Currencies(Total requested, Approved, case avaialable Amount)
                                CaseOperations caseOperations = new CaseOperations();
#pragma warning restore SA1123 // Do not place regions within elements
                                caseOperations.UpdateCaseLocationAndGOPCurrency(orgService, tracingService, targetEntity, targetEntity);
                                #endregion

#pragma warning disable SA1123 // Do not place regions within elements
                                #region Preferred Caller Language
                                if (targetEntity.Attributes.Contains("jarvis_callerlanguage") && targetEntity.Attributes["jarvis_callerlanguage"] != null)
                                {
                                    EntityReference callerPreferredLanguage = (EntityReference)targetEntity.Attributes["jarvis_callerlanguage"];
                                    bool isSupported = false;
                                    Entity callerLanguage = orgService.Retrieve(callerPreferredLanguage.LogicalName, callerPreferredLanguage.Id, new ColumnSet("jarvis_vassupportedlanguage", "jarvis_vasstandardlanguage"));
                                    if (callerLanguage.Attributes.Contains("jarvis_vasstandardlanguage") && callerLanguage.Attributes["jarvis_vasstandardlanguage"] != null)
                                    {
                                        isSupported = (bool)callerLanguage.Attributes["jarvis_vasstandardlanguage"];
                                        if (!isSupported)
                                        {
                                            if (callerLanguage.Attributes.Contains("jarvis_vassupportedlanguage") && callerLanguage.Attributes["jarvis_vassupportedlanguage"] != null)
                                            {
                                                targetEntity["jarvis_callerlanguagesupported"] = (EntityReference)callerLanguage.Attributes["jarvis_vassupportedlanguage"];
                                            }
                                            else
                                            {
                                                targetEntity["jarvis_callerlanguagesupported"] = (EntityReference)targetEntity.Attributes["jarvis_callerlanguage"];
                                            }
                                        }
                                        else
                                        {
                                            targetEntity["jarvis_callerlanguagesupported"] = (EntityReference)targetEntity.Attributes["jarvis_callerlanguage"];
                                        }
                                    }
                                    else
                                    {
                                        targetEntity["jarvis_callerlanguagesupported"] = (EntityReference)targetEntity.Attributes["jarvis_callerlanguage"];
                                    }
                                }
#pragma warning restore SA1123 // Do not place regions within elements
                                #endregion

#pragma warning disable SA1123 // Do not place regions within elements
                                #region Preferred Driver Language
                                if (targetEntity.Attributes.Contains("jarvis_driverlanguage") && targetEntity.Attributes["jarvis_driverlanguage"] != null)
                                {
                                    EntityReference callerPreferredLanguage = (EntityReference)targetEntity.Attributes["jarvis_driverlanguage"];
                                    bool isSupported = false;
                                    Entity callerLanguage = orgService.Retrieve(callerPreferredLanguage.LogicalName, callerPreferredLanguage.Id, new ColumnSet("jarvis_vassupportedlanguage", "jarvis_vasstandardlanguage"));
                                    if (callerLanguage.Attributes.Contains("jarvis_vasstandardlanguage") && callerLanguage.Attributes["jarvis_vasstandardlanguage"] != null)
                                    {
                                        isSupported = (bool)callerLanguage.Attributes["jarvis_vasstandardlanguage"];
                                        if (!isSupported)
                                        {
                                            if (callerLanguage.Attributes.Contains("jarvis_vassupportedlanguage") && callerLanguage.Attributes["jarvis_vassupportedlanguage"] != null)
                                            {
                                                targetEntity["jarvis_driverlanguagesupported"] = (EntityReference)callerLanguage.Attributes["jarvis_vassupportedlanguage"];
                                            }
                                            else
                                            {
                                                targetEntity["jarvis_driverlanguagesupported"] = (EntityReference)targetEntity.Attributes["jarvis_driverlanguage"];
                                            }
                                        }
                                        else
                                        {
                                            targetEntity["jarvis_driverlanguagesupported"] = (EntityReference)targetEntity.Attributes["jarvis_driverlanguage"];
                                        }
                                    }
                                    else
                                    {
                                        targetEntity["jarvis_driverlanguagesupported"] = (EntityReference)targetEntity.Attributes["jarvis_driverlanguage"];
                                    }
                                }
#pragma warning restore SA1123 // Do not place regions within elements
                                #endregion

#pragma warning disable SA1123 // Do not place regions within elements
                                #region AddIncidentNatureSubgriddata
                                if ((targetEntity.Attributes.Contains("jarvis_incidentnature") && targetEntity.Attributes["jarvis_incidentnature"] != null) || (targetEntity.Attributes.Contains("jarvis_vehicle") && targetEntity.Attributes["jarvis_vehicle"] != null))
                                {
                                    tracingService.Trace("Entered AddIncidentNatureSubgriddata of Case");
                                    tracingService.Trace("Incident Nature Associate");
                                    Incidentnature incident_nat = new Incidentnature();
                                    incident_nat.AssociateIncidentNaturetoCaseMultiselect(targetEntity, targetEntity, orgService, context.MessageName.ToUpper());
                                    incident_nat.AssociateKnowledgeSearch(targetEntity, null, orgService, tracingService);
                                }
#pragma warning restore SA1123 // Do not place regions within elements
                                #endregion

#pragma warning disable SA1123 // Do not place regions within elements
                                #region SetTimeZone
                                if (targetEntity.Attributes.Contains("jarvis_country") && targetEntity.Attributes["jarvis_country"] != null)
                                {
                                    this.SetTimeZoneFields(orgService, tracingService, targetEntity);
                                }
#pragma warning restore SA1123 // Do not place regions within elements
                                #endregion

                                this.SetCaseRegistrationShadow(orgService, tracingService, targetEntity);
                                if (incident.Attributes.Contains("statuscode") && incident.Attributes["statuscode"] != null)
                                {
                                    OptionSetValue statusReason = (OptionSetValue)incident.Attributes["statuscode"];
                                    if (statusReason.Value == 20)
                                    {
                                        CaseOperations operations = new CaseOperations();
                                        operations.ValidateCaseOpening(orgService, tracingService, incident, incident);
                                    }
                                }

                                if (targetEntity.Attributes.Contains("casetypecode") && targetEntity.Attributes["casetypecode"] != null)
                                {
                                    OptionSetValue caseTypeCode = (OptionSetValue)targetEntity.Attributes["casetypecode"];

                                    if (caseTypeCode != null && caseTypeCode.Value == 2)
                                    {
                                        ////targetEntity.Attributes["parentcaseid"] = null;
                                        targetEntity.Attributes["title"] = null;
                                    }
                                }
                            }
                        }
                    }
                }
                catch (InvalidPluginExecutionException ex)
                {
                    throw new InvalidPluginExecutionException("Error in Case Pre Operations " + ex.Message + " ");
                }
            }
        }

        /// <summary>
        /// Get Region Based On Country.
        /// </summary>
        /// <param name="service">Org service.</param>
        /// <param name="tracingService">tracing Service.</param>
        /// <param name="countryId">country Id.</param>
        /// <returns>Exception details.</returns>
        private Entity GetRegionBasedOnCountry(IOrganizationService service, ITracingService tracingService, Guid countryId)
        {
            string regionFetchXml = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>
                                          <entity name='jarvis_region'>
                                            <attribute name='jarvis_regionid' />
                                            <attribute name='jarvis_name' />
                                            <attribute name='createdon' />
                                            <order attribute='jarvis_name' descending='false' />
                                            <link-entity name='jarvis_region_jarvis_country' from='jarvis_regionid' to='jarvis_regionid' visible='false' intersect='true'>
                                              <link-entity name='jarvis_country' from='jarvis_countryid' to='jarvis_countryid' alias='ac'>
                                                <filter type='and'>
                                                  <condition attribute='jarvis_countryid' operator='eq'  uitype='jarvis_country' value='{0}' />
                                                </filter>
                                              </link-entity>
                                            </link-entity>
                                          </entity>
                                        </fetch>";

            EntityCollection regionCollection = service.RetrieveMultiple(new FetchExpression(string.Format(regionFetchXml, countryId)));

            if (regionCollection != null && regionCollection.Entities != null && regionCollection.Entities.Count > 0)
            {
                return regionCollection.Entities.FirstOrDefault();
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Update Time Stamp.
        /// </summary>
        /// <param name="target">target details.</param>
        /// <param name="mercuriusStatus">Status param.</param>
        private void UpdateTimeStamp(Entity target, int mercuriusStatus)
        {
            target["jarvis_casestatusupdate"] = DateTime.UtcNow;
        }

        /// <summary>
        /// Get Time zone By Code.
        /// </summary>
        /// <param name="service">Org service.</param>
        /// <param name="tracingService">tracing Service.</param>
        /// <param name="timeZoneCode">country Id.</param>
        /// <returns>Exception details.</returns>
        private Entity GetTimezoneByCode(IOrganizationService service, ITracingService tracingService, int timeZoneCode)
        {
            string timeZoneFetchXml = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>
                                          <entity name='timezonedefinition'>
                                            <attribute name='userinterfacename' />
                                                <filter type='and'>
                                                  <condition attribute='timezonecode' operator='eq'  value='{0}' />
                                                </filter>
                                          </entity>
                                        </fetch>";

            EntityCollection timeZoneCollection = service.RetrieveMultiple(new FetchExpression(string.Format(timeZoneFetchXml, timeZoneCode)));
            if (timeZoneCollection != null && timeZoneCollection.Entities != null && timeZoneCollection.Entities.Count > 0)
            {
                return timeZoneCollection.Entities.FirstOrDefault();
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Set Time Zone Fields.
        /// </summary>
        /// <param name="service">Org service.</param>
        /// <param name="tracingService">tracing Service.</param>
        /// <param name="targetEntity">target Entity.</param>
        private void SetTimeZoneFields(IOrganizationService service, ITracingService tracingService, Entity targetEntity)
        {
            if (targetEntity.Attributes.Contains("modifiedby") && targetEntity.Attributes["modifiedby"] != null)
            {
                EntityReference modifiedByID = (EntityReference)targetEntity.Attributes["modifiedby"];
                string modifiedBy = string.Empty;
                EntityCollection systemuser = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getModifiedUser, modifiedByID.Id)));
                if (systemuser != null && systemuser.Entities.Count > 0)
                {
                    modifiedBy = (string)systemuser.Entities[0].Attributes["fullname"];
                    if (modifiedBy.ToUpperInvariant().Contains("MERCURIUS"))
                    {
                        var timezoneLabel = "(GMT+01:00)";
                        int timeZoneCode = 105;
                        EntityReference country = (EntityReference)targetEntity.Attributes["jarvis_country"];
                        Entity retrievedCountry = service.Retrieve(country.LogicalName, country.Id, new ColumnSet("jarvis_timezone"));
                        if (retrievedCountry.Attributes.Contains("jarvis_timezone") && retrievedCountry.Attributes["jarvis_timezone"] != null)
                        {
                            targetEntity["jarvis_timezone"] = retrievedCountry.Attributes["jarvis_timezone"];
                            timeZoneCode = (int)retrievedCountry.Attributes["jarvis_timezone"];
                            Entity timeZone = this.GetTimezoneByCode(service, tracingService, timeZoneCode);
                            if (timeZone != null && timeZone.Attributes.Contains("userinterfacename") && timeZone.Attributes["userinterfacename"] != null)
                            {
                                var userInterfaceName = timeZone.Attributes["userinterfacename"];
                                timezoneLabel = userInterfaceName.ToString().Substring(0, 11);
                                targetEntity["jarvis_timezonelabel"] = timezoneLabel;
                            }
                        }
                        else
                        {
                            targetEntity["jarvis_timezone"] = timeZoneCode;
                            targetEntity["jarvis_timezonelabel"] = timezoneLabel;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Set Case Registration Number Shadow.
        /// </summary>
        /// <param name="service">Org service.</param>
        /// <param name="tracingService">tracing Service.</param>
        /// <param name="targetEntity">target Entity.</param>
        private void SetCaseRegistrationShadow(IOrganizationService service, ITracingService tracingService, Entity targetEntity)
        {
            if (targetEntity.Attributes.Contains("jarvis_registrationnumber") && targetEntity.Attributes["jarvis_registrationnumber"] != null)
            {
                string regNumber = (string)targetEntity.Attributes["jarvis_registrationnumber"];
                string regNumberShadow = Regex.Replace(regNumber, @"(\s+|@|&|'|\(|\)|<|>|#|-)", " ");
                regNumberShadow = string.Concat(regNumberShadow.Where(c => !char.IsWhiteSpace(c)));
                targetEntity["jarvis_registrationnumbershadow"] = regNumberShadow;
            }
        }
    }
}
