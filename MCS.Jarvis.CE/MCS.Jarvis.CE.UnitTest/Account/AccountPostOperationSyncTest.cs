//-----------------------------------------------------------------------
// <copyright file="AccountPostOperationSyncTest.cs" company="Microsoft">
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
    using static MCS.Jarvis.CE.BusinessProcessesShared.Helpers.Constants;

    /// <summary>
    /// AccountPostOperationSyncTest.
    /// </summary>
    [TestClass]
    public class AccountPostOperationSyncTest : UnitTestBase
    {
        /// <summary>
        /// Postive Scenario for Account Post Operation.
        /// </summary>
        [TestMethod]
        public void AccountPostOperationTestActiveUpdate()
        {
            Entity targetAccount = new Entity("account", Guid.NewGuid());
            targetAccount.Attributes[Accounts.VasExternalStatus] = new OptionSetValue((int)VasStatus.Active);
            targetAccount.Attributes[Accounts.VasStatus] = new OptionSetValue((int)VasStatus.Active);
            targetAccount.Attributes["jarvis_accounttype"] = new OptionSetValue(334030001);
            Entity postImagAccount = new Entity("account", Guid.NewGuid());
            postImagAccount.Attributes[Accounts.VasExternalStatus] = new OptionSetValue((int)VasStatus.Active);
            postImagAccount.Attributes[Accounts.VasStatus] = new OptionSetValue((int)VasStatus.Active);
            postImagAccount.Attributes[Accounts.ViewStatus] = new OptionSetValue(1);
            postImagAccount.Attributes["jarvis_accounttype"] = new OptionSetValue(334030001);
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", targetAccount),
               };

            //// Setting Post Entity Image.
            this.PluginExecutionContext.PostEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PostImage", postImagAccount },
                };
            };

            AccountPostOperationSync plugin = new AccountPostOperationSync();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Postive Scenario for Account Post Operation.
        /// </summary>
        [TestMethod]
        public void AccountPostOperationTestActiveUpdate1()
        {
            Entity targetAccount = new Entity("account", Guid.NewGuid());
            targetAccount.Attributes[Accounts.VasExternalStatus] = new OptionSetValue((int)VasStatus.Active);
            targetAccount.Attributes[Accounts.VasStatus] = new OptionSetValue((int)VasStatus.Active);
            targetAccount.Attributes["jarvis_accounttype"] = new OptionSetValue(334030001);
            Entity postImagAccount = new Entity("account", Guid.NewGuid());
            postImagAccount.Attributes[Accounts.VasExternalStatus] = new OptionSetValue((int)VasStatus.InActive);
            postImagAccount.Attributes[Accounts.VasStatus] = new OptionSetValue((int)VasStatus.InActive);
            postImagAccount.Attributes[Accounts.ViewStatus] = new OptionSetValue(0);
            postImagAccount.Attributes["jarvis_accounttype"] = new OptionSetValue(334030001);
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", targetAccount),
               };

            //// Setting Post Entity Image.
            this.PluginExecutionContext.PostEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PostImage", postImagAccount },
                };
            };

            AccountPostOperationSync plugin = new AccountPostOperationSync();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Postive Scenario for Account Post Operation.
        /// </summary>
        [TestMethod]
        public void AccountPostOperationTestActiveUpdate2()
        {
            Entity targetAccount = new Entity("account", Guid.NewGuid());
            targetAccount.Attributes[Accounts.ViewStatus] = new OptionSetValue(0);
            targetAccount.Attributes["jarvis_accounttype"] = new OptionSetValue(334030001);
            Entity postImagAccount = new Entity("account", Guid.NewGuid());
            postImagAccount.Attributes[Accounts.VasExternalStatus] = new OptionSetValue((int)VasStatus.InActive);
            postImagAccount.Attributes[Accounts.VasStatus] = new OptionSetValue((int)VasStatus.InActive);
            postImagAccount.Attributes[Accounts.ViewStatus] = new OptionSetValue(0);
            postImagAccount.Attributes["jarvis_accounttype"] = new OptionSetValue(334030001);
            postImagAccount.Attributes[Accounts.BookableResource] = new EntityReference("bookableresource", Guid.NewGuid());
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", targetAccount),
               };

            //// Setting Post Entity Image.
            this.PluginExecutionContext.PostEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PostImage", postImagAccount },
                };
            };

            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                Entity bookableResource = new Entity("bookableresource", Guid.NewGuid());
                bookableResource.Attributes["statecode"] = new OptionSetValue(1);
                return bookableResource;
            };

            AccountPostOperationSync plugin = new AccountPostOperationSync();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Postive Scenario for Account Post Operation.
        /// </summary>
        [TestMethod]
        public void AccountPostOperationTestActiveUpdate3()
        {
            Entity targetAccount = new Entity("account", Guid.NewGuid());
            targetAccount.Attributes[Accounts.ViewStatus] = new OptionSetValue(1);
            targetAccount.Attributes["jarvis_accounttype"] = new OptionSetValue(334030001);
            Entity postImagAccount = new Entity("account", Guid.NewGuid());
            postImagAccount.Attributes[Accounts.VasExternalStatus] = new OptionSetValue((int)VasStatus.Active);
            postImagAccount.Attributes[Accounts.VasStatus] = new OptionSetValue((int)VasStatus.Active);
            postImagAccount.Attributes[Accounts.ViewStatus] = new OptionSetValue(1);
            postImagAccount.Attributes["jarvis_accounttype"] = new OptionSetValue(334030001);
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", targetAccount),
               };

            //// Setting Post Entity Image.
            this.PluginExecutionContext.PostEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PostImage", postImagAccount },
                };
            };

            AccountPostOperationSync plugin = new AccountPostOperationSync();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Negative Scenario for Account Post Operation.
        /// </summary>
        [TestMethod]
        public void AccountPostOperationTestNegativeDepth()
        {
            Entity targetAccount = new Entity("account", Guid.NewGuid());
            targetAccount.Attributes[Accounts.ViewStatus] = new OptionSetValue(1);
            targetAccount.Attributes["jarvis_accounttype"] = new OptionSetValue(334030001);
            Entity postImagAccount = new Entity("account", Guid.NewGuid());
            postImagAccount.Attributes[Accounts.VasExternalStatus] = new OptionSetValue((int)VasStatus.Active);
            postImagAccount.Attributes[Accounts.VasStatus] = new OptionSetValue((int)VasStatus.Active);
            postImagAccount.Attributes[Accounts.ViewStatus] = new OptionSetValue(1);
            postImagAccount.Attributes["jarvis_accounttype"] = new OptionSetValue(334030001);

            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", targetAccount),
               };

            //// Setting Post Entity Image.
            this.PluginExecutionContext.PostEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PostImage", postImagAccount },
                };
            };

            this.PluginExecutionContext.DepthGet = () => 3;

            AccountPostOperationSync plugin = new AccountPostOperationSync();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Negative Scenario for Account Post Operation.
        /// </summary>
        [TestMethod]
        public void AccountPostOperationTestNegativeAccountType()
        {
            Entity targetAccount = new Entity("account", Guid.NewGuid());
            targetAccount.Attributes[Accounts.ViewStatus] = new OptionSetValue(1);
            targetAccount.Attributes["jarvis_accounttype"] = new OptionSetValue(334030001);
            Entity postImagAccount = new Entity("account", Guid.NewGuid());
            postImagAccount.Attributes[Accounts.VasExternalStatus] = new OptionSetValue((int)VasStatus.Active);
            postImagAccount.Attributes[Accounts.VasStatus] = new OptionSetValue((int)VasStatus.Active);
            postImagAccount.Attributes[Accounts.ViewStatus] = new OptionSetValue(1);
            postImagAccount.Attributes["jarvis_accounttype"] = new OptionSetValue(334030000);

            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", targetAccount),
               };

            //// Setting Post Entity Image.
            this.PluginExecutionContext.PostEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PostImage", postImagAccount },
                };
            };

            this.PluginExecutionContext.DepthGet = () => 2;

            AccountPostOperationSync plugin = new AccountPostOperationSync();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Negative Scenario for Account Post Operation.
        /// </summary>
        [TestMethod]
        public void AccountPostOperationTestNoPostImage1()
        {
            Entity targetAccount = new Entity("account", Guid.NewGuid());
            targetAccount.Attributes[Accounts.ViewStatus] = new OptionSetValue(1);
            targetAccount.Attributes["jarvis_accounttype"] = new OptionSetValue(334030001);
            Entity postImagAccount = new Entity("account", Guid.NewGuid());
            postImagAccount.Attributes[Accounts.ViewStatus] = new OptionSetValue(1);
            postImagAccount.Attributes["jarvis_accounttype"] = new OptionSetValue(334030001);

            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", targetAccount),
               };

            //// Setting Post Entity Image.
            this.PluginExecutionContext.PostEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PostImage", postImagAccount },
                };
            };

            this.PluginExecutionContext.DepthGet = () => 2;

            AccountPostOperationSync plugin = new AccountPostOperationSync();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Negative Scenario for Account Post Operation.
        /// </summary>
        [TestMethod]
        public void AccountPostOperationTestNoPostImage2()
        {
            Entity targetAccount = new Entity("account", Guid.NewGuid());
            targetAccount.Attributes[Accounts.VasExternalStatus] = new OptionSetValue((int)VasStatus.Active);
            targetAccount.Attributes[Accounts.VasStatus] = new OptionSetValue((int)VasStatus.Active);
            targetAccount.Attributes["jarvis_accounttype"] = new OptionSetValue(334030001);
            Entity postImagAccount = new Entity("account", Guid.NewGuid());
            postImagAccount.Attributes[Accounts.ViewStatus] = new OptionSetValue(1);
            postImagAccount.Attributes["jarvis_accounttype"] = new OptionSetValue(334030001);

            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", targetAccount),
               };

            //// Setting Post Entity Image.
            this.PluginExecutionContext.PostEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PostImage", postImagAccount },
                };
            };

            this.PluginExecutionContext.DepthGet = () => 2;

            AccountPostOperationSync plugin = new AccountPostOperationSync();
            plugin.Execute(this.ServiceProvider);
        }
    }
}
