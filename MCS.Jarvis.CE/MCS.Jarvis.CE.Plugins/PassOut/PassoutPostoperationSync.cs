// <copyright file="PassoutPostoperationSync.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MCS.Jarvis.CE.Plugins
{
    using System;
    using System.Linq;
    using MCS.Jarvis.CE.BusinessProcessesShared.Helpers;
    using MCS.Jarvis.CE.BusinessProcessShared.AppNotification;
    using MCS.Jarvis.CE.BusinessProcessShared.CaseMonitor;
    using MCS.jarvis.CE.BusinessProcessShared.TranslationProcess;
    using Microsoft.Crm.Sdk.Messages;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Messages;
    using Microsoft.Xrm.Sdk.Query;

    /// <summary>
    /// Pass out Post operation Sync.
    /// </summary>
    public class PassoutPostoperationSync : IPlugin
    {
        /// <summary>
        /// Execute method.
        /// </summary>
        /// <param name="serviceProvider">service Provider.</param>
        /// <exception cref="InvalidPluginExecutionException">Invalid Plugin Execution Exception.</exception>
        public void Execute(IServiceProvider serviceProvider)
        {
            // Extract the tracing service for use in debugging sandboxed plug-ins.
            ITracingService tracingService =
                (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            // Obtain the execution context from the service provider.
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            Guid initiatingUserID;
            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                try
                {
                    // region Create
                    if (context.Stage == 40 && context.MessageName.ToUpper() == "CREATE")
                    {
                        IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                        IOrganizationService service = serviceFactory.CreateOrganizationService(null);
                        IOrganizationService userService = serviceFactory.CreateOrganizationService(context.InitiatingUserId);
                        initiatingUserID = context.UserId;
                        Entity passOut = (Entity)context.InputParameters["Target"];
                        Entity passoutImg = (Entity)context.PostEntityImages["PostImage"];
                        bool isUpdate = false;
                        decimal gopLimitApprov = 0;
                        decimal exchangeValue = 0;
                        int source = 0;
                        int statusReason = 0;
                        EntityReference totalpassoutcurrency = new EntityReference();
                        EntityReference goplimitoutapprovedcurrency = new EntityReference();
                        EntityReference totalrestcurrencyout = new EntityReference();
                        int caseType = 0;
                        if (passOut.Attributes.Contains("statuscode") && passOut.Attributes["statuscode"] != null)
                        {
                            statusReason = ((OptionSetValue)passOut.Attributes["statuscode"]).Value;
                        }
                        else if (passoutImg.Attributes.Contains("statuscode") && passoutImg.Attributes["statuscode"] != null)
                        {
                            statusReason = ((OptionSetValue)passoutImg.Attributes["statuscode"]).Value;
                        }

                        if (passOut.Attributes.Contains("jarvis_source_") && passOut.Attributes["jarvis_source_"] != null)
                        {
                            source = ((OptionSetValue)passOut.Attributes["jarvis_source_"]).Value;
                        }

                        if (passOut.Attributes.Contains("jarvis_incident") && passOut.Attributes["jarvis_incident"] != null)
                        {
                            EntityReference incident = (EntityReference)passOut.Attributes["jarvis_incident"];

#pragma warning disable SA1123 // Do not place regions within elements
                            #region Pass Out Calculations
                            Entity parentCase = new Entity(incident.LogicalName);
                            ////349955 - OnCreate of PassOut With (RepairDealer,ETATime,ETADate) to check CreatedBy is Mercurius and location Not Null
                            Entity retreiveCase = service.Retrieve(incident.LogicalName, incident.Id, new ColumnSet("statuscode", "jarvis_totalgoplimitoutapproved", "jarvis_totalgoplimitoutapprovedcurrency", "jarvis_totalrestcurrencyout", "jarvis_restgoplimitout", "createdby", "casetypecode"));
#pragma warning restore SA1123 // Do not place regions within elements
                            if (retreiveCase.Attributes.Contains("casetypecode") && retreiveCase.Attributes["casetypecode"] != null)
                            {
                                caseType = ((OptionSetValue)retreiveCase.Attributes["casetypecode"]).Value;
                            }

                            if (retreiveCase.Attributes.Contains("jarvis_totalgoplimitoutapproved") && retreiveCase.Attributes["jarvis_totalgoplimitoutapproved"] != null
                                && passOut.Attributes.Contains("statuscode") && (((OptionSetValue)passOut.Attributes["statuscode"]).Value.Equals(334030001) || ((OptionSetValue)passOut.Attributes["statuscode"]).Value.Equals(334030002)))
                            {
                                decimal gopLimitOut = 0;
                                decimal outexchangeValue = 0;
                                decimal restLimit = 0;
                                gopLimitOut = (decimal)retreiveCase.Attributes["jarvis_totalgoplimitoutapproved"];
                                if (retreiveCase.Attributes.Contains("jarvis_totalgoplimitoutapprovedcurrency"))
                                {
                                    goplimitoutapprovedcurrency = (EntityReference)retreiveCase.Attributes["jarvis_totalgoplimitoutapprovedcurrency"];
                                }

                                if (retreiveCase.Attributes.Contains("jarvis_totalrestcurrencyout"))
                                {
                                    totalrestcurrencyout = (EntityReference)retreiveCase.Attributes["jarvis_totalrestcurrencyout"];
                                }

                                if (retreiveCase.Attributes.Contains("jarvis_totalpassoutcurrency"))
                                {
                                    totalpassoutcurrency = (EntityReference)retreiveCase.Attributes["jarvis_totalpassoutcurrency"];
                                }
                                else
                                {
                                    totalpassoutcurrency = (EntityReference)passOut.Attributes["transactioncurrencyid"];
                                }

                                var passoutCurrency = (EntityReference)passOut.Attributes["transactioncurrencyid"];
                                if (passOut.Attributes.Contains("transactioncurrencyid") && passOut.Attributes["transactioncurrencyid"] != null)
                                {
                                    if (retreiveCase["jarvis_totalrestcurrencyout"] == null)
                                    {
                                        parentCase["jarvis_totalrestcurrencyout"] = passoutCurrency;
                                        totalrestcurrencyout = passoutCurrency;
                                    }

                                    parentCase["jarvis_totalpassoutcurrency"] = passoutCurrency;
                                }

                                // Calculate total from Pass out amount
                                EntityCollection casePassouts = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.CaseActivePassouts, incident.Id)));
                                var uniquePassoutsperDealer = casePassouts.Entities.GroupBy(apprGOP => apprGOP.Attributes["jarvis_repairingdealer"]).Select(x => x.First()).OrderByDescending(g => g.Attributes["modifiedon"]).ToList();
                                foreach (var passout in uniquePassoutsperDealer)
                                {
                                    var passoutTotalOutput = ((Money)passout["jarvis_goplimitout"]).Value;
                                    var casepassoutCurrency = (EntityReference)passout["transactioncurrencyid"];

                                    // Convert Passout Currency to totalpassoutcurrency
                                    exchangeValue = this.CurrencyExchange(casepassoutCurrency.Id, totalpassoutcurrency.Id, service);
                                    gopLimitApprov = gopLimitApprov + (passoutTotalOutput * exchangeValue);
                                }

                                // Convert Passout Limit Out Currency to Case Approved
                                outexchangeValue = this.CurrencyExchange(totalpassoutcurrency.Id, goplimitoutapprovedcurrency.Id, service);
                                if (gopLimitOut > 0 && gopLimitOut > (gopLimitApprov * outexchangeValue))
                                {
                                    // jarvis_restgoplimitout
                                    restLimit = gopLimitOut - (gopLimitApprov * outexchangeValue);
                                }
                                else
                                {
                                    restLimit = gopLimitOut - (gopLimitApprov * outexchangeValue);
                                }

                                // Convert Case Approved Currency to Case Available Amount Currency
                                outexchangeValue = this.CurrencyExchange(goplimitoutapprovedcurrency.Id, totalrestcurrencyout.Id, service);
                                isUpdate = true;
                                parentCase["jarvis_restgoplimitout"] = restLimit * outexchangeValue;
                                parentCase["jarvis_totalpassoutamount"] = gopLimitApprov;
                            }

                            ////647796
                            if (!passOut.Attributes.Contains("jarvis_casegopoutavailableamount") && passOut.Attributes.Contains("jarvis_repairingdealer") && passOut.Attributes.Contains("transactioncurrencyid") && retreiveCase.Attributes.Contains("jarvis_restgoplimitout") && retreiveCase.Attributes.Contains("jarvis_totalrestcurrencyout") && retreiveCase.Attributes["jarvis_totalrestcurrencyout"] != null)
                            {
                                decimal caseRestGopLimitOut = ((decimal?)retreiveCase.Attributes["jarvis_restgoplimitout"]).HasValue ? (decimal)retreiveCase.Attributes["jarvis_restgoplimitout"] : 0;
                                EntityReference caseRestGOPCurrency = (EntityReference)retreiveCase.Attributes["jarvis_totalrestcurrencyout"];

                                if (passOut.Attributes["transactioncurrencyid"] != null)
                                {
                                    EntityReference passOutCurrency = passOut.GetAttributeValue<EntityReference>("transactioncurrencyid");
                                    if (passOutCurrency != null && caseRestGOPCurrency != null)
                                    {
                                        decimal passOutexchangeValue = CrmHelper.CurrencyExchange(caseRestGOPCurrency.Id, passOutCurrency.Id, service);
                                        Entity passOutUpdate = new Entity(passOut.LogicalName, passOut.Id);
                                        passOutUpdate.Attributes["jarvis_casegopoutavailableamount"] = new Money(caseRestGopLimitOut * passOutexchangeValue);
                                        service.Update(passOutUpdate);
                                    }
                                }
                            }

#pragma warning disable SA1123 // Do not place regions within elements
                            #region ETA/ATA

                            if (passOut.Attributes.Contains("jarvis_etc") && passOut.Attributes["jarvis_etc"] != null)
                            {
                                isUpdate = true;
                                parentCase["jarvis_etc"] = (DateTime)passOut.Attributes["jarvis_etc"];
                            }
#pragma warning restore SA1123 // Do not place regions within elements

                            if (passOut.Attributes.Contains("jarvis_eta") && passOut.Attributes["jarvis_eta"] != null)
                            {
                                isUpdate = true;
                                parentCase["jarvis_eta"] = (DateTime)passOut.Attributes["jarvis_eta"];
                                parentCase["jarvis_etavalidation"] = "check";
                            }

                            if (passOut.Attributes.Contains("jarvis_ata") && passOut.Attributes["jarvis_ata"] != null)
                            {
                                isUpdate = true;
                                parentCase["jarvis_ata"] = (DateTime)passOut.Attributes["jarvis_ata"];
                                parentCase["jarvis_atavalidation"] = "check";
                            }

                            if (passOut.Attributes.Contains("jarvis_atc") && passOut.Attributes["jarvis_atc"] != null)
                            {
                                isUpdate = true;
                                parentCase["jarvis_atc"] = (DateTime)passOut.Attributes["jarvis_atc"];
                            }

                            #endregion

                            #endregion

                            if (isUpdate)
                            {
                                parentCase.Id = incident.Id;
                                userService.Update(parentCase);
                            }

#pragma warning disable SA1123 // Do not place regions within elements
                            #region FU RD Approval Monitor Action
                            if (retreiveCase.Attributes.Contains("statuscode") && retreiveCase.Attributes["statuscode"] != null)
                            {
                                OptionSetValue caseStatus = (OptionSetValue)retreiveCase.Attributes["statuscode"];
                                if (caseStatus.Value == 30 && statusReason == 1)
                                {
                                    // Pass Out
#pragma warning disable SA1123 // Do not place regions within elements
                                    #region Case Monitor Action - Pass Out

                                    if (passoutImg.Attributes.Contains("jarvis_repairingdealer") && passoutImg.Attributes["jarvis_repairingdealer"] != null)
                                    {
#pragma warning disable SA1123 // Do not place regions within elements
                                        #region Draft Monitor Actions - FU RD

                                        EntityReference homeDealer = (EntityReference)passoutImg.Attributes["jarvis_repairingdealer"];
#pragma warning restore SA1123 // Do not place regions within elements
                                        Entity account = service.Retrieve(homeDealer.LogicalName, homeDealer.Id, new ColumnSet("jarvis_language", "jarvis_address1_country"));
                                        string isoLangCode = string.Empty;
                                        string isCountryCode = string.Empty;
                                        if (account.Attributes.Contains("jarvis_address1_country") && account.Attributes["jarvis_address1_country"] != null)
                                        {
                                            EntityReference hdcountry = (EntityReference)account.Attributes["jarvis_address1_country"];
                                            Entity country = service.Retrieve(hdcountry.LogicalName, hdcountry.Id, new ColumnSet("jarvis_iso2countrycode"));
                                            if (country.Attributes.Contains("jarvis_iso2countrycode") && country.Attributes["jarvis_iso2countrycode"] != null)
                                            {
                                                isCountryCode = (string)country.Attributes["jarvis_iso2countrycode"];
                                            }
                                        }

                                        if (account.Attributes.Contains("jarvis_language") && account.Attributes["jarvis_language"] != null)
                                        {
                                            EntityReference language = (EntityReference)account.Attributes["jarvis_language"];
                                            Entity hdlanguage = service.Retrieve(language.LogicalName, language.Id, new ColumnSet("jarvis_iso3languagecode6392t"));
                                            if (hdlanguage.Attributes.Contains("jarvis_iso3languagecode6392t") && hdlanguage.Attributes["jarvis_iso3languagecode6392t"] != null)
                                            {
                                                isoLangCode = (string)hdlanguage.Attributes["jarvis_iso3languagecode6392t"];
                                            }
                                        }

                                        if (!string.IsNullOrEmpty(isoLangCode) || !string.IsNullOrEmpty(isCountryCode))
                                        {
                                            CaseMonitorProcess op = new CaseMonitorProcess();
                                            string fucomment = isoLangCode + " " + isCountryCode + " " + "FU RD on PO Approval";
                                            op.AutomateMonitorCreation(retreiveCase, fucomment, 2, 4, 15, " ", userService);
                                        }

                                        #endregion
                                    }
#pragma warning restore SA1123 // Do not place regions within elements

                                    #endregion
                                }
                            }
#pragma warning restore SA1123 // Do not place regions within elements
                            #endregion

#pragma warning disable SA1123 // Do not place regions within elements
                            #region Pass Out Translation
                            bool isAutomation = false;
#pragma warning restore SA1123 // Do not place regions within elements
                            isAutomation = CrmHelper.GetAutomationConfig(service, MCS.Jarvis.CE.Plugins.Constants.JarvisConfiguration.Automationtranslation, tracingService);
                            if (isAutomation)
                            {
                                if (passOut.Contains("jarvis_reason") && passOut.Attributes["jarvis_reason"] != null)
                                {
                                    Entity passOutUpdate = new Entity(passOut.LogicalName);
                                    passOutUpdate.Id = passOut.Id;
                                    TranslationProcess operations = new TranslationProcess();
                                    operations.PassOutStandardProcess(service, tracingService, retreiveCase, context.InitiatingUserId, passOut);
                                    Entity caseUpdate = service.Retrieve(passOut.LogicalName, passOut.Id, new ColumnSet("jarvis_translationsstatusdelayedetareason"));
                                    OptionSetValue translationStatusLoc = (OptionSetValue)caseUpdate.Attributes["jarvis_translationsstatusdelayedetareason"];
                                    if (translationStatusLoc.Value != 334030001)
                                    {
                                        // In-Progress
                                        passOutUpdate["jarvis_translationsstatusdelayedetareason"] = new OptionSetValue(334030001);
                                        userService.Update(passOutUpdate);
                                    }
                                }
                            }
                            #endregion

#pragma warning disable SA1123 // Do not place regions within elements
                            #region Set RD=HD

                            // Update Case jarvis RDHD field
                            if (passOut.Attributes.Contains("jarvis_repairingdealer") && passOut.Attributes["jarvis_repairingdealer"] != null)
                            {
                                this.SetCaseRDHD(service, incident);
                            }
#pragma warning restore SA1123 // Do not place regions within elements
                            #endregion

#pragma warning disable SA1123 // Do not place regions within elements
                            #region Set Appointment Fields
                            if (passoutImg.Attributes.Contains("jarvis_incident") && passoutImg.Attributes["jarvis_incident"] != null)
                            {
                                if (passOut.Attributes.Contains("jarvis_repairingdealer") || passOut.Attributes.Contains("jarvis_etatime") || passOut.Attributes.Contains("jarvis_etadate"))
                                {
                                    OptionSetValue isMercuriusMode = new OptionSetValue();
                                    isMercuriusMode = CrmHelper.GetReleaseAutomationConfig(service, MCS.Jarvis.CE.Plugins.Constants.JarvisConfiguration.Automationreleasecase, tracingService);
                                    if (isMercuriusMode.Value == 3)
                                    {
                                        // OneCaseOnly
                                        this.AppointmentUpdate(passoutImg, service, retreiveCase, incident, tracingService);
                                    }
                                }
                            }
#pragma warning restore SA1123 // Do not place regions within elements
                            #endregion

#pragma warning disable SA1123 // Do not place regions within elements
                            #region Close Monitor Actions

                            CaseMonitorProcess operation = new CaseMonitorProcess();
#pragma warning restore SA1123 // Do not place regions within elements
                            string fucomments = "Pass Out,Confirm case opening";
                            operation.AutomateCloseMonitorActions(retreiveCase, fucomments, 1, fucomments, userService);

                            #endregion

#pragma warning disable SA1123 // Do not place regions within elements
                            #region Monitor Action Creation - ETA

                            if (passoutImg.Attributes.Contains("statuscode") && passoutImg.Attributes["statuscode"] != null)
                            {
                                if (statusReason == 334030001)
                                {
                                    if (retreiveCase.Attributes.Contains("statuscode") && retreiveCase.Attributes["statuscode"] != null)
                                    {
                                        OptionSetValue caseStatus = (OptionSetValue)retreiveCase.Attributes["statuscode"];
                                        if (caseStatus.Value == 30)
                                        {
                                            // Pass Out
#pragma warning disable SA1123 // Do not place regions within elements
                                            #region Case Monitor Action - Pass Out

                                            if (passoutImg.Attributes.Contains("jarvis_repairingdealer") && passoutImg.Attributes["jarvis_repairingdealer"] != null)
                                            {
                                                EntityReference homeDealer = (EntityReference)passoutImg.Attributes["jarvis_repairingdealer"];
                                                EntityCollection getMonitorActionsForCase = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getPassOutsETA, incident.Id)));
                                                EntityCollection getMonitorActionsForPassOut = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getPassOutsDraft, incident.Id)));

#pragma warning disable SA1123 // Do not place regions within elements
                                                #region Sent Monitor Actions
                                                if (getMonitorActionsForCase != null && getMonitorActionsForCase.Entities.Count > 0)
                                                {
                                                    Entity account = service.Retrieve(homeDealer.LogicalName, homeDealer.Id, new ColumnSet("jarvis_language", "jarvis_address1_country"));
                                                    string isoLangCode = string.Empty;
                                                    string isCountryCode = string.Empty;
                                                    if (account.Attributes.Contains("jarvis_address1_country") && account.Attributes["jarvis_address1_country"] != null)
                                                    {
                                                        EntityReference hdcountry = (EntityReference)account.Attributes["jarvis_address1_country"];
                                                        Entity country = service.Retrieve(hdcountry.LogicalName, hdcountry.Id, new ColumnSet("jarvis_iso2countrycode"));
                                                        if (country.Attributes.Contains("jarvis_iso2countrycode") && country.Attributes["jarvis_iso2countrycode"] != null)
                                                        {
                                                            isCountryCode = (string)country.Attributes["jarvis_iso2countrycode"];
                                                        }
                                                    }

                                                    if (account.Attributes.Contains("jarvis_language") && account.Attributes["jarvis_language"] != null)
                                                    {
                                                        EntityReference language = (EntityReference)account.Attributes["jarvis_language"];
                                                        Entity hdlanguage = service.Retrieve(language.LogicalName, language.Id, new ColumnSet("jarvis_iso3languagecode6392t"));
                                                        if (hdlanguage.Attributes.Contains("jarvis_iso3languagecode6392t") && hdlanguage.Attributes["jarvis_iso3languagecode6392t"] != null)
                                                        {
                                                            isoLangCode = (string)hdlanguage.Attributes["jarvis_iso3languagecode6392t"];
                                                        }
                                                    }

                                                    if (!string.IsNullOrEmpty(isoLangCode) || !string.IsNullOrEmpty(isCountryCode))
                                                    {
                                                        CaseMonitorProcess operations = new CaseMonitorProcess();
                                                        string fucomment = isoLangCode + " " + isCountryCode + " " + "Chase ETA";
                                                        operations.AutomateMonitorCreation(retreiveCase, fucomment, 2, 4, 15, " ", userService);
                                                    }
                                                }
#pragma warning restore SA1123 // Do not place regions within elements
                                                #endregion
                                            }
#pragma warning restore SA1123 // Do not place regions within elements

                                            #endregion
                                        }
                                    }
                                }
                            }
