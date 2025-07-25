//-----------------------------------------------------------------------
// <copyright file="GOPCalculation.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace MCS.Jarvis.CE.BusinessProcessesShared.GOP
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IdentityModel.Protocols.WSTrust;
    using System.Linq;
    using System.Runtime.Remoting.Services;
    using System.Security.Policy;
    using System.Text;
    using System.Threading.Tasks;
    using MCS.Jarvis.CE.BusinessProcessesShared.Helpers;
    using MCS.Jarvis.CE.Commons;
    using MCS.Jarvis.CE.Commons.Entities;
    using MCS.Jarvis.CE.Plugins;
    using Microsoft.Crm.Sdk.Messages;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Messages;
    using Microsoft.Xrm.Sdk.Query;
    using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar;
    using static MCS.Jarvis.CE.BusinessProcessesShared.Helpers.Constants;
    using static MCS.Jarvis.CE.Commons.Helpers.Constants;
    using static MCS.Jarvis.CE.Plugins.Constants;
    using Constants = Helpers.Constants;
    using JarvisConfiguration = MCS.Jarvis.CE.BusinessProcessesShared.Helpers.Constants.JarvisConfiguration;

    /// <summary>
    /// Tis Class handles all GOP Related Functionalities.
    /// </summary>
    public class GOPCalculation
    {
        /// <summary>
        /// PaymentType.
        /// </summary>
        public enum PaymentType : int
        {
            /// <summary>
            /// Cash Value
            /// </summary>
            Cash = 334030000,

            /// <summary>
            /// Contract Value
            /// </summary>
            Contract = 334030001,

            /// <summary>
            /// Credit Card Value
            /// </summary>
            Credit_Card = 334030002,

            /// <summary>
            /// GOP value
            /// </summary>
            GOP = 334030003,

            /// <summary>
            /// Warranty Value
            /// </summary>
            Warranty = 334030005,

            /// <summary>
            /// Whitelist Value
            /// </summary>
            Whitelist = 334030006,

            /// <summary>
            /// RD_HD Value
            /// </summary>
            RD_HD = 334030007,
        }

        /// <summary>
        /// Responsible for Checking Conditions If calculatiuons needs to be performed while GOp Creation.
        /// </summary>
        /// <param name="gop">target entity.</param>
        /// <param name="gopImg">pre Image.</param>
        /// <param name="service">Org Service.</param>
        /// <param name="tracingService">Tracing Service.</param>
        /// <param name="adminservice">Admin Service.</param>
        public void GopCreateOperations(Entity gop, Entity gopImg, IOrganizationService service, ITracingService tracingService, IOrganizationService adminservice)
        {
            try
            {
                if (((OptionSetValue)gop.Attributes["jarvis_source_"]).Value != 334030002 && ((OptionSetValue)gop.Attributes["jarvis_source_"]).Value != 334030005)
                {
                    // Exception on blacklisted Dealer if source
                    if (gop.Attributes.Contains("jarvis_dealer") && gop.Attributes["jarvis_dealer"] != null)
                    {
                        tracingService.Trace("source" + ((OptionSetValue)gop.Attributes["jarvis_source_"]).Value);
                        EntityReference gopDealer = (EntityReference)gop.Attributes["jarvis_dealer"];
                        Entity jarvisDealer = service.Retrieve(gopDealer.LogicalName, gopDealer.Id, new ColumnSet("jarvis_blacklisted"));
                        if (jarvisDealer != null && jarvisDealer.Attributes.Contains("jarvis_blacklisted") && (bool)jarvisDealer.Attributes["jarvis_blacklisted"])
                        {
                            throw new InvalidPluginExecutionException("You cannot save the GOP because the Home Dealer is blacklisted, please change the GOP Dealer");
                        }
                    }
                }

                if (gop.Attributes.Contains("jarvis_parentgop") && gop.Attributes["jarvis_parentgop"] != null)
                {
                    this.OnCopyCancelParentGop(gop, gopImg, adminservice, tracingService);
                }

                if (((OptionSetValue)gop.Attributes["jarvis_source_"]).Value.Equals(334030003) || ((OptionSetValue)gop.Attributes["jarvis_source_"]).Value.Equals(334030002))
                {
                    // Entity gopImg = (Entity)context.PreEntityImages["PreGOPImage"];
                    decimal gopinAmnt = 0;
                    decimal gopOutAmnt = 0;
                    decimal serviceFee = 0;
                    decimal exchangeValue = 1;
                    decimal outexchangeValue = 1;
                    EntityReference gopinCurrency = new EntityReference();
                    EntityReference gopOutCurrency = new EntityReference();
                    EntityReference servicefeeEntity = new EntityReference();
                    EntityReference incident = new EntityReference();
                    bool gOPInChanged = false;
                    bool gOPOutChanged = false;

                    // Service fee calculation from case Service Line
                    if (gop.Attributes.Contains("jarvis_incident") && gop.Attributes["jarvis_incident"] != null)
                    {
                        incident = (EntityReference)gop.Attributes["jarvis_incident"];
                    }

                    tracingService.Trace("Set GOP Dealer function call.");
                    this.SetGOPdealer(gop, gopImg, incident, service, tracingService);
                    tracingService.Trace("GetStaircaseFees function call.");
                    Entity staircaseFee = this.GetStaircaseFees(gop, incident, service, tracingService);

                    if (staircaseFee != null)
                    {
                        if (staircaseFee.Attributes.Contains("transactioncurrencyid"))
                        {
                            servicefeeEntity = (EntityReference)staircaseFee.Attributes["transactioncurrencyid"];
                        }

                        if (staircaseFee.Attributes.Contains("jarvis_staircasefee"))
                        {
                            serviceFee = ((Money)staircaseFee.Attributes["jarvis_staircasefee"]).Value;
                            tracingService.Trace("staircase fee not null." + serviceFee.ToString());
                        }
                    }
                    else
                    {
                        serviceFee = 0;
                    }

                    if (gop.Attributes.Contains("jarvis_goplimitout") && gop.Attributes["jarvis_goplimitout"] != null)
                    {
                        gopOutAmnt = (decimal)gop.Attributes["jarvis_goplimitout"];

                        gOPOutChanged = true;
                    }

                    if (gop.Attributes.Contains("jarvis_gopoutcurrency") && gop.Attributes["jarvis_gopoutcurrency"] != null)
                    {
                        gopOutCurrency = (EntityReference)gop.Attributes["jarvis_gopoutcurrency"];
                    }

                    if (gop.Attributes.Contains("jarvis_goplimitin") && gop.Attributes["jarvis_goplimitin"] != null)
                    {
                        gopinAmnt = (decimal)gop.Attributes["jarvis_goplimitin"];

                        gOPInChanged = true;
                    }

                    if (gop.Attributes.Contains("jarvis_gopincurrency") && gop.Attributes["jarvis_gopincurrency"] != null)
                    {
                        gopinCurrency = (EntityReference)gop.Attributes["jarvis_gopincurrency"];
                    }

                    if (gOPInChanged || (gop.Attributes.Contains("jarvis_gopoutcurrency") && gop.Attributes["jarvis_gopoutcurrency"] != null) || (gop.Attributes.Contains("jarvis_gopincurrency") && gop.Attributes["jarvis_gopincurrency"] != null))
                    {
                        if (gopOutCurrency.Id.Equals(Guid.Empty))
                        {
                            gopOutCurrency = gopinCurrency;
                            gop["jarvis_gopoutcurrency"] = gopinCurrency;
                        }

                        if (servicefeeEntity != null)
                        {
                            exchangeValue = this.CurrencyExchange(servicefeeEntity.Id, gopinCurrency.Id, service);
                        }
                        else
                        {
                            exchangeValue = 1;
                        }

                        outexchangeValue = this.CurrencyExchange(gopinCurrency.Id, gopOutCurrency.Id, service);
                        decimal calculateGOPOut = (gopinAmnt - (serviceFee * exchangeValue)) * outexchangeValue;
                        if (calculateGOPOut < 0)
                        {
                            calculateGOPOut = 0;
                        }

                        gop["jarvis_goplimitout"] = calculateGOPOut;
                    }

                    if (gOPOutChanged)
                    {
                        if (gopinCurrency.Id.Equals(Guid.Empty))
                        {
                            gopinCurrency = gopOutCurrency;
                            gop["jarvis_gopincurrency"] = gopOutCurrency;
                        }

                        if (servicefeeEntity != null)
                        {
                            exchangeValue = this.CurrencyExchange(servicefeeEntity.Id, gopOutCurrency.Id, service);
                        }
                        else
                        {
                            exchangeValue = 1;
                        }

                        outexchangeValue = this.CurrencyExchange(gopOutCurrency.Id, gopinCurrency.Id, service);
                        decimal calculateGOPIn = (gopOutAmnt + (serviceFee * exchangeValue)) * outexchangeValue;

                        gop["jarvis_goplimitin"] = calculateGOPIn;
                        gop["jarvis_goplimitout"] = gopOutAmnt;
                    }

                    this.CalculateTotalLimitOut(gop, gopImg, service, serviceFee, servicefeeEntity, "create", tracingService, adminservice);
                }
            }
            catch (InvalidPluginExecutionException ex)
            {
                tracingService.Trace(ex.Message);
                throw new InvalidPluginExecutionException("Error in GOP Create Operations :" + ex.Message + string.Empty);
            }
        }

        /// <summary>
        /// Responsible for Checking Conditions If calculatiuons needs to be performed while GOp Updation.
        /// </summary>
        /// <param name="gop">target entity.</param>
        /// <param name="gopImg">pre Image.</param>
        /// <param name="service">Org Service.</param>
        /// <param name="tracingService">Tracing Service.</param>
        /// <param name="adminservice">Admin Service.</param>
        public void GopUpdateOperations(Entity gop, Entity gopImg, IOrganizationService service, ITracingService tracingService, IOrganizationService adminservice)
        {
            decimal gopinAmnt = 0;
            decimal gopOutAmnt = 0;
            decimal serviceFee = 45;
            decimal exchangeValue = 0;
            decimal outexchangeValue = 0;
            int source = 0;
            EntityReference gopinCurrency = new EntityReference();
            EntityReference gopOutCurrency = new EntityReference();
            EntityReference servicefeeEntity = new EntityReference();
            EntityReference incident = new EntityReference();
            bool gOPInChanged = false;
            bool gOPOutChanged = false;
            bool isShadowrecord = false;
            bool isCopied = false;
            bool isInactive = false;
            OptionSetValue requesttype = new OptionSetValue();
            int paymenttype = 0;
            try
            {
                if (gopImg.Attributes.Contains("jarvis_incident") && gopImg.Attributes["jarvis_incident"] != null)
                {
                    incident = (EntityReference)gopImg.Attributes["jarvis_incident"];
                }

                isInactive = gop.Attributes.Contains("statecode") && ((OptionSetValue)gop.Attributes["statecode"]).Value.Equals(1);

                if (gop.Attributes.Contains("jarvis_source_") && gop.Attributes["jarvis_source_"] != null)
                {
                    source = ((OptionSetValue)gop.Attributes["jarvis_source_"]).Value;
                }
                else if (gopImg.Attributes.Contains("jarvis_source_") && gopImg.Attributes["jarvis_source_"] != null)
                {
                    source = ((OptionSetValue)gopImg.Attributes["jarvis_source_"]).Value;
                }

                if (gop.Attributes.Contains("jarvis_paymenttype") && gop.Attributes["jarvis_paymenttype"] != null)
                {
                    paymenttype = ((OptionSetValue)gop.Attributes["jarvis_paymenttype"]).Value;
                }
                else if (gopImg.Attributes.Contains("jarvis_paymenttype") && gopImg.Attributes["jarvis_paymenttype"] != null)
                {
                    paymenttype = ((OptionSetValue)gopImg.Attributes["jarvis_paymenttype"]).Value;
                }

                if (gop.Attributes.Contains("jarvis_relatedgop") && gop.Attributes["jarvis_relatedgop"] != null && requesttype.Value == 334030001)
                {
                    isShadowrecord = true;
                }

                if (gop.Attributes.Contains("statuscode") && gop.Attributes["statuscode"] != null && ((OptionSetValue)gop.Attributes["statuscode"]).Equals(334030001))
                {
                    isCopied = true;
                }

                Entity parentCase = service.Retrieve(incident.LogicalName, incident.Id, new ColumnSet("jarvis_caseserviceline", "statecode"));

                #region GOP-Limit_Calculations
                if (source.Equals(334030003) || source.Equals(334030002))
                {
                    if (!isInactive)
                    {
                        if (gop.Attributes.Contains("jarvis_requesttype") && gop.Attributes["jarvis_requesttype"] != null)
                        {
                            requesttype = (OptionSetValue)gop.Attributes["jarvis_requesttype"];
                        }
                        else if (gopImg.Attributes.Contains("jarvis_requesttype") && gopImg.Attributes["jarvis_requesttype"] != null)
                        {
                            requesttype = (OptionSetValue)gopImg.Attributes["jarvis_requesttype"];
                        }

                        //// Service fee calculation from case Service Line and GOP Dealer Country

                        tracingService.Trace("Set gop dealer function call.");
                        this.SetGOPdealer(gop, gopImg, incident, service, tracingService);
                        tracingService.Trace("Get Staircase fee function call.");
                        Entity staircaseFee = null;
                        if (gop.Attributes.Contains("jarvis_dealer") && gop.Attributes["jarvis_dealer"] != null && !gopImg.Attributes.Contains("jarvis_dealer"))
                        {
                            staircaseFee = this.GetStaircaseFees(gop, incident, service, tracingService);
                        }
                        else if (gopImg.Attributes.Contains("jarvis_dealer") && gopImg.Attributes["jarvis_dealer"] != null)
                        {
                            staircaseFee = this.GetStaircaseFees(gopImg, incident, service, tracingService);
                        }

                        if (staircaseFee != null)
                        {
                            if (staircaseFee.Attributes.Contains("transactioncurrencyid"))
                            {
                                servicefeeEntity = (EntityReference)staircaseFee.Attributes["transactioncurrencyid"];
                            }

                            if (staircaseFee.Attributes.Contains("jarvis_staircasefee"))
                            {
                                serviceFee = ((Money)staircaseFee.Attributes["jarvis_staircasefee"]).Value;
                                tracingService.Trace("service fee amount" + serviceFee.ToString());
                            }
                        }
                        else
                        {
                            serviceFee = 0;
                        }

                        if (gop.Attributes.Contains("jarvis_goplimitout") && gop.Attributes["jarvis_goplimitout"] != null)
                        {
                            gopOutAmnt = (decimal)gop.Attributes["jarvis_goplimitout"];

                            gOPOutChanged = true;
                        }

                        if (gop.Attributes.Contains("jarvis_gopoutcurrency") && gop.Attributes["jarvis_gopoutcurrency"] != null)
                        {
                            gopOutCurrency = (EntityReference)gop.Attributes["jarvis_gopoutcurrency"];
                        }
                        else if (gopImg.Attributes.Contains("jarvis_gopoutcurrency") && gopImg.Attributes["jarvis_gopoutcurrency"] != null)
                        {
                            gopOutCurrency = (EntityReference)gopImg.Attributes["jarvis_gopoutcurrency"];
                        }

                        if (gop.Attributes.Contains("jarvis_goplimitin") && gop.Attributes["jarvis_goplimitin"] != null)
                        {
                            gopinAmnt = (decimal)gop.Attributes["jarvis_goplimitin"];

                            gOPInChanged = true;
                        }
                        else
                        {
                            Entity gopPre = service.Retrieve(gop.LogicalName, gop.Id, new ColumnSet(new string[] { "jarvis_goplimitin" }));
                            if (gopPre.Attributes["jarvis_goplimitin"] != null)
                            {
                                gopinAmnt = (decimal)gopPre.Attributes["jarvis_goplimitin"];
                            }
                        }

                        if (gop.Attributes.Contains("jarvis_gopincurrency") && gop.Attributes["jarvis_gopincurrency"] != null)
                        {
                            gopinCurrency = (EntityReference)gop.Attributes["jarvis_gopincurrency"];
                        }
                        else if (gopImg.Attributes.Contains("jarvis_gopincurrency") && gopImg.Attributes["jarvis_gopincurrency"] != null)
                        {
                            gopinCurrency = (EntityReference)gopImg.Attributes["jarvis_gopincurrency"];
                        }

                        if (gOPInChanged || (gop.Attributes.Contains("jarvis_gopoutcurrency") && gop.Attributes["jarvis_gopoutcurrency"] != null) || (gop.Attributes.Contains("jarvis_gopincurrency") && gop.Attributes["jarvis_gopincurrency"] != null))
                        {
                            if (servicefeeEntity != null)
                            {
                                exchangeValue = this.CurrencyExchange(servicefeeEntity.Id, gopinCurrency.Id, service);
                            }
                            else
                            {
                                exchangeValue = 1;
                            }

                            outexchangeValue = this.CurrencyExchange(gopinCurrency.Id, gopOutCurrency.Id, service);
                            decimal calculateGOPOut = (gopinAmnt - (serviceFee * exchangeValue)) * outexchangeValue;
                            if (calculateGOPOut < 0)
                            {
                                calculateGOPOut = 0;
                            }

                            gop["jarvis_goplimitout"] = calculateGOPOut;
                            gOPInChanged = true;
                        }

                        if (gOPOutChanged)
                        {
                            if (servicefeeEntity != null)
                            {
                                exchangeValue = this.CurrencyExchange(servicefeeEntity.Id, gopOutCurrency.Id, service);
                            }
                            else
                            {
                                exchangeValue = 1;
                            }

                            outexchangeValue = this.CurrencyExchange(gopOutCurrency.Id, gopinCurrency.Id, service);

                            decimal calculateGOPIn = (gopOutAmnt + (serviceFee * exchangeValue)) * outexchangeValue;
                            gop["jarvis_goplimitin"] = calculateGOPIn;
                        }
                    }

                    if (gOPInChanged || gOPOutChanged || gop.Attributes.Contains("jarvis_totallimitout") || gop.Attributes.Contains("jarvis_totallimitin") || gop.Attributes.Contains("jarvis_gopapproval") || gop.Attributes.Contains("statecode"))
                    {
                        this.CalculateTotalLimitOut(gop, gopImg, service, serviceFee, servicefeeEntity, "update", tracingService, adminservice);
                    }

                    if (!isShadowrecord && isInactive && !isCopied && paymenttype != 334030002)
                    {
                        if (parentCase.Attributes.Contains("statecode") && !((OptionSetValue)parentCase.Attributes["statecode"]).Value.Equals(1))
                        {
                            this.DeactivateRelatedGops(gop, gopImg, service, serviceFee, servicefeeEntity);
                        }
                    }
                }
                #endregion
            }
            catch (InvalidPluginExecutionException ex)
            {
                tracingService.Trace(ex.Message);
                throw new InvalidPluginExecutionException("Error in GOP Update Operations :" + ex.Message + string.Empty);
            }
        }

        /// <summary>
        /// Responsible for Updating GOP target Entity with Updated values after calculation.
        /// </summary>
        /// <param name="gop">target entity.</param>
        /// <param name="gopImg">pre Image.</param>
        /// <param name="service">Org Service.</param>
        /// <param name="serviceFee">service fee applicable as per the selected case Service line.</param>
        /// <param name="servicefeeEntity">service fee entity applicable as per the selected case Service line.</param>
        /// <param name="type">type.</param>
        /// <param name="tracingService">tracingService.</param>
        /// <param name="adminservice">Admin Service.</param>
        /// <returns>Entity.</returns> Retuens the Gop Entity.
        public Entity CalculateTotalLimitOut(Entity gop, Entity gopImg, IOrganizationService service, decimal serviceFee, EntityReference servicefeeEntity, string type, ITracingService tracingService, IOrganizationService adminservice)
        {
            #region Initialize variables
            EntityReference caseId = new EntityReference();
            EntityReference repairingDealer = new EntityReference();
            EntityReference gopDealer = new EntityReference();
            OptionSetValue requesttype = new OptionSetValue();
            OptionSetValue paymentType = new OptionSetValue();
            decimal totalLimitOut = 0;
            bool approvedGOp = false;
            OptionSetValue gopApproval = new OptionSetValue();
            bool isInactive = false;
            bool paymentTypevaialable = false;
            decimal limitout = 0;
            decimal limitin = 0;
            int source = 0;
            string paymentTypeName = string.Empty;
            EntityReference limitinCurrency = new EntityReference();
            EntityReference limitoutCurrency = new EntityReference();
            EntityReference approvedGopCurrency = new EntityReference();
            EntityReference approvedGopCurrencyIn = new EntityReference();

            if (gop.Attributes.Contains("jarvis_requesttype") && gop.Attributes["jarvis_requesttype"] != null)
            {
                requesttype = (OptionSetValue)gop.Attributes["jarvis_requesttype"];
            }
            else if (gopImg.Attributes.Contains("jarvis_requesttype") && gopImg.Attributes["jarvis_requesttype"] != null)
            {
                requesttype = (OptionSetValue)gopImg.Attributes["jarvis_requesttype"];
            }

            if (gop.Attributes.Contains("jarvis_goplimitin") && gop.Attributes["jarvis_goplimitin"] != null)
            {
                limitin = (decimal)gop.Attributes["jarvis_goplimitin"];
            }
            else if (gopImg.Attributes.Contains("jarvis_goplimitin") && gopImg.Attributes["jarvis_goplimitin"] != null)
            {
                limitin = (decimal)gopImg.Attributes["jarvis_goplimitin"];
            }

            if (gop.Attributes.Contains("jarvis_goplimitout") && gop.Attributes["jarvis_goplimitout"] != null)
            {
                limitout = (decimal)gop.Attributes["jarvis_goplimitout"];
            }
            else if (gopImg.Attributes.Contains("jarvis_goplimitout") && gopImg.Attributes["jarvis_goplimitout"] != null)
            {
                limitout = (decimal)gopImg.Attributes["jarvis_goplimitout"];
            }

            if (gop.Attributes.Contains("jarvis_gopincurrency") && gop.Attributes["jarvis_gopincurrency"] != null)
            {
                limitinCurrency = (EntityReference)gop.Attributes["jarvis_gopincurrency"];
            }
            else if (gopImg.Attributes.Contains("jarvis_gopincurrency") && gopImg.Attributes["jarvis_gopincurrency"] != null)
            {
                limitinCurrency = (EntityReference)gopImg.Attributes["jarvis_gopincurrency"];
            }

            if (gop.Attributes.Contains("jarvis_gopoutcurrency") && gop.Attributes["jarvis_gopoutcurrency"] != null)
            {
                limitoutCurrency = (EntityReference)gop.Attributes["jarvis_gopoutcurrency"];
            }
            else if (gopImg.Attributes.Contains("jarvis_gopoutcurrency") && gopImg.Attributes["jarvis_gopoutcurrency"] != null)
            {
                limitoutCurrency = (EntityReference)gopImg.Attributes["jarvis_gopoutcurrency"];
            }

            if (gop.Attributes.Contains("jarvis_gopapproval"))
            {
                gopApproval = (OptionSetValue)gop.Attributes["jarvis_gopapproval"];
                if (gopApproval.Value.Equals(334030001))
                {
                    approvedGOp = true;
                }
            }
            else if (gopImg.Attributes.Contains("jarvis_gopapproval"))
            {
                gopApproval = (OptionSetValue)gopImg.Attributes["jarvis_gopapproval"];
                if (gopApproval.Value.Equals(334030001))
                {
                    approvedGOp = true;
                }
            }

            if (gop.Attributes.Contains("statecode"))
            {
                isInactive = ((OptionSetValue)gop.Attributes["statecode"]).Value.Equals(1);
            }
            else if (gopImg.Attributes.Contains("statecode"))
            {
                isInactive = ((OptionSetValue)gopImg.Attributes["statecode"]).Value.Equals(1);
            }

            if (gop.Attributes.Contains("jarvis_paymenttype") && gop.Attributes["jarvis_paymenttype"] != null)
            {
                paymentType = (OptionSetValue)gop.Attributes["jarvis_paymenttype"];
                paymentTypevaialable = true;
            }
            else if (gopImg.Attributes.Contains("jarvis_paymenttype") && gopImg.Attributes["jarvis_paymenttype"] != null)
            {
                paymentType = (OptionSetValue)gopImg.Attributes["jarvis_paymenttype"];
                paymentTypevaialable = true;
            }

            if (paymentType != null)
            {
                paymentTypeName = Enum.GetName(typeof(PaymentType), paymentType.Value);
            }

            if (gop.Attributes.Contains("jarvis_source_") && gop.Attributes["jarvis_source_"] != null)
            {
                source = ((OptionSetValue)gop.Attributes["jarvis_source_"]).Value;
            }
            else if (gopImg.Attributes.Contains("jarvis_source_") && gopImg.Attributes["jarvis_source_"] != null)
            {
                source = ((OptionSetValue)gopImg.Attributes["jarvis_source_"]).Value;
            }

            if (gop.Attributes.Contains("jarvis_incident") && gop.Attributes["jarvis_incident"] != null)
            {
                caseId = (EntityReference)gop.Attributes["jarvis_incident"];
            }
            else if (gopImg.Attributes.Contains("jarvis_incident") && gopImg.Attributes["jarvis_incident"] != null)
            {
                caseId = (EntityReference)gopImg.Attributes["jarvis_incident"];
            }

            if (gop.Attributes.Contains("jarvis_repairingdealer") && gop.Attributes["jarvis_repairingdealer"] != null)
            {
                repairingDealer = (EntityReference)gop.Attributes["jarvis_repairingdealer"];
            }
            else if (gopImg.Attributes.Contains("jarvis_repairingdealer") && gopImg.Attributes["jarvis_repairingdealer"] != null)
            {
                repairingDealer = (EntityReference)gopImg.Attributes["jarvis_repairingdealer"];
            }

            if (gopImg.Attributes.Contains("jarvis_dealer") && gopImg.Attributes["jarvis_dealer"] != null)
            {
                gopDealer = (EntityReference)gopImg.Attributes["jarvis_dealer"];
            }

            bool passOutAutomationEnabled = CrmHelper.GetAutomationConfig(service, JarvisConfiguration.Automationavailableamount, tracingService);
            bool volvoPayAutomationEnabled = CrmHelper.GetAutomationConfig(service, JarvisConfiguration.VolvoPayAutomation, tracingService);
            volvoPayAutomationEnabled = paymentType.Value.Equals(334030002) && volvoPayAutomationEnabled;
            bool isMercurius = false;
            bool isMercuriusModifiedUser = false;
            if (volvoPayAutomationEnabled && approvedGOp)
            {
                gop["jarvis_contact"] = "Payment through Volvo Pay";
            }

            if (source.Equals(334030002))
            {
                isMercurius = true;

                EntityReference modifiedby = new EntityReference();
                if (gop.Attributes.Contains("modifiedby"))
                {
                    modifiedby = (EntityReference)gop.Attributes["modifiedby"];
                }
                else if (gopImg.Attributes.Contains("modifiedby"))
                {
                    modifiedby = (EntityReference)gopImg.Attributes["modifiedby"];
                }

                if (modifiedby != null)
                {
                    Entity modifiedUser = service.Retrieve("systemuser", modifiedby.Id, new ColumnSet("fullname"));
                    if (modifiedUser != null && modifiedUser.Attributes.Contains("fullname"))
                    {
                        if (((string)modifiedUser.Attributes["fullname"]).ToUpper().Contains("MERCURIUS"))
                        {
                            isMercuriusModifiedUser = true;
                        }
                        else
                        {
                            isMercuriusModifiedUser = false;
                        }
                    }
                }
            }
            #endregion

            // #157721-[Update] GOP default payment type
            if (isMercuriusModifiedUser && !paymentTypevaialable)
            {
                // get existing GOP PaymentType and set
                this.SetFieldsFromApprovedGOP(gop, gopImg, caseId, service, tracingService);
            }

            #region HD Cancellation and with Related GOP approval

            // #367316-INT: GOP+ approved in Mercurius, GOP RD still unapproved in OneCase
            Entity parentCase = service.Retrieve(caseId.LogicalName, caseId.Id, new ColumnSet("statecode", "jarvis_restgoplimitout", "jarvis_totalrestcurrencyout", "jarvis_totalgoplimitoutapproved", "jarvis_totalgoplimitoutapprovedcurrency", "jarvis_totalbookedamountinclvat", "jarvis_totalcreditcardpaymentamountcurrency", "jarvis_caseserviceline"));

            if (requesttype.Value == 334030001 && gopImg.Attributes.Contains("jarvis_relatedgop") && gopImg.Attributes["jarvis_relatedgop"] != null)
            {
                EntityReference relatedGOP_RD = (EntityReference)gopImg.Attributes["jarvis_relatedgop"];
                Entity updatedRD = service.Retrieve(relatedGOP_RD.LogicalName, relatedGOP_RD.Id, new ColumnSet("jarvis_incident", "jarvis_gopapproval", "jarvis_totallimitout", "jarvis_totallimitin", "jarvis_totallimitoutcurrency", "jarvis_totallimitincurrency", "jarvis_dealer", "jarvis_repairingdealer", "jarvis_goplimitout", "jarvis_paymenttype", "jarvis_gopoutcurrency", "statecode"));
                bool isCopiedCancelled = gop.Attributes.Contains("statuscode") && ((OptionSetValue)gop.Attributes["statuscode"]).Value.Equals(334030001);

                // #101577-HD GOP Cancellation
                if (!isCopiedCancelled && gop.Attributes.Contains("statecode") && ((OptionSetValue)gop.Attributes["statecode"]).Value.Equals(1) && updatedRD != null && updatedRD.Attributes.Contains("statecode") && ((OptionSetValue)updatedRD.Attributes["statecode"]).Value.Equals(0))
                {
                    if (parentCase.Attributes.Contains("statecode") && !((OptionSetValue)parentCase.Attributes["statecode"]).Value.Equals(1))
                    {
                        Entity updateRelatedGOP_RD = new Entity(relatedGOP_RD.LogicalName, relatedGOP_RD.Id);
                        updateRelatedGOP_RD["jarvis_goplimitin"] = (decimal)0;
                        updateRelatedGOP_RD["jarvis_goplimitout"] = (decimal)0;
                        updateRelatedGOP_RD["jarvis_lineage"] = "Cancelled  from Shadow HD";
                        service.Update(updateRelatedGOP_RD);

                        #region UpdateRequest - GOP Deactivation

                        updateRelatedGOP_RD["statecode"] = new OptionSetValue(1);
                        updateRelatedGOP_RD["statuscode"] = new OptionSetValue(2);

                        UpdateRequest updateBPFRequest = new UpdateRequest()
                        {
                            Target = updateRelatedGOP_RD,
                        };
                        service.Execute(updateBPFRequest);

                        #endregion
                    }
                }
                else if (gop.Attributes.Contains("jarvis_gopapproval") && approvedGOp && !isInactive)
                {
                    if (updatedRD != null && updatedRD.Attributes.Contains("jarvis_gopapproval") && !((OptionSetValue)updatedRD.Attributes["jarvis_gopapproval"]).Value.Equals(334030001) && updatedRD.Attributes.Contains("statecode") && ((OptionSetValue)updatedRD.Attributes["statecode"]).Value.Equals(0))
                    {
                        Entity updateRelatedGOP_RD = new Entity(relatedGOP_RD.LogicalName, relatedGOP_RD.Id);
                        if (gop.Attributes.Contains("jarvis_contact"))
                        {
                            updateRelatedGOP_RD["jarvis_contact"] = gop.Attributes["jarvis_contact"];
                            updatedRD["jarvis_contact"] = gop.Attributes["jarvis_contact"];
                        }
                        else
                        {
                            updateRelatedGOP_RD["jarvis_contact"] = "Approved from related HD";
                            updatedRD["jarvis_contact"] = "Approved from related HD";
                        }

                        updateRelatedGOP_RD["jarvis_gopapproval"] = (OptionSetValue)gop.Attributes["jarvis_gopapproval"];
                        updatedRD["jarvis_gopapproval"] = (OptionSetValue)gop.Attributes["jarvis_gopapproval"];
                        if (requesttype != null && (source.Equals(334030003) || (isMercurius && !paymentTypevaialable)))
                        {
                            this.CalculateCaseTotalLimitInOut(updatedRD, gopImg, service, serviceFee, servicefeeEntity, adminservice, tracingService);
                        }

                        // #629815- Update RD with Has been sent Status
                        updateRelatedGOP_RD["statuscode"] = new OptionSetValue(30);
                        updateRelatedGOP_RD["jarvis_lineage"] = "Approved  from Shadow HD";
                        service.Update(updateRelatedGOP_RD);

                        // User Story 90225: GOP+ approval: HD to RD (Flow 2/2)
                        if (passOutAutomationEnabled)
                        {
                            this.UpdatePassoutFromRD(updatedRD, gopImg, service, serviceFee, servicefeeEntity, adminservice);
                        }

                        // tracingService.Trace("CopyUnapprovedRDs from Shadow HD to RD :" + gop.Id);
                        // this.GOPAutoApproval(updatedRD, gopImg, service, serviceFee, servicefeeEntity, tracingService, adminservice);
                        // this.CopyUnapprovedRDs(updatedRD, gopImg, service, adminservice);
                    }

                    // Entity caseObj = service.Retrieve(relatedGOP_RD.LogicalName, relatedGOP_RD.Id, new ColumnSet("jarvis_totalgoplimitoutapproved", "jarvis_totalgoplimitinapproved", "jarvis_totalgoplimitoutapprovedcurrency", "jarvis_totalcurrencyinapproved"));
                }
            }
            #endregion

            #region HD Approval from Eservice/Mercurius

            // 629712 - E service Hd approval approve the Pending requests
            else
            {
                if (requesttype.Value == 334030001 && source.Equals(334030002) && gop.Attributes.Contains("jarvis_gopapproval") && ((OptionSetValue)gop.Attributes["jarvis_gopapproval"]).Value.Equals(334030001))
                {
                    // this.GOPAutoApproval(gop, gopImg, service, serviceFee, servicefeeEntity, tracingService, adminservice);
                    tracingService.Trace("CopyUnapprovedRDs from HD :" + gop.Id);
                    // 609054 609054 Force Copied
                    this.CopyUnapprovedRDs(gop, gopImg, parentCase, serviceFee, servicefeeEntity, service, adminservice, tracingService);
                }
            }
            #endregion

            // Normal GOp Calculation and for Unapproved GOP from Mercurius
            // 189308-Approved -Total GOP IN and Total GOP OUT should be populated from Mercurius => Total IN to OUT calculation must be disabled. (exception: Modified by = "Mercurius" AND approval set to yes"
            if (requesttype != null && (source.Equals(334030003) || (isMercurius && !paymentTypevaialable && requesttype.Value == 334030002)))
            {
                #region RequestType GOP Calculation
                if (requesttype.Value == 334030000)
                {
                    gop["jarvis_totallimitin"] = limitin;
                    gop["jarvis_totallimitout"] = limitout;
                    gop["jarvis_totallimitincurrency"] = limitinCurrency;
                    gop["jarvis_totallimitoutcurrency"] = limitoutCurrency;
                }
                #endregion
                #region Normal GOP HD Calculation

                // UserStory-90225 GOP + approval HD to RD
                else if (requesttype.Value == 334030001) // HD
                {
                    if (!(gopImg.Attributes.Contains("jarvis_relatedgop") && gopImg.Attributes["jarvis_relatedgop"] != null))
                    {
                        if (isInactive)
                        {
                            gop["jarvis_goplimitin"] = Convert.ToDecimal(0.0);
                            gop["jarvis_goplimitout"] = Convert.ToDecimal(0.0);
                            gop["jarvis_totallimitin"] = Convert.ToDecimal(0.0);
                            gop["jarvis_totallimitout"] = Convert.ToDecimal(0.0);
                            gop["jarvis_totallimitincurrency"] = limitinCurrency;
                            gop["jarvis_totallimitoutcurrency"] = limitoutCurrency;

                            this.CalculateCaseTotalLimitInOut(gop, gopImg, service, serviceFee, servicefeeEntity, adminservice, tracingService);
                        }
                        else
                        {
                            gop["jarvis_totallimitin"] = (decimal)Math.Round(limitin, 2, System.MidpointRounding.AwayFromZero);
                            gop["jarvis_totallimitout"] = (decimal)Math.Round(limitout, 2, System.MidpointRounding.AwayFromZero);
                            gop["jarvis_totallimitincurrency"] = limitinCurrency;
                            gop["jarvis_totallimitoutcurrency"] = limitoutCurrency;

                            if (volvoPayAutomationEnabled)
                            {
                                this.CalculateBookingAmount(gop, gopImg, parentCase, service, tracingService, servicefeeEntity, serviceFee, false);
                            }

                            this.CalculateCaseTotalLimitInOut(gop, gopImg, service, serviceFee, servicefeeEntity, adminservice, tracingService);

                            if (gop.Attributes.Contains("jarvis_gopapproval") && ((OptionSetValue)gop.Attributes["jarvis_gopapproval"]).Value.Equals(334030001))
                            {
                                // this.GOPAutoApproval(gop, gopImg, service, serviceFee, servicefeeEntity, tracingService, adminservice);
                                tracingService.Trace("CopyUnapprovedRDs from HD :" + gop.Id);

                                // this.CopyUnapprovedRDs(gop, gopImg, service, adminservice);
                            }
                        }
                    }
                }
                #endregion

                #region RD Calculation
                else if (requesttype.Value == 334030002)
                {
                    if (gopApproval.Value != 334030002)
                    {
                        //// 423126 - Enable/Disable One Case - GOP to Pass Out Available Amount.
                        totalLimitOut = limitout;
                        var requesterName = string.Empty;
                        decimal caseAvialableAmount = 0;
                        EntityReference currencyout = new EntityReference();
                        decimal caseTotalApprovedAmount = 0;
                        bool isforceCopied = false;
                        EntityReference currencyCaseTotalApproved = new EntityReference();
                        if (gop.Attributes.Contains("jarvis_requestedcontact"))
                        {
                            requesterName = "and requested by " + (string)gop.Attributes["jarvis_requestedcontact"];
                        }
                        else if (gopImg.Attributes.Contains("jarvis_requestedcontact"))
                        {
                            requesterName = "and requested by " + (string)gopImg.Attributes["jarvis_requestedcontact"];
                        }

                        Entity caseObj = service.Retrieve("incident", caseId.Id, new ColumnSet("jarvis_restgoplimitout", "jarvis_totalrestcurrencyout", "jarvis_totalgoplimitoutapproved", "jarvis_totalgoplimitoutapprovedcurrency", "jarvis_totalbookedamountinclvat", "jarvis_totalcreditcardpaymentamountcurrency", "jarvis_caseserviceline"));

                        if (caseObj.Attributes.Contains("jarvis_restgoplimitout") && caseObj["jarvis_totalrestcurrencyout"] != null && !volvoPayAutomationEnabled)
                        {
                            caseAvialableAmount = (decimal)caseObj["jarvis_restgoplimitout"];
                            currencyout = (EntityReference)caseObj["jarvis_totalrestcurrencyout"];
                        }

                        if (caseObj.Attributes.Contains("jarvis_totalgoplimitoutapproved") && caseObj["jarvis_totalgoplimitoutapprovedcurrency"] != null)
                        {
                            caseTotalApprovedAmount = (decimal)caseObj["jarvis_totalgoplimitoutapproved"];
                            currencyCaseTotalApproved = (EntityReference)caseObj["jarvis_totalgoplimitoutapprovedcurrency"];
                        }

                        // #101577-HD GOP Cancellation
                        if (isInactive)
                        {
                            gop["jarvis_goplimitin"] = Convert.ToDecimal(0.0);
                            gop["jarvis_goplimitout"] = Convert.ToDecimal(0.0);
                            gop["jarvis_totallimitin"] = Convert.ToDecimal(0.0);
                            gop["jarvis_totallimitout"] = Convert.ToDecimal(0.0);
                            gop["jarvis_totallimitincurrency"] = limitinCurrency;
                            gop["jarvis_totallimitoutcurrency"] = limitoutCurrency;
                            this.CalculateCaseTotalLimitInOut(gop, gopImg, service, serviceFee, servicefeeEntity, adminservice, tracingService);
                        }
                        else
                        {
                            if (gop.Attributes.Contains("jarvis_istranslate") && (bool)gop.Attributes["jarvis_istranslate"] == true)
                            {
                                isforceCopied = true;
                            }

                            if (!isforceCopied)
                            {
                                this.CalculateRD(gop, gopImg, caseObj, service, serviceFee, servicefeeEntity, adminservice, tracingService);

                                this.CalculateCaseTotalLimitInOut(gop, gopImg, service, serviceFee, servicefeeEntity, adminservice, tracingService);
                            }
                        }
                    }
                    else
                    {
                        this.CalculateCaseTotalLimitInOut(gop, gopImg, service, serviceFee, servicefeeEntity, adminservice, tracingService);
                    }

                }
                #endregion
            }
            else
            {
                // 574020- Update passout on Approval from Mercurius
                if (requesttype != null && requesttype.Value == 334030002)
                {
                    if (approvedGOp && passOutAutomationEnabled)
                    {
                        this.UpdatePassoutFromRD(gop, gopImg, service, serviceFee, servicefeeEntity, adminservice);
                    }
                }

                if (isInactive)
                {
                    gop["jarvis_goplimitin"] = Convert.ToDecimal(0.0);
                    gop["jarvis_goplimitout"] = Convert.ToDecimal(0.0);
                    gop["jarvis_totallimitin"] = Convert.ToDecimal(0.0);
                    gop["jarvis_totallimitout"] = Convert.ToDecimal(0.0);
                    gop["jarvis_totallimitincurrency"] = limitinCurrency;
                    gop["jarvis_totallimitoutcurrency"] = limitoutCurrency;
                }

                this.CalculateCaseTotalLimitInOut(gop, gopImg, service, serviceFee, servicefeeEntity, adminservice, tracingService);

                // if (requesttype != null && requesttype.Value == 334030002 && gop.Attributes.Contains("jarvis_gopapproval") && (bool)gop.Attributes["jarvis_gopapproval"])
                // {
                //    tracingService.Trace("CopyUnapprovedRDs from RD :" + gop.Id);
                //    this.CopyUnapprovedRDs(gop, gopImg, service, adminservice);
                // }
            }

            // if (createShadowGOp) Done as part of Post Operation to link with existing RD
            // {
            //    ShadowGOP(gop, gopImg, service, serviceFee, servicefeeEntity);
            // }
            // else
            // {

            // }
            return gop;
        }

        /// <summary>
        /// Currency Exchange.
        /// </summary>
        /// <param name="sourceCurrencyId">Source Currency Id.</param>
        /// <param name="targetCurrencyId">Target Currency Id.</param>
        /// <param name="service">Org service.</param>
        /// <returns>decimal exchange value.</returns>
        public decimal CurrencyExchange(Guid sourceCurrencyId, Guid targetCurrencyId, IOrganizationService service)
        {
            decimal exchangeValue = 0;
            if (sourceCurrencyId == targetCurrencyId)
            {
                exchangeValue = 1;
            }
            else
            {
                EntityCollection eXchangeRates = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.ExchangeRate, sourceCurrencyId, targetCurrencyId)));

                foreach (var exchangeRate in eXchangeRates.Entities)
                {
                    exchangeValue = (decimal)exchangeRate["jarvis_value"];
                }
            }

            return exchangeValue;
        }

        /// <summary>
        /// deactivate Related All GOP.
        /// </summary>
        /// <param name="gop">gop entity.</param>
        /// <param name="gopImg">gop Image.</param>
        /// <param name="service">Org service.</param>
        /// <param name="serviceFee">service Fee.</param>
        /// <param name="servicefeeEntity">service fee Entity.</param>
        public void DeactivateRelatedGops(Entity gop, Entity gopImg, IOrganizationService service, decimal serviceFee, EntityReference servicefeeEntity)
        {
            EntityReference caseId = new EntityReference();
            EntityReference gopDealer = new EntityReference();
            bool alreadyCancelled = false;
            if (gop.Attributes.Contains("jarvis_incident") && gop.Attributes["jarvis_incident"] != null)
            {
                caseId = (EntityReference)gop.Attributes["jarvis_incident"];
            }
            else if (gopImg.Attributes.Contains("jarvis_incident") && gopImg.Attributes["jarvis_incident"] != null)
            {
                caseId = (EntityReference)gopImg.Attributes["jarvis_incident"];
            }

            if (gop.Attributes.Contains("jarvis_dealer") && gop.Attributes["jarvis_dealer"] != null)
            {
                gopDealer = (EntityReference)gop.Attributes["jarvis_dealer"];
            }
            else if (gopImg.Attributes.Contains("jarvis_dealer") && gopImg.Attributes["jarvis_dealer"] != null)
            {
                gopDealer = (EntityReference)gopImg.Attributes["jarvis_dealer"];
            }

            if (!gop.Attributes.Contains("statecode") && gopImg.Attributes.Contains("statecode"))
            {
                alreadyCancelled = ((OptionSetValue)gopImg.Attributes["statecode"]).Value.Equals(1);
            }

            if (!alreadyCancelled)
            {
                EntityCollection gopsForDealer = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.deactivateGOPsForDealer, caseId.Id, gopDealer.Id)));
                foreach (var deactivateGOP in gopsForDealer.Entities)
                {
                    if (deactivateGOP.Id != gop.Id)
                    {
                        #region UpdateRequest - GOP Deactivation
                        OptionSetValue gopType = (OptionSetValue)deactivateGOP.Attributes["jarvis_requesttype"];
                        Entity gopUpdates = new Entity(deactivateGOP.LogicalName);
                        gopUpdates.Id = deactivateGOP.Id;
                        gopUpdates["jarvis_goplimitin"] = Convert.ToDecimal(0.0);
                        gopUpdates["jarvis_goplimitout"] = Convert.ToDecimal(0.0);
                        gopUpdates["jarvis_totallimitin"] = Convert.ToDecimal(0.0);
                        gopUpdates["jarvis_totallimitout"] = Convert.ToDecimal(0.0);
                        gopUpdates["statecode"] = new OptionSetValue(1);
                        gopUpdates["statuscode"] = new OptionSetValue(2);
                        UpdateRequest updateBPFRequest = new UpdateRequest()
                        {
                            Target = gopUpdates,
                        };
                        service.Execute(updateBPFRequest);
                    }

                    #endregion

                    // SetStateRequest request = new SetStateRequest() { EntityMoniker = deactivateGOP.ToEntityReference(), State = new OptionSetValue(1), Status = new OptionSetValue(2) };
                    // service.Execute(request);
                }
            }
        }

        /// <summary>
        /// calculate Case Total Limit In Out.
        /// </summary>
        /// <param name="gop">gop entity.</param>
        /// <param name="gopImg">gop Image.</param>
        /// <param name="service">Org service.</param>
        /// <param name="serviceFee">service Fee.</param>
        /// <param name="servicefeeEntity">service fee Entity.</param>
        /// <param name="adminservice">adminservice.</param>
        /// <param name="tracingService">tracingService.</param>
        /// <returns> name="Entity GOP". </returns>
        public Entity CalculateCaseTotalLimitInOut(Entity gop, Entity gopImg, IOrganizationService service, decimal serviceFee, EntityReference servicefeeEntity, IOrganizationService adminservice, ITracingService tracingService)
        {
            decimal exchangeValue = 0;
            bool approvedGOp = false;
            EntityReference caseId = new EntityReference();
            decimal limitout = 0;
            decimal limitin = 0;
            decimal totalPassoutAmount = 0;
            List<Entity> approvedGOPList = new List<Entity>();
            List<Entity> unapprovedGOPList = new List<Entity>();
            List<Entity> requestedGOPList = new List<Entity>();
            List<Entity> allGopsList = new List<Entity>();
            List<Entity> creditCardGops = new List<Entity>();
            List<Entity> approvedCreditCardGops = new List<Entity>();
            EntityReference goplimitoutapprovedcurrency = new EntityReference();
            EntityReference limitoutcurrency = new EntityReference();
            EntityReference limitincurrency = new EntityReference();
            EntityReference totalrestoutcurrency = new EntityReference();
            EntityReference totalPassoutCurrency = new EntityReference();
            EntityReference gopDealer = new EntityReference();
            EntityReference gopRepairingDealer = new EntityReference();
            decimal totalRequestedGopIN = 0;
            EntityReference totalRequestedGopINcurrency = new EntityReference();
            decimal totalRequestedGopout = 0;
            EntityReference totalRequestedGopoutcurrency = new EntityReference();
            OptionSetValue requesttype = new OptionSetValue();
            int paymenttype = 0;
            bool alreadyCancelled = false;
            bool isDdeclined = false;
            Entity currentGOP = new Entity(gop.LogicalName, gop.Id);
            OptionSetValue gopApproval = new OptionSetValue();
            #region Initialise parent GOP That is cascading the RDs for calucltion
            if (gop.Attributes.Contains("jarvis_totallimitoutcurrency"))
            {
                currentGOP["jarvis_totallimitoutcurrency"] = (EntityReference)gop["jarvis_totallimitoutcurrency"];
            }
            else if (gopImg.Attributes.Contains("jarvis_totallimitoutcurrency"))
            {
                currentGOP["jarvis_totallimitoutcurrency"] = (EntityReference)gopImg["jarvis_totallimitoutcurrency"];
            }

            if (gop.Attributes.Contains("jarvis_totallimitout"))
            {
                currentGOP["jarvis_totallimitout"] = (decimal)gop["jarvis_totallimitout"];
            }
            else if (gopImg.Attributes.Contains("jarvis_totallimitout"))
            {
                currentGOP["jarvis_totallimitout"] = (decimal)gopImg["jarvis_totallimitout"];
            }

            if (gop.Attributes.Contains("jarvis_totallimitin"))
            {
                currentGOP["jarvis_totallimitin"] = (decimal)gop["jarvis_totallimitin"];
            }
            else if (gopImg.Attributes.Contains("jarvis_totallimitin"))
            {
                currentGOP["jarvis_totallimitin"] = (decimal)gopImg["jarvis_totallimitin"];
            }

            if (gop.Attributes.Contains("jarvis_totallimitincurrency"))
            {
                currentGOP["jarvis_totallimitincurrency"] = (EntityReference)gop["jarvis_totallimitincurrency"];
            }
            else if (gopImg.Attributes.Contains("jarvis_totallimitincurrency"))
            {
                currentGOP["jarvis_totallimitincurrency"] = (EntityReference)gopImg["jarvis_totallimitincurrency"];
            }

            if (gop.Attributes.Contains("jarvis_requesttype") && gop.Attributes["jarvis_requesttype"] != null)
            {
                currentGOP["jarvis_requesttype"] = (OptionSetValue)gop.Attributes["jarvis_requesttype"];
            }
            else if (gopImg.Attributes.Contains("jarvis_requesttype") && gopImg.Attributes["jarvis_requesttype"] != null)
            {
                currentGOP["jarvis_requesttype"] = (OptionSetValue)gopImg.Attributes["jarvis_requesttype"];
            }

            if (gop.Attributes.Contains("jarvis_goplimitin") && gop.Attributes["jarvis_goplimitin"] != null)
            {
                currentGOP["jarvis_goplimitin"] = (decimal)gop.Attributes["jarvis_goplimitin"];
            }
            else if (gopImg.Attributes.Contains("jarvis_goplimitin") && gopImg.Attributes["jarvis_goplimitin"] != null)
            {
                currentGOP["jarvis_goplimitin"] = (decimal)gopImg.Attributes["jarvis_goplimitin"];
            }

            if (gop.Attributes.Contains("jarvis_goplimitout") && gop.Attributes["jarvis_goplimitout"] != null)
            {
                currentGOP["jarvis_goplimitout"] = (decimal)gop.Attributes["jarvis_goplimitout"];
            }
            else if (gopImg.Attributes.Contains("jarvis_goplimitout") && gopImg.Attributes["jarvis_goplimitout"] != null)
            {
                currentGOP["jarvis_goplimitout"] = (decimal)gopImg.Attributes["jarvis_goplimitout"];
            }

            if (gop.Attributes.Contains("jarvis_gopincurrency") && gop.Attributes["jarvis_gopincurrency"] != null)
            {
                currentGOP["jarvis_gopincurrency"] = (EntityReference)gop.Attributes["jarvis_gopincurrency"];
            }
            else if (gopImg.Attributes.Contains("jarvis_gopincurrency") && gopImg.Attributes["jarvis_gopincurrency"] != null)
            {
                currentGOP["jarvis_gopincurrency"] = (EntityReference)gopImg.Attributes["jarvis_gopincurrency"];
            }

            if (gop.Attributes.Contains("jarvis_gopoutcurrency") && gop.Attributes["jarvis_gopoutcurrency"] != null)
            {
                currentGOP["jarvis_gopoutcurrency"] = (EntityReference)gop.Attributes["jarvis_gopoutcurrency"];
            }
            else if (gopImg.Attributes.Contains("jarvis_gopoutcurrency") && gopImg.Attributes["jarvis_gopoutcurrency"] != null)
            {
                currentGOP["jarvis_gopoutcurrency"] = (EntityReference)gopImg.Attributes["jarvis_gopoutcurrency"];
            }

            if (gop.Attributes.Contains("jarvis_gopapproval"))
            {
                currentGOP["jarvis_gopapproval"] = (OptionSetValue)gop.Attributes["jarvis_gopapproval"];
            }
            else if (gopImg.Attributes.Contains("jarvis_gopapproval"))
            {
                currentGOP["jarvis_gopapproval"] = (OptionSetValue)gopImg.Attributes["jarvis_gopapproval"];
            }

            if (gop.Attributes.Contains("jarvis_paymenttype") && gop.Attributes["jarvis_paymenttype"] != null)
            {
                currentGOP["jarvis_paymenttype"] = (OptionSetValue)gop.Attributes["jarvis_paymenttype"];
            }
            else if (gopImg.Attributes.Contains("jarvis_paymenttype") && gopImg.Attributes["jarvis_paymenttype"] != null)
            {
                currentGOP["jarvis_paymenttype"] = (OptionSetValue)gopImg.Attributes["jarvis_paymenttype"];
            }

            if (gop.Attributes.Contains("jarvis_repairingdealer") && gop.Attributes["jarvis_repairingdealer"] != null)
            {
                currentGOP["jarvis_repairingdealer"] = (EntityReference)gop.Attributes["jarvis_repairingdealer"];
            }
            else if (gopImg.Attributes.Contains("jarvis_repairingdealer") && gopImg.Attributes["jarvis_repairingdealer"] != null)
            {
                currentGOP["jarvis_repairingdealer"] = (EntityReference)gopImg.Attributes["jarvis_repairingdealer"];
            }

            if (gopImg.Attributes.Contains("jarvis_dealer") && gopImg.Attributes["jarvis_dealer"] != null)
            {
                currentGOP["jarvis_dealer"] = (EntityReference)gopImg.Attributes["jarvis_dealer"];
            }
            #endregion

            #region initialise Parameters
            if (gop.Attributes.Contains("jarvis_requesttype") && gop.Attributes["jarvis_requesttype"] != null)
            {
                requesttype = (OptionSetValue)gop.Attributes["jarvis_requesttype"];
            }
            else if (gopImg.Attributes.Contains("jarvis_requesttype") && gopImg.Attributes["jarvis_requesttype"] != null)
            {
                requesttype = (OptionSetValue)gopImg.Attributes["jarvis_requesttype"];
            }

            if (gop.Attributes.Contains("jarvis_incident") && gop.Attributes["jarvis_incident"] != null)
            {
                caseId = (EntityReference)gop.Attributes["jarvis_incident"];
                currentGOP["jarvis_incident"] = (EntityReference)gop.Attributes["jarvis_incident"];
            }
            else if (gopImg.Attributes.Contains("jarvis_incident") && gopImg.Attributes["jarvis_incident"] != null)
            {
                caseId = (EntityReference)gopImg.Attributes["jarvis_incident"];
                currentGOP["jarvis_incident"] = (EntityReference)gopImg.Attributes["jarvis_incident"];
            }

            if (gop.Attributes.Contains("jarvis_gopapproval"))
            {
                gopApproval = (OptionSetValue)gop.Attributes["jarvis_gopapproval"];
                if (gopApproval.Value.Equals(334030001))
                {
                    approvedGOp = true;
                }
                else if (gopApproval.Value.Equals(334030002))
                {
                    isDdeclined = true;
                }
            }
            else if (gopImg.Attributes.Contains("jarvis_gopapproval"))
            {
                gopApproval = (OptionSetValue)gopImg.Attributes["jarvis_gopapproval"];
                if (gopApproval.Value.Equals(334030001))
                {
                    approvedGOp = true;
                }
                else if (gopApproval.Value.Equals(334030002))
                {
                    isDdeclined = true;
                }
            }

            if (gop.Attributes.Contains("jarvis_paymenttype"))
            {
                paymenttype = ((OptionSetValue)gop.Attributes["jarvis_paymenttype"]).Value;
            }
            else if (gopImg.Attributes.Contains("jarvis_paymenttype"))
            {
                paymenttype = ((OptionSetValue)gopImg.Attributes["jarvis_paymenttype"]).Value;
            }

            if (gop.Attributes.Contains("jarvis_totallimitout"))
            {
                // caseObj["jarvis_totalgoplimitoutapproved"] = (decimal)gop.Attributes["jarvis_totallimitout"];
                limitout = (decimal)gop.Attributes["jarvis_totallimitout"];
            }
            else if (gopImg.Attributes.Contains("jarvis_totallimitout"))
            {
                // caseObj["jarvis_totalgoplimitoutapproved"] = (decimal)gopImg.Attributes["jarvis_totallimitout"];
                limitout = (decimal)gopImg.Attributes["jarvis_totallimitout"];
            }

            if (gop.Attributes.Contains("jarvis_totallimitin"))
            {
                // caseObj["jarvis_totalgoplimitinapproved"] = (decimal)gop.Attributes["jarvis_totallimitin"];
                limitin = (decimal)gop.Attributes["jarvis_totallimitin"];
            }
            else if (gopImg.Attributes.Contains("jarvis_totallimitin"))
            {
                // caseObj["jarvis_totalgoplimitinapproved"] = (decimal)gopImg.Attributes["jarvis_totallimitin"];
                limitin = (decimal)gopImg.Attributes["jarvis_totallimitin"];
            }

            if (gop.Attributes.Contains("jarvis_totallimitoutcurrency"))
            {
                // caseObj["jarvis_totalgoplimitoutapprovedcurrency"] = (EntityReference)gop.Attributes["jarvis_totallimitoutcurrency"];
                limitoutcurrency = (EntityReference)gop.Attributes["jarvis_totallimitoutcurrency"];
            }
            else if (gopImg.Attributes.Contains("jarvis_totallimitoutcurrency"))
            {
                // caseObj["jarvis_totalgoplimitoutapprovedcurrency"] = (EntityReference)gopImg.Attributes["jarvis_totallimitoutcurrency"];
                limitoutcurrency = (EntityReference)gopImg.Attributes["jarvis_totallimitoutcurrency"];
            }

            if (gop.Attributes.Contains("jarvis_totallimitincurrency"))
            {
                limitincurrency = (EntityReference)gop.Attributes["jarvis_totallimitincurrency"];
            }
            else if (gopImg.Attributes.Contains("jarvis_totallimitincurrency"))
            {
                limitincurrency = (EntityReference)gopImg.Attributes["jarvis_totallimitincurrency"];
            }

            if (gop.Attributes.Contains("jarvis_repairingdealer"))
            {
                gopRepairingDealer = (EntityReference)gop.Attributes["jarvis_repairingdealer"];
            }
            else if (gopImg.Attributes.Contains("jarvis_repairingdealer"))
            {
                gopRepairingDealer = (EntityReference)gopImg.Attributes["jarvis_repairingdealer"];
            }

            if (gopImg.Attributes.Contains("jarvis_dealer") && gopImg.Attributes["jarvis_dealer"] != null)
            {
                gopDealer = (EntityReference)gopImg.Attributes["jarvis_dealer"];
                gop["jarvis_dealer"] = (EntityReference)gopImg.Attributes["jarvis_dealer"];
            }

            if (!gop.Attributes.Contains("statecode") && gopImg.Attributes.Contains("statecode"))
            {
                alreadyCancelled = ((OptionSetValue)gopImg.Attributes["statecode"]).Value.Equals(1);
            }

            bool volvoPayAutomationEnabled = CrmHelper.GetAutomationConfig(service, JarvisConfiguration.VolvoPayAutomation, tracingService);
            #endregion

            if (!alreadyCancelled)
            {
                // UserStory-138782: GOP from multiple HDs
                // Get distint GOp By Dealer and status
                EntityCollection approvedGopOfDealer = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getGOPsForCase, caseId.Id)));
                if (approvedGopOfDealer.Entities.Count > 0)
                {
                    approvedGOPList = approvedGopOfDealer.Entities.Where(g => g.Attributes.Contains("jarvis_gopapproval") && ((OptionSetValue)g.Attributes["jarvis_gopapproval"]).Value.Equals(334030001)).GroupBy(apprGOP => apprGOP.Attributes["jarvis_dealer"]).Select(x => x.First()).ToList();
                    unapprovedGOPList = approvedGopOfDealer.Entities.OrderByDescending(g => g.Attributes["modifiedon"]).GroupBy(apprGOP => apprGOP.Attributes["jarvis_dealer"]).Select(x => x.First()).ToList();
                    allGopsList = approvedGopOfDealer.Entities.ToList();
                    creditCardGops = approvedGopOfDealer.Entities.Where(g => ((OptionSetValue)g.Attributes["jarvis_paymenttype"]).Value.Equals(334030002)).ToList();
                    approvedCreditCardGops = approvedGopOfDealer.Entities.Where(g => ((OptionSetValue)g.Attributes["jarvis_paymenttype"]).Value.Equals(334030002) && ((OptionSetValue)g.Attributes["jarvis_gopapproval"]).Value.Equals(334030001)).ToList();

                    if (gop.Attributes.Contains("statecode") && ((OptionSetValue)gop.Attributes["statecode"]).Value.Equals(1))
                    {
                        unapprovedGOPList.RemoveAll(s => s.Id.Equals(gop.Id));
                        approvedGOPList.RemoveAll(s => s.Id.Equals(gop.Id));
                    }

                    // Current GOP remove from List and related GOPs with same dealer if any
                    if (unapprovedGOPList.Count > 0)
                    {
                        unapprovedGOPList.RemoveAll(s => ((EntityReference)s.Attributes["jarvis_dealer"]).Id == gopDealer.Id);
                        unapprovedGOPList.RemoveAll(s => s.Id.Equals(gop.Id));
                        if (!isDdeclined)
                        {
                            unapprovedGOPList.Add(currentGOP);
                        }
                    }
                }

                Entity caseObj = service.Retrieve("incident", caseId.Id, new ColumnSet("jarvis_totalgoplimitoutapproved", "jarvis_totalgoplimitinapproved", "jarvis_totalgoplimitoutapprovedcurrency", "jarvis_totalcurrencyinapproved", "jarvis_totalgoplimitin", "jarvis_totalcurrencyin", "jarvis_restgoplimitout", "jarvis_totalrestcurrencyout", "jarvis_totalgoplimitout", "jarvis_totalcurrencyout", "jarvis_totalpassoutamount", "jarvis_totalpassoutcurrency", "jarvis_totalcreditcardrequestedamountcurreny", "jarvis_totalcreditcardpaymentamountcurrency", "jarvis_totalbookedamountinclvat", "jarvis_caseserviceline"));

                // Step-1 TOTAL requested GOP IN =SUM of the TOTAL GOP IN for each GOP Dealer (first unapproved, if not found then approved).
                #region TOTAL requested GOP IN

                /* if (!approvedGOp || unapprovedGOPList.Count < 1)
                 {
                     TotalRequestedGopINcurrency = limitincurrency;
                     TotalRequestedGopIN = limitin;

                     unapprovedGOPList.RemoveAll(s => (((EntityReference)s.Attributes["jarvis_dealer"]).Id == gopDealer.Id));
                 }*/
                if (totalRequestedGopINcurrency == null || totalRequestedGopINcurrency.Id.Equals(Guid.Empty)) // In case of approved , currency will be defaulted to latest unapproved gop limitInCurrency if no currency available in case Level
                {
                    if (caseObj.Attributes.Contains("jarvis_totalcurrencyin") && caseObj["jarvis_totalcurrencyin"] != null)
                    {
                        totalRequestedGopINcurrency = (EntityReference)caseObj["jarvis_totalcurrencyin"];
                    }
                    else if (unapprovedGOPList.Count > 0 && unapprovedGOPList.FirstOrDefault().Attributes.Contains("jarvis_totallimitincurrency") && unapprovedGOPList.FirstOrDefault()["jarvis_totallimitincurrency"] != null)
                    {
                        totalRequestedGopINcurrency = (EntityReference)unapprovedGOPList.FirstOrDefault().Attributes["jarvis_totallimitincurrency"];
                    }
                    else
                    {
                        totalRequestedGopINcurrency = limitincurrency;
                    }
                }

                // Cuurent Record limit In
                exchangeValue = this.CurrencyExchange(limitincurrency.Id, totalRequestedGopINcurrency.Id, service);

                // For first record
                if (unapprovedGOPList.Count == 0 || unapprovedGOPList == null)
                {
                    totalRequestedGopIN = limitin * exchangeValue;
                }

                foreach (var gopEntity in unapprovedGOPList)
                {
                    decimal totalRequestedDealerGopIN = 0;

                    // #609013- Multiple Rd for same HD calculation
                    // Get latest GOp RDs per dealer and RD
                    requestedGOPList = approvedGopOfDealer.Entities.Where(g => g.Attributes.Contains("jarvis_repairingdealer") && ((EntityReference)g.Attributes["jarvis_dealer"]).Id.Equals(((EntityReference)gopEntity.Attributes["jarvis_dealer"]).Id) && ((OptionSetValue)g.Attributes["jarvis_requesttype"]).Value.Equals(334030002)).OrderByDescending(g => g.Attributes["modifiedon"]).GroupBy(apprGOP => new { v = apprGOP.Attributes["jarvis_dealer"], w = apprGOP.Attributes["jarvis_repairingdealer"] }).Select(x => x.First()).ToList();

                    // take first Unapproved GOps then Approved
                    // Update the List with Current RD trigerring the Logic
                    if (requesttype.Value.Equals(334030002) && !isDdeclined && requestedGOPList.Count > 0)
                    {
                        requestedGOPList.RemoveAll(s => (((EntityReference)s.Attributes["jarvis_dealer"]).Id == gopDealer.Id) && (((EntityReference)s.Attributes["jarvis_repairingdealer"]).Id == gopRepairingDealer.Id));

                        if (gopDealer.Id.Equals(((EntityReference)gopEntity.Attributes["jarvis_dealer"]).Id))
                        {
                            requestedGOPList.Add(currentGOP);
                        }
                    }
                    #region #609013 Multi RDs case
                    /*
                     WHEN it is a Multi RDs case AND there are multiple GOP RD requests for the same GOP Dealer
                    So, the exception kicks in: When we have more than one Pass Out AND when every Pass Out has a GOP RD,
                    IF(Latest GOP IN HD amount  per GOP - Dealer > Sum of all latest GOP OUT RDs by Repairing Dealer + Staircase fee * GOP - Dealer)
                        THEN show “Latest GOP IN HD amount  per GOP - Dealer".
                    IF(Latest GOP IN HD amount  per GOP - Dealer < Sum of all latest GOP OUT RDs by  Repairing Dealer + Staircase fee * GOP Dealer)
                        THEN show “Sum of all latest GOP OUT RDs by Repairing Dealer + Staircasfee * GOP - Dealer".
                    */
                    if (requestedGOPList != null && requestedGOPList.Count > 1)
                    {
                        // Get the latest GOP Hd for the dealer
                        EntityCollection approvedGopHDOfDealer = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getGOPsForDealer, caseId.Id, ((EntityReference)gopEntity.Attributes["jarvis_dealer"]).Id)));
                        decimal approvedGopHDTotalOutput = 0;
                        Entity approvedGOPHD = approvedGopHDOfDealer.Entities.Where(g => ((OptionSetValue)g.Attributes["jarvis_requesttype"]).Value.Equals(334030001)).FirstOrDefault();
                        if (approvedGOPHD != null && requesttype.Value.Equals(334030001) && gopDealer.Id.Equals(((EntityReference)approvedGOPHD.Attributes["jarvis_dealer"]).Id))
                        {
                            approvedGOPHD = currentGOP;
                        }

                        if (approvedGOPHD != null)
                        {
                            approvedGopHDTotalOutput = (decimal)approvedGOPHD["jarvis_goplimitin"];
                            EntityReference approvedGopCurrency = (EntityReference)approvedGOPHD["jarvis_gopincurrency"];
                            exchangeValue = this.CurrencyExchange(approvedGopCurrency.Id, totalRequestedGopINcurrency.Id, service);
                            approvedGopHDTotalOutput = approvedGopHDTotalOutput * exchangeValue;
                        }

                        // Calculate the Sum of Gop Ins of the Latest RDs for multiple passout
                        // TotalRequestedGopIN = 0;
                        foreach (var requested_gopEntity in requestedGOPList)
                        {
                            if (requested_gopEntity.Attributes.Contains("jarvis_goplimitout"))
                            {
                                var gopEntityTotalOutput = (decimal)requested_gopEntity["jarvis_goplimitout"];
                                var gopEntityCurrency = (EntityReference)requested_gopEntity["jarvis_gopoutcurrency"];
                                exchangeValue = this.CurrencyExchange(gopEntityCurrency.Id, totalRequestedGopINcurrency.Id, service);
                                totalRequestedDealerGopIN = totalRequestedDealerGopIN + (gopEntityTotalOutput * exchangeValue);
                            }
                        }

                        // Add Service fee per dealer
                        if (servicefeeEntity != null)
                        {
                            exchangeValue = this.CurrencyExchange(servicefeeEntity.Id, totalRequestedGopINcurrency.Id, service);
                        }
                        else
                        {
                            exchangeValue = 1;
                        }

                        totalRequestedDealerGopIN = totalRequestedDealerGopIN + (serviceFee * exchangeValue);

                        // Compare with Latest HD Amount
                        if (decimal.Round(approvedGopHDTotalOutput) > decimal.Round(totalRequestedDealerGopIN))
                        {
                            totalRequestedGopIN = approvedGopHDTotalOutput;
                            totalRequestedDealerGopIN = 0;
                        }
                        else
                        {
                            totalRequestedGopIN = totalRequestedGopIN + totalRequestedDealerGopIN;
                            totalRequestedDealerGopIN = 0;
                        }
                    }
                    #endregion

                    // Single RD for Dealer
                    else if (requesttype.Value.Equals(334030002) && gopDealer.Id.Equals(((EntityReference)gopEntity.Attributes["jarvis_dealer"]).Id))
                    {
                        var gopEntityTotalOutput = limitin;
                        exchangeValue = this.CurrencyExchange(limitincurrency.Id, totalRequestedGopINcurrency.Id, service);
                        totalRequestedGopIN = totalRequestedGopIN + (gopEntityTotalOutput * exchangeValue);
                    }

                    // Single HD for Dealer
                    else
                    {
                        if (gopEntity.Attributes.Contains("jarvis_totallimitin"))
                        {
                            var gopEntityTotalOutput = (decimal)gopEntity["jarvis_totallimitin"];
                            var gopEntityCurrency = (EntityReference)gopEntity["jarvis_totallimitincurrency"];
                            exchangeValue = this.CurrencyExchange(gopEntityCurrency.Id, totalRequestedGopINcurrency.Id, service);
                            totalRequestedGopIN = totalRequestedGopIN + (gopEntityTotalOutput * exchangeValue);
                        }
                    }
                }

                if (caseObj.Attributes.Contains("jarvis_totalcurrencyin") && caseObj["jarvis_totalcurrencyin"] != null)
                {
                    var currencyin = (EntityReference)caseObj["jarvis_totalcurrencyin"];
                    var outexchangeCurrency = this.CurrencyExchange(totalRequestedGopINcurrency.Id, currencyin.Id, service);
                    caseObj["jarvis_totalgoplimitin"] = (decimal)Math.Round(totalRequestedGopIN * outexchangeCurrency, 2, System.MidpointRounding.AwayFromZero);

                    // totalrestoutcurrency = currencyout;
                }
                else
                {
                    caseObj["jarvis_totalgoplimitin"] = (decimal)Math.Round(totalRequestedGopIN, 2, System.MidpointRounding.AwayFromZero);
                    caseObj["jarvis_totalcurrencyin"] = totalRequestedGopINcurrency;
                }

                #endregion

                // Step-2 TOTAL requested GOP OUT =SUM of the TOTAL GOP OUT for each GOP Dealer (first unapproved, if not found then approved).
                #region TOTAL requested GOP OUT

                if (totalRequestedGopoutcurrency == null || totalRequestedGopoutcurrency.Id.Equals(Guid.Empty))/// out case of approved , currency will be defaulted to latest unapproved gop limitoutCurrency
                {
                    Entity unapprovedGOP = unapprovedGOPList.FirstOrDefault();
                    if (caseObj.Attributes.Contains("jarvis_totalcurrencyout") && caseObj["jarvis_totalcurrencyout"] != null)
                    {
                        totalRequestedGopoutcurrency = (EntityReference)caseObj["jarvis_totalcurrencyout"];
                    }
                    else if (unapprovedGOPList.Count > 0 && unapprovedGOP.Attributes.Contains("jarvis_totallimitoutcurrency") && unapprovedGOP["jarvis_totallimitoutcurrency"] != null)
                    {
                        totalRequestedGopoutcurrency = (EntityReference)unapprovedGOP.Attributes["jarvis_totallimitoutcurrency"];
                    }
                    else
                    {
                        totalRequestedGopoutcurrency = limitoutcurrency;
                    }
                }

                // Cuurent Record limit In
                exchangeValue = this.CurrencyExchange(limitoutcurrency.Id, totalRequestedGopoutcurrency.Id, service);
                if (caseObj["jarvis_totalgoplimitin"] != null)
                {
                    totalRequestedGopout = (decimal)caseObj["jarvis_totalgoplimitin"];
                }
                else
                {
                    totalRequestedGopout = limitout * exchangeValue;
                }

                if (caseObj.Attributes.Contains("jarvis_totalcurrencyout") && caseObj["jarvis_totalcurrencyout"] != null)
                {
                    var currencyout = (EntityReference)caseObj["jarvis_totalcurrencyout"];
                    var currencyin = (EntityReference)caseObj["jarvis_totalcurrencyin"];
                    var outexchangeCurrency2 = this.CurrencyExchange(currencyin.Id, currencyout.Id, service);
                    decimal serviceFeeExchangeValue = 1;
                    if (servicefeeEntity != null)
                    {
                        serviceFeeExchangeValue = this.CurrencyExchange(servicefeeEntity.Id, currencyout.Id, service);
                    }

                    caseObj["jarvis_totalgoplimitout"] = (decimal)Math.Round((totalRequestedGopout - (serviceFee * serviceFeeExchangeValue)) * outexchangeCurrency2, 2, System.MidpointRounding.AwayFromZero);

                    // totalrestoutcurrency = currencyout;
                }
                else
                {
                    var currencyin = (EntityReference)caseObj["jarvis_totalcurrencyin"];
                    var outexchangeCurrency2 = this.CurrencyExchange(currencyin.Id, totalRequestedGopoutcurrency.Id, service);
                    decimal serviceFeeExchangeValue = 1;
                    if (servicefeeEntity != null)
                    {
                        serviceFeeExchangeValue = this.CurrencyExchange(servicefeeEntity.Id, totalRequestedGopoutcurrency.Id, service);
                    }

                    caseObj["jarvis_totalgoplimitout"] = (decimal)Math.Round((totalRequestedGopout - (serviceFee * serviceFeeExchangeValue)) * outexchangeCurrency2, 2, System.MidpointRounding.AwayFromZero);

                    caseObj["jarvis_totalcurrencyout"] = totalRequestedGopoutcurrency;
                }
                #endregion

                // Step-3 TOTAL  Approved GOP OUT =SUM of the TOTAL GOP OUT for each GOP Dealer (only approved GOP)
                #region TOTAL  Approved GOP OUT
                decimal totalApprovedGopout = 0;
                EntityReference totalApprovedGopoutcurrency = new EntityReference();
                if (approvedGOp)
                {
                    totalApprovedGopoutcurrency = limitoutcurrency;
                    totalApprovedGopout = limitout;
                    approvedGOPList.RemoveAll(s => ((EntityReference)s.Attributes["jarvis_dealer"]).Id == gopDealer.Id);

                    foreach (var gopEntity in approvedGOPList)
                    {
                        if (gopEntity.Attributes.Contains("jarvis_totallimitout"))
                        {
                            var gopEntityTotalOutput = (decimal)gopEntity["jarvis_totallimitout"];
                            var gopEntityCurrency = (EntityReference)gopEntity["jarvis_totallimitoutcurrency"];
                            exchangeValue = this.CurrencyExchange(gopEntityCurrency.Id, totalApprovedGopoutcurrency.Id, service);
                            totalApprovedGopout = totalApprovedGopout + (gopEntityTotalOutput * exchangeValue);
                        }
                    }

                    if (caseObj.Attributes.Contains("jarvis_totalgoplimitoutapprovedcurrency") && caseObj["jarvis_totalgoplimitoutapprovedcurrency"] != null)
                    {
                        var totalcurrencyout = (EntityReference)caseObj["jarvis_totalgoplimitoutapprovedcurrency"];
                        var outexchangeCurrency3 = this.CurrencyExchange(totalApprovedGopoutcurrency.Id, totalcurrencyout.Id, service);
                        caseObj["jarvis_totalgoplimitoutapproved"] = (decimal)Math.Round(totalApprovedGopout * outexchangeCurrency3, 2, System.MidpointRounding.AwayFromZero);

                        // totalrestoutcurrency = currencyout;
                    }
                    else
                    {
                        caseObj["jarvis_totalgoplimitoutapproved"] = (decimal)Math.Round(totalApprovedGopout, 2, System.MidpointRounding.AwayFromZero);
                        if (!totalApprovedGopoutcurrency.Id.Equals(Guid.Empty))
                        {
                            caseObj["jarvis_totalgoplimitoutapprovedcurrency"] = totalApprovedGopoutcurrency;
                        }
                    }
                }

                #endregion

                // Step-4 TOTAL  Approved GOP IN =SUM of the TOTAL GOP IN for each GOP Dealer (only approved GOP)
                #region TOTAL  Approved GOP IN
                decimal totalApprovedGopin = 0;
                EntityReference totalApprovedGopincurrency = new EntityReference();
                if (approvedGOp)
                {
                    totalApprovedGopincurrency = limitincurrency;
                    totalApprovedGopin = limitin;
                    approvedGOPList.RemoveAll(s => ((EntityReference)s.Attributes["jarvis_dealer"]).Id == gopDealer.Id);

                    foreach (var gopEntity in approvedGOPList)
                    {
                        if (gopEntity.Attributes.Contains("jarvis_totallimitin"))
                        {
                            var gopEntityTotalinput = (decimal)gopEntity["jarvis_totallimitin"];
                            var gopEntityCurrency = (EntityReference)gopEntity["jarvis_totallimitincurrency"];
                            exchangeValue = this.CurrencyExchange(gopEntityCurrency.Id, totalApprovedGopincurrency.Id, service);
                            totalApprovedGopin = totalApprovedGopin + (gopEntityTotalinput * exchangeValue);
                        }
                    }

                    if (caseObj.Attributes.Contains("jarvis_totalcurrencyinapproved") && caseObj["jarvis_totalcurrencyinapproved"] != null)
                    {
                        var totalcurrencyin = (EntityReference)caseObj["jarvis_totalcurrencyinapproved"];
                        var outexchangeCurrency4 = this.CurrencyExchange(totalApprovedGopincurrency.Id, totalcurrencyin.Id, service);
                        caseObj["jarvis_totalgoplimitinapproved"] = (decimal)Math.Round(totalApprovedGopin * outexchangeCurrency4, 2, System.MidpointRounding.AwayFromZero);

                        // totalrestoutcurrency = currencyout;
                    }
                    else
                    {
                        caseObj["jarvis_totalgoplimitinapproved"] = (decimal)Math.Round(totalApprovedGopin, 2, System.MidpointRounding.AwayFromZero);
                        if (!totalApprovedGopincurrency.Id.Equals(Guid.Empty))
                        {
                            caseObj["jarvis_totalcurrencyinapproved"] = totalApprovedGopincurrency;
                        }
                    }
                }

                #endregion

                // Step-5 Total Credit Card Booking Amount
                if (paymenttype.Equals(334030002))
                {
                    creditCardGops.RemoveAll(s => s.Id.Equals(gop.Id));

                    #region Total Credit Card Booking Amount
                    if (approvedGOp)
                    {
                        EntityReference totalCreditCardBookingAmountcurrency = new EntityReference();
                        if (gop.Attributes.Contains("jarvis_creditcardincurrency") && gop["jarvis_creditcardincurrency"] != null && approvedGOp)
                        {
                            totalCreditCardBookingAmountcurrency = (EntityReference)gop.Attributes["jarvis_creditcardincurrency"];
                        }
                        else if (gopImg.Attributes.Contains("jarvis_creditcardincurrency") && gopImg["jarvis_creditcardincurrency"] != null && approvedGOp)
                        {
                            totalCreditCardBookingAmountcurrency = (EntityReference)gopImg.Attributes["jarvis_creditcardincurrency"];
                        }
                        else if (caseObj.Attributes.Contains("jarvis_totalcreditcardpaymentamountcurrency") && caseObj["jarvis_totalcreditcardpaymentamountcurrency"] != null)
                        {
                            totalCreditCardBookingAmountcurrency = (EntityReference)caseObj["jarvis_totalcreditcardpaymentamountcurrency"];
                        }
                        else
                        {
                            List<Entity> filteredList = allGopsList.Where(g => (g.Attributes.Contains("jarvis_creditcardincurrency") && g["jarvis_creditcardincurrency"] != null)).ToList();
                            if (filteredList != null && filteredList.Count > 0)
                            {
                                totalCreditCardBookingAmountcurrency = (EntityReference)filteredList.FirstOrDefault().Attributes["jarvis_creditcardincurrency"];
                            }
                        }

                        decimal totalCreditCardBookingAmount = 0;

                        if (gop.Attributes.Contains("jarvis_creditcardgopinbooking") && gop.Attributes.Contains("jarvis_creditcardincurrency") && gop.Attributes["jarvis_creditcardgopinbooking"] != null && approvedGOp)
                        {
                            exchangeValue = this.CurrencyExchange(((EntityReference)gop.Attributes["jarvis_creditcardincurrency"]).Id, totalCreditCardBookingAmountcurrency.Id, service);
                            totalCreditCardBookingAmount = (decimal)gop["jarvis_creditcardgopinbooking"] * exchangeValue;
                        }
                        else if (gopImg.Attributes.Contains("jarvis_creditcardgopinbooking") && gopImg.Attributes.Contains("jarvis_creditcardincurrency") && gopImg.Attributes["jarvis_creditcardgopinbooking"] != null && approvedGOp)
                        {
                            exchangeValue = this.CurrencyExchange(((EntityReference)gopImg.Attributes["jarvis_creditcardincurrency"]).Id, totalCreditCardBookingAmountcurrency.Id, service);
                            totalCreditCardBookingAmount = (decimal)gopImg["jarvis_creditcardgopinbooking"] * exchangeValue;
                        }

                        foreach (var gopEntity in approvedCreditCardGops)
                        {
                            if (gopEntity.Attributes.Contains("jarvis_creditcardgopinbooking"))
                            {
                                var gopEntityTotalinput = (decimal)gopEntity["jarvis_creditcardgopinbooking"];
                                var gopEntityCurrency = (EntityReference)gopEntity["jarvis_creditcardincurrency"];
                                exchangeValue = this.CurrencyExchange(gopEntityCurrency.Id, totalCreditCardBookingAmountcurrency.Id, service);
                                totalCreditCardBookingAmount = totalCreditCardBookingAmount + (gopEntityTotalinput * exchangeValue);
                            }
                        }

                        caseObj["jarvis_totalcreditcardpaymentamountcurrency"] = totalCreditCardBookingAmountcurrency;
                        caseObj["jarvis_totalbookedamountinclvat"] = (decimal)Math.Round(totalCreditCardBookingAmount, 2, System.MidpointRounding.AwayFromZero);
                    }
                    #endregion

                    #region Total Credit Card Requested Amount
                    //if (volvoPayAutomationEnabled)
                    //{
                    EntityReference totalCreditCardRequestedAmountcurrency = new EntityReference();
                    if (caseObj.Attributes.Contains("jarvis_totalcreditcardrequestedamountcurreny") && caseObj["jarvis_totalcreditcardrequestedamountcurreny"] != null)
                    {
                        totalCreditCardRequestedAmountcurrency = (EntityReference)caseObj["jarvis_totalcreditcardrequestedamountcurreny"];
                    }
                    else if (gop.Attributes.Contains("jarvis_creditcardincurrency") && gop["jarvis_creditcardincurrency"] != null)
                    {
                        totalCreditCardRequestedAmountcurrency = (EntityReference)gop.Attributes["jarvis_creditcardincurrency"];
                    }
                    else
                    {
                        totalCreditCardRequestedAmountcurrency = (EntityReference)allGopsList.Where(g => (g.Attributes.Contains("jarvis_gopoutcurrency") && g["jarvis_gopoutcurrency"] != null)).FirstOrDefault().Attributes["jarvis_gopoutcurrency"];
                    }

                    decimal totalCreditCardRequestedAmount = 0;
                    if (gop.Attributes.Contains("jarvis_creditcardgopinbooking") && gop.Attributes["jarvis_creditcardgopinbooking"] != null && gop.Attributes.Contains("jarvis_creditcardincurrency") && gop.Attributes["jarvis_creditcardincurrency"] != null && !isDdeclined)
                    {
                        exchangeValue = this.CurrencyExchange(((EntityReference)gop.Attributes["jarvis_creditcardincurrency"]).Id, totalCreditCardRequestedAmountcurrency.Id, service);
                        totalCreditCardRequestedAmount = (decimal)gop["jarvis_creditcardgopinbooking"] * exchangeValue;
                    }
                    if (gopImg.Attributes.Contains("jarvis_creditcardgopinbooking") && gopImg.Attributes["jarvis_creditcardgopinbooking"] != null && gopImg.Attributes.Contains("jarvis_creditcardincurrency") && gopImg.Attributes["jarvis_creditcardincurrency"] != null && !isDdeclined)
                    {
                        exchangeValue = this.CurrencyExchange(((EntityReference)gopImg.Attributes["jarvis_creditcardincurrency"]).Id, totalCreditCardRequestedAmountcurrency.Id, service);
                        totalCreditCardRequestedAmount = (decimal)gopImg["jarvis_creditcardgopinbooking"] * exchangeValue;
                    }

                    foreach (var gopEntity in creditCardGops.Where(g => ((OptionSetValue)g.Attributes["jarvis_gopapproval"]).Value != 334030002).ToList())
                    {
                        if (gopEntity.Attributes.Contains("jarvis_creditcardgopinbooking"))
                        {
                            var gopEntityTotalinput = (decimal)gopEntity["jarvis_creditcardgopinbooking"];
                            var gopEntityCurrency = (EntityReference)gopEntity["jarvis_creditcardincurrency"];
                            exchangeValue = this.CurrencyExchange(gopEntityCurrency.Id, totalCreditCardRequestedAmountcurrency.Id, service);
                            totalCreditCardRequestedAmount = totalCreditCardRequestedAmount + (gopEntityTotalinput * exchangeValue);
                        }
                    }

                    caseObj["jarvis_totalcreditcardrequestedamountcurreny"] = totalCreditCardRequestedAmountcurrency;
                    caseObj["jarvis_totalrequestedccamount"] = Math.Round(totalCreditCardRequestedAmount, 2, System.MidpointRounding.AwayFromZero);
                    //}
                    #endregion
                }

                if (approvedGOp)
                {
                    // Step-5 Calculate total passout Amount jarvis_totalpassoutamount jarvis_totalpassoutcurrency
                    if (caseObj.Attributes.Contains("jarvis_totalrestcurrencyout") && caseObj["jarvis_totalrestcurrencyout"] != null)
                    {
                        totalApprovedGopoutcurrency = (EntityReference)caseObj["jarvis_totalrestcurrencyout"];
                    }
                    else
                    {
                        if (!totalApprovedGopoutcurrency.Id.Equals(Guid.Empty))
                        {
                            caseObj["jarvis_totalrestcurrencyout"] = totalApprovedGopoutcurrency;
                        }
                    }

                    if (caseObj.Attributes.Contains("jarvis_totalpassoutamount") && caseObj.Attributes["jarvis_totalpassoutamount"] != null && caseObj.Attributes.Contains("jarvis_totalpassoutcurrency") && caseObj.Attributes["jarvis_totalpassoutcurrency"] != null)
                    {
                        var passoutTotalOutput = (decimal)caseObj.Attributes["jarvis_totalpassoutamount"];
                        totalPassoutCurrency = (EntityReference)caseObj.Attributes["jarvis_totalpassoutcurrency"];
                        exchangeValue = this.CurrencyExchange(totalPassoutCurrency.Id, totalApprovedGopoutcurrency.Id, service);
                        totalPassoutAmount = totalPassoutAmount + (passoutTotalOutput * exchangeValue);
                    }
                    else
                    {
                        EntityCollection casePassouts = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.CaseActivePassouts, caseId.Id)));
                        if (casePassouts.Entities.Count() > 0)
                        {
                            totalPassoutCurrency = (EntityReference)casePassouts.Entities.FirstOrDefault().Attributes["transactioncurrencyid"];
                            foreach (var passout in casePassouts.Entities)
                            {
                                if (passout.Attributes.Contains("jarvis_goplimitout"))
                                {
                                    var passoutTotalOutput = ((Money)passout["jarvis_goplimitout"]).Value; ;
                                    var passoutCurrency2 = (EntityReference)passout["transactioncurrencyid"];

                                    // Convert Passout Currency to GOT APProved Amount Currency
                                    exchangeValue = this.CurrencyExchange(passoutCurrency2.Id, totalApprovedGopoutcurrency.Id, service);
                                    totalPassoutAmount = totalPassoutAmount + (passoutTotalOutput * exchangeValue);
                                }
                            }
                        }
                    }

                    // Step-6 Calculate TOtal AVAILABLE AMOUNT OUT = Total Approved Amount - Total Passout Amount
                    totalApprovedGopout = (decimal)caseObj["jarvis_totalgoplimitoutapproved"];
                    var limitoutApprovedCurrency = (EntityReference)caseObj["jarvis_totalgoplimitoutapprovedcurrency"];
                    if (totalPassoutCurrency != null && !totalPassoutCurrency.Id.Equals(Guid.Empty))
                    {
                        var passoutCurrency3 = totalPassoutCurrency;
                    }
                    else
                    {
                        totalPassoutCurrency = limitoutApprovedCurrency;
                    }

                    var outexchangeCurrency4 = this.CurrencyExchange(limitoutApprovedCurrency.Id, totalApprovedGopoutcurrency.Id, service);
                    var amountrest = (totalApprovedGopout * outexchangeCurrency4) - totalPassoutAmount;
                    caseObj["jarvis_restgoplimitout"] = (decimal)Math.Round(amountrest, 2, System.MidpointRounding.AwayFromZero);
                }

                service.Update(caseObj);

                if (gop.Attributes.Contains("jarvis_gopapproval") && ((OptionSetValue)gop.Attributes["jarvis_gopapproval"]).Value.Equals(334030001))
                {
                    // 609054 609054 Force Copied
                    this.CopyUnapprovedRDs(gop, gopImg, caseObj, serviceFee, servicefeeEntity, service, adminservice, tracingService);
                }

                // throw new InvalidPluginExecutionException("cALCULTED Total requested Limit In" + TotalRequestedGopIN.ToString());
            }

            return gop;
        }

        /// <summary>
        /// UserStory-91008  GOP + request RD to HD.
        /// </summary>
        /// <param name="gop">gop entity.</param>
        /// <param name="gopImg">gop Image.</param>
        /// <param name="service">Org service.</param>
        /*public void ShadowGOP(Entity gop, Entity gopImg, IOrganizationService service)
        {
            bool meetsShadowConditions = false;
            Entity shadowGOP = new Entity();
            bool createShadowEntity = false;
            EntityReference repairingDealer = new EntityReference();
            EntityReference caseId = new EntityReference();

            if (gop.Attributes.Contains("jarvis_incident") && gop.Attributes["jarvis_incident"] != null)
            {
                caseId = (EntityReference)gop.Attributes["jarvis_incident"];
            }
            else if (gopImg.Attributes.Contains("jarvis_incident") && gopImg.Attributes["jarvis_incident"] != null)
            {
                caseId = (EntityReference)gopImg.Attributes["jarvis_incident"];
            }

            if (gop.Attributes.Contains("jarvis_repairingdealer") && gop.Attributes["jarvis_repairingdealer"] != null)
            {
                repairingDealer = (EntityReference)gop.Attributes["jarvis_repairingdealer"];
            }
            else if (gopImg.Attributes.Contains("jarvis_repairingdealer") && gopImg.Attributes["jarvis_repairingdealer"] != null)
            {
                repairingDealer = (EntityReference)gopImg.Attributes["jarvis_repairingdealer"];
            }

            // Check if the RD has already created Corresponding HD
            EntityCollection realtedShadowGOps = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.geRelatedShadowGOP, gop.Id, caseId.Id)));
            if (realtedShadowGOps.Entities.Count() > 0)
            {
                shadowGOP = realtedShadowGOps.Entities[0];
            }
            else
            {
                shadowGOP = new Entity(gop.LogicalName, new Guid());
                createShadowEntity = true;
            }

            if (gop.Attributes.Contains("jarvis_gopapproval"))
            {
                shadowGOP["jarvis_gopapproval"] = (OptionSetValue)gop.Attributes["jarvis_gopapproval"];
            }

            if (gop.Attributes.Contains("jarvis_totallimitout"))
            {
                shadowGOP["jarvis_totallimitout"] = gop.Attributes["jarvis_totallimitout"];
                meetsShadowConditions = meetsShadowConditions && true;
            }

            if (gop.Attributes.Contains("jarvis_totallimitin"))
            {
                shadowGOP["jarvis_totallimitin"] = gop.Attributes["jarvis_totallimitin"];
                meetsShadowConditions = meetsShadowConditions && true;
            }

            if (gop.Attributes.Contains("jarvis_totallimitoutcurrency"))
            {
                shadowGOP["jarvis_totallimitoutcurrency"] = gop.Attributes["jarvis_totallimitoutcurrency"];
                meetsShadowConditions = meetsShadowConditions && true;
            }

            if (gop.Attributes.Contains("jarvis_totallimitincurrency"))
            {
                shadowGOP["jarvis_totallimitincurrency"] = gop.Attributes["jarvis_totallimitincurrency"];
                meetsShadowConditions = meetsShadowConditions && true;
            }

            if (gop.Attributes.Contains("jarvis_gopreason"))
            {
                shadowGOP["jarvis_gopreason"] = gop.Attributes["jarvis_gopreason"] + "Please consider previous GOP+ to better understand the total amount to approve";
                meetsShadowConditions = true;
            }

            if (gopImg.Attributes.Contains("jarvis_dealer") && gopImg.Attributes["jarvis_dealer"] != null)
            {
                shadowGOP["jarvis_dealer"] = gopImg.Attributes["jarvis_dealer"];
            }

            shadowGOP["jarvis_comment"] = "<Repairing Dealer Name>";
            if (gopImg.Attributes.Contains("jarvis_incident"))
            {
                shadowGOP["jarvis_incident"] = gopImg.Attributes["jarvis_incident"];
            }

            if (gopImg.Attributes.Contains("jarvis_dealer"))
            {
                shadowGOP["jarvis_dealer"] = gopImg.Attributes["jarvis_dealer"];
            }

            if (meetsShadowConditions)
            {
                if (createShadowEntity)
                {
                    shadowGOP["jarvis_relatedgop"] = new EntityReference(gop.LogicalName, gop.Id);
                    service.Create(shadowGOP);
                }
                else
                {
                    service.Update(shadowGOP);
                }
            }
        }*/

        /// <summary>
        /// Update Pass out From RD .
        /// </summary>
        /// <param name="gop">gop entity.</param>
        /// <param name="gopImg">gop Image.</param>
        /// <param name="service">Org service.</param>
        /// <param name="serviceFee">service Fee.</param>
        /// <param name="servicefeeEntity">service fee Entity.</param>
        /// <param name="adminservice">admin service.</param>
        public void UpdatePassoutFromRD(Entity gop, Entity gopImg, IOrganizationService service, decimal serviceFee, EntityReference servicefeeEntity, IOrganizationService adminservice)
        {
            try
            {
                decimal limitout = 0;
                EntityReference repairingDealer = new EntityReference();
                OptionSetValue passoutpaymentType = new OptionSetValue();
                EntityReference limitoutcurrency = new EntityReference();
                EntityReference passoutcurrency = new EntityReference();
                if (gop.Attributes.Contains("jarvis_repairingdealer") && gop.Attributes["jarvis_repairingdealer"] != null)
                {
                    repairingDealer = (EntityReference)gop.Attributes["jarvis_repairingdealer"];
                }
                else if (gopImg.Attributes.Contains("jarvis_repairingdealer") && gopImg.Attributes["jarvis_repairingdealer"] != null)
                {
                    repairingDealer = (EntityReference)gopImg.Attributes["jarvis_repairingdealer"];
                }

                if (gop.Attributes.Contains("jarvis_goplimitout") && gop.Attributes["jarvis_goplimitout"] != null)
                {
                    limitout = (decimal)gop.Attributes["jarvis_goplimitout"];
                }
                else if (gopImg.Attributes.Contains("jarvis_goplimitout") && gopImg.Attributes["jarvis_goplimitout"] != null)
                {
                    limitout = (decimal)gopImg.Attributes["jarvis_goplimitout"];
                }

                if (gop.Contains("jarvis_paymenttype") && gop.Attributes["jarvis_paymenttype"] != null)
                {
                    passoutpaymentType = (OptionSetValue)gop.Attributes["jarvis_paymenttype"];
                }
                else if (gopImg.Attributes.Contains("jarvis_paymenttype") && gopImg.Attributes["jarvis_paymenttype"] != null)
                {
                    passoutpaymentType = (OptionSetValue)gopImg.Attributes["jarvis_paymenttype"];
                }

                if (gop.Attributes.Contains("jarvis_gopoutcurrency"))
                {
                    // caseObj["jarvis_totalgoplimitoutapprovedcurrency"] = (EntityReference)gop.Attributes["jarvis_totallimitoutcurrency"];
                    limitoutcurrency = (EntityReference)gop.Attributes["jarvis_gopoutcurrency"];
                }
                else if (gopImg.Attributes.Contains("jarvis_gopoutcurrency"))
                {
                    limitoutcurrency = (EntityReference)gopImg.Attributes["jarvis_gopoutcurrency"];
                }

                Entity passout = service.Retrieve(repairingDealer.LogicalName, repairingDealer.Id, new ColumnSet("transactioncurrencyid"));

                if (passout != null && passout.Attributes.Contains("transactioncurrencyid"))
                {
                    passoutcurrency = (EntityReference)passout.Attributes["transactioncurrencyid"];
                    var outexchangeCurrency = this.CurrencyExchange(limitoutcurrency.Id, passoutcurrency.Id, service);
                    limitout = limitout * outexchangeCurrency;
                }

                Entity realedPassout = new Entity(repairingDealer.LogicalName);
                realedPassout.Id = repairingDealer.Id;
                realedPassout["jarvis_goplimitout"] = (decimal)Math.Round(limitout, 2, System.MidpointRounding.AwayFromZero);
                realedPassout["statuscode"] = new OptionSetValue(334030001);
                realedPassout["jarvis_paymenttype"] = passoutpaymentType;

                adminservice.Update(realedPassout);
            }
            catch (InvalidPluginExecutionException ex)
            {
                throw new InvalidPluginExecutionException("The Approved GOP RD might increase the passout amount beyond Case available amount !");
            }
        }

        /// <summary>
        /// check ShadowGOP Conditions.
        /// </summary>
        /// <param name="gop">gop entity.</param>
        /// <param name="gopImg">gop Image.</param>
        /// <param name="service">Org service.</param>
        /// <param name="trace">trace service.</param>
        /*public void CheckShadowGOPConditions(Entity gop, Entity gopImg, IOrganizationService service, ITracingService trace)
        {
            OptionSetValue requesttype = new OptionSetValue();
            bool approvedGOp = false;
            string approvedBy = string.Empty;
            OptionSetValue gopApproval = new OptionSetValue();
            if (gop.Attributes.Contains("jarvis_requesttype") && gop.Attributes["jarvis_requesttype"] != null)
            {
                requesttype = (OptionSetValue)gop.Attributes["jarvis_requesttype"];
            }
            else if (gopImg.Attributes.Contains("jarvis_requesttype") && gopImg.Attributes["jarvis_requesttype"] != null)
            {
                requesttype = (OptionSetValue)gopImg.Attributes["jarvis_requesttype"];
            }

            if (gop.Attributes.Contains("jarvis_gopapproval"))
            {
                gopApproval = (OptionSetValue)gop.Attributes["jarvis_gopapproval"];
                if (gopApproval.Value.Equals(334030001))
                {
                    approvedGOp = true;
                }
            }
            else if (gopImg.Attributes.Contains("jarvis_gopapproval"))
            {
                gopApproval = (OptionSetValue)gopImg.Attributes["jarvis_gopapproval"];
                if (gopApproval.Value.Equals(334030001))
                {
                    approvedGOp = true;
                }
            }

            if (gop.Attributes.Contains("jarvis_contact") && gop.Attributes["jarvis_contact"] != null)
            {
                approvedBy = (string)gop.Attributes["jarvis_contact"];
            }
            else if (gopImg.Attributes.Contains("jarvis_contact") && gopImg.Attributes["jarvis_contact"] != null)
            {
                approvedBy = (string)gopImg.Attributes["jarvis_contact"];
            }

            if (requesttype.Value == 334030002)
            {
                if (!(approvedGOp && approvedBy.StartsWith("automatically approved via")))
                {
                    this.ShadowGOP(gop, gopImg, service);
                }
            }
        }*/

        /// <summary>
        /// SetGOPDealer.
        /// </summary>
        /// <param name="gop">gop param.</param>
        /// <param name="gopImg">gop Img.</param>
        /// <param name="incident">incident Obj.</param>
        /// <param name="service">Org service.</param>
        /// <param name="tracingService">tracing Service.</param>
        public void SetGOPdealer(Entity gop, Entity gopImg, EntityReference incident, IOrganizationService service, ITracingService tracingService)
        {
            tracingService.Trace("Set GOP Dealer when null");
            EntityCollection retrievedGopsforCase = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getGOPsForCase, incident.Id)));
            Entity lastModifiedGOPHD = retrievedGopsforCase.Entities.Where(g => g.Attributes.Contains("jarvis_gopapproval") && ((OptionSetValue)g.Attributes["jarvis_gopapproval"]).Value.Equals(334030001)).FirstOrDefault();
            if (lastModifiedGOPHD != null)
            {
                if ((!gop.Attributes.Contains("jarvis_dealer") && !gopImg.Attributes.Contains("jarvis_dealer")) || (gop.Attributes.Contains("jarvis_dealer") && gop.Attributes["jarvis_dealer"] == null))
                {
                    if (lastModifiedGOPHD.Attributes.Contains("jarvis_dealer") && lastModifiedGOPHD.Attributes["jarvis_dealer"] != null)
                    {
                        gop["jarvis_dealer"] = lastModifiedGOPHD.Attributes["jarvis_dealer"];
                    }
                }
            }
        }

        /// <summary>
        /// SetGOPDealer.
        /// </summary>
        /// <param name="gop">gop param.</param>
        /// <param name="gopImg">gop Img.</param>
        /// <param name="incident">incident Obj.</param>
        /// <param name="service">Org service.</param>
        /// <param name="tracingService">tracing Service.</param>
        public void SetFieldsFromApprovedGOP(Entity gop, Entity gopImg, EntityReference incident, IOrganizationService service, ITracingService tracingService)
        {
            tracingService.Trace("Set GOP Dealer when null");
            EntityCollection retrievedGopsforCase = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getGOPsForCase, incident.Id)));
            Entity lastModifiedGOPHD = retrievedGopsforCase.Entities.Where(g => g.Attributes.Contains("jarvis_gopapproval") && ((OptionSetValue)g.Attributes["jarvis_gopapproval"]).Value.Equals(334030001)).FirstOrDefault();
            if (lastModifiedGOPHD != null)
            {
                if ((!gop.Attributes.Contains("jarvis_paymenttype") && !gopImg.Attributes.Contains("jarvis_paymenttype")) || (gop.Attributes.Contains("jarvis_paymenttype") && gop.Attributes["jarvis_paymenttype"] == null))
                {
                    if (lastModifiedGOPHD.Attributes.Contains("jarvis_paymenttype") && lastModifiedGOPHD.Attributes["jarvis_paymenttype"] != null)
                    {
                        gop["jarvis_paymenttype"] = lastModifiedGOPHD.Attributes["jarvis_paymenttype"];

                        if (((OptionSetValue)lastModifiedGOPHD.Attributes["jarvis_paymenttype"]).Value.Equals(334030002))
                        {
                            // Set Channel
                            if ((!gop.Attributes.Contains("jarvis_channel") && !gopImg.Attributes.Contains("jarvis_channel")) || (gop.Attributes.Contains("jarvis_channel") && gop.Attributes["jarvis_channel"] == null))
                            {
                                if (lastModifiedGOPHD.Attributes.Contains("jarvis_channel") && lastModifiedGOPHD.Attributes["jarvis_channel"] != null)
                                {
                                    gop["jarvis_channel"] = lastModifiedGOPHD.Attributes["jarvis_channel"];
                                }
                            }

                            // Set VAT
                            if ((!gop.Attributes.Contains("jarvis_vat") && !gopImg.Attributes.Contains("jarvis_vat")) || (gop.Attributes.Contains("jarvis_vat") && gop.Attributes["jarvis_vat"] == null))
                            {
                                if (lastModifiedGOPHD.Attributes.Contains("jarvis_vat") && lastModifiedGOPHD.Attributes["jarvis_vat"] != null)
                                {
                                    gop["jarvis_vat"] = lastModifiedGOPHD.Attributes["jarvis_vat"];
                                }
                            }

                            // Set Credit card Language
                            if ((!gop.Attributes.Contains("jarvis_creditcardlanguage") && !gopImg.Attributes.Contains("jarvis_creditcardlanguage")) || (gop.Attributes.Contains("jarvis_creditcardlanguage") && gop.Attributes["jarvis_creditcardlanguage"] == null))
                            {
                                if (lastModifiedGOPHD.Attributes.Contains("jarvis_creditcardlanguage") && lastModifiedGOPHD.Attributes["jarvis_creditcardlanguage"] != null)
                                {
                                    gop["jarvis_creditcardlanguage"] = lastModifiedGOPHD.Attributes["jarvis_creditcardlanguage"];
                                }
                            }

                            // Set Credit card Currency
                            if ((!gop.Attributes.Contains("jarvis_creditcardincurrency") && !gopImg.Attributes.Contains("jarvis_creditcardincurrency")) || (gop.Attributes.Contains("jarvis_creditcardincurrency") && gop.Attributes["jarvis_creditcardincurrency"] == null))
                            {
                                if (lastModifiedGOPHD.Attributes.Contains("jarvis_creditcardincurrency") && lastModifiedGOPHD.Attributes["jarvis_creditcardincurrency"] != null)
                                {
                                    gop["jarvis_creditcardincurrency"] = lastModifiedGOPHD.Attributes["jarvis_creditcardincurrency"];
                                }
                            }
                        }
                    }

                }

            }
        }

        /// <summary>
        /// OnCopyCancelParentGop.
        /// </summary>
        /// <param name="gop">gop.</param>
        /// <param name="gopImg">Gop image.</param>
        /// <param name="service">service.</param>
        /// <param name="trace">trace.</param>
        public void OnCopyCancelParentGop(Entity gop, Entity gopImg, IOrganizationService service, ITracingService trace)
        {
            EntityReference caseId = new EntityReference();
            OptionSetValue requesttype = new OptionSetValue();

            if (gop.Attributes.Contains("jarvis_incident") && gop.Attributes["jarvis_incident"] != null)
            {
                caseId = (EntityReference)gop.Attributes["jarvis_incident"];
            }
            else if (gopImg.Attributes.Contains("jarvis_incident") && gopImg.Attributes["jarvis_incident"] != null)
            {
                caseId = (EntityReference)gopImg.Attributes["jarvis_incident"];
            }

            if (gop.Attributes.Contains("jarvis_requesttype") && gop.Attributes["jarvis_requesttype"] != null)
            {
                requesttype = (OptionSetValue)gop.Attributes["jarvis_requesttype"];
            }
            else if (gopImg.Attributes.Contains("jarvis_requesttype") && gopImg.Attributes["jarvis_requesttype"] != null)
            {
                requesttype = (OptionSetValue)gopImg.Attributes["jarvis_requesttype"];
            }

            if (gop.Attributes.Contains("jarvis_parentgop") && gop.Attributes["jarvis_parentgop"] != null)
            {
                EntityReference parentGopRef = (EntityReference)gop.Attributes["jarvis_parentgop"];
                Entity parentGop = service.Retrieve(gop.LogicalName, parentGopRef.Id, new ColumnSet("jarvis_gopapproval", "jarvis_dealer"));
                if (parentGop.Attributes.Contains("jarvis_gopapproval") && parentGop.Attributes["jarvis_gopapproval"] != null)
                {
                    bool parentGopApproved = ((OptionSetValue)parentGop.Attributes["jarvis_gopapproval"]).Value.Equals(334030001)
;

                    if (!parentGopApproved)
                    {
                        Entity updateParentGop = new Entity(gop.LogicalName, parentGopRef.Id);
                        if (gop.Attributes.Contains("jarvis_dealer") && parentGop.Attributes.Contains("jarvis_dealer"))
                        {
                            if (((EntityReference)gop.Attributes["jarvis_dealer"]).Id.Equals(((EntityReference)parentGop.Attributes["jarvis_dealer"]).Id))
                            {
                                updateParentGop["jarvis_isdealercopied"] = true;
                            }
                            else
                            {
                                updateParentGop["jarvis_isdealercopied"] = false;
                            }
                        }

                        // Check if the RD has already created Corresponding HD
                        EntityCollection realtedShadowGOps = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.geRelatedShadowGOP, updateParentGop.Id, caseId.Id)));
                        if (realtedShadowGOps.Entities.Count() > 0)
                        {
                            Entity shadowGOP = realtedShadowGOps.Entities[0];
                            shadowGOP["statecode"] = new OptionSetValue(1);
                            shadowGOP["statuscode"] = new OptionSetValue(334030001);
                            shadowGOP["jarvis_isdealercopied"] = true;
                            service.Update(shadowGOP);
                        }

                        updateParentGop["statecode"] = new OptionSetValue(1);
                        updateParentGop["statuscode"] = new OptionSetValue(334030001);
                        service.Update(updateParentGop);
                    }
                }

                if (requesttype.Value.Equals(334030002))
                {
                    EntityCollection realtedShadowGOps = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.geRelatedShadowGOP, gop.Id, caseId.Id)));
                    if (realtedShadowGOps.Entities.Count() > 0)
                    {
                        Entity shadowGOP = realtedShadowGOps.Entities[0];
                        shadowGOP["statuscode"] = new OptionSetValue(20);
                        service.Update(shadowGOP);
                    }
                }
            }
        }

        /// <summary>
        /// CopyUnapprovedRDs.
        /// </summary>
        /// <param name="gop">gop.</param>
        /// <param name="gopImg">gopImg.</param>
        /// <param name="caseObj">Parent case reference .</param>
        /// <param name="serviceFee">serviceFee.</param>
        /// <param name="servicefeeEntity">servicefeeEntity.</param>
        /// <param name="service">service.</param>
        /// <param name="adminservice">adminservice.</param>
        /// <param name="tracingService">tracingService.</param>
        public void CopyUnapprovedRDs(Entity gop, Entity gopImg, Entity caseObj, decimal serviceFee, EntityReference servicefeeEntity, IOrganizationService service, IOrganizationService adminservice, ITracingService tracingService)
        {
            EntityReference caseId = new EntityReference();
            EntityReference gopDealer = new EntityReference();
            EntityReference currencyout = new EntityReference();
            List<Entity> unapprovedRDs = new List<Entity>();
            bool isInactive = false;
            Entity parentHD = new Entity(gop.LogicalName, gop.Id);
            int counter = 0;
            #region Initialise parent GOP That is cascading the RDs for calucltion
            if (gop.Attributes.Contains("jarvis_totallimitoutcurrency"))
            {
                parentHD["jarvis_totallimitoutcurrency"] = (EntityReference)gop["jarvis_totallimitoutcurrency"];
            }
            else if (gopImg.Attributes.Contains("jarvis_totallimitoutcurrency"))
            {
                parentHD["jarvis_totallimitoutcurrency"] = (EntityReference)gopImg["jarvis_totallimitoutcurrency"];
            }

            if (gop.Attributes.Contains("jarvis_totallimitout"))
            {
                parentHD["jarvis_totallimitout"] = (decimal)gop["jarvis_totallimitout"];
            }
            else if (gopImg.Attributes.Contains("jarvis_totallimitout"))
            {
                parentHD["jarvis_totallimitout"] = (decimal)gopImg["jarvis_totallimitout"];
            }

            if (gop.Attributes.Contains("jarvis_totallimitin"))
            {
                parentHD["jarvis_totallimitin"] = (decimal)gop["jarvis_totallimitin"];
            }
            else if (gopImg.Attributes.Contains("jarvis_totallimitin"))
            {
                parentHD["jarvis_totallimitin"] = (decimal)gopImg["jarvis_totallimitin"];
            }

            if (gop.Attributes.Contains("jarvis_totallimitincurrency"))
            {
                parentHD["jarvis_totallimitincurrency"] = (EntityReference)gop["jarvis_totallimitincurrency"];
            }
            else if (gopImg.Attributes.Contains("jarvis_totallimitincurrency"))
            {
                parentHD["jarvis_totallimitincurrency"] = (EntityReference)gopImg["jarvis_totallimitincurrency"];
            }

            if (gop.Attributes.Contains("jarvis_requesttype") && gop.Attributes["jarvis_requesttype"] != null)
            {
                parentHD["jarvis_requesttype"] = (OptionSetValue)gop.Attributes["jarvis_requesttype"];
            }
            else if (gopImg.Attributes.Contains("jarvis_requesttype") && gopImg.Attributes["jarvis_requesttype"] != null)
            {
                parentHD["jarvis_requesttype"] = (OptionSetValue)gopImg.Attributes["jarvis_requesttype"];
            }

            if (gop.Attributes.Contains("jarvis_goplimitin") && gop.Attributes["jarvis_goplimitin"] != null)
            {
                parentHD["jarvis_goplimitin"] = (decimal)gop.Attributes["jarvis_goplimitin"];
            }
            else if (gopImg.Attributes.Contains("jarvis_goplimitin") && gopImg.Attributes["jarvis_goplimitin"] != null)
            {
                parentHD["jarvis_goplimitin"] = (decimal)gopImg.Attributes["jarvis_goplimitin"];
            }

            if (gop.Attributes.Contains("jarvis_goplimitout") && gop.Attributes["jarvis_goplimitout"] != null)
            {
                parentHD["jarvis_goplimitout"] = (decimal)gop.Attributes["jarvis_goplimitout"];
            }
            else if (gopImg.Attributes.Contains("jarvis_goplimitout") && gopImg.Attributes["jarvis_goplimitout"] != null)
            {
                parentHD["jarvis_goplimitout"] = (decimal)gopImg.Attributes["jarvis_goplimitout"];
            }

            if (gop.Attributes.Contains("jarvis_gopincurrency") && gop.Attributes["jarvis_gopincurrency"] != null)
            {
                parentHD["jarvis_gopincurrency"] = (EntityReference)gop.Attributes["jarvis_gopincurrency"];
            }
            else if (gopImg.Attributes.Contains("jarvis_gopincurrency") && gopImg.Attributes["jarvis_gopincurrency"] != null)
            {
                parentHD["jarvis_gopincurrency"] = (EntityReference)gopImg.Attributes["jarvis_gopincurrency"];
            }

            if (gop.Attributes.Contains("jarvis_gopoutcurrency") && gop.Attributes["jarvis_gopoutcurrency"] != null)
            {
                parentHD["jarvis_gopoutcurrency"] = (EntityReference)gop.Attributes["jarvis_gopoutcurrency"];
            }
            else if (gopImg.Attributes.Contains("jarvis_gopoutcurrency") && gopImg.Attributes["jarvis_gopoutcurrency"] != null)
            {
                parentHD["jarvis_gopoutcurrency"] = (EntityReference)gopImg.Attributes["jarvis_gopoutcurrency"];
            }

            if (gop.Attributes.Contains("jarvis_gopapproval"))
            {
                parentHD["jarvis_gopapproval"] = (OptionSetValue)gop.Attributes["jarvis_gopapproval"];
            }
            else if (gopImg.Attributes.Contains("jarvis_gopapproval"))
            {
                parentHD["jarvis_gopapproval"] = (OptionSetValue)gopImg.Attributes["jarvis_gopapproval"];
            }

            if (gop.Attributes.Contains("jarvis_paymenttype") && gop.Attributes["jarvis_paymenttype"] != null)
            {
                parentHD["jarvis_paymenttype"] = (OptionSetValue)gop.Attributes["jarvis_paymenttype"];
            }
            else if (gopImg.Attributes.Contains("jarvis_paymenttype") && gopImg.Attributes["jarvis_paymenttype"] != null)
            {
                parentHD["jarvis_paymenttype"] = (OptionSetValue)gopImg.Attributes["jarvis_paymenttype"];
            }

            if (gop.Attributes.Contains("jarvis_repairingdealer") && gop.Attributes["jarvis_repairingdealer"] != null)
            {
                parentHD["jarvis_repairingdealer"] = (EntityReference)gop.Attributes["jarvis_repairingdealer"];
            }
            else if (gopImg.Attributes.Contains("jarvis_repairingdealer") && gopImg.Attributes["jarvis_repairingdealer"] != null)
            {
                parentHD["jarvis_repairingdealer"] = (EntityReference)gopImg.Attributes["jarvis_repairingdealer"];
            }

            if (gopImg.Attributes.Contains("jarvis_dealer") && gopImg.Attributes["jarvis_dealer"] != null)
            {
                parentHD["jarvis_dealer"] = (EntityReference)gopImg.Attributes["jarvis_dealer"];
            }
            #endregion

            if (gop.Attributes.Contains("statecode"))
            {
                isInactive = ((OptionSetValue)gop.Attributes["statecode"]).Value.Equals(1);
            }
            else if (gopImg.Attributes.Contains("statecode"))
            {
                isInactive = ((OptionSetValue)gopImg.Attributes["statecode"]).Value.Equals(1);
            }

            if (gop.Attributes.Contains("jarvis_incident") && gop.Attributes["jarvis_incident"] != null)
            {
                caseId = (EntityReference)gop.Attributes["jarvis_incident"];
            }
            else if (gopImg.Attributes.Contains("jarvis_incident") && gopImg.Attributes["jarvis_incident"] != null)
            {
                caseId = (EntityReference)gopImg.Attributes["jarvis_incident"];
            }

            if (gop.Attributes.Contains("jarvis_dealer") && gop.Attributes["jarvis_dealer"] != null)
            {
                gopDealer = (EntityReference)gop.Attributes["jarvis_dealer"];
            }
            else if (gopImg.Attributes.Contains("jarvis_dealer") && gopImg.Attributes["jarvis_dealer"] != null)
            {
                gopDealer = (EntityReference)gopImg.Attributes["jarvis_dealer"];
            }

            parentHD["jarvis_incident"] = caseId;
            parentHD["jarvis_source_"] = new OptionSetValue(334030003);

            if (!isInactive && gop.Attributes.Contains("jarvis_gopapproval") && ((OptionSetValue)gop.Attributes["jarvis_gopapproval"]).Value.Equals(334030001))
            {
                EntityCollection retrievedGopsforCase = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getGOPsForDealer, caseId.Id, gopDealer.Id)));
                unapprovedRDs = retrievedGopsforCase.Entities.Where(g => g.Attributes.Contains("jarvis_gopapproval") && ((OptionSetValue)g.Attributes["jarvis_gopapproval"]).Value.Equals(334030000) && ((OptionSetValue)g.Attributes["jarvis_requesttype"]).Value.Equals(334030002)).OrderBy(g => g.Attributes["jarvis_totallimitin"]).ToList();
                foreach (Entity gopRD in unapprovedRDs)
                {
                    if (gopRD.Id != gop.Id && (!((OptionSetValue)gopImg.Attributes["jarvis_paymenttype"]).Value.Equals(334030002) || ((OptionSetValue)gopImg.Attributes["jarvis_volvopaypaymentrequestsent"]).Value.Equals(334030000)))
                    {
                        counter++;
                        EntityReference repairingDealer = new EntityReference();
                        if (gopRD.Attributes.Contains("jarvis_repairingdealer") && gopRD.Attributes["jarvis_repairingdealer"] != null)
                        {
                            repairingDealer = (EntityReference)gopRD.Attributes["jarvis_repairingdealer"];
                        }
                        else if (gopImg.Attributes.Contains("jarvis_repairingdealer") && gopImg.Attributes["jarvis_repairingdealer"] != null)
                        {
                            repairingDealer = (EntityReference)gopImg.Attributes["jarvis_repairingdealer"];
                        }

                        Entity copiedRD = new Entity(gopRD.LogicalName);

                        copiedRD["jarvis_incident"] = caseId;
                        copiedRD["jarvis_requesttype"] = (OptionSetValue)gopRD.Attributes["jarvis_requesttype"];
                        copiedRD["jarvis_source_"] = new OptionSetValue(334030003);
                        copiedRD["jarvis_repairingdealer"] = repairingDealer;
                        copiedRD["jarvis_dealer"] = gopDealer;

                        // copiedRD["statuscode"] = new OptionSetValue(334030001);
                        if (gopRD.Attributes.Contains("jarvis_paymenttype"))
                        {
                            copiedRD["jarvis_paymenttype"] = (OptionSetValue)gopRD.Attributes["jarvis_paymenttype"];
                            OptionSetValue paymentType = new OptionSetValue();
                            paymentType = (OptionSetValue)gopRD.Attributes["jarvis_paymenttype"];
                            if (paymentType.Value.Equals(334030002))
                            {
                                if (gopRD.Attributes.Contains("jarvis_vat"))
                                {
                                    copiedRD["jarvis_vat"] = gopRD.Attributes["jarvis_vat"];
                                }

                                if (gopRD.Attributes.Contains("jarvis_creditcardemailaddress"))
                                {
                                    copiedRD["jarvis_creditcardemailaddress"] = gopRD.Attributes["jarvis_creditcardemailaddress"];
                                }

                                if (gopRD.Attributes.Contains("jarvis_creditcardmessage"))
                                {
                                    copiedRD["jarvis_creditcardmessage"] = gopRD.Attributes["jarvis_creditcardmessage"];
                                }

                                if (gopRD.Attributes.Contains("jarvis_creditcardlanguage"))
                                {
                                    copiedRD["jarvis_creditcardlanguage"] = gopRD.Attributes["jarvis_creditcardlanguage"];
                                }

                                if (gopRD.Attributes.Contains("jarvis_channel"))
                                {
                                    copiedRD["jarvis_channel"] = gopRD.Attributes["jarvis_channel"];
                                }

                                if (gopRD.Attributes.Contains("jarvis_creditcardincurrency"))
                                {
                                    copiedRD["jarvis_creditcardincurrency"] = gopRD.Attributes["jarvis_creditcardincurrency"];
                                }
                            }
                        }

                        if (gopRD.Attributes.Contains("jarvis_goplimitout"))
                        {
                            copiedRD["jarvis_goplimitout"] = gopRD.Attributes["jarvis_goplimitout"];
                        }

                        if (gopRD.Attributes.Contains("jarvis_gopoutcurrency"))
                        {
                            copiedRD["jarvis_gopoutcurrency"] = gopRD.Attributes["jarvis_gopoutcurrency"];
                        }

                        if (gopRD.Attributes.Contains("jarvis_gopincurrency"))
                        {
                            copiedRD["jarvis_gopincurrency"] = gopRD.Attributes["jarvis_gopincurrency"];
                        }

                        if (gopRD.Attributes.Contains("jarvis_gopreason"))
                        {
                            copiedRD["jarvis_gopreason"] = gopRD.Attributes["jarvis_gopreason"];
                        }

                        if (gopRD.Attributes.Contains("jarvis_dealer") && gopRD.Attributes["jarvis_dealer"] != null)
                        {
                            copiedRD["jarvis_dealer"] = gopRD.Attributes["jarvis_dealer"];
                        }

                        if (gopRD.Attributes.Contains("jarvis_contact") && gopRD.Attributes["jarvis_contact"] != null)
                        {
                            copiedRD["jarvis_contact"] = gopRD.Attributes["jarvis_contact"];
                        }

                        if (gopRD.Attributes.Contains("jarvis_requestedcontact") && gopRD.Attributes["jarvis_requestedcontact"] != null)
                        {
                            copiedRD["jarvis_requestedcontact"] = gopRD.Attributes["jarvis_requestedcontact"];
                        }

                        if (gopRD.Attributes.Contains("jarvis_rdpartyordernumber") && gopRD.Attributes["jarvis_rdpartyordernumber"] != null)
                        {
                            copiedRD["jarvis_rdpartyordernumber"] = gopRD.Attributes["jarvis_rdpartyordernumber"];
                        }

                        copiedRD["jarvis_gopapproval"] = new OptionSetValue(334030000);
                        copiedRD["jarvis_parentgop"] = new EntityReference(gopRD.LogicalName, gopRD.Id);
                        copiedRD["jarvis_istranslate"] = true;

                        // To get Updated Case when one of teh RD is auto approved hence updating passout and Case avalable amount
                        if (counter > 1)
                        {
                            caseObj = service.Retrieve("incident", caseObj.Id, new ColumnSet("jarvis_totalgoplimitoutapproved", "jarvis_totalgoplimitinapproved", "jarvis_totalgoplimitoutapprovedcurrency", "jarvis_totalcurrencyinapproved", "jarvis_totalgoplimitin", "jarvis_totalcurrencyin", "jarvis_restgoplimitout", "jarvis_totalrestcurrencyout", "jarvis_totalgoplimitout", "jarvis_totalcurrencyout", "jarvis_totalpassoutamount", "jarvis_totalpassoutcurrency", "jarvis_totalcreditcardrequestedamountcurreny", "jarvis_totalcreditcardpaymentamountcurrency", "jarvis_totalbookedamountinclvat", "jarvis_caseserviceline"));

                        }
                        this.CalculateRD(copiedRD, parentHD, caseObj, service, serviceFee, servicefeeEntity, adminservice, tracingService);
                        adminservice.Create(copiedRD);
                    }
                }
            }
        }

        /// <summary>
        /// Calculation on GOp Level for RD.
        /// </summary>
        /// <param name="gop">gop.</param>
        /// <param name="gopImg">gopImg.</param>
        /// <param name="service">service.</param>
        /// <param name="serviceFee">serviceFee.</param>
        /// <param name="servicefeeEntity">servicefeeEntity.</param>
        /// <param name="adminservice">adminservice.</param>
        /// <param name="tracingService">tracingService.</param>
        public void CalculateRD(Entity gop, Entity gopImg, Entity caseObj, IOrganizationService service, decimal serviceFee, EntityReference servicefeeEntity, IOrganizationService adminservice, ITracingService tracingService)
        {
            #region Initialize variables
            EntityReference caseId = new EntityReference();
            EntityReference repairingDealer = new EntityReference();
            EntityReference gopDealer = new EntityReference();
            OptionSetValue requesttype = new OptionSetValue();
            OptionSetValue paymentType = new OptionSetValue();
            decimal totalLimitOut = 0;
            bool approvedGOp = false;
            decimal limitout = 0;
            decimal limitin = 0;
            string paymentTypeName = string.Empty;
            OptionSetValue gopApproval = new OptionSetValue();
            EntityReference limitinCurrency = new EntityReference();
            EntityReference limitoutCurrency = new EntityReference();
            decimal approvedGopTotalOutput = 0;
            EntityReference approvedGopCurrency = new EntityReference();
            decimal approvedGopTotalLimitIn = 0;
            EntityReference approvedGopCurrencyIn = new EntityReference();
            string lineage = string.Empty;
            decimal firstPendingCreditCardBooking = 0;
            EntityReference currencyfirstPendingCreditCard = new EntityReference();

            if (gop.Attributes.Contains("jarvis_lineage") && gop.Attributes["jarvis_lineage"] != null)
            {
                lineage = (string)gop.Attributes["jarvis_lineage"];
            }
            else if (gopImg.Attributes.Contains("jarvis_lineage") && gopImg.Attributes["jarvis_lineage"] != null)
            {
                lineage = (string)gopImg.Attributes["jarvis_lineage"];
            }

            if (gop.Attributes.Contains("jarvis_requesttype") && gop.Attributes["jarvis_requesttype"] != null)
            {
                requesttype = (OptionSetValue)gop.Attributes["jarvis_requesttype"];
            }
            else if (gopImg.Attributes.Contains("jarvis_requesttype") && gopImg.Attributes["jarvis_requesttype"] != null)
            {
                requesttype = (OptionSetValue)gopImg.Attributes["jarvis_requesttype"];
            }

            if (gop.Attributes.Contains("jarvis_goplimitin") && gop.Attributes["jarvis_goplimitin"] != null)
            {
                limitin = (decimal)gop.Attributes["jarvis_goplimitin"];
            }
            else if (gopImg.Attributes.Contains("jarvis_goplimitin") && gopImg.Attributes["jarvis_goplimitin"] != null)
            {
                limitin = (decimal)gopImg.Attributes["jarvis_goplimitin"];
            }

            if (gop.Attributes.Contains("jarvis_goplimitout") && gop.Attributes["jarvis_goplimitout"] != null)
            {
                limitout = (decimal)gop.Attributes["jarvis_goplimitout"];
            }
            else if (gopImg.Attributes.Contains("jarvis_goplimitout") && gopImg.Attributes["jarvis_goplimitout"] != null)
            {
                limitout = (decimal)gopImg.Attributes["jarvis_goplimitout"];
            }

            if (gop.Attributes.Contains("jarvis_gopincurrency") && gop.Attributes["jarvis_gopincurrency"] != null)
            {
                limitinCurrency = (EntityReference)gop.Attributes["jarvis_gopincurrency"];
            }
            else if (gopImg.Attributes.Contains("jarvis_gopincurrency") && gopImg.Attributes["jarvis_gopincurrency"] != null)
            {
                limitinCurrency = (EntityReference)gopImg.Attributes["jarvis_gopincurrency"];
            }

            if (gop.Attributes.Contains("jarvis_gopoutcurrency") && gop.Attributes["jarvis_gopoutcurrency"] != null)
            {
                limitoutCurrency = (EntityReference)gop.Attributes["jarvis_gopoutcurrency"];
            }
            else if (gopImg.Attributes.Contains("jarvis_gopoutcurrency") && gopImg.Attributes["jarvis_gopoutcurrency"] != null)
            {
                limitoutCurrency = (EntityReference)gopImg.Attributes["jarvis_gopoutcurrency"];
            }

            if (gop.Attributes.Contains("jarvis_gopapproval"))
            {
                gopApproval = (OptionSetValue)gop.Attributes["jarvis_gopapproval"];
                approvedGOp = ((OptionSetValue)gop.Attributes["jarvis_gopapproval"]).Value.Equals(334030001);
            }
            else if (gopImg.Attributes.Contains("jarvis_gopapproval"))
            {
                gopApproval = (OptionSetValue)gopImg.Attributes["jarvis_gopapproval"];
                approvedGOp = ((OptionSetValue)gopImg.Attributes["jarvis_gopapproval"]).Value.Equals(334030001);
            }

            if (gop.Attributes.Contains("jarvis_paymenttype") && gop.Attributes["jarvis_paymenttype"] != null)
            {
                paymentType = (OptionSetValue)gop.Attributes["jarvis_paymenttype"];
            }
            else if (gopImg.Attributes.Contains("jarvis_paymenttype") && gopImg.Attributes["jarvis_paymenttype"] != null)
            {
                paymentType = (OptionSetValue)gopImg.Attributes["jarvis_paymenttype"];
            }

            if (paymentType != null)
            {
                paymentTypeName = Enum.GetName(typeof(PaymentType), paymentType.Value);
            }

            if (gop.Attributes.Contains("jarvis_incident") && gop.Attributes["jarvis_incident"] != null)
            {
                caseId = (EntityReference)gop.Attributes["jarvis_incident"];
            }
            else if (gopImg.Attributes.Contains("jarvis_incident") && gopImg.Attributes["jarvis_incident"] != null)
            {
                caseId = (EntityReference)gopImg.Attributes["jarvis_incident"];
            }

            if (gop.Attributes.Contains("jarvis_repairingdealer") && gop.Attributes["jarvis_repairingdealer"] != null)
            {
                repairingDealer = (EntityReference)gop.Attributes["jarvis_repairingdealer"];
            }
            else if (gopImg.Attributes.Contains("jarvis_repairingdealer") && gopImg.Attributes["jarvis_repairingdealer"] != null)
            {
                repairingDealer = (EntityReference)gopImg.Attributes["jarvis_repairingdealer"];
            }

            if (gopImg.Attributes.Contains("jarvis_dealer") && gopImg.Attributes["jarvis_dealer"] != null)
            {
                gopDealer = (EntityReference)gopImg.Attributes["jarvis_dealer"];
            }

            bool volvoPayAutomationEnabled = CrmHelper.GetAutomationConfig(service, JarvisConfiguration.VolvoPayAutomation, tracingService);
            volvoPayAutomationEnabled = paymentType.Value.Equals(334030002) && volvoPayAutomationEnabled;
            //Entity caseObj = service.Retrieve("incident", caseId.Id, new ColumnSet("jarvis_restgoplimitout", "jarvis_totalrestcurrencyout", "jarvis_totalgoplimitoutapproved", "jarvis_totalgoplimitoutapprovedcurrency", "jarvis_totalbookedamountinclvat", "jarvis_totalcreditcardpaymentamountcurrency", "jarvis_caseserviceline"));
            bool passOutAutomationEnabled = CrmHelper.GetAutomationConfig(service, JarvisConfiguration.Automationavailableamount, tracingService);
            decimal exchangeValue = 0;
            decimal outexchangeValue = 1;
            //// 423126 - Enable/Disable One Case - GOP to Pass Out Available Amount.
            totalLimitOut = limitout;
            var requesterName = string.Empty;
            decimal caseAvialableAmount = 0;
            EntityReference currencyout = new EntityReference();
            decimal caseTotalApprovedAmount = 0;
            EntityReference currencyCaseTotalApproved = new EntityReference();
            decimal caseTotalApprovedBookingAmount = 0;
            EntityReference currencyCaseTotalApprovedBooking = new EntityReference();
            bool isforceCopied = false;
            if (gop.Attributes.Contains("jarvis_requestedcontact"))
            {
                requesterName = "and requested by " + (string)gop.Attributes["jarvis_requestedcontact"];
            }
            else if (gopImg.Attributes.Contains("jarvis_requestedcontact"))
            {
                requesterName = "and requested by " + (string)gopImg.Attributes["jarvis_requestedcontact"];
            }

            if (caseObj.Attributes.Contains("jarvis_restgoplimitout") && caseObj["jarvis_totalrestcurrencyout"] != null)
            {
                caseAvialableAmount = (decimal)caseObj["jarvis_restgoplimitout"];
                currencyout = (EntityReference)caseObj["jarvis_totalrestcurrencyout"];
            }

            if (caseObj.Attributes.Contains("jarvis_totalgoplimitoutapproved") && caseObj["jarvis_totalgoplimitoutapprovedcurrency"] != null)
            {
                caseTotalApprovedAmount = (decimal)caseObj["jarvis_totalgoplimitoutapproved"];
                currencyCaseTotalApproved = (EntityReference)caseObj["jarvis_totalgoplimitoutapprovedcurrency"];
            }

            if (gop.Attributes.Contains("jarvis_istranslate") && (bool)gop.Attributes["jarvis_istranslate"] == true)
            {
                isforceCopied = true;
            }
            #endregion

            if ((!gop.Attributes.Contains("jarvis_dealer") && !gopImg.Attributes.Contains("jarvis_dealer")) ||
                   (gop.Attributes.Contains("jarvis_dealer") && gop.Attributes["jarvis_dealer"] == null))
            {
                this.SetGOPdealer(gop, gopImg, caseId, service, tracingService);
                if (gop.Attributes.Contains("jarvis_dealer") && gop.Attributes["jarvis_dealer"] != null)
                {
                    gopDealer = (EntityReference)gop.Attributes["jarvis_dealer"];
                }

                if (gop.Attributes.Contains("jarvis_paymenttype") && gop.Attributes["jarvis_paymenttype"] != null)
                {
                    paymentType = (OptionSetValue)gop.Attributes["jarvis_paymenttype"];
                    paymentTypeName = Enum.GetName(typeof(PaymentType), paymentType.Value);
                }
            }

            // UserStory-138782: GOP from multiple HDs
            EntityCollection approvedGopOfDealer = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getGOPsForDealer, caseId.Id, gopDealer.Id)));

            decimal passoutTotalOutput = 0;
            EntityReference passoutCurrency = new EntityReference();

            // # Exception Bug -173833,571263
            Entity lastApprovedDealerGOP = new Entity();
            if (isforceCopied)
            {
               // if (gopImg.Attributes.Contains("jarvis_requesttype") && gopImg.Attributes["jarvis_requesttype"] != null && ((OptionSetValue)gopImg.Attributes["jarvis_requesttype"]).Value.Equals(334030001))
               // {
                    lastApprovedDealerGOP = gopImg;
                    if (gopImg.Attributes.Contains("jarvis_totallimitout"))
                    {
                        approvedGopTotalOutput = (decimal)gopImg["jarvis_totallimitout"];
                    }

                    if (gopImg.Attributes.Contains("jarvis_totallimitoutcurrency"))
                    {
                        approvedGopCurrency = (EntityReference)gopImg["jarvis_totallimitoutcurrency"];
                    }

                    if (gopImg.Attributes.Contains("jarvis_totallimitin"))
                    {
                        approvedGopTotalLimitIn = (decimal)gopImg["jarvis_totallimitin"];
                    }

                    if (gopImg.Attributes.Contains("jarvis_totallimitincurrency"))
                    {
                        approvedGopCurrencyIn = (EntityReference)gopImg["jarvis_totallimitincurrency"];
                    }

                    exchangeValue = this.CurrencyExchange(approvedGopCurrency.Id, limitoutCurrency.Id, service);

                // }
            }
            else
            {
                lastApprovedDealerGOP = approvedGopOfDealer.Entities.Where(g => ((OptionSetValue)g.Attributes["jarvis_gopapproval"]).Value.Equals(334030001)).FirstOrDefault();
                if (lastApprovedDealerGOP != null && lastApprovedDealerGOP.Id != Guid.Empty)
                {
                    approvedGopTotalOutput = (decimal)lastApprovedDealerGOP["jarvis_totallimitout"];
                    approvedGopCurrency = (EntityReference)lastApprovedDealerGOP["jarvis_totallimitoutcurrency"];
                    approvedGopTotalLimitIn = (decimal)lastApprovedDealerGOP["jarvis_totallimitin"];
                    approvedGopCurrencyIn = (EntityReference)lastApprovedDealerGOP["jarvis_totallimitincurrency"];
                    exchangeValue = this.CurrencyExchange(approvedGopCurrency.Id, limitoutCurrency.Id, service);
                }
            }

            EntityCollection casePassouts = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.CasePassouts, caseId.Id, repairingDealer.Id)));
            var passoutEntity = casePassouts.Entities.FirstOrDefault();
            if (casePassouts.Entities.Count > 0 && passoutEntity != null)
            {
                passoutTotalOutput = ((Money)passoutEntity["jarvis_goplimitout"]).Value;
                passoutCurrency = (EntityReference)passoutEntity["transactioncurrencyid"];
            }

            // #173833-IF GOP with “Payment type” <> “Credit Card” AND SUM of { (GOP OUT of RD) +(Sum of ALL the passed out amount(s) to the remaining RD(s)*other than the one used in this GOP RD) -(SUM of all last Approved Total GOP OUT value for each unique GOP Dealers)} <= Total GOP OUT from previous GOP
            decimal addedGopLimitOut = limitout;
            bool overrideGopLimitOut = false;

            if (lastApprovedDealerGOP != null && lastApprovedDealerGOP.Id != Guid.Empty)
            {
                // Step-1  (Sum of ALL the passed out amount(s) to the remaining RD(s)*other than the one used in this GOP RD)
                decimal totalPassoutAmount = 0;
                EntityCollection caseallPassouts = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.CaseActivePassouts, caseId.Id)));
                if (caseallPassouts.Entities.Count() > 0)
                {
                    foreach (var passout in caseallPassouts.Entities)
                    {
                        if (passout.Attributes.Contains("jarvis_goplimitout") && !passout.Id.Equals(repairingDealer.Id))
                        {
                            var passoutTotal = ((Money)passout["jarvis_goplimitout"]).Value;
                            var passoutCurrency2 = (EntityReference)passout["transactioncurrencyid"];

                            // Convert Passout Currency to GOT APProved Amount Currency
                            var passoutexchangeValue = this.CurrencyExchange(passoutCurrency2.Id, limitoutCurrency.Id, service);
                            totalPassoutAmount = totalPassoutAmount + (passoutTotal * passoutexchangeValue);
                        }
                    }

                    addedGopLimitOut = addedGopLimitOut + totalPassoutAmount;
                }

                // Step-2  SUM of all last Approved Total GOP OUT value for each unique GOP Dealers
                if (caseObj.Attributes.Contains("jarvis_totalgoplimitoutapproved") && caseObj["jarvis_totalgoplimitoutapprovedcurrency"] != null)
                {
                    var casetotalApprovedtoGoplimitOutexchange = this.CurrencyExchange(currencyCaseTotalApproved.Id, limitoutCurrency.Id, service);

                    // addedGopLimitOut = addedGopLimitOut - (caseTotalApprovedAmount * casetotalApprovedtoGoplimitOutexchange);
                    // Step-3 Compare
                    if (decimal.Round(addedGopLimitOut) <= decimal.Round(caseTotalApprovedAmount * casetotalApprovedtoGoplimitOutexchange))
                    {
                        overrideGopLimitOut = true;
                    }
                }
            }

            if (overrideGopLimitOut)
            {
                totalLimitOut = approvedGopTotalOutput * exchangeValue;
                lineage = lineage + "||overrideGopLimitOut from HD (Exception#173833) :" + lastApprovedDealerGOP.Id.ToString() + "HD Total Amount-" + approvedGopTotalOutput.ToString();
            }
            else
            {
                // 1. Add the GOP Amount of Last Approved GOP Of Same Home Dealer
                if (isforceCopied)
                {
                    if (caseObj != null && caseObj.Attributes.Contains("jarvis_totalgoplimitoutapproved") && caseObj.Attributes.Contains("jarvis_totalgoplimitoutapprovedcurrency"))
                    {
                        approvedGopTotalOutput = (decimal)caseObj["jarvis_totalgoplimitoutapproved"];
                        approvedGopCurrency = (EntityReference)caseObj["jarvis_totalgoplimitoutapprovedcurrency"];
                    }
                    else
                    {
                        if (gopImg.Attributes.Contains("jarvis_totallimitoutcurrency"))
                        {
                            approvedGopCurrency = (EntityReference)gopImg["jarvis_totallimitoutcurrency"];
                        }

                        if (gopImg.Attributes.Contains("jarvis_totallimitout"))
                        {
                            approvedGopTotalOutput = (decimal)gopImg["jarvis_totallimitout"];
                        }
                    }

                    if (gopImg.Attributes.Contains("jarvis_totallimitincurrency"))
                    {
                        approvedGopCurrencyIn = (EntityReference)gopImg["jarvis_totallimitincurrency"];
                    }

                    if (gopImg.Attributes.Contains("jarvis_requestedcontact"))
                    {
                        requesterName = "and requested by " + (string)gopImg.Attributes["jarvis_requestedcontact"];
                    }

                    exchangeValue = this.CurrencyExchange(approvedGopCurrency.Id, limitoutCurrency.Id, service);
                    decimal exchangedapprovedGopTotalOutput = approvedGopTotalOutput * exchangeValue;
                    lineage = lineage + "||IsForceCopied for HD Approval :" + gopImg.Id.ToString() + "(Limitout[" + totalLimitOut.ToString() + "]+ApprovedGOPtotalLimitOut[" + exchangedapprovedGopTotalOutput.ToString() + "])";
                    totalLimitOut = totalLimitOut + exchangedapprovedGopTotalOutput;
                }
                else
                {
                    foreach (var approvedGop in approvedGopOfDealer.Entities)
                    {
                        if (((OptionSetValue)approvedGop.Attributes["jarvis_gopapproval"]).Value.Equals(334030001) && !approvedGop.Id.Equals(gop.Id))
                        {
                            approvedGopTotalOutput = (decimal)approvedGop["jarvis_totallimitout"];
                            approvedGopCurrency = (EntityReference)approvedGop["jarvis_totallimitoutcurrency"];
                            approvedGopTotalLimitIn = (decimal)approvedGop["jarvis_totallimitin"];
                            approvedGopCurrencyIn = (EntityReference)approvedGop["jarvis_totallimitincurrency"];

                            // var paymentType = (OptionSetValue)gop.Attributes["jarvis_paymenttype"]
                            if (approvedGop.Attributes.Contains("jarvis_requestedcontact"))
                            {
                                requesterName = "and requested by " + (string)approvedGop.Attributes["jarvis_requestedcontact"];
                            }

                            exchangeValue = this.CurrencyExchange(approvedGopCurrency.Id, limitoutCurrency.Id, service);
                            decimal exchangedapprovedGopTotalOutput = approvedGopTotalOutput * exchangeValue;
                            lineage = lineage + "||Latest GOP :" + approvedGop.Id.ToString() + "(Limitout[" + totalLimitOut.ToString() + "]+ApprovedGOPtotalLimitOut[" + exchangedapprovedGopTotalOutput.ToString() + "])";
                            totalLimitOut = totalLimitOut + exchangedapprovedGopTotalOutput;
                            break;
                        }
                    }
                }

                // 2. Reduce Last Passout of the Repairing Delaer from Limit out
                exchangeValue = this.CurrencyExchange(passoutCurrency.Id, limitoutCurrency.Id, service);
                decimal exchangedpassoutTotalOutput = passoutTotalOutput * exchangeValue;
                totalLimitOut = totalLimitOut - exchangedpassoutTotalOutput;
                lineage = lineage + " - PassoutAmount[" + exchangedpassoutTotalOutput.ToString() + "]";

                // 3. Reduce the case Available amount
                if (caseObj.Attributes.Contains("jarvis_restgoplimitout") && caseObj["jarvis_totalrestcurrencyout"] != null)
                {
                    var restAmountexchangeValue = this.CurrencyExchange(currencyout.Id, limitoutCurrency.Id, service);
                    decimal exchangedcaseAvialableAmount = caseAvialableAmount * restAmountexchangeValue;
                    totalLimitOut = totalLimitOut - exchangedcaseAvialableAmount;
                    lineage = lineage + " - CaseAvialableAmount[" + exchangedcaseAvialableAmount.ToString() + "]";
                }
            }

            // createShadowGOp = true;
            // }
            gop["jarvis_totallimitout"] = (decimal)Math.Round(totalLimitOut, 2, System.MidpointRounding.AwayFromZero);
            gop["jarvis_totallimitoutcurrency"] = limitoutCurrency;
            gop["jarvis_lineage"] = lineage;

            // Calculate Total GOp Limit IN
            if (servicefeeEntity != null)
            {
                exchangeValue = this.CurrencyExchange(servicefeeEntity.Id, limitoutCurrency.Id, service);
            }
            else
            {
                exchangeValue = 1;
            }

            outexchangeValue = this.CurrencyExchange(limitoutCurrency.Id, limitinCurrency.Id, service);
            decimal calculateGOPIn = (totalLimitOut + (serviceFee * exchangeValue)) * outexchangeValue;

            gop["jarvis_totallimitin"] = (decimal)Math.Round(calculateGOPIn, 2, System.MidpointRounding.AwayFromZero);
            gop["jarvis_totallimitincurrency"] = limitinCurrency;

            // UserStory-131411 GOP+ request approval automation
            bool islimitoutLessThancaseAvailable = false;

            // Bug - 686560
            bool isautoapprovalCC = false;

            if (caseObj.Attributes.Contains("jarvis_restgoplimitout") && caseObj["jarvis_totalrestcurrencyout"] != null) // && !volvoPayAutomationEnabled)
            {
                var limitIntoCaseAvailableExchange = this.CurrencyExchange(currencyout.Id, limitoutCurrency.Id, service);
                decimal passoutGOPOutExchange = 0;
                if (passoutTotalOutput > 0 && passoutCurrency != null)
                {
                    passoutGOPOutExchange = this.CurrencyExchange(passoutCurrency.Id, limitoutCurrency.Id, service);
                }

                islimitoutLessThancaseAvailable = decimal.Round(limitout) <= decimal.Round((caseAvialableAmount * limitIntoCaseAvailableExchange) + (passoutTotalOutput * passoutGOPOutExchange));
            }

            exchangeValue = this.CurrencyExchange(approvedGopCurrencyIn.Id, limitinCurrency.Id, service);

            // Bug - 686560
            // Check if GOP RD with Payment Type Credit Card is applicable for autoapproval
            if ((decimal.Round(calculateGOPIn) <= decimal.Round(approvedGopTotalLimitIn * exchangeValue)) && !approvedGOp && (paymentType.Value != 334030006 || isforceCopied) && islimitoutLessThancaseAvailable && (lastApprovedDealerGOP != null))
            {
                isautoapprovalCC = true;
            }

            // Auto approval for GOP RD with other Payment types
            if ((decimal.Round(calculateGOPIn) <= decimal.Round(approvedGopTotalLimitIn * exchangeValue)) && !approvedGOp && (paymentType.Value != 334030006 || isforceCopied) && islimitoutLessThancaseAvailable && (lastApprovedDealerGOP != null) && !volvoPayAutomationEnabled)
            {
                gop.Attributes["jarvis_gopapproval"] = new OptionSetValue(334030001);
                gop["statuscode"] = new OptionSetValue(30);
                gop.Attributes["jarvis_gopapprovaltime"] = DateTime.UtcNow;
                gop.Attributes["jarvis_contact"] = "automatically approved via " + paymentTypeName + " " + requesterName;

                // gop.Attributes["jarvis_gopapproval"] = new OptionSetValue(334030001);
                //// 423126 - Enable/Disable One Case - GOP to Pass Out Available Amount.
                if (passOutAutomationEnabled && requesttype.Value == 334030002)
                {
                    this.UpdatePassoutFromRD(gop, gopImg, service, serviceFee, servicefeeEntity, adminservice);
                }

                //// this.CopyUnapprovedRDs(gop, gopImg, service, adminservice);
            }

            // else if ((calculateGOPIn <= (approvedGopTotalLimitIn * exchangeValue)) && !approvedGOp && paymentType.Value == 334030006){
            // this.checkShadowGOPConditions(gop, gopImg, service);
            // }
            else
            {
                if (approvedGOp && requesttype.Value == 334030002)
                {
                    // #90225-GOP+ approval: HD to RD (Flow 2/2)
                    //// 423126 - Enable/Disable One Case - GOP to Pass Out Available Amount.
                    // this.CopyUnapprovedRDs(gop, gopImg, service, adminservice);
                    if (passOutAutomationEnabled)
                    {
                        // gop.Attributes["jarvis_gopapproval"] = new OptionSetValue(334030001);
                        this.UpdatePassoutFromRD(gop, gopImg, service, serviceFee, servicefeeEntity, adminservice);
                    }
                }
            }

            #region calculate Booking Amount
            tracingService.Trace("volvopaymentenabled:" + volvoPayAutomationEnabled.ToString());
            if (volvoPayAutomationEnabled)
            {
                tracingService.Trace("Calculate Booking Amount for GOP RD");
                this.CalculateBookingAmount(gop, gopImg, caseObj, service, tracingService, servicefeeEntity, serviceFee, isautoapprovalCC);
            }
            #endregion
        }

        /// <summary>
        /// Get Staircase Fee.
        /// </summary>
        /// <param name="gop">gop Entity.</param>
        /// <param name="incident">incident Entity.</param>
        /// <param name="service">Org service.</param>
        /// <param name="tracingService">tracing service.</param>
        /// <returns>staircase fee entity.</returns>
        public Entity GetStaircaseFees(Entity gop, EntityReference incident, IOrganizationService service, ITracingService tracingService)
        {
            tracingService.Trace("Get Staircase Fees");
            Entity parentCase = service.Retrieve(incident.LogicalName, incident.Id, new ColumnSet("jarvis_caseserviceline"));
            Entity staircaseFee = null;
            if (gop.Attributes.Contains("jarvis_dealer") && gop.Attributes["jarvis_dealer"] != null)
            {
                EntityReference gopDealer = (EntityReference)gop.Attributes["jarvis_dealer"];
                Entity dealer = service.Retrieve(gopDealer.LogicalName, gopDealer.Id, new ColumnSet("jarvis_address1_country"));
                if (parentCase.Attributes.Contains("jarvis_caseserviceline") && dealer.Attributes.Contains("jarvis_address1_country"))
                {
                    EntityReference caseServiceLine = (EntityReference)parentCase.Attributes["jarvis_caseserviceline"];
                    EntityReference dealerCountry = (EntityReference)dealer.Attributes["jarvis_address1_country"];
                    EntityCollection staircaseFeesCollection = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.GetStaircaseFee, dealerCountry.Id, caseServiceLine.Id)));
                    tracingService.Trace("Get staircase fee count" + staircaseFeesCollection.Entities.Count());
                    if (staircaseFeesCollection.Entities.Count() > 0)
                    {
                        staircaseFee = staircaseFeesCollection.Entities[0];
                    }
                }
            }

            return staircaseFee;
        }

        /// <summary>
        /// Calculate Booking amount.
        /// </summary>
        /// <param name="gop">gop object.</param>
        /// <param name="gopImg">gop Image.</param>
        /// <param name="caseObj"> case object.</param>
        /// <param name="service">service object.</param>
        /// <param name="tracingService">tracing Service.</param>
        /// <param name="servicefeeEntity">service fee Entity.</param>
        /// <param name="serviceFee">service Fee.</param>
        /// <param name="isAutoApprovalCC">is AutoApproval CreditCard.</param>
        public void CalculateBookingAmount(Entity gop, Entity gopImg, Entity caseObj, IOrganizationService service, ITracingService tracingService, EntityReference servicefeeEntity, decimal serviceFee, bool isAutoApprovalCC)
        {
            decimal exchangeValue = 0;
            decimal totallimitout = 0;
            decimal bookingAmount = 0;
            EntityReference totallimitoutCurrency = new EntityReference();
            decimal totallimitin = 0;
            EntityReference totallimitinCurrency = new EntityReference();
            OptionSetValue gopApproval = new OptionSetValue();
            OptionSetValue requesttype = new OptionSetValue();
            EntityReference bookingCurrency = new EntityReference();
            EntityReference vatRef = new EntityReference();

            if (gop.Attributes.Contains("jarvis_requesttype") && gop.Attributes["jarvis_requesttype"] != null)
            {
                requesttype = (OptionSetValue)gop.Attributes["jarvis_requesttype"];
            }
            else if (gopImg.Attributes.Contains("jarvis_requesttype") && gopImg.Attributes["jarvis_requesttype"] != null)
            {
                requesttype = (OptionSetValue)gopImg.Attributes["jarvis_requesttype"];
            }

            if (gop.Attributes.Contains("jarvis_gopapproval"))
            {
                gopApproval = (OptionSetValue)gop.Attributes["jarvis_gopapproval"];
            }
            else if (gopImg.Attributes.Contains("jarvis_gopapproval"))
            {
                gopApproval = (OptionSetValue)gopImg.Attributes["jarvis_gopapproval"];
            }

            if (gop.Attributes.Contains("jarvis_totallimitout") && gop.Attributes["jarvis_totallimitout"] != null)
            {
                totallimitout = (decimal)gop.Attributes["jarvis_totallimitout"];
            }
            else if (gopImg.Attributes.Contains("jarvis_totallimitout") && gopImg.Attributes["jarvis_totallimitout"] != null)
            {
                totallimitout = (decimal)gopImg.Attributes["jarvis_totallimitout"];
            }

            if (gop.Attributes.Contains("jarvis_totallimitoutcurrency") && gop.Attributes["jarvis_totallimitoutcurrency"] != null)
            {
                totallimitoutCurrency = (EntityReference)gop.Attributes["jarvis_totallimitoutcurrency"];
            }
            else if (gopImg.Attributes.Contains("jarvis_totallimitoutcurrency") && gopImg.Attributes["jarvis_totallimitoutcurrency"] != null)
            {
                totallimitoutCurrency = (EntityReference)gopImg.Attributes["jarvis_totallimitoutcurrency"];
            }

            if (requesttype.Value == 334030002 && gopApproval.Value.Equals(334030000) && isAutoApprovalCC)
            {
                tracingService.Trace("Update Booking Amount to 0");
                gop["jarvis_creditcardgopinbooking"] = bookingAmount;
            }
            else
            {
                if (gopApproval.Value.Equals(334030000) && (gopImg.Attributes.Contains("jarvis_vat") || gop.Attributes.Contains("jarvis_vat")) && (gopImg.Attributes.Contains("jarvis_creditcardincurrency") || gop.Attributes.Contains("jarvis_creditcardincurrency")))
                {
                    tracingService.Trace("Calculate Booking Amount if not 0");
                    // Calculate Total GOp Limit IN
                    if (servicefeeEntity != null)
                    {
                        exchangeValue = this.CurrencyExchange(servicefeeEntity.Id, totallimitoutCurrency.Id, service);
                    }
                    else
                    {
                        exchangeValue = 1;
                    }

                    if (gop.Attributes.Contains("jarvis_totallimitin") && gop.Attributes["jarvis_totallimitin"] != null)
                    {
                        totallimitin = (decimal)gop.Attributes["jarvis_totallimitin"];
                    }

                    if (gop.Attributes.Contains("jarvis_totallimitincurrency") && gop.Attributes["jarvis_totallimitincurrency"] != null)
                    {
                        totallimitinCurrency = (EntityReference)gop.Attributes["jarvis_totallimitincurrency"];
                    }

                    if (gop.Attributes.Contains("jarvis_creditcardincurrency") && gop.Attributes["jarvis_creditcardincurrency"] != null)
                    {
                        bookingCurrency = (EntityReference)gop.Attributes["jarvis_creditcardincurrency"];
                    }
                    else if (gopImg.Attributes.Contains("jarvis_creditcardincurrency") && gopImg.Attributes["jarvis_creditcardincurrency"] != null)
                    {
                        bookingCurrency = (EntityReference)gopImg.Attributes["jarvis_creditcardincurrency"];
                    }

                    if (gop.Attributes.Contains("jarvis_vat") && gop.Attributes["jarvis_vat"] != null)
                    {
                        vatRef = (EntityReference)gop.Attributes["jarvis_vat"];
                    }
                    else if (gopImg.Attributes.Contains("jarvis_vat") && gopImg.Attributes["jarvis_vat"] != null)
                    {
                        vatRef = (EntityReference)gopImg.Attributes["jarvis_vat"];
                    }

                    // EntityReference bookingCurrency = (EntityReference)gopImg["jarvis_creditcardincurrency"];
                    // EntityReference vatRef = (EntityReference)gopImg["jarvis_vat"];
                    Entity vatEntity = service.Retrieve(vatRef.LogicalName, vatRef.Id, new ColumnSet("jarvis_vat"));
                    decimal vat = 0;

                    if (vatEntity.Attributes.Contains("jarvis_vat") && vatEntity["jarvis_vat"] != null)
                    {
                        vat = ((decimal)vatEntity["jarvis_vat"]) * (decimal)0.01;
                    }

                    EntityCollection caseGOPs = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getGOPsForCase, caseObj.Id)));
                    List<Entity> dealerGOPs = caseGOPs.Entities.Where(g => ((OptionSetValue)g.Attributes["jarvis_paymenttype"]).Value.Equals(334030002) && ((OptionSetValue)g.Attributes["jarvis_gopapproval"]).Value.Equals(334030001)).OrderByDescending(g => g.Attributes["jarvis_gopapprovaltime"]).ToList();
                    List<Entity> sameGOPDealer = dealerGOPs.Where(g => ((EntityReference)g.Attributes["jarvis_dealer"]).Id.Equals(((EntityReference)gop.Attributes["jarvis_dealer"]).Id)).ToList();
                    Entity requested_gopEntity = sameGOPDealer.FirstOrDefault();

                    if (dealerGOPs.Count > 0)
                    {
                        var totalLimitInToBookingAmountExchange = this.CurrencyExchange(totallimitoutCurrency.Id, bookingCurrency.Id, service);
                        decimal exchangedbookingAmount = totallimitout * totalLimitInToBookingAmountExchange;

                        if (requested_gopEntity.Attributes.Contains("jarvis_totallimitout") && requested_gopEntity.Attributes.Contains("jarvis_totallimitoutcurrency") && (((EntityReference)requested_gopEntity.Attributes["jarvis_creditcardincurrency"]).Id != null))
                        {
                            var gopEntitybookingAmount = (decimal)requested_gopEntity["jarvis_totallimitout"];
                            var gopEntityCurrency = (EntityReference)requested_gopEntity["jarvis_totallimitoutcurrency"];
                            var bookingmountexchangeValue = this.CurrencyExchange(gopEntityCurrency.Id, bookingCurrency.Id, service);
                            exchangedbookingAmount = exchangedbookingAmount - (gopEntitybookingAmount * bookingmountexchangeValue);
                        }

                        gop["jarvis_creditcardgopinbooking"] = (decimal)Math.Round(exchangedbookingAmount * (decimal)(1 + vat), 2, System.MidpointRounding.AwayFromZero);
                    }
                    else
                    {
                        // First Credit Card GOP
                        if (caseObj.Attributes.Contains("jarvis_caseserviceline") && caseObj["jarvis_caseserviceline"] != null)
                        {
                            EntityReference caseServiceLine = (EntityReference)caseObj.Attributes["jarvis_caseserviceline"];
                            Entity volvoPayFeeEntity = service.Retrieve(caseServiceLine.LogicalName, caseServiceLine.Id, new ColumnSet("jarvis_volvopayfee", "jarvis_volvopayfeecurrency"));
                            int vovlvopayfee = 0;
                            EntityReference vovlvopayfeeCurrency = totallimitinCurrency;
                            if (volvoPayFeeEntity.Attributes.Contains("jarvis_volvopayfee") && volvoPayFeeEntity["jarvis_volvopayfee"] != null)
                            {
                                vovlvopayfee = (int)volvoPayFeeEntity["jarvis_volvopayfee"];
                            }

                            if (volvoPayFeeEntity.Attributes.Contains("jarvis_volvopayfeecurrency") && volvoPayFeeEntity["jarvis_volvopayfeecurrency"] != null)
                            {
                                vovlvopayfeeCurrency = (EntityReference)volvoPayFeeEntity["jarvis_volvopayfeecurrency"];
                            }

                            var bookingmountexchangeValue = this.CurrencyExchange(vovlvopayfeeCurrency.Id, bookingCurrency.Id, service);
                            var totalLimitInToBookingAmountExchange = this.CurrencyExchange(totallimitinCurrency.Id, bookingCurrency.Id, service);
                            decimal exchangedbookingAmount = ((totallimitin * totalLimitInToBookingAmountExchange) + (vovlvopayfee * bookingmountexchangeValue)) * (decimal)(1 + vat);
                            gop["jarvis_creditcardgopinbooking"] = (decimal)Math.Round(exchangedbookingAmount, 2, System.MidpointRounding.AwayFromZero);
                        }
                    }

                    tracingService.Trace("Booking Amount calculation completed");
                }
            }
        }
    }
}
