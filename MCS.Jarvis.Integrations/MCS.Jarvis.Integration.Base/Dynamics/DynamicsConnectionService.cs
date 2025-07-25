namespace MCS.Jarvis.Integration.Base.Dynamics
{
    using System.Globalization;
    using System.Net;
    using System.Net.Http.Headers;
   //// using Microsoft.Azure.Services.AppAuthentication;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public static class DynamicsConnectionService
    {
        /// <summary>
        /// Gets or sets dynamics API URL.
        /// </summary>
        public static string? CrmAPIResource { get; set; }

        /// <summary>
        /// Get dynamics CRM access token.
        /// </summary>
        /// <param name="config">application settings configuration.</param>
        /// <returns>return access token.</returns>
        public static IDictionary<string, DateTime> GetDynamicsTokenUsingClientCredentials(IConfiguration config, IHttpClientFactory httpClientFactory)
        {
            if (config == null)
            {
                throw new ArgumentException("Null Configuration Argument", nameof(config));
            }

            string resourceUrl = config.GetSection("DynamicsConfig:CrmResourceUrl").Value;
            string tokenBaseUrl = config.GetSection("DynamicsConfig:CrmTokenBaseUrl").Value;
            string clientIdValue = config.GetSection("DynamicsConfig:CrmClientId").Value;
            string clientSecretValue = config.GetSection("DynamicsConfig:CrmClientSecret").Value;
            string tenantId = config.GetSection("DynamicsConfig:TenantId").Value; var clientIdList = clientIdValue.Split(';').ToList();
            var secretList = clientSecretValue.Split(';').ToList();
            if (clientIdList.Count != secretList.Count)
            {
                throw new ArgumentException("Client Id and secrets are not matching");
            }

            int counter = 0;
            IDictionary<string, DateTime> authenticationToken = new Dictionary<string, DateTime>();
            foreach (var clientId in clientIdList)
            {
                var clientSecret = secretList[counter++];
                using (HttpClientHandler httpHandler = new HttpClientHandler() { DefaultProxyCredentials = CredentialCache.DefaultCredentials })
                {
                    using (var client = httpClientFactory.CreateClient())
                    {
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                        client.BaseAddress = new Uri(string.Format(CultureInfo.InvariantCulture, tokenBaseUrl, tenantId)); // We want the response to be JSON.
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json")); // Build up the data to POST.
                        List<KeyValuePair<string, string>> postData = new List<KeyValuePair<string, string>>();
                        postData.Add(new KeyValuePair<string, string>("grant_type", "client_credentials"));
                        postData.Add(new KeyValuePair<string, string>("client_id", clientId));
                        postData.Add(new KeyValuePair<string, string>("client_secret", clientSecret));
                        postData.Add(new KeyValuePair<string, string>("resource", resourceUrl));
                        using (FormUrlEncodedContent content = new FormUrlEncodedContent(postData))
                        {
                            // Post to the Server and parse the response.
                            HttpResponseMessage response = client.PostAsync("Token", content).Result;
                            if (!response.IsSuccessStatusCode)
                            {
                                throw new HttpRequestException($"Error in Generating dynamics token. Error Message: {response.ReasonPhrase}; Status Code: {response.StatusCode}; Content: {response.Content.ReadAsStringAsync().GetAwaiter().GetResult()}");
                            }

                            JObject obj = JsonConvert.DeserializeObject<JObject>(response.Content.ReadAsStringAsync().Result);
                            string accessToken = (string)obj["access_token"];
                            int tokenExpiresIn = Convert.ToInt32(obj["expires_in"], CultureInfo.InvariantCulture);
                            DateTime tokenExpiredOn = DateTime.UtcNow.AddSeconds(tokenExpiresIn);
                            if (authenticationToken.ContainsKey(accessToken))
                            {
                                authenticationToken[accessToken] = tokenExpiredOn;
                            }
                            else
                            {
                                authenticationToken.Add(accessToken, tokenExpiredOn);
                            }
                        }
                    }
                }
            }

            return authenticationToken;
        }

        /// <summary>
        /// Get dynamics token using managed identity.We are not using managed Indentity.
        /// </summary>
        /// <param name="config">configuration parameter.</param>
        /// <returns>return access token.</returns>
        ////public static IDictionary<string, DateTime> GetDynamicsTokenUsingManagedIdentity(IConfiguration config)
        ////{
        ////    if (config == null)
        ////    {
        ////        throw new ArgumentException("Null Configuration Argument", nameof(config));
        ////    }

        ////    IDictionary<string, DateTime> authenticationToken = new Dictionary<string, DateTime>();
        ////    string resourceUrl = config.GetSection("DynamicsConfig:CrmResourceUrl").Value;
        ////    var azureServiceTokenProvider = new AzureServiceTokenProvider();
        ////    var authresult = azureServiceTokenProvider.GetAuthenticationResultAsync(resourceUrl).Result;
        ////    string accessToken = authresult.AccessToken;
        ////    DateTime tokenExpiredOn = authresult.ExpiresOn.UtcDateTime;
        ////    if (authenticationToken.ContainsKey(accessToken))
        ////    {
        ////        authenticationToken[accessToken] = tokenExpiredOn;
        ////    }
        ////    else
        ////    {
        ////        authenticationToken.Add(accessToken, tokenExpiredOn);
        ////    }

        ////    return authenticationToken;
        ////}
    }
}
