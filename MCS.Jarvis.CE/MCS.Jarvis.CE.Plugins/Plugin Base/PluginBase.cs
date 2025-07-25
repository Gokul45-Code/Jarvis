//------------------------------------------------------------------------
// <Copyright file="PluginBase.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
//------------------------------------------------------------------------
namespace Plugins
{
    using System;
    using System.Globalization;
    using System.ServiceModel;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;

    /// <summary>
    /// Base class for all plug-in classes.
    /// </summary>
    public abstract class PluginBase : IPlugin
    {
        /// <summary>
        /// Gets the name of the child class.
        /// </summary>
        /// <value>The name of the child class.</value>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "PluginBase")]
        protected string ChildClassName { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginBase"/> class.
        /// </summary>
        /// <param name="childClassName">The derived class.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "PluginBase")]
        internal PluginBase(Type childClassName)
        {
            this.ChildClassName = childClassName.ToString();
        }

        /// <summary>
        /// Plug-in context object.
        /// </summary>
        public class LocalPluginContext
        {
            /// <summary>
            /// Gets service Provider.
            /// </summary>
            internal IServiceProvider ServiceProvider { get; private set; }

            /// <summary>
            /// Gets the Microsoft Dynamics 365 organization service.
            /// </summary>
            internal IOrganizationService OrganizationService { get; private set; }

            /// <summary>
            /// Gets the Microsoft Dynamics 365 organization service under System user.
            /// </summary>
            internal IOrganizationService AdminOrganizationService { get; private set; }

            /// <summary>
            /// Gets iPluginExecutionContext contains information that describes the run-time environment in which the plug-in executes, information related to the execution pipeline, and entity business information.
            /// </summary>
            internal IPluginExecutionContext PluginExecutionContext { get; private set; }

            /////// <summary>
            /////// Synchronous registered plug-ins can post the execution context to the Microsoft Azure Service Bus. <br/>
            /////// It is through this notification service that synchronous plug-ins can send brokered messages to the Microsoft Azure Service Bus.
            /////// </summary>
            ////internal IServiceEndpointNotificationService NotificationService { get; private set; }

            /// <summary>
            /// Gets provides logging run-time trace information for plug-ins.
            /// </summary>
            internal ITracingService TracingService { get; private set; }

            ///// <summary>
            ///// Gets provides Initiating User Language Code.
            ///// </summary>
            // internal int LanguageCode
            // {
            //    get
            //    {
            //        QueryExpression userSettingsQuery = new QueryExpression("usersettings");
            //        userSettingsQuery.ColumnSet.AddColumns("uilanguageid", "systemuserid");
            //        userSettingsQuery.Criteria.AddCondition("systemuserid", ConditionOperator.Equal, this.PluginExecutionContext.InitiatingUserId);
            //        EntityCollection userSettings = this.OrganizationService.RetrieveMultiple(userSettingsQuery);
            //        if (userSettings?.Entities != null && userSettings.Entities.Count > 0)
            //        {
            //            return (int)userSettings.Entities[0]["uilanguageid"];
            //        }
            //        return 1033;
            //    }
            // }

            /// <summary>
            /// Initializes a new instance of the <see cref="LocalPluginContext"/> class.
            /// Helper object that stores the services available in this plug-in.
            /// </summary>
            /// <param name="serviceProvider">service provider.</param>
            internal LocalPluginContext(IServiceProvider serviceProvider)
            {
                if (serviceProvider == null)
                {
                    throw new InvalidPluginExecutionException("service Provider");
                }

                // Obtain the execution context service from the service provider.
                this.PluginExecutionContext = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

                // Obtain the tracing service from the service provider.
                this.TracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

                ////// Get the notification service from the service provider.
                ////NotificationService = (IServiceEndpointNotificationService)serviceProvider.GetService(typeof(IServiceEndpointNotificationService));

                // Obtain the organization factory service from the service provider.
                IOrganizationServiceFactory factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));

                // Use the factory to generate the organization service.
                this.OrganizationService = factory.CreateOrganizationService(this.PluginExecutionContext.UserId);

                // Use the factory to generate the organization service under System user.
                this.AdminOrganizationService = factory.CreateOrganizationService(null);
            }

            /// <summary>
            /// Prevents a default instance of the <see cref="LocalPluginContext" /> class from being created.
            /// </summary>
            private LocalPluginContext()
            {
            }

            /// <summary>
            /// Writes a trace message to the CRM trace log.
            /// </summary>
            /// <param name="message">Message name to trace.</param>
            internal void Trace(string message)
            {
                if (string.IsNullOrWhiteSpace(message) || this.TracingService == null)
                {
                    return;
                }

                if (this.PluginExecutionContext == null)
                {
                    this.TracingService.Trace(message);
                }
                else
                {
                    this.TracingService.Trace(
                        "{0}, Correlation Id: {1}, Initiating User: {2}",
                        message,
                        this.PluginExecutionContext.CorrelationId,
                        this.PluginExecutionContext.InitiatingUserId);
                }
            }
        }

        /// <summary>
        /// Main entry point for he business logic that the plug-in is to execute.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <remarks>
        /// For improved performance, Microsoft Dynamics 365 caches plug-in instances.
        /// The plug-in's Execute method should be written to be stateless as the constructor
        /// is not called for every invocation of the plug-in. Also, multiple system threads
        /// could execute the plug-in at the same time. All per invocation state information
        /// is stored in the context. This means that you should not use global variables in plug-ins.
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "CrmVSSolution411.NewProj.PluginBase+LocalPluginContext.Trace(System.String)", Justification = "Execute")]
        public void Execute(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw new InvalidPluginExecutionException("service Provider");
            }

            // Construct the local plug-in context.
            LocalPluginContext localcontext = new LocalPluginContext(serviceProvider);

            localcontext.Trace(string.Format(CultureInfo.InvariantCulture, "Entered {0}.Execute()", this.ChildClassName));

            try
            {
                //// Invoke the custom implementation
                this.ExecuteCrmPlugin(localcontext);
                //// now exit - if the derived plug-in has incorrectly registered overlapping event registrations,
                //// guard against multiple executions.
                return;
            }
            catch (FaultException<OrganizationServiceFault> e)
            {
                localcontext.Trace(string.Format(CultureInfo.InvariantCulture, "Exception: {0}", e.ToString()));

                // Handle the exception.
                // throw new InvalidPluginExecutionException("Organization Service Fault", e);
            }
            finally
            {
                localcontext.Trace(string.Format(CultureInfo.InvariantCulture, "Exiting {0}.Execute()", this.ChildClassName));
            }
        }

        /// <summary>
        /// Placeholder for a custom plug-in implementation.
        /// </summary>
        /// <param name="localcontext">Context for the current plug-in.</param>
        public virtual void ExecuteCrmPlugin(LocalPluginContext localcontext)
        {
            // Do nothing.
        }
    }
}