// <copyright file="CreateCasesIn.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace IntegrationProcess
{
    using System.Collections.Generic;
    using IntegrationProcess.Helper.Constants;
    using MCS.Jarvis.Integration.Base.Dynamics;
    using MCS.Jarvis.Integration.Base.Logging;
    using MCS.Jarvis.IntegrationProcess.Helper;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Create Case Inbound.
    /// </summary>
    public class EtaCalculationAutomation
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
        private readonly List<JarvisPassOutIncidentEta> jarvisPassOutIncidents = new ();
        private int counter = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="EtaCalculationAutomation"/> class.
        /// </summary>
        /// <param name="dynamicsApiClient">Dynamics Client.</param>
        /// <param name="logger">Logger.</param>
        public EtaCalculationAutomation(IDynamicsApiClient dynamicsApiClient, ILoggerService logger)
        {
            this.dynamicsApiClient = dynamicsApiClient;
            this.logger = logger;
            this.helper = new IntegrationHelper(this.dynamicsApiClient, this.logger);
        }

        /// <summary>
        /// IntegrationProcessAsyncMethod.
        /// </summary>
        /// <returns>Task.</returns>
        /// <exception cref="ArgumentException">Argument Exception.</exception>
        public async Task<HttpResponseMessage> IntegrationProcessAsync()
        {
            try
            {
                ////Getting All Master Data and Related Queries in single call aganist unique identifier from the payload "id".
                this.GetConfigMasterDataAsBatch();

                if (this.jarvisPassOutIncidents != null && this.jarvisPassOutIncidents.Count > 0)
                {
                    this.jarvisPassOutIncidents.ForEach(item => item.PassoutEtaHours = (item.JarvisAta - item.Casecreatedon).TotalHours > 0 ? (item.JarvisAta - item.Casecreatedon).TotalHours : 0);
                    var caseWithEtaSum = this.jarvisPassOutIncidents.GroupBy(x => x.JarvisIncidentValue).ToList().Select(x => new JarvisPassOutIncidentEta { JarvisIncidentValue = x.Key, CaseEtaAverage = x.Average(x => x.PassoutEtaHours), CaseJarvisCountry = x.FirstOrDefault().CaseJarvisCountry });
                    var records = caseWithEtaSum.GroupBy(x => x.CaseJarvisCountry).ToList().Select(x => new JarvisPassOutIncidentEta { CaseJarvisCountry = x.Key, CaseEtaAverage = x.Average(x => x.CaseEtaAverage) });
                    if (records.Any())
                    {
                        //// EntityMappingEtaCalculation to Country Payload generation.
                        var (contentId, targetEntity) = this.EntityMappingEtaCalculation(records.ToList());

                        ////Checking MultipartContent contains Data and executing whole payload request..
                        if (this.multipartContent.Count > 0)
                        {
                            var response = await this.dynamicsApiClient.ExecuteBatchRequest(this.multipartContent);
                            return response == null ? new HttpResponseMessage() : response;
                        }

                        this.logger.LogException(new ArgumentException("Error in Updating ETA Calculation"));
                        throw new ArgumentException("Error in Updating ETA Calculation");
                    }
                }

                this.logger.LogException(new ArgumentException("No Records Found."));
                throw new ArgumentException("No Records Found.");
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"IntegrationProcessAsync:" + ex.Message);
            }
        }

        /// <summary>
        /// EntityMappingEtaCalculation.
        /// </summary>
        /// <param name="countryEtaData">countryEtaData.</param>
        /// <returns>Target entity.</returns>
        /// <exception cref="ArgumentException">Argument Exception.</exception>
        private (int, string) EntityMappingEtaCalculation(List<JarvisPassOutIncidentEta> countryEtaData)
        {
            try
            {
                string? targetEntity = Constants.JarvisCountries;

                foreach (var item in countryEtaData)
                {
                    if (item.CaseEtaAverage > 0)
                    {
                        JObject payload = new ();
                        string recordkey;
                        recordkey = item.CaseJarvisCountry;
                        payload.Add(Constants.JarvisAverageetaduration, item.CaseEtaAverage);
                        //// Framing Country Content for Batch
                        if (payload.Count > 0)
                        {
                            this.multipartContent.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Patch, targetEntity, "(" + recordkey + ")", Interlocked.Increment(ref this.counter), payload.ToString(), false));
                        }
                    }
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
        /// <exception cref="ArgumentException">Argument Exception.</exception>
        private void GetConfigMasterDataAsBatch()
        {
            try
            {
                var response = this.dynamicsApiClient.RetrieveResultSetByFetchXml(Constants.JarvisPassouts, CrmQueries.PassOutforLast365Days, string.Empty, 0);
                if (response != null)
                {
                    var passoutList = response.Select(obj => JsonConvert.DeserializeObject<JarvisPassOutIncidentEta>(obj.ToString())).ToList();
                    this.jarvisPassOutIncidents.AddRange(passoutList);
                }
                else
                {
                    this.logger.LogWarning("No Passout in last one year.");
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"GetConfigMasterDataAsBatch:" + ex.Message);
            }
        }
    }

    /// <summary>
    /// JarvisPassOutIncidentEta.
    /// </summary>
    public class JarvisPassOutIncidentEta
    {
        /// <summary>
        /// Gets or sets jarvis_passoutid.
        /// </summary>
        [JsonProperty("jarvis_passoutid")]
        public string JarvisPassoutid { get; set; }

        /// <summary>
        /// Gets or sets jarvis_ata.
        /// </summary>
        [JsonProperty("jarvis_ata")]
        public DateTime JarvisAta { get; set; }

        /// <summary>
        /// Gets or sets _jarvis_incident_value.
        /// </summary>
        [JsonProperty("_jarvis_incident_value")]
        public string JarvisIncidentValue { get; set; }

        /// <summary>
        /// Gets or sets incident createdOn.
        /// </summary>
        [JsonProperty("case.createdon")]
        public DateTime Casecreatedon { get; set; }

        /// <summary>
        /// Gets or sets incident Country.
        /// </summary>
        [JsonProperty("case.jarvis_country")]
        public string CaseJarvisCountry { get; set; }

        /// <summary>
        /// Gets or sets eta Average in a case.
        /// </summary>
        public double CaseEtaAverage { get; set; }

        /// <summary>
        /// Gets or sets eta Average in a Passout.
        /// </summary>
        public double PassoutEtaHours { get; set; }
    }
}