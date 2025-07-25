// <copyright file="PickReleaseQueueItemTest.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace MCS.Jarvis.CE.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using MCS.Jarvis.CE.IntegrationPlugin;
    using MCS.Jarvis.CE.Plugins.Actions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.Xrm.Sdk;

    /// <summary>
    /// PickReleaseQueueItemTest Class.
    /// </summary>
    [TestClass]
    public class PickReleaseQueueItemTest : UnitTestBase
    {
        /// <summary>
        /// Positive Scenario but due to object is null we will get Expected Exception.
        /// </summary>
        [TestMethod]
        public void PickReleaseQueueItemAssignTest()
        {
            Guid queueItemId = Guid.NewGuid();
            Guid userId = Guid.NewGuid();
            bool pickOrRelease = true;

            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("QueueItemID", queueItemId),
                   new KeyValuePair<string, object>("UserID", userId),
                   new KeyValuePair<string, object>("PickOrRelease", pickOrRelease),
               };

            PickReleaseQueueItem plugin = new PickReleaseQueueItem();
            plugin.Execute(this.ServiceProvider);
        }
    }
}
