using MCS.Jarvis.CE.Plugins.IncidentNature;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCS.Jarvis.CE.UnitTest
{
    /// <summary>
    /// Incident Nature Pre Operation Delete Sync Test.
    /// </summary>
    [TestClass]
    public class IncidentNaturePreOperationDeleteSyncTest : UnitTestBase
    {
        /// <summary>
        /// Incident Nature Pre Operation Delete Sync Delete.
        /// </summary>
        [TestMethod]
        public void IncidentNaturePreOperationDeleteSync_Delete()
        {
            var targetEntity = new Entity("jarvis_incidentnature");
            targetEntity.Attributes["jarvis_case"] = new EntityReference("incident", Guid.NewGuid());
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection { new KeyValuePair<string, object>("Relationship", new Relationship("Relationname")) };
            this.PluginExecutionContext.MessageNameGet = () => "Delete";
            this.PluginExecutionContext.StageGet = () => 20;
            this.PluginExecutionContext.UserIdGet = () => Guid.NewGuid();
            IncidentNaturePreOperationDeleteSync plugin = new IncidentNaturePreOperationDeleteSync();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Incident Nature Pre Operation Delete Sync associate with Incident.
        /// </summary>
        [TestMethod]
        public void IncidentNaturePreOperationDeleteSync_Associate_Incident()
        {
            var targetEntity = new Entity("jarvis_incidentnature");
            targetEntity.Attributes["jarvis_case"] = new EntityReference("incident", Guid.NewGuid());
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection { new KeyValuePair<string, object>("Relationship", new Relationship("jarvis_Incident_jarvis_IncidentNature_jarvis_Inc")), new KeyValuePair<string, object>("Target", new EntityReference("incident", Guid.NewGuid())) };
            this.PluginExecutionContext.MessageNameGet = () => "associate";
            this.PluginExecutionContext.StageGet = () => 20;
            this.PluginExecutionContext.UserIdGet = () => Guid.NewGuid();
            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "incident")
                {
                    Entity inc = new Entity("incident");
                    inc.Id = guid;
                    inc.Attributes["jarvis_country"] = new EntityReference("jarvis_country", "Name", "test");
                    return inc;
                }

                return null;
            };
            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();
                if ((query as FetchExpression).Query.Contains("<entity name='jarvis_incidentnature'>"))
                {
                    Entity incidentType = new Entity("jarvis_incidentnature");
                    incidentType.Id = Guid.NewGuid();
                    incidentType["jarvis_name"] = "DEU";
                    incidentType["jarvis_incidenttype"] = new EntityReference("incident", Guid.NewGuid());
                    incidentType["createdon"] = DateTime.UtcNow;
                    result.Entities.Add(incidentType);
                }

                return result;
            };
            IncidentNaturePreOperationDeleteSync plugin = new IncidentNaturePreOperationDeleteSync();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Incident Nature Pre Operation Delete Sync associate with Incident.
        /// </summary>
        [TestMethod]
        public void IncidentNaturePreOperationDeleteSync_Associate_IncidentNature()
        {
            var targetEntity = new Entity("jarvis_incidentnature");
            targetEntity.Attributes["jarvis_case"] = new EntityReference("incident", Guid.NewGuid());
            EntityReferenceCollection entityReferences = new EntityReferenceCollection();
            entityReferences.Add(new EntityReference("incident", Guid.NewGuid()));
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection {
                new KeyValuePair<string, object>("Relationship", new Relationship("jarvis_Incident_jarvis_IncidentNature_jarvis_Inc")), 
                new KeyValuePair<string, object>("Target", new EntityReference("jarvis_incidentnature", Guid.NewGuid())),
                new KeyValuePair<string, object>("RelatedEntities", entityReferences),
            };
            this.PluginExecutionContext.MessageNameGet = () => "associate";
            this.PluginExecutionContext.StageGet = () => 20;
            this.PluginExecutionContext.UserIdGet = () => Guid.NewGuid();
            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "incident")
                {
                    Entity inc = new Entity("incident");
                    inc.Id = guid;
                    inc.Attributes["jarvis_country"] = new EntityReference("jarvis_country", "Name", "test");
                    return inc;
                }

                return null;
            };
            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();
                if ((query as FetchExpression).Query.Contains("<entity name='jarvis_incidentnature'>"))
                {
                    Entity incidentType = new Entity("jarvis_incidentnature");
                    incidentType.Id = Guid.NewGuid();
                    incidentType["jarvis_name"] = "DEU";
                    incidentType["jarvis_incidenttype"] = new EntityReference("incident", Guid.NewGuid());
                    incidentType["createdon"] = DateTime.UtcNow;
                    result.Entities.Add(incidentType);
                }

                return result;
            };
            IncidentNaturePreOperationDeleteSync plugin = new IncidentNaturePreOperationDeleteSync();
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Incident Nature Pre Operation Delete Sync disassociate.
        /// </summary>
        [TestMethod]
        public void IncidentNaturePreOperationDeleteSync_Disassociate()
        {
            var targetEntity = new Entity("jarvis_incidentnature");
            targetEntity.Attributes["jarvis_case"] = new EntityReference("incident", Guid.NewGuid());
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection { new KeyValuePair<string, object>("Relationship", new Relationship("Relationname")) };
            this.PluginExecutionContext.MessageNameGet = () => "disassociate";
            this.PluginExecutionContext.StageGet = () => 20;
            this.PluginExecutionContext.UserIdGet = () => Guid.NewGuid();
            IncidentNaturePreOperationDeleteSync plugin = new IncidentNaturePreOperationDeleteSync();
            plugin.Execute(this.ServiceProvider);
        }
    }
}
