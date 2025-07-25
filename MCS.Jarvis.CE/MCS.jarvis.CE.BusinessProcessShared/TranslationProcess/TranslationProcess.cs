// <copyright file="TranslationProcess.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace MCS.jarvis.CE.BusinessProcessShared.TranslationProcess
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using MCS.Jarvis.CE.Plugins;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;
    using static MCS.Jarvis.CE.BusinessProcessesShared.Helpers.Constants;

    /// <summary>
    /// Translation Process.
    /// </summary>
    public class TranslationProcess
    {
        /// <summary>
        /// Standard Process.
        /// </summary>
        /// <param name="service">Org service.</param>
        /// <param name="tracing">tracing service.</param>
        /// <param name="incident">incident entity.</param>
        /// <param name="triggeredBy">triggered By.</param>
        public void CaseStandardProcess(IOrganizationService service, ITracingService tracing, Entity incident, Guid triggeredBy)
        {
            List<Guid> languageList = new List<Guid>();
            List<string> isoCodes = new List<string>();
            incident = service.Retrieve(incident.LogicalName, incident.Id, new ColumnSet("jarvis_homedealer", "customerid", "jarvis_callerlanguagesupported", "jarvis_driverlanguagesupported", "jarvis_country"));

#pragma warning disable SA1123 // Do not place regions within elements
            #region HD
            if (incident.Attributes.Contains("jarvis_homedealer") && incident.Attributes["jarvis_homedealer"] != null)
            {
                Entity language = this.BusinessPartner(service, (EntityReference)incident.Attributes["jarvis_homedealer"]);
                bool isExist = languageList.Contains(language.Id);
                if (language != null && !isExist)
                {
                    // getCaseTranslation
                    languageList.Add(language.Id);
                    EntityCollection languages = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getCaseTranslation, incident.Id, language.Id)));
                    if (languages.Entities.Count == 0)
                    {
                        Entity caseTranslation = new Entity("jarvis_casetranslation");
                        if (language.Attributes.Contains("jarvis_iso2languagecode6391") && language.Attributes["jarvis_iso2languagecode6391"] != null)
                        {
                            isoCodes.Add((string)language.Attributes["jarvis_iso2languagecode6391"]);
                            caseTranslation["jarvis_name"] = (string)language.Attributes["jarvis_iso2languagecode6391"];
                            caseTranslation["jarvis_language"] = language.ToEntityReference();
                            caseTranslation["jarvis_incident"] = incident.ToEntityReference();
                            service.Create(caseTranslation);
                        }
                    }
                }
            }
#pragma warning restore SA1123 // Do not place regions within elements
            #endregion

#pragma warning disable SA1123 // Do not place regions within elements
            #region Customer
            if (incident.Attributes.Contains("customerid") && incident.Attributes["customerid"] != null)
            {
                Entity language = this.BusinessPartner(service, (EntityReference)incident.Attributes["customerid"]);
                bool isExist = languageList.Contains(language.Id);
                if (language != null && !isExist)
                {
                    languageList.Add(language.Id);
                    EntityCollection languages = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getCaseTranslation, incident.Id, language.Id)));
                    if (languages.Entities.Count == 0)
                    {
                        Entity caseTranslation = new Entity("jarvis_casetranslation");
                        if (language.Attributes.Contains("jarvis_iso2languagecode6391") && language.Attributes["jarvis_iso2languagecode6391"] != null)
                        {
                            isoCodes.Add((string)language.Attributes["jarvis_iso2languagecode6391"]);
                            caseTranslation["jarvis_name"] = (string)language.Attributes["jarvis_iso2languagecode6391"];
                            caseTranslation["jarvis_language"] = language.ToEntityReference();
                            caseTranslation["jarvis_incident"] = incident.ToEntityReference();
                            service.Create(caseTranslation);
                        }
                    }
                }
            }
#pragma warning restore SA1123 // Do not place regions within elements
            #endregion

#pragma warning disable SA1123 // Do not place regions within elements
            #region Case-Contacts
            EntityCollection getCaseContacts = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getCaseContacts, incident.Id)));
#pragma warning restore SA1123 // Do not place regions within elements
            if (getCaseContacts != null && getCaseContacts.Entities.Count > 0)
            {
                foreach (var item in getCaseContacts.Entities)
                {
                    if (item.Attributes.Contains("jarvis_preferredlanguage") && item.Attributes["jarvis_preferredlanguage"] != null)
                    {
                        Entity language = this.GetLanguage(service, (EntityReference)item.Attributes["jarvis_preferredlanguage"]);
                        bool isExist = languageList.Contains(language.Id);
                        if (language != null && !isExist)
                        {
                            languageList.Add(language.Id);
                            EntityCollection languages = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getCaseTranslation, incident.Id, language.Id)));
                            if (languages.Entities.Count == 0)
                            {
                                Entity caseTranslation = new Entity("jarvis_casetranslation");
                                if (language.Attributes.Contains("jarvis_iso2languagecode6391") && language.Attributes["jarvis_iso2languagecode6391"] != null)
                                {
                                    isoCodes.Add((string)language.Attributes["jarvis_iso2languagecode6391"]);
                                    caseTranslation["jarvis_name"] = (string)language.Attributes["jarvis_iso2languagecode6391"];
                                    caseTranslation["jarvis_language"] = language.ToEntityReference();
                                    caseTranslation["jarvis_incident"] = incident.ToEntityReference();
                                    service.Create(caseTranslation);
                                }
                            }
                        }
                    }
                }
            }
            #endregion

#pragma warning disable SA1123 // Do not place regions within elements
            #region Country
            if (incident.Attributes.Contains("jarvis_country") && incident.Attributes["jarvis_country"] != null)
            {
                EntityCollection countryLanguages = this.GetCountryLanguages(service, (EntityReference)incident.Attributes["jarvis_country"]);
                if (countryLanguages.Entities.Count > 0)
                {
                    foreach (var item in countryLanguages.Entities)
                    {
                        bool isExist = languageList.Contains(item.Id);
                        if (!isExist)
                        {
                            languageList.Add(item.Id);
                            EntityCollection languages = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getCaseTranslation, incident.Id, item.Id)));
                            if (languages.Entities.Count == 0)
                            {
                                Entity caseTranslation = new Entity("jarvis_casetranslation");
                                if (item.Attributes.Contains("jarvis_iso2languagecode6391") && item.Attributes["jarvis_iso2languagecode6391"] != null)
                                {
                                    isoCodes.Add((string)item.Attributes["jarvis_iso2languagecode6391"]);
                                    caseTranslation["jarvis_name"] = (string)item.Attributes["jarvis_iso2languagecode6391"];
                                    caseTranslation["jarvis_language"] = item.ToEntityReference();
                                    caseTranslation["jarvis_incident"] = incident.ToEntityReference();
                                    service.Create(caseTranslation);
                                }
                            }
                        }
                    }
                }
            }
#pragma warning restore SA1123 // Do not place regions within elements
            #endregion

#pragma warning disable SA1123 // Do not place regions within elements
            #region PassOut
            EntityCollection passOut = this.GetPassOuts(service, incident);
#pragma warning restore SA1123 // Do not place regions within elements
            if (passOut != null && passOut.Entities.Count > 0)
            {
                foreach (var item in passOut.Entities)
                {
                    if (item.Attributes.Contains("jarvis_repairingdealer") && item.Attributes["jarvis_repairingdealer"] != null)
                    {
                        Entity language = this.BusinessPartner(service, (EntityReference)item.Attributes["jarvis_repairingdealer"]);
                        bool isExist = languageList.Contains(language.Id);
                        if (language != null && !isExist)
                        {
                            languageList.Add(language.Id);
                            EntityCollection languages = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getCaseTranslation, incident.Id, language.Id)));
                            if (languages.Entities.Count == 0)
                            {
                                Entity caseTranslation = new Entity("jarvis_casetranslation");
                                if (language.Attributes.Contains("jarvis_iso2languagecode6391") && language.Attributes["jarvis_iso2languagecode6391"] != null)
                                {
                                    isoCodes.Add((string)language.Attributes["jarvis_iso2languagecode6391"]);
                                    caseTranslation["jarvis_name"] = (string)language.Attributes["jarvis_iso2languagecode6391"];
                                    caseTranslation["jarvis_language"] = language.ToEntityReference();
                                    caseTranslation["jarvis_incident"] = incident.ToEntityReference();
                                    service.Create(caseTranslation);
                                }
                            }
                        }
                    }
                }
            }
            #endregion

