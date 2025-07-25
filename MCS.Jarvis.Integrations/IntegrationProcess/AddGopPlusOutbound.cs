// <copyright file="AddGopPlusOutbound.cs" company="Microsoft.">
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
    /// AddGopPlusOutbound Class.
    /// </summary>
    public class AddGopPlusOutbound
    {
        private readonly IDynamicsApiClient dynamicsApiClient;
        private readonly IConfiguration config;
        private readonly ILoggerService logger;
        private readonly IntegrationHelper helper;
        private readonly Dictionary<string, JArray?> retrieveList = new Dictionary<string, JArray?>();
        private JObject sbMessage = new ();
        private string? modifiedOn = DateTime.UtcNow.ToString();

        /// <summary>
        /// Initializes a new instance of the <see cref="AddGopPlusOutbound"/> class.
        /// AddGopPlusOutbound Constructor.
        /// </summary>
        /// <param name="dynamicsApiClient">DynamicsApiClient.</param>
        /// <param name="config">Config.</param>
        /// <param name="logger">Logger.</param>
        public AddGopPlusOutbound(IDynamicsApiClient dynamicsApiClient, IConfiguration config, ILoggerService logger)
        {
            this.dynamicsApiClient = dynamicsApiClient;
            this.config = config;
            this.logger = logger;
            this.helper = new IntegrationHelper(this.dynamicsApiClient, this.logger);
        }

        /// <summary>
        /// IntegrationProcessAsync.
        /// </summary>
        /// <param name="gopId">GopId.</param>
        /// <param name="entityData">EntityData.</param>
        /// <param name="gopPlusType">gopPlusType.</param>
        /// <returns>Async Task.</returns>
        /// <exception cref="ArgumentException">Argument Null Exception.</exception>
        public async Task<JObject> IntegrationProcessAsync(string gopId, string entityData, string gopPlusType)
        {
            try
            {
                ////Checking Payload contains unique identifier
                if (!string.IsNullOrEmpty(gopId))
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
                            await this.GetConfigMasterDataAsBatch(gopId.ToString().ToLower(), gopPlusType, modifiedBy);
                            var retrieveGop = this.retrieveList.First(item => item.Key.ToUpper() == Constants.JarvisGops.ToUpper()).Value?.First().ToObject<JObject>();
                            retrieveGop?.Merge(record);
                            if (retrieveGop != null)
                            {
                                this.retrieveList.First(item => item.Key.ToUpper() == Constants.JarvisGops.ToUpper()).Value?.RemoveAll();
                                this.retrieveList.First(item => item.Key.ToUpper() == Constants.JarvisGops.ToUpper()).Value?.Add(retrieveGop);
                            }
                        }
                        else
                        {
                            ////Getting All Master Data and Related Queries in single call aganist unique identifier from the payload "id".
                            await this.GetConfigMasterDataAsBatch(gopId.ToString().ToLower(), gopPlusType, modifiedBy);
                        }
                    }
                    else
                    {
                        ////Getting All Master Data and Related Queries in single call aganist unique identifier from the payload "id".
                        await this.GetConfigMasterDataAsBatch(gopId.ToString().ToLower(), gopPlusType, modifiedBy);
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

                            ////Retriving active GopHD or GopRD Plus of incident and IntegrationMappings of Gop
                            if (intConfigRecord.TryGetValue(gopPlusType.ToString(), StringComparison.OrdinalIgnoreCase, out JToken? gopMappings))
                            {
                                ////Incident.Active.GopsHD or GopRD to SRBreakdownGopPlusArgus Payload generation.
                                JArray gopsPayload = this.GopToSRBreakdownGopPlusArgus(gopMappings.ToObject<JObject>(), gopPlusType);
                                JObject keyValues = new ()
                                {
                                    { SiebelConstants.SRBreakdownGopPlusArgus, gopsPayload },
                                };
                                this.sbMessage.Add(SiebelConstants.ListOfSRBreakdownGopPlusArgus, keyValues);
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
        /// GetConfigMasterDataAsBatch.
        /// </summary>
        /// <param name="sourceGopId">SourceGopId.</param>
        /// <param name="gopPlusType">GopPlusType.</param>
        /// <param name="modifiedBy">Modified By.</param>
        /// <returns>AsyncTask.</returns>
        /// <exception cref="ArgumentException">Argument Null Exception.</exception>
        private async Task GetConfigMasterDataAsBatch(string sourceGopId, string gopPlusType, string? modifiedBy)
        {
            try
            {
                List<HttpMessageContent> masterContents = new List<HttpMessageContent>();
                this.logger.LogTrace("Enter into ConfigurationMaster Data");
                //// Used configuration code to retrieve Config record
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.JarvisIntegrationConfiguration, string.Format(CrmQueries.JarvisIntegrationConfigurationQuery, "ADDGOPPLUS002".ToString().ToUpper()), 1, string.Empty, false));
                if (gopPlusType == GopRequestType.GOPHD)
                {
                    masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.JarvisGops, string.Format(CrmQueries.GopPlusHDSiebelOutboundUpdateQuery, sourceGopId), 1, string.Empty, false));
                }
                else
                {
                    masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.JarvisGops, string.Format(CrmQueries.GopPlusRDSiebelOutboundUpdateQuery, sourceGopId), 1, string.Empty, false));
                }

                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.Transactioncurrencies, CrmQueries.JarvisStatecodeActiveQuery, 1, string.Empty, false));
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.JarvisBrands, CrmQueries.JarvisStatecodeActiveQuery, 1, string.Empty, false));
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.JarvisCountries, CrmQueries.JarvisStatecodeActiveQuery, 1, string.Empty, false));
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.JarvisGopTranslations, string.Format(CrmQueries.GetGOPTranslation, sourceGopId), 1, string.Empty, false));
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
        /// Gop Plus to SRBdGopInLoginfoArgus.
        /// </summary>
        /// <param name="mappings">Mapping.</param>
        /// <returns>Jarray.</returns>
        /// <exception cref="ArgumentException">Argument Null Exception.</exception>
        private JArray GopToSRBreakdownGopPlusArgus(JObject? mappings, string gopPlusType)
        {
            try
            {
                ////Retrive Incident Payload from Retrieve List
                JArray gopList = new JArray();
                string? sourceEntity = DynamicsApiHelper.GetStringValueFromJObject(mappings, Constants.SourceEntityName).ToLower();
                var matchMasterRecord = this.retrieveList.First(item => item.Key.ToUpper().Contains(sourceEntity.ToUpper())).Value?.ToList();
                if (matchMasterRecord != null && matchMasterRecord.Count > 0)
                {
                    foreach (var item in matchMasterRecord)
                    {
                        if (mappings != null && mappings.TryGetValue(Constants.JarvisFieldMappings, out JToken? fieldMapping) && fieldMapping != null)
                        {
                            JToken? shawdowGopComment = null;
                            this.helper.ValidateSetFieldMappingOutbound(item.ToObject<JObject>(), fieldMapping);
                            JObject payload = this.helper.SetFieldMappingOutbound(item.ToObject<JObject>(), fieldMapping, this.retrieveList);
                            var gopTranslation = this.retrieveList.First(item => item.Key.ToUpper().Contains(Constants.JarvisGopTranslations.ToUpper())).Value?.ToList();
                            item.ToObject<JObject>()?.TryGetValue(SiebelConstants.JarvisShadowGopcomment, StringComparison.OrdinalIgnoreCase, out shawdowGopComment);
                            if (gopTranslation != null && gopTranslation.Count > 0)
                            {
                                if (gopPlusType == GopRequestType.GOPHD)
                                {
                                    var matchedGopTranslation = gopTranslation.Where(x => item["jarvis_Dealer"]?["_jarvis_language_value"] != null && x["_jarvis_language_value"]?.ToString() == item["jarvis_Dealer"]?["_jarvis_language_value"]?.ToString()).FirstOrDefault();
                                    if (matchedGopTranslation != null)
                                    {
                                        payload["Comment"] = matchedGopTranslation["jarvis_gopcomment"];
                                    }

                                    if (shawdowGopComment != null && !string.IsNullOrEmpty(shawdowGopComment.ToString()))
                                    {
                                        payload["Comment"] = string.Format("{0} {1}", shawdowGopComment.ToString(), payload["Comment"]?.ToString()).Trim();
                                    }
                                }
                                else
                                {
                                    var matchedGopTranslation = gopTranslation.Where(x => item["jarvis_RepairingDealer"]?["jarvis_RepairingDealer"]?["_jarvis_language_value"] != null && x["_jarvis_language_value"]?.ToString() == item["jarvis_RepairingDealer"]?["jarvis_RepairingDealer"]?["_jarvis_language_value"]?.ToString()).FirstOrDefault();
                                    if (matchedGopTranslation != null)
                                    {
                                        payload["Comment"] = matchedGopTranslation["jarvis_gopreason"];
                                    }
                                }
                            }

                            if (payload != null && gopPlusType == GopRequestType.GOPHD && payload.SelectToken(SiebelConstants.HDContactArgus) != null && payload.SelectToken(SiebelConstants.HDContactArgus)?.ToString().Length > 50)
                            {
                                payload[SiebelConstants.HDContactArgus] = payload.SelectToken(SiebelConstants.HDContactArgus)?.ToString().Substring(0, 50);
                            }

                            gopList.Add(payload);
                        }
                    }
                }

                return gopList;
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"GopToSRBreakdownGopPlusArgus:" + ex.Message);
            }
        }

        /// <summary>
        /// Case to Service Request Bd Argus.
        /// </summary>
        /// <param name="mappings">Mappping.</param>
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
