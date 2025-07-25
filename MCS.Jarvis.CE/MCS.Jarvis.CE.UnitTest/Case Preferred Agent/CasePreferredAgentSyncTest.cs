// <copyright file="CasePreferredAgentSyncTest.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MCS.Jarvis.CE.UnitTest
{
    using System;
    using System.Collections.Generic;
    using MCS.Jarvis.CE.Plugins.Case_Preferred_Agent;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.Xrm.Sdk;

    /// <summary>
    /// Case Preferred Agent Sync Test.
    /// </summary>
    [TestClass]
    public class CasePreferredAgentSyncTest : UnitTestBase
    {
        /// <summary>
        /// Case Preferred Agent Sync Create.
        /// </summary>
        [TestMethod]
        public void CasePreferredAgentSync_Create()
        {
            var targetEntity = new Entity("jarvis_casepreferredagent");
            targetEntity.Attributes["jarvis_case"] = new EntityReference("incident", Guid.NewGuid());
            targetEntity.Attributes["jarvis_preferredvasoperator"] = new EntityReference("systemuser", Guid.NewGuid());
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection { new KeyValuePair<string, object>("Target", targetEntity) };
            this.PluginExecutionContext.MessageNameGet = () => "Create";
            this.PluginExecutionContext.StageGet = () => 20;
            this.PluginExecutionContext.UserIdGet = () => Guid.NewGuid();
            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "systemuser")
                {
                    Entity user = new Entity("systemuser");
                    user.Id = Guid.NewGuid();
                    user["fullname"] = "test";
                    return user;
                }

                if (entityName == "incident")
                {
                    Entity inc = new Entity("incident");
                    inc.Id = guid;
                    inc.Attributes["jarvis_preferredvasoperator"] = new EntityReference("systemuser", Guid.NewGuid());
                    return inc;
                }

                return null;
            };
            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();
                return result;
            };
            CasePreferredAgentSync plugin = new CasePreferredAgentSync();

            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Case Preferred Agent Sync Create Without Vas Operator.
        /// </summary>
        [TestMethod]
        public void CasePreferredAgentSync_Create_NoVasOperator()
        {
            var targetEntity = new Entity("jarvis_casepreferredagent");
            targetEntity.Attributes["jarvis_case"] = new EntityReference("incident", Guid.NewGuid());
            targetEntity.Attributes["jarvis_preferredvasoperator"] = new EntityReference("systemuser", Guid.NewGuid());
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection { new KeyValuePair<string, object>("Target", targetEntity) };
            this.PluginExecutionContext.MessageNameGet = () => "Create";
            this.PluginExecutionContext.StageGet = () => 20;
            this.PluginExecutionContext.UserIdGet = () => Guid.NewGuid();
            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "systemuser")
                {
                    Entity user = new Entity("systemuser");
                    user.Id = Guid.NewGuid();
                    user["fullname"] = "test";
                    return user;
                }

                if (entityName == "incident")
                {
                    Entity inc = new Entity("incident");
                    inc.Id = guid;
                    inc.Attributes["dealer"] = new EntityReference("account", Guid.NewGuid());
                    return inc;
                }

                return null;
            };
            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();
                return result;
            };
            CasePreferredAgentSync plugin = new CasePreferredAgentSync();

            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Case Preferred Agent Sync Delete.
        /// </summary>
        [TestMethod]
        public void CasePreferredAgentSync_Delete()
        {
            var targetEntity = new Entity("jarvis_casepreferredagent");
            targetEntity.Attributes["jarvis_case"] = new EntityReference("incident", Guid.NewGuid());
            targetEntity.Attributes["jarvis_preferredvasoperator"] = new EntityReference("systemuser", Guid.NewGuid());
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection { new KeyValuePair<string, object>("Target", new EntityReference("jarvis_casepreferredagent", Guid.NewGuid())) };
            this.PluginExecutionContext.MessageNameGet = () => "Delete";
            this.PluginExecutionContext.StageGet = () => 20;
            this.PluginExecutionContext.UserIdGet = () => Guid.NewGuid();

            // Setting pre-entity image.
            this.PluginExecutionContext.PreEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                    { "PreImage", targetEntity },
                };
            };
            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "systemuser")
                {
                    Entity user = new Entity("systemuser");
                    user.Id = Guid.NewGuid();
                    user["fullname"] = "test";
                    return user;
                }

                if (entityName == "incident")
                {
                    Entity inc = new Entity("incident");
                    inc.Id = guid;
                    inc.Attributes["jarvis_preferredvasoperator"] = "test";
                    return inc;
                }

                return null;
            };
            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();
                return result;
            };
            CasePreferredAgentSync plugin = new CasePreferredAgentSync();

            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Case Preferred Agent Sync Delete with multiple operators.
        /// </summary>
        [TestMethod]
        public void CasePreferredAgentSync_Delete_WithMultipleOperators()
        {
            var targetEntity = new Entity("jarvis_casepreferredagent");
            targetEntity.Attributes["jarvis_case"] = new EntityReference("incident", Guid.NewGuid());
            targetEntity.Attributes["jarvis_preferredvasoperator"] = new EntityReference("systemuser", Guid.NewGuid());
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection { new KeyValuePair<string, object>("Target", new EntityReference("jarvis_casepreferredagent", Guid.NewGuid())) };
            this.PluginExecutionContext.MessageNameGet = () => "Delete";
            this.PluginExecutionContext.StageGet = () => 20;
            this.PluginExecutionContext.UserIdGet = () => Guid.NewGuid();

            // Setting pre-entity image.
            this.PluginExecutionContext.PreEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                    { "PreImage", targetEntity },
                };
            };
            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "systemuser")
                {
                    Entity user = new Entity("systemuser");
                    user.Id = Guid.NewGuid();
                    user["fullname"] = "test";
                    return user;
                }

                if (entityName == "incident")
                {
                    Entity inc = new Entity("incident");
                    inc.Id = guid;
                    inc.Attributes["jarvis_preferredvasoperator"] = "test,test2";
                    return inc;
                }

                return null;
            };
            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();
                return result;
            };
            CasePreferredAgentSync plugin = new CasePreferredAgentSync();

            plugin.Execute(this.ServiceProvider);
        }
    }
}
