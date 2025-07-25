//-----------------------------------------------------------------------
// <copyright file="CasePostOperationSync.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace MCS.Jarvis.CE.Plugins
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.ServiceModel;
    using System.Text.RegularExpressions;
    using MCS.Jarvis.CE.BusinessProcessesShared;
    using MCS.Jarvis.CE.BusinessProcessesShared.Helpers;
    using MCS.Jarvis.CE.BusinessProcessShared.AppNotification;
    using MCS.Jarvis.CE.BusinessProcessShared.Case;
    using MCS.Jarvis.CE.BusinessProcessShared.CaseEscalation;
    using MCS.Jarvis.CE.BusinessProcessShared.CaseMonitor;
    using MCS.jarvis.CE.BusinessProcessShared.TranslationProcess;
    using MCS.jarvis.CE.BusinessProcessShared.VASBreakdownProcess;
    using Microsoft.Crm.Sdk.Messages;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;
    using static MCS.Jarvis.CE.BusinessProcessesShared.Helpers.Constants;

    /// <summary>
    /// Case Post Operation Sync.
    /// </summary>
    public class CasePostOperationSync : IPlugin
    {
        /// <summary>
        /// Case Post Operation Sync.
        /// </summary>
        /// <param name="serviceProvider">service Provider.</param>
        /// <exception cref="FaultException">Fault Exception.</exception>
        /// <exception cref="InvalidPluginExecutionException">Invalid Plugin Execution Exception.</exception>
        /// <exception cref="Exception">Exception details.</exception>
        public void Execute(IServiceProvider serviceProvider)
        {
            // Extract the tracing service for use in debugging sandboxed plug-ins.
            ITracingService tracingService =
                (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            // Obtain the execution context from the service provider.
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            if (context.Depth > 6)
            {
                return;
            }

            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                Entity incident = (Entity)context.InputParameters["Target"];

                try
                {
                    #region Post Create

                    if (context.Stage == 40 && context.MessageName.ToUpper() == "CREATE")
                    {
                        IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                        IOrganizationService uservice = serviceFactory.CreateOrganizationService(context.InitiatingUserId);
                        IOrganizationService service = serviceFactory.CreateOrganizationService(null);

                        ////bool isUpdate = false;
                        Entity incidentRetrieve = (Entity)context.PostEntityImages["PostImage"];

                        if (incidentRetrieve.Attributes.Contains("caseorigincode") && incidentRetrieve.Attributes["caseorigincode"] != null)
                        {
                            OptionSetValue caseOrigin = (OptionSetValue)incidentRetrieve.Attributes["caseorigincode"];
                            tracingService.Trace("Case Origin :" + caseOrigin.Value + " ");
                            if (incidentRetrieve.Attributes.Contains("jarvis_mercuriusstatus") && incidentRetrieve.Attributes["jarvis_mercuriusstatus"] != null)
                            {
                                OptionSetValue mercStatus = (OptionSetValue)incidentRetrieve.Attributes["jarvis_mercuriusstatus"];
                                tracingService.Trace("mercStatus Origin :" + mercStatus.Value + " ");
                                if (caseOrigin.Value != 1)
                                {
                                    // Phone (Manual Case Creation)
                                    #region Create and Set BPF
                                    SetProcessRequest request = new SetProcessRequest();
                                    request.Target = incidentRetrieve.ToEntityReference();
                                    EntityCollection retriveConfiguration = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getProcessName, Constants.Incident.BpfStageName)));
                                    if (retriveConfiguration.Entities.Count > 0 && retriveConfiguration.Entities.Count == 1)
                                    {
                                        if (retriveConfiguration.Entities[0].Attributes.Contains("jarvis_integrationcode") && retriveConfiguration.Entities[0].Attributes["jarvis_integrationcode"] != null)
                                        {
                                            string bpfName = (string)retriveConfiguration.Entities[0].Attributes["jarvis_integrationcode"];
                                            tracingService.Trace("bpfName Origin :" + bpfName + " ");
                                            EntityCollection retrieveProcess = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getBreakDownProcess, bpfName)));
                                            if (retrieveProcess.Entities.Count > 0 && retrieveProcess.Entities.Count == 1)
                                            {
                                                request.NewProcess = new EntityReference(retrieveProcess.Entities[0].LogicalName, retrieveProcess.Entities[0].Id);
                                                service.Execute(request);
                                                tracingService.Trace("Completed:");

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
                                                string activeStageName = string.Empty;
                                                if (mercStatus.Value == 0 || mercStatus.Value == 100)
                                                {
                                                    activeStageName = Constants.Incident.BpfStage1;
                                                }

                                                if (mercStatus.Value == 200)
                                                {
                                                    activeStageName = Constants.Incident.BpfStage2;
                                                }

                                                if (mercStatus.Value == 300)
                                                {
                                                    activeStageName = Constants.Incident.BpfStage3;
                                                }

                                                if (mercStatus.Value == 400)
                                                {
                                                    activeStageName = Constants.Incident.BpfStage4;
                                                }

                                                if (mercStatus.Value == 500)
                                                {
                                                    activeStageName = Constants.Incident.BpfStage5;
                                                }

                                                if (mercStatus.Value == 600)
                                                {
                                                    activeStageName = Constants.Incident.BpfStage6;
                                                }

                                                if (mercStatus.Value == 700)
                                                {
                                                    activeStageName = Constants.Incident.BpfStage7;
                                                }

                                                if (mercStatus.Value == 800)
                                                {
                                                    activeStageName = Constants.Incident.BpfStage8;
                                                }

                                                if (mercStatus.Value == 900)
                                                {
                                                    activeStageName = Constants.Incident.BpfStage9;
                                                }

                                                EntityCollection getcaseBPFProcess = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getCaseBPFInstance, incidentRetrieve.Id)));
                                                if (getcaseBPFProcess.Entities.Count > 0)
                                                {
                                                    Guid caseBPFID = getcaseBPFProcess.Entities[0].Id;
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
                                            }
                                        }
                                    }
                                    #endregion
                                }
                            }
                        }

                        // Create Case Monitor
