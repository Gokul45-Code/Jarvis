// <copyright file="CaseContact.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MCS.Jarvis.CE.BusinessProcessShared.Case
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using MCS.Jarvis.CE.Plugins;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;
    using static MCS.Jarvis.CE.BusinessProcessesShared.Helpers.Constants;

    /// <summary>
    /// Case Contact Incident.
    /// </summary>
    public class CaseContactIncident
    {
        /// <summary>
        /// Case Contact Create.
        /// </summary>
        /// <param name="incident">incident details.</param>
        /// <param name="service">Org service.</param>
        /// <param name="operation">operation details.</param>
        /// <param name="incidentPostImage">incident Post Image.</param>
        /// <param name="tracingService">tracing Service.</param>
        public void CaseContactCreate(Entity incident, IOrganizationService service, string operation, Entity incidentPostImage, ITracingService tracingService)
        {
            // Caller
            // If Traget contains Caller Details
            if (incident.Attributes.Contains("jarvis_callernameargus") || incident.Attributes.Contains("jarvis_callerphone") || incident.Attributes.Contains("jarvis_callerphonenumbertype") || incident.Attributes.Contains("jarvis_callerlanguage") || incident.Attributes.Contains("jarvis_callerrole") || incident.Attributes.Contains("customerid") || incident.Attributes.Contains("jarvis_calleremail"))
            {
                if (operation == "CREATE")
                {
                    Entity caseContactCaller = this.FramingCallerDetails(incident, service, null, operation);
                    if (caseContactCaller.Attributes.Contains(Casecontact.jarvisFirstname) || caseContactCaller.Attributes.Contains(Casecontact.jarvisMobilephone) || caseContactCaller.Attributes.Contains(Casecontact.jarvisPreferredlanguage) || caseContactCaller.Attributes.Contains(Casecontact.jarvisRole))
                    {
                        caseContactCaller[Casecontact.jarvisIncident] = new EntityReference("incident", (Guid)incident.Attributes["incidentid"]);
                        service.Create(caseContactCaller);
                    }
                }
                else if (operation == "UPDATE")
                {
                    // Retrive Case Contact Type Caller
                    tracingService.Trace("Into update of case contact");
                    QueryExpression queryExCaseContactCaller = new QueryExpression(Casecontact.jarvisCasecontact);
                    queryExCaseContactCaller.ColumnSet = new ColumnSet(Casecontact.jarvisCasecontactid, Casecontact.jarvisIncident, Casecontact.jarvisIsManualUpdate);
                    FilterExpression callerFilter = new FilterExpression();
                    callerFilter.AddCondition(Casecontact.jarvisIncident, ConditionOperator.Equal, incident.Attributes["incidentid"]);
                    callerFilter.AddCondition(Casecontact.jarvisCasecontacttype, ConditionOperator.Equal, 334030000);
                    queryExCaseContactCaller.AddOrder("modifiedon", OrderType.Descending);
                    queryExCaseContactCaller.Criteria.AddFilter(callerFilter);
                    queryExCaseContactCaller.NoLock = true;
                    EntityCollection callerCollection = service.RetrieveMultiple(queryExCaseContactCaller);
                    bool callerCollectionFlag = callerCollection.Entities.Any();
                    bool manualUpdate = false;
                    if (callerCollectionFlag)
                    {
                        manualUpdate = callerCollection.Entities[0].Attributes.Contains(Casecontact.jarvisIsManualUpdate) && (bool)callerCollection.Entities[0].Attributes[Casecontact.jarvisIsManualUpdate];
                    }

                    if (manualUpdate)
                    {
                        // Update CaseContact isManualupdate flag
                        tracingService.Trace("Is Manual Update");
                        Entity caseContact = new Entity(Casecontact.jarvisCasecontact);
                        caseContact.Attributes[Casecontact.jarvisCasecontactid] = callerCollection.Entities[0].Attributes["jarvis_casecontactid"];
                        caseContact.Attributes[Casecontact.jarvisIsManualUpdate] = false;
                        service.Update(caseContact);
                    }
                    else
                    {
                        tracingService.Trace("Is not manual update");
                        Entity caseContactCaller = this.FramingCallerDetails(incident, service, incidentPostImage, operation);
                        if (caseContactCaller.Attributes.Contains(Casecontact.jarvisFirstname) || caseContactCaller.Attributes.Contains(Casecontact.jarvisMobilephone) || caseContactCaller.Attributes.Contains(Casecontact.jarvisPreferredlanguage) || caseContactCaller.Attributes.Contains(Casecontact.jarvisCallerlanguage) || caseContactCaller.Attributes.Contains(Casecontact.jarvisRole) ||
                            caseContactCaller.Attributes.Contains(Casecontact.jarvisPhone) || caseContactCaller.Attributes.Contains(Casecontact.jarvisEmail))
                        {
                            // Create New Caller
                            if (!callerCollectionFlag)
                            {
                                caseContactCaller[Casecontact.jarvisIncident] = new EntityReference("incident", (Guid)incident.Attributes["incidentid"]);
                                tracingService.Trace("Creating case Contact Caller");
                                service.Create(caseContactCaller);
                            }
                            else
                            {
                                caseContactCaller.Attributes[Casecontact.jarvisCasecontactid] = callerCollection.Entities[0].Attributes["jarvis_casecontactid"];
                                caseContactCaller.Attributes[Casecontact.jarvisIsManualUpdate] = false;
                                tracingService.Trace("Updating Case Contact caller");
                                service.Update(caseContactCaller);
                            }
                        }
                    }
                }
            }

            int role = 0;
            tracingService.Trace("Before Caller role");
            if (incidentPostImage.Attributes.Contains("jarvis_callerrole") && incidentPostImage.Attributes["jarvis_callerrole"] != null)
            {
                role = ((OptionSetValue)incidentPostImage.Attributes["jarvis_callerrole"]).Value;
            }

            tracingService.Trace("After Caller role");

            // Driver
            if (incident.Attributes.Contains("jarvis_drivername") || incident.Attributes.Contains("jarvis_driverphone") || incident.Attributes.Contains("jarvis_driverlanguage") || role != 0)
            {
                // Check is driver is unique
                bool isDriver = this.CaseContactisUnique(service, incidentPostImage);

                if (operation == "CREATE" && !isDriver)
                {
                    Entity caseContactDriver = this.FramingDriverDetails(incidentPostImage, service, operation);
                    if (caseContactDriver.Attributes.Contains(Casecontact.jarvisFirstname) || caseContactDriver.Attributes.Contains(Casecontact.jarvisMobilephone) || caseContactDriver.Attributes.Contains(Casecontact.jarvisPreferredlanguage) || caseContactDriver.Attributes.Contains(Casecontact.jarvisDriverlanguage))
                    {
                        caseContactDriver[Casecontact.jarvisIncident] = new EntityReference("incident", (Guid)incident.Attributes["incidentid"]);
                        service.Create(caseContactDriver);
                    }
                }
                else if (operation == "UPDATE")
                {
                    // Retrive Case Contact Type Driver
                    QueryExpression queryExCaseContactDriver = new QueryExpression(Casecontact.jarvisCasecontact);
                    queryExCaseContactDriver.ColumnSet = new ColumnSet(Casecontact.jarvisCasecontactid, Casecontact.jarvisIncident, Casecontact.jarvisIsManualUpdate);
                    FilterExpression driverFilter = new FilterExpression();
                    driverFilter.AddCondition(Casecontact.jarvisIncident, ConditionOperator.Equal, incident.Attributes["incidentid"]);
                    driverFilter.AddCondition(Casecontact.jarvisCasecontacttype, ConditionOperator.Equal, 334030001);
                    queryExCaseContactDriver.AddOrder("modifiedon", OrderType.Descending);
                    queryExCaseContactDriver.Criteria.AddFilter(driverFilter);
                    queryExCaseContactDriver.NoLock = true;
                    EntityCollection driverCollection = service.RetrieveMultiple(queryExCaseContactDriver);
                    bool createDriver = driverCollection.Entities.Any();
                    bool manualUpdate = false;
                    if (createDriver)
                    {
                        manualUpdate = driverCollection.Entities[0].Attributes.Contains(Casecontact.jarvisIsManualUpdate) && (bool)driverCollection.Entities[0].Attributes[Casecontact.jarvisIsManualUpdate];
                    }

                    if (manualUpdate)
                    {
                        // Update CaseContact isManualupdate flag
                        Entity caseContact = new Entity(Casecontact.jarvisCasecontact);
                        caseContact.Attributes[Casecontact.jarvisCasecontactid] = driverCollection.Entities[0].Attributes["jarvis_casecontactid"];
                        caseContact.Attributes[Casecontact.jarvisIsManualUpdate] = false;
                        service.Update(caseContact);
                    }
                    else
                    {
                        Entity caseContactDriver = this.FramingDriverDetails(incidentPostImage, service, operation);
                        if (caseContactDriver.Attributes.Contains(Casecontact.jarvisFirstname) || caseContactDriver.Attributes.Contains(Casecontact.jarvisMobilephone) || caseContactDriver.Attributes.Contains(Casecontact.jarvisPreferredlanguage) || caseContactDriver.Attributes.Contains(Casecontact.jarvisDriverlanguage))
                        {
                            if (!createDriver && !isDriver)
                            {
                                // Create new Driver
                                caseContactDriver[Casecontact.jarvisIncident] = new EntityReference("incident", (Guid)incident.Attributes["incidentid"]);
                                service.Create(caseContactDriver);
                            }
                            else if (createDriver)
                            {
                                // update existing Driver
                                caseContactDriver[Casecontact.jarvisIncident] = new EntityReference("incident", (Guid)incident.Attributes["incidentid"]);
                                caseContactDriver.Attributes[Casecontact.jarvisCasecontactid] = driverCollection.Entities[0].Attributes["jarvis_casecontactid"];
                                caseContactDriver.Attributes[Casecontact.jarvisIsManualUpdate] = false;
                                service.Update(caseContactDriver);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Create Customer Case Contact for creating and updating customer case contact.
        /// </summary>
        /// <param name="incident">incident details.</param>
        /// <param name="service">Org service.</param>
        /// <param name="operation">operation details.</param>
        /// <param name="tracingService">tracing Service.</param>
        public void CreateCustomerCaseContact(Entity incident, IOrganizationService service, string operation, ITracingService tracingService)
        {
            tracingService.Trace("inside CreateCustomerCaseContact method for create");
            EntityReference caseCustomer = (EntityReference)incident.Attributes["customerid"];
            Entity customer = service.Retrieve(caseCustomer.LogicalName, caseCustomer.Id, new ColumnSet("name", "telephone1", "jarvis_language"));

            // Customer Create
            if (operation == "CREATE" && customer != null)
            {
                Entity caseContactCustomer = new Entity(Casecontact.jarvisCasecontact);
                if (customer.Attributes.Contains("name"))
                {
                    caseContactCustomer.Attributes[Casecontact.jarvisFirstname] = customer.Attributes["name"];
                    caseContactCustomer.Attributes[Casecontact.jarvisbusinessPartner] = customer.Attributes["name"];
                }

                if (customer.Attributes.Contains("telephone1"))
                {
                    caseContactCustomer.Attributes[Casecontact.jarvisPhone] = customer.Attributes["telephone1"];
                }

                if (customer.Attributes.Contains("jarvis_language"))
                {
                    caseContactCustomer.Attributes[Casecontact.jarvisPreferredlanguage] = (EntityReference)customer.Attributes["jarvis_language"];
                }

                caseContactCustomer.Attributes[Casecontact.jarvisRole] = new OptionSetValue(334030001);
                caseContactCustomer.Attributes[Casecontact.jarvisbusinessPartnerType] = new OptionSetValue(334030000);
                caseContactCustomer.Attributes[Casecontact.jarvisIncident] = new EntityReference("incident", (Guid)incident.Attributes["incidentid"]);
                tracingService.Trace("Creating  CustomerCaseContact");
                service.Create(caseContactCustomer);
            }

            // Customer Update
            if (operation == "UPDATE" && customer != null)
            {
                tracingService.Trace("inside CreateCustomerCaseContact method for update");
                Entity existingCustomer = new Entity();
                QueryExpression queryExCaseContactCustomer = new QueryExpression(Casecontact.jarvisCasecontact);
                queryExCaseContactCustomer.ColumnSet = new ColumnSet(Casecontact.jarvisCasecontactid, Casecontact.jarvisIncident, Casecontact.jarvisbusinessPartnerType, Casecontact.jarvisRole);
                FilterExpression customerFilter = new FilterExpression();
                customerFilter.AddCondition(Casecontact.jarvisIncident, ConditionOperator.Equal, incident.Attributes["incidentid"]);
                customerFilter.AddCondition(Casecontact.jarvisRole, ConditionOperator.Equal, 334030001);
                customerFilter.AddCondition(Casecontact.jarvisbusinessPartnerType, ConditionOperator.Equal, 334030000);

                queryExCaseContactCustomer.Criteria.AddFilter(customerFilter);
                queryExCaseContactCustomer.NoLock = true;
                EntityCollection customerCollection = service.RetrieveMultiple(queryExCaseContactCustomer);

                if (customerCollection != null && customerCollection.Entities.Count > 0)
                {
                    existingCustomer = customerCollection.Entities[0];
                    tracingService.Trace("customerCollection count" + customerCollection.Entities.Count);
                    if (existingCustomer != null)
                    {
                        Entity caseContactCustomer = new Entity(Casecontact.jarvisCasecontact);
                        caseContactCustomer.Id = existingCustomer.Id;
                        if (customer.Attributes.Contains("name"))
                        {
                            caseContactCustomer.Attributes[Casecontact.jarvisFirstname] = customer.Attributes["name"];
                            caseContactCustomer.Attributes[Casecontact.jarvisbusinessPartner] = customer.Attributes["name"];
                        }

                        if (customer.Attributes.Contains("telephone1"))
                        {
                            caseContactCustomer.Attributes[Casecontact.jarvisPhone] = customer.Attributes["telephone1"];
                        }

                        if (customer.Attributes.Contains("jarvis_language"))
                        {
                            caseContactCustomer.Attributes[Casecontact.jarvisPreferredlanguage] = (EntityReference)customer.Attributes["jarvis_language"];
                        }

                        caseContactCustomer.Attributes[Casecontact.jarvisbusinessPartnerType] = new OptionSetValue(334030000);
                        tracingService.Trace("Updating  CustomerCaseContact");
                        service.Update(caseContactCustomer);
                    }
                }
            }
        }

        /// <summary>
        /// Create Case Contact from Customer on updating customer.
        /// </summary>
        /// <param name="incident">incident details.</param>
        /// <param name="service">Org service.</param>
        /// <param name="operation">operation details.</param>
        /// <param name="tracingService">tracing Service.</param>
        public void CreateCustomerContacts(Entity incident, IOrganizationService service, string operation, ITracingService tracingService)
        {
            tracingService.Trace("Creating Customer Contacts.");
            tracingService.Trace("Create Customer Contacts method incident id : " + incident.Id);
            EntityReference caseCustomer = (EntityReference)incident.Attributes["customerid"];
            tracingService.Trace("Create Customer Contacts method for create with customer id : " + caseCustomer.Id);
            EntityCollection getCustomerContacts = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.GetCustomerContacts, caseCustomer.Id)));
            IDictionary<int, int> options = new Dictionary<int, int>();
            options.Add(1, 334030001);
            options.Add(2, 334030002);
            options.Add(3, 334030003);

            // Customer Create
            if (operation == "CREATE" && getCustomerContacts.Entities.Count > 0)
            {
                tracingService.Trace("Inside Create Customer Contacts method for create");
                foreach (var customer in getCustomerContacts.Entities)
                {
                    Entity caseContactCustomer = new Entity(Casecontact.jarvisCasecontact);
                    caseContactCustomer.Attributes[Casecontact.jarvisFirstname] = customer.Attributes.Contains("firstname") ? customer.Attributes["firstname"] : string.Empty;
                    caseContactCustomer.Attributes[Casecontact.jarvisLastname] = customer.Attributes.Contains("lastname") ? customer.Attributes["lastname"] : string.Empty;
                    caseContactCustomer.Attributes["jarvis_jobtitle"] = customer.Attributes.Contains("jarvis_title") ? ((EntityReference)customer.Attributes["jarvis_title"])?.Name : string.Empty;
                    caseContactCustomer.Attributes["jarvis_name"] = customer.Attributes.Contains("fullname") ? customer.Attributes["fullname"] : string.Empty;
                    caseContactCustomer.Attributes[Casecontact.jarvisbusinessPartner] = customer.Attributes.Contains("parentcustomerid") ? ((EntityReference)customer.Attributes["parentcustomerid"])?.Name : string.Empty;
                    caseContactCustomer.Attributes[Casecontact.jarvisPhone] = customer.Attributes.Contains("company") ? customer.Attributes["company"] : string.Empty;
                    caseContactCustomer.Attributes[Casecontact.jarvisMobilephone] = customer.Attributes.Contains("mobilephone") ? customer.Attributes["mobilephone"] : string.Empty;
                    caseContactCustomer.Attributes[Casecontact.jarvisEmail] = customer.Attributes.Contains("emailaddress1") ? customer.Attributes["emailaddress1"] : string.Empty;
                    caseContactCustomer.Attributes[Casecontact.jarvisPreferredlanguage] = customer.Attributes.Contains("jarvis_language") ? (EntityReference)customer.Attributes["jarvis_language"] : null;
                    caseContactCustomer.Attributes[Casecontact.jarvisRole] = new OptionSetValue(334030001);
                    caseContactCustomer.Attributes[Casecontact.jarvisbusinessPartnerType] = customer.Attributes.Contains("jarvis_parentaccounttype") ? (OptionSetValue)customer.Attributes["jarvis_parentaccounttype"] : new OptionSetValue(334030000);
                    caseContactCustomer.Attributes[Casecontact.jarvisIncident] = new EntityReference("incident", (Guid)incident.Attributes["incidentid"]);
                    caseContactCustomer.Attributes["jarvis_callorder"] = customer.Attributes.Contains("jarvis_sortorder") ? customer.Attributes["jarvis_sortorder"] : null;
                    caseContactCustomer.Attributes["jarvis_iscustomercontact"] = true;
                    if (customer.Attributes.Contains("preferredcontactmethodcode"))
                    {
                        if (customer.Attributes["preferredcontactmethodcode"] != null)
                        {
                            var code = ((OptionSetValue)customer["preferredcontactmethodcode"]).Value;
                            options.TryGetValue(code, out int contactCode);
                            caseContactCustomer.Attributes[Casecontact.jarvisPreferredmethodofcontact] = new OptionSetValue(contactCode);
                        }
                        else
                        {
                            caseContactCustomer.Attributes[Casecontact.jarvisPreferredmethodofcontact] = null;
                        }
                    }

                    service.Create(caseContactCustomer);
                }

                tracingService.Trace("Creating Customer Case Contact with no of records : " + getCustomerContacts.Entities.Count.ToString());
                tracingService.Trace("Created Customer Case Contact Successfully.");
            }

            // Customer Update
            if (operation == "UPDATE")
            {
                tracingService.Trace("Inside CreateCustomerContacts method for update");
                QueryExpression queryExCaseContactCustomer = new QueryExpression(Casecontact.jarvisCasecontact);
                queryExCaseContactCustomer.ColumnSet = new ColumnSet(Casecontact.jarvisCasecontactid, Casecontact.jarvisIncident, Casecontact.jarvisbusinessPartnerType, Casecontact.jarvisRole);
                FilterExpression customerFilter = new FilterExpression();
                customerFilter.AddCondition(Casecontact.jarvisIncident, ConditionOperator.Equal, incident.Attributes["incidentid"]);
                customerFilter.AddCondition(Casecontact.jarvisRole, ConditionOperator.Equal, 334030001);
                customerFilter.AddCondition("jarvis_iscustomercontact", ConditionOperator.Equal, true);

                queryExCaseContactCustomer.Criteria.AddFilter(customerFilter);
                queryExCaseContactCustomer.NoLock = true;
                EntityCollection customerContactCollection = service.RetrieveMultiple(queryExCaseContactCustomer);
                tracingService.Trace("Updating Customer Case Contact by removing old customer records count : " + customerContactCollection.Entities.Count.ToString());
                if (customerContactCollection != null && customerContactCollection.Entities.Count > 0)
                {
                    foreach (var customer in customerContactCollection.Entities)
                    {
                        service.Delete(Casecontact.jarvisCasecontact, customer.Id);
                    }
                }

                tracingService.Trace("Updating Customer Case Contact by adding new customer records count : " + getCustomerContacts.Entities.Count.ToString());
                if (getCustomerContacts.Entities.Count > 0)
                {
                    foreach (var customer in getCustomerContacts.Entities)
                    {
                        Entity caseContactCustomer = new Entity(Casecontact.jarvisCasecontact);
                        caseContactCustomer.Attributes[Casecontact.jarvisFirstname] = customer.Attributes.Contains("firstname") ? customer.Attributes["firstname"] : string.Empty;
                        caseContactCustomer.Attributes[Casecontact.jarvisLastname] = customer.Attributes.Contains("lastname") ? customer.Attributes["lastname"] : string.Empty;
                        caseContactCustomer.Attributes["jarvis_jobtitle"] = customer.Attributes.Contains("jarvis_title") ? ((EntityReference)customer.Attributes["jarvis_title"])?.Name : string.Empty;
                        caseContactCustomer.Attributes["jarvis_name"] = customer.Attributes.Contains("fullname") ? customer.Attributes["fullname"] : string.Empty;
                        caseContactCustomer.Attributes[Casecontact.jarvisbusinessPartner] = customer.Attributes.Contains("parentcustomerid") ? ((EntityReference)customer.Attributes["parentcustomerid"])?.Name : string.Empty;
                        caseContactCustomer.Attributes[Casecontact.jarvisPhone] = customer.Attributes.Contains("company") ? customer.Attributes["company"] : string.Empty;
                        caseContactCustomer.Attributes[Casecontact.jarvisMobilephone] = customer.Attributes.Contains("mobilephone") ? customer.Attributes["mobilephone"] : string.Empty;
                        caseContactCustomer.Attributes[Casecontact.jarvisEmail] = customer.Attributes.Contains("emailaddress1") ? customer.Attributes["emailaddress1"] : string.Empty;
                        caseContactCustomer.Attributes[Casecontact.jarvisPreferredlanguage] = customer.Attributes.Contains("jarvis_language") ? (EntityReference)customer.Attributes["jarvis_language"] : null;
                        caseContactCustomer.Attributes[Casecontact.jarvisRole] = new OptionSetValue(334030001);
                        caseContactCustomer.Attributes[Casecontact.jarvisbusinessPartnerType] = customer.Attributes.Contains("jarvis_parentaccounttype") ? (OptionSetValue)customer.Attributes["jarvis_parentaccounttype"] : new OptionSetValue(334030000);
                        caseContactCustomer.Attributes[Casecontact.jarvisIncident] = new EntityReference("incident", (Guid)incident.Attributes["incidentid"]);
                        caseContactCustomer.Attributes["jarvis_callorder"] = customer.Attributes.Contains("jarvis_sortorder") ? customer.Attributes["jarvis_sortorder"] : null;
                        caseContactCustomer.Attributes["jarvis_iscustomercontact"] = true;
                        if (customer.Attributes.Contains("preferredcontactmethodcode"))
                        {
                            if (customer.Attributes["preferredcontactmethodcode"] != null)
                            {
                                var code = ((OptionSetValue)customer["preferredcontactmethodcode"]).Value;
                                options.TryGetValue(code, out int contactCode);
                                caseContactCustomer.Attributes[Casecontact.jarvisPreferredmethodofcontact] = new OptionSetValue(contactCode);
                            }
                            else
                            {
                                caseContactCustomer.Attributes[Casecontact.jarvisPreferredmethodofcontact] = null;
                            }
                        }

                        service.Create(caseContactCustomer);
                    }
                }

                tracingService.Trace("Updated Customer Case Contact Successfully.");
            }
        }

        /// <summary>
        /// Caller Helper method.
        /// </summary>
        /// <param name="incident">Incident details.</param>
        /// <param name="service">Org service.</param>
        /// <param name="operation">operation details.</param>
        /// <returns>Entity details.</returns>
        private Entity FramingCallerDetails(Entity incident, IOrganizationService service, Entity incidentpostImage, string operation)
        {
            IDictionary<int, int> options = new Dictionary<int, int>();
            options.Add(1, 334030001);
            options.Add(2, 334030006);
            options.Add(3, 334030002);
            options.Add(4, 334030004);
            options.Add(5, 334030005);

            Entity caseContact = new Entity(Casecontact.jarvisCasecontact);

            // Caller Detail Record Cretaaion
            if (incident.Attributes.Contains("jarvis_callernameargus"))
            {
                // To check if Caller name is null
                if (!string.IsNullOrEmpty(incident.GetAttributeValue<string>("jarvis_callernameargus")) && !string.IsNullOrWhiteSpace(incident.GetAttributeValue<string>("jarvis_callernameargus")))
                {
                    var fullName = incident.Attributes["jarvis_callernameargus"].ToString().Split(new char[] { ' ' }, 2);
                    caseContact.Attributes[Casecontact.jarvisFirstname] = fullName[0];

                    // check if Caller conatin Lastname
                    if (fullName.Count() > 1)
                    {
                        caseContact.Attributes[Casecontact.jarvisLastname] = fullName[1];
                    }
                    else
                    {
                        caseContact.Attributes[Casecontact.jarvisLastname] = null;
                    }

                    caseContact.Attributes["jarvis_name"] = incident.Attributes["jarvis_callernameargus"].ToString();
                }
                else
                {
                    caseContact.Attributes["jarvis_name"] = null;
                    caseContact.Attributes[Casecontact.jarvisFirstname] = null;
                    caseContact.Attributes[Casecontact.jarvisLastname] = null;
                }
            }

            ////476631 - UI Change
            //// Setting Case Contact Phone number base on Caller Phone Number Type.
            if (operation.ToUpper() == "CREATE" && incident.Attributes.Contains("jarvis_callerphone") && incident.Attributes.Contains("jarvis_callerphonenumbertype") && incident.Attributes["jarvis_callerphonenumbertype"] != null)
            {
                OptionSetValue callerPhoneType = (OptionSetValue)incident.Attributes["jarvis_callerphonenumbertype"];
                //// Caller Phone Type is Mobile Phone
                if (callerPhoneType?.Value != null && callerPhoneType?.Value == 334030000)
                {
                    caseContact.Attributes[Casecontact.jarvisMobilephone] = incident.Attributes["jarvis_callerphone"];
                }
                //// Caller Phone Type is Fixed Phone
                else if (callerPhoneType?.Value != null && callerPhoneType?.Value == 334030001)
                {
                    caseContact.Attributes[Casecontact.jarvisPhone] = incident.Attributes["jarvis_callerphone"];
                }
            }
            else if (operation.ToUpper() == "UPDATE" && (incident.Attributes.Contains("jarvis_callerphone") || incident.Attributes.Contains("jarvis_callerphonenumbertype")))
            {
                if (incidentpostImage != null && incidentpostImage.Attributes.Contains("jarvis_callerphonenumbertype") && incidentpostImage.Attributes["jarvis_callerphonenumbertype"] != null)
                {
                    OptionSetValue callerPhoneType = (OptionSetValue)incidentpostImage.Attributes["jarvis_callerphonenumbertype"];
                    //// Caller Phone Type is Mobile Phone
                    if (callerPhoneType?.Value != null && callerPhoneType?.Value == 334030000)
                    {
                        if (incidentpostImage.Attributes.Contains("jarvis_callerphone") && incidentpostImage.Attributes["jarvis_callerphone"] != null)
                        {
                            caseContact.Attributes[Casecontact.jarvisMobilephone] = incidentpostImage.Attributes["jarvis_callerphone"];
                            if (incident.Attributes.Contains("jarvis_callerphonenumbertype"))
                            {
                                caseContact.Attributes[Casecontact.jarvisPhone] = null;
                            }
                        }
                        else
                        {
                            caseContact.Attributes[Casecontact.jarvisMobilephone] = null;
                        }
                    }
                    //// Caller Phone Type is Fixed Phone
                    else if (callerPhoneType?.Value != null && callerPhoneType?.Value == 334030001)
                    {
                        if (incidentpostImage.Attributes.Contains("jarvis_callerphone") && incidentpostImage.Attributes["jarvis_callerphone"] != null)
                        {
                            caseContact.Attributes[Casecontact.jarvisPhone] = incidentpostImage.Attributes["jarvis_callerphone"];
                            if (incident.Attributes.Contains("jarvis_callerphonenumbertype"))
                            {
                                caseContact.Attributes[Casecontact.jarvisMobilephone] = null;
                            }
                        }
                        else
                        {
                            caseContact.Attributes[Casecontact.jarvisPhone] = null;
                        }
                    }
                }
            }

            ////if (incident.Attributes.Contains("jarvis_callerphone"))
            ////{
            ////    caseContact.Attributes[Casecontact.jarvisMobilephone] = incident.Attributes["jarvis_callerphone"];
            ////}

            ////if (incident.Attributes.Contains("jarvis_callerfixedphone"))
            ////{
            ////    caseContact.Attributes[Casecontact.jarvisPhone] = incident.Attributes["jarvis_callerfixedphone"];
            ////}

            if (incident.Attributes.Contains("jarvis_calleremail"))
            {
                caseContact.Attributes[Casecontact.jarvisEmail] = incident.Attributes["jarvis_calleremail"];
            }

            if (incident.Attributes.Contains("jarvis_callerrole"))
            {
                if (incident.Attributes["jarvis_callerrole"] != null)
                {
                    var callerRole = ((OptionSetValue)incident["jarvis_callerrole"]).Value;
                    options.TryGetValue(callerRole, out int role);
                    caseContact.Attributes[Casecontact.jarvisRole] = new OptionSetValue(role);
                }
                else
                {
                    caseContact.Attributes[Casecontact.jarvisRole] = null;
                }
            }

            if (incident.Attributes.Contains("jarvis_callerlanguagesupported") && incident.Attributes["jarvis_callerlanguagesupported"] != null)
            {
                caseContact[Casecontact.jarvisPreferredlanguage] = (EntityReference)incident.Attributes["jarvis_callerlanguagesupported"];
            }

            if (incident.Attributes.Contains("jarvis_callerlanguage") && incident.Attributes["jarvis_callerlanguage"] != null)
            {
                caseContact[Casecontact.jarvisCallerlanguage] = (EntityReference)incident.Attributes["jarvis_callerlanguage"];
            }

            caseContact.Attributes[Casecontact.jarvisIsManualUpdate] = false;
            if (operation == "CREATE")
            {
                caseContact.Attributes[Casecontact.jarvisPreferredmethodofcontact] = new OptionSetValue(Casecontact.callerMethodofContact);
            }

            caseContact.Attributes[Casecontact.jarvisCasecontacttype] = new OptionSetValue(Casecontact.callerContactType);

            return caseContact;
        }

        /// <summary>
        /// Driver Helper method.
        /// </summary>
        /// <param name="incidentPostImage">incident Post Image.</param>
        /// <param name="service">service details.</param>
        /// <param name="operation">operation details.</param>
        /// <returns>Entity details.</returns>
        private Entity FramingDriverDetails(Entity incidentPostImage, IOrganizationService service, string operation)
        {
            Entity caseContact = new Entity(Casecontact.jarvisCasecontact);

            if (incidentPostImage.Attributes.Contains("jarvis_drivername"))
            {
                // To check if driver name is null
                if (!string.IsNullOrEmpty(incidentPostImage.GetAttributeValue<string>("jarvis_drivername")) && !string.IsNullOrWhiteSpace(incidentPostImage.GetAttributeValue<string>("jarvis_drivername")))
                {
                    var fullName = incidentPostImage.Attributes["jarvis_drivername"].ToString().Split(new char[] { ' ' }, 2);
                    caseContact.Attributes[Casecontact.jarvisFirstname] = fullName[0];

                    // check if driver conatin Lastname
                    if (fullName.Count() > 1)
                    {
                        caseContact.Attributes[Casecontact.jarvisLastname] = fullName[1];
                    }
                    else
                    {
                        caseContact.Attributes[Casecontact.jarvisLastname] = null;
                    }

                    caseContact.Attributes["jarvis_name"] = incidentPostImage.Attributes["jarvis_drivername"].ToString();
                }
                else
                {
                    caseContact.Attributes["jarvis_name"] = null;
                    caseContact.Attributes[Casecontact.jarvisFirstname] = null;
                    caseContact.Attributes[Casecontact.jarvisLastname] = null;
                }
            }

            if (incidentPostImage.Attributes.Contains("jarvis_driverphone") && incidentPostImage.Attributes["jarvis_driverphone"] != null)
            {
                caseContact.Attributes[Casecontact.jarvisMobilephone] = incidentPostImage.Attributes["jarvis_driverphone"];
            }

            if (incidentPostImage.Attributes.Contains("jarvis_driverlanguagesupported") && incidentPostImage.Attributes["jarvis_driverlanguagesupported"] != null)
            {
                caseContact[Casecontact.jarvisPreferredlanguage] = (EntityReference)incidentPostImage.Attributes["jarvis_driverlanguagesupported"];
            }

            if (incidentPostImage.Attributes.Contains("jarvis_driverlanguage") && incidentPostImage.Attributes["jarvis_driverlanguage"] != null)
            {
                caseContact[Casecontact.jarvisDriverlanguage] = (EntityReference)incidentPostImage.Attributes["jarvis_driverlanguage"];
            }

            caseContact.Attributes[Casecontact.jarvisIsManualUpdate] = false;
            if (operation == "CREATE")
            {
                caseContact.Attributes[Casecontact.jarvisPreferredmethodofcontact] = new OptionSetValue(Casecontact.driverMethodofContact);
            }

            caseContact.Attributes[Casecontact.jarvisCasecontacttype] = new OptionSetValue(Casecontact.driverContactType);
            caseContact.Attributes[Casecontact.jarvisRole] = new OptionSetValue(Casecontact.driverRole);

            return caseContact;
        }

        /// <summary>
        /// Check if Driver is unique.
        /// </summary>
        /// <param name="service">Org service.</param>
        /// <param name="casePostImage">Post Image.</param>
        /// <returns>Exception details.</returns>
        private bool CaseContactisUnique(IOrganizationService service, Entity casePostImage)
        {
            string firstName = null, lastName = null;
            if (casePostImage.Attributes.Contains("jarvis_drivername"))
            {
                // To check if Caller name is null
                if (!string.IsNullOrEmpty(casePostImage.GetAttributeValue<string>("jarvis_drivername")) && !string.IsNullOrWhiteSpace(casePostImage.GetAttributeValue<string>("jarvis_callernameargus")))
                {
                    var fullName = casePostImage.Attributes["jarvis_drivername"].ToString().Split(new char[] { ' ' }, 2);
                    firstName = fullName[0];

                    // check if Caller conatin Lastname
                    if (fullName.Count() > 1)
                    {
                        lastName = fullName[1];
                    }
                }
            }

            // Fetch Record of role driver and caseContactType = Caller
            QueryExpression queryExCaseContactDriver = new QueryExpression(Casecontact.jarvisCasecontact);
            queryExCaseContactDriver.ColumnSet = new ColumnSet(Casecontact.jarvisCasecontactid, Casecontact.jarvisIncident, Casecontact.jarvisIsManualUpdate, Casecontact.jarvisLastname, Casecontact.jarvisFirstname);
            FilterExpression driverFilter = new FilterExpression();
            driverFilter.AddCondition(Casecontact.jarvisIncident, ConditionOperator.Equal, casePostImage.Attributes["incidentid"]);
            driverFilter.AddCondition(Casecontact.jarvisRole, ConditionOperator.Equal, Casecontact.driverRole);
            driverFilter.AddCondition(Casecontact.jarvisCasecontacttype, ConditionOperator.Equal, 334030000);
            queryExCaseContactDriver.Criteria.AddFilter(driverFilter);
            queryExCaseContactDriver.NoLock = true;
            EntityCollection driverCollection = service.RetrieveMultiple(queryExCaseContactDriver);

            if (driverCollection.Entities.Any())
            {
                foreach (Entity entity in driverCollection.Entities)
                {
                    if (!string.IsNullOrEmpty(entity.GetAttributeValue<string>(Casecontact.jarvisFirstname)) && !string.IsNullOrWhiteSpace(entity.GetAttributeValue<string>(Casecontact.jarvisFirstname)))
                    {
                        // if Firstname contains and it is identical
                        if (entity.Attributes[Casecontact.jarvisFirstname].ToString() == firstName)
                        {
                            // if LastName contains and it is identical
                            if (!string.IsNullOrEmpty(entity.GetAttributeValue<string>(Casecontact.jarvisLastname)) && !string.IsNullOrWhiteSpace(entity.GetAttributeValue<string>(Casecontact.jarvisLastname)))
                            {
                                if (entity.Attributes[Casecontact.jarvisLastname].ToString() == lastName)
                                {
                                    return true;
                                }
                            }
                            else if (lastName != null)
                            {
                                return false;
                            }
                            else
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }
    }
}
