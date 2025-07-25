//-----------------------------------------------------------------------
// <copyright file="CaseCreatePostOperationUserProfileSync.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace MCS.Jarvis.CE.Plugins
{
    using System;
    using System.ServiceModel;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;

    /// <summary>
    /// CaseCreate PostOperation UserProfile Sync.
    /// </summary>
    public class CaseCreatePostOperationUserProfileSync : IPlugin
    {
        /// <summary>
        /// Execute CRM plugin.
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
                if (context.Stage == (int)Constants.PluginStage.PostOperation && context.MessageName.ToUpper() == Constants.PluginMessage.Create)
                {
                    if (!context.InputParameters.Contains("Target") || !(context.InputParameters["Target"] is Entity))
                    {
                        return;
                    }

                    IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                    IOrganizationService service = serviceFactory.CreateOrganizationService(null);

                    Entity caseObject = context.InputParameters["Target"] as Entity;

                    EntityReference caseBusinessPartner = caseObject.Attributes.Contains(Case.Attributes.Customer) ? (EntityReference)caseObject.Attributes[Case.Attributes.Customer] : null;

                    EntityReference caseHomeDealer = caseObject.Attributes.Contains(Case.Attributes.HomeDealer) ? (EntityReference)caseObject.Attributes[Case.Attributes.HomeDealer] : null;

                    EntityReference caseBreakdownCountry = caseObject.Attributes.Contains(Case.Attributes.BreakdownCountry) ? (EntityReference)caseObject.Attributes[Case.Attributes.BreakdownCountry] : null;

                    CaseUserProfileOperation caseUserProfileOperation = new CaseUserProfileOperation(service, tracingService);

                    if (caseBusinessPartner != null && caseBusinessPartner.Id != null)
                    {
                        //// Logic to sync up/create Case User Profiles based on Customer

                        Entity businessPartner = service.Retrieve(BusinessPartner.EntityLogicalName, caseBusinessPartner.Id, new ColumnSet(new string[] { BusinessPartner.Atrributes.Country, BusinessPartner.Atrributes.BusinessPartnerCountry }));

                        EntityReference country = businessPartner.Attributes.Contains(BusinessPartner.Atrributes.Country) ?
                            (EntityReference)businessPartner.Attributes[BusinessPartner.Atrributes.Country]
                            : (businessPartner.Attributes.Contains(BusinessPartner.Atrributes.BusinessPartnerCountry)
                            ? (EntityReference)businessPartner.Attributes[BusinessPartner.Atrributes.BusinessPartnerCountry]
                            : null);

                        caseUserProfileOperation.SyncCaseUserProfile(caseObject, country, caseBusinessPartner);
                    }

                    if (caseHomeDealer != null && caseHomeDealer.Id != null)
                    {
                        //// logic to sync up/create Case User Profile based on Home Dealer

                        Entity homeDealer = service.Retrieve(BusinessPartner.EntityLogicalName, caseHomeDealer.Id, new ColumnSet(new string[] { BusinessPartner.Atrributes.Country, BusinessPartner.Atrributes.BusinessPartnerCountry }));

                        EntityReference country = homeDealer.Attributes.Contains(BusinessPartner.Atrributes.Country) ?
                            (EntityReference)homeDealer.Attributes[BusinessPartner.Atrributes.Country]
                            : (homeDealer.Attributes.Contains(BusinessPartner.Atrributes.BusinessPartnerCountry)
                            ? (EntityReference)homeDealer.Attributes[BusinessPartner.Atrributes.BusinessPartnerCountry]
                            : null);

                        caseUserProfileOperation.SyncCaseUserProfile(caseObject, country, caseHomeDealer);
                    }

                    if (caseBreakdownCountry != null && caseBreakdownCountry.Id != null)
                    {
                        //// logic to sync up/create Case User Profile based on Breakdown country

                        caseUserProfileOperation.SyncCaseUserProfile(caseObject, caseBreakdownCountry, null);
                    }
                }
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
