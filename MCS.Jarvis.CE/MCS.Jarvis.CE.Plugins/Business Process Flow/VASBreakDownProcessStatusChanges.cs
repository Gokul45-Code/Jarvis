// <copyright file="VASBreakDownProcessStatusChanges.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace MCS.Jarvis.CE.Plugins.Business_Process_Flow
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using MCS.Jarvis.CE.BusinessProcessesShared.Helpers;
    using MCS.Jarvis.CE.BusinessProcessShared.CaseMonitor;
    using MCS.Jarvis.CE.Commons;
    using Microsoft.Crm.Sdk.Messages;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Client;
    using Microsoft.Xrm.Sdk.Query;
    using static MCS.Jarvis.CE.BusinessProcessesShared.Helpers.Constants;

    /// <summary>
    /// VAS Break Down Process Status Changes.
    /// </summary>
    public class VASBreakDownProcessStatusChanges : IPlugin
    {
        /// <summary>
        /// Execute Method.
        /// </summary>
        /// <param name="serviceProvider">service Provider.</param>
        /// <exception cref="InvalidPluginExecutionException">Invalid Plugin Execution Exception.</exception>
        public void Execute(IServiceProvider serviceProvider)
        {
            // Extract the tracing service for use in debugging sandboxed plug-ins.
            ITracingService tracingService =
                (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            // Obtain the execution context from the service provider.
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                try
                {
                    IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                    IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
                    IOrganizationService adminservice = serviceFactory.CreateOrganizationService(null);
                    tracingService.Trace("Entered plugin");

                    Entity targetEntity = (Entity)context.PostEntityImages["PostImage"];
                    tracingService.Trace("Entered PostImage");

#pragma warning disable SA1123 // Do not place regions within elements
                    #region Update

                    if (context.Stage == 40 && context.MessageName.ToUpper() == "UPDATE")
                    {
                        Entity pretargetEntity = (Entity)context.PreEntityImages["PreImage"];
                        tracingService.Trace("Entered PreImage");
                        EntityReference currentBpfStageId = targetEntity.Attributes.Contains("activestageid") ? targetEntity.GetAttributeValue<EntityReference>("activestageid") : null;
                        EntityReference preBpfStageId = pretargetEntity.Attributes.Contains("activestageid") ? pretargetEntity.GetAttributeValue<EntityReference>("activestageid") : null;
                        OptionSetValue bpfStatus = targetEntity.Attributes.Contains("statuscode") ? targetEntity.GetAttributeValue<OptionSetValue>("statuscode") : null;
                        bool canMove = this.StageCheck(currentBpfStageId.Name, preBpfStageId.Name);
                        if (!canMove)
                        {
                            throw new InvalidPluginExecutionException("Cannot reactivate an already completed stage");
                        }

                        EntityReference incident = targetEntity.Attributes.Contains("bpf_incidentid") ? targetEntity.GetAttributeValue<EntityReference>("bpf_incidentid") : null;
                        if (currentBpfStageId != null)
                        {
                            tracingService.Trace("Entered currentBpfStageId");
                            if (incident != null)
                            {
                                tracingService.Trace("Entered cincident");
                                tracingService.Trace("Entered here");
                                bool caseUpdate = false;
                                bool closeCase = false;
                                Entity parentCase = new Entity(incident.LogicalName);
                                parentCase.Id = incident.Id;
                                Entity incidentRetrieve = service.Retrieve(incident.LogicalName, incident.Id, new ColumnSet("createdon", "caseorigincode", "statecode", "casetypecode", "jarvis_mercuriusstatus", "statuscode", "jarvis_querydecisioncatagory", "jarvis_casestatusupdate", "jarvis_automationcriteriamet", "jarvis_hdrd", "ownerid"));
                                OptionSetValue statecode = (OptionSetValue)incidentRetrieve.Attributes["statecode"];
                                OptionSetValue caseTypecode = (OptionSetValue)incidentRetrieve.Attributes["casetypecode"];
                                OptionSetValue statusCode = (OptionSetValue)incidentRetrieve.Attributes["statuscode"];
                                OptionSetValue mercStatus = (OptionSetValue)incidentRetrieve.Attributes["jarvis_mercuriusstatus"];
                                bool caseOpeningValidation = CrmHelper.GetAutomationConfig(service, JarvisConfiguration.CaseOpeningValidation, tracingService);
                                bool isTrigerredFromMercurius = false;

                                if (incidentRetrieve.Attributes.Contains("ownerid") && incidentRetrieve.Attributes["ownerid"] != null)
                                {
                                    EntityReference createdBy = (EntityReference)incidentRetrieve.Attributes["ownerid"];
                                    if (createdBy.Name.ToUpperInvariant().Contains("MERCURIUS"))
                                    {
                                        isTrigerredFromMercurius = true;
                                    }
                                }

                                DateTime caseUpdateTime = default(DateTime);
                                bool automationcriteriamet = false;
                                if (incidentRetrieve.Attributes.Contains("jarvis_automationcriteriamet") && incidentRetrieve.Attributes["jarvis_automationcriteriamet"] != null)
                                {
                                    automationcriteriamet = (bool)incidentRetrieve.Attributes["jarvis_automationcriteriamet"];
                                }
                                else if (pretargetEntity.Attributes.Contains("jarvis_automationcriteriamet") && pretargetEntity.Attributes["jarvis_automationcriteriamet"] != null)
                                {
                                    automationcriteriamet = (bool)pretargetEntity.Attributes["jarvis_automationcriteriamet"];
                                }

                                // Check if Automation Criteria Met
                                if (bpfStatus.Value == 2)
                                {
                                    CaseMonitorProcess operation = new CaseMonitorProcess();
                                    OptionSetValue releaseConfig = new OptionSetValue();
                                    releaseConfig = CrmHelper.GetReleaseAutomationConfig(service, MCS.Jarvis.CE.Plugins.Constants.JarvisConfiguration.Automationreleasecase, tracingService);

                                    // Query
                                    if (caseTypecode.Value == 3)
                                    {
                                        operation.CloseMonitorActions(incidentRetrieve, service, tracingService);
                                    }
                                    else
                                    {
                                        if (releaseConfig.Value == 1)
                                        {
                                            operation.CloseMonitorActions(incidentRetrieve, service, tracingService);
                                        }
                                        else
                                        {
                                            if (!(incidentRetrieve.Contains(MCS.Jarvis.CE.Plugins.Constants.Incident.JarvisHDRD) && (bool)incidentRetrieve.Attributes[MCS.Jarvis.CE.Plugins.Constants.Incident.JarvisHDRD]))
                                            {
                                                EntityCollection monitorActions = service.RetrieveMultiple(new FetchExpression(string.Format(MCS.Jarvis.CE.Plugins.Constants.FetchXmls.getOpenMO, incident.Id)));
                                                if (monitorActions.Entities.Count > 0)
                                                {
                                                    throw new InvalidPluginExecutionException("Please complete open Monitor Action(s) before closing the case");
                                                }
                                            }
                                        }
                                    }

                                    if (releaseConfig.Value == 3)
                                    {
                                        if (!(incidentRetrieve.Contains(MCS.Jarvis.CE.Plugins.Constants.Incident.JarvisHDRD) && (bool)incidentRetrieve.Attributes[MCS.Jarvis.CE.Plugins.Constants.Incident.JarvisHDRD]))
                                        {
                                            EntityCollection passouts = service.RetrieveMultiple(new FetchExpression(string.Format(MCS.Jarvis.CE.Plugins.Constants.FetchXmls.getOpenPassOuts, incident.Id)));
                                            if (passouts.Entities.Count > 0)
                                            {
                                                throw new InvalidPluginExecutionException("Please fulfill the missing timestamps on Pass Out(s) before closing the case");
                                            }

                                            //EntityCollection gop = service.RetrieveMultiple(new FetchExpression(string.Format(MCS.Jarvis.CE.Plugins.Constants.FetchXmls.getUnApprovedGOP, incident.Id)));
                                            //if (gop.Entities.Count > 0)
                                            //{
                                            //    throw new InvalidPluginExecutionException("One or more GOP request(s) is still open in the current case. Please check the GOP request(s) and update the GOP before proceeding.");
                                            //}

                                            EntityCollection passout = service.RetrieveMultiple(new FetchExpression(string.Format(MCS.Jarvis.CE.Plugins.Constants.FetchXmls.CaseActivePassouts, incidentRetrieve.Id)));
                                            if (passout.Entities.Count > 0)
                                            {
                                                tracingService.Trace("passouts value is " + passout.Entities.Count.ToString() + " ");
                                                foreach (var item in passout.Entities)
                                                {
                                                    EntityCollection jed = service.RetrieveMultiple(new FetchExpression(string.Format(MCS.Jarvis.CE.BusinessProcessesShared.Helpers.Constants.FetchXmls.getHasBeenSentJEDs, incidentRetrieve.Id, item.Id)));
                                                    tracingService.Trace("jed value is " + jed.Entities.Count.ToString() + " ");
                                                    if (jed.Entities.Count == 0)
                                                    {
                                                        throw new InvalidPluginExecutionException("Job End Details for one or more Pass Outs is not been sent");
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    caseUpdate = true;
                                    if (statusCode.Value != 5)
                                    {
                                        closeCase = true;
                                        EntityCollection user = service.RetrieveMultiple(new FetchExpression(string.Format(MCS.Jarvis.CE.Plugins.Constants.FetchXmls.getUnAssignedUser)));
                                        if (user.Entities.Count > 0 && caseTypecode.Value == 2)
                                        {
                                            parentCase["ownerid"] = user.Entities[0].ToEntityReference();
                                        }

                                        if (mercStatus.Value != 900)
                                        {
                                            parentCase["jarvis_mercuriusstatus"] = new OptionSetValue(900);
                                        }
                                    }
                                }

                                if (incidentRetrieve.Attributes.Contains("jarvis_casestatusupdate") && incidentRetrieve.Attributes["jarvis_casestatusupdate"] != null)
                                {
                                    caseUpdateTime = (DateTime)incidentRetrieve.Attributes["jarvis_casestatusupdate"];
                                }
                                else
                                {
                                    caseUpdateTime = (DateTime)incidentRetrieve.Attributes["createdon"];
                                }

                                if (statecode.Value == 0)
                                {
#pragma warning disable SA1123 // Do not place regions within elements
                                    #region set StatusCode and Mercurius Status

#pragma warning disable SA1123 // Do not place regions within elements
                                    #region Breakdown

                                    // Breakdown
#pragma warning disable SA1123 // Do not place regions within elements
                                    if (caseTypecode.Value != 3)
                                    {
                                        if (currentBpfStageId.Name == Constants.Incident.bpfStage1)
                                        {
                                            if (statusCode.Value != 10)
                                            {
                                                parentCase["statuscode"] = new OptionSetValue(10);
                                                if (incidentRetrieve.Attributes.Contains("caseorigincode") && incidentRetrieve.Attributes["caseorigincode"] != null)
                                                {
                                                    OptionSetValue caseOrigin = (OptionSetValue)incidentRetrieve.Attributes["caseorigincode"];
                                                    tracingService.Trace("Case Origin :" + caseOrigin.Value + " ");

                                                    // Phone (Manual Case Creation)
                                                    if (caseOrigin.Value == 1)
                                                    {
                                                        caseUpdate = true;
                                                        parentCase["jarvis_mercuriusstatus"] = new OptionSetValue(000);
                                                        parentCase["jarvis_casestatusupdate"] = DateTime.UtcNow;
                                                    }
                                                    else
                                                    {
                                                        caseUpdate = true;
                                                        parentCase["jarvis_mercuriusstatus"] = new OptionSetValue(0);
                                                        parentCase["jarvis_casestatusupdate"] = DateTime.UtcNow;
                                                    }
                                                }
                                            }
                                        }

                                        if (currentBpfStageId.Name == Constants.Incident.bpfStage2)
                                        {
                                            if (statusCode.Value != 20)
                                            {
                                                caseUpdate = true;
                                                parentCase["statuscode"] = new OptionSetValue(20);
                                                parentCase["jarvis_mercuriusstatus"] = new OptionSetValue(100);
                                                if (caseUpdateTime != null)
                                                {
                                                    parentCase["jarvis_canvasdate"] = (DateTime)caseUpdateTime;
                                                }

                                                parentCase["jarvis_casestatusupdate"] = DateTime.UtcNow;
                                            }
                                        }

                                        if (currentBpfStageId.Name == Constants.Incident.bpfStage3)
                                        {
                                            if (statusCode.Value != 30)
                                            {
                                                caseUpdate = true;
                                                tracingService.Trace(Constants.Incident.bpfStage3);
                                                parentCase["statuscode"] = new OptionSetValue(30);
                                                if (caseUpdateTime != null)
                                                {
                                                    parentCase["jarvis_canvasdate"] = (DateTime)caseUpdateTime;
                                                }

                                                parentCase["jarvis_mercuriusstatus"] = new OptionSetValue(200);
                                                parentCase["jarvis_casestatusupdate"] = DateTime.UtcNow;
                                            }
                                        }

                                        if (currentBpfStageId.Name == Constants.Incident.bpfStage4)
                                        {
                                            if (statusCode.Value != 40)
                                            {
                                                caseUpdate = true;
                                                parentCase["statuscode"] = new OptionSetValue(40);
                                                if (caseUpdateTime != null)
                                                {
                                                    parentCase["jarvis_canvasdate"] = (DateTime)caseUpdateTime;
                                                }

                                                parentCase["jarvis_mercuriusstatus"] = new OptionSetValue(300);
                                                parentCase["jarvis_casestatusupdate"] = DateTime.UtcNow;
                                            }
                                        }

                                        if (currentBpfStageId.Name == Constants.Incident.bpfStage5)
                                        {
                                            if (statusCode.Value != 50)
                                            {
                                                caseUpdate = true;
                                                if (caseUpdateTime != null)
                                                {
                                                    parentCase["jarvis_canvasdate"] = (DateTime)caseUpdateTime;
                                                }

                                                parentCase["statuscode"] = new OptionSetValue(50);
                                                parentCase["jarvis_mercuriusstatus"] = new OptionSetValue(400);
                                                parentCase["jarvis_casestatusupdate"] = DateTime.UtcNow;
                                            }
                                        }

                                        if (currentBpfStageId.Name == Constants.Incident.bpfStage6)
                                        {
                                            if (statusCode.Value != 60)
                                            {
                                                caseUpdate = true;
                                                parentCase["statuscode"] = new OptionSetValue(60);
                                                if (caseUpdateTime != null)
                                                {
                                                    parentCase["jarvis_canvasdate"] = (DateTime)caseUpdateTime;
                                                }

                                                parentCase["jarvis_mercuriusstatus"] = new OptionSetValue(500);
                                                parentCase["jarvis_casestatusupdate"] = DateTime.UtcNow;
                                            }
                                        }

                                        if (currentBpfStageId.Name == Constants.Incident.bpfStage7)
                                        {
                                            if (statusCode.Value != 70)
                                            {
                                                caseUpdate = true;
                                                parentCase["statuscode"] = new OptionSetValue(70);
                                                if (caseUpdateTime != null)
                                                {
                                                    parentCase["jarvis_canvasdate"] = (DateTime)caseUpdateTime;
                                                }

                                                parentCase["jarvis_mercuriusstatus"] = new OptionSetValue(600);
                                                parentCase["jarvis_casestatusupdate"] = DateTime.UtcNow;
                                            }
                                        }

                                        if (currentBpfStageId.Name == Constants.Incident.bpfStage8)
                                        {
                                            if (statusCode.Value != 80)
                                            {
                                                caseUpdate = true;
                                                parentCase["statuscode"] = new OptionSetValue(80);
                                                if (caseUpdateTime != null)
                                                {
                                                    parentCase["jarvis_canvasdate"] = (DateTime)caseUpdateTime;
                                                }

                                                parentCase["jarvis_mercuriusstatus"] = new OptionSetValue(700);
                                                parentCase["jarvis_casestatusupdate"] = DateTime.UtcNow;
                                            }
                                        }

                                        if (currentBpfStageId.Name == Constants.Incident.bpfStage9 && statecode.Value == 0)
                                        {
                                            if (statusCode.Value != 90)
                                            {
                                                caseUpdate = true;
                                                parentCase["statuscode"] = new OptionSetValue(90);
                                                if (caseUpdateTime != null)
                                                {
                                                    parentCase["jarvis_canvasdate"] = (DateTime)caseUpdateTime;
                                                }

                                                parentCase["jarvis_mercuriusstatus"] = new OptionSetValue(800);
                                                parentCase["jarvis_casestatusupdate"] = DateTime.UtcNow;
                                            }
                                        }

                                        if (currentBpfStageId.Name == Constants.Incident.bpfStage9 && statecode.Value == 1)
                                        {
                                            caseUpdate = true;
                                            parentCase["jarvis_mercuriusstatus"] = new OptionSetValue(900);
                                        }
                                    }
                                    #endregion

                                    #region Query
                                    else
                                    {
                                        if (currentBpfStageId.Name == Constants.Incident.bpfStage1)
                                        {
                                            if (statusCode.Value != 110)
                                            {
                                                caseUpdate = true;
                                                parentCase["statuscode"] = new OptionSetValue(110);
                                                parentCase["jarvis_casestatusupdate"] = DateTime.UtcNow;
                                            }
                                        }

                                        if (currentBpfStageId.Name == Constants.Incident.bpfStage10)
                                        {
                                            if (statusCode.Value != 120)
                                            {
                                                caseUpdate = true;
                                                parentCase["statuscode"] = new OptionSetValue(120);
                                                if (caseUpdateTime != null)
                                                {
                                                    parentCase["jarvis_canvasdate"] = (DateTime)caseUpdateTime;
                                                }

                                                parentCase["jarvis_casestatusupdate"] = DateTime.UtcNow;
                                            }
                                        }

                                        if (currentBpfStageId.Name == Constants.Incident.bpfStage11)
                                        {
                                            if (statusCode.Value != 130)
                                            {
                                                caseUpdate = true;
                                                parentCase["statuscode"] = new OptionSetValue(130);
                                                if (caseUpdateTime != null)
                                                {
                                                    parentCase["jarvis_canvasdate"] = (DateTime)caseUpdateTime;
                                                }

                                                parentCase["jarvis_casestatusupdate"] = DateTime.UtcNow;
                                            }
                                        }

                                        if (currentBpfStageId.Name == Constants.Incident.bpfStage12)
                                        {
                                            if (statusCode.Value != 140)
                                            {
                                                if (incidentRetrieve.Attributes.Contains("jarvis_querydecisioncatagory") && incidentRetrieve.Attributes["jarvis_querydecisioncatagory"] != null)
                                                {
                                                    EntityReference decisionCategory = (EntityReference)incidentRetrieve.Attributes["jarvis_querydecisioncatagory"];
                                                    Entity queryCategory = service.Retrieve(decisionCategory.LogicalName, decisionCategory.Id, new ColumnSet("jarvis_autoclose"));
                                                    if (queryCategory.Attributes.Contains("jarvis_autoclose") && queryCategory.Attributes["jarvis_autoclose"] != null)
                                                    {
                                                        bool autoClose = (bool)queryCategory.Attributes["jarvis_autoclose"];
                                                        if (autoClose)
                                                        {
                                                            caseUpdate = true;
                                                            parentCase["statuscode"] = new OptionSetValue(150);
                                                            if (caseUpdateTime != null)
                                                            {
                                                                parentCase["jarvis_canvasdate"] = (DateTime)caseUpdateTime;
                                                            }

                                                            parentCase["jarvis_casestatusupdate"] = DateTime.UtcNow;
                                                        }
                                                        else
                                                        {
                                                            caseUpdate = true;
                                                            parentCase["statuscode"] = new OptionSetValue(140);
                                                            if (caseUpdateTime != null)
                                                            {
                                                                parentCase["jarvis_canvasdate"] = (DateTime)caseUpdateTime;
                                                            }

                                                            parentCase["jarvis_casestatusupdate"] = DateTime.UtcNow;
                                                        }
                                                    }
                                                }
                                            }
                                        }

                                        if (currentBpfStageId.Name == Constants.Incident.bpfStage13)
                                        {
                                            if (statusCode.Value != 150)
                                            {
                                                caseUpdate = true;
                                                parentCase["statuscode"] = new OptionSetValue(150);
                                                if (caseUpdateTime != null)
                                                {
                                                    parentCase["jarvis_canvasdate"] = (DateTime)caseUpdateTime;
                                                }

                                                parentCase["jarvis_casestatusupdate"] = DateTime.UtcNow;
                                            }
                                        }
                                    }
#pragma warning restore SA1123 // Do not place regions within elements
#pragma warning restore SA1123 // Do not place regions within elements
#pragma warning restore SA1123 // Do not place regions within elements

                                    #endregion

                                    #endregion

                                    #region Case Opening Validate Integration Progress
                                    if (caseOpeningValidation && currentBpfStageId.Name == Constants.Incident.bpfStage2 && !isTrigerredFromMercurius)
                                    {
                                        EntityCollection unfinishedVehicleIntegration = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.GetCaseVehicleIntegrationStatus, incidentRetrieve.Id)));
                                        if (unfinishedVehicleIntegration.Entities.Count > 0)
                                        {
                                            throw new InvalidPluginExecutionException("The vehicle coverage validation is in progress. Case opening can only be validated once the vehicle coverage is available.");
                                        }
                                    }
                                    #endregion
                                    }

                                if (caseUpdate)
                                {
                                    tracingService.Trace("EUpdate");
                                    service.Update(parentCase);

#pragma warning disable SA1123 // Do not place regions within elements
                                    #region Close Case
                                    if (closeCase)
                                    {
                                        // Create the incident's resolution.
                                        Entity incidentResolution = new Entity("incidentresolution");
                                        incidentResolution.Attributes.Add("incidentid", new EntityReference("incident", parentCase.Id));

                                        // Close the incident with the resolution.
                                        var closeIncidentRequest = new CloseIncidentRequest
                                        {
                                            IncidentResolution = incidentResolution,
                                            Status = new OptionSetValue(5),
                                        };
                                        service.Execute(closeIncidentRequest);
                                    }
#pragma warning restore SA1123 // Do not place regions within elements
                                    #endregion
                                }
                            }
                        }
                    }
#pragma warning restore SA1123 // Do not place regions within elements
                    #endregion

#pragma warning disable SA1123 // Do not place regions within elements
                    #region Create

                    if (context.Stage == 40 && context.MessageName.ToUpper() == "CREATE")
                    {
                        EntityReference incident = targetEntity.Attributes.Contains("bpf_incidentid") ? targetEntity.GetAttributeValue<EntityReference>("bpf_incidentid") : null;
                        Entity incidentRetrieve = service.Retrieve(incident.LogicalName, incident.Id, new ColumnSet("ownerid", "casetypecode", "statuscode", "customerid", "jarvis_country", "description", "jarvis_homedealer", "jarvis_assistancetype", "jarvis_loadcargo", "createdby"));

#pragma warning disable SA1123 // Do not place regions within elements
                        #region Auto Movement

                        if (incidentRetrieve.Attributes.Contains("casetypecode") && incidentRetrieve.Attributes["casetypecode"] != null)
                        {
                            OptionSetValue caseType = (OptionSetValue)incidentRetrieve.Attributes["casetypecode"];

                            // Breakdown
                            if (caseType.Value != 3)
                            {
                                if (incidentRetrieve.Attributes.Contains("statuscode") && incidentRetrieve.Attributes["statuscode"] != null)
                                {
                                    OptionSetValue caseStatus = (OptionSetValue)incidentRetrieve.Attributes["statuscode"];

                                    // Case Opening
                                    if (caseStatus.Value == 10)
                                    {
                                        if (incidentRetrieve.Attributes.Contains("customerid") && incidentRetrieve.Attributes["customerid"] != null)
                                        {
                                            if (incidentRetrieve.Attributes.Contains("jarvis_country") && incidentRetrieve.Attributes["jarvis_country"] != null)
                                            {
                                                if (incidentRetrieve.Attributes.Contains("description") && incidentRetrieve.Attributes["description"] != null)
                                                {
                                                    if (incidentRetrieve.Attributes.Contains("jarvis_homedealer") && incidentRetrieve.Attributes["jarvis_homedealer"] != null)
                                                    {
                                                        if (incidentRetrieve.Attributes.Contains("ownerid") && incidentRetrieve.Attributes["ownerid"] != null)
                                                        {
                                                            EntityReference createdBy = (EntityReference)incidentRetrieve.Attributes["ownerid"];
                                                            if (createdBy.Name.ToUpperInvariant().Contains("MERCURIUS"))
                                                            {
                                                                this.ExecuteBPF(incident, Constants.Incident.bpfStage2, service, tracingService);
                                                            }
                                                        }
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
                    }
#pragma warning restore SA1123 // Do not place regions within elements

                    #endregion
                }
                catch (InvalidPluginExecutionException ex)
                {
                    throw new InvalidPluginExecutionException(ex.Message + " ");
                }
            }
        }

        /// <summary>
        /// Execute BPF.
        /// </summary>
        /// <param name="incidentRetrieve">incident Retrieve.</param>
        /// <param name="stageName">stage Name.</param>
        /// <param name="service">Org service.</param>
        /// <param name="tracingService">tracing Service.</param>
        internal void ExecuteBPF(EntityReference incidentRetrieve, string stageName, IOrganizationService service, ITracingService tracingService)
        {
#pragma warning disable SA1123 // Do not place regions within elements
            #region Stage Updates

            string activeStageName = stageName;
#pragma warning restore SA1123 // Do not place regions within elements

            // Get Process Instances
            RetrieveProcessInstancesRequest processInstanceRequest = new RetrieveProcessInstancesRequest
            {
                EntityId = incidentRetrieve.Id,

                EntityLogicalName = incidentRetrieve.LogicalName,
            };

            RetrieveProcessInstancesResponse processInstanceResponse = (RetrieveProcessInstancesResponse)service.Execute(processInstanceRequest);
            var activeProcessInstance = processInstanceResponse.Processes.Entities[0];
            Guid activeProcessInstanceID = activeProcessInstance.Id;

            // Retrieve the process stages in the active path of the current process instance
            RetrieveActivePathRequest pathReq = new RetrieveActivePathRequest
            {
                ProcessInstanceId = activeProcessInstanceID,
            };
            EntityCollection getcaseBPFProcess = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getCaseBPFInstance, incidentRetrieve.Id)));
            if (getcaseBPFProcess.Entities.Count > 0)
            {
                Guid caseBPFID = getcaseBPFProcess.Entities[0].Id;
                if (getcaseBPFProcess.Entities[0].Attributes.Contains("statecode"))
                {
                    if (((OptionSetValue)getcaseBPFProcess.Entities[0].Attributes["statecode"]).Value != 0)
                    {
                        return;
                    }
                }

                RetrieveActivePathResponse pathResp = (RetrieveActivePathResponse)service.Execute(pathReq);
                tracingService.Trace("Stages Count:" + pathResp.ProcessStages.Entities.Count + " ");
                for (int i = 0; i < pathResp.ProcessStages.Entities.Count; i++)
                {
                    // Retrieve the active stage name and active stage position based on the activeStageId for the process instance
                    if (pathResp.ProcessStages.Entities[i].Attributes["stagename"].ToString() == activeStageName)
                    {
                        tracingService.Trace("StageName:" + activeStageName + "  " + ((Guid)pathResp.ProcessStages.Entities[i].Attributes["processstageid"]).ToString() + " ");
                        Guid processStageID = (Guid)pathResp.ProcessStages.Entities[i].Attributes["processstageid"];
                        int activeStagePosition = i;

                        // service.Retrieve(activeProcessInstance.LogicalName, activeProcessInstanceID , new ColumnSet(true));
                        Entity retrievedProcessInstance = new Entity("jarvis_vasbreakdownprocess");
                        tracingService.Trace("process name:" + activeProcessInstance.LogicalName + " ");
                        tracingService.Trace("process id:" + activeProcessInstanceID.ToString());
                        retrievedProcessInstance.Id = caseBPFID;
                        retrievedProcessInstance.Attributes["activestageid"] = new EntityReference("processstage", processStageID); // processStageID
                        service.Update(retrievedProcessInstance);
                        tracingService.Trace("Completed:");
                    }
                }
            }

            #endregion
        }

        /// <summary>
        /// Stage Check.
        /// </summary>
        /// <param name="currentStage">current Stage.</param>
        /// <param name="preStage">pre Stage.</param>
        /// <returns>returns Exception.</returns>
        internal bool StageCheck(string currentStage, string preStage)
        {
            bool canMove = true;
            int currentStageOrder;
            int preStageOrder;
            int result1;
            int result2;
            if (currentStage.ToUpper().Trim() != preStage.ToUpper().Trim())
            {
                Dictionary<string, int> stageList = new Dictionary<string, int>();
                stageList.Add(Constants.Incident.bpfStage1, 1);
                stageList.Add(Constants.Incident.bpfStage2, 2);
                stageList.Add(Constants.Incident.bpfStage3, 3);
                stageList.Add(Constants.Incident.bpfStage4, 4);
                stageList.Add(Constants.Incident.bpfStage5, 5);
                stageList.Add(Constants.Incident.bpfStage6, 6);
                stageList.Add(Constants.Incident.bpfStage7, 7);
                stageList.Add(Constants.Incident.bpfStage8, 8);
                stageList.Add(Constants.Incident.bpfStage9, 9);
                stageList.Add(Constants.Incident.bpfStage10, 10);
                stageList.Add(Constants.Incident.bpfStage11, 11);
                stageList.Add(Constants.Incident.bpfStage12, 12);
                stageList.Add(Constants.Incident.bpfStage13, 13);

                if (stageList.TryGetValue(currentStage, out result1))
                {
                    currentStageOrder = result1;
                    if (stageList.TryGetValue(preStage, out result2))
                    {
                        preStageOrder = result2;
                        if (currentStageOrder < preStageOrder)
                        {
                            canMove = false;
                        }
                    }
                }
            }

            return canMove;
        }
    }
}
