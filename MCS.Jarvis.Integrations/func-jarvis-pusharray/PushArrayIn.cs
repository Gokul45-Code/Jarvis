// <copyright file="PushArray.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Func_jarvis_pusharray
{
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using IntegrationProcess;
    using MCS.Jarvis.Integration.Base.Dynamics;
    using MCS.Jarvis.Integration.Base.Logging;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Push Array of Data into Jarvis through Integration Process Async.
    /// </summary>
    public class PushArrayIn
    {
        /// <summary>
        /// application settings configuration.
        /// </summary>
        private readonly IConfiguration config;

        /// <summary>
        /// dynamics application client.
        /// </summary>
        private readonly IDynamicsApiClient dynamicsClient;
        private int counter = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="PushArrayIn"/> class.
        /// PushArrayIn.
        /// </summary>
        /// <param name="config">Config.</param>
        /// <param name="dynamicsClient">DynamicsApiClient.</param>
        public PushArrayIn(IConfiguration config, IDynamicsApiClient dynamicsClient)
        {
            this.config = config;
            this.dynamicsClient = dynamicsClient;
        }

        /// <summary>
        /// CDB Push Array Function.
        /// </summary>
        /// <param name="req">Requ.</param>
        /// <param name="log">Log.</param>
        /// <returns>AsyncTask.</returns>
        /// <exception cref="ArgumentException">Argument Null Exception.</exception>
        [FunctionName("inboundupsertcustomerpusharray")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            try
            {
                log.LogInformation("Push Array Function for CDB Started....");
                ILoggerService logger = new LoggerService(log);
                this.dynamicsClient.SetLoggingReference(logger);

                string connectionString = this.config.GetSection("AzureWebJobsStorage").Value;
                string containerName = this.config.GetSection("CDBContainerName").Value;
                string blobName = this.config.GetSection("CDBFileName").Value;

                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer container = blobClient.GetContainerReference(containerName);
                CloudBlockBlob blob = container.GetBlockBlobReference(blobName);

                using (var stream = new MemoryStream())
                {
                    await blob.DownloadToStreamAsync(stream);
                    log.LogInformation("CDB File is Downloaded from Blob location");
                    stream.Seek(0, SeekOrigin.Begin);
                    using (var reader = new StreamReader(stream))
                    {
                        var jsonString = await reader.ReadToEndAsync();
                        JArray jArray = JArray.Parse(jsonString);
                        log.LogInformation($"Converted into Jarray Object");
                        //// Call CDB Integration process method.
                        await this.CallCDBIntegrationProcessAsync(jArray, logger);
                        log.LogInformation("Push Array Function for CDB Ended....");
                        return new OkObjectResult("Push Array Function for CDB Executed...");
                    }
                }
            }
            catch (Exception ex)
            {
                log.LogError($"Push Array Function for CDB Failed with Execption : {ex.Message}");
                throw new ArgumentException(ex.Message);
            }
        }

        /// <summary>
        /// CallCDBIntegrationProcessAsync.
        /// </summary>
        /// <param name="jArray">JArray.</param>
        /// <param name="logger">Logger.</param>
        /// <returns>AsyncTask.</returns>
        private async Task CallCDBIntegrationProcessAsync(JArray jArray, ILoggerService logger)
        {
            ////Looping Jarray element and calling the CDB Integration Process Async.
            foreach (var payLoad in jArray)
            {
                try
                {
                    logger.LogTrace($"CDB {Interlocked.Increment(ref this.counter)} Payload is Started.... ");

                    UpsertCustomersIn upsertCustomersIn = new UpsertCustomersIn(this.dynamicsClient, logger);

                    // framed payload and passed DateTime.Now
                    HttpResponseMessage result = await upsertCustomersIn.IntegrationProcessAsync(payLoad.ToObject<JObject>(), DateTime.UtcNow);
                    if (result.IsSuccessStatusCode)
                    {
                        logger.LogTrace($"CDB {this.counter} Payload is Ended.... ");
                    }
                    else
                    {
                        logger.LogTrace($"CDB {this.counter} Payload is Failed with Error {result.Content}");
                    }
                }
                catch (Exception ex)
                {
                    logger.LogTrace($"CDB {this.counter} Payload is Failed with Exception {ex.Message} ");
                }
            }
        }
    }
}
