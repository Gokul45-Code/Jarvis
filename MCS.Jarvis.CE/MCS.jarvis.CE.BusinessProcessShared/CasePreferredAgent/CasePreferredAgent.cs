// <copyright file="CasePreferredAgent.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MCS.Jarvis.CE.BusinessProcessShared.CasePreferredAgent
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using MCS.Jarvis.CE.BusinessProcessShared.AppNotification;
    using MCS.Jarvis.CE.Plugins;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;

    /// <summary>
    /// Case Preferred Agent.
    /// </summary>
    public class CasePreferredAgent
    {
        /// <summary>
        /// Create Case Preferred Agent. update "jarvis preferred vas operator" on Case entity on creation of CasePreferredAgent.
        /// </summary>
        /// <param name="initiatingUserID">initiating User ID.</param>
        /// <param name="casePreferredAgent">case Preferred Agent.</param>
        /// <param name="caseId">case Id.</param>
        /// <param name="service">Org service.</param>
        /// <param name="adminService">admin service.</param>
        public void CreateCasePreferredAgent(Guid initiatingUserID, Entity casePreferredAgent, Guid caseId, IOrganizationService service, IOrganizationService adminService)
        {
            if (casePreferredAgent.Attributes.Contains(Constants.PreferredAgent.jarvisPreferredVasOperator))
            {
                Entity casePreferredVasOperator = service.Retrieve("incident", caseId, new ColumnSet(Constants.PreferredAgent.jarvisPreferredVasOperator));
                Guid userId = ((EntityReference)casePreferredAgent.Attributes[Constants.PreferredAgent.jarvisPreferredVasOperator]).Id;
                Entity user = service.Retrieve("systemuser", userId, new ColumnSet("fullname"));

                if (user != null && user.Attributes.Contains("fullname") && user.Attributes["fullname"] != null)
                {
                    Entity updateCase = new Entity("incident");
                    if (casePreferredVasOperator.Attributes.Contains(Constants.PreferredAgent.jarvisPreferredVasOperator) && casePreferredVasOperator.Attributes[Constants.PreferredAgent.jarvisPreferredVasOperator] != null)
                    {
                        string existingVasAgents = string.Format("{0},{1}", casePreferredVasOperator.Attributes[Constants.PreferredAgent.jarvisPreferredVasOperator], user.Attributes["fullname"]);
                        updateCase.Attributes[Constants.PreferredAgent.jarvisPreferredVasOperator] = existingVasAgents;
                        updateCase.Attributes["incidentid"] = caseId;
                    }
                    else
                    {
                        updateCase.Attributes[Constants.PreferredAgent.jarvisPreferredVasOperator] = user.Attributes["fullname"];
                        updateCase.Attributes["incidentid"] = caseId;
                    }

                    if (updateCase.Attributes.Contains(Constants.PreferredAgent.jarvisPreferredVasOperator))
                    {
                        service.Update(updateCase);
                        string body = Constants.NotificationData.PreferredAgentAddedbody;
                        bool notifyAll = false;
                        CaseNotifiaction casenotifiaction = new CaseNotifiaction();
                        casenotifiaction.FrameNotifiaction(initiatingUserID, adminService, caseId, userId, Constants.NotificationData.PreferredAgentAdded, body, notifyAll);
                    }
                }
            }
        }

        /// <summary>
        /// Delete Case Preferred Agent.
        /// </summary>
        /// <param name="initiatingUserID">initiating User ID.</param>
        /// <param name="casePreferredAgent">case Preferred Agent.</param>
        /// <param name="caseId">case Id.</param>
        /// <param name="service">Org service.</param>
        /// <param name="adminService">admin service.</param>
        public void DeleteCasePreferredAgent(Guid initiatingUserID, Entity casePreferredAgent, Guid caseId, IOrganizationService service, IOrganizationService adminService)
        {
            Entity casePreferredVasOperator = service.Retrieve("incident", caseId, new ColumnSet(Constants.PreferredAgent.jarvisPreferredVasOperator));
            Guid userId = ((EntityReference)casePreferredAgent.Attributes[Constants.PreferredAgent.jarvisPreferredVasOperator]).Id;
            Entity user = service.Retrieve("systemuser", userId, new ColumnSet("fullname"));
            if (user != null && user.Attributes.Contains("fullname") && user.Attributes["fullname"] != null)
            {
                Entity updateCase = new Entity("incident");
                if (casePreferredVasOperator.Attributes.Contains(Constants.PreferredAgent.jarvisPreferredVasOperator) && casePreferredVasOperator.Attributes[Constants.PreferredAgent.jarvisPreferredVasOperator] != null)
                {
                    string[] existingVasAgents = casePreferredVasOperator.Attributes[Constants.PreferredAgent.jarvisPreferredVasOperator].ToString().Split(',');

                    if (existingVasAgents.Contains(user.Attributes["fullname"]))
                    {
                        List<string> vasAgentList = new List<string>(existingVasAgents);
                        vasAgentList.Remove(user.Attributes["fullname"].ToString());

                        if (vasAgentList.Count > 0)
                        {
                            var vasAgent = string.Join(",", vasAgentList);
                            updateCase.Attributes[Constants.PreferredAgent.jarvisPreferredVasOperator] = vasAgent;
                            updateCase.Attributes["incidentid"] = caseId;
                        }
                        else
                        {
                            updateCase.Attributes[Constants.PreferredAgent.jarvisPreferredVasOperator] = null;
                            updateCase.Attributes["incidentid"] = caseId;
                        }
                    }
                }

                if (updateCase.Attributes.Contains(Constants.PreferredAgent.jarvisPreferredVasOperator))
                {
                    service.Update(updateCase);
                }

                string body = Constants.NotificationData.PreferredAgentRemovedbody;
                bool notifyAll = false;
                CaseNotifiaction casenotifiaction = new CaseNotifiaction();
                casenotifiaction.FrameNotifiaction(initiatingUserID, adminService, caseId, userId, Constants.NotificationData.PreferredAgentRemoved, body, notifyAll);
            }
        }
    }
}
