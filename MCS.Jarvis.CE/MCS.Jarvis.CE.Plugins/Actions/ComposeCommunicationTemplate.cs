// <copyright file="ComposeCommunicationTemplate.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace MCS.Jarvis.CE.Plugins.Actions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.ServiceModel;
    using global::Plugins;
    using Microsoft.Crm.Sdk.Messages;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Compose Communication Template.
    /// </summary>
    public class ComposeCommunicationTemplate : PluginBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ComposeCommunicationTemplate"/> class.
        /// ComposeCommunicationTemplate:Constructor.
        /// </summary>
        public ComposeCommunicationTemplate()
            : base(typeof(ComposeCommunicationTemplate))
        {
        }

        /// <summary>
        /// Compose Email Body for Template.
        /// </summary>
        /// <param name="localcontext">local context.</param>
        /// <exception cref="InvalidPluginExecutionException">Exception thrown.</exception>
        public override void ExecuteCrmPlugin(LocalPluginContext localcontext)
        {
            // Extract the tracing service for use in debugging sandboxed plug-ins.
            ITracingService tracingService =
                localcontext.TracingService;

            // Obtain the execution context from the service provider.
            IPluginExecutionContext context = localcontext.PluginExecutionContext;

            string propName = string.Empty;
            ResourcesPayload resourcePayload = new ResourcesPayload();
            try
            {
                if (context.InputParameters.Contains("Operation") && context.InputParameters["Operation"] != null)
                {
                    IOrganizationService service = localcontext.OrganizationService;
                    int operation = (int)context.InputParameters["Operation"];

                    // PlaceHolder
                    if (operation == 1)
                    {
#pragma warning disable SA1123 // Do not place regions within elements
                        #region PlaceHolder

                        if (context.InputParameters.Contains("emailBodyInput") && context.InputParameters["emailBodyInput"] != null)
                        {
                            string emailBodyInput = (string)context.InputParameters["emailBodyInput"];
                            string output = (string)context.InputParameters["emailBodyInput"];
                            if (context.InputParameters.Contains("contactList") && context.InputParameters["contactList"] != null)
                            {
                                string language = (string)context.InputParameters["contactList"];
                                string incident = (string)context.InputParameters["Incident"];

                                if (context.InputParameters.Contains("composedInput") && context.InputParameters["composedInput"] != null)
                                {
                                    string composedInput = (string)context.InputParameters["composedInput"];
                                    JObject gopTemplate = JObject.Parse(composedInput);

                                    if (context.InputParameters.Contains("placeHolderInput") && context.InputParameters["placeHolderInput"] != null)
                                    {
                                        string placeHolderInput = (string)context.InputParameters["placeHolderInput"];
                                        JArray placeHolderTemplate = JArray.Parse(placeHolderInput);
                                        Guid caseId = new Guid(incident);
                                        Entity parentCase = service.Retrieve("incident", caseId, new ColumnSet("jarvis_timezone", "jarvis_timezonelabel", "createdon", "jarvis_vehicle"));
                                        int timeZoneCode = 105;
                                        string timeZoneLabel = "(GMT+01:00)";
                                        DateTime jedCreatedOn = DateTime.Now;
                                        DateTime cccCreatedOn = DateTime.Now;
                                        DateTime gopModifiedOn = DateTime.Now;
                                        DateTime vehicleDeliveryDate = DateTime.Now;
                                        DateTime caseCreatedOn;
                                        if (context.InputParameters.Contains("DealerTimeZone") && context.InputParameters["DealerTimeZone"] != null)
                                        {
                                            timeZoneCode = (int)context.InputParameters["DealerTimeZone"];
                                            Entity timeZone = this.GetTimezoneByCode(service, tracingService, timeZoneCode);
                                            if (timeZone != null && timeZone.Attributes.Contains("userinterfacename") && timeZone.Attributes["userinterfacename"] != null)
                                            {
                                                var userInterfaceName = timeZone.Attributes["userinterfacename"];
                                                timeZoneLabel = userInterfaceName.ToString().Substring(0, 11);
                                            }
                                        }

                                        caseCreatedOn = this.RetrieveLocalTimeFromUTCTime(service, (DateTime)parentCase.Attributes["createdon"], timeZoneCode);
#pragma warning disable SA1123 // Do not place regions within elements
                                        #region Vehicle
                                        if (parentCase.Attributes.Contains("jarvis_vehicle") && parentCase.Attributes["jarvis_vehicle"] != null)
                                        {
                                            EntityReference caseVehicle = (EntityReference)parentCase.Attributes["jarvis_vehicle"];
                                            Entity vehicle = service.Retrieve(caseVehicle.LogicalName, caseVehicle.Id, new ColumnSet("jarvis_deliverydate"));
                                            if (vehicle.Attributes.Contains("jarvis_deliverydate") && vehicle.Attributes["jarvis_deliverydate"] != null)
                                            {
                                                vehicleDeliveryDate = (DateTime)vehicle.Attributes["jarvis_deliverydate"];
                                            }
                                        }
                                        #endregion
#pragma warning disable SA1123 // Do not place regions within elements
                                        #region Job EndTranslation

                                        if (context.InputParameters.Contains("jobenddetailsID") && context.InputParameters["jobenddetailsID"] != null)
                                        {
                                            string jobenddetailsID = (string)context.InputParameters["jobenddetailsID"];
                                            Guid jedID = new Guid(jobenddetailsID);
                                            Entity jobEndDetail = service.Retrieve("jarvis_jobenddetails", jedID, new ColumnSet("createdon"));
                                            jedCreatedOn = (DateTime)jobEndDetail.Attributes["createdon"];
                                            if (parentCase.Attributes.Contains("jarvis_timezone") && parentCase.Attributes["jarvis_timezone"] != null)
                                            {
                                                timeZoneCode = (int)parentCase.Attributes["jarvis_timezone"];
                                            }

                                            if (parentCase.Attributes.Contains("jarvis_timezonelabel") && parentCase.Attributes["jarvis_timezonelabel"] != null)
                                            {
                                                timeZoneLabel = (string)parentCase.Attributes["jarvis_timezonelabel"];
                                            }

                                            jedCreatedOn = this.RetrieveLocalTimeFromUTCTime(service, jedCreatedOn, timeZoneCode);
                                            EntityCollection jobtranslationCollection = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getJEDTranslations, incident, jobenddetailsID, language)));
                                            if (jobtranslationCollection != null && jobtranslationCollection.Entities.Count == 1)
                                            {
                                                if (jobtranslationCollection.Entities[0].Attributes.Contains("jarvis_actualcausefault") && jobtranslationCollection.Entities[0].Attributes["jarvis_actualcausefault"] != null)
                                                {
                                                    output = output.Replace("jarvis_actualcausefault", (string)jobtranslationCollection.Entities[0].Attributes["jarvis_actualcausefault"]);
                                                }
                                                else
                                                {
                                                    output = output.Replace("jarvis_actualcausefault", string.Empty);
                                                }

                                                if (jobtranslationCollection.Entities[0].Attributes.Contains("jarvis_warrantyreason") && jobtranslationCollection.Entities[0].Attributes["jarvis_warrantyreason"] != null)
                                                {
                                                    output = output.Replace("jarvis_warrantyreason", (string)jobtranslationCollection.Entities[0].Attributes["jarvis_warrantyreason"]);
                                                }
                                                else
                                                {
                                                    output = output.Replace("jarvis_warrantyreason", string.Empty);
                                                }

                                                if (jobtranslationCollection.Entities[0].Attributes.Contains("jarvis_temporaryrepair") && jobtranslationCollection.Entities[0].Attributes["jarvis_temporaryrepair"] != null)
                                                {
                                                    output = output.Replace("jarvis_temporaryrepair", (string)jobtranslationCollection.Entities[0].Attributes["jarvis_temporaryrepair"]);
                                                }
                                                else
                                                {
                                                    output = output.Replace("jarvis_temporaryrepair", string.Empty);
                                                }

                                                if (jobtranslationCollection.Entities[0].Attributes.Contains("jarvis_comment") && jobtranslationCollection.Entities[0].Attributes["jarvis_comment"] != null)
                                                {
                                                    output = output.Replace("jarvis_partsinfo", (string)jobtranslationCollection.Entities[0].Attributes["jarvis_comment"]);
                                                }
                                                else
                                                {
                                                    output = output.Replace("jarvis_partsinfo", string.Empty);
                                                }
                                            }
                                            else
                                            {
                                                Entity jobeenddetail = service.Retrieve("jarvis_jobenddetails", new Guid(jobenddetailsID), new ColumnSet("jarvis_warrantyreason", "jarvis_temporaryrepair", "jarvis_comment", "jarvis_actualcausefault"));
                                                if (jobeenddetail.Attributes.Contains("description") && jobeenddetail.Attributes["description"] != null)
                                                {
                                                    output = output.Replace("jarvis_warrantyreason", (string)jobeenddetail.Attributes["jarvis_warrantyreason"]);
                                                }
                                                else
                                                {
                                                    output = output.Replace("jarvis_warrantyreason", string.Empty);
                                                }

                                                if (jobeenddetail.Attributes.Contains("jarvis_temporaryrepair") && jobeenddetail.Attributes["jarvis_temporaryrepair"] != null)
                                                {
                                                    output = output.Replace("jarvis_temporaryrepair", (string)jobeenddetail.Attributes["jarvis_temporaryrepair"]);
                                                }
                                                else
                                                {
                                                    output = output.Replace("jarvis_temporaryrepair", string.Empty);
                                                }

                                                if (jobeenddetail.Attributes.Contains("jarvis_comment") && jobeenddetail.Attributes["jarvis_comment"] != null)
                                                {
                                                    output = output.Replace("jarvis_partsinfo", (string)jobeenddetail.Attributes["jarvis_comment"]);
                                                }
                                                else
                                                {
                                                    output = output.Replace("jarvis_partsinfo", string.Empty);
                                                }

                                                if (jobeenddetail.Attributes.Contains("jarvis_actualcausefault") && jobeenddetail.Attributes["jarvis_actualcausefault"] != null)
                                                {
                                                    output = output.Replace("jarvis_actualcausefault", (string)jobeenddetail.Attributes["jarvis_actualcausefault"]);
                                                }
                                                else
                                                {
                                                    output = output.Replace("jarvis_actualcausefault", string.Empty);
                                                }
                                            }
                                        }
#pragma warning restore SA1123 // Do not place regions within elements

                                        #endregion

#pragma warning disable SA1123 // Do not place regions within elements
                                        #region CCC
                                        if (context.InputParameters.Contains("cccID") && context.InputParameters["cccID"] != null)
                                        {
                                            string cccID = (string)context.InputParameters["cccID"];
                                            Guid customerCommitmentCodeID = new Guid(cccID);
                                            Entity jobEndDetail = service.Retrieve("jarvis_caseobservationcode", customerCommitmentCodeID, new ColumnSet("createdon"));
                                            cccCreatedOn = (DateTime)jobEndDetail.Attributes["createdon"];
                                            cccCreatedOn = this.RetrieveLocalTimeFromUTCTime(service, cccCreatedOn, timeZoneCode);
                                        }
                                        #endregion

#pragma warning disable SA1123 // Do not place regions within elements
                                        #region GOP Modified
                                        if (context.InputParameters.Contains("gopID") && context.InputParameters["gopID"] != null)
                                        {
                                            string gop = (string)context.InputParameters["gopID"];
                                            Guid gopID = new Guid(gop);
                                            Entity jobEndDetail = service.Retrieve("jarvis_gop", gopID, new ColumnSet("modifiedon"));
                                            gopModifiedOn = (DateTime)jobEndDetail.Attributes["modifiedon"];
                                            gopModifiedOn = this.RetrieveLocalTimeFromUTCTime(service, gopModifiedOn, timeZoneCode);
                                        }
                                        #endregion

                                        foreach (JObject item in placeHolderTemplate)
                                        {
                                            if (item.TryGetValue("jarvis_placeholder", StringComparison.OrdinalIgnoreCase, out JToken placeholder) && placeholder != null &&
                                                item.TryGetValue("jarvis_placeholdersource", StringComparison.OrdinalIgnoreCase, out JToken source) && source != null)
                                            {
                                                if (output.Contains(placeholder.ToString()))
                                                {
                                                    if (gopTemplate.SelectToken(source.ToString()) != null)
                                                    {
                                                        propName = placeholder.ToString();

                                                        // throw new InvalidPluginExecutionException(placeholder.ToString() + gopTemplate.SelectToken(source.ToString()).Value<string>());
                                                        if (placeholder.ToString() == "casecreatedon")
                                                        {
                                                            output = output.Replace(placeholder.ToString(), caseCreatedOn.ToString("yyyy-MM-dd HH:mm") + " " + timeZoneLabel);
                                                        }

                                                        if (placeholder.ToString() == "JEDCreatedOn")
                                                        {
                                                            output = output.Replace(placeholder.ToString(), jedCreatedOn.ToString("yyyy-MM-dd HH:mm") + " " + timeZoneLabel);
                                                        }

                                                        if (placeholder.ToString() == "jarvis_observationcode.createdon")
                                                        {
                                                            output = output.Replace(placeholder.ToString(), cccCreatedOn.ToString("yyyy-MM-dd HH:mm") + " " + timeZoneLabel);
                                                        }

                                                        if (placeholder.ToString() == "jarvis_gop.modifiedon")
                                                        {
                                                            output = output.Replace(placeholder.ToString(), gopModifiedOn.ToString("yyyy-MM-dd HH:mm") + " " + timeZoneLabel);
                                                        }

                                                        if (placeholder.ToString() == "jarvis_gop.jarvis_case.jarvis_vehicle.jarvis_deliverydate")
                                                        {
                                                            output = output.Replace(placeholder.ToString(), vehicleDeliveryDate.ToString("yyyy-MM-dd HH:mm") + " " + timeZoneLabel);
                                                        }

                                                        output = output.Replace(placeholder.ToString(), gopTemplate.SelectToken(source.ToString()).Value<string>());
                                                    }
                                                    else
                                                    {
                                                        output = output.Replace(placeholder.ToString(), " ");
                                                    }
                                                }
                                            }
                                        }

#pragma warning disable SA1123 // Do not place regions within elements
                                        #region Translation

                                        EntityCollection translationCollection = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getCaseTranslation, incident, language)));
#pragma warning restore SA1123 // Do not place regions within elements
                                        if (translationCollection != null && translationCollection.Entities.Count == 1)
                                        {
                                            //// 611701 US Remvoing Translation location in Template.
                                            ////if (translationCollection.Entities[0].Attributes.Contains("jarvis_location") && translationCollection.Entities[0].Attributes["jarvis_location"] != null)
                                            ////{
                                            ////    output = output.Replace("jarvis_location", (string)translationCollection.Entities[0].Attributes["jarvis_location"]);
                                            ////}
                                            ////else
                                            ////{
                                            ////    output = output.Replace("jarvis_location", string.Empty);
                                            ////}

                                            if (translationCollection.Entities[0].Attributes.Contains("jarvis_description") && translationCollection.Entities[0].Attributes["jarvis_description"] != null)
                                            {
                                                output = output.Replace("jarvis_reportedfault", (string)translationCollection.Entities[0].Attributes["jarvis_description"]);
                                            }
                                            else
                                            {
                                                output = output.Replace("jarvis_reportedfault", string.Empty);
                                            }

                                            if (translationCollection.Entities[0].Attributes.Contains("jarvis_customerexpectations") && translationCollection.Entities[0].Attributes["jarvis_customerexpectations"] != null)
                                            {
                                                output = output.Replace("jarvis_customerexpectations", (string)translationCollection.Entities[0].Attributes["jarvis_customerexpectations"]);
                                            }
                                            else
                                            {
                                                output = output.Replace("jarvis_customerexpectations", string.Empty);
                                            }
                                        }
                                        else
                                        {
                                            Entity incidentObj = service.Retrieve("incident", new Guid(incident), new ColumnSet("description", "jarvis_location", "jarvis_customerexpectations"));
                                            if (incidentObj.Attributes.Contains("description") && incidentObj.Attributes["description"] != null)
                                            {
                                                output = output.Replace("jarvis_reportedfault", (string)incidentObj.Attributes["description"]);
                                            }
                                            else
                                            {
                                                output = output.Replace("jarvis_reportedfault", string.Empty);
                                            }

                                            if (incidentObj.Attributes.Contains("jarvis_location") && incidentObj.Attributes["jarvis_location"] != null)
                                            {
                                                output = output.Replace("jarvis_location", (string)incidentObj.Attributes["jarvis_location"]);
                                            }
                                            else
                                            {
                                                output = output.Replace("jarvis_location", string.Empty);
                                            }

                                            if (incidentObj.Attributes.Contains("jarvis_customerexpectations") && incidentObj.Attributes["jarvis_customerexpectations"] != null)
                                            {
                                                output = output.Replace("jarvis_customerexpectations", (string)incidentObj.Attributes["jarvis_customerexpectations"]);
                                            }
                                            else
                                            {
                                                output = output.Replace("jarvis_customerexpectations", string.Empty);
                                            }
                                        }
                                        #endregion

#pragma warning disable SA1123 // Do not place regions within elements
                                        #region GOP

                                        EntityCollection gopCollection = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.fetchGOP, incident)));
