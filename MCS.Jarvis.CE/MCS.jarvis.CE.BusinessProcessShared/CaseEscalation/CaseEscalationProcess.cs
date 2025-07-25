// <copyright file="CaseEscalationProcess.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MCS.Jarvis.CE.BusinessProcessShared.CaseEscalation
{
    using Microsoft.Xrm.Sdk;
    using static MCS.Jarvis.CE.BusinessProcessesShared.Helpers.Constants;

    /// <summary>
    /// Case Escalation Process.
    /// </summary>
    public class CaseEscalationProcess
    {
        /// <summary>
        /// Create Escalation Process.
        /// </summary>
        /// <param name="contextIncident"> context of Incident.</param>
        /// <param name="postImgIncident">Post Incident Image.</param>
        /// <param name="service">Org Service.</param>
        /// <param name="tracingService">Tracing Service.</param>
        public void CreateEscalationProcess(Entity contextIncident, Entity postImgIncident, IOrganizationService service, ITracingService tracingService)
        {
            tracingService.Trace("Entered Method Create Case Escalation/De-Escalation");
            string subject;
            string mainCategory = postImgIncident.GetAttributeValue<EntityReference>(Incident.JarvisEscalationMainCategory)?.Name;
            string subCategory = postImgIncident.GetAttributeValue<EntityReference>(Incident.JarvisEscalationSubcategory)?.Name;
            string remark = postImgIncident.GetAttributeValue<string>(Incident.JarvisEscalationRemarks);
            if (contextIncident.Attributes.Contains(Incident.IsEscalated) && contextIncident.Attributes[Incident.IsEscalated] != null)
            {
                if (bool.Parse(contextIncident.Attributes[Incident.IsEscalated].ToString()) == true)
                {
                    subject = postImgIncident.GetAttributeValue<string>(Incident.Title)?.ToString() + " escalated " + ((!string.IsNullOrEmpty(mainCategory) || !string.IsNullOrEmpty(subCategory) || !string.IsNullOrEmpty(remark)) ? "with " : string.Empty) + (!string.IsNullOrEmpty(mainCategory) ? (mainCategory + " Category ") : string.Empty) + ((!string.IsNullOrEmpty(mainCategory) && !string.IsNullOrEmpty(subCategory)) ? "+ " : string.Empty) + (!string.IsNullOrEmpty(subCategory) ? (subCategory + " Subcategory ") : string.Empty) + (((!string.IsNullOrEmpty(mainCategory) || !string.IsNullOrEmpty(subCategory)) && !string.IsNullOrEmpty(remark)) ? "and " : string.Empty) + (!string.IsNullOrEmpty(remark) ? ("Remarks: " + remark) : string.Empty);
                }
                else
                {
                    subject = postImgIncident.GetAttributeValue<string>(Incident.Title)?.ToString() + " de-escalated";
                }
            }
            else
            {
                subject = postImgIncident.GetAttributeValue<string>(Incident.Title)?.ToString() + " escalated " + ((!string.IsNullOrEmpty(mainCategory) || !string.IsNullOrEmpty(subCategory) || !string.IsNullOrEmpty(remark)) ? "with " : string.Empty) + (!string.IsNullOrEmpty(mainCategory) ? (mainCategory + " Category ") : string.Empty) + ((!string.IsNullOrEmpty(mainCategory) && !string.IsNullOrEmpty(subCategory)) ? "+ " : string.Empty) + (!string.IsNullOrEmpty(subCategory) ? (subCategory + " Subcategory ") : string.Empty) + (((!string.IsNullOrEmpty(mainCategory) || !string.IsNullOrEmpty(subCategory)) && !string.IsNullOrEmpty(remark)) ? "and " : string.Empty) + (!string.IsNullOrEmpty(remark) ? ("Remarks: " + remark) : string.Empty);
            }

            tracingService.Trace("Create Case Escalation/De-Escalation subject is " + subject);

            // Create Escalation Action
            Entity escalation = new Entity(General.JarvisCaseEscalation);
            escalation[CaseEscalation.Subject] = subject;
            escalation[CaseEscalation.RegardingObjectId] = contextIncident.ToEntityReference();
            escalation[CaseEscalation.JarvisMainCategory] = postImgIncident.GetAttributeValue<EntityReference>(Incident.JarvisEscalationMainCategory);
            escalation[CaseEscalation.JarvisSubCategory] = postImgIncident.GetAttributeValue<EntityReference>(Incident.JarvisEscalationSubcategory);
            escalation[CaseEscalation.JarvisRemark] = postImgIncident.GetAttributeValue<string>(Incident.JarvisEscalationRemarks);
            var escalateId = service.Create(escalation);
            escalation = new Entity(General.JarvisCaseEscalation);
            escalation.Id = escalateId;
            escalation[CaseEscalation.StateCode] = new OptionSetValue((int)EscalationActivitySatatus.Completed);
            service.Update(escalation);
        }
    }
}
