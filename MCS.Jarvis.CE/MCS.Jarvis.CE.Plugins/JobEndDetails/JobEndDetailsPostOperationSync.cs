// <copyright file="JobEndDetailsPostOperationSync.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MCS.Jarvis.CE.Plugins
{
    using System;
    using global::Plugins;
    using MCS.Jarvis.CE.BusinessProcessesShared;
    using MCS.Jarvis.CE.BusinessProcessesShared.Helpers;
    using MCS.Jarvis.CE.BusinessProcessShared.CaseMonitor;
    using MCS.jarvis.CE.BusinessProcessShared.TranslationProcess;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;
    using Newtonsoft.Json.Linq;
    using Newtonsoft.Json;
    using System.Linq;

    /// <summary>
    /// Job End Details Post Operation Sync.
    /// </summary>
    public class JobEndDetailsPostOperationSync : PluginBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JobEndDetailsPostOperationSync"/> class.
        /// </summary>
        public JobEndDetailsPostOperationSync()
            : base(typeof(JobEndDetailsPostOperationSync))
        {
        }

        /// <summary>
        /// Execute CRM Plugin.
        /// </summary>
        /// <param name="localcontext">local context.</param>
        /// <exception cref="InvalidPluginExecutionException">Invalid Plugin Execution Exception.</exception>
        public override void ExecuteCrmPlugin(LocalPluginContext localcontext)
        {
            // Extract the tracing service for use in debugging sandboxed plug-ins.
            ITracingService tracingService =
                localcontext.TracingService;

            // Obtain the execution context from the service provider.
            IPluginExecutionContext context = localcontext.PluginExecutionContext;
            string incidentStringArray = string.Empty;
            JArray incidentnature = new JArray();
            JArray incidentnature2 = new JArray();
            string incidentStringArray2 = string.Empty;
            string incShadow = string.Empty;
            Guid towingId = new Guid();
            var incnatureObj = new JObject();
            var incnatureObj2 = new JObject();
            var incidentNatureReferences = new EntityReferenceCollection();



            try
            {
                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                {
                    Entity jobEndDetail = (Entity)context.InputParameters["Target"];

                    // region Create
                    if (context.Stage == 40 && context.MessageName.ToUpper() == "CREATE")
                    {
                        IOrganizationService service = localcontext.OrganizationService;
                        IOrganizationService adminservice = localcontext.AdminOrganizationService;
                        if (jobEndDetail.Attributes.Contains("jarvis_incident") && jobEndDetail.Attributes["jarvis_incident"] != null)
                        {
                            EntityReference incident = (EntityReference)jobEndDetail.Attributes["jarvis_incident"];
                            Entity parentCase = new Entity(incident.LogicalName);
                            parentCase.Id = incident.Id;
                            bool customerContactHasMobileorEmail = false;
                            Entity caseEntity = service.Retrieve(incident.LogicalName, incident.Id, new ColumnSet("statuscode", "customerid", "jarvis_atc", "jarvis_onetimecustomercountry", "jarvis_onetimecustomerlanguage", "jarvis_incidentnature", "jarvis_incidentnatureshadow"));
                            bool updateCaseStatus = false;
                            if (jobEndDetail.Attributes.Contains("jarvis_actualcausefault") && jobEndDetail.Attributes["jarvis_actualcausefault"] != null)
                            {
                                string actualCase = (string)jobEndDetail.Attributes["jarvis_actualcausefault"];
                                parentCase["jarvis_actualcausefault"] = actualCase;
                                updateCaseStatus = true;
                            }

                            if (jobEndDetail.Attributes.Contains("jarvis_mileage") && jobEndDetail.Attributes["jarvis_mileage"] != null)
                            {
                                decimal milage = (decimal)jobEndDetail.Attributes["jarvis_mileage"];
                                parentCase["jarvis_mileage"] = Convert.ToInt32(milage);
                                parentCase["jarvis_mileageafterrepair"] = Convert.ToInt32(milage);
                                updateCaseStatus = true;
                            }

                            if (jobEndDetail.Attributes.Contains("jarvis_mileageunit") && jobEndDetail.Attributes["jarvis_mileageunit"] != null)
                            {
                                EntityReference milUnit = (EntityReference)jobEndDetail.Attributes["jarvis_mileageunit"];
                                parentCase["jarvis_mileageunit"] = milUnit;
                                parentCase["jarvis_mileageunitafterrepair"] = milUnit;
                                updateCaseStatus = true;
                            }

                            tracingService.Trace("updateCaseStatus : " + updateCaseStatus.ToString() + " ");

                            // region JED Monitor Action_US_248390_5.3.5_
                            // get CustomerContact
                            tracingService.Trace("Inside PASS JED Monitor Action");
                            EntityCollection getContactwithMobileandEmail = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.caseContactHasMobileAndEmail, caseEntity.Id)));
                            if (getContactwithMobileandEmail.Entities.Count > 0)
                            {
                                customerContactHasMobileorEmail = true;
                                tracingService.Trace("customer has mobile count:" + getContactwithMobileandEmail.Entities.Count);
                            }

                            tracingService.Trace("customerContactHasMobile:" + customerContactHasMobileorEmail);
                            if (!customerContactHasMobileorEmail && jobEndDetail.Attributes.Contains("jarvis_actualcausefault") && jobEndDetail.Attributes["jarvis_actualcausefault"] != null)
                            {
                                tracingService.Trace("Entered MA Pass JED");

                                // region Monitor Creation - ATC
                                CaseMonitorProcess operations = new CaseMonitorProcess();
                                string isCountryCode = string.Empty;
                                string isoLangCode = string.Empty;

                                if (caseEntity.Attributes.Contains("jarvis_onetimecustomercountry") && caseEntity.Attributes["jarvis_onetimecustomercountry"] != null)
                                {
                                    EntityReference hdcountry = (EntityReference)caseEntity.Attributes["jarvis_onetimecustomercountry"];
                                    Entity country = service.Retrieve(hdcountry.LogicalName, hdcountry.Id, new ColumnSet("jarvis_iso2countrycode"));
                                    if (country.Attributes.Contains("jarvis_iso2countrycode") && country.Attributes["jarvis_iso2countrycode"] != null)
                                    {
                                        isCountryCode = (string)country.Attributes["jarvis_iso2countrycode"];
                                    }
                                }

                                if (caseEntity.Attributes.Contains("jarvis_onetimecustomerlanguage") && caseEntity.Attributes["jarvis_onetimecustomerlanguage"] != null)
                                {
                                    EntityReference language = (EntityReference)caseEntity.Attributes["jarvis_onetimecustomerlanguage"];
                                    Entity hdlanguage = service.Retrieve(language.LogicalName, language.Id, new ColumnSet("jarvis_iso3languagecode6392t"));
                                    if (hdlanguage.Attributes.Contains("jarvis_iso3languagecode6392t") && hdlanguage.Attributes["jarvis_iso3languagecode6392t"] != null)
                                    {
                                        isoLangCode = (string)hdlanguage.Attributes["jarvis_iso3languagecode6392t"];
                                    }
                                }

                                if (!string.IsNullOrEmpty(isoLangCode) || !string.IsNullOrEmpty(isCountryCode))
                                {
                                    tracingService.Trace("Creating MA Pass JED");
                                    string fucomment = isoLangCode + " " + isCountryCode + " " + "Pass JED";
                                    operations.AutomateMonitorCreationTime(caseEntity, fucomment, 2, 16, 0, " ", DateTime.UtcNow, 0, service);
                                }

                                // endregion
                            }

                            // region US335230 Close MA 5.13.1.2
                            tracingService.Trace("inside JED creation and  close Chase JEDS ");
                            CaseMonitorProcess operationJED = new CaseMonitorProcess();
                            string fucomments = "Chase JEDS";
                            operationJED.AutomateCloseMonitorActions(parentCase, fucomments, 1, fucomments, service);

                            // endregion
                            // region US 568834 Close MA Chase Diagnose
                            if (caseEntity.Attributes.Contains("jarvis_atc") && caseEntity.Attributes["jarvis_atc"] != null)
                            {
                                tracingService.Trace("inside ata has value and JED created hence close Chase Diagnose ");
                                CaseMonitorProcess operation = new CaseMonitorProcess();
                                string fucomment = "Chase Diagnose";
                                operation.AutomateCloseMonitorActions(caseEntity, fucomment, 1, fucomment, service);
                            }

                            // endregion

                            // endregion
                            // region Set Case Status for Movement
                            if (updateCaseStatus)
                            {
                                bool isAutomate = false;
                                isAutomate = CrmHelper.GetAutomationConfig(service, MCS.Jarvis.CE.Plugins.Constants.JarvisConfiguration.Automationcasestatuschange, tracingService);
                                if (isAutomate)
                                {
                                    if (caseEntity.Attributes.Contains("statuscode") && caseEntity.Attributes["statuscode"] != null)
                                    {
                                        tracingService.Trace("Inside update of Case");
                                        OptionSetValue status = (OptionSetValue)caseEntity.Attributes["statuscode"];
                                        if (status.Value == 80)
                                        {
                                            // Repair Summary
                                            parentCase["statuscode"] = new OptionSetValue(90); // Case Closure
                                            parentCase["jarvis_mercuriusstatus"] = new OptionSetValue(800);
                                            tracingService.Trace("Before update of Case");
                                        }
                                    }
                                }

                                service.Update(parentCase);
                            }

                            // endregion
                            // region JED Translations
                            bool isAutomation = false;
                            isAutomation = CrmHelper.GetAutomationConfig(service, MCS.Jarvis.CE.Plugins.Constants.JarvisConfiguration.Automationtranslation, tracingService);
                            if (isAutomation)
                            {
                                if (jobEndDetail.Attributes.Contains("jarvis_actualcausefault") || jobEndDetail.Attributes.Contains("jarvis_temporaryrepair"))
                                {
                                    TranslationProcess operation = new TranslationProcess();
                                    operation.JEDStandardProcess(adminservice, tracingService, new Entity(incident.LogicalName, incident.Id), context.InitiatingUserId, jobEndDetail);
                                    Entity caseUpdate = service.Retrieve(jobEndDetail.LogicalName, jobEndDetail.Id, new ColumnSet("jarvis_translationstatusactualcausefault", "jarvis_translationstatustemporaryrepair"));
                                    Entity caseToUpdate = new Entity(jobEndDetail.LogicalName);
                                    caseToUpdate.Id = jobEndDetail.Id;
                                    bool isUpdate = false;
                                    if (caseUpdate.Attributes.Contains("jarvis_translationstatusactualcausefault") && caseUpdate.Attributes["jarvis_translationstatusactualcausefault"] != null && jobEndDetail.Attributes.Contains("jarvis_actualcausefault"))
                                    {
                                        OptionSetValue translationStatusRF = (OptionSetValue)caseUpdate.Attributes["jarvis_translationstatusactualcausefault"];
                                        if (translationStatusRF.Value != 334030001)
                                        {
                                            // In-Progress
                                            caseToUpdate["jarvis_translationstatusactualcausefault"] = new OptionSetValue(334030001);
                                            isUpdate = true;
                                        }
                                    }

                                    OptionSetValue translationStatusLoc = (OptionSetValue)caseUpdate.Attributes["jarvis_translationstatustemporaryrepair"];
                                    if (translationStatusLoc.Value != 334030001 && jobEndDetail.Attributes.Contains("jarvis_temporaryrepair"))
                                    {
                                        // In-Progress
                                        caseToUpdate["jarvis_translationstatustemporaryrepair"] = new OptionSetValue(334030001);
                                        isUpdate = true;
                                    }

                                    if (isUpdate)
                                    {
                                        service.Update(caseToUpdate);
                                    }
                                }
                            }
                            #region UpdateIncidentNature-JedsCreate
                            if (jobEndDetail.GetAttribute‌​‌​Value<bool>("jarvis_towing") == true)
                            {
                                EntityCollection getAllExistingjedsCollection = service.RetrieveMultiple(new FetchExpression(string.Format(MCS.Jarvis.CE.Plugins.Constants.FetchXmls.getAllExistingjedsRecords, caseEntity.Id)));
                                if (getAllExistingjedsCollection.Entities.Count > 0)
                                {
                                    tracingService.Trace("Count of Jeds Records are" + getAllExistingjedsCollection.Entities.Count);
                                    foreach (Entity jedsRecords in getAllExistingjedsCollection.Entities)
                                    {
                                        if (jedsRecords.Id != jobEndDetail.Id)
                                        {
                                            Entity jedsRecord = new Entity("jarvis_jobenddetails");
                                            jedsRecord.Id = jedsRecords.Id;
                                            jedsRecord["jarvis_towing"] = true;
                                            service.Update(jedsRecord);
                                        }
                                    }
                                    Guid incidentnatid = Guid.NewGuid();
                                    if (caseEntity.Attributes.Contains("jarvis_incidentnatureshadow") && caseEntity.Attributes["jarvis_incidentnatureshadow"] != null)
                                    {
                                        fetchandUpdateIncidentNatureMultiselect(caseEntity, tracingService, service);
                                    }
                                    else
                                    {
                                        UpdateIncidentNatureMultiselect(caseEntity, tracingService, service);
                                    }
                                }
                            }
                            #endregion

                            // endregion
                        }
                    }

                    // endregion
                    // region Update
                    if (context.Stage == 40 && context.MessageName.ToUpper() == "UPDATE")
                    {
                        if (context.Depth == 1)
                        {
                            IOrganizationService service = localcontext.OrganizationService;
                            IOrganizationService adminservice = localcontext.AdminOrganizationService;
                            bool isUpdate = false;
                            bool customerContactHasMobileorEmail = false;
                            Entity jobEndDetailImg = (Entity)context.PostEntityImages["PostImage"];
                            if (jobEndDetailImg.Attributes.Contains("jarvis_incident") && jobEndDetailImg.Attributes["jarvis_incident"] != null)
                            {
                                EntityReference incident = (EntityReference)jobEndDetailImg.Attributes["jarvis_incident"];
                                Entity parentCase = new Entity(incident.LogicalName);
                                parentCase.Id = incident.Id;
                                Entity caseEntity = service.Retrieve(incident.LogicalName, incident.Id, new ColumnSet("statuscode", "customerid", "jarvis_onetimecustomercountry", "jarvis_onetimecustomerlanguage", "jarvis_incidentnature", "jarvis_incidentnatureshadow"));

                                // Set case -customer Informed
                                //if (jobEndDetail.Attributes.Contains("statuscode") && jobEndDetail.Attributes["statuscode"] != null)
                                //{
                                //    OptionSetValue jedstatus = (OptionSetValue)jobEndDetail.Attributes["statuscode"];
                                //    if (jedstatus.Value == 334030002)
                                //    {
                                //        // Has Been Sent
                                //        if (caseEntity.Attributes.Contains("statuscode") && caseEntity.Attributes["statuscode"] != null)
                                //        {
                                //            OptionSetValue status = (OptionSetValue)caseEntity.Attributes["statuscode"];
                                //            if (status.Value == 90)
                                //            {
                                //                // Case closure
                                //                CaseOperations caseOperationsSet = new CaseOperations();
                                //                OptionSetValue releaseConfig = new OptionSetValue();
                                //                releaseConfig = CrmHelper.GetReleaseAutomationConfig(service, MCS.Jarvis.CE.Plugins.Constants.JarvisConfiguration.Automationreleasecase, tracingService);
                                //                caseOperationsSet.SetCustomerInformed(service, tracingService, parentCase, status, releaseConfig.Value);
                                //                isUpdate = true;
                                //            }
                                //        }
                                //    }
                                //}

                                if (jobEndDetailImg.Attributes.Contains("jarvis_actualcausefault") && jobEndDetailImg.Attributes["jarvis_actualcausefault"] != null)
                                {
                                    string actualCase = (string)jobEndDetailImg.Attributes["jarvis_actualcausefault"];
                                    parentCase["jarvis_actualcausefault"] = actualCase;
                                    isUpdate = true;
                                }

                                if (jobEndDetailImg.Attributes.Contains("jarvis_mileage") && jobEndDetailImg.Attributes["jarvis_mileage"] != null)
                                {
                                    isUpdate = true;
                                    decimal milage = (decimal)jobEndDetailImg.Attributes["jarvis_mileage"];
                                    parentCase["jarvis_mileage"] = Convert.ToInt32(milage);
                                    parentCase["jarvis_mileageafterrepair"] = Convert.ToInt32(milage);
                                }

                                if (jobEndDetailImg.Attributes.Contains("jarvis_mileageunit") && jobEndDetailImg.Attributes["jarvis_mileageunit"] != null)
                                {
                                    isUpdate = true;
                                    EntityReference milUnit = (EntityReference)jobEndDetailImg.Attributes["jarvis_mileageunit"];
                                    parentCase["jarvis_mileageunit"] = milUnit;
                                    parentCase["jarvis_mileageunitafterrepair"] = milUnit;
                                }

                                if (isUpdate)
                                {
                                    parentCase.Id = incident.Id;

                                    // region Set Case Status for Movement
                                    bool isAutomate = false;
                                    isAutomate = CrmHelper.GetAutomationConfig(service, MCS.Jarvis.CE.Plugins.Constants.JarvisConfiguration.Automationcasestatuschange, tracingService);
                                    if (isAutomate)
                                    {
                                        if (caseEntity.Attributes.Contains("statuscode") && caseEntity.Attributes["statuscode"] != null)
                                        {
                                            OptionSetValue status = (OptionSetValue)caseEntity.Attributes["statuscode"];
                                            if (status.Value == 80)
                                            {
                                                // Repair Summary
                                                parentCase["statuscode"] = new OptionSetValue(90); // Case Closure
                                                parentCase["jarvis_mercuriusstatus"] = new OptionSetValue(800); // Case Closure
                                            }
                                        }
                                    }

                                    // endregion
                                    service.Update(parentCase);
                                }

                                // region JED Monitor Action_US_248390_5.3.5
                                if (jobEndDetail.Attributes.Contains("jarvis_actualcausefault") && jobEndDetail.Attributes["jarvis_actualcausefault"] != null)
                                {
                                    EntityCollection getContactwithMobileandEmail = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.caseContactHasMobileAndEmail, caseEntity.Id)));
                                    if (getContactwithMobileandEmail.Entities.Count > 0)
                                    {
                                        customerContactHasMobileorEmail = true;
                                        tracingService.Trace("customer has mobile count:" + getContactwithMobileandEmail.Entities.Count);
                                    }

                                    tracingService.Trace("customerContactHasMobile:" + customerContactHasMobileorEmail);
                                    if (!customerContactHasMobileorEmail)
                                    {
                                        tracingService.Trace("Entered MA Pass JED");

                                        // region Monitor Creation - ATC
                                        CaseMonitorProcess operations = new CaseMonitorProcess();
                                        string isCountryCode = string.Empty;
                                        string isoLangCode = string.Empty;
                                        if (caseEntity.Attributes.Contains("jarvis_onetimecustomercountry") && caseEntity.Attributes["jarvis_onetimecustomercountry"] != null)
                                        {
                                            EntityReference hdcountry = (EntityReference)caseEntity.Attributes["jarvis_onetimecustomercountry"];
                                            Entity country = service.Retrieve(hdcountry.LogicalName, hdcountry.Id, new ColumnSet("jarvis_iso2countrycode"));
                                            if (country.Attributes.Contains("jarvis_iso2countrycode") && country.Attributes["jarvis_iso2countrycode"] != null)
                                            {
                                                isCountryCode = (string)country.Attributes["jarvis_iso2countrycode"];
                                            }
                                        }

                                        if (caseEntity.Attributes.Contains("jarvis_onetimecustomerlanguage") && caseEntity.Attributes["jarvis_onetimecustomerlanguage"] != null)
                                        {
                                            EntityReference language = (EntityReference)caseEntity.Attributes["jarvis_onetimecustomerlanguage"];
                                            Entity hdlanguage = service.Retrieve(language.LogicalName, language.Id, new ColumnSet("jarvis_iso3languagecode6392t"));
                                            if (hdlanguage.Attributes.Contains("jarvis_iso3languagecode6392t") && hdlanguage.Attributes["jarvis_iso3languagecode6392t"] != null)
                                            {
                                                isoLangCode = (string)hdlanguage.Attributes["jarvis_iso3languagecode6392t"];
                                            }
                                        }

                                        if (!string.IsNullOrEmpty(isoLangCode) || !string.IsNullOrEmpty(isCountryCode))
                                        {
                                            tracingService.Trace("Creating MA Pass JED");
                                            string fucomment = isoLangCode + " " + isCountryCode + " " + "Pass JED";
                                            operations.AutomateMonitorCreationTime(caseEntity, fucomment, 2, 16, 0, " ", DateTime.UtcNow, 0, service);
                                        }
                                    }
                                }

                                // endregion

                                // endregion

                                // region JED Translations
                                bool isAutomation = false;
                                isAutomation = CrmHelper.GetAutomationConfig(service, MCS.Jarvis.CE.Plugins.Constants.JarvisConfiguration.Automationtranslation, tracingService);
                                if (isAutomation)
                                {
                                    if (jobEndDetail.Attributes.Contains("jarvis_actualcausefault") || jobEndDetail.Attributes.Contains("jarvis_temporaryrepair"))
                                    {
                                        TranslationProcess operation = new TranslationProcess();
                                        operation.JEDStandardProcess(adminservice, tracingService, new Entity(incident.LogicalName, incident.Id), context.InitiatingUserId, jobEndDetail);
                                        Entity caseUpdate = service.Retrieve(jobEndDetail.LogicalName, jobEndDetail.Id, new ColumnSet("jarvis_translationstatusactualcausefault", "jarvis_translationstatustemporaryrepair"));
                                        Entity caseToUpdate = new Entity(jobEndDetail.LogicalName);
                                        caseToUpdate.Id = jobEndDetail.Id;
                                        bool update = false;
                                        if (caseUpdate.Attributes.Contains("jarvis_translationstatusactualcausefault") && caseUpdate.Attributes["jarvis_translationstatusactualcausefault"] != null)
                                        {
                                            OptionSetValue translationStatusRF = (OptionSetValue)caseUpdate.Attributes["jarvis_translationstatusactualcausefault"];
                                            if (translationStatusRF.Value != 334030001 && jobEndDetail.Attributes.Contains("jarvis_actualcausefault"))
                                            {
                                                // In-Progress
                                                caseToUpdate["jarvis_translationstatusactualcausefault"] = new OptionSetValue(334030001);
                                                update = true;
                                            }
                                        }

                                        OptionSetValue translationStatusLoc = (OptionSetValue)caseUpdate.Attributes["jarvis_translationstatustemporaryrepair"];
                                        if (translationStatusLoc.Value != 334030001 && jobEndDetail.Attributes.Contains("jarvis_temporaryrepair"))
                                        {
                                            // In-Progress
                                            caseToUpdate["jarvis_translationstatustemporaryrepair"] = new OptionSetValue(334030001);
                                            update = true;
                                        }

                                        if (update)
                                        {
                                            service.Update(caseToUpdate);
                                        }
                                    }
                                }
                                #region UpdateIncidentNature-JedsUpdate
                                if (jobEndDetail.GetAttribute‌​‌​Value<bool>("jarvis_towing") == true)
                                {
                                    EntityCollection getAllExistingjedsCollection = service.RetrieveMultiple(new FetchExpression(string.Format(MCS.Jarvis.CE.Plugins.Constants.FetchXmls.getAllExistingjedsRecords, caseEntity.Id)));
                                    if (getAllExistingjedsCollection.Entities.Count > 0)
                                    {
                                        tracingService.Trace("Count of Jeds Records are" + getAllExistingjedsCollection.Entities.Count);
                                        foreach (Entity jedsRecords in getAllExistingjedsCollection.Entities)
                                        {
                                            if (jedsRecords.Id != jobEndDetail.Id)
                                            {
                                                Entity jedsRecord = new Entity("jarvis_jobenddetails");
                                                jedsRecord.Id = jedsRecords.Id;
                                                jedsRecord["jarvis_towing"] = true;
                                                service.Update(jedsRecord);
                                            }
                                        }
                                        tracingService.Trace("Jeds Records are updated with Towing=true foreach");
                                        Guid incidentnatid = Guid.NewGuid();
                                        var incidentNaturesMultiselect = JsonConvert.DeserializeObject(caseEntity.Attributes["jarvis_incidentnature"].ToString());
                                        if (incidentNaturesMultiselect != null)
                                        {
                                            if (caseEntity.Attributes["jarvis_incidentnatureshadow"] != null && caseEntity.Attributes["jarvis_incidentnatureshadow"].ToString().Contains("Towing"))
                                            { }
                                            else
                                            {
                                                foreach (JObject nature in (Newtonsoft.Json.Linq.JArray)incidentNaturesMultiselect)
                                                {
                                                    if (nature.TryGetValue("_id", StringComparison.OrdinalIgnoreCase, out JToken incidentNatureId) && incidentNatureId != null)
                                                    {
                                                        EntityReference incidentNatureObj = new EntityReference(Constants.IncidentNature.jarvisIncidentNature, (Guid)incidentNatureId);
                                                        incidentNatureReferences.Add(incidentNatureObj);
                                                        incidentnatid = (Guid)incidentNatureId;
                                                    }
                                                    else if (nature.TryGetValue("id", StringComparison.OrdinalIgnoreCase, out JToken incidentNatureId_id) && incidentNatureId_id != null)
                                                    {
                                                        EntityReference incidentNatureObj = new EntityReference(Constants.IncidentNature.jarvisIncidentNature, (Guid)incidentNatureId_id);
                                                        incidentNatureReferences.Add(incidentNatureObj);
                                                        incidentnatid = (Guid)incidentNatureId_id;
                                                    }
                                                    if (nature.TryGetValue("_name", StringComparison.OrdinalIgnoreCase, out JToken name) && name != null)
                                                    {
                                                        var incnatureObj1 = new JObject();
                                                        incnatureObj1 = new JObject();
                                                        incnatureObj1.Add("id", incidentnatid);
                                                        incnatureObj1.Add("entityType", "jarvis_incidentnature");
                                                        incnatureObj1.Add("name", name);
                                                        incidentStringArray = incidentStringArray + name + ",";
                                                        incidentnature.Add(incnatureObj1);
                                                    }
                                                    if (nature.TryGetValue("name", StringComparison.OrdinalIgnoreCase, out JToken name_IN) && name_IN != null)
                                                    {
                                                        var incnatureObj1 = new JObject();
                                                        incnatureObj1.Add("id", incidentnatid);
                                                        incnatureObj1.Add("entityType", "jarvis_incidentnature");
                                                        incnatureObj1.Add("name", name_IN);
                                                        incidentStringArray = incidentStringArray + name_IN + ",";
                                                        incidentnature.Add(incnatureObj1);
                                                    }
                                                }
                                                tracingService.Trace("Incident Nature Object is " + incidentnature);
                                                Entity incidentNatureUpdate = new Entity("incident");
                                                string incidentNatureToAdd = "Towing";
                                                EntityCollection getTowingRecordInfoCollection = service.RetrieveMultiple(new FetchExpression(string.Format(MCS.Jarvis.CE.Plugins.Constants.FetchXmls.getTowingRecordInfo)));
                                                if (getTowingRecordInfoCollection.Entities.Count == 1)
                                                {
                                                    towingId = getTowingRecordInfoCollection.Entities[0].Id;
                                                }
                                                EntityReference incidentNatureId1 = new EntityReference(Constants.IncidentNature.jarvisIncidentNature, towingId);
                                                incnatureObj2.Add("id", incidentNatureId1.Id);
                                                incnatureObj2.Add("entityType", "jarvis_incidentnature");
                                                incnatureObj2.Add("name", incidentNatureToAdd);
                                                incidentnature2.Add(incnatureObj2);
                                                incidentnature.Merge(incidentnature2);
                                                incidentStringArray = incidentnature.ToString();
                                                var jsonData = JsonConvert.DeserializeObject(incidentStringArray);
                                                tracingService.Trace("combined JSON Data 1 is " + jsonData);
                                                string trimmedJsonString = JsonConvert.SerializeObject(jsonData, Formatting.None);
                                                incidentNatureUpdate["jarvis_incidentnature"] = trimmedJsonString;
                                                incidentNatureUpdate.Id = incident.Id;
                                                service.Update(incidentNatureUpdate);
                                            }
                                        }

                                    }
                                }
                                else if (jobEndDetail.GetAttributeValue<bool>("jarvis_towing") == false && caseEntity.Attributes["jarvis_incidentnatureshadow"] != null && caseEntity.Attributes["jarvis_incidentnatureshadow"].ToString().Contains("Towing"))
                                {
                                    bool isTowingFalseinanyExistingRepairInfo = true;
                                    bool towingValueinExistingRecords = true;
                                    tracingService.Trace("Execution coming inside Towing condition");
                                    EntityCollection getAllExistingJEDsInfoRecordsCollection = service.RetrieveMultiple(new FetchExpression(string.Format(MCS.Jarvis.CE.Plugins.Constants.FetchXmls.getAllExistingjedsRecords, caseEntity.Id)));
                                    if (getAllExistingJEDsInfoRecordsCollection.Entities.Count > 0)
                                    {
                                        tracingService.Trace("Count of Repair Info Records are" + getAllExistingJEDsInfoRecordsCollection.Entities.Count);
                                        foreach (Entity JEDsInfoRecords in getAllExistingJEDsInfoRecordsCollection.Entities)
                                        {
                                            if (jobEndDetail.Id != JEDsInfoRecords.Id)
                                            {
                                                towingValueinExistingRecords = JEDsInfoRecords.GetAttributeValue<bool>("jarvis_towing");
                                                // check if Towing is set to True in any of the Record
                                                if (towingValueinExistingRecords)
                                                {
                                                    isTowingFalseinanyExistingRepairInfo = false;
                                                    break;
                                                }
                                                else
                                                {
                                                    isTowingFalseinanyExistingRepairInfo = true;
                                                }
                                            }
                                        }
                                        if (isTowingFalseinanyExistingRepairInfo == true)
                                        {
                                            tracingService.Trace("coming till here 0");
                                            EntityCollection getTowingRecordInfoCollection = service.RetrieveMultiple(new FetchExpression(string.Format(MCS.Jarvis.CE.Plugins.Constants.FetchXmls.getTowingRecordInfo)));
                                            if (getTowingRecordInfoCollection.Entities.Count == 1)
                                            {
                                                towingId = getTowingRecordInfoCollection.Entities[0].Id;
                                            }
                                            string incidentNatureToAdd = "Towing";
                                            #region tryapproach2
                                            tracingService.Trace("coming inside remove inc nat approach 2 condition");
                                            Guid incidentnatid = Guid.NewGuid();
                                            var incidentNaturesMultiselect = JsonConvert.DeserializeObject(caseEntity.Attributes["jarvis_incidentnature"].ToString());
                                            if (incidentNaturesMultiselect != null)
                                            {
                                                foreach (JObject nature in (Newtonsoft.Json.Linq.JArray)incidentNaturesMultiselect)
                                                {
                                                    if (nature.TryGetValue("_id", StringComparison.OrdinalIgnoreCase, out JToken incidentNatureId) && incidentNatureId != null)
                                                    {
                                                        EntityReference incidentNatureObj = new EntityReference(Constants.IncidentNature.jarvisIncidentNature, (Guid)incidentNatureId);
                                                        incidentNatureReferences.Add(incidentNatureObj);
                                                        incidentnatid = (Guid)incidentNatureId;
                                                    }
                                                    else if (nature.TryGetValue("id", StringComparison.OrdinalIgnoreCase, out JToken incidentNatureId_id) && incidentNatureId_id != null)
                                                    {
                                                        EntityReference incidentNatureObj = new EntityReference(Constants.IncidentNature.jarvisIncidentNature, (Guid)incidentNatureId_id);
                                                        incidentNatureReferences.Add(incidentNatureObj);
                                                        incidentnatid = (Guid)incidentNatureId_id;
                                                    }
                                                    if (nature.TryGetValue("_name", StringComparison.OrdinalIgnoreCase, out JToken name) && name != null)
                                                    {
                                                        var incnatureObj1 = new JObject();
                                                        incnatureObj1.Add("id", incidentnatid);
                                                        incnatureObj1.Add("entityType", "jarvis_incidentnature");
                                                        incnatureObj1.Add("name", name);
                                                        incidentStringArray = incidentStringArray + name + ",";
                                                        incidentnature.Add(incnatureObj1);
                                                    }
                                                    if (nature.TryGetValue("name", StringComparison.OrdinalIgnoreCase, out JToken name_IN) && name_IN != null)
                                                    {
                                                        var incnatureObj1 = new JObject();
                                                        incnatureObj1.Add("id", incidentnatid);
                                                        incnatureObj1.Add("entityType", "jarvis_incidentnature");
                                                        incnatureObj1.Add("name", name_IN);
                                                        incidentStringArray = incidentStringArray + name_IN + ",";
                                                        incidentnature.Add(incnatureObj1);
                                                    }
                                                }
                                                tracingService.Trace("Incident Nature Object is " + incidentnature);
                                                Entity incidentNatureUpdate = new Entity("incident");
                                                incidentNatureToAdd = "Towing";
                                                getTowingRecordInfoCollection = service.RetrieveMultiple(new FetchExpression(string.Format(MCS.Jarvis.CE.Plugins.Constants.FetchXmls.getTowingRecordInfo)));
                                                if (getTowingRecordInfoCollection.Entities.Count == 1)
                                                {
                                                    towingId = getTowingRecordInfoCollection.Entities[0].Id;
                                                }
                                                EntityReference incidentNatureId1 = new EntityReference(Constants.IncidentNature.jarvisIncidentNature, towingId);

                                                incidentStringArray = incidentnature.ToString();
                                                //var removedIncidentStringArray = incidentnature.ToString();
                                                var removedIncidentStringArray = incidentnature.ToString();

                                                // Test Removal Approaches 
                                                var jArray = JArray.Parse(removedIncidentStringArray);
                                                var nametoDetect = "Towing";
                                                //JToken item = jArray.FirstOrDefault(arr => arr.Type == JTokenType.String && arr.Value<string>() == nametoDetect);
                                                //item?.Remove();
                                                JArray filteredJArray = new JArray(jArray.Where(item1 => (string)item1["name"] != "Towing"));

                                                //string filteredJArrayString = jArray.ToString();
                                                string filteredJArrayString = filteredJArray.ToString();


                                                tracingService.Trace("Item received is" + filteredJArray);
                                                tracingService.Trace("Removed Incident Nature is" + removedIncidentStringArray);
                                                var jsonData = JsonConvert.DeserializeObject(filteredJArrayString);


                                                tracingService.Trace("combined JSON Data 1 is " + jsonData);
                                                string trimmedJsonString = JsonConvert.SerializeObject(jsonData, Formatting.None);
                                                incidentNatureUpdate["jarvis_incidentnature"] = trimmedJsonString;
                                                incidentNatureUpdate.Id = incident.Id;
                                                service.Update(incidentNatureUpdate);
                                            }
                                            #endregion
                                        }
                                        #endregion
                                    }
                                }

                                // endregion
                            }
                        }
                    }

                    // endregion
                }
            }
            catch (InvalidPluginExecutionException ex)
            {
                throw new InvalidPluginExecutionException("Error in Job End Detail Operations " + ex.InnerException.Message + " ");
            }
        }

        private void UpdateIncidentNatureMultiselect(Entity caseEntity, ITracingService tracingService, IOrganizationService service)
        {
            string incidentStringArray = string.Empty;
            JArray incidentnature = new JArray();
            JArray incidentnature2 = new JArray();
            string incidentStringArray2 = string.Empty;
            string incShadow = string.Empty;
            Guid towingId = new Guid();
            var incidentNatureReferenceDissociate = new EntityReferenceCollection();
            var incidentNatureReferenceAssociate = new EntityReferenceCollection();
            var incnatureObj = new JObject();
            var incnatureObj2 = new JObject();
            var incnatureObjArray = new JObject();
            var incidentNatureReferences = new EntityReferenceCollection();
            Guid incidentnatid = Guid.NewGuid();
            Entity incidentNatureUpdate = new Entity("incident");
            string incidentNatureToAdd = "Towing";
            EntityCollection getTowingRecordInfoCollection = service.RetrieveMultiple(new FetchExpression(string.Format(MCS.Jarvis.CE.Plugins.Constants.FetchXmls.getTowingRecordInfo)));
            if (getTowingRecordInfoCollection.Entities.Count == 1)
            {
                towingId = getTowingRecordInfoCollection.Entities[0].Id;
            }
            EntityReference incidentNatureId1 = new EntityReference(Constants.IncidentNature.jarvisIncidentNature, towingId);
            incnatureObj2.Add("id", incidentNatureId1.Id);
            incnatureObj2.Add("entityType", "jarvis_incidentnature");
            incnatureObj2.Add("name", incidentNatureToAdd);
            incidentnature2.Add(incnatureObj2);
            incidentnature.Merge(incidentnature2);
            incidentStringArray = incidentnature.ToString();
            var jsonData = JsonConvert.DeserializeObject(incidentStringArray);
            tracingService.Trace("combined JSON Data 1 is " + jsonData);
            string trimmedJsonString = JsonConvert.SerializeObject(jsonData, Formatting.None);
            incidentNatureUpdate["jarvis_incidentnature"] = trimmedJsonString;
            incidentNatureUpdate.Id = caseEntity.Id;
            service.Update(incidentNatureUpdate);
        }

        private void fetchandUpdateIncidentNatureMultiselect(Entity caseEntity, ITracingService tracingService, IOrganizationService service)
        {
            string incidentStringArray = string.Empty;
            JArray incidentnature = new JArray();
            JArray incidentnature2 = new JArray();
            string incidentStringArray2 = string.Empty;
            string incShadow = string.Empty;
            Guid towingId = new Guid();
            var incidentNatureReferenceDissociate = new EntityReferenceCollection();
            var incidentNatureReferenceAssociate = new EntityReferenceCollection();
            var incnatureObj = new JObject();
            var incnatureObj2 = new JObject();
            var incnatureObjArray = new JObject();
            var incidentNatureReferences = new EntityReferenceCollection();
            Guid incidentnatid = Guid.NewGuid();
            if (caseEntity.Attributes["jarvis_incidentnatureshadow"] != null && caseEntity.Attributes["jarvis_incidentnatureshadow"].ToString().Contains("Towing"))
            { }
            else
            {
                var incidentNaturesMultiselect = JsonConvert.DeserializeObject(caseEntity.Attributes["jarvis_incidentnature"].ToString());
                foreach (JObject nature in (Newtonsoft.Json.Linq.JArray)incidentNaturesMultiselect)
                {
                    if (nature.TryGetValue("_id", StringComparison.OrdinalIgnoreCase, out JToken incidentNatureId) && incidentNatureId != null)
                    {
                        EntityReference incidentNatureObj = new EntityReference(Constants.IncidentNature.jarvisIncidentNature, (Guid)incidentNatureId);
                        incidentNatureReferences.Add(incidentNatureObj);
                        incidentnatid = (Guid)incidentNatureId;
                    }
                    else if (nature.TryGetValue("id", StringComparison.OrdinalIgnoreCase, out JToken incidentNatureId_id) && incidentNatureId_id != null)
                    {
                        EntityReference incidentNatureObj = new EntityReference(Constants.IncidentNature.jarvisIncidentNature, (Guid)incidentNatureId_id);
                        incidentNatureReferences.Add(incidentNatureObj);
                        incidentnatid = (Guid)incidentNatureId_id;
                    }
                    if (nature.TryGetValue("_name", StringComparison.OrdinalIgnoreCase, out JToken name) && name != null)
                    {
                        var incnatureObj1 = new JObject();
                        incnatureObj1 = new JObject();
                        incnatureObj1.Add("id", incidentnatid);
                        incnatureObj1.Add("entityType", "jarvis_incidentnature");
                        incnatureObj1.Add("name", name);
                        incidentStringArray = incidentStringArray + name + ",";
                        incidentnature.Add(incnatureObj1);
                    }
                    if (nature.TryGetValue("name", StringComparison.OrdinalIgnoreCase, out JToken name_IN) && name_IN != null)
                    {
                        var incnatureObj1 = new JObject();
                        incnatureObj1.Add("id", incidentnatid);
                        incnatureObj1.Add("entityType", "jarvis_incidentnature");
                        incnatureObj1.Add("name", name_IN);
                        incidentStringArray = incidentStringArray + name_IN + ",";
                        incidentnature.Add(incnatureObj1);
                    }
                }
                Entity incidentNatureUpdate = new Entity("incident");
                string incidentNatureToAdd = "Towing";
                EntityCollection getTowingRecordInfoCollection = service.RetrieveMultiple(new FetchExpression(string.Format(MCS.Jarvis.CE.Plugins.Constants.FetchXmls.getTowingRecordInfo)));
                if (getTowingRecordInfoCollection.Entities.Count == 1)
                {
                    towingId = getTowingRecordInfoCollection.Entities[0].Id;
                }
                EntityReference incidentNatureId1 = new EntityReference(Constants.IncidentNature.jarvisIncidentNature, towingId);
                incnatureObj2.Add("id", incidentNatureId1.Id);
                incnatureObj2.Add("entityType", "jarvis_incidentnature");
                incnatureObj2.Add("name", incidentNatureToAdd);
                incidentnature2.Add(incnatureObj2);
                incidentnature.Merge(incidentnature2);
                incidentStringArray = incidentnature.ToString();
                var jsonData = JsonConvert.DeserializeObject(incidentStringArray);
                tracingService.Trace("combined JSON Data 1 is " + jsonData);
                string trimmedJsonString = JsonConvert.SerializeObject(jsonData, Formatting.None);
                incidentNatureUpdate["jarvis_incidentnature"] = trimmedJsonString;
                incidentNatureUpdate.Id = caseEntity.Id;
                service.Update(incidentNatureUpdate);
            }
        }
    }
}
