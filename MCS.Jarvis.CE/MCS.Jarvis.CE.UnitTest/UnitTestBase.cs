// -----------------------------------------------------------------------
// <copyright file="UnitTestBase.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace MCS.Jarvis.CE.UnitTest
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using System.Fakes;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Fakes;
    using Microsoft.Xrm.Sdk.Workflow;
    using Microsoft.Xrm.Sdk.Workflow.Fakes;
    using Newtonsoft.Json.Fakes;
    using Newtonsoft.Json.Linq.Fakes;

    /// <summary>
    /// Unit Test Base Logic Class.
    /// </summary>
    [TestClass]
    public class UnitTestBase
    {
#pragma warning restore CS3003 // Type is not CLS-compliant
        /// <summary>
        /// Gets or sets Stub IOrganization Service.
        /// </summary>
#pragma warning disable CS3003 // Type is not CLS-compliant
        public StubIOrganizationService Service { get; set; }
#pragma warning restore CS3003 // Type is not CLS-compliant
        /// <summary>
        /// Gets or sets Stub IPluginExecution Context.
        /// </summary>
#pragma warning disable CS3003 // Type is not CLS-compliant
        public StubIPluginExecutionContext PluginExecutionContext { get; set; }
#pragma warning restore CS3003 // Type is not CLS-compliant
        /// <summary>
        /// Gets or sets Stub IOrganizationService Factory.
        /// </summary>
#pragma warning disable CS3003 // Type is not CLS-compliant
        public StubIOrganizationServiceFactory Factory { get; set; }
#pragma warning restore CS3003 // Type is not CLS-compliant
        /// <summary>
        /// Gets or sets Stub IService sProvider.
        /// </summary>
#pragma warning disable CS3003 // Type is not CLS-compliant
        public StubIServiceProvider ServiceProvider { get; set; }
#pragma warning restore CS3003 // Type is not CLS-compliant
        /// <summary>
        /// Gets or sets Stub ITracing Service.
        /// </summary>
#pragma warning disable CS3003 // Type is not CLS-compliant
        public StubITracingService TracingService { get; set; }
#pragma warning restore CS3003 // Type is not CLS-compliant
        /// <summary>
        /// Gets or sets Stub IWorkflow Context.
        /// </summary>
#pragma warning disable CS3003 // Type is not CLS-compliant
        public StubIWorkflowContext WorkflowContext { get; set; }
#pragma warning restore CS3003 // Type is not CLS-compliant
        /// <summary>
        /// Gets or sets Stub IService Endpoint Notification Service.
        /// </summary>
#pragma warning disable CS3003 // Type is not CLS-compliant
        public StubIServiceEndpointNotificationService ServiceEndpointNotificationService { get; set; }
#pragma warning restore CS3003 // Type is not CLS-compliant

        /// <summary>
        /// Initialize test.
        /// </summary>
        [TestInitialize]
        public void Init()
        {
            //// this.ServiceClient = new ShimCrmServiceClient();

            this.Service = new StubIOrganizationService();

            this.PluginExecutionContext = new StubIPluginExecutionContext();

            this.WorkflowContext = new StubIWorkflowContext();

            this.Factory = new StubIOrganizationServiceFactory
            {
                CreateOrganizationServiceNullableOfGuid = id => this.Service,
            };


            this.TracingService = new StubITracingService();

            this.ServiceEndpointNotificationService = new StubIServiceEndpointNotificationService();

            this.ServiceProvider = new StubIServiceProvider
            {
                GetServiceType = t =>
                {
                    if (t == typeof(IWorkflowContext))
                    {
                        return this.WorkflowContext;
                    }

                    if (t == typeof(IPluginExecutionContext))
                    {
                        return this.PluginExecutionContext;
                    }

                    if (t == typeof(ITracingService))
                    {
                        return this.TracingService;
                    }

                    if (t == typeof(IOrganizationServiceFactory))
                    {
                        return this.Factory;
                    }

                    if (t == typeof(IServiceEndpointNotificationService))
                    {
                        return this.ServiceEndpointNotificationService;
                    }

                    return null;
                },
            };

            this.WorkflowContext.UserIdGet = () =>
            {
                return Guid.NewGuid();
            };
            this.WorkflowContext.CorrelationIdGet = () =>
            {
                return Guid.NewGuid();
            };
            this.WorkflowContext.InitiatingUserIdGet = () =>
            {
                return Guid.NewGuid();
            };
        }

        /// <summary>
        /// Test Cleanup.
        /// </summary>
        [TestCleanup]
        public void Cleanup()
        {
            this.Service = null;

            this.PluginExecutionContext = null;

            this.Factory = null;

            this.ServiceProvider = null;

            this.WorkflowContext = null;
        }

        /// <summary>
        /// Executes the workflow.
        /// </summary>
        /// <param name="workflow">The workflow.</param>
        /// <param name="inputs">The inputs.</param>
        /// <returns>Execution result.</returns>
        public IDictionary<string, object> ExecuteWorkflow(Activity workflow, IDictionary<string, object> inputs)
        {
            var invoker = new WorkflowInvoker(workflow);
            invoker.Extensions.Add<ITracingService>(() => this.TracingService);
            invoker.Extensions.Add<IWorkflowContext>(() => this.WorkflowContext);
            invoker.Extensions.Add<IOrganizationServiceFactory>(() => this.Factory);

            if (inputs != null)
            {
                return invoker.Invoke(inputs);
            }
            else
            {
                return invoker.Invoke();
            }
        }

        /// <summary>
        /// Executes the workflow.
        /// </summary>
        /// <param name="workflow">The workflow.</param>
        /// <returns>Execution result.</returns>
        public IDictionary<string, object> ExecuteWorkflow(Activity workflow)
        {
            return this.ExecuteWorkflow(workflow, null);
        }
    }
}
