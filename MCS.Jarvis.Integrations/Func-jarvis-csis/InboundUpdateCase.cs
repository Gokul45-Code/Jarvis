// <copyright file="InboundCreateCase.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Func_jarvis_csis
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
    /// Inbound update Case.
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
        /// Azure Function Run function.
        /// </summary>
        /// <param name="myQueueItem">Queue Name.</param>
        /// <param name="log">logger.</param>
        /// <exception cref="ArgumentException">Argument Exception.</exception>
        [FunctionName("inboundupdatecase")]
        [ExponentialBackoffRetry(3, "00:00:30", "00:02:00")]
        public void Run([ServiceBusTrigger("%InboundUpdateCaseQueue%", Connection = "ServiceBusConnection")] string myQueueItem, ILogger log)
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
                //string transType = originalJson["SiebelMessage"]["@TransType"].ToString();
                if (originalJson["SiebelMessage"] != null && originalJson["SiebelMessage"]["ListOfServiceRequestBDASSSAPArgus"] != null) //&& transType.ToUpper() == "OneCase.Event.Case".ToUpper()
                {
                    //// log.LogInformation($"isUpdate: {originalJson["SiebelMessage"]["@TransType"].ToString().ToUpper()}");
                    JObject payLoad = (JObject)originalJson["SiebelMessage"]["ListOfServiceRequestBDASSSAPArgus"]["ASSSAPResponseMessage"];
                    log.LogInformation("payload is ready");
                    this.dynamicsClient.SetLoggingReference(logger);
                    log.LogInformation("setting logger info in dynamicsclient");
                    UpdateCaseCsisIn casesIn = new UpdateCaseCsisIn(this.dynamicsClient, logger);
                    log.LogInformation("Created the updateCasein");
                    var caseId = casesIn.IntegrationProcessAsync(payLoad);
                    log.LogInformation($"Successfully updated the case record: {caseId}");
                }
                else
                {
                    throw new ArgumentException("CSIS Integration: Not a valid request Body from CSIS.");
                }
            }
            else
            {
                throw new ArgumentException("CSIS Integration: No request Body from CSIS.");
            }
        }
    }
}
