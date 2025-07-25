// <copyright file="UpsertPassout.cs" company="Microsoft">
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
    /// UpsertPassout.
    /// </summary>
    public class UpsertPassout
    {
        private readonly IDynamicsApiClient dynamicsApiClient;

        /// <summary>
        /// configuration object.
        /// </summary>
        private readonly IConfiguration config;

        /// <summary>
        /// logger object.
        /// </summary>
        private readonly ILoggerService logger;
        private readonly IntegrationHelper helper;
        private readonly List<HttpMessageContent> multipartContent = new List<HttpMessageContent>();
        private readonly Dictionary<string, JArray?> retrieveList = new Dictionary<string, JArray?>();
        private int counter = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpsertPassout"/> class.
        /// Upsert Passout constructor for siebel - ATC,ETC,ETA,DelayedETA,ATA.
        /// </summary>
        /// <param name="dynamicsApiClient">DynamicsApiClient.</param>
        /// <param name="config">Config.</param>
        /// <param name="logger">Logger.</param>
        public UpsertPassout(IDynamicsApiClient dynamicsApiClient, IConfiguration config, ILoggerService logger)
        {
            this.dynamicsApiClient = dynamicsApiClient;
            this.config = config;
            this.logger = logger;
            this.helper = new IntegrationHelper(this.dynamicsApiClient, this.logger);
        }

        /// <summary>
        /// IntegrationProcessAsync.
        /// </summary>
        /// <param name="payLoad">Payload.</param>
        /// <param name="caseNumberArgus">CaseNumberArgus.</param>
        /// <param name="transType">TransType.</param>
        /// <param name="caseNumberJarvis">CaseNumberJarvis.</param>
        /// <param name="eventTimestamp">EvenTimeStamp.</param>
        /// <returns>Task.</returns>
        /// <exception cref="ArgumentException">Argument Null Exception.</exception>
        public async Task<HttpResponseMessage> IntegrationProcessAsync(JObject payLoad, string caseNumberArgus, string transType, string caseNumberJarvis, DateTime eventTimestamp)
        {
            try
            {
                ////Checking Payload contains unique identifier
                if (payLoad != null && payLoad.TryGetValue(SiebelConstants.TdiPartner, StringComparison.OrdinalIgnoreCase, out JToken? bpResponsibleUnit) &&
                        payLoad.TryGetValue(SiebelConstants.TdiMarket, StringComparison.OrdinalIgnoreCase, out JToken? bpRetailCountry))
                {
                    ////Getting All Master Data and Related Queries in single call aganist unique identifier from the payload "id".
                    await this.GetConfigMasterDataAsBatch(bpResponsibleUnit.ToString(), bpRetailCountry.ToString(), caseNumberArgus, caseNumberJarvis);

                    var configRecord = this.retrieveList.First(item => item.Key.ToUpper() == Constants.JarvisIntegrationConfiguration.ToUpper()).Value;
                    var passoutRecord = this.retrieveList.First(item => item.Key.ToUpper() == Constants.JarvisPassouts.ToUpper()).Value;
                    if (configRecord != null && configRecord.Count > 0 && passoutRecord != null && passoutRecord.Count > 0
                          && configRecord.First().ToObject<JObject>().TryGetValue(Constants.JarvisIntegrationMapping, StringComparison.OrdinalIgnoreCase, out JToken? intMapping))
                    {
                        var updatetimestamp = string.Empty;
                        var updateField = string.Empty;
                        switch (transType)
                        {
                            case SiebelConstants.AtaUpdate:
                                {
                                    updatetimestamp = passoutRecord.First().SelectToken(SiebelConstants.JarvisAtaupdatetimestamp)?.ToString();
                                    updateField = SiebelConstants.JarvisAtaupdatetimestamp;
                                    break;
                                }

                            case SiebelConstants.AtcUpdate:
                                {
                                    updatetimestamp = passoutRecord.First().SelectToken(SiebelConstants.JarvisAtcupdatetimestamp)?.ToString();
                                    updateField = SiebelConstants.JarvisAtcupdatetimestamp;
                                    break;
                                }

                            case SiebelConstants.EtaUpdate:
                                {
                                    updatetimestamp = passoutRecord.First().SelectToken(SiebelConstants.JarvisEtaupdatetimestamp)?.ToString();
                                    updateField = SiebelConstants.JarvisEtaupdatetimestamp;
                                    break;
                                }

                            case SiebelConstants.GpsetaUpdate:
                                {
                                    updatetimestamp = passoutRecord.First().SelectToken(SiebelConstants.JarvisGpsetaupdatetimestamp)?.ToString();
                                    updateField = SiebelConstants.JarvisGpsetaupdatetimestamp;
                                    break;
                                }

                            case SiebelConstants.EtcUpdate:
                                {
                                    updatetimestamp = passoutRecord.First().SelectToken(SiebelConstants.JarvisEtcupdatetimestamp)?.ToString();
                                    updateField = SiebelConstants.JarvisEtcupdatetimestamp;
                                    break;
                                }
                        }

                        if (updatetimestamp != null && !string.IsNullOrEmpty(updatetimestamp.ToString()))
                        {
                            DateTime passOutUpdateTimeStamp = DateTime.Parse(updatetimestamp.ToString());
                            int result = DateTime.Compare(eventTimestamp, passOutUpdateTimeStamp);
                            if (result <= 0)
                            {
                                this.logger.LogException(new Exception(string.Format("Payload Timestamp should be greater than {0} Timestamp in Jarvis", updateField)));
                                throw new ArgumentException(string.Format("Payload Timestamp should be greater than {0} Timestamp in Jarvis", updateField));
                            }
                        }

                        JObject jarvis_congif = JObject.Parse(intMapping.ToString());

                        ////Retriving case IntegrationMappings
                        if (jarvis_congif != null && jarvis_congif.TryGetValue(transType, StringComparison.OrdinalIgnoreCase, out JToken? caseconfigMapping))
                        {
                            this.helper.ValidateSetFieldMapping(payLoad, caseconfigMapping, this.retrieveList);
                            //// ServiceRequestBdArgus to Incident Payload generation.
#pragma warning disable S1481 // Unused local variables should be removed
                            var (contentId, targetEntity) = this.ServiceRequestBdArgusToCase(payLoad, caseconfigMapping.ToObject<JObject>(), passoutRecord.First().ToObject<JObject>(), updateField, eventTimestamp);
#pragma warning restore S1481 // Unused local variables should be removed

                            ////Checking MultipartContent contains Data and executing whole payload request..
                            if (this.multipartContent.Count > 0)
                            {
                                var response = await this.dynamicsApiClient.ExecuteBatchRequest(this.multipartContent);
                                return response == null ? new HttpResponseMessage() : response;
                            }

                            this.logger.LogException(new Exception("Error in Generating Payload"));
                            throw new ArgumentException("Error in Generating Payload");
                        }

                        this.logger.LogException(new Exception("No Integration Configuration Found/ Integration is not Active, Pleae activate the configuraiton record."));
                        throw new ArgumentException("Payload does not contain unique indentifier");
                    }
                    else
                    {
                        this.logger.LogException(new Exception("No Integration Configuration Found/ Integration is not Active/ No Passout Found, Pleae activate the configuraiton record."));
                        throw new ArgumentException("No Integration Configuration Found/ Integration is not Active/ No Passout Found, Pleae activate the configuraiton record.");
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
        /// GetConfigMasterDataAsBatch.
        /// </summary>
        /// <param name="bpResponsibleUnit">BPResponsibleUnit.</param>
        /// <param name="bpRetailCountry">BPRetailCountry.</param>
        /// <param name="caseNumberArgus">CaseNumberArgus.</param>
        /// <param name="caseNumberJarvis">CaseNumberJarvis.</param>
        /// <returns>Task.</returns>
        /// <exception cref="ArgumentException">Argument Null Exception.</exception>
        private async Task GetConfigMasterDataAsBatch(string bpResponsibleUnit, string bpRetailCountry, string caseNumberArgus, string caseNumberJarvis)
        {
            try
            {
                List<HttpMessageContent> masterContents = new List<HttpMessageContent>();
                this.logger.LogTrace("Enter into ConfigurationMaster Data");
                //// Used configuration code to retrieve Config record
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.JarvisIntegrationConfiguration, string.Format("?$filter=(statecode eq 0 and jarvis_integrationcode eq '{0}')", "JARVIS002".ToString().ToUpper()), 1, string.Empty, false));
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.JarvisPassouts, string.Format(CrmQueries.PassoutSiebelInboundQuery, caseNumberArgus, caseNumberJarvis, bpResponsibleUnit, bpRetailCountry), 1, string.Empty, false));

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
        /// ServiceRequestBdArgusToCase.
        /// </summary>
        /// <param name="currentPayload">CurrentPayload.</param>
        /// <param name="mappings">Mappings.</param>
        /// <param name="passout">PassOut.</param>
        /// <param name="updateField">UpdateField.</param>
        /// <param name="eventTimestamp">EvenTimeStamp.</param>
        /// <returns>Int,String.</returns>
        /// <exception cref="ArgumentException">Argument Null Exception.</exception>
        private (int, string) ServiceRequestBdArgusToCase(JObject currentPayload, JObject? mappings, JObject? passout, string updateField, DateTime eventTimestamp)
        {
            try
            {
                JObject payload = new JObject();
                string? targetEntity = DynamicsApiHelper.GetStringValueFromJObject(mappings, Constants.TargetEntityName);
                string? targetFieldSchema = DynamicsApiHelper.GetStringValueFromJObject(mappings, Constants.TargetFieldSchema);

                string? recordkey = string.Empty;
                if (passout != null && passout.HasValues && targetFieldSchema != null)
                {
                    recordkey = passout.GetValue(targetFieldSchema)?.ToString();
                }

                //// Framing Dealers to Account Payload
                if (mappings != null && mappings.TryGetValue(Constants.JarvisFieldMappings, out JToken? fieldMapping) && fieldMapping != null)
                {
                    payload = this.helper.SetFieldMapping(currentPayload, fieldMapping, this.retrieveList);
                    payload.Add(updateField, eventTimestamp);
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
    }
}