#pragma warning disable SA1123 // Do not place regions within elements
                        #region Create Case Monitor - Case Opening

                        if (incidentRetrieve.Attributes.Contains("jarvis_sourceid") && incidentRetrieve.Attributes["jarvis_sourceid"] != null)
                        {
                            // jarvis_sourceid
                            CaseMonitorProcess operations = new CaseMonitorProcess();
                            if (incidentRetrieve.Attributes.Contains("casetypecode") && incidentRetrieve.Attributes["casetypecode"] != null)
                            {
                                OptionSetValue statusreason = (OptionSetValue)incidentRetrieve.Attributes["statuscode"];
                                OptionSetValue caseTypeCode = (OptionSetValue)incidentRetrieve.Attributes["casetypecode"];
                                if (caseTypeCode.Value == 2 && statusreason.Value == 10)
                                {
                                    // Breakdown - Case Opening
                                    if (incidentRetrieve.Attributes.Contains("jarvis_callerrole") && incidentRetrieve.Attributes["jarvis_callerrole"] != null)
                                    {
                                        // jarvis_callerrole
                                        OptionSetValue callerRole = (OptionSetValue)incidentRetrieve.Attributes["jarvis_callerrole"];
                                        if (callerRole.Value == 4 || callerRole.Value == 3)
                                        {
                                            // HD/RD
                                            if (callerRole.Value == 4)
                                            {
                                                // RD
                                                string isCountryCode = string.Empty;
                                                string isoLangCode = string.Empty;
                                                if (incidentRetrieve.Attributes.Contains("jarvis_callerlanguage") && incidentRetrieve.Attributes["jarvis_callerlanguage"] != null)
                                                {
                                                    // jarvis_callerlanguage
                                                    EntityReference language = (EntityReference)incidentRetrieve.Attributes["jarvis_callerlanguage"];
                                                    Entity rdlanguage = service.Retrieve(language.LogicalName, language.Id, new ColumnSet("jarvis_iso3languagecode6392t"));
                                                    if (rdlanguage.Attributes.Contains("jarvis_iso3languagecode6392t") && rdlanguage.Attributes["jarvis_iso3languagecode6392t"] != null)
                                                    {
                                                        isoLangCode = (string)rdlanguage.Attributes["jarvis_iso3languagecode6392t"];
                                                    }
                                                }

                                                if (incidentRetrieve.Attributes.Contains("jarvis_homedealer") && incidentRetrieve.Attributes["jarvis_homedealer"] != null)
                                                {
                                                    EntityReference homeDealer = (EntityReference)incidentRetrieve.Attributes["jarvis_homedealer"];
                                                    Entity account = service.Retrieve(homeDealer.LogicalName, homeDealer.Id, new ColumnSet("jarvis_address1_country"));
                                                    if (account.Attributes.Contains("jarvis_address1_country") && account.Attributes["jarvis_address1_country"] != null)
                                                    {
                                                        EntityReference hdcountry = (EntityReference)account.Attributes["jarvis_address1_country"];
                                                        Entity country = service.Retrieve(hdcountry.LogicalName, hdcountry.Id, new ColumnSet("jarvis_iso2countrycode"));
                                                        if (country.Attributes.Contains("jarvis_iso2countrycode") && country.Attributes["jarvis_iso2countrycode"] != null)
                                                        {
                                                            isCountryCode = (string)country.Attributes["jarvis_iso2countrycode"];
                                                        }
                                                    }
                                                }

                                                if (!string.IsNullOrEmpty(isoLangCode) || !string.IsNullOrEmpty(isCountryCode))
                                                {
                                                    string fucomment = isoLangCode + " " + isCountryCode + " " + "New Case RD";
                                                    EntityReference createdBy = (EntityReference)incidentRetrieve.Attributes["createdby"];
                                                    if (createdBy.Name.ToUpperInvariant().Contains("MERCURIUS"))
                                                    {
                                                        operations.AutomateMonitorCreation(incidentRetrieve, fucomment, 1, 2, 0, " ", service);
                                                    }
                                                }
                                            }

                                            if (callerRole.Value == 3)
                                            {
                                                // HD
                                                if (incidentRetrieve.Attributes.Contains("jarvis_homedealer") && incidentRetrieve.Attributes["jarvis_homedealer"] != null)
                                                {
                                                    // jarvis_callerlanguage
                                                    EntityReference homeDealer = (EntityReference)incidentRetrieve.Attributes["jarvis_homedealer"];
                                                    Entity account = service.Retrieve(homeDealer.LogicalName, homeDealer.Id, new ColumnSet("jarvis_language", "jarvis_address1_country"));
                                                    string isoLangCode = string.Empty;
                                                    string isCountryCode = string.Empty;
                                                    if (account.Attributes.Contains("jarvis_address1_country") && account.Attributes["jarvis_address1_country"] != null)
                                                    {
                                                        EntityReference hdcountry = (EntityReference)account.Attributes["jarvis_address1_country"];
                                                        Entity country = service.Retrieve(hdcountry.LogicalName, hdcountry.Id, new ColumnSet("jarvis_iso2countrycode"));
                                                        if (country.Attributes.Contains("jarvis_iso2countrycode") && country.Attributes["jarvis_iso2countrycode"] != null)
                                                        {
                                                            isCountryCode = (string)country.Attributes["jarvis_iso2countrycode"];
                                                        }
                                                    }

                                                    if (account.Attributes.Contains("jarvis_language") && account.Attributes["jarvis_language"] != null)
                                                    {
                                                        EntityReference language = (EntityReference)account.Attributes["jarvis_language"];
                                                        Entity hDlanguage = service.Retrieve(language.LogicalName, language.Id, new ColumnSet("jarvis_iso3languagecode6392t"));
                                                        if (hDlanguage.Attributes.Contains("jarvis_iso3languagecode6392t") && hDlanguage.Attributes["jarvis_iso3languagecode6392t"] != null)
                                                        {
                                                            isoLangCode = (string)hDlanguage.Attributes["jarvis_iso3languagecode6392t"];
                                                        }
                                                    }

                                                    if (!string.IsNullOrEmpty(isoLangCode) || !string.IsNullOrEmpty(isCountryCode))
                                                    {
                                                        string fucomment = isoLangCode + " " + isCountryCode + " " + "New Case HD";
                                                        EntityReference createdBy = (EntityReference)incidentRetrieve.Attributes["createdby"];
                                                        if (createdBy.Name.ToUpperInvariant().Contains("MERCURIUS"))
                                                        {
                                                            operations.AutomateMonitorCreation(incidentRetrieve, fucomment, 1, 2, 0, " ", service);
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

#pragma warning disable SA1123 // Do not place regions within elements
                        #region Update Vehicle

                        if (incidentRetrieve.Attributes.Contains("jarvis_vehicle") && incidentRetrieve.Attributes["jarvis_vehicle"] != null)
                        {
                            EntityReference vehicle = (EntityReference)incidentRetrieve.Attributes["jarvis_vehicle"];
                            Entity vehicleRetrieve = service.Retrieve(vehicle.LogicalName, vehicle.Id, new ColumnSet("jarvis_isdummyvehicle"));
                            Entity vehicleToUpdate = new Entity(vehicle.LogicalName);
                            vehicleToUpdate.Id = vehicle.Id;
                            bool isVehUpdate = false;
                            if (incident.Attributes.Contains("jarvis_registrationnumber") && incident.Attributes["jarvis_registrationnumber"] != null)
                            {
                                string registrationNumber = (string)incident.Attributes["jarvis_registrationnumber"];
                                vehicleToUpdate["jarvis_updatedregistrationnumber"] = registrationNumber;
                                vehicleToUpdate["jarvis_name"] = registrationNumber;
                                isVehUpdate = true;
                            }

                            if (incident.Attributes.Contains("jarvis_homedealer") && incident.Attributes["jarvis_homedealer"] != null)
                            {
                                tracingService.Trace("Contains home dealer");
                                var modifiedBy = (EntityReference)incidentRetrieve.Attributes["createdby"];
                                tracingService.Trace("Retrieve");
                                if (modifiedBy != null)
                                {
                                    bool userCheck = CrmHelper.CheckUserIsMercurius(modifiedBy.Id, tracingService, service);
                                    if (!userCheck)
                                    {
                                        EntityReference homeDealer = (EntityReference)incidentRetrieve.Attributes["jarvis_homedealer"];
                                        vehicleToUpdate["jarvis_updatedhomedealer"] = homeDealer;
                                        isVehUpdate = true;
                                    }
                                }
                            }

                            if (incident.Attributes.Contains("customerid") && incident.Attributes["customerid"] != null)
                            {
                                EntityReference customer = (EntityReference)incident.Attributes["customerid"];
                                vehicleToUpdate["jarvis_updatedowningcustomer"] = customer;
                                vehicleToUpdate["jarvis_updatedusingcustomer"] = customer;
                                isVehUpdate = true;
                            }

                            if (isVehUpdate && !(vehicleRetrieve.Attributes.Contains("jarvis_isdummyvehicle") && (bool)vehicleRetrieve.Attributes["jarvis_isdummyvehicle"] == true))
                            {
                                service.Update(vehicleToUpdate);
                            }

#pragma warning disable SA1123 // Do not place regions within elements
                            #region Case Soft Offer

                            DateTime currentDateTime = DateTime.UtcNow;
#pragma warning restore SA1123 // Do not place regions within elements
                            string currentDateString = currentDateTime.ToString("yyyy-MM-dd");
                            EntityCollection getVehicleOffers = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getSoftOfferCodes, vehicle.Id, currentDateString, currentDateString)));
                            if (getVehicleOffers != null && getVehicleOffers.Entities.Count > 0)
                            {
                                CaseOperations caseOperationsSet = new CaseOperations();
                                caseOperationsSet.CreateCaseSoftOffer(service, incidentRetrieve.ToEntityReference(), getVehicleOffers.Entities[0]);
                            }
                            #endregion

#pragma warning disable SA1123 // Do not place regions within elements
                            #region Progress indicator
                            tracingService.Trace("Progress Indicator");
#pragma warning restore SA1123 // Do not place regions within elements
                            CaseOperations caseOperations = new CaseOperations();
                            caseOperations.CreateCaseProgressIndicator(service, tracingService, incidentRetrieve.ToEntityReference(), getVehicleOffers.Entities.Count > 0 ? 1 : 0);
                            #endregion
                        }
#pragma warning restore SA1123 // Do not place regions within elements

#pragma warning disable SA1123 // Do not place regions within elements
                        #region AddIncidentNatureSubgriddata
                        if (incident.Attributes.Contains("jarvis_incidentnature") && incident.Attributes["jarvis_incidentnature"] != null)
                        {
                            this.UpdateIncidentNatureSubgrid(incident, context.MessageName.ToUpper(), service, tracingService);
                        }
#pragma warning restore SA1123 // Do not place regions within elements
                        #endregion
                        #endregion

#pragma warning disable SA1123 // Do not place regions within elements
                        #region Average ETA
                        if (incident.Attributes.Contains(Incident.JarvisCountry) && incident.Attributes[Incident.JarvisCountry] != null && incident.Attributes.Contains(Incident.CreatedOn) && incident.Attributes[Incident.CreatedOn] != null
                        && incident.Attributes.Contains(Incident.casetypecode) && incident.Attributes[Incident.casetypecode] != null && ((OptionSetValue)incident.Attributes[Incident.casetypecode]).Value == (int)CaseTypeCode.Breakdown && incident.Attributes.Contains(Incident.JarvisAssistancetype)
                            && incident.Attributes[Incident.JarvisAssistancetype] != null && ((OptionSetValue)incident.Attributes[Incident.JarvisAssistancetype]).Value == (int)AssistanceType.Breakdown_immediate)
                        {
                            EntityReference countryReference = (EntityReference)incident.Attributes[Incident.JarvisCountry];
                            tracingService.Trace("Incident country - " + countryReference.Id.ToString());
                            Entity country = service.Retrieve(countryReference.LogicalName, countryReference.Id, new ColumnSet(JarvisCountry.JarvisAverageetaduration));
                            ////Update Case
                            Entity caseToUpdate = new Entity(incident.LogicalName);
                            caseToUpdate.Id = incident.Id;
                            if (country != null && country.Attributes.Contains(JarvisCountry.JarvisAverageetaduration) && country.Attributes[JarvisCountry.JarvisAverageetaduration] != null)
                            {
                                tracingService.Trace("Country Average ETA - " + country.Attributes[JarvisCountry.JarvisAverageetaduration].ToString());
                                tracingService.Trace("Case created on - " + DateTime.Parse(incident.Attributes[Incident.CreatedOn].ToString()));
                                caseToUpdate[Incident.JarvisAverageEta] = DateTime.Parse(incident.Attributes[Incident.CreatedOn].ToString()).AddHours(double.Parse(country.Attributes[JarvisCountry.JarvisAverageetaduration].ToString()));
                                tracingService.Trace("Case Average ETA - " + caseToUpdate.Attributes[Incident.JarvisAverageEta].ToString());
                                service.Update(caseToUpdate);
                            }
                        }
#pragma warning restore SA1123 // Do not place regions within elements
                        #endregion

#pragma warning disable SA1123 // Do not place regions within elements
                        #region SetRD/HD
                        if (incident.Attributes.Contains("jarvis_homedealer") && incidentRetrieve.Attributes.Contains("casetypecode") && incidentRetrieve.Attributes["casetypecode"] != null)
                        {
                            OptionSetValue caseTypeCode = (OptionSetValue)incidentRetrieve.Attributes["casetypecode"];
                            if (caseTypeCode.Value == 2)
                            {
                                // Breakdown
                                this.SetRDHD(service, incident);
                            }
                        }
#pragma warning restore SA1123 // Do not place regions within elements
                        #endregion

#pragma warning disable SA1123 // Do not place regions within elements
                        #region Customer CaseContact Create
                        if (incident.Attributes.Contains("customerid") && incident.Attributes["customerid"] != null)
                        {
                            CaseContactIncident contactCustomer = new CaseContactIncident();
                            contactCustomer.CreateCustomerCaseContact(incident, service, context.MessageName.ToUpper(), tracingService);
                            contactCustomer.CreateCustomerContacts(incident, service, context.MessageName.ToUpper(), tracingService);
                        }
#pragma warning restore SA1123 // Do not place regions within elements

                        #endregion

                        // Check if target contains Driver or Caller data
                        if (incident.Attributes.Contains("jarvis_callernameargus") || incident.Attributes.Contains("jarvis_callerphone") ||
                            incident.Attributes.Contains("jarvis_callerlanguage") || incident.Attributes.Contains("jarvis_callerrole") ||
                            incident.Attributes.Contains("customerid") || incident.Attributes.Contains("jarvis_drivername") ||
                            incident.Attributes.Contains("jarvis_driverphone") || incident.Attributes.Contains("jarvis_driverlanguage") ||
                            incident.Attributes.Contains("jarvis_driverlanguagesupported") || incident.Attributes.Contains("jarvis_callerlanguagesupported") ||
                            incident.Attributes.Contains("jarvis_calleremail") || incident.Attributes.Contains("jarvis_callerphonenumbertype"))
                        {
                            CaseContactIncident contact = new CaseContactIncident();
                            if (incidentRetrieve.Attributes.Contains("casetypecode") && incidentRetrieve.Attributes["casetypecode"] != null)
                            {
                                OptionSetValue caseTypeCode = (OptionSetValue)incidentRetrieve.Attributes["casetypecode"];
                                if (caseTypeCode.Value == 2)
                                {
                                    // Breakdown
                                    contact.CaseContactCreate(incident, service, context.MessageName.ToUpper(), incidentRetrieve, tracingService);
                                }
                            }
                        }

#pragma warning disable SA1123 // Do not place regions within elements
                        #region Case Translations
                        bool isAutomation = false;
#pragma warning restore SA1123 // Do not place regions within elements
                        isAutomation = CrmHelper.GetAutomationConfig(service, MCS.Jarvis.CE.Plugins.Constants.JarvisConfiguration.Automationtranslation, tracingService);
                        OptionSetValue caseTypeCodes = (OptionSetValue)incidentRetrieve.Attributes["casetypecode"];
                        if (isAutomation)
                        {
                            if (caseTypeCodes.Value == 2)
                            {
                                // Breakdown
                                if (incident.Attributes.Contains("description") || incident.Attributes.Contains("jarvis_location") || incident.Attributes.Contains("jarvis_customerexpectations"))
                                {
                                    Entity caseToUpdate = new Entity(incident.LogicalName);
                                    caseToUpdate.Id = incident.Id;
                                    TranslationProcess operations = new TranslationProcess();
                                    operations.CaseStandardProcess(service, tracingService, incident, context.InitiatingUserId);
                                    if (incident.Attributes.Contains("description"))
                                    {
                                        caseToUpdate["jarvis_translationstatusreportedfault"] = new OptionSetValue(334030001);
                                    }

                                    if (incident.Attributes.Contains("jarvis_location"))
                                    {
                                        caseToUpdate["jarvis_translationstatuslocation"] = new OptionSetValue(334030001);
                                    }

                                    if (incident.Attributes.Contains("jarvis_customerexpectations"))
                                    {
                                        caseToUpdate["jarvis_translationstatuscustomerexpectations"] = new OptionSetValue(334030001);
                                    }

                                    uservice.Update(caseToUpdate);
                                    context.SharedVariables.Add("Translate", true);
                                }
                            }
                            else
                            {
                                // Query
                                if (incident.Attributes.Contains("jarvis_querydescription") || incident.Attributes.Contains("jarvis_extrainformationonsolutionquery") || incident.Attributes.Contains("jarvis_querydecision"))
                                {
                                    Entity caseToUpdate = new Entity(incident.LogicalName);
                                    caseToUpdate.Id = incident.Id;
                                    //// throw new InvalidPluginExecutionException("Query");
                                    if (incidentRetrieve.Attributes.Contains("parentcaseid") && incidentRetrieve.Attributes["parentcaseid"] != null)
                                    {
                                        EntityReference parentCaseRef = (EntityReference)incidentRetrieve.Attributes["parentcaseid"];
                                        Entity parentCase = new Entity(parentCaseRef.LogicalName);
                                        parentCase.Id = parentCaseRef.Id;
                                        //// throw new InvalidPluginExecutionException("parentCase" + parentCase.Id.ToString());
                                        TranslationProcess operations = new TranslationProcess();
                                        operations.CaseQueryStandardProcess(service, tracingService, parentCase, incidentRetrieve, context.InitiatingUserId);
                                        if (incident.Attributes.Contains("jarvis_querydescription"))
                                        {
                                            caseToUpdate["jarvis_translationstatusalertdescription"] = new OptionSetValue(334030001);
                                        }

                                        if (incident.Attributes.Contains("jarvis_extrainformationonsolutionquery"))
                                        {
                                            caseToUpdate["jarvis_translationstatusalertextrainformation"] = new OptionSetValue(334030001);
                                        }

                                        if (incident.Attributes.Contains("jarvis_querydecision"))
                                        {
                                            caseToUpdate["jarvis_translationstatusalertdecision"] = new OptionSetValue(334030001);
                                        }

                                        uservice.Update(caseToUpdate);
                                        context.SharedVariables.Add("Translate", true);
                                    }
                                }
                            }
                        }

                        #endregion
                    }
#pragma warning restore SA1123 // Do not place regions within elements

                    #endregion

#pragma warning disable SA1123 // Do not place regions within elements
                    #region Post Update

                    if (context.Stage == 40 && context.MessageName.ToUpper() == "UPDATE")
                    {
                        IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                        IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
                        Guid initiatingUserID = context.UserId;
                        IOrganizationService adminservice = serviceFactory.CreateOrganizationService(null);

                        tracingService.Trace("Entered Update of Case");
                        Entity incidentRetrieve = (Entity)context.PostEntityImages["PostImage"];
                        Entity incidentPreImage = (Entity)context.PreEntityImages["PreImage"];
                        OptionSetValue statusPre = new OptionSetValue();
                        OptionSetValue caseTypeCode = new OptionSetValue();
                        bool customerContactHasMobile = false;
                        tracingService.Trace("Got Images");
                        if (incidentPreImage.Attributes.Contains("statuscode") && incidentPreImage.Attributes["statuscode"] != null)
                        {
                            statusPre = (OptionSetValue)incidentPreImage.Attributes["statuscode"];
                        }

                        if (incidentRetrieve.Attributes.Contains("casetypecode") && incidentRetrieve.Attributes["casetypecode"] != null)
                        {
                            caseTypeCode = (OptionSetValue)incidentRetrieve.Attributes["casetypecode"];
                        }

#pragma warning disable SA1123 // Do not place regions within elements
                        #region Case Translations
                        bool isAutomation = false;
#pragma warning restore SA1123 // Do not place regions within elements
                        isAutomation = CrmHelper.GetAutomationConfig(service, MCS.Jarvis.CE.Plugins.Constants.JarvisConfiguration.Automationtranslation, tracingService);
                        OptionSetValue caseTypeCodes = (OptionSetValue)incidentRetrieve.Attributes["casetypecode"];
                        if (isAutomation)
                        {
                            if (caseTypeCodes.Value == 2)
                            {
                                // Breakdown
                                if (incident.Attributes.Contains("description") || incident.Attributes.Contains("jarvis_location") || incident.Attributes.Contains("jarvis_customerexpectations"))
                                {
                                    TranslationProcess operations = new TranslationProcess();
                                    operations.CaseStandardProcess(adminservice, tracingService, incident, context.InitiatingUserId);
                                    IOrganizationService newservice = serviceFactory.CreateOrganizationService(context.UserId);
                                    Entity caseUpdate = newservice.Retrieve(incidentRetrieve.LogicalName, incidentRetrieve.Id, new ColumnSet("jarvis_translationstatusreportedfault", "jarvis_translationstatuslocation", "jarvis_translationstatuscustomerexpectations"));
                                    Entity caseToUpdate = new Entity(incident.LogicalName);
                                    caseToUpdate.Id = incident.Id;
                                    bool isUpdate = false;
                                    if (caseUpdate.Attributes.Contains("jarvis_translationstatusreportedfault") && caseUpdate.Attributes["jarvis_translationstatusreportedfault"] != null && incident.Attributes.Contains("description"))
                                    {
                                        OptionSetValue translationStatusRF = (OptionSetValue)incidentRetrieve.Attributes["jarvis_translationstatusreportedfault"];
                                        if (translationStatusRF.Value != 334030001)
                                        {
                                            // In-Progress
                                            caseToUpdate["jarvis_translationstatusreportedfault"] = new OptionSetValue(334030001);
                                            isUpdate = true;
                                        }
                                    }

                                    if (caseUpdate.Attributes.Contains("jarvis_translationstatuslocation") && caseUpdate.Attributes["jarvis_translationstatuslocation"] != null)
                                    {
                                        OptionSetValue translationStatusLoc = (OptionSetValue)incidentRetrieve.Attributes["jarvis_translationstatuslocation"];
                                        if (translationStatusLoc.Value != 334030001 && incidentRetrieve.Attributes.Contains("jarvis_location") && incident.Attributes.Contains("jarvis_location"))
                                        {
                                            // In-Progress
                                            caseToUpdate["jarvis_translationstatuslocation"] = new OptionSetValue(334030001);
                                            isUpdate = true;
                                        }
                                    }

                                    if (caseUpdate.Attributes.Contains("jarvis_translationstatuscustomerexpectations") && caseUpdate.Attributes["jarvis_translationstatuscustomerexpectations"] != null)
                                    {
                                        OptionSetValue translationStatusCE = (OptionSetValue)incidentRetrieve.Attributes["jarvis_translationstatuscustomerexpectations"];
                                        if (translationStatusCE.Value != 334030001 && incidentRetrieve.Attributes.Contains("jarvis_customerexpectations") && incident.Attributes.Contains("jarvis_customerexpectations"))
                                        {
                                            // In-Progress
                                            caseToUpdate["jarvis_translationstatuscustomerexpectations"] = new OptionSetValue(334030001);
                                            isUpdate = true;
                                        }
                                    }

                                    if (isUpdate)
                                    {
                                        service.Update(caseToUpdate);
                                    }
                                }
                            }
                            else
                            {
                                if (incident.Attributes.Contains("jarvis_querydescription") || incident.Attributes.Contains("jarvis_extrainformationonsolutionquery") || incident.Attributes.Contains("jarvis_querydecision"))
                                {
                                    if (incidentRetrieve.Attributes.Contains("parentcaseid") && incidentRetrieve.Attributes["parentcaseid"] != null)
                                    {
                                        EntityReference parentCaseRef = (EntityReference)incidentRetrieve.Attributes["parentcaseid"];
                                        Entity parentCase = new Entity(parentCaseRef.LogicalName);
                                        parentCase.Id = parentCaseRef.Id;

                                        TranslationProcess operations = new TranslationProcess();
                                        operations.CaseQueryStandardProcess(adminservice, tracingService, parentCase, incidentRetrieve, context.InitiatingUserId);
                                        IOrganizationService newservice = serviceFactory.CreateOrganizationService(context.UserId);
                                        Entity caseUpdate = newservice.Retrieve(incidentRetrieve.LogicalName, incidentRetrieve.Id, new ColumnSet("jarvis_translationstatusalertdecision", "jarvis_translationstatusalertdescription", "jarvis_translationstatusalertextrainformation"));
                                        Entity caseToUpdate = new Entity(incident.LogicalName);
                                        caseToUpdate.Id = incident.Id;
                                        bool isUpdate = false;
                                        if (caseUpdate.Attributes.Contains("jarvis_translationstatusalertdescription") && caseUpdate.Attributes["jarvis_translationstatusalertdescription"] != null)
                                        {
                                            OptionSetValue translationStatusRF = (OptionSetValue)incidentRetrieve.Attributes["jarvis_translationstatusalertdescription"];
                                            if (translationStatusRF.Value != 334030001 && incident.Attributes.Contains("jarvis_querydescription"))
                                            {
                                                // In-Progress
                                                caseToUpdate["jarvis_translationstatusalertdescription"] = new OptionSetValue(334030001);
                                                isUpdate = true;
                                            }
                                        }

                                        if (caseUpdate.Attributes.Contains("jarvis_translationstatusalertextrainformation") && caseUpdate.Attributes["jarvis_translationstatusalertextrainformation"] != null)
                                        {
                                            OptionSetValue translationStatusLoc = (OptionSetValue)incidentRetrieve.Attributes["jarvis_translationstatusalertextrainformation"];
                                            if (translationStatusLoc.Value != 334030001 && incidentRetrieve.Attributes.Contains("jarvis_extrainformationonsolutionquery") && incident.Attributes.Contains("jarvis_extrainformationonsolutionquery"))
                                            {
                                                // In-Progress
                                                caseToUpdate["jarvis_translationstatusalertextrainformation"] = new OptionSetValue(334030001);
                                                isUpdate = true;
                                            }
                                        }

                                        if (caseUpdate.Attributes.Contains("jarvis_translationstatusalertdecision") && caseUpdate.Attributes["jarvis_translationstatusalertdecision"] != null)
                                        {
                                            OptionSetValue translationStatusCE = (OptionSetValue)incidentRetrieve.Attributes["jarvis_translationstatusalertdecision"];
                                            if (translationStatusCE.Value != 334030001 && incidentRetrieve.Attributes.Contains("jarvis_querydecision") && incident.Attributes.Contains("jarvis_querydecision"))
                                            {
                                                // In-Progress
                                                caseToUpdate["jarvis_translationstatusalertdecision"] = new OptionSetValue(334030001);
                                                isUpdate = true;
                                            }
                                        }

                                        if (isUpdate)
                                        {
                                            service.Update(caseToUpdate);
                                        }
                                    }
                                }
                            }
                        }

                        #endregion

#pragma warning disable SA1123 // Do not place regions within elements
                        #region AddIncidentNatureSubgriddata
                        if (incident.Attributes.Contains("jarvis_incidentnature") && incident.Attributes["jarvis_incidentnature"] != null)
                        {
                            tracingService.Trace("Entered AddIncidentNatureSubgriddata of Case");
                            tracingService.Trace("Incident Nature Associate");
                            this.UpdateIncidentNatureSubgrid(incident, context.MessageName.ToUpper(), service, tracingService);
                        }
#pragma warning restore SA1123 // Do not place regions within elements
                        #endregion
                        var isincidentnaturetowingupdated = string.Empty;
                        if (incident.Attributes.Contains("jarvis_isincidentnaturetowing") && incident.Attributes["jarvis_isincidentnaturetowing"] != null )
                            {
                             isincidentnaturetowingupdated = incident.Attributes["jarvis_isincidentnaturetowing"].ToString();
                        }

                        // Update all existing Repair Info Records
                        if (incident.Attributes.Contains("jarvis_incidentnature") && incident.Attributes["jarvis_incidentnature"] != null && incident.Attributes["jarvis_incidentnatureshadow"].ToString().Contains("Towing") && isincidentnaturetowingupdated == "Yes")
                        {
                            EntityCollection getAllExistingRepairInfoRecordsCollection = service.RetrieveMultiple(new FetchExpression(string.Format(MCS.Jarvis.CE.Plugins.Constants.FetchXmls.getAllExistingRepairInfoRecords, incident.Id)));
                            if (getAllExistingRepairInfoRecordsCollection.Entities.Count > 0)
                            {
                                tracingService.Trace("Count of Repair Info Records are" + getAllExistingRepairInfoRecordsCollection.Entities.Count);
                                foreach (Entity repairInfoRecords in getAllExistingRepairInfoRecordsCollection.Entities)
                                {

                                    Entity repairInfoRecord = new Entity("jarvis_repairinformation");
                                    repairInfoRecord.Id = repairInfoRecords.Id;
                                    repairInfoRecord["jarvis_towing"] = true;
                                    service.Update(repairInfoRecord);
                                }

                            }
                            // Update all JEDs with Towing-True
                            EntityCollection getAllExistingjedsCollection = service.RetrieveMultiple(new FetchExpression(string.Format(MCS.Jarvis.CE.Plugins.Constants.FetchXmls.getAllExistingjedsRecords, incident.Id)));
                            if (getAllExistingjedsCollection.Entities.Count > 0)
                            {
                                foreach (Entity jedsRecords in getAllExistingjedsCollection.Entities)
                                {
                                    Entity jedsRecord = new Entity("jarvis_jobenddetails");
                                    jedsRecord.Id = jedsRecords.Id;
                                    jedsRecord["jarvis_towing"] = true;
                                    service.Update(jedsRecord);
                                    tracingService.Trace("JED Records are updated with Towing Info");
                                }
                            }
                        }
                        else if (incident.Attributes.Contains("jarvis_incidentnature") && incident.Attributes["jarvis_incidentnature"] != null && !incident.Attributes["jarvis_incidentnatureshadow"].ToString().Contains("Towing") && isincidentnaturetowingupdated == "No")
                        {
                            EntityCollection getAllExistingRepairInfoRecordsCollection = service.RetrieveMultiple(new FetchExpression(string.Format(MCS.Jarvis.CE.Plugins.Constants.FetchXmls.getAllExistingRepairInfoRecords, incident.Id)));
                            if (getAllExistingRepairInfoRecordsCollection.Entities.Count > 0)
                            {
                                foreach (Entity repairInfoRecords in getAllExistingRepairInfoRecordsCollection.Entities)
                                {

                                    Entity repairInfoRecord = new Entity("jarvis_repairinformation");
                                    repairInfoRecord.Id = repairInfoRecords.Id;
                                    repairInfoRecord["jarvis_towing"] = false;
                                    service.Update(repairInfoRecord);
                                    tracingService.Trace("Repair Info Records are updated with Towing Info");
                                }

                            }
                            // Update JEDs with Towing- False
                            EntityCollection getAllExistingjedsCollection = service.RetrieveMultiple(new FetchExpression(string.Format(MCS.Jarvis.CE.Plugins.Constants.FetchXmls.getAllExistingjedsRecords, incident.Id)));
                            if (getAllExistingjedsCollection.Entities.Count > 0)
                            {
                                foreach (Entity jedsRecords in getAllExistingjedsCollection.Entities)
                                {
                                    Entity jedsRecord = new Entity("jarvis_jobenddetails");
                                    jedsRecord.Id = jedsRecords.Id;
                                    jedsRecord["jarvis_towing"] = false;
                                    service.Update(jedsRecord);
                                    tracingService.Trace("JED Records are updated with Towing Info");
                                }
                            }
                        }

#pragma warning disable SA1123 // Do not place regions within elements
                        #region Vehicle Updates

                        if (incidentRetrieve.Attributes.Contains("jarvis_vehicle") && incidentRetrieve.Attributes["jarvis_vehicle"] != null)
                        {
                            EntityReference vehicle = (EntityReference)incidentRetrieve.Attributes["jarvis_vehicle"];
                            Entity vehicleRetrieve = service.Retrieve(vehicle.LogicalName, vehicle.Id, new ColumnSet("jarvis_isdummyvehicle"));
                            Entity vehicleToUpdate = new Entity(vehicle.LogicalName);
                            bool isUpdate = false;

                            if (incident.Attributes.Contains("jarvis_homedealer") && incident.Attributes["jarvis_homedealer"] != null)
                            {
                                var modifiedBy = (EntityReference)incident.Attributes["modifiedby"];
                                if (modifiedBy != null)
                                {
                                    bool userCheck = CrmHelper.CheckUserIsMercurius(modifiedBy.Id, tracingService, service);
                                    if (!userCheck)
                                    {
                                        EntityReference homeDealer = (EntityReference)incident.Attributes["jarvis_homedealer"];
                                        vehicleToUpdate["jarvis_updatedhomedealer"] = homeDealer;
                                        isUpdate = true;
                                    }
                                }
                            }

                            if (incident.Attributes.Contains("jarvis_registrationnumber") && !string.IsNullOrEmpty((string)incident.Attributes["jarvis_registrationnumber"]))
                            {
                                string regNumber = (string)incident.Attributes["jarvis_registrationnumber"];
                                vehicleToUpdate["jarvis_updatedregistrationnumber"] = regNumber;
                                vehicleToUpdate["jarvis_name"] = regNumber;
                                string regNumberShadow = Regex.Replace(regNumber, @"(\s+|@|&|'|\(|\)|<|>|#|-)", " ");
                                vehicleToUpdate["jarvis_updatedregistrationnumbershadow"] = regNumberShadow;
                                isUpdate = true;
                            }

                            if (incident.Attributes.Contains("customerid") && incident.Attributes["customerid"] != null)
                            {
                                EntityReference customer = (EntityReference)incident.Attributes["customerid"];
                                vehicleToUpdate["jarvis_updatedowningcustomer"] = customer; // jarvis_updatedowningcustomer
                                vehicleToUpdate["jarvis_updatedusingcustomer"] = customer;
                                isUpdate = true;
                            }

                            if (isUpdate && !(vehicleRetrieve.Attributes.Contains("jarvis_isdummyvehicle") && (bool)vehicleRetrieve.Attributes["jarvis_isdummyvehicle"] == true))
                            {
                                tracingService.Trace("Entered Update of Vehicle");

#pragma warning disable SA1123 // Do not place regions within elements
                                #region Update Vehicle

                                vehicleToUpdate.Id = vehicle.Id;
#pragma warning restore SA1123 // Do not place regions within elements
                                service.Update(vehicleToUpdate);

                                #endregion
                            }

#pragma warning disable SA1123 // Do not place regions within elements
                            #region Associate Incident Nature to Case

                            if (incident.Attributes.Contains("jarvis_vehicle") && incident.Attributes["jarvis_vehicle"] != null)
                            {
                                EntityCollection getCaseSoftOffers = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getCaseSoftOffers, incidentRetrieve.Id)));

#pragma warning disable SA1123 // Do not place regions within elements
                                #region Case Soft Offer
                                if (getCaseSoftOffers != null && getCaseSoftOffers.Entities.Count > 0)
                                {
                                    foreach (var item in getCaseSoftOffers.Entities)
                                    {
                                        adminservice.Delete(getCaseSoftOffers.Entities[0].LogicalName, getCaseSoftOffers.Entities[0].Id);
                                    }
                                }
#pragma warning restore SA1123 // Do not place regions within elements

                                DateTime currentDateTime = DateTime.UtcNow;
                                string currentDateString = currentDateTime.ToString("yyyy-MM-dd");
                                EntityCollection getVehicleOffers = adminservice.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getSoftOfferCodes, vehicle.Id, currentDateString, currentDateString)));
                                if (getVehicleOffers != null && getVehicleOffers.Entities.Count > 0)
                                {
                                    CaseOperations caseOperationsSet = new CaseOperations();
                                    caseOperationsSet.CreateCaseSoftOffer(adminservice, incidentRetrieve.ToEntityReference(), getVehicleOffers.Entities[0]);
                                }
                                #endregion
                                /* tracingService.Trace("Entered Associate Incident Nature");
                                 EntityReference vehicleTarget = (EntityReference)incident.Attributes["jarvis_vehicle"];
                                 EntityReference vehiclePre = new EntityReference();
                                 if (incidentPreImage.Attributes.Contains("jarvis_vehicle") && incidentPreImage.Attributes["jarvis_vehicle"] != null)
                                 {
                                     vehiclePre = (EntityReference)incidentPreImage.Attributes["jarvis_vehicle"];
                                 }

                                 if (vehicleTarget.Id != vehiclePre.Id)
                                 {
                                     Incidentnature incidentnature = new Incidentnature();
                                     incidentnature.AssociateIncidentNature(incident, service, vehicle.Id, context.MessageName.ToUpper());
                                 }*/
                            }
#pragma warning restore SA1123 // Do not place regions within elements
                            #endregion
                        }
