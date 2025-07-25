// <copyright file="AddPassoutInbound.cs" company="Microsoft">
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
    /// Add Passout Inbound.
    /// </summary>
    public class AddPassoutInbound
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
        private readonly Dictionary<string, JArray?> retrieveList = new ();
        private int counter = 0;
        private List<HttpMessageContent> multipartContent = new ();
        private bool isCreate = false;
        private string passoutId = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddPassoutInbound"/> class.
        /// </summary>
        /// <param name="dynamicsApiClient">Dynamics Client.</param>
        /// <param name="logger">Logger.</param>
        public AddPassoutInbound(IDynamicsApiClient dynamicsApiClient, ILoggerService logger)
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
        /// <param name="eventTimestamp">Event Time stamp.</param>
        /// <returns>Task.</returns>
        /// <exception cref="ArgumentException">Argument Exception.</exception>
        public async Task<HttpResponseMessage> IntegrationProcessAsync(JObject payLoad, string caseNumberArgus, string caseNumberJarvis, DateTime eventTimestamp)
        {
            try
            {
                ////Checking Payload contains unique identifier
                if (payLoad != null && !string.IsNullOrEmpty(caseNumberArgus))
                {
                    if (payLoad.TryGetValue(SiebelConstants.TdiPartner, StringComparison.OrdinalIgnoreCase, out JToken? bpResponsibleUnit) &&
                        payLoad.TryGetValue(SiebelConstants.TdiMarket, StringComparison.OrdinalIgnoreCase, out JToken? bpRetailCountry))
                    {
                        ////Getting All Master Data and Related Queries in single call aganist unique identifier from the payload "id".
                        await this.GetConfigMasterDataAsBatch(caseNumberArgus.ToString(), caseNumberJarvis, bpResponsibleUnit.ToString(), bpRetailCountry.ToString());

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
                            if (jarvis_congif != null && jarvis_congif.TryGetValue(Constants.JarvisPassouts, StringComparison.OrdinalIgnoreCase, out JToken? caseconfigMapping))
                            {
                                this.helper.ValidateSetFieldMapping(payLoad, caseconfigMapping, this.retrieveList);
                                payLoad.TryGetValue(SiebelConstants.HomeDealerNotUsedFlg, StringComparison.OrdinalIgnoreCase, out JToken? homeDealerNotUsedFlg);
                                payLoad.TryGetValue(SiebelConstants.PassOutAmountCurrencyBDARGUS, StringComparison.OrdinalIgnoreCase, out JToken? passOutAmountCurrency);

                                if (homeDealerNotUsedFlg != null && homeDealerNotUsedFlg.ToString().ToUpper() == "N")
                                {
                                    if (string.IsNullOrEmpty(passOutAmountCurrency?.ToString()))
                                    {
                                        this.logger.LogException(new ArgumentException("PassOutAmountCurrencyBDARGUS is a mandatory field."));
                                        throw new ArgumentException("PassOutAmountCurrencyBDARGUS is a mandatory field.");
                                    }

                                    if (string.IsNullOrEmpty(payLoad[SiebelConstants.PassOutAmountBDARGUS]?.ToString()))
                                    {
                                        this.logger.LogException(new ArgumentException("PassOutAmountBDARGUS is a mandatory field."));
                                        throw new ArgumentException("PassOutAmountBDARGUS is a mandatory field.");
                                    }
                                }
#pragma warning disable S1481 // Unused local variables should be removed
                                var (contentId, targetEntity) = this.ServiceRequestBdArgusToCase(payLoad, caseconfigMapping.ToObject<JObject>(), incidentid, homeDealerNotUsedFlg == null ? string.Empty : homeDealerNotUsedFlg.ToString(), eventTimestamp);
#pragma warning restore S1481 // Unused local variables should be removed

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
                                    if (this.isCreate && homeDealerNotUsedFlg != null && homeDealerNotUsedFlg.ToString().ToUpper() == "Y" && (string.IsNullOrEmpty(payLoad[SiebelConstants.PassOutAmountBDARGUS]?.ToString()) || payLoad[SiebelConstants.PassOutAmountBDARGUS]?.ToString() == "0") && targetEntity != null)
                                    {
                                        JObject payload = new ()
                                        {
                                            { SiebelConstants.Statecode, 1 },
                                        };
                                        this.multipartContent = new ()
                                        {
                                            DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Patch, targetEntity, "(" + this.passoutId + ")", Interlocked.Increment(ref this.counter), payload.ToString(), false),
                                        };
                                        response = await this.dynamicsApiClient.ExecuteBatchRequest(this.multipartContent);
                                    }

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
        /// LookupValueFromTarget - Return guid and true for the record matched from Jarvis else return false.
        /// </summary>
        /// <param name="retrieveList">Retrive List.</param>
        /// <param name="sourcePayload">Source Payload.</param>
        /// <param name="sourceField">Source Field.</param>
        /// <param name="targetLookupEntity">Target Lookup Enitity.</param>
        /// <param name="targetFieldSchema">Target Field Schema.</param>
        /// <param name="targetEntityName">Target Entity Name.</param>
        /// <returns>JToken.</returns>
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
        /// ServiceRequestBdArgusToCaseMethod.
        /// </summary>
        /// <param name="currentPayload">Current Payload.</param>
        /// <param name="mappings">Mapping.</param>
        /// <param name="incidentid">Incident Id.</param>
        /// <returns>Task.</returns>
        /// <exception cref="ArgumentException">Argument Exception.</exception>
        private (int, string?) ServiceRequestBdArgusToCase(JObject currentPayload, JObject? mappings, string incidentid, string homeDealerNotUsedFlg, DateTime eventTimestamp)
        {
            try
            {
                JObject payload = new ();
                string recordkey;
                //// Planing to use for Update Case logic.
                string? sourceField = DynamicsApiHelper.GetStringValueFromJObject(mappings, Constants.SourceFieldSchema);
                string? targetEntity = DynamicsApiHelper.GetStringValueFromJObject(mappings, Constants.TargetEntityName).ToLower();
                string? targetLookupEntityName = DynamicsApiHelper.GetStringValueFromJObject(mappings, Constants.TargetLookupEntityName).ToLower();
                string? targetFieldSchema = DynamicsApiHelper.GetStringValueFromJObject(mappings, Constants.TargetLookupFieldSchema);
                string? targetLookupEntity = DynamicsApiHelper.GetStringValueFromJObject(mappings, Constants.TargetLookupEntity);
                ////Retrieving Brands Match from the paylaod to check is present in CRM BPBrands.
                var retrievePassOut = this.LookupValueFromList(this.retrieveList, currentPayload, sourceField, targetLookupEntity, targetFieldSchema, targetLookupEntityName);
                if (!this.isCreate && currentPayload != null && targetEntity != null && (retrievePassOut.Item1 && retrievePassOut.value != null))
                {
                    recordkey = retrievePassOut.value.ToString();
                    var passoutItems = this.retrieveList.First(item => item.Key.ToUpper().Contains(Constants.JarvisPassouts.ToUpper())).Value;
                    var passout = passoutItems?.FirstOrDefault(x => x[targetLookupEntityName]?.ToString() == recordkey);
                    var lastUpdateTimeStamp = passout?.SelectToken(SiebelConstants.JarvisPassoutupdatetimestamp);
                    if (lastUpdateTimeStamp != null && !string.IsNullOrEmpty(lastUpdateTimeStamp.ToString()))
                    {
                        DateTime updateTimeStamp = DateTime.Parse(lastUpdateTimeStamp.ToString());
                        int result = DateTime.Compare(eventTimestamp, updateTimeStamp);
                        if (result <= 0)
                        {
                            this.logger.LogException(new Exception("Payload Timesamp should be greater than Pass Out's Timestamp in Jarvis"));
                            throw new ArgumentException("Payload Timestamp should be greater than Pass Out's Timestamp in Jarvis");
                        }
                    }
                }
                else
                {
                    this.isCreate = true;
                    recordkey = Guid.NewGuid().ToString();
                    this.passoutId = recordkey;
                }
                //// Framing Dealers to Account Payload
                if (mappings != null && mappings.TryGetValue(Constants.JarvisFieldMappings, out JToken? fieldMapping) && fieldMapping != null && currentPayload != null)
                {
                    payload = this.helper.SetFieldMapping(currentPayload, fieldMapping, this.retrieveList);
                    payload.Add(SiebelConstants.JarvisIncidentBind, string.Format("/{0}({1})", Constants.Incidents, incidentid));
                    payload.Add(Constants.CaseJarvisSource, 334030002);
                    payload.Add(SiebelConstants.JarvisPassoutupdatetimestamp, eventTimestamp);
                    if (string.IsNullOrEmpty(payload[SiebelConstants.JarvisGoplimitout]?.ToString()))
                    {
                        payload[SiebelConstants.JarvisGoplimitout] = 0.0;
                    }

                    if (!this.isCreate && !string.IsNullOrEmpty(homeDealerNotUsedFlg) && homeDealerNotUsedFlg.ToString().ToUpper() == "Y" && payload[SiebelConstants.JarvisGoplimitout]?.ToString() == "0")
                    {
                        payload.Add(SiebelConstants.Statecode, 1);
                    }
                    else
                    {
                        payload.Add(Constants.Statuscode, 334030002);
                    }
                }
                else
                {
                    this.logger.LogWarning("NoConfiguration field mapping is statisfied.");
                }

                // Framing Case Content for Batch
                if (payload.Count > 0 && targetEntity != null)
                {
                    this.multipartContent.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Patch, targetEntity, "(" + recordkey + ")", Interlocked.Increment(ref this.counter), payload.ToString(), false));
                }

                return (this.counter, targetEntity);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"ServiceRequestBdArgusToCase:" + ex.Message);
            }
        }

        /// <summary>
        /// Get ConfigurationMasterDataasBatch.
        /// </summary>
        /// <param name="caseNumberArgus">Case Number Argus.</param>
        /// <param name="caseNumberJarvis">Case Number Jarvis.</param>
        /// <param name="bpResponsibleUnit">Responsible Unit it.</param>
        /// <param name="bpRetailCountry">Retail Country.</param>
        /// <returns>Task.</returns>
        /// <exception cref="ArgumentException">Argument Exception.</exception>
        private async Task GetConfigMasterDataAsBatch(string caseNumberArgus, string caseNumberJarvis, string bpResponsibleUnit, string bpRetailCountry)
        {
            try
            {
                List<HttpMessageContent> masterContents = new ();
                this.logger.LogTrace("Enter into ConfigurationMaster Data");
                //// Used configuration code to retrieve Config record
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.JarvisIntegrationConfiguration, string.Format(CrmQueries.JarvisIntegrationConfigurationQuery, "ADDPASSOUT001".ToString().ToUpper()), 1, string.Empty, false));
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.Incidents, string.Format(CrmQueries.IncidentGetInboundQuery, caseNumberArgus, caseNumberJarvis), 1, string.Empty, false));
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, CtdiConstants.Transactioncurrencies, CrmQueries.JarvisStatecodeActiveQuery, 1, string.Empty, false));
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.Accounts, string.Format(CrmQueries.AccountGetQuery, bpResponsibleUnit, bpRetailCountry), 1, string.Empty, false));
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.JarvisPassouts, string.Format(CrmQueries.JarvispassoutQuery, caseNumberJarvis, caseNumberArgus), 1, string.Empty, false));
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, CtdiConstants.Teams, CrmQueries.JarvisTeamsQuery, 1, string.Empty, false));
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.JarvisBrands, CrmQueries.JarvisStatecodeActiveQuery, 1, string.Empty, false));

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