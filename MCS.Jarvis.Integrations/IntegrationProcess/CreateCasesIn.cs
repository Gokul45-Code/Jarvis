// <copyright file="CreateCasesIn.cs" company="Microsoft">
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
    /// Create Case Inbound.
    /// </summary>
    public class CreateCasesIn
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
        /// Initializes a new instance of the <see cref="CreateCasesIn"/> class.
        /// </summary>
        /// <param name="dynamicsApiClient">Dynamics Client.</param>
        /// <param name="logger">Logger.</param>
        public CreateCasesIn(IDynamicsApiClient dynamicsApiClient, ILoggerService logger)
        {
            this.dynamicsApiClient = dynamicsApiClient;
            this.logger = logger;
            this.helper = new IntegrationHelper(this.dynamicsApiClient, this.logger);
        }

        /// <summary>
        /// IntegrationProcessAsyncMethod.
        /// </summary>
        /// <param name="payLoad">Payload.</param>
        /// <returns>Task.</returns>
        /// <exception cref="ArgumentException">Argument Exception.</exception>
        public async Task<HttpResponseMessage> IntegrationProcessAsync(JObject payLoad)
        {
            try
            {
                ////Checking Payload contains unique identifier
                if (payLoad != null && payLoad.TryGetValue(SiebelConstants.CaseNumberArgus, StringComparison.OrdinalIgnoreCase, out JToken? caseNumber))
                {
                    if (payLoad.TryGetValue(SiebelConstants.CustomerNumARGUS, StringComparison.OrdinalIgnoreCase, out JToken? customerNumber) &&
                        payLoad.TryGetValue(SiebelConstants.TdiPartner, StringComparison.OrdinalIgnoreCase, out JToken? bpResponsibleUnit) &&
                        payLoad.TryGetValue(SiebelConstants.TdiMarket, StringComparison.OrdinalIgnoreCase, out JToken? bpRetailCountry) &&
                        payLoad.TryGetValue(SiebelConstants.VINSerialBDARGUS, StringComparison.OrdinalIgnoreCase, out JToken? vehicleVINSerial))
                    {
                        ////Getting All Master Data and Related Queries in single call aganist unique identifier from the payload "id".
                        await this.GetConfigMasterDataAsBatch(bpResponsibleUnit.ToString(), bpRetailCountry.ToString(), customerNumber.ToString(), vehicleVINSerial.ToString());

                        var configRecord = this.retrieveList.First(item => item.Key.ToUpper() == Constants.JarvisIntegrationConfiguration.ToUpper()).Value;
                        if (configRecord != null && configRecord.Count > 0
                              && configRecord.First().ToObject<JObject>().TryGetValue(Constants.JarvisIntegrationMapping, StringComparison.OrdinalIgnoreCase, out JToken? intMapping))
                        {
                            JObject jarvis_congif = JObject.Parse(intMapping.ToString());

                            ////Retriving case IntegrationMappings
                            if (jarvis_congif != null && jarvis_congif.TryGetValue("incident", StringComparison.OrdinalIgnoreCase, out JToken? caseconfigMapping))
                            {
                                this.helper.ValidateSetFieldMapping(payLoad, caseconfigMapping, this.retrieveList);
                                //// ServiceRequestBdArgus to Incident Payload generation.
#pragma warning disable S1481 // Unused local variables should be removed
                                var (contentId, targetEntity) = this.ServiceRequestBdArgusToCase(payLoad, caseconfigMapping.ToObject<JObject>());
#pragma warning restore S1481 // Unused local variables should be removed

                                //// Calling BusinessPartnerBrands Concept
                                if (jarvis_congif.TryGetValue(Constants.Brands, StringComparison.OrdinalIgnoreCase, out JToken? brandsConfigMappings) && brandsConfigMappings != null)
                                {
                                    var bpBrands = this.helper.BrandsToBusinessPartnerBrands(brandsConfigMappings.ToObject<JObject>(), payLoad, this.retrieveList, bpResponsibleUnit.ToString(), bpRetailCountry.ToString());
                                    if (bpBrands.Item1 != null && bpBrands.Item1 != string.Empty && bpBrands.Item2 != null && bpBrands.Item2.Count > 0)
                                    {
                                        this.multipartContent.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Patch, Constants.JarvisBusinesspartnerbrandses, "(" + bpBrands.Item1 + ")", Interlocked.Increment(ref this.counter), bpBrands.Item2.ToString(), false));
                                    }
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
                        this.logger.LogException(new ArgumentException($"Payload does not contain unique indentifier - {SiebelConstants.CustomerNumARGUS}/{SiebelConstants.TdiPartner} or {SiebelConstants.TdiMarket}/{SiebelConstants.VINSerialBDARGUS}"));
                        throw new ArgumentException($"Payload does not contain unique indentifier - {SiebelConstants.CustomerNumARGUS}/{SiebelConstants.TdiPartner} or {SiebelConstants.TdiMarket}/{SiebelConstants.VINSerialBDARGUS}");
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
        /// Retrieve lookup Business Partner Brands from Target.
        /// </summary>
        /// <param name="retrieveList">Retrieve List of Master data.</param>
        /// <param name="sourcePayload">Source Payload.</param>
        /// <param name="sourceField">Source field.</param>
        /// <param name="targetLookupEntity">Target Lookup entity.</param>
        /// <param name="targetFieldSchema">Target field schema.</param>
        /// <param name="targetEntityName">target entity name.</param>
        /// <returns>JToken of Id.</returns>
        /// <exception cref="ArgumentException">Argument Exception.</exception>
        public (bool, JToken? value) LookupValueBpBrandFromTarget(Dictionary<string, JArray?> retrieveList, JObject sourcePayload, string sourceField, string targetLookupEntity, string targetFieldSchema, string targetEntityName)
        {
            try
            {
                var matchMasterRecord = retrieveList.First(item => item.Key.ToUpper().Contains(targetLookupEntity.ToUpper()));

                if (!string.IsNullOrEmpty(sourceField) && !string.IsNullOrEmpty(targetFieldSchema) && !string.IsNullOrEmpty(targetEntityName) && matchMasterRecord.Key != null)
                {
                    var matachingrecord = matchMasterRecord.Value?.ToList();

                    var sourceValue = sourcePayload.SelectToken(sourceField);
                    if (!string.IsNullOrEmpty(sourceValue?.ToString()) && matachingrecord != null && matachingrecord.Count > 0)
                    {
                        var matchRecord = matachingrecord.Where(item => item.SelectToken(targetFieldSchema)?.ToString().ToUpper() == sourceValue.ToString().ToUpper()).ToList();

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
                throw new ArgumentException($" Retrieving Lookup Value from Jarvis entity {targetLookupEntity} Failed with Error Message - " + ex.Message);
            }
        }

        /// <summary>
        /// ServiceRequestBdArgusToCaseMethod.
        /// </summary>
        /// <param name="currentPayload">Current Payload.</param>
        /// <param name="mappings">Mapping.</param>
        /// <returns>Target entity.</returns>
        /// <exception cref="ArgumentException">Argument Exception.</exception>
        private (int, string) ServiceRequestBdArgusToCase(JObject currentPayload, JObject? mappings)
        {
            try
            {
                JObject payload = new ();

                string? targetEntity = DynamicsApiHelper.GetStringValueFromJObject(mappings, Constants.TargetEntityName).ToLower();

                string recordkey;
                recordkey = Guid.NewGuid().ToString();

                //// Framing Dealers to Account Payload
                if (mappings != null && mappings.TryGetValue(Constants.JarvisFieldMappings, out JToken? fieldMapping) && fieldMapping != null)
                {
                    payload = this.helper.SetFieldMapping(currentPayload, fieldMapping, this.retrieveList);
                    if (payload.SelectToken(SiebelConstants.Caseorigincode) == null || string.IsNullOrEmpty(payload.SelectToken(SiebelConstants.Caseorigincode)?.ToString()))
                    {
                        payload[SiebelConstants.Caseorigincode] = 334030001;
                    }

                    if (payload.SelectToken("['" + SiebelConstants.JarvisVehicleBind + "']") == null || string.IsNullOrEmpty(payload.SelectToken("['" + SiebelConstants.JarvisVehicleBind + "']")?.ToString()))
                    {
                        var vehicleBrand = currentPayload.SelectToken(SiebelConstants.BrandArgus);
                        if (vehicleBrand != null)
                        {
                            var vehicle = this.retrieveList.First(item => item.Key.ToUpper() == Constants.JarvisVehicles.ToUpper()).Value;
                            var matchedDummyVehicle = vehicle?.AsEnumerable().Where(x => x.SelectToken(SiebelConstants.JarvisIsDummyVehicle) != null && (bool)x.SelectToken(SiebelConstants.JarvisIsDummyVehicle) && x.SelectToken(SiebelConstants.VehicleBrandArgusCode)?.ToString().ToUpper() == vehicleBrand?.ToString().ToUpper());
                            if (matchedDummyVehicle != null && matchedDummyVehicle.Any())
                            {
                                payload.Add(SiebelConstants.JarvisVehicleBind, string.Format("/{0}({1})", Constants.JarvisVehicles, matchedDummyVehicle.First()[SiebelConstants.JarvisVehicleId]));
                            }
                        }
                    }

                    JToken? user = this.SetOwnerUser(mappings, currentPayload.SelectToken(SiebelConstants.RequestWorkerLoginId));
                    if (user != null && !string.IsNullOrEmpty(user.ToString()))
                    {
                        payload.Add(CtdiConstants.OwnerId, user);
                    }
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
        /// <param name="bpResponsibleUnit">Responsible unit.</param>
        /// <param name="bpRetailCountry">Retail Country.</param>
        /// <param name="customerNumber">Customer Number.</param>
        /// <param name="vehicleVINSerial">VIN number.</param>
        /// <returns>Task.</returns>
        /// <exception cref="ArgumentException">Argument Exception.</exception>
        private async Task GetConfigMasterDataAsBatch(string bpResponsibleUnit, string bpRetailCountry, string customerNumber, string vehicleVINSerial)
        {
            try
            {
                List<HttpMessageContent> masterContents = new ();
                this.logger.LogTrace("Enter into ConfigurationMaster Data");
                //// Used configuration code to retrieve Config record
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.JarvisIntegrationConfiguration, string.Format(CrmQueries.JarvisIntegrationConfigurationQuery, "JARVIS001".ToString().ToUpper()), 1, string.Empty, false));
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.Accounts, string.Format(CrmQueries.AccountSiebelCreateInboundQuery, customerNumber, bpResponsibleUnit, bpRetailCountry), 1, string.Empty, false));
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.JarvisVehicles, string.Format(CrmQueries.VehicleSiebelCreateUpdateInboundQuery, vehicleVINSerial), 1, string.Empty, false));
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.JarvisMileages, CrmQueries.JarvisStatecodeActiveQuery, 1, string.Empty, false));
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.JarvisLanguages, CrmQueries.JarvisStatecodeActiveQuery, 1, string.Empty, false));
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.JarvisBrands, CrmQueries.JarvisStatecodeActiveQuery, 1, string.Empty, false));
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, CtdiConstants.JarvisCountries, CrmQueries.JarvisStatecodeActiveQuery, 1, string.Empty, false));
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, CtdiConstants.Teams, CrmQueries.JarvisTeamsQuery, 1, string.Empty, false));
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.Users, CrmQueries.JarvisUsersQuery, 1, string.Empty, false));
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
        /// SetOwnerUser.
        /// </summary>
        /// <param name="mappings">Mapping details.</param>
        /// <param name="requestWorkerLoginId">RequestWorkerLoginId from payload.</param>
        /// <returns>User mapping.</returns>
        private JToken? SetOwnerUser(JObject mappings, JToken? requestWorkerLoginId)
        {
            try
            {
                var jarvisUsers = this.retrieveList.First(item => item.Key.ToUpper() == Constants.Users.ToUpper()).Value;

                // retreive user from jarvis for framing owner
                if (jarvisUsers != null)
                {
                    JToken? userIdJarvis = null;
                    string userJarvisMercurius = DynamicsApiHelper.GetStringValueFromJObject(mappings, SiebelConstants.OwnerAssignedUser);
                    string userJarvisUnAssigned = DynamicsApiHelper.GetStringValueFromJObject(mappings, SiebelConstants.OwnerUnassignedUser);
                    var matachingRecordTeams = jarvisUsers.ToList();
                    if (requestWorkerLoginId != null && requestWorkerLoginId.ToString().ToUpper() == SiebelConstants.Mercurius)
                    {
                        var matchRecord = matachingRecordTeams.Where(item => item[Constants.Fullname].ToString().ToUpper().Contains(userJarvisMercurius.ToString().ToUpper()));
                        userIdJarvis = matchRecord.Select(item => item[Constants.Systemuserid]).First();
                    }
                    else if (requestWorkerLoginId == null || string.IsNullOrEmpty(requestWorkerLoginId.ToString()))
                    {
                        var matchRecord = matachingRecordTeams.Where(item => item[Constants.Fullname].ToString().ToUpper().Contains(userJarvisUnAssigned.ToString().ToUpper()));
                        userIdJarvis = matchRecord.Select(item => item[Constants.Systemuserid]).First();
                    }

                    if (userIdJarvis != null && !string.IsNullOrEmpty(userIdJarvis.ToString()))
                    {
                        JToken token = string.Format("/{0}({1})", Constants.Users, userIdJarvis.ToString());
                        return token;
                    }

                    return userIdJarvis;
                }
                else
                {
                    throw new ArgumentException($" Jarvis User is not having any value");
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException($" Set Owner User Failed with Error Message - " + ex.Message);
            }
        }

        /// <summary>
        /// Return Key Payload for BusinessPartnerBrands.
        /// </summary>
        /// <param name="brandsMappings">Mapping.</param>
        /// <param name="currentPayload">Current Payload.</param>
        /// <param name="retrieveList">Retrieve List.</param>
        /// <param name="bpResponsibleUnit">Responsible unit.</param>
        /// <param name="bpRetailCountry">Retail Country.</param>
        /// <returns>Jobject of brand.</returns>
        /// <exception cref="ArgumentException">Argument Exception.</exception>
        private (string, JObject) BrandsToBusinessPartnerBrands(JObject brandsMappings, JObject currentPayload, Dictionary<string, JArray?> retrieveList, string bpResponsibleUnit, string bpRetailCountry)
        {
            try
            {
                JObject payload = new JObject();
                string bpBrandRecordKey = string.Empty;
                string? sourceEntity = DynamicsApiHelper.GetStringValueFromJObject(brandsMappings, Constants.SourceEntityName);
                string? sourceField = DynamicsApiHelper.GetStringValueFromJObject(brandsMappings, Constants.SourceFieldSchema);
                string? targetLookupEntity = DynamicsApiHelper.GetStringValueFromJObject(brandsMappings, Constants.TargetLookupEntity);
                string? targetLookupEntityName = DynamicsApiHelper.GetStringValueFromJObject(brandsMappings, Constants.TargetEntityId);
                string? targetFieldSchema = DynamicsApiHelper.GetStringValueFromJObject(brandsMappings, Constants.TargetLookupFieldSchema);
                string? targetRelationship = DynamicsApiHelper.GetStringValueFromJObject(brandsMappings, Constants.TargetRelationshipName);
                bool? isRequired = DynamicsApiHelper.GetBoolValueFromJObject(brandsMappings, Constants.IsRequired);

                if (!string.IsNullOrEmpty(sourceEntity) && !string.IsNullOrEmpty(sourceField) && !string.IsNullOrEmpty(targetLookupEntity) && !string.IsNullOrEmpty(targetLookupEntityName) && !string.IsNullOrEmpty(targetFieldSchema) && !string.IsNullOrEmpty(targetRelationship))
                {
                    var sourceValue = currentPayload.SelectToken(sourceField);
                    if (isRequired.HasValue && isRequired.Value && sourceValue != null && sourceValue.ToString() != string.Empty)
                    {
                        var retrieveAccounts = retrieveList.First(item => item.Key.ToUpper().Contains(sourceEntity.ToUpper()));
                        brandsMappings.TryGetValue(Constants.JarvisFieldMappings, out JToken? fieldMapping);
                        if (retrieveAccounts.Key != null && fieldMapping != null)
                        {
                            var accountPayload = retrieveAccounts.Value?.First(item => item[Constants.JarvisResponsableunitid] != null && item[Constants.JarvisResponsableunitid]?.ToString().ToUpper() == bpResponsibleUnit.ToString().ToUpper() && item[Constants.JarvisRetailcountryid] != null && item[Constants.JarvisRetailcountryid]?.ToString().ToUpper() == bpRetailCountry.ToString().ToUpper());
                            if (accountPayload != null && accountPayload.HasValues)
                            {
                                string? accountId = accountPayload.SelectToken(Constants.AccountID)?.ToString();
                                string? accountNumber = accountPayload.SelectToken(Constants.Accountnumber)?.ToString();
                                JToken? bpBrandsList = this.helper.SubgridValueFromTarget(retrieveList, sourceEntity, targetRelationship);

                                if (bpBrandsList != null && bpBrandsList.Any())
                                {
                                    retrieveList.Add(Constants.JarvisBusinesspartnerbrandses, bpBrandsList.Value<JArray>());

                                    var bpBrandRecord = this.LookupValueBpBrandFromTarget(retrieveList, currentPayload, sourceField, targetLookupEntity, targetFieldSchema, targetLookupEntityName);
                                    if (bpBrandRecord.Item1 && bpBrandRecord.value != null)
                                    {
                                        bpBrandRecordKey = bpBrandRecord.value.ToString();
                                        payload.Add(CtdiConstants.JarvisStateCode, 0);

                                        return (bpBrandRecordKey, payload);
                                    }
                                }

                                bpBrandRecordKey = Guid.NewGuid().ToString();
                                payload = this.helper.SetFieldMapping(currentPayload, fieldMapping, this.retrieveList);
                                payload.Add(CtdiConstants.JarvisBusinessPartnerId, accountNumber);
                                JToken accountToken = string.Format("/{0}({1})", sourceEntity, accountId);
                                payload.Add(CtdiConstants.JarvisBusinessPartnerOdataBind, accountToken);
                            }
                        }
                    }
                    else
                    {
                        this.logger.LogException(new Exception($"Value for Required Field from configuration is missing in the payload:-{sourceField}"));
                        throw new ArgumentException($"Value for Required Field from configuration is missing in the payload:-{sourceField}");
                    }
                }

                return (bpBrandRecordKey, payload);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"BrandsToBusinessPartnerBrands:" + ex.Message);
            }
        }
    }
}