#pragma warning disable SA1123 // Do not place regions within elements
            #region English
            bool isAvailable = isoCodes.Contains("EN");
#pragma warning restore SA1123 // Do not place regions within elements
            if (!isAvailable)
            {
                EntityCollection getEnglish = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getEnglishSupportedLanguage)));
                if (getEnglish.Entities.Count > 0)
                {
                    bool isExist = languageList.Contains(getEnglish.Entities[0].Id);
                    if (!isExist)
                    {
                        EntityCollection languages = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getCaseTranslation, incident.Id, getEnglish.Entities[0].Id)));
                        if (languages.Entities.Count == 0)
                        {
                            isoCodes.Add("EN");
                            Entity caseTranslation = new Entity("jarvis_casetranslation");
                            caseTranslation["jarvis_name"] = "EN";
                            caseTranslation["jarvis_language"] = getEnglish.Entities[0].ToEntityReference();
                            caseTranslation["jarvis_incident"] = incident.ToEntityReference();
                            service.Create(caseTranslation);
                        }
                    }
                }
            }
            #endregion
        }

        /// <summary>
        /// Case Query Standard Process.
        /// </summary>
        /// <param name="service">Org service.</param>
        /// <param name="tracing">tracing service.</param>
        /// <param name="incident">incident entity.</param>
        /// <param name="parentCase">parent Case.</param>
        /// <param name="triggeredBy">triggered By.</param>
        public void CaseQueryStandardProcess(IOrganizationService service, ITracingService tracing, Entity incident, Entity parentCase, Guid triggeredBy)
        {
            List<Guid> languageList = new List<Guid>();
            List<string> isoCodes = new List<string>();
            incident = service.Retrieve(incident.LogicalName, incident.Id, new ColumnSet("jarvis_homedealer", "customerid", "jarvis_callerlanguagesupported", "jarvis_driverlanguagesupported", "jarvis_country"));

#pragma warning disable SA1123 // Do not place regions within elements
            #region HD
            if (incident.Attributes.Contains("jarvis_homedealer") && incident.Attributes["jarvis_homedealer"] != null)
            {
                Entity language = this.BusinessPartner(service, (EntityReference)incident.Attributes["jarvis_homedealer"]);
                bool isExist = languageList.Contains(language.Id);
                if (language != null && !isExist)
                {
                    // getCaseTranslation
                    languageList.Add(language.Id);
                    EntityCollection languages = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getQueryTranslations, parentCase.Id, language.Id)));
                    if (languages.Entities.Count == 0)
                    {
                        Entity caseTranslation = new Entity("jarvis_querycasetranslation");
                        if (language.Attributes.Contains("jarvis_iso2languagecode6391") && language.Attributes["jarvis_iso2languagecode6391"] != null)
                        {
                            isoCodes.Add((string)language.Attributes["jarvis_iso2languagecode6391"]);
                            caseTranslation["jarvis_name"] = (string)language.Attributes["jarvis_iso2languagecode6391"];
                            caseTranslation["jarvis_language"] = language.ToEntityReference();
                            caseTranslation["jarvis_incident"] = parentCase.ToEntityReference();
                            service.Create(caseTranslation);
                        }
                    }
                }
            }
#pragma warning restore SA1123 // Do not place regions within elements
            #endregion

#pragma warning disable SA1123 // Do not place regions within elements
            #region Customer
            if (incident.Attributes.Contains("customerid") && incident.Attributes["customerid"] != null)
            {
                Entity language = this.BusinessPartner(service, (EntityReference)incident.Attributes["customerid"]);
                bool isExist = languageList.Contains(language.Id);
                if (language != null && !isExist)
                {
                    languageList.Add(language.Id);
                    EntityCollection languages = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getQueryTranslations, parentCase.Id, language.Id)));
                    if (languages.Entities.Count == 0)
                    {
                        Entity caseTranslation = new Entity("jarvis_querycasetranslation");
                        if (language.Attributes.Contains("jarvis_iso2languagecode6391") && language.Attributes["jarvis_iso2languagecode6391"] != null)
                        {
                            isoCodes.Add((string)language.Attributes["jarvis_iso2languagecode6391"]);
                            caseTranslation["jarvis_name"] = (string)language.Attributes["jarvis_iso2languagecode6391"];
                            caseTranslation["jarvis_language"] = language.ToEntityReference();
                            caseTranslation["jarvis_incident"] = parentCase.ToEntityReference();
                            service.Create(caseTranslation);
                        }
                    }
                }
            }
#pragma warning restore SA1123 // Do not place regions within elements
            #endregion

#pragma warning disable SA1123 // Do not place regions within elements
            #region PassOut
            EntityCollection passOut = this.GetPassOuts(service, incident);
#pragma warning restore SA1123 // Do not place regions within elements
            if (passOut != null && passOut.Entities.Count > 0)
            {
                foreach (var item in passOut.Entities)
                {
                    if (item.Attributes.Contains("jarvis_repairingdealer") && item.Attributes["jarvis_repairingdealer"] != null)
                    {
                        Entity language = this.BusinessPartner(service, (EntityReference)item.Attributes["jarvis_repairingdealer"]);
                        bool isExist = languageList.Contains(language.Id);
                        if (language != null && !isExist)
                        {
                            languageList.Add(language.Id);
                            EntityCollection languages = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getQueryTranslations, parentCase.Id, language.Id)));
                            if (languages.Entities.Count == 0)
                            {
                                Entity caseTranslation = new Entity("jarvis_querycasetranslation");
                                if (language.Attributes.Contains("jarvis_iso2languagecode6391") && language.Attributes["jarvis_iso2languagecode6391"] != null)
                                {
                                    isoCodes.Add((string)language.Attributes["jarvis_iso2languagecode6391"]);
                                    caseTranslation["jarvis_name"] = (string)language.Attributes["jarvis_iso2languagecode6391"];
                                    caseTranslation["jarvis_language"] = language.ToEntityReference();
                                    caseTranslation["jarvis_incident"] = parentCase.ToEntityReference();
                                    service.Create(caseTranslation);
                                }
                            }
                        }
                    }
                }
            }
            #endregion

#pragma warning disable SA1123 // Do not place regions within elements
            #region English
            bool isAvailable = isoCodes.Contains("EN");
