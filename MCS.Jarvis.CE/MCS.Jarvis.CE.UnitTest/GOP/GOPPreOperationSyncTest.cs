//-----------------------------------------------------------------------
// <copyright file="GOPPreOperationSyncTest.cs" company="Microsoft">
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
    /// GOPPreOperationSync Test.
    /// </summary>
    [TestClass]
    public class GOPPreOperationSyncTest : UnitTestBase
    {
        /// <summary>
        /// Positive scenario with user for GOP PreOperationSync Create Scenario
        /// where GOP Approved is true.
        /// </summary>
        [TestMethod]
        public void GOPPreOperationSyncTestAsMercuriusCreate()
        {
            var gopImg = new Entity("jarvis_gop");

            gopImg.Attributes[Gop.Source] = new OptionSetValue((int)Source.Jarvis);
            gopImg.Attributes[Gop.Approved] = true;
            gopImg.Attributes[Gop.requestType] = new OptionSetValue((int)GopRequestType.GOP_HD);
            gopImg.Attributes[Casecontact.jarvisIncident] = new EntityReference("incident", Guid.NewGuid());
            gopImg.Attributes["statecode"] = new OptionSetValue(1);
            var createdByRef = new EntityReference("systemuser", Guid.NewGuid());
            createdByRef.Name = "John Doe"; // Set the Name property of createdByRef
            gopImg.Attributes["createdby"] = createdByRef; // Set "createdby" attribute
            gopImg.Attributes["jarvis_paymenttype"] = new OptionSetValue(334030002); // Set "jarvis_paymenttype" attribute
            gopImg.Attributes["jarvis_gopapproval"] = new OptionSetValue(334030000); // Set "jarvis_gopapproval" attribute
            gopImg.Attributes[Gop.jarvis_goplimitin] = 200.54M;
            gopImg.Attributes[Gop.jarvis_goplimitout] = 400.34M;
            gopImg.Attributes["jarvis_gopoutcurrency"] = new EntityReference("gopoutcurrency", Guid.Empty);
            gopImg.Attributes["jarvis_gopincurrency"] = new EntityReference("gopincurrency", Guid.NewGuid());

            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
            {
                new KeyValuePair<string, object>("Target", gopImg),
            };
            this.PluginExecutionContext.MessageNameGet = () => "Create";

            // Setting service retrieve.
            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "account")
                {
                    Entity account = new Entity("account");
                    account.Id = Guid.NewGuid();
                    account["jarvis_language"] = new EntityReference("language", Guid.NewGuid());
                    account["jarvis_country"] = new EntityReference("country", Guid.NewGuid());
                    return account;
                }

                if (entityName == "incident")
                {
                    Entity inc = new Entity("incident");
                    inc.Id = guid;
                    inc["casetypecode"] = new OptionSetValue(2);
                    inc["statuscode"] = new OptionSetValue(20);
                    inc["customerid"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_caseserviceline"] = new EntityReference("caseserviceline", Guid.NewGuid());
                    return inc;
                }

                if (entityName == "country")
                {
                    Entity country = new Entity("country");
                    country.Id = Guid.NewGuid();
                    country["jarvis_iso2countrycode"] = "AUS";
                    return country;
                }

                if (entityName == "language")
                {
                    Entity language = new Entity("language");
                    language.Id = Guid.NewGuid();
                    language["jarvis_iso3languagecode6392t"] = "AUS";
                    return language;
                }

                if (entityName == "caseserviceline")
                {
                    Entity caseserviceline = new Entity("caseserviceline");
                    caseserviceline.Id = Guid.NewGuid();
                    caseserviceline["jarvis_servicefee"] = new Money(100); // Example service fee
                    caseserviceline["transactioncurrencyid"] = new EntityReference("transactioncurrency", Guid.NewGuid());
                    return caseserviceline;
                }

                if (entityName == "transactioncurrency")
                {
                    Entity transactioncurrency = new Entity("transactioncurrency");
                    transactioncurrency.Id = Guid.NewGuid();
                    return transactioncurrency;
                }

                return null;
            };
            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();
                if (query.GetType().Name == "FetchExpression")
                {
                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_exchangerate'>"))
                    {
                        Entity exchangeRate = new Entity("jarvis_exchangerate");
                        exchangeRate["jarvis_name"] = "testpassout";
                        exchangeRate["jarvis_value"] = Convert.ToDecimal(1);
                        exchangeRate["createdon"] = DateTime.UtcNow;
                        exchangeRate["modifiedon"] = DateTime.UtcNow;
                        exchangeRate.Id = Guid.NewGuid();
                        result.Entities.Add(exchangeRate);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_gop'>"))
                    {
                        Entity gopRecord = new Entity("jarvis_gop");

                        gopRecord.Attributes[Gop.Source] = new OptionSetValue((int)Source.Jarvis);
                        gopRecord.Attributes[Gop.Approved] = true;
                        gopRecord.Attributes[Gop.requestType] = new OptionSetValue((int)GopRequestType.GOP_HD);
                        gopRecord.Attributes[Casecontact.jarvisIncident] = new EntityReference("incident", Guid.NewGuid());
                        gopRecord.Attributes["statecode"] = new OptionSetValue(1);
                        gopRecord.Attributes["createdby"] = createdByRef; // Set "createdby" attribute
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
            GOPPreOperationSync plugin = new GOPPreOperationSync();

            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Positive scenario with user for GOP PreOperationSync Update Scenario
        /// where GOP Approved is true.
        /// </summary>
        [TestMethod]
        public void GOPPreOperationSyncTestAsMercuriusUpdate()
        {
            var gop = new Entity("jarvis_gop");
            gop.Attributes[Gop.Source] = new OptionSetValue((int)Source.Jarvis);
            gop.Attributes[Gop.requestType] = new OptionSetValue((int)GopRequestType.GOP_HD);
            gop.Attributes["jarvis_relatedgop"] = new EntityReference("jarvis_gop", Guid.NewGuid());
            gop.Attributes[Casecontact.jarvisIncident] = new EntityReference("incident", Guid.NewGuid());
            gop.Attributes["statecode"] = new OptionSetValue(1);
            gop.Attributes["jarvis_paymenttype"] = new OptionSetValue(334030002); // Set "jarvis_paymenttype" attribute
            gop.Attributes["jarvis_gopapproval"] = new OptionSetValue(334030000); // Set "jarvis_gopapproval" attribute
            gop.Attributes[Gop.jarvis_goplimitin] = 200.54M;
            gop.Attributes[Gop.jarvis_goplimitout] = 400.34M;
            gop.Attributes["jarvis_gopoutcurrency"] = new EntityReference("gopoutcurrency", Guid.Empty);
            gop.Attributes["jarvis_gopincurrency"] = new EntityReference("gopincurrency", Guid.NewGuid());

            var gopImg = new Entity("jarvis_gop");
            gopImg.Attributes[Gop.Source] = new OptionSetValue((int)Source.Jarvis);
            gopImg.Attributes[Gop.Approved] = true;
            gopImg.Attributes[Gop.requestType] = new OptionSetValue((int)GopRequestType.GOP_HD);
            gopImg.Attributes[Casecontact.jarvisIncident] = new EntityReference("incident", Guid.NewGuid());
            gopImg.Attributes["statecode"] = new OptionSetValue(1);
            var createdByRef = new EntityReference("systemuser", Guid.NewGuid());
            createdByRef.Name = "John Doe"; // Set the Name property of createdByRef
            gopImg.Attributes["createdby"] = createdByRef; // Set "createdby" attribute
            gopImg.Attributes["jarvis_paymenttype"] = new OptionSetValue(334030002); // Set "jarvis_paymenttype" attribute
            gopImg.Attributes["jarvis_gopapproval"] = new OptionSetValue(334030000); // Set "jarvis_gopapproval" attribute
            gopImg.Attributes[Gop.jarvis_goplimitin] = 200.54M;
            gopImg.Attributes[Gop.jarvis_goplimitout] = 400.34M;
            gopImg.Attributes["jarvis_gopoutcurrency"] = new EntityReference("gopoutcurrency", Guid.Empty);
            gopImg.Attributes["jarvis_gopincurrency"] = new EntityReference("gopincurrency", Guid.NewGuid());

            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
            {
                new KeyValuePair<string, object>("Target", gop),
            };
            this.PluginExecutionContext.MessageNameGet = () => "Update";

            // Setting post-entity image.
            this.PluginExecutionContext.PreEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                    { "PreGOPImage", gopImg },
                };
            };

            // Setting service retrieve.
            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "account")
                {
                    Entity account = new Entity("account");
                    account.Id = Guid.NewGuid();
                    account["jarvis_language"] = new EntityReference("language", Guid.NewGuid());
                    account["jarvis_country"] = new EntityReference("country", Guid.NewGuid());
                    return account;
                }

                if (entityName == "incident")
                {
                    Entity inc = new Entity("incident");
                    inc.Id = guid;
                    inc["casetypecode"] = new OptionSetValue(2);
                    inc["statuscode"] = new OptionSetValue(20);
                    inc["customerid"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_caseserviceline"] = new EntityReference("caseserviceline", Guid.NewGuid());
                    return inc;
                }

                if (entityName == "country")
                {
                    Entity country = new Entity("country");
                    country.Id = Guid.NewGuid();
                    country["jarvis_iso2countrycode"] = "AUS";
                    return country;
                }

                if (entityName == "language")
                {
                    Entity language = new Entity("language");
                    language.Id = Guid.NewGuid();
                    language["jarvis_iso3languagecode6392t"] = "AUS";
                    return language;
                }

                if (entityName == "caseserviceline")
                {
                    Entity caseserviceline = new Entity("caseserviceline");
                    caseserviceline.Id = Guid.NewGuid();
                    caseserviceline["jarvis_servicefee"] = new Money(100); // Example service fee
                    caseserviceline["transactioncurrencyid"] = new EntityReference("transactioncurrency", Guid.NewGuid());
                    return caseserviceline;
                }

                if (entityName == "transactioncurrency")
                {
                    Entity transactioncurrency = new Entity("transactioncurrency");
                    transactioncurrency.Id = Guid.NewGuid();
                    return transactioncurrency;
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
                        Entity gopRecord = new Entity("jarvis_gop");

                        gopRecord.Attributes[Gop.Source] = new OptionSetValue((int)Source.Jarvis);
                        gopRecord.Attributes[Gop.Approved] = true;
                        gopRecord.Attributes[Gop.requestType] = new OptionSetValue((int)GopRequestType.GOP_HD);
                        gopRecord.Attributes[Casecontact.jarvisIncident] = new EntityReference("incident", Guid.NewGuid());
                        gopRecord.Attributes["statecode"] = new OptionSetValue(1);
                        gopRecord.Attributes["createdby"] = createdByRef; // Set "createdby" attribute
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
            GOPPreOperationSync plugin = new GOPPreOperationSync();

            plugin.Execute(this.ServiceProvider);
        }
    }
}