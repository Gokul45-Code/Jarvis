// <copyright file="AccountPostOperationSync.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace MCS.Jarvis.CE.Plugins
{
    using System;
    using System.Globalization;
    using global::Plugins;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;
    using static MCS.Jarvis.CE.BusinessProcessesShared.Helpers.Constants;

    /// <summary>
    /// Account PostOperation Update Sync.
    /// </summary>
    public class AccountPostOperationSync : PluginBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AccountPostOperationSync"/> class.
        /// AccountPostOperationUpdateSync Operation Constructor.
        /// </summary>
        /// <exception cref="InvalidPluginExecutionException">Invalid Plugin Execution Exception.</exception>
        public AccountPostOperationSync()
            : base(typeof(AccountPostOperationSync))
        {
        }

        /// <summary>
        /// Execute AccountPostOperationUpdateSync Post Operation Plugin.
        /// </summary>
        /// <param name="localcontext">local context.</param>
        public override void ExecuteCrmPlugin(LocalPluginContext localcontext)
        {
            ITracingService traceService = localcontext.TracingService;
            traceService.Trace("Start AccountPostOperationUpdateSync:" + DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));
            try
            {
                IPluginExecutionContext context = localcontext.PluginExecutionContext;
                IOrganizationService service = localcontext.AdminOrganizationService;

                if (context.InputParameters.Contains(General.Target) && context.InputParameters[General.Target] is Entity entity)
                {
                    if (context.Depth > 2)
                    {
                        return;
                    }

                    traceService.Trace("Entered into Account Post Operation Status Update/Create.");

                    if (context.PostEntityImages.Contains("PostImage") && context.PostEntityImages["PostImage"] != null)
                    {
                        traceService.Trace("Post Image present.");
                        bool activateBR = false;
                        Entity accountPostImg = context.PostEntityImages["PostImage"];
                        if (accountPostImg != null && accountPostImg.Attributes.Contains("jarvis_accounttype"))
                        {
                            OptionSetValue accountType = (OptionSetValue)accountPostImg.Attributes["jarvis_accounttype"];
                            ////If Customer return.
                            if (accountType != null && accountType.Value == (int)334030000)
                            {
                                return;
                            }
                        }

                        if (entity.Attributes.Contains(Accounts.VasExternalStatus) || entity.Attributes.Contains(Accounts.VasStatus))
                        {
                            traceService.Trace("Entered in On update of External Status and OneCase status");
                            if (accountPostImg.Attributes.Contains(Accounts.VasExternalStatus) && accountPostImg.Attributes.Contains(Accounts.VasStatus) && accountPostImg.Attributes.Contains(Accounts.ViewStatus))
                            {
                                OptionSetValue externalStatus = (OptionSetValue)accountPostImg.Attributes[Accounts.VasExternalStatus];
                                OptionSetValue oneCaseStatus = (OptionSetValue)accountPostImg.Attributes[Accounts.VasStatus];
                                OptionSetValue status = (OptionSetValue)accountPostImg.Attributes[Accounts.ViewStatus];
                                Entity accountUpdate = new Entity(entity.LogicalName, entity.Id);
                                traceService.Trace($"Status contains if check {externalStatus != null && externalStatus.Value == (int)VasStatus.Active && oneCaseStatus != null && oneCaseStatus.Value == (int)VasStatus.Active}");
                                traceService.Trace($"Status contains elseif check {externalStatus != null && oneCaseStatus != null && (externalStatus.Value == (int)VasStatus.InActive || oneCaseStatus.Value == (int)VasStatus.InActive)}");
                                if (externalStatus != null && externalStatus.Value == (int)VasStatus.Active && oneCaseStatus != null && oneCaseStatus.Value == (int)VasStatus.Active)
                                {
                                    if (status.Value != 0)
                                    {
                                        accountUpdate[Accounts.ViewStatus] = new OptionSetValue(0);
                                        accountUpdate[Accounts.ViewStatusReason] = new OptionSetValue(1);
                                        activateBR = true;
                                    }
                                }
                                else if (externalStatus != null && oneCaseStatus != null && (externalStatus.Value == (int)VasStatus.InActive || oneCaseStatus.Value == (int)VasStatus.InActive))
                                {
                                    if (status.Value != 1)
                                    {
                                        accountUpdate[Accounts.ViewStatus] = new OptionSetValue(1);
                                        accountUpdate[Accounts.ViewStatusReason] = new OptionSetValue(2);
                                    }
                                }

                                if (accountUpdate != null && accountUpdate.Attributes.Count > 0)
                                {
                                    traceService.Trace($"Bookable Resource activate {activateBR}");
                                    service.Update(accountUpdate);
                                    this.ActivateBookableResource(service, traceService, activateBR, accountPostImg);
                                }
                            }
                        }
                        else if (entity.Attributes.Contains(Accounts.ViewStatus))
                        {
                            traceService.Trace("Entered in On update of View Status");
                            if (accountPostImg != null && accountPostImg.Attributes.Contains(Accounts.VasExternalStatus) && accountPostImg.Attributes.Contains(Accounts.VasStatus))
                            {
                                Entity accountUpdate = new Entity(entity.LogicalName, entity.Id);
                                traceService.Trace("Post Image present.");
                                OptionSetValue status = (OptionSetValue)entity.Attributes[Accounts.ViewStatus];
                                OptionSetValue oneCaseStatus = (OptionSetValue)accountPostImg.Attributes[Accounts.VasStatus];
                                OptionSetValue externalStatus = (OptionSetValue)accountPostImg.Attributes[Accounts.VasExternalStatus];
                                if (status.Value == 0 && oneCaseStatus.Value != (int)VasStatus.Active)
                                {
                                    accountUpdate[Accounts.VasStatus] = new OptionSetValue((int)VasStatus.Active);
                                    activateBR = true;
                                }
                                else if (status.Value == 1 && oneCaseStatus.Value != (int)VasStatus.InActive)
                                {
                                    accountUpdate[Accounts.VasStatus] = new OptionSetValue((int)VasStatus.InActive);
                                }

                                if (accountUpdate != null && accountUpdate.Attributes.Count > 0)
                                {
                                    traceService.Trace($"Bookable Resource activate {activateBR}");
                                    service.Update(accountUpdate);
                                    this.ActivateBookableResource(service, traceService, activateBR, accountPostImg);
                                }
                            }
                        }
                    }
                }
            }
            catch (InvalidPluginExecutionException pex)
            {
                traceService.Trace(pex.Message);
                traceService.Trace(pex.StackTrace);
                throw new InvalidPluginExecutionException($"Error: {pex.Message}");
            }
        }

        /// <summary>
        /// Activate Bookable Resource.
        /// </summary>
        /// <param name="service">Org service.</param>
        /// <param name="tracingService">tracing service.</param>
        /// <param name="activate">activate param.</param>
        /// <param name="accountPostImg">account post image.</param>
        internal void ActivateBookableResource(IOrganizationService service, ITracingService tracingService, bool activate, Entity accountPostImg)
        {
            tracingService.Trace("Get into Bookable Resource");
            if (activate)
            {
                if (accountPostImg != null && accountPostImg.Attributes.Contains(Accounts.BookableResource) && accountPostImg.Attributes[Accounts.BookableResource] != null)
                {
                    tracingService.Trace("Image of Bookable Resource associated for the account.");
                    EntityReference bookableResourceRef = (EntityReference)accountPostImg.Attributes[Accounts.BookableResource];
                    Entity bookableResource = service.Retrieve("bookableresource", bookableResourceRef.Id, new ColumnSet("statecode"));
                    if (bookableResource != null && bookableResource.Attributes.Contains("statecode"))
                    {
                        tracingService.Trace("Bookable Resource retrieved");
                        OptionSetValue brstateCode = (OptionSetValue)bookableResource.Attributes["statecode"];
                        if (brstateCode != null && brstateCode.Value != 0)
                        {
                            tracingService.Trace("Check the bookable resoruce condition");
                            Entity bookResoruceUpdate = new Entity(bookableResource.LogicalName, bookableResourceRef.Id);
                            bookResoruceUpdate[Accounts.ViewStatus] = new OptionSetValue(0);
                            bookResoruceUpdate[Accounts.ViewStatusReason] = new OptionSetValue(1);
                            service.Update(bookResoruceUpdate);
                            tracingService.Trace("Bookable resource activated");
                        }
                    }
                }
            }
        }
    }
}