#pragma warning restore SA1123 // Do not place regions within elements
            if (!isAvailable)
            {
                EntityCollection getEnglish = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getEnglishSupportedLanguage)));
                if (getEnglish.Entities.Count > 0)
                {
                    bool isExist = languageList.Contains(getEnglish.Entities[0].Id);
                    if (!isExist)
                    {
                        EntityCollection languages = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getQueryTranslations, parentCase.Id, getEnglish.Entities[0].Id)));
                        if (languages.Entities.Count == 0)
                        {
                            isoCodes.Add("EN");
                            Entity caseTranslation = new Entity("jarvis_querycasetranslation");
                            caseTranslation["jarvis_name"] = "EN";
                            caseTranslation["jarvis_language"] = getEnglish.Entities[0].ToEntityReference();
                            caseTranslation["jarvis_incident"] = parentCase.ToEntityReference();
                            service.Create(caseTranslation);
                        }
                    }
                }
            }
            #endregion
        }

        /// <summary>
        /// GOP Standard Process.
        /// </summary>
        /// <param name="service">Organization service.</param>
        /// <param name="tracing">tracing service.</param>
        /// <param name="incident">incident entity.</param>
        /// <param name="triggeredBy">triggered By.</param>
        /// <param name="gop">gop entity.</param>
        public void GOPStandardProcess(IOrganizationService service, ITracingService tracing, Entity incident, Guid triggeredBy, Entity gop)
        {
            List<Guid> languageList = new List<Guid>();
            List<string> isoCodes = new List<string>();
            incident = service.Retrieve(incident.LogicalName, incident.Id, new ColumnSet("jarvis_homedealer", "customerid", "jarvis_callerlanguagesupported", "jarvis_driverlanguagesupported", "jarvis_country"));

#pragma warning disable SA1123 // Do not place regions within elements
            #region HD
            if (incident.Attributes.Contains("jarvis_homedealer") && incident.Attributes["jarvis_homedealer"] != null)
            {
                Entity language = this.BusinessPartner(service, (EntityReference)incident.Attributes["jarvis_homedealer"]);
                bool isExist = languageList.Contains(language.Id);
                if (language != null && !isExist)
                {
                    languageList.Add(language.Id);
                    EntityCollection languages = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getGOPTranslation, incident.Id, gop.Id, language.Id)));
                    if (languages.Entities.Count == 0)
                    {
                        Entity caseTranslation = new Entity("jarvis_goptranslation");
                        if (language.Attributes.Contains("jarvis_iso2languagecode6391") && language.Attributes["jarvis_iso2languagecode6391"] != null)
                        {
                            isoCodes.Add((string)language.Attributes["jarvis_iso2languagecode6391"]);
                            caseTranslation["jarvis_name"] = (string)language.Attributes["jarvis_iso2languagecode6391"];
                            caseTranslation["jarvis_language"] = language.ToEntityReference();
                            caseTranslation["jarvis_case"] = incident.ToEntityReference();
                            caseTranslation["jarvis_gop"] = gop.ToEntityReference();
                            service.Create(caseTranslation);
                        }
                    }
                }
            }
#pragma warning restore SA1123 // Do not place regions within elements
            #endregion

#pragma warning disable SA1123 // Do not place regions within elements
            #region Customer
            if (incident.Attributes.Contains("customerid") && incident.Attributes["customerid"] != null)
            {
                Entity language = this.BusinessPartner(service, (EntityReference)incident.Attributes["customerid"]);
                bool isExist = languageList.Contains(language.Id);
                if (language != null && !isExist)
                {
                    languageList.Add(language.Id);
                    EntityCollection languages = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getGOPTranslation, incident.Id, gop.Id, language.Id)));
                    if (languages.Entities.Count == 0)
                    {
                        Entity caseTranslation = new Entity("jarvis_goptranslation");
                        if (language.Attributes.Contains("jarvis_iso2languagecode6391") && language.Attributes["jarvis_iso2languagecode6391"] != null)
                        {
                            isoCodes.Add((string)language.Attributes["jarvis_iso2languagecode6391"]);
                            caseTranslation["jarvis_name"] = (string)language.Attributes["jarvis_iso2languagecode6391"];
                            caseTranslation["jarvis_language"] = language.ToEntityReference();
                            caseTranslation["jarvis_case"] = incident.ToEntityReference();
                            caseTranslation["jarvis_gop"] = gop.ToEntityReference();
                            service.Create(caseTranslation);
                        }
                    }
                }
            }
#pragma warning restore SA1123 // Do not place regions within elements
            #endregion

#pragma warning disable SA1123 // Do not place regions within elements
            #region Case-Contacts
            EntityCollection getCaseContacts = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getCaseContacts, incident.Id)));
#pragma warning restore SA1123 // Do not place regions within elements
            if (getCaseContacts != null && getCaseContacts.Entities.Count > 0)
            {
                foreach (var item in getCaseContacts.Entities)
                {
                    if (item.Attributes.Contains("jarvis_preferredlanguage") && item.Attributes["jarvis_preferredlanguage"] != null)
                    {
                        Entity language = this.GetLanguage(service, (EntityReference)item.Attributes["jarvis_preferredlanguage"]);
                        bool isExist = languageList.Contains(language.Id);
                        if (language != null && !isExist)
                        {
                            languageList.Add(language.Id);
                            EntityCollection languages = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getGOPTranslation, incident.Id, gop.Id, language.Id)));
                            if (languages.Entities.Count == 0)
                            {
                                Entity caseTranslation = new Entity("jarvis_goptranslation");
                                if (language.Attributes.Contains("jarvis_iso2languagecode6391") && language.Attributes["jarvis_iso2languagecode6391"] != null)
                                {
                                    isoCodes.Add((string)language.Attributes["jarvis_iso2languagecode6391"]);
                                    caseTranslation["jarvis_name"] = (string)language.Attributes["jarvis_iso2languagecode6391"];
                                    caseTranslation["jarvis_language"] = language.ToEntityReference();
                                    caseTranslation["jarvis_case"] = incident.ToEntityReference();
                                    caseTranslation["jarvis_gop"] = gop.ToEntityReference();
                                    service.Create(caseTranslation);
                                }
                            }
                        }
                    }
                }
            }
            #endregion

#pragma warning disable SA1123 // Do not place regions within elements
            #region GOP-Contacts
            EntityCollection getGOPContacts = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getGOPCaseContacts, gop.Id)));
#pragma warning restore SA1123 // Do not place regions within elements
            if (getGOPContacts != null && getGOPContacts.Entities.Count > 0)
            {
                foreach (var item in getGOPContacts.Entities)
                {
                    if (item.Attributes.Contains("jarvis_preferredlanguage") && item.Attributes["jarvis_preferredlanguage"] != null)
                    {
                        Entity language = this.GetLanguage(service, (EntityReference)item.Attributes["jarvis_preferredlanguage"]);
                        bool isExist = languageList.Contains(language.Id);
                        if (language != null && !isExist)
                        {
                            languageList.Add(language.Id);
                            EntityCollection languages = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getGOPTranslation, incident.Id, gop.Id, language.Id)));
                            if (languages.Entities.Count == 0)
                            {
                                Entity caseTranslation = new Entity("jarvis_goptranslation");
                                if (language.Attributes.Contains("jarvis_iso2languagecode6391") && language.Attributes["jarvis_iso2languagecode6391"] != null)
                                {
                                    isoCodes.Add((string)language.Attributes["jarvis_iso2languagecode6391"]);
                                    caseTranslation["jarvis_name"] = (string)language.Attributes["jarvis_iso2languagecode6391"];
                                    caseTranslation["jarvis_language"] = language.ToEntityReference();
                                    caseTranslation["jarvis_case"] = incident.ToEntityReference();
                                    caseTranslation["jarvis_gop"] = gop.ToEntityReference();
                                    service.Create(caseTranslation);
                                }
                            }
                        }
                    }
                }
            }
            #endregion

#pragma warning disable SA1123 // Do not place regions within elements
            #region Country
            if (incident.Attributes.Contains("jarvis_country") && incident.Attributes["jarvis_country"] != null)
            {
                EntityCollection countryLanguages = this.GetCountryLanguages(service, (EntityReference)incident.Attributes["jarvis_country"]);
                if (countryLanguages.Entities.Count > 0)
                {
                    foreach (var item in countryLanguages.Entities)
                    {
                        bool isExist = languageList.Contains(item.Id);
                        if (!isExist)
                        {
                            languageList.Add(item.Id);
                            EntityCollection languages = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getGOPTranslation, incident.Id, gop.Id, item.Id)));
                            if (languages.Entities.Count == 0)
                            {
                                Entity caseTranslation = new Entity("jarvis_goptranslation");
                                if (item.Attributes.Contains("jarvis_iso2languagecode6391") && item.Attributes["jarvis_iso2languagecode6391"] != null)
                                {
                                    isoCodes.Add((string)item.Attributes["jarvis_iso2languagecode6391"]);
                                    caseTranslation["jarvis_name"] = (string)item.Attributes["jarvis_iso2languagecode6391"];
                                    caseTranslation["jarvis_language"] = item.ToEntityReference();
                                    caseTranslation["jarvis_case"] = incident.ToEntityReference();
                                    caseTranslation["jarvis_gop"] = gop.ToEntityReference();
                                    service.Create(caseTranslation);
                                }
                            }
                        }
                    }
                }
            }
