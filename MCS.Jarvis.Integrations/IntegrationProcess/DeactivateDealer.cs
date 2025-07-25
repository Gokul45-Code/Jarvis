// <copyright file="DeactivateDealer.cs" company="Microsoft.">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace IntegrationProcess
{
    using System.Globalization;
    using IntegrationProcess.Helper.Constants;
    using MCS.Jarvis.Integration.Base.Dynamics;
    using MCS.Jarvis.Integration.Base.Logging;
    using MCS.Jarvis.IntegrationProcess.Helper;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// DeactivateDealer.
    /// </summary>
    public class DeactivateDealer
    {
        private readonly IDynamicsApiClient dynamicsApiClient;

        /// <summary>
        /// logger object.
        /// </summary>
        private readonly ILoggerService logger;
        private readonly IntegrationHelper helper;
        private readonly List<HttpMessageContent> multipartContent = new List<HttpMessageContent>();
        private readonly Dictionary<string, JArray?> retrieveMasterList = new Dictionary<string, JArray?>();
        private int counter = 0;
        private string? recordkey;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeactivateDealer"/> class.
        /// DeactivateDealer Constructor.
        /// </summary>
        /// <param name="dynamicsApiClient">Dynamics Client.</param>
        /// <param name="logger">Logger.</param>
        public DeactivateDealer(IDynamicsApiClient dynamicsApiClient, ILoggerService logger)
        {
            this.dynamicsApiClient = dynamicsApiClient;
            this.logger = logger;
            this.helper = new IntegrationHelper(this.dynamicsApiClient, this.logger);
        }

        /// <summary>
        /// DeactivateDealerIntegrationProcess - Get Master Data, Executing Deactivating Request.
        /// </summary>
        /// <param name="payLoad">Payload.</param>
        /// <param name="timestamp">TimeStamp.</param>
        /// <returns>Task.</returns>
        /// <exception cref="ArgumentException">Argument Null Exception.</exception>
        public async Task<HttpResponseMessage?> DeactivateDealerIntegrationProcess(JObject payLoad, DateTime timestamp)
        {
            try
            {
                ////Checking Payload contains unique identifier
                if (payLoad != null && payLoad.TryGetValue("id", StringComparison.OrdinalIgnoreCase, out JToken? id))
                {
                    ////Getting All Master Data and Related Queries in single call aganist unique identifier from the payload "id".
                    await this.GetDealerMasterData(id.ToString());

                    // Validation For Timestamp
                    var accountRecord = this.retrieveMasterList.First(item => item.Key.ToUpper() == "accounts".ToUpper()).Value;
                    if (accountRecord?.ToList().Count == 0)
                    {
                        this.logger.LogException(new Exception("There was no dealer record in Jarvis with the given ID"));
                        throw new ArgumentException("There was no dealer record in Jarvis with the given ID");
                    }
                    else if (accountRecord != null && accountRecord.Count > 0
                           && accountRecord.First().ToObject<JObject>().TryGetValue("jarvis_ctditimestamp", StringComparison.OrdinalIgnoreCase, out JToken? createtimestamp) && createtimestamp.ToString() != string.Empty)
                    {
                        accountRecord.First().ToObject<JObject>().TryGetValue("accountid", StringComparison.OrdinalIgnoreCase, out JToken? dealerId);
                        this.recordkey = dealerId?.ToString();
                        DateTime jarvisTime = DateTime.Parse(createtimestamp.ToString());
                        int result = DateTime.Compare(timestamp, jarvisTime);
                        if (result <= 0)
                        {
                            this.logger.LogException(new Exception("Payload Timesamp should be greater than Dealer's Timestamp in Jarvis"));
                            throw new ArgumentException("Payload Timesamp should be greater than Dealer's Timestamp in Jarvis");
                        }
                    }

                    var configRecord = this.retrieveMasterList.First(item => item.Key.ToUpper() == Constants.JarvisIntegrationConfiguration.ToUpper()).Value;
                    if (configRecord != null && configRecord.Count > 0
                          && configRecord.First().ToObject<JObject>().TryGetValue(Constants.JarvisIntegrationMapping, StringComparison.OrdinalIgnoreCase, out JToken? intMapping))
                    {
                        // JsonLoadSettings
                        JObject dyn = JObject.Parse(intMapping.ToString());

                        // Dealer
                        if (dyn.TryGetValue("Dealers", StringComparison.OrdinalIgnoreCase, out JToken? dealer))
                        {
                            JObject payload = new JObject();
                            //// PBI#288892 - CTDIReactivate
                            //// payload.Add(CtdiConstants.JarvisStateCode, 1);
                            payload.Add(CtdiConstants.ExternalStatus, 334030001);
                            string? targetEntityDealer = DynamicsApiHelper.GetStringValueFromJObject(dealer.ToObject<JObject>(), Constants.TargetEntityName).ToLower();
                            this.multipartContent.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Patch, targetEntityDealer, "(" + this.recordkey + ")", Interlocked.Increment(ref this.counter), payload.ToString(), false));

                            if (dyn.TryGetValue("Brands", StringComparison.OrdinalIgnoreCase, out JToken? brandsMapping))
                            {
                                ////Retriving All BusinessPartnerBrands belong to Businesspartner
                                string? targetRelationshipBrandName = DynamicsApiHelper.GetStringValueFromJObject(brandsMapping.ToObject<JObject>(), Constants.TargetRelationshipName);
                                string? targetEntityBrand = DynamicsApiHelper.GetStringValueFromJObject(brandsMapping.ToObject<JObject>(), Constants.TargetEntityName).ToLower();
                                string? targetEntityBrandId = DynamicsApiHelper.GetStringValueFromJObject(brandsMapping.ToObject<JObject>(), Constants.TargetEntityId).ToLower();
                                if (targetEntityBrand != null && targetRelationshipBrandName != null)
                                {
                                    JToken? bpBrandsList = this.helper.SubgridValueFromTarget(this.retrieveMasterList, targetEntityDealer, targetRelationshipBrandName);
                                    if (bpBrandsList != null && bpBrandsList.Any())
                                    {
                                        this.DeactivateRecordList(bpBrandsList, targetEntityBrand, targetEntityBrandId);
                                    }
                                }

                                if (brandsMapping.ToObject<JObject>().TryGetValue("classes", StringComparison.OrdinalIgnoreCase, out JToken? brandClasses))
                                {
                                    string? targetRelationshipClassName = DynamicsApiHelper.GetStringValueFromJObject(brandClasses.ToObject<JObject>(), Constants.TargetRelationshipName);
                                    string? targetEntityClass = DynamicsApiHelper.GetStringValueFromJObject(brandClasses.ToObject<JObject>(), Constants.TargetEntityName).ToLower();
                                    string? targetEntityClassId = DynamicsApiHelper.GetStringValueFromJObject(brandClasses.ToObject<JObject>(), Constants.TargetEntityId).ToLower();
                                    if (targetEntityClass != null && targetRelationshipClassName != null)
                                    {
                                        JToken? bpBrandsCodeList = this.helper.SubgridValueFromTarget(this.retrieveMasterList, targetEntityDealer, targetRelationshipClassName);
                                        if (bpBrandsCodeList != null && brandClasses.Any())
                                        {
                                            this.DeactivateRecordList(bpBrandsCodeList, targetEntityClass, targetEntityClassId);
                                        }
                                    }
                                }

                                if (dyn != null && dyn.TryGetValue("OpeningHours", StringComparison.OrdinalIgnoreCase, out JToken? openHours))
                                {
                                    string? targetRelationshipOpenHoursName = DynamicsApiHelper.GetStringValueFromJObject(openHours.ToObject<JObject>(), Constants.TargetRelationshipName);
                                    string? targetEntityOpenHours = DynamicsApiHelper.GetStringValueFromJObject(openHours.ToObject<JObject>(), Constants.TargetEntityName).ToLower();
                                    string? targetEntityOpenHoursId = DynamicsApiHelper.GetStringValueFromJObject(openHours.ToObject<JObject>(), Constants.TargetEntityId).ToLower();
                                    if (targetEntityOpenHours != null && targetRelationshipOpenHoursName != null)
                                    {
                                        JToken? bpOpeningHoursList = this.helper.SubgridValueFromTarget(this.retrieveMasterList, targetEntityDealer, targetRelationshipOpenHoursName);
                                        if (bpOpeningHoursList != null && openHours.Any())
                                        {
                                            this.DeactivateRecordList(bpOpeningHoursList, targetEntityOpenHours, targetEntityOpenHoursId);
                                        }
                                    }
                                }

                                // BookableResourcs
                                if (dyn != null && dyn.TryGetValue(CtdiConstants.Bookableresourcecharacteristics, StringComparison.OrdinalIgnoreCase, out JToken? bookableresource))
                                {
                                    string? targetBookableResourceServiceRelation = DynamicsApiHelper.GetStringValueFromJObject(bookableresource.ToObject<JObject>(), Constants.TargetRelationshipName);
                                    string? targetEntityBookableResourceService = DynamicsApiHelper.GetStringValueFromJObject(bookableresource.ToObject<JObject>(), Constants.TargetEntityName).ToLower();
                                    string? targetEntityBookableResourceServicesId = DynamicsApiHelper.GetStringValueFromJObject(bookableresource.ToObject<JObject>(), Constants.TargetEntityId).ToLower();
                                    if (targetEntityBookableResourceService != null && targetBookableResourceServiceRelation != null)
                                    {
                                        JToken? bpBookableResource = this.helper.SubgridValueFromTarget(this.retrieveMasterList, targetEntityDealer, CtdiConstants.AccountbookableresourceAccountId);

                                        if (bpBookableResource != null)
                                        {
                                            var bookablecharecteristics = bpBookableResource.Children().Values(targetBookableResourceServiceRelation).Values().ToList();
                                            if (bookablecharecteristics != null && bookablecharecteristics.Any())
                                            {
                                                this.RelatedDeactivateRecordList(bookablecharecteristics, targetEntityBookableResourceService, targetEntityBookableResourceServicesId);
                                            }
                                        }
                                    }
                                }
                            }

                            ////Checking MultipartContent contains Data and executing whole payload request..
                            if (this.multipartContent.Count > 0)
                            {
                                ////ExecuteBatch...
                                var response = await this.dynamicsApiClient.ExecuteBatchRequest(this.multipartContent);
                                return response;
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
                    this.logger.LogException(new Exception("TLIP Payload does not contain unique indentifier"));
                    throw new ArgumentException("Payload does not contain unique indentifier");
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"CTDI Integration:" + ex.Message);
            }
        }

        /// <summary>
        /// GetDealerMasterData - Get dealer master data.
        /// </summary>
        /// <param name="sourceAccountNumber">SourceAcoountNumber.</param>
        /// <returns>Task.</returns>
        /// <exception cref="ArgumentException">Argument Null Exception.</exception>
        private async Task GetDealerMasterData(string sourceAccountNumber)
        {
            try
            {
                List<HttpMessageContent> masterContents = new List<HttpMessageContent>();
                this.logger.LogTrace("Enter into ConfigurationMaster Data");

                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.JarvisIntegrationConfiguration, string.Format(CrmQueries.JarvisIntegrationConfigurationQuery, CtdiConstants.JarvisCTDIIntegrationConfiguration.ToString().ToUpper()), 1, string.Empty, false));
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, CtdiConstants.Accounts, string.Format(CrmQueries.AccountDeactivateQuery, sourceAccountNumber), 1, string.Empty, false));

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
                                this.retrieveMasterList.Add(odataContext.ToString().Split("#")[1].ToString().Split("(")[0].ToString(), jsonData.GetValue("value")?.ToObject<JArray>());
                            }
                        }
                    }
                }

                if (this.retrieveMasterList.Count <= 0)
                {
                    this.logger.LogException(new ArgumentException("Getting IntegrationConfiguration,MasterData and Dealers record Failed"));
                    throw new ArgumentException("No MasterData,Configuration Record Retrieved");
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException($" Get Dealer Master Data Failed with Error Message - " + ex.Message);
            }
        }

        /// <summary>
        ///  DeactivateRecordList - Add records in the list for deactivate if records are active.
        /// </summary>
        /// <param name="recordList">RecordList.</param>
        /// <param name="targetEntity">TargetEntity.</param>
        /// <param name="targetEntityId">TargetEntityId.</param>
        private void DeactivateRecordList(JToken recordList, string targetEntity, string targetEntityId)
        {
            try
            {
                JObject payload = new JObject
                {
                    { CtdiConstants.JarvisStateCode, 1 },
                };

                if (recordList != null && recordList.Any())
                {
                    foreach (var rec in recordList.ToList())
                    {
                        if (rec.ToObject<JObject>().ContainsKey(CtdiConstants.JarvisStateCode) && rec.ToObject<JObject>().ContainsKey(targetEntityId)
                                  && rec.ToObject<JObject>().TryGetValue(CtdiConstants.JarvisStateCode, StringComparison.OrdinalIgnoreCase, out JToken? stateCodeValue) && Convert.ToInt32(stateCodeValue.Value<JToken>(), CultureInfo.InvariantCulture) == 0)
                        {
                            this.multipartContent.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Patch, targetEntity, "(" + rec.ToObject<JObject>()[targetEntityId].ToString().ToLower() + ")", Interlocked.Increment(ref this.counter), payload.ToString(), false));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException($" Deactivate Record List Failed with Error Message - " + ex.Message);
            }
        }

        /// <summary>
        /// Decativate records of the Related Enity records.
        /// </summary>
        /// <param name="recordList">RecordList.</param>
        /// <param name="targetEntity">TargetEntity.</param>
        /// <param name="targetEntityId">TargetEntityId.</param>
        /// <exception cref="ArgumentException">Argument Null Exception.</exception>
        private void RelatedDeactivateRecordList(List<JToken> recordList, string targetEntity, string targetEntityId)
        {
            try
            {
                JObject payload = new JObject
                {
                    { CtdiConstants.JarvisStateCode, 1 },
                };

                if (recordList != null && recordList.Any())
                {
                    foreach (var rec in recordList.ToList())
                    {
                        if (rec.ToObject<JObject>().ContainsKey(CtdiConstants.JarvisStateCode) && rec.ToObject<JObject>().ContainsKey(targetEntityId)
                                  && rec.ToObject<JObject>().TryGetValue(CtdiConstants.JarvisStateCode, StringComparison.OrdinalIgnoreCase, out JToken? stateCodeValue) && Convert.ToInt32(stateCodeValue.Value<JToken>(), CultureInfo.InvariantCulture) == 0)
                        {
                            this.multipartContent.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Patch, targetEntity, "(" + rec.ToObject<JObject>()[targetEntityId].ToString().ToLower() + ")", Interlocked.Increment(ref this.counter), payload.ToString(), false));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException($" Deactivate Record List Failed with Error Message - " + ex.Message);
            }
        }
    }
}
