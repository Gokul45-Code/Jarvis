// <copyright file="func_jarvis_volvopay.cs" company="Microsoft.">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace func_jarvis_volvopay
{
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
    using System;
    using System.Globalization;
    using System.IO;
    using System.Threading.Tasks;

    public class CapturePaymentStatusChange
    {
        private readonly IConfiguration config;
        private readonly IDynamicsApiClient dynamicsClient;

        public CapturePaymentStatusChange(IConfiguration config, IDynamicsApiClient dynamicsClient)
        {
            this.config = config;
            this.dynamicsClient = dynamicsClient;
        }

        [FunctionName("CapturePaymentStatusChange")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            string volvoPayEventType = data?.eventType;
            string paymentRequestId = data?.payload?.paymentRequestId;
            string volvoPayEventOn = DateTime.UtcNow.ToString("MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture);

            if (!string.IsNullOrEmpty(paymentRequestId) && !string.IsNullOrEmpty(volvoPayEventType))
            {
                ILoggerService logger = new LoggerService(log);
                log.LogInformation($"Recieved VolvoPay PaymentStatusChange " + volvoPayEventType + " " + paymentRequestId + " " + volvoPayEventOn);
                this.dynamicsClient.SetLoggingReference(logger);
                VolvoPayInbound volvoPayInbound = new VolvoPayInbound(this.dynamicsClient, this.config, logger);
                var gopId = await volvoPayInbound.IntegrationProcessAsync(paymentRequestId, volvoPayEventType, volvoPayEventOn);
                log.LogInformation($"Successfully updated the gop record: {gopId}");
            }
            else
            {
                return new NotFoundObjectResult("Payload does not contain either the paymentRequestId or volvoPayEventType");
            }

            // Return a response to acknowledge the webhook request
            return new OkObjectResult("Hello, This HTTP triggered func_jarvis_volvopay-CapturePaymentStatusChange executed successfully.");
        }
    }
}
