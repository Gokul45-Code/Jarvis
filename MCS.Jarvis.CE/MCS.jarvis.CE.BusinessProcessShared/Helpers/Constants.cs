// <copyright file="Constants.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace MCS.Jarvis.CE.BusinessProcessesShared.Helpers
{
    using System;
    using System.CodeDom;
    using System.IdentityModel.Metadata;
    using System.Runtime.InteropServices;
    using System.Text.RegularExpressions;
    using Microsoft.Xrm.Sdk;

    /// <summary>
    /// Constants class.
    /// </summary>
    public class Constants
    {
        /// <summary>
        /// General class.
        /// </summary>
        public class General
        {
            /// <summary>
            /// Target attribute.
            /// </summary>
            public const string Target = "Target";

            /// <summary>
            /// modified by attribute.
            /// </summary>
            public const string ModifiedBy = "modifiedby";

            /// <summary>
            /// Pre Image attribute.
            /// </summary>
            public const string PreImage = "PreImage";

            /// <summary>
            /// Post Image attribute.
            /// </summary>
            public const string PostImage = "PostImage";

            /// <summary>
            /// JarvisCaseEscalation attribute.
            /// </summary>
            public const string JarvisCaseEscalation = "jarvis_caseescalation";
        }

        /// <summary>
        /// Incident Entity.
        /// </summary>
        public class Incident
        {
            /// <summary>
            /// incident attribute.
            /// </summary>
            public const string logicalName = "incident";

            /// <summary>
            /// case origin code attribute.
            /// </summary>
            public const string caseOriginCode = "caseorigincode";

            /// <summary>
            /// VASBreakDownConfiguration attribute.
            /// </summary>
            public const string bpfStageName = "VASBreakDownConfiguration";

            /// <summary>
            /// VASSearchAPI attribute.
            /// </summary>
            public const string searchAPI = "VASSearchAPI";

            /// <summary>
            /// Case Opening attribute.
            /// </summary>
            public const string bpfStage1 = "Case Opening";

            /// <summary>
            /// Guarantee Of Payment attribute.
            /// </summary>
            public const string bpfStage2 = "Guarantee Of Payment";

            /// <summary>
            /// Pass Out attribute.
            /// </summary>
            public const string bpfStage3 = "Pass Out";

            /// <summary>
            /// ETA Technician attribute.
            /// </summary>
            public const string bpfStage4 = "ETA Technician";

            /// <summary>
            /// Waiting For Repair Start attribute.
            /// </summary>
            public const string bpfStage5 = "Waiting For Repair Start";

            /// <summary>
            /// Repair Ongoing attribute.
            /// </summary>
            public const string bpfStage6 = "Repair Ongoing";

            /// <summary>
            /// Repair Finished attribute.
            /// </summary>
            public const string bpfStage7 = "Repair Finished";

            /// <summary>
            /// Repair Summary attribute.
            /// </summary>
            public const string bpfStage8 = "Repair Summary";

            /// <summary>
            /// Case Closure attribute.
            /// </summary>
            public const string bpfStage9 = "Case Closure";

            /// <summary>
            /// Ongoing attribute.
            /// </summary>
            public const string bpfStage10 = "Ongoing";

            /// <summary>
            /// Solved attribute.
            /// </summary>
            public const string bpfStage11 = "Solved";

            /// <summary>
            /// Credit To HD attribute.
            /// </summary>
            public const string bpfStage12 = "Credit To HD";

            /// <summary>
            /// Closed attribute.
            /// </summary>
            public const string bpfStage13 = "Closed";

            /// <summary>
            /// case type code attribute.
            /// </summary>
            public const string casetypecode = "casetypecode";

            /// <summary>
            /// status code attribute.
            /// </summary>
            public const string caseStatus = "statuscode";

            /// <summary>
            /// Home Dealer attribute.
            /// </summary>
            public const string HomeDealer = "jarvis_homedealer";

            /// <summary>
            /// Jarvis Assistance type attribute.
            /// </summary>
            public const string JarvisAssistancetype = "jarvis_assistancetype";

            /// <summary>
            /// Jarvis Country attribute.
            /// </summary>
            public const string JarvisCountry = "jarvis_country";

            /// <summary>
            /// Created On attribute.
            /// </summary>
            public const string CreatedOn = "createdon";

            /// <summary>
            /// Jarvis average eta attribute.
            /// </summary>
            public const string JarvisAverageEta = "jarvis_averageeta";

            /// <summary>
            /// IsEscalated attribute.
            /// </summary>
            public const string IsEscalated = "isescalated";

            /// <summary>
            /// JarvisEscalationMainCategory attribute.
            /// </summary>
            public const string JarvisEscalationMainCategory = "jarvis_escalationmaincategory";

            /// <summary>
            /// JarvisEscalationRemarks attribute.
            /// </summary>
            public const string JarvisEscalationRemarks = "jarvis_escalationremarks";

            /// <summary>
            /// JarvisEscalationSubcategory attribute.
            /// </summary>
            public const string JarvisEscalationSubcategory = "jarvis_escalationsubcategory";

            /// <summary>
            /// Case Title attribute.
            /// </summary>
            public const string Title = "title";

            /// <summary>
            /// Caller Role attribute.
            /// </summary>
            public const string CallerRole = "jarvis_callerrole";

            /// <summary>
            /// Jarvis RestGop Limit Out.
            /// </summary>
            public const string JarvisRestGopLimitOut = "jarvis_restgoplimitout";

            /// <summary>
            /// Jarvis Total Rest Currency Out.
            /// </summary>
            public const string JarvisTotalRestCurrencyOut = "jarvis_totalrestcurrencyout";
        }

        /// <summary>
        /// Case Escalation.
        /// </summary>
        public class CaseEscalation
        {
            /// <summary>
            /// Subject attribute.
            /// </summary>
            public const string Subject = "subject";

            /// <summary>
            /// Regarding Object Id.
            /// </summary>
            public const string RegardingObjectId = "regardingobjectid";

            /// <summary>
            /// Jarvis Main Category.
            /// </summary>
            public const string JarvisMainCategory = "jarvis_maincategory";

            /// <summary>
            /// Jarvis Sub Category.
            /// </summary>
            public const string JarvisSubCategory = "jarvis_subcategory";

            /// <summary>
            /// Jarvis Remark.
            /// </summary>
            public const string JarvisRemark = "jarvis_remark";

            /// <summary>
            /// State Code.
            /// </summary>
            public const string StateCode = "statecode";
        }

        /// <summary>
        /// Incident Nature.
        /// </summary>
        public class IncidentNature
        {
            /// <summary>
            /// jarvis Incident jarvis Incident Nature jarvis Inc.
            /// </summary>
            public const string jarvisIncidentjarvisIncidentNaturejarvisInc = "jarvis_Incident_jarvis_IncidentNature_jarvis_Inc";

            /// <summary>
            /// Incident Nature Vehicle Fuel PID jarvis incident nature id.
            /// </summary>
            public const string IncidentNatureVehicleFuelPid_jarvisincidentnatureid = "IncidentNature_VehicleFuelPid.jarvis_incidentnatureid";

            /// <summary>
            /// jarvis Incident Nature.
            /// </summary>
            public const string jarvisIncidentNature = "jarvis_incidentnature";

            /// <summary>
            /// Incident Incident Nature_jarvis Incident nature id.
            /// </summary>
            public const string IncidentIncidentNature_jarvisIincidentnatureid = "Incident_IncidentNature.jarvis_incidentnatureid";

            /// <summary>
            /// jarvis Incident jarvis Incident Nature Sub grid.
            /// </summary>
            public const string jarvisIncidentjarvisIncidentNatureSubgrid = "jarvis_IncidentNature_Incident_Incident";
        }

        /// <summary>
        /// Case Origin Code.
        /// </summary>
        public enum CaseOriginCode
        {
            /// <summary>
            /// Phone Value
            /// </summary>
            Phone = 1,

            /// <summary>
            /// Phone_Mercurius Value
            /// </summary>
            Phone_Mercurius = 334030001,

            /// <summary>
            /// Web E-service Value
            /// </summary>
            Web_Eservice = 334030002,

            /// <summary>
            /// Email_Mercurius Value
            /// </summary>
            Email_Mercurius = 334030003,

            /// <summary>
            /// Fax_Mercurius Value
            /// </summary>
            Fax_Mercurius = 334030004,

            /// <summary>
            /// Telematics_Mercurius Value
            /// </summary>
            Telematics_Mercurius = 334030005,
        }

        /// <summary>
        /// Account Type.
        /// </summary>
        public enum AccountType
        {
            /// <summary>
            /// Dealer Value.
            /// </summary>
            Dealer = 334030001,

            /// <summary>
            /// Customer Value
            /// </summary>
            Customer = 334030000,

            /// <summary>
            /// Partner Value.
            /// </summary>
            Partner = 334030002,

            /// <summary>
            /// MarketCompany Value.
            /// </summary>
            MarketCompany = 334030003,
        }

        /// <summary>
        /// Business Partner .
        /// </summary>
        public class Accounts
        {
            /// <summary>
            /// Business Partner Type.
            /// </summary>
            public const string BusinessPartnerType = "jarvis_accounttype";

            /// <summary>
            /// Blacklist attribute.
            /// </summary>
            public const string Blacklist = "jarvis_blacklisted";

            /// <summary>
            /// ViewStatus attribute.
            /// </summary>
            public const string ViewStatus = "statecode";

            /// <summary>
            /// ViewStatusReason attribute.
            /// </summary>
            public const string ViewStatusReason = "statuscode";

            /// <summary>
            /// VasExternalStatus attribute.
            /// </summary>
            public const string VasExternalStatus = "jarvis_externalstatus";

            /// <summary>
            /// VasStatus attribute.
            /// </summary>
            public const string VasStatus = "jarvis_onecasestatus";

            /// <summary>
            /// Bookable Resource attribute.
            /// </summary>
            public const string BookableResource = "jarvis_bookableresource";

            /// <summary>
            /// External Contact.
            /// </summary>
            public const string ExternalCurrency = "jarvis_externalcurrency";

            /// <summary>
            /// VAS Supported Contact.
            /// </summary>
            public const string SupportedCurrency = "jarvis_currency";
        }

        /// <summary>
        /// Vas Status for both external and one case.
        /// </summary>
        public enum VasStatus
        {
            /// <summary>
            /// Active Status.
            /// </summary>
            Active = 334030000,

            /// <summary>
            /// InActive Status.
            /// </summary>
            InActive = 334030001,
        }

        /// <summary>
        /// Case Type Code.
        /// </summary>
        public enum CaseTypeCode
        {
            /// <summary>
            /// Breakdown Value.
            /// </summary>
            Breakdown = 2,

            /// <summary>
            /// Query Value.
            /// </summary>
            Query = 3,
        }

        /// <summary>
        /// Escalation Activity Status.
        /// </summary>
        public enum EscalationActivitySatatus
        {
            /// <summary>
            /// Open Value.
            /// </summary>
            Open = 0,

            /// <summary>
            /// Completed Value.
            /// </summary>
            Completed = 1,

            /// <summary>
            /// Canceled Value.
            /// </summary>
            Canceled = 2,

            /// <summary>
            /// Scheduled Value.
            /// </summary>
            Scheduled = 3,
        }

        /// <summary>
        /// Assistance Type.
        /// </summary>
        public enum AssistanceType
        {
            /// <summary>
            /// Breakdown immediate.
            /// </summary>
            Breakdown_immediate = 334030000,
        }

        /// <summary>
        /// GOP class.
        /// </summary>
        public class Gop
        {
            /// <summary>
            /// Source attribute.
            /// </summary>
            public const string Source = "jarvis_source_";

            /// <summary>
            /// Approved attribute.
            /// </summary>
            public const string Approved = "jarvis_approved";

            /// <summary>
            /// RequestType attribute.
            /// </summary>
            public const string requestType = "jarvis_requesttype";

            /// <summary>
            /// totalLimitIn attribute.
            /// </summary>
            public const string totalLimitIn = "jarvis_totallimitin";

            /// <summary>
            /// totalLimitOut attribute.
            /// </summary>
            public const string totalLimitOut = "jarvis_totallimitout";

            /// <summary>
            /// contact attribute.
            /// </summary>
            public const string contact = "jarvis_contact";

            /// <summary>
            /// jarvis_mercurius gop id attribute.
            /// </summary>
            public const string jarvis_mercuriusgopid = "jarvis_mercuriusgopid";

            /// <summary>
            /// jarvis_gop limit in attribute.
            /// </summary>
            public const string jarvis_goplimitin = "jarvis_goplimitin";

            /// <summary>
            /// jarvis_gop limit out attribute.
            /// </summary>
            public const string jarvis_goplimitout = "jarvis_goplimitout";

            /// <summary>
            /// Comment attribute.
            /// </summary>
            public const string Comment = "jarvis_comment";

            /// <summary>
            /// GopReason attribute.
            /// </summary>
            public const string GopReason = "jarvis_gopreason";

            /// <summary>
            /// jarvis_translation status comment attribute.
            /// </summary>
            public const string Translationstatuscomment = "jarvis_translationstatuscomment";

            /// <summary>
            /// Translation status gop reason attribute.
            /// </summary>
            public const string Translationstatusgopreason = "jarvis_translationstatusgopreason";

            /// <summary>
            /// Gop Approval attribute.
            /// </summary>
            public const string GopApproval = "jarvis_gopapproval";

            /// <summary>
            /// Payment Type attribute.
            /// </summary>
            public const string PaymentType = "jarvis_paymenttype";

            /// <summary>
            /// Jarvis Gop In Currency.
            /// </summary>
            public const string JarvisGopInCurrency = "jarvis_gopincurrency";

            /// <summary>
            /// jarvis_case gop out available amount HD currency  attribute.
            /// </summary>
            public const string JarvisAmountHdCurrency = "jarvis_casegopoutavailableamounthdcurrency";

            /// <summary>
            /// jarvis_case gop out available amount HD  attribute.
            /// </summary>
            public const string JarvisAmountHd = "jarvis_casegopoutavailableamounthd";

            /// <summary>
            /// Jarvis Gop Out Currency attribute.
            /// </summary>
            public const string JarvisGopOutCurrency = "jarvis_gopoutcurrency";

            /// <summary>
            /// jarvis_case gop out available amount RD currency attribute.
            /// </summary>
            public const string JarvisAmountRdCurrency = "jarvis_casegopoutavailableamountrdcurrency";

            /// <summary>
            /// jarvis_case gop out available amount RD attribute.
            /// </summary>
            public const string JarvisAmountRd = "jarvis_casegopoutavailableamountrd";
        }

        /// <summary>
        /// Pass Out class.
        /// </summary>
        public class PassOut
        {
            /// <summary>
            /// ETA attribute.
            /// </summary>
            public const string Eta = "jarvis_eta";

            /// <summary>
            /// Status Code attribute.
            /// </summary>
            public const string Statuscode = "statuscode";

            /// <summary>
            /// Jarvis Mercurius Pass out id attribute.
            /// </summary>
            public const string JarvisMercuriusPassoutid = "jarvis_mercuriuspassoutid";

            /// <summary>
            /// Source attribute.
            /// </summary>
            public const string Source = "jarvis_source_";

            /// <summary>
            /// Jarvis Contact attribute.
            /// </summary>
            public const string JarvisContact = "jarvis_contact";

            /// <summary>
            /// transaction currency id attribute.
            /// </summary>
            public const string TransactionCurrencyId = "transactioncurrencyid";

            /// <summary>
            /// jarvis_description attribute.
            /// </summary>
            public const string JarvisDescription = "jarvis_description";

            /// <summary>
            /// Jarvis Gop Limit Out attribute.
            /// </summary>
            public const string JarvisGopLimitOut = "jarvis_goplimitout";

            /// <summary>
            /// Jarvis Payment type attribute.
            /// </summary>
            public const string JarvisPaymentType = "jarvis_paymenttype";

            /// <summary>
            /// Jarvis Repairing Dealer attribute.
            /// </summary>
            public const string JarvisRepairingDealer = "jarvis_repairingdealer";

            /// <summary>
            /// Jarvis ATA attribute.
            /// </summary>
            public const string JarvisAta = "jarvis_ata";

            /// <summary>
            /// jarvis ATC attribute.
            /// </summary>
            public const string JarvisAtc = "jarvis_atc";

            /// <summary>
            /// jarvis ETA attribute.
            /// </summary>
            public const string JarvisEta = "jarvis_eta";

            /// <summary>
            /// jarvis ETC attribute.
            /// </summary>
            public const string JarvisEtc = "jarvis_etc";

            /// <summary>
            /// modified on attribute.
            /// </summary>
            public const string Modifiedon = "modifiedon";

            /// <summary>
            /// jarvis_case gop out available amount attribute.
            /// </summary>
            public const string CaseGopOutAvailableAmount = "jarvis_casegopoutavailableamount";

            /// <summary>
            /// Jarvis GPS ETA attribute.
            /// </summary>
            public const string JarvisGpsETA = "jarvis_gpseta";
        }

        /// <summary>
        /// Case Notification.
        /// </summary>
        public class CaseNotification
        {
            /// <summary>
            /// jarvis Integration Mapping attribute.
            /// </summary>
            public const string jarvisIntegrationMapping = "jarvis_integrationmapping";

            /// <summary>
            /// Case2 Account Account id attribute.
            /// </summary>
            public const string Case2AccountAccountid = "Case2Account.accountid";

            /// <summary>
            /// Case2 Account Name attribute.
            /// </summary>
            public const string Case2AccountName = "Case2Account.name";
        }

        /// <summary>
        /// Case Monitor.
        /// </summary>
        public class CaseMonitor
        {
            /// <summary>
            /// regarding object id attribute.
            /// </summary>
            public const string regardingobjectid = "regardingobjectid";

            /// <summary>
            /// jarvis_fu link new attribute.
            /// </summary>
            public const string jarvis_fulinknew = "jarvis_fulinknew";

            /// <summary>
            /// jarvis_source attribute.
            /// </summary>
            public const string jarvis_source = "jarvis_source";
        }

        /// <summary>
        /// Notes class.
        /// </summary>
        public class Notes
        {
            /// <summary>
            /// regarding Object Id attribute.
            /// </summary>
            public const string regardingObjectId = "objectid";

            /// <summary>
            /// is document attribute.
            /// </summary>
            public const string isdocument = "isdocument";
        }

        /// <summary>
        /// Pass Out Status.
        /// </summary>
        public enum PassOutStatus
        {
            /// <summary>
            /// Draft Value.
            /// </summary>
            Draft = 1,

            /// <summary>
            /// To Be Sent Value.
            /// </summary>
            ToBeSent = 334030001,

            /// <summary>
            /// Cancelled Value
            /// </summary>
            Cancelled = 2,

            /// <summary>
            /// Sent Value.
            /// </summary>
            Sent = 334030002,
        }

        /// <summary>
        /// Gop Request Type.
        /// </summary>
        public enum GopRequestType
        {
            /// <summary>
            /// GOP Value.
            /// </summary>
            GOP = 334030000,

            /// <summary>
            /// GOP HD Value.
            /// </summary>
            GOP_HD = 334030001,

            /// <summary>
            /// GOP RD Value.
            /// </summary>
            GOP_RD = 334030002,
        }

        /// <summary>
        /// Gop Approval.
        /// </summary>
        public enum GopApproval
        {
            /// <summary>
            /// Pending Value.
            /// </summary>
            Pending = 334030000,

            /// <summary>
            /// Approved Value.
            /// </summary>
            Approved = 334030001,

            /// <summary>
            /// Declined Value.
            /// </summary>
            Declined = 334030002,
        }

        /// <summary>
        /// Payment Type.
        /// </summary>
        public enum PaymentType
        {
            /// <summary>
            /// CreditCard Value.
            /// </summary>
            CreditCard = 334030002,
        }

        /// <summary>
        /// Translation Status.
        /// </summary>
        public enum TranslationStatus
        {
            /// <summary>
            /// NotStarted Value.
            /// </summary>
            NotStarted = 334030000,

            /// <summary>
            /// InProgress Value.
            /// </summary>
            InProgress = 334030001,

            /// <summary>
            /// Completed Value.
            /// </summary>
            Completed = 334030002,
        }

        /// <summary>
        /// Pass Out Translation.
        /// </summary>
        public class PassOutTranslation
        {
            /// <summary>
            /// Source attribute.
            /// </summary>
            public const string Source = "jarvis_source";

            /// <summary>
            /// EtaReason attribute.
            /// </summary>
            public const string EtaReason = "jarvis_etareason";
        }

        /// <summary>
        /// Case Translation.
        /// </summary>
        public class CaseTranslation
        {
            /// <summary>
            /// Source attribute.
            /// </summary>
            public const string Source = "jarvis_source";

            /// <summary>
            /// Description attribute.
            /// </summary>
            public const string Description = "jarvis_description";
        }

        /// <summary>
        /// Repair Info Translation.
        /// </summary>
        public class RepairInfoTranslation
        {
            /// <summary>
            /// Source attribute.
            /// </summary>
            public const string Source = "jarvis_source";

            /// <summary>
            /// RepairInformation attribute.
            /// </summary>
            public const string RepairInformation = "jarvis_comment";

            /// <summary>
            /// PartsInformation attribute.
            /// </summary>
            public const string PartsInformation = "jarvis_partsinformation";

            /// <summary>
            /// TowingRental attribute.
            /// </summary>
            public const string TowingRental = "jarvis_towingrental";

            /// <summary>
            /// WarrantyInformation attribute.
            /// </summary>
            public const string WarrantyInformation = "jarvis_warrantyinformation";
        }

        /// <summary>
        /// Job End Translation.
        /// </summary>
        public class JobEndTranslation
        {
            /// <summary>
            /// Source attribute.
            /// </summary>
            public const string Source = "jarvis_source";

            /// <summary>
            /// ActualCauseFault attribute.
            /// </summary>
            public const string ActualCauseFault = "jarvis_actualcausefault";

            /// <summary>
            /// TemporaryRepair attribute.
            /// </summary>
            public const string TemporaryRepair = "jarvis_temporaryrepair";
        }

        /// <summary>
        /// Remark class.
        /// </summary>
        public class Remark
        {
            /// <summary>
            /// source attribute.
            /// </summary>
            public const string remarkSource = "source";

            /// <summary>
            /// regarding object id attribute.
            /// </summary>
            public const string regardingField = "regardingobjectid";
        }

        /// <summary>
        /// Users class.
        /// </summary>
        public class Users
        {
            /// <summary>
            /// logicalName attribute.
            /// </summary>
            public const string logicalName = "systemuser";

            /// <summary>
            /// fullName attribute.
            /// </summary>
            public const string fullName = "fullname";
        }

        /// <summary>
        /// Jarvis country class.
        /// </summary>
        public class JarvisCountry
        {
            /// <summary>
            /// Jarvis Average ETA duration.
            /// </summary>
            public const string JarvisAverageetaduration = "jarvis_averageetaduration";

            /// <summary>
            /// Jarvis Name.
            /// </summary>
            public const string Name = "jarvis_name";
        }

        /// <summary>
        /// Source enum.
        /// </summary>
        public enum Source
        {
            /// <summary>
            /// CTDI Value.
            /// </summary>
            CTDI = 334030000,

            /// <summary>
            /// CDB Value.
            /// </summary>
            CDB = 334030001,

            /// <summary>
            /// Mercurius Value.
            /// </summary>
            Mercurius = 334030002,

            /// <summary>
            /// Jarvis Value.
            /// </summary>
            Jarvis = 334030003,

            /// <summary>
            /// VDA Value.
            /// </summary>
            VDA = 334030004,

            /// <summary>
            /// eService Value.
            /// </summary>
            eService = 334030005,
        }

        /// <summary>
        /// Post Source.
        /// </summary>
        public enum PostSource
        {
            /// <summary>
            /// Auto Post Value.
            /// </summary>
            AutoPost = 1,

            /// <summary>
            /// Manual Post Value.
            /// </summary>
            ManualPost = 2,

            /// <summary>
            /// Action Hub Post Value.
            /// </summary>
            ActionHubPost = 3,
        }

        /// <summary>
        /// Case Contact class.
        /// </summary>
        public class Casecontact
        {
            /// <summary>
            /// jarvis First name.
            /// </summary>
            public const string jarvisFirstname = "jarvis_firstname";

            /// <summary>
            /// jarvis Last name.
            /// </summary>
            public const string jarvisLastname = "jarvis_lastname";

            /// <summary>
            /// jarvis Mobile phone.
            /// </summary>
            public const string jarvisMobilephone = "jarvis_mobilephone";

            /// <summary>
            /// jarvis Role.
            /// </summary>
            public const string jarvisRole = "jarvis_role";

            /// <summary>
            /// jarvis Preferred language.
            /// </summary>
            public const string jarvisPreferredlanguage = "jarvis_preferredlanguage";

            /// <summary>
            /// jarvis Case contact type.
            /// </summary>
            public const string jarvisCasecontacttype = "jarvis_casecontacttype";

            /// <summary>
            /// jarvis Preferred method of contact.
            /// </summary>
            public const string jarvisPreferredmethodofcontact = "jarvis_preferredmethodofcontact";

            /// <summary>
            /// jarvis Incident.
            /// </summary>
            public const string jarvisIncident = "jarvis_incident";

            /// <summary>
            /// jarvis Case contact id.
            /// </summary>
            public const string jarvisCasecontactid = "jarvis_casecontactid";

            /// <summary>
            /// jarvis Case contact.
            /// </summary>
            public const string jarvisCasecontact = "jarvis_casecontact";

            /// <summary>
            /// jarvis Case.
            /// </summary>
            public const string jarvisCase = "jarvis_case";

            /// <summary>
            /// jarvis Is Manual Update.
            /// </summary>
            public const string jarvisIsManualUpdate = "jarvis_ismanualupdate";

            /// <summary>
            /// jarvis Phone.
            /// </summary>
            public const string jarvisPhone = "jarvis_phone";

            /// <summary>
            /// jarvis Email.
            /// </summary>
            public const string jarvisEmail = "jarvis_email";

            /// <summary>
            /// jarvis business Partner.
            /// </summary>
            public const string jarvisbusinessPartner = "jarvis_businesspartner";

            /// <summary>
            /// jarvis business Partner Type.
            /// </summary>
            public const string jarvisbusinessPartnerType = "jarvis_businesspartnertype";

            /// <summary>
            /// jarvis Caller language.
            /// </summary>
            public const string jarvisCallerlanguage = "jarvis_callerlanguage";

            /// <summary>
            /// jarvis Driver language.
            /// </summary>
            public const string jarvisDriverlanguage = "jarvis_driverlanguage";

            /// <summary>
            /// driver Role.
            /// </summary>
            public const int driverRole = 334030006;

            /// <summary>
            /// driver Contact Type.
            /// </summary>
            public const int driverContactType = 334030001;

            /// <summary>
            /// driver Method of Contact.
            /// </summary>
            public const int driverMethodofContact = 334030001;

            /// <summary>
            /// caller Contact Type.
            /// </summary>
            public const int callerContactType = 334030000;

            /// <summary>
            /// caller Method of Contact.
            /// </summary>
            public const int callerMethodofContact = 334030003;
        }

        /// <summary>
        /// Trans Type.
        /// </summary>
        public class TransType
        {
            /// <summary>
            /// pass Out Update.
            /// </summary>
            public const string PassOutUpdate = "Jarvis.Event.PassOut";

            /// <summary>
            /// pass Out ETA.
            /// </summary>
            public const string PassOutETA = "Jarvis.Event.ETAUpdate";

            /// <summary>
            /// pass Out ATA.
            /// </summary>
            public const string PassOutATA = "Jarvis.Event.ATAUpdate";

            /// <summary>
            /// pass Out ATA.
            /// </summary>
            public const string PassOutETC = "Jarvis.Event.ETCUpdate";

            /// <summary>
            /// pass Out ATA.
            /// </summary>
            public const string PassOutATC = "Jarvis.Event.ATCUpdate";

            /// <summary>
            /// pass Out Delayed ETA.
            /// </summary>
            public const string PassOutDelayedETA = "Jarvis.Event.delayedETAUpdate";

            /// <summary>
            /// gop Plus.
            /// </summary>
            public const string GopPlus = "Jarvis.Event.GOP+";

            /// <summary>
            /// Create Case.
            /// </summary>
            public const string CreateCase = "Jarvis.Event.CreateCase";

            /// <summary>
            /// Update Case.
            /// </summary>
            public const string UpdateCase = "Jarvis.Event.UpdateCase";

            /// <summary>
            /// CSIS Case.
            /// </summary>
            public const string CSISCase = "CSIS.Event.Case";
        }

        /// <summary>
        /// Plugin Stage.
        /// </summary>
        public enum PluginStage : int
        {
            /// <summary>
            /// Pre Validation.
            /// </summary>
            PreValidation = 10,

            /// <summary>
            /// Pre Operation.
            /// </summary>
            PreOperation = 20,

            /// <summary>
            /// Post Operation.
            /// </summary>
            PostOperation = 40,
        }

        /// <summary>
        /// Plugin Message.
        /// </summary>
        public class PluginMessage
        {
            /// <summary>
            /// Create Message.
            /// </summary>
            public const string Create = "CREATE";

            /// <summary>
            /// Update Message.
            /// </summary>
            public const string Update = "UPDATE";

            /// <summary>
            /// Delete Message.
            /// </summary>
            public const string Delete = "DELETE";
        }

        /// <summary>
        /// Translation Type.
        /// </summary>
        public class TranslationType
        {
            /// <summary>
            /// Create attribute.
            /// </summary>
            public const string Create = "CREATE";

            /// <summary>
            /// Repair Info.
            /// </summary>
            public const string RepairInfo = "REPAIR INFO";

            /// <summary>
            /// Warranty attribute.
            /// </summary>
            public const string Warranty = "WARRANTY";

            /// <summary>
            /// Towing Info.
            /// </summary>
            public const string TowingInfo = "TOWING INFO";

            /// <summary>
            /// Actual Fault.
            /// </summary>
            public const string ActualFault = "ACTUAL CAUSE/FAULT";

            /// <summary>
            /// Temp Repair.
            /// </summary>
            public const string TempRepair = "TEMP REPAIR";
        }

        /// <summary>
        /// Jarvis Configuration.
        /// </summary>
        public class JarvisConfiguration
        {
            /// <summary>
            /// Automation monitor action.
            /// </summary>
            public const string Automationmonitoraction = "jarvis_automationmonitoraction";

            /// <summary>
            /// Communication gop confirmation.
            /// </summary>
            public const string Communicationgopconfirmation = "jarvis_communicationgopconfirmation";

            /// <summary>
            /// Communication pass out confirmation.
            /// </summary>
            public const string Communicationpassoutconfirmation = "jarvis_communicationpassoutconfirmation";

            /// <summary>
            /// Communication eta.
            /// </summary>
            public const string Communicationeta = "jarvis_communicationeta";

            /// <summary>
            /// Communication etc.
            /// </summary>
            public const string Communicationetc = "jarvis_communicationetc";

            /// <summary>
            /// Communication job end details.
            /// </summary>
            public const string Communicationjobenddetails = "jarvis_communicationjobenddetails";

            /// <summary>
            /// Communication customer commitment codes.
            /// </summary>
            public const string Communicationcustomercommitmentcodes = "jarvis_communicationcustomercommitmentcodes";

            /// <summary>
            /// Automation translation.
            /// </summary>
            public const string Automationtranslation = "jarvis_automationtranslation";

            /// <summary>
            /// Automation case status change.
            /// </summary>
            public const string Automationcasestatuschange = "jarvis_automationcasestatuschange";

            /// <summary>
            /// Automation initial gop.
            /// </summary>
            public const string Automationinitialgop = "jarvis_automationinitialgop";

            /// <summary>
            /// Automation available amount.
            /// </summary>
            public const string Automationavailableamount = "jarvis_automationavailableamount";

            /// <summary>
            /// Automation volvo pay.
            /// </summary>
            public const string VolvoPayAutomation = "jarvis_volvopayautomation";

            /// <summary>
            /// Automation gop.
            /// </summary>
            public const string Automationgop = "jarvis_automationgop";

            /// <summary>
            /// Automation whitelist approval.
            /// </summary>
            public const string Automationwhitelistapproval = "jarvis_automationwhitelistapproval";

            /// <summary>
            /// Automation release case.
            /// </summary>
            public const string Automationreleasecase = "jarvis_automationreleasecase";

            /// <summary>
            /// Case Opening validation for Integration of warranty /Contract /whitelist data.
            /// </summary>
            public const string CaseOpeningValidation = "jarvis_caseopeningvalidation";
        }

        /// <summary>
        /// Class to Access FetchXml.
        /// </summary>
        public class FetchXmls
        {
            /// <summary>
            /// // Retrieve PassOuts .
            /// </summary>
            public const string retrievePassOuts = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' no-lock='true' distinct='false'>
            <entity name='jarvis_passout'>
            <attribute name='jarvis_passoutid'/>
            <attribute name='jarvis_name'/>
            <attribute name='createdon'/>
            <attribute name='jarvis_etc'/>
            <attribute name='jarvis_eta'/>
            <attribute name='jarvis_incident'/>
            <attribute name='jarvis_atc'/>
            <attribute name='jarvis_ata'/>
            <attribute name='modifiedon'/>
            <order attribute='jarvis_etc' descending='false'/>
            <filter type='and'>
            <condition attribute='jarvis_incident' operator='eq' value='{0}'/>
            <condition attribute='jarvis_sendmissionconfirmationviaemail' operator='eq' value='1'/>
            </filter>
            </entity>
            </fetch>";

            /// <summary>
            ///    // Calculate Total Approved Amount for GOP.
            /// </summary>
            public const string calculateTotalAmntAppr = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false' aggregate='true'>
            <entity name='jarvis_gop'>
            <attribute name='jarvis_goplimitout' alias='goplimitout' aggregate='sum' /> 
            <filter type='and'>
            <condition attribute='jarvis_incident' operator='eq' value='{0}'/>
            <condition attribute='jarvis_gopapproval' operator='eq' value='334030001'  />
            <condition attribute='jarvis_relatedgop' operator='null' />
            <condition attribute='statecode' operator='eq' value='0' />
            </filter>
            </entity>
            </fetch>";

            /// <summary>
            ///  // Calculate Total Approved Amount for Pass Outs.
            /// </summary>
            public const string calculatePassOutTotalAmnt = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false' aggregate='true'>
            <entity name='jarvis_passout'>
            <attribute name='jarvis_goplimitout' alias='goplimitout' aggregate='sum'/> 
            <filter type='and'>
            <condition attribute='jarvis_incident' operator='eq' value='{0}'/>
            <condition attribute='statecode' operator='eq' value='0' />
            </filter>
            </entity>
            </fetch>";

            /// <summary>
            ///   // Get VAS Configuration.
            /// </summary>
            public const string VASConfiguration = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' no-lock='true' distinct='false'>
            <entity name='jarvis_configurationjarvis'>
            <attribute name='jarvis_configurationjarvisid' />
            <attribute name='jarvis_name' />
            <attribute name='createdon' />
            <attribute name='jarvis_servicefee' />
            <attribute name='transactioncurrencyid' />
            <order attribute='jarvis_name' descending='false' />
            <filter type='and'>
            <condition attribute='jarvis_name' operator='eq' value='{0}' />
            </filter>
            </entity>
            </fetch>";

            /// <summary>
            /// Get VAS Knowledge Search Configuration.
            /// </summary>
            public const string VASConfigurationKnowledge = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' no-lock='true' distinct='false'>
              <entity name='jarvis_configurationjarvis'>
                <attribute name='jarvis_configurationjarvisid' />
                <attribute name='jarvis_name' />
                <attribute name='modifiedon' />
                <attribute name='jarvis_knowledgesearchforfainclalert' />
                <attribute name='jarvis_knowledgesearchforfa' />
                <attribute name='jarvis_knowledgesearchforbdinclalert' />
                <attribute name='jarvis_knowledgesearchforbd' />
                <attribute name='jarvis_knowledgebaseoverviewurl' />
                <attribute name='jarvis_incidentnatureconjunctionbd' />
                <attribute name='jarvis_incidentnatureconjunctionfa' />
                <order attribute='modifiedon' descending='true' />
                <filter type='and'>
                  <condition attribute='statecode' operator='eq' value='0' />
                </filter>
              </entity>
            </fetch>";

            /// <summary>
            ///   // Get Exchange Rate.
            /// </summary>
            public const string ExchangeRate = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' no-lock='true' distinct='false'>
    <entity name='jarvis_exchangerate'>
    <attribute name='jarvis_exchangerateid' />
    <attribute name='jarvis_name' />
    <attribute name='createdon' />
    <attribute name='jarvis_value' />
    <order attribute='jarvis_name' descending='false' />
    <filter type='and'>
      <condition attribute='jarvis_basecurrency' operator='eq' uitype='transactioncurrency' value='{0}' />
      <condition attribute='jarvis_countercurrency' operator='eq' uitype='transactioncurrency' value='{1}' />
    </filter>
  </entity>
</fetch>";

            /// <summary>
            ///  // Get Work Type for Work Order creation.
            /// </summary>
            public const string getWorkType = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' no-lock='true' distinct='false'>
  <entity name='msdyn_workordertype'>
    <attribute name='msdyn_workordertypeid' />
    <attribute name='msdyn_name' />
    <attribute name='createdon' />
    <attribute name='msdyn_incidentrequired' />
    <order attribute='msdyn_name' descending='false' />
  </entity>
</fetch>";

            /// <summary>
            ///    // Get Business Process Name.
            /// </summary>
            public const string getProcessName = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='jarvis_integrationconfiguration'>
    <attribute name='jarvis_integrationconfigurationid'/>
    <attribute name='jarvis_integrationname'/>
    <attribute name='jarvis_integrationcode'/>
    <order attribute='jarvis_integrationname' descending='false'/>
    <filter type='and'>
      <condition attribute='jarvis_description' operator='like' value='%{0}%'/>
    </filter>
  </entity>
</fetch>";

            /// <summary>
            ///    // Get Business Process For Case.
            /// </summary>
            public const string getBreakDownProcess = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='workflow'>
    <attribute name='workflowid' />
    <attribute name='name' />
    <attribute name='category' />
    <attribute name='primaryentity' />
    <attribute name='statecode' />
    <attribute name='createdon' />
    <attribute name='ownerid' />
    <attribute name='owningbusinessunit' />
    <attribute name='type' />
    <order attribute='name' descending='false' />
    <filter type='and'>
      <condition attribute='name' operator='eq' value='{0}'/>
      <condition attribute='category' operator='eq' value='4' />
    </filter>
  </entity>
</fetch>";

            /// <summary>
            ///  // Get BPF Instance for Case.
            /// </summary>
            public const string getCaseBPFInstance = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
	<entity name='jarvis_vasbreakdownprocess'>
		<attribute name='businessprocessflowinstanceid' />
		<attribute name='bpf_name' />
		<attribute name='createdon' />
		<attribute name='bpf_incidentid' />
		<attribute name='activestageid' />
		<attribute name='statecode' />
		<attribute name='createdby' />
		<attribute name='processid' />
		<order attribute='bpf_name' descending='false' />
		<filter type='and'>
			<condition attribute='bpf_incidentid' operator='eq' value='{0}'/>
		</filter>
	</entity>
</fetch>";

            /// <summary>
            ///  // Get PriceList for WO.
            /// </summary>
            public const string getPricelist = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='pricelevel'>
    <attribute name='name' />
    <attribute name='transactioncurrencyid' />
    <attribute name='enddate' />
    <attribute name='begindate' />
    <attribute name='statecode' />
    <attribute name='pricelevelid' />
    <order attribute='name' descending='false' />
  </entity>
</fetch>";

            /// <summary>
            ///  // Get Work Orders.
            /// </summary>
            public const string getWorkOrders = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='msdyn_workorder'>
    <attribute name='msdyn_name' />
    <attribute name='createdon' />
    <attribute name='msdyn_serviceaccount' />
    <attribute name='msdyn_workorderid' />
    <attribute name='msdyn_functionallocation' />
    <attribute name='msdyn_longitude' />
    <attribute name='msdyn_latitude' />
    <order attribute='msdyn_name' descending='false' />
    <filter type='and'>
      <condition attribute='msdyn_servicerequest' operator='eq' value='{0}'/>
    </filter>
  </entity>
</fetch>";

            /// <summary>
            ///  // Get Brand Incident Types.
            /// </summary>
            public const string getIncidentTypesBrands = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='msdyn_incidenttype'>
    <attribute name='msdyn_name' />
    <attribute name='createdon' />
    <attribute name='msdyn_estimatedduration' />
    <attribute name='msdyn_incidenttypeid' />
    <order attribute='msdyn_name' descending='false' />
    <filter type='and'>
      <condition attribute='jarvis_brandrelated' operator='eq' value='1' />
      <condition attribute='jarvis_brand' operator='eq' value='{0}'/>
    </filter>
  </entity>
</fetch>";

            /// <summary>
            /// // Get Incident Nature For WO.
            /// </summary>
            public const string getIncidentNatureForCase = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='jarvis_incidentnature'>
    <attribute name='jarvis_incidentnatureid' />
    <attribute name='jarvis_name' />
    <attribute name='createdon' />
    <attribute name='jarvis_incidenttype' />
    <attribute name='jarvis_description' />
    <attribute name='jarvis_incident' />
    <order attribute='jarvis_name' descending='false' />
    <filter type='and'>
      <condition attribute='jarvis_incident' operator='eq' value='{0}'/>
      <condition attribute='jarvis_incidenttype' operator='not-null' />
    </filter>
  </entity>
</fetch>";

            /// <summary>
            ///   // Get Incident Nature For Delete.
            /// </summary>
            public const string getIncNatures = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>
  <entity name='jarvis_incidentnature'>
    <attribute name='jarvis_incidentnatureid' />
    <attribute name='jarvis_name' />
    <attribute name='createdon' />
    <order attribute='jarvis_name' descending='false' />
    <filter type='and'>
      <condition attribute='statecode' operator='eq' value='0' />
    </filter>
    <link-entity name='jarvis_incident_jarvis_incidentnature' from='jarvis_incidentnatureid' to='jarvis_incidentnatureid' visible='false' intersect='true'>
      <link-entity name='incident' from='incidentid' to='incidentid' alias='ac'>
        <filter type='and'>
          <condition attribute='incidentid' operator='eq' value='{0}'/>
        </filter>
      </link-entity>
    </link-entity>
  </entity>
</fetch>";

            /// <summary>
            ///     // Get WO Requirement.
            /// </summary>
            public const string getResourceRequirement = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='msdyn_resourcerequirement'>
    <attribute name='msdyn_resourcerequirementid' />
    <attribute name='msdyn_name' />
    <attribute name='createdon' />
    <order attribute='msdyn_name' descending='false' />
    <filter type='and'>
      <condition attribute='msdyn_workorder' operator='eq' value='{0}'/>
    </filter>
  </entity>
</fetch>";

            /// <summary>
            ///   // Get Resource Characteristics.
            /// </summary>
            public const string getListofResourceCharacteristics = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='msdyn_requirementcharacteristic'>
    <attribute name='msdyn_requirementcharacteristicid' />
    <attribute name='msdyn_resourcerequirement' />
    <attribute name='msdyn_ratingvalue' />
    <attribute name='msdyn_characteristic' />
    <attribute name='createdon' />
    <order attribute='msdyn_resourcerequirement' descending='false' />
    <filter type='and'>
      <condition attribute='msdyn_resourcerequirement' operator='eq' value='{0}'/>
    </filter>
  </entity>
</fetch>";

            /// <summary>
            ///    // Get Bookable Resource Characteristics.
            /// </summary>
            public const string getBookableServices = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='bookableresourcecharacteristic'>
    <attribute name='createdon' />
    <attribute name='resource' />
    <attribute name='ratingvalue' />
    <attribute name='characteristic' />
    <attribute name='bookableresourcecharacteristicid' />
    <order attribute='resource' descending='false' />
    <order attribute='characteristic' descending='false' />
    <filter type='and'>
      <condition attribute='resource' operator='eq' value='{0}'/>
    </filter>
  </entity>
</fetch>";

            /// <summary>
            ///   // Get Params For Search Availability API.
            /// </summary>
            public const string getParamsForSearchAPI = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='jarvis_integrationconfiguration'>
    <attribute name='jarvis_integrationconfigurationid'/>
    <attribute name='jarvis_integrationname'/>
    <attribute name='jarvis_integrationmapping'/>
    <attribute name='jarvis_integrationcode'/>
    <order attribute='jarvis_integrationname' descending='false'/>
    <filter type='and'>
      <condition attribute='jarvis_integrationcode' operator='like' value='%{0}%'/>
    </filter>
  </entity>
</fetch>";

            /// <summary>
            /// // Get Pass outs.
            /// </summary>
            public const string CasePassouts = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
   <entity name='jarvis_passout'>
    <attribute name='jarvis_passoutid' />
    <attribute name='jarvis_name' />
    <attribute name='createdon' />
    <attribute name='jarvis_repairingdealer' />
    <attribute name='jarvis_goplimitout' />
    <attribute name='transactioncurrencyid' />
    <attribute name='modifiedon' />
    <order attribute='modifiedon' descending='true' />
    <filter type='and'>
      <condition attribute='statecode' operator='eq' value='0' />
      <condition attribute='jarvis_incident' operator='eq' uitype='incident' value='{0}' />
<condition attribute='jarvis_passoutid' operator='eq'   uitype='jarvis_passout' value='{1}' />
 <condition attribute='statuscode' operator='in'>
        <value>334030002</value>
        <value>334030001</value>
      </condition>
    </filter>
  </entity>
</fetch>";

            /// <summary>
            /// // Get Active Pass outs.
            /// </summary>
            public const string CaseActivePassouts = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='jarvis_passout'>
    <attribute name='jarvis_passoutid' />
    <attribute name='jarvis_name' />
    <attribute name='createdon' />
    <attribute name='jarvis_repairingdealer' />
    <attribute name='jarvis_goplimitout' />
    <attribute name='transactioncurrencyid' />
    <attribute name='modifiedon' />
    <order attribute='modifiedon' descending='true' />
    <filter type='and'>
      <condition attribute='statecode' operator='eq' value='0' />
      <condition attribute='jarvis_incident' operator='eq' uitype='incident' value='{0}' />
      <condition attribute='statuscode' operator='in'>
        <value>334030002</value>
        <value>334030001</value>
      </condition>
    </filter>
  </entity>
</fetch>";

            /// <summary>
            ///  // fetch all GOPs related to case having the current pass out as repairing dealer.
            /// </summary>
            public const string CaseGOPsForPassout = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='jarvis_gop'>
    <attribute name='jarvis_gopid' />
    <attribute name='jarvis_name' />
    <attribute name='createdon' />
    <attribute name='jarvis_totallimitout' />
    <attribute name='jarvis_requesttype' />
    <attribute name='jarvis_totallimitoutcurrency' />
    <order attribute='createdon' descending='true' />
    <filter type='and'>
      <condition attribute='statecode' operator='eq' value='0' />
      <condition attribute='jarvis_relatedgop' operator='null' />
      <condition attribute='jarvis_incident' operator='eq' uitype='incident' value='{0}' />
      <condition attribute='jarvis_repairingdealer' operator='eq' uitype='jarvis_passout' value='{1}' />
    </filter>
  </entity>
</fetch>";

            /// <summary>
            ///  // Fetch Last Modified GOP for Query.
            /// </summary>
            public const string getLastModGOP = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='jarvis_gop'>
    <attribute name='jarvis_gopid' />
    <attribute name='jarvis_name' />
    <attribute name='createdon' />
    <attribute name='modifiedon' />
    <order attribute='modifiedon' descending='true' />
    <filter type='and'>
      <condition attribute='jarvis_gopapproval' operator='eq' value='334030001'  />
      <condition attribute='jarvis_incident' operator='eq' value='{0}'/>
      <condition attribute='jarvis_relatedgop' operator='null' />
     <condition attribute='statecode' operator='eq' value='0' />
    </filter>
  </entity>
</fetch>";

            /// <summary>
            /// // Fetch all Gop of a Case with particular home dealer.
            /// </summary>
            public const string getGOPsForDealer = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='jarvis_gop'>
    <attribute name='jarvis_gopid' />
    <attribute name='jarvis_name' />
    <attribute name='jarvis_name' />
    <attribute name='jarvis_paymenttype' />
    <attribute name='jarvis_goplimitout' />
    <attribute name='jarvis_goplimitin' />
    <attribute name='jarvis_totallimitout' />
    <attribute name='jarvis_requesttype' />
    <attribute name='jarvis_approved' />
    <attribute name='jarvis_gopapproval' />
    <attribute name='jarvis_requestedcontact' />
    <attribute name='jarvis_totallimitoutcurrency' />
    <attribute name='jarvis_totallimitin' />
    <attribute name='jarvis_totallimitincurrency' />
    <attribute name='jarvis_repairingdealer' />
    <attribute name='jarvis_gopoutcurrency' />
    <attribute name='jarvis_gopincurrency' />
    <attribute name='jarvis_rdpartyordernumber' />
    <attribute name='jarvis_requestedcontact' />
    <attribute name='jarvis_dealer' />
    <attribute name='jarvis_contact' />
    <attribute name='jarvis_requestedcontact' />
    <attribute name='jarvis_rdpartyordernumber' />
    <attribute name='jarvis_gopreason' />
    <attribute name='modifiedon' />
    <attribute name='jarvis_paymenttype' />
    <attribute name='jarvis_gopapprovaltime' />
<attribute name='jarvis_creditcardgopinbooking' />
<attribute name='jarvis_volvopaypaymentrequestsent' />
<attribute name='jarvis_vat' />
    <attribute name='jarvis_creditcardemailaddress' />
    <attribute name='jarvis_creditcardmessage' />
    <attribute name='jarvis_creditcardlanguage' />
    <attribute name='jarvis_channel' />
    <attribute name='jarvis_creditcardincurrency' />
    <order attribute='jarvis_gopapprovaltime' descending='true' />
    <order attribute='modifiedon' descending='true' />
    <filter type='and'>
      <condition attribute='statecode' operator='eq' value='0' />
      <condition attribute='jarvis_relatedgop' operator='null' />
      <condition attribute='jarvis_incident' operator='eq' uitype='incident' value='{0}' />
      <condition attribute='jarvis_gopapproval' operator='ne' value='334030002' />
      <condition attribute='jarvis_dealer' operator='eq' uitype='account' value='{1}' />
    </filter>
  </entity>
</fetch>";

            /// <summary>
            ///    // Fetch all Gop of a case.
            /// </summary>
            public const string getGOPsForCase = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='jarvis_gop'>
    <attribute name='jarvis_gopid' />
    <attribute name='jarvis_name' />
    <attribute name='jarvis_dealer' />
    <attribute name='modifiedon' />
<attribute name='jarvis_paymenttype' />
<attribute name='createdon' />
    <attribute name='jarvis_goplimitout' />
    <attribute name='jarvis_goplimitin' />
    <attribute name='jarvis_totallimitout' />
    <attribute name='jarvis_requesttype' />
    <attribute name='jarvis_paymenttype' />
    <attribute name='jarvis_approved' />
    <attribute name='jarvis_gopapproval' />
    <attribute name='jarvis_totallimitoutcurrency' />
    <attribute name='jarvis_totallimitin' />
    <attribute name='jarvis_totallimitincurrency' />
    <attribute name='jarvis_creditcardgopinbooking' />
    <attribute name='jarvis_creditcardincurrency' />
    <attribute name='jarvis_gopoutcurrency' />
    <attribute name='jarvis_repairingdealer' />
    <attribute name='jarvis_creditcardgopinbooking' />
    <attribute name='jarvis_creditcardincurrency' />
    <attribute name='jarvis_channel' />
    <attribute name='jarvis_vat' />
    <attribute name='jarvis_creditcardlanguage' />
    <order attribute='jarvis_gopapprovaltime' descending='true' />
    <order attribute='modifiedon' descending='true' />
    <filter type='and'>
      <condition attribute='statecode' operator='eq' value='0' />
      <condition attribute='jarvis_relatedgop' operator='null' />
      <condition attribute='jarvis_gopapproval' operator='ne' value='334030002' />
      <condition attribute='jarvis_incident' operator='eq' uitype='incident' value='{0}' />
    </filter>
  </entity>
</fetch>";

            /// <summary>
            ///    // IncidentNatures associated to Vehicle-Fuel/PowerType.
            /// </summary>
            public const string getFuelPowerType = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>
<entity name='jarvis_vehiclefuelpowertype'>
<attribute name='jarvis_vehiclefuelpowertypeid' />
<filter type='and'>
<condition attribute='jarvis_vehiclefuelpowertypeid' operator='eq' value='{0}'/>
<condition attribute='statecode' operator='eq' value='0' />
</filter>
<link-entity name='jarvis_incidentnature_jarvis_vehiclefue' from='jarvis_vehiclefuelpowertypeid' to='jarvis_vehiclefuelpowertypeid'  link-type='outer'>
<link-entity name='jarvis_incidentnature' from='jarvis_incidentnatureid' to='jarvis_incidentnatureid' alias='IncidentNature_VehicleFuelPid'>
<attribute name='jarvis_incidentnatureid'/>
<attribute name='jarvis_name' />
</link-entity>
</link-entity>
</entity>
</fetch>";

            /// <summary>
            ///   // Get IncidentNature Associated To Case.
            /// </summary>
            public const string getIncidentNature = @"<fetch>
	<entity name='incident'>
		<filter type='and'>
			<condition attribute='incidentid' operator='eq' value='{0}'/>
              <condition attribute='statecode' operator='eq' value='0' />
		</filter>
			<link-entity name='jarvis_incident_jarvis_incidentnature' from='incidentid' to='incidentid' link-type='outer' intersect='true'>
			<link-entity name='jarvis_incidentnature' from='jarvis_incidentnatureid' to='jarvis_incidentnatureid' alias='Incident_IncidentNature' link-type='outer' intersect='true'>
				<attribute name='jarvis_incidentnatureid' />
			</link-entity>
		</link-entity>
	</entity>
</fetch>";

            /// <summary>
            ///  // Get Related Shadow GOP .
            /// </summary>
            public const string geRelatedShadowGOP = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='jarvis_gop'>
    <attribute name='jarvis_gopid' />
    <attribute name='jarvis_name' />
    <attribute name='createdon' />
    <attribute name='jarvis_totallimitoutcurrency' />
    <attribute name='jarvis_totallimitout' />
    <attribute name='jarvis_totallimitincurrency' />
    <attribute name='jarvis_totallimitin' />
    <attribute name='jarvis_repairingdealer' />
    <attribute name='modifiedon' />
    <attribute name='jarvis_gopreason' />
    <attribute name='jarvis_gopoutcurrency' />
    <attribute name='jarvis_goplimitout' />
    <attribute name='jarvis_goplimitin' />
    <attribute name='jarvis_gopincurrency' />
    <attribute name='jarvis_dealer' />
    <order attribute='modifiedon' descending='true' />
    <filter type='and'>
      <condition attribute='jarvis_relatedgop' operator='eq'  uitype='jarvis_gop' value='{0}' />
      <condition attribute='statecode' operator='eq' value='0' />
      <condition attribute='jarvis_incident' operator='eq' uitype='incident' value='{1}' />
    </filter>
  </entity>
</fetch>";

            /// <summary>
            /// // Get Active Cases.
            /// </summary>
            public const string geRActiveCases = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='incident'>
    <attribute name='customerid' />
    <attribute name='jarvis_homedealer' />
    <attribute name='jarvis_country' />
    <attribute name='casetypecode' />
    <attribute name='jarvis_caseserviceline' />
    <attribute name='jarvis_caselocation' />
    <attribute name='incidentid' />
    <attribute name='caseorigincode' />
    <order attribute='title' descending='false' />
    <filter type='and'>
      <condition attribute='statecode' operator='eq' value='0' />
    </filter>
  </entity>
</fetch>";

            /// <summary>
            /// // Get Jarvis Url from integration Config.
            /// </summary>
            public const string getConfigJarvisUrl = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>
<entity name='jarvis_integrationconfiguration'>
<attribute name='jarvis_integrationconfigurationid'/>
<attribute name='jarvis_integrationname'/>
<attribute name='jarvis_integrationcode'/>
<attribute name='jarvis_integrationmapping'/>
<order attribute='jarvis_integrationname' descending='false'/>
<filter type='and'>
<condition attribute='jarvis_integrationcode' operator='eq' value='JARVISURL'/>
</filter>
</entity>
</fetch>";

            /// <summary>
            /// // Get Customer data related to Case.
            /// </summary>
            public const string getCaseCustomerdata = @"<fetch>
	<entity name='incident'>
		<attribute name='incidentid' />
			<attribute name='jarvis_preferredvasoperator' />
	<attribute name='title' />
		<filter type='and'>
			<condition attribute='incidentid' operator='eq' value='{0}'/>
		</filter>
		<link-entity name='account' from='accountid' to='customerid' alias='Case2Account' link-type='outer'>
			<attribute name='accountid' />
			<attribute name='name' />
		</link-entity>
	</entity>
</fetch>";

            /// <summary>
            /// get Incident Nature For Case Sub grid.
            /// </summary>
            public const string getIncidentNatureForCaseSubgrid = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='jarvis_incidentnature'>
    <attribute name='jarvis_incidentnatureid' />
    <attribute name='jarvis_name' />
    <attribute name='createdon' />
    <attribute name='jarvis_incidenttype' />
    <attribute name='jarvis_description' />
    <attribute name='jarvis_incident' />
    <order attribute='jarvis_name' descending='false' />
    <filter type='and'>
      <condition attribute='jarvis_incident' operator='eq' value='{0}'/>
      <condition attribute='statecode' operator='eq' value='0' />
    </filter>
  </entity>
</fetch>";

            /// <summary>
            ///   // Get Has Been Sent JEDS.
            /// </summary>
            public const string getHasBeenSentJEDs = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='jarvis_jobenddetails'>
    <attribute name='jarvis_name' />
    <attribute name='createdon' />
    <order attribute='jarvis_name' descending='false' />
    <filter type='and'>
      <condition attribute='jarvis_incident' operator='eq' value='{0}' />
      <condition attribute='jarvis_repairingdealerpassout' operator='eq' value='{1}'/>
      <condition attribute='statuscode' operator='eq' value='334030002' />
    </filter>
  </entity>
</fetch>";

            /// <summary>
            ///   // Get Completed Emails for Case.
            /// </summary>
            public const string getCaseEmailsCompleted = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='email'>
    <attribute name='subject' />
    <attribute name='regardingobjectid' />
    <attribute name='from' />
    <attribute name='to' />
    <attribute name='prioritycode' />
    <attribute name='statuscode' />
    <attribute name='modifiedon' />
    <attribute name='activityid' />
    <order attribute='subject' descending='false' />
    <filter type='and'>
      <condition attribute='jarvis_relatedjobenddetail' operator='eq' value='{0}'/>
      <condition attribute='statuscode' operator='in'>
        <value>3</value>
        <value>6</value>
      </condition>
    </filter>
  </entity>
</fetch>";

            /// <summary>
            ///   // Get Completed Emails for Case.
            /// </summary>
            public const string getCaseEmailsNotCompleted = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='email'>
     <attribute name='createdon' />
    <order attribute='createdon' descending='false' />
    <filter type='and'>
      <condition attribute='regardingobjectid' operator='eq' value='{0}' />
      <condition attribute='statuscode' operator='eq' value='1' />
    </filter>
  </entity>
</fetch>";

            /// <summary>
            ///   // Get User Id based on name to send notification.
            /// </summary>
            public const string getAllVasOperatorIds = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>
<entity name='jarvis_casepreferredagent'>
<attribute name='jarvis_preferredvasoperator' distinct='true'/>
<link-entity name='incident' from='incidentid' to='jarvis_case' link-type='inner' alias='ab'>
<filter type='and'>
<condition attribute='incidentid' operator='eq' uiname='' uitype='incident' value='{0}'/>
</filter>
</link-entity>
</entity>
</fetch>";

            /// <summary>
            /// get Automation Config.
            /// </summary>
            public const string getAutomationConfigs = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='jarvis_configurationjarvis'>
    <attribute name='jarvis_configurationjarvisid' />
    <attribute name='jarvis_name' />
    <attribute name='jarvis_communicationpassoutconfirmation' />
    <attribute name='jarvis_communicationjobenddetails' />
    <attribute name='jarvis_communicationgopconfirmation' />
    <attribute name='jarvis_communicationetc' />
    <attribute name='jarvis_communicationeta' />
    <attribute name='jarvis_communicationcustomercommitmentcodes' />
    <attribute name='jarvis_automationwhitelistapproval' />
    <attribute name='jarvis_automationtranslation' />
    <attribute name='jarvis_automationreleasecase' />
    <attribute name='jarvis_automationmonitoraction' />
    <attribute name='jarvis_automationinitialgop' />
    <attribute name='jarvis_automationgop' />
    <attribute name='jarvis_automationcasestatuschange' />
    <attribute name='jarvis_automationavailableamount' />
    <attribute name='jarvis_volvopayautomation' />
    <attribute name='jarvis_caseopeningvalidation' />
    <order attribute='jarvis_name' descending='false' />
    <filter type='and'>
      <condition attribute='statuscode' operator='eq' value='1' />
    </filter>
  </entity>
</fetch>";

            /// <summary>
            /// // Fetch all Gop of a Case with particular home dealer.
            /// </summary>
            public const string deactivateGOPsForDealer = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='jarvis_gop'>
    <attribute name='jarvis_gopid' />
    <attribute name='jarvis_name' />
    <attribute name='jarvis_name' />
    <attribute name='jarvis_paymenttype' />
    <attribute name='jarvis_goplimitout' />
    <attribute name='jarvis_goplimitin' />
    <attribute name='jarvis_totallimitout' />
    <attribute name='jarvis_requesttype' />
    <attribute name='jarvis_approved' />
    <attribute name='jarvis_gopapproval' />
    <attribute name='jarvis_requestedcontact' />
    <attribute name='jarvis_totallimitoutcurrency' />
    <attribute name='jarvis_totallimitin' />
    <attribute name='jarvis_totallimitincurrency' />
    <attribute name='modifiedon' />
    <order attribute='modifiedon' descending='true' />
    <filter type='and'>
      <condition attribute='statecode' operator='eq' value='0' />
      <condition attribute='jarvis_incident' operator='eq' uitype='incident' value='{0}' />
      <condition attribute='jarvis_dealer' operator='eq' uitype='account' value='{1}' />
    </filter>
  </entity>
</fetch>";

            /// <summary>
            /// Case All Pass outs - Fetch All active and inactive Pass Outs of Case.
            /// </summary>
            public const string CaseAllPassouts = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='jarvis_passout'>
    <attribute name='jarvis_passoutid' />
    <attribute name='jarvis_name' />
    <attribute name='jarvis_goplimitout' />
    <attribute name='transactioncurrencyid' />
    <attribute name='modifiedon' />
    <order attribute='modifiedon' descending='true' />
    <filter type='and'>
      <condition attribute='jarvis_incident' operator='eq' uitype='incident' value='{0}' />
    </filter>
  </entity>
</fetch>";

            /// <summary>
            /// CaseAllGOPs - Fetch All active and inactive PassOuts of Case.
            /// </summary>
            public const string CaseAllGOPs = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='jarvis_gop'>
    <attribute name='jarvis_gopid' />
    <attribute name='jarvis_name' />
    <attribute name='jarvis_requesttype' />
    <attribute name='jarvis_gopoutcurrency' />
    <attribute name='jarvis_gopincurrency' /> 
    <attribute name='modifiedon' />
    <order attribute='modifiedon' descending='true' />
    <filter type='and'>
      <condition attribute='jarvis_incident' operator='eq' value='{0}' />
    </filter>
  </entity>
</fetch>";

            /// <summary>
            /// Currency Mapping - Retrieve active currency Mapping.
            /// </summary>
            public const string CurrencyMapping = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' no-lock='true' distinct='false'>
  <entity name='jarvis_currencymapping'>
    <attribute name='jarvis_currencymappingid' />
    <attribute name='jarvis_name' />
    <attribute name='createdon' />
    <attribute name='jarvis_vassupportedcurrency' />
    <order attribute='jarvis_name' descending='false' />
    <filter type='and'>
      <condition attribute='jarvis_currency' operator='eq' value='{0}' />
      <condition attribute='statecode' operator='eq' value='0' />
    </filter>
  </entity>
</fetch>";

            /// <summary>
            /// DefaultEuroCurrency - Get the default Euro Currency.
            /// </summary>
            public const string DefaultEuroCurrency = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' no-lock='true' distinct='false'>
  <entity name='transactioncurrency'>
    <attribute name='transactioncurrencyid' />
    <attribute name='currencyname' />
    <attribute name='isocurrencycode' />
    <attribute name='currencysymbol' />
    <attribute name='exchangerate' />
    <attribute name='currencyprecision' />
    <order attribute='currencyname' descending='false' />
    <filter type='and'>
      <condition attribute='isocurrencycode' operator='eq' value='EUR' />
      <condition attribute='statecode' operator='eq' value='0' />
    </filter>
  </entity>
</fetch>";

            /// <summary>
            ///  //Fetch Staircase fee.
            /// </summary>
            public const string GetStaircaseFee = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='jarvis_staircasefee'>
    <attribute name='jarvis_staircasefeeid' />
    <attribute name='jarvis_name' />
    <attribute name='createdon' />
    <attribute name='jarvis_staircasefee' />
    <attribute name='transactioncurrencyid' />
    <order attribute='jarvis_name' descending='false' />
    <filter type='and'>
      <condition attribute='jarvis_country' operator='eq' uitype='jarvis_country' value='{0}' />
      <condition attribute='jarvis_serviceline' operator='eq' uitype='jarvis_serviceline' value='{1}' />
      <condition attribute='statecode' operator='eq' value='0' />
    </filter>
  </entity>
</fetch>";

            /// <summary>
            /// Fetch xml for Current Monitor Actions.
            /// </summary>
            public const string GetCurrentMonitorActions = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' no-lock='true' distinct='false'>
  <entity name='jarvis_casemonitoraction'>
    <attribute name='activityid' />
    <attribute name='subject' />
    <attribute name='createdon' />
    <attribute name='prioritycode' />
    <attribute name='jarvis_monitorsortorder' />
    <attribute name='jarvis_followuptime' />
    <attribute name='jarvis_followuplanguage' />
    <attribute name='jarvis_followupcountry' />
    <attribute name='regardingobjectid' />
    <attribute name='actualstart' />
    <attribute name='actualend' />
    <attribute name='createdby' />
    <attribute name='jarvis_followuptimestamp' />
    <order attribute='jarvis_monitorsortorder' descending='false' />
    <order attribute='prioritycode' descending='false' />
    <order attribute='jarvis_followuptimestamp' descending='false' />
    <filter type='and'>
       <condition attribute='regardingobjectid' operator='eq' value='{0}'/>
       <condition attribute='statecode' operator='eq' value='0' />
       <condition attribute='statuscode' operator='eq' value='1' />
       <condition attribute='jarvis_monitorcurrentupcoming' operator='like' value='%Current%' />
    </filter>
  </entity>
</fetch>";

            /// <summary>
            /// Fetch xml for Upcoming Monitor Actions.
            /// </summary>
            public const string GetUpcomingMonitorActions = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' no-lock='true' distinct='false'>
  <entity name='jarvis_casemonitoraction'>
    <attribute name='activityid' />
    <attribute name='subject' />
    <attribute name='createdon' />
    <attribute name='prioritycode' />
    <attribute name='jarvis_monitorsortorder' />
    <attribute name='jarvis_followuptime' />
    <attribute name='jarvis_followuplanguage' />
    <attribute name='jarvis_followupcountry' />
    <attribute name='regardingobjectid' />
    <attribute name='actualstart' />
    <attribute name='actualend' />
    <attribute name='createdby' />
    <attribute name='jarvis_followuptimestamp' />
    <order attribute='jarvis_monitorsortorder' descending='false' />
    <order attribute='prioritycode' descending='false' />
    <order attribute='jarvis_followuptimestamp' descending='false' />
    <filter type='and'>
       <condition attribute='regardingobjectid' operator='eq' value='{0}'/>
       <condition attribute='statecode' operator='eq' value='0' />
       <condition attribute='statuscode' operator='eq' value='1' />
       <condition attribute='jarvis_monitorcurrentupcoming' operator='like' value='%Upcoming%' />
    </filter>
  </entity>
</fetch>";

            /// <summary>
            /// Get Default YYY Language.
            /// </summary>
            public const string GetYYYLanguage = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' no-lock='true' distinct='false'>
  <entity name='jarvis_language'>
    <attribute name='jarvis_languageid' />
    <attribute name='jarvis_name' />
    <order attribute='jarvis_name' descending='false' />
    <filter type='and'>
      <condition attribute='jarvis_iso3languagecode6392t' operator='eq' value='YYY' />
      <condition attribute='statecode' operator='eq' value='0' />
    </filter>
  </entity>
</fetch>";

            /// <summary>
            /// Get Default YY Country.
            /// </summary>
            public const string GetYYCountry = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' no-lock='true' distinct='false'>
  <entity name='jarvis_country'>
    <attribute name='jarvis_countryid' />
    <attribute name='jarvis_name' />
    <attribute name='createdon' />
    <order attribute='jarvis_name' descending='false' />
    <filter type='and'>
      <condition attribute='statecode' operator='eq' value='0' />
      <condition attribute='jarvis_iso2countrycode' operator='eq' value='YY' />
    </filter>
  </entity>
</fetch>";


            /// <summary>
            /// //Get Dealer Not Found.
            /// </summary>
            public const string GetDealerNotFound = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
        <entity name='account'>
		<attribute name='name' />
		<attribute name='accountid' />
		<!-- Filter By -->
		<filter type='and'>
			<condition attribute='jarvis_responsableunitid' operator='like' value='%dummy%' />
		</filter>
	</entity>
</fetch>";

            /// <summary>
            ///  // Get Case  Vehicle Integration Status.
            /// </summary>
            public const string GetCaseVehicleIntegrationStatus = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='jarvis_casevehicledataintegrationstatus'>
    <attribute name='jarvis_vehiclerecordtype' />
    <attribute name='jarvis_progressstatus' />
   <order attribute='jarvis_vehiclerecordtype' descending='false' />
    <filter type='and'>
      <condition attribute='statecode' operator='eq' value='0' />
      <condition attribute='jarvis_case' operator='eq' uitype='incident' value='{0}' />
      <condition attribute='jarvis_progressstatus' operator='in'>
        <value>334030000</value>
        <value>334030001</value>
      </condition>
    </filter>
  </entity>
</fetch>";
        }

        /// <summary>
        /// Attributes Jarvis.
        /// </summary>
        public class AttributesJarvis
        {
            /// <summary>
            /// Gets or sets Id.
            /// </summary>
            public string Id { get; set; }

            /// <summary>
            /// Gets or sets TransType.
            /// </summary>
            public string TransType { get; set; }

            /// <summary>
            /// Gets or sets EntityData.
            /// </summary>
            public string EntityData { get; set; }
        }

        /// <summary>
        /// Trans Attributes Jarvis.
        /// </summary>
        public class TransAttributesJarvis
        {
            /// <summary>
            /// Gets or sets Id.
            /// </summary>
            public string Id { get; set; }

            /// <summary>
            /// Gets or sets TransType.
            /// </summary>
            public string TransType { get; set; }

            /// <summary>
            /// Gets or sets EntityData.
            /// </summary>
            public string EntityData { get; set; }

            /// <summary>
            /// Gets or sets TranslationType.
            /// </summary>
            public string TranslationType { get; set; }
        }

        /// <summary>
        /// Gop Jarvis.
        /// </summary>
        public class GopJarvis
        {
            /// <summary>
            /// Gets or sets IncidentId.
            /// </summary>
            public string IncidentId { get; set; }
        }
    }
}