#pragma warning restore SA1123 // Do not place regions within elements
            #endregion

            #region GOP

            if (gop.Attributes.Contains("jarvis_dealer") && gop.Attributes["jarvis_dealer"] != null)
            {
                Entity language = this.BusinessPartner(service, (EntityReference)gop.Attributes["jarvis_dealer"]);
                bool isExist = languageList.Contains(language.Id);
                if (language != null && !isExist)
                {
                    languageList.Add(language.Id);
                    EntityCollection languages = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getGOPTranslation, incident.Id, gop.Id, language.Id)));
                    if (languages.Entities.Count == 0)
                    {
                        Entity caseTranslation = new Entity("jarvis_goptranslation");
                        if (language.Attributes.Contains("jarvis_iso2languagecode6391") && language.Attributes["jarvis_iso2languagecode6391"] != null)
                        {
                            isoCodes.Add((string)language.Attributes["jarvis_iso2languagecode6391"]);
                            caseTranslation["jarvis_name"] = (string)language.Attributes["jarvis_iso2languagecode6391"];
                            caseTranslation["jarvis_language"] = language.ToEntityReference();
                            caseTranslation["jarvis_case"] = incident.ToEntityReference();
                            caseTranslation["jarvis_gop"] = gop.ToEntityReference();
                            service.Create(caseTranslation);
                        }
                    }
                }
            }


            #endregion

#pragma warning disable SA1123 // Do not place regions within elements
            #region PassOut
            EntityCollection passOut = this.GetPassOuts(service, incident);
#pragma warning restore SA1123 // Do not place regions within elements
            if (passOut != null && passOut.Entities.Count > 0)
            {
                foreach (var item in passOut.Entities)
                {
                    if (item.Attributes.Contains("jarvis_repairingdealer") && item.Attributes["jarvis_repairingdealer"] != null)
                    {
                        Entity language = this.BusinessPartner(service, (EntityReference)item.Attributes["jarvis_repairingdealer"]);
                        bool isExist = languageList.Contains(language.Id);
                        if (language != null && !isExist)
                        {
                            languageList.Add(language.Id);
                            EntityCollection languages = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getGOPTranslation, incident.Id, gop.Id, language.Id)));
                            if (languages.Entities.Count == 0)
                            {
                                Entity caseTranslation = new Entity("jarvis_goptranslation");
                                if (language.Attributes.Contains("jarvis_iso2languagecode6391") && language.Attributes["jarvis_iso2languagecode6391"] != null)
                                {
                                    isoCodes.Add((string)language.Attributes["jarvis_iso2languagecode6391"]);
                                    caseTranslation["jarvis_name"] = (string)language.Attributes["jarvis_iso2languagecode6391"];
                                    caseTranslation["jarvis_language"] = language.ToEntityReference();
                                    caseTranslation["jarvis_case"] = incident.ToEntityReference();
                                    caseTranslation["jarvis_gop"] = gop.ToEntityReference();
                                    service.Create(caseTranslation);
                                }
                            }
                        }
                    }
                }
            }
            #endregion

#pragma warning disable SA1123 // Do not place regions within elements
            #region English
            bool isAvailable = isoCodes.Contains("EN");
#pragma warning restore SA1123 // Do not place regions within elements
            if (!isAvailable)
            {
                EntityCollection getEnglish = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getEnglishSupportedLanguage)));
                if (getEnglish.Entities.Count > 0)
                {
                    bool isExist = languageList.Contains(getEnglish.Entities[0].Id);
                    if (!isExist)
                    {
                        EntityCollection languages = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getGOPTranslation, incident.Id, gop.Id, getEnglish.Entities[0].Id)));
                        if (languages.Entities.Count == 0)
                        {
                            isoCodes.Add("EN");
                            Entity caseTranslation = new Entity("jarvis_goptranslation");
                            caseTranslation["jarvis_name"] = "EN";
                            caseTranslation["jarvis_language"] = getEnglish.Entities[0].ToEntityReference();
                            caseTranslation["jarvis_case"] = incident.ToEntityReference();
                            caseTranslation["jarvis_gop"] = gop.ToEntityReference();
                            service.Create(caseTranslation);
                        }
                    }
                }
            }
            #endregion
        }

        /// <summary>
        /// Pass Out Standard Process.
        /// </summary>
        /// <param name="service">Org service.</param>
        /// <param name="tracing">tracing Service.</param>
        /// <param name="incident">incident entity.</param>
        /// <param name="triggeredBy">triggered By.</param>
        /// <param name="gop">gop entity.</param>
        public void PassOutStandardProcess(IOrganizationService service, ITracingService tracing, Entity incident, Guid triggeredBy, Entity gop)
        {
            List<Guid> languageList = new List<Guid>();
            List<string> isoCodes = new List<string>();
            incident = service.Retrieve(incident.LogicalName, incident.Id, new ColumnSet("jarvis_homedealer", "customerid", "jarvis_callerlanguagesupported", "jarvis_driverlanguagesupported", "jarvis_country"));

#pragma warning disable SA1123 // Do not place regions within elements
            #region HD
            if (incident.Attributes.Contains("jarvis_homedealer") && incident.Attributes["jarvis_homedealer"] != null)
            {
                Entity language = this.BusinessPartner(service, (EntityReference)incident.Attributes["jarvis_homedealer"]);
                bool isExist = languageList.Contains(language.Id);
                if (language != null && !isExist)
                {
                    languageList.Add(language.Id);
                    EntityCollection languages = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getPassOutTranslation, incident.Id, gop.Id, language.Id)));
                    if (languages.Entities.Count == 0)
                    {
                        Entity caseTranslation = new Entity("jarvis_passouttranslation");
                        if (language.Attributes.Contains("jarvis_iso2languagecode6391") && language.Attributes["jarvis_iso2languagecode6391"] != null)
                        {
                            isoCodes.Add((string)language.Attributes["jarvis_iso2languagecode6391"]);
                            caseTranslation["jarvis_name"] = (string)language.Attributes["jarvis_iso2languagecode6391"];
                            caseTranslation["jarvis_language"] = language.ToEntityReference();
                            caseTranslation["jarvis_case"] = incident.ToEntityReference();
                            caseTranslation["jarvis_passout"] = gop.ToEntityReference();
                            service.Create(caseTranslation);
                        }
                    }
                }
            }
#pragma warning restore SA1123 // Do not place regions within elements
            #endregion

#pragma warning disable SA1123 // Do not place regions within elements
            #region Customer
            if (incident.Attributes.Contains("customerid") && incident.Attributes["customerid"] != null)
            {
                Entity language = this.BusinessPartner(service, (EntityReference)incident.Attributes["customerid"]);
                bool isExist = languageList.Contains(language.Id);
                if (language != null && !isExist)
                {
                    languageList.Add(language.Id);
                    EntityCollection languages = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getPassOutTranslation, incident.Id, gop.Id, language.Id)));
                    if (languages.Entities.Count == 0)
                    {
                        Entity caseTranslation = new Entity("jarvis_passouttranslation");
                        if (language.Attributes.Contains("jarvis_iso2languagecode6391") && language.Attributes["jarvis_iso2languagecode6391"] != null)
                        {
                            isoCodes.Add((string)language.Attributes["jarvis_iso2languagecode6391"]);
                            caseTranslation["jarvis_name"] = (string)language.Attributes["jarvis_iso2languagecode6391"];
                            caseTranslation["jarvis_language"] = language.ToEntityReference();
                            caseTranslation["jarvis_case"] = incident.ToEntityReference();
                            caseTranslation["jarvis_passout"] = gop.ToEntityReference();
                            service.Create(caseTranslation);
                        }
                    }
                }
            }
