// <copyright file="CasePostOperationAsyncTest.cs" company="PlaceholderCompany">
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
    using static MCS.Jarvis.CE.BusinessProcessesShared.Helpers.Constants;

    /// <summary>
    /// Case Post Operation Async Test.
    /// </summary>
    [TestClass]
    public class CasePostOperationAsyncTest : UnitTestBase
    {
        /// <summary>
        /// Positive scenario for CasePostOperationAsyncTest Create Scenario.
        /// </summary>
        [TestMethod]
        public void CasePostOperationAsyncTestCreate()
        {
            Entity incident = new Entity("incident");
            incident.Id = Guid.Empty;
            incident["statuscode"] = new OptionSetValue(20);
            incident["caseorigincode"] = new OptionSetValue(334030002);
            incident["jarvis_mercuriusstatus"] = new OptionSetValue(200);
            incident["customerid"] = new EntityReference("account", Guid.NewGuid());
            incident[Incident.JarvisRestGopLimitOut] = Convert.ToDecimal(10);
            Entity postEntityImage = new Entity("incident");
            postEntityImage["customerid"] = new EntityReference("account", Guid.NewGuid());
            postEntityImage[Incident.HomeDealer] = new EntityReference("account", Guid.NewGuid());
            postEntityImage.Attributes[Incident.CallerRole] = new OptionSetValue(4);
            postEntityImage[Incident.JarvisRestGopLimitOut] = Convert.ToDecimal(20);
            postEntityImage[Incident.JarvisTotalRestCurrencyOut] = new EntityReference("jarvis_currency", Guid.NewGuid());
            //// Setting Input Parameters.
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", incident),
               };
            Entity preEntityImage = new Entity("incident");
            preEntityImage["customerid"] = new EntityReference("account", Guid.NewGuid());
            preEntityImage[Incident.HomeDealer] = new EntityReference("account", Guid.NewGuid());
            preEntityImage.Attributes[Incident.CallerRole] = new OptionSetValue(4);
            preEntityImage[Incident.JarvisRestGopLimitOut] = Convert.ToDecimal(10);
            this.PluginExecutionContext.MessageNameGet = () => "Update";
            this.PluginExecutionContext.StageGet = () => 40;
            //// Setting Post and Pre Image
            this.PluginExecutionContext.PostEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PostImage", postEntityImage },
                };
            };
            this.PluginExecutionContext.PreEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PreImage", preEntityImage },
                };
            };
            this.PluginExecutionContext.DepthGet = () => 1;
            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();
                if (query.GetType().Name == "FetchExpression")
                {
                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_gop'>"))
                    {
                Entity gopRecord = new Entity("jarvis_gop");

                gopRecord.Attributes[Gop.Source] = new OptionSetValue((int)Source.Jarvis);
                gopRecord.Attributes[Gop.Approved] = true;
                gopRecord.Attributes[Gop.requestType] = new OptionSetValue((int)GopRequestType.GOP_HD);
                gopRecord.Attributes[Casecontact.jarvisIncident] = new EntityReference("incident", Guid.NewGuid());
                gopRecord.Attributes["statecode"] = new OptionSetValue(1);
                gopRecord.Attributes["jarvis_paymenttype"] = new OptionSetValue(334030002); // Set "jarvis_paymenttype" attribute
                gopRecord.Attributes["jarvis_gopapproval"] = new OptionSetValue(334030000); // Set "jarvis_gopapproval" attribute
                gopRecord.Attributes[Gop.jarvis_goplimitin] = 200.54M;
                gopRecord.Attributes[Gop.jarvis_goplimitout] = 400.34M;
                gopRecord.Attributes["jarvis_gopoutcurrency"] = new EntityReference("gopoutcurrency", Guid.Empty);
                gopRecord.Attributes["jarvis_gopincurrency"] = new EntityReference("gopincurrency", Guid.NewGuid());
                gopRecord["modifiedon"] = DateTime.UtcNow;
                gopRecord.Attributes["jarvis_dealer"] = new EntityReference("account", Guid.NewGuid());
                gopRecord.Attributes["jarvis_creditcardincurrency"] = new EntityReference("gopoutcurrency", Guid.NewGuid());
                gopRecord.Attributes["jarvis_creditcardgopinbooking"] = 200.54M;
                result.Entities.Add(gopRecord);
            }
                }

                return result;
            };
            CasePostOperationAsync plugin = new CasePostOperationAsync();

            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Positive scenario for CasePostOperationAsyncTest No Post Image Scenario.
        /// </summary>
        [TestMethod]
        public void CasePostOperationAsyncTestNoPostImg()
        {
            Entity incident = new Entity("incident");
            incident.Id = Guid.Empty;
            incident["statuscode"] = new OptionSetValue(20);
            incident["caseorigincode"] = new OptionSetValue(334030002);
            incident["jarvis_mercuriusstatus"] = new OptionSetValue(200);
            incident["customerid"] = new EntityReference("account", Guid.NewGuid());
            incident[Incident.JarvisRestGopLimitOut] = Convert.ToDecimal(10);
            Entity postEntityImage = new Entity("incident");
            postEntityImage["customerid"] = new EntityReference("account", Guid.NewGuid());
            postEntityImage[Incident.HomeDealer] = new EntityReference("account", Guid.NewGuid());
            postEntityImage.Attributes[Incident.CallerRole] = new OptionSetValue(4);
            postEntityImage[Incident.JarvisRestGopLimitOut] = Convert.ToDecimal(20);
            postEntityImage[Incident.JarvisTotalRestCurrencyOut] = new EntityReference("jarvis_currency", Guid.NewGuid());
            //// Setting Input Parameters.
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", incident),
               };
            Entity preEntityImage = new Entity("incident");
            preEntityImage["customerid"] = new EntityReference("account", Guid.NewGuid());
            preEntityImage[Incident.HomeDealer] = new EntityReference("account", Guid.NewGuid());
            preEntityImage.Attributes[Incident.CallerRole] = new OptionSetValue(4);
            preEntityImage[Incident.JarvisRestGopLimitOut] = Convert.ToDecimal(10);
            this.PluginExecutionContext.MessageNameGet = () => "Update";
            this.PluginExecutionContext.StageGet = () => 40;
            //// Setting Pre Image
            this.PluginExecutionContext.PreEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PreImage", preEntityImage },
                };
            };
            this.PluginExecutionContext.DepthGet = () => 1;
            CasePostOperationAsync plugin = new CasePostOperationAsync();

            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Positive scenario for CasePostOperationAsyncTest Exception Scenario.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidPluginExecutionException))]
        public void CasePostOperationAsyncTestException()
        {
            CasePostOperationAsync plugin = new CasePostOperationAsync();
            plugin.Execute(null);
        }

        /// <summary>
        /// Positive scenario for CasePostOperationAsyncTest Error Scenario.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(NullReferenceException))]
        public void CasePostOperationAsyncTestError()
        {
            Entity incident = new Entity("incident");
            incident.Id = Guid.Empty;
            incident["statuscode"] = new OptionSetValue(20);
            incident["caseorigincode"] = new OptionSetValue(334030002);
            incident["jarvis_mercuriusstatus"] = new OptionSetValue(200);
            incident["customerid"] = new EntityReference("account", Guid.NewGuid());
            incident[Incident.JarvisRestGopLimitOut] = Convert.ToDecimal(10);
            this.PluginExecutionContext = null;
            CasePostOperationAsync plugin = new CasePostOperationAsync();

            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Positive scenario for CasePostOperationAsyncTest Depth 3 Scenario.
        /// </summary>
        [TestMethod]
        public void CasePostOperationAsyncTestDepth()
        {
            Entity incident = new Entity("incident");
            incident.Id = Guid.Empty;
            incident["statuscode"] = new OptionSetValue(20);
            incident["caseorigincode"] = new OptionSetValue(334030002);
            incident["jarvis_mercuriusstatus"] = new OptionSetValue(200);
            incident["customerid"] = new EntityReference("account", Guid.NewGuid());
            incident[Incident.JarvisRestGopLimitOut] = Convert.ToDecimal(10);
            Entity postEntityImage = new Entity("incident");
            postEntityImage["customerid"] = new EntityReference("account", Guid.NewGuid());
            postEntityImage[Incident.HomeDealer] = new EntityReference("account", Guid.NewGuid());
            postEntityImage.Attributes[Incident.CallerRole] = new OptionSetValue(4);
            postEntityImage[Incident.JarvisRestGopLimitOut] = Convert.ToDecimal(20);
            postEntityImage[Incident.JarvisTotalRestCurrencyOut] = new EntityReference("jarvis_currency", Guid.NewGuid());
            //// Setting Input Parameters.
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", incident),
               };
            Entity preEntityImage = new Entity("incident");
            preEntityImage["customerid"] = new EntityReference("account", Guid.NewGuid());
            preEntityImage[Incident.HomeDealer] = new EntityReference("account", Guid.NewGuid());
            preEntityImage.Attributes[Incident.CallerRole] = new OptionSetValue(4);
            preEntityImage[Incident.JarvisRestGopLimitOut] = Convert.ToDecimal(10);
            this.PluginExecutionContext.MessageNameGet = () => "Update";
            this.PluginExecutionContext.StageGet = () => 40;
            this.PluginExecutionContext.DepthGet = () => 3;
            CasePostOperationAsync plugin = new CasePostOperationAsync();

            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Positive scenario for CasePostOperationAsyncTest Invalid error Scenario.
        /// </summary>
        [ExpectedException(typeof(InvalidPluginExecutionException))]
        [TestMethod]
        public void CasePostOperationAsyncTestInvalidError()
        {
            Entity incident = new Entity("incident");
            incident.Id = Guid.Empty;
            incident["statuscode"] = new OptionSetValue(20);
            incident["caseorigincode"] = new OptionSetValue(334030002);
            incident["jarvis_mercuriusstatus"] = new OptionSetValue(200);
            incident["customerid"] = new EntityReference("account", Guid.NewGuid());
            incident[Incident.JarvisRestGopLimitOut] = Convert.ToDecimal(10);
            Entity postEntityImage = new Entity("incident");
            postEntityImage["customerid"] = new EntityReference("account", Guid.NewGuid());
            postEntityImage[Incident.HomeDealer] = new EntityReference("account", Guid.NewGuid());
            postEntityImage.Attributes[Incident.CallerRole] = new OptionSetValue(4);
            postEntityImage[Incident.JarvisRestGopLimitOut] = Convert.ToDecimal(20);
            //// Setting Input Parameters.
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", incident),
               };
            Entity preEntityImage = new Entity("incident");
            preEntityImage["customerid"] = new EntityReference("account", Guid.NewGuid());
            preEntityImage[Incident.HomeDealer] = new EntityReference("account", Guid.NewGuid());
            preEntityImage.Attributes[Incident.CallerRole] = new OptionSetValue(4);
            preEntityImage[Incident.JarvisRestGopLimitOut] = Convert.ToDecimal(10);
            this.PluginExecutionContext.MessageNameGet = () => "Update";
            this.PluginExecutionContext.StageGet = () => 40;
            //// Setting Post and Pre Image
            this.PluginExecutionContext.PostEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PostImage", postEntityImage },
                };
            };
            this.PluginExecutionContext.PreEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PreImage", preEntityImage },
                };
            };
            this.PluginExecutionContext.DepthGet = () => 1;
            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();
                if (query.GetType().Name == "FetchExpression")
                {
                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_gop'>"))
                    {
                        Entity gopRecord = new Entity("jarvis_gop");

                        gopRecord.Attributes[Gop.Source] = new OptionSetValue((int)Source.Jarvis);
                        gopRecord.Attributes[Gop.Approved] = true;
                        gopRecord.Attributes[Gop.requestType] = new OptionSetValue((int)GopRequestType.GOP_HD);
                        gopRecord.Attributes[Casecontact.jarvisIncident] = new EntityReference("incident", Guid.NewGuid());
                        gopRecord.Attributes["statecode"] = new OptionSetValue(1);
                        gopRecord.Attributes["jarvis_paymenttype"] = new OptionSetValue(334030002); // Set "jarvis_paymenttype" attribute
                        gopRecord.Attributes["jarvis_gopapproval"] = new OptionSetValue(334030000); // Set "jarvis_gopapproval" attribute
                        gopRecord.Attributes[Gop.jarvis_goplimitin] = 200.54M;
                        gopRecord.Attributes[Gop.jarvis_goplimitout] = 400.34M;
                        gopRecord.Attributes["jarvis_gopoutcurrency"] = new EntityReference("gopoutcurrency", Guid.Empty);
                        gopRecord.Attributes["jarvis_gopincurrency"] = new EntityReference("gopincurrency", Guid.NewGuid());
                        gopRecord["modifiedon"] = DateTime.UtcNow;
                        gopRecord.Attributes["jarvis_dealer"] = new EntityReference("account", Guid.NewGuid());
                        gopRecord.Attributes["jarvis_creditcardincurrency"] = new EntityReference("gopoutcurrency", Guid.NewGuid());
                        gopRecord.Attributes["jarvis_creditcardgopinbooking"] = 200.54M;
                        result.Entities.Add(gopRecord);
                    }
                }

                return result;
            };
            CasePostOperationAsync plugin = new CasePostOperationAsync();

            plugin.Execute(this.ServiceProvider);
        }
    }
}
