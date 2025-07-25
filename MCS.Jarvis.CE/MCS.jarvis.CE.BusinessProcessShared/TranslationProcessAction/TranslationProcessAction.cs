// <copyright file="TranslationProcessAction.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace MCS.Jarvis.CE.BusinessProcessShared.TranslationProcessAction
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;
    using Constants = MCS.Jarvis.CE.Plugins.Constants;

    /// <summary>
    /// Translation Process Action.
    /// </summary>
    public class TranslationProcessAction
    {
        /// <summary>
        /// Force HD Customer Country - Case Translation.
        /// </summary>
        /// <param name="service">Org Service.</param>
        /// <param name="tracing">Tracing Service.</param>
        /// <param name="entityName">Entity Name.</param>
        /// <param name="incident">Incident.</param>
        /// <param name="targetEntity">Target Entity.</param>
        /// <param name="triggeredBy">Triggered By.</param>
        /// <returns>Is Translation Requried Int.</returns>
        public int ForceHDCustomerCountryTranslation(IOrganizationService service, ITracingService tracing, string entityName, Entity incident, Guid targetEntity, Guid triggeredBy)
        {
            int createFlag = 0;
            if (!string.IsNullOrEmpty(entityName))
            {
                switch (entityName.ToUpper())
                {
                    case "CASE":
                        {
                            return this.CaseHDCCProcess(service, tracing, incident, triggeredBy, createFlag);
                        }

                    case "GOP":
                        {
                            Entity gop = new Entity("jarvis_gop", targetEntity);
                            return this.GOPHDCCProcess(service, tracing, incident, triggeredBy, gop, createFlag);
                        }

                    case "PASSOUT":
                        {
                            Entity passOut = new Entity("jarvis_passout", targetEntity);
                            return this.PassOutHDCCProcess(service, tracing, incident, triggeredBy, passOut, createFlag);
                        }

                    case "JED":
                        {
                            Entity jed = new Entity("jarvis_jobenddetails", targetEntity);
                            return this.JEDHDCCProcess(service, tracing, incident, triggeredBy, jed, createFlag);
                        }

                    case "REPAIR INFO":
                        {
                            Entity repairInfo = new Entity("jarvis_repairinformation", targetEntity);
                            return this.RepairInfoHDCCProcess(service, tracing, incident, triggeredBy, repairInfo, createFlag);
                        }

                    default:
                        {
                            return 0;
                        }
                }
            }

            return createFlag;
        }

        /// <summary>
        /// Force Case-Contacts Translation.
        /// </summary>
        /// <param name="service">Org Service.</param>
        /// <param name="tracing">Tracing Service.</param>
        /// <param name="entityName">Entity Name.</param>
        /// <param name="incident">Incident.</param>
        /// <param name="targetEntity">Target Entity.</param>
        /// <param name="triggeredBy">Triggered By.</param>
        /// <returns>Is Translation Requried Int.</returns>
        public int ForceCaseContactsTranslation(IOrganizationService service, ITracingService tracing, string entityName, Entity incident, Guid targetEntity, Guid triggeredBy)
        {
            int createFlag = 0;
            if (!string.IsNullOrEmpty(entityName))
            {
                switch (entityName.ToUpper())
                {
                    case "CASE":
                        {
                            return this.CaseCCProcess(service, tracing, incident, triggeredBy, createFlag);
                        }

                    case "CASE.GOP":
                        {
                            Entity gop = new Entity("jarvis_gop", targetEntity);
                            return this.CaseGOPCCProcess(service, tracing, incident, triggeredBy, gop, createFlag);
                        }

                    case "CASE.PASSOUT":
                        {
                            Entity passOut = new Entity("jarvis_passout", targetEntity);
                            return this.CasePassOutCCProcess(service, tracing, incident, triggeredBy, passOut, createFlag);
                        }

                    case "CASE.JED":
                        {
                            Entity jed = new Entity("jarvis_jobenddetails", targetEntity);
                            return this.CaseJEDCCProcess(service, tracing, incident, triggeredBy, jed, createFlag);
                        }

                    case "CASE.REPAIR INFO":
                        {
                            Entity repairInfo = new Entity("jarvis_repairinformation", targetEntity);
                            return this.CaseRepairInfoCCProcess(service, tracing, incident, triggeredBy, repairInfo, createFlag);
                        }

                    case "GOP":
                        {
                            Entity gop = new Entity("jarvis_gop", targetEntity);
                            return this.GOPCCProcess(service, tracing, incident, triggeredBy, gop, createFlag);
                        }

                    case "PASSOUT":
                        {
                            Entity passOut = new Entity("jarvis_passout", targetEntity);
                            return this.PassOutCCProcess(service, tracing, incident, triggeredBy, passOut, createFlag);
                        }

                    case "JED":
                        {
                            Entity jed = new Entity("jarvis_jobenddetails", targetEntity);
                            return this.JEDCCProcess(service, tracing, incident, triggeredBy, jed, createFlag);
                        }

                    default:
                        {
                            return 0;
                        }
                }
            }

            return createFlag;
        }

        /// <summary>
        /// Force PassOut Translation.
        /// </summary>
        /// <param name="service">Org Service.</param>
        /// <param name="tracing">Tracing Service.</param>
        /// <param name="entityName">Entity Name.</param>
        /// <param name="incident">Incident.</param>
        /// <param name="targetEntity">Target Entity.</param>
        /// <param name="triggeredBy">Triggered By.</param>
        /// <returns>Is Translation Requried Int.</returns>
        public int ForcePassOutsTranslation(IOrganizationService service, ITracingService tracing, string entityName, Entity incident, Guid targetEntity, Guid triggeredBy)
        {
            int createFlag = 0;
            if (!string.IsNullOrEmpty(entityName))
            {
                switch (entityName.ToUpper())
                {
                    case "CASE":
                        {
                            return this.CasePassOutProcess(service, tracing, incident, triggeredBy, createFlag);
                        }

                    case "GOP":
                        {
                            Entity gop = new Entity("jarvis_gop", targetEntity);
                            return this.GOPPassOutProcess(service, tracing, incident, triggeredBy, gop, createFlag);
                        }

                    case "PASSOUT":
                        {
                            Entity passOut = new Entity("jarvis_passout", targetEntity);
                            return this.PassOutProcess(service, tracing, incident, triggeredBy, passOut, createFlag);
                        }

                    case "JED":
                        {
                            Entity jed = new Entity("jarvis_jobenddetails", targetEntity);
                            return this.JEDPassOutProcess(service, tracing, incident, triggeredBy, jed, createFlag);
                        }

                    case "REPAIR INFO":
                        {
                            Entity repairInfo = new Entity("jarvis_repairinformation", targetEntity);
                            return this.RepairInfoPassOutProcess(service, tracing, incident, triggeredBy, repairInfo, createFlag);
                        }

                    default:
                        {
                            return 0;
                        }
                }
            }

            return createFlag;
        }

        /// <summary>
        /// Force PassOut Translation.
        /// </summary>
        /// <param name="service">Org Service.</param>
        /// <param name="tracing">Tracing Service.</param>
        /// <param name="entityName">Entity Name.</param>
        /// <param name="incident">Incident.</param>
        /// <param name="targetEntity">Target Entity.</param>
        /// <param name="triggeredBy">Triggered By.</param>
        /// <returns>Is Translation Requried Int.</returns>
        public int ForceGOPsTranslation(IOrganizationService service, ITracingService tracing, string entityName, Entity incident, Guid targetEntity, Guid triggeredBy)
        {
            int createFlag = 0;
            if (!string.IsNullOrEmpty(entityName))
            {
                switch (entityName.ToUpper())
                {
                    case "CASE":
                        {
                            return this.CaseGOPProcess(service, tracing, incident, triggeredBy, createFlag);
                        }

                    case "GOP":
                        {
                            Entity gop = new Entity("jarvis_gop", targetEntity);
                            return this.GOPProcess(service, tracing, incident, triggeredBy, gop, createFlag);
                        }

                    case "PASSOUT":
                        {
                            Entity passOut = new Entity("jarvis_passout", targetEntity);
                            return this.PassOutGOPProcess(service, tracing, incident, triggeredBy, passOut, createFlag);
                        }

                    case "JED":
                        {
                            Entity jed = new Entity("jarvis_jobenddetails", targetEntity);
                            return this.JEDGOPProcess(service, tracing, incident, triggeredBy, jed, createFlag);
                        }

                    case "REPAIR INFO":
                        {
                            Entity repairInfo = new Entity("jarvis_repairinformation", targetEntity);
                            return this.RepairInfoGOPProcess(service, tracing, incident, triggeredBy, repairInfo, createFlag);
                        }

                    default:
                        {
                            return 0;
                        }
                }
            }

            return createFlag;
        }

        /// <summary>
        /// Force Case Query Translation.
        /// </summary>
        /// <param name="service">Org Service.</param>
        /// <param name="tracing">Tracing Service.</param>
        /// <param name="entityName">Entity Name.</param>
        /// <param name="incident">Incident.</param>
        /// <param name="targetEntity">Target Entity.</param>
        /// <param name="triggeredBy">Triggered By.</param>
        /// <returns>Is Translation Requried Int.</returns>
        public int ForceCaseQueryTranslation(IOrganizationService service, ITracingService tracing, string entityName, Entity incident, Guid targetEntity, Guid triggeredBy)
        {
            int createFlag = 0;
            if (!string.IsNullOrEmpty(entityName))
            {
                switch (entityName.ToUpper())
                {
                    case "CASE":
                        {
                            Entity relatedCase = new Entity("incident", targetEntity);
                            return this.CaseQueryHDCCPassOutProcess(service, tracing, relatedCase, incident, triggeredBy, createFlag);
                        }

                    default:
                        {
                            return 0;
                        }
                }
            }

            return createFlag;
        }

        /// <summary>
        /// Case HD, Customer and Country Process.
        /// </summary>
        /// <param name="service">Org Service.</param>
        /// <param name="tracing">Tracing.</param>
        /// <param name="incident">Incident.</param>
        /// <param name="triggeredBy">Triggered By.</param>
        /// <param name="createFlag">Create Flag.</param>
        /// <returns>Is Translation Required Int.</returns>
        public int CaseHDCCProcess(IOrganizationService service, ITracingService tracing, Entity incident, Guid triggeredBy, int createFlag)
        {
            List<Guid> languageList = new List<Guid>();
            List<string> isoCodes = new List<string>();
            incident = service.Retrieve(incident.LogicalName, incident.Id, new ColumnSet("jarvis_homedealer", "customerid", "jarvis_country"));

#pragma warning disable SA1123 // Do not place regions within elements
            #region HD
            if (incident.Attributes.Contains("jarvis_homedealer") && incident.Attributes["jarvis_homedealer"] != null)
            {
                Entity language = this.BusinessPartner(service, (EntityReference)incident.Attributes["jarvis_homedealer"]);
                if (language != null)
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
                            createFlag++;
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
                            createFlag++;
                        }
                    }
                }
            }
