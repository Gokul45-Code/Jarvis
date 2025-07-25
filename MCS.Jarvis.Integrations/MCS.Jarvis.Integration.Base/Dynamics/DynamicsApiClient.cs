namespace MCS.Jarvis.Integration.Base.Dynamics
{
    using System.Globalization;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Xml;
    using MCS.Jarvis.Integration.Base.Logging;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Constant = MCS.Jarvis.Integration.Base.EntityWrapper.Constant;

    public class DynamicsApiClient : IDynamicsApiClient
    {
        /// <summary>
        /// Http Client object.
        /// </summary>
        private readonly HttpClient httpClient;

        /// <summary>
        /// Dynamics Authentication pool.
        /// </summary>
        private readonly DynamicsAuthConnectionPool authConnectionPool;

        /// <summary>
        /// logger object.
        /// </summary>
        private ILoggerService logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicsApiClient" /> class.
        /// </summary>
        /// <param name="config">application settings configuration.</param>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public DynamicsApiClient(IConfiguration config, IHttpClientFactory httpClientFactory)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config), "Null Configuration Object");
            }

            DynamicsConnectionService.CrmAPIResource = config.GetSection("DynamicsConfig:CrmApiUrl").Value;
            this.authConnectionPool = new DynamicsAuthConnectionPool(config, httpClientFactory);
            bool preferConnectionAffinity = Convert.ToBoolean(config.GetSection("DynamicsConfig:PreferConnectionAffinity").Value);

            // instantiate Http Client
            //var httpHandler = new HttpClientHandler() { DefaultProxyCredentials = CredentialCache.DefaultCredentials, UseCookies = preferConnectionAffinity };
            this.httpClient = httpClientFactory.CreateClient();
            //this.httpClient = new HttpClient(httpHandler);
            this.httpClient.BaseAddress = new Uri(DynamicsConnectionService.CrmAPIResource);
            this.httpClient.DefaultRequestHeaders.Add("OData-MaxVersion", "4.0");
            this.httpClient.DefaultRequestHeaders.Add("OData-Version", "4.0");
            this.httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            this.httpClient.Timeout = new TimeSpan(0, 0, Constant.TimeSpanSeconds);
        }

        /// <summary>
        /// Gets Authorization token or Renew if token Is Expired.
        /// </summary>
        private string AuthToken
        {
            get
            {
                return this.authConnectionPool.GetAuthenticationToken();
            }
        }

        /// <summary>
        /// Set Logging Reference.
        /// </summary>
        /// <param name="logger"></param>
        public void SetLoggingReference(ILoggerService logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// create HTTP Message content.
        /// </summary>
        /// <param name="httpMethod">HTTP Method.</param>
        /// <param name="entitySetName">entity set name.</param>
        /// <param name="requestResource">request resource.</param>
        /// <param name="contentId">content ID.</param>
        /// <param name="content">content message.</param>
        /// <param name="bypassPluginValidation">By Pass Plugin Validation.</param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "<Pending>")]
        public static HttpMessageContent CreateHttpMessageContent(HttpMethod httpMethod, string entitySetName, string requestResource, int contentId = 0, string? content = null, bool bypassPluginValidation = false, bool preferFormattedValue = false)
        {
            HttpRequestMessage requestMessage = new HttpRequestMessage(httpMethod, DynamicsConnectionService.CrmAPIResource + entitySetName + requestResource);
            HttpMessageContent messageContent = new HttpMessageContent(requestMessage);
            messageContent.Headers.Remove("Content-Type");
            messageContent.Headers.Add("Content-Type", "application/http");
            messageContent.Headers.Add("Content-Transfer-Encoding", "binary");
            messageContent.Headers.Add("OData-MaxVersion", "4.0");
            messageContent.Headers.Add("OData-Version", "4.0");

            // only GET request requires Accept header
            if (httpMethod == HttpMethod.Get)
            {
                requestMessage.Headers.Add("Accept", "application/json");
                if (preferFormattedValue)
                {
                    requestMessage.Headers.Add("Prefer", "odata.include-annotations=*");
                }
            }
            else
            {
                // prefer representation

                // request other than GET may have content, which is normally JSON
                if (!string.IsNullOrEmpty(content))
                {
                    StringContent stringContent = new StringContent(content);
                    stringContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json;type=entry");
                    requestMessage.Content = stringContent;
                }

                messageContent.Headers.Add("Content-ID", contentId.ToString(CultureInfo.InvariantCulture));

                // bypass plugin validation
                if (bypassPluginValidation)
                {
                    requestMessage.Headers.Add("MSCRM.BypassCustomPluginExecution", "true");
                }
            }

            return messageContent;
        }

        /// <summary>
        /// Retrieve Result Set By Fetch Xml.
        /// </summary>
        /// <param name="entitySetName">entity Set Name.</param>
        /// <param name="fetchXml">fetch Xml.</param>
        /// <param name="userId">user Id.</param>
        /// <param name="fetchCount">fetch Count.</param>
        /// <returns>Result set.</returns>
        public ICollection<JObject> RetrieveResultSetByFetchXml(string entitySetName, string fetchXml, string userId, int fetchCount = 5000)
        {
            ICollection<JObject> result = new List<JObject>();
            int pageNumber = 0;
            string? pagingcookie = string.Empty;
            do
            {
                pageNumber++;

                string xml = FormatFetchXml(fetchXml, pagingcookie, pageNumber, fetchCount, userId);

                JObject collection = this.RetrieveFetchXmlResultFirstSet(entitySetName, xml);
                JToken valArray;
                if (collection.TryGetValue("value", out valArray))
                {
                    JArray results = (JArray)valArray;
                    foreach (JObject fetchResult in results.OfType<JObject>())
                    {
                        result.Add(fetchResult);
                    }
                }

                pagingcookie = DynamicsApiHelper.GetStringValueFromJObject(collection, "@Microsoft.Dynamics.CRM.fetchxmlpagingcookie");
            }
            while (!string.IsNullOrWhiteSpace(pagingcookie));

            return result;
        }

        /// <summary>
        /// Retrieve Limited Result Set By Fetch XML.
        /// </summary>
        /// <param name="entitySetName">entity Set Name.</param>
        /// <param name="fetchXml">Fetch XML.</param>
        /// <param name="pageNumber">page Number.</param>
        /// <param name="pagingCookie">paging Cookie.</param>
        /// <param name="userId">user Id.</param>
        /// <returns></returns>
        public (ICollection<JObject> fetchResults, string? pagingCookie) RetrieveLimitedResultSetByFetchXml(string entitySetName, string fetchXml, int pageNumber, string pagingCookie, string userId)
        {
            const int FetchCount = 5000;
            ICollection<JObject> result = new List<JObject>();
            string xml = FormatFetchXml(fetchXml, pagingCookie, pageNumber, FetchCount, userId);
            JObject collection = this.RetrieveFetchXmlResultFirstSet(entitySetName, xml);
            JToken valArray;
            if (collection.TryGetValue("value", out valArray))
            {
                JArray results = (JArray)valArray;
                foreach (JObject fetchResult in results.OfType<JObject>())
                {
                    result.Add(fetchResult);
                }
            }

            string? updatedPagingCookie = DynamicsApiHelper.GetStringValueFromJObject(collection, "@Microsoft.Dynamics.CRM.fetchxmlpagingcookie");
            return (fetchResults: result, pagingCookie: updatedPagingCookie);
        }

        /// <summary>
        /// Execute single NN associate request.
        /// </summary>
        /// <param name="firstEntitySetName">first Entity Set Name.</param>
        /// <param name="firstEntityId">first Entity Id.</param>
        /// <param name="relationshipName">relationship Name.</param>
        /// <param name="secondEntitySetName">second Entity Set Name.</param>
        /// <param name="secondEntityId">second Entity Id.</param>
        /// <param name="parseErrorAndThrowException">parse and throw exception.</param>
        /// <returns>associate request.</returns>
        [Obsolete("Use newMethod instead", false)]
        public async Task<bool> ExecuteNNAssociateAsync(string firstEntitySetName, string firstEntityId, string relationshipName, string secondEntitySetName, string secondEntityId, bool parseErrorAndThrowException = true)
        {
            bool isSuccess = false;

            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
            using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, firstEntitySetName + "(" + firstEntityId + ")/" + relationshipName + "/$ref"))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", this.AuthToken);

                JObject content = new JObject();
                content["@odata.id"] = DynamicsConnectionService.CrmAPIResource + secondEntitySetName + "(" + secondEntityId + ")";
                request.Content = new StringContent(content.ToString(), Encoding.UTF8, "application/json");

                HttpResponseMessage response = await this.SendAsync(request, parseErrorAndThrowException).ConfigureAwait(false);
                if (response.StatusCode == HttpStatusCode.NoContent)
                {
                    isSuccess = true;
                }

                return isSuccess;
            }
        }

        /// <summary>
        /// Execute single NN associate request.
        /// </summary>
        /// <param name="firstEntitySetName">first Entity Set Name.</param>
        /// <param name="firstEntityId">first Entity Id.</param>
        /// <param name="relationshipName">relationship Name.</param>
        /// <param name="secondEntitySetName">second Entity Set Name.</param>
        /// <param name="secondEntityId">second Entity Id.</param>
        /// <param name="parseErrorAndThrowException">parse and throw exception.</param>
        /// <returns>associate request.</returns>
        public bool ExecuteNNAssociate(string firstEntitySetName, string firstEntityId, string relationshipName, string secondEntitySetName, string secondEntityId, bool parseErrorAndThrowException = true)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            return this.ExecuteNNAssociateAsync(firstEntitySetName, firstEntityId, relationshipName, secondEntitySetName, secondEntityId, parseErrorAndThrowException).GetAwaiter().GetResult();
