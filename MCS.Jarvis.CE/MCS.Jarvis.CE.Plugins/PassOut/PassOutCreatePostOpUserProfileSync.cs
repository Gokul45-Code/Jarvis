// <copyright file="PassOutCreatePostOpUserProfileSync.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MCS.Jarvis.CE.Plugins
{
    using System;
    using System.ServiceModel;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;

    /// <summary>
    /// Pass Out Create Post Op User Profile Sync.
    /// </summary>
    public class PassOutCreatePostOpUserProfileSync : IPlugin
    {
        /// <summary>
        /// Execute Method.
        /// </summary>
        /// <param name="serviceProvider">service provider.</param>
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
                if (context.Stage == (int)Constants.PluginStage.PostOperation && context.MessageName.ToUpper() == Constants.PluginMessage.Create)
                {
                    if (!context.InputParameters.Contains("Target") || !(context.InputParameters["Target"] is Entity))
                    {
                        return;
                    }

                    IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                    IOrganizationService service = serviceFactory.CreateOrganizationService(null);

                    Entity passOutObject = context.InputParameters["Target"] as Entity;

                    EntityReference caseRef = passOutObject.Attributes.Contains(PassOut.Attributes.Case) ?
                        (EntityReference)passOutObject.Attributes[PassOut.Attributes.Case] : null;

                    EntityReference businessPartnerRef = passOutObject.Attributes.Contains(PassOut.Attributes.BusinessPartner) ?
                        (EntityReference)passOutObject.Attributes[PassOut.Attributes.BusinessPartner] : null;

                    if (caseRef == null || businessPartnerRef == null)
                    {
                        tracingService.Trace("The passout record does not have sufficient information to create Case User Profiles...");
                        return;
                    }

                    Entity caseObject = service.Retrieve(Case.EntityLogicalName, caseRef.Id, new ColumnSet(new string[] { Case.Attributes.Location, Case.Attributes.ServiceLine, Case.Attributes.CaseType }));

                    Entity businessPartner = service.Retrieve(BusinessPartner.EntityLogicalName, businessPartnerRef.Id, new ColumnSet(new string[] { BusinessPartner.Atrributes.Country, BusinessPartner.Atrributes.BusinessPartnerCountry }));

                    EntityReference country = businessPartner.Attributes.Contains(BusinessPartner.Atrributes.Country) ?
                                (EntityReference)businessPartner.Attributes[BusinessPartner.Atrributes.Country]
                                : (businessPartner.Attributes.Contains(BusinessPartner.Atrributes.BusinessPartnerCountry)
                                ? (EntityReference)businessPartner.Attributes[BusinessPartner.Atrributes.BusinessPartnerCountry]
                                : null);

                    CaseUserProfileOperation caseUserProfileOperation = new CaseUserProfileOperation(service, tracingService);

                    caseUserProfileOperation.SyncCaseUserProfile(caseObject, country, businessPartnerRef);
                }
            }

            // catch (InvalidOperationException oex)
            // {
            //    tracingService.Trace(oex.Message);
            //    tracingService.Trace(oex.StackTrace);
            // }
            // catch (FaultException<OrganizationServiceFault> fex)
            // {
            //    tracingService.Trace(fex.Message);
            //    tracingService.Trace(fex.StackTrace);
            //    throw fex;
            // }
            // catch (Exception ex)
            // {
            //    tracingService.Trace(ex.Message);
            //    tracingService.Trace(ex.StackTrace);
            //    throw new InvalidPluginExecutionException("Error in syncing up Case User Profiles on creation of Case " + ex.Message);
            // }
            catch (InvalidPluginExecutionException ex)
            {
                throw new InvalidPluginExecutionException(ex.Message);
            }
        }
    }
}