#pragma warning restore SA1123 // Do not place regions within elements
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
                                    createFlag++;
                                }
                            }
                        }
                    }
                }
            }
#pragma warning restore SA1123 // Do not place regions within elements
            #endregion

            return createFlag;
        }

        /// <summary>
        /// Case Query HD, Customer and PassOut Process.
        /// </summary>
        /// <param name="service">Org Service.</param>
        /// <param name="tracing">Tracing.</param>
        /// <param name="incident">Alert Incident.</param>
        /// <param name="parentCase">Parent Case.</param>
        /// <param name="triggeredBy">Triggered By.</param>
        /// <param name="createFlag">Create Flag.</param>
        /// <returns>Is Translation Required Int.</returns>
        public int CaseQueryHDCCPassOutProcess(IOrganizationService service, ITracingService tracing, Entity incident, Entity parentCase, Guid triggeredBy, int createFlag)
        {
            List<Guid> languageList = new List<Guid>();
            List<string> isoCodes = new List<string>();
            incident = service.Retrieve(incident.LogicalName, incident.Id, new ColumnSet("jarvis_homedealer", "customerid", "jarvis_callerlanguagesupported", "jarvis_driverlanguagesupported", "jarvis_country"));
            parentCase = service.Retrieve(parentCase.LogicalName, parentCase.Id, new ColumnSet("jarvis_queryrepairingdealer", "customerid"));
#pragma warning disable SA1123 // Do not place regions within elements
            #region HD
            if (incident.Attributes.Contains("jarvis_homedealer") && incident.Attributes["jarvis_homedealer"] != null)
            {
                Entity language = this.BusinessPartner(service, (EntityReference)incident.Attributes["jarvis_homedealer"]);
                if (language != null)
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
                            createFlag++;
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
                            createFlag++;
                        }
                    }
                }
            }
#pragma warning restore SA1123 // Do not place regions within elements
            #endregion

#pragma warning disable SA1123 // Do not place regions within elements
            #region Alert Customer
            if (parentCase.Attributes.Contains("customerid") && parentCase.Attributes["customerid"] != null)
            {
                Entity language = this.BusinessPartner(service, (EntityReference)parentCase.Attributes["customerid"]);
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
                            createFlag++;
                        }
                    }
                }
            }
#pragma warning restore SA1123 // Do not place regions within elements
            #endregion