#pragma warning restore CS0618 // Type or member is obsolete
        }

        /// <summary>
        /// Execute single NN disassociate request.
        /// </summary>
        /// <param name="firstEntitySetName">first Entity Set Name.</param>
        /// <param name="firstEntityId">first Entity Id.</param>
        /// <param name="relationshipName">relationship Name.</param>
        /// <param name="secondEntitySetName">second Entity Set Name.</param>
        /// <param name="secondEntityId">second Entity Id.</param>
        /// <param name="parseErrorAndThrowException">parse and throw exception.</param>
        /// <returns>associate request.</returns>
        public async Task<bool> ExecuteNNDisassociateAsync(string firstEntitySetName, string firstEntityId, string relationshipName, string secondEntitySetName, string secondEntityId, bool parseErrorAndThrowException = true)
        {
            bool isSuccess = false;

            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
            string disassociateUrl = "?$id=" + DynamicsConnectionService.CrmAPIResource + secondEntitySetName + "(" + secondEntityId + ")";
            using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, firstEntitySetName + "(" + firstEntityId + ")/" + relationshipName + "/$ref" + disassociateUrl))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", this.AuthToken);

#pragma warning disable CS0618 // Type or member is obsolete
                HttpResponseMessage response = await this.SendAsync(request, parseErrorAndThrowException).ConfigureAwait(false);
#pragma warning restore CS0618 // Type or member is obsolete

                if (response.StatusCode == HttpStatusCode.NoContent)
                {
                    isSuccess = true;
                }

                return isSuccess;
            }
        }

        /// <summary>
        /// Execute single NN disassociate request.
        /// </summary>
        /// <param name="firstEntitySetName">first Entity Set Name.</param>
        /// <param name="firstEntityId">first Entity Id.</param>
        /// <param name="relationshipName">relationship Name.</param>
        /// <param name="secondEntitySetName">second Entity Set Name.</param>
        /// <param name="secondEntityId">second Entity Id.</param>
        /// <param name="parseErrorAndThrowException">parse and throw exception.</param>
        /// <returns>associate request.</returns>
        public bool ExecuteNNDisassociate(string firstEntitySetName, string firstEntityId, string relationshipName, string secondEntitySetName, string secondEntityId, bool parseErrorAndThrowException = true)
        {
            return this.ExecuteNNDisassociateAsync(firstEntitySetName, firstEntityId, relationshipName, secondEntitySetName, secondEntityId, parseErrorAndThrowException).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Get NN associated records.
        /// </summary>
        /// <param name="entitySetName">entity Set Name.</param>
        /// <param name="entityId">entity Id.</param>
        /// <param name="relationshipName">relationship Name.</param>
        /// <param name="selectQuery">select Query.</param>
        /// <param name="parseErrorAndThrowException">parse and throw exception.</param>
        /// <returns>NN associated records.</returns>
        [Obsolete("Use newMethod instead", false)]
        public async Task<ICollection<JObject>> GetNNAssociatedAsync(string entitySetName, string entityId, string relationshipName, string selectQuery, bool parseErrorAndThrowException = true)
        {
            List<JObject> result = new List<JObject>();

            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
            string selectQ = string.Empty;
            if (!string.IsNullOrWhiteSpace(selectQuery))
            {
                selectQ = "?$select=" + selectQuery;
            }

            using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, entitySetName + "(" + entityId + ")/" + relationshipName + selectQ))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", this.AuthToken);

                HttpResponseMessage response = await this.SendAsync(request, parseErrorAndThrowException).ConfigureAwait(false);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    JObject collection = JsonConvert.DeserializeObject<JObject>(response.Content.ReadAsStringAsync().Result);
                    JToken valArray;
                    if (collection.TryGetValue("value", out valArray))
                    {
                        JArray results = (JArray)valArray;
                        foreach (JObject fetchResult in results.OfType<JObject>())
                        {
                            result.Add(fetchResult);
                        }
                    }
                }

                return result;
            }
        }

        /// <summary>
        /// Get NN associated records.
        /// </summary>
        /// <param name="entitySetName">entity Set Name.</param>
        /// <param name="entityId">entity Id.</param>
        /// <param name="relationshipName">relationship Name.</param>
        /// <param name="selectQuery">select Query.</param>
        /// <param name="parseErrorAndThrowException">parse and throw exception.</param>
        /// <returns>NN associated records.</returns>
        public ICollection<JObject> GetNNAssociated(string entitySetName, string entityId, string relationshipName, string selectQuery, bool parseErrorAndThrowException = true)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            return this.GetNNAssociatedAsync(entitySetName, entityId, relationshipName, selectQuery, parseErrorAndThrowException).GetAwaiter().GetResult();
