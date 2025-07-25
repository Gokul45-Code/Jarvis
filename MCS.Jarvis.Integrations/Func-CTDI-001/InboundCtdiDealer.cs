

namespace Func_ctdi_001
{
    using System;
    using IntegrationProcess;
    using System.Net.Http;
    using MCS.Jarvis.Integration.Base.Dynamics;
    using MCS.Jarvis.Integration.Base.Logging;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Host;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// InboundCtdiDealer.
    /// </summary>
    public class InboundCtdiDealer
    {
        private readonly IConfiguration config;
        private readonly IDynamicsApiClient dynamicsClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="InboundCtdiDealer"/> class.
        /// </summary>
        /// <param name="config">Configuration.</param>
        /// <param name="dynamicsClient">Dynamics Client.</param>
        public InboundCtdiDealer(IConfiguration config, IDynamicsApiClient dynamicsClient)
        {
            this.config = config;
            this.dynamicsClient = dynamicsClient;
        }

        /// <summary>
        /// Azure Run.
        /// </summary>
        /// <param name="myQueueItem">CtdiDealerQueue.</param>
        /// <param name="log">Logger.</param>
        [FunctionName("InboundCtdiDealer")]
        [ExponentialBackoffRetry(3, "00:00:30", "00:02:00")]
        public void Run([ServiceBusTrigger("%CtdiDealerQueue%", Connection = "ServiceBusConnection")] string myQueueItem, ILogger log)
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
                            throw new ArgumentException("CTDI Integration: creationTimestamp is missing from the payload");
                        }

                        DateTime creationTimestamp = DateTime.Parse(originalJson["header"]["creationTimestamp"].ToString());
                        JObject payLoad = (JObject)originalJson["body"];
                        this.dynamicsClient.SetLoggingReference(logger);
                        UpsertDealersIn dealersIn = new UpsertDealersIn(this.dynamicsClient, logger);
                        ////framed payload and passed creationtimestamp
                        var result = dealersIn.IntegrationProcessAsync(payLoad, creationTimestamp);
                        log.LogInformation("triggered the method.");
#pragma warning disable S6422 // Calls to "async" methods should not be blocking in Azure Functions
                        if (result.Result.IsSuccessStatusCode)
                        {
                            log.LogInformation($"Message body : {result.Result.StatusCode}, Message Content:{result.Result.Content.ReadAsStringAsync().GetAwaiter().GetResult()}");
#pragma warning restore S6422 // Calls to "async" methods should not be blocking in Azure Functions
                        }
                        else
                        {
                            log.LogError($"CTDI Integration: Failed in updating Dealers.");
                            throw new ArgumentException($"CTDI Integration: Failed in updating Dealers.");
                        }
                    }
                    ////Deactivate
                    else if (originalJson["body"] != null && originalJson["header"] != null && originalJson["header"]["action"].ToString().ToUpper() == "DELETE")
                    {
                        log.LogInformation($"isUpdate: {originalJson["header"]["action"].ToString().ToUpper()}");
                        if ((originalJson["header"]["creationTimestamp"] == null) || (originalJson["header"]["creationTimestamp"].ToString() == string.Empty))
                        {
                            throw new ArgumentException("CTDI Integration: creationTimestamp is missing from the payload");
                        }

                        DateTime creationTimestamp = DateTime.Parse(originalJson["header"]["creationTimestamp"].ToString());
                        JObject payLoad = (JObject)originalJson["body"];
                        this.dynamicsClient.SetLoggingReference(logger);
                        DeactivateDealer dealersIn = new DeactivateDealer(this.dynamicsClient, logger);
                        ////framed payload and passed creationtimestamp
                        var result = dealersIn.DeactivateDealerIntegrationProcess(payLoad, creationTimestamp);
                        log.LogInformation("triggered the method.");
#pragma warning disable S6422 // Calls to "async" methods should not be blocking in Azure Functions
                        if (result.Result.IsSuccessStatusCode)
                        {
                            log.LogInformation($"Message body : {result.Result.StatusCode}, Message Content:{result.Result.Content.ReadAsStringAsync().GetAwaiter().GetResult()}");
#pragma warning restore S6422 // Calls to "async" methods should not be blocking in Azure Functions
                        }
                        else
                        {
                            log.LogError($"CTDI Integration: Failed in deactivate Dealer.");
                            throw new ArgumentException($"CTDI Integration: Failed in deactiave Dealer.");
                        }
                    }
                    else
                    {
                        log.LogError($"CTDI Integration: Failed - No Dealer payload.");
                        throw new ArgumentException("CTDI Integration: Failed - No Dealer payload.");
                    }
                }
            }
            catch (Exception ex)
            {
                log.LogError($"CTDI Integration :{ex.Message}");
                throw new ArgumentException(ex.Message);
            }
        }
    }
}