#pragma warning restore SA1123 // Do not place regions within elements
                            #endregion

#pragma warning disable SA1123 // Do not place regions within elements
                            #region Chase ATA_US_335230_5.9.1.1
                            if (passOut.Attributes.Contains("jarvis_eta") && passOut.Attributes["jarvis_eta"] != null && !passOut.Attributes.Contains("jarvis_ata") && caseType == 2)
                            {
                                tracingService.Trace("inside Chase ATA when initial eta provided");
                                DateTime initialEta = (DateTime)passOut.Attributes["jarvis_eta"];
                                EntityReference homeDealer = (EntityReference)passoutImg.Attributes["jarvis_repairingdealer"];
                                Entity account = service.Retrieve(homeDealer.LogicalName, homeDealer.Id, new ColumnSet("jarvis_language", "jarvis_address1_country"));
                                string isoLangCode = string.Empty;
                                string isCountryCode = string.Empty;
                                if (account.Attributes.Contains("jarvis_address1_country") && account.Attributes["jarvis_address1_country"] != null)
                                {
                                    EntityReference hdcountry = (EntityReference)account.Attributes["jarvis_address1_country"];
                                    Entity country = service.Retrieve(hdcountry.LogicalName, hdcountry.Id, new ColumnSet("jarvis_iso2countrycode"));
                                    if (country.Attributes.Contains("jarvis_iso2countrycode") && country.Attributes["jarvis_iso2countrycode"] != null)
                                    {
                                        isCountryCode = (string)country.Attributes["jarvis_iso2countrycode"];
                                    }
                                }

                                if (account.Attributes.Contains("jarvis_language") && account.Attributes["jarvis_language"] != null)
                                {
                                    EntityReference language = (EntityReference)account.Attributes["jarvis_language"];
                                    Entity hdlanguage = service.Retrieve(language.LogicalName, language.Id, new ColumnSet("jarvis_iso3languagecode6392t"));
                                    if (hdlanguage.Attributes.Contains("jarvis_iso3languagecode6392t") && hdlanguage.Attributes["jarvis_iso3languagecode6392t"] != null)
                                    {
                                        isoLangCode = (string)hdlanguage.Attributes["jarvis_iso3languagecode6392t"];
                                    }
                                }

                                if (!string.IsNullOrEmpty(isoLangCode) || !string.IsNullOrEmpty(isCountryCode))
                                {
                                    tracingService.Trace("creating MA for Chase ATA");
                                    CaseMonitorProcess operationata = new CaseMonitorProcess();
                                    string fucomment = isoLangCode + " " + isCountryCode + " " + "Chase ATA";
                                    //operationata.AutomateMonitorCreationTime(retreiveCase, fucomment, 1, 8, 0, " ", initialEta, 30, userService);
                                    operationata.AutomateMonitorCreationTime(retreiveCase, fucomment, 2, 17, 0, " ", initialEta, 30, userService);

                                }
                            }
