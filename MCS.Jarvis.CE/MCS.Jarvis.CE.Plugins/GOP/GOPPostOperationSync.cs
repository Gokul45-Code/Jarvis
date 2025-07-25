// <copyright file="GOPPostOperationSync.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MCS.Jarvis.CE.Plugins
{
    using System;
    using System.Linq;
    using MCS.Jarvis.CE.BusinessProcessesShared.Helpers;
    using MCS.Jarvis.CE.BusinessProcessShared.CaseMonitor;
    using MCS.jarvis.CE.BusinessProcessShared.TranslationProcess;
    using Microsoft.Crm.Sdk.Messages;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;

    /// <summary>
    /// GOP Post Operation Sync.
    /// </summary>
    public class GOPPostOperationSync : IPlugin
    {
        /// <summary>
        /// Execute GOP Post Operation Sync.
        /// </summary>
        /// <param name="serviceProvider">Service Provider.</param>
        /// <exception cref="InvalidPluginExecutionException">Invalid Plugin Execution Exception.</exception>
        public void Execute(IServiceProvider serviceProvider)
        {
            // Extract the tracing service for use in debugging sandboxed plug-ins.
            ITracingService tracingService =
                (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            // Obtain the execution context from the service provider.
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            try
            {
                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                {
#pragma warning disable SA1123 // Do not place regions within elements
                    #region Create

                    if (context.Stage == 40 && context.MessageName.ToUpper() == "CREATE")
                    {
                        IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                        IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
                        IOrganizationService adminservice = serviceFactory.CreateOrganizationService(null);
                        Entity gop = (Entity)context.InputParameters["Target"];
                        Entity gopImg = (Entity)context.PostEntityImages["PostImage"];
                        bool isApproved = false;
                        bool setStage = false;
                        EntityReference gopinCurrency = new EntityReference();
                        EntityReference gopOutCurrency = new EntityReference();
                        OptionSetValue status = new OptionSetValue();
                        bool isAutomate = false;
                        isAutomate = CrmHelper.GetAutomationConfig(service, MCS.Jarvis.CE.Plugins.Constants.JarvisConfiguration.Automationcasestatuschange, tracingService);
                        if (gop.Attributes.Contains("jarvis_incident") && gop.Attributes["jarvis_incident"] != null)
                        {
                            EntityReference incident = (EntityReference)gop.Attributes["jarvis_incident"];
                            Entity incidentRetrieve = service.Retrieve(incident.LogicalName, incident.Id, new ColumnSet("customerid", "statuscode", "jarvis_homedealer", "jarvis_callerrole", "jarvis_country", "jarvis_sourceid", "jarvis_onetimecustomercountry", "jarvis_onetimecustomerlanguage"));
                            Entity parentCase = new Entity(incident.LogicalName);
                            parentCase.Id = incident.Id;

                            if (incidentRetrieve.Attributes.Contains("statuscode") && incidentRetrieve.Attributes["statuscode"] != null)
                            {
                                status = (OptionSetValue)incidentRetrieve.Attributes["statuscode"];
                            }

                            if (gopImg.Attributes.Contains("jarvis_gopapproval") && gopImg.Attributes["jarvis_gopapproval"] != null)
                            {
                                tracingService.Trace("got gop approval");
                                var gopApprovalValue = (OptionSetValue)gopImg.Attributes["jarvis_gopapproval"];
                                if (gopApprovalValue.Value == 334030001)
                                {
                                    isApproved = true;
                                }

                                if (isApproved)
                                {
                                    tracingService.Trace("inside approved");

#pragma warning disable SA1123 // Do not place regions within elements
                                    #region Commented Code

                                    //// if (gopImg.Attributes.Contains("jarvis_goplimitout") && gopImg.Attributes["jarvis_goplimitout"] != null)
                                    //// {
                                    ////    gopOutAmnt = (decimal)gopImg.Attributes["jarvis_goplimitout"];
                                    ////    parentCase["jarvis_totalgoplimitoutapproved"] = gopOutAmnt;
                                    //// }
                                    //// if (gopImg.Attributes.Contains("jarvis_gopoutcurrency") && gopImg.Attributes["jarvis_gopoutcurrency"] != null)
                                    //// {
                                    ////    gopOutCurrency = (EntityReference)gopImg.Attributes["jarvis_gopoutcurrency"];
                                    ////    parentCase["jarvis_totalgoplimitoutapprovedcurrency"] = gopOutCurrency;
                                    //// }
                                    //// if (gopImg.Attributes.Contains("jarvis_goplimitin") && gopImg.Attributes["jarvis_goplimitin"] != null)
                                    //// {
                                    ////    gopinAmnt = (decimal)gopImg.Attributes["jarvis_goplimitin"];
                                    ////    parentCase["jarvis_totalgoplimitinapproved"] = gopinAmnt;
                                    //// }
                                    //// if (gopImg.Attributes.Contains("jarvis_gopincurrency") && gopImg.Attributes["jarvis_gopincurrency"] != null)
                                    //// {
                                    ////    gopinCurrency = (EntityReference)gopImg.Attributes["jarvis_gopincurrency"];
                                    ////    parentCase["jarvis_totalcurrencyinapproved"] = gopinCurrency;
                                    //// }
                                    //// if (gopImg.Attributes.Contains("jarvis_requesttype") && gopImg.Attributes["jarvis_requesttype"]!=null)
                                    //// {
                                    ////    OptionSetValue gopRequestType = (OptionSetValue)gopImg.Attributes["jarvis_requesttype"];
                                    ////    if (gopRequestType.Value == 334030000)//GOP
                                    ////    {

                                    #endregion

                                    if (status.Value == 20)
                                    {
                                        setStage = true;
                                    }
#pragma warning restore SA1123 // Do not place regions within elements
                                }
                                else
                                {
#pragma warning disable SA1123 // Do not place regions within elements
                                    #region Case Monitor Action Automation - GOP

                                    if (status.Value == 20)
                                    {
                                        tracingService.Trace("Gop Status pending");
                                        if (gopImg.Attributes.Contains("jarvis_requesttype") && gopImg.Attributes["jarvis_requesttype"] != null)
                                        {
                                            OptionSetValue gopType = (OptionSetValue)gopImg.Attributes["jarvis_requesttype"];
                                            if (gopType.Value == 334030001)
                                            {
                                                CaseMonitorProcess operations = new CaseMonitorProcess();
                                                if (incidentRetrieve.Attributes.Contains("jarvis_homedealer") && incidentRetrieve.Attributes["jarvis_homedealer"] != null)
                                                {
                                                    EntityReference homeDealer = (EntityReference)incidentRetrieve.Attributes["jarvis_homedealer"];
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

                                                    if (isCountryCode == null || isCountryCode == string.Empty)
                                                    {
                                                        EntityReference customer = (EntityReference)incidentRetrieve.Attributes["customerid"];
                                                        isCountryCode = operations.GetCountryCode(customer, service, tracingService);
                                                    }

                                                    if (!string.IsNullOrEmpty(isoLangCode) || !string.IsNullOrEmpty(isCountryCode))
                                                    {
                                                        tracingService.Trace("Creating MA Chase GOP");
                                                        string fucomment = isoLangCode + " " + isCountryCode + " " + "Chase GOP";
                                                        operations.AutomateMonitorCreation(incidentRetrieve, fucomment, 1, 3, 0, string.Empty, service);
                                                    }
                                                }
                                            }
                                        }
                                    }
#pragma warning restore SA1123 // Do not place regions within elements

                                    #endregion
                                }
                            }

#pragma warning disable SA1123 // Do not place regions within elements
                            #region Case Monitor Automation - HD/RD

                            if (status.Value != 20)
                            {
#pragma warning disable SA1123 // Do not place regions within elements
                                #region Case Monitor Action Automation - GOP HD

                                if (gopImg.Attributes.Contains("jarvis_requesttype") && gopImg.Attributes["jarvis_requesttype"] != null)
                                {
                                    OptionSetValue gopType = (OptionSetValue)gopImg.Attributes["jarvis_requesttype"];
                                    if (gopType.Value == 334030001)
                                    {
                                        EntityCollection gopHDCollection = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getGOPHDForCase, incident.Id)));
                                        if (gopHDCollection.Entities.Count > 0)
                                        {
                                            bool isCreate = false;
                                            bool approval = false;
                                            foreach (var item in gopHDCollection.Entities)
                                            {
                                                if (item.Attributes.Contains("jarvis_gopapproval") && item.Attributes["jarvis_gopapproval"] != null)
                                                {
                                                    var gopApprovalValue = (OptionSetValue)gopImg.Attributes["jarvis_gopapproval"];
                                                    if (gopApprovalValue.Value == 334030001)
                                                    {
                                                        approval = true;
                                                    }

                                                    // bool approval = (bool)item.Attributes["jarvis_approved"];
                                                    if (!approval)
                                                    {
                                                        isCreate = true;
                                                    }
                                                }
                                            }

                                            if (!isApproved)
                                            {
                                                isCreate = true;
                                            }

                                            if (isCreate)
                                            {
                                                if (incidentRetrieve.Attributes.Contains("jarvis_homedealer") && incidentRetrieve.Attributes["jarvis_homedealer"] != null)
                                                {
                                                    CaseMonitorProcess operations = new CaseMonitorProcess();
                                                    //if (!(gop.Attributes.Contains("jarvis_relatedgop") && gop.Attributes["jarvis_relatedgop"] != null))
                                                    // {
                                                    EntityReference homeDealer = (EntityReference)incidentRetrieve.Attributes["jarvis_homedealer"];
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

                                                    if (isCountryCode == null || isCountryCode == string.Empty)
                                                        {
                                                            EntityReference customer = (EntityReference)incidentRetrieve.Attributes["customerid"];
                                                            isCountryCode = operations.GetCountryCode(customer, service, tracingService);
                                                        }

                                                    if (!string.IsNullOrEmpty(isoLangCode) || !string.IsNullOrEmpty(isCountryCode))
                                                        {
                                                            //string fucomment = isoLangCode + " " + isCountryCode + " " + "Chase GOP HD";
                                                            string fucomment = isoLangCode + " " + isCountryCode + " " + "Chase GOP";
                                                            if (gopImg.Attributes.Contains("jarvis_paymenttype") && gopImg.Attributes["jarvis_paymenttype"] != null)
                                                            {
                                                                OptionSetValue paymentType = (OptionSetValue)gopImg.Attributes["jarvis_paymenttype"];
                                                                if (paymentType.Value != 334030006)
                                                                {
                                                                    tracingService.Trace("Chase GOP");
                                                                    operations.AutomateMonitorCreation(incidentRetrieve, fucomment, 1, 5, 0, string.Empty, service);
                                                                }
                                                            }
                                                            else
                                                            {
                                                                tracingService.Trace("Chase GOP");
                                                                operations.AutomateMonitorCreation(incidentRetrieve, fucomment, 1, 5, 0, string.Empty, service);
                                                            }
                                                    }

                                                    // }
                                                }
                                            }
                                        }
                                    }
                                }
#pragma warning restore SA1123 // Do not place regions within elements

                                #endregion

#pragma warning disable SA1123 // Do not place regions within elements
                                #region Case Monitor Action Automation - GOP RD

                                if (gopImg.Attributes.Contains("jarvis_requesttype") && gopImg.Attributes["jarvis_requesttype"] != null)
                                {
                                    OptionSetValue gopType = (OptionSetValue)gopImg.Attributes["jarvis_requesttype"];
                                    if (gopType.Value == 334030002)
                                    {
                                        if (isApproved)
                                        {
                                            EntityCollection gopRDCollection = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getGOPRDForCase, incident.Id)));
                                            if (gopRDCollection.Entities.Count > 0)
                                            {
                                                if (gopImg.Attributes.Contains("jarvis_repairingdealer") && gopImg.Attributes["jarvis_repairingdealer"] != null)
                                                {
                                                    EntityReference repairingDealerGOPRD = (EntityReference)gopImg.Attributes["jarvis_repairingdealer"];
                                                    Entity repairingDealerAccount = service.Retrieve(repairingDealerGOPRD.LogicalName, repairingDealerGOPRD.Id, new ColumnSet("jarvis_repairingdealer"));
                                                    EntityReference homeDealer = (EntityReference)repairingDealerAccount.Attributes["jarvis_repairingdealer"];
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
                                                        tracingService.Trace("Creating MA Pass GOP RD");
                                                        CaseMonitorProcess operations = new CaseMonitorProcess();
                                                        //string fucomment = isoLangCode + " " + isCountryCode + " " + "Pass GOP RD";
                                                        string fucomment = isoLangCode + " " + isCountryCode + " " + "Pass GOP";
                                                        operations.AutomateMonitorCreation(incidentRetrieve, fucomment, 1, 5, 0, string.Empty, service);
                                                    }
                                                }

                                                // for paymet type-->Whitelist
                                                if (gopImg.Attributes.Contains("jarvis_paymenttype") && gopImg.Attributes["jarvis_paymenttype"] != null)
                                                {
                                                    tracingService.Trace("Inside closing of Monitor action-Chase OK Cust Whitelist GOP HD ");
                                                    OptionSetValue gopPaymentType = (OptionSetValue)gopImg.Attributes["jarvis_paymenttype"];
                                                    if (gopPaymentType.Value == 334030006)
                                                    {
                                                        tracingService.Trace("Creating MA Chase OK Cust Whitelist GOP HD");
                                                        CaseMonitorProcess operations = new CaseMonitorProcess();
                                                        string fucomment = "Chase OK Cust Whitelist GOP HD";
                                                        operations.AutomateCloseMonitorActions(incidentRetrieve, fucomment, 1, fucomment, service);
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            // Payment Type Whitelist and Not Approved
                                            if (gopImg.Attributes.Contains("jarvis_paymenttype") && gopImg.Attributes["jarvis_paymenttype"] != null)
                                            {
                                                OptionSetValue gopPaymentType = (OptionSetValue)gopImg.Attributes["jarvis_paymenttype"];
                                                if (gopPaymentType.Value == 334030006)
                                                {
                                                    EntityCollection gopRDCollection = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getGOPRDForCase, incident.Id)));
                                                    tracingService.Trace(Convert.ToString(gopRDCollection.Entities.Count));
                                                    if (gopRDCollection.Entities.Any())
                                                    {
                                                        if (gopImg.Attributes.Contains("jarvis_dealer") && gopImg.Attributes["jarvis_dealer"] != null)
                                                        {
                                                            EntityReference customer = (EntityReference)incidentRetrieve.Attributes["customerid"];
                                                            Entity account = service.Retrieve(customer.LogicalName, customer.Id, new ColumnSet("jarvis_language", "jarvis_address1_country"));
                                                            string isoLangCode = string.Empty;
                                                            string isCountryCode = string.Empty;
                                                            if (account.Attributes.Contains("jarvis_address1_country") && account.Attributes["jarvis_address1_country"] != null)
                                                            {
                                                                EntityReference customerCountry = (EntityReference)account.Attributes["jarvis_address1_country"];
                                                                Entity country = service.Retrieve(customerCountry.LogicalName, customerCountry.Id, new ColumnSet("jarvis_iso2countrycode"));
                                                                if (country.Attributes.Contains("jarvis_iso2countrycode") && country.Attributes["jarvis_iso2countrycode"] != null)
                                                                {
                                                                    isCountryCode = (string)country.Attributes["jarvis_iso2countrycode"];
                                                                    tracingService.Trace(isCountryCode);
                                                                }
                                                            }

                                                            if (account.Attributes.Contains("jarvis_language") && account.Attributes["jarvis_language"] != null)
                                                            {
                                                                EntityReference language = (EntityReference)account.Attributes["jarvis_language"];
                                                                Entity customerLanguage = service.Retrieve(language.LogicalName, language.Id, new ColumnSet("jarvis_iso3languagecode6392t"));
                                                                if (customerLanguage.Attributes.Contains("jarvis_iso3languagecode6392t") && customerLanguage.Attributes["jarvis_iso3languagecode6392t"] != null)
                                                                {
                                                                    isoLangCode = (string)customerLanguage.Attributes["jarvis_iso3languagecode6392t"];
                                                                    tracingService.Trace(isoLangCode);
                                                                }
                                                            }

                                                            if (!string.IsNullOrEmpty(isoLangCode) || !string.IsNullOrEmpty(isCountryCode))
                                                            {
                                                                CaseMonitorProcess operations = new CaseMonitorProcess();
                                                                string fucomment = isoLangCode + " " + isCountryCode + " " + "Chase OK Cust Whitelist GOP HD";
                                                                tracingService.Trace("Creating MA for Chase OK Cust Whitelist GOP HD ");
                                                                operations.AutomateMonitorCreation(incidentRetrieve, fucomment, 1, 5, 0, string.Empty, service);
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
#pragma warning restore SA1123 // Do not place regions within elements
                                #endregion
                            }
#pragma warning restore SA1123 // Do not place regions within elements

                            #endregion

#pragma warning disable SA1123 // Do not place regions within elements
                            #region Case Monitor Action Automation - GOP+

                            if (gopImg.Attributes.Contains("createdby") && gopImg.Attributes["createdby"] != null)
                            {
                                EntityReference createdBy = (EntityReference)gopImg.Attributes["createdby"];
                                if (!createdBy.Name.ToUpperInvariant().Contains("MERCURIUS"))
                                {
                                    if (gopImg.Attributes.Contains("jarvis_paymenttype") && gopImg.Attributes["jarvis_paymenttype"] != null)
                                    {
                                        // 334030002
                                        OptionSetValue paymentType = (OptionSetValue)gopImg.Attributes["jarvis_paymenttype"];
                                        if (paymentType.Value == 334030002)
                                        {
                                            if (gopImg.Attributes.Contains("jarvis_gopapproval") && gopImg.Attributes["jarvis_gopapproval"] != null)
                                            {
                                                OptionSetValue gopApproval = (OptionSetValue)gopImg.Attributes["jarvis_gopapproval"];

#pragma warning disable SA1123 // Do not place regions within elements
                                                #region Pending

                                                if (gopApproval.Value == 334030000)
                                                {
                                                    string isoLangCode = string.Empty;
                                                    string isCountryCode = string.Empty;
                                                    if (incidentRetrieve.Attributes.Contains("jarvis_onetimecustomercountry") && incidentRetrieve.Attributes["jarvis_onetimecustomercountry"] != null)
                                                    {
                                                        EntityReference hdcountry = (EntityReference)incidentRetrieve.Attributes["jarvis_onetimecustomercountry"];
                                                        Entity country = service.Retrieve(hdcountry.LogicalName, hdcountry.Id, new ColumnSet("jarvis_iso2countrycode"));
                                                        if (country.Attributes.Contains("jarvis_iso2countrycode") && country.Attributes["jarvis_iso2countrycode"] != null)
                                                        {
                                                            isCountryCode = (string)country.Attributes["jarvis_iso2countrycode"];
                                                        }
                                                    }

                                                    if (incidentRetrieve.Attributes.Contains("jarvis_onetimecustomerlanguage") && incidentRetrieve.Attributes["jarvis_onetimecustomerlanguage"] != null)
                                                    {
                                                        EntityReference language = (EntityReference)incidentRetrieve.Attributes["jarvis_onetimecustomerlanguage"];
                                                        Entity hdlanguage = service.Retrieve(language.LogicalName, language.Id, new ColumnSet("jarvis_iso3languagecode6392t"));
                                                        if (hdlanguage.Attributes.Contains("jarvis_iso3languagecode6392t") && hdlanguage.Attributes["jarvis_iso3languagecode6392t"] != null)
                                                        {
                                                            isoLangCode = (string)hdlanguage.Attributes["jarvis_iso3languagecode6392t"];
                                                        }
                                                    }

                                                    if (!string.IsNullOrEmpty(isoLangCode) || !string.IsNullOrEmpty(isCountryCode))
                                                    {
                                                        CaseMonitorProcess operation = new CaseMonitorProcess();
                                                        string fucomment = isoLangCode + " " + isCountryCode + " " + "Chase CC Payment";
                                                        tracingService.Trace("Creating MA for Chase CC Payment");
                                                        operation.AutomateMonitorCreationTime(incidentRetrieve, fucomment, 1, 5, 0, string.Empty, DateTime.UtcNow, 15, service);
                                                    }
                                                }
#pragma warning restore SA1123 // Do not place regions within elements

                                                #endregion

                                            }
                                        }
                                    }
                                }
                            }
#pragma warning restore SA1123 // Do not place regions within elements

                            #endregion

#pragma warning disable SA1123 // Do not place regions within elements
                            #region Case MA Closure
                            if (gop.Attributes.Contains("jarvis_gopapproval") && gop.Attributes["jarvis_gopapproval"] != null)
                            {
                                tracingService.Trace("got gop approval");
                                var gopApprovalValue = (OptionSetValue)gop.Attributes["jarvis_gopapproval"];
                                if (gopApprovalValue.Value == 334030001)
                                {
                                    CaseMonitorProcess operation = new CaseMonitorProcess();
                                    //// Check if all GOP related to the case are approved
                                    bool closeMA = operation.CheckUnapprovedGOP(gop, incidentRetrieve, service, tracingService);
                                    //// If no unapproved GOP then close MA Chase GOP
                                    if (closeMA)
                                    {
                                        string fucomments = "Chase GOP,Chase OK Cust Whitelist GOP HD";
                                        operation.AutomateCloseMonitorActions(incidentRetrieve, fucomments, 1, fucomments, service);
                                        EntityCollection passOutCollection = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.CaseActivePassouts, incidentRetrieve.Id)));
                                        foreach (var passout in passOutCollection.Entities)
                                        {
                                            EntityReference repairingDealerpassOut = (EntityReference)passout.Attributes["jarvis_repairingdealer"];
                                            string countryLangCode = operation.GetCountryAndLanguageCode(repairingDealerpassOut, service, tracingService);
                                            if (!string.IsNullOrEmpty(countryLangCode))
                                            {
                                                fucomments = countryLangCode + " " + "Pass GOP";
                                                operation.AutomateMonitorCreation(incidentRetrieve, fucomments, 1, 5, 0, string.Empty, service);
                                            }
                                        }
                                    }
                                }
                            }
                            #endregion

#pragma warning disable SA1123 // Do not place regions within elements
                            #region Update Case

                            // service.Update(parentCase);
                            if (setStage && isAutomate)
                            {
                                tracingService.Trace("Updating case stage to Pass Out");
                                this.ExecuteBPF(incident, Constants.Incident.BpfStage3, service, tracingService);
                            }
#pragma warning restore SA1123 // Do not place regions within elements

                            #endregion

#pragma warning disable SA1123 // Do not place regions within elements
                            #region Cancel ParentGop
                            /*if (gop.Attributes.Contains("jarvis_parentgop") && gop.Attributes["jarvis_parentgop"] != null)
                            {
                                EntityReference parentGopRef = (EntityReference)gop.Attributes["jarvis_parentgop"];
                                Entity parentGop = service.Retrieve(gop.LogicalName, parentGopRef.Id, new ColumnSet("jarvis_approved", "jarvis_gopapproval"));
                                ////if (parentGop.Attributes.Contains("jarvis_approved") && parentGop.Attributes["jarvis_approved"] != null)
                                if (parentGop.Attributes.Contains("jarvis_gopapproval") && parentGop.Attributes["jarvis_gopapproval"] != null)
                                {
                                    OptionSetValue parentGopApproved = (OptionSetValue)parentGop.Attributes["jarvis_gopapproval"];

                                    if (parentGopApproved.Value == 334030000)
                                    {
                                        Entity updateParentGop = new Entity(gop.LogicalName, parentGopRef.Id);
                                        updateParentGop["statecode"] = new OptionSetValue(1);
                                        updateParentGop["statuscode"] = new OptionSetValue(334030001);
                                        service.Update(updateParentGop);
                                    }
                                }
                            }*/
#pragma warning restore SA1123 // Do not place regions within elements
                            #endregion

#pragma warning disable SA1123 // Do not place regions within elements
                            #region GOP Translation
                            bool isAutomation = false;
#pragma warning restore SA1123 // Do not place regions within elements
                            isAutomation = CrmHelper.GetAutomationConfig(service, MCS.Jarvis.CE.Plugins.Constants.JarvisConfiguration.Automationtranslation, tracingService);
                            if (isAutomation)
                            {
                                if (gop.Contains("jarvis_comment") && gop.Attributes["jarvis_comment"] != null)
                                {
                                    Entity passOutUpdate = new Entity(gop.LogicalName);
                                    passOutUpdate.Id = gop.Id;
                                    TranslationProcess operations = new TranslationProcess();
                                    operations.GOPStandardProcess(adminservice, tracingService, parentCase, context.InitiatingUserId, gop);
                                    Entity caseUpdate = service.Retrieve(gop.LogicalName, gop.Id, new ColumnSet("jarvis_translationstatuscomment"));
                                    OptionSetValue translationStatusLoc = (OptionSetValue)caseUpdate.Attributes["jarvis_translationstatuscomment"];
                                    if (translationStatusLoc.Value != 334030001)
                                    {
                                        passOutUpdate["jarvis_translationstatuscomment"] = new OptionSetValue(334030001);
                                        service.Update(passOutUpdate);
                                    }
                                }

                                if (gop.Contains("jarvis_gopreason") && gop.Attributes["jarvis_gopreason"] != null)
                                {
                                    Entity passOutUpdate = new Entity(gop.LogicalName);
                                    passOutUpdate.Id = gop.Id;
                                    TranslationProcess operations = new TranslationProcess();
                                    operations.GOPStandardProcess(adminservice, tracingService, parentCase, context.InitiatingUserId, gop);
                                    Entity caseUpdate = service.Retrieve(gop.LogicalName, gop.Id, new ColumnSet("jarvis_translationstatusgopreason"));
                                    OptionSetValue translationStatusLoc = (OptionSetValue)caseUpdate.Attributes["jarvis_translationstatusgopreason"];
                                    if (translationStatusLoc.Value != 334030001)
                                    {
                                        passOutUpdate["jarvis_translationstatusgopreason"] = new OptionSetValue(334030001);
                                        service.Update(passOutUpdate);
                                    }
                                }
                            }
                            #endregion
                        }
                    }
#pragma warning restore SA1123 // Do not place regions within elements

                    #endregion

#pragma warning disable SA1123 // Do not place regions within elements
                    #region Update

                    if (context.Stage == 40 && context.MessageName.ToUpper() == "UPDATE")
                    {
                        IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                        IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
                        IOrganizationService adminservice = serviceFactory.CreateOrganizationService(null);
                        string modifiedBy = string.Empty;
                        EntityCollection systemuser = adminservice.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getModifiedUser, context.UserId)));
                        if (systemuser != null && systemuser.Entities.Count > 0)
                        {
                            modifiedBy = (string)systemuser.Entities[0].Attributes["fullname"];
                        }

                        Entity gop = (Entity)context.InputParameters["Target"];
                        Entity gopImg = (Entity)context.PostEntityImages["PostImage"];
                        bool isApproved = false;
                        bool setStage = false;
                        decimal gopinAmnt = 0;
                        decimal gopOutAmnt = 0;
                        EntityReference gopinCurrency = new EntityReference();
                        EntityReference gopOutCurrency = new EntityReference();
                        bool isAutomate = false;
                        isAutomate = CrmHelper.GetAutomationConfig(service, MCS.Jarvis.CE.Plugins.Constants.JarvisConfiguration.Automationcasestatuschange, tracingService);
                        if (gopImg.Attributes.Contains("jarvis_incident") && gopImg.Attributes["jarvis_incident"] != null)
                        {
                            EntityReference incident = (EntityReference)gopImg.Attributes["jarvis_incident"];
                            Entity incidentRetrieve = service.Retrieve(incident.LogicalName, incident.Id, new ColumnSet("statuscode", "jarvis_homedealer", "jarvis_callerrole", "jarvis_country", "jarvis_sourceid", "jarvis_onetimecustomercountry", "jarvis_onetimecustomerlanguage"));
                            Entity parentCase = new Entity(incident.LogicalName);
                            parentCase.Id = incident.Id;

                            if (gopImg.Attributes.Contains("jarvis_gopapproval") && gopImg.Attributes["jarvis_gopapproval"] != null)
                            {
                                tracingService.Trace("got gop approval");
                                var gopApprovalValue = (OptionSetValue)gopImg.Attributes["jarvis_gopapproval"];
                                if (gopApprovalValue.Value == 334030001)
                                {
                                    isApproved = true;
                                }
#pragma warning disable SA1123 // Do not place regions within elements
                                #region Case Monitor Action Automation - Volvo Pay Cancellation

                                if (gopImg.Attributes.Contains("jarvis_requesttype") && gopImg.Attributes["jarvis_requesttype"] != null)
                                {
                                    OptionSetValue gopPaymentType = new OptionSetValue();
                                    if (gopImg.Attributes.Contains("jarvis_paymenttype") && gopImg.Attributes["jarvis_paymenttype"] != null)
                                    {
                                        gopPaymentType = (OptionSetValue)gopImg.Attributes["jarvis_paymenttype"];
                                    }

                                    if (gopPaymentType.Value == 334030002)
                                    {
                                        CaseMonitorProcess operation = new CaseMonitorProcess();
                                        if (gopApprovalValue.Value == 334030002 && modifiedBy.ToUpperInvariant().Contains("PAY")) // Declined
                                        {
                                            string isoLangCode = string.Empty;
                                            string isCountryCode = string.Empty;
                                            if (incidentRetrieve.Attributes.Contains("jarvis_onetimecustomercountry") && incidentRetrieve.Attributes["jarvis_onetimecustomercountry"] != null)
                                            {
                                                EntityReference hdcountry = (EntityReference)incidentRetrieve.Attributes["jarvis_onetimecustomercountry"];
                                                Entity country = service.Retrieve(hdcountry.LogicalName, hdcountry.Id, new ColumnSet("jarvis_iso2countrycode"));
                                                if (country.Attributes.Contains("jarvis_iso2countrycode") && country.Attributes["jarvis_iso2countrycode"] != null)
                                                {
                                                    isCountryCode = (string)country.Attributes["jarvis_iso2countrycode"];
                                                }
                                            }

                                            if (incidentRetrieve.Attributes.Contains("jarvis_onetimecustomerlanguage") && incidentRetrieve.Attributes["jarvis_onetimecustomerlanguage"] != null)
                                            {
                                                EntityReference language = (EntityReference)incidentRetrieve.Attributes["jarvis_onetimecustomerlanguage"];
                                                Entity hdlanguage = service.Retrieve(language.LogicalName, language.Id, new ColumnSet("jarvis_iso3languagecode6392t"));
                                                if (hdlanguage.Attributes.Contains("jarvis_iso3languagecode6392t") && hdlanguage.Attributes["jarvis_iso3languagecode6392t"] != null)
                                                {
                                                    isoLangCode = (string)hdlanguage.Attributes["jarvis_iso3languagecode6392t"];
                                                }
                                            }

                                            if (!string.IsNullOrEmpty(isoLangCode) || !string.IsNullOrEmpty(isCountryCode))
                                            {
                                                string fucomment = isoLangCode + " " + isCountryCode + " " + "Volvo Pay cancellation";
                                                operation.AutomateMonitorCreation(incidentRetrieve, fucomment, 1, 3, 0, string.Empty, service);
                                            }

                                        }

                                        if (gopApprovalValue.Value == 334030002)
                                        {
                                            // Close CC Chase Payment
                                            string fucomments = "Chase CC Payment";
                                            operation.AutomateCloseMonitorActions(incidentRetrieve, fucomments, 1, fucomments, service);
                                        }
                                    }
                                }
#pragma warning restore SA1123 // Do not place regions within elements

                                #endregion
                                if (isApproved)
                                {
#pragma warning disable SA1123 // Do not place regions within elements
                                    #region Commented Code

                                    //// if (gopImg.Attributes.Contains("jarvis_goplimitout") && gopImg.Attributes["jarvis_goplimitout"] != null)
                                    //// {
                                    ////    gopOutAmnt = (decimal)gopImg.Attributes["jarvis_goplimitout"];
                                    ////    parentCase["jarvis_totalgoplimitoutapproved"] = gopOutAmnt;
                                    //// }
                                    //// if (gopImg.Attributes.Contains("jarvis_gopoutcurrency") && gopImg.Attributes["jarvis_gopoutcurrency"] != null)
                                    //// {
                                    ////    gopOutCurrency = (EntityReference)gopImg.Attributes["jarvis_gopoutcurrency"];
                                    ////    parentCase["jarvis_totalgoplimitoutapprovedcurrency"] = gopOutCurrency;
                                    //// }
                                    //// if (gopImg.Attributes.Contains("jarvis_goplimitin") && gopImg.Attributes["jarvis_goplimitin"] != null)
                                    //// {
                                    ////    gopinAmnt = (decimal)gopImg.Attributes["jarvis_goplimitin"];
                                    ////    parentCase["jarvis_totalgoplimitinapproved"] = gopinAmnt;
                                    //// }
                                    //// if (gopImg.Attributes.Contains("jarvis_gopincurrency") && gopImg.Attributes["jarvis_gopincurrency"] != null)
                                    //// {
                                    ////    gopinCurrency = (EntityReference)gopImg.Attributes["jarvis_gopincurrency"];
                                    ////    parentCase["jarvis_totalcurrencyinapproved"] = gopinCurrency;
                                    //// }
                                    //// if (gopImg.Attributes.Contains("jarvis_requesttype") && gopImg.Attributes["jarvis_requesttype"] != null)
                                    //// {
                                    ////    OptionSetValue gopRequestType = (OptionSetValue)gopImg.Attributes["jarvis_requesttype"];
                                    ////    if (gopRequestType.Value == 334030000)//GOP
                                    ////    {
                                    #endregion

                                    if (incidentRetrieve.Attributes.Contains("statuscode") && incidentRetrieve.Attributes["statuscode"] != null)
                                    {
                                        OptionSetValue status = (OptionSetValue)incidentRetrieve.Attributes["statuscode"];
                                        if (status.Value == 20)
                                        {
                                            setStage = true;
                                        }

#pragma warning disable SA1123 // Do not place regions within elements
                                        #region Case Monitor Action Automation - GOP+

                                        if (gop.Attributes.Contains("jarvis_paymenttype") || gop.Attributes.Contains("jarvis_gopapproval"))
                                        {
                                            if (gopImg.Attributes.Contains("createdby") && gopImg.Attributes["createdby"] != null)
                                            {
                                                EntityReference createdBy = (EntityReference)gopImg.Attributes["createdby"];
                                                if (!createdBy.Name.ToUpperInvariant().Contains("MERCURIUS"))
                                                {
                                                    if (gopImg.Attributes.Contains("jarvis_paymenttype") && gopImg.Attributes["jarvis_paymenttype"] != null)
                                                    {
                                                        // 334030002
                                                        OptionSetValue paymentType = (OptionSetValue)gopImg.Attributes["jarvis_paymenttype"];
                                                        if (paymentType.Value == 334030002)
                                                        {
                                                            if (gopImg.Attributes.Contains("jarvis_gopapproval") && gopImg.Attributes["jarvis_gopapproval"] != null)
                                                            {
                                                                OptionSetValue gopApproval = (OptionSetValue)gopImg.Attributes["jarvis_gopapproval"];

#pragma warning disable SA1123 // Do not place regions within elements
                                                                #region Pending

                                                                if (gopApproval.Value == 334030000)
                                                                {
                                                                    string isoLangCode = string.Empty;
                                                                    string isCountryCode = string.Empty;
                                                                    if (incidentRetrieve.Attributes.Contains("jarvis_onetimecustomercountry") && incidentRetrieve.Attributes["jarvis_onetimecustomercountry"] != null)
                                                                    {
                                                                        EntityReference hdcountry = (EntityReference)incidentRetrieve.Attributes["jarvis_onetimecustomercountry"];
                                                                        Entity country = service.Retrieve(hdcountry.LogicalName, hdcountry.Id, new ColumnSet("jarvis_iso2countrycode"));
                                                                        if (country.Attributes.Contains("jarvis_iso2countrycode") && country.Attributes["jarvis_iso2countrycode"] != null)
                                                                        {
                                                                            isCountryCode = (string)country.Attributes["jarvis_iso2countrycode"];
                                                                        }
                                                                    }

                                                                    if (incidentRetrieve.Attributes.Contains("jarvis_onetimecustomerlanguage") && incidentRetrieve.Attributes["jarvis_onetimecustomerlanguage"] != null)
                                                                    {
                                                                        EntityReference language = (EntityReference)incidentRetrieve.Attributes["jarvis_onetimecustomerlanguage"];
                                                                        Entity hdlanguage = service.Retrieve(language.LogicalName, language.Id, new ColumnSet("jarvis_iso3languagecode6392t"));
                                                                        if (hdlanguage.Attributes.Contains("jarvis_iso3languagecode6392t") && hdlanguage.Attributes["jarvis_iso3languagecode6392t"] != null)
                                                                        {
                                                                            isoLangCode = (string)hdlanguage.Attributes["jarvis_iso3languagecode6392t"];
                                                                        }
                                                                    }

                                                                    if (!string.IsNullOrEmpty(isoLangCode) || !string.IsNullOrEmpty(isCountryCode))
                                                                    {
                                                                        CaseMonitorProcess operation = new CaseMonitorProcess();
                                                                        string fucomment = isoLangCode + " " + isCountryCode + " " + "Chase CC Payment";
                                                                        operation.AutomateMonitorCreationTime(incidentRetrieve, fucomment, 1, 5, 0, string.Empty, DateTime.UtcNow, 15, service);
                                                                    }
                                                                }
#pragma warning restore SA1123 // Do not place regions within elements

                                                                #endregion

                                                            }
                                                        }

                                                        OptionSetValue gopType = (OptionSetValue)gopImg.Attributes["jarvis_requesttype"];
                                                    }
                                                }
                                            }

                                            if (gopImg.Attributes.Contains("jarvis_paymenttype") && gopImg.Attributes["jarvis_paymenttype"] != null && gopImg.Attributes.Contains("jarvis_requesttype") && gopImg.Attributes["jarvis_requesttype"] != null)
                                            {
                                                CaseMonitorProcess operations = new CaseMonitorProcess();
                                                OptionSetValue paymentType = (OptionSetValue)gopImg.Attributes["jarvis_paymenttype"];
                                                OptionSetValue gopType = (OptionSetValue)gopImg.Attributes["jarvis_requesttype"];
                                                if (paymentType.Value == 334030006 && gopType.Value == 334030002)
                                                {
                                                    string fucomment = "Chase OK Cust Whitelist GOP HD";
                                                    operations.AutomateCloseMonitorActions(incidentRetrieve, fucomment, 1, fucomment, service);
                                                }
                                            }
                                        }
#pragma warning restore SA1123 // Do not place regions within elements

                                        #endregion

#pragma warning disable SA1123 // Do not place regions within elements
                                        #region Case Monitor Action Automation - GOP RD

                                        if (gopImg.Attributes.Contains("jarvis_requesttype") && gopImg.Attributes["jarvis_requesttype"] != null)
                                        {
                                            OptionSetValue gopType = (OptionSetValue)gopImg.Attributes["jarvis_requesttype"];
                                            if (gopType.Value == 334030002)
                                            {
                                                if (isApproved)
                                                {
                                                    EntityCollection gopRDCollection = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getGOPRDForCase, incident.Id)));
                                                    if (gopRDCollection.Entities.Count > 0)
                                                    {
                                                        if (gopImg.Attributes.Contains("jarvis_repairingdealer") && gopImg.Attributes["jarvis_repairingdealer"] != null)
                                                        {
                                                            EntityReference repairingDealerGOPRD = (EntityReference)gopImg.Attributes["jarvis_repairingdealer"];
                                                            Entity repairingDealerAccount = service.Retrieve(repairingDealerGOPRD.LogicalName, repairingDealerGOPRD.Id, new ColumnSet("jarvis_repairingdealer"));
                                                            EntityReference homeDealer = (EntityReference)repairingDealerAccount.Attributes["jarvis_repairingdealer"];
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
                                                                //// string fucomment = isoLangCode + " " + isCountryCode + " " + "Pass GOP RD";
                                                                string fucomment = isoLangCode + " " + isCountryCode + " " + "Pass GOP";
                                                                operations.AutomateMonitorCreation(incidentRetrieve, fucomment, 1, 5, 0, string.Empty, service);
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
#pragma warning restore SA1123 // Do not place regions within elements
                                        #endregion

#pragma warning disable SA1123 // Do not place regions within elements
                                        #region Case MA Closure On Update
                                        if (gop.Attributes.Contains("jarvis_gopapproval") && gop.Attributes["jarvis_gopapproval"] != null)
                                        {
                                            tracingService.Trace("got gop approval");
                                            var gopApproval = (OptionSetValue)gop.Attributes["jarvis_gopapproval"];
                                            if (gopApproval.Value == 334030001)
                                            {
                                                CaseMonitorProcess operation = new CaseMonitorProcess();
                                                //// Check if all GOP related to the case are approved
                                                bool closeMA = operation.CheckUnapprovedGOP(gop, incidentRetrieve, service, tracingService);
                                                //// If no unapproved GOP then close MA Chase GOP
                                                if (closeMA)
                                                {
                                                    string fucomments = "Chase GOP,Chase OK Cust Whitelist GOP HD";
                                                    operation.AutomateCloseMonitorActions(incidentRetrieve, fucomments, 1, fucomments, service);
                                                    EntityCollection passOutCollection = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.CaseActivePassouts, incidentRetrieve.Id)));
                                                    foreach (var passout in passOutCollection.Entities)
                                                    {
                                                        EntityReference repairingDealerpassOut = (EntityReference)passout.Attributes["jarvis_repairingdealer"];
                                                        string countryLangCode = operation.GetCountryAndLanguageCode(repairingDealerpassOut, service, tracingService);
                                                        if (!string.IsNullOrEmpty(countryLangCode))
                                                        {
                                                            fucomments = countryLangCode + " " + "Pass GOP";
                                                            operation.AutomateMonitorCreation(incidentRetrieve, fucomments, 1, 5, 0, string.Empty, service);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        #endregion

                                        if (gopImg.Attributes.Contains("jarvis_paymenttype") && gopImg.Attributes["jarvis_paymenttype"] != null)
                                        {
                                            CaseMonitorProcess operations = new CaseMonitorProcess();
                                            OptionSetValue paymentType = (OptionSetValue)gopImg.Attributes["jarvis_paymenttype"];
                                            if (paymentType.Value == 334030002) // Credit Card
                                            {
                                                string fuCCcomment = "Chase CC Payment";
                                                operations.AutomateCloseMonitorActions(incidentRetrieve, fuCCcomment, 1, fuCCcomment, service);
                                            }
                                        }
                                    }
#pragma warning restore SA1123 // Do not place regions within elements

                                }
                            }

#pragma warning disable SA1123 // Do not place regions within elements
                            #region Update Case

                            // service.Update(parentCase);
                            if (setStage && isAutomate)
                            {
                                this.ExecuteBPF(incident, Constants.Incident.BpfStage3, service, tracingService);
                            }
#pragma warning restore SA1123 // Do not place regions within elements
                            #endregion

#pragma warning disable SA1123 // Do not place regions within elements
                            #region GOP Translation
                            bool isAutomation = false;
#pragma warning restore SA1123 // Do not place regions within elements
                            isAutomation = CrmHelper.GetAutomationConfig(service, MCS.Jarvis.CE.Plugins.Constants.JarvisConfiguration.Automationtranslation, tracingService);
                            if (isAutomation)
                            {
                                if (gop.Contains("jarvis_comment") && gop.Attributes["jarvis_comment"] != null)
                                {
                                    Entity passOutUpdate = new Entity(gop.LogicalName);
                                    passOutUpdate.Id = gop.Id;
                                    TranslationProcess operations = new TranslationProcess();
                                    operations.GOPStandardProcess(adminservice, tracingService, parentCase, context.InitiatingUserId, gop);
                                    Entity caseUpdate = service.Retrieve(gop.LogicalName, gop.Id, new ColumnSet("jarvis_translationstatuscomment"));
                                    OptionSetValue translationStatusLoc = (OptionSetValue)caseUpdate.Attributes["jarvis_translationstatuscomment"];
                                    if (translationStatusLoc.Value != 334030001)
                                    {
                                        passOutUpdate["jarvis_translationstatuscomment"] = new OptionSetValue(334030001);
                                        service.Update(passOutUpdate);
                                    }
                                }

                                if (gop.Contains("jarvis_gopreason") && gop.Attributes["jarvis_gopreason"] != null)
                                {
                                    Entity passOutUpdate = new Entity(gop.LogicalName);
                                    passOutUpdate.Id = gop.Id;
                                    TranslationProcess operations = new TranslationProcess();
                                    operations.GOPStandardProcess(adminservice, tracingService, parentCase, context.InitiatingUserId, gop);
                                    Entity caseUpdate = service.Retrieve(gop.LogicalName, gop.Id, new ColumnSet("jarvis_translationstatusgopreason"));
                                    OptionSetValue translationStatusLoc = (OptionSetValue)caseUpdate.Attributes["jarvis_translationstatusgopreason"];
                                    if (translationStatusLoc.Value != 334030001)
                                    {
                                        passOutUpdate["jarvis_translationstatusgopreason"] = new OptionSetValue(334030001);
                                        service.Update(passOutUpdate);
                                    }
                                }
                            }
                            #endregion
                        }
                    }
#pragma warning restore SA1123 // Do not place regions within elements
                    #endregion
                }
            }
            catch (InvalidPluginExecutionException ex)
            {
                throw new InvalidPluginExecutionException("Error in GOP Operations " + ex.Message + string.Empty);
            }
        }

        /// <summary>
        /// execute BPF.
        /// </summary>
        /// <param name="incidentRetrieve">Entity Reference.</param>
        /// <param name="stageName">stage name .</param>
        /// <param name="service">service Provider.</param>
        /// <param name="tracingService">tracing Provider.</param>
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
                tracingService.Trace("Stages Count:" + pathResp.ProcessStages.Entities.Count + string.Empty);
                for (int i = 0; i < pathResp.ProcessStages.Entities.Count; i++)
                {
                    // Retrieve the active stage name and active stage position based on the activeStageId for the process instance
                    if (pathResp.ProcessStages.Entities[i].Attributes["stagename"].ToString() == activeStageName)
                    {
                        tracingService.Trace("StageName:" + activeStageName + "  " + ((Guid)pathResp.ProcessStages.Entities[i].Attributes["processstageid"]).ToString() + string.Empty);
                        Guid processStageID = (Guid)pathResp.ProcessStages.Entities[i].Attributes["processstageid"];
                        int activeStagePosition = i;
                        Entity retrievedProcessInstance = new Entity("jarvis_vasbreakdownprocess");
                        tracingService.Trace("process name:" + activeProcessInstance.LogicalName + string.Empty);
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
    }
}
