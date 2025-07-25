// <copyright file="UserOperation.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace MCS.Jarvis.CE.BusinessProcessesShared.User
{
    using MCS.Jarvis.CE.BusinessProcessesShared;
    using MCS.Jarvis.CE.BusinessProcessesShared.Helpers;
    using MCS.Jarvis.CE.Commons;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;

    /// <summary>
    /// Class for UserOperation.
    /// </summary>
    public class UserOperation
    {
        /// <summary>
        /// User Update Operation.
        /// </summary>
        /// <param name="userUpdate">user Update.</param>
        /// <param name="userImg">user Image.</param>
        /// <param name="service">service param.</param>
        /// <param name="tracingService">tracing Service.</param>
        /// <exception cref="InvalidPluginExecutionException">Invalid Plugin Execution Exception.</exception>
        public void UserUpdateOperation(Entity userUpdate, Entity userImg, IOrganizationService service, ITracingService tracingService)
        {
            try
            {
                // Get All active cases
                EntityCollection activecases = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.geRActiveCases)));
                foreach (var caseUpdateObject in activecases.Entities)
                {
                    EntityReference caseBusinessPartner = caseUpdateObject.Attributes.Contains(Case.Attributes.Customer) ? (EntityReference)caseUpdateObject.Attributes[Case.Attributes.Customer] : null;

                    EntityReference caseHomeDealer = caseUpdateObject.Attributes.Contains(Case.Attributes.HomeDealer) ? (EntityReference)caseUpdateObject.Attributes[Case.Attributes.HomeDealer] : null;

                    EntityReference caseBreakdownCountry = caseUpdateObject.Attributes.Contains(Case.Attributes.BreakdownCountry) ? (EntityReference)caseUpdateObject.Attributes[Case.Attributes.BreakdownCountry] : null;

                    CaseUserProfileOperation caseUserProfileOperation = new CaseUserProfileOperation(service, tracingService);

                    EntityReference targetUser = new EntityReference(userUpdate.LogicalName, userUpdate.Id);

                    // Delete existing User Profile for the user
                    caseUserProfileOperation.RemoveCaseUserProfile(caseUpdateObject.ToEntityReference(), null, targetUser);

                    if (caseBusinessPartner != null && caseBusinessPartner.Id != null)
                    {
                        // Logic to sync up/create Case User Profiles based on Customer
                        Entity businessPartner = service.Retrieve(BusinessPartner.EntityLogicalName, caseBusinessPartner.Id, new ColumnSet(new string[] { BusinessPartner.Atrributes.Country, BusinessPartner.Atrributes.BusinessPartnerCountry }));

                        EntityReference country = businessPartner.Attributes.Contains(BusinessPartner.Atrributes.Country) ?
                            (EntityReference)businessPartner.Attributes[BusinessPartner.Atrributes.Country]
                            : (businessPartner.Attributes.Contains(BusinessPartner.Atrributes.BusinessPartnerCountry)
                            ? (EntityReference)businessPartner.Attributes[BusinessPartner.Atrributes.BusinessPartnerCountry]
                        : null);

                        caseUserProfileOperation.SyncCaseUserProfile(caseUpdateObject, country, caseBusinessPartner);
                    }

                    if (caseHomeDealer != null && caseHomeDealer.Id != null)
                    {
                        // logic to sync up/create Case User Profile based on Home Dealer
                        Entity homeDealer = service.Retrieve(BusinessPartner.EntityLogicalName, caseHomeDealer.Id, new ColumnSet(new string[] { BusinessPartner.Atrributes.Country, BusinessPartner.Atrributes.BusinessPartnerCountry }));
                        EntityReference country = homeDealer.Attributes.Contains(BusinessPartner.Atrributes.Country) ?
                            (EntityReference)homeDealer.Attributes[BusinessPartner.Atrributes.Country]
                            : (homeDealer.Attributes.Contains(BusinessPartner.Atrributes.BusinessPartnerCountry)
                            ? (EntityReference)homeDealer.Attributes[BusinessPartner.Atrributes.BusinessPartnerCountry]
                        : null);

                        caseUserProfileOperation.SyncCaseUserProfile(caseUpdateObject, country, caseHomeDealer);
                    }

                    if (caseBreakdownCountry != null && caseBreakdownCountry.Id != null)
                    {
                        // logic to sync up/create Case User Profile based on Breakdown country
                        caseUserProfileOperation.SyncCaseUserProfile(caseUpdateObject, caseBreakdownCountry, null);
                    }
                }
            }
            catch (InvalidPluginExecutionException oex)
            {
                tracingService.Trace(oex.Message);
                tracingService.Trace(oex.StackTrace);
                throw new InvalidPluginExecutionException("Error in syncing up Case User Profiles on creation of Case " + oex.Message);
            }
        }
    }
}
