// -----------------------------------------------------------------------
// <copyright file="VehiclePreOperationSyncTest.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace MCS.Jarvis.CE.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Controls;
    using MCS.Jarvis.CE.Plugins;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;
    using static MCS.Jarvis.CE.BusinessProcessesShared.Helpers.Constants;

    /// <summary>
    /// Vehicle PreOperationSync Test.
    /// </summary>
    [TestClass]
    public class VehiclePreOperationSyncTest : UnitTestBase
    {
        /// <summary>
        /// VehiclePreOperationSync PreCreate WithValidData ShouldUpdateFields.
        /// </summary>
        [TestMethod]
        public void VehiclePreOperationSync_PreCreate_WithValidData_ShouldUpdateFields()
        {
            // Arrange
            var targetEntity = new Entity("vehicle");
            targetEntity.Attributes["jarvis_name"] = "ABC 123";
            targetEntity.Attributes["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            targetEntity.Attributes["jarvis_owningcustomer"] = new EntityReference("contact", Guid.NewGuid());
            targetEntity.Attributes["jarvis_usingcustomer"] = new EntityReference("contact", Guid.NewGuid());

            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection { new KeyValuePair<string, object>("Target", targetEntity) };
            this.PluginExecutionContext.MessageNameGet = () => "Create";
            this.PluginExecutionContext.StageGet = () => 20;

            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "account")
                {
                    Entity account = new Entity("account");
                    account.Id = Guid.NewGuid();
                    account["jarvis_onecasestatus"] = new OptionSetValue(334030000);
                    return account;
                }

                return null;
            };

            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();
                if (query.GetType().Name == "FetchExpression")
                {
                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_gop'>"))
                    {
                        Entity account = new Entity("account");
                        account.Id = Guid.NewGuid();
                        account["name"] = "Account name";
                        account["jarvis_language"] = new EntityReference("jarvis_language", Guid.NewGuid());
                        account["jarvis_address1_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                        result.Entities.Add(account);
                    }
                }

                return result;
            };

            VehiclePreOperationSync plugin = new VehiclePreOperationSync();

            plugin.Execute(this.ServiceProvider);

            // Assert
            Assert.IsTrue(targetEntity.Attributes.Contains("jarvis_registrationnumbershadow"));
            Assert.AreEqual("ABC123", targetEntity["jarvis_registrationnumbershadow"]);

            Assert.IsTrue(targetEntity.Attributes.Contains("jarvis_updatedregistrationnumber"));
            Assert.AreEqual("ABC 123", targetEntity["jarvis_updatedregistrationnumber"]);

            Assert.IsTrue(targetEntity.Attributes.Contains("jarvis_homedealer"));
            Assert.IsNotNull(targetEntity["jarvis_homedealer"]);

            Assert.IsTrue(targetEntity.Attributes.Contains("jarvis_updatedowningcustomer"));
            Assert.IsNotNull(targetEntity["jarvis_updatedowningcustomer"]);

            Assert.IsTrue(targetEntity.Attributes.Contains("jarvis_updatedusingcustomer"));
            Assert.IsNotNull(targetEntity["jarvis_updatedusingcustomer"]);
        }

        /// <summary>
        /// Vehicle Pre Operation Sync Pre Update With Valid Data Should Update Fields.
        /// </summary>
        [TestMethod]
        public void VehiclePreOperationSync_PreUpdate_WithValidData_ShouldUpdateFields()
        {
            // Arrange
            var targetEntity = new Entity("vehicle");
            targetEntity.Attributes["jarvis_name"] = "XYZ 789";
            targetEntity.Attributes["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            targetEntity.Attributes["jarvis_owningcustomer"] = new EntityReference("contact", Guid.NewGuid());
            targetEntity.Attributes["jarvis_usingcustomer"] = new EntityReference("contact", Guid.NewGuid());

            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection { new KeyValuePair<string, object>("Target", targetEntity) };
            this.PluginExecutionContext.MessageNameGet = () => "Update";
            this.PluginExecutionContext.StageGet = () => 20;
            this.PluginExecutionContext.PreEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PreImage", targetEntity },
                };
            };

            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "account")
                {
                    Entity account = new Entity("account");
                    account.Id = Guid.NewGuid();
                    account["jarvis_onecasestatus"] = new OptionSetValue(334030000);
                    return account;
                }

                return null;
            };

            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();
                if (query.GetType().Name == "FetchExpression")
                {
                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_gop'>"))
                    {
                        Entity account = new Entity("account");
                        account.Id = Guid.NewGuid();
                        account["name"] = "Account name";
                        account["jarvis_language"] = new EntityReference("jarvis_language", Guid.NewGuid());
                        account["jarvis_address1_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                        result.Entities.Add(account);
                    }
                }

                return result;
            };

            VehiclePreOperationSync plugin = new VehiclePreOperationSync();

            plugin.Execute(this.ServiceProvider);

            // Assert
            Assert.IsTrue(targetEntity.Attributes.Contains("jarvis_registrationnumbershadow"));
            Assert.AreEqual("XYZ789", targetEntity["jarvis_registrationnumbershadow"]);

            Assert.IsTrue(targetEntity.Attributes.Contains("jarvis_updatedregistrationnumber"));
            Assert.AreEqual("XYZ 789", targetEntity["jarvis_updatedregistrationnumber"]);

            Assert.IsTrue(targetEntity.Attributes.Contains("jarvis_homedealer"));
            Assert.IsNotNull(targetEntity["jarvis_homedealer"]);

            Assert.IsTrue(targetEntity.Attributes.Contains("jarvis_updatedowningcustomer"));
            Assert.IsNotNull(targetEntity["jarvis_updatedowningcustomer"]);

            Assert.IsTrue(targetEntity.Attributes.Contains("jarvis_updatedusingcustomer"));
            Assert.IsNotNull(targetEntity["jarvis_updatedusingcustomer"]);
        }
    }
}
