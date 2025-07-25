// <copyright file="UpdateCaseCsisIn.cs" company="Microsoft.">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace IntegrationProcess
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using IntegrationProcess.Helper.Constants;
    using MCS.Jarvis.Integration.Base.Dynamics;
    using MCS.Jarvis.Integration.Base.Logging;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// UpdateCaseCsisIn Integration process.
    /// </summary>
    public class UpdateCaseCsisIn
    {
        private readonly IDynamicsApiClient dynamicsClient;
        private readonly ILoggerService logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateCaseCsisIn"/> class.
        /// UpdateCaseCsisIn.
        /// </summary>
        /// <param name="dynamicsClient">dynamicsClient.</param>
        /// <param name="logger">logger.</param>
        public UpdateCaseCsisIn(IDynamicsApiClient dynamicsClient, ILoggerService logger)
        {
            this.dynamicsClient = dynamicsClient;
            this.logger = logger;
        }

        /// <summary>
        /// IntegrationProcessAsync.
        /// </summary>
        /// <param name="payLoad">Paylaod.</param>
        /// <returns>Task.</returns>
        /// <exception cref="ArgumentException">ArgumentException.</exception>
        public async Task<string> IntegrationProcessAsync(JObject payLoad)
        {
            try
            {
                if (payLoad != null && payLoad.TryGetValue(SiebelConstants.CaseNumber, StringComparison.OrdinalIgnoreCase, out JToken? caseNumber))
                {
                    payLoad.TryGetValue(SiebelConstants.MessageCode, StringComparison.OrdinalIgnoreCase, out JToken? messageCode);
                    payLoad.TryGetValue(SiebelConstants.LogEntryDate, StringComparison.OrdinalIgnoreCase, out JToken? logEntryDate);

                    string caseId = await this.GetCaseRecord(caseNumber.ToString());
                    if (!string.IsNullOrEmpty(caseId))
                    {
                        if (messageCode != null && !string.IsNullOrEmpty(messageCode.ToString()) && logEntryDate != null && !string.IsNullOrEmpty(logEntryDate.ToString()))
                        {
                            this.logger.LogTrace($"Case Record retrieve {caseId} for the Case Number {caseId}");
                            await this.UpdateCaseRecord(caseId, messageCode.ToString(), logEntryDate.ToString());
                            return caseId;
                        }
                        else
                        {
                            this.logger.LogTrace($"Payload does not contain MessageCode or LogEntryDate.");
                            throw new ArgumentException($"Payload does not contain MessageCode or LogEntryDate.");
                        }
                    }

                    this.logger.LogTrace($"Failed to retrieve/Update Case record in CRM. Case Number: {caseNumber}");
                    throw new ArgumentException($"Failed to retrieve/Update Case record in CRM. Case Number: {caseNumber}");
                }
                else
                {
                    this.logger.LogException(new ArgumentException("Payload does not contain unique indentifier - CaseNumber"));
                    throw new ArgumentException("Payload does not contain unique indentifier - CaseNumber");
                }
            }
            catch (Exception ex)
            {
                this.logger.LogTrace($"Failed to retrieve/Update Case record in CRM.");
                throw new ArgumentException($"IntegrationProcessAsync:" + ex.Message);
            }
        }

        /// <summary>
        /// GetCaseRecord.
        /// </summary>
        /// <param name="caseNumber">case Number.</param>
        /// <returns>Task.</returns>
        /// <exception cref="ArgumentException">ArgumentException.</exception>
        public async Task<string> GetCaseRecord(string caseNumber)
        {
            try
            {
                this.logger.LogTrace("Enter into GetCaseRecord");
                string entitySetName = Constants.Incidents;
                string selectQuery = Constants.IncidentId;
                string filterQuery = $"{SiebelConstants.JarvisSourceId} eq '{caseNumber}'";
                var resultSet = await this.dynamicsClient.RetrieveResultSetByFilterAsync(entitySetName, selectQuery, filterQuery);
                this.logger.LogTrace("CaseRecordRetrieved");
                if (resultSet != null && resultSet.Count > 0)
                {
                    var caseRecord = resultSet.First();
                    return caseRecord.Value<string>(Constants.IncidentId) ?? string.Empty;
                }

                this.logger.LogTrace($"Failed to retrieve Case record from CRM. Case Number: {caseNumber}");
                throw new ArgumentException($"Failed to retrieve Case record from CRM. Case Number: {caseNumber}");
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"GetCaseRecord:" + ex.Message);
            }
        }

        /// <summary>
        /// UpdateCaseRecord.
        /// </summary>
        /// <param name="caseRecordId">case RecordId.</param>
        /// <param name="messageCode">message Code.</param>
        /// <param name="logEntryDate">logEntryDate.</param>
        /// <returns>Task.</returns>
        /// <exception cref="ArgumentException">ArgumentException.</exception>
        public async Task UpdateCaseRecord(string caseRecordId, string messageCode, string logEntryDate)
        {
            try
            {
                int? invoiceStatus = null;
                this.logger.LogTrace("Enter into UpdateCaseRecord");

                if (!string.IsNullOrEmpty(messageCode) && messageCode.ToLower() == "Invoiced".ToLower())
                {
                    invoiceStatus = int.Parse(CaseInvoiceStatus.CsisTransferCompleted);
                }
                else
                {
                    invoiceStatus = int.Parse(CaseInvoiceStatus.CsisTransferFailed);
                }

                // Construct the update GOP Record
                var updateData = new JObject
                {
                    { SiebelConstants.JarvisInvoiceStatus, invoiceStatus },
                    { SiebelConstants.JarvisTimestamp, logEntryDate },
                };

                string entitySetName = Constants.Incidents;
                await this.dynamicsClient.UpdateEntityAsync(entitySetName, caseRecordId, updateData);
                this.logger.LogTrace("CaseRecordUpdated");
            }
            catch (Exception ex)
            {
                this.logger.LogTrace($"Failed to update Case record in CRM. Case Record ID: {caseRecordId}");
                throw new ArgumentException($"UpdateCaseRecordInCRM : Failed to update Case record in CRM. Case Record ID: {caseRecordId}  : {ex.Message}");
            }
        }
    }
}
