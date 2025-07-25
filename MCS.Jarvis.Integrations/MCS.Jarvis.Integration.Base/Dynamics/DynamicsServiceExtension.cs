namespace MCS.Jarvis.Integration.Base.Dynamics
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;

    /// <summary>
    /// Dynamics Service Extension.
    /// </summary>
    public static class DynamicsServiceExtension
    {
        /// <summary>
        /// Clones a HttpRequestMessage instance.
        /// </summary>
        /// <param name="request">The HttpRequestMessage to clone.</param>
        /// <returns>A copy of the HttpRequestMessage.</returns>
        [Obsolete("Use newMethod instead", false)]
        public static HttpRequestMessage Clone(this HttpRequestMessage request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request", "Null Argument");
            }

            var clone = new HttpRequestMessage(request.Method, request.RequestUri)
            {
                Content = request.Content?.Clone(),
                Version = request.Version,
            };
            foreach (KeyValuePair<string, object?> prop in request.Options)
            {
                clone.Properties.Add(prop);
            }

            foreach (KeyValuePair<string, IEnumerable<string>> header in request.Headers)
            {
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            return clone;
        }

        /// <summary>
        /// Clones a HttpContent instance.
        /// </summary>
        /// <param name="content">The HttpContent to clone.</param>
        /// <returns>A copy of the HttpContent.</returns>
        public static HttpContent? Clone(this HttpContent content)
        {
            if (content == null)
            {
                return null;
            }

            HttpContent clone;

            switch (content)
            {
                case StringContent sc:
                    clone = new StringContent(sc.ReadAsStringAsync().Result);
                    break;
                default:
                    HttpRequestException exception = new HttpRequestException($"{content.GetType()} Content type not implemented for HttpContent.Clone extension method.");
                    throw exception;
            }

            clone.Headers.Clear();
            foreach (KeyValuePair<string, IEnumerable<string>> header in content.Headers)
            {
                clone.Headers.Add(header.Key, header.Value);
            }

            return clone;
        }

        /// <summary>
        /// Clone Batch Request.
        /// </summary>
        /// <param name="request">HTTP Request Message.</param>
        /// <returns>return cloned HTTP Request Message.</returns>
        [Obsolete("Use newMethod instead", false)]
        public static HttpRequestMessage CloneBatchRequest(this HttpRequestMessage request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request", "Null Argument");
            }

            var clone = new HttpRequestMessage(request.Method, request.RequestUri)
            {
                Content = request.Content,
                Version = request.Version,
            };
            foreach (KeyValuePair<string, object?> prop in request.Properties)
            {
                clone.Properties.Add(prop);
            }

            foreach (KeyValuePair<string, IEnumerable<string>> header in request.Headers)
            {
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            return clone;
        }
    }
}
