// <copyright file="AccountPreOperationSync.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MCS.Jarvis.CE.Plugins
{
    using System;
    using System.Globalization;
    using global::Plugins;
    using MCS.Jarvis.CE.BusinessProcessShared.Account;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;
    using static MCS.Jarvis.CE.BusinessProcessesShared.Helpers.Constants;

    /// <summary>
    /// Incident Nature Pre Operation Delete Sync.
    /// </summary>
    public class AccountPreOperationSync : PluginBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AccountPreOperationSync"/> class.
        /// </summary>
        public AccountPreOperationSync()
            : base(typeof(AccountPreOperationSync))
        {
        }

        /// <summary>
        /// Execute CRM Plugin.
        /// </summary>
        /// <param name="localcontext">local context.</param>
        /// <exception cref="InvalidPluginExecutionException">Invalid Plugin Execution Exception.</exception>
        /// <exception cref="Exception">Exception details.</exception>
        public override void ExecuteCrmPlugin(LocalPluginContext localcontext)
        {
            ITracingService traceService = localcontext.TracingService;
            traceService.Trace("Start AccountPreOperationSync:" + DateTime.UtcNow.ToString("o", System.Globalization.CultureInfo.InvariantCulture));
            try
            {
                IPluginExecutionContext context = localcontext.PluginExecutionContext;
                IOrganizationService orgService = localcontext.OrganizationService;
                IOrganizationService adminService = localcontext.AdminOrganizationService;

                if (context.Depth > 1)
                {
                    return;
                }

                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                {
                    Entity account = (Entity)context.InputParameters["Target"];
                    if (context.MessageName.ToString().ToUpper() == "CREATE")
                    {
                        traceService.Trace("Create scenario");
                        if (account.Attributes.Contains("jarvis_address1_country") && account.Attributes["jarvis_address1_country"] != null)
                        {
                            EntityReference hdcountry = (EntityReference)account.Attributes["jarvis_address1_country"];
                            Entity country = orgService.Retrieve(hdcountry.LogicalName, hdcountry.Id, new ColumnSet("jarvis_timezone"));
                            if (country.Attributes.Contains("jarvis_timezone") && country.Attributes["jarvis_timezone"] != null)
                            {
                                account["jarvis_communicationtimezone"] = country.Attributes["jarvis_timezone"];
                                traceService.Trace("Create scenario jarvis_communicationtimezone: " + account["jarvis_communicationtimezone"]);
                            }
                        }

                        if ((account.Attributes.Contains("jarvis_openinghourtimezone") && account.Attributes["jarvis_openinghourtimezone"] == null) || !account.Attributes.Contains("jarvis_openinghourtimezone"))
                        {
                            account["jarvis_openinghourtimezone"] = 105;
                            traceService.Trace("Create scenario jarvis_openinghourtimezone: " + account["jarvis_openinghourtimezone"]);
                        }

                        ////620696- Setting Shadow Name and Account Number field based on Hidden Name and Account number field update.
                        if (account.Attributes.Contains("name"))
                        {
                            account["jarvis_nameshadow"] = (string)account.Attributes["name"];
                        }

                        if (account.Attributes.Contains("accountnumber"))
                        {
                            account["jarvis_accountnumbershadow"] = (string)account.Attributes["accountnumber"];
                        }
                        ////620696- Setting Shadow Name and Account Number field based on Hidden Name and Account number field update.

                        ////557019 - Supported currency for dealers
                        if (account.Attributes.Contains(Accounts.ExternalCurrency) && account.Attributes[Accounts.ExternalCurrency] != null)
                        {
                            EntityReference externalCurrency = (EntityReference)account.Attributes[Accounts.ExternalCurrency];
                            //// Supported currency contains data so no need to set it up.
                            if (account.Attributes.Contains(Accounts.SupportedCurrency) && account.Attributes[Accounts.SupportedCurrency] != null)
                            {
                                return;
                            }
                            else
                            {
                                AccountProcess accountProcess = new AccountProcess();
                                EntityReference supportedCurrency = accountProcess.SetSupportedCurrency(externalCurrency, adminService);
                                if (supportedCurrency != null)
                                {
                                    account[Accounts.SupportedCurrency] = supportedCurrency;
                                }
                            }
                        }
                        ////557019 - Supported currency for dealers
                    }

                    if (context.MessageName.ToString().ToUpper() == "UPDATE")
                    {
                        Entity accountImage = context.PreEntityImages["PreImage"];
                        traceService.Trace("Update scenario");
                        if (account.Attributes.Contains("jarvis_address1_country") && account.Attributes["jarvis_address1_country"] != null)
                        {
                            EntityReference hdcountry = (EntityReference)account.Attributes["jarvis_address1_country"];
                            Entity country = orgService.Retrieve(hdcountry.LogicalName, hdcountry.Id, new ColumnSet("jarvis_timezone"));
                            if (country.Attributes.Contains("jarvis_timezone") && country.Attributes["jarvis_timezone"] != null)
                            {
                                account["jarvis_communicationtimezone"] = country.Attributes["jarvis_timezone"];
                                traceService.Trace("Update scenario jarvis_communicationtimezone: " + account["jarvis_communicationtimezone"]);
                            }
                        }

                        if (((accountImage.Attributes.Contains("jarvis_openinghourtimezone") && accountImage.Attributes["jarvis_openinghourtimezone"] == null) || !accountImage.Attributes.Contains("jarvis_openinghourtimezone")) && ((account.Attributes.Contains("jarvis_openinghourtimezone") && account.Attributes["jarvis_openinghourtimezone"] == null) || !account.Attributes.Contains("jarvis_openinghourtimezone")))
                        {
                            account["jarvis_openinghourtimezone"] = 105;
                            traceService.Trace("update scenario jarvis_openinghourtimezone: " + account["jarvis_openinghourtimezone"]);
                        }
                        ////620696- Setting Shadow Name and Account Number field based on Hidden Name and Account number field update.
                        if (account.Attributes.Contains("name"))
                        {
                            account["jarvis_nameshadow"] = (string)account.Attributes["name"];
                        }

                        if (account.Attributes.Contains("accountnumber"))
                        {
                            account["jarvis_accountnumbershadow"] = (string)account.Attributes["accountnumber"];
                        }
                        ////620696- Setting Shadow Name and Account Number field based on Hidden Name and Account number field update.

                        ////557019 - Supported currency for dealers
                        if (account.Attributes.Contains(Accounts.ExternalCurrency) && account.Attributes["jarvis_externalcurrency"] != null)
                        {
                            EntityReference externalCurrency = (EntityReference)account.Attributes[Accounts.ExternalCurrency];
                            //// Supported currency contains data so no need to set it up.
                            if ((account.Attributes.Contains(Accounts.SupportedCurrency) && account.Attributes[Accounts.SupportedCurrency] != null) || (accountImage.Attributes.Contains(Accounts.SupportedCurrency) && accountImage.Attributes[Accounts.SupportedCurrency] != null))
                            {
                                return;
                            }
                            else
                            {
                                AccountProcess accountProcess = new AccountProcess();
                                EntityReference supportedCurrency = accountProcess.SetSupportedCurrency(externalCurrency, adminService);
                                if (supportedCurrency != null)
                                {
                                    account[Accounts.SupportedCurrency] = supportedCurrency;
                                }
                            }
                        }
                        ////557019 - Supported currency for dealers
                    }
                }
            }
            catch (InvalidPluginExecutionException pex)
            {
                traceService.Trace(pex.Message);
                traceService.Trace(pex.StackTrace);
                throw new InvalidPluginExecutionException("Error in AccountPreOperationSync " + pex.Message);
            }
            finally
            {
                traceService.Trace("End AccountPreOperationSync:" + DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));
            }
        }
    }
}
