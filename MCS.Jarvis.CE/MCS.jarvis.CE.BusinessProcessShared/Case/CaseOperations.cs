// <copyright file="CaseOperations.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MCS.Jarvis.CE.BusinessProcessesShared
{
    using System;
    using System.Linq;
    using System.Runtime.Remoting.Services;
    using MCS.Jarvis.CE.BusinessProcessesShared.Helpers;
    using MCS.Jarvis.CE.Commons;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;

    /// <summary>
    /// Case Operations.
    /// </summary>
    public class CaseOperations
    {
        /// <summary>
        /// Update Case Location.
        /// </summary>
        /// <param name="orgService">Org Service.</param>
        /// <param name="tracingService">tracing service.</param>
        /// <param name="targetEntity">target entity.</param>
        /// <param name="caseImg">case Image.</param>
        /// <returns>Entity details.</returns>
        public Entity UpdateCaseLocationAndGOPCurrency(IOrganizationService orgService, ITracingService tracingService, Entity targetEntity, Entity caseImg)
        {
            EntityReference breakDownCountryRef = new EntityReference();
            EntityReference homeDealerRef = new EntityReference();
            bool conditionCheck = false;
            if (targetEntity.Attributes.Contains(Case.Attributes.BreakdownCountry) && targetEntity.Attributes[Case.Attributes.BreakdownCountry] != null)
            {
                breakDownCountryRef = (EntityReference)targetEntity.Attributes[Case.Attributes.BreakdownCountry];
                Entity caseObj = orgService.Retrieve("jarvis_country", breakDownCountryRef.Id, new ColumnSet("transactioncurrencyid"));

                if (caseObj.Attributes.Contains("transactioncurrencyid") && caseObj["transactioncurrencyid"] != null)
                {
                    var currencyout = (EntityReference)caseObj["transactioncurrencyid"];

                    // trigger Case Available amount
                    this.TriggerCaseGOPcalculation(targetEntity, caseImg, orgService, "jarvis_totalrestcurrencyout", "jarvis_restgoplimitout", currencyout);

                    // trigger Case Total GOP limit out amount
                    this.TriggerCaseGOPcalculation(targetEntity, caseImg, orgService, "jarvis_totalcurrencyout", "jarvis_totalgoplimitout", currencyout);

                    // trigger Case Total GOP limit out amount
                    this.TriggerCaseGOPcalculation(targetEntity, caseImg, orgService, "jarvis_totalgoplimitoutapprovedcurrency", "jarvis_totalgoplimitoutapproved", currencyout);

                    // trigger Case Total Credit Card Requested Amount 
                    this.TriggerCaseGOPcalculation(targetEntity, caseImg, orgService, "jarvis_totalcreditcardrequestedamountcurreny", "jarvis_totalrequestedccamount", currencyout);
                }

                conditionCheck = true;
            }
            else if (caseImg.Attributes.Contains(Case.Attributes.BreakdownCountry) && caseImg.Attributes[Case.Attributes.BreakdownCountry] != null)
            {
                breakDownCountryRef = (EntityReference)caseImg.Attributes[Case.Attributes.BreakdownCountry];
                conditionCheck = true;
            }

            if (targetEntity.Attributes.Contains(Case.Attributes.HomeDealer) && targetEntity.Attributes[Case.Attributes.HomeDealer] != null)
            {
                homeDealerRef = (EntityReference)targetEntity.Attributes[Case.Attributes.HomeDealer];
                conditionCheck = conditionCheck && true;
            }
            else if (caseImg.Attributes.Contains(Case.Attributes.HomeDealer) && caseImg.Attributes[Case.Attributes.HomeDealer] != null)
            {
                homeDealerRef = (EntityReference)caseImg.Attributes[Case.Attributes.HomeDealer];
                conditionCheck = conditionCheck && true;
            }

            if (conditionCheck)
            {
                Entity homeDealer = orgService.Retrieve(homeDealerRef.LogicalName, homeDealerRef.Id, new ColumnSet(new string[] { BusinessPartner.Atrributes.BusinessPartnerCountry }));

                EntityReference homeDealerCountryRef = homeDealer.Attributes.Contains(BusinessPartner.Atrributes.BusinessPartnerCountry) ? (EntityReference)homeDealer.Attributes[BusinessPartner.Atrributes.BusinessPartnerCountry] : null;

                if (homeDealerCountryRef != null)
                {
                    // Set Total requested IN GOP Currency
                    Entity hDcurrency = orgService.Retrieve("jarvis_country", homeDealerCountryRef.Id, new ColumnSet("transactioncurrencyid"));
                    if (hDcurrency.Attributes.Contains("transactioncurrencyid") && hDcurrency["transactioncurrencyid"] != null)
                    {
                        var currencyout = (EntityReference)hDcurrency["transactioncurrencyid"];
                        // trigger Case Total requested IN amount
                        this.TriggerCaseGOPcalculation(targetEntity, caseImg, orgService, "jarvis_totalcurrencyin", "jarvis_totalgoplimitin", currencyout);
                    }

                    if (breakDownCountryRef.Id == homeDealerCountryRef.Id)
                    {
                        targetEntity.Attributes[Case.Attributes.Location] = new OptionSetValue((int)Case.CaseLocation.National);
                    }
                    else
                    {
                        Entity breakDownRegion = this.GetRegionBasedOnCountry(orgService, tracingService, breakDownCountryRef.Id);

                        Entity dealerRegion = this.GetRegionBasedOnCountry(orgService, tracingService, homeDealerCountryRef.Id);

                        if (breakDownRegion != null && dealerRegion != null)
                        {
                            string breakDownRegionId = breakDownRegion.Attributes.Contains("jarvis_regionid") ? Convert.ToString(breakDownRegion.Attributes["jarvis_regionid"]) : string.Empty;

                            string dealerRegionId = dealerRegion.Attributes.Contains("jarvis_regionid") ? Convert.ToString(dealerRegion.Attributes["jarvis_regionid"]) : string.Empty;

                            if (!string.IsNullOrEmpty(breakDownRegionId) && !string.IsNullOrEmpty(dealerRegionId) && breakDownRegionId == dealerRegionId)
                            {
                                targetEntity.Attributes[Case.Attributes.Location] = new OptionSetValue((int)Case.CaseLocation.Regional);
                            }
                            else
                            {
                                targetEntity.Attributes[Case.Attributes.Location] = new OptionSetValue((int)Case.CaseLocation.International);
                            }
                        }
                        else
                        {
                            targetEntity.Attributes[Case.Attributes.Location] = new OptionSetValue((int)Case.CaseLocation.International);
                        }
                    }
                }
            }

            return targetEntity;
        }

        /// <summary>
        /// Get Region Based On Country.
        /// </summary>
        /// <param name="service">Org Service.</param>
        /// <param name="tracingService">tracing service.</param>
        /// <param name="countryId">country id.</param>
        /// <returns>entity details.</returns>
        public Entity GetRegionBasedOnCountry(IOrganizationService service, ITracingService tracingService, Guid countryId)
        {
            string regionFetchXml = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>
                                          <entity name='jarvis_region'>
                                            <attribute name='jarvis_regionid' />
                                            <attribute name='jarvis_name' />
                                            <attribute name='createdon' />
                                            <order attribute='jarvis_name' descending='false' />
                                            <link-entity name='jarvis_region_jarvis_country' from='jarvis_regionid' to='jarvis_regionid' visible='false' intersect='true'>
                                              <link-entity name='jarvis_country' from='jarvis_countryid' to='jarvis_countryid' alias='ac'>
                                                <filter type='and'>
                                                  <condition attribute='jarvis_countryid' operator='eq'  uitype='jarvis_country' value='{0}' />
                                                </filter>
                                              </link-entity>
                                            </link-entity>
                                          </entity>
                                        </fetch>";

            EntityCollection regionCollection = service.RetrieveMultiple(new FetchExpression(string.Format(regionFetchXml, countryId)));

            if (regionCollection != null && regionCollection.Entities != null && regionCollection.Entities.Count > 0)
            {
                return regionCollection.Entities.FirstOrDefault();
            }

            return null;
        }

        /// <summary>
        /// Currency Exchange.
        /// </summary>
        /// <param name="sourceCurrencyId">Source Currency Id.</param>
        /// <param name="targetCurrencyId">Target Currency Id.</param>
        /// <param name="service">Org Service.</param>
        /// <returns>decimal value.</returns>
        public decimal CurrencyExchange(Guid sourceCurrencyId, Guid targetCurrencyId, IOrganizationService service)
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
        /// Trigger Case GOP calculation.
        /// </summary>
        /// <param name="targetEntity">target Entity.</param>
        /// <param name="caseImg">Case Image.</param>
        /// <param name="service">Org Service.</param>
        /// <param name="currencyField">currency Field.</param>
        /// <param name="amountField">amount Field.</param>
        /// <param name="currencyout">currency out.</param>
        public void TriggerCaseGOPcalculation(Entity targetEntity, Entity caseImg, IOrganizationService service, string currencyField, string amountField, EntityReference currencyout)
        {
            decimal limitout = 0;
            if (targetEntity.Attributes.Contains(amountField) && targetEntity.Attributes[amountField] != null)
            {
                limitout = (decimal)targetEntity.Attributes[amountField];
            }
            else if (caseImg.Attributes.Contains(amountField) && caseImg.Attributes[amountField] != null)
            {
                limitout = (decimal)caseImg.Attributes[amountField];
            }

            EntityReference totalrestoutcurrency = new EntityReference();
            if (targetEntity.Attributes.Contains(currencyField) && targetEntity.Attributes[currencyField] != null)
            {
                totalrestoutcurrency = (EntityReference)targetEntity.Attributes[currencyField];
            }
            else if (caseImg.Attributes.Contains(currencyField) && caseImg.Attributes[currencyField] != null)
            {
                totalrestoutcurrency = (EntityReference)caseImg.Attributes[currencyField];
            }

            if (limitout > 0 && !totalrestoutcurrency.Id.Equals(currencyout.Id))
            {
                var outexchangeValue = this.CurrencyExchange(totalrestoutcurrency.Id, currencyout.Id, service);
                targetEntity.Attributes[amountField] = limitout * outexchangeValue;
            }

            targetEntity.Attributes[currencyField] = currencyout;
        }

        /// <summary>
        /// Set Customer Informed if :
        /// at least one jarvis job end details status reason = "has been sent" for each pass out
        /// AND all email activity are “closed”
        /// AND “Case Status” = “Case Closure”.
        /// </summary>
        /// <param name="orgService">service Instance.</param>
        /// <param name="tracingService">Tracing instance.</param>
        /// <param name="targetEntity">Current entity.</param>
        /// <param name="caseStatusCode">Case BPF Status.</param>
        /// <param name="releaseConfigValue">Release Case Configuration.</param>
        public void SetCustomerInformed(IOrganizationService orgService, ITracingService tracingService, Entity targetEntity, OptionSetValue caseStatusCode, int releaseConfigValue)
        {
            tracingService.Trace("Statuscode value is " + caseStatusCode.Value.ToString() + " ");

            // Case Closure
            if (caseStatusCode.Value == 90)
            {
                if (releaseConfigValue == 3)
                {
                    // CaseActivePassouts
                    bool isJEDopen = false;
                    EntityCollection passouts = orgService.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.CaseActivePassouts, targetEntity.Id)));
                    if (passouts.Entities.Count > 0)
                    {
                        tracingService.Trace("passouts value is " + passouts.Entities.Count.ToString() + " ");
                        foreach (var item in passouts.Entities)
                        {
                            EntityCollection jed = orgService.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getHasBeenSentJEDs, targetEntity.Id, item.Id)));
                            tracingService.Trace("jed value is " + jed.Entities.Count.ToString() + " ");
                            if (jed.Entities.Count == 0)
                            {
                                isJEDopen = true;
                            }
                        }
                    }

                    EntityCollection uncompletedEmails = orgService.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getCaseEmailsNotCompleted, targetEntity.Id)));
                    if (isJEDopen == false && uncompletedEmails.Entities.Count <= 0)
                    {
                        tracingService.Trace("CI is True");
                        targetEntity["jarvis_customerinformed"] = true;
                    }
                }
            }
        }

        /// <summary>
        /// Create Case Soft Offer.
        /// </summary>
        /// <param name="orgService">org Service.</param>
        /// <param name="incident">incident details.</param>
        /// <param name="softOffer">soft Offer.</param>
        public void CreateCaseSoftOffer(IOrganizationService orgService, EntityReference incident, Entity softOffer)
        {
            // Case Soft Offer.
            Entity caseSoftOffer = new Entity("jarvis_casesoftoffer");
            caseSoftOffer["jarvis_case"] = incident;
            if (softOffer.Attributes.Contains("jarvis_name") && softOffer.Attributes["jarvis_name"] != null)
            {
                caseSoftOffer["jarvis_name"] = (string)softOffer.Attributes["jarvis_name"];
            }

            if (softOffer.Attributes.Contains("jarvis_contractno") && softOffer.Attributes["jarvis_contractno"] != null)
            {
                caseSoftOffer["jarvis_contractno"] = (string)softOffer.Attributes["jarvis_contractno"];
            }

            if (softOffer.Attributes.Contains("jarvis_description") && softOffer.Attributes["jarvis_description"] != null)
            {
                caseSoftOffer["jarvis_description"] = (string)softOffer.Attributes["jarvis_description"];
            }

            if (softOffer.Attributes.Contains("jarvis_marketcode") && softOffer.Attributes["jarvis_marketcode"] != null)
            {
                caseSoftOffer["jarvis_marketcode"] = (string)softOffer.Attributes["jarvis_marketcode"];
            }

            if (softOffer.Attributes.Contains("jarvis_softoffercode") && softOffer.Attributes["jarvis_softoffercode"] != null)
            {
                caseSoftOffer["jarvis_softoffercodelookup"] = (EntityReference)softOffer.Attributes["jarvis_softoffercode"];
                caseSoftOffer["jarvis_softoffercode"] = (EntityReference)softOffer.Attributes["jarvis_softoffercode"];
            }

            if (softOffer.Attributes.Contains("jarvis_startdate") && softOffer.Attributes["jarvis_startdate"] != null)
            {
                caseSoftOffer["jarvis_startdate"] = (DateTime)softOffer.Attributes["jarvis_startdate"];
            }

            if (softOffer.Attributes.Contains("jarvis_expirydate") && softOffer.Attributes["jarvis_expirydate"] != null)
            {
                caseSoftOffer["jarvis_expirydate"] = (DateTime)softOffer.Attributes["jarvis_expirydate"];
            }

            if (softOffer.Attributes.Contains("jarvis_vehicle") && softOffer.Attributes["jarvis_vehicle"] != null)
            {
                caseSoftOffer["jarvis_vehicle"] = (EntityReference)softOffer.Attributes["jarvis_vehicle"];
            }

            orgService.Create(caseSoftOffer);
        }

        /// <summary>
        /// Create Case Progress Indicator.
        /// </summary>
        /// <param name="orgService">Org Service.</param>
        /// <param name="tracingService"> tracing Service.</param>
        /// <param name="incident">Incident details.</param>
        /// <param name="softofferCount">Soft offer Count.</param>
        public void CreateCaseProgressIndicator(IOrganizationService orgService, ITracingService tracingService, EntityReference incident, int softofferCount)
        {
            tracingService.Trace("Started Progress Indicator creation");

            // Soft Offer
            Entity progressIndicator = new Entity("jarvis_casevehicledataintegrationstatus");
            progressIndicator["jarvis_case"] = incident;
            progressIndicator["jarvis_progressstatus"] = new OptionSetValue((int)CaseVehicleDataIntegrationStatus.ProgressStatus.Completed);
            progressIndicator["jarvis_vehiclerecordtype"] = new OptionSetValue((int)CaseVehicleDataIntegrationStatus.VehicleRecordType.SoftOffer);
            progressIndicator["jarvis_countrecord"] = softofferCount.ToString();

            EntityCollection progreeIndicatorCollection = new EntityCollection();
            progreeIndicatorCollection.Entities.Add(progressIndicator);

            // Warranty
            progressIndicator = new Entity("jarvis_casevehicledataintegrationstatus");
            progressIndicator.Attributes["jarvis_case"] = incident;
            progressIndicator["jarvis_progressstatus"] = new OptionSetValue((int)CaseVehicleDataIntegrationStatus.ProgressStatus.NotStarted);
            progressIndicator["jarvis_vehiclerecordtype"] = new OptionSetValue((int)CaseVehicleDataIntegrationStatus.VehicleRecordType.Warranty);
            progreeIndicatorCollection.Entities.Add(progressIndicator);

            // Contract
            progressIndicator = new Entity("jarvis_casevehicledataintegrationstatus");
            progressIndicator.Attributes["jarvis_case"] = incident;
            progressIndicator["jarvis_progressstatus"] = new OptionSetValue((int)CaseVehicleDataIntegrationStatus.ProgressStatus.NotStarted);
            progressIndicator["jarvis_vehiclerecordtype"] = new OptionSetValue((int)CaseVehicleDataIntegrationStatus.VehicleRecordType.Contract);
            progreeIndicatorCollection.Entities.Add(progressIndicator);

            // Whitelist
            progressIndicator = new Entity("jarvis_casevehicledataintegrationstatus");
            progressIndicator["jarvis_case"] = incident;
            progressIndicator["jarvis_progressstatus"] = new OptionSetValue((int)CaseVehicleDataIntegrationStatus.ProgressStatus.Completed);
            progressIndicator["jarvis_vehiclerecordtype"] = new OptionSetValue((int)CaseVehicleDataIntegrationStatus.VehicleRecordType.WhiteList);
            progressIndicator["jarvis_countrecord"] = "N/A";
            progreeIndicatorCollection.Entities.Add(progressIndicator);

            // Add each entity.
            foreach (var entity in progreeIndicatorCollection.Entities)
            {
                orgService.Create(entity);
            }

            tracingService.Trace("End Progress Indicator creation");
        }

        /// <summary>
        /// Set Updated Dealer.
        /// </summary>
        /// <param name="service">Org service.</param>
        /// <param name="tracingService">Tracing service.</param>
        /// <param name="vehicle">vehicle details.</param>
        /// <param name="vehicleImg">vehicle Img.</param>
        /// <returns>Entity Vehicle.</returns>
        public Entity SetUpdatedDealer(IOrganizationService service, ITracingService tracingService, Entity vehicle, Entity vehicleImg)
        {
            tracingService.Trace("Set Updated Dealer");
            EntityReference dealerNotFound = null;
            if ((vehicle.Attributes.Contains("jarvis_homedealer") && vehicle.Attributes["jarvis_homedealer"] != null) ||
                (vehicleImg.Attributes.Contains("jarvis_homedealer") && vehicleImg.Attributes["jarvis_homedealer"] != null))
            {
                tracingService.Trace("Vehicle contains homedealer");
                EntityReference homeDealer = new EntityReference();
                if (vehicle.Attributes.Contains("jarvis_homedealer") && vehicle.Attributes["jarvis_homedealer"] != null)
                {
                    homeDealer = (EntityReference)vehicle.Attributes["jarvis_homedealer"];
                }
                else if (vehicleImg.Attributes.Contains("jarvis_homedealer") && vehicleImg.Attributes["jarvis_homedealer"] != null)
                {
                    homeDealer = (EntityReference)vehicleImg.Attributes["jarvis_homedealer"];
                }

                Entity account = service.Retrieve(homeDealer.LogicalName, homeDealer.Id, new ColumnSet("jarvis_onecasestatus"));
                if (account.Attributes.Contains("jarvis_onecasestatus") && account.Attributes["jarvis_onecasestatus"] != null)
                {
                    var vasStatusValue = (OptionSetValue)account.Attributes["jarvis_onecasestatus"];
                    if (vasStatusValue.Value != 334030000)
                    {
                        tracingService.Trace("Home dealer is not active");
                        dealerNotFound = this.GetDealerNotFound(service, tracingService);
                        vehicle["jarvis_updatedhomedealer"] = dealerNotFound;
                    }
                    else
                    {
                        tracingService.Trace("set active home dealer");
                        vehicle["jarvis_updatedhomedealer"] = homeDealer;
                    }
                }
                else
                {
                    dealerNotFound = this.GetDealerNotFound(service, tracingService);
                    vehicle["jarvis_updatedhomedealer"] = dealerNotFound;
                }
            }
            else
            {
                dealerNotFound = this.GetDealerNotFound(service, tracingService);
                vehicle["jarvis_updatedhomedealer"] = dealerNotFound;
            }

            return vehicle;
        }

        /// <summary>
        /// Get Dealer Not Found.
        /// </summary>
        /// <param name="service">Org service.</param>
        /// <param name="tracingService">tracing service.</param>
        /// <param name="vehicle">vehicle object.</param>
        /// <returns>updated dealer object.</returns>
        public EntityReference GetDealerNotFound(IOrganizationService service, ITracingService tracingService)
        {
            EntityCollection dealerNotFoundCollection = service.RetrieveMultiple(new FetchExpression(Constants.FetchXmls.GetDealerNotFound));
            EntityReference updatedDealer = new EntityReference();
            if (dealerNotFoundCollection.Entities.Any())
            {
                Entity dealerNotFound = dealerNotFoundCollection.Entities.FirstOrDefault();
                if (dealerNotFound != null)
                {
                    if (dealerNotFound.Attributes.Contains("accountid") && dealerNotFound.Attributes["accountid"] != null)
                    {
                        tracingService.Trace("Get dealer not found");
                        updatedDealer = new EntityReference(dealerNotFound.LogicalName, (Guid)dealerNotFound.Attributes["accountid"]);
                        tracingService.Trace("Return Dealer Not found");
                    }
                }
            }

            return updatedDealer;
        }

        /// <summary>
        /// Validate Case Opening.
        /// </summary>
        /// <param name="service">Org service.</param>
        /// <param name="tracingService">tracing service.</param>
        /// <param name="incident">incident object.</param>
        /// <param name="incidentImg">incident Image object.</param>
        public void ValidateCaseOpening(IOrganizationService service, ITracingService tracingService, Entity incident, Entity incidentImg)
        {
            tracingService.Trace("Check for home dealer");
            if ((incident.Attributes.Contains("jarvis_homedealer") && incident.Attributes["jarvis_homedealer"] != null) ||
                (incidentImg.Attributes.Contains("jarvis_homedealer") && incidentImg.Attributes["jarvis_homedealer"] != null))
            {
                EntityReference homeDealer = null;

                if (incident.Attributes.Contains("jarvis_homedealer") && incident.Attributes["jarvis_homedealer"] != null)
                {
                    homeDealer = (EntityReference)incident.Attributes["jarvis_homedealer"];
                }
                else if (incidentImg.Attributes.Contains("jarvis_homedealer") && incidentImg.Attributes["jarvis_homedealer"] != null)
                {
                    homeDealer = (EntityReference)incidentImg.Attributes["jarvis_homedealer"];
                }

                if (homeDealer != null)
                {
                    tracingService.Trace("Get Home dealer details");
                    Entity account = service.Retrieve(homeDealer.LogicalName, homeDealer.Id, new ColumnSet("jarvis_onecasestatus", "jarvis_accounttype", "jarvis_responsableunitid"));

                    if (account != null)
                    {
                        tracingService.Trace("Validate account details");
                        if (account.Attributes.Contains("jarvis_accounttype") && account.Attributes["jarvis_accounttype"] != null)
                        {
                            tracingService.Trace("Get account type");
                            var accountTypeValue = (OptionSetValue)account.Attributes["jarvis_accounttype"];
                            //// Account type is customer
                            if (accountTypeValue.Value == 334030000)
                            {
                                tracingService.Trace("Account is customer");
                                throw new InvalidPluginExecutionException("Please select a valid HD of type Dealer, Market Company or Partner");
                            }
                        }

                        if (account.Attributes.Contains("jarvis_responsableunitid") && account.Attributes["jarvis_responsableunitid"] != null)
                        {
                            if (account.Attributes["jarvis_responsableunitid"].ToString().ToUpper() == "DUMMY")
                            {
                                tracingService.Trace("Account is dummy");
                                ////Dealer Not Found
                                throw new InvalidPluginExecutionException("Please select an active HD");
                            }
                        }

                        if (account.Attributes.Contains("jarvis_onecasestatus") && account.Attributes["jarvis_onecasestatus"] != null)
                        {
                            var vasStatusValue = (OptionSetValue)account.Attributes["jarvis_onecasestatus"];
                            ////VAS status is not active
                            if (vasStatusValue.Value != 334030000)
                            {
                                tracingService.Trace("Account is inactive");
                                throw new InvalidPluginExecutionException("Please select an active HD");
                            }
                        }
                    }
                }
            }
        }
    }
}