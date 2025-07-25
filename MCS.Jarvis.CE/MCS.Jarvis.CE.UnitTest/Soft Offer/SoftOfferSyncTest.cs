//-----------------------------------------------------------------------
// <copyright file="SoftOfferSyncTest.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace MCS.Jarvis.CE.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using MCS.Jarvis.CE.Plugins;
    using MCS.Jarvis.CE.Plugins.SoftOffer;
    using MCS.Jarvis.IntegrationPlugin;
    using Microsoft.Crm.Sdk.Messages;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.Xrm.Sdk;
    using static MCS.Jarvis.CE.BusinessProcessesShared.Helpers.Constants;
    using static MCS.Jarvis.CE.Plugins.Constants;

    /// <summary>
    /// Soft Offer Sync Test.
    /// </summary>
    [TestClass]
    public class SoftOfferSyncTest : UnitTestBase
    {
        /// <summary>
        /// Positive scenario with Soft Offer Sync Create Scenario.
        /// Contact Should not be bull.
        /// </summary>
        [TestMethod]
        public void SoftOfferSyncCreateTest()
        {
            var softOffer = new Entity("jarvis_softoffer");
            softOffer.Id = Guid.NewGuid();
            softOffer["jarvis_startdate"] = DateTime.UtcNow;
            softOffer["jarvis_expirydate"] = DateTime.UtcNow;

            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", softOffer),
               };
            this.PluginExecutionContext.MessageNameGet = () => "Create";
            this.PluginExecutionContext.StageGet = () => 40;

            this.Service.ExecuteOrganizationRequest = processInstanceRequest =>
            {
                return new LocalTimeFromUtcTimeResponse();
            };

            SoftOfferSync plugin = new SoftOfferSync();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Positive scenario with Soft Offer Sync Update Scenario.
        /// </summary>
        [TestMethod]
        public void SoftOfferSyncUpdateTest()
        {
            var softOffer = new Entity("jarvis_softoffer");
            softOffer.Id = Guid.NewGuid();
            softOffer["jarvis_startdate"] = DateTime.UtcNow;
            softOffer["jarvis_expirydate"] = DateTime.UtcNow;

            var preImageSoftOffer = new Entity("jarvis_softoffer");
            preImageSoftOffer.Id = Guid.NewGuid();
            preImageSoftOffer["jarvis_startdate"] = DateTime.UtcNow;
            preImageSoftOffer["jarvis_expirydate"] = DateTime.UtcNow;

            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", softOffer),
               };
            this.PluginExecutionContext.MessageNameGet = () => "Update";
            this.PluginExecutionContext.StageGet = () => 20;

            this.PluginExecutionContext.PreEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PreImage", preImageSoftOffer },
                };
            };

            this.Service.ExecuteOrganizationRequest = processInstanceRequest =>
            {
                return new LocalTimeFromUtcTimeResponse();
            };

            SoftOfferSync plugin = new SoftOfferSync();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Positive scenario with Soft Offer Sync Update and Deactivate Scenario.
        /// </summary>
        [TestMethod]
        public void SoftOfferSyncUpdateDeactivateTest()
        {
            var softOffer = new Entity("jarvis_softoffer");
            softOffer.Id = Guid.NewGuid();
            DateTime presentDateTime = DateTime.UtcNow;
            ////softOffer["jarvis_startdate"] = presentDateTime.AddDays(-7);
            ////softOffer["jarvis_expirydate"] = presentDateTime.AddDays(-5);

            var preImageSoftOffer = new Entity("jarvis_softoffer");
            preImageSoftOffer.Id = Guid.NewGuid();
            preImageSoftOffer["jarvis_startdate"] = presentDateTime.AddDays(-7);
            preImageSoftOffer["jarvis_expirydate"] = presentDateTime.AddDays(-5);

            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", softOffer),
               };
            this.PluginExecutionContext.MessageNameGet = () => "Update";
            this.PluginExecutionContext.StageGet = () => 20;

            this.PluginExecutionContext.PreEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PreImage", preImageSoftOffer },
                };
            };

            this.Service.ExecuteOrganizationRequest = processInstanceRequest =>
            {
                return new LocalTimeFromUtcTimeResponse();
            };

            SoftOfferSync plugin = new SoftOfferSync();
            plugin.Execute(this.ServiceProvider);
        }
    }
}
