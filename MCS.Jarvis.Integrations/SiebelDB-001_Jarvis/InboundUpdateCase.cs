// <copyright file="InboundUpdateCase.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SiebelDB_001_Jarvis
{
    using System;
    using System.Globalization;
    using System.Threading.Tasks;
    using System.Xml;
    using IntegrationProcess;
    using MCS.Jarvis.Integration.Base.Dynamics;
    using MCS.Jarvis.Integration.Base.Logging;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Inbound update case class.
    /// </summary>
    public class InboundUpdateCase
    {
        private readonly IConfiguration config;
        private readonly IDynamicsApiClient dynamicsClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="InboundUpdateCase"/> class.
        /// </summary>
        /// <param name="config">Configuration.</param>
        /// <param name="dynamicsClient">Dynamics Client.</param>
        public InboundUpdateCase(IConfiguration config, IDynamicsApiClient dynamicsClient)
        {
            this.config = config;
            this.dynamicsClient = dynamicsClient;
        }

        /// <summary>
        /// Azure function Run method.
        /// </summary>
        /// <param name="myQueueItem">Queue Name.</param>
        /// <param name="log">logger.</param>
        /// <exception cref="ArgumentException">Argument Exception.</exception>
        /// <returns>representing the asynchronous operation.</returns>
        [FunctionName("inboundupdatecase")]
        [ExponentialBackoffRetry(3, "00:00:30", "00:02:00")]
        public async Task Run([ServiceBusTrigger("%InboundUpdateCaseQueue%", Connection = "ServiceBusConnection")] string myQueueItem, ILogger log)
        {
            log.LogInformation($"C# ServiceBus queue trigger function processed message: {myQueueItem}");
            if (!string.IsNullOrEmpty(myQueueItem))
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(myQueueItem);
                ILoggerService logger = new LoggerService(log);
                string jsonData = JsonConvert.SerializeXmlNode(doc.GetElementsByTagName("SiebelMessage").Item(0));
                log.LogInformation($"C# ServiceBus queue trigger function processed message: {jsonData}");
                JObject originalJson = JObject.Parse(jsonData);
                log.LogInformation("parse into jobject..");
                string transType = originalJson["SiebelMessage"]["@TransType"].ToString();
                if (originalJson["SiebelMessage"] != null && originalJson["SiebelMessage"]["ListOfServiceRequestBreakdownArgus"] != null && transType.ToUpper() == "Mercurius.Event.UpdateCase".ToUpper())
                {
                    if (originalJson.SelectToken("SiebelMessage.@EventTimestamp") == null || string.IsNullOrEmpty(originalJson.SelectToken("SiebelMessage.@EventTimestamp").ToString()))
                    {
                        log.LogError("EventTimestamp is a mandatory field.");
                        throw new ArgumentException("EventTimestamp is a mandatory field.");
                    }

                    log.LogInformation($"isUpdate: {originalJson["SiebelMessage"]["@TransType"].ToString().ToUpper()}");
                    await this.UpdateCase(log, logger, originalJson);
                }
                else
                {
                    throw new ArgumentException("SIEBEL Integration: Not a valid request Body from SIEBEL " + transType);
                }
            }
            else
            {
                throw new ArgumentException("SIEBEL Integration: No request Body from SIEBEL");
            }
        }

        private async Task UpdateCase(ILogger log, ILoggerService logger, JObject originalJson)
        {
            DateTime eventTimestamp = DateTime.ParseExact(originalJson.SelectToken("SiebelMessage.@EventTimestamp").ToString(), "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
            JObject payLoad = (JObject)originalJson["SiebelMessage"]["ListOfServiceRequestBreakdownArgus"]["ServiceRequestBdArgus"];
            log.LogInformation("payload is ready");
            this.dynamicsClient.SetLoggingReference(logger);
            log.LogInformation("setting logger info in dynamicsclient");
            if (payLoad != null && !string.IsNullOrEmpty(eventTimestamp.ToString()) && payLoad.ContainsKey("ForcedClosedARGUS") && payLoad["ForcedClosedARGUS"].ToString().ToUpper() == "Y")
            {
                ForceCloseCaseInbound casesIn = new ForceCloseCaseInbound(this.dynamicsClient, logger);
                log.LogInformation("Created the upsertCasein");
                var result = await casesIn.IntegrationProcessAsync(payLoad);
                log.LogInformation("triggered the method.");
                if (result.IsSuccessStatusCode)
                {
                    log.LogInformation($"Message body : {result.StatusCode}, Message Content:{result.Content.ReadAsStringAsync().GetAwaiter().GetResult()}");
                }
                else
                {
                    throw new ArgumentException($"Update case: Failed to Force Close Case.");
                }
            }
            else if (payLoad != null && !string.IsNullOrEmpty(eventTimestamp.ToString()))
            {
                UpdateCaseInbound casesIn = new UpdateCaseInbound(this.dynamicsClient, logger);
                log.LogInformation("Created the upsertCasein");
                var result = await casesIn.IntegrationProcessAsync(payLoad, eventTimestamp);
                log.LogInformation("triggered the method.");
                if (result.IsSuccessStatusCode)
                {
                    log.LogInformation($"Message body : {result.StatusCode}, Message Content:{result.Content.ReadAsStringAsync().GetAwaiter().GetResult()}");
                }
                else
                {
                    throw new ArgumentException($"Update case: Failed in updating case.");
                }
            }
            else
            {
                throw new ArgumentException("Invalid Payload or EventTimestamp.");
            }
        }
    }
}
