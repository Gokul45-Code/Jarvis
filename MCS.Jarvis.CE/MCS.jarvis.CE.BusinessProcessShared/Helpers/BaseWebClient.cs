// <copyright file="BaseWebClient.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace MCS.Jarvis.CE.BusinessProcessesShared.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.Xrm.Sdk;
    using Newtonsoft.Json.Linq;
    using Polly;
    using static MCS.Jarvis.CE.BusinessProcessesShared.Helpers.Constants;

    /// <summary>
    /// Base Web Client.
    /// </summary>
    public class BaseWebClient
    {
        /// <summary>
        /// Upload String.
        /// </summary>
        /// <param name="location">location value.</param>
        /// <param name="jsonObject">json Object.</param>
        /// <param name="tracingService">tracing service.</param>
        public void UploadString(Uri location, string jsonObject, ITracingService tracingService)
        {
            using (var client = new FunctionWebClient())
            {
                tracingService.Trace("Before: UploadStringAsync");
                client.Headers[HttpRequestHeader.ContentType] = "application/json";
                ////client.Headers[HttpRequestHeader.Authorization] = GetAccessToken("", "", "", "", tracingService);
                client.UploadStringAsync(location, jsonObject);
            }
        }

        /// <summary>
        /// Upload String.
        /// </summary>
        /// <param name="location">location value.</param>
        /// <param name="jsonObject">json Object.</param>
        /// <param name="tracingService">tracing service.</param>
        /// <param name="secureString">secure String.</param>
        public async void UploadString(Uri location, string jsonObject, ITracingService tracingService, string secureString)
        {
            JObject authoVariables = JObject.Parse(secureString);
            var getTokenTask = Task.Run(async () => await GetToken(authoVariables["tokenUrl"].ToString(), authoVariables["tenantId"].ToString(), authoVariables["client_id"].ToString(), authoVariables["client_secret"].ToString(), authoVariables["scope"].ToString(), tracingService));
            await Task.WhenAll(getTokenTask);
            if (getTokenTask.Result == null)
            {
                return;
            }

            tracingService.Trace("Received token");
            var tokenResponse = JObject.Parse(getTokenTask.Result);
            string accessToken = tokenResponse["access_token"].ToString();

            using (var client = new FunctionWebClient())
            {
                tracingService.Trace("Before: UploadStringAsync");
                client.Headers[HttpRequestHeader.ContentType] = "application/json";
                client.Headers[HttpRequestHeader.Authorization] = "Bearer " + accessToken;
                client.Headers["api-key"] = authoVariables["api-key"].ToString();
                client.UploadStringAsync(location, jsonObject);
            }
        }

        /// <summary>
        /// Get Token.
        /// </summary>
        /// <param name="tokenurl">token url.</param>
        /// <param name="tenantId">tenant Id.</param>
        /// <param name="clientId">client Id.</param>
        /// <param name="clientSecret">client secret.</param>
        /// <param name="scope">scope param.</param>
        /// <param name="tracingService">tracing service.</param>
        /// <returns>Task returned.</returns>
        private static async Task<string> GetToken(string tokenurl, string tenantId, string clientId, string clientSecret, string scope, ITracingService tracingService)
        {
            var authorityUrl = string.Format(tokenurl, tenantId);
            using (HttpClient httpClient = new HttpClient())
            {
                var formContent = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("resource", clientId),
                    new KeyValuePair<string, string>("client_id", clientId),
                    new KeyValuePair<string, string>("client_secret", clientSecret),
                    new KeyValuePair<string, string>("scope", scope),
                    new KeyValuePair<string, string>("grant_type", "client_credentials"),
                });
                var response = await Policy.Handle<HttpRequestException>().WaitAndRetryAsync(
              retryCount: 2,
              sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
              onRetry: (exception, timeSpan, retryCount, context) =>
              {
                  tracingService.Trace("Retry: " + exception);
                  //// Log the retry attempt
              }).ExecuteAsync(() => httpClient.PostAsync(authorityUrl, formContent));
                ////  HttpResponseMessage response = await httpClient.PostAsync(tenantId, formContent);
                tracingService.Trace("Successfully created response");
                return !response.IsSuccessStatusCode ? null
                  : response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            }
        }
    }

    /// <summary>
    /// Function Web Client.
    /// </summary>
    public class FunctionWebClient : WebClient
    {
        /// <summary>
        /// Get Web Request.
        /// </summary>
        /// <param name="address">Address param.</param>
        /// <returns>Web Request.</returns>
        ////Overrides the Get Web Request method and sets keep alive to false
        protected override WebRequest GetWebRequest(Uri address)
        {
            HttpWebRequest req = (HttpWebRequest)base.GetWebRequest(address);
            req.KeepAlive = false;
            return req;
        }
    }
}