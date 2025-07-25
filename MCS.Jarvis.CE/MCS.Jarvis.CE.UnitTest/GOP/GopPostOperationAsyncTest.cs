//-----------------------------------------------------------------------
// <copyright file="GopPostOperationAsyncTest.cs" company="Microsoft">
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
    using static MCS.Jarvis.CE.BusinessProcessesShared.Helpers.Constants;

    /// <summary>
    /// GOP PostOperationAsync Test.
    /// </summary>
    [TestClass]
    public class GopPostOperationAsyncTest : UnitTestBase
    {
        /// <summary>
        /// Positive scenario with user for GOP PostOperationAsync Create Scenario for ADD GOP Triggered
        /// Contact Should not be bull.
        /// </summary>
        [TestMethod]
        public void GopPostOperationAsyncTestAsMercuriusCreate()
        {
            var incident = new Entity("incident"); // Create a mock or actual instance of the "incident" Entity
            incident.Id = Guid.NewGuid(); // Set a valid GUID for the "incident" entity's ID
            incident[Incident.caseOriginCode] = new OptionSetValue(2);
            incident[Incident.casetypecode] = new OptionSetValue(2);

            var gopImg = new Entity("jarvis_gop");
            gopImg.Attributes[Gop.totalLimitIn] = 500.00M;
            gopImg.Attributes[Gop.totalLimitOut] = 1000.00M;
            gopImg.Attributes[Gop.Source] = new OptionSetValue((int)Source.Jarvis);
            gopImg.Attributes[Gop.Approved] = true;
            gopImg.Attributes[Gop.contact] = "1a679c95-33f0-40e1-ae35-bd2e72ff8ac3";
            gopImg.Attributes[Gop.requestType] = new OptionSetValue((int)GopRequestType.GOP_HD);
            gopImg.Attributes[Casecontact.jarvisIncident] = new EntityReference("incident", Guid.NewGuid());
            gopImg.Attributes[Gop.jarvis_goplimitin] = 200.54M;
            gopImg.Attributes[Gop.jarvis_goplimitout] = 400.34M;
            gopImg.Attributes["statecode"] = new OptionSetValue(1);
            gopImg.Attributes[Gop.GopApproval] = new OptionSetValue((int)GopApproval.Approved);
            gopImg.Attributes[Gop.PaymentType] = new OptionSetValue((int)PaymentType.CreditCard);
            gopImg.Attributes[Gop.jarvis_goplimitin] = 200.00M;
            gopImg.Attributes[Gop.jarvis_goplimitout] = 200.00M;
            gopImg.Attributes["statuscode"] = new OptionSetValue(2);
            gopImg.Attributes["jarvis_isdealercopied"] = false;
            gopImg.Attributes["jarvis_triggeredby"] = new EntityReference("systemuser", Guid.NewGuid());


            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
            {
                new KeyValuePair<string, object>("Target", gopImg),
            };
            this.PluginExecutionContext.MessageNameGet = () => "Create";
            this.PluginExecutionContext.StageGet = () => 40;

            // Setting post-entity image.
            this.PluginExecutionContext.PostEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                    { "PostImage", gopImg },
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

            GopPostOperationAsync plugin = new GopPostOperationAsync("test", "test");

            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Positive scenario with user for GOP PostOperationAsync Create Scenario for ADD GOP HD or ADD GOP RD Triggered
        ///  /// Contact Should be bull.
        /// </summary>
        [TestMethod]
        public void GopPostOperationAsyncTestAsMercuriusCreateHDorRD()
        {
            var incident = new Entity("incident");
            incident.Id = Guid.NewGuid();
            incident[Incident.caseOriginCode] = new OptionSetValue(2);
            incident[Incident.casetypecode] = new OptionSetValue(2);

            var gopImg = new Entity("jarvis_gop");
            gopImg.Attributes[Gop.totalLimitIn] = 500.00M;
            gopImg.Attributes[Gop.totalLimitOut] = 1000.00M;
            gopImg.Attributes[Gop.Source] = new OptionSetValue((int)Source.Jarvis);
            gopImg.Attributes[Gop.Approved] = true;
            gopImg.Attributes[Gop.contact] = null;
            gopImg.Attributes[Gop.requestType] = new OptionSetValue((int)GopRequestType.GOP_HD);
            gopImg.Attributes[Casecontact.jarvisIncident] = new EntityReference("incident", Guid.NewGuid());
            gopImg.Attributes[Gop.jarvis_goplimitin] = 200.54M;
            gopImg.Attributes[Gop.jarvis_goplimitout] = 400.34M;
            gopImg.Attributes["statecode"] = new OptionSetValue(1);
            gopImg.Attributes[Gop.GopApproval] = new OptionSetValue((int)GopApproval.Approved);
            gopImg.Attributes[Gop.PaymentType] = new OptionSetValue((int)PaymentType.CreditCard);
            gopImg.Attributes[Gop.jarvis_goplimitin] = 200.00M;
            gopImg.Attributes[Gop.jarvis_goplimitout] = 200.00M;
            gopImg.Attributes["statuscode"] = new OptionSetValue(2);
            gopImg.Attributes["jarvis_isdealercopied"] = false;
            gopImg.Attributes["jarvis_triggeredby"] = "test";
            gopImg.Attributes["jarvis_triggeredby"] = new EntityReference("systemuser", Guid.NewGuid());


            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
            {
                new KeyValuePair<string, object>("Target", gopImg),
            };
            this.PluginExecutionContext.MessageNameGet = () => "Create";
            this.PluginExecutionContext.StageGet = () => 40;

            // Setting post-entity image.
            this.PluginExecutionContext.PostEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                    { "PostImage", gopImg },
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

            GopPostOperationAsync plugin = new GopPostOperationAsync("test", "test");

            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Positive scenario with user for GOP PostOperationAsync Create Scenario for ADD GOP Triggered
        /// Contact Should not be bull.
        /// </summary>
        [TestMethod]
        public void GopPostOperationAsyncTestAsMercuriusCreate2()
        {
            var incident = new Entity("incident"); // Create a mock or actual instance of the "incident" Entity
            incident.Id = Guid.NewGuid(); // Set a valid GUID for the "incident" entity's ID
            incident[Incident.caseOriginCode] = new OptionSetValue(2);
            incident[Incident.casetypecode] = new OptionSetValue(2);

            var gopImg = new Entity("jarvis_gop");
            gopImg.Attributes[Gop.totalLimitIn] = 500.00M;
            gopImg.Attributes[Gop.totalLimitOut] = 1000.00M;
            gopImg.Attributes[Gop.Source] = new OptionSetValue((int)Source.Jarvis);
            gopImg.Attributes[Gop.Approved] = true;
            gopImg.Attributes[Gop.contact] = "1a679c95-33f0-40e1-ae35-bd2e72ff8ac3";
            gopImg.Attributes[Gop.requestType] = new OptionSetValue((int)GopRequestType.GOP_HD);
            gopImg.Attributes[Casecontact.jarvisIncident] = new EntityReference("incident", Guid.NewGuid());
            gopImg.Attributes[Gop.jarvis_goplimitin] = 200.54M;
            gopImg.Attributes[Gop.jarvis_goplimitout] = 400.34M;
            gopImg.Attributes["statecode"] = new OptionSetValue(1);
            gopImg.Attributes[Gop.GopApproval] = new OptionSetValue((int)GopApproval.Approved);
            gopImg.Attributes[Gop.PaymentType] = new OptionSetValue(34399992);
            gopImg.Attributes[Gop.jarvis_goplimitin] = 200.00M;
            gopImg.Attributes[Gop.jarvis_goplimitout] = 200.00M;
            gopImg.Attributes["statuscode"] = new OptionSetValue(2);
            gopImg.Attributes["jarvis_isdealercopied"] = false;
            gopImg.Attributes["jarvis_triggeredby"] = new EntityReference("systemuser", Guid.NewGuid());


            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
            {
                new KeyValuePair<string, object>("Target", gopImg),
            };
            this.PluginExecutionContext.MessageNameGet = () => "Create";
            this.PluginExecutionContext.StageGet = () => 40;

            // Setting post-entity image.
            this.PluginExecutionContext.PostEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                    { "PostImage", gopImg },
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

            GopPostOperationAsync plugin = new GopPostOperationAsync("test", "test");

            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Positive scenario with user for GOP PostOperationAsync for Update Scenario.
        /// </summary>
        [TestMethod]
        public void GopPostOperationAsyncTestAsMercuriusUpdate()
        {
            var gopImg = new Entity("jarvis_gop");
            gopImg.Attributes[Gop.totalLimitIn] = 500.00M;
            gopImg.Attributes[Gop.totalLimitOut] = 1000.00M;
            gopImg.Attributes[Gop.Source] = new OptionSetValue((int)Source.Jarvis);
            gopImg.Attributes[Gop.Approved] = true;
            gopImg.Attributes[Gop.requestType] = new OptionSetValue((int)GopRequestType.GOP_HD);
            gopImg.Attributes[Casecontact.jarvisIncident] = new EntityReference("incident", Guid.NewGuid());
            gopImg.Attributes[Gop.jarvis_goplimitin] = 200.54M;
            gopImg.Attributes[Gop.jarvis_goplimitout] = 400.34M;
            gopImg.Attributes["statecode"] = new OptionSetValue(1);
            gopImg.Attributes[Gop.GopApproval] = new OptionSetValue((int)GopApproval.Approved);
            gopImg.Attributes[Gop.PaymentType] = new OptionSetValue((int)PaymentType.CreditCard);
            gopImg.Attributes[Gop.jarvis_goplimitin] = 200.00M;
            gopImg.Attributes[Gop.jarvis_goplimitout] = 200.00M;
            gopImg.Attributes["statuscode"] = new OptionSetValue(2);
            gopImg.Attributes["jarvis_isdealercopied"] = false;
            gopImg.Attributes["jarvis_triggeredby"] = new EntityReference("systemuser", Guid.NewGuid());

            var gopPreImg = new Entity("jarvis_gop");
            gopPreImg.Attributes[Gop.totalLimitIn] = 500.00M;
            gopPreImg.Attributes[Gop.totalLimitOut] = 1000.00M;
            gopPreImg.Attributes[Gop.Source] = new OptionSetValue((int)Source.Jarvis);
            gopPreImg.Attributes[Gop.Approved] = true;
            gopPreImg.Attributes[Gop.requestType] = new OptionSetValue((int)GopRequestType.GOP_HD);
            gopPreImg.Attributes[Casecontact.jarvisIncident] = new EntityReference("incident", Guid.NewGuid());
            gopPreImg.Attributes[Gop.jarvis_goplimitin] = 200.54M;
            gopPreImg.Attributes[Gop.jarvis_goplimitout] = 400.34M;
            gopPreImg.Attributes["statecode"] = new OptionSetValue(1);
            gopPreImg.Attributes[Gop.GopApproval] = new OptionSetValue((int)GopApproval.Approved);
            gopPreImg.Attributes[Gop.PaymentType] = new OptionSetValue((int)PaymentType.CreditCard);
            gopPreImg.Attributes[Gop.jarvis_goplimitin] = 200.00M;
            gopPreImg.Attributes[Gop.jarvis_goplimitout] = 200.00M;
            gopPreImg.Attributes["statuscode"] = new OptionSetValue(2);
            gopPreImg.Attributes["jarvis_isdealercopied"] = false;
            gopPreImg.Attributes["jarvis_triggeredby"] = new EntityReference("systemuser", Guid.NewGuid());


            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
            {
                new KeyValuePair<string, object>("Target", gopImg),
            };
            this.PluginExecutionContext.MessageNameGet = () => "Update";
            this.PluginExecutionContext.StageGet = () => 40;

            // Setting post-entity image.
            this.PluginExecutionContext.PostEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                    { "PostImage", gopImg },
                    { "preImage", gopPreImg },
                    { "ComparePreImage", gopPreImg },
                    { "ComparePostImage", gopImg },
                };
            };

            this.PluginExecutionContext.PreEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                    { "PostImage", gopImg },
                    { "preImage", gopPreImg },
                    { "ComparePreImage", gopPreImg },
                    { "ComparePostImage", gopImg },
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

            GopPostOperationAsync plugin = new GopPostOperationAsync("test", "test");

            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Positive scenario with user for GOP PostOperationAsync for Update Scenario.
        /// </summary>
        [TestMethod]
        public void GopPostOperationAsyncTestAsMercuriusUpdate2()
        {
            var gopImg = new Entity("jarvis_gop");
            gopImg.Attributes[Gop.totalLimitIn] = 500.00M;
            gopImg.Attributes[Gop.totalLimitOut] = 1000.00M;
            gopImg.Attributes[Gop.Source] = new OptionSetValue((int)Source.Jarvis);
            gopImg.Attributes[Gop.Approved] = true;
            gopImg.Attributes[Gop.requestType] = new OptionSetValue((int)GopRequestType.GOP_HD);
            gopImg.Attributes[Casecontact.jarvisIncident] = new EntityReference("incident", Guid.NewGuid());
            gopImg.Attributes[Gop.jarvis_goplimitin] = 200.54M;
            gopImg.Attributes[Gop.jarvis_goplimitout] = 400.34M;
            gopImg.Attributes["statecode"] = new OptionSetValue(1);
            gopImg.Attributes[Gop.GopApproval] = new OptionSetValue((int)GopApproval.Pending);
            gopImg.Attributes[Gop.PaymentType] = new OptionSetValue((int)PaymentType.CreditCard);
            gopImg.Attributes[Gop.jarvis_goplimitin] = 200.00M;
            gopImg.Attributes[Gop.jarvis_goplimitout] = 200.00M;
            gopImg.Attributes["statuscode"] = new OptionSetValue(334030001);
            gopImg.Attributes["jarvis_isdealercopied"] = false;
            gopImg.Attributes["jarvis_triggeredby"] = new EntityReference("systemuser", Guid.NewGuid());

            var gopPreImg = new Entity("jarvis_gop");
            gopPreImg.Attributes[Gop.totalLimitIn] = 500.00M;
            gopPreImg.Attributes[Gop.totalLimitOut] = 1000.00M;
            gopPreImg.Attributes[Gop.Source] = new OptionSetValue((int)Source.Jarvis);
            gopPreImg.Attributes[Gop.Approved] = true;
            gopPreImg.Attributes[Gop.requestType] = new OptionSetValue((int)GopRequestType.GOP_HD);
            gopPreImg.Attributes[Casecontact.jarvisIncident] = new EntityReference("incident", Guid.NewGuid());
            gopPreImg.Attributes[Gop.jarvis_goplimitin] = 200.54M;
            gopPreImg.Attributes[Gop.jarvis_goplimitout] = 400.34M;
            gopPreImg.Attributes["statecode"] = new OptionSetValue(1);
            gopPreImg.Attributes[Gop.GopApproval] = new OptionSetValue((int)GopApproval.Approved);
            gopPreImg.Attributes[Gop.PaymentType] = new OptionSetValue((int)PaymentType.CreditCard);
            gopPreImg.Attributes[Gop.jarvis_goplimitin] = 200.00M;
            gopPreImg.Attributes[Gop.jarvis_goplimitout] = 200.00M;
            gopPreImg.Attributes["statuscode"] = new OptionSetValue(2);
            gopPreImg.Attributes["jarvis_isdealercopied"] = false;
            gopPreImg.Attributes["jarvis_triggeredby"] = new EntityReference("systemuser", Guid.NewGuid());


            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
            {
                new KeyValuePair<string, object>("Target", gopImg),
            };
            this.PluginExecutionContext.MessageNameGet = () => "Update";
            this.PluginExecutionContext.StageGet = () => 40;

            // Setting post-entity image.
            this.PluginExecutionContext.PostEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                    { "PostImage", gopImg },
                    { "preImage", gopPreImg },
                    { "ComparePreImage", gopPreImg },
                    { "ComparePostImage", gopImg },
                };
            };

            this.PluginExecutionContext.PreEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                    { "PostImage", gopImg },
                    { "preImage", gopPreImg },
                    { "ComparePreImage", gopPreImg },
                    { "ComparePostImage", gopImg },
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

            GopPostOperationAsync plugin = new GopPostOperationAsync("test", "test");

            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Positive scenario with user for GOP PostOperationAsync for Update Scenario.
        /// </summary>
        [TestMethod]
        public void GopPostOperationAsyncTestAsMercuriusUpdate3()
        {
            var gopImg = new Entity("jarvis_gop");
            gopImg.Attributes[Gop.totalLimitIn] = 500.00M;
            gopImg.Attributes[Gop.totalLimitOut] = 1000.00M;
            gopImg.Attributes[Gop.Source] = new OptionSetValue((int)Source.Jarvis);
            gopImg.Attributes[Gop.Approved] = true;
            gopImg.Attributes[Gop.requestType] = new OptionSetValue((int)GopRequestType.GOP_HD);
            gopImg.Attributes[Casecontact.jarvisIncident] = new EntityReference("incident", Guid.NewGuid());
            gopImg.Attributes[Gop.jarvis_goplimitin] = 200.54M;
            gopImg.Attributes[Gop.jarvis_goplimitout] = 400.34M;
            gopImg.Attributes["statecode"] = new OptionSetValue(0);
            gopImg.Attributes[Gop.GopApproval] = new OptionSetValue((int)GopApproval.Approved);
            gopImg.Attributes[Gop.PaymentType] = new OptionSetValue((int)PaymentType.CreditCard);
            gopImg.Attributes[Gop.jarvis_goplimitin] = 200.00M;
            gopImg.Attributes[Gop.jarvis_goplimitout] = 200.00M;
            gopImg.Attributes["statuscode"] = new OptionSetValue(334030001);
            gopImg.Attributes["jarvis_isdealercopied"] = false;
            gopImg.Attributes["jarvis_triggeredby"] = new EntityReference("systemuser", Guid.NewGuid());
            gopImg.Attributes[Gop.contact] = "test";

            var gopPreImg = new Entity("jarvis_gop");
            gopPreImg.Attributes[Gop.totalLimitIn] = 500.00M;
            gopPreImg.Attributes[Gop.totalLimitOut] = 1000.00M;
            gopPreImg.Attributes[Gop.Source] = new OptionSetValue((int)Source.Jarvis);
            gopPreImg.Attributes[Gop.Approved] = true;
            gopPreImg.Attributes[Gop.requestType] = new OptionSetValue((int)GopRequestType.GOP_HD);
            gopPreImg.Attributes[Casecontact.jarvisIncident] = new EntityReference("incident", Guid.NewGuid());
            gopPreImg.Attributes[Gop.jarvis_goplimitin] = 200.54M;
            gopPreImg.Attributes[Gop.jarvis_goplimitout] = 400.34M;
            gopPreImg.Attributes["statecode"] = new OptionSetValue(0);
            gopPreImg.Attributes[Gop.GopApproval] = new OptionSetValue((int)GopApproval.Approved);
            gopPreImg.Attributes[Gop.PaymentType] = new OptionSetValue((int)PaymentType.CreditCard);
            gopPreImg.Attributes[Gop.jarvis_goplimitin] = 200.00M;
            gopPreImg.Attributes[Gop.jarvis_goplimitout] = 200.00M;
            gopPreImg.Attributes["statuscode"] = new OptionSetValue(2);
            gopPreImg.Attributes["jarvis_isdealercopied"] = false;
            gopPreImg.Attributes["jarvis_triggeredby"] = new EntityReference("systemuser", Guid.NewGuid());
            gopPreImg.Attributes[Gop.contact] = "test1";


            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
            {
                new KeyValuePair<string, object>("Target", gopImg),
            };
            this.PluginExecutionContext.MessageNameGet = () => "Update";
            this.PluginExecutionContext.StageGet = () => 40;

            // Setting post-entity image.
            this.PluginExecutionContext.PostEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                    { "PostImage", gopImg },
                    { "preImage", gopPreImg },
                    { "ComparePreImage", gopPreImg },
                    { "ComparePostImage", gopImg },
                };
            };

            this.PluginExecutionContext.PreEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                    { "PostImage", gopImg },
                    { "preImage", gopPreImg },
                    { "ComparePreImage", gopPreImg },
                    { "ComparePostImage", gopImg },
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

            GopPostOperationAsync plugin = new GopPostOperationAsync("test", "test");

            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Positive scenario with user for GOP PostOperationAsync for Update Scenario.
        /// </summary>
        [TestMethod]
        public void GopPostOperationAsyncTestAsMercuriusUpdate4()
        {
            var gopImg = new Entity("jarvis_gop");
            gopImg.Attributes[Gop.totalLimitIn] = 500.00M;
            gopImg.Attributes[Gop.totalLimitOut] = 1000.00M;
            gopImg.Attributes[Gop.Source] = new OptionSetValue((int)Source.Jarvis);
            gopImg.Attributes[Gop.Approved] = true;
            gopImg.Attributes[Gop.requestType] = new OptionSetValue((int)GopRequestType.GOP_HD);
            gopImg.Attributes[Casecontact.jarvisIncident] = new EntityReference("incident", Guid.NewGuid());
            gopImg.Attributes[Gop.jarvis_goplimitin] = 200.54M;
            gopImg.Attributes[Gop.jarvis_goplimitout] = 400.34M;
            gopImg.Attributes["statecode"] = new OptionSetValue(0);
            gopImg.Attributes[Gop.GopApproval] = new OptionSetValue((int)GopApproval.Approved);
            gopImg.Attributes[Gop.PaymentType] = new OptionSetValue((int)PaymentType.CreditCard);
            gopImg.Attributes[Gop.jarvis_goplimitin] = 200.00M;
            gopImg.Attributes[Gop.jarvis_goplimitout] = 200.00M;
            gopImg.Attributes["statuscode"] = new OptionSetValue(334030001);
            gopImg.Attributes["jarvis_isdealercopied"] = false;
            gopImg.Attributes["jarvis_triggeredby"] = new EntityReference("systemuser", Guid.NewGuid());
            gopImg.Attributes[Gop.contact] = "test";
            gopImg.Attributes[Gop.Comment] = "test";
            gopImg.Attributes[Gop.Translationstatuscomment] = new OptionSetValue((int)TranslationStatus.NotStarted);

            var gopPreImg = new Entity("jarvis_gop");
            gopPreImg.Attributes[Gop.totalLimitIn] = 500.00M;
            gopPreImg.Attributes[Gop.totalLimitOut] = 1000.00M;
            gopPreImg.Attributes[Gop.Source] = new OptionSetValue((int)Source.Jarvis);
            gopPreImg.Attributes[Gop.Approved] = true;
            gopPreImg.Attributes[Gop.requestType] = new OptionSetValue((int)GopRequestType.GOP_HD);
            gopPreImg.Attributes[Casecontact.jarvisIncident] = new EntityReference("incident", Guid.NewGuid());
            gopPreImg.Attributes[Gop.jarvis_goplimitin] = 200.54M;
            gopPreImg.Attributes[Gop.jarvis_goplimitout] = 400.34M;
            gopPreImg.Attributes["statecode"] = new OptionSetValue(0);
            gopPreImg.Attributes[Gop.GopApproval] = new OptionSetValue((int)GopApproval.Approved);
            gopPreImg.Attributes[Gop.PaymentType] = new OptionSetValue((int)PaymentType.CreditCard);
            gopPreImg.Attributes[Gop.jarvis_goplimitin] = 200.00M;
            gopPreImg.Attributes[Gop.jarvis_goplimitout] = 200.00M;
            gopPreImg.Attributes["statuscode"] = new OptionSetValue(2);
            gopPreImg.Attributes["jarvis_isdealercopied"] = false;
            gopPreImg.Attributes["jarvis_triggeredby"] = new EntityReference("systemuser", Guid.NewGuid());
            gopPreImg.Attributes[Gop.contact] = "test1";


            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
            {
                new KeyValuePair<string, object>("Target", gopImg),
            };
            this.PluginExecutionContext.MessageNameGet = () => "Update";
            this.PluginExecutionContext.StageGet = () => 40;

            // Setting post-entity image.
            this.PluginExecutionContext.PostEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                    { "PostImage", gopImg },
                    { "preImage", gopPreImg },
                    { "ComparePreImage", gopPreImg },
                    { "ComparePostImage", gopImg },
                };
            };

            this.PluginExecutionContext.PreEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                    { "PostImage", gopImg },
                    { "preImage", gopPreImg },
                    { "ComparePreImage", gopPreImg },
                    { "ComparePostImage", gopImg },
                };
            };

            // Setting service retrieve.
            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "systemuser")
                {
                    Entity user = new Entity("systemuser");
                    user.Id = Guid.NewGuid();
                    user["fullname"] = "Vivek";
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

            GopPostOperationAsync plugin = new GopPostOperationAsync("test", "test");

            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Positive scenario with user for GOP PostOperationAsync for Update Scenario.
        /// </summary>
        [TestMethod]
        public void GopPostOperationAsyncTestAsMercuriusUpdate5()
        {
            var gopImg = new Entity("jarvis_gop");
            gopImg.Attributes[Gop.totalLimitIn] = 500.00M;
            gopImg.Attributes[Gop.totalLimitOut] = 1000.00M;
            gopImg.Attributes[Gop.Source] = new OptionSetValue((int)Source.Jarvis);
            gopImg.Attributes[Gop.Approved] = true;
            gopImg.Attributes[Gop.requestType] = new OptionSetValue((int)GopRequestType.GOP_HD);
            gopImg.Attributes[Casecontact.jarvisIncident] = new EntityReference("incident", Guid.NewGuid());
            gopImg.Attributes[Gop.jarvis_goplimitin] = 200.54M;
            gopImg.Attributes[Gop.jarvis_goplimitout] = 400.34M;
            gopImg.Attributes["statecode"] = new OptionSetValue(0);
            gopImg.Attributes[Gop.GopApproval] = new OptionSetValue((int)GopApproval.Approved);
            gopImg.Attributes[Gop.PaymentType] = new OptionSetValue((int)PaymentType.CreditCard);
            gopImg.Attributes[Gop.jarvis_goplimitin] = 200.00M;
            gopImg.Attributes[Gop.jarvis_goplimitout] = 200.00M;
            gopImg.Attributes["statuscode"] = new OptionSetValue(334030001);
            gopImg.Attributes["jarvis_isdealercopied"] = false;
            gopImg.Attributes[Gop.contact] = "test";
            gopImg.Attributes[Gop.Comment] = "test";
            gopImg.Attributes[Gop.Translationstatuscomment] = new OptionSetValue((int)TranslationStatus.NotStarted);

            var gopPreImg = new Entity("jarvis_gop");
            gopPreImg.Attributes[Gop.totalLimitIn] = 500.00M;
            gopPreImg.Attributes[Gop.totalLimitOut] = 1000.00M;
            gopPreImg.Attributes[Gop.Source] = new OptionSetValue((int)Source.Jarvis);
            gopPreImg.Attributes[Gop.Approved] = true;
            gopPreImg.Attributes[Gop.requestType] = new OptionSetValue((int)GopRequestType.GOP_HD);
            gopPreImg.Attributes[Casecontact.jarvisIncident] = new EntityReference("incident", Guid.NewGuid());
            gopPreImg.Attributes[Gop.jarvis_goplimitin] = 200.54M;
            gopPreImg.Attributes[Gop.jarvis_goplimitout] = 400.34M;
            gopPreImg.Attributes["statecode"] = new OptionSetValue(0);
            gopPreImg.Attributes[Gop.GopApproval] = new OptionSetValue((int)GopApproval.Approved);
            gopPreImg.Attributes[Gop.PaymentType] = new OptionSetValue((int)PaymentType.CreditCard);
            gopPreImg.Attributes[Gop.jarvis_goplimitin] = 200.00M;
            gopPreImg.Attributes[Gop.jarvis_goplimitout] = 200.00M;
            gopPreImg.Attributes["statuscode"] = new OptionSetValue(2);
            gopPreImg.Attributes["jarvis_isdealercopied"] = false;
            gopPreImg.Attributes[Gop.contact] = "test1";


            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
            {
                new KeyValuePair<string, object>("Target", gopImg),
            };
            this.PluginExecutionContext.MessageNameGet = () => "Update";
            this.PluginExecutionContext.StageGet = () => 40;

            // Setting post-entity image.
            this.PluginExecutionContext.PostEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                    { "PostImage", gopImg },
                    { "preImage", gopPreImg },
                    { "ComparePreImage", gopPreImg },
                    { "ComparePostImage", gopImg },
                };
            };

            this.PluginExecutionContext.PreEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                    { "PostImage", gopImg },
                    { "preImage", gopPreImg },
                    { "ComparePreImage", gopPreImg },
                    { "ComparePostImage", gopImg },
                };
            };

            // Setting service retrieve.
            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "systemuser")
                {
                    Entity user = new Entity("systemuser");
                    user.Id = Guid.NewGuid();
                    user["fullname"] = "Vivek";
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

            GopPostOperationAsync plugin = new GopPostOperationAsync("test", "test");

            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Positive scenario with user for GOP PostOperationAsync for Update Scenario.
        /// </summary>
        [TestMethod]
        public void GopPostOperationAsyncTestAsMercuriusUpdate6()
        {
            var gopImg = new Entity("jarvis_gop");
            gopImg.Attributes[Gop.Source] = new OptionSetValue((int)Source.Jarvis);
            gopImg.Attributes[Gop.Approved] = true;
            gopImg.Attributes[Gop.requestType] = new OptionSetValue((int)GopRequestType.GOP_HD);
            gopImg.Attributes[Casecontact.jarvisIncident] = new EntityReference("incident", Guid.NewGuid());
            gopImg.Attributes[Gop.jarvis_goplimitin] = 200.54M;
            gopImg.Attributes[Gop.jarvis_goplimitout] = 400.34M;
            gopImg.Attributes["statecode"] = new OptionSetValue(3);
            gopImg.Attributes[Gop.GopApproval] = new OptionSetValue((int)GopApproval.Approved);
            gopImg.Attributes[Gop.PaymentType] = new OptionSetValue((int)PaymentType.CreditCard);
            gopImg.Attributes[Gop.jarvis_goplimitin] = 200.00M;
            gopImg.Attributes[Gop.jarvis_goplimitout] = 200.00M;
            gopImg.Attributes["statuscode"] = new OptionSetValue(334030001);
            gopImg.Attributes["jarvis_isdealercopied"] = false;
            gopImg.Attributes[Gop.contact] = "test";
            gopImg.Attributes[Gop.Comment] = "test";
            gopImg.Attributes[Gop.Translationstatuscomment] = new OptionSetValue((int)TranslationStatus.NotStarted);
            gopImg.Attributes[Gop.Translationstatusgopreason] = new OptionSetValue((int)TranslationStatus.NotStarted);
            gopImg.Attributes["jarvis_triggeredby"] = new EntityReference("systemuser", Guid.NewGuid());

            var gopPreImg = new Entity("jarvis_gop");
            gopPreImg.Attributes[Gop.Source] = new OptionSetValue((int)Source.Jarvis);
            gopPreImg.Attributes[Gop.Approved] = true;
            gopPreImg.Attributes[Gop.requestType] = new OptionSetValue((int)GopRequestType.GOP_HD);
            gopPreImg.Attributes[Casecontact.jarvisIncident] = new EntityReference("incident", Guid.NewGuid());
            gopPreImg.Attributes[Gop.jarvis_goplimitin] = 200.54M;
            gopPreImg.Attributes[Gop.jarvis_goplimitout] = 400.34M;
            gopPreImg.Attributes["statecode"] = new OptionSetValue(0);
            gopPreImg.Attributes[Gop.GopApproval] = new OptionSetValue((int)GopApproval.Approved);
            gopPreImg.Attributes[Gop.PaymentType] = new OptionSetValue((int)PaymentType.CreditCard);
            gopPreImg.Attributes[Gop.jarvis_goplimitin] = 200.00M;
            gopPreImg.Attributes[Gop.jarvis_goplimitout] = 200.00M;
            gopPreImg.Attributes["statuscode"] = new OptionSetValue(2);
            gopPreImg.Attributes["jarvis_isdealercopied"] = false;
            gopPreImg.Attributes[Gop.contact] = "test1";
            gopPreImg.Attributes[Gop.Translationstatuscomment] = new OptionSetValue((int)TranslationStatus.InProgress);
            gopPreImg.Attributes[Gop.Translationstatusgopreason] = new OptionSetValue((int)TranslationStatus.InProgress);
            gopPreImg.Attributes["jarvis_triggeredby"] = new EntityReference("systemuser", Guid.NewGuid());


            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
            {
                new KeyValuePair<string, object>("Target", gopImg),
            };
            this.PluginExecutionContext.MessageNameGet = () => "Update";
            this.PluginExecutionContext.StageGet = () => 40;

            // Setting post-entity image.
            this.PluginExecutionContext.PostEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                    { "PostImage", gopImg },
                    { "preImage", gopPreImg },
                    { "ComparePreImage", gopPreImg },
                    { "ComparePostImage", gopImg },
                };
            };

            this.PluginExecutionContext.PreEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                    { "PostImage", gopImg },
                    { "preImage", gopPreImg },
                    { "ComparePreImage", gopPreImg },
                    { "ComparePostImage", gopImg },
                };
            };

            // Setting service retrieve.
            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "systemuser")
                {
                    Entity user = new Entity("systemuser");
                    user.Id = Guid.NewGuid();
                    user["fullname"] = "Vivek";
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

            GopPostOperationAsync plugin = new GopPostOperationAsync("test", "test");

            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Positive scenario with user for GOP PostOperationAsync for Update Scenario.
        /// </summary>
        [TestMethod]
        public void GopPostOperationAsyncTestAsMercuriusUpdate7()
        {
            var gopImg = new Entity("jarvis_gop");
            gopImg.Attributes[Gop.Source] = new OptionSetValue((int)Source.Jarvis);
            gopImg.Attributes[Gop.Approved] = true;
            gopImg.Attributes[Gop.requestType] = new OptionSetValue((int)GopRequestType.GOP_HD);
            gopImg.Attributes[Casecontact.jarvisIncident] = new EntityReference("incident", Guid.NewGuid());
            gopImg.Attributes[Gop.jarvis_goplimitin] = 200.54M;
            gopImg.Attributes[Gop.jarvis_goplimitout] = 400.34M;
            gopImg.Attributes["statecode"] = new OptionSetValue(3);
            gopImg.Attributes[Gop.GopApproval] = new OptionSetValue((int)GopApproval.Approved);
            gopImg.Attributes[Gop.PaymentType] = new OptionSetValue(34323000);
            gopImg.Attributes[Gop.jarvis_goplimitin] = 200.00M;
            gopImg.Attributes[Gop.jarvis_goplimitout] = 200.00M;
            gopImg.Attributes["statuscode"] = new OptionSetValue(334030001);
            gopImg.Attributes["jarvis_isdealercopied"] = false;
            gopImg.Attributes[Gop.contact] = "test";
            gopImg.Attributes[Gop.Comment] = "test";
            gopImg.Attributes[Gop.Translationstatuscomment] = new OptionSetValue((int)TranslationStatus.NotStarted);
            gopImg.Attributes[Gop.Translationstatusgopreason] = new OptionSetValue((int)TranslationStatus.NotStarted);
            gopImg.Attributes["jarvis_triggeredby"] = new EntityReference("systemuser", Guid.NewGuid());

            var gopPreImg = new Entity("jarvis_gop");
            gopPreImg.Attributes[Gop.Source] = new OptionSetValue((int)Source.Jarvis);
            gopPreImg.Attributes[Gop.Approved] = true;
            gopPreImg.Attributes[Gop.requestType] = new OptionSetValue((int)GopRequestType.GOP_HD);
            gopPreImg.Attributes[Casecontact.jarvisIncident] = new EntityReference("incident", Guid.NewGuid());
            gopPreImg.Attributes[Gop.jarvis_goplimitin] = 200.54M;
            gopPreImg.Attributes[Gop.jarvis_goplimitout] = 400.34M;
            gopPreImg.Attributes["statecode"] = new OptionSetValue(0);
            gopPreImg.Attributes[Gop.GopApproval] = new OptionSetValue((int)GopApproval.Approved);
            gopPreImg.Attributes[Gop.PaymentType] = new OptionSetValue(34323000);
            gopPreImg.Attributes[Gop.jarvis_goplimitin] = 200.00M;
            gopPreImg.Attributes[Gop.jarvis_goplimitout] = 200.00M;
            gopPreImg.Attributes["statuscode"] = new OptionSetValue(2);
            gopPreImg.Attributes["jarvis_isdealercopied"] = false;
            gopPreImg.Attributes[Gop.contact] = "test1";
            gopPreImg.Attributes[Gop.Translationstatuscomment] = new OptionSetValue((int)TranslationStatus.InProgress);
            gopPreImg.Attributes[Gop.Translationstatusgopreason] = new OptionSetValue((int)TranslationStatus.InProgress);
            gopPreImg.Attributes["jarvis_triggeredby"] = new EntityReference("systemuser", Guid.NewGuid());


            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
            {
                new KeyValuePair<string, object>("Target", gopImg),
            };
            this.PluginExecutionContext.MessageNameGet = () => "Update";
            this.PluginExecutionContext.StageGet = () => 40;

            // Setting post-entity image.
            this.PluginExecutionContext.PostEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                    { "PostImage", gopImg },
                    { "preImage", gopPreImg },
                    { "ComparePreImage", gopPreImg },
                    { "ComparePostImage", gopImg },
                };
            };

            this.PluginExecutionContext.PreEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                    { "PostImage", gopImg },
                    { "preImage", gopPreImg },
                    { "ComparePreImage", gopPreImg },
                    { "ComparePostImage", gopImg },
                };
            };

            // Setting service retrieve.
            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "systemuser")
                {
                    Entity user = new Entity("systemuser");
                    user.Id = Guid.NewGuid();
                    user["fullname"] = "Vivek";
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

            GopPostOperationAsync plugin = new GopPostOperationAsync("test", "test");

            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Positive scenario with user for GOP PostOperationAsync for Update Scenario.
        /// </summary>
        [TestMethod]
        public void GopPostOperationAsyncTestAsMercuriusUpdate8()
        {
            var gopImg = new Entity("jarvis_gop");
            gopImg.Attributes[Gop.Source] = new OptionSetValue((int)Source.Jarvis);
            gopImg.Attributes[Gop.Approved] = true;
            gopImg.Attributes[Gop.requestType] = new OptionSetValue((int)GopRequestType.GOP_HD);
            gopImg.Attributes[Casecontact.jarvisIncident] = new EntityReference("incident", Guid.NewGuid());
            gopImg.Attributes[Gop.jarvis_goplimitin] = 200.54M;
            gopImg.Attributes[Gop.jarvis_goplimitout] = 400.34M;
            gopImg.Attributes["statecode"] = new OptionSetValue(3);
            gopImg.Attributes[Gop.GopApproval] = new OptionSetValue((int)GopApproval.Approved);
            gopImg.Attributes[Gop.PaymentType] = new OptionSetValue(34323000);
            gopImg.Attributes[Gop.jarvis_goplimitin] = 200.00M;
            gopImg.Attributes[Gop.jarvis_goplimitout] = 200.00M;
            gopImg.Attributes["statuscode"] = new OptionSetValue(334030001);
            gopImg.Attributes["jarvis_isdealercopied"] = false;
            gopImg.Attributes[Gop.contact] = "test";
            gopImg.Attributes[Gop.Comment] = "test";
            gopImg.Attributes[Gop.Translationstatuscomment] = new OptionSetValue((int)TranslationStatus.NotStarted);
            gopImg.Attributes[Gop.Translationstatusgopreason] = new OptionSetValue((int)TranslationStatus.NotStarted);

            var gopPreImg = new Entity("jarvis_gop");
            gopPreImg.Attributes[Gop.Source] = new OptionSetValue((int)Source.Jarvis);
            gopPreImg.Attributes[Gop.Approved] = true;
            gopPreImg.Attributes[Gop.requestType] = new OptionSetValue((int)GopRequestType.GOP_HD);
            gopPreImg.Attributes[Casecontact.jarvisIncident] = new EntityReference("incident", Guid.NewGuid());
            gopPreImg.Attributes[Gop.jarvis_goplimitin] = 200.54M;
            gopPreImg.Attributes[Gop.jarvis_goplimitout] = 400.34M;
            gopPreImg.Attributes["statecode"] = new OptionSetValue(0);
            gopPreImg.Attributes[Gop.GopApproval] = new OptionSetValue((int)GopApproval.Approved);
            gopPreImg.Attributes[Gop.PaymentType] = new OptionSetValue(34323000);
            gopPreImg.Attributes[Gop.jarvis_goplimitin] = 200.00M;
            gopPreImg.Attributes[Gop.jarvis_goplimitout] = 200.00M;
            gopPreImg.Attributes["statuscode"] = new OptionSetValue(2);
            gopPreImg.Attributes["jarvis_isdealercopied"] = false;
            gopPreImg.Attributes[Gop.contact] = "test1";
            gopPreImg.Attributes[Gop.Translationstatuscomment] = new OptionSetValue((int)TranslationStatus.InProgress);
            gopPreImg.Attributes[Gop.Translationstatusgopreason] = new OptionSetValue((int)TranslationStatus.InProgress);


            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
            {
                new KeyValuePair<string, object>("Target", gopImg),
            };
            this.PluginExecutionContext.MessageNameGet = () => "Update";
            this.PluginExecutionContext.StageGet = () => 40;

            // Setting post-entity image.
            this.PluginExecutionContext.PostEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                    { "PostImage", gopImg },
                    { "preImage", gopPreImg },
                    { "ComparePreImage", gopPreImg },
                    { "ComparePostImage", gopImg },
                };
            };

            this.PluginExecutionContext.PreEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                    { "PostImage", gopImg },
                    { "preImage", gopPreImg },
                    { "ComparePreImage", gopPreImg },
                    { "ComparePostImage", gopImg },
                };
            };

            // Setting service retrieve.
            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
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

            GopPostOperationAsync plugin = new GopPostOperationAsync("test", "test");

            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Positive scenario with user for GOP PostOperationAsync for Update Scenario.
        /// </summary>
        [TestMethod]
        public void GopPostOperationAsyncTestAsMercuriusUpdate9()
        {
            var gopImg = new Entity("jarvis_gop");
            gopImg.Attributes[Gop.Source] = new OptionSetValue((int)Source.Jarvis);
            gopImg.Attributes[Gop.Approved] = true;
            gopImg.Attributes[Gop.requestType] = new OptionSetValue((int)GopRequestType.GOP_HD);
            gopImg.Attributes[Casecontact.jarvisIncident] = new EntityReference("incident", Guid.NewGuid());
            gopImg.Attributes[Gop.jarvis_goplimitin] = 200.54M;
            gopImg.Attributes[Gop.jarvis_goplimitout] = 400.34M;
            gopImg.Attributes["statecode"] = new OptionSetValue(3);
            gopImg.Attributes[Gop.GopApproval] = new OptionSetValue((int)GopApproval.Approved);
            gopImg.Attributes[Gop.PaymentType] = new OptionSetValue((int)PaymentType.CreditCard);
            gopImg.Attributes[Gop.jarvis_goplimitin] = 200.00M;
            gopImg.Attributes[Gop.jarvis_goplimitout] = 200.00M;
            gopImg.Attributes["statuscode"] = new OptionSetValue(334030001);
            gopImg.Attributes["jarvis_isdealercopied"] = false;
            gopImg.Attributes[Gop.contact] = "test";
            gopImg.Attributes[Gop.Comment] = "test";
            gopImg.Attributes[Gop.Translationstatuscomment] = new OptionSetValue((int)TranslationStatus.NotStarted);
            gopImg.Attributes[Gop.Translationstatusgopreason] = new OptionSetValue((int)TranslationStatus.NotStarted);

            var gopPreImg = new Entity("jarvis_gop");
            gopPreImg.Attributes[Gop.Source] = new OptionSetValue((int)Source.Jarvis);
            gopPreImg.Attributes[Gop.Approved] = true;
            gopPreImg.Attributes[Gop.requestType] = new OptionSetValue((int)GopRequestType.GOP_HD);
            gopPreImg.Attributes[Casecontact.jarvisIncident] = new EntityReference("incident", Guid.NewGuid());
            gopPreImg.Attributes[Gop.jarvis_goplimitin] = 200.54M;
            gopPreImg.Attributes[Gop.jarvis_goplimitout] = 400.34M;
            gopPreImg.Attributes["statecode"] = new OptionSetValue(0);
            gopPreImg.Attributes[Gop.GopApproval] = new OptionSetValue((int)GopApproval.Approved);
            gopPreImg.Attributes[Gop.PaymentType] = new OptionSetValue((int)PaymentType.CreditCard);
            gopPreImg.Attributes[Gop.jarvis_goplimitin] = 200.00M;
            gopPreImg.Attributes[Gop.jarvis_goplimitout] = 200.00M;
            gopPreImg.Attributes["statuscode"] = new OptionSetValue(2);
            gopPreImg.Attributes["jarvis_isdealercopied"] = false;
            gopPreImg.Attributes[Gop.contact] = "test1";
            gopPreImg.Attributes[Gop.Translationstatuscomment] = new OptionSetValue((int)TranslationStatus.InProgress);
            gopPreImg.Attributes[Gop.Translationstatusgopreason] = new OptionSetValue((int)TranslationStatus.InProgress);


            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
            {
                new KeyValuePair<string, object>("Target", gopImg),
            };
            this.PluginExecutionContext.MessageNameGet = () => "Update";
            this.PluginExecutionContext.StageGet = () => 40;

            // Setting post-entity image.
            this.PluginExecutionContext.PostEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                    { "PostImage", gopImg },
                    { "preImage", gopPreImg },
                    { "ComparePreImage", gopPreImg },
                    { "ComparePostImage", gopImg },
                };
            };

            this.PluginExecutionContext.PreEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                    { "PostImage", gopImg },
                    { "preImage", gopPreImg },
                    { "ComparePreImage", gopPreImg },
                    { "ComparePostImage", gopImg },
                };
            };

            // Setting service retrieve.
            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
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

            GopPostOperationAsync plugin = new GopPostOperationAsync("test", "test");

            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Positive scenario with user for GOP PostOperationAsync for Update Scenario.
        /// </summary>
        [TestMethod]
        public void GopPostOperationAsyncTestAsMercuriusDepth()
        {
            var gopImg = new Entity("jarvis_gop");
            gopImg.Attributes[Gop.Source] = new OptionSetValue((int)Source.Jarvis);
            gopImg.Attributes[Gop.Approved] = true;
            gopImg.Attributes[Gop.requestType] = new OptionSetValue((int)GopRequestType.GOP_HD);
            gopImg.Attributes[Casecontact.jarvisIncident] = new EntityReference("incident", Guid.NewGuid());
            gopImg.Attributes[Gop.jarvis_goplimitin] = 200.54M;
            gopImg.Attributes[Gop.jarvis_goplimitout] = 400.34M;
            gopImg.Attributes["statecode"] = new OptionSetValue(3);
            gopImg.Attributes[Gop.GopApproval] = new OptionSetValue((int)GopApproval.Approved);
            gopImg.Attributes[Gop.PaymentType] = new OptionSetValue((int)PaymentType.CreditCard);
            gopImg.Attributes[Gop.jarvis_goplimitin] = 200.00M;
            gopImg.Attributes[Gop.jarvis_goplimitout] = 200.00M;
            gopImg.Attributes["statuscode"] = new OptionSetValue(334030001);
            gopImg.Attributes["jarvis_isdealercopied"] = false;
            gopImg.Attributes[Gop.contact] = "test";
            gopImg.Attributes[Gop.Comment] = "test";
            gopImg.Attributes[Gop.Translationstatuscomment] = new OptionSetValue((int)TranslationStatus.NotStarted);
            gopImg.Attributes[Gop.Translationstatusgopreason] = new OptionSetValue((int)TranslationStatus.NotStarted);

            var gopPreImg = new Entity("jarvis_gop");
            gopPreImg.Attributes[Gop.Source] = new OptionSetValue((int)Source.Jarvis);
            gopPreImg.Attributes[Gop.Approved] = true;
            gopPreImg.Attributes[Gop.requestType] = new OptionSetValue((int)GopRequestType.GOP_HD);
            gopPreImg.Attributes[Casecontact.jarvisIncident] = new EntityReference("incident", Guid.NewGuid());
            gopPreImg.Attributes[Gop.jarvis_goplimitin] = 200.54M;
            gopPreImg.Attributes[Gop.jarvis_goplimitout] = 400.34M;
            gopPreImg.Attributes["statecode"] = new OptionSetValue(0);
            gopPreImg.Attributes[Gop.GopApproval] = new OptionSetValue((int)GopApproval.Approved);
            gopPreImg.Attributes[Gop.PaymentType] = new OptionSetValue((int)PaymentType.CreditCard);
            gopPreImg.Attributes[Gop.jarvis_goplimitin] = 200.00M;
            gopPreImg.Attributes[Gop.jarvis_goplimitout] = 200.00M;
            gopPreImg.Attributes["statuscode"] = new OptionSetValue(2);
            gopPreImg.Attributes["jarvis_isdealercopied"] = false;
            gopPreImg.Attributes[Gop.contact] = "test1";
            gopPreImg.Attributes[Gop.Translationstatuscomment] = new OptionSetValue((int)TranslationStatus.InProgress);
            gopPreImg.Attributes[Gop.Translationstatusgopreason] = new OptionSetValue((int)TranslationStatus.InProgress);

            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
            {
                new KeyValuePair<string, object>("Target", gopImg),
            };
            this.PluginExecutionContext.MessageNameGet = () => "Update";
            this.PluginExecutionContext.StageGet = () => 40;
            this.PluginExecutionContext.DepthGet = () => 3;

            // Setting post-entity image.
            this.PluginExecutionContext.PostEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                    { "PostImage", gopImg },
                    { "preImage", gopPreImg },
                    { "ComparePreImage", gopPreImg },
                    { "ComparePostImage", gopImg },
                };
            };

            this.PluginExecutionContext.PreEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                    { "PostImage", gopImg },
                    { "preImage", gopPreImg },
                    { "ComparePreImage", gopPreImg },
                    { "ComparePostImage", gopImg },
                };
            };

            // Setting service retrieve.
            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
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

            GopPostOperationAsync plugin = new GopPostOperationAsync("test", "test");

            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Positive scenario with user for GOP PostOperationAsync for Update Scenario.
        /// </summary>
        [TestMethod]
        public void GopPostOperationAsyncTestAsMercuriusUpdate10()
        {
            var gopImg = new Entity("jarvis_gop");
            gopImg.Attributes[Gop.Source] = new OptionSetValue((int)Source.Jarvis);
            gopImg.Attributes[Gop.Approved] = true;
            gopImg.Attributes[Gop.requestType] = new OptionSetValue((int)GopRequestType.GOP_RD);
            gopImg.Attributes[Casecontact.jarvisIncident] = new EntityReference("incident", Guid.NewGuid());
            gopImg.Attributes[Gop.jarvis_goplimitin] = 200.54M;
            gopImg.Attributes[Gop.jarvis_goplimitout] = 400.34M;
            gopImg.Attributes["statecode"] = new OptionSetValue(3);
            gopImg.Attributes[Gop.GopApproval] = new OptionSetValue((int)GopApproval.Approved);
            gopImg.Attributes[Gop.PaymentType] = new OptionSetValue((int)PaymentType.CreditCard);
            gopImg.Attributes[Gop.jarvis_goplimitin] = 200.00M;
            gopImg.Attributes[Gop.jarvis_goplimitout] = 200.00M;
            gopImg.Attributes["statuscode"] = new OptionSetValue(334030001);
            gopImg.Attributes["jarvis_isdealercopied"] = false;
            gopImg.Attributes[Gop.contact] = "test";
            gopImg.Attributes[Gop.Comment] = "test";
            gopImg.Attributes[Gop.GopReason] = "test";
            gopImg.Attributes[Gop.Translationstatuscomment] = new OptionSetValue((int)TranslationStatus.NotStarted);
            gopImg.Attributes[Gop.Translationstatusgopreason] = new OptionSetValue((int)TranslationStatus.NotStarted);

            var gopPreImg = new Entity("jarvis_gop");
            gopPreImg.Attributes[Gop.Source] = new OptionSetValue((int)Source.Jarvis);
            gopPreImg.Attributes[Gop.Approved] = true;
            gopPreImg.Attributes[Gop.requestType] = new OptionSetValue((int)GopRequestType.GOP_RD);
            gopPreImg.Attributes[Casecontact.jarvisIncident] = new EntityReference("incident", Guid.NewGuid());
            gopPreImg.Attributes[Gop.jarvis_goplimitin] = 200.54M;
            gopPreImg.Attributes[Gop.jarvis_goplimitout] = 400.34M;
            gopPreImg.Attributes["statecode"] = new OptionSetValue(0);
            gopPreImg.Attributes[Gop.GopApproval] = new OptionSetValue((int)GopApproval.Approved);
            gopPreImg.Attributes[Gop.PaymentType] = new OptionSetValue((int)PaymentType.CreditCard);
            gopPreImg.Attributes[Gop.jarvis_goplimitin] = 200.00M;
            gopPreImg.Attributes[Gop.jarvis_goplimitout] = 200.00M;
            gopPreImg.Attributes["statuscode"] = new OptionSetValue(2);
            gopPreImg.Attributes["jarvis_isdealercopied"] = false;
            gopPreImg.Attributes[Gop.contact] = "test1";
            gopPreImg.Attributes[Gop.GopReason] = "test";
            gopPreImg.Attributes[Gop.Translationstatuscomment] = new OptionSetValue((int)TranslationStatus.InProgress);
            gopPreImg.Attributes[Gop.Translationstatusgopreason] = new OptionSetValue((int)TranslationStatus.InProgress);


            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
            {
                new KeyValuePair<string, object>("Target", gopImg),
            };
            this.PluginExecutionContext.MessageNameGet = () => "Update";
            this.PluginExecutionContext.StageGet = () => 40;

            // Setting post-entity image.
            this.PluginExecutionContext.PostEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                    { "PostImage", gopImg },
                    { "preImage", gopPreImg },
                    { "ComparePreImage", gopPreImg },
                    { "ComparePostImage", gopImg },
                };
            };

            this.PluginExecutionContext.PreEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                    { "PostImage", gopImg },
                    { "preImage", gopPreImg },
                    { "ComparePreImage", gopPreImg },
                    { "ComparePostImage", gopImg },
                };
            };

            // Setting service retrieve.
            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
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

            GopPostOperationAsync plugin = new GopPostOperationAsync("test", "test");

            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Positive scenario with user for GOP PostOperationAsync for Update Scenario.
        /// </summary>
        [TestMethod]
        public void GopPostOperationAsyncTestAsMercuriusUpdate11()
        {
            var gopImg = new Entity("jarvis_gop");
            gopImg.Attributes[Gop.Source] = new OptionSetValue((int)Source.Jarvis);
            gopImg.Attributes[Gop.Approved] = true;
            gopImg.Attributes[Gop.requestType] = new OptionSetValue((int)GopRequestType.GOP_RD);
            gopImg.Attributes[Casecontact.jarvisIncident] = new EntityReference("incident", Guid.NewGuid());
            gopImg.Attributes[Gop.jarvis_goplimitin] = 200.54M;
            gopImg.Attributes[Gop.jarvis_goplimitout] = 400.34M;
            gopImg.Attributes["statecode"] = new OptionSetValue(3);
            gopImg.Attributes[Gop.GopApproval] = new OptionSetValue((int)GopApproval.Approved);
            gopImg.Attributes[Gop.PaymentType] = new OptionSetValue(34323000);
            gopImg.Attributes[Gop.jarvis_goplimitin] = 200.00M;
            gopImg.Attributes[Gop.jarvis_goplimitout] = 200.00M;
            gopImg.Attributes["statuscode"] = new OptionSetValue(334030001);
            gopImg.Attributes["jarvis_isdealercopied"] = false;
            gopImg.Attributes[Gop.contact] = "test";
            gopImg.Attributes[Gop.Comment] = "test";
            gopImg.Attributes[Gop.GopReason] = "test";
            gopImg.Attributes[Gop.Translationstatuscomment] = new OptionSetValue((int)TranslationStatus.NotStarted);
            gopImg.Attributes[Gop.Translationstatusgopreason] = new OptionSetValue((int)TranslationStatus.NotStarted);
            gopImg.Attributes["jarvis_triggeredby"] = new EntityReference("systemuser", Guid.NewGuid());

            var gopPreImg = new Entity("jarvis_gop");
            gopPreImg.Attributes[Gop.Source] = new OptionSetValue((int)Source.Jarvis);
            gopPreImg.Attributes[Gop.Approved] = true;
            gopPreImg.Attributes[Gop.requestType] = new OptionSetValue((int)GopRequestType.GOP_RD);
            gopPreImg.Attributes[Casecontact.jarvisIncident] = new EntityReference("incident", Guid.NewGuid());
            gopPreImg.Attributes[Gop.jarvis_goplimitin] = 200.54M;
            gopPreImg.Attributes[Gop.jarvis_goplimitout] = 400.34M;
            gopPreImg.Attributes["statecode"] = new OptionSetValue(0);
            gopPreImg.Attributes[Gop.GopApproval] = new OptionSetValue((int)GopApproval.Approved);
            gopPreImg.Attributes[Gop.PaymentType] = new OptionSetValue(34323000);
            gopPreImg.Attributes[Gop.jarvis_goplimitin] = 200.00M;
            gopPreImg.Attributes[Gop.jarvis_goplimitout] = 200.00M;
            gopPreImg.Attributes["statuscode"] = new OptionSetValue(2);
            gopPreImg.Attributes["jarvis_isdealercopied"] = false;
            gopPreImg.Attributes[Gop.contact] = "test1";
            gopPreImg.Attributes[Gop.GopReason] = "test";
            gopPreImg.Attributes[Gop.Translationstatuscomment] = new OptionSetValue((int)TranslationStatus.InProgress);
            gopPreImg.Attributes[Gop.Translationstatusgopreason] = new OptionSetValue((int)TranslationStatus.InProgress);
            gopPreImg.Attributes["jarvis_triggeredby"] = new EntityReference("systemuser", Guid.NewGuid());


            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
            {
                new KeyValuePair<string, object>("Target", gopImg),
            };
            this.PluginExecutionContext.MessageNameGet = () => "Update";
            this.PluginExecutionContext.StageGet = () => 40;

            // Setting post-entity image.
            this.PluginExecutionContext.PostEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                    { "PostImage", gopImg },
                    { "preImage", gopPreImg },
                    { "ComparePreImage", gopPreImg },
                    { "ComparePostImage", gopImg },
                };
            };

            this.PluginExecutionContext.PreEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                    { "PostImage", gopImg },
                    { "preImage", gopPreImg },
                    { "ComparePreImage", gopPreImg },
                    { "ComparePostImage", gopImg },
                };
            };

            // Setting service retrieve.
            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "systemuser")
                {
                    Entity user = new Entity("systemuser");
                    user.Id = Guid.NewGuid();
                    user["fullname"] = "Vivek";
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

            GopPostOperationAsync plugin = new GopPostOperationAsync("test", "test");

            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Positive scenario with user for GOP PostOperationAsync for Update Scenario.
        /// </summary>
        [TestMethod]
        public void GopPostOperationAsyncTestAsMercuriusUpdate12()
        {
            var gopImg = new Entity("jarvis_gop");
            gopImg.Attributes[Gop.Source] = new OptionSetValue((int)Source.Jarvis);
            gopImg.Attributes[Gop.Approved] = true;
            gopImg.Attributes[Gop.requestType] = new OptionSetValue((int)GopRequestType.GOP_RD);
            gopImg.Attributes[Casecontact.jarvisIncident] = new EntityReference("incident", Guid.NewGuid());
            gopImg.Attributes[Gop.jarvis_goplimitin] = 200.54M;
            gopImg.Attributes[Gop.jarvis_goplimitout] = 400.34M;
            gopImg.Attributes["statecode"] = new OptionSetValue(3);
            gopImg.Attributes[Gop.GopApproval] = new OptionSetValue((int)GopApproval.Approved);
            gopImg.Attributes[Gop.PaymentType] = new OptionSetValue(34323000);
            gopImg.Attributes[Gop.jarvis_goplimitin] = 200.00M;
            gopImg.Attributes[Gop.jarvis_goplimitout] = 200.00M;
            gopImg.Attributes["statuscode"] = new OptionSetValue(334030001);
            gopImg.Attributes["jarvis_isdealercopied"] = false;
            gopImg.Attributes[Gop.contact] = "test";
            gopImg.Attributes[Gop.Comment] = "test";
            gopImg.Attributes[Gop.GopReason] = "test";
            gopImg.Attributes[Gop.Translationstatuscomment] = new OptionSetValue((int)TranslationStatus.NotStarted);
            gopImg.Attributes[Gop.Translationstatusgopreason] = new OptionSetValue((int)TranslationStatus.NotStarted);

            var gopPreImg = new Entity("jarvis_gop");
            gopPreImg.Attributes[Gop.Source] = new OptionSetValue((int)Source.Jarvis);
            gopPreImg.Attributes[Gop.Approved] = true;
            gopPreImg.Attributes[Gop.requestType] = new OptionSetValue((int)GopRequestType.GOP_RD);
            gopPreImg.Attributes[Casecontact.jarvisIncident] = new EntityReference("incident", Guid.NewGuid());
            gopPreImg.Attributes[Gop.jarvis_goplimitin] = 200.54M;
            gopPreImg.Attributes[Gop.jarvis_goplimitout] = 400.34M;
            gopPreImg.Attributes["statecode"] = new OptionSetValue(0);
            gopPreImg.Attributes[Gop.GopApproval] = new OptionSetValue((int)GopApproval.Approved);
            gopPreImg.Attributes[Gop.PaymentType] = new OptionSetValue(34323000);
            gopPreImg.Attributes[Gop.jarvis_goplimitin] = 200.00M;
            gopPreImg.Attributes[Gop.jarvis_goplimitout] = 200.00M;
            gopPreImg.Attributes["statuscode"] = new OptionSetValue(2);
            gopPreImg.Attributes["jarvis_isdealercopied"] = false;
            gopPreImg.Attributes[Gop.contact] = "test1";
            gopPreImg.Attributes[Gop.GopReason] = "test";
            gopPreImg.Attributes[Gop.Translationstatuscomment] = new OptionSetValue((int)TranslationStatus.InProgress);
            gopPreImg.Attributes[Gop.Translationstatusgopreason] = new OptionSetValue((int)TranslationStatus.InProgress);


            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
            {
                new KeyValuePair<string, object>("Target", gopImg),
            };
            this.PluginExecutionContext.MessageNameGet = () => "Update";
            this.PluginExecutionContext.StageGet = () => 40;

            // Setting post-entity image.
            this.PluginExecutionContext.PostEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                    { "PostImage", gopImg },
                    { "preImage", gopPreImg },
                    { "ComparePreImage", gopPreImg },
                    { "ComparePostImage", gopImg },
                };
            };

            this.PluginExecutionContext.PreEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                    { "PostImage", gopImg },
                    { "preImage", gopPreImg },
                    { "ComparePreImage", gopPreImg },
                    { "ComparePostImage", gopImg },
                };
            };

            // Setting service retrieve.
            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
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

            GopPostOperationAsync plugin = new GopPostOperationAsync("test", "test");

            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Positive scenario with user for GOP PostOperationAsync for Update Scenario.
        /// </summary>
        [TestMethod]
        public void GopPostOperationAsyncTestAsMercuriusUpdate13()
        {
            var gopImg = new Entity("jarvis_gop");
            gopImg.Attributes[Gop.Source] = new OptionSetValue((int)Source.Jarvis);
            gopImg.Attributes[Gop.Approved] = true;
            gopImg.Attributes[Gop.requestType] = new OptionSetValue((int)GopRequestType.GOP_RD);
            gopImg.Attributes[Casecontact.jarvisIncident] = new EntityReference("incident", Guid.NewGuid());
            gopImg.Attributes[Gop.jarvis_goplimitin] = 200.54M;
            gopImg.Attributes[Gop.jarvis_goplimitout] = 400.34M;
            gopImg.Attributes["statecode"] = new OptionSetValue(3);
            gopImg.Attributes[Gop.GopApproval] = new OptionSetValue((int)GopApproval.Approved);
            gopImg.Attributes[Gop.PaymentType] = new OptionSetValue((int)PaymentType.CreditCard);
            gopImg.Attributes[Gop.jarvis_goplimitin] = 200.00M;
            gopImg.Attributes[Gop.jarvis_goplimitout] = 200.00M;
            gopImg.Attributes["statuscode"] = new OptionSetValue(334030001);
            gopImg.Attributes["jarvis_isdealercopied"] = false;
            gopImg.Attributes[Gop.contact] = "test";
            gopImg.Attributes[Gop.Comment] = "test";
            gopImg.Attributes[Gop.GopReason] = "test";
            gopImg.Attributes[Gop.Translationstatuscomment] = new OptionSetValue((int)TranslationStatus.NotStarted);
            gopImg.Attributes[Gop.Translationstatusgopreason] = new OptionSetValue((int)TranslationStatus.NotStarted);
            gopImg.Attributes["jarvis_triggeredby"] = new EntityReference("systemuser", Guid.NewGuid());

            var gopPreImg = new Entity("jarvis_gop");
            gopPreImg.Attributes[Gop.Source] = new OptionSetValue((int)Source.Jarvis);
            gopPreImg.Attributes[Gop.Approved] = true;
            gopPreImg.Attributes[Gop.requestType] = new OptionSetValue((int)GopRequestType.GOP_RD);
            gopPreImg.Attributes[Casecontact.jarvisIncident] = new EntityReference("incident", Guid.NewGuid());
            gopPreImg.Attributes[Gop.jarvis_goplimitin] = 200.54M;
            gopPreImg.Attributes[Gop.jarvis_goplimitout] = 400.34M;
            gopPreImg.Attributes["statecode"] = new OptionSetValue(0);
            gopPreImg.Attributes[Gop.GopApproval] = new OptionSetValue((int)GopApproval.Approved);
            gopPreImg.Attributes[Gop.PaymentType] = new OptionSetValue((int)PaymentType.CreditCard);
            gopPreImg.Attributes[Gop.jarvis_goplimitin] = 200.00M;
            gopPreImg.Attributes[Gop.jarvis_goplimitout] = 200.00M;
            gopPreImg.Attributes["statuscode"] = new OptionSetValue(2);
            gopPreImg.Attributes["jarvis_isdealercopied"] = false;
            gopPreImg.Attributes[Gop.contact] = "test1";
            gopPreImg.Attributes[Gop.GopReason] = "test";
            gopPreImg.Attributes[Gop.Translationstatuscomment] = new OptionSetValue((int)TranslationStatus.InProgress);
            gopPreImg.Attributes[Gop.Translationstatusgopreason] = new OptionSetValue((int)TranslationStatus.InProgress);
            gopPreImg.Attributes["jarvis_triggeredby"] = new EntityReference("systemuser", Guid.NewGuid());


            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
            {
                new KeyValuePair<string, object>("Target", gopImg),
            };
            this.PluginExecutionContext.MessageNameGet = () => "Update";
            this.PluginExecutionContext.StageGet = () => 40;

            // Setting post-entity image.
            this.PluginExecutionContext.PostEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                    { "PostImage", gopImg },
                    { "preImage", gopPreImg },
                    { "ComparePreImage", gopPreImg },
                    { "ComparePostImage", gopImg },
                };
            };

            this.PluginExecutionContext.PreEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                    { "PostImage", gopImg },
                    { "preImage", gopPreImg },
                    { "ComparePreImage", gopPreImg },
                    { "ComparePostImage", gopImg },
                };
            };

            // Setting service retrieve.
            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
            {
                if (entityName == "systemuser")
                {
                    Entity user = new Entity("systemuser");
                    user.Id = Guid.NewGuid();
                    user["fullname"] = "Vivek";
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

            GopPostOperationAsync plugin = new GopPostOperationAsync("test", "test");

            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Positive scenario with user for GOP PostOperationAsync for Update Scenario of GOP+HD Triggered
        /// contact should has value and case breakdown should be true.
        /// </summary>
        [TestMethod]
        public void GopPostOperationAsyncTestAsMercuriusUpdateGOPHDTriggered()
        {
            var incident = new Entity("incident"); // Create a mock or actual instance of the "incident" Entity
            incident.Id = Guid.NewGuid(); // Set a valid GUID for the "incident" entity's ID
            incident[Incident.caseOriginCode] = new OptionSetValue(2);
            incident[Incident.casetypecode] = new OptionSetValue(2);

            var gopImg = new Entity("jarvis_gop");
            gopImg.Attributes[Gop.totalLimitIn] = 500.00M;
            gopImg.Attributes[Gop.totalLimitOut] = 1000.00M;
            gopImg.Attributes[Gop.Source] = new OptionSetValue((int)Source.Jarvis);
            gopImg.Attributes[Gop.Approved] = true;
            gopImg.Attributes[Gop.contact] = "1a679c95-33f0-40e1-ae35-bd2e72ff8ac3";
            gopImg.Attributes[Gop.requestType] = new OptionSetValue((int)GopRequestType.GOP_HD);
            gopImg.Attributes[Casecontact.jarvisIncident] = new EntityReference("incident", Guid.NewGuid());
            gopImg.Attributes[Gop.jarvis_goplimitin] = 200.54M;
            gopImg.Attributes[Gop.jarvis_goplimitout] = 400.34M;
            gopImg.Attributes["statecode"] = new OptionSetValue(0);
            gopImg.Attributes[Gop.GopApproval] = new OptionSetValue((int)GopApproval.Approved);
            gopImg.Attributes[Gop.PaymentType] = new OptionSetValue((int)PaymentType.CreditCard);
            gopImg.Attributes[Gop.jarvis_goplimitin] = 200.00M;
            gopImg.Attributes[Gop.jarvis_goplimitout] = 200.00M;
            gopImg.Attributes["statuscode"] = new OptionSetValue(2);
            gopImg.Attributes["jarvis_isdealercopied"] = false;
            gopImg.Attributes["jarvis_triggeredby"] = new EntityReference("systemuser", Guid.NewGuid());

            var gopPreImg = new Entity("jarvis_gop");
            gopPreImg.Attributes[Gop.totalLimitIn] = 500.00M;
            gopPreImg.Attributes[Gop.totalLimitOut] = 1000.00M;
            gopPreImg.Attributes[Gop.Source] = new OptionSetValue((int)Source.Jarvis);
            gopPreImg.Attributes[Gop.Approved] = true;
            gopPreImg.Attributes[Gop.requestType] = new OptionSetValue((int)GopRequestType.GOP_HD);
            gopPreImg.Attributes[Casecontact.jarvisIncident] = new EntityReference("incident", Guid.NewGuid());
            gopPreImg.Attributes[Gop.jarvis_goplimitin] = 200.54M;
            gopPreImg.Attributes[Gop.jarvis_goplimitout] = 400.34M;
            gopPreImg.Attributes["statecode"] = new OptionSetValue(1);
            gopPreImg.Attributes[Gop.GopApproval] = new OptionSetValue((int)GopApproval.Approved);
            gopPreImg.Attributes[Gop.PaymentType] = new OptionSetValue((int)PaymentType.CreditCard);
            gopPreImg.Attributes[Gop.jarvis_goplimitin] = 200.00M;
            gopPreImg.Attributes[Gop.jarvis_goplimitout] = 200.00M;
            gopPreImg.Attributes["statuscode"] = new OptionSetValue(2);
            gopPreImg.Attributes["jarvis_isdealercopied"] = false;
            gopPreImg.Attributes["jarvis_triggeredby"] = new EntityReference("systemuser", Guid.NewGuid());


            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
            {
                new KeyValuePair<string, object>("Target", gopImg),
            };
            this.PluginExecutionContext.MessageNameGet = () => "Update";
            this.PluginExecutionContext.StageGet = () => 40;

            // Setting post-entity image.
            this.PluginExecutionContext.PostEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                    { "PostImage", gopImg },
                    { "preImage", gopPreImg },
                    { "ComparePreImage", gopPreImg },
                    { "ComparePostImage", gopImg },
                };
            };

            this.PluginExecutionContext.PreEntityImagesGet = () =>
            {
                return new EntityImageCollection
                {
                    { "PostImage", gopImg },
                    { "preImage", gopPreImg },
                    { "ComparePreImage", gopPreImg },
                    { "ComparePostImage", gopImg },
                };
            };
            // Setting service retrieve.
            this.Service.RetrieveStringGuidColumnSet = (entityName, guid, secondaryUserColumnSet) =>
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

            GopPostOperationAsync plugin = new GopPostOperationAsync("test", "test");

            plugin.Execute(this.ServiceProvider);
        }

        /// <summary>
        /// Negative scenario where secure and unsecure strings are not provided.
        /// </summary>
        [TestMethod]

        // [ExpectedException(typeof(InvalidPluginExecutionException))]
        public void GopPostOperationAsyncTestNoSecureUnsecureStrings()
        {
            // Creating the plugin instance without providing secure and unsecure strings.
            Action createPlugin = () => new GopPostOperationAsync(null, null);

            Assert.ThrowsException<InvalidPluginExecutionException>(createPlugin);

            // The expected exception (InvalidPluginExecutionException) will be thrown, indicating the absence of secure and unsecure strings.
        }
    }
}