#pragma warning restore SA1123 // Do not place regions within elements
            #endregion

#pragma warning disable SA1123 // Do not place regions within elements
            #region Case-Contacts
            EntityCollection getCaseContacts = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getCaseContacts, incident.Id)));
#pragma warning restore SA1123 // Do not place regions within elements
            if (getCaseContacts != null && getCaseContacts.Entities.Count > 0)
            {
                foreach (var item in getCaseContacts.Entities)
                {
                    if (item.Attributes.Contains("jarvis_preferredlanguage") && item.Attributes["jarvis_preferredlanguage"] != null)
                    {
                        Entity language = this.GetLanguage(service, (EntityReference)item.Attributes["jarvis_preferredlanguage"]);
                        bool isExist = languageList.Contains(language.Id);
                        if (language != null && !isExist)
                        {
                            languageList.Add(language.Id);
                            EntityCollection languages = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getPassOutTranslation, incident.Id, gop.Id, language.Id)));
                            if (languages.Entities.Count == 0)
                            {
                                Entity caseTranslation = new Entity("jarvis_passouttranslation");
                                if (language.Attributes.Contains("jarvis_iso2languagecode6391") && language.Attributes["jarvis_iso2languagecode6391"] != null)
                                {
                                    isoCodes.Add((string)language.Attributes["jarvis_iso2languagecode6391"]);
                                    caseTranslation["jarvis_name"] = (string)language.Attributes["jarvis_iso2languagecode6391"];
                                    caseTranslation["jarvis_language"] = language.ToEntityReference();
                                    caseTranslation["jarvis_case"] = incident.ToEntityReference();
                                    caseTranslation["jarvis_passout"] = gop.ToEntityReference();
                                    service.Create(caseTranslation);
                                }
                            }
                        }
                    }
                }
            }
            #endregion

#pragma warning disable SA1123 // Do not place regions within elements
            #region PassOut-Contacts
            EntityCollection getGOPContacts = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getPassOutCaseContacts, gop.Id)));
#pragma warning restore SA1123 // Do not place regions within elements
            if (getGOPContacts != null && getGOPContacts.Entities.Count > 0)
            {
                foreach (var item in getGOPContacts.Entities)
                {
                    if (item.Attributes.Contains("jarvis_preferredlanguage") && item.Attributes["jarvis_preferredlanguage"] != null)
                    {
                        Entity language = this.GetLanguage(service, (EntityReference)item.Attributes["jarvis_preferredlanguage"]);
                        bool isExist = languageList.Contains(language.Id);
                        if (language != null && !isExist)
                        {
                            languageList.Add(language.Id);
                            EntityCollection languages = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getPassOutTranslation, incident.Id, gop.Id, language.Id)));
                            if (languages.Entities.Count == 0)
                            {
                                Entity caseTranslation = new Entity("jarvis_passouttranslation");
                                if (language.Attributes.Contains("jarvis_iso2languagecode6391") && language.Attributes["jarvis_iso2languagecode6391"] != null)
                                {
                                    isoCodes.Add((string)language.Attributes["jarvis_iso2languagecode6391"]);
                                    caseTranslation["jarvis_name"] = (string)language.Attributes["jarvis_iso2languagecode6391"];
                                    caseTranslation["jarvis_language"] = language.ToEntityReference();
                                    caseTranslation["jarvis_case"] = incident.ToEntityReference();
                                    caseTranslation["jarvis_passout"] = gop.ToEntityReference();
                                    service.Create(caseTranslation);
                                }
                            }
                        }
                    }
                }
            }
            #endregion

#pragma warning disable SA1123 // Do not place regions within elements
            #region Country
            if (incident.Attributes.Contains("jarvis_country") && incident.Attributes["jarvis_country"] != null)
            {
                EntityCollection countryLanguages = this.GetCountryLanguages(service, (EntityReference)incident.Attributes["jarvis_country"]);
                if (countryLanguages.Entities.Count > 0)
                {
                    foreach (var item in countryLanguages.Entities)
                    {
                        bool isExist = languageList.Contains(item.Id);
                        if (!isExist)
                        {
                            languageList.Add(item.Id);
                            EntityCollection languages = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getPassOutTranslation, incident.Id, gop.Id, item.Id)));
                            if (languages.Entities.Count == 0)
                            {
                                Entity caseTranslation = new Entity("jarvis_passouttranslation");
                                if (item.Attributes.Contains("jarvis_iso2languagecode6391") && item.Attributes["jarvis_iso2languagecode6391"] != null)
                                {
                                    isoCodes.Add((string)item.Attributes["jarvis_iso2languagecode6391"]);
                                    caseTranslation["jarvis_name"] = (string)item.Attributes["jarvis_iso2languagecode6391"];
                                    caseTranslation["jarvis_language"] = item.ToEntityReference();
                                    caseTranslation["jarvis_case"] = incident.ToEntityReference();
                                    caseTranslation["jarvis_passout"] = gop.ToEntityReference();
                                    service.Create(caseTranslation);
                                }
                            }
                        }
                    }
                }
            }
#pragma warning restore SA1123 // Do not place regions within elements
            #endregion

#pragma warning disable SA1123 // Do not place regions within elements
            #region PassOut
            EntityCollection passOut = this.GetPassOuts(service, incident);
#pragma warning restore SA1123 // Do not place regions within elements
            if (passOut != null && passOut.Entities.Count > 0)
            {
                foreach (var item in passOut.Entities)
                {
                    if (item.Attributes.Contains("jarvis_repairingdealer") && item.Attributes["jarvis_repairingdealer"] != null)
                    {
                        Entity language = this.BusinessPartner(service, (EntityReference)item.Attributes["jarvis_repairingdealer"]);
                        bool isExist = languageList.Contains(language.Id);
                        if (language != null && !isExist)
                        {
                            languageList.Add(language.Id);
                            EntityCollection languages = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getPassOutTranslation, incident.Id, gop.Id, language.Id)));
                            if (languages.Entities.Count == 0)
                            {
                                Entity caseTranslation = new Entity("jarvis_passouttranslation");
                                if (language.Attributes.Contains("jarvis_iso2languagecode6391") && language.Attributes["jarvis_iso2languagecode6391"] != null)
                                {
                                    isoCodes.Add((string)language.Attributes["jarvis_iso2languagecode6391"]);
                                    caseTranslation["jarvis_name"] = (string)language.Attributes["jarvis_iso2languagecode6391"];
                                    caseTranslation["jarvis_language"] = language.ToEntityReference();
                                    caseTranslation["jarvis_case"] = incident.ToEntityReference();
                                    caseTranslation["jarvis_passout"] = gop.ToEntityReference();
                                    service.Create(caseTranslation);
                                }
                            }
                        }
                    }
                }
            }
            #endregion

#pragma warning disable SA1123 // Do not place regions within elements
            #region English
            bool isAvailable = isoCodes.Contains("EN");