#pragma warning restore SA1123 // Do not place regions within elements

                            #endregion

#pragma warning disable SA1123 // Do not place regions within elements
                            #region Stage Changes
                            if (passOut.Contains("statuscode") && passOut.Attributes["statuscode"] != null)
                            {
                                OptionSetValue status = (OptionSetValue)passOut.Attributes["statuscode"];
                                if (status.Value == 334030001)
                                {
                                    // Send
                                    Entity parentCaseUpdated = service.Retrieve(incident.LogicalName, incident.Id, new ColumnSet("jarvis_atc", "jarvis_etc", "jarvis_eta", "jarvis_ata", "jarvis_totalgoplimitoutapproved", "jarvis_totalgoplimitoutapprovedcurrency", "jarvis_totalrestcurrencyout", "jarvis_eta", "jarvis_totalpassoutamount", "jarvis_totalpassoutcurrency", "statuscode"));

                                    if (parentCaseUpdated.Attributes.Contains("statuscode") && parentCaseUpdated.Attributes["statuscode"] != null)
                                    {
                                        OptionSetValue casestatus = (OptionSetValue)parentCaseUpdated.Attributes["statuscode"];
                                        if (casestatus.Value == 30)
                                        {
                                            // PassOut
                                            this.UpdateCaseStage(parentCaseUpdated, incident, serviceFactory, userService, tracingService, initiatingUserID);
                                        }
                                    }
                                }
                            }
#pragma warning restore SA1123 // Do not place regions within elements

                            #endregion
                        }
                    }

                    // endregion
                    // region Update
                    if (context.Stage == 40 && context.MessageName.ToUpper() == "UPDATE")
                    {
                        tracingService.Trace("Enter in update");
                        IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                        IOrganizationService service = serviceFactory.CreateOrganizationService(null);
                        IOrganizationService userService = serviceFactory.CreateOrganizationService(context.InitiatingUserId);
                        initiatingUserID = context.UserId;
                        Entity passOut = (Entity)context.InputParameters["Target"];
                        Entity passoutImg = (Entity)context.PostEntityImages["PostImage"];
                        Entity passoutPreImg = (Entity)context.PreEntityImages["PreImage"];
                        bool isUpdate = false;
                        decimal gopLimitApprov = 0;
                        decimal exchangeValue = 0;
                        int statusReason = 0;
                        bool customerContactHasMobile = false;
                        EntityReference passoutCurrency = new EntityReference();
                        EntityReference incident = (EntityReference)passoutImg.Attributes["jarvis_incident"];
                        int source = 0;
                        int caseType = 0;

#pragma warning disable SA1123 // Do not place regions within elements
                        #region Pass Out Calculations

                        if (passOut.Attributes.Contains("statuscode") && passOut.Attributes["statuscode"] != null)
                        {
                            statusReason = ((OptionSetValue)passOut.Attributes["statuscode"]).Value;
                        }
                        else if (passoutImg.Attributes.Contains("statuscode") && passoutImg.Attributes["statuscode"] != null)
                        {
                            statusReason = ((OptionSetValue)passoutImg.Attributes["statuscode"]).Value;
                        }
#pragma warning restore SA1123 // Do not place regions within elements

                        if (passOut.Attributes.Contains("transactioncurrencyid") && passOut.Attributes["transactioncurrencyid"] != null)
                        {
                            passoutCurrency = (EntityReference)passOut.Attributes["transactioncurrencyid"];
                        }
                        else if (passoutImg.Attributes.Contains("transactioncurrencyid") && passoutImg.Attributes["transactioncurrencyid"] != null)
                        {
                            passoutCurrency = (EntityReference)passoutImg.Attributes["transactioncurrencyid"];
                        }

                        if (passOut.Attributes.Contains("jarvis_source_") && passOut.Attributes["jarvis_source_"] != null)
                        {
                            source = ((OptionSetValue)passOut.Attributes["jarvis_source_"]).Value;
                        }
                        else if (passoutImg.Attributes.Contains("jarvis_source_") && passoutImg.Attributes["jarvis_source_"] != null)
                        {
                            source = ((OptionSetValue)passoutImg.Attributes["jarvis_source_"]).Value;
                        }

                        tracingService.Trace("Got References");
                        Entity parentCase = new Entity(incident.LogicalName);
                        Entity retreiveCase = service.Retrieve(incident.LogicalName, incident.Id, new ColumnSet("jarvis_homedealer", "statuscode", "jarvis_assistancetype", "jarvis_totalgoplimitoutapproved", "jarvis_totalgoplimitoutapprovedcurrency", "jarvis_totalrestcurrencyout", "jarvis_totalpassoutcurrency", "createdby", "customerid", "jarvis_atc", "casetypecode"));
                        if (retreiveCase.Attributes.Contains("casetypecode") && retreiveCase.Attributes["casetypecode"] != null)
                        {
                            caseType = ((OptionSetValue)retreiveCase.Attributes["casetypecode"]).Value;
                        }

                        if ((passOut.Attributes.Contains("jarvis_goplimitout") || (passOut.Attributes.Contains("transactioncurrencyid")) || ((passOut.Attributes.Contains("statuscode")) && ((OptionSetValue)passOut.Attributes["statuscode"]).Value.Equals(334030001))) && retreiveCase.Attributes.Contains("jarvis_totalgoplimitoutapproved") && retreiveCase.Attributes["jarvis_totalgoplimitoutapproved"] != null && (statusReason.Equals(334030001) || statusReason.Equals(334030002) || statusReason.Equals(2)))
                        {
                            decimal gopLimitOut = 0;
                            gopLimitOut = (decimal)retreiveCase.Attributes["jarvis_totalgoplimitoutapproved"];
                            decimal outexchangeValue = 0;
                            decimal restLimit = 0;
                            EntityReference goplimitoutapprovedcurrency = (EntityReference)retreiveCase.Attributes["jarvis_totalgoplimitoutapprovedcurrency"];
                            EntityReference totalrestcurrencyout = (EntityReference)retreiveCase.Attributes["jarvis_totalrestcurrencyout"];
                            tracingService.Trace("Passout Calculation initiated");
                            EntityReference totalpassoutcurrency = new EntityReference();
                            if (retreiveCase.Attributes.Contains("jarvis_totalpassoutcurrency"))
                            {
                                totalpassoutcurrency = (EntityReference)retreiveCase.Attributes["jarvis_totalpassoutcurrency"];
                            }
                            else
                            {
                                totalpassoutcurrency = passoutCurrency;
                            }

                            if (passoutCurrency != null)
                            {
                                if (retreiveCase["jarvis_totalrestcurrencyout"] == null)
                                {
                                    parentCase["jarvis_totalrestcurrencyout"] = passoutCurrency;
                                    totalrestcurrencyout = passoutCurrency;
                                }

                                parentCase["jarvis_totalpassoutcurrency"] = passoutCurrency;
                            }

                            tracingService.Trace("Got Total Rest Limits records");

                            // Calculate total from Pass out amount
                            EntityCollection casePassouts = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.CaseActivePassouts, incident.Id)));
                            tracingService.Trace("Got casePassouts records");
                            if (casePassouts.Entities.Count > 0)
                            {
                                tracingService.Trace("Count of casePassouts records " + casePassouts.Entities.Count + " ");
                                var uniquePassoutsperDealer = casePassouts.Entities.GroupBy(apprGOP => apprGOP.Attributes["jarvis_repairingdealer"]).Select(x => x.First()).OrderByDescending(g => g.Attributes["modifiedon"]).ToList();
                                foreach (var passout in uniquePassoutsperDealer)
                                {
                                    var passoutTotalOutput = ((Money)passout["jarvis_goplimitout"]).Value;
                                    var casepassoutCurrency = (EntityReference)passout["transactioncurrencyid"];
                                    if (statusReason.Equals(2))
                                    {
                                        // Canceled
                                        parentCase["jarvis_totalpassoutcurrency"] = casepassoutCurrency;
                                        parentCase["jarvis_totalrestcurrencyout"] = casepassoutCurrency;
                                    }

                                    // Convert Passout Currency to totalpassoutcurrency
                                    tracingService.Trace("Getting Rates");
                                    exchangeValue = this.CurrencyExchange(casepassoutCurrency.Id, totalpassoutcurrency.Id, service);
                                    gopLimitApprov = gopLimitApprov + (passoutTotalOutput * exchangeValue);
                                    tracingService.Trace("Got Rates");
                                }

                                // Calculate the Available GOp Amount on Case
                                tracingService.Trace("outexchangeValue");
                                outexchangeValue = this.CurrencyExchange(totalpassoutcurrency.Id, goplimitoutapprovedcurrency.Id, service);
                                tracingService.Trace("Got outexchangeValue");
                                if (gopLimitOut > 0 && gopLimitOut > (gopLimitApprov * outexchangeValue))
                                {
                                    // jarvis_restgoplimitout
                                    restLimit = gopLimitOut - (gopLimitApprov * outexchangeValue);
                                }
                                else
                                {
                                    restLimit = gopLimitOut - (gopLimitApprov * outexchangeValue);
                                }

                                // Convert Case Approved Currency to Case Available Amount Currency
                                tracingService.Trace("outexchangeValue1");
                                outexchangeValue = this.CurrencyExchange(goplimitoutapprovedcurrency.Id, totalrestcurrencyout.Id, service);
                                tracingService.Trace("Got outexchangeValue 1");
                                isUpdate = true;
                                parentCase["jarvis_restgoplimitout"] = restLimit * outexchangeValue;
                                parentCase["jarvis_totalpassoutamount"] = gopLimitApprov;
                            }
                            else
                            {
                                parentCase["jarvis_totalpassoutamount"] = null;
                                parentCase["jarvis_totalpassoutcurrency"] = null;
                                parentCase["jarvis_restgoplimitout"] = gopLimitOut;
                                parentCase["jarvis_totalrestcurrencyout"] = goplimitoutapprovedcurrency;
                                isUpdate = true;
                            }
                        }