#pragma warning restore SA1123 // Do not place regions within elements
                                        if (gopCollection != null && gopCollection.Entities.Count > 0)
                                        {
                                            if (gopCollection.Entities[0].Attributes.Contains("jarvis_dealer") && gopCollection.Entities[0].Attributes["jarvis_dealer"] != null)
                                            {
                                                EntityReference dealer = (EntityReference)gopCollection.Entities[0].Attributes["jarvis_dealer"];
                                                Entity customer = service.Retrieve(dealer.LogicalName, dealer.Id, new ColumnSet("name", "address1_city", "address1_line1", "address1_postalcode", "jarvis_address1_country", "jarvis_vatid"));
                                                if (customer.Attributes.Contains("name"))
                                                {
                                                    output = output.Replace("GOPDealerName", (string)customer.Attributes["name"]);
                                                }
                                                else
                                                {
                                                    output = output.Replace("GOPDealerName", " ");
                                                }

                                                if (customer.Attributes.Contains("address1_city"))
                                                {
                                                    output = output.Replace("GOPDealerTown", (string)customer.Attributes["address1_city"]);
                                                }
                                                else
                                                {
                                                    output = output.Replace("GOPDealerTown", " ");
                                                }

                                                if (customer.Attributes.Contains("address1_line1"))
                                                {
                                                    output = output.Replace("GOPDealerAddress", (string)customer.Attributes["address1_line1"]);
                                                }
                                                else
                                                {
                                                    output = output.Replace("GOPDealerAddress", " ");
                                                }

                                                if (customer.Attributes.Contains("address1_postalcode"))
                                                {
                                                    output = output.Replace("GOPDealerPostal", (string)customer.Attributes["address1_postalcode"]);
                                                }
                                                else
                                                {
                                                    output = output.Replace("GOPDealerPostal", " ");
                                                }

                                                if (customer.Attributes.Contains("jarvis_address1_country"))
                                                {
                                                    output = output.Replace("GOPDealerCountry", ((EntityReference)customer.Attributes["jarvis_address1_country"]).Name);
                                                }
                                                else
                                                {
                                                    output = output.Replace("GOPDealerCountry", " ");
                                                }

                                                if (customer.Attributes.Contains("jarvis_vatid"))
                                                {
                                                    output = output.Replace("GOPDealerVATID", (string)customer.Attributes["jarvis_vatid"]);
                                                }
                                                else
                                                {
                                                    output = output.Replace("GOPDealerVATID", " ");
                                                }
                                            }
                                        }

                                        #endregion

