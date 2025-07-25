// <copyright file="UpdateCaseClosureOut.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace IntegrationProcess
{
    using IntegrationProcess.Helper.Constants;
    using MCS.Jarvis.Integration.Base.Dynamics;
    using MCS.Jarvis.Integration.Base.Logging;
    using MCS.Jarvis.IntegrationProcess.Helper;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json.Linq;
    using System;

    /// <summary>
    /// UpdateCaseOut Class.
    /// </summary>
    public class UpdateCaseClosureOut
    {
        private readonly IDynamicsApiClient dynamicsApiClient;
        private readonly IConfiguration config;
        private readonly ILoggerService logger;
        private readonly IntegrationHelper helper;
        private readonly Dictionary<string, JArray?> retrieveList = new Dictionary<string, JArray?>();
        private JObject sbMessage = new ();
        private string? modifiedOn = DateTime.UtcNow.ToString();

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateCaseClosureOut"/> class.
        /// UpdateCaseClosureOut Constructor.
        /// </summary>
        /// <param name="dynamicsApiClient">DynamicsApiClient.</param>
        /// <param name="config">Config.</param>
        /// <param name="logger">Logger.</param>
        public UpdateCaseClosureOut(IDynamicsApiClient dynamicsApiClient, IConfiguration config, ILoggerService logger)
        {
            this.dynamicsApiClient = dynamicsApiClient;
            this.config = config;
            this.logger = logger;
            this.helper = new IntegrationHelper(this.dynamicsApiClient, this.logger);
        }

        /// <summary>
        /// IntegrationProcessAsync.
        /// </summary>
        /// <param name="caseId">CaseId.</param>
        /// <param name="entityData">EntityData.</param>
        /// <returns>Task.</returns>
        /// <exception cref="ArgumentException">Argument Null Exception.</exception>
        public async Task<JObject> IntegrationProcessAsync(string caseId, string entityData)
        {
            try
            {
                ////Checking Payload contains unique identifier
                if (!string.IsNullOrEmpty(caseId))
                {
                    string? modifiedBy = string.Empty;
                    string? caseWorker = string.Empty;
                    if (!string.IsNullOrEmpty(entityData))
                    {
                        this.logger.LogTrace($"PluginContextData: {entityData}");
                        JObject record = this.helper.ConvertContextToObject(entityData);
                        if (record.HasValues)
                        {
                            this.modifiedOn = record.GetValue("modifiedon")?.ToString();
                            modifiedBy = record.GetValue(Constants.ModifiedBy) != null ? record.GetValue(Constants.ModifiedBy)?.ToString() : string.Empty;
                            caseWorker = record.GetValue(Constants.Owner) != null ? record.GetValue(Constants.Owner)?.ToString() : string.Empty;
                            ////Getting All Master Data and Related Queries in single call aganist unique identifier from the payload "id".
                            await this.GetConfigMasterDataAsBatch(caseId.ToString().ToLower(), modifiedBy, caseWorker);
                            var retrieveInc = this.retrieveList.First(item => item.Key.ToUpper() == Constants.Incidents.ToUpper()).Value?.First().ToObject<JObject>();
                            retrieveInc?.Merge(record);
                            if (retrieveInc != null)
                            {
                                this.retrieveList.First(item => item.Key.ToUpper() == Constants.Incidents.ToUpper()).Value?.RemoveAll();
                                this.retrieveList.First(item => item.Key.ToUpper() == Constants.Incidents.ToUpper()).Value?.Add(retrieveInc);
                            }
                        }
                        else
                        {
                            ////Getting All Master Data and Related Queries in single call aganist unique identifier from the payload "id".
                            await this.GetConfigMasterDataAsBatch(caseId.ToString().ToLower(), modifiedBy, caseWorker);
                        }
                    }
                    else
                    {
                        ////Getting All Master Data and Related Queries in single call aganist unique identifier from the payload "id".
                        await this.GetConfigMasterDataAsBatch(caseId.ToString().ToLower(), modifiedBy, caseWorker);
                    }

                    var configRecord = this.retrieveList.First(item => item.Key.ToUpper() == Constants.JarvisIntegrationConfiguration.ToUpper()).Value;
                    if (configRecord != null && configRecord.Count > 0
                          && configRecord.First().ToObject<JObject>().TryGetValue(Constants.JarvisIntegrationMapping, StringComparison.OrdinalIgnoreCase, out JToken? intConfig))
                    {
                        JObject intConfigRecord = JObject.Parse(intConfig.ToString());

                        ////Retriving Incident IntegrationMappings
                        if (intConfigRecord != null && intConfigRecord.TryGetValue(Constants.Incident, StringComparison.OrdinalIgnoreCase, out JToken? intMapping))
                        {
                            //// ServiceRequestBdArgus to Incident Payload generation.
#pragma warning disable S1481 // Unused local variables should be removed
                            this.sbMessage = this.CaseToServiceRequestBdArgus(caseId.ToString().ToLower(), intMapping.ToObject<JObject>(), caseWorker);

#pragma warning restore S1481 // Unused local variables should be removed
                            this.GopMapping(intConfigRecord);
                            this.PassoutMapping(intConfigRecord);
                            ////Checking MultipartContent contains Data and executing whole payload request..
                            if (this.sbMessage.HasValues)
                            {
                                return new JObject { { "sbMessage", this.sbMessage }, { "modifiedOn", this.modifiedOn } };
                            }

                            this.logger.LogException(new Exception("Error in Generating Payload"));
                            throw new ArgumentException("Error in Generating Payload");
                        }

                        this.logger.LogException(new Exception("No Integration Configuration Found/ Integration is not Active, Pleae activate the configuraiton record."));
                        throw new ArgumentException("No Integration Configuration Found/ Integration is not Active, Pleae activate the configuraiton record.");
                    }
                    else
                    {
                        this.logger.LogException(new Exception("No Integration Configuration Found/ Integration is not Active, Pleae activate the configuraiton record."));
                        throw new ArgumentException("No Integration Configuration Found/ Integration is not Active, Pleae activate the configuraiton record.");
                    }
                }
                else
                {
                    this.logger.LogException(new Exception("Payload does not contain unique indentifier IncidentId"));
                    throw new ArgumentException("Payload does not contain unique indentifier IncidentId");
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"IntegrationProcessAsync:" + ex.Message);
            }
        }

        private void PassoutMapping(JObject intConfigRecord)
        {
            var passoutRecords = this.retrieveList.First(item => item.Key.ToUpper().Contains(Constants.JarvisPassouts.ToUpper()));
            if (passoutRecords.Value != null && passoutRecords.Value?.ToList().Count > 0 && intConfigRecord.TryGetValue(Constants.JarvisPassouts, StringComparison.OrdinalIgnoreCase, out JToken? intPassoutMapping) && intPassoutMapping.ToObject<JObject>() != null)
            {
                if (intPassoutMapping.ToObject<JObject>().TryGetValue(Constants.JarvisFieldMappings, out JToken? passoutFieldMapping) && passoutFieldMapping != null)
                {
                    JArray? passoutPayloads = new JArray();
                    foreach (var item in passoutRecords.Value?.ToList())
                    {
                        JObject? passoutPayload;
                        passoutPayload = this.helper.SetFieldMappingOutbound(item.ToObject<JObject>(), passoutFieldMapping, this.retrieveList);
                        passoutPayloads.Add(passoutPayload);
                    }

                    this.sbMessage.Add(SiebelConstants.ListOfSRBdPassoutLoginfoArgus, new JObject(new JProperty(SiebelConstants.SRBdPassoutLoginfoArgus, passoutPayloads)));
                }
            }
        }

        private void GopMapping(JObject intConfigRecord)
        {
            var gopRecords = this.retrieveList.First(item => item.Key.ToUpper().Contains(Constants.JarvisGops.ToUpper()));
            if (gopRecords.Value != null && gopRecords.Value?.ToList().Count > 0 && intConfigRecord.TryGetValue(Constants.JarvisGops, StringComparison.OrdinalIgnoreCase, out JToken? intGopMapping) && intGopMapping.ToObject<JObject>() != null)
            {
                if (intGopMapping.ToObject<JObject>().TryGetValue(Constants.JarvisFieldMappings, out JToken? gopFieldMapping) && gopFieldMapping != null)
                {
                    JArray? gopPayloads = new JArray();
                    foreach (var item in gopRecords.Value?.ToList())
                    {
                        JObject? gopPayload;
                        gopPayload = this.helper.SetFieldMappingOutbound(item.ToObject<JObject>(), gopFieldMapping, this.retrieveList);
                        gopPayloads.Add(gopPayload);
                    }

                    this.sbMessage.Add(SiebelConstants.ListOfSRBdGopInLoginfoArgus, new JObject(new JProperty(SiebelConstants.SRBdGopInLoginfoArgus, gopPayloads)));
                }
            }
        }

        /// <summary>
        /// Get ConfigMasterDat as Batch for UpsertCaseOut.
        /// </summary>
        /// <param name="sourceCaseId">SourceCaseId.</param>
        /// <param name="modifiedBy">Modified by.</param>
        /// <returns>Task.</returns>
        /// <exception cref="ArgumentException">Argument Null Exception.</exception>
        private async Task GetConfigMasterDataAsBatch(string sourceCaseId, string? modifiedBy, string? caseWorker)
        {
            try
            {
                List<HttpMessageContent> masterContents = new List<HttpMessageContent>();
                this.logger.LogTrace("Enter into ConfigurationMaster Data");
                //// Used configuration code to retrieve Config record
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.JarvisIntegrationConfiguration, string.Format(CrmQueries.JarvisIntegrationConfigurationQuery, "CSIS001".ToString().ToUpper()), 1, string.Empty, false));
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.Incidents, string.Format(CrmQueries.IncidentCsisOutboundQuery, sourceCaseId), 1, string.Empty, false));
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.JarvisMileages, CrmQueries.JarvisStatecodeActiveQuery, 1, string.Empty, false));
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.JarvisLanguages, CrmQueries.JarvisStatecodeActiveQuery, 1, string.Empty, false));
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.JarvisBrands, CrmQueries.JarvisStatecodeActiveQuery, 1, string.Empty, false));
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.JarvisCountries, CrmQueries.JarvisStatecodeActiveQuery, 1, string.Empty, false));
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, SiebelConstants.Incidentresolutions, string.Format(CrmQueries.JarvisIncidentResolutionQuery, sourceCaseId), 1, string.Empty, false));
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.JarvisPassouts, string.Format(CrmQueries.GetCsisPassoutOutboundQuery, sourceCaseId), 1, string.Empty, false));
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.JarvisGops, string.Format(CrmQueries.GetCsisGopOutboundQuery, sourceCaseId), 1, string.Empty, false));
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.Transactioncurrencies, CrmQueries.JarvisStatecodeActiveQuery, 1, string.Empty, false));
                if (!string.IsNullOrEmpty(modifiedBy) && !string.IsNullOrEmpty(caseWorker))
                {
                    masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.Users, string.Format(CrmQueries.GetModifiedOwnerUserDetails, modifiedBy, caseWorker), 1, string.Empty, false));
                }

                var response = await this.dynamicsApiClient.ExecuteGetBatchRequest(masterContents);
                if (response != null)
                {
                    foreach (var res in response)
                    {
                        if (res.IsSuccessStatusCode)
                        {
                            var jsonPayload = await res.Content.ReadAsStringAsync();
                            JObject jsonData = JObject.Parse(jsonPayload);
                            if (jsonData.HasValues && jsonData.TryGetValue("value", out JToken? value) && jsonData.TryGetValue("@odata.context", out JToken? odataContext) && odataContext.ToString() != null)
                            {
                                this.retrieveList.Add(odataContext.ToString().Split("#")[1].ToString().Split("(")[0].ToString(), jsonData.GetValue("value")?.ToObject<JArray>());
                            }
                        }
                    }
                }

                if (this.retrieveList.Count <= 0)
                {
                    this.logger.LogException(new ArgumentException("Getting IntegrationConfiguration,MasterData and Dealers record Failed"));
                    throw new ArgumentException("No MasterData,Configuration Record Retrieved");
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"GetConfigMasterDataAsBatch:" + ex.Message);
            }
        }

        /// <summary>
        /// Case to Service Request Bd Argus.
        /// </summary>
        /// <param name="caseId">CaseId.</param>
        /// <param name="mappings">Mappings.</param>
        /// <param name="caseWorker">caseWorker.</param>
        /// <returns>JObject.</returns>
        /// <exception cref="ArgumentException">Argument Null Exception.</exception>
        private JObject CaseToServiceRequestBdArgus(string caseId, JObject? mappings, string? caseWorker)
        {
            try
            {
                ////Retrive Incident Payload from Retrieve List
                JObject? payload;
                JObject casePayload = new JObject();
                string? sourceEntity = DynamicsApiHelper.GetStringValueFromJObject(mappings, Constants.SourceEntityName).ToLower();
                string? sourceFieldSchema = DynamicsApiHelper.GetStringValueFromJObject(mappings, Constants.SourceFieldSchema);

                var matchMasterRecord = this.retrieveList.First(item => item.Key.ToUpper().Contains(sourceEntity.ToUpper())).Value?.ToList();
                if (matchMasterRecord != null && matchMasterRecord.Count > 0)
                {
                    payload = matchMasterRecord.First(item => item[sourceFieldSchema]?.ToString().ToLower() == caseId).ToObject<JObject>();

                    if (mappings != null && mappings.TryGetValue(Constants.JarvisFieldMappings, out JToken? fieldMapping) && fieldMapping != null && payload != null && payload.HasValues)
                    {
                        this.helper.ValidateSetFieldMappingOutbound(payload, fieldMapping);
                        casePayload = this.helper.SetFieldMappingOutbound(payload, fieldMapping, this.retrieveList);

                        // ForceClose
                        if (matchMasterRecord.First().SelectToken(SiebelConstants.JarvisMercuriusstatus) != null && Convert.ToInt64(matchMasterRecord.First().SelectToken(SiebelConstants.JarvisMercuriusstatus)?.ToString()) == 900 &&
                            matchMasterRecord.First().SelectToken(Constants.Statuscode) != null && Convert.ToInt64(matchMasterRecord.First().SelectToken(Constants.Statuscode)?.ToString()) == 1000)
                        {
                            if (matchMasterRecord.First().SelectToken("modifiedon") != null && this.modifiedOn == null)
                            {
                                this.modifiedOn = matchMasterRecord.First().SelectToken("modifiedon")?.ToString();
                            }

                            var resolutionpayload = this.retrieveList.First(item => item.Key.ToUpper() == SiebelConstants.Incidentresolutions.ToUpper()).Value;
                            if (resolutionpayload != null && resolutionpayload.Count() > 0)
                            {
                                JObject? closerPayload = resolutionpayload.First().ToObject<JObject>();

                                if (closerPayload != null && closerPayload.TryGetValue(SiebelConstants.JarvisClosureDescriptionIncidentResolution, StringComparison.OrdinalIgnoreCase, out JToken? closerDescription) &&
                                   closerPayload.TryGetValue(SiebelConstants.JarvisClosureTypeIncidentResolution, StringComparison.OrdinalIgnoreCase, out JToken? closertype)
                                   && closerDescription.SelectToken(SiebelConstants.JarvisName) != null && closertype.SelectToken(SiebelConstants.JarvisName) != null)
                                {
                                    casePayload.Add(SiebelConstants.ForceCloseReason, closerDescription.SelectToken(SiebelConstants.JarvisName));
                                    casePayload.Add(SiebelConstants.ForcedCloseType, closertype.SelectToken(SiebelConstants.JarvisName));
                                    casePayload.Add(SiebelConstants.ForcedClosedARGUS, "Y");
                                }
                            }
                        }
                        else
                        {
                            casePayload.Add(SiebelConstants.ForcedClosedARGUS, "N");
                        }
                    }
                }

                return casePayload;
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"ServiceRequestBdArgusToCase:" + ex.Message);
            }
        }
    }
}