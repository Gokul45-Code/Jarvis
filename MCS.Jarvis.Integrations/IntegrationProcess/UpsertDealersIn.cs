// <copyright file="UpsertDealersIn.cs" company="Microsoft.">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace IntegrationProcess
{
    using System.Collections.Generic;
    using System.Globalization;
    using IntegrationProcess.Helper.Constants;
    using MCS.Jarvis.Integration.Base.Dynamics;
    using MCS.Jarvis.Integration.Base.Logging;
    using MCS.Jarvis.IntegrationProcess.Helper;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// UpsertDealersIn - Upsert Dealer.
    /// </summary>
    public class UpsertDealersIn
    {
        /// <summary>
        /// Dynamics application client.
        /// </summary>
        private readonly IDynamicsApiClient dynamicsApiClient;

        /// <summary>
        /// logger object.
        /// </summary>
        private readonly ILoggerService logger;
        private readonly IntegrationHelper helper;
        private readonly List<HttpMessageContent> multipartContent = new List<HttpMessageContent>();
        private readonly Dictionary<string, JArray?> retrieveList = new Dictionary<string, JArray?>();
        private int counter = 0;
        private JToken? jarvisTeams;
        private bool isCreate = false;
        private JToken? bpBrandsList;
        private List<JToken>? bookableResourceList;
        private JToken? lunchHour;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpsertDealersIn"/> class.
        /// UpsertDealersIn - Constructor.
        /// </summary>
        /// <param name="dynamicsApiClient">DynamicsApiClient.</param>
        /// <param name="logger">Logger.</param>
        public UpsertDealersIn(IDynamicsApiClient dynamicsApiClient, ILoggerService logger)
        {
            this.dynamicsApiClient = dynamicsApiClient;
            this.logger = logger;
            this.helper = new IntegrationHelper(this.dynamicsApiClient, this.logger);
        }

        /// <summary>
        /// IntegrationProcessAsync - Get Master Data, Calling Field Validation , Executing Inbound Request.
        /// </summary>
        /// <param name="payLoad">Paylaod.</param>
        /// <param name="timestamp">TimeStamp.</param>
        /// <returns>Task.</returns>
        /// <exception cref="ArgumentException">Argument Null Exception.</exception>
        public async Task<HttpResponseMessage> IntegrationProcessAsync(JObject payLoad, DateTime timestamp)
        {
            try
            {
                ////Checking Payload contains unique identifier
                if (payLoad != null && payLoad.TryGetValue(Constants.Id, StringComparison.OrdinalIgnoreCase, out JToken? id))
                {
                    ////Getting All Master Data and Related Queries in single call aganist unique identifier from the payload "id".
                    await this.GetConfigMasterDataAsBatch(id.ToString());
                    //// Get Jarvis Teams for Owner Team
                    this.jarvisTeams = this.retrieveList.First(item => item.Key.ToUpper() == Constants.Teams.ToUpper()).Value;

                    // Validation For Timestamp
                    var accountRecord = this.retrieveList.First(item => item.Key.ToUpper() == Constants.Accounts.ToUpper()).Value;
                    if (accountRecord != null && accountRecord.Count > 0
                          && accountRecord.First().ToObject<JObject>().TryGetValue(CtdiConstants.DealerCTDITimestamp, StringComparison.OrdinalIgnoreCase, out JToken? createtimestamp) && createtimestamp.ToString() != string.Empty)
                    {
                        DateTime jarvisTime = DateTime.Parse(createtimestamp.ToString());
                        int result = DateTime.Compare(timestamp, jarvisTime);
                        if (result <= 0)
                        {
                            this.logger.LogException(new Exception("Payload Timesamp should be greater than Dealer's Timestamp in Jarvis"));
                            throw new ArgumentException("Payload Timestamp should be greater than Dealer's Timestamp in Jarvis");
                        }
                    }

                    var configRecord = this.retrieveList.First(item => item.Key.ToUpper() == Constants.JarvisIntegrationConfiguration.ToUpper()).Value;
                    if (configRecord != null && configRecord.Count > 0
                          && configRecord.First().ToObject<JObject>().TryGetValue(Constants.JarvisIntegrationMapping, StringComparison.OrdinalIgnoreCase, out JToken? intMapping))
                    {
                        // JsonLoadSettings
                        JObject dyn = JObject.Parse(intMapping.ToString());

                        // Dealers IntegrationMappings Validation
                        if (dyn != null && dyn.TryGetValue(Constants.Dealers, StringComparison.OrdinalIgnoreCase, out JToken? validationDealer))
                        {
                            // ValidateSetFieldMapping Overwritable one for Dealer only.
                            this.helper.ValidateSetFieldMappingWithOverWritable(payLoad, validationDealer, this.retrieveList);

                            // Brands IntegrationMapping Validation
                            if (dyn.TryGetValue(Constants.Brands, StringComparison.OrdinalIgnoreCase, out JToken? brandsMapping) && payLoad.TryGetValue(Constants.Brands, StringComparison.OrdinalIgnoreCase, out JToken? brandsPayload))
                            {
                                foreach (JToken brand in brandsPayload)
                                {
                                    // Brand Validation
                                    this.helper.ValidateSetFieldMapping(brand.ToObject<JObject>(), brandsMapping, this.retrieveList, Constants.JarvisBrand);

                                    if (brandsMapping.ToObject<JObject>().TryGetValue(Constants.Classes, StringComparison.OrdinalIgnoreCase, out JToken? brandClasses) && brand.ToObject<JObject>().TryGetValue(Constants.Classes, StringComparison.OrdinalIgnoreCase, out JToken? brandClassesPayload))
                                    {
                                        // Class Validation
                                        foreach (JToken classes in brandClassesPayload)
                                        {
                                            this.helper.ValidateSetFieldMapping(classes.ToObject<JObject>(), brandClasses, this.retrieveList, Constants.JarvisClass);
                                        }
                                    }
                                }

                                if (dyn != null && dyn.TryGetValue(Constants.OpeningHours, StringComparison.OrdinalIgnoreCase, out JToken? openHours) && payLoad.TryGetValue(Constants.JarvisOpenHours, StringComparison.OrdinalIgnoreCase, out JToken? openHoursPayload))
                                {
                                    // openHours Validation
                                    foreach (JToken openHour in openHoursPayload)
                                    {
                                        this.helper.ValidateSetFieldMapping(openHour.ToObject<JObject>(), openHours, this.retrieveList, Constants.JarvisOpenHours);
                                    }
                                }
                            }
                        }

                        ////Framing Dealers Payload
                        if (dyn != null && dyn.TryGetValue(Constants.Dealers, StringComparison.OrdinalIgnoreCase, out JToken? dealer))
                        {
                            dyn.TryGetValue("BillingAddress", StringComparison.OrdinalIgnoreCase, out JToken? billAddrsMapping);
                            //// Dealers to BusinessPartner Payload generation.
                            var (contentId, targetEntity, accountid, retrieveAccountType) = this.DealerToBusinessPartner(payLoad, dealer.ToObject<JObject>(), timestamp, billAddrsMapping?.ToObject<JObject>());
                            if (dyn.TryGetValue(Constants.Brands, StringComparison.OrdinalIgnoreCase, out JToken? brandsMapping) && payLoad.TryGetValue(Constants.Brands, StringComparison.OrdinalIgnoreCase, out JToken? brandsPayload))
                            {
                                ////Retriving All BusinessPartnerBrands belong to Businesspartner
                                string? targetRelationshipName = DynamicsApiHelper.GetStringValueFromJObject(brandsMapping.ToObject<JObject>(), Constants.TargetRelationshipName);

                                if (!this.isCreate && this.retrieveList != null && targetEntity != null && targetRelationshipName != null)
                                {
                                    this.bpBrandsList = this.helper.SubgridValueFromTarget(this.retrieveList, targetEntity, targetRelationshipName);
                                }

                                foreach (JToken brand in brandsPayload)
                                {
                                    //// Brands to BusinessPartnerBrands Payload generation.
                                    this.BrandsToBusinessPartnerBrands(brand.ToObject<JObject>(), brandsMapping.ToObject<JObject>(), contentId, this.bpBrandsList, id.ToString());
                                }

                                ////DeactivateBrandsNotexistsinPaylaod...
                                ////Delete Classes Paylaod...
                                ////OnlyApplicableInOnUpdateScenarios...
                                if (!this.isCreate)
                                {
                                    /////Deactivating Non matched BusinesPartner Brands from Brands Paylaod.
                                    this.DeactivateNonMatachedBusinesPartnerBrands();
                                    ////DeleteBusinessPartnerBrandsCode
                                    if (brandsMapping != null && brandsMapping.ToObject<JObject>().ContainsKey(Constants.Classes) && brandsMapping.ToObject<JObject>().TryGetValue(Constants.Classes, StringComparison.OrdinalIgnoreCase, out JToken? classMappings))
                                    {
                                        string? targetCodesRelationshipName = DynamicsApiHelper.GetStringValueFromJObject(classMappings.ToObject<JObject>(), Constants.TargetRelationshipName);
                                        if (!string.IsNullOrEmpty(targetEntity) && !string.IsNullOrEmpty(targetCodesRelationshipName))
                                        {
                                            this.DeleteBusinessPartnerBrandsCodes(targetEntity, targetCodesRelationshipName);
                                        }
                                    }
                                }
                                ////Only Required in Create Scenarios to Create OpeningHours
                                else
                                {
                                    if (dyn.TryGetValue(Constants.OpeningHours, StringComparison.OrdinalIgnoreCase, out JToken? openHoursMapping))
                                    {
                                        //// OpeningHours - Custom Logic Cant be configured.
                                        this.BusinessPartnerOpeningHours(payLoad, contentId, openHoursMapping.ToObject<JObject>());
                                    }
                                }

                                if (this.retrieveList != null && targetEntity != null && payLoad.SelectToken("latitude") != null && !string.IsNullOrEmpty(payLoad.SelectToken("latitude")?.ToString()) && payLoad.SelectToken("longitude") != null && !string.IsNullOrEmpty(payLoad.SelectToken("longitude")?.ToString()))
                                {
                                    dyn.TryGetValue(CtdiConstants.Bookableresourcecharacteristics, StringComparison.OrdinalIgnoreCase, out JToken? bookableresourcecharacteristics);
                                    this.BookableResource(payLoad, contentId, id.ToString(), bookableresourcecharacteristics?.ToObject<JObject>(), payLoad.SelectToken("name")?.ToString(), accountid, targetEntity, retrieveAccountType);
                                    if (!this.isCreate)
                                    {
                                        this.DeleteBookableResourceService(bookableresourcecharacteristics?.ToObject<JObject>());
                                    }
                                }
                            }

                            ////Checking MultipartContent contains Data and executing whole payload request..
                            if (this.multipartContent.Count > 0)
                            {
                                ////ExecuteBatch...
                                var response = await this.dynamicsApiClient.ExecuteBatchRequest(this.multipartContent);
                                if (response != null)
                                {
                                    return response;
                                }
                            }

                            this.logger.LogException(new Exception("Error in Generating Payload"));
                            throw new ArgumentException("Error in Generating Payload");
                        }

                        this.logger.LogException(new Exception("No Dealer Integration Configuration Found/ Integration is not Active, Pleae activate the configuraiton record."));
                        throw new ArgumentException($"No Dealer Integration Configuration Found. Integration is not Active, Pleae activate the configuraiton record.");
                    }
                    else
                    {
                        this.logger.LogException(new Exception("No Integration Configuration Found. Integration is not Active, Pleae activate the configuraiton record."));
                        throw new ArgumentException($"No Integration Configuration Found. Integration is not Active, Pleae activate the configuraiton record.");
                    }
                }
                else
                {
                    this.logger.LogException(new Exception("TLIP Payload does not contain unique indentifier"));
                    throw new ArgumentException($"Payload does not contain unique indentifier");
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"CTDI Integration:" + ex.Message);
            }
        }

        /// <summary>
        /// DealerToBusinessPartner - Set Dealer Payload.
        /// </summary>
        /// <param name="currentPayload">CurrentPayload.</param>
        /// <param name="mappings">Mappings.</param>
        /// <param name="timestamp">TimeStamp.</param>
        /// <returns>int,String,String.</returns>
        /// <exception cref="ArgumentException">Argument Null Exception.</exception>
        private (int, string, string, (bool, JToken? value)) DealerToBusinessPartner(JObject currentPayload, JObject? mappings, DateTime timestamp, JObject? billAddrsMappings)
        {
            try
            {
                JObject payload = new JObject();

                string? sourceField = DynamicsApiHelper.GetStringValueFromJObject(mappings, Constants.SourceFieldSchema);
                string? targetEntity = DynamicsApiHelper.GetStringValueFromJObject(mappings, Constants.TargetEntityName).ToLower();
                string? targetLookupEntityName = DynamicsApiHelper.GetStringValueFromJObject(mappings, Constants.TargetLookupEntityName).ToLower();
                string? targetFieldSchema = DynamicsApiHelper.GetStringValueFromJObject(mappings, Constants.TargetLookupFieldSchema);
                string? targetLookupEntity = DynamicsApiHelper.GetStringValueFromJObject(mappings, Constants.TargetLookupEntity);

                ////RetriveAccountValueFromTarget. Check Account is present or not and assign Guid of Account.
                var retrieveAccount = this.helper.LookupValueFromTarget(this.retrieveList, currentPayload, sourceField, targetLookupEntity, targetFieldSchema, targetLookupEntityName);
                //// 574704 - MC for retrieve Account.Jarvis_AccountType.
                (bool, JToken?) retrieveAccountType = (false, null);
                string recordkey;
                if (retrieveAccount.Item1 && retrieveAccount.value != null)
                {
                    recordkey = retrieveAccount.value.ToString();
                    ////Retrieve Account jarvis_accountType from Target.
                    retrieveAccountType = this.helper.LookupValueFromTarget(this.retrieveList, currentPayload, sourceField, targetLookupEntity, targetFieldSchema, "jarvis_accounttype");
                }
                else
                {
                    this.isCreate = true;
                    recordkey = Guid.NewGuid().ToString();
                }

                //// Framing Dealers to Account Payload
                if (mappings != null && mappings.TryGetValue(Constants.JarvisFieldMappings, out JToken? fieldMapping) && fieldMapping != null && billAddrsMappings != null)
                {
                    // SetFieldMapping with Overwritable for Dealer Only.
                    payload = this.helper.SetFieldMappingWithOverWritable(currentPayload, fieldMapping, this.retrieveList);

                    // Framing Billing Address if Exist with Code 0
                    currentPayload.TryGetValue("addresses", StringComparison.OrdinalIgnoreCase, out JToken? addresses);
                    if (addresses != null && addresses.ToList().Count > 0)
                    {
                        foreach (var billAddressess in addresses.ToList())
                        {
                            if (billAddressess.ToObject<JObject>().TryGetValue("code", StringComparison.OrdinalIgnoreCase, out JToken? code) && code != null && code.ToString() == "0")
                            {
                                billAddrsMappings.TryGetValue(Constants.JarvisFieldMappings, out JToken? addressMapping);
                                //// SetFieldMapping with Overwritable for Dealer Only,Hence Address also need to be overwritable.
                                payload.Merge(this.helper.SetFieldMappingWithOverWritable(billAddressess.ToObject<JObject>(), addressMapping, this.retrieveList));
                                break;
                            }
                        }
                    }

                    // Setting Supported Language Based On External Language.
                    payload.Merge(this.SetSupportedLanguage(currentPayload, payload));
                }
                else
                {
                    this.logger.LogWarning("NoConfiguration field mapping is statisfied.");
                }

                // Framing Dealer Content for Batch
                if (payload.Count > 0)
                {
                    // Set Dealer Owner
                    JToken? token = this.SetOwnerTeam(mappings);
                    ////Defaulting Values for BusinespartnerBrands and Also adding buisnesspartner Reference for CTDI Integration.
                    payload.Add(CtdiConstants.OwnerId, token);
                    payload.Add(CtdiConstants.DealerAddress1, DealerAddressType.Primary);
                    payload.Add(CtdiConstants.DealerJarvisSource, DealerJarvisSource.CTDI);
                    payload.Add(CtdiConstants.DealerAccountType, DealerAccountType.Dealer);
                    payload.Add(CtdiConstants.DealerCTDITimestamp, timestamp);
                    payload.Add(CtdiConstants.ExternalStatus, 334030000);
                    payload.Add(CtdiConstants.OneCaseStatus, 334030001);

                    // removing currency and language when update
                    // 574704 - CTDI to revert DealerAccountType.
                    if (!this.isCreate)
                    {
                        payload.Remove(CtdiConstants.OneCaseStatus);
                        payload.Remove(CtdiConstants.DealerJarvisExternalCurrencyId);
                        payload.Remove(CtdiConstants.DealerAccountType);
                        //// Removing both External and Supported Language.
                        payload.Remove(CtdiConstants.DealerJarvisLanguage);
                        payload.Remove(CtdiConstants.DealerExternalJarvisLanguage);
                    }

                    this.multipartContent.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Patch, targetEntity, "(" + recordkey + ")", Interlocked.Increment(ref this.counter), payload.ToString(), false));
                }

                return (this.counter, targetEntity, recordkey, retrieveAccountType);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"DealerToBusinessPartner:" + ex.Message);
            }
        }

        /// <summary>
        /// BrandsToBusinessPartnerBrands - Set Brand Payload.
        /// </summary>
        /// <param name="currentPayload">CurrentPayload.</param>
        /// <param name="mappings">Mappings.</param>
        /// <param name="sourceContentId">sourceContentId.</param>
        /// <param name="retrieveValues">retrieveValues.</param>
        /// <param name="accountNumber">accountNumber.</param>
        /// <exception cref="ArgumentException">Argument Null Exception.</exception>
        private void BrandsToBusinessPartnerBrands(JObject? currentPayload, JObject? mappings, int sourceContentId, JToken? retrieveValues, string accountNumber)
        {
            try
            {
                JObject payload = new JObject();
                string brandRecordkey;
                string? sourceField = DynamicsApiHelper.GetStringValueFromJObject(mappings, Constants.SourceFieldSchema);
                string? targetEntity = DynamicsApiHelper.GetStringValueFromJObject(mappings, Constants.TargetEntityName).ToLower();
                string? targetLookupEntityName = DynamicsApiHelper.GetStringValueFromJObject(mappings, Constants.TargetLookupEntityName).ToLower();
                string? targetFieldSchema = DynamicsApiHelper.GetStringValueFromJObject(mappings, Constants.TargetLookupFieldSchema);
                string? targetLookupEntity = DynamicsApiHelper.GetStringValueFromJObject(mappings, Constants.TargetLookupEntity);

                ////Retrieving Brands Match from the paylaod to check is present in CRM BPBrands.
                var retrieveBrand = this.helper.LookupValueFromTarget(this.retrieveList, currentPayload, sourceField, targetLookupEntity, targetFieldSchema, targetLookupEntityName);

                ////Check If its create then No need to pull the subgrid value from Payload
                if (!this.isCreate && retrieveValues != null && retrieveValues.ToList().Count > 0 && targetEntity != null && (retrieveBrand.Item1 && retrieveBrand.value != null))
                {
                    var recordMatch = retrieveValues.Where(item => item[CtdiConstants.JarvisBrandValue]?.ToString().ToUpper() == retrieveBrand.value.ToString().ToUpper()).ToList();

                    // Busines parnter brands from Payload is matching with Jarvis BPBrands.
                    if (recordMatch != null && recordMatch.Count > 0 && recordMatch.First().ToObject<JObject>().TryGetValue(CtdiConstants.JarvisBusinessPartnerBrandId, StringComparison.OrdinalIgnoreCase, out JToken? value) && value != null && this.bpBrandsList != null)
                    {
                        brandRecordkey = value.ToString();
                        ////Remove from the matched value from parent list so we can reuse to deactivate....
                        this.bpBrandsList.First(item => item[CtdiConstants.JarvisBrandValue]?.ToString().ToUpper() == retrieveBrand.value.ToString().ToUpper()).Remove();
                    }

                    // NotMatched from Payload then create in Jarvise
                    else
                    {
                        brandRecordkey = Guid.NewGuid().ToString();
                    }
                }
                else
                {
                    brandRecordkey = Guid.NewGuid().ToString();
                }

                if (mappings != null && mappings.TryGetValue(Constants.JarvisFieldMappings, out JToken? fieldMapping) && fieldMapping != null)
                {
                    payload = this.helper.SetFieldMapping(currentPayload, fieldMapping, this.retrieveList);
                }
                else
                {
                    this.logger.LogWarning("NoConfiguration field mapping is statisfied.");
                }

                if (payload.Count > 0 && !string.IsNullOrEmpty(targetEntity))
                {
                    // set brand owner
                    JToken? token = this.SetOwnerTeam(mappings);
                    ////Activate if in case its already deactivate in jarvis.
                    ////Defaulting Values for BusinessPartnerBrands for CTDI Integration.
                    payload.Add(CtdiConstants.OwnerId, token);
                    payload.Add(CtdiConstants.JarvisStateCode, 0);
                    payload.Add(CtdiConstants.JarvisBusinessPartnerId, accountNumber);
                    payload.Add(CtdiConstants.JarvisBusinessPartnerOdataBind, string.Format("${0}", sourceContentId));
                    var brandContentId = Interlocked.Increment(ref this.counter);
                    this.multipartContent.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Patch, targetEntity, "(" + brandRecordkey + ")", brandContentId, payload.ToString(), false));
                    //// Classe --> BusinessPartnerBrandsCode
                    if (mappings != null && mappings.TryGetValue("classes", StringComparison.OrdinalIgnoreCase, out JToken? classesMapping) && currentPayload != null && currentPayload.TryGetValue("classes", StringComparison.OrdinalIgnoreCase, out JToken? classesPayload))
                    {
                        foreach (var classes in classesPayload)
                        {
                            //// Classes to BusinessPartnerBrandsCode Payload generation.
                            this.ClassesToBusinessPartnerBrandsCode(classes.ToObject<JObject>(), classesMapping.ToObject<JObject>(), brandContentId, sourceContentId);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"BrandsToBusinessPartnerBrands:" + ex.Message);
            }
        }

        /// <summary>
        /// ClassesToBusinessPartnerBrandsCode - Set Class Payload.
        /// </summary>
        /// <param name="currentPayload">CurrentPayload.</param>
        /// <param name="mappings">Mappings.</param>
        /// <param name="brandsContentId">BrandsContentId.</param>
        /// <param name="parentContentId">ParentContentId.</param>
        /// <exception cref="ArgumentException">Argument Null Exception.</exception>
        private void ClassesToBusinessPartnerBrandsCode(JObject? currentPayload, JObject? mappings, int brandsContentId, int parentContentId)
        {
            try
            {
                JObject payload;
                string classesRecordKey = Guid.NewGuid().ToString();
                string? targetEntity = DynamicsApiHelper.GetStringValueFromJObject(mappings, Constants.TargetEntityName).ToLower();
                if (currentPayload != null && currentPayload.Count > 0 && mappings != null && mappings.Count > 0)
                {
                    if (mappings.TryGetValue(Constants.JarvisFieldMappings, out JToken? fieldMapping) && fieldMapping != null)
                    {
                        payload = this.helper.SetFieldMapping(currentPayload, fieldMapping, this.retrieveList);
                        if (payload != null && payload.Count > 0 && !string.IsNullOrEmpty(targetEntity))
                        {
                            // set brandcode owner
                            JToken? token = this.SetOwnerTeam(mappings);

                            payload.Add(CtdiConstants.OwnerId, token);
                            payload.Add(CtdiConstants.JarvisBusinessPartnerOdataBind, string.Format("${0}", parentContentId));
                            payload.Add(CtdiConstants.JarvisBusinessPartnerBrandOdataBind, string.Format("${0}", brandsContentId));
                            this.multipartContent.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Patch, targetEntity, "(" + classesRecordKey + ")", Interlocked.Increment(ref this.counter), payload.ToString(), false));
                        }
                    }
                    else
                    {
                        this.logger.LogWarning("NoConfiguration field mapping is statisfied.");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"ClassesToBusinessPartnerBrandsCode:" + ex.Message);
            }
        }

        /// <summary>
        /// DeactivateNonMatachedBusinesPartnerBrands - Deactivate brand list.
        /// </summary>
        private void DeactivateNonMatachedBusinesPartnerBrands()
        {
            try
            {
                JObject payload = new JObject
                {
                    { CtdiConstants.JarvisStateCode, 1 },
                };
                var targetEntity = CtdiConstants.JarvisBusinessPartnerBrandses;
                //// if BusinessPartnerBrandsList Contains Data then Deactivate the BusinessPartnerBrands.
                if (this.bpBrandsList != null && this.bpBrandsList.Any())
                {
                    foreach (var bpBrands in this.bpBrandsList.ToList())
                    {
                        if (bpBrands.ToObject<JObject>().ContainsKey(CtdiConstants.JarvisStateCode) && bpBrands.ToObject<JObject>().ContainsKey(CtdiConstants.JarvisBusinessPartnerBrandId)
                                  && bpBrands.ToObject<JObject>().TryGetValue(CtdiConstants.JarvisStateCode, StringComparison.OrdinalIgnoreCase, out JToken? stateCodeValue) && Convert.ToInt32(stateCodeValue.Value<JToken>(), CultureInfo.InvariantCulture) == 0)
                        {
                            this.multipartContent.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Patch, targetEntity, "(" + bpBrands.ToObject<JObject>()?[CtdiConstants.JarvisBusinessPartnerBrandId]?.ToString().ToLower() + ")", Interlocked.Increment(ref this.counter), payload.ToString(), false));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException($" Deactivate NonMatached BusinesPartnerBrands Failed with Error Message - " + ex.Message);
            }
        }

        /// <summary>
        /// DeleteBusinessPartnerBrandsCodes - Delete Class.
        /// </summary>
        /// <param name="targetEntity">TargetEntity.</param>
        /// <param name="targetCodesRelationshipName">targetCodesRelationshipName.</param>
        private void DeleteBusinessPartnerBrandsCodes(string targetEntity, string targetCodesRelationshipName)
        {
            try
            {
                var targetClassesEntity = CtdiConstants.JarvisBusinessPartnerBrandCodeses;
                var bpBrandsCodeList = this.helper.SubgridValueFromTarget(this.retrieveList, targetEntity, targetCodesRelationshipName);

                if (bpBrandsCodeList != null && bpBrandsCodeList.Any())
                {
                    foreach (var bpBrandsCode in bpBrandsCodeList.ToList())
                    {
                        if (bpBrandsCode.ToObject<JObject>().TryGetValue(CtdiConstants.JarvisBusinessPartnerBrandCodeId, StringComparison.OrdinalIgnoreCase, out JToken? bpBrandsCodeId) && bpBrandsCodeId.Value<JToken>()?.ToString() != null)
                        {
                            this.multipartContent.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Delete, targetClassesEntity, "(" + bpBrandsCodeId.Value<JToken>()?.ToString().ToLower() + ")", Interlocked.Increment(ref this.counter), string.Empty, false));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException($" Delete BusinessPartnerBrandsCodes Failed with Error Message - " + ex.Message);
            }
        }

        /// <summary>
        /// BusinessPartnerOpeningHours - Set Open Hours Payload.
        /// </summary>
        /// <param name="payload">Paylaod.</param>
        /// <param name="sourceContentId">SourceContentId.</param>
        /// <param name="mappings">Mapping.</param>
        private void BusinessPartnerOpeningHours(JObject payload, int sourceContentId, JObject? mappings)
        {
            try
            {
                string[] days = { "monday", "tuesday", "wednesday", "thursday", "friday", "saturday", "sunday" };
                payload.TryGetValue("openHours", StringComparison.OrdinalIgnoreCase, out JToken? openHours);
                if (openHours != null && openHours.ToList().Count > 0 && sourceContentId != 0)
                {
                    // Get Lunch Hour
                    foreach (var openHourLunch in openHours.ToList())
                    {
                        openHourLunch.ToObject<JObject>().TryGetValue("code", StringComparison.OrdinalIgnoreCase, out JToken? code);
                        var codeValue = Convert.ToInt16(code?.Value<JToken>(), CultureInfo.InvariantCulture);
                        if (codeValue == 6)
                        {
                            this.lunchHour = openHourLunch;
                        }
                    }

                    List<JToken> openHoursWithoutLunch;
                    openHoursWithoutLunch = openHours.ToList();
                    if (this.lunchHour != null)
                    {
                        openHoursWithoutLunch.Remove(this.lunchHour);
                    }

                    foreach (var openHour in openHoursWithoutLunch)
                    {
                        var targetOpeningHoursEntity = CtdiConstants.JarvisBusinessPartnerOpeningHourses;

                        for (int i = 0; i < days.Length; i++)
                        {
                            JObject openHoursPayload = new JObject();
                            if (openHour.ToObject<JObject>().TryGetValue("code", StringComparison.OrdinalIgnoreCase, out JToken? code) &&
                            openHour.ToObject<JObject>().TryGetValue($"{days[i]}from", StringComparison.OrdinalIgnoreCase, out JToken? fromDay) &&
                            openHour.ToObject<JObject>().TryGetValue($"{days[i]}to", StringComparison.OrdinalIgnoreCase, out JToken? toDay) && openHour.ToObject<JObject>().TryGetValue("description", StringComparison.OrdinalIgnoreCase, out JToken? description))
                            {
                                if (this.lunchHour != null)
                                {
                                    this.lunchHour.ToObject<JObject>().TryGetValue($"{days[i]}from", StringComparison.OrdinalIgnoreCase, out JToken? lunchFrom);
                                    this.lunchHour.ToObject<JObject>().TryGetValue($"{days[i]}to", StringComparison.OrdinalIgnoreCase, out JToken? lunchTo);

                                    //// Set Lunch From and Lunch To
                                    if (lunchFrom == null)
                                    {
                                        openHoursPayload.Add(CtdiConstants.JarvisLunchFrom, string.Empty);
                                    }
                                    else
                                    {
                                        openHoursPayload.Add(CtdiConstants.JarvisLunchFrom, Convert.ToString(lunchFrom.Value<JToken>(), CultureInfo.InvariantCulture));
                                    }

                                    if (lunchTo == null)
                                    {
                                        openHoursPayload.Add(CtdiConstants.JarvisLunchTo, string.Empty);
                                    }
                                    else
                                    {
                                        openHoursPayload.Add(CtdiConstants.JarvisLunchTo, Convert.ToString(lunchTo.Value<JToken>(), CultureInfo.InvariantCulture));
                                    }
                                }

                                // set openhours owner
                                JToken? token = this.SetOwnerTeam(mappings);
                                openHoursPayload.Add(CtdiConstants.OwnerId, token);
                                openHoursPayload.Add(CtdiConstants.JarvisName, Convert.ToString(description.Value<JToken>(), CultureInfo.InvariantCulture));
                                openHoursPayload.Add(CtdiConstants.JarvisOpenHoursCode, Convert.ToInt16(code.Value<JToken>(), CultureInfo.InvariantCulture));
                                openHoursPayload.Add(CtdiConstants.JarvisWeekDay, i + 1);
                                openHoursPayload.Add(CtdiConstants.JarvisFrom, Convert.ToString(fromDay.Value<JToken>(), CultureInfo.InvariantCulture));
                                openHoursPayload.Add(CtdiConstants.JarvisTo, Convert.ToString(toDay.Value<JToken>(), CultureInfo.InvariantCulture));
                                openHoursPayload.Add(CtdiConstants.JarvisSortOrder, i + 1);
                                openHoursPayload.Add(CtdiConstants.JarvisBusinessPartnerOdataBind, string.Format("${0}", sourceContentId));
                                this.multipartContent.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Patch, targetOpeningHoursEntity, "(" + Guid.NewGuid().ToString() + ")", Interlocked.Increment(ref this.counter), openHoursPayload.ToString(), false));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException($" BusinessPartnerOpeningHours Method Failed with Error Message - " + ex.Message);
            }
        }

        /// <summary>
        /// GetConfigMasterDataAsBatch - Retrieve Master Data.
        /// </summary>
        /// <param name="sourceAccountNumber">SourceAccountNumber.</param>?
        /// <returns>Task.</returns>
        /// <exception cref="ArgumentException">Argument Null Exception.</exception>
        private async Task GetConfigMasterDataAsBatch(string sourceAccountNumber)
        {
            try
            {
                List<HttpMessageContent> masterContents = new List<HttpMessageContent>();
                this.logger.LogTrace("Enter into ConfigurationMaster Data");
                //// Used configuration code to retrieve Config record and master data
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.JarvisIntegrationConfiguration, string.Format(CrmQueries.JarvisIntegrationConfigurationQuery, CtdiConstants.JarvisCTDIIntegrationConfiguration.ToString().ToUpper()), 1, string.Empty, false));
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, CtdiConstants.Accounts, string.Format(CrmQueries.AccountUpsertQuery, sourceAccountNumber), 1, string.Empty, false));
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, CtdiConstants.JarvisCountries, CrmQueries.JarvisStatecodeActiveQuery, 1, string.Empty, false));
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, CtdiConstants.JarvisLanguages, CrmQueries.JarvisStatecodeActiveQuery, 1, string.Empty, false));
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, CtdiConstants.Transactioncurrencies, CrmQueries.JarvisStatecodeActiveQuery, 1, string.Empty, false));
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, CtdiConstants.JarvisBrands, CrmQueries.JarvisStatecodeActiveQuery, 1, string.Empty, false));
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, CtdiConstants.JarvisMainCodes, CrmQueries.JarvisStatecodeActiveQuery, 1, string.Empty, false));
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, CtdiConstants.JarvisSubCodes, CrmQueries.JarvisStatecodeActiveQuery, 1, string.Empty, false));
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, CtdiConstants.Teams, CrmQueries.JarvisTeamsQuery, 1, string.Empty, false));
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, CtdiConstants.Characteristics, CrmQueries.JarvisStatecodeActiveQuery, 1, string.Empty, false));

                // framing integraion configuration and master data
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

                // all configuration and master data pushed into retrieve list dictionary
                if (this.retrieveList.Count <= 0)
                {
                    this.logger.LogException(new ArgumentException("Getting IntegrationConfiguration,MasterData and Dealers record Failed"));
                    throw new ArgumentException($"No MasterData, Configuration Record Retrieved");
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"GetConfigMasterDataAsBatch:" + ex.Message);
            }
        }

        /// <summary>
        /// SetOwnerTeam - Set Owner Team For Records.
        /// </summary>
        /// <param name="mappings">Mappings.</param>
        /// <returns>Jtoken.</returns>
        /// <exception cref="ArgumentException">Argument Null Exception.</exception>
        private JToken? SetOwnerTeam(JObject? mappings)
        {
            try
            {
                // retreive team from jarvis for framing owner
                if (this.jarvisTeams != null)
                {
                    string teamJarvis = DynamicsApiHelper.GetStringValueFromJObject(mappings, CtdiConstants.OwnerTeam);
                    var matachingRecordTeams = this.jarvisTeams.ToList();
                    var matchRecord = matachingRecordTeams.Where(item => item[CtdiConstants.Name]?.ToString() == teamJarvis.ToString());
                    JToken? teamIdJarvis = matchRecord.Select(item => item[CtdiConstants.TeamId]).First();
                    JToken token = string.Format("/{0}({1})", CtdiConstants.Teams, teamIdJarvis?.ToString());
                    return token;
                }
                else
                {
                    throw new ArgumentException($" Jarvis Team is not having any value");
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException($" Set Owner Team Failed with Error Message - " + ex.Message);
            }
        }

        /// <summary>
        /// Creation of Bookable Resource and Bookbale Resource Service.
        /// </summary>
        /// <param name="currentPayload">CurrentPayload.</param>
        /// <param name="contentid">ContentId.</param>
        /// <param name="accountNumber">AcocuntNumber.</param>
        /// <param name="mappings">Mappings.</param>
        /// <param name="bPname">BPNname.</param>
        /// <param name="accountid">AccountId.</param>
        /// <param name="accountEntityName">AccountEntityName.</param>
        /// <exception cref="ArgumentException">Argument Null Exception.</exception>
        private void BookableResource(JObject currentPayload, int contentid, string accountNumber, JObject? mappings, string? bPname, string accountid, string accountEntityName, (bool, JToken?) retrieveAccountType)
        {
            JObject payload = new JObject();
            if (currentPayload.TryGetValue("services", StringComparison.OrdinalIgnoreCase, out JToken? curpayload))
            {
                var currentPayloadData = curpayload.DistinctBy(item => item["code"]);
                int bookableResourceContentId = 0;
                bool service247 = false;
                string? bookableresourceid = null, bookableresourcerecordkey = null;
                List<JToken>? bookablecharecteristics = null;

                string? sourceField = DynamicsApiHelper.GetStringValueFromJObject(mappings, Constants.SourceFieldSchema);
                string? targetEntity = DynamicsApiHelper.GetStringValueFromJObject(mappings, Constants.TargetEntityName).ToLower();
                string? targetEntityId = DynamicsApiHelper.GetStringValueFromJObject(mappings, Constants.TargetEntityId).ToLower();
                string? targetLookupEntityName = DynamicsApiHelper.GetStringValueFromJObject(mappings, Constants.TargetLookupEntityName).ToLower();
                string? targetFieldSchema = DynamicsApiHelper.GetStringValueFromJObject(mappings, Constants.TargetLookupFieldSchema);
                string? targetLookupEntity = DynamicsApiHelper.GetStringValueFromJObject(mappings, Constants.TargetLookupEntity);
                string? targetRelationshipName = DynamicsApiHelper.GetStringValueFromJObject(mappings, Constants.TargetRelationshipName);

                JToken? token = this.SetOwnerTeam(mappings);
                //// Creation of New Bookabale Resource while creation of Business partner
                if (this.isCreate)
                {
                    bookableresourcerecordkey = Guid.NewGuid().ToString();

                    // set BookableResource owner
                    payload.Add(CtdiConstants.OwnerId, token);

                    payload.Add("name", bPname?.ToString());
                    payload.Add("resourcetype", 5);
                    payload.Add("timezone", 105);
                    payload.Add(CtdiConstants.JarvisBusinessPartnerId, accountNumber);
                    payload.Add(CtdiConstants.AccountIdOdatabind, string.Format("${0}", contentid));
                    bookableResourceContentId = Interlocked.Increment(ref this.counter);
                    this.multipartContent.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Patch, CtdiConstants.Bookableresources, "(" + bookableresourcerecordkey + ")", bookableResourceContentId, payload.ToString(), false));

                    bookableresourceid = bookableresourcerecordkey;
                }
                else
                {
                    ////Extract Bookable Resource and service if Already Exists
                    JToken? brList = this.helper.SubgridValueFromTarget(this.retrieveList, accountEntityName, CtdiConstants.AccountbookableresourceAccountId);
                    if (brList != null && brList.Any())
                    {
                        bookableresourceid = brList.First().SelectToken(CtdiConstants.Bookableresourceid)?.ToString();
                        bookablecharecteristics = brList.Children().Values(targetRelationshipName).Values().ToList();
                        this.bookableResourceList = brList.Children().Values(targetRelationshipName).Values().ToList<JToken>();
                    }
                    else
                    {
                        bookableresourcerecordkey = Guid.NewGuid().ToString();

                        // set BookableResource owner
                        payload.Add(CtdiConstants.OwnerId, token);

                        payload.Add("name", bPname?.ToString());
                        payload.Add("resourcetype", 5);
                        payload.Add("timezone", 95);
                        payload.Add(CtdiConstants.JarvisBusinessPartnerId, accountNumber);
                        payload.Add(CtdiConstants.AccountIdOdatabind, string.Format("${0}", contentid));
                        bookableResourceContentId = Interlocked.Increment(ref this.counter);
                        this.multipartContent.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Patch, CtdiConstants.Bookableresources, "(" + bookableresourcerecordkey + ")", bookableResourceContentId, payload.ToString(), false));

                        bookableresourceid = bookableresourcerecordkey;
                    }
                }

                //// update Name and Activate the Bookable Resource
                if (bookableresourceid != null)
                {
                    payload = new JObject();
                    if (!this.isCreate)
                    {
                        if (retrieveAccountType.Item1 && retrieveAccountType.Item2 != null)
                        {
                            if (!(retrieveAccountType.Item2.Value<int>() != 334030001))
                            {
                                payload.Add(CtdiConstants.JarvisStateCode, 0);
                            }
                        }
                    }

                    payload.Add("name", bPname?.ToString());
                    bookableResourceContentId = Interlocked.Increment(ref this.counter);
                    this.multipartContent.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Patch, CtdiConstants.Bookableresources, "(" + bookableresourceid + ")", bookableResourceContentId, payload.ToString(), false));
                }

                ////check payload contains Services
                if (currentPayloadData.Any())
                {
                    ////for each service in payload
                    foreach (JToken cPL in currentPayloadData)
                    {
                        cPL.ToObject<JObject>().TryGetValue("code", StringComparison.OrdinalIgnoreCase, out JToken? code);

                        ////Check current service Conatins code
                        if (code != null)
                        {
                            // Reset value for each payload
                            string? serviceId = null, bookableresourceStatus = null, bRkey = null;
                            var retrievebookable = this.helper.LookupValueFromTarget(this.retrieveList, cPL.ToObject<JObject>(), sourceField, targetLookupEntity, targetFieldSchema, targetLookupEntityName);
                            if (code.ToString() == "1400")
                            {
                                service247 = true;
                            }

                            // Service Check IF contain in Masterdata
                            if (!this.isCreate && bookablecharecteristics != null && bookablecharecteristics.Count > 0 && targetEntity != null && (retrievebookable.Item1 && retrievebookable.value != null))
                            {
                                var recordMatch = bookablecharecteristics.Where(item => item[CtdiConstants.CharacteristicValue]?.ToString().ToUpper() == retrievebookable.value.ToString().ToUpper()).ToList();

                                // Bookable  resource service from Payload is matching with Jarvis BPBrands.
                                if (recordMatch != null && recordMatch.Count > 0 && recordMatch.First().ToObject<JObject>().TryGetValue(targetEntityId, StringComparison.OrdinalIgnoreCase, out JToken? value) && value != null && this.bookableResourceList != null)
                                {
                                    recordMatch.First().ToObject<JObject>().TryGetValue(CtdiConstants.JarvisStateCode, StringComparison.OrdinalIgnoreCase, out JToken? status);

                                    bookableresourceStatus = status.ToString();
                                    bRkey = value.ToString();
                                    ////Remove from the matched value from parent list so we can reuse to deactivate....
                                    this.bookableResourceList.RemoveAll(item => item[CtdiConstants.CharacteristicValue]?.ToString().ToUpper() == retrievebookable.value.ToString().ToUpper());
                                }
                                else
                                {
                                    serviceId = Guid.NewGuid().ToString();
                                }
                            }
                            else if (retrievebookable.Item1)
                            {
                                serviceId = Guid.NewGuid().ToString();
                            }

                            ////Creating New Bookable Resource Service
                            payload = new JObject();
                            if (serviceId != null)
                            {
                                if (mappings != null && mappings.TryGetValue(Constants.JarvisFieldMappings, out JToken? fieldMapping) && fieldMapping != null)
                                {
                                    payload = this.helper.SetFieldMapping(cPL?.ToObject<JObject>(), fieldMapping, this.retrieveList);
                                }

                                if (payload.Count > 0 && targetEntity != null)
                                {
                                    if (!this.isCreate && bookableresourceid != null)
                                    {
                                        payload.Add(CtdiConstants.ResourceOdataBind, string.Format("/bookableresources({0})", bookableresourceid));
                                    }
                                    else
                                    {
                                        payload.Add(CtdiConstants.ResourceOdataBind, string.Format("${0}", bookableResourceContentId));
                                    }

                                    ////set BookableResource owner
                                    payload.Add(CtdiConstants.OwnerId, token);
                                    var bookableResourceServiceId = Interlocked.Increment(ref this.counter);
                                    this.multipartContent.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Patch, targetEntity, "(" + serviceId + ")", bookableResourceServiceId, payload.ToString(), false));
                                }
                            }

                            // Activating Bookable Resource Service if inactive
                            else if (bRkey != null && !this.isCreate && bookableresourceStatus == "1" && targetEntity != null)
                            {
                                payload.Add(CtdiConstants.JarvisStateCode, 0);
                                var bookableResourceServiceId = Interlocked.Increment(ref this.counter);
                                this.multipartContent.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Patch, targetEntity, "(" + bRkey + ")", bookableResourceServiceId, payload.ToString(), false));
                            }
                        }
                    }
                }

                // Update Bussiness Partner (Bookable Resource and  service247)
                if (bookableresourceid != null || bookableresourcerecordkey != null)
                {
                    payload = new JObject();
                    if (this.isCreate)
                    {
                        payload.Add(CtdiConstants.JarvisBookableResourceOdataBind, string.Format("${0}", bookableResourceContentId));
                        payload.Add(CtdiConstants.JarvisService247, service247);
                        var bookableRSId = Interlocked.Increment(ref this.counter);
                        this.multipartContent.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Patch, accountEntityName, "(" + accountid + ")", bookableRSId, payload.ToString(), false));
                    }
                    else
                    {
                        payload.Add(CtdiConstants.JarvisBookableResourceOdataBind, string.Format("${0}", bookableResourceContentId));
                        payload.Add(CtdiConstants.JarvisService247, service247);
                        var bookableRSId = Interlocked.Increment(ref this.counter);
                        this.multipartContent.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Patch, accountEntityName, "(" + accountid + ")", bookableRSId, payload.ToString(), false));
                    }
                }
            }
        }

        /// <summary>
        /// BookabaleResourceService Deletion.
        /// </summary>
        /// <param name="mappings">Mappings.</param>
        private void DeleteBookableResourceService(JObject? mappings)
        {
            if (this.bookableResourceList != null)
            {
                string? targetEntity = DynamicsApiHelper.GetStringValueFromJObject(mappings, Constants.TargetEntityName).ToLower();
                string? targetEntityId = DynamicsApiHelper.GetStringValueFromJObject(mappings, Constants.TargetEntityId).ToLower();

                foreach (JToken brCharecteristics in this.bookableResourceList)
                {
                    var bresourceCharecteristics = brCharecteristics.ToObject<JObject>();

                    if (bresourceCharecteristics != null && bresourceCharecteristics.TryGetValue(targetEntityId, StringComparison.OrdinalIgnoreCase, out JToken? brCharId) && brCharId.Value<JToken>()?.ToString() != null)
                    {
                        this.multipartContent.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Delete, targetEntity, "(" + brCharId.Value<JToken>()?.ToString().ToLower() + ")", Interlocked.Increment(ref this.counter), string.Empty, false));
                    }
                }
            }
        }

        private JObject? SetSupportedLanguage(JObject currentPaylaod, JObject framedPayload)
        {
            try
            {
                JObject supLangPayload = new JObject();

                var languageRecords = this.retrieveList.First(item => item.Key.ToUpper() == CtdiConstants.JarvisLanguages.ToUpper()).Value;
                if (languageRecords?.Count > 0)
                {
                    var sourceLangValue = currentPaylaod.SelectToken(CtdiConstants.LanguageCode)?.Value<JToken>();
                    if (sourceLangValue != null && !string.IsNullOrEmpty(sourceLangValue.ToString()))
                    {
                        var matchRecord = languageRecords.Where(item => item[Constants.JarvisIsoLanguageCodeLong]?.ToString().ToUpper().Replace("-", string.Empty) == sourceLangValue.ToString().ToUpper().Replace("-", string.Empty)).ToList();

                        if (matchRecord != null && matchRecord.Count > 0)
                        {
                            var matchedLanguageRecord = matchRecord.First().ToObject<JObject>();
                            if (matchedLanguageRecord != null && matchedLanguageRecord.TryGetValue(CtdiConstants.JarvisVasstandardlanguage, out JToken? supFlag) && supFlag != null && !string.IsNullOrEmpty(supFlag.ToString()))
                            {
                                if ((bool)supFlag)
                                {
                                    JToken token = string.Format("/{0}({1})", Constants.JarvisLanguages, matchedLanguageRecord.SelectToken(Constants.JarvisLanguageid)?.ToString());
                                    supLangPayload.Add(string.Concat(Constants.JarvisLanguage, "@odata.bind"), token);
                                    return supLangPayload;
                                }
                                else
                                {
                                    if (matchedLanguageRecord.TryGetValue(CtdiConstants.JarvisVassupportedlanguage, out JToken? supLang) && supLang != null && !string.IsNullOrEmpty(supLang.ToString()))
                                    {
                                        JToken token = string.Format("/{0}({1})", Constants.JarvisLanguages, supLang.ToString());
                                        supLangPayload.Add(string.Concat(Constants.JarvisLanguage, "@odata.bind"), token);
                                        return supLangPayload;
                                    }
                                }
                            }
                            else
                            {
                                if (matchedLanguageRecord != null && matchedLanguageRecord.TryGetValue(CtdiConstants.JarvisVassupportedlanguage, out JToken? supLang) && supLang != null && !string.IsNullOrEmpty(supLang.ToString()))
                                {
                                    JToken token = string.Format("/{0}({1})", Constants.JarvisLanguages, supLang.ToString());
                                    supLangPayload.Add(string.Concat(Constants.JarvisLanguage, "@odata.bind"), token);
                                    return supLangPayload;
                                }
                            }
                        }

                        return null;
                    }

                    return null;
                }
                else
                {
                    throw new ArgumentException($"No Master Data for Language present");
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException($" Setting supported language for BusinessPartner Failed- " + ex.Message);
            }
        }
    }
}