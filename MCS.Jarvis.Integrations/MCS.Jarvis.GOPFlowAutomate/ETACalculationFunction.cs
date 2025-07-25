using System;
using IntegrationProcess;
using MCS.Jarvis.Integration.Base.Dynamics;
using MCS.Jarvis.Integration.Base.Logging;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GOPFlowAutomate
{
    public class ETACalculationFunction
    {
        private readonly IConfiguration config;
        private readonly IDynamicsApiClient dynamicsClient;
        public ETACalculationFunction(IConfiguration config, IDynamicsApiClient dynamicsClient)
        {
            this.config = config;
            this.dynamicsClient = dynamicsClient;
        }
        [FunctionName("etacalculationfunction")]
        public void Run([TimerTrigger("%FunctionTriggerSchedule%")]TimerInfo myTimer, ILogger log) ////0 0 0 * 6- 12am saturday
        {
            ILoggerService logger = new LoggerService(log);
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            this.dynamicsClient.SetLoggingReference(logger);
            log.LogInformation("setting logger info in dynamicsclient");
            EtaCalculationAutomation etaCalculation = new EtaCalculationAutomation(this.dynamicsClient, logger);
            log.LogInformation("Trigger to ETA Calculation.");
            var result = etaCalculation.IntegrationProcessAsync();
            if (result.Result.IsSuccessStatusCode)
            {
                log.LogInformation($"Message body : {result.Result.StatusCode}, Message Content:{result.Result.Content.ReadAsStringAsync().GetAwaiter().GetResult()}");
#pragma warning restore S6422 // Calls to "async" methods should not be blocking in Azure Functions
            }
            else
            {
                throw new ArgumentException($"ETA Calculation : Failed to calculate ETA.");
            }
        }
    }
}