#pragma warning restore CS0618 // Type or member is obsolete
        }

        /// <summary>
        /// execute batch request.
        /// </summary>
        /// <param name="httpMessageContents">HTTP Message contents.</param>
        /// <returns>return HTTP Response Message.</returns>
        public async Task<HttpResponseMessage?> ExecuteBatchRequest(ICollection<HttpMessageContent> httpMessageContents)
        {
            MultipartContent? batchContent = null;
            MultipartContent? changeSetContent = null;
            if (httpMessageContents == null)
            {
                return null;
            }

            string changeSet = $"changeset_{Guid.NewGuid()}";
            changeSetContent = new MultipartContent("mixed", changeSet);
            ((List<HttpMessageContent>)httpMessageContents).ForEach((c) => changeSetContent.Add(c));
            string batchName = $"batch_{Guid.NewGuid()}";
            batchContent = new MultipartContent("mixed", batchName);

            batchContent.Add(changeSetContent);
            var responseMessage = await this.ExecutePatchBatchInDynamics(batchContent).ConfigureAwait(false);
            return responseMessage;
        }

        /// <summary>
        /// execute batch request.
        /// </summary>
        /// <param name="httpMessageContents">HTTP Message contents.</param>
        /// <returns>return HTTP Response Message.</returns>
        public async Task<ICollection<HttpResponseMessage>?> ExecuteGetBatchRequest(ICollection<HttpMessageContent> httpMessageContents)
        {
            this.logger.LogTrace("Enter into ExecuteGetBatchRequest");
            MultipartContent? batchContent = null;
            if (httpMessageContents == null)
            {
                return null;
            }

            string batchName = $"batch_{Guid.NewGuid()}";
            batchContent = new MultipartContent("mixed", batchName);
            ((List<HttpMessageContent>)httpMessageContents).ForEach((c) => batchContent.Add(c));
            var responseMessage = await this.ExecuteGetBatchInDynamics(batchContent).ConfigureAwait(false);
            this.logger.LogTrace("GetAllBatchRequestCompleted");
            return responseMessage;
        }

        /// <summary>
        /// Execute Fetch XML Batch.
        /// </summary>
        /// <param name="fetchXML">fetch XML.</param>
        /// <param name="entitySetName">entity Set Name.</param>
        /// <param name="batchName">batch Name.</param>
        /// <param name="fetchCount">fetch Count.</param>
        /// <returns></returns>
        public ICollection<JObject> ExecuteFetchXmlBatch(string fetchXML, string entitySetName, string batchName, int fetchCount = 5000)
        {
            List<JObject> result = new List<JObject>();
            if (string.IsNullOrEmpty(fetchXML))
            {
                return result;
            }

            int count = 0;
            int pageNumber = 0;
            string? pagingcookie = string.Empty;
            do
            {
                MultipartContent? batchContent = null;
                JObject? collection = null;
                try
                {
                    count++;
                    batchContent = new MultipartContent("mixed", "batch_" + (batchName + count ?? string.Empty));
                    pageNumber++; string xml = FormatFetchXml(fetchXML, pagingcookie, pageNumber, fetchCount, string.Empty);
                    using (HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, DynamicsConnectionService.CrmAPIResource + entitySetName + "?fetchXml=" + WebUtility.UrlEncode(xml)))
                    {
                        httpRequestMessage.Headers.Add("Prefer", "odata.include-annotations=*");
                        using (HttpContent httpMessageContent = new HttpMessageContent(httpRequestMessage))
                        {
                            httpMessageContent.Headers.Remove("Content-Type");
                            httpMessageContent.Headers.Add("Content-Type", "application/http");
                            httpMessageContent.Headers.Add("Content-Transfer-Encoding", "binary");
                            httpMessageContent.Headers.Add("Content-ID", count.ToString(CultureInfo.InvariantCulture));
                            httpMessageContent.Headers.Add("OData-MaxVersion", "4.0");
                            httpMessageContent.Headers.Add("OData-Version", "4.0");
                            batchContent.Add(httpMessageContent); //// Execute Batch and get response
                            var responseContent = this.ExecuteBatch(batchContent);
                            if (responseContent == null)
                            {
                                return result;
                            }

                            MultipartMemoryStreamProvider body = responseContent.ReadAsMultipartAsync().Result;
                            if (body?.Contents != null && body?.Contents.Count > 0)
                            {
                                foreach (HttpContent c in body.Contents)
                                {
                                    HttpResponseMessage responseMessage = DeserializeToResponse(c.ReadAsStreamAsync().Result);
                                    if (responseMessage != null)
                                    {
                                        collection = JsonConvert.DeserializeObject<JObject>(responseMessage.Content.ReadAsStringAsync().Result);
                                        JToken valArray;
                                        if (collection.TryGetValue("value", out valArray))
                                        {
                                            JArray results = (JArray)valArray;
                                            foreach (JObject fetchResult in results.OfType<JObject>())
                                            {
                                                result.Add(fetchResult);
                                            }
                                        }

                                        pagingcookie = DynamicsApiHelper.GetStringValueFromJObject(collection, "@Microsoft.Dynamics.CRM.fetchxmlpagingcookie");
                                    }
                                    else
                                    {
                                        pagingcookie = string.Empty;
                                    }
                                }
                            }
                            else
                            {
                                pagingcookie = string.Empty;
                            }
                        }
                    }
                }
                finally
                {
                    batchContent?.Dispose();
                }
            }
            while (!string.IsNullOrWhiteSpace(pagingcookie));
            return result;
        }

        /// <summary>
        /// Share Record To Access Team.
        /// </summary>
        /// <param name="requestName">request Name.</param>
        /// <param name="content">content message.</param>
        /// <param name="parseErrorAndThrowException">parse and throw exception.</param>
        /// <returns>if record is successfully shared or not.</returns>
        public async Task<bool> ShareRecordToAccessTeamAsync(string requestName, string content, bool parseErrorAndThrowException = true)
        {
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
            using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, DynamicsConnectionService.CrmAPIResource + requestName))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", this.AuthToken);
                if (content != null)
                {
                    request.Content = new StringContent(content.ToString(), Encoding.UTF8, "application/json");
                }

#pragma warning disable CS0618 // Type or member is obsolete
                HttpResponseMessage response = await this.SendAsync(request, parseErrorAndThrowException).ConfigureAwait(false);
#pragma warning restore CS0618 // Type or member is obsolete
                if (response.StatusCode == HttpStatusCode.NoContent)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Share Record To Access Team.
        /// </summary>
        /// <param name="requestName">request Name.</param>
        /// <param name="content">content message.</param>
        /// <param name="parseErrorAndThrowException">parse and throw exception.</param>
        /// <returns>if record is successfully shared or not.</returns>
        public bool ShareRecordToAccessTeam(string requestName, string content, bool parseErrorAndThrowException = true)
        {
            return this.ShareRecordToAccessTeamAsync(requestName, content, parseErrorAndThrowException).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Execute single create request.
        /// </summary>
        /// <param name="entitySetName">entity Set Name.</param>
        /// <param name="jsonObject">jObject value.</param>
        /// <param name="parseErrorAndThrowException">parse and throw exception.</param>
        /// <returns>created Id.</returns>
        public async Task<string?> CreateEntityAsync(string entitySetName, JObject jsonObject, bool parseErrorAndThrowException = true)
        {
            string? createdId = null;

            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
            using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, entitySetName))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", this.AuthToken);
                if (jsonObject != null)
                {
                    request.Content = new StringContent(jsonObject.ToString(), Encoding.UTF8, "application/json");
                }

#pragma warning disable CS0618 // Type or member is obsolete
                HttpResponseMessage response = await this.SendAsync(request, parseErrorAndThrowException).ConfigureAwait(false);
#pragma warning restore CS0618 // Type or member is obsolete

                if (response.StatusCode == HttpStatusCode.NoContent)
                {
                    string? uri = response.Headers.GetValues("OData-EntityId").FirstOrDefault();
                    if (!string.IsNullOrWhiteSpace(uri) && !string.IsNullOrWhiteSpace(DynamicsConnectionService.CrmAPIResource))
                    {
                        createdId = uri.Replace(DynamicsConnectionService.CrmAPIResource, string.Empty, StringComparison.OrdinalIgnoreCase).Replace(entitySetName, string.Empty, StringComparison.OrdinalIgnoreCase).Substring(1, 36);
                    }
                }

                return createdId;
            }
        }

        /// <summary>
        /// Execute single create request.
        /// </summary>
        /// <param name="entitySetName">entity Set Name.</param>
        /// <param name="jsonObject">jObject value.</param>
        /// <param name="parseErrorAndThrowException">parse and throw exception.</param>
        /// <returns>created Id.</returns>
        public string? CreateEntity(string entitySetName, JObject jsonObject, bool parseErrorAndThrowException = true)
        {
            return this.CreateEntityAsync(entitySetName, jsonObject, parseErrorAndThrowException).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Execute single update request.
        /// </summary>
        /// <param name="entitySetName">entity Set Name.</param>
        /// <param name="entityId">entity Id.</param>
        /// <param name="jsonObject">jObject value.</param>
        /// <param name="parseErrorAndThrowException">parse Error And Throw Exception.</param>
        /// <returns>return updated record ID.</returns>
        [Obsolete("Use newMethod instead", false)]
        public async Task<string> UpdateEntityAsync(string entitySetName, string entityId, JObject jsonObject, bool parseErrorAndThrowException = true)
        {
            string updatedRecordId = string.Empty;

            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
            using (HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("PATCH"), entitySetName + "(" + entityId + ")"))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", this.AuthToken);
                if (jsonObject != null)
                {
                    request.Content = new StringContent(jsonObject.ToString(), Encoding.UTF8, "application/json");
                }

                var response = await this.SendAsync(request, parseErrorAndThrowException).ConfigureAwait(false);
                if (response.StatusCode == HttpStatusCode.NoContent)
                {
                    string? uri = response.Headers.GetValues("OData-EntityId").FirstOrDefault();
                    if (!string.IsNullOrWhiteSpace(uri) && !string.IsNullOrWhiteSpace(DynamicsConnectionService.CrmAPIResource))
                    {
                        updatedRecordId = uri.Replace(DynamicsConnectionService.CrmAPIResource, string.Empty, StringComparison.OrdinalIgnoreCase).Replace(entitySetName, string.Empty, StringComparison.OrdinalIgnoreCase).Substring(1, 36);
                    }
                }

                return updatedRecordId;
            }
        }

        /// <summary>
        /// Execute single update request.
        /// </summary>
        /// <param name="entitySetName">entity Set Name.</param>
        /// <param name="entityId">entity Id.</param>
        /// <param name="jsonObject">jObject value.</param>
        /// <param name="parseErrorAndThrowException">parse Error And Throw Exception.</param>
        /// <returns>return updated record ID.</returns>
        public string UpdateEntity(string entitySetName, string entityId, JObject jsonObject, bool parseErrorAndThrowException = true)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            return this.UpdateEntityAsync(entitySetName, entityId, jsonObject, parseErrorAndThrowException).GetAwaiter().GetResult();