#pragma warning disable SA1123 // Do not place regions within elements
                        #region Monitor Action Creation

                        if (passOut.Attributes.Contains("statuscode") && passOut.Attributes["statuscode"] != null)
                        {
                            if (statusReason == 334030001)
                            {
                                if (retreiveCase.Attributes.Contains("statuscode") && retreiveCase.Attributes["statuscode"] != null)
                                {
                                    OptionSetValue caseStatus = (OptionSetValue)retreiveCase.Attributes["statuscode"];
                                    if (caseStatus.Value == 30)
                                    {
                                        // Pass Out
#pragma warning disable SA1123 // Do not place regions within elements
                                        #region Case Monitor Action - Pass Out

                                        if (passoutImg.Attributes.Contains("jarvis_repairingdealer") && passoutImg.Attributes["jarvis_repairingdealer"] != null)
                                        {
                                            EntityCollection getMonitorActionsForCase = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getPassOutsETA, incident.Id)));
                                            EntityCollection getMonitorActionsForPassOut = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getPassOutsDraft, incident.Id)));

#pragma warning disable SA1123 // Do not place regions within elements
                                            #region Sent Monitor Actions
                                            if (getMonitorActionsForCase != null && getMonitorActionsForCase.Entities.Count > 0)
                                            {
                                                EntityReference homeDealer = (EntityReference)passoutImg.Attributes["jarvis_repairingdealer"];
                                                Entity account = service.Retrieve(homeDealer.LogicalName, homeDealer.Id, new ColumnSet("jarvis_language", "jarvis_address1_country"));
                                                string isoLangCode = string.Empty;
                                                string isCountryCode = string.Empty;
                                                if (account.Attributes.Contains("jarvis_address1_country") && account.Attributes["jarvis_address1_country"] != null)
                                                {
                                                    EntityReference hdcountry = (EntityReference)account.Attributes["jarvis_address1_country"];
                                                    Entity country = service.Retrieve(hdcountry.LogicalName, hdcountry.Id, new ColumnSet("jarvis_iso2countrycode"));
                                                    if (country.Attributes.Contains("jarvis_iso2countrycode") && country.Attributes["jarvis_iso2countrycode"] != null)
                                                    {
                                                        isCountryCode = (string)country.Attributes["jarvis_iso2countrycode"];
                                                    }
                                                }

                                                if (account.Attributes.Contains("jarvis_language") && account.Attributes["jarvis_language"] != null)
                                                {
                                                    EntityReference language = (EntityReference)account.Attributes["jarvis_language"];
                                                    Entity hdlanguage = service.Retrieve(language.LogicalName, language.Id, new ColumnSet("jarvis_iso3languagecode6392t"));
                                                    if (hdlanguage.Attributes.Contains("jarvis_iso3languagecode6392t") && hdlanguage.Attributes["jarvis_iso3languagecode6392t"] != null)
                                                    {
                                                        isoLangCode = (string)hdlanguage.Attributes["jarvis_iso3languagecode6392t"];
                                                    }
                                                }

                                                if (!string.IsNullOrEmpty(isoLangCode) || !string.IsNullOrEmpty(isCountryCode))
                                                {
                                                    CaseMonitorProcess operation = new CaseMonitorProcess();
                                                    string fucomment = isoLangCode + " " + isCountryCode + " " + "Chase ETA";
                                                    operation.AutomateMonitorCreation(retreiveCase, fucomment, 2, 4, 15, " ", userService);
                                                }
                                            }
#pragma warning restore SA1123 // Do not place regions within elements
                                            #endregion
                                        }
#pragma warning restore SA1123 // Do not place regions within elements

                                        #endregion
                                    }
                                }
                            }
                        }
#pragma warning restore SA1123 // Do not place regions within elements

                        #endregion

#pragma warning disable SA1123 // Do not place regions within elements
                        #region ETA/ATA US_248390_5.3.2

                        // get CustomerContact
                        tracingService.Trace("inside 5.3.1_MonitorActionETA");
#pragma warning restore SA1123 // Do not place regions within elements
                        EntityCollection getContactwithMobile = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.caseContactHasMobile, retreiveCase.Id)));
                        if (getContactwithMobile.Entities.Count > 0)
                        {
                            customerContactHasMobile = true;
                            tracingService.Trace("customer has mobile count:" + getContactwithMobile.Entities.Count);
                        }

                        tracingService.Trace("customerContactHasMobile:" + customerContactHasMobile);

                        if (!customerContactHasMobile && passOut.Attributes.Contains("jarvis_etc") && passOut.Attributes["jarvis_etc"] != null && !retreiveCase.Attributes.Contains("jarvis_atc") && !passOut.Attributes.Contains("jarvis_atc"))
                        {
                            isUpdate = true;
                            parentCase["jarvis_etc"] = (DateTime)passOut.Attributes["jarvis_etc"];
                            tracingService.Trace("Inside etc has value and eta not");

#pragma warning disable SA1123 // Do not place regions within elements
                            #region Case Monitor Action Automation -  ETC

                            if (retreiveCase.Attributes.Contains("statuscode") && retreiveCase.Attributes["statuscode"] != null)
                            {
                                OptionSetValue casestatusReason = (OptionSetValue)retreiveCase.Attributes["statuscode"];
                                tracingService.Trace("Case status reason:" + casestatusReason.Value);
                                if (casestatusReason.Value == 50 || casestatusReason.Value == 60 || casestatusReason.Value == 70)
                                {
                                    if (retreiveCase.Attributes.Contains("customerid") && retreiveCase.Attributes["customerid"] != null)
                                    {
                                        // jarvis_callerlanguage
                                        EntityReference customer = (EntityReference)retreiveCase.Attributes["customerid"];
                                        Entity account = service.Retrieve(customer.LogicalName, customer.Id, new ColumnSet("jarvis_language", "jarvis_address1_country"));
                                        string isoLangCode = string.Empty;
                                        string isCountryCode = string.Empty;
                                        if (account.Attributes.Contains("jarvis_address1_country") && account.Attributes["jarvis_address1_country"] != null)
                                        {
                                            EntityReference hdcountry = (EntityReference)account.Attributes["jarvis_address1_country"];
                                            Entity country = service.Retrieve(hdcountry.LogicalName, hdcountry.Id, new ColumnSet("jarvis_iso2countrycode"));
                                            if (country.Attributes.Contains("jarvis_iso2countrycode") && country.Attributes["jarvis_iso2countrycode"] != null)
                                            {
                                                isCountryCode = (string)country.Attributes["jarvis_iso2countrycode"];
                                            }
                                        }

                                        if (account.Attributes.Contains("jarvis_language") && account.Attributes["jarvis_language"] != null)
                                        {
                                            EntityReference language = (EntityReference)account.Attributes["jarvis_language"];
                                            Entity hdlanguage = service.Retrieve(language.LogicalName, language.Id, new ColumnSet("jarvis_iso3languagecode6392t"));
                                            if (hdlanguage.Attributes.Contains("jarvis_iso3languagecode6392t") && hdlanguage.Attributes["jarvis_iso3languagecode6392t"] != null)
                                            {
                                                isoLangCode = (string)hdlanguage.Attributes["jarvis_iso3languagecode6392t"];
                                            }
                                        }

                                        if (!string.IsNullOrEmpty(isoLangCode) || !string.IsNullOrEmpty(isCountryCode))
                                        {
                                            tracingService.Trace("creating MA for Pass ETC");
                                            CaseMonitorProcess operation = new CaseMonitorProcess();
                                            string fucomment = isoLangCode + " " + isCountryCode + " " + "Pass ETC";
                                            operation.AutomateMonitorCreationTime(retreiveCase, fucomment, 2, 9, 0, " ", DateTime.UtcNow, 0, userService);
                                        }
                                    }
                                }
                            }
#pragma warning restore SA1123 // Do not place regions within elements

                            #endregion
                        }

                        if (passOut.Attributes.Contains("jarvis_eta") && passOut.Attributes["jarvis_eta"] != null)
                        {
                            isUpdate = true;
                            parentCase["jarvis_eta"] = (DateTime)passOut.Attributes["jarvis_eta"];
                            parentCase["jarvis_etavalidation"] = "check";
                        }
                        else if (passOut.Attributes.Contains("jarvis_eta") && passOut.Attributes["jarvis_eta"] == null && passoutPreImg.Attributes.Contains("jarvis_eta") && passoutPreImg.Attributes["jarvis_eta"] != null)
                        {
                            isUpdate = true;
                            parentCase["jarvis_eta"] = null;
                        }

                        if (passOut.Attributes.Contains("jarvis_ata") && passOut.Attributes["jarvis_ata"] != null)
                        {
                            isUpdate = true;
                            parentCase["jarvis_ata"] = (DateTime)passOut.Attributes["jarvis_ata"];
                            parentCase["jarvis_atavalidation"] = "check";
                        }

                        if (passOut.Attributes.Contains("jarvis_etc") && passOut.Attributes["jarvis_etc"] != null)
                        {
                            isUpdate = true;
                            parentCase["jarvis_etc"] = (DateTime)passOut.Attributes["jarvis_etc"];
                        }

                        if (passOut.Attributes.Contains("jarvis_atc") && passOut.Attributes["jarvis_atc"] != null)
                        {
                            isUpdate = true;
                            parentCase["jarvis_atc"] = (DateTime)passOut.Attributes["jarvis_atc"];
                        }
                        else
                        {
                            parentCase.Attributes.Remove("jarvis_atc");
                        }

                        #endregion
                        #endregion

