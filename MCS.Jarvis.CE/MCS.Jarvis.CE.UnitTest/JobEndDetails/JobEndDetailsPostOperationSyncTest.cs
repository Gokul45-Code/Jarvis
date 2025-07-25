// <copyright file="JobEndDetailsPostOperationSyncTest.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MCS.Jarvis.CE.UnitTest
{
    using System;
    using System.Collections.Generic;
    using MCS.Jarvis.CE.Plugins;
    using Microsoft.Crm.Sdk.Messages;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;

    /// <summary>
    /// Job End Details Post Operation Sync Test.
    /// </summary>
    [TestClass]
    public class JobEndDetailsPostOperationSyncTest : UnitTestBase
    {
        /// <summary>
        /// Job End Details Execute Create Test.
        /// </summary>
        [TestMethod]
        public void JobEndDetailsExecuteCreateTest()
        {
            var jedInfo = new Entity("jarvis_jobenddetailses");
            jedInfo.Id = Guid.NewGuid();
            jedInfo["jarvis_incident"] = new EntityReference("incident", Guid.NewGuid());
            jedInfo.Attributes["jarvis_repairingdealerpassout"] = new EntityReference("jarvis_passout", Guid.NewGuid());
            jedInfo.Attributes["jarvis_actualcausefault"] = "test";
            jedInfo.Attributes["jarvis_mileage"] = Convert.ToDecimal(12);
            jedInfo.Attributes["jarvis_mileageunit"] = new EntityReference("jarvis_mileages", Guid.NewGuid());
            jedInfo.Attributes["jarvis_temporaryrepair"] = "test";
            jedInfo.Attributes["jarvis_translationstatusactualcausefault"] = new OptionSetValue(334030000);
            jedInfo.Attributes["jarvis_translationstatustemporaryrepair"] = new OptionSetValue(334030000);
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", jedInfo),
               };
            this.PluginExecutionContext.MessageNameGet = () => "CREATE";
            this.PluginExecutionContext.StageGet = () => 40;

            // Setting service retrieve.
            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "jarvis_passout")
                {
                    Entity passout = new Entity("jarvis_passout");
                    passout.Id = Guid.NewGuid();
                    passout["jarvis_repairingdealer"] = new EntityReference("account", Guid.NewGuid());
                    return passout;
                }

                if (entityName == "incident")
                {
                    Entity inc = new Entity("incident");
                    inc.Id = guid;
                    inc["statuscode"] = new OptionSetValue(80);
                    inc["customerid"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_atc"] = DateTime.UtcNow;
                    inc["jarvis_onetimecustomercountry"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    inc["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    inc["jarvis_onetimecustomerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    return inc;
                }

                if (entityName == "account")
                {
                    Entity account = new Entity("account");
                    account.Id = guid;
                    account["jarvis_language"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    account["jarvis_address1_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    return account;
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

                if (entityName == "jarvis_jobenddetailses")
                {
                    Entity jedInfoRecord = new Entity("jarvis_jobenddetailses");
                    jedInfoRecord.Id = Guid.NewGuid();
                    jedInfoRecord["jarvis_incident"] = new EntityReference("incident", Guid.NewGuid());
                    jedInfoRecord.Attributes["jarvis_repairingdealerpassout"] = new EntityReference("jarvis_passout", Guid.NewGuid());
                    jedInfoRecord.Attributes["jarvis_actualcausefault"] = "test";
                    jedInfoRecord.Attributes["jarvis_mileage"] = "12";
                    jedInfoRecord.Attributes["jarvis_mileageunit"] = new EntityReference("jarvis_mileages", Guid.NewGuid());
                    jedInfoRecord.Attributes["jarvis_temporaryrepair"] = "test";
                    jedInfoRecord.Attributes["jarvis_translationstatusactualcausefault"] = new OptionSetValue(334030000);
                    jedInfoRecord.Attributes["jarvis_translationstatustemporaryrepair"] = new OptionSetValue(334030000);
                    return jedInfoRecord;
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
                        passoutResult.Id = Guid.NewGuid();
                        result.Entities.Add(passoutResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_casecontact'>"))
                    {
                        Entity caseContact = new Entity("jarvis_casecontact");
                        caseContact["jarvis_name"] = "testcontact";
                        caseContact["jarvis_preferredlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
                        caseContact["createdon"] = DateTime.UtcNow;
                        caseContact.Id = Guid.NewGuid();
                        result.Entities.Add(caseContact);
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

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_configurationjarvis'>"))
                    {
                        Entity configResult = new Entity("jarvis_configurationjarvis");
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationmonitoraction"] = true;
                        configResult["jarvis_automationcasestatuschange"] = true;
                        configResult["createdon"] = DateTime.UtcNow;
                        configResult.Id = Guid.NewGuid();
                        result.Entities.Add(configResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_casemonitoraction'>"))
                    {
                        Entity casemonitoraction = new Entity("jarvis_casemonitoraction");
                        casemonitoraction["subject"] = true;
                        casemonitoraction["statuscode"] = new OptionSetValue(2);
                        casemonitoraction.Id = Guid.NewGuid();
                        result.Entities.Add(casemonitoraction);
                    }
                }

                return result;
            };
            JobEndDetailsPostOperationSync plugin = new JobEndDetailsPostOperationSync();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Job End Details Execute Create Test.
        /// </summary>
        [TestMethod]
        public void JobEndDetailsExecuteCreateNoContactsTest()
        {
            var jedInfo = new Entity("jarvis_jobenddetailses");
            jedInfo.Id = Guid.NewGuid();
            jedInfo["jarvis_incident"] = new EntityReference("incident", Guid.NewGuid());
            jedInfo.Attributes["jarvis_repairingdealerpassout"] = new EntityReference("jarvis_passout", Guid.NewGuid());
            jedInfo.Attributes["jarvis_actualcausefault"] = "test";
            jedInfo.Attributes["jarvis_mileage"] = Convert.ToDecimal(12);
            jedInfo.Attributes["jarvis_mileageunit"] = new EntityReference("jarvis_mileages", Guid.NewGuid());
            jedInfo.Attributes["jarvis_temporaryrepair"] = "test";
            jedInfo.Attributes["jarvis_translationstatusactualcausefault"] = new OptionSetValue(334030000);
            jedInfo.Attributes["jarvis_translationstatustemporaryrepair"] = new OptionSetValue(334030000);
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", jedInfo),
               };
            this.PluginExecutionContext.MessageNameGet = () => "CREATE";
            this.PluginExecutionContext.StageGet = () => 40;

            // Setting service retrieve.
            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "incident")
                {
                    Entity inc = new Entity("incident");
                    inc.Id = guid;
                    inc["statuscode"] = new OptionSetValue(80);
                    inc["customerid"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_atc"] = DateTime.UtcNow;
                    inc["jarvis_onetimecustomercountry"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    inc["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    inc["jarvis_onetimecustomerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    return inc;
                }

                if (entityName == "account")
                {
                    Entity account = new Entity("account");
                    account.Id = guid;
                    account["jarvis_language"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    account["jarvis_address1_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    return account;
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

                if (entityName == "jarvis_jobenddetailses")
                {
                    Entity jedInfoRecord = new Entity("jarvis_jobenddetailses");
                    jedInfoRecord.Id = Guid.NewGuid();
                    jedInfoRecord["jarvis_incident"] = new EntityReference("incident", Guid.NewGuid());
                    jedInfoRecord.Attributes["jarvis_repairingdealerpassout"] = new EntityReference("jarvis_passout", Guid.NewGuid());
                    jedInfoRecord.Attributes["jarvis_actualcausefault"] = "test";
                    jedInfoRecord.Attributes["jarvis_mileage"] = "12";
                    jedInfoRecord.Attributes["jarvis_mileageunit"] = new EntityReference("jarvis_mileages", Guid.NewGuid());
                    jedInfoRecord.Attributes["jarvis_temporaryrepair"] = "test";
                    jedInfoRecord.Attributes["jarvis_translationstatusactualcausefault"] = new OptionSetValue(334030000);
                    jedInfoRecord.Attributes["jarvis_translationstatustemporaryrepair"] = new OptionSetValue(334030000);
                    return jedInfoRecord;
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
                        passoutResult.Id = Guid.NewGuid();
                        result.Entities.Add(passoutResult);
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

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_configurationjarvis'>"))
                    {
                        Entity configResult = new Entity("jarvis_configurationjarvis");
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationmonitoraction"] = true;
                        configResult["jarvis_automationcasestatuschange"] = true;
                        configResult["createdon"] = DateTime.UtcNow;
                        configResult.Id = Guid.NewGuid();
                        result.Entities.Add(configResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_casemonitoraction'>"))
                    {
                        Entity casemonitoraction = new Entity("jarvis_casemonitoraction");
                        casemonitoraction["subject"] = true;
                        casemonitoraction["statuscode"] = new OptionSetValue(2);
                        casemonitoraction.Id = Guid.NewGuid();
                        result.Entities.Add(casemonitoraction);
                    }
                }

                return result;
            };
            this.Service.ExecuteOrganizationRequest = processInstanceRequest =>
            {
                return new LocalTimeFromUtcTimeResponse();
            };
            JobEndDetailsPostOperationSync plugin = new JobEndDetailsPostOperationSync();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Job End Details Execute Update Test.
        /// </summary>
        [TestMethod]
        public void JobEndDetailsExecuteUpdateTest()
        {
            var jedInfo = new Entity("jarvis_jobenddetailses");
            jedInfo.Id = Guid.NewGuid();
            jedInfo["jarvis_incident"] = new EntityReference("incident", Guid.NewGuid());
            jedInfo.Attributes["jarvis_repairingdealerpassout"] = new EntityReference("jarvis_passout", Guid.NewGuid());
            jedInfo.Attributes["jarvis_actualcausefault"] = "test";
            jedInfo.Attributes["jarvis_mileage"] = Convert.ToDecimal(12);
            jedInfo["statuscode"] = new OptionSetValue(334030002);
            jedInfo.Attributes["jarvis_mileageunit"] = new EntityReference("jarvis_mileages", Guid.NewGuid());
            jedInfo.Attributes["jarvis_temporaryrepair"] = "test";
            jedInfo.Attributes["jarvis_translationstatusactualcausefault"] = new OptionSetValue(334030000);
            jedInfo.Attributes["jarvis_translationstatustemporaryrepair"] = new OptionSetValue(334030000);

            var postImg = new Entity("jarvis_jobenddetailses");
            postImg.Attributes["jarvis_incident"] = new EntityReference("incident", Guid.NewGuid());
            postImg.Attributes["jarvis_actualcausefault"] = "test";
            postImg.Attributes["jarvis_mileage"] = Convert.ToDecimal(12);
            postImg.Attributes["jarvis_mileageunit"] = new EntityReference("jarvis_mileages", Guid.NewGuid());
            postImg.Attributes["jarvis_temporaryrepair"] = "test";
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", jedInfo),
               };
            this.PluginExecutionContext.MessageNameGet = () => "UPDATE";
            this.PluginExecutionContext.StageGet = () => 40;

            //// Setting Post Entity Image.
            this.PluginExecutionContext.PostEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PostImage", postImg },
                };
            };

            // Setting service retrieve.
            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "jarvis_passout")
                {
                    Entity passout = new Entity("jarvis_passout");
                    passout.Id = Guid.NewGuid();
                    passout["jarvis_repairingdealer"] = new EntityReference("account", Guid.NewGuid());
                    return passout;
                }

                if (entityName == "incident")
                {
                    Entity inc = new Entity("incident");
                    inc.Id = guid;
                    inc["statuscode"] = new OptionSetValue(90);
                    inc["customerid"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_atc"] = DateTime.UtcNow;
                    inc["jarvis_onetimecustomercountry"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    inc["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    inc["jarvis_onetimecustomerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    return inc;
                }

                if (entityName == "account")
                {
                    Entity account = new Entity("account");
                    account.Id = guid;
                    account["jarvis_language"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    account["jarvis_address1_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    return account;
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

                if (entityName == "jarvis_jobenddetailses")
                {
                    Entity jedInfoRecord = new Entity("jarvis_jobenddetailses");
                    jedInfoRecord.Id = Guid.NewGuid();
                    jedInfoRecord["jarvis_incident"] = new EntityReference("incident", Guid.NewGuid());
                    jedInfoRecord.Attributes["jarvis_repairingdealerpassout"] = new EntityReference("jarvis_passout", Guid.NewGuid());
                    jedInfoRecord.Attributes["jarvis_actualcausefault"] = "test";
                    jedInfoRecord.Attributes["jarvis_mileage"] = "12";
                    jedInfoRecord.Attributes["jarvis_mileageunit"] = new EntityReference("jarvis_mileages", Guid.NewGuid());
                    jedInfoRecord.Attributes["jarvis_temporaryrepair"] = "test";
                    jedInfoRecord.Attributes["jarvis_translationstatusactualcausefault"] = new OptionSetValue(334030000);
                    jedInfoRecord.Attributes["jarvis_translationstatustemporaryrepair"] = new OptionSetValue(334030000);
                    return jedInfoRecord;
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
                        passoutResult.Id = Guid.NewGuid();
                        result.Entities.Add(passoutResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_casecontact'>"))
                    {
                        Entity caseContact = new Entity("jarvis_casecontact");
                        caseContact["jarvis_name"] = "testcontact";
                        caseContact["jarvis_preferredlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
                        caseContact["createdon"] = DateTime.UtcNow;
                        caseContact.Id = Guid.NewGuid();
                        result.Entities.Add(caseContact);
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

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_configurationjarvis'>"))
                    {
                        Entity configResult = new Entity("jarvis_configurationjarvis");
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationmonitoraction"] = true;
                        configResult["jarvis_automationcasestatuschange"] = true;
                        configResult["jarvis_automationreleasecase"] = new OptionSetValue(3);
                        configResult["createdon"] = DateTime.UtcNow;
                        configResult.Id = Guid.NewGuid();
                        result.Entities.Add(configResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_casemonitoraction'>"))
                    {
                        Entity casemonitoraction = new Entity("jarvis_casemonitoraction");
                        casemonitoraction["subject"] = true;
                        casemonitoraction["statuscode"] = new OptionSetValue(2);
                        casemonitoraction.Id = Guid.NewGuid();
                        result.Entities.Add(casemonitoraction);
                    }
                }

                return result;
            };
            this.Service.ExecuteOrganizationRequest = processInstanceRequest =>
            {
                return new LocalTimeFromUtcTimeResponse();
            };
            JobEndDetailsPostOperationSync plugin = new JobEndDetailsPostOperationSync();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Job End Details Execute Update Test.
        /// </summary>
        [TestMethod]
        public void JobEndDetailsExecuteUpdateStatus80Test()
        {
            var jedInfo = new Entity("jarvis_jobenddetailses");
            jedInfo.Id = Guid.NewGuid();
            jedInfo["jarvis_incident"] = new EntityReference("incident", Guid.NewGuid());
            jedInfo.Attributes["jarvis_repairingdealerpassout"] = new EntityReference("jarvis_passout", Guid.NewGuid());
            jedInfo.Attributes["jarvis_actualcausefault"] = "test";
            jedInfo.Attributes["jarvis_mileage"] = Convert.ToDecimal(12);
            jedInfo["statuscode"] = new OptionSetValue(334030002);
            jedInfo.Attributes["jarvis_mileageunit"] = new EntityReference("jarvis_mileages", Guid.NewGuid());
            jedInfo.Attributes["jarvis_temporaryrepair"] = "test";
            jedInfo.Attributes["jarvis_translationstatusactualcausefault"] = new OptionSetValue(334030000);
            jedInfo.Attributes["jarvis_translationstatustemporaryrepair"] = new OptionSetValue(334030000);

            var postImg = new Entity("jarvis_jobenddetailses");
            postImg.Attributes["jarvis_incident"] = new EntityReference("incident", Guid.NewGuid());
            postImg.Attributes["jarvis_actualcausefault"] = "test";
            postImg.Attributes["jarvis_mileage"] = Convert.ToDecimal(12);
            postImg.Attributes["jarvis_mileageunit"] = new EntityReference("jarvis_mileages", Guid.NewGuid());
            postImg.Attributes["jarvis_temporaryrepair"] = "test";
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", jedInfo),
               };
            this.PluginExecutionContext.MessageNameGet = () => "UPDATE";
            this.PluginExecutionContext.StageGet = () => 40;

            //// Setting Post Entity Image.
            this.PluginExecutionContext.PostEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PostImage", postImg },
                };
            };

            // Setting service retrieve.
            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "jarvis_passout")
                {
                    Entity passout = new Entity("jarvis_passout");
                    passout.Id = Guid.NewGuid();
                    passout["jarvis_repairingdealer"] = new EntityReference("account", Guid.NewGuid());
                    return passout;
                }

                if (entityName == "incident")
                {
                    Entity inc = new Entity("incident");
                    inc.Id = guid;
                    inc["statuscode"] = new OptionSetValue(80);
                    inc["customerid"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_homedealer"] = new EntityReference("account", Guid.NewGuid());
                    inc["jarvis_atc"] = DateTime.UtcNow;
                    inc["jarvis_onetimecustomercountry"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    inc["jarvis_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    inc["jarvis_onetimecustomerlanguage"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    return inc;
                }

                if (entityName == "account")
                {
                    Entity account = new Entity("account");
                    account.Id = guid;
                    account["jarvis_language"] = new EntityReference("jarvis_language", Guid.NewGuid());
                    account["jarvis_address1_country"] = new EntityReference("jarvis_country", Guid.NewGuid());
                    return account;
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

                if (entityName == "jarvis_jobenddetailses")
                {
                    Entity jedInfoRecord = new Entity("jarvis_jobenddetailses");
                    jedInfoRecord.Id = Guid.NewGuid();
                    jedInfoRecord["jarvis_incident"] = new EntityReference("incident", Guid.NewGuid());
                    jedInfoRecord.Attributes["jarvis_repairingdealerpassout"] = new EntityReference("jarvis_passout", Guid.NewGuid());
                    jedInfoRecord.Attributes["jarvis_actualcausefault"] = "test";
                    jedInfoRecord.Attributes["jarvis_mileage"] = "12";
                    jedInfoRecord.Attributes["jarvis_mileageunit"] = new EntityReference("jarvis_mileages", Guid.NewGuid());
                    jedInfoRecord.Attributes["jarvis_temporaryrepair"] = "test";
                    jedInfoRecord.Attributes["jarvis_translationstatusactualcausefault"] = new OptionSetValue(334030000);
                    jedInfoRecord.Attributes["jarvis_translationstatustemporaryrepair"] = new OptionSetValue(334030000);
                    return jedInfoRecord;
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
                        passoutResult.Id = Guid.NewGuid();
                        result.Entities.Add(passoutResult);
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

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_configurationjarvis'>"))
                    {
                        Entity configResult = new Entity("jarvis_configurationjarvis");
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationmonitoraction"] = true;
                        configResult["jarvis_automationcasestatuschange"] = true;
                        configResult["jarvis_automationreleasecase"] = new OptionSetValue(3);
                        configResult["createdon"] = DateTime.UtcNow;
                        configResult.Id = Guid.NewGuid();
                        result.Entities.Add(configResult);
                    }

                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_casemonitoraction'>"))
                    {
                        Entity casemonitoraction = new Entity("jarvis_casemonitoraction");
                        casemonitoraction["subject"] = true;
                        casemonitoraction["statuscode"] = new OptionSetValue(2);
                        casemonitoraction.Id = Guid.NewGuid();
                        result.Entities.Add(casemonitoraction);
                    }
                }

                return result;
            };
            this.Service.ExecuteOrganizationRequest = processInstanceRequest =>
            {
                return new LocalTimeFromUtcTimeResponse();
            };
            JobEndDetailsPostOperationSync plugin = new JobEndDetailsPostOperationSync();
            plugin.Execute(this.ServiceProvider);
        }
    }
}
