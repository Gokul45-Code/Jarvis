//-----------------------------------------------------------------------
// <copyright file="CasePostOperationSyncTest.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace MCS.Jarvis.CE.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Controls;
    using MCS.Jarvis.CE.Plugins;
    using Microsoft.Crm.Sdk.Messages;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;
    using Newtonsoft.Json;
    using static MCS.Jarvis.CE.BusinessProcessesShared.Helpers.Constants;
    using Constants = MCS.Jarvis.CE.BusinessProcessesShared.Helpers.Constants;

    /// <summary>
    /// CasePostOperationSync Test.
    /// </summary>
    [TestClass]
    public class CasePostOperationSyncTest : UnitTestBase
    {
        /// <summary>
        /// PreValidate Update With Complete Data Test Method Create case type code 2.
        /// </summary>
        [TestMethod]
        public void PostUpdateWithCompleteDataTestMethodCreateCaseType2()
        {
            Entity incidentTarget = new Entity("incident");

            EntityReference users = new EntityReference("systemuser", Guid.NewGuid());
            users.Name = "MERCURIUS";

            incidentTarget.Id = Guid.Empty;

            incidentTarget["statuscode"] = new OptionSetValue(10);
            incidentTarget["incidentid"] = Guid.NewGuid();
            incidentTarget["caseorigincode"] = new OptionSetValue(2);
            incidentTarget["casetypecode"] = new OptionSetValue(2);
            incidentTarget["jarvis_mercuriusstatus"] = new OptionSetValue(200);
            incidentTarget["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
            incidentTarget["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["customerid"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["jarvis_registrationnumber"] = "123";
            incidentTarget["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incidentTarget["description"] = "This is for testing";
            incidentTarget["jarvis_location"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incidentTarget["jarvis_customerexpectations"] = "test";
            incidentTarget["jarvis_incidentnature"] = "[{\"id\":\"3efba61a-85ad-44b7-a187-7f54acaf7a67\"}]";
            incidentTarget["jarvis_callerrole"] = new OptionSetValue(4);
            incidentTarget["jarvis_callerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incidentTarget["isescalated"] = true;
            incidentTarget["createdon"] = DateTime.UtcNow;
            incidentTarget["jarvis_assistancetype"] = new OptionSetValue(334030000);
            incidentTarget.Attributes["modifiedby"] = users;

            Entity incident = new Entity("incident");
            incident.Id = Guid.Empty;
            incident["statuscode"] = new OptionSetValue(10);
            incident["incidentid"] = new EntityReference("incident", Guid.NewGuid());
            incident["caseorigincode"] = new OptionSetValue(2);
            incident["casetypecode"] = new OptionSetValue(2);
            incident["jarvis_mercuriusstatus"] = new OptionSetValue(200);
            incident["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
            incident["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incident["customerid"] = new EntityReference("account", Guid.NewGuid());
            incident["jarvis_registrationnumber"] = "123";
            incident["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["description"] = "This is for testing";
            incident["jarvis_driverlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incident["jarvis_translationstatusreportedfault"] = new OptionSetValue(334030000);
            incident["jarvis_translationstatuslocation"] = new OptionSetValue(334030000);
            incident["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["jarvis_translationstatuscustomerexpectations"] = new OptionSetValue(334030000);
            incident["jarvis_location"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["jarvis_customerexpectations"] = "test";
            incident["ownerid"] = new EntityReference { LogicalName = "system_user", Name = "test" };
            incident["jarvis_sourceid"] = 232323232;
            incident["jarvis_callerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incident["jarvis_callerrole"] = new OptionSetValue(4);
            incident["createdby"] = users;
            incident.Attributes["jarvis_etc"] = DateTime.Now;
            //// Setting Input Parameters.
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", incidentTarget),
               };
            this.PluginExecutionContext.MessageNameGet = () => "CREATE";
            this.PluginExecutionContext.StageGet = () => 40;
            //// Setting Post and Pre Image
            this.PluginExecutionContext.PostEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PostImage", incident },
                };
            };
            this.PluginExecutionContext.PreEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PreImage", incident },
                };
            };
            this.PluginExecutionContext.SharedVariablesGet = () =>
            {
                return new ParameterCollection();
            };

            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();
                if (query.GetType().Name == "FetchExpression")
                {
                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_vasbreakdownprocess'>"))
                    {
                        Entity vasprocess = new Entity("jarvis_vasbreakdownprocess");
                        vasprocess["businessprocessflowinstanceid"] = Guid.NewGuid();
                        vasprocess["bpf_name"] = "test";
                        vasprocess["bpf_incidentid"] = Guid.NewGuid();
                        vasprocess["activestageid"] = Guid.NewGuid();
                        vasprocess["statecode"] = new OptionSetValue(0);
                        vasprocess["createdon"] = DateTime.UtcNow;
                        vasprocess["createdby"] = "test";
                        vasprocess.Id = Guid.NewGuid();
                        result.Entities.Add(vasprocess);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='incident'>"))
                    {
                        Entity incidentResult = new Entity("incident");
                        incidentResult[Constants.IncidentNature.IncidentIncidentNature_jarvisIincidentnatureid] = new AliasedValue("incidentNature", "incidentnatureid", Guid.NewGuid());
                        incidentResult["jarvis_hdrd"] = false;
                        incidentResult["jarvis_homedealer"] = new EntityReference("account", incident.Id);
                        incidentResult["CaseRD.jarvis_repairingdealer"] = new AliasedValue("account", "account", new EntityReference("account", incident.Id));
                        incidentResult.Id = Guid.NewGuid();
                        result.Entities.Add(incidentResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_casecontact'>"))
                    {
                        Entity caseContact = new Entity("jarvis_casecontact");
                        caseContact["jarvis_name"] = "testcontact";
                        caseContact["jarvis_preferredlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
                        caseContact["createdon"] = DateTime.UtcNow;
                        caseContact["jarvis_mobilephone"] = "9248428492";
                        caseContact["jarvis_role"] = new OptionSetValue(2);
                        caseContact.Id = Guid.NewGuid();
                        result.Entities.Add(caseContact);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='contact'>"))
                    {
                        Entity caseContact = new Entity("contact");
                        caseContact["jarvis_name"] = "testcontact";
                        caseContact["jarvis_firstname"] = "testcontact";
                        caseContact["jarvis_language"] = new EntityReference("jarvis_language", Guid.NewGuid());
                        caseContact["createdon"] = DateTime.UtcNow;
                        caseContact["telephone1"] = "9248428492";
                        caseContact["jarvis_role"] = new OptionSetValue(2);
                        caseContact["parentcustomerid"] = new EntityReference("account", incident.Id);
                        caseContact.Id = Guid.NewGuid();
                        caseContact.Attributes["preferredcontactmethodcode"] = new OptionSetValue(1);
                        result.Entities.Add(caseContact);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='workflow'>"))
                    {
                        Entity scheduleConsent = new Entity("workflow");
                        scheduleConsent["name"] = "BreakdownProcess";
                        scheduleConsent.Id = Guid.NewGuid();
                        result.Entities.Add(scheduleConsent);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_configurationjarvis'>"))
                    {
                        Entity configResult = new Entity("jarvis_configurationjarvis");
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationcasestatuschange"] = true;
                        configResult["createdon"] = DateTime.UtcNow;
                        configResult.Id = Guid.NewGuid();
                        result.Entities.Add(configResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_integrationconfiguration'>"))
                    {
                        Entity configResult = new Entity("jarvis_integrationconfiguration");
                        configResult["jarvis_integrationname"] = "test";
                        configResult["jarvis_integrationcode"] = "test";
                        configResult["createdon"] = DateTime.UtcNow;
                        configResult.Id = Guid.NewGuid();
                        result.Entities.Add(configResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_language'>"))
                    {
                        Entity language = new Entity("jarvis_language");
                        language.Id = Guid.NewGuid();
                        language["jarvis_iso3languagecode6392t"] = "DEU";
                        language["jarvis_iso2languagecode6391"] = "DE";
                        language["jarvis_name"] = "DE";
                        result.Entities.Add(language);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_casesoftoffer'>"))
                    {
                        Entity casesoftoffer = new Entity("jarvis_casesoftoffer");
                        casesoftoffer.Id = Guid.NewGuid();
                        casesoftoffer["jarvis_name"] = "DEU";
                        casesoftoffer["jarvis_contractno"] = "DE";
                        casesoftoffer["jarvis_description"] = "DE";
                        casesoftoffer["jarvis_startdate"] = DateTime.UtcNow;
                        casesoftoffer["jarvis_expirydate"] = DateTime.UtcNow;
                        casesoftoffer["statuscode"] = new OptionSetValue(0);
                        casesoftoffer["jarvis_softoffercodelookup"] = new EntityReference("jarvis_softoffer", Guid.NewGuid());
                        result.Entities.Add(casesoftoffer);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_softoffer'>"))
                    {
                        Entity softoffer = new Entity("jarvis_softoffer");
                        softoffer.Id = Guid.NewGuid();
                        softoffer["jarvis_name"] = "DEU";
                        softoffer["jarvis_softoffercode"] = new EntityReference("jarvis_softoffer", Guid.NewGuid());
                        softoffer["jarvis_contractno"] = "DE";
                        softoffer["jarvis_startdate"] = DateTime.UtcNow;
                        softoffer["jarvis_expirydate"] = DateTime.UtcNow;
                        softoffer["statuscode"] = new OptionSetValue(0);
                        softoffer["jarvis_marketcode"] = "DE";
                        softoffer["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
                        result.Entities.Add(softoffer);
                    }

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
                        passoutResult.Id = Guid.NewGuid();
                        result.Entities.Add(passoutResult);
                    }
                }

                return result;
            };

            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "incident")
                {
                    Entity inc = new Entity("incident");
                    inc.Id = guid;
                    inc["customerid"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_callerlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    inc["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    inc["jarvis_driverlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    inc["jarvis_translationstatusreportedfault"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatuslocation"] = new OptionSetValue(334030000);
                    inc["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    inc["jarvis_translationstatuscustomerexpectations"] = new OptionSetValue(334030000);
                    return inc;
                }

                if (entityName == "jarvis_vehicle")
                {
                    Entity client = new Entity("jarvis_vehicle");
                    client.Id = new Guid("6186f7f1-5f09-ed11-82e5-002248934e82");
                    client["jarvis_brandid"] = new EntityReference("jarvis_brand", Guid.NewGuid());
                    client["jarvis_fuelpowertype"] = new EntityReference("jarvis_vehiclefuelpowertype", Guid.NewGuid());
                    return client;
                }

                if (entityName == "jarvis_country")
                {
                    Entity country = new Entity("jarvis_country");
                    country.Id = guid;
                    country["jarvis_iso2countrycode"] = "NL";
                    country.Attributes[JarvisCountry.JarvisAverageetaduration] = "12";
                    return country;
                }

                if (entityName == "jarvis_language")
                {
                    Entity language = new Entity("jarvis_language");
                    language.Id = guid;
                    language["jarvis_iso3languagecode6392t"] = "DEU";
                    language["jarvis_iso2languagecode6391"] = "DE";
                    return language;
                }

                if (entityName == "account")
                {
                    Entity account = new Entity("account");
                    account.Id = guid;
                    account["jarvis_language"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    account["jarvis_address1_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    return account;
                }

                if (entityName == "jarvis_brand")
                {
                    Entity client = new Entity("jarvis_brand");
                    client.Id = new Guid("6186f7f1-5f09-ed11-82e5-002248934e82");
                    client["jarvis_mercuriusbrandcode"] = "ted";
                    return client;
                }

                return null;
            };
            this.Service.ExecuteOrganizationRequest = processInstanceRequest =>
            {
                if (processInstanceRequest.RequestName.Contains("RetrieveProcessInstances"))
                {
                    return new RetrieveProcessInstancesResponse()
                    {
                        Results = new ParameterCollection
                {
                    new KeyValuePair<string, object>("Processes", new EntityCollection(new List<Entity> { incident })),
                },
                    };
                }

                if (processInstanceRequest.RequestName.Contains("RetrieveActivePath"))
                {
                    Entity activepath = new Entity("activepath");
                    activepath["stagename"] = "Guarantee Of Payment";
                    activepath["processstageid"] = Guid.NewGuid();
                    return new RetrieveActivePathResponse()
                    {
                        Results = new ParameterCollection
                {
                    new KeyValuePair<string, object>("ProcessStages", new EntityCollection(new List<Entity> { activepath })),
                },
                    };
                }

                return null;
            };

            CasePostOperationSync plugin = new CasePostOperationSync();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// PreValidate Update With Complete Data Test Method Create case type code 2 with caller role 3.
        /// </summary>
        [TestMethod]
        public void PostUpdateWithCompleteDataTestMethodCreateCallerRole3()
        {
            Entity incidentTarget = new Entity("incident");

            EntityReference users = new EntityReference("systemuser", Guid.NewGuid());
            users.Name = "MERCURIUS";

            incidentTarget.Id = Guid.Empty;

            incidentTarget["statuscode"] = new OptionSetValue(10);
            incidentTarget["incidentid"] = Guid.NewGuid();
            incidentTarget["caseorigincode"] = new OptionSetValue(2);
            incidentTarget["casetypecode"] = new OptionSetValue(2);
            incidentTarget["jarvis_mercuriusstatus"] = new OptionSetValue(300);
            incidentTarget["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
            incidentTarget["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["customerid"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["jarvis_registrationnumber"] = "123";
            incidentTarget["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incidentTarget["description"] = "This is for testing";
            incidentTarget["jarvis_location"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incidentTarget["jarvis_customerexpectations"] = "test";
            incidentTarget["jarvis_incidentnature"] = "[{\"id\":\"3efba61a-85ad-44b7-a187-7f54acaf7a67\"}]";
            incidentTarget["jarvis_callerrole"] = new OptionSetValue(3);
            incidentTarget["jarvis_callerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incidentTarget["isescalated"] = true;
            incidentTarget["createdon"] = DateTime.UtcNow;
            incidentTarget["jarvis_assistancetype"] = new OptionSetValue(334030000);

            Entity incident = new Entity("incident");
            incident.Id = Guid.Empty;
            incident["statuscode"] = new OptionSetValue(10);
            incident["incidentid"] = new EntityReference("incident", Guid.NewGuid());
            incident["caseorigincode"] = new OptionSetValue(2);
            incident["casetypecode"] = new OptionSetValue(2);
            incident["jarvis_mercuriusstatus"] = new OptionSetValue(300);
            incident["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
            incident["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incident["customerid"] = new EntityReference("account", Guid.NewGuid());
            incident["jarvis_registrationnumber"] = "123";
            incident["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["description"] = "This is for testing";
            incident["jarvis_driverlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incident["jarvis_translationstatusreportedfault"] = new OptionSetValue(334030000);
            incident["jarvis_translationstatuslocation"] = new OptionSetValue(334030000);
            incident["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["jarvis_translationstatuscustomerexpectations"] = new OptionSetValue(334030000);
            incident["jarvis_location"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["jarvis_customerexpectations"] = "test";
            incident["ownerid"] = new EntityReference { LogicalName = "system_user", Name = "test" };
            incident["jarvis_sourceid"] = 232323232;
            incident["jarvis_callerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incident["jarvis_callerrole"] = new OptionSetValue(3);
            incident["createdby"] = users;
            incident.Attributes["jarvis_etc"] = DateTime.Now;
            //// Setting Input Parameters.
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", incidentTarget),
               };
            this.PluginExecutionContext.MessageNameGet = () => "CREATE";
            this.PluginExecutionContext.StageGet = () => 40;
            //// Setting Post and Pre Image
            this.PluginExecutionContext.PostEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PostImage", incident },
                };
            };
            this.PluginExecutionContext.SharedVariablesGet = () =>
            {
                return new ParameterCollection();
            };

            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();
                if (query.GetType().Name == "FetchExpression")
                {
                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_vasbreakdownprocess'>"))
                    {
                        Entity vasprocess = new Entity("jarvis_vasbreakdownprocess");
                        vasprocess["businessprocessflowinstanceid"] = Guid.NewGuid();
                        vasprocess["bpf_name"] = "test";
                        vasprocess["bpf_incidentid"] = Guid.NewGuid();
                        vasprocess["activestageid"] = Guid.NewGuid();
                        vasprocess["statecode"] = new OptionSetValue(1);
                        vasprocess["createdon"] = DateTime.UtcNow;
                        vasprocess["createdby"] = "test";
                        vasprocess.Id = Guid.NewGuid();
                        result.Entities.Add(vasprocess);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='incident'>"))
                    {
                        Entity incidentResult = new Entity("incident");
                        incidentResult[Constants.IncidentNature.IncidentIncidentNature_jarvisIincidentnatureid] = new AliasedValue("incidentNature", "incidentnatureid", Guid.NewGuid());
                        incidentResult["jarvis_hdrd"] = false;
                        incidentResult["jarvis_homedealer"] = new EntityReference("account", incident.Id);
                        incidentResult["CaseRD.jarvis_repairingdealer"] = new AliasedValue("account", "account", new EntityReference("account", incident.Id));
                        incidentResult.Id = Guid.NewGuid();
                        result.Entities.Add(incidentResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='contact'>"))
                    {
                        Entity caseContact = new Entity("contact");
                        caseContact["jarvis_name"] = "testcontact";
                        caseContact["jarvis_firstname"] = "testcontact";
                        caseContact["jarvis_language"] = new EntityReference("jarvis_language", Guid.NewGuid());
                        caseContact["createdon"] = DateTime.UtcNow;
                        caseContact["telephone1"] = "9248428492";
                        caseContact["jarvis_role"] = new OptionSetValue(2);
                        caseContact["parentcustomerid"] = new EntityReference("account", incident.Id);
                        caseContact.Id = Guid.NewGuid();
                        caseContact.Attributes["preferredcontactmethodcode"] = null;
                        result.Entities.Add(caseContact);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_casecontact'>"))
                    {
                        Entity caseContact = new Entity("jarvis_casecontact");
                        caseContact["jarvis_name"] = "testcontact";
                        caseContact["jarvis_preferredlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
                        caseContact["createdon"] = DateTime.UtcNow;
                        caseContact["jarvis_mobilephone"] = "9248428492";
                        caseContact["jarvis_role"] = new OptionSetValue(2);
                        caseContact.Id = Guid.NewGuid();
                        result.Entities.Add(caseContact);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='workflow'>"))
                    {
                        Entity scheduleConsent = new Entity("workflow");
                        scheduleConsent["name"] = "BreakdownProcess";
                        scheduleConsent.Id = Guid.NewGuid();
                        result.Entities.Add(scheduleConsent);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_configurationjarvis'>"))
                    {
                        Entity configResult = new Entity("jarvis_configurationjarvis");
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationcasestatuschange"] = true;
                        configResult["createdon"] = DateTime.UtcNow;
                        configResult.Id = Guid.NewGuid();
                        result.Entities.Add(configResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_integrationconfiguration'>"))
                    {
                        Entity configResult = new Entity("jarvis_integrationconfiguration");
                        configResult["jarvis_integrationname"] = "test";
                        configResult["jarvis_integrationcode"] = "test";
                        configResult["createdon"] = DateTime.UtcNow;
                        configResult.Id = Guid.NewGuid();
                        result.Entities.Add(configResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_language'>"))
                    {
                        Entity language = new Entity("jarvis_language");
                        language.Id = Guid.NewGuid();
                        language["jarvis_iso3languagecode6392t"] = "DEU";
                        language["jarvis_iso2languagecode6391"] = "DE";
                        language["jarvis_name"] = "DE";
                        result.Entities.Add(language);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_softoffer'>"))
                    {
                        Entity softoffer = new Entity("jarvis_softoffer");
                        softoffer.Id = Guid.NewGuid();
                        softoffer["jarvis_name"] = "DEU";
                        softoffer["jarvis_softoffercode"] = new EntityReference("jarvis_softoffer", Guid.NewGuid());
                        softoffer["jarvis_contractno"] = "DE";
                        softoffer["jarvis_startdate"] = DateTime.UtcNow;
                        softoffer["jarvis_expirydate"] = DateTime.UtcNow;
                        softoffer["statuscode"] = new OptionSetValue(0);
                        softoffer["jarvis_marketcode"] = "DE";
                        softoffer["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
                        result.Entities.Add(softoffer);
                    }

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
                        passoutResult.Id = Guid.NewGuid();
                        result.Entities.Add(passoutResult);
                    }
                }

                return result;
            };

            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "incident")
                {
                    Entity inc = new Entity("incident");
                    inc.Id = guid;
                    inc["customerid"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_callerlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    inc["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    inc["jarvis_driverlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    inc["jarvis_translationstatusreportedfault"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatuslocation"] = new OptionSetValue(334030000);
                    inc["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    inc["jarvis_translationstatuscustomerexpectations"] = new OptionSetValue(334030000);
                    return inc;
                }

                if (entityName == "jarvis_vehicle")
                {
                    Entity client = new Entity("jarvis_vehicle");
                    client.Id = new Guid("6186f7f1-5f09-ed11-82e5-002248934e82");
                    client["jarvis_brandid"] = new EntityReference("jarvis_brand", Guid.NewGuid());
                    client["jarvis_fuelpowertype"] = new EntityReference("jarvis_vehiclefuelpowertype", Guid.NewGuid());
                    return client;
                }

                if (entityName == "jarvis_country")
                {
                    Entity country = new Entity("jarvis_country");
                    country.Id = guid;
                    country["jarvis_iso2countrycode"] = "NL";
                    country.Attributes[JarvisCountry.JarvisAverageetaduration] = "12";
                    return country;
                }

                if (entityName == "jarvis_language")
                {
                    Entity language = new Entity("jarvis_language");
                    language.Id = guid;
                    language["jarvis_iso3languagecode6392t"] = "DEU";
                    language["jarvis_iso2languagecode6391"] = "DE";
                    return language;
                }

                if (entityName == "account")
                {
                    Entity account = new Entity("account");
                    account.Id = guid;
                    account["jarvis_language"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    account["jarvis_address1_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    return account;
                }

                return null;
            };
            this.Service.ExecuteOrganizationRequest = processInstanceRequest =>
            {
                if (processInstanceRequest.RequestName.Contains("RetrieveProcessInstances"))
                {
                    return new RetrieveProcessInstancesResponse()
                    {
                        Results = new ParameterCollection
                {
                    new KeyValuePair<string, object>("Processes", new EntityCollection(new List<Entity> { incident })),
                },
                    };
                }

                if (processInstanceRequest.RequestName.Contains("RetrieveActivePath"))
                {
                    Entity activepath = new Entity("activepath");
                    activepath["stagename"] = "Pass Out";
                    activepath["processstageid"] = Guid.NewGuid();
                    return new RetrieveActivePathResponse()
                    {
                        Results = new ParameterCollection
                {
                    new KeyValuePair<string, object>("ProcessStages", new EntityCollection(new List<Entity> { activepath })),
                },
                    };
                }

                return null;
            };

            CasePostOperationSync plugin = new CasePostOperationSync();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// PreValidate Update With Complete Data Test Method Create case type code 3.
        /// </summary>
        [TestMethod]
        public void PostUpdateWithCompleteDataTestMethodCreateCaseType3()
        {
            Entity incidentTarget = new Entity("incident");

            EntityReference users = new EntityReference("systemuser", Guid.NewGuid());
            users.Name = "MERCURIUS";

            incidentTarget.Id = Guid.Empty;

            incidentTarget["statuscode"] = new OptionSetValue(10);
            incidentTarget["incidentid"] = Guid.NewGuid();
            incidentTarget["caseorigincode"] = new OptionSetValue(2);
            incidentTarget["casetypecode"] = new OptionSetValue(3);
            incidentTarget["jarvis_mercuriusstatus"] = new OptionSetValue(100);
            incidentTarget["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
            incidentTarget["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["customerid"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["jarvis_registrationnumber"] = "123";
            incidentTarget["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incidentTarget["description"] = "This is for testing";
            incidentTarget["jarvis_location"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incidentTarget["jarvis_customerexpectations"] = "test";
            incidentTarget["jarvis_incidentnature"] = "[{\"id\":\"3efba61a-85ad-44b7-a187-7f54acaf7a67\"}]";
            incidentTarget["jarvis_callerrole"] = new OptionSetValue(3);
            incidentTarget["jarvis_callerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incidentTarget["isescalated"] = true;
            incidentTarget["createdon"] = DateTime.UtcNow;
            incidentTarget["jarvis_assistancetype"] = new OptionSetValue(334030000);
            incidentTarget["jarvis_querydecision"] = "test";
            incidentTarget["jarvis_querydescription"] = "test";
            incidentTarget["jarvis_extrainformationonsolutionquery"] = "test";

            Entity incident = new Entity("incident");
            incident.Id = Guid.Empty;
            incident["statuscode"] = new OptionSetValue(10);
            incident["incidentid"] = new EntityReference("incident", Guid.NewGuid());
            incident["caseorigincode"] = new OptionSetValue(2);
            incident["casetypecode"] = new OptionSetValue(3);
            incident["jarvis_mercuriusstatus"] = new OptionSetValue(100);
            incident["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
            incident["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incident["customerid"] = new EntityReference("account", Guid.NewGuid());
            incident["jarvis_registrationnumber"] = "123";
            incident["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["description"] = "This is for testing";
            incident["jarvis_driverlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incident["jarvis_translationstatusreportedfault"] = new OptionSetValue(334030000);
            incident["jarvis_translationstatuslocation"] = new OptionSetValue(334030000);
            incident["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["jarvis_translationstatuscustomerexpectations"] = new OptionSetValue(334030000);
            incident["jarvis_location"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["jarvis_customerexpectations"] = "test";
            incident["ownerid"] = new EntityReference { LogicalName = "system_user", Name = "test" };
            incident["jarvis_sourceid"] = 232323232;
            incident["jarvis_callerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incident["jarvis_callerrole"] = new OptionSetValue(3);
            incident["createdby"] = users;
            incident.Attributes["jarvis_etc"] = DateTime.Now;
            incident["parentcaseid"] = new EntityReference("incident", Guid.NewGuid());

            //// Setting Input Parameters.
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", incidentTarget),
               };
            this.PluginExecutionContext.MessageNameGet = () => "CREATE";
            this.PluginExecutionContext.StageGet = () => 40;
            //// Setting Post and Pre Image
            this.PluginExecutionContext.PostEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PostImage", incident },
                };
            };
            this.PluginExecutionContext.PreEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PreImage", incident },
                };
            };
            this.PluginExecutionContext.SharedVariablesGet = () =>
            {
                return new ParameterCollection();
            };

            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();
                if (query.GetType().Name == "FetchExpression")
                {
                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_vasbreakdownprocess'>"))
                    {
                        Entity vasprocess = new Entity("jarvis_vasbreakdownprocess");
                        vasprocess["businessprocessflowinstanceid"] = Guid.NewGuid();
                        vasprocess["bpf_name"] = "test";
                        vasprocess["bpf_incidentid"] = Guid.NewGuid();
                        vasprocess["activestageid"] = Guid.NewGuid();
                        vasprocess["statecode"] = new OptionSetValue(0);
                        vasprocess["createdon"] = DateTime.UtcNow;
                        vasprocess["createdby"] = "test";
                        vasprocess.Id = Guid.NewGuid();
                        result.Entities.Add(vasprocess);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='incident'>"))
                    {
                        Entity incidentResult = new Entity("incident");
                        incidentResult[Constants.IncidentNature.IncidentIncidentNature_jarvisIincidentnatureid] = new AliasedValue("incidentNature", "incidentnatureid", Guid.NewGuid());
                        incidentResult["jarvis_hdrd"] = false;
                        incidentResult["jarvis_homedealer"] = new EntityReference("account", incident.Id);
                        incidentResult["CaseRD.jarvis_repairingdealer"] = new AliasedValue("account", "account", new EntityReference("account", incident.Id));
                        incidentResult.Id = Guid.NewGuid();
                        result.Entities.Add(incidentResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_casecontact'>"))
                    {
                        Entity caseContact = new Entity("jarvis_casecontact");
                        caseContact["jarvis_name"] = "testcontact";
                        caseContact["jarvis_preferredlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
                        caseContact["createdon"] = DateTime.UtcNow;
                        caseContact["jarvis_mobilephone"] = "9248428492";
                        caseContact["jarvis_role"] = new OptionSetValue(2);
                        caseContact.Id = Guid.NewGuid();
                        result.Entities.Add(caseContact);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='workflow'>"))
                    {
                        Entity scheduleConsent = new Entity("workflow");
                        scheduleConsent["name"] = "BreakdownProcess";
                        scheduleConsent.Id = Guid.NewGuid();
                        result.Entities.Add(scheduleConsent);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_configurationjarvis'>"))
                    {
                        Entity configResult = new Entity("jarvis_configurationjarvis");
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationcasestatuschange"] = true;
                        configResult["createdon"] = DateTime.UtcNow;
                        configResult.Id = Guid.NewGuid();
                        result.Entities.Add(configResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_integrationconfiguration'>"))
                    {
                        Entity configResult = new Entity("jarvis_integrationconfiguration");
                        configResult["jarvis_integrationname"] = "test";
                        configResult["jarvis_integrationcode"] = "test";
                        configResult["createdon"] = DateTime.UtcNow;
                        configResult.Id = Guid.NewGuid();
                        result.Entities.Add(configResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_language'>"))
                    {
                        Entity language = new Entity("jarvis_language");
                        language.Id = Guid.NewGuid();
                        language["jarvis_iso3languagecode6392t"] = "DEU";
                        language["jarvis_iso2languagecode6391"] = "DE";
                        language["jarvis_name"] = "DE";
                        result.Entities.Add(language);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_casesoftoffer'>"))
                    {
                        Entity casesoftoffer = new Entity("jarvis_casesoftoffer");
                        casesoftoffer.Id = Guid.NewGuid();
                        casesoftoffer["jarvis_name"] = "DEU";
                        casesoftoffer["jarvis_contractno"] = "DE";
                        casesoftoffer["jarvis_description"] = "DE";
                        casesoftoffer["jarvis_startdate"] = DateTime.UtcNow;
                        casesoftoffer["jarvis_expirydate"] = DateTime.UtcNow;
                        casesoftoffer["statuscode"] = new OptionSetValue(0);
                        casesoftoffer["jarvis_softoffercodelookup"] = new EntityReference("jarvis_softoffer", Guid.NewGuid());
                        result.Entities.Add(casesoftoffer);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_softoffer'>"))
                    {
                        Entity softoffer = new Entity("jarvis_softoffer");
                        softoffer.Id = Guid.NewGuid();
                        softoffer["jarvis_name"] = "DEU";
                        softoffer["jarvis_softoffercode"] = new EntityReference("jarvis_softoffer", Guid.NewGuid());
                        softoffer["jarvis_contractno"] = "DE";
                        softoffer["jarvis_startdate"] = DateTime.UtcNow;
                        softoffer["jarvis_expirydate"] = DateTime.UtcNow;
                        softoffer["statuscode"] = new OptionSetValue(0);
                        softoffer["jarvis_marketcode"] = "DE";
                        softoffer["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
                        result.Entities.Add(softoffer);
                    }

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
                        passoutResult.Id = Guid.NewGuid();
                        result.Entities.Add(passoutResult);
                    }
                }

                return result;
            };

            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "incident")
                {
                    Entity inc = new Entity("incident");
                    inc.Id = guid;
                    inc["customerid"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_callerlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    inc["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    inc["jarvis_driverlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    inc["jarvis_translationstatusreportedfault"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatuslocation"] = new OptionSetValue(334030000);
                    inc["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    inc["jarvis_translationstatuscustomerexpectations"] = new OptionSetValue(334030000);
                    return inc;
                }

                if (entityName == "jarvis_vehicle")
                {
                    Entity client = new Entity("jarvis_vehicle");
                    client.Id = new Guid("6186f7f1-5f09-ed11-82e5-002248934e82");
                    client["jarvis_brandid"] = new EntityReference("jarvis_brand", Guid.NewGuid());
                    client["jarvis_fuelpowertype"] = new EntityReference("jarvis_vehiclefuelpowertype", Guid.NewGuid());
                    return client;
                }

                if (entityName == "jarvis_country")
                {
                    Entity country = new Entity("jarvis_country");
                    country.Id = guid;
                    country["jarvis_iso2countrycode"] = "NL";
                    country.Attributes[JarvisCountry.JarvisAverageetaduration] = "12";
                    return country;
                }

                if (entityName == "jarvis_language")
                {
                    Entity language = new Entity("jarvis_language");
                    language.Id = guid;
                    language["jarvis_iso3languagecode6392t"] = "DEU";
                    language["jarvis_iso2languagecode6391"] = "DE";
                    return language;
                }

                if (entityName == "account")
                {
                    Entity account = new Entity("account");
                    account.Id = guid;
                    account["jarvis_language"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    account["jarvis_address1_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    return account;
                }

                if (entityName == "jarvis_brand")
                {
                    Entity client = new Entity("jarvis_brand");
                    client.Id = new Guid("6186f7f1-5f09-ed11-82e5-002248934e82");
                    client["jarvis_mercuriusbrandcode"] = "ted";
                    return client;
                }

                return null;
            };
            this.Service.ExecuteOrganizationRequest = processInstanceRequest =>
            {
                if (processInstanceRequest.RequestName.Contains("RetrieveProcessInstances"))
                {
                    return new RetrieveProcessInstancesResponse()
                    {
                        Results = new ParameterCollection
                {
                    new KeyValuePair<string, object>("Processes", new EntityCollection(new List<Entity> { incident })),
                },
                    };
                }

                if (processInstanceRequest.RequestName.Contains("RetrieveActivePath"))
                {
                    Entity activepath = new Entity("activepath");
                    activepath["stagename"] = "Case Opening";
                    activepath["processstageid"] = Guid.NewGuid();
                    return new RetrieveActivePathResponse()
                    {
                        Results = new ParameterCollection
                {
                    new KeyValuePair<string, object>("ProcessStages", new EntityCollection(new List<Entity> { activepath })),
                },
                    };
                }

                return null;
            };

            CasePostOperationSync plugin = new CasePostOperationSync();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// PreValidate Update With Complete Data Test Method Create case type code 2.
        /// </summary>
        [TestMethod]
        public void PostUpdateWithCompleteDataTestMethodCreateMerStatus400()
        {
            Entity incidentTarget = new Entity("incident");

            EntityReference users = new EntityReference("systemuser", Guid.NewGuid());
            users.Name = "MERCURIUS";

            incidentTarget.Id = Guid.Empty;

            incidentTarget["statuscode"] = new OptionSetValue(10);
            incidentTarget["incidentid"] = Guid.NewGuid();
            incidentTarget["caseorigincode"] = new OptionSetValue(2);
            incidentTarget["casetypecode"] = new OptionSetValue(2);
            incidentTarget["jarvis_mercuriusstatus"] = new OptionSetValue(400);
            incidentTarget["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
            incidentTarget["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["customerid"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["jarvis_registrationnumber"] = "123";
            incidentTarget["description"] = "This is for testing";
            incidentTarget["jarvis_location"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incidentTarget["jarvis_customerexpectations"] = "test";
            incidentTarget["jarvis_callerrole"] = new OptionSetValue(4);
            incidentTarget["jarvis_callerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incidentTarget["isescalated"] = true;
            incidentTarget["createdon"] = DateTime.UtcNow;
            incidentTarget["jarvis_assistancetype"] = new OptionSetValue(334030000);

            Entity incident = new Entity("incident");
            incident.Id = Guid.Empty;
            incident["statuscode"] = new OptionSetValue(10);
            incident["incidentid"] = new EntityReference("incident", Guid.NewGuid());
            incident["caseorigincode"] = new OptionSetValue(2);
            incident["casetypecode"] = new OptionSetValue(2);
            incident["jarvis_mercuriusstatus"] = new OptionSetValue(400);
            incident["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incident["customerid"] = new EntityReference("account", Guid.NewGuid());
            incident["jarvis_registrationnumber"] = "123";
            incident["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["description"] = "This is for testing";
            incident["jarvis_driverlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incident["jarvis_translationstatusreportedfault"] = new OptionSetValue(334030000);
            incident["jarvis_translationstatuslocation"] = new OptionSetValue(334030000);
            incident["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["jarvis_translationstatuscustomerexpectations"] = new OptionSetValue(334030000);
            incident["jarvis_location"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["jarvis_customerexpectations"] = "test";
            incident["ownerid"] = new EntityReference { LogicalName = "system_user", Name = "test" };
            incident["jarvis_callerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incident["jarvis_callerrole"] = new OptionSetValue(4);
            incident["createdby"] = users;
            incident.Attributes["jarvis_etc"] = DateTime.Now;
            //// Setting Input Parameters.
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", incidentTarget),
               };
            this.PluginExecutionContext.MessageNameGet = () => "CREATE";
            this.PluginExecutionContext.StageGet = () => 40;
            //// Setting Post and Pre Image
            this.PluginExecutionContext.PostEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PostImage", incident },
                };
            };
            this.PluginExecutionContext.SharedVariablesGet = () =>
            {
                return new ParameterCollection();
            };

            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();
                if (query.GetType().Name == "FetchExpression")
                {
                    if ((query as FetchExpression).Query.Contains("<entity name='incident'>"))
                    {
                        Entity incidentResult = new Entity("incident");
                        incidentResult[Constants.IncidentNature.IncidentIncidentNature_jarvisIincidentnatureid] = new AliasedValue("incidentNature", "incidentnatureid", Guid.NewGuid());
                        incidentResult["jarvis_hdrd"] = false;
                        incidentResult["jarvis_homedealer"] = new EntityReference("account", incident.Id);
                        incidentResult["CaseRD.jarvis_repairingdealer"] = new AliasedValue("account", "account", new EntityReference("account", incident.Id));
                        incidentResult.Id = Guid.NewGuid();
                        result.Entities.Add(incidentResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_casecontact'>"))
                    {
                        Entity caseContact = new Entity("jarvis_casecontact");
                        caseContact["jarvis_name"] = "testcontact";
                        caseContact["jarvis_preferredlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
                        caseContact["createdon"] = DateTime.UtcNow;
                        caseContact["jarvis_mobilephone"] = "9248428492";
                        caseContact["jarvis_role"] = new OptionSetValue(2);
                        caseContact.Id = Guid.NewGuid();
                        result.Entities.Add(caseContact);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='workflow'>"))
                    {
                        Entity scheduleConsent = new Entity("workflow");
                        scheduleConsent["name"] = "BreakdownProcess";
                        scheduleConsent.Id = Guid.NewGuid();
                        result.Entities.Add(scheduleConsent);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_configurationjarvis'>"))
                    {
                        Entity configResult = new Entity("jarvis_configurationjarvis");
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationcasestatuschange"] = true;
                        configResult["createdon"] = DateTime.UtcNow;
                        configResult.Id = Guid.NewGuid();
                        result.Entities.Add(configResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_integrationconfiguration'>"))
                    {
                        Entity configResult = new Entity("jarvis_integrationconfiguration");
                        configResult["jarvis_integrationname"] = "test";
                        configResult["jarvis_integrationcode"] = "test";
                        configResult["createdon"] = DateTime.UtcNow;
                        configResult.Id = Guid.NewGuid();
                        result.Entities.Add(configResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_language'>"))
                    {
                        Entity language = new Entity("jarvis_language");
                        language.Id = Guid.NewGuid();
                        language["jarvis_iso3languagecode6392t"] = "DEU";
                        language["jarvis_iso2languagecode6391"] = "DE";
                        language["jarvis_name"] = "DE";
                        result.Entities.Add(language);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_casesoftoffer'>"))
                    {
                        Entity casesoftoffer = new Entity("jarvis_casesoftoffer");
                        casesoftoffer.Id = Guid.NewGuid();
                        casesoftoffer["jarvis_name"] = "DEU";
                        casesoftoffer["jarvis_contractno"] = "DE";
                        casesoftoffer["jarvis_description"] = "DE";
                        casesoftoffer["jarvis_startdate"] = DateTime.UtcNow;
                        casesoftoffer["jarvis_expirydate"] = DateTime.UtcNow;
                        casesoftoffer["statuscode"] = new OptionSetValue(0);
                        casesoftoffer["jarvis_softoffercodelookup"] = new EntityReference("jarvis_softoffer", Guid.NewGuid());
                        result.Entities.Add(casesoftoffer);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_softoffer'>"))
                    {
                        Entity softoffer = new Entity("jarvis_softoffer");
                        softoffer.Id = Guid.NewGuid();
                        softoffer["jarvis_name"] = "DEU";
                        softoffer["jarvis_softoffercode"] = new EntityReference("jarvis_softoffer", Guid.NewGuid());
                        softoffer["jarvis_contractno"] = "DE";
                        softoffer["jarvis_startdate"] = DateTime.UtcNow;
                        softoffer["jarvis_expirydate"] = DateTime.UtcNow;
                        softoffer["statuscode"] = new OptionSetValue(0);
                        softoffer["jarvis_marketcode"] = "DE";
                        softoffer["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
                        result.Entities.Add(softoffer);
                    }

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
                        passoutResult.Id = Guid.NewGuid();
                        result.Entities.Add(passoutResult);
                    }
                }

                return result;
            };

            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "incident")
                {
                    Entity inc = new Entity("incident");
                    inc.Id = guid;
                    inc["customerid"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_callerlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    inc["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    inc["jarvis_driverlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    inc["jarvis_translationstatusreportedfault"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatuslocation"] = new OptionSetValue(334030000);
                    inc["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    inc["jarvis_translationstatuscustomerexpectations"] = new OptionSetValue(334030000);
                    return inc;
                }

                if (entityName == "jarvis_vehicle")
                {
                    Entity client = new Entity("jarvis_vehicle");
                    client.Id = new Guid("6186f7f1-5f09-ed11-82e5-002248934e82");
                    client["jarvis_brandid"] = new EntityReference("jarvis_brand", Guid.NewGuid());
                    client["jarvis_fuelpowertype"] = new EntityReference("jarvis_vehiclefuelpowertype", Guid.NewGuid());
                    return client;
                }

                if (entityName == "jarvis_country")
                {
                    Entity country = new Entity("jarvis_country");
                    country.Id = guid;
                    country["jarvis_iso2countrycode"] = "NL";
                    country.Attributes[JarvisCountry.JarvisAverageetaduration] = "12";
                    return country;
                }

                if (entityName == "jarvis_language")
                {
                    Entity language = new Entity("jarvis_language");
                    language.Id = guid;
                    language["jarvis_iso3languagecode6392t"] = "DEU";
                    language["jarvis_iso2languagecode6391"] = "DE";
                    return language;
                }

                if (entityName == "account")
                {
                    Entity account = new Entity("account");
                    account.Id = guid;
                    account["jarvis_language"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    account["jarvis_address1_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    return account;
                }

                return null;
            };
            this.Service.ExecuteOrganizationRequest = processInstanceRequest =>
            {
                if (processInstanceRequest.RequestName.Contains("RetrieveProcessInstances"))
                {
                    return new RetrieveProcessInstancesResponse()
                    {
                        Results = new ParameterCollection
                {
                    new KeyValuePair<string, object>("Processes", new EntityCollection(new List<Entity> { incident })),
                },
                    };
                }

                if (processInstanceRequest.RequestName.Contains("RetrieveActivePath"))
                {
                    Entity activepath = new Entity("activepath");
                    activepath["stagename"] = "Guarantee Of Payment";
                    activepath["processstageid"] = Guid.NewGuid();
                    return new RetrieveActivePathResponse()
                    {
                        Results = new ParameterCollection
                {
                    new KeyValuePair<string, object>("ProcessStages", new EntityCollection(new List<Entity> { activepath })),
                },
                    };
                }

                return null;
            };

            CasePostOperationSync plugin = new CasePostOperationSync();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// PreValidate Update With Complete Data Test Method Create with Mercurius status 500.
        /// </summary>
        [TestMethod]
        public void PostUpdateWithCompleteDataTestMethodCreateMerStatus500()
        {
            Entity incidentTarget = new Entity("incident");

            EntityReference users = new EntityReference("systemuser", Guid.NewGuid());
            users.Name = "MERCURIUS";

            incidentTarget.Id = Guid.Empty;

            incidentTarget["statuscode"] = new OptionSetValue(10);
            incidentTarget["incidentid"] = Guid.NewGuid();
            incidentTarget["caseorigincode"] = new OptionSetValue(2);
            incidentTarget["casetypecode"] = new OptionSetValue(2);
            incidentTarget["jarvis_mercuriusstatus"] = new OptionSetValue(500);
            incidentTarget["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
            incidentTarget["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["customerid"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["jarvis_registrationnumber"] = "123";
            incidentTarget["description"] = "This is for testing";
            incidentTarget["jarvis_location"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incidentTarget["jarvis_customerexpectations"] = "test";
            incidentTarget["jarvis_callerrole"] = new OptionSetValue(4);
            incidentTarget["jarvis_callerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incidentTarget["isescalated"] = true;
            incidentTarget["createdon"] = DateTime.UtcNow;
            incidentTarget["jarvis_assistancetype"] = new OptionSetValue(334030000);

            Entity incident = new Entity("incident");
            incident.Id = Guid.Empty;
            incident["statuscode"] = new OptionSetValue(10);
            incident["incidentid"] = new EntityReference("incident", Guid.NewGuid());
            incident["caseorigincode"] = new OptionSetValue(2);
            incident["casetypecode"] = new OptionSetValue(2);
            incident["jarvis_mercuriusstatus"] = new OptionSetValue(500);
            incident["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incident["customerid"] = new EntityReference("account", Guid.NewGuid());
            incident["jarvis_registrationnumber"] = "123";
            incident["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["description"] = "This is for testing";
            incident["jarvis_driverlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incident["jarvis_translationstatusreportedfault"] = new OptionSetValue(334030000);
            incident["jarvis_translationstatuslocation"] = new OptionSetValue(334030000);
            incident["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["jarvis_translationstatuscustomerexpectations"] = new OptionSetValue(334030000);
            incident["jarvis_location"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["jarvis_customerexpectations"] = "test";
            incident["ownerid"] = new EntityReference { LogicalName = "system_user", Name = "test" };
            incident["jarvis_callerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incident["jarvis_callerrole"] = new OptionSetValue(4);
            incident["createdby"] = users;
            incident.Attributes["jarvis_etc"] = DateTime.Now;
            //// Setting Input Parameters.
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", incidentTarget),
               };
            this.PluginExecutionContext.MessageNameGet = () => "CREATE";
            this.PluginExecutionContext.StageGet = () => 40;
            //// Setting Post and Pre Image
            this.PluginExecutionContext.PostEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PostImage", incident },
                };
            };
            this.PluginExecutionContext.SharedVariablesGet = () =>
            {
                return new ParameterCollection();
            };

            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();
                if (query.GetType().Name == "FetchExpression")
                {
                    if ((query as FetchExpression).Query.Contains("<entity name='incident'>"))
                    {
                        Entity incidentResult = new Entity("incident");
                        incidentResult[Constants.IncidentNature.IncidentIncidentNature_jarvisIincidentnatureid] = new AliasedValue("incidentNature", "incidentnatureid", Guid.NewGuid());
                        incidentResult["jarvis_hdrd"] = false;
                        incidentResult["jarvis_homedealer"] = new EntityReference("account", incident.Id);
                        incidentResult["CaseRD.jarvis_repairingdealer"] = new AliasedValue("account", "account", new EntityReference("account", incident.Id));
                        incidentResult.Id = Guid.NewGuid();
                        result.Entities.Add(incidentResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_casecontact'>"))
                    {
                        Entity caseContact = new Entity("jarvis_casecontact");
                        caseContact["jarvis_name"] = "testcontact";
                        caseContact["jarvis_preferredlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
                        caseContact["createdon"] = DateTime.UtcNow;
                        caseContact["jarvis_mobilephone"] = "9248428492";
                        caseContact["jarvis_role"] = new OptionSetValue(2);
                        caseContact.Id = Guid.NewGuid();
                        result.Entities.Add(caseContact);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='workflow'>"))
                    {
                        Entity scheduleConsent = new Entity("workflow");
                        scheduleConsent["name"] = "BreakdownProcess";
                        scheduleConsent.Id = Guid.NewGuid();
                        result.Entities.Add(scheduleConsent);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_configurationjarvis'>"))
                    {
                        Entity configResult = new Entity("jarvis_configurationjarvis");
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationcasestatuschange"] = true;
                        configResult["createdon"] = DateTime.UtcNow;
                        configResult.Id = Guid.NewGuid();
                        result.Entities.Add(configResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_integrationconfiguration'>"))
                    {
                        Entity configResult = new Entity("jarvis_integrationconfiguration");
                        configResult["jarvis_integrationname"] = "test";
                        configResult["jarvis_integrationcode"] = "test";
                        configResult["createdon"] = DateTime.UtcNow;
                        configResult.Id = Guid.NewGuid();
                        result.Entities.Add(configResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_language'>"))
                    {
                        Entity language = new Entity("jarvis_language");
                        language.Id = Guid.NewGuid();
                        language["jarvis_iso3languagecode6392t"] = "DEU";
                        language["jarvis_iso2languagecode6391"] = "DE";
                        language["jarvis_name"] = "DE";
                        result.Entities.Add(language);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_casesoftoffer'>"))
                    {
                        Entity casesoftoffer = new Entity("jarvis_casesoftoffer");
                        casesoftoffer.Id = Guid.NewGuid();
                        casesoftoffer["jarvis_name"] = "DEU";
                        casesoftoffer["jarvis_contractno"] = "DE";
                        casesoftoffer["jarvis_description"] = "DE";
                        casesoftoffer["jarvis_startdate"] = DateTime.UtcNow;
                        casesoftoffer["jarvis_expirydate"] = DateTime.UtcNow;
                        casesoftoffer["statuscode"] = new OptionSetValue(0);
                        casesoftoffer["jarvis_softoffercodelookup"] = new EntityReference("jarvis_softoffer", Guid.NewGuid());
                        result.Entities.Add(casesoftoffer);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_softoffer'>"))
                    {
                        Entity softoffer = new Entity("jarvis_softoffer");
                        softoffer.Id = Guid.NewGuid();
                        softoffer["jarvis_name"] = "DEU";
                        softoffer["jarvis_softoffercode"] = new EntityReference("jarvis_softoffer", Guid.NewGuid());
                        softoffer["jarvis_contractno"] = "DE";
                        softoffer["jarvis_startdate"] = DateTime.UtcNow;
                        softoffer["jarvis_expirydate"] = DateTime.UtcNow;
                        softoffer["statuscode"] = new OptionSetValue(0);
                        softoffer["jarvis_marketcode"] = "DE";
                        softoffer["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
                        result.Entities.Add(softoffer);
                    }

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
                        passoutResult.Id = Guid.NewGuid();
                        result.Entities.Add(passoutResult);
                    }
                }

                return result;
            };

            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "incident")
                {
                    Entity inc = new Entity("incident");
                    inc.Id = guid;
                    inc["customerid"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_callerlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    inc["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    inc["jarvis_driverlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    inc["jarvis_translationstatusreportedfault"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatuslocation"] = new OptionSetValue(334030000);
                    inc["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    inc["jarvis_translationstatuscustomerexpectations"] = new OptionSetValue(334030000);
                    return inc;
                }

                if (entityName == "jarvis_vehicle")
                {
                    Entity client = new Entity("jarvis_vehicle");
                    client.Id = new Guid("6186f7f1-5f09-ed11-82e5-002248934e82");
                    client["jarvis_brandid"] = new EntityReference("jarvis_brand", Guid.NewGuid());
                    client["jarvis_fuelpowertype"] = new EntityReference("jarvis_vehiclefuelpowertype", Guid.NewGuid());
                    return client;
                }

                if (entityName == "jarvis_country")
                {
                    Entity country = new Entity("jarvis_country");
                    country.Id = guid;
                    country["jarvis_iso2countrycode"] = "NL";
                    country.Attributes[JarvisCountry.JarvisAverageetaduration] = "12";
                    return country;
                }

                if (entityName == "jarvis_language")
                {
                    Entity language = new Entity("jarvis_language");
                    language.Id = guid;
                    language["jarvis_iso3languagecode6392t"] = "DEU";
                    language["jarvis_iso2languagecode6391"] = "DE";
                    return language;
                }

                if (entityName == "account")
                {
                    Entity account = new Entity("account");
                    account.Id = guid;
                    account["jarvis_language"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    account["jarvis_address1_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    return account;
                }

                return null;
            };
            this.Service.ExecuteOrganizationRequest = processInstanceRequest =>
            {
                if (processInstanceRequest.RequestName.Contains("RetrieveProcessInstances"))
                {
                    return new RetrieveProcessInstancesResponse()
                    {
                        Results = new ParameterCollection
                {
                    new KeyValuePair<string, object>("Processes", new EntityCollection(new List<Entity> { incident })),
                },
                    };
                }

                if (processInstanceRequest.RequestName.Contains("RetrieveActivePath"))
                {
                    Entity activepath = new Entity("activepath");
                    activepath["stagename"] = "Guarantee Of Payment";
                    activepath["processstageid"] = Guid.NewGuid();
                    return new RetrieveActivePathResponse()
                    {
                        Results = new ParameterCollection
                {
                    new KeyValuePair<string, object>("ProcessStages", new EntityCollection(new List<Entity> { activepath })),
                },
                    };
                }

                return null;
            };

            CasePostOperationSync plugin = new CasePostOperationSync();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// PreValidate Update With Complete Data Test Method Create with Mercurius status 600.
        /// </summary>
        [TestMethod]
        public void PostUpdateWithCompleteDataTestMethodCreateMerStatus600()
        {
            Entity incidentTarget = new Entity("incident");

            EntityReference users = new EntityReference("systemuser", Guid.NewGuid());
            users.Name = "MERCURIUS";

            incidentTarget.Id = Guid.Empty;

            incidentTarget["statuscode"] = new OptionSetValue(10);
            incidentTarget["incidentid"] = Guid.NewGuid();
            incidentTarget["caseorigincode"] = new OptionSetValue(2);
            incidentTarget["casetypecode"] = new OptionSetValue(2);
            incidentTarget["jarvis_mercuriusstatus"] = new OptionSetValue(600);
            incidentTarget["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
            incidentTarget["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["customerid"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["jarvis_registrationnumber"] = "123";
            incidentTarget["description"] = "This is for testing";
            incidentTarget["jarvis_location"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incidentTarget["jarvis_customerexpectations"] = "test";
            incidentTarget["jarvis_callerrole"] = new OptionSetValue(4);
            incidentTarget["jarvis_callerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incidentTarget["isescalated"] = true;
            incidentTarget["createdon"] = DateTime.UtcNow;
            incidentTarget["jarvis_assistancetype"] = new OptionSetValue(334030000);

            Entity incident = new Entity("incident");
            incident.Id = Guid.Empty;
            incident["statuscode"] = new OptionSetValue(10);
            incident["incidentid"] = new EntityReference("incident", Guid.NewGuid());
            incident["caseorigincode"] = new OptionSetValue(2);
            incident["casetypecode"] = new OptionSetValue(2);
            incident["jarvis_mercuriusstatus"] = new OptionSetValue(600);
            incident["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incident["customerid"] = new EntityReference("account", Guid.NewGuid());
            incident["jarvis_registrationnumber"] = "123";
            incident["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["description"] = "This is for testing";
            incident["jarvis_driverlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incident["jarvis_translationstatusreportedfault"] = new OptionSetValue(334030000);
            incident["jarvis_translationstatuslocation"] = new OptionSetValue(334030000);
            incident["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["jarvis_translationstatuscustomerexpectations"] = new OptionSetValue(334030000);
            incident["jarvis_location"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["jarvis_customerexpectations"] = "test";
            incident["ownerid"] = new EntityReference { LogicalName = "system_user", Name = "test" };
            incident["jarvis_callerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incident["jarvis_callerrole"] = new OptionSetValue(4);
            incident["createdby"] = users;
            incident.Attributes["jarvis_etc"] = DateTime.Now;
            //// Setting Input Parameters.
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", incidentTarget),
               };
            this.PluginExecutionContext.MessageNameGet = () => "CREATE";
            this.PluginExecutionContext.StageGet = () => 40;
            //// Setting Post and Pre Image
            this.PluginExecutionContext.PostEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PostImage", incident },
                };
            };
            this.PluginExecutionContext.SharedVariablesGet = () =>
            {
                return new ParameterCollection();
            };

            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();
                if (query.GetType().Name == "FetchExpression")
                {
                    if ((query as FetchExpression).Query.Contains("<entity name='incident'>"))
                    {
                        Entity incidentResult = new Entity("incident");
                        incidentResult[Constants.IncidentNature.IncidentIncidentNature_jarvisIincidentnatureid] = new AliasedValue("incidentNature", "incidentnatureid", Guid.NewGuid());
                        incidentResult["jarvis_hdrd"] = false;
                        incidentResult["jarvis_homedealer"] = new EntityReference("account", incident.Id);
                        incidentResult["CaseRD.jarvis_repairingdealer"] = new AliasedValue("account", "account", new EntityReference("account", incident.Id));
                        incidentResult.Id = Guid.NewGuid();
                        result.Entities.Add(incidentResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_casecontact'>"))
                    {
                        Entity caseContact = new Entity("jarvis_casecontact");
                        caseContact["jarvis_name"] = "testcontact";
                        caseContact["jarvis_preferredlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
                        caseContact["createdon"] = DateTime.UtcNow;
                        caseContact["jarvis_mobilephone"] = "9248428492";
                        caseContact["jarvis_role"] = new OptionSetValue(2);
                        caseContact.Id = Guid.NewGuid();
                        result.Entities.Add(caseContact);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='workflow'>"))
                    {
                        Entity scheduleConsent = new Entity("workflow");
                        scheduleConsent["name"] = "BreakdownProcess";
                        scheduleConsent.Id = Guid.NewGuid();
                        result.Entities.Add(scheduleConsent);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_configurationjarvis'>"))
                    {
                        Entity configResult = new Entity("jarvis_configurationjarvis");
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationcasestatuschange"] = true;
                        configResult["createdon"] = DateTime.UtcNow;
                        configResult.Id = Guid.NewGuid();
                        result.Entities.Add(configResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_integrationconfiguration'>"))
                    {
                        Entity configResult = new Entity("jarvis_integrationconfiguration");
                        configResult["jarvis_integrationname"] = "test";
                        configResult["jarvis_integrationcode"] = "test";
                        configResult["createdon"] = DateTime.UtcNow;
                        configResult.Id = Guid.NewGuid();
                        result.Entities.Add(configResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_language'>"))
                    {
                        Entity language = new Entity("jarvis_language");
                        language.Id = Guid.NewGuid();
                        language["jarvis_iso3languagecode6392t"] = "DEU";
                        language["jarvis_iso2languagecode6391"] = "DE";
                        language["jarvis_name"] = "DE";
                        result.Entities.Add(language);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_casesoftoffer'>"))
                    {
                        Entity casesoftoffer = new Entity("jarvis_casesoftoffer");
                        casesoftoffer.Id = Guid.NewGuid();
                        casesoftoffer["jarvis_name"] = "DEU";
                        casesoftoffer["jarvis_contractno"] = "DE";
                        casesoftoffer["jarvis_description"] = "DE";
                        casesoftoffer["jarvis_startdate"] = DateTime.UtcNow;
                        casesoftoffer["jarvis_expirydate"] = DateTime.UtcNow;
                        casesoftoffer["statuscode"] = new OptionSetValue(0);
                        casesoftoffer["jarvis_softoffercodelookup"] = new EntityReference("jarvis_softoffer", Guid.NewGuid());
                        result.Entities.Add(casesoftoffer);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_softoffer'>"))
                    {
                        Entity softoffer = new Entity("jarvis_softoffer");
                        softoffer.Id = Guid.NewGuid();
                        softoffer["jarvis_name"] = "DEU";
                        softoffer["jarvis_softoffercode"] = new EntityReference("jarvis_softoffer", Guid.NewGuid());
                        softoffer["jarvis_contractno"] = "DE";
                        softoffer["jarvis_startdate"] = DateTime.UtcNow;
                        softoffer["jarvis_expirydate"] = DateTime.UtcNow;
                        softoffer["statuscode"] = new OptionSetValue(0);
                        softoffer["jarvis_marketcode"] = "DE";
                        softoffer["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
                        result.Entities.Add(softoffer);
                    }

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
                        passoutResult.Id = Guid.NewGuid();
                        result.Entities.Add(passoutResult);
                    }
                }

                return result;
            };

            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "incident")
                {
                    Entity inc = new Entity("incident");
                    inc.Id = guid;
                    inc["customerid"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_callerlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    inc["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    inc["jarvis_driverlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    inc["jarvis_translationstatusreportedfault"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatuslocation"] = new OptionSetValue(334030000);
                    inc["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    inc["jarvis_translationstatuscustomerexpectations"] = new OptionSetValue(334030000);
                    return inc;
                }

                if (entityName == "jarvis_vehicle")
                {
                    Entity client = new Entity("jarvis_vehicle");
                    client.Id = new Guid("6186f7f1-5f09-ed11-82e5-002248934e82");
                    client["jarvis_brandid"] = new EntityReference("jarvis_brand", Guid.NewGuid());
                    client["jarvis_fuelpowertype"] = new EntityReference("jarvis_vehiclefuelpowertype", Guid.NewGuid());
                    return client;
                }

                if (entityName == "jarvis_country")
                {
                    Entity country = new Entity("jarvis_country");
                    country.Id = guid;
                    country["jarvis_iso2countrycode"] = "NL";
                    country.Attributes[JarvisCountry.JarvisAverageetaduration] = "12";
                    return country;
                }

                if (entityName == "jarvis_language")
                {
                    Entity language = new Entity("jarvis_language");
                    language.Id = guid;
                    language["jarvis_iso3languagecode6392t"] = "DEU";
                    language["jarvis_iso2languagecode6391"] = "DE";
                    return language;
                }

                if (entityName == "account")
                {
                    Entity account = new Entity("account");
                    account.Id = guid;
                    account["jarvis_language"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    account["jarvis_address1_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    return account;
                }

                return null;
            };
            this.Service.ExecuteOrganizationRequest = processInstanceRequest =>
            {
                if (processInstanceRequest.RequestName.Contains("RetrieveProcessInstances"))
                {
                    return new RetrieveProcessInstancesResponse()
                    {
                        Results = new ParameterCollection
                {
                    new KeyValuePair<string, object>("Processes", new EntityCollection(new List<Entity> { incident })),
                },
                    };
                }

                if (processInstanceRequest.RequestName.Contains("RetrieveActivePath"))
                {
                    Entity activepath = new Entity("activepath");
                    activepath["stagename"] = "Guarantee Of Payment";
                    activepath["processstageid"] = Guid.NewGuid();
                    return new RetrieveActivePathResponse()
                    {
                        Results = new ParameterCollection
                {
                    new KeyValuePair<string, object>("ProcessStages", new EntityCollection(new List<Entity> { activepath })),
                },
                    };
                }

                return null;
            };

            CasePostOperationSync plugin = new CasePostOperationSync();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// PreValidate Update With Complete Data Test Method Create with Mercurius status 700.
        /// </summary>
        [TestMethod]
        public void PostUpdateWithCompleteDataTestMethodCreateMerStatus700()
        {
            Entity incidentTarget = new Entity("incident");

            EntityReference users = new EntityReference("systemuser", Guid.NewGuid());
            users.Name = "MERCURIUS";

            incidentTarget.Id = Guid.Empty;

            incidentTarget["statuscode"] = new OptionSetValue(10);
            incidentTarget["incidentid"] = Guid.NewGuid();
            incidentTarget["caseorigincode"] = new OptionSetValue(2);
            incidentTarget["casetypecode"] = new OptionSetValue(2);
            incidentTarget["jarvis_mercuriusstatus"] = new OptionSetValue(700);
            incidentTarget["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
            incidentTarget["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["customerid"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["jarvis_registrationnumber"] = "123";
            incidentTarget["description"] = "This is for testing";
            incidentTarget["jarvis_location"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incidentTarget["jarvis_customerexpectations"] = "test";
            incidentTarget["jarvis_callerrole"] = new OptionSetValue(4);
            incidentTarget["jarvis_callerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incidentTarget["isescalated"] = true;
            incidentTarget["createdon"] = DateTime.UtcNow;
            incidentTarget["jarvis_assistancetype"] = new OptionSetValue(334030000);

            Entity incident = new Entity("incident");
            incident.Id = Guid.Empty;
            incident["statuscode"] = new OptionSetValue(10);
            incident["incidentid"] = new EntityReference("incident", Guid.NewGuid());
            incident["caseorigincode"] = new OptionSetValue(2);
            incident["casetypecode"] = new OptionSetValue(2);
            incident["jarvis_mercuriusstatus"] = new OptionSetValue(700);
            incident["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incident["customerid"] = new EntityReference("account", Guid.NewGuid());
            incident["jarvis_registrationnumber"] = "123";
            incident["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["description"] = "This is for testing";
            incident["jarvis_driverlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incident["jarvis_translationstatusreportedfault"] = new OptionSetValue(334030000);
            incident["jarvis_translationstatuslocation"] = new OptionSetValue(334030000);
            incident["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["jarvis_translationstatuscustomerexpectations"] = new OptionSetValue(334030000);
            incident["jarvis_location"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["jarvis_customerexpectations"] = "test";
            incident["ownerid"] = new EntityReference { LogicalName = "system_user", Name = "test" };
            incident["jarvis_callerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incident["jarvis_callerrole"] = new OptionSetValue(4);
            incident["createdby"] = users;
            incident.Attributes["jarvis_etc"] = DateTime.Now;
            //// Setting Input Parameters.
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", incidentTarget),
               };
            this.PluginExecutionContext.MessageNameGet = () => "CREATE";
            this.PluginExecutionContext.StageGet = () => 40;
            //// Setting Post and Pre Image
            this.PluginExecutionContext.PostEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PostImage", incident },
                };
            };
            this.PluginExecutionContext.SharedVariablesGet = () =>
            {
                return new ParameterCollection();
            };

            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();
                if (query.GetType().Name == "FetchExpression")
                {
                    if ((query as FetchExpression).Query.Contains("<entity name='incident'>"))
                    {
                        Entity incidentResult = new Entity("incident");
                        incidentResult[Constants.IncidentNature.IncidentIncidentNature_jarvisIincidentnatureid] = new AliasedValue("incidentNature", "incidentnatureid", Guid.NewGuid());
                        incidentResult["jarvis_hdrd"] = false;
                        incidentResult["jarvis_homedealer"] = new EntityReference("account", incident.Id);
                        incidentResult["CaseRD.jarvis_repairingdealer"] = new AliasedValue("account", "account", new EntityReference("account", incident.Id));
                        incidentResult.Id = Guid.NewGuid();
                        result.Entities.Add(incidentResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_casecontact'>"))
                    {
                        Entity caseContact = new Entity("jarvis_casecontact");
                        caseContact["jarvis_name"] = "testcontact";
                        caseContact["jarvis_preferredlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
                        caseContact["createdon"] = DateTime.UtcNow;
                        caseContact["jarvis_mobilephone"] = "9248428492";
                        caseContact["jarvis_role"] = new OptionSetValue(2);
                        caseContact.Id = Guid.NewGuid();
                        result.Entities.Add(caseContact);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='workflow'>"))
                    {
                        Entity scheduleConsent = new Entity("workflow");
                        scheduleConsent["name"] = "BreakdownProcess";
                        scheduleConsent.Id = Guid.NewGuid();
                        result.Entities.Add(scheduleConsent);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_configurationjarvis'>"))
                    {
                        Entity configResult = new Entity("jarvis_configurationjarvis");
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationcasestatuschange"] = true;
                        configResult["createdon"] = DateTime.UtcNow;
                        configResult.Id = Guid.NewGuid();
                        result.Entities.Add(configResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_integrationconfiguration'>"))
                    {
                        Entity configResult = new Entity("jarvis_integrationconfiguration");
                        configResult["jarvis_integrationname"] = "test";
                        configResult["jarvis_integrationcode"] = "test";
                        configResult["createdon"] = DateTime.UtcNow;
                        configResult.Id = Guid.NewGuid();
                        result.Entities.Add(configResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_language'>"))
                    {
                        Entity language = new Entity("jarvis_language");
                        language.Id = Guid.NewGuid();
                        language["jarvis_iso3languagecode6392t"] = "DEU";
                        language["jarvis_iso2languagecode6391"] = "DE";
                        language["jarvis_name"] = "DE";
                        result.Entities.Add(language);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_casesoftoffer'>"))
                    {
                        Entity casesoftoffer = new Entity("jarvis_casesoftoffer");
                        casesoftoffer.Id = Guid.NewGuid();
                        casesoftoffer["jarvis_name"] = "DEU";
                        casesoftoffer["jarvis_contractno"] = "DE";
                        casesoftoffer["jarvis_description"] = "DE";
                        casesoftoffer["jarvis_startdate"] = DateTime.UtcNow;
                        casesoftoffer["jarvis_expirydate"] = DateTime.UtcNow;
                        casesoftoffer["statuscode"] = new OptionSetValue(0);
                        casesoftoffer["jarvis_softoffercodelookup"] = new EntityReference("jarvis_softoffer", Guid.NewGuid());
                        result.Entities.Add(casesoftoffer);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_softoffer'>"))
                    {
                        Entity softoffer = new Entity("jarvis_softoffer");
                        softoffer.Id = Guid.NewGuid();
                        softoffer["jarvis_name"] = "DEU";
                        softoffer["jarvis_softoffercode"] = new EntityReference("jarvis_softoffer", Guid.NewGuid());
                        softoffer["jarvis_contractno"] = "DE";
                        softoffer["jarvis_startdate"] = DateTime.UtcNow;
                        softoffer["jarvis_expirydate"] = DateTime.UtcNow;
                        softoffer["statuscode"] = new OptionSetValue(0);
                        softoffer["jarvis_marketcode"] = "DE";
                        softoffer["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
                        result.Entities.Add(softoffer);
                    }

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
                        passoutResult.Id = Guid.NewGuid();
                        result.Entities.Add(passoutResult);
                    }
                }

                return result;
            };

            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "incident")
                {
                    Entity inc = new Entity("incident");
                    inc.Id = guid;
                    inc["customerid"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_callerlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    inc["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    inc["jarvis_driverlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    inc["jarvis_translationstatusreportedfault"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatuslocation"] = new OptionSetValue(334030000);
                    inc["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    inc["jarvis_translationstatuscustomerexpectations"] = new OptionSetValue(334030000);
                    return inc;
                }

                if (entityName == "jarvis_vehicle")
                {
                    Entity client = new Entity("jarvis_vehicle");
                    client.Id = new Guid("6186f7f1-5f09-ed11-82e5-002248934e82");
                    client["jarvis_brandid"] = new EntityReference("jarvis_brand", Guid.NewGuid());
                    client["jarvis_fuelpowertype"] = new EntityReference("jarvis_vehiclefuelpowertype", Guid.NewGuid());
                    return client;
                }

                if (entityName == "jarvis_country")
                {
                    Entity country = new Entity("jarvis_country");
                    country.Id = guid;
                    country["jarvis_iso2countrycode"] = "NL";
                    country.Attributes[JarvisCountry.JarvisAverageetaduration] = "12";
                    return country;
                }

                if (entityName == "jarvis_language")
                {
                    Entity language = new Entity("jarvis_language");
                    language.Id = guid;
                    language["jarvis_iso3languagecode6392t"] = "DEU";
                    language["jarvis_iso2languagecode6391"] = "DE";
                    return language;
                }

                if (entityName == "account")
                {
                    Entity account = new Entity("account");
                    account.Id = guid;
                    account["jarvis_language"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    account["jarvis_address1_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    return account;
                }

                return null;
            };
            this.Service.ExecuteOrganizationRequest = processInstanceRequest =>
            {
                if (processInstanceRequest.RequestName.Contains("RetrieveProcessInstances"))
                {
                    return new RetrieveProcessInstancesResponse()
                    {
                        Results = new ParameterCollection
                {
                    new KeyValuePair<string, object>("Processes", new EntityCollection(new List<Entity> { incident })),
                },
                    };
                }

                if (processInstanceRequest.RequestName.Contains("RetrieveActivePath"))
                {
                    Entity activepath = new Entity("activepath");
                    activepath["stagename"] = "Guarantee Of Payment";
                    activepath["processstageid"] = Guid.NewGuid();
                    return new RetrieveActivePathResponse()
                    {
                        Results = new ParameterCollection
                {
                    new KeyValuePair<string, object>("ProcessStages", new EntityCollection(new List<Entity> { activepath })),
                },
                    };
                }

                return null;
            };

            CasePostOperationSync plugin = new CasePostOperationSync();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// PreValidate Update With Complete Data Test Method Create with Mercurius status 800.
        /// </summary>
        [TestMethod]
        public void PostUpdateWithCompleteDataTestMethodCreateMerStatus800()
        {
            Entity incidentTarget = new Entity("incident");

            EntityReference users = new EntityReference("systemuser", Guid.NewGuid());
            users.Name = "MERCURIUS";

            incidentTarget.Id = Guid.Empty;

            incidentTarget["statuscode"] = new OptionSetValue(10);
            incidentTarget["incidentid"] = Guid.NewGuid();
            incidentTarget["caseorigincode"] = new OptionSetValue(2);
            incidentTarget["casetypecode"] = new OptionSetValue(2);
            incidentTarget["jarvis_mercuriusstatus"] = new OptionSetValue(800);
            incidentTarget["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
            incidentTarget["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["customerid"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["jarvis_registrationnumber"] = "123";
            incidentTarget["description"] = "This is for testing";
            incidentTarget["jarvis_location"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incidentTarget["jarvis_customerexpectations"] = "test";
            incidentTarget["jarvis_callerrole"] = new OptionSetValue(4);
            incidentTarget["jarvis_callerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incidentTarget["isescalated"] = true;
            incidentTarget["createdon"] = DateTime.UtcNow;
            incidentTarget["jarvis_assistancetype"] = new OptionSetValue(334030000);

            Entity incident = new Entity("incident");
            incident.Id = Guid.Empty;
            incident["statuscode"] = new OptionSetValue(10);
            incident["incidentid"] = new EntityReference("incident", Guid.NewGuid());
            incident["caseorigincode"] = new OptionSetValue(2);
            incident["casetypecode"] = new OptionSetValue(2);
            incident["jarvis_mercuriusstatus"] = new OptionSetValue(800);
            incident["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incident["customerid"] = new EntityReference("account", Guid.NewGuid());
            incident["jarvis_registrationnumber"] = "123";
            incident["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["description"] = "This is for testing";
            incident["jarvis_driverlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incident["jarvis_translationstatusreportedfault"] = new OptionSetValue(334030000);
            incident["jarvis_translationstatuslocation"] = new OptionSetValue(334030000);
            incident["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["jarvis_translationstatuscustomerexpectations"] = new OptionSetValue(334030000);
            incident["jarvis_location"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["jarvis_customerexpectations"] = "test";
            incident["ownerid"] = new EntityReference { LogicalName = "system_user", Name = "test" };
            incident["jarvis_callerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incident["jarvis_callerrole"] = new OptionSetValue(4);
            incident["createdby"] = users;
            incident.Attributes["jarvis_etc"] = DateTime.Now;
            //// Setting Input Parameters.
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", incidentTarget),
               };
            this.PluginExecutionContext.MessageNameGet = () => "CREATE";
            this.PluginExecutionContext.StageGet = () => 40;
            //// Setting Post and Pre Image
            this.PluginExecutionContext.PostEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PostImage", incident },
                };
            };
            this.PluginExecutionContext.SharedVariablesGet = () =>
            {
                return new ParameterCollection();
            };

            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();
                if (query.GetType().Name == "FetchExpression")
                {
                    if ((query as FetchExpression).Query.Contains("<entity name='incident'>"))
                    {
                        Entity incidentResult = new Entity("incident");
                        incidentResult[Constants.IncidentNature.IncidentIncidentNature_jarvisIincidentnatureid] = new AliasedValue("incidentNature", "incidentnatureid", Guid.NewGuid());
                        incidentResult["jarvis_hdrd"] = false;
                        incidentResult["jarvis_homedealer"] = new EntityReference("account", incident.Id);
                        incidentResult["CaseRD.jarvis_repairingdealer"] = new AliasedValue("account", "account", new EntityReference("account", incident.Id));
                        incidentResult.Id = Guid.NewGuid();
                        result.Entities.Add(incidentResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_casecontact'>"))
                    {
                        Entity caseContact = new Entity("jarvis_casecontact");
                        caseContact["jarvis_name"] = "testcontact";
                        caseContact["jarvis_preferredlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
                        caseContact["createdon"] = DateTime.UtcNow;
                        caseContact["jarvis_mobilephone"] = "9248428492";
                        caseContact["jarvis_role"] = new OptionSetValue(2);
                        caseContact.Id = Guid.NewGuid();
                        result.Entities.Add(caseContact);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='workflow'>"))
                    {
                        Entity scheduleConsent = new Entity("workflow");
                        scheduleConsent["name"] = "BreakdownProcess";
                        scheduleConsent.Id = Guid.NewGuid();
                        result.Entities.Add(scheduleConsent);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_configurationjarvis'>"))
                    {
                        Entity configResult = new Entity("jarvis_configurationjarvis");
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationcasestatuschange"] = true;
                        configResult["createdon"] = DateTime.UtcNow;
                        configResult.Id = Guid.NewGuid();
                        result.Entities.Add(configResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_integrationconfiguration'>"))
                    {
                        Entity configResult = new Entity("jarvis_integrationconfiguration");
                        configResult["jarvis_integrationname"] = "test";
                        configResult["jarvis_integrationcode"] = "test";
                        configResult["createdon"] = DateTime.UtcNow;
                        configResult.Id = Guid.NewGuid();
                        result.Entities.Add(configResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_language'>"))
                    {
                        Entity language = new Entity("jarvis_language");
                        language.Id = Guid.NewGuid();
                        language["jarvis_iso3languagecode6392t"] = "DEU";
                        language["jarvis_iso2languagecode6391"] = "DE";
                        language["jarvis_name"] = "DE";
                        result.Entities.Add(language);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_casesoftoffer'>"))
                    {
                        Entity casesoftoffer = new Entity("jarvis_casesoftoffer");
                        casesoftoffer.Id = Guid.NewGuid();
                        casesoftoffer["jarvis_name"] = "DEU";
                        casesoftoffer["jarvis_contractno"] = "DE";
                        casesoftoffer["jarvis_description"] = "DE";
                        casesoftoffer["jarvis_startdate"] = DateTime.UtcNow;
                        casesoftoffer["jarvis_expirydate"] = DateTime.UtcNow;
                        casesoftoffer["statuscode"] = new OptionSetValue(0);
                        casesoftoffer["jarvis_softoffercodelookup"] = new EntityReference("jarvis_softoffer", Guid.NewGuid());
                        result.Entities.Add(casesoftoffer);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_softoffer'>"))
                    {
                        Entity softoffer = new Entity("jarvis_softoffer");
                        softoffer.Id = Guid.NewGuid();
                        softoffer["jarvis_name"] = "DEU";
                        softoffer["jarvis_softoffercode"] = new EntityReference("jarvis_softoffer", Guid.NewGuid());
                        softoffer["jarvis_contractno"] = "DE";
                        softoffer["jarvis_startdate"] = DateTime.UtcNow;
                        softoffer["jarvis_expirydate"] = DateTime.UtcNow;
                        softoffer["statuscode"] = new OptionSetValue(0);
                        softoffer["jarvis_marketcode"] = "DE";
                        softoffer["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
                        result.Entities.Add(softoffer);
                    }

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
                        passoutResult.Id = Guid.NewGuid();
                        result.Entities.Add(passoutResult);
                    }
                }

                return result;
            };

            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "incident")
                {
                    Entity inc = new Entity("incident");
                    inc.Id = guid;
                    inc["customerid"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_callerlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    inc["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    inc["jarvis_driverlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    inc["jarvis_translationstatusreportedfault"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatuslocation"] = new OptionSetValue(334030000);
                    inc["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    inc["jarvis_translationstatuscustomerexpectations"] = new OptionSetValue(334030000);
                    return inc;
                }

                if (entityName == "jarvis_vehicle")
                {
                    Entity client = new Entity("jarvis_vehicle");
                    client.Id = new Guid("6186f7f1-5f09-ed11-82e5-002248934e82");
                    client["jarvis_brandid"] = new EntityReference("jarvis_brand", Guid.NewGuid());
                    client["jarvis_fuelpowertype"] = new EntityReference("jarvis_vehiclefuelpowertype", Guid.NewGuid());
                    return client;
                }

                if (entityName == "jarvis_country")
                {
                    Entity country = new Entity("jarvis_country");
                    country.Id = guid;
                    country["jarvis_iso2countrycode"] = "NL";
                    country.Attributes[JarvisCountry.JarvisAverageetaduration] = "12";
                    return country;
                }

                if (entityName == "jarvis_language")
                {
                    Entity language = new Entity("jarvis_language");
                    language.Id = guid;
                    language["jarvis_iso3languagecode6392t"] = "DEU";
                    language["jarvis_iso2languagecode6391"] = "DE";
                    return language;
                }

                if (entityName == "account")
                {
                    Entity account = new Entity("account");
                    account.Id = guid;
                    account["jarvis_language"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    account["jarvis_address1_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    return account;
                }

                return null;
            };
            this.Service.ExecuteOrganizationRequest = processInstanceRequest =>
            {
                if (processInstanceRequest.RequestName.Contains("RetrieveProcessInstances"))
                {
                    return new RetrieveProcessInstancesResponse()
                    {
                        Results = new ParameterCollection
                {
                    new KeyValuePair<string, object>("Processes", new EntityCollection(new List<Entity> { incident })),
                },
                    };
                }

                if (processInstanceRequest.RequestName.Contains("RetrieveActivePath"))
                {
                    Entity activepath = new Entity("activepath");
                    activepath["stagename"] = "Guarantee Of Payment";
                    activepath["processstageid"] = Guid.NewGuid();
                    return new RetrieveActivePathResponse()
                    {
                        Results = new ParameterCollection
                {
                    new KeyValuePair<string, object>("ProcessStages", new EntityCollection(new List<Entity> { activepath })),
                },
                    };
                }

                return null;
            };

            CasePostOperationSync plugin = new CasePostOperationSync();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// PreValidate Update With Complete Data Test Method Create with Mercurius status 900.
        /// </summary>
        [TestMethod]
        public void PostUpdateWithCompleteDataTestMethodCreateMerStatus900()
        {
            Entity incidentTarget = new Entity("incident");

            EntityReference users = new EntityReference("systemuser", Guid.NewGuid());
            users.Name = "MERCURIUS";

            incidentTarget.Id = Guid.Empty;

            incidentTarget["statuscode"] = new OptionSetValue(10);
            incidentTarget["incidentid"] = Guid.NewGuid();
            incidentTarget["caseorigincode"] = new OptionSetValue(2);
            incidentTarget["casetypecode"] = new OptionSetValue(2);
            incidentTarget["jarvis_mercuriusstatus"] = new OptionSetValue(900);
            incidentTarget["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
            incidentTarget["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["customerid"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["jarvis_registrationnumber"] = "123";
            incidentTarget["description"] = "This is for testing";
            incidentTarget["jarvis_location"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incidentTarget["jarvis_customerexpectations"] = "test";
            incidentTarget["jarvis_callerrole"] = new OptionSetValue(4);
            incidentTarget["jarvis_callerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incidentTarget["isescalated"] = true;
            incidentTarget["createdon"] = DateTime.UtcNow;
            incidentTarget["jarvis_assistancetype"] = new OptionSetValue(334030000);

            Entity incident = new Entity("incident");
            incident.Id = Guid.Empty;
            incident["statuscode"] = new OptionSetValue(10);
            incident["incidentid"] = new EntityReference("incident", Guid.NewGuid());
            incident["caseorigincode"] = new OptionSetValue(2);
            incident["casetypecode"] = new OptionSetValue(2);
            incident["jarvis_mercuriusstatus"] = new OptionSetValue(900);
            incident["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incident["customerid"] = new EntityReference("account", Guid.NewGuid());
            incident["jarvis_registrationnumber"] = "123";
            incident["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["description"] = "This is for testing";
            incident["jarvis_driverlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incident["jarvis_translationstatusreportedfault"] = new OptionSetValue(334030000);
            incident["jarvis_translationstatuslocation"] = new OptionSetValue(334030000);
            incident["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["jarvis_translationstatuscustomerexpectations"] = new OptionSetValue(334030000);
            incident["jarvis_location"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["jarvis_customerexpectations"] = "test";
            incident["ownerid"] = new EntityReference { LogicalName = "system_user", Name = "test" };
            incident["jarvis_callerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incident["jarvis_callerrole"] = new OptionSetValue(4);
            incident["createdby"] = users;
            incident.Attributes["jarvis_etc"] = DateTime.Now;
            //// Setting Input Parameters.
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", incidentTarget),
               };
            this.PluginExecutionContext.MessageNameGet = () => "CREATE";
            this.PluginExecutionContext.StageGet = () => 40;
            //// Setting Post and Pre Image
            this.PluginExecutionContext.PostEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PostImage", incident },
                };
            };
            this.PluginExecutionContext.SharedVariablesGet = () =>
            {
                return new ParameterCollection();
            };

            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();
                if (query.GetType().Name == "FetchExpression")
                {
                    if ((query as FetchExpression).Query.Contains("<entity name='incident'>"))
                    {
                        Entity incidentResult = new Entity("incident");
                        incidentResult[Constants.IncidentNature.IncidentIncidentNature_jarvisIincidentnatureid] = new AliasedValue("incidentNature", "incidentnatureid", Guid.NewGuid());
                        incidentResult["jarvis_hdrd"] = false;
                        incidentResult["jarvis_homedealer"] = new EntityReference("account", incident.Id);
                        incidentResult["CaseRD.jarvis_repairingdealer"] = new AliasedValue("account", "account", new EntityReference("account", incident.Id));
                        incidentResult.Id = Guid.NewGuid();
                        result.Entities.Add(incidentResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_casecontact'>"))
                    {
                        Entity caseContact = new Entity("jarvis_casecontact");
                        caseContact["jarvis_name"] = "testcontact";
                        caseContact["jarvis_preferredlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
                        caseContact["createdon"] = DateTime.UtcNow;
                        caseContact["jarvis_mobilephone"] = "9248428492";
                        caseContact["jarvis_role"] = new OptionSetValue(2);
                        caseContact.Id = Guid.NewGuid();
                        result.Entities.Add(caseContact);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='workflow'>"))
                    {
                        Entity scheduleConsent = new Entity("workflow");
                        scheduleConsent["name"] = "BreakdownProcess";
                        scheduleConsent.Id = Guid.NewGuid();
                        result.Entities.Add(scheduleConsent);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_configurationjarvis'>"))
                    {
                        Entity configResult = new Entity("jarvis_configurationjarvis");
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationcasestatuschange"] = true;
                        configResult["createdon"] = DateTime.UtcNow;
                        configResult.Id = Guid.NewGuid();
                        result.Entities.Add(configResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_integrationconfiguration'>"))
                    {
                        Entity configResult = new Entity("jarvis_integrationconfiguration");
                        configResult["jarvis_integrationname"] = "test";
                        configResult["jarvis_integrationcode"] = "test";
                        configResult["createdon"] = DateTime.UtcNow;
                        configResult.Id = Guid.NewGuid();
                        result.Entities.Add(configResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_language'>"))
                    {
                        Entity language = new Entity("jarvis_language");
                        language.Id = Guid.NewGuid();
                        language["jarvis_iso3languagecode6392t"] = "DEU";
                        language["jarvis_iso2languagecode6391"] = "DE";
                        language["jarvis_name"] = "DE";
                        result.Entities.Add(language);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_casesoftoffer'>"))
                    {
                        Entity casesoftoffer = new Entity("jarvis_casesoftoffer");
                        casesoftoffer.Id = Guid.NewGuid();
                        casesoftoffer["jarvis_name"] = "DEU";
                        casesoftoffer["jarvis_contractno"] = "DE";
                        casesoftoffer["jarvis_description"] = "DE";
                        casesoftoffer["jarvis_startdate"] = DateTime.UtcNow;
                        casesoftoffer["jarvis_expirydate"] = DateTime.UtcNow;
                        casesoftoffer["statuscode"] = new OptionSetValue(0);
                        casesoftoffer["jarvis_softoffercodelookup"] = new EntityReference("jarvis_softoffer", Guid.NewGuid());
                        result.Entities.Add(casesoftoffer);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_softoffer'>"))
                    {
                        Entity softoffer = new Entity("jarvis_softoffer");
                        softoffer.Id = Guid.NewGuid();
                        softoffer["jarvis_name"] = "DEU";
                        softoffer["jarvis_softoffercode"] = new EntityReference("jarvis_softoffer", Guid.NewGuid());
                        softoffer["jarvis_contractno"] = "DE";
                        softoffer["jarvis_startdate"] = DateTime.UtcNow;
                        softoffer["jarvis_expirydate"] = DateTime.UtcNow;
                        softoffer["statuscode"] = new OptionSetValue(0);
                        softoffer["jarvis_marketcode"] = "DE";
                        softoffer["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
                        result.Entities.Add(softoffer);
                    }

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
                        passoutResult.Id = Guid.NewGuid();
                        result.Entities.Add(passoutResult);
                    }
                }

                return result;
            };

            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "incident")
                {
                    Entity inc = new Entity("incident");
                    inc.Id = guid;
                    inc["customerid"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_callerlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    inc["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    inc["jarvis_driverlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    inc["jarvis_translationstatusreportedfault"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatuslocation"] = new OptionSetValue(334030000);
                    inc["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    inc["jarvis_translationstatuscustomerexpectations"] = new OptionSetValue(334030000);
                    return inc;
                }

                if (entityName == "jarvis_vehicle")
                {
                    Entity client = new Entity("jarvis_vehicle");
                    client.Id = new Guid("6186f7f1-5f09-ed11-82e5-002248934e82");
                    client["jarvis_brandid"] = new EntityReference("jarvis_brand", Guid.NewGuid());
                    client["jarvis_fuelpowertype"] = new EntityReference("jarvis_vehiclefuelpowertype", Guid.NewGuid());
                    return client;
                }

                if (entityName == "jarvis_country")
                {
                    Entity country = new Entity("jarvis_country");
                    country.Id = guid;
                    country["jarvis_iso2countrycode"] = "NL";
                    country.Attributes[JarvisCountry.JarvisAverageetaduration] = "12";
                    return country;
                }

                if (entityName == "jarvis_language")
                {
                    Entity language = new Entity("jarvis_language");
                    language.Id = guid;
                    language["jarvis_iso3languagecode6392t"] = "DEU";
                    language["jarvis_iso2languagecode6391"] = "DE";
                    return language;
                }

                if (entityName == "account")
                {
                    Entity account = new Entity("account");
                    account.Id = guid;
                    account["jarvis_language"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    account["jarvis_address1_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    return account;
                }

                return null;
            };
            this.Service.ExecuteOrganizationRequest = processInstanceRequest =>
            {
                if (processInstanceRequest.RequestName.Contains("RetrieveProcessInstances"))
                {
                    return new RetrieveProcessInstancesResponse()
                    {
                        Results = new ParameterCollection
                {
                    new KeyValuePair<string, object>("Processes", new EntityCollection(new List<Entity> { incident })),
                },
                    };
                }

                if (processInstanceRequest.RequestName.Contains("RetrieveActivePath"))
                {
                    Entity activepath = new Entity("activepath");
                    activepath["stagename"] = "Guarantee Of Payment";
                    activepath["processstageid"] = Guid.NewGuid();
                    return new RetrieveActivePathResponse()
                    {
                        Results = new ParameterCollection
                {
                    new KeyValuePair<string, object>("ProcessStages", new EntityCollection(new List<Entity> { activepath })),
                },
                    };
                }

                return null;
            };

            CasePostOperationSync plugin = new CasePostOperationSync();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// PreValidate Update With Complete Data Test Method with statuscode 50.
        /// </summary>
        [TestMethod]
        public void PostUpdateWithCompleteDataTestMethodStCode50()
        {
            Entity incidentTarget = new Entity("incident");

            EntityReference users = new EntityReference("systemuser", Guid.NewGuid());
            users.Name = "MERCURIUS";

            incidentTarget.Id = Guid.Empty;

            incidentTarget["statuscode"] = new OptionSetValue(50);
            incidentTarget["incidentid"] = Guid.NewGuid();
            incidentTarget["caseorigincode"] = new OptionSetValue(2);
            incidentTarget["casetypecode"] = new OptionSetValue(2);
            incidentTarget["jarvis_mercuriusstatus"] = new OptionSetValue(200);
            incidentTarget["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
            incidentTarget["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["customerid"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["jarvis_registrationnumber"] = "123";
            incidentTarget["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incidentTarget["description"] = "This is for testing";
            incidentTarget["jarvis_location"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incidentTarget["jarvis_customerexpectations"] = "test";
            incidentTarget["jarvis_incidentnature"] = "[{\"id\":\"3efba61a-85ad-44b7-a187-7f54acaf7a67\"}]";
            incidentTarget["jarvis_callerrole"] = new OptionSetValue(3);
            incidentTarget["jarvis_callerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incidentTarget["isescalated"] = true;
            incidentTarget.Attributes["modifiedby"] = users;
            Entity incident = new Entity("incident");
            incident.Id = Guid.Empty;
            incident["statuscode"] = new OptionSetValue(10);
            incident["incidentid"] = new EntityReference("incident", Guid.NewGuid());
            incident["caseorigincode"] = new OptionSetValue(2);
            incident["casetypecode"] = new OptionSetValue(2);
            incident["jarvis_mercuriusstatus"] = new OptionSetValue(200);
            incident["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
            incident["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incident["customerid"] = new EntityReference("account", Guid.NewGuid());
            incident["jarvis_registrationnumber"] = "123";
            incident["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["description"] = "This is for testing";
            incident["jarvis_driverlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incident["jarvis_translationstatusreportedfault"] = new OptionSetValue(334030000);
            incident["jarvis_translationstatuslocation"] = new OptionSetValue(334030000);
            incident["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["jarvis_translationstatuscustomerexpectations"] = new OptionSetValue(334030000);
            incident["jarvis_location"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["jarvis_customerexpectations"] = "test";
            incident["ownerid"] = new EntityReference { LogicalName = "system_user", Name = "test" };
            incident["jarvis_sourceid"] = 232323232;
            incident["jarvis_callerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incident["jarvis_callerrole"] = new OptionSetValue(3);
            incident["createdby"] = users;
            incident.Attributes["jarvis_etc"] = DateTime.Now;
            //// Setting Input Parameters.
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", incidentTarget),
               };
            this.PluginExecutionContext.MessageNameGet = () => "Update";
            this.PluginExecutionContext.StageGet = () => 40;
            //// Setting Post and Pre Image
            this.PluginExecutionContext.PostEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PostImage", incident },
                };
            };
            this.PluginExecutionContext.PreEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PreImage", incident },
                };
            };

            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();
                if (query.GetType().Name == "FetchExpression")
                {
                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_vasbreakdownprocess'>"))
                    {
                        Entity vasprocess = new Entity("jarvis_vasbreakdownprocess");
                        vasprocess["businessprocessflowinstanceid"] = Guid.NewGuid();
                        vasprocess["bpf_name"] = "test";
                        vasprocess["bpf_incidentid"] = Guid.NewGuid();
                        vasprocess["activestageid"] = Guid.NewGuid();
                        vasprocess["statecode"] = new OptionSetValue(0);
                        vasprocess["createdon"] = DateTime.UtcNow;
                        vasprocess["createdby"] = "test";
                        vasprocess.Id = Guid.NewGuid();
                        result.Entities.Add(vasprocess);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='incident'>"))
                    {
                        Entity incidentResult = new Entity("incident");
                        incidentResult[Constants.IncidentNature.IncidentIncidentNature_jarvisIincidentnatureid] = new AliasedValue("incidentNature", "incidentnatureid", Guid.NewGuid());
                        incidentResult["jarvis_hdrd"] = false;
                        incidentResult["jarvis_homedealer"] = new EntityReference("account", incident.Id);
                        incidentResult["CaseRD.jarvis_repairingdealer"] = new AliasedValue("account", "account", new EntityReference("account", incident.Id));
                        incidentResult.Id = Guid.NewGuid();
                        result.Entities.Add(incidentResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_casecontact'>"))
                    {
                        Entity caseContact = new Entity("jarvis_casecontact");
                        caseContact["jarvis_name"] = "testcontact";
                        caseContact["jarvis_preferredlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
                        caseContact["createdon"] = DateTime.UtcNow;
                        caseContact["jarvis_mobilephone"] = "9248428492";
                        caseContact["jarvis_role"] = new OptionSetValue(2);
                        caseContact.Id = Guid.NewGuid();
                        result.Entities.Add(caseContact);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_configurationjarvis'>"))
                    {
                        Entity configResult = new Entity("jarvis_configurationjarvis");
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationcasestatuschange"] = true;
                        configResult["createdon"] = DateTime.UtcNow;
                        configResult.Id = Guid.NewGuid();
                        result.Entities.Add(configResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_language'>"))
                    {
                        Entity language = new Entity("jarvis_language");
                        language.Id = Guid.NewGuid();
                        language["jarvis_iso3languagecode6392t"] = "DEU";
                        language["jarvis_iso2languagecode6391"] = "DE";
                        language["jarvis_name"] = "DE";
                        result.Entities.Add(language);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_casesoftoffer'>"))
                    {
                        Entity casesoftoffer = new Entity("jarvis_casesoftoffer");
                        casesoftoffer.Id = Guid.NewGuid();
                        casesoftoffer["jarvis_name"] = "DEU";
                        casesoftoffer["jarvis_contractno"] = "DE";
                        casesoftoffer["jarvis_description"] = "DE";
                        casesoftoffer["jarvis_startdate"] = DateTime.UtcNow;
                        casesoftoffer["jarvis_expirydate"] = DateTime.UtcNow;
                        casesoftoffer["statuscode"] = new OptionSetValue(0);
                        casesoftoffer["jarvis_softoffercodelookup"] = new EntityReference("jarvis_softoffer", Guid.NewGuid());
                        result.Entities.Add(casesoftoffer);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_softoffer'>"))
                    {
                        Entity softoffer = new Entity("jarvis_softoffer");
                        softoffer.Id = Guid.NewGuid();
                        softoffer["jarvis_name"] = "DEU";
                        softoffer["jarvis_softoffercode"] = new EntityReference("jarvis_softoffer", Guid.NewGuid());
                        softoffer["jarvis_contractno"] = "DE";
                        softoffer["jarvis_startdate"] = DateTime.UtcNow;
                        softoffer["jarvis_expirydate"] = DateTime.UtcNow;
                        softoffer["statuscode"] = new OptionSetValue(0);
                        softoffer["jarvis_marketcode"] = "DE";
                        softoffer["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
                        result.Entities.Add(softoffer);
                    }

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
                        passoutResult.Id = Guid.NewGuid();
                        result.Entities.Add(passoutResult);
                    }
                }

                return result;
            };

            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "incident")
                {
                    Entity inc = new Entity("incident");
                    inc.Id = guid;
                    inc["customerid"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_callerlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    inc["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    inc["jarvis_driverlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    inc["jarvis_translationstatusreportedfault"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatuslocation"] = new OptionSetValue(334030000);
                    inc["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    inc["jarvis_translationstatuscustomerexpectations"] = new OptionSetValue(334030000);
                    return inc;
                }

                if (entityName == "jarvis_vehicle")
                {
                    Entity client = new Entity("jarvis_vehicle");
                    client.Id = new Guid("6186f7f1-5f09-ed11-82e5-002248934e82");
                    client["jarvis_brandid"] = new EntityReference("jarvis_brand", Guid.NewGuid());
                    client["jarvis_fuelpowertype"] = new EntityReference("jarvis_vehiclefuelpowertype", Guid.NewGuid());
                    return client;
                }

                if (entityName == "jarvis_country")
                {
                    Entity country = new Entity("jarvis_country");
                    country.Id = guid;
                    country["jarvis_iso2countrycode"] = "NL";
                    return country;
                }

                if (entityName == "jarvis_language")
                {
                    Entity language = new Entity("jarvis_language");
                    language.Id = guid;
                    language["jarvis_iso3languagecode6392t"] = "DEU";
                    language["jarvis_iso2languagecode6391"] = "DE";
                    return language;
                }

                if (entityName == "account")
                {
                    Entity account = new Entity("account");
                    account.Id = guid;
                    account["jarvis_language"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    account["jarvis_address1_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    return account;
                }

                if (entityName == "jarvis_brand")
                {
                    Entity client = new Entity("jarvis_brand");
                    client.Id = new Guid("6186f7f1-5f09-ed11-82e5-002248934e82");
                    client["jarvis_mercuriusbrandcode"] = "ted";
                    return client;
                }

                return null;
            };
            this.Service.ExecuteOrganizationRequest = processInstanceRequest =>
            {
                if (processInstanceRequest.RequestName.Contains("RetrieveProcessInstances"))
                {
                    return new RetrieveProcessInstancesResponse()
                {
                    Results = new ParameterCollection
                {
                    new KeyValuePair<string, object>("Processes", new EntityCollection(new List<Entity> { incident })),
                },
                };
                }

                if (processInstanceRequest.RequestName.Contains("RetrieveActivePath"))
                {
                    Entity activepath = new Entity("activepath");
                    activepath["stagename"] = "Waiting For Repair Start";
                    activepath["processstageid"] = Guid.NewGuid();
                    return new RetrieveActivePathResponse()
                    {
                        Results = new ParameterCollection
                {
                    new KeyValuePair<string, object>("ProcessStages", new EntityCollection(new List<Entity> { activepath })),
                },
                    };
                }

                return null;
            };

            CasePostOperationSync plugin = new CasePostOperationSync();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// PreValidate Update With Complete Data Test Method with statuscode 50 with atc.
        /// </summary>
        [TestMethod]
        public void PostUpdateWithCompleteDataTestMethodStCode50WithAtc()
        {
            Entity incidentTarget = new Entity("incident");

            EntityReference users = new EntityReference("systemuser", Guid.NewGuid());
            users.Name = "MERCURIUS";

            incidentTarget.Id = Guid.Empty;

            incidentTarget["statuscode"] = new OptionSetValue(50);
            incidentTarget["incidentid"] = Guid.NewGuid();
            incidentTarget["caseorigincode"] = new OptionSetValue(2);
            incidentTarget["casetypecode"] = new OptionSetValue(2);
            incidentTarget["jarvis_mercuriusstatus"] = new OptionSetValue(200);
            incidentTarget["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
            incidentTarget["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["customerid"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["jarvis_registrationnumber"] = "123";
            incidentTarget["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incidentTarget["description"] = "This is for testing";
            incidentTarget["jarvis_location"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incidentTarget["jarvis_customerexpectations"] = "test";
            incidentTarget["jarvis_incidentnature"] = "[{\"id\":\"3efba61a-85ad-44b7-a187-7f54acaf7a67\"}]";
            incidentTarget["jarvis_callerrole"] = new OptionSetValue(3);
            incidentTarget["jarvis_callerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incidentTarget["isescalated"] = true;
            incidentTarget.Attributes["modifiedby"] = users;

            Entity incident = new Entity("incident");
            incident.Id = Guid.Empty;
            incident["statuscode"] = new OptionSetValue(10);
            incident["incidentid"] = new EntityReference("incident", Guid.NewGuid());
            incident["caseorigincode"] = new OptionSetValue(2);
            incident["casetypecode"] = new OptionSetValue(2);
            incident["jarvis_mercuriusstatus"] = new OptionSetValue(200);
            incident["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
            incident["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incident["customerid"] = new EntityReference("account", Guid.NewGuid());
            incident["jarvis_registrationnumber"] = "123";
            incident["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["description"] = "This is for testing";
            incident["jarvis_driverlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incident["jarvis_translationstatusreportedfault"] = new OptionSetValue(334030000);
            incident["jarvis_translationstatuslocation"] = new OptionSetValue(334030000);
            incident["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["jarvis_translationstatuscustomerexpectations"] = new OptionSetValue(334030000);
            incident["jarvis_location"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["jarvis_customerexpectations"] = "test";
            incident["ownerid"] = new EntityReference { LogicalName = "system_user", Name = "test" };
            incident["jarvis_sourceid"] = 232323232;
            incident["jarvis_callerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incident["jarvis_callerrole"] = new OptionSetValue(3);
            incident["createdby"] = users;
            incident.Attributes["jarvis_ata"] = DateTime.Now;
            //// Setting Input Parameters.
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", incidentTarget),
               };
            this.PluginExecutionContext.MessageNameGet = () => "Update";
            this.PluginExecutionContext.StageGet = () => 40;
            //// Setting Post and Pre Image
            this.PluginExecutionContext.PostEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PostImage", incident },
                };
            };
            this.PluginExecutionContext.PreEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PreImage", incident },
                };
            };

            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();
                if (query.GetType().Name == "FetchExpression")
                {
                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_vasbreakdownprocess'>"))
                    {
                        Entity vasprocess = new Entity("jarvis_vasbreakdownprocess");
                        vasprocess["businessprocessflowinstanceid"] = Guid.NewGuid();
                        vasprocess["bpf_name"] = "test";
                        vasprocess["bpf_incidentid"] = Guid.NewGuid();
                        vasprocess["activestageid"] = Guid.NewGuid();
                        vasprocess["statecode"] = new OptionSetValue(0);
                        vasprocess["createdon"] = DateTime.UtcNow;
                        vasprocess["createdby"] = "test";
                        vasprocess.Id = Guid.NewGuid();
                        result.Entities.Add(vasprocess);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='incident'>"))
                    {
                        Entity incidentResult = new Entity("incident");
                        incidentResult[Constants.IncidentNature.IncidentIncidentNature_jarvisIincidentnatureid] = new AliasedValue("incidentNature", "incidentnatureid", Guid.NewGuid());
                        incidentResult["jarvis_hdrd"] = false;
                        incidentResult["jarvis_homedealer"] = new EntityReference("account", incident.Id);
                        incidentResult["CaseRD.jarvis_repairingdealer"] = new AliasedValue("account", "account", new EntityReference("account", incident.Id));
                        incidentResult.Id = Guid.NewGuid();
                        result.Entities.Add(incidentResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_casecontact'>"))
                    {
                        Entity caseContact = new Entity("jarvis_casecontact");
                        caseContact["jarvis_name"] = "testcontact";
                        caseContact["jarvis_preferredlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
                        caseContact["createdon"] = DateTime.UtcNow;
                        caseContact["jarvis_mobilephone"] = "9248428492";
                        caseContact["jarvis_role"] = new OptionSetValue(2);
                        caseContact.Id = Guid.NewGuid();
                        result.Entities.Add(caseContact);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_configurationjarvis'>"))
                    {
                        Entity configResult = new Entity("jarvis_configurationjarvis");
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationcasestatuschange"] = true;
                        configResult["createdon"] = DateTime.UtcNow;
                        configResult.Id = Guid.NewGuid();
                        result.Entities.Add(configResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_language'>"))
                    {
                        Entity language = new Entity("jarvis_language");
                        language.Id = Guid.NewGuid();
                        language["jarvis_iso3languagecode6392t"] = "DEU";
                        language["jarvis_iso2languagecode6391"] = "DE";
                        language["jarvis_name"] = "DE";
                        result.Entities.Add(language);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_casesoftoffer'>"))
                    {
                        Entity casesoftoffer = new Entity("jarvis_casesoftoffer");
                        casesoftoffer.Id = Guid.NewGuid();
                        casesoftoffer["jarvis_name"] = "DEU";
                        casesoftoffer["jarvis_contractno"] = "DE";
                        casesoftoffer["jarvis_description"] = "DE";
                        casesoftoffer["jarvis_startdate"] = DateTime.UtcNow;
                        casesoftoffer["jarvis_expirydate"] = DateTime.UtcNow;
                        casesoftoffer["statuscode"] = new OptionSetValue(0);
                        casesoftoffer["jarvis_softoffercodelookup"] = new EntityReference("jarvis_softoffer", Guid.NewGuid());
                        result.Entities.Add(casesoftoffer);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_softoffer'>"))
                    {
                        Entity softoffer = new Entity("jarvis_softoffer");
                        softoffer.Id = Guid.NewGuid();
                        softoffer["jarvis_name"] = "DEU";
                        softoffer["jarvis_softoffercode"] = new EntityReference("jarvis_softoffer", Guid.NewGuid());
                        softoffer["jarvis_contractno"] = "DE";
                        softoffer["jarvis_startdate"] = DateTime.UtcNow;
                        softoffer["jarvis_expirydate"] = DateTime.UtcNow;
                        softoffer["statuscode"] = new OptionSetValue(0);
                        softoffer["jarvis_marketcode"] = "DE";
                        softoffer["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
                        result.Entities.Add(softoffer);
                    }

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
                        passoutResult.Id = Guid.NewGuid();
                        result.Entities.Add(passoutResult);
                    }
                }

                return result;
            };

            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "incident")
                {
                    Entity inc = new Entity("incident");
                    inc.Id = guid;
                    inc["customerid"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_callerlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    inc["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    inc["jarvis_driverlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    inc["jarvis_translationstatusreportedfault"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatuslocation"] = new OptionSetValue(334030000);
                    inc["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    inc["jarvis_translationstatuscustomerexpectations"] = new OptionSetValue(334030000);
                    return inc;
                }

                if (entityName == "jarvis_vehicle")
                {
                    Entity client = new Entity("jarvis_vehicle");
                    client.Id = new Guid("6186f7f1-5f09-ed11-82e5-002248934e82");
                    client["jarvis_brandid"] = new EntityReference("jarvis_brand", Guid.NewGuid());
                    client["jarvis_fuelpowertype"] = new EntityReference("jarvis_vehiclefuelpowertype", Guid.NewGuid());
                    return client;
                }

                if (entityName == "jarvis_country")
                {
                    Entity country = new Entity("jarvis_country");
                    country.Id = guid;
                    country["jarvis_iso2countrycode"] = "NL";
                    return country;
                }

                if (entityName == "jarvis_language")
                {
                    Entity language = new Entity("jarvis_language");
                    language.Id = guid;
                    language["jarvis_iso3languagecode6392t"] = "DEU";
                    language["jarvis_iso2languagecode6391"] = "DE";
                    return language;
                }

                if (entityName == "account")
                {
                    Entity account = new Entity("account");
                    account.Id = guid;
                    account["jarvis_language"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    account["jarvis_address1_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    return account;
                }

                if (entityName == "jarvis_brand")
                {
                    Entity client = new Entity("jarvis_brand");
                    client.Id = new Guid("6186f7f1-5f09-ed11-82e5-002248934e82");
                    client["jarvis_mercuriusbrandcode"] = "ted";
                    return client;
                }

                return null;
            };
            this.Service.ExecuteOrganizationRequest = processInstanceRequest =>
            {
                if (processInstanceRequest.RequestName.Contains("RetrieveProcessInstances"))
                {
                    return new RetrieveProcessInstancesResponse()
                    {
                        Results = new ParameterCollection
                {
                    new KeyValuePair<string, object>("Processes", new EntityCollection(new List<Entity> { incident })),
                },
                    };
                }

                if (processInstanceRequest.RequestName.Contains("RetrieveActivePath"))
                {
                    Entity activepath = new Entity("activepath");
                    activepath["stagename"] = "Waiting For Repair Start";
                    activepath["processstageid"] = Guid.NewGuid();
                    return new RetrieveActivePathResponse()
                    {
                        Results = new ParameterCollection
                {
                    new KeyValuePair<string, object>("ProcessStages", new EntityCollection(new List<Entity> { activepath })),
                },
                    };
                }

                return null;
            };

            CasePostOperationSync plugin = new CasePostOperationSync();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// PreValidate Update With Complete Data Test Method with statuscode 10.
        /// </summary>
        [TestMethod]
        public void PostUpdateWithCompleteDataTestMethodStCode10()
        {
            Entity incidentTarget = new Entity("incident");

            EntityReference users = new EntityReference("systemuser", Guid.NewGuid());
            users.Name = "MERCURIUS";

            incidentTarget.Id = Guid.Empty;

            incidentTarget["statuscode"] = new OptionSetValue(10);
            incidentTarget["incidentid"] = Guid.NewGuid();
            incidentTarget["caseorigincode"] = new OptionSetValue(2);
            incidentTarget["casetypecode"] = new OptionSetValue(3);
            incidentTarget["jarvis_mercuriusstatus"] = new OptionSetValue(200);
            incidentTarget["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
            incidentTarget["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["customerid"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["jarvis_registrationnumber"] = "123";
            incidentTarget["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incidentTarget["description"] = "This is for testing";
            incidentTarget["jarvis_location"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incidentTarget["jarvis_customerexpectations"] = "test";
            incidentTarget["jarvis_incidentnature"] = "[{\"id\":\"3efba61a-85ad-44b7-a187-7f54acaf7a67\"}]";
            incidentTarget["jarvis_callerrole"] = new OptionSetValue(3);
            incidentTarget["jarvis_callerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incidentTarget["isescalated"] = true;
            incidentTarget["jarvis_querydescription"] = "test";
            incidentTarget["jarvis_extrainformationonsolutionquery"] = "test";
            incidentTarget["jarvis_translationstatusalertdecision"] = new OptionSetValue(334030000);
            incidentTarget["jarvis_querydecision"] = "test";
            incidentTarget.Attributes["jarvis_eta"] = DateTime.Now;
            incidentTarget.Attributes["modifiedby"] = users;

            Entity incident = new Entity("incident");
            incident.Id = Guid.Empty;
            incident["statuscode"] = new OptionSetValue(10);
            incident["incidentid"] = new EntityReference("incident", Guid.NewGuid());
            incident["caseorigincode"] = new OptionSetValue(2);
            incident["casetypecode"] = new OptionSetValue(3);
            incident["jarvis_mercuriusstatus"] = new OptionSetValue(200);
            incident["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
            incident["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incident["customerid"] = new EntityReference("account", Guid.NewGuid());
            incident["jarvis_registrationnumber"] = "123";
            incident["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["description"] = "This is for testing";
            incident["jarvis_driverlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incident["jarvis_translationstatusreportedfault"] = new OptionSetValue(334030000);
            incident["jarvis_translationstatuslocation"] = new OptionSetValue(334030000);
            incident["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["jarvis_translationstatuscustomerexpectations"] = new OptionSetValue(334030000);
            incident["jarvis_location"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["jarvis_customerexpectations"] = "test";
            incident["ownerid"] = new EntityReference { LogicalName = "system_user", Name = "MERCURIUS" };
            incident["jarvis_sourceid"] = 232323232;
            incident["jarvis_callerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incident["jarvis_callerrole"] = new OptionSetValue(3);
            incident["createdby"] = users;
            incident.Attributes["jarvis_etc"] = DateTime.Now;
            incident["parentcaseid"] = new EntityReference("incident", Guid.NewGuid());
            incident["jarvis_translationstatusalertdescription"] = new OptionSetValue(334030000);
            incident["jarvis_translationstatusalertextrainformation"] = new OptionSetValue(334030000);
            incident["jarvis_translationstatusalertdecision"] = new OptionSetValue(334030000);
            incident["jarvis_extrainformationonsolutionquery"] = "test";
            incident["jarvis_querydecision"] = "test";
            incident["modifiedby"] = users;
            //// Setting Input Parameters.
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", incidentTarget),
               };
            this.PluginExecutionContext.MessageNameGet = () => "Update";
            this.PluginExecutionContext.StageGet = () => 40;
            //// Setting Post and Pre Image
            this.PluginExecutionContext.PostEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PostImage", incident },
                };
            };
            this.PluginExecutionContext.PreEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PreImage", incident },
                };
            };

            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();
                if (query.GetType().Name == "FetchExpression")
                {
                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_vasbreakdownprocess'>"))
                    {
                        Entity vasprocess = new Entity("jarvis_vasbreakdownprocess");
                        vasprocess["businessprocessflowinstanceid"] = Guid.NewGuid();
                        vasprocess["bpf_name"] = "test";
                        vasprocess["bpf_incidentid"] = Guid.NewGuid();
                        vasprocess["activestageid"] = Guid.NewGuid();
                        vasprocess["statecode"] = new OptionSetValue(0);
                        vasprocess["createdon"] = DateTime.UtcNow;
                        vasprocess["createdby"] = "test";
                        vasprocess.Id = Guid.NewGuid();
                        result.Entities.Add(vasprocess);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='incident'>"))
                    {
                        Entity incidentResult = new Entity("incident");
                        incidentResult[Constants.IncidentNature.IncidentIncidentNature_jarvisIincidentnatureid] = new AliasedValue("incidentNature", "incidentnatureid", Guid.NewGuid());
                        incidentResult["jarvis_hdrd"] = false;
                        incidentResult["jarvis_homedealer"] = new EntityReference("account", incident.Id);
                        incidentResult["CaseRD.jarvis_repairingdealer"] = new AliasedValue("account", "account", new EntityReference("account", incident.Id));
                        incidentResult.Id = Guid.NewGuid();
                        result.Entities.Add(incidentResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_configurationjarvis'>"))
                    {
                        Entity configResult = new Entity("jarvis_configurationjarvis");
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationcasestatuschange"] = true;
                        configResult["createdon"] = DateTime.UtcNow;
                        configResult.Id = Guid.NewGuid();
                        result.Entities.Add(configResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_language'>"))
                    {
                        Entity language = new Entity("jarvis_language");
                        language.Id = Guid.NewGuid();
                        language["jarvis_iso3languagecode6392t"] = "DEU";
                        language["jarvis_iso2languagecode6391"] = "DE";
                        language["jarvis_name"] = "DE";
                        result.Entities.Add(language);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_casesoftoffer'>"))
                    {
                        Entity casesoftoffer = new Entity("jarvis_casesoftoffer");
                        casesoftoffer.Id = Guid.NewGuid();
                        casesoftoffer["jarvis_name"] = "DEU";
                        casesoftoffer["jarvis_contractno"] = "DE";
                        casesoftoffer["jarvis_description"] = "DE";
                        casesoftoffer["jarvis_startdate"] = DateTime.UtcNow;
                        casesoftoffer["jarvis_expirydate"] = DateTime.UtcNow;
                        casesoftoffer["statuscode"] = new OptionSetValue(0);
                        casesoftoffer["jarvis_softoffercodelookup"] = new EntityReference("jarvis_softoffer", Guid.NewGuid());
                        result.Entities.Add(casesoftoffer);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_softoffer'>"))
                    {
                        Entity softoffer = new Entity("jarvis_softoffer");
                        softoffer.Id = Guid.NewGuid();
                        softoffer["jarvis_name"] = "DEU";
                        softoffer["jarvis_softoffercode"] = new EntityReference("jarvis_softoffer", Guid.NewGuid());
                        softoffer["jarvis_contractno"] = "DE";
                        softoffer["jarvis_startdate"] = DateTime.UtcNow;
                        softoffer["jarvis_expirydate"] = DateTime.UtcNow;
                        softoffer["statuscode"] = new OptionSetValue(0);
                        softoffer["jarvis_marketcode"] = "DE";
                        softoffer["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
                        result.Entities.Add(softoffer);
                    }

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
                        passoutResult.Id = Guid.NewGuid();
                        result.Entities.Add(passoutResult);
                    }
                }

                return result;
            };

            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "incident")
                {
                    Entity inc = new Entity("incident");
                    inc.Id = guid;
                    inc["customerid"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_callerlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    inc["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    inc["jarvis_driverlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    inc["jarvis_translationstatusreportedfault"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatuslocation"] = new OptionSetValue(334030000);
                    inc["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    inc["jarvis_translationstatuscustomerexpectations"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatusalertdescription"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatusalertextrainformation"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatusalertdecision"] = new OptionSetValue(334030000);
                    return inc;
                }

                if (entityName == "jarvis_vehicle")
                {
                    Entity client = new Entity("jarvis_vehicle");
                    client.Id = new Guid("6186f7f1-5f09-ed11-82e5-002248934e82");
                    client["jarvis_brandid"] = new EntityReference("jarvis_brand", Guid.NewGuid());
                    client["jarvis_fuelpowertype"] = new EntityReference("jarvis_vehiclefuelpowertype", Guid.NewGuid());
                    return client;
                }

                if (entityName == "jarvis_country")
                {
                    Entity country = new Entity("jarvis_country");
                    country.Id = guid;
                    country["jarvis_iso2countrycode"] = "NL";
                    return country;
                }

                if (entityName == "jarvis_language")
                {
                    Entity language = new Entity("jarvis_language");
                    language.Id = guid;
                    language["jarvis_iso3languagecode6392t"] = "DEU";
                    language["jarvis_iso2languagecode6391"] = "DE";
                    return language;
                }

                if (entityName == "account")
                {
                    Entity account = new Entity("account");
                    account.Id = guid;
                    account["jarvis_language"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    account["jarvis_address1_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    return account;
                }

                if (entityName == "jarvis_brand")
                {
                    Entity client = new Entity("jarvis_brand");
                    client.Id = new Guid("6186f7f1-5f09-ed11-82e5-002248934e82");
                    client["jarvis_mercuriusbrandcode"] = "ted";
                    return client;
                }

                return null;
            };
            this.Service.ExecuteOrganizationRequest = processInstanceRequest =>
            {
                if (processInstanceRequest.RequestName.Contains("RetrieveProcessInstances"))
                {
                    return new RetrieveProcessInstancesResponse()
                    {
                        Results = new ParameterCollection
                {
                    new KeyValuePair<string, object>("Processes", new EntityCollection(new List<Entity> { incident })),
                },
                    };
                }

                if (processInstanceRequest.RequestName.Contains("RetrieveActivePath"))
                {
                    Entity activepath = new Entity("activepath");
                    activepath["stagename"] = "Waiting For Repair Start";
                    activepath["processstageid"] = Guid.NewGuid();
                    return new RetrieveActivePathResponse()
                    {
                        Results = new ParameterCollection
                {
                    new KeyValuePair<string, object>("ProcessStages", new EntityCollection(new List<Entity> { activepath })),
                },
                    };
                }

                return null;
            };

            CasePostOperationSync plugin = new CasePostOperationSync();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// PreValidate Update With Complete Data Test Method with statuscode 10 and caller role 4.
        /// </summary>
        [TestMethod]
        public void PostUpdateWithCompleteDataTestMethodStCode10Role4()
        {
            Entity incidentTarget = new Entity("incident");

            EntityReference users = new EntityReference("systemuser", Guid.NewGuid());
            users.Name = "MERCURIUS";

            incidentTarget.Id = Guid.Empty;

            incidentTarget["statuscode"] = new OptionSetValue(10);
            incidentTarget["incidentid"] = Guid.NewGuid();
            incidentTarget["caseorigincode"] = new OptionSetValue(2);
            incidentTarget["casetypecode"] = new OptionSetValue(2);
            incidentTarget["jarvis_mercuriusstatus"] = new OptionSetValue(200);
            incidentTarget["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
            incidentTarget["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["customerid"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["jarvis_registrationnumber"] = "123";
            incidentTarget["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incidentTarget["description"] = "This is for testing";
            incidentTarget["jarvis_location"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incidentTarget["jarvis_customerexpectations"] = "test";
            incidentTarget["jarvis_incidentnature"] = "[{\"id\":\"3efba61a-85ad-44b7-a187-7f54acaf7a67\"}]";
            incidentTarget["jarvis_callerrole"] = new OptionSetValue(4);
            incidentTarget["jarvis_callerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incidentTarget["isescalated"] = true;
            incidentTarget["jarvis_querydescription"] = "test";
            incidentTarget["jarvis_extrainformationonsolutionquery"] = "test";
            incidentTarget["jarvis_translationstatusalertdecision"] = new OptionSetValue(334030000);
            incidentTarget["jarvis_querydecision"] = "test";
            incidentTarget.Attributes["jarvis_eta"] = DateTime.Now;
            incidentTarget["modifiedby"] = users;

            Entity incident = new Entity("incident");
            incident.Id = Guid.Empty;
            incident["statuscode"] = new OptionSetValue(10);
            incident["incidentid"] = new EntityReference("incident", Guid.NewGuid());
            incident["caseorigincode"] = new OptionSetValue(2);
            incident["casetypecode"] = new OptionSetValue(2);
            incident["jarvis_mercuriusstatus"] = new OptionSetValue(200);
            incident["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
            incident["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incident["customerid"] = new EntityReference("account", Guid.NewGuid());
            incident["jarvis_registrationnumber"] = "123";
            incident["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["description"] = "This is for testing";
            incident["jarvis_driverlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incident["jarvis_translationstatusreportedfault"] = new OptionSetValue(334030000);
            incident["jarvis_translationstatuslocation"] = new OptionSetValue(334030000);
            incident["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["jarvis_translationstatuscustomerexpectations"] = new OptionSetValue(334030000);
            incident["jarvis_location"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["jarvis_customerexpectations"] = "test";
            incident["ownerid"] = new EntityReference { LogicalName = "system_user", Name = "MERCURIUS" };
            incident["jarvis_sourceid"] = 232323232;
            incident["jarvis_callerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incident["jarvis_callerrole"] = new OptionSetValue(4);
            incident["createdby"] = users;
            incident.Attributes["jarvis_etc"] = DateTime.Now;
            incident["parentcaseid"] = new EntityReference("incident", Guid.NewGuid());
            incident["jarvis_translationstatusalertdescription"] = new OptionSetValue(334030000);
            incident["jarvis_translationstatusalertextrainformation"] = new OptionSetValue(334030000);
            incident["jarvis_translationstatusalertdecision"] = new OptionSetValue(334030000);
            incident["jarvis_extrainformationonsolutionquery"] = "test";
            incident["jarvis_querydecision"] = "test";
            incident["modifiedby"] = users;
            //// Setting Input Parameters.
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", incidentTarget),
               };
            this.PluginExecutionContext.MessageNameGet = () => "Update";
            this.PluginExecutionContext.StageGet = () => 40;
            //// Setting Post and Pre Image
            this.PluginExecutionContext.PostEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PostImage", incident },
                };
            };
            this.PluginExecutionContext.PreEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PreImage", incident },
                };
            };

            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();
                if (query.GetType().Name == "FetchExpression")
                {
                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_vasbreakdownprocess'>"))
                    {
                        Entity vasprocess = new Entity("jarvis_vasbreakdownprocess");
                        vasprocess["businessprocessflowinstanceid"] = Guid.NewGuid();
                        vasprocess["bpf_name"] = "test";
                        vasprocess["bpf_incidentid"] = Guid.NewGuid();
                        vasprocess["activestageid"] = Guid.NewGuid();
                        vasprocess["statecode"] = new OptionSetValue(0);
                        vasprocess["createdon"] = DateTime.UtcNow;
                        vasprocess["createdby"] = "test";
                        vasprocess.Id = Guid.NewGuid();
                        result.Entities.Add(vasprocess);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='incident'>"))
                    {
                        Entity incidentResult = new Entity("incident");
                        incidentResult[Constants.IncidentNature.IncidentIncidentNature_jarvisIincidentnatureid] = new AliasedValue("incidentNature", "incidentnatureid", Guid.NewGuid());
                        incidentResult["jarvis_hdrd"] = false;
                        incidentResult["jarvis_homedealer"] = new EntityReference("account", incident.Id);
                        incidentResult["CaseRD.jarvis_repairingdealer"] = new AliasedValue("account", "account", new EntityReference("account", incident.Id));
                        incidentResult.Id = Guid.NewGuid();
                        result.Entities.Add(incidentResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_configurationjarvis'>"))
                    {
                        Entity configResult = new Entity("jarvis_configurationjarvis");
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationcasestatuschange"] = true;
                        configResult["createdon"] = DateTime.UtcNow;
                        configResult.Id = Guid.NewGuid();
                        result.Entities.Add(configResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_language'>"))
                    {
                        Entity language = new Entity("jarvis_language");
                        language.Id = Guid.NewGuid();
                        language["jarvis_iso3languagecode6392t"] = "DEU";
                        language["jarvis_iso2languagecode6391"] = "DE";
                        language["jarvis_name"] = "DE";
                        result.Entities.Add(language);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_casesoftoffer'>"))
                    {
                        Entity casesoftoffer = new Entity("jarvis_casesoftoffer");
                        casesoftoffer.Id = Guid.NewGuid();
                        casesoftoffer["jarvis_name"] = "DEU";
                        casesoftoffer["jarvis_contractno"] = "DE";
                        casesoftoffer["jarvis_description"] = "DE";
                        casesoftoffer["jarvis_startdate"] = DateTime.UtcNow;
                        casesoftoffer["jarvis_expirydate"] = DateTime.UtcNow;
                        casesoftoffer["statuscode"] = new OptionSetValue(0);
                        casesoftoffer["jarvis_softoffercodelookup"] = new EntityReference("jarvis_softoffer", Guid.NewGuid());
                        result.Entities.Add(casesoftoffer);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_softoffer'>"))
                    {
                        Entity softoffer = new Entity("jarvis_softoffer");
                        softoffer.Id = Guid.NewGuid();
                        softoffer["jarvis_name"] = "DEU";
                        softoffer["jarvis_softoffercode"] = new EntityReference("jarvis_softoffer", Guid.NewGuid());
                        softoffer["jarvis_contractno"] = "DE";
                        softoffer["jarvis_startdate"] = DateTime.UtcNow;
                        softoffer["jarvis_expirydate"] = DateTime.UtcNow;
                        softoffer["statuscode"] = new OptionSetValue(0);
                        softoffer["jarvis_marketcode"] = "DE";
                        softoffer["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
                        result.Entities.Add(softoffer);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='contact'>"))
                    {
                        Entity caseContact = new Entity("contact");
                        caseContact["jarvis_name"] = "testcontact";
                        caseContact["firstname"] = "testcontact";
                        caseContact["jarvis_language"] = new EntityReference("jarvis_language", Guid.NewGuid());
                        caseContact["createdon"] = DateTime.UtcNow;
                        caseContact["telephone1"] = "9248428492";
                        caseContact["jarvis_role"] = new OptionSetValue(2);
                        caseContact["parentcustomerid"] = new EntityReference("account", incident.Id);
                        caseContact.Id = Guid.NewGuid();
                        caseContact.Attributes["preferredcontactmethodcode"] = new OptionSetValue(1);
                        result.Entities.Add(caseContact);
                    }

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
                        passoutResult.Id = Guid.NewGuid();
                        result.Entities.Add(passoutResult);
                    }
                }

                if (query.GetType().Name == "QueryExpression")
                {
                    if ((query as QueryExpression).EntityName.Contains("jarvis_casecontact"))
                    {
                        Entity caseContact = new Entity("jarvis_casecontact");
                        caseContact["jarvis_name"] = "testcontact";
                        caseContact["jarvis_preferredlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
                        caseContact["createdon"] = DateTime.UtcNow;
                        caseContact["jarvis_mobilephone"] = "9248428492";
                        caseContact["jarvis_role"] = new OptionSetValue(2);
                        caseContact["jarvis_casecontactid"] = new EntityReference("jarvis_casecontact", Guid.NewGuid());
                        caseContact["jarvis_iscustomercontact"] = true;
                        caseContact.Id = Guid.NewGuid();
                        result.Entities.Add(caseContact);
                    }
                }

                return result;
            };

            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "incident")
                {
                    Entity inc = new Entity("incident");
                    inc.Id = guid;
                    inc["customerid"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_callerlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    inc["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    inc["jarvis_driverlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    inc["jarvis_translationstatusreportedfault"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatuslocation"] = new OptionSetValue(334030000);
                    inc["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    inc["jarvis_translationstatuscustomerexpectations"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatusalertdescription"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatusalertextrainformation"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatusalertdecision"] = new OptionSetValue(334030000);
                    return inc;
                }

                if (entityName == "jarvis_vehicle")
                {
                    Entity client = new Entity("jarvis_vehicle");
                    client.Id = new Guid("6186f7f1-5f09-ed11-82e5-002248934e82");
                    client["jarvis_brandid"] = new EntityReference("jarvis_brand", Guid.NewGuid());
                    client["jarvis_fuelpowertype"] = new EntityReference("jarvis_vehiclefuelpowertype", Guid.NewGuid());
                    return client;
                }

                if (entityName == "jarvis_country")
                {
                    Entity country = new Entity("jarvis_country");
                    country.Id = guid;
                    country["jarvis_iso2countrycode"] = "NL";
                    return country;
                }

                if (entityName == "jarvis_language")
                {
                    Entity language = new Entity("jarvis_language");
                    language.Id = guid;
                    language["jarvis_iso3languagecode6392t"] = "DEU";
                    language["jarvis_iso2languagecode6391"] = "DE";
                    return language;
                }

                if (entityName == "account")
                {
                    Entity account = new Entity("account");
                    account.Id = guid;
                    account["jarvis_language"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    account["jarvis_address1_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    return account;
                }

                if (entityName == "jarvis_brand")
                {
                    Entity client = new Entity("jarvis_brand");
                    client.Id = new Guid("6186f7f1-5f09-ed11-82e5-002248934e82");
                    client["jarvis_mercuriusbrandcode"] = "ted";
                    return client;
                }

                return null;
            };
            this.Service.ExecuteOrganizationRequest = processInstanceRequest =>
            {
                if (processInstanceRequest.RequestName.Contains("RetrieveProcessInstances"))
                {
                    return new RetrieveProcessInstancesResponse()
                    {
                        Results = new ParameterCollection
                {
                    new KeyValuePair<string, object>("Processes", new EntityCollection(new List<Entity> { incident })),
                },
                    };
                }

                if (processInstanceRequest.RequestName.Contains("RetrieveActivePath"))
                {
                    Entity activepath = new Entity("activepath");
                    activepath["stagename"] = "Waiting For Repair Start";
                    activepath["processstageid"] = Guid.NewGuid();
                    return new RetrieveActivePathResponse()
                    {
                        Results = new ParameterCollection
                {
                    new KeyValuePair<string, object>("ProcessStages", new EntityCollection(new List<Entity> { activepath })),
                },
                    };
                }

                return null;
            };

            CasePostOperationSync plugin = new CasePostOperationSync();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// PreValidate Update With Complete Data Test Method with statuscode 10.
        /// </summary>
        [TestMethod]
        public void PostUpdateWithCompleteDataTestMethodStCode10withCaseType()
        {
            Entity incidentTarget = new Entity("incident");

            EntityReference users = new EntityReference("systemuser", Guid.NewGuid());
            users.Name = "MERCURIUS";

            incidentTarget.Id = Guid.Empty;

            incidentTarget["statuscode"] = new OptionSetValue(10);
            incidentTarget["incidentid"] = Guid.NewGuid();
            incidentTarget["caseorigincode"] = new OptionSetValue(2);
            incidentTarget["casetypecode"] = new OptionSetValue(4);
            incidentTarget["jarvis_mercuriusstatus"] = new OptionSetValue(200);
            incidentTarget["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
            incidentTarget["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["customerid"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["jarvis_registrationnumber"] = "123";
            incidentTarget["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incidentTarget["description"] = "This is for testing";
            incidentTarget["jarvis_location"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incidentTarget["jarvis_customerexpectations"] = "test";
            incidentTarget["jarvis_incidentnature"] = "[{\"id\":\"3efba61a-85ad-44b7-a187-7f54acaf7a67\"}]";
            incidentTarget["jarvis_callerrole"] = new OptionSetValue(4);
            incidentTarget["jarvis_callerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incidentTarget["isescalated"] = true;
            incidentTarget["jarvis_querydescription"] = "test";
            incidentTarget["jarvis_extrainformationonsolutionquery"] = "test";
            incidentTarget["jarvis_translationstatusalertdecision"] = new OptionSetValue(334030000);
            incidentTarget["jarvis_querydecision"] = "test";
            incidentTarget.Attributes["jarvis_eta"] = DateTime.Now;
            incidentTarget["modifiedby"] = users;

            Entity incident = new Entity("incident");
            incident.Id = Guid.Empty;
            incident["statuscode"] = new OptionSetValue(10);
            incident["incidentid"] = new EntityReference("incident", Guid.NewGuid());
            incident["caseorigincode"] = new OptionSetValue(2);
            incident["casetypecode"] = new OptionSetValue(4);
            incident["jarvis_mercuriusstatus"] = new OptionSetValue(200);
            incident["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
            incident["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incident["customerid"] = new EntityReference("account", Guid.NewGuid());
            incident["jarvis_registrationnumber"] = "123";
            incident["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["description"] = "This is for testing";
            incident["jarvis_driverlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incident["jarvis_translationstatusreportedfault"] = new OptionSetValue(334030000);
            incident["jarvis_translationstatuslocation"] = new OptionSetValue(334030000);
            incident["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["jarvis_translationstatuscustomerexpectations"] = new OptionSetValue(334030000);
            incident["jarvis_location"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["jarvis_customerexpectations"] = "test";
            incident["ownerid"] = new EntityReference { LogicalName = "system_user", Name = "MERCURIUS" };
            incident["jarvis_sourceid"] = 232323232;
            incident["jarvis_callerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incident["jarvis_callerrole"] = new OptionSetValue(4);
            incident["createdby"] = users;
            incident.Attributes["jarvis_etc"] = DateTime.Now;
            incident["parentcaseid"] = new EntityReference("incident", Guid.NewGuid());
            incident["jarvis_translationstatusalertdescription"] = new OptionSetValue(334030000);
            incident["jarvis_translationstatusalertextrainformation"] = new OptionSetValue(334030000);
            incident["jarvis_translationstatusalertdecision"] = new OptionSetValue(334030000);
            incident["jarvis_extrainformationonsolutionquery"] = "test";
            incident["jarvis_querydecision"] = "test";
            //// Setting Input Parameters.
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", incidentTarget),
               };
            this.PluginExecutionContext.MessageNameGet = () => "Update";
            this.PluginExecutionContext.StageGet = () => 40;
            //// Setting Post and Pre Image
            this.PluginExecutionContext.PostEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PostImage", incident },
                };
            };
            this.PluginExecutionContext.PreEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PreImage", incident },
                };
            };

            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();
                if (query.GetType().Name == "FetchExpression")
                {
                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_vasbreakdownprocess'>"))
                    {
                        Entity vasprocess = new Entity("jarvis_vasbreakdownprocess");
                        vasprocess["businessprocessflowinstanceid"] = Guid.NewGuid();
                        vasprocess["bpf_name"] = "test";
                        vasprocess["bpf_incidentid"] = Guid.NewGuid();
                        vasprocess["activestageid"] = Guid.NewGuid();
                        vasprocess["statecode"] = new OptionSetValue(0);
                        vasprocess["createdon"] = DateTime.UtcNow;
                        vasprocess["createdby"] = "test";
                        vasprocess.Id = Guid.NewGuid();
                        result.Entities.Add(vasprocess);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='incident'>"))
                    {
                        Entity incidentResult = new Entity("incident");
                        incidentResult[Constants.IncidentNature.IncidentIncidentNature_jarvisIincidentnatureid] = new AliasedValue("incidentNature", "incidentnatureid", Guid.NewGuid());
                        incidentResult["jarvis_hdrd"] = false;
                        incidentResult["jarvis_homedealer"] = new EntityReference("account", incident.Id);
                        incidentResult["CaseRD.jarvis_repairingdealer"] = new AliasedValue("account", "account", new EntityReference("account", incident.Id));
                        incidentResult.Id = Guid.NewGuid();
                        result.Entities.Add(incidentResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_configurationjarvis'>"))
                    {
                        Entity configResult = new Entity("jarvis_configurationjarvis");
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationcasestatuschange"] = true;
                        configResult["createdon"] = DateTime.UtcNow;
                        configResult.Id = Guid.NewGuid();
                        result.Entities.Add(configResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_language'>"))
                    {
                        Entity language = new Entity("jarvis_language");
                        language.Id = Guid.NewGuid();
                        language["jarvis_iso3languagecode6392t"] = "DEU";
                        language["jarvis_iso2languagecode6391"] = "DE";
                        language["jarvis_name"] = "DE";
                        result.Entities.Add(language);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='contact'>"))
                    {
                        Entity caseContact = new Entity("contact");
                        caseContact["jarvis_name"] = "testcontact";
                        caseContact["jarvis_firstname"] = "testcontact";
                        caseContact["jarvis_language"] = new EntityReference("jarvis_language", Guid.NewGuid());
                        caseContact["createdon"] = DateTime.UtcNow;
                        caseContact["telephone1"] = "9248428492";
                        caseContact["jarvis_role"] = new OptionSetValue(2);
                        caseContact["parentcustomerid"] = new EntityReference("account", incident.Id);
                        caseContact.Id = Guid.NewGuid();
                        caseContact.Attributes["preferredcontactmethodcode"] = null;
                        result.Entities.Add(caseContact);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_casesoftoffer'>"))
                    {
                        Entity casesoftoffer = new Entity("jarvis_casesoftoffer");
                        casesoftoffer.Id = Guid.NewGuid();
                        casesoftoffer["jarvis_name"] = "DEU";
                        casesoftoffer["jarvis_contractno"] = "DE";
                        casesoftoffer["jarvis_description"] = "DE";
                        casesoftoffer["jarvis_startdate"] = DateTime.UtcNow;
                        casesoftoffer["jarvis_expirydate"] = DateTime.UtcNow;
                        casesoftoffer["statuscode"] = new OptionSetValue(0);
                        casesoftoffer["jarvis_softoffercodelookup"] = new EntityReference("jarvis_softoffer", Guid.NewGuid());
                        result.Entities.Add(casesoftoffer);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_softoffer'>"))
                    {
                        Entity softoffer = new Entity("jarvis_softoffer");
                        softoffer.Id = Guid.NewGuid();
                        softoffer["jarvis_name"] = "DEU";
                        softoffer["jarvis_softoffercode"] = new EntityReference("jarvis_softoffer", Guid.NewGuid());
                        softoffer["jarvis_contractno"] = "DE";
                        softoffer["jarvis_startdate"] = DateTime.UtcNow;
                        softoffer["jarvis_expirydate"] = DateTime.UtcNow;
                        softoffer["statuscode"] = new OptionSetValue(0);
                        softoffer["jarvis_marketcode"] = "DE";
                        softoffer["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
                        result.Entities.Add(softoffer);
                    }

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
                        passoutResult.Id = Guid.NewGuid();
                        result.Entities.Add(passoutResult);
                    }
                }

                return result;
            };

            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "incident")
                {
                    Entity inc = new Entity("incident");
                    inc.Id = guid;
                    inc["customerid"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_callerlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    inc["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    inc["jarvis_driverlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    inc["jarvis_translationstatusreportedfault"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatuslocation"] = new OptionSetValue(334030000);
                    inc["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    inc["jarvis_translationstatuscustomerexpectations"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatusalertdescription"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatusalertextrainformation"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatusalertdecision"] = new OptionSetValue(334030000);
                    return inc;
                }

                if (entityName == "jarvis_vehicle")
                {
                    Entity client = new Entity("jarvis_vehicle");
                    client.Id = new Guid("6186f7f1-5f09-ed11-82e5-002248934e82");
                    client["jarvis_brandid"] = new EntityReference("jarvis_brand", Guid.NewGuid());
                    client["jarvis_fuelpowertype"] = new EntityReference("jarvis_vehiclefuelpowertype", Guid.NewGuid());
                    return client;
                }

                if (entityName == "jarvis_country")
                {
                    Entity country = new Entity("jarvis_country");
                    country.Id = guid;
                    country["jarvis_iso2countrycode"] = "NL";
                    return country;
                }

                if (entityName == "jarvis_language")
                {
                    Entity language = new Entity("jarvis_language");
                    language.Id = guid;
                    language["jarvis_iso3languagecode6392t"] = "DEU";
                    language["jarvis_iso2languagecode6391"] = "DE";
                    return language;
                }

                if (entityName == "account")
                {
                    Entity account = new Entity("account");
                    account.Id = guid;
                    account["jarvis_language"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    account["jarvis_address1_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    return account;
                }

                if (entityName == "jarvis_brand")
                {
                    Entity client = new Entity("jarvis_brand");
                    client.Id = new Guid("6186f7f1-5f09-ed11-82e5-002248934e82");
                    client["jarvis_mercuriusbrandcode"] = "ted";
                    return client;
                }

                return null;
            };
            this.Service.ExecuteOrganizationRequest = processInstanceRequest =>
            {
                if (processInstanceRequest.RequestName.Contains("RetrieveProcessInstances"))
                {
                    return new RetrieveProcessInstancesResponse()
                    {
                        Results = new ParameterCollection
                {
                    new KeyValuePair<string, object>("Processes", new EntityCollection(new List<Entity> { incident })),
                },
                    };
                }

                if (processInstanceRequest.RequestName.Contains("RetrieveActivePath"))
                {
                    Entity activepath = new Entity("activepath");
                    activepath["stagename"] = "Waiting For Repair Start";
                    activepath["processstageid"] = Guid.NewGuid();
                    return new RetrieveActivePathResponse()
                    {
                        Results = new ParameterCollection
                {
                    new KeyValuePair<string, object>("ProcessStages", new EntityCollection(new List<Entity> { activepath })),
                },
                    };
                }

                return null;
            };

            CasePostOperationSync plugin = new CasePostOperationSync();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// PreValidate Update With Complete Data Test Method with statuscode 30.
        /// </summary>
        [TestMethod]
        public void PostUpdateWithCompleteDataTestMethodStCode30()
        {
            Entity incidentTarget = new Entity("incident");

            EntityReference users = new EntityReference("systemuser", Guid.NewGuid());
            users.Name = "MERCURIUS";

            incidentTarget.Id = Guid.Empty;

            incidentTarget["statuscode"] = new OptionSetValue(30);
            incidentTarget["incidentid"] = Guid.NewGuid();
            incidentTarget["caseorigincode"] = new OptionSetValue(2);
            incidentTarget["casetypecode"] = new OptionSetValue(3);
            incidentTarget["jarvis_mercuriusstatus"] = new OptionSetValue(200);
            incidentTarget["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
            incidentTarget["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["customerid"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["jarvis_registrationnumber"] = "123";
            incidentTarget["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incidentTarget["description"] = "This is for testing";
            incidentTarget["jarvis_location"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incidentTarget["jarvis_customerexpectations"] = "test";
            incidentTarget["jarvis_incidentnature"] = "[{\"id\":\"3efba61a-85ad-44b7-a187-7f54acaf7a67\"}]";
            incidentTarget["jarvis_callerrole"] = new OptionSetValue(4);
            incidentTarget["jarvis_callerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incidentTarget["isescalated"] = true;
            incidentTarget["jarvis_querydescription"] = "test";
            incidentTarget["jarvis_extrainformationonsolutionquery"] = "test";
            incidentTarget["jarvis_translationstatusalertdecision"] = new OptionSetValue(334030000);
            incidentTarget["jarvis_querydecision"] = "test";
            incidentTarget.Attributes["jarvis_eta"] = DateTime.Now;
            incidentTarget.Attributes["modifiedby"] = users;

            Entity incident = new Entity("incident");
            incident.Id = Guid.Empty;
            incident["statuscode"] = new OptionSetValue(30);
            incident["incidentid"] = new EntityReference("incident", Guid.NewGuid());
            incident["caseorigincode"] = new OptionSetValue(2);
            incident["casetypecode"] = new OptionSetValue(3);
            incident["jarvis_mercuriusstatus"] = new OptionSetValue(200);
            incident["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
            incident["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incident["customerid"] = new EntityReference("account", Guid.NewGuid());
            incident["jarvis_registrationnumber"] = "123";
            incident["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["description"] = "This is for testing";
            incident["jarvis_driverlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incident["jarvis_translationstatusreportedfault"] = new OptionSetValue(334030000);
            incident["jarvis_translationstatuslocation"] = new OptionSetValue(334030000);
            incident["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["jarvis_translationstatuscustomerexpectations"] = new OptionSetValue(334030000);
            incident["jarvis_location"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["jarvis_customerexpectations"] = "test";
            incident["ownerid"] = new EntityReference { LogicalName = "system_user", Name = "test" };
            incident["jarvis_sourceid"] = 232323232;
            incident["jarvis_callerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incident["jarvis_callerrole"] = new OptionSetValue(4);
            incident["createdby"] = users;
            incident.Attributes["jarvis_etc"] = DateTime.Now;
            incident["parentcaseid"] = new EntityReference("incident", Guid.NewGuid());
            incident["jarvis_translationstatusalertdescription"] = new OptionSetValue(334030000);
            incident["jarvis_translationstatusalertextrainformation"] = new OptionSetValue(334030000);
            incident["jarvis_translationstatusalertdecision"] = new OptionSetValue(334030000);
            incident["jarvis_extrainformationonsolutionquery"] = "test";
            incident["jarvis_querydecision"] = "test";
            incident["jarvis_onetimecustomerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incident["jarvis_onetimecustomercountry"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["jarvis_latitude"] = Convert.ToDecimal(11);
            incident["jarvis_longitude"] = Convert.ToDecimal(11);
            //// Setting Input Parameters.
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", incidentTarget),
               };
            this.PluginExecutionContext.MessageNameGet = () => "Update";
            this.PluginExecutionContext.StageGet = () => 40;
            //// Setting Post and Pre Image
            this.PluginExecutionContext.PostEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PostImage", incident },
                };
            };
            incident["statuscode"] = new OptionSetValue(20);
            this.PluginExecutionContext.PreEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PreImage", incident },
                };
            };

            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();
                if (query.GetType().Name == "FetchExpression")
                {
                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_vasbreakdownprocess'>"))
                    {
                        Entity vasprocess = new Entity("jarvis_vasbreakdownprocess");
                        vasprocess["businessprocessflowinstanceid"] = Guid.NewGuid();
                        vasprocess["bpf_name"] = "test";
                        vasprocess["bpf_incidentid"] = Guid.NewGuid();
                        vasprocess["activestageid"] = Guid.NewGuid();
                        vasprocess["statecode"] = new OptionSetValue(0);
                        vasprocess["createdon"] = DateTime.UtcNow;
                        vasprocess["createdby"] = "test";
                        vasprocess.Id = Guid.NewGuid();
                        result.Entities.Add(vasprocess);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='incident'>"))
                    {
                        Entity incidentResult = new Entity("incident");
                        incidentResult[Constants.IncidentNature.IncidentIncidentNature_jarvisIincidentnatureid] = new AliasedValue("incidentNature", "incidentnatureid", Guid.NewGuid());
                        incidentResult["jarvis_hdrd"] = false;
                        incidentResult["jarvis_homedealer"] = new EntityReference("account", incident.Id);
                        incidentResult["CaseRD.jarvis_repairingdealer"] = new AliasedValue("account", "account", new EntityReference("account", incident.Id));
                        incidentResult.Id = Guid.NewGuid();
                        result.Entities.Add(incidentResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_configurationjarvis'>"))
                    {
                        Entity configResult = new Entity("jarvis_configurationjarvis");
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationcasestatuschange"] = true;
                        configResult["createdon"] = DateTime.UtcNow;
                        configResult.Id = Guid.NewGuid();
                        result.Entities.Add(configResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_language'>"))
                    {
                        Entity language = new Entity("jarvis_language");
                        language.Id = Guid.NewGuid();
                        language["jarvis_iso3languagecode6392t"] = "DEU";
                        language["jarvis_iso2languagecode6391"] = "DE";
                        language["jarvis_name"] = "DE";
                        result.Entities.Add(language);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_casesoftoffer'>"))
                    {
                        Entity casesoftoffer = new Entity("jarvis_casesoftoffer");
                        casesoftoffer.Id = Guid.NewGuid();
                        casesoftoffer["jarvis_name"] = "DEU";
                        casesoftoffer["jarvis_contractno"] = "DE";
                        casesoftoffer["jarvis_description"] = "DE";
                        casesoftoffer["jarvis_startdate"] = DateTime.UtcNow;
                        casesoftoffer["jarvis_expirydate"] = DateTime.UtcNow;
                        casesoftoffer["statuscode"] = new OptionSetValue(0);
                        casesoftoffer["jarvis_softoffercodelookup"] = new EntityReference("jarvis_softoffer", Guid.NewGuid());
                        result.Entities.Add(casesoftoffer);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_softoffer'>"))
                    {
                        Entity softoffer = new Entity("jarvis_softoffer");
                        softoffer.Id = Guid.NewGuid();
                        softoffer["jarvis_name"] = "DEU";
                        softoffer["jarvis_softoffercode"] = new EntityReference("jarvis_softoffer", Guid.NewGuid());
                        softoffer["jarvis_contractno"] = "DE";
                        softoffer["jarvis_startdate"] = DateTime.UtcNow;
                        softoffer["jarvis_expirydate"] = DateTime.UtcNow;
                        softoffer["statuscode"] = new OptionSetValue(0);
                        softoffer["jarvis_marketcode"] = "DE";
                        softoffer["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
                        result.Entities.Add(softoffer);
                    }

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
                        passoutResult.Id = Guid.NewGuid();
                        result.Entities.Add(passoutResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='msdyn_workordertype'>"))
                    {
                        Entity workOrder = new Entity("msdyn_workordertype");
                        workOrder.Id = Guid.NewGuid();
                        workOrder["msdyn_name"] = "DEU";
                        workOrder["msdyn_incidentrequired"] = "DE";
                        workOrder["createdon"] = DateTime.UtcNow;
                        result.Entities.Add(workOrder);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='msdyn_incidenttype'>"))
                    {
                        Entity incidentType = new Entity("msdyn_incidenttype");
                        incidentType.Id = Guid.NewGuid();
                        incidentType["msdyn_name"] = "DEU";
                        incidentType["msdyn_estimatedduration"] = "12";
                        incidentType["msdyn_incidenttypeid"] = new EntityReference("incident", incident.Id);
                        incidentType["createdon"] = DateTime.UtcNow;
                        result.Entities.Add(incidentType);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_incidentnature'>"))
                    {
                        Entity incidentType = new Entity("jarvis_incidentnature");
                        incidentType.Id = Guid.NewGuid();
                        incidentType["jarvis_name"] = "DEU";
                        incidentType["jarvis_incidenttype"] = new EntityReference("incident", incident.Id);
                        incidentType["createdon"] = DateTime.UtcNow;
                        result.Entities.Add(incidentType);
                    }

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

            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "incident")
                {
                    Entity inc = new Entity("incident");
                    inc.Id = guid;
                    inc["customerid"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_callerlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    inc["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    inc["jarvis_driverlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    inc["jarvis_translationstatusreportedfault"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatuslocation"] = new OptionSetValue(334030000);
                    inc["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    inc["jarvis_translationstatuscustomerexpectations"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatusalertdescription"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatusalertextrainformation"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatusalertdecision"] = new OptionSetValue(334030000);
                    return inc;
                }

                if (entityName == "jarvis_vehicle")
                {
                    Entity client = new Entity("jarvis_vehicle");
                    client.Id = new Guid("6186f7f1-5f09-ed11-82e5-002248934e82");
                    client["jarvis_brandid"] = new EntityReference("jarvis_brand", Guid.NewGuid());
                    client["jarvis_fuelpowertype"] = new EntityReference("jarvis_vehiclefuelpowertype", Guid.NewGuid());
                    return client;
                }

                if (entityName == "jarvis_country")
                {
                    Entity country = new Entity("jarvis_country");
                    country.Id = guid;
                    country["jarvis_iso2countrycode"] = "NL";
                    return country;
                }

                if (entityName == "jarvis_language")
                {
                    Entity language = new Entity("jarvis_language");
                    language.Id = guid;
                    language["jarvis_iso3languagecode6392t"] = "DEU";
                    language["jarvis_iso2languagecode6391"] = "DE";
                    return language;
                }

                if (entityName == "account")
                {
                    Entity account = new Entity("account");
                    account.Id = guid;
                    account["jarvis_language"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    account["jarvis_address1_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    return account;
                }

                if (entityName == "jarvis_brand")
                {
                    Entity client = new Entity("jarvis_brand");
                    client.Id = new Guid("6186f7f1-5f09-ed11-82e5-002248934e82");
                    client["jarvis_mercuriusbrandcode"] = "ted";
                    return client;
                }

                return null;
            };
            this.Service.ExecuteOrganizationRequest = processInstanceRequest =>
            {
                if (processInstanceRequest.RequestName.Contains("RetrieveProcessInstances"))
                {
                    return new RetrieveProcessInstancesResponse()
                    {
                        Results = new ParameterCollection
                {
                    new KeyValuePair<string, object>("Processes", new EntityCollection(new List<Entity> { incident })),
                },
                    };
                }

                if (processInstanceRequest.RequestName.Contains("RetrieveActivePath"))
                {
                    Entity activepath = new Entity("activepath");
                    activepath["stagename"] = "Waiting For Repair Start";
                    activepath["processstageid"] = Guid.NewGuid();
                    return new RetrieveActivePathResponse()
                    {
                        Results = new ParameterCollection
                {
                    new KeyValuePair<string, object>("ProcessStages", new EntityCollection(new List<Entity> { activepath })),
                },
                    };
                }

                return null;
            };
            this.Service.CreateEntity = (entity) =>
            {
                return Guid.NewGuid();
            };
            CasePostOperationSync plugin = new CasePostOperationSync();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// PreValidate Update With Complete Data Test Method with statuscode 40.
        /// </summary>
        [TestMethod]
        public void PostUpdateWithCompleteDataTestMethodStCode40()
        {
            Entity incidentTarget = new Entity("incident");

            EntityReference users = new EntityReference("systemuser", Guid.NewGuid());
            users.Name = "MERCURIUS";

            incidentTarget.Id = Guid.Empty;

            incidentTarget["statuscode"] = new OptionSetValue(40);
            incidentTarget["incidentid"] = Guid.NewGuid();
            incidentTarget["caseorigincode"] = new OptionSetValue(2);
            incidentTarget["casetypecode"] = new OptionSetValue(2);
            incidentTarget["jarvis_mercuriusstatus"] = new OptionSetValue(200);
            incidentTarget["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
            incidentTarget["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["customerid"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["jarvis_registrationnumber"] = "123";
            incidentTarget["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incidentTarget["description"] = "This is for testing";
            incidentTarget["jarvis_location"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incidentTarget["jarvis_customerexpectations"] = "test";
            incidentTarget["jarvis_incidentnature"] = "[{\"id\":\"3efba61a-85ad-44b7-a187-7f54acaf7a67\"}]";
            incidentTarget["jarvis_callerrole"] = new OptionSetValue(4);
            incidentTarget["jarvis_callerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incidentTarget["isescalated"] = true;
            incidentTarget["jarvis_querydescription"] = "test";
            incidentTarget["jarvis_extrainformationonsolutionquery"] = "test";
            incidentTarget["jarvis_translationstatusalertdecision"] = new OptionSetValue(334030000);
            incidentTarget["jarvis_querydecision"] = "test";
            incidentTarget.Attributes["jarvis_eta"] = DateTime.Now;
            incidentTarget.Attributes["modifiedby"] = users;

            Entity incident = new Entity("incident");
            incident.Id = Guid.Empty;
            incident["statuscode"] = new OptionSetValue(40);
            incident["incidentid"] = new EntityReference("incident", Guid.NewGuid());
            incident["caseorigincode"] = new OptionSetValue(2);
            incident["casetypecode"] = new OptionSetValue(2);
            incident["jarvis_mercuriusstatus"] = new OptionSetValue(200);
            incident["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
            incident["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incident["customerid"] = new EntityReference("account", Guid.NewGuid());
            incident["jarvis_registrationnumber"] = "123";
            incident["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["description"] = "This is for testing";
            incident["jarvis_driverlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incident["jarvis_translationstatusreportedfault"] = new OptionSetValue(334030000);
            incident["jarvis_translationstatuslocation"] = new OptionSetValue(334030000);
            incident["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["jarvis_translationstatuscustomerexpectations"] = new OptionSetValue(334030000);
            incident["jarvis_location"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["jarvis_customerexpectations"] = "test";
            incident["ownerid"] = new EntityReference { LogicalName = "system_user", Name = "MERCURIUS" };
            incident["jarvis_sourceid"] = 232323232;
            incident["jarvis_callerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incident["jarvis_callerrole"] = new OptionSetValue(4);
            incident["createdby"] = users;
            incident["parentcaseid"] = new EntityReference("incident", Guid.NewGuid());
            incident["jarvis_translationstatusalertdescription"] = new OptionSetValue(334030000);
            incident["jarvis_translationstatusalertextrainformation"] = new OptionSetValue(334030000);
            incident["jarvis_translationstatusalertdecision"] = new OptionSetValue(334030000);
            incident["jarvis_extrainformationonsolutionquery"] = "test";
            incident["jarvis_querydecision"] = "test";
            incident["jarvis_onetimecustomerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incident["jarvis_onetimecustomercountry"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident.Attributes["jarvis_eta"] = DateTime.Now;
            //// Setting Input Parameters.
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", incidentTarget),
               };
            this.PluginExecutionContext.MessageNameGet = () => "Update";
            this.PluginExecutionContext.StageGet = () => 40;
            //// Setting Post and Pre Image
            this.PluginExecutionContext.PostEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PostImage", incident },
                };
            };
            this.PluginExecutionContext.PreEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PreImage", incident },
                };
            };

            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();
                if (query.GetType().Name == "FetchExpression")
                {
                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_vasbreakdownprocess'>"))
                    {
                        Entity vasprocess = new Entity("jarvis_vasbreakdownprocess");
                        vasprocess["businessprocessflowinstanceid"] = Guid.NewGuid();
                        vasprocess["bpf_name"] = "test";
                        vasprocess["bpf_incidentid"] = Guid.NewGuid();
                        vasprocess["activestageid"] = Guid.NewGuid();
                        vasprocess["statecode"] = new OptionSetValue(0);
                        vasprocess["createdon"] = DateTime.UtcNow;
                        vasprocess["createdby"] = "test";
                        vasprocess.Id = Guid.NewGuid();
                        result.Entities.Add(vasprocess);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='incident'>"))
                    {
                        Entity incidentResult = new Entity("incident");
                        incidentResult[Constants.IncidentNature.IncidentIncidentNature_jarvisIincidentnatureid] = new AliasedValue("incidentNature", "incidentnatureid", Guid.NewGuid());
                        incidentResult["jarvis_hdrd"] = true;
                        incidentResult["jarvis_homedealer"] = new EntityReference("account", incident.Id);
                        incidentResult["CaseRD.jarvis_repairingdealer"] = new AliasedValue("account", "account", new EntityReference("account", Guid.NewGuid()));
                        incidentResult.Id = Guid.NewGuid();
                        result.Entities.Add(incidentResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_configurationjarvis'>"))
                    {
                        Entity configResult = new Entity("jarvis_configurationjarvis");
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationcasestatuschange"] = true;
                        configResult["createdon"] = DateTime.UtcNow;
                        configResult.Id = Guid.NewGuid();
                        result.Entities.Add(configResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_language'>"))
                    {
                        Entity language = new Entity("jarvis_language");
                        language.Id = Guid.NewGuid();
                        language["jarvis_iso3languagecode6392t"] = "DEU";
                        language["jarvis_iso2languagecode6391"] = "DE";
                        language["jarvis_name"] = "DE";
                        result.Entities.Add(language);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_casesoftoffer'>"))
                    {
                        Entity casesoftoffer = new Entity("jarvis_casesoftoffer");
                        casesoftoffer.Id = Guid.NewGuid();
                        casesoftoffer["jarvis_name"] = "DEU";
                        casesoftoffer["jarvis_contractno"] = "DE";
                        casesoftoffer["jarvis_description"] = "DE";
                        casesoftoffer["jarvis_startdate"] = DateTime.UtcNow;
                        casesoftoffer["jarvis_expirydate"] = DateTime.UtcNow;
                        casesoftoffer["statuscode"] = new OptionSetValue(0);
                        casesoftoffer["jarvis_softoffercodelookup"] = new EntityReference("jarvis_softoffer", Guid.NewGuid());
                        result.Entities.Add(casesoftoffer);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_softoffer'>"))
                    {
                        Entity softoffer = new Entity("jarvis_softoffer");
                        softoffer.Id = Guid.NewGuid();
                        softoffer["jarvis_name"] = "DEU";
                        softoffer["jarvis_softoffercode"] = new EntityReference("jarvis_softoffer", Guid.NewGuid());
                        softoffer["jarvis_contractno"] = "DE";
                        softoffer["jarvis_startdate"] = DateTime.UtcNow;
                        softoffer["jarvis_expirydate"] = DateTime.UtcNow;
                        softoffer["statuscode"] = new OptionSetValue(0);
                        softoffer["jarvis_marketcode"] = "DE";
                        softoffer["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
                        result.Entities.Add(softoffer);
                    }

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
                        passoutResult.Id = Guid.NewGuid();
                        result.Entities.Add(passoutResult);
                    }
                }

                return result;
            };

            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "incident")
                {
                    Entity inc = new Entity("incident");
                    inc.Id = guid;
                    inc["customerid"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_callerlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    inc["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    inc["jarvis_driverlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    inc["jarvis_translationstatusreportedfault"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatuslocation"] = new OptionSetValue(334030000);
                    inc["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    inc["jarvis_translationstatuscustomerexpectations"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatusalertdescription"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatusalertextrainformation"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatusalertdecision"] = new OptionSetValue(334030000);
                    return inc;
                }

                if (entityName == "jarvis_vehicle")
                {
                    Entity client = new Entity("jarvis_vehicle");
                    client.Id = new Guid("6186f7f1-5f09-ed11-82e5-002248934e82");
                    client["jarvis_brandid"] = new EntityReference("jarvis_brand", Guid.NewGuid());
                    client["jarvis_fuelpowertype"] = new EntityReference("jarvis_vehiclefuelpowertype", Guid.NewGuid());
                    return client;
                }

                if (entityName == "jarvis_country")
                {
                    Entity country = new Entity("jarvis_country");
                    country.Id = guid;
                    country["jarvis_iso2countrycode"] = "NL";
                    return country;
                }

                if (entityName == "jarvis_language")
                {
                    Entity language = new Entity("jarvis_language");
                    language.Id = guid;
                    language["jarvis_iso3languagecode6392t"] = "DEU";
                    language["jarvis_iso2languagecode6391"] = "DE";
                    return language;
                }

                if (entityName == "account")
                {
                    Entity account = new Entity("account");
                    account.Id = guid;
                    account["jarvis_language"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    account["jarvis_address1_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    return account;
                }

                if (entityName == "jarvis_brand")
                {
                    Entity client = new Entity("jarvis_brand");
                    client.Id = new Guid("6186f7f1-5f09-ed11-82e5-002248934e82");
                    client["jarvis_mercuriusbrandcode"] = "ted";
                    return client;
                }

                return null;
            };
            this.Service.ExecuteOrganizationRequest = processInstanceRequest =>
            {
                if (processInstanceRequest.RequestName.Contains("RetrieveProcessInstances"))
                {
                    return new RetrieveProcessInstancesResponse()
                    {
                        Results = new ParameterCollection
                {
                    new KeyValuePair<string, object>("Processes", new EntityCollection(new List<Entity> { incident })),
                },
                    };
                }

                if (processInstanceRequest.RequestName.Contains("RetrieveActivePath"))
                {
                    Entity activepath = new Entity("activepath");
                    activepath["stagename"] = "Waiting For Repair Start";
                    activepath["processstageid"] = Guid.NewGuid();
                    return new RetrieveActivePathResponse()
                    {
                        Results = new ParameterCollection
                {
                    new KeyValuePair<string, object>("ProcessStages", new EntityCollection(new List<Entity> { activepath })),
                },
                    };
                }

                return null;
            };

            CasePostOperationSync plugin = new CasePostOperationSync();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// PreValidate Update With Complete Data Test Method with statuscode 80.
        /// </summary>
        [TestMethod]
        public void PostUpdateWithCompleteDataTestMethodStCode80()
        {
            Entity incidentTarget = new Entity("incident");

            EntityReference users = new EntityReference("systemuser", Guid.NewGuid());
            users.Name = "MERCURIUS";

            incidentTarget.Id = Guid.Empty;

            incidentTarget["statuscode"] = new OptionSetValue(80);
            incidentTarget["incidentid"] = Guid.NewGuid();
            incidentTarget["caseorigincode"] = new OptionSetValue(2);
            incidentTarget["casetypecode"] = new OptionSetValue(2);
            incidentTarget["jarvis_mercuriusstatus"] = new OptionSetValue(200);
            incidentTarget["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
            incidentTarget["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["customerid"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["jarvis_registrationnumber"] = "123";
            incidentTarget["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incidentTarget["description"] = "This is for testing";
            incidentTarget["jarvis_location"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incidentTarget["jarvis_customerexpectations"] = "test";
            incidentTarget["jarvis_incidentnature"] = "[{\"id\":\"3efba61a-85ad-44b7-a187-7f54acaf7a67\"}]";
            incidentTarget["jarvis_callerrole"] = new OptionSetValue(4);
            incidentTarget["jarvis_callerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incidentTarget["isescalated"] = true;
            incidentTarget["jarvis_querydescription"] = "test";
            incidentTarget["jarvis_extrainformationonsolutionquery"] = "test";
            incidentTarget["jarvis_translationstatusalertdecision"] = new OptionSetValue(334030000);
            incidentTarget["jarvis_querydecision"] = "test";
            incidentTarget.Attributes["jarvis_eta"] = DateTime.Now;
            incidentTarget.Attributes["jarvis_atc"] = DateTime.Now;
            incidentTarget.Attributes["modifiedby"] = users;

            Entity incident = new Entity("incident");
            incident.Id = Guid.Empty;
            incident["statuscode"] = new OptionSetValue(80);
            incident["incidentid"] = new EntityReference("incident", Guid.NewGuid());
            incident["caseorigincode"] = new OptionSetValue(2);
            incident["casetypecode"] = new OptionSetValue(2);
            incident["jarvis_mercuriusstatus"] = new OptionSetValue(200);
            incident["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
            incident["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incident["customerid"] = new EntityReference("account", Guid.NewGuid());
            incident["jarvis_registrationnumber"] = "123";
            incident["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["description"] = "This is for testing";
            incident["jarvis_driverlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incident["jarvis_translationstatusreportedfault"] = new OptionSetValue(334030000);
            incident["jarvis_translationstatuslocation"] = new OptionSetValue(334030000);
            incident["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["jarvis_translationstatuscustomerexpectations"] = new OptionSetValue(334030000);
            incident["jarvis_location"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["jarvis_customerexpectations"] = "test";
            incident["ownerid"] = new EntityReference { LogicalName = "system_user", Name = "MERCURIUS" };
            incident["jarvis_sourceid"] = 232323232;
            incident["jarvis_callerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incident["jarvis_callerrole"] = new OptionSetValue(4);
            incident["createdby"] = users;
            incident["parentcaseid"] = new EntityReference("incident", Guid.NewGuid());
            incident["jarvis_translationstatusalertdescription"] = new OptionSetValue(334030000);
            incident["jarvis_translationstatusalertextrainformation"] = new OptionSetValue(334030000);
            incident["jarvis_translationstatusalertdecision"] = new OptionSetValue(334030000);
            incident["jarvis_extrainformationonsolutionquery"] = "test";
            incident["jarvis_querydecision"] = "test";
            incident["jarvis_onetimecustomerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incident["jarvis_onetimecustomercountry"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["jarvis_actualcausefault"] = "test";
            incident["jarvis_mileageafterrepair"] = 10;
            incident["jarvis_mileageunitafterrepair"] = new EntityReference("jarvis_mileageunit", Guid.NewGuid());
            incident.Attributes["jarvis_atc"] = DateTime.Now;
            //// Setting Input Parameters.
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", incidentTarget),
               };
            this.PluginExecutionContext.MessageNameGet = () => "Update";
            this.PluginExecutionContext.StageGet = () => 40;
            //// Setting Post and Pre Image
            this.PluginExecutionContext.PostEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PostImage", incident },
                };
            };
            this.PluginExecutionContext.PreEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PreImage", incident },
                };
            };

            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();
                if (query.GetType().Name == "FetchExpression")
                {
                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_vasbreakdownprocess'>"))
                    {
                        Entity vasprocess = new Entity("jarvis_vasbreakdownprocess");
                        vasprocess["businessprocessflowinstanceid"] = Guid.NewGuid();
                        vasprocess["bpf_name"] = "test";
                        vasprocess["bpf_incidentid"] = Guid.NewGuid();
                        vasprocess["activestageid"] = Guid.NewGuid();
                        vasprocess["statecode"] = new OptionSetValue(0);
                        vasprocess["createdon"] = DateTime.UtcNow;
                        vasprocess["createdby"] = "test";
                        vasprocess.Id = Guid.NewGuid();
                        result.Entities.Add(vasprocess);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_repairinformation'>"))
                    {
                        Entity repairInfoResult = new Entity("jarvis_repairinformation");
                        repairInfoResult["jarvis_name"] = "testpassout";
                        repairInfoResult["createdon"] = DateTime.UtcNow;
                        repairInfoResult.Id = Guid.NewGuid();
                        result.Entities.Add(repairInfoResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='incident'>"))
                    {
                        Entity incidentResult = new Entity("incident");
                        incidentResult[Constants.IncidentNature.IncidentIncidentNature_jarvisIincidentnatureid] = new AliasedValue("incidentNature", "incidentnatureid", Guid.NewGuid());
                        incidentResult["jarvis_hdrd"] = true;
                        incidentResult["jarvis_homedealer"] = new EntityReference("account", incident.Id);
                        incidentResult["CaseRD.jarvis_repairingdealer"] = new AliasedValue("account", "account", new EntityReference("account", incident.Id));
                        incidentResult.Id = Guid.NewGuid();
                        result.Entities.Add(incidentResult);
                        result.Entities.Add(incidentResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_configurationjarvis'>"))
                    {
                        Entity configResult = new Entity("jarvis_configurationjarvis");
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationcasestatuschange"] = true;
                        configResult["createdon"] = DateTime.UtcNow;
                        configResult.Id = Guid.NewGuid();
                        result.Entities.Add(configResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_language'>"))
                    {
                        Entity language = new Entity("jarvis_language");
                        language.Id = Guid.NewGuid();
                        language["jarvis_iso3languagecode6392t"] = "DEU";
                        language["jarvis_iso2languagecode6391"] = "DE";
                        language["jarvis_name"] = "DE";
                        result.Entities.Add(language);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_casesoftoffer'>"))
                    {
                        Entity casesoftoffer = new Entity("jarvis_casesoftoffer");
                        casesoftoffer.Id = Guid.NewGuid();
                        casesoftoffer["jarvis_name"] = "DEU";
                        casesoftoffer["jarvis_contractno"] = "DE";
                        casesoftoffer["jarvis_description"] = "DE";
                        casesoftoffer["jarvis_startdate"] = DateTime.UtcNow;
                        casesoftoffer["jarvis_expirydate"] = DateTime.UtcNow;
                        casesoftoffer["statuscode"] = new OptionSetValue(0);
                        casesoftoffer["jarvis_softoffercodelookup"] = new EntityReference("jarvis_softoffer", Guid.NewGuid());
                        result.Entities.Add(casesoftoffer);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_softoffer'>"))
                    {
                        Entity softoffer = new Entity("jarvis_softoffer");
                        softoffer.Id = Guid.NewGuid();
                        softoffer["jarvis_name"] = "DEU";
                        softoffer["jarvis_softoffercode"] = new EntityReference("jarvis_softoffer", Guid.NewGuid());
                        softoffer["jarvis_contractno"] = "DE";
                        softoffer["jarvis_startdate"] = DateTime.UtcNow;
                        softoffer["jarvis_expirydate"] = DateTime.UtcNow;
                        softoffer["statuscode"] = new OptionSetValue(0);
                        softoffer["jarvis_marketcode"] = "DE";
                        softoffer["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
                        result.Entities.Add(softoffer);
                    }

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
                        passoutResult.Id = Guid.NewGuid();
                        result.Entities.Add(passoutResult);
                    }
                }

                return result;
            };

            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "incident")
                {
                    Entity inc = new Entity("incident");
                    inc.Id = guid;
                    inc["customerid"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_callerlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    inc["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    inc["jarvis_driverlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    inc["jarvis_translationstatusreportedfault"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatuslocation"] = new OptionSetValue(334030000);
                    inc["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    inc["jarvis_translationstatuscustomerexpectations"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatusalertdescription"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatusalertextrainformation"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatusalertdecision"] = new OptionSetValue(334030000);
                    return inc;
                }

                if (entityName == "jarvis_vehicle")
                {
                    Entity client = new Entity("jarvis_vehicle");
                    client.Id = new Guid("6186f7f1-5f09-ed11-82e5-002248934e82");
                    client["jarvis_brandid"] = new EntityReference("jarvis_brand", Guid.NewGuid());
                    client["jarvis_fuelpowertype"] = new EntityReference("jarvis_vehiclefuelpowertype", Guid.NewGuid());
                    return client;
                }

                if (entityName == "jarvis_country")
                {
                    Entity country = new Entity("jarvis_country");
                    country.Id = guid;
                    country["jarvis_iso2countrycode"] = "NL";
                    return country;
                }

                if (entityName == "jarvis_language")
                {
                    Entity language = new Entity("jarvis_language");
                    language.Id = guid;
                    language["jarvis_iso3languagecode6392t"] = "DEU";
                    language["jarvis_iso2languagecode6391"] = "DE";
                    return language;
                }

                if (entityName == "account")
                {
                    Entity account = new Entity("account");
                    account.Id = guid;
                    account["jarvis_language"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    account["jarvis_address1_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    return account;
                }

                if (entityName == "jarvis_brand")
                {
                    Entity client = new Entity("jarvis_brand");
                    client.Id = new Guid("6186f7f1-5f09-ed11-82e5-002248934e82");
                    client["jarvis_mercuriusbrandcode"] = "ted";
                    return client;
                }

                return null;
            };
            this.Service.ExecuteOrganizationRequest = processInstanceRequest =>
            {
                if (processInstanceRequest.RequestName.Contains("RetrieveProcessInstances"))
                {
                    return new RetrieveProcessInstancesResponse()
                    {
                        Results = new ParameterCollection
                {
                    new KeyValuePair<string, object>("Processes", new EntityCollection(new List<Entity> { incident })),
                },
                    };
                }

                if (processInstanceRequest.RequestName.Contains("RetrieveActivePath"))
                {
                    Entity activepath = new Entity("activepath");
                    activepath["stagename"] = "Waiting For Repair Start";
                    activepath["processstageid"] = Guid.NewGuid();
                    return new RetrieveActivePathResponse()
                    {
                        Results = new ParameterCollection
                {
                    new KeyValuePair<string, object>("ProcessStages", new EntityCollection(new List<Entity> { activepath })),
                },
                    };
                }

                return null;
            };

            CasePostOperationSync plugin = new CasePostOperationSync();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// PreValidate Update With Complete Data Test Method with statuscode 80.
        /// </summary>
        [TestMethod]
        public void PostUpdateWithCompleteDataTestMethodStCode80RD()
        {
            Entity incidentTarget = new Entity("incident");

            EntityReference users = new EntityReference("systemuser", Guid.NewGuid());
            users.Name = "MERCURIUS";

            incidentTarget.Id = Guid.Empty;

            incidentTarget["statuscode"] = new OptionSetValue(80);
            incidentTarget["incidentid"] = Guid.NewGuid();
            incidentTarget["caseorigincode"] = new OptionSetValue(2);
            incidentTarget["casetypecode"] = new OptionSetValue(2);
            incidentTarget["jarvis_mercuriusstatus"] = new OptionSetValue(200);
            incidentTarget["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
            incidentTarget["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["customerid"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["jarvis_registrationnumber"] = "123";
            incidentTarget["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incidentTarget["description"] = "This is for testing";
            incidentTarget["jarvis_location"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incidentTarget["jarvis_customerexpectations"] = "test";
            incidentTarget["jarvis_incidentnature"] = "[{\"id\":\"3efba61a-85ad-44b7-a187-7f54acaf7a67\"}]";
            incidentTarget["jarvis_callerrole"] = new OptionSetValue(4);
            incidentTarget["jarvis_callerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incidentTarget["isescalated"] = true;
            incidentTarget["jarvis_querydescription"] = "test";
            incidentTarget["jarvis_extrainformationonsolutionquery"] = "test";
            incidentTarget["jarvis_translationstatusalertdecision"] = new OptionSetValue(334030000);
            incidentTarget["jarvis_querydecision"] = "test";
            incidentTarget.Attributes["jarvis_eta"] = DateTime.Now;
            incidentTarget.Attributes["jarvis_atc"] = DateTime.Now;
            incidentTarget.Attributes["modifiedby"] = users;

            Entity incident = new Entity("incident");
            incident.Id = Guid.Empty;
            incident["statuscode"] = new OptionSetValue(80);
            incident["incidentid"] = new EntityReference("incident", Guid.NewGuid());
            incident["caseorigincode"] = new OptionSetValue(2);
            incident["casetypecode"] = new OptionSetValue(2);
            incident["jarvis_mercuriusstatus"] = new OptionSetValue(200);
            incident["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
            incident["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incident["customerid"] = new EntityReference("account", Guid.NewGuid());
            incident["jarvis_registrationnumber"] = "123";
            incident["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["description"] = "This is for testing";
            incident["jarvis_driverlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incident["jarvis_translationstatusreportedfault"] = new OptionSetValue(334030000);
            incident["jarvis_translationstatuslocation"] = new OptionSetValue(334030000);
            incident["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["jarvis_translationstatuscustomerexpectations"] = new OptionSetValue(334030000);
            incident["jarvis_location"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["jarvis_customerexpectations"] = "test";
            incident["ownerid"] = new EntityReference { LogicalName = "system_user", Name = "MERCURIUS" };
            incident["jarvis_sourceid"] = 232323232;
            incident["jarvis_callerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incident["jarvis_callerrole"] = new OptionSetValue(4);
            incident["createdby"] = users;
            incident["parentcaseid"] = new EntityReference("incident", Guid.NewGuid());
            incident["jarvis_translationstatusalertdescription"] = new OptionSetValue(334030000);
            incident["jarvis_translationstatusalertextrainformation"] = new OptionSetValue(334030000);
            incident["jarvis_translationstatusalertdecision"] = new OptionSetValue(334030000);
            incident["jarvis_extrainformationonsolutionquery"] = "test";
            incident["jarvis_querydecision"] = "test";
            incident["jarvis_onetimecustomerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incident["jarvis_onetimecustomercountry"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["jarvis_actualcausefault"] = "test";
            incident["jarvis_mileageafterrepair"] = 10;
            incident["jarvis_mileageunitafterrepair"] = new EntityReference("jarvis_mileageunit", Guid.NewGuid());
            incident.Attributes["jarvis_atc"] = DateTime.Now;
            //// Setting Input Parameters.
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", incidentTarget),
               };
            this.PluginExecutionContext.MessageNameGet = () => "Update";
            this.PluginExecutionContext.StageGet = () => 40;
            //// Setting Post and Pre Image
            this.PluginExecutionContext.PostEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PostImage", incident },
                };
            };
            this.PluginExecutionContext.PreEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PreImage", incident },
                };
            };

            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();
                if (query.GetType().Name == "FetchExpression")
                {
                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_vasbreakdownprocess'>"))
                    {
                        Entity vasprocess = new Entity("jarvis_vasbreakdownprocess");
                        vasprocess["businessprocessflowinstanceid"] = Guid.NewGuid();
                        vasprocess["bpf_name"] = "test";
                        vasprocess["bpf_incidentid"] = Guid.NewGuid();
                        vasprocess["activestageid"] = Guid.NewGuid();
                        vasprocess["statecode"] = new OptionSetValue(0);
                        vasprocess["createdon"] = DateTime.UtcNow;
                        vasprocess["createdby"] = "test";
                        vasprocess.Id = Guid.NewGuid();
                        result.Entities.Add(vasprocess);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_repairinformation'>"))
                    {
                        Entity repairInfoResult = new Entity("jarvis_repairinformation");
                        repairInfoResult["jarvis_name"] = "testpassout";
                        repairInfoResult["createdon"] = DateTime.UtcNow;
                        repairInfoResult.Id = Guid.NewGuid();
                        result.Entities.Add(repairInfoResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='incident'>"))
                    {
                        Entity incidentResult = new Entity("incident");
                        incidentResult[Constants.IncidentNature.IncidentIncidentNature_jarvisIincidentnatureid] = new AliasedValue("incidentNature", "incidentnatureid", Guid.NewGuid());
                        incidentResult["jarvis_hdrd"] = true;
                        incidentResult["jarvis_homedealer"] = new EntityReference("account", incident.Id);
                        incidentResult.Id = Guid.NewGuid();
                        result.Entities.Add(incidentResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_configurationjarvis'>"))
                    {
                        Entity configResult = new Entity("jarvis_configurationjarvis");
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationcasestatuschange"] = true;
                        configResult["createdon"] = DateTime.UtcNow;
                        configResult.Id = Guid.NewGuid();
                        result.Entities.Add(configResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_language'>"))
                    {
                        Entity language = new Entity("jarvis_language");
                        language.Id = Guid.NewGuid();
                        language["jarvis_iso3languagecode6392t"] = "DEU";
                        language["jarvis_iso2languagecode6391"] = "DE";
                        language["jarvis_name"] = "DE";
                        result.Entities.Add(language);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_casesoftoffer'>"))
                    {
                        Entity casesoftoffer = new Entity("jarvis_casesoftoffer");
                        casesoftoffer.Id = Guid.NewGuid();
                        casesoftoffer["jarvis_name"] = "DEU";
                        casesoftoffer["jarvis_contractno"] = "DE";
                        casesoftoffer["jarvis_description"] = "DE";
                        casesoftoffer["jarvis_startdate"] = DateTime.UtcNow;
                        casesoftoffer["jarvis_expirydate"] = DateTime.UtcNow;
                        casesoftoffer["statuscode"] = new OptionSetValue(0);
                        casesoftoffer["jarvis_softoffercodelookup"] = new EntityReference("jarvis_softoffer", Guid.NewGuid());
                        result.Entities.Add(casesoftoffer);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_softoffer'>"))
                    {
                        Entity softoffer = new Entity("jarvis_softoffer");
                        softoffer.Id = Guid.NewGuid();
                        softoffer["jarvis_name"] = "DEU";
                        softoffer["jarvis_softoffercode"] = new EntityReference("jarvis_softoffer", Guid.NewGuid());
                        softoffer["jarvis_contractno"] = "DE";
                        softoffer["jarvis_startdate"] = DateTime.UtcNow;
                        softoffer["jarvis_expirydate"] = DateTime.UtcNow;
                        softoffer["statuscode"] = new OptionSetValue(0);
                        softoffer["jarvis_marketcode"] = "DE";
                        softoffer["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
                        result.Entities.Add(softoffer);
                    }

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
                        passoutResult.Id = Guid.NewGuid();
                        result.Entities.Add(passoutResult);
                    }
                }

                return result;
            };

            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "incident")
                {
                    Entity inc = new Entity("incident");
                    inc.Id = guid;
                    inc["customerid"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_callerlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    inc["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    inc["jarvis_driverlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    inc["jarvis_translationstatusreportedfault"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatuslocation"] = new OptionSetValue(334030000);
                    inc["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    inc["jarvis_translationstatuscustomerexpectations"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatusalertdescription"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatusalertextrainformation"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatusalertdecision"] = new OptionSetValue(334030000);
                    return inc;
                }

                if (entityName == "jarvis_vehicle")
                {
                    Entity client = new Entity("jarvis_vehicle");
                    client.Id = new Guid("6186f7f1-5f09-ed11-82e5-002248934e82");
                    client["jarvis_brandid"] = new EntityReference("jarvis_brand", Guid.NewGuid());
                    client["jarvis_fuelpowertype"] = new EntityReference("jarvis_vehiclefuelpowertype", Guid.NewGuid());
                    return client;
                }

                if (entityName == "jarvis_country")
                {
                    Entity country = new Entity("jarvis_country");
                    country.Id = guid;
                    country["jarvis_iso2countrycode"] = "NL";
                    return country;
                }

                if (entityName == "jarvis_language")
                {
                    Entity language = new Entity("jarvis_language");
                    language.Id = guid;
                    language["jarvis_iso3languagecode6392t"] = "DEU";
                    language["jarvis_iso2languagecode6391"] = "DE";
                    return language;
                }

                if (entityName == "account")
                {
                    Entity account = new Entity("account");
                    account.Id = guid;
                    account["jarvis_language"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    account["jarvis_address1_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    return account;
                }

                if (entityName == "jarvis_brand")
                {
                    Entity client = new Entity("jarvis_brand");
                    client.Id = new Guid("6186f7f1-5f09-ed11-82e5-002248934e82");
                    client["jarvis_mercuriusbrandcode"] = "ted";
                    return client;
                }

                return null;
            };
            this.Service.ExecuteOrganizationRequest = processInstanceRequest =>
            {
                if (processInstanceRequest.RequestName.Contains("RetrieveProcessInstances"))
                {
                    return new RetrieveProcessInstancesResponse()
                    {
                        Results = new ParameterCollection
                {
                    new KeyValuePair<string, object>("Processes", new EntityCollection(new List<Entity> { incident })),
                },
                    };
                }

                if (processInstanceRequest.RequestName.Contains("RetrieveActivePath"))
                {
                    Entity activepath = new Entity("activepath");
                    activepath["stagename"] = "Waiting For Repair Start";
                    activepath["processstageid"] = Guid.NewGuid();
                    return new RetrieveActivePathResponse()
                    {
                        Results = new ParameterCollection
                {
                    new KeyValuePair<string, object>("ProcessStages", new EntityCollection(new List<Entity> { activepath })),
                },
                    };
                }

                return null;
            };

            CasePostOperationSync plugin = new CasePostOperationSync();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// PreValidate Update With Complete Data Test Method with statuscode 80 with no passout.
        /// </summary>
        [TestMethod]
        public void PostUpdateWithCompleteDataTestMethodStCode80NoPassout()
        {
            Entity incidentTarget = new Entity("incident");

            EntityReference users = new EntityReference("systemuser", Guid.NewGuid());
            users.Name = "MERCURIUS";

            incidentTarget.Id = Guid.Empty;

            incidentTarget["statuscode"] = new OptionSetValue(80);
            incidentTarget["incidentid"] = Guid.NewGuid();
            incidentTarget["caseorigincode"] = new OptionSetValue(2);
            incidentTarget["casetypecode"] = new OptionSetValue(2);
            incidentTarget["jarvis_mercuriusstatus"] = new OptionSetValue(200);
            incidentTarget["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
            incidentTarget["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["customerid"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["jarvis_registrationnumber"] = "123";
            incidentTarget["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incidentTarget["description"] = "This is for testing";
            incidentTarget["jarvis_location"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incidentTarget["jarvis_customerexpectations"] = "test";
            incidentTarget["jarvis_incidentnature"] = "[{\"id\":\"3efba61a-85ad-44b7-a187-7f54acaf7a67\"}]";
            incidentTarget["jarvis_callerrole"] = new OptionSetValue(4);
            incidentTarget["jarvis_callerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incidentTarget["isescalated"] = true;
            incidentTarget["jarvis_querydescription"] = "test";
            incidentTarget["jarvis_extrainformationonsolutionquery"] = "test";
            incidentTarget["jarvis_translationstatusalertdecision"] = new OptionSetValue(334030000);
            incidentTarget["jarvis_querydecision"] = "test";
            incidentTarget.Attributes["jarvis_eta"] = DateTime.Now;
            incidentTarget.Attributes["jarvis_atc"] = DateTime.Now;
            incidentTarget.Attributes["modifiedby"] = users;

            Entity incident = new Entity("incident");
            incident.Id = Guid.Empty;
            incident["statuscode"] = new OptionSetValue(80);
            incident["incidentid"] = new EntityReference("incident", Guid.NewGuid());
            incident["caseorigincode"] = new OptionSetValue(2);
            incident["casetypecode"] = new OptionSetValue(2);
            incident["jarvis_mercuriusstatus"] = new OptionSetValue(200);
            incident["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
            incident["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incident["customerid"] = new EntityReference("account", Guid.NewGuid());
            incident["jarvis_registrationnumber"] = "123";
            incident["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["description"] = "This is for testing";
            incident["jarvis_driverlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incident["jarvis_translationstatusreportedfault"] = new OptionSetValue(334030000);
            incident["jarvis_translationstatuslocation"] = new OptionSetValue(334030000);
            incident["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["jarvis_translationstatuscustomerexpectations"] = new OptionSetValue(334030000);
            incident["jarvis_location"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["jarvis_customerexpectations"] = "test";
            incident["ownerid"] = new EntityReference { LogicalName = "system_user", Name = "MERCURIUS" };
            incident["jarvis_sourceid"] = 232323232;
            incident["jarvis_callerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incident["jarvis_callerrole"] = new OptionSetValue(4);
            incident["createdby"] = users;
            incident["parentcaseid"] = new EntityReference("incident", Guid.NewGuid());
            incident["jarvis_translationstatusalertdescription"] = new OptionSetValue(334030000);
            incident["jarvis_translationstatusalertextrainformation"] = new OptionSetValue(334030000);
            incident["jarvis_translationstatusalertdecision"] = new OptionSetValue(334030000);
            incident["jarvis_extrainformationonsolutionquery"] = "test";
            incident["jarvis_querydecision"] = "test";
            incident["jarvis_onetimecustomerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incident["jarvis_onetimecustomercountry"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["jarvis_actualcausefault"] = "test";
            incident["jarvis_mileageafterrepair"] = 10;
            incident["jarvis_mileageunitafterrepair"] = new EntityReference("jarvis_mileageunit", Guid.NewGuid());
            incident.Attributes["jarvis_atc"] = DateTime.Now;
            //// Setting Input Parameters.
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", incidentTarget),
               };
            this.PluginExecutionContext.MessageNameGet = () => "Update";
            this.PluginExecutionContext.StageGet = () => 40;
            //// Setting Post and Pre Image
            this.PluginExecutionContext.PostEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PostImage", incident },
                };
            };
            this.PluginExecutionContext.PreEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PreImage", incident },
                };
            };

            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();
                if (query.GetType().Name == "FetchExpression")
                {
                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_vasbreakdownprocess'>"))
                    {
                        Entity vasprocess = new Entity("jarvis_vasbreakdownprocess");
                        vasprocess["businessprocessflowinstanceid"] = Guid.NewGuid();
                        vasprocess["bpf_name"] = "test";
                        vasprocess["bpf_incidentid"] = Guid.NewGuid();
                        vasprocess["activestageid"] = Guid.NewGuid();
                        vasprocess["statecode"] = new OptionSetValue(0);
                        vasprocess["createdon"] = DateTime.UtcNow;
                        vasprocess["createdby"] = "test";
                        vasprocess.Id = Guid.NewGuid();
                        result.Entities.Add(vasprocess);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='incident'>"))
                    {
                        Entity incidentResult = new Entity("incident");
                        incidentResult[Constants.IncidentNature.IncidentIncidentNature_jarvisIincidentnatureid] = new AliasedValue("incidentNature", "incidentnatureid", Guid.NewGuid());
                        incidentResult["jarvis_hdrd"] = false;
                        incidentResult["jarvis_homedealer"] = new EntityReference("account", incident.Id);
                        incidentResult["CaseRD.jarvis_repairingdealer"] = new AliasedValue("account", "account", new EntityReference("account", incident.Id));
                        incidentResult.Id = Guid.NewGuid();
                        result.Entities.Add(incidentResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_configurationjarvis'>"))
                    {
                        Entity configResult = new Entity("jarvis_configurationjarvis");
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationcasestatuschange"] = true;
                        configResult["createdon"] = DateTime.UtcNow;
                        configResult.Id = Guid.NewGuid();
                        result.Entities.Add(configResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_language'>"))
                    {
                        Entity language = new Entity("jarvis_language");
                        language.Id = Guid.NewGuid();
                        language["jarvis_iso3languagecode6392t"] = "DEU";
                        language["jarvis_iso2languagecode6391"] = "DE";
                        language["jarvis_name"] = "DE";
                        result.Entities.Add(language);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_casesoftoffer'>"))
                    {
                        Entity casesoftoffer = new Entity("jarvis_casesoftoffer");
                        casesoftoffer.Id = Guid.NewGuid();
                        casesoftoffer["jarvis_name"] = "DEU";
                        casesoftoffer["jarvis_contractno"] = "DE";
                        casesoftoffer["jarvis_description"] = "DE";
                        casesoftoffer["jarvis_startdate"] = DateTime.UtcNow;
                        casesoftoffer["jarvis_expirydate"] = DateTime.UtcNow;
                        casesoftoffer["statuscode"] = new OptionSetValue(0);
                        casesoftoffer["jarvis_softoffercodelookup"] = new EntityReference("jarvis_softoffer", Guid.NewGuid());
                        result.Entities.Add(casesoftoffer);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_softoffer'>"))
                    {
                        Entity softoffer = new Entity("jarvis_softoffer");
                        softoffer.Id = Guid.NewGuid();
                        softoffer["jarvis_name"] = "DEU";
                        softoffer["jarvis_softoffercode"] = new EntityReference("jarvis_softoffer", Guid.NewGuid());
                        softoffer["jarvis_contractno"] = "DE";
                        softoffer["jarvis_startdate"] = DateTime.UtcNow;
                        softoffer["jarvis_expirydate"] = DateTime.UtcNow;
                        softoffer["statuscode"] = new OptionSetValue(0);
                        softoffer["jarvis_marketcode"] = "DE";
                        softoffer["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
                        result.Entities.Add(softoffer);
                    }
                }

                return result;
            };

            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "incident")
                {
                    Entity inc = new Entity("incident");
                    inc.Id = guid;
                    inc["customerid"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_callerlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    inc["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    inc["jarvis_driverlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    inc["jarvis_translationstatusreportedfault"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatuslocation"] = new OptionSetValue(334030000);
                    inc["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    inc["jarvis_translationstatuscustomerexpectations"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatusalertdescription"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatusalertextrainformation"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatusalertdecision"] = new OptionSetValue(334030000);
                    return inc;
                }

                if (entityName == "jarvis_vehicle")
                {
                    Entity client = new Entity("jarvis_vehicle");
                    client.Id = new Guid("6186f7f1-5f09-ed11-82e5-002248934e82");
                    client["jarvis_brandid"] = new EntityReference("jarvis_brand", Guid.NewGuid());
                    client["jarvis_fuelpowertype"] = new EntityReference("jarvis_vehiclefuelpowertype", Guid.NewGuid());
                    return client;
                }

                if (entityName == "jarvis_country")
                {
                    Entity country = new Entity("jarvis_country");
                    country.Id = guid;
                    country["jarvis_iso2countrycode"] = "NL";
                    return country;
                }

                if (entityName == "jarvis_language")
                {
                    Entity language = new Entity("jarvis_language");
                    language.Id = guid;
                    language["jarvis_iso3languagecode6392t"] = "DEU";
                    language["jarvis_iso2languagecode6391"] = "DE";
                    return language;
                }

                if (entityName == "account")
                {
                    Entity account = new Entity("account");
                    account.Id = guid;
                    account["jarvis_language"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    account["jarvis_address1_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    return account;
                }

                if (entityName == "jarvis_brand")
                {
                    Entity client = new Entity("jarvis_brand");
                    client.Id = new Guid("6186f7f1-5f09-ed11-82e5-002248934e82");
                    client["jarvis_mercuriusbrandcode"] = "ted";
                    return client;
                }

                return null;
            };
            this.Service.ExecuteOrganizationRequest = processInstanceRequest =>
            {
                if (processInstanceRequest.RequestName.Contains("RetrieveProcessInstances"))
                {
                    return new RetrieveProcessInstancesResponse()
                    {
                        Results = new ParameterCollection
                {
                    new KeyValuePair<string, object>("Processes", new EntityCollection(new List<Entity> { incident })),
                },
                    };
                }

                if (processInstanceRequest.RequestName.Contains("RetrieveActivePath"))
                {
                    Entity activepath = new Entity("activepath");
                    activepath["stagename"] = "Waiting For Repair Start";
                    activepath["processstageid"] = Guid.NewGuid();
                    return new RetrieveActivePathResponse()
                    {
                        Results = new ParameterCollection
                {
                    new KeyValuePair<string, object>("ProcessStages", new EntityCollection(new List<Entity> { activepath })),
                },
                    };
                }

                return null;
            };

            CasePostOperationSync plugin = new CasePostOperationSync();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// PreValidate Update With Complete Data Test Method with statuscode 80 with no Atc.
        /// </summary>
        [TestMethod]
        public void PostUpdateWithCompleteDataTestMethodStCode80NoAtc()
        {
            Entity incidentTarget = new Entity("incident");

            EntityReference users = new EntityReference("systemuser", Guid.NewGuid());
            users.Name = "MERCURIUS";

            incidentTarget.Id = Guid.Empty;

            incidentTarget["statuscode"] = new OptionSetValue(80);
            incidentTarget["incidentid"] = Guid.NewGuid();
            incidentTarget["caseorigincode"] = new OptionSetValue(2);
            incidentTarget["casetypecode"] = new OptionSetValue(2);
            incidentTarget["jarvis_mercuriusstatus"] = new OptionSetValue(200);
            incidentTarget["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
            incidentTarget["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["customerid"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["jarvis_registrationnumber"] = "123";
            incidentTarget["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incidentTarget["description"] = "This is for testing";
            incidentTarget["jarvis_location"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incidentTarget["jarvis_customerexpectations"] = "test";
            incidentTarget["jarvis_incidentnature"] = "[{\"id\":\"3efba61a-85ad-44b7-a187-7f54acaf7a67\"}]";
            incidentTarget["jarvis_callerrole"] = new OptionSetValue(4);
            incidentTarget["jarvis_callerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incidentTarget["isescalated"] = true;
            incidentTarget["jarvis_querydescription"] = "test";
            incidentTarget["jarvis_extrainformationonsolutionquery"] = "test";
            incidentTarget["jarvis_translationstatusalertdecision"] = new OptionSetValue(334030000);
            incidentTarget["jarvis_querydecision"] = "test";
            incidentTarget.Attributes["jarvis_eta"] = DateTime.Now;
            incidentTarget.Attributes["jarvis_atc"] = DateTime.Now;
            incidentTarget.Attributes["modifiedby"] = users;

            Entity incident = new Entity("incident");
            incident.Id = Guid.Empty;
            incident["statuscode"] = new OptionSetValue(80);
            incident["incidentid"] = new EntityReference("incident", Guid.NewGuid());
            incident["caseorigincode"] = new OptionSetValue(2);
            incident["casetypecode"] = new OptionSetValue(2);
            incident["jarvis_mercuriusstatus"] = new OptionSetValue(200);
            incident["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
            incident["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incident["customerid"] = new EntityReference("account", Guid.NewGuid());
            incident["jarvis_registrationnumber"] = "123";
            incident["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["description"] = "This is for testing";
            incident["jarvis_driverlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incident["jarvis_translationstatusreportedfault"] = new OptionSetValue(334030000);
            incident["jarvis_translationstatuslocation"] = new OptionSetValue(334030000);
            incident["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["jarvis_translationstatuscustomerexpectations"] = new OptionSetValue(334030000);
            incident["jarvis_location"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["jarvis_customerexpectations"] = "test";
            incident["ownerid"] = new EntityReference { LogicalName = "system_user", Name = "MERCURIUS" };
            incident["jarvis_sourceid"] = 232323232;
            incident["jarvis_callerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incident["jarvis_callerrole"] = new OptionSetValue(4);
            incident["createdby"] = users;
            incident["parentcaseid"] = new EntityReference("incident", Guid.NewGuid());
            incident["jarvis_translationstatusalertdescription"] = new OptionSetValue(334030000);
            incident["jarvis_translationstatusalertextrainformation"] = new OptionSetValue(334030000);
            incident["jarvis_translationstatusalertdecision"] = new OptionSetValue(334030000);
            incident["jarvis_extrainformationonsolutionquery"] = "test";
            incident["jarvis_querydecision"] = "test";
            incident["jarvis_onetimecustomerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incident["jarvis_onetimecustomercountry"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["jarvis_actualcausefault"] = "test";
            incident["jarvis_mileageafterrepair"] = 10;
            incident["jarvis_mileageunitafterrepair"] = new EntityReference("jarvis_mileageunit", Guid.NewGuid());
            //// Setting Input Parameters.
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", incidentTarget),
               };
            this.PluginExecutionContext.MessageNameGet = () => "Update";
            this.PluginExecutionContext.StageGet = () => 40;
            //// Setting Post and Pre Image
            this.PluginExecutionContext.PostEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PostImage", incident },
                };
            };
            incident["statuscode"] = new OptionSetValue(70);
            this.PluginExecutionContext.PreEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PreImage", incident },
                };
            };

            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();
                if (query.GetType().Name == "FetchExpression")
                {
                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_vasbreakdownprocess'>"))
                    {
                        Entity vasprocess = new Entity("jarvis_vasbreakdownprocess");
                        vasprocess["businessprocessflowinstanceid"] = Guid.NewGuid();
                        vasprocess["bpf_name"] = "test";
                        vasprocess["bpf_incidentid"] = Guid.NewGuid();
                        vasprocess["activestageid"] = Guid.NewGuid();
                        vasprocess["statecode"] = new OptionSetValue(0);
                        vasprocess["createdon"] = DateTime.UtcNow;
                        vasprocess["createdby"] = "test";
                        vasprocess.Id = Guid.NewGuid();
                        result.Entities.Add(vasprocess);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='incident'>"))
                    {
                        Entity incidentResult = new Entity("incident");
                        incidentResult[Constants.IncidentNature.IncidentIncidentNature_jarvisIincidentnatureid] = new AliasedValue("incidentNature", "incidentnatureid", Guid.NewGuid());
                        incidentResult["jarvis_hdrd"] = false;
                        incidentResult["jarvis_homedealer"] = new EntityReference("account", incident.Id);
                        incidentResult["CaseRD.jarvis_repairingdealer"] = new AliasedValue("account", "account", new EntityReference("account", incident.Id));
                        incidentResult.Id = Guid.NewGuid();
                        result.Entities.Add(incidentResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_configurationjarvis'>"))
                    {
                        Entity configResult = new Entity("jarvis_configurationjarvis");
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationcasestatuschange"] = true;
                        configResult["createdon"] = DateTime.UtcNow;
                        configResult.Id = Guid.NewGuid();
                        result.Entities.Add(configResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_language'>"))
                    {
                        Entity language = new Entity("jarvis_language");
                        language.Id = Guid.NewGuid();
                        language["jarvis_iso3languagecode6392t"] = "DEU";
                        language["jarvis_iso2languagecode6391"] = "DE";
                        language["jarvis_name"] = "DE";
                        result.Entities.Add(language);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_casesoftoffer'>"))
                    {
                        Entity casesoftoffer = new Entity("jarvis_casesoftoffer");
                        casesoftoffer.Id = Guid.NewGuid();
                        casesoftoffer["jarvis_name"] = "DEU";
                        casesoftoffer["jarvis_contractno"] = "DE";
                        casesoftoffer["jarvis_description"] = "DE";
                        casesoftoffer["jarvis_startdate"] = DateTime.UtcNow;
                        casesoftoffer["jarvis_expirydate"] = DateTime.UtcNow;
                        casesoftoffer["statuscode"] = new OptionSetValue(0);
                        casesoftoffer["jarvis_softoffercodelookup"] = new EntityReference("jarvis_softoffer", Guid.NewGuid());
                        result.Entities.Add(casesoftoffer);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_softoffer'>"))
                    {
                        Entity softoffer = new Entity("jarvis_softoffer");
                        softoffer.Id = Guid.NewGuid();
                        softoffer["jarvis_name"] = "DEU";
                        softoffer["jarvis_softoffercode"] = new EntityReference("jarvis_softoffer", Guid.NewGuid());
                        softoffer["jarvis_contractno"] = "DE";
                        softoffer["jarvis_startdate"] = DateTime.UtcNow;
                        softoffer["jarvis_expirydate"] = DateTime.UtcNow;
                        softoffer["statuscode"] = new OptionSetValue(0);
                        softoffer["jarvis_marketcode"] = "DE";
                        softoffer["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
                        result.Entities.Add(softoffer);
                    }
                }

                return result;
            };

            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "incident")
                {
                    Entity inc = new Entity("incident");
                    inc.Id = guid;
                    inc["customerid"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_callerlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    inc["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    inc["jarvis_driverlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    inc["jarvis_translationstatusreportedfault"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatuslocation"] = new OptionSetValue(334030000);
                    inc["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    inc["jarvis_translationstatuscustomerexpectations"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatusalertdescription"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatusalertextrainformation"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatusalertdecision"] = new OptionSetValue(334030000);
                    return inc;
                }

                if (entityName == "jarvis_vehicle")
                {
                    Entity client = new Entity("jarvis_vehicle");
                    client.Id = new Guid("6186f7f1-5f09-ed11-82e5-002248934e82");
                    client["jarvis_brandid"] = new EntityReference("jarvis_brand", Guid.NewGuid());
                    client["jarvis_fuelpowertype"] = new EntityReference("jarvis_vehiclefuelpowertype", Guid.NewGuid());
                    return client;
                }

                if (entityName == "jarvis_country")
                {
                    Entity country = new Entity("jarvis_country");
                    country.Id = guid;
                    country["jarvis_iso2countrycode"] = "NL";
                    return country;
                }

                if (entityName == "jarvis_language")
                {
                    Entity language = new Entity("jarvis_language");
                    language.Id = guid;
                    language["jarvis_iso3languagecode6392t"] = "DEU";
                    language["jarvis_iso2languagecode6391"] = "DE";
                    return language;
                }

                if (entityName == "account")
                {
                    Entity account = new Entity("account");
                    account.Id = guid;
                    account["jarvis_language"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    account["jarvis_address1_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    return account;
                }

                if (entityName == "jarvis_brand")
                {
                    Entity client = new Entity("jarvis_brand");
                    client.Id = new Guid("6186f7f1-5f09-ed11-82e5-002248934e82");
                    client["jarvis_mercuriusbrandcode"] = "ted";
                    return client;
                }

                return null;
            };
            this.Service.ExecuteOrganizationRequest = processInstanceRequest =>
            {
                if (processInstanceRequest.RequestName.Contains("RetrieveProcessInstances"))
                {
                    return new RetrieveProcessInstancesResponse()
                    {
                        Results = new ParameterCollection
                {
                    new KeyValuePair<string, object>("Processes", new EntityCollection(new List<Entity> { incident })),
                },
                    };
                }

                if (processInstanceRequest.RequestName.Contains("RetrieveActivePath"))
                {
                    Entity activepath = new Entity("activepath");
                    activepath["stagename"] = "Waiting For Repair Start";
                    activepath["processstageid"] = Guid.NewGuid();
                    return new RetrieveActivePathResponse()
                    {
                        Results = new ParameterCollection
                {
                    new KeyValuePair<string, object>("ProcessStages", new EntityCollection(new List<Entity> { activepath })),
                },
                    };
                }

                return null;
            };

            CasePostOperationSync plugin = new CasePostOperationSync();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// PreValidate Update With Complete Data Test Method with statuscode 5.
        /// </summary>
        [TestMethod]
        public void PostUpdateWithCompleteDataTestMethodStCode5()
        {
            Entity incidentTarget = new Entity("incident");

            EntityReference users = new EntityReference("systemuser", Guid.NewGuid());
            users.Name = "MERCURIUS";

            incidentTarget.Id = Guid.Empty;

            incidentTarget["statuscode"] = new OptionSetValue(5);
            incidentTarget["incidentid"] = Guid.NewGuid();
            incidentTarget["caseorigincode"] = new OptionSetValue(2);
            incidentTarget["casetypecode"] = new OptionSetValue(2);
            incidentTarget["jarvis_mercuriusstatus"] = new OptionSetValue(200);
            incidentTarget["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
            incidentTarget["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["customerid"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["jarvis_registrationnumber"] = "123";
            incidentTarget["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incidentTarget["description"] = "This is for testing";
            incidentTarget["jarvis_location"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incidentTarget["jarvis_customerexpectations"] = "test";
            incidentTarget["jarvis_incidentnature"] = "[{\"id\":\"3efba61a-85ad-44b7-a187-7f54acaf7a67\"}]";
            incidentTarget["jarvis_callerrole"] = new OptionSetValue(4);
            incidentTarget["jarvis_callerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incidentTarget["isescalated"] = true;
            incidentTarget["jarvis_querydescription"] = "test";
            incidentTarget["jarvis_extrainformationonsolutionquery"] = "test";
            incidentTarget["jarvis_translationstatusalertdecision"] = new OptionSetValue(334030000);
            incidentTarget["jarvis_querydecision"] = "test";
            incidentTarget.Attributes["jarvis_eta"] = DateTime.Now;
            incidentTarget.Attributes["jarvis_atc"] = DateTime.Now;
            incidentTarget["modifiedby"] = users;

            Entity incident = new Entity("incident");
            incident.Id = Guid.Empty;
            incident["statuscode"] = new OptionSetValue(5);
            incident["incidentid"] = new EntityReference("incident", Guid.NewGuid());
            incident["caseorigincode"] = new OptionSetValue(2);
            incident["casetypecode"] = new OptionSetValue(2);
            incident["jarvis_mercuriusstatus"] = new OptionSetValue(200);
            incident["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
            incident["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incident["customerid"] = new EntityReference("account", Guid.NewGuid());
            incident["jarvis_registrationnumber"] = "123";
            incident["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["description"] = "This is for testing";
            incident["jarvis_driverlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incident["jarvis_translationstatusreportedfault"] = new OptionSetValue(334030000);
            incident["jarvis_translationstatuslocation"] = new OptionSetValue(334030000);
            incident["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["jarvis_translationstatuscustomerexpectations"] = new OptionSetValue(334030000);
            incident["jarvis_location"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["jarvis_customerexpectations"] = "test";
            incident["ownerid"] = new EntityReference { LogicalName = "system_user", Name = "MERCURIUS" };
            incident["jarvis_sourceid"] = 232323232;
            incident["jarvis_callerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incident["jarvis_callerrole"] = new OptionSetValue(4);
            incident["createdby"] = users;
            incident["parentcaseid"] = new EntityReference("incident", Guid.NewGuid());
            incident["jarvis_translationstatusalertdescription"] = new OptionSetValue(334030000);
            incident["jarvis_translationstatusalertextrainformation"] = new OptionSetValue(334030000);
            incident["jarvis_translationstatusalertdecision"] = new OptionSetValue(334030000);
            incident["jarvis_extrainformationonsolutionquery"] = "test";
            incident["jarvis_querydecision"] = "test";
            incident["jarvis_onetimecustomerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incident["jarvis_onetimecustomercountry"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["jarvis_actualcausefault"] = "test";
            incident["jarvis_mileageafterrepair"] = 10;
            incident["jarvis_mileageunitafterrepair"] = new EntityReference("jarvis_mileageunit", Guid.NewGuid());
            //// Setting Input Parameters.
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", incidentTarget),
               };
            this.PluginExecutionContext.MessageNameGet = () => "Update";
            this.PluginExecutionContext.StageGet = () => 40;
            //// Setting Post and Pre Image
            this.PluginExecutionContext.PostEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PostImage", incident },
                };
            };
            this.PluginExecutionContext.PreEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PreImage", incident },
                };
            };

            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();
                if (query.GetType().Name == "FetchExpression")
                {
                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_vasbreakdownprocess'>"))
                    {
                        Entity vasprocess = new Entity("jarvis_vasbreakdownprocess");
                        vasprocess["businessprocessflowinstanceid"] = Guid.NewGuid();
                        vasprocess["bpf_name"] = "test";
                        vasprocess["bpf_incidentid"] = Guid.NewGuid();
                        vasprocess["activestageid"] = Guid.NewGuid();
                        vasprocess["statecode"] = new OptionSetValue(0);
                        vasprocess["createdon"] = DateTime.UtcNow;
                        vasprocess["createdby"] = "test";
                        vasprocess.Id = Guid.NewGuid();
                        result.Entities.Add(vasprocess);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='incident'>"))
                    {
                        Entity incidentResult = new Entity("incident");
                        incidentResult[Constants.IncidentNature.IncidentIncidentNature_jarvisIincidentnatureid] = new AliasedValue("incidentNature", "incidentnatureid", Guid.NewGuid());
                        incidentResult["jarvis_hdrd"] = true;
                        incidentResult["jarvis_homedealer"] = new EntityReference("account", incident.Id);
                        incidentResult.Id = Guid.NewGuid();
                        result.Entities.Add(incidentResult);
                        result.Entities.Add(incidentResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_configurationjarvis'>"))
                    {
                        Entity configResult = new Entity("jarvis_configurationjarvis");
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationcasestatuschange"] = true;
                        configResult["jarvis_automationreleasecase"] = new OptionSetValue(1);
                        configResult["createdon"] = DateTime.UtcNow;
                        configResult.Id = Guid.NewGuid();
                        result.Entities.Add(configResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_language'>"))
                    {
                        Entity language = new Entity("jarvis_language");
                        language.Id = Guid.NewGuid();
                        language["jarvis_iso3languagecode6392t"] = "DEU";
                        language["jarvis_iso2languagecode6391"] = "DE";
                        language["jarvis_name"] = "DE";
                        result.Entities.Add(language);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_casesoftoffer'>"))
                    {
                        Entity casesoftoffer = new Entity("jarvis_casesoftoffer");
                        casesoftoffer.Id = Guid.NewGuid();
                        casesoftoffer["jarvis_name"] = "DEU";
                        casesoftoffer["jarvis_contractno"] = "DE";
                        casesoftoffer["jarvis_description"] = "DE";
                        casesoftoffer["jarvis_startdate"] = DateTime.UtcNow;
                        casesoftoffer["jarvis_expirydate"] = DateTime.UtcNow;
                        casesoftoffer["statuscode"] = new OptionSetValue(0);
                        casesoftoffer["jarvis_softoffercodelookup"] = new EntityReference("jarvis_softoffer", Guid.NewGuid());
                        result.Entities.Add(casesoftoffer);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_softoffer'>"))
                    {
                        Entity softoffer = new Entity("jarvis_softoffer");
                        softoffer.Id = Guid.NewGuid();
                        softoffer["jarvis_name"] = "DEU";
                        softoffer["jarvis_softoffercode"] = new EntityReference("jarvis_softoffer", Guid.NewGuid());
                        softoffer["jarvis_contractno"] = "DE";
                        softoffer["jarvis_startdate"] = DateTime.UtcNow;
                        softoffer["jarvis_expirydate"] = DateTime.UtcNow;
                        softoffer["statuscode"] = new OptionSetValue(0);
                        softoffer["jarvis_marketcode"] = "DE";
                        softoffer["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
                        result.Entities.Add(softoffer);
                    }

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
                        passoutResult.Id = Guid.NewGuid();
                        result.Entities.Add(passoutResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='systemuser'>"))
                    {
                        Entity user = new Entity("systemuser");
                        user.Id = Guid.NewGuid();
                        user["fullname"] = "MERCURIUS";
                        result.Entities.Add(user);
                    }
                }

                return result;
            };

            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "incident")
                {
                    Entity inc = new Entity("incident");
                    inc.Id = guid;
                    inc["customerid"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_callerlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    inc["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    inc["jarvis_driverlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    inc["jarvis_translationstatusreportedfault"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatuslocation"] = new OptionSetValue(334030000);
                    inc["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    inc["jarvis_translationstatuscustomerexpectations"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatusalertdescription"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatusalertextrainformation"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatusalertdecision"] = new OptionSetValue(334030000);
                    return inc;
                }

                if (entityName == "jarvis_vehicle")
                {
                    Entity client = new Entity("jarvis_vehicle");
                    client.Id = new Guid("6186f7f1-5f09-ed11-82e5-002248934e82");
                    client["jarvis_brandid"] = new EntityReference("jarvis_brand", Guid.NewGuid());
                    client["jarvis_fuelpowertype"] = new EntityReference("jarvis_vehiclefuelpowertype", Guid.NewGuid());
                    return client;
                }

                if (entityName == "jarvis_country")
                {
                    Entity country = new Entity("jarvis_country");
                    country.Id = guid;
                    country["jarvis_iso2countrycode"] = "NL";
                    return country;
                }

                if (entityName == "jarvis_language")
                {
                    Entity language = new Entity("jarvis_language");
                    language.Id = guid;
                    language["jarvis_iso3languagecode6392t"] = "DEU";
                    language["jarvis_iso2languagecode6391"] = "DE";
                    return language;
                }

                if (entityName == "account")
                {
                    Entity account = new Entity("account");
                    account.Id = guid;
                    account["jarvis_language"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    account["jarvis_address1_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    return account;
                }

                if (entityName == "jarvis_brand")
                {
                    Entity client = new Entity("jarvis_brand");
                    client.Id = new Guid("6186f7f1-5f09-ed11-82e5-002248934e82");
                    client["jarvis_mercuriusbrandcode"] = "ted";
                    return client;
                }

                return null;
            };
            this.Service.ExecuteOrganizationRequest = processInstanceRequest =>
            {
                if (processInstanceRequest.RequestName.Contains("RetrieveProcessInstances"))
                {
                    return new RetrieveProcessInstancesResponse()
                    {
                        Results = new ParameterCollection
                {
                    new KeyValuePair<string, object>("Processes", new EntityCollection(new List<Entity> { incident })),
                },
                    };
                }

                if (processInstanceRequest.RequestName.Contains("RetrieveActivePath"))
                {
                    Entity activepath = new Entity("activepath");
                    activepath["stagename"] = "Waiting For Repair Start";
                    activepath["processstageid"] = Guid.NewGuid();
                    return new RetrieveActivePathResponse()
                    {
                        Results = new ParameterCollection
                {
                    new KeyValuePair<string, object>("ProcessStages", new EntityCollection(new List<Entity> { activepath })),
                },
                    };
                }

                return null;
            };

            CasePostOperationSync plugin = new CasePostOperationSync();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// PreValidate Update With Complete Data Test Method with statuscode 70.
        /// </summary>
        [TestMethod]
        public void PostUpdateWithCompleteDataTestMethodStCode70()
        {
            Entity incidentTarget = new Entity("incident");

            EntityReference users = new EntityReference("systemuser", Guid.NewGuid());
            users.Name = "MERCURIUS";

            incidentTarget.Id = Guid.Empty;

            incidentTarget["statuscode"] = new OptionSetValue(70);
            incidentTarget["incidentid"] = Guid.NewGuid();
            incidentTarget["caseorigincode"] = new OptionSetValue(2);
            incidentTarget["casetypecode"] = new OptionSetValue(2);
            incidentTarget["jarvis_mercuriusstatus"] = new OptionSetValue(200);
            incidentTarget["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
            incidentTarget["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["customerid"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["jarvis_registrationnumber"] = "123";
            incidentTarget["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incidentTarget["description"] = "This is for testing";
            incidentTarget["jarvis_location"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incidentTarget["jarvis_customerexpectations"] = "test";
            incidentTarget["jarvis_incidentnature"] = "[{\"id\":\"3efba61a-85ad-44b7-a187-7f54acaf7a67\"}]";
            incidentTarget["jarvis_callerrole"] = new OptionSetValue(4);
            incidentTarget["jarvis_callerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incidentTarget["isescalated"] = true;
            incidentTarget["jarvis_querydescription"] = "test";
            incidentTarget["jarvis_extrainformationonsolutionquery"] = "test";
            incidentTarget["jarvis_translationstatusalertdecision"] = new OptionSetValue(334030000);
            incidentTarget["jarvis_querydecision"] = "test";
            incidentTarget.Attributes["jarvis_eta"] = DateTime.Now;
            incidentTarget.Attributes["jarvis_atc"] = DateTime.Now;
            incidentTarget.Attributes["modifiedby"] = users;

            Entity incident = new Entity("incident");
            incident.Id = Guid.Empty;
            incident["statuscode"] = new OptionSetValue(70);
            incident["incidentid"] = new EntityReference("incident", Guid.NewGuid());
            incident["caseorigincode"] = new OptionSetValue(2);
            incident["casetypecode"] = new OptionSetValue(2);
            incident["jarvis_mercuriusstatus"] = new OptionSetValue(200);
            incident["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
            incident["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incident["customerid"] = new EntityReference("account", Guid.NewGuid());
            incident["jarvis_registrationnumber"] = "123";
            incident["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["description"] = "This is for testing";
            incident["jarvis_driverlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incident["jarvis_translationstatusreportedfault"] = new OptionSetValue(334030000);
            incident["jarvis_translationstatuslocation"] = new OptionSetValue(334030000);
            incident["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["jarvis_translationstatuscustomerexpectations"] = new OptionSetValue(334030000);
            incident["jarvis_location"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["jarvis_customerexpectations"] = "test";
            incident["ownerid"] = new EntityReference { LogicalName = "system_user", Name = "MERCURIUS" };
            incident["jarvis_sourceid"] = 232323232;
            incident["jarvis_callerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incident["jarvis_callerrole"] = new OptionSetValue(4);
            incident["createdby"] = users;
            incident["parentcaseid"] = new EntityReference("incident", Guid.NewGuid());
            incident["jarvis_translationstatusalertdescription"] = new OptionSetValue(334030000);
            incident["jarvis_translationstatusalertextrainformation"] = new OptionSetValue(334030000);
            incident["jarvis_translationstatusalertdecision"] = new OptionSetValue(334030000);
            incident["jarvis_extrainformationonsolutionquery"] = "test";
            incident["jarvis_querydecision"] = "test";
            incident["jarvis_onetimecustomerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incident["jarvis_onetimecustomercountry"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["jarvis_actualcausefault"] = "test";
            incident["jarvis_mileageafterrepair"] = 10;
            incident["jarvis_mileageunitafterrepair"] = new EntityReference("jarvis_mileageunit", Guid.NewGuid());
            //// Setting Input Parameters.
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", incidentTarget),
               };
            this.PluginExecutionContext.MessageNameGet = () => "Update";
            this.PluginExecutionContext.StageGet = () => 40;
            //// Setting Post and Pre Image
            this.PluginExecutionContext.PostEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PostImage", incident },
                };
            };
            this.PluginExecutionContext.PreEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PreImage", incident },
                };
            };

            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();
                if (query.GetType().Name == "FetchExpression")
                {
                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_vasbreakdownprocess'>"))
                    {
                        Entity vasprocess = new Entity("jarvis_vasbreakdownprocess");
                        vasprocess["businessprocessflowinstanceid"] = Guid.NewGuid();
                        vasprocess["bpf_name"] = "test";
                        vasprocess["bpf_incidentid"] = Guid.NewGuid();
                        vasprocess["activestageid"] = Guid.NewGuid();
                        vasprocess["statecode"] = new OptionSetValue(1);
                        vasprocess["createdon"] = DateTime.UtcNow;
                        vasprocess["createdby"] = "test";
                        vasprocess.Id = Guid.NewGuid();
                        result.Entities.Add(vasprocess);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='incident'>"))
                    {
                        Entity incidentResult = new Entity("incident");
                        incidentResult[Constants.IncidentNature.IncidentIncidentNature_jarvisIincidentnatureid] = new AliasedValue("incidentNature", "incidentnatureid", Guid.NewGuid());
                        incidentResult["jarvis_hdrd"] = true;
                        incidentResult["jarvis_homedealer"] = new EntityReference("account", incident.Id);
                        incidentResult["CaseRD.jarvis_repairingdealer"] = new AliasedValue("account", "account", new EntityReference("account", incident.Id));
                        incidentResult.Id = Guid.NewGuid();
                        result.Entities.Add(incidentResult);
                        result.Entities.Add(incidentResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_configurationjarvis'>"))
                    {
                        Entity configResult = new Entity("jarvis_configurationjarvis");
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationcasestatuschange"] = true;
                        configResult["createdon"] = DateTime.UtcNow;
                        configResult.Id = Guid.NewGuid();
                        result.Entities.Add(configResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_language'>"))
                    {
                        Entity language = new Entity("jarvis_language");
                        language.Id = Guid.NewGuid();
                        language["jarvis_iso3languagecode6392t"] = "DEU";
                        language["jarvis_iso2languagecode6391"] = "DE";
                        language["jarvis_name"] = "DE";
                        result.Entities.Add(language);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_casesoftoffer'>"))
                    {
                        Entity casesoftoffer = new Entity("jarvis_casesoftoffer");
                        casesoftoffer.Id = Guid.NewGuid();
                        casesoftoffer["jarvis_name"] = "DEU";
                        casesoftoffer["jarvis_contractno"] = "DE";
                        casesoftoffer["jarvis_description"] = "DE";
                        casesoftoffer["jarvis_startdate"] = DateTime.UtcNow;
                        casesoftoffer["jarvis_expirydate"] = DateTime.UtcNow;
                        casesoftoffer["statuscode"] = new OptionSetValue(0);
                        casesoftoffer["jarvis_softoffercodelookup"] = new EntityReference("jarvis_softoffer", Guid.NewGuid());
                        result.Entities.Add(casesoftoffer);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_softoffer'>"))
                    {
                        Entity softoffer = new Entity("jarvis_softoffer");
                        softoffer.Id = Guid.NewGuid();
                        softoffer["jarvis_name"] = "DEU";
                        softoffer["jarvis_softoffercode"] = new EntityReference("jarvis_softoffer", Guid.NewGuid());
                        softoffer["jarvis_contractno"] = "DE";
                        softoffer["jarvis_startdate"] = DateTime.UtcNow;
                        softoffer["jarvis_expirydate"] = DateTime.UtcNow;
                        softoffer["statuscode"] = new OptionSetValue(0);
                        softoffer["jarvis_marketcode"] = "DE";
                        softoffer["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
                        result.Entities.Add(softoffer);
                    }

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
                        passoutResult.Id = Guid.NewGuid();
                        result.Entities.Add(passoutResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_repairinformation'>"))
                    {
                        Entity repairInfoResult = new Entity("jarvis_repairinformation");
                        repairInfoResult["jarvis_name"] = "testpassout";
                        repairInfoResult["createdon"] = DateTime.UtcNow;
                        repairInfoResult.Id = Guid.NewGuid();
                        result.Entities.Add(repairInfoResult);
                    }
                }

                return result;
            };

            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "incident")
                {
                    Entity inc = new Entity("incident");
                    inc.Id = guid;
                    inc["customerid"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_callerlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    inc["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    inc["jarvis_driverlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    inc["jarvis_translationstatusreportedfault"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatuslocation"] = new OptionSetValue(334030000);
                    inc["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    inc["jarvis_translationstatuscustomerexpectations"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatusalertdescription"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatusalertextrainformation"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatusalertdecision"] = new OptionSetValue(334030000);
                    return inc;
                }

                if (entityName == "jarvis_vehicle")
                {
                    Entity client = new Entity("jarvis_vehicle");
                    client.Id = new Guid("6186f7f1-5f09-ed11-82e5-002248934e82");
                    client["jarvis_brandid"] = new EntityReference("jarvis_brand", Guid.NewGuid());
                    client["jarvis_fuelpowertype"] = new EntityReference("jarvis_vehiclefuelpowertype", Guid.NewGuid());
                    return client;
                }

                if (entityName == "jarvis_country")
                {
                    Entity country = new Entity("jarvis_country");
                    country.Id = guid;
                    country["jarvis_iso2countrycode"] = "NL";
                    return country;
                }

                if (entityName == "jarvis_language")
                {
                    Entity language = new Entity("jarvis_language");
                    language.Id = guid;
                    language["jarvis_iso3languagecode6392t"] = "DEU";
                    language["jarvis_iso2languagecode6391"] = "DE";
                    return language;
                }

                if (entityName == "account")
                {
                    Entity account = new Entity("account");
                    account.Id = guid;
                    account["jarvis_language"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    account["jarvis_address1_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    return account;
                }

                if (entityName == "jarvis_brand")
                {
                    Entity client = new Entity("jarvis_brand");
                    client.Id = new Guid("6186f7f1-5f09-ed11-82e5-002248934e82");
                    client["jarvis_mercuriusbrandcode"] = "ted";
                    return client;
                }

                return null;
            };
            this.Service.ExecuteOrganizationRequest = processInstanceRequest =>
            {
                if (processInstanceRequest.RequestName.Contains("RetrieveProcessInstances"))
                {
                    return new RetrieveProcessInstancesResponse()
                    {
                        Results = new ParameterCollection
                {
                    new KeyValuePair<string, object>("Processes", new EntityCollection(new List<Entity> { incident })),
                },
                    };
                }

                if (processInstanceRequest.RequestName.Contains("RetrieveActivePath"))
                {
                    Entity activepath = new Entity("activepath");
                    activepath["stagename"] = "Waiting For Repair Start";
                    activepath["processstageid"] = Guid.NewGuid();
                    return new RetrieveActivePathResponse()
                    {
                        Results = new ParameterCollection
                {
                    new KeyValuePair<string, object>("ProcessStages", new EntityCollection(new List<Entity> { activepath })),
                },
                    };
                }

                return null;
            };

            CasePostOperationSync plugin = new CasePostOperationSync();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// PreValidate Update With Complete Data Test Method with statuscode 70 with jarvis_act.
        /// </summary>
        [TestMethod]
        public void PostUpdateWithCompleteDataTestMethodStCode70Atc()
        {
            Entity incidentTarget = new Entity("incident");

            EntityReference users = new EntityReference("systemuser", Guid.NewGuid());
            users.Name = "MERCURIUS";

            incidentTarget.Id = Guid.Empty;

            incidentTarget["statuscode"] = new OptionSetValue(70);
            incidentTarget["incidentid"] = Guid.NewGuid();
            incidentTarget["caseorigincode"] = new OptionSetValue(2);
            incidentTarget["casetypecode"] = new OptionSetValue(2);
            incidentTarget["jarvis_mercuriusstatus"] = new OptionSetValue(200);
            incidentTarget["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
            incidentTarget["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["customerid"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["jarvis_registrationnumber"] = "123";
            incidentTarget["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incidentTarget["description"] = "This is for testing";
            incidentTarget["jarvis_location"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incidentTarget["jarvis_customerexpectations"] = "test";
            incidentTarget["jarvis_incidentnature"] = "[{\"id\":\"3efba61a-85ad-44b7-a187-7f54acaf7a67\"}]";
            incidentTarget["jarvis_callerrole"] = new OptionSetValue(4);
            incidentTarget["jarvis_callerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incidentTarget["isescalated"] = true;
            incidentTarget["jarvis_querydescription"] = "test";
            incidentTarget["jarvis_extrainformationonsolutionquery"] = "test";
            incidentTarget["jarvis_translationstatusalertdecision"] = new OptionSetValue(334030000);
            incidentTarget["jarvis_querydecision"] = "test";
            incidentTarget.Attributes["jarvis_eta"] = DateTime.Now;
            incidentTarget.Attributes["jarvis_atc"] = DateTime.Now;
            incidentTarget.Attributes["modifiedby"] = users;

            Entity incident = new Entity("incident");
            incident.Id = Guid.Empty;
            incident["statuscode"] = new OptionSetValue(70);
            incident["incidentid"] = new EntityReference("incident", Guid.NewGuid());
            incident["caseorigincode"] = new OptionSetValue(2);
            incident["casetypecode"] = new OptionSetValue(2);
            incident["jarvis_mercuriusstatus"] = new OptionSetValue(200);
            incident["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
            incident["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incident["customerid"] = new EntityReference("account", Guid.NewGuid());
            incident["jarvis_registrationnumber"] = "123";
            incident["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["description"] = "This is for testing";
            incident["jarvis_driverlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incident["jarvis_translationstatusreportedfault"] = new OptionSetValue(334030000);
            incident["jarvis_translationstatuslocation"] = new OptionSetValue(334030000);
            incident["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["jarvis_translationstatuscustomerexpectations"] = new OptionSetValue(334030000);
            incident["jarvis_location"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["jarvis_customerexpectations"] = "test";
            incident["ownerid"] = new EntityReference { LogicalName = "system_user", Name = "MERCURIUS" };
            incident["jarvis_sourceid"] = 232323232;
            incident["jarvis_callerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incident["jarvis_callerrole"] = new OptionSetValue(4);
            incident["createdby"] = users;
            incident["parentcaseid"] = new EntityReference("incident", Guid.NewGuid());
            incident["jarvis_translationstatusalertdescription"] = new OptionSetValue(334030000);
            incident["jarvis_translationstatusalertextrainformation"] = new OptionSetValue(334030000);
            incident["jarvis_translationstatusalertdecision"] = new OptionSetValue(334030000);
            incident["jarvis_extrainformationonsolutionquery"] = "test";
            incident["jarvis_querydecision"] = "test";
            incident["jarvis_onetimecustomerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incident["jarvis_onetimecustomercountry"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["jarvis_actualcausefault"] = "test";
            incident["jarvis_mileageafterrepair"] = 10;
            incident["jarvis_mileageunitafterrepair"] = new EntityReference("jarvis_mileageunit", Guid.NewGuid());
            incident.Attributes["jarvis_atc"] = DateTime.Now;
            //// Setting Input Parameters.
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", incidentTarget),
               };
            this.PluginExecutionContext.MessageNameGet = () => "Update";
            this.PluginExecutionContext.StageGet = () => 40;
            //// Setting Post and Pre Image
            this.PluginExecutionContext.PostEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PostImage", incident },
                };
            };
            incident["statuscode"] = new OptionSetValue(60);
            this.PluginExecutionContext.PreEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PreImage", incident },
                };
            };

            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();
                if (query.GetType().Name == "FetchExpression")
                {
                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_vasbreakdownprocess'>"))
                    {
                        Entity vasprocess = new Entity("jarvis_vasbreakdownprocess");
                        vasprocess["businessprocessflowinstanceid"] = Guid.NewGuid();
                        vasprocess["bpf_name"] = "test";
                        vasprocess["bpf_incidentid"] = Guid.NewGuid();
                        vasprocess["activestageid"] = Guid.NewGuid();
                        vasprocess["statecode"] = new OptionSetValue(1);
                        vasprocess["createdon"] = DateTime.UtcNow;
                        vasprocess["createdby"] = "test";
                        vasprocess.Id = Guid.NewGuid();
                        result.Entities.Add(vasprocess);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='incident'>"))
                    {
                        Entity incidentResult = new Entity("incident");
                        incidentResult[Constants.IncidentNature.IncidentIncidentNature_jarvisIincidentnatureid] = new AliasedValue("incidentNature", "incidentnatureid", Guid.NewGuid());
                        incidentResult["jarvis_hdrd"] = true;
                        incidentResult["jarvis_homedealer"] = new EntityReference("account", incident.Id);
                        incidentResult["CaseRD.jarvis_repairingdealer"] = new AliasedValue("account", "account", new EntityReference("account", incident.Id));
                        incidentResult.Id = Guid.NewGuid();
                        result.Entities.Add(incidentResult);
                        result.Entities.Add(incidentResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_configurationjarvis'>"))
                    {
                        Entity configResult = new Entity("jarvis_configurationjarvis");
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationcasestatuschange"] = true;
                        configResult["createdon"] = DateTime.UtcNow;
                        configResult.Id = Guid.NewGuid();
                        result.Entities.Add(configResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_language'>"))
                    {
                        Entity language = new Entity("jarvis_language");
                        language.Id = Guid.NewGuid();
                        language["jarvis_iso3languagecode6392t"] = "DEU";
                        language["jarvis_iso2languagecode6391"] = "DE";
                        language["jarvis_name"] = "DE";
                        result.Entities.Add(language);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_casesoftoffer'>"))
                    {
                        Entity casesoftoffer = new Entity("jarvis_casesoftoffer");
                        casesoftoffer.Id = Guid.NewGuid();
                        casesoftoffer["jarvis_name"] = "DEU";
                        casesoftoffer["jarvis_contractno"] = "DE";
                        casesoftoffer["jarvis_description"] = "DE";
                        casesoftoffer["jarvis_startdate"] = DateTime.UtcNow;
                        casesoftoffer["jarvis_expirydate"] = DateTime.UtcNow;
                        casesoftoffer["statuscode"] = new OptionSetValue(0);
                        casesoftoffer["jarvis_softoffercodelookup"] = new EntityReference("jarvis_softoffer", Guid.NewGuid());
                        result.Entities.Add(casesoftoffer);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_softoffer'>"))
                    {
                        Entity softoffer = new Entity("jarvis_softoffer");
                        softoffer.Id = Guid.NewGuid();
                        softoffer["jarvis_name"] = "DEU";
                        softoffer["jarvis_softoffercode"] = new EntityReference("jarvis_softoffer", Guid.NewGuid());
                        softoffer["jarvis_contractno"] = "DE";
                        softoffer["jarvis_startdate"] = DateTime.UtcNow;
                        softoffer["jarvis_expirydate"] = DateTime.UtcNow;
                        softoffer["statuscode"] = new OptionSetValue(0);
                        softoffer["jarvis_marketcode"] = "DE";
                        softoffer["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
                        result.Entities.Add(softoffer);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_repairinformation'>"))
                    {
                        Entity repairInfoResult = new Entity("jarvis_repairinformation");
                        repairInfoResult["jarvis_name"] = "testpassout";
                        repairInfoResult["createdon"] = DateTime.UtcNow;
                        repairInfoResult.Id = Guid.NewGuid();
                        result.Entities.Add(repairInfoResult);
                    }
                }

                return result;
            };

            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "incident")
                {
                    Entity inc = new Entity("incident");
                    inc.Id = guid;
                    inc["customerid"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_callerlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    inc["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    inc["jarvis_driverlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    inc["jarvis_translationstatusreportedfault"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatuslocation"] = new OptionSetValue(334030000);
                    inc["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    inc["jarvis_translationstatuscustomerexpectations"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatusalertdescription"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatusalertextrainformation"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatusalertdecision"] = new OptionSetValue(334030000);
                    return inc;
                }

                if (entityName == "jarvis_vehicle")
                {
                    Entity client = new Entity("jarvis_vehicle");
                    client.Id = new Guid("6186f7f1-5f09-ed11-82e5-002248934e82");
                    client["jarvis_brandid"] = new EntityReference("jarvis_brand", Guid.NewGuid());
                    client["jarvis_fuelpowertype"] = new EntityReference("jarvis_vehiclefuelpowertype", Guid.NewGuid());
                    return client;
                }

                if (entityName == "jarvis_country")
                {
                    Entity country = new Entity("jarvis_country");
                    country.Id = guid;
                    country["jarvis_iso2countrycode"] = "NL";
                    return country;
                }

                if (entityName == "jarvis_language")
                {
                    Entity language = new Entity("jarvis_language");
                    language.Id = guid;
                    language["jarvis_iso3languagecode6392t"] = "DEU";
                    language["jarvis_iso2languagecode6391"] = "DE";
                    return language;
                }

                if (entityName == "account")
                {
                    Entity account = new Entity("account");
                    account.Id = guid;
                    account["jarvis_language"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    account["jarvis_address1_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    return account;
                }

                if (entityName == "jarvis_brand")
                {
                    Entity client = new Entity("jarvis_brand");
                    client.Id = new Guid("6186f7f1-5f09-ed11-82e5-002248934e82");
                    client["jarvis_mercuriusbrandcode"] = "ted";
                    return client;
                }

                return null;
            };
            this.Service.ExecuteOrganizationRequest = processInstanceRequest =>
            {
                if (processInstanceRequest.RequestName.Contains("RetrieveProcessInstances"))
                {
                    return new RetrieveProcessInstancesResponse()
                    {
                        Results = new ParameterCollection
                {
                    new KeyValuePair<string, object>("Processes", new EntityCollection(new List<Entity> { incident })),
                },
                    };
                }

                if (processInstanceRequest.RequestName.Contains("RetrieveActivePath"))
                {
                    Entity activepath = new Entity("activepath");
                    activepath["stagename"] = "Waiting For Repair Start";
                    activepath["processstageid"] = Guid.NewGuid();
                    return new RetrieveActivePathResponse()
                    {
                        Results = new ParameterCollection
                {
                    new KeyValuePair<string, object>("ProcessStages", new EntityCollection(new List<Entity> { activepath })),
                },
                    };
                }

                return null;
            };

            CasePostOperationSync plugin = new CasePostOperationSync();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// PreValidate Update With Complete Data Test Method with statuscode 70 with No JED.
        /// </summary>
        [TestMethod]
        public void PostUpdateWithCompleteDataTestMethodStCode70NoJed()
        {
            Entity incidentTarget = new Entity("incident");

            EntityReference users = new EntityReference("systemuser", Guid.NewGuid());
            users.Name = "MERCURIUS";

            incidentTarget.Id = Guid.Empty;

            incidentTarget["statuscode"] = new OptionSetValue(70);
            incidentTarget["incidentid"] = Guid.NewGuid();
            incidentTarget["caseorigincode"] = new OptionSetValue(2);
            incidentTarget["casetypecode"] = new OptionSetValue(2);
            incidentTarget["jarvis_mercuriusstatus"] = new OptionSetValue(200);
            incidentTarget["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
            incidentTarget["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["customerid"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["jarvis_registrationnumber"] = "123";
            incidentTarget["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incidentTarget["description"] = "This is for testing";
            incidentTarget["jarvis_location"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incidentTarget["jarvis_customerexpectations"] = "test";
            incidentTarget["jarvis_incidentnature"] = "[{\"id\":\"3efba61a-85ad-44b7-a187-7f54acaf7a67\"}]";
            incidentTarget["jarvis_callerrole"] = new OptionSetValue(4);
            incidentTarget["jarvis_callerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incidentTarget["isescalated"] = true;
            incidentTarget["jarvis_querydescription"] = "test";
            incidentTarget["jarvis_extrainformationonsolutionquery"] = "test";
            incidentTarget["jarvis_translationstatusalertdecision"] = new OptionSetValue(334030000);
            incidentTarget["jarvis_querydecision"] = "test";
            incidentTarget.Attributes["jarvis_eta"] = DateTime.Now;
            incidentTarget.Attributes["jarvis_atc"] = DateTime.Now;
            incidentTarget.Attributes["modifiedby"] = users;

            Entity incident = new Entity("incident");
            incident.Id = Guid.Empty;
            incident["statuscode"] = new OptionSetValue(70);
            incident["incidentid"] = new EntityReference("incident", Guid.NewGuid());
            incident["caseorigincode"] = new OptionSetValue(2);
            incident["casetypecode"] = new OptionSetValue(2);
            incident["jarvis_mercuriusstatus"] = new OptionSetValue(200);
            incident["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
            incident["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incident["customerid"] = new EntityReference("account", Guid.NewGuid());
            incident["jarvis_registrationnumber"] = "123";
            incident["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["description"] = "This is for testing";
            incident["jarvis_driverlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incident["jarvis_translationstatusreportedfault"] = new OptionSetValue(334030000);
            incident["jarvis_translationstatuslocation"] = new OptionSetValue(334030000);
            incident["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["jarvis_translationstatuscustomerexpectations"] = new OptionSetValue(334030000);
            incident["jarvis_location"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["jarvis_customerexpectations"] = "test";
            incident["ownerid"] = new EntityReference { LogicalName = "system_user", Name = "MERCURIUS" };
            incident["jarvis_sourceid"] = 232323232;
            incident["jarvis_callerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incident["jarvis_callerrole"] = new OptionSetValue(4);
            incident["createdby"] = users;
            incident["parentcaseid"] = new EntityReference("incident", Guid.NewGuid());
            incident["jarvis_translationstatusalertdescription"] = new OptionSetValue(334030000);
            incident["jarvis_translationstatusalertextrainformation"] = new OptionSetValue(334030000);
            incident["jarvis_translationstatusalertdecision"] = new OptionSetValue(334030000);
            incident["jarvis_extrainformationonsolutionquery"] = "test";
            incident["jarvis_querydecision"] = "test";
            incident["jarvis_onetimecustomerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incident["jarvis_onetimecustomercountry"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident.Attributes["jarvis_atc"] = DateTime.Now;
            //// Setting Input Parameters.
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", incidentTarget),
               };
            this.PluginExecutionContext.MessageNameGet = () => "Update";
            this.PluginExecutionContext.StageGet = () => 40;
            //// Setting Post and Pre Image
            this.PluginExecutionContext.PostEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PostImage", incident },
                };
            };
            incident["statuscode"] = new OptionSetValue(60);
            this.PluginExecutionContext.PreEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PreImage", incident },
                };
            };

            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();
                if (query.GetType().Name == "FetchExpression")
                {
                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_vasbreakdownprocess'>"))
                    {
                        Entity vasprocess = new Entity("jarvis_vasbreakdownprocess");
                        vasprocess["businessprocessflowinstanceid"] = Guid.NewGuid();
                        vasprocess["bpf_name"] = "test";
                        vasprocess["bpf_incidentid"] = Guid.NewGuid();
                        vasprocess["activestageid"] = Guid.NewGuid();
                        vasprocess["statecode"] = new OptionSetValue(1);
                        vasprocess["createdon"] = DateTime.UtcNow;
                        vasprocess["createdby"] = "test";
                        vasprocess.Id = Guid.NewGuid();
                        result.Entities.Add(vasprocess);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='incident'>"))
                    {
                        Entity incidentResult = new Entity("incident");
                        incidentResult[Constants.IncidentNature.IncidentIncidentNature_jarvisIincidentnatureid] = new AliasedValue("incidentNature", "incidentnatureid", Guid.NewGuid());
                        incidentResult["jarvis_hdrd"] = true;
                        incidentResult["jarvis_homedealer"] = new EntityReference("account", incident.Id);
                        incidentResult["CaseRD.jarvis_repairingdealer"] = new AliasedValue("account", "account", new EntityReference("account", incident.Id));
                        incidentResult.Id = Guid.NewGuid();
                        result.Entities.Add(incidentResult);
                        result.Entities.Add(incidentResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_configurationjarvis'>"))
                    {
                        Entity configResult = new Entity("jarvis_configurationjarvis");
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationcasestatuschange"] = true;
                        configResult["createdon"] = DateTime.UtcNow;
                        configResult.Id = Guid.NewGuid();
                        result.Entities.Add(configResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_language'>"))
                    {
                        Entity language = new Entity("jarvis_language");
                        language.Id = Guid.NewGuid();
                        language["jarvis_iso3languagecode6392t"] = "DEU";
                        language["jarvis_iso2languagecode6391"] = "DE";
                        language["jarvis_name"] = "DE";
                        result.Entities.Add(language);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_casesoftoffer'>"))
                    {
                        Entity casesoftoffer = new Entity("jarvis_casesoftoffer");
                        casesoftoffer.Id = Guid.NewGuid();
                        casesoftoffer["jarvis_name"] = "DEU";
                        casesoftoffer["jarvis_contractno"] = "DE";
                        casesoftoffer["jarvis_description"] = "DE";
                        casesoftoffer["jarvis_startdate"] = DateTime.UtcNow;
                        casesoftoffer["jarvis_expirydate"] = DateTime.UtcNow;
                        casesoftoffer["statuscode"] = new OptionSetValue(0);
                        casesoftoffer["jarvis_softoffercodelookup"] = new EntityReference("jarvis_softoffer", Guid.NewGuid());
                        result.Entities.Add(casesoftoffer);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_softoffer'>"))
                    {
                        Entity softoffer = new Entity("jarvis_softoffer");
                        softoffer.Id = Guid.NewGuid();
                        softoffer["jarvis_name"] = "DEU";
                        softoffer["jarvis_softoffercode"] = new EntityReference("jarvis_softoffer", Guid.NewGuid());
                        softoffer["jarvis_contractno"] = "DE";
                        softoffer["jarvis_startdate"] = DateTime.UtcNow;
                        softoffer["jarvis_expirydate"] = DateTime.UtcNow;
                        softoffer["statuscode"] = new OptionSetValue(0);
                        softoffer["jarvis_marketcode"] = "DE";
                        softoffer["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
                        result.Entities.Add(softoffer);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_repairinformation'>"))
                    {
                        Entity repairInfoResult = new Entity("jarvis_repairinformation");
                        repairInfoResult["jarvis_name"] = "testpassout";
                        repairInfoResult["createdon"] = DateTime.UtcNow;
                        repairInfoResult.Id = Guid.NewGuid();
                        result.Entities.Add(repairInfoResult);
                    }
                }

                return result;
            };

            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "incident")
                {
                    Entity inc = new Entity("incident");
                    inc.Id = guid;
                    inc["customerid"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_callerlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    inc["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    inc["jarvis_driverlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    inc["jarvis_translationstatusreportedfault"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatuslocation"] = new OptionSetValue(334030000);
                    inc["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    inc["jarvis_translationstatuscustomerexpectations"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatusalertdescription"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatusalertextrainformation"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatusalertdecision"] = new OptionSetValue(334030000);
                    return inc;
                }

                if (entityName == "jarvis_vehicle")
                {
                    Entity client = new Entity("jarvis_vehicle");
                    client.Id = new Guid("6186f7f1-5f09-ed11-82e5-002248934e82");
                    client["jarvis_brandid"] = new EntityReference("jarvis_brand", Guid.NewGuid());
                    client["jarvis_fuelpowertype"] = new EntityReference("jarvis_vehiclefuelpowertype", Guid.NewGuid());
                    return client;
                }

                if (entityName == "jarvis_country")
                {
                    Entity country = new Entity("jarvis_country");
                    country.Id = guid;
                    country["jarvis_iso2countrycode"] = "NL";
                    return country;
                }

                if (entityName == "jarvis_language")
                {
                    Entity language = new Entity("jarvis_language");
                    language.Id = guid;
                    language["jarvis_iso3languagecode6392t"] = "DEU";
                    language["jarvis_iso2languagecode6391"] = "DE";
                    return language;
                }

                if (entityName == "account")
                {
                    Entity account = new Entity("account");
                    account.Id = guid;
                    account["jarvis_language"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    account["jarvis_address1_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    return account;
                }

                if (entityName == "jarvis_brand")
                {
                    Entity client = new Entity("jarvis_brand");
                    client.Id = new Guid("6186f7f1-5f09-ed11-82e5-002248934e82");
                    client["jarvis_mercuriusbrandcode"] = "ted";
                    return client;
                }

                return null;
            };
            this.Service.ExecuteOrganizationRequest = processInstanceRequest =>
            {
                if (processInstanceRequest.RequestName.Contains("RetrieveProcessInstances"))
                {
                    return new RetrieveProcessInstancesResponse()
                    {
                        Results = new ParameterCollection
                {
                    new KeyValuePair<string, object>("Processes", new EntityCollection(new List<Entity> { incident })),
                },
                    };
                }

                if (processInstanceRequest.RequestName.Contains("RetrieveActivePath"))
                {
                    Entity activepath = new Entity("activepath");
                    activepath["stagename"] = "Waiting For Repair Start";
                    activepath["processstageid"] = Guid.NewGuid();
                    return new RetrieveActivePathResponse()
                    {
                        Results = new ParameterCollection
                {
                    new KeyValuePair<string, object>("ProcessStages", new EntityCollection(new List<Entity> { activepath })),
                },
                    };
                }

                return null;
            };

            CasePostOperationSync plugin = new CasePostOperationSync();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// PreValidate Update With Complete Data Test Method with statuscode 60.
        /// </summary>
        [TestMethod]
        public void PostUpdateWithCompleteDataTestMethodStCode60()
        {
            Entity incidentTarget = new Entity("incident");

            EntityReference users = new EntityReference("systemuser", Guid.NewGuid());
            users.Name = "MERCURIUS";

            incidentTarget.Id = Guid.Empty;

            incidentTarget["statuscode"] = new OptionSetValue(60);
            incidentTarget["incidentid"] = Guid.NewGuid();
            incidentTarget["caseorigincode"] = new OptionSetValue(2);
            incidentTarget["casetypecode"] = new OptionSetValue(2);
            incidentTarget["jarvis_mercuriusstatus"] = new OptionSetValue(200);
            incidentTarget["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
            incidentTarget["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["customerid"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["jarvis_registrationnumber"] = "123";
            incidentTarget["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incidentTarget["description"] = "This is for testing";
            incidentTarget["jarvis_location"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incidentTarget["jarvis_customerexpectations"] = "test";
            incidentTarget["jarvis_incidentnature"] = "[{\"id\":\"3efba61a-85ad-44b7-a187-7f54acaf7a67\"}]";
            incidentTarget["jarvis_callerrole"] = new OptionSetValue(4);
            incidentTarget["jarvis_callerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incidentTarget["isescalated"] = true;
            incidentTarget["jarvis_querydescription"] = "test";
            incidentTarget["jarvis_extrainformationonsolutionquery"] = "test";
            incidentTarget["jarvis_translationstatusalertdecision"] = new OptionSetValue(334030000);
            incidentTarget["jarvis_querydecision"] = "test";
            incidentTarget.Attributes["jarvis_eta"] = DateTime.Now;
            incidentTarget.Attributes["jarvis_atc"] = DateTime.Now;
            incidentTarget.Attributes["modifiedby"] = users;

            Entity incident = new Entity("incident");
            incident.Id = Guid.Empty;
            incident["statuscode"] = new OptionSetValue(60);
            incident["incidentid"] = new EntityReference("incident", Guid.NewGuid());
            incident["caseorigincode"] = new OptionSetValue(2);
            incident["casetypecode"] = new OptionSetValue(2);
            incident["jarvis_mercuriusstatus"] = new OptionSetValue(200);
            incident["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
            incident["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incident["customerid"] = new EntityReference("account", Guid.NewGuid());
            incident["jarvis_registrationnumber"] = "123";
            incident["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["description"] = "This is for testing";
            incident["jarvis_driverlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incident["jarvis_translationstatusreportedfault"] = new OptionSetValue(334030000);
            incident["jarvis_translationstatuslocation"] = new OptionSetValue(334030000);
            incident["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["jarvis_translationstatuscustomerexpectations"] = new OptionSetValue(334030000);
            incident["jarvis_location"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["jarvis_customerexpectations"] = "test";
            incident["ownerid"] = new EntityReference { LogicalName = "system_user", Name = "MERCURIUS" };
            incident["jarvis_sourceid"] = 232323232;
            incident["jarvis_callerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incident["jarvis_callerrole"] = new OptionSetValue(4);
            incident["createdby"] = users;
            incident["parentcaseid"] = new EntityReference("incident", Guid.NewGuid());
            incident["jarvis_translationstatusalertdescription"] = new OptionSetValue(334030000);
            incident["jarvis_translationstatusalertextrainformation"] = new OptionSetValue(334030000);
            incident["jarvis_translationstatusalertdecision"] = new OptionSetValue(334030000);
            incident["jarvis_extrainformationonsolutionquery"] = "test";
            incident["jarvis_querydecision"] = "test";
            incident["jarvis_onetimecustomerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incident["jarvis_onetimecustomercountry"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["jarvis_actualcausefault"] = "test";
            incident["jarvis_mileageafterrepair"] = 10;
            incident["jarvis_mileageunitafterrepair"] = new EntityReference("jarvis_mileageunit", Guid.NewGuid());
            //// Setting Input Parameters.
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", incidentTarget),
               };
            this.PluginExecutionContext.MessageNameGet = () => "Update";
            this.PluginExecutionContext.StageGet = () => 40;
            //// Setting Post and Pre Image
            this.PluginExecutionContext.PostEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PostImage", incident },
                };
            };
            this.PluginExecutionContext.PreEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PreImage", incident },
                };
            };

            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();
                if (query.GetType().Name == "FetchExpression")
                {
                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_vasbreakdownprocess'>"))
                    {
                        Entity vasprocess = new Entity("jarvis_vasbreakdownprocess");
                        vasprocess["businessprocessflowinstanceid"] = Guid.NewGuid();
                        vasprocess["bpf_name"] = "test";
                        vasprocess["bpf_incidentid"] = Guid.NewGuid();
                        vasprocess["activestageid"] = Guid.NewGuid();
                        vasprocess["statecode"] = new OptionSetValue(1);
                        vasprocess["createdon"] = DateTime.UtcNow;
                        vasprocess["createdby"] = "test";
                        vasprocess.Id = Guid.NewGuid();
                        result.Entities.Add(vasprocess);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='incident'>"))
                    {
                        Entity incidentResult = new Entity("incident");
                        incidentResult[Constants.IncidentNature.IncidentIncidentNature_jarvisIincidentnatureid] = new AliasedValue("incidentNature", "incidentnatureid", Guid.NewGuid());
                        incidentResult["jarvis_hdrd"] = true;
                        incidentResult["jarvis_homedealer"] = new EntityReference("account", incident.Id);
                        incidentResult["CaseRD.jarvis_repairingdealer"] = new AliasedValue("account", "account", new EntityReference("account", incident.Id));
                        incidentResult.Id = Guid.NewGuid();
                        result.Entities.Add(incidentResult);
                        result.Entities.Add(incidentResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_configurationjarvis'>"))
                    {
                        Entity configResult = new Entity("jarvis_configurationjarvis");
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationcasestatuschange"] = true;
                        configResult["createdon"] = DateTime.UtcNow;
                        configResult.Id = Guid.NewGuid();
                        result.Entities.Add(configResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_language'>"))
                    {
                        Entity language = new Entity("jarvis_language");
                        language.Id = Guid.NewGuid();
                        language["jarvis_iso3languagecode6392t"] = "DEU";
                        language["jarvis_iso2languagecode6391"] = "DE";
                        language["jarvis_name"] = "DE";
                        result.Entities.Add(language);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_casesoftoffer'>"))
                    {
                        Entity casesoftoffer = new Entity("jarvis_casesoftoffer");
                        casesoftoffer.Id = Guid.NewGuid();
                        casesoftoffer["jarvis_name"] = "DEU";
                        casesoftoffer["jarvis_contractno"] = "DE";
                        casesoftoffer["jarvis_description"] = "DE";
                        casesoftoffer["jarvis_startdate"] = DateTime.UtcNow;
                        casesoftoffer["jarvis_expirydate"] = DateTime.UtcNow;
                        casesoftoffer["statuscode"] = new OptionSetValue(0);
                        casesoftoffer["jarvis_softoffercodelookup"] = new EntityReference("jarvis_softoffer", Guid.NewGuid());
                        result.Entities.Add(casesoftoffer);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_softoffer'>"))
                    {
                        Entity softoffer = new Entity("jarvis_softoffer");
                        softoffer.Id = Guid.NewGuid();
                        softoffer["jarvis_name"] = "DEU";
                        softoffer["jarvis_softoffercode"] = new EntityReference("jarvis_softoffer", Guid.NewGuid());
                        softoffer["jarvis_contractno"] = "DE";
                        softoffer["jarvis_startdate"] = DateTime.UtcNow;
                        softoffer["jarvis_expirydate"] = DateTime.UtcNow;
                        softoffer["statuscode"] = new OptionSetValue(0);
                        softoffer["jarvis_marketcode"] = "DE";
                        softoffer["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
                        result.Entities.Add(softoffer);
                    }

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
                        passoutResult.Id = Guid.NewGuid();
                        result.Entities.Add(passoutResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_repairinformation'>"))
                    {
                        Entity repairInfoResult = new Entity("jarvis_repairinformation");
                        repairInfoResult["jarvis_name"] = "testpassout";
                        repairInfoResult["createdon"] = DateTime.UtcNow;
                        repairInfoResult.Id = Guid.NewGuid();
                        result.Entities.Add(repairInfoResult);
                    }
                }

                return result;
            };

            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "incident")
                {
                    Entity inc = new Entity("incident");
                    inc.Id = guid;
                    inc["customerid"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_callerlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    inc["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    inc["jarvis_driverlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    inc["jarvis_translationstatusreportedfault"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatuslocation"] = new OptionSetValue(334030000);
                    inc["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    inc["jarvis_translationstatuscustomerexpectations"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatusalertdescription"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatusalertextrainformation"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatusalertdecision"] = new OptionSetValue(334030000);
                    return inc;
                }

                if (entityName == "jarvis_vehicle")
                {
                    Entity client = new Entity("jarvis_vehicle");
                    client.Id = new Guid("6186f7f1-5f09-ed11-82e5-002248934e82");
                    client["jarvis_brandid"] = new EntityReference("jarvis_brand", Guid.NewGuid());
                    client["jarvis_fuelpowertype"] = new EntityReference("jarvis_vehiclefuelpowertype", Guid.NewGuid());
                    return client;
                }

                if (entityName == "jarvis_country")
                {
                    Entity country = new Entity("jarvis_country");
                    country.Id = guid;
                    country["jarvis_iso2countrycode"] = "NL";
                    return country;
                }

                if (entityName == "jarvis_language")
                {
                    Entity language = new Entity("jarvis_language");
                    language.Id = guid;
                    language["jarvis_iso3languagecode6392t"] = "DEU";
                    language["jarvis_iso2languagecode6391"] = "DE";
                    return language;
                }

                if (entityName == "account")
                {
                    Entity account = new Entity("account");
                    account.Id = guid;
                    account["jarvis_language"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    account["jarvis_address1_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    return account;
                }

                if (entityName == "jarvis_brand")
                {
                    Entity client = new Entity("jarvis_brand");
                    client.Id = new Guid("6186f7f1-5f09-ed11-82e5-002248934e82");
                    client["jarvis_mercuriusbrandcode"] = "ted";
                    return client;
                }

                return null;
            };
            this.Service.ExecuteOrganizationRequest = processInstanceRequest =>
            {
                if (processInstanceRequest.RequestName.Contains("RetrieveProcessInstances"))
                {
                    return new RetrieveProcessInstancesResponse()
                    {
                        Results = new ParameterCollection
                {
                    new KeyValuePair<string, object>("Processes", new EntityCollection(new List<Entity> { incident })),
                },
                    };
                }

                if (processInstanceRequest.RequestName.Contains("RetrieveActivePath"))
                {
                    Entity activepath = new Entity("activepath");
                    activepath["stagename"] = "Waiting For Repair Start";
                    activepath["processstageid"] = Guid.NewGuid();
                    return new RetrieveActivePathResponse()
                    {
                        Results = new ParameterCollection
                {
                    new KeyValuePair<string, object>("ProcessStages", new EntityCollection(new List<Entity> { activepath })),
                },
                    };
                }

                return null;
            };

            CasePostOperationSync plugin = new CasePostOperationSync();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// PreValidate Update With Complete Data Test Method with statuscode 60 with jarvis_act.
        /// </summary>
        [TestMethod]
        public void PostUpdateWithCompleteDataTestMethodStCode60Atc()
        {
            Entity incidentTarget = new Entity("incident");

            EntityReference users = new EntityReference("systemuser", Guid.NewGuid());
            users.Name = "MERCURIUS";

            incidentTarget.Id = Guid.Empty;

            incidentTarget["statuscode"] = new OptionSetValue(60);
            incidentTarget["incidentid"] = Guid.NewGuid();
            incidentTarget["caseorigincode"] = new OptionSetValue(2);
            incidentTarget["casetypecode"] = new OptionSetValue(2);
            incidentTarget["jarvis_mercuriusstatus"] = new OptionSetValue(200);
            incidentTarget["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
            incidentTarget["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["customerid"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["jarvis_registrationnumber"] = "123";
            incidentTarget["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incidentTarget["description"] = "This is for testing";
            incidentTarget["jarvis_location"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incidentTarget["jarvis_customerexpectations"] = "test";
            incidentTarget["jarvis_incidentnature"] = "[{\"id\":\"3efba61a-85ad-44b7-a187-7f54acaf7a67\"}]";
            incidentTarget["jarvis_callerrole"] = new OptionSetValue(4);
            incidentTarget["jarvis_callerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incidentTarget["isescalated"] = true;
            incidentTarget["jarvis_querydescription"] = "test";
            incidentTarget["jarvis_extrainformationonsolutionquery"] = "test";
            incidentTarget["jarvis_translationstatusalertdecision"] = new OptionSetValue(334030000);
            incidentTarget["jarvis_querydecision"] = "test";
            incidentTarget.Attributes["jarvis_eta"] = DateTime.Now;
            incidentTarget.Attributes["jarvis_atc"] = DateTime.Now;
            incidentTarget.Attributes["modifiedby"] = users;

            Entity incident = new Entity("incident");
            incident.Id = Guid.Empty;
            incident["statuscode"] = new OptionSetValue(60);
            incident["incidentid"] = new EntityReference("incident", Guid.NewGuid());
            incident["caseorigincode"] = new OptionSetValue(2);
            incident["casetypecode"] = new OptionSetValue(2);
            incident["jarvis_mercuriusstatus"] = new OptionSetValue(200);
            incident["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
            incident["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incident["customerid"] = new EntityReference("account", Guid.NewGuid());
            incident["jarvis_registrationnumber"] = "123";
            incident["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["description"] = "This is for testing";
            incident["jarvis_driverlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incident["jarvis_translationstatusreportedfault"] = new OptionSetValue(334030000);
            incident["jarvis_translationstatuslocation"] = new OptionSetValue(334030000);
            incident["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["jarvis_translationstatuscustomerexpectations"] = new OptionSetValue(334030000);
            incident["jarvis_location"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["jarvis_customerexpectations"] = "test";
            incident["ownerid"] = new EntityReference { LogicalName = "system_user", Name = "MERCURIUS" };
            incident["jarvis_sourceid"] = 232323232;
            incident["jarvis_callerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incident["jarvis_callerrole"] = new OptionSetValue(4);
            incident["createdby"] = users;
            incident["parentcaseid"] = new EntityReference("incident", Guid.NewGuid());
            incident["jarvis_translationstatusalertdescription"] = new OptionSetValue(334030000);
            incident["jarvis_translationstatusalertextrainformation"] = new OptionSetValue(334030000);
            incident["jarvis_translationstatusalertdecision"] = new OptionSetValue(334030000);
            incident["jarvis_extrainformationonsolutionquery"] = "test";
            incident["jarvis_querydecision"] = "test";
            incident["jarvis_onetimecustomerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incident["jarvis_onetimecustomercountry"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["jarvis_actualcausefault"] = "test";
            incident["jarvis_mileageafterrepair"] = 10;
            incident["jarvis_mileageunitafterrepair"] = new EntityReference("jarvis_mileageunit", Guid.NewGuid());
            incident.Attributes["jarvis_atc"] = DateTime.Now;
            //// Setting Input Parameters.
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", incidentTarget),
               };
            this.PluginExecutionContext.MessageNameGet = () => "Update";
            this.PluginExecutionContext.StageGet = () => 40;
            //// Setting Post and Pre Image
            this.PluginExecutionContext.PostEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PostImage", incident },
                };
            };
            incident["statuscode"] = new OptionSetValue(60);
            this.PluginExecutionContext.PreEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PreImage", incident },
                };
            };

            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();
                if (query.GetType().Name == "FetchExpression")
                {
                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_vasbreakdownprocess'>"))
                    {
                        Entity vasprocess = new Entity("jarvis_vasbreakdownprocess");
                        vasprocess["businessprocessflowinstanceid"] = Guid.NewGuid();
                        vasprocess["bpf_name"] = "test";
                        vasprocess["bpf_incidentid"] = Guid.NewGuid();
                        vasprocess["activestageid"] = Guid.NewGuid();
                        vasprocess["statecode"] = new OptionSetValue(1);
                        vasprocess["createdon"] = DateTime.UtcNow;
                        vasprocess["createdby"] = "test";
                        vasprocess.Id = Guid.NewGuid();
                        result.Entities.Add(vasprocess);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='incident'>"))
                    {
                        Entity incidentResult = new Entity("incident");
                        incidentResult[Constants.IncidentNature.IncidentIncidentNature_jarvisIincidentnatureid] = new AliasedValue("incidentNature", "incidentnatureid", Guid.NewGuid());
                        incidentResult["jarvis_hdrd"] = true;
                        incidentResult["jarvis_homedealer"] = new EntityReference("account", incident.Id);
                        incidentResult["CaseRD.jarvis_repairingdealer"] = new AliasedValue("account", "account", new EntityReference("account", incident.Id));
                        incidentResult.Id = Guid.NewGuid();
                        result.Entities.Add(incidentResult);
                        result.Entities.Add(incidentResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_configurationjarvis'>"))
                    {
                        Entity configResult = new Entity("jarvis_configurationjarvis");
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationcasestatuschange"] = true;
                        configResult["createdon"] = DateTime.UtcNow;
                        configResult.Id = Guid.NewGuid();
                        result.Entities.Add(configResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_language'>"))
                    {
                        Entity language = new Entity("jarvis_language");
                        language.Id = Guid.NewGuid();
                        language["jarvis_iso3languagecode6392t"] = "DEU";
                        language["jarvis_iso2languagecode6391"] = "DE";
                        language["jarvis_name"] = "DE";
                        result.Entities.Add(language);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_casesoftoffer'>"))
                    {
                        Entity casesoftoffer = new Entity("jarvis_casesoftoffer");
                        casesoftoffer.Id = Guid.NewGuid();
                        casesoftoffer["jarvis_name"] = "DEU";
                        casesoftoffer["jarvis_contractno"] = "DE";
                        casesoftoffer["jarvis_description"] = "DE";
                        casesoftoffer["jarvis_startdate"] = DateTime.UtcNow;
                        casesoftoffer["jarvis_expirydate"] = DateTime.UtcNow;
                        casesoftoffer["statuscode"] = new OptionSetValue(0);
                        casesoftoffer["jarvis_softoffercodelookup"] = new EntityReference("jarvis_softoffer", Guid.NewGuid());
                        result.Entities.Add(casesoftoffer);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_softoffer'>"))
                    {
                        Entity softoffer = new Entity("jarvis_softoffer");
                        softoffer.Id = Guid.NewGuid();
                        softoffer["jarvis_name"] = "DEU";
                        softoffer["jarvis_softoffercode"] = new EntityReference("jarvis_softoffer", Guid.NewGuid());
                        softoffer["jarvis_contractno"] = "DE";
                        softoffer["jarvis_startdate"] = DateTime.UtcNow;
                        softoffer["jarvis_expirydate"] = DateTime.UtcNow;
                        softoffer["statuscode"] = new OptionSetValue(0);
                        softoffer["jarvis_marketcode"] = "DE";
                        softoffer["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
                        result.Entities.Add(softoffer);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_repairinformation'>"))
                    {
                        Entity repairInfoResult = new Entity("jarvis_repairinformation");
                        repairInfoResult["jarvis_name"] = "testpassout";
                        repairInfoResult["createdon"] = DateTime.UtcNow;
                        repairInfoResult.Id = Guid.NewGuid();
                        result.Entities.Add(repairInfoResult);
                    }
                }

                return result;
            };

            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "incident")
                {
                    Entity inc = new Entity("incident");
                    inc.Id = guid;
                    inc["customerid"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_callerlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    inc["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    inc["jarvis_driverlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    inc["jarvis_translationstatusreportedfault"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatuslocation"] = new OptionSetValue(334030000);
                    inc["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    inc["jarvis_translationstatuscustomerexpectations"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatusalertdescription"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatusalertextrainformation"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatusalertdecision"] = new OptionSetValue(334030000);
                    return inc;
                }

                if (entityName == "jarvis_vehicle")
                {
                    Entity client = new Entity("jarvis_vehicle");
                    client.Id = new Guid("6186f7f1-5f09-ed11-82e5-002248934e82");
                    client["jarvis_brandid"] = new EntityReference("jarvis_brand", Guid.NewGuid());
                    client["jarvis_fuelpowertype"] = new EntityReference("jarvis_vehiclefuelpowertype", Guid.NewGuid());
                    return client;
                }

                if (entityName == "jarvis_country")
                {
                    Entity country = new Entity("jarvis_country");
                    country.Id = guid;
                    country["jarvis_iso2countrycode"] = "NL";
                    return country;
                }

                if (entityName == "jarvis_language")
                {
                    Entity language = new Entity("jarvis_language");
                    language.Id = guid;
                    language["jarvis_iso3languagecode6392t"] = "DEU";
                    language["jarvis_iso2languagecode6391"] = "DE";
                    return language;
                }

                if (entityName == "account")
                {
                    Entity account = new Entity("account");
                    account.Id = guid;
                    account["jarvis_language"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    account["jarvis_address1_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    return account;
                }

                if (entityName == "jarvis_brand")
                {
                    Entity client = new Entity("jarvis_brand");
                    client.Id = new Guid("6186f7f1-5f09-ed11-82e5-002248934e82");
                    client["jarvis_mercuriusbrandcode"] = "ted";
                    return client;
                }

                return null;
            };
            this.Service.ExecuteOrganizationRequest = processInstanceRequest =>
            {
                if (processInstanceRequest.RequestName.Contains("RetrieveProcessInstances"))
                {
                    return new RetrieveProcessInstancesResponse()
                    {
                        Results = new ParameterCollection
                {
                    new KeyValuePair<string, object>("Processes", new EntityCollection(new List<Entity> { incident })),
                },
                    };
                }

                if (processInstanceRequest.RequestName.Contains("RetrieveActivePath"))
                {
                    Entity activepath = new Entity("activepath");
                    activepath["stagename"] = "Waiting For Repair Start";
                    activepath["processstageid"] = Guid.NewGuid();
                    return new RetrieveActivePathResponse()
                    {
                        Results = new ParameterCollection
                {
                    new KeyValuePair<string, object>("ProcessStages", new EntityCollection(new List<Entity> { activepath })),
                },
                    };
                }

                return null;
            };

            CasePostOperationSync plugin = new CasePostOperationSync();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// PreValidate Update With Complete Data Test Method with statuscode 60 with No JED.
        /// </summary>
        [TestMethod]
        public void PostUpdateWithCompleteDataTestMethodStCode60NoJed()
        {
            Entity incidentTarget = new Entity("incident");

            EntityReference users = new EntityReference("systemuser", Guid.NewGuid());
            users.Name = "MERCURIUS";

            incidentTarget.Id = Guid.Empty;

            incidentTarget["statuscode"] = new OptionSetValue(60);
            incidentTarget["incidentid"] = Guid.NewGuid();
            incidentTarget["caseorigincode"] = new OptionSetValue(2);
            incidentTarget["casetypecode"] = new OptionSetValue(2);
            incidentTarget["jarvis_mercuriusstatus"] = new OptionSetValue(200);
            incidentTarget["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
            incidentTarget["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["customerid"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["jarvis_registrationnumber"] = "123";
            incidentTarget["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incidentTarget["description"] = "This is for testing";
            incidentTarget["jarvis_location"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incidentTarget["jarvis_customerexpectations"] = "test";
            incidentTarget["jarvis_incidentnature"] = "[{\"id\":\"3efba61a-85ad-44b7-a187-7f54acaf7a67\"}]";
            incidentTarget["jarvis_callerrole"] = new OptionSetValue(4);
            incidentTarget["jarvis_callerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incidentTarget["isescalated"] = true;
            incidentTarget["jarvis_querydescription"] = "test";
            incidentTarget["jarvis_extrainformationonsolutionquery"] = "test";
            incidentTarget["jarvis_translationstatusalertdecision"] = new OptionSetValue(334030000);
            incidentTarget["jarvis_querydecision"] = "test";
            incidentTarget.Attributes["jarvis_eta"] = DateTime.Now;
            incidentTarget.Attributes["jarvis_atc"] = DateTime.Now;
            incidentTarget.Attributes["modifiedby"] = users;

            Entity incident = new Entity("incident");
            incident.Id = Guid.Empty;
            incident["statuscode"] = new OptionSetValue(60);
            incident["incidentid"] = new EntityReference("incident", Guid.NewGuid());
            incident["caseorigincode"] = new OptionSetValue(2);
            incident["casetypecode"] = new OptionSetValue(2);
            incident["jarvis_mercuriusstatus"] = new OptionSetValue(200);
            incident["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
            incident["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incident["customerid"] = new EntityReference("account", Guid.NewGuid());
            incident["jarvis_registrationnumber"] = "123";
            incident["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["description"] = "This is for testing";
            incident["jarvis_driverlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incident["jarvis_translationstatusreportedfault"] = new OptionSetValue(334030000);
            incident["jarvis_translationstatuslocation"] = new OptionSetValue(334030000);
            incident["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["jarvis_translationstatuscustomerexpectations"] = new OptionSetValue(334030000);
            incident["jarvis_location"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["jarvis_customerexpectations"] = "test";
            incident["ownerid"] = new EntityReference { LogicalName = "system_user", Name = "MERCURIUS" };
            incident["jarvis_sourceid"] = 232323232;
            incident["jarvis_callerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incident["jarvis_callerrole"] = new OptionSetValue(4);
            incident["createdby"] = users;
            incident["parentcaseid"] = new EntityReference("incident", Guid.NewGuid());
            incident["jarvis_translationstatusalertdescription"] = new OptionSetValue(334030000);
            incident["jarvis_translationstatusalertextrainformation"] = new OptionSetValue(334030000);
            incident["jarvis_translationstatusalertdecision"] = new OptionSetValue(334030000);
            incident["jarvis_extrainformationonsolutionquery"] = "test";
            incident["jarvis_querydecision"] = "test";
            incident["jarvis_onetimecustomerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incident["jarvis_onetimecustomercountry"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident.Attributes["jarvis_etc"] = DateTime.Now;
            //// Setting Input Parameters.
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", incidentTarget),
               };
            this.PluginExecutionContext.MessageNameGet = () => "Update";
            this.PluginExecutionContext.StageGet = () => 40;
            //// Setting Post and Pre Image
            this.PluginExecutionContext.PostEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PostImage", incident },
                };
            };
            incident["statuscode"] = new OptionSetValue(50);
            this.PluginExecutionContext.PreEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PreImage", incident },
                };
            };

            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();
                if (query.GetType().Name == "FetchExpression")
                {
                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_vasbreakdownprocess'>"))
                    {
                        Entity vasprocess = new Entity("jarvis_vasbreakdownprocess");
                        vasprocess["businessprocessflowinstanceid"] = Guid.NewGuid();
                        vasprocess["bpf_name"] = "test";
                        vasprocess["bpf_incidentid"] = Guid.NewGuid();
                        vasprocess["activestageid"] = Guid.NewGuid();
                        vasprocess["statecode"] = new OptionSetValue(1);
                        vasprocess["createdon"] = DateTime.UtcNow;
                        vasprocess["createdby"] = "test";
                        vasprocess.Id = Guid.NewGuid();
                        result.Entities.Add(vasprocess);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='incident'>"))
                    {
                        Entity incidentResult = new Entity("incident");
                        incidentResult[Constants.IncidentNature.IncidentIncidentNature_jarvisIincidentnatureid] = new AliasedValue("incidentNature", "incidentnatureid", Guid.NewGuid());
                        incidentResult["jarvis_hdrd"] = true;
                        incidentResult["jarvis_homedealer"] = new EntityReference("account", incident.Id);
                        incidentResult["CaseRD.jarvis_repairingdealer"] = new AliasedValue("account", "account", new EntityReference("account", incident.Id));
                        incidentResult.Id = Guid.NewGuid();
                        result.Entities.Add(incidentResult);
                        result.Entities.Add(incidentResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_configurationjarvis'>"))
                    {
                        Entity configResult = new Entity("jarvis_configurationjarvis");
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationcasestatuschange"] = true;
                        configResult["createdon"] = DateTime.UtcNow;
                        configResult.Id = Guid.NewGuid();
                        result.Entities.Add(configResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_language'>"))
                    {
                        Entity language = new Entity("jarvis_language");
                        language.Id = Guid.NewGuid();
                        language["jarvis_iso3languagecode6392t"] = "DEU";
                        language["jarvis_iso2languagecode6391"] = "DE";
                        language["jarvis_name"] = "DE";
                        result.Entities.Add(language);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_casesoftoffer'>"))
                    {
                        Entity casesoftoffer = new Entity("jarvis_casesoftoffer");
                        casesoftoffer.Id = Guid.NewGuid();
                        casesoftoffer["jarvis_name"] = "DEU";
                        casesoftoffer["jarvis_contractno"] = "DE";
                        casesoftoffer["jarvis_description"] = "DE";
                        casesoftoffer["jarvis_startdate"] = DateTime.UtcNow;
                        casesoftoffer["jarvis_expirydate"] = DateTime.UtcNow;
                        casesoftoffer["statuscode"] = new OptionSetValue(0);
                        casesoftoffer["jarvis_softoffercodelookup"] = new EntityReference("jarvis_softoffer", Guid.NewGuid());
                        result.Entities.Add(casesoftoffer);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_softoffer'>"))
                    {
                        Entity softoffer = new Entity("jarvis_softoffer");
                        softoffer.Id = Guid.NewGuid();
                        softoffer["jarvis_name"] = "DEU";
                        softoffer["jarvis_softoffercode"] = new EntityReference("jarvis_softoffer", Guid.NewGuid());
                        softoffer["jarvis_contractno"] = "DE";
                        softoffer["jarvis_startdate"] = DateTime.UtcNow;
                        softoffer["jarvis_expirydate"] = DateTime.UtcNow;
                        softoffer["statuscode"] = new OptionSetValue(0);
                        softoffer["jarvis_marketcode"] = "DE";
                        softoffer["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
                        result.Entities.Add(softoffer);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_repairinformation'>"))
                    {
                        Entity repairInfoResult = new Entity("jarvis_repairinformation");
                        repairInfoResult["jarvis_name"] = "testpassout";
                        repairInfoResult["createdon"] = DateTime.UtcNow;
                        repairInfoResult.Id = Guid.NewGuid();
                        result.Entities.Add(repairInfoResult);
                    }
                }

                return result;
            };

            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "incident")
                {
                    Entity inc = new Entity("incident");
                    inc.Id = guid;
                    inc["customerid"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_callerlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    inc["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    inc["jarvis_driverlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    inc["jarvis_translationstatusreportedfault"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatuslocation"] = new OptionSetValue(334030000);
                    inc["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    inc["jarvis_translationstatuscustomerexpectations"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatusalertdescription"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatusalertextrainformation"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatusalertdecision"] = new OptionSetValue(334030000);
                    return inc;
                }

                if (entityName == "jarvis_vehicle")
                {
                    Entity client = new Entity("jarvis_vehicle");
                    client.Id = new Guid("6186f7f1-5f09-ed11-82e5-002248934e82");
                    client["jarvis_brandid"] = new EntityReference("jarvis_brand", Guid.NewGuid());
                    client["jarvis_fuelpowertype"] = new EntityReference("jarvis_vehiclefuelpowertype", Guid.NewGuid());
                    return client;
                }

                if (entityName == "jarvis_country")
                {
                    Entity country = new Entity("jarvis_country");
                    country.Id = guid;
                    country["jarvis_iso2countrycode"] = "NL";
                    return country;
                }

                if (entityName == "jarvis_language")
                {
                    Entity language = new Entity("jarvis_language");
                    language.Id = guid;
                    language["jarvis_iso3languagecode6392t"] = "DEU";
                    language["jarvis_iso2languagecode6391"] = "DE";
                    return language;
                }

                if (entityName == "account")
                {
                    Entity account = new Entity("account");
                    account.Id = guid;
                    account["jarvis_language"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    account["jarvis_address1_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    return account;
                }

                if (entityName == "jarvis_brand")
                {
                    Entity client = new Entity("jarvis_brand");
                    client.Id = new Guid("6186f7f1-5f09-ed11-82e5-002248934e82");
                    client["jarvis_mercuriusbrandcode"] = "ted";
                    return client;
                }

                return null;
            };
            this.Service.ExecuteOrganizationRequest = processInstanceRequest =>
            {
                if (processInstanceRequest.RequestName.Contains("RetrieveProcessInstances"))
                {
                    return new RetrieveProcessInstancesResponse()
                    {
                        Results = new ParameterCollection
                {
                    new KeyValuePair<string, object>("Processes", new EntityCollection(new List<Entity> { incident })),
                },
                    };
                }

                if (processInstanceRequest.RequestName.Contains("RetrieveActivePath"))
                {
                    Entity activepath = new Entity("activepath");
                    activepath["stagename"] = "Waiting For Repair Start";
                    activepath["processstageid"] = Guid.NewGuid();
                    return new RetrieveActivePathResponse()
                    {
                        Results = new ParameterCollection
                {
                    new KeyValuePair<string, object>("ProcessStages", new EntityCollection(new List<Entity> { activepath })),
                },
                    };
                }

                return null;
            };

            CasePostOperationSync plugin = new CasePostOperationSync();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Post Update With Complete Data Test Method with depth 7.
        /// </summary>
        [TestMethod]
        public void PostUpdateWithCompleteDataTestMethodwithDepth()
        {
            Entity incident = new Entity("incident");
            incident.Id = Guid.Empty;
            incident["statuscode"] = new OptionSetValue(10);
            incident["caseorigincode"] = new OptionSetValue(2);
            incident["jarvis_mercuriusstatus"] = new OptionSetValue(200);
            incident["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
            incident["jarvis_homedelaer"] = new EntityReference("account", Guid.NewGuid());
            incident["customerid"] = new EntityReference("account", Guid.NewGuid());
            incident["jarvis_registrationnumber"] = "123";
            //// Setting Input Parameters.
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", incident),
               };
            this.PluginExecutionContext.MessageNameGet = () => "Update";
            this.PluginExecutionContext.StageGet = () => 40;
            this.PluginExecutionContext.DepthGet = () => 7;
            CasePostOperationSync plugin = new CasePostOperationSync();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// PreValidate Update With Complete Data Test Method with statuscode 150.
        /// </summary>
        [TestMethod]
        public void PostUpdateWithCompleteDataTestMethodStCode150()
        {
            Entity incidentTarget = new Entity("incident");

            EntityReference users = new EntityReference("systemuser", Guid.NewGuid());
            users.Name = "MERCURIUS";

            incidentTarget.Id = Guid.Empty;

            incidentTarget["statuscode"] = new OptionSetValue(150);
            incidentTarget["incidentid"] = Guid.NewGuid();
            incidentTarget["caseorigincode"] = new OptionSetValue(2);
            incidentTarget["casetypecode"] = new OptionSetValue(2);
            incidentTarget["jarvis_mercuriusstatus"] = new OptionSetValue(200);
            incidentTarget["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
            incidentTarget["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["customerid"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["jarvis_registrationnumber"] = "123";
            incidentTarget["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incidentTarget["description"] = "This is for testing";
            incidentTarget["jarvis_location"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incidentTarget["jarvis_customerexpectations"] = "test";
            incidentTarget["jarvis_incidentnature"] = "[{\"id\":\"3efba61a-85ad-44b7-a187-7f54acaf7a67\"}]";
            incidentTarget["jarvis_callerrole"] = new OptionSetValue(4);
            incidentTarget["jarvis_callerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incidentTarget["isescalated"] = true;
            incidentTarget["jarvis_querydescription"] = "test";
            incidentTarget["jarvis_extrainformationonsolutionquery"] = "test";
            incidentTarget["jarvis_translationstatusalertdecision"] = new OptionSetValue(334030000);
            incidentTarget["jarvis_querydecision"] = "test";
            incidentTarget.Attributes["jarvis_eta"] = DateTime.Now;
            incidentTarget.Attributes["jarvis_atc"] = DateTime.Now;
            incidentTarget.Attributes["modifiedby"] = users;

            Entity incident = new Entity("incident");
            incident.Id = Guid.Empty;
            incident["statuscode"] = new OptionSetValue(150);
            incident["incidentid"] = new EntityReference("incident", Guid.NewGuid());
            incident["caseorigincode"] = new OptionSetValue(2);
            incident["casetypecode"] = new OptionSetValue(2);
            incident["jarvis_mercuriusstatus"] = new OptionSetValue(200);
            incident["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
            incident["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incident["customerid"] = new EntityReference("account", Guid.NewGuid());
            incident["jarvis_registrationnumber"] = "123";
            incident["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["description"] = "This is for testing";
            incident["jarvis_driverlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incident["jarvis_translationstatusreportedfault"] = new OptionSetValue(334030000);
            incident["jarvis_translationstatuslocation"] = new OptionSetValue(334030000);
            incident["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["jarvis_translationstatuscustomerexpectations"] = new OptionSetValue(334030000);
            incident["jarvis_location"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["jarvis_customerexpectations"] = "test";
            incident["ownerid"] = new EntityReference { LogicalName = "system_user", Name = "MERCURIUS" };
            incident["jarvis_sourceid"] = 232323232;
            incident["jarvis_callerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incident["jarvis_callerrole"] = new OptionSetValue(4);
            incident["createdby"] = users;
            incident["parentcaseid"] = new EntityReference("incident", Guid.NewGuid());
            incident["jarvis_translationstatusalertdescription"] = new OptionSetValue(334030000);
            incident["jarvis_translationstatusalertextrainformation"] = new OptionSetValue(334030000);
            incident["jarvis_translationstatusalertdecision"] = new OptionSetValue(334030000);
            incident["jarvis_extrainformationonsolutionquery"] = "test";
            incident["jarvis_querydecision"] = "test";
            incident["jarvis_onetimecustomerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incident["jarvis_onetimecustomercountry"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident.Attributes["jarvis_etc"] = DateTime.Now;
            //// Setting Input Parameters.
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", incidentTarget),
               };
            this.PluginExecutionContext.MessageNameGet = () => "Update";
            this.PluginExecutionContext.StageGet = () => 40;
            //// Setting Post and Pre Image
            this.PluginExecutionContext.PostEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PostImage", incident },
                };
            };
            this.PluginExecutionContext.PreEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PreImage", incident },
                };
            };

            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();
                if (query.GetType().Name == "FetchExpression")
                {
                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_vasbreakdownprocess'>"))
                    {
                        Entity vasprocess = new Entity("jarvis_vasbreakdownprocess");
                        vasprocess["businessprocessflowinstanceid"] = Guid.NewGuid();
                        vasprocess["bpf_name"] = "test";
                        vasprocess["bpf_incidentid"] = Guid.NewGuid();
                        vasprocess["activestageid"] = Guid.NewGuid();
                        vasprocess["statecode"] = new OptionSetValue(1);
                        vasprocess["createdon"] = DateTime.UtcNow;
                        vasprocess["createdby"] = "test";
                        vasprocess.Id = Guid.NewGuid();
                        result.Entities.Add(vasprocess);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='incident'>"))
                    {
                        Entity incidentResult = new Entity("incident");
                        incidentResult[Constants.IncidentNature.IncidentIncidentNature_jarvisIincidentnatureid] = new AliasedValue("incidentNature", "incidentnatureid", Guid.NewGuid());
                        incidentResult["jarvis_hdrd"] = true;
                        incidentResult["jarvis_homedealer"] = new EntityReference("account", incident.Id);
                        incidentResult["CaseRD.jarvis_repairingdealer"] = new AliasedValue("account", "account", new EntityReference("account", incident.Id));
                        incidentResult.Id = Guid.NewGuid();
                        result.Entities.Add(incidentResult);
                        result.Entities.Add(incidentResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_configurationjarvis'>"))
                    {
                        Entity configResult = new Entity("jarvis_configurationjarvis");
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationcasestatuschange"] = true;
                        configResult["createdon"] = DateTime.UtcNow;
                        configResult.Id = Guid.NewGuid();
                        result.Entities.Add(configResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_language'>"))
                    {
                        Entity language = new Entity("jarvis_language");
                        language.Id = Guid.NewGuid();
                        language["jarvis_iso3languagecode6392t"] = "DEU";
                        language["jarvis_iso2languagecode6391"] = "DE";
                        language["jarvis_name"] = "DE";
                        result.Entities.Add(language);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_casesoftoffer'>"))
                    {
                        Entity casesoftoffer = new Entity("jarvis_casesoftoffer");
                        casesoftoffer.Id = Guid.NewGuid();
                        casesoftoffer["jarvis_name"] = "DEU";
                        casesoftoffer["jarvis_contractno"] = "DE";
                        casesoftoffer["jarvis_description"] = "DE";
                        casesoftoffer["jarvis_startdate"] = DateTime.UtcNow;
                        casesoftoffer["jarvis_expirydate"] = DateTime.UtcNow;
                        casesoftoffer["statuscode"] = new OptionSetValue(0);
                        casesoftoffer["jarvis_softoffercodelookup"] = new EntityReference("jarvis_softoffer", Guid.NewGuid());
                        result.Entities.Add(casesoftoffer);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_softoffer'>"))
                    {
                        Entity softoffer = new Entity("jarvis_softoffer");
                        softoffer.Id = Guid.NewGuid();
                        softoffer["jarvis_name"] = "DEU";
                        softoffer["jarvis_softoffercode"] = new EntityReference("jarvis_softoffer", Guid.NewGuid());
                        softoffer["jarvis_contractno"] = "DE";
                        softoffer["jarvis_startdate"] = DateTime.UtcNow;
                        softoffer["jarvis_expirydate"] = DateTime.UtcNow;
                        softoffer["statuscode"] = new OptionSetValue(0);
                        softoffer["jarvis_marketcode"] = "DE";
                        softoffer["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
                        result.Entities.Add(softoffer);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_repairinformation'>"))
                    {
                        Entity repairInfoResult = new Entity("jarvis_repairinformation");
                        repairInfoResult["jarvis_name"] = "testpassout";
                        repairInfoResult["createdon"] = DateTime.UtcNow;
                        repairInfoResult.Id = Guid.NewGuid();
                        result.Entities.Add(repairInfoResult);
                    }
                }

                return result;
            };

            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "incident")
                {
                    Entity inc = new Entity("incident");
                    inc.Id = guid;
                    inc["customerid"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_callerlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    inc["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    inc["jarvis_driverlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    inc["jarvis_translationstatusreportedfault"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatuslocation"] = new OptionSetValue(334030000);
                    inc["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    inc["jarvis_translationstatuscustomerexpectations"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatusalertdescription"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatusalertextrainformation"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatusalertdecision"] = new OptionSetValue(334030000);
                    return inc;
                }

                if (entityName == "jarvis_vehicle")
                {
                    Entity client = new Entity("jarvis_vehicle");
                    client.Id = new Guid("6186f7f1-5f09-ed11-82e5-002248934e82");
                    client["jarvis_brandid"] = new EntityReference("jarvis_brand", Guid.NewGuid());
                    client["jarvis_fuelpowertype"] = new EntityReference("jarvis_vehiclefuelpowertype", Guid.NewGuid());
                    return client;
                }

                if (entityName == "jarvis_country")
                {
                    Entity country = new Entity("jarvis_country");
                    country.Id = guid;
                    country["jarvis_iso2countrycode"] = "NL";
                    return country;
                }

                if (entityName == "jarvis_language")
                {
                    Entity language = new Entity("jarvis_language");
                    language.Id = guid;
                    language["jarvis_iso3languagecode6392t"] = "DEU";
                    language["jarvis_iso2languagecode6391"] = "DE";
                    return language;
                }

                if (entityName == "account")
                {
                    Entity account = new Entity("account");
                    account.Id = guid;
                    account["jarvis_language"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    account["jarvis_address1_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    return account;
                }

                if (entityName == "jarvis_brand")
                {
                    Entity client = new Entity("jarvis_brand");
                    client.Id = new Guid("6186f7f1-5f09-ed11-82e5-002248934e82");
                    client["jarvis_mercuriusbrandcode"] = "ted";
                    return client;
                }

                return null;
            };
            this.Service.ExecuteOrganizationRequest = processInstanceRequest =>
            {
                if (processInstanceRequest.RequestName.Contains("RetrieveProcessInstances"))
                {
                    return new RetrieveProcessInstancesResponse()
                    {
                        Results = new ParameterCollection
                {
                    new KeyValuePair<string, object>("Processes", new EntityCollection(new List<Entity> { incident })),
                },
                    };
                }

                if (processInstanceRequest.RequestName.Contains("RetrieveActivePath"))
                {
                    Entity activepath = new Entity("activepath");
                    activepath["stagename"] = "Waiting For Repair Start";
                    activepath["processstageid"] = Guid.NewGuid();
                    return new RetrieveActivePathResponse()
                    {
                        Results = new ParameterCollection
                {
                    new KeyValuePair<string, object>("ProcessStages", new EntityCollection(new List<Entity> { activepath })),
                },
                    };
                }

                return null;
            };

            CasePostOperationSync plugin = new CasePostOperationSync();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// PreValidate Update With Complete Data Test Method with statuscode 90.
        /// </summary>
        [TestMethod]
        public void PostUpdateWithCompleteDataTestMethodStCode90()
        {
            Entity incidentTarget = new Entity("incident");

            EntityReference users = new EntityReference("systemuser", Guid.NewGuid());
            users.Name = "MERCURIUS";

            incidentTarget.Id = Guid.Empty;

            incidentTarget["statuscode"] = new OptionSetValue(90);
            incidentTarget["incidentid"] = Guid.NewGuid();
            incidentTarget["caseorigincode"] = new OptionSetValue(2);
            incidentTarget["casetypecode"] = new OptionSetValue(2);
            incidentTarget["jarvis_mercuriusstatus"] = new OptionSetValue(200);
            incidentTarget["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
            incidentTarget["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["customerid"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["jarvis_registrationnumber"] = "123";
            incidentTarget["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incidentTarget["description"] = "This is for testing";
            incidentTarget["jarvis_location"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incidentTarget["jarvis_customerexpectations"] = "test";
            incidentTarget["jarvis_incidentnature"] = "[{\"id\":\"3efba61a-85ad-44b7-a187-7f54acaf7a67\"}]";
            incidentTarget["jarvis_callerrole"] = new OptionSetValue(4);
            incidentTarget["jarvis_callerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incidentTarget["isescalated"] = true;
            incidentTarget["jarvis_querydescription"] = "test";
            incidentTarget["jarvis_extrainformationonsolutionquery"] = "test";
            incidentTarget["jarvis_translationstatusalertdecision"] = new OptionSetValue(334030000);
            incidentTarget["jarvis_querydecision"] = "test";
            incidentTarget.Attributes["jarvis_eta"] = DateTime.Now;
            incidentTarget.Attributes["jarvis_atc"] = DateTime.Now;
            incidentTarget["modifiedby"] = users;

            Entity incident = new Entity("incident");
            incident.Id = Guid.Empty;
            incident["statuscode"] = new OptionSetValue(90);
            incident["statecode"] = new OptionSetValue(0);
            incident["incidentid"] = new EntityReference("incident", Guid.NewGuid());
            incident["caseorigincode"] = new OptionSetValue(2);
            incident["casetypecode"] = new OptionSetValue(2);
            incident["jarvis_mercuriusstatus"] = new OptionSetValue(200);
            incident["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
            incident["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incident["customerid"] = new EntityReference("account", Guid.NewGuid());
            incident["jarvis_registrationnumber"] = "123";
            incident["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["description"] = "This is for testing";
            incident["jarvis_driverlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incident["jarvis_translationstatusreportedfault"] = new OptionSetValue(334030000);
            incident["jarvis_translationstatuslocation"] = new OptionSetValue(334030000);
            incident["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["jarvis_translationstatuscustomerexpectations"] = new OptionSetValue(334030000);
            incident["jarvis_location"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["jarvis_customerexpectations"] = "test";
            incident["ownerid"] = new EntityReference { LogicalName = "system_user", Name = "MERCURIUS" };
            incident["jarvis_sourceid"] = 232323232;
            incident["jarvis_callerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incident["jarvis_callerrole"] = new OptionSetValue(4);
            incident["createdby"] = users;
            incident["parentcaseid"] = new EntityReference("incident", Guid.NewGuid());
            incident["jarvis_translationstatusalertdescription"] = new OptionSetValue(334030000);
            incident["jarvis_translationstatusalertextrainformation"] = new OptionSetValue(334030000);
            incident["jarvis_translationstatusalertdecision"] = new OptionSetValue(334030000);
            incident["jarvis_extrainformationonsolutionquery"] = "test";
            incident["jarvis_querydecision"] = "test";
            incident["jarvis_onetimecustomerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incident["jarvis_onetimecustomercountry"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident.Attributes["jarvis_etc"] = DateTime.Now;
            //// Setting Input Parameters.
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", incidentTarget),
               };
            this.PluginExecutionContext.MessageNameGet = () => "Update";
            this.PluginExecutionContext.StageGet = () => 40;
            //// Setting Post and Pre Image
            this.PluginExecutionContext.PostEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PostImage", incident },
                };
            };
            incident["statuscode"] = new OptionSetValue(80);
            this.PluginExecutionContext.PreEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PreImage", incident },
                };
            };

            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();
                if (query.GetType().Name == "FetchExpression")
                {
                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_vasbreakdownprocess'>"))
                    {
                        Entity vasprocess = new Entity("jarvis_vasbreakdownprocess");
                        vasprocess["businessprocessflowinstanceid"] = Guid.NewGuid();
                        vasprocess["bpf_name"] = "test";
                        vasprocess["bpf_incidentid"] = Guid.NewGuid();
                        vasprocess["activestageid"] = Guid.NewGuid();
                        vasprocess["statecode"] = new OptionSetValue(1);
                        vasprocess["createdon"] = DateTime.UtcNow;
                        vasprocess["createdby"] = "test";
                        vasprocess.Id = Guid.NewGuid();
                        result.Entities.Add(vasprocess);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='incident'>"))
                    {
                        Entity incidentResult = new Entity("incident");
                        incidentResult[Constants.IncidentNature.IncidentIncidentNature_jarvisIincidentnatureid] = new AliasedValue("incidentNature", "incidentnatureid", Guid.NewGuid());
                        incidentResult["jarvis_hdrd"] = true;
                        incidentResult["jarvis_homedealer"] = new EntityReference("account", incident.Id);
                        incidentResult["CaseRD.jarvis_repairingdealer"] = new AliasedValue("account", "account", new EntityReference("account", incident.Id));
                        incidentResult.Id = Guid.NewGuid();
                        result.Entities.Add(incidentResult);
                        result.Entities.Add(incidentResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_configurationjarvis'>"))
                    {
                        Entity configResult = new Entity("jarvis_configurationjarvis");
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationcasestatuschange"] = true;
                        configResult["createdon"] = DateTime.UtcNow;
                        configResult.Id = Guid.NewGuid();
                        result.Entities.Add(configResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_language'>"))
                    {
                        Entity language = new Entity("jarvis_language");
                        language.Id = Guid.NewGuid();
                        language["jarvis_iso3languagecode6392t"] = "DEU";
                        language["jarvis_iso2languagecode6391"] = "DE";
                        language["jarvis_name"] = "DE";
                        result.Entities.Add(language);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_casesoftoffer'>"))
                    {
                        Entity casesoftoffer = new Entity("jarvis_casesoftoffer");
                        casesoftoffer.Id = Guid.NewGuid();
                        casesoftoffer["jarvis_name"] = "DEU";
                        casesoftoffer["jarvis_contractno"] = "DE";
                        casesoftoffer["jarvis_description"] = "DE";
                        casesoftoffer["jarvis_startdate"] = DateTime.UtcNow;
                        casesoftoffer["jarvis_expirydate"] = DateTime.UtcNow;
                        casesoftoffer["statuscode"] = new OptionSetValue(0);
                        casesoftoffer["jarvis_softoffercodelookup"] = new EntityReference("jarvis_softoffer", Guid.NewGuid());
                        result.Entities.Add(casesoftoffer);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_softoffer'>"))
                    {
                        Entity softoffer = new Entity("jarvis_softoffer");
                        softoffer.Id = Guid.NewGuid();
                        softoffer["jarvis_name"] = "DEU";
                        softoffer["jarvis_softoffercode"] = new EntityReference("jarvis_softoffer", Guid.NewGuid());
                        softoffer["jarvis_contractno"] = "DE";
                        softoffer["jarvis_startdate"] = DateTime.UtcNow;
                        softoffer["jarvis_expirydate"] = DateTime.UtcNow;
                        softoffer["statuscode"] = new OptionSetValue(0);
                        softoffer["jarvis_marketcode"] = "DE";
                        softoffer["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
                        result.Entities.Add(softoffer);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_repairinformation'>"))
                    {
                        Entity repairInfoResult = new Entity("jarvis_repairinformation");
                        repairInfoResult["jarvis_name"] = "testpassout";
                        repairInfoResult["createdon"] = DateTime.UtcNow;
                        repairInfoResult.Id = Guid.NewGuid();
                        result.Entities.Add(repairInfoResult);
                    }
                }

                return result;
            };

            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "incident")
                {
                    Entity inc = new Entity("incident");
                    inc.Id = guid;
                    inc["customerid"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_callerlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    inc["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    inc["jarvis_driverlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    inc["jarvis_translationstatusreportedfault"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatuslocation"] = new OptionSetValue(334030000);
                    inc["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    inc["jarvis_translationstatuscustomerexpectations"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatusalertdescription"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatusalertextrainformation"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatusalertdecision"] = new OptionSetValue(334030000);
                    return inc;
                }

                if (entityName == "jarvis_vehicle")
                {
                    Entity client = new Entity("jarvis_vehicle");
                    client.Id = new Guid("6186f7f1-5f09-ed11-82e5-002248934e82");
                    client["jarvis_brandid"] = new EntityReference("jarvis_brand", Guid.NewGuid());
                    client["jarvis_fuelpowertype"] = new EntityReference("jarvis_vehiclefuelpowertype", Guid.NewGuid());
                    return client;
                }

                if (entityName == "jarvis_country")
                {
                    Entity country = new Entity("jarvis_country");
                    country.Id = guid;
                    country["jarvis_iso2countrycode"] = "NL";
                    return country;
                }

                if (entityName == "jarvis_language")
                {
                    Entity language = new Entity("jarvis_language");
                    language.Id = guid;
                    language["jarvis_iso3languagecode6392t"] = "DEU";
                    language["jarvis_iso2languagecode6391"] = "DE";
                    return language;
                }

                if (entityName == "account")
                {
                    Entity account = new Entity("account");
                    account.Id = guid;
                    account["jarvis_language"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    account["jarvis_address1_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    return account;
                }

                if (entityName == "jarvis_brand")
                {
                    Entity client = new Entity("jarvis_brand");
                    client.Id = new Guid("6186f7f1-5f09-ed11-82e5-002248934e82");
                    client["jarvis_mercuriusbrandcode"] = "ted";
                    return client;
                }

                return null;
            };
            this.Service.ExecuteOrganizationRequest = processInstanceRequest =>
            {
                if (processInstanceRequest.RequestName.Contains("RetrieveProcessInstances"))
                {
                    return new RetrieveProcessInstancesResponse()
                    {
                        Results = new ParameterCollection
                {
                    new KeyValuePair<string, object>("Processes", new EntityCollection(new List<Entity> { incident })),
                },
                    };
                }

                if (processInstanceRequest.RequestName.Contains("RetrieveActivePath"))
                {
                    Entity activepath = new Entity("activepath");
                    activepath["stagename"] = "Waiting For Repair Start";
                    activepath["processstageid"] = Guid.NewGuid();
                    return new RetrieveActivePathResponse()
                    {
                        Results = new ParameterCollection
                {
                    new KeyValuePair<string, object>("ProcessStages", new EntityCollection(new List<Entity> { activepath })),
                },
                    };
                }

                return null;
            };

            CasePostOperationSync plugin = new CasePostOperationSync();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// PreValidate Update With Complete Data Test Method with statuscode 120.
        /// </summary>
        [TestMethod]
        public void PostUpdateWithCompleteDataTestMethodStCode120()
        {
            Entity incidentTarget = new Entity("incident");

            EntityReference users = new EntityReference("systemuser", Guid.NewGuid());
            users.Name = "MERCURIUS";

            incidentTarget.Id = Guid.Empty;

            incidentTarget["statuscode"] = new OptionSetValue(120);
            incidentTarget["incidentid"] = Guid.NewGuid();
            incidentTarget["caseorigincode"] = new OptionSetValue(2);
            incidentTarget["casetypecode"] = new OptionSetValue(2);
            incidentTarget["jarvis_mercuriusstatus"] = new OptionSetValue(200);
            incidentTarget["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
            incidentTarget["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["customerid"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["jarvis_registrationnumber"] = "123";
            incidentTarget["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incidentTarget["description"] = "This is for testing";
            incidentTarget["jarvis_location"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incidentTarget["jarvis_customerexpectations"] = "test";
            incidentTarget["jarvis_incidentnature"] = "[{\"id\":\"3efba61a-85ad-44b7-a187-7f54acaf7a67\"}]";
            incidentTarget["jarvis_callerrole"] = new OptionSetValue(4);
            incidentTarget["jarvis_callerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incidentTarget["isescalated"] = true;
            incidentTarget["jarvis_querydescription"] = "test";
            incidentTarget["jarvis_extrainformationonsolutionquery"] = "test";
            incidentTarget["jarvis_translationstatusalertdecision"] = new OptionSetValue(334030000);
            incidentTarget["jarvis_querydecision"] = "test";
            incidentTarget.Attributes["jarvis_eta"] = DateTime.Now;
            incidentTarget.Attributes["jarvis_atc"] = DateTime.Now;
            incidentTarget.Attributes["modifiedby"] = users;

            Entity incident = new Entity("incident");
            incident.Id = Guid.Empty;
            incident["statuscode"] = new OptionSetValue(120);
            incident["incidentid"] = new EntityReference("incident", Guid.NewGuid());
            incident["caseorigincode"] = new OptionSetValue(2);
            incident["casetypecode"] = new OptionSetValue(2);
            incident["jarvis_mercuriusstatus"] = new OptionSetValue(200);
            incident["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
            incident["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incident["customerid"] = new EntityReference("account", Guid.NewGuid());
            incident["jarvis_registrationnumber"] = "123";
            incident["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["description"] = "This is for testing";
            incident["jarvis_driverlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incident["jarvis_translationstatusreportedfault"] = new OptionSetValue(334030000);
            incident["jarvis_translationstatuslocation"] = new OptionSetValue(334030000);
            incident["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["jarvis_translationstatuscustomerexpectations"] = new OptionSetValue(334030000);
            incident["jarvis_location"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["jarvis_customerexpectations"] = "test";
            incident["ownerid"] = new EntityReference { LogicalName = "system_user", Name = "MERCURIUS" };
            incident["jarvis_sourceid"] = 232323232;
            incident["jarvis_callerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incident["jarvis_callerrole"] = new OptionSetValue(4);
            incident["createdby"] = users;
            incident["parentcaseid"] = new EntityReference("incident", Guid.NewGuid());
            incident["jarvis_translationstatusalertdescription"] = new OptionSetValue(334030000);
            incident["jarvis_translationstatusalertextrainformation"] = new OptionSetValue(334030000);
            incident["jarvis_translationstatusalertdecision"] = new OptionSetValue(334030000);
            incident["jarvis_extrainformationonsolutionquery"] = "test";
            incident["jarvis_querydecision"] = "test";
            incident["jarvis_onetimecustomerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incident["jarvis_onetimecustomercountry"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident.Attributes["jarvis_etc"] = DateTime.Now;
            //// Setting Input Parameters.
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", incidentTarget),
               };
            this.PluginExecutionContext.MessageNameGet = () => "Update";
            this.PluginExecutionContext.StageGet = () => 40;
            //// Setting Post and Pre Image
            this.PluginExecutionContext.PostEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PostImage", incident },
                };
            };
            this.PluginExecutionContext.PreEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PreImage", incident },
                };
            };

            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();
                if (query.GetType().Name == "FetchExpression")
                {
                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_vasbreakdownprocess'>"))
                    {
                        Entity vasprocess = new Entity("jarvis_vasbreakdownprocess");
                        vasprocess["businessprocessflowinstanceid"] = Guid.NewGuid();
                        vasprocess["bpf_name"] = "test";
                        vasprocess["bpf_incidentid"] = Guid.NewGuid();
                        vasprocess["activestageid"] = Guid.NewGuid();
                        vasprocess["statecode"] = new OptionSetValue(1);
                        vasprocess["createdon"] = DateTime.UtcNow;
                        vasprocess["createdby"] = "test";
                        vasprocess.Id = Guid.NewGuid();
                        result.Entities.Add(vasprocess);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='incident'>"))
                    {
                        Entity incidentResult = new Entity("incident");
                        incidentResult[Constants.IncidentNature.IncidentIncidentNature_jarvisIincidentnatureid] = new AliasedValue("incidentNature", "incidentnatureid", Guid.NewGuid());
                        incidentResult["jarvis_hdrd"] = true;
                        incidentResult["jarvis_homedealer"] = new EntityReference("account", incident.Id);
                        incidentResult["CaseRD.jarvis_repairingdealer"] = new AliasedValue("account", "account", new EntityReference("account", incident.Id));
                        incidentResult.Id = Guid.NewGuid();
                        result.Entities.Add(incidentResult);
                        result.Entities.Add(incidentResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_configurationjarvis'>"))
                    {
                        Entity configResult = new Entity("jarvis_configurationjarvis");
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationcasestatuschange"] = true;
                        configResult["createdon"] = DateTime.UtcNow;
                        configResult.Id = Guid.NewGuid();
                        result.Entities.Add(configResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_language'>"))
                    {
                        Entity language = new Entity("jarvis_language");
                        language.Id = Guid.NewGuid();
                        language["jarvis_iso3languagecode6392t"] = "DEU";
                        language["jarvis_iso2languagecode6391"] = "DE";
                        language["jarvis_name"] = "DE";
                        result.Entities.Add(language);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_casesoftoffer'>"))
                    {
                        Entity casesoftoffer = new Entity("jarvis_casesoftoffer");
                        casesoftoffer.Id = Guid.NewGuid();
                        casesoftoffer["jarvis_name"] = "DEU";
                        casesoftoffer["jarvis_contractno"] = "DE";
                        casesoftoffer["jarvis_description"] = "DE";
                        casesoftoffer["jarvis_startdate"] = DateTime.UtcNow;
                        casesoftoffer["jarvis_expirydate"] = DateTime.UtcNow;
                        casesoftoffer["statuscode"] = new OptionSetValue(0);
                        casesoftoffer["jarvis_softoffercodelookup"] = new EntityReference("jarvis_softoffer", Guid.NewGuid());
                        result.Entities.Add(casesoftoffer);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_softoffer'>"))
                    {
                        Entity softoffer = new Entity("jarvis_softoffer");
                        softoffer.Id = Guid.NewGuid();
                        softoffer["jarvis_name"] = "DEU";
                        softoffer["jarvis_softoffercode"] = new EntityReference("jarvis_softoffer", Guid.NewGuid());
                        softoffer["jarvis_contractno"] = "DE";
                        softoffer["jarvis_startdate"] = DateTime.UtcNow;
                        softoffer["jarvis_expirydate"] = DateTime.UtcNow;
                        softoffer["statuscode"] = new OptionSetValue(0);
                        softoffer["jarvis_marketcode"] = "DE";
                        softoffer["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
                        result.Entities.Add(softoffer);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_repairinformation'>"))
                    {
                        Entity repairInfoResult = new Entity("jarvis_repairinformation");
                        repairInfoResult["jarvis_name"] = "testpassout";
                        repairInfoResult["createdon"] = DateTime.UtcNow;
                        repairInfoResult.Id = Guid.NewGuid();
                        result.Entities.Add(repairInfoResult);
                    }
                }

                return result;
            };

            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "incident")
                {
                    Entity inc = new Entity("incident");
                    inc.Id = guid;
                    inc["customerid"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_callerlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    inc["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    inc["jarvis_driverlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    inc["jarvis_translationstatusreportedfault"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatuslocation"] = new OptionSetValue(334030000);
                    inc["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    inc["jarvis_translationstatuscustomerexpectations"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatusalertdescription"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatusalertextrainformation"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatusalertdecision"] = new OptionSetValue(334030000);
                    return inc;
                }

                if (entityName == "jarvis_vehicle")
                {
                    Entity client = new Entity("jarvis_vehicle");
                    client.Id = new Guid("6186f7f1-5f09-ed11-82e5-002248934e82");
                    client["jarvis_brandid"] = new EntityReference("jarvis_brand", Guid.NewGuid());
                    client["jarvis_fuelpowertype"] = new EntityReference("jarvis_vehiclefuelpowertype", Guid.NewGuid());
                    return client;
                }

                if (entityName == "jarvis_country")
                {
                    Entity country = new Entity("jarvis_country");
                    country.Id = guid;
                    country["jarvis_iso2countrycode"] = "NL";
                    return country;
                }

                if (entityName == "jarvis_language")
                {
                    Entity language = new Entity("jarvis_language");
                    language.Id = guid;
                    language["jarvis_iso3languagecode6392t"] = "DEU";
                    language["jarvis_iso2languagecode6391"] = "DE";
                    return language;
                }

                if (entityName == "account")
                {
                    Entity account = new Entity("account");
                    account.Id = guid;
                    account["jarvis_language"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    account["jarvis_address1_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    return account;
                }

                if (entityName == "jarvis_brand")
                {
                    Entity client = new Entity("jarvis_brand");
                    client.Id = new Guid("6186f7f1-5f09-ed11-82e5-002248934e82");
                    client["jarvis_mercuriusbrandcode"] = "ted";
                    return client;
                }

                return null;
            };
            this.Service.ExecuteOrganizationRequest = processInstanceRequest =>
            {
                if (processInstanceRequest.RequestName.Contains("RetrieveProcessInstances"))
                {
                    return new RetrieveProcessInstancesResponse()
                    {
                        Results = new ParameterCollection
                {
                    new KeyValuePair<string, object>("Processes", new EntityCollection(new List<Entity> { incident })),
                },
                    };
                }

                if (processInstanceRequest.RequestName.Contains("RetrieveActivePath"))
                {
                    Entity activepath = new Entity("activepath");
                    activepath["stagename"] = "Waiting For Repair Start";
                    activepath["processstageid"] = Guid.NewGuid();
                    return new RetrieveActivePathResponse()
                    {
                        Results = new ParameterCollection
                {
                    new KeyValuePair<string, object>("ProcessStages", new EntityCollection(new List<Entity> { activepath })),
                },
                    };
                }

                return null;
            };

            CasePostOperationSync plugin = new CasePostOperationSync();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// PreValidate Update With Complete Data Test Method with statuscode 130.
        /// </summary>
        [TestMethod]
        public void PostUpdateWithCompleteDataTestMethodStCode130()
        {
            Entity incidentTarget = new Entity("incident");

            EntityReference users = new EntityReference("systemuser", Guid.NewGuid());
            users.Name = "MERCURIUS";

            incidentTarget.Id = Guid.Empty;

            incidentTarget["statuscode"] = new OptionSetValue(130);
            incidentTarget["incidentid"] = Guid.NewGuid();
            incidentTarget["caseorigincode"] = new OptionSetValue(2);
            incidentTarget["casetypecode"] = new OptionSetValue(2);
            incidentTarget["jarvis_mercuriusstatus"] = new OptionSetValue(200);
            incidentTarget["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
            incidentTarget["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["customerid"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["jarvis_registrationnumber"] = "123";
            incidentTarget["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incidentTarget["description"] = "This is for testing";
            incidentTarget["jarvis_location"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incidentTarget["jarvis_customerexpectations"] = "test";
            incidentTarget["jarvis_incidentnature"] = "[{\"id\":\"3efba61a-85ad-44b7-a187-7f54acaf7a67\"}]";
            incidentTarget["jarvis_callerrole"] = new OptionSetValue(4);
            incidentTarget["jarvis_callerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incidentTarget["isescalated"] = true;
            incidentTarget["jarvis_querydescription"] = "test";
            incidentTarget["jarvis_extrainformationonsolutionquery"] = "test";
            incidentTarget["jarvis_translationstatusalertdecision"] = new OptionSetValue(334030000);
            incidentTarget["jarvis_querydecision"] = "test";
            incidentTarget.Attributes["jarvis_eta"] = DateTime.Now;
            incidentTarget.Attributes["jarvis_atc"] = DateTime.Now;
            incidentTarget["modifiedby"] = users;

            Entity incident = new Entity("incident");
            incident.Id = Guid.Empty;
            incident["statuscode"] = new OptionSetValue(130);
            incident["incidentid"] = new EntityReference("incident", Guid.NewGuid());
            incident["caseorigincode"] = new OptionSetValue(2);
            incident["casetypecode"] = new OptionSetValue(2);
            incident["jarvis_mercuriusstatus"] = new OptionSetValue(200);
            incident["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
            incident["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incident["customerid"] = new EntityReference("account", Guid.NewGuid());
            incident["jarvis_registrationnumber"] = "123";
            incident["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["description"] = "This is for testing";
            incident["jarvis_driverlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incident["jarvis_translationstatusreportedfault"] = new OptionSetValue(334030000);
            incident["jarvis_translationstatuslocation"] = new OptionSetValue(334030000);
            incident["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["jarvis_translationstatuscustomerexpectations"] = new OptionSetValue(334030000);
            incident["jarvis_location"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["jarvis_customerexpectations"] = "test";
            incident["ownerid"] = new EntityReference { LogicalName = "system_user", Name = "MERCURIUS" };
            incident["jarvis_sourceid"] = 232323232;
            incident["jarvis_callerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incident["jarvis_callerrole"] = new OptionSetValue(4);
            incident["createdby"] = users;
            incident["parentcaseid"] = new EntityReference("incident", Guid.NewGuid());
            incident["jarvis_translationstatusalertdescription"] = new OptionSetValue(334030000);
            incident["jarvis_translationstatusalertextrainformation"] = new OptionSetValue(334030000);
            incident["jarvis_translationstatusalertdecision"] = new OptionSetValue(334030000);
            incident["jarvis_extrainformationonsolutionquery"] = "test";
            incident["jarvis_querydecision"] = "test";
            incident["jarvis_onetimecustomerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incident["jarvis_onetimecustomercountry"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident.Attributes["jarvis_etc"] = DateTime.Now;
            //// Setting Input Parameters.
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", incidentTarget),
               };
            this.PluginExecutionContext.MessageNameGet = () => "Update";
            this.PluginExecutionContext.StageGet = () => 40;
            //// Setting Post and Pre Image
            this.PluginExecutionContext.PostEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PostImage", incident },
                };
            };
            this.PluginExecutionContext.PreEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PreImage", incident },
                };
            };

            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();
                if (query.GetType().Name == "FetchExpression")
                {
                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_vasbreakdownprocess'>"))
                    {
                        Entity vasprocess = new Entity("jarvis_vasbreakdownprocess");
                        vasprocess["businessprocessflowinstanceid"] = Guid.NewGuid();
                        vasprocess["bpf_name"] = "test";
                        vasprocess["bpf_incidentid"] = Guid.NewGuid();
                        vasprocess["activestageid"] = Guid.NewGuid();
                        vasprocess["statecode"] = new OptionSetValue(1);
                        vasprocess["createdon"] = DateTime.UtcNow;
                        vasprocess["createdby"] = "test";
                        vasprocess.Id = Guid.NewGuid();
                        result.Entities.Add(vasprocess);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='incident'>"))
                    {
                        Entity incidentResult = new Entity("incident");
                        incidentResult[Constants.IncidentNature.IncidentIncidentNature_jarvisIincidentnatureid] = new AliasedValue("incidentNature", "incidentnatureid", Guid.NewGuid());
                        incidentResult["jarvis_hdrd"] = true;
                        incidentResult["jarvis_homedealer"] = new EntityReference("account", incident.Id);
                        incidentResult["CaseRD.jarvis_repairingdealer"] = new AliasedValue("account", "account", new EntityReference("account", incident.Id));
                        incidentResult.Id = Guid.NewGuid();
                        result.Entities.Add(incidentResult);
                        result.Entities.Add(incidentResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_configurationjarvis'>"))
                    {
                        Entity configResult = new Entity("jarvis_configurationjarvis");
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationcasestatuschange"] = true;
                        configResult["createdon"] = DateTime.UtcNow;
                        configResult.Id = Guid.NewGuid();
                        result.Entities.Add(configResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_language'>"))
                    {
                        Entity language = new Entity("jarvis_language");
                        language.Id = Guid.NewGuid();
                        language["jarvis_iso3languagecode6392t"] = "DEU";
                        language["jarvis_iso2languagecode6391"] = "DE";
                        language["jarvis_name"] = "DE";
                        result.Entities.Add(language);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_casesoftoffer'>"))
                    {
                        Entity casesoftoffer = new Entity("jarvis_casesoftoffer");
                        casesoftoffer.Id = Guid.NewGuid();
                        casesoftoffer["jarvis_name"] = "DEU";
                        casesoftoffer["jarvis_contractno"] = "DE";
                        casesoftoffer["jarvis_description"] = "DE";
                        casesoftoffer["jarvis_startdate"] = DateTime.UtcNow;
                        casesoftoffer["jarvis_expirydate"] = DateTime.UtcNow;
                        casesoftoffer["statuscode"] = new OptionSetValue(0);
                        casesoftoffer["jarvis_softoffercodelookup"] = new EntityReference("jarvis_softoffer", Guid.NewGuid());
                        result.Entities.Add(casesoftoffer);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_softoffer'>"))
                    {
                        Entity softoffer = new Entity("jarvis_softoffer");
                        softoffer.Id = Guid.NewGuid();
                        softoffer["jarvis_name"] = "DEU";
                        softoffer["jarvis_softoffercode"] = new EntityReference("jarvis_softoffer", Guid.NewGuid());
                        softoffer["jarvis_contractno"] = "DE";
                        softoffer["jarvis_startdate"] = DateTime.UtcNow;
                        softoffer["jarvis_expirydate"] = DateTime.UtcNow;
                        softoffer["statuscode"] = new OptionSetValue(0);
                        softoffer["jarvis_marketcode"] = "DE";
                        softoffer["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
                        result.Entities.Add(softoffer);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_repairinformation'>"))
                    {
                        Entity repairInfoResult = new Entity("jarvis_repairinformation");
                        repairInfoResult["jarvis_name"] = "testpassout";
                        repairInfoResult["createdon"] = DateTime.UtcNow;
                        repairInfoResult.Id = Guid.NewGuid();
                        result.Entities.Add(repairInfoResult);
                    }
                }

                return result;
            };

            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "incident")
                {
                    Entity inc = new Entity("incident");
                    inc.Id = guid;
                    inc["customerid"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_callerlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    inc["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    inc["jarvis_driverlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    inc["jarvis_translationstatusreportedfault"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatuslocation"] = new OptionSetValue(334030000);
                    inc["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    inc["jarvis_translationstatuscustomerexpectations"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatusalertdescription"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatusalertextrainformation"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatusalertdecision"] = new OptionSetValue(334030000);
                    return inc;
                }

                if (entityName == "jarvis_vehicle")
                {
                    Entity client = new Entity("jarvis_vehicle");
                    client.Id = new Guid("6186f7f1-5f09-ed11-82e5-002248934e82");
                    client["jarvis_brandid"] = new EntityReference("jarvis_brand", Guid.NewGuid());
                    client["jarvis_fuelpowertype"] = new EntityReference("jarvis_vehiclefuelpowertype", Guid.NewGuid());
                    return client;
                }

                if (entityName == "jarvis_country")
                {
                    Entity country = new Entity("jarvis_country");
                    country.Id = guid;
                    country["jarvis_iso2countrycode"] = "NL";
                    return country;
                }

                if (entityName == "jarvis_language")
                {
                    Entity language = new Entity("jarvis_language");
                    language.Id = guid;
                    language["jarvis_iso3languagecode6392t"] = "DEU";
                    language["jarvis_iso2languagecode6391"] = "DE";
                    return language;
                }

                if (entityName == "account")
                {
                    Entity account = new Entity("account");
                    account.Id = guid;
                    account["jarvis_language"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    account["jarvis_address1_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    return account;
                }

                if (entityName == "jarvis_brand")
                {
                    Entity client = new Entity("jarvis_brand");
                    client.Id = new Guid("6186f7f1-5f09-ed11-82e5-002248934e82");
                    client["jarvis_mercuriusbrandcode"] = "ted";
                    return client;
                }

                return null;
            };
            this.Service.ExecuteOrganizationRequest = processInstanceRequest =>
            {
                if (processInstanceRequest.RequestName.Contains("RetrieveProcessInstances"))
                {
                    return new RetrieveProcessInstancesResponse()
                    {
                        Results = new ParameterCollection
                {
                    new KeyValuePair<string, object>("Processes", new EntityCollection(new List<Entity> { incident })),
                },
                    };
                }

                if (processInstanceRequest.RequestName.Contains("RetrieveActivePath"))
                {
                    Entity activepath = new Entity("activepath");
                    activepath["stagename"] = "Waiting For Repair Start";
                    activepath["processstageid"] = Guid.NewGuid();
                    return new RetrieveActivePathResponse()
                    {
                        Results = new ParameterCollection
                {
                    new KeyValuePair<string, object>("ProcessStages", new EntityCollection(new List<Entity> { activepath })),
                },
                    };
                }

                return null;
            };

            CasePostOperationSync plugin = new CasePostOperationSync();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// PreValidate Update With Complete Data Test Method with statuscode 140.
        /// </summary>
        [TestMethod]
        public void PostUpdateWithCompleteDataTestMethodStCode140()
        {
            Entity incidentTarget = new Entity("incident");

            EntityReference users = new EntityReference("systemuser", Guid.NewGuid());
            users.Name = "MERCURIUS";

            incidentTarget.Id = Guid.Empty;

            incidentTarget["statuscode"] = new OptionSetValue(140);
            incidentTarget["incidentid"] = Guid.NewGuid();
            incidentTarget["caseorigincode"] = new OptionSetValue(2);
            incidentTarget["casetypecode"] = new OptionSetValue(2);
            incidentTarget["jarvis_mercuriusstatus"] = new OptionSetValue(200);
            incidentTarget["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
            incidentTarget["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["customerid"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["jarvis_registrationnumber"] = "123";
            incidentTarget["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incidentTarget["description"] = "This is for testing";
            incidentTarget["jarvis_location"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incidentTarget["jarvis_customerexpectations"] = "test";
            incidentTarget["jarvis_incidentnature"] = "[{\"id\":\"3efba61a-85ad-44b7-a187-7f54acaf7a67\"}]";
            incidentTarget["jarvis_callerrole"] = new OptionSetValue(4);
            incidentTarget["jarvis_callerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incidentTarget["isescalated"] = true;
            incidentTarget["jarvis_querydescription"] = "test";
            incidentTarget["jarvis_extrainformationonsolutionquery"] = "test";
            incidentTarget["jarvis_translationstatusalertdecision"] = new OptionSetValue(334030000);
            incidentTarget["jarvis_querydecision"] = "test";
            incidentTarget.Attributes["jarvis_eta"] = DateTime.Now;
            incidentTarget.Attributes["jarvis_atc"] = DateTime.Now;
            incidentTarget["modifiedby"] = users;

            Entity incident = new Entity("incident");
            incident.Id = Guid.Empty;
            incident["statuscode"] = new OptionSetValue(140);
            incident["incidentid"] = new EntityReference("incident", Guid.NewGuid());
            incident["caseorigincode"] = new OptionSetValue(2);
            incident["casetypecode"] = new OptionSetValue(2);
            incident["jarvis_mercuriusstatus"] = new OptionSetValue(200);
            incident["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
            incident["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incident["customerid"] = new EntityReference("account", Guid.NewGuid());
            incident["jarvis_registrationnumber"] = "123";
            incident["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["description"] = "This is for testing";
            incident["jarvis_driverlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incident["jarvis_translationstatusreportedfault"] = new OptionSetValue(334030000);
            incident["jarvis_translationstatuslocation"] = new OptionSetValue(334030000);
            incident["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["jarvis_translationstatuscustomerexpectations"] = new OptionSetValue(334030000);
            incident["jarvis_location"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["jarvis_customerexpectations"] = "test";
            incident["ownerid"] = new EntityReference { LogicalName = "system_user", Name = "MERCURIUS" };
            incident["jarvis_sourceid"] = 232323232;
            incident["jarvis_callerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incident["jarvis_callerrole"] = new OptionSetValue(4);
            incident["createdby"] = users;
            incident["parentcaseid"] = new EntityReference("incident", Guid.NewGuid());
            incident["jarvis_translationstatusalertdescription"] = new OptionSetValue(334030000);
            incident["jarvis_translationstatusalertextrainformation"] = new OptionSetValue(334030000);
            incident["jarvis_translationstatusalertdecision"] = new OptionSetValue(334030000);
            incident["jarvis_extrainformationonsolutionquery"] = "test";
            incident["jarvis_querydecision"] = "test";
            incident["jarvis_onetimecustomerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incident["jarvis_onetimecustomercountry"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident.Attributes["jarvis_etc"] = DateTime.Now;
            incident["jarvis_querydecisioncatagory"] = new EntityReference("jarvis_querydecisioncatagory", Guid.NewGuid());
            //// Setting Input Parameters.
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", incidentTarget),
               };
            this.PluginExecutionContext.MessageNameGet = () => "Update";
            this.PluginExecutionContext.StageGet = () => 40;
            //// Setting Post and Pre Image
            this.PluginExecutionContext.PostEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PostImage", incident },
                };
            };
            this.PluginExecutionContext.PreEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PreImage", incident },
                };
            };

            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();
                if (query.GetType().Name == "FetchExpression")
                {
                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_vasbreakdownprocess'>"))
                    {
                        Entity vasprocess = new Entity("jarvis_vasbreakdownprocess");
                        vasprocess["businessprocessflowinstanceid"] = Guid.NewGuid();
                        vasprocess["bpf_name"] = "test";
                        vasprocess["bpf_incidentid"] = Guid.NewGuid();
                        vasprocess["activestageid"] = Guid.NewGuid();
                        vasprocess["statecode"] = new OptionSetValue(1);
                        vasprocess["createdon"] = DateTime.UtcNow;
                        vasprocess["createdby"] = "test";
                        vasprocess.Id = Guid.NewGuid();
                        result.Entities.Add(vasprocess);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='incident'>"))
                    {
                        Entity incidentResult = new Entity("incident");
                        incidentResult[Constants.IncidentNature.IncidentIncidentNature_jarvisIincidentnatureid] = new AliasedValue("incidentNature", "incidentnatureid", Guid.NewGuid());
                        incidentResult["jarvis_hdrd"] = true;
                        incidentResult["jarvis_homedealer"] = new EntityReference("account", incident.Id);
                        incidentResult["CaseRD.jarvis_repairingdealer"] = new AliasedValue("account", "account", new EntityReference("account", incident.Id));
                        incidentResult.Id = Guid.NewGuid();
                        result.Entities.Add(incidentResult);
                        result.Entities.Add(incidentResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_configurationjarvis'>"))
                    {
                        Entity configResult = new Entity("jarvis_configurationjarvis");
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationcasestatuschange"] = true;
                        configResult["createdon"] = DateTime.UtcNow;
                        configResult.Id = Guid.NewGuid();
                        result.Entities.Add(configResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_language'>"))
                    {
                        Entity language = new Entity("jarvis_language");
                        language.Id = Guid.NewGuid();
                        language["jarvis_iso3languagecode6392t"] = "DEU";
                        language["jarvis_iso2languagecode6391"] = "DE";
                        language["jarvis_name"] = "DE";
                        result.Entities.Add(language);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_casesoftoffer'>"))
                    {
                        Entity casesoftoffer = new Entity("jarvis_casesoftoffer");
                        casesoftoffer.Id = Guid.NewGuid();
                        casesoftoffer["jarvis_name"] = "DEU";
                        casesoftoffer["jarvis_contractno"] = "DE";
                        casesoftoffer["jarvis_description"] = "DE";
                        casesoftoffer["jarvis_startdate"] = DateTime.UtcNow;
                        casesoftoffer["jarvis_expirydate"] = DateTime.UtcNow;
                        casesoftoffer["statuscode"] = new OptionSetValue(0);
                        casesoftoffer["jarvis_softoffercodelookup"] = new EntityReference("jarvis_softoffer", Guid.NewGuid());
                        result.Entities.Add(casesoftoffer);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_softoffer'>"))
                    {
                        Entity softoffer = new Entity("jarvis_softoffer");
                        softoffer.Id = Guid.NewGuid();
                        softoffer["jarvis_name"] = "DEU";
                        softoffer["jarvis_softoffercode"] = new EntityReference("jarvis_softoffer", Guid.NewGuid());
                        softoffer["jarvis_contractno"] = "DE";
                        softoffer["jarvis_startdate"] = DateTime.UtcNow;
                        softoffer["jarvis_expirydate"] = DateTime.UtcNow;
                        softoffer["statuscode"] = new OptionSetValue(0);
                        softoffer["jarvis_marketcode"] = "DE";
                        softoffer["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
                        result.Entities.Add(softoffer);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_repairinformation'>"))
                    {
                        Entity repairInfoResult = new Entity("jarvis_repairinformation");
                        repairInfoResult["jarvis_name"] = "testpassout";
                        repairInfoResult["createdon"] = DateTime.UtcNow;
                        repairInfoResult.Id = Guid.NewGuid();
                        result.Entities.Add(repairInfoResult);
                    }
                }

                return result;
            };

            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "incident")
                {
                    Entity inc = new Entity("incident");
                    inc.Id = guid;
                    inc["customerid"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_callerlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    inc["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    inc["jarvis_driverlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    inc["jarvis_translationstatusreportedfault"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatuslocation"] = new OptionSetValue(334030000);
                    inc["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    inc["jarvis_translationstatuscustomerexpectations"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatusalertdescription"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatusalertextrainformation"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatusalertdecision"] = new OptionSetValue(334030000);
                    return inc;
                }

                if (entityName == "jarvis_vehicle")
                {
                    Entity client = new Entity("jarvis_vehicle");
                    client.Id = new Guid("6186f7f1-5f09-ed11-82e5-002248934e82");
                    client["jarvis_brandid"] = new EntityReference("jarvis_brand", Guid.NewGuid());
                    client["jarvis_fuelpowertype"] = new EntityReference("jarvis_vehiclefuelpowertype", Guid.NewGuid());
                    return client;
                }

                if (entityName == "jarvis_querydecisioncatagory")
                {
                    Entity client = new Entity("jarvis_querydecisioncatagory");
                    client.Id = Guid.NewGuid();
                    client["jarvis_autoclose"] = true;
                    return client;
                }

                if (entityName == "jarvis_country")
                {
                    Entity country = new Entity("jarvis_country");
                    country.Id = guid;
                    country["jarvis_iso2countrycode"] = "NL";
                    return country;
                }

                if (entityName == "jarvis_language")
                {
                    Entity language = new Entity("jarvis_language");
                    language.Id = guid;
                    language["jarvis_iso3languagecode6392t"] = "DEU";
                    language["jarvis_iso2languagecode6391"] = "DE";
                    return language;
                }

                if (entityName == "account")
                {
                    Entity account = new Entity("account");
                    account.Id = guid;
                    account["jarvis_language"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    account["jarvis_address1_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    return account;
                }

                if (entityName == "jarvis_brand")
                {
                    Entity client = new Entity("jarvis_brand");
                    client.Id = new Guid("6186f7f1-5f09-ed11-82e5-002248934e82");
                    client["jarvis_mercuriusbrandcode"] = "ted";
                    return client;
                }

                return null;
            };
            this.Service.ExecuteOrganizationRequest = processInstanceRequest =>
            {
                if (processInstanceRequest.RequestName.Contains("RetrieveProcessInstances"))
                {
                    return new RetrieveProcessInstancesResponse()
                    {
                        Results = new ParameterCollection
                {
                    new KeyValuePair<string, object>("Processes", new EntityCollection(new List<Entity> { incident })),
                },
                    };
                }

                if (processInstanceRequest.RequestName.Contains("RetrieveActivePath"))
                {
                    Entity activepath = new Entity("activepath");
                    activepath["stagename"] = "Waiting For Repair Start";
                    activepath["processstageid"] = Guid.NewGuid();
                    return new RetrieveActivePathResponse()
                    {
                        Results = new ParameterCollection
                {
                    new KeyValuePair<string, object>("ProcessStages", new EntityCollection(new List<Entity> { activepath })),
                },
                    };
                }

                return null;
            };

            CasePostOperationSync plugin = new CasePostOperationSync();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// PreValidate Update With Complete Data Test Method with statuscode 140 with autoclose false.
        /// </summary>
        [TestMethod]
        public void PostUpdateWithCompleteDataTestMethodStCode140AutoClose()
        {
            Entity incidentTarget = new Entity("incident");

            EntityReference users = new EntityReference("systemuser", Guid.NewGuid());
            users.Name = "MERCURIUS";

            incidentTarget.Id = Guid.Empty;

            incidentTarget["statuscode"] = new OptionSetValue(140);
            incidentTarget["incidentid"] = Guid.NewGuid();
            incidentTarget["caseorigincode"] = new OptionSetValue(2);
            incidentTarget["casetypecode"] = new OptionSetValue(2);
            incidentTarget["jarvis_mercuriusstatus"] = new OptionSetValue(200);
            incidentTarget["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
            incidentTarget["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["customerid"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["jarvis_registrationnumber"] = "123";
            incidentTarget["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incidentTarget["description"] = "This is for testing";
            incidentTarget["jarvis_location"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incidentTarget["jarvis_customerexpectations"] = "test";
            incidentTarget["jarvis_incidentnature"] = "[{\"id\":\"3efba61a-85ad-44b7-a187-7f54acaf7a67\"}]";
            incidentTarget["jarvis_callerrole"] = new OptionSetValue(4);
            incidentTarget["jarvis_callerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incidentTarget["isescalated"] = true;
            incidentTarget["jarvis_querydescription"] = "test";
            incidentTarget["jarvis_extrainformationonsolutionquery"] = "test";
            incidentTarget["jarvis_translationstatusalertdecision"] = new OptionSetValue(334030000);
            incidentTarget["jarvis_querydecision"] = "test";
            incidentTarget.Attributes["jarvis_eta"] = DateTime.Now;
            incidentTarget.Attributes["jarvis_atc"] = DateTime.Now;
            incidentTarget.Attributes["modifiedby"] = users;

            Entity incident = new Entity("incident");
            incident.Id = Guid.Empty;
            incident["statuscode"] = new OptionSetValue(140);
            incident["incidentid"] = new EntityReference("incident", Guid.NewGuid());
            incident["caseorigincode"] = new OptionSetValue(2);
            incident["casetypecode"] = new OptionSetValue(2);
            incident["jarvis_mercuriusstatus"] = new OptionSetValue(200);
            incident["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
            incident["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incident["customerid"] = new EntityReference("account", Guid.NewGuid());
            incident["jarvis_registrationnumber"] = "123";
            incident["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["description"] = "This is for testing";
            incident["jarvis_driverlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incident["jarvis_translationstatusreportedfault"] = new OptionSetValue(334030000);
            incident["jarvis_translationstatuslocation"] = new OptionSetValue(334030000);
            incident["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["jarvis_translationstatuscustomerexpectations"] = new OptionSetValue(334030000);
            incident["jarvis_location"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["jarvis_customerexpectations"] = "test";
            incident["ownerid"] = new EntityReference { LogicalName = "system_user", Name = "MERCURIUS" };
            incident["jarvis_sourceid"] = 232323232;
            incident["jarvis_callerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incident["jarvis_callerrole"] = new OptionSetValue(4);
            incident["createdby"] = users;
            incident["parentcaseid"] = new EntityReference("incident", Guid.NewGuid());
            incident["jarvis_translationstatusalertdescription"] = new OptionSetValue(334030000);
            incident["jarvis_translationstatusalertextrainformation"] = new OptionSetValue(334030000);
            incident["jarvis_translationstatusalertdecision"] = new OptionSetValue(334030000);
            incident["jarvis_extrainformationonsolutionquery"] = "test";
            incident["jarvis_querydecision"] = "test";
            incident["jarvis_onetimecustomerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incident["jarvis_onetimecustomercountry"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident.Attributes["jarvis_etc"] = DateTime.Now;
            incident["jarvis_querydecisioncatagory"] = new EntityReference("jarvis_querydecisioncatagory", Guid.NewGuid());
            //// Setting Input Parameters.
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", incidentTarget),
               };
            this.PluginExecutionContext.MessageNameGet = () => "Update";
            this.PluginExecutionContext.StageGet = () => 40;
            //// Setting Post and Pre Image
            this.PluginExecutionContext.PostEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PostImage", incident },
                };
            };
            this.PluginExecutionContext.PreEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PreImage", incident },
                };
            };

            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();
                if (query.GetType().Name == "FetchExpression")
                {
                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_vasbreakdownprocess'>"))
                    {
                        Entity vasprocess = new Entity("jarvis_vasbreakdownprocess");
                        vasprocess["businessprocessflowinstanceid"] = Guid.NewGuid();
                        vasprocess["bpf_name"] = "test";
                        vasprocess["bpf_incidentid"] = Guid.NewGuid();
                        vasprocess["activestageid"] = Guid.NewGuid();
                        vasprocess["statecode"] = new OptionSetValue(1);
                        vasprocess["createdon"] = DateTime.UtcNow;
                        vasprocess["createdby"] = "test";
                        vasprocess.Id = Guid.NewGuid();
                        result.Entities.Add(vasprocess);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='incident'>"))
                    {
                        Entity incidentResult = new Entity("incident");
                        incidentResult[Constants.IncidentNature.IncidentIncidentNature_jarvisIincidentnatureid] = new AliasedValue("incidentNature", "incidentnatureid", Guid.NewGuid());
                        incidentResult["jarvis_hdrd"] = true;
                        incidentResult["jarvis_homedealer"] = new EntityReference("account", incident.Id);
                        incidentResult["CaseRD.jarvis_repairingdealer"] = new AliasedValue("account", "account", new EntityReference("account", incident.Id));
                        incidentResult.Id = Guid.NewGuid();
                        result.Entities.Add(incidentResult);
                        result.Entities.Add(incidentResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_configurationjarvis'>"))
                    {
                        Entity configResult = new Entity("jarvis_configurationjarvis");
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationcasestatuschange"] = true;
                        configResult["createdon"] = DateTime.UtcNow;
                        configResult.Id = Guid.NewGuid();
                        result.Entities.Add(configResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_language'>"))
                    {
                        Entity language = new Entity("jarvis_language");
                        language.Id = Guid.NewGuid();
                        language["jarvis_iso3languagecode6392t"] = "DEU";
                        language["jarvis_iso2languagecode6391"] = "DE";
                        language["jarvis_name"] = "DE";
                        result.Entities.Add(language);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_casesoftoffer'>"))
                    {
                        Entity casesoftoffer = new Entity("jarvis_casesoftoffer");
                        casesoftoffer.Id = Guid.NewGuid();
                        casesoftoffer["jarvis_name"] = "DEU";
                        casesoftoffer["jarvis_contractno"] = "DE";
                        casesoftoffer["jarvis_description"] = "DE";
                        casesoftoffer["jarvis_startdate"] = DateTime.UtcNow;
                        casesoftoffer["jarvis_expirydate"] = DateTime.UtcNow;
                        casesoftoffer["statuscode"] = new OptionSetValue(0);
                        casesoftoffer["jarvis_softoffercodelookup"] = new EntityReference("jarvis_softoffer", Guid.NewGuid());
                        result.Entities.Add(casesoftoffer);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_softoffer'>"))
                    {
                        Entity softoffer = new Entity("jarvis_softoffer");
                        softoffer.Id = Guid.NewGuid();
                        softoffer["jarvis_name"] = "DEU";
                        softoffer["jarvis_softoffercode"] = new EntityReference("jarvis_softoffer", Guid.NewGuid());
                        softoffer["jarvis_contractno"] = "DE";
                        softoffer["jarvis_startdate"] = DateTime.UtcNow;
                        softoffer["jarvis_expirydate"] = DateTime.UtcNow;
                        softoffer["statuscode"] = new OptionSetValue(0);
                        softoffer["jarvis_marketcode"] = "DE";
                        softoffer["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
                        result.Entities.Add(softoffer);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_repairinformation'>"))
                    {
                        Entity repairInfoResult = new Entity("jarvis_repairinformation");
                        repairInfoResult["jarvis_name"] = "testpassout";
                        repairInfoResult["createdon"] = DateTime.UtcNow;
                        repairInfoResult.Id = Guid.NewGuid();
                        result.Entities.Add(repairInfoResult);
                    }
                }

                return result;
            };

            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "incident")
                {
                    Entity inc = new Entity("incident");
                    inc.Id = guid;
                    inc["customerid"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_callerlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    inc["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    inc["jarvis_driverlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    inc["jarvis_translationstatusreportedfault"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatuslocation"] = new OptionSetValue(334030000);
                    inc["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    inc["jarvis_translationstatuscustomerexpectations"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatusalertdescription"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatusalertextrainformation"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatusalertdecision"] = new OptionSetValue(334030000);
                    return inc;
                }

                if (entityName == "jarvis_vehicle")
                {
                    Entity client = new Entity("jarvis_vehicle");
                    client.Id = new Guid("6186f7f1-5f09-ed11-82e5-002248934e82");
                    client["jarvis_brandid"] = new EntityReference("jarvis_brand", Guid.NewGuid());
                    client["jarvis_fuelpowertype"] = new EntityReference("jarvis_vehiclefuelpowertype", Guid.NewGuid());
                    return client;
                }

                if (entityName == "jarvis_querydecisioncatagory")
                {
                    Entity client = new Entity("jarvis_querydecisioncatagory");
                    client.Id = Guid.NewGuid();
                    client["jarvis_autoclose"] = false;
                    return client;
                }

                if (entityName == "jarvis_country")
                {
                    Entity country = new Entity("jarvis_country");
                    country.Id = guid;
                    country["jarvis_iso2countrycode"] = "NL";
                    return country;
                }

                if (entityName == "jarvis_language")
                {
                    Entity language = new Entity("jarvis_language");
                    language.Id = guid;
                    language["jarvis_iso3languagecode6392t"] = "DEU";
                    language["jarvis_iso2languagecode6391"] = "DE";
                    return language;
                }

                if (entityName == "account")
                {
                    Entity account = new Entity("account");
                    account.Id = guid;
                    account["jarvis_language"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    account["jarvis_address1_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    return account;
                }

                if (entityName == "jarvis_brand")
                {
                    Entity client = new Entity("jarvis_brand");
                    client.Id = new Guid("6186f7f1-5f09-ed11-82e5-002248934e82");
                    client["jarvis_mercuriusbrandcode"] = "ted";
                    return client;
                }

                return null;
            };
            this.Service.ExecuteOrganizationRequest = processInstanceRequest =>
            {
                if (processInstanceRequest.RequestName.Contains("RetrieveProcessInstances"))
                {
                    return new RetrieveProcessInstancesResponse()
                    {
                        Results = new ParameterCollection
                {
                    new KeyValuePair<string, object>("Processes", new EntityCollection(new List<Entity> { incident })),
                },
                    };
                }

                if (processInstanceRequest.RequestName.Contains("RetrieveActivePath"))
                {
                    Entity activepath = new Entity("activepath");
                    activepath["stagename"] = "Waiting For Repair Start";
                    activepath["processstageid"] = Guid.NewGuid();
                    return new RetrieveActivePathResponse()
                    {
                        Results = new ParameterCollection
                {
                    new KeyValuePair<string, object>("ProcessStages", new EntityCollection(new List<Entity> { activepath })),
                },
                    };
                }

                return null;
            };

            CasePostOperationSync plugin = new CasePostOperationSync();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// PreValidate Update With Complete Data Test Method with statuscode 140 with No catagory.
        /// </summary>
        [TestMethod]
        public void PostUpdateWithCompleteDataTestMethodStCode140NoCatagory()
        {
            Entity incidentTarget = new Entity("incident");

            EntityReference users = new EntityReference("systemuser", Guid.NewGuid());
            users.Name = "MERCURIUS";

            incidentTarget.Id = Guid.Empty;

            incidentTarget["statuscode"] = new OptionSetValue(140);
            incidentTarget["incidentid"] = Guid.NewGuid();
            incidentTarget["caseorigincode"] = new OptionSetValue(2);
            incidentTarget["casetypecode"] = new OptionSetValue(2);
            incidentTarget["jarvis_mercuriusstatus"] = new OptionSetValue(200);
            incidentTarget["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
            incidentTarget["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["customerid"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["jarvis_registrationnumber"] = "123";
            incidentTarget["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incidentTarget["description"] = "This is for testing";
            incidentTarget["jarvis_location"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incidentTarget["jarvis_customerexpectations"] = "test";
            incidentTarget["jarvis_incidentnature"] = "[{\"id\":\"3efba61a-85ad-44b7-a187-7f54acaf7a67\"}]";
            incidentTarget["jarvis_callerrole"] = new OptionSetValue(4);
            incidentTarget["jarvis_callerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incidentTarget["isescalated"] = true;
            incidentTarget["jarvis_querydescription"] = "test";
            incidentTarget["jarvis_extrainformationonsolutionquery"] = "test";
            incidentTarget["jarvis_translationstatusalertdecision"] = new OptionSetValue(334030000);
            incidentTarget["jarvis_querydecision"] = "test";
            incidentTarget.Attributes["jarvis_eta"] = DateTime.Now;
            incidentTarget.Attributes["jarvis_atc"] = DateTime.Now;
            incidentTarget.Attributes["modifiedby"] = users;

            Entity incident = new Entity("incident");
            incident.Id = Guid.Empty;
            incident["statuscode"] = new OptionSetValue(140);
            incident["incidentid"] = new EntityReference("incident", Guid.NewGuid());
            incident["caseorigincode"] = new OptionSetValue(2);
            incident["casetypecode"] = new OptionSetValue(2);
            incident["jarvis_mercuriusstatus"] = new OptionSetValue(200);
            incident["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
            incident["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incident["customerid"] = new EntityReference("account", Guid.NewGuid());
            incident["jarvis_registrationnumber"] = "123";
            incident["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["description"] = "This is for testing";
            incident["jarvis_driverlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incident["jarvis_translationstatusreportedfault"] = new OptionSetValue(334030000);
            incident["jarvis_translationstatuslocation"] = new OptionSetValue(334030000);
            incident["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["jarvis_translationstatuscustomerexpectations"] = new OptionSetValue(334030000);
            incident["jarvis_location"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["jarvis_customerexpectations"] = "test";
            incident["ownerid"] = new EntityReference { LogicalName = "system_user", Name = "MERCURIUS" };
            incident["jarvis_sourceid"] = 232323232;
            incident["jarvis_callerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incident["jarvis_callerrole"] = new OptionSetValue(4);
            incident["createdby"] = users;
            incident["parentcaseid"] = new EntityReference("incident", Guid.NewGuid());
            incident["jarvis_translationstatusalertdescription"] = new OptionSetValue(334030000);
            incident["jarvis_translationstatusalertextrainformation"] = new OptionSetValue(334030000);
            incident["jarvis_translationstatusalertdecision"] = new OptionSetValue(334030000);
            incident["jarvis_extrainformationonsolutionquery"] = "test";
            incident["jarvis_querydecision"] = "test";
            incident["jarvis_onetimecustomerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incident["jarvis_onetimecustomercountry"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident.Attributes["jarvis_etc"] = DateTime.Now;
            incident["jarvis_querydecisioncatagory"] = new EntityReference("jarvis_querydecisioncatagory", Guid.NewGuid());
            //// Setting Input Parameters.
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", incidentTarget),
               };
            this.PluginExecutionContext.MessageNameGet = () => "Update";
            this.PluginExecutionContext.StageGet = () => 40;
            //// Setting Post and Pre Image
            this.PluginExecutionContext.PostEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PostImage", incident },
                };
            };
            this.PluginExecutionContext.PreEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PreImage", incident },
                };
            };

            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();
                if (query.GetType().Name == "FetchExpression")
                {
                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_vasbreakdownprocess'>"))
                    {
                        Entity vasprocess = new Entity("jarvis_vasbreakdownprocess");
                        vasprocess["businessprocessflowinstanceid"] = Guid.NewGuid();
                        vasprocess["bpf_name"] = "test";
                        vasprocess["bpf_incidentid"] = Guid.NewGuid();
                        vasprocess["activestageid"] = Guid.NewGuid();
                        vasprocess["statecode"] = new OptionSetValue(1);
                        vasprocess["createdon"] = DateTime.UtcNow;
                        vasprocess["createdby"] = "test";
                        vasprocess.Id = Guid.NewGuid();
                        result.Entities.Add(vasprocess);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='incident'>"))
                    {
                        Entity incidentResult = new Entity("incident");
                        incidentResult[Constants.IncidentNature.IncidentIncidentNature_jarvisIincidentnatureid] = new AliasedValue("incidentNature", "incidentnatureid", Guid.NewGuid());
                        incidentResult["jarvis_hdrd"] = true;
                        incidentResult["jarvis_homedealer"] = new EntityReference("account", incident.Id);
                        incidentResult["CaseRD.jarvis_repairingdealer"] = new AliasedValue("account", "account", new EntityReference("account", incident.Id));
                        incidentResult.Id = Guid.NewGuid();
                        result.Entities.Add(incidentResult);
                        result.Entities.Add(incidentResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_configurationjarvis'>"))
                    {
                        Entity configResult = new Entity("jarvis_configurationjarvis");
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationcasestatuschange"] = true;
                        configResult["createdon"] = DateTime.UtcNow;
                        configResult.Id = Guid.NewGuid();
                        result.Entities.Add(configResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_language'>"))
                    {
                        Entity language = new Entity("jarvis_language");
                        language.Id = Guid.NewGuid();
                        language["jarvis_iso3languagecode6392t"] = "DEU";
                        language["jarvis_iso2languagecode6391"] = "DE";
                        language["jarvis_name"] = "DE";
                        result.Entities.Add(language);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_casesoftoffer'>"))
                    {
                        Entity casesoftoffer = new Entity("jarvis_casesoftoffer");
                        casesoftoffer.Id = Guid.NewGuid();
                        casesoftoffer["jarvis_name"] = "DEU";
                        casesoftoffer["jarvis_contractno"] = "DE";
                        casesoftoffer["jarvis_description"] = "DE";
                        casesoftoffer["jarvis_startdate"] = DateTime.UtcNow;
                        casesoftoffer["jarvis_expirydate"] = DateTime.UtcNow;
                        casesoftoffer["statuscode"] = new OptionSetValue(0);
                        casesoftoffer["jarvis_softoffercodelookup"] = new EntityReference("jarvis_softoffer", Guid.NewGuid());
                        result.Entities.Add(casesoftoffer);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_softoffer'>"))
                    {
                        Entity softoffer = new Entity("jarvis_softoffer");
                        softoffer.Id = Guid.NewGuid();
                        softoffer["jarvis_name"] = "DEU";
                        softoffer["jarvis_softoffercode"] = new EntityReference("jarvis_softoffer", Guid.NewGuid());
                        softoffer["jarvis_contractno"] = "DE";
                        softoffer["jarvis_startdate"] = DateTime.UtcNow;
                        softoffer["jarvis_expirydate"] = DateTime.UtcNow;
                        softoffer["statuscode"] = new OptionSetValue(0);
                        softoffer["jarvis_marketcode"] = "DE";
                        softoffer["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
                        result.Entities.Add(softoffer);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_repairinformation'>"))
                    {
                        Entity repairInfoResult = new Entity("jarvis_repairinformation");
                        repairInfoResult["jarvis_name"] = "testpassout";
                        repairInfoResult["createdon"] = DateTime.UtcNow;
                        repairInfoResult.Id = Guid.NewGuid();
                        result.Entities.Add(repairInfoResult);
                    }
                }

                return result;
            };

            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "incident")
                {
                    Entity inc = new Entity("incident");
                    inc.Id = guid;
                    inc["customerid"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_callerlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    inc["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    inc["jarvis_driverlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    inc["jarvis_translationstatusreportedfault"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatuslocation"] = new OptionSetValue(334030000);
                    inc["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    inc["jarvis_translationstatuscustomerexpectations"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatusalertdescription"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatusalertextrainformation"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatusalertdecision"] = new OptionSetValue(334030000);
                    return inc;
                }

                if (entityName == "jarvis_vehicle")
                {
                    Entity client = new Entity("jarvis_vehicle");
                    client.Id = new Guid("6186f7f1-5f09-ed11-82e5-002248934e82");
                    client["jarvis_brandid"] = new EntityReference("jarvis_brand", Guid.NewGuid());
                    client["jarvis_fuelpowertype"] = new EntityReference("jarvis_vehiclefuelpowertype", Guid.NewGuid());
                    return client;
                }

                if (entityName == "jarvis_querydecisioncatagory")
                {
                    Entity client = new Entity("jarvis_querydecisioncatagory");
                    client.Id = Guid.NewGuid();
                    return client;
                }

                if (entityName == "jarvis_country")
                {
                    Entity country = new Entity("jarvis_country");
                    country.Id = guid;
                    country["jarvis_iso2countrycode"] = "NL";
                    return country;
                }

                if (entityName == "jarvis_language")
                {
                    Entity language = new Entity("jarvis_language");
                    language.Id = guid;
                    language["jarvis_iso3languagecode6392t"] = "DEU";
                    language["jarvis_iso2languagecode6391"] = "DE";
                    return language;
                }

                if (entityName == "account")
                {
                    Entity account = new Entity("account");
                    account.Id = guid;
                    account["jarvis_language"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    account["jarvis_address1_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    return account;
                }

                if (entityName == "jarvis_brand")
                {
                    Entity client = new Entity("jarvis_brand");
                    client.Id = new Guid("6186f7f1-5f09-ed11-82e5-002248934e82");
                    client["jarvis_mercuriusbrandcode"] = "ted";
                    return client;
                }

                return null;
            };
            this.Service.ExecuteOrganizationRequest = processInstanceRequest =>
            {
                if (processInstanceRequest.RequestName.Contains("RetrieveProcessInstances"))
                {
                    return new RetrieveProcessInstancesResponse()
                    {
                        Results = new ParameterCollection
                {
                    new KeyValuePair<string, object>("Processes", new EntityCollection(new List<Entity> { incident })),
                },
                    };
                }

                if (processInstanceRequest.RequestName.Contains("RetrieveActivePath"))
                {
                    Entity activepath = new Entity("activepath");
                    activepath["stagename"] = "Waiting For Repair Start";
                    activepath["processstageid"] = Guid.NewGuid();
                    return new RetrieveActivePathResponse()
                    {
                        Results = new ParameterCollection
                {
                    new KeyValuePair<string, object>("ProcessStages", new EntityCollection(new List<Entity> { activepath })),
                },
                    };
                }

                return null;
            };

            CasePostOperationSync plugin = new CasePostOperationSync();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// PreValidate Update With Complete Data Test Method with statuscode 20.
        /// </summary>
        [TestMethod]
        public void PostUpdateWithCompleteDataTestMethodStCode20()
        {
            Entity incidentTarget = new Entity("incident");

            EntityReference users = new EntityReference("systemuser", Guid.NewGuid());
            users.Name = "MERCURIUS";

            incidentTarget.Id = Guid.Empty;

            incidentTarget["statuscode"] = new OptionSetValue(20);
            incidentTarget["incidentid"] = Guid.NewGuid();
            incidentTarget["caseorigincode"] = new OptionSetValue(2);
            incidentTarget["casetypecode"] = new OptionSetValue(2);
            incidentTarget["jarvis_mercuriusstatus"] = new OptionSetValue(200);
            incidentTarget["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
            incidentTarget["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["customerid"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["jarvis_registrationnumber"] = "123";
            incidentTarget["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incidentTarget["description"] = "This is for testing";
            incidentTarget["jarvis_location"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incidentTarget["jarvis_customerexpectations"] = "test";
            incidentTarget["jarvis_incidentnature"] = "[{\"id\":\"3efba61a-85ad-44b7-a187-7f54acaf7a67\"}]";
            incidentTarget["jarvis_callerrole"] = new OptionSetValue(4);
            incidentTarget["jarvis_callerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incidentTarget["isescalated"] = true;
            incidentTarget["jarvis_querydescription"] = "test";
            incidentTarget["jarvis_extrainformationonsolutionquery"] = "test";
            incidentTarget["jarvis_translationstatusalertdecision"] = new OptionSetValue(334030000);
            incidentTarget["jarvis_querydecision"] = "test";
            incidentTarget.Attributes["jarvis_eta"] = DateTime.Now;
            incidentTarget.Attributes["jarvis_atc"] = DateTime.Now;
            incidentTarget.Attributes["modifiedby"] = users;
            incidentTarget.Attributes["modifiedby"] = users;

            Entity incident = new Entity("incident");
            incident.Id = Guid.Empty;
            incident["statuscode"] = new OptionSetValue(20);
            incident["incidentid"] = new EntityReference("incident", Guid.NewGuid());
            incident["caseorigincode"] = new OptionSetValue(2);
            incident["casetypecode"] = new OptionSetValue(2);
            incident["jarvis_mercuriusstatus"] = new OptionSetValue(200);
            incident["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
            incident["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incident["customerid"] = new EntityReference("account", Guid.NewGuid());
            incident["jarvis_registrationnumber"] = "123";
            incident["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["description"] = "This is for testing";
            incident["jarvis_driverlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incident["jarvis_translationstatusreportedfault"] = new OptionSetValue(334030000);
            incident["jarvis_translationstatuslocation"] = new OptionSetValue(334030000);
            incident["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["jarvis_translationstatuscustomerexpectations"] = new OptionSetValue(334030000);
            incident["jarvis_location"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident["jarvis_customerexpectations"] = "test";
            incident["ownerid"] = new EntityReference { LogicalName = "system_user", Name = "MERCURIUS" };
            incident["jarvis_sourceid"] = 232323232;
            incident["jarvis_callerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incident["jarvis_callerrole"] = new OptionSetValue(4);
            incident["createdby"] = users;
            incident["parentcaseid"] = new EntityReference("incident", Guid.NewGuid());
            incident["jarvis_translationstatusalertdescription"] = new OptionSetValue(334030000);
            incident["jarvis_translationstatusalertextrainformation"] = new OptionSetValue(334030000);
            incident["jarvis_translationstatusalertdecision"] = new OptionSetValue(334030000);
            incident["jarvis_extrainformationonsolutionquery"] = "test";
            incident["jarvis_querydecision"] = "test";
            incident["jarvis_onetimecustomerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incident["jarvis_onetimecustomercountry"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incident.Attributes["jarvis_etc"] = DateTime.Now;
            //// Setting Input Parameters.
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", incidentTarget),
               };
            this.PluginExecutionContext.MessageNameGet = () => "Update";
            this.PluginExecutionContext.StageGet = () => 40;
            //// Setting Post and Pre Image
            this.PluginExecutionContext.PostEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PostImage", incident },
                };
            };
            incident["statuscode"] = new OptionSetValue(10);
            this.PluginExecutionContext.PreEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PreImage", incident },
                };
            };

            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();
                if (query.GetType().Name == "FetchExpression")
                {
                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_vasbreakdownprocess'>"))
                    {
                        Entity vasprocess = new Entity("jarvis_vasbreakdownprocess");
                        vasprocess["businessprocessflowinstanceid"] = Guid.NewGuid();
                        vasprocess["bpf_name"] = "test";
                        vasprocess["bpf_incidentid"] = Guid.NewGuid();
                        vasprocess["activestageid"] = Guid.NewGuid();
                        vasprocess["statecode"] = new OptionSetValue(1);
                        vasprocess["createdon"] = DateTime.UtcNow;
                        vasprocess["createdby"] = "test";
                        vasprocess.Id = Guid.NewGuid();
                        result.Entities.Add(vasprocess);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='incident'>"))
                    {
                        Entity incidentResult = new Entity("incident");
                        incidentResult[Constants.IncidentNature.IncidentIncidentNature_jarvisIincidentnatureid] = new AliasedValue("incidentNature", "incidentnatureid", Guid.NewGuid());
                        incidentResult["jarvis_hdrd"] = true;
                        incidentResult["jarvis_homedealer"] = new EntityReference("account", incident.Id);
                        incidentResult["CaseRD.jarvis_repairingdealer"] = new AliasedValue("account", "account", new EntityReference("account", incident.Id));
                        incidentResult.Id = Guid.NewGuid();
                        result.Entities.Add(incidentResult);
                        result.Entities.Add(incidentResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_configurationjarvis'>"))
                    {
                        Entity configResult = new Entity("jarvis_configurationjarvis");
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationcasestatuschange"] = true;
                        configResult["createdon"] = DateTime.UtcNow;
                        configResult.Id = Guid.NewGuid();
                        result.Entities.Add(configResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_language'>"))
                    {
                        Entity language = new Entity("jarvis_language");
                        language.Id = Guid.NewGuid();
                        language["jarvis_iso3languagecode6392t"] = "DEU";
                        language["jarvis_iso2languagecode6391"] = "DE";
                        language["jarvis_name"] = "DE";
                        result.Entities.Add(language);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_casesoftoffer'>"))
                    {
                        Entity casesoftoffer = new Entity("jarvis_casesoftoffer");
                        casesoftoffer.Id = Guid.NewGuid();
                        casesoftoffer["jarvis_name"] = "DEU";
                        casesoftoffer["jarvis_contractno"] = "DE";
                        casesoftoffer["jarvis_description"] = "DE";
                        casesoftoffer["jarvis_startdate"] = DateTime.UtcNow;
                        casesoftoffer["jarvis_expirydate"] = DateTime.UtcNow;
                        casesoftoffer["statuscode"] = new OptionSetValue(0);
                        casesoftoffer["jarvis_softoffercodelookup"] = new EntityReference("jarvis_softoffer", Guid.NewGuid());
                        result.Entities.Add(casesoftoffer);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_softoffer'>"))
                    {
                        Entity softoffer = new Entity("jarvis_softoffer");
                        softoffer.Id = Guid.NewGuid();
                        softoffer["jarvis_name"] = "DEU";
                        softoffer["jarvis_softoffercode"] = new EntityReference("jarvis_softoffer", Guid.NewGuid());
                        softoffer["jarvis_contractno"] = "DE";
                        softoffer["jarvis_startdate"] = DateTime.UtcNow;
                        softoffer["jarvis_expirydate"] = DateTime.UtcNow;
                        softoffer["statuscode"] = new OptionSetValue(0);
                        softoffer["jarvis_marketcode"] = "DE";
                        softoffer["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
                        result.Entities.Add(softoffer);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_repairinformation'>"))
                    {
                        Entity repairInfoResult = new Entity("jarvis_repairinformation");
                        repairInfoResult["jarvis_name"] = "testpassout";
                        repairInfoResult["createdon"] = DateTime.UtcNow;
                        repairInfoResult.Id = Guid.NewGuid();
                        result.Entities.Add(repairInfoResult);
                    }
                }

                return result;
            };

            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "incident")
                {
                    Entity inc = new Entity("incident");
                    inc.Id = guid;
                    inc["customerid"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_callerlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    inc["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    inc["jarvis_driverlanguagesupported"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    inc["jarvis_translationstatusreportedfault"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatuslocation"] = new OptionSetValue(334030000);
                    inc["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    inc["jarvis_translationstatuscustomerexpectations"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatusalertdescription"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatusalertextrainformation"] = new OptionSetValue(334030000);
                    inc["jarvis_translationstatusalertdecision"] = new OptionSetValue(334030000);
                    return inc;
                }

                if (entityName == "jarvis_vehicle")
                {
                    Entity client = new Entity("jarvis_vehicle");
                    client.Id = new Guid("6186f7f1-5f09-ed11-82e5-002248934e82");
                    client["jarvis_brandid"] = new EntityReference("jarvis_brand", Guid.NewGuid());
                    client["jarvis_fuelpowertype"] = new EntityReference("jarvis_vehiclefuelpowertype", Guid.NewGuid());
                    return client;
                }

                if (entityName == "jarvis_country")
                {
                    Entity country = new Entity("jarvis_country");
                    country.Id = guid;
                    country["jarvis_iso2countrycode"] = "NL";
                    return country;
                }

                if (entityName == "jarvis_language")
                {
                    Entity language = new Entity("jarvis_language");
                    language.Id = guid;
                    language["jarvis_iso3languagecode6392t"] = "DEU";
                    language["jarvis_iso2languagecode6391"] = "DE";
                    return language;
                }

                if (entityName == "account")
                {
                    Entity account = new Entity("account");
                    account.Id = guid;
                    account["jarvis_language"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    account["jarvis_address1_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    return account;
                }

                if (entityName == "jarvis_brand")
                {
                    Entity client = new Entity("jarvis_brand");
                    client.Id = new Guid("6186f7f1-5f09-ed11-82e5-002248934e82");
                    client["jarvis_mercuriusbrandcode"] = "ted";
                    return client;
                }

                return null;
            };
            this.Service.ExecuteOrganizationRequest = processInstanceRequest =>
            {
                if (processInstanceRequest.RequestName.Contains("RetrieveProcessInstances"))
                {
                    return new RetrieveProcessInstancesResponse()
                    {
                        Results = new ParameterCollection
                {
                    new KeyValuePair<string, object>("Processes", new EntityCollection(new List<Entity> { incident })),
                },
                    };
                }

                if (processInstanceRequest.RequestName.Contains("RetrieveActivePath"))
                {
                    Entity activepath = new Entity("activepath");
                    activepath["stagename"] = "Waiting For Repair Start";
                    activepath["processstageid"] = Guid.NewGuid();
                    return new RetrieveActivePathResponse()
                    {
                        Results = new ParameterCollection
                {
                    new KeyValuePair<string, object>("ProcessStages", new EntityCollection(new List<Entity> { activepath })),
                },
                    };
                }

                return null;
            };

            CasePostOperationSync plugin = new CasePostOperationSync();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Post Update With Complete Data Test Method with Exception.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(NullReferenceException))]
        public void PostUpdateWithCompleteDataTestMethodwithException()
        {
            Entity incident = new Entity("incident");
            incident.Id = Guid.Empty;
            incident["statuscode"] = new OptionSetValue(10);
            incident["caseorigincode"] = new OptionSetValue(2);
            incident["jarvis_mercuriusstatus"] = new OptionSetValue(200);
            incident["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
            incident["jarvis_homedelaer"] = new EntityReference("account", Guid.NewGuid());
            incident["customerid"] = new EntityReference("account", Guid.NewGuid());
            incident["jarvis_registrationnumber"] = "123";
            //// Setting Input Parameters.
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", incident),
               };
            this.PluginExecutionContext.MessageNameGet = () => "Update";
            this.PluginExecutionContext.StageGet = () => 40;
            CasePostOperationSync plugin = new CasePostOperationSync();
            plugin.Execute(this.ServiceProvider);
        }
    }
}