#pragma warning restore CS0618 // Type or member is obsolete
        }

        /// <summary>
        /// create or update entity object.
        /// </summary>
        /// <param name="entitySetName">entity set name.</param>
        /// <param name="compositeKey">composite Key.</param>
        /// <param name="jsonObject">JSON Object.</param>
        /// <param name="parseErrorAndThrowException">parse Error And Throw Exception.</param>
        /// <returns></returns>
        public async Task<JObject?> UpsertEntityAsync(string entitySetName, string compositeKey, JObject jsonObject, bool parseErrorAndThrowException = true)
        {
            JObject? result = null;

            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
            using (HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("PATCH"), entitySetName + "(" + compositeKey + ")"))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", this.AuthToken);
                request.Headers.Add("prefer", "return=representation");
                if (jsonObject != null)
                {
                    request.Content = new StringContent(jsonObject.ToString(), Encoding.UTF8, "application/json");
                }

#pragma warning disable CS0618 // Type or member is obsolete
                var response = await this.SendAsync(request, parseErrorAndThrowException).ConfigureAwait(false);
#pragma warning restore CS0618 // Type or member is obsolete
                if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created)
                {
                    result = JsonConvert.DeserializeObject<JObject>(response.Content.ReadAsStringAsync().Result);
                }

                return result;
            }
        }

        /// <summary>
        /// create or update entity object.
        /// </summary>
        /// <param name="entitySetName">entity set name.</param>
        /// <param name="compositeKey">composite Key.</param>
        /// <param name="jsonObject">JSON Object.</param>
        /// <param name="parseErrorAndThrowException">parse Error And Throw Exception.</param>
        /// <returns></returns>
        public JObject? UpsertEntity(string entitySetName, string compositeKey, JObject jsonObject, bool parseErrorAndThrowException = true)
        {
            return this.UpsertEntityAsync(entitySetName, compositeKey, jsonObject, parseErrorAndThrowException).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Execute single delete request in Async.
        /// </summary>
        /// <param name="entitySetName">entity Set Name.</param>
        /// <param name="entityId">entity Id.</param>
        /// <param name="parseErrorAndThrowException">parse Error And Throw Exception.</param>
        /// <returns>success boolean value.</returns>
        public async Task<bool> DeleteEntityAsync(string entitySetName, string entityId, bool parseErrorAndThrowException = true)
        {
            bool isSuccess = false;

            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
            using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, entitySetName + "(" + entityId + ")"))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", this.AuthToken);

#pragma warning disable CS0618 // Type or member is obsolete
                HttpResponseMessage response = await this.SendAsync(request, parseErrorAndThrowException).ConfigureAwait(false);
#pragma warning restore CS0618 // Type or member is obsolete

                if (response.StatusCode == HttpStatusCode.NoContent)
                {
                    isSuccess = true;
                }

                return isSuccess;
            }
        }

        /// <summary>
        /// Execute single delete request in Async.
        /// </summary>
        /// <param name="entitySetName">entity Set Name.</param>
        /// <param name="entityId">entity Id.</param>
        /// <param name="parseErrorAndThrowException">parse Error And Throw Exception.</param>
        /// <returns>success boolean value.</returns>
        public bool DeleteEntity(string entitySetName, string entityId, bool parseErrorAndThrowException = true)
        {
            return this.DeleteEntityAsync(entitySetName, entityId, parseErrorAndThrowException).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Retrieve record using Guid.
        /// </summary>
        /// <param name="entitySetName">entity Set Name.</param>
        /// <param name="entityId">entity Id.</param>
        /// <param name="columns">columns value.</param>
        /// <param name="parseErrorAndThrowException">parse Error And Throw Exception.</param>
        /// <returns>record Guid.</returns>
        public async Task<JObject> RetrieveEntityByIdAsync(string entitySetName, string entityId, string[] columns = null, bool parseErrorAndThrowException = true)
        {
            JObject? result = null;

            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
            string query = string.Empty;

            if (columns != null)
            {
                query = (columns.Length > 0) ? ("?$select=" + string.Join(",", columns)) : string.Empty;
            }

            using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, entitySetName + "(" + entityId + ")" + query))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", this.AuthToken);
#pragma warning disable CS0618 // Type or member is obsolete
                var response = await this.SendAsync(request, parseErrorAndThrowException).ConfigureAwait(false);
#pragma warning restore CS0618 // Type or member is obsolete
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    result = JsonConvert.DeserializeObject<JObject>(response.Content.ReadAsStringAsync().Result);
                }
                else if (response.StatusCode == HttpStatusCode.NotFound && parseErrorAndThrowException)
                {
                    throw new HttpRequestException(HttpStatusCode.NotFound.ToString());
                }

                return result;
            }
        }

        /// <summary>
        /// Retrieve record using Guid.
        /// </summary>
        /// <param name="entitySetName">entity Set Name.</param>
        /// <param name="entityId">entity Id.</param>
        /// <param name="columns">columns value.</param>
        /// <param name="parseErrorAndThrowException">parse Error And Throw Exception.</param>
        /// <returns>record Guid.</returns>
        public JObject RetrieveEntityById(string entitySetName, string entityId, string[] columns = null, bool parseErrorAndThrowException = true)
        {
            return this.RetrieveEntityByIdAsync(entitySetName, entityId, columns, parseErrorAndThrowException).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Retrieve Result Set By API Filter Query.
        /// </summary>
        /// <param name="entitySetName">entity set name.</param>
        /// <param name="selectQuery">select query.</param>
        /// <param name="filterQuery">filter query.</param>
        /// <param name="parseErrorAndThrowException">parse Error And Throw Exception.</param>
        /// <returns>Result set by API.</returns>
        public async Task<List<JObject>> RetrieveResultSetByFilterAsync(string entitySetName, string selectQuery, string filterQuery, bool parseErrorAndThrowException = true)
        {
            List<JObject> result = new List<JObject>();

            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
            selectQuery = !string.IsNullOrWhiteSpace(selectQuery) ? ("$select=" + selectQuery) : string.Empty;
            filterQuery = !string.IsNullOrWhiteSpace(filterQuery) ? ("$filter=" + filterQuery) : string.Empty;
            string? query = null;
            if (!string.IsNullOrWhiteSpace(selectQuery) && !string.IsNullOrWhiteSpace(filterQuery))
            {
                query = "?" + selectQuery + "&" + filterQuery;
            }
            else if (!string.IsNullOrWhiteSpace(selectQuery) && string.IsNullOrWhiteSpace(filterQuery))
            {
                query = "?" + selectQuery;
            }
            else if (string.IsNullOrWhiteSpace(selectQuery) && !string.IsNullOrWhiteSpace(filterQuery))
            {
                query = "?" + filterQuery;
            }

            using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, entitySetName + query))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", this.AuthToken);
#pragma warning disable CS0618 // Type or member is obsolete
                var response = await this.SendAsync(request, parseErrorAndThrowException).ConfigureAwait(false);