#pragma warning disable SA1123 // Do not place regions within elements
                        #region Chase ATA_US_335230_5.9.1.1
                        if (passOut.Attributes.Contains("jarvis_eta") && passOut.Attributes["jarvis_eta"] != null && !passOut.Attributes.Contains("jarvis_ata") && !passoutPreImg.Contains("jarvis_eta") && caseType == 2)
                        {
                            tracingService.Trace("inside Chase ATA when initial eta provided");
                            DateTime initialEta = (DateTime)passOut.Attributes["jarvis_eta"];
                            EntityReference homeDealer = (EntityReference)passoutImg.Attributes["jarvis_repairingdealer"];
                            Entity account = service.Retrieve(homeDealer.LogicalName, homeDealer.Id, new ColumnSet("jarvis_language", "jarvis_address1_country"));
                            string isoLangCode = string.Empty;
                            string isCountryCode = string.Empty;
                            if (account.Attributes.Contains("jarvis_address1_country") && account.Attributes["jarvis_address1_country"] != null)
                            {
                                EntityReference hdcountry = (EntityReference)account.Attributes["jarvis_address1_country"];
                                Entity country = service.Retrieve(hdcountry.LogicalName, hdcountry.Id, new ColumnSet("jarvis_iso2countrycode"));
                                if (country.Attributes.Contains("jarvis_iso2countrycode") && country.Attributes["jarvis_iso2countrycode"] != null)
                                {
                                    isCountryCode = (string)country.Attributes["jarvis_iso2countrycode"];
                                }
                            }

                            if (account.Attributes.Contains("jarvis_language") && account.Attributes["jarvis_language"] != null)
                            {
                                EntityReference language = (EntityReference)account.Attributes["jarvis_language"];
                                Entity hdlanguage = service.Retrieve(language.LogicalName, language.Id, new ColumnSet("jarvis_iso3languagecode6392t"));
                                if (hdlanguage.Attributes.Contains("jarvis_iso3languagecode6392t") && hdlanguage.Attributes["jarvis_iso3languagecode6392t"] != null)
                                {
                                    isoLangCode = (string)hdlanguage.Attributes["jarvis_iso3languagecode6392t"];
                                }
                            }

                            if (!string.IsNullOrEmpty(isoLangCode) || !string.IsNullOrEmpty(isCountryCode))
                            {
                                tracingService.Trace("creating MA for Chase ATA");
                                CaseMonitorProcess operation = new CaseMonitorProcess();
                                string fucomment = isoLangCode + " " + isCountryCode + " " + "Chase ATA";
                                //operation.AutomateMonitorCreationTime(retreiveCase, fucomment, 1, 8, 0, " ", initialEta, 30, userService);
                                operation.AutomateMonitorCreationTime(retreiveCase, fucomment, 2, 17, 0, " ", initialEta, 30, userService);

                            }
                        }
#pragma warning restore SA1123 // Do not place regions within elements

                        #endregion

#pragma warning disable SA1123 // Do not place regions within elements
                        #region Chase ATA_US_335230 ETA less than 60 minutes
                        if (passOut.Attributes.Contains("jarvis_eta") && passOut.Attributes["jarvis_eta"] != null && !passOut.Attributes.Contains("jarvis_ata") && passoutPreImg.Contains("jarvis_eta") && passoutPreImg.Attributes["jarvis_eta"] != null && caseType == 2)
                        {
                            DateTime currentEta = (DateTime)passOut.Attributes["jarvis_eta"];
                            DateTime previousEta = (DateTime)passoutPreImg.Attributes["jarvis_eta"];
                            TimeSpan diff = currentEta - previousEta;
                            tracingService.Trace("diff total hours:" + diff.TotalHours);
                            tracingService.Trace("diff total minutes" + diff.TotalMinutes);

                            EntityReference homeDealer = (EntityReference)passoutImg.Attributes["jarvis_repairingdealer"];
                            Entity account = service.Retrieve(homeDealer.LogicalName, homeDealer.Id, new ColumnSet("jarvis_language", "jarvis_address1_country"));
                            string isoLangCode = string.Empty;
                            string isCountryCode = string.Empty;
                            if (account.Attributes.Contains("jarvis_address1_country") && account.Attributes["jarvis_address1_country"] != null)
                            {
                                EntityReference hdcountry = (EntityReference)account.Attributes["jarvis_address1_country"];
                                Entity country = service.Retrieve(hdcountry.LogicalName, hdcountry.Id, new ColumnSet("jarvis_iso2countrycode"));
                                if (country.Attributes.Contains("jarvis_iso2countrycode") && country.Attributes["jarvis_iso2countrycode"] != null)
                                {
                                    isCountryCode = (string)country.Attributes["jarvis_iso2countrycode"];
                                }
                            }

                            if (account.Attributes.Contains("jarvis_language") && account.Attributes["jarvis_language"] != null)
                            {
                                EntityReference language = (EntityReference)account.Attributes["jarvis_language"];
                                Entity hdlanguage = service.Retrieve(language.LogicalName, language.Id, new ColumnSet("jarvis_iso3languagecode6392t"));
                                if (hdlanguage.Attributes.Contains("jarvis_iso3languagecode6392t") && hdlanguage.Attributes["jarvis_iso3languagecode6392t"] != null)
                                {
                                    isoLangCode = (string)hdlanguage.Attributes["jarvis_iso3languagecode6392t"];
                                }
                            }

                            if (diff.TotalMinutes <= 60 && diff.TotalMinutes >= 0)
                            {
                                if (!string.IsNullOrEmpty(isoLangCode) || !string.IsNullOrEmpty(isCountryCode))
                                {
                                    tracingService.Trace("creating/updating MA for Chase ATA difference less/equal 60 mins");
                                    CaseMonitorProcess operation = new CaseMonitorProcess();
                                    string fucomment = isoLangCode + " " + isCountryCode + " " + "Chase ATA";
                                    operation.AutomateMonitorCreationTime(retreiveCase, fucomment, 2, 17, 0, " ", currentEta, 30, userService);
                                }
                            }
                            //// case type 2 = breakdown
                            else if (diff.TotalMinutes > 60 && caseType == 2)
                            {
                                if (retreiveCase.Attributes.Contains("jarvis_assistancetype") && retreiveCase.Attributes["jarvis_assistancetype"] != null)
                                {
                                    int assistanceType = ((OptionSetValue)retreiveCase.Attributes["jarvis_assistancetype"]).Value;

                                    if (assistanceType != 334030002 && assistanceType != 334030003 && (!string.IsNullOrEmpty(isoLangCode) || !string.IsNullOrEmpty(isCountryCode)))
                                    {
                                        tracingService.Trace("creating/updating MA for Chase ATA difference greater than 60 mins");
                                        CaseMonitorProcess operation = new CaseMonitorProcess();
                                        string fucomment = isoLangCode + " " + isCountryCode + " " + "Chase reason of delayed ETA";
                                        operation.AutomateMonitorCreationTime(retreiveCase, fucomment, 1, 8, 0, " ", DateTime.UtcNow, 0, userService);
                                    }
                                }
                            }
                        }
#pragma warning restore SA1123 // Do not place regions within elements
                        #endregion

#pragma warning disable SA1123 // Do not place regions within elements
                        #region Close_MA_ChaseATA_5.9.1.2 and Create MA for Chase Diagnose 5.12.1.1
                        if (passOut.Attributes.Contains("jarvis_ata") && passOut.Attributes["jarvis_ata"] != null && !passoutPreImg.Contains("jarvis_ata") && caseType == 2)
                        {
#pragma warning disable SA1123 // Do not place regions within elements
                            #region close MA
                            tracingService.Trace("inside ata has initial value hence close Chase ATA ");
#pragma warning restore SA1123 // Do not place regions within elements
                            CaseMonitorProcess operation = new CaseMonitorProcess();
                            string fucomments = "Chase ATA,Chase reason of delayed ETA";
                            operation.AutomateCloseMonitorActions(retreiveCase, fucomments, 1, fucomments, userService);
                            #endregion

#pragma warning disable SA1123 // Do not place regions within elements
                            #region Create MA for Chase Diagnose 5.12.1.1
                            DateTime initialAta = (DateTime)passOut.Attributes["jarvis_ata"];
#pragma warning restore SA1123 // Do not place regions within elements
                            EntityCollection getRepairInfo = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getRepairInformation, incident.Id, passOut.Id)));
                            tracingService.Trace("Repair Info count:" + getRepairInfo.Entities.Count);
                            if (getRepairInfo.Entities.Count == 0)
                            {
                                tracingService.Trace("If no Repair Information present then create MA for Chase Diagnose");
                                EntityReference homeDealer = (EntityReference)passoutImg.Attributes["jarvis_repairingdealer"];
                                Entity account = service.Retrieve(homeDealer.LogicalName, homeDealer.Id, new ColumnSet("jarvis_language", "jarvis_address1_country"));
                                string isoLangCode = string.Empty;
                                string isCountryCode = string.Empty;
                                if (account.Attributes.Contains("jarvis_address1_country") && account.Attributes["jarvis_address1_country"] != null)
                                {
                                    EntityReference hdcountry = (EntityReference)account.Attributes["jarvis_address1_country"];
                                    Entity country = service.Retrieve(hdcountry.LogicalName, hdcountry.Id, new ColumnSet("jarvis_iso2countrycode"));
                                    if (country.Attributes.Contains("jarvis_iso2countrycode") && country.Attributes["jarvis_iso2countrycode"] != null)
                                    {
                                        isCountryCode = (string)country.Attributes["jarvis_iso2countrycode"];
                                    }
                                }

                                if (account.Attributes.Contains("jarvis_language") && account.Attributes["jarvis_language"] != null)
                                {
                                    EntityReference language = (EntityReference)account.Attributes["jarvis_language"];
                                    Entity hdlanguage = service.Retrieve(language.LogicalName, language.Id, new ColumnSet("jarvis_iso3languagecode6392t"));
                                    if (hdlanguage.Attributes.Contains("jarvis_iso3languagecode6392t") && hdlanguage.Attributes["jarvis_iso3languagecode6392t"] != null)
                                    {
                                        isoLangCode = (string)hdlanguage.Attributes["jarvis_iso3languagecode6392t"];
                                    }
                                }

                                if (!string.IsNullOrEmpty(isoLangCode) || !string.IsNullOrEmpty(isCountryCode))
                                {
                                    tracingService.Trace("creating MA for Chase Diagnose");
                                    CaseMonitorProcess operationDiag = new CaseMonitorProcess();
                                    string fucomment = isoLangCode + " " + isCountryCode + " " + "Chase Diagnose";
                                    operationDiag.AutomateMonitorCreationTime(retreiveCase, fucomment, 1, 8, 0, " ", initialAta, 60, userService);
                                }
                            }
                            #endregion
                        }
#pragma warning restore SA1123 // Do not place regions within elements

                        #endregion

