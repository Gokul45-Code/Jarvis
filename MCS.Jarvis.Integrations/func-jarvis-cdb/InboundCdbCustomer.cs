// <copyright file="InboundCustomer.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Func_jarvis_cdb
{
    using System;
    using IntegrationProcess;
    using MCS.Jarvis.Integration.Base.Dynamics;
    using MCS.Jarvis.Integration.Base.Logging;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Inbound CDB Customer class.
    /// </summary>
    public class InboundCdbCustomer
    {
        private readonly IConfiguration config;
        private readonly IDynamicsApiClient dynamicsClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="InboundCdbCustomer"/> class.
        /// </summary>
        /// <param name="config">Configuration.</param>
        /// <param name="dynamicsClient">Dynamics Client.</param>
        public InboundCdbCustomer(IConfiguration config, IDynamicsApiClient dynamicsClient)
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
        [FunctionName("inboundcdbcustomer")]
        [ExponentialBackoffRetry(3, "00:00:30", "00:02:00")]
        public void Run([ServiceBusTrigger("%CdbCustomerQueue%", Connection = "ServiceBusConnection")] string myQueueItem, ILogger log)
        {
            try
            {
                log.LogInformation($"C# ServiceBus queue trigger function processed message: {myQueueItem}");
                ILoggerService logger = new LoggerService(log);
                if (!string.IsNullOrEmpty(myQueueItem))
                {
                    JObject originalJson = JObject.Parse(myQueueItem);
                    ////Upsert
                    if (originalJson["body"] != null && originalJson["header"] != null && originalJson["header"]["action"].ToString().ToUpper() == "UPDATE")
                    {
                        log.LogInformation($"isUpdate: {originalJson["header"]["action"].ToString().ToUpper()}");
                        if ((originalJson["header"]["creationTimestamp"] == null) || (originalJson["header"]["creationTimestamp"].ToString() == string.Empty))
                        {
                            throw new ArgumentException("CDB Integration: creationTimestamp is missing from the payload");
                        }

                        DateTime creationTimestamp = DateTime.Parse(originalJson["header"]["creationTimestamp"].ToString());
                        JObject payLoad = (JObject)originalJson["body"]["customer"];
                        this.dynamicsClient.SetLoggingReference(logger);
                        UpsertCustomersIn upsertCustomersIn = new UpsertCustomersIn(this.dynamicsClient, logger);
                        ////framed payload and passed creationtimestamp
                        var result = upsertCustomersIn.IntegrationProcessAsync(payLoad, creationTimestamp);
                        log.LogInformation("triggered the method.");
#pragma warning disable S6422 // Calls to "async" methods should not be blocking in Azure Functions
                        if (result.Result.IsSuccessStatusCode)
                        {
                            log.LogInformation($"Message body : {result.Result.StatusCode}, Message Content:{result.Result.Content.ReadAsStringAsync().GetAwaiter().GetResult()}");
                        }
                        else
                        {
                            log.LogError($"CDB Integration: Failed in updating Customer.");
                            throw new ArgumentException($"CDB Integration: Failed in updating Customer.");
                        }
                    }
                    else
                    {
                        log.LogError("CDB Integration: No request Body from CDB");
                        throw new ArgumentException("CDB Integration: No request Body from CDB");
                    }
                }
            }
            catch (Exception ex)
            {
                log.LogError($"CDB Integration :{ex.Message}");
                throw new ArgumentException(ex.Message);
            }
        }
    }
}