#pragma warning restore CS0618 // Type or member is obsolete

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    JObject collection = JsonConvert.DeserializeObject<JObject>(response.Content.ReadAsStringAsync().Result);
                    JToken valArray;
                    if (collection.TryGetValue("value", out valArray))
                    {
                        JArray results = (JArray)valArray;
                        foreach (JObject fetchResult in results.OfType<JObject>())
                        {
                            result.Add(fetchResult);
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Retrieve Result Set By API Filter Query.
        /// </summary>
        /// <param name="entitySetName">entity set name.</param>
        /// <param name="selectQuery">select query.</param>
        /// <param name="filterQuery">filter query.</param>
        /// <param name="parseErrorAndThrowException">parse Error And Throw Exception.</param>
        /// <returns>Result set by API.</returns>
        public ICollection<JObject> RetrieveResultSetByFilter(string entitySetName, string selectQuery, string filterQuery, bool parseErrorAndThrowException = true)
        {
            return this.RetrieveResultSetByFilterAsync(entitySetName, selectQuery, filterQuery, parseErrorAndThrowException).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Execute global action asynchronous.
        /// </summary>
        /// <param name="requestParameter">action request input parameter.</param>
        /// <param name="actionName">action name.</param>
        /// <param name="parseErrorAndThrowException">parse Error And Throw Exception.</param>
        /// <returns>action response output parameter.</returns>
        public async Task<JObject?> ExecuteGlobalActionAsync(JObject requestParameter, string actionName, bool parseErrorAndThrowException = true)
        {
            JObject? actionExecuteResult = null;
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
            using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, actionName))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", this.AuthToken);
                if (requestParameter != null)
                {
                    request.Content = new StringContent(requestParameter.ToString(), Encoding.UTF8, "application/json");
                }

#pragma warning disable CS0618 // Type or member is obsolete
                var response = await this.SendAsync(request, parseErrorAndThrowException).ConfigureAwait(false);
#pragma warning restore CS0618 // Type or member is obsolete
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    actionExecuteResult = JsonConvert.DeserializeObject<JObject>(response.Content.ReadAsStringAsync().Result);
                }
            }

            return actionExecuteResult;
        }

        /// <summary>
        /// Execute global action asynchronous.
        /// </summary>
        /// <param name="requestParameter">action request input parameter.</param>
        /// <param name="actionName">action name.</param>
        /// <param name="parseErrorAndThrowException">parse Error And Throw Exception.</param>
        /// <returns>action response output parameter.</returns>
        public JObject? ExecuteGlobalAction(JObject requestParameter, string actionName, bool parseErrorAndThrowException = true)
        {
            return this.ExecuteGlobalActionAsync(requestParameter, actionName, parseErrorAndThrowException).GetAwaiter().GetResult();
        }

        /// <summary>
        /// executes an entity bound action.
        /// </summary>
        /// <param name="requestParameter">request parameters object.</param>
        /// <param name="actionName">action name.</param>
        /// <param name="recordId">record id.</param>
        /// <param name="entitySchemaName">entity schema name.</param>
        /// <param name="parseErrorAndThrowException">parse Error And Throw Exception.</param>
        /// <returns>action response.</returns>
        public async Task<JObject?> ExecuteEntityBoundActionAsync(JObject requestParameter, string actionName, string recordId, string entitySchemaName, bool parseErrorAndThrowException = true)
        {
            JObject? actionExecuteResult = null;
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
            using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, this.httpClient.BaseAddress + entitySchemaName + "(" + recordId + ")/Microsoft.Dynamics.CRM." + actionName))
            {
                if (requestParameter != null)
                {
                    request.Content = new StringContent(requestParameter.ToString(), Encoding.UTF8, "application/json");
                }

                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", this.AuthToken);
#pragma warning disable CS0618 // Type or member is obsolete
                var response = await this.SendAsync(request, parseErrorAndThrowException).ConfigureAwait(false);
#pragma warning restore CS0618 // Type or member is obsolete
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    actionExecuteResult = JsonConvert.DeserializeObject<JObject>(response.Content.ReadAsStringAsync().Result);
                }
            }

            return actionExecuteResult;
        }

        /// <summary>
        /// executes an entity bound action.
        /// </summary>
        /// <param name="requestParameter">request parameters object.</param>
        /// <param name="actionName">action name.</param>
        /// <param name="recordId">record id.</param>
        /// <param name="entitySchemaName">entity schema name.</param>
        /// <param name="parseErrorAndThrowException">parse Error And Throw Exception.</param>
        /// <returns>action response.</returns>
        public JObject? ExecuteEntityBoundAction(JObject requestParameter, string actionName, string recordId, string entitySchemaName, bool parseErrorAndThrowException = true)
        {
            return this.ExecuteEntityBoundActionAsync(requestParameter, actionName, recordId, entitySchemaName, parseErrorAndThrowException).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Execute Send Email.
        /// </summary>
        /// <param name="requestParameter">Request Parameter.</param>
        /// <param name="emailId">Email Id.</param>
        /// <param name="parseErrorAndThrowException">parse Error And Throw Exception.</param>
        /// <returns>Returns the object.</returns>
        public async Task<JObject?> ExecuteSendEmailActionAsync(JObject requestParameter, string emailId, bool parseErrorAndThrowException = true)
        {
            JObject? actionExecuteResult = null;
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
            if (requestParameter != null)
            {
                using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, this.httpClient.BaseAddress + "emails(" + emailId + ")/Microsoft.Dynamics.CRM.SendEmail"))
                {
                    request.Content = new StringContent(requestParameter.ToString(), Encoding.UTF8, "application/json");
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", this.AuthToken);

#pragma warning disable CS0618 // Type or member is obsolete
                    var response = await this.SendAsync(request, parseErrorAndThrowException).ConfigureAwait(false);
#pragma warning restore CS0618 // Type or member is obsolete
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        actionExecuteResult = JsonConvert.DeserializeObject<JObject>(response.Content.ReadAsStringAsync().Result);
                    }
                }
            }

            return actionExecuteResult;
        }

        /// <summary>
        /// Execute Send Email.
        /// </summary>
        /// <param name="requestParameter">Request Parameter.</param>
        /// <param name="emailId">Email Id.</param>
        /// <param name="parseErrorAndThrowException">parse Error And Throw Exception.</param>
        /// <returns>Returns the object.</returns>
        public JObject? ExecuteSendEmailAction(JObject requestParameter, string emailId, bool parseErrorAndThrowException = true)
        {
            return this.ExecuteSendEmailActionAsync(requestParameter, emailId, parseErrorAndThrowException).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Execute add members to team.
        /// </summary>
        /// <param name="userIdList">user list which need to be added to team.</param>
        /// <param name="teamId">team id.</param>
        /// <param name="parseErrorAndThrowException">parse Error And Throw Exception.</param>
        /// <returns>if task is successfull or not.</returns>
        public async Task<bool> ExecuteAddTeamMembersToTeam(ICollection<JToken> userIdList, string teamId, bool parseErrorAndThrowException = true)
        {
            bool isSuccess = false;
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

            string? teamMemberList = this.FormatTeamMembers(userIdList);

            if (teamMemberList != null)
            {
                using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "teams(" + teamId + ")/Microsoft.Dynamics.CRM.AddMembersTeam"))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", this.AuthToken);
                    request.Content = new StringContent(teamMemberList, Encoding.UTF8, "application/json");

#pragma warning disable CS0618 // Type or member is obsolete
                    HttpResponseMessage response = await this.SendAsync(request, parseErrorAndThrowException).ConfigureAwait(false);
#pragma warning restore CS0618 // Type or member is obsolete
                    if (response != null && response.StatusCode == HttpStatusCode.NoContent)
                    {
                        isSuccess = true;
                    }
                    else if (response != null)
                    {
                        this.logger.LogTrace("response statuscode :" + response.StatusCode);
                        this.logger.LogTrace("Team Member List to be added in new team from Queue: " + teamMemberList.ToString());
                    }
                }
            }
            else
            {
                this.logger.LogTrace("Team member List is null.");
            }

            return isSuccess;
        }

        /// <summary>
        /// Execute remove members from team.
        /// </summary>
        /// <param name="userIdList"></param>
        /// <param name="teamId">ser list which need to be removed to team.</param>
        /// <param name="parseErrorAndThrowException">parse Error And Throw Exception.</param>
        /// <returns>if task is successfull or not.</returns>
        public async Task<bool> ExecuteRemoveTeamMembersToTeam(ICollection<JToken> userIdList, string teamId, bool parseErrorAndThrowException = true)
        {
            bool isSuccess = false;
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

            string? teamMemberList = this.FormatTeamMembers(userIdList);

            if (teamMemberList != null)
            {
                using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "teams(" + teamId + ")/Microsoft.Dynamics.CRM.RemoveMembersTeam"))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", this.AuthToken);
                    request.Content = new StringContent(teamMemberList, Encoding.UTF8, "application/json");

#pragma warning disable CS0618 // Type or member is obsolete
                    HttpResponseMessage response = await this.SendAsync(request, parseErrorAndThrowException).ConfigureAwait(false);
