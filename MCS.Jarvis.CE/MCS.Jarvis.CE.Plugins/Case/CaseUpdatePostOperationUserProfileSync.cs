// <copyright file="CaseUpdatePostOperationUserProfileSync.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MCS.Jarvis.CE.Plugins
{
    using System;
    using System.ServiceModel;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;

    /// <summary>
    /// Case Update Post Operation User Profile Sync.
    /// </summary>
    public class CaseUpdatePostOperationUserProfileSync : IPlugin
    {
        /// <summary>
        /// Execute Method.
        /// </summary>
        /// <param name="serviceProvider">service Provider.</param>
        /// <exception cref="InvalidPluginExecutionException">Invalid Plugin Execution Exception.</exception>
        public void Execute(IServiceProvider serviceProvider)
        {
            ITracingService tracingService =
                (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            // Obtain the execution context from the service provider.
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            try
            {
                if (context.Stage == (int)Constants.PluginStage.PostOperation && context.MessageName.ToUpper() == Constants.PluginMessage.Update)
                {
                    if (!context.InputParameters.Contains("Target") || !(context.InputParameters["Target"] is Entity) || !context.PreEntityImages.Contains("PreImage") || !(context.PreEntityImages["PreImage"] is Entity))
                    {
                        return;
                    }

                    IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                    IOrganizationService service = serviceFactory.CreateOrganizationService(null);

                    Entity caseUpdateObject = context.InputParameters["Target"] as Entity;

                    Entity caseImage = context.PreEntityImages["PreImage"] as Entity;

                    EntityReference caseBusinessPartner = caseUpdateObject.Attributes.Contains(Case.Attributes.Customer) ? (EntityReference)caseUpdateObject.Attributes[Case.Attributes.Customer] : null;

                    EntityReference caseHomeDealer = caseUpdateObject.Attributes.Contains(Case.Attributes.HomeDealer) ? (EntityReference)caseUpdateObject.Attributes[Case.Attributes.HomeDealer] : null;

                    EntityReference caseBreakdownCountry = caseUpdateObject.Attributes.Contains(Case.Attributes.BreakdownCountry) ? (EntityReference)caseUpdateObject.Attributes[Case.Attributes.BreakdownCountry] : null;

                    CaseUserProfileOperation caseUserProfileOperation = new CaseUserProfileOperation(service, tracingService);

                    if (caseBusinessPartner != null && caseBusinessPartner.Id != null)
                    {
                        EntityReference oldCaseBusinessPartner = caseImage.Attributes.Contains(Case.Attributes.Customer) ? (EntityReference)caseImage.Attributes[Case.Attributes.Customer] : null;

                        if (oldCaseBusinessPartner != null && oldCaseBusinessPartner.Id != null && !caseBusinessPartner.Id.Equals(oldCaseBusinessPartner.Id))
                        {
                            caseUserProfileOperation.RemoveCaseUserProfile(caseUpdateObject.ToEntityReference(), oldCaseBusinessPartner);
                            Entity businessPartner = service.Retrieve(BusinessPartner.EntityLogicalName, caseBusinessPartner.Id, new ColumnSet(new string[] { BusinessPartner.Atrributes.Country, BusinessPartner.Atrributes.BusinessPartnerCountry }));

                            EntityReference country = businessPartner.Attributes.Contains(BusinessPartner.Atrributes.Country) ?
                                (EntityReference)businessPartner.Attributes[BusinessPartner.Atrributes.Country]
                                : (businessPartner.Attributes.Contains(BusinessPartner.Atrributes.BusinessPartnerCountry)
                                ? (EntityReference)businessPartner.Attributes[BusinessPartner.Atrributes.BusinessPartnerCountry]
                            : null);

                            caseUserProfileOperation.SyncCaseUserProfile(caseImage, country, caseBusinessPartner);
                        }
                    }

                    if (caseHomeDealer != null && caseHomeDealer.Id != null)
                    {
                        EntityReference oldCaseHomeDealer = caseImage.Attributes.Contains(Case.Attributes.HomeDealer) ? (EntityReference)caseImage.Attributes[Case.Attributes.HomeDealer] : null;

                        if (oldCaseHomeDealer != null && oldCaseHomeDealer.Id != null && !caseHomeDealer.Id.Equals(oldCaseHomeDealer.Id))
                        {
                            caseUserProfileOperation.RemoveCaseUserProfile(caseUpdateObject.ToEntityReference(), oldCaseHomeDealer);

                            Entity homeDealer = service.Retrieve(BusinessPartner.EntityLogicalName, caseHomeDealer.Id, new ColumnSet(new string[] { BusinessPartner.Atrributes.Country, BusinessPartner.Atrributes.BusinessPartnerCountry }));

                            EntityReference country = homeDealer.Attributes.Contains(BusinessPartner.Atrributes.Country) ?
                                (EntityReference)homeDealer.Attributes[BusinessPartner.Atrributes.Country]
                                : (homeDealer.Attributes.Contains(BusinessPartner.Atrributes.BusinessPartnerCountry)
                                ? (EntityReference)homeDealer.Attributes[BusinessPartner.Atrributes.BusinessPartnerCountry]
                                : null);

                            caseUserProfileOperation.SyncCaseUserProfile(caseImage, country, caseHomeDealer);
                        }
                    }

                    if (caseBreakdownCountry != null && caseBreakdownCountry.Id != null)
                    {
                        caseUserProfileOperation.RemoveCaseUserProfile(caseUpdateObject.ToEntityReference(), null);

                        caseUserProfileOperation.SyncCaseUserProfile(caseImage, caseBreakdownCountry, null);
                    }
                }
            }
            catch (InvalidOperationException oex)
            {
                tracingService.Trace(oex.Message);
                tracingService.Trace(oex.StackTrace);
            }
            catch (FaultException<OrganizationServiceFault> fex)
            {
                tracingService.Trace(fex.Message);
                tracingService.Trace(fex.StackTrace);
            }
            catch (Exception ex)
            {
                tracingService.Trace(ex.Message);
                tracingService.Trace(ex.StackTrace);
                throw new InvalidPluginExecutionException("Error in syncing up Case User Profiles on creation of Case " + ex.Message);
            }
        }
    }
}
