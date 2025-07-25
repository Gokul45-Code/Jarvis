// <copyright file="BaseWebClient.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MCS.Jarvis.CE.Plugins
{
    using System;
    using System.Net;
    using Microsoft.Xrm.Sdk;

    /// <summary>
    /// Base WebClient.
    /// </summary>
    public class BaseWebClient
    {
        /// <summary>
        /// Upload string.
        /// </summary>
        /// <param name="location">location details.</param>
        /// <param name="jsonObject">object.</param>
        /// <param name="tracingService">tracing service.</param>
        public void UploadString(Uri location, string jsonObject, ITracingService tracingService)
        {
            using (var client = new FunctionWebClient())
            {
                tracingService.Trace("Before: UploadStringAsync");
                client.Headers[HttpRequestHeader.ContentType] = "application/json";
                client.UploadStringAsync(location, jsonObject);
            }
        }
    }

    /// <summary>
    /// Function Web Client.
    /// </summary>
    public class FunctionWebClient : WebClient
    {
        /// <summary>
        /// Overrides the GetWebRequest method and sets keep alive to false.
        /// </summary>
        /// <param name="address">address details.</param>
        /// <returns>Override method.</returns>
        protected override WebRequest GetWebRequest(Uri address)
        {
            HttpWebRequest req = (HttpWebRequest)base.GetWebRequest(address);
            req.KeepAlive = false;

            return req;
        }
    }
}
