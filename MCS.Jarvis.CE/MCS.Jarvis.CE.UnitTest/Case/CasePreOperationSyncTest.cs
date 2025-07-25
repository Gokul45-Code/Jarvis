namespace MCS.Jarvis.CE.UnitTest
{
    using System;
    using System.Collections.Generic;
    using MCS.Jarvis.CE.Plugins;
    using Microsoft.Crm.Sdk.Messages;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;
    using static MCS.Jarvis.CE.BusinessProcessesShared.Helpers.Constants;
    using static MCS.Jarvis.CE.Plugins.Constants;

    /// <summary>
    /// CasePreOperationSyncTest.
    /// </summary>
    [TestClass]
    public class CasePreOperationSyncTest : UnitTestBase
    {
        /// <summary>
        /// PreValidate Update With Complete Data Test Method Create case type code 2.
        /// </summary>
        [TestMethod]
        public void CasePreOperationDepthCheck()
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
            incidentTarget["jarvis_driverlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incident["jarvis_callerrole"] = new OptionSetValue(4);
            incident["createdby"] = users;
            incident.Attributes["jarvis_etc"] = DateTime.Now;
            //// Setting Input Parameters.

            this.PluginExecutionContext.DepthGet = () =>
            {
                return 5;
            };

            CasePreOperationSync plugin = new CasePreOperationSync(null, null);
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// PreValidate Update With Complete Data Test Method Create case type code 2.
        /// </summary>
        [TestMethod]
        public void BreakdownCasePreOperationCreateNoBlacklist()
        {
            Entity incidentTarget = new Entity("incident");

            EntityReference users = new EntityReference("systemuser", Guid.NewGuid());
            users.Name = "MERCURIUS";

            incidentTarget.Id = Guid.Empty;
            Guid sameGuid = Guid.NewGuid();
            incidentTarget["statuscode"] = new OptionSetValue(20);
            incidentTarget["incidentid"] = Guid.NewGuid();
            incidentTarget["caseorigincode"] = new OptionSetValue(2);
            incidentTarget["casetypecode"] = new OptionSetValue(2);
            incidentTarget["jarvis_mercuriusstatus"] = new OptionSetValue(200);
            incidentTarget["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
            incidentTarget["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["customerid"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["jarvis_restgoplimitout"] = 100.0M;
            incidentTarget["jarvis_totalrestcurrencyout"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentTarget["jarvis_totalgoplimitout"] = 100.0M;
            incidentTarget["jarvis_totalcurrencyout"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentTarget["jarvis_totalgoplimitoutapproved"] = 100.0M;
            incidentTarget["jarvis_totalgoplimitoutapprovedcurrency"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentTarget["jarvis_totalrequestedccamount"] = 100.0M;
            incidentTarget["jarvis_totalcreditcardrequestedamountcurreny"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentTarget["jarvis_totalgoplimitin"] = 100.0M;
            incidentTarget["jarvis_totalcurrencyin"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentTarget["jarvis_registrationnumber"] = "123";
            incidentTarget["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incidentTarget["description"] = "This is for testing";
            incidentTarget["jarvis_location"] = new EntityReference("jarvis_country", Guid.NewGuid());
            incidentTarget["jarvis_customerexpectations"] = "test";
            incidentTarget["jarvis_incidentnature"] = "[\r\n  {\r\n    \"id\": \"3efba61a-85ad-44b7-a187-7f54acaf7a67\",\r\n    \"name\": \"Accident\"\r\n  },\r\n  {\r\n    \"_id\": \"3efba61a-85ad-44b7-a187-7f54acaf7a68\",\r\n    \"_name\": \"Air Problem\"\r\n  }\r\n]";
            incidentTarget["jarvis_callerrole"] = new OptionSetValue(4);
            incidentTarget["jarvis_callerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incidentTarget["jarvis_driverlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incidentTarget["isescalated"] = false;
            incidentTarget["createdon"] = DateTime.UtcNow;
            incidentTarget["jarvis_assistancetype"] = new OptionSetValue(334030000);
            incidentTarget.Attributes["modifiedby"] = users;
            incidentTarget.Attributes["createdby"] = users;

            //// Setting Input Parameters.
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", incidentTarget),
               };
            this.PluginExecutionContext.MessageNameGet = () => "CREATE";
            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();
                if (query.GetType().Name == "FetchExpression")
                {


                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_exchangerate'>"))
                    {
                        Entity exchangeRateResult = new Entity("jarvis_exchangerate");
                        exchangeRateResult["jarvis_value"] = 1M;
                        exchangeRateResult.Id = Guid.NewGuid();
                        result.Entities.Add(exchangeRateResult);
                    }


                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_configurationjarvis'>"))
                    {
                        Entity configResult = new Entity("jarvis_configurationjarvis");
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationcasestatuschange"] = true;
                        configResult["jarvis_knowledgesearchforbd"] = "\"breakdown\" AND [incident.jarvis_incidentnatureshadow] AND ([incident.account.HD_country] OR [incident.jarvis_country])";
                        configResult["jarvis_incidentnatureconjunctionbd"] = "AND";
                        configResult["createdon"] = DateTime.UtcNow;
                        configResult.Id = Guid.NewGuid();
                        result.Entities.Add(configResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_region'>"))
                    {
                        Entity regionResult = new Entity("jarvis_region");
                        regionResult["jarvis_name"] = "testpassout";
                        regionResult.Id = Guid.NewGuid();
                        result.Entities.Add(regionResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_vehiclefuelpowertype'>"))
                    {
                        Entity fuelPowerType = new Entity("jarvis_vehiclefuelpowertype");
                        fuelPowerType["jarvis_name"] = "testpassout";
                        fuelPowerType[Constants.IncidentNature.IncidentNatureVehicleFuelPid_jarvisincidentnatureid] = new AliasedValue("incidentNature", "incidentnatureid", Guid.NewGuid());
                        fuelPowerType["IncidentNature_VehicleFuelPid.jarvis_name"] = new AliasedValue("incidentNature", "incidentnatureid", "Air Problem2");
                        fuelPowerType["jarvis_vehiclefuelpowertypeid"] = Guid.NewGuid();
                        fuelPowerType.Id = Guid.NewGuid();
                        result.Entities.Add(fuelPowerType);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='timezonedefinition'>"))
                    {
                        Entity timeZoneDef = new Entity("timezonedefinition");
                        timeZoneDef["userinterfacename"] = "GMT+1TestingpurposeTestMoreTest";
                        timeZoneDef.Id = Guid.NewGuid();
                        result.Entities.Add(timeZoneDef);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='systemuser'>"))
                    {
                        Entity userRef = new Entity("systemuser");
                        userRef["fullname"] = "MERCURIUS";
                        userRef.Id = Guid.NewGuid();
                        result.Entities.Add(userRef);
                    }
                }

                return result;
            };

            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {

                if (entityName == "systemuser")
                {
                    Entity user = new Entity("systemuser");
                    user.Id = Guid.NewGuid();
                    user.Attributes["fullname"] = "MERCURIUS";
                    return user;
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
                    country.Attributes["transactioncurrencyid"] = new EntityReference("transactioncurrency", Guid.NewGuid());
                    country.Attributes["jarvis_timezone"] = 106;
                    return country;
                }

                if (entityName == "jarvis_language")
                {
                    Entity language = new Entity("jarvis_language");
                    language.Id = guid;
                    language["jarvis_iso3languagecode6392t"] = "DEU";
                    language["jarvis_iso2languagecode6391"] = "DE";
                    language["jarvis_vassupportedlanguage"] = true;
                    language["jarvis_vasstandardlanguage"] = true;
                    return language;
                }

                if (entityName == "account")
                {
                    Entity account = new Entity("account");
                    account.Id = guid;
                    account["jarvis_language"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    account["jarvis_address1_country"] = new EntityReference("jarvis_country", sameGuid);
                    account["jarvis_blacklisted"] = false;
                    account["jarvis_onecasestatus"] = new OptionSetValue(334030000);
                    account["jarvis_accounttype"] = new OptionSetValue(334030001);
                    account["jarvis_responsableunitid"] = "NotDummy";
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

            CasePreOperationSync plugin = new CasePreOperationSync(null, null);
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// PreValidate Update With Complete Data Test Method Create case type code 2.
        /// </summary>
        [TestMethod]
        public void BreakdownCasePreOperationCreateNoBlacklist1()
        {
            Entity incidentTarget = new Entity("incident");

            EntityReference users = new EntityReference("systemuser", Guid.NewGuid());
            users.Name = "TEST";

            incidentTarget.Id = Guid.Empty;
            Guid sameGuid = Guid.NewGuid();
            incidentTarget["statuscode"] = new OptionSetValue(20);
            incidentTarget["incidentid"] = Guid.NewGuid();
            incidentTarget["caseorigincode"] = new OptionSetValue(2);
            incidentTarget["casetypecode"] = new OptionSetValue(2);
            incidentTarget["jarvis_mercuriusstatus"] = new OptionSetValue(200);
            incidentTarget["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
            incidentTarget["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["customerid"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["jarvis_restgoplimitout"] = 100.0M;
            incidentTarget["jarvis_totalrestcurrencyout"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentTarget["jarvis_totalgoplimitout"] = 100.0M;
            incidentTarget["jarvis_totalcurrencyout"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentTarget["jarvis_totalgoplimitoutapproved"] = 100.0M;
            incidentTarget["jarvis_totalgoplimitoutapprovedcurrency"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentTarget["jarvis_totalrequestedccamount"] = 100.0M;
            incidentTarget["jarvis_totalcreditcardrequestedamountcurreny"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentTarget["jarvis_totalgoplimitin"] = 100.0M;
            incidentTarget["jarvis_totalcurrencyin"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentTarget["jarvis_registrationnumber"] = "123";
            incidentTarget["jarvis_country"] = new EntityReference("jarvis_country", sameGuid);
            incidentTarget["description"] = "This is for testing";
            incidentTarget["jarvis_location"] = new EntityReference("jarvis_country", sameGuid);
            incidentTarget["jarvis_customerexpectations"] = "test";
            incidentTarget["jarvis_incidentnature"] = "[\r\n  {\r\n    \"id\": \"3efba61a-85ad-44b7-a187-7f54acaf7a67\",\r\n    \"name\": \"Accident\"\r\n  },\r\n  {\r\n    \"_id\": \"3efba61a-85ad-44b7-a187-7f54acaf7a68\",\r\n    \"_name\": \"Air Problem\"\r\n  }\r\n]";
            incidentTarget["jarvis_callerrole"] = new OptionSetValue(4);
            incidentTarget["jarvis_callerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incidentTarget["jarvis_driverlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incidentTarget["isescalated"] = false;
            incidentTarget["createdon"] = DateTime.UtcNow;
            incidentTarget["jarvis_assistancetype"] = new OptionSetValue(334030000);
            incidentTarget.Attributes["modifiedby"] = users;
            incidentTarget.Attributes["createdby"] = users;

            //// Setting Input Parameters.
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", incidentTarget),
               };
            this.PluginExecutionContext.MessageNameGet = () => "CREATE";
            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();
                if (query.GetType().Name == "FetchExpression")
                {


                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_exchangerate'>"))
                    {
                        Entity exchangeRateResult = new Entity("jarvis_exchangerate");
                        exchangeRateResult["jarvis_value"] = 1M;
                        exchangeRateResult.Id = Guid.NewGuid();
                        result.Entities.Add(exchangeRateResult);
                    }


                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_configurationjarvis'>"))
                    {
                        Entity configResult = new Entity("jarvis_configurationjarvis");
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationcasestatuschange"] = true;
                        configResult["jarvis_knowledgesearchforbd"] = "\"breakdown\" AND [incident.jarvis_incidentnatureshadow] AND ([incident.account.HD_country] OR [incident.jarvis_country])";
                        configResult["jarvis_incidentnatureconjunctionbd"] = "AND";
                        configResult["createdon"] = DateTime.UtcNow;
                        configResult.Id = Guid.NewGuid();
                        result.Entities.Add(configResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_region'>"))
                    {
                        Entity regionResult = new Entity("jarvis_region");
                        regionResult["jarvis_name"] = "testpassout";
                        regionResult.Id = Guid.NewGuid();
                        result.Entities.Add(regionResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_vehiclefuelpowertype'>"))
                    {
                        Entity fuelPowerType = new Entity("jarvis_vehiclefuelpowertype");
                        fuelPowerType["jarvis_name"] = "testpassout";
                        fuelPowerType[Constants.IncidentNature.IncidentNatureVehicleFuelPid_jarvisincidentnatureid] = new AliasedValue("incidentNature", "incidentnatureid", Guid.NewGuid());
                        fuelPowerType["IncidentNature_VehicleFuelPid.jarvis_name"] = new AliasedValue("incidentNature", "incidentnatureid", "Air Problem2");
                        fuelPowerType["jarvis_vehiclefuelpowertypeid"] = Guid.NewGuid();
                        fuelPowerType.Id = Guid.NewGuid();
                        result.Entities.Add(fuelPowerType);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='timezonedefinition'>"))
                    {
                        Entity timeZoneDef = new Entity("timezonedefinition");
                        timeZoneDef["userinterfacename"] = "GMT+1TestingpurposeTestMoreTest";
                        timeZoneDef.Id = Guid.NewGuid();
                        result.Entities.Add(timeZoneDef);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='systemuser'>"))
                    {
                        Entity userRef = new Entity("systemuser");
                        userRef["fullname"] = "MERCURIUS";
                        userRef.Id = Guid.NewGuid();
                        result.Entities.Add(userRef);
                    }
                }

                return result;
            };

            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {

                if (entityName == "systemuser")
                {
                    Entity user = new Entity("systemuser");
                    user.Id = Guid.NewGuid();
                    user.Attributes["fullname"] = "TEST";
                    return user;
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
                    country.Id = sameGuid;
                    country["jarvis_iso2countrycode"] = "NL";
                    country.Attributes[JarvisCountry.JarvisAverageetaduration] = "12";
                    country.Attributes["transactioncurrencyid"] = new EntityReference("transactioncurrency", Guid.NewGuid());
                    country.Attributes["jarvis_timezone"] = 106;
                    return country;
                }

                if (entityName == "jarvis_language")
                {
                    Entity language = new Entity("jarvis_language");
                    language.Id = guid;
                    language["jarvis_iso3languagecode6392t"] = "DEU";
                    language["jarvis_iso2languagecode6391"] = "DE";
                    language["jarvis_vassupportedlanguage"] = new EntityReference("jarvis_language", language.Id);
                    language["jarvis_vasstandardlanguage"] = false;
                    return language;
                }

                if (entityName == "account")
                {
                    Entity account = new Entity("account");
                    account.Id = guid;
                    account["jarvis_language"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    account["jarvis_address1_country"] = new EntityReference("jarvis_country", sameGuid);
                    account["jarvis_blacklisted"] = false;
                    account["jarvis_onecasestatus"] = new OptionSetValue(334030000);
                    account["jarvis_accounttype"] = new OptionSetValue(334030001);
                    account["jarvis_responsableunitid"] = "NotDummy";
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

            CasePreOperationSync plugin = new CasePreOperationSync(null, null);
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Pre Create with blacklist as Yes.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidPluginExecutionException))]
        public void BreakdownCasePreOperationCreateYesBlacklist1()
        {
            Entity incidentTarget = new Entity("incident");

            EntityReference users = new EntityReference("systemuser", Guid.NewGuid());
            users.Name = "TEST";

            incidentTarget.Id = Guid.Empty;
            Guid sameGuid = Guid.NewGuid();
            incidentTarget["statuscode"] = new OptionSetValue(20);
            incidentTarget["incidentid"] = Guid.NewGuid();
            incidentTarget["caseorigincode"] = new OptionSetValue(2);
            incidentTarget["casetypecode"] = new OptionSetValue(2);
            incidentTarget["jarvis_mercuriusstatus"] = new OptionSetValue(200);
            incidentTarget["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
            incidentTarget["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["customerid"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["jarvis_restgoplimitout"] = 100.0M;
            incidentTarget["jarvis_totalrestcurrencyout"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentTarget["jarvis_totalgoplimitout"] = 100.0M;
            incidentTarget["jarvis_totalcurrencyout"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentTarget["jarvis_totalgoplimitoutapproved"] = 100.0M;
            incidentTarget["jarvis_totalgoplimitoutapprovedcurrency"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentTarget["jarvis_totalrequestedccamount"] = 100.0M;
            incidentTarget["jarvis_totalcreditcardrequestedamountcurreny"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentTarget["jarvis_totalgoplimitin"] = 100.0M;
            incidentTarget["jarvis_totalcurrencyin"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentTarget["jarvis_registrationnumber"] = "123";
            incidentTarget["jarvis_country"] = new EntityReference("jarvis_country", sameGuid);
            incidentTarget["description"] = "This is for testing";
            incidentTarget["jarvis_location"] = new EntityReference("jarvis_country", sameGuid);
            incidentTarget["jarvis_customerexpectations"] = "test";
            incidentTarget["jarvis_incidentnature"] = "[\r\n  {\r\n    \"id\": \"3efba61a-85ad-44b7-a187-7f54acaf7a67\",\r\n    \"name\": \"Accident\"\r\n  },\r\n  {\r\n    \"_id\": \"3efba61a-85ad-44b7-a187-7f54acaf7a68\",\r\n    \"_name\": \"Air Problem\"\r\n  }\r\n]";
            incidentTarget["jarvis_callerrole"] = new OptionSetValue(4);
            incidentTarget["jarvis_callerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incidentTarget["jarvis_driverlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incidentTarget["isescalated"] = false;
            incidentTarget["createdon"] = DateTime.UtcNow;
            incidentTarget["jarvis_assistancetype"] = new OptionSetValue(334030000);
            incidentTarget.Attributes["modifiedby"] = users;
            incidentTarget.Attributes["createdby"] = users;

            //// Setting Input Parameters.
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", incidentTarget),
               };
            this.PluginExecutionContext.MessageNameGet = () => "CREATE";

            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {

                if (entityName == "systemuser")
                {
                    Entity user = new Entity("systemuser");
                    user.Id = Guid.NewGuid();
                    user.Attributes["fullname"] = "TEST";
                    return user;
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
                    country.Id = sameGuid;
                    country["jarvis_iso2countrycode"] = "NL";
                    country.Attributes[JarvisCountry.JarvisAverageetaduration] = "12";
                    country.Attributes["transactioncurrencyid"] = new EntityReference("transactioncurrency", Guid.NewGuid());
                    country.Attributes["jarvis_timezone"] = 106;
                    return country;
                }

                if (entityName == "jarvis_language")
                {
                    Entity language = new Entity("jarvis_language");
                    language.Id = guid;
                    language["jarvis_iso3languagecode6392t"] = "DEU";
                    language["jarvis_iso2languagecode6391"] = "DE";
                    language["jarvis_vassupportedlanguage"] = new EntityReference("jarvis_language", language.Id);
                    language["jarvis_vasstandardlanguage"] = false;
                    return language;
                }

                if (entityName == "account")
                {
                    Entity account = new Entity("account");
                    account.Id = guid;
                    account["jarvis_language"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    account["jarvis_address1_country"] = new EntityReference("jarvis_country", sameGuid);
                    account["jarvis_blacklisted"] = true;
                    account["jarvis_onecasestatus"] = new OptionSetValue(334030000);
                    account["jarvis_accounttype"] = new OptionSetValue(334030001);
                    account["jarvis_responsableunitid"] = "NotDummy";
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

            CasePreOperationSync plugin = new CasePreOperationSync(null, null);
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// PreValidate Update With Complete Data Test Method Create case type code 3.
        /// </summary>
        [TestMethod]
        public void BreakdownCasePreOperationCreateTypeCode3()
        {
            Entity incidentTarget = new Entity("incident");

            EntityReference users = new EntityReference("systemuser", Guid.NewGuid());
            users.Name = "TEST";

            incidentTarget.Id = Guid.Empty;
            Guid sameGuid = Guid.NewGuid();
            incidentTarget["statuscode"] = new OptionSetValue(20);
            incidentTarget["incidentid"] = Guid.NewGuid();
            incidentTarget["caseorigincode"] = new OptionSetValue(2);
            incidentTarget["casetypecode"] = new OptionSetValue(3);
            incidentTarget["jarvis_mercuriusstatus"] = new OptionSetValue(200);
            incidentTarget["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
            incidentTarget["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["customerid"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["jarvis_restgoplimitout"] = 100.0M;
            incidentTarget["jarvis_totalrestcurrencyout"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentTarget["jarvis_totalgoplimitout"] = 100.0M;
            incidentTarget["jarvis_totalcurrencyout"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentTarget["jarvis_totalgoplimitoutapproved"] = 100.0M;
            incidentTarget["jarvis_totalgoplimitoutapprovedcurrency"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentTarget["jarvis_totalrequestedccamount"] = 100.0M;
            incidentTarget["jarvis_totalcreditcardrequestedamountcurreny"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentTarget["jarvis_totalgoplimitin"] = 100.0M;
            incidentTarget["jarvis_totalcurrencyin"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentTarget["jarvis_registrationnumber"] = "123";
            incidentTarget["jarvis_country"] = new EntityReference("jarvis_country", sameGuid);
            incidentTarget["description"] = "This is for testing";
            incidentTarget["jarvis_location"] = new EntityReference("jarvis_country", sameGuid);
            incidentTarget["jarvis_customerexpectations"] = "test";
            incidentTarget["jarvis_incidentnature"] = "[\r\n  {\r\n    \"id\": \"3efba61a-85ad-44b7-a187-7f54acaf7a67\",\r\n    \"name\": \"Accident\"\r\n  },\r\n  {\r\n    \"_id\": \"3efba61a-85ad-44b7-a187-7f54acaf7a68\",\r\n    \"_name\": \"Air Problem\"\r\n  }\r\n]";
            incidentTarget["jarvis_callerrole"] = new OptionSetValue(4);
            incidentTarget["jarvis_callerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incidentTarget["jarvis_driverlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incidentTarget["isescalated"] = false;
            incidentTarget["createdon"] = DateTime.UtcNow;
            incidentTarget["jarvis_assistancetype"] = new OptionSetValue(334030000);
            incidentTarget.Attributes["modifiedby"] = users;
            incidentTarget.Attributes["createdby"] = users;
            incidentTarget["parentcaseid"] = new EntityReference("incident", Guid.NewGuid());

            //// Setting Input Parameters.
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", incidentTarget),
               };
            this.PluginExecutionContext.MessageNameGet = () => "CREATE";
            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();
                if (query.GetType().Name == "FetchExpression")
                {
                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_exchangerate'>"))
                    {
                        Entity exchangeRateResult = new Entity("jarvis_exchangerate");
                        exchangeRateResult["jarvis_value"] = 1M;
                        exchangeRateResult.Id = Guid.NewGuid();
                        result.Entities.Add(exchangeRateResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_gop'>"))
                    {
                        Entity gop = new Entity("jarvis_gop");
                        gop["modifiedon"] = DateTime.Now;
                        gop.Id = Guid.NewGuid();
                        result.Entities.Add(gop);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_configurationjarvis'>"))
                    {
                        Entity configResult = new Entity("jarvis_configurationjarvis");
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationcasestatuschange"] = true;
                        configResult["jarvis_knowledgesearchforbd"] = "\"breakdown\" AND [incident.jarvis_incidentnatureshadow] AND ([incident.account.HD_country] OR [incident.jarvis_country])";
                        configResult["jarvis_incidentnatureconjunctionbd"] = "AND";
                        configResult["createdon"] = DateTime.UtcNow;
                        configResult.Id = Guid.NewGuid();
                        result.Entities.Add(configResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_region'>"))
                    {
                        Entity regionResult = new Entity("jarvis_region");
                        regionResult["jarvis_name"] = "testpassout";
                        regionResult.Id = Guid.NewGuid();
                        result.Entities.Add(regionResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_vehiclefuelpowertype'>"))
                    {
                        Entity fuelPowerType = new Entity("jarvis_vehiclefuelpowertype");
                        fuelPowerType["jarvis_name"] = "testpassout";
                        fuelPowerType[Constants.IncidentNature.IncidentNatureVehicleFuelPid_jarvisincidentnatureid] = new AliasedValue("incidentNature", "incidentnatureid", Guid.NewGuid());
                        fuelPowerType["IncidentNature_VehicleFuelPid.jarvis_name"] = new AliasedValue("incidentNature", "incidentnatureid", "Air Problem2");
                        fuelPowerType["jarvis_vehiclefuelpowertypeid"] = Guid.NewGuid();
                        fuelPowerType.Id = Guid.NewGuid();
                        result.Entities.Add(fuelPowerType);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='timezonedefinition'>"))
                    {
                        Entity timeZoneDef = new Entity("timezonedefinition");
                        timeZoneDef["userinterfacename"] = "GMT+1TestingpurposeTestMoreTest";
                        timeZoneDef.Id = Guid.NewGuid();
                        result.Entities.Add(timeZoneDef);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='systemuser'>"))
                    {
                        Entity userRef = new Entity("systemuser");
                        userRef["fullname"] = "MERCURIUS";
                        userRef.Id = Guid.NewGuid();
                        result.Entities.Add(userRef);
                    }
                }

                return result;
            };

            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {

                if (entityName == "systemuser")
                {
                    Entity user = new Entity("systemuser");
                    user.Id = Guid.NewGuid();
                    user.Attributes["fullname"] = "TEST";
                    return user;
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
                    country.Id = sameGuid;
                    country["jarvis_iso2countrycode"] = "NL";
                    country.Attributes[JarvisCountry.JarvisAverageetaduration] = "12";
                    country.Attributes["transactioncurrencyid"] = new EntityReference("transactioncurrency", Guid.NewGuid());
                    country.Attributes["jarvis_timezone"] = 106;
                    return country;
                }

                if (entityName == "jarvis_language")
                {
                    Entity language = new Entity("jarvis_language");
                    language.Id = guid;
                    language["jarvis_iso3languagecode6392t"] = "DEU";
                    language["jarvis_iso2languagecode6391"] = "DE";
                    language["jarvis_vasstandardlanguage"] = false;
                    return language;
                }

                if (entityName == "account")
                {
                    Entity account = new Entity("account");
                    account.Id = guid;
                    account["jarvis_language"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    account["jarvis_address1_country"] = new EntityReference("jarvis_country", sameGuid);
                    account["jarvis_blacklisted"] = false;
                    account["jarvis_onecasestatus"] = new OptionSetValue(334030000);
                    account["jarvis_accounttype"] = new OptionSetValue(334030001);
                    account["jarvis_responsableunitid"] = "NotDummy";
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

            CasePreOperationSync plugin = new CasePreOperationSync(null, null);
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// PreValidate Update With Complete Data Test Method Create case type code 3.
        /// </summary>
        [TestMethod]
        public void BreakdownCasePreOperationUpdateTypeCode3()
        {
            Entity incidentTarget = new Entity("incident");
            Entity incidentImgTarget = new Entity("incident");
            EntityReference users = new EntityReference("systemuser", Guid.NewGuid());
            users.Name = "TEST";

            incidentTarget.Id = Guid.Empty;
            Guid sameGuid = Guid.NewGuid();
            incidentTarget["statuscode"] = new OptionSetValue(90);
            incidentTarget["incidentid"] = Guid.NewGuid();
            incidentTarget["caseorigincode"] = new OptionSetValue(2);
            incidentTarget["casetypecode"] = new OptionSetValue(3);
            incidentTarget["jarvis_mercuriusstatus"] = new OptionSetValue(200);
            incidentTarget["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
            incidentTarget["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["customerid"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["jarvis_restgoplimitout"] = 100.0M;
            incidentTarget["jarvis_totalrestcurrencyout"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentTarget["jarvis_totalgoplimitout"] = 100.0M;
            incidentTarget["jarvis_totalcurrencyout"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentTarget["jarvis_totalgoplimitoutapproved"] = 100.0M;
            incidentTarget["jarvis_totalgoplimitoutapprovedcurrency"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentTarget["jarvis_totalrequestedccamount"] = 100.0M;
            incidentTarget["jarvis_totalcreditcardrequestedamountcurreny"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentTarget["jarvis_totalgoplimitin"] = 100.0M;
            incidentTarget["jarvis_totalcurrencyin"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentTarget["jarvis_registrationnumber"] = "123";
            incidentTarget["jarvis_country"] = new EntityReference("jarvis_country", sameGuid);
            incidentTarget["description"] = "This is for testing";
            incidentTarget["jarvis_location"] = new EntityReference("jarvis_country", sameGuid);
            incidentTarget["jarvis_customerexpectations"] = "test";
            incidentTarget["jarvis_incidentnature"] = "[\r\n  {\r\n    \"id\": \"3efba61a-85ad-44b7-a187-7f54acaf7a67\",\r\n    \"name\": \"Accident\"\r\n  },\r\n  {\r\n    \"_id\": \"3efba61a-85ad-44b7-a187-7f54acaf7a68\",\r\n    \"_name\": \"Air Problem\"\r\n  }\r\n]";
            incidentTarget["jarvis_callerrole"] = new OptionSetValue(4);
            incidentTarget["jarvis_callerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incidentTarget["jarvis_driverlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incidentTarget["isescalated"] = false;
            incidentTarget["createdon"] = DateTime.UtcNow;
            incidentTarget["jarvis_assistancetype"] = new OptionSetValue(334030000);
            incidentTarget.Attributes["modifiedby"] = users;
            incidentTarget.Attributes["createdby"] = users;
            incidentTarget["parentcaseid"] = new EntityReference("incident", Guid.NewGuid());
            incidentTarget["jarvis_customerinformed"] = true;
            incidentTarget["ownerid"] = new EntityReference("systemuser", Guid.NewGuid());

            incidentImgTarget["jarvis_fumonitorskill"] = new EntityReference("jarvis_fumonitorskill", Guid.NewGuid());
            incidentImgTarget["jarvis_futimestamp"] = DateTime.Now;
            incidentImgTarget["statuscode"] = new OptionSetValue(90);
            incidentImgTarget["statecode"] = new OptionSetValue(2);
            incidentImgTarget["incidentid"] = Guid.NewGuid();
            incidentImgTarget["caseorigincode"] = new OptionSetValue(2);
            incidentImgTarget["casetypecode"] = new OptionSetValue(3);
            incidentImgTarget["jarvis_mercuriusstatus"] = new OptionSetValue(200);
            incidentImgTarget["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
            incidentImgTarget["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incidentImgTarget["customerid"] = new EntityReference("account", Guid.NewGuid());
            incidentImgTarget["jarvis_restgoplimitout"] = 100.0M;
            incidentImgTarget["jarvis_totalrestcurrencyout"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentImgTarget["jarvis_totalgoplimitout"] = 100.0M;
            incidentImgTarget["jarvis_totalcurrencyout"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentImgTarget["jarvis_totalgoplimitoutapproved"] = 100.0M;
            incidentImgTarget["jarvis_totalgoplimitoutapprovedcurrency"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentImgTarget["jarvis_totalrequestedccamount"] = 100.0M;
            incidentImgTarget["jarvis_totalcreditcardrequestedamountcurreny"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentImgTarget["jarvis_totalgoplimitin"] = 100.0M;
            incidentImgTarget["jarvis_totalcurrencyin"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentImgTarget["jarvis_registrationnumber"] = "123";
            incidentImgTarget["jarvis_country"] = new EntityReference("jarvis_country", sameGuid);
            incidentImgTarget["description"] = "This is for testing";
            incidentImgTarget["jarvis_location"] = new EntityReference("jarvis_country", sameGuid);
            incidentImgTarget["jarvis_customerexpectations"] = "test";
            incidentImgTarget["jarvis_incidentnature"] = "[\r\n  {\r\n    \"id\": \"3efba61a-85ad-44b7-a187-7f54acaf7a67\",\r\n    \"name\": \"Accident\"\r\n  },\r\n  {\r\n    \"_id\": \"3efba61a-85ad-44b7-a187-7f54acaf7a68\",\r\n    \"_name\": \"Air Problem\"\r\n  }\r\n]";
            incidentImgTarget["jarvis_callerrole"] = new OptionSetValue(4);
            incidentImgTarget["jarvis_callerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incidentImgTarget["jarvis_driverlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incidentImgTarget["isescalated"] = false;
            incidentImgTarget["createdon"] = DateTime.UtcNow;
            incidentImgTarget["jarvis_assistancetype"] = new OptionSetValue(334030000);
            incidentImgTarget.Attributes["modifiedby"] = users;
            incidentImgTarget.Attributes["createdby"] = users;
            incidentImgTarget["parentcaseid"] = new EntityReference("incident", Guid.NewGuid());
            incidentImgTarget["jarvis_hdrd"] = true;

            //// Setting Input Parameters.
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", incidentTarget),
               };
            this.PluginExecutionContext.PreEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PreImage", incidentImgTarget },
                };
            };
            this.PluginExecutionContext.MessageNameGet = () => "UPDATE";
            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();
                if (query.GetType().Name == "FetchExpression")
                {
                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_exchangerate'>"))
                    {
                        Entity exchangeRateResult = new Entity("jarvis_exchangerate");
                        exchangeRateResult["jarvis_value"] = 1M;
                        exchangeRateResult.Id = Guid.NewGuid();
                        result.Entities.Add(exchangeRateResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_gop'>"))
                    {
                        Entity gop = new Entity("jarvis_gop");
                        gop["modifiedon"] = DateTime.Now;
                        gop.Id = Guid.NewGuid();
                        result.Entities.Add(gop);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_configurationjarvis'>"))
                    {
                        Entity configResult = new Entity("jarvis_configurationjarvis");
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationcasestatuschange"] = true;
                        configResult["jarvis_automationreleasecase"] = new OptionSetValue(0);
                        configResult["jarvis_knowledgesearchforbd"] = "\"breakdown\" AND [incident.jarvis_incidentnatureshadow] AND ([incident.account.HD_country] OR [incident.jarvis_country])";
                        configResult["jarvis_incidentnatureconjunctionbd"] = "AND";
                        configResult["createdon"] = DateTime.UtcNow;
                        configResult.Id = Guid.NewGuid();
                        result.Entities.Add(configResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_region'>"))
                    {
                        Entity regionResult = new Entity("jarvis_region");
                        regionResult["jarvis_name"] = "testpassout";
                        regionResult.Id = Guid.NewGuid();
                        result.Entities.Add(regionResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_vehiclefuelpowertype'>"))
                    {
                        Entity fuelPowerType = new Entity("jarvis_vehiclefuelpowertype");
                        fuelPowerType["jarvis_name"] = "testpassout";
                        fuelPowerType[Constants.IncidentNature.IncidentNatureVehicleFuelPid_jarvisincidentnatureid] = new AliasedValue("incidentNature", "incidentnatureid", Guid.NewGuid());
                        fuelPowerType["IncidentNature_VehicleFuelPid.jarvis_name"] = new AliasedValue("incidentNature", "incidentnatureid", "Air Problem2");
                        fuelPowerType["jarvis_vehiclefuelpowertypeid"] = Guid.NewGuid();
                        fuelPowerType.Id = Guid.NewGuid();
                        result.Entities.Add(fuelPowerType);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='timezonedefinition'>"))
                    {
                        Entity timeZoneDef = new Entity("timezonedefinition");
                        timeZoneDef["userinterfacename"] = "GMT+1TestingpurposeTestMoreTest";
                        timeZoneDef.Id = Guid.NewGuid();
                        result.Entities.Add(timeZoneDef);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='systemuser'>"))
                    {
                        Entity userRef = new Entity("systemuser");
                        userRef["fullname"] = "MERCURIUS";
                        userRef.Id = Guid.NewGuid();
                        result.Entities.Add(userRef);
                    }
                }

                return result;
            };

            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {

                if (entityName == "systemuser")
                {
                    Entity user = new Entity("systemuser");
                    user.Id = Guid.NewGuid();
                    user.Attributes["fullname"] = "TEST";
                    return user;
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
                    country.Id = sameGuid;
                    country["jarvis_iso2countrycode"] = "NL";
                    country.Attributes[JarvisCountry.JarvisAverageetaduration] = "12";
                    country.Attributes["transactioncurrencyid"] = new EntityReference("transactioncurrency", Guid.NewGuid());
                    country.Attributes["jarvis_timezone"] = 106;
                    return country;
                }

                if (entityName == "jarvis_language")
                {
                    Entity language = new Entity("jarvis_language");
                    language.Id = guid;
                    language["jarvis_iso3languagecode6392t"] = "DEU";
                    language["jarvis_iso2languagecode6391"] = "DE";
                    language["jarvis_vasstandardlanguage"] = false;
                    return language;
                }

                if (entityName == "account")
                {
                    Entity account = new Entity("account");
                    account.Id = guid;
                    account["jarvis_language"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    account["jarvis_address1_country"] = new EntityReference("jarvis_country", sameGuid);
                    account["jarvis_blacklisted"] = false;
                    account["jarvis_onecasestatus"] = new OptionSetValue(334030000);
                    account["jarvis_accounttype"] = new OptionSetValue(334030001);
                    account["jarvis_responsableunitid"] = "NotDummy";
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

            CasePreOperationSync plugin = new CasePreOperationSync(null, null);
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// PreValidate Update With Complete Data Test Method Create case type code 3.
        /// </summary>
        [TestMethod]
        public void BreakdownCasePreOperationUpdateTypeCode900()
        {
            Entity incidentTarget = new Entity("incident");
            Entity incidentImgTarget = new Entity("incident");
            EntityReference users = new EntityReference("systemuser", Guid.NewGuid());
            users.Name = "TEST";

            incidentTarget.Id = Guid.Empty;
            Guid sameGuid = Guid.NewGuid();
            incidentTarget["incidentid"] = Guid.NewGuid();
            incidentTarget["caseorigincode"] = new OptionSetValue(2);
            incidentTarget["casetypecode"] = new OptionSetValue(2);
            incidentTarget["jarvis_mercuriusstatus"] = new OptionSetValue(900);
            incidentTarget["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
            incidentTarget["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["customerid"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["jarvis_restgoplimitout"] = 100.0M;
            incidentTarget["jarvis_totalrestcurrencyout"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentTarget["jarvis_totalgoplimitout"] = 100.0M;
            incidentTarget["jarvis_totalcurrencyout"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentTarget["jarvis_totalgoplimitoutapproved"] = 100.0M;
            incidentTarget["jarvis_totalgoplimitoutapprovedcurrency"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentTarget["jarvis_totalrequestedccamount"] = 100.0M;
            incidentTarget["jarvis_totalcreditcardrequestedamountcurreny"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentTarget["jarvis_totalgoplimitin"] = 100.0M;
            incidentTarget["jarvis_totalcurrencyin"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentTarget["jarvis_registrationnumber"] = "123";
            incidentTarget["jarvis_country"] = new EntityReference("jarvis_country", sameGuid);
            incidentTarget["description"] = "This is for testing";
            incidentTarget["jarvis_location"] = new EntityReference("jarvis_country", sameGuid);
            incidentTarget["jarvis_customerexpectations"] = "test";
            incidentTarget["jarvis_incidentnature"] = "[\r\n  {\r\n    \"id\": \"3efba61a-85ad-44b7-a187-7f54acaf7a67\",\r\n    \"name\": \"Accident\"\r\n  },\r\n  {\r\n    \"_id\": \"3efba61a-85ad-44b7-a187-7f54acaf7a68\",\r\n    \"_name\": \"Air Problem\"\r\n  }\r\n]";
            incidentTarget["jarvis_callerrole"] = new OptionSetValue(4);
            incidentTarget["jarvis_callerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incidentTarget["jarvis_driverlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incidentTarget["isescalated"] = false;
            incidentTarget["createdon"] = DateTime.UtcNow;
            incidentTarget["jarvis_assistancetype"] = new OptionSetValue(334030000);
            incidentTarget.Attributes["modifiedby"] = users;
            incidentTarget.Attributes["createdby"] = users;
            incidentTarget["parentcaseid"] = new EntityReference("incident", Guid.NewGuid());
            incidentTarget["jarvis_customerinformed"] = true;
            incidentTarget["ownerid"] = new EntityReference("systemuser", Guid.NewGuid());
            incidentTarget["jarvis_fumonitorskill"] = new EntityReference("jarvis_fumonitorskill", Guid.NewGuid());
            incidentTarget["jarvis_futimestamp"] = DateTime.Now;

            incidentImgTarget["statuscode"] = new OptionSetValue(90);
            incidentImgTarget["incidentid"] = Guid.NewGuid();
            incidentImgTarget["statecode"] = new OptionSetValue(2);
            incidentImgTarget["caseorigincode"] = new OptionSetValue(2);
            incidentImgTarget["casetypecode"] = new OptionSetValue(2);
            incidentImgTarget["jarvis_mercuriusstatus"] = new OptionSetValue(900);
            incidentImgTarget["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
            incidentImgTarget["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incidentImgTarget["customerid"] = new EntityReference("account", Guid.NewGuid());
            incidentImgTarget["jarvis_restgoplimitout"] = 100.0M;
            incidentImgTarget["jarvis_totalrestcurrencyout"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentImgTarget["jarvis_totalgoplimitout"] = 100.0M;
            incidentImgTarget["jarvis_totalcurrencyout"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentImgTarget["jarvis_totalgoplimitoutapproved"] = 100.0M;
            incidentImgTarget["jarvis_totalgoplimitoutapprovedcurrency"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentImgTarget["jarvis_totalrequestedccamount"] = 100.0M;
            incidentImgTarget["jarvis_totalcreditcardrequestedamountcurreny"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentImgTarget["jarvis_totalgoplimitin"] = 100.0M;
            incidentImgTarget["jarvis_totalcurrencyin"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentImgTarget["jarvis_registrationnumber"] = "123";
            incidentImgTarget["jarvis_country"] = new EntityReference("jarvis_country", sameGuid);
            incidentImgTarget["description"] = "This is for testing";
            incidentImgTarget["jarvis_location"] = new EntityReference("jarvis_country", sameGuid);
            incidentImgTarget["jarvis_customerexpectations"] = "test";
            incidentImgTarget["jarvis_incidentnature"] = "[\r\n  {\r\n    \"id\": \"3efba61a-85ad-44b7-a187-7f54acaf7a67\",\r\n    \"name\": \"Accident\"\r\n  },\r\n  {\r\n    \"_id\": \"3efba61a-85ad-44b7-a187-7f54acaf7a68\",\r\n    \"_name\": \"Air Problem\"\r\n  }\r\n]";
            incidentImgTarget["jarvis_callerrole"] = new OptionSetValue(4);
            incidentImgTarget["jarvis_callerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incidentImgTarget["jarvis_driverlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incidentImgTarget["isescalated"] = false;
            incidentImgTarget["createdon"] = DateTime.UtcNow;
            incidentImgTarget["jarvis_assistancetype"] = new OptionSetValue(334030000);
            incidentImgTarget.Attributes["modifiedby"] = users;
            incidentImgTarget.Attributes["createdby"] = users;
            incidentImgTarget["parentcaseid"] = new EntityReference("incident", Guid.NewGuid());
            incidentImgTarget["jarvis_hdrd"] = true;

            //// Setting Input Parameters.
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", incidentTarget),
               };
            this.PluginExecutionContext.PreEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PreImage", incidentImgTarget },
                };
            };
            this.PluginExecutionContext.MessageNameGet = () => "UPDATE";
            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();
                if (query.GetType().Name == "FetchExpression")
                {
                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_exchangerate'>"))
                    {
                        Entity exchangeRateResult = new Entity("jarvis_exchangerate");
                        exchangeRateResult["jarvis_value"] = 1M;
                        exchangeRateResult.Id = Guid.NewGuid();
                        result.Entities.Add(exchangeRateResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_gop'>"))
                    {
                        Entity gop = new Entity("jarvis_gop");
                        gop["modifiedon"] = DateTime.Now;
                        gop.Id = Guid.NewGuid();
                        result.Entities.Add(gop);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_configurationjarvis'>"))
                    {
                        Entity configResult = new Entity("jarvis_configurationjarvis");
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationcasestatuschange"] = true;
                        configResult["jarvis_automationreleasecase"] = new OptionSetValue(1);
                        configResult["jarvis_knowledgesearchforbd"] = "\"breakdown\" AND [incident.jarvis_incidentnatureshadow] AND ([incident.account.HD_country] OR [incident.jarvis_country])";
                        configResult["jarvis_incidentnatureconjunctionbd"] = "AND";
                        configResult["createdon"] = DateTime.UtcNow;
                        configResult.Id = Guid.NewGuid();
                        result.Entities.Add(configResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_region'>"))
                    {
                        Entity regionResult = new Entity("jarvis_region");
                        regionResult["jarvis_name"] = "testpassout";
                        regionResult.Id = Guid.NewGuid();
                        result.Entities.Add(regionResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_vehiclefuelpowertype'>"))
                    {
                        Entity fuelPowerType = new Entity("jarvis_vehiclefuelpowertype");
                        fuelPowerType["jarvis_name"] = "testpassout";
                        fuelPowerType[Constants.IncidentNature.IncidentNatureVehicleFuelPid_jarvisincidentnatureid] = new AliasedValue("incidentNature", "incidentnatureid", Guid.NewGuid());
                        fuelPowerType["IncidentNature_VehicleFuelPid.jarvis_name"] = new AliasedValue("incidentNature", "incidentnatureid", "Air Problem2");
                        fuelPowerType["jarvis_vehiclefuelpowertypeid"] = Guid.NewGuid();
                        fuelPowerType.Id = Guid.NewGuid();
                        result.Entities.Add(fuelPowerType);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='timezonedefinition'>"))
                    {
                        Entity timeZoneDef = new Entity("timezonedefinition");
                        timeZoneDef["userinterfacename"] = "GMT+1TestingpurposeTestMoreTest";
                        timeZoneDef.Id = Guid.NewGuid();
                        result.Entities.Add(timeZoneDef);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='systemuser'>"))
                    {
                        Entity userRef = new Entity("systemuser");
                        userRef["fullname"] = "MERCURIUS";
                        userRef.Id = Guid.NewGuid();
                        result.Entities.Add(userRef);
                    }

                    if((query as FetchExpression).Query.Contains("<entity name='queueitem'>"))
                    {
                        Entity queue = new Entity("queueitem");
                        queue["fullname"] = "MERCURIUS";
                        queue.Id = Guid.NewGuid();
                        result.Entities.Add(queue);
                    }
                }

                return result;
            };

            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {

                if (entityName == "systemuser")
                {
                    Entity user = new Entity("systemuser");
                    user.Id = Guid.NewGuid();
                    user.Attributes["fullname"] = "TEST";
                    return user;
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
                    country.Id = sameGuid;
                    country["jarvis_iso2countrycode"] = "NL";
                    country.Attributes[JarvisCountry.JarvisAverageetaduration] = "12";
                    country.Attributes["transactioncurrencyid"] = new EntityReference("transactioncurrency", Guid.NewGuid());
                    country.Attributes["jarvis_timezone"] = 106;
                    return country;
                }

                if (entityName == "jarvis_language")
                {
                    Entity language = new Entity("jarvis_language");
                    language.Id = guid;
                    language["jarvis_iso3languagecode6392t"] = "DEU";
                    language["jarvis_iso2languagecode6391"] = "DE";
                    language["jarvis_vasstandardlanguage"] = false;
                    return language;
                }

                if (entityName == "account")
                {
                    Entity account = new Entity("account");
                    account.Id = guid;
                    account["jarvis_language"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    account["jarvis_address1_country"] = new EntityReference("jarvis_country", sameGuid);
                    account["jarvis_blacklisted"] = false;
                    account["jarvis_onecasestatus"] = new OptionSetValue(334030000);
                    account["jarvis_accounttype"] = new OptionSetValue(334030001);
                    account["jarvis_responsableunitid"] = "NotDummy";
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

            CasePreOperationSync plugin = new CasePreOperationSync(null, null);
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// PreValidate Update With Complete Data Test Method Create case type code 3.
        /// </summary>
        [TestMethod]
        public void BreakdownCasePreOperationUpdateTypeCode200()
        {
            Entity incidentTarget = new Entity("incident");
            Entity incidentImgTarget = new Entity("incident");
            EntityReference users = new EntityReference("systemuser", Guid.NewGuid());
            users.Name = "TEST";

            incidentTarget.Id = Guid.Empty;
            Guid sameGuid = Guid.NewGuid();
            incidentTarget["incidentid"] = Guid.NewGuid();
            incidentTarget["caseorigincode"] = new OptionSetValue(2);
            incidentTarget["casetypecode"] = new OptionSetValue(2);
            incidentTarget["jarvis_mercuriusstatus"] = new OptionSetValue(200);
            incidentTarget["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
            incidentTarget["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["customerid"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["jarvis_restgoplimitout"] = 100.0M;
            incidentTarget["jarvis_totalrestcurrencyout"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentTarget["jarvis_totalgoplimitout"] = 100.0M;
            incidentTarget["jarvis_totalcurrencyout"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentTarget["jarvis_totalgoplimitoutapproved"] = 100.0M;
            incidentTarget["jarvis_totalgoplimitoutapprovedcurrency"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentTarget["jarvis_totalrequestedccamount"] = 100.0M;
            incidentTarget["jarvis_totalcreditcardrequestedamountcurreny"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentTarget["jarvis_totalgoplimitin"] = 100.0M;
            incidentTarget["jarvis_totalcurrencyin"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentTarget["jarvis_registrationnumber"] = "123";
            incidentTarget["jarvis_country"] = new EntityReference("jarvis_country", sameGuid);
            incidentTarget["description"] = "This is for testing";
            incidentTarget["jarvis_location"] = new EntityReference("jarvis_country", sameGuid);
            incidentTarget["jarvis_customerexpectations"] = "test";
            incidentTarget["jarvis_incidentnature"] = "[\r\n  {\r\n    \"id\": \"3efba61a-85ad-44b7-a187-7f54acaf7a67\",\r\n    \"name\": \"Accident\"\r\n  },\r\n  {\r\n    \"_id\": \"3efba61a-85ad-44b7-a187-7f54acaf7a68\",\r\n    \"_name\": \"Air Problem\"\r\n  }\r\n]";
            incidentTarget["jarvis_callerrole"] = new OptionSetValue(4);
            incidentTarget["jarvis_callerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incidentTarget["jarvis_driverlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incidentTarget["isescalated"] = false;
            incidentTarget["createdon"] = DateTime.UtcNow;
            incidentTarget["jarvis_assistancetype"] = new OptionSetValue(334030000);
            incidentTarget.Attributes["modifiedby"] = users;
            incidentTarget.Attributes["createdby"] = users;
            incidentTarget["parentcaseid"] = new EntityReference("incident", Guid.NewGuid());
            incidentTarget["ownerid"] = new EntityReference("systemuser", Guid.NewGuid());
            incidentTarget["jarvis_fumonitorskill"] = new EntityReference("jarvis_fumonitorskill", Guid.NewGuid());
            incidentTarget["jarvis_futimestamp"] = DateTime.Now;
            incidentTarget["statuscode"] = new OptionSetValue(20);

            incidentImgTarget["jarvis_casestatusupdate"] = DateTime.Now;
            incidentImgTarget["statuscode"] = new OptionSetValue(90);
            incidentImgTarget["incidentid"] = Guid.NewGuid();
            incidentImgTarget["statecode"] = new OptionSetValue(2);
            incidentImgTarget["caseorigincode"] = new OptionSetValue(2);
            incidentImgTarget["casetypecode"] = new OptionSetValue(2);
            incidentImgTarget["jarvis_mercuriusstatus"] = new OptionSetValue(200);
            incidentImgTarget["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
            incidentImgTarget["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incidentImgTarget["customerid"] = new EntityReference("account", Guid.NewGuid());
            incidentImgTarget["jarvis_restgoplimitout"] = 100.0M;
            incidentImgTarget["jarvis_totalrestcurrencyout"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentImgTarget["jarvis_totalgoplimitout"] = 100.0M;
            incidentImgTarget["jarvis_totalcurrencyout"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentImgTarget["jarvis_totalgoplimitoutapproved"] = 100.0M;
            incidentImgTarget["jarvis_totalgoplimitoutapprovedcurrency"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentImgTarget["jarvis_totalrequestedccamount"] = 100.0M;
            incidentImgTarget["jarvis_totalcreditcardrequestedamountcurreny"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentImgTarget["jarvis_totalgoplimitin"] = 100.0M;
            incidentImgTarget["jarvis_totalcurrencyin"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentImgTarget["jarvis_registrationnumber"] = "123";
            incidentImgTarget["jarvis_country"] = new EntityReference("jarvis_country", sameGuid);
            incidentImgTarget["description"] = "This is for testing";
            incidentImgTarget["jarvis_location"] = new EntityReference("jarvis_country", sameGuid);
            incidentImgTarget["jarvis_customerexpectations"] = "test";
            incidentImgTarget["jarvis_incidentnature"] = "[\r\n  {\r\n    \"id\": \"3efba61a-85ad-44b7-a187-7f54acaf7a67\",\r\n    \"name\": \"Accident\"\r\n  },\r\n  {\r\n    \"_id\": \"3efba61a-85ad-44b7-a187-7f54acaf7a68\",\r\n    \"_name\": \"Air Problem\"\r\n  }\r\n]";
            incidentImgTarget["jarvis_callerrole"] = new OptionSetValue(4);
            incidentImgTarget["jarvis_callerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incidentImgTarget["jarvis_driverlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incidentImgTarget["isescalated"] = false;
            incidentImgTarget["createdon"] = DateTime.UtcNow;
            incidentImgTarget["jarvis_assistancetype"] = new OptionSetValue(334030000);
            incidentImgTarget.Attributes["modifiedby"] = users;
            incidentImgTarget.Attributes["createdby"] = users;
            incidentImgTarget["parentcaseid"] = new EntityReference("incident", Guid.NewGuid());
            incidentImgTarget["jarvis_hdrd"] = true;

            //// Setting Input Parameters.
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", incidentTarget),
               };
            this.PluginExecutionContext.PreEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PreImage", incidentImgTarget },
                };
            };
            this.PluginExecutionContext.MessageNameGet = () => "UPDATE";
            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();
                if (query.GetType().Name == "FetchExpression")
                {
                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_exchangerate'>"))
                    {
                        Entity exchangeRateResult = new Entity("jarvis_exchangerate");
                        exchangeRateResult["jarvis_value"] = 1M;
                        exchangeRateResult.Id = Guid.NewGuid();
                        result.Entities.Add(exchangeRateResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_gop'>"))
                    {
                        Entity gop = new Entity("jarvis_gop");
                        gop["modifiedon"] = DateTime.Now;
                        gop.Id = Guid.NewGuid();
                        result.Entities.Add(gop);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_configurationjarvis'>"))
                    {
                        Entity configResult = new Entity("jarvis_configurationjarvis");
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationcasestatuschange"] = true;
                        configResult["jarvis_automationreleasecase"] = new OptionSetValue(1);
                        configResult["jarvis_knowledgesearchforbd"] = "\"breakdown\" AND [incident.jarvis_incidentnatureshadow] AND ([incident.account.HD_country] OR [incident.jarvis_country])";
                        configResult["jarvis_incidentnatureconjunctionbd"] = "AND";
                        configResult["createdon"] = DateTime.UtcNow;
                        configResult.Id = Guid.NewGuid();
                        result.Entities.Add(configResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_region'>"))
                    {
                        Entity regionResult = new Entity("jarvis_region");
                        regionResult["jarvis_name"] = "testpassout";
                        regionResult.Id = Guid.NewGuid();
                        result.Entities.Add(regionResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_vehiclefuelpowertype'>"))
                    {
                        Entity fuelPowerType = new Entity("jarvis_vehiclefuelpowertype");
                        fuelPowerType["jarvis_name"] = "testpassout";
                        fuelPowerType[Constants.IncidentNature.IncidentNatureVehicleFuelPid_jarvisincidentnatureid] = new AliasedValue("incidentNature", "incidentnatureid", Guid.NewGuid());
                        fuelPowerType["IncidentNature_VehicleFuelPid.jarvis_name"] = new AliasedValue("incidentNature", "incidentnatureid", "Air Problem2");
                        fuelPowerType["jarvis_vehiclefuelpowertypeid"] = Guid.NewGuid();
                        fuelPowerType.Id = Guid.NewGuid();
                        result.Entities.Add(fuelPowerType);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='timezonedefinition'>"))
                    {
                        Entity timeZoneDef = new Entity("timezonedefinition");
                        timeZoneDef["userinterfacename"] = "GMT+1TestingpurposeTestMoreTest";
                        timeZoneDef.Id = Guid.NewGuid();
                        result.Entities.Add(timeZoneDef);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='systemuser'>"))
                    {
                        Entity userRef = new Entity("systemuser");
                        userRef["fullname"] = "MERCURIUS";
                        userRef.Id = Guid.NewGuid();
                        result.Entities.Add(userRef);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='queueitem'>"))
                    {
                        Entity queue = new Entity("queueitem");
                        queue["fullname"] = "MERCURIUS";
                        queue.Id = Guid.NewGuid();
                        result.Entities.Add(queue);
                    }
                }

                return result;
            };

            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {

                if (entityName == "systemuser")
                {
                    Entity user = new Entity("systemuser");
                    user.Id = Guid.NewGuid();
                    user.Attributes["fullname"] = "TEST";
                    return user;
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
                    country.Id = sameGuid;
                    country["jarvis_iso2countrycode"] = "NL";
                    country.Attributes[JarvisCountry.JarvisAverageetaduration] = "12";
                    country.Attributes["transactioncurrencyid"] = new EntityReference("transactioncurrency", Guid.NewGuid());
                    country.Attributes["jarvis_timezone"] = 106;
                    return country;
                }

                if (entityName == "jarvis_language")
                {
                    Entity language = new Entity("jarvis_language");
                    language.Id = guid;
                    language["jarvis_iso3languagecode6392t"] = "DEU";
                    language["jarvis_iso2languagecode6391"] = "DE";
                    language["jarvis_vasstandardlanguage"] = false;
                    return language;
                }

                if (entityName == "account")
                {
                    Entity account = new Entity("account");
                    account.Id = guid;
                    account["jarvis_language"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    account["jarvis_address1_country"] = new EntityReference("jarvis_country", sameGuid);
                    account["jarvis_blacklisted"] = false;
                    account["jarvis_onecasestatus"] = new OptionSetValue(334030000);
                    account["jarvis_accounttype"] = new OptionSetValue(334030001);
                    account["jarvis_responsableunitid"] = "NotDummy";
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

            CasePreOperationSync plugin = new CasePreOperationSync(null, null);
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// PreValidate Update With Complete Data Test Method Create case type code 3.
        /// </summary>
        [TestMethod]
        public void BreakdownCasePreOperationUpdateTypeCode300()
        {
            Entity incidentTarget = new Entity("incident");
            Entity incidentImgTarget = new Entity("incident");
            EntityReference users = new EntityReference("systemuser", Guid.NewGuid());
            users.Name = "TEST";

            incidentTarget.Id = Guid.Empty;
            Guid sameGuid = Guid.NewGuid();
            incidentTarget["incidentid"] = Guid.NewGuid();
            incidentTarget["caseorigincode"] = new OptionSetValue(2);
            incidentTarget["casetypecode"] = new OptionSetValue(2);
            incidentTarget["jarvis_mercuriusstatus"] = new OptionSetValue(300);
            incidentTarget["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
            incidentTarget["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["customerid"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["jarvis_restgoplimitout"] = 100.0M;
            incidentTarget["jarvis_totalrestcurrencyout"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentTarget["jarvis_totalgoplimitout"] = 100.0M;
            incidentTarget["jarvis_totalcurrencyout"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentTarget["jarvis_totalgoplimitoutapproved"] = 100.0M;
            incidentTarget["jarvis_totalgoplimitoutapprovedcurrency"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentTarget["jarvis_totalrequestedccamount"] = 100.0M;
            incidentTarget["jarvis_totalcreditcardrequestedamountcurreny"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentTarget["jarvis_totalgoplimitin"] = 100.0M;
            incidentTarget["jarvis_totalcurrencyin"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentTarget["jarvis_registrationnumber"] = "123";
            incidentTarget["jarvis_country"] = new EntityReference("jarvis_country", sameGuid);
            incidentTarget["description"] = "This is for testing";
            incidentTarget["jarvis_location"] = new EntityReference("jarvis_country", sameGuid);
            incidentTarget["jarvis_customerexpectations"] = "test";
            incidentTarget["jarvis_incidentnature"] = "[\r\n  {\r\n    \"id\": \"3efba61a-85ad-44b7-a187-7f54acaf7a67\",\r\n    \"name\": \"Accident\"\r\n  },\r\n  {\r\n    \"_id\": \"3efba61a-85ad-44b7-a187-7f54acaf7a68\",\r\n    \"_name\": \"Air Problem\"\r\n  }\r\n]";
            incidentTarget["jarvis_callerrole"] = new OptionSetValue(4);
            incidentTarget["jarvis_callerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incidentTarget["jarvis_driverlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incidentTarget["isescalated"] = false;
            incidentTarget["createdon"] = DateTime.UtcNow;
            incidentTarget["jarvis_assistancetype"] = new OptionSetValue(334030000);
            incidentTarget.Attributes["modifiedby"] = users;
            incidentTarget.Attributes["createdby"] = users;
            incidentTarget["parentcaseid"] = new EntityReference("incident", Guid.NewGuid());
            incidentTarget["ownerid"] = new EntityReference("systemuser", Guid.NewGuid());
            incidentTarget["statuscode"] = new OptionSetValue(20);

            incidentImgTarget["jarvis_fumonitorskill"] = new EntityReference("jarvis_fumonitorskill", Guid.NewGuid());
            incidentImgTarget["jarvis_futimestamp"] = DateTime.Now;
            incidentImgTarget["jarvis_casestatusupdate"] = DateTime.Now;
            incidentImgTarget["statuscode"] = new OptionSetValue(90);
            incidentImgTarget["incidentid"] = Guid.NewGuid();
            incidentImgTarget["statecode"] = new OptionSetValue(2);
            incidentImgTarget["caseorigincode"] = new OptionSetValue(2);
            incidentImgTarget["casetypecode"] = new OptionSetValue(2);
            incidentImgTarget["jarvis_mercuriusstatus"] = new OptionSetValue(300);
            incidentImgTarget["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
            incidentImgTarget["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incidentImgTarget["customerid"] = new EntityReference("account", Guid.NewGuid());
            incidentImgTarget["jarvis_restgoplimitout"] = 100.0M;
            incidentImgTarget["jarvis_totalrestcurrencyout"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentImgTarget["jarvis_totalgoplimitout"] = 100.0M;
            incidentImgTarget["jarvis_totalcurrencyout"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentImgTarget["jarvis_totalgoplimitoutapproved"] = 100.0M;
            incidentImgTarget["jarvis_totalgoplimitoutapprovedcurrency"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentImgTarget["jarvis_totalrequestedccamount"] = 100.0M;
            incidentImgTarget["jarvis_totalcreditcardrequestedamountcurreny"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentImgTarget["jarvis_totalgoplimitin"] = 100.0M;
            incidentImgTarget["jarvis_totalcurrencyin"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentImgTarget["jarvis_registrationnumber"] = "123";
            incidentImgTarget["jarvis_country"] = new EntityReference("jarvis_country", sameGuid);
            incidentImgTarget["description"] = "This is for testing";
            incidentImgTarget["jarvis_location"] = new EntityReference("jarvis_country", sameGuid);
            incidentImgTarget["jarvis_customerexpectations"] = "test";
            incidentImgTarget["jarvis_incidentnature"] = "[\r\n  {\r\n    \"id\": \"3efba61a-85ad-44b7-a187-7f54acaf7a67\",\r\n    \"name\": \"Accident\"\r\n  },\r\n  {\r\n    \"_id\": \"3efba61a-85ad-44b7-a187-7f54acaf7a68\",\r\n    \"_name\": \"Air Problem\"\r\n  }\r\n]";
            incidentImgTarget["jarvis_callerrole"] = new OptionSetValue(4);
            incidentImgTarget["jarvis_callerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incidentImgTarget["jarvis_driverlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incidentImgTarget["isescalated"] = false;
            incidentImgTarget["createdon"] = DateTime.UtcNow;
            incidentImgTarget["jarvis_assistancetype"] = new OptionSetValue(334030000);
            incidentImgTarget.Attributes["modifiedby"] = users;
            incidentImgTarget.Attributes["createdby"] = users;
            incidentImgTarget["parentcaseid"] = new EntityReference("incident", Guid.NewGuid());
            incidentImgTarget["jarvis_hdrd"] = true;

            //// Setting Input Parameters.
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", incidentTarget),
               };
            this.PluginExecutionContext.PreEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PreImage", incidentImgTarget },
                };
            };
            this.PluginExecutionContext.MessageNameGet = () => "UPDATE";
            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();
                if (query.GetType().Name == "FetchExpression")
                {
                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_exchangerate'>"))
                    {
                        Entity exchangeRateResult = new Entity("jarvis_exchangerate");
                        exchangeRateResult["jarvis_value"] = 1M;
                        exchangeRateResult.Id = Guid.NewGuid();
                        result.Entities.Add(exchangeRateResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_gop'>"))
                    {
                        Entity gop = new Entity("jarvis_gop");
                        gop["modifiedon"] = DateTime.Now;
                        gop.Id = Guid.NewGuid();
                        result.Entities.Add(gop);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_configurationjarvis'>"))
                    {
                        Entity configResult = new Entity("jarvis_configurationjarvis");
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationcasestatuschange"] = true;
                        configResult["jarvis_automationreleasecase"] = new OptionSetValue(1);
                        configResult["jarvis_knowledgesearchforbd"] = "\"breakdown\" AND [incident.jarvis_incidentnatureshadow] AND ([incident.account.HD_country] OR [incident.jarvis_country])";
                        configResult["jarvis_incidentnatureconjunctionbd"] = "AND";
                        configResult["createdon"] = DateTime.UtcNow;
                        configResult.Id = Guid.NewGuid();
                        result.Entities.Add(configResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_region'>"))
                    {
                        Entity regionResult = new Entity("jarvis_region");
                        regionResult["jarvis_name"] = "testpassout";
                        regionResult.Id = Guid.NewGuid();
                        result.Entities.Add(regionResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_vehiclefuelpowertype'>"))
                    {
                        Entity fuelPowerType = new Entity("jarvis_vehiclefuelpowertype");
                        fuelPowerType["jarvis_name"] = "testpassout";
                        fuelPowerType[Constants.IncidentNature.IncidentNatureVehicleFuelPid_jarvisincidentnatureid] = new AliasedValue("incidentNature", "incidentnatureid", Guid.NewGuid());
                        fuelPowerType["IncidentNature_VehicleFuelPid.jarvis_name"] = new AliasedValue("incidentNature", "incidentnatureid", "Air Problem2");
                        fuelPowerType["jarvis_vehiclefuelpowertypeid"] = Guid.NewGuid();
                        fuelPowerType.Id = Guid.NewGuid();
                        result.Entities.Add(fuelPowerType);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='timezonedefinition'>"))
                    {
                        Entity timeZoneDef = new Entity("timezonedefinition");
                        timeZoneDef["userinterfacename"] = "GMT+1TestingpurposeTestMoreTest";
                        timeZoneDef.Id = Guid.NewGuid();
                        result.Entities.Add(timeZoneDef);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='systemuser'>"))
                    {
                        Entity userRef = new Entity("systemuser");
                        userRef["fullname"] = "MERCURIUS";
                        userRef.Id = Guid.NewGuid();
                        result.Entities.Add(userRef);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='queueitem'>"))
                    {
                        Entity queue = new Entity("queueitem");
                        queue["fullname"] = "MERCURIUS";
                        queue.Id = Guid.NewGuid();
                        result.Entities.Add(queue);
                    }
                }

                return result;
            };

            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {

                if (entityName == "systemuser")
                {
                    Entity user = new Entity("systemuser");
                    user.Id = Guid.NewGuid();
                    user.Attributes["fullname"] = "TEST";
                    return user;
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
                    country.Id = sameGuid;
                    country["jarvis_iso2countrycode"] = "NL";
                    country.Attributes[JarvisCountry.JarvisAverageetaduration] = "12";
                    country.Attributes["transactioncurrencyid"] = new EntityReference("transactioncurrency", Guid.NewGuid());
                    country.Attributes["jarvis_timezone"] = 106;
                    return country;
                }

                if (entityName == "jarvis_language")
                {
                    Entity language = new Entity("jarvis_language");
                    language.Id = guid;
                    language["jarvis_iso3languagecode6392t"] = "DEU";
                    language["jarvis_iso2languagecode6391"] = "DE";
                    language["jarvis_vasstandardlanguage"] = false;
                    return language;
                }

                if (entityName == "account")
                {
                    Entity account = new Entity("account");
                    account.Id = guid;
                    account["jarvis_language"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    account["jarvis_address1_country"] = new EntityReference("jarvis_country", sameGuid);
                    account["jarvis_blacklisted"] = false;
                    account["jarvis_onecasestatus"] = new OptionSetValue(334030000);
                    account["jarvis_accounttype"] = new OptionSetValue(334030001);
                    account["jarvis_responsableunitid"] = "NotDummy";
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

            CasePreOperationSync plugin = new CasePreOperationSync(null, null);
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// PreValidate Update With Complete Data Test Method Create case type code 3.
        /// </summary>
        [TestMethod]
        public void BreakdownCasePreOperationUpdateTypeCode000()
        {
            Entity incidentTarget = new Entity("incident");
            Entity incidentImgTarget = new Entity("incident");
            EntityReference users = new EntityReference("systemuser", Guid.NewGuid());
            users.Name = "TEST";

            incidentTarget.Id = Guid.Empty;
            Guid sameGuid = Guid.NewGuid();
            incidentTarget["incidentid"] = Guid.NewGuid();
            incidentTarget["caseorigincode"] = new OptionSetValue(2);
            incidentTarget["casetypecode"] = new OptionSetValue(2);
            incidentTarget["jarvis_mercuriusstatus"] = new OptionSetValue(0);
            incidentTarget["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
            incidentTarget["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["customerid"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["jarvis_restgoplimitout"] = 100.0M;
            incidentTarget["jarvis_totalrestcurrencyout"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentTarget["jarvis_totalgoplimitout"] = 100.0M;
            incidentTarget["jarvis_totalcurrencyout"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentTarget["jarvis_totalgoplimitoutapproved"] = 100.0M;
            incidentTarget["jarvis_totalgoplimitoutapprovedcurrency"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentTarget["jarvis_totalrequestedccamount"] = 100.0M;
            incidentTarget["jarvis_totalcreditcardrequestedamountcurreny"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentTarget["jarvis_totalgoplimitin"] = 100.0M;
            incidentTarget["jarvis_totalcurrencyin"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentTarget["jarvis_registrationnumber"] = "123";
            incidentTarget["jarvis_country"] = new EntityReference("jarvis_country", sameGuid);
            incidentTarget["description"] = "This is for testing";
            incidentTarget["jarvis_location"] = new EntityReference("jarvis_country", sameGuid);
            incidentTarget["jarvis_customerexpectations"] = "test";
            incidentTarget["jarvis_incidentnature"] = "[\r\n  {\r\n    \"id\": \"3efba61a-85ad-44b7-a187-7f54acaf7a67\",\r\n    \"name\": \"Accident\"\r\n  },\r\n  {\r\n    \"_id\": \"3efba61a-85ad-44b7-a187-7f54acaf7a68\",\r\n    \"_name\": \"Air Problem\"\r\n  }\r\n]";
            incidentTarget["jarvis_callerrole"] = new OptionSetValue(4);
            incidentTarget["jarvis_callerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incidentTarget["jarvis_driverlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incidentTarget["isescalated"] = false;
            incidentTarget["createdon"] = DateTime.UtcNow;
            incidentTarget["jarvis_assistancetype"] = new OptionSetValue(334030000);
            incidentTarget.Attributes["modifiedby"] = users;
            incidentTarget.Attributes["createdby"] = users;
            incidentTarget["parentcaseid"] = new EntityReference("incident", Guid.NewGuid());
            incidentTarget["ownerid"] = new EntityReference("systemuser", Guid.NewGuid());
            incidentTarget["statuscode"] = new OptionSetValue(20);

            incidentImgTarget["jarvis_fumonitorskill"] = new EntityReference("jarvis_fumonitorskill", Guid.NewGuid());
            incidentImgTarget["jarvis_futimestamp"] = DateTime.Now;
            incidentImgTarget["jarvis_casestatusupdate"] = DateTime.Now;
            incidentImgTarget["statuscode"] = new OptionSetValue(90);
            incidentImgTarget["incidentid"] = Guid.NewGuid();
            incidentImgTarget["statecode"] = new OptionSetValue(2);
            incidentImgTarget["caseorigincode"] = new OptionSetValue(2);
            incidentImgTarget["casetypecode"] = new OptionSetValue(2);
            incidentImgTarget["jarvis_mercuriusstatus"] = new OptionSetValue(0);
            incidentImgTarget["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
            incidentImgTarget["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incidentImgTarget["customerid"] = new EntityReference("account", Guid.NewGuid());
            incidentImgTarget["jarvis_restgoplimitout"] = 100.0M;
            incidentImgTarget["jarvis_totalrestcurrencyout"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentImgTarget["jarvis_totalgoplimitout"] = 100.0M;
            incidentImgTarget["jarvis_totalcurrencyout"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentImgTarget["jarvis_totalgoplimitoutapproved"] = 100.0M;
            incidentImgTarget["jarvis_totalgoplimitoutapprovedcurrency"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentImgTarget["jarvis_totalrequestedccamount"] = 100.0M;
            incidentImgTarget["jarvis_totalcreditcardrequestedamountcurreny"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentImgTarget["jarvis_totalgoplimitin"] = 100.0M;
            incidentImgTarget["jarvis_totalcurrencyin"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentImgTarget["jarvis_registrationnumber"] = "123";
            incidentImgTarget["jarvis_country"] = new EntityReference("jarvis_country", sameGuid);
            incidentImgTarget["description"] = "This is for testing";
            incidentImgTarget["jarvis_location"] = new EntityReference("jarvis_country", sameGuid);
            incidentImgTarget["jarvis_customerexpectations"] = "test";
            incidentImgTarget["jarvis_incidentnature"] = "[\r\n  {\r\n    \"id\": \"3efba61a-85ad-44b7-a187-7f54acaf7a67\",\r\n    \"name\": \"Accident\"\r\n  },\r\n  {\r\n    \"_id\": \"3efba61a-85ad-44b7-a187-7f54acaf7a68\",\r\n    \"_name\": \"Air Problem\"\r\n  }\r\n]";
            incidentImgTarget["jarvis_callerrole"] = new OptionSetValue(4);
            incidentImgTarget["jarvis_callerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incidentImgTarget["jarvis_driverlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incidentImgTarget["isescalated"] = false;
            incidentImgTarget["createdon"] = DateTime.UtcNow;
            incidentImgTarget["jarvis_assistancetype"] = new OptionSetValue(334030000);
            incidentImgTarget.Attributes["modifiedby"] = users;
            incidentImgTarget.Attributes["createdby"] = users;
            incidentImgTarget["parentcaseid"] = new EntityReference("incident", Guid.NewGuid());
            incidentImgTarget["jarvis_hdrd"] = true;

            //// Setting Input Parameters.
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", incidentTarget),
               };
            this.PluginExecutionContext.PreEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PreImage", incidentImgTarget },
                };
            };
            this.PluginExecutionContext.MessageNameGet = () => "UPDATE";
            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();
                if (query.GetType().Name == "FetchExpression")
                {
                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_exchangerate'>"))
                    {
                        Entity exchangeRateResult = new Entity("jarvis_exchangerate");
                        exchangeRateResult["jarvis_value"] = 1M;
                        exchangeRateResult.Id = Guid.NewGuid();
                        result.Entities.Add(exchangeRateResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_gop'>"))
                    {
                        Entity gop = new Entity("jarvis_gop");
                        gop["modifiedon"] = DateTime.Now;
                        gop.Id = Guid.NewGuid();
                        result.Entities.Add(gop);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_configurationjarvis'>"))
                    {
                        Entity configResult = new Entity("jarvis_configurationjarvis");
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationcasestatuschange"] = true;
                        configResult["jarvis_automationreleasecase"] = new OptionSetValue(1);
                        configResult["jarvis_knowledgesearchforbd"] = "\"breakdown\" AND [incident.jarvis_incidentnatureshadow] AND ([incident.account.HD_country] OR [incident.jarvis_country])";
                        configResult["jarvis_incidentnatureconjunctionbd"] = "AND";
                        configResult["createdon"] = DateTime.UtcNow;
                        configResult.Id = Guid.NewGuid();
                        result.Entities.Add(configResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_region'>"))
                    {
                        Entity regionResult = new Entity("jarvis_region");
                        regionResult["jarvis_name"] = "testpassout";
                        regionResult.Id = Guid.NewGuid();
                        result.Entities.Add(regionResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_vehiclefuelpowertype'>"))
                    {
                        Entity fuelPowerType = new Entity("jarvis_vehiclefuelpowertype");
                        fuelPowerType["jarvis_name"] = "testpassout";
                        fuelPowerType[Constants.IncidentNature.IncidentNatureVehicleFuelPid_jarvisincidentnatureid] = new AliasedValue("incidentNature", "incidentnatureid", Guid.NewGuid());
                        fuelPowerType["IncidentNature_VehicleFuelPid.jarvis_name"] = new AliasedValue("incidentNature", "incidentnatureid", "Air Problem2");
                        fuelPowerType["jarvis_vehiclefuelpowertypeid"] = Guid.NewGuid();
                        fuelPowerType.Id = Guid.NewGuid();
                        result.Entities.Add(fuelPowerType);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='timezonedefinition'>"))
                    {
                        Entity timeZoneDef = new Entity("timezonedefinition");
                        timeZoneDef["userinterfacename"] = "GMT+1TestingpurposeTestMoreTest";
                        timeZoneDef.Id = Guid.NewGuid();
                        result.Entities.Add(timeZoneDef);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='systemuser'>"))
                    {
                        Entity userRef = new Entity("systemuser");
                        userRef["fullname"] = "MERCURIUS";
                        userRef.Id = Guid.NewGuid();
                        result.Entities.Add(userRef);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='queueitem'>"))
                    {
                        Entity queue = new Entity("queueitem");
                        queue["fullname"] = "MERCURIUS";
                        queue.Id = Guid.NewGuid();
                        result.Entities.Add(queue);
                    }
                }

                return result;
            };

            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {

                if (entityName == "systemuser")
                {
                    Entity user = new Entity("systemuser");
                    user.Id = Guid.NewGuid();
                    user.Attributes["fullname"] = "TEST";
                    return user;
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
                    country.Id = sameGuid;
                    country["jarvis_iso2countrycode"] = "NL";
                    country.Attributes[JarvisCountry.JarvisAverageetaduration] = "12";
                    country.Attributes["transactioncurrencyid"] = new EntityReference("transactioncurrency", Guid.NewGuid());
                    country.Attributes["jarvis_timezone"] = 106;
                    return country;
                }

                if (entityName == "jarvis_language")
                {
                    Entity language = new Entity("jarvis_language");
                    language.Id = guid;
                    language["jarvis_iso3languagecode6392t"] = "DEU";
                    language["jarvis_iso2languagecode6391"] = "DE";
                    language["jarvis_vasstandardlanguage"] = false;
                    return language;
                }

                if (entityName == "account")
                {
                    Entity account = new Entity("account");
                    account.Id = guid;
                    account["jarvis_language"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    account["jarvis_address1_country"] = new EntityReference("jarvis_country", sameGuid);
                    account["jarvis_blacklisted"] = false;
                    account["jarvis_onecasestatus"] = new OptionSetValue(334030000);
                    account["jarvis_accounttype"] = new OptionSetValue(334030001);
                    account["jarvis_responsableunitid"] = "NotDummy";
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

            CasePreOperationSync plugin = new CasePreOperationSync(null, null);
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// PreValidate Update With Complete Data Test Method Create case type code 3.
        /// </summary>
        [TestMethod]
        public void BreakdownCasePreOperationUpdateTypeCode100()
        {
            Entity incidentTarget = new Entity("incident");
            Entity incidentImgTarget = new Entity("incident");
            EntityReference users = new EntityReference("systemuser", Guid.NewGuid());
            users.Name = "TEST";

            incidentTarget.Id = Guid.Empty;
            Guid sameGuid = Guid.NewGuid();
            incidentTarget["incidentid"] = Guid.NewGuid();
            incidentTarget["caseorigincode"] = new OptionSetValue(2);
            incidentTarget["casetypecode"] = new OptionSetValue(2);
            incidentTarget["jarvis_mercuriusstatus"] = new OptionSetValue(100);
            incidentTarget["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
            incidentTarget["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["customerid"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["jarvis_restgoplimitout"] = 100.0M;
            incidentTarget["jarvis_totalrestcurrencyout"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentTarget["jarvis_totalgoplimitout"] = 100.0M;
            incidentTarget["jarvis_totalcurrencyout"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentTarget["jarvis_totalgoplimitoutapproved"] = 100.0M;
            incidentTarget["jarvis_totalgoplimitoutapprovedcurrency"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentTarget["jarvis_totalrequestedccamount"] = 100.0M;
            incidentTarget["jarvis_totalcreditcardrequestedamountcurreny"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentTarget["jarvis_totalgoplimitin"] = 100.0M;
            incidentTarget["jarvis_totalcurrencyin"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentTarget["jarvis_registrationnumber"] = "123";
            incidentTarget["jarvis_country"] = new EntityReference("jarvis_country", sameGuid);
            incidentTarget["description"] = "This is for testing";
            incidentTarget["jarvis_location"] = new EntityReference("jarvis_country", sameGuid);
            incidentTarget["jarvis_customerexpectations"] = "test";
            incidentTarget["jarvis_incidentnature"] = "[\r\n  {\r\n    \"id\": \"3efba61a-85ad-44b7-a187-7f54acaf7a67\",\r\n    \"name\": \"Accident\"\r\n  },\r\n  {\r\n    \"_id\": \"3efba61a-85ad-44b7-a187-7f54acaf7a68\",\r\n    \"_name\": \"Air Problem\"\r\n  }\r\n]";
            incidentTarget["jarvis_callerrole"] = new OptionSetValue(4);
            incidentTarget["jarvis_callerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incidentTarget["jarvis_driverlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incidentTarget["isescalated"] = false;
            incidentTarget["createdon"] = DateTime.UtcNow;
            incidentTarget["jarvis_assistancetype"] = new OptionSetValue(334030000);
            incidentTarget.Attributes["modifiedby"] = users;
            incidentTarget.Attributes["createdby"] = users;
            incidentTarget["parentcaseid"] = new EntityReference("incident", Guid.NewGuid());
            incidentTarget["ownerid"] = new EntityReference("systemuser", Guid.NewGuid());
            incidentTarget["statuscode"] = new OptionSetValue(20);

            incidentImgTarget["jarvis_fumonitorskill"] = new EntityReference("jarvis_fumonitorskill", Guid.NewGuid());
            incidentImgTarget["jarvis_futimestamp"] = DateTime.Now;
            incidentImgTarget["jarvis_casestatusupdate"] = DateTime.Now;
            incidentImgTarget["statuscode"] = new OptionSetValue(90);
            incidentImgTarget["incidentid"] = Guid.NewGuid();
            incidentImgTarget["statecode"] = new OptionSetValue(2);
            incidentImgTarget["caseorigincode"] = new OptionSetValue(2);
            incidentImgTarget["casetypecode"] = new OptionSetValue(2);
            incidentImgTarget["jarvis_mercuriusstatus"] = new OptionSetValue(100);
            incidentImgTarget["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
            incidentImgTarget["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incidentImgTarget["customerid"] = new EntityReference("account", Guid.NewGuid());
            incidentImgTarget["jarvis_restgoplimitout"] = 100.0M;
            incidentImgTarget["jarvis_totalrestcurrencyout"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentImgTarget["jarvis_totalgoplimitout"] = 100.0M;
            incidentImgTarget["jarvis_totalcurrencyout"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentImgTarget["jarvis_totalgoplimitoutapproved"] = 100.0M;
            incidentImgTarget["jarvis_totalgoplimitoutapprovedcurrency"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentImgTarget["jarvis_totalrequestedccamount"] = 100.0M;
            incidentImgTarget["jarvis_totalcreditcardrequestedamountcurreny"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentImgTarget["jarvis_totalgoplimitin"] = 100.0M;
            incidentImgTarget["jarvis_totalcurrencyin"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentImgTarget["jarvis_registrationnumber"] = "123";
            incidentImgTarget["jarvis_country"] = new EntityReference("jarvis_country", sameGuid);
            incidentImgTarget["description"] = "This is for testing";
            incidentImgTarget["jarvis_location"] = new EntityReference("jarvis_country", sameGuid);
            incidentImgTarget["jarvis_customerexpectations"] = "test";
            incidentImgTarget["jarvis_incidentnature"] = "[\r\n  {\r\n    \"id\": \"3efba61a-85ad-44b7-a187-7f54acaf7a67\",\r\n    \"name\": \"Accident\"\r\n  },\r\n  {\r\n    \"_id\": \"3efba61a-85ad-44b7-a187-7f54acaf7a68\",\r\n    \"_name\": \"Air Problem\"\r\n  }\r\n]";
            incidentImgTarget["jarvis_callerrole"] = new OptionSetValue(4);
            incidentImgTarget["jarvis_callerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incidentImgTarget["jarvis_driverlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incidentImgTarget["isescalated"] = false;
            incidentImgTarget["createdon"] = DateTime.UtcNow;
            incidentImgTarget["jarvis_assistancetype"] = new OptionSetValue(334030000);
            incidentImgTarget.Attributes["modifiedby"] = users;
            incidentImgTarget.Attributes["createdby"] = users;
            incidentImgTarget["parentcaseid"] = new EntityReference("incident", Guid.NewGuid());
            incidentImgTarget["jarvis_hdrd"] = true;

            //// Setting Input Parameters.
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", incidentTarget),
               };
            this.PluginExecutionContext.PreEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PreImage", incidentImgTarget },
                };
            };
            this.PluginExecutionContext.MessageNameGet = () => "UPDATE";
            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();
                if (query.GetType().Name == "FetchExpression")
                {
                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_exchangerate'>"))
                    {
                        Entity exchangeRateResult = new Entity("jarvis_exchangerate");
                        exchangeRateResult["jarvis_value"] = 1M;
                        exchangeRateResult.Id = Guid.NewGuid();
                        result.Entities.Add(exchangeRateResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_gop'>"))
                    {
                        Entity gop = new Entity("jarvis_gop");
                        gop["modifiedon"] = DateTime.Now;
                        gop.Id = Guid.NewGuid();
                        result.Entities.Add(gop);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_configurationjarvis'>"))
                    {
                        Entity configResult = new Entity("jarvis_configurationjarvis");
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationcasestatuschange"] = true;
                        configResult["jarvis_automationreleasecase"] = new OptionSetValue(1);
                        configResult["jarvis_knowledgesearchforbd"] = "\"breakdown\" AND [incident.jarvis_incidentnatureshadow] AND ([incident.account.HD_country] OR [incident.jarvis_country])";
                        configResult["jarvis_incidentnatureconjunctionbd"] = "AND";
                        configResult["createdon"] = DateTime.UtcNow;
                        configResult.Id = Guid.NewGuid();
                        result.Entities.Add(configResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_region'>"))
                    {
                        Entity regionResult = new Entity("jarvis_region");
                        regionResult["jarvis_name"] = "testpassout";
                        regionResult.Id = Guid.NewGuid();
                        result.Entities.Add(regionResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_vehiclefuelpowertype'>"))
                    {
                        Entity fuelPowerType = new Entity("jarvis_vehiclefuelpowertype");
                        fuelPowerType["jarvis_name"] = "testpassout";
                        fuelPowerType[Constants.IncidentNature.IncidentNatureVehicleFuelPid_jarvisincidentnatureid] = new AliasedValue("incidentNature", "incidentnatureid", Guid.NewGuid());
                        fuelPowerType["IncidentNature_VehicleFuelPid.jarvis_name"] = new AliasedValue("incidentNature", "incidentnatureid", "Air Problem2");
                        fuelPowerType["jarvis_vehiclefuelpowertypeid"] = Guid.NewGuid();
                        fuelPowerType.Id = Guid.NewGuid();
                        result.Entities.Add(fuelPowerType);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='timezonedefinition'>"))
                    {
                        Entity timeZoneDef = new Entity("timezonedefinition");
                        timeZoneDef["userinterfacename"] = "GMT+1TestingpurposeTestMoreTest";
                        timeZoneDef.Id = Guid.NewGuid();
                        result.Entities.Add(timeZoneDef);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='systemuser'>"))
                    {
                        Entity userRef = new Entity("systemuser");
                        userRef["fullname"] = "MERCURIUS";
                        userRef.Id = Guid.NewGuid();
                        result.Entities.Add(userRef);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='queueitem'>"))
                    {
                        Entity queue = new Entity("queueitem");
                        queue["fullname"] = "MERCURIUS";
                        queue.Id = Guid.NewGuid();
                        result.Entities.Add(queue);
                    }
                }

                return result;
            };
            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {

                if (entityName == "systemuser")
                {
                    Entity user = new Entity("systemuser");
                    user.Id = Guid.NewGuid();
                    user.Attributes["fullname"] = "TEST";
                    return user;
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
                    country.Id = sameGuid;
                    country["jarvis_iso2countrycode"] = "NL";
                    country.Attributes[JarvisCountry.JarvisAverageetaduration] = "12";
                    country.Attributes["transactioncurrencyid"] = new EntityReference("transactioncurrency", Guid.NewGuid());
                    country.Attributes["jarvis_timezone"] = 106;
                    return country;
                }

                if (entityName == "jarvis_language")
                {
                    Entity language = new Entity("jarvis_language");
                    language.Id = guid;
                    language["jarvis_iso3languagecode6392t"] = "DEU";
                    language["jarvis_iso2languagecode6391"] = "DE";
                    language["jarvis_vasstandardlanguage"] = false;
                    return language;
                }

                if (entityName == "account")
                {
                    Entity account = new Entity("account");
                    account.Id = guid;
                    account["jarvis_language"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    account["jarvis_address1_country"] = new EntityReference("jarvis_country", sameGuid);
                    account["jarvis_blacklisted"] = false;
                    account["jarvis_onecasestatus"] = new OptionSetValue(334030000);
                    account["jarvis_accounttype"] = new OptionSetValue(334030001);
                    account["jarvis_responsableunitid"] = "NotDummy";
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

            CasePreOperationSync plugin = new CasePreOperationSync(null, null);
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// PreValidate Update With Complete Data Test Method Create case type code 3.
        /// </summary>
        [TestMethod]
        public void BreakdownCasePreOperationUpdateTypeCode400()
        {
            Entity incidentTarget = new Entity("incident");
            Entity incidentImgTarget = new Entity("incident");
            EntityReference users = new EntityReference("systemuser", Guid.NewGuid());
            users.Name = "TEST";

            incidentTarget.Id = Guid.Empty;
            Guid sameGuid = Guid.NewGuid();
            incidentTarget["incidentid"] = Guid.NewGuid();
            incidentTarget["caseorigincode"] = new OptionSetValue(2);
            incidentTarget["casetypecode"] = new OptionSetValue(2);
            incidentTarget["jarvis_mercuriusstatus"] = new OptionSetValue(400);
            incidentTarget["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
            incidentTarget["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["customerid"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["jarvis_restgoplimitout"] = 100.0M;
            incidentTarget["jarvis_totalrestcurrencyout"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentTarget["jarvis_totalgoplimitout"] = 100.0M;
            incidentTarget["jarvis_totalcurrencyout"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentTarget["jarvis_totalgoplimitoutapproved"] = 100.0M;
            incidentTarget["jarvis_totalgoplimitoutapprovedcurrency"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentTarget["jarvis_totalrequestedccamount"] = 100.0M;
            incidentTarget["jarvis_totalcreditcardrequestedamountcurreny"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentTarget["jarvis_totalgoplimitin"] = 100.0M;
            incidentTarget["jarvis_totalcurrencyin"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentTarget["jarvis_registrationnumber"] = "123";
            incidentTarget["jarvis_country"] = new EntityReference("jarvis_country", sameGuid);
            incidentTarget["description"] = "This is for testing";
            incidentTarget["jarvis_location"] = new EntityReference("jarvis_country", sameGuid);
            incidentTarget["jarvis_customerexpectations"] = "test";
            incidentTarget["jarvis_incidentnature"] = "[\r\n  {\r\n    \"id\": \"3efba61a-85ad-44b7-a187-7f54acaf7a67\",\r\n    \"name\": \"Accident\"\r\n  },\r\n  {\r\n    \"_id\": \"3efba61a-85ad-44b7-a187-7f54acaf7a68\",\r\n    \"_name\": \"Air Problem\"\r\n  }\r\n]";
            incidentTarget["jarvis_callerrole"] = new OptionSetValue(4);
            incidentTarget["jarvis_callerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incidentTarget["jarvis_driverlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incidentTarget["isescalated"] = false;
            incidentTarget["createdon"] = DateTime.UtcNow;
            incidentTarget["jarvis_assistancetype"] = new OptionSetValue(334030000);
            incidentTarget.Attributes["modifiedby"] = users;
            incidentTarget.Attributes["createdby"] = users;
            incidentTarget["parentcaseid"] = new EntityReference("incident", Guid.NewGuid());
            incidentTarget["ownerid"] = new EntityReference("systemuser", Guid.NewGuid());
            incidentTarget["statuscode"] = new OptionSetValue(20);

            incidentImgTarget["jarvis_fumonitorskill"] = new EntityReference("jarvis_fumonitorskill", Guid.NewGuid());
            incidentImgTarget["jarvis_futimestamp"] = DateTime.Now;
            incidentImgTarget["jarvis_casestatusupdate"] = DateTime.Now;
            incidentImgTarget["statuscode"] = new OptionSetValue(90);
            incidentImgTarget["incidentid"] = Guid.NewGuid();
            incidentImgTarget["statecode"] = new OptionSetValue(2);
            incidentImgTarget["caseorigincode"] = new OptionSetValue(2);
            incidentImgTarget["casetypecode"] = new OptionSetValue(2);
            incidentImgTarget["jarvis_mercuriusstatus"] = new OptionSetValue(400);
            incidentImgTarget["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
            incidentImgTarget["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incidentImgTarget["customerid"] = new EntityReference("account", Guid.NewGuid());
            incidentImgTarget["jarvis_restgoplimitout"] = 100.0M;
            incidentImgTarget["jarvis_totalrestcurrencyout"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentImgTarget["jarvis_totalgoplimitout"] = 100.0M;
            incidentImgTarget["jarvis_totalcurrencyout"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentImgTarget["jarvis_totalgoplimitoutapproved"] = 100.0M;
            incidentImgTarget["jarvis_totalgoplimitoutapprovedcurrency"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentImgTarget["jarvis_totalrequestedccamount"] = 100.0M;
            incidentImgTarget["jarvis_totalcreditcardrequestedamountcurreny"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentImgTarget["jarvis_totalgoplimitin"] = 100.0M;
            incidentImgTarget["jarvis_totalcurrencyin"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentImgTarget["jarvis_registrationnumber"] = "123";
            incidentImgTarget["jarvis_country"] = new EntityReference("jarvis_country", sameGuid);
            incidentImgTarget["description"] = "This is for testing";
            incidentImgTarget["jarvis_location"] = new EntityReference("jarvis_country", sameGuid);
            incidentImgTarget["jarvis_customerexpectations"] = "test";
            incidentImgTarget["jarvis_incidentnature"] = "[\r\n  {\r\n    \"id\": \"3efba61a-85ad-44b7-a187-7f54acaf7a67\",\r\n    \"name\": \"Accident\"\r\n  },\r\n  {\r\n    \"_id\": \"3efba61a-85ad-44b7-a187-7f54acaf7a68\",\r\n    \"_name\": \"Air Problem\"\r\n  }\r\n]";
            incidentImgTarget["jarvis_callerrole"] = new OptionSetValue(4);
            incidentImgTarget["jarvis_callerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incidentImgTarget["jarvis_driverlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incidentImgTarget["isescalated"] = false;
            incidentImgTarget["createdon"] = DateTime.UtcNow;
            incidentImgTarget["jarvis_assistancetype"] = new OptionSetValue(334030000);
            incidentImgTarget.Attributes["modifiedby"] = users;
            incidentImgTarget.Attributes["createdby"] = users;
            incidentImgTarget["parentcaseid"] = new EntityReference("incident", Guid.NewGuid());
            incidentImgTarget["jarvis_hdrd"] = true;

            //// Setting Input Parameters.
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", incidentTarget),
               };
            this.PluginExecutionContext.PreEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PreImage", incidentImgTarget },
                };
            };
            this.PluginExecutionContext.MessageNameGet = () => "UPDATE";
            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();
                if (query.GetType().Name == "FetchExpression")
                {
                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_exchangerate'>"))
                    {
                        Entity exchangeRateResult = new Entity("jarvis_exchangerate");
                        exchangeRateResult["jarvis_value"] = 1M;
                        exchangeRateResult.Id = Guid.NewGuid();
                        result.Entities.Add(exchangeRateResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_gop'>"))
                    {
                        Entity gop = new Entity("jarvis_gop");
                        gop["modifiedon"] = DateTime.Now;
                        gop.Id = Guid.NewGuid();
                        result.Entities.Add(gop);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_configurationjarvis'>"))
                    {
                        Entity configResult = new Entity("jarvis_configurationjarvis");
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationcasestatuschange"] = true;
                        configResult["jarvis_automationreleasecase"] = new OptionSetValue(1);
                        configResult["jarvis_knowledgesearchforbd"] = "\"breakdown\" AND [incident.jarvis_incidentnatureshadow] AND ([incident.account.HD_country] OR [incident.jarvis_country])";
                        configResult["jarvis_incidentnatureconjunctionbd"] = "AND";
                        configResult["createdon"] = DateTime.UtcNow;
                        configResult.Id = Guid.NewGuid();
                        result.Entities.Add(configResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_region'>"))
                    {
                        Entity regionResult = new Entity("jarvis_region");
                        regionResult["jarvis_name"] = "testpassout";
                        regionResult.Id = Guid.NewGuid();
                        result.Entities.Add(regionResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_vehiclefuelpowertype'>"))
                    {
                        Entity fuelPowerType = new Entity("jarvis_vehiclefuelpowertype");
                        fuelPowerType["jarvis_name"] = "testpassout";
                        fuelPowerType[Constants.IncidentNature.IncidentNatureVehicleFuelPid_jarvisincidentnatureid] = new AliasedValue("incidentNature", "incidentnatureid", Guid.NewGuid());
                        fuelPowerType["IncidentNature_VehicleFuelPid.jarvis_name"] = new AliasedValue("incidentNature", "incidentnatureid", "Air Problem2");
                        fuelPowerType["jarvis_vehiclefuelpowertypeid"] = Guid.NewGuid();
                        fuelPowerType.Id = Guid.NewGuid();
                        result.Entities.Add(fuelPowerType);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='timezonedefinition'>"))
                    {
                        Entity timeZoneDef = new Entity("timezonedefinition");
                        timeZoneDef["userinterfacename"] = "GMT+1TestingpurposeTestMoreTest";
                        timeZoneDef.Id = Guid.NewGuid();
                        result.Entities.Add(timeZoneDef);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='systemuser'>"))
                    {
                        Entity userRef = new Entity("systemuser");
                        userRef["fullname"] = "MERCURIUS";
                        userRef.Id = Guid.NewGuid();
                        result.Entities.Add(userRef);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='queueitem'>"))
                    {
                        Entity queue = new Entity("queueitem");
                        queue["fullname"] = "MERCURIUS";
                        queue.Id = Guid.NewGuid();
                        result.Entities.Add(queue);
                    }
                }

                return result;
            };

            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {

                if (entityName == "systemuser")
                {
                    Entity user = new Entity("systemuser");
                    user.Id = Guid.NewGuid();
                    user.Attributes["fullname"] = "TEST";
                    return user;
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
                    country.Id = sameGuid;
                    country["jarvis_iso2countrycode"] = "NL";
                    country.Attributes[JarvisCountry.JarvisAverageetaduration] = "12";
                    country.Attributes["transactioncurrencyid"] = new EntityReference("transactioncurrency", Guid.NewGuid());
                    country.Attributes["jarvis_timezone"] = 106;
                    return country;
                }

                if (entityName == "jarvis_language")
                {
                    Entity language = new Entity("jarvis_language");
                    language.Id = guid;
                    language["jarvis_iso3languagecode6392t"] = "DEU";
                    language["jarvis_iso2languagecode6391"] = "DE";
                    language["jarvis_vasstandardlanguage"] = false;
                    return language;
                }

                if (entityName == "account")
                {
                    Entity account = new Entity("account");
                    account.Id = guid;
                    account["jarvis_language"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    account["jarvis_address1_country"] = new EntityReference("jarvis_country", sameGuid);
                    account["jarvis_blacklisted"] = false;
                    account["jarvis_onecasestatus"] = new OptionSetValue(334030000);
                    account["jarvis_accounttype"] = new OptionSetValue(334030001);
                    account["jarvis_responsableunitid"] = "NotDummy";
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

            CasePreOperationSync plugin = new CasePreOperationSync(null, null);
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// PreValidate Update With Complete Data Test Method Create case type code 3.
        /// </summary>
        [TestMethod]
        public void BreakdownCasePreOperationUpdateTypeCode500()
        {
            Entity incidentTarget = new Entity("incident");
            Entity incidentImgTarget = new Entity("incident");
            EntityReference users = new EntityReference("systemuser", Guid.NewGuid());
            users.Name = "TEST";

            incidentTarget.Id = Guid.Empty;
            Guid sameGuid = Guid.NewGuid();
            incidentTarget["incidentid"] = Guid.NewGuid();
            incidentTarget["caseorigincode"] = new OptionSetValue(2);
            incidentTarget["casetypecode"] = new OptionSetValue(2);
            incidentTarget["jarvis_mercuriusstatus"] = new OptionSetValue(500);
            incidentTarget["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
            incidentTarget["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["customerid"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["jarvis_restgoplimitout"] = 100.0M;
            incidentTarget["jarvis_totalrestcurrencyout"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentTarget["jarvis_totalgoplimitout"] = 100.0M;
            incidentTarget["jarvis_totalcurrencyout"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentTarget["jarvis_totalgoplimitoutapproved"] = 100.0M;
            incidentTarget["jarvis_totalgoplimitoutapprovedcurrency"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentTarget["jarvis_totalrequestedccamount"] = 100.0M;
            incidentTarget["jarvis_totalcreditcardrequestedamountcurreny"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentTarget["jarvis_totalgoplimitin"] = 100.0M;
            incidentTarget["jarvis_totalcurrencyin"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentTarget["jarvis_registrationnumber"] = "123";
            incidentTarget["jarvis_country"] = new EntityReference("jarvis_country", sameGuid);
            incidentTarget["description"] = "This is for testing";
            incidentTarget["jarvis_location"] = new EntityReference("jarvis_country", sameGuid);
            incidentTarget["jarvis_customerexpectations"] = "test";
            incidentTarget["jarvis_incidentnature"] = "[\r\n  {\r\n    \"id\": \"3efba61a-85ad-44b7-a187-7f54acaf7a67\",\r\n    \"name\": \"Accident\"\r\n  },\r\n  {\r\n    \"_id\": \"3efba61a-85ad-44b7-a187-7f54acaf7a68\",\r\n    \"_name\": \"Air Problem\"\r\n  }\r\n]";
            incidentTarget["jarvis_callerrole"] = new OptionSetValue(4);
            incidentTarget["jarvis_callerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incidentTarget["jarvis_driverlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incidentTarget["isescalated"] = false;
            incidentTarget["createdon"] = DateTime.UtcNow;
            incidentTarget["jarvis_assistancetype"] = new OptionSetValue(334030000);
            incidentTarget.Attributes["modifiedby"] = users;
            incidentTarget.Attributes["createdby"] = users;
            incidentTarget["parentcaseid"] = new EntityReference("incident", Guid.NewGuid());
            incidentTarget["ownerid"] = new EntityReference("systemuser", Guid.NewGuid());
            incidentTarget["statuscode"] = new OptionSetValue(20);

            incidentImgTarget["jarvis_fumonitorskill"] = new EntityReference("jarvis_fumonitorskill", Guid.NewGuid());
            incidentImgTarget["jarvis_futimestamp"] = DateTime.Now;
            incidentImgTarget["jarvis_casestatusupdate"] = DateTime.Now;
            incidentImgTarget["statuscode"] = new OptionSetValue(90);
            incidentImgTarget["incidentid"] = Guid.NewGuid();
            incidentImgTarget["statecode"] = new OptionSetValue(2);
            incidentImgTarget["caseorigincode"] = new OptionSetValue(2);
            incidentImgTarget["casetypecode"] = new OptionSetValue(2);
            incidentImgTarget["jarvis_mercuriusstatus"] = new OptionSetValue(500);
            incidentImgTarget["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
            incidentImgTarget["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incidentImgTarget["customerid"] = new EntityReference("account", Guid.NewGuid());
            incidentImgTarget["jarvis_restgoplimitout"] = 100.0M;
            incidentImgTarget["jarvis_totalrestcurrencyout"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentImgTarget["jarvis_totalgoplimitout"] = 100.0M;
            incidentImgTarget["jarvis_totalcurrencyout"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentImgTarget["jarvis_totalgoplimitoutapproved"] = 100.0M;
            incidentImgTarget["jarvis_totalgoplimitoutapprovedcurrency"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentImgTarget["jarvis_totalrequestedccamount"] = 100.0M;
            incidentImgTarget["jarvis_totalcreditcardrequestedamountcurreny"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentImgTarget["jarvis_totalgoplimitin"] = 100.0M;
            incidentImgTarget["jarvis_totalcurrencyin"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentImgTarget["jarvis_registrationnumber"] = "123";
            incidentImgTarget["jarvis_country"] = new EntityReference("jarvis_country", sameGuid);
            incidentImgTarget["description"] = "This is for testing";
            incidentImgTarget["jarvis_location"] = new EntityReference("jarvis_country", sameGuid);
            incidentImgTarget["jarvis_customerexpectations"] = "test";
            incidentImgTarget["jarvis_incidentnature"] = "[\r\n  {\r\n    \"id\": \"3efba61a-85ad-44b7-a187-7f54acaf7a67\",\r\n    \"name\": \"Accident\"\r\n  },\r\n  {\r\n    \"_id\": \"3efba61a-85ad-44b7-a187-7f54acaf7a68\",\r\n    \"_name\": \"Air Problem\"\r\n  }\r\n]";
            incidentImgTarget["jarvis_callerrole"] = new OptionSetValue(4);
            incidentImgTarget["jarvis_callerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incidentImgTarget["jarvis_driverlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incidentImgTarget["isescalated"] = false;
            incidentImgTarget["createdon"] = DateTime.UtcNow;
            incidentImgTarget["jarvis_assistancetype"] = new OptionSetValue(334030000);
            incidentImgTarget.Attributes["modifiedby"] = users;
            incidentImgTarget.Attributes["createdby"] = users;
            incidentImgTarget["parentcaseid"] = new EntityReference("incident", Guid.NewGuid());
            incidentImgTarget["jarvis_hdrd"] = true;

            //// Setting Input Parameters.
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", incidentTarget),
               };
            this.PluginExecutionContext.PreEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PreImage", incidentImgTarget },
                };
            };
            this.PluginExecutionContext.MessageNameGet = () => "UPDATE";
            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();
                if (query.GetType().Name == "FetchExpression")
                {
                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_exchangerate'>"))
                    {
                        Entity exchangeRateResult = new Entity("jarvis_exchangerate");
                        exchangeRateResult["jarvis_value"] = 1M;
                        exchangeRateResult.Id = Guid.NewGuid();
                        result.Entities.Add(exchangeRateResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_gop'>"))
                    {
                        Entity gop = new Entity("jarvis_gop");
                        gop["modifiedon"] = DateTime.Now;
                        gop.Id = Guid.NewGuid();
                        result.Entities.Add(gop);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_configurationjarvis'>"))
                    {
                        Entity configResult = new Entity("jarvis_configurationjarvis");
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationcasestatuschange"] = true;
                        configResult["jarvis_automationreleasecase"] = new OptionSetValue(1);
                        configResult["jarvis_knowledgesearchforbd"] = "\"breakdown\" AND [incident.jarvis_incidentnatureshadow] AND ([incident.account.HD_country] OR [incident.jarvis_country])";
                        configResult["jarvis_incidentnatureconjunctionbd"] = "AND";
                        configResult["createdon"] = DateTime.UtcNow;
                        configResult.Id = Guid.NewGuid();
                        result.Entities.Add(configResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_region'>"))
                    {
                        Entity regionResult = new Entity("jarvis_region");
                        regionResult["jarvis_name"] = "testpassout";
                        regionResult.Id = Guid.NewGuid();
                        result.Entities.Add(regionResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_vehiclefuelpowertype'>"))
                    {
                        Entity fuelPowerType = new Entity("jarvis_vehiclefuelpowertype");
                        fuelPowerType["jarvis_name"] = "testpassout";
                        fuelPowerType[Constants.IncidentNature.IncidentNatureVehicleFuelPid_jarvisincidentnatureid] = new AliasedValue("incidentNature", "incidentnatureid", Guid.NewGuid());
                        fuelPowerType["IncidentNature_VehicleFuelPid.jarvis_name"] = new AliasedValue("incidentNature", "incidentnatureid", "Air Problem2");
                        fuelPowerType["jarvis_vehiclefuelpowertypeid"] = Guid.NewGuid();
                        fuelPowerType.Id = Guid.NewGuid();
                        result.Entities.Add(fuelPowerType);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='timezonedefinition'>"))
                    {
                        Entity timeZoneDef = new Entity("timezonedefinition");
                        timeZoneDef["userinterfacename"] = "GMT+1TestingpurposeTestMoreTest";
                        timeZoneDef.Id = Guid.NewGuid();
                        result.Entities.Add(timeZoneDef);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='systemuser'>"))
                    {
                        Entity userRef = new Entity("systemuser");
                        userRef["fullname"] = "MERCURIUS";
                        userRef.Id = Guid.NewGuid();
                        result.Entities.Add(userRef);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='queueitem'>"))
                    {
                        Entity queue = new Entity("queueitem");
                        queue["fullname"] = "MERCURIUS";
                        queue.Id = Guid.NewGuid();
                        result.Entities.Add(queue);
                    }
                }

                return result;
            };

            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {

                if (entityName == "systemuser")
                {
                    Entity user = new Entity("systemuser");
                    user.Id = Guid.NewGuid();
                    user.Attributes["fullname"] = "TEST";
                    return user;
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
                    country.Id = sameGuid;
                    country["jarvis_iso2countrycode"] = "NL";
                    country.Attributes[JarvisCountry.JarvisAverageetaduration] = "12";
                    country.Attributes["transactioncurrencyid"] = new EntityReference("transactioncurrency", Guid.NewGuid());
                    country.Attributes["jarvis_timezone"] = 106;
                    return country;
                }

                if (entityName == "jarvis_language")
                {
                    Entity language = new Entity("jarvis_language");
                    language.Id = guid;
                    language["jarvis_iso3languagecode6392t"] = "DEU";
                    language["jarvis_iso2languagecode6391"] = "DE";
                    language["jarvis_vasstandardlanguage"] = false;
                    return language;
                }

                if (entityName == "account")
                {
                    Entity account = new Entity("account");
                    account.Id = guid;
                    account["jarvis_language"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    account["jarvis_address1_country"] = new EntityReference("jarvis_country", sameGuid);
                    account["jarvis_blacklisted"] = false;
                    account["jarvis_onecasestatus"] = new OptionSetValue(334030000);
                    account["jarvis_accounttype"] = new OptionSetValue(334030001);
                    account["jarvis_responsableunitid"] = "NotDummy";
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

            CasePreOperationSync plugin = new CasePreOperationSync(null, null);
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// PreValidate Update With Complete Data Test Method Create case type code 3.
        /// </summary>
        [TestMethod]
        public void BreakdownCasePreOperationUpdateTypeCode600()
        {
            Entity incidentTarget = new Entity("incident");
            Entity incidentImgTarget = new Entity("incident");
            EntityReference users = new EntityReference("systemuser", Guid.NewGuid());
            users.Name = "TEST";

            incidentTarget.Id = Guid.Empty;
            Guid sameGuid = Guid.NewGuid();
            incidentTarget["incidentid"] = Guid.NewGuid();
            incidentTarget["caseorigincode"] = new OptionSetValue(2);
            incidentTarget["casetypecode"] = new OptionSetValue(2);
            incidentTarget["jarvis_mercuriusstatus"] = new OptionSetValue(600);
            incidentTarget["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
            incidentTarget["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["customerid"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["jarvis_restgoplimitout"] = 100.0M;
            incidentTarget["jarvis_totalrestcurrencyout"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentTarget["jarvis_totalgoplimitout"] = 100.0M;
            incidentTarget["jarvis_totalcurrencyout"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentTarget["jarvis_totalgoplimitoutapproved"] = 100.0M;
            incidentTarget["jarvis_totalgoplimitoutapprovedcurrency"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentTarget["jarvis_totalrequestedccamount"] = 100.0M;
            incidentTarget["jarvis_totalcreditcardrequestedamountcurreny"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentTarget["jarvis_totalgoplimitin"] = 100.0M;
            incidentTarget["jarvis_totalcurrencyin"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentTarget["jarvis_registrationnumber"] = "123";
            incidentTarget["jarvis_country"] = new EntityReference("jarvis_country", sameGuid);
            incidentTarget["description"] = "This is for testing";
            incidentTarget["jarvis_location"] = new EntityReference("jarvis_country", sameGuid);
            incidentTarget["jarvis_customerexpectations"] = "test";
            incidentTarget["jarvis_incidentnature"] = "[\r\n  {\r\n    \"id\": \"3efba61a-85ad-44b7-a187-7f54acaf7a67\",\r\n    \"name\": \"Accident\"\r\n  },\r\n  {\r\n    \"_id\": \"3efba61a-85ad-44b7-a187-7f54acaf7a68\",\r\n    \"_name\": \"Air Problem\"\r\n  }\r\n]";
            incidentTarget["jarvis_callerrole"] = new OptionSetValue(4);
            incidentTarget["jarvis_callerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incidentTarget["jarvis_driverlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incidentTarget["isescalated"] = false;
            incidentTarget["createdon"] = DateTime.UtcNow;
            incidentTarget["jarvis_assistancetype"] = new OptionSetValue(334030000);
            incidentTarget.Attributes["modifiedby"] = users;
            incidentTarget.Attributes["createdby"] = users;
            incidentTarget["parentcaseid"] = new EntityReference("incident", Guid.NewGuid());
            incidentTarget["ownerid"] = new EntityReference("systemuser", Guid.NewGuid());
            incidentTarget["statuscode"] = new OptionSetValue(20);

            incidentImgTarget["jarvis_fumonitorskill"] = new EntityReference("jarvis_fumonitorskill", Guid.NewGuid());
            incidentImgTarget["jarvis_futimestamp"] = DateTime.Now;
            incidentImgTarget["jarvis_casestatusupdate"] = DateTime.Now;
            incidentImgTarget["statuscode"] = new OptionSetValue(90);
            incidentImgTarget["incidentid"] = Guid.NewGuid();
            incidentImgTarget["statecode"] = new OptionSetValue(2);
            incidentImgTarget["caseorigincode"] = new OptionSetValue(2);
            incidentImgTarget["casetypecode"] = new OptionSetValue(2);
            incidentImgTarget["jarvis_mercuriusstatus"] = new OptionSetValue(600);
            incidentImgTarget["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
            incidentImgTarget["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incidentImgTarget["customerid"] = new EntityReference("account", Guid.NewGuid());
            incidentImgTarget["jarvis_restgoplimitout"] = 100.0M;
            incidentImgTarget["jarvis_totalrestcurrencyout"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentImgTarget["jarvis_totalgoplimitout"] = 100.0M;
            incidentImgTarget["jarvis_totalcurrencyout"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentImgTarget["jarvis_totalgoplimitoutapproved"] = 100.0M;
            incidentImgTarget["jarvis_totalgoplimitoutapprovedcurrency"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentImgTarget["jarvis_totalrequestedccamount"] = 100.0M;
            incidentImgTarget["jarvis_totalcreditcardrequestedamountcurreny"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentImgTarget["jarvis_totalgoplimitin"] = 100.0M;
            incidentImgTarget["jarvis_totalcurrencyin"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentImgTarget["jarvis_registrationnumber"] = "123";
            incidentImgTarget["jarvis_country"] = new EntityReference("jarvis_country", sameGuid);
            incidentImgTarget["description"] = "This is for testing";
            incidentImgTarget["jarvis_location"] = new EntityReference("jarvis_country", sameGuid);
            incidentImgTarget["jarvis_customerexpectations"] = "test";
            incidentImgTarget["jarvis_incidentnature"] = "[\r\n  {\r\n    \"id\": \"3efba61a-85ad-44b7-a187-7f54acaf7a67\",\r\n    \"name\": \"Accident\"\r\n  },\r\n  {\r\n    \"_id\": \"3efba61a-85ad-44b7-a187-7f54acaf7a68\",\r\n    \"_name\": \"Air Problem\"\r\n  }\r\n]";
            incidentImgTarget["jarvis_callerrole"] = new OptionSetValue(4);
            incidentImgTarget["jarvis_callerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incidentImgTarget["jarvis_driverlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incidentImgTarget["isescalated"] = false;
            incidentImgTarget["createdon"] = DateTime.UtcNow;
            incidentImgTarget["jarvis_assistancetype"] = new OptionSetValue(334030000);
            incidentImgTarget.Attributes["modifiedby"] = users;
            incidentImgTarget.Attributes["createdby"] = users;
            incidentImgTarget["parentcaseid"] = new EntityReference("incident", Guid.NewGuid());
            incidentImgTarget["jarvis_hdrd"] = true;

            //// Setting Input Parameters.
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", incidentTarget),
               };
            this.PluginExecutionContext.PreEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PreImage", incidentImgTarget },
                };
            };
            this.PluginExecutionContext.MessageNameGet = () => "UPDATE";
            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();
                if (query.GetType().Name == "FetchExpression")
                {
                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_exchangerate'>"))
                    {
                        Entity exchangeRateResult = new Entity("jarvis_exchangerate");
                        exchangeRateResult["jarvis_value"] = 1M;
                        exchangeRateResult.Id = Guid.NewGuid();
                        result.Entities.Add(exchangeRateResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_gop'>"))
                    {
                        Entity gop = new Entity("jarvis_gop");
                        gop["modifiedon"] = DateTime.Now;
                        gop.Id = Guid.NewGuid();
                        result.Entities.Add(gop);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_configurationjarvis'>"))
                    {
                        Entity configResult = new Entity("jarvis_configurationjarvis");
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationcasestatuschange"] = true;
                        configResult["jarvis_automationreleasecase"] = new OptionSetValue(1);
                        configResult["jarvis_knowledgesearchforbd"] = "\"breakdown\" AND [incident.jarvis_incidentnatureshadow] AND ([incident.account.HD_country] OR [incident.jarvis_country])";
                        configResult["jarvis_incidentnatureconjunctionbd"] = "AND";
                        configResult["createdon"] = DateTime.UtcNow;
                        configResult.Id = Guid.NewGuid();
                        result.Entities.Add(configResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_region'>"))
                    {
                        Entity regionResult = new Entity("jarvis_region");
                        regionResult["jarvis_name"] = "testpassout";
                        regionResult.Id = Guid.NewGuid();
                        result.Entities.Add(regionResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_vehiclefuelpowertype'>"))
                    {
                        Entity fuelPowerType = new Entity("jarvis_vehiclefuelpowertype");
                        fuelPowerType["jarvis_name"] = "testpassout";
                        fuelPowerType[Constants.IncidentNature.IncidentNatureVehicleFuelPid_jarvisincidentnatureid] = new AliasedValue("incidentNature", "incidentnatureid", Guid.NewGuid());
                        fuelPowerType["IncidentNature_VehicleFuelPid.jarvis_name"] = new AliasedValue("incidentNature", "incidentnatureid", "Air Problem2");
                        fuelPowerType["jarvis_vehiclefuelpowertypeid"] = Guid.NewGuid();
                        fuelPowerType.Id = Guid.NewGuid();
                        result.Entities.Add(fuelPowerType);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='timezonedefinition'>"))
                    {
                        Entity timeZoneDef = new Entity("timezonedefinition");
                        timeZoneDef["userinterfacename"] = "GMT+1TestingpurposeTestMoreTest";
                        timeZoneDef.Id = Guid.NewGuid();
                        result.Entities.Add(timeZoneDef);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='systemuser'>"))
                    {
                        Entity userRef = new Entity("systemuser");
                        userRef["fullname"] = "MERCURIUS";
                        userRef.Id = Guid.NewGuid();
                        result.Entities.Add(userRef);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='queueitem'>"))
                    {
                        Entity queue = new Entity("queueitem");
                        queue["fullname"] = "MERCURIUS";
                        queue.Id = Guid.NewGuid();
                        result.Entities.Add(queue);
                    }
                }

                return result;
            };

            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {

                if (entityName == "systemuser")
                {
                    Entity user = new Entity("systemuser");
                    user.Id = Guid.NewGuid();
                    user.Attributes["fullname"] = "TEST";
                    return user;
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
                    country.Id = sameGuid;
                    country["jarvis_iso2countrycode"] = "NL";
                    country.Attributes[JarvisCountry.JarvisAverageetaduration] = "12";
                    country.Attributes["transactioncurrencyid"] = new EntityReference("transactioncurrency", Guid.NewGuid());
                    country.Attributes["jarvis_timezone"] = 106;
                    return country;
                }

                if (entityName == "jarvis_language")
                {
                    Entity language = new Entity("jarvis_language");
                    language.Id = guid;
                    language["jarvis_iso3languagecode6392t"] = "DEU";
                    language["jarvis_iso2languagecode6391"] = "DE";
                    language["jarvis_vasstandardlanguage"] = false;
                    return language;
                }

                if (entityName == "account")
                {
                    Entity account = new Entity("account");
                    account.Id = guid;
                    account["jarvis_language"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    account["jarvis_address1_country"] = new EntityReference("jarvis_country", sameGuid);
                    account["jarvis_blacklisted"] = false;
                    account["jarvis_onecasestatus"] = new OptionSetValue(334030000);
                    account["jarvis_accounttype"] = new OptionSetValue(334030001);
                    account["jarvis_responsableunitid"] = "NotDummy";
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

            CasePreOperationSync plugin = new CasePreOperationSync(null, null);
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// PreValidate Update With Complete Data Test Method Create case type code 3.
        /// </summary>
        [TestMethod]
        public void BreakdownCasePreOperationUpdateTypeCode700()
        {
            Entity incidentTarget = new Entity("incident");
            Entity incidentImgTarget = new Entity("incident");
            EntityReference users = new EntityReference("systemuser", Guid.NewGuid());
            users.Name = "TEST";

            incidentTarget.Id = Guid.Empty;
            Guid sameGuid = Guid.NewGuid();
            incidentTarget["incidentid"] = Guid.NewGuid();
            incidentTarget["caseorigincode"] = new OptionSetValue(2);
            incidentTarget["casetypecode"] = new OptionSetValue(2);
            incidentTarget["jarvis_mercuriusstatus"] = new OptionSetValue(700);
            incidentTarget["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
            incidentTarget["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["customerid"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["jarvis_restgoplimitout"] = 100.0M;
            incidentTarget["jarvis_totalrestcurrencyout"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentTarget["jarvis_totalgoplimitout"] = 100.0M;
            incidentTarget["jarvis_totalcurrencyout"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentTarget["jarvis_totalgoplimitoutapproved"] = 100.0M;
            incidentTarget["jarvis_totalgoplimitoutapprovedcurrency"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentTarget["jarvis_totalrequestedccamount"] = 100.0M;
            incidentTarget["jarvis_totalcreditcardrequestedamountcurreny"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentTarget["jarvis_totalgoplimitin"] = 100.0M;
            incidentTarget["jarvis_totalcurrencyin"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentTarget["jarvis_registrationnumber"] = "123";
            incidentTarget["jarvis_country"] = new EntityReference("jarvis_country", sameGuid);
            incidentTarget["description"] = "This is for testing";
            incidentTarget["jarvis_location"] = new EntityReference("jarvis_country", sameGuid);
            incidentTarget["jarvis_customerexpectations"] = "test";
            incidentTarget["jarvis_incidentnature"] = "[\r\n  {\r\n    \"id\": \"3efba61a-85ad-44b7-a187-7f54acaf7a67\",\r\n    \"name\": \"Accident\"\r\n  },\r\n  {\r\n    \"_id\": \"3efba61a-85ad-44b7-a187-7f54acaf7a68\",\r\n    \"_name\": \"Air Problem\"\r\n  }\r\n]";
            incidentTarget["jarvis_callerrole"] = new OptionSetValue(4);
            incidentTarget["jarvis_callerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incidentTarget["jarvis_driverlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incidentTarget["isescalated"] = false;
            incidentTarget["createdon"] = DateTime.UtcNow;
            incidentTarget["jarvis_assistancetype"] = new OptionSetValue(334030000);
            incidentTarget.Attributes["modifiedby"] = users;
            incidentTarget.Attributes["createdby"] = users;
            incidentTarget["parentcaseid"] = new EntityReference("incident", Guid.NewGuid());
            incidentTarget["ownerid"] = new EntityReference("systemuser", Guid.NewGuid());
            incidentTarget["statuscode"] = new OptionSetValue(20);

            incidentImgTarget["jarvis_fumonitorskill"] = new EntityReference("jarvis_fumonitorskill", Guid.NewGuid());
            incidentImgTarget["jarvis_futimestamp"] = DateTime.Now;
            incidentImgTarget["jarvis_casestatusupdate"] = DateTime.Now;
            incidentImgTarget["statuscode"] = new OptionSetValue(90);
            incidentImgTarget["incidentid"] = Guid.NewGuid();
            incidentImgTarget["statecode"] = new OptionSetValue(2);
            incidentImgTarget["caseorigincode"] = new OptionSetValue(2);
            incidentImgTarget["casetypecode"] = new OptionSetValue(2);
            incidentImgTarget["jarvis_mercuriusstatus"] = new OptionSetValue(700);
            incidentImgTarget["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
            incidentImgTarget["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incidentImgTarget["customerid"] = new EntityReference("account", Guid.NewGuid());
            incidentImgTarget["jarvis_restgoplimitout"] = 100.0M;
            incidentImgTarget["jarvis_totalrestcurrencyout"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentImgTarget["jarvis_totalgoplimitout"] = 100.0M;
            incidentImgTarget["jarvis_totalcurrencyout"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentImgTarget["jarvis_totalgoplimitoutapproved"] = 100.0M;
            incidentImgTarget["jarvis_totalgoplimitoutapprovedcurrency"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentImgTarget["jarvis_totalrequestedccamount"] = 100.0M;
            incidentImgTarget["jarvis_totalcreditcardrequestedamountcurreny"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentImgTarget["jarvis_totalgoplimitin"] = 100.0M;
            incidentImgTarget["jarvis_totalcurrencyin"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentImgTarget["jarvis_registrationnumber"] = "123";
            incidentImgTarget["jarvis_country"] = new EntityReference("jarvis_country", sameGuid);
            incidentImgTarget["description"] = "This is for testing";
            incidentImgTarget["jarvis_location"] = new EntityReference("jarvis_country", sameGuid);
            incidentImgTarget["jarvis_customerexpectations"] = "test";
            incidentImgTarget["jarvis_incidentnature"] = "[\r\n  {\r\n    \"id\": \"3efba61a-85ad-44b7-a187-7f54acaf7a67\",\r\n    \"name\": \"Accident\"\r\n  },\r\n  {\r\n    \"_id\": \"3efba61a-85ad-44b7-a187-7f54acaf7a68\",\r\n    \"_name\": \"Air Problem\"\r\n  }\r\n]";
            incidentImgTarget["jarvis_callerrole"] = new OptionSetValue(4);
            incidentImgTarget["jarvis_callerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incidentImgTarget["jarvis_driverlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incidentImgTarget["isescalated"] = false;
            incidentImgTarget["createdon"] = DateTime.UtcNow;
            incidentImgTarget["jarvis_assistancetype"] = new OptionSetValue(334030000);
            incidentImgTarget.Attributes["modifiedby"] = users;
            incidentImgTarget.Attributes["createdby"] = users;
            incidentImgTarget["parentcaseid"] = new EntityReference("incident", Guid.NewGuid());
            incidentImgTarget["jarvis_hdrd"] = true;

            //// Setting Input Parameters.
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", incidentTarget),
               };
            this.PluginExecutionContext.PreEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PreImage", incidentImgTarget },
                };
            };
            this.PluginExecutionContext.MessageNameGet = () => "UPDATE";
            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();
                if (query.GetType().Name == "FetchExpression")
                {
                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_exchangerate'>"))
                    {
                        Entity exchangeRateResult = new Entity("jarvis_exchangerate");
                        exchangeRateResult["jarvis_value"] = 1M;
                        exchangeRateResult.Id = Guid.NewGuid();
                        result.Entities.Add(exchangeRateResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_gop'>"))
                    {
                        Entity gop = new Entity("jarvis_gop");
                        gop["modifiedon"] = DateTime.Now;
                        gop.Id = Guid.NewGuid();
                        result.Entities.Add(gop);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_configurationjarvis'>"))
                    {
                        Entity configResult = new Entity("jarvis_configurationjarvis");
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationcasestatuschange"] = true;
                        configResult["jarvis_automationreleasecase"] = new OptionSetValue(1);
                        configResult["jarvis_knowledgesearchforbd"] = "\"breakdown\" AND [incident.jarvis_incidentnatureshadow] AND ([incident.account.HD_country] OR [incident.jarvis_country])";
                        configResult["jarvis_incidentnatureconjunctionbd"] = "AND";
                        configResult["createdon"] = DateTime.UtcNow;
                        configResult.Id = Guid.NewGuid();
                        result.Entities.Add(configResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_region'>"))
                    {
                        Entity regionResult = new Entity("jarvis_region");
                        regionResult["jarvis_name"] = "testpassout";
                        regionResult.Id = Guid.NewGuid();
                        result.Entities.Add(regionResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_vehiclefuelpowertype'>"))
                    {
                        Entity fuelPowerType = new Entity("jarvis_vehiclefuelpowertype");
                        fuelPowerType["jarvis_name"] = "testpassout";
                        fuelPowerType[Constants.IncidentNature.IncidentNatureVehicleFuelPid_jarvisincidentnatureid] = new AliasedValue("incidentNature", "incidentnatureid", Guid.NewGuid());
                        fuelPowerType["IncidentNature_VehicleFuelPid.jarvis_name"] = new AliasedValue("incidentNature", "incidentnatureid", "Air Problem2");
                        fuelPowerType["jarvis_vehiclefuelpowertypeid"] = Guid.NewGuid();
                        fuelPowerType.Id = Guid.NewGuid();
                        result.Entities.Add(fuelPowerType);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='timezonedefinition'>"))
                    {
                        Entity timeZoneDef = new Entity("timezonedefinition");
                        timeZoneDef["userinterfacename"] = "GMT+1TestingpurposeTestMoreTest";
                        timeZoneDef.Id = Guid.NewGuid();
                        result.Entities.Add(timeZoneDef);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='systemuser'>"))
                    {
                        Entity userRef = new Entity("systemuser");
                        userRef["fullname"] = "MERCURIUS";
                        userRef.Id = Guid.NewGuid();
                        result.Entities.Add(userRef);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='queueitem'>"))
                    {
                        Entity queue = new Entity("queueitem");
                        queue["fullname"] = "MERCURIUS";
                        queue.Id = Guid.NewGuid();
                        result.Entities.Add(queue);
                    }
                }

                return result;
            };

            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {

                if (entityName == "systemuser")
                {
                    Entity user = new Entity("systemuser");
                    user.Id = Guid.NewGuid();
                    user.Attributes["fullname"] = "TEST";
                    return user;
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
                    country.Id = sameGuid;
                    country["jarvis_iso2countrycode"] = "NL";
                    country.Attributes[JarvisCountry.JarvisAverageetaduration] = "12";
                    country.Attributes["transactioncurrencyid"] = new EntityReference("transactioncurrency", Guid.NewGuid());
                    country.Attributes["jarvis_timezone"] = 106;
                    return country;
                }

                if (entityName == "jarvis_language")
                {
                    Entity language = new Entity("jarvis_language");
                    language.Id = guid;
                    language["jarvis_iso3languagecode6392t"] = "DEU";
                    language["jarvis_iso2languagecode6391"] = "DE";
                    language["jarvis_vasstandardlanguage"] = false;
                    return language;
                }

                if (entityName == "account")
                {
                    Entity account = new Entity("account");
                    account.Id = guid;
                    account["jarvis_language"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    account["jarvis_address1_country"] = new EntityReference("jarvis_country", sameGuid);
                    account["jarvis_blacklisted"] = false;
                    account["jarvis_onecasestatus"] = new OptionSetValue(334030000);
                    account["jarvis_accounttype"] = new OptionSetValue(334030001);
                    account["jarvis_responsableunitid"] = "NotDummy";
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

            CasePreOperationSync plugin = new CasePreOperationSync(null, null);
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// PreValidate Update With Complete Data Test Method Create case type code 3.
        /// </summary>
        [TestMethod]
        public void BreakdownCasePreOperationUpdateTypeCode800()
        {
            Entity incidentTarget = new Entity("incident");
            Entity incidentImgTarget = new Entity("incident");
            EntityReference users = new EntityReference("systemuser", Guid.NewGuid());
            users.Name = "TEST";

            incidentTarget.Id = Guid.Empty;
            Guid sameGuid = Guid.NewGuid();
            incidentTarget["incidentid"] = Guid.NewGuid();
            incidentTarget["caseorigincode"] = new OptionSetValue(2);
            incidentTarget["casetypecode"] = new OptionSetValue(2);
            incidentTarget["jarvis_mercuriusstatus"] = new OptionSetValue(800);
            incidentTarget["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
            incidentTarget["jarvis_homedealer"] = new EntityReference("accoun8t", Guid.NewGuid());
            incidentTarget["customerid"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["jarvis_restgoplimitout"] = 100.0M;
            incidentTarget["jarvis_totalrestcurrencyout"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentTarget["jarvis_totalgoplimitout"] = 100.0M;
            incidentTarget["jarvis_totalcurrencyout"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentTarget["jarvis_totalgoplimitoutapproved"] = 100.0M;
            incidentTarget["jarvis_totalgoplimitoutapprovedcurrency"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentTarget["jarvis_totalrequestedccamount"] = 100.0M;
            incidentTarget["jarvis_totalcreditcardrequestedamountcurreny"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentTarget["jarvis_totalgoplimitin"] = 100.0M;
            incidentTarget["jarvis_totalcurrencyin"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentTarget["jarvis_registrationnumber"] = "123";
            incidentTarget["jarvis_country"] = new EntityReference("jarvis_country", sameGuid);
            incidentTarget["description"] = "This is for testing";
            incidentTarget["jarvis_location"] = new EntityReference("jarvis_country", sameGuid);
            incidentTarget["jarvis_customerexpectations"] = "test";
            incidentTarget["jarvis_incidentnature"] = "[\r\n  {\r\n    \"id\": \"3efba61a-85ad-44b7-a187-7f54acaf7a67\",\r\n    \"name\": \"Accident\"\r\n  },\r\n  {\r\n    \"_id\": \"3efba61a-85ad-44b7-a187-7f54acaf7a68\",\r\n    \"_name\": \"Air Problem\"\r\n  }\r\n]";
            incidentTarget["jarvis_callerrole"] = new OptionSetValue(4);
            incidentTarget["jarvis_callerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incidentTarget["jarvis_driverlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incidentTarget["isescalated"] = false;
            incidentTarget["createdon"] = DateTime.UtcNow;
            incidentTarget["jarvis_assistancetype"] = new OptionSetValue(334030000);
            incidentTarget.Attributes["modifiedby"] = users;
            incidentTarget.Attributes["createdby"] = users;
            incidentTarget["parentcaseid"] = new EntityReference("incident", Guid.NewGuid());
            incidentTarget["ownerid"] = new EntityReference("systemuser", Guid.NewGuid());
            incidentTarget["statuscode"] = new OptionSetValue(20);

            incidentImgTarget["jarvis_fumonitorskill"] = new EntityReference("jarvis_fumonitorskill", Guid.NewGuid());
            incidentImgTarget["jarvis_futimestamp"] = DateTime.Now;
            incidentImgTarget["jarvis_casestatusupdate"] = DateTime.Now;
            incidentImgTarget["statuscode"] = new OptionSetValue(90);
            incidentImgTarget["incidentid"] = Guid.NewGuid();
            incidentImgTarget["statecode"] = new OptionSetValue(2);
            incidentImgTarget["caseorigincode"] = new OptionSetValue(2);
            incidentImgTarget["casetypecode"] = new OptionSetValue(2);
            incidentImgTarget["jarvis_mercuriusstatus"] = new OptionSetValue(800);
            incidentImgTarget["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
            incidentImgTarget["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incidentImgTarget["customerid"] = new EntityReference("account", Guid.NewGuid());
            incidentImgTarget["jarvis_restgoplimitout"] = 100.0M;
            incidentImgTarget["jarvis_totalrestcurrencyout"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentImgTarget["jarvis_totalgoplimitout"] = 100.0M;
            incidentImgTarget["jarvis_totalcurrencyout"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentImgTarget["jarvis_totalgoplimitoutapproved"] = 100.0M;
            incidentImgTarget["jarvis_totalgoplimitoutapprovedcurrency"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentImgTarget["jarvis_totalrequestedccamount"] = 100.0M;
            incidentImgTarget["jarvis_totalcreditcardrequestedamountcurreny"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentImgTarget["jarvis_totalgoplimitin"] = 100.0M;
            incidentImgTarget["jarvis_totalcurrencyin"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentImgTarget["jarvis_registrationnumber"] = "123";
            incidentImgTarget["jarvis_country"] = new EntityReference("jarvis_country", sameGuid);
            incidentImgTarget["description"] = "This is for testing";
            incidentImgTarget["jarvis_location"] = new EntityReference("jarvis_country", sameGuid);
            incidentImgTarget["jarvis_customerexpectations"] = "test";
            incidentImgTarget["jarvis_incidentnature"] = "[\r\n  {\r\n    \"id\": \"3efba61a-85ad-44b7-a187-7f54acaf7a67\",\r\n    \"name\": \"Accident\"\r\n  },\r\n  {\r\n    \"_id\": \"3efba61a-85ad-44b7-a187-7f54acaf7a68\",\r\n    \"_name\": \"Air Problem\"\r\n  }\r\n]";
            incidentImgTarget["jarvis_callerrole"] = new OptionSetValue(4);
            incidentImgTarget["jarvis_callerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incidentImgTarget["jarvis_driverlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incidentImgTarget["isescalated"] = false;
            incidentImgTarget["createdon"] = DateTime.UtcNow;
            incidentImgTarget["jarvis_assistancetype"] = new OptionSetValue(334030000);
            incidentImgTarget.Attributes["modifiedby"] = users;
            incidentImgTarget.Attributes["createdby"] = users;
            incidentImgTarget["parentcaseid"] = new EntityReference("incident", Guid.NewGuid());
            incidentImgTarget["jarvis_hdrd"] = true;

            //// Setting Input Parameters.
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", incidentTarget),
               };
            this.PluginExecutionContext.PreEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PreImage", incidentImgTarget },
                };
            };
            this.PluginExecutionContext.MessageNameGet = () => "UPDATE";
            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();
                if (query.GetType().Name == "FetchExpression")
                {
                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_exchangerate'>"))
                    {
                        Entity exchangeRateResult = new Entity("jarvis_exchangerate");
                        exchangeRateResult["jarvis_value"] = 1M;
                        exchangeRateResult.Id = Guid.NewGuid();
                        result.Entities.Add(exchangeRateResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_gop'>"))
                    {
                        Entity gop = new Entity("jarvis_gop");
                        gop["modifiedon"] = DateTime.Now;
                        gop.Id = Guid.NewGuid();
                        result.Entities.Add(gop);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_configurationjarvis'>"))
                    {
                        Entity configResult = new Entity("jarvis_configurationjarvis");
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationcasestatuschange"] = true;
                        configResult["jarvis_automationreleasecase"] = new OptionSetValue(1);
                        configResult["jarvis_knowledgesearchforbd"] = "\"breakdown\" AND [incident.jarvis_incidentnatureshadow] AND ([incident.account.HD_country] OR [incident.jarvis_country])";
                        configResult["jarvis_incidentnatureconjunctionbd"] = "AND";
                        configResult["createdon"] = DateTime.UtcNow;
                        configResult.Id = Guid.NewGuid();
                        result.Entities.Add(configResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_region'>"))
                    {
                        Entity regionResult = new Entity("jarvis_region");
                        regionResult["jarvis_name"] = "testpassout";
                        regionResult.Id = Guid.NewGuid();
                        result.Entities.Add(regionResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_vehiclefuelpowertype'>"))
                    {
                        Entity fuelPowerType = new Entity("jarvis_vehiclefuelpowertype");
                        fuelPowerType["jarvis_name"] = "testpassout";
                        fuelPowerType[Constants.IncidentNature.IncidentNatureVehicleFuelPid_jarvisincidentnatureid] = new AliasedValue("incidentNature", "incidentnatureid", Guid.NewGuid());
                        fuelPowerType["IncidentNature_VehicleFuelPid.jarvis_name"] = new AliasedValue("incidentNature", "incidentnatureid", "Air Problem2");
                        fuelPowerType["jarvis_vehiclefuelpowertypeid"] = Guid.NewGuid();
                        fuelPowerType.Id = Guid.NewGuid();
                        result.Entities.Add(fuelPowerType);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='timezonedefinition'>"))
                    {
                        Entity timeZoneDef = new Entity("timezonedefinition");
                        timeZoneDef["userinterfacename"] = "GMT+1TestingpurposeTestMoreTest";
                        timeZoneDef.Id = Guid.NewGuid();
                        result.Entities.Add(timeZoneDef);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='systemuser'>"))
                    {
                        Entity userRef = new Entity("systemuser");
                        userRef["fullname"] = "MERCURIUS";
                        userRef.Id = Guid.NewGuid();
                        result.Entities.Add(userRef);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='queueitem'>"))
                    {
                        Entity queue = new Entity("queueitem");
                        queue["fullname"] = "MERCURIUS";
                        queue.Id = Guid.NewGuid();
                        result.Entities.Add(queue);
                    }
                }

                return result;
            };

            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {

                if (entityName == "systemuser")
                {
                    Entity user = new Entity("systemuser");
                    user.Id = Guid.NewGuid();
                    user.Attributes["fullname"] = "TEST";
                    return user;
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
                    country.Id = sameGuid;
                    country["jarvis_iso2countrycode"] = "NL";
                    country.Attributes[JarvisCountry.JarvisAverageetaduration] = "12";
                    country.Attributes["transactioncurrencyid"] = new EntityReference("transactioncurrency", Guid.NewGuid());
                    country.Attributes["jarvis_timezone"] = 106;
                    return country;
                }

                if (entityName == "jarvis_language")
                {
                    Entity language = new Entity("jarvis_language");
                    language.Id = guid;
                    language["jarvis_iso3languagecode6392t"] = "DEU";
                    language["jarvis_iso2languagecode6391"] = "DE";
                    language["jarvis_vasstandardlanguage"] = false;
                    return language;
                }

                if (entityName == "accoun8t")
                {
                    Entity account = new Entity("account");
                    account.Id = guid;
                    account["jarvis_language"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    account["jarvis_address1_country"] = new EntityReference("jarvis_country", sameGuid);
                    account["jarvis_blacklisted"] = false;
                    account["jarvis_onecasestatus"] = new OptionSetValue(334030000);
                    account["jarvis_accounttype"] = new OptionSetValue(334030001);
                    account["jarvis_responsableunitid"] = "NotDummy";
                    return account;
                }

                if (entityName == "account")
                {
                    Entity account = new Entity("account");
                    account.Id = guid;
                    account["jarvis_language"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    account["jarvis_address1_country"] = new EntityReference("jarvis_country", sameGuid);
                    account["jarvis_blacklisted"] = false;
                    account["jarvis_onecasestatus"] = new OptionSetValue(334030000);
                    account["jarvis_accounttype"] = new OptionSetValue(334030001);
                    account["jarvis_responsableunitid"] = "NotDummy";
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

            CasePreOperationSync plugin = new CasePreOperationSync(null, null);
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// PreValidate Update With Complete Data Test Method Create case type code 3.
        /// </summary>
        [TestMethod]
        public void BreakdownCasePreOperationUpdateTypeCode901()
        {
            Entity incidentTarget = new Entity("incident");
            Entity incidentImgTarget = new Entity("incident");
            EntityReference users = new EntityReference("systemuser", Guid.NewGuid());
            users.Name = "TEST";

            incidentTarget.Id = Guid.Empty;
            Guid sameGuid = Guid.NewGuid();
            incidentTarget["incidentid"] = Guid.NewGuid();
            incidentTarget["caseorigincode"] = new OptionSetValue(2);
            incidentTarget["casetypecode"] = new OptionSetValue(2);
            incidentTarget["jarvis_mercuriusstatus"] = new OptionSetValue(900);
            incidentTarget["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
            incidentTarget["jarvis_homedealer"] = new EntityReference("accoun8t", Guid.NewGuid());
            incidentTarget["customerid"] = new EntityReference("account", Guid.NewGuid());
            incidentTarget["jarvis_restgoplimitout"] = 100.0M;
            incidentTarget["jarvis_totalrestcurrencyout"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentTarget["jarvis_totalgoplimitout"] = 100.0M;
            incidentTarget["jarvis_totalcurrencyout"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentTarget["jarvis_totalgoplimitoutapproved"] = 100.0M;
            incidentTarget["jarvis_totalgoplimitoutapprovedcurrency"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentTarget["jarvis_totalrequestedccamount"] = 100.0M;
            incidentTarget["jarvis_totalcreditcardrequestedamountcurreny"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentTarget["jarvis_totalgoplimitin"] = 100.0M;
            incidentTarget["jarvis_totalcurrencyin"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentTarget["jarvis_registrationnumber"] = "123";
            incidentTarget["jarvis_country"] = new EntityReference("jarvis_country", sameGuid);
            incidentTarget["description"] = "This is for testing";
            incidentTarget["jarvis_location"] = new EntityReference("jarvis_country", sameGuid);
            incidentTarget["jarvis_customerexpectations"] = "test";
            incidentTarget["jarvis_incidentnature"] = "[\r\n  {\r\n    \"id\": \"3efba61a-85ad-44b7-a187-7f54acaf7a67\",\r\n    \"name\": \"Accident\"\r\n  },\r\n  {\r\n    \"_id\": \"3efba61a-85ad-44b7-a187-7f54acaf7a68\",\r\n    \"_name\": \"Air Problem\"\r\n  }\r\n]";
            incidentTarget["jarvis_callerrole"] = new OptionSetValue(4);
            incidentTarget["jarvis_callerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incidentTarget["jarvis_driverlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incidentTarget["isescalated"] = false;
            incidentTarget["createdon"] = DateTime.UtcNow;
            incidentTarget["jarvis_assistancetype"] = new OptionSetValue(334030000);
            incidentTarget.Attributes["modifiedby"] = users;
            incidentTarget.Attributes["createdby"] = users;
            incidentTarget.Attributes["createdby"] = users;
            incidentTarget["parentcaseid"] = new EntityReference("incident", Guid.NewGuid());
            incidentTarget["ownerid"] = new EntityReference("systemuser", Guid.NewGuid());
            incidentTarget["statuscode"] = new OptionSetValue(20);
            incidentTarget["jarvis_customerinformed"] = false;

            incidentImgTarget["jarvis_fumonitorskill"] = new EntityReference("jarvis_fumonitorskill", Guid.NewGuid());
            incidentImgTarget["jarvis_futimestamp"] = DateTime.Now;
            incidentImgTarget["jarvis_casestatusupdate"] = DateTime.Now;
            incidentImgTarget["statuscode"] = new OptionSetValue(90);
            incidentImgTarget["incidentid"] = Guid.NewGuid();
            incidentImgTarget["statecode"] = new OptionSetValue(2);
            incidentImgTarget["caseorigincode"] = new OptionSetValue(2);
            incidentImgTarget["casetypecode"] = new OptionSetValue(2);
            incidentImgTarget["jarvis_mercuriusstatus"] = new OptionSetValue(900);
            incidentImgTarget["jarvis_vehicle"] = new EntityReference("jarvis_vehicle", Guid.NewGuid());
            incidentImgTarget["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
            incidentImgTarget["customerid"] = new EntityReference("account", Guid.NewGuid());
            incidentImgTarget["jarvis_restgoplimitout"] = 100.0M;
            incidentImgTarget["jarvis_totalrestcurrencyout"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentImgTarget["jarvis_totalgoplimitout"] = 100.0M;
            incidentImgTarget["jarvis_totalcurrencyout"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentImgTarget["jarvis_totalgoplimitoutapproved"] = 100.0M;
            incidentImgTarget["jarvis_totalgoplimitoutapprovedcurrency"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentImgTarget["jarvis_totalrequestedccamount"] = 100.0M;
            incidentImgTarget["jarvis_totalcreditcardrequestedamountcurreny"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentImgTarget["jarvis_totalgoplimitin"] = 100.0M;
            incidentImgTarget["jarvis_totalcurrencyin"] = new EntityReference("transcationcurrency", Guid.NewGuid());
            incidentImgTarget["jarvis_registrationnumber"] = "123";
            incidentImgTarget["jarvis_country"] = new EntityReference("jarvis_country", sameGuid);
            incidentImgTarget["description"] = "This is for testing";
            incidentImgTarget["jarvis_location"] = new EntityReference("jarvis_country", sameGuid);
            incidentImgTarget["jarvis_customerexpectations"] = "test";
            incidentImgTarget["jarvis_incidentnature"] = "[\r\n  {\r\n    \"id\": \"3efba61a-85ad-44b7-a187-7f54acaf7a67\",\r\n    \"name\": \"Accident\"\r\n  },\r\n  {\r\n    \"_id\": \"3efba61a-85ad-44b7-a187-7f54acaf7a68\",\r\n    \"_name\": \"Air Problem\"\r\n  }\r\n]";
            incidentImgTarget["jarvis_callerrole"] = new OptionSetValue(4);
            incidentImgTarget["jarvis_callerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incidentImgTarget["jarvis_driverlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
            incidentImgTarget["isescalated"] = false;
            incidentImgTarget["createdon"] = DateTime.UtcNow;
            incidentImgTarget["jarvis_assistancetype"] = new OptionSetValue(334030000);
            incidentImgTarget.Attributes["modifiedby"] = users;
            incidentImgTarget.Attributes["createdby"] = users;
            incidentImgTarget["parentcaseid"] = new EntityReference("incident", Guid.NewGuid());
            incidentImgTarget["jarvis_hdrd"] = true;

            //// Setting Input Parameters.
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", incidentTarget),
               };
            this.PluginExecutionContext.PreEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PreImage", incidentImgTarget },
                };
            };
            this.PluginExecutionContext.MessageNameGet = () => "UPDATE";
            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();
                if (query.GetType().Name == "FetchExpression")
                {
                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_exchangerate'>"))
                    {
                        Entity exchangeRateResult = new Entity("jarvis_exchangerate");
                        exchangeRateResult["jarvis_value"] = 1M;
                        exchangeRateResult.Id = Guid.NewGuid();
                        result.Entities.Add(exchangeRateResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_gop'>"))
                    {
                        Entity gop = new Entity("jarvis_gop");
                        gop["modifiedon"] = DateTime.Now;
                        gop.Id = Guid.NewGuid();
                        result.Entities.Add(gop);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_configurationjarvis'>"))
                    {
                        Entity configResult = new Entity("jarvis_configurationjarvis");
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationcasestatuschange"] = true;
                        configResult["jarvis_automationreleasecase"] = new OptionSetValue(1);
                        configResult["jarvis_knowledgesearchforbd"] = "\"breakdown\" AND [incident.jarvis_incidentnatureshadow] AND ([incident.account.HD_country] OR [incident.jarvis_country])";
                        configResult["jarvis_incidentnatureconjunctionbd"] = "AND";
                        configResult["createdon"] = DateTime.UtcNow;
                        configResult.Id = Guid.NewGuid();
                        result.Entities.Add(configResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_region'>"))
                    {
                        Entity regionResult = new Entity("jarvis_region");
                        regionResult["jarvis_name"] = "testpassout";
                        regionResult.Id = Guid.NewGuid();
                        result.Entities.Add(regionResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_vehiclefuelpowertype'>"))
                    {
                        Entity fuelPowerType = new Entity("jarvis_vehiclefuelpowertype");
                        fuelPowerType["jarvis_name"] = "testpassout";
                        fuelPowerType[Constants.IncidentNature.IncidentNatureVehicleFuelPid_jarvisincidentnatureid] = new AliasedValue("incidentNature", "incidentnatureid", Guid.NewGuid());
                        fuelPowerType["IncidentNature_VehicleFuelPid.jarvis_name"] = new AliasedValue("incidentNature", "incidentnatureid", "Air Problem2");
                        fuelPowerType["jarvis_vehiclefuelpowertypeid"] = Guid.NewGuid();
                        fuelPowerType.Id = Guid.NewGuid();
                        result.Entities.Add(fuelPowerType);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='timezonedefinition'>"))
                    {
                        Entity timeZoneDef = new Entity("timezonedefinition");
                        timeZoneDef["userinterfacename"] = "GMT+1TestingpurposeTestMoreTest";
                        timeZoneDef.Id = Guid.NewGuid();
                        result.Entities.Add(timeZoneDef);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='systemuser'>"))
                    {
                        Entity userRef = new Entity("systemuser");
                        userRef["fullname"] = "MERCURIUS";
                        userRef.Id = Guid.NewGuid();
                        result.Entities.Add(userRef);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='queueitem'>"))
                    {
                        Entity queue = new Entity("queueitem");
                        queue["fullname"] = "MERCURIUS";
                        queue.Id = Guid.NewGuid();
                        result.Entities.Add(queue);
                    }
                }

                return result;
            };

            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {

                if (entityName == "systemuser")
                {
                    Entity user = new Entity("systemuser");
                    user.Id = Guid.NewGuid();
                    user.Attributes["fullname"] = "TEST";
                    return user;
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
                    country.Id = sameGuid;
                    country["jarvis_iso2countrycode"] = "NL";
                    country.Attributes[JarvisCountry.JarvisAverageetaduration] = "12";
                    country.Attributes["transactioncurrencyid"] = new EntityReference("transactioncurrency", Guid.NewGuid());
                    country.Attributes["jarvis_timezone"] = 106;
                    return country;
                }

                if (entityName == "jarvis_language")
                {
                    Entity language = new Entity("jarvis_language");
                    language.Id = guid;
                    language["jarvis_iso3languagecode6392t"] = "DEU";
                    language["jarvis_iso2languagecode6391"] = "DE";
                    language["jarvis_vasstandardlanguage"] = false;
                    return language;
                }

                if (entityName == "accoun8t")
                {
                    Entity account = new Entity("account");
                    account.Id = guid;
                    account["jarvis_language"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    account["jarvis_address1_country"] = new EntityReference("jarvis_country", sameGuid);
                    account["jarvis_blacklisted"] = false;
                    account["jarvis_onecasestatus"] = new OptionSetValue(334030000);
                    account["jarvis_accounttype"] = new OptionSetValue(334030001);
                    account["jarvis_responsableunitid"] = "NotDummy";
                    return account;
                }

                if (entityName == "account")
                {
                    Entity account = new Entity("account");
                    account.Id = guid;
                    account["jarvis_language"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    account["jarvis_address1_country"] = new EntityReference("jarvis_country", sameGuid);
                    account["jarvis_blacklisted"] = false;
                    account["jarvis_onecasestatus"] = new OptionSetValue(334030000);
                    account["jarvis_accounttype"] = new OptionSetValue(334030001);
                    account["jarvis_responsableunitid"] = "NotDummy";
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

            CasePreOperationSync plugin = new CasePreOperationSync(null, null);
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// PreValidate Update With Complete Data Test Method Create case type code 2.
        /// </summary>
        public void CasePreOperationDepthCheck1()
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
    }
}
