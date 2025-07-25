//-----------------------------------------------------------------------
// <copyright file="PassoutPreOperationSyncTest.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace MCS.Jarvis.CE.UnitTest
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Windows.Controls;
    using MCS.Jarvis.CE.Plugins;
    using Microsoft.Crm.Sdk.Messages;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;
    using Newtonsoft.Json;
    using static MCS.Jarvis.CE.BusinessProcessesShared.Helpers.Constants;
    using static MCS.Jarvis.CE.Plugins.Constants;
    using Constants = MCS.Jarvis.CE.BusinessProcessesShared.Helpers.Constants;

    /// <summary>
    /// PassoutPreOperationSync Test.
    /// </summary>
    [TestClass]
    public class PassoutPreOperationSyncTest : UnitTestBase
    {
        /// <summary>
        /// Pass Out Pre Operation Sync Test Create.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidPluginExecutionException))]
        public void PassOutPreOpSyncTestCreate()
        {
            var passOutObject = new Entity("jarvis_passout");
            passOutObject["jarvis_source_"] = new OptionSetValue(334030002);
            passOutObject[MCS.Jarvis.CE.Plugins.PassOut.Attributes.Case] = new EntityReference("incident", Guid.NewGuid());
            passOutObject[MCS.Jarvis.CE.Plugins.PassOut.Attributes.BusinessPartner] = new EntityReference("businesspartner", Guid.NewGuid());
            EntityReference users = new EntityReference("systemuser", Guid.NewGuid());
            passOutObject["createdby"] = users;

            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
            {
                new KeyValuePair<string, object>("Target", passOutObject),
            };

            this.PluginExecutionContext.MessageNameGet = () => "Create";
            this.PluginExecutionContext.StageGet = () => 40;
            this.PluginExecutionContext.DepthGet = () => 1;

            // Setting service retrieve.
            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "incident")
                {
                    var caseObject = new Entity("incident");
                    caseObject.Id = guid;
                    caseObject["jarvis_timezone"] = 105;
                    caseObject["jarvis_timezonelabel"] = "(GMT+01:00)";
                    caseObject["createdon"] = DateTime.UtcNow;
                    caseObject["jarvis_restgoplimitout"] = 10;
                    caseObject["jarvis_totalrestcurrencyout"] = new EntityReference("transactioncurrencies", Guid.NewGuid());
                    caseObject["createdby"] = users;
                    return caseObject;
                }

                if (entityName == "systemuser")
                {
                    var user = new Entity("systemuser");
                    user.Id = guid;
                    user["fullname"] = "MERCURIUS";
                    return user;
                }

                return null;
            };

            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();
                if (query.GetType().Name == "FetchExpression")
                {
                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_passout'>"))
                    {
                        Entity passoutResult = new Entity("jarvis_passout");
                        passoutResult["jarvis_name"] = "testpassout";
                        passoutResult["jarvis_repairingdealer"] = new EntityReference("account", Guid.NewGuid());
                        passoutResult["jarvis_goplimitout"] = Convert.ToDecimal(12);
                        passoutResult["transactioncurrencyid"] = new EntityReference("transactioncurrencies", Guid.NewGuid());
                        passoutResult["createdon"] = DateTime.UtcNow;
                        passoutResult["modifiedon"] = DateTime.UtcNow;
                        passoutResult.Attributes["jarvis_etc"] = DateTime.Now;
                        passoutResult.Attributes["jarvis_ata"] = DateTime.Now;
                        passoutResult.Attributes["jarvis_eta"] = DateTime.Now;
                        passoutResult.Attributes["jarvis_atc"] = DateTime.Now;
                        passoutResult["jarvis_paymenttype"] = new OptionSetValue(334030002);
                        passoutResult.Id = Guid.NewGuid();
                        result.Entities.Add(passoutResult);
                    }
                }

                return result;
            };

            PassoutPreOperationSync plugin = new PassoutPreOperationSync();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Pass Out Pre Operation Sync Test Create.
        /// </summary>
        [TestMethod]
        public void PassOutPreOpSyncTestCreate2()
        {
            var passOutObject = new Entity("jarvis_passout");
            passOutObject["jarvis_source_"] = new OptionSetValue(334030002);
            passOutObject[MCS.Jarvis.CE.Plugins.PassOut.Attributes.Case] = new EntityReference("incident", Guid.NewGuid());
           // passOutObject[MCS.Jarvis.CE.Plugins.PassOut.Attributes.BusinessPartner] = new EntityReference("businesspartner", Guid.NewGuid());
            EntityReference users = new EntityReference("systemuser", Guid.NewGuid());
            passOutObject["createdby"] = users;
            passOutObject.Attributes["jarvis_etc"] = DateTime.Now;
            passOutObject.Attributes["jarvis_ata"] = DateTime.Now;
            passOutObject.Attributes["jarvis_eta"] = DateTime.Now;
            passOutObject.Attributes["jarvis_atc"] = DateTime.Now;
            passOutObject["jarvis_paymenttype"] = new OptionSetValue(334030002);

            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
            {
                new KeyValuePair<string, object>("Target", passOutObject),
            };

            this.PluginExecutionContext.MessageNameGet = () => "Create";
            this.PluginExecutionContext.StageGet = () => 40;
            this.PluginExecutionContext.DepthGet = () => 1;

            // Setting service retrieve.
            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "incident")
                {
                    var caseObject = new Entity("incident");
                    caseObject.Id = guid;
                    caseObject["jarvis_timezone"] = 105;
                    caseObject["jarvis_timezonelabel"] = "(GMT+01:00)";
                    caseObject["createdon"] = DateTime.UtcNow;
                    caseObject["jarvis_restgoplimitout"] = 10;
                    caseObject["jarvis_totalrestcurrencyout"] = new EntityReference("transactioncurrencies", Guid.NewGuid());
                    caseObject["createdby"] = users;
                    return caseObject;
                }

                if (entityName == "systemuser")
                {
                    var user = new Entity("systemuser");
                    user.Id = guid;
                    user["fullname"] = "MERCURIUS";
                    return user;
                }

                return null;
            };

            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();
                if (query.GetType().Name == "FetchExpression")
                {
                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_passout'>"))
                    {
                        Entity passoutResult = new Entity("jarvis_passout");
                        passoutResult["jarvis_name"] = "testpassout";
                        passoutResult["jarvis_repairingdealer"] = new EntityReference("account", Guid.NewGuid());
                        passoutResult["jarvis_goplimitout"] = Convert.ToDecimal(12);
                        passoutResult["transactioncurrencyid"] = new EntityReference("transactioncurrencies", Guid.NewGuid());
                        passoutResult["createdon"] = DateTime.UtcNow;
                        passoutResult["modifiedon"] = DateTime.UtcNow;
                        passoutResult.Attributes["jarvis_etc"] = DateTime.Now;
                        passoutResult.Attributes["jarvis_ata"] = DateTime.Now;
                        passoutResult.Attributes["jarvis_eta"] = DateTime.Now;
                        passoutResult.Attributes["jarvis_atc"] = DateTime.Now;
                        passoutResult["jarvis_paymenttype"] = new OptionSetValue(334030002);
                        passoutResult.Id = Guid.NewGuid();
                        result.Entities.Add(passoutResult);
                    }
                }

                return result;
            };

            this.Service.ExecuteOrganizationRequest = processInstanceRequest =>
            {
                return new LocalTimeFromUtcTimeResponse();
            };

            PassoutPreOperationSync plugin = new PassoutPreOperationSync();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Pass Out Pre Operation Sync Test Create.
        /// </summary>
        [TestMethod]
        public void PassOutPreOpSyncTestCreate3()
        {
            var passOutObject = new Entity("jarvis_passout");
            passOutObject["jarvis_source_"] = new OptionSetValue(334030002);
            passOutObject[MCS.Jarvis.CE.Plugins.PassOut.Attributes.Case] = new EntityReference("incident", Guid.NewGuid());
            passOutObject[MCS.Jarvis.CE.Plugins.PassOut.Attributes.BusinessPartner] = new EntityReference("account", Guid.NewGuid());
            EntityReference users = new EntityReference("systemuser", Guid.NewGuid());
            passOutObject["createdby"] = users;
            passOutObject.Attributes["jarvis_etc"] = DateTime.Now;
            passOutObject.Attributes["jarvis_ata"] = DateTime.Now;
            passOutObject.Attributes["jarvis_eta"] = DateTime.Now;
            passOutObject.Attributes["jarvis_atc"] = DateTime.Now;

            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
            {
                new KeyValuePair<string, object>("Target", passOutObject),
            };

            this.PluginExecutionContext.MessageNameGet = () => "Create";
            this.PluginExecutionContext.StageGet = () => 40;
            this.PluginExecutionContext.DepthGet = () => 1;

            // Setting service retrieve.
            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "incident")
                {
                    var caseObject = new Entity("incident");
                    caseObject.Id = guid;
                    caseObject["jarvis_timezone"] = 105;
                    caseObject["jarvis_timezonelabel"] = "(GMT+01:00)";
                    caseObject["createdon"] = DateTime.UtcNow;
                    caseObject["jarvis_restgoplimitout"] = 10;
                    caseObject["jarvis_totalrestcurrencyout"] = new EntityReference("transactioncurrencies", Guid.NewGuid());
                    caseObject["createdby"] = users;
                    return caseObject;
                }

                if (entityName == "systemuser")
                {
                    var user = new Entity("systemuser");
                    user.Id = guid;
                    user["fullname"] = "firstname lastname";
                    return user;
                }

                if (entityName == "account")
                {
                    var account = new Entity("account");
                    account["name"] = "firstname lastname";
                    account["jarvis_responsableunitid"] = "test123";
                    return account;
                }

                return null;
            };

            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();
                if (query.GetType().Name == "FetchExpression")
                {
                    if (query is FetchExpression fetchExpression && fetchExpression.Query.Contains("<entity name='jarvis_gop'>"))
                    {
                        var gop = new Entity("jarvis_gop");
                        gop.Id = Guid.NewGuid();
                        gop.Attributes["jarvis_paymenttype"] = new OptionSetValue(334030002);
                        gop.Attributes["modifiedon"] = DateTime.UtcNow;
                        result.Entities.Add(gop);
                    }
                }

                return result;
            };

            this.Service.ExecuteOrganizationRequest = processInstanceRequest =>
            {
                return new LocalTimeFromUtcTimeResponse();
            };

            PassoutPreOperationSync plugin = new PassoutPreOperationSync();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Pass Out Pre Operation Sync Test Update.
        /// </summary>
        [TestMethod]
        public void PassOutPreOpSyncTestUpdate()
        {
            var passOutObject = new Entity("jarvis_passout");
            passOutObject["jarvis_source_"] = new OptionSetValue(334030002);
            passOutObject[MCS.Jarvis.CE.Plugins.PassOut.Attributes.Case] = new EntityReference("incident", Guid.NewGuid());
            passOutObject[MCS.Jarvis.CE.Plugins.PassOut.Attributes.BusinessPartner] = new EntityReference("account", Guid.NewGuid());
            EntityReference users = new EntityReference("systemuser", Guid.NewGuid());
            passOutObject["modifiedby"] = users;
            passOutObject["jarvis_etc"] = DateTime.Now;
            passOutObject["jarvis_ata"] = DateTime.Now;
            passOutObject["jarvis_eta"] = DateTime.Now;
            passOutObject["jarvis_atc"] = DateTime.Now;
            passOutObject["jarvis_reason"] = "Delayed Reason";
            passOutObject["jarvis_gpseta"] = DateTime.Now;
            passOutObject["jarvis_delayedeta"] = DateTime.Now;
            passOutObject["jarvis_paymenttype"] = new OptionSetValue(334030002);

            var passout = new Entity("jarvis_passout");
            passout["jarvis_source_"] = new OptionSetValue(334030002);
            passout[MCS.Jarvis.CE.Plugins.PassOut.Attributes.Case] = new EntityReference("incident", Guid.NewGuid());
            passout[MCS.Jarvis.CE.Plugins.PassOut.Attributes.BusinessPartner] = new EntityReference("account", Guid.NewGuid());
            passout["createdby"] = users;
            passout.Attributes["jarvis_etc"] = DateTime.Now;
            passout.Attributes["jarvis_ata"] = DateTime.Now;
            passout.Attributes["jarvis_eta"] = DateTime.Now;
            passout.Attributes["jarvis_atc"] = DateTime.Now;

            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
            {
                new KeyValuePair<string, object>("Target", passOutObject),
            };
            this.PluginExecutionContext.MessageNameGet = () => "Update";
            this.PluginExecutionContext.StageGet = () => 40;

            //// Setting Pre Image
            this.PluginExecutionContext.PreEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PreImage", passout },
                };
            };

            // Setting service retrieve.
            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "incident")
                {
                    var caseObject = new Entity("incident");
                    caseObject.Id = guid;
                    caseObject["jarvis_timezone"] = 105;
                    caseObject["jarvis_timezonelabel"] = "(GMT+01:00)";
                    caseObject["createdon"] = DateTime.UtcNow;
                    caseObject["jarvis_restgoplimitout"] = 10;
                    caseObject["jarvis_totalrestcurrencyout"] = new EntityReference("transactioncurrencies", Guid.NewGuid());
                    caseObject["jarvis_totalgoplimitoutapproved"] = 10;
                    caseObject["jarvis_totalgoplimitoutapprovedcurrency"] = new EntityReference("transactioncurrencies", Guid.NewGuid());
                    caseObject["jarvis_totalpassoutamount"] = 10;
                    caseObject["jarvis_totalpassoutcurrency"] = new EntityReference("transactioncurrencies", Guid.NewGuid());
                    caseObject["jarvis_etc"] = DateTime.Now;
                    caseObject["jarvis_ata"] = DateTime.Now;
                    caseObject["jarvis_eta"] = DateTime.Now;
                    caseObject["jarvis_atc"] = DateTime.Now;
                    caseObject["statuscode"] = new OptionSetValue(10);
                    caseObject["modifiedby"] = users;
                    return caseObject;
                }

                if (entityName == "systemuser")
                {
                    var user = new Entity("systemuser");
                    user.Id = guid;
                    user["fullname"] = "MERCURIUS";
                    return user;
                }

                if (entityName == "account")
                {
                    var account = new Entity("account");
                    account["name"] = "firstname lastname";
                    account["jarvis_responsableunitid"] = "test123";
                    return account;
                }

                return null;
            };
            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();
                if (query.GetType().Name == "FetchExpression")
                {
                    if (query is FetchExpression fetchExpression && fetchExpression.Query.Contains("<entity name='jarvis_gop'>"))
                    {
                        var gop = new Entity("jarvis_gop");
                        gop.Id = Guid.NewGuid();
                        gop.Attributes["jarvis_paymenttype"] = new OptionSetValue(334030002);
                        gop.Attributes["modifiedon"] = DateTime.UtcNow;
                        result.Entities.Add(gop);
                    }
                }

                return result;
            };

            this.Service.ExecuteOrganizationRequest = processInstanceRequest =>
            {
                return new LocalTimeFromUtcTimeResponse();
            };

            PassoutPreOperationSync plugin = new PassoutPreOperationSync();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Pass Out Pre Operation Sync Test Update.
        /// </summary>
        [TestMethod]
        public void PassOutPreOpSyncTestUpdate1()
        {
            var passOutObject = new Entity("jarvis_passout");
            //passOutObject["jarvis_source_"] = new OptionSetValue(334030002);
            passOutObject[MCS.Jarvis.CE.Plugins.PassOut.Attributes.Case] = new EntityReference("incident", Guid.NewGuid());
            passOutObject[MCS.Jarvis.CE.Plugins.PassOut.Attributes.BusinessPartner] = new EntityReference("account", Guid.NewGuid());
            EntityReference users = new EntityReference("systemuser", Guid.NewGuid());
            passOutObject["jarvis_etc"] = DateTime.Now;
            passOutObject["jarvis_ata"] = DateTime.Now;
            //passOutObject["jarvis_eta"] = DateTime.Now;
            passOutObject["jarvis_atc"] = DateTime.Now;

            var passout = new Entity("jarvis_passout");
            passout["jarvis_source_"] = new OptionSetValue(334030002);
            passout[MCS.Jarvis.CE.Plugins.PassOut.Attributes.Case] = new EntityReference("incident", Guid.NewGuid());
            passout[MCS.Jarvis.CE.Plugins.PassOut.Attributes.BusinessPartner] = new EntityReference("account", Guid.NewGuid());
            passout["modifiedby"] = users;
            passout.Attributes["jarvis_etc"] = DateTime.Now;
            passout.Attributes["jarvis_ata"] = DateTime.Now;
            passout.Attributes["jarvis_eta"] = DateTime.Now;
            passout.Attributes["jarvis_atc"] = DateTime.Now;
            passout["jarvis_reason"] = "Delayed Reason";
            passout["jarvis_gpseta"] = DateTime.Now;
            passout["jarvis_delayedeta"] = DateTime.Now;
            passout["jarvis_paymenttype"] = new OptionSetValue(334030002);

            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
            {
                new KeyValuePair<string, object>("Target", passOutObject),
            };
            this.PluginExecutionContext.MessageNameGet = () => "Update";
            this.PluginExecutionContext.StageGet = () => 40;

            //// Setting Pre Image
            this.PluginExecutionContext.PreEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PreImage", passout },
                };
            };

            // Setting service retrieve.
            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "incident")
                {
                    var caseObject = new Entity("incident");
                    caseObject.Id = guid;
                    caseObject["jarvis_timezone"] = 105;
                    caseObject["jarvis_timezonelabel"] = "(GMT+01:00)";
                    caseObject["createdon"] = DateTime.UtcNow;
                    caseObject["jarvis_restgoplimitout"] = 10;
                    caseObject["jarvis_totalrestcurrencyout"] = new EntityReference("transactioncurrencies", Guid.NewGuid());
                    caseObject["jarvis_totalgoplimitoutapproved"] = 10;
                    caseObject["jarvis_totalgoplimitoutapprovedcurrency"] = new EntityReference("transactioncurrencies", Guid.NewGuid());
                    caseObject["jarvis_totalpassoutamount"] = 10;
                    caseObject["jarvis_totalpassoutcurrency"] = new EntityReference("transactioncurrencies", Guid.NewGuid());
                    caseObject["jarvis_etc"] = DateTime.Now;
                    caseObject["jarvis_ata"] = DateTime.Now;
                    caseObject["jarvis_eta"] = DateTime.Now;
                    caseObject["jarvis_atc"] = DateTime.Now;
                    caseObject["statuscode"] = new OptionSetValue(10);
                    caseObject["modifiedby"] = users;
                    return caseObject;
                }

                if (entityName == "systemuser")
                {
                    var user = new Entity("systemuser");
                    user.Id = guid;
                    user["fullname"] = "MERCURIUS";
                    return user;
                }

                if (entityName == "account")
                {
                    var account = new Entity("account");
                    account["name"] = "firstname lastname";
                    account["jarvis_responsableunitid"] = "test123";
                    return account;
                }

                return null;
            };
            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();
                if (query.GetType().Name == "FetchExpression")
                {
                    if (query is FetchExpression fetchExpression && fetchExpression.Query.Contains("<entity name='jarvis_gop'>"))
                    {
                        var gop = new Entity("jarvis_gop");
                        gop.Id = Guid.NewGuid();
                        gop.Attributes["jarvis_paymenttype"] = new OptionSetValue(334030002);
                        gop.Attributes["modifiedon"] = DateTime.UtcNow;
                        result.Entities.Add(gop);
                    }
                }

                return result;
            };

            this.Service.ExecuteOrganizationRequest = processInstanceRequest =>
            {
                return new LocalTimeFromUtcTimeResponse();
            };

            PassoutPreOperationSync plugin = new PassoutPreOperationSync();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Pass Out Pre Operation Sync Test Update.
        /// </summary>
        [TestMethod]
        public void PassOutPreOpSyncTestUpdate2()
        {
            var passOutObject = new Entity("jarvis_passout");
            //passOutObject["jarvis_source_"] = new OptionSetValue(334030002);
            passOutObject[MCS.Jarvis.CE.Plugins.PassOut.Attributes.Case] = new EntityReference("incident", Guid.NewGuid());
            passOutObject[MCS.Jarvis.CE.Plugins.PassOut.Attributes.BusinessPartner] = new EntityReference("account", Guid.NewGuid());
            EntityReference users = new EntityReference("systemuser", Guid.NewGuid());
            passOutObject["jarvis_etc"] = DateTime.Now;
            passOutObject["jarvis_ata"] = DateTime.Now;
            //passOutObject["jarvis_eta"] = DateTime.Now;
            passOutObject["jarvis_atc"] = DateTime.Now;

            var passout = new Entity("jarvis_passout");
            passout["jarvis_source_"] = new OptionSetValue(334030003);
            passout[MCS.Jarvis.CE.Plugins.PassOut.Attributes.Case] = new EntityReference("incident", Guid.NewGuid());
            passout[MCS.Jarvis.CE.Plugins.PassOut.Attributes.BusinessPartner] = new EntityReference("account", Guid.NewGuid());
            passout["modifiedby"] = users;
            passout.Attributes["jarvis_etc"] = DateTime.Now;
            passout.Attributes["jarvis_ata"] = DateTime.Now;
            passout.Attributes["jarvis_eta"] = DateTime.Now;
            passout.Attributes["jarvis_atc"] = DateTime.Now;
            passout["jarvis_reason"] = "Delayed Reason";
            passout["jarvis_gpseta"] = DateTime.Now;
            //passout["jarvis_delayedeta"] = DateTime.Now;
            passout["jarvis_paymenttype"] = new OptionSetValue(334030002);

            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
            {
                new KeyValuePair<string, object>("Target", passOutObject),
            };
            this.PluginExecutionContext.MessageNameGet = () => "Update";
            this.PluginExecutionContext.StageGet = () => 40;

            //// Setting Pre Image
            this.PluginExecutionContext.PreEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PreImage", passout },
                };
            };

            // Setting service retrieve.
            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "incident")
                {
                    var caseObject = new Entity("incident");
                    caseObject.Id = guid;
                    caseObject["jarvis_timezone"] = 105;
                    caseObject["jarvis_timezonelabel"] = "(GMT+01:00)";
                    caseObject["createdon"] = DateTime.UtcNow;
                    caseObject["jarvis_restgoplimitout"] = 10;
                    caseObject["jarvis_totalrestcurrencyout"] = new EntityReference("transactioncurrencies", Guid.NewGuid());
                    caseObject["jarvis_totalgoplimitoutapproved"] = 10;
                    caseObject["jarvis_totalgoplimitoutapprovedcurrency"] = new EntityReference("transactioncurrencies", Guid.NewGuid());
                    caseObject["jarvis_totalpassoutamount"] = 10;
                    caseObject["jarvis_totalpassoutcurrency"] = new EntityReference("transactioncurrencies", Guid.NewGuid());
                    caseObject["jarvis_etc"] = DateTime.Now;
                    caseObject["jarvis_ata"] = DateTime.Now;
                    caseObject["jarvis_eta"] = DateTime.Now;
                    caseObject["jarvis_atc"] = DateTime.Now;
                    caseObject["statuscode"] = new OptionSetValue(10);
                    caseObject["modifiedby"] = users;
                    return caseObject;
                }

                if (entityName == "systemuser")
                {
                    var user = new Entity("systemuser");
                    user.Id = guid;
                    user["fullname"] = "MERCURIUS";
                    return user;
                }

                if (entityName == "account")
                {
                    var account = new Entity("account");
                    account["name"] = "firstname lastname";
                    account["jarvis_responsableunitid"] = "test123";
                    return account;
                }

                return null;
            };
            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();
                if (query.GetType().Name == "FetchExpression")
                {
                    if (query is FetchExpression fetchExpression && fetchExpression.Query.Contains("<entity name='jarvis_gop'>"))
                    {
                        var gop = new Entity("jarvis_gop");
                        gop.Id = Guid.NewGuid();
                        gop.Attributes["jarvis_paymenttype"] = new OptionSetValue(334030002);
                        gop.Attributes["modifiedon"] = DateTime.UtcNow;
                        result.Entities.Add(gop);
                    }
                }

                return result;
            };

            this.Service.ExecuteOrganizationRequest = processInstanceRequest =>
            {
                return new LocalTimeFromUtcTimeResponse();
            };

            PassoutPreOperationSync plugin = new PassoutPreOperationSync();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Pass Out Pre Operation Sync Test Update.
        /// </summary>
        [TestMethod]
        public void PassOutPreOpSyncTestUpdate3()
        {
            var passOutObject = new Entity("jarvis_passout");
            //passOutObject["jarvis_source_"] = new OptionSetValue(334030002);
            passOutObject[MCS.Jarvis.CE.Plugins.PassOut.Attributes.Case] = new EntityReference("incident", Guid.NewGuid());
            passOutObject[MCS.Jarvis.CE.Plugins.PassOut.Attributes.BusinessPartner] = new EntityReference("account", Guid.NewGuid());
            EntityReference users = new EntityReference("systemuser", Guid.NewGuid());
            passOutObject["jarvis_etc"] = DateTime.Now;
            passOutObject["jarvis_ata"] = DateTime.Now;
            passOutObject["jarvis_eta"] = DateTime.Now.AddMinutes(20);
            passOutObject["jarvis_atc"] = DateTime.Now;

            var passout = new Entity("jarvis_passout");
            passout["jarvis_source_"] = new OptionSetValue(334030003);
            passout[MCS.Jarvis.CE.Plugins.PassOut.Attributes.Case] = new EntityReference("incident", Guid.NewGuid());
            passout[MCS.Jarvis.CE.Plugins.PassOut.Attributes.BusinessPartner] = new EntityReference("account", Guid.NewGuid());
            passout["modifiedby"] = users;
            passout.Attributes["jarvis_etc"] = DateTime.Now;
            passout.Attributes["jarvis_ata"] = DateTime.Now;
            passout.Attributes["jarvis_eta"] = DateTime.Now;
            passout.Attributes["jarvis_atc"] = DateTime.Now;
            passout["jarvis_reason"] = "Delayed Reason";
            passout["jarvis_gpseta"] = DateTime.Now;
            //passout["jarvis_delayedeta"] = DateTime.Now;
            //passout["jarvis_paymenttype"] = new OptionSetValue(334030002);

            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
            {
                new KeyValuePair<string, object>("Target", passOutObject),
            };
            this.PluginExecutionContext.MessageNameGet = () => "Update";
            this.PluginExecutionContext.StageGet = () => 40;

            //// Setting Pre Image
            this.PluginExecutionContext.PreEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PreImage", passout },
                };
            };

            // Setting service retrieve.
            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "incident")
                {
                    var caseObject = new Entity("incident");
                    caseObject.Id = guid;
                    caseObject["jarvis_timezone"] = 105;
                    caseObject["jarvis_timezonelabel"] = "(GMT+01:00)";
                    caseObject["createdon"] = DateTime.UtcNow;
                    caseObject["jarvis_restgoplimitout"] = 10;
                    caseObject["jarvis_totalrestcurrencyout"] = new EntityReference("transactioncurrencies", Guid.NewGuid());
                    caseObject["jarvis_totalgoplimitoutapproved"] = 10;
                    caseObject["jarvis_totalgoplimitoutapprovedcurrency"] = new EntityReference("transactioncurrencies", Guid.NewGuid());
                    caseObject["jarvis_totalpassoutamount"] = 10;
                    caseObject["jarvis_totalpassoutcurrency"] = new EntityReference("transactioncurrencies", Guid.NewGuid());
                    caseObject["jarvis_etc"] = DateTime.Now;
                    caseObject["jarvis_ata"] = DateTime.Now;
                    caseObject["jarvis_eta"] = DateTime.Now;
                    caseObject["jarvis_atc"] = DateTime.Now;
                    caseObject["statuscode"] = new OptionSetValue(10);
                    caseObject["modifiedby"] = users;
                    return caseObject;
                }

                if (entityName == "systemuser")
                {
                    var user = new Entity("systemuser");
                    user.Id = guid;
                    user["fullname"] = "firstname lastname";
                    return user;
                }

                if (entityName == "account")
                {
                    var account = new Entity("account");
                    account["name"] = "firstname lastname";
                    account["jarvis_responsableunitid"] = "test123";
                    return account;
                }

                return null;
            };
            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();
                if (query.GetType().Name == "FetchExpression")
                {
                    if (query is FetchExpression fetchExpression && fetchExpression.Query.Contains("<entity name='jarvis_gop'>"))
                    {
                        var gop = new Entity("jarvis_gop");
                        gop.Id = Guid.NewGuid();
                        gop.Attributes["jarvis_paymenttype"] = new OptionSetValue(334030002);
                        gop.Attributes["modifiedon"] = DateTime.UtcNow;
                        result.Entities.Add(gop);
                    }
                }

                return result;
            };

            this.Service.ExecuteOrganizationRequest = processInstanceRequest =>
            {
                return new LocalTimeFromUtcTimeResponse();
            };

            PassoutPreOperationSync plugin = new PassoutPreOperationSync();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Pass Out Pre Operation Sync Test Update.
        /// </summary>
        [TestMethod]
        public void PassOutPreOpSyncTestUpdate4()
        {
            var passOutObject = new Entity("jarvis_passout");
            //passOutObject["jarvis_source_"] = new OptionSetValue(334030002);
            passOutObject[MCS.Jarvis.CE.Plugins.PassOut.Attributes.Case] = new EntityReference("incident", Guid.NewGuid());
            passOutObject[MCS.Jarvis.CE.Plugins.PassOut.Attributes.BusinessPartner] = new EntityReference("account", Guid.NewGuid());
            EntityReference users = new EntityReference("systemuser", Guid.NewGuid());
            passOutObject["jarvis_etc"] = DateTime.Now;
            passOutObject["jarvis_ata"] = DateTime.Now;
            passOutObject["jarvis_eta"] = DateTime.Now;
            passOutObject["jarvis_atc"] = DateTime.Now;

            var passout = new Entity("jarvis_passout");
            passout["jarvis_source_"] = new OptionSetValue(334030003);
            passout[MCS.Jarvis.CE.Plugins.PassOut.Attributes.Case] = new EntityReference("incident", Guid.NewGuid());
            passout[MCS.Jarvis.CE.Plugins.PassOut.Attributes.BusinessPartner] = new EntityReference("account", Guid.NewGuid());
            passout["modifiedby"] = users;
            passout.Attributes["jarvis_etc"] = DateTime.Now;
            passout.Attributes["jarvis_ata"] = DateTime.Now;
            passout.Attributes["jarvis_eta"] = DateTime.Now.AddMinutes(20);
            passout.Attributes["jarvis_atc"] = DateTime.Now;
            passout["jarvis_reason"] = "Delayed Reason";
            passout["jarvis_gpseta"] = DateTime.Now;
            //passout["jarvis_delayedeta"] = DateTime.Now;
            //passout["jarvis_paymenttype"] = new OptionSetValue(334030002);

            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
            {
                new KeyValuePair<string, object>("Target", passOutObject),
            };
            this.PluginExecutionContext.MessageNameGet = () => "Update";
            this.PluginExecutionContext.StageGet = () => 40;

            //// Setting Pre Image
            this.PluginExecutionContext.PreEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PreImage", passout },
                };
            };

            // Setting service retrieve.
            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "incident")
                {
                    var caseObject = new Entity("incident");
                    caseObject.Id = guid;
                    caseObject["jarvis_timezone"] = 105;
                    caseObject["jarvis_timezonelabel"] = "(GMT+01:00)";
                    caseObject["createdon"] = DateTime.UtcNow;
                    caseObject["jarvis_restgoplimitout"] = 10;
                    caseObject["jarvis_totalrestcurrencyout"] = new EntityReference("transactioncurrencies", Guid.NewGuid());
                    caseObject["jarvis_totalgoplimitoutapproved"] = 10;
                    caseObject["jarvis_totalgoplimitoutapprovedcurrency"] = new EntityReference("transactioncurrencies", Guid.NewGuid());
                    caseObject["jarvis_totalpassoutamount"] = 10;
                    caseObject["jarvis_totalpassoutcurrency"] = new EntityReference("transactioncurrencies", Guid.NewGuid());
                    caseObject["jarvis_etc"] = DateTime.Now;
                    caseObject["jarvis_ata"] = DateTime.Now;
                    caseObject["jarvis_eta"] = DateTime.Now;
                    caseObject["jarvis_atc"] = DateTime.Now;
                    caseObject["statuscode"] = new OptionSetValue(10);
                    caseObject["modifiedby"] = users;
                    return caseObject;
                }

                if (entityName == "systemuser")
                {
                    var user = new Entity("systemuser");
                    user.Id = guid;
                    user["fullname"] = "firstname lastname";
                    return user;
                }

                if (entityName == "account")
                {
                    var account = new Entity("account");
                    account["name"] = "firstname lastname";
                    account["jarvis_responsableunitid"] = "test123";
                    return account;
                }

                return null;
            };
            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();
                if (query.GetType().Name == "FetchExpression")
                {
                    if (query is FetchExpression fetchExpression && fetchExpression.Query.Contains("<entity name='jarvis_gop'>"))
                    {
                        var gop = new Entity("jarvis_gop");
                        gop.Id = Guid.NewGuid();
                        gop.Attributes["jarvis_paymenttype"] = new OptionSetValue(334030002);
                        gop.Attributes["modifiedon"] = DateTime.UtcNow;
                        result.Entities.Add(gop);
                    }
                }

                return result;
            };

            this.Service.ExecuteOrganizationRequest = processInstanceRequest =>
            {
                return new LocalTimeFromUtcTimeResponse();
            };

            PassoutPreOperationSync plugin = new PassoutPreOperationSync();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Pass Out Pre Operation Sync Test Update.
        /// </summary>
        [TestMethod]
        public void PassOutPreOpSyncTestUpdate5()
        {
            var passOutObject = new Entity("jarvis_passout");
            //passOutObject["jarvis_source_"] = new OptionSetValue(334030002);
            passOutObject[MCS.Jarvis.CE.Plugins.PassOut.Attributes.Case] = new EntityReference("incident", Guid.NewGuid());
            passOutObject[MCS.Jarvis.CE.Plugins.PassOut.Attributes.BusinessPartner] = new EntityReference("account", Guid.NewGuid());
            EntityReference users = new EntityReference("systemuser", Guid.NewGuid());
            passOutObject["jarvis_etc"] = DateTime.Now;
            passOutObject["jarvis_ata"] = DateTime.Now;
            passOutObject["jarvis_eta"] = DateTime.Now;
            passOutObject["jarvis_atc"] = DateTime.Now;
            passOutObject["jarvis_delayedeta"] = DateTime.Now.AddMinutes(20);
            passOutObject["jarvis_gpseta"] = DateTime.Now.AddMinutes(20);

            var passout = new Entity("jarvis_passout");
            passout["jarvis_source_"] = new OptionSetValue(334030003);
            passout[MCS.Jarvis.CE.Plugins.PassOut.Attributes.Case] = new EntityReference("incident", Guid.NewGuid());
            passout[MCS.Jarvis.CE.Plugins.PassOut.Attributes.BusinessPartner] = new EntityReference("account", Guid.NewGuid());
            passout["modifiedby"] = users;
            passout.Attributes["jarvis_etc"] = DateTime.Now;
            passout.Attributes["jarvis_ata"] = DateTime.Now;
            passout.Attributes["jarvis_eta"] = DateTime.Now.AddMinutes(20);
            passout.Attributes["jarvis_atc"] = DateTime.Now;
            passout["jarvis_reason"] = "Delayed Reason";
            passout["jarvis_gpseta"] = DateTime.Now.AddMinutes(20);
            //passout["jarvis_delayedeta"] = DateTime.Now;
            //passout["jarvis_paymenttype"] = new OptionSetValue(334030002);

            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
            {
                new KeyValuePair<string, object>("Target", passOutObject),
            };
            this.PluginExecutionContext.MessageNameGet = () => "Update";
            this.PluginExecutionContext.StageGet = () => 40;

            //// Setting Pre Image
            this.PluginExecutionContext.PreEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PreImage", passout },
                };
            };

            // Setting service retrieve.
            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "incident")
                {
                    var caseObject = new Entity("incident");
                    caseObject.Id = guid;
                    caseObject["jarvis_timezone"] = 105;
                    caseObject["jarvis_timezonelabel"] = "(GMT+01:00)";
                    caseObject["createdon"] = DateTime.UtcNow;
                    caseObject["jarvis_restgoplimitout"] = 10;
                    caseObject["jarvis_totalrestcurrencyout"] = new EntityReference("transactioncurrencies", Guid.NewGuid());
                    caseObject["jarvis_totalgoplimitoutapproved"] = 10;
                    caseObject["jarvis_totalgoplimitoutapprovedcurrency"] = new EntityReference("transactioncurrencies", Guid.NewGuid());
                    caseObject["jarvis_totalpassoutamount"] = 10;
                    caseObject["jarvis_totalpassoutcurrency"] = new EntityReference("transactioncurrencies", Guid.NewGuid());
                    caseObject["jarvis_etc"] = DateTime.Now;
                    caseObject["jarvis_ata"] = DateTime.Now;
                    caseObject["jarvis_eta"] = DateTime.Now;
                    caseObject["jarvis_atc"] = DateTime.Now;
                    caseObject["statuscode"] = new OptionSetValue(10);
                    caseObject["modifiedby"] = users;
                    return caseObject;
                }

                if (entityName == "systemuser")
                {
                    var user = new Entity("systemuser");
                    user.Id = guid;
                    user["fullname"] = "firstname lastname";
                    return user;
                }

                if (entityName == "account")
                {
                    var account = new Entity("account");
                    account["name"] = "firstname lastname";
                    account["jarvis_responsableunitid"] = "test123";
                    return account;
                }

                return null;
            };
            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();
                if (query.GetType().Name == "FetchExpression")
                {
                    if (query is FetchExpression fetchExpression && fetchExpression.Query.Contains("<entity name='jarvis_gop'>"))
                    {
                        var gop = new Entity("jarvis_gop");
                        gop.Id = Guid.NewGuid();
                        gop.Attributes["jarvis_paymenttype"] = new OptionSetValue(334030002);
                        gop.Attributes["modifiedon"] = DateTime.UtcNow;
                        result.Entities.Add(gop);
                    }
                }

                return result;
            };

            this.Service.ExecuteOrganizationRequest = processInstanceRequest =>
            {
                return new LocalTimeFromUtcTimeResponse();
            };

            PassoutPreOperationSync plugin = new PassoutPreOperationSync();
            plugin.Execute(this.ServiceProvider);
        }
    }
}
