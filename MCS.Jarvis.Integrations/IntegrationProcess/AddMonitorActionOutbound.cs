// <copyright file="AddMonitorActionOutbound.cs" company="Microsoft.">
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
    /// AddMonitorActionOutbound.
    /// </summary>
    public class AddMonitorActionOutbound
    {
        private readonly IDynamicsApiClient dynamicsApiClient;
        private readonly IConfiguration config;
        private readonly ILoggerService logger;
        private readonly IntegrationHelper helper;
        private readonly Dictionary<string, JArray?> retrieveList = new Dictionary<string, JArray?>();
        private JObject sbMessage = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="AddMonitorActionOutbound"/> class.
        /// AddPassOutOutbound Constructor.
        /// </summary>
        /// <param name="dynamicsApiClient">DynamicsApiClient.</param>
        /// <param name="config">Config.</param>
        /// <param name="logger">Logger.</param>
        public AddMonitorActionOutbound(IDynamicsApiClient dynamicsApiClient, IConfiguration config, ILoggerService logger)
        {
            this.dynamicsApiClient = dynamicsApiClient;
            this.config = config;
            this.logger = logger;
            this.helper = new IntegrationHelper(this.dynamicsApiClient, this.logger);
        }

        /// <summary>
        /// IntegrationProcessAsync.
        /// </summary>
        /// <param name="monitorActionId">MonitorActionId.</param>
        /// <param name="entityData">EntityData.</param>
        /// <returns>Jobject.</returns>
        /// <exception cref="ArgumentException">Argument Null Exceptions.</exception>
        public async Task<JObject> IntegrationProcessAsync(string monitorActionId, string entityData)
        {
            try
            {
                ////Checking Payload contains unique identifier
                if (!string.IsNullOrEmpty(monitorActionId))
                {
                    ////Getting All Master Data and Related Queries in single call aganist unique identifier from the payload "id".
                    await this.GetConfigMasterDataAsBatch(monitorActionId.ToString().ToLower());

                    if (!string.IsNullOrEmpty(entityData))
                    {
                        this.logger.LogTrace($"PluginContextData: {entityData}");
                        JObject record = this.helper.ConvertContextToObject(entityData);
                        if (record.HasValues)
                        {
                            var retrieveInc = this.retrieveList.First(item => item.Key.ToUpper() == Constants.Incidents.ToUpper()).Value?.First().ToObject<JObject>();
                            retrieveInc?.Merge(record);
                            this.retrieveList.First(item => item.Key.ToUpper() == Constants.Incidents.ToUpper()).Value?.RemoveAll();
                            this.retrieveList.First(item => item.Key.ToUpper() == Constants.Incidents.ToUpper()).Value?.Add(retrieveInc);
                        }
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

                            if (intConfigRecord.TryGetValue(Constants.JarvisCasemonitoractions, StringComparison.OrdinalIgnoreCase, out JToken? monitorActionMapping))
                            {
                                ////CaseMonitorAction To ActionBdMonitorHistoryArgus Payload generation.
                                JArray monitorActionPayload = this.CaseMonitorActionToActionBdMonitorHistoryArgus(monitorActionMapping.ToObject<JObject>());
                                JObject keyValues = new()
                                {
                                    { SiebelConstants.ActionBdMonitorHistoryArgus, monitorActionPayload },
                                };
                                this.sbMessage.Add(SiebelConstants.ListOfActionBdMonitorHistoryArgus, keyValues);
                            }
#pragma warning restore S1481 // Unused local variables should be removed

                            ////Checking MultipartContent contains Data and executing whole payload request..
                            if (this.sbMessage.HasValues)
                            {
                                return this.sbMessage;
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
        /// GetConfigMasterDataAsBatch.
        /// </summary>
        /// <param name="sourceMontiorActionId">SourceMonitorActionId.</param>
        /// <returns>Task.</returns>
        /// <exception cref="ArgumentException">Argument Null Exceptions.</exception>
        private async Task GetConfigMasterDataAsBatch(string sourceMontiorActionId)
        {
            try
            {
                List<HttpMessageContent> masterContents = new List<HttpMessageContent>();
                this.logger.LogTrace("Enter into ConfigurationMaster Data");
                //// Used configuration code to retrieve Config record
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.JarvisIntegrationConfiguration, string.Format(CrmQueries.JarvisIntegrationConfigurationQuery, "CASEMONITORACTION002".ToString().ToUpper()), 1, string.Empty, false));
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.Incidents, string.Format(CrmQueries.MonitorActionOutboundQuery, sourceMontiorActionId), 1, string.Empty, false));
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
        /// CaseMonitorAction To ActionBdMonitorHistoryArgus.
        /// </summary>
        /// <param name="mappings">Mappings.</param>
        /// <returns>Jarray.</returns>
        /// <exception cref="ArgumentException">Argument Null Exception.</exception>
        private JArray CaseMonitorActionToActionBdMonitorHistoryArgus(JObject? mappings)
        {
            try
            {
                ////Retrive Case Monitor Action Payload from Retrieve List
                JArray monitorActionList = new JArray();
                string dateTimeValue = string.Empty;
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
                            //// Merging DATE and Time of seperate field into single field for the Payload.
                            if (payload.TryGetValue(SiebelConstants.FuDateArgus, StringComparison.OrdinalIgnoreCase, out JToken? date) && !string.IsNullOrEmpty(date.ToString()))
                            {
                                if (item.ToObject<JObject>().TryGetValue(SiebelConstants.PriorityCode, StringComparison.OrdinalIgnoreCase, out JToken? priorityCode) && !string.IsNullOrEmpty(priorityCode.ToString()))
                                {
                                    string? nullData = null;
                                    this.logger.LogTrace($"priority code comparision:{priorityCode.ToString()}, {CasePriorityCode.High}");
                                    if (priorityCode.ToString() == CasePriorityCode.High)
                                    {
                                        payload.SelectToken(SiebelConstants.FuDateArgus)?.Replace(nullData);
                                    }
                                    else if (priorityCode.ToString() == CasePriorityCode.Medium)
                                    {
                                        payload.SelectToken(SiebelConstants.SpecialAttentionARGUS)?.Replace("Y");
                                    }
                                }
                            }

                            monitorActionList.Add(payload);
                        }
                    }
                }

                return monitorActionList;
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"CaseMonitorActionToActionBdMonitorHistoryArgus:" + ex.Message);
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
