using MCS.Jarvis.CE.Plugins.RepairInformation;
using MCS.Jarvis.CE.Plugins.Team;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCS.Jarvis.CE.UnitTest
{
    /// <summary>
    /// Add Remove Team Members Test.
    /// </summary>
    [TestClass]
    public class AddRemoveTeamMembersTest : UnitTestBase
    {
        /// <summary>
        /// Repair Information Sync Execute Create Test.
        /// </summary>
        [TestMethod]
        public void AddRemoveTeamMembersExecuteTestAssociate()
        {
            var teamInfo = new Entity("team");
            teamInfo.Id = Guid.NewGuid();
            teamInfo["businessunitid"] = new EntityReference("businessunit", Guid.NewGuid());
            teamInfo.Attributes["jarvis_membersecurityroles"] = "user";
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", new EntityReference("team", Guid.NewGuid())),
                   new KeyValuePair<string, object>("Relationship", new Relationship("teammembership_association")),
                   new KeyValuePair<string, object>("RelatedEntities", new EntityReferenceCollection{new EntityReference("systemuser", Guid.NewGuid()) }),
               };
            this.PluginExecutionContext.MessageNameGet = () => "Associate";
            this.PluginExecutionContext.StageGet = () => 40;

            // Setting service retrieve.
            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "systemuser")
                {
                    Entity user = new Entity("systemuser");
                    user.Id = Guid.NewGuid();
                    user["fullname"] = "Mercurius";
                    user["businessunitid"] = new EntityReference("businessunit", Guid.NewGuid());
                    return user;
                }

                if (entityName == "team")
                {
                    Entity teamInfoRecord = new Entity("team");
                    teamInfoRecord.Id = Guid.NewGuid();
                    teamInfoRecord["businessunitid"] = new EntityReference("businessunit", Guid.NewGuid());
                    teamInfoRecord.Attributes["jarvis_membersecurityroles"] = "user";
                    return teamInfoRecord;
                }

                return null;
            };
            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();
                if (query.GetType().Name == "FetchExpression")
                {
                    if ((query as FetchExpression).Query.Contains("<entity name='systemuser'>"))
                    {
                        Entity user = new Entity("systemuser");
                        user.Id = Guid.NewGuid();
                        user["fullname"] = "Mercurius";
                        result.Entities.Add(user);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='role'>"))
                    {
                        Entity user = new Entity("role");
                        user.Id = Guid.NewGuid();
                        user["name"] = "Mercurius";
                        result.Entities.Add(user);
                    }
                }

                return result;
            };
            AddRemoveTeamMembers plugin = new AddRemoveTeamMembers();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Repair Information Sync Execute Create Test.
        /// </summary>
        [TestMethod]
        public void AddRemoveTeamMembersExecuteTestDisassociate()
        {
            var teamInfo = new Entity("team");
            teamInfo.Id = Guid.NewGuid();
            teamInfo["businessunitid"] = new EntityReference("businessunit", Guid.NewGuid());
            teamInfo.Attributes["jarvis_membersecurityroles"] = "user";
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", new EntityReference("team", Guid.NewGuid())),
                   new KeyValuePair<string, object>("Relationship", new Relationship("teammembership_association")),
                   new KeyValuePair<string, object>("RelatedEntities", new EntityReferenceCollection{new EntityReference("systemuser", Guid.NewGuid()) }),
               };
            this.PluginExecutionContext.MessageNameGet = () => "Disassociate";
            this.PluginExecutionContext.StageGet = () => 40;

            // Setting service retrieve.
            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "systemuser")
                {
                    Entity user = new Entity("systemuser");
                    user.Id = Guid.NewGuid();
                    user["fullname"] = "Mercurius";
                    user["businessunitid"] = new EntityReference("businessunit", Guid.NewGuid());
                    return user;
                }

                if (entityName == "team")
                {
                    Entity teamInfoRecord = new Entity("team");
                    teamInfoRecord.Id = Guid.NewGuid();
                    teamInfoRecord["businessunitid"] = new EntityReference("businessunit", Guid.NewGuid());
                    teamInfoRecord.Attributes["jarvis_membersecurityroles"] = "user";
                    return teamInfoRecord;
                }

                return null;
            };
            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();
                if (query.GetType().Name == "FetchExpression")
                {
                    if ((query as FetchExpression).Query.Contains("<entity name='systemuser'>"))
                    {
                        Entity user = new Entity("systemuser");
                        user.Id = Guid.NewGuid();
                        user["fullname"] = "Mercurius";
                        result.Entities.Add(user);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='role'>"))
                    {
                        Entity user = new Entity("role");
                        user.Id = Guid.NewGuid();
                        user["name"] = "Mercurius";
                        result.Entities.Add(user);
                    }
                }

                return result;
            };
            AddRemoveTeamMembers plugin = new AddRemoveTeamMembers();
            plugin.Execute(this.ServiceProvider);
        }
    }
}
