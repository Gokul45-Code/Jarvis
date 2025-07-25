// <copyright file="TeamBusinessProcess.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace MCS.jarvis.CE.BusinessProcessShared.MemberAssociation
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Web.Security;
    using MCS.Jarvis.CE.Plugins;
    using Microsoft.Crm.Sdk.Messages;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Client;
    using Microsoft.Xrm.Sdk.Query;
    using static MCS.Jarvis.CE.BusinessProcessesShared.Helpers.Constants;

    /// <summary>
    /// Team Business Process.
    /// </summary>
    public class TeamBusinessProcess
    {
        /// <summary>
        /// Assign Member Roles.
        /// </summary>
        /// <param name="systemuser">system user.</param>
        /// <param name="teamBusinessunit">team business unit.</param>
        /// <param name="memberRoles">member Roles.</param>
        /// <param name="service">Org service.</param>
        public void AssignMemberRoles(EntityReference systemuser, EntityReference teamBusinessunit, string memberRoles, IOrganizationService service)
        {
            Entity user = service.Retrieve(systemuser.LogicalName, systemuser.Id, new ColumnSet("businessunitid"));
            if (user.Attributes.Contains("businessunitid") && user.Attributes["businessunitid"] != null)
            {
                EntityReference userBusinessunit = (EntityReference)user.Attributes["businessunitid"];
                if (userBusinessunit.Id != teamBusinessunit.Id)
                {
#pragma warning disable SA1123 // Do not place regions within elements
                    #region Reassign User business unit

                    SetBusinessSystemUserRequest changeUserBURequest = new SetBusinessSystemUserRequest();
#pragma warning restore SA1123 // Do not place regions within elements
                    changeUserBURequest.BusinessId = teamBusinessunit.Id;
                    changeUserBURequest.UserId = systemuser.Id;
                    changeUserBURequest.ReassignPrincipal = new EntityReference(systemuser.LogicalName, systemuser.Id);
                    service.Execute(changeUserBURequest);

                    #endregion
                }

#pragma warning disable SA1123 // Do not place regions within elements
                #region Assign Member Roles
                string[] splitString = memberRoles.Split(',');
#pragma warning restore SA1123 // Do not place regions within elements
                EntityCollection getRoles = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getUserMemberRoles, systemuser.Id)));
                List<string> roleCollection = new List<string>();
                if (getRoles != null && getRoles.Entities.Count > 0)
                {
                    foreach (var item in getRoles.Entities)
                    {
                        roleCollection.Add(getRoles.Entities[0].Id.ToString());
                    }
                }

                foreach (string str in splitString)
                {
                    this.AssociateRoles(systemuser, teamBusinessunit, str, roleCollection, service);
                }
                #endregion
            }
        }

        /// <summary>
        /// Associate Roles.
        /// </summary>
        /// <param name="systemuser">system user.</param>
        /// <param name="businessunit">business unit.</param>
        /// <param name="role">role value.</param>
        /// <param name="roleList">role List.</param>
        /// <param name="service">Org service.</param>
        public void AssociateRoles(EntityReference systemuser, EntityReference businessunit, string role, List<string> roleList, IOrganizationService service)
        {
            EntityCollection getRoles = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getTeamMemberRoles, businessunit.Id, role)));
            if (getRoles != null && getRoles.Entities.Count > 0)
            {
                EntityReference roles = (EntityReference)getRoles.Entities[0].ToEntityReference();
                bool alreadyExist = roleList.Contains(roles.Id.ToString());

                // Associate the user with the role.
                if (roles.Id != Guid.Empty && systemuser.Id != Guid.Empty && alreadyExist == false)
                {
                    service.Associate(
                                "systemuser",
                                systemuser.Id,
                                new Relationship("systemuserroles_association"),
                                new EntityReferenceCollection() { new EntityReference(roles.LogicalName, roles.Id) });
                }
            }
        }

        /// <summary>
        /// Remove Member Roles.
        /// </summary>
        /// <param name="systemuser">system user.</param>
        /// <param name="teamBusinessunit">business unit.</param>
        /// <param name="memberRoles">role value.</param>
        /// <param name="roleCollection">role Collection.</param>
        /// <param name="service">Org service.</param>
        public void RemoveMemberRoles(EntityReference systemuser, EntityReference teamBusinessunit, string memberRoles, List<string> roleCollection, IOrganizationService service)
        {
            EntityCollection getTeamRoles = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getTeamMemberRoles, teamBusinessunit.Id, memberRoles)));
            if (getTeamRoles != null && getTeamRoles.Entities.Count > 0)
            {
                EntityReference teamrole = (EntityReference)getTeamRoles.Entities[0].ToEntityReference();

                bool alreadyExist = roleCollection.Contains(teamrole.Id.ToString());
                service.Disassociate(
                                  "systemuser",
                                  systemuser.Id,
                                  new Relationship("systemuserroles_association"),
                                  new EntityReferenceCollection() { new EntityReference("role", teamrole.Id) });
            }
        }
    }
}