#pragma warning restore SA1123 // Do not place regions within elements
                        #endregion

#pragma warning disable SA1123 // Do not place regions within elements
                        #region Auto Movement

                        if (incidentRetrieve.Attributes.Contains("casetypecode") && incidentRetrieve.Attributes["casetypecode"] != null)
                        {
                            bool isAutomate = false;
                            isAutomate = CrmHelper.GetAutomationConfig(service, MCS.Jarvis.CE.Plugins.Constants.JarvisConfiguration.Automationcasestatuschange, tracingService);
                            tracingService.Trace("Entered Auto Movement");
                            OptionSetValue caseType = (OptionSetValue)incidentRetrieve.Attributes["casetypecode"];
                            if (isAutomate)
                            {
                                if (caseType.Value != 3)
                                {
                                    // Breakdown
                                    if (incidentRetrieve.Attributes.Contains("statuscode") && incidentRetrieve.Attributes["statuscode"] != null)
                                    {
                                        OptionSetValue caseStatus = (OptionSetValue)incidentRetrieve.Attributes["statuscode"];
                                        if (caseStatus.Value == 10)
                                        {
                                            // Case Opening
                                            if (incidentRetrieve.Attributes.Contains("customerid") && incidentRetrieve.Attributes["customerid"] != null)
                                            {
                                                if (incidentRetrieve.Attributes.Contains("jarvis_country") && incidentRetrieve.Attributes["jarvis_country"] != null)
                                                {
                                                    if (incidentRetrieve.Attributes.Contains("description") && incidentRetrieve.Attributes["description"] != null)
                                                    {
                                                        if (incidentRetrieve.Attributes.Contains("jarvis_homedealer") && incidentRetrieve.Attributes["jarvis_homedealer"] != null)
                                                        {
                                                            // if (incidentRetrieve.Attributes.Contains("jarvis_assistancetype") && incidentRetrieve.Attributes["jarvis_assistancetype"] != null)
                                                            // {
                                                            if (incidentRetrieve.Attributes.Contains("ownerid") && incidentRetrieve.Attributes["ownerid"] != null)
                                                            {
                                                                EntityReference createdBy = (EntityReference)incidentRetrieve.Attributes["ownerid"];
                                                                if (createdBy.Name.ToUpperInvariant().Contains("MERCURIUS"))
                                                                {
                                                                    this.ExecuteBPF(incidentRetrieve.ToEntityReference(), Constants.Incident.BpfStage2, service, tracingService);
                                                                }
                                                            }

                                                            // }
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

#pragma warning disable SA1123 // Do not place regions within elements
                        #region Create Case Monitor - Case Opening

                        if (incidentRetrieve.Attributes.Contains("statuscode") && incidentRetrieve.Attributes["statuscode"] != null)
                        {
                            OptionSetValue statusreason = (OptionSetValue)incidentRetrieve.Attributes["statuscode"];
                            if (statusreason.Value == 10)
                            {
                                // Case Opening
                                if (incident.Attributes.Contains("jarvis_callerrole") || incident.Attributes.Contains("jarvis_callerlanguage"))
                                {
                                    tracingService.Trace("Entered Case Monitor - Case Opening");
                                    if (incidentRetrieve.Attributes.Contains("jarvis_sourceid") && incidentRetrieve.Attributes["jarvis_sourceid"] != null)
                                    {
                                        // jarvis_sourceid
                                        CaseMonitorProcess operations = new CaseMonitorProcess();
                                        if (incidentRetrieve.Attributes.Contains("casetypecode") && incidentRetrieve.Attributes["casetypecode"] != null)
                                        {
                                            caseTypeCode = (OptionSetValue)incidentRetrieve.Attributes["casetypecode"];
                                            if (caseTypeCode.Value == 2)
                                            {
                                                // Breakdown
                                                if (incidentRetrieve.Attributes.Contains("jarvis_callerrole") && incidentRetrieve.Attributes["jarvis_callerrole"] != null)
                                                {
                                                    // jarvis_callerrole
                                                    OptionSetValue callerRole = (OptionSetValue)incidentRetrieve.Attributes["jarvis_callerrole"];
                                                    if (callerRole.Value == 4 || callerRole.Value == 3)
                                                    {
                                                        // HD/RD
                                                        if (callerRole.Value == 4)
                                                        {
                                                            // RD
                                                            string isCountryCode = string.Empty;
                                                            string isoLangCode = string.Empty;
                                                            if (incidentRetrieve.Attributes.Contains("jarvis_callerlanguage") && incidentRetrieve.Attributes["jarvis_callerlanguage"] != null)
                                                            {
                                                                // jarvis_callerlanguage
                                                                EntityReference language = (EntityReference)incidentRetrieve.Attributes["jarvis_callerlanguage"];
                                                                Entity rDlanguage = service.Retrieve(language.LogicalName, language.Id, new ColumnSet("jarvis_iso3languagecode6392t"));
                                                                if (rDlanguage.Attributes.Contains("jarvis_iso3languagecode6392t") && rDlanguage.Attributes["jarvis_iso3languagecode6392t"] != null)
                                                                {
                                                                    isoLangCode = (string)rDlanguage.Attributes["jarvis_iso3languagecode6392t"];
                                                                }
                                                            }

                                                            if (incidentRetrieve.Attributes.Contains("jarvis_homedealer") && incidentRetrieve.Attributes["jarvis_homedealer"] != null)
                                                            {
                                                                EntityReference homeDealer = (EntityReference)incidentRetrieve.Attributes["jarvis_homedealer"];
                                                                Entity account = service.Retrieve(homeDealer.LogicalName, homeDealer.Id, new ColumnSet("jarvis_address1_country"));
                                                                if (account.Attributes.Contains("jarvis_address1_country") && account.Attributes["jarvis_address1_country"] != null)
                                                                {
                                                                    EntityReference hdcountry = (EntityReference)account.Attributes["jarvis_address1_country"];
                                                                    Entity country = service.Retrieve(hdcountry.LogicalName, hdcountry.Id, new ColumnSet("jarvis_iso2countrycode"));
                                                                    if (country.Attributes.Contains("jarvis_iso2countrycode") && country.Attributes["jarvis_iso2countrycode"] != null)
                                                                    {
                                                                        isCountryCode = (string)country.Attributes["jarvis_iso2countrycode"];
                                                                    }
                                                                }
                                                            }

                                                            if (!string.IsNullOrEmpty(isoLangCode) || !string.IsNullOrEmpty(isCountryCode))
                                                            {
                                                                string fucomment = isoLangCode + " " + isCountryCode + " " + "New Case RD";
                                                                EntityReference createdBy = (EntityReference)incidentRetrieve.Attributes["createdby"];
                                                                if (createdBy.Name.ToUpperInvariant().Contains("MERCURIUS"))
                                                                {
                                                                    operations.AutomateMonitorCreation(incidentRetrieve, fucomment, 1, 2, 0, " ", service);
                                                                }
                                                            }
                                                        }

                                                        if (callerRole.Value == 3)
                                                        {
                                                            // HD
                                                            if (incidentRetrieve.Attributes.Contains("jarvis_homedealer") && incidentRetrieve.Attributes["jarvis_homedealer"] != null)
                                                            {
                                                                // jarvis_callerlanguage
                                                                EntityReference homeDealer = (EntityReference)incidentRetrieve.Attributes["jarvis_homedealer"];
                                                                Entity account = service.Retrieve(homeDealer.LogicalName, homeDealer.Id, new ColumnSet("jarvis_language", "jarvis_address1_country"));
                                                                string isoLangCode = string.Empty;
                                                                string isCountryCode = string.Empty;
                                                                if (account.Attributes.Contains("jarvis_address1_country") && account.Attributes["jarvis_address1_country"] != null)
                                                                {
                                                                    EntityReference hdcountry = (EntityReference)account.Attributes["jarvis_address1_country"];
                                                                    Entity country = service.Retrieve(hdcountry.LogicalName, hdcountry.Id, new ColumnSet("jarvis_iso2countrycode"));
                                                                    if (country.Attributes.Contains("jarvis_iso2countrycode") && country.Attributes["jarvis_iso2countrycode"] != null)
                                                                    {
                                                                        isCountryCode = (string)country.Attributes["jarvis_iso2countrycode"];
                                                                    }
                                                                }

                                                                if (account.Attributes.Contains("jarvis_language") && account.Attributes["jarvis_language"] != null)
                                                                {
                                                                    EntityReference language = (EntityReference)account.Attributes["jarvis_language"];
                                                                    Entity hdlanguage = service.Retrieve(language.LogicalName, language.Id, new ColumnSet("jarvis_iso3languagecode6392t"));
                                                                    if (hdlanguage.Attributes.Contains("jarvis_iso3languagecode6392t") && hdlanguage.Attributes["jarvis_iso3languagecode6392t"] != null)
                                                                    {
                                                                        isoLangCode = (string)hdlanguage.Attributes["jarvis_iso3languagecode6392t"];
                                                                    }
                                                                }

                                                                if (!string.IsNullOrEmpty(isoLangCode) || !string.IsNullOrEmpty(isCountryCode))
                                                                {
                                                                    string fucomment = isoLangCode + " " + isCountryCode + " " + "New Case HD";
                                                                    EntityReference createdBy = (EntityReference)incidentRetrieve.Attributes["createdby"];
                                                                    if (createdBy.Name.ToUpperInvariant().Contains("MERCURIUS"))
                                                                    {
                                                                        operations.AutomateMonitorCreation(incidentRetrieve, fucomment, 1, 2, 0, " ", service);
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
                            }
                        }
#pragma warning restore SA1123 // Do not place regions within elements

                        #endregion

#pragma warning disable SA1123 // Do not place regions within elements
                        #region Status Reason
                        if (incident.Attributes.Contains("statuscode") && incident.Attributes["statuscode"] != null)
                        {
                            bool isAutomate = false;
                            isAutomate = CrmHelper.GetAutomationConfig(service, MCS.Jarvis.CE.Plugins.Constants.JarvisConfiguration.Automationcasestatuschange, tracingService);
                            OptionSetValue statusReason = (OptionSetValue)incident.Attributes["statuscode"];
                            tracingService.Trace("Entered Status Reason Update" + statusReason.Value);
                            if (statusReason.Value == 50 || statusReason.Value == 60 || statusReason.Value == 70)
                            {
#pragma warning disable SA1123 // Do not place regions within elements
                                #region Monitor Action Creation - ATC

                                if (incidentRetrieve.Attributes.Contains("jarvis_atc") && incidentRetrieve.Attributes["jarvis_atc"] != null)
                                {
                                    tracingService.Trace("case has ATC hence skipped Chase ATC logic");
                                }
                                else
                                {
                                    EntityCollection getlatestPassOut = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getLastModifiedPassOutForCase, incident.Id)));
                                    tracingService.Trace("latest passouts" + getlatestPassOut.Entities.Count);
                                    if (getlatestPassOut.Entities.Count > 0)
                                    {
                                        EntityReference repairDealer = (EntityReference)getlatestPassOut[0].Attributes["jarvis_repairingdealer"];
                                        if (getlatestPassOut[0].Attributes.Contains("jarvis_etc") && getlatestPassOut[0].Attributes["jarvis_etc"] != null && !getlatestPassOut[0].Attributes.Contains("jarvis_atc"))
                                        {
#pragma warning disable SA1123 // Do not place regions within elements
                                            #region Monitor Creation - ATC
                                            tracingService.Trace("eta is graeter than today +60 mins and inside MA creation for ATC");
#pragma warning restore SA1123 // Do not place regions within elements
                                            CaseMonitorProcess operations = new CaseMonitorProcess();
                                            string isCountryCode = string.Empty;
                                            string isoLangCode = string.Empty;
                                            DateTime etc = DateTime.Now;
                                            if (incidentRetrieve.Attributes.Contains("jarvis_etc") && incidentRetrieve.Attributes["jarvis_etc"] != null)
                                            {
                                                etc = (DateTime)incidentRetrieve.Attributes["jarvis_etc"];
                                            }

                                            Entity account = service.Retrieve(repairDealer.LogicalName, repairDealer.Id, new ColumnSet("jarvis_address1_country", "jarvis_language"));
                                            if (account.Attributes.Contains("jarvis_address1_country") && account.Attributes["jarvis_address1_country"] != null)
                                            {
                                                EntityReference hdcountry = (EntityReference)account.Attributes["jarvis_address1_country"];
                                                Entity country = service.Retrieve(hdcountry.LogicalName, hdcountry.Id, new ColumnSet("jarvis_iso2countrycode"));
                                                if (country.Attributes.Contains("jarvis_iso2countrycode") && country.Attributes["jarvis_iso2countrycode"] != null)
                                                {
                                                    isCountryCode = (string)country.Attributes["jarvis_iso2countrycode"];
                                                }
                                            }

                                            if (account.Attributes.Contains("jarvis_language") && account.Attributes["jarvis_language"] != null)
                                            {
                                                EntityReference language = (EntityReference)account.Attributes["jarvis_language"];
                                                Entity hdlanguage = service.Retrieve(language.LogicalName, language.Id, new ColumnSet("jarvis_iso3languagecode6392t"));
                                                if (hdlanguage.Attributes.Contains("jarvis_iso3languagecode6392t") && hdlanguage.Attributes["jarvis_iso3languagecode6392t"] != null)
                                                {
                                                    isoLangCode = (string)hdlanguage.Attributes["jarvis_iso3languagecode6392t"];
                                                }
                                            }

                                            if (!string.IsNullOrEmpty(isoLangCode) || !string.IsNullOrEmpty(isCountryCode))
                                            {
                                                string fucomment = isoLangCode + " " + isCountryCode + " " + "Chase ATC";
                                                tracingService.Trace("comment:" + fucomment);
                                                operations.AutomateMonitorCreationTime(incidentRetrieve, fucomment, 2, 15, 0, " ", etc, 60, service);
                                            }

                                            #endregion
                                        }
                                    }
                                }
#pragma warning restore SA1123 // Do not place regions within elements
                                #endregion

#pragma warning disable SA1123 // Do not place regions within elements
                                #region Monitor Action Creation - ETC

                                if (incidentRetrieve.Attributes.Contains("jarvis_etc") && incidentRetrieve.Attributes["jarvis_etc"] != null)
                                {
                                    // Skip
                                }
                                else
                                {
                                    EntityCollection getlatestPassOut = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getLastModifiedPassOutForCase, incident.Id)));
                                    if (getlatestPassOut.Entities.Count > 0)
                                    {
                                        EntityReference repairDealer = (EntityReference)getlatestPassOut[0].Attributes["jarvis_repairingdealer"];
                                        EntityCollection getRepairInfo = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getRepairInfo, incident.Id)));
                                        if (getRepairInfo.Entities.Count > 0)
                                        {
#pragma warning disable SA1123 // Do not place regions within elements
                                            #region Monitor Creation - ETC

                                            CaseMonitorProcess operations = new CaseMonitorProcess();
#pragma warning restore SA1123 // Do not place regions within elements
                                            string isCountryCode = string.Empty;
                                            string isoLangCode = string.Empty;

                                            Entity account = service.Retrieve(repairDealer.LogicalName, repairDealer.Id, new ColumnSet("jarvis_address1_country", "jarvis_language"));
                                            if (account.Attributes.Contains("jarvis_address1_country") && account.Attributes["jarvis_address1_country"] != null)
                                            {
                                                EntityReference hdcountry = (EntityReference)account.Attributes["jarvis_address1_country"];
                                                Entity country = service.Retrieve(hdcountry.LogicalName, hdcountry.Id, new ColumnSet("jarvis_iso2countrycode"));
                                                if (country.Attributes.Contains("jarvis_iso2countrycode") && country.Attributes["jarvis_iso2countrycode"] != null)
                                                {
                                                    isCountryCode = (string)country.Attributes["jarvis_iso2countrycode"];
                                                }
                                            }

                                            if (account.Attributes.Contains("jarvis_language") && account.Attributes["jarvis_language"] != null)
                                            {
                                                EntityReference language = (EntityReference)account.Attributes["jarvis_language"];
                                                Entity hdlanguage = service.Retrieve(language.LogicalName, language.Id, new ColumnSet("jarvis_iso3languagecode6392t"));
                                                if (hdlanguage.Attributes.Contains("jarvis_iso3languagecode6392t") && hdlanguage.Attributes["jarvis_iso3languagecode6392t"] != null)
                                                {
                                                    isoLangCode = (string)hdlanguage.Attributes["jarvis_iso3languagecode6392t"];
                                                }
                                            }

                                            if (!string.IsNullOrEmpty(isoLangCode) || !string.IsNullOrEmpty(isCountryCode))
                                            {
                                                string fucomment = isoLangCode + " " + isCountryCode + " " + "Chase ETC";
                                                operations.AutomateMonitorCreation(incident, fucomment, 2, 9, 0, string.Empty, service);
                                            }

                                            #endregion
                                        }
                                    }
                                }
#pragma warning restore SA1123 // Do not place regions within elements
                                #endregion
                            }

                            if (statusReason.Value == 10)
                            {
                                this.ExecuteBPF(incident.ToEntityReference(), Constants.Incident.BpfStage1, service, tracingService);
                            }

                            if (statusReason.Value == 20)
                            {
#pragma warning disable SA1123 // Do not place regions within elements
                                #region Close Monitor Actions

                                CaseMonitorProcess operation = new CaseMonitorProcess();
#pragma warning restore SA1123 // Do not place regions within elements
                                string fucomments = "New Case RD,New Case HD";
                                operation.AutomateCloseMonitorActions(incident, fucomments, 1, fucomments, service);

                                #endregion

                                tracingService.Trace("Validate Case Opening");
                                CaseOperations operations = new CaseOperations();
                                operations.ValidateCaseOpening(service, tracingService, incident, incidentRetrieve);

                                this.ExecuteBPF(incident.ToEntityReference(), Constants.Incident.BpfStage2, service, tracingService);
                                if (statusPre.Value != statusReason.Value)
                                {
                                    this.SendNotification(initiatingUserID, adminservice, incident, Constants.NotificationData.CaseOpneing2Gop);
                                }
                            }

                            if (statusReason.Value == 30)
                            {
#pragma warning disable SA1123 // Do not place regions within elements
                                #region Create Monitor Actions - PassOut
                                string str = "Pass Out";
#pragma warning restore SA1123 // Do not place regions within elements
                                ////EntityCollection getMonitorPassOutActionsForCase = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getDraftPassOutActions, incident.Id, str, 1)));
                                //// getClosePassOutActions
                                EntityCollection getMonitorPassOutActionsForCase = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getClosePassOutActions, incident.Id, str, 1)));

                                if (getMonitorPassOutActionsForCase.Entities.Count < 1)
                                {
                                    if (incidentRetrieve.Attributes.Contains("jarvis_country") && incidentRetrieve.Attributes["jarvis_country"] != null)
                                    {
                                        string isoLangCode = string.Empty;
                                        string isCountryCode = string.Empty;
                                        EntityReference caseCountry = (EntityReference)incidentRetrieve.Attributes["jarvis_country"];
                                        Entity country = service.Retrieve(caseCountry.LogicalName, caseCountry.Id, new ColumnSet("jarvis_iso2countrycode"));
                                        if (country.Attributes.Contains("jarvis_iso2countrycode") && country.Attributes["jarvis_iso2countrycode"] != null)
                                        {
                                            isCountryCode = (string)country.Attributes["jarvis_iso2countrycode"];
                                            EntityCollection languageCollection = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getCountryLanguage, caseCountry.Id)));
                                            if (languageCollection != null && languageCollection.Entities.Count > 0)
                                            {
                                                if (languageCollection.Entities[0].Attributes.Contains("jarvis_iso3languagecode6392t") && languageCollection.Entities[0].Attributes["jarvis_iso3languagecode6392t"] != null)
                                                {
                                                    isoLangCode = (string)languageCollection.Entities[0].Attributes["jarvis_iso3languagecode6392t"];
                                                }
                                            }
                                        }

                                        if (!string.IsNullOrEmpty(isoLangCode) || !string.IsNullOrEmpty(isCountryCode))
                                        {
                                            CaseMonitorProcess moperation = new CaseMonitorProcess();
                                            string fucomm = isoLangCode + " " + isCountryCode + " " + "Pass Out";
                                            moperation.AutomateMonitorCreation(incidentRetrieve, fucomm, 1, 4, 0, " ", service);
                                        }
                                    }
                                }

                                #endregion

#pragma warning disable SA1123 // Do not place regions within elements
                                #region Create Monitor Actions - PassOut by Caller Customer
                                if (incidentRetrieve.Attributes.Contains("jarvis_callerrole") && incidentRetrieve.Attributes["jarvis_callerrole"] != null)
                                {
                                    if (incidentRetrieve.Attributes.Contains("casetypecode") && incidentRetrieve.Attributes["casetypecode"] != null)
                                    {
                                        OptionSetValue caseType = (OptionSetValue)incidentRetrieve.Attributes["casetypecode"];

                                        str = "Pass Out";
                                        OptionSetValue callerRole = (OptionSetValue)incidentRetrieve.Attributes["jarvis_callerrole"];
                                        EntityCollection getMAPassOutForCase = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getClosePassOutActions, incident.Id, str, 1)));
                                        if (getMAPassOutForCase.Entities.Count < 1)
                                        {
                                            if (callerRole.Value == 1 && caseType.Value == 2)
                                            {
#pragma warning disable SA1123 // Do not place regions within elements
                                                EntityCollection getAutoApprGOP = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getAutomaticallyApprovedGOP, incident.Id)));
#pragma warning restore SA1123 // Do not place regions within elements
                                                if (getAutoApprGOP.Entities.Count > 0)
                                                {
                                                    if (incidentRetrieve.Attributes.Contains("jarvis_country") && incidentRetrieve.Attributes["jarvis_country"] != null)
                                                    {
                                                        string isoLangCode = string.Empty;
                                                        string isCountryCode = string.Empty;
                                                        EntityReference caseCountry = (EntityReference)incidentRetrieve.Attributes["jarvis_country"];
                                                        Entity country = service.Retrieve(caseCountry.LogicalName, caseCountry.Id, new ColumnSet("jarvis_iso2countrycode"));
                                                        if (country.Attributes.Contains("jarvis_iso2countrycode") && country.Attributes["jarvis_iso2countrycode"] != null)
                                                        {
                                                            isCountryCode = (string)country.Attributes["jarvis_iso2countrycode"];
                                                            EntityCollection languageCollection = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getCountryLanguage, caseCountry.Id)));
                                                            if (languageCollection != null && languageCollection.Entities.Count > 0)
                                                            {
                                                                if (languageCollection.Entities[0].Attributes.Contains("jarvis_iso3languagecode6392t") && languageCollection.Entities[0].Attributes["jarvis_iso3languagecode6392t"] != null)
                                                                {
                                                                    isoLangCode = (string)languageCollection.Entities[0].Attributes["jarvis_iso3languagecode6392t"];
                                                                }
                                                            }
                                                        }

                                                        if (!string.IsNullOrEmpty(isoLangCode) || !string.IsNullOrEmpty(isCountryCode))
                                                        {
                                                            CaseMonitorProcess moperation = new CaseMonitorProcess();
                                                            string fucomm = isoLangCode + " " + isCountryCode + " " + "Pass Out";
                                                            moperation.AutomateMonitorCreation(incidentRetrieve, fucomm, 1, 4, 0, " ", service);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
#pragma warning restore SA1123 // Do not place regions within elements
                                #endregion

#pragma warning disable SA1123 // Do not place regions within elements
                                #region Create Monitor Actions - Confirm Case Opening
                                if (incidentRetrieve.Attributes.Contains("jarvis_callerrole") && incidentRetrieve.Attributes["jarvis_callerrole"] != null)
                                {
                                    str = "Confirm case opening";
                                    OptionSetValue callerRole = (OptionSetValue)incidentRetrieve.Attributes["jarvis_callerrole"];
                                    EntityReference createdBy = (EntityReference)incidentRetrieve.Attributes["createdby"];
                                    EntityCollection getMonitorCaseConfirmForCase = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getClosePassOutActions, incident.Id, str, 1)));
                                    if (createdBy.Name.ToUpperInvariant().Contains("MERCURIUS") && getMonitorCaseConfirmForCase.Entities.Count < 1)
                                    {
                                        if (callerRole.Value != 1)
                                        {
                                            if (callerRole.Value != 3)
                                            {
#pragma warning disable SA1123 // Do not place regions within elements
                                                #region Get Auto Approved GOP

                                                EntityCollection getAutoApprGOP = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getAutomaticallyApprovedGOP, incident.Id)));
#pragma warning restore SA1123 // Do not place regions within elements
                                                //// throw new InvalidPluginExecutionException("Count of GOP: " + getAutoApprGOP.Entities.Count.ToString() + "");
                                                if (getAutoApprGOP.Entities.Count > 0)
                                                {
                                                    if (incidentRetrieve.Attributes.Contains("jarvis_country") && incidentRetrieve.Attributes["jarvis_country"] != null)
                                                    {
                                                        string isoLangCode = string.Empty;
                                                        string isCountryCode = string.Empty;
                                                        EntityReference caseCountry = (EntityReference)incidentRetrieve.Attributes["jarvis_country"];
                                                        Entity country = service.Retrieve(caseCountry.LogicalName, caseCountry.Id, new ColumnSet("jarvis_iso2countrycode"));
                                                        if (country.Attributes.Contains("jarvis_iso2countrycode") && country.Attributes["jarvis_iso2countrycode"] != null)
                                                        {
                                                            isCountryCode = (string)country.Attributes["jarvis_iso2countrycode"];
                                                            EntityCollection languageCollection = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getCountryLanguage, caseCountry.Id)));
                                                            if (languageCollection != null && languageCollection.Entities.Count > 0)
                                                            {
                                                                if (languageCollection.Entities[0].Attributes.Contains("jarvis_iso3languagecode6392t") && languageCollection.Entities[0].Attributes["jarvis_iso3languagecode6392t"] != null)
                                                                {
                                                                    isoLangCode = (string)languageCollection.Entities[0].Attributes["jarvis_iso3languagecode6392t"];
                                                                }
                                                            }
                                                        }

                                                        if (!string.IsNullOrEmpty(isoLangCode) || !string.IsNullOrEmpty(isCountryCode))
                                                        {
                                                            CaseMonitorProcess moperation = new CaseMonitorProcess();
                                                            string fucomm = isoLangCode + " " + isCountryCode + " " + "Confirm case opening";
                                                            moperation.AutomateMonitorCreation(incidentRetrieve, fucomm, 1, 2, 0, " ", service);
                                                        }
                                                    }
                                                }
                                                #endregion
                                            }
                                        }
                                    }
                                }