#pragma warning disable SA1123 // Do not place regions within elements
                        #region 5.13.1.1  Create MA for JED
                        if (passOut.Attributes.Contains("jarvis_atc") && passOut.Attributes["jarvis_atc"] != null && !passoutPreImg.Contains("jarvis_atc") && caseType == 2)
                        {
                            DateTime initialAtc = (DateTime)passOut.Attributes["jarvis_atc"];
                            EntityCollection getJeds = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getJeds, incident.Id, passOut.Id)));
                            tracingService.Trace("JEd count" + getJeds.Entities.Count);
                            if (getJeds.Entities.Count == 0)
                            {
                                tracingService.Trace("If no Repair Information present then create MA for Chase Diagnose");
                                EntityReference homeDealer = (EntityReference)passoutImg.Attributes["jarvis_repairingdealer"];
                                Entity account = service.Retrieve(homeDealer.LogicalName, homeDealer.Id, new ColumnSet("jarvis_language", "jarvis_address1_country"));
                                string isoLangCode = string.Empty;
                                string isCountryCode = string.Empty;
                                if (account.Attributes.Contains("jarvis_address1_country") && account.Attributes["jarvis_address1_country"] != null)
                                {
                                    EntityReference hdcountry = (EntityReference)account.Attributes["jarvis_address1_country"];
                                    Entity country = service.Retrieve(hdcountry.LogicalName, hdcountry.Id, new ColumnSet("jarvis_iso2countrycode"));
                                    if (country.Attributes.Contains("jarvis_iso2countrycode") && country.Attributes["jarvis_iso2countrycode"] != null)
                                    {
                                        isCountryCode = (string)country.Attributes["jarvis_iso2countrycode"];
                                    }
                                }

                                if (account.Attributes.Contains("jarvis_language") && account.Attributes["jarvis_language"] != null)
                                {
                                    EntityReference language = (EntityReference)account.Attributes["jarvis_language"];
                                    Entity hdlanguage = service.Retrieve(language.LogicalName, language.Id, new ColumnSet("jarvis_iso3languagecode6392t"));
                                    if (hdlanguage.Attributes.Contains("jarvis_iso3languagecode6392t") && hdlanguage.Attributes["jarvis_iso3languagecode6392t"] != null)
                                    {
                                        isoLangCode = (string)hdlanguage.Attributes["jarvis_iso3languagecode6392t"];
                                    }
                                }

                                if (!string.IsNullOrEmpty(isoLangCode) || !string.IsNullOrEmpty(isCountryCode))
                                {
                                    tracingService.Trace("creating MA for Chase JEDS");
                                    CaseMonitorProcess operationDiag = new CaseMonitorProcess();
                                    string fucomment = isoLangCode + " " + isCountryCode + " " + "Chase JEDS";
                                    operationDiag.AutomateMonitorCreationTime(retreiveCase, fucomment, 1, 8, 0, " ", initialAtc, 60, userService);
                                }
                            }

                            if (getJeds.Entities.Count > 0)
                            {
                                tracingService.Trace("inside ata has value and JED was created, close Chase Diagnose ");
                                CaseMonitorProcess operation = new CaseMonitorProcess();
                                string fucomment = "Chase Diagnose";
                                operation.AutomateCloseMonitorActions(retreiveCase, fucomment, 1, fucomment, userService);
                            }
                        }
#pragma warning restore SA1123 // Do not place regions within elements
                        #endregion

                        if (isUpdate)
                        {
                            parentCase.Id = incident.Id;
                            userService.Update(parentCase);
                        }

#pragma warning disable SA1123 // Do not place regions within elements
                        #region Pass Out Translation
                        bool isAutomation = false;
#pragma warning restore SA1123 // Do not place regions within elements
                        isAutomation = CrmHelper.GetAutomationConfig(service, MCS.Jarvis.CE.Plugins.Constants.JarvisConfiguration.Automationtranslation, tracingService);
                        if (isAutomation)
                        {
                            if (passOut.Contains("jarvis_reason") && passOut.Attributes["jarvis_reason"] != null)
                            {
                                Entity passOutUpdate = new Entity(passOut.LogicalName);
                                passOutUpdate.Id = passOut.Id;
                                TranslationProcess operations = new TranslationProcess();
                                operations.PassOutStandardProcess(service, tracingService, retreiveCase, context.InitiatingUserId, passOut);
                                Entity caseUpdate = service.Retrieve(passOut.LogicalName, passOut.Id, new ColumnSet("jarvis_translationsstatusdelayedetareason"));
                                OptionSetValue translationStatusLoc = (OptionSetValue)caseUpdate.Attributes["jarvis_translationsstatusdelayedetareason"];
                                if (translationStatusLoc.Value != 334030001)
                                {
                                    // In-Progress
                                    passOutUpdate["jarvis_translationsstatusdelayedetareason"] = new OptionSetValue(334030001);
                                    userService.Update(passOutUpdate);
                                }
                            }
                        }
                        #endregion

#pragma warning disable SA1123 // Do not place regions within elements
                        #region Set Appointment Fields
                        if (passoutImg.Attributes.Contains("jarvis_incident") && passoutImg.Attributes["jarvis_incident"] != null)
                        {
                            if (passOut.Attributes.Contains("jarvis_repairingdealer") || passOut.Attributes.Contains("jarvis_etatime") || passOut.Attributes.Contains("jarvis_etadate"))
                            {
                                tracingService.Trace($"{retreiveCase.Attributes.Count}");
                                OptionSetValue isMercuriusMode = new OptionSetValue();
                                isMercuriusMode = CrmHelper.GetReleaseAutomationConfig(service, MCS.Jarvis.CE.Plugins.Constants.JarvisConfiguration.Automationreleasecase, tracingService);
                                if (isMercuriusMode.Value == 3)
                                {
                                    // OneCaseOnly
                                    this.AppointmentUpdate(passoutImg, service, retreiveCase, incident, tracingService);
                                }
                            }
                        }
#pragma warning restore SA1123 // Do not place regions within elements
                        #endregion

#pragma warning disable SA1123 // Do not place regions within elements
                        #region Set RD=HD
                        retreiveCase = service.Retrieve(incident.LogicalName, incident.Id, new ColumnSet("statuscode"));
#pragma warning restore SA1123 // Do not place regions within elements
                        if (retreiveCase.Attributes.Contains("statuscode") && retreiveCase.Attributes["statuscode"] != null)
                        {
                            OptionSetValue caseStatus = (OptionSetValue)retreiveCase.Attributes["statuscode"];
                            if (caseStatus.Value != 5)
                            {
                                if (caseStatus.Value != 1000)
                                {
                                    // Update Case jarvis RDHD field
                                    if (passoutImg.Attributes.Contains("jarvis_repairingdealer") && passoutImg.Attributes["jarvis_repairingdealer"] != null)
                                    {
                                        this.SetCaseRDHD(service, incident);
                                    }
                                }
                            }
                        }

                        #endregion

#pragma warning disable SA1123 // Do not place regions within elements
                        #region Stage Changes
                        if (passOut.Contains("statuscode") && passOut.Attributes["statuscode"] != null)
                        {
                            OptionSetValue status = (OptionSetValue)passOut.Attributes["statuscode"];
                            tracingService.Trace("ATA" + passOut.Contains("jarvis_ata").ToString());
                            if (status.Value == 334030001)
                            {
                                // Send
                                Entity parentCaseUpdated = service.Retrieve(incident.LogicalName, incident.Id, new ColumnSet("jarvis_atc", "jarvis_etc", "jarvis_eta", "jarvis_ata", "jarvis_totalgoplimitoutapproved", "jarvis_totalgoplimitoutapprovedcurrency", "jarvis_totalrestcurrencyout", "jarvis_eta", "jarvis_totalpassoutamount", "jarvis_totalpassoutcurrency", "statuscode"));
                                this.UpdateCaseStage(parentCaseUpdated, incident, serviceFactory, userService, tracingService, initiatingUserID);
                            }
                        }
#pragma warning restore SA1123 // Do not place regions within elements

                        #endregion
                    }

                    // endregion
                }
                catch (InvalidPluginExecutionException ex)
                {
                    throw new InvalidPluginExecutionException(ex.Message + " ");
                }
            }
        }

        /// <summary>
        /// Updating Stages as per Case Status US:347853.
        /// </summary>
        /// <param name="parentCaseUpdated">parent Case Updated.</param>
        /// <param name="incident">incident details.</param>
        /// <param name="serviceFactory">service Factory.</param>
        /// <param name="service">Org service.</param>
        /// <param name="tracingService">tracing Service.</param>
        /// <param name="initiatingUserID">initiating User ID.</param>
        internal void UpdateCaseStage(Entity parentCaseUpdated, EntityReference incident, IOrganizationServiceFactory serviceFactory, IOrganizationService service, ITracingService tracingService, Guid initiatingUserID)
        {
            bool isAutomate = false;
            isAutomate = CrmHelper.GetAutomationConfig(service, MCS.Jarvis.CE.Plugins.Constants.JarvisConfiguration.Automationcasestatuschange, tracingService);
            if (isAutomate)
            {
                if (parentCaseUpdated.Attributes.Contains("jarvis_totalpassoutamount") && parentCaseUpdated.Attributes["jarvis_totalpassoutamount"] != null)
                {
                    if (parentCaseUpdated.Attributes.Contains("jarvis_totalpassoutcurrency") && parentCaseUpdated.Attributes["jarvis_totalpassoutcurrency"] != null)
                    {
                        Entity parentCase = service.Retrieve(incident.LogicalName, incident.Id, new ColumnSet("statuscode"));

#pragma warning disable SA1123 // Do not place regions within elements
                        #region Update Case Status
                        if (parentCase.Attributes.Contains("statuscode") && parentCase.Attributes["statuscode"] != null)
                        {
                            OptionSetValue casestatus = (OptionSetValue)parentCase.Attributes["statuscode"];
                            if (casestatus.Value == 30)
                            {
                                // PassOut
                                tracingService.Trace("updating Mercurius status");
                                Entity incidentToUpdate = new Entity(parentCaseUpdated.LogicalName);
                                incidentToUpdate.Id = parentCaseUpdated.Id;
                                incidentToUpdate["jarvis_mercuriusstatus"] = new OptionSetValue(300); // ETA Technician
                                incidentToUpdate["jarvis_casestatusupdate"] = DateTime.UtcNow;      // 560406
                                service.Update(incidentToUpdate);
                                IOrganizationService adminservice = serviceFactory.CreateOrganizationService(null);
                                this.SendNotification(initiatingUserID, adminservice, parentCaseUpdated, Constants.NotificationData.StatusPassout2ETATechnicianbody);
                            }
                        }
#pragma warning restore SA1123 // Do not place regions within elements

                        #endregion
                    }
                }
            }

#pragma warning disable SA1123 // Do not place regions within elements
            #region Close Monitor Actions

            CaseMonitorProcess moperations = new CaseMonitorProcess();
#pragma warning restore SA1123 // Do not place regions within elements
            string fumcomment = "Pass out,Confirm case opening,FU RD on PO Approval";
            moperations.AutomateCloseMonitorActions(parentCaseUpdated, fumcomment, 1, fumcomment, service);
            #endregion
            //// Force Auto Movement
            if (isAutomate)
            {
                Entity incidentPostUpdate = service.Retrieve(incident.LogicalName, incident.Id, new ColumnSet("jarvis_atc", "jarvis_etc", "jarvis_eta", "jarvis_ata", "jarvis_totalgoplimitoutapproved", "jarvis_totalgoplimitoutapprovedcurrency", "jarvis_totalrestcurrencyout", "jarvis_eta", "jarvis_totalpassoutamount", "jarvis_totalpassoutcurrency", "statuscode"));

                if (incidentPostUpdate.Attributes.Contains("statuscode") && incidentPostUpdate.Attributes["statuscode"] != null)
                {
                    OptionSetValue caseStatus = (OptionSetValue)incidentPostUpdate.Attributes["statuscode"];

#pragma warning disable SA1123 // Do not place regions within elements
                    #region Waiting For Repair

                    if (caseStatus.Value == 40)
                    {
                        // ETA Technician
                        if ((incidentPostUpdate.Attributes.Contains("jarvis_eta") && incidentPostUpdate.Attributes["jarvis_eta"] != null) ||
                            (incidentPostUpdate.Attributes.Contains("jarvis_ata") && incidentPostUpdate.Attributes["jarvis_ata"] != null))
                        {
                            this.ExecuteBPF(incidentPostUpdate.ToEntityReference(), Constants.Incident.BpfStage5, service, tracingService);
                        }
                    }
#pragma warning restore SA1123 // Do not place regions within elements
                    #endregion

#pragma warning disable SA1123 // Do not place regions within elements
                    #region Repair On Going

                    incidentPostUpdate = service.Retrieve(incident.LogicalName, incident.Id, new ColumnSet("jarvis_atc", "jarvis_etc", "jarvis_eta", "jarvis_ata", "jarvis_totalgoplimitoutapproved", "jarvis_totalgoplimitoutapprovedcurrency", "jarvis_totalrestcurrencyout", "jarvis_eta", "jarvis_totalpassoutamount", "jarvis_totalpassoutcurrency", "statuscode"));
#pragma warning restore SA1123 // Do not place regions within elements
                    caseStatus = (OptionSetValue)incidentPostUpdate.Attributes["statuscode"];
                    if (caseStatus.Value == 50)
                    {
                        // Waiting For Repair
                        if (incidentPostUpdate.Attributes.Contains("jarvis_ata") && incidentPostUpdate.Attributes["jarvis_ata"] != null)
                        {
                            this.ExecuteBPF(incidentPostUpdate.ToEntityReference(), Constants.Incident.BpfStage6, service, tracingService);
                        }
                    }
                    #endregion

#pragma warning disable SA1123 // Do not place regions within elements
                    #region Repair Finished

                    incidentPostUpdate = service.Retrieve(incident.LogicalName, incident.Id, new ColumnSet("jarvis_atc", "jarvis_etc", "jarvis_eta", "jarvis_ata", "jarvis_totalgoplimitoutapproved", "jarvis_totalgoplimitoutapprovedcurrency", "jarvis_totalrestcurrencyout", "jarvis_eta", "jarvis_totalpassoutamount", "jarvis_totalpassoutcurrency", "statuscode"));
#pragma warning restore SA1123 // Do not place regions within elements
                    caseStatus = (OptionSetValue)incidentPostUpdate.Attributes["statuscode"];
                    if (caseStatus.Value == 60)
                    {
                        // Repair Ongoing
                        EntityCollection getNonETCPassOuts = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getNonETCPassOuts, incidentPostUpdate.Id)));
                        if ((incidentPostUpdate.Attributes.Contains("jarvis_etc") && incidentPostUpdate.Attributes["jarvis_etc"] != null)
                            || (incidentPostUpdate.Attributes.Contains("jarvis_atc") && incidentPostUpdate.Attributes["jarvis_atc"] != null))
                        {
                            this.ExecuteBPF(incidentPostUpdate.ToEntityReference(), Constants.Incident.BpfStage7, service, tracingService);

#pragma warning disable SA1123 // Do not place regions within elements
                            #region Close Monitor Actions

                            CaseMonitorProcess operations = new CaseMonitorProcess();
#pragma warning restore SA1123 // Do not place regions within elements
                            string fucomment = "ETC,Pass Repair info";
                            operations.AutomateCloseMonitorActions(incidentPostUpdate, fucomment, 1, fucomment, service);

                            #endregion
                        }
                    }
                    #endregion

