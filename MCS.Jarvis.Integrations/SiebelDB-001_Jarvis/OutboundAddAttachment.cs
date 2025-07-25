// <copyright file="OutboundAddAttachment.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SiebelDB_CasePassOutbound
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using Azure.Messaging.ServiceBus;
    using IntegrationProcess;
    using IntegrationProcess.Helper.Constants;
    using MCS.Jarvis.Integration.Base.Dynamics;
    using MCS.Jarvis.Integration.Base.Logging;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    /// <summary>
    /// OutboundAddAttachment Azure Function Class.
    /// </summary>
    public class OutboundAddAttachment
    {
        private readonly IConfiguration config;
        private readonly IDynamicsApiClient dynamicsClient;
        private readonly string azureServicebusConnectionstring;
        private readonly string queueName;

        /// <summary>
        /// Initializes a new instance of the <see cref="OutboundAddAttachment"/> class.
        /// </summary>
        /// <param name="config">Configuration.</param>
        /// <param name="dynamicsClient">Dynamics client.</param>
        public OutboundAddAttachment(IConfiguration config, IDynamicsApiClient dynamicsClient)
        {
            this.config = config;
            this.dynamicsClient = dynamicsClient;
            this.azureServicebusConnectionstring = this.config.GetSection("ServiceBusConnection").Value;
            this.queueName = this.config.GetSection("OutboundAddAttachmentQueue").Value;
        }

        /// <summary>
        /// OutboundAddAttachment Azure Function.
        /// </summary>
        /// <param name="req">Request.</param>
        /// <param name="log">logger.</param>
        /// <returns>Task.</returns>
        /// <exception cref="ArgumentException">Argument Exception.</exception>
        [FunctionName("outboundattachout")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            string id = data?.Id;
            string transType = data?.TransType;
            string entityData = data?.EntityData;

            #region AddAttachment
            if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(transType))
            {
                if (transType.ToUpper() == TransType.AttachmentOutbound)
                {
                    return await this.AttachmentActionResult(log, id, entityData, transType);
                }
                else
                {
                    return new NotFoundObjectResult($"TransType {transType} is not Implemented" + this.queueName);
                }
            }
            else
            {
                log.LogError($"Value for Required Field from configuraiton is missing in the paylaod : Id or Transtype");
                throw new ArgumentException($"Value for Required Field from configuraiton is missing in the paylaod : Id or Transtype");
            }
            #endregion
        }

        private async Task<IActionResult> AttachmentActionResult(ILogger log, string attachmentId, string entityData, string transType)
        {
            #region AttachmentAction
            ILoggerService logger = new LoggerService(log);
            log.LogInformation($"Add Attachment and transType received in request is " + attachmentId + " " + transType);
            this.dynamicsClient.SetLoggingReference(logger);
            AddAttachmentOutbound addAttachmentAction = new AddAttachmentOutbound(this.dynamicsClient, this.config, logger);
            var response = await addAttachmentAction.IntegrationProcessAsync(attachmentId, entityData);
            var xmlPayload = JsonConvert.DeserializeXNode(response.ToString(), "ServiceRequestBdArgus");
            var xmlData =
            new XElement(
                "SiebelMessage",
                new XAttribute("MessageId", attachmentId),
                new XAttribute("EventTimestamp", DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture)),
                new XAttribute("TransType", transType),
                new XAttribute("MessageType", "Integration Object"),
                new XAttribute("IntObjectName", "Mercurius JARVIS Case Attachment Info ARGUS"),
                new XAttribute("IntObjectFormat", "Siebel Hierarchical"),
                new XElement("ListOfServiceRequestBreakdownArgus", xmlPayload.Descendants("ServiceRequestBdArgus")));
            ServiceBusClient sbClient = new ServiceBusClient(this.azureServicebusConnectionstring);
            ServiceBusSender sender = sbClient.CreateSender(this.queueName);
            ServiceBusMessage message = new ServiceBusMessage(Encoding.UTF8.GetBytes(xmlData.ToString()))
            {
                MessageId = Guid.NewGuid().ToString(),
                ContentType = "application/xml",
                Subject = transType,
            };
            await sender.SendMessageAsync(message);
            ////IQueueClient sbClient = new QueueClient(connectionString: this.azureServicebusConnectionstring, this.queueName);
            ////var orderMessage = new Message(Encoding.UTF8.GetBytes(xmlData.ToString()))
            ////{
            ////    MessageId = Guid.NewGuid().ToString(),
            ////    ContentType = "application/xml",
            ////    Label = transType,
            ////};
            ////await sbClient.SendAsync(orderMessage);
            log.LogInformation($"Successfully send data to servicebus queue " + this.queueName + " with Data " + xmlData);
            return new OkObjectResult($"Successfully send data to servicebus queue " + this.queueName);
            #endregion
        }
    }
}
