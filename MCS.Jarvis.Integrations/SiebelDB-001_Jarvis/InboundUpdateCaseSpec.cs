// <copyright file="InboundUpdateCaseSpec.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SiebelDB_001_Jarvis
{
    using System;
    using System.Globalization;
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
    /// Inbound update case specification class.
    /// </summary>
    public class InboundUpdateCaseSpec
    {
        private readonly IConfiguration config;
        private readonly IDynamicsApiClient dynamicsClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="InboundUpdateCaseSpec"/> class.
        /// </summary>
        /// <param name="config">Configuration.</param>
        /// <param name="dynamicsClient">Dynamics Client.</param>
        public InboundUpdateCaseSpec(IConfiguration config, IDynamicsApiClient dynamicsClient)
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
        /// <returns>representing the asynchronous operation.</returns>
        [FunctionName("inboundupdatecasespecificentity")]
        [ExponentialBackoffRetry(3, "00:00:30", "00:02:00")]
        public async Task Run([ServiceBusTrigger("%InboundUpdateCaseSpecQueue%", Connection = "ServiceBusConnection")] string myQueueItem, ILogger log)
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
                //// Removing DelayedETAUpdate lgoic.
                if (originalJson["SiebelMessage"] != null && originalJson["SiebelMessage"]["ListOfServiceRequestBreakdownArgus"] != null &&
                (transType.ToUpper() == "Mercurius.Event.ETAUpdate".ToUpper() || transType.ToUpper() == "Mercurius.Event.ETCUpdate".ToUpper() ||
                transType.ToUpper() == "Mercurius.Event.ATCUpdate".ToUpper() || transType.ToUpper() == "Mercurius.Event.ATAUpdate".ToUpper() ||
                transType.ToUpper() == "Mercurius.Event.GPSETAUpdate".ToUpper()))
                {
                    if (originalJson.SelectToken("SiebelMessage.@EventTimestamp") == null || string.IsNullOrEmpty(originalJson.SelectToken("SiebelMessage.@EventTimestamp").ToString()))
                    {
                        log.LogError("EventTimestamp is a mandatory field.");
                        throw new ArgumentException("EventTimestamp is a mandatory field.");
                    }

                    log.LogInformation($"isUpdate: {transType.ToUpper()}");
                    await this.UpdateEtaEtcAtcAta(log, logger, originalJson, transType);
                }
                else if (originalJson["SiebelMessage"] != null && originalJson["SiebelMessage"]["ListOfServiceRequestBreakdownArgus"] != null &&
                transType.ToUpper() == "Mercurius.Event.AddRemark".ToUpper())
                {
                    log.LogInformation($"isAddRemark: {transType.ToUpper()}");
                    await this.AddRemark(log, logger, originalJson);
                }
                else if (originalJson["SiebelMessage"] != null && originalJson["SiebelMessage"]["ListOfServiceRequestBreakdownArgus"] != null &&
                (transType.ToUpper() == "Mercurius.Event.AddReportFault1".ToUpper() || transType.ToUpper() == "Mercurius.Event.AddReportFault2".ToUpper()))
                {
                    if (originalJson.SelectToken("SiebelMessage.@EventTimestamp") == null || string.IsNullOrEmpty(originalJson.SelectToken("SiebelMessage.@EventTimestamp").ToString()))
                    {
                        log.LogError("EventTimestamp is a mandatory field.");
                        throw new ArgumentException("EventTimestamp is a mandatory field.");
                    }

                    log.LogInformation($"isAddReportFault: {transType.ToUpper()}");
                    await this.AddReportFault(log, logger, originalJson, transType);
                }
                else if (originalJson["SiebelMessage"] != null && originalJson["SiebelMessage"]["ListOfServiceRequestBreakdownArgus"] != null && transType.ToUpper() == "Mercurius.Event.AddGOP".ToUpper())
                {
                    if (originalJson.SelectToken("SiebelMessage.@EventTimestamp") == null || string.IsNullOrEmpty(originalJson.SelectToken("SiebelMessage.@EventTimestamp").ToString()))
                    {
                        log.LogError("EventTimestamp is a mandatory field.");
                        throw new ArgumentException("EventTimestamp is a mandatory field.");
                    }

                    log.LogInformation($"isAddGOP: {originalJson["SiebelMessage"]["@TransType"].ToString().ToUpper()}");
                    await this.AddGOP(log, logger, originalJson);
                }
                else if (originalJson["SiebelMessage"] != null && originalJson["SiebelMessage"]["ListOfServiceRequestBreakdownArgus"] != null && transType.ToUpper() == "Mercurius.Event.PassOut".ToUpper())
                {
                    if (originalJson.SelectToken("SiebelMessage.@EventTimestamp") == null || string.IsNullOrEmpty(originalJson.SelectToken("SiebelMessage.@EventTimestamp").ToString()))
                    {
                        log.LogError("EventTimestamp is a mandatory field.");
                        throw new ArgumentException("EventTimestamp is a mandatory field.");
                    }

                    log.LogInformation($"isAddPassout: {originalJson["SiebelMessage"]["@TransType"].ToString().ToUpper()}");
                    await this.AddPassout(log, logger, originalJson);
                }
                else if (originalJson["SiebelMessage"] != null && originalJson["SiebelMessage"]["ListOfServiceRequestBreakdownArgus"] != null &&
                (transType.ToUpper() == "Mercurius.Event.RepairInfo1".ToUpper() || transType.ToUpper() == "Mercurius.Event.RepairInfo2".ToUpper()))
                {
                    log.LogInformation($"isAddRepairInfo: {transType.ToUpper()}");
                    await this.AddRepairInfo(log, logger, originalJson, transType);
                }
                //// Adding DelayedETAUpdateLogic.
                else if (originalJson["SiebelMessage"] != null && originalJson["SiebelMessage"]["ListOfServiceRequestBreakdownArgus"] != null && (transType.ToUpper() == "Mercurius.Event.DelayedETAUpdate1".ToUpper() || transType.ToUpper() == "Mercurius.Event.DelayedETAUpdate2".ToUpper()))
                {
                    if (originalJson.SelectToken("SiebelMessage.@EventTimestamp") == null || string.IsNullOrEmpty(originalJson.SelectToken("SiebelMessage.@EventTimestamp").ToString()))
                    {
                        log.LogError("EventTimestamp is a mandatory field.");
                        throw new ArgumentException("EventTimestamp is a mandatory field.");
                    }

                    log.LogInformation($"isDelayedETAUpdate: {transType.ToUpper()}");
                    await this.DelayedETA(log, logger, originalJson, transType);
                }
                else if (originalJson["SiebelMessage"] != null && originalJson["SiebelMessage"]["ListOfServiceRequestBreakdownArgus"] != null && transType.ToUpper() == "Mercurius.Event.AddGOP+".ToUpper())
                {
                    if (originalJson.SelectToken("SiebelMessage.@EventTimestamp") == null || string.IsNullOrEmpty(originalJson.SelectToken("SiebelMessage.@EventTimestamp").ToString()))
                    {
                        log.LogError("EventTimestamp is a mandatory field.");
                        throw new ArgumentException("EventTimestamp is a mandatory field.");
                    }

                    log.LogInformation($"isAddGOP+: {originalJson["SiebelMessage"]["@TransType"].ToString().ToUpper()}");
                    await this.AddGOPPlus(log, logger, originalJson);
                }
                else if (originalJson["SiebelMessage"] != null && originalJson["SiebelMessage"]["ListOfServiceRequestBreakdownArgus"] != null &&
                (transType.ToUpper() == "Mercurius.Event.JobEnd1".ToUpper() || transType.ToUpper() == "Mercurius.Event.JobEnd2".ToUpper()))
                {
                    if (originalJson.SelectToken("SiebelMessage.@EventTimestamp") == null || string.IsNullOrEmpty(originalJson.SelectToken("SiebelMessage.@EventTimestamp").ToString()))
                    {
                        log.LogError("EventTimestamp is a mandatory field.");
                        throw new ArgumentException("EventTimestamp is a mandatory field.");
                    }

                    log.LogInformation($"isAddJodEndDetails: {transType.ToUpper()}");
                    await this.AddJobEndDetails(log, logger, originalJson, transType);
                }
                else if (originalJson["SiebelMessage"] != null && originalJson["SiebelMessage"]["ListOfServiceRequestBreakdownArgus"] != null && transType.ToUpper() == "Mercurius.Event.SaveExitMonitorHis".ToUpper())
                {
                    log.LogInformation($"isAddSaveExitMonitor: {originalJson["SiebelMessage"]["@TransType"].ToString().ToUpper()}");
                    await this.AddCaseMonitorAction(log, logger, originalJson);
                }
                else if (originalJson["SiebelMessage"] != null && originalJson["SiebelMessage"]["ListOfServiceRequestBreakdownArgus"] != null && transType.ToUpper() == "Mercurius.Event.AddAttachment".ToUpper())
                {
                    log.LogInformation($"AddAttachment: {originalJson["SiebelMessage"]["@TransType"].ToString().ToUpper()}");
                    await this.AddAttachment(log, logger, originalJson);
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

        private async Task AddReportFault(ILogger log, ILoggerService logger, JObject originalJson, string transType)
        {
            DateTime eventTimestamp = DateTime.ParseExact(originalJson.SelectToken("SiebelMessage.@EventTimestamp").ToString(), "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
            JObject payLoad = (JObject)originalJson["SiebelMessage"]["ListOfServiceRequestBreakdownArgus"]["ServiceRequestBdArgus"];
            string caseNumberArgus = payLoad["CaseNumberARGUS"].ToString();
            string caseNumberJarvis = payLoad["CaseNumberJARVIS"]?.ToString();
            var reportFaultList = payLoad["ListOfSRBdReportFaultArgus"]["SRBdReportFaultArgus"];
            log.LogInformation("payload is ready");
            this.dynamicsClient.SetLoggingReference(logger);
            log.LogInformation("setting logger info in dynamicsclient");
            if (reportFaultList.GetType() == typeof(JObject))
            {
                AddReportFaultInbound addReportFault = new AddReportFaultInbound(this.dynamicsClient, logger);
                log.LogInformation("Add ReportFault inbound");
                var result = await addReportFault.IntegrationProcessAsync(JObject.Parse(reportFaultList.ToString()), caseNumberArgus, caseNumberJarvis, transType, eventTimestamp);
                log.LogInformation("triggered the method.");
                if (result.IsSuccessStatusCode)
                {
                    log.LogInformation($"Message body : {result.StatusCode}, Message Content:{result.Content.ReadAsStringAsync().GetAwaiter().GetResult()}");
                }
                else
                {
                    throw new ArgumentException($"Add ReportFault: Failed in adding ReportFault with case number " + caseNumberArgus);
                }
            }
            else
            {
                bool isSuccess = true;
                foreach (var item in reportFaultList)
                {
                    AddReportFaultInbound addReportFault = new AddReportFaultInbound(this.dynamicsClient, logger);
                    log.LogInformation("Add ReportFault inbound");
                    var result = await addReportFault.IntegrationProcessAsync(JObject.Parse(item.ToString()), caseNumberArgus, caseNumberJarvis, transType, eventTimestamp);
                    log.LogInformation("triggered the method.");
                    if (result.IsSuccessStatusCode)
                    {
                        log.LogInformation($"Message body : {result.StatusCode}, Message Content:{result.Content.ReadAsStringAsync().GetAwaiter().GetResult()}");
                    }
                    else
                    {
                        isSuccess = false;
                    }
                }

                if (!isSuccess)
                {
                    throw new ArgumentException($"Add ReportFault: Failed in adding ReportFault with case number " + caseNumberArgus);
                }
            }
        }

        private async Task AddRemark(ILogger log, ILoggerService logger, JObject originalJson)
        {
            JObject payLoad = (JObject)originalJson["SiebelMessage"]["ListOfServiceRequestBreakdownArgus"]["ServiceRequestBdArgus"];
            string caseNumberArgus = payLoad["CaseNumberARGUS"].ToString();
            string caseNumberJarvis = payLoad["CaseNumberJARVIS"]?.ToString();
            var remarkList = payLoad["ListOfServiceRequestRemark"]["ServiceRequestRemark"];
            log.LogInformation("payload is ready");
            this.dynamicsClient.SetLoggingReference(logger);
            log.LogInformation("setting logger info in dynamicsclient");
            if (remarkList.GetType() == typeof(JObject))
            {
                AddRemarkInbound addRemarks = new AddRemarkInbound(this.dynamicsClient, logger);
                log.LogInformation("Add remark inbound");
                var result = await addRemarks.IntegrationProcessAsync(JObject.Parse(remarkList.ToString()), caseNumberArgus, caseNumberJarvis);
                log.LogInformation("triggered the method.");
                if (result.IsSuccessStatusCode)
                {
                    log.LogInformation($"Message body : {result.StatusCode}, Message Content:{result.Content.ReadAsStringAsync().GetAwaiter().GetResult()}");
                }
                else
                {
                    throw new ArgumentException($"Add Remark: Failed in adding remark with case number " + caseNumberArgus);
                }
            }
            else
            {
                bool isSuccess = true;
                foreach (var item in remarkList)
                {
                    AddRemarkInbound addRemarks = new AddRemarkInbound(this.dynamicsClient, logger);
                    log.LogInformation("Add remark inbound");
                    var result = await addRemarks.IntegrationProcessAsync(JObject.Parse(item.ToString()), caseNumberArgus, caseNumberJarvis);
                    log.LogInformation("triggered the method.");
                    if (result.IsSuccessStatusCode)
                    {
                        log.LogInformation($"Message body : {result.StatusCode}, Message Content:{result.Content.ReadAsStringAsync().GetAwaiter().GetResult()}");
                    }
                    else
                    {
                        isSuccess = false;
                    }
                }

                if (!isSuccess)
                {
                    throw new ArgumentException($"Add remark: Failed in adding remark with case number " + caseNumberArgus);
                }
            }
        }

        private async Task UpdateEtaEtcAtcAta(ILogger log, ILoggerService logger, JObject originalJson, string transType)
        {
            DateTime eventTimestamp = DateTime.ParseExact(originalJson.SelectToken("SiebelMessage.@EventTimestamp").ToString(), "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
            JObject payLoad = (JObject)originalJson["SiebelMessage"]["ListOfServiceRequestBreakdownArgus"]["ServiceRequestBdArgus"];
            string caseNumberArgus = payLoad["CaseNumberARGUS"].ToString();
            string caseNumberJarvis = payLoad["CaseNumberJARVIS"]?.ToString();
            var passoutList = payLoad["ListOfSRBdPassoutLoginfoArgus"]["SRBdPassoutLoginfoArgus"];
            log.LogInformation("payload is ready");
            this.dynamicsClient.SetLoggingReference(logger);
            log.LogInformation("setting logger info in dynamicsclient");
            if (passoutList.GetType() == typeof(JObject))
            {
                UpsertPassout casesIn = new UpsertPassout(this.dynamicsClient, this.config, logger);
                log.LogInformation("Created the upsertCase inbound");
                var result = await casesIn.IntegrationProcessAsync(JObject.Parse(passoutList.ToString()), caseNumberArgus, transType, caseNumberJarvis, eventTimestamp);
                log.LogInformation("triggered the method.");
                if (result.IsSuccessStatusCode)
                {
                    log.LogInformation($"Message body : {result.StatusCode}, Message Content:{result.Content.ReadAsStringAsync().GetAwaiter().GetResult()}");
                }
                else
                {
                    throw new ArgumentException($"Update case: Failed in updating case with case number " + caseNumberArgus + " " + caseNumberJarvis);
                }
            }
            else
            {
                bool isSuccess = true;
                foreach (var item in passoutList)
                {
                    UpsertPassout casesIn = new UpsertPassout(this.dynamicsClient, this.config, logger);
                    log.LogInformation("Created the upsertCase inbound");
                    var result = await casesIn.IntegrationProcessAsync(JObject.Parse(item.ToString()), caseNumberArgus, transType, caseNumberJarvis, eventTimestamp);
                    log.LogInformation("triggered the method.");
                    if (result.IsSuccessStatusCode)
                    {
                        log.LogInformation($"Message body : {result.StatusCode}, Message Content:{result.Content.ReadAsStringAsync().GetAwaiter().GetResult()}");
                    }
                    else
                    {
                        isSuccess = false;
                    }
                }

                if (!isSuccess)
                {
                    throw new ArgumentException($"Upsert case: Failed in Upsert case with case number " + caseNumberArgus + " " + caseNumberJarvis);
                }
            }
        }

        private async Task AddRepairInfo(ILogger log, ILoggerService logger, JObject originalJson, string transType)
        {
            JObject payLoad = (JObject)originalJson["SiebelMessage"]["ListOfServiceRequestBreakdownArgus"]["ServiceRequestBdArgus"];
            string caseNumberArgus = payLoad["CaseNumberARGUS"].ToString();
            string caseNumberJarvis = payLoad["CaseNumberJARVIS"]?.ToString();
            var passOutPayload = payLoad["ListOfSRBdPassoutLoginfoArgus"]["SRBdPassoutLoginfoArgus"];
            JObject repairInfo;
            if (passOutPayload.HasValues)
            {
                repairInfo = JObject.Parse(passOutPayload["ListOfSRBreakdownRepairinfoArgus"]["SRBreakdownRepairinfoArgus"].ToString());
                if (repairInfo.HasValues)
                {
                    log.LogInformation("payload is ready");
                    this.dynamicsClient.SetLoggingReference(logger);
                    log.LogInformation("setting logger info in dynamicsclient");
                    AddRepairInfoInbound addRepairInfo = new AddRepairInfoInbound(this.dynamicsClient, logger);
                    log.LogInformation("Add ReportFault inbound");
                    var result = await addRepairInfo.IntegrationProcessAsync(repairInfo, JObject.Parse(passOutPayload.ToString()), caseNumberArgus, caseNumberJarvis, transType);
                    log.LogInformation("triggered the method.");
                    if (result.IsSuccessStatusCode)
                    {
                        log.LogInformation($"Message body : {result.StatusCode}, Message Content:{result.Content.ReadAsStringAsync().GetAwaiter().GetResult()}");
                    }
                    else
                    {
                        throw new ArgumentException($"Add RepairInfo: Failed in adding RepairInfo with case number " + caseNumberArgus);
                    }
                }
                else
                {
                    throw new ArgumentException($"Add RepairInfo: No Repair info data in the payload for the case number " + caseNumberArgus);
                }
            }
            else
            {
                throw new ArgumentException($"Add AddRepairInfo: Failed in adding RepairInfo because no PassOut payload for the case number " + caseNumberArgus);
            }
        }

        private async Task AddJobEndDetails(ILogger log, ILoggerService logger, JObject originalJson, string transType)
        {
            DateTime eventTimestamp = DateTime.ParseExact(originalJson.SelectToken("SiebelMessage.@EventTimestamp").ToString(), "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
            JObject payLoad = (JObject)originalJson["SiebelMessage"]["ListOfServiceRequestBreakdownArgus"]["ServiceRequestBdArgus"];
            string caseNumberArgus = string.Empty;
            string caseNumberJarvis = string.Empty;

            if (payLoad.HasValues)
            {
                caseNumberArgus = payLoad["CaseNumberARGUS"].ToString();
                caseNumberJarvis = payLoad["CaseNumberJARVIS"]?.ToString();
                log.LogInformation("payload is ready");
                this.dynamicsClient.SetLoggingReference(logger);
                log.LogInformation("setting logger info in dynamicsclient");
                AddJobEndDetailsInbound addJobEndDetails = new AddJobEndDetailsInbound(this.dynamicsClient, logger);
                log.LogInformation("Add Job End Details inbound");
                var result = await addJobEndDetails.IntegrationProcessAsync(payLoad, caseNumberArgus, caseNumberJarvis, transType, eventTimestamp);
                log.LogInformation("triggered the method.");
                if (result.IsSuccessStatusCode)
                {
                    log.LogInformation($"Message body : {result.StatusCode}, Message Content:{result.Content.ReadAsStringAsync().GetAwaiter().GetResult()}");
                }
                else
                {
                    throw new ArgumentException($"Add JobEndDetails: Failed in adding JobEndDetails with case number " + caseNumberArgus + " " + caseNumberJarvis);
                }
            }
            else
            {
                throw new ArgumentException($"Add JobEndDetails: Failed in adding JobEndDetails because no payload for the case number " + caseNumberArgus + " " + caseNumberJarvis);
            }
        }

        private async Task AddGOP(ILogger log, ILoggerService logger, JObject originalJson)
        {
            DateTime eventTimestamp = DateTime.ParseExact(originalJson.SelectToken("SiebelMessage.@EventTimestamp").ToString(), "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
            JObject payLoad = (JObject)originalJson["SiebelMessage"]["ListOfServiceRequestBreakdownArgus"]["ServiceRequestBdArgus"];
            log.LogInformation("payload is ready");
            string caseNumberArgus = payLoad["CaseNumberARGUS"].ToString();
            string caseNumberJarvis = payLoad["CaseNumberJARVIS"]?.ToString();
            var gopData = payLoad["ListOfSRBdGopInLoginfoArgus"]["SRBdGopInLoginfoArgus"];
            this.dynamicsClient.SetLoggingReference(logger);
            log.LogInformation("setting logger info in dynamicsclient");
            AddGopInbound casesIn = new AddGopInbound(this.dynamicsClient, logger);
            log.LogInformation("Created the add GOP");
            var result = await casesIn.IntegrationProcessAsync(JObject.Parse(gopData.ToString()), caseNumberArgus, caseNumberJarvis, eventTimestamp);
            log.LogInformation("triggered the method.");
            if (result.IsSuccessStatusCode)
            {
                log.LogInformation($"Message body : {result.StatusCode}, Message Content:{result.Content.ReadAsStringAsync().GetAwaiter().GetResult()}");
            }
            else
            {
                throw new ArgumentException($"Update case: Failed in updating case.");
            }
        }

        private async Task AddGOPPlus(ILogger log, ILoggerService logger, JObject originalJson)
        {
            DateTime eventTimestamp = DateTime.ParseExact(originalJson.SelectToken("SiebelMessage.@EventTimestamp").ToString(), "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
            JObject payLoad = (JObject)originalJson["SiebelMessage"]["ListOfServiceRequestBreakdownArgus"]["ServiceRequestBdArgus"];
            log.LogInformation("payload is ready");
            string caseNumberArgus = payLoad["CaseNumberARGUS"].ToString();
            string caseNumberJarvis = payLoad["CaseNumberJARVIS"]?.ToString();
            var gopData = payLoad["ListOfSRBreakdownGopPlusArgus"]["SRBreakdownGopPlusArgus"];
            this.dynamicsClient.SetLoggingReference(logger);
            log.LogInformation("setting logger info in dynamicsclient");
            AddGopPlusInbound casesIn = new AddGopPlusInbound(this.dynamicsClient, logger);
            log.LogInformation("Created the upsert GOP+");
            var result = await casesIn.IntegrationProcessAsync(JObject.Parse(gopData.ToString()), caseNumberArgus, caseNumberJarvis, eventTimestamp);
            log.LogInformation("triggered the method.");
            if (result.IsSuccessStatusCode)
            {
                log.LogInformation($"Message body : {result.StatusCode}, Message Content:{result.Content.ReadAsStringAsync().GetAwaiter().GetResult()}");
            }
            else
            {
                throw new ArgumentException($"Update case: Failed in updating case.");
            }
        }

        private async Task AddPassout(ILogger log, ILoggerService logger, JObject originalJson)
        {
            DateTime eventTimestamp = DateTime.ParseExact(originalJson.SelectToken("SiebelMessage.@EventTimestamp").ToString(), "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
            JObject payLoad = (JObject)originalJson["SiebelMessage"]["ListOfServiceRequestBreakdownArgus"]["ServiceRequestBdArgus"];
            log.LogInformation("payload is ready");
            string caseNumberArgus = payLoad["CaseNumberARGUS"].ToString();
            string caseNumberJarvis = payLoad["CaseNumberJARVIS"]?.ToString();
            var passoutData = payLoad["ListOfSRBdPassoutLoginfoArgus"]["SRBdPassoutLoginfoArgus"];
            this.dynamicsClient.SetLoggingReference(logger);
            log.LogInformation("setting logger info in dynamicsclient");
            AddPassoutInbound casesIn = new AddPassoutInbound(this.dynamicsClient, logger);
            log.LogInformation("Created the upsertDealersin");
            var result = await casesIn.IntegrationProcessAsync(JObject.Parse(passoutData.ToString()), caseNumberArgus, caseNumberJarvis, eventTimestamp);
            log.LogInformation("triggered the method.");
            if (result.IsSuccessStatusCode)
            {
                log.LogInformation($"Message body : {result.StatusCode}, Message Content:{result.Content.ReadAsStringAsync().GetAwaiter().GetResult()}");
            }
            else
            {
                throw new ArgumentException($"Update case: Failed in updating case.");
            }
        }

        private async Task AddCaseMonitorAction(ILogger log, ILoggerService logger, JObject originalJson)
        {
            JObject payLoad = (JObject)originalJson["SiebelMessage"]["ListOfServiceRequestBreakdownArgus"]["ServiceRequestBdArgus"];
            log.LogInformation("payload is ready");
            string caseNumberArgus = payLoad["CaseNumberARGUS"].ToString();
            string caseNumberJarvis = payLoad["CaseNumberJARVIS"]?.ToString();
            var caseMonitorData = payLoad["ListOfActionBdMonitorHistoryArgus"]["ActionBdMonitorHistoryArgus"];
            this.dynamicsClient.SetLoggingReference(logger);
            log.LogInformation("setting logger info in dynamicsclient");
            AddCaseMonitorActionInbound casesIn = new AddCaseMonitorActionInbound(this.dynamicsClient, logger);
            log.LogInformation("Create the Add Case Monitor Action");
            var result = await casesIn.IntegrationProcessAsync(JObject.Parse(caseMonitorData.ToString()), caseNumberArgus, caseNumberJarvis);
            log.LogInformation("triggered the method.");
            if (result.IsSuccessStatusCode)
            {
                log.LogInformation($"Message body : {result.StatusCode}, Message Content:{result.Content.ReadAsStringAsync().GetAwaiter().GetResult()}");
            }
            else
            {
                throw new ArgumentException($"Add Case Monitor Action: Failed in adding case monitor action.");
            }
        }

        private async Task DelayedETA(ILogger log, ILoggerService logger, JObject originalJson, string transType)
        {
            DateTime eventTimestamp = DateTime.ParseExact(originalJson.SelectToken("SiebelMessage.@EventTimestamp").ToString(), "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
            JObject payLoad = (JObject)originalJson["SiebelMessage"]["ListOfServiceRequestBreakdownArgus"]["ServiceRequestBdArgus"];
            string caseNumberArgus = payLoad["CaseNumberARGUS"].ToString();
            string caseNumberJarvis = payLoad["CaseNumberJARVIS"]?.ToString();
            var passOutPayload = payLoad["ListOfSRBdPassoutLoginfoArgus"]["SRBdPassoutLoginfoArgus"];
            JObject delayedEtaPayload;
            if (passOutPayload.HasValues)
            {
                delayedEtaPayload = JObject.Parse(passOutPayload["ListOfSrBreakdownEtaLogArgus"]["SrBreakdownEtaLogArgus"].ToString());

                log.LogInformation("payload is ready");
                this.dynamicsClient.SetLoggingReference(logger);
                log.LogInformation("setting logger info in dynamicsclient");
                if (delayedEtaPayload.HasValues)
                {
                    DelayedEtaInbound delayedEta = new DelayedEtaInbound(this.dynamicsClient, logger);
                    log.LogInformation("Add DealyedETA inbound");
                    var result = await delayedEta.IntegrationProcessAsync(JObject.Parse(passOutPayload.ToString()), delayedEtaPayload, caseNumberArgus, caseNumberJarvis, transType, eventTimestamp);
                    log.LogInformation("triggered the method.");
                    if (result.IsSuccessStatusCode)
                    {
                        log.LogInformation($"Message body : {result.StatusCode}, Message Content:{result.Content.ReadAsStringAsync().GetAwaiter().GetResult()}");
                    }
                    else
                    {
                        throw new ArgumentException($"Add DealyedETA: Failed in adding DelayedETA with case number " + caseNumberArgus);
                    }
                }
            }
            else
            {
                throw new ArgumentException($"Add DealyedETA: Failed in adding DelayedETA with case number " + caseNumberArgus);
            }
        }

        private async Task AddAttachment(ILogger log, ILoggerService logger, JObject originalJson)
        {
            JObject payLoad = (JObject)originalJson["SiebelMessage"]["ListOfServiceRequestBreakdownArgus"]["ServiceRequestBdArgus"];
            string caseNumberArgus = payLoad["CaseNumberARGUS"].ToString();
            string caseNumberJarvis = payLoad["CaseNumberJARVIS"]?.ToString();
            var attachmentPayload = payLoad["ListOfServiceRequestBdAttachmentArgus"]["ServiceRequestBdAttachmentArgus"];
            if (attachmentPayload.HasValues)
            {
                log.LogInformation("payload is ready");
                this.dynamicsClient.SetLoggingReference(logger);
                log.LogInformation("setting logger info in dynamicsclient");

                AddAttachmentInbound addAttachment = new AddAttachmentInbound(this.dynamicsClient, logger);
                log.LogInformation("Add Attachment inbound");
                var result = await addAttachment.IntegrationProcessAsync(JObject.Parse(attachmentPayload.ToString()), caseNumberArgus, caseNumberJarvis);
                log.LogInformation("triggered the method.");
                if (result.IsSuccessStatusCode)
                {
                    log.LogInformation($"Message body : {result.StatusCode}, Message Content:{result.Content.ReadAsStringAsync().GetAwaiter().GetResult()}");
                }
                else
                {
                    throw new ArgumentException($"Add Attachment: Failed in adding Attachment with case number " + caseNumberArgus);
                }
            }
            else
            {
                throw new ArgumentException($"Add Attachment: Failed in adding Attachment with case number " + caseNumberArgus);
            }
        }
    }
}
