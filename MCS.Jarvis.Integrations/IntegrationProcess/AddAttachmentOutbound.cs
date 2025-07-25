// <copyright file="AddAttachmentOutbound.cs" company="Microsoft.">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace IntegrationProcess
{
    using System.Text;
    using System.Web;
    using HtmlAgilityPack;
    using IntegrationProcess.Helper.Constants;
    using MCS.Jarvis.Integration.Base.Dynamics;
    using MCS.Jarvis.Integration.Base.Logging;
    using MCS.Jarvis.IntegrationProcess.Helper;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// AddAttachmentOutbound.
    /// </summary>
    public class AddAttachmentOutbound
    {
        private readonly IDynamicsApiClient dynamicsApiClient;
        private readonly IConfiguration config;
        private readonly ILoggerService logger;
        private readonly IntegrationHelper helper;
        private readonly Dictionary<string, JArray?> retrieveList = new Dictionary<string, JArray?>();
        private JObject sbMessage = new ();

        /// <summary>
        /// Initializes a new instance of the <see cref="AddAttachmentOutbound"/> class.
        /// AddAttachmentOutbound Constructor.
        /// </summary>
        /// <param name="dynamicsApiClient">dynamicsApiClient.</param>
        /// <param name="config">Config.</param>
        /// <param name="logger">Logger.</param>
        public AddAttachmentOutbound(IDynamicsApiClient dynamicsApiClient, IConfiguration config, ILoggerService logger)
        {
            this.dynamicsApiClient = dynamicsApiClient;
            this.config = config;
            this.logger = logger;
            this.helper = new IntegrationHelper(this.dynamicsApiClient, this.logger);
        }

        /// <summary>
        /// IntegrationProcessAsync.
        /// </summary>
        /// <param name="annotationId">annotationId.</param>
        /// <param name="entityData">EntityData.</param>
        /// <returns>Task Jobject.</returns>
        /// <exception cref="ArgumentException">Argument Null Exception.</exception>
        public async Task<JObject> IntegrationProcessAsync(string annotationId, string entityData)
        {
            try
            {
                string? modifiedBy = string.Empty;
                ////Checking Payload contains unique identifier
                if (!string.IsNullOrEmpty(annotationId))
                {
                    if (!string.IsNullOrEmpty(entityData))
                    {
                        this.logger.LogTrace($"PluginContextData: {entityData}");
                        JObject record = this.helper.ConvertContextToObject(entityData);
                        if (record.HasValues)
                        {
                            modifiedBy = record.GetValue(Constants.ModifiedBy) != null ? record.GetValue(Constants.ModifiedBy)?.ToString() : string.Empty;
                            ////Getting All Master Data and Related Queries in single call aganist unique identifier from the payload "id".
                            await this.GetConfigMasterDataAsBatch(annotationId.ToString().ToLower(), modifiedBy);
                            var retrieveNotes = this.retrieveList.First(item => item.Key.ToUpper() == Constants.Annotations.ToUpper()).Value?.First().ToObject<JObject>();
                            retrieveNotes?.Merge(record);
                            if (retrieveNotes != null)
                            {
                                this.retrieveList.First(item => item.Key.ToUpper() == Constants.Annotations.ToUpper()).Value?.RemoveAll();
                                this.retrieveList.First(item => item.Key.ToUpper() == Constants.Annotations.ToUpper()).Value?.Add(retrieveNotes);
                            }
                        }
                        else
                        {
                            await this.GetConfigMasterDataAsBatch(annotationId.ToString().ToLower(), modifiedBy);
                        }
                    }
                    else
                    {
                        await this.GetConfigMasterDataAsBatch(annotationId.ToString().ToLower(), modifiedBy);
                    }

                    var configRecord = this.retrieveList.First(item => item.Key.ToUpper() == Constants.JarvisIntegrationConfiguration.ToUpper()).Value;
                    if (configRecord != null && configRecord.Count > 0
                          && configRecord.First().ToObject<JObject>().TryGetValue(Constants.JarvisIntegrationMapping, StringComparison.OrdinalIgnoreCase, out JToken? intConfig) && intConfig != null)
                    {
                        JObject intConfigRecord = JObject.Parse(intConfig.ToString());

                        ////Retriving Incident IntegrationMappings
                        if (intConfigRecord != null && intConfigRecord.TryGetValue(Constants.Incident, StringComparison.OrdinalIgnoreCase, out JToken? intMapping))
                        {
                            //// ServiceRequestBdArgus to Incident Payload generation.
#pragma warning disable S1481 // Unused local variables should be removed
                            this.sbMessage = this.CaseToServiceRequestBdArgus(intMapping.ToObject<JObject>());

                            ////Retriving active Notes of incident and IntegrationMappings of Notes
                            if (intConfigRecord.TryGetValue(Constants.Annotations, StringComparison.OrdinalIgnoreCase, out JToken? annotationMapping))
                            {
                                ////Incident.Notes.Attachment to serviceRequestBdAttachmentArgus Payload generation.
                                JArray annotationPaylaod = this.NoteToserviceRequestBdAttachmentArgus(annotationMapping.ToObject<JObject>());
                                JObject keyValues = new ()
                                {
                                    { SiebelConstants.ServiceRequestBdAttachmentArgus, annotationPaylaod },
                                };
                                this.sbMessage.Add(SiebelConstants.ListOfServiceRequestBdAttachmentArgus, keyValues);
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
                        throw new ArgumentException("No Integration Configuration Found/ Integration is not Active, Pleae activate the configuraiton record.");
                    }
                    else
                    {
                        this.logger.LogException(new Exception("No Integration Configuration Found/ Integration is not Active, Pleae activate the configuraiton record."));
                        throw new ArgumentException("No Integration Configuration Found/ Integration is not Active, Pleae activate the configuraiton record.");
                    }
                }
                else
                {
                    this.logger.LogException(new Exception("Payload does not contain unique indentifier IncidentId"));
                    throw new ArgumentException("Payload does not contain unique indentifier IncidentId");
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"IntegrationProcessAsync:" + ex.Message);
            }
        }

        /// <summary>
        /// Get Config Master Data as Batch - AnnotationId.
        /// </summary>
        /// <param name="sourceAnnotationId">SourceAnnotationId.</param>
        /// <returns>Task.</returns>
        /// <exception cref="ArgumentException">Argument Null Exception.</exception>
        private async Task GetConfigMasterDataAsBatch(string sourceAnnotationId, string? modifiedBy)
        {
            try
            {
                List<HttpMessageContent> masterContents = new List<HttpMessageContent>();
                this.logger.LogTrace("Enter into ConfigurationMaster Data");
                //// Used configuration code to retrieve Config record
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.JarvisIntegrationConfiguration, string.Format(CrmQueries.JarvisIntegrationConfigurationQuery, "ADDATTACH002".ToString().ToUpper()), 1, string.Empty, false));
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.Annotations, string.Format(CrmQueries.JarvisAttachmentOutboundQuery, sourceAnnotationId), 1, string.Empty, false));
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
        /// Note to serviceRequestBdAttachmentArgus.
        /// </summary>
        /// <param name="mappings">Mappings.</param>
        /// <returns>Jarray.</returns>
        /// <exception cref="ArgumentException">Argument Null Exception.</exception>
        private JArray NoteToserviceRequestBdAttachmentArgus(JObject? mappings)
        {
            try
            {
                JArray noteList = new JArray();
                string? sourceEntity = DynamicsApiHelper.GetStringValueFromJObject(mappings, Constants.SourceEntityName).ToLower();
                var matchMasterRecord = this.retrieveList.First(item => item.Key.ToUpper().Contains(sourceEntity.ToUpper())).Value?.ToList();
                if (matchMasterRecord != null && matchMasterRecord.Count > 0)
                {
                    foreach (var item in matchMasterRecord)
                    {
                        if (item != null && item.ToObject<JObject>() != null && mappings != null && mappings.TryGetValue(Constants.JarvisFieldMappings, out JToken? fieldMapping) && fieldMapping != null)
                        {
                            this.helper.ValidateSetFieldMappingOutbound(item.ToObject<JObject>(), fieldMapping);
                            JObject payload = this.helper.SetFieldMappingOutbound(item.ToObject<JObject>(), fieldMapping, this.retrieveList);
                            payload.TryGetValue(AttachmentConstants.ActivityFileName, StringComparison.OrdinalIgnoreCase, out JToken? fileName);
                            payload.TryGetValue(AttachmentConstants.AttachmentId, StringComparison.OrdinalIgnoreCase, out JToken? docmentBody);
                            if (fileName != null && docmentBody != null)
                            {
                                payload.SelectToken(AttachmentConstants.ActivityFileName)?.Replace(fileName.ToString());
                                payload.SelectToken(AttachmentConstants.ActivityFileExt)?.Replace(fileName.ToString().Split(".").Last());

                                JObject attachmentId = new JObject();

                                attachmentId.Add($"#{AttachmentConstants.Text}", docmentBody);
                                attachmentId.Add($"@{AttachmentConstants.AttachmentIsTextData}", false);
                                attachmentId.Add($"@{AttachmentConstants.Extension}", fileName.ToString().Split(".").Last());
                                attachmentId.Add($"@{AttachmentConstants.ContentId}", item.SelectToken(AttachmentConstants.EntityId));

                                payload.SelectToken(AttachmentConstants.AttachmentId)?.Replace(attachmentId);
                            }

                            var pageContent = payload.SelectToken("ActivityComments");
                            if (pageContent?.ToString() != null)
                            {
                                HtmlDocument htmlDcoument = new HtmlDocument();
                                htmlDcoument.LoadHtml(pageContent.ToString());
                                string plainText = this.ConvertHtmlToText(htmlDcoument);
                                payload.SelectToken("ActivityComments")?.Replace(plainText);
                            }

                            noteList.Add(payload);
                        }
                    }
                }

                return noteList;
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"NoteToserviceRequestBdAttachmentArgus:" + ex.Message);
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
        /// ConvertHtmlToText.
        /// </summary>
        /// <param name="htmlContent">htrmlContent.</param>
        /// <returns>string.</returns>
        private string ConvertHtmlToText(HtmlDocument htmlContent)
        {
            var sb = new StringBuilder();
            if (!string.IsNullOrEmpty(htmlContent.DocumentNode.InnerText) || !string.IsNullOrWhiteSpace(htmlContent.DocumentNode.InnerText))
            {
                foreach (var node in htmlContent.DocumentNode.DescendantsAndSelf())
                {
                    if (!node.HasChildNodes)
                    {
                        string text = node.InnerText;
                        if (!string.IsNullOrEmpty(text))
                        {
                            sb.AppendLine(text.Trim());
                        }
                    }
                }

                if (!string.IsNullOrEmpty(sb.ToString()) || !string.IsNullOrWhiteSpace(sb.ToString()))
                {
                    return HttpUtility.HtmlDecode(sb.ToString());
                }
            }

            return string.Empty;
        }
    }
}