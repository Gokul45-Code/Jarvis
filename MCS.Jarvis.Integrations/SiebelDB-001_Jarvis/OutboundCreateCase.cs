// <copyright file="OutboundCreateCase.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SiebelDB_001_Jarvis
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using Azure.Messaging.ServiceBus;
    using IntegrationProcess;
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
    /// OutboundCreateCase Azure Function Class.
    /// </summary>
    public class OutboundCreateCase
    {
        private readonly IConfiguration config;
        private readonly IDynamicsApiClient dynamicsClient;
        private readonly string azureServicebusConnectionstring;
        private readonly string queueName;
        ////private IQueueClient sbClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="OutboundCreateCase"/> class.
        /// </summary>
        /// <param name="config">Configuration.</param>
        /// <param name="dynamicsClient">Dynamics Client.</param>
        public OutboundCreateCase(IConfiguration config, IDynamicsApiClient dynamicsClient)
        {
            this.config = config;
            this.dynamicsClient = dynamicsClient;
            this.azureServicebusConnectionstring = this.config.GetSection("ServiceBusConnection").Value;
            this.queueName = this.config.GetSection("OutboundCreateCaseQueue").Value;
        }

        /// <summary>
        /// OutboundCreateCase Azure Function.
        /// </summary>
        /// <param name="req">Request.</param>
        /// <param name="log">Logger.</param>
        /// <returns>Task.</returns>
        /// <exception cref="ArgumentException">Argument Exception.</exception>
        [FunctionName("outboundcreatecase")]
        public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
        ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            ILoggerService logger = new LoggerService(log);
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            string incidentId = data?.Id;
            string transType = data?.TransType;
            string entityData = data?.EntityData;

            #region CreateCase
            if (!string.IsNullOrEmpty(incidentId) && !string.IsNullOrEmpty(transType))
            {
                log.LogInformation($"Incident Id received in request is" + incidentId);
                this.dynamicsClient.SetLoggingReference(logger);
                CreateCasesOut casesOut = new CreateCasesOut(this.dynamicsClient, this.config, logger);
                var response = await casesOut.IntegrationProcessAsync(incidentId, entityData);
                var xmlPayload = JsonConvert.DeserializeXNode(response.GetValue("sbMessage").ToString(), "ServiceRequestBdArgus");

                var xmlData =
                new XElement(
                    "SiebelMessage",
                    new XAttribute("MessageId", incidentId),
                    new XAttribute("EventTimestamp", DateTime.Parse(response.GetValue("modifiedOn").ToString()).ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture)),
                    new XAttribute("TransType", transType),
                    new XAttribute("MessageType", "Integration Object"),
                    new XAttribute("IntObjectName", "Mercurius Case Detail Info ARGUS"),
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
                ////this.sbClient = new QueueClient(this.azureServicebusConnectionstring, this.queueName);
                ////var orderMessage = new Message(Encoding.UTF8.GetBytes(xmlData.ToString()))
                ////{
                ////    MessageId = Guid.NewGuid().ToString(),
                ////    ContentType = "application/xml",
                ////    Label = transType,
                ////};
                ////await this.sbClient.SendAsync(orderMessage);
                log.LogInformation($"Successfully send data to servicebus queue " + this.queueName + " with Data " + xmlData);
                return new OkObjectResult($"Successfully send data to servicebus queue " + this.queueName);
            }
            else
            {
                logger.LogException(new Exception($"Value for Required Field from configuraiton is missing in the paylaod : IncidentId"));
                throw new ArgumentException($"Value for Required Field from configuraiton is missing in the paylaod : IncidentId");
            }
            #endregion
        }
    }
}
