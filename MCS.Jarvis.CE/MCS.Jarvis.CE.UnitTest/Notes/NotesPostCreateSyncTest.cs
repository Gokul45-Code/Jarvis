//-----------------------------------------------------------------------
// <copyright file="NotesPostCreateSyncTest.cs" company="Microsoft">
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
    public class NotesPostCreateSyncTest : UnitTestBase
    {
        /// <summary>
        /// Positive Scenario for Notes.
        /// </summary>
        [TestMethod]
        public void NotesPostCreateSyncTestUpdate()
        {
            var incident = new Entity("incident");
            incident.Id = Guid.NewGuid();
            incident[Incident.caseOriginCode] = new OptionSetValue(2);
            incident[Incident.casetypecode] = new OptionSetValue(2);

            var postImg = new Entity("incident");
            postImg.Attributes[Notes.regardingObjectId] = new EntityReference("incident", incident.Id);
            postImg.Attributes[Notes.isdocument] = true;
            var createdByRef = new EntityReference("systemuser", Guid.NewGuid());
            createdByRef.Name = "MERCURIUS"; // Set the Name property of createdByRef
            postImg.Attributes["createdby"] = createdByRef;
            postImg.Attributes["filename"] = "filesample";

            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", incident),
               };

            //// Setting Post Entity Image.
            this.PluginExecutionContext.PostEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PostImage", postImg },
                };
            };

            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();

                if (query is FetchExpression fetchExpression && fetchExpression.Query.Contains("<entity name='jarvis_passout'>"))
                {
                    var entity1 = new Entity("jarvis_passout");
                    entity1["jarvis_repairingdealer"] = new EntityReference("account", Guid.NewGuid());
                    result.Entities.Add(entity1);

                    var entity2 = new Entity("jarvis_passout");
                    entity2["jarvis_repairingdealer"] = new EntityReference("account", Guid.NewGuid());
                    result.Entities.Add(entity2);
                }

                if (query is FetchExpression fetch && fetch.Query.Contains("<entity name='jarvis_configurationjarvis'>"))
                {
                    var entity1 = new Entity("jarvis_configurationjarvis");
                    entity1["jarvis_automationmonitoraction"] = true;
                    result.Entities.Add(entity1);
                }

                if (query is FetchExpression expression && expression.Query.Contains("<entity name='jarvis_casemonitoraction'>"))
                {
                    var entity1 = new Entity("jarvis_casemonitoraction");
                    entity1["statuscode"] = new OptionSetValue(2);
                    result.Entities.Add(entity1);
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
                    account["jarvis_address1_country"] = new EntityReference("country", Guid.NewGuid());
                    return account;
                }

                if (entityName == "incident")
                {
                    Entity inc = new Entity("incident");
                    inc.Id = guid;

                    inc["jarvis_callerlanguage"] = new EntityReference("language", Guid.NewGuid());
                    inc["jarvis_callerrole"] = new EntityReference("role", Guid.NewGuid());
                    inc["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());

                    return inc;
                }

                if (entityName == "country")
                {
                    Entity country = new Entity("country");
                    country.Id = Guid.NewGuid();
                    country["jarvis_iso3countrycode"] = "AUS";
                    country["jarvis_iso2countrycode"] = "AU";
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
            NotesPostCreateSync plugin = new NotesPostCreateSync();

            this.Service.ExecuteOrganizationRequest = (request) =>
            {
                LocalTimeFromUtcTimeResponse response = new LocalTimeFromUtcTimeResponse();
                return response;
            };
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Negative Scenario for Notes.
        /// </summary>
        [TestMethod]
        public void NotesPostCreateSyncTestNoCreatedByUpdate()
        {
            var incident = new Entity("incident");
            incident.Id = Guid.NewGuid();
            incident[Incident.caseOriginCode] = new OptionSetValue(2);
            incident[Incident.casetypecode] = new OptionSetValue(2);

            var postImg = new Entity("incident");
            postImg.Attributes[Notes.regardingObjectId] = new EntityReference("incident", incident.Id);
            postImg.Attributes[Notes.isdocument] = true;
            postImg.Attributes["filename"] = "filesample";

            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", incident),
               };

            //// Setting Post Entity Image.
            this.PluginExecutionContext.PostEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PostImage", postImg },
                };
            };

            NotesPostCreateSync plugin = new NotesPostCreateSync();
            plugin.Execute(this.ServiceProvider);

        }

        /// <summary>
        /// Negative Scenario for Notes.
        /// </summary>
        [TestMethod]
        public void NotesPostCreateSyncTestNullCreatedByUpdate()
        {
            var incident = new Entity("incident");
            incident.Id = Guid.NewGuid();
            incident[Incident.caseOriginCode] = new OptionSetValue(2);
            incident[Incident.casetypecode] = new OptionSetValue(2);

            var postImg = new Entity("incident");
            postImg.Attributes[Notes.regardingObjectId] = new EntityReference("incident", incident.Id);
            postImg.Attributes[Notes.isdocument] = true;
            postImg.Attributes["filename"] = "filesample";
            postImg.Attributes["createdby"] = null;

            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", incident),
               };

            //// Setting Post Entity Image.
            this.PluginExecutionContext.PostEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PostImage", postImg },
                };
            };

            NotesPostCreateSync plugin = new NotesPostCreateSync();
            plugin.Execute(this.ServiceProvider);

        }
    }
}