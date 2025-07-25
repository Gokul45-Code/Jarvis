// <copyright file="DealersIn.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Func_ctdi_001
{
    using System;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Azure.Messaging.EventGrid;
    using Azure.Messaging.EventGrid.SystemEvents;
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
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// DealersIn - Class for Upsert and Deactivate CTDI Integration.
    /// </summary>
    public class DealersIn
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
        /// Initializes a new instance of the <see cref="DealersIn"/> class.
        /// DealersIn - Constructor.
        /// </summary>
        /// <param name="config">config.</param>
        /// <param name="dynamicsClient">dynamicsClient.</param>
        public DealersIn(IConfiguration config, IDynamicsApiClient dynamicsClient)
        {
            this.config = config;
            this.dynamicsClient = dynamicsClient;
            this.azureServicebusConnectionstring = this.config.GetSection("ServiceBusConnection").Value;
            this.queueName = this.config.GetSection("CtdiDealerQueue").Value;
        }

        /// <summary>
        /// inboundupsertdealer Function.
        /// </summary>
        /// <param name="req">req.</param>
        /// <param name="log">log.</param>
        /// <returns>OkObject.</returns>
        [FunctionName("inboundupsertdealer")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            try
            {
                log.LogInformation("DealersIn Function triggered");
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
                    else if (eventGridEvent.EventType.ToUpper() == "Azure.TILAPI.Push.Event".ToUpper())
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

                        throw new ArgumentException("CTDI - No Request Body from CTDI Integration.");
                    }
                }

                throw new ArgumentException("CTDI Integration 2: No request Body from CTDI");
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex.Message);
            }
        }
    }
}
