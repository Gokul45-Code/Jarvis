// <copyright file="Constants.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace MCS.Jarvis.CE.Plugins
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics;
    using System.IdentityModel.Metadata;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Microsoft.Xrm.Sdk;

    /// <summary>
    /// Constants class.
    /// </summary>
    public class Constants
    {
        /// <summary>
        /// Incident class.
        /// </summary>
        public class Incident
        {
            /// <summary>
            /// BPF Stage Name.
            /// </summary>
            public const string BpfStageName = "VASBreakDownConfiguration";

            /// <summary>
            /// search API.
            /// </summary>
            public const string SearchAPI = "VASSearchAPI";

            /// <summary>
            /// BPF Stage1.
            /// </summary>
            public const string BpfStage1 = "Case Opening";

            /// <summary>
            /// BPF Stage2.
            /// </summary>
            public const string BpfStage2 = "Guarantee Of Payment";

            /// <summary>
            /// BPF Stage3.
            /// </summary>
            public const string BpfStage3 = "Pass Out";

            /// <summary>
            /// BPF Stage4.
            /// </summary>
            public const string BpfStage4 = "ETA Technician";

            /// <summary>
            /// BPF Stage5.
            /// </summary>
            public const string BpfStage5 = "Waiting For Repair Start";

            /// <summary>
            /// BPF Stage6.
            /// </summary>
            public const string BpfStage6 = "Repair Ongoing";

            /// <summary>
            /// BPF Stage7.
            /// </summary>
            public const string BpfStage7 = "Repair Finished";

            /// <summary>
            /// BPF Stage8.
            /// </summary>
            public const string BpfStage8 = "Repair Summary";

            /// <summary>
            /// BPF Stage9.
            /// </summary>
            public const string BpfStage9 = "Case Closure";

            /// <summary>
            /// BPF Stage10.
            /// </summary>
            public const string BpfStage10 = "Ongoing";

            /// <summary>
            /// BPF Stage11.
            /// </summary>
            public const string BpfStage11 = "Solved";

            /// <summary>
            /// BPF Stage12.
            /// </summary>
            public const string BpfStage12 = "Credit To HD";

            /// <summary>
            /// BPF Stage13.
            /// </summary>
            public const string BpfStage13 = "Closed";

            /// <summary>
            /// incident attribute.
            /// </summary>
            public const string IncidentValue = "incident";

            /// <summary>
            /// incident Id.
            /// </summary>
            public const string IncidentId = "incidentid";

            /// <summary>
            /// jarvis HD RD.
            /// </summary>
            public const string JarvisHDRD = "jarvis_hdrd";

            /// <summary>
            /// case RD jarvis Repairing dealer.
            /// </summary>
            public const string CaseRDjarvisRepairingdealer = "CaseRD.jarvis_repairingdealer";

            /// <summary>
            /// jarvis Home dealer.
            /// </summary>
            public const string JarvisHomedealer = "jarvis_homedealer";
        }

        /// <summary>
        /// Case Contact.
        /// </summary>
        public class Casecontact
        {
            /// <summary>
            /// jarvis First name.
            /// </summary>
            public const string JarvisFirstname = "jarvis_firstname";

            /// <summary>
            /// jarvis Last name.
            /// </summary>
            public const string JarvisLastname = "jarvis_lastname";

            /// <summary>
            /// jarvis Mobile phone.
            /// </summary>
            public const string JarvisMobilephone = "jarvis_mobilephone";

            /// <summary>
            /// jarvis Role.
            /// </summary>
            public const string JarvisRole = "jarvis_role";

            /// <summary>
            /// jarvis Preferred language.
            /// </summary>
            public const string JarvisPreferredlanguage = "jarvis_preferredlanguage";

            /// <summary>
            /// jarvis Case contact type.
            /// </summary>
            public const string JarvisCasecontacttype = "jarvis_casecontacttype";

            /// <summary>
            /// jarvis Preferred method of contact.
            /// </summary>
            public const string JarvisPreferredmethodofcontact = "jarvis_preferredmethodofcontact";

            /// <summary>
            /// jarvis Incident.
            /// </summary>
            public const string JarvisIncident = "jarvis_incident";

            /// <summary>
            /// jarvis Case contact id.
            /// </summary>
            public const string JarvisCasecontactid = "jarvis_casecontactid";

            /// <summary>
            /// jarvis Case contact.
            /// </summary>
            public const string JarvisCasecontact = "jarvis_casecontact";
        }

        /// <summary>
        /// Notification Data.
        /// </summary>
        public class NotificationData
        {
            /// <summary>
            /// monitor Action Complete.
            /// </summary>
            public const string MonitorActionComplete = "Monitor Action Completed";

            /// <summary>
            /// monitor Action Create.
            /// </summary>
            public const string MonitorActionCreate = "Monitor Action Created";

            /// <summary>
            /// status Changed.
            /// </summary>
            public const string StatusChanged = "Case stage changed";

            /// <summary>
            /// preferred Agent Added.
            /// </summary>
            public const string PreferredAgentAdded = "Preferred Agent added";

            /// <summary>
            /// preferred Agent Added body.
            /// </summary>
            public const string PreferredAgentAddedbody = "You’re now a preferred Agent for Case";

            /// <summary>
            /// preferred Agent Removed.
            /// </summary>
            public const string PreferredAgentRemoved = "Preferred Agent Removed";

            /// <summary>
            /// preferred Agent Removed body.
            /// </summary>
            public const string PreferredAgentRemovedbody = "You’ve been removed as a preferred Agent from Case";

            /// <summary>
            /// case Opening to GOP.
            /// </summary>
            public const string CaseOpneing2Gop = "Case changed from 'Case Opening' to 'Guarantee of Payment'";

            /// <summary>
            /// GOP to Pass out.
            /// </summary>
            public const string Gop2PassOut = "Case changed from 'Guarantee of Payment' to 'Pass Out'";

            /// <summary>
            /// status Pass out to ETA Technician body.
            /// </summary>
            public const string StatusPassout2ETATechnicianbody = "Case changed from 'Pass Out' to 'ETA Technician'";

            /// <summary>
            /// eta Technician to Waiting for repair start.
            /// </summary>
            public const string EtaTechnician2Waitingforrepairstart = "Case changed from 'ETA Technician' to 'Waiting For Repair Start'";

            /// <summary>
            /// waiting for repair start to Repair ongoing.
            /// </summary>
            public const string Waitingforrepairstart2Repairongoing = "Case changed from 'Waiting For Repair Start' to 'Repair Ongoing'";

            /// <summary>
            /// repair Ongoing to Repair finished.
            /// </summary>
            public const string RepairOngoing2Repairfinished = "Case changed from 'Repair Ongoing' to 'Repair Finished'";

            /// <summary>
            /// repair finished to Repair summary.
            /// </summary>
            public const string Repairfinished2Repairsummary = "Case changed from 'Repair Finished' to 'Repair Summary'";

            /// <summary>
            /// repair summary to Case closure.
            /// </summary>
            public const string Repairsummary2Caseclosure = "Case changed from 'Repair Summary' to 'Case closure'";
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
            /// pass Out Delayed ETA.
            /// </summary>
            public const string PassOutDelayedETA = "Jarvis.Event.delayedETAUpdate";
        }

        /// <summary>
        /// Jarvis Configuration.
        /// </summary>
        public class JarvisConfiguration
        {
            /// <summary>
            /// automation monitor action.
            /// </summary>
            public const string Automationmonitoraction = "jarvis_automationmonitoraction";

            /// <summary>
            /// communication GOP confirmation.
            /// </summary>
            public const string Communicationgopconfirmation = "jarvis_communicationgopconfirmation";

            /// <summary>
            /// communication pass out confirmation.
            /// </summary>
            public const string Communicationpassoutconfirmation = "jarvis_communicationpassoutconfirmation";

            /// <summary>
            /// communication eta.
            /// </summary>
            public const string Communicationeta = "jarvis_communicationeta";

            /// <summary>
            /// communication etc.
            /// </summary>
            public const string Communicationetc = "jarvis_communicationetc";

            /// <summary>
            /// communication job end details.
            /// </summary>
            public const string Communicationjobenddetails = "jarvis_communicationjobenddetails";

            /// <summary>
            /// communication customer commitment codes.
            /// </summary>
            public const string Communicationcustomercommitmentcodes = "jarvis_communicationcustomercommitmentcodes";

            /// <summary>
            /// jarvis automation translation.
            /// </summary>
            public const string Automationtranslation = "jarvis_automationtranslation";

            /// <summary>
            /// automation case status change.
            /// </summary>
            public const string Automationcasestatuschange = "jarvis_automationcasestatuschange";

            /// <summary>
            /// automation initial GOP.
            /// </summary>
            public const string Automationinitialgop = "jarvis_automationinitialgop";

            /// <summary>
            /// automation available amount.
            /// </summary>
            public const string Automationavailableamount = "jarvis_automationavailableamount";

            /// <summary>
            /// automation GOP.
            /// </summary>
            public const string Automationgop = "jarvis_automationgop";

            /// <summary>
            /// automation whitelist approval.
            /// </summary>
            public const string Automationwhitelistapproval = "jarvis_automationwhitelistapproval";

            /// <summary>
            ///  // Release Case.
            /// </summary>
            public const string Automationreleasecase = "jarvis_automationreleasecase";
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
        /// Team class.
        /// </summary>
        public class Team
        {
            /// <summary>
            /// team membership.
            /// </summary>
            public const string teammembership = "teammembership_association";
        }

        /// <summary>
        /// Incident Nature.
        /// </summary>
        public class IncidentNature
        {
            /// <summary>
            /// jarvis Incident jarvis Incident Nature jarvis.
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
            /// Incident Incident Nature jarvis Incident nature id.
            /// </summary>
            public const string IncidentIncidentNature_jarvisIincidentnatureid = "Incident_IncidentNature.jarvis_incidentnatureid";

            /// <summary>
            /// jarvis Incident jarvis Incident Nature Sub grid.
            /// </summary>
            public const string jarvisIncidentjarvisIncidentNatureSubgrid = "jarvis_IncidentNature_Incident_Incident";
        }

        /// <summary>
        /// Preferred Agent.
        /// </summary>
        public class PreferredAgent
        {
            /// <summary>
            /// jarvis Preferred VasOperator.
            /// </summary>
            public const string jarvisPreferredVasOperator = "jarvis_preferredvasoperator";

            /// <summary>
            /// jarvis Case Preferred Agent Id.
            /// </summary>
            public const string jarvisCasePreferredAgentId = "jarvis_casepreferredagentid";
        }

        /// <summary>
        /// Class to Access FetchXml.
        /// </summary>
        public class FetchXmls
        {
            /// <summary>
            /// get Mileage Units.
            /// </summary>
            public const string getMileageUnits = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='jarvis_mileage'>
    <attribute name='jarvis_mileageid' />
    <attribute name='jarvis_name' />
    <order attribute='jarvis_name' descending='false' />
    <filter type='and'>
      <condition attribute='statecode' operator='eq' value='0' />
    </filter>
  </entity>
</fetch>";

            /// <summary>
            ///  //Retrieve PassOuts .
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
            /// //Calculate Total Approved Amount for GOP.
            /// </summary>
            public const string calculateTotalAmntAppr = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false' aggregate='true'>
  <entity name='jarvis_gop'>
    <attribute name='jarvis_goplimitout' alias='goplimitout' aggregate='sum' /> 
    <filter type='and'>
<condition attribute='statecode' operator='eq' value='0' />
      <condition attribute='jarvis_incident' operator='eq' value='{0}'/>
      <condition attribute='jarvis_approved' operator='eq' value='1' />
    </filter>
  </entity>
</fetch>";

            /// <summary>
            /// get Active Actions.
            /// </summary>
            public const string getActiveActions = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='jarvis_casemonitoraction'>
    <attribute name='activityid' />
    <attribute name='subject' />
    <attribute name='createdon' />
    <order attribute='subject' descending='false' />
    <filter type='and'>
      <condition attribute='regardingobjectid' operator='eq' value='{0}'/>
      <condition attribute='statuscode' operator='eq' value='1' />
    </filter>
  </entity>
</fetch>";

            /// <summary>
            /// //Calculate Total Approved Amount for Pass Outs.
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
            ///  //Get VAS Configuration.
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
            ///  //Get Exchange Rate.
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
            ///    //Get Work Type for Work Order creation.
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
            ///  //Get Business Process Name.
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
            ///  //Get Business Process For Case.
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
            /// //Get BPF Instance for Case.
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
            /// //Get PriceList for WO.
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
            /// //Get Work Orders.
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
            /// //Get Brand Incident Types.
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
            ///  //Get Incident Nature For WO.
            /// </summary>
            public const string getIncidentNatureForCase = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>
  <entity name='jarvis_incidentnature'>
    <attribute name='jarvis_incidentnatureid' />
    <attribute name='jarvis_name' />
    <attribute name='createdon' />
    <attribute name='jarvis_incidenttype' />
    <order attribute='jarvis_name' descending='false' />
    <filter type='and'>
      <condition attribute='jarvis_incidenttype' operator='not-null' />
    </filter>
    <link-entity name='jarvis_incident_jarvis_incidentnature' from='jarvis_incidentnatureid' to='jarvis_incidentnatureid' visible='false' intersect='true'>
      <link-entity name='incident' from='incidentid' to='incidentid' alias='ai'>
        <filter type='and'>
          <condition attribute='incidentid' operator='eq' value='{0}'/>
        </filter>
      </link-entity>
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
            /// //Get Incident Nature For Delete.
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
            ///  //Get WO Requirement.
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
            ///  //Get Resource Characteristics.
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
            /// //Get Bookable Resource Characteristics.
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
            /// //Get Search Availability API.
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
            /// //Get All Pass out.
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
            ///  //Get All Active Pass out.
            /// </summary>
            public const string CaseActivePassouts = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='jarvis_passout'>
    <attribute name='jarvis_passoutid' />
    <attribute name='jarvis_name' />
    <attribute name='createdon' />
    <attribute name='jarvis_repairingdealer' />
    <attribute name='jarvis_goplimitout' />
    <attribute name='transactioncurrencyid' />
    <attribute name='jarvis_etadate' />
    <attribute name='jarvis_etatime' />
    <attribute name='jarvis_atcdate' />
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
            /// Case All Pass out.
            /// </summary>
            public const string CaseAllPassouts = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='jarvis_passout'>
    <attribute name='jarvis_passoutid' />
    <attribute name='jarvis_name' />
    <attribute name='createdon' />
    <attribute name='jarvis_repairingdealer' />
    <attribute name='jarvis_goplimitout' />
    <attribute name='transactioncurrencyid' />
    <attribute name='jarvis_atadate' />
    <attribute name='jarvis_atatime' />
    <attribute name='jarvis_atcdate' />
    <attribute name='modifiedon' />
    <order attribute='modifiedon' descending='true' />
    <filter type='and'>
      <condition attribute='statecode' operator='eq' value='0' />
      <condition attribute='jarvis_incident' operator='eq' uitype='incident' value='{0}' />
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
      <condition attribute='jarvis_incident' operator='eq' uitype='incident' value='{0}' />
      <condition attribute='jarvis_repairingdealer' operator='eq' uitype='jarvis_passout' value='{1}' />
    </filter>
  </entity>
</fetch>";

            /// <summary>
            /// //Fetch Last Modified GOP for Query.
            /// </summary>
            public const string getLastModGOP = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='jarvis_gop'>
    <attribute name='jarvis_gopid' />
    <attribute name='jarvis_name' />
    <attribute name='createdon' />
    <attribute name='modifiedon' />
    <attribute name='jarvis_paymenttype' />
    <order attribute='modifiedon' descending='true' />
    <filter type='and'>
      <condition attribute='jarvis_approved' operator='eq' value='1' />
      <condition attribute='jarvis_incident' operator='eq' value='{0}'/>
      <condition attribute='jarvis_relatedgop' operator='null' />
     </filter>
  </entity>
</fetch>";

            /// <summary>
            ///  //Fetch Resolution.
            /// </summary>
            public const string getCaseResolution = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
    <entity name='incidentresolution'>
    <attribute name='resolutiontypecode'/>
    <attribute name='createdon'/>
    <attribute name='modifiedon'/>
    <order attribute='modifiedon' descending='true' />
    <filter type='and'>
      <condition attribute='incidentid' operator='eq' value='{0}'/>
    </filter>
  </entity>
</fetch>";

            /// <summary>
            /// //Fetch Label Translation.
            /// </summary>
            public const string getLabelTranslation = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='jarvis_communicationlabeltranslation'>
    <attribute name='jarvis_communicationlabeltranslationid' />
    <attribute name='jarvis_name' />
    <attribute name='createdon' />
    <order attribute='jarvis_name' descending='false' />
    <filter type='and'>
      <condition attribute='jarvis_language' operator='eq' value='{0}'/>
      <condition attribute='jarvis_communicationplaceholder' operator='eq' value='{1}'/>
    </filter>
  </entity>
</fetch>";

            /// <summary>
            ///  //Fetch Case Translation.
            /// </summary>
            public const string getCaseTranslation = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='jarvis_casetranslation'>
    <attribute name='jarvis_casetranslationid' />
    <attribute name='jarvis_name' />
    <attribute name='createdon' />
    <attribute name='jarvis_location' />
    <attribute name='jarvis_description' />
    <attribute name='jarvis_customerexpectations' />
    <order attribute='jarvis_name' descending='false' />
    <filter type='and'>
      <condition attribute='jarvis_incident' operator='eq' value='{0}'/>
      <condition attribute='jarvis_language' operator='eq' value='{1}'/>
    </filter>
  </entity>
</fetch>";

            /// <summary>
            /// //Fetch Monitor Actions.
            /// </summary>
            public const string getMonitorActions = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='jarvis_casemonitoraction'>
    <attribute name='activityid' />
    <attribute name='subject' />
    <attribute name='createdon' />
    <attribute name='prioritycode' />
    <attribute name='jarvis_monitorsortorder' />
    <attribute name='jarvis_followuptime' />
    <attribute name='jarvis_followuplanguage' />
    <attribute name='jarvis_followupcountry' />
    <attribute name='actualstart' />
    <attribute name='createdby' />
    <order attribute='jarvis_monitorsortorder' descending='false' />
    <order attribute='actualstart' descending='false' />
    <filter type='and'>
      <condition attribute='regardingobjectid' operator='eq' uiname='' uitype='incident' value='{0}'/>
      <condition attribute='activityid' operator='ne' value='{1}'/>
      <condition attribute='statuscode' operator='eq' value='1' />
    </filter>
  </entity>
</fetch>";

            /// <summary>
            ///  //Fetch Latest GOP.
            /// </summary>
            public const string fetchGOP = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='jarvis_gop'>
    <attribute name='jarvis_gopid' />
    <attribute name='jarvis_name' />
    <attribute name='createdon' />
    <attribute name='jarvis_totallimitoutcurrency' />
    <attribute name='jarvis_totallimitout' />
    <attribute name='jarvis_totallimitincurrency' />
    <attribute name='jarvis_totallimitin' />
    <attribute name='jarvis_requestedcontact' />
    <attribute name='jarvis_requesttype' />
    <attribute name='jarvis_repairingdealer' />
    <attribute name='jarvis_paymenttype' />
    <attribute name='jarvis_gopreason' />
    <attribute name='jarvis_gopoutcurrency' />
    <attribute name='jarvis_goplimitout' />
    <attribute name='jarvis_goplimitin' />
    <attribute name='jarvis_gopincurrency' />
    <attribute name='jarvis_dealer' />
    <attribute name='jarvis_comment' />
    <attribute name='modifiedon' />
    <order attribute='jarvis_gopapprovaltime' descending='true' />
    <order attribute='modifiedon' descending='true'/>
    <filter type='and'>
      <condition attribute='jarvis_incident' operator='eq' value='{0}'/>
       <condition attribute='jarvis_gopapproval' operator='eq' value='334030001' />
    </filter>
  </entity>
</fetch>";

            /// <summary>
            /// //Fetch Vehicle Contracts.
            /// </summary>
            public const string getContracts = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='jarvis_vehiclecontract'>
    <attribute name='jarvis_vehiclecontractid' />
    <attribute name='jarvis_name' />
    <attribute name='createdon' />
    <attribute name='jarvis_vehicle' />
    <attribute name='jarvis_startmileage' />
    <attribute name='jarvis_startdate' />
    <attribute name='jarvis_mileageunit' />
    <attribute name='jarvis_maxmileage' />
    <attribute name='jarvis_expiringdate' />
    <attribute name='jarvis_description' />
    <attribute name='jarvis_country' />
    <attribute name='jarvis_contracttype' />
    <attribute name='statuscode' />
    <attribute name='jarvis_tsacontracttype' />
    <order attribute='jarvis_name' descending='false' />
    <filter type='and'>
      <condition attribute='jarvis_case' operator='eq' value='{0}'/>
      <condition attribute='statecode' operator='eq' value='0' />
    </filter>
  </entity>
</fetch>";

            /// <summary>
            /// //Fetch Vehicle Warranties.
            /// </summary>
            public const string getWarranties = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='jarvis_vehiclewarranty'>
    <attribute name='jarvis_vehiclewarrantyid' />
    <attribute name='jarvis_name' />
    <attribute name='createdon' />
    <attribute name='statuscode' />
    <attribute name='jarvis_warrantycode' />
    <attribute name='jarvis_vehicle' />
    <attribute name='jarvis_startdate' />
    <attribute name='jarvis_expirydate' />
    <attribute name='jarvis_description' />
    <attribute name='jarvis_case' />
    <order attribute='jarvis_name' descending='false' />
    <filter type='and'>
      <condition attribute='jarvis_case' operator='eq' value='{0}'/>
      <condition attribute='statuscode' operator='eq' value='1' />
    </filter>
  </entity>
</fetch>";

            /// <summary>
            ///   //Fetch Case Soft Offers.
            /// </summary>
            public const string getSoftOffers = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='jarvis_casesoftoffer'>
    <attribute name='jarvis_casesoftofferid' />
    <attribute name='jarvis_name' />
    <attribute name='jarvis_contractno' />
    <attribute name='jarvis_description' />
    <attribute name='jarvis_startdate' />
    <attribute name='jarvis_expirydate' />
    <attribute name='statuscode' />
    <attribute name='jarvis_softoffercodelookup' />
    <attribute name='jarvis_vehicle' />
    <attribute name='jarvis_marketcode' />
    <attribute name='jarvis_softoffercode' />
    <order attribute='jarvis_expirydate' descending='false' />
    <filter type='and'>
      <condition attribute='jarvis_case' operator='eq' value='{0}'/>
      <condition attribute='statecode' operator='eq' value='0' />
    </filter>
  </entity>
</fetch>";

            /// <summary>
            /// //Fetch Non ETC PassOuts.
            /// </summary>
            public const string getNonETCPassOuts = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='jarvis_passout'>
    <attribute name='jarvis_passoutid' />
    <attribute name='jarvis_name' />
    <attribute name='createdon' />
    <order attribute='jarvis_name' descending='false' />
    <filter type='and'>
      <condition attribute='jarvis_incident' operator='eq' value='{0}'/>
      <condition attribute='jarvis_etc' operator='null' />
    </filter>
  </entity>
</fetch>";

            /// <summary>
            /// //Fetch Non ATC PassOuts.
            /// </summary>
            public const string getNonATCPassOuts = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='jarvis_passout'>
    <attribute name='jarvis_passoutid' />
    <attribute name='jarvis_name' />
    <attribute name='createdon' />
    <order attribute='jarvis_name' descending='false' />
    <filter type='and'>
      <condition attribute='jarvis_incident' operator='eq' value='{0}'/>
      <condition attribute='jarvis_atc' operator='null' />
    </filter>
  </entity>
</fetch>";

            /// <summary>
            /// get Close Actions.
            /// </summary>
            public const string getCloseActions = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='jarvis_casemonitoraction'>
    <attribute name='activityid' />
    <attribute name='subject' />
    <attribute name='createdon' />
    <order attribute='subject' descending='false' />
    <filter type='and'>
      <condition attribute='regardingobjectid' operator='eq' value='{0}'/>
      <condition attribute='statuscode' operator='eq' value='1' />
      <condition attribute='subject' operator='like' value='%{1}%'/>
    </filter>
  </entity>
</fetch>";

            /// <summary>
            /// get Close Pass Out Actions.
            /// </summary>
            public const string getClosePassOutActions = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='jarvis_casemonitoraction'>
    <attribute name='activityid' />
    <attribute name='subject' />
    <attribute name='createdon' />
    <order attribute='subject' descending='false' />
    <filter type='and'>
      <condition attribute='regardingobjectid' operator='eq' value='{0}'/>
      <condition attribute='statuscode' operator='eq' value='2' />
      <condition attribute='subject' operator='like' value='%{1}%'/>
    </filter>
  </entity>
</fetch>";

            /// <summary>
            /// get Draft Pass Out Actions.
            /// </summary>
            public const string getDraftPassOutActions = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='jarvis_casemonitoraction'>
    <attribute name='activityid' />
    <attribute name='subject' />
    <attribute name='statuscode' />
    <order attribute='subject' descending='false' />
    <filter type='and'>
      <condition attribute='regardingobjectid' operator='eq' value='{0}'/>
      <condition attribute='subject' operator='like' value='%{1}%'/>
      <condition attribute='statuscode' operator='eq' value='1'/>
    </filter>
  </entity>
</fetch>";

            /// <summary>
            /// get Open Pass Out Actions.
            /// </summary>
            public const string getOpenPassOutActions = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='jarvis_casemonitoraction'>
    <attribute name='activityid' />
    <attribute name='subject' />
    <attribute name='statuscode' />
    <order attribute='subject' descending='false' />
    <filter type='and'>
      <condition attribute='regardingobjectid' operator='eq' value='{0}'/>
      <condition attribute='subject' operator='like' value='%{1}%'/>
      <condition attribute='statuscode' operator='eq' value='1'/>
    </filter>
  </entity>
</fetch>";

            /// <summary>
            /// //Get pass out Related to case.
            /// </summary>
            public const string getPassoutRD = @"<fetch>
	<entity name='incident'>
		<attribute name='incidentid' />
		<attribute name='title' />
        <attribute name='jarvis_hdrd' />
		<attribute name='jarvis_homedealer' />
        <attribute name='casetypecode' />
		<filter type='and'>
			<condition attribute='incidentid' operator='eq' value='{0}' />
		</filter>
		<link-entity name='jarvis_passout' from='jarvis_incident' to='incidentid' alias='CaseRD' link-type='outer'>
		<attribute name='jarvis_repairingdealer' />
<filter type='and'>
      <condition attribute='statecode' operator='eq' value='0' />
    </filter>
		</link-entity>
	</entity>
</fetch>";

            /// <summary>
            ///   //Get Related Country for MO.
            /// </summary>
            public const string getCountryMO = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='jarvis_country'>
    <attribute name='jarvis_countryid' />
    <attribute name='jarvis_name' />
    <attribute name='createdon' />
<attribute name='jarvis_iso2countrycode'/>
    <order attribute='jarvis_name' descending='false' />
    <filter type='and'>
      <condition attribute='jarvis_iso2countrycode' operator='eq' value='{0}'/>
    </filter>
  </entity>
</fetch>";

            /// <summary>
            ///  //Get Country-Language.
            /// </summary>
            public const string getCountryLanguage = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>
  <entity name='jarvis_language'>
    <attribute name='jarvis_languageid' />
    <attribute name='jarvis_name' />
    <attribute name='jarvis_iso2languagecode6391' />
<attribute name='jarvis_iso3languagecode6392t'/>
    <order attribute='jarvis_name' descending='false' />
<filter type='and'>
      <condition attribute='jarvis_vasstandardlanguage' operator='eq' value='1' />
    </filter>
    <link-entity name='jarvis_country_jarvis_language' from='jarvis_languageid' to='jarvis_languageid' visible='false' intersect='true'>
      <link-entity name='jarvis_country' from='jarvis_countryid' to='jarvis_countryid' alias='ac'>
        <filter type='and'>
          <condition attribute='jarvis_countryid' operator='eq' value='{0}'/>
        </filter>
      </link-entity>
    </link-entity>
  </entity>
</fetch>";

            /// <summary>
            /// getNonSteeringLanguageMO.
            /// </summary>
            public const string getNonSteeringLanguageMO = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='jarvis_language'>
    <attribute name='jarvis_languageid' />
    <attribute name='jarvis_name' />
    <attribute name='createdon' />
    <order attribute='jarvis_name' descending='false' />
    <filter type='and'>
      <condition attribute='jarvis_iso3languagecode6392t' operator='eq' value='{0}'/>
      <condition attribute='statecode' operator='eq' value='0' />
    </filter>
  </entity>
</fetch>";

            /// <summary>
            /// //Fetch Approved GOP for Monitor.
            /// </summary>
            public const string approvedHDGOP = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='jarvis_gop'>
    <attribute name='jarvis_gopid' />
    <attribute name='jarvis_name' />
    <attribute name='createdon' />
    <order attribute='jarvis_name' descending='false' />
    <filter type='and'>
      <condition attribute='jarvis_approved' operator='eq' value='1' />
      <condition attribute='jarvis_incident' operator='eq' value='{0}'/>
      <condition attribute='jarvis_requesttype' operator='eq' value='334030001' />
    </filter>
  </entity>
</fetch>";

            /// <summary>
            /// //Fetch Sent/Non ETA PassOuts.
            /// </summary>
            public const string getPassOutsETA = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='jarvis_passout'>
    <attribute name='jarvis_passoutid' />
    <attribute name='jarvis_name' />
    <attribute name='createdon' />
    <order attribute='jarvis_name' descending='false' />
    <filter type='and'>
      <condition attribute='jarvis_eta' operator='null' />
      <condition attribute='statuscode' operator='eq' value='334030001' />
      <condition attribute='jarvis_incident' operator='eq' value='{0}'/>
    </filter>
  </entity>
</fetch>";

            /// <summary>
            /// //Fetch Draft PassOuts.
            /// </summary>
            public const string getPassOutsDraft = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='jarvis_passout'>
    <attribute name='jarvis_passoutid' />
    <attribute name='jarvis_name' />
    <attribute name='createdon' />
    <order attribute='jarvis_name' descending='false' />
    <filter type='and'>
      <condition attribute='statuscode' operator='eq' value='1' />
      <condition attribute='jarvis_incident' operator='eq' value='{0}'/>
    </filter>
  </entity>
</fetch>";

            /// <summary>
            /// //Fetch JED Translations.
            /// </summary>
            public const string getJEDTranslations = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='jarvis_jobenddetailstranslation'>
    <attribute name='jarvis_jobenddetailstranslationid' />
    <attribute name='jarvis_name' />
    <attribute name='createdon' />
    <attribute name='jarvis_warrantyreason' />
    <attribute name='jarvis_temporaryrepair' />
    <attribute name='jarvis_comment' />
    <attribute name='jarvis_actualcausefault' />
    <order attribute='jarvis_name' descending='false' />
    <filter type='and'>
      <condition attribute='jarvis_case' operator='eq' value='{0}'/>
      <condition attribute='jarvis_jobenddetails' operator='eq' value='{1}'/>
      <condition attribute='jarvis_language' operator='eq' value='{2}'/>
    </filter>
  </entity>
</fetch>";

            /// <summary>
            ///   //Get RInfo Records older than a fiscal Value.
            /// </summary>
            public const string getRIOlderRecords = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='jarvis_repairinformation'>
    <attribute name='jarvis_repairinformationid' />
    <attribute name='jarvis_name' />
    <attribute name='createdon' />
    <order attribute='jarvis_name' descending='false' />
    <filter type='and'>
      <condition attribute='jarvis_incident' operator='eq' value='{0}'/>
      <condition attribute='createdon' operator='on-or-after' value='{1}'/>
    </filter>
  </entity>
</fetch>";

            /// <summary>
            /// //Check for Open Monitor Actions.
            /// </summary>
            public const string getOpenMO = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='jarvis_casemonitoraction'>
    <attribute name='activityid' />
    <attribute name='subject' />
    <attribute name='createdon' />
    <order attribute='subject' descending='false' />
    <filter type='and'>
      <condition attribute='regardingobjectid' operator='eq' value='{0}'/>
      <condition attribute='statecode' operator='eq' value='0' />
    </filter>
  </entity>
</fetch>";

            /// <summary>
            /// //Get Open PassOuts.
            /// </summary>
            public const string getOpenPassOuts = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='jarvis_passout'>
    <attribute name='jarvis_passoutid' />
    <attribute name='jarvis_name' />
    <attribute name='createdon' />
    <order attribute='jarvis_name' descending='false' />
    <filter type='and'>
      <filter type='or'>
        <condition attribute='jarvis_ata' operator='null' />
        <condition attribute='jarvis_atc' operator='null' />
      </filter>
      <condition attribute='jarvis_incident' operator='eq' value='{0}'/>
      <condition attribute='statuscode' operator='ne' value='2' />
    </filter>
  </entity>
</fetch>";

            /// <summary>
            /// //Get Open GOPs.
            /// </summary>
            public const string getUnApprovedGOP = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='jarvis_gop'>
    <attribute name='jarvis_gopid' />
    <attribute name='jarvis_name' />
    <attribute name='createdon' />
    <order attribute='jarvis_name' descending='false' />
    <filter type='and'>
      <condition attribute='jarvis_incident' operator='eq' value='{0}'/>
      <condition attribute='statecode' operator='ne' value='1' />
      <condition attribute='jarvis_relatedgop' operator='null' />
      <condition attribute='jarvis_gopapproval' operator='eq' value='334030000' />
    </filter>
  </entity>
</fetch>";

            /// <summary>
            /// get All Pass Out ETA Full filled.
            /// </summary>
            public const string getPassOutsETAFullfilled = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='jarvis_passout'>
    <attribute name='jarvis_passoutid' />
    <attribute name='jarvis_name' />
    <attribute name='createdon' />
    <order attribute='jarvis_name' descending='false' />
    <filter type='and'>
      <condition attribute='statuscode' operator='ne' value='2' />
      <condition attribute='jarvis_eta' operator='not-null' />
      <condition attribute='jarvis_incident' operator='eq' value='{0}'/>
    </filter>
  </entity>
</fetch>";

            /// <summary>
            /// get Info For Repair Information.
            /// </summary>
            public const string getInfoForRepairInformation = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='jarvis_passout'>
    <attribute name='jarvis_passoutid' />
    <attribute name='jarvis_name' />
    <attribute name='createdon' />
    <order attribute='jarvis_name' descending='false' />
    <filter type='and'>
      <condition attribute='statuscode' operator='ne' value='2' />
      <condition attribute='jarvis_etc' operator='null' />
      <condition attribute='jarvis_incident' operator='eq' value='{0}'/>
      <condition attribute='jarvis_passoutid' operator='eq' value='{1}'/>
    </filter>
  </entity>
</fetch>";

            /// <summary>
            /// get All Delayed Pass Out ETC.
            /// </summary>
            public const string getDelayedPassOutsETC = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='jarvis_repairinformation'>
    <attribute name='jarvis_repairinformationid' />
    <attribute name='jarvis_name' />
    <attribute name='createdon' />
    <order attribute='jarvis_name' descending='false' />
    <filter type='and'>
      <condition attribute='jarvis_incident' operator='eq' value='{0}'/>
      <condition attribute='jarvis_repairingdealerpassout' operator='eq' value='{1}'/>
    </filter>
  </entity>
</fetch>";

            // <summary>
            // RetrieveExistingAllRepairingDealersfortheCase
            // </summary>
            public const string getAllExistingRepairInfoRecords = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='jarvis_repairinformation'>
    <attribute name='jarvis_repairinformationid' />
    <attribute name='jarvis_name' />
    <attribute name='jarvis_towing' />
    <attribute name='createdon' />
    <order attribute='jarvis_name' descending='false' />
    <filter type='and'>
      <condition attribute='jarvis_incident' operator='eq' value='{0}'/>
    </filter>
  </entity>
</fetch>";

            // <summary>
            // RetrieveExistingAllRepairingDealersfortheCase
            // </summary>
            public const string getAllExistingjedsRecords = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='jarvis_jobenddetails'>
    <attribute name='jarvis_jobenddetailsid' />
    <attribute name='jarvis_name' />
    <attribute name='jarvis_towing' />
    <attribute name='createdon' />
    <order attribute='jarvis_name' descending='false' />
    <filter type='and'>
      <condition attribute='jarvis_incident' operator='eq' value='{0}'/>
    </filter>
  </entity>
</fetch>";

            // <summary>
            // RetrieveTowingInfoRecord
            // </summary>
            public const string getTowingRecordInfo = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name = 'jarvis_incidentnature' >
    <attribute name='jarvis_incidentnatureid' />
    <attribute name = 'jarvis_name' />
    <attribute name='createdon' />
    <order attribute = 'jarvis_name' descending='false' />
    <filter type = 'and' >
      <condition attribute='jarvis_name' operator='eq' value='Towing' />
    </filter>
  </entity>
</fetch>";

            /// <summary>
            /// get Automatically Approved GOP.
            /// </summary>
            public const string getAutomaticallyApprovedGOP = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='jarvis_gop'>
    <attribute name='jarvis_gopid' />
    <attribute name='jarvis_name' />
    <attribute name='createdon' />
    <order attribute='jarvis_name' descending='false' />
    <filter type='and'>
      <condition attribute='jarvis_incident' operator='eq' value='{0}'/>
      <condition attribute='jarvis_gopapproval' operator='eq' value='334030001' />
      <filter type='or'>
        <condition attribute='jarvis_contact' operator='like' value='%automatically%' />
        <condition attribute='jarvis_contact' operator='like' value='%automated%' />
      </filter>
      </filter>
  </entity>
</fetch>";

            /// <summary>
            /// get GOP HD For Case.
            /// </summary>
            public const string getGOPHDForCase = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='jarvis_gop'>
    <attribute name='jarvis_gopid' />
    <attribute name='jarvis_name' />
    <attribute name='jarvis_gopapproval' />
    <order attribute='jarvis_name' descending='false' />
    <filter type='and'>
      <filter type='or'>
        <condition attribute='jarvis_gopapproval' operator='eq' value='334030000' />
        <condition attribute='jarvis_requesttype' operator='eq' value='334030001' />
      </filter>
      <condition attribute='jarvis_incident' operator='eq' value='{0}'/>
    </filter>
  </entity>
</fetch>";

            /// <summary>
            /// get GOP HD For Case.
            /// </summary>
            public const string getGOPHDVolvoForCase = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='jarvis_gop'>
    <attribute name='jarvis_gopid' />
    <attribute name='jarvis_name' />
    <attribute name='jarvis_gopapproval' />
    <order attribute='jarvis_name' descending='false' />
    <filter type='and'>
      <condition attribute='jarvis_requesttype' operator='eq' value='334030001'/>
      <condition attribute='jarvis_incident' operator='eq' value='{0}'/>
    </filter>
  </entity>
</fetch>";

            /// <summary>
            /// get GOP RD For Case.
            /// </summary>
            public const string getGOPRDForCase = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='jarvis_gop'>
    <attribute name='jarvis_gopid' />
    <attribute name='jarvis_name' />
    <attribute name='createdon' />
    <attribute name='jarvis_gopapproval' />
    <attribute name='modifiedon' />
    <order attribute='modifiedon' descending='true' />
    <filter type='and'>
      <condition attribute='jarvis_requesttype' operator='eq' value='334030002' />
      <condition attribute='jarvis_incident' operator='eq' value='{0}'/>
    </filter>
  </entity>
</fetch>";

            /// <summary>
            /// get Last Modified Pass Out.
            /// </summary>
            public const string getLastModifiedPassOut = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='jarvis_passout'>
    <attribute name='jarvis_passoutid' />
    <attribute name='jarvis_name' />
    <attribute name='createdon' />
    <attribute name='jarvis_repairingdealer' />
    <attribute name='jarvis_etc' />
    <attribute name='jarvis_atc' />
    <attribute name='modifiedon' />
    <order attribute='modifiedon' descending='true' />
    <filter type='and'>
      <condition attribute='jarvis_incident' operator='eq' value='{0}'/>
      <condition attribute='statuscode' operator='eq' value='334030001' />
    </filter>
  </entity>
</fetch>";

            /// <summary>
            /// get Last Modified Pass Out For Case.
            /// </summary>
            public const string getLastModifiedPassOutForCase = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='jarvis_passout'>
    <attribute name='jarvis_passoutid' />
    <attribute name='jarvis_name' />
    <attribute name='createdon' />
    <attribute name='jarvis_repairingdealer' />
    <attribute name='jarvis_etc' />
    <attribute name='jarvis_atc' />
    <attribute name='modifiedon' />
    <order attribute='modifiedon' descending='true' />
    <filter type='and'>
      <condition attribute='jarvis_incident' operator='eq' value='{0}'/>
      <condition attribute='statuscode' operator='ne' value='2' />
    </filter>
  </entity>
</fetch>";

            /// <summary>
            /// get UnAssigned User.
            /// </summary>
            public const string getUnAssignedUser = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='systemuser'>
    <attribute name='fullname' />
    <attribute name='businessunitid' />
    <attribute name='title' />
    <attribute name='address1_telephone1' />
    <attribute name='positionid' />
    <attribute name='systemuserid' />
    <order attribute='fullname' descending='false' />
    <filter type='and'>
      <condition attribute='fullname' operator='like' value='%UNASSIGNED%' />
    </filter>
  </entity>
</fetch>";

            /// <summary>
            ///   // Fetch All GOP of a case.
            /// </summary>
            public const string getGOPsForCase = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='jarvis_gop'>
    <attribute name='jarvis_gopid' />
    <attribute name='jarvis_name' />
    <attribute name='jarvis_dealer' />
    <attribute name='modifiedon' />
<attribute name='createdon' />
    <attribute name='jarvis_goplimitout' />
    <attribute name='jarvis_goplimitin' />
    <attribute name='jarvis_totallimitout' />
    <attribute name='jarvis_requesttype' />
    <attribute name='jarvis_approved' />
    <attribute name='jarvis_totallimitoutcurrency' />
    <attribute name='jarvis_totallimitin' />
    <attribute name='jarvis_totallimitincurrency' />
    <order attribute='modifiedon' descending='true' />
    <filter type='and'>
      <condition attribute='statecode' operator='eq' value='0' />
      <condition attribute='jarvis_relatedgop' operator='null' />
      <condition attribute='jarvis_incident' operator='eq' uitype='incident' value='{0}' />
    </filter>
  </entity>
</fetch>";

            /// <summary>
            /// //Fetch Open Pass outs.
            /// </summary>
            public const string getRDPassOuts = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='jarvis_passout'>
    <attribute name='jarvis_passoutid' />
    <attribute name='jarvis_name' />
    <attribute name='createdon' />
    <order attribute='jarvis_name' descending='false' />
    <filter type='and'>
      <condition attribute='jarvis_repairingdealer' operator='eq' value='{0}'/>
      <condition attribute='statuscode' operator='not-in'>
        <value>3</value>
        <value>2</value>
      </condition>
      <condition attribute='jarvis_incident' operator='eq' value='{1}'/>
    </filter>
  </entity>
</fetch>";

            /// <summary>
            ///  // Fetch Repair Info for Case.
            /// </summary>
            public const string getRepairInfo = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='jarvis_repairinformation'>
    <attribute name='jarvis_repairinformationid' />
    <attribute name='jarvis_name' />
    <attribute name='createdon' />
    <order attribute='jarvis_name' descending='false' />
    <filter type='and'>
      <condition attribute='jarvis_incident' operator='eq' value='{0}'/>
    </filter>
  </entity>
</fetch>";

            /// <summary>
            /// get Repair Information.
            /// </summary>
            public const string getRepairInformation = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name = 'jarvis_repairinformation' >
    <attribute name='jarvis_repairinformationid' />
    <attribute name = 'jarvis_name' />
    <attribute name='createdon' />
    <order attribute = 'jarvis_name' descending='false' />
    <filter type = 'and' >
      <condition attribute='jarvis_incident' operator='eq'  value='{0}' />
      <condition attribute = 'jarvis_repairingdealerpassout' operator='eq'  value='{1}' />
<condition attribute='statecode' operator='eq' value='0' />
    </filter>
  </entity>
</fetch>";

            /// <summary>
            /// get all JED.
            /// </summary>
            public const string getJeds = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name = 'jarvis_jobenddetails' >
    <attribute name='jarvis_jobenddetailsid' />
    <attribute name = 'jarvis_name' />
    <attribute name='createdon' />
    <order attribute = 'jarvis_name' descending='false' />
    <filter type = 'and' >
      <condition attribute='jarvis_repairingdealerpassout' operator='eq'  value='{1}' />
      <condition attribute = 'jarvis_incident' operator='eq'  value='{0}' />
      <condition attribute = 'statecode' operator='eq' value='0' />
    </filter>
  </entity>
</fetch>";

            /// <summary>
            ///  // Fetch Team Member Roles.
            /// </summary>
            public const string getTeamMemberRoles = @" < fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='role'>
    <attribute name='name' />
    <attribute name='businessunitid' />
    <attribute name='roleid' />
    <order attribute='name' descending='false' />
    <filter type='and'>
      <condition attribute='businessunitid' operator='eq' value='{0}'/>
      <condition attribute='name' operator='eq' value='{1}'/>
    </filter>
  </entity>
</fetch>";

            /// <summary>
            /// // Fetch User Roles.
            /// </summary>
            public const string getUserMemberRoles = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>
  <entity name='role'>
    <attribute name='name' />
    <attribute name='businessunitid' />
    <attribute name='roleid' />
    <order attribute='name' descending='false' />
    <link-entity name='systemuserroles' from='roleid' to='roleid' visible='false' intersect='true'>
      <link-entity name='systemuser' from='systemuserid' to='systemuserid' alias='ac'>
        <filter type='and'>
          <condition attribute='systemuserid' operator='eq' value='{0}'/>
        </filter>
      </link-entity>
    </link-entity>
  </entity>
</fetch>";

            /// <summary>
            /// get Soft Offer Codes.
            /// </summary>
            public const string getSoftOfferCodes = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='jarvis_softoffer'>
    <attribute name='jarvis_softofferid' />
    <attribute name='jarvis_name' />
    <attribute name='jarvis_softoffercode' />
    <attribute name='jarvis_contractno' />
    <attribute name='jarvis_marketcode' />
    <attribute name='jarvis_startdate' />
    <attribute name='jarvis_expirydate' />
    <attribute name='jarvis_vehicle' />
    <attribute name='statecode' />
    <attribute name='jarvis_description' />
    <order attribute='jarvis_expirydate' descending='false' />
    <filter type='and'>
      <condition attribute='jarvis_vehicle' operator='eq' value='{0}'/>
      <condition attribute='jarvis_startdate' operator='on-or-before' value='{1}'/>
      <condition attribute='jarvis_expirydate' operator='on-or-after' value='{2}'/>
    </filter>
    <link-entity name='jarvis_softoffercode' from='jarvis_softoffercodeid' to='jarvis_softoffercode' visible='false' link-type='outer' alias='a_b55067383e0243029d57e5611fe35ec6'>
      <attribute name='jarvis_description' />
    </link-entity>
  </entity>
</fetch>";

            /// <summary>
            /// get Case Soft Offers.
            /// </summary>
            public const string getCaseSoftOffers = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='jarvis_casesoftoffer'>
    <attribute name='jarvis_casesoftofferid' />
    <attribute name='jarvis_name' />
    <attribute name='jarvis_contractno' />
    <attribute name='jarvis_description' />
    <attribute name='jarvis_startdate' />
    <attribute name='jarvis_expirydate' />
    <attribute name='statuscode' />
    <attribute name='jarvis_softoffercodelookup' />
    <order attribute='jarvis_expirydate' descending='false' />
    <filter type='and'>
      <condition attribute='jarvis_case' operator='eq' value='{0}'/>
      <condition attribute='statecode' operator='eq' value='0' />
    </filter>
  </entity>
</fetch>";

            /// <summary>
            /// case Contact Has Mobile.
            /// </summary>
            public const string caseContactHasMobile = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='jarvis_casecontact'>
    <attribute name='jarvis_casecontactid' />
    <attribute name ='jarvis_name' />
    <attribute name ='jarvis_mobilephone' />
    <attribute name ='jarvis_role' />
    <order attribute ='jarvis_name' descending='false' />
    <filter type ='and'>
      <condition attribute='jarvis_incident' operator='eq' value='{0}'/>
      <condition attribute ='jarvis_role' operator='eq' value='334030001'/>
      <condition attribute ='jarvis_mobilephone' operator='not-null' />
      <condition attribute='statecode' operator='eq' value='0' />
    </filter>
  </entity>
</fetch>";

            /// <summary>
            /// case Contact Has Mobile And Email.
            /// </summary>
            public const string caseContactHasMobileAndEmail = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='jarvis_casecontact'>
    <attribute name='jarvis_casecontactid' />
    <attribute name ='jarvis_name' />
    <order attribute ='jarvis_name' descending='false' />
    <filter type='and'>
      <condition attribute='jarvis_incident' operator='eq'  value='{0}' />
      <condition attribute='jarvis_role' operator='eq' value='334030001' />
      <filter type='or'>
        <condition attribute='jarvis_mobilephone' operator='not-null' />
        <condition attribute='jarvis_email' operator='not-null' />
      </filter>
    </filter> 
</entity>
</fetch>";

            /// <summary>
            /// get Monitor Actions For FU.
            /// </summary>
            public const string getMonitorActionsForFU = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
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
   <order attribute='actualstart' descending='false' />
    <filter type='and'>
       <condition attribute='regardingobjectid' operator='eq' value='{0}'/>
       <condition attribute='statuscode' operator='eq' value='1' />
    </filter>
  </entity>
</fetch>";

            /// <summary>
            /// get Monitor Skill.
            /// </summary>
            public const string getMonitorSkill = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='jarvis_monitorskill'>
    <attribute name='jarvis_monitorskillid' />
    <attribute name='jarvis_name' />
    <attribute name='createdon' />
    <order attribute='jarvis_name' descending='false' />
    <filter type='and'>
      <condition attribute='jarvis_caselocation' operator='eq' value='{0}' />
      <condition attribute='jarvis_casetype' operator='eq' value='{1}' />
      <condition attribute='jarvis_country' operator='eq' value='{2}'/>
      <condition attribute='jarvis_serviceline' operator='eq' value='{3}'/>
      <condition attribute='statecode' operator='eq' value='0' />
    </filter>
  </entity>
</fetch>";

            /// <summary>
            /// get Language MO.
            /// </summary>
            public const string getLanguageMO = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='jarvis_language'>
    <attribute name='jarvis_languageid' />
    <attribute name='jarvis_name' />
    <attribute name='createdon' />
    <order attribute='jarvis_name' descending='false' />
    <filter type='and'>
      <condition attribute='jarvis_iso3languagecode6392t' operator='eq' value='{0}'/>
      <condition attribute='statecode' operator='eq' value='0' />
      <condition attribute='jarvis_vasstandardlanguage' operator='eq' value='1' />
    </filter>
  </entity>
</fetch>";

            /// <summary>
            /// get Services For Resource.
            /// </summary>
            public const string getServicesForResource = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='characteristic'>
    <attribute name='name' />
    <attribute name='createdon' />
    <attribute name='description' />
    <attribute name='characteristictype' />
    <attribute name='characteristicid' />
    <order attribute='name' descending='false' />
    <filter type='and'>
      <condition attribute='characteristictype' operator='eq' value='1' />
      <condition attribute='name' operator='eq' value='{0}'/>
      <condition attribute='statecode' operator='eq' value='0' />
    </filter>
  </entity>
</fetch>";

            /// <summary>
            ///  //Get Active Pass out.
            /// </summary>
            public const string CaseAllActivePassouts = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
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
      </filter>
  </entity>
</fetch>";

            /// <summary>
            /// Case All Active GOPs.
            /// </summary>
            public const string CaseAllActiveGOPs = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' no-lock='true' distinct='false'>
  <entity name='jarvis_gop'>
    <attribute name='jarvis_gopid' />
    <attribute name='jarvis_name' />
    <attribute name='createdon' />
    <attribute name='jarvis_dealer' />
    <attribute name='modifiedon' />
    <order attribute='modifiedon' descending='true' />
    <filter type='and'>
      <condition attribute='jarvis_incident' operator='eq' uitype='incident' value='{0}' />
      <condition attribute='statecode' operator='eq' value='0' />
    </filter>
  </entity>
</fetch>";

            /// <summary>
            ///   // Get EN .
            /// </summary>
            public const string getEnglishSupportedLanguage = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='jarvis_language'>
    <attribute name='jarvis_languageid' />
    <attribute name='jarvis_name' />
    <attribute name='createdon' />
    <order attribute='jarvis_name' descending='false' />
    <filter type='and'>
      <condition attribute='jarvis_iso2languagecode6391' operator='eq' value='EN' />
      <condition attribute='jarvis_vasstandardlanguage' operator='eq' value='1' />
    </filter>
  </entity>
</fetch>";

            /// <summary>
            /// get GOP Translation.
            /// </summary>
            public const string getGOPTranslation = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='jarvis_goptranslation'>
    <attribute name='jarvis_goptranslationid' />
    <attribute name='jarvis_name' />
    <attribute name='createdon' />
    <order attribute='jarvis_name' descending='false' />
    <filter type='and'>
      <condition attribute='jarvis_case' operator='eq' value='{0}'/>
      <condition attribute='jarvis_gop' operator='eq' value='{1}'/>
      <condition attribute='statecode' operator='eq' value='0' />
      <condition attribute='jarvis_language' operator='eq' value='{2}'/>
    </filter>
  </entity>
</fetch>";

            /// <summary>
            /// get Repair Info Translation.
            /// </summary>
            public const string getRepairInfoTranslation = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='jarvis_repairinformationtranslation'>
    <attribute name='jarvis_repairinformationtranslationid' />
    <attribute name='jarvis_name' />
    <attribute name='createdon' />
    <order attribute='jarvis_name' descending='false' />
    <filter type='and'>
      <condition attribute='jarvis_case' operator='eq' value='{0}'/>
      <condition attribute='jarvis_repairinformation' operator='eq' value='{1}'/>
      <condition attribute='jarvis_language' operator='eq' value='{2}'/>
      <condition attribute='statecode' operator='eq' value='0' />
    </filter>
  </entity>
</fetch>";

            /// <summary>
            /// get Pass Out Translation.
            /// </summary>
            public const string getPassOutTranslation = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='jarvis_passouttranslation'>
    <attribute name='jarvis_passouttranslationid' />
    <attribute name='jarvis_name' />
    <attribute name='createdon' />
    <order attribute='jarvis_name' descending='false' />
    <filter type='and'>
      <condition attribute='jarvis_case' operator='eq' value='{0}'/>
      <condition attribute='jarvis_passout' operator='eq' value='{1}'/>
      <condition attribute='jarvis_language' operator='eq' value='{2}'/>
      <condition attribute='statecode' operator='eq' value='0' />
    </filter>
  </entity>
</fetch>";

            /// <summary>
            /// get Query Translations.
            /// </summary>
            public const string getQueryTranslations = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='jarvis_querycasetranslation'>
    <attribute name='jarvis_querycasetranslationid' />
    <attribute name='jarvis_name' />
    <attribute name='createdon' />
    <order attribute='jarvis_name' descending='false' />
    <filter type='and'>
      <condition attribute='jarvis_incident' operator='eq' value='{0}'/>
      <condition attribute='statecode' operator='eq' value='0' />
      <condition attribute='jarvis_language' operator='eq' value='{1}'/>
    </filter>
  </entity>
</fetch>";

            /// <summary>
            /// get Modified User.
            /// </summary>
            public const string getModifiedUser = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='systemuser'>
    <attribute name='fullname' />
    <attribute name='businessunitid' />
    <attribute name='title' />
    <attribute name='address1_telephone1' />
    <attribute name='positionid' />
    <attribute name='systemuserid' />
    <order attribute='fullname' descending='false' />
    <filter type='and'>
      <condition attribute='systemuserid' operator='eq' value='{0}'/>
    </filter>
  </entity>
</fetch>";

            /// <summary>
            /// get Case Contacts.
            /// </summary>
            public const string getCaseContacts = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='jarvis_casecontact'>
    <attribute name='jarvis_casecontactid' />
    <attribute name='jarvis_name' />
    <attribute name='createdon' />
    <attribute name='jarvis_preferredlanguage' />
    <order attribute='jarvis_name' descending='false' />
    <filter type='and'>
      <condition attribute='jarvis_incident' operator='eq' value='{0}'/>
      <condition attribute='statecode' operator='eq' value='0' />
      <condition attribute='jarvis_preferredlanguage' operator='not-null' />
    </filter>
  </entity>
</fetch>";

            /// <summary>
            /// get GOP Case Contacts.
            /// </summary>
            public const string getGOPCaseContacts = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='jarvis_casecontact'>
    <attribute name='jarvis_casecontactid' />
    <attribute name='jarvis_name' />
    <attribute name='createdon' />
    <attribute name='jarvis_preferredlanguage' />
    <order attribute='jarvis_name' descending='false' />
    <filter type='and'>
      <condition attribute='jarvis_gop' operator='eq' value='{0}'/>
      <condition attribute='statecode' operator='eq' value='0' />
      <condition attribute='jarvis_preferredlanguage' operator='not-null' />
    </filter>
  </entity>
</fetch>";

            /// <summary>
            /// get Pass Out Case Contacts.
            /// </summary>
            public const string getPassOutCaseContacts = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='jarvis_casecontact'>
    <attribute name='jarvis_casecontactid' />
    <attribute name='jarvis_name' />
    <attribute name='createdon' />
    <attribute name='jarvis_preferredlanguage' />
    <order attribute='jarvis_name' descending='false' />
    <filter type='and'>
      <condition attribute='jarvis_passout' operator='eq' value='{0}'/>
      <condition attribute='statecode' operator='eq' value='0' />
      <condition attribute='jarvis_preferredlanguage' operator='not-null' />
    </filter>
  </entity>
</fetch>";

            /// <summary>
            /// get JED Case Contacts.
            /// </summary>
            public const string getJEDCaseContacts = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='jarvis_casecontact'>
    <attribute name='jarvis_casecontactid' />
    <attribute name='jarvis_name' />
    <attribute name='createdon' />
    <attribute name='jarvis_preferredlanguage' />
    <order attribute='jarvis_name' descending='false' />
    <filter type='and'>
      <condition attribute='jarvis_jobenddetails' operator='eq' value='{0}'/>
      <condition attribute='statecode' operator='eq' value='0' />
      <condition attribute='jarvis_preferredlanguage' operator='not-null' />
    </filter>
  </entity>
</fetch>";

            /// <summary>
            /// //Fetch Monitor Actions.
            /// </summary>
            public const string getHDRDMonitorActions = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='jarvis_casemonitoraction'>
    <attribute name='activityid' />
    <attribute name='subject' />
    <attribute name='createdon' />
    <attribute name='prioritycode' />
    <attribute name='jarvis_monitorsortorder' />
    <attribute name='jarvis_followuptime' />
    <attribute name='jarvis_followuplanguage' />
    <attribute name='jarvis_followupcountry' />
    <attribute name='actualstart' />
    <attribute name='createdby' />
    <order attribute='jarvis_monitorsortorder' descending='false' />
    <order attribute='actualstart' descending='false' />
    <filter type='and'>
      <condition attribute='regardingobjectid' operator='eq' uiname='' uitype='incident' value='{0}'/>
      <condition attribute='statecode' operator='eq' value='0' />
      <condition attribute='subject' operator='eq' value='YYY YY Case auto close HD=RD' />
      <condition attribute='statuscode' operator='eq' value='1' />
    </filter>
  </entity>
</fetch>";

            /// <summary>
            /// get Queue Item Of case.
            /// </summary>
            public const string getQueueItemOfcase = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='queueitem'>
    <attribute name='objecttypecode' />
    <attribute name='objectid' />
    <attribute name='queueitemid' />
    <attribute name='title' />
    <attribute name='enteredon' />
     <attribute name='workerid' />
    <order attribute='enteredon' descending='true' />
    <filter type='and'>
      <condition attribute='objectid' operator='eq'  uitype='incident' value='{0}' />
      <condition attribute='objecttypecode' operator='eq' value='112' />
      <condition attribute='statecode' operator='eq' value='0' />
    </filter>
  </entity>
</fetch>";

            /// <summary>
            /// //Get Customer Contacts.
            /// </summary>
            public const string GetCustomerContacts = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
        <entity name='contact'>
		<attribute name='contactid' />
		<attribute name='parentcustomerid' />
		<attribute name='jarvis_sortorder' />
		<attribute name='firstname' />
		<attribute name='company' />
		<attribute name='fullname' />
		<attribute name='lastname' />
        <attribute name='telephone1' />
        <attribute name='mobilephone' />
        <attribute name='jarvis_language' />
        <attribute name='emailaddress1' />
        <attribute name='jarvis_title' />
        <attribute name='preferredcontactmethodcode' />
        <attribute name='jarvis_parentaccounttype' />
		<!-- Filter By -->
		<filter type='and'>
			<condition attribute='parentcustomerid' operator='eq' value='{0}' />
		</filter>
	</entity>
</fetch>";

            /// <summary>
            /// getNotesForCase.
            /// </summary>
            public const string getNotesForCase = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='annotation'>
    <attribute name='subject' />
    <attribute name='notetext' />
    <attribute name='filename' />
    <attribute name='annotationid' />
    <order attribute='subject' descending='false' />
	<filter type='and'>
			<condition attribute='objectid' operator='eq' value='{0}' />
		</filter>
  </entity>
</fetch>";

            /// <summary>
            /// getPostsForCase.
            /// </summary>
            public const string getPostsForCase = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='post'>
    <attribute name='postid' />
    <attribute name='createdon' />
    <attribute name='createdby' />
    <attribute name='source' />
    <attribute name='modifiedon' />
    <attribute name='regardingobjectid' />
    <filter type='and'>
      <condition attribute='source' operator='eq' value='2' />
       <condition attribute='regardingobjectid' operator='eq' value='{0}' />
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
            /// Gets or sets  Id.
            /// </summary>
            public string Id { get; set; }

            /// <summary>
            /// Gets or sets TransType.
            /// </summary>
            public string TransType { get; set; }
        }
    }
}