#pragma warning restore CS0618 // Type or member is obsolete
                    if (response != null && response.StatusCode == HttpStatusCode.NoContent)
                    {
                        isSuccess = true;
                    }
                    else if (response != null)
                    {
                        this.logger.LogTrace("response statuscode :" + response.StatusCode);
                        this.logger.LogTrace("Team Member List to be removed" + teamMemberList.ToString());
                    }
                }
            }
            else
            {
                this.logger.LogTrace("Team member List is null.");
            }

            return isSuccess;
        }

        /// <summary>
        /// Format team members.
        /// </summary>
        /// <param name="users">get user JSON list.</param>
        /// <returns>return user JSON list.</returns>
        public string? FormatTeamMembers(ICollection<JToken> users)
        {
            string? teamMemberList = null;
            if (users != null)
            {
                List<JToken> usersList = users.ToList();
                JObject usersFormatedList =
                new JObject(
                    new JProperty(
                        "Members",
                        new JArray(
                                    from user in usersList
                                    select new JObject(
                                                        new JProperty("@odata.type", "Microsoft.Dynamics.CRM.systemuser"),
                                                        new JProperty("systemuserid", user)))));

                teamMemberList = Convert.ToString(usersFormatedList, CultureInfo.InvariantCulture);
            }
            else
            {
                this.logger.LogTrace("No Team Members exist");
            }

            return teamMemberList;
        }

        /// <summary>
        /// Execute Send Email.
        /// </summary>
        /// <param name="requestParameter">Request Parameter.</param>
        /// <param name="parseErrorAndThrowException">parse Error And Throw Exception.</param>
        /// <returns>Returns the object.</returns>
        public async Task<JObject?> ExecuteSendEmailFromTemplateActionAsync(JObject requestParameter, bool parseErrorAndThrowException = true)
        {
            JObject? actionExecuteResult = null;
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
            if (requestParameter != null)
            {
                using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, this.httpClient.BaseAddress + "SendEmailFromTemplate"))
                {
                    request.Content = new StringContent(requestParameter.ToString(), Encoding.UTF8, "application/json");
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", this.AuthToken);
#pragma warning disable CS0618 // Type or member is obsolete
                    HttpResponseMessage response = await this.SendAsync(request, parseErrorAndThrowException).ConfigureAwait(true);
#pragma warning restore CS0618 // Type or member is obsolete
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        actionExecuteResult = JsonConvert.DeserializeObject<JObject>(response.Content.ReadAsStringAsync().Result);
                    }
                }
            }

            return actionExecuteResult;
        }

        /// <summary>
        /// Execute Send Email.
        /// </summary>
        /// <param name="requestParameter">Request Parameter.</param>
        /// <param name="parseErrorAndThrowException">parse Error And Throw Exception.</param>
        /// <returns>Returns the object.</returns>
        public JObject? ExecuteSendEmailFromTemplateAction(JObject requestParameter, bool parseErrorAndThrowException = true)
        {
            return this.ExecuteSendEmailFromTemplateActionAsync(requestParameter, parseErrorAndThrowException).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Retrieve Fetch Xml Result First Set.
        /// </summary>
        /// <param name="entitySetName">entity Set Name.</param>
        /// <param name="fetchXml">fetch Xml.</param>
        /// <param name="parseErrorAndThrowException">parse and throw exception.</param>
        /// <returns>fetch xml value.</returns>
        public async Task<ICollection<JObject>> RetrieveTopRecordsFromFetchXmlAsync(string entitySetName, string fetchXml, bool parseErrorAndThrowException = true)
        {
            JObject? collection = null;
            List<JObject> result = new List<JObject>();
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
            using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, entitySetName + "?fetchXml=" + WebUtility.UrlEncode(fetchXml)))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", this.AuthToken);
                ////request.Headers.Add("Prefer", "odata.maxpagesize=5000");
                request.Headers.Add("Prefer", "odata.include-annotations=*");

#pragma warning disable CS0618 // Type or member is obsolete
                HttpResponseMessage response = await this.SendAsync(request, parseErrorAndThrowException).ConfigureAwait(false);
#pragma warning restore CS0618 // Type or member is obsolete
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    collection = JsonConvert.DeserializeObject<JObject>(response.Content.ReadAsStringAsync().Result);
                    JToken valArray;
                    if (collection.TryGetValue("value", out valArray))
                    {
                        JArray results = (JArray)valArray;
                        foreach (JObject fetchResult in results.OfType<JObject>())
                        {
                            result.Add(fetchResult);
                        }
                    }
                }

                return result;
            }
        }

        /// <summary>
        /// Retrieve Fetch Xml Result First Set.
        /// </summary>
        /// <param name="entitySetName">entity Set Name.</param>
        /// <param name="fetchXml">fetch Xml.</param>
        /// <param name="parseErrorAndThrowException">parse and throw exception.</param>
        /// <returns>fetch xml value.</returns>
        public ICollection<JObject> RetrieveTopRecordsFromFetchXml(string entitySetName, string fetchXml, bool parseErrorAndThrowException = true)
        {
            return this.RetrieveTopRecordsFromFetchXmlAsync(entitySetName, fetchXml, parseErrorAndThrowException).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Retrieve limited result set by change tracking.
        /// </summary>
        /// <param name="entitySetName">entity set name.</param>
        /// <param name="selectQuery">select query.</param>
        /// <param name="deltaToken">existing paging token if any.</param>
        /// <param name="pagingCookie">paging cookie if any.</param>
        /// <param name="parseErrorAndThrowException">parse and throw exception.</param>
        /// <returns></returns>
        public (ICollection<JObject> results, string? pagingCookieVal, string? deltaTokenVal) RetrieveLimitedResultSetByChangeTracking(string entitySetName, string selectQuery, string deltaToken, string pagingCookie, bool parseErrorAndThrowException = true)
        {
            List<JObject> result = new List<JObject>();

            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
            selectQuery = !string.IsNullOrWhiteSpace(selectQuery) ? ("$select=" + selectQuery) : string.Empty;
            deltaToken = !string.IsNullOrWhiteSpace(deltaToken) ? ("$deltatoken=" + deltaToken) : string.Empty;
            string? query = null;

            if (!string.IsNullOrWhiteSpace(selectQuery) && !string.IsNullOrWhiteSpace(deltaToken))
            {
                query = "?" + selectQuery + "&" + deltaToken;
            }
            else if (!string.IsNullOrWhiteSpace(selectQuery) && string.IsNullOrWhiteSpace(deltaToken))
            {
                query = "?" + selectQuery;
            }
            else if (string.IsNullOrWhiteSpace(selectQuery) && !string.IsNullOrWhiteSpace(deltaToken))
            {
                query = "?" + "$select=*" + "&" + deltaToken;
            }
            else if (string.IsNullOrWhiteSpace(selectQuery) && string.IsNullOrWhiteSpace(deltaToken))
            {
                query = "?" + "$select=*";
            }

            if (!string.IsNullOrWhiteSpace(pagingCookie))
            {
                query = query + "&" + pagingCookie;
            }

            string? updatedPagingToken = string.Empty;
            string? updatedDeltaToken = string.Empty;

            using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, entitySetName + query))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", this.AuthToken);
                request.Headers.Add("Prefer", "odata.track-changes");
#pragma warning disable CS0618 // Type or member is obsolete
                var response = this.SendAsync(request, parseErrorAndThrowException).GetAwaiter().GetResult();