#pragma warning restore SA1123 // Do not place regions within elements
                                #endregion

#pragma warning disable SA1123 // Do not place regions within elements
                                #region Close Monitor Actions

                                CaseMonitorProcess operation = new CaseMonitorProcess();
#pragma warning restore SA1123 // Do not place regions within elements
                                ////string fucomments = "Chase GOP,New Case RD,New Case HD";
                                string fucomments = "New Case RD,New Case HD";
                                operation.AutomateCloseMonitorActions(incident, fucomments, 1, fucomments, service);

                                #endregion
                                this.ExecuteBPF(incident.ToEntityReference(), Constants.Incident.BpfStage3, service, tracingService);
                                if (incidentRetrieve.Attributes.Contains("jarvis_mercuriusstatus") && incidentRetrieve.Attributes["jarvis_mercuriusstatus"] != null)
                                {
                                    // 300
                                    OptionSetValue mercuriusStatus = (OptionSetValue)incidentRetrieve.Attributes["jarvis_mercuriusstatus"];
                                    if (statusPre.Value != statusReason.Value)
                                    {
                                        this.SendNotification(initiatingUserID, adminservice, incident, Constants.NotificationData.Gop2PassOut);
                                    }
                                }
                            }

                            if (statusReason.Value == 40)
                            {
#pragma warning disable SA1123 // Do not place regions within elements
                                #region Close Monitor Actions

                                CaseMonitorProcess operation = new CaseMonitorProcess();
#pragma warning restore SA1123 // Do not place regions within elements
                                string fucomments = "Pass out,Confirm case opening,FU RD on PO Approval";
                                operation.AutomateCloseMonitorActions(incident, fucomments, 1, fucomments, service);

                                #endregion

                                this.ExecuteBPF(incident.ToEntityReference(), Constants.Incident.BpfStage4, service, tracingService);

#pragma warning disable SA1123 // Do not place regions within elements
                                #region AutoMovement - ETA

                                if (incidentRetrieve.Attributes.Contains("jarvis_eta") && incidentRetrieve.Attributes["jarvis_eta"] != null && isAutomate)
                                {
                                    this.ExecuteBPF(incident.ToEntityReference(), Constants.Incident.BpfStage5, service, tracingService);
                                }
#pragma warning restore SA1123 // Do not place regions within elements

                                #endregion
                            }

                            if (statusReason.Value == 50)
                            {
#pragma warning disable SA1123 // Do not place regions within elements
                                #region Close Monitor Actions

                                CaseMonitorProcess operation = new CaseMonitorProcess();
#pragma warning restore SA1123 // Do not place regions within elements
                                string fucomments = "Pass ETA,Chase ETA";
                                operation.AutomateCloseMonitorActions(incident, fucomments, 1, fucomments, service);

                                #endregion

                                if (incidentRetrieve.Attributes.Contains("jarvis_ata") && incidentRetrieve.Attributes["jarvis_ata"] != null && isAutomate)
                                {
                                    this.ExecuteBPF(incident.ToEntityReference(), Constants.Incident.BpfStage6, service, tracingService);
                                }
                                else
                                {
                                    this.ExecuteBPF(incident.ToEntityReference(), Constants.Incident.BpfStage5, service, tracingService);
                                }

                                if (statusPre.Value != statusReason.Value)
                                {
                                    this.SendNotification(initiatingUserID, adminservice, incident, Constants.NotificationData.EtaTechnician2Waitingforrepairstart);
                                }
                            }

                            if (statusReason.Value == 60)
                            {
                                bool jedPresent = false;
                                if (incidentRetrieve.Attributes.Contains("jarvis_actualcausefault") && incidentRetrieve.Attributes.Contains("jarvis_mileageafterrepair") && incidentRetrieve.Attributes.Contains("jarvis_mileageunitafterrepair")
                                    && incidentRetrieve.Attributes["jarvis_actualcausefault"] != null && incidentRetrieve.Attributes["jarvis_mileageafterrepair"] != null && incidentRetrieve.Attributes["jarvis_mileageunitafterrepair"] != null)
                                {
                                    jedPresent = true;
                                    tracingService.Trace("Setting JED present true");
                                }

                                if (incidentRetrieve.Attributes.Contains("jarvis_atc") && incidentRetrieve.Attributes["jarvis_atc"] != null && isAutomate && jedPresent)
                                {
                                    EntityCollection passOut = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getNonATCPassOuts, incidentRetrieve.Id)));
                                    if (passOut.Entities.Count == 0)
                                    {
                                        tracingService.Trace("Navigating stage to Case Closure");
                                        this.ExecuteBPF(incident.ToEntityReference(), Constants.Incident.BpfStage9, service, tracingService);

#pragma warning disable SA1123 // Do not place regions within elements
                                        #region Close Monitor Actions
                                        CaseMonitorProcess operation = new CaseMonitorProcess();
#pragma warning restore SA1123 // Do not place regions within elements

                                        string fucomments = "Pass ETC,ATC";
                                        operation.AutomateCloseMonitorActions(incident, fucomments, 1, fucomments, service);

                                        #endregion
                                    }
                                }
                                else if (((incidentRetrieve.Attributes.Contains("jarvis_etc") && incidentRetrieve.Attributes["jarvis_etc"] != null)
                                    || (incidentRetrieve.Attributes.Contains("jarvis_atc") && incidentRetrieve.Attributes["jarvis_atc"] != null)) && isAutomate)
                                {
                                    this.ExecuteBPF(incident.ToEntityReference(), Constants.Incident.BpfStage7, service, tracingService);
                                }
                                else
                                {
                                    this.ExecuteBPF(incident.ToEntityReference(), Constants.Incident.BpfStage6, service, tracingService);
                                }

                                if (statusPre.Value != statusReason.Value)
                                {
                                    this.SendNotification(initiatingUserID, adminservice, incident, Constants.NotificationData.Waitingforrepairstart2Repairongoing);
                                }
                            }

                            if (statusReason.Value == 70)
                            {
#pragma warning disable SA1123 // Do not place regions within elements
                                #region Close Monitor Actions

                                CaseMonitorProcess operation = new CaseMonitorProcess();
#pragma warning restore SA1123 // Do not place regions within elements
                                string fucomments = "Chase ETC,Pass Repair info";
                                operation.AutomateCloseMonitorActions(incident, fucomments, 1, fucomments, service);

                                #endregion
                                bool jedPresent = false;
                                if (incidentRetrieve.Attributes.Contains("jarvis_actualcausefault") && incidentRetrieve.Attributes.Contains("jarvis_mileageafterrepair") && incidentRetrieve.Attributes.Contains("jarvis_mileageunitafterrepair")
                                    && incidentRetrieve.Attributes["jarvis_actualcausefault"] != null && incidentRetrieve.Attributes["jarvis_mileageafterrepair"] != null && incidentRetrieve.Attributes["jarvis_mileageunitafterrepair"] != null)
                                {
                                    jedPresent = true;
                                    tracingService.Trace("Setting JED present true");
                                }

                                if (incidentRetrieve.Attributes.Contains("jarvis_atc") && incidentRetrieve.Attributes["jarvis_atc"] != null && isAutomate && jedPresent)
                                {
                                    EntityCollection passOut = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getNonATCPassOuts, incidentRetrieve.Id)));
                                    if (passOut.Entities.Count == 0)
                                    {
                                        tracingService.Trace("Navigating stage to Case Closure");
                                        this.ExecuteBPF(incident.ToEntityReference(), Constants.Incident.BpfStage9, service, tracingService);

#pragma warning disable SA1123 // Do not place regions within elements
                                        #region Close Monitor Actions

                                        fucomments = "Pass ETC,ATC";
#pragma warning restore SA1123 // Do not place regions within elements
                                        operation.AutomateCloseMonitorActions(incident, fucomments, 1, fucomments, service);

                                        #endregion
                                    }
                                }
                                else if (incidentRetrieve.Attributes.Contains("jarvis_atc") && incidentRetrieve.Attributes["jarvis_atc"] != null && isAutomate)
                                {
                                    EntityCollection passOut = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getNonATCPassOuts, incidentRetrieve.Id)));
                                    if (passOut.Entities.Count == 0)
                                    {
                                        tracingService.Trace("Navigating stage to Repair Summary");
                                        this.ExecuteBPF(incident.ToEntityReference(), Constants.Incident.BpfStage8, service, tracingService);

#pragma warning disable SA1123 // Do not place regions within elements
                                        #region Close Monitor Actions

                                        fucomments = "Pass ETC,ATC";
#pragma warning restore SA1123 // Do not place regions within elements
                                        operation.AutomateCloseMonitorActions(incident, fucomments, 1, fucomments, service);

                                        #endregion
                                    }
                                }
                                else
                                {
                                    this.ExecuteBPF(incident.ToEntityReference(), Constants.Incident.BpfStage7, service, tracingService);
                                }

                                if (statusPre.Value != statusReason.Value)
                                {
                                    this.SendNotification(initiatingUserID, adminservice, incident, Constants.NotificationData.RepairOngoing2Repairfinished);
                                }
                            }

                            if (statusReason.Value == 80)
                            {
                                bool jedPresent = false;
                                if (incidentRetrieve.Attributes.Contains("jarvis_actualcausefault") && incidentRetrieve.Attributes.Contains("jarvis_mileageafterrepair") && incidentRetrieve.Attributes.Contains("jarvis_mileageunitafterrepair")
                                    && incidentRetrieve.Attributes["jarvis_actualcausefault"] != null && incidentRetrieve.Attributes["jarvis_mileageafterrepair"] != null && incidentRetrieve.Attributes["jarvis_mileageunitafterrepair"] != null)
                                {
                                    jedPresent = true;
                                    tracingService.Trace("Setting JED present true");
                                }

                                if (incidentRetrieve.Attributes.Contains("jarvis_atc") && incidentRetrieve.Attributes["jarvis_atc"] != null && isAutomate && jedPresent)
                                {
                                    EntityCollection passOut = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getNonATCPassOuts, incidentRetrieve.Id)));
                                    if (passOut.Entities.Count == 0)
                                    {
                                        tracingService.Trace("Navigating stage to Case Closure");
                                        this.ExecuteBPF(incident.ToEntityReference(), Constants.Incident.BpfStage9, service, tracingService);
                                    }
                                }
                                else
                                {
                                    this.ExecuteBPF(incident.ToEntityReference(), Constants.Incident.BpfStage8, service, tracingService);
                                }

