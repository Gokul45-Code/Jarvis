// <copyright file="CaseNotifiaction.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MCS.Jarvis.CE.BusinessProcessShared.AppNotification
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using MCS.Jarvis.CE.BusinessProcessesShared.Helpers;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;

    /// <summary>
    /// Case Notification.
    /// </summary>
    public class CaseNotifiaction
    {
        /// <summary>
        /// Frame Notification.
        /// </summary>
        /// <param name="initiatinguserID">initiating user ID.</param>
        /// <param name="service">Service object.</param>
        /// <param name="caseId">Case Id.</param>
        /// <param name="ownerId">Owner Id.</param>
        /// <param name="message">Message to be send.</param>
        /// <param name="body">Body of message.</param>
        /// <param name="multiplenotification">multiple notification.</param>
        public void FrameNotifiaction(Guid initiatinguserID, IOrganizationService service, Guid caseId, Guid ownerId, string message, string body, bool multiplenotification)
        {
            string keyMatch = message.ToUpper();
            EntityCollection config = service.RetrieveMultiple(new FetchExpression(Constants.FetchXmls.getConfigJarvisUrl));
            if (config.Entities.Any())
            {
                EntityCollection case2Customer = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getCaseCustomerdata, caseId)));
                if (case2Customer.Entities.Any())
                {
                    List<LinkData> linkdata = new List<LinkData>();
                    var organissationUrl = config.Entities.First().Attributes[Constants.CaseNotification.jarvisIntegrationMapping];

                    string casetitle = case2Customer.Entities.First().Contains("title") ? case2Customer.Entities.First().Attributes["title"].ToString() : "View Case";
                    string caseUrl = string.Format("{0}incident&id={1}", organissationUrl, caseId);
                    linkdata.Add(new LinkData
                    {
                        Title = casetitle,
                        Url = caseUrl,
                    });
                    if (case2Customer.Entities.First().Contains(Constants.CaseNotification.Case2AccountAccountid))
                    {
                        string customertitle = case2Customer.Entities.First().Contains(Constants.CaseNotification.Case2AccountAccountid) ? (string)((AliasedValue)case2Customer.Entities.First().Attributes[Constants.CaseNotification.Case2AccountName]).Value : "View Customer";
                        string customerurl = string.Format("{0}account&id={1}", organissationUrl, (Guid)((AliasedValue)case2Customer.Entities.First().Attributes[Constants.CaseNotification.Case2AccountAccountid]).Value);
                        linkdata.Add(new LinkData
                        {
                            Title = customertitle,
                            Url = customerurl,
                        });
                    }

                    List<Guid> ids = new List<Guid>();
                    Entity caseWorker = service.Retrieve("incident", caseId, new ColumnSet("ownerid"));
                    if (caseWorker != null && caseWorker.Attributes.Contains("ownerid"))
                    {
                        ids.Add(((EntityReference)caseWorker.Attributes["ownerid"]).Id);
                    }

                    if (multiplenotification)
                    {
                        EntityCollection userids = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getAllVasOperatorIds, caseId)));
                        if (userids.Entities.Any())
                        {
                            foreach (Entity entity in userids.Entities)
                            {
                                if (entity.Attributes.Contains("jarvis_preferredvasoperator"))
                                {
                                    if (!ids.Contains(((EntityReference)entity.Attributes["jarvis_preferredvasoperator"]).Id))
                                    {
                                        ids.Add(((EntityReference)entity.Attributes["jarvis_preferredvasoperator"]).Id);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (ownerId != Guid.Empty)
                        {
                            if (!ids.Contains(ownerId))
                            {
                                ids.Clear();
                                ids.Add(ownerId);
                            }
                        }
                    }

                    if (keyMatch.Contains("MONITOR"))
                    {
                        ids.Remove(initiatinguserID);
                    }

                    if (message == "Case stage changed")
                    {
                        ids.Remove(initiatinguserID);
                    }

                    if (linkdata.Count > 0 && ids != null)
                    {
                        InAppNotification inAppNotification = new InAppNotification();
                        inAppNotification.CreateInAppnotification(service, ids, linkdata, message, body);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Link Data.
    /// </summary>
    public class LinkData
    {
        /// <summary>
        /// Gets or sets title property.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets url property.
        /// </summary>
        public string Url { get; set; }
    }
}