#pragma warning restore SA1123 // Do not place regions within elements
            if (!isAvailable)
            {
                EntityCollection getEnglish = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getEnglishSupportedLanguage)));
                if (getEnglish.Entities.Count > 0)
                {
                    bool isExist = languageList.Contains(getEnglish.Entities[0].Id);
                    if (!isExist)
                    {
                        EntityCollection languages = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getPassOutTranslation, incident.Id, gop.Id, getEnglish.Entities[0].Id)));
                        if (languages.Entities.Count == 0)
                        {
                            isoCodes.Add("EN");
                            Entity caseTranslation = new Entity("jarvis_passouttranslation");
                            caseTranslation["jarvis_name"] = "EN";
                            caseTranslation["jarvis_language"] = getEnglish.Entities[0].ToEntityReference();
                            caseTranslation["jarvis_case"] = incident.ToEntityReference();
                            caseTranslation["jarvis_passout"] = gop.ToEntityReference();
                            service.Create(caseTranslation);
                        }
                    }
                }
            }
            #endregion
        }

        /// <summary>
        /// JED Standard Process.
        /// </summary>
        /// <param name="service">Org service.</param>
        /// <param name="tracing">tracing service.</param>
        /// <param name="incident">incident entity.</param>
        /// <param name="triggeredBy">triggered By.</param>
        /// <param name="gop">gop entity.</param>
        public void JEDStandardProcess(IOrganizationService service, ITracingService tracing, Entity incident, Guid triggeredBy, Entity gop)
        {
            List<Guid> languageList = new List<Guid>();
            List<string> isoCodes = new List<string>();
            incident = service.Retrieve(incident.LogicalName, incident.Id, new ColumnSet("jarvis_homedealer", "customerid", "jarvis_callerlanguagesupported", "jarvis_driverlanguagesupported", "jarvis_country"));

#pragma warning disable SA1123 // Do not place regions within elements
            #region HD
            if (incident.Attributes.Contains("jarvis_homedealer") && incident.Attributes["jarvis_homedealer"] != null)
            {
                Entity language = this.BusinessPartner(service, (EntityReference)incident.Attributes["jarvis_homedealer"]);
                bool isExist = languageList.Contains(language.Id);
                if (language != null && !isExist)
                {
                    languageList.Add(language.Id);
                    EntityCollection languages = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getJEDTranslations, incident.Id, gop.Id, language.Id)));
                    if (languages.Entities.Count == 0)
                    {
                        Entity caseTranslation = new Entity("jarvis_jobenddetailstranslation");
                        if (language.Attributes.Contains("jarvis_iso2languagecode6391") && language.Attributes["jarvis_iso2languagecode6391"] != null)
                        {
                            isoCodes.Add((string)language.Attributes["jarvis_iso2languagecode6391"]);
                            caseTranslation["jarvis_name"] = (string)language.Attributes["jarvis_iso2languagecode6391"];
                            caseTranslation["jarvis_language"] = language.ToEntityReference();
                            caseTranslation["jarvis_case"] = incident.ToEntityReference();
                            caseTranslation["jarvis_jobenddetails"] = gop.ToEntityReference();
                            service.Create(caseTranslation);
                        }
                    }
                }
            }
#pragma warning restore SA1123 // Do not place regions within elements
            #endregion

#pragma warning disable SA1123 // Do not place regions within elements
            #region Customer
            if (incident.Attributes.Contains("customerid") && incident.Attributes["customerid"] != null)
            {
                Entity language = this.BusinessPartner(service, (EntityReference)incident.Attributes["customerid"]);
                bool isExist = languageList.Contains(language.Id);
                if (language != null && !isExist)
                {
                    languageList.Add(language.Id);
                    EntityCollection languages = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getJEDTranslations, incident.Id, gop.Id, language.Id)));
                    if (languages.Entities.Count == 0)
                    {
                        Entity caseTranslation = new Entity("jarvis_jobenddetailstranslation");
                        if (language.Attributes.Contains("jarvis_iso2languagecode6391") && language.Attributes["jarvis_iso2languagecode6391"] != null)
                        {
                            isoCodes.Add((string)language.Attributes["jarvis_iso2languagecode6391"]);
                            caseTranslation["jarvis_name"] = (string)language.Attributes["jarvis_iso2languagecode6391"];
                            caseTranslation["jarvis_language"] = language.ToEntityReference();
                            caseTranslation["jarvis_case"] = incident.ToEntityReference();
                            caseTranslation["jarvis_jobenddetails"] = gop.ToEntityReference();
                            service.Create(caseTranslation);
                        }
                    }
                }
            }
#pragma warning restore SA1123 // Do not place regions within elements
            #endregion

#pragma warning disable SA1123 // Do not place regions within elements
            #region Case-Contacts
            EntityCollection getCaseContacts = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getCaseContacts, incident.Id)));
#pragma warning restore SA1123 // Do not place regions within elements
            if (getCaseContacts != null && getCaseContacts.Entities.Count > 0)
            {
                foreach (var item in getCaseContacts.Entities)
                {
                    if (item.Attributes.Contains("jarvis_preferredlanguage") && item.Attributes["jarvis_preferredlanguage"] != null)
                    {
                        Entity language = this.GetLanguage(service, (EntityReference)item.Attributes["jarvis_preferredlanguage"]);
                        bool isExist = languageList.Contains(language.Id);
                        if (language != null && !isExist)
                        {
                            languageList.Add(language.Id);
                            EntityCollection languages = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getJEDTranslations, incident.Id, gop.Id, language.Id)));
                            if (languages.Entities.Count == 0)
                            {
                                Entity caseTranslation = new Entity("jarvis_jobenddetailstranslation");
                                if (language.Attributes.Contains("jarvis_iso2languagecode6391") && language.Attributes["jarvis_iso2languagecode6391"] != null)
                                {
                                    isoCodes.Add((string)language.Attributes["jarvis_iso2languagecode6391"]);
                                    caseTranslation["jarvis_name"] = (string)language.Attributes["jarvis_iso2languagecode6391"];
                                    caseTranslation["jarvis_language"] = language.ToEntityReference();
                                    caseTranslation["jarvis_case"] = incident.ToEntityReference();
                                    caseTranslation["jarvis_jobenddetails"] = gop.ToEntityReference();
                                    service.Create(caseTranslation);
                                }
                            }
                        }
                    }
                }
            }
            #endregion

#pragma warning disable SA1123 // Do not place regions within elements
            #region JED-Contacts
            EntityCollection getGOPContacts = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getJEDCaseContacts, gop.Id)));
#pragma warning restore SA1123 // Do not place regions within elements
            if (getGOPContacts != null && getGOPContacts.Entities.Count > 0)
            {
                foreach (var item in getGOPContacts.Entities)
                {
                    if (item.Attributes.Contains("jarvis_preferredlanguage") && item.Attributes["jarvis_preferredlanguage"] != null)
                    {
                        Entity language = this.GetLanguage(service, (EntityReference)item.Attributes["jarvis_preferredlanguage"]);
                        bool isExist = languageList.Contains(language.Id);
                        if (language != null && !isExist)
                        {
                            languageList.Add(language.Id);
                            EntityCollection languages = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getJEDTranslations, incident.Id, gop.Id, language.Id)));
                            if (languages.Entities.Count == 0)
                            {
                                Entity caseTranslation = new Entity("jarvis_jobenddetailstranslation");
                                if (language.Attributes.Contains("jarvis_iso2languagecode6391") && language.Attributes["jarvis_iso2languagecode6391"] != null)
                                {
                                    isoCodes.Add((string)language.Attributes["jarvis_iso2languagecode6391"]);
                                    caseTranslation["jarvis_name"] = (string)language.Attributes["jarvis_iso2languagecode6391"];
                                    caseTranslation["jarvis_language"] = language.ToEntityReference();
                                    caseTranslation["jarvis_case"] = incident.ToEntityReference();
                                    caseTranslation["jarvis_jobenddetails"] = gop.ToEntityReference();
                                    service.Create(caseTranslation);
                                }
                            }
                        }
                    }
                }
            }
            #endregion

