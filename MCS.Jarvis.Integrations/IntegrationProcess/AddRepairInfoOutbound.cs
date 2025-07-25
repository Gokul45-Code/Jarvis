// <copyright file="AddRepairInfoOutbound.cs" company="Microsoft.">
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
    /// AddRepairInfoOutbound Class.
    /// </summary>
    public class AddRepairInfoOutbound
    {
        private readonly IDynamicsApiClient dynamicsApiClient;
        private readonly ILoggerService logger;
        private readonly IntegrationHelper helper;
        private readonly string azureServicebusConnectionstring;
        private readonly string queueName;
        private readonly Dictionary<string, JArray?> retrieveList = new Dictionary<string, JArray?>();

        /// <summary>
        /// Initializes a new instance of the <see cref="AddRepairInfoOutbound"/> class.
        /// AddRepairInfoOutbound Constructor.
        /// </summary>
        /// <param name="dynamicsApiClient">DynamicsApiClient.</param>
        /// <param name="config">Config.</param>
        /// <param name="logger">Logger.</param>
        public AddRepairInfoOutbound(IDynamicsApiClient dynamicsApiClient, IConfiguration config, ILoggerService logger)
        {
            this.dynamicsApiClient = dynamicsApiClient;
            this.logger = logger;
            this.helper = new IntegrationHelper(this.dynamicsApiClient, this.logger);
            this.azureServicebusConnectionstring = config.GetSection("ServiceBusConnection").Value;
            this.queueName = config.GetSection("OutboundUpdateCaseSpecQueue").Value;
        }

        /// <summary>
        /// IntegrationProcessAsync.
        /// </summary>
        /// <param name="repairInfoTransId">RepairInfoTransId.</param>
        /// <param name="entityData">EntityData.</param>
        /// <param name="transType">TransType.</param>
        /// <param name="translationType">TranslationType.</param>
        /// <returns>Async Task.</returns>
        /// <exception cref="ArgumentException">Argument Null Exception.</exception>
        public async Task IntegrationProcessAsync(string repairInfoTransId, string entityData, string transType, string translationType)
        {
            try
            {
                ////Checking Payload contains unique identifier
                if (!string.IsNullOrEmpty(repairInfoTransId) && !string.IsNullOrEmpty(transType) && !string.IsNullOrEmpty(translationType))
                {
                    string? triggeredBy = string.Empty;
                    if (!string.IsNullOrEmpty(entityData))
                    {
                        this.logger.LogTrace($"PluginContextData: {entityData}");
                        JObject record = this.helper.ConvertContextToObject(entityData);
                        if (record.HasValues)
                        {
                            triggeredBy = record.GetValue(Constants.TriggeredBy) != null ? record.GetValue(Constants.TriggeredBy)?.ToString() : string.Empty;
                            ////Getting All Master Data and Related Queries in single call aganist unique identifier from the payload "id".
                            await this.GetConfigMasterDataAsBatch(repairInfoTransId.ToString().ToLower(), triggeredBy);
                            var retrieveInc = this.retrieveList.First(item => item.Key.ToUpper() == Constants.JarvisRepairinformationtranslations.ToUpper()).Value?.First().ToObject<JObject>();
                            retrieveInc?.Merge(record);
                            if (retrieveInc != null)
                            {
                                this.retrieveList.First(item => item.Key.ToUpper() == Constants.JarvisRepairinformationtranslations.ToUpper()).Value?.RemoveAll();
                                this.retrieveList.First(item => item.Key.ToUpper() == Constants.JarvisRepairinformationtranslations.ToUpper()).Value?.Add(retrieveInc);
                            }
                        }
                        else
                        {
                            ////Getting All Master Data and Related Queries in single call aganist unique identifier from the payload "id".
                            await this.GetConfigMasterDataAsBatch(repairInfoTransId.ToString().ToLower(), triggeredBy);
                        }
                    }
                    else
                    {
                        ////Getting All Master Data and Related Queries in single call aganist unique identifier from the payload "id".
                        await this.GetConfigMasterDataAsBatch(repairInfoTransId.ToString().ToLower(), triggeredBy);
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
                                if (intConfigRecord.TryGetValue(Constants.JarvisRepairinformationtranslations, StringComparison.OrdinalIgnoreCase, out JToken? translationMappings))
                                {
                                    //// SRBdRepairInfoArgus to Case translation Payload generation.

                                    await this.RepairInfoToSRBdReportFaultArgus(translationMappings.ToObject<JObject>(), repairInfoTransId, transType, casePayload, passOutPayload, translationType);
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
        /// Get ConfigMasterDat as Batch for AddRepairInfoOutbound.
        /// </summary>
        /// <param name="repairInfoTransId">RepairInfoTransId.</param>
        /// <param name="triggeredBy">triggeredBy.</param>
        /// <returns>AsyncTask.</returns>
        /// <exception cref="ArgumentException">Argument Null Exception.</exception>
        private async Task GetConfigMasterDataAsBatch(string repairInfoTransId, string? triggeredBy)
        {
            try
            {
                List<HttpMessageContent> masterContents = new List<HttpMessageContent>();
                this.logger.LogTrace("Enter into ConfigurationMaster Data");
                //// Used configuration code to retrieve Config record
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.JarvisIntegrationConfiguration, string.Format(CrmQueries.JarvisIntegrationConfigurationQuery, "REPAIRINFO002".ToString().ToUpper()), 1, string.Empty, false));
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.JarvisRepairinformationtranslations, string.Format(CrmQueries.RepairInfoTranslationOutboundQuery, repairInfoTransId), 1, string.Empty, false));
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.JarvisLanguages, CrmQueries.JarvisStatecodeActiveQuery, 1, string.Empty, false));
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.JarvisBrands, CrmQueries.JarvisStatecodeActiveQuery, 1, string.Empty, false));
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.JarvisCountries, CrmQueries.JarvisStatecodeActiveQuery, 1, string.Empty, false));
                if (!string.IsNullOrEmpty(triggeredBy))
                {
                    masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.Users, string.Format(CrmQueries.GetUserDetails, triggeredBy), 1, string.Empty, false));
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
        /// RepairInfoToSRBdReportFaultArgus.
        /// </summary>
        /// <param name="mappings">Mapping.</param>
        /// <param name="repairInfoTransId">RepairInfoTransId.</param>
        /// <param name="transType">TransType.</param>
        /// <param name="casePayload">CasePaylaod.</param>
        /// <param name="passOutPayload">PassoutPayload.</param>
        /// <param name="translationType">translationType.</param>
        /// <returns>AsyncTask.</returns>
        /// <exception cref="ArgumentException">Argument Null Exception.</exception>
        private async Task RepairInfoToSRBdReportFaultArgus(JObject? mappings, string repairInfoTransId, string transType, JObject casePayload, JObject passOutPayload, string translationType)
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
                        //// For REPAIR INFO
                        if (item.ToObject<JObject>().TryGetValue("jarvis_RepairInformation", out JToken? originalRepairInfoDetail) && mappings != null
                            && mappings.TryGetValue("jarvis_repairinformation", out JToken? fieldMappingRepairInfo) && mappings.TryGetValue("jarvis_towingrental", out JToken fieldMappingTowingRental) && mappings.TryGetValue("jarvis_warrantyinformation", out JToken? fieldMappingWarrantyInfo))
                        {
                            string? originalRepairInfo = originalRepairInfoDetail.SelectToken("jarvis_repairinformation")?.ToString();
                            string? originalWarrantyInfo = originalRepairInfoDetail.SelectToken("jarvis_warrantyinformation")?.ToString();
                            string? originalTowingInfo = originalRepairInfoDetail.SelectToken("jarvis_towingrental")?.ToString();
                            string? originalPartsInfo = originalRepairInfoDetail.SelectToken("jarvis_partsinformation")?.ToString();

                            ////Repair Info Payload sent to Mercurius
                            if ((!string.IsNullOrEmpty(originalRepairInfo) || !string.IsNullOrEmpty(originalPartsInfo)) && (!string.IsNullOrWhiteSpace(originalRepairInfo) || !string.IsNullOrWhiteSpace(originalPartsInfo))
                                && fieldMappingRepairInfo != null && casePayload.HasValues && passOutPayload.HasValues && (translationType.ToUpper() == TranslationType.Create.ToUpper() || translationType.ToUpper() == TranslationType.RepairInfo.ToUpper()))
                            {
                                JObject finalRepairInfoPayload = new (casePayload);
                                JArray repairInfoPayload = this.SetMappingRepairInformationPayload(item, fieldMappingRepairInfo, TranslationType.RepairInfo.ToUpper());

                                ////Framing Comment from Repair Info Payload with PartsInformation.
                                if (repairInfoPayload != null && repairInfoPayload.First().HasValues)
                                {
                                    string commentValue = string.Format("Diagnose: {0} - Parts: {1}", repairInfoPayload.First?.SelectToken("Comment")?.ToString(), string.IsNullOrEmpty(originalPartsInfo) ? null : repairInfoPayload.First?.SelectToken("Comment2")?.ToString());
                                    repairInfoPayload.First().SelectToken("Comment2")?.Parent?.Remove();
                                    repairInfoPayload.First().SelectToken("Comment")?.Replace(commentValue);
                                }
                                //// All the repairInfo Payload as array of object
                                JObject repairPassOutInfo = this.FrameRepairInformationPaylaod(repairInfoPayload, passOutPayload);
                                //// Passout Object with Repair Info list converted into PassOut List and also adding with Case details in final Repair InfoPaylaod.
                                finalRepairInfoPayload.Add("ListOfSRBdPassoutLoginfoArgus", repairPassOutInfo);
                                await this.RepairInfoSendMessages(finalRepairInfoPayload, repairInfoTransId, transType);
                            }
                            ////Warranty Info Payload sent to Mercurius
                            if (!string.IsNullOrEmpty(originalWarrantyInfo) && !string.IsNullOrWhiteSpace(originalWarrantyInfo) && casePayload.HasValues && passOutPayload.HasValues && (translationType.ToUpper() == TranslationType.Create.ToUpper() || translationType.ToUpper() == TranslationType.Warranty.ToUpper()))
                            {
                                JObject finalWarrantyPayload = new (casePayload);
                                JArray warantyInfoPayload = this.SetMappingRepairInformationPayload(item, fieldMappingWarrantyInfo, TranslationType.Warranty);
                                JObject repairPassOutInfo = this.FrameRepairInformationPaylaod(warantyInfoPayload, passOutPayload);
                                finalWarrantyPayload.Add("ListOfSRBdPassoutLoginfoArgus", repairPassOutInfo);
                                await this.RepairInfoSendMessages(finalWarrantyPayload, repairInfoTransId, transType);
                            }
                            ////Towing Info Payload sent to Mercurius
                            if (!string.IsNullOrEmpty(originalTowingInfo) && !string.IsNullOrWhiteSpace(originalTowingInfo) && casePayload.HasValues && passOutPayload.HasValues && (translationType.ToUpper() == TranslationType.Create.ToUpper() || translationType.ToUpper() == TranslationType.TowingInfo.ToUpper()))
                            {
                                JObject finalTowingInfoPayload = new (casePayload);
                                JArray towingInfoInfoPayload = this.SetMappingRepairInformationPayload(item, fieldMappingTowingRental, TranslationType.TowingInfo);
                                JObject repairPassOutInfo = this.FrameRepairInformationPaylaod(towingInfoInfoPayload, passOutPayload);
                                finalTowingInfoPayload.Add("ListOfSRBdPassoutLoginfoArgus", repairPassOutInfo);
                                await this.RepairInfoSendMessages(finalTowingInfoPayload, repairInfoTransId, transType);
                            }
                        }
                        else
                        {
                            throw new ArgumentException($"RepairInfoToSRBdReportFaultArgus: No Field Mapping and RepairInformation not exists.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"ServiceRequestBdArgusToCase:" + ex.Message);
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
        /// SetMappingRepairInformationPayload - Setting Mapping for RepairInfo.
        /// </summary>
        /// <param name="item">Item.</param>
        /// <param name="fieldMappings">FieldMapping.</param>
        /// <param name="typeRepairInfo">TypeRepairInfo.</param>
        /// <returns>JArray.</returns>
        private JArray SetMappingRepairInformationPayload(JToken item, JToken fieldMappings, string typeRepairInfo)
        {
            JArray repairInfoList = new ();

            if (fieldMappings.ToObject<JObject>().TryGetValue("fieldMappings", out JToken? fieldMapping) && fieldMapping != null && item != null && item.HasValues)
            {
                this.helper.ValidateSetFieldMappingOutbound(item.ToObject<JObject>(), fieldMapping);
                JObject payload = this.helper.SetFieldMappingOutbound(item.ToObject<JObject>(), fieldMapping, this.retrieveList);
                payload.Add("Type", typeRepairInfo);
                repairInfoList.Add(payload);
            }

            return repairInfoList;
        }

        /// <summary>
        /// FrameRepairInformationPaylaod.
        /// </summary>
        /// <param name="repairInfoPayload">RepairInfoPayload.</param>
        /// <param name="passOutPayload">PassOutPayload.</param>
        /// <returns>Jobject.</returns>
        private JObject FrameRepairInformationPaylaod(JArray? repairInfoPayload, JObject passOutPayload)
        {
            JObject repairInfoObj = new ()
            {
                  { "SRBreakdownRepairinfoArgus", repairInfoPayload },
            };
            //// RepairInfo Object converted into Array.
            JObject repairInfoList = new (passOutPayload);
            repairInfoList.Add("ListOfSRBreakdownRepairinfoArgus", repairInfoObj);
            JArray repairInfoArray = new (repairInfoList);
            //// RepirInfo Array to Repair Info PassOut Object

            JObject repairPassOutInfo = new ()
            {
                 { "SRBdPassoutLoginfoArgus", repairInfoArray },
            };
            return repairPassOutInfo;
        }

        /// <summary>
        /// PassOutoSRBdPassoutLoginfoArgus.
        /// </summary>
        /// <param name="mappings">Mapping.</param>
        /// <returns>JObject.</returns>
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
        /// RepairInfoSendMessages.
        /// </summary>
        /// <param name="response">Response.</param>
        /// <param name="caseTransId">CaseTransId.</param>
        /// <param name="transType">TransType.</param>
        /// <returns>Task.</returns>
        private async Task RepairInfoSendMessages(JObject response, string caseTransId, string transType)
        {
            var xmlPayload = JsonConvert.DeserializeXNode(response.ToString(), "ServiceRequestBdArgus");
            var xmlData =
            new XElement(
                "SiebelMessage",
                new XAttribute("MessageId", caseTransId),
                new XAttribute("EventTimestamp", DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture)),
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
