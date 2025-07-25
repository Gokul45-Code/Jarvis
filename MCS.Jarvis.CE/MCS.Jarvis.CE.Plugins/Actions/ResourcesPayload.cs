// <copyright file="ResourcesPayload.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace MCS.Jarvis.CE.Plugins
{
    using System;
    using System.Web.UI;
    using Newtonsoft.Json;

    // Object to construct output to App

    /// <summary>
    /// Resources Pay load.
    /// </summary>
    public class ResourcesPayload
    {
        /// <summary>
        /// Gets or sets name.
        /// </summary>
        [JsonProperty("Name")]
        public string name { get; set; }

        /// <summary>
        /// Gets or sets resourceID.
        /// </summary>
        [JsonProperty("Id")]
        public string resourceID { get; set; }

        /// <summary>
        /// Gets or sets startTime.
        /// </summary>
        [JsonProperty("Start Time")]
        public string StartTime { get; set; }

        /// <summary>
        /// Gets or sets endTime.
        /// </summary>
        [JsonProperty("End Time")]
        public string EndTime { get; set; }

        /// <summary>
        /// Gets or sets distance.
        /// </summary>
        [JsonProperty("Distance")]
        public string Distance { get; set; }

        /// <summary>
        /// Gets or sets travelTime.
        /// </summary>
        [JsonProperty("Travel Time")]
        public string TravelTime { get; set; }

        /// <summary>
        /// Gets or sets travelStartTime.
        /// </summary>
        [JsonProperty("Travel Start Time")]
        public string TravelStartTime { get; set; }

        /// <summary>
        /// Gets or sets accountID.
        /// </summary>
        [JsonProperty("AccountID")]
        public string AccountID { get; set; }

        /// <summary>
        /// Gets or sets temporaryRepairInfo.
        /// </summary>
        [JsonProperty("Temporary Dealer Information")]
        public string TemporaryRepairInfo { get; set; }

        /// <summary>
        /// Gets or sets dealerType.
        /// </summary>
        [JsonProperty("Dealer Type")]
        public string DealerType { get; set; }

        /// <summary>
        /// Gets or sets service.
        /// </summary>
        [JsonProperty("Service")]
        public string Service { get; set; }

        /// <summary>
        /// Gets or sets tDI.
        /// </summary>
        [JsonProperty("TDI")]
        public string TDI { get; set; }

        /// <summary>
        /// Gets or sets validFrom.
        /// </summary>
        [JsonProperty("Valid From")]
        public string ValidFrom { get; set; }

        /// <summary>
        /// Gets or sets validTo.
        /// </summary>
        [JsonProperty("Valid To")]
        public string ValidTo { get; set; }

        /// <summary>
        /// Gets or sets country.
        /// </summary>
        [JsonProperty("Country")]
        public string Country { get; set; }

        /// <summary>
        /// Gets or sets city.
        /// </summary>
        [JsonProperty("City")]
        public string City { get; set; }

        /// <summary>
        /// Gets or sets TimeZone.
        /// </summary>
        [JsonProperty("TimeZone")]
        public string TimeZone { get; set; }

        /// <summary>
        /// Gets or sets Responsible Unit ID.
        /// </summary>
        [JsonProperty("ResponsibleUnitID")]
        public string ResponsibleUnitID { get; set; }
    }

    /// <summary>
    /// Input Pay Load.
    /// </summary>
    public class InputPayLoad
    {
        /// <summary>
        /// Gets or sets resourceType.
        /// </summary>
        public int ResourceType { get; set; }

        /// <summary>
        /// Gets or sets duration.
        /// </summary>
        public int Duration { get; set; }

        /// <summary>
        /// Gets or sets remainingDuration.
        /// </summary>
        public int RemainingDuration { get; set; }

        /// <summary>
        /// Gets or sets work location.
        /// </summary>
        public int Worklocation { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether isPrimary.
        /// </summary>
        public bool IsPrimary { get; set; }

        /// <summary>
        /// Gets or sets effort.
        /// </summary>
        public int Effort { get; set; }

        /// <summary>
        /// Gets or sets timeTo.
        /// </summary>
        public int TimeTo { get; set; }

        /// <summary>
        /// Gets or sets timeToPromised.
        /// </summary>
        public int TimeToPromised { get; set; }

        /// <summary>
        /// Gets or sets version.
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Gets or sets travelUnit.
        /// </summary>
        public int TravelUnit { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether useRealTimeResourceLocation.
        /// </summary>
        public bool UseRealTimeResourceLocation { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether considerTravelTime.
        /// </summary>
        public bool ConsiderTravelTime { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether considerSlotsWithOverlappingBooking.
        /// </summary>
        public bool ConsiderSlotsWithOverlappingBooking { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether considerSlotsWithLessThanRequiredDuration.
        /// </summary>
        public bool ConsiderSlotsWithLessThanRequiredDuration { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether considerSlotsWithProposedBookings.
        /// </summary>
        public bool ConsiderSlotsWithProposedBookings { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether considerAppointments.
        /// </summary>
        public bool ConsiderAppointments { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether movePastStartDateToCurrentDate.
        /// </summary>
        public bool MovePastStartDateToCurrentDate { get; set; }
    }

    /// <summary>
    /// Communication Pay Load.
    /// </summary>
    public class CommunicationPayLoad
    {
        /// <summary>
        /// Gets or sets emailAddress.
        /// </summary>
        public string emailAddress { get; set; }

        /// <summary>
        /// Gets or sets language.
        /// </summary>
        public string language { get; set; }

        /// <summary>
        /// Gets or sets contact id.
        /// </summary>
        public string contactid { get; set; }
    }

    /// <summary>
    /// GOP Communication.
    /// </summary>
    public class GOPCommunication
    {
        /// <summary>
        /// Gets or sets incident jarvis source id.
        /// </summary>
        [JsonProperty("incident.jarvis_sourceid")]
        public string incidentjarvis_sourceid { get; set; }

        /// <summary>
        /// Gets or sets jarvis contact.
        /// </summary>
        [JsonProperty("jarvis_gop.jarvis_contact")]
        public DateTime jarvis_gopjarvis_contact { get; set; }

        /// <summary>
        /// Gets or sets jarvis dealer name.
        /// </summary>
        [JsonProperty("jarvis_gop.jarvis_dealer.name")]
        public string jarvis_gopjarvis_dealername { get; set; }

        /// <summary>
        /// Gets or sets jarvis dealer address1 line1.
        /// </summary>
        [JsonProperty("jarvis_gop.jarvis_dealer.address1_line1")]
        public string jarvis_gopjarvis_dealeraddress1_line1 { get; set; }

        /// <summary>
        /// Gets or sets jarvis dealer address1 postal code.
        /// </summary>
        [JsonProperty("jarvis_gop.jarvis_dealer.address1_postalcode")]
        public string jarvis_gopjarvis_dealeraddress1_postalcode { get; set; }

        /// <summary>
        /// Gets or sets jarvis dealer address1 city.
        /// </summary>
        [JsonProperty("jarvis_gop.jarvis_dealer.address1_city")]
        public string jarvis_gopjarvis_dealeraddress1_city { get; set; }

        /// <summary>
        /// Gets or sets jarvis dealer telephone1.
        /// </summary>
        [JsonProperty("jarvis_gop.jarvis_dealer.telephone1")]
        public string jarvis_gopjarvis_dealertelephone1 { get; set; }

        /// <summary>
        /// Gets or sets jarvis dealer jarvis vat id.
        /// </summary>
        [JsonProperty("jarvis_gop.jarvis_dealer.jarvis_vatid")]
        public string jarvis_gopjarvis_dealerjarvis_vatid { get; set; }

        /// <summary>
        /// Gets or sets jarvis dealer jarvis address1 country.
        /// </summary>
        [JsonProperty("jarvis_gop.jarvis_dealer.jarvis_address1_country")]
        public string jarvis_gopjarvis_dealerjarvis_address1_country { get; set; }

        /// <summary>
        /// Gets or sets jarvis case jarvis vehicle jarvis model type.
        /// </summary>
        [JsonProperty("jarvis_gop.jarvis_case.jarvis_vehicle.jarvis_modeltype ")]
        public string jarvis_gopjarvis_casejarvis_vehiclejarvis_modeltype { get; set; }

        /// <summary>
        /// Gets or sets jarvis case jarvis vehicle jarvis chassis number new.
        /// </summary>
        [JsonProperty("jarvis_gop.jarvis_case.jarvis_vehicle.jarvis_chassisnumbernew ")]
        public string jarvis_gopjarvis_casejarvis_vehiclejarvis_chassisnumbernew { get; set; }

        /// <summary>
        /// Gets or sets jarvis case jarvis vehicle jarvis vin.
        /// </summary>
        [JsonProperty("jarvis_gop.jarvis_case.jarvis_vehicle.jarvis_vin")]
        public string jarvis_gopjarvis_casejarvis_vehiclejarvis_vin { get; set; }

        /// <summary>
        /// Gets or sets jarvis case jarvis vehicle jarvis updated registration number.
        /// </summary>
        [JsonProperty("jarvis_gop.jarvis_case.jarvis_vehicle.jarvis_updatedregistrationnumber")]
        public string jarvis_gopjarvis_casejarvis_vehiclejarvis_updatedregistrationnumber { get; set; }

        /// <summary>
        /// Gets or sets jarvis case jarvis registration trailer.
        /// </summary>
        [JsonProperty("jarvis_gop.jarvis_case. jarvis_regtrailer")]
        public string jarvis_gopjarvis_casejarvis_regtrailer { get; set; }

        /// <summary>
        /// Gets or sets jarvis case jarvis vehicle jarvis after market model.
        /// </summary>
        [JsonProperty("jarvis_gop.jarvis_case.jarvis_vehicle.jarvis_aftermarketmodel")]
        public string jarvis_gopjarvis_casejarvis_vehiclejarvis_aftermarketmodel { get; set; }

        /// <summary>
        /// Gets or sets jarvis case jarvis vehicle jarvis delivery date.
        /// </summary>
        [JsonProperty("jarvis_gop.jarvis_case.jarvis_vehicle.jarvis_deliverydate")]
        public string jarvis_gopjarvis_casejarvis_vehiclejarvis_deliverydate { get; set; }

        /// <summary>
        /// Gets or sets jarvis case jarvis mileage.
        /// </summary>
        [JsonProperty("jarvis_gop.jarvis_case.jarvis_mileage ")]
        public string jarvis_gopjarvis_casejarvis_mileage { get; set; }

        /// <summary>
        /// Gets or sets jarvis case jarvis mileage unit.
        /// </summary>
        [JsonProperty("jarvis_gop.jarvis_case.jarvis_mileageunit ")]
        public string jarvis_gopjarvis_casejarvis_mileageunit { get; set; }

        /// <summary>
        /// Gets or sets jarvis case created on.
        /// </summary>
        [JsonProperty("jarvis_gop.jarvis_case.createdon")]
        public string jarvis_gopjarvis_casecreatedon { get; set; }

        /// <summary>
        /// Gets or sets jarvis case jarvis Caller Name ARGUS.
        /// </summary>
        [JsonProperty("jarvis_gop.jarvis_case.jarvis_CallerNameARGUS")]
        public string jarvis_gopjarvis_casejarvis_CallerNameARGUS { get; set; }

        /// <summary>
        /// Gets or sets jarvis case jarvis caller phone.
        /// </summary>
        [JsonProperty("jarvis_gop.jarvis_case.jarvis_callerphone")]
        public string jarvis_gopjarvis_casejarvis_callerphone { get; set; }

        /// <summary>
        /// Gets or sets jarvis case jarvis driver name.
        /// </summary>
        [JsonProperty("jarvis_gop.jarvis_case.jarvis_drivername")]
        public string jarvis_gopjarvis_casejarvis_drivername { get; set; }

        /// <summary>
        /// Gets or sets jarvis case jarvis Driver Phone.
        /// </summary>
        [JsonProperty("jarvis_gop.jarvis_case.jarvis_DriverPhone")]
        public string jarvis_gopjarvis_casejarvis_DriverPhone { get; set; }

        /// <summary>
        /// Gets or sets jarvis case jarvis case translation jarvis location.
        /// </summary>
        [JsonProperty("jarvis_gop.jarvis_case.jarvis_casetranslation.jarvis_location")]
        public string jarvis_gopjarvis_casejarvis_casetranslationjarvis_location { get; set; }

        /// <summary>
        /// Gets or sets jarvis case jarvis case translation jarvis reported fault.
        /// </summary>
        [JsonProperty("jarvis_gop.jarvis_case.jarvis_casetranslation.jarvis_reportedfault")]
        public string jarvis_gopjarvis_casejarvis_casetranslationjarvis_reportedfault { get; set; }

        /// <summary>
        /// Gets or sets jarvis total limit in.
        /// </summary>
        [JsonProperty("jarvis_gop.jarvis_totallimitin")]
        public string jarvis_gopjarvis_totallimitin { get; set; }

        /// <summary>
        /// Gets or sets jarvis total limit in currency.
        /// </summary>
        [JsonProperty("jarvis_gop.jarvis_totallimitincurrency")]
        public string jarvis_gopjarvis_totallimitincurrency { get; set; }

        /// <summary>
        /// Gets or sets jarvis payment type.
        /// </summary>
        [JsonProperty("jarvis_gop.jarvis_paymenttype")]
        public string jarvis_gopjarvis_paymenttype { get; set; }

        /// <summary>
        /// Gets or sets modified on.
        /// </summary>
        [JsonProperty("jarvis_gop.modifiedon")]
        public string jarvis_gopmodifiedon { get; set; }
    }
}