#pragma warning disable SA1123 // Do not place regions within elements
            #region Country
            if (incident.Attributes.Contains("jarvis_country") && incident.Attributes["jarvis_country"] != null)
            {
                EntityCollection countryLanguages = this.GetCountryLanguages(service, (EntityReference)incident.Attributes["jarvis_country"]);
                if (countryLanguages.Entities.Count > 0)
                {
                    foreach (var item in countryLanguages.Entities)
                    {
                        bool isExist = languageList.Contains(item.Id);
                        if (!isExist)
                        {
                            languageList.Add(item.Id);
                            EntityCollection languages = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getJEDTranslations, incident.Id, gop.Id, item.Id)));
                            if (languages.Entities.Count == 0)
                            {
                                Entity caseTranslation = new Entity("jarvis_jobenddetailstranslation");
                                if (item.Attributes.Contains("jarvis_iso2languagecode6391") && item.Attributes["jarvis_iso2languagecode6391"] != null)
                                {
                                    isoCodes.Add((string)item.Attributes["jarvis_iso2languagecode6391"]);
                                    caseTranslation["jarvis_name"] = (string)item.Attributes["jarvis_iso2languagecode6391"];
                                    caseTranslation["jarvis_language"] = item.ToEntityReference();
                                    caseTranslation["jarvis_case"] = incident.ToEntityReference();
                                    caseTranslation["jarvis_jobenddetails"] = gop.ToEntityReference();
                                    service.Create(caseTranslation);
                                }
                            }
                        }
                    }
                }
            }
#pragma warning restore SA1123 // Do not place regions within elements
            #endregion

#pragma warning disable SA1123 // Do not place regions within elements
            #region PassOut
            EntityCollection passOut = this.GetPassOuts(service, incident);
#pragma warning restore SA1123 // Do not place regions within elements
            if (passOut != null && passOut.Entities.Count > 0)
            {
                foreach (var item in passOut.Entities)
                {
                    if (item.Attributes.Contains("jarvis_repairingdealer") && item.Attributes["jarvis_repairingdealer"] != null)
                    {
                        Entity language = this.BusinessPartner(service, (EntityReference)item.Attributes["jarvis_repairingdealer"]);
                        bool isExist = languageList.Contains(language.Id);
                        if (language != null && !isExist)
                        {
                            languageList.Add(language.Id);
                            EntityCollection languages = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getJEDTranslations, incident.Id, gop.Id, language.Id)));
                            if (languages.Entities.Count == 0)
                            {
                                Entity caseTranslation = new Entity("jarvis_jobenddetailstranslation");
                                if (language.Attributes.Contains("jarvis_iso2languagecode6391") && language.Attributes["jarvis_iso2languagecode6391"] != null)
                                {
                                    isoCodes.Add((string)language.Attributes["jarvis_iso2languagecode6391"]);
                                    caseTranslation["jarvis_name"] = (string)language.Attributes["jarvis_iso2languagecode6391"];
                                    caseTranslation["jarvis_language"] = language.ToEntityReference();
                                    caseTranslation["jarvis_case"] = incident.ToEntityReference();
                                    caseTranslation["jarvis_jobenddetails"] = gop.ToEntityReference();
                                    service.Create(caseTranslation);
                                }
                            }
                        }
                    }
                }
            }
            #endregion

#pragma warning disable SA1123 // Do not place regions within elements
            #region English
            bool isAvailable = isoCodes.Contains("EN");
#pragma warning restore SA1123 // Do not place regions within elements
            if (!isAvailable)
            {
                EntityCollection getEnglish = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getEnglishSupportedLanguage)));
                if (getEnglish.Entities.Count > 0)
                {
                    bool isExist = languageList.Contains(getEnglish.Entities[0].Id);
                    if (!isExist)
                    {
                        EntityCollection languages = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getJEDTranslations, incident.Id, gop.Id, getEnglish.Entities[0].Id)));
                        if (languages.Entities.Count == 0)
                        {
                            isoCodes.Add("EN");
                            Entity caseTranslation = new Entity("jarvis_jobenddetailstranslation");
                            caseTranslation["jarvis_name"] = "EN";
                            caseTranslation["jarvis_language"] = getEnglish.Entities[0].ToEntityReference();
                            caseTranslation["jarvis_jobenddetails"] = gop.ToEntityReference();
                            caseTranslation["jarvis_case"] = incident.ToEntityReference();
                            service.Create(caseTranslation);
                        }
                    }
                }
            }
            #endregion
        }

        /// <summary>
        /// Repair Info Standard Process.
        /// </summary>
        /// <param name="service">Org service.</param>
        /// <param name="tracing">tracing service.</param>
        /// <param name="incident">incident entity.</param>
        /// <param name="triggeredBy">triggered- By.</param>
        /// <param name="gop">gop entity.</param>
        public void RepairInfoStandardProcess(IOrganizationService service, ITracingService tracing, Entity incident, Guid triggeredBy, Entity gop)
        {
            List<Guid> languageList = new List<Guid>();
            List<string> isoCodes = new List<string>();
            incident = service.Retrieve(incident.LogicalName, incident.Id, new ColumnSet("jarvis_homedealer", "customerid", "jarvis_callerlanguagesupported", "jarvis_driverlanguagesupported", "jarvis_country"));

#pragma warning disable SA1123 // Do not place regions within elements
            #region HD
            if (incident.Attributes.Contains("jarvis_homedealer") && incident.Attributes["jarvis_homedealer"] != null)
            {
                Entity language = this.BusinessPartner(service, (EntityReference)incident.Attributes["jarvis_homedealer"]);
                bool isExist = languageList.Contains(language.Id);
                if (language != null && !isExist)
                {
                    languageList.Add(language.Id);
                    EntityCollection languages = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getRepairInfoTranslation, incident.Id, gop.Id, language.Id)));
                    if (languages.Entities.Count == 0)
                    {
                        Entity caseTranslation = new Entity("jarvis_repairinformationtranslation");
                        if (language.Attributes.Contains("jarvis_iso2languagecode6391") && language.Attributes["jarvis_iso2languagecode6391"] != null)
                        {
                            isoCodes.Add((string)language.Attributes["jarvis_iso2languagecode6391"]);
                            caseTranslation["jarvis_name"] = (string)language.Attributes["jarvis_iso2languagecode6391"];
                            caseTranslation["jarvis_language"] = language.ToEntityReference();
                            caseTranslation["jarvis_case"] = incident.ToEntityReference();
                            caseTranslation["jarvis_repairinformation"] = gop.ToEntityReference();
                            service.Create(caseTranslation);
                        }
                    }
                }
            }
#pragma warning restore SA1123 // Do not place regions within elements
            #endregion

#pragma warning disable SA1123 // Do not place regions within elements
            #region Customer
            if (incident.Attributes.Contains("customerid") && incident.Attributes["customerid"] != null)
            {
                Entity language = this.BusinessPartner(service, (EntityReference)incident.Attributes["customerid"]);
                bool isExist = languageList.Contains(language.Id);
                if (language != null && !isExist)
                {
                    languageList.Add(language.Id);
                    EntityCollection languages = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getRepairInfoTranslation, incident.Id, gop.Id, language.Id)));
                    if (languages.Entities.Count == 0)
                    {
                        Entity caseTranslation = new Entity("jarvis_repairinformationtranslation");
                        if (language.Attributes.Contains("jarvis_iso2languagecode6391") && language.Attributes["jarvis_iso2languagecode6391"] != null)
                        {
                            isoCodes.Add((string)language.Attributes["jarvis_iso2languagecode6391"]);
                            caseTranslation["jarvis_name"] = (string)language.Attributes["jarvis_iso2languagecode6391"];
                            caseTranslation["jarvis_language"] = language.ToEntityReference();
                            caseTranslation["jarvis_case"] = incident.ToEntityReference();
                            caseTranslation["jarvis_repairinformation"] = gop.ToEntityReference();
                            service.Create(caseTranslation);
                        }
                    }
                }
            }
#pragma warning restore SA1123 // Do not place regions within elements
            #endregion

#pragma warning disable SA1123 // Do not place regions within elements
            #region Case-Contacts
            EntityCollection getCaseContacts = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getCaseContacts, incident.Id)));
#pragma warning restore SA1123 // Do not place regions within elements
            if (getCaseContacts != null && getCaseContacts.Entities.Count > 0)
            {
                foreach (var item in getCaseContacts.Entities)
                {
                    if (item.Attributes.Contains("jarvis_preferredlanguage") && item.Attributes["jarvis_preferredlanguage"] != null)
                    {
                        Entity language = this.GetLanguage(service, (EntityReference)item.Attributes["jarvis_preferredlanguage"]);
                        bool isExist = languageList.Contains(language.Id);
                        if (language != null && !isExist)
                        {
                            languageList.Add(language.Id);
                            EntityCollection languages = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getRepairInfoTranslation, incident.Id, gop.Id, language.Id)));
                            if (languages.Entities.Count == 0)
                            {
                                Entity caseTranslation = new Entity("jarvis_repairinformationtranslation");
                                if (language.Attributes.Contains("jarvis_iso2languagecode6391") && language.Attributes["jarvis_iso2languagecode6391"] != null)
                                {
                                    isoCodes.Add((string)language.Attributes["jarvis_iso2languagecode6391"]);
                                    caseTranslation["jarvis_name"] = (string)language.Attributes["jarvis_iso2languagecode6391"];
                                    caseTranslation["jarvis_language"] = language.ToEntityReference();
                                    caseTranslation["jarvis_case"] = incident.ToEntityReference();
                                    caseTranslation["jarvis_repairinformation"] = gop.ToEntityReference();
                                    service.Create(caseTranslation);
                                }
                            }
                        }
                    }
                }
            }
            #endregion

#pragma warning disable SA1123 // Do not place regions within elements
            #region Country
            if (incident.Attributes.Contains("jarvis_country") && incident.Attributes["jarvis_country"] != null)
            {
                EntityCollection countryLanguages = this.GetCountryLanguages(service, (EntityReference)incident.Attributes["jarvis_country"]);
                if (countryLanguages.Entities.Count > 0)
                {
                    foreach (var item in countryLanguages.Entities)
                    {
                        bool isExist = languageList.Contains(item.Id);
                        if (!isExist)
                        {
                            languageList.Add(item.Id);
                            EntityCollection languages = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getRepairInfoTranslation, incident.Id, gop.Id, item.Id)));
                            if (languages.Entities.Count == 0)
                            {
                                Entity caseTranslation = new Entity("jarvis_repairinformationtranslation");
                                if (item.Attributes.Contains("jarvis_iso2languagecode6391") && item.Attributes["jarvis_iso2languagecode6391"] != null)
                                {
                                    isoCodes.Add((string)item.Attributes["jarvis_iso2languagecode6391"]);
                                    caseTranslation["jarvis_name"] = (string)item.Attributes["jarvis_iso2languagecode6391"];
                                    caseTranslation["jarvis_language"] = item.ToEntityReference();
                                    caseTranslation["jarvis_case"] = incident.ToEntityReference();
                                    caseTranslation["jarvis_repairinformation"] = gop.ToEntityReference();
                                    service.Create(caseTranslation);
                                }
                            }
                        }
                    }
                }
            }
#pragma warning restore SA1123 // Do not place regions within elements
            #endregion

#pragma warning disable SA1123 // Do not place regions within elements
            #region PassOut
            EntityCollection passOut = this.GetPassOuts(service, incident);
#pragma warning restore SA1123 // Do not place regions within elements
            if (passOut != null && passOut.Entities.Count > 0)
            {
                foreach (var item in passOut.Entities)
                {
                    if (item.Attributes.Contains("jarvis_repairingdealer") && item.Attributes["jarvis_repairingdealer"] != null)
                    {
                        Entity language = this.BusinessPartner(service, (EntityReference)item.Attributes["jarvis_repairingdealer"]);
                        bool isExist = languageList.Contains(language.Id);
                        if (language != null && !isExist)
                        {
                            languageList.Add(language.Id);
                            EntityCollection languages = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getRepairInfoTranslation, incident.Id, gop.Id, language.Id)));
                            if (languages.Entities.Count == 0)
                            {
                                Entity caseTranslation = new Entity("jarvis_repairinformationtranslation");
                                if (language.Attributes.Contains("jarvis_iso2languagecode6391") && language.Attributes["jarvis_iso2languagecode6391"] != null)
                                {
                                    isoCodes.Add((string)language.Attributes["jarvis_iso2languagecode6391"]);
                                    caseTranslation["jarvis_name"] = (string)language.Attributes["jarvis_iso2languagecode6391"];
                                    caseTranslation["jarvis_language"] = language.ToEntityReference();
                                    caseTranslation["jarvis_case"] = incident.ToEntityReference();
                                    caseTranslation["jarvis_repairinformation"] = gop.ToEntityReference();
                                    service.Create(caseTranslation);
                                }
                            }
                        }
                    }
                }
            }
            #endregion

#pragma warning disable SA1123 // Do not place regions within elements
            #region English
            bool isAvailable = isoCodes.Contains("EN");
#pragma warning restore SA1123 // Do not place regions within elements
            if (!isAvailable)
            {
                EntityCollection getEnglish = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getEnglishSupportedLanguage)));
                if (getEnglish.Entities.Count > 0)
                {
                    bool isExist = languageList.Contains(getEnglish.Entities[0].Id);
                    if (!isExist)
                    {
                        EntityCollection languages = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getRepairInfoTranslation, incident.Id, gop.Id, getEnglish.Entities[0].Id)));
                        if (languages.Entities.Count == 0)
                        {
                            isoCodes.Add("EN");
                            Entity caseTranslation = new Entity("jarvis_repairinformationtranslation");
                            caseTranslation["jarvis_name"] = "EN";
                            caseTranslation["jarvis_language"] = getEnglish.Entities[0].ToEntityReference();
                            caseTranslation["jarvis_case"] = incident.ToEntityReference();
                            caseTranslation["jarvis_repairinformation"] = gop.ToEntityReference();
                            service.Create(caseTranslation);
                        }
                    }
                }
            }
            #endregion
        }

        /// <summary>
        /// Business Partner.
        /// </summary>
        /// <param name="service">Org service.</param>
        /// <param name="account">Account Entity Reference.</param>
        /// <returns>Language Entity.</returns>
        public Entity BusinessPartner(IOrganizationService service, EntityReference account)
        {
            EntityReference bulanguage = new EntityReference();
            Entity language = new Entity();
            Entity businessPartner = service.Retrieve(account.LogicalName, account.Id, new ColumnSet("jarvis_language"));
            if (businessPartner.Attributes.Contains("jarvis_language") && businessPartner.Attributes["jarvis_language"] != null)
            {
                bulanguage = (EntityReference)businessPartner.Attributes["jarvis_language"];
                language = service.Retrieve(bulanguage.LogicalName, bulanguage.Id, new ColumnSet("jarvis_iso2languagecode6391"));
            }

            return language;
        }

        /// <summary>
        /// Get Country Languages.
        /// </summary>
        /// <param name="service">Org service.</param>
        /// <param name="country">country entity reference.</param>
        /// <returns>Language EntityCollection.</returns>
        public EntityCollection GetCountryLanguages(IOrganizationService service, EntityReference country)
        {
            EntityCollection language = new EntityCollection();
            language = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getCountryLanguage, country.Id)));
            return language;
        }

        /// <summary>
        /// Get Language.
        /// </summary>
        /// <param name="service">Org service.</param>
        /// <param name="languageRef">languageRef Entity Reference.</param>
        /// <returns>Language Entity.</returns>
        public Entity GetLanguage(IOrganizationService service, EntityReference languageRef)
        {
            Entity language = service.Retrieve(languageRef.LogicalName, languageRef.Id, new ColumnSet("jarvis_iso2languagecode6391"));
            return language;
        }

        /// <summary>
        /// Get PassOuts.
        /// </summary>
        /// <param name="service">Org service.</param>
        /// <param name="incident">incident entity.</param>
        /// <returns>Pass out EntityCollection.</returns>
        public EntityCollection GetPassOuts(IOrganizationService service, Entity incident)
        {
            EntityCollection passOut = new EntityCollection();
            passOut = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.CaseAllActivePassouts, incident.Id)));
            return passOut;
        }
    }
}
