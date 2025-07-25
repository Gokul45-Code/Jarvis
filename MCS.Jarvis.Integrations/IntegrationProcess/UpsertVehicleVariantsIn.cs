// <copyright file="UpsertVehicleVariantsIn.cs" company="Microsoft">
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
    /// UpsertVehicleVariantsIn.
    /// </summary>
    public class UpsertVehicleVariantsIn
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

        /// <summary>
        /// Initializes a new instance of the <see cref="UpsertVehicleVariantsIn"/> class.
        /// UpsertCustomersIn - Constructor.
        /// </summary>
        /// <param name="dynamicsApiClient">dynamicsApiClient.</param>
        /// <param name="logger">logger.</param>
        public UpsertVehicleVariantsIn(IDynamicsApiClient dynamicsApiClient, ILoggerService logger)
        {
            this.dynamicsApiClient = dynamicsApiClient;
            this.logger = logger;
            this.helper = new IntegrationHelper(this.dynamicsApiClient, this.logger);
        }

        /// <summary>
        /// IntegrationProcessAsync - Get Master Data, Calling Field Validation , Executing Inbound Request.
        /// </summary>
        /// <param name="payLoad">payLoad.</param>
        /// <returns>AsyncTask.</returns>
        /// <exception cref="ArgumentException">ArgumentException.</exception>
        public async Task<HttpResponseMessage> IntegrationProcessAsync(JObject payLoad)
        {
            try
            {
                ////Checking Payload contains unique identifier
                if (payLoad != null)
                {
                    string? chassisSeries = payLoad.SelectToken(VdaConstans.ChassisSeries)?.ToString();
                    string? chassisNumber = payLoad.SelectToken(VdaConstans.ChassisNumber)?.ToString();
                    if (!string.IsNullOrEmpty(chassisSeries) && !string.IsNullOrEmpty(chassisNumber))
                    {
                        ////Getting All Master Data and Related Queries in single call aganist unique identifier from the payload "id".
                        await this.GetConfigMasterDataAsBatch(chassisNumber, chassisSeries);
                        var configRecord = this.retrieveList.First(item => item.Key.ToUpper() == Constants.JarvisIntegrationConfiguration.ToUpper()).Value;
                        if (configRecord != null && configRecord.Count > 0
                              && configRecord.First().ToObject<JObject>().TryGetValue(Constants.JarvisIntegrationMapping, StringComparison.OrdinalIgnoreCase, out JToken? intMapping))
                        {
                            // JsonLoadSettings
                            JObject jarvis_congif = JObject.Parse(intMapping.ToString());
                            if (jarvis_congif != null && jarvis_congif.TryGetValue(VdaConstans.Vehicle, StringComparison.OrdinalIgnoreCase, out JToken? vehicleconfigMapping))
                            {
                                this.helper.ValidateSetFieldMapping(payLoad, vehicleconfigMapping, this.retrieveList);

#pragma warning disable S1481 // Unused local variables should be removed
                                var (contentId, targetEntity) = this.VehicleinfoVdaToJarvis(payLoad, vehicleconfigMapping.ToObject<JObject>());

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
        /// <param name="chassisNumber">chassisNumber.</param>
        /// <param name="chassisSeries">chassisSeries.</param>
        /// <returns>AsyncTask.</returns>
        /// <exception cref="ArgumentException">ArgumentException.</exception>
        ///
        private async Task GetConfigMasterDataAsBatch(string chassisNumber, string chassisSeries)
        {
            try
            {
                List<HttpMessageContent> masterContents = new List<HttpMessageContent>();
                this.logger.LogTrace("Enter into ConfigurationMaster Data");
                //// Used configuration code to retrieve Config record and master data
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.JarvisIntegrationConfiguration, string.Format(CrmQueries.JarvisIntegrationConfigurationQuery, "VDAVARIANTS001"), 1, string.Empty, false));
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.JarvisVehicles, string.Format(CrmQueries.VehicleGetInfo, chassisSeries, chassisNumber), 1, string.Empty, false));
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, VdaConstans.JarvisVehiclefuelpowertypes, CrmQueries.VehicleFuelPowerTypes, 1, string.Empty, false));

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
        /// <param name="currentPayload">currentPayload.</param>
        /// <param name="mappings">mappings.</param>
        /// <returns>Int,String.</returns>
        /// <exception cref="ArgumentException">ArgumentException.</exception>
        private (int, string?) VehicleinfoVdaToJarvis(JObject currentPayload, JObject? mappings)
        {
            try
            {
                JObject payload = new ();
                string recordkey;
                string? sourceField = DynamicsApiHelper.GetStringValueFromJObject(mappings, Constants.SourceFieldSchema);
                string? targetEntity = DynamicsApiHelper.GetStringValueFromJObject(mappings, Constants.TargetEntityName).ToLower();
                string? targetLookupEntityName = DynamicsApiHelper.GetStringValueFromJObject(mappings, Constants.TargetLookupEntityName).ToLower();
                string? targetFieldSchema = DynamicsApiHelper.GetStringValueFromJObject(mappings, Constants.TargetLookupFieldSchema);
                string? targetLookupEntity = DynamicsApiHelper.GetStringValueFromJObject(mappings, Constants.TargetLookupEntity);
                string? sourceLookupEntity = DynamicsApiHelper.GetStringValueFromJObject(mappings, Constants.SourceLookupEntity);
                ////Retrieving Vehicle Match from the paylaod to check if present in CRM Vehicles.
                var retrieveVehicle = this.helper.LookupValueFromTarget(this.retrieveList, currentPayload, sourceField, targetLookupEntity, targetFieldSchema, targetLookupEntityName);

                if (currentPayload != null && targetEntity != null && (retrieveVehicle.Item1 && retrieveVehicle.value != null))
                {
                    recordkey = retrieveVehicle.value.ToString();
                }
                else
                {
                    this.logger.LogException(new ArgumentException("Not a valid Vehicle"));
                    throw new ArgumentException($"Not a valid Vehicle");
                }

                //// Framing Dealers to Account Payload
                if (mappings != null && mappings.TryGetValue(Constants.JarvisFieldMappings, out JToken? fieldMapping) && fieldMapping != null && currentPayload != null)
                {
                    payload = this.helper.SetFieldMapping(currentPayload, fieldMapping, this.retrieveList);
                    string? vehicleFuelPowerTypeId = string.Empty;
                    var variants = currentPayload.SelectToken(sourceLookupEntity);
                    var masterFuelType = this.retrieveList.First(item => item.Key == VdaConstans.JarvisVehiclefuelpowertypes).Value?.GroupBy(item => item[VdaConstans.JarvisPriority]);
                    if (variants != null && variants.Any() && masterFuelType != null && masterFuelType.Any())
                    {
                        foreach (var variant in masterFuelType)
                        {
                            var fuelType = variant.Join(
                                variants,
                                a => a.SelectToken(VdaConstans.JarvisFamilyid)?.ToString().ToUpper(),
                                b => b.SelectToken(VdaConstans.VariantFamilyID)?.ToString().ToUpper(),
                                (a, b) => new { a, b })
                            .Where(x => x.a.SelectToken(VdaConstans.JarvisVariantid)?.ToString().ToUpper() == x.b.SelectToken(VdaConstans.VariantID)?.ToString().ToUpper())
                            .Select(x => x.a)
                            .FirstOrDefault();
                            if (fuelType != null)
                            {
                                vehicleFuelPowerTypeId = fuelType.SelectToken("jarvis_vehiclefuelpowertypeid")?.ToString();
                                this.logger.LogTrace("Vehicle Fuel/Power Type will be updated with " + fuelType.SelectToken("jarvis_familyid") + " : " + fuelType.SelectToken("jarvis_variantid"));
                                break;
                            }
                        }
                    }
                    else
                    {
                        // this.logger.LogException(new ArgumentException("Variants not found in Payload/No mater data."));
                        this.logger.LogWarning("Variants not found in Payload/No mater data.");
                    }

                    if (!string.IsNullOrEmpty(vehicleFuelPowerTypeId))
                    {
                        payload.Add(VdaConstans.VehicleFuelTypeBind, string.Format(" /{0}({1})", VdaConstans.JarvisVehiclefuelpowertypes, vehicleFuelPowerTypeId));
                    }
                    else
                    {
                        // this.logger.LogException(new ArgumentException("None of the Variant matched."));
                        payload.Add(VdaConstans.VehicleFuelTypeBind.Replace("@odata.bind", string.Empty), null);
                        this.logger.LogWarning("None of the Variant matched.");
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
    }
}
