// <copyright file="AddAttachmentInbound.cs" company="Microsoft">
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
    /// AddAttachmentInbound Class.
    /// </summary>
    public class AddAttachmentInbound
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
        /// Initializes a new instance of the <see cref="AddAttachmentInbound"/> class.
        /// </summary>
        /// <param name="dynamicsApiClient">Dynamics Client.</param>
        /// <param name="logger">Logger.</param>
        public AddAttachmentInbound(IDynamicsApiClient dynamicsApiClient, ILoggerService logger)
        {
            this.dynamicsApiClient = dynamicsApiClient;
            this.logger = logger;
            this.helper = new IntegrationHelper(this.dynamicsApiClient, this.logger);
        }

        /// <summary>
        /// AddAttachmentInboundIntegrationProcessAsync.
        /// </summary>
        /// <param name="attachmentPayload">Attachment Payload.</param>
        /// <param name="caseNumberArgus">Case Number Argus.</param>
        /// <param name="caseNumberJarvis">Case Number Jarvis.</param>
        /// <returns>Task.</returns>
        /// <exception cref="ArgumentException">Argument Exception.</exception>
        public async Task<HttpResponseMessage> IntegrationProcessAsync(JObject attachmentPayload, string caseNumberArgus, string caseNumberJarvis)
        {
            try
            {
                ////Checking Payload contains unique identifier
                if (attachmentPayload != null && !string.IsNullOrEmpty(caseNumberArgus))
                {
                    attachmentPayload.TryGetValue(AttachmentConstants.ActivityFileName, StringComparison.OrdinalIgnoreCase, out JToken? activityFileName);
                    attachmentPayload.TryGetValue(AttachmentConstants.ActivityFileExt, StringComparison.OrdinalIgnoreCase, out JToken? activityFileExt);
                    attachmentPayload.TryGetValue(AttachmentConstants.AttachmentId, StringComparison.OrdinalIgnoreCase, out JToken? attachmentBody);

                    //// Valdiation on mandatory fields in payload.
                    if (activityFileExt != null && activityFileName != null && attachmentBody != null && attachmentBody.SelectToken("#text") != null)
                    {
                        var fileName = $"{activityFileName.ToString()}.{activityFileExt.ToString()}";
                        string noteText = string.Empty;
                        string? country = string.Empty;

                        ////Retrieve Required Values from the Payload to mapping.
                        attachmentPayload.TryGetValue(SiebelConstants.TdiPartner, StringComparison.OrdinalIgnoreCase, out JToken? bpResponsibleUnit);
                        attachmentPayload.TryGetValue(SiebelConstants.TdiMarket, StringComparison.OrdinalIgnoreCase, out JToken? bpRetailCountry);
                        attachmentPayload.TryGetValue(SiebelConstants.HomeDealerIdBDARGUS, StringComparison.OrdinalIgnoreCase, out JToken? homeDealerId);
                        attachmentPayload.TryGetValue(SiebelConstants.HomeDealerCountryBDARGUS, StringComparison.OrdinalIgnoreCase, out JToken? homeCountry);
                        attachmentPayload.TryGetValue(AttachmentConstants.SourceARGUS, StringComparison.OrdinalIgnoreCase, out JToken? sourceArgus);
                        attachmentPayload.TryGetValue(AttachmentConstants.ActivityComments, StringComparison.OrdinalIgnoreCase, out JToken? activityComments);
                        attachmentPayload.TryGetValue(AttachmentConstants.McARGUS, StringComparison.OrdinalIgnoreCase, out JToken? mcARGUS);

                        //// TDI Partner, TDI Market and HomeDealerIdBDArgus contains Data from the payload.
                        if (bpResponsibleUnit != null && bpRetailCountry != null && homeDealerId != null && !string.IsNullOrEmpty(bpResponsibleUnit.ToString()) && !string.IsNullOrEmpty(bpRetailCountry.ToString()) && !string.IsNullOrEmpty(homeDealerId.ToString()) && (mcARGUS == null || (mcARGUS != null && string.IsNullOrEmpty(mcARGUS.ToString()))))
                        {
                            JObject payload = new ();
                            ////Get Dealer Data based on TDI Partner and TDI Market.
                            await this.GetConfigMasterDataAsBatchForDealer(bpResponsibleUnit.ToString(), bpRetailCountry.ToString(), caseNumberArgus, caseNumberJarvis);
                            var account = this.retrieveList.First(item => item.Key.ToUpper().Contains(Constants.Accounts.ToUpper())).Value;
                            var incident = this.retrieveList.First(item => item.Key.ToUpper().Contains(Constants.Incident.ToUpper())).Value;
                            string? incidentid = string.Empty;
                            if (incident != null && incident.Any())
                            {
                                incidentid = incident.First()?.SelectToken(Constants.IncidentId)?.ToString();
                            }
                            else
                            {
                                this.logger.LogException(new ArgumentException("Not a valid incident : " + caseNumberArgus + " " + caseNumberJarvis + "."));
                                throw new ArgumentException("Not a valid incident : " + caseNumberArgus + " " + caseNumberJarvis + ".");
                            }

                            if (!(account != null && account.Any()))
                            {
                                this.logger.LogException(new ArgumentException("Not a valid Home Dealer : " + bpResponsibleUnit + " " + bpRetailCountry + "."));
                                throw new ArgumentException("Not a valid Home Dealer : " + bpResponsibleUnit + " " + bpRetailCountry + ".");
                            }

                            country = homeCountry != null && !string.IsNullOrEmpty(homeCountry.ToString()) ? homeCountry.ToString() : incident?.First()?.SelectToken(AttachmentConstants.HomeDealerCountryBDARGUS)?.ToString();
                            //// Dealer logic
                            noteText = $"Comment : {activityComments?.ToString()}\n" +
                                       $"Dealer : {bpResponsibleUnit.ToString()}\n" +
                                       $"Name : {account.First().SelectToken(AttachmentConstants.AccountName)}\n" +
                                       $"Zip Code: {account.First().SelectToken(AttachmentConstants.Address1Postalcode)}\n" +
                                       $"City : {account.First().SelectToken(AttachmentConstants.Address1City)}\n" +
                                       $"Country : {country}\n";
                            payload.Add(AttachmentConstants.Filename, fileName);
                            payload.Add(AttachmentConstants.Notetext, noteText);
                            payload.Add(AttachmentConstants.AttachmentIncident, string.Format("/{0}({1})", Constants.Incidents, incidentid));
                            payload.Add(AttachmentConstants.Documentbody, attachmentBody.SelectToken("#text"));

                            if (payload.Count > 0)
                            {
                                this.multipartContent.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Patch, AttachmentConstants.EntityName, "(" + Guid.NewGuid().ToString() + ")", Interlocked.Increment(ref this.counter), payload.ToString(), false));
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

                        //// TDI Partner Contains Data from the payload.
                        else if (bpResponsibleUnit != null && !string.IsNullOrEmpty(bpResponsibleUnit.ToString()) && (bpRetailCountry == null || string.IsNullOrEmpty(bpRetailCountry.ToString())) && (mcARGUS == null || (mcARGUS != null && string.IsNullOrEmpty(mcARGUS.ToString()))))
                        {
                            // Do Customer logic
                            JObject payload = new ();
                            ////Get Dealer Data based on TDI Partner and TDI Market.
                            await this.GetConfigMasterDataAsBatchForCustomer(caseNumberArgus, caseNumberJarvis, bpResponsibleUnit.ToString());
                            var account = this.retrieveList.First(item => item.Key.ToUpper().Contains(Constants.Accounts.ToUpper())).Value;
                            var incident = this.retrieveList.First(item => item.Key.ToUpper().Contains(Constants.Incident.ToUpper())).Value;
                            string? incidentid = string.Empty;
                            if (incident != null && incident.Any())
                            {
                                incidentid = incident.First().SelectToken(Constants.IncidentId)?.ToString();
                            }
                            else
                            {
                                this.logger.LogException(new ArgumentException("Not a valid incident : " + caseNumberArgus + " " + caseNumberJarvis + "."));
                                throw new ArgumentException("Not a valid incident : " + caseNumberArgus + " " + caseNumberJarvis + ".");
                            }

                            if (!(account != null && account.Any()))
                            {
                                this.logger.LogException(new ArgumentException("Not a valid Customer : " + bpResponsibleUnit + "."));
                                throw new ArgumentException("Not a valid Customer with ID : " + bpResponsibleUnit + ".");
                            }

                            country = homeCountry != null && !string.IsNullOrEmpty(homeCountry.ToString()) ? homeCountry.ToString() : incident.First().SelectToken(AttachmentConstants.HomeDealerCountryBDARGUS)?.ToString();
                            //// Customer logic
                            noteText = $"Comment : {activityComments?.ToString()}\n" +
                                       $"Customer : {bpResponsibleUnit.ToString()}\n" +
                                       $"Name : {account.First().SelectToken(AttachmentConstants.AccountName)}\n" +
                                       $"Zip Code: {account.First().SelectToken(AttachmentConstants.Address1Postalcode)}\n" +
                                       $"City : {account.First().SelectToken(AttachmentConstants.Address1City)}\n" +
                                       $"Country : {country}";
                            payload.Add(AttachmentConstants.Filename, fileName);
                            payload.Add(AttachmentConstants.Notetext, noteText);
                            payload.Add(AttachmentConstants.AttachmentIncident, string.Format("/{0}({1})", Constants.Incidents, incidentid));
                            payload.Add(AttachmentConstants.Documentbody, attachmentBody.SelectToken("#text"));

                            if (payload.Count > 0)
                            {
                                this.multipartContent.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Patch, AttachmentConstants.EntityName, "(" + Guid.NewGuid().ToString() + ")", Interlocked.Increment(ref this.counter), payload.ToString(), false));
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

                        //// IF TDI Partne and TDI Market from the payload is empty or MCArgus contains data.
                        else if ((bpResponsibleUnit == null && bpRetailCountry == null) || ((bpResponsibleUnit != null && string.IsNullOrEmpty(bpResponsibleUnit.ToString())) && (bpRetailCountry != null && string.IsNullOrEmpty(bpRetailCountry.ToString()))) || (mcARGUS != null && !string.IsNullOrEmpty(mcARGUS.ToString())))
                        {
                            JObject payload = new ();
                            ////Get Dealer Data based on TDI Partner and TDI Market.
                            await this.GetConfigMasterDataAsBatchForMarketCompany(caseNumberArgus, caseNumberJarvis);
                            var incident = this.retrieveList.First(item => item.Key.ToUpper().Contains(Constants.Incident.ToUpper())).Value;

                            ////Retrieve MC Values from Payload.
                            attachmentPayload.TryGetValue(AttachmentConstants.McCountryARGUS, StringComparison.OrdinalIgnoreCase, out JToken? mcCountryARGUS);
                            attachmentPayload.TryGetValue(AttachmentConstants.McCountryCodeARGUS, StringComparison.OrdinalIgnoreCase, out JToken? mcCountryCodeARGUS);
                            string? incidentid = string.Empty;
                            if (incident != null && incident.Any())
                            {
                                incidentid = incident.First().SelectToken(Constants.IncidentId)?.ToString();
                            }
                            else
                            {
                                this.logger.LogException(new ArgumentException("Not a valid incident : " + caseNumberArgus + " " + caseNumberJarvis + "."));
                                throw new ArgumentException("Not a valid incident : " + caseNumberArgus + " " + caseNumberJarvis + ".");
                            }

                            country = mcCountryARGUS != null && !string.IsNullOrEmpty(mcCountryARGUS.ToString()) ? mcCountryARGUS.ToString() : homeCountry != null && !string.IsNullOrEmpty(homeCountry.ToString()) ? homeCountry.ToString() : incident.First().SelectToken(AttachmentConstants.HomeDealerCountryBDARGUS)?.ToString();

                            //// Market Company logic
                            noteText = $"Comment : {activityComments?.ToString()}\n" +
                                       $"Market Company : {incident.First().SelectToken(AttachmentConstants.HomeDealerResponsibleUnitId)}\n" +
                                       $"Name : {incident.First().SelectToken(AttachmentConstants.HomeDealerNameBDARGUS)}\n" +
                                       $"Zip Code: {incident.First().SelectToken(AttachmentConstants.HomeDealerZipCode)}\n" +
                                       $"City : {incident.First().SelectToken(AttachmentConstants.HomeDealerCity)}\n" +
                                       $"Country : {country}";

                            payload.Add(AttachmentConstants.Filename, fileName);
                            payload.Add(AttachmentConstants.Notetext, noteText);
                            payload.Add(AttachmentConstants.AttachmentIncident, string.Format("/{0}({1})", Constants.Incidents, incidentid));
                            payload.Add(AttachmentConstants.Documentbody, attachmentBody.SelectToken("#text"));

                            if (payload.Count > 0)
                            {
                                this.multipartContent.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Patch, AttachmentConstants.EntityName, "(" + Guid.NewGuid().ToString() + ")", Interlocked.Increment(ref this.counter), payload.ToString(), false));
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
                        else
                        {
                            throw new ArgumentException("TDI Partner and TDI Market Logic is not statisfied to Create Notes with Attachment against the case.");
                        }
                    }
                    else
                    {
                        throw new ArgumentException($"ActivityFileName , ActivityFileExtension and AttachmentId is missing in the Paylaod.");
                    }
                }
                else
                {
                    throw new ArgumentException($"Unique Identifier CaseNumberArgus is Missing in the Payload.");
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"IntegrationProcessAsync:" + ex.Message);
            }
        }

        /// <summary>
        /// GetConfigMasterDataAsBatchForDealer.
        /// </summary>
        /// <param name="bpResponsibleUnit">bpResponsibleUnit.</param>
        /// <param name="bpRetailCountry">bpRetailCountry.</param>
        /// <param name="caseNumberArgus">caseNumberArgus.</param>
        /// <param name="caseNumberJarvis">caseNumberJarvis.</param>
        /// <returns>Task.</returns>
        /// <exception cref="ArgumentException">Argument Exception.</exception>
        private async Task GetConfigMasterDataAsBatchForDealer(string bpResponsibleUnit, string bpRetailCountry, string caseNumberArgus, string caseNumberJarvis)
        {
            try
            {
                List<HttpMessageContent> masterContents = new ();
                this.logger.LogTrace("Enter into ConfigurationMaster Data");
                //// Used configuration code to retrieve Config record
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.Incidents, string.Format(CrmQueries.JarvisAttachmentIncidentQuery, caseNumberArgus, caseNumberJarvis), 1, string.Empty, false));
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.Accounts, string.Format(CrmQueries.JarvisAttachmentAccountDealerQuery, bpResponsibleUnit, bpRetailCountry), 1, string.Empty, false));

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
        /// GetConfigMasterDataAsBatchForCustomer.
        /// </summary>
        /// <param name="caseNumberArgus">caseNumberArgus.</param>
        /// <param name="caseNumberJarvis">caseNumberJarvis.</param>
        /// <param name="businessPartnerId">businessPartnerId.</param>
        /// <returns>Task.</returns>
        /// <exception cref="ArgumentException">Argument Exception.</exception>
        private async Task GetConfigMasterDataAsBatchForCustomer(string caseNumberArgus, string caseNumberJarvis, string businessPartnerId)
        {
            try
            {
                List<HttpMessageContent> masterContents = new ();
                this.logger.LogTrace("Enter into ConfigurationMaster Data");
                //// Used configuration code to retrieve Config record
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.Incidents, string.Format(CrmQueries.JarvisAttachmentIncidentQuery, caseNumberArgus, caseNumberJarvis), 1, string.Empty, false));
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.Accounts, string.Format(CrmQueries.JarvisAttachmentAccountCustomerQuery, businessPartnerId), 1, string.Empty, false));

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
        /// GetConfigMasterDataAsBatchForMarketCompany.
        /// </summary>
        /// <param name="caseNumberArgus">caseNumberArgus.</param>
        /// <param name="caseNumberJarvis">caseNumberJarvis.</param>
        /// <returns>Task.</returns>
        /// <exception cref="ArgumentException">Argument Exception.</exception>
        private async Task GetConfigMasterDataAsBatchForMarketCompany(string caseNumberArgus, string caseNumberJarvis)
        {
            try
            {
                List<HttpMessageContent> masterContents = new ();
                this.logger.LogTrace("Enter into ConfigurationMaster Data");
                //// Used configuration code to retrieve Config record
                masterContents.Add(DynamicsApiClient.CreateHttpMessageContent(HttpMethod.Get, Constants.Incidents, string.Format(CrmQueries.JarvisAttachmentIncidentQuery, caseNumberArgus, caseNumberJarvis), 1, string.Empty, false));
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