#pragma warning restore CS0618 // Type or member is obsolete

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    JObject collection = JsonConvert.DeserializeObject<JObject>(response.Content.ReadAsStringAsync().Result);
                    JToken valArray;

                    if (collection.TryGetValue("value", out valArray))
                    {
                        JArray results = (JArray)valArray;
                        foreach (JObject fetchResult in results.OfType<JObject>())
                        {
                            result.Add(fetchResult);
                        }
                    }

                    updatedPagingToken = DynamicsApiHelper.GetStringValueFromJObject(collection, "@odata.nextLink");
                    if (!string.IsNullOrEmpty(updatedPagingToken))
                    {
                        int pagingCookieIndex = updatedPagingToken.IndexOf("$skiptoken=", StringComparison.OrdinalIgnoreCase);
                        updatedPagingToken = updatedPagingToken.Substring(pagingCookieIndex, updatedPagingToken.Length - pagingCookieIndex);
                    }

                    updatedDeltaToken = DynamicsApiHelper.GetStringValueFromJObject(collection, "@odata.deltaLink");
                    if (!string.IsNullOrEmpty(updatedDeltaToken))
                    {
                        int deltaTokenIndex = updatedDeltaToken.IndexOf("$deltatoken=", StringComparison.OrdinalIgnoreCase);
                        updatedDeltaToken = updatedDeltaToken.Substring(deltaTokenIndex + 12, updatedDeltaToken.Length - (deltaTokenIndex + 12));
                    }
                    else
                    {
                        updatedDeltaToken = string.Empty;
                    }
                }
                else
                {
                    throw new HttpRequestException($"Error in retrieving records using change tracking. Exception Reason is {response.ReasonPhrase}. Error Message is : {response.Content.ReadAsStringAsync().Result}");
                }
            }

            return (results: result, pagingCookieVal: updatedPagingToken, deltaTokenVal: updatedDeltaToken);
        }

        /// <summary>
        /// Get Paging cookie from result.
        /// </summary>
        /// <param name="cookie">cookie value.</param>
        /// <returns>paging cookie.</returns>
        private static string? GetPagingCookie(string cookie)
        {
            string decodedcookie = WebUtility.UrlDecode(cookie);
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.DtdProcessing = DtdProcessing.Prohibit;
            StringReader stringReader = new StringReader(decodedcookie);
            using (XmlTextReader reader = new XmlTextReader(stringReader))
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(reader);
                XmlAttribute? xmlAttribute = doc?.FirstChild?.Attributes?["pagingcookie"];
                string? attrpagingcookie = xmlAttribute?.InnerXml;
                string? pagingcookie = WebUtility.UrlDecode(attrpagingcookie);

                return pagingcookie;
            }
        }

        /// <summary>
        /// Format Xml for paging cookie.
        /// </summary>
        /// <param name="xml">xml value.</param>
        /// <param name="cookie">cookie value.</param>
        /// <param name="page">page value.</param>
        /// <param name="count">count value.</param>
        /// <param name="userId">user Id.</param>
        /// <returns>string builders object.</returns>
        private static string FormatFetchXml(string xml, string cookie, int page, int count, string userId)
        {
            StringBuilder sb = new StringBuilder(1024);
            try
            {
                if (!string.IsNullOrWhiteSpace(xml) && xml.Contains("<fetch ", StringComparison.OrdinalIgnoreCase))
                {
                    xml = xml.Replace("<fetch ", "<fetch page=\"" + page.ToString(CultureInfo.InvariantCulture) + "\" count=\"" + count.ToString(CultureInfo.InvariantCulture) + "\" ", StringComparison.OrdinalIgnoreCase);
                }

                if (!string.IsNullOrWhiteSpace(userId) && !string.IsNullOrWhiteSpace(xml) && xml.Contains("operator=\"eq-userid\"", StringComparison.OrdinalIgnoreCase))
                {
                    xml = xml.Replace("operator=\"eq-userid\"", "operator=\"eq\" uitype=\"systemuser\" value=\"{" + userId + "}\"", StringComparison.OrdinalIgnoreCase);
                }

                XmlReaderSettings settings = new XmlReaderSettings();
#pragma warning disable CS0618 // Type or member is obsolete
                settings.ProhibitDtd = true;
#pragma warning restore CS0618 // Type or member is obsolete
                settings.XmlResolver = null;
                settings.DtdProcessing = DtdProcessing.Prohibit;
                StringReader stringReader = new StringReader(xml);

                using (XmlTextReader reader = new XmlTextReader(stringReader))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(reader);

                    XmlAttributeCollection? attrs = doc.DocumentElement?.Attributes;

                    if (!string.IsNullOrWhiteSpace(cookie))
                    {
                        string? pagingcookie = GetPagingCookie(cookie);
                        if (!string.IsNullOrWhiteSpace(pagingcookie))
                        {
                            XmlAttribute pagingAttr = doc.CreateAttribute("paging-cookie");
                            pagingAttr.Value = pagingcookie;
                            attrs?.Append(pagingAttr);
                        }
                    }

                    StringWriter stringWriter = new StringWriter(sb, CultureInfo.InvariantCulture);
                    XmlTextWriter writer = new XmlTextWriter(stringWriter);
                    doc.WriteTo(writer);
                    writer.Close();
                }
            }
            catch (Exception e)
            {
                var exception = e;
                throw exception;
            }

            return sb.ToString();
        }

        /// <summary>
        /// Read HTTP Message Contents.
        /// </summary>
        /// <param name="body">response body.</param>
        /// <returns></returns>
        private static async Task<List<HttpResponseMessage>> ReadHttpContents(MultipartMemoryStreamProvider body)
        {
            List<HttpResponseMessage> results = new List<HttpResponseMessage>();
            if (body?.Contents != null)
            {
                foreach (HttpContent c in body.Contents)
                {
                    if (c.IsMimeMultipartContent())
                    {
                        results.AddRange(await ReadHttpContents(await c.ReadAsMultipartAsync().ConfigureAwait(false)).ConfigureAwait(false));
                    }
                    else if (c.IsHttpResponseMessageContent())
                    {
                        HttpResponseMessage responseMessage = await c.ReadAsHttpResponseMessageAsync().ConfigureAwait(false);
                        if (responseMessage != null)
                        {
                            results.Add(responseMessage);
                        }
                    }
                    else
                    {
                        HttpResponseMessage responseMessage = DeserializeToResponse(await c.ReadAsStreamAsync().ConfigureAwait(false));
                        if (responseMessage != null)
                        {
                            results.Add(responseMessage);
                        }
                    }
                }
            }

            return results;
        }

        /// <summary>
        /// Read HTTP Content with request body.
        /// </summary>
        /// <param name="responseMessage">response message.</param>
        /// <returns>return HTTP Response Message.</returns>
        private static async Task<List<HttpResponseMessage>> ReadHttpContentsWithRequestBody(HttpResponseMessage responseMessage)
        {
            var responseContent = responseMessage.Content.ReadAsMultipartAsync().Result;
            var requestMessage = (responseMessage?.RequestMessage?.Content as IEnumerable<HttpContent>)?.ToList();

            List<HttpResponseMessage> results = new List<HttpResponseMessage>();
            if (responseContent?.Contents != null && requestMessage != null)
            {
                int requestMessageCounter = 0;
                foreach (HttpContent c in responseContent.Contents)
                {
                    var requestMsg = ((HttpMessageContent)requestMessage[requestMessageCounter]).HttpRequestMessage;
                    HttpResponseMessage res = DeserializeToResponse(await c.ReadAsStreamAsync().ConfigureAwait(false));
                    res.RequestMessage = requestMsg;

                    results.Add(res);

                    requestMessageCounter++;
                }
            }

            return results;
        }

        /// <summary>
        /// Sends all requests with retry capabilities.
        /// </summary>
        /// <param name="request">The request to send.</param>
        /// <param name="parseErrorResponse">Is Parse error response.</param>
        /// <param name="retryCount">The number of retry attempts.</param>
        /// <returns>The response for the request.</returns>
        [Obsolete("Use newMethod instead", false)]
        private async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage? request,
            bool parseErrorResponse = true,
            int retryCount = 0)
        {
            var clonedRequest = request != null ? request.Clone() : new HttpRequestMessage();
            try
            {
                HttpResponseMessage response = await this.httpClient.SendAsync(clonedRequest, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
                if (!response.IsSuccessStatusCode)
                {
                    // Handle exception & retry mechanism for API rate limit, Bad Gateway, Service not available
                    if ((int)response.StatusCode != Constant.TooManyRequest &&
                        (int)response.StatusCode != Constant.RequestTimeout &&
                        (int)response.StatusCode != Constant.BadGateway &&
                        (int)response.StatusCode != Constant.ServiceUnavailable &&
                        (int)response.StatusCode != Constant.GatewayTimeout)
                    {
                        // Not a service protection limit error
                        if (parseErrorResponse)
                        {
                            throw new HttpRequestException($"Error in processing dynamics request. Error Message: {response.ReasonPhrase}; Status Code: {response.StatusCode}; Content: {response.Content.ReadAsStringAsync().GetAwaiter().GetResult()}");
                        }
                        else
                        {
                            return response;
                        }
                    }
                    else
                    {
                        // Give up re-trying if exceeding the maxRetries
                        if (++retryCount >= Constant.DynamicsApiMaxRetries)
                        {
                            throw new HttpRequestException($"Error in processing dynamics request. Error Message: {response.ReasonPhrase}; Status Code: {response.StatusCode}; Content: {response.Content.ReadAsStringAsync().GetAwaiter().GetResult()}");
                        }

                        // Try to use the Retry-After header value if it is returned.
                        if (response.Headers.Contains("Retry-After"))
                        {
#pragma warning disable CS8604 // Possible null reference argument.
                            int seconds = int.Parse(response.Headers.GetValues("Retry-After").FirstOrDefault(), CultureInfo.InvariantCulture);
#pragma warning restore CS8604 // Possible null reference argument.
                            this.logger.LogWarning($"Encountered API Rate limit. Retry after duration is {seconds}. Error message: {response.ReasonPhrase}; Status Code: {response.StatusCode}; Content: {response.Content.ReadAsStringAsync().GetAwaiter().GetResult()}");
                            if (seconds > 0)
                            {
                                Thread.Sleep(TimeSpan.FromSeconds(seconds));
                            }
                            else
                            {
                                // Use exponential backoff strategy
                                int minutes = (int)Math.Pow(2, retryCount - 1);
                                Thread.Sleep(TimeSpan.FromMinutes(minutes));
                            }
                        }
                        else
                        {
                            // Otherwise, use an exponential backoff strategy
                            int minutes = (int)Math.Pow(2, retryCount - 1);
                            this.logger.LogWarning($"Encountered API Rate limit. Retry after duration is {minutes * 60}. Error message: {response.ReasonPhrase}; Status Code: {response.StatusCode}; Content: {response.Content.ReadAsStringAsync().GetAwaiter().GetResult()}");
                            Thread.Sleep(TimeSpan.FromMinutes(minutes));
                        }

                        return await this.SendAsync(request, parseErrorResponse, retryCount).ConfigureAwait(false);
                    }
                }
                else
                {
                    return response;
                }
            }
            finally
            {
                if (clonedRequest != null)
                {
                    clonedRequest.Dispose();
                }
            }
        }

        /// <summary>
        /// Sends all requests with retry capabilities.
        /// </summary>
        /// <param name="request">The request to send.</param>
        /// <param name="parseErrorResponse">Is Parse error response.</param>
        /// <param name="retryCount">The number of retry attempts.</param>
        /// <returns>The response for the request.</returns>
        private async Task<HttpResponseMessage> SendBatchAsync(
            HttpRequestMessage request,
            bool parseErrorResponse = true)
        {
            int retryCount = 0;
            while (true)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                var clonedRequest = request.CloneBatchRequest();
#pragma warning restore CS0618 // Type or member is obsolete

                HttpResponseMessage response = await this.httpClient.SendAsync(clonedRequest).ConfigureAwait(false);
                if (!response.IsSuccessStatusCode)
                {
                    // Handle exception & retry mechanism for API rate limit, Bad Gateway, Service not available
                    if ((int)response.StatusCode != Constant.TooManyRequest &&
                        (int)response.StatusCode != Constant.RequestTimeout &&
                        (int)response.StatusCode != Constant.BadGateway &&
                        (int)response.StatusCode != Constant.ServiceUnavailable &&
                        (int)response.StatusCode != Constant.GatewayTimeout)
                    {
                        // Not a service protection limit error
                        if (parseErrorResponse)
                        {
                            throw new HttpRequestException($"Error in processing dynamics request. Error Message: {response.ReasonPhrase}; Status Code: {response.StatusCode}; Content: {response.Content.ReadAsStringAsync().GetAwaiter().GetResult()}");
                        }
                        else
                        {
                            return response;
                        }
                    }
                    else
                    {
                        // Give up re-trying if exceeding the maxRetries
                        if (++retryCount >= Constant.DynamicsApiMaxRetries)
                        {
                            throw new HttpRequestException($"Error in processing dynamics request. Error Message: {response.ReasonPhrase}; Status Code: {response.StatusCode}; Content: {response.Content.ReadAsStringAsync().GetAwaiter().GetResult()}");
                        }

                        // Try to use the Retry-After header value if it is returned.
                        if (response.Headers.Contains("Retry-After"))
                        {
#pragma warning disable CS8604 // Possible null reference argument.
                            int seconds = int.Parse(response.Headers.GetValues("Retry-After").FirstOrDefault(), CultureInfo.InvariantCulture);
#pragma warning restore CS8604 // Possible null reference argument.
                            this.logger.LogWarning($"Encountered API Rate limit. Retry after duration is {seconds}. Error message: {response.ReasonPhrase}; Status Code: {response.StatusCode}; Content: {response.Content.ReadAsStringAsync().GetAwaiter().GetResult()}");
                            if (seconds > 0)
                            {
                                Thread.Sleep(TimeSpan.FromSeconds(seconds));
                            }
                            else
                            {
                                // Use exponential backoff strategy
                                int minutes = (int)Math.Pow(2, retryCount - 1);
                                Thread.Sleep(TimeSpan.FromMinutes(minutes));
                            }
                        }
                        else
                        {
                            // Otherwise, use an exponential backoff strategy
                            int minutes = (int)Math.Pow(2, retryCount - 1);
                            this.logger.LogWarning($"Encountered API Rate limit. Retry after duration is {minutes * 60}. Error message: {response.ReasonPhrase}; Status Code: {response.StatusCode}; Content: {response.Content.ReadAsStringAsync().GetAwaiter().GetResult()}");
                            Thread.Sleep(TimeSpan.FromMinutes(minutes));
                        }
                    }
                }
                else
                {
                    return response;
                }
            }
        }

        /// <summary>
        /// Retrieve Fetch Xml Result First Set.
        /// </summary>
        /// <param name="entitySetName">entity Set Name.</param>
        /// <param name="fetchXml">fetch Xml.</param>
        /// <returns>fetch xml value.</returns>
        private JObject RetrieveFetchXmlResultFirstSet(string entitySetName, string fetchXml)
        {
            JObject? collection = null;

            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
            using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, entitySetName + "?fetchXml=" + WebUtility.UrlEncode(fetchXml)))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", this.AuthToken);
                //// request.Headers.Add("Prefer", "odata.maxpagesize=5000");
                ////request.Headers.Add("Prefer", "odata.include-annotations=*");

