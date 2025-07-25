// <copyright file="DelayedEtaInbound.cs" company="Microsoft">
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
    /// Delayed ETA Inbound.
    /// </summary>
    public class DelayedEtaInbound
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

        // we will using the same field for update case so do not remove it, it will reuse it in future.
        private bool isCreate = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelayedEtaInbound"/> class.
        /// </summary>
        /// <param name="dynamicsApiClient">Dynamics Client.</param>
        /// <param name="logger">logger.</param>
        public DelayedEtaInbound(IDynamicsApiClient dynamicsApiClient, ILoggerService logger)
        {
            this.dynamicsApiClient = dynamicsApiClient;
            this.logger = logger;
            this.helper = new IntegrationHelper(this.dynamicsApiClient, this.logger);
        }

        /// <summary>
        /// IntegrationProcessAsync.
        /// </summary>
        /// <param name="passOutPayload">Passout Payload.</param>
        /// <param name="payload">Payload.</param>
        /// <param name="caseNumberArgus">Case Number Argus.</param>
        /// <param name="caseNumberJarvis">Case Number Jarvis.</param>
        /// <param name="transType">Trans Type.</param>
        /// <param name="eventTimestamp">Event Time stamp.</param>
        /// <returns>Task.</returns>
        /// <exception cref="ArgumentException">Argument Exception.</exception>
        public async Task<HttpResponseMessage> IntegrationProcessAsync(JObject passOutPayload, JObject payload, string caseNumberArgus, string caseNumberJarvis, string transType, DateTime eventTimestamp)
        {
            try
            {
                ////Checking Payload contains unique identifier
                if (passOutPayload != null && !string.IsNullOrEmpty(caseNumberArgus) && passOutPayload.TryGetValue(SiebelConstants.TdiPartner, StringComparison.OrdinalIgnoreCase, out JToken? bpResponsibleUnit) &&
                        passOutPayload.TryGetValue(SiebelConstants.TdiMarket, StringComparison.OrdinalIgnoreCase, out JToken? bpRetailCountry))
                {
                    ////Getting All Master Data and Related Queries in single call aganist unique identifier from the payload "id".
                    await this.GetConfigMasterDataAsBatch(caseNumberArgus, caseNumberJarvis, transType, bpResponsibleUnit.ToString(), bpRetailCountry.ToString());

                    var configRecord = this.retrieveList.First(item => item.Key.ToUpper() == Constants.JarvisIntegrationConfiguration.ToUpper()).Value;
                    var passout = this.retrieveList.First(item => item.Key.ToUpper().Contains(Constants.JarvisPassouts.ToUpper())).Value;
                    string? incidentid = string.Empty;
                    string? passoutId = string.Empty;
                    if (passout != null && passout.Any())
                    {
                        incidentid = passout.First().SelectToken(SiebelConstants.JarvisIncidentValue)?.ToString();
                        passoutId = passout.First().SelectToken(SiebelConstants.JarvisPassoutid)?.ToString();
                    }
                    else
                    {
                        this.logger.LogException(new ArgumentException("Not a valid Record : " + caseNumberArgus + " " + caseNumberJarvis + " with Repairing Dealer " + bpResponsibleUnit.ToString() + " and " + bpRetailCountry + "."));
                        throw new ArgumentException("Not a valid Record : " + caseNumberArgus + " " + caseNumberJarvis + " with Repairing Dealer " + bpResponsibleUnit.ToString() + " and " + bpRetailCountry + ".");
                    }

                    if (!string.IsNullOrEmpty(incidentid) && !string.IsNullOrEmpty(passoutId) && configRecord != null && configRecord.Count > 0
                              && configRecord.First().ToObject<JObject>().TryGetValue(Constants.JarvisIntegrationMapping, StringComparison.OrdinalIgnoreCase, out JToken? intMapping))
                    {
                        JObject jarvis_congif = JObject.Parse(intMapping.ToString());
                        ////Retriving delayedETA IntegrationMappings
                        if (jarvis_congif != null && jarvis_congif.TryGetValue(transType, StringComparison.OrdinalIgnoreCase, out JToken? delayedEtaconfigMapping))
                        {
                            this.helper.ValidateSetFieldMapping(payload, delayedEtaconfigMapping, this.retrieveList);
                            if (transType.ToUpper() == SiebelConstants.DelayedEtaUpdate1.ToUpper())
                            {
                                var updatetimestamp = passout.First().SelectToken(SiebelConstants.JarvisDelayedetaupdatetimestamp)?.ToString();
                                if (updatetimestamp != null && !string.IsNullOrEmpty(updatetimestamp.ToString()))
                                {
                                    DateTime passOutUpdateTimeStamp = DateTime.Parse(updatetimestamp.ToString());
                                    int result = DateTime.Compare(eventTimestamp, passOutUpdateTimeStamp);
                                    if (result <= 0)
                                    {
                                        this.logger.LogException(new Exception("Payload Timestamp should be greater than DelayedETA's Timestamp in Jarvis"));
                                        throw new ArgumentException("Payload Timestamp should be greater than DelayedETA's Timestamp in Jarvis");
                                    }
                                }
                                //// SrBreakdownEtaLogArgus To DealyedETA Payload generation.
#pragma warning disable S1481 // Unused local variables should be removed
                                var (contentId, targetEntity) = this.SrBreakdownEtaLogArgusToDealyedETA(payload, delayedEtaconfigMapping.ToObject<JObject>(), passoutId, eventTimestamp);
#pragma warning restore S1481 // Unused local variables should be removed
                            }
                            else
                            {
                                //// SrBreakdownEtaLogArgus To EtaTranslation Payload generation.
#pragma warning disable S1481 // Unused local variables should be removed
                                var (contentId, targetEntity) = this.SrBreakdownEtaLogArgusToEtaTranslation(payload, delayedEtaconfigMapping.ToObject<JObject>(), incidentid, passoutId);
#pragma warning restore S1481 // Unused local variables should be removed
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
        /// SrBreakdownEtaLogArgus to PassOut DealyedETA.
        /// </summary>
        /// <param name="currentPayload">Current Payload.</param>
        /// <param name="mappings">Mapping.</param>
        /// <param name="passoutId">PassOut Id.</param>
        /// <param name="eventTimestamp">Event Time stamp.</param>
        /// <returns>Target entity.</returns>
        /// <exception cref="ArgumentException">Argument Exception.</exception>
        private (int, string) SrBreakdownEtaLogArgusToDealyedETA(JObject currentPayload, JObject? mappings, string passoutId, DateTime eventTimestamp)
        {
            try
            {
                JObject payload = new ();

                string? targetEntity = DynamicsApiHelper.GetStringValueFromJObject(mappings, Constants.TargetEntityName).ToLower();
                //// Framing Repair Info to Account Payload
                if (mappings != null && mappings.TryGetValue(Constants.JarvisFieldMappings, out JToken? fieldMapping) && fieldMapping != null)
                {
                    payload = this.helper.SetFieldMapping(currentPayload, fieldMapping, this.retrieveList);
                    payload.Add(SiebelConstants.JarvisDelayedetaupdatetimestamp, eventTimestamp);
                    if (payload.TryGetValue("jarvis_reason", StringComparison.OrdinalIgnoreCase, out JToken? reason) && reason != null && string.IsNullOrEmpty(reason.ToString()))
                    {
                        payload.SelectToken("jarvis_reason")?.Replace("Reason not provided");
                    }
                }
                else
                {
                    this.logger.LogWarning("NoConfiguration field mapping is statisfied.");
                }

                // Framing Repair Info Content for Batch
                if (payload.Count > 0)
                {
                    this.multipartContent.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Patch, targetEntity, "(" + passoutId + ")", Interlocked.Increment(ref this.counter), payload.ToString(), false));
                }

                return (this.counter, targetEntity);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"ServiceRequestBdArgusToCase:" + ex.Message);
            }
        }

        /// <summary>
        /// SrBreakdownEtaLogArgus to PassOut ETA Transaltion.
        /// </summary>
        /// <param name="currentPayload">Current Payload.</param>
        /// <param name="mappings">Mapping.</param>
        /// <param name="incidentid">Incident Id.</param>
        /// <returns>Target entity.</returns>
        /// <exception cref="ArgumentException">Argument Exception.</exception>
        private (int, string) SrBreakdownEtaLogArgusToEtaTranslation(JObject currentPayload, JObject? mappings, string incidentid, string passoutId)
        {
            try
            {
                JObject payload = new ();
                string? etaTranslationkey;
                string? sourceField = DynamicsApiHelper.GetStringValueFromJObject(mappings, Constants.SourceFieldSchema);
                string? targetEntity = DynamicsApiHelper.GetStringValueFromJObject(mappings, Constants.TargetEntityName);
                string? targetLookupEntityName = DynamicsApiHelper.GetStringValueFromJObject(mappings, Constants.TargetLookupEntityName);
                string? targetLookupEntity = DynamicsApiHelper.GetStringValueFromJObject(mappings, Constants.TargetLookupEntity);

                var retrieveEtaTranslation = this.LookupValueEtaTranslation(this.retrieveList, currentPayload, sourceField, targetLookupEntity, targetLookupEntityName);
                if (retrieveEtaTranslation.Item1 && retrieveEtaTranslation.value != null)
                {
                    etaTranslationkey = retrieveEtaTranslation.value.ToString();
                }
                else
                {
                    this.isCreate = true;
                    etaTranslationkey = Guid.NewGuid().ToString();
                }
                //// Framing Dealers to Account Payload
                if (mappings != null && mappings.TryGetValue(Constants.JarvisFieldMappings, out JToken? fieldMapping) && fieldMapping != null && etaTranslationkey != null)
                {
                    payload = this.helper.SetFieldMapping(currentPayload, fieldMapping, this.retrieveList);
                    payload.Add(Constants.JarvisSource, 334030002);
                    if (this.isCreate)
                    {
                        payload.Add(SiebelConstants.JarvisCase, string.Format("/{0}({1})", Constants.Incidents, incidentid));
                        payload.Add(SiebelConstants.JarvisPassOut, string.Format("/{0}({1})", Constants.JarvisPassouts, passoutId));
                    }

                    if (payload.TryGetValue("jarvis_reason", StringComparison.OrdinalIgnoreCase, out JToken? reason) && reason != null && string.IsNullOrEmpty(reason.ToString()))
                    {
                        payload.SelectToken("jarvis_reason")?.Replace("Reason not provided");
                    }
                }
                else
                {
                    this.logger.LogWarning("No Configuration field mapping is statisfied or no etaTranslationKey is present.");
                }

                // Framing Case Content for Batch
                if (payload.Count > 0)
                {
                    this.multipartContent.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Patch, targetEntity, "(" + etaTranslationkey + ")", Interlocked.Increment(ref this.counter), payload.ToString(), false));
                }

                return (this.counter, targetEntity);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"ServiceRequestBdArgusToTranslation:" + ex.Message);
            }
        }

        /// <summary>
        /// Lookup ETA Translation Record.
        /// </summary>
        /// <param name="retrieveList">Retrive List of Master Data.</param>
        /// <param name="sourcePayload">Source Payload.</param>
        /// <param name="sourceField">Source Field.</param>
        /// <param name="targetLookupEntity">Target Lookup Entity.</param>
        /// <param name="targetEntityName">Target Entity Name.</param>
        /// <returns>JToken of Id.</returns>
        /// <exception cref="ArgumentException">Argument Exception.</exception>
        private (bool, JToken? value) LookupValueEtaTranslation(Dictionary<string, JArray?> retrieveList, JObject sourcePayload, string sourceField, string targetLookupEntity, string targetEntityName)
        {
            try
            {
                var matchMasterRecord = retrieveList.First(item => item.Key.ToUpper().Contains(targetLookupEntity.ToUpper()));

                if (!string.IsNullOrEmpty(sourceField) && !string.IsNullOrEmpty(targetLookupEntity) && !string.IsNullOrEmpty(targetEntityName) && matchMasterRecord.Key != null)
                {
                    var matachingrecord = matchMasterRecord.Value?.ToList();
                    var sourceValue = sourcePayload.SelectToken(sourceField)?.Value<JToken>();
                    if (!string.IsNullOrEmpty(sourceValue?.ToString()) && matachingrecord != null && matachingrecord.Count > 0)
                    {
                        var matchRecord = matachingrecord.Where(item => item[Constants.JarvisLanguage]?[Constants.JarvisMercuriuslanguagecode]?.ToString().ToUpper().Replace("-", string.Empty) == sourceValue.ToString().ToUpper().Replace("-", string.Empty)).ToList();

                        if (matchRecord != null && matchRecord.Count > 0)
                        {
                            JToken? jToken = matchRecord.Select(item => item[targetEntityName.ToLower()]).First();
                            return (true, jToken);
                        }
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
                throw new ArgumentException($" Retrieving Lookup Value from Jarvis Failed with Error Message - " + ex.Message);
            }
        }

        /// <summary>
        /// GetConfigurationDataAsBatch.
        /// </summary>
        /// <param name="caseNumberArgus">Case Number Argus.</param>
        /// <param name="caseNumberJarvis">Case Number Jarvis.</param>
        /// <param name="transType">Trans type.</param>
        /// <param name="bpResponsibleUnit">Responsible unit.</param>
        /// <param name="bpRetailCountry">Retail country.</param>
        /// <returns>Task.</returns>
        /// <exception cref="ArgumentException">Argument Exception.</exception>
        private async Task GetConfigMasterDataAsBatch(string caseNumberArgus, string caseNumberJarvis, string transType, string bpResponsibleUnit, string bpRetailCountry)
        {
            try
            {
                List<HttpMessageContent> masterContents = new ();
                this.logger.LogTrace("Enter into ConfigurationMaster Data");
                //// Used configuration code to retrieve Config record
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.JarvisIntegrationConfiguration, string.Format(CrmQueries.JarvisIntegrationConfigurationQuery, "DELAYEDETA001".ToString().ToUpper()), 1, string.Empty, false));
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.JarvisPassouts, string.Format(CrmQueries.JarvisGetPassoutDelayedetaInboundQuery, caseNumberArgus, caseNumberJarvis, bpResponsibleUnit, bpRetailCountry), 1, string.Empty, false));
                if (transType.ToUpper() == SiebelConstants.DelayedEtaUpdate2.ToUpper())
                {
                    masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, SiebelConstants.JarvisPassouttranslations, string.Format(CrmQueries.JarvisPassOuttranslationInboundQuery, caseNumberArgus, caseNumberJarvis), 1, string.Empty, false));
                    masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.JarvisLanguages, CrmQueries.JarvisStatecodeActiveQuery, 1, string.Empty, false));
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