#pragma warning disable SA1123 // Do not place regions within elements
                                        #region Vehicle Contract

                                        EntityCollection contractCollection = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getContracts, incident)));
#pragma warning restore SA1123 // Do not place regions within elements
                                        if (contractCollection != null && contractCollection.Entities.Count > 0)
                                        {
                                            if (contractCollection.Entities[0].Attributes.Contains("jarvis_tsacontracttype"))
                                            {
                                                output = output.Replace("VehicleContractCode", (string)contractCollection.Entities[0].Attributes["jarvis_tsacontracttype"]);
                                            }
                                            else
                                            {
                                                output = output.Replace("VehicleContractCode", " ");
                                            }

                                            if (contractCollection.Entities[0].Attributes.Contains("jarvis_name"))
                                            {
                                                output = output.Replace("VehicleContractName", (string)contractCollection.Entities[0].Attributes["jarvis_name"]);
                                            }
                                            else
                                            {
                                                output = output.Replace("VehicleContractName", " ");
                                            }

                                            if (contractCollection.Entities[0].Attributes.Contains("jarvis_startdate"))
                                            {
                                                ////DateTime vehStartDate = this.RetrieveLocalTimeFromUTCTime(service, (DateTime)contractCollection.Entities[0].Attributes["jarvis_startdate"], timeZoneCode);
                                                output = output.Replace("VehicleContractStartDate", ((DateTime)contractCollection.Entities[0].Attributes["jarvis_startdate"]).ToString("yyyy-MM-dd HH:mm"));
                                            }
                                            else
                                            {
                                                output = output.Replace("VehicleContractStartDate", " ");
                                            }

                                            if (contractCollection.Entities[0].Attributes.Contains("jarvis_expiringdate"))
                                            {
                                                ////DateTime vehExpDate = this.RetrieveLocalTimeFromUTCTime(service, (DateTime)contractCollection.Entities[0].Attributes["jarvis_expiringdate"], timeZoneCode);
                                                output = output.Replace("VehicleContractExpDate", ((DateTime)contractCollection.Entities[0].Attributes["jarvis_expiringdate"]).ToString("yyyy-MM-dd HH:mm"));
                                            }
                                            else
                                            {
                                                output = output.Replace("VehicleContractExpDate", " ");
                                            }

                                            if (contractCollection.Entities[0].Attributes.Contains("jarvis_maxmileage"))
                                            {
                                                output = output.Replace("VehicleContractMaxMileage", ((int)contractCollection.Entities[0].Attributes["jarvis_maxmileage"]).ToString());
                                            }
                                            else
                                            {
                                                output = output.Replace("VehicleContractMaxMileage", " ");
                                            }

                                            if (contractCollection.Entities[0].Attributes.Contains("jarvis_startmileage"))
                                            {
                                                output = output.Replace("VehicleContractStartMileage", ((int)contractCollection.Entities[0].Attributes["jarvis_startmileage"]).ToString());
                                            }
                                            else
                                            {
                                                output = output.Replace("VehicleContractStartMileage", " ");
                                            }
                                        }
                                        else
                                        {
                                            output = output.Replace("VehicleContractCode", " ");
                                            output = output.Replace("VehicleContractName", " ");
                                            output = output.Replace("VehicleContractStartDate", " ");
                                            output = output.Replace("VehicleContractExpDate", " ");
                                            output = output.Replace("VehicleContractMaxMileage", " ");
                                            output = output.Replace("VehicleContractStartMileage", " ");
                                        }

                                        #endregion

