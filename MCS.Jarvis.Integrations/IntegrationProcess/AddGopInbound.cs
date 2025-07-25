// <copyright file="AddGopInbound.cs" company="Microsoft">
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
    /// Add GOP Inbound.
    /// </summary>
    public class AddGopInbound
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
        private string gopId = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddGopInbound"/> class.
        /// </summary>
        /// <param name="dynamicsApiClient">Dynamics Client.</param>
        /// <param name="logger">Logger.</param>
        public AddGopInbound(IDynamicsApiClient dynamicsApiClient, ILoggerService logger)
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
                            if (jarvis_congif != null && jarvis_congif.TryGetValue(Constants.JarvisGops, StringComparison.OrdinalIgnoreCase, out JToken? caseconfigMapping))
                            {
                                this.helper.ValidateSetFieldMapping(payLoad, caseconfigMapping, this.retrieveList);
                                var limitInCurrencyBDARGUS = payLoad.SelectToken(SiebelConstants.LimitInCurrencyBDARGUS);
                                var limitOutCurrencyBDARGUS = payLoad.SelectToken(SiebelConstants.LimitOutCurrencyBDARGUS);
                                var homeDealerNotUsedFlg = payLoad.SelectToken(SiebelConstants.HomeDealerNotUsedFlg);
                                if (homeDealerNotUsedFlg != null && homeDealerNotUsedFlg.ToString().ToUpper() == "N")
                                {
                                    if (string.IsNullOrEmpty(limitInCurrencyBDARGUS?.ToString()))
                                    {
                                        this.logger.LogException(new ArgumentException("LimitInCurrencyBDARGUS is a mandatory field."));
                                        throw new ArgumentException("LimitInCurrencyBDARGUS is a mandatory field.");
                                    }

                                    if (string.IsNullOrEmpty(limitOutCurrencyBDARGUS?.ToString()))
                                    {
                                        this.logger.LogException(new ArgumentException("LimitOutCurrencyBDARGUS is a mandatory field."));
                                        throw new ArgumentException("LimitOutCurrencyBDARGUS is a mandatory field.");
                                    }

                                    if (string.IsNullOrEmpty(payLoad[SiebelConstants.LimitInAmountBDARGUS]?.ToString()))
                                    {
                                        this.logger.LogException(new ArgumentException("LimitInAmountBDARGUS is a mandatory field."));
                                        throw new ArgumentException("LimitInAmountBDARGUS is a mandatory field.");
                                    }

                                    if (string.IsNullOrEmpty(payLoad[SiebelConstants.LimitOutAmountBDARGUS]?.ToString()))
                                    {
                                        this.logger.LogException(new ArgumentException("LimitOutAmountBDARGUS is a mandatory field."));
                                        throw new ArgumentException("LimitOutAmountBDARGUS is a mandatory field.");
                                    }
                                }
                                //// ServiceRequestBdArgus to Incident Payload generation.
#pragma warning disable S1481 // Unused local variables should be removed
                                var (contentId, targetEntity) = this.ServiceRequestBdArgusToCase(payLoad, caseconfigMapping.ToObject<JObject>(), incidentid, eventTimestamp);
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
                                    if (homeDealerNotUsedFlg != null && homeDealerNotUsedFlg.ToString().ToUpper() == "Y" && (string.IsNullOrEmpty(payLoad[SiebelConstants.LimitInAmountBDARGUS]?.ToString()) || payLoad[SiebelConstants.LimitInAmountBDARGUS]?.ToString() == "0") && (string.IsNullOrEmpty(payLoad[SiebelConstants.LimitOutAmountBDARGUS]?.ToString()) || payLoad[SiebelConstants.LimitOutAmountBDARGUS]?.ToString() == "0") && string.IsNullOrEmpty(limitInCurrencyBDARGUS?.ToString()) && string.IsNullOrEmpty(limitOutCurrencyBDARGUS?.ToString()) && targetEntity != null)
                                    {
                                        JObject payload = new ()
                                        {
                                            { SiebelConstants.Statecode, 1 },
                                            { Constants.Statuscode, 2 },
                                            { SiebelConstants.JarvisGoplimitin, 0.0 },
                                            { SiebelConstants.JarvisPaymenttype, 334030003 },
                                            { SiebelConstants.JarvisCurrencyBind, "/" + Constants.Transactioncurrencies + "(" + this.GetCurrencyCode("EURO") + ")" },
                                            { SiebelConstants.JarvisGOPOutCurrencyBind, "/" + Constants.Transactioncurrencies + "(" + this.GetCurrencyCode("EURO") + ")" },
                                            { SiebelConstants.JarvisGOPTotalLimitInCurrencyBind, "/" + Constants.Transactioncurrencies + "(" + this.GetCurrencyCode("EURO") + ")" },
                                            { SiebelConstants.JarvisGOPTotalLimitOutCurrencyBind, "/" + Constants.Transactioncurrencies + "(" + this.GetCurrencyCode("EURO") + ")" },
                                        };
                                        this.multipartContent = new ()
                                        {
                                            DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Patch, targetEntity, "(" + this.gopId + ")", Interlocked.Increment(ref this.counter), payload.ToString(), false),
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
        /// ServiceRequestBdArgusToCaseMethod.
        /// </summary>
        /// <param name="currentPayload">Current Payload.</param>
        /// <param name="mappings">Mapping.</param>
        /// <param name="incidentid">Incident Id.</param>
        /// <param name="eventTimestamp">Event Time stamp.</param>
        /// <returns>Task.</returns>
        /// <exception cref="ArgumentException">Argument Exception.</exception>
        private (int, string) ServiceRequestBdArgusToCase(JObject currentPayload, JObject? mappings, string incidentid, DateTime eventTimestamp)
        {
            try
            {
                JObject payload = new ();

                //// Planing to use for Add remark.
                string? targetEntity = DynamicsApiHelper.GetStringValueFromJObject(mappings, Constants.TargetEntityName).ToLower();

                string recordkey;
                recordkey = Guid.NewGuid().ToString();
                this.gopId = recordkey;
                //// Framing Dealers to Account Payload
                if (mappings != null && mappings.TryGetValue(Constants.JarvisFieldMappings, out JToken? fieldMapping) && fieldMapping != null)
                {
                    payload = this.helper.SetFieldMapping(currentPayload, fieldMapping, this.retrieveList);
                    ////Set GOP type to GOP
                    payload.Add(Constants.JarvisRequesttype, 334030001);
                    payload.Add(SiebelConstants.JarvisIncidentBind, string.Format("/{0}({1})", Constants.Incidents, incidentid));
                    payload.Add(Constants.JarvisGopApproval, 334030001);
                    payload.Add(Constants.CaseJarvisSource, 334030002);
                    payload.Add(SiebelConstants.JarvisGopupdatetimestamp, eventTimestamp);
                    if (string.IsNullOrEmpty(payload[SiebelConstants.JarvisTotallimitin]?.ToString()))
                    {
                        payload[SiebelConstants.JarvisTotallimitin] = 0.0;
                    }

                    if (string.IsNullOrEmpty(payload[SiebelConstants.JarvisTotallimitout]?.ToString()))
                    {
                        payload[SiebelConstants.JarvisTotallimitout] = 0.0;
                    }
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
                throw new ArgumentException($"ServiceRequestBdArgusToCase:" + ex.Message);
            }
        }

        /// <summary>
        /// Get ConfigurationMasterDataasBatch.
        /// </summary>
        /// <param name="caseNumberArgus">Case Number Argus.</param>
        /// <param name="caseNumberJarvis">Case Number Jarvis.</param>
        /// <returns>Task.</returns>
        /// <exception cref="ArgumentException">Argument Exception.</exception>
        private async Task GetConfigMasterDataAsBatch(string caseNumberArgus, string caseNumberJarvis, string bpResponsibleUnit, string bpRetailCountry)
        {
            try
            {
                List<HttpMessageContent> masterContents = new ();
                this.logger.LogTrace("Enter into ConfigurationMaster Data");
                //// Used configuration code to retrieve Config record
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.JarvisIntegrationConfiguration, string.Format(CrmQueries.JarvisIntegrationConfigurationQuery, "ADDGOP001".ToString().ToUpper()), 1, string.Empty, false));
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.Incidents, string.Format(CrmQueries.IncidentGetInboundQuery, caseNumberArgus, caseNumberJarvis), 1, string.Empty, false));
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, CtdiConstants.Transactioncurrencies, CrmQueries.JarvisStatecodeActiveQuery, 1, string.Empty, false));
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.Accounts, string.Format(CrmQueries.AccountGetQuery, bpResponsibleUnit, bpRetailCountry), 1, string.Empty, false));
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

        private string GetCurrencyCode(string currencyCode)
        {
            var matchMasterRecord = this.retrieveList.First(item => item.Key.ToUpper().Contains(CtdiConstants.Transactioncurrencies.ToUpper()));
            if (!string.IsNullOrEmpty(currencyCode))
            {
                var matachingrecord = matchMasterRecord.Value?.ToList();
                if (!string.IsNullOrEmpty(currencyCode.ToString()) && matachingrecord != null && matachingrecord.Count > 0)
                {
                    var matchRecord = matachingrecord.Where(item => item[SiebelConstants.Currencyname]?.ToString().ToUpper() == currencyCode.ToString().ToUpper()).ToList();

                    if (matchRecord != null && matchRecord.Count > 0)
                    {
                        var record = matchRecord.Select(item => item[SiebelConstants.Transactioncurrencyid.ToLower()]).First();
                        if (record != null)
                        {
                            return record.ToString();
                        }
                    }
                }
            }

            return string.Empty;
        }
    }
}