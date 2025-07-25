// <copyright file="CaseResolutionSyncTest.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MCS.Jarvis.CE.UnitTest
{
    using System;
    using System.Collections.Generic;
    using MCS.Jarvis.CE.Plugins;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;

    /// <summary>
    /// CaseResolutionsync Test.
    /// </summary>
    [TestClass]
    public class CaseResolutionSyncTest : UnitTestBase
    {
        /// <summary>
        /// Positive Scenario Resolution type 5.
        /// </summary>
        [TestMethod]
        public void CaseResolutionSyncTestUpdate_ResolutionType5()
        {
            //// Setting Input Parameters.
            Entity incidentReso = new Entity("incidentresolutions");
            incidentReso.Id = Guid.NewGuid();
            incidentReso["incidentid"] = new EntityReference("incident", Guid.NewGuid());
            incidentReso["resolutiontypecode"] = new OptionSetValue(5);
            incidentReso["casetypecode"] = new OptionSetValue(2);

            var postImg = new Entity("incidentresolutions");
            postImg.Attributes["incidentid"] = new EntityReference("incident", Guid.NewGuid());
            postImg["casetypecode"] = new OptionSetValue(2);

            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", incidentReso),
               };
            //// Setting Post Entity Image.
            this.PluginExecutionContext.PostEntityImagesGet = () =>
            {
                return new EntityImageCollection
                 {
                        { "PostImage", postImg },
                 };
            };
            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "incident")
                {
                    Entity inc = new Entity("incident");
                    inc.Id = guid;
                    inc["casetypecode"] = new OptionSetValue(2);
                    return inc;
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
                }

                return result;
            };
            CaseResolutionSync plugin = new CaseResolutionSync();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Positive Scenario Resolution type 5.
        /// </summary>
        [TestMethod]
        public void CaseResolutionSyncTestUpdate_ResolutionType1000()
        {
            //// Setting Input Parameters.
            Entity incidentReso = new Entity("incidentresolutions");
            incidentReso.Id = Guid.NewGuid();
            incidentReso["incidentid"] = new EntityReference("incident", Guid.NewGuid());
            incidentReso["resolutiontypecode"] = new OptionSetValue(1000);
            incidentReso["casetypecode"] = new OptionSetValue(2);

            var postImg = new Entity("incidentresolutions");
            postImg.Attributes["incidentid"] = new EntityReference("incident", Guid.NewGuid());
            postImg["casetypecode"] = new OptionSetValue(2);

            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", incidentReso),
               };
            //// Setting Post Entity Image.
            this.PluginExecutionContext.PostEntityImagesGet = () =>
            {
                return new EntityImageCollection
                 {
                        { "PostImage", postImg },
                 };
            };
            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "incident")
                {
                    Entity inc = new Entity("incident");
                    inc.Id = guid;
                    inc["casetypecode"] = new OptionSetValue(2);
                    return inc;
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
                }

                return result;
            };
            CaseResolutionSync plugin = new CaseResolutionSync();
            plugin.Execute(this.ServiceProvider);
        }
    }
}
