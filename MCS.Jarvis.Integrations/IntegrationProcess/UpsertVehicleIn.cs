// <copyright file="UpsertVehicleIn.cs" company="Microsoft">
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
    /// Upsert Vehicle Inbound.
    /// </summary>
    public class UpsertVehicleIn
    { /// <summary>
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
        private bool isCreate = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpsertVehicleIn"/> class.
        /// </summary>
        /// <param name="dynamicsApiClient">Dynamics Client.</param>
        /// <param name="logger">Logger.</param>
        public UpsertVehicleIn(IDynamicsApiClient dynamicsApiClient, ILoggerService logger)
        {
            this.dynamicsApiClient = dynamicsApiClient;
            this.logger = logger;
            this.helper = new IntegrationHelper(this.dynamicsApiClient, this.logger);
        }

        /// <summary>
        /// IntegrationProcessAsync - Get Master Data, Calling Field Validation , Executing Inbound Request.
        /// </summary>
        /// <param name="payLoad">Payload.</param>
        /// <returns>Task.</returns>
        /// <exception cref="ArgumentException">Argument Exception.</exception>
        public async Task<HttpResponseMessage> IntegrationProcessAsync(JObject? payLoad)
        {
            try
            {
                ////Checking Payload contains unique identifier
                if (payLoad != null)
                {
                    if (payLoad.SelectToken(VdaConstans.RepairingDealerRCID) != null)
                    {
                        JToken? existingValue = payLoad.SelectToken(VdaConstans.RepairingDealerRCID);
                        existingValue?.Replace(existingValue.ToString().TrimStart('0'));
                    }

                    if (payLoad.SelectToken(VdaConstans.RepairingDealerID) != null)
                    {
                        JToken? existingValue = payLoad.SelectToken(VdaConstans.RepairingDealerID);
                        existingValue?.Replace(existingValue.ToString().TrimStart());
                    }

                    string? chassisSeries = payLoad.SelectToken(VdaConstans.ChassisSeries)?.ToString();
                    string? chassisNumber = payLoad.SelectToken(VdaConstans.ChassisNumber)?.ToString();
                    string? owningEndCustomerID = payLoad.SelectToken(VdaConstans.OwningEndCustomerID)?.ToString();
                    string? usingEndCustomerID = payLoad.SelectToken(VdaConstans.UsingEndCustomerID)?.ToString();
                    string? repairingDealerID = payLoad.SelectToken(VdaConstans.RepairingDealerID)?.ToString();
                    string? repairingDealerRCID = payLoad.SelectToken(VdaConstans.RepairingDealerRCID)?.ToString();
                    if (!string.IsNullOrEmpty(chassisSeries) && !string.IsNullOrEmpty(chassisNumber))
                    {
                        ////Getting All Master Data and Related Queries in single call aganist unique identifier from the payload "id".
                        await this.GetConfigMasterDataAsBatch(chassisNumber, chassisSeries, owningEndCustomerID, usingEndCustomerID, repairingDealerID, repairingDealerRCID);
                        var configRecord = this.retrieveList.First(item => item.Key.ToUpper() == Constants.JarvisIntegrationConfiguration.ToUpper()).Value;
                        if (configRecord != null && configRecord.Count > 0
                              && configRecord.First().ToObject<JObject>().TryGetValue(Constants.JarvisIntegrationMapping, StringComparison.OrdinalIgnoreCase, out JToken? intMapping))
                        {
                            // JsonLoadSettings
                            JObject jarvis_congif = JObject.Parse(intMapping.ToString());
                            if (jarvis_congif != null && jarvis_congif.TryGetValue(VdaConstans.Vehicle, StringComparison.OrdinalIgnoreCase, out JToken? vehicleconfigMapping))
                            {
                                this.helper.ValidateSetFieldMappingWithOverWritable(payLoad, vehicleconfigMapping, this.retrieveList);

#pragma warning disable S1481 // Unused local variables should be removed
                                var (contentId, targetEntity) = this.VehicleinfoVdaToJarvis(payLoad, jarvis_congif);

                                // Checking MultipartContent contains Data and executing whole payload request..
                                if (this.multipartContent.Count > 0)
                                {
                                    var response = await this.dynamicsApiClient.ExecuteBatchRequest(this.multipartContent);
                                    return response == null ? new HttpResponseMessage() : response;
                                }

                                this.logger.LogException(new ArgumentException("Error in Generating Payload"));
                                throw new ArgumentException("Error in Generating Payload");
                            }

                            this.logger.LogException(new ArgumentException("No Integration Configuration Found/ Integration is not Active, Pleae activate the configuraiton record."));
                            throw new ArgumentException("No Integration Configuration Found/ Integration is not Active, Pleae activate the configuraiton record.");
                        }
                        else
                        {
                            this.logger.LogException(new Exception("No Integration Configuration Found. Integration is not Active, Please activate the configuraiton record."));
                            throw new ArgumentException($"No Integration Configuration Found. Integration is not Active, Please activate the configuraiton record.");
                        }
                    }
                    else
                    {
                        this.logger.LogException(new Exception("Not a valid record " + chassisNumber + " " + chassisSeries));
                        throw new ArgumentException($"Not a valid record " + chassisNumber + " " + chassisSeries);
                    }
                }
                else
                {
                    this.logger.LogException(new Exception("Payload does not contain unique indentifier"));
                    throw new ArgumentException($"Payload does not contain unique indentifier");
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"VDA Integration:" + ex.Message);
            }
        }

        /// <summary>
        /// GetConfigMasterDataAsBatch - Retrieve Master Data.
        /// </summary>
        /// <param name="chassisNumber">Chassis Number.</param>
        /// <param name="chassisSeries">Chassis Series.</param>
        /// <param name="owningEndCustomerID">Owning End Customer ID.</param>
        /// <param name="usingEndCustomerID">Using End Customer ID.</param>
        /// <param name="repairingDealerID">Repairing Dealer ID.</param>
        /// <param name="repairingDealerRCID">Repairing Dealer RC ID.</param>
        /// <returns>Task.</returns>
        /// <exception cref="ArgumentException">Argument Exception.</exception>
        ///
        private async Task GetConfigMasterDataAsBatch(string chassisNumber, string chassisSeries, string? owningEndCustomerID, string? usingEndCustomerID, string? repairingDealerID, string? repairingDealerRCID)
        {
            try
            {
                List<HttpMessageContent> masterContents = new List<HttpMessageContent>();
                this.logger.LogTrace("Enter into ConfigurationMaster Data");
                //// Used configuration code to retrieve Config record and master data
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.JarvisIntegrationConfiguration, string.Format(CrmQueries.JarvisIntegrationConfigurationQuery, "VDA001"), 1, string.Empty, false));
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.Accounts, string.Format(CrmQueries.VehicleGetAccounts, owningEndCustomerID, usingEndCustomerID, repairingDealerRCID, repairingDealerID), 1, string.Empty, false));
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.JarvisCountries, CrmQueries.JarvisStatecodeActiveQuery, 1, string.Empty, false));
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.JarvisVehicles, string.Format(CrmQueries.VehicleGetInfo, chassisSeries, chassisNumber), 1, string.Empty, false));
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.JarvisBrands, CrmQueries.JarvisStatecodeActiveQuery, 1, string.Empty, false));
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, CtdiConstants.Teams, CrmQueries.JarvisTeamsQuery, 1, string.Empty, false));

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
                    this.logger.LogException(new ArgumentException("Getting IntegrationConfiguration,MasterData and Customers record Failed"));
                    throw new ArgumentException($"No MasterData, Configuration Record Retrieved");
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"GetConfigMasterDataAsBatch:" + ex.Message);
            }
        }

        /// <summary>
        /// VehicleinfoVdaToJarvis.
        /// </summary>
        /// <param name="currentPayload">Current Payload.</param>
        /// <param name="jarvis_congif">Jarvis Configuration.</param>
        /// <returns>Target entity.</returns>
        /// <exception cref="ArgumentException">Argument Exception.</exception>
        private (int, string?) VehicleinfoVdaToJarvis(JObject currentPayload, JObject jarvis_congif)
        {
            try
            {
                JObject payload = new ();
                string recordkey;
                jarvis_congif.TryGetValue(VdaConstans.Vehicle, StringComparison.OrdinalIgnoreCase, out JToken? vehicleconfigMapping);
                jarvis_congif.TryGetValue(VdaConstans.VehicleAccount, StringComparison.OrdinalIgnoreCase, out JToken? vehicleAccountconfigMapping);
                JObject? mappings = vehicleconfigMapping?.ToObject<JObject>();
                string? sourceField = DynamicsApiHelper.GetStringValueFromJObject(mappings, Constants.SourceFieldSchema);
                string? targetEntity = DynamicsApiHelper.GetStringValueFromJObject(mappings, Constants.TargetEntityName).ToLower();
                string? targetLookupEntityName = DynamicsApiHelper.GetStringValueFromJObject(mappings, Constants.TargetLookupEntityName).ToLower();
                string? targetFieldSchema = DynamicsApiHelper.GetStringValueFromJObject(mappings, Constants.TargetLookupFieldSchema);
                string? targetLookupEntity = DynamicsApiHelper.GetStringValueFromJObject(mappings, Constants.TargetLookupEntity);
                string? defaultCustomer = DynamicsApiHelper.GetStringValueFromJObject(mappings, Constants.DefaultCustomer);
                string? defaultDealer = DynamicsApiHelper.GetStringValueFromJObject(mappings, Constants.DefaultDealer);

                ////Retrieving Vehicle Match from the paylaod to check if present in CRM Vehicles.
                var retrieveVehicle = this.helper.LookupValueFromTarget(this.retrieveList, currentPayload, sourceField, targetLookupEntity, targetFieldSchema, targetLookupEntityName);
                if (!this.isCreate && currentPayload != null && targetEntity != null && (retrieveVehicle.Item1 && retrieveVehicle.value != null))
                {
                    recordkey = retrieveVehicle.value.ToString();
                }
                else
                {
                    this.isCreate = true;
                    recordkey = Guid.NewGuid().ToString();
                }

                //// Framing Dealers to Account Payload
                if (mappings != null && mappings.TryGetValue(Constants.JarvisFieldMappings, out JToken? fieldMapping) && fieldMapping != null && currentPayload != null)
                {
                    payload = this.helper.SetFieldMappingWithOverWritable(currentPayload, fieldMapping, this.retrieveList);
                    JToken? team = this.SetOwnerTeam(mappings);
                    payload.Add(CtdiConstants.OwnerId, team);
                    payload.Add(Constants.JarvisSource, 334030004);
                    if (vehicleAccountconfigMapping.ToObject<JObject>().TryGetValue(Constants.JarvisFieldMappings, out JToken? fieldMappingDealer) && fieldMappingDealer != null)
                    {
                        payload.Merge(this.helper.SetFieldMapping(currentPayload, fieldMappingDealer, this.retrieveList));
                    }

                    if (!payload.ContainsKey(VdaConstans.JarvisUsingCustomer))
                    {
                        payload.Add(VdaConstans.JarvisUsingCustomer, string.Format("/{0}({1})", Constants.Accounts, defaultCustomer.ToString()));
                    }

                    if (!payload.ContainsKey(VdaConstans.JarvisOwningCustomer))
                    {
                        payload.Add(VdaConstans.JarvisOwningCustomer, string.Format("/{0}({1})", Constants.Accounts, defaultCustomer.ToString()));
                    }

                    if (!payload.ContainsKey(VdaConstans.JarvisHomeDealer))
                    {
                        payload.Add(VdaConstans.JarvisHomeDealer, string.Format("/{0}({1})", Constants.Accounts, defaultDealer.ToString()));
                    }
                }
                else
                {
                    this.logger.LogWarning("NoConfiguration field mapping is statisfied.");
                }

                // Framing Vehicle Content for Batch
                if (payload.Count > 0 && targetEntity != null)
                {
                    this.multipartContent.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Patch, targetEntity, "(" + recordkey + ")", Interlocked.Increment(ref this.counter), payload.ToString(), false));
                }

                return (this.counter, targetEntity);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"VehicleinfoVdaToJarvis:" + ex.Message);
            }
        }

        private JToken? SetOwnerTeam(JObject mappings)
        {
            try
            {
                var jarvisTeams = this.retrieveList.First(item => item.Key.ToUpper() == Constants.Teams.ToUpper()).Value;

                // retreive team from jarvis for framing owner
                if (jarvisTeams != null)
                {
                    string teamJarvis = DynamicsApiHelper.GetStringValueFromJObject(mappings, CtdiConstants.OwnerTeam);
                    var matachingRecordTeams = jarvisTeams.ToList();
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
    }
}
