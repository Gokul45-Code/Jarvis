//-----------------------------------------------------------------------
// <copyright file="PassoutPostOperationAsyncTest.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace MCS.Jarvis.CE.UnitTest
{
    using System;
    using System.Collections.Generic;
    using MCS.Jarvis.IntegrationPlugin;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;
    using static MCS.Jarvis.CE.BusinessProcessesShared.Helpers.Constants;

    /// <summary>
    /// Pass out Post Operation Async Test.
    /// </summary>
    [TestClass]
    public class PassoutPostOperationAsyncTest : UnitTestBase
    {
        /// <summary>
        /// Positive scenario with user for Pass out Post Operation Async Create Scenario.
        /// Contact Should not be bull.
        /// </summary>
        [TestMethod]
        public void PassoutPostOperationAsyncTestCreate()
        {
            var incident = new Entity("incident");
            incident.Id = Guid.NewGuid();
            incident[Incident.caseOriginCode] = new OptionSetValue(2);
            incident[Incident.casetypecode] = new OptionSetValue(2);

            var postImg = new Entity("jarvis_passout");

            postImg.Attributes[BusinessProcessesShared.Helpers.Constants.PassOut.Source] = new OptionSetValue((int)Source.Jarvis);
            postImg.Attributes[BusinessProcessesShared.Helpers.Constants.PassOut.Statuscode] = new OptionSetValue((int)PassOutStatus.ToBeSent);
            postImg.Attributes[Casecontact.jarvisIncident] = new EntityReference("incident", Guid.NewGuid());
            postImg.Attributes[PassOut.Modifiedon] = DateTime.UtcNow;
            postImg.Attributes[PassOut.JarvisEta] = DateTime.UtcNow;
            postImg.Attributes[PassOut.JarvisAta] = DateTime.UtcNow;
            postImg.Attributes[PassOut.JarvisEtc] = DateTime.UtcNow;
            postImg.Attributes[PassOut.JarvisAtc] = DateTime.UtcNow;
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
            {
                new KeyValuePair<string, object>("Target", postImg),
            };
            this.PluginExecutionContext.MessageNameGet = () => "Create";
            this.PluginExecutionContext.StageGet = () => 40;

            // Setting post-entity image.
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
            string secureString = $"{{\r\n  \"functionAppUrl\": \"https://test.azure-api.net/\",\r\n  \"client_id\": \"{Guid.Empty}\",\r\n  \"client_secret\": \"{Guid.Empty}\",\r\n  \"scope\":\"api://{Guid.Empty}/.default\",\r\n  \"tenantId\": \"{Guid.Empty}\",\r\n  \"tokenUrl\":\"https://login.microsoftonline.com/{{0}}/oauth2/token\",\r\n\"api-key\":\"{Guid.Empty}\"\r\n}}";
            PassoutPostOperationAsync plugin = new PassoutPostOperationAsync("test", secureString);

            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Positive scenario with user for Pass out Post Operation Async Update Scenario
        /// When Trans Type is not Jarvis Event ETA Update nor Jarvis Event Pass Out.
        /// </summary>
        [TestMethod]
        public void PassoutPostOperationAsyncTestUpdate()
        {
            // Arrange
            Entity passout = new Entity("incident");
            passout.Id = Guid.NewGuid();
            passout.Attributes[BusinessProcessesShared.Helpers.Constants.PassOut.Statuscode] = new OptionSetValue((int)PassOutStatus.ToBeSent);
            passout[Incident.casetypecode] = new OptionSetValue(2);
            passout.Attributes[PassOut.Modifiedon] = DateTime.UtcNow;
            passout.Attributes[PassOut.JarvisEta] = DateTime.UtcNow;
            passout.Attributes[PassOut.JarvisAta] = DateTime.UtcNow;
            passout.Attributes[PassOut.JarvisEtc] = DateTime.UtcNow;
            passout.Attributes[PassOut.JarvisAtc] = DateTime.UtcNow;
            Entity passOutImg = new Entity("passout");
            passOutImg.Id = Guid.NewGuid();
            passOutImg[Casecontact.jarvisIncident] = new EntityReference("incident", Guid.NewGuid());
            passOutImg.Attributes[BusinessProcessesShared.Helpers.Constants.PassOut.Source] = new OptionSetValue((int)Source.Jarvis);
            passOutImg.Attributes[BusinessProcessesShared.Helpers.Constants.PassOut.Statuscode] = new OptionSetValue((int)PassOutStatus.ToBeSent);
            passOutImg.Attributes[Casecontact.jarvisIncident] = new EntityReference("incident", Guid.NewGuid());
            passOutImg.Attributes[PassOut.Modifiedon] = DateTime.UtcNow;
            passOutImg.Attributes[PassOut.JarvisEta] = DateTime.UtcNow;
            passOutImg.Attributes[PassOut.JarvisAta] = DateTime.UtcNow;
            passOutImg.Attributes[PassOut.JarvisEtc] = DateTime.UtcNow;
            passOutImg.Attributes[PassOut.JarvisAtc] = DateTime.UtcNow;
            Entity postImage = new Entity("passout");
            postImage.Id = Guid.NewGuid();
            postImage.Attributes[PassOut.JarvisDescription] = "test";

            Entity preImage = new Entity("passout");
            preImage.Id = Guid.NewGuid();

            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
             {
                  new KeyValuePair<string, object>(General.Target, passout),
             };

            this.PluginExecutionContext.PostEntityImagesGet = () => new EntityImageCollection
            {
                 { "PostImage", passOutImg },
                 { "ComparePostImage", passOutImg },
            };

            this.PluginExecutionContext.PreEntityImagesGet = () => new EntityImageCollection
            {
                 { "ComparePreImage", preImage },
            };

            this.PluginExecutionContext.MessageNameGet = () => "Update";

            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, columnSet) =>
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
            string secureString = $"{{\r\n  \"functionAppUrl\": \"https://test.azure-api.net/\",\r\n  \"client_id\": \"{Guid.Empty}\",\r\n  \"client_secret\": \"{Guid.Empty}\",\r\n  \"scope\":\"api://{Guid.Empty}/.default\",\r\n  \"tenantId\": \"{Guid.Empty}\",\r\n  \"tokenUrl\":\"https://login.microsoftonline.com/{{0}}/oauth2/token\",\r\n\"api-key\":\"{Guid.Empty}\"\r\n}}";
            PassoutPostOperationAsync plugin = new PassoutPostOperationAsync("test", secureString);

            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Positive scenario with user for Pass out Post Operation Async Update Scenario
        /// When Trans Type is not Jarvis Event ETA Update nor Jarvis Event Pass Out.
        /// </summary>
        [TestMethod]
        public void PassoutPostOperationAsyncTestUpdateCancelled()
        {
            // Arrange
            Entity passout = new Entity("incident");
            passout.Id = Guid.NewGuid();
            passout.Attributes[BusinessProcessesShared.Helpers.Constants.PassOut.Statuscode] = new OptionSetValue((int)PassOutStatus.Cancelled);
            passout[Incident.casetypecode] = new OptionSetValue(2);
            passout.Attributes[PassOut.Modifiedon] = DateTime.UtcNow;
            passout.Attributes[PassOut.JarvisEta] = DateTime.UtcNow;
            passout.Attributes[PassOut.JarvisAta] = DateTime.UtcNow;
            passout.Attributes[PassOut.JarvisEtc] = DateTime.UtcNow;
            passout.Attributes[PassOut.JarvisAtc] = DateTime.UtcNow;
            Entity passOutImg = new Entity("passout");
            passOutImg.Id = Guid.NewGuid();
            passOutImg[Casecontact.jarvisIncident] = new EntityReference("incident", Guid.NewGuid());
            passOutImg.Attributes[BusinessProcessesShared.Helpers.Constants.PassOut.Source] = new OptionSetValue((int)Source.Jarvis);
            passOutImg.Attributes[BusinessProcessesShared.Helpers.Constants.PassOut.Statuscode] = new OptionSetValue((int)PassOutStatus.Cancelled);
            passOutImg.Attributes[Casecontact.jarvisIncident] = new EntityReference("incident", Guid.NewGuid());
            passOutImg.Attributes[PassOut.Modifiedon] = DateTime.UtcNow;
            passOutImg.Attributes[PassOut.JarvisEta] = DateTime.UtcNow;
            passOutImg.Attributes[PassOut.JarvisAta] = DateTime.UtcNow;
            passOutImg.Attributes[PassOut.JarvisEtc] = DateTime.UtcNow;
            passOutImg.Attributes[PassOut.JarvisAtc] = DateTime.UtcNow;
            Entity postImage = new Entity("passout");
            postImage.Id = Guid.NewGuid();
            postImage.Attributes[PassOut.JarvisDescription] = "test";

            Entity preImage = new Entity("passout");
            preImage.Id = Guid.NewGuid();

            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
             {
                  new KeyValuePair<string, object>(General.Target, passout),
             };

            this.PluginExecutionContext.PostEntityImagesGet = () => new EntityImageCollection
            {
                 { "PostImage", passOutImg },
                 { "ComparePostImage", passOutImg },
            };

            this.PluginExecutionContext.PreEntityImagesGet = () => new EntityImageCollection
            {
                 { "ComparePreImage", preImage },
            };

            this.PluginExecutionContext.MessageNameGet = () => "Update";

            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, columnSet) =>
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
            string secureString = $"{{\r\n  \"functionAppUrl\": \"https://test.azure-api.net/\",\r\n  \"client_id\": \"{Guid.Empty}\",\r\n  \"client_secret\": \"{Guid.Empty}\",\r\n  \"scope\":\"api://{Guid.Empty}/.default\",\r\n  \"tenantId\": \"{Guid.Empty}\",\r\n  \"tokenUrl\":\"https://login.microsoftonline.com/{{0}}/oauth2/token\",\r\n\"api-key\":\"{Guid.Empty}\"\r\n}}";
            PassoutPostOperationAsync plugin = new PassoutPostOperationAsync("test", secureString);

            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Positive scenario with user for Pass out Post Operation Async Update Scenario
        /// For Trans Type = Jarvis Event ETA Update with Mercurius user.
        /// </summary>
        [TestMethod]
        public void PassoutPostOperationAsyncTestUpdateETAUpdate()
        {
            // Arrange
            Entity passout = new Entity("incident");
            passout.Id = Guid.NewGuid();
            passout.Attributes[BusinessProcessesShared.Helpers.Constants.PassOut.Statuscode] = new OptionSetValue((int)PassOutStatus.Cancelled);
            passout[Incident.casetypecode] = new OptionSetValue(2);
            passout.Attributes[PassOut.Modifiedon] = DateTime.UtcNow;
            passout.Attributes[PassOut.JarvisEta] = DateTime.UtcNow;
            passout.Attributes[PassOut.JarvisAta] = DateTime.UtcNow;
            passout.Attributes[PassOut.JarvisEtc] = DateTime.UtcNow;
            passout.Attributes[PassOut.JarvisAtc] = DateTime.UtcNow;

            Entity passOutImg = new Entity("passout");
            passOutImg.Id = Guid.NewGuid();
            passOutImg[Casecontact.jarvisIncident] = new EntityReference("incident", Guid.NewGuid());
            passOutImg.Attributes[PassOut.Modifiedon] = DateTime.UtcNow;
            passOutImg.Attributes[PassOut.JarvisEta] = DateTime.UtcNow;
            passOutImg.Attributes[PassOut.JarvisAta] = DateTime.UtcNow;
            passOutImg.Attributes[PassOut.JarvisEtc] = DateTime.UtcNow;
            passOutImg.Attributes[PassOut.JarvisAtc] = DateTime.UtcNow;
            passOutImg[BusinessProcessesShared.Helpers.Constants.PassOut.Statuscode] = new OptionSetValue((int)PassOutStatus.Sent);

            Entity preImagePassout = new Entity("passout");
            preImagePassout.Id = Guid.NewGuid();
            preImagePassout.Attributes[BusinessProcessesShared.Helpers.Constants.PassOut.Eta] = DateTime.MinValue;
            preImagePassout.Attributes[BusinessProcessesShared.Helpers.Constants.PassOut.JarvisGpsETA] = DateTime.MinValue;
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
             {
                  new KeyValuePair<string, object>(General.Target, passout),
             };
            this.PluginExecutionContext.UserIdGet = () => Guid.NewGuid();
            this.PluginExecutionContext.PostEntityImagesGet = () => new EntityImageCollection
            {
                 { "PostImage", passOutImg },
                 { "ComparePostImage", passOutImg },
            };

            this.PluginExecutionContext.PreEntityImagesGet = () => new EntityImageCollection
            {
                 { "ComparePreImage", preImagePassout },
            };

            this.PluginExecutionContext.MessageNameGet = () => "Update";

            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, columnSet) =>
            {
                if (entityName == "systemuser")
                {
                    Entity user = new Entity("systemuser");
                    user.Id = Guid.NewGuid();
                    user["fullname"] = "MERCURIUS";
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
                        configResult["jarvis_automationreleasecase"] = new OptionSetValue(3);
                        configResult["createdon"] = DateTime.UtcNow;
                        configResult.Id = Guid.NewGuid();
                        result.Entities.Add(configResult);
                    }
                }

                return result;
            };
            string secureString = $"{{\r\n  \"functionAppUrl\": \"https://test.azure-api.net/\",\r\n  \"client_id\": \"{Guid.Empty}\",\r\n  \"client_secret\": \"{Guid.Empty}\",\r\n  \"scope\":\"api://{Guid.Empty}/.default\",\r\n  \"tenantId\": \"{Guid.Empty}\",\r\n  \"tokenUrl\":\"https://login.microsoftonline.com/{{0}}/oauth2/token\",\r\n\"api-key\":\"{Guid.Empty}\"\r\n}}";
            PassoutPostOperationAsync plugin = new PassoutPostOperationAsync("Jarvis.Event.ETAUpdate", secureString);

            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Positive scenario with user for Pass out Post Operation Async Update Scenario
        /// For Trans type = Jarvis.Event.PassOut.
        /// </summary>
        [TestMethod]
        public void PassoutPostOperationAsyncTestUpdateEventPassout()
        {
            // Arrange
            Entity incident = new Entity("incident");
            incident.Id = Guid.NewGuid();
            incident[Incident.caseOriginCode] = new OptionSetValue(2);
            incident[Incident.casetypecode] = new OptionSetValue(2);

            Entity passOutImg = new Entity("passout");
            passOutImg.Id = Guid.NewGuid();
            passOutImg[Casecontact.jarvisIncident] = new EntityReference("incident", incident.Id);

            passOutImg[BusinessProcessesShared.Helpers.Constants.PassOut.Statuscode] = new OptionSetValue((int)PassOutStatus.Sent);

            Entity preImagePassout = new Entity("passout");
            preImagePassout.Id = Guid.NewGuid();
            preImagePassout.Attributes[BusinessProcessesShared.Helpers.Constants.PassOut.Eta] = DateTime.MinValue;

            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
             {
                  new KeyValuePair<string, object>(General.Target, incident),
             };

            this.PluginExecutionContext.PostEntityImagesGet = () => new EntityImageCollection
            {
                 { "PostImage", passOutImg },
                 { "ComparePostImage", passOutImg },
            };

            this.PluginExecutionContext.PreEntityImagesGet = () => new EntityImageCollection
            {
                 { "ComparePreImage", preImagePassout },
            };

            this.PluginExecutionContext.MessageNameGet = () => "Update";

            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, columnSet) =>
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
            string secureString = $"{{\r\n  \"functionAppUrl\": \"https://test.azure-api.net/\",\r\n  \"client_id\": \"{Guid.Empty}\",\r\n  \"client_secret\": \"{Guid.Empty}\",\r\n  \"scope\":\"api://{Guid.Empty}/.default\",\r\n  \"tenantId\": \"{Guid.Empty}\",\r\n  \"tokenUrl\":\"https://login.microsoftonline.com/{{0}}/oauth2/token\",\r\n\"api-key\":\"{Guid.Empty}\"\r\n}}";
            PassoutPostOperationAsync plugin = new PassoutPostOperationAsync("Jarvis.Event.PassOut", secureString);

            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Negative scenario where secure and unsecure strings are not provided.
        /// </summary>
        [TestMethod]

        // [ExpectedException(typeof(InvalidPluginExecutionException))]
        public void PassoutPostOperationAsyncTestNoSecureUnsecureStrings()
        {
            // Creating the plugin instance without providing secure and unsecure strings.
            Action createPlugin = () => new PassoutPostOperationAsync(null, null);

            Assert.ThrowsException<InvalidPluginExecutionException>(createPlugin);

            // The expected exception (InvalidPluginExecutionException) will be thrown, indicating the absence of secure and unsecure strings.
        }
    }
}
