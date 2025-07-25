// <copyright file="InboundCreateCase.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SiebelDB_001_Jarvis
{
    using System;
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
    /// Inbound Create Case.
    /// </summary>
    public class InboundCreateCase
    {
        private readonly IConfiguration config;
        private readonly IDynamicsApiClient dynamicsClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="InboundCreateCase"/> class.
        /// </summary>
        /// <param name="config">Configuration.</param>
        /// <param name="dynamicsClient">Dynamics Client.</param>
        public InboundCreateCase(IConfiguration config, IDynamicsApiClient dynamicsClient)
        {
            this.config = config;
            this.dynamicsClient = dynamicsClient;
        }

        /// <summary>
        /// Azure Function Run function.
        /// </summary>
        /// <param name="myQueueItem">Queue Name.</param>
        /// <param name="log">logger.</param>
        /// <exception cref="ArgumentException">Argument Exception.</exception>
        [FunctionName("inboundcreatecase")]
        [ExponentialBackoffRetry(3, "00:00:30", "00:02:00")]
        public void Run([ServiceBusTrigger("%InboundCreateCaseQueue%", Connection = "ServiceBusConnection")] string myQueueItem, ILogger log)
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
                if (originalJson["SiebelMessage"] != null && originalJson["SiebelMessage"]["ListOfServiceRequestBreakdownArgus"] != null && transType.ToUpper() == "Mercurius.Event.CreateCase".ToUpper())
                {
                    log.LogInformation($"isCreate: {originalJson["SiebelMessage"]["@TransType"].ToString().ToUpper()}");
                    JObject payLoad = (JObject)originalJson["SiebelMessage"]["ListOfServiceRequestBreakdownArgus"]["ServiceRequestBdArgus"];
                    log.LogInformation("payload is ready");
                    this.dynamicsClient.SetLoggingReference(logger);
                    log.LogInformation("setting logger info in dynamicsclient");
                    CreateCasesIn casesIn = new CreateCasesIn(this.dynamicsClient, logger);
                    log.LogInformation("Created the upsertDealersin");
                    var result = casesIn.IntegrationProcessAsync(payLoad);
                    log.LogInformation("triggered the method.");
#pragma warning disable S6422 // Calls to "async" methods should not be blocking in Azure Functions
                    if (result.Result.IsSuccessStatusCode)
                    {
                        log.LogInformation($"Message body : {result.Result.StatusCode}, Message Content:{result.Result.Content.ReadAsStringAsync().GetAwaiter().GetResult()}");
#pragma warning restore S6422 // Calls to "async" methods should not be blocking in Azure Functions
                    }
                    else
                    {
                        throw new ArgumentException($"Create case: Failed in creating case.");
                    }
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
    }
}
