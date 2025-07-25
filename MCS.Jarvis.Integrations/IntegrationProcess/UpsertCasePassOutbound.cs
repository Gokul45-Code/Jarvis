// <copyright file="UpsertCasePassOutbound.cs" company="Microsoft">
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
    /// UpsertCasePassOutbound Class.
    /// </summary>
    public class UpsertCasePassOutbound
    {
        private readonly IDynamicsApiClient dynamicsApiClient;
        private readonly IConfiguration config;
        private readonly ILoggerService logger;
        private readonly IntegrationHelper helper;
        private readonly Dictionary<string, JArray?> retrieveList = new Dictionary<string, JArray?>();
        private JObject sbMessage = new ();
        private string? modifiedOn = DateTime.UtcNow.ToString();

        /// <summary>
        /// Initializes a new instance of the <see cref="UpsertCasePassOutbound"/> class.
        /// UpsertCasePassOutbound Constructor.
        /// </summary>
        /// <param name="dynamicsApiClient">DynamicsApiClient.</param>
        /// <param name="config">Config.</param>
        /// <param name="logger">Logger.</param>
        public UpsertCasePassOutbound(IDynamicsApiClient dynamicsApiClient, IConfiguration config, ILoggerService logger)
        {
            this.dynamicsApiClient = dynamicsApiClient;
            this.config = config;
            this.logger = logger;
            this.helper = new IntegrationHelper(this.dynamicsApiClient, this.logger);
        }

        /// <summary>
        /// UpsertCasePassOutbound IntegrationProcess Async.
        /// </summary>
        /// <param name="passOutId">PassOut.</param>
        /// <param name="entityData">EntityData.</param>
        /// <param name="transType">TransType.</param>
        /// <returns>Task.</returns>
        /// <exception cref="ArgumentException">Argument Null Exception.</exception>
        public async Task<JObject> IntegrationProcessAsync(string passOutId, string entityData, string transType)
        {
            try
            {
                ////Checking Payload contains unique identifier
                if (!string.IsNullOrEmpty(passOutId) && !string.IsNullOrEmpty(transType))
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
                            await this.GetConfigMasterDataAsBatch(passOutId, modifiedBy);
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
                            await this.GetConfigMasterDataAsBatch(passOutId, modifiedBy);
                        }
                    }
                    else
                    {
                        ////Getting All Master Data and Related Queries in single call aganist unique identifier from the payload "id".
                        await this.GetConfigMasterDataAsBatch(passOutId, modifiedBy);
                    }

                    var configRecord = this.retrieveList.First(item => item.Key.ToUpper() == Constants.JarvisIntegrationConfiguration.ToUpper()).Value;
                    if (configRecord != null && configRecord.Count > 0
                          && configRecord.First().ToObject<JObject>().TryGetValue(Constants.JarvisIntegrationMapping, StringComparison.OrdinalIgnoreCase, out JToken? intConfig))
                    {
                        JObject intConfigRecord = JObject.Parse(intConfig.ToString());

                        if (intConfigRecord != null && intConfigRecord.TryGetValue(Constants.Incident, StringComparison.OrdinalIgnoreCase, out JToken? intMapping))
                        {
                            this.sbMessage = this.CaseToServiceRequestBdArgus(intMapping.ToObject<JObject>());
                        }
                        ////Retriving PassOut IntegrationMappings
                        if (intConfigRecord != null && intConfigRecord.TryGetValue(transType, StringComparison.OrdinalIgnoreCase, out JToken? passOutMapping))
                        {
                            //// ServiceRequestBdArgus to PassOut Payload generation
#pragma warning disable S1481 // Unused local variables should be removed
                            JArray passOutPayload = this.PassOutToSRBdPassoutLoginfoArgus(passOutId, passOutMapping.ToObject<JObject>());
                            JObject keyValues = new ()
                                {
                                    { SiebelConstants.SRBdPassoutLoginfoArgus, passOutPayload },
                                };
                            this.sbMessage.Add(SiebelConstants.ListOfSRBdPassoutLoginfoArgus, keyValues);

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
        /// Get Configuration Data and Master Data.
        /// </summary>
        /// <param name="passOutId">PassOutId.</param>
        /// <param name="modifiedBy">Modified By.</param>
        /// <returns>Task.</returns>
        /// <exception cref="ArgumentException">Argument Null Exception.</exception>
        private async Task GetConfigMasterDataAsBatch(string passOutId, string? modifiedBy)
        {
            try
            {
                List<HttpMessageContent> masterContents = new List<HttpMessageContent>();
                this.logger.LogTrace("Enter into ConfigurationMaster Data");
                //// Used configuration code to retrieve Config record
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.JarvisIntegrationConfiguration, string.Format(CrmQueries.JarvisIntegrationConfigurationQuery, "JarvisPassOutTime001".ToString().ToUpper()), 1, string.Empty, false));
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.JarvisPassouts, string.Format(CrmQueries.PassoutDateSiebelOutboundQuery, passOutId), 1, string.Empty, false, true));
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
                    this.logger.LogException(new ArgumentException("Getting IntegrationConfiguration,MasterData and PassOut record Failed"));
                    throw new ArgumentException("No MasterData,Configuration Record Retrieved");
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"GetConfigMasterDataAsBatch:" + ex.Message);
            }
        }

        /// <summary>
        /// PassOutToSRBdPassoutLoginfoArgus.
        /// </summary>
        /// <param name="passOutId">PassOutId.</param>
        /// <param name="mappings">Mappings.</param>
        /// <returns>JArray.</returns>
        /// <exception cref="ArgumentException">Argument Null Exception.</exception>
        private JArray PassOutToSRBdPassoutLoginfoArgus(string passOutId, JObject? mappings)
        {
            try
            {
                ////Retrive PassOut Payload from Retrieve List
                JObject? payload;
                JArray passOutList = new JArray();
                string? sourceEntity = DynamicsApiHelper.GetStringValueFromJObject(mappings, Constants.SourceEntityName).ToLower();
                string? sourceFieldSchema = DynamicsApiHelper.GetStringValueFromJObject(mappings, Constants.SourceFieldSchema);

                var matchMasterRecord = this.retrieveList.First(item => item.Key.ToUpper().Contains(sourceEntity.ToUpper())).Value?.ToList();
                if (matchMasterRecord != null && matchMasterRecord.Count > 0)
                {
                    payload = matchMasterRecord.First(item => item[sourceFieldSchema]?.ToString().ToLower() == passOutId).ToObject<JObject>();
                    if (mappings != null && mappings.TryGetValue(Constants.JarvisFieldMappings, out JToken? fieldMapping) && fieldMapping != null && payload != null && payload.HasValues)
                    {
                        this.helper.ValidateSetFieldMappingOutbound(payload, fieldMapping);
                        JObject passOutPayload = this.helper.SetFieldMappingOutbound(payload, fieldMapping, this.retrieveList);
                        passOutList.Add(passOutPayload);
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
