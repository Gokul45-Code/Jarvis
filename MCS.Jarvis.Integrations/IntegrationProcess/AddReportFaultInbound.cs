// <copyright file="AddReportFaultInbound.cs" company="Microsoft.">
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
    /// Add Report Fault Inbound.
    /// </summary>
    public class AddReportFaultInbound
    {
        /// <summary>
        /// DynamicsApiClient - for creating dynamics api client.
        /// </summary>
        private readonly IDynamicsApiClient dynamicsApiClient;

        /// <summary>
        /// logger object.
        /// </summary>
        private readonly ILoggerService logger;
        private readonly List<HttpMessageContent> multipartContent = new ();
        private readonly Dictionary<string, JArray?> retrieveList = new ();

        /// <summary>
        /// helper object.
        /// </summary>
        private readonly IntegrationHelper helper;
        private int counter = 0;

        // we will using the same field for update case so do not remove it, it will reuse it in future.
        private bool isCreate = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddReportFaultInbound"/> class.
        /// </summary>
        /// <param name="dynamicsApiClient">Dynamics Client.</param>
        /// <param name="logger">Logger.</param>
        public AddReportFaultInbound(IDynamicsApiClient dynamicsApiClient, ILoggerService logger)
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
        /// <param name="transType">Trans type.</param>
        /// <param name="eventTimestamp">Event time stamp.</param>
        /// <returns>Task.</returns>
        /// <exception cref="ArgumentException">Argument Exception.</exception>
        public async Task<HttpResponseMessage> IntegrationProcessAsync(JObject payLoad, string caseNumberArgus, string caseNumberJarvis, string transType, DateTime eventTimestamp)
        {
            try
            {
                ////Checking Payload contains unique identifier
                if (payLoad != null && !string.IsNullOrEmpty(caseNumberArgus))
                {
                    ////Getting All Master Data and Related Queries in single call aganist unique identifier from the payload "id".
                    await this.GetConfigMasterDataAsBatch(caseNumberArgus, caseNumberJarvis, transType);

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
                        if (jarvis_congif != null && jarvis_congif.TryGetValue(transType, StringComparison.OrdinalIgnoreCase, out JToken? caseconfigMapping))
                        {
                            this.helper.ValidateSetFieldMapping(payLoad, caseconfigMapping, this.retrieveList);
                            if (transType.ToUpper() == SiebelConstants.AddReportFault1.ToUpper())
                            {
                                var updatetimestamp = incident.First().SelectToken(SiebelConstants.JarvisDescriptionupdatetimestamp);
                                if (updatetimestamp != null && !string.IsNullOrEmpty(updatetimestamp.ToString()))
                                {
                                    DateTime incidentUpdateTimeStamp = DateTime.Parse(updatetimestamp.ToString());
                                    int result = DateTime.Compare(eventTimestamp, incidentUpdateTimeStamp);
                                    if (result <= 0)
                                    {
                                        this.logger.LogException(new Exception("Payload Timestamp should be greater than Report Fault's Timestamp in Jarvis"));
                                        throw new ArgumentException("Payload Timestamp should be greater than Report Fault's Timestamp in Jarvis");
                                    }
                                }
                                //// ServiceRequestBdArgus to Incident Payload generation.
#pragma warning disable S1481 // Unused local variables should be removed
                                var (contentId, targetEntity) = this.ServiceRequestBdArgusToCase(payLoad, caseconfigMapping.ToObject<JObject>(), incidentid, eventTimestamp);
#pragma warning restore S1481 // Unused local variables should be removed
                            }
                            else
                            {
                                //// ServiceRequestBdArgus to case translation Payload generation.
#pragma warning disable S1481 // Unused local variables should be removed
                                var (contentId, targetEntity) = this.ServiceRequestBdArgusToCaseTranslation(payLoad, caseconfigMapping.ToObject<JObject>(), incidentid);
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
        /// ServiceRequestBdArgusToCaseMethod.
        /// </summary>
        /// <param name="currentPayload">Current Payload.</param>
        /// <param name="mappings">Mapping.</param>
        /// <param name="incidentid">Incident Id.</param>
        /// <param name="eventTimestamp">Event time stamp.</param>
        /// <returns>Target entity.</returns>
        /// <exception cref="ArgumentException">Argument Exception.</exception>
        private (int, string) ServiceRequestBdArgusToCase(JObject currentPayload, JObject? mappings, string incidentid, DateTime eventTimestamp)
        {
            try
            {
                JObject payload = new ();

                string? targetEntity = DynamicsApiHelper.GetStringValueFromJObject(mappings, Constants.TargetEntityName).ToLower();

                //// Framing Dealers to Account Payload
                if (mappings != null && mappings.TryGetValue(Constants.JarvisFieldMappings, out JToken? fieldMapping) && fieldMapping != null)
                {
                    payload = this.helper.SetFieldMapping(currentPayload, fieldMapping, this.retrieveList);
                    payload.Add(SiebelConstants.JarvisDescriptionupdatetimestamp, eventTimestamp);
                }
                else
                {
                    this.logger.LogWarning("NoConfiguration field mapping is statisfied.");
                }

                // Framing Case Content for Batch
                if (payload.Count > 0)
                {
                    this.multipartContent.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Patch, targetEntity, "(" + incidentid + ")", Interlocked.Increment(ref this.counter), payload.ToString(), false));
                }

                return (this.counter, targetEntity);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"ServiceRequestBdArgusToCase:" + ex.Message);
            }
        }

        /// <summary>
        /// ServiceRequestBdArgusToCaseTranslation.
        /// </summary>
        /// <param name="currentPayload">Current Payload.</param>
        /// <param name="mappings">Mapping.</param>
        /// <param name="incidentid">Incident Id.</param>
        /// <returns>Target entity.</returns>
        /// <exception cref="ArgumentException">Argument Exception.</exception>
        private (int, string) ServiceRequestBdArgusToCaseTranslation(JObject currentPayload, JObject? mappings, string incidentid)
        {
            try
            {
                JObject payload = new ();
                string caseTranslationkey;
                string? sourceField = DynamicsApiHelper.GetStringValueFromJObject(mappings, Constants.SourceFieldSchema);
                string? targetEntity = DynamicsApiHelper.GetStringValueFromJObject(mappings, Constants.TargetEntityName);
                string? targetLookupEntityName = DynamicsApiHelper.GetStringValueFromJObject(mappings, Constants.TargetLookupEntityName);
                string? targetLookupEntity = DynamicsApiHelper.GetStringValueFromJObject(mappings, Constants.TargetLookupEntity);

                var retrieveCaseTranslation = this.LookupValueCaseTranslation(this.retrieveList, currentPayload, sourceField, targetLookupEntity, targetLookupEntityName);
                if (retrieveCaseTranslation.Item1 && retrieveCaseTranslation.value != null)
                {
#pragma warning disable S1854 // Unused assignments should be removed
                    caseTranslationkey = retrieveCaseTranslation.value.ToString();
#pragma warning restore S1854 // Unused assignments should be removed
                }
                else
                {
                    this.isCreate = true;
#pragma warning disable S1854 // Unused assignments should be removed
                    caseTranslationkey = Guid.NewGuid().ToString();
#pragma warning restore S1854 // Unused assignments should be removed
                }
                //// Framing Dealers to Account Payload
                if (mappings != null && mappings.TryGetValue(Constants.JarvisFieldMappings, out JToken? fieldMapping) && fieldMapping != null)
                {
                    payload = this.helper.SetFieldMapping(currentPayload, fieldMapping, this.retrieveList);
                    if (this.isCreate)
                    {
                        payload.Add(Constants.JarvisSource, 334030002);
                        payload.Add(Constants.JarvisType, 334030001);
                        payload.Add(SiebelConstants.JarvisIncidentBind, string.Format("/incidents({0})", incidentid));
                    }
                }
                else
                {
                    this.logger.LogWarning("NoConfiguration field mapping is statisfied.");
                }

                // Framing Case Content for Batch
                if (payload.Count > 0)
                {
                    this.multipartContent.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Patch, targetEntity, "(" + caseTranslationkey + ")", Interlocked.Increment(ref this.counter), payload.ToString(), false));
                }

                return (this.counter, targetEntity);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"ServiceRequestBdArgusToCaseTranslation:" + ex.Message);
            }
        }

        private (bool, JToken? value) LookupValueCaseTranslation(Dictionary<string, JArray?> retrieveList, JObject sourcePayload, string sourceField, string targetLookupEntity, string targetEntityName)
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
        /// Get ConfigurationMasterDataasBatch.
        /// </summary>
        /// <param name="caseNumberArgus">Case Number Argus.</param>
        /// <param name="caseNumberJarvis">Case Number Jarvis.</param>
        /// <param name="transType">Trans type.</param>
        /// <returns>Task.</returns>
        /// <exception cref="ArgumentException">Argument Exception.</exception>
        private async Task GetConfigMasterDataAsBatch(string caseNumberArgus, string caseNumberJarvis, string transType)
        {
            try
            {
                List<HttpMessageContent> masterContents = new ();
                this.logger.LogTrace("Enter into ConfigurationMaster Data");
                //// Used configuration code to retrieve Config record
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.JarvisIntegrationConfiguration, string.Format(CrmQueries.JarvisIntegrationConfigurationQuery, "REPORTFAULT001".ToString().ToUpper()), 1, string.Empty, false));
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.Incidents, string.Format(CrmQueries.IncidentGetInboundQuery, caseNumberArgus, caseNumberJarvis), 1, string.Empty, false));
                if (transType.ToUpper() == SiebelConstants.AddReportFault2.ToUpper())
                {
                    masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.JarvisCasetranslations, string.Format(CrmQueries.JarvisCasetranslationInboundQuery, caseNumberArgus, caseNumberJarvis), 1, string.Empty, false));
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