#pragma warning disable SA1123 // Do not place regions within elements
                                        #region Vehicle Warranty

                                        EntityCollection warrantyCollection = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getWarranties, incident)));
#pragma warning restore SA1123 // Do not place regions within elements
                                        if (warrantyCollection != null && warrantyCollection.Entities.Count > 0)
                                        {
                                            if (warrantyCollection.Entities[0].Attributes.Contains("jarvis_warrantycode"))
                                            {
                                                output = output.Replace("CaseWarrantyCode", (string)warrantyCollection.Entities[0].Attributes["jarvis_warrantycode"]);
                                            }
                                            else
                                            {
                                                output = output.Replace("CaseWarrantyCode", " ");
                                            }

                                            if (warrantyCollection.Entities[0].Attributes.Contains("jarvis_description"))
                                            {
                                                output = output.Replace("CaseWarrantyDesc", (string)warrantyCollection.Entities[0].Attributes["jarvis_description"]);
                                            }
                                            else
                                            {
                                                output = output.Replace("CaseWarrantyDesc", " ");
                                            }

                                            if (warrantyCollection.Entities[0].Attributes.Contains("jarvis_startdate"))
                                            {
                                                ////DateTime vehStartDate = this.RetrieveLocalTimeFromUTCTime(service, (DateTime)contractCollection.Entities[0].Attributes["jarvis_startdate"], timeZoneCode);
                                                output = output.Replace("CaseWarrantyStartDate", ((DateTime)warrantyCollection.Entities[0].Attributes["jarvis_startdate"]).ToString("yyyy-MM-dd HH:mm"));
                                            }
                                            else
                                            {
                                                output = output.Replace("CaseWarrantyStartDate", " ");
                                            }

                                            if (warrantyCollection.Entities[0].Attributes.Contains("jarvis_expirydate"))
                                            {
                                                ////DateTime vehExpDate = this.RetrieveLocalTimeFromUTCTime(service, (DateTime)contractCollection.Entities[0].Attributes["jarvis_expiringdate"], timeZoneCode);
                                                output = output.Replace("CaseWarrantyExpDate", ((DateTime)warrantyCollection.Entities[0].Attributes["jarvis_expirydate"]).ToString("yyyy-MM-dd HH:mm"));
                                            }
                                            else
                                            {
                                                output = output.Replace("CaseWarrantyExpDate", " ");
                                            }

                                        }
                                        else
                                        {
                                            output = output.Replace("CaseWarrantyCode", " ");
                                            output = output.Replace("CaseWarrantyDesc", " ");
                                            output = output.Replace("CaseWarrantyStartDate", " ");
                                            output = output.Replace("CaseWarrantyExpDate", " ");
                                        }

                                        #endregion

#pragma warning disable SA1123 // Do not place regions within elements
                                        #region Case Soft Offer

                                        EntityCollection caseSoftCollection = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getSoftOffers, incident)));
#pragma warning restore SA1123 // Do not place regions within elements
                                        if (caseSoftCollection != null && caseSoftCollection.Entities.Count > 0)
                                        {
                                            if (caseSoftCollection.Entities[0].Attributes.Contains("jarvis_softoffercodelookup"))
                                            {
                                                EntityReference csofferCode = (EntityReference)caseSoftCollection.Entities[0].Attributes["jarvis_softoffercodelookup"];
                                                Entity softOffer = service.Retrieve(csofferCode.LogicalName, csofferCode.Id, new ColumnSet("jarvis_description"));
                                                if (softOffer.Attributes.Contains("jarvis_description"))
                                                {
                                                    output = output.Replace("CaseSoftOfferDesc", (string)softOffer.Attributes["jarvis_description"]);
                                                }
                                                else
                                                {
                                                    output = output.Replace("CaseSoftOfferDesc", " ");
                                                }

                                                output = output.Replace("CaseSoftOfferCode", ((EntityReference)caseSoftCollection.Entities[0].Attributes["jarvis_softoffercodelookup"]).Name);
                                            }
                                            else
                                            {
                                                output = output.Replace("CaseSoftOfferCode", " ");
                                                output = output.Replace("CaseSoftOfferDesc", " ");
                                            }

                                            if (caseSoftCollection.Entities[0].Attributes.Contains("jarvis_startdate"))
                                            {
                                                ////DateTime caseSoftOfferStartDate = this.RetrieveLocalTimeFromUTCTime(service, (DateTime)caseSoftCollection.Entities[0].Attributes["jarvis_startdate"], timeZoneCode);
                                                output = output.Replace("CaseSoftOfferStartDate", ((DateTime)caseSoftCollection.Entities[0].Attributes["jarvis_startdate"]).ToString("yyyy-MM-dd HH:mm"));
                                            }
                                            else
                                            {
                                                output = output.Replace("CaseSoftOfferStartDate", " ");
                                            }

                                            if (caseSoftCollection.Entities[0].Attributes.Contains("jarvis_expirydate"))
                                            {
                                                ////DateTime caseSoftOfferExpDate = this.RetrieveLocalTimeFromUTCTime(service, (DateTime)caseSoftCollection.Entities[0].Attributes["jarvis_expirydate"], timeZoneCode);
                                                output = output.Replace("CaseSoftOfferExpDate", ((DateTime)caseSoftCollection.Entities[0].Attributes["jarvis_expirydate"]).ToString("yyyy-MM-dd HH:mm"));
                                            }
                                            else
                                            {
                                                output = output.Replace("CaseSoftOfferExpDate", " ");
                                            }
                                        }
                                        else
                                        {
                                            output = output.Replace("CaseSoftOfferCode", " ");
                                            output = output.Replace("CaseSoftOfferDesc", " ");
                                            output = output.Replace("CaseSoftOfferStartDate", " ");
                                            output = output.Replace("CaseSoftOfferExpDate", " ");
                                        }

                                        #endregion
                                    }
                                }

                                context.OutputParameters["emailBodyOutput"] = output;
                            }
                        }