#pragma warning disable SA1123 // Do not place regions within elements
                                #region Close Monitor Actions

                                CaseMonitorProcess operation = new CaseMonitorProcess();
#pragma warning restore SA1123 // Do not place regions within elements
                                string fucomments = "Pass ETC,ATC";
                                operation.AutomateCloseMonitorActions(incident, fucomments, 1, fucomments, service);

                                #endregion
                                //// this.ExecuteBPF(incident.ToEntityReference(), Constants.Incident.bpfStage8, service, tracingService);
                                if (statusPre.Value != statusReason.Value)
                                {
                                    this.SendNotification(initiatingUserID, adminservice, incident, Constants.NotificationData.Repairfinished2Repairsummary);
                                }
                            }

                            if (statusReason.Value == 90 && ((OptionSetValue)incidentRetrieve["statecode"]).Value == 0)
                            {
                                // throw new InvalidPluginExecutionException("resol");
                                this.ExecuteBPF(incident.ToEntityReference(), Constants.Incident.BpfStage9, service, tracingService);
                                if (statusPre.Value != statusReason.Value)
                                {
                                    this.SendNotification(initiatingUserID, adminservice, incident, Constants.NotificationData.Repairsummary2Caseclosure);
                                }
                                //// 633887 - 3.1.3
                                CaseMonitorProcess monitorProcess = new CaseMonitorProcess();
                                monitorProcess.UpdateNextMOForCase(adminservice, incident, tracingService);
                            }

                            if (statusReason.Value == 120)
                            {
                                this.ExecuteBPF(incident.ToEntityReference(), Constants.Incident.BpfStage10, service, tracingService);
                            }

                            if (statusReason.Value == 130)
                            {
                                this.ExecuteBPF(incident.ToEntityReference(), Constants.Incident.BpfStage11, service, tracingService);
                            }

                            if (statusReason.Value == 140)
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
                                            this.ExecuteBPF(incident.ToEntityReference(), Constants.Incident.BpfStage13, service, tracingService);
                                        }
                                        else
                                        {
                                            this.ExecuteBPF(incident.ToEntityReference(), Constants.Incident.BpfStage12, service, tracingService);
                                        }
                                    }
                                    else
                                    {
                                        this.ExecuteBPF(incident.ToEntityReference(), Constants.Incident.BpfStage12, service, tracingService);
                                    }
                                }
                            }

                            if (statusReason.Value == 150)
                            { // Skip Validations
                                CaseMonitorProcess operation = new CaseMonitorProcess();
                                operation.CloseMonitorActions(incidentRetrieve, service, tracingService);
                                this.ExecuteBPF(incident.ToEntityReference(), Constants.Incident.BpfStage13, service, tracingService);
                            }

