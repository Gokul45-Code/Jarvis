// <copyright file="CaseContactHelper.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MCS.Jarvis.CE.BusinessProcessShared.CaseContact
{
    using System.Collections.Generic;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;
    using static MCS.Jarvis.CE.BusinessProcessesShared.Helpers.Constants;

    /// <summary>
    /// Case Contact Helper.
    /// </summary>
    public class CaseContactHelper
    {
        /// <summary>
        /// Update Incident.
        /// </summary>
        /// <param name="caseConatct">case Contact.</param>
        /// <param name="ccpostImg">cC Post Image.</param>
        /// <param name="incidentRetrieve">Retrieve Incident.</param>
        /// <param name="service">Org service.</param>
        /// <param name="tracingService">tracing Service.</param>
        public void UpdateIncident(Entity caseConatct, Entity ccpostImg, Entity incidentRetrieve, IOrganizationService service, ITracingService tracingService)
        {
            if (ccpostImg != null && (bool)ccpostImg.Attributes[Casecontact.jarvisIsManualUpdate])
            {
                tracingService.Trace("Case Contact contains post Images");
                string fullName = null, phone = null, fixedphone = null, emailCaseContact = null;
                object language = null;

                // name
                if (caseConatct.Attributes.Contains(Casecontact.jarvisFirstname) || caseConatct.Attributes.Contains(Casecontact.jarvisLastname))
                {
                    string firstName = string.Empty;
                    string lastName = string.Empty;

                    if (ccpostImg.Attributes.Contains(Casecontact.jarvisFirstname) && ccpostImg.Attributes[Casecontact.jarvisFirstname] != null)
                    {
                        firstName = (string)ccpostImg.Attributes[Casecontact.jarvisFirstname];
                    }

                    if (ccpostImg.Attributes.Contains(Casecontact.jarvisLastname) && ccpostImg.Attributes[Casecontact.jarvisLastname] != null)
                    {
                        lastName = (string)ccpostImg.Attributes[Casecontact.jarvisLastname];
                    }

                    if (!string.IsNullOrEmpty(firstName) || !string.IsNullOrEmpty(lastName))
                    {
                        fullName = string.Format("{0} {1}", firstName, lastName).Trim();
                    }
                }

                // Phone
                if (caseConatct.Attributes.Contains(Casecontact.jarvisMobilephone))
                {
                    if (ccpostImg.Attributes.Contains(Casecontact.jarvisMobilephone) && ccpostImg.Attributes[Casecontact.jarvisMobilephone] != null)
                    {
                        phone = (string)ccpostImg.Attributes[Casecontact.jarvisMobilephone];
                        tracingService.Trace("phone:" + phone);
                    }
                }

                // Fixed phone
                if (caseConatct.Attributes.Contains(Casecontact.jarvisPhone))
                {
                    if (ccpostImg.Attributes.Contains(Casecontact.jarvisPhone) && ccpostImg.Attributes[Casecontact.jarvisPhone] != null)
                    {
                        fixedphone = (string)ccpostImg.Attributes[Casecontact.jarvisPhone];
                        tracingService.Trace("fixedphone:" + fixedphone);
                    }
                }

                // email
                if (caseConatct.Attributes.Contains(Casecontact.jarvisEmail))
                {
                    if (ccpostImg.Attributes.Contains(Casecontact.jarvisEmail) && ccpostImg.Attributes[Casecontact.jarvisEmail] != null)
                    {
                        emailCaseContact = (string)ccpostImg.Attributes[Casecontact.jarvisEmail];
                        tracingService.Trace("email:" + emailCaseContact);
                    }
                }

                // language
                if (caseConatct.Attributes.Contains(Casecontact.jarvisPreferredlanguage))
                {
                    if (ccpostImg.Attributes.Contains(Casecontact.jarvisPreferredlanguage) && ccpostImg.Attributes[Casecontact.jarvisPreferredlanguage] != null)
                    {
                        language = ccpostImg.Attributes[Casecontact.jarvisPreferredlanguage];
                    }
                }

                Entity incident = new Entity("incident");
                int? issytemCaseContact = ccpostImg.Attributes.Contains("jarvis_casecontacttype") ? ((OptionSetValue)ccpostImg.Attributes["jarvis_casecontacttype"]).Value : (int?)null;

                // caller
                if (issytemCaseContact == 334030000)
                {
                    IDictionary<int, int> options = new Dictionary<int, int>();
                    options.Add(334030001, 1);
                    options.Add(334030006, 2);
                    options.Add(334030002, 3);
                    options.Add(334030004, 4);
                    options.Add(334030005, 5);

                    // Role
                    if (caseConatct.Attributes.Contains(Casecontact.jarvisRole))
                    {
                        if (ccpostImg.Attributes[Casecontact.jarvisRole] != null)
                        {
                            var callerRole = ((OptionSetValue)ccpostImg[Casecontact.jarvisRole]).Value;
                            options.TryGetValue(callerRole, out int role);
                            incident.Attributes["jarvis_callerrole"] = new OptionSetValue(role);
                        }
                        else
                        {
                            incident.Attributes["jarvis_callerrole"] = null;
                        }
                    }

                    if (caseConatct.Attributes.Contains(Casecontact.jarvisFirstname) || caseConatct.Attributes.Contains(Casecontact.jarvisLastname))
                    {
                        incident.Attributes["jarvis_callernameargus"] = fullName;
                    }

                    if (caseConatct.Attributes.Contains(Casecontact.jarvisMobilephone))
                    {
                        tracingService.Trace("Enter into mobile phone logic.");
                        if (incidentRetrieve != null && incidentRetrieve.Attributes.Contains("jarvis_callerphonenumbertype") && incidentRetrieve.Attributes["jarvis_callerphonenumbertype"] != null)
                        {
                            OptionSetValue callerPhoneType = (OptionSetValue)incidentRetrieve.Attributes["jarvis_callerphonenumbertype"];
                            if (callerPhoneType?.Value != null && callerPhoneType?.Value == 334030000)
                            {
                                incident.Attributes["jarvis_callerphone"] = phone;
                            }
                            else
                            {
                                incident.Attributes["jarvis_callerphonenumbertype"] = new OptionSetValue(334030001);
                            }
                        }
                        else
                        {
                            incident.Attributes["jarvis_callerphonenumbertype"] = new OptionSetValue(334030000);
                            incident.Attributes["jarvis_callerphone"] = phone;
                        }
                    }

                    if (caseConatct.Attributes.Contains(Casecontact.jarvisPhone))
                    {
                        if (incidentRetrieve != null && incidentRetrieve.Attributes.Contains("jarvis_callerphonenumbertype") && incidentRetrieve.Attributes["jarvis_callerphonenumbertype"] != null)
                        {
                            OptionSetValue callerPhoneType = (OptionSetValue)incidentRetrieve.Attributes["jarvis_callerphonenumbertype"];
                            if (callerPhoneType?.Value != null && callerPhoneType?.Value == 334030001)
                            {
                                incident.Attributes["jarvis_callerphone"] = fixedphone;
                            }
                            else
                            {
                                incident.Attributes["jarvis_callerphonenumbertype"] = new OptionSetValue(334030000);
                            }
                        }
                        else
                        {
                            incident.Attributes["jarvis_callerphonenumbertype"] = new OptionSetValue(334030001);
                            incident.Attributes["jarvis_callerphone"] = fixedphone;
                        }
                    }

                    if (caseConatct.Attributes.Contains(Casecontact.jarvisEmail))
                    {
                        incident.Attributes["jarvis_calleremail"] = emailCaseContact;
                    }

                    // Preferred Caller Language
                    Entity caseContactlang = new Entity(caseConatct.LogicalName);
                    caseContactlang.Id = caseConatct.Id;
                    if (caseConatct.Attributes.Contains("jarvis_callerlanguage") && caseConatct.Attributes["jarvis_callerlanguage"] != null)
                    {
                        tracingService.Trace("casecontact has caller language");
                        incident.Attributes["jarvis_callerlanguage"] = caseConatct.Attributes["jarvis_callerlanguage"];
                        EntityReference callerPreferredLanguage = (EntityReference)caseConatct.Attributes["jarvis_callerlanguage"];
                        bool isSupported = false;
                        Entity callerLanguage = service.Retrieve(callerPreferredLanguage.LogicalName, callerPreferredLanguage.Id, new ColumnSet("jarvis_vassupportedlanguage", "jarvis_vasstandardlanguage"));
                        if (callerLanguage.Attributes.Contains("jarvis_vassupportedlanguage") && callerLanguage.Attributes["jarvis_vassupportedlanguage"] != null)
                        {
                            if (callerLanguage.Attributes.Contains("jarvis_vasstandardlanguage") && callerLanguage.Attributes["jarvis_vasstandardlanguage"] != null)
                            {
                                isSupported = (bool)callerLanguage.Attributes["jarvis_vasstandardlanguage"];
                            }

                            if (!isSupported)
                            {
                                if (callerLanguage.Attributes.Contains("jarvis_vassupportedlanguage") && callerLanguage.Attributes["jarvis_vassupportedlanguage"] != null)
                                {
                                    caseContactlang["jarvis_preferredlanguage"] = (EntityReference)callerLanguage.Attributes["jarvis_vassupportedlanguage"];
                                    tracingService.Trace("setting preferrend language" + (EntityReference)callerLanguage.Attributes["jarvis_vassupportedlanguage"]);
                                }
                                else
                                {
                                    caseContactlang["jarvis_preferredlanguage"] = (EntityReference)caseConatct.Attributes["jarvis_callerlanguage"];
                                }
                            }
                            else
                            {
                                caseContactlang["jarvis_preferredlanguage"] = (EntityReference)caseConatct.Attributes["jarvis_callerlanguage"];
                            }
                        }
                        else
                        {
                            caseContactlang["jarvis_preferredlanguage"] = (EntityReference)caseConatct.Attributes["jarvis_callerlanguage"];
                        }

                        service.Update(caseContactlang);
                    }
                }

                // Driver
                if (issytemCaseContact == 334030001)
                {
                    if (caseConatct.Attributes.Contains(Casecontact.jarvisFirstname) || caseConatct.Attributes.Contains(Casecontact.jarvisLastname))
                    {
                        incident.Attributes["jarvis_drivername"] = fullName;
                    }

                    if (caseConatct.Attributes.Contains(Casecontact.jarvisMobilephone))
                    {
                        incident.Attributes["jarvis_driverphone"] = phone;
                    }

                    if (caseConatct.Attributes.Contains(Casecontact.jarvisPreferredlanguage))
                    {
                        incident.Attributes["jarvis_driverlanguage"] = language;
                    }

                    // Preferred Driver Language
                    Entity caseContactlang = new Entity(caseConatct.LogicalName);
                    caseContactlang.Id = caseConatct.Id;
                    if (caseConatct.Attributes.Contains("jarvis_driverlanguage") && caseConatct.Attributes["jarvis_driverlanguage"] != null)
                    {
                        tracingService.Trace("casecontact has driver language");
                        incident.Attributes["jarvis_driverlanguage"] = caseConatct.Attributes["jarvis_driverlanguage"];
                        EntityReference callerPreferredLanguage = (EntityReference)caseConatct.Attributes["jarvis_driverlanguage"];
                        bool isSupported = false;
                        Entity driverSupportLanguage = service.Retrieve(callerPreferredLanguage.LogicalName, callerPreferredLanguage.Id, new ColumnSet("jarvis_vassupportedlanguage", "jarvis_vasstandardlanguage"));
                        if (driverSupportLanguage.Attributes.Contains("jarvis_vassupportedlanguage") && driverSupportLanguage.Attributes["jarvis_vassupportedlanguage"] != null)
                        {
                            if (driverSupportLanguage.Attributes.Contains("jarvis_vasstandardlanguage") && driverSupportLanguage.Attributes["jarvis_vasstandardlanguage"] != null)
                            {
                                isSupported = (bool)driverSupportLanguage.Attributes["jarvis_vasstandardlanguage"];
                            }

                            if (!isSupported)
                            {
                                if (driverSupportLanguage.Attributes.Contains("jarvis_vassupportedlanguage") && driverSupportLanguage.Attributes["jarvis_vassupportedlanguage"] != null)
                                {
                                    caseContactlang["jarvis_preferredlanguage"] = (EntityReference)driverSupportLanguage.Attributes["jarvis_vassupportedlanguage"];
                                    tracingService.Trace("setting preferrend language" + (EntityReference)driverSupportLanguage.Attributes["jarvis_vassupportedlanguage"]);
                                }
                                else
                                {
                                    caseContactlang["jarvis_preferredlanguage"] = (EntityReference)caseConatct.Attributes["jarvis_driverlanguage"];
                                }
                            }
                            else
                            {
                                caseContactlang["jarvis_preferredlanguage"] = (EntityReference)caseConatct.Attributes["jarvis_driverlanguage"];
                            }
                        }
                        else
                        {
                            caseContactlang["jarvis_preferredlanguage"] = (EntityReference)caseConatct.Attributes["jarvis_driverlanguage"];
                        }

                        service.Update(caseContactlang);
                    }
                }

                if ((incident.Attributes.Count > 0 && incident.Attributes.Contains("jarvis_callerrole")) || incident.Attributes.Contains("jarvis_callernameargus") ||
                    incident.Attributes.Contains("jarvis_callerphone") || incident.Attributes.Contains("jarvis_callerlanguage") ||
                    incident.Attributes.Contains("jarvis_drivername") || incident.Attributes.Contains("jarvis_driverphone") ||
                    incident.Attributes.Contains("jarvis_driverlanguage") || incident.Attributes.Contains("jarvis_callerphonenumbertype") || incident.Attributes.Contains("jarvis_calleremail"))
                {
                    EntityReference linkedIncident = (EntityReference)ccpostImg.Attributes[Casecontact.jarvisIncident];
                    incident.Attributes["incidentid"] = linkedIncident.Id;
                    tracingService.Trace("Updating Case");
                    service.Update(incident);
                }
            }
        }
    }
}
