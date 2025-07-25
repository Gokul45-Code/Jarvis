// <Copyright file="VehiclePreOperationSync.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace MCS.Jarvis.CE.Plugins
{
    using System;
    using System.Linq;
    using System.Text.RegularExpressions;
    using MCS.Jarvis.CE.BusinessProcessesShared;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;

    /// <summary>
    /// Vehicle Pre Operation Sync.
    /// </summary>
    public class VehiclePreOperationSync : IPlugin
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

            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                Entity vehicle = (Entity)context.InputParameters["Target"];
                try
                {
                    // region Pre-Create
                    if (context.Stage == 20 && context.MessageName.ToUpper() == "CREATE")
                    {
                        IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                        IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
                        bool setUpdatedhomeDealer = true;

                        if (vehicle.Attributes.Contains("jarvis_name") && vehicle.Attributes["jarvis_name"] != null)
                        {
                            tracingService.Trace("entered create");
                            string regNumber = (string)vehicle.Attributes["jarvis_name"];
                            string regNumberShadow = Regex.Replace(regNumber, @"(\s+|@|&|'|\(|\)|<|>|#|-)", " ");
                            regNumberShadow = string.Concat(regNumberShadow.Where(c => !char.IsWhiteSpace(c)));
                            if (regNumberShadow != null && regNumberShadow != string.Empty)
                            {
                                vehicle["jarvis_registrationnumbershadow"] = regNumberShadow;
                            }

                            vehicle["jarvis_updatedregistrationnumber"] = regNumber;
                        }

                        /*if (vehicle.Attributes.Contains("jarvis_homedealer") && vehicle.Attributes["jarvis_homedealer"] != null)
                        {
                            EntityReference homeDealer = (EntityReference)vehicle.Attributes["jarvis_homedealer"];
                            vehicle["jarvis_updatedhomedealer"] = homeDealer;
                        }*/

                        if ((vehicle.Attributes.Contains("jarvis_homedealer") && vehicle.Attributes["jarvis_homedealer"] != null) ||
                           (vehicle.Attributes.Contains("jarvis_updatedhomedealer") && vehicle.Attributes["jarvis_updatedhomedealer"] != null))
                        {
                            if (vehicle.Attributes.Contains("jarvis_updatedhomedealer") && vehicle.Attributes["jarvis_updatedhomedealer"] != null)
                            {
                                EntityReference updatedHomeDealer = (EntityReference)vehicle.Attributes["jarvis_updatedhomedealer"];
                                Entity dealerAccount = service.Retrieve(updatedHomeDealer.LogicalName, updatedHomeDealer.Id, new ColumnSet("jarvis_onecasestatus", "jarvis_responsableunitid"));
                                if (dealerAccount.Attributes.Contains("jarvis_responsableunitid") && dealerAccount.Attributes["jarvis_responsableunitid"] != null)
                                {
                                    if (dealerAccount.Attributes["jarvis_responsableunitid"].ToString().ToUpper() != "DUMMY")
                                    {
                                        if (dealerAccount.Attributes.Contains("jarvis_onecasestatus") && dealerAccount.Attributes["jarvis_onecasestatus"] != null)
                                        {
                                            var vasStatusValue = (OptionSetValue)dealerAccount.Attributes["jarvis_onecasestatus"];
                                            if (vasStatusValue.Value == 334030000)
                                            {
                                                setUpdatedhomeDealer = false;
                                            }
                                        }
                                    }
                                }
                            }

                            if (setUpdatedhomeDealer)
                            {
                                CaseOperations operations = new CaseOperations();
                                vehicle = operations.SetUpdatedDealer(service, tracingService, vehicle, vehicle);
                            }
                        }

                        if (vehicle.Attributes.Contains("jarvis_owningcustomer") && vehicle.Attributes["jarvis_owningcustomer"] != null)
                        {
                            EntityReference owningCustomer = (EntityReference)vehicle.Attributes["jarvis_owningcustomer"];
                            vehicle["jarvis_updatedowningcustomer"] = owningCustomer;
                        }

                        if (vehicle.Attributes.Contains("jarvis_usingcustomer") && vehicle.Attributes["jarvis_usingcustomer"] != null)
                        {
                            EntityReference usingCustomer = (EntityReference)vehicle.Attributes["jarvis_usingcustomer"];
                            vehicle["jarvis_updatedusingcustomer"] = usingCustomer;
                        }
                    }

                    // endregion
                    // region Pre-Update
                    if (context.Stage == 20 && context.MessageName.ToUpper() == "UPDATE")
                    {
                        tracingService.Trace("enter update");
                        IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                        IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
                        bool setUpdatedhomeDealer = true;

                        tracingService.Trace("get preimage");
                        Entity vehiclePreImage = context.PreEntityImages["PreImage"];
                        tracingService.Trace("set preimage");
                        //Entity vehiclePreImage = (Entity)context.PreEntityImages["PreImage"];

                        if (vehicle.Attributes.Contains("jarvis_name") && vehicle.Attributes["jarvis_name"] != null)
                        {
                            tracingService.Trace("entered update");
                            string regNumber = (string)vehicle.Attributes["jarvis_name"];
                            string regNumberShadow = Regex.Replace(regNumber, @"(\s+|@|&|'|\(|\)|<|>|#|-)", " ");
                            regNumberShadow = string.Concat(regNumberShadow.Where(c => !char.IsWhiteSpace(c)));
                            if (regNumberShadow != null && regNumberShadow != string.Empty)
                            {
                                vehicle["jarvis_registrationnumbershadow"] = regNumberShadow;
                            }

                            vehicle["jarvis_updatedregistrationnumber"] = regNumber;
                        }

                        // if (vehicle.Attributes.Contains("jarvis_homedealer") && vehicle.Attributes["jarvis_homedealer"] != null)
                        // {
                        //    EntityReference homeDealer = (EntityReference)vehicle.Attributes["jarvis_homedealer"];
                        //    vehicle["jarvis_updatedhomedealer"] = homeDealer;
                        // }

                        if ((vehicle.Attributes.Contains("jarvis_homedealer") && vehicle.Attributes["jarvis_homedealer"] != null) ||
                          (vehicle.Attributes.Contains("jarvis_updatedhomedealer") && vehicle.Attributes["jarvis_updatedhomedealer"] != null))
                        {
                            if (vehicle.Attributes.Contains("jarvis_updatedhomedealer") && vehicle.Attributes["jarvis_updatedhomedealer"] != null)
                            {
                                tracingService.Trace("jarvis_updatedhomedealer is updated");
                                EntityReference updatedHomeDealer = (EntityReference)vehicle.Attributes["jarvis_updatedhomedealer"];
                                Entity dealerAccount = service.Retrieve(updatedHomeDealer.LogicalName, updatedHomeDealer.Id, new ColumnSet("jarvis_onecasestatus", "jarvis_responsableunitid"));
                                tracingService.Trace("Updated dealer retrieved");
                                if (dealerAccount.Attributes.Contains("jarvis_responsableunitid") && dealerAccount.Attributes["jarvis_responsableunitid"] != null)
                                {
                                    tracingService.Trace("Updated dealer contains responsable unit id");
                                    if (dealerAccount.Attributes["jarvis_responsableunitid"].ToString().ToUpper() != "DUMMY")
                                    {
                                        tracingService.Trace("Updated dealer responsable unit id not dummy");
                                        if (dealerAccount.Attributes.Contains("jarvis_onecasestatus") && dealerAccount.Attributes["jarvis_onecasestatus"] != null)
                                        {
                                            tracingService.Trace("Updated dealer VAS status is not null");
                                            var vasStatusValue = (OptionSetValue)dealerAccount.Attributes["jarvis_onecasestatus"];
                                            tracingService.Trace("vasStatusValue" + vasStatusValue.ToString());
                                            if (vasStatusValue.Value == 334030000)
                                            {
                                                tracingService.Trace("Updated home dealer is active");
                                                setUpdatedhomeDealer = false;
                                            }
                                        }
                                    }
                                }
                            }

                            if (setUpdatedhomeDealer)
                            {
                                tracingService.Trace("Update dealer");
                                CaseOperations operations = new CaseOperations();
                                vehicle = operations.SetUpdatedDealer(service, tracingService, vehicle, vehiclePreImage);
                            }
                        }

                        if (vehicle.Attributes.Contains("jarvis_owningcustomer") && vehicle.Attributes["jarvis_owningcustomer"] != null)
                        {
                            EntityReference owningCustomer = (EntityReference)vehicle.Attributes["jarvis_owningcustomer"];
                            vehicle["jarvis_updatedowningcustomer"] = owningCustomer;
                        }

                        if (vehicle.Attributes.Contains("jarvis_usingcustomer") && vehicle.Attributes["jarvis_usingcustomer"] != null)
                        {
                            EntityReference usingCustomer = (EntityReference)vehicle.Attributes["jarvis_usingcustomer"];
                            vehicle["jarvis_updatedusingcustomer"] = usingCustomer;
                        }
                    }

                    // endregion
                }
                catch (InvalidPluginExecutionException ex)
                {
                    throw new InvalidPluginExecutionException("Error in Vehicle Operations " + ex.Message + " ");
                }
            }
        }
    }
}
