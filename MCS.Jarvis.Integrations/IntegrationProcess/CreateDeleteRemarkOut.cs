// <copyright file="CreateDeleteRemarkOut.cs" company="Microsoft.">
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
    /// CreateDeleteRemarkOut Class.
    /// </summary>
    public class CreateDeleteRemarkOut
    {
        private readonly IDynamicsApiClient dynamicsApiClient;
        private readonly IConfiguration config;
        private readonly ILoggerService logger;
        private readonly IntegrationHelper helper;
        private readonly Dictionary<string, JArray?> retrieveList = new Dictionary<string, JArray?>();
#pragma warning disable S1450 // Private fields only used as local variables in methods should become local variables
        private JObject sbMessage = new ();
#pragma warning restore S1450 // Private fields only used as local variables in methods should become local variables

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateDeleteRemarkOut"/> class.
        /// CreateDeleteRemarkOut Constructor.
        /// </summary>
        /// <param name="dynamicsApiClient">DynamicsApiClient.</param>
        /// <param name="config">Config.</param>
        /// <param name="logger">Logger.</param>
        public CreateDeleteRemarkOut(IDynamicsApiClient dynamicsApiClient, IConfiguration config, ILoggerService logger)
        {
            this.dynamicsApiClient = dynamicsApiClient;
            this.config = config;
            this.logger = logger;
            this.helper = new IntegrationHelper(this.dynamicsApiClient, this.logger);
        }

        /// <summary>
        /// IntegrationProcessAsync.
        /// </summary>
        /// <param name="postId">postId.</param>
        /// <returns>Jobject.</returns>
        /// <exception cref="ArgumentException">Argument Null Exception.</exception>
        public async Task<JObject> IntegrationProcessAsync(string postId)
        {
            try
            {
                ////Checking Payload contains unique identifier
                if (!string.IsNullOrEmpty(postId))
                {
                    ////Getting All Master Data and Related Queries in single call aganist unique identifier from the payload "id".
                    await this.GetConfigMasterDataAsBatch(postId.ToString().ToLower());

                    var configRecord = this.retrieveList.First(item => item.Key.ToUpper() == Constants.JarvisIntegrationConfiguration.ToUpper()).Value;
                    if (configRecord != null && configRecord.Count > 0
                          && configRecord.First().ToObject<JObject>().TryGetValue(Constants.JarvisIntegrationMapping, StringComparison.OrdinalIgnoreCase, out JToken? intConfig))
                    {
                        JObject intConfigRecord = JObject.Parse(intConfig.ToString());

                        ////Retriving Incident IntegrationMappings
                        if (intConfigRecord != null && intConfigRecord.TryGetValue(Constants.Incident, StringComparison.OrdinalIgnoreCase, out JToken? intMapping))
                        {
                            //// ServiceRequestBdArgus to Incident Payload generation.
                            this.sbMessage = this.CaseToServiceRequestBdArgus(intMapping.ToObject<JObject>());

                            ////Retriving Posts IntegrationMappings
                            if (intConfigRecord.TryGetValue(Constants.Posts, StringComparison.OrdinalIgnoreCase, out JToken? translationMappings))
                            {
                                //// Post to ServiceRequestRemark Payload generation.
                                JArray remarksPayload = this.PostToServiceRequestRemark(translationMappings.ToObject<JObject>());
                                JObject serviceRequestRemark = new JObject
                                  {
                                    { "ServiceRequestRemark", remarksPayload },
                                  };
                                this.sbMessage.Add("ListOfServiceRequestRemark", serviceRequestRemark);
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
                        throw new ArgumentException("Payload does not contain unique indentifier");
                    }
                    else
                    {
                        this.logger.LogException(new Exception("No Integration Configuration Found/ Integration is not Active, Pleae activate the configuraiton record."));
                        throw new ArgumentException("No Integration Configuration Found/ Integration is not Active, Pleae activate the configuraiton record.");
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
        /// Get ConfigMasterDat as Batch for UpsertCaseOut.
        /// </summary>
        /// <param name="sourcePostId">sourcePostId.</param>
        /// <returns>Task.</returns>
        /// <exception cref="ArgumentException">Argument Null Exception.</exception>
        private async Task GetConfigMasterDataAsBatch(string sourcePostId)
        {
            try
            {
                List<HttpMessageContent> masterContents = new List<HttpMessageContent>();
                this.logger.LogTrace("Enter into ConfigurationMaster Data");
                //// Used configuration code to retrieve Config record
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.JarvisIntegrationConfiguration, string.Format("?$filter=(statecode eq 0 and jarvis_integrationcode eq '{0}')", "REMARKS002".ToString().ToUpper()), 1, string.Empty, false));
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.Posts, string.Format(CrmQueries.JarvispostOutboundQuery, sourcePostId), 1, string.Empty, false));

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
        /// Case to Service Request Bd Argus.
        /// </summary>
        /// <param name="mappings">Mapping.</param>
        /// <returns>Jarray.</returns>
        /// <exception cref="ArgumentException">Argument Null Exception.</exception>
        private JArray PostToServiceRequestRemark(JObject? mappings)
        {
            try
            {
                ////Retrive Incident Payload from Retrieve List
                JArray postList = new JArray();
                string? sourceEntity = DynamicsApiHelper.GetStringValueFromJObject(mappings, Constants.SourceEntityName).ToLower();
                var matchMasterRecord = this.retrieveList.First(item => item.Key.ToUpper().Contains(sourceEntity.ToUpper())).Value?.ToList();
                if (matchMasterRecord != null && matchMasterRecord.Count > 0)
                {
                    foreach (var item in matchMasterRecord)
                    {
                        if (mappings != null && mappings.TryGetValue("fieldMappings", out JToken? fieldMapping) && fieldMapping != null)
                        {
                            this.helper.ValidateSetFieldMappingOutbound(item.ToObject<JObject>(), fieldMapping);
                            JObject keyValues = this.helper.SetFieldMappingOutbound(item.ToObject<JObject>(), fieldMapping, this.retrieveList);
                            ////HardCoding Audience
                            keyValues.Add("Audience", "Public");
                            postList.Add(keyValues);
                        }
                    }
                }

                return postList;
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"PostToServiceRequestRemark:" + ex.Message);
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

                    if (mappings != null && mappings.TryGetValue("fieldMappings", out JToken? fieldMapping) && fieldMapping != null && payload != null && payload.HasValues)
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
