// -----------------------------------------------------------------------
// <copyright file="RemarksPreCreateSyncTest.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace MCS.Jarvis.CE.UnitTest
{
    using System;
    using System.Collections.Generic;
    using MCS.Jarvis.CE.Plugins;
    using MCS.Jarvis.CE.Plugins.Remarks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;
    using static MCS.Jarvis.CE.BusinessProcessesShared.Helpers.Constants;

    /// <summary>
    /// Remarks PreCreateOperationSyncTest.
    /// </summary>
    [TestClass]
    public class RemarksPreCreateSyncTest : UnitTestBase
    {
        /// <summary>
        /// Remarks Pre Create Sync Method.
        /// </summary>
        [TestMethod]
        public void RemarksPreCreateSyncMethod()
        {
            //// Setting Input Parameters.
            Entity post = new Entity("post");
            post.Id = Guid.NewGuid();
            post["source"] = new OptionSetValue(2);
            post["regardingobjectid"] = new EntityReference("incident", Guid.NewGuid());
            post["largetext"] = "Subject Large Text";

            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", post),
               };
            this.PluginExecutionContext.MessageNameGet = () => "Create";
            this.PluginExecutionContext.StageGet = () => 40;

            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();
                if (query.GetType().Name == "FetchExpression")
                {
                    if ((query as FetchExpression).Query.Contains("<entity name='post'>"))
                    {
                        Entity postEntity = new Entity("post");
                        postEntity.Id = Guid.NewGuid();
                        result.Entities.Add(postEntity);
                    }
                }

                return result;
            };

            RemarksPreCreateSync plugin = new RemarksPreCreateSync();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Remarks Pre Create Sync Method.
        /// </summary>
        [TestMethod]
        public void RemarksPreCreateSyncNoLargeText()
        {
            //// Setting Input Parameters.
            Entity post = new Entity("post");
            post.Id = Guid.NewGuid();
            post["source"] = new OptionSetValue(2);
            post["regardingobjectid"] = new EntityReference("incident", Guid.NewGuid());

            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", post),
               };
            this.PluginExecutionContext.MessageNameGet = () => "Create";
            this.PluginExecutionContext.StageGet = () => 40;

            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();
                if (query.GetType().Name == "FetchExpression")
                {
                    if ((query as FetchExpression).Query.Contains("<entity name='post'>"))
                    {
                        Entity postEntity = new Entity("post");
                        postEntity.Id = Guid.NewGuid();
                        result.Entities.Add(postEntity);
                    }
                }

                return result;
            };

            RemarksPreCreateSync plugin = new RemarksPreCreateSync();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Remarks Pre Create Sync Method with Exception.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(NullReferenceException))]
        public void RemarksPreCreateSyncException()
        {
            //// Setting Input Parameters.
            Entity post = new Entity("post");
            post.Id = Guid.NewGuid();
            post["source"] = new OptionSetValue(2);
            post["regardingobjectid"] = new EntityReference("incident", Guid.NewGuid());

            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", post),
               };
            this.PluginExecutionContext.MessageNameGet = () => "Create";
            this.PluginExecutionContext.StageGet = () => 40;

            RemarksPreCreateSync plugin = new RemarksPreCreateSync();
            plugin.Execute(this.ServiceProvider);
        }
    }
}
