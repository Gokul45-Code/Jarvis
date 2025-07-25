using System;
using System.Runtime.Remoting.Services;
using System.ServiceModel;
using MCS.Jarvis.CE.BusinessProcessesShared.GOP;
using MCS.Jarvis.CE.BusinessProcessesShared.User;
using MCS.Jarvis.CE.Commons;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Plugins;

namespace MCS.Jarvis.CE.Plugins
{
   
    public class UserSubgridFilterPreOperationSync : PluginBase
    {

        public UserSubgridFilterPreOperationSync() : base(typeof(UserUpdatePostOperationUserProfileAsync))
        {

        }



        public override void ExecuteCrmPlugin(LocalPluginContext localcontext)
        {
            ITracingService traceService = localcontext.TracingService;
            try
            {

                IPluginExecutionContext context = localcontext.PluginExecutionContext;
                IOrganizationService orgService = localcontext.OrganizationService;
                if (context.Depth > 1)
                {
                    return;
                }
                

                // Check that all of the following conditions are true:
                //  1. plug-in is running synchronously
                //  3. plug-in is running on the 'RetrieveMultiple' event
                if (context.Mode == 0 && context.MessageName.Equals("RetrieveMultiple") && context.InputParameters.Contains("Query"))
                {

                    // The InputParameters collection contains all the data passed in the message request.

                    if (context.InputParameters["Query"] is QueryExpression)
                    {
                        QueryExpression qe = (QueryExpression)context.InputParameters["Query"];
                        if (qe.EntityName == "systemusers")
                        {
                            string currentCaseId = string.Empty;
                            ConditionExpression[] filters = qe.Criteria.Conditions.ToArray();
                            foreach (var filter in filters)
                            {
                                currentCaseId = filter.Values[0].ToString();
                            }
                            Entity parentCase = orgService.Retrieve(Case.EntityLogicalName, Guid.Parse(currentCaseId), new ColumnSet("jarvis_country", "jarvis_caseserviceline"));
                            EntityReference jarvisCountry = parentCase.Attributes.Contains(Case.Attributes.BreakdownCountry) ? (EntityReference)parentCase.Attributes[Case.Attributes.BreakdownCountry] : null;
                            EntityReference jarvisserviceline = parentCase.Attributes.Contains(Case.Attributes.ServiceLine) ? (EntityReference)parentCase.Attributes[Case.Attributes.ServiceLine] : null;

                            FetchXmlToQueryExpressionRequest req = new FetchXmlToQueryExpressionRequest();
                            req.FetchXml = "&lt;fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true'&gt;"
                                           + "&lt;entity name='systemuser'&gt;"
                                           + "&lt;attribute name='fullname' /&gt;"
                                           + "&lt;attribute name='systemuserid' /&gt;"
                                           + "&lt;attribute name='internalemailaddress' /&gt;"
                                           + "&lt;order attribute='fullname' descending='false' /&gt;"
                                           + "&lt;link-entity name='jarvis_teamprofiledetails' from='jarvis_user' to='systemuserid' link-type='inner' alias='ah'&gt;"
                                          + "&lt;filter type='and'&gt;"
                                           + "&lt;condition attribute='jarvis_country' operator='eq'  uitype='jarvis_country' value='" + jarvisCountry.Id + "' /&gt;"
                                           + "&lt;condition attribute='jarvis_serviceline' operator='eq'  uitype='jarvis_serviceline' value='" + jarvisserviceline.Id + "' /&gt;"
                                          + "&lt;/filter&gt;"
                                           + "&lt;/link-entity&gt;"
                                           + "&lt;/entity&gt;"
                                           + "&lt;/fetch&gt;";
                            FetchXmlToQueryExpressionResponse resp = (FetchXmlToQueryExpressionResponse)orgService.Execute(req);
                            context.InputParameters["Query"] = resp.Query;
                            // Get the QueryExpression from the property bag
                            //QueryExpression objQueryExpression = (QueryExpression)context.InputParameters["Query"];
                            //objQueryExpression.Distinct = true;

                            //// Calculate completed Revisiones
                            //string[] CompletedGuids = getAllOpenRevisionsGuid(service);

                            //// Add the filter using the N to N middle table 
                            //LinkEntity linkEntity1 = new LinkEntity();
                            //linkEntity1.JoinOperator = JoinOperator.Natural;
                            //linkEntity1.LinkFromEntityName = "L_controlmeasure";
                            //linkEntity1.LinkFromAttributeName = "L_controlmeasureid";
                            //linkEntity1.LinkToEntityName = "L_controlmeasure_hazardouseventrevision";
                            //linkEntity1.LinkToAttributeName = "L_controlmeasureid";

                            //// Condition : where hazarrevisionID is in the returned Guidvalues
                            //ConditionExpression statusCompletedCond = new ConditionExpression("L_hazardouseventrevisionid", ConditionOperator.In, CompletedGuids);

                            //linkEntity1.LinkCriteria.AddCondition(statusCompletedCond);

                            //objQueryExpression.LinkEntities.Add(linkEntity1);

                        }

                    }

                }
            }
            catch (Exception ex)
            {
                traceService.Trace(ex.Message);
                throw new InvalidPluginExecutionException("Error in GOP Operations " + ex.Message + "");
            }
        }







    }
}
