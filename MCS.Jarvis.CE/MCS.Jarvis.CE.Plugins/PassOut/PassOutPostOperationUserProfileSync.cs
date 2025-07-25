// <copyright file="PassOutPostOperationUserProfileSync.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MCS.Jarvis.CE.Plugins
{
    using System;
    using System.ServiceModel;
    using Microsoft.Xrm.Sdk;

    /// <summary>
    /// Pass Out Post Operation User Profile Sync.
    /// </summary>
    public class PassOutPostOperationUserProfileSync : IPlugin
    {
        /// <summary>
        /// Execute Method.
        /// </summary>
        /// <param name="serviceProvider">Service Provider.</param>
        /// <exception cref="InvalidPluginExecutionException">Invalid Plugin Execution Exception.</exception>
        public void Execute(IServiceProvider serviceProvider)
        {
            // Extract the tracing service for use in debugging sandboxed plug-ins.
            ITracingService tracingService =
                (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            // Obtain the execution context from the service provider.
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(null);

            // region Delete of Passout
            try
            {
                if (context.Stage == (int)Constants.PluginStage.PostOperation && context.MessageName.ToUpper() == Constants.PluginMessage.Delete)
                {
                    if (!context.InputParameters.Contains("Target") || !(context.InputParameters["Target"] is EntityReference) || !context.PreEntityImages.Contains("PreImage") || !(context.PreEntityImages["PreImage"] is Entity))
                    {
                        return;
                    }

                    EntityReference passOutDeleted = (EntityReference)context.InputParameters["Target"];

                    Entity passOutImage = context.PreEntityImages["PreImage"] as Entity;

                    EntityReference caseRef = passOutImage.Attributes.Contains(PassOut.Attributes.Case) ? (EntityReference)passOutImage.Attributes[PassOut.Attributes.Case] : null;

                    EntityReference businessPartnerRef = passOutImage.Attributes.Contains(PassOut.Attributes.BusinessPartner) ? (EntityReference)passOutImage.Attributes[PassOut.Attributes.BusinessPartner] : null;

                    CaseUserProfileOperation caseUserProfileOperation = new CaseUserProfileOperation(service, tracingService);

                    caseUserProfileOperation.RemoveCaseUserProfile(caseRef, businessPartnerRef);
                }
            }
            catch (InvalidOperationException oex)
            {
                tracingService.Trace(oex.Message);
                tracingService.Trace(oex.StackTrace);
            }
            catch (FaultException<OrganizationServiceFault> fex)
            {
                tracingService.Trace(fex.Message);
                tracingService.Trace(fex.StackTrace);
                throw fex;
            }
            catch (Exception ex)
            {
                tracingService.Trace(ex.Message);
                tracingService.Trace(ex.StackTrace);
                throw new InvalidPluginExecutionException("Error in syncing up Case User Profiles on creation of Case " + ex.Message);
            }

            // endregion
            // region Deactivation of Passout
            try
            {
                if (context.Stage == (int)Constants.PluginStage.PostOperation && (context.MessageName.ToUpper() == Constants.PluginMessage.Update))
                {
                    if (!context.InputParameters.Contains("Target") || !(context.InputParameters["Target"] is Entity) || !context.PreEntityImages.Contains("PreImage") || !(context.PreEntityImages["PreImage"] is Entity))
                    {
                        return;
                    }

                    Entity passOutObj = context.InputParameters["Target"] as Entity;

                    Entity passOutImage = context.PreEntityImages["PreImage"] as Entity;

                    if (passOutObj.Attributes.Contains(PassOut.Attributes.Status) && passOutObj.Attributes[PassOut.Attributes.Status] != null)
                    {
                        OptionSetValue status = (OptionSetValue)passOutObj.Attributes[PassOut.Attributes.Status];

                        if (status.Value != (int)PassOut.Status.InActive)
                        {
                            return;
                        }

                        EntityReference caseRef = passOutImage.Attributes.Contains(PassOut.Attributes.Case) ? (EntityReference)passOutImage.Attributes[PassOut.Attributes.Case] : null;

                        EntityReference businessPartnerRef = passOutImage.Attributes.Contains(PassOut.Attributes.BusinessPartner) ? (EntityReference)passOutImage.Attributes[PassOut.Attributes.BusinessPartner] : null;

                        CaseUserProfileOperation caseUserProfileOperation = new CaseUserProfileOperation(service, tracingService);

                        caseUserProfileOperation.RemoveCaseUserProfile(caseRef, businessPartnerRef);
                    }
                }
            }

            // catch (InvalidOperationException oex)
            // {
            //    tracingService.Trace(oex.Message);
            //    tracingService.Trace(oex.StackTrace);
            // }
            // catch (FaultException<OrganizationServiceFault> fex)
            // {
            //    tracingService.Trace(fex.Message);
            //    tracingService.Trace(fex.StackTrace);
            //    throw fex;
            // }
            // catch (Exception ex)
            // {
            //    tracingService.Trace(ex.Message);
            //    tracingService.Trace(ex.StackTrace);
            //    throw new InvalidPluginExecutionException("Error in syncing up Case User Profiles on creation of Case " + ex.Message);
            // }
            catch (InvalidPluginExecutionException ex)
            {
                throw new InvalidPluginExecutionException(ex.Message);
            }

            // endregion
        }
    }
}
