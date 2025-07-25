// <copyright file="JobEndOutbound.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace IntegrationProcess
{
    using System.Globalization;
    using System.Text;
    using System.Xml.Linq;
    using Azure.Messaging.ServiceBus;
    using IntegrationProcess.Helper.Constants;
    using MCS.Jarvis.Integration.Base.Dynamics;
    using MCS.Jarvis.Integration.Base.Logging;
    using MCS.Jarvis.IntegrationProcess.Helper;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Job End Details Outbound Class.
    /// </summary>
    public class JobEndOutbound
    {
        private readonly IDynamicsApiClient dynamicsApiClient;
        private readonly ILoggerService logger;
        private readonly IntegrationHelper helper;
        private readonly string azureServicebusConnectionstring;
        private readonly string queueName;
        private readonly Dictionary<string, JArray?> retrieveList = new Dictionary<string, JArray?>();
        private string? modifiedOn = DateTime.UtcNow.ToString();

        /// <summary>
        /// Initializes a new instance of the <see cref="JobEndOutbound"/> class.
        /// JobEndOutbound Constructor.
        /// </summary>
        /// <param name="dynamicsApiClient">DynamicsApiClient.</param>
        /// <param name="config">Config.</param>
        /// <param name="logger">Logger.</param>
        public JobEndOutbound(IDynamicsApiClient dynamicsApiClient, IConfiguration config, ILoggerService logger)
        {
            this.dynamicsApiClient = dynamicsApiClient;
            this.logger = logger;
            this.helper = new IntegrationHelper(this.dynamicsApiClient, this.logger);
            this.azureServicebusConnectionstring = config.GetSection("ServiceBusConnection").Value;
            this.queueName = config.GetSection("OutboundUpdateCaseSpecQueue").Value;
        }

        /// <summary>
        /// IntegrationProcessAysync siebel - DelayedEtaOutbound.
        /// </summary>
        /// <param name="jobEndTransId">JobEndTransId.</param>
        /// <param name="transType">TransType.</param>
        /// <param name="entityData">EntityData.</param>
        /// <param name="translationType">translationType.</param>
        /// <returns>Task.</returns>
        /// <exception cref="ArgumentException">Argument Null Exception.</exception>
        public async Task IntegrationProcessAsync(string jobEndTransId, string transType, string entityData, string translationType)
        {
            try
            {
                ////Checking Payload contains unique identifier
                if (!string.IsNullOrEmpty(jobEndTransId) && !string.IsNullOrEmpty(transType) && !string.IsNullOrEmpty(translationType))
                {
                    string? triggeredBy = string.Empty;

                    if (!string.IsNullOrEmpty(entityData))
                    {
                        this.logger.LogTrace($"PluginContextData: {entityData}");
                        JObject? record = this.helper.ConvertContextToObject(entityData);
                        if (record.HasValues)
                        {
                            triggeredBy = record.GetValue(Constants.TriggeredBy) != null ? record.GetValue(Constants.TriggeredBy)?.ToString() : string.Empty;
                            ////Getting All Master Data and Related Queries in single call aganist unique identifier from the payload "id".
                            await this.GetConfigMasterDataAsBatch(jobEndTransId.ToString().ToLower(), triggeredBy);
                            this.modifiedOn = record.GetValue("modifiedon")?.ToString();
                            var retrieveInc = this.retrieveList.First(item => item.Key.ToUpper() == Constants.JarvisJobenddetailstranslations.ToUpper()).Value?.First().ToObject<JObject>();
                            retrieveInc?.Merge(record);
                            if (retrieveInc != null)
                            {
                                this.retrieveList.First(item => item.Key.ToUpper() == Constants.JarvisJobenddetailstranslations.ToUpper()).Value?.RemoveAll();
                                this.retrieveList.First(item => item.Key.ToUpper() == Constants.JarvisJobenddetailstranslations.ToUpper()).Value?.Add(retrieveInc);
                            }
                        }
                        else
                        {
                            ////Getting All Master Data and Related Queries in single call aganist unique identifier from the payload "id".
                            await this.GetConfigMasterDataAsBatch(jobEndTransId.ToString().ToLower(), triggeredBy);
                        }
                    }
                    else
                    {
                        ////Getting All Master Data and Related Queries in single call aganist unique identifier from the payload "id".
                        await this.GetConfigMasterDataAsBatch(jobEndTransId.ToString().ToLower(), triggeredBy);
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
                            JObject casePayload = this.CaseToServiceRequestBdArgus(intMapping.ToObject<JObject>());

                            if (intConfigRecord.TryGetValue(Constants.JarvisPassouts, StringComparison.OrdinalIgnoreCase, out JToken? passOutMapping))
                            {
                                //// PassOut Payload
                                JObject passOutPayload = this.PassOutoSRBdPassoutLoginfoArgus(passOutMapping.ToObject<JObject>());
                                ////Retriving casetransaltion IntegrationMappings
                                if (intConfigRecord.TryGetValue(Constants.JarvisJobenddetailstranslations, StringComparison.OrdinalIgnoreCase, out JToken? translationMappings))
                                {
                                    //// SRBdRepairInfoArgus to Case translation Payload generation.

                                    await this.JobEndTransToSRBreakdownJobEndDetailsArgus(translationMappings.ToObject<JObject>(), jobEndTransId, transType, casePayload, passOutPayload, translationType);
                                }
                            }
                        }
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
        /// Get Config,Master and Related Data as Batch.
        /// </summary>
        /// <param name="jobEndDetailId">Job End Details.</param>
        /// <param name="modifiedBy">ModifiedBy.</param>
        /// <returns>Task.</returns>
        /// <exception cref="ArgumentException">Argument Null Exception.</exception>
        private async Task GetConfigMasterDataAsBatch(string jobEndDetailId, string? modifiedBy)
        {
            try
            {
                List<HttpMessageContent> masterContents = new List<HttpMessageContent>();
                this.logger.LogTrace("Enter into ConfigurationMaster Data");
                //// Used configuration code to retrieve Config record
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.JarvisIntegrationConfiguration, string.Format(CrmQueries.JarvisIntegrationConfigurationQuery, "JOBENDDETAIL002".ToString().ToUpper()), 1, string.Empty, false));
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.JarvisJobenddetailstranslations, string.Format(CrmQueries.JarvisJobEndTranslationOutboundQuery, jobEndDetailId), 1, string.Empty, false));
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.JarvisLanguages, CrmQueries.JarvisStatecodeActiveQuery, 1, string.Empty, false));
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.JarvisMileages, CrmQueries.JarvisStatecodeActiveQuery, 1, string.Empty, false));
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.JarvisBrands, CrmQueries.JarvisStatecodeActiveQuery, 1, string.Empty, false));
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.JarvisCountries, CrmQueries.JarvisStatecodeActiveQuery, 1, string.Empty, false));
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
        /// Job End Details Translation.
        /// </summary>
        /// <param name="mappings">Mappings.</param>
        /// <param name="jobEndTransId">JobEndTransId.</param>
        /// <param name="transType">TransType.</param>
        /// <param name="casePayload">CasePayload.</param>
        /// <param name="passOutPayload">PassOutPaylaod.</param>
        /// <param name="translationType">translationType.</param>
        /// <returns>Task.</returns>
        /// <exception cref="ArgumentException">Argument Null Exception.</exception>
        private async Task JobEndTransToSRBreakdownJobEndDetailsArgus(JObject? mappings, string jobEndTransId, string transType, JObject casePayload, JObject passOutPayload, string translationType)
        {
            try
            {
                ////Retrive Incident Payload from Retrieve List
                string? sourceEntity = DynamicsApiHelper.GetStringValueFromJObject(mappings, Constants.SourceEntityName).ToLower();
                var matchMasterRecord = this.retrieveList.First(item => item.Key.ToUpper().Contains(sourceEntity.ToUpper())).Value?.ToList();
                if (matchMasterRecord != null && matchMasterRecord.Count > 0)
                {
                    foreach (var item in matchMasterRecord)
                    {
                        //// For JOB END DETAILS TRANSLATION
                        if (item.ToObject<JObject>().TryGetValue(Constants.JarvisJobEndDetails, out JToken? originalJobEndDetail) && mappings != null
                            && mappings.TryGetValue(SiebelConstants.JarvisActualcausefault, out JToken? fieldMappingActualFault) && mappings.TryGetValue(SiebelConstants.JarvisTemporaryrepair, out JToken? fieldMappingTempRepair))
                        {
                            string? originalActualFault = originalJobEndDetail.SelectToken(SiebelConstants.JarvisActualcausefault)?.ToString();
                            string? originalTemporaryRepair = originalJobEndDetail.SelectToken(SiebelConstants.JarvisTemporaryrepair)?.ToString();

                            ////Job End Details Actual Fault Payload sent to Mercurius
                            if (!string.IsNullOrEmpty(originalActualFault) && !string.IsNullOrWhiteSpace(originalActualFault) && fieldMappingActualFault != null && casePayload.HasValues && passOutPayload.HasValues && (translationType.ToUpper() == TranslationType.Create.ToUpper() || translationType.ToUpper() == TranslationType.ActualCause.ToUpper()))
                            {
                                JObject finalAcutalFaultPayload = new (casePayload);
                                JArray jobEndPayload = this.SetMappingJobEndDetailTransPayload(item, fieldMappingActualFault);
                                //// All the jobEndDetail Payload as array of object
                                JObject jobEndPassOutInfo = this.FrameJobEndPaylaod(jobEndPayload, passOutPayload);
                                //// Passout Object with Job End list converted into PassOut List and also adding with Case details in final Actual fault Paylaod.
                                finalAcutalFaultPayload.Add(SiebelConstants.ListOfSRBdPassoutLoginfoArgus, jobEndPassOutInfo);
                                await this.JobEndDetailSendMessages(finalAcutalFaultPayload, jobEndTransId, transType);
                            }
                            ////Job End Details Temporary Repair Payload sent to Mercurius
                            if (!string.IsNullOrEmpty(originalTemporaryRepair) && !string.IsNullOrWhiteSpace(originalTemporaryRepair) && casePayload.HasValues && passOutPayload.HasValues && (translationType.ToUpper() == TranslationType.Create.ToUpper() || translationType.ToUpper() == TranslationType.TempRepair.ToUpper()))
                            {
                                JObject finalTempRepairPayload = new (casePayload);
                                JArray tempRepairPayload = this.SetMappingJobEndDetailTransPayload(item, fieldMappingTempRepair);
                                //// All the jobEndDetail Payload as array of object
                                JObject jobEndPassOutInfo = this.FrameJobEndPaylaod(tempRepairPayload, passOutPayload);
                                //// Passout Object with Job End list converted into PassOut List and also adding with Case details in final Temp Repair Paylaod.
                                finalTempRepairPayload.Add(SiebelConstants.ListOfSRBdPassoutLoginfoArgus, jobEndPassOutInfo);
                                await this.JobEndDetailSendMessages(finalTempRepairPayload, jobEndTransId, transType);
                            }
                        }
                        else
                        {
                            throw new ArgumentException($"No Field Mapping and Job End Details not exists.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"JobEndTransToSRBreakdownJobEndDetailsArgus:" + ex.Message);
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

        /// <summary>
        /// Set Field Mapping for Job End Detail Translation.
        /// </summary>
        /// <param name="item">Item.</param>
        /// <param name="fieldMappings">FieldMappings.</param>
        /// <returns>JArray.</returns>
        private JArray SetMappingJobEndDetailTransPayload(JToken item, JToken fieldMappings)
        {
            JArray jobEndList = new ();

            if (fieldMappings.ToObject<JObject>().TryGetValue(Constants.JarvisFieldMappings, out JToken? fieldMapping) && fieldMapping != null && item != null && item.HasValues)
            {
                this.helper.ValidateSetFieldMappingOutbound(item.ToObject<JObject>(), fieldMapping);
                JObject payload = this.helper.SetFieldMappingOutbound(item.ToObject<JObject>(), fieldMapping, this.retrieveList);
                jobEndList.Add(payload);
            }

            return jobEndList;
        }

        /// <summary>
        /// Framing Job End Details Payload.
        /// </summary>
        /// <param name="jobEndPayload">JobEndPayload.</param>
        /// <param name="passOutPayload">PassOutPayload.</param>
        /// <returns>Jobject.</returns>
        private JObject FrameJobEndPaylaod(JArray jobEndPayload, JObject passOutPayload)
        {
            JObject jobEndInfoObj = new ()
            {
                  { SiebelConstants.SRBreakdownJobEndDetailsArgus, jobEndPayload },
            };
            //// JobEndDetails Object converted into Array.
            JObject jobEndList = new (passOutPayload);
            jobEndList.Add(SiebelConstants.ListOfSRBreakdownJobEndDetailsArgus, jobEndInfoObj);
            JArray jobEndArray = new (jobEndList);

            JObject jobEndPassOutInfo = new ()
            {
                 { SiebelConstants.SRBdPassoutLoginfoArgus, jobEndArray },
            };
            return jobEndPassOutInfo;
        }

        /// <summary>
        /// Framing PassOut Payload.
        /// </summary>
        /// <param name="mappings">Mappings.</param>
        /// <returns>JOject.</returns>
        /// <exception cref="ArgumentException">Argument Null Exception.</exception>
        private JObject PassOutoSRBdPassoutLoginfoArgus(JObject? mappings)
        {
            try
            {
                ////Retrive Incident Payload from Retrieve List
                JObject? payload;
                JObject passOutPayload = new JObject();
                string? sourceEntity = DynamicsApiHelper.GetStringValueFromJObject(mappings, Constants.SourceEntityName).ToLower();

                var matchMasterRecord = this.retrieveList.First(item => item.Key.ToUpper().Contains(sourceEntity.ToUpper())).Value?.ToList();
                if (matchMasterRecord != null && matchMasterRecord.Count > 0)
                {
                    payload = matchMasterRecord.First().ToObject<JObject>();

                    if (mappings != null && mappings.TryGetValue(Constants.JarvisFieldMappings, out JToken? fieldMapping) && fieldMapping != null && payload != null && payload.HasValues)
                    {
                        this.helper.ValidateSetFieldMappingOutbound(payload, fieldMapping);
                        passOutPayload = this.helper.SetFieldMappingOutbound(payload, fieldMapping, this.retrieveList);
                    }
                }

                return passOutPayload;
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"PassOutoSRBdPassoutLoginfoArgus:" + ex.Message);
            }
        }

        /// <summary>
        /// JobEndDetailSendMessages.
        /// </summary>
        /// <param name="response">Response.</param>
        /// <param name="caseTransId">CaseTransId.</param>
        /// <param name="transType">TransType.</param>
        /// <returns>Task.</returns>
        private async Task JobEndDetailSendMessages(JObject response, string caseTransId, string transType)
        {
            var xmlPayload = JsonConvert.DeserializeXNode(response.ToString(), "ServiceRequestBdArgus");
            var xmlData =
            new XElement(
                "SiebelMessage",
                new XAttribute("MessageId", caseTransId),
                new XAttribute("EventTimestamp", DateTime.Parse(this.modifiedOn ?? DateTime.UtcNow.ToString()).ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture)),
                new XAttribute("TransType", transType),
                new XAttribute("MessageType", "Integration Object"),
                new XAttribute("IntObjectName", "Mercurius Case Detail Info ARGUS"),
                new XAttribute("IntObjectFormat", "Siebel Hierarchical"),
                new XElement("ListOfServiceRequestBreakdownArgus", xmlPayload?.Descendants("ServiceRequestBdArgus")));

            ServiceBusClient sbClient = new ServiceBusClient(this.azureServicebusConnectionstring);
            ServiceBusSender sender = sbClient.CreateSender(this.queueName);
            ServiceBusMessage message = new ServiceBusMessage(Encoding.UTF8.GetBytes(xmlData.ToString()))
            {
                MessageId = Guid.NewGuid().ToString(),
                ContentType = "application/xml",
                Subject = transType,
            };
            await sender.SendMessageAsync(message);
            ////IQueueClient sbClient = new QueueClient(connectionString: this.azureServicebusConnectionstring, this.queueName);
            ////var orderMessage = new Message(Encoding.UTF8.GetBytes(xmlData.ToString()))
            ////{
            ////    MessageId = Guid.NewGuid().ToString(),
            ////    ContentType = "application/xml",
            ////    Label = transType,
            ////};
            ////await sbClient.SendAsync(orderMessage);
            this.logger.LogTrace($"Successfully send data to servicebus queue " + this.queueName + " with Data " + xmlData);
        }
    }
}
