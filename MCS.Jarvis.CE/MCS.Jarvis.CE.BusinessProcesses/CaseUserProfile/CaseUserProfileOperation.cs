// <copyright file="CaseUserProfileOperation.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MCS.Jarvis.CE.BusinessProcesses
{
    using System;
    using System.ServiceModel;
    using MCS.Jarvis.CE.Commons;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;

    /// <summary>
    /// Case User Profile Operation.
    /// </summary>
    public class CaseUserProfileOperation
    {
        /// <summary>
        /// Gets or sets org Service.
        /// </summary>
        private IOrganizationService Service { get; set; }

        /// <summary>
        /// Gets or sets tracing service.
        /// </summary>
        private ITracingService TracingService { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CaseUserProfileOperation"/> class.
        /// </summary>
        /// <param name="service">Org service.</param>
        /// <param name="tracingService">tracing service.</param>
        public CaseUserProfileOperation(IOrganizationService service, ITracingService tracingService)
        {
            this.Service = service;
            this.TracingService = tracingService;
        }

        /// <summary>
        /// sync Case User Profile.
        /// </summary>
        /// <param name="caseObject">case object.</param>
        /// <param name="country">country details.</param>
        /// <param name="businessPartner">Business Partner.</param>
        /// <exception cref="InvalidOperationException">Invalid Operation Exception.</exception>
        public void SyncCaseUserProfile(Entity caseObject, EntityReference country, EntityReference businessPartner)
        {
            OptionSetValue caseTypeCode = caseObject.Attributes.Contains(Case.Attributes.CaseType) ?
                (OptionSetValue)caseObject.Attributes[Case.Attributes.CaseType] : null;

            EntityReference serviceLine = caseObject.Attributes.Contains(Case.Attributes.ServiceLine) ?
                (EntityReference)caseObject.Attributes[Case.Attributes.ServiceLine] : null;

            OptionSetValue caseLocation = caseObject.Attributes.Contains(Case.Attributes.Location) ?
                (OptionSetValue)caseObject.Attributes[Case.Attributes.Location] : null;

            if (country == null || caseTypeCode == null || serviceLine == null || caseLocation == null)
            {
                this.TracingService.Trace("Sync of Case User Profile is not possible since the case does not have all the necessary details in place (Country/Case Type/Service Line/Case Location)");
                throw new InvalidOperationException("Sync of Case User Profile is not possible since the case does not have all the necessary details in place (Country/Case Type/Service Line/Case Location)");
            }

            // call out the retrieve method for Team Profiles
            EntityCollection matchingUserProfiles = this.GetMatchingUserProfiles(country, caseTypeCode, serviceLine, caseLocation);

            // Call out the Case User Profile Create method
            this.CreateCaseUserProfiles(caseObject, businessPartner, matchingUserProfiles);
        }

        /// <summary>
        /// Remove Case User Profile.
        /// </summary>
        /// <param name="caseRef">case reference.</param>
        /// <param name="businessPartner">business partner.</param>
        public void RemoveCaseUserProfile(EntityReference caseRef, EntityReference businessPartner)
        {
            try
            {
                EntityCollection userProfilesToRemoveCollection = this.GetUserProfilesToRemove(caseRef, businessPartner);

                if (userProfilesToRemoveCollection != null && userProfilesToRemoveCollection.Entities != null && userProfilesToRemoveCollection.Entities.Count > 0)
                {
                    foreach (Entity userProfile in userProfilesToRemoveCollection.Entities)
                    {
                        this.Service.Delete(CaseUserProfile.EntityLogicalName, userProfile.Id);
                    }
                }
            }
            catch (FaultException<OrganizationServiceFault> fex)
            {
                this.TracingService.Trace(fex.Message);
                throw fex;
            }
        }

        /// <summary>
        /// Get User Profiles To Remove.
        /// </summary>
        /// <param name="caseRef">case reference.</param>
        /// <param name="businessPartner">business partner.</param>
        /// <returns>Entity collection.</returns>
        private EntityCollection GetUserProfilesToRemove(EntityReference caseRef, EntityReference businessPartner)
        {
            try
            {
                QueryExpression userProfilesToRemoveQuery = new QueryExpression()
                {
                    EntityName = CaseUserProfile.EntityLogicalName,
                    NoLock = true,
                    ColumnSet = new ColumnSet(new string[] { UserProfile.Attributes.Profile, UserProfile.Attributes.User }),
                    Criteria = new FilterExpression()
                    {
                        FilterOperator = LogicalOperator.And,
                    },
                };

                userProfilesToRemoveQuery.Criteria.AddCondition(new ConditionExpression(CaseUserProfile.Atrributes.Case, ConditionOperator.Equal, caseRef.Id));

                if (businessPartner != null)
                {
                    userProfilesToRemoveQuery.Criteria.AddCondition(new ConditionExpression(CaseUserProfile.Atrributes.BusinessPartner, ConditionOperator.Equal, businessPartner.Id));
                }

                return this.Service.RetrieveMultiple(userProfilesToRemoveQuery);
            }
            catch (FaultException<OrganizationServiceFault> fex)
            {
                this.TracingService.Trace(fex.Message);
                throw fex;
            }
        }

        /// <summary>
        /// Get Matching User Profiles.
        /// </summary>
        /// <param name="country">Country details.</param>
        /// <param name="caseTypeCode">case type code.</param>
        /// <param name="serviceLine">service line.</param>
        /// <param name="caseLocation">case location.</param>
        /// <returns>Entity collection.</returns>
        private EntityCollection GetMatchingUserProfiles(EntityReference country, OptionSetValue caseTypeCode, EntityReference serviceLine, OptionSetValue caseLocation)
        {
            try
            {
                QueryExpression userProfileQuery = new QueryExpression()
                {
                    EntityName = UserProfile.EntityLogicalName,
                    NoLock = true,
                    ColumnSet = new ColumnSet(new string[] { UserProfile.Attributes.Profile, UserProfile.Attributes.User }),
                    Criteria = new FilterExpression()
                    {
                        FilterOperator = LogicalOperator.And,
                        Conditions =
                        {
                            new ConditionExpression(UserProfile.Attributes.Country, ConditionOperator.Equal, country.Id),
                            new ConditionExpression(UserProfile.Attributes.CaseLocation, ConditionOperator.Equal, caseLocation.Value),
                            new ConditionExpression(UserProfile.Attributes.CaseType, ConditionOperator.Equal, caseTypeCode.Value),
                            new ConditionExpression(UserProfile.Attributes.ServiceLine, ConditionOperator.Equal, serviceLine.Id),
                            new ConditionExpression(UserProfile.Attributes.Status, ConditionOperator.Equal, (int)UserProfile.Status.Active),
                        },
                    },
                };

                return this.Service.RetrieveMultiple(userProfileQuery);
            }
            catch (FaultException<OrganizationServiceFault> fex)
            {
                this.TracingService.Trace(fex.Message);
                throw fex;
            }
        }

        /// <summary>
        /// Create Case User Profiles.
        /// </summary>
        /// <param name="casedetails">case details.</param>
        /// <param name="businessPartner">business partner.</param>
        /// <param name="userProfiles">user profile.</param>
        private void CreateCaseUserProfiles(Entity casedetails, EntityReference businessPartner, EntityCollection userProfiles)
        {
            // Create the Case User profile record per team profile
            try
            {
                if (userProfiles != null && userProfiles.Entities != null && userProfiles.Entities.Count > 0)
                {
                    foreach (Entity userProfile in userProfiles.Entities)
                    {
                        Entity caseUserProfile = new Entity(CaseUserProfile.EntityLogicalName);

                        caseUserProfile.Attributes.Add(CaseUserProfile.Atrributes.Case, casedetails.ToEntityReference());
                        caseUserProfile.Attributes.Add(CaseUserProfile.Atrributes.BusinessPartner, businessPartner);
                        caseUserProfile.Attributes.Add(CaseUserProfile.Atrributes.User, userProfile.Attributes.Contains(UserProfile.Attributes.User) ? (EntityReference)userProfile.Attributes[UserProfile.Attributes.User] : null);
                        caseUserProfile.Attributes.Add(CaseUserProfile.Atrributes.Profile, userProfile.Attributes.Contains(UserProfile.Attributes.Profile) ? (EntityReference)userProfile.Attributes[UserProfile.Attributes.Profile] : null);

                        this.Service.Create(caseUserProfile);
                    }
                }
            }
            catch (FaultException<OrganizationServiceFault> fex)
            {
                this.TracingService.Trace(fex.Message);
                throw fex;
            }
        }
    }
}
