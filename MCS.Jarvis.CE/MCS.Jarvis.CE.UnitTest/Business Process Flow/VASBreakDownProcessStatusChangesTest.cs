//-----------------------------------------------------------------------
// <copyright file="VASBreakDownProcessStatusChangesTest.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace MCS.Jarvis.CE.UnitTest
{
    using System;
    using System.Collections.Generic;
    using MCS.Jarvis.CE.Plugins;
    using MCS.Jarvis.CE.Plugins.Business_Process_Flow;
    using MCS.Jarvis.CE.Plugins.CaseContact;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;
    using static MCS.Jarvis.CE.BusinessProcessesShared.Helpers.Constants;

    /// <summary>
    /// VAS BreakDown Process Status Changes Test.
    /// </summary>
    [TestClass]
    public class VASBreakDownProcessStatusChangesTest : UnitTestBase
    {
        /// <summary>
        /// VAS BreakDown Process Status Changes Test Method.
        /// </summary>
        // [TestMethod]
        public void VASBreakDownProcessStatusChangesTestMethod()
        {
            var vasProcess = new Entity("jarvis_vasbreakdownprocess");
            EntityReference activeStage = new EntityReference();
            activeStage.Id = Guid.NewGuid();
            activeStage.Name = MCS.Jarvis.CE.BusinessProcessesShared.Helpers.Constants.Incident.bpfStage2;
            vasProcess["activestageid"] = activeStage;
            vasProcess["statuscode"] = new OptionSetValue(2);
            vasProcess["bpf_incidentid"] = new EntityReference("incident", Guid.NewGuid());

            Entity vasProcessPreImg = new Entity("jarvis_vasbreakdownprocess");
            EntityReference activeStageImg = new EntityReference();
            activeStageImg.Id = Guid.NewGuid();
            activeStageImg.Name = MCS.Jarvis.CE.BusinessProcessesShared.Helpers.Constants.Incident.bpfStage1;
            vasProcessPreImg["activestageid"] = activeStageImg;
            vasProcessPreImg["statuscode"] = new OptionSetValue(2);
            vasProcessPreImg["bpf_incidentid"] = new EntityReference("incident", Guid.NewGuid());

            Entity vasProcessPostImg = new Entity("jarvis_vasbreakdownprocess");
            vasProcessPostImg["activestageid"] = Guid.NewGuid();
            vasProcessPostImg["statuscode"] = new OptionSetValue(2);
            vasProcessPostImg["bpf_incidentid"] = Guid.NewGuid();

            EntityReference users = new EntityReference("systemuser", Guid.NewGuid());
            users.Name = "firstname lastname";

            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
            {
                new KeyValuePair<string, object>("Target", vasProcess),
            };
            this.PluginExecutionContext.MessageNameGet = () => "Update";
            this.PluginExecutionContext.StageGet = () => 40;
            this.PluginExecutionContext.PreEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PreImage", vasProcessPreImg },
                };
            };
            this.PluginExecutionContext.PostEntityImagesGet = () => new EntityImageCollection { new KeyValuePair<string, Entity>("PostImage", vasProcessPostImg) };

            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "incident")
                {
                    Entity inc = new Entity("incident");
                    inc.Id = guid;
                    inc["createdon"] = DateTime.Now;
                    inc["createdby"] = users;
                    inc["caseorigincode"] = new OptionSetValue(334030002);
                    inc["statecode"] = new OptionSetValue(0);
                    inc["casetypecode"] = new OptionSetValue(2);
                    inc["jarvis_mercuriusstatus"] = new OptionSetValue(200);
                    inc["statuscode"] = new OptionSetValue(20);
                    inc["jarvis_querydecisioncatagory"] = new EntityReference("jarvis_querydecisioncatagory", Guid.NewGuid());
                    inc["jarvis_casestatusupdate"] = DateTime.Now;
                    inc["jarvis_automationcriteriamet"] = true;
                    inc["jarvis_hdrd"] = true;
                    inc["ownerid"] = users;
                    return inc;
                }

                return null;
            };

            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();
                if (query.GetType().Name == "FetchExpression")
                {
                    if ((query as FetchExpression).Query.Contains("<entity name='jarvis_configurationjarvis'>"))
                    {
                        Entity configResult = new Entity("jarvis_configurationjarvis");
                        configResult["jarvis_automationtranslation"] = true;
                        configResult["jarvis_automationmonitoraction"] = true;
                        configResult["jarvis_automationcasestatuschange"] = true;
                        configResult["jarvis_automationreleasecase"] = new OptionSetValue(1);
                        configResult["createdon"] = DateTime.UtcNow;
                        configResult.Id = Guid.NewGuid();
                        result.Entities.Add(configResult);
                    }
                }

                return result;
            };

            VASBreakDownProcessStatusChanges plugin = new VASBreakDownProcessStatusChanges();
            plugin.Execute(this.ServiceProvider);
        }
    }
}
