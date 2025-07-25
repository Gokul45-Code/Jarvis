// <copyright file="CaseAvailableGOPOut.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MCS.Jarvis.CE.BusinessProcessShared.Case
{
    using System;
    using MCS.Jarvis.CE.BusinessProcessesShared.Helpers;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Messages;
    using Microsoft.Xrm.Sdk.Query;
    using static MCS.Jarvis.CE.BusinessProcessesShared.Helpers.Constants;

    /// <summary>
    /// Case Available GOP Out.
    /// </summary>
    public class CaseAvailableGOPOut
    {
        /// <summary>
        /// Update GOP Available Amount.
        /// </summary>
        /// <param name="adminService">Org service.</param>
        /// <param name="caseId">incident Id.</param>
        /// <param name="caseAvailableCurrency">total rest currency.</param>
        /// <param name="caseAvailableGOPout">case rest gop limit out.</param>
        /// <param name="executeTransactionRequest">Transaction Request.</param>
        /// <param name="tracingService">Trace service.</param>
        /// <returns>execute Transaction Request.</returns>
        /// <exception cref="InvalidPluginExecutionException">Plugin Exception.</exception>
        public ExecuteTransactionRequest UpdateGOPAvailableAmount(IOrganizationService adminService, Guid caseId, EntityReference caseAvailableCurrency, decimal caseAvailableGOPout, ExecuteTransactionRequest executeTransactionRequest, ITracingService tracingService)
        {
            try
            {
                tracingService.Trace("Enter into GOP Available amount update");
                EntityCollection gops = adminService.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.CaseAllGOPs, caseId)));
                if (gops.Entities.Count > 0)
                {
                    foreach (var gop in gops.Entities)
                    {
                        if (gop.Attributes.Contains(Gop.requestType) && gop.Attributes[Gop.requestType] != null)
                        {
                            OptionSetValue requestType = (OptionSetValue)gop.Attributes[Gop.requestType];

                            // GOP HD
                            if (requestType != null && requestType.Value == (int)GopRequestType.GOP_HD && gop.Attributes.Contains(Gop.JarvisGopInCurrency) && gop.Attributes[Gop.JarvisGopInCurrency] != null)
                            {
                                EntityReference gopInCurrency = gop.GetAttributeValue<EntityReference>(Gop.JarvisGopInCurrency);
                                if (gopInCurrency != null)
                                {
                                    decimal exchangeValue = CrmHelper.CurrencyExchange(caseAvailableCurrency.Id, gopInCurrency.Id, adminService);
                                    Entity gopHdUpdate = new Entity(gop.LogicalName, gop.Id);
                                    gopHdUpdate.Attributes[Gop.JarvisAmountHdCurrency] = gopInCurrency;
                                    gopHdUpdate.Attributes[Gop.JarvisAmountHd] = caseAvailableGOPout * exchangeValue;
                                    UpdateRequest gopUpdate = new UpdateRequest { Target = gopHdUpdate };
                                    executeTransactionRequest.Requests.Add(gopUpdate);
                                }
                            }
                            else if (requestType != null && requestType.Value == (int)GopRequestType.GOP_RD && gop.Attributes.Contains(Gop.JarvisGopOutCurrency) && gop.Attributes[Gop.JarvisGopOutCurrency] != null)
                            {
                                // GOP RD
                                EntityReference gopOutCurrency = gop.GetAttributeValue<EntityReference>(Gop.JarvisGopOutCurrency);
                                if (gopOutCurrency != null)
                                {
                                    decimal exchangeValue = CrmHelper.CurrencyExchange(caseAvailableCurrency.Id, gopOutCurrency.Id, adminService);
                                    Entity gopRdUpdate = new Entity(gop.LogicalName, gop.Id);
                                    gopRdUpdate.Attributes[Gop.JarvisAmountRdCurrency] = gopOutCurrency;
                                    gopRdUpdate.Attributes[Gop.JarvisAmountRd] = caseAvailableGOPout * exchangeValue;
                                    UpdateRequest gopUpdate = new UpdateRequest { Target = gopRdUpdate };
                                    executeTransactionRequest.Requests.Add(gopUpdate);
                                }
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }

                tracingService.Trace("Completed GOP Available amount update");
                return executeTransactionRequest;
            }
            catch (InvalidPluginExecutionException ex)
            {
                throw new InvalidPluginExecutionException($"UpdateGOPAvailableAmount Method : {ex.Message}");
            }
        }

        /// <summary>
        /// Update PassOut Available Amount.
        /// </summary>
        /// <param name="adminService">Org Service.</param>
        /// <param name="caseId">Incident Id.</param>
        /// <param name="caseAvailableCurrency">total rest currency.</param>
        /// <param name="caseAvailableGOPout">case rest gop limit out.</param>
        /// <param name="executeTransactionRequest">Transaction Request.</param>
        /// <param name="tracingService">Trace service.</param>
        /// <returns>Execute Transaction Request.</returns>
        /// <exception cref="InvalidPluginExecutionException">Plugin Exception.</exception>
        public ExecuteTransactionRequest UpdatePassOutAvailableAmount(IOrganizationService adminService, Guid caseId, EntityReference caseAvailableCurrency, decimal caseAvailableGOPout, ExecuteTransactionRequest executeTransactionRequest, ITracingService tracingService)
        {
            try
            {
                tracingService.Trace("Enter into PassOut Available amount update");
                EntityCollection passOuts = adminService.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.CaseAllPassouts, caseId)));
                if (passOuts.Entities.Count > 0)
                {
                    foreach (var passOut in passOuts.Entities)
                    {
                        //// PassOut
                        if (passOut.Attributes.Contains(PassOut.TransactionCurrencyId) && passOut.Attributes[PassOut.TransactionCurrencyId] != null)
                        {
                            EntityReference passOutCurrency = passOut.GetAttributeValue<EntityReference>(PassOut.TransactionCurrencyId);
                            if (passOutCurrency != null)
                            {
                                decimal exchangeValue = CrmHelper.CurrencyExchange(caseAvailableCurrency.Id, passOutCurrency.Id, adminService);
                                Entity passOutUpdate = new Entity(passOut.LogicalName, passOut.Id);
                                passOutUpdate.Attributes[PassOut.CaseGopOutAvailableAmount] = new Money(caseAvailableGOPout * exchangeValue);
                                UpdateRequest passUpdate = new UpdateRequest { Target = passOutUpdate };
                                executeTransactionRequest.Requests.Add(passUpdate);
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                tracingService.Trace("Completed PassOut Available amount update");
                return executeTransactionRequest;
            }
            catch (InvalidPluginExecutionException ex)
            {
                throw new InvalidPluginExecutionException($"UpdatePassOutAvailableAmount Method : {ex.Message}");
            }
        }
    }
}