#pragma warning disable SA1123 // Do not place regions within elements
                    #region Repair Summary

                    incidentPostUpdate = service.Retrieve(incident.LogicalName, incident.Id, new ColumnSet("jarvis_atc", "jarvis_etc", "jarvis_eta", "jarvis_ata", "jarvis_totalgoplimitoutapproved", "jarvis_totalgoplimitoutapprovedcurrency", "jarvis_totalrestcurrencyout", "jarvis_eta", "jarvis_totalpassoutamount", "jarvis_totalpassoutcurrency", "statuscode"));
#pragma warning restore SA1123 // Do not place regions within elements
                    caseStatus = (OptionSetValue)incidentPostUpdate.Attributes["statuscode"];
                    if (caseStatus.Value == 70)
                    {
                        // Repair Finished
                        EntityCollection getNonATCPassOuts = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getNonATCPassOuts, incidentPostUpdate.Id)));
                        if (incidentPostUpdate.Attributes.Contains("jarvis_atc") && incidentPostUpdate.Attributes["jarvis_atc"] != null)
                        {
                            this.ExecuteBPF(incidentPostUpdate.ToEntityReference(), Constants.Incident.BpfStage8, service, tracingService);

#pragma warning disable SA1123 // Do not place regions within elements
                            #region Close Monitor Actions

                            CaseMonitorProcess operations = new CaseMonitorProcess();
#pragma warning restore SA1123 // Do not place regions within elements
                            string fucomment = "Pass ETC,ATC";
                            operations.AutomateCloseMonitorActions(incidentPostUpdate, fucomment, 1, fucomment, service);

                            #endregion
                        }
                    }
                    #endregion

#pragma warning disable SA1123 // Do not place regions within elements
                    #region Case Closure

                    incidentPostUpdate = service.Retrieve(incident.LogicalName, incident.Id, new ColumnSet("statuscode", "jarvis_actualcausefault", "jarvis_mileageafterrepair", "jarvis_mileageunitafterrepair"));
