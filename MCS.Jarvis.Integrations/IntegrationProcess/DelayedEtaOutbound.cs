// <copyright file="DelayedEtaOutbound.cs" company="Microsoft.">
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

    /// <summary>
    /// DelayedEtaOutbound Class.
    /// </summary>
    public class DelayedEtaOutbound
    {
        private readonly IDynamicsApiClient dynamicsApiClient;
        private readonly IConfiguration config;
        private readonly ILoggerService logger;
        private readonly IntegrationHelper helper;
        private readonly int counter = 0;
        private readonly Dictionary<string, JArray?> retrieveList = new Dictionary<string, JArray?>();
        private JObject sbMessage = new ();
        private string? modifiedOn = DateTime.UtcNow.ToString();

        /// <summary>
        /// Initializes a new instance of the <see cref="DelayedEtaOutbound"/> class.
        /// DelayedEtaOutbound Constructor.
        /// </summary>
        /// <param name="dynamicsApiClient">DynamicsApiClient.</param>
        /// <param name="config">Config.</param>
        /// <param name="logger">Logger.</param>
        public DelayedEtaOutbound(IDynamicsApiClient dynamicsApiClient, IConfiguration config, ILoggerService logger)
        {
            this.dynamicsApiClient = dynamicsApiClient;
            this.config = config;
            this.logger = logger;
            this.helper = new IntegrationHelper(this.dynamicsApiClient, this.logger);
        }

        /// <summary>
        /// IntegrationProcessAsync.
        /// </summary>
        /// <param name="etaPassOutTransId">EtaPassOutTransId.</param>
        /// <param name="entityData">EntityData.</param>
        /// <returns>Task.</returns>
        /// <exception cref="ArgumentException">Argument Null Exception.</exception>
        public async Task<JObject> IntegrationProcessAsync(string etaPassOutTransId, string entityData)
        {
            try
            {
                ////Checking Payload contains unique identifier
                if (!string.IsNullOrEmpty(etaPassOutTransId))
                {
                    string? triggeredBy = string.Empty;
                    if (!string.IsNullOrEmpty(entityData))
                    {
                        this.logger.LogTrace($"PluginContextData: {entityData}");
                        JObject record = this.helper.ConvertContextToObject(entityData);
                        if (record.HasValues)
                        {
                            this.modifiedOn = record.GetValue("modifiedon")?.ToString();
                            triggeredBy = record.GetValue(Constants.TriggeredBy) != null ? record.GetValue(Constants.TriggeredBy)?.ToString() : string.Empty;
                            ////Getting All Master Data and Related Queries in single call aganist unique identifier from the payload "id".
                            await this.GetConfigMasterDataAsBatch(etaPassOutTransId.ToString().ToLower(), triggeredBy);
                            var retrieveInc = this.retrieveList.First(item => item.Key.ToUpper() == Constants.JarvisPassouttranslations.ToUpper()).Value?.First().ToObject<JObject>();
                            retrieveInc?.Merge(record);
                            if (retrieveInc != null)
                            {
                                this.retrieveList.First(item => item.Key.ToUpper() == Constants.JarvisPassouttranslations.ToUpper()).Value?.RemoveAll();
                                this.retrieveList.First(item => item.Key.ToUpper() == Constants.JarvisPassouttranslations.ToUpper()).Value?.Add(retrieveInc);
                            }
                        }
                        else
                        {
                            ////Getting All Master Data and Related Queries in single call aganist unique identifier from the payload "id".
                            await this.GetConfigMasterDataAsBatch(etaPassOutTransId.ToString().ToLower(), triggeredBy);
                        }
                    }
                    else
                    {
                        ////Getting All Master Data and Related Queries in single call aganist unique identifier from the payload "id".
                        await this.GetConfigMasterDataAsBatch(etaPassOutTransId.ToString().ToLower(), triggeredBy);
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
                            this.sbMessage = this.CaseToServiceRequestBdArgus(intMapping.ToObject<JObject>());

                            if (intConfigRecord.TryGetValue(Constants.JarvisPassouts, StringComparison.OrdinalIgnoreCase, out JToken? passOutMappings))
                            {
                                JArray passOutsPayload = this.PassOutoSRBdPassoutLoginfoArgus(passOutMappings.ToObject<JObject>(), intConfigRecord);
                                JObject passOutValues = new ()
                                {
                                    { "SRBdPassoutLoginfoArgus", passOutsPayload },
                                };

                                this.sbMessage.Add("ListOfSRBdPassoutLoginfoArgus", passOutValues);
                            }
#pragma warning restore S1481 // Unused local variables should be removed

                            ////Checking MultipartContent contains Data and executing whole payload request..
                            if (this.sbMessage.HasValues)
                            {
                                return new JObject { { "sbMessage", this.sbMessage }, { "modifiedOn", this.modifiedOn } };
                            }

                            this.logger.LogException(new Exception("Error in Generating Payload"));
                            throw new ArgumentException("Error in Generating Payload");
                        }

                        this.logger.LogException(new Exception("No Integration Configuration Found/ Integration is not Active, Pleae activate the configuraiton record."));
                        throw new ArgumentException("Payload does not contain unique indentifier");
                    }
                    else
                    {
                        this.logger.LogException(new Exception("No Integration Configuration Found/ Integration is not Active, Pleae activate the configuraiton record."));
                        throw new ArgumentException("No Integration Configuration Found/ Integration is not Active, Pleae activate the configuraiton record.");
                    }
                }
                else
                {
                    this.logger.LogException(new Exception("Payload does not contain unique indentifier"));
                    throw new ArgumentException("Payload does not contain unique indentifier");
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"IntegrationProcessAsync:" + ex.Message);
            }
        }

        /// <summary>
        /// Get ConfigMasterDat as Batch for UpsertCaseOut.
        /// </summary>
        /// <param name="etaPassOutTransId">etaPassOutTransId.</param>
        /// <param name="modifiedBy">Modified By.</param>
        /// <returns>Task.</returns>
        /// <exception cref="ArgumentException">Argument Null Exception.</exception>
        private async Task GetConfigMasterDataAsBatch(string etaPassOutTransId, string? modifiedBy)
        {
            try
            {
                List<HttpMessageContent> masterContents = new List<HttpMessageContent>();
                this.logger.LogTrace("Enter into ConfigurationMaster Data");
                //// Used configuration code to retrieve Config record
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.JarvisIntegrationConfiguration, string.Format(CrmQueries.JarvisIntegrationConfigurationQuery, "DELAYEDETA002".ToString().ToUpper()), 1, string.Empty, false));
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.JarvisPassouttranslations, string.Format(CrmQueries.JarvisPassOuttranslationOutboundQuery, etaPassOutTransId), 1, string.Empty, false));
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.JarvisLanguages, CrmQueries.JarvisStatecodeActiveQuery, 1, string.Empty, false));
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.JarvisBrands, CrmQueries.JarvisStatecodeActiveQuery, 1, string.Empty, false));
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.JarvisCountries, CrmQueries.JarvisStatecodeActiveQuery, 1, string.Empty, false));
                if (!string.IsNullOrEmpty(modifiedBy))
                {
                    masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.Users, string.Format(CrmQueries.GetUserDetails, modifiedBy), 1, string.Empty, false));
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
        /// EtaTranslation To SrBreakdownEtaLogArgus.
        /// </summary>
        /// <param name="mappings">Mappings.</param>
        /// <returns>JArray.</returns>
        /// <exception cref="ArgumentException">Argument Null Exception.</exception>
        private JArray EtaTranslationToSrBreakdownEtaLogArgus(JObject? mappings)
        {
            try
            {
                ////Retrive Incident Payload from Retrieve List
                JArray etaTransList = new JArray();
                string? sourceEntity = DynamicsApiHelper.GetStringValueFromJObject(mappings, Constants.SourceEntityName).ToLower();
                var matchMasterRecord = this.retrieveList.First(item => item.Key.ToUpper().Contains(sourceEntity.ToUpper())).Value?.ToList();
                if (matchMasterRecord != null && matchMasterRecord.Count > 0)
                {
                    foreach (var item in matchMasterRecord)
                    {
                        if (mappings != null && mappings.TryGetValue(Constants.JarvisFieldMappings, StringComparison.OrdinalIgnoreCase, out JToken? fieldMapping) && fieldMapping != null)
                        {
                            this.helper.ValidateSetFieldMappingOutbound(item.ToObject<JObject>(), fieldMapping);
                            JObject payload = this.helper.SetFieldMappingOutbound(item.ToObject<JObject>(), fieldMapping, this.retrieveList);
                            etaTransList.Add(payload);
                        }
                    }
                }

                return etaTransList;
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"EtaTranslationToSrBreakdownEtaLogArgus:" + ex.Message);
            }
        }

        /// <summary>
        /// PassOutoSRBdPassoutLoginfoArgus.
        /// </summary>
        /// <param name="mappings">Mapping.</param>
        /// <returns>JArray.</returns>
        /// <exception cref="ArgumentException">Argument Null Exception.</exception>
        private JArray PassOutoSRBdPassoutLoginfoArgus(JObject? mappings, JObject intConfigRecord)
        {
            try
            {
                ////Retrive PassOut Payload from Retrieve List
                JObject? payload;
                JArray passOutList = new JArray();
                string? sourceEntity = DynamicsApiHelper.GetStringValueFromJObject(mappings, Constants.SourceEntityName).ToLower();
                var matchMasterRecord = this.retrieveList.First(item => item.Key.ToUpper().Contains(sourceEntity.ToUpper())).Value?.ToList();
                if (matchMasterRecord != null && matchMasterRecord.Count > 0)
                {
                    payload = matchMasterRecord.First().ToObject<JObject>();

                    if (mappings != null && mappings.TryGetValue(Constants.JarvisFieldMappings, StringComparison.OrdinalIgnoreCase, out JToken? fieldMapping) && fieldMapping != null && payload != null && payload.HasValues)
                    {
                        this.helper.ValidateSetFieldMappingOutbound(payload, fieldMapping);
                        JObject passOutPayload = this.helper.SetFieldMappingOutbound(payload, fieldMapping, this.retrieveList);

                        ////Retriving Eta Translation IntegrationMappings
                        if (intConfigRecord.TryGetValue(Constants.JarvisPassouttranslations, StringComparison.OrdinalIgnoreCase, out JToken? translationMappings))
                        {
                            //// SRBdReportFaultArgus to Case translation Payload generation.
                            JArray etaTranslationList = this.EtaTranslationToSrBreakdownEtaLogArgus(translationMappings.ToObject<JObject>());
                            JObject keyValues = new ()
                                {
                                    { "SrBreakdownEtaLogArgus", etaTranslationList },
                                };

                            passOutPayload.Add("ListOfSrBreakdownEtaLogArgus", keyValues);
                        }

                        passOutList.Add(passOutPayload);
                    }
                }

                return passOutList;
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"PassOutoSRBdPassoutLoginfoArgus:" + ex.Message);
            }
        }

        /// <summary>
        /// CaseToServiceRequestBdArgus.
        /// </summary>
        /// <param name="mappings">Mappings.</param>
        /// <returns>JObject.</returns>
        /// <exception cref="ArgumentException">Argument Null Exception.</exception>
        private JObject CaseToServiceRequestBdArgus(JObject? mappings)
        {
            try
            {
                ////Retrive Incident Payload from Retrieve List
                JObject? payload;
                JObject casePayload = new JObject();
                string? sourceEntity = DynamicsApiHelper.GetStringValueFromJObject(mappings, Constants.SourceEntityName).ToLower();

                var matchMasterRecord = this.retrieveList.First(item => item.Key.ToUpper().Contains(sourceEntity.ToUpper())).Value?.ToList();
                if (matchMasterRecord != null && matchMasterRecord.Count > 0)
                {
                    payload = matchMasterRecord.First().ToObject<JObject>();

                    if (mappings != null && mappings.TryGetValue(Constants.JarvisFieldMappings, StringComparison.OrdinalIgnoreCase, out JToken? fieldMapping) && fieldMapping != null && payload != null && payload.HasValues)
                    {
                        this.helper.ValidateSetFieldMappingOutbound(payload, fieldMapping);
                        casePayload = this.helper.SetFieldMappingOutbound(payload, fieldMapping, this.retrieveList);
                    }
                }

                return casePayload;
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"CaseToServiceRequestBdArgus:" + ex.Message);
            }
        }
    }
}
