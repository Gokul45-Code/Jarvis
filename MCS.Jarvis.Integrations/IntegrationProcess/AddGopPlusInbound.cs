// <copyright file="AddGopPlusInbound.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace IntegrationProcess
{
    using IntegrationProcess.Helper.Constants;
    using MCS.Jarvis.Integration.Base.Dynamics;
    using MCS.Jarvis.Integration.Base.Logging;
    using MCS.Jarvis.IntegrationProcess.Helper;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Add GOP Plus Inbound.
    /// </summary>
    public class AddGopPlusInbound
    {
        /// <summary>
        /// DynamicsApiClient - for creating dynamics api client.
        /// </summary>
        private readonly IDynamicsApiClient dynamicsApiClient;

        /// <summary>
        /// logger object.
        /// </summary>
        private readonly ILoggerService logger;

        /// <summary>
        /// helper object.
        /// </summary>
        private readonly IntegrationHelper helper;
        private readonly List<HttpMessageContent> multipartContent = new ();
        private readonly Dictionary<string, JArray?> retrieveList = new ();
        private int counter = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddGopPlusInbound"/> class.
        /// </summary>
        /// <param name="dynamicsApiClient">Dynamics Client.</param>
        /// <param name="logger">logger.</param>
        public AddGopPlusInbound(IDynamicsApiClient dynamicsApiClient, ILoggerService logger)
        {
            this.dynamicsApiClient = dynamicsApiClient;
            this.logger = logger;
            this.helper = new IntegrationHelper(this.dynamicsApiClient, this.logger);
        }

        /// <summary>
        /// IntegrationProcessAsyncMethod.
        /// </summary>
        /// <param name="payLoad">Payload.</param>
        /// <param name="caseNumberArgus">Case Number Argus.</param>
        /// <param name="caseNumberJarvis">Case Number Jarvis.</param>
        /// <param name="eventTimestamp">event Timestamp.</param>
        /// <returns>Task.</returns>
        /// <exception cref="ArgumentException">Argument Exception.</exception>
#pragma warning disable S4457 // Parameter validation in "async"/"await" methods should be wrapped
        public async Task<HttpResponseMessage> IntegrationProcessAsync(JObject payLoad, string caseNumberArgus, string caseNumberJarvis, DateTime eventTimestamp)
#pragma warning restore S4457 // Parameter validation in "async"/"await" methods should be wrapped
        {
            try
            {
                ////Checking Payload contains unique identifier
                if (payLoad != null && !string.IsNullOrEmpty(caseNumberArgus))
                {
                    string gopType = string.Empty;
                    if (payLoad.TryGetValue(SiebelConstants.TdiPartner, StringComparison.OrdinalIgnoreCase, out JToken? bpResponsibleUnit) &&
                        payLoad.TryGetValue(SiebelConstants.TdiMarket, StringComparison.OrdinalIgnoreCase, out JToken? bpRetailCountry)
                        && payLoad.TryGetValue(SiebelConstants.Type, StringComparison.OrdinalIgnoreCase, out JToken? requestType))
                    {
                        if (!string.IsNullOrEmpty(requestType.ToString()) && requestType.ToString().ToUpper() == SiebelConstants.GopPlus.ToUpper())
                        {
                            gopType = SiebelConstants.JarvisGoprd;
                        }
                        else if (!string.IsNullOrEmpty(requestType.ToString()) && requestType.ToString().ToUpper() == SiebelConstants.GopPlusHd.ToUpper())
                        {
                            gopType = SiebelConstants.JarvisGophd;
                        }
                        else
                        {
                            this.logger.LogException(new ArgumentException("Not a valid GOP Type : " + requestType.ToString() + "."));
                            throw new ArgumentException("Not a valid GOP Type : " + requestType.ToString() + ".");
                        }
                        ////Getting All Master Data and Related Queries in single call aganist unique identifier from the payload "id".
                        await this.GetConfigMasterDataAsBatch(caseNumberArgus.ToString(), caseNumberJarvis, bpResponsibleUnit.ToString(), bpRetailCountry.ToString(), gopType);

                        var configRecord = this.retrieveList.First(item => item.Key.ToUpper() == Constants.JarvisIntegrationConfiguration.ToUpper()).Value;
                        var incident = this.retrieveList.First(item => item.Key.ToUpper().Contains(Constants.Incident.ToUpper())).Value;
                        string? incidentid = string.Empty;
                        if (incident != null && incident.Any())
                        {
                            incidentid = incident.First().SelectToken(Constants.IncidentId)?.ToString();
                        }
                        else
                        {
                            this.logger.LogException(new ArgumentException("Not a valid incident : " + caseNumberArgus + " " + caseNumberJarvis + "."));
                            throw new ArgumentException("Not a valid incident : " + caseNumberArgus + " " + caseNumberJarvis + ".");
                        }

                        if (!string.IsNullOrEmpty(incidentid) && configRecord != null && configRecord.Count > 0
                                  && configRecord.First().ToObject<JObject>().TryGetValue(Constants.JarvisIntegrationMapping, StringComparison.OrdinalIgnoreCase, out JToken? intMapping))
                        {
                            JObject jarvis_congif = JObject.Parse(intMapping.ToString());

                            ////Retriving case IntegrationMappings
                            if (jarvis_congif != null && jarvis_congif.TryGetValue(gopType, StringComparison.OrdinalIgnoreCase, out JToken? caseconfigMapping))
                            {
                                this.helper.ValidateSetFieldMapping(payLoad, caseconfigMapping, this.retrieveList);
                                if (gopType == SiebelConstants.JarvisGoprd)
                                {
#pragma warning disable S1481 // Unused local variables should be removed
                                    var (contentId, targetEntity) = this.SRBreakdownGopPlusRDArgus(payLoad, caseconfigMapping.ToObject<JObject>(), incidentid, eventTimestamp);
#pragma warning restore S1481 // Unused local variables should be removed
                                }
                                else
                                {
#pragma warning disable S1481 // Unused local variables should be removed
                                    var (contentId, targetEntity) = this.SRBreakdownGopPlusHDArgus(payLoad, caseconfigMapping.ToObject<JObject>(), incidentid, eventTimestamp);
#pragma warning restore S1481 // Unused local variables should be removed
                                }

                                //// Mercurius Brand Integration.
                                //// Calling BusinessPartnerBrands Concept
                                if (jarvis_congif.TryGetValue(Constants.Brands, StringComparison.OrdinalIgnoreCase, out JToken? brandsConfigMappings) && brandsConfigMappings != null)
                                {
                                    var bpBrands = this.helper.BrandsToBusinessPartnerBrands(brandsConfigMappings.ToObject<JObject>(), payLoad, this.retrieveList, bpResponsibleUnit.ToString(), bpRetailCountry.ToString());
                                    if (bpBrands.Item1 != null && bpBrands.Item1 != string.Empty && bpBrands.Item2 != null && bpBrands.Item2.Count > 0)
                                    {
                                        this.multipartContent.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Patch, Constants.JarvisBusinesspartnerbrandses, "(" + bpBrands.Item1 + ")", Interlocked.Increment(ref this.counter), bpBrands.Item2.ToString(), false));
                                    }
                                }

                                ////Checking MultipartContent contains Data and executing whole payload request..
                                if (this.multipartContent.Count > 0)
                                {
                                    var response = await this.dynamicsApiClient.ExecuteBatchRequest(this.multipartContent);
                                    return response == null ? new HttpResponseMessage() : response;
                                }

                                this.logger.LogException(new ArgumentException("Error in Generating Payload"));
                                throw new ArgumentException("Error in Generating Payload");
                            }

                            this.logger.LogException(new ArgumentException("No Integration Configuration Found/ Integration is not Active, Pleae activate the configuraiton record."));
                            throw new ArgumentException("Payload does not contain unique indentifier");
                        }
                        else
                        {
                            this.logger.LogException(new ArgumentException("No Integration Configuration Found/ Integration is not Active, Pleae activate the configuraiton record."));
                            throw new ArgumentException("No Integration Configuration Found/ Integration is not Active, Pleae activate the configuraiton record.");
                        }
                    }
                    else
                    {
                        this.logger.LogException(new ArgumentException("Payload does not contain unique indentifier - HomeDealerIdBDARGUS"));
                        throw new ArgumentException("Payload does not contain unique indentifier - HomeDealerIdBDARGUS");
                    }
                }
                else
                {
                    this.logger.LogException(new ArgumentException("Payload does not contain unique indentifier - CaseNumberARGUS"));
                    throw new ArgumentException("Payload does not contain unique indentifier - CaseNumberARGUS");
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"IntegrationProcessAsync:" + ex.Message);
            }
        }

        /// <summary>
        /// Lookup Value From List.
        /// </summary>
        /// <param name="retrieveList">retrieveList.</param>
        /// <param name="sourcePayload">sourcePayload.</param>
        /// <param name="sourceField">sourceField.</param>
        /// <param name="targetLookupEntity">targetLookupEntity.</param>
        /// <param name="targetFieldSchema">targetFieldSchema.</param>
        /// <param name="targetEntityName">targetEntityName.</param>
        /// <returns>JToken.</returns>
        /// <exception cref="ArgumentException">Argument Exception.</exception>
        public (bool, JToken? value) LookupValueFromList(Dictionary<string, JArray?> retrieveList, JObject sourcePayload, string sourceField, string targetLookupEntity, string targetFieldSchema, string targetEntityName)
        {
            try
            {
                var matchMasterRecord = retrieveList.First(item => item.Key.ToUpper().Contains(targetLookupEntity.ToUpper()));

                if (!string.IsNullOrEmpty(sourceField) && !string.IsNullOrEmpty(targetFieldSchema) && !string.IsNullOrEmpty(targetEntityName) && matchMasterRecord.Key != null)
                {
                    var matachingrecord = matchMasterRecord.Value?.ToList();
                    if (sourceField.Split(",").Length > 1 && targetFieldSchema.Split(",").Length > 1
                   && sourceField.Split(",").Length == targetFieldSchema.Split(",").Length)
                    {
                        for (var i = 0; i < sourceField.Split(",").Length; i++)
                        {
                            if (matachingrecord != null && matachingrecord.Count > 0)
                            {
                                var sourceValue = sourcePayload.SelectToken(sourceField.Split(",")[i]) != null ? sourcePayload.SelectToken(sourceField.Split(",")[i])?.Value<JToken>() : string.Empty;
                                if (!string.IsNullOrEmpty(sourceValue?.ToString()))
                                {
                                    var matchrecord = matachingrecord.Where(item => item.SelectToken(targetFieldSchema.Split(",")[i])?.ToString().ToUpper().Replace("-", string.Empty) == sourceValue.ToString().ToUpper().Replace("-", string.Empty)).ToList();
                                    matachingrecord = matchrecord;
                                }
                            }
                            else
                            {
                                break;
                            }
                        }

                        if (matachingrecord != null && matachingrecord.Count > 0)
                        {
                            JToken? jToken = matachingrecord.Select(item => item[targetEntityName.ToLower()]).First();
                            return (true, jToken);
                        }
                    }
                    else
                    {
                        this.logger.LogException(new ArgumentException("Not a Valid Configuration to match and retrive value from source"));
                        throw new ArgumentException($"Not a Valid Configuration to match and retrive value from source");
                    }
                }
                else
                {
                    this.logger.LogException(new ArgumentException("Not a Valid Configuration to match and retrive value from source"));
                    throw new ArgumentException($"Not a Valid Configuration to match and retrive value from source");
                }

                return (false, string.Empty);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($" Retrieving Lookup Value from Jarvis entity {targetLookupEntity} Failed with Error Message - " + ex.Message);
            }
        }

        /// <summary>
        /// SRBreakdownGopPlusRDArgus.
        /// </summary>
        /// <param name="currentPayload">Current Payload.</param>
        /// <param name="mappings">Mapping.</param>
        /// <param name="incidentid">Incident Id.</param>
        /// <returns>Target entity.</returns>
        /// <exception cref="ArgumentException">Argument Exception.</exception>
        private (int, string) SRBreakdownGopPlusRDArgus(JObject currentPayload, JObject? mappings, string incidentid, DateTime eventTimestamp)
        {
            try
            {
                JObject payload = new ();
                string? sourceField = DynamicsApiHelper.GetStringValueFromJObject(mappings, Constants.SourceFieldSchema);
                string? targetEntity = DynamicsApiHelper.GetStringValueFromJObject(mappings, Constants.TargetEntityName).ToLower();
                string? targetLookupEntityName = DynamicsApiHelper.GetStringValueFromJObject(mappings, Constants.TargetLookupEntityName).ToLower();
                string? targetFieldSchema = DynamicsApiHelper.GetStringValueFromJObject(mappings, Constants.TargetLookupFieldSchema);
                string? targetLookupEntity = DynamicsApiHelper.GetStringValueFromJObject(mappings, Constants.TargetLookupEntity);
                ////Retrieving Brands Match from the paylaod to check is present in CRM BPBrands.
                var retrieveRepairingDealer = this.LookupValueFromList(this.retrieveList, currentPayload, sourceField, targetLookupEntity, targetFieldSchema, targetLookupEntityName);
                string repairingDealerId = string.Empty;
                string recordkey;
                recordkey = Guid.NewGuid().ToString();
                if (currentPayload != null && (retrieveRepairingDealer.Item1 && retrieveRepairingDealer.value != null))
                {
                    repairingDealerId = retrieveRepairingDealer.value.ToString();
                }
                else
                {
                    this.logger.LogException(new ArgumentException("Case doesn't have a valid Passout : Repairing Dealer"));
                    throw new ArgumentException("Case doesn't have a valid Passout : Repairing Dealer");
                }
                //// Framing Dealers to Account Payload
                if (mappings != null && mappings.TryGetValue(Constants.JarvisFieldMappings, out JToken? fieldMapping) && fieldMapping != null && currentPayload != null)
                {
                    payload = this.helper.SetFieldMapping(currentPayload, fieldMapping, this.retrieveList);
                    ////Set incident id
                    ////var homeDealerId = this.retrieveList.First(item => item.Key.ToUpper().Contains(Constants.Incident.ToUpper())).Value.FirstOrDefault()?.SelectToken(SiebelConstants.JarvisHomedealerValue);
                    payload.Add(SiebelConstants.JarvisIncidentBind, string.Format("/{0}({1})", Constants.Incidents, incidentid));
                    payload.Add(SiebelConstants.RepairingDealer, string.Format(" /{0}({1})", Constants.JarvisPassouts, repairingDealerId));
                    ////payload.Add(SiebelConstants.HomeDealer, string.Format(" /{0}({1})", Constants.Accounts, homeDealerId));
                    payload.Add(Constants.CaseJarvisSource, 334030002);
                    payload.Add(SiebelConstants.JarvisGoplimitin, 0);
                    payload.Add(SiebelConstants.JarvisGopupdatetimestamp, eventTimestamp);
                }
                else
                {
                    this.logger.LogWarning("NoConfiguration field mapping is statisfied.");
                }

                // Framing Case Content for Batch
                if (payload.Count > 0)
                {
                    this.multipartContent.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Patch, targetEntity, "(" + recordkey + ")", Interlocked.Increment(ref this.counter), payload.ToString(), false));
                }

                return (this.counter, targetEntity);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"SRBreakdownGopPlusRDArgus:" + ex.Message);
            }
        }

        /// <summary>
        /// SRBreakdownGopPlusHDArgus.
        /// </summary>
        /// <param name="currentPayload">Current Payload.</param>
        /// <param name="mappings">Mapping details.</param>
        /// <param name="incidentid">Incident Id.</param>
        /// <returns>Task.</returns>
        /// <exception cref="ArgumentException">Argument Exception.</exception>
        private (int, string) SRBreakdownGopPlusHDArgus(JObject currentPayload, JObject? mappings, string incidentid, DateTime eventTimestamp)
        {
            try
            {
                JObject payload = new ();

                //// Planing to use for Add remark.
                string? targetEntity = DynamicsApiHelper.GetStringValueFromJObject(mappings, Constants.TargetEntityName).ToLower();
                string? targetLookupEntity = DynamicsApiHelper.GetStringValueFromJObject(mappings, Constants.TargetEntityName).ToLower();
                string? targetLookupEntityName = DynamicsApiHelper.GetStringValueFromJObject(mappings, Constants.TargetLookupEntityName).ToLower();
                string? recordkey;
                bool isActive = true;

                var matchMasterRecord = this.retrieveList.First(item => item.Key.ToUpper().Contains(targetLookupEntity.ToUpper()));

                if (!string.IsNullOrEmpty(targetLookupEntityName) && matchMasterRecord.Key != null)
                {
                    var matachingrecord = matchMasterRecord.Value?.ToList();
                    if (matachingrecord != null && matachingrecord.Count > 0)
                    {
                        var mercuriusGopId = currentPayload.SelectToken("Id") != null ? currentPayload.SelectToken("Id")?.ToString().ToUpper() : string.Empty;
                        var jarvisGopId = currentPayload.SelectToken("IdJARVIS") != null ? currentPayload.SelectToken("IdJARVIS")?.ToString().ToUpper() : string.Empty;
                        var gophd = matachingrecord.Where(item => (item.SelectToken("jarvis_gopid") != null && item.SelectToken("jarvis_gopid")?.ToString().ToUpper() == jarvisGopId) || (item.SelectToken("jarvis_mercuriusgopid") != null && item.SelectToken("jarvis_mercuriusgopid")?.ToString().ToUpper() == mercuriusGopId)).FirstOrDefault();
                        if (gophd != null)
                        {
                            recordkey = gophd.SelectToken(targetLookupEntityName.ToLower())?.ToString();
                            isActive = !Convert.ToBoolean(gophd.SelectToken("statecode"));
                           //// var gophd = matachingrecord.FirstOrDefault(x => x[targetLookupEntityName].ToString() == recordkey);
                            var lastUpdateTimeStamp = gophd?.SelectToken(SiebelConstants.JarvisGopupdatetimestamp);
                            if (lastUpdateTimeStamp != null && !string.IsNullOrEmpty(lastUpdateTimeStamp.ToString()))
                            {
                                DateTime updateTimeStamp = DateTime.Parse(lastUpdateTimeStamp.ToString());
                                int result = DateTime.Compare(eventTimestamp, updateTimeStamp);
                                if (result <= 0)
                                {
                                    this.logger.LogException(new Exception("Payload Timesamp should be greater than GOP's Timestamp in Jarvis"));
                                    throw new ArgumentException("Payload Timestamp should be greater than GOP's Timestamp in Jarvis");
                                }
                            }
                        }
                        else
                        {
                            recordkey = Guid.NewGuid().ToString();
                        }
                    }
                    else
                    {
                        recordkey = Guid.NewGuid().ToString();
                    }
                }
                else
                {
                    recordkey = Guid.NewGuid().ToString();
                }
                //// Framing Dealers to Account Payload
                if (mappings != null && mappings.TryGetValue(Constants.JarvisFieldMappings, out JToken? fieldMapping) && fieldMapping != null)
                {
                    payload = this.helper.SetFieldMapping(currentPayload, fieldMapping, this.retrieveList);
                    ////Set incident id
                    payload.Add(SiebelConstants.JarvisIncidentBind, string.Format("/{0}({1})", Constants.Incidents, incidentid));
                    payload.Add(Constants.CaseJarvisSource, 334030002);
                    payload.Add(SiebelConstants.JarvisGopupdatetimestamp, eventTimestamp);
                }
                else
                {
                    this.logger.LogWarning("NoConfiguration field mapping is statisfied.");
                }

                // Framing Case Content for Batch
                if (payload.Count > 0 && isActive)
                {
                    this.multipartContent.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Patch, targetEntity, "(" + recordkey + ")", Interlocked.Increment(ref this.counter), payload.ToString(), false));
                }

                return (this.counter, targetEntity);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"SRBreakdownGopPlusHDArgus:" + ex.Message);
            }
        }

        /// <summary>
        /// Get ConfigurationMasterDataasBatch.
        /// </summary>
        /// <param name="caseNumberArgus">Case Number Argus.</param>
        /// <param name="caseNumberJarvis">Case Number Jarvis.</param>
        /// <param name="bpResponsibleUnit">Responsible Unit Id.</param>
        /// <param name="bpRetailCountry">Retail Country.</param>
        /// <param name="gopType">GOP type.</param>
        /// <returns>Task.</returns>
        /// <exception cref="ArgumentException">Argument Exception.</exception>
        private async Task GetConfigMasterDataAsBatch(string caseNumberArgus, string caseNumberJarvis, string bpResponsibleUnit, string bpRetailCountry, string gopType)
        {
            try
            {
                List<HttpMessageContent> masterContents = new ();
                this.logger.LogTrace("Enter into ConfigurationMaster Data");
                //// Used configuration code to retrieve Config record
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.JarvisIntegrationConfiguration, string.Format(CrmQueries.JarvisIntegrationConfigurationQuery, "ADDGOPPLUS001".ToString().ToUpper()), 1, string.Empty, false));
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.Incidents, string.Format(CrmQueries.IncidentGopGetQuery, caseNumberArgus, caseNumberJarvis), 1, string.Empty, false));
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, CtdiConstants.Transactioncurrencies, CrmQueries.JarvisStatecodeActiveQuery, 1, string.Empty, false));
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.Accounts, string.Format(CrmQueries.AccountGetQuery, bpResponsibleUnit, bpRetailCountry), 1, string.Empty, false));
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, CtdiConstants.Teams, CrmQueries.JarvisTeamsQuery, 1, string.Empty, false));
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.JarvisBrands, CrmQueries.JarvisStatecodeActiveQuery, 1, string.Empty, false));

                if (gopType == SiebelConstants.JarvisGophd)
                {
                    masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.JarvisGops, string.Format(CrmQueries.JarvisGetUnapprovedGop, caseNumberArgus, caseNumberJarvis, bpResponsibleUnit, bpRetailCountry), 1, string.Empty, false));
                }
                else if (gopType == SiebelConstants.JarvisGoprd)
                {
                    masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.JarvisPassouts, string.Format(CrmQueries.JarvispassoutQuery, caseNumberJarvis, caseNumberArgus), 1, string.Empty, false));
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
    }
}