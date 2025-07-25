//-----------------------------------------------------------------------
// <copyright file="PassOutPostOperationUserProfileSyncTest.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace MCS.Jarvis.CE.UnitTest
{
    using System;
    using System.Collections.Generic;
    using MCS.Jarvis.CE.Plugins;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;

    /// <summary>
    /// PassOut PostOperation UserProfileSync Test.
    /// </summary>
    [TestClass]
    public class PassOutPostOperationUserProfileSyncTest : UnitTestBase
    {
        /// <summary>
        /// Positive scenario with user for PassOutCreatePostOpUserProfileSync Delete Scenario
        /// Contact Should not be bull.
        /// </summary>
        [TestMethod]
        public void PassOutPostOpUserProfileSyncDeleteTest()
        {
            var passOutObject = new Entity("jarvis_passout");
            passOutObject.Id = Guid.NewGuid();
            passOutObject[MCS.Jarvis.CE.Plugins.PassOut.Attributes.Case] = new EntityReference("incident", Guid.NewGuid());
            passOutObject[MCS.Jarvis.CE.Plugins.PassOut.Attributes.BusinessPartner] = new EntityReference("businesspartner", Guid.NewGuid());

            Entity preEntityImage = new Entity("jarvis_passout");
            preEntityImage.Id = Guid.NewGuid();
            preEntityImage[MCS.Jarvis.CE.Plugins.PassOut.Attributes.Case] = new EntityReference("incident", Guid.NewGuid());
            preEntityImage[MCS.Jarvis.CE.Plugins.PassOut.Attributes.BusinessPartner] = new EntityReference("businesspartner", Guid.NewGuid());

            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
            {
                new KeyValuePair<string, object>("Target", passOutObject),
            };
            this.PluginExecutionContext.MessageNameGet = () => "Delete";
            this.PluginExecutionContext.StageGet = () => 40;
            this.PluginExecutionContext.PreEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PreImage", preEntityImage },
                };
            };
            // Setting service retrieve.
            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "incident")
                {
                    var caseObject = new Entity("incident");
                    caseObject.Id = guid;
                    caseObject[Case.Attributes.Location] = new OptionSetValue(334030001);
                    caseObject[Case.Attributes.ServiceLine] = new EntityReference("caseserviceline", Guid.NewGuid());
                    caseObject[Case.Attributes.CaseType] = new OptionSetValue(2);
                    return caseObject;
                }

                if (entityName == "account")
                {
                    var businessPartner = new Entity("account");
                    businessPartner.Id = guid;
                    businessPartner[BusinessPartner.Atrributes.Country] = new EntityReference("country", Guid.NewGuid());
                    businessPartner[BusinessPartner.Atrributes.BusinessPartnerCountry] = new EntityReference("country", Guid.NewGuid());
                    return businessPartner;
                }



                return null;
            };

            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();

                if (query.GetType().Name == "FetchExpression")
                {
                    if (query is FetchExpression fetchExpression && fetchExpression.Query.Contains("<entity name='jarvis_caseuser'>"))
                    {
                        var caseUser = new Entity("jarvis_caseuser");
                        caseUser.Id = Guid.NewGuid();
                        result.Entities.Add(caseUser);
                    }
                }

                return result;
            };

            Plugins.PassOutPostOperationUserProfileSync plugin = new Plugins.PassOutPostOperationUserProfileSync();

            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Positive scenario with user for PassOutCreatePostOpUserProfileSync Update Scenario
        /// Contact Should not be bull.
        /// </summary>
        [TestMethod]
        public void PassOutPostOpUserProfileSyncUpdateTest()
        {
            var passOutObject = new Entity("jarvis_passout");
            passOutObject.Id = Guid.NewGuid();
            passOutObject[MCS.Jarvis.CE.Plugins.PassOut.Attributes.Case] = new EntityReference("incident", Guid.NewGuid());
            passOutObject[MCS.Jarvis.CE.Plugins.PassOut.Attributes.BusinessPartner] = new EntityReference("businesspartner", Guid.NewGuid());
            passOutObject[MCS.Jarvis.CE.Plugins.PassOut.Attributes.Status] = new OptionSetValue(1);

            Entity preEntityImage = new Entity("jarvis_passout");
            preEntityImage.Id = Guid.NewGuid();
            preEntityImage[MCS.Jarvis.CE.Plugins.PassOut.Attributes.Case] = new EntityReference("incident", Guid.NewGuid());
            preEntityImage[MCS.Jarvis.CE.Plugins.PassOut.Attributes.BusinessPartner] = new EntityReference("businesspartner", Guid.NewGuid());

            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
            {
                new KeyValuePair<string, object>("Target", passOutObject),
            };
            this.PluginExecutionContext.MessageNameGet = () => "Update";
            this.PluginExecutionContext.StageGet = () => 40;
            this.PluginExecutionContext.PreEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PreImage", preEntityImage },
                };
            };
            // Setting service retrieve.
            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "incident")
                {
                    var caseObject = new Entity("incident");
                    caseObject.Id = guid;
                    caseObject[Case.Attributes.Location] = new OptionSetValue(334030001);
                    caseObject[Case.Attributes.ServiceLine] = new EntityReference("caseserviceline", Guid.NewGuid());
                    caseObject[Case.Attributes.CaseType] = new OptionSetValue(2);
                    return caseObject;
                }

                if (entityName == "account")
                {
                    var businessPartner = new Entity("account");
                    businessPartner.Id = guid;
                    businessPartner[BusinessPartner.Atrributes.Country] = new EntityReference("country", Guid.NewGuid());
                    businessPartner[BusinessPartner.Atrributes.BusinessPartnerCountry] = new EntityReference("country", Guid.NewGuid());
                    return businessPartner;
                }



                return null;
            };

            this.Service.RetrieveMultipleQueryBase = query =>
            {
                var result = new EntityCollection();

                if (query.GetType().Name == "FetchExpression")
                {
                    if (query is FetchExpression fetchExpression && fetchExpression.Query.Contains("<entity name='jarvis_caseuser'>"))
                    {
                        var caseUser = new Entity("jarvis_caseuser");
                        caseUser.Id = Guid.NewGuid();
                        result.Entities.Add(caseUser);
                    }
                }

                return result;
            };

            Plugins.PassOutPostOperationUserProfileSync plugin = new Plugins.PassOutPostOperationUserProfileSync();

            plugin.Execute(this.ServiceProvider);
        }
    }
}