#pragma warning disable SA1123 // Do not place regions within elements
            #region PassOut

            if (parentCase.Attributes.Contains("jarvis_queryrepairingdealer") && parentCase.Attributes["jarvis_queryrepairingdealer"] != null)
            {
                Entity language = this.BusinessPartner(service, (EntityReference)parentCase.Attributes["jarvis_queryrepairingdealer"]);
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
                            createFlag++;
                        }
                    }
                }
            }

            #endregion

            return createFlag;
        }

        /// <summary>
        /// GOP HD, Customer and Country Process.
        /// </summary>
        /// <param name="service">Org Service.</param>
        /// <param name="tracing">Tracing.</param>
        /// <param name="incident">Incident.</param>
        /// <param name="triggeredBy">Triggered By.</param>
        /// <param name="gop">GOP Record.</param>
        /// <param name="createFlag">Create Flag.</param>
        /// <returns>Is Translation Required Int.</returns>
        public int GOPHDCCProcess(IOrganizationService service, ITracingService tracing, Entity incident, Guid triggeredBy, Entity gop, int createFlag)
        {
            List<Guid> languageList = new List<Guid>();
            List<string> isoCodes = new List<string>();
            incident = service.Retrieve(incident.LogicalName, incident.Id, new ColumnSet("jarvis_homedealer", "customerid", "jarvis_country"));

#pragma warning disable SA1123 // Do not place regions within elements
            #region HD
            if (incident.Attributes.Contains("jarvis_homedealer") && incident.Attributes["jarvis_homedealer"] != null)
            {
                Entity language = this.BusinessPartner(service, (EntityReference)incident.Attributes["jarvis_homedealer"]);
                if (language != null)
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
                            createFlag++;
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
                            createFlag++;
                        }
                    }
                }
            }
#pragma warning restore SA1123 // Do not place regions within elements
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
                                    createFlag++;
                                }
                            }
                        }
                    }
                }
            }
#pragma warning restore SA1123 // Do not place regions within elements
            #endregion

            return createFlag;
        }

        /// <summary>
        /// PassOut HD, Customer and Country Process.
        /// </summary>
        /// <param name="service">Org Service.</param>
        /// <param name="tracing">Tracing.</param>
        /// <param name="incident">Incident.</param>
        /// <param name="triggeredBy">Triggered By.</param>
        /// <param name="passOut">PassOut Record.</param>
        /// <param name="createFlag">Create Flag.</param>
        /// <returns>Is Translation Required Int.</returns>
        public int PassOutHDCCProcess(IOrganizationService service, ITracingService tracing, Entity incident, Guid triggeredBy, Entity passOut, int createFlag)
        {
            List<Guid> languageList = new List<Guid>();
            List<string> isoCodes = new List<string>();
            incident = service.Retrieve(incident.LogicalName, incident.Id, new ColumnSet("jarvis_homedealer", "customerid", "jarvis_country"));

#pragma warning disable SA1123 // Do not place regions within elements
            #region HD
            if (incident.Attributes.Contains("jarvis_homedealer") && incident.Attributes["jarvis_homedealer"] != null)
            {
                Entity language = this.BusinessPartner(service, (EntityReference)incident.Attributes["jarvis_homedealer"]);
                if (language != null)
                {
                    languageList.Add(language.Id);
                    EntityCollection languages = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getPassOutTranslation, incident.Id, passOut.Id, language.Id)));
                    if (languages.Entities.Count == 0)
                    {
                        Entity caseTranslation = new Entity("jarvis_passouttranslation");
                        if (language.Attributes.Contains("jarvis_iso2languagecode6391") && language.Attributes["jarvis_iso2languagecode6391"] != null)
                        {
                            isoCodes.Add((string)language.Attributes["jarvis_iso2languagecode6391"]);
                            caseTranslation["jarvis_name"] = (string)language.Attributes["jarvis_iso2languagecode6391"];
                            caseTranslation["jarvis_language"] = language.ToEntityReference();
                            caseTranslation["jarvis_case"] = incident.ToEntityReference();
                            caseTranslation["jarvis_passout"] = passOut.ToEntityReference();
                            service.Create(caseTranslation);
                            createFlag++;
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
                    EntityCollection languages = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getPassOutTranslation, incident.Id, passOut.Id, language.Id)));
                    if (languages.Entities.Count == 0)
                    {
                        Entity caseTranslation = new Entity("jarvis_passouttranslation");
                        if (language.Attributes.Contains("jarvis_iso2languagecode6391") && language.Attributes["jarvis_iso2languagecode6391"] != null)
                        {
                            isoCodes.Add((string)language.Attributes["jarvis_iso2languagecode6391"]);
                            caseTranslation["jarvis_name"] = (string)language.Attributes["jarvis_iso2languagecode6391"];
                            caseTranslation["jarvis_language"] = language.ToEntityReference();
                            caseTranslation["jarvis_case"] = incident.ToEntityReference();
                            caseTranslation["jarvis_passout"] = passOut.ToEntityReference();
                            service.Create(caseTranslation);
                            createFlag++;
                        }
                    }
                }
            }
#pragma warning restore SA1123 // Do not place regions within elements
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
                            EntityCollection languages = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getPassOutTranslation, incident.Id, passOut.Id, item.Id)));
                            if (languages.Entities.Count == 0)
                            {
                                Entity caseTranslation = new Entity("jarvis_passouttranslation");
                                if (item.Attributes.Contains("jarvis_iso2languagecode6391") && item.Attributes["jarvis_iso2languagecode6391"] != null)
                                {
                                    isoCodes.Add((string)item.Attributes["jarvis_iso2languagecode6391"]);
                                    caseTranslation["jarvis_name"] = (string)item.Attributes["jarvis_iso2languagecode6391"];
                                    caseTranslation["jarvis_language"] = item.ToEntityReference();
                                    caseTranslation["jarvis_case"] = incident.ToEntityReference();
                                    caseTranslation["jarvis_passout"] = passOut.ToEntityReference();
                                    service.Create(caseTranslation);
                                    createFlag++;
                                }
                            }
                        }
                    }
                }
            }
#pragma warning restore SA1123 // Do not place regions within elements
            #endregion

            return createFlag;
        }

        /// <summary>
        /// JED HD, Customer and Country Process.
        /// </summary>
        /// <param name="service">Org Service.</param>
        /// <param name="tracing">Tracing.</param>
        /// <param name="incident">Incident.</param>
        /// <param name="triggeredBy">Triggered By.</param>
        /// <param name="jed">JED Record.</param>
        /// <param name="createFlag">Create Flag.</param>
        /// <returns>Is Translation Required Int.</returns>
        public int JEDHDCCProcess(IOrganizationService service, ITracingService tracing, Entity incident, Guid triggeredBy, Entity jed, int createFlag)
        {
            List<Guid> languageList = new List<Guid>();
            List<string> isoCodes = new List<string>();
            incident = service.Retrieve(incident.LogicalName, incident.Id, new ColumnSet("jarvis_homedealer", "customerid", "jarvis_country"));

#pragma warning disable SA1123 // Do not place regions within elements
            #region HD
            if (incident.Attributes.Contains("jarvis_homedealer") && incident.Attributes["jarvis_homedealer"] != null)
            {
                Entity language = this.BusinessPartner(service, (EntityReference)incident.Attributes["jarvis_homedealer"]);
                if (language != null)
                {
                    languageList.Add(language.Id);
                    EntityCollection languages = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getJEDTranslations, incident.Id, jed.Id, language.Id)));
                    if (languages.Entities.Count == 0)
                    {
                        Entity caseTranslation = new Entity("jarvis_jobenddetailstranslation");
                        if (language.Attributes.Contains("jarvis_iso2languagecode6391") && language.Attributes["jarvis_iso2languagecode6391"] != null)
                        {
                            isoCodes.Add((string)language.Attributes["jarvis_iso2languagecode6391"]);
                            caseTranslation["jarvis_name"] = (string)language.Attributes["jarvis_iso2languagecode6391"];
                            caseTranslation["jarvis_language"] = language.ToEntityReference();
                            caseTranslation["jarvis_case"] = incident.ToEntityReference();
                            caseTranslation["jarvis_jobenddetails"] = jed.ToEntityReference();
                            service.Create(caseTranslation);
                            createFlag++;
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
                    EntityCollection languages = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getJEDTranslations, incident.Id, jed.Id, language.Id)));
                    if (languages.Entities.Count == 0)
                    {
                        Entity caseTranslation = new Entity("jarvis_jobenddetailstranslation");
                        if (language.Attributes.Contains("jarvis_iso2languagecode6391") && language.Attributes["jarvis_iso2languagecode6391"] != null)
                        {
                            isoCodes.Add((string)language.Attributes["jarvis_iso2languagecode6391"]);
                            caseTranslation["jarvis_name"] = (string)language.Attributes["jarvis_iso2languagecode6391"];
                            caseTranslation["jarvis_language"] = language.ToEntityReference();
                            caseTranslation["jarvis_case"] = incident.ToEntityReference();
                            caseTranslation["jarvis_jobenddetails"] = jed.ToEntityReference();
                            service.Create(caseTranslation);
                            createFlag++;
                        }
                    }
                }
            }
