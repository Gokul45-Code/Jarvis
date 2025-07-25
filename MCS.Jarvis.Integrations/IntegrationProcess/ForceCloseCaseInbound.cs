// <copyright file="ForceCloseCaseInbound.cs" company="Microsoft">
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
    /// Force Closure Case Inbound.
    /// </summary>
    public class ForceCloseCaseInbound
    {
        /// <summary>
        /// DynamicsApiClient - for creating dynamics api client.
        /// </summary>
        private readonly IDynamicsApiClient dynamicsApiClient;

        /// <summary>
        /// logger object.
        /// </summary>
        private readonly ILoggerService logger;
        private readonly List<HttpMessageContent> multipartContent = new ();
        private readonly Dictionary<string, JArray?> retrieveList = new ();

        /// <summary>
        /// helper object.
        /// </summary>
        private readonly IntegrationHelper helper;
        private int counter = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="ForceCloseCaseInbound"/> class.
        /// </summary>
        /// <param name="dynamicsApiClient">Dynamics Client.</param>
        /// <param name="logger">logger.</param>
        public ForceCloseCaseInbound(IDynamicsApiClient dynamicsApiClient, ILoggerService logger)
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
                if (payLoad != null && payLoad.TryGetValue(SiebelConstants.CaseNumberArgus, StringComparison.OrdinalIgnoreCase, out JToken? caseNumberArgus))
                {
                    payLoad.TryGetValue(SiebelConstants.CaseNumberJarvis, StringComparison.OrdinalIgnoreCase, out JToken? caseNumberJarvis);
                    payLoad.TryGetValue(SiebelConstants.ForceCloseReason, StringComparison.OrdinalIgnoreCase, out JToken? forceCloseReason);
                    payLoad.TryGetValue(SiebelConstants.ForcedCloseType, StringComparison.OrdinalIgnoreCase, out JToken? forcedCloseType);
                    if (forceCloseReason != null && forcedCloseType != null)
                    {
                        ////Getting All Master Data and Related Queries in single call aganist unique identifier from the payload "id".
                        await this.GetConfigMasterDataAsBatch(caseNumberArgus.ToString(), caseNumberJarvis?.ToString(), forceCloseReason.ToString(), forcedCloseType.ToString());

                        var incident = this.retrieveList.First(item => item.Key.ToUpper().Contains(Constants.Incident.ToUpper())).Value;
                        string? incidentid = string.Empty;
                        if (incident != null && incident.Any())
                        {
                            incidentid = incident.First().SelectToken(Constants.IncidentId)?.ToString();
                            string? serviceLine = incident.First().SelectToken(SiebelConstants.JarvisCaseservicelineValue)?.ToString();
                            string? forceCloseType = incident.First().SelectToken(SiebelConstants.ForceClosureType)?.ToString();
                            var (contentId, targetEntity) = this.ServiceRequestBdArgusToCase(incidentid, serviceLine, forceCloseType);
                            if (this.multipartContent.Count > 0 && !string.IsNullOrEmpty(targetEntity))
                            {
                                var response = await this.dynamicsApiClient.ExecuteBatchRequest(this.multipartContent);
                                return response == null ? new HttpResponseMessage() : response;
                            }

                            this.logger.LogException(new ArgumentException("Error in Generating Payload"));
                            throw new ArgumentException("Error in Generating Payload");
                        }
                        else
                        {
                            this.logger.LogException(new ArgumentException("Not a valid incident : " + caseNumberArgus + " " + caseNumberJarvis + "."));
                            throw new ArgumentException("Not a valid incident : " + caseNumberArgus + " " + caseNumberJarvis + ".");
                        }
                    }
                    else
                    {
                        this.logger.LogException(new ArgumentException($"Payload does not contain unique indentifier - {SiebelConstants.ForceCloseReason} or {SiebelConstants.ForcedCloseType}"));
                        throw new ArgumentException($"Payload does not contain unique indentifier - {SiebelConstants.ForceCloseReason} or {SiebelConstants.ForcedCloseType}");
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
        /// <param name="incidentId">Incident Id.</param>
        /// <param name="serviceLine">Service Line.</param>
        /// <param name="forceCloseType">Fore closure type.</param>
        /// <returns>Target entity.</returns>
        /// <exception cref="ArgumentException">Argument Exception.</exception>
        private (int, string) ServiceRequestBdArgusToCase(string? incidentId, string? serviceLine, string? forceCloseType)
        {
            try
            {
                JObject payload = new ();

                //// Planing to use for Update Case logic.
                string? targetEntity = SiebelConstants.Incidentresolutions;

                ////RetriveAccountValueFromTarget. Check Account is present or not and assign Guid of Account.
                var retrieveCaseResolution = this.retrieveList.First(item => item.Key.ToUpper().Contains(SiebelConstants.JarvisCaseforceclosedescriptions.ToUpper()));
                string recordkey = Guid.NewGuid().ToString();
                if (retrieveCaseResolution.Key != null)
                {
                    var matachingrecord = retrieveCaseResolution.Value?.ToList();
                    if (matachingrecord != null && matachingrecord.Count > 0)
                    {
                        var matchRecord = matachingrecord.FirstOrDefault(item => item.SelectToken(SiebelConstants.JarvisCaseStatus)?.ToString().ToUpper() == forceCloseType?.ToUpper() && item.SelectToken(SiebelConstants.JarvisServicelineValue)?.ToString().ToLower() == serviceLine?.ToLower());
                        //// Framing Dealers to Account Payload
                        if (matchRecord != null)
                        {
                            payload.Add(SiebelConstants.CaseResolutionIncident, string.Format("/{0}({1})", Constants.Incidents, incidentId));
                            payload.Add(SiebelConstants.JarvisClosureDescription, string.Format("/{0}({1})", SiebelConstants.JarvisCaseforceclosedescriptions, matchRecord.SelectToken(SiebelConstants.JarvisCaseforceclosedescriptionid)));
                            payload.Add(SiebelConstants.JarvisClosureType, string.Format("/{0}({1})", SiebelConstants.JarvisCaseforceclosetypes, matchRecord.SelectToken(SiebelConstants.JarvisForceclosetypeValue)));
                            payload.Add(SiebelConstants.ResolutionTypeCode, 1000);
                            payload.Add(SiebelConstants.JarvisCaseStatusResolution, string.Format("/{0}({1})", SiebelConstants.JarvisCasestatuses, matchRecord.SelectToken(SiebelConstants.JarvisCasestatusValue)));
                            payload.Add(SiebelConstants.JarvisServiceLineResolution, string.Format("/{0}({1})", SiebelConstants.JarvisServicelines, serviceLine));
                        }
                        else
                        {
                            this.logger.LogWarning("Not a valid Case Force Close Descriptions");
                            throw new ArgumentException($"ServiceRequestBdArgusToCase: Not a valid Case Force Close Descriptions");
                        }
                    }
                    else
                    {
                        this.logger.LogWarning("Not a valid Case Force Close Descriptions");
                        throw new ArgumentException($"ServiceRequestBdArgusToCase: Not a valid Case Force Close Descriptions");
                    }
                }
                else
                {
                    this.logger.LogWarning("Not a valid Case Force Close Descriptions");
                    throw new ArgumentException($"ServiceRequestBdArgusToCase: Not a valid Case Force Close Descriptions");
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
        /// <param name="caseNumberArgus">Case Number Argus.</param>
        /// <param name="caseNumberJarvis">Case Number Jarvis.</param>
        /// <param name="forceCloseReason">Force closure reason.</param>
        /// <param name="forcedCloseType">Force closure type.</param>
        /// <returns>Task.</returns>
        /// <exception cref="ArgumentException">Argument Exception.</exception>
        private async Task GetConfigMasterDataAsBatch(string caseNumberArgus, string? caseNumberJarvis, string forceCloseReason, string forcedCloseType)
        {
            try
            {
                List<HttpMessageContent> masterContents = new ();
                this.logger.LogTrace("Enter into ConfigurationMaster Data");
                //// Used configuration code to retrieve Config record
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, SiebelConstants.JarvisCaseforceclosedescriptions, string.Format(CrmQueries.CaseResolutionGetQuery, forceCloseReason.Replace("'", "''"), forcedCloseType), 1, string.Empty, false));
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.Incidents, string.Format(CrmQueries.CaseForceCloseInQuery, caseNumberArgus, caseNumberJarvis), 1, string.Empty, false, true));
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
