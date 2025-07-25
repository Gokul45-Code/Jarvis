// <Copyright file="NotesPostCreateSync.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace MCS.Jarvis.CE.Plugins.Notes
{
    using System;
    using System.Globalization;
    using global::Plugins;
    using MCS.Jarvis.CE.BusinessProcessShared.CaseMonitor;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;

    /// <summary>
    /// Notes Post Create Sync.
    /// </summary>
    public class NotesPostCreateSync : PluginBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotesPostCreateSync"/> class.
        /// Notes Post Create Sync.
        /// </summary>
        public NotesPostCreateSync()
            : base(typeof(NotesPostCreateSync))
        {
        }

        /// <summary>
        /// Execute CRM Plugin.
        /// </summary>
        /// <param name="localcontext">local context.</param>
        /// <exception cref="Exception">Exception details.</exception>
        public override void ExecuteCrmPlugin(LocalPluginContext localcontext)
        {
            // Extract the tracing service for use in debugging sandboxed plug-ins.
            ITracingService tracingService =
                localcontext.TracingService;

            // Obtain the execution context from the service provider.
            IPluginExecutionContext context = localcontext.PluginExecutionContext;

            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                try
                {
                    Entity annotation = (Entity)context.InputParameters["Target"];
                    Entity annotationPostImg = (Entity)context.PostEntityImages["PostImage"];
                    IOrganizationService service = localcontext.OrganizationService;

                    if (annotationPostImg.Attributes.Contains("createdby") && annotationPostImg.Attributes["createdby"] != null)
                    {
                        EntityReference createdBy = (EntityReference)annotationPostImg.Attributes["createdby"];
                        if (createdBy.Name.ToUpperInvariant().Contains("MERCURIUS"))
                        {
                            if (annotationPostImg.Attributes.Contains("filename") && annotationPostImg.Attributes["filename"] != null)
                            {
                                if (annotationPostImg.Attributes.Contains("objectid") && annotationPostImg.Attributes["objectid"] != null)
                                {
                                    EntityReference incident = (EntityReference)annotationPostImg.Attributes["objectid"];
                                    Entity incidentRetrieve = service.Retrieve(incident.LogicalName, incident.Id, new ColumnSet("jarvis_callerlanguage", "jarvis_callerrole", "jarvis_homedealer"));
                                    EntityCollection getlatestPassOut = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getLastModifiedPassOutForCase, incident.Id)));

                                    if (getlatestPassOut.Entities.Count > 0)
                                    {
                                        if (getlatestPassOut.Entities[0].Attributes.Contains("jarvis_repairingdealer") && getlatestPassOut.Entities[0].Attributes["jarvis_repairingdealer"] != null)
                                        {
                                            EntityReference repairDealer = (EntityReference)getlatestPassOut.Entities[0].Attributes["jarvis_repairingdealer"];

                                            // region Monitor Creation
                                            CaseMonitorProcess operations = new CaseMonitorProcess();
                                            string isCountryCode = string.Empty;
                                            string isoLangCode = string.Empty;

                                            Entity account = service.Retrieve(repairDealer.LogicalName, repairDealer.Id, new ColumnSet("jarvis_address1_country", "jarvis_language"));
                                            if (account.Attributes.Contains("jarvis_address1_country") && account.Attributes["jarvis_address1_country"] != null)
                                            {
                                                EntityReference hdcountry = (EntityReference)account.Attributes["jarvis_address1_country"];
                                                Entity country = service.Retrieve(hdcountry.LogicalName, hdcountry.Id, new ColumnSet("jarvis_iso2countrycode"));
                                                if (country.Attributes.Contains("jarvis_iso2countrycode") && country.Attributes["jarvis_iso2countrycode"] != null)
                                                {
                                                    isCountryCode = (string)country.Attributes["jarvis_iso2countrycode"];
                                                }
                                            }

                                            if (account.Attributes.Contains("jarvis_language") && account.Attributes["jarvis_language"] != null)
                                            {
                                                EntityReference language = (EntityReference)account.Attributes["jarvis_language"];
                                                Entity hdlanguage = service.Retrieve(language.LogicalName, language.Id, new ColumnSet("jarvis_iso3languagecode6392t"));
                                                if (hdlanguage.Attributes.Contains("jarvis_iso3languagecode6392t") && hdlanguage.Attributes["jarvis_iso3languagecode6392t"] != null)
                                                {
                                                    isoLangCode = (string)hdlanguage.Attributes["jarvis_iso3languagecode6392t"];
                                                }
                                            }

                                            if (!string.IsNullOrEmpty(isoLangCode) || !string.IsNullOrEmpty(isCountryCode))
                                            {
                                                string fucomment = isoLangCode + " " + isCountryCode + " " + "Attachment Avail";
                                                operations.AutomateMonitorCreation(incidentRetrieve, fucomment, 1, 14, 0, string.Empty, service);
                                            }

                                            // endregion
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (InvalidPluginExecutionException ex)
                {
                    tracingService.Trace(ex.Message);
                    tracingService.Trace(ex.StackTrace);
                    throw new InvalidPluginExecutionException("Error in NotesPostOperationSync " + ex.Message);
                }
                finally
                {
                    tracingService.Trace("End NotesPostOperationSync:" + DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));

                    // No Error Message Throwing Implictly to avoid blocking user because of integration.
                }
            }
        }
    }
}
