//-----------------------------------------------------------------------
// <copyright file="NotesPreCreateSyncTest.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace MCS.Jarvis.CE.UnitTest
{
    using System;
    using System.Collections.Generic;
    using MCS.Jarvis.CE.Plugins.Notes;
    using Microsoft.Crm.Sdk.Messages;
    using Microsoft.Crm.Sdk.Messages.Fakes;
    using Microsoft.QualityTools.Testing.Fakes;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;
    using static MCS.Jarvis.CE.BusinessProcessesShared.Helpers.Constants;

    /// <summary>
    /// NotesPostCreateSync Test.
    /// </summary>
    [TestClass]
    public class NotesPreCreateSyncTest : UnitTestBase
    {
        /// <summary>
        /// Positive Scenario for Notes.
        /// </summary>
        [TestMethod]
        public void NotesPreCreateSyncTestMethod()
        {
            var note = new Entity("annotation");
            note["objectid"] = new EntityReference("incident", Guid.NewGuid());
            note["subject"] = "subject";

            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", note),
               };

            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();

                if (query is FetchExpression fetchExpression && fetchExpression.Query.Contains("<entity name='incident'>"))
                {
                    var caseObj = new Entity("incident");
                    caseObj.Id = Guid.NewGuid();
                    result.Entities.Add(caseObj);
                }

                return result;
            };

            NotesPreCreateSync plugin = new NotesPreCreateSync();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Positive Scenario for Notes.
        /// </summary>
        [TestMethod]
        public void NotesPreCreateSyncTestMethod1()
        {
            var note = new Entity("annotation");
            note["objectid"] = new EntityReference("incident", Guid.NewGuid());
            //note["subject"] = "subject";

            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", note),
               };

            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();

                //if (query is FetchExpression fetchExpression && fetchExpression.Query.Contains("<entity name='incident'>"))
                //{
                //    var caseObj = new Entity("incident");
                //    caseObj.Id = Guid.NewGuid();
                //}

                return result;
            };

            NotesPreCreateSync plugin = new NotesPreCreateSync();
            plugin.Execute(this.ServiceProvider);
        }
    }
}
