// <copyright file="VolvoPayInbound.cs" company="Microsoft.">
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
    /// VolvoPayInbound Integration process.
    /// </summary>
    public class VolvoPayInbound
    {
        private readonly IDynamicsApiClient dynamicsClient;
        private readonly IConfiguration config;
        private readonly ILoggerService logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="VolvoPayInbound"/> class.
        /// VolvoPayInbound.
        /// </summary>
        /// <param name="dynamicsClient">dynamicsClient.</param>
        /// <param name="config">config.</param>
        /// <param name="logger">logger.</param>
        public VolvoPayInbound(IDynamicsApiClient dynamicsClient, IConfiguration config, ILoggerService logger)
        {
            this.dynamicsClient = dynamicsClient;
            this.config = config;
            this.logger = logger;
        }

        /// <summary>
        /// IntegrationProcessAsync.
        /// </summary>
        /// <param name="paymentRequestId">paymentRequestId.</param>
        /// <param name="volvoPayEventType">volvoPayEventType.</param>
        /// <param name="volvoPayEventOn">volvoPayEventOn.</param>
        /// <returns>Task.</returns>
        /// <exception cref="ArgumentException">ArgumentException.</exception>
        public async Task<string> IntegrationProcessAsync(string paymentRequestId, string volvoPayEventType, string volvoPayEventOn)
        {
            try
            {
                string gopId = await this.GetGOPRecord(paymentRequestId);
                if (!string.IsNullOrEmpty(gopId))
                {
                    this.logger.LogTrace($"GOP Record retrieve {gopId} for the paymentRequestId {paymentRequestId}");
                    await this.UpdateGOPRecord(gopId, volvoPayEventType, volvoPayEventOn);
                    return gopId;
                }

                this.logger.LogTrace($"Failed to retrieve/Update GOP record in CRM. Payment Request ID: {paymentRequestId}");
                throw new ArgumentException($"Failed to retrieve/Update GOP record in CRM. Payment Request ID: {paymentRequestId}");
            }
            catch (Exception ex)
            {
                this.logger.LogTrace($"Failed to retrieve/Update GOP record in CRM. Payment Request ID: {paymentRequestId}");
                throw new ArgumentException($"IntegrationProcessAsync:" + ex.Message);
            }
        }

        /// <summary>
        /// GetGOPRecord.
        /// </summary>
        /// <param name="paymentRequestId">paymentRequestId.</param>
        /// <returns>Task.</returns>
        /// <exception cref="ArgumentException">ArgumentException.</exception>
        public async Task<string> GetGOPRecord(string paymentRequestId)
        {
            try
            {
                this.logger.LogTrace("Enter into GetGOPRecord");
                string entitySetName = VolvoPayConstants.EntityName;
                string selectQuery = VolvoPayConstants.EntityId;
                string filterQuery = $"{VolvoPayConstants.VolvoPaymentRequestId} eq '{paymentRequestId}'";
                var resultSet = await this.dynamicsClient.RetrieveResultSetByFilterAsync(entitySetName, selectQuery, filterQuery);
                this.logger.LogTrace("GOPRecordRetrieved");
                if (resultSet != null && resultSet.Count > 0)
                {
                    var gopRecord = resultSet.First();
                    return gopRecord.Value<string>(VolvoPayConstants.EntityId) ?? string.Empty;
                }

                this.logger.LogTrace($"Failed to retrieve GOP record from CRM. Payment Request ID: {paymentRequestId}");
                throw new ArgumentException($"Failed to retrieve GOP record from CRM. Payment Request ID: {paymentRequestId}");
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"GetGOPRecord:" + ex.Message);
            }
        }

        /// <summary>
        /// UpdateGOPRecord.
        /// </summary>
        /// <param name="gopRecordId">gopRecordId.</param>
        /// <param name="volvoPayEventType">volvoPayEventType.</param>
        /// <param name="volvoPayEventOn">volvoPayEventOn.</param>
        /// <returns>Task.</returns>
        /// <exception cref="ArgumentException">ArgumentException.</exception>
        public async Task UpdateGOPRecord(string gopRecordId, string volvoPayEventType, string volvoPayEventOn)
        {
            try
            {
                this.logger.LogTrace("Enter into UpdateGOPRecord");

                // Construct the update GOP Record
                var updateData = new JObject
                {
                    { VolvoPayConstants.VolvoPayEventType, volvoPayEventType },
                    { VolvoPayConstants.VolvoPayEventOn, volvoPayEventOn },
                };

                string entitySetName = VolvoPayConstants.EntityName;
                await this.dynamicsClient.UpdateEntityAsync(entitySetName, gopRecordId, updateData);
                this.logger.LogTrace("GOPRecordUpdated");
            }
            catch (Exception ex)
            {
                this.logger.LogTrace($"Failed to update GOP record in CRM. GOP Record ID: {gopRecordId}");
                throw new ArgumentException($"UpdateGOPRecordInCRM : Failed to update GOP record in CRM. GOP Record ID: {gopRecordId}  : {ex.Message}");
            }
        }
    }
}
