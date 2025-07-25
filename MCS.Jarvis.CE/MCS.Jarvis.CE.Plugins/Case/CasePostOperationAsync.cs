//-----------------------------------------------------------------------
// <copyright file="CasePostOperationAsync.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace MCS.Jarvis.CE.Plugins
{
    using System;
    using System.Globalization;
    using global::Plugins;
    using MCS.Jarvis.CE.BusinessProcessesShared.Helpers;
    using MCS.Jarvis.CE.BusinessProcessShared.Case;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Messages;
    using static MCS.Jarvis.CE.BusinessProcessesShared.Helpers.Constants;

    /// <summary>
    /// Case Post Operation Async.
    /// </summary>
    public class CasePostOperationAsync : PluginBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CasePostOperationAsync"/> class.
        /// Constructor CasePostOperationAsync.
        /// </summary>
        public CasePostOperationAsync()
            : base(typeof(CasePostOperationAsync))
        {
        }

        /// <summary>
        /// Execute CRM Plugin.
        /// </summary>
        /// <param name="localcontext">local Context.</param>
        public override void ExecuteCrmPlugin(LocalPluginContext localcontext)
        {
            ITracingService tracingService = localcontext.TracingService;
            tracingService.Trace("Start CasePostOperationAsync:" + DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));
            try
            {
                IPluginExecutionContext context = localcontext.PluginExecutionContext;
                if (context.InputParameters.Contains(General.Target) && context.InputParameters[General.Target] is Entity entity)
                {
                    if (context.Depth > 2)
                    {
                        tracingService.Trace("Depth is greater than 1");
                        return;
                    }

                    tracingService.Trace("Enter into the Case Record");
                    IOrganizationService adminService = localcontext.AdminOrganizationService;
                    Entity incidentPostImg = context.PostEntityImages?[General.PostImage];
                    Entity incidentPreImg = context.PreEntityImages?[General.PreImage];
                    tracingService.Trace("Retrieved the images");
                    if (incidentPostImg != null && incidentPreImg != null)
                    {
                        tracingService.Trace($"{incidentPreImg.Contains(Incident.JarvisRestGopLimitOut)}");
                        tracingService.Trace($"{entity.Contains(Incident.JarvisRestGopLimitOut)}");
                        if ((!incidentPreImg.Contains(Incident.JarvisRestGopLimitOut) || entity.Contains(Incident.JarvisRestGopLimitOut)) || (incidentPreImg.Contains(Incident.JarvisRestGopLimitOut) && incidentPostImg.Contains(Incident.JarvisRestGopLimitOut) && incidentPostImg.Contains(Incident.JarvisTotalRestCurrencyOut) && incidentPostImg.Attributes[Incident.JarvisTotalRestCurrencyOut] != null
                            && incidentPreImg.Attributes[Incident.JarvisRestGopLimitOut] != incidentPostImg.Attributes[Incident.JarvisRestGopLimitOut]))
                        {
                            tracingService.Trace("Compare Image Value");
                            EntityReference caseAvailableCurrency = incidentPostImg.GetAttributeValue<EntityReference>(Incident.JarvisTotalRestCurrencyOut);
                            decimal caseAvailableGOPout = (decimal)incidentPostImg.Attributes[Incident.JarvisRestGopLimitOut];

                            CaseAvailableGOPOut caseAvailable = new CaseAvailableGOPOut();

                            var requestToUpdateRecords = new ExecuteTransactionRequest()
                            {
                                // Create an empty organization request collection.
                                Requests = new OrganizationRequestCollection(),
                                ReturnResponses = false,
                            };
                            //// GOP Update
                            requestToUpdateRecords = caseAvailable.UpdateGOPAvailableAmount(adminService, entity.Id, caseAvailableCurrency, caseAvailableGOPout, requestToUpdateRecords, tracingService);

                            //// PassOut Update
                            requestToUpdateRecords = caseAvailable.UpdatePassOutAvailableAmount(adminService, entity.Id, caseAvailableCurrency, caseAvailableGOPout, requestToUpdateRecords, tracingService);

                            if (requestToUpdateRecords.Requests.Count > 0)
                            {
                                adminService.Execute(requestToUpdateRecords);
                            }
                        }
                    }
                    else
                    {
                        tracingService.Trace($"Case GOP Out Available Amount is not equal with preimage and postimage.");
                        return;
                    }
                }
            }
            catch (InvalidPluginExecutionException ex)
            {
                tracingService.Trace(ex.Message);
                tracingService.Trace(ex.StackTrace);
                throw new InvalidPluginExecutionException($"CasePostOperationSync Exception : {ex.Message}");
            }
            catch (Exception ex)
            {
                tracingService.Trace(ex.Message);
                tracingService.Trace(ex.StackTrace);
                throw new InvalidPluginExecutionException($"CasePostOperationSync Exception : {ex.Message}");
            }
            finally
            {
                tracingService.Trace("End CasePostOperationAsync:" + DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));
            }
        }
    }
}