#pragma warning disable SA1123 // Do not place regions within elements
                            #region Pass Out

                            // Create Work Order
#pragma warning disable SA1123 // Do not place regions within elements
                            #region Create WO

                            if (statusReason.Value == 30)
                            {
                                // Pass Out
                                service = serviceFactory.CreateOrganizationService(null);

#pragma warning disable SA1123 // Do not place regions within elements
                                #region Case Monitor Action - PassOut

#pragma warning disable SA1123 // Do not place regions within elements
                                #region Close Monitor Actions

                                CaseMonitorProcess operations = new CaseMonitorProcess();
#pragma warning restore SA1123 // Do not place regions within elements
#pragma warning restore SA1123 // Do not place regions within elements
                                ////string fucomment = "Chase GOP";
                                //// operations.AutomateCloseMonitorActions(incident, fucomment, 1, fucomment, service);

                                #endregion

                                #endregion
                                //// throw new InvalidPluginExecutionException("WO");
                                Guid workOrderID = Guid.Empty;
                                EntityCollection retriveWO = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getWorkOrders, incidentRetrieve.Id)));

                                if (retriveWO.Entities.Count == 0)
                                {
                                    Entity workOrder = new Entity("msdyn_workorder");
                                    workOrder["msdyn_servicerequest"] = incidentRetrieve.ToEntityReference();
                                    if (incidentRetrieve.Attributes.Contains("jarvis_latitude") && incidentRetrieve.Attributes["jarvis_latitude"] != null)
                                    {
                                        decimal latitude = (decimal)incidentRetrieve.Attributes["jarvis_latitude"];
                                        workOrder["msdyn_latitude"] = Convert.ToDouble(latitude);
                                    }

                                    if (incidentRetrieve.Attributes.Contains("jarvis_longitude") && incidentRetrieve.Attributes["jarvis_longitude"] != null)
                                    {
                                        decimal longitude = (decimal)incidentRetrieve.Attributes["jarvis_longitude"];
                                        workOrder["msdyn_longitude"] = Convert.ToDouble(longitude);
                                    }

                                    if (incidentRetrieve.Attributes.Contains("customerid") && incidentRetrieve.Attributes["customerid"] != null)
                                    {
                                        workOrder["msdyn_serviceaccount"] = (EntityReference)incidentRetrieve.Attributes["customerid"];
                                    }

                                    EntityCollection workTypes = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getWorkType)));
                                    if (workTypes.Entities.Count > 0)
                                    {
                                        workOrder["msdyn_workordertype"] = new EntityReference(workTypes.Entities[0].LogicalName, workTypes.Entities[0].Id);
                                    }

                                    workOrderID = service.Create(workOrder);

