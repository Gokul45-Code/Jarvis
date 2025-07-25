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
    using Microsoft.Xrm.Sdk.Query;

    /// <summary>
    /// CaseCreatePostOperationUserProfileSyncTest Test.
    /// </summary>
    [TestClass]
    public class CaseCreatePostOperationUserProfileSyncTest : UnitTestBase
    {
        /// <summary>
        /// Post Create With Case Business Partner Not Null.
        /// </summary>
        [TestMethod]
        public void PostCreateWithCaseBusinessPartnerNotNull()
        {
            Entity incident = new Entity("incident");
            incident.Id = Guid.Empty;
            incident["customerid"] = new EntityReference("account", Guid.NewGuid());
            incident["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incident["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["casetypecode"] = new OptionSetValue(2);
            incident["jarvis_caselocation"] = new OptionSetValue(1);
            incident["jarvis_caseserviceline"] = new EntityReference("jarivs_serviceline", Guid.NewGuid());

            //// Setting Input Parameters.
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", incident),
               };
            this.PluginExecutionContext.MessageNameGet = () => "Create";
            this.PluginExecutionContext.StageGet = () => 40;

            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "account")
                {
                    Entity client = new Entity("account");
                    client.Id = guid;
                    client["jarvis_address1_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    client["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    return client;
                }

                return null;
            };

            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();
                if (query.GetType().Name == "FetchExpression")
                {
                    if (query is FetchExpression fetchExpression && fetchExpression.Query.Contains("<entity name='jarvis_teamprofiledetails'>"))
                    {
                        var teamprofile = new Entity("jarvis_teamprofiledetails");
                        teamprofile.Id = Guid.NewGuid();
                        result.Entities.Add(teamprofile);
                    }
                }

                if (query.GetType().Name == "FetchExpression")
                {
                    if (query is FetchExpression fetchExpression && fetchExpression.Query.Contains("<entity name='jarvis_caseuser'>"))
                    {
                        var caseUser = new Entity("jarvis_caseuser");
                        caseUser.Id = Guid.NewGuid();
                        result.Entities.Add(caseUser);
                    }
                }

                return result;
            };

            CaseCreatePostOperationUserProfileSync plugin = new CaseCreatePostOperationUserProfileSync();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Post Create With Case Exception.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidPluginExecutionException))]
        public void PostCreateWithCaseException()
        {
            Entity incident = new Entity("incident");
            incident.Id = Guid.Empty;
            incident["customerid"] = new EntityReference("account", Guid.NewGuid());
            incident["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incident["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["casetypecode"] = new OptionSetValue(2);
            //incident["jarvis_caselocation"] = new OptionSetValue(1);
            incident["jarvis_caseserviceline"] = new EntityReference("jarivs_serviceline", Guid.NewGuid());

            //// Setting Input Parameters.
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", incident),
               };
            this.PluginExecutionContext.MessageNameGet = () => "Create";
            this.PluginExecutionContext.StageGet = () => 40;

            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "account")
                {
                    Entity client = new Entity("account");
                    client.Id = guid;
                    client["jarvis_address1_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    client["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    return client;
                }

                return null;
            };

            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();
                if (query.GetType().Name == "FetchExpression")
                {
                    if (query is FetchExpression fetchExpression && fetchExpression.Query.Contains("<entity name='jarvis_teamprofiledetails'>"))
                    {
                        var teamprofile = new Entity("jarvis_teamprofiledetails");
                        teamprofile.Id = Guid.NewGuid();
                        result.Entities.Add(teamprofile);
                    }
                }

                if (query.GetType().Name == "FetchExpression")
                {
                    if (query is FetchExpression fetchExpression && fetchExpression.Query.Contains("<entity name='jarvis_caseuser'>"))
                    {
                        var caseUser = new Entity("jarvis_caseuser");
                        caseUser.Id = Guid.NewGuid();
                        result.Entities.Add(caseUser);
                    }
                }

                return result;
            };

            CaseCreatePostOperationUserProfileSync plugin = new CaseCreatePostOperationUserProfileSync();
            plugin.Execute(this.ServiceProvider);
        }
    }
}