#pragma warning restore SA1123 // Do not place regions within elements
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
                            EntityCollection languages = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getJEDTranslations, incident.Id, jed.Id, item.Id)));
                            if (languages.Entities.Count == 0)
                            {
                                Entity caseTranslation = new Entity("jarvis_jobenddetailstranslation");
                                if (item.Attributes.Contains("jarvis_iso2languagecode6391") && item.Attributes["jarvis_iso2languagecode6391"] != null)
                                {
                                    isoCodes.Add((string)item.Attributes["jarvis_iso2languagecode6391"]);
                                    caseTranslation["jarvis_name"] = (string)item.Attributes["jarvis_iso2languagecode6391"];
                                    caseTranslation["jarvis_language"] = item.ToEntityReference();
                                    caseTranslation["jarvis_case"] = incident.ToEntityReference();
                                    caseTranslation["jarvis_jobenddetails"] = jed.ToEntityReference();
                                    service.Create(caseTranslation);
                                    createFlag++;
                                }
                            }
                        }
                    }
                }
            }
#pragma warning restore SA1123 // Do not place regions within elements
            #endregion

            return createFlag;
        }

        /// <summary>
        /// Repair Info HD, Customer and Country Process.
        /// </summary>
        /// <param name="service">Org Service.</param>
        /// <param name="tracing">Tracing.</param>
        /// <param name="incident">Incident.</param>
        /// <param name="triggeredBy">Triggered By.</param>
        /// <param name="repairInfo">Repair Info Record.</param>
        /// <param name="createFlag">Create Flag.</param>
        /// <returns>Is Translation Required Int.</returns>
        public int RepairInfoHDCCProcess(IOrganizationService service, ITracingService tracing, Entity incident, Guid triggeredBy, Entity repairInfo, int createFlag)
        {
            List<Guid> languageList = new List<Guid>();
            List<string> isoCodes = new List<string>();
            incident = service.Retrieve(incident.LogicalName, incident.Id, new ColumnSet("jarvis_homedealer", "customerid", "jarvis_country"));

#pragma warning disable SA1123 // Do not place regions within elements
            #region HD
            if (incident.Attributes.Contains("jarvis_homedealer") && incident.Attributes["jarvis_homedealer"] != null)
            {
                Entity language = this.BusinessPartner(service, (EntityReference)incident.Attributes["jarvis_homedealer"]);
                if (language != null)
                {
                    languageList.Add(language.Id);
                    EntityCollection languages = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getRepairInfoTranslation, incident.Id, repairInfo.Id, language.Id)));
                    if (languages.Entities.Count == 0)
                    {
                        Entity caseTranslation = new Entity("jarvis_repairinformationtranslation");
                        if (language.Attributes.Contains("jarvis_iso2languagecode6391") && language.Attributes["jarvis_iso2languagecode6391"] != null)
                        {
                            isoCodes.Add((string)language.Attributes["jarvis_iso2languagecode6391"]);
                            caseTranslation["jarvis_name"] = (string)language.Attributes["jarvis_iso2languagecode6391"];
                            caseTranslation["jarvis_language"] = language.ToEntityReference();
                            caseTranslation["jarvis_case"] = incident.ToEntityReference();
                            caseTranslation["jarvis_repairinformation"] = repairInfo.ToEntityReference();
                            service.Create(caseTranslation);
                            createFlag++;
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
                    EntityCollection languages = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getRepairInfoTranslation, incident.Id, repairInfo.Id, language.Id)));
                    if (languages.Entities.Count == 0)
                    {
                        Entity caseTranslation = new Entity("jarvis_repairinformationtranslation");
                        if (language.Attributes.Contains("jarvis_iso2languagecode6391") && language.Attributes["jarvis_iso2languagecode6391"] != null)
                        {
                            isoCodes.Add((string)language.Attributes["jarvis_iso2languagecode6391"]);
                            caseTranslation["jarvis_name"] = (string)language.Attributes["jarvis_iso2languagecode6391"];
                            caseTranslation["jarvis_language"] = language.ToEntityReference();
                            caseTranslation["jarvis_case"] = incident.ToEntityReference();
                            caseTranslation["jarvis_repairinformation"] = repairInfo.ToEntityReference();
                            service.Create(caseTranslation);
                            createFlag++;
                        }
                    }
                }
            }
#pragma warning restore SA1123 // Do not place regions within elements
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
                            EntityCollection languages = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getRepairInfoTranslation, incident.Id, repairInfo.Id, item.Id)));
                            if (languages.Entities.Count == 0)
                            {
                                Entity caseTranslation = new Entity("jarvis_repairinformationtranslation");
                                if (item.Attributes.Contains("jarvis_iso2languagecode6391") && item.Attributes["jarvis_iso2languagecode6391"] != null)
                                {
                                    isoCodes.Add((string)item.Attributes["jarvis_iso2languagecode6391"]);
                                    caseTranslation["jarvis_name"] = (string)item.Attributes["jarvis_iso2languagecode6391"];
                                    caseTranslation["jarvis_language"] = item.ToEntityReference();
                                    caseTranslation["jarvis_case"] = incident.ToEntityReference();
                                    caseTranslation["jarvis_repairinformation"] = repairInfo.ToEntityReference();
                                    service.Create(caseTranslation);
                                    createFlag++;
                                }
                            }
                        }
                    }
                }
            }