#pragma warning restore SA1123 // Do not place regions within elements
                    caseStatus = (OptionSetValue)incidentPostUpdate.Attributes["statuscode"];
                    if (caseStatus.Value == 80)
                    {
                        // Repair Summary
                        if (incidentPostUpdate.Attributes.Contains("jarvis_actualcausefault") && incidentPostUpdate.Attributes["jarvis_actualcausefault"] != null)
                        {
                            if (incidentPostUpdate.Attributes.Contains("jarvis_mileageafterrepair") && incidentPostUpdate.Attributes["jarvis_mileageafterrepair"] != null)
                            {
                                if (incidentPostUpdate.Attributes.Contains("jarvis_mileageunitafterrepair") && incidentPostUpdate.Attributes["jarvis_mileageunitafterrepair"] != null)
                                {
                                    this.ExecuteBPF(incidentPostUpdate.ToEntityReference(), Constants.Incident.BpfStage9, service, tracingService);
                                }
                            }
                        }
                    }

                    #endregion
                }
            }
        }

        /// <summary>
        /// Execute BPF.
        /// </summary>
        /// <param name="incidentRetrieve">incident Retrieve.</param>
        /// <param name="stageName">stage Name.</param>
        /// <param name="service">Org service.</param>
        /// <param name="tracingService">tracing Service.</param>
        internal void ExecuteBPF(EntityReference incidentRetrieve, string stageName, IOrganizationService service, ITracingService tracingService)
        {
#pragma warning disable SA1123 // Do not place regions within elements
            #region Stage Updates

            string activeStageName = stageName;
#pragma warning restore SA1123 // Do not place regions within elements

            // Get Process Instances
            RetrieveProcessInstancesRequest processInstanceRequest = new RetrieveProcessInstancesRequest
            {
                EntityId = incidentRetrieve.Id,

                EntityLogicalName = incidentRetrieve.LogicalName,
            };

            RetrieveProcessInstancesResponse processInstanceResponse = (RetrieveProcessInstancesResponse)service.Execute(processInstanceRequest);
            var activeProcessInstance = processInstanceResponse.Processes.Entities[0];
            Guid activeProcessInstanceID = activeProcessInstance.Id;

            // Retrieve the process stages in the active path of the current process instance
            RetrieveActivePathRequest pathReq = new RetrieveActivePathRequest
            {
                ProcessInstanceId = activeProcessInstanceID,
            };

            EntityCollection getcaseBPFProcess = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getCaseBPFInstance, incidentRetrieve.Id)));
            if (getcaseBPFProcess.Entities.Count > 0)
            {
                Guid caseBPFID = getcaseBPFProcess.Entities[0].Id;
                RetrieveActivePathResponse pathResp = (RetrieveActivePathResponse)service.Execute(pathReq);
                tracingService.Trace("Stages Count:" + pathResp.ProcessStages.Entities.Count + " ");
                for (int i = 0; i < pathResp.ProcessStages.Entities.Count; i++)
                {
                    // Retrieve the active stage name and active stage position based on the activeStageId for the process instance
                    if (pathResp.ProcessStages.Entities[i].Attributes["stagename"].ToString() == activeStageName)
                    {
                        tracingService.Trace("StageName:" + activeStageName + "  " + ((Guid)pathResp.ProcessStages.Entities[i].Attributes["processstageid"]).ToString() + " ");
                        Guid processStageID = (Guid)pathResp.ProcessStages.Entities[i].Attributes["processstageid"];
                        int activeStagePosition = i;

                        Entity retrievedProcessInstance = new Entity("jarvis_vasbreakdownprocess");
                        tracingService.Trace("process name:" + activeProcessInstance.LogicalName + " ");
                        tracingService.Trace("process id:" + activeProcessInstanceID.ToString());
                        retrievedProcessInstance.Id = caseBPFID;
                        retrievedProcessInstance.Attributes["activestageid"] = new EntityReference("processstage", processStageID); // processStageID
                        service.Update(retrievedProcessInstance);
                        tracingService.Trace("Completed:");
                    }
                }
            }

            #endregion
        }

        /// <summary>
        /// deactivate Pass out With Same Dealer.
        /// </summary>
        /// <param name="passOut">pass Out.</param>
        /// <param name="passOutImg">pass Out Image.</param>
        /// <param name="service">Org service.</param>
        internal void DeactivatePassoutWithSameDealer(Entity passOut, Entity passOutImg, IOrganizationService service)
        {
            EntityReference incident = (EntityReference)passOutImg.Attributes["jarvis_incident"];
            EntityReference passoutRD = (EntityReference)passOutImg.Attributes["jarvis_repairingdealer"];
            EntityCollection casePassouts = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.CaseActivePassouts, incident.Id)));
            foreach (var casePassout in casePassouts.Entities)
            {
                EntityReference passoutDealer = (EntityReference)casePassout["jarvis_repairingdealer"];
                if (passoutDealer.Id == passoutRD.Id && casePassout.Id != passOut.Id)
                {
#pragma warning disable SA1123 // Do not place regions within elements
                    #region Update Request - Deactivate Pass Out

                    Entity deactivatePassOut = new Entity(casePassout.LogicalName);
#pragma warning restore SA1123 // Do not place regions within elements
                    deactivatePassOut.Id = casePassout.Id;
                    deactivatePassOut["statecode"] = new OptionSetValue(1);
                    deactivatePassOut["statuscode"] = new OptionSetValue(2);
                    //UpdateRequest updatePassOutRequest = new UpdateRequest()
                    //{
                    //    Target = deactivatePassOut,
                    //};
                    //service.Execute(updatePassOutRequest);
                    service.Update(deactivatePassOut);
                    #endregion
                }
            }
        }

        /// <summary>
        /// Currency Exchange.
        /// </summary>
        /// <param name="sourceCurrencyId">Source Currency Id.</param>
        /// <param name="targetCurrencyId">Target Currency Id.</param>
        /// <param name="service">Org service.</param>
        /// <returns>Exchange Rate.</returns>
        internal decimal CurrencyExchange(Guid sourceCurrencyId, Guid targetCurrencyId, IOrganizationService service)
        {
            decimal exchangeValue = 0;
            if (sourceCurrencyId == targetCurrencyId)
            {
                exchangeValue = 1;
            }
            else
            {
                EntityCollection exchangeRates = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.ExchangeRate, sourceCurrencyId, targetCurrencyId)));

                foreach (var exchangeRate in exchangeRates.Entities)
                {
                    exchangeValue = (decimal)exchangeRate["jarvis_value"];
                }
            }

            return exchangeValue;
        }

        /// <summary>
        /// Set Case RDHD.
        /// </summary>
        /// <param name="service">Org service.</param>
        /// <param name="incident">incident details.</param>
        internal void SetCaseRDHD(IOrganizationService service, EntityReference incident)
        {
            EntityCollection passoutRd = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getPassoutRD, incident.Id)));
            Entity updateCase = new Entity(Constants.Incident.IncidentValue);
            if (passoutRd.Entities.First().Contains("casetypecode") && passoutRd.Entities.First().Attributes["casetypecode"] != null)
            {
                OptionSetValue caseTypeCode = (OptionSetValue)passoutRd.Entities.First().Attributes["casetypecode"];
                if (caseTypeCode.Value == 2)
                {
                    // Breakdown
                    if (passoutRd.Entities.Count > 1 && passoutRd.Entities.First().Contains(Constants.Incident.JarvisHDRD) && (bool)passoutRd.Entities.First().Attributes[Constants.Incident.JarvisHDRD])
                    {
                        updateCase.Attributes[Constants.Incident.IncidentId] = incident.Id;
                        updateCase.Attributes[Constants.Incident.JarvisHDRD] = false;
                    }
                    else if (passoutRd.Entities.Count == 1 && passoutRd.Entities.First().Contains(Constants.Incident.CaseRDjarvisRepairingdealer) && passoutRd.Entities.First().Contains("jarvis_homedealer"))
                    {
                        if ((((EntityReference)passoutRd.Entities.First().Attributes[Constants.Incident.JarvisHomedealer]).Id == ((EntityReference)((AliasedValue)passoutRd.Entities.First().Attributes[Constants.Incident.CaseRDjarvisRepairingdealer]).Value).Id) &&
                            (!(bool)passoutRd.Entities.First().Attributes[Constants.Incident.JarvisHDRD] || !passoutRd.Entities.First().Contains(Constants.Incident.JarvisHDRD)))
                        {
                            updateCase.Attributes[Constants.Incident.IncidentId] = incident.Id;
                            updateCase.Attributes[Constants.Incident.JarvisHDRD] = true;
                        }
                        else if (((EntityReference)passoutRd.Entities.First().Attributes[Constants.Incident.JarvisHomedealer]).Id != ((EntityReference)((AliasedValue)passoutRd.Entities.First().Attributes[Constants.Incident.CaseRDjarvisRepairingdealer]).Value).Id &&
                            passoutRd.Entities.First().Contains(Constants.Incident.JarvisHDRD) && (bool)passoutRd.Entities.First().Attributes[Constants.Incident.JarvisHDRD])
                        {
                            updateCase.Attributes[Constants.Incident.IncidentId] = incident.Id;
                            updateCase.Attributes[Constants.Incident.JarvisHDRD] = false;
                        }
                    }
                    else
                    {
                        if (passoutRd.Entities.First().Contains(Constants.Incident.JarvisHDRD) && (bool)passoutRd.Entities.First().Attributes[Constants.Incident.JarvisHDRD])
                        {
                            updateCase.Attributes[Constants.Incident.IncidentId] = incident.Id;
                            updateCase.Attributes[Constants.Incident.JarvisHDRD] = false;
                        }
                    }

                    if (updateCase.Attributes.Count > 0)
                    {
                        service.Update(updateCase);
                    }
                }
            }
        }

        /// <summary>
        /// Appointment Update.
        /// </summary>
        /// <param name="passOutPostImage">pass Out Post Image.</param>
        /// <param name="service">Org service.</param>
        /// <param name="caseEntity">case Entity.</param>
        /// <param name="incident">incident details.</param>
        /// <param name="tracingService">tracing Service.</param>
        internal void AppointmentUpdate(Entity passOutPostImage, IOrganizationService service, Entity caseEntity, EntityReference incident, ITracingService tracingService)
        {
            tracingService.Trace("Enter into AppointmentUpdate");

            Entity parentCase = service.Retrieve(incident.LogicalName, incident.Id, new ColumnSet("jarvis_assistancetype", "createdby", "jarvis_location", "jarvis_timezonelabel", "description", "jarvis_customerexpectations"));
            bool userCheck = false;
            if (passOutPostImage != null && parentCase != null && parentCase.Attributes.Contains("jarvis_assistancetype") && parentCase.Attributes.Contains("createdby"))
            {
                var assistanceType = ((OptionSetValue)parentCase.Attributes["jarvis_assistancetype"]).Value;

                if (assistanceType == 334030002 || assistanceType == 334030003)
                {
                    var createdBy = (EntityReference)parentCase.Attributes["createdby"];
                    var caseLocation = parentCase.Attributes.Contains("jarvis_location") ? parentCase.Attributes["jarvis_location"] : null;
                    var timezonelabel = parentCase.Attributes.Contains("jarvis_timezonelabel") ? parentCase.Attributes["jarvis_timezonelabel"] : null;
                    if (createdBy != null)
                    {
                        userCheck = CrmHelper.CheckUserIsMercurius(createdBy.Id, tracingService, service);
                    }

                    if (passOutPostImage.Attributes["jarvis_repairingdealer"] != null)
                    {
                        EntityReference rD = (EntityReference)passOutPostImage.Attributes["jarvis_repairingdealer"];
                        Entity reparingDealer = service.Retrieve("account", rD.Id, new ColumnSet("jarvis_responsableunitid", "name"));
                        if (reparingDealer != null)
                        {
                            if (reparingDealer.Attributes.Contains("jarvis_responsableunitid") || reparingDealer.Attributes.Contains("name"))
                            {
                                string blankValue = " ";
                                string formattedDate = blankValue;
                                DateTime isodateTime = DateTime.MinValue;
                                DateTime cstTime = DateTime.MinValue;
                                if (passOutPostImage.Attributes.Contains("jarvis_etadate") && passOutPostImage.Attributes["jarvis_etadate"] != null)
                                {
                                    TimeZoneInfo cstZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");

                                    // Formating Date based on the TimeZone
                                    DateTime dateValue = (DateTime)passOutPostImage.Attributes["jarvis_etadate"];
                                    DateTime utctime = dateValue.ToUniversalTime();
                                    dateValue = utctime.AddHours(12);
                                    cstTime = TimeZoneInfo.ConvertTimeFromUtc(dateValue, cstZone);
                                    formattedDate = cstTime.ToLocalTime().ToString("yyyy-MM-dd");
                                }

                                string accountNumber = reparingDealer.Attributes.Contains("jarvis_responsableunitid") ? reparingDealer.Attributes["jarvis_responsableunitid"].ToString() : blankValue;
                                string accountName = reparingDealer.Attributes.Contains("name") ? reparingDealer.Attributes["name"].ToString() : blankValue;
                                string appointmentETAtime = passOutPostImage.Attributes.Contains("jarvis_etatime") ? passOutPostImage.Attributes["jarvis_etatime"].ToString() : blankValue;

                                ////if (cstTime != DateTime.MinValue && !string.IsNullOrEmpty(appointmentETAtime))
                                if (cstTime != DateTime.MinValue && appointmentETAtime.Length == 4)
                                {
                                    tracingService.Trace("Format ISO Datetime");
                                    isodateTime = new DateTime(cstTime.Year, cstTime.Month, cstTime.Day, Convert.ToInt32(appointmentETAtime.Substring(0, 2)), Convert.ToInt32(appointmentETAtime.Substring(2, 2)), 0);
                                    tracingService.Trace("ISO Datetime Formatted");
                                }

                                // Update Case Appointment Fields
                                Entity updateCase = new Entity(Constants.Incident.IncidentValue);
                                updateCase.Id = incident.Id;
                                updateCase["jarvis_etatimeappointment"] = appointmentETAtime != blankValue ? passOutPostImage.Attributes["jarvis_etatime"] : null;
                                updateCase["jarvis_etadateappointment"] = formattedDate != blankValue ? passOutPostImage.Attributes["jarvis_etadate"] : null;
                                updateCase["jarvis_dealerappointment"] = rD;
                                if (!(userCheck && caseLocation != null))
                                {
                                    if (isodateTime != DateTime.MinValue)
                                    {
                                        updateCase["jarvis_location"] = string.Format("Responsible Unit ID: {0} \nDealer Name: {1} \nETA: {2} {3}", accountNumber, accountName, isodateTime.ToString("HH:mm yyyy-MM-dd"), timezonelabel);
                                    }
                                    else
                                    {
                                        updateCase["jarvis_location"] = string.Format("Responsible Unit ID: {0} \nDealer Name: {1} \nETA: {2} {3} {4}", accountNumber, accountName, appointmentETAtime, formattedDate, timezonelabel);
                                    }

                                    //updateCase["jarvis_location"] = string.Format("Responsible Unit ID: {0} \nDealer Name: {1} \nETA: {2} {3} {4}", accountNumber, accountName, appointmentETAtime, formattedDate, timezonelabel);

                                    if (parentCase.Attributes.Contains("jarvis_customerexpectations") && parentCase.Attributes["jarvis_customerexpectations"] != null)
                                    {
                                        updateCase["jarvis_customerexpectations"] = (string)parentCase.Attributes["jarvis_customerexpectations"];
                                    }

                                    if (parentCase.Attributes.Contains("description") && parentCase.Attributes["description"] != null)
                                    {
                                        updateCase["description"] = (string)parentCase.Attributes["description"];
                                    }
                                }

                                service.Update(updateCase);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// send Notification.
        /// </summary>
        /// <param name="initiatingUserID">initiating User ID.</param>
        /// <param name="adminService">Admin Service.</param>
        /// <param name="incident">Incident details.</param>
        /// <param name="body">body details.</param>
        internal void SendNotification(Guid initiatingUserID, IOrganizationService adminService, Entity incident, string body)
        {
            bool notifyAll = true;
            CaseNotifiaction casenotifiaction = new CaseNotifiaction();
            casenotifiaction.FrameNotifiaction(initiatingUserID, adminService, incident.Id, new Guid(new byte[16]), Constants.NotificationData.StatusChanged, body, notifyAll);
        }
    }
}