#pragma warning disable SA1123 // Do not place regions within elements
                                    #region Create WO Incident From Brand

                                    if (workOrderID != Guid.Empty)
                                    {
                                        if (incidentRetrieve.Attributes.Contains("jarvis_vehicle") && incidentRetrieve.Attributes["jarvis_vehicle"] != null)
                                        {
                                            EntityReference vehicleID = (EntityReference)incidentRetrieve.Attributes["jarvis_vehicle"];
                                            Entity vehicle = service.Retrieve(vehicleID.LogicalName, vehicleID.Id, new ColumnSet("jarvis_brandid"));
                                            if (vehicle.Attributes.Contains("jarvis_brandid") && vehicle.Attributes["jarvis_brandid"] != null)
                                            {
                                                EntityReference brand = (EntityReference)vehicle.Attributes["jarvis_brandid"];
                                                EntityCollection incidentTypesColl = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getIncidentTypesBrands, brand.Id)));
                                                if (incidentTypesColl.Entities.Count > 0)
                                                {
                                                    for (int i = 0; i < incidentTypesColl.Entities.Count; i++)
                                                    {
                                                        Entity incidentTypeForWO = new Entity("msdyn_workorderincident");
                                                        incidentTypeForWO["msdyn_incidenttype"] = incidentTypesColl.Entities[i].ToEntityReference();
                                                        if (i == 0)
                                                        {
                                                            // First Iteration
                                                            incidentTypeForWO["msdyn_isprimary"] = true;
                                                        }

                                                        incidentTypeForWO["msdyn_workorder"] = new EntityReference("msdyn_workorder", workOrderID);
                                                        service.Create(incidentTypeForWO);
                                                    }
                                                }
                                            }
                                        }
                                    }
#pragma warning restore SA1123 // Do not place regions within elements

                                    #endregion

#pragma warning disable SA1123 // Do not place regions within elements
                                    #region Create WO Incident From Case

                                    if (workOrderID != Guid.Empty)
                                    {
                                        EntityCollection incidentTypesColl = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getIncidentNatureForCase, incidentRetrieve.Id)));
                                        if (incidentTypesColl.Entities.Count > 0)
                                        {
                                            foreach (var items in incidentTypesColl.Entities)
                                            {
                                                Entity incidentTypeForWO = new Entity("msdyn_workorderincident");
                                                if (items.Attributes.Contains("jarvis_incidenttype") && items.Attributes["jarvis_incidenttype"] != null)
                                                {
                                                    incidentTypeForWO["msdyn_incidenttype"] = (EntityReference)items.Attributes["jarvis_incidenttype"];
                                                }

                                                incidentTypeForWO["msdyn_workorder"] = new EntityReference("msdyn_workorder", workOrderID);
                                                service.Create(incidentTypeForWO);
                                            }
                                        }
                                    }
#pragma warning restore SA1123 // Do not place regions within elements

                                    #endregion
                                }

                                this.ExecuteBPF(incident.ToEntityReference(), Constants.Incident.BpfStage3, service, tracingService);
                            }
#pragma warning restore SA1123 // Do not place regions within elements
#pragma warning restore SA1123 // Do not place regions within elements
                            #endregion

                            #endregion

                            if (statusReason.Value == 5)
                            {
#pragma warning disable SA1123 // Do not place regions within elements
                                #region Check for Case Closure
                                if (incident.Attributes.Contains("modifiedby") && incident.Attributes["modifiedby"] != null)
                                {
                                    EntityReference modifiedByID = (EntityReference)incident.Attributes["modifiedby"];
                                    string modifiedBy = string.Empty;
                                    EntityCollection systemuser = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getModifiedUser, modifiedByID.Id)));
                                    OptionSetValue releaseConfig = new OptionSetValue();
                                    releaseConfig = CrmHelper.GetReleaseAutomationConfig(service, MCS.Jarvis.CE.Plugins.Constants.JarvisConfiguration.Automationreleasecase, tracingService);
                                    if (systemuser != null && systemuser.Entities.Count > 0)
                                    {
                                        modifiedBy = (string)systemuser.Entities[0].Attributes["fullname"];
                                    }

                                    if (modifiedBy.ToUpperInvariant().Contains("MERCURIUS") || releaseConfig.Value == 1)
                                    {
                                        // Skip Validations
                                        CaseMonitorProcess operation = new CaseMonitorProcess();
                                        operation.CloseMonitorActions(incidentRetrieve, service, tracingService);
                                    }
                                }
#pragma warning restore SA1123 // Do not place regions within elements

                                if (caseTypeCode.Value == 2)
                                {
                                    // Breakdown
                                    EntityCollection user = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getUnAssignedUser)));

#pragma warning disable SA1123 // Do not place regions within elements
                                    #region Assign OwnerShip to Unassigned
                                    if (user.Entities.Count > 0)
                                    {
                                        Entity caseToUpdate = new Entity(incident.LogicalName);
                                        caseToUpdate.Id = incident.Id;
                                        caseToUpdate["ownerid"] = user.Entities[0].ToEntityReference();
                                        service.Update(caseToUpdate);
                                    }
#pragma warning restore SA1123 // Do not place regions within elements
                                    #endregion
                                }

                                #endregion

                                VASBreakDownProcess vasBreakdown = new VASBreakDownProcess();
                                vasBreakdown.setStateBPF(incident, service, 2, 1);
                            }
                        }
#pragma warning restore SA1123 // Do not place regions within elements

                        #endregion

#pragma warning disable SA1123 // Do not place regions within elements
                        #region SetRD/HD
                        if (incident.Attributes.Contains("jarvis_homedealer") && incidentRetrieve.Attributes.Contains("casetypecode") && incidentRetrieve.Attributes["casetypecode"] != null)
                        {
                            OptionSetValue statusReason = (OptionSetValue)incidentRetrieve.Attributes["statuscode"];
                            tracingService.Trace("Entered SetRDHD of Case");
                            if (statusReason.Value != 5)
                            {
                                // Case Resolved
                                if (statusReason.Value != 1000)
                                {
                                    // Force Closed
                                    if (caseTypeCode.Value == 2)
                                    {
                                        // Breakdown
                                        this.SetRDHD(service, incident);
                                    }
                                }
                            }
                        }
#pragma warning restore SA1123 // Do not place regions within elements
                        #endregion

#pragma warning disable SA1123 // Do not place regions within elements
                        #region US_248390_5.3.1_MonitorActionETA
                        ////get CustomerContact
                        tracingService.Trace("inside PassETA_MonitorActionETA");
#pragma warning restore SA1123 // Do not place regions within elements
                        EntityCollection getContactwithMobile = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.caseContactHasMobile, incidentRetrieve.Id)));
                        if (getContactwithMobile.Entities.Count > 0)
                        {
                            customerContactHasMobile = true;
                            tracingService.Trace("customer has mobile count:" + getContactwithMobile.Entities.Count);
                        }

                        tracingService.Trace("customerContactHasMobile:" + customerContactHasMobile);
                        if (!customerContactHasMobile && incident.Attributes.Contains("jarvis_eta") && incident.Attributes["jarvis_eta"] != null && !incident.Attributes.Contains("jarvis_ata") && !incidentPreImage.Attributes.Contains("jarvis_eta"))
                        {
                            tracingService.Trace("Entered MA ETC");
                            Entity incidentATA = service.Retrieve(incident.LogicalName, incident.Id, new ColumnSet("jarvis_ata"));
                            if (!incident.Attributes.Contains("jarvis_ata"))
                            {
                                OptionSetValue statusReason = (OptionSetValue)incidentRetrieve.Attributes["statuscode"];
                                if (statusReason.Value == 30 || statusReason.Value == 40 || statusReason.Value == 50)
                                {
#pragma warning disable SA1123 // Do not place regions within elements
                                    #region Monitor Creation - ATC
                                    tracingService.Trace("Status reason" + statusReason.Value);
#pragma warning restore SA1123 // Do not place regions within elements
                                    CaseMonitorProcess operations = new CaseMonitorProcess();
                                    string isCountryCode = string.Empty;
                                    string isoLangCode = string.Empty;

                                    if (incidentRetrieve.Attributes.Contains("jarvis_onetimecustomercountry") && incidentRetrieve.Attributes["jarvis_onetimecustomercountry"] != null)
                                    {
                                        EntityReference hdcountry = (EntityReference)incidentRetrieve.Attributes["jarvis_onetimecustomercountry"];
                                        Entity country = service.Retrieve(hdcountry.LogicalName, hdcountry.Id, new ColumnSet("jarvis_iso2countrycode"));
                                        if (country.Attributes.Contains("jarvis_iso2countrycode") && country.Attributes["jarvis_iso2countrycode"] != null)
                                        {
                                            isCountryCode = (string)country.Attributes["jarvis_iso2countrycode"];
                                        }
                                    }

                                    if (incidentRetrieve.Attributes.Contains("jarvis_onetimecustomerlanguage") && incidentRetrieve.Attributes["jarvis_onetimecustomerlanguage"] != null)
                                    {
                                        EntityReference language = (EntityReference)incidentRetrieve.Attributes["jarvis_onetimecustomerlanguage"];
                                        Entity hdlanguage = service.Retrieve(language.LogicalName, language.Id, new ColumnSet("jarvis_iso3languagecode6392t"));
                                        if (hdlanguage.Attributes.Contains("jarvis_iso3languagecode6392t") && hdlanguage.Attributes["jarvis_iso3languagecode6392t"] != null)
                                        {
                                            isoLangCode = (string)hdlanguage.Attributes["jarvis_iso3languagecode6392t"];
                                        }
                                    }

                                    if (!string.IsNullOrEmpty(isoLangCode) || !string.IsNullOrEmpty(isCountryCode))
                                    {
                                        tracingService.Trace("Creating MA Pass ETA");
                                        string fucomment = isoLangCode + " " + isCountryCode + " " + "Pass ETA";
                                        operations.AutomateMonitorCreationTime(incidentRetrieve, fucomment, 2, 11, 0, " ", DateTime.UtcNow, 0, service);
                                    }

                                    #endregion
                                }
                            }
                        }

                        #endregion

#pragma warning disable SA1123 // Do not place regions within elements
                        #region US_248390_5.3.4 MonitorActionATC

                        ////get CustomerContact
                        tracingService.Trace("inside 5.3.4_MonitorActionETA");
#pragma warning restore SA1123 // Do not place regions within elements
                        EntityCollection getContactwithMobileandEmail = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.caseContactHasMobileAndEmail, incidentRetrieve.Id)));
                        if (getContactwithMobileandEmail.Entities.Count > 0)
                        {
                            customerContactHasMobile = true;
                            tracingService.Trace("customer has mobile count:" + getContactwithMobileandEmail.Entities.Count);
                        }

                        tracingService.Trace("customerContactHasMobile:" + customerContactHasMobile);
                        if (!customerContactHasMobile && incident.Attributes.Contains("jarvis_atc") && incident.Attributes["jarvis_atc"] != null)
                        {
                            tracingService.Trace("Entered MA ATC");
                            OptionSetValue statusReason = (OptionSetValue)incidentRetrieve.Attributes["statuscode"];
                            if (statusReason.Value == 50 || statusReason.Value == 60 || statusReason.Value == 70 || statusReason.Value == 80)
                            {
#pragma warning disable SA1123 // Do not place regions within elements
                                #region Monitor Creation - ATC
                                tracingService.Trace("Status reason" + statusReason.Value);
#pragma warning restore SA1123 // Do not place regions within elements
                                CaseMonitorProcess operations = new CaseMonitorProcess();
                                string isCountryCode = string.Empty;
                                string isoLangCode = string.Empty;
                                if (incidentRetrieve.Attributes.Contains("jarvis_onetimecustomercountry") && incidentRetrieve.Attributes["jarvis_onetimecustomercountry"] != null)
                                {
                                    EntityReference hdcountry = (EntityReference)incidentRetrieve.Attributes["jarvis_onetimecustomercountry"];
                                    Entity country = service.Retrieve(hdcountry.LogicalName, hdcountry.Id, new ColumnSet("jarvis_iso2countrycode"));
                                    if (country.Attributes.Contains("jarvis_iso2countrycode") && country.Attributes["jarvis_iso2countrycode"] != null)
                                    {
                                        isCountryCode = (string)country.Attributes["jarvis_iso2countrycode"];
                                    }
                                }

                                if (incidentRetrieve.Attributes.Contains("jarvis_onetimecustomerlanguage") && incidentRetrieve.Attributes["jarvis_onetimecustomerlanguage"] != null)
                                {
                                    EntityReference language = (EntityReference)incidentRetrieve.Attributes["jarvis_onetimecustomerlanguage"];
                                    Entity hdlanguage = service.Retrieve(language.LogicalName, language.Id, new ColumnSet("jarvis_iso3languagecode6392t"));
                                    if (hdlanguage.Attributes.Contains("jarvis_iso3languagecode6392t") && hdlanguage.Attributes["jarvis_iso3languagecode6392t"] != null)
                                    {
                                        isoLangCode = (string)hdlanguage.Attributes["jarvis_iso3languagecode6392t"];
                                    }
                                }

                                if (!string.IsNullOrEmpty(isoLangCode) || !string.IsNullOrEmpty(isCountryCode))
                                {
                                    tracingService.Trace("Creating MA Pass ATC");
                                    string fucomment = isoLangCode + " " + isCountryCode + " " + "Pass ATC";
                                    operations.AutomateMonitorCreationTime(incidentRetrieve, fucomment, 2, 15, 0, " ", DateTime.UtcNow, 0, service);
                                }

                                #endregion
                            }
                        }

                        #endregion

                        // Check if target contains Driver or Caller data
                        if (caseTypeCode.Value == 2)
                        {
                            // Breakdown
                            if (incident.Attributes.Contains("jarvis_callernameargus") || incident.Attributes.Contains("jarvis_callerphone") ||
                            incident.Attributes.Contains("jarvis_callerlanguage") || incident.Attributes.Contains("jarvis_callerrole") ||
                            incident.Attributes.Contains("customerid") || incident.Attributes.Contains("jarvis_drivername") ||
                            incident.Attributes.Contains("jarvis_driverphone") || incident.Attributes.Contains("jarvis_driverlanguage") ||
                            incident.Attributes.Contains("jarvis_driverlanguagesupported") || incident.Attributes.Contains("jarvis_callerlanguagesupported") ||
                            incident.Attributes.Contains("jarvis_callerphonenumbertype") || incident.Attributes.Contains("jarvis_calleremail"))
                            {
                                tracingService.Trace("Entered CaseContactIncident of Case");
                                CaseContactIncident contact = new CaseContactIncident();
                                tracingService.Trace("Case Contact Create");
                                contact.CaseContactCreate(incident, service, context.MessageName.ToUpper(), incidentRetrieve, tracingService);
                            }
                        }

#pragma warning disable SA1123 // Do not place regions within elements
                        #region Customer CaseContact Update
                        if (incident.Attributes.Contains("customerid") && incident.Attributes["customerid"] != null)
                        {
                            EntityReference caseNewCustomer = (EntityReference)incident.Attributes["customerid"];
                            if (incidentPreImage.Attributes.Contains("customerid") && incidentPreImage.Attributes["customerid"] != null)
                            {
                                EntityReference caseCustomer = (EntityReference)incidentPreImage.Attributes["customerid"];
                                if (caseCustomer.Id != caseNewCustomer.Id)
                                {
                                    CaseContactIncident contactCustomer = new CaseContactIncident();
                                    contactCustomer.CreateCustomerCaseContact(incident, adminservice, context.MessageName.ToUpper(), tracingService);
                                    contactCustomer.CreateCustomerContacts(incident, adminservice, context.MessageName.ToUpper(), tracingService);
                                }
                            }
                        }
#pragma warning restore SA1123 // Do not place regions within elements

                        #endregion

#pragma warning disable SA1123 // Do not place regions within elements
                        #region Create Case Escalation/De-escalation
                        if (incident.Attributes.Contains(Incident.IsEscalated) || incident.Attributes.Contains(Incident.JarvisEscalationMainCategory)
                       || incident.Attributes.Contains(Incident.JarvisEscalationRemarks) || incident.Attributes.Contains(Incident.JarvisEscalationSubcategory))
                        {
                            tracingService.Trace("Entered Create Case Escalation/De-Escalation.");
                            CaseEscalationProcess operations = new CaseEscalationProcess();
                            operations.CreateEscalationProcess(incident, incidentRetrieve, service, tracingService);
                            tracingService.Trace("Completed Create Case Escalation/De-Escalation.");
                        }
#pragma warning restore SA1123 // Do not place regions within elements
                        #endregion
                    }
#pragma warning restore SA1123 // Do not place regions within elements
                    #endregion
                }
                catch (InvalidPluginExecutionException pex)
                {
                    tracingService.Trace(pex.Message);
                    tracingService.Trace(pex.StackTrace);
                    throw new InvalidPluginExecutionException(pex.Message);
                }
                finally
                {
                    tracingService.Trace("End CasePostOperationSync:" + DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));

                    // No Error Message Throwing Implictly to avoid blocking user because of integration.
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
        /// Update Incident Nature Sub grid.
        /// </summary>
        /// <param name="incidentRetrieve">incident Retrieve.</param>
        /// <param name="stageName">stage Name.</param>
        /// <param name="service">Org service.</param>
        /// <param name="tracingService">tracing Service.</param>
        internal void UpdateIncidentNatureSubgrid(Entity incidentRetrieve, string stageName, IOrganizationService service, ITracingService tracingService)
        {
            if (incidentRetrieve.Attributes.Contains("jarvis_incidentnature") && incidentRetrieve.Attributes["jarvis_incidentnature"] != null)
            {
                Incidentnature incidentnature = new Incidentnature();
                incidentnature.AssociateIncidentNaturetoCase(incidentRetrieve, service, stageName);
            }
        }

        /// <summary>
        /// Send Notification.
        /// </summary>
        /// <param name="initiatingUserID">initiating User ID.</param>
        /// <param name="adminService">admin Service.</param>
        /// <param name="incident">incident details.</param>
        /// <param name="body">body details.</param>
        internal void SendNotification(Guid initiatingUserID, IOrganizationService adminService, Entity incident, string body)
        {
            bool notifyAll = true;
            CaseNotifiaction casenotifiaction = new CaseNotifiaction();
            casenotifiaction.FrameNotifiaction(initiatingUserID, adminService, incident.Id, Guid.NewGuid(), Constants.NotificationData.StatusChanged, body, notifyAll);
        }

        /// <summary>
        /// Set RD HD.
        /// </summary>
        /// <param name="service">Org service.</param>
        /// <param name="incident">incident details.</param>
        internal void SetRDHD(IOrganizationService service, Entity incident)
        {
            EntityCollection passoutRd = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getPassoutRD, incident.Id)));
            Entity updateCase = new Entity(Constants.Incident.IncidentValue);
            if (passoutRd.Entities.Count > 1 && passoutRd.Entities.First().Contains(Constants.Incident.JarvisHDRD) && (bool)passoutRd.Entities.First().Attributes[Constants.Incident.JarvisHDRD])
            {
                updateCase.Attributes[Constants.Incident.IncidentId] = incident.Id;
                updateCase.Attributes[Constants.Incident.JarvisHDRD] = false;
            }
            else if (passoutRd.Entities.Count == 1 && passoutRd.Entities.First().Contains(Constants.Incident.CaseRDjarvisRepairingdealer) && passoutRd.Entities.First().Contains(Constants.Incident.JarvisHomedealer))
            {
                if (((EntityReference)passoutRd.Entities.First().Attributes[Constants.Incident.JarvisHomedealer]).Id == ((EntityReference)((AliasedValue)passoutRd.Entities.First().Attributes[Constants.Incident.CaseRDjarvisRepairingdealer]).Value).Id &&
                    (!(bool)passoutRd.Entities.First().Attributes[Constants.Incident.JarvisHDRD] || !passoutRd.Entities.First().Contains(Constants.Incident.JarvisHDRD)))
                {
                    updateCase.Attributes[Constants.Incident.IncidentId] = incident.Id;
                    updateCase.Attributes[Constants.Incident.JarvisHDRD] = true;
                }
                else if (((EntityReference)passoutRd.Entities.First().Attributes[Constants.Incident.JarvisHomedealer]).Id != ((EntityReference)((AliasedValue)passoutRd.Entities.First().Attributes[Constants.Incident.CaseRDjarvisRepairingdealer]).Value).Id &&
                    passoutRd.Entities.First().Contains(Constants.Incident.JarvisHDRD) && (bool)passoutRd.Entities.First().Attributes[Constants.Incident.JarvisHDRD])
                {
                    updateCase.Attributes[Constants.Incident.IncidentId] = incident.Id;
                    updateCase.Attributes[Constants.Incident.JarvisHDRD] = false;
                }
            }
            else
            {
                if (passoutRd.Entities.First().Contains(Constants.Incident.JarvisHDRD) && (bool)passoutRd.Entities.First().Attributes[Constants.Incident.JarvisHDRD])
                {
                    updateCase.Attributes[Constants.Incident.IncidentId] = incident.Id;
                    updateCase.Attributes[Constants.Incident.JarvisHDRD] = false;
                }
            }

            if (updateCase.Attributes.Count > 0)
            {
                service.Update(updateCase);
            }
        }
    }
}