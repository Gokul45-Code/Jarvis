// <copyright file="UpsertCustomersIn.cs" company="Microsoft.">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace IntegrationProcess
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using IntegrationProcess.Helper.Constants;
    using MCS.Jarvis.Integration.Base.Dynamics;
    using MCS.Jarvis.Integration.Base.Logging;
    using MCS.Jarvis.IntegrationProcess.Helper;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// UpsertCustomerIn.
    /// </summary>
    public class UpsertCustomersIn
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

        /// <summary>
        /// Initializes a new instance of the <see cref="UpsertCustomersIn"/> class.
        /// UpsertCustomersIn - Constructor.
        /// </summary>
        /// <param name="dynamicsApiClient">DynamicsApiClient.</param>
        /// <param name="logger">Logger.</param>
        public UpsertCustomersIn(IDynamicsApiClient dynamicsApiClient, ILoggerService logger)
        {
            this.dynamicsApiClient = dynamicsApiClient;
            this.logger = logger;
            this.helper = new IntegrationHelper(this.dynamicsApiClient, this.logger);
        }

        /// <summary>
        /// IntegrationProcessAsync - Get Master Data, Calling Field Validation , Executing Inbound Request.
        /// </summary>
        /// <param name="payLoad">Payload.</param>
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
                            this.logger.LogException(new Exception("Payload Timesamp should be greater than Customer's Timestamp in Jarvis"));
                            throw new ArgumentException("Payload Timestamp should be greater than Customer's Timestamp in Jarvis");
                        }
                    }

                    var configRecord = this.retrieveList.First(item => item.Key.ToUpper() == Constants.JarvisIntegrationConfiguration.ToUpper()).Value;
                    if (configRecord != null && configRecord.Count > 0
                          && configRecord.First().ToObject<JObject>().TryGetValue(Constants.JarvisIntegrationMapping, StringComparison.OrdinalIgnoreCase, out JToken? intMapping))
                    {
                        // JsonLoadSettings
                        JObject dyn = JObject.Parse(intMapping.ToString());

                        // Customers IntegrationMappings Validation
                        if (dyn != null && dyn.TryGetValue(CdbConstans.Customers, StringComparison.OrdinalIgnoreCase, out JToken? validationCustomers))
                        {
                            this.helper.ValidateSetFieldMappingWithOverWritable(payLoad, validationCustomers, this.retrieveList);
                        }

                        ////Retriving Customers IntegrationMappings
                        if (dyn != null && dyn.TryGetValue(CdbConstans.Customers, StringComparison.OrdinalIgnoreCase, out JToken? customers))
                        {
                            //// Customers to BusinessPartner Payload generation.
                            this.CustomerToBusinessPartner(payLoad, customers.ToObject<JObject>(), timestamp);

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

                        this.logger.LogException(new Exception("No Customer Integration Configuration Found/ Integration is not Active, Please activate the configuraiton record."));
                        throw new ArgumentException($"No Customer Integration Configuration Found. Integration is not Active, Please activate the configuraiton record.");
                    }
                    else
                    {
                        this.logger.LogException(new Exception("No Integration Configuration Found. Integration is not Active, Please activate the configuraiton record."));
                        throw new ArgumentException($"No Integration Configuration Found. Integration is not Active, Please activate the configuraiton record.");
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
                throw new ArgumentException($"CDB Integration:" + ex.Message);
            }
        }

        /// <summary>
        /// GetConfigMasterDataAsBatch - Retrieve Master Data.
        /// </summary>
        /// <param name="sourceAccountNumber">SourceAccountNumber.</param>
        /// <returns>Task.</returns>
        /// <exception cref="ArgumentException">Argument Null Exception.</exception>
        ///
        private async Task GetConfigMasterDataAsBatch(string sourceAccountNumber)
        {
            try
            {
                List<HttpMessageContent> masterContents = new List<HttpMessageContent>();
                this.logger.LogTrace("Enter into ConfigurationMaster Data");
                //// Used configuration code to retrieve Config record and master data
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.JarvisIntegrationConfiguration, string.Format(CrmQueries.JarvisIntegrationConfigurationQuery, CdbConstans.JarvisCTDIIntegrationConfiguration.ToString().ToUpper()), 1, string.Empty, false));
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, CtdiConstants.Accounts, string.Format(CrmQueries.AccountQueryCDB, sourceAccountNumber), 1, string.Empty, false));
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, CtdiConstants.JarvisCountries, CrmQueries.GetCountryWithSupportedLanguage, 1, string.Empty, false));
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, CtdiConstants.Teams, CrmQueries.JarvisTeamsQuery, 1, string.Empty, false));
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, CtdiConstants.JarvisLanguages, CrmQueries.GetEngLanguage, 1, string.Empty, false));

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
        /// SetOwnerTeam - Set Owner Team For Records.
        /// </summary>
        /// <param name="mappings">Mappings.</param>
        /// <returns>Jtoken.</returns>
        /// <exception cref="ArgumentException">Argumetn Null Exception.</exception>
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
        /// Upsert Customer Record in Business Partner.
        /// </summary>
        /// <param name="currentPayload">CurrentPayload.</param>
        /// <param name="mappings">Mappings.</param>
        /// <param name="timestamp">TimeStamp.</param>
        /// <exception cref="ArgumentException">Argument Null Exception.</exception>
        private void CustomerToBusinessPartner(JObject currentPayload, JObject? mappings, DateTime timestamp)
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
                string recordkey;
                if (retrieveAccount.Item1 && retrieveAccount.value != null)
                {
                    recordkey = retrieveAccount.value.ToString();
                }
                else
                {
                    this.isCreate = true;
                    recordkey = Guid.NewGuid().ToString();
                }

                //// Framing Customers to Account Payload
                if (mappings != null && mappings.TryGetValue(Constants.JarvisFieldMappings, out JToken? fieldMapping) && fieldMapping != null)
                {
                    payload = this.helper.SetFieldMappingWithOverWritable(currentPayload, fieldMapping, this.retrieveList);
                }
                else
                {
                    this.logger.LogWarning("NoConfiguration field mapping is statisfied.");
                }

                // Framing Customer Content for Batch
                if (payload.Count > 0)
                {
                    JToken? token = this.SetOwnerTeam(mappings);
                    ////Defaulting Values for BusinespartnerBrands and Also adding buisnesspartner Reference for CTDI Integration.
                    payload.Add(CtdiConstants.OwnerId, token);
                    payload.Add(CtdiConstants.DealerJarvisSource, DealerJarvisSource.CDB);
                    payload.Add(CtdiConstants.DealerAccountType, DealerAccountType.Customer);
                    payload.Add(CtdiConstants.DealerCTDITimestamp, timestamp);
                    payload.Add(CtdiConstants.ExternalStatus, 334030000);
                    payload.Add(CtdiConstants.OneCaseStatus, 334030000);

                    if (this.isCreate)
                    {
                        var retrieveCountry = this.helper.LookupValueFromTarget(this.retrieveList, currentPayload, CdbConstans.MainAddressCountryCode, Constants.JarvisCountries, CdbConstans.Iso2CountryCode, CdbConstans.CountryId);
                        if (retrieveCountry.Item1 && retrieveCountry.value != null)
                        {
                            var countryRecords = this.retrieveList.First(item => item.Key.ToUpper().Contains(Constants.JarvisCountries.ToUpper())).Value?.ToList();
                            var matchedCountry = countryRecords?.Where(item => item[CdbConstans.CountryId]?.ToString() == retrieveCountry.value.ToString());
                            var supportedLangs = matchedCountry?.SelectMany(item => item.SelectToken(CdbConstans.SupportedLangForCountry)).Where(x => Convert.ToBoolean(x[CtdiConstants.JarvisVasstandardlanguage])).ToList();
                            if (supportedLangs != null && supportedLangs.Count == 1)
                            {
                                JToken exLangtoken = string.Format("/{0}({1})", Constants.JarvisLanguages, supportedLangs.First().SelectToken(Constants.JarvisLanguageid)?.ToString());
                                payload.Add(CdbConstans.CustomerExternalJarvisLanguage, exLangtoken);
                                JToken? isSupLangToken = supportedLangs.First().SelectToken(CtdiConstants.JarvisVasstandardlanguage);
                                if (isSupLangToken != null && !(bool)isSupLangToken && supportedLangs.First().SelectToken(CtdiConstants.JarvisVassupportedlanguage) != null && !string.IsNullOrEmpty(supportedLangs.First().SelectToken(CtdiConstants.JarvisVassupportedlanguage).ToString()))
                                {
                                    JToken supLangtoken = string.Format("/{0}({1})", Constants.JarvisLanguages, supportedLangs.First().SelectToken(CtdiConstants.JarvisVassupportedlanguage)?.ToString());
                                    payload.Add(string.Concat(Constants.JarvisLanguage, "@odata.bind"), supLangtoken);
                                }
                                else
                                {
                                    payload.Add(string.Concat(Constants.JarvisLanguage, "@odata.bind"), exLangtoken);
                                }
                            }
                            else
                            {
                                var langEngRecord = this.retrieveList.First(item => item.Key.ToUpper().Contains(Constants.JarvisLanguages.ToUpper())).Value?.ToList();
                                if (langEngRecord != null && langEngRecord.Any())
                                {
                                    JToken exLangtoken = string.Format("/{0}({1})", Constants.JarvisLanguages, langEngRecord.First().SelectToken(Constants.JarvisLanguageid)?.ToString());
                                    payload.Add(CdbConstans.CustomerExternalJarvisLanguage, exLangtoken);
                                    JToken? isSupLangToken = langEngRecord.First().SelectToken(CtdiConstants.JarvisVasstandardlanguage);
                                    if (isSupLangToken != null && !(bool)isSupLangToken && langEngRecord.First().SelectToken(CtdiConstants.JarvisVassupportedlanguage) != null && !string.IsNullOrEmpty(langEngRecord.First().SelectToken(CtdiConstants.JarvisVassupportedlanguage).ToString()))
                                    {
                                        JToken supLangtoken = string.Format("/{0}({1})", Constants.JarvisLanguages, langEngRecord.First().SelectToken(CtdiConstants.JarvisVassupportedlanguage)?.ToString());
                                        payload.Add(string.Concat(Constants.JarvisLanguage, "@odata.bind"), supLangtoken);
                                    }
                                    else
                                    {
                                        payload.Add(string.Concat(Constants.JarvisLanguage, "@odata.bind"), exLangtoken);
                                    }
                                }
                            }
                        }
                    }

                    this.multipartContent.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Patch, targetEntity, "(" + recordkey + ")", Interlocked.Increment(ref this.counter), payload.ToString(), false));
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"CustomerToBusinessPartner:" + ex.Message);
            }
        }
    }
}
