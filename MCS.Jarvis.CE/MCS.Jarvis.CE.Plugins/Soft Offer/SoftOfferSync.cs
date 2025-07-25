// <Copyright file="SoftOffer.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace MCS.Jarvis.CE.Plugins.SoftOffer
{
    using System;
    using global::Plugins;
    using Microsoft.Crm.Sdk.Messages;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Messages;

    /// <summary>
    /// Soft Offer Sync.
    /// </summary>
    public class SoftOfferSync : PluginBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SoftOfferSync"/> class.
        /// SoftOfferSync:Constructor.
        /// </summary>
        public SoftOfferSync()
            : base(typeof(SoftOfferSync))
        {
        }

        /// <summary>
        /// Execute Plugin Logic for SoftOfferSync.
        /// </summary>
        /// <param name="localcontext">local Context.</param>
        /// <exception cref="InvalidPluginExecutionException">Plugin Exception.</exception>
        public override void ExecuteCrmPlugin(LocalPluginContext localcontext)
        {
            ITracingService tracingService =
                localcontext.TracingService;

            IPluginExecutionContext context = localcontext.PluginExecutionContext;

            IOrganizationService service = localcontext.OrganizationService;
            IOrganizationService adminservice = localcontext.AdminOrganizationService;
            try
            {
                // region Update
                DateTime startDate = new DateTime();
                DateTime expiryDate = new DateTime();
                int timeZoneCode = 105;

                if (context.Stage == 40 && context.MessageName.ToUpper() == "CREATE")
                {
                    if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                    {
                        Entity softoffer = (Entity)context.InputParameters["Target"];
                        tracingService.Trace("softoffer entity Id" + softoffer.Id);

                        if (softoffer.Attributes.Contains("jarvis_startdate") && softoffer.Attributes["jarvis_startdate"] != null)
                        {
                            var targetDateSource = (DateTime)softoffer.Attributes["jarvis_startdate"];
                            startDate = this.RetrieveLocalTimeFromUTCTime(service, targetDateSource, timeZoneCode);
                        }

                        if (softoffer.Attributes.Contains("jarvis_expirydate") && softoffer.Attributes["jarvis_expirydate"] != null)
                        {
                            var targetDateSource = (DateTime)softoffer.Attributes["jarvis_expirydate"];
                            expiryDate = this.RetrieveLocalTimeFromUTCTime(service, targetDateSource, timeZoneCode);
                        }

                        if (startDate != null && expiryDate != null)
                        {
                            this.DeactivateSoftOffer(softoffer, startDate, expiryDate, service, tracingService);
                            this.ActivateSoftOffer(softoffer, startDate, expiryDate, service, tracingService);
                        }
                    }
                }

                if (context.Stage == 20 && context.MessageName.ToUpper() == "UPDATE")
                {
                    if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                    {
                        Entity softoffer = (Entity)context.InputParameters["Target"];
                        tracingService.Trace("softoffer entity Id" + softoffer.Id);
                        Entity softOfferPreImg = context.PreEntityImages.Contains("PreImage") ? (Entity)context.PreEntityImages["PreImage"] : null;
                        if (softoffer.Attributes.Contains("jarvis_startdate") && softoffer.Attributes["jarvis_startdate"] != null)
                        {
                            var targetDateSource = (DateTime)softoffer.Attributes["jarvis_startdate"];
                            startDate = this.RetrieveLocalTimeFromUTCTime(service, targetDateSource, timeZoneCode);
                        }
                        else
                        {
                            if (softOfferPreImg != null && softOfferPreImg.Attributes.Contains("jarvis_startdate"))
                            {
                                var targetDateSource = (DateTime)softOfferPreImg.Attributes["jarvis_startdate"];
                                startDate = this.RetrieveLocalTimeFromUTCTime(service, targetDateSource, timeZoneCode);
                            }
                        }

                        if (softoffer.Attributes.Contains("jarvis_expirydate") && softoffer.Attributes["jarvis_expirydate"] != null)
                        {
                            var targetDateSource = (DateTime)softoffer.Attributes["jarvis_expirydate"];
                            expiryDate = this.RetrieveLocalTimeFromUTCTime(service, targetDateSource, timeZoneCode);
                        }
                        else
                        {
                            if (softOfferPreImg != null && softOfferPreImg.Attributes.Contains("jarvis_expirydate"))
                            {
                                var targetDateSource = (DateTime)softOfferPreImg.Attributes["jarvis_expirydate"];
                                expiryDate = this.RetrieveLocalTimeFromUTCTime(service, targetDateSource, timeZoneCode);
                            }
                        }

                        if (startDate != null && expiryDate != null)
                        {
                            this.DeactivateSoftOffer(softoffer, startDate, expiryDate, service, tracingService);
                            this.ActivateSoftOffer(softoffer, startDate, expiryDate, service, tracingService);
                        }
                    }
                }

                // endregion
            }
            catch (InvalidPluginExecutionException ex)
            {
                tracingService.Trace(ex.Message);
                tracingService.Trace(ex.StackTrace);
                throw new InvalidPluginExecutionException("Error in Soft offer Update" + ex.Message);
            }
        }

        /// <summary>
        /// Deactivating the soft offer.
        /// </summary>
        /// <param name="softoffer">soft offer.</param>
        /// <param name="startDate">start Date.</param>
        /// <param name="expiryDate">expiry Date.</param>
        /// <param name="service">Org service.</param>
        /// <param name="tracingService">tracing Service.</param>
        public void DeactivateSoftOffer(Entity softoffer, DateTime startDate, DateTime expiryDate, IOrganizationService service, ITracingService tracingService)
        {
            int timeZoneCode = 105;
            var targetDateSource = DateTime.Now.Date;
            DateTime todayDate = this.RetrieveLocalTimeFromUTCTime(service, targetDateSource, timeZoneCode);
            tracingService.Trace("today date" + todayDate.Date.ToString());
            if (!(todayDate.Date >= startDate.Date && todayDate.Date <= expiryDate.Date))
            {
                tracingService.Trace("start date" + startDate.Date.ToString());
                tracingService.Trace("expiry date" + expiryDate.Date.ToString());
                Entity softOfferDeactivate = new Entity(softoffer.LogicalName);
                softOfferDeactivate.Id = softoffer.Id;
                softOfferDeactivate["statecode"] = new OptionSetValue(1);
                softOfferDeactivate["statuscode"] = new OptionSetValue(2);
                //UpdateRequest deactivateSO = new UpdateRequest()
                //{
                //    Target = softOfferDeactivate,
                //};
                //service.Execute(deactivateSO);
                service.Update(softOfferDeactivate);
            }
        }

        /// <summary>
        /// Activate Soft Offer.
        /// </summary>
        /// <param name="softoffer">soft offer.</param>
        /// <param name="startDate">start date.</param>
        /// <param name="expiryDate">expiry date.</param>
        /// <param name="service">Org service.</param>
        /// <param name="tracingService">tracing service.</param>
        public void ActivateSoftOffer(Entity softoffer, DateTime startDate, DateTime expiryDate, IOrganizationService service, ITracingService tracingService)
        {
            int timeZoneCode = 105;
            var targetDateSource = DateTime.Now.Date;
            DateTime todayDate = this.RetrieveLocalTimeFromUTCTime(service, targetDateSource, timeZoneCode);
            tracingService.Trace("today date" + todayDate.Date.ToString());
            if (todayDate.Date >= startDate.Date && todayDate.Date <= expiryDate.Date)
            {
                tracingService.Trace("start date" + startDate.Date.ToString());
                tracingService.Trace("expiry date" + expiryDate.Date.ToString());
                Entity softOfferActivate = new Entity(softoffer.LogicalName);
                softOfferActivate.Id = softoffer.Id;
                softOfferActivate["statecode"] = new OptionSetValue(0);
                softOfferActivate["statuscode"] = new OptionSetValue(1);
                //UpdateRequest activateSO = new UpdateRequest()
                //{
                //    Target = softOfferActivate,
                //};
                //service.Execute(activateSO);
                service.Update(softOfferActivate);
            }
        }

        /// <summary>
        /// Retrieve Local Time From UTC Time.
        /// </summary>
        /// <param name="service">Org Service.</param>
        /// <param name="utcTime">UTC Time.</param>
        /// <param name="timeZoneCode">Time Zone Code.</param>
        /// <returns>Date Time.</returns>
        public DateTime RetrieveLocalTimeFromUTCTime(IOrganizationService service, DateTime utcTime, int? timeZoneCode)
        {
            if (!timeZoneCode.HasValue)
            {
                return utcTime;
            }

            var request = new LocalTimeFromUtcTimeRequest
            {
                TimeZoneCode = timeZoneCode.Value,
                UtcTime = utcTime.ToUniversalTime(),
            };

            var response = (LocalTimeFromUtcTimeResponse)service.Execute(request);

            return response.LocalTime;
        }
    }
}
