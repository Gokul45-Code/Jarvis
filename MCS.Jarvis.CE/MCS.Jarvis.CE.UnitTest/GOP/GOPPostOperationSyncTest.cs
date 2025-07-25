//-----------------------------------------------------------------------
// <copyright file="GopPostOperationSyncTest.cs" company="Microsoft">
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
    /// GopPostOperationSync Test.
    /// </summary>
    [TestClass]
    public class GopPostOperationSyncTest : UnitTestBase
    {
        /// <summary>
        /// Positive scenario with Mercurius user for GopPostOperationAsync Create Scenario for ADD GOP Triggered
        /// where Gop Approved is true.
        /// </summary>
        [TestMethod]
        public void GopPostOperationSyncTestAsMercuriusCreate_ApprovedTrue()
        {
            var incident = new Entity("incident"); // Create a mock or actual instance of the "incident" Entity
            incident.Id = Guid.NewGuid(); // Set a valid GUID for the "incident" entity's ID
            incident[Incident.caseOriginCode] = new OptionSetValue(2);
            incident[Incident.casetypecode] = new OptionSetValue(2);
            incident["customerid"] = new EntityReference("account", Guid.NewGuid());

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

            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
            {
                new KeyValuePair<string, object>("Target", gopImg),
            };
            this.PluginExecutionContext.MessageNameGet = () => "Create";
            this.PluginExecutionContext.StageGet = () => 40;

            // Setting post-entity image.
            this.PluginExecutionContext.PostEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                    { "PostImage", gopImg },
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

                return null;
            };

            GOPPostOperationSync plugin = new GOPPostOperationSync();

            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Positive scenario with Mercurius user for GopPostOperationAsync Create Scenario for ADD GOP Triggered
        /// where Gop Approved is false.
        /// </summary>
        [TestMethod]
        public void GopPostOperationSyncTestAsMercuriusCreate_ApprovedFalse()
        {
            var incident = new Entity("incident"); // Create a mock or actual instance of the "incident" Entity
            incident.Id = Guid.NewGuid(); // Set a valid GUID for the "incident" entity's ID
            incident[Incident.caseOriginCode] = new OptionSetValue(2);
            incident[Incident.casetypecode] = new OptionSetValue(2);
            incident["customerid"] = new EntityReference("account", Guid.NewGuid());

            var gopImg = new Entity("jarvis_gop");

            gopImg.Attributes[Gop.Source] = new OptionSetValue((int)Source.Jarvis);
            gopImg.Attributes[Gop.Approved] = false;
            gopImg.Attributes[Gop.requestType] = new OptionSetValue((int)GopRequestType.GOP_HD);
            gopImg.Attributes[Casecontact.jarvisIncident] = new EntityReference("incident", Guid.NewGuid());
            gopImg.Attributes["statecode"] = new OptionSetValue(1);
            var createdByRef = new EntityReference("systemuser", Guid.NewGuid());
            createdByRef.Name = "John Doe"; // Set the Name property of createdByRef
            gopImg.Attributes["createdby"] = createdByRef; // Set "createdby" attribute
            gopImg.Attributes["jarvis_paymenttype"] = new OptionSetValue(334030002); // Set "jarvis_paymenttype" attribute
            gopImg.Attributes["jarvis_gopapproval"] = new OptionSetValue(334030000); // Set "jarvis_gopapproval" attribute

            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
            {
                new KeyValuePair<string, object>("Target", gopImg),
            };
            this.PluginExecutionContext.MessageNameGet = () => "Create";
            this.PluginExecutionContext.StageGet = () => 40;

            // Setting post-entity image.
            this.PluginExecutionContext.PostEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                    { "PostImage", gopImg },
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

                return null;
            };

            GOPPostOperationSync plugin = new GOPPostOperationSync();

            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Positive scenario with Mercurius user for GopPostOperationAsync Create Scenario for ADD GOP Triggered
        /// where Status value is not equal to 20.
        /// </summary>
        [TestMethod]
        public void GopPostOperationSyncTestAsMercuriusCreate_ApprovedIsNull()
        {
            var incident = new Entity("incident"); // Create a mock or actual instance of the "incident" Entity
            incident.Id = Guid.NewGuid(); // Set a valid GUID for the "incident" entity's ID
            incident[Incident.caseOriginCode] = new OptionSetValue(2);
            incident[Incident.casetypecode] = new OptionSetValue(2);
            incident["customerid"] = new EntityReference("account", Guid.NewGuid());

            var gopImg = new Entity("jarvis_gop");

            gopImg.Attributes[Gop.Source] = new OptionSetValue((int)Source.Jarvis);

            gopImg.Attributes[Gop.requestType] = new OptionSetValue((int)GopRequestType.GOP_HD);
            gopImg.Attributes[Casecontact.jarvisIncident] = new EntityReference("incident", Guid.NewGuid());
            gopImg.Attributes["statecode"] = new OptionSetValue(1);
            var createdByRef = new EntityReference("systemuser", Guid.NewGuid());
            createdByRef.Name = "John Doe"; // Set the Name property of createdByRef
            gopImg.Attributes["createdby"] = createdByRef; // Set "createdby" attribute
            gopImg.Attributes["jarvis_paymenttype"] = new OptionSetValue(334030002); // Set "jarvis_paymenttype" attribute
            gopImg.Attributes["jarvis_gopapproval"] = new OptionSetValue(334030000); // Set "jarvis_gopapproval" attribute

            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
            {
                new KeyValuePair<string, object>("Target", gopImg),
            };
            this.PluginExecutionContext.MessageNameGet = () => "Create";
            this.PluginExecutionContext.StageGet = () => 40;

            // Setting post-entity image.
            this.PluginExecutionContext.PostEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                    { "PostImage", gopImg },
                };
            };

            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();

                if (query is FetchExpression fetchExpression && fetchExpression.Query.Contains("<entity name='jarvis_gop'>"))
                {
                    var gopHDCollection = new EntityCollection();
                    var entity1 = new Entity("gopHD");
                    gopHDCollection.Entities.Add(entity1);
                    var entity2 = new Entity("gopHD");
                    gopHDCollection.Entities.Add(entity2);
                    var entity3 = new Entity("gopHD");
                    gopHDCollection.Entities.Add(entity3);
                    result = gopHDCollection;
                }

                return result;
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
                    inc["statuscode"] = new OptionSetValue(30);
                    inc["customerid"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
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

                return null;
            };

            GOPPostOperationSync plugin = new GOPPostOperationSync();

            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Positive scenario with Mercurius user for GopPostOperationAsync Create Scenario for ADD GOP Triggered
        /// where for GOPApproval is Approved.
        /// </summary>
        [TestMethod]
        public void GopPostOperationSyncTestAsMercuriusCreate_GOPApproved()
        {
            var incident = new Entity("incident"); // Create a mock or actual instance of the "incident" Entity
            incident.Id = Guid.NewGuid(); // Set a valid GUID for the "incident" entity's ID
            incident[Incident.caseOriginCode] = new OptionSetValue(2);
            incident[Incident.casetypecode] = new OptionSetValue(2);
            incident["customerid"] = new EntityReference("account", Guid.NewGuid());

            var gopImg = new Entity("jarvis_gop");

            gopImg.Attributes[Gop.Source] = new OptionSetValue((int)Source.Jarvis);

            gopImg.Attributes[Gop.requestType] = new OptionSetValue((int)GopRequestType.GOP_HD);
            gopImg.Attributes[Casecontact.jarvisIncident] = new EntityReference("incident", Guid.NewGuid());
            gopImg.Attributes["statecode"] = new OptionSetValue(1);
            var createdByRef = new EntityReference("systemuser", Guid.NewGuid());
            createdByRef.Name = "John Doe"; // Set the Name property of createdByRef
            gopImg.Attributes["createdby"] = createdByRef; // Set "createdby" attribute
            gopImg.Attributes["jarvis_paymenttype"] = new OptionSetValue(334030002); // Set "jarvis_paymenttype" attribute
            gopImg.Attributes["jarvis_gopapproval"] = new OptionSetValue(334030001); ////Approved // Set "jarvis_gopapproval" attribute
            gopImg.Attributes["jarvis_dealer"] = new EntityReference("account", Guid.NewGuid());

            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
            {
                new KeyValuePair<string, object>("Target", gopImg),
            };
            this.PluginExecutionContext.MessageNameGet = () => "Create";
            this.PluginExecutionContext.StageGet = () => 40;

            // Setting post-entity image.
            this.PluginExecutionContext.PostEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                    { "PostImage", gopImg },
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

            GOPPostOperationSync plugin = new GOPPostOperationSync();

            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Positive scenario with Mercurius user for GopPostOperationAsync Update Scenario for ADD GOP Triggered
        /// where Gop Approved is true.
        /// </summary>
        [TestMethod]
        public void GopPostOperationSyncTestAsMercuriusUpdate_ApprovedTrue()
        {
            var incident = new Entity("incident"); // Create a mock or actual instance of the "incident" Entity
            incident.Id = Guid.NewGuid(); // Set a valid GUID for the "incident" entity's ID
            incident[Incident.caseOriginCode] = new OptionSetValue(2);
            incident[Incident.casetypecode] = new OptionSetValue(2);
            incident["customerid"] = new EntityReference("account", Guid.NewGuid());

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

            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
            {
                new KeyValuePair<string, object>("Target", gopImg),
            };
            this.PluginExecutionContext.MessageNameGet = () => "Update";
            this.PluginExecutionContext.StageGet = () => 40;

            // Setting post-entity image.
            this.PluginExecutionContext.PostEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                    { "PostImage", gopImg },
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

                return null;
            };

            GOPPostOperationSync plugin = new GOPPostOperationSync();

            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Positive scenario with Mercurius user for GopPostOperationAsync Update Scenario for ADD GOP Triggered
        /// where Gop Approved is true and status is 30.
        /// </summary>
        [TestMethod]
        public void GopPostOperationSyncTestAsMercuriusUpdate_ApprovedTrueAndStatusis30()
        {
            var incident = new Entity("incident"); // Create a mock or actual instance of the "incident" Entity
            incident.Id = Guid.NewGuid(); // Set a valid GUID for the "incident" entity's ID
            incident[Incident.caseOriginCode] = new OptionSetValue(2);
            incident[Incident.casetypecode] = new OptionSetValue(2);
            incident["customerid"] = new EntityReference("account", Guid.NewGuid());

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
            gopImg.Attributes["jarvis_contact"] = "AUTOMATICALLY";

            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
            {
                new KeyValuePair<string, object>("Target", gopImg),
            };
            this.PluginExecutionContext.MessageNameGet = () => "Update";
            this.PluginExecutionContext.StageGet = () => 40;

            // Setting post-entity image.
            this.PluginExecutionContext.PostEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                    { "PostImage", gopImg },
                };
            };

            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();

                if (query is FetchExpression fetchExpression && fetchExpression.Query.Contains("<entity name='jarvis_language'>"))
                {
                    var languageCollection = new EntityCollection();
                    var entity1 = new Entity("jarvis_language");

                    // Add attributes to the entity
                    entity1.Attributes["jarvis_iso3languagecode6392t"] = "EN"; // Example attribute with value

                    languageCollection.Entities.Add(entity1);
                    result = languageCollection;
                }

                return result;
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
                    inc["statuscode"] = new OptionSetValue(30);
                    inc["customerid"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_callerrole"] = new OptionSetValue(2);
                    inc["jarvis_sourceid"] = new OptionSetValue(334030003);
                    inc["jarvis_country"] = new EntityReference("country", Guid.NewGuid());
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

                return null;
            };

            GOPPostOperationSync plugin = new GOPPostOperationSync();

            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Positive scenario with Mercurius user for GopPostOperationAsync Update Scenario for ADD GOP Triggered
        /// where for GOPApproval is Approved.
        /// </summary>
        [TestMethod]
        public void GopPostOperationSyncTestAsMercuriusUpdate_GOPApproved()
        {
            var incident = new Entity("incident"); // Create a mock or actual instance of the "incident" Entity
            incident.Id = Guid.NewGuid(); // Set a valid GUID for the "incident" entity's ID
            incident[Incident.caseOriginCode] = new OptionSetValue(2);
            incident[Incident.casetypecode] = new OptionSetValue(2);
            incident["customerid"] = new EntityReference("account", Guid.NewGuid());

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
            gopImg.Attributes["jarvis_gopapproval"] = new OptionSetValue(334030001); ////Approved // Set "jarvis_gopapproval" attribute
            gopImg.Attributes["jarvis_dealer"] = new EntityReference("account", Guid.NewGuid());

            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
            {
                new KeyValuePair<string, object>("Target", gopImg),
            };
            this.PluginExecutionContext.MessageNameGet = () => "Update";
            this.PluginExecutionContext.StageGet = () => 40;

            // Setting post-entity image.
            this.PluginExecutionContext.PostEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                    { "PostImage", gopImg },
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

            GOPPostOperationSync plugin = new GOPPostOperationSync();

            plugin.Execute(this.ServiceProvider);
        }
    }
}