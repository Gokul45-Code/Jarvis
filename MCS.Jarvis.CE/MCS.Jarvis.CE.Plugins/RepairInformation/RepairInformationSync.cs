// <Copyright file="RepairInformation.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace MCS.Jarvis.CE.Plugins.RepairInformation
{
    using System;
    using global::Plugins;
    using MCS.Jarvis.CE.BusinessProcessesShared.Helpers;
    using MCS.Jarvis.CE.BusinessProcessShared.CaseMonitor;
    using MCS.jarvis.CE.BusinessProcessShared.TranslationProcess;
    using MCS.Jarvis.CE.BusinessProcessShared.Case;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;
    using Newtonsoft.Json.Linq;
    using Newtonsoft.Json;
    using System.Linq;

    /// <summary>
    /// Repair Information Sync.
    /// </summary>
    public class RepairInformationSync : PluginBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RepairInformationSync"/> class.
        /// RepairInformationSync:Constructor.
        /// </summary>
        public RepairInformationSync()
            : base(typeof(RepairInformationSync))
        {
        }

        /// <summary>
        /// Execute Plugin Logic for RepairInformationSync.
        /// </summary>
        /// <param name="localcontext">local Context.</param>
        /// <exception cref="InvalidPluginExecutionException">Plugin Exception.</exception>
        public override void ExecuteCrmPlugin(LocalPluginContext localcontext)
        {
            // Extract the tracing service for use in debugging sandboxed plug-ins.
            ITracingService tracingService =
                localcontext.TracingService;

            // Obtain the execution context from the service provider.
            IPluginExecutionContext context = localcontext.PluginExecutionContext;

            // Organization Service
            IOrganizationService service = localcontext.OrganizationService;
            IOrganizationService adminservice = localcontext.AdminOrganizationService;
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
            Relationship relationship = new Relationship(Constants.IncidentNature.jarvisIncidentjarvisIncidentNaturejarvisInc);
            try
            {
                // region Create
                if (context.Stage == 40 && context.MessageName.ToUpper() == "CREATE")
                {
                    if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                    {
                        Entity repairInfo = (Entity)context.InputParameters["Target"];
                        if (repairInfo.Attributes.Contains("jarvis_incident") && repairInfo.Attributes["jarvis_incident"] != null)
                        {
                            EntityReference incidentReference = (EntityReference)repairInfo.Attributes["jarvis_incident"];
                            Entity incident = service.Retrieve(incidentReference.LogicalName, incidentReference.Id, new ColumnSet("statuscode", "jarvis_homedealer", "jarvis_incidentnature", "jarvis_incidentnatureshadow"));
                            EntityReference repDealer = (EntityReference)repairInfo.Attributes["jarvis_repairingdealerpassout"];
                            Entity passOutRD = service.Retrieve(repDealer.LogicalName, repDealer.Id, new ColumnSet("jarvis_repairingdealer"));

                            if (incident.Attributes.Contains("statuscode") && incident.Attributes["statuscode"] != null)
                            {
                                OptionSetValue statusReason = (OptionSetValue)incident.Attributes["statuscode"];

                                if (statusReason.Value == 50 || statusReason.Value == 60)
                                {
                                    tracingService.Trace("Case is in Repair Start Stage");
                                    CaseMonitorProcess operations = new CaseMonitorProcess();
                                    EntityCollection passOut = service.RetrieveMultiple(new FetchExpression(string.Format(MCS.Jarvis.CE.Plugins.Constants.FetchXmls.getNonETCPassOuts, incidentReference.Id)));
                                    if (passOut.Entities.Count != 0)
                                    {
                                        EntityCollection getExistingRepairInfo = service.RetrieveMultiple(new FetchExpression(string.Format(MCS.Jarvis.CE.Plugins.Constants.FetchXmls.getDelayedPassOutsETC, incidentReference.Id, repDealer.Id)));
                                        if (getExistingRepairInfo.Entities.Count > 0)
                                        {
                                            // region Create Monitor Action
                                            if (incident.Attributes.Contains("jarvis_homedealer") && incident.Attributes["jarvis_homedealer"] != null)
                                            {
                                                EntityReference homeDealer = (EntityReference)incident.Attributes["jarvis_homedealer"];
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
                                                    string fucomment = isoLangCode + " " + isCountryCode + " " + "Pass Repair info";
                                                    operations.AutomateMonitorCreation(incident, fucomment, 2, 9, 0, string.Empty, service);
                                                }
                                            }

                                            string fuMAcomment = "Chase Diagnose";
                                            operations.AutomateCloseMonitorActions(incident, fuMAcomment, 1, fuMAcomment, service);
                                            // endregion
                                        }
                                    }

                                    if (repairInfo.Attributes.Contains("jarvis_repairingdealerpassout") && repairInfo.Attributes["jarvis_repairingdealerpassout"] != null)
                                    {
                                        EntityCollection delayedpassOut = service.RetrieveMultiple(new FetchExpression(string.Format(MCS.Jarvis.CE.Plugins.Constants.FetchXmls.getInfoForRepairInformation, incidentReference.Id, repDealer.Id)));
                                        EntityCollection getExistingRepairInfo = service.RetrieveMultiple(new FetchExpression(string.Format(MCS.Jarvis.CE.Plugins.Constants.FetchXmls.getDelayedPassOutsETC, incidentReference.Id, repDealer.Id)));
                                        if (delayedpassOut.Entities.Count != 0)
                                        {
                                            bool isCreate = false;
                                            if (getExistingRepairInfo.Entities.Count > 0)
                                            {
                                                foreach (var item in getExistingRepairInfo.Entities)
                                                {
                                                    DateTime createdOn = (DateTime)item.Attributes["createdon"];
                                                    if (createdOn < DateTime.UtcNow.AddMinutes(60))
                                                    {
                                                        isCreate = true;
                                                    }
                                                }

                                                if (isCreate)
                                                {
                                                    // region Create Monitor Action
                                                    if (passOutRD.Attributes.Contains("jarvis_repairingdealer") && passOutRD.Attributes["jarvis_repairingdealer"] != null)
                                                    {
                                                        EntityReference homeDealer = (EntityReference)passOutRD.Attributes["jarvis_repairingdealer"];
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
                                                            string fucomment = isoLangCode + " " + isCountryCode + " " + "Chase ETC";
                                                            operations.AutomateMonitorCreation(incident, fucomment, 2, 9, 0, string.Empty, service);
                                                        }
                                                    }

                                                    // endregion
                                                }
                                            }
                                        }
                                    }
                                }
                                #region UpdateIncidentNature-RepairInfoCreate
                                if (repairInfo.GetAttribute‌​‌​Value<bool>("jarvis_towing") == true)
                                {
                                    EntityCollection getAllExistingRepairInfoRecordsCollection = service.RetrieveMultiple(new FetchExpression(string.Format(MCS.Jarvis.CE.Plugins.Constants.FetchXmls.getAllExistingRepairInfoRecords, incidentReference.Id)));
                                    if (getAllExistingRepairInfoRecordsCollection.Entities.Count > 0)
                                    {
                                        foreach (Entity repairInfoRecords in getAllExistingRepairInfoRecordsCollection.Entities)
                                        {
                                            if (repairInfoRecords.Id != repairInfo.Id)
                                            {
                                                Entity repairInfoRecord = new Entity("jarvis_repairinformation");
                                                repairInfoRecord.Id = repairInfoRecords.Id;
                                                repairInfoRecord["jarvis_towing"] = true;
                                                service.Update(repairInfoRecord);
                                                tracingService.Trace("Repair Info Records are updated with Towing Info");
                                            }
                                        }

                                        #region JArrayApproach
                                        Guid incidentnatid = Guid.NewGuid();

                                        if (incident.Attributes.Contains("jarvis_incidentnatureshadow") && incident.Attributes["jarvis_incidentnatureshadow"] != null)
                                        {
                                            this.fetchandUpdateIncidentNatureMultiselect(incident, tracingService, service);
                                        }
                                        else
                                        {
                                            this.UpdateIncidentNatureMultiselect(incident, tracingService, service);
                                        }
                                        #endregion

                                    }
                                }
                            }

                            #endregion


                            // region Repair Info Translations
                            bool isAutomation = false;
                            isAutomation = CrmHelper.GetAutomationConfig(service, MCS.Jarvis.CE.Plugins.Constants.JarvisConfiguration.Automationtranslation, tracingService);
                            if (isAutomation)
                            {
                                if (repairInfo.Attributes.Contains("jarvis_partsinformation") || repairInfo.Attributes.Contains("jarvis_repairinformation") || repairInfo.Attributes.Contains("jarvis_towingrental") || repairInfo.Attributes.Contains("jarvis_warrantyinformation"))
                                {
                                    TranslationProcess operations = new TranslationProcess();
                                    operations.RepairInfoStandardProcess(adminservice, tracingService, incident, context.InitiatingUserId, repairInfo);
                                    Entity caseUpdate = service.Retrieve(repairInfo.LogicalName, repairInfo.Id, new ColumnSet("jarvis_translationstatuspartsinformation", "jarvis_translationstatusrepairinformation", "jarvis_translationstatustowinginformation", "jarvis_translationstatuswarrantyinformation"));
                                    Entity caseToUpdate = new Entity(repairInfo.LogicalName);
                                    caseToUpdate.Id = repairInfo.Id;
                                    bool isUpdate = false;
                                    OptionSetValue translationStatusParts = (OptionSetValue)caseUpdate.Attributes["jarvis_translationstatuspartsinformation"];
                                    if (translationStatusParts.Value != 334030001 && repairInfo.Attributes.Contains("jarvis_partsinformation"))
                                    {
                                        // In-Progress
                                        caseToUpdate["jarvis_translationstatuspartsinformation"] = new OptionSetValue(334030001);
                                        isUpdate = true;
                                    }

                                    OptionSetValue translationStatusLoc = (OptionSetValue)caseUpdate.Attributes["jarvis_translationstatusrepairinformation"];
                                    if (translationStatusLoc.Value != 334030001 && repairInfo.Attributes.Contains("jarvis_repairinformation"))
                                    {
                                        // In-Progress
                                        caseToUpdate["jarvis_translationstatusrepairinformation"] = new OptionSetValue(334030001);
                                        isUpdate = true;
                                    }

                                    OptionSetValue translationStatusCE = (OptionSetValue)caseUpdate.Attributes["jarvis_translationstatustowinginformation"];
                                    if (translationStatusCE.Value != 334030001 && repairInfo.Attributes.Contains("jarvis_towingrental"))
                                    {
                                        // In-Progress
                                        caseToUpdate["jarvis_translationstatustowinginformation"] = new OptionSetValue(334030001);
                                        isUpdate = true;
                                    }

                                    OptionSetValue translationStatusWarranty = (OptionSetValue)caseUpdate.Attributes["jarvis_translationstatuswarrantyinformation"];
                                    if (translationStatusWarranty.Value != 334030001 && repairInfo.Attributes.Contains("jarvis_warrantyinformation"))
                                    {
                                        // In-Progress
                                        caseToUpdate["jarvis_translationstatuswarrantyinformation"] = new OptionSetValue(334030001);
                                        isUpdate = true;
                                    }

                                    if (isUpdate)
                                    {
                                        service.Update(caseToUpdate);
                                    }
                                }
                            }

                            // endregion
                        }
                    }
                }
                // endregion
                // region Update
                if (context.Stage == 40 && context.MessageName.ToUpper() == "UPDATE")
                {
                    if (context.Depth == 1)
                    {
                        if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                        {
                            Entity repairInfo = (Entity)context.InputParameters["Target"];
                            Entity repairInfoImg = (Entity)context.PostEntityImages["PostImage"];
                            if (repairInfoImg.Attributes.Contains("jarvis_incident") && repairInfoImg.Attributes["jarvis_incident"] != null)
                            {
                                EntityReference incidentReference = (EntityReference)repairInfoImg.Attributes["jarvis_incident"];
                                Entity incident = service.Retrieve(incidentReference.LogicalName, incidentReference.Id, new ColumnSet("statuscode", "jarvis_homedealer", "jarvis_incidentnature", "jarvis_incidentnatureshadow"));

                                // region Repair Info Translations
                                bool isAutomation = false;
                                isAutomation = CrmHelper.GetAutomationConfig(adminservice, MCS.Jarvis.CE.Plugins.Constants.JarvisConfiguration.Automationtranslation, tracingService);
                                if (isAutomation)
                                {
                                    if (repairInfo.Attributes.Contains("jarvis_partsinformation") || repairInfo.Attributes.Contains("jarvis_repairinformation") || repairInfo.Attributes.Contains("jarvis_towingrental") || repairInfo.Attributes.Contains("jarvis_warrantyinformation"))
                                    {
                                        TranslationProcess operations = new TranslationProcess();
                                        operations.RepairInfoStandardProcess(service, tracingService, incident, context.InitiatingUserId, repairInfo);
                                        Entity caseUpdate = service.Retrieve(repairInfo.LogicalName, repairInfo.Id, new ColumnSet("jarvis_translationstatuspartsinformation", "jarvis_translationstatusrepairinformation", "jarvis_translationstatustowinginformation", "jarvis_translationstatuswarrantyinformation"));
                                        Entity caseToUpdate = new Entity(repairInfo.LogicalName);
                                        caseToUpdate.Id = repairInfo.Id;
                                        bool isUpdate = false;
                                        if (caseUpdate.Attributes.Contains("jarvis_translationstatuspartsinformation") && caseUpdate.Attributes["jarvis_translationstatuspartsinformation"] != null && repairInfo.Attributes.Contains("jarvis_partsinformation"))
                                        {
                                            OptionSetValue translationStatusParts = (OptionSetValue)caseUpdate.Attributes["jarvis_translationstatuspartsinformation"];
                                            if (translationStatusParts.Value != 334030001 && repairInfo.Attributes.Contains("jarvis_partsinformation"))
                                            {
                                                // In-Progress
                                                caseToUpdate["jarvis_translationstatuspartsinformation"] = new OptionSetValue(334030001);
                                                isUpdate = true;
                                            }
                                        }

                                        OptionSetValue translationStatusLoc = (OptionSetValue)caseUpdate.Attributes["jarvis_translationstatusrepairinformation"];
                                        if (translationStatusLoc.Value != 334030001 && repairInfoImg.Attributes.Contains("jarvis_repairinformation") && repairInfo.Attributes.Contains("jarvis_repairinformation"))
                                        {
                                            // In-Progress
                                            caseToUpdate["jarvis_translationstatusrepairinformation"] = new OptionSetValue(334030001);
                                            isUpdate = true;
                                        }

                                        OptionSetValue translationStatusCE = (OptionSetValue)caseUpdate.Attributes["jarvis_translationstatustowinginformation"];
                                        if (translationStatusCE.Value != 334030001 && repairInfoImg.Attributes.Contains("jarvis_towingrental") && repairInfo.Attributes.Contains("jarvis_towingrental"))
                                        {
                                            // In-Progress
                                            caseToUpdate["jarvis_translationstatustowinginformation"] = new OptionSetValue(334030001);
                                            isUpdate = true;
                                        }

                                        OptionSetValue translationStatusWarranty = (OptionSetValue)caseUpdate.Attributes["jarvis_translationstatuswarrantyinformation"];
                                        if (translationStatusWarranty.Value != 334030001 && repairInfoImg.Attributes.Contains("jarvis_warrantyinformation") && repairInfo.Attributes.Contains("jarvis_warrantyinformation"))
                                        {
                                            // In-Progress
                                            caseToUpdate["jarvis_translationstatuswarrantyinformation"] = new OptionSetValue(334030001);
                                            isUpdate = true;
                                        }

                                        if (isUpdate)
                                        {
                                            service.Update(caseToUpdate);
                                        }
                                    }
                                }
                                // endregion

                                #region incidentnatureupdate-towing
                                if (repairInfo.GetAttribute‌​‌​Value<bool>("jarvis_towing") == true)
                                {
                                    EntityCollection getAllExistingRepairInfoRecordsCollection = service.RetrieveMultiple(new FetchExpression(string.Format(MCS.Jarvis.CE.Plugins.Constants.FetchXmls.getAllExistingRepairInfoRecords, incidentReference.Id)));
                                    if (getAllExistingRepairInfoRecordsCollection.Entities.Count > 0)
                                    {
                                        tracingService.Trace("Count of Repair Info Records are" + getAllExistingRepairInfoRecordsCollection.Entities.Count);
                                        foreach (Entity repairInfoRecords in getAllExistingRepairInfoRecordsCollection.Entities)
                                        {
                                            if (repairInfoRecords.Id != repairInfo.Id)
                                            {
                                                Entity repairInfoRecord = new Entity("jarvis_repairinformation");
                                                repairInfoRecord.Id = repairInfoRecords.Id;
                                                repairInfoRecord["jarvis_towing"] = true;
                                                service.Update(repairInfoRecord);
                                                tracingService.Trace("Repair Info Records are updated with Towing Info");
                                            }
                                        }
                                        #region tryJArrayApproach

                                        if (incident.Attributes.Contains("jarvis_incidentnatureshadow") && incident.Attributes["jarvis_incidentnatureshadow"] != null)
                                        {
                                            this.fetchandUpdateIncidentNatureMultiselect(incident, tracingService, service);
                                        }
                                        else
                                        {
                                            this.UpdateIncidentNatureMultiselect(incident, tracingService, service);
                                        }
                                        #endregion
                                    }
                                }
                                else if (repairInfo.GetAttributeValue<bool>("jarvis_towing") == false && incident.Attributes["jarvis_incidentnatureshadow"] != null && incident.Attributes["jarvis_incidentnatureshadow"].ToString().Contains("Towing"))
                                {
                                    bool isTowingFalseinanyExistingRepairInfo = true;
                                    bool towingValueinExistingRecords = true;
                                    EntityCollection getAllExistingRepairInfoRecordsCollection = service.RetrieveMultiple(new FetchExpression(string.Format(MCS.Jarvis.CE.Plugins.Constants.FetchXmls.getAllExistingRepairInfoRecords, incident.Id)));
                                    if (getAllExistingRepairInfoRecordsCollection.Entities.Count > 0)
                                    {
                                        foreach (Entity repairInfoRecords in getAllExistingRepairInfoRecordsCollection.Entities)
                                        {
                                            if (!repairInfoRecords.Id.Equals(repairInfo.Id))
                                            {
                                                towingValueinExistingRecords = repairInfoRecords.GetAttributeValue<bool>("jarvis_towing");;
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
                                            EntityCollection getTowingRecordInfoCollection = service.RetrieveMultiple(new FetchExpression(string.Format(MCS.Jarvis.CE.Plugins.Constants.FetchXmls.getTowingRecordInfo)));
                                            if (getTowingRecordInfoCollection.Entities.Count == 1)
                                            {
                                                towingId = getTowingRecordInfoCollection.Entities[0].Id;
                                            }
                                            string incidentNatureToAdd = "Towing";
                                            #region tryJObject Approach
                                            Guid incidentnatid = Guid.NewGuid();
                                            var incidentNaturesMultiselect = JsonConvert.DeserializeObject(incident.Attributes["jarvis_incidentnature"].ToString());
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
                                                var removedIncidentStringArray = incidentnature.ToString();
                                                var jArray = JArray.Parse(removedIncidentStringArray);
                                                var nametoDetect = "Towing";
                                                JArray filteredJArray = new JArray(jArray.Where(item1 => (string)item1["name"] != "Towing"));
                                                string filteredJArrayString = filteredJArray.ToString();
                                                var jsonData = JsonConvert.DeserializeObject(filteredJArrayString);

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
                            }
                        }
                    }
                }
            }

            // endregion
            catch (InvalidPluginExecutionException ex)
            {
                tracingService.Trace(ex.Message);
                tracingService.Trace(ex.StackTrace);
                throw new InvalidPluginExecutionException("Error in Repair Info Monitor Operations " + ex.Message);
            }
        }

        private void UpdateIncidentNatureMultiselect(Entity incident, ITracingService tracingService, IOrganizationService service)
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
            tracingService.Trace("coming inside Shadow null condition");
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
            // incidentStringArray = incidentStringArray + incnatureObj + ",";
            incidentnature2.Add(incnatureObj2);
            incidentStringArray = incidentnature2.ToString();
            var jsonData = JsonConvert.DeserializeObject(incidentStringArray);
            tracingService.Trace("combined JSON Data 1 is " + jsonData);
            string trimmedJsonString = JsonConvert.SerializeObject(jsonData, Formatting.None);
            incidentNatureUpdate["jarvis_incidentnature"] = trimmedJsonString;
            incidentNatureUpdate.Id = incident.Id;
            service.Update(incidentNatureUpdate);
        }

        private void fetchandUpdateIncidentNatureMultiselect(Entity incident, ITracingService tracingService, IOrganizationService service)
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
            if (incident.Attributes["jarvis_incidentnatureshadow"] != null && incident.Attributes["jarvis_incidentnatureshadow"].ToString().Contains("Towing"))
            { }
            else
            {
                var incidentNaturesMultiselect = JsonConvert.DeserializeObject(incident.Attributes["jarvis_incidentnature"].ToString());
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
                // incidentStringArray = incidentStringArray + incnatureObj + ",";
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
