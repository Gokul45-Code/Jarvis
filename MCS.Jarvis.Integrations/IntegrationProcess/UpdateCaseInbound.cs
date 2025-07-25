// <copyright file="UpdateCaseInbound.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace IntegrationProcess
{
    using System.Text;
    using IntegrationProcess.Helper.Constants;
    using MCS.Jarvis.Integration.Base.Dynamics;
    using MCS.Jarvis.Integration.Base.Logging;
    using MCS.Jarvis.IntegrationProcess.Helper;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Update Case Inbound.
    /// </summary>
    public class UpdateCaseInbound
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
        /// Initializes a new instance of the <see cref="UpdateCaseInbound"/> class.
        /// </summary>
        /// <param name="dynamicsApiClient">Dynamics Client.</param>
        /// <param name="logger">Logger.</param>
        public UpdateCaseInbound(IDynamicsApiClient dynamicsApiClient, ILoggerService logger)
        {
            this.dynamicsApiClient = dynamicsApiClient;
            this.logger = logger;
            this.helper = new IntegrationHelper(this.dynamicsApiClient, this.logger);
        }

        /// <summary>
        /// IntegrationProcessAsyncMethod.
        /// </summary>
        /// <param name="payLoad">Paylaod.</param>
        /// <param name="eventTimestamp">Event Time stamp.</param>
        /// <returns>Task.</returns>
        /// <exception cref="ArgumentException">Argument Exception.</exception>
        public async Task<HttpResponseMessage> IntegrationProcessAsync(JObject payLoad, DateTime eventTimestamp)
        {
            try
            {
                ////Checking Payload contains unique identifier
                if (payLoad != null && payLoad.TryGetValue(SiebelConstants.CaseNumberArgus, StringComparison.OrdinalIgnoreCase, out JToken? caseNumberArgus))
                {
                    string accountFilterQuery = this.AccountFilterQuery(payLoad);
                    payLoad.TryGetValue(SiebelConstants.CaseNumberJarvis, StringComparison.OrdinalIgnoreCase, out JToken? caseNumberJarvis);
                    payLoad.TryGetValue(SiebelConstants.VINSerialBDARGUS, StringComparison.OrdinalIgnoreCase, out JToken? vehicleVINSerial);
                    payLoad.TryGetValue(SiebelConstants.TdiPartner, StringComparison.OrdinalIgnoreCase, out JToken? bpResponsibleUnit);
                    payLoad.TryGetValue(SiebelConstants.TdiMarket, StringComparison.OrdinalIgnoreCase, out JToken? bpRetailCountry);
                    if (bpResponsibleUnit != null && bpRetailCountry != null)
                    {
                        ////Getting All Master Data and Related Queries in single call aganist unique identifier from the payload "id".
                        await this.GetConfigMasterDataAsBatch(accountFilterQuery, caseNumberArgus.ToString(), caseNumberJarvis?.ToString(), vehicleVINSerial?.ToString());
                        var incident = this.retrieveList.First(item => item.Key.ToUpper() == Constants.Incidents.ToUpper()).Value?.FirstOrDefault();
                        var configRecord = this.retrieveList.First(item => item.Key.ToUpper() == Constants.JarvisIntegrationConfiguration.ToUpper()).Value;
                        if (incident != null)
                        {
                            var lastCaseUpdateTimeStamp = incident.SelectToken(SiebelConstants.JarvisCaseupdatetimestamp);
                            if (lastCaseUpdateTimeStamp != null && !string.IsNullOrEmpty(lastCaseUpdateTimeStamp.ToString()))
                            {
                                DateTime caseUpdateTimeStamp = DateTime.Parse(lastCaseUpdateTimeStamp.ToString());
                                int result = DateTime.Compare(eventTimestamp, caseUpdateTimeStamp);
                                if (result <= 0)
                                {
                                    this.logger.LogException(new Exception("Payload Timesamp should be greater than Case's Timestamp in Jarvis"));
                                    throw new ArgumentException("Payload Timestamp should be greater than Case's Timestamp in Jarvis");
                                }
                            }

                            if (configRecord != null && configRecord.Count > 0
                                  && configRecord.First().ToObject<JObject>().TryGetValue(Constants.JarvisIntegrationMapping, StringComparison.OrdinalIgnoreCase, out JToken? intMapping))
                            {
                                JObject jarvis_congif = JObject.Parse(intMapping.ToString());

                                ////Retriving case IntegrationMappings

                                if (jarvis_congif != null && jarvis_congif.TryGetValue(CtdiConstants.Incident, StringComparison.OrdinalIgnoreCase, out JToken? caseconfigMapping))
                                {
                                    this.helper.ValidateSetFieldMapping(payLoad, caseconfigMapping, this.retrieveList);
                                    //// ServiceRequestBdArgus to Incident Payload generation.
#pragma warning disable S1481 // Unused local variables should be removed
                                    var (contentId, targetEntity) = this.ServiceRequestBdArgusToCase(payLoad, caseconfigMapping.ToObject<JObject>(), eventTimestamp);

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
                            this.logger.LogException(new ArgumentException("Not a valid Case"));
                            throw new ArgumentException("Not a valid Case");
                        }
                    }
                    else
                    {
                        this.logger.LogException(new ArgumentException($"Payload does not contain unique indentifier - {SiebelConstants.TdiPartner} or {SiebelConstants.TdiMarket}"));
                        throw new ArgumentException($"Payload does not contain unique indentifier - {SiebelConstants.TdiPartner} or {SiebelConstants.TdiMarket}");
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
        /// <param name="timestamp">time stamp.</param>
        /// <returns>Target entity.</returns>
        /// <exception cref="ArgumentException">Argument Exception.</exception>
        private (int, string) ServiceRequestBdArgusToCase(JObject currentPayload, JObject? mappings, DateTime timestamp)
        {
            try
            {
                JObject payload = new ();

                //// Planing to use for Update Case logic.
                string? targetEntity = DynamicsApiHelper.GetStringValueFromJObject(mappings, Constants.TargetEntityName).ToLower();

                ////RetriveAccountValueFromTarget. Check Account is present or not and assign Guid of Account.

                var retrieveIncident = this.retrieveList.First(item => item.Key.ToUpper() == Constants.Incidents.ToUpper()).Value?.FirstOrDefault();
                string? recordkey = string.Empty;
                string? mercuriusstatus = string.Empty;
                string? ownerId = string.Empty;

                if (retrieveIncident != null && retrieveIncident.HasValues)
                {
                    recordkey = retrieveIncident.SelectToken(Constants.IncidentId)?.ToString();
                    mercuriusstatus = retrieveIncident.SelectToken(SiebelConstants.JarvisMercuriusstatus)?.ToString();
                    ownerId = retrieveIncident.SelectToken(SiebelConstants.OwnerId)?.ToString();
                }

                //// Framing Dealers to Account Payload
                if (mappings != null && mappings.TryGetValue(Constants.JarvisFieldMappings, out JToken? fieldMapping) && fieldMapping != null)
                {
                    payload = this.helper.SetFieldMapping(currentPayload, fieldMapping, this.retrieveList);
                    payload.Add(SiebelConstants.JarvisCaseupdatetimestamp, timestamp);
                    if (payload.GetValue(SiebelConstants.JarvisMercuriusstatus)?.ToString() == "850")
                    {
                        payload.Add(SiebelConstants.Statecode, 0);
                        payload.Add(SiebelConstants.JarvisReopenedreason, "Reopened from Mercurius");
                        payload.Add(Constants.Statuscode, 85);
                    }

                    if (string.IsNullOrEmpty(payload.GetValue(SiebelConstants.JarvisMercuriusstatus)?.ToString()) || (payload.GetValue(SiebelConstants.JarvisMercuriusstatus)?.ToString() != "850" && Convert.ToInt32(payload.GetValue(SiebelConstants.JarvisMercuriusstatus)) < Convert.ToInt32(mercuriusstatus)))
                    {
                        payload.Remove(SiebelConstants.JarvisMercuriusstatus);
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

                    JToken? user = this.SetOwnerUser(mappings, currentPayload.SelectToken(SiebelConstants.RequestWorkerLoginId), ownerId);
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

        private JToken? SetOwnerUser(JObject mappings, JToken? requestWorkerLoginId, string? ownerId)
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
                    if (requestWorkerLoginId != null && !string.IsNullOrEmpty(ownerId) && requestWorkerLoginId.ToString().ToUpper() == SiebelConstants.Mercurius && ownerId.ToUpper().Contains(userJarvisUnAssigned.ToUpper()))
                    {
                        var matchRecord = matachingRecordTeams.Where(item => item[Constants.Fullname].ToString().ToUpper().Contains(userJarvisMercurius.ToString().ToUpper()));
                        userIdJarvis = matchRecord.Select(item => item[Constants.Systemuserid]).First();
                    }
                    else if ((requestWorkerLoginId == null || string.IsNullOrEmpty(requestWorkerLoginId.ToString())) && !string.IsNullOrEmpty(ownerId) && ownerId.ToUpper().Contains(userJarvisMercurius.ToUpper()))
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
        /// AccountFilterQuery.
        /// </summary>
        /// <param name="currentPayload">Current Payload.</param>
        /// <returns>Filter Query.</returns>
        /// <exception cref="ArgumentException">Argument Exception.</exception>
        private string AccountFilterQuery(JObject currentPayload)
        {
            try
            {
                StringBuilder filterQuery = new StringBuilder();
                if (currentPayload != null)
                {
                    currentPayload.TryGetValue(SiebelConstants.CustomerNumARGUS, StringComparison.OrdinalIgnoreCase, out JToken? customerNumber);
                    currentPayload.TryGetValue(SiebelConstants.TdiPartner, StringComparison.OrdinalIgnoreCase, out JToken? bpResponsibleUnit);
                    currentPayload.TryGetValue(SiebelConstants.TdiMarket, StringComparison.OrdinalIgnoreCase, out JToken? bpRetailCountry);
                    filterQuery.Append(string.Format("accountnumber eq '{0}' or (jarvis_responsableunitid eq '{1}' and jarvis_retailcountryid eq '{2}') ", customerNumber, bpResponsibleUnit, bpRetailCountry));
                }

                return filterQuery.ToString();
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"AccountFilterQuery:" + ex.Message);
            }
        }

        /// <summary>
        /// Get ConfigurationMasterDataasBatch.
        /// </summary>
        /// <param name="accountFilterQuery">Account Filter Query.</param>
        /// <param name="caseNumberArgus">Case Number Argus.</param>
        /// <param name="caseNumberJarvis">Case Number Jarvis.</param>
        /// <param name="vehicleVINSerial">VIN number.</param>
        /// <returns>Task.</returns>
        /// <exception cref="ArgumentException">Argument Exception.</exception>
        private async Task GetConfigMasterDataAsBatch(string accountFilterQuery, string caseNumberArgus, string? caseNumberJarvis, string? vehicleVINSerial)
        {
            try
            {
                List<HttpMessageContent> masterContents = new ();
                this.logger.LogTrace("Enter into ConfigurationMaster Data");
                //// Used configuration code to retrieve Config record
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.JarvisIntegrationConfiguration, string.Format(CrmQueries.JarvisIntegrationConfigurationQuery, "UpdateCase001".ToString().ToUpper()), 1, string.Empty, false));
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.Accounts, string.Format(CrmQueries.AccountSiebelInboundQuery, accountFilterQuery), 1, string.Empty, false));
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.Incidents, string.Format(CrmQueries.IncidentGetInboundQuery, caseNumberArgus, caseNumberJarvis), 1, string.Empty, false, true));
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.JarvisMileages, CrmQueries.JarvisStatecodeActiveQuery, 1, string.Empty, false));
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.JarvisLanguages, CrmQueries.JarvisStatecodeActiveQuery, 1, string.Empty, false));
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.JarvisBrands, CrmQueries.JarvisStatecodeActiveQuery, 1, string.Empty, false));
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, CtdiConstants.JarvisCountries, CrmQueries.JarvisStatecodeActiveQuery, 1, string.Empty, false));
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, CtdiConstants.Transactioncurrencies, CrmQueries.JarvisStatecodeActiveQuery, 1, string.Empty, false));
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.JarvisVehicles, string.Format(CrmQueries.VehicleSiebelCreateUpdateInboundQuery, vehicleVINSerial), 1, string.Empty, false));
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
    }
}
