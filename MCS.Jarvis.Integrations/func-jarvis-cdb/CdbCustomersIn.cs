// <copyright file="CdbCustomersIn.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Func_jarvis_cdb
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using Azure.Messaging.EventGrid;
    using Azure.Messaging.EventGrid.SystemEvents;
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
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// CdbCustomerIn.
    /// </summary>
    public class CdbCustomersIn
    {
        /// <summary>
        /// application settings configuration.
        /// </summary>
        private readonly IConfiguration config;

        /// <summary>
        /// dynamics application client.
        /// </summary>
        private readonly IDynamicsApiClient dynamicsClient;
        private readonly string azureServicebusConnectionstring;
        private readonly string queueName;
        ////private IQueueClient sbClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="CdbCustomersIn"/> class.
        /// DealersIn - Constructor.
        /// </summary>
        /// <param name="config">Config.</param>
        /// <param name="dynamicsClient">dynamicsClient.</param>
        public CdbCustomersIn(IConfiguration config, IDynamicsApiClient dynamicsClient)
        {
            this.config = config;
            this.dynamicsClient = dynamicsClient;
            this.azureServicebusConnectionstring = this.config.GetSection("ServiceBusConnection").Value;
            this.queueName = this.config.GetSection("CdbCustomerQueue").Value;
        }

        /// <summary>
        /// inboundupsertcustomer.
        /// </summary>
        /// <param name="req">req.</param>
        /// <param name="log">log.</param>
        /// <returns>ActionResult.</returns>
        [FunctionName("inboundupsertcustomer")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            try
            {
                log.LogInformation("UpsertCustomersIn Function triggered");
                ILoggerService logger = new LoggerService(log);
                string requestBody = string.Empty;

                BinaryData events = await BinaryData.FromStreamAsync(req.Body);
                log.LogInformation($"Received events: {events}");
                EventGridEvent[] eventGridEvents = EventGridEvent.ParseMany(events);
                foreach (EventGridEvent eventGridEvent in eventGridEvents)
                {
                    ////event grid validation
                    if (eventGridEvent.TryGetSystemEventData(out object eventData))
                    {
                        if (eventData is SubscriptionValidationEventData subscriptionValidationEventData)
                        {
                            log.LogInformation($"Got SubscriptionValidation event data, validation code: {subscriptionValidationEventData.ValidationCode}, topic: {eventGridEvent.Topic}");
                            var responseData = new
                            {
                                ValidationResponse = subscriptionValidationEventData.ValidationCode,
                            };

                            return new OkObjectResult(responseData);
                        }
                    }

                    var eventType = this.config.GetSection("EventType").Value;
                    if (!string.IsNullOrEmpty(eventType) && !string.IsNullOrWhiteSpace(eventType))
                    {
                        List<string> eventTypes = eventType.Split(",").ToList();
                        if (eventTypes.Any(item => item.ToString().ToUpper() == eventGridEvent.EventType.ToUpper()))
                        {
                            requestBody = eventGridEvent.Data.ToString();
                            log.LogInformation($"{requestBody} && {requestBody.GetType()},{string.IsNullOrEmpty(requestBody)}");
                            if (!string.IsNullOrEmpty(requestBody))
                            {
                                JObject originalJson = JObject.Parse(requestBody);
                                ServiceBusClient sbClient = new ServiceBusClient(this.azureServicebusConnectionstring);
                                ServiceBusSender sender = sbClient.CreateSender(this.queueName);
                                ServiceBusMessage message = new ServiceBusMessage(Encoding.UTF8.GetBytes(originalJson.ToString()))
                                {
                                    MessageId = Guid.NewGuid().ToString(),
                                    ContentType = "application/json",
                                };
                                await sender.SendMessageAsync(message);
                                ////this.sbClient = new QueueClient(this.azureServicebusConnectionstring, this.queueName);
                                ////var orderMessage = new Message(Encoding.UTF8.GetBytes(originalJson.ToString()))
                                ////{
                                ////    MessageId = Guid.NewGuid().ToString(),
                                ////    ContentType = "application/json",
                                ////};
                                ////await this.sbClient.SendAsync(orderMessage);
                                log.LogInformation($"Successfully send data to servicebus queue " + this.queueName + " with Data " + originalJson);
                                return new OkObjectResult("Request body has been processed successfully.");
                            }

                            log.LogInformation("CDB Integration: No request Body from CDB");
                            throw new ArgumentException("CDB Integration: No request Body from CDB");
                        }
                        else
                        {
                            log.LogInformation("Event Type is not matched/subscribed for CDB integration");
                            throw new ArgumentException("Event Type is not matched/subscribed for CDB integration");
                        }
                    }
                    else
                    {
                        log.LogInformation("Even Type data is not configured in App Settings, Please provide valid evenType with ',' seperate if its having multiple values.");
                        throw new ArgumentException("Even Type data is not configured in App Settings, Please provide valid evenType with ',' seperate if its having multiple values.");
                    }
                }

                log.LogInformation("CDB Integration: No request Body from CDB");
                throw new ArgumentException("CDB Integration: No request Body from CDB");
            }
            catch (Exception ex)
            {
                log.LogError($"CDB Integration :{ex.Message}");
                return new BadRequestObjectResult(ex.Message);
            }
        }
    }
}
