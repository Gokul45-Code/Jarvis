//-----------------------------------------------------------------------
// <copyright file="UserUpdatePostOperationUserProfileAsyncTest.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace MCS.Jarvis.CE.UnitTest
{
    using System;
    using System.Collections.Generic;
    using MCS.Jarvis.CE.Plugins;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;

    /// <summary>
    /// UserUpdate PostOperationUserProfileAsyncTest.
    /// </summary>
    [TestClass]
    public class UserUpdatePostOperationUserProfileAsyncTest : UnitTestBase
    {
        /// <summary>
        /// UserUpdatePostOperationUserProfileAsync WhenUpdateMessage ShouldCallUserOperationWithCorrectParameters.
        /// </summary>
        [TestMethod]
        public void UserUpdatePostOperationUserProfileAsync_WhenUpdateMessage_ShouldCallUserOperationWithCorrectParameters()
        {
            // Arrange
            var targetEntity = new Entity("systemuser");
            var preImageEntity = new Entity("systemuser");

            PluginExecutionContext.InputParametersGet = () => new ParameterCollection { new KeyValuePair<string, object>("Target", targetEntity) };
            this.PluginExecutionContext.MessageNameGet = () => "Update";

            this.PluginExecutionContext.PreEntityImagesGet = () => new EntityImageCollection { new KeyValuePair<string, Entity>("PreUserImage", preImageEntity) };

            // Set up active cases
            var activeCases = new EntityCollection();
            var case1 = new Entity("incident");
            case1.Id = Guid.NewGuid();
            case1[Case.Attributes.Customer] = new EntityReference("account", Guid.NewGuid());
            case1[Case.Attributes.HomeDealer] = new EntityReference("account", Guid.NewGuid());
            case1[Case.Attributes.BreakdownCountry] = new EntityReference("jarvis_country", Guid.NewGuid());
            activeCases.Entities.Add(case1);

            this.Service.RetrieveMultipleQueryBase = (query) =>
            {
                if (query is FetchExpression fetch)
                {
                    return activeCases;
                }

                return null;
            };

            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "account")
                {
                    Entity account = new Entity("account");
                    account.Id = guid;
                    account["jarvis_address1_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    return account;
                }

                return null;
            };

            UserUpdatePostOperationUserProfileAsync plugin = new UserUpdatePostOperationUserProfileAsync();

            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// UserUpdatePostOperationUserProfileAsync with depth 2.
        /// </summary>
        [TestMethod]
        public void UserUpdatePostOperationUserProfileAsync_WhenDepthTwo()
        {
            // Arrange
            var targetEntity = new Entity("systemuser");
            var preImageEntity = new Entity("systemuser");

            PluginExecutionContext.InputParametersGet = () => new ParameterCollection { new KeyValuePair<string, object>("Target", targetEntity) };
            this.PluginExecutionContext.MessageNameGet = () => "Update";
            this.PluginExecutionContext.DepthGet = () => 2;

            this.PluginExecutionContext.PreEntityImagesGet = () => new EntityImageCollection { new KeyValuePair<string, Entity>("PreUserImage", preImageEntity) };

            // Set up active cases
            var activeCases = new EntityCollection();
            var case1 = new Entity("incident");
            case1.Id = Guid.NewGuid();
            case1[Case.Attributes.Customer] = new EntityReference("account", Guid.NewGuid());
            case1[Case.Attributes.HomeDealer] = new EntityReference("account", Guid.NewGuid());
            case1[Case.Attributes.BreakdownCountry] = new EntityReference("jarvis_country", Guid.NewGuid());
            activeCases.Entities.Add(case1);

            UserUpdatePostOperationUserProfileAsync plugin = new UserUpdatePostOperationUserProfileAsync();

            plugin.Execute(this.ServiceProvider);
        }
    }
}