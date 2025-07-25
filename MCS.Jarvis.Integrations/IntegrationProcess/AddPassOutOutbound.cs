// <copyright file="AddPassOutOutbound.cs" company="Microsoft.">
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
    /// AddPassOutOutbound Class.
    /// </summary>
    public class AddPassOutOutbound
    {
        private readonly IDynamicsApiClient dynamicsApiClient;
        private readonly IConfiguration config;
        private readonly ILoggerService logger;
        private readonly IntegrationHelper helper;
        private readonly Dictionary<string, JArray?> retrieveList = new Dictionary<string, JArray?>();
        private JObject sbMessage = new ();
        private string? modifiedOn = DateTime.UtcNow.ToString();

        /// <summary>
        /// Initializes a new instance of the <see cref="AddPassOutOutbound"/> class.
        /// AddPassOutOutbound Constructor.
        /// </summary>
        /// <param name="dynamicsApiClient">DynamicsApiClient.</param>
        /// <param name="config">Config.</param>
        /// <param name="logger">Logger.</param>
        public AddPassOutOutbound(IDynamicsApiClient dynamicsApiClient, IConfiguration config, ILoggerService logger)
        {
            this.dynamicsApiClient = dynamicsApiClient;
            this.config = config;
            this.logger = logger;
            this.helper = new IntegrationHelper(this.dynamicsApiClient, this.logger);
        }

        /// <summary>
        /// IntegrationProcessAsync.
        /// </summary>
        /// <param name="passOutId">PassOutId.</param>
        /// <param name="entityData">EntityData.</param>
        /// <returns>Jobject.</returns>
        /// <exception cref="ArgumentException">Argument Null Exception.</exception>
        public async Task<JObject> IntegrationProcessAsync(string passOutId, string entityData)
        {
            try
            {
                ////Checking Payload contains unique identifier
                if (!string.IsNullOrEmpty(passOutId))
                {
                    string? modifiedBy = string.Empty;
                    if (!string.IsNullOrEmpty(entityData))
                    {
                        this.logger.LogTrace($"PluginContextData: {entityData}");
                        JObject record = this.helper.ConvertContextToObject(entityData);
                        if (record.HasValues)
                        {
                            this.modifiedOn = record.GetValue("modifiedon")?.ToString();
                            modifiedBy = record.GetValue(Constants.ModifiedBy) != null ? record.GetValue(Constants.ModifiedBy)?.ToString() : string.Empty;
                            ////Getting All Master Data and Related Queries in single call aganist unique identifier from the payload "id".
                            await this.GetConfigMasterDataAsBatch(passOutId.ToString().ToLower(), modifiedBy);
                            var retrieveInc = this.retrieveList.First(item => item.Key.ToUpper() == Constants.JarvisPassouts.ToUpper()).Value?.First().ToObject<JObject>();
                            retrieveInc?.Merge(record);
                            if (retrieveInc != null)
                            {
                                this.retrieveList.First(item => item.Key.ToUpper() == Constants.JarvisPassouts.ToUpper()).Value?.RemoveAll();
                                this.retrieveList.First(item => item.Key.ToUpper() == Constants.JarvisPassouts.ToUpper()).Value?.Add(retrieveInc);
                            }
                        }
                        else
                        {
                            ////Getting All Master Data and Related Queries in single call aganist unique identifier from the payload "id".
                            await this.GetConfigMasterDataAsBatch(passOutId.ToString().ToLower(), modifiedBy);
                        }
                    }
                    else
                    {
                        ////Getting All Master Data and Related Queries in single call aganist unique identifier from the payload "id".
                        await this.GetConfigMasterDataAsBatch(passOutId.ToString().ToLower(), modifiedBy);
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

                            ////Retriving active Passouts of incident and IntegrationMappings of Passouts
                            if (intConfigRecord.TryGetValue(Constants.JarvisPassouts, StringComparison.OrdinalIgnoreCase, out JToken? passOutMappings))
                            {
                                ////Passouts to sRBdPassoutLoginfoArgus Payload generation.
                                JArray passOutPayload = this.PassOutToSRBdPassoutLoginfoArgus(passOutMappings.ToObject<JObject>());
                                JObject keyValues = new ()
                                {
                                    { SiebelConstants.SRBdPassoutLoginfoArgus, passOutPayload },
                                };
                                this.sbMessage.Add(SiebelConstants.ListOfSRBdPassoutLoginfoArgus, keyValues);
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

        /// <summary>
        /// Get ConfigMasterDat as Batch for AddPassOutOutbound.
        /// </summary>
        /// <param name="sourcePassOutId">sourcePassOutId.</param>
        /// <param name="modifiedBy">Modified by.</param>
        /// <returns>Async Task.</returns>
        /// <exception cref="ArgumentException">Argument Null Exception.</exception>
        private async Task GetConfigMasterDataAsBatch(string sourcePassOutId, string? modifiedBy)
        {
            try
            {
                List<HttpMessageContent> masterContents = new List<HttpMessageContent>();
                this.logger.LogTrace("Enter into ConfigurationMaster Data");
                //// Used configuration code to retrieve Config record
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.JarvisIntegrationConfiguration, string.Format(CrmQueries.JarvisIntegrationConfigurationQuery, "ADDPASSOUT002".ToString().ToUpper()), 1, string.Empty, false));
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.JarvisPassouts, string.Format(CrmQueries.PassOutSiebelOutboundUpdateQuery, sourcePassOutId), 1, string.Empty, false));
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.Transactioncurrencies, CrmQueries.JarvisStatecodeActiveQuery, 1, string.Empty, false));
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
        /// PassOuts to SRBdPassoutLoginfoArgus.
        /// </summary>
        /// <param name="mappings">Mapping.</param>
        /// <returns>Jarray.</returns>
        /// <exception cref="ArgumentException">Argument Null Exception.</exception>
        private JArray PassOutToSRBdPassoutLoginfoArgus(JObject? mappings)
        {
            try
            {
                ////Retrive Incident Payload from Retrieve List
                JArray passOutList = new JArray();
                string? sourceEntity = DynamicsApiHelper.GetStringValueFromJObject(mappings, Constants.SourceEntityName).ToLower();
                var matchMasterRecord = this.retrieveList.First(item => item.Key.ToUpper().Contains(sourceEntity.ToUpper())).Value?.ToList();
                if (matchMasterRecord != null && matchMasterRecord.Count > 0)
                {
                    foreach (var item in matchMasterRecord)
                    {
                        if (mappings != null && mappings.TryGetValue(Constants.JarvisFieldMappings, out JToken? fieldMapping) && fieldMapping != null)
                        {
                            this.helper.ValidateSetFieldMappingOutbound(item.ToObject<JObject>(), fieldMapping);
                            JObject payload = this.helper.SetFieldMappingOutbound(item.ToObject<JObject>(), fieldMapping, this.retrieveList);
                            ////Cancel RD logic.
                            ////StatusCode == CANCELLED
                            if (item.SelectToken("statuscode") != null && Convert.ToInt64(item.SelectToken("statuscode")?.ToString()) == 2)
                            {
                                payload.SelectToken(SiebelConstants.HomeDealerNotUsedFlg)?.Replace("Y");
                                payload.SelectToken(SiebelConstants.PassOutAmountBDARGUS)?.Replace(0);
                                payload.SelectToken(SiebelConstants.PassOutAmountCurrencyBDARGUS)?.Replace(null);
                            }

                            passOutList.Add(payload);
                        }
                    }
                }

                return passOutList;
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"PassOutToSRBdPassoutLoginfoArgus:" + ex.Message);
            }
        }

        /// <summary>
        /// Case to Service Request Bd Argus.
        /// </summary>
        /// <param name="mappings">Mapping.</param>
        /// <returns>Jobject.</returns>
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

                    if (mappings != null && mappings.TryGetValue(Constants.JarvisFieldMappings, out JToken? fieldMapping) && fieldMapping != null && payload != null && payload.HasValues)
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