#pragma warning disable CS0618 // Type or member is obsolete
                Task<HttpResponseMessage> taskResponse = this.SendAsync(request);
#pragma warning restore CS0618 // Type or member is obsolete
                HttpResponseMessage response = taskResponse.Result;
                ////200
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    collection = JsonConvert.DeserializeObject<JObject>(response.Content.ReadAsStringAsync().Result);
                }
                else
                {
                    HttpRequestException exception = new HttpRequestException("Retrieve Fetch Xml Result First Set -- FAIL -- " + Convert.ToString(response.ReasonPhrase, CultureInfo.InvariantCulture));
                    throw exception;
                }

                return collection;
            }
        }

        /// <summary>
        /// Deserialize To HTTP Response Message.
        /// </summary>
        /// <param name="stream">stream object.</param>
        /// <returns>return HTTP Response Message.</returns>
        private static HttpResponseMessage DeserializeToResponse(Stream stream)
        {
            HttpResponseMessage response = new HttpResponseMessage();
            MemoryStream memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);
            response.Content = new ByteArrayContent(memoryStream.ToArray());
            response.Content.Headers.Add("Content-Type", "application/http;msgtype=response");
            return response.Content.ReadAsHttpResponseMessageAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Execute Batch.
        /// </summary>
        /// <param name="multipartContent">Multi Part Content.</param>
        /// <returns>return HTTP content.</returns>
        private HttpContent ExecuteBatch(MultipartContent multipartContent)
        {
            HttpContent? responseContent = null;
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
            using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, DynamicsConnectionService.CrmAPIResource + "$batch"))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", this.AuthToken);
                request.Content = multipartContent;
                HttpResponseMessage response = this.SendBatchAsync(request).Result;
                responseContent = response.Content;
                return responseContent;
            }
        }

        /// <summary>
        /// Execute Batch request in Dynamics.
        /// </summary>
        /// <param name="multipartContent">multi part Content.</param>
        /// <returns>return HTTP Response Message.</returns>
        private async Task<List<HttpResponseMessage>> ExecuteGetBatchInDynamics(MultipartContent multipartContent)
        {
            this.logger.LogTrace("Enter into batch in dynamics");
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
            using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, DynamicsConnectionService.CrmAPIResource + "$batch"))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", this.AuthToken);
                request.Content = multipartContent;
                HttpResponseMessage response = await this.SendBatchAsync(request).ConfigureAwait(false);
                var responses = await ReadHttpContentsWithRequestBody(response).ConfigureAwait(false);
                return responses;
            }
        }

        private async Task<HttpResponseMessage> ExecutePatchBatchInDynamics(MultipartContent multipartContent)
        {
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
            using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, DynamicsConnectionService.CrmAPIResource + "$batch"))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", this.AuthToken);
                request.Content = multipartContent;
                HttpResponseMessage response = await this.SendBatchAsync(request).ConfigureAwait(false);
                return response;
            }
        }
    }
}
