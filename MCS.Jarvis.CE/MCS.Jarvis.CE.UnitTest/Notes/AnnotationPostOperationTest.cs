//-----------------------------------------------------------------------
// <copyright file="AnnotationPostOperationTest.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace MCS.Jarvis.CE.UnitTest
{
    using System;
    using System.Collections.Generic;
    using MCS.Jarvis.CE.IntegrationPlugin;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.Xrm.Sdk;
    using static MCS.Jarvis.CE.BusinessProcessesShared.Helpers.Constants;

    /// <summary>
    /// Annotation PostOperationTest.
    /// </summary>
    [TestClass]
    public class AnnotationPostOperationTest : UnitTestBase
    {
        /// <summary>
        /// Positive Scenario but due to object is null we will get Expected Exception.
        /// </summary>
        [TestMethod]
        public void AnnotationNonMercuriusUser()
        {
            var incident = new Entity("incident");
            incident.Id = Guid.NewGuid();
            incident[Incident.caseOriginCode] = new OptionSetValue(2);
            incident[Incident.casetypecode] = new OptionSetValue(2);

            var postImg = new Entity("incident");
            postImg.Attributes[Notes.regardingObjectId] = new EntityReference("incident", incident.Id);
            postImg.Attributes[Notes.isdocument] = true;

            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", incident),
               };
            this.PluginExecutionContext.MessageNameGet = () => "Update";
            this.PluginExecutionContext.StageGet = () => 40;
            this.PluginExecutionContext.UserIdGet = () => Guid.NewGuid();

            //// Setting Post Entity Image.
            this.PluginExecutionContext.PostEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PostImage", postImg },
                };
            };

            // Setting service retrieve.
            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "systemuser")
                {
                    Entity user = new Entity("systemuser");
                    user.Id = Guid.NewGuid();
                    user["fullname"] = "OneCase";
                    return user;
                }

                if (entityName == "incident")
                {
                    Entity inc = new Entity("incident");
                    inc.Id = guid;
                    inc["casetypecode"] = new OptionSetValue(2);
                    return inc;
                }

                return null;
            };
            string secureString = $"{{\r\n  \"functionAppUrl\": \"https://test.azure-api.net/\",\r\n  \"client_id\": \"{Guid.Empty}\",\r\n  \"client_secret\": \"{Guid.Empty}\",\r\n  \"scope\":\"api://{Guid.Empty}/.default\",\r\n  \"tenantId\": \"{Guid.Empty}\",\r\n  \"tokenUrl\":\"https://login.microsoftonline.com/{{0}}/oauth2/token\",\r\n\"api-key\":\"{Guid.Empty}\"\r\n}}";
            AnnotationPostOperation plugin = new AnnotationPostOperation("test", secureString);
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Positive Scenario but due to object is null we will get Expected Exception.
        /// </summary>
        [TestMethod]
        public void AnnotationMercuriusUser()
        {
            var incident = new Entity("incident");
            incident.Id = Guid.NewGuid();
            incident[Incident.caseOriginCode] = new OptionSetValue(2);
            incident[Incident.casetypecode] = new OptionSetValue(2);

            var postImg = new Entity("incident");
            postImg.Attributes[Notes.regardingObjectId] = new EntityReference("incident", incident.Id);
            postImg.Attributes[Notes.isdocument] = true;

            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
               {
                   new KeyValuePair<string, object>("Target", incident),
               };
            this.PluginExecutionContext.MessageNameGet = () => "Update";
            this.PluginExecutionContext.StageGet = () => 40;
            this.PluginExecutionContext.UserIdGet = () => Guid.NewGuid();

            //// Setting Post Entity Image.
            this.PluginExecutionContext.PostEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                        { "PostImage", postImg },
                };
            };

            // Setting service retrieve.
            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "systemuser")
                {
                    Entity user = new Entity("systemuser");
                    user.Id = Guid.NewGuid();
                    user["fullname"] = "Mercurius";
                    return user;
                }

                if (entityName == "incident")
                {
                    Entity inc = new Entity("incident");
                    inc.Id = guid;
                    inc["casetypecode"] = new OptionSetValue(2);
                    return inc;
                }

                return null;
            };

            AnnotationPostOperation plugin = new AnnotationPostOperation("test", "test");
            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Negative scenario where secure and unsecure strings are not provided.
        /// </summary>
        [TestMethod]
        public void AnnotationNoSecureUnsecureStrings()
        {
            Action createPlugin = () => new AnnotationPostOperation(null, null);
            Assert.ThrowsException<InvalidPluginExecutionException>(createPlugin);
        }
    }
}