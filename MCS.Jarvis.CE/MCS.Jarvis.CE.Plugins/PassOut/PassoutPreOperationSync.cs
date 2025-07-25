// <copyright file="PassOutPreOperationSync.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MCS.Jarvis.CE.Plugins
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using MCS.Jarvis.CE.BusinessProcessShared.AppNotification;
    using MCS.Jarvis.CE.BusinessProcessShared.PassOut;
    using Microsoft.Crm.Sdk.Messages;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;

    /// <summary>
    /// Pass out Pre Operation Sync.
    /// </summary>
    public class PassoutPreOperationSync : IPlugin
    {
        /// <summary>
        /// Execute method.
        /// </summary>
        /// <param name="serviceProvider">Service Provider.</param>
        /// <exception cref="InvalidPluginExecutionException">Invalid Plugin Execution Exception.</exception>
        public void Execute(IServiceProvider serviceProvider)
        {
            // Extract the tracing service for use in debugging sandboxed plug-ins.
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            // Obtain the execution context from the service provider.
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                try
                {
                    // region Update
                    if (context.MessageName.ToUpper() == "UPDATE")
                    {
                        IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                        IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
                        Guid initiatingUserID = context.UserId;
                        Entity passOut = (Entity)context.InputParameters["Target"];
                        string delayedReason = string.Empty;
                        DateTime gpsETA = DateTime.MinValue;
                        DateTime delayedETA = DateTime.MinValue;
                        DateTime eta = DateTime.MinValue;
                        DateTime atcDate = DateTime.MinValue;
                        DateTime ataDate = DateTime.MinValue;
                        OptionSetValue passoutpaymentType = new OptionSetValue();
                        Entity passOutImg = (Entity)context.PreEntityImages["PreImage"];
                        EntityReference incident = (EntityReference)passOutImg.Attributes["jarvis_incident"];
                        Entity parentCase = service.Retrieve(incident.LogicalName, incident.Id, new ColumnSet("jarvis_timezone", "jarvis_timezonelabel", "jarvis_atc", "jarvis_etc", "jarvis_eta", "jarvis_ata", "jarvis_totalgoplimitoutapproved", "jarvis_totalgoplimitoutapprovedcurrency", "jarvis_restgoplimitout", "jarvis_totalrestcurrencyout", "jarvis_eta", "jarvis_totalpassoutamount", "jarvis_totalpassoutcurrency", "statuscode"));
                        int timeZoneCode = 105;
                        string timeZoneLabel = "GMT+1";
                        if (parentCase.Attributes.Contains("jarvis_timezone") && parentCase.Attributes["jarvis_timezone"] != null)
                        {
                            timeZoneCode = (int)parentCase.Attributes["jarvis_timezone"];
                        }

                        if (parentCase.Attributes.Contains("jarvis_timezonelabel") && parentCase.Attributes["jarvis_timezonelabel"] != null)
                        {
                            timeZoneLabel = (string)parentCase.Attributes["jarvis_timezonelabel"];
                        }

                        int source = 0;
                        bool isModifiedByMercurius = false;
                        EntityReference modifiedBy = new EntityReference();
                        if (passOut.Attributes.Contains("modifiedby"))
                        {
                            modifiedBy = (EntityReference)passOut.Attributes["modifiedby"];
                        }
                        else if (passOutImg.Attributes.Contains("modifiedby"))
                        {
                            modifiedBy = (EntityReference)passOutImg.Attributes["modifiedby"];
                        }

                        if (modifiedBy != null)
                        {
                            // EntityReference modifiedBy = (EntityReference)passOutImg.Attributes["modifiedby"];
                            // string modifiedBy = string.Empty;
                            Entity userFullname = service.Retrieve(modifiedBy.LogicalName, modifiedBy.Id, new ColumnSet("fullname"));
                            if (userFullname != null && userFullname.Attributes.Contains("fullname"))
                            {
                                string fullName = (string)userFullname.Attributes["fullname"];
                                if (fullName.ToUpper().Contains("MERCURIUS"))
                                {
                                    isModifiedByMercurius = true;
                                }
                            }
                        }

                        if (passOut.Contains("jarvis_reason") && passOut.Attributes["jarvis_reason"] != null)
                        {
                            delayedReason = (string)passOut.Attributes["jarvis_reason"];
                        }
                        else if (passOutImg.Contains("jarvis_reason") && passOutImg.Attributes["jarvis_reason"] != null)
                        {
                            delayedReason = (string)passOutImg.Attributes["jarvis_reason"];
                        }

                        if (passOut.Contains("jarvis_gpseta") && passOut.Attributes["jarvis_gpseta"] != null)
                        {
                            gpsETA = (DateTime)passOut.Attributes["jarvis_gpseta"];
                        }
                        else if (passOutImg.Contains("jarvis_gpseta") && passOutImg.Attributes["jarvis_gpseta"] != null)
                        {
                            gpsETA = (DateTime)passOutImg.Attributes["jarvis_gpseta"];
                        }

                        if (passOut.Contains("jarvis_delayedeta") && passOut.Attributes["jarvis_delayedeta"] != null)
                        {
                            delayedETA = (DateTime)passOut.Attributes["jarvis_delayedeta"];
                        }
                        else if (passOutImg.Contains("jarvis_delayedeta") && passOutImg.Attributes["jarvis_delayedeta"] != null)
                        {
                            delayedETA = (DateTime)passOutImg.Attributes["jarvis_delayedeta"];
                        }

                        if (passOut.Contains("jarvis_eta") && passOut.Attributes["jarvis_eta"] != null)
                        {
                            eta = (DateTime)passOut.Attributes["jarvis_eta"];
                        }
                        else if (passOutImg.Contains("jarvis_eta") && passOutImg.Attributes["jarvis_eta"] != null)
                        {
                            eta = (DateTime)passOutImg.Attributes["jarvis_eta"];
                        }

                        var etaconverted = this.RetrieveLocalTimeFromUTCTime(service, eta, timeZoneCode);

                        if (passOut.Attributes.Contains("jarvis_source_") && passOut.Attributes["jarvis_source_"] != null)
                        {
                            source = ((OptionSetValue)passOut.Attributes["jarvis_source_"]).Value;
                        }
                        else if (passOutImg.Attributes.Contains("jarvis_source_") && passOutImg.Attributes["jarvis_source_"] != null)
                        {
                            source = ((OptionSetValue)passOutImg.Attributes["jarvis_source_"]).Value;
                        }

                        if (passOut.Contains("jarvis_paymenttype") && passOut.Attributes["jarvis_paymenttype"] != null)
                        {
                            passoutpaymentType = (OptionSetValue)passOut.Attributes["jarvis_paymenttype"];
                        }
                        else if (passOutImg.Attributes.Contains("jarvis_paymenttype") && passOutImg.Attributes["jarvis_paymenttype"] != null)
                        {
                            passoutpaymentType = (OptionSetValue)passOutImg.Attributes["jarvis_paymenttype"];
                        }
                        else
                        {
                            this.SetPaymentType(passOut, incident, service);
                        }

                        if (passOut.Contains("jarvis_repairingdealer") && passOut.Attributes["jarvis_repairingdealer"] != null)
                        {
                            tracingService.Trace("In update");
                            this.SetPassoutNameAndRDId(passOut, service);
                        }

                        //if (source.Equals(334030003))
                        //{
                        //    this.LimitOutValidation(passOut, passOutImg, parentCase, service);
                        //}
                        #region Update ETA
                        if (eta != DateTime.MinValue)
                        {
                            tracingService.Trace("ETA Contains value");
                            tracingService.Trace("Source value " + ((OptionSetValue)passOutImg["jarvis_source_"]).Value);
                            if (passOutImg.Contains("jarvis_source_") && ((OptionSetValue)passOutImg["jarvis_source_"]).Value == 334030003 && !passOut.Contains("jarvis_delayedeta"))
                            {
                                tracingService.Trace("Source is jarvis");
                                if (passOutImg.Contains("jarvis_eta") && passOutImg.Attributes["jarvis_eta"] != null)
                                {
                                    tracingService.Trace("Source is jarvis compare" + passOutImg.Attributes["jarvis_eta"]);
                                    if (passOutImg.Contains("jarvis_eta") && eta > (DateTime)passOutImg["jarvis_eta"] && passOutImg.Contains("jarvis_repairingdealer"))
                                    {
                                        tracingService.Trace("Post created with greater than previous eta");

                                        passOut["jarvis_delayedeta"] = passOut["jarvis_eta"];

                                        tracingService.Trace("Update Passout");
                                        Entity autoPost = new Entity("post");
                                        autoPost["source"] = new OptionSetValue(1);
                                        if (delayedReason != string.Empty)
                                        {
                                            autoPost["text"] = "ETA updated to " + etaconverted + timeZoneLabel + " with the reason '" + delayedReason + "' from " + ((EntityReference)passOutImg["jarvis_repairingdealer"]).Name + " by " + ((EntityReference)passOutImg["modifiedby"]).Name + " on Delayed ETA";
                                        }
                                        else
                                        {
                                            autoPost["text"] = "ETA updated to " + etaconverted + timeZoneLabel + " from " + ((EntityReference)passOutImg["jarvis_repairingdealer"]).Name + " by " + ((EntityReference)passOutImg["modifiedby"]).Name + " on Delayed ETA";
                                        }

                                        autoPost["regardingobjectid"] = new EntityReference("incident", parentCase.Id);
                                        service.Create(autoPost);
                                    }
                                    else if (passOut.Attributes.Contains("jarvis_eta") && passOut.Attributes["jarvis_eta"] != null && (DateTime)passOut["jarvis_eta"] < (DateTime)passOutImg["jarvis_eta"] && passOutImg.Contains("jarvis_repairingdealer"))
                                    {
                                        tracingService.Trace("Pre ETA Enter");
                                        tracingService.Trace("Post created with less than previous eta");
                                        Entity autoPost = new Entity("post");
                                        autoPost["source"] = new OptionSetValue(1);
                                        autoPost["text"] = "ETA updated to " + etaconverted + timeZoneLabel + " from " + ((EntityReference)passOutImg["jarvis_repairingdealer"]).Name + " by " + ((EntityReference)passOutImg["modifiedby"]).Name;
                                        tracingService.Trace("ETA updated to " + etaconverted + timeZoneLabel + " from " + ((EntityReference)passOutImg["jarvis_repairingdealer"]).Name + " by " + ((EntityReference)passOutImg["modifiedby"]).Name + "  with the reason if available");
                                        tracingService.Trace("source and type is paased:");
                                        autoPost["regardingobjectid"] = new EntityReference("incident", parentCase.Id);
                                        service.Create(autoPost);
                                        tracingService.Trace("After creation of post");
                                    }
                                }
                            }
                            else if (delayedETA != DateTime.MinValue && passOutImg.Contains("jarvis_repairingdealer") && passOut.Contains("jarvis_delayedeta") && ((OptionSetValue)passOutImg["jarvis_source_"]).Value == 334030003)
                            {
                                tracingService.Trace("Delayed Enter");
                                tracingService.Trace("reason");
                                Entity autoPost = new Entity("post");
                                autoPost["source"] = new OptionSetValue(1);
                                var delayedETAConverted = this.RetrieveLocalTimeFromUTCTime(service, delayedETA, timeZoneCode);
                                passOut["jarvis_eta"] = delayedETA;
                                if (delayedReason != string.Empty)
                                {
                                    autoPost["text"] = "ETA updated to " + delayedETAConverted + timeZoneLabel + " with the reason '" + delayedReason + "' from " + ((EntityReference)passOutImg["jarvis_repairingdealer"]).Name + " by " + ((EntityReference)passOutImg["modifiedby"]).Name + " on Delayed ETA ";
                                }
                                else
                                {
                                    autoPost["text"] = "ETA updated to " + delayedETAConverted + timeZoneLabel + " from " + ((EntityReference)passOutImg["jarvis_repairingdealer"]).Name + " by " + ((EntityReference)passOutImg["modifiedby"]).Name + " on Delayed ETA ";
                                }

                                autoPost["regardingobjectid"] = new EntityReference("incident", parentCase.Id);
                                service.Create(autoPost);
                            }
                        }
#pragma warning restore SA1123 // Do not place regions within elements

                        if (passOut.Contains("jarvis_gpseta") && passOut.Attributes["jarvis_gpseta"] != null && gpsETA != DateTime.MinValue && parentCase.Contains("jarvis_eta") && parentCase["jarvis_eta"] != null && passOutImg.Contains("jarvis_repairingdealer"))
                        {
                            TimeSpan ts = gpsETA - (DateTime)parentCase["jarvis_eta"];
                            double diff = Math.Abs(ts.TotalMinutes);
                            tracingService.Trace(diff + ":");
                            if (diff > 15)
                            {
                                passOut["jarvis_eta"] = gpsETA;
                                var day = gpsETA.Day;
                                var month = gpsETA.Month;
                                var year = gpsETA.Year;
                                var hh = gpsETA.Hour.ToString();
                                if (hh.ToString().Length < 2)
                                {
                                    hh = '0' + hh;
                                }

                                var mm = gpsETA.Minute.ToString();
                                if (mm.ToString().Length < 2)
                                {
                                    mm = '0' + mm;
                                }

                                var gpsETAConverted = this.RetrieveLocalTimeFromUTCTime(service, gpsETA, timeZoneCode);

                                var newDateToSet = new DateTime(year, month, day, 0, 0, 0, 0);
                                passOut["jarvis_etadate"] = newDateToSet;
                                passOut["jarvis_etatime"] = hh.ToString() + mm.ToString();
                                Entity autoPost = new Entity("post");
                                autoPost["source"] = new OptionSetValue(1);
                                autoPost["text"] = "ETA updated from GPS to " + gpsETAConverted + timeZoneLabel + " from " + ((EntityReference)passOutImg["jarvis_repairingdealer"]).Name;
                                autoPost["regardingobjectid"] = new EntityReference("incident", parentCase.Id);
                                service.Create(autoPost);
                            }
                        }

                        if (source != null)
                        {
                            var passoutCntrls = new List<string>() { "jarvis_eta", "jarvis_ata", "jarvis_etc", "jarvis_atc" };

                            foreach (var targetField in passoutCntrls)
                            {
                                if (passOut.Attributes.Contains(targetField) && passOut.Attributes[targetField] != null)
                                {
                                    var timefield = targetField + "time";
                                    var dateField = targetField + "date";
                                    var targetDateSource = (DateTime)passOut.Attributes[targetField];
                                    DateTime targetDate = this.RetrieveLocalTimeFromUTCTime(service, targetDateSource, timeZoneCode);

                                    var day = targetDate.Day;
                                    var month = targetDate.Month;
                                    var year = targetDate.Year;
                                    var hh = targetDate.Hour.ToString();
                                    if (hh.ToString().Length < 2)
                                    {
                                        hh = '0' + hh;
                                    }

                                    var mm = targetDate.Minute.ToString();
                                    if (mm.ToString().Length < 2)
                                    {
                                        mm = '0' + mm;
                                    }

                                    var newDateToSet = new DateTime(year, month, day, 0, 0, 0, 0);
                                    passOut[dateField] = newDateToSet;
                                    passOut[timefield] = hh.ToString() + mm.ToString();
                                }
                            }
                        }
                        #endregion

#pragma warning disable SA1123 // Do not place regions within elements
                        #region PassOut DateTime Update

                        if (!(context.Depth > 1))
                        {
                            tracingService.Trace($"Enter into Update PassOut Date Time: {context.Depth}");
                            PassOutProcess passOutProcess = new PassOutProcess();
                            passOut = passOutProcess.UpdatePassOutDateTimeField(service, passOut, passOutImg, timeZoneCode, tracingService);
                        }
#pragma warning restore SA1123 // Do not place regions within elements

                        #endregion

#pragma warning disable SA1123 // Do not place regions within elements
                        #region ATC cannot be earlier than ATA validation
                        var exceptionMessage = string.Empty;
#pragma warning restore SA1123 // Do not place regions within elements
                        if (!isModifiedByMercurius)
                        {
                            Entity caseEntity = service.Retrieve(parentCase.LogicalName, parentCase.Id, new ColumnSet("jarvis_hdrd"));
                            bool hdRD = false;
                            if (caseEntity.Attributes.Contains("jarvis_hdrd") && caseEntity.Attributes["jarvis_hdrd"] != null)
                            {
                                hdRD = (bool)caseEntity.Attributes["jarvis_hdrd"];
                            }

                            if (passOut.Attributes.Contains("jarvis_ata") && passOut.Attributes["jarvis_ata"] != null)
                            {
                                ataDate = (DateTime)passOut.Attributes["jarvis_ata"];
                                var ataConverted = this.RetrieveLocalTimeFromUTCTime(service, ataDate, timeZoneCode);
                                tracingService.Trace("ata converted" + ataConverted.ToString());

                                if (this.RetrieveLocalTimeFromUTCTime(service, ataDate, timeZoneCode).CompareTo(this.RetrieveLocalTimeFromUTCTime(service, DateTime.UtcNow, timeZoneCode)) > 0 && !hdRD)
                                {
                                    throw new InvalidPluginExecutionException("Timestamp error, ATA cannot be in the future.");
                                }

                                exceptionMessage = "ATA cannot be later than ATC";
                            }
                            else
                            {
                                Entity passOutDate = service.Retrieve(passOut.LogicalName, passOut.Id, new ColumnSet("jarvis_ata"));
                                if (passOutDate.Attributes.Contains("jarvis_ata") && passOutDate.Attributes["jarvis_ata"] != null)
                                {
                                    ataDate = (DateTime)passOutDate.Attributes["jarvis_ata"];
                                }
                            }

                            if (passOut.Attributes.Contains("jarvis_atc") && passOut.Attributes["jarvis_atc"] != null)
                            {
                                tracingService.Trace("ATC date" + passOut.Attributes["jarvis_atc"].ToString());
                                atcDate = (DateTime)passOut.Attributes["jarvis_atc"];
                                if (this.RetrieveLocalTimeFromUTCTime(service, atcDate, timeZoneCode).CompareTo(this.RetrieveLocalTimeFromUTCTime(service, DateTime.UtcNow, timeZoneCode)) > 0 && !hdRD)
                                {
                                    throw new InvalidPluginExecutionException("Timestamp error, ATC cannot be in the future.");
                                }

                                exceptionMessage = "ATC cannot be earlier than ATA";
                            }
                            else
                            {
                                Entity passOutDate = service.Retrieve(passOut.LogicalName, passOut.Id, new ColumnSet("jarvis_atc"));
                                if (passOutDate.Attributes.Contains("jarvis_atc") && passOutDate.Attributes["jarvis_atc"] != null)
                                {
                                    atcDate = (DateTime)passOutDate.Attributes["jarvis_atc"];
                                }
                            }

                            if (ataDate != DateTime.MinValue && atcDate != DateTime.MinValue)
                            {
                                DateTime ataConvertedDate = this.RetrieveLocalTimeFromUTCTime(service, ataDate, timeZoneCode);
                                DateTime atcConvertedDate = this.RetrieveLocalTimeFromUTCTime(service, atcDate, timeZoneCode);
                                tracingService.Trace("ATA date Converted to local:" + ataConvertedDate);
                                tracingService.Trace("ATC date Converted to local:" + atcConvertedDate);

                                if (atcConvertedDate.CompareTo(ataConvertedDate) < 0 && exceptionMessage != null)
                                {
                                    // atc is earlier than ata
                                    throw new InvalidPluginExecutionException(exceptionMessage);
                                }
                            }
                        }
                        #endregion
                    }
                    else if (context.MessageName.ToUpper() == "CREATE")
                    {
                        // region Create
                        IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                        IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
                        Entity passOut = (Entity)context.InputParameters["Target"];
                        OptionSetValue passoutpaymentType = new OptionSetValue();
                        EntityReference incident = (EntityReference)passOut.Attributes["jarvis_incident"];
                        Entity parentCase = service.Retrieve(incident.LogicalName, incident.Id, new ColumnSet("jarvis_timezone", "jarvis_timezonelabel", "createdon", "jarvis_restgoplimitout", "jarvis_totalrestcurrencyout"));

                        int timeZoneCode = 105;
                        string timeZoneLabel = "GMT+1";
                        if (parentCase.Attributes.Contains("jarvis_timezone") && parentCase.Attributes["jarvis_timezone"] != null)
                        {
                            timeZoneCode = (int)parentCase.Attributes["jarvis_timezone"];
                        }

                        if (parentCase.Attributes.Contains("jarvis_timezonelabel") && parentCase.Attributes["jarvis_timezonelabel"] != null)
                        {
                            timeZoneLabel = (string)parentCase.Attributes["jarvis_timezonelabel"];
                        }

                        DateTime atcDate = DateTime.MinValue;
                        DateTime ataDate = DateTime.MinValue;

                        bool isCreatedByMercurius = false;
                        EntityReference createdBy = new EntityReference();
                        if (passOut.Attributes.Contains("createdby"))
                        {
                            createdBy = (EntityReference)passOut.Attributes["createdby"];
                        }

                        if (createdBy != null)
                        {
                            // EntityReference modifiedBy = (EntityReference)passOutImg.Attributes["modifiedby"];
                            // string modifiedBy = string.Empty;
                            Entity userFullname = service.Retrieve(createdBy.LogicalName, createdBy.Id, new ColumnSet("fullname"));
                            if (userFullname != null && userFullname.Attributes.Contains("fullname"))
                            {
                                string fullName = (string)userFullname.Attributes["fullname"];
                                if (fullName.ToUpper().Contains("MERCURIUS"))
                                {
                                    isCreatedByMercurius = true;
                                }
                            }
                        }

                        if (passOut.Attributes.Contains("jarvis_repairingdealer") && passOut.Attributes["jarvis_repairingdealer"] != null)
                        {
                            EntityReference repairDealer = (EntityReference)passOut.Attributes["jarvis_repairingdealer"];
                            EntityCollection openPassOuts = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getRDPassOuts, repairDealer.Id, incident.Id)));
                            if (openPassOuts.Entities.Count > 0)
                            {
                                throw new InvalidPluginExecutionException("Open Pass Out(s) with the same Repairing Dealer already exists");
                            }
                        }

                        int source = 0;
                        if (passOut.Attributes.Contains("jarvis_source_") && passOut.Attributes["jarvis_source_"] != null)
                        {
                            source = ((OptionSetValue)passOut.Attributes["jarvis_source_"]).Value;
                        }

                        if (source != 334030003)
                        {
                            var passoutCntrls = new List<string>() { "jarvis_eta", "jarvis_ata", "jarvis_etc", "jarvis_atc" };

                            foreach (var targetField in passoutCntrls)
                            {
                                if (passOut.Attributes.Contains(targetField) && passOut.Attributes[targetField] != null)
                                {
                                    var timefield = targetField + "time";
                                    var dateField = targetField + "date";
                                    if (parentCase.Attributes.Contains("jarvis_timezonelabel") && parentCase.Attributes["jarvis_timezonelabel"] != null)
                                    {
                                        timeZoneLabel = (string)parentCase.Attributes["jarvis_timezonelabel"];
                                    }

                                    var targetDateSource = (DateTime)passOut.Attributes[targetField];
                                    DateTime targetDate = this.RetrieveLocalTimeFromUTCTime(service, targetDateSource, timeZoneCode);

                                    var day = targetDate.Day;
                                    var month = targetDate.Month;
                                    var year = targetDate.Year;
                                    var hh = targetDate.Hour.ToString();
                                    if (hh.ToString().Length < 2)
                                    {
                                        hh = '0' + hh;
                                    }

                                    var mm = targetDate.Minute.ToString();
                                    if (mm.ToString().Length < 2)
                                    {
                                        mm = '0' + mm;
                                    }

                                    var newDateToSet = new DateTime(year, month, day, 0, 0, 0, 0);
                                    passOut[dateField] = newDateToSet;
                                    passOut[timefield] = hh.ToString() + mm.ToString();
                                }
                            }
                        }
                        //else
                        //{
                        //    this.LimitOutValidation(passOut, passOut, parentCase, service);
                        //}

                        if (passOut.Contains("jarvis_paymenttype") && passOut.Attributes["jarvis_paymenttype"] != null)
                        {
                            passoutpaymentType = (OptionSetValue)passOut.Attributes["jarvis_paymenttype"];
                        }
                        else
                        {
                            this.SetPaymentType(passOut, incident, service);
                        }

                        if (passOut.Attributes.Contains("jarvis_repairingdealer") && passOut.Attributes["jarvis_repairingdealer"] != null)
                        {
                            tracingService.Trace("In create");
                            this.SetPassoutNameAndRDId(passOut, service);
                        }

#pragma warning disable SA1123 // Do not place regions within elements
                        #region PassOut DateTime Update
                        if (!(context.Depth > 1))
                        {
                            tracingService.Trace($"Enter into Create PassOut Date Time: {context.Depth}");
                            PassOutProcess passOutProcess = new PassOutProcess();
                            passOut = passOutProcess.UpdatePassOutDateTimeField(service, passOut, null, timeZoneCode, tracingService);
                        }
#pragma warning restore SA1123 // Do not place regions within elements
                        #endregion

#pragma warning disable SA1123 // Do not place regions within elements
                        #region ATC cannot be earlier than ATA validation
                        var exceptionMessage = string.Empty;
#pragma warning restore SA1123 // Do not place regions within elements
                        if (!isCreatedByMercurius)
                        {
                            if (passOut.Attributes.Contains("jarvis_ata") && passOut.Attributes["jarvis_ata"] != null)
                            {
                                ataDate = (DateTime)passOut.Attributes["jarvis_ata"];
                                var ataConverted = this.RetrieveLocalTimeFromUTCTime(service, ataDate, timeZoneCode);
                                tracingService.Trace("ata converted" + ataConverted.ToString());

                                if (this.RetrieveLocalTimeFromUTCTime(service, ataDate, timeZoneCode).CompareTo(this.RetrieveLocalTimeFromUTCTime(service, DateTime.UtcNow, timeZoneCode)) > 0)
                                {
                                    throw new InvalidPluginExecutionException("Timestamp error, ATA cannot be in the future.");
                                }

                                exceptionMessage = "ATA cannot be later than ATC";
                            }

                            if (passOut.Attributes.Contains("jarvis_atc") && passOut.Attributes["jarvis_atc"] != null)
                            {
                                tracingService.Trace("ATC date" + passOut.Attributes["jarvis_atc"].ToString());
                                atcDate = (DateTime)passOut.Attributes["jarvis_atc"];
                                if (this.RetrieveLocalTimeFromUTCTime(service, atcDate, timeZoneCode).CompareTo(this.RetrieveLocalTimeFromUTCTime(service, DateTime.UtcNow, timeZoneCode)) > 0)
                                {
                                    throw new InvalidPluginExecutionException("Timestamp error, ATC cannot be in the future.");
                                }

                                exceptionMessage = "ATC cannot be earlier than ATA";
                            }

                            if (ataDate != DateTime.MinValue && atcDate != DateTime.MinValue)
                            {
                                DateTime ataConvertedDate = this.RetrieveLocalTimeFromUTCTime(service, ataDate, timeZoneCode);
                                DateTime atcConvertedDate = this.RetrieveLocalTimeFromUTCTime(service, atcDate, timeZoneCode);
                                tracingService.Trace("ATA date Converted to local:" + ataConvertedDate);
                                tracingService.Trace("ATC date Converted to local:" + atcConvertedDate);

                                if (atcConvertedDate.CompareTo(ataConvertedDate) < 0 && exceptionMessage != null)
                                {
                                    // atc is earlier than ata
                                    throw new InvalidPluginExecutionException(exceptionMessage);
                                }
                            }
                        }
                        #endregion
                    }

                    // endregion
                }
                catch (InvalidPluginExecutionException ex)
                {
                    throw new InvalidPluginExecutionException(ex.Message + string.Empty);
                }
            }
        }

        /// <summary>
        /// Retrieve Local Time From UTC Time.
        /// </summary>
        /// <param name="service">Org Service.</param>
        /// <param name="utcTime">UTC Time.</param>
        /// <param name="timeZoneCode">Time Zone Code.</param>
        /// <returns>Date Time.</returns>
        public DateTime RetrieveLocalTimeFromUTCTime(IOrganizationService service, DateTime utcTime, int? timeZoneCode)
        {
            if (!timeZoneCode.HasValue)
            {
                return utcTime;
            }

            var request = new LocalTimeFromUtcTimeRequest
            {
                TimeZoneCode = timeZoneCode.Value,
                UtcTime = utcTime.ToUniversalTime(),
            };

            var response = (LocalTimeFromUtcTimeResponse)service.Execute(request);

            return response.LocalTime;
        }

        /// <summary>
        /// execute BPF.
        /// </summary>
        /// <param name="incidentRetrieve">incident Retrieve.</param>
        /// <param name="stageName">stage Name.</param>
        /// <param name="service">Org service.</param>
        /// <param name="tracingService">tracing Service.</param>
        internal void ExecuteBPF(EntityReference incidentRetrieve, string stageName, IOrganizationService service, ITracingService tracingService)
        {
            // region Stage Updates
            string activeStageName = stageName;

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
                if (getcaseBPFProcess.Entities[0].Attributes.Contains("statecode"))
                {
                    if (((OptionSetValue)getcaseBPFProcess.Entities[0].Attributes["statecode"]).Value != 0)
                    {
                        return;
                    }
                }

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
                        //// service.Retrieve(activeProcessInstance.LogicalName, activeProcessInstanceID , new ColumnSet(true));
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

            // endregion
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

        /// <summary>
        /// Set Pass out payment Type.
        /// </summary>
        /// <param name="passout">pass out.</param>
        /// <param name="incident">incident details.</param>
        /// <param name="service">Org service.</param>
        internal void SetPaymentType(Entity passout, EntityReference incident, IOrganizationService service)
        {
            // get existing GOP PaymentType and set
            EntityCollection gopOfDealer = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getLastModGOP, incident.Id)));
            if (gopOfDealer.Entities.Count > 0)
            {
                Entity latestGOP = gopOfDealer.Entities.Where(g => (g.Attributes.Contains("jarvis_paymenttype") && g.Attributes["jarvis_paymenttype"] != null)).OrderByDescending(g => g.Attributes["modifiedon"]).FirstOrDefault();
                if (latestGOP != null)
                {
                    OptionSetValue paymentType = (OptionSetValue)latestGOP.Attributes["jarvis_paymenttype"];
                    passout["jarvis_paymenttype"] = paymentType;
                }
            }
        }

        /// <summary>
        /// Set Pass out Name And RD Id.
        /// </summary>
        /// <param name="passOut">pass out.</param>
        /// <param name="service">Org service.</param>
        internal void SetPassoutNameAndRDId(Entity passOut, IOrganizationService service)
        {
            EntityReference repairingDealer = (EntityReference)passOut.Attributes["jarvis_repairingdealer"];
            Entity rddetails = service.Retrieve(repairingDealer.LogicalName, repairingDealer.Id, new ColumnSet("name", "jarvis_responsableunitid"));
            if (rddetails.Attributes.Contains("name") && rddetails.Attributes["name"] != null)
            {
                passOut["jarvis_name"] = rddetails.Attributes["name"];
            }

            if (rddetails.Attributes.Contains("jarvis_responsableunitid") && rddetails.Attributes["jarvis_responsableunitid"] != null)
            {
                passOut["jarvis_repairingdealerid"] = rddetails.Attributes["jarvis_responsableunitid"];
            }
        }

        /// <summary>
        /// LimitOutValidation
        /// </summary>
        /// <param name="passOut">passout</param>
        /// <param name="passoutImg">passout</param>
        /// <param name="incident">passout</param>
        /// <param name="service">Orgservice</param>
        internal void LimitOutValidation(Entity passOut, Entity passoutImg, Entity parentCase, IOrganizationService service)
        {
            EntityReference passoutCurrency = new EntityReference();
            EntityReference caseAvailableCurrency = new EntityReference();
            if (passOut.Attributes.Contains("transactioncurrencyid"))
            {
                passoutCurrency = (EntityReference)passOut.Attributes["transactioncurrencyid"];
            }
            else if (passoutImg.Attributes.Contains("transactioncurrencyid"))
            {
                passoutCurrency = (EntityReference)passoutImg.Attributes["transactioncurrencyid"];
            }

            //throw new InvalidPluginExecutionException("Not enough GOP available Amount");
            // Passout Validation for GOP Limit out Exceeding
            if (parentCase.Attributes.Contains("jarvis_restgoplimitout") && parentCase.Attributes["jarvis_restgoplimitout"] != null && parentCase.Attributes.Contains("jarvis_totalrestcurrencyout") && parentCase.Attributes["jarvis_totalrestcurrencyout"] != null
                && (passOut.Attributes.Contains("jarvis_goplimitout") || passOut.Attributes.Contains("transactioncurrencyid")) && passoutCurrency != null)
            {
                Money passoutTotalOutput = (Money)passOut.Attributes["jarvis_goplimitout"];
                caseAvailableCurrency = (EntityReference)parentCase.Attributes["jarvis_totalrestcurrencyout"];
                decimal caseAvailableAmount = (decimal)parentCase.Attributes["jarvis_restgoplimitout"];
                var exchangeValue = this.CurrencyExchange(caseAvailableCurrency.Id, passoutCurrency.Id, service);
                caseAvailableAmount = caseAvailableAmount * exchangeValue;
                if (passoutTotalOutput.Value > caseAvailableAmount)
                {
                    throw new InvalidPluginExecutionException("Not enough GOP available Amount");
                }
            }
        }

        /// <summary>
        /// CurrencyExchange.
        /// </summary>
        /// <param name="sourceCurrencyId">SourceCurrencyId.</param>
        /// <param name="targetCurrencyId">TargetCurrencyId.</param>
        /// <param name="service">service.</param>
        /// <returns>ExchangeRate.</returns>
        internal decimal CurrencyExchange(Guid sourceCurrencyId, Guid targetCurrencyId, IOrganizationService service)
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
    }
}
