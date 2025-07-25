// <copyright file="AddRepairInfoInbound.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace IntegrationProcess
{
    using System.Collections.Immutable;
    using IntegrationProcess.Helper.Constants;
    using MCS.Jarvis.Integration.Base.Dynamics;
    using MCS.Jarvis.Integration.Base.Logging;
    using MCS.Jarvis.IntegrationProcess.Helper;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Add Repair Information Inbound.
    /// </summary>
    public class AddRepairInfoInbound
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
        /// Initializes a new instance of the <see cref="AddRepairInfoInbound"/> class.
        /// </summary>
        /// <param name="dynamicsApiClient">Dynamics Client.</param>
        /// <param name="logger">Logger.</param>
        public AddRepairInfoInbound(IDynamicsApiClient dynamicsApiClient, ILoggerService logger)
        {
            this.dynamicsApiClient = dynamicsApiClient;
            this.logger = logger;
            this.helper = new IntegrationHelper(this.dynamicsApiClient, this.logger);
        }

        /// <summary>
        /// IntegrationProcessAsyncMethod.
        /// </summary>
        /// <param name="payLoad">Payload.</param>
        /// <param name="passOutPayload">PassOut Payload.</param>
        /// <param name="caseNumberArgus">Case number Argus.</param>
        /// <param name="caseNumberJarvis">Case number Jarvis.</param>
        /// <param name="transType">Trans type.</param>
        /// <returns>Task.</returns>
        /// <exception cref="ArgumentException">Argument Exception.</exception>
        public async Task<HttpResponseMessage> IntegrationProcessAsync(JObject payLoad, JObject passOutPayload, string caseNumberArgus, string caseNumberJarvis, string transType)
        {
            try
            {
                ////Checking Payload contains unique identifier
                if (payLoad != null && !string.IsNullOrEmpty(caseNumberArgus) && passOutPayload.TryGetValue(SiebelConstants.TdiPartner, StringComparison.OrdinalIgnoreCase, out JToken? bpResponsibleUnit) && passOutPayload.TryGetValue(SiebelConstants.TdiMarket, StringComparison.OrdinalIgnoreCase, out JToken? bpRetailCountry) &&
                    payLoad.TryGetValue("Type", StringComparison.OrdinalIgnoreCase, out JToken? type))
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
                        this.logger.LogException(new ArgumentException("Not a valid Record : " + caseNumberArgus + " " + caseNumberJarvis + " with Repairing Dealer responsible Unit and Retail Country Id" + bpResponsibleUnit.ToString() + "and " + bpRetailCountry.ToString() + "."));
                        throw new ArgumentException("Not a valid Record : " + caseNumberArgus + " " + caseNumberJarvis + " with Repairing Dealer responsible Unit and Retail Country Id" + bpResponsibleUnit.ToString() + "and " + bpRetailCountry.ToString() + ".");
                    }

                    if (!string.IsNullOrEmpty(incidentid) && !string.IsNullOrEmpty(passoutId) && configRecord != null && configRecord.Count > 0
                              && configRecord.First().ToObject<JObject>().TryGetValue(Constants.JarvisIntegrationMapping, StringComparison.OrdinalIgnoreCase, out JToken? intMapping))
                    {
                        JObject jarvis_congif = JObject.Parse(intMapping.ToString());

                        ////Retriving case IntegrationMappings
                        if (jarvis_congif != null && jarvis_congif.TryGetValue(transType, StringComparison.OrdinalIgnoreCase, out JToken? caseconfigMapping) &&
                            caseconfigMapping.ToObject<JObject>().TryGetValue(type.ToString(), StringComparison.OrdinalIgnoreCase, out JToken? repairInfoconfigMapping))
                        {
                            this.helper.ValidateSetFieldMapping(payLoad, repairInfoconfigMapping, this.retrieveList);
                            if (transType.ToUpper() == SiebelConstants.AddRepairInfo1.ToUpper())
                            {
                                //// ServiceRequestBdArgus to Incident Reapir Info Payload generation.
#pragma warning disable S1481 // Unused local variables should be removed
                                var (contentId, targetEntity) = this.ServiceRequestBdArgusToCase(payLoad, repairInfoconfigMapping.ToObject<JObject>(), incidentid, passoutId, type.ToString());
#pragma warning restore S1481 // Unused local variables should be removed
                            }
                            else
                            {
                                //// ServiceRequestBdArgusToTranslation to Repair Info translation Payload generation.
#pragma warning disable S1481 // Unused local variables should be removed
                                var (contentId, targetEntity) = this.ServiceRequestBdArgusToTranslation(payLoad, repairInfoconfigMapping.ToObject<JObject>(), incidentid);
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
        /// <param name="passoutId">PassOut Id.</param>
        /// <param name="type">Type.</param>
        /// <returns>Target entity.</returns>
        /// <exception cref="ArgumentException">Argument Exception.</exception>
        private (int, string) ServiceRequestBdArgusToCase(JObject currentPayload, JObject? mappings, string incidentid, string passoutId, string type)
        {
            try
            {
                JObject payload = new ();

                string? targetEntity = DynamicsApiHelper.GetStringValueFromJObject(mappings, Constants.TargetEntityName).ToLower();
                string recordkey;
                recordkey = Guid.NewGuid().ToString();
                //// Framing Repair Info to Account Payload
                if (mappings != null && mappings.TryGetValue(Constants.JarvisFieldMappings, out JToken? fieldMapping) && fieldMapping != null)
                {
                    payload = this.helper.SetFieldMapping(currentPayload, fieldMapping, this.retrieveList);
                    payload.Add(SiebelConstants.JarvisIncidentBind, string.Format("/{0}({1})", Constants.Incidents, incidentid));
                    payload.Add(SiebelConstants.JarvisRepairingDealerPassOut, string.Format(" /{0}({1})", Constants.JarvisPassouts, passoutId));
                    payload.Add(Constants.JarvisSource, 334030002);
                    payload.Add(Constants.RepairInfoJarvisAudience, 100000002);
                    //// payload.Add(SiebelConstants.jarvis_partsinformation, "not required");
                    if (type == SiebelConstants.TowingInfo)
                    {
                        payload.Add(SiebelConstants.JarvisTowingnecessary, true);
                    }
                    else if (type.ToUpper() == SiebelConstants.Warranty)
                    {
                        payload.Add(SiebelConstants.JarvisWarranty, true);
                    }
                }
                else
                {
                    this.logger.LogWarning("NoConfiguration field mapping is statisfied.");
                }

                // Framing Repair Info Content for Batch
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
        /// ServiceRequestBdArgusToCaseMethod.
        /// </summary>
        /// <param name="currentPayload">Current Payload.</param>
        /// <param name="mappings">Mapping.</param>
        /// <param name="incidentid">Incident Id.</param>
        /// <returns>Target entity.</returns>
        /// <exception cref="ArgumentException">Argument Exception.</exception>
        private (int, string) ServiceRequestBdArgusToTranslation(JObject currentPayload, JObject? mappings, string incidentid)
        {
            try
            {
                JObject payload = new ();
                string caseTranslationkey = string.Empty;
                string repairinformationid = string.Empty;
                string? sourceField = DynamicsApiHelper.GetStringValueFromJObject(mappings, Constants.SourceFieldSchema);
                string? targetEntity = DynamicsApiHelper.GetStringValueFromJObject(mappings, Constants.TargetEntityName);
                string? targetLookupEntityName = DynamicsApiHelper.GetStringValueFromJObject(mappings, Constants.TargetLookupEntityName);
                string? targetLookupEntity = DynamicsApiHelper.GetStringValueFromJObject(mappings, Constants.TargetLookupEntity);
                string? targetAlternateKeys = DynamicsApiHelper.GetStringValueFromJObject(mappings, Constants.TargetAlternateKeys);
                string? targetLookupFieldValue = DynamicsApiHelper.GetStringValueFromJObject(mappings, Constants.TargetLookupFieldValue);
                var retrieveCaseTranslation = this.LookupValueTranslation(this.retrieveList, currentPayload, sourceField, targetLookupEntity, targetLookupEntityName, targetAlternateKeys, targetLookupFieldValue);
                if (retrieveCaseTranslation.Item1 && retrieveCaseTranslation.value != null)
                {
#pragma warning disable S1854 // Unused assignments should be removed
                    caseTranslationkey = retrieveCaseTranslation.value.ToString();
#pragma warning restore S1854 // Unused assignments should be removed
                }
                else if (!retrieveCaseTranslation.Item1 && retrieveCaseTranslation.value != null)
                {
                    this.isCreate = true;
#pragma warning disable S1854 // Unused assignments should be removed
                    caseTranslationkey = Guid.NewGuid().ToString();
                    repairinformationid = retrieveCaseTranslation.value.ToString();
#pragma warning restore S1854 // Unused assignments should be removed
                }
                //// Framing Dealers to Account Payload
                if (mappings != null && mappings.TryGetValue(Constants.JarvisFieldMappings, out JToken? fieldMapping) && fieldMapping != null)
                {
                    payload = this.helper.SetFieldMapping(currentPayload, fieldMapping, this.retrieveList);
                    payload.Add(Constants.JarvisSource, 334030002);
                    if (this.isCreate)
                    {
                        payload.Add(SiebelConstants.JarvisCase, string.Format("/{0}({1})", Constants.Incidents, incidentid));
                        payload.Add(SiebelConstants.JarvisRepairInformationBind, string.Format("/{0}({1})", SiebelConstants.JarvisRepairinformations, repairinformationid));
                        payload.Add(Constants.JarvisType, "334030001");
                    }
                }
                else
                {
                    this.logger.LogWarning("No Configuration field mapping is statisfied.");
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
                throw new ArgumentException($"ServiceRequestBdArgusToTranslation:" + ex.Message);
            }
        }

        private (bool, JToken? value) LookupValueTranslation(Dictionary<string, JArray?> retrieveList, JObject sourcePayload, string sourceField, string targetLookupEntity, string targetEntityName, string targetAlternateKeys, string targetLookupFieldValue)
        {
            try
            {
                var passoutRecord = retrieveList.First(item => item.Key.ToUpper().Contains(Constants.JarvisPassouts.ToUpper())).Value?.ToList();
                var matchMasterRecord = retrieveList.First(item => item.Key.ToUpper().Contains(targetLookupEntity.ToUpper())).Value;
                var repairInfoList = matchMasterRecord?.Where(item => passoutRecord.Select(item => item[SiebelConstants.JarvisPassoutid]?.ToString()).Contains(item[SiebelConstants.JarvisRepairingdealerpassoutValue]?.ToString()));
                if (!string.IsNullOrEmpty(sourceField) && !string.IsNullOrEmpty(targetLookupEntity) && !string.IsNullOrEmpty(targetEntityName) && matchMasterRecord != null && repairInfoList?.ToList().Count > 0)
                {
                    var repairInfoTranslation = repairInfoList.SelectMany(item => item.SelectToken(targetAlternateKeys));
                    var sourceValue = sourcePayload.SelectToken(sourceField)?.Value<JToken>();
                    if (!string.IsNullOrEmpty(sourceValue?.ToString()))
                    {
                        var matchRecord = repairInfoTranslation.Where(item => item.SelectToken(targetLookupFieldValue)?.ToString().ToUpper() == sourceValue.ToString().ToUpper() && item[Constants.JarvisType]?.ToString() == "334030001").OrderByDescending(item => item["modifiedon"]).ToList();

                        if (matchRecord != null && matchRecord.Count > 0)
                        {
                            JToken? jToken = matchRecord.Select(item => item[targetEntityName.ToLower()]).First();
                            return (true, jToken);
                        }
                        else
                        {
                            JToken? jToken = repairInfoList.Select(item => item[SiebelConstants.JarvisRepairinformationid]).First();
                            return (false, jToken);
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
        /// <param name="bpResponsibleUnit">Responsible unit Id.</param>
        /// <param name="bpRetailCountry">Retail Country.</param>
        /// <returns>Task.</returns>
        /// <exception cref="ArgumentException">Argument Exception.</exception>
        private async Task GetConfigMasterDataAsBatch(string caseNumberArgus, string caseNumberJarvis, string transType, string bpResponsibleUnit, string bpRetailCountry)
        {
            try
            {
                List<HttpMessageContent> masterContents = new ();
                this.logger.LogTrace("Enter into ConfigurationMaster Data");
                //// Used configuration code to retrieve Config record
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.JarvisIntegrationConfiguration, string.Format(CrmQueries.JarvisIntegrationConfigurationQuery, "REPAIRINFO001".ToString().ToUpper()), 1, string.Empty, false));
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.JarvisPassouts, string.Format(CrmQueries.JarvisGetRepairingDealerRepairInfo, caseNumberArgus, caseNumberJarvis, bpResponsibleUnit, bpRetailCountry), 1, string.Empty, false));
                if (transType.ToUpper() == SiebelConstants.AddRepairInfo2.ToUpper())
                {
                    masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, SiebelConstants.JarvisRepairinformations, string.Format(CrmQueries.JarvisGetRepairInfo, caseNumberArgus, caseNumberJarvis), 1, string.Empty, false));
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