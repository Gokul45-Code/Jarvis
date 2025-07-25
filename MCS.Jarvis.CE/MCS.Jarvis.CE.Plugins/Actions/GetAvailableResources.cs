// <copyright file="GetAvailableResources.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace MCS.Jarvis.CE.Plugins
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Runtime.Remoting.Services;
    using System.Threading;
    using System.Xml.Linq;
    using Microsoft.Crm.Sdk.Messages;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;
    using Newtonsoft.Json;

    /// <summary>
    /// Get Available Resources.
    /// </summary>
    public class GetAvailableResources : IPlugin
    {
        /// <summary>
        /// Execute Method.
        /// </summary>
        /// <param name="serviceProvider">service Provider.</param>
        public void Execute(IServiceProvider serviceProvider)
        {
            // Extract the tracing service for use in debugging sandboxed plug-ins.
            ITracingService tracingService =
                (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            // Obtain the execution context from the service provider.
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            if (context.InputParameters.Contains("searchRadius") && context.InputParameters["searchRadius"] != null)
            {
                IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = serviceFactory.CreateOrganizationService(null);
                if (context.InputParameters.Contains("caseID") && context.InputParameters["caseID"] != null)
                {
                    Guid caseID = new Guid((string)context.InputParameters["caseID"]);
                    Entity incident = service.Retrieve("incident", caseID, new ColumnSet("jarvis_latitude", "jarvis_longitude"));
                    decimal latitude = 0;
                    decimal longitude = 0;
                    DateTime currentDate = DateTime.Now.Date;
                    DateTime fromDate = currentDate.Add(DateTime.Now.TimeOfDay);
                    DateTime toDate = currentDate.Add(DateTime.Now.TimeOfDay);
                    Guid workOrderID = Guid.Empty;
                    string configurationParams = string.Empty;
                    EntityCollection characteristics = new EntityCollection();
                    EntityCollection getConfig = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getParamsForSearchAPI, Constants.Incident.SearchAPI)));
                    if (getConfig.Entities.Count > 0)
                    {
                        if (getConfig.Entities[0].Attributes.Contains("jarvis_integrationmapping") && getConfig.Entities[0].Attributes["jarvis_integrationmapping"] != null)
                        {
                            configurationParams = (string)getConfig.Entities[0].Attributes["jarvis_integrationmapping"];
                        }
                    }

                    if (incident.Attributes.Contains("jarvis_latitude") && incident.Attributes["jarvis_latitude"] != null)
                    {
                        latitude = (decimal)incident.Attributes["jarvis_latitude"];
                    }

                    if (incident.Attributes.Contains("jarvis_longitude") && incident.Attributes["jarvis_longitude"] != null)
                    {
                        longitude = (decimal)incident.Attributes["jarvis_longitude"];
                    }

                    List<string> resourceCharc = new List<string>();
                    if (context.InputParameters.Contains("Servicelist") && context.InputParameters["Servicelist"] != null)
                    {
                        string serviceCollection = (string)context.InputParameters["Servicelist"];
                        string[] servicelist = serviceCollection.Split(',');
                        for (int i = 0; i < servicelist.Length; i++)
                        {
                            EntityCollection retrieveService = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getServicesForResource, servicelist[i])));
                            if (retrieveService.Entities.Count > 0)
                            {
                                resourceCharc.Add(retrieveService.Entities[0].Id.ToString());
                                characteristics.Entities.Add(retrieveService.Entities[0]);
                            }
                        }

                        tracingService.Trace("resourceCharc:" + resourceCharc.Count.ToString());
                    }
                    else
                    {
                        EntityCollection retriveWO = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getWorkOrders, caseID)));
                        if (retriveWO.Entities.Count > 0)
                        {
                            workOrderID = retriveWO.Entities[0].Id;
                            EntityCollection resReq = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getResourceRequirement, workOrderID)));
                            if (resReq.Entities.Count > 0)
                            {
                                EntityCollection resCharc = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getListofResourceCharacteristics, resReq.Entities[0].Id)));
                                if (resCharc.Entities.Count > 0)
                                {
                                    foreach (var item in resCharc.Entities)
                                    {
                                        if (item.Attributes.Contains("msdyn_characteristic") && item.Attributes["msdyn_characteristic"] != null)
                                        {
                                            resourceCharc.Add(((EntityReference)item.Attributes["msdyn_characteristic"]).Id.ToString());
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (context.InputParameters.Contains("FromDate") && context.InputParameters["FromDate"] != null)
                    {
                        string dateString = (string)context.InputParameters["FromDate"];
                        fromDate = Convert.ToDateTime(dateString).Date;
                        if (context.InputParameters.Contains("FromTime") && context.InputParameters["FromTime"] != null)
                        {
                            string fromTime = (string)context.InputParameters["FromTime"];
                            fromTime = fromTime.Replace(":", string.Empty);
                            string[] substrings = SplitString(fromTime, 2);
                            TimeSpan timeToAdd = new TimeSpan(Convert.ToInt16(substrings[0]), Convert.ToInt16(substrings[1]), 0);
                            fromDate = fromDate.Add(timeToAdd);
                        }
                    }

                    if (context.InputParameters.Contains("ToDate") && context.InputParameters["ToDate"] != null)
                    {
                        string dateString = (string)context.InputParameters["ToDate"];
                        toDate = Convert.ToDateTime(dateString).Date;
                        if (context.InputParameters.Contains("ToTime") && context.InputParameters["ToTime"] != null)
                        {
                            string toTime = (string)context.InputParameters["ToTime"];
                            toTime = toTime.Replace(":", string.Empty);
                            string[] substrings = SplitString(toTime, 2);
                            TimeSpan timeToAddTo = new TimeSpan(Convert.ToInt16(substrings[0]), Convert.ToInt16(substrings[1]), 0);
                            toDate = toDate.Add(timeToAddTo);
                        }
                    }

                    string searchRadius = (string)context.InputParameters["searchRadius"];
                    string results = this.GetResources(searchRadius, latitude, longitude, workOrderID, resourceCharc, configurationParams, characteristics, fromDate, toDate, service, tracingService);
                    context.OutputParameters["resources"] = results;
                }
            }
        }

        /// <summary>
        /// get Resources.
        /// </summary>
        /// <param name="searchRadius">search Radius.</param>
        /// <param name="latitude">latitude param.</param>
        /// <param name="longitude">longitude param.</param>
        /// <param name="workOrderID">work Order ID.</param>
        /// <param name="resourceList">resource List.</param>
        /// <param name="configJSON">config JSON.</param>
        /// <param name="characteristicsCollection">characteristics Collection.</param>
        /// <param name="fromDate">from Date.</param>
        /// <param name="toDate">to Date.</param>
        /// <param name="service">Org service.</param>
        /// <param name="tracing">tracing service.</param>
        /// <returns>Resources Value.</returns>
        public string GetResources(string searchRadius, decimal latitude, decimal longitude, Guid workOrderID, List<string> resourceList, string configJSON, EntityCollection characteristicsCollection, DateTime fromDate, DateTime toDate, IOrganizationService service, ITracingService tracing)
        {
            string results = "[]";
            InputPayLoad inputParam =
            JsonConvert.DeserializeObject<InputPayLoad>(configJSON);
            decimal radius = Convert.ToDecimal(searchRadius);
            Entity settings = new Entity("organization");
            settings["UseRealTimeResourceLocation"] = inputParam.UseRealTimeResourceLocation;
            settings["ConsiderTravelTime"] = inputParam.ConsiderTravelTime;
            settings["ConsiderSlotsWithOverlappingBooking"] = inputParam.ConsiderSlotsWithOverlappingBooking;
            settings["ConsiderSlotsWithLessThanRequiredDuration"] = inputParam.ConsiderSlotsWithLessThanRequiredDuration;
            settings["ConsiderSlotsWithProposedBookings"] = inputParam.ConsiderSlotsWithProposedBookings;
            settings["ConsiderAppointments"] = inputParam.ConsiderAppointments;
            settings["MovePastStartDateToCurrentDate"] = inputParam.MovePastStartDateToCurrentDate;

            // Travel Radius
            Entity travelRadius = new Entity("travelradius");
            travelRadius["Value"] = Convert.ToInt32(searchRadius);
            travelRadius["Unit"] = inputParam.TravelUnit;
            settings["MaxResourceTravelRadius"] = travelRadius;

            // Work Order
            Entity workOrder = new Entity("msdyn_workorder");
            workOrder.Id = workOrderID;
            settings["msdyn_workorder"] = workOrder;

            // ResourceSpecification
            var resourceSpecification = new Entity("organization");
            var constraints = new Entity("organization");
            constraints["Characteristics"] = characteristicsCollection;
            var entityCollectionResourceType = new EntityCollection();
            entityCollectionResourceType.Entities.Add(new Entity
            {
                Id = new Guid(),
                LogicalName = "ResourceTypes",
                Attributes = new AttributeCollection
                {
                   new KeyValuePair<string, object>("value", inputParam.ResourceType),
                },
            });
            resourceSpecification["ResourceTypes"] = entityCollectionResourceType;

            Entity requirement = new Entity("msdyn_resourcerequirement");
            DateTime currentDate = DateTime.Now.Date;
            DateTime startDate = DateTime.Now.Date;
            requirement["msdyn_duration"] = inputParam.Duration;
            requirement["msdyn_longitude"] = Convert.ToDouble(longitude);
            requirement["msdyn_latitude"] = Convert.ToDouble(latitude);
            requirement["msdyn_fromdate"] = fromDate;
            requirement["msdyn_todate"] = toDate;
            requirement["msdyn_timefrompromised"] = fromDate;
            requirement["msdyn_timetopromised"] = toDate;
            requirement["msdyn_remainingduration"] = inputParam.RemainingDuration;
            requirement["msdyn_worklocation"] = new OptionSetValue(inputParam.Worklocation); // Onsite
            requirement["msdyn_effort"] = inputParam.Effort;
            requirement["msdyn_isprimary"] = inputParam.IsPrimary;

            // Execute Request
            var response = service.Execute(new OrganizationRequest("msdyn_SearchResourceAvailability") // msdyn_SearchResourceAvailability
            {
                Parameters = { { "Version", inputParam.Version }, { "Requirement", requirement }, { "Settings", settings }, { "ResourceSpecification", resourceSpecification } },
            });
            EntityCollection timeSlots = (EntityCollection)response.Results["TimeSlots"]; // TimeSlots

            List<ResourcesPayload> payList = new List<ResourcesPayload>();
            if (timeSlots.Entities.Count > 0)
            {
                var timeSlotsOrdered = timeSlots.Entities.OrderBy(ent => ent.GetAttributeValue<Entity>("Travel").GetAttributeValue<double>("Distance"));
                List<string> existingList = new List<string>();
                foreach (var item in timeSlotsOrdered)
                {
                    bool serviceCheck = false;
                    bool isResource = false;
                    List<string> blist = new List<string>();
                    decimal distances = 0;
                    ResourcesPayload pload = new ResourcesPayload();
                    if (item.Attributes.Contains("Resource") && item.Attributes["Resource"] != null)
                    {
                        Entity resource = (Entity)item.Attributes["Resource"];
                        if (resource.Attributes.Contains("Resource") && resource.Attributes["Resource"] != null)
                        {
                            EntityReference resourceGroup = (EntityReference)resource.Attributes["Resource"];
                            pload.name = resourceGroup.Name;
                            pload.resourceID = resourceGroup.Id.ToString();
                            isResource = existingList.Contains(pload.resourceID);
                            if (!isResource)
                            {
                                existingList.Add(pload.resourceID);
                            }

                            Entity bookResource = service.Retrieve(resourceGroup.LogicalName, resourceGroup.Id, new ColumnSet("accountid", "timezone"));
                            if (bookResource.Attributes.Contains("accountid") && bookResource.Attributes["accountid"] != null)
                            {
                                EntityReference account = (EntityReference)bookResource.Attributes["accountid"];
                                pload.AccountID = account.Id.ToString();
                                Entity parentAccount = service.Retrieve(account.LogicalName, account.Id, new ColumnSet("jarvis_responsableunitid", "jarvis_temporarydealerinformationvalidfrom", "jarvis_temporarydealerinformationvaliduntil", "jarvis_vasdealertype", "jarvis_temporarydealerinformation", "jarvis_service247", "address1_city", "jarvis_address1_country", "jarvis_country"));
                                if (parentAccount.Attributes.Contains("jarvis_temporarydealerinformationvalidfrom") && parentAccount.Attributes["jarvis_temporarydealerinformationvalidfrom"] != null)
                                {
                                    int timeZoneCode = 105;
                                    DateTime createdTime = this.RetrieveLocalTimeFromUTCTime(service, (DateTime)parentAccount.Attributes["jarvis_temporarydealerinformationvalidfrom"], timeZoneCode);
                                    pload.ValidFrom = createdTime.ToString("yyyy-MM-dd HH:mm");
                                }
                                else
                                {
                                    pload.ValidFrom = " ";
                                }

                                if (parentAccount.Attributes.Contains("jarvis_temporarydealerinformation") && parentAccount.Attributes["jarvis_temporarydealerinformation"] != null)
                                {
                                    pload.TemporaryRepairInfo = (string)parentAccount.Attributes["jarvis_temporarydealerinformation"];
                                }
                                else
                                {
                                    pload.TemporaryRepairInfo = " ";
                                }

                                pload.TDI = "No";
                                if (parentAccount.Attributes.Contains("jarvis_temporarydealerinformationvaliduntil") && parentAccount.Attributes["jarvis_temporarydealerinformationvaliduntil"] != null)
                                {
                                    DateTime validTo = (DateTime)parentAccount.Attributes["jarvis_temporarydealerinformationvaliduntil"];
                                    if (validTo < DateTime.Now)
                                    {
                                        pload.ValidFrom = " ";
                                        pload.ValidTo = " ";
                                        pload.TemporaryRepairInfo = " ";
                                        pload.TDI = "No";
                                    }
                                    else
                                    {
                                        int timeZoneCode = 105;
                                        DateTime createdToTime = this.RetrieveLocalTimeFromUTCTime(service, (DateTime)parentAccount.Attributes["jarvis_temporarydealerinformationvaliduntil"], timeZoneCode);
                                        pload.ValidTo = createdToTime.ToString("yyyy-MM-dd HH:mm");
                                        pload.TDI = "Yes";
                                    }
                                }
                                else
                                {
                                    pload.ValidTo = " ";
                                }

                                if (parentAccount.Attributes.Contains("jarvis_vasdealertype") && parentAccount.Attributes["jarvis_vasdealertype"] != null)
                                {
                                    pload.DealerType = ((EntityReference)parentAccount.Attributes["jarvis_vasdealertype"]).Name;
                                }
                                else
                                {
                                    pload.DealerType = " ";
                                }

                                if (parentAccount.Attributes.Contains("jarvis_service247") && parentAccount.Attributes["jarvis_service247"] != null)
                                {
                                    pload.Service = ((bool)parentAccount.Attributes["jarvis_service247"]).ToString();

                                    // pLoad.TDI = ((bool)parentAccount.Attributes["jarvis_service247"]).ToString();
                                }
                                else
                                {
                                    pload.Service = "No";
                                }

                                if (parentAccount.Attributes.Contains("address1_city") && parentAccount.Attributes["address1_city"] != null)
                                {
                                    pload.City = (string)parentAccount.Attributes["address1_city"];
                                }

                                if (parentAccount.Attributes.Contains("jarvis_country") && parentAccount.Attributes["jarvis_country"] != null)
                                {
                                    pload.Country = ((EntityReference)parentAccount.Attributes["jarvis_country"]).Name;
                                }
                                else if (parentAccount.Attributes.Contains("jarvis_address1_country") && parentAccount.Attributes["jarvis_address1_country"] != null)
                                {
                                    pload.Country = ((EntityReference)parentAccount.Attributes["jarvis_address1_country"]).Name;
                                }

                                if (parentAccount.Attributes.Contains("jarvis_responsableunitid") && parentAccount.Attributes["jarvis_responsableunitid"] != null)
                                {
                                    pload.ResponsibleUnitID = (string)parentAccount.Attributes["jarvis_responsableunitid"];
                                }
                                else
                                {
                                    pload.ResponsibleUnitID = string.Empty;
                                }

                                if (bookResource.Attributes.Contains("timezone") && bookResource.Attributes["timezone"] != null)
                                {
                                    int timeZoneCode = (int)bookResource.Attributes["timezone"];
                                    Entity timeZone = this.GetTimezoneByCode(service, tracing, timeZoneCode);
                                    if (timeZone != null && timeZone.Attributes.Contains("userinterfacename") && timeZone.Attributes["userinterfacename"] != null)
                                    {
                                        var userInterfaceName = timeZone.Attributes["userinterfacename"];
                                        string timeZoneLabel = userInterfaceName.ToString().Substring(0, 11);
                                        pload.TimeZone = timeZoneLabel;
                                    }
                                }
                                else
                                {
                                    pload.TimeZone = "(GMT+01:00)";
                                }
                            }

                            EntityCollection resourceServices = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getBookableServices, resourceGroup.Id)));
                            if (resourceServices.Entities.Count > 0)
                            {
                                foreach (var sitem in resourceServices.Entities)
                                {
                                    if (sitem.Attributes.Contains("characteristic") && sitem.Attributes["characteristic"] != null)
                                    {
                                        blist.Add(((EntityReference)sitem.Attributes["characteristic"]).Id.ToString());
                                    }
                                }

                                serviceCheck = resourceList.All(value => blist.Contains(value));
                            }
                        }
                    }

                    if (item.Attributes.Contains("StartTime") && item.Attributes["StartTime"] != null)
                    {
                        pload.StartTime = ((DateTime)item.Attributes["StartTime"]).ToString("dd/MM/yyyy hh:mm tt");
                    }

                    if (item.Attributes.Contains("EndTime") && item.Attributes["EndTime"] != null)
                    {
                        pload.EndTime = ((DateTime)item.Attributes["EndTime"]).ToString("dd/MM/yyyy hh:mm tt");
                    }

                    // Travel
                    if (item.Attributes.Contains("Travel") && item.Attributes["Travel"] != null)
                    {
                        Entity travel = (Entity)item.Attributes["Travel"];
                        if (travel.Attributes.Contains("Distance") && travel.Attributes["Distance"] != null)
                        {
                            double distance = (double)travel.Attributes["Distance"];
                            distances = Convert.ToDecimal(distance);

                            distance = Math.Round(distance, 2);
                            pload.Distance = distance.ToString() + " " + "km";
                        }

                        if (travel.Attributes.Contains("TravelTime") && travel.Attributes["TravelTime"] != null)
                        {
                            double travelTime = (double)travel.Attributes["TravelTime"];
                            travelTime = Math.Round(travelTime, 2);
                            pload.TravelTime = travelTime.ToString() + " " + "mins";
                        }

                        pload.TravelStartTime = startDate.ToString("dd/MM/yyyy hh:mm tt");
                    }

                    bool isPotential = true;
                    if (item.Attributes.Contains("Potential") && item.Attributes["Potential"] != null)
                    {
                        isPotential = (bool)item.Attributes["Potential"];
                    }

                    if (distances <= Convert.ToDecimal(searchRadius))
                    {
                        if (serviceCheck)
                        {
                            if (!isResource)
                            {
                                if (isPotential)
                                {
                                    payList.Add(pload);
                                }
                            }
                        }
                    }
                }
            }

            var resOutput = JsonConvert.SerializeObject(payList);
            results = resOutput.ToString();
            return results;
        }

        /// <summary>
        /// Retrieve Local Time From UTC Time.
        /// </summary>
        /// <param name="service">Org service.</param>
        /// <param name="utcTime">UTC Time.</param>
        /// <param name="timeZoneCode">Time Zone Code.</param>
        /// <returns>UTC Value.</returns>
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
        /// Split String.
        /// </summary>
        /// <param name="input">input param.</param>
        /// <param name="chunkSize">chunk Size.</param>
        /// <returns>returns chunks.</returns>
        private static string[] SplitString(string input, int chunkSize)
        {
            int length = input.Length;
            int numChunks = (length + chunkSize - 1) / chunkSize;
            string[] chunks = new string[numChunks];

            for (int i = 0; i < numChunks; i++)
            {
                int startIndex = i * chunkSize;
                int endIndex = Math.Min(startIndex + chunkSize, length);
                chunks[i] = input.Substring(startIndex, endIndex - startIndex);
            }

            return chunks;
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
