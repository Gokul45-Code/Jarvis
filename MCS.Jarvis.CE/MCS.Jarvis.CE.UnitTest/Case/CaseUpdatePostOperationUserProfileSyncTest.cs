//-----------------------------------------------------------------------
// <copyright file="CaseUpdatePostOperationUserProfileSyncTest.cs" company="Microsoft">
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

    /// <summary>
    /// CaseUpdatePostOperationUserProfileSync Test.
    /// </summary>
    [TestClass]
    public class CaseUpdatePostOperationUserProfileSyncTest : UnitTestBase
    {
        /// <summary>
        /// PreValidate Update With CaseBusinessPartner Entity not null.
        /// </summary>
        [TestMethod]
        public void PostUpdateWithCaseBusinessPartnerNotNull()
        {
            Entity incident = new Entity("incident");
            incident.Id = Guid.Empty;
            incident["statuscode"] = new OptionSetValue(10);
            incident["caseorigincode"] = new OptionSetValue(2);
            incident["jarvis_mercuriusstatus"] = new OptionSetValue(200);
            incident["customerid"] = new EntityReference("account", Guid.NewGuid());

            Entity preEntityImage = new Entity("incident");
            preEntityImage["customerid"] = new EntityReference("account", Guid.NewGuid());

            //// Setting Input Parameters.
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", incident),
               };
            this.PluginExecutionContext.MessageNameGet = () => "Update";
            this.PluginExecutionContext.StageGet = () => 40;
            //// Setting Post and Pre Image
            this.PluginExecutionContext.PreEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PreImage", preEntityImage },
                };
            };

            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "account")
                {
                    Entity client = new Entity("account");
                    client.Id = guid;
                    client["jarvis_address1_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    return client;
                }

                return null;
            };
            CaseUpdatePostOperationUserProfileSync plugin = new CaseUpdatePostOperationUserProfileSync();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// PreValidate Update With CaseHomeDealer NotNull.
        /// </summary>
        [TestMethod]
        public void PostUpdateWithcaseHomeDealerNotNull()
        {
            Entity incident = new Entity("incident");
            incident.Id = Guid.Empty;
            incident["statuscode"] = new OptionSetValue(10);
            incident["caseorigincode"] = new OptionSetValue(2);
            incident["jarvis_mercuriusstatus"] = new OptionSetValue(200);
            incident["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
            incident["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());

            Entity preEntityImage = new Entity("incident");
            preEntityImage["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());

            //// Setting Input Parameters.
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", incident),
               };
            this.PluginExecutionContext.MessageNameGet = () => "Update";
            this.PluginExecutionContext.StageGet = () => 40;
            //// Setting Post and Pre Image
            this.PluginExecutionContext.PreEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PreImage", preEntityImage },
                };
            };

            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "account")
                {
                    Entity client = new Entity("account");
                    client.Id = guid;
                    client["jarvis_address1_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    return client;
                }

                return null;
            };
            CaseUpdatePostOperationUserProfileSync plugin = new CaseUpdatePostOperationUserProfileSync();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// PreValidate Update With CaseBreakDownCountry NotNull.
        /// </summary>
        [TestMethod]
        public void PostUpdateWithcaseBreakdownCountryNotNull()
        {
            Entity incident = new Entity("incident");
            incident.Id = Guid.Empty;
            incident["statuscode"] = new OptionSetValue(10);
            incident["caseorigincode"] = new OptionSetValue(2);
            incident["jarvis_mercuriusstatus"] = new OptionSetValue(200);
            incident["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
            incident["jarvis_country"] = new EntityReference("account", Guid.NewGuid());

            Entity preEntityImage = new Entity("incident");
            preEntityImage["jarvis_country"] = new EntityReference("account", Guid.NewGuid());

            //// Setting Input Parameters.
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", incident),
               };
            this.PluginExecutionContext.MessageNameGet = () => "Update";
            this.PluginExecutionContext.StageGet = () => 40;
            //// Setting Post and Pre Image
            this.PluginExecutionContext.PreEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PreImage", preEntityImage },
                };
            };
            CaseUpdatePostOperationUserProfileSync plugin = new CaseUpdatePostOperationUserProfileSync();
            plugin.Execute(this.ServiceProvider);
        }
    }
}