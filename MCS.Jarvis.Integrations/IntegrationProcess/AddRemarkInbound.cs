// <copyright file="AddRemarkInbound.cs" company="Microsoft">
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
    /// Add Remark Inbound.
    /// </summary>
    public class AddRemarkInbound
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
        /// Initializes a new instance of the <see cref="AddRemarkInbound"/> class.
        /// </summary>
        /// <param name="dynamicsApiClient">Dynamic Client.</param>
        /// <param name="logger">Logger.</param>
        public AddRemarkInbound(IDynamicsApiClient dynamicsApiClient, ILoggerService logger)
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
        /// <returns>Task.</returns>
        /// <exception cref="ArgumentException">Argument Exception.</exception>
        public async Task<HttpResponseMessage> IntegrationProcessAsync(JObject payLoad, string caseNumberArgus, string caseNumberJarvis)
        {
            try
            {
                ////Checking Payload contains unique identifier
                if (payLoad != null && !string.IsNullOrEmpty(caseNumberArgus))
                {
                    ////Getting All Master Data and Related Queries in single call aganist unique identifier from the payload "id".
                    await this.GetConfigMasterDataAsBatch(caseNumberArgus.ToString(), caseNumberJarvis);

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
                        if (jarvis_congif != null && jarvis_congif.TryGetValue(Constants.Post, StringComparison.OrdinalIgnoreCase, out JToken? caseconfigMapping))
                        {
                            this.helper.ValidateSetFieldMapping(payLoad, caseconfigMapping, this.retrieveList);
                            //// ServiceRequestBdArgus to Incident Payload generation.
#pragma warning disable S1481 // Unused local variables should be removed
                            var (contentId, targetEntity) = this.ServiceRequestBdArgusToCase(payLoad, caseconfigMapping.ToObject<JObject>(), incidentid);
#pragma warning restore S1481 // Unused local variables should be removed

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
        /// <returns>Target Entity.</returns>
        /// <exception cref="ArgumentException">Argument Exception.</exception>
        private (int, string) ServiceRequestBdArgusToCase(JObject currentPayload, JObject? mappings, string incidentid)
        {
            try
            {
                JObject payload = new ();

                //// Planing to use for Add remark.
                string? targetEntity = DynamicsApiHelper.GetStringValueFromJObject(mappings, Constants.TargetEntityName).ToLower();

                string recordkey;
                recordkey = Guid.NewGuid().ToString();

                //// Framing Dealers to Account Payload
                if (mappings != null && mappings.TryGetValue(Constants.JarvisFieldMappings, out JToken? fieldMapping) && fieldMapping != null)
                {
                    payload = this.helper.SetFieldMapping(currentPayload, fieldMapping, this.retrieveList);
                    payload.Add(Constants.Source, 2);
                    payload.Add(SiebelConstants.RegardingobjectIncident, string.Format("/{0}({1})", Constants.Incidents, incidentid));
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
        private async Task GetConfigMasterDataAsBatch(string caseNumberArgus, string caseNumberJarvis)
        {
            try
            {
                List<HttpMessageContent> masterContents = new ();
                this.logger.LogTrace("Enter into ConfigurationMaster Data");
                //// Used configuration code to retrieve Config record
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.JarvisIntegrationConfiguration, string.Format(CrmQueries.JarvisIntegrationConfigurationQuery, "REMARKS001".ToString().ToUpper()), 1, string.Empty, false));
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.Incidents, string.Format(CrmQueries.IncidentGetInboundQuery, caseNumberArgus, caseNumberJarvis), 1, string.Empty, false));

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