// <copyright file="AccountPreOperationSyncTest.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MCS.Jarvis.CE.UnitTest
{
    using System;
    using System.Collections.Generic;
    using MCS.Jarvis.CE.Plugins;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.Xrm.Sdk;

    /// <summary>
    /// Account Pre Operation Sync Test.
    /// </summary>
    [TestClass]
    public class AccountPreOperationSyncTest : UnitTestBase
    {
        /// <summary>
        /// Postive Scenario for Account Pre Operation on create a business partner.
        /// </summary>
        [TestMethod]
        public void AccountPreOperationTestCreate()
        {
            Entity account = new Entity("account", Guid.NewGuid());
            account["jarvis_address1_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            account["name"] = "test account";
            account["accountnumber"] = "1212121";
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", account),
               };
            this.PluginExecutionContext.MessageNameGet = () => "CREATE";
            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "jarvis_country")
                {
                    Entity country = new Entity("jarvis_country");
                    country.Id = guid;
                    country["jarvis_iso2countrycode"] = "NL";
                    country.Attributes["jarvis_timezone"] = "105";
                    return country;
                }

                return null;
            };
            AccountPreOperationSync plugin = new AccountPreOperationSync();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Postive Scenario for Account Pre Operation on update of a business partner.
        /// </summary>
        [TestMethod]
        public void AccountPreOperationTestUpdate()
        {
            Entity account = new Entity("account", Guid.NewGuid());
            account["jarvis_address1_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            account["name"] = "test account";
            account["accountnumber"] = "1212121";
            Entity preImagAccount = new Entity("account", Guid.NewGuid());
            preImagAccount["jarvis_address1_country"] = new EntityReference("jarvis_country", Guid.NewGuid());

            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", account),
               };
            this.PluginExecutionContext.PreEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PreImage", preImagAccount },
                };
            };
            this.PluginExecutionContext.MessageNameGet = () => "UPDATE";
            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "jarvis_country")
                {
                    Entity country = new Entity("jarvis_country");
                    country.Id = guid;
                    country["jarvis_iso2countrycode"] = "NL";
                    country.Attributes["jarvis_timezone"] = "105";
                    return country;
                }

                return null;
            };
            AccountPreOperationSync plugin = new AccountPreOperationSync();
            plugin.Execute(this.ServiceProvider);
        }
    }
}
