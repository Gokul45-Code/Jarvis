// <copyright file="CaseUserProfileOperation.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MCS.Jarvis.CE.BusinessProcessesShared
{
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
        /// Initializes a new instance of the <see cref="CaseUserProfileOperation"/> class.
        /// </summary>
        /// <param name="service">Org Service.</param>
        /// <param name="tracingService">tracing Service.</param>
        public CaseUserProfileOperation(IOrganizationService service, ITracingService tracingService)
        {
            this.Service = service;
            this.TracingService = tracingService;
        }

        /// <summary>
        /// Gets or sets org Service.
        /// </summary>
        private IOrganizationService Service { get; set; }

        /// <summary>
        /// Gets or sets tracing Service.
        /// </summary>
        private ITracingService TracingService { get; set; }

        /// <summary>
        /// Sync Case User Profile.
        /// </summary>
        /// <param name="caseObject">case object.</param>
        /// <param name="country">Country details.</param>
        /// <param name="businessPartner">business Partner.</param>
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
            }
            else
            {
                // call out the retrieve method for Team Profiles
                EntityCollection matchingUserProfiles = this.GetMatchingUserProfiles(country, caseTypeCode, serviceLine, caseLocation);

                // Call out the Case User Profile Create method
                this.CreateCaseUserProfiles(caseObject, businessPartner, matchingUserProfiles);
            }
        }

        /// <summary>
        /// Remove Case User Profile.
        /// </summary>
        /// <param name="caseRef">case reference.</param>
        /// <param name="businessPartner">business Partner.</param>
        /// <param name="targetUser">target user.</param>
        public void RemoveCaseUserProfile(EntityReference caseRef, EntityReference businessPartner = null, EntityReference targetUser = null)
        {
            try
            {
                EntityCollection userProfilesToRemoveCollection = this.GetUserProfilesToRemove(caseRef, businessPartner, targetUser);

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
                this.TracingService.Trace(fex.StackTrace);
                throw fex;
            }
        }

        /// <summary>
        /// Get User Profiles To Remove.
        /// </summary>
        /// <param name="caseRef">case reference.</param>
        /// <param name="businessPartner">business Partner.</param>
        /// <param name="targetUser">target user.</param>
        /// <returns>entity collection.</returns>
        private EntityCollection GetUserProfilesToRemove(EntityReference caseRef, EntityReference businessPartner = null, EntityReference targetUser = null)
        {
            try
            {
                QueryExpression userProfilesToRemoveQuery = new QueryExpression()
                {
                    EntityName = CaseUserProfile.EntityLogicalName,
                    NoLock = true,
                    ColumnSet = new ColumnSet(new string[] { CaseUserProfile.Atrributes.Profile, CaseUserProfile.Atrributes.User }),
                    Criteria = new FilterExpression()
                    {
                        FilterOperator = LogicalOperator.And,
                    },
                };

                userProfilesToRemoveQuery.Criteria.AddCondition(new ConditionExpression(CaseUserProfile.Atrributes.Case, ConditionOperator.Equal, caseRef.Id));

                if (businessPartner != null && businessPartner.Id != null)
                {
                    userProfilesToRemoveQuery.Criteria.AddCondition(new ConditionExpression(CaseUserProfile.Atrributes.BusinessPartner, ConditionOperator.Equal, businessPartner.Id));
                }

                if (targetUser != null && targetUser.Id != null)
                {
                    userProfilesToRemoveQuery.Criteria.AddCondition(new ConditionExpression(CaseUserProfile.Atrributes.User, ConditionOperator.Equal, targetUser.Id));
                }

                return this.Service.RetrieveMultiple(userProfilesToRemoveQuery);
            }
            catch (FaultException<OrganizationServiceFault> fex)
            {
                this.TracingService.Trace(fex.Message);
                this.TracingService.Trace(fex.StackTrace);
                throw fex;
            }
        }

        /// <summary>
        /// get Matching User Profiles.
        /// </summary>
        /// <param name="country">country details.</param>
        /// <param name="caseTypeCode">case Type Code.</param>
        /// <param name="serviceLine">service Line.</param>
        /// <param name="caseLocation">case Location.</param>
        /// <returns>entity collection.</returns>
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
                this.TracingService.Trace(fex.StackTrace);
                throw fex;
            }
        }

        /// <summary>
        /// Create Case User Profiles.
        /// </summary>
        /// <param name="caseEntity">Case details.</param>
        /// <param name="businessPartner">business partner.</param>
        /// <param name="userProfiles">user profiles.</param>
        private void CreateCaseUserProfiles(Entity caseEntity, EntityReference businessPartner, EntityCollection userProfiles)
        {
            // Create the Case User profile record per team profile
            try
            {
                if (userProfiles != null && userProfiles.Entities != null && userProfiles.Entities.Count > 0)
                {
                    foreach (Entity userProfile in userProfiles.Entities)
                    {
                        // check if exists
                        QueryExpression userProfileQuery = new QueryExpression()
                        {
                            EntityName = CaseUserProfile.EntityLogicalName,
                            NoLock = true,
                            ColumnSet = new ColumnSet(new string[] { CaseUserProfile.Atrributes.Case, CaseUserProfile.Atrributes.BusinessPartner, CaseUserProfile.Atrributes.User, CaseUserProfile.Atrributes.Profile }),
                            Criteria = new FilterExpression()
                            {
                                FilterOperator = LogicalOperator.And,
                                Conditions =
                        {
                            new ConditionExpression(CaseUserProfile.Atrributes.Case, ConditionOperator.Equal, caseEntity.ToEntityReference().Id),
                            new ConditionExpression(CaseUserProfile.Atrributes.BusinessPartner, ConditionOperator.Equal, businessPartner.Id),
                            new ConditionExpression(CaseUserProfile.Atrributes.User, ConditionOperator.Equal, ((EntityReference)userProfile.Attributes[UserProfile.Attributes.User]).Id),
                            new ConditionExpression(CaseUserProfile.Atrributes.Profile, ConditionOperator.Equal, ((EntityReference)userProfile.Attributes[UserProfile.Attributes.Profile]).Id),
                            new ConditionExpression(CaseUserProfile.Atrributes.Status, ConditionOperator.Equal, (int)UserProfile.Status.Active),
                        },
                            },
                        };

                        var results = this.Service.RetrieveMultiple(userProfileQuery);
                        if (results.Entities.Count < 1)
                        {
                            Entity caseUserProfile = new Entity(CaseUserProfile.EntityLogicalName);

                            caseUserProfile.Attributes.Add(CaseUserProfile.Atrributes.Case, caseEntity.ToEntityReference());
                            caseUserProfile.Attributes.Add(CaseUserProfile.Atrributes.BusinessPartner, businessPartner);
                            caseUserProfile.Attributes.Add(CaseUserProfile.Atrributes.User, userProfile.Attributes.Contains(UserProfile.Attributes.User) ? (EntityReference)userProfile.Attributes[UserProfile.Attributes.User] : null);
                            caseUserProfile.Attributes.Add(CaseUserProfile.Atrributes.Profile, userProfile.Attributes.Contains(UserProfile.Attributes.Profile) ? (EntityReference)userProfile.Attributes[UserProfile.Attributes.Profile] : null);

                            this.Service.Create(caseUserProfile);
                        }
                    }
                }
            }
            catch (FaultException<OrganizationServiceFault> fex)
            {
                this.TracingService.Trace(fex.Message);
                this.TracingService.Trace(fex.StackTrace);
                throw fex;
            }
        }
    }
}