#pragma warning restore SA1123 // Do not place regions within elements

                        #endregion
                    }

                    // Label
                    if (operation == 2)
                    {
#pragma warning disable SA1123 // Do not place regions within elements
                        #region Label

                        if (context.InputParameters.Contains("emailBodyInput") && context.InputParameters["emailBodyInput"] != null)
                        {
                            string emailBodyInput = (string)context.InputParameters["emailBodyInput"];
                            string output = (string)context.InputParameters["emailBodyInput"];
                            if (context.InputParameters.Contains("contactList") && context.InputParameters["contactList"] != null)
                            {
                                string language = (string)context.InputParameters["contactList"];

                                if (context.InputParameters.Contains("composedInput") && context.InputParameters["composedInput"] != null)
                                {
                                    string composedInput = (string)context.InputParameters["composedInput"];
                                    JObject gopTemplate = JObject.Parse(composedInput);

                                    if (context.InputParameters.Contains("placeHolderInput") && context.InputParameters["placeHolderInput"] != null)
                                    {
                                        string placeHolderInput = (string)context.InputParameters["placeHolderInput"];
                                        JArray placeHolderTemplate = JArray.Parse(placeHolderInput);

                                        foreach (JObject item in placeHolderTemplate)
                                        {
                                            if (item.TryGetValue("jarvis_placeholder", StringComparison.OrdinalIgnoreCase, out JToken placeholder) && placeholder != null &&
                                                item.TryGetValue("jarvis_name", StringComparison.OrdinalIgnoreCase, out JToken name) && name != null && item.TryGetValue("jarvis_communicationplaceholderid", StringComparison.OrdinalIgnoreCase, out JToken communicationID) && communicationID != null)
                                            {
                                                if (output.Contains(placeholder.ToString()))
                                                {
                                                    propName = placeholder.ToString();
                                                    EntityCollection labelCollection = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getLabelTranslation, language, communicationID)));
                                                    if (labelCollection != null && labelCollection.Entities.Count != 0)
                                                    {
                                                        if (labelCollection.Entities[0].Attributes.Contains("jarvis_name") && labelCollection.Entities[0].Attributes["jarvis_name"] != null)
                                                        {
                                                            string translatedText = (string)labelCollection.Entities[0].Attributes["jarvis_name"];
                                                            output = output.Replace(placeholder.ToString(), translatedText);
                                                        }
                                                        else
                                                        {
                                                            // throw new InvalidPluginExecutionException(placeholder.ToString() + name.ToString());
                                                            output = output.Replace(placeholder.ToString(), name.ToString());
                                                        }
                                                    }
                                                    else
                                                    {
                                                        output = output.Replace(placeholder.ToString(), name.ToString());
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }

                                context.OutputParameters["emailBodyOutput"] = output;
                            }
                        }
#pragma warning restore SA1123 // Do not place regions within elements

                        #endregion
                    }

                    // Compose Contact list
                    if (operation == 3)
                    {
                        if (context.InputParameters.Contains("contactList") && context.InputParameters["contactList"] != null)
                        {
                            string contactlist = (string)context.InputParameters["contactList"];

                            // string language = (string)context.InputParameters["emailBodyInput"];
                            List<CommunicationPayLoad> data = JsonConvert.DeserializeObject<List<CommunicationPayLoad>>(contactlist);

                            // Group
                            var groupedData = data.GroupBy(p => p.language)
                              .Select(g =>
                              {
                                  string name = string.Join(";", g.Select(p => p.contactid));
                                  return new
                                  {
                                      Email = name,
                                      Language = g.Key,
                                  };
                              })
                              .ToList();

                            string resultJson = JsonConvert.SerializeObject(groupedData, Formatting.Indented);

                            context.OutputParameters["emailBodyOutput"] = resultJson;
                        }
                    }
                }
            }
            catch (InvalidPluginExecutionException oex)
            {
                tracingService.Trace(oex.Message);
                tracingService.Trace(oex.StackTrace);
                throw new InvalidPluginExecutionException(oex.Message + propName);
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
        /// Get Time zone By Code.
        /// </summary>
        /// <param name="service">Org service.</param>
        /// <param name="tracingService">tracing Service.</param>
        /// <param name="timeZoneCode">country Id.</param>
        /// <returns>Exception details.</returns>
        private Entity GetTimezoneByCode(IOrganizationService service, ITracingService tracingService, int timeZoneCode)
        {
            string timeZoneFetchXml = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>
                                          <entity name='timezonedefinition'>
                                            <attribute name='userinterfacename' />
                                                <filter type='and'>
                                                  <condition attribute='timezonecode' operator='eq'  value='{0}' />
                                                </filter>
                                          </entity>
                                        </fetch>";

            EntityCollection timeZoneCollection = service.RetrieveMultiple(new FetchExpression(string.Format(timeZoneFetchXml, timeZoneCode)));
            if (timeZoneCollection != null && timeZoneCollection.Entities != null && timeZoneCollection.Entities.Count > 0)
            {
                return timeZoneCollection.Entities.FirstOrDefault();
            }
            else
            {
                return null;
            }
        }
    }
}
