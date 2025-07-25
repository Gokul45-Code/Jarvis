// <copyright file="InboundAddAttachment.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SiebelDB_001_Jarvis
{
    using System;
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
    /// Inbound Add Attachment.
    /// </summary>
    public class InboundAddAttachment
    {
        private readonly IConfiguration config;
        private readonly IDynamicsApiClient dynamicsClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="InboundAddAttachment"/> class.
        /// </summary>
        /// <param name="config">Configuration.</param>
        /// <param name="dynamicsClient">Dynamics Client.</param>
        public InboundAddAttachment(IConfiguration config, IDynamicsApiClient dynamicsClient)
        {
            this.config = config;
            this.dynamicsClient = dynamicsClient;
        }

        /// <summary>
        /// Azure Function Run Function.
        /// </summary>
        /// <param name="myQueueItem">Queue Name.</param>
        /// <param name="log">logger.</param>
        /// <exception cref="ArgumentException">Argument Exception.</exception>
        /// <returns>representing the asynchronous operation.</returns>
        [FunctionName("inboundattachin")]
        [ExponentialBackoffRetry(3, "00:00:30", "00:02:00")]
        public async Task Run([ServiceBusTrigger("%InboundAddAttachmentQueue%", Connection = "ServiceBusConnection")] string myQueueItem, ILogger log)
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

                if (originalJson["SiebelMessage"] != null && originalJson["SiebelMessage"]["ListOfServiceRequestBreakdownArgus"] != null && transType.ToUpper() == "Mercurius.Event.AddAttachment".ToUpper())
                {
                    log.LogInformation($"AddAttachment: {originalJson["SiebelMessage"]["@TransType"].ToString().ToUpper()}");
                    await this.AddAttachment(log, logger, originalJson);
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

        private async Task AddAttachment(ILogger log, ILoggerService logger, JObject originalJson)
        {
            JObject payLoad = (JObject)originalJson["SiebelMessage"]["ListOfServiceRequestBreakdownArgus"]["ServiceRequestBdArgus"];
            string caseNumberArgus = payLoad["CaseNumberARGUS"].ToString();
            string caseNumberJarvis = payLoad["CaseNumberJARVIS"]?.ToString();
            var attachmentPayload = payLoad["ListOfServiceRequestBdAttachmentArgus"]["ServiceRequestBdAttachmentArgus"];
            if (attachmentPayload.HasValues)
            {
                log.LogInformation("payload is ready");
                this.dynamicsClient.SetLoggingReference(logger);
                log.LogInformation("setting logger info in dynamicsclient");

                AddAttachmentInbound addAttachment = new AddAttachmentInbound(this.dynamicsClient, logger);
                log.LogInformation("Add Attachment inbound");
                var result = await addAttachment.IntegrationProcessAsync(JObject.Parse(attachmentPayload.ToString()), caseNumberArgus, caseNumberJarvis);
                log.LogInformation("triggered the method.");
                if (result.IsSuccessStatusCode)
                {
                    log.LogInformation($"Message body : {result.StatusCode}, Message Content:{result.Content.ReadAsStringAsync().GetAwaiter().GetResult()}");
                }
                else
                {
                    throw new ArgumentException($"Add Attachment: Failed in adding Attachment with case number " + caseNumberArgus);
                }
            }
            else
            {
                throw new ArgumentException($"Add Attachment: Failed in adding Attachment with case number " + caseNumberArgus);
            }
        }
    }
}
