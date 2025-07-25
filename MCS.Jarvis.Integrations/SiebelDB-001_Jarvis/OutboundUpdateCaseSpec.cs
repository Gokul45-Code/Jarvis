// <copyright file="OutboundUpdateCaseSpec.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SiebelDB_CasePassOutbound
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using Azure.Messaging.ServiceBus;
    using IntegrationProcess;
    using IntegrationProcess.Helper.Constants;
    using MCS.Jarvis.Integration.Base.Dynamics;
    using MCS.Jarvis.Integration.Base.Logging;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.ServiceBus;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    /// <summary>
    /// OutboundUpdateCaseSpec Azure Function Class.
    /// </summary>
    public class OutboundUpdateCaseSpec
    {
        private readonly IConfiguration config;
        private readonly IDynamicsApiClient dynamicsClient;
        private readonly string azureServicebusConnectionstring;
        private readonly string queueName;

        /// <summary>
        /// Initializes a new instance of the <see cref="OutboundUpdateCaseSpec"/> class.
        /// </summary>
        /// <param name="config">Configuration.</param>
        /// <param name="dynamicsClient">Dynamics Client.</param>
        public OutboundUpdateCaseSpec(IConfiguration config, IDynamicsApiClient dynamicsClient)
        {
            this.config = config;
            this.dynamicsClient = dynamicsClient;
            this.azureServicebusConnectionstring = this.config.GetSection("ServiceBusConnection").Value;
            this.queueName = this.config.GetSection("OutboundUpdateCaseSpecQueue").Value;
        }

        /// <summary>
        /// OutboundUpdateCaseSpec Azure Function.
        /// </summary>
        /// <param name="req">Request.</param>
        /// <param name="log">Logger.</param>
        /// <returns>Task.</returns>
        /// <exception cref="ArgumentException">Argument Exception.</exception>
        [FunctionName("outboundupdatecasespecificentity")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            string id = data?.Id;
            string transType = data?.TransType;
            string entityData = data?.EntityData;

            #region CaseSpecificEntities
            if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(transType))
            {
                switch (transType.ToUpper())
                {
                    case TransType.EtaOutbound or TransType.EtcOutbound or TransType.AtaOutbound or TransType.AtcOutbound: // or TransType.delayedEtaOutbound
                        {
                            return await this.PassOutActionResult(log, id, entityData, transType);
                        }

                    case TransType.DelayedEtaOutbound:
                        {
                            return await this.DelayedEtaActionResult(log, id, entityData, transType);
                        }

                    case TransType.AddRemarkOutbound:
                        {
                            return await this.RemarkActionResult(log, id, transType);
                        }

                    case TransType.ReportFaultOutbound:
                        {
                            return await this.ReportFaultActionResult(log, id, entityData, transType);
                        }

                    case TransType.AddGopOutbound:
                        {
                            return await this.AddGopActionResult(log, id, entityData, transType);
                        }

                    case GopRequestType.GOPHD:
                        {
                            return await this.AddGopPlusActionResult(log, id, entityData, GopRequestType.GOPHD);
                        }

                    case GopRequestType.GOPRD:
                        {
                            return await this.AddGopPlusActionResult(log, id, entityData, GopRequestType.GOPRD);
                        }

                    case TransType.AddPassOutOutbound:
                        {
                            return await this.AddPassOutActionResult(log, id, entityData, transType);
                        }

                    case TransType.RepairInfoOutbound:
                        {
                            string translationType = data?.TranslationType;
                            return await this.RepairInfoActionResult(log, id, entityData, transType, translationType);
                        }

                    case TransType.JobEndOutbound:
                        {
                            string translationType = data?.TranslationType;
                            return await this.JobEndActionResult(log, id, entityData, transType, translationType);
                        }

                    case TransType.MonitorHisOutbound:
                        {
                            return await this.MonitorActionResult(log, id, entityData, transType);
                        }

                    case TransType.AttachmentOutbound:
                        {
                            return await this.AttachmentActionResult(log, id, entityData, transType);
                        }

                    default:
                        {
                            return new NotFoundObjectResult($"TransType {transType} is not Implemented" + this.queueName);
                        }
                }
            }
            #endregion
            else
            {
                log.LogError($"Value for Required Field from configuraiton is missing in the paylaod : Id or Transtype");
                throw new ArgumentException($"Value for Required Field from configuraiton is missing in the paylaod : Id or Transtype");
            }
        }

        /// <summary>
        /// PassOut Outbound Integration Action.
        /// </summary>
        /// <param name="log">Logger.</param>
        /// <param name="passOutId">PassOut Id.</param>
        /// <param name="transType">TransType.</param>
        /// <returns>Task.</returns>
        private async Task<IActionResult> PassOutActionResult(ILogger log, string passOutId, string entityData, string transType)
        {
            #region PassOut
            ILoggerService logger = new LoggerService(log);
            log.LogInformation($"PassOut Id and transType received in request is " + passOutId + " " + transType);
            this.dynamicsClient.SetLoggingReference(logger);
            UpsertCasePassOutbound passOutSpec = new UpsertCasePassOutbound(this.dynamicsClient, this.config, logger);
            var response = await passOutSpec.IntegrationProcessAsync(passOutId, entityData, transType);
            var xmlPayload = JsonConvert.DeserializeXNode(response.GetValue("sbMessage").ToString(), "ServiceRequestBdArgus");

            var xmlData =
            new XElement(
                "SiebelMessage",
                new XAttribute("MessageId", passOutId),
                new XAttribute("EventTimestamp", DateTime.Parse(response.GetValue("modifiedOn").ToString()).ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture)),
                new XAttribute("TransType", transType),
                new XAttribute("MessageType", "Integration Object"),
                new XAttribute("IntObjectName", "Mercurius Case Detail Info ARGUS"),
                new XAttribute("IntObjectFormat", "Siebel Hierarchical"),
                new XElement("ListOfServiceRequestBreakdownArgus", xmlPayload.Descendants("ServiceRequestBdArgus")));
            ServiceBusClient sbClient = new ServiceBusClient(this.azureServicebusConnectionstring);
            ServiceBusSender sender = sbClient.CreateSender(this.queueName);
            ServiceBusMessage message = new ServiceBusMessage(Encoding.UTF8.GetBytes(xmlData.ToString()))
            {
                MessageId = Guid.NewGuid().ToString(),
                ContentType = "application/xml",
                Subject = transType,
            };
            await sender.SendMessageAsync(message);
            ////IQueueClient sbClient = new QueueClient(connectionString: this.azureServicebusConnectionstring, this.queueName);
            ////var orderMessage = new Message(Encoding.UTF8.GetBytes(xmlData.ToString()))
            ////{
            ////    MessageId = Guid.NewGuid().ToString(),
            ////    ContentType = "application/xml",
            ////    Label = transType,
            ////};
            ////await sbClient.SendAsync(orderMessage);
            log.LogInformation($"Successfully send data to servicebus queue " + this.queueName + " with Data " + xmlData);
            return new OkObjectResult($"Successfully send data to servicebus queue " + this.queueName);
            #endregion
        }

        /// <summary>
        /// Remark Outbound Integration Action.
        /// </summary>
        /// <param name="log">Logger.</param>
        /// <param name="postId">Post Id.</param>
        /// <param name="transType">Trans type.</param>
        /// <returns>Task.</returns>
        private async Task<IActionResult> RemarkActionResult(ILogger log, string postId, string transType)
        {
            #region Remark
            ILoggerService logger = new LoggerService(log);
            log.LogInformation($"Post Id and transType received in request is " + postId + " " + transType);
            this.dynamicsClient.SetLoggingReference(logger);
            CreateDeleteRemarkOut remarkOut = new CreateDeleteRemarkOut(this.dynamicsClient, this.config, logger);
            var response = await remarkOut.IntegrationProcessAsync(postId);
            var xmlPayload = JsonConvert.DeserializeXNode(response.ToString(), "ServiceRequestBdArgus");
            var xmlData =
            new XElement(
                "SiebelMessage",
                new XAttribute("MessageId", postId),
                new XAttribute("EventTimestamp", DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture)),
                new XAttribute("TransType", transType),
                new XAttribute("MessageType", "Integration Object"),
                new XAttribute("IntObjectName", "Mercurius Case Detail Info ARGUS"),
                new XAttribute("IntObjectFormat", "Siebel Hierarchical"),
                new XElement("ListOfServiceRequestBreakdownArgus", xmlPayload.Descendants("ServiceRequestBdArgus")));
            ServiceBusClient sbClient = new ServiceBusClient(this.azureServicebusConnectionstring);
            ServiceBusSender sender = sbClient.CreateSender(this.queueName);
            ServiceBusMessage message = new ServiceBusMessage(Encoding.UTF8.GetBytes(xmlData.ToString()))
            {
                MessageId = Guid.NewGuid().ToString(),
                ContentType = "application/xml",
                Subject = transType,
            };
            await sender.SendMessageAsync(message);
            //IQueueClient sbClient = new QueueClient(connectionString: this.azureServicebusConnectionstring, this.queueName);
            //var orderMessage = new Message(Encoding.UTF8.GetBytes(xmlData.ToString()))
            //{
            //    MessageId = Guid.NewGuid().ToString(),
            //    ContentType = "application/xml",
            //    Label = transType,
            //};
            //await sbClient.SendAsync(orderMessage);
            log.LogInformation($"Successfully send data to servicebus queue " + this.queueName + " with Data " + xmlData);
            return new OkObjectResult($"Successfully send data to servicebus queue " + this.queueName);
            #endregion
        }

        /// <summary>
        ///  ReportFault Outbound Integration Action.
        /// </summary>
        /// <param name="log">Logger.</param>
        /// <param name="caseTransId">caseTransId.</param>
        /// <param name="transType">transType.</param>
        /// <returns>Task.</returns>
        private async Task<IActionResult> ReportFaultActionResult(ILogger log, string caseTransId, string entityData, string transType)
        {
            #region ReportedFault
            ILoggerService logger = new LoggerService(log);
            log.LogInformation($"Report Fault Case translation Id and transType received in request is " + caseTransId + " " + transType);
            this.dynamicsClient.SetLoggingReference(logger);
            ReportFaultOut reportFaultOut = new ReportFaultOut(this.dynamicsClient, this.config, logger);
            var response = await reportFaultOut.IntegrationProcessAsync(caseTransId, entityData);
            var xmlPayload = JsonConvert.DeserializeXNode(response.GetValue("sbMessage").ToString(), "ServiceRequestBdArgus");
            var xmlData =
            new XElement(
                "SiebelMessage",
                new XAttribute("MessageId", caseTransId),
                new XAttribute("EventTimestamp", DateTime.Parse(response.GetValue("modifiedOn").ToString()).ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture)),
                new XAttribute("TransType", transType),
                new XAttribute("MessageType", "Integration Object"),
                new XAttribute("IntObjectName", "Mercurius Case Detail Info ARGUS"),
                new XAttribute("IntObjectFormat", "Siebel Hierarchical"),
                new XElement("ListOfServiceRequestBreakdownArgus", xmlPayload.Descendants("ServiceRequestBdArgus")));
            ServiceBusClient sbClient = new ServiceBusClient(this.azureServicebusConnectionstring);
            ServiceBusSender sender = sbClient.CreateSender(this.queueName);
            ServiceBusMessage message = new ServiceBusMessage(Encoding.UTF8.GetBytes(xmlData.ToString()))
            {
                MessageId = Guid.NewGuid().ToString(),
                ContentType = "application/xml",
                Subject = transType,
            };
            await sender.SendMessageAsync(message);
            ////IQueueClient sbClient = new QueueClient(connectionString: this.azureServicebusConnectionstring, this.queueName);
            ////var orderMessage = new Message(Encoding.UTF8.GetBytes(xmlData.ToString()))
            ////{
            ////    MessageId = Guid.NewGuid().ToString(),
            ////    ContentType = "application/xml",
            ////    Label = transType,
            ////};
            ////await sbClient.SendAsync(orderMessage);
            log.LogInformation($"Successfully send data to servicebus queue " + this.queueName + " with Data " + xmlData);
            return new OkObjectResult($"Successfully send data to servicebus queue " + this.queueName);
            #endregion
        }

        /// <summary>
        /// GOP Outbound Integration Action.
        /// </summary>
        /// <param name="log">Logger.</param>
        /// <param name="gopId">Gop Id.</param>
        /// <param name="transType">transType.</param>
        /// <returns>Task.</returns>
        private async Task<IActionResult> AddGopActionResult(ILogger log, string gopId, string entityData, string transType)
        {
            #region Gop
            ILoggerService logger = new LoggerService(log);
            log.LogInformation($"Add Gop and transType received in request is " + gopId + " " + transType);
            this.dynamicsClient.SetLoggingReference(logger);
            AddGopOut addGopOut = new AddGopOut(this.dynamicsClient, this.config, logger);
            var response = await addGopOut.IntegrationProcessAsync(gopId, entityData);
            var xmlPayload = JsonConvert.DeserializeXNode(response.GetValue("sbMessage").ToString(), "ServiceRequestBdArgus");
            var xmlData =
            new XElement(
                "SiebelMessage",
                new XAttribute("MessageId", gopId),
                new XAttribute("EventTimestamp", DateTime.Parse(response.GetValue("modifiedOn").ToString()).ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture)),
                new XAttribute("TransType", transType),
                new XAttribute("MessageType", "Integration Object"),
                new XAttribute("IntObjectName", "Mercurius Case Detail Info ARGUS"),
                new XAttribute("IntObjectFormat", "Siebel Hierarchical"),
                new XElement("ListOfServiceRequestBreakdownArgus", xmlPayload.Descendants("ServiceRequestBdArgus")));
            ServiceBusClient sbClient = new ServiceBusClient(this.azureServicebusConnectionstring);
            ServiceBusSender sender = sbClient.CreateSender(this.queueName);
            ServiceBusMessage message = new ServiceBusMessage(Encoding.UTF8.GetBytes(xmlData.ToString()))
            {
                MessageId = Guid.NewGuid().ToString(),
                ContentType = "application/xml",
                Subject = transType,
            };
            await sender.SendMessageAsync(message);
            ////IQueueClient sbClient = new QueueClient(connectionString: this.azureServicebusConnectionstring, this.queueName);
            ////var orderMessage = new Message(Encoding.UTF8.GetBytes(xmlData.ToString()))
            ////{
            ////    MessageId = Guid.NewGuid().ToString(),
            ////    ContentType = "application/xml",
            ////    Label = transType,
            ////};
            ////await sbClient.SendAsync(orderMessage);
            log.LogInformation($"Successfully send data to servicebus queue " + this.queueName + " with Data " + xmlData);
            return new OkObjectResult($"Successfully send data to servicebus queue " + this.queueName);
            #endregion
        }

        /// <summary>
        /// GOP Plus Outbound Integration Action.
        /// </summary>
        /// <param name="log">Logger.</param>
        /// <param name="gopId">GopId.</param>
        /// <param name="entityData">entityData.</param>
        /// <returns>Task.</returns>
        private async Task<IActionResult> AddGopPlusActionResult(ILogger log, string gopId, string entityData, string gopPlusType)
        {
            #region GopPlus
            ILoggerService logger = new LoggerService(log);
            string transType = "Jarvis.Event.AddGOP+";
            log.LogInformation($"Add Gop and transType received in request is " + gopId + " " + transType);
            this.dynamicsClient.SetLoggingReference(logger);
            AddGopPlusOutbound addGopPlusOut = new AddGopPlusOutbound(this.dynamicsClient, this.config, logger);
            var response = await addGopPlusOut.IntegrationProcessAsync(gopId, entityData, gopPlusType);
            var xmlPayload = JsonConvert.DeserializeXNode(response.GetValue("sbMessage").ToString(), "ServiceRequestBdArgus");
            var xmlData =
            new XElement(
                "SiebelMessage",
                new XAttribute("MessageId", gopId),
                new XAttribute("EventTimestamp", DateTime.Parse(response.GetValue("modifiedOn").ToString()).ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture)),
                new XAttribute("TransType", transType),
                new XAttribute("MessageType", "Integration Object"),
                new XAttribute("IntObjectName", "Mercurius Case Detail Info ARGUS"),
                new XAttribute("IntObjectFormat", "Siebel Hierarchical"),
                new XElement("ListOfServiceRequestBreakdownArgus", xmlPayload.Descendants("ServiceRequestBdArgus")));
            ServiceBusClient sbClient = new ServiceBusClient(this.azureServicebusConnectionstring);
            ServiceBusSender sender = sbClient.CreateSender(this.queueName);
            ServiceBusMessage message = new ServiceBusMessage(Encoding.UTF8.GetBytes(xmlData.ToString()))
            {
                MessageId = Guid.NewGuid().ToString(),
                ContentType = "application/xml",
                Subject = transType,
            };
            await sender.SendMessageAsync(message);
            ////IQueueClient sbClient = new QueueClient(connectionString: this.azureServicebusConnectionstring, this.queueName);
            ////var orderMessage = new Message(Encoding.UTF8.GetBytes(xmlData.ToString()))
            ////{
            ////    MessageId = Guid.NewGuid().ToString(),
            ////    ContentType = "application/xml",
            ////    Label = transType,
            ////};
            ////await sbClient.SendAsync(orderMessage);
            log.LogInformation($"Successfully send data to servicebus queue " + this.queueName + " with Data " + xmlData);
            return new OkObjectResult($"Successfully send data to servicebus queue " + this.queueName);
            #endregion
        }

        /// <summary>
        /// PASSOUT Outbound Integration Action.
        /// </summary>
        /// <param name="log">Logger.</param>
        /// <param name="passOutId">PassOut Id.</param>
        /// <param name="entityData">Entity Data.</param>
        /// <param name="transType">TransType.</param>
        /// <returns>Task.</returns>
        private async Task<IActionResult> AddPassOutActionResult(ILogger log, string passOutId, string entityData, string transType)
        {
            #region PassOut
            ILoggerService logger = new LoggerService(log);
            log.LogInformation($"Add PassOut Update and transType received in request is " + passOutId + " " + transType);
            this.dynamicsClient.SetLoggingReference(logger);
            AddPassOutOutbound addPassOut = new AddPassOutOutbound(this.dynamicsClient, this.config, logger);
            var response = await addPassOut.IntegrationProcessAsync(passOutId, entityData);
            var xmlPayload = JsonConvert.DeserializeXNode(response.GetValue("sbMessage").ToString(), "ServiceRequestBdArgus");
            var xmlData =
            new XElement(
                "SiebelMessage",
                new XAttribute("MessageId", passOutId),
                new XAttribute("EventTimestamp", DateTime.Parse(response.GetValue("modifiedOn").ToString()).ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture)),
                new XAttribute("TransType", transType),
                new XAttribute("MessageType", "Integration Object"),
                new XAttribute("IntObjectName", "Mercurius Case Detail Info ARGUS"),
                new XAttribute("IntObjectFormat", "Siebel Hierarchical"),
                new XElement("ListOfServiceRequestBreakdownArgus", xmlPayload.Descendants("ServiceRequestBdArgus")));
            ServiceBusClient sbClient = new ServiceBusClient(this.azureServicebusConnectionstring);
            ServiceBusSender sender = sbClient.CreateSender(this.queueName);
            ServiceBusMessage message = new ServiceBusMessage(Encoding.UTF8.GetBytes(xmlData.ToString()))
            {
                MessageId = Guid.NewGuid().ToString(),
                ContentType = "application/xml",
                Subject = transType,
            };
            await sender.SendMessageAsync(message);
            ////IQueueClient sbClient = new QueueClient(connectionString: this.azureServicebusConnectionstring, this.queueName);
            ////var orderMessage = new Message(Encoding.UTF8.GetBytes(xmlData.ToString()))
            ////{
            ////    MessageId = Guid.NewGuid().ToString(),
            ////    ContentType = "application/xml",
            ////    Label = transType,
            ////};
            ////await sbClient.SendAsync(orderMessage);
            log.LogInformation($"Successfully send data to servicebus queue " + this.queueName + " with Data " + xmlData);
            return new OkObjectResult($"Successfully send data to servicebus queue " + this.queueName);
            #endregion
        }

        /// <summary>
        /// RepairInfo Outbound Integration Action.
        /// </summary>
        /// <param name="log">Logger.</param>
        /// <param name="caseTransId">Case Trans Id.</param>
        /// <param name="transType">Trans Type.</param>
        /// <returns>Task.</returns>
        private async Task<IActionResult> RepairInfoActionResult(ILogger log, string caseTransId, string entityData, string transType, string translationType)
        {
            #region Repair Information
            ILoggerService logger = new LoggerService(log);
            log.LogInformation($"Report Fault Case translation Id and transType received in request is " + caseTransId + " " + transType + " " + translationType);
            this.dynamicsClient.SetLoggingReference(logger);
            AddRepairInfoOutbound repairInfoOut = new AddRepairInfoOutbound(this.dynamicsClient, this.config, logger);
            await repairInfoOut.IntegrationProcessAsync(caseTransId, entityData, transType, translationType);
            return new OkObjectResult($"Successfully send data to servicebus queue " + this.queueName);
            #endregion
        }

        /// <summary>
        /// ReportFault Outbound Integration Action.
        /// </summary>
        /// <param name="log">Logger.</param>
        /// <param name="etaPassOutTransId">ETA Passout Trans Id.</param>
        /// <param name="entityData">Entity Data.</param>
        /// <param name="transType">Trans Type.</param>
        /// <returns>Task.</returns>
        private async Task<IActionResult> DelayedEtaActionResult(ILogger log, string etaPassOutTransId, string entityData, string transType)
        {
            #region DelayedETA
            ILoggerService logger = new LoggerService(log);
            log.LogInformation($"Delayed ETA PassOut translation Id and transType received in request is " + etaPassOutTransId + " " + transType);
            this.dynamicsClient.SetLoggingReference(logger);
            DelayedEtaOutbound delayedEta = new DelayedEtaOutbound(this.dynamicsClient, this.config, logger);
            var response = await delayedEta.IntegrationProcessAsync(etaPassOutTransId, entityData);
            var xmlPayload = JsonConvert.DeserializeXNode(response.GetValue("sbMessage").ToString(), "ServiceRequestBdArgus");
            var xmlData =
            new XElement(
                "SiebelMessage",
                new XAttribute("MessageId", etaPassOutTransId),
                new XAttribute("EventTimestamp", DateTime.Parse(response.GetValue("modifiedOn").ToString()).ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture)),
                new XAttribute("TransType", transType),
                new XAttribute("MessageType", "Integration Object"),
                new XAttribute("IntObjectName", "Mercurius Case Detail Info ARGUS"),
                new XAttribute("IntObjectFormat", "Siebel Hierarchical"),
                new XElement("ListOfServiceRequestBreakdownArgus", xmlPayload.Descendants("ServiceRequestBdArgus")));
            ServiceBusClient sbClient = new ServiceBusClient(this.azureServicebusConnectionstring);
            ServiceBusSender sender = sbClient.CreateSender(this.queueName);
            ServiceBusMessage message = new ServiceBusMessage(Encoding.UTF8.GetBytes(xmlData.ToString()))
            {
                MessageId = Guid.NewGuid().ToString(),
                ContentType = "application/xml",
                Subject = transType,
            };
            await sender.SendMessageAsync(message);
            ////IQueueClient sbClient = new QueueClient(connectionString: this.azureServicebusConnectionstring, this.queueName);
            ////var orderMessage = new Message(Encoding.UTF8.GetBytes(xmlData.ToString()))
            ////{
            ////    MessageId = Guid.NewGuid().ToString(),
            ////    ContentType = "application/xml",
            ////    Label = transType,
            ////};
            ////await sbClient.SendAsync(orderMessage);
            log.LogInformation($"Successfully send data to servicebus queue " + this.queueName + " with Data " + xmlData);
            return new OkObjectResult($"Successfully send data to servicebus queue " + this.queueName);
            #endregion
        }

        /// <summary>
        /// RepairInfo Outbound Integration Action.
        /// </summary>
        /// <param name="log">Logger.</param>
        /// <param name="jobEndTransId">job End Trans Id.</param>
        /// <param name="entityData">Entity Data.</param>
        /// <param name="transType">Trans type.</param>
        /// <param name="translationType">translationType.</param>
        /// <returns>Task.</returns>
        private async Task<IActionResult> JobEndActionResult(ILogger log, string jobEndTransId, string entityData, string transType, string translationType)
        {
            #region JobEndDetails
            ILoggerService logger = new LoggerService(log);
            log.LogInformation($"Job End Detail translation Id and transType received in request is " + jobEndTransId + " " + transType + " " + translationType);
            this.dynamicsClient.SetLoggingReference(logger);
            JobEndOutbound jobEndOut = new JobEndOutbound(this.dynamicsClient, this.config, logger);
            await jobEndOut.IntegrationProcessAsync(jobEndTransId, transType, entityData, translationType);
            return new OkObjectResult($"Successfully send data to servicebus queue " + this.queueName);
            #endregion
        }

        /// <summary>
        /// MonitorAction Outbound Integration Action.
        /// </summary>
        /// <param name="log">Logger.</param>
        /// <param name="monitorActionId">Monitor Action Id.</param>
        /// <param name="entityData">Entity Data.</param>
        /// <param name="transType">Trans Type.</param>
        /// <returns>Task.</returns>
        private async Task<IActionResult> MonitorActionResult(ILogger log, string monitorActionId, string entityData, string transType)
        {
            #region MonitorAction
            ILoggerService logger = new LoggerService(log);
            log.LogInformation($"Monitor Action Id and transType received in request is " + monitorActionId + " " + transType);
            this.dynamicsClient.SetLoggingReference(logger);
            AddMonitorActionOutbound addMonitorAction = new AddMonitorActionOutbound(this.dynamicsClient, this.config, logger);
            var response = await addMonitorAction.IntegrationProcessAsync(monitorActionId, entityData);
            var xmlPayload = JsonConvert.DeserializeXNode(response.ToString(), "ServiceRequestBdArgus");
            var xmlData =
            new XElement(
                "SiebelMessage",
                new XAttribute("MessageId", monitorActionId),
                new XAttribute("EventTimestamp", DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture)),
                new XAttribute("TransType", transType),
                new XAttribute("MessageType", "Integration Object"),
                new XAttribute("IntObjectName", "Mercurius Case Detail Info ARGUS"),
                new XAttribute("IntObjectFormat", "Siebel Hierarchical"),
                new XElement("ListOfServiceRequestBreakdownArgus", xmlPayload.Descendants("ServiceRequestBdArgus")));
            ServiceBusClient sbClient = new ServiceBusClient(this.azureServicebusConnectionstring);
            ServiceBusSender sender = sbClient.CreateSender(this.queueName);
            ServiceBusMessage message = new ServiceBusMessage(Encoding.UTF8.GetBytes(xmlData.ToString()))
            {
                MessageId = Guid.NewGuid().ToString(),
                ContentType = "application/xml",
                Subject = transType,
            };
            await sender.SendMessageAsync(message);
            ////IQueueClient sbClient = new QueueClient(connectionString: this.azureServicebusConnectionstring, this.queueName);
            ////var orderMessage = new Message(Encoding.UTF8.GetBytes(xmlData.ToString()))
            ////{
            ////    MessageId = Guid.NewGuid().ToString(),
            ////    ContentType = "application/xml",
            ////    Label = transType,
            ////};
            ////await sbClient.SendAsync(orderMessage);
            log.LogInformation($"Successfully send data to servicebus queue " + this.queueName + " with Data " + xmlData);
            return new OkObjectResult($"Successfully send data to servicebus queue " + this.queueName);
            #endregion
        }

        private async Task<IActionResult> AttachmentActionResult(ILogger log, string attachmentId, string entityData, string transType)
        {
            #region AttachmentAction
            ILoggerService logger = new LoggerService(log);
            log.LogInformation($"Add Attachment and transType received in request is " + attachmentId + " " + transType);
            this.dynamicsClient.SetLoggingReference(logger);
            AddAttachmentOutbound addAttachmentAction = new AddAttachmentOutbound(this.dynamicsClient, this.config, logger);
            var response = await addAttachmentAction.IntegrationProcessAsync(attachmentId, entityData);
            var xmlPayload = JsonConvert.DeserializeXNode(response.ToString(), "ServiceRequestBdArgus");
            var xmlData =
            new XElement(
                "SiebelMessage",
                new XAttribute("MessageId", attachmentId),
                new XAttribute("EventTimestamp", DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture)),
                new XAttribute("TransType", transType),
                new XAttribute("MessageType", "Integration Object"),
                new XAttribute("IntObjectName", "Mercurius JARVIS Case Attachment Info ARGUS"),
                new XAttribute("IntObjectFormat", "Siebel Hierarchical"),
                new XElement("ListOfServiceRequestBreakdownArgus", xmlPayload.Descendants("ServiceRequestBdArgus")));
            ServiceBusClient sbClient = new ServiceBusClient(this.azureServicebusConnectionstring);
            ServiceBusSender sender = sbClient.CreateSender(this.queueName);
            ServiceBusMessage message = new ServiceBusMessage(Encoding.UTF8.GetBytes(xmlData.ToString()))
            {
                MessageId = Guid.NewGuid().ToString(),
                ContentType = "application/xml",
                Subject = transType,
            };
            await sender.SendMessageAsync(message);
            ////IQueueClient sbClient = new QueueClient(connectionString: this.azureServicebusConnectionstring, this.queueName);
            ////var orderMessage = new Message(Encoding.UTF8.GetBytes(xmlData.ToString()))
            ////{
            ////    MessageId = Guid.NewGuid().ToString(),
            ////    ContentType = "application/xml",
            ////    Label = transType,
            ////};
            ////await sbClient.SendAsync(orderMessage);
            log.LogInformation($"Successfully send data to servicebus queue " + this.queueName + " with Data " + xmlData);
            return new OkObjectResult($"Successfully send data to servicebus queue " + this.queueName);
            #endregion
        }
    }
}