#pragma warning restore SA1123 // Do not place regions within elements
            #endregion

            return createFlag;
        }

        /// <summary>
        /// Case contact Process.
        /// </summary>
        /// <param name="service">Org Service.</param>
        /// <param name="tracing">Tracing.</param>
        /// <param name="incident">Incident.</param>
        /// <param name="triggeredBy">Triggered By.</param>
        /// <param name="createFlag">Create Flag.</param>
        /// <returns>Is Translation Required Int.</returns>
        public int CaseCCProcess(IOrganizationService service, ITracingService tracing, Entity incident, Guid triggeredBy, int createFlag)
        {
            List<Guid> languageList = new List<Guid>();
            List<string> isoCodes = new List<string>();

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
                                    createFlag++;
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            return createFlag;
        }

        /// <summary>
        /// Case GOP - Case Contact Process.
        /// </summary>
        /// <param name="service">Org Service.</param>
        /// <param name="tracing">Tracing.</param>
        /// <param name="incident">Incident.</param>
        /// <param name="triggeredBy">Triggered By.</param>
        /// <param name="gop">GOP Record.</param>
        /// <param name="createFlag">Create Flag.</param>
        /// <returns>Is Translation Required Int.</returns>
        public int CaseGOPCCProcess(IOrganizationService service, ITracingService tracing, Entity incident, Guid triggeredBy, Entity gop, int createFlag)
        {
            List<Guid> languageList = new List<Guid>();
            List<string> isoCodes = new List<string>();

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
                                    createFlag++;
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            return createFlag;
        }

        /// <summary>
        /// Case PassOut - Case Contact Process.
        /// </summary>
        /// <param name="service">Org Service.</param>
        /// <param name="tracing">Tracing.</param>
        /// <param name="incident">Incident.</param>
        /// <param name="triggeredBy">Triggered By.</param>
        /// <param name="passOut">PassOut Record.</param>
        /// <param name="createFlag">Create Flag.</param>
        /// <returns>Is Translation Required Int.</returns>
        public int CasePassOutCCProcess(IOrganizationService service, ITracingService tracing, Entity incident, Guid triggeredBy, Entity passOut, int createFlag)
        {
            List<Guid> languageList = new List<Guid>();
            List<string> isoCodes = new List<string>();

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
                            EntityCollection languages = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getPassOutTranslation, incident.Id, passOut.Id, language.Id)));
                            if (languages.Entities.Count == 0)
                            {
                                Entity caseTranslation = new Entity("jarvis_passouttranslation");
                                if (language.Attributes.Contains("jarvis_iso2languagecode6391") && language.Attributes["jarvis_iso2languagecode6391"] != null)
                                {
                                    isoCodes.Add((string)language.Attributes["jarvis_iso2languagecode6391"]);
                                    caseTranslation["jarvis_name"] = (string)language.Attributes["jarvis_iso2languagecode6391"];
                                    caseTranslation["jarvis_language"] = language.ToEntityReference();
                                    caseTranslation["jarvis_case"] = incident.ToEntityReference();
                                    caseTranslation["jarvis_passout"] = passOut.ToEntityReference();
                                    service.Create(caseTranslation);
                                    createFlag++;
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            return createFlag;
        }

        /// <summary>
        /// Case JED - Case Contact Process.
        /// </summary>
        /// <param name="service">Org Service.</param>
        /// <param name="tracing">Tracing.</param>
        /// <param name="incident">Incident.</param>
        /// <param name="triggeredBy">Triggered By.</param>
        /// <param name="jed">JED Record.</param>
        /// <param name="createFlag">Create Flag.</param>
        /// <returns>Is Translation Required Int.</returns>
        public int CaseJEDCCProcess(IOrganizationService service, ITracingService tracing, Entity incident, Guid triggeredBy, Entity jed, int createFlag)
        {
            List<Guid> languageList = new List<Guid>();
            List<string> isoCodes = new List<string>();

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
                            EntityCollection languages = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getJEDTranslations, incident.Id, jed.Id, language.Id)));
                            if (languages.Entities.Count == 0)
                            {
                                Entity caseTranslation = new Entity("jarvis_jobenddetailstranslation");
                                if (language.Attributes.Contains("jarvis_iso2languagecode6391") && language.Attributes["jarvis_iso2languagecode6391"] != null)
                                {
                                    isoCodes.Add((string)language.Attributes["jarvis_iso2languagecode6391"]);
                                    caseTranslation["jarvis_name"] = (string)language.Attributes["jarvis_iso2languagecode6391"];
                                    caseTranslation["jarvis_language"] = language.ToEntityReference();
                                    caseTranslation["jarvis_case"] = incident.ToEntityReference();
                                    caseTranslation["jarvis_jobenddetails"] = jed.ToEntityReference();
                                    service.Create(caseTranslation);
                                    createFlag++;
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            return createFlag;
        }

        /// <summary>
        /// Case Repair Info - Case Contact Process.
        /// </summary>
        /// <param name="service">Org Service.</param>
        /// <param name="tracing">Tracing.</param>
        /// <param name="incident">Incident.</param>
        /// <param name="triggeredBy">Triggered By.</param>
        /// <param name="repairInfo">Repair Info Record.</param>
        /// <param name="createFlag">Create Flag.</param>
        /// <returns>Is Translation Required Int.</returns>
        public int CaseRepairInfoCCProcess(IOrganizationService service, ITracingService tracing, Entity incident, Guid triggeredBy, Entity repairInfo, int createFlag)
        {
            List<Guid> languageList = new List<Guid>();
            List<string> isoCodes = new List<string>();

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
                            EntityCollection languages = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getRepairInfoTranslation, incident.Id, repairInfo.Id, language.Id)));
                            if (languages.Entities.Count == 0)
                            {
                                Entity caseTranslation = new Entity("jarvis_repairinformationtranslation");
                                if (language.Attributes.Contains("jarvis_iso2languagecode6391") && language.Attributes["jarvis_iso2languagecode6391"] != null)
                                {
                                    isoCodes.Add((string)language.Attributes["jarvis_iso2languagecode6391"]);
                                    caseTranslation["jarvis_name"] = (string)language.Attributes["jarvis_iso2languagecode6391"];
                                    caseTranslation["jarvis_language"] = language.ToEntityReference();
                                    caseTranslation["jarvis_case"] = incident.ToEntityReference();
                                    caseTranslation["jarvis_repairinformation"] = repairInfo.ToEntityReference();
                                    service.Create(caseTranslation);
                                    createFlag++;
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            return createFlag;
        }

        /// <summary>
        /// GOP - Case Contact Process.
        /// </summary>
        /// <param name="service">Org Service.</param>
        /// <param name="tracing">Tracing.</param>
        /// <param name="incident">Incident.</param>
        /// <param name="triggeredBy">Triggered By.</param>
        /// <param name="gop">GOP Record.</param>
        /// <param name="createFlag">Create Flag.</param>
        /// <returns>Is Translation Required Int.</returns>
        public int GOPCCProcess(IOrganizationService service, ITracingService tracing, Entity incident, Guid triggeredBy, Entity gop, int createFlag)
        {
            List<Guid> languageList = new List<Guid>();
            List<string> isoCodes = new List<string>();

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
                                    createFlag++;
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            return createFlag;
        }

        /// <summary>
        /// PassOut - Case Contact Process.
        /// </summary>
        /// <param name="service">Org Service.</param>
        /// <param name="tracing">Tracing.</param>
        /// <param name="incident">Incident.</param>
        /// <param name="triggeredBy">Triggered By.</param>
        /// <param name="passOut">PassOut Record.</param>
        /// <param name="createFlag">Create Flag.</param>
        /// <returns>Is Translation Required Int.</returns>
        public int PassOutCCProcess(IOrganizationService service, ITracingService tracing, Entity incident, Guid triggeredBy, Entity passOut, int createFlag)
        {
            List<Guid> languageList = new List<Guid>();
            List<string> isoCodes = new List<string>();

#pragma warning disable SA1123 // Do not place regions within elements
            #region PassOut-Contacts
            EntityCollection getGOPContacts = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getPassOutCaseContacts, passOut.Id)));
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
                            EntityCollection languages = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getPassOutTranslation, incident.Id, passOut.Id, language.Id)));
                            if (languages.Entities.Count == 0)
                            {
                                Entity caseTranslation = new Entity("jarvis_passouttranslation");
                                if (language.Attributes.Contains("jarvis_iso2languagecode6391") && language.Attributes["jarvis_iso2languagecode6391"] != null)
                                {
                                    isoCodes.Add((string)language.Attributes["jarvis_iso2languagecode6391"]);
                                    caseTranslation["jarvis_name"] = (string)language.Attributes["jarvis_iso2languagecode6391"];
                                    caseTranslation["jarvis_language"] = language.ToEntityReference();
                                    caseTranslation["jarvis_case"] = incident.ToEntityReference();
                                    caseTranslation["jarvis_passout"] = passOut.ToEntityReference();
                                    service.Create(caseTranslation);
                                    createFlag++;
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            return createFlag;
        }

        /// <summary>
        /// JED - Case Contact Process.
        /// </summary>
        /// <param name="service">Org Service.</param>
        /// <param name="tracing">Tracing.</param>
        /// <param name="incident">Incident.</param>
        /// <param name="triggeredBy">Triggered By.</param>
        /// <param name="jed">JED Record.</param>
        /// <param name="createFlag">Create Flag.</param>
        /// <returns>Is Translation Required Int.</returns>
        public int JEDCCProcess(IOrganizationService service, ITracingService tracing, Entity incident, Guid triggeredBy, Entity jed, int createFlag)
        {
            List<Guid> languageList = new List<Guid>();
            List<string> isoCodes = new List<string>();

#pragma warning disable SA1123 // Do not place regions within elements
            #region JED-Contacts
            EntityCollection getGOPContacts = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getJEDCaseContacts, jed.Id)));
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
                            EntityCollection languages = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getJEDTranslations, incident.Id, jed.Id, language.Id)));
                            if (languages.Entities.Count == 0)
                            {
                                Entity caseTranslation = new Entity("jarvis_jobenddetailstranslation");
                                if (language.Attributes.Contains("jarvis_iso2languagecode6391") && language.Attributes["jarvis_iso2languagecode6391"] != null)
                                {
                                    isoCodes.Add((string)language.Attributes["jarvis_iso2languagecode6391"]);
                                    caseTranslation["jarvis_name"] = (string)language.Attributes["jarvis_iso2languagecode6391"];
                                    caseTranslation["jarvis_language"] = language.ToEntityReference();
                                    caseTranslation["jarvis_case"] = incident.ToEntityReference();
                                    caseTranslation["jarvis_jobenddetails"] = jed.ToEntityReference();
                                    service.Create(caseTranslation);
                                    createFlag++;
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            return createFlag;
        }

        /// <summary>
        /// Case PassOut Process.
        /// </summary>
        /// <param name="service">Org Service.</param>
        /// <param name="tracing">Tracing.</param>
        /// <param name="incident">Incident.</param>
        /// <param name="triggeredBy">Triggered By.</param>
        /// <param name="createFlag">Create Flag.</param>
        /// <returns>Is Translation Required Int.</returns>
        public int CasePassOutProcess(IOrganizationService service, ITracingService tracing, Entity incident, Guid triggeredBy, int createFlag)
        {
            List<Guid> languageList = new List<Guid>();
            List<string> isoCodes = new List<string>();

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
                                    createFlag++;
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            return createFlag;
        }

        /// <summary>
        /// GOP PassOut Process.
        /// </summary>
        /// <param name="service">Org Service.</param>
        /// <param name="tracing">Tracing.</param>
        /// <param name="incident">Incident.</param>
        /// <param name="triggeredBy">Triggered By.</param>
        /// <param name="gop">GOP Record.</param>
        /// <param name="createFlag">Create Flag.</param>
        /// <returns>Is Translation Required Int.</returns>
        public int GOPPassOutProcess(IOrganizationService service, ITracingService tracing, Entity incident, Guid triggeredBy, Entity gop, int createFlag)
        {
            List<Guid> languageList = new List<Guid>();
            List<string> isoCodes = new List<string>();

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
                                    createFlag++;
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            return createFlag;
        }

        /// <summary>
        /// PassOut Process.
        /// </summary>
        /// <param name="service">Org Service.</param>
        /// <param name="tracing">Tracing.</param>
        /// <param name="incident">Incident.</param>
        /// <param name="triggeredBy">Triggered By.</param>
        /// <param name="targetPassOut">Target PassOut Record.</param>
        /// <param name="createFlag">Create Flag.</param>
        /// <returns>Is Translation Required Int.</returns>
        public int PassOutProcess(IOrganizationService service, ITracingService tracing, Entity incident, Guid triggeredBy, Entity targetPassOut, int createFlag)
        {
            List<Guid> languageList = new List<Guid>();
            List<string> isoCodes = new List<string>();

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
                            EntityCollection languages = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getPassOutTranslation, incident.Id, targetPassOut.Id, language.Id)));
                            if (languages.Entities.Count == 0)
                            {
                                Entity caseTranslation = new Entity("jarvis_passouttranslation");
                                if (language.Attributes.Contains("jarvis_iso2languagecode6391") && language.Attributes["jarvis_iso2languagecode6391"] != null)
                                {
                                    isoCodes.Add((string)language.Attributes["jarvis_iso2languagecode6391"]);
                                    caseTranslation["jarvis_name"] = (string)language.Attributes["jarvis_iso2languagecode6391"];
                                    caseTranslation["jarvis_language"] = language.ToEntityReference();
                                    caseTranslation["jarvis_case"] = incident.ToEntityReference();
                                    caseTranslation["jarvis_passout"] = targetPassOut.ToEntityReference();
                                    service.Create(caseTranslation);
                                    createFlag++;
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            return createFlag;
        }

        /// <summary>
        /// JED PassOut Process.
        /// </summary>
        /// <param name="service">Org Service.</param>
        /// <param name="tracing">Tracing.</param>
        /// <param name="incident">Incident.</param>
        /// <param name="triggeredBy">Triggered By.</param>
        /// <param name="jed">JED Record.</param>
        /// <param name="createFlag">Create Flag.</param>
        /// <returns>Is Translation Required Int.</returns>
        public int JEDPassOutProcess(IOrganizationService service, ITracingService tracing, Entity incident, Guid triggeredBy, Entity jed, int createFlag)
        {
            List<Guid> languageList = new List<Guid>();
            List<string> isoCodes = new List<string>();

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
                            EntityCollection languages = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getJEDTranslations, incident.Id, jed.Id, language.Id)));
                            if (languages.Entities.Count == 0)
                            {
                                Entity caseTranslation = new Entity("jarvis_jobenddetailstranslation");
                                if (language.Attributes.Contains("jarvis_iso2languagecode6391") && language.Attributes["jarvis_iso2languagecode6391"] != null)
                                {
                                    isoCodes.Add((string)language.Attributes["jarvis_iso2languagecode6391"]);
                                    caseTranslation["jarvis_name"] = (string)language.Attributes["jarvis_iso2languagecode6391"];
                                    caseTranslation["jarvis_language"] = language.ToEntityReference();
                                    caseTranslation["jarvis_case"] = incident.ToEntityReference();
                                    caseTranslation["jarvis_jobenddetails"] = jed.ToEntityReference();
                                    service.Create(caseTranslation);
                                    createFlag++;
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            return createFlag;
        }

        /// <summary>
        /// Repair Info PassOut Process.
        /// </summary>
        /// <param name="service">Org Service.</param>
        /// <param name="tracing">Tracing.</param>
        /// <param name="incident">Incident.</param>
        /// <param name="triggeredBy">Triggered By.</param>
        /// <param name="repairInfo">Repair Info Record.</param>
        /// <param name="createFlag">Create Flag.</param>
        /// <returns>Is Translation Required Int.</returns>
        public int RepairInfoPassOutProcess(IOrganizationService service, ITracingService tracing, Entity incident, Guid triggeredBy, Entity repairInfo, int createFlag)
        {
            List<Guid> languageList = new List<Guid>();
            List<string> isoCodes = new List<string>();

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
                            EntityCollection languages = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getRepairInfoTranslation, incident.Id, repairInfo.Id, language.Id)));
                            if (languages.Entities.Count == 0)
                            {
                                Entity caseTranslation = new Entity("jarvis_repairinformationtranslation");
                                if (language.Attributes.Contains("jarvis_iso2languagecode6391") && language.Attributes["jarvis_iso2languagecode6391"] != null)
                                {
                                    isoCodes.Add((string)language.Attributes["jarvis_iso2languagecode6391"]);
                                    caseTranslation["jarvis_name"] = (string)language.Attributes["jarvis_iso2languagecode6391"];
                                    caseTranslation["jarvis_language"] = language.ToEntityReference();
                                    caseTranslation["jarvis_case"] = incident.ToEntityReference();
                                    caseTranslation["jarvis_repairinformation"] = repairInfo.ToEntityReference();
                                    service.Create(caseTranslation);
                                    createFlag++;
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            return createFlag;
        }

        /// <summary>
        /// Case GOP Process.
        /// </summary>
        /// <param name="service">Org Service.</param>
        /// <param name="tracing">Tracing.</param>
        /// <param name="incident">Incident.</param>
        /// <param name="triggeredBy">Triggered By.</param>
        /// <param name="createFlag">Create Flag.</param>
        /// <returns>Is Translation Required Int.</returns>
        public int CaseGOPProcess(IOrganizationService service, ITracingService tracing, Entity incident, Guid triggeredBy, int createFlag)
        {
            List<Guid> languageList = new List<Guid>();
            List<string> isoCodes = new List<string>();

#pragma warning disable SA1123 // Do not place regions within elements
            #region GOP
            EntityCollection gops = this.GetGOPs(service, incident);
#pragma warning restore SA1123 // Do not place regions within elements
            if (gops != null && gops.Entities.Count > 0)
            {
                foreach (var item in gops.Entities)
                {
                    if (item.Attributes.Contains("jarvis_dealer") && item.Attributes["jarvis_dealer"] != null)
                    {
                        Entity language = this.BusinessPartner(service, (EntityReference)item.Attributes["jarvis_dealer"]);
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
                                    createFlag++;
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            return createFlag;
        }

        /// <summary>
        /// PassOut Process.
        /// </summary>
        /// <param name="service">Org Service.</param>
        /// <param name="tracing">Tracing.</param>
        /// <param name="incident">Incident.</param>
        /// <param name="triggeredBy">Triggered By.</param>
        /// <param name="targetGop">Target GOP Record.</param>
        /// <param name="createFlag">Create Flag.</param>
        /// <returns>Is Translation Required Int.</returns>
        public int GOPProcess(IOrganizationService service, ITracingService tracing, Entity incident, Guid triggeredBy, Entity targetGop, int createFlag)
        {
            List<Guid> languageList = new List<Guid>();
            List<string> isoCodes = new List<string>();

#pragma warning disable SA1123 // Do not place regions within elements
            #region GOP
            EntityCollection gops = this.GetGOPs(service, incident);
#pragma warning restore SA1123 // Do not place regions within elements
            if (gops != null && gops.Entities.Count > 0)
            {
                foreach (var item in gops.Entities)
                {
                    if (item.Attributes.Contains("jarvis_dealer") && item.Attributes["jarvis_dealer"] != null)
                    {
                        Entity language = this.BusinessPartner(service, (EntityReference)item.Attributes["jarvis_dealer"]);
                        bool isExist = languageList.Contains(language.Id);
                        if (language != null && !isExist)
                        {
                            languageList.Add(language.Id);
                            EntityCollection languages = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getGOPTranslation, incident.Id, targetGop.Id, language.Id)));
                            if (languages.Entities.Count == 0)
                            {
                                Entity caseTranslation = new Entity("jarvis_goptranslation");
                                if (language.Attributes.Contains("jarvis_iso2languagecode6391") && language.Attributes["jarvis_iso2languagecode6391"] != null)
                                {
                                    isoCodes.Add((string)language.Attributes["jarvis_iso2languagecode6391"]);
                                    caseTranslation["jarvis_name"] = (string)language.Attributes["jarvis_iso2languagecode6391"];
                                    caseTranslation["jarvis_language"] = language.ToEntityReference();
                                    caseTranslation["jarvis_case"] = incident.ToEntityReference();
                                    caseTranslation["jarvis_gop"] = targetGop.ToEntityReference();
                                    service.Create(caseTranslation);
                                    createFlag++;
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            return createFlag;
        }

        /// <summary>
        /// PassOut GOP Process.
        /// </summary>
        /// <param name="service">Org Service.</param>
        /// <param name="tracing">Tracing.</param>
        /// <param name="incident">Incident.</param>
        /// <param name="triggeredBy">Triggered By.</param>
        /// <param name="targetPassOut">Target PassOut Record.</param>
        /// <param name="createFlag">Create Flag.</param>
        /// <returns>Is Translation Required Int.</returns>
        public int PassOutGOPProcess(IOrganizationService service, ITracingService tracing, Entity incident, Guid triggeredBy, Entity targetPassOut, int createFlag)
        {
            List<Guid> languageList = new List<Guid>();
            List<string> isoCodes = new List<string>();

#pragma warning disable SA1123 // Do not place regions within elements
            #region GOP
            EntityCollection gops = this.GetGOPs(service, incident);
#pragma warning restore SA1123 // Do not place regions within elements
            if (gops != null && gops.Entities.Count > 0)
            {
                foreach (var item in gops.Entities)
                {
                    if (item.Attributes.Contains("jarvis_dealer") && item.Attributes["jarvis_dealer"] != null)
                    {
                        Entity language = this.BusinessPartner(service, (EntityReference)item.Attributes["jarvis_dealer"]);
                        bool isExist = languageList.Contains(language.Id);
                        if (language != null && !isExist)
                        {
                            languageList.Add(language.Id);
                            EntityCollection languages = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getPassOutTranslation, incident.Id, targetPassOut.Id, language.Id)));
                            if (languages.Entities.Count == 0)
                            {
                                Entity caseTranslation = new Entity("jarvis_passouttranslation");
                                if (language.Attributes.Contains("jarvis_iso2languagecode6391") && language.Attributes["jarvis_iso2languagecode6391"] != null)
                                {
                                    isoCodes.Add((string)language.Attributes["jarvis_iso2languagecode6391"]);
                                    caseTranslation["jarvis_name"] = (string)language.Attributes["jarvis_iso2languagecode6391"];
                                    caseTranslation["jarvis_language"] = language.ToEntityReference();
                                    caseTranslation["jarvis_case"] = incident.ToEntityReference();
                                    caseTranslation["jarvis_passout"] = targetPassOut.ToEntityReference();
                                    service.Create(caseTranslation);
                                    createFlag++;
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            return createFlag;
        }

        /// <summary>
        /// JED GOP Process.
        /// </summary>
        /// <param name="service">Org Service.</param>
        /// <param name="tracing">Tracing.</param>
        /// <param name="incident">Incident.</param>
        /// <param name="triggeredBy">Triggered By.</param>
        /// <param name="jed">JED Record.</param>
        /// <param name="createFlag">Create Flag.</param>
        /// <returns>Is Translation Required Int.</returns>
        public int JEDGOPProcess(IOrganizationService service, ITracingService tracing, Entity incident, Guid triggeredBy, Entity jed, int createFlag)
        {
            List<Guid> languageList = new List<Guid>();
            List<string> isoCodes = new List<string>();

#pragma warning disable SA1123 // Do not place regions within elements
            #region PassOut
            EntityCollection gops = this.GetGOPs(service, incident);
#pragma warning restore SA1123 // Do not place regions within elements
            if (gops != null && gops.Entities.Count > 0)
            {
                foreach (var item in gops.Entities)
                {
                    if (item.Attributes.Contains("jarvis_dealer") && item.Attributes["jarvis_dealer"] != null)
                    {
                        Entity language = this.BusinessPartner(service, (EntityReference)item.Attributes["jarvis_dealer"]);
                        bool isExist = languageList.Contains(language.Id);
                        if (language != null && !isExist)
                        {
                            languageList.Add(language.Id);
                            EntityCollection languages = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getJEDTranslations, incident.Id, jed.Id, language.Id)));
                            if (languages.Entities.Count == 0)
                            {
                                Entity caseTranslation = new Entity("jarvis_jobenddetailstranslation");
                                if (language.Attributes.Contains("jarvis_iso2languagecode6391") && language.Attributes["jarvis_iso2languagecode6391"] != null)
                                {
                                    isoCodes.Add((string)language.Attributes["jarvis_iso2languagecode6391"]);
                                    caseTranslation["jarvis_name"] = (string)language.Attributes["jarvis_iso2languagecode6391"];
                                    caseTranslation["jarvis_language"] = language.ToEntityReference();
                                    caseTranslation["jarvis_case"] = incident.ToEntityReference();
                                    caseTranslation["jarvis_jobenddetails"] = jed.ToEntityReference();
                                    service.Create(caseTranslation);
                                    createFlag++;
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            return createFlag;
        }

        /// <summary>
        /// Repair Info GOP Process.
        /// </summary>
        /// <param name="service">Org Service.</param>
        /// <param name="tracing">Tracing.</param>
        /// <param name="incident">Incident.</param>
        /// <param name="triggeredBy">Triggered By.</param>
        /// <param name="repairInfo">Repair Info Record.</param>
        /// <param name="createFlag">Create Flag.</param>
        /// <returns>Is Translation Required Int.</returns>
        public int RepairInfoGOPProcess(IOrganizationService service, ITracingService tracing, Entity incident, Guid triggeredBy, Entity repairInfo, int createFlag)
        {
            List<Guid> languageList = new List<Guid>();
            List<string> isoCodes = new List<string>();

#pragma warning disable SA1123 // Do not place regions within elements
            #region PassOut
            EntityCollection gops = this.GetGOPs(service, incident);
#pragma warning restore SA1123 // Do not place regions within elements
            if (gops != null && gops.Entities.Count > 0)
            {
                foreach (var item in gops.Entities)
                {
                    if (item.Attributes.Contains("jarvis_dealer") && item.Attributes["jarvis_dealer"] != null)
                    {
                        Entity language = this.BusinessPartner(service, (EntityReference)item.Attributes["jarvis_dealer"]);
                        bool isExist = languageList.Contains(language.Id);
                        if (language != null && !isExist)
                        {
                            languageList.Add(language.Id);
                            EntityCollection languages = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getRepairInfoTranslation, incident.Id, repairInfo.Id, language.Id)));
                            if (languages.Entities.Count == 0)
                            {
                                Entity caseTranslation = new Entity("jarvis_repairinformationtranslation");
                                if (language.Attributes.Contains("jarvis_iso2languagecode6391") && language.Attributes["jarvis_iso2languagecode6391"] != null)
                                {
                                    isoCodes.Add((string)language.Attributes["jarvis_iso2languagecode6391"]);
                                    caseTranslation["jarvis_name"] = (string)language.Attributes["jarvis_iso2languagecode6391"];
                                    caseTranslation["jarvis_language"] = language.ToEntityReference();
                                    caseTranslation["jarvis_case"] = incident.ToEntityReference();
                                    caseTranslation["jarvis_repairinformation"] = repairInfo.ToEntityReference();
                                    service.Create(caseTranslation);
                                    createFlag++;
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            return createFlag;
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

        /// <summary>
        /// Get GOPs.
        /// </summary>
        /// <param name="service">Org service.</param>
        /// <param name="incident">incident entity.</param>
        /// <returns>Pass out EntityCollection.</returns>
        public EntityCollection GetGOPs(IOrganizationService service, Entity incident)
        {
            EntityCollection gops = new EntityCollection();
            gops = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.CaseAllActiveGOPs, incident.Id)));
            return gops;
        }
    }
}
