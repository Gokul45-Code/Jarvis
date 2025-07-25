// <copyright file="IncidentNature.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MCS.Jarvis.CE.BusinessProcessShared.Case
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using MCS.Jarvis.CE.BusinessProcessesShared.Helpers;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using static MCS.Jarvis.CE.BusinessProcessesShared.Helpers.Constants;

    /// <summary>
    /// Incident Nature helper class.
    /// </summary>
    public class Incidentnature
    {
        /// <summary>
        /// Associate Incident Nature.
        /// </summary>
        /// <param name="incident">Incident details.</param>
        /// <param name="service">Org Service.</param>
        /// <param name="vehicleid">Vehicle Id.</param>
        /// <param name="contextmessage">Context Message.</param>
        public void AssociateIncidentNature(Entity incident, IOrganizationService service, Guid vehicleid, string contextmessage)
        {
            // Retrive Vehicle-Fuel/PowerType from Vehicles
            Entity fuelPowerTypeRecord = service.Retrieve("jarvis_vehicle", vehicleid, new ColumnSet("jarvis_fuelpowertype"));

            if (fuelPowerTypeRecord.Attributes.Contains("jarvis_fuelpowertype"))
            {
                var incidentNatureReferences = new EntityReferenceCollection();
                Relationship relationship = new Relationship(Constants.IncidentNature.jarvisIncidentjarvisIncidentNaturejarvisInc);
                EntityReference fuelPowerTypeReference = (EntityReference)fuelPowerTypeRecord.Attributes["jarvis_fuelpowertype"];

                // Retrive IncidentNatures associated to Vehicle-Fuel/PowerType
                EntityCollection incidentNatureRecords = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getFuelPowerType, fuelPowerTypeReference.Id)));

                if (incidentNatureRecords.Entities.Count > 0)
                {
                    // Bug-532727 Avoid case Unsaved changes
                    // on update operation
                    if (contextmessage == "UPDATE")
                    {
                        this.UpdateIncidentNatue(incidentNatureReferences, incidentNatureRecords, service, incident.Attributes["incidentid"]);
                    }

                    // Create operation
                    if (contextmessage == "CREATE")
                    {
                        this.CreateIncidentNatue(incidentNatureReferences, incidentNatureRecords);
                    }

                    if (incidentNatureReferences.Count > 0)
                    {
                        service.Associate("incident", (Guid)incident.Attributes["incidentid"], relationship, incidentNatureReferences);
                    }
                }
            }
        }

        /// <summary>
        /// Create Incident Nature.
        /// </summary>
        /// <param name="incidentNatureReferences">incident Nature References.</param>
        /// <param name="incidentNatureRecords">incident Nature Records.</param>
        /// <returns>Entity Reference Collection.</returns>
        public EntityReferenceCollection CreateIncidentNatue(EntityReferenceCollection incidentNatureReferences, EntityCollection incidentNatureRecords)
        {
            foreach (Entity item in incidentNatureRecords.Entities)
            {
                EntityReference incidentNatureId = new EntityReference(Constants.IncidentNature.jarvisIncidentNature, (Guid)((AliasedValue)item.Attributes[Constants.IncidentNature.IncidentNatureVehicleFuelPid_jarvisincidentnatureid]).Value);
                incidentNatureReferences.Add(incidentNatureId);
            }

            return incidentNatureReferences;
        }

        /// <summary>
        /// Update Incident Nature.
        /// </summary>
        /// <param name="incidentNatureReferences">incident Nature References.</param>
        /// <param name="incidentNatureRecords">incident Nature Records.</param>
        /// <param name="service">Org Service.</param>
        /// <param name="incident">Incident details.</param>
        /// <returns>Entity Reference Collection.</returns>
        public EntityReferenceCollection UpdateIncidentNatue(EntityReferenceCollection incidentNatureReferences, EntityCollection incidentNatureRecords, IOrganizationService service, object incident)
        {
            List<Guid> exitingIncidentNature = new List<Guid>();

            // Extracting Existing Incident Nature associated to case
            EntityCollection case2incidentNature = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getIncidentNature, incident)));
            if (case2incidentNature.Entities.Count > 0)
            {
                foreach (Entity item in case2incidentNature.Entities)
                {
                    if (item.Attributes.Contains(Constants.IncidentNature.IncidentIncidentNature_jarvisIincidentnatureid))
                    {
                        exitingIncidentNature.Add((Guid)((AliasedValue)item.Attributes[Constants.IncidentNature.IncidentIncidentNature_jarvisIincidentnatureid]).Value);
                    }
                }
            }

            foreach (Entity item in incidentNatureRecords.Entities)
            {
                // check latest incident natuure already associated
                if (!exitingIncidentNature.Contains((Guid)((AliasedValue)item.Attributes[Constants.IncidentNature.IncidentNatureVehicleFuelPid_jarvisincidentnatureid]).Value))
                {
                    EntityReference incidentNatureId = new EntityReference(Constants.IncidentNature.jarvisIncidentNature, (Guid)((AliasedValue)item.Attributes[Constants.IncidentNature.IncidentNatureVehicleFuelPid_jarvisincidentnatureid]).Value);
                    incidentNatureReferences.Add(incidentNatureId);
                }
            }

            return incidentNatureReferences;
        }

        /// <summary>
        /// Associate Incident Nature to Case.
        /// </summary>
        /// <param name="incident">Incident details.</param>
        /// <param name="service">Org Service.</param>
        /// <param name="contextmessage">Context Message.</param>
        public void AssociateIncidentNaturetoCase(Entity incident, IOrganizationService service, string contextmessage)
        {
            var incidentNatureReferences = new EntityReferenceCollection();
            var incidentNatureReferenceDissociate = new EntityReferenceCollection();
            Relationship relationship = new Relationship(Constants.IncidentNature.jarvisIncidentjarvisIncidentNaturejarvisInc);
            if (incident.Attributes.Contains("jarvis_incidentnature"))
            {
                var incidentNaturesMultiselect = JsonConvert.DeserializeObject(incident.Attributes["jarvis_incidentnature"].ToString());
                if (incidentNaturesMultiselect != null)
                {
                    foreach (JObject nature in (Newtonsoft.Json.Linq.JArray)incidentNaturesMultiselect)
                    {
                        Guid incidentnatid = Guid.NewGuid();
                        if (nature.TryGetValue("_id", StringComparison.OrdinalIgnoreCase, out JToken incidentNatureId) && incidentNatureId != null)
                        {
                            EntityReference incidentNatureObj = new EntityReference(Constants.IncidentNature.jarvisIncidentNature, (Guid)incidentNatureId);
                            incidentNatureReferences.Add(incidentNatureObj);
                            incidentnatid = (Guid)incidentNatureId;
                        }
                        else if (nature.TryGetValue("id", StringComparison.OrdinalIgnoreCase, out JToken incidentNatureId_id) && incidentNatureId_id != null)
                        {
                            EntityReference incidentNatureObj = new EntityReference(Constants.IncidentNature.jarvisIncidentNature, (Guid)incidentNatureId_id);
                            incidentNatureReferences.Add(incidentNatureObj);
                            incidentnatid = (Guid)incidentNatureId_id;
                        }
                    }
                }

                // Retrive IncidentNatures associated to Case
                EntityCollection incidentNatureRecords = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getIncidentNature, incident.Id)));
                if (incidentNatureRecords.Entities.Count > 0 && contextmessage == "UPDATE")
                {
                    foreach (Entity item in incidentNatureRecords.Entities)
                    {
                        if (item.Attributes.Contains(Constants.IncidentNature.IncidentIncidentNature_jarvisIincidentnatureid))
                        {
                            // Dissociate the Records that are not there in multiselect
                            Guid existingIncidentnatureId = (Guid)((AliasedValue)item.Attributes[Constants.IncidentNature.IncidentIncidentNature_jarvisIincidentnatureid]).Value;
                            var incidentNatureDissociate = incidentNatureReferences.ToList().Where(nature => nature.Id.Equals(existingIncidentnatureId)).ToList();
                            if (incidentNatureDissociate.Count < 1)
                            {
                                EntityReference incidentNatureObjtoDissociate = new EntityReference(Constants.IncidentNature.jarvisIncidentNature, existingIncidentnatureId);
                                incidentNatureReferenceDissociate.Add(incidentNatureObjtoDissociate);
                            }

                            // Associate only the records that are not associated
                            EntityReference incidentNatureObj = new EntityReference(Constants.IncidentNature.jarvisIncidentNature, existingIncidentnatureId);
                            incidentNatureReferences.Remove(incidentNatureObj);
                        }
                    }

                    if (incidentNatureReferences.Count > 0)
                    {
                        service.Associate("incident", (Guid)incident.Attributes["incidentid"], relationship, incidentNatureReferences);
                    }

                    if (incidentNatureReferenceDissociate.Count > 0)
                    {
                        service.Disassociate("incident", (Guid)incident.Attributes["incidentid"], relationship, incidentNatureReferenceDissociate);
                    }
                }
                else
                {
                    if (incidentNatureRecords.Entities.Count > 0)
                    {
                        foreach (Entity item in incidentNatureRecords.Entities)
                        {
                            if (item.Attributes.Contains(Constants.IncidentNature.IncidentIncidentNature_jarvisIincidentnatureid))
                            {
                                Guid existingIncidentnatureId = (Guid)((AliasedValue)item.Attributes[Constants.IncidentNature.IncidentIncidentNature_jarvisIincidentnatureid]).Value;

                                // Associate only the records that are not associated
                                EntityReference incidentNatureObj = new EntityReference(Constants.IncidentNature.jarvisIncidentNature, existingIncidentnatureId);
                                incidentNatureReferences.Remove(incidentNatureObj);
                            }
                        }
                    }

                    if (incidentNatureReferences.Count > 0)
                    {
                        service.Associate("incident", (Guid)incident.Attributes["incidentid"], relationship, incidentNatureReferences);
                    }
                }
            }
        }

        /// <summary>
        /// Associate Incident Nature From Sub grid To Multiselect.
        /// </summary>
        /// <param name="service">Org Service.</param>
        /// <param name="context">Context details.</param>
        public void AssociateIncidentNatureFromSubgridToMultiselect(IOrganizationService service, IPluginExecutionContext context)
        {
            // Get the “Relationship” Key from context
            if (context.InputParameters.Contains("Relationship"))
            {
                // Get the Relationship name for which this plugin fired
                string relationshipName = ((Relationship)context.InputParameters["Relationship"]).SchemaName;

                if (relationshipName.Equals(Constants.IncidentNature.jarvisIncidentjarvisIncidentNaturejarvisInc, StringComparison.CurrentCultureIgnoreCase))
                {
                    EntityReference targetEntity = (EntityReference)context.InputParameters["Target"];
                    if (targetEntity.LogicalName.Equals("incident"))
                    {
                        this.LinktoMultiSelect(targetEntity, service);
                    }
                    else if (targetEntity.LogicalName.Equals(Constants.IncidentNature.jarvisIncidentNature))
                    {
                        if (context.InputParameters.Contains("RelatedEntities") && context.InputParameters["RelatedEntities"] is EntityReferenceCollection)
                        {
                            var relatedEntities = context.InputParameters["RelatedEntities"] as EntityReferenceCollection;
                            if (relatedEntities.Count > 0 && relatedEntities[0].LogicalName.Equals("incident"))
                            {
                                EntityReference incidentRelated = relatedEntities[0];
                                this.LinktoMultiSelect(incidentRelated, service);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Link to Multi Select.
        /// </summary>
        /// <param name="incidentRelated">incident Related.</param>
        /// <param name="service">Org Service.</param>
        public void LinktoMultiSelect(EntityReference incidentRelated, IOrganizationService service)
        {
            // Associate Multiselect from Subgrid.
            EntityCollection incNatureCollectionCase = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getIncNatures, incidentRelated.Id)));
            string incidentStringArray = string.Empty;
            string incShadow = string.Empty;
            string incKnowledgeSearch = string.Empty;
            if (incNatureCollectionCase.Entities.Count > 0)
            {
                JArray incidentnature = new JArray();
                foreach (var item in incNatureCollectionCase.Entities)
                {
                    string name = (string)item.Attributes["jarvis_name"];
                    var incnatureObj = new JObject();
                    incnatureObj.Add("id", item.Id);
                    incnatureObj.Add("entityType", "jarvis_incidentnature");
                    incnatureObj.Add("name", name);
                    incidentStringArray = incnatureObj + ",";
                    if (incShadow != string.Empty)
                    {
                        incShadow = incShadow + string.Empty + name + string.Empty + ";";
                    }
                    else
                    {
                        incShadow = string.Empty + name + string.Empty + ";";
                    }

                    if (incKnowledgeSearch != string.Empty)
                    {
                        incKnowledgeSearch = incKnowledgeSearch + "'" + name + "'" + "|";
                    }
                    else
                    {
                        incKnowledgeSearch = "'" + name + "'" + "|";
                    }

                    incidentnature.Add(incnatureObj);
                }

                if (incidentnature.Count > 0)
                {
                    Entity caseToUpdate = new Entity(incidentRelated.LogicalName, incidentRelated.Id);
                    incidentStringArray = incidentnature.ToString();
                    var jsonData = JsonConvert.DeserializeObject(incidentStringArray);
                    string trimmedJsonString = JsonConvert.SerializeObject(jsonData, Formatting.None);
                    caseToUpdate["jarvis_incidentnature"] = trimmedJsonString;
                    Entity incident = service.Retrieve("incident", incidentRelated.Id, new ColumnSet("jarvis_country"));
                    if (incident.Attributes.Contains("jarvis_country") && incident.Attributes["jarvis_country"] != null)
                    {
                        EntityReference country = (EntityReference)incident.Attributes["jarvis_country"];
                        incKnowledgeSearch = incKnowledgeSearch + "'" + country.Name + "'";
                    }

                    incKnowledgeSearch = incKnowledgeSearch.Replace("'", "\"");
                    caseToUpdate["jarvis_incidentnatureshadow"] = incShadow;
                    caseToUpdate["jarvis_knowledgesearch"] = incKnowledgeSearch;

                    // Update Case
                    service.Update(caseToUpdate);
                }
                else
                {
                    // Update Case
                    Entity caseToUpdate = new Entity(incidentRelated.LogicalName);
                    caseToUpdate.Id = incidentRelated.Id;
                    caseToUpdate["jarvis_incidentnatureshadow"] = null;
                    caseToUpdate["jarvis_knowledgesearch"] = null;
                    service.Update(caseToUpdate);
                }
            }
        }

        /// <summary>
        /// Link to Multi Select From Vehicle.
        /// </summary>
        /// <param name="incidentRelated">incident Related.</param>
        /// <param name="vehicleid">Vehicle Id.</param>
        /// <param name="service">Org Service.</param>
        /// <returns>Entity details.</returns>
        public Entity LinktoMultiSelectFromVehicle(Entity incidentRelated, Guid vehicleid, IOrganizationService service)
        {
            // Retrive Vehicle-Fuel/PowerType from Vehicles Associate Multiselect
            string incidentStringArray = string.Empty;
            string incShadow = string.Empty;
            string incKnowledgeSearch = string.Empty;

            // Retrive Vehicle-Fuel/PowerType from Vehicles
            Entity fuelPowerTypeRecord = service.Retrieve("jarvis_vehicle", vehicleid, new ColumnSet("jarvis_fuelpowertype"));

            if (fuelPowerTypeRecord.Attributes.Contains("jarvis_fuelpowertype"))
            {
                EntityReference fuelPowerTypeReference = (EntityReference)fuelPowerTypeRecord.Attributes["jarvis_fuelpowertype"];

                // Retrive IncidentNatures associated to Vehicle-Fuel/PowerType
                EntityCollection incidentNatureRecords = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getFuelPowerType, fuelPowerTypeReference.Id)));

                if (incidentNatureRecords.Entities.Count > 0)
                {
                    JArray incidentnature = new JArray();
                    foreach (var item in incidentNatureRecords.Entities)
                    {
                        string name = (string)item.Attributes["jarvis_name"];
                        string vehiclefuelpowertypeid = ((Guid)item.Attributes["jarvis_vehiclefuelpowertypeid"]).ToString();
                        var incnatureObj = new JObject();
                        incnatureObj.Add("id", vehiclefuelpowertypeid);
                        incnatureObj.Add("entityType", "jarvis_incidentnature");
                        incnatureObj.Add("name", name);
                        incidentStringArray = incnatureObj + ",";
                        if (incShadow != string.Empty)
                        {
                            incShadow = incShadow + string.Empty + name + string.Empty + ";";
                        }
                        else
                        {
                            incShadow = string.Empty + name + string.Empty + ";";
                        }

                        if (incKnowledgeSearch != string.Empty)
                        {
                            incKnowledgeSearch = incKnowledgeSearch + "'" + name + "'" + "|";
                        }
                        else
                        {
                            incKnowledgeSearch = "'" + name + "'" + "|";
                        }

                        incidentnature.Add(incnatureObj);
                    }

                    incidentStringArray = incidentnature.ToString();
                    var jsonData = JsonConvert.DeserializeObject(incidentStringArray);
                    string trimmedJsonString = JsonConvert.SerializeObject(jsonData, Formatting.None);
                    incidentRelated["jarvis_incidentnature"] = trimmedJsonString;
                    if (incidentRelated.Attributes.Contains("jarvis_country") && incidentRelated.Attributes["jarvis_country"] != null)
                    {
                        EntityReference country = (EntityReference)incidentRelated.Attributes["jarvis_country"];
                        incKnowledgeSearch = incKnowledgeSearch + "'" + country.Name + "'";
                    }

                    incKnowledgeSearch = incKnowledgeSearch.Replace("'", "\"");
                    incidentRelated["jarvis_incidentnatureshadow"] = incShadow;
                    incidentRelated["jarvis_knowledgesearch"] = incKnowledgeSearch;
                }
            }

            return incidentRelated;
        }

        /// <summary>
        /// Associate Incident Nature to Case Multi select.
        /// </summary>
        /// <param name="incident">incident details.</param>
        /// <param name="caseImage">Case image.</param>
        /// <param name="service">Org service.</param>
        /// <param name="contextmessage">context message.</param>
        /// <returns>Entity details.</returns>
        public Entity AssociateIncidentNaturetoCaseMultiselect(Entity incident, Entity caseImage, IOrganizationService service, string contextmessage)
        {
            var incidentNatureReferences = new EntityReferenceCollection();
            string incShadow = string.Empty;
            string incKnowledgeSearch = string.Empty;
            string incidentStringArray = string.Empty;
            JArray incidentnature = new JArray();
            bool isIncidentNatureChanged = true;
            if (incident.Attributes.Contains("jarvis_incidentnature"))
            {
                var incidentNaturesMultiselect = JsonConvert.DeserializeObject(incident.Attributes["jarvis_incidentnature"].ToString());
                if (incidentNaturesMultiselect != null)
                {
                    ////if (incident.Attributes.Contains("jarvis_country") && incident.Attributes["jarvis_country"] != null)
                    ////{
                    ////    EntityReference countryReference = (EntityReference)incident.Attributes["jarvis_country"];
                    ////    Entity country = service.Retrieve(countryReference.LogicalName, countryReference.Id, new ColumnSet(JarvisCountry.Name));
                    ////    if (country != null && country.Contains(JarvisCountry.Name) && country.Attributes[JarvisCountry.Name] != null)
                    ////    {
                    ////        incKnowledgeSearch = incKnowledgeSearch + "'" + (string)country.Attributes[JarvisCountry.Name] + "'";
                    ////    }
                    ////}

                    foreach (JObject nature in (Newtonsoft.Json.Linq.JArray)incidentNaturesMultiselect)
                    {
                        Guid incidentnatid = Guid.NewGuid();
                        isIncidentNatureChanged = true;
                        if (nature.TryGetValue("_id", StringComparison.OrdinalIgnoreCase, out JToken incidentNatureId) && incidentNatureId != null)
                        {
                            EntityReference incidentNatureObj = new EntityReference(Constants.IncidentNature.jarvisIncidentNature, (Guid)incidentNatureId);
                            incidentNatureReferences.Add(incidentNatureObj);
                            incidentnatid = (Guid)incidentNatureId;
                        }
                        else if (nature.TryGetValue("id", StringComparison.OrdinalIgnoreCase, out JToken incidentNatureId_id) && incidentNatureId_id != null)
                        {
                            EntityReference incidentNatureObj = new EntityReference(Constants.IncidentNature.jarvisIncidentNature, (Guid)incidentNatureId_id);
                            incidentNatureReferences.Add(incidentNatureObj);
                            incidentnatid = (Guid)incidentNatureId_id;
                        }

                        if (nature.TryGetValue("_name", StringComparison.OrdinalIgnoreCase, out JToken name) && name != null)
                        {
                            var incnatureObj = new JObject();
                            incnatureObj.Add("id", incidentnatid);
                            incnatureObj.Add("entityType", "jarvis_incidentnature");
                            incnatureObj.Add("name", name);
                            incidentStringArray = incidentStringArray + name + ",";
                            incidentnature.Add(incnatureObj);
                            if (incShadow != string.Empty)
                            {
                                incShadow = incShadow + string.Empty + name + string.Empty + ";";
                            }
                            else
                            {
                                incShadow = string.Empty + name + string.Empty + ";";
                            }

                            ////if (incKnowledgeSearch != string.Empty)
                            ////{
                            ////    incKnowledgeSearch = incKnowledgeSearch + "'" + name + "'" + "|";
                            ////}
                            ////else
                            ////{
                            ////    incKnowledgeSearch = "'" + name + "'" + "|";
                            ////}
                        }

                        if (nature.TryGetValue("name", StringComparison.OrdinalIgnoreCase, out JToken name_IN) && name_IN != null)
                        {
                            var incnatureObj = new JObject();
                            incnatureObj.Add("id", incidentnatid);
                            incnatureObj.Add("entityType", "jarvis_incidentnature");
                            incnatureObj.Add("name", name_IN);
                            incidentStringArray = incidentStringArray + name_IN + ",";
                            incidentnature.Add(incnatureObj);
                            if (incShadow != string.Empty)
                            {
                                incShadow = incShadow + string.Empty + name_IN + string.Empty + ";";
                            }
                            else
                            {
                                incShadow = string.Empty + name_IN + string.Empty + ";";
                            }

                            ////if (incKnowledgeSearch != string.Empty)
                            ////{
                            ////    incKnowledgeSearch = incKnowledgeSearch + "'" + name_IN + "'" + "|";
                            ////}
                            ////else
                            ////{
                            ////    incKnowledgeSearch = "'" + name_IN + "'" + "|";
                            ////}
                        }
                    }
                }
            }
            else if (caseImage.Attributes.Contains("jarvis_incidentnature"))
            {
                var incidentNaturesMultiselect = JsonConvert.DeserializeObject(caseImage.Attributes["jarvis_incidentnature"].ToString());
                if (incidentNaturesMultiselect != null)
                {
                    foreach (JObject nature in (Newtonsoft.Json.Linq.JArray)incidentNaturesMultiselect)
                    {
                        Guid incidentnatid = Guid.NewGuid();
                        if (nature.TryGetValue("_id", StringComparison.OrdinalIgnoreCase, out JToken incidentNatureId) && incidentNatureId != null)
                        {
                            EntityReference incidentNatureObj = new EntityReference(Constants.IncidentNature.jarvisIncidentNature, (Guid)incidentNatureId);
                            incidentNatureReferences.Add(incidentNatureObj);
                            incidentnatid = (Guid)incidentNatureId;
                        }
                        else if (nature.TryGetValue("id", StringComparison.OrdinalIgnoreCase, out JToken incidentNatureId_id) && incidentNatureId_id != null)
                        {
                            EntityReference incidentNatureObj = new EntityReference(Constants.IncidentNature.jarvisIncidentNature, (Guid)incidentNatureId_id);
                            incidentNatureReferences.Add(incidentNatureObj);
                            incidentnatid = (Guid)incidentNatureId_id;
                        }

                        if (nature.TryGetValue("_name", StringComparison.OrdinalIgnoreCase, out JToken name) && name != null)
                        {
                            var incnatureObj = new JObject();
                            incnatureObj.Add("id", incidentnatid);
                            incnatureObj.Add("entityType", "jarvis_incidentnature");
                            incnatureObj.Add("name", name);
                            incidentStringArray = incidentStringArray + name + ",";
                            incidentnature.Add(incnatureObj);
                            if (incShadow != string.Empty)
                            {
                                incShadow = incShadow + string.Empty + name + string.Empty + ";";
                            }
                            else
                            {
                                incShadow = string.Empty + name + string.Empty + ";";
                            }

                            ////if (incKnowledgeSearch != string.Empty)
                            ////{
                            ////    incKnowledgeSearch = incKnowledgeSearch + "'" + name + "'" + "|";
                            ////}
                            ////else
                            ////{
                            ////    incKnowledgeSearch = "'" + name + "'" + "|";
                            ////}
                        }

                        if (nature.TryGetValue("name", StringComparison.OrdinalIgnoreCase, out JToken name_IN) && name_IN != null)
                        {
                            var incnatureObj = new JObject();
                            incnatureObj.Add("id", incidentnatid);
                            incnatureObj.Add("entityType", "jarvis_incidentnature");
                            incnatureObj.Add("name", name_IN);
                            incidentStringArray = incidentStringArray + name_IN + ",";
                            incidentnature.Add(incnatureObj);
                            if (incShadow != string.Empty)
                            {
                                incShadow = incShadow + string.Empty + name_IN + string.Empty + ";";
                            }
                            else
                            {
                                incShadow = string.Empty + name_IN + string.Empty + ";";
                            }

                            ////if (incKnowledgeSearch != string.Empty)
                            ////{
                            ////    incKnowledgeSearch = incKnowledgeSearch + "'" + name_IN + "'" + "|";
                            ////}
                            ////else
                            ////{
                            ////    incKnowledgeSearch = "'" + name_IN + "'" + "|";
                            ////}
                        }
                    }

                    ////if (caseImage.Attributes.Contains("jarvis_country") && caseImage.Attributes["jarvis_country"] != null)
                    ////{
                    ////    EntityReference country = (EntityReference)caseImage.Attributes["jarvis_country"];
                    ////    incKnowledgeSearch = incKnowledgeSearch + "'" + country.Name + "'";
                    ////}
                }
            }

            if (incident.Attributes.Contains("jarvis_vehicle") && incident.Attributes["jarvis_vehicle"] != null)
            {
                EntityReference vehicle = (EntityReference)incident.Attributes["jarvis_vehicle"];
                Entity fuelPowerTypeRecord = service.Retrieve("jarvis_vehicle", vehicle.Id, new ColumnSet("jarvis_fuelpowertype"));

                if (fuelPowerTypeRecord.Attributes.Contains("jarvis_fuelpowertype"))
                {
                    EntityReference fuelPowerTypeReference = (EntityReference)fuelPowerTypeRecord.Attributes["jarvis_fuelpowertype"];

                    // Retrive IncidentNatures associated to Vehicle-Fuel/PowerType
                    EntityCollection incidentVehicleNatureRecords = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getFuelPowerType, fuelPowerTypeReference.Id)));

                    if (incidentVehicleNatureRecords.Entities.Count > 0)
                    {
                        isIncidentNatureChanged = true;
                        ////if (incShadow.Length < 1 && incident.Attributes.Contains("jarvis_country") && incident.Attributes["jarvis_country"] != null)
                        ////{
                        ////    EntityReference countryReference = (EntityReference)incident.Attributes["jarvis_country"];
                        ////    Entity country = service.Retrieve(countryReference.LogicalName, countryReference.Id, new ColumnSet(JarvisCountry.Name));
                        ////    if (country != null && country.Contains(JarvisCountry.Name) && country.Attributes[JarvisCountry.Name] != null)
                        ////    {
                        ////        incKnowledgeSearch = incKnowledgeSearch + "'" + (string)country.Attributes[JarvisCountry.Name] + "'";
                        ////    }
                        ////}
                        ////else if (incShadow.Length < 1 && caseImage.Attributes.Contains("jarvis_country") && caseImage.Attributes["jarvis_country"] != null)
                        ////{
                        ////    EntityReference countryReference = (EntityReference)caseImage.Attributes["jarvis_country"];
                        ////    Entity country = service.Retrieve(countryReference.LogicalName, countryReference.Id, new ColumnSet(JarvisCountry.Name));
                        ////    if (country != null && country.Contains(JarvisCountry.Name) && country.Attributes[JarvisCountry.Name] != null)
                        ////    {
                        ////        incKnowledgeSearch = incKnowledgeSearch + "'" + (string)country.Attributes[JarvisCountry.Name] + "'";
                        ////    }
                        ////}

                        foreach (var item in incidentVehicleNatureRecords.Entities)
                        {
                            EntityReference incidentNatureId = new EntityReference(Constants.IncidentNature.jarvisIncidentNature, (Guid)((AliasedValue)item.Attributes[Constants.IncidentNature.IncidentNatureVehicleFuelPid_jarvisincidentnatureid]).Value);
                            incidentNatureReferences.Add(incidentNatureId);
                            string name = (string)((AliasedValue)item.Attributes["IncidentNature_VehicleFuelPid.jarvis_name"]).Value;
                            string vehiclefuelpowertypeid = ((Guid)item.Attributes["jarvis_vehiclefuelpowertypeid"]).ToString();
                            var incnatureObj = new JObject();
                            incnatureObj.Add("id", incidentNatureId.Id);
                            incnatureObj.Add("entityType", "jarvis_incidentnature");
                            incnatureObj.Add("name", name);
                            if (!incidentStringArray.Contains(name))
                            {
                                incidentStringArray = incidentStringArray + name + ",";
                                if (incShadow != string.Empty)
                                {
                                    incShadow = incShadow + string.Empty + name + string.Empty + ";";
                                }
                                else
                                {
                                    incShadow = string.Empty + name + string.Empty + ";";
                                }

                                ////if (incKnowledgeSearch != string.Empty)
                                ////{
                                ////    incKnowledgeSearch = incKnowledgeSearch + "'" + name + "'" + "|";
                                ////}
                                ////else
                                ////{
                                ////    incKnowledgeSearch = "'" + name + "'" + "|";
                                ////}

                                incidentnature.Add(incnatureObj);
                            }
                        }

                        incidentStringArray = incidentnature.ToString();
                        var jsonData = JsonConvert.DeserializeObject(incidentStringArray);
                        string trimmedJsonString = JsonConvert.SerializeObject(jsonData, Formatting.None);
                        incident["jarvis_incidentnature"] = trimmedJsonString;
                    }
                }
            }

            ////incKnowledgeSearch = incKnowledgeSearch.Replace("'", "\"");
            incident["jarvis_incidentnatureshadow"] = incShadow;
            ////incident["jarvis_knowledgesearch"] = incKnowledgeSearch;

            return incident;
        }

        /// <summary>
        /// Associate Incident Nature to Case Multi select.
        /// </summary>
        /// <param name="incident">incident details.</param>
        /// <param name="caseImage">Case image.</param>
        /// <param name="service">Org service.</param>
        /// <param name="tracingService">Tracing service.</param>
        /// <returns>Entity details.</returns>
        public Entity AssociateKnowledgeSearch(Entity incident, Entity caseImage, IOrganizationService service, ITracingService tracingService)
        {
            tracingService.Trace("Enter into Setting Knowledge Search");
            OptionSetValue caseType = null;
            bool isEscalate = false;
            EntityReference parentCaseRef = new EntityReference();
            string incidentNatureKs = string.Empty;
            string incCountryKs = string.Empty;
            string hdCountryKs = string.Empty;
            string alertCategoryKs = string.Empty;
            string faAlertCategoryKs = string.Empty;
            string knowledgeSearchValue = string.Empty;

            EntityCollection jarvisConfigKs = service.RetrieveMultiple(new FetchExpression(Constants.FetchXmls.VASConfigurationKnowledge));

            if (incident.Attributes.Contains("casetypecode") && incident.Attributes["casetypecode"] != null)
            {
                caseType = (OptionSetValue)incident.Attributes["casetypecode"];
            }
            else if (caseImage != null && caseImage.Attributes.Contains("casetypecode") && caseImage.Attributes["casetypecode"] != null)
            {
                caseType = (OptionSetValue)caseImage.Attributes["casetypecode"];
            }

            if (incident.Attributes.Contains("isescalated") && incident.Attributes["isescalated"] != null)
            {
                isEscalate = (bool)incident.Attributes["isescalated"];
            }
            else if (caseImage != null && caseImage.Attributes.Contains("isescalated") && caseImage.Attributes["isescalated"] != null)
            {
                isEscalate = (bool)caseImage.Attributes["isescalated"];
            }

            if (incident.Attributes.Contains("parentcaseid") && incident.Attributes["parentcaseid"] != null)
            {
                parentCaseRef = (EntityReference)incident.Attributes["parentcaseid"];
            }
            else if (caseImage != null && caseImage.Attributes.Contains("parentcaseid") && caseImage.Attributes["parentcaseid"] != null)
            {
                parentCaseRef = (EntityReference)caseImage.Attributes["parentcaseid"];
            }


            if (jarvisConfigKs != null && jarvisConfigKs.Entities != null && jarvisConfigKs.Entities.Count > 0)
            {
                tracingService.Trace($"KS: Crossed the Jarvis Configuration Check");
                if (caseType != null)
                {
                    tracingService.Trace($"KS: Recieved both Case Type and Escalate {caseType.Value} , {isEscalate}");
                    if (caseType.Value == 2 && !isEscalate && jarvisConfigKs[0].Attributes.Contains("jarvis_knowledgesearchforbd") && jarvisConfigKs[0].Attributes["jarvis_knowledgesearchforbd"] != null && jarvisConfigKs[0].Attributes.Contains("jarvis_incidentnatureconjunctionbd") && jarvisConfigKs[0].Attributes["jarvis_incidentnatureconjunctionbd"] != null)
                    {
                        tracingService.Trace($"KS: Breakdown and Not Escalate Scenario");
                        this.RetrieveKnowledgeSearch(incident, caseImage, service, jarvisConfigKs, "jarvis_incidentnatureconjunctionbd", tracingService, out incidentNatureKs, out incCountryKs, out hdCountryKs, out alertCategoryKs, out faAlertCategoryKs);
                        knowledgeSearchValue = this.ReplaceKnowledgeSearch(jarvisConfigKs[0].Attributes["jarvis_knowledgesearchforbd"].ToString(), caseType, incidentNatureKs, incCountryKs, hdCountryKs, alertCategoryKs, faAlertCategoryKs, tracingService);
                        incident["jarvis_knowledgesearch"] = knowledgeSearchValue;
                    }
                    else if (caseType.Value == 2 && isEscalate && jarvisConfigKs[0].Attributes.Contains("jarvis_knowledgesearchforbdinclalert") && jarvisConfigKs[0].Attributes["jarvis_knowledgesearchforbdinclalert"] != null && jarvisConfigKs[0].Attributes.Contains("jarvis_incidentnatureconjunctionbd") && jarvisConfigKs[0].Attributes["jarvis_incidentnatureconjunctionbd"] != null)
                    {
                        tracingService.Trace($"KS: Breakdown and Not Escalate Scenario");
                        this.RetrieveKnowledgeSearch(incident, caseImage, service, jarvisConfigKs, "jarvis_incidentnatureconjunctionbd", tracingService, out incidentNatureKs, out incCountryKs, out hdCountryKs, out alertCategoryKs, out faAlertCategoryKs);
                        knowledgeSearchValue = this.ReplaceKnowledgeSearch(jarvisConfigKs[0].Attributes["jarvis_knowledgesearchforbdinclalert"].ToString(), caseType, incidentNatureKs, incCountryKs, hdCountryKs, alertCategoryKs, faAlertCategoryKs, tracingService);
                        incident["jarvis_knowledgesearch"] = knowledgeSearchValue;
                    }
                    else if (caseType.Value == 3 && !isEscalate && jarvisConfigKs[0].Attributes.Contains("jarvis_knowledgesearchforfa") && jarvisConfigKs[0].Attributes["jarvis_knowledgesearchforfa"] != null && jarvisConfigKs[0].Attributes.Contains("jarvis_incidentnatureconjunctionfa") && jarvisConfigKs[0].Attributes["jarvis_incidentnatureconjunctionfa"] != null)
                    {
                        if (parentCaseRef != null && parentCaseRef.Id != Guid.Empty)
                        {
                            Entity parentCase = service.Retrieve("incident", parentCaseRef.Id, new ColumnSet("jarvis_incidentnature", "jarvis_vehicle", "jarvis_country", "jarvis_homedealer", "jarvis_escalationmaincategory"));
                            this.RetrieveKnowledgeSearch(parentCase, caseImage, service, jarvisConfigKs, "jarvis_incidentnatureconjunctionfa", tracingService, out incidentNatureKs, out incCountryKs, out hdCountryKs, out alertCategoryKs, out faAlertCategoryKs);
                            knowledgeSearchValue = this.ReplaceKnowledgeSearch(jarvisConfigKs[0].Attributes["jarvis_knowledgesearchforfa"].ToString(), caseType, incidentNatureKs, incCountryKs, hdCountryKs, alertCategoryKs, faAlertCategoryKs, tracingService);
                            incident["jarvis_knowledgesearch"] = knowledgeSearchValue;
                        }
                    }
                    else if (caseType.Value == 3 && isEscalate && jarvisConfigKs[0].Attributes.Contains("jarvis_knowledgesearchforfainclalert") && jarvisConfigKs[0].Attributes["jarvis_knowledgesearchforfainclalert"] != null && jarvisConfigKs[0].Attributes.Contains("jarvis_incidentnatureconjunctionfa") && jarvisConfigKs[0].Attributes["jarvis_incidentnatureconjunctionfa"] != null)
                    {
                        if (parentCaseRef != null && parentCaseRef.Id != Guid.Empty)
                        {
                            Entity parentCase = service.Retrieve("incident", parentCaseRef.Id, new ColumnSet("jarvis_incidentnature", "jarvis_vehicle", "jarvis_country", "jarvis_homedealer", "jarvis_escalationmaincategory"));
                            this.RetrieveKnowledgeSearch(parentCase, caseImage, service, jarvisConfigKs, "jarvis_incidentnatureconjunctionfa", tracingService, out incidentNatureKs, out incCountryKs, out hdCountryKs, out alertCategoryKs, out faAlertCategoryKs);
                            knowledgeSearchValue = this.ReplaceKnowledgeSearch(jarvisConfigKs[0].Attributes["jarvis_knowledgesearchforfainclalert"].ToString(), caseType, incidentNatureKs, incCountryKs, hdCountryKs, alertCategoryKs, faAlertCategoryKs, tracingService);
                            incident["jarvis_knowledgesearch"] = knowledgeSearchValue;
                        }
                    }
                }
            }

            return incident;
        }

        /// <summary>
        /// Retriev Knowledge Search.
        /// </summary>
        /// <param name="incident">Incident.</param>
        /// <param name="caseImage">caseImage.</param>
        /// <param name="service">Organization Service.</param>
        /// <param name="jarvisConfigKs">Jarvis Configuration.</param>
        /// <param name="incNatureConjuction">Incident Nature Conjuction.</param>
        /// <param name="tracingService">Tracing Service.</param>
        /// <param name="incidentNatureKs">Incident Nature Ks.</param>
        /// <param name="incCountryKs">Incident Nature Country Ks.</param>
        /// <param name="hdCountryKs">Home Dealer Country Ks.</param>
        /// <param name="alertCategoryKs">Alert Category Ks.</param>
        /// <param name="faAlertCategoryKs">FA Alert Category Ks.</param>
        public void RetrieveKnowledgeSearch(Entity incident, Entity caseImage, IOrganizationService service, EntityCollection jarvisConfigKs, string incNatureConjuction, ITracingService tracingService, out string incidentNatureKs, out string incCountryKs, out string hdCountryKs, out string alertCategoryKs, out string faAlertCategoryKs)
        {
            tracingService.Trace($"KS: Enter into Retrieve Knowledege search for HD, Country, Incident Nature and AlertCategory");

            incidentNatureKs = string.Empty;
            incCountryKs = string.Empty;
            hdCountryKs = string.Empty;
            alertCategoryKs = string.Empty;
            faAlertCategoryKs = string.Empty;
            List<string> incidentNatureList = new List<string>();

            string incidentNature = string.Empty;
            EntityReference vehicle = new EntityReference();
            EntityReference countryReference = new EntityReference();
            EntityReference homeDealer = new EntityReference();
            EntityReference escalationMainCategory = new EntityReference();
            EntityReference queryCategories = new EntityReference();

            if (incident.Attributes.Contains("jarvis_incidentnature") && incident.Attributes["jarvis_incidentnature"] != null)
            {
                incidentNature = (string)incident.Attributes["jarvis_incidentnature"];
            }
            else if (caseImage != null && caseImage.Attributes.Contains("jarvis_incidentnature") && caseImage.Attributes["jarvis_incidentnature"] != null)
            {
                incidentNature = (string)caseImage.Attributes["jarvis_incidentnature"];
            }

            if (incident.Attributes.Contains("jarvis_vehicle") && incident.Attributes["jarvis_vehicle"] != null)
            {
                vehicle = (EntityReference)incident.Attributes["jarvis_vehicle"];
            }
            else if (caseImage != null && caseImage.Attributes.Contains("jarvis_vehicle") && caseImage.Attributes["jarvis_vehicle"] != null)
            {
                vehicle = (EntityReference)caseImage.Attributes["jarvis_vehicle"];
            }

            if (incident.Attributes.Contains("jarvis_country"))
            {
                countryReference = (EntityReference)incident.Attributes["jarvis_country"];
            }
            else if (caseImage != null && caseImage.Attributes.Contains("jarvis_country") && caseImage.Attributes["jarvis_country"] != null)
            {
                countryReference = (EntityReference)caseImage.Attributes["jarvis_country"];
            }

            if (incident.Attributes.Contains("jarvis_homedealer"))
            {
                homeDealer = (EntityReference)incident.Attributes["jarvis_homedealer"];
            }
            else if (caseImage != null && caseImage.Attributes.Contains("jarvis_homedealer") && caseImage.Attributes["jarvis_homedealer"] != null)
            {
                homeDealer = (EntityReference)caseImage.Attributes["jarvis_homedealer"];
            }

            if (incident.Attributes.Contains("jarvis_escalationmaincategory"))
            {
                escalationMainCategory = (EntityReference)incident.Attributes["jarvis_escalationmaincategory"];
            }
            else if (caseImage != null && caseImage.Attributes.Contains("jarvis_escalationmaincategory") && caseImage.Attributes["jarvis_escalationmaincategory"] != null)
            {
                escalationMainCategory = (EntityReference)caseImage.Attributes["jarvis_escalationmaincategory"];
            }

            if (incident.Attributes.Contains("jarvis_querycategory") && incident.Attributes["jarvis_querycategory"] != null)
            {
                queryCategories = (EntityReference)incident.Attributes["jarvis_querycategory"];
            }
            else if (caseImage != null && caseImage.Attributes.Contains("jarvis_querycategory") && caseImage.Attributes["jarvis_querycategory"] != null)
            {
                queryCategories = (EntityReference)caseImage.Attributes["jarvis_querycategory"];
            }

            tracingService.Trace($"KS: IncidentNature, Vehicle, Country and HD Country and escalationCategory");

            if (!string.IsNullOrEmpty(incidentNature))
            {
                var incidentNaturesMultiselect = JsonConvert.DeserializeObject(incidentNature);
                if (incidentNaturesMultiselect != null)
                {
                    foreach (JObject nature in (Newtonsoft.Json.Linq.JArray)incidentNaturesMultiselect)
                    {
                        if (nature.TryGetValue("_name", StringComparison.OrdinalIgnoreCase, out JToken name) && name != null)
                        {
                            if (incidentNatureKs != string.Empty)
                            {
                                incidentNatureKs = incidentNatureKs + jarvisConfigKs[0].Attributes[incNatureConjuction] + "'" + name + "'";
                            }
                            else
                            {
                                incidentNatureKs = "'" + name + "'";
                            }

                            incidentNatureList.Add(name.ToString());
                        }

                        if (nature.TryGetValue("name", StringComparison.OrdinalIgnoreCase, out JToken name_IN) && name_IN != null)
                        {
                            if (incidentNatureKs != string.Empty)
                            {
                                incidentNatureKs = incidentNatureKs + jarvisConfigKs[0].Attributes[incNatureConjuction] + "'" + name_IN + "'";
                            }
                            else
                            {
                                incidentNatureKs = "'" + name_IN + "'";
                            }

                            tracingService.Trace($"KS: IncidentNature - {incidentNatureKs}");

                            incidentNatureList.Add(name_IN.ToString());

                            tracingService.Trace($"KS: IncidentNatureList : {incidentNatureList.Count}");
                        }
                    }
                }
            }

            if (vehicle != null && vehicle.Id != Guid.Empty)
            {
                tracingService.Trace($"Enter int Vehicle IncidentNature {vehicle.Id}");

                Entity fuelPowerTypeRecord = service.Retrieve("jarvis_vehicle", vehicle.Id, new ColumnSet("jarvis_fuelpowertype"));

                if (fuelPowerTypeRecord.Attributes.Contains("jarvis_fuelpowertype") && fuelPowerTypeRecord.Attributes["jarvis_fuelpowertype"] != null)
                {
                    EntityReference fuelPowerTypeReference = (EntityReference)fuelPowerTypeRecord.Attributes["jarvis_fuelpowertype"];

                    // Retrive IncidentNatures associated to Vehicle-Fuel/PowerType
                    EntityCollection incidentVehicleNatureRecords = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getFuelPowerType, fuelPowerTypeReference.Id)));

                    tracingService.Trace($"Incident Nature - {incidentVehicleNatureRecords.Entities.Count}");

                    if (incidentVehicleNatureRecords.Entities.Count > 0)
                    {
                        foreach (var item in incidentVehicleNatureRecords.Entities)
                        {
                            string name = (string)((AliasedValue)item.Attributes["IncidentNature_VehicleFuelPid.jarvis_name"]).Value;
                            string vehiclefuelpowertypeid = ((Guid)item.Attributes["jarvis_vehiclefuelpowertypeid"]).ToString();

                            if (!incidentNatureList.Contains(name))
                            {
                                if (incidentNatureKs != string.Empty)
                                {
                                    incidentNatureKs = incidentNatureKs + jarvisConfigKs[0].Attributes[incNatureConjuction] + "'" + name + "'";
                                }
                                else
                                {
                                    incidentNatureKs = "'" + name + "'";
                                }

                                tracingService.Trace($"KS: Vehicle.IncidentNature - {incidentNatureKs}");

                                incidentNatureList.Add(name);
                            }
                        }

                        tracingService.Trace($"Vehicle.Incident Nature - Completed");
                    }
                }
            }

            if (countryReference != null && countryReference.Id != Guid.Empty)
            {
                tracingService.Trace($"Ks: Enter into country {countryReference.Id}");
                Entity country = service.Retrieve("jarvis_country", countryReference.Id, new ColumnSet(JarvisCountry.Name));
                tracingService.Trace($"Retrieve Country : {country.Id}, {country.Attributes.Contains(JarvisCountry.Name)}");
                if (country != null && country.Attributes.Contains(JarvisCountry.Name) && country.Attributes[JarvisCountry.Name] != null)
                {
                    incCountryKs = "'" + (string)country.Attributes[JarvisCountry.Name] + "'";
                    tracingService.Trace($"KS: Country - {incCountryKs}");
                }

                tracingService.Trace($"Ks: Country Completed");
            }
            else
            {
                incCountryKs = string.Empty;
                tracingService.Trace($"Ks: Country Completed");
            }

            if (homeDealer != null && homeDealer.Id != Guid.Empty)
            {
                tracingService.Trace($"Ks: Enter into Home dealer");
                Entity account = service.Retrieve("account", homeDealer.Id, new ColumnSet("jarvis_address1_country"));
                if (account != null && account.Attributes.Contains("jarvis_address1_country") && account.Attributes["jarvis_address1_country"] != null)
                {
                    EntityReference addressCountry = (EntityReference)account.Attributes["jarvis_address1_country"];
                    if (addressCountry != null && addressCountry.Id != Guid.Empty)
                    {
                        Entity country = service.Retrieve("jarvis_country", addressCountry.Id, new ColumnSet(JarvisCountry.Name));

                        if (country != null && country.Id != Guid.Empty && country.Contains(JarvisCountry.Name) && country.Attributes[JarvisCountry.Name] != null)
                        {
                            hdCountryKs = "'" + (string)country.Attributes[JarvisCountry.Name] + "'";
                            tracingService.Trace($"KS: HdCountry - {hdCountryKs}");
                        }
                    }
                }

                tracingService.Trace($"Ks: Home Dealer Completed");
            }
            else
            {
                hdCountryKs = string.Empty;
                tracingService.Trace($"Ks: Home Dealer Completed");
            }

            if (escalationMainCategory != null && escalationMainCategory.Id != Guid.Empty)
            {
                Entity alertCategory = service.Retrieve("jarvis_escalationmaincategory", escalationMainCategory.Id, new ColumnSet("jarvis_name"));
                if (alertCategory != null && alertCategory.Attributes.Contains("jarvis_name") && alertCategory.Attributes["jarvis_name"] != null)
                {
                    alertCategoryKs = "'" + (string)alertCategory.Attributes["jarvis_name"] + "'";
                    tracingService.Trace($"KS: AlertCategory - {alertCategoryKs}");
                }

                tracingService.Trace($"Ks: Alert Category Completed");
            }
            else
            {
                alertCategoryKs = string.Empty;
                tracingService.Trace($"Ks: Alert Category Completed");
            }

            if (queryCategories != null && queryCategories.Id != Guid.Empty)
            {
                Entity queryCategory = service.Retrieve("jarvis_querycategory", queryCategories.Id, new ColumnSet("jarvis_name"));
                if (queryCategory != null && queryCategory.Attributes.Contains("jarvis_name") && queryCategory.Attributes["jarvis_name"] != null)
                {
                    faAlertCategoryKs = "'" + (string)queryCategory.Attributes["jarvis_name"] + "'";
                    tracingService.Trace($"KS: FA AlertCategory - {faAlertCategoryKs}");
                }
            }

            tracingService.Trace($"KS: Completed Retrieve Knowledege search for HD, Country, Incident Nature and AlertCategory");
        }

        /// <summary>
        /// Replace KnowledgeSearch.
        /// </summary>
        /// <param name="knowledgeSearchKey">knowledge Search Key.</param>
        /// <param name="caseType">case Type.</param>
        /// <param name="incidentNatureKs">incident NatureKs.</param>
        /// <param name="incCountryKs">inc Country Ks.</param>
        /// <param name="hdCountryKs">hd Country Ks.</param>
        /// <param name="alertCategoryKs">alert CategoryKs.</param>
        /// <param name="faAlertCategoryKs">fa AlertCategory Ks.</param>
        /// <param name="tracingService">tracing Service.</param>
        /// <returns>string.</returns>
        private string ReplaceKnowledgeSearch(string knowledgeSearchKey, OptionSetValue caseType, string incidentNatureKs, string incCountryKs, string hdCountryKs, string alertCategoryKs, string faAlertCategoryKs, ITracingService tracingService)
        {
            tracingService.Trace($"KS: Enter into Replace knowledgeSearch Method");
            string knowledgeSearchValue = string.Empty;

            if (!string.IsNullOrEmpty(knowledgeSearchKey))
            {
                tracingService.Trace($"KS: Knowledge search key: {knowledgeSearchKey}");
                knowledgeSearchValue = knowledgeSearchKey.Replace("[incident.jarvis_incidentnatureshadow]", string.Format("({0})", incidentNatureKs.Replace("'", "\"")));
                tracingService.Trace($"KS: After Incident Nature Replace: {knowledgeSearchValue}");

                knowledgeSearchValue = knowledgeSearchValue.Replace("[incident.account.HD_country]", string.Format("{0}", hdCountryKs.Replace("'", "\"")));
                tracingService.Trace($"KS: After HD_Country Replace: {knowledgeSearchValue}");

                knowledgeSearchValue = knowledgeSearchValue.Replace("[incident.jarvis_country]", string.Format("{0}", incCountryKs.Replace("'", "\"")));
                tracingService.Trace($"KS: After Country Replace: {knowledgeSearchValue}");

                if (caseType.Value == 2)
                {
                    knowledgeSearchValue = knowledgeSearchValue.Replace("[incident.jarvis_alertcategory]", string.Format("{0}", alertCategoryKs.Replace("'", "\"")));
                    tracingService.Trace($"KS: After AlertCategory Replace: {knowledgeSearchValue}");
                }
                else if (caseType.Value == 3)
                {
                    knowledgeSearchValue = knowledgeSearchValue.Replace("[incident.jarvis_alertcategory]", string.Format("{0}", alertCategoryKs.Replace("'", "\"")));
                    tracingService.Trace($"KS: After AlertCategory Replace: {knowledgeSearchValue}");
                }
            }

            return knowledgeSearchValue;
        }
    }
}