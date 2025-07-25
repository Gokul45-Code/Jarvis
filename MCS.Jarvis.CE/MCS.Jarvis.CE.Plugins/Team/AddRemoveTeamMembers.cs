// <Copyright file="AddRemoveTeamMembers.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace MCS.Jarvis.CE.Plugins.Team
{
    using System;
    using System.Collections.Generic;
    using global::Plugins;
    using MCS.jarvis.CE.BusinessProcessShared.MemberAssociation;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;

    /// <summary>
    /// Add Remove Team Members.
    /// </summary>
    public class AddRemoveTeamMembers : PluginBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AddRemoveTeamMembers"/> class.
        /// </summary>
        public AddRemoveTeamMembers()
           : base(typeof(AddRemoveTeamMembers))
        {
        }

        /// <summary>
        /// Execute CRM Plugin.
        /// </summary>
        /// <param name="localcontext">local context.</param>
        public override void ExecuteCrmPlugin(LocalPluginContext localcontext)
        {
            // Extract the tracing service for use in debugging sandboxed plug-ins.
            ITracingService tracingService =
                localcontext.TracingService;

            // Obtain the execution context from the service provider.
            IPluginExecutionContext context = localcontext.PluginExecutionContext;

            // Organization Service
            IOrganizationService service = localcontext.OrganizationService;
            Guid initiatingUserID = context.UserId;
            IOrganizationService adminservice = localcontext.AdminOrganizationService;

            try
            {
                // region Add Users
                if (context.MessageName == "Associate")
                {
                    if (context.InputParameters.Contains("Relationship"))
                    {
                        string relationshipName = ((Relationship)context.InputParameters["Relationship"]).SchemaName;
                        if (relationshipName.Equals(Constants.Team.teammembership, StringComparison.CurrentCultureIgnoreCase))
                        {
                            EntityReference targetEntity = (EntityReference)context.InputParameters["Target"];
                            if (targetEntity.LogicalName.Equals("team"))
                            {
                                if (context.InputParameters.Contains("RelatedEntities") && context.InputParameters["RelatedEntities"] is EntityReferenceCollection)
                                {
                                    var relatedEntities = context.InputParameters["RelatedEntities"] as EntityReferenceCollection;
                                    if (relatedEntities.Count > 0 && ((EntityReference)relatedEntities[0]).LogicalName.Equals("systemuser"))
                                    {
                                        EntityReference systemuser = relatedEntities[0];
                                        Entity team = service.Retrieve(targetEntity.LogicalName, targetEntity.Id, new ColumnSet("businessunitid", "jarvis_membersecurityroles"));
                                        if (team.Attributes.Contains("businessunitid") && team.Attributes["businessunitid"] != null)
                                        {
                                            EntityReference businessUnit = (EntityReference)team.Attributes["businessunitid"];
                                            if (team.Attributes.Contains("jarvis_membersecurityroles") && team.Attributes["jarvis_membersecurityroles"] != null)
                                            {
                                                string memberRoles = (string)team.Attributes["jarvis_membersecurityroles"];
                                                if (memberRoles != string.Empty)
                                                {
                                                    TeamBusinessProcess businessProcess = new TeamBusinessProcess();
                                                    businessProcess.AssignMemberRoles(systemuser, businessUnit, memberRoles, service);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                // endregion
                // region Remove Users
                if (context.MessageName == "Disassociate")
                {
                    if (context.InputParameters.Contains("Relationship"))
                    {
                        string relationshipName = ((Relationship)context.InputParameters["Relationship"]).SchemaName;
                        if (relationshipName.Equals(Constants.Team.teammembership, StringComparison.CurrentCultureIgnoreCase))
                        {
                            EntityReference targetEntity = (EntityReference)context.InputParameters["Target"];
                            if (targetEntity.LogicalName.Equals("team"))
                            {
                                if (context.InputParameters.Contains("RelatedEntities") && context.InputParameters["RelatedEntities"] is EntityReferenceCollection)
                                {
                                    var relatedEntities = context.InputParameters["RelatedEntities"] as EntityReferenceCollection;
                                    if (relatedEntities.Count > 0 && ((EntityReference)relatedEntities[0]).LogicalName.Equals("systemuser"))
                                    {
                                        EntityReference systemuser = relatedEntities[0];
                                        Entity team = service.Retrieve(targetEntity.LogicalName, targetEntity.Id, new ColumnSet("businessunitid", "jarvis_membersecurityroles"));
                                        if (team.Attributes.Contains("businessunitid") && team.Attributes["businessunitid"] != null)
                                        {
                                            EntityReference businessUnit = (EntityReference)team.Attributes["businessunitid"];
                                            if (team.Attributes.Contains("jarvis_membersecurityroles") && team.Attributes["jarvis_membersecurityroles"] != null)
                                            {
                                                EntityCollection getRoles = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getUserMemberRoles, systemuser.Id)));
                                                if (getRoles != null && getRoles.Entities.Count > 0)
                                                {
                                                    List<string> roleCollection = new List<string>();
                                                    foreach (var item in getRoles.Entities)
                                                    {
                                                        roleCollection.Add(getRoles.Entities[0].Id.ToString());
                                                    }

                                                    string memberRoles = (string)team.Attributes["jarvis_membersecurityroles"];
                                                    if (memberRoles != string.Empty)
                                                    {
                                                        TeamBusinessProcess businessProcess = new TeamBusinessProcess();
                                                        string[] splitString = memberRoles.Split(',');
                                                        foreach (string str in splitString)
                                                        {
                                                            businessProcess.RemoveMemberRoles(systemuser, businessUnit, str, roleCollection, service);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                // endregion
            }
            catch (InvalidPluginExecutionException ex)
            {
                tracingService.Trace(ex.Message);
                tracingService.Trace(ex.StackTrace);
                throw new InvalidPluginExecutionException("Error in Case Monitor Operations " + ex.Message);
            }
        }
    }
}
