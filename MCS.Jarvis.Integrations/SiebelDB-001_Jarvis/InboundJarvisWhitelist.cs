// <copyright file="InboundJarvisWhitelist.cs" company="PlaceholderCompany">
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
    /// Azure Function for Jarvis WhiteList.
    /// </summary>
    public class InboundJarvisWhitelist
    {
        private readonly IConfiguration config;
        private readonly IDynamicsApiClient dynamicsClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="InboundJarvisWhitelist"/> class.
        /// </summary>
        /// <param name="config">Configuration.</param>
        /// <param name="dynamicsClient">Dynamic Client.</param>
        public InboundJarvisWhitelist(IConfiguration config, IDynamicsApiClient dynamicsClient)
        {
            this.config = config;
            this.dynamicsClient = dynamicsClient;
        }

        /// <summary>
        /// Azure Function for customer and vehicle whitelist.
        /// </summary>
        /// <param name="myQueueItem">Queue Name.</param>
        /// <param name="log">logger.</param>
        /// <exception cref="ArgumentException">Exception.</exception>
        /// <returns>representing the asynchronous operation.</returns>
        [FunctionName("inboundjarviswhitelist")]
        [ExponentialBackoffRetry(3, "00:00:30", "00:02:00")]
        public async Task Run([ServiceBusTrigger("%InboundJarvisWhitelistQueue%", Connection = "ServiceBusConnection")] string myQueueItem, ILogger log)
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
                if (originalJson["SiebelMessage"] != null && originalJson["SiebelMessage"]["ListOfAccountBreakdownArgus"] != null &&
                transType.ToUpper() == "Mercurius.Event.CustWhitelist".ToUpper())
                {
                    log.LogInformation($"isCustomerWhitelist: {transType.ToUpper()}");
                    await this.AddCustomerWhitelist(log, logger, originalJson);
                }
                else if (originalJson["SiebelMessage"] != null && originalJson["SiebelMessage"]["ListOfVehiclesUnitArgus"] != null && originalJson["SiebelMessage"]["ListOfVehiclesUnitArgus"]["VehicleUnitsBdArgus"] != null &&
                transType.ToUpper() == "Mercurius.Event.VINWhitelist".ToUpper())
                {
                    log.LogInformation($"isVINWhitelist: {transType.ToUpper()}");
                    await this.AddVINWhitelist(log, logger, originalJson);
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

        private async Task AddCustomerWhitelist(ILogger log, ILoggerService logger, JObject originalJson)
        {
            JObject payLoad = (JObject)originalJson["SiebelMessage"]["ListOfAccountBreakdownArgus"]["Account"];
            log.LogInformation("payload is ready");
            this.dynamicsClient.SetLoggingReference(logger);
            log.LogInformation("setting logger info in dynamicsclient");
            AddCustomerWhitelist addCustomerWhitelist = new AddCustomerWhitelist(this.dynamicsClient, logger);
            log.LogInformation("Add Customer Whitelist inbound");
            var result = await addCustomerWhitelist.IntegrationProcessAsync(JObject.Parse(payLoad.ToString()));
            log.LogInformation("triggered the method.");
            if (result.IsSuccessStatusCode)
            {
                log.LogInformation($"Message body : {result.StatusCode}, Message Content:{result.Content.ReadAsStringAsync().GetAwaiter().GetResult()}");
            }
            else
            {
                throw new ArgumentException($"Add Customer Whitelist: Failed in adding Customer Whitelist.");
            }
        }

        private async Task AddVINWhitelist(ILogger log, ILoggerService logger, JObject originalJson)
        {
            JObject payLoad = (JObject)originalJson["SiebelMessage"]["ListOfVehiclesUnitArgus"]["VehicleUnitsBdArgus"];
            log.LogInformation("payload is ready");
            string vinNumber = payLoad["SerialNumber"]?.ToString();
            var vehicleUnit = payLoad["ListOfVehicleUnitFinancialArgus"]["VehicleUnitFinancialArgus"];
            this.dynamicsClient.SetLoggingReference(logger);
            log.LogInformation("setting logger info in dynamicsclient");
            AddVINWhitelist addVinWhitelist = new AddVINWhitelist(this.dynamicsClient, logger);
            log.LogInformation("Add Customer Whitelist inbound");
            var result = await addVinWhitelist.IntegrationProcessAsync(JObject.Parse(vehicleUnit.ToString()), vinNumber);
            log.LogInformation("triggered the method.");
            if (result.IsSuccessStatusCode)
            {
                log.LogInformation($"Message body : {result.StatusCode}, Message Content:{result.Content.ReadAsStringAsync().GetAwaiter().GetResult()}");
            }
            else
            {
                throw new ArgumentException($"Add VIN Whitelist: Failed in adding VIN Whitelist.");
            }
        }
    }
}
