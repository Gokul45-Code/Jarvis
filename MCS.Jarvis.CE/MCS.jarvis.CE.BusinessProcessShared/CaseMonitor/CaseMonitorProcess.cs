// <copyright file="CaseMonitorProcess.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MCS.Jarvis.CE.BusinessProcessShared.CaseMonitor
{
    using System;
    using System.Linq;
    using MCS.Jarvis.CE.BusinessProcessesShared.Helpers;
    using MCS.Jarvis.CE.BusinessProcessShared.AppNotification;
    using Microsoft.Crm.Sdk.Messages;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;

    /// <summary>
    /// Initializes a new instance of the <see cref="CaseMonitorProcess"/> class.
    /// </summary>
    public class CaseMonitorProcess
    {
        /// <summary>
        /// create Monitor Process.
        /// </summary>
        /// <param name="initiatingUserID">initiating User ID.</param>
        /// <param name="monitor">monitor details.</param>
        /// <param name="incident">incident details.</param>
        /// <param name="service">Org service.</param>
        /// <param name="adminService">admin Service.</param>
        /// <param name="tracingService">tracing Service.</param>
        /// <param name="iscreate">is create.</param>
        /// <param name="dueDate">due Date.</param>
        public void CreateMonitorProcess(Guid initiatingUserID, Entity monitor, Entity incident, IOrganizationService service, IOrganizationService adminService, ITracingService tracingService, bool iscreate = false, DateTime? dueDate = null)
        {
            bool typeCheck = false;
            OptionSetValue urgency = new OptionSetValue();
            string subject = string.Empty;

            // Identify Action Type
            // region Identify Action Type
            if (monitor.Attributes.Contains("jarvis_actiontype") && monitor.Attributes["jarvis_actiontype"] != null)
            {
                // Already Set
            }
            else
            {
                if (monitor.Attributes.Contains("prioritycode") && monitor.Attributes["prioritycode"] != null)
                {
                    urgency = (OptionSetValue)monitor.Attributes["prioritycode"];

                    // High
                    if (urgency.Value == 0)
                    {
                        monitor["jarvis_actiontype"] = new OptionSetValue(1);
                        monitor["jarvis_monitorsortorder"] = 1;
                        typeCheck = true;
                    }

                    if (!typeCheck)
                    {
                        if (incident.Attributes.Contains("statuscode") && incident.Attributes["statuscode"] != null)
                        {
                            OptionSetValue caseStatus = (OptionSetValue)incident.Attributes["statuscode"];
                            if (caseStatus.Value == 10)
                            {
                                // Case Opening
                                monitor["jarvis_actiontype"] = new OptionSetValue(2);
                                monitor["jarvis_monitorsortorder"] = 2;
                                typeCheck = true;
                            }
                        }
                    }

                    if (!typeCheck)
                    {
                        if (incident.Attributes.Contains("statuscode") && incident.Attributes["statuscode"] != null)
                        {
                            OptionSetValue caseStatus = (OptionSetValue)incident.Attributes["statuscode"];
                            if (caseStatus.Value == 20)
                            {
                                // GOP
                                monitor["jarvis_actiontype"] = new OptionSetValue(3);
                                monitor["jarvis_monitorsortorder"] = 3;
                                typeCheck = true;
                            }
                        }
                    }

                    if (!typeCheck)
                    {
                        if (incident.Attributes.Contains("statuscode") && incident.Attributes["statuscode"] != null)
                        {
                            OptionSetValue caseStatus = (OptionSetValue)incident.Attributes["statuscode"];
                            if (caseStatus.Value == 30)
                            {
                                // Pass Out
                                monitor["jarvis_actiontype"] = new OptionSetValue(4);
                                monitor["jarvis_monitorsortorder"] = 4;
                                typeCheck = true;
                            }
                        }
                    }

                    if (!typeCheck)
                    {
                        if (monitor.Attributes.Contains("subject") && monitor.Attributes["subject"] != null)
                        {
                            subject = (string)monitor.Attributes["subject"];
                            if (subject.Contains("GOP+"))
                            {
                                // GOP+
                                monitor["jarvis_actiontype"] = new OptionSetValue(5);
                                monitor["jarvis_monitorsortorder"] = 5;
                                typeCheck = true;
                            }
                        }
                    }

                    if (!typeCheck)
                    {
                        if (monitor.Attributes.Contains("subject") && monitor.Attributes["subject"] != null)
                        {
                            subject = (string)monitor.Attributes["subject"];
                            if (subject.Contains("DESC"))
                            {
                                // Decision
                                monitor["jarvis_actiontype"] = new OptionSetValue(6);
                                monitor["jarvis_monitorsortorder"] = 6;
                                typeCheck = true;
                            }
                        }
                    }

                    if (!typeCheck)
                    {
                        if (incident.Attributes.Contains("isescalated") && incident.Attributes["isescalated"] != null)
                        {
                            bool isescalated = (bool)incident.Attributes["isescalated"];
                            if (isescalated)
                            {
                                // Pass Out
                                monitor["jarvis_actiontype"] = new OptionSetValue(7);
                                monitor["jarvis_monitorsortorder"] = 7;
                                typeCheck = true;
                            }
                        }
                    }

                    if (!typeCheck)
                    {
                        if (urgency.Value == 1)
                        {
                            // Medium
                            monitor["jarvis_actiontype"] = new OptionSetValue(8);
                            monitor["jarvis_monitorsortorder"] = 8;
                            typeCheck = true;
                        }
                    }

                    if (!typeCheck)
                    {
                        if (urgency.Value == 2)
                        {
                            // Standard
                            monitor["jarvis_actiontype"] = new OptionSetValue(17);
                            monitor["jarvis_monitorsortorder"] = 17;
                        }
                    }
                }
            }

            // endregion Identify Action Type

            // region update Follow Up Timestamp
            tracingService.Trace("update Follow Up Timestamp");
            int timeZoneCode = 105;
            if (monitor.Attributes.Contains("actualstart") && monitor.Attributes["actualstart"] != null && !monitor.Attributes.Contains("jarvis_followuptimestamp"))
            {
                if (monitor.Attributes.Contains("jarvis_followuptime") && monitor.Attributes["jarvis_followuptime"] != null)
                {
                    string time = (string)monitor.Attributes["jarvis_followuptime"];

                    if (time != null)
                    {
                        time = time.Replace(":", string.Empty);
                        string[] substrings = SplitString(time, 2);
                        TimeSpan futime = new TimeSpan(Convert.ToInt16(substrings[0]), Convert.ToInt16(substrings[1]), 0);
                        DateTime actualStart = (DateTime)monitor.Attributes["actualstart"];
                        var currentuserTimeCode = this.RetrieveCurrentUsersTimeZoneSettings(service);
                        tracingService.Trace("current user time zone code " + currentuserTimeCode?.ToString());
                        if (currentuserTimeCode != null)
                        {
                            timeZoneCode = (int)currentuserTimeCode;
                        }

                        actualStart = this.RetrieveLocalTimeFromUTCTime(service, actualStart, timeZoneCode);
                        actualStart = actualStart.Date;
                        actualStart = actualStart.Add(futime);
                        monitor["jarvis_followuptimestamp"] = actualStart;
                    }
                    else
                    {
                        monitor["jarvis_followuptimestamp"] = (DateTime)monitor.Attributes["actualstart"];
                    }
                }
                else
                {
                    monitor["jarvis_followuptimestamp"] = (DateTime)monitor.Attributes["actualstart"];
                }
            }

            // endregion update Follow Up Timestamp
            if (monitor.Attributes.Contains("subject") && monitor.Attributes["subject"] != null)
            {
                string fucomment = (string)monitor.Attributes["subject"];
                string[] countryCode = fucomment.Split(' ');
                if (countryCode != null && countryCode.Length > 1)
                {
                    string code = countryCode[1];
                    string languageCode = countryCode[0];
                    EntityCollection getMonitorActionsForCase = service.RetrieveMultiple(new FetchExpression(string.Format(MCS.Jarvis.CE.Plugins.Constants.FetchXmls.getCountryMO, code)));
                    EntityCollection getLanguageforMOCase = service.RetrieveMultiple(new FetchExpression(string.Format(MCS.Jarvis.CE.Plugins.Constants.FetchXmls.getLanguageMO, languageCode)));
                    if (getMonitorActionsForCase.Entities.Count > 0)
                    {
                        monitor["jarvis_followupcountry"] = new EntityReference(getMonitorActionsForCase.Entities[0].LogicalName, getMonitorActionsForCase.Entities[0].Id);
                    }

                    if (getLanguageforMOCase.Entities.Count > 0)
                    {
                        monitor["jarvis_followuplanguage"] = new EntityReference(getLanguageforMOCase.Entities[0].LogicalName, getLanguageforMOCase.Entities[0].Id);
                    }
                }

                // region Populate Country and Language  --Added by Divakar
                // region Fall Back Country and Language
                if (incident.Attributes.Contains("jarvis_country") && incident.Attributes["jarvis_country"] != null)
                {
                    if (!monitor.Attributes.Contains("jarvis_followupcountry"))
                    {
                        EntityReference caseCountry = (EntityReference)incident.Attributes["jarvis_country"];
                        monitor["jarvis_followupcountry"] = caseCountry;
                    }
                }

                if (!monitor.Attributes.Contains("jarvis_followuplanguage"))
                {
                    EntityCollection getLanguageforMOCaseEN = service.RetrieveMultiple(new FetchExpression(string.Format(Plugins.Constants.FetchXmls.getLanguageMO, "ENG")));
                    if (getLanguageforMOCaseEN.Entities.Count > 0)
                    {
                        // throw new InvalidPluginExecutionException((string)getLanguageforMOCase.Entities[0].Attributes["jarvis_name"]);
                        monitor["jarvis_followuplanguage"] = new EntityReference(getLanguageforMOCaseEN.Entities[0].LogicalName, getLanguageforMOCaseEN.Entities[0].Id);
                    }
                }

                // end region
                if (iscreate)
                {
                    OptionSetValue actiontype = (OptionSetValue)monitor.Attributes["jarvis_actiontype"];
                    ////int timeZoneCode = 105;
                    DateTime actualStart = this.RetrieveLocalTimeFromUTCTime(service, DateTime.UtcNow, timeZoneCode);
                    if (dueDate != null)
                    {
                        actualStart = this.RetrieveLocalTimeFromUTCTime(service, dueDate.Value, timeZoneCode);
                        monitor["actualstart"] = actualStart; // jarvis_followuptime date
                    }
                    else
                    {
                        monitor["actualstart"] = actualStart;
                    }

                    monitor["regardingobjectid"] = incident.ToEntityReference();
                    monitor["jarvis_monitorsortorder"] = actiontype.Value;
                    service.Create(monitor);
                }
                else
                {
                    ////int timeZoneCode = 105;
                    DateTime createdTime = this.RetrieveLocalTimeFromUTCTime(service, (DateTime)monitor.Attributes["createdon"], timeZoneCode);
                    string body = string.Format("'{0}' Action added {1}", monitor.Attributes["subject"], createdTime);
                    bool notifyAll = true;
                    CaseNotifiaction casenotifiaction = new CaseNotifiaction();
                    casenotifiaction.FrameNotifiaction(initiatingUserID, adminService, incident.Id, new Guid(new byte[16]), MCS.Jarvis.CE.Plugins.Constants.NotificationData.MonitorActionCreate, body, notifyAll);
                }
            }
        }

        /// <summary>
        /// delete Monitor Process.
        /// </summary>
        /// <param name="monitor">monitor details.</param>
        /// <param name="incident">incident details.</param>
        /// <param name="service">Org service.</param>
        public void DeleteMonitorProcess(EntityReference monitor, EntityReference incident, IOrganizationService service)
        {
            OptionSetValue casetypecode = new OptionSetValue();
            OptionSetValue caselocation = new OptionSetValue();
            EntityReference caseserviceline = new EntityReference();
            EntityReference fucountry = new EntityReference();
            Entity parentCase = service.Retrieve(incident.LogicalName, incident.Id, new ColumnSet("jarvis_caseserviceline", "casetypecode", "jarvis_caselocation"));
            EntityCollection getMonitorActionsForCase = service.RetrieveMultiple(new FetchExpression(string.Format(MCS.Jarvis.CE.Plugins.Constants.FetchXmls.getMonitorActions, incident.Id, monitor.Id)));
            if (getMonitorActionsForCase != null && getMonitorActionsForCase.Entities.Count > 0)
            {
                // region Update Case
                Entity incidentToUpdate = new Entity(incident.LogicalName);
                incidentToUpdate.Id = incident.Id;
                if (getMonitorActionsForCase.Entities[0].Attributes.Contains("subject") && getMonitorActionsForCase.Entities[0].Attributes["subject"] != null)
                {
                    incidentToUpdate["jarvis_fucomment"] = (string)getMonitorActionsForCase.Entities[0].Attributes["subject"];
                }

                if (getMonitorActionsForCase.Entities[0].Attributes.Contains("prioritycode") && getMonitorActionsForCase.Entities[0].Attributes["prioritycode"] != null)
                {
                    OptionSetValue urgency = (OptionSetValue)getMonitorActionsForCase.Entities[0].Attributes["prioritycode"];
                    int priorityCode = 1;
                    if (urgency.Value == 0)
                    {
                        priorityCode = 1;
                    }

                    if (urgency.Value == 1)
                    {
                        priorityCode = 2;
                    }

                    if (urgency.Value == 2)
                    {
                        priorityCode = 3;
                    }

                    incidentToUpdate["prioritycode"] = new OptionSetValue(priorityCode);
                }

                if (getMonitorActionsForCase.Entities[0].Attributes.Contains("jarvis_monitorsortorder") && getMonitorActionsForCase.Entities[0].Attributes["jarvis_monitorsortorder"] != null)
                {
                    incidentToUpdate["jarvis_monitorsortorder"] = (int)getMonitorActionsForCase.Entities[0].Attributes["jarvis_monitorsortorder"];
                }

                if (getMonitorActionsForCase.Entities[0].Attributes.Contains("actualstart") && getMonitorActionsForCase.Entities[0].Attributes["actualstart"] != null)
                {
                    if (getMonitorActionsForCase.Entities[0].Attributes.Contains("jarvis_followuptime") && getMonitorActionsForCase.Entities[0].Attributes["jarvis_followuptime"] != null)
                    {
                        string time = (string)getMonitorActionsForCase.Entities[0].Attributes["jarvis_followuptime"];
                        if (time != null)
                        {
                            time = time.Replace(":", string.Empty);
                            string[] substrings = SplitString(time, 2);
                            TimeSpan futime = new TimeSpan(Convert.ToInt16(substrings[0]), Convert.ToInt16(substrings[1]), 0);
                            DateTime actualStart = ((DateTime)getMonitorActionsForCase.Entities[0].Attributes["actualstart"]).Date;
                            int timeZoneCode = 105;
                            actualStart = this.RetrieveLocalTimeFromUTCTime(service, actualStart, timeZoneCode);
                            actualStart = actualStart.Date;
                            actualStart = actualStart.Add(futime);

                            incidentToUpdate["jarvis_futimestamp"] = actualStart;
                        }
                        else
                        {
                            incidentToUpdate["jarvis_futimestamp"] = (DateTime)getMonitorActionsForCase.Entities[0].Attributes["actualstart"];
                        }
                    }
                    else
                    {
                        incidentToUpdate["jarvis_futimestamp"] = (DateTime)getMonitorActionsForCase.Entities[0].Attributes["actualstart"];
                    }
                }

                if (getMonitorActionsForCase.Entities[0].Attributes.Contains("jarvis_followuplanguage") && getMonitorActionsForCase.Entities[0].Attributes["jarvis_followuplanguage"] != null)
                {
                    incidentToUpdate["jarvis_fulanguage"] = (EntityReference)getMonitorActionsForCase.Entities[0].Attributes["jarvis_followuplanguage"];
                }

                if (getMonitorActionsForCase.Entities[0].Attributes.Contains("jarvis_followupcountry") && getMonitorActionsForCase.Entities[0].Attributes["jarvis_followupcountry"] != null)
                {
                    incidentToUpdate["jarvis_fucountry"] = (EntityReference)getMonitorActionsForCase.Entities[0].Attributes["jarvis_followupcountry"];
                    fucountry = (EntityReference)getMonitorActionsForCase.Entities[0].Attributes["jarvis_followupcountry"];
                    if (parentCase.Attributes.Contains("casetypecode") && parentCase.Attributes["casetypecode"] != null)
                    {
                        casetypecode = (OptionSetValue)parentCase.Attributes["casetypecode"];
                        if (parentCase.Attributes.Contains("jarvis_caselocation") && parentCase.Attributes["jarvis_caselocation"] != null)
                        {
                            caselocation = (OptionSetValue)parentCase.Attributes["jarvis_caselocation"];
                            if (parentCase.Attributes.Contains("jarvis_caseserviceline") && parentCase.Attributes["jarvis_caseserviceline"] != null)
                            {
                                caseserviceline = (EntityReference)parentCase.Attributes["jarvis_caseserviceline"];
                                EntityCollection caseSkill = service.RetrieveMultiple(new FetchExpression(string.Format(MCS.Jarvis.CE.Plugins.Constants.FetchXmls.getMonitorSkill, caselocation.Value, casetypecode.Value, fucountry.Id, caseserviceline.Id)));
                                if (caseSkill != null && caseSkill.Entities.Count > 0)
                                {
                                    incidentToUpdate["jarvis_fumonitorskill"] = caseSkill.Entities[0].ToEntityReference();
                                }
                            }
                        }
                    }
                }

                incidentToUpdate["jarvis_fulinknew"] = getMonitorActionsForCase.Entities[0].ToEntityReference();
                EntityReference createdBy = (EntityReference)getMonitorActionsForCase.Entities[0].Attributes["createdby"];
                incidentToUpdate["jarvis_fuuser"] = createdBy;

                service.Update(incidentToUpdate);

                // endregion
            }
            else
            {
                // region Update Case to Null
                Entity incidentToUpdate = new Entity(incident.LogicalName);
                incidentToUpdate.Id = incident.Id;
                incidentToUpdate["jarvis_fucomment"] = null;
                incidentToUpdate["prioritycode"] = null;
                incidentToUpdate["jarvis_monitorsortorder"] = null;
                incidentToUpdate["jarvis_futimestamp"] = null;
                incidentToUpdate["jarvis_fulanguage"] = null;
                incidentToUpdate["jarvis_fulinknew"] = null;
                incidentToUpdate["jarvis_followuptime"] = null;
                incidentToUpdate["jarvis_fuuser"] = null;
                incidentToUpdate["jarvis_fucountry"] = null;
                incidentToUpdate["jarvis_fumonitorskill"] = null;
                service.Update(incidentToUpdate);

                // endregion
            }
        }

        /// <summary>
        /// automate Monitor Creation.
        /// </summary>
        /// <param name="incident">incident details.</param>
        /// <param name="fucomment">fu Comment.</param>
        /// <param name="urgency">Urgency details.</param>
        /// <param name="actionType">action Type.</param>
        /// <param name="minutes">minutes details.</param>
        /// <param name="closeAction">close Action.</param>
        /// <param name="service">Org service.</param>
        public void AutomateMonitorCreation(Entity incident, string fucomment, int urgency, int actionType, int minutes, string closeAction, IOrganizationService service)
        {
            bool isAutomation = false;
            bool isCreate = false;
            isAutomation = CrmHelper.GetAutomationConfig(service, MCS.Jarvis.CE.Plugins.Constants.JarvisConfiguration.Automationmonitoraction, null);
            if (isAutomation)
            {
                EntityCollection getMonitorActionsForCase = service.RetrieveMultiple(new FetchExpression(string.Format(MCS.Jarvis.CE.Plugins.Constants.FetchXmls.getDraftPassOutActions, incident.Id, fucomment, urgency)));

                if (getMonitorActionsForCase.Entities.Count > 0)
                {
                    if (getMonitorActionsForCase.Entities[0].Attributes.Contains("statuscode") && getMonitorActionsForCase.Entities[0].Attributes["statuscode"] != null)
                    {
                        OptionSetValue statusCode = (OptionSetValue)getMonitorActionsForCase.Entities[0].Attributes["statuscode"];

                        if (statusCode.Value == 2)
                        {
                            // Completed
                            isCreate = true;
                        }
                    }
                }
                else
                {
                    isCreate = true;
                }

                if (isCreate)
                {
                    // region Create Monitor Action
                    Entity monitor = new Entity("jarvis_casemonitoraction");
                    monitor["subject"] = fucomment;
                    monitor["prioritycode"] = new OptionSetValue(urgency);
                    int timeZoneCode = 105;
                    DateTime actualStart = this.RetrieveLocalTimeFromUTCTime(service, DateTime.UtcNow, timeZoneCode);
                    monitor["actualstart"] = actualStart; // jarvis_followuptime date
                    monitor["regardingobjectid"] = incident.ToEntityReference();
                    monitor["jarvis_followuptime"] = actualStart.AddMinutes(minutes).ToString("HH:mm");
                    monitor["jarvis_followuptimestamp"] = DateTime.UtcNow;
                    monitor["jarvis_actiontype"] = new OptionSetValue(actionType);
                    monitor["jarvis_monitorsortorder"] = actionType;
                    service.Create(monitor);

                    // endregion
                }
            }
        }

        /// <summary>
        /// automate Monitor Creation Time.
        /// </summary>
        /// <param name="incident">incident details.</param>
        /// <param name="fucomment">fu Comment.</param>
        /// <param name="urgency">urgency details.</param>
        /// <param name="actionType">action Type.</param>
        /// <param name="minutes">minutes details.</param>
        /// <param name="closeAction">close Action.</param>
        /// <param name="actualStartEstimated">actual Start Estimated.</param>
        /// <param name="actualStartMins">actual Start Mins.</param>
        /// <param name="service">Org service.</param>
        public void AutomateMonitorCreationTime(Entity incident, string fucomment, int urgency, int actionType, int minutes, string closeAction, DateTime actualStartEstimated, int actualStartMins, IOrganizationService service)
        {
            bool isCreate = false;
            bool isUpdate = false;
            bool isAutomation = false;
            DateTime addFuTime = DateTime.UtcNow;
            addFuTime = actualStartEstimated.AddMinutes(actualStartMins);
            isAutomation = CrmHelper.GetAutomationConfig(service, MCS.Jarvis.CE.Plugins.Constants.JarvisConfiguration.Automationmonitoraction, null);
            if (isAutomation)
            {
                EntityCollection getMonitorActionsForCase = service.RetrieveMultiple(new FetchExpression(string.Format(MCS.Jarvis.CE.Plugins.Constants.FetchXmls.getOpenPassOutActions, incident.Id, fucomment, urgency)));

                if (getMonitorActionsForCase.Entities.Count > 0)
                {
                    if (getMonitorActionsForCase.Entities[0].Attributes.Contains("statuscode") && getMonitorActionsForCase.Entities[0].Attributes["statuscode"] != null)
                    {
                        OptionSetValue statusCode = (OptionSetValue)getMonitorActionsForCase.Entities[0].Attributes["statuscode"];

                        if (statusCode.Value == 2)
                        {
                            // Completed
                            isCreate = true;
                        }
                        else if (statusCode.Value == 1)
                        {
                            // open
                            isUpdate = true;
                        }
                    }
                }
                else
                {
                    isCreate = true;
                }

                if (isCreate)
                {
                    // region Create Monitor Action
                    Entity monitor = new Entity("jarvis_casemonitoraction");
                    monitor["subject"] = fucomment;
                    monitor["prioritycode"] = new OptionSetValue(urgency);

                    int timeZoneCode = 105;
                    DateTime actualStart = this.RetrieveLocalTimeFromUTCTime(service, actualStartEstimated, timeZoneCode);
                    monitor["actualstart"] = actualStart.AddMinutes(actualStartMins); // jarvis_followuptime date
                    monitor["regardingobjectid"] = incident.ToEntityReference();
                    monitor["jarvis_followuptime"] = actualStart.AddMinutes(actualStartMins).ToString("HH:mm");
                    monitor["jarvis_actiontype"] = new OptionSetValue(actionType);
                    monitor["jarvis_followuptimestamp"] = addFuTime;
                    monitor["jarvis_monitorsortorder"] = actionType;
                    service.Create(monitor);

                    // endregion
                }

                if (isUpdate)
                {
                    Entity monitor = new Entity("jarvis_casemonitoraction");
                    monitor.Id = getMonitorActionsForCase.Entities[0].Id;
                    int timeZoneCode = 105;
                    DateTime actualStart = this.RetrieveLocalTimeFromUTCTime(service, actualStartEstimated, timeZoneCode);
                    monitor["actualstart"] = actualStart.AddMinutes(actualStartMins); // jarvis_followuptime date
                    monitor["jarvis_followuptime"] = actualStart.AddMinutes(actualStartMins).ToString("HH:mm");
                    service.Update(monitor);
                }
            }
        }

        /// <summary>
        /// automate Close Monitor Actions.
        /// </summary>
        /// <param name="incident">incident details.</param>
        /// <param name="fucomment">fu Comment.</param>
        /// <param name="urgency">Urgency details.</param>
        /// <param name="closeAction">close Action.</param>
        /// <param name="service">Org service.</param>
        public void AutomateCloseMonitorActions(Entity incident, string fucomment, int urgency, string closeAction, IOrganizationService service)
        {
            // region Close Monitor Action
            if (!string.IsNullOrEmpty(closeAction))
            {
                string[] splitString = closeAction.Split(',');
                foreach (string str in splitString)
                {
                    EntityCollection getMonitorActionsForCase = service.RetrieveMultiple(new FetchExpression(string.Format(MCS.Jarvis.CE.Plugins.Constants.FetchXmls.getCloseActions, incident.Id, str, urgency)));
                    if (getMonitorActionsForCase != null && getMonitorActionsForCase.Entities.Count > 0)
                    {
                        foreach (var item in getMonitorActionsForCase.Entities)
                        {
                            Entity monitorClose = new Entity("jarvis_casemonitoraction");
                            monitorClose.Id = item.Id;
                            monitorClose["statecode"] = new OptionSetValue(1);
                            monitorClose["statuscode"] = new OptionSetValue(2);
                            service.Update(monitorClose);
                        }
                    }
                }
            }

            // endregion
        }

        /// <summary>
        /// update Monitor Process.
        /// </summary>
        /// <param name="initiatingUserID">initiating User ID.</param>
        /// <param name="monitor">monitor details.</param>
        /// <param name="incident">incident details.</param>
        /// <param name="service">Org service.</param>
        /// <param name="adminService">admin Service.</param>
        public void UpdateMonitorProcess(Guid initiatingUserID, Entity monitor, EntityReference incident, IOrganizationService service, IOrganizationService adminService)
        {
            if (monitor.Attributes.Contains("statecode") && ((OptionSetValue)monitor.Attributes["statecode"]).Value == 1)
            {
                string body = string.Format("'{0}' Action completed {1}", monitor.Attributes["subject"], monitor.Attributes["createdon"]);
                bool notifyAll = true;
                CaseNotifiaction casenotifiaction = new CaseNotifiaction();
                casenotifiaction.FrameNotifiaction(initiatingUserID, adminService, incident.Id, new Guid(new byte[16]), MCS.Jarvis.CE.Plugins.Constants.NotificationData.MonitorActionComplete, body, notifyAll);
            }
        }

        /// <summary>
        /// Update Monitor Action Type Process.
        /// </summary>
        /// <param name="monitor">Monitor details.</param>
        /// <param name="urgency">urgency details.</param>
        /// <param name="subject">subject details.</param>
        /// <param name="incident">incident details.</param>
        /// <param name="service">Org service.</param>
        /// <param name="adminService">admin Service.</param>
        /// <param name="actualStart">actual Start date.</param>
        /// <param name="followuptime">follow up time.</param>
        /// <param name="tracingService">tracing Service.</param>
        public void UpdateMonitorActionTypeProcess(Entity monitor, OptionSetValue urgency, string subject, Entity incident, IOrganizationService service, IOrganizationService adminService, DateTime actualStart, string followuptime, ITracingService tracingService)
        {
            bool typeCheck = false;

            // Identify Action Type
            // region Identify Action Type
            if (urgency.Value == 0)
            {
                // High
                monitor["jarvis_actiontype"] = new OptionSetValue(1);
                monitor["jarvis_monitorsortorder"] = 1;
                typeCheck = true;
            }

            if (!typeCheck)
            {
                if (incident.Attributes.Contains("statuscode") && incident.Attributes["statuscode"] != null)
                {
                    OptionSetValue caseStatus = (OptionSetValue)incident.Attributes["statuscode"];
                    if (caseStatus.Value == 10)
                    {
                        // Case Opening
                        monitor["jarvis_actiontype"] = new OptionSetValue(2);
                        monitor["jarvis_monitorsortorder"] = 2;
                        typeCheck = true;
                    }
                }
            }

            if (!typeCheck)
            {
                if (incident.Attributes.Contains("statuscode") && incident.Attributes["statuscode"] != null)
                {
                    OptionSetValue caseStatus = (OptionSetValue)incident.Attributes["statuscode"];
                    if (caseStatus.Value == 20)
                    {
                        // GOP
                        monitor["jarvis_actiontype"] = new OptionSetValue(3);
                        monitor["jarvis_monitorsortorder"] = 3;
                        typeCheck = true;
                    }
                }
            }

            if (!typeCheck)
            {
                if (incident.Attributes.Contains("statuscode") && incident.Attributes["statuscode"] != null)
                {
                    OptionSetValue caseStatus = (OptionSetValue)incident.Attributes["statuscode"];
                    if (caseStatus.Value == 30)
                    {
                        // Pass Out
                        monitor["jarvis_actiontype"] = new OptionSetValue(4);
                        monitor["jarvis_monitorsortorder"] = 4;
                        typeCheck = true;
                    }
                }
            }

            if (!typeCheck)
            {
                if (subject.Contains("GOP+"))
                {
                    // GOP+
                    monitor["jarvis_actiontype"] = new OptionSetValue(5);
                    monitor["jarvis_monitorsortorder"] = 5;
                    typeCheck = true;
                }
            }

            if (!typeCheck)
            {
                if (subject.Contains("DESC"))
                {
                    // Decision
                    monitor["jarvis_actiontype"] = new OptionSetValue(6);
                    monitor["jarvis_monitorsortorder"] = 6;
                    typeCheck = true;
                }
            }

            if (!typeCheck)
            {
                if (incident.Attributes.Contains("isescalated") && incident.Attributes["isescalated"] != null)
                {
                    bool isescalated = (bool)incident.Attributes["isescalated"];
                    if (isescalated)
                    {
                        // Pass Out
                        monitor["jarvis_actiontype"] = new OptionSetValue(7);
                        monitor["jarvis_monitorsortorder"] = 7;
                        typeCheck = true;
                    }
                }
            }

            if (!typeCheck)
            {
                if (urgency.Value == 1)
                {
                    // Medium
                    monitor["jarvis_actiontype"] = new OptionSetValue(8);
                    monitor["jarvis_monitorsortorder"] = 8;
                    typeCheck = true;
                }
            }

            if (!typeCheck)
            {
                if (urgency.Value == 2)
                {
                    // Standard
                    monitor["jarvis_actiontype"] = new OptionSetValue(17);
                    monitor["jarvis_monitorsortorder"] = 17;
                }
            }

            // endregion
            // region update Follow Up Timestamp
            tracingService.Trace("update Follow Up Timestamp");
            int timeZoneCode = 105;
            if ((monitor.Attributes.Contains("actualstart") || monitor.Attributes.Contains("jarvis_followuptime")) && !string.IsNullOrEmpty(actualStart.ToString()))
            {
                if (!string.IsNullOrEmpty(followuptime))
                {
                    followuptime = followuptime.Replace(":", string.Empty);
                    string[] substrings = SplitString(followuptime, 2);
                    TimeSpan futime = new TimeSpan(Convert.ToInt16(substrings[0]), Convert.ToInt16(substrings[1]), 0);
                    var currentuserTimeCode = this.RetrieveCurrentUsersTimeZoneSettings(service);
                    tracingService.Trace("current user time zone code " + currentuserTimeCode?.ToString());
                    if (currentuserTimeCode != null)
                    {
                        timeZoneCode = (int)currentuserTimeCode;
                    }

                    actualStart = this.RetrieveLocalTimeFromUTCTime(service, actualStart, timeZoneCode);
                    actualStart = actualStart.Date;
                    actualStart = actualStart.Add(futime);
                    monitor["jarvis_followuptimestamp"] = actualStart;
                }
                else
                {
                    monitor["jarvis_followuptimestamp"] = (DateTime)monitor.Attributes["actualstart"];
                }
            }

            // endregion update Follow Up Timestamp
            if (!string.IsNullOrEmpty(subject))
            {
                // Country
                // region Populate Country
                if (incident.Attributes.Contains("jarvis_country") && incident.Attributes["jarvis_country"] != null)
                {
                    EntityReference caseCountry = (EntityReference)incident.Attributes["jarvis_country"];
                    monitor["jarvis_followupcountry"] = caseCountry;
                }

                EntityCollection getLanguageforMOCaseEN = service.RetrieveMultiple(new FetchExpression(string.Format(MCS.Jarvis.CE.Plugins.Constants.FetchXmls.getLanguageMO, "ENG")));
                if (getLanguageforMOCaseEN.Entities.Count > 0)
                {
                    // throw new InvalidPluginExecutionException((string)getLanguageforMOCase.Entities[0].Attributes["jarvis_name"]);
                    monitor["jarvis_followuplanguage"] = new EntityReference(getLanguageforMOCaseEN.Entities[0].LogicalName, getLanguageforMOCaseEN.Entities[0].Id);
                }

                string fuComment = subject;
                string[] countryCode = fuComment?.Split(' ');
                if (countryCode != null && countryCode.Length > 1)
                {
                    string code = countryCode[1];
                    string languageCode = countryCode[0];
                    EntityCollection getMonitorActionsForCase = service.RetrieveMultiple(new FetchExpression(string.Format(MCS.Jarvis.CE.Plugins.Constants.FetchXmls.getCountryMO, code)));
                    EntityCollection getLanguageforMOCase = service.RetrieveMultiple(new FetchExpression(string.Format(MCS.Jarvis.CE.Plugins.Constants.FetchXmls.getLanguageMO, languageCode)));
                    if (getMonitorActionsForCase.Entities.Count > 0)
                    {
                        monitor["jarvis_followupcountry"] = new EntityReference(getMonitorActionsForCase.Entities[0].LogicalName, getMonitorActionsForCase.Entities[0].Id);
                    }

                    if (getLanguageforMOCase.Entities.Count > 0)
                    {
                        // throw new InvalidPluginExecutionException((string)getLanguageforMOCase.Entities[0].Attributes["jarvis_name"]);
                        monitor["jarvis_followuplanguage"] = new EntityReference(getLanguageforMOCase.Entities[0].LogicalName, getLanguageforMOCase.Entities[0].Id);
                    }
                }
            }

            // endregion
        }

        /// <summary>
        /// Retrieve Current Users Time Zone Settings.
        /// </summary>
        /// <param name="service">Org Service.</param>
        /// <returns>Time zone code.</returns>
        public int? RetrieveCurrentUsersTimeZoneSettings(IOrganizationService service)
        {
            int tz = 105;
            EntityCollection currentUserSettings = service.RetrieveMultiple(
            new QueryExpression("usersettings")
            {
                NoLock = true,
                ColumnSet = new ColumnSet("localeid", "timezonecode"),
                Criteria = new FilterExpression
                {
                    Conditions =
                {
                     new ConditionExpression("systemuserid", ConditionOperator.EqualUserId),
                },
                },
            });
            //// trace.Trace($"User Setting Collection Counr: {currentUserSettings.Entities.Count}");
            if (currentUserSettings != null && currentUserSettings.Entities.Count > 0)
            {
                Entity currentUserSetting = currentUserSettings.Entities[0];
                //// trace.Trace($"Timezonecode: {currentUserSetting.Attributes["timezonecode"]}");
                if (currentUserSetting.Attributes["timezonecode"] != null)
                {
                    tz = (int)currentUserSetting.Attributes["timezonecode"];
                }
            }

            return tz;
        }

        /// <summary>
        /// Retrieve Local Time From UTC Time.
        /// </summary>
        /// <param name="service">Org Service.</param>
        /// <param name="utcTime">UTC Time.</param>
        /// <param name="timeZoneCode">time Zone Code.</param>
        /// <returns>Date time.</returns>
        public DateTime RetrieveLocalTimeFromUTCTime(IOrganizationService service, DateTime utcTime, int? timeZoneCode)
        {
            if (!timeZoneCode.HasValue)
            {
                return utcTime;
            }

            var request = new LocalTimeFromUtcTimeRequest
            {
                TimeZoneCode = timeZoneCode.Value,
                UtcTime = utcTime.ToUniversalTime(),
            };

            var response = (LocalTimeFromUtcTimeResponse)service.Execute(request);

            return response.LocalTime;
        }

        /// <summary>
        /// update Next MO For Case.
        /// </summary>
        /// <param name="service">Org service.</param>
        /// <param name="incident">incident details.</param>
        /// <param name="tracingService">tracing Service.</param>
        /// <param name="actionFlag">Action Flag.</param>
        public void UpdateNextMOForCase(IOrganizationService service, Entity incident, ITracingService tracingService, bool actionFlag = false)
        {
            //// 633887

            tracingService.Trace($"Action flag is {actionFlag}");
            tracingService.Trace("Enter into Setting next Monitor Action for Case.");
            EntityCollection getCurrentMonitorActions = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.GetCurrentMonitorActions, incident.Id)));
            Entity parentCase = service.Retrieve(incident.LogicalName, incident.Id, new ColumnSet("jarvis_caseserviceline", "casetypecode", "jarvis_caselocation", "statuscode", "jarvis_fulinknew", "jarvis_fumonitorskill"));
            if (getCurrentMonitorActions != null && getCurrentMonitorActions.Entities.Count > 0)
            {
                tracingService.Trace("Set Monitor Action to Case based on Current.");
                this.SetMonitorActionToCase(service, incident, parentCase, getCurrentMonitorActions, actionFlag, tracingService);
            }
            else
            {
                EntityCollection getUpcomingMonitorActions = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.GetUpcomingMonitorActions, incident.Id)));
                if (getUpcomingMonitorActions != null && getUpcomingMonitorActions.Entities.Count > 0)
                {
                    tracingService.Trace("Set Monitor Action to Case based on Upcoming.");
                    this.SetMonitorActionToCase(service, incident, parentCase, getUpcomingMonitorActions, actionFlag, tracingService);
                }
                else
                {
                    tracingService.Trace("Set Monitor Action to case based on Case Closure");
                    //// 3.1.3
                    if (parentCase.Attributes.Contains("statuscode") && parentCase.Attributes["statuscode"] != null && ((OptionSetValue)parentCase.Attributes["statuscode"]).Value == (int)90 && parentCase.Attributes.Contains("jarvis_fulinknew") && parentCase.Attributes["jarvis_fulinknew"] != null)
                    {
                        EntityCollection getDefaultYYYLanguage = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.GetYYYLanguage)));
                        EntityCollection getDefaultYYCountry = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.GetYYCountry)));
                        Entity incidentToUpdate = new Entity(incident.LogicalName);
                        incidentToUpdate.Id = incident.Id;
                        incidentToUpdate["jarvis_fucomment"] = "YYY YY Case Closure";
                        if (parentCase.Attributes.Contains("jarvis_fulinknew") && parentCase.Attributes["jarvis_fulinknew"] != null)
                        {
                            incidentToUpdate["jarvis_fulinknew"] = null;
                        }

                        if (parentCase.Attributes.Contains("jarvis_fumonitorskill") && parentCase.Attributes["jarvis_fumonitorskill"] != null)
                        {
                            incidentToUpdate["jarvis_fumonitorskill"] = null;
                        }

                        incidentToUpdate["jarvis_futimestamp"] = DateTime.UtcNow;
                        if (getDefaultYYYLanguage != null && getDefaultYYYLanguage.Entities.Count > 0)
                        {
                            incidentToUpdate["jarvis_fulanguage"] = getDefaultYYYLanguage.Entities[0].ToEntityReference();
                        }

                        if (getDefaultYYCountry != null && getDefaultYYCountry.Entities.Count > 0)
                        {
                            incidentToUpdate["jarvis_fucountry"] = getDefaultYYCountry.Entities[0].ToEntityReference();
                        }

                        incidentToUpdate["prioritycode"] = new OptionSetValue(3);
                        incidentToUpdate["jarvis_monitorsortorder"] = (int)17;
                        service.Update(incidentToUpdate);
                        tracingService.Trace("Case Upate done");
                    }
                }
            }
        }

        /// <summary>
        /// Close Monitor Actions.
        /// </summary>
        /// <param name="incident">incident details.</param>
        /// <param name="service">Org service.</param>
        /// <param name="tracingService">tracing Service.</param>
        public void CloseMonitorActions(Entity incident, IOrganizationService service, ITracingService tracingService)
        {
            EntityCollection getMonitorActionsForCase = service.RetrieveMultiple(new FetchExpression(string.Format(MCS.Jarvis.CE.Plugins.Constants.FetchXmls.getActiveActions, incident.Id)));
            tracingService.Trace("Retrieved open MA count" + getMonitorActionsForCase.Entities.Count);
            if (getMonitorActionsForCase != null && getMonitorActionsForCase.Entities.Count > 0)
            {
                foreach (var item in getMonitorActionsForCase.Entities)
                {
                    Entity monitorClose = new Entity("jarvis_casemonitoraction");
                    monitorClose.Id = item.Id;
                    monitorClose["statecode"] = new OptionSetValue(1);
                    monitorClose["statuscode"] = new OptionSetValue(2);
                    service.Update(monitorClose);
                }
            }
        }

        /// <summary>
        /// Check Unapproved GOP.
        /// </summary>
        /// <param name="gop">gop entity.</param>
        /// <param name="incident">incident entity.</param>
        /// <param name="service">Org service.</param>
        /// <param name="tracingService">Tracing service.</param>
        /// <returns>boolean value.</returns>
        public bool CheckUnapprovedGOP(Entity gop, Entity incident, IOrganizationService service, ITracingService tracingService)
        {
            EntityCollection gopCollection = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getGOPsForCase, incident.Id)));
            var upapprovedGOP = gopCollection.Entities.Where(g => ((OptionSetValue)g.Attributes["jarvis_gopapproval"]).Value != 334030001);
            if (upapprovedGOP.Any())
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Get Country Code.
        /// </summary>
        /// <param name="customer">incident entity.</param>
        /// <param name="service">Org service.</param>
        /// <param name="tracingService">Tracing service.</param>
        /// <returns>country code.</returns>
        public string GetCountryCode(EntityReference customer, IOrganizationService service, ITracingService tracingService)
        {
            string isCountryCode = string.Empty;
            Entity accountObj = service.Retrieve(customer.LogicalName, customer.Id, new ColumnSet("jarvis_language", "jarvis_address1_country"));
            if (accountObj.Attributes.Contains("jarvis_address1_country") && accountObj.Attributes["jarvis_address1_country"] != null)
            {
                EntityReference customerCountry = (EntityReference)accountObj.Attributes["jarvis_address1_country"];
                Entity country = service.Retrieve(customerCountry.LogicalName, customerCountry.Id, new ColumnSet("jarvis_iso2countrycode"));
                if (country.Attributes.Contains("jarvis_iso2countrycode") && country.Attributes["jarvis_iso2countrycode"] != null)
                {
                    isCountryCode = (string)country.Attributes["jarvis_iso2countrycode"];
                }
            }

            return isCountryCode;
        }

        /// <summary>
        /// Get Country Code.
        /// </summary>
        /// <param name="customer">incident entity.</param>
        /// <param name="service">Org service.</param>
        /// <param name="tracingService">Tracing service.</param>
        /// <returns>country and language code.</returns>
        public string GetCountryAndLanguageCode(EntityReference customer, IOrganizationService service, ITracingService tracingService)
        {
            string countryAndLanguageCode = string.Empty;
            string isoLangCode = string.Empty;
            string isCountryCode = string.Empty;
            Entity accountObj = service.Retrieve(customer.LogicalName, customer.Id, new ColumnSet("jarvis_language", "jarvis_address1_country"));
            if (accountObj.Attributes.Contains("jarvis_address1_country") && accountObj.Attributes["jarvis_address1_country"] != null)
            {
                EntityReference customerCountry = (EntityReference)accountObj.Attributes["jarvis_address1_country"];
                Entity country = service.Retrieve(customerCountry.LogicalName, customerCountry.Id, new ColumnSet("jarvis_iso2countrycode"));
                if (country.Attributes.Contains("jarvis_iso2countrycode") && country.Attributes["jarvis_iso2countrycode"] != null)
                {
                    isCountryCode = (string)country.Attributes["jarvis_iso2countrycode"];
                }
            }

            if (accountObj.Attributes.Contains("jarvis_language") && accountObj.Attributes["jarvis_language"] != null)
            {
                EntityReference language = (EntityReference)accountObj.Attributes["jarvis_language"];
                Entity hdlanguage = service.Retrieve(language.LogicalName, language.Id, new ColumnSet("jarvis_iso3languagecode6392t"));
                if (hdlanguage.Attributes.Contains("jarvis_iso3languagecode6392t") && hdlanguage.Attributes["jarvis_iso3languagecode6392t"] != null)
                {
                    isoLangCode = (string)hdlanguage.Attributes["jarvis_iso3languagecode6392t"];
                }
            }

            if (!string.IsNullOrEmpty(isoLangCode) || !string.IsNullOrEmpty(isCountryCode))
            {
                countryAndLanguageCode = isoLangCode + " " + isCountryCode;
            }

            return countryAndLanguageCode;
        }

        /// <summary>
        /// Split String.
        /// </summary>
        /// <param name="input">input details.</param>
        /// <param name="chunkSize">chunk Size.</param>
        /// <returns>chunks string.</returns>
        private static string[] SplitString(string input, int chunkSize)
        {
            int length = input.Length;
            int numChunks = (length + chunkSize - 1) / chunkSize;
            string[] chunks = new string[numChunks];

            for (int i = 0; i < numChunks; i++)
            {
                int startIndex = i * chunkSize;
                int endIndex = Math.Min(startIndex + chunkSize, length);
                chunks[i] = input.Substring(startIndex, endIndex - startIndex);
            }

            return chunks;
        }

        private void SetMonitorActionToCase(IOrganizationService service, Entity incident, Entity parentCase, EntityCollection getMonitorActionsForCase, bool actionFlag, ITracingService tracingService)
        {
            tracingService.Trace($"action flag is {actionFlag}");
            tracingService.Trace("Enter into setting monitor action to case.");

            OptionSetValue casetypecode;
            OptionSetValue caselocation;
            EntityReference caseserviceline;
            EntityReference fucountry;

            // region Update Case
            Entity incidentToUpdate = new Entity(incident.LogicalName);
            incidentToUpdate.Id = incident.Id;
            if (getMonitorActionsForCase.Entities[0].Attributes.Contains("subject") && getMonitorActionsForCase.Entities[0].Attributes["subject"] != null)
            {
                incidentToUpdate["jarvis_fucomment"] = (string)getMonitorActionsForCase.Entities[0].Attributes["subject"];
                tracingService.Trace("FU Comment");
            }

            if (getMonitorActionsForCase.Entities[0].Attributes.Contains("prioritycode") && getMonitorActionsForCase.Entities[0].Attributes["prioritycode"] != null)
            {
                OptionSetValue urgency = (OptionSetValue)getMonitorActionsForCase.Entities[0].Attributes["prioritycode"];
                int priorityCode = 1;
                if (urgency.Value == 0)
                {
                    priorityCode = 1;
                }

                if (urgency.Value == 1)
                {
                    priorityCode = 2;
                }

                if (urgency.Value == 2)
                {
                    priorityCode = 3;
                }

                incidentToUpdate["prioritycode"] = new OptionSetValue(priorityCode);
                tracingService.Trace("prioritycode " + priorityCode + string.Empty);
            }

            if (getMonitorActionsForCase.Entities[0].Attributes.Contains("jarvis_monitorsortorder") && getMonitorActionsForCase.Entities[0].Attributes["jarvis_monitorsortorder"] != null)
            {
                incidentToUpdate["jarvis_monitorsortorder"] = (int)getMonitorActionsForCase.Entities[0].Attributes["jarvis_monitorsortorder"];
                tracingService.Trace("SortOrder");
            }

            if (getMonitorActionsForCase.Entities[0].Attributes.Contains("jarvis_followuptimestamp") && getMonitorActionsForCase.Entities[0].Attributes["jarvis_followuptimestamp"] != null)
            {
                incidentToUpdate["jarvis_futimestamp"] = (DateTime)getMonitorActionsForCase.Entities[0].Attributes["jarvis_followuptimestamp"];
            }

            if (getMonitorActionsForCase.Entities[0].Attributes.Contains("jarvis_followuplanguage") && getMonitorActionsForCase.Entities[0].Attributes["jarvis_followuplanguage"] != null)
            {
                incidentToUpdate["jarvis_fulanguage"] = (EntityReference)getMonitorActionsForCase.Entities[0].Attributes["jarvis_followuplanguage"];
                tracingService.Trace("FU Language");
            }

            if (getMonitorActionsForCase.Entities[0].Attributes.Contains("jarvis_followupcountry") && getMonitorActionsForCase.Entities[0].Attributes["jarvis_followupcountry"] != null)
            {
                incidentToUpdate["jarvis_fucountry"] = (EntityReference)getMonitorActionsForCase.Entities[0].Attributes["jarvis_followupcountry"];
                fucountry = (EntityReference)getMonitorActionsForCase.Entities[0].Attributes["jarvis_followupcountry"];
                tracingService.Trace("FU Country");
                if (parentCase.Attributes.Contains("casetypecode") && parentCase.Attributes["casetypecode"] != null)
                {
                    casetypecode = (OptionSetValue)parentCase.Attributes["casetypecode"];
                    if (parentCase.Attributes.Contains("jarvis_caselocation") && parentCase.Attributes["jarvis_caselocation"] != null)
                    {
                        caselocation = (OptionSetValue)parentCase.Attributes["jarvis_caselocation"];
                        if (parentCase.Attributes.Contains("jarvis_caseserviceline") && parentCase.Attributes["jarvis_caseserviceline"] != null)
                        {
                            caseserviceline = (EntityReference)parentCase.Attributes["jarvis_caseserviceline"];
                            EntityCollection caseSkill = service.RetrieveMultiple(new FetchExpression(string.Format(MCS.Jarvis.CE.Plugins.Constants.FetchXmls.getMonitorSkill, caselocation.Value, casetypecode.Value, fucountry.Id, caseserviceline.Id)));
                            if (caseSkill != null && caseSkill.Entities.Count > 0)
                            {
                                incidentToUpdate["jarvis_fumonitorskill"] = caseSkill.Entities[0].ToEntityReference();
                            }
                        }
                    }
                }
            }

            incidentToUpdate["jarvis_fulinknew"] = getMonitorActionsForCase.Entities[0].ToEntityReference();
            tracingService.Trace("FU link");
            EntityReference createdBy = (EntityReference)getMonitorActionsForCase.Entities[0].Attributes["createdby"];
            incidentToUpdate["jarvis_fuuser"] = createdBy;
            tracingService.Trace("FU User");
            if (actionFlag)
            {
                tracingService.Trace("Action Flag is true");
                if (parentCase.Attributes.Contains("jarvis_fulinknew") && parentCase.Attributes["jarvis_fulinknew"] != null)
                {
                    EntityReference currentMonitorAction = (EntityReference)parentCase.Attributes["jarvis_fulinknew"];
                    Entity updateMonitorAction = getMonitorActionsForCase.Entities[0];
                    if (updateMonitorAction.Id != null && currentMonitorAction.Id != null && currentMonitorAction.Id != updateMonitorAction.Id)
                    {
                        tracingService.Trace("Update through PA as Action Flag is True && FUlinknew is different for the current");
                        service.Update(incidentToUpdate);
                    }
                }
                else
                {
                    tracingService.Trace("Update through plugin as Action Flag is True && FUlinknew is empty before");
                    service.Update(incidentToUpdate);
                }

                tracingService.Trace("PA setting current is completed.");
            }
            else
            {
                tracingService.Trace("Update through plugin as Action Flag is False");
                service.Update(incidentToUpdate);
            }

            tracingService.Trace("Case Upate done");
        }
    }
}
