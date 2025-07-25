// <copyright file="VdaVehicleInbound.cs" company="PlaceholderCompany">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Func_Jarvis_Vda
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
    /// Initializes a new instance of the <see cref="VdaVehicleInbound"/> class.
    /// Azure Function App for VDA.
    /// </summary>
    public class VdaVehicleInbound
    {
        private readonly IConfiguration config;
        private readonly IDynamicsApiClient dynamicsClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="VdaVehicleInbound"/> class.
        /// </summary>
        /// <param name="config">Configuration.</param>
        /// <param name="dynamicsClient">Dynamic Client.</param>
        public VdaVehicleInbound(IConfiguration config, IDynamicsApiClient dynamicsClient)
        {
            this.config = config;
            this.dynamicsClient = dynamicsClient;
        }

        /// <summary>
        /// Azure Function.
        /// </summary>
        /// <param name="myQueueItem">Queue.</param>
        /// <param name="log">logger.</param>
        /// <exception cref="ArgumentException">Exception.</exception>
        /// <returns>representing the asynchronous operation.</returns>
        [FunctionName("inboundupsertvehicle")]
        [ExponentialBackoffRetry(3, "00:00:30", "00:02:00")]
        public async Task Run([ServiceBusTrigger("%InboundQueue%", Connection = "ServiceBusConnection")] string myQueueItem, ILogger log)
        {
            log.LogInformation($"C# ServiceBus queue trigger function processed message: {myQueueItem}");
            if (!string.IsNullOrEmpty(myQueueItem))
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(myQueueItem);
                ILoggerService logger = new LoggerService(log);
                string jsonData = JsonConvert.SerializeXmlNode(doc.GetElementsByTagName("SyncHardIndividualProduct").Item(0));
                log.LogInformation($"C# ServiceBus queue trigger function processed message: {jsonData}");
                JObject originalJson = JObject.Parse(jsonData);
                log.LogInformation("parse into jobject..");
                string actionCriteria = originalJson["SyncHardIndividualProduct"]["DataArea"]["Sync"]["volvo:ActionCriteria"].ToString();
                if (originalJson["SyncHardIndividualProduct"] != null && originalJson["SyncHardIndividualProduct"]["DataArea"] != null && actionCriteria.ToUpper() == "GeneralInformation".ToUpper())
                {
                    log.LogInformation($"isUpsert: {originalJson["SyncHardIndividualProduct"]["DataArea"]}");
                    await this.UpsertVehicle(log, logger, originalJson);
                }
                else if (originalJson["SyncHardIndividualProduct"] != null && originalJson["SyncHardIndividualProduct"]["DataArea"] != null && actionCriteria.ToUpper() == "Variants".ToUpper())
                {
                    log.LogInformation($"isUpsert for Fuel/Power Type from VDA: {originalJson["SyncHardIndividualProduct"]["DataArea"]}");
                    await this.UpdateVehicleVariant(log, logger, originalJson);
                }
                else
                {
                    throw new ArgumentException("VDA Integration: Not a valid request Body from VDA ");
                }
            }
            else
            {
                throw new ArgumentException("VDA Integration: No request Body from VDA");
            }
        }

        private async Task UpsertVehicle(ILogger log, ILoggerService logger, JObject originalJson)
        {
            JObject payLoad = (JObject)originalJson["SyncHardIndividualProduct"]["DataArea"]["HardIndividualProduct"];
            log.LogInformation("payload is ready");
            this.dynamicsClient.SetLoggingReference(logger);
            log.LogInformation("setting logger info in dynamicsclient");
            UpsertVehicleIn casesIn = new UpsertVehicleIn(this.dynamicsClient, logger);
            log.LogInformation("Created the upsertVehicleInbound");
            var result = await casesIn.IntegrationProcessAsync(payLoad);
            log.LogInformation("triggered the method.");
            if (result.IsSuccessStatusCode)
            {
                log.LogInformation($"Message body : {result.StatusCode}, Message Content:{result.Content.ReadAsStringAsync().GetAwaiter().GetResult()}");
            }
            else
            {
                throw new ArgumentException($"Upsert Vehicle: Failed in updating vehicle.");
            }
        }

        private async Task UpdateVehicleVariant(ILogger log, ILoggerService logger, JObject originalJson)
        {
            JObject payLoad = (JObject)originalJson["SyncHardIndividualProduct"]["DataArea"]["HardIndividualProduct"];
            log.LogInformation("payload is ready");
            this.dynamicsClient.SetLoggingReference(logger);
            log.LogInformation("setting logger info in dynamicsclient");
            UpsertVehicleVariantsIn casesIn = new UpsertVehicleVariantsIn(this.dynamicsClient, logger);
            log.LogInformation("Created the upsertVehicleInbound");
            var result = await casesIn.IntegrationProcessAsync(payLoad);
            log.LogInformation("triggered the method.");
            if (result.IsSuccessStatusCode)
            {
                log.LogInformation($"Message body : {result.StatusCode}, Message Content:{result.Content.ReadAsStringAsync().GetAwaiter().GetResult()}");
            }
            else
            {
                throw new ArgumentException($"Update of Vehicle Variant: Failed in updating vehicle.");
            }
        }
    }
}
