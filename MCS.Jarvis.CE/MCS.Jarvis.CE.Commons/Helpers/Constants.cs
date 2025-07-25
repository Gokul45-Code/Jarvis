// <copyright file="Constants.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MCS.Jarvis.CE.Commons.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Constants Class.
    /// </summary>
    public class Constants
    {
        /// <summary>
        /// Incident Class.
        /// </summary>
        public class Incident
        {
            /// <summary>
            /// VASBreakDownConfiguration attribute name.
            /// </summary>
            public const string BpfStageName = "VASBreakDownConfiguration";

            /// <summary>
            /// VASSearchAPI attribute name.
            /// </summary>
            public const string SearchAPI = "VASSearchAPI";

            /// <summary>
            /// Case Opening attribute name.
            /// </summary>
            public const string BpfStage1 = "Case Opening";

            /// <summary>
            /// Guarantee Of Payment attribute name.
            /// </summary>
            public const string BpfStage2 = "Guarantee Of Payment";

            /// <summary>
            /// Pass Out attribute name.
            /// </summary>
            public const string BpfStage3 = "Pass Out";

            /// <summary>
            /// ETA Technician attribute name.
            /// </summary>
            public const string BpfStage4 = "ETA Technician";

            /// <summary>
            /// Waiting For Repair Start attribute name.
            /// </summary>
            public const string BpfStage5 = "Waiting For Repair Start";

            /// <summary>
            /// Repair Ongoing attribute name.
            /// </summary>
            public const string BpfStage6 = "Repair Ongoing";

            /// <summary>
            /// Repair Finished attribute name.
            /// </summary>
            public const string BpfStage7 = "Repair Finished";

            /// <summary>
            /// Repair Summary attribute name.
            /// </summary>
            public const string BpfStage8 = "Repair Summary";

            /// <summary>
            /// Case Closure attribute name.
            /// </summary>
            public const string BpfStage9 = "Case Closure";

            /// <summary>
            /// Ongoing attribute name.
            /// </summary>
            public const string BpfStage10 = "Ongoing";

            /// <summary>
            /// Solved attribute name.
            /// </summary>
            public const string BpfStage11 = "Solved";

            /// <summary>
            /// Credit To HD attribute name.
            /// </summary>
            public const string BpfStage12 = "Credit To HD";

            /// <summary>
            /// Closed attribute name.
            /// </summary>
            public const string BpfStage13 = "Closed";
        }

        /// <summary>
        /// Case contact.
        /// </summary>
        public class Casecontact
        {
            /// <summary>
            /// First name.
            /// </summary>
            public const string JarvisFirstname = "jarvis_firstname";

            /// <summary>
            /// Last name.
            /// </summary>
            public const string JarvisLastname = "jarvis_lastname";

            /// <summary>
            /// Mobile phone.
            /// </summary>
            public const string JarvisMobilephone = "jarvis_mobilephone";

            /// <summary>
            /// Role Attribute.
            /// </summary>
            public const string JarvisRole = "jarvis_role";

            /// <summary>
            /// Preferred language.
            /// </summary>
            public const string JarvisPreferredlanguage = "jarvis_preferredlanguage";

            /// <summary>
            /// Case contact type.
            /// </summary>
            public const string JarvisCasecontacttype = "jarvis_casecontacttype";

            /// <summary>
            /// Preferred method of contact.
            /// </summary>
            public const string JarvisPreferredmethodofcontact = "jarvis_preferredmethodofcontact";

            /// <summary> Attribute name.
            /// Incident.
            /// </summary>
            public const string JarvisIncident = "jarvis_incident";

            /// <summary>
            /// Case contact id.
            /// </summary>
            public const string JarvisCasecontactid = "jarvis_casecontactid";

            /// <summary>
            /// Case contact.
            /// </summary>
            public const string JarvisCasecontact = "jarvis_casecontact";
        }

        /// <summary>
        /// Trans Type.
        /// </summary>
        public class TransType
        {
            /// <summary>
            /// Pass Out Update.
            /// </summary>
            public const string PassOutUpdate = "Jarvis.Event.PassOut";

            /// <summary>
            /// Pass Out ETA.
            /// </summary>
            public const string PassOutETA = "Jarvis.Event.ETAUpdate";

            /// <summary>
            /// Pass Out Delayed ETA.
            /// </summary>
            public const string PassOutDelayedETA = "Jarvis.Event.delayedETAUpdate";
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
        /// Class to Access FetchXml.
        /// </summary>
        public class FetchXmls
        {
            /// <summary>
            ///   //Retrieve Pass Out.
            /// </summary>
            public const string RetrievePassOuts = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' no-lock='true' distinct='false'>
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
            ///  //Calculate Total Approved Amount for GOP.
            /// </summary>
            public const string CalculateTotalAmntAppr = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false' aggregate='true'>
  <entity name='jarvis_gop'>
    <attribute name='jarvis_goplimitout' alias='goplimitout' aggregate='sum' /> 
    <filter type='and'>
      <condition attribute='jarvis_incident' operator='eq' value='{0}'/>
      <condition attribute='jarvis_approved' operator='eq' value='1' />
    </filter>
  </entity>
</fetch>";

            /// <summary>
            ///  //Calculate Total Approved Amount for Pass Outs.
            /// </summary>
            public const string CalculatePassOutTotalAmnt = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false' aggregate='true'>
  <entity name='jarvis_passout'>
    <attribute name='jarvis_goplimitout' alias='goplimitout' aggregate='sum'/> 
    <filter type='and'>
      <condition attribute='jarvis_incident' operator='eq' value='{0}'/>
      <condition attribute='statecode' operator='eq' value='0' />
    </filter>
  </entity>
</fetch>";

            /// <summary>
            ///    //Get VAS Configuration.
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
            ///  //Get Work Type for Work Order creation.
            /// </summary>
            public const string GetWorkType = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' no-lock='true' distinct='false'>
  <entity name='msdyn_workordertype'>
    <attribute name='msdyn_workordertypeid' />
    <attribute name='msdyn_name' />
    <attribute name='createdon' />
    <attribute name='msdyn_incidentrequired' />
    <order attribute='msdyn_name' descending='false' />
  </entity>
</fetch>";

            /// <summary>
            /// //Get Business Process Name.
            /// </summary>
            public const string GetProcessName = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
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
            /// //Get Business Process For Case.
            /// </summary>
            public const string GetBreakDownProcess = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
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
            ///  //Get BPF Instance for Case.
            /// </summary>
            public const string GetCaseBPFInstance = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
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
            ///  //Get PriceList for WO.
            /// </summary>
            public const string GetPricelist = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
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
            ///  //Get Work Orders.
            /// </summary>
            public const string GetWorkOrders = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
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
            ///  //Get Brand Incident Types.
            /// </summary>
            public const string GetIncidentTypesBrands = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
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
            ///    //Get Incident Nature For WO.
            /// </summary>
            public const string GetIncidentNatureForCase = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
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
            ///  //Get Incident Nature For Delete.
            /// </summary>
            public const string GetIncNatures = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>
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
            ///    //Get WO Requirement.
            /// </summary>
            public const string GetResourceRequirement = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
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
            public const string GetListofResourceCharacteristics = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
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
            ///   //Get Bookable Resource Characteristics.
            /// </summary>
            public const string GetBookableServices = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
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
            /// Search Availability API.
            /// </summary>
            public const string GetParamsForSearchAPI = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
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
            /// //Get Pass out.
            /// </summary>
            public const string CasePassouts = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='jarvis_passout'>
    <attribute name='jarvis_passoutid' />
    <attribute name='jarvis_name' />
    <attribute name='createdon' />
    <attribute name='jarvis_repairingdealer' />
    <attribute name='jarvis_goplimitout' />
    <attribute name='transactioncurrencyid' />
    <order attribute='jarvis_name' descending='false' />
    <filter type='and'>
      <condition attribute='statecode' operator='eq' value='0' />
      <condition attribute='jarvis_incident' operator='eq' uitype='incident' value='{0}' />
<condition attribute='jarvis_passoutid' operator='ne'   uitype='jarvis_passout' value='{1}' />
    </filter>
  </entity>
</fetch>";

            /// <summary>
            /// //Get Active Pass out.
            /// </summary>
            public const string CaseActivePassouts = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='jarvis_passout'>
    <attribute name='jarvis_passoutid' />
    <attribute name='jarvis_name' />
    <attribute name='createdon' />
    <attribute name='jarvis_repairingdealer' />
    <attribute name='jarvis_goplimitout' />
    <attribute name='transactioncurrencyid' />
    <order attribute='jarvis_name' descending='false' />
    <filter type='and'>
      <condition attribute='statecode' operator='eq' value='0' />
      <condition attribute='jarvis_incident' operator='eq' uitype='incident' value='{0}' />
    </filter>
  </entity>
</fetch>";

            /// <summary>
            /// // Fetch all GOP related to case having the current Pass out as repairing dealer.
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
      <condition attribute='jarvis_incident' operator='eq' uitype='incident' value='{0}' />
      <condition attribute='jarvis_repairingdealer' operator='eq' uitype='jarvis_passout' value='{1}' />
    </filter>
  </entity>
</fetch>";

            /// <summary>
            ///  //Fetch Last Modified GOP for Query.
            /// </summary>
            public const string GetLastModGOP = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='jarvis_gop'>
    <attribute name='jarvis_gopid' />
    <attribute name='jarvis_name' />
    <attribute name='createdon' />
    <attribute name='modifiedon' />
    <order attribute='modifiedon' descending='true' />
    <filter type='and'>
      <condition attribute='jarvis_approved' operator='eq' value='1' />
      <condition attribute='jarvis_incident' operator='eq' value='{0}'/>
    </filter>
  </entity>
</fetch>";

            /// <summary>
            /// // Fetch of a Case with particular home dealer.
            /// </summary>
            public const string GetGOPsForDealer = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='jarvis_gop'>
    <attribute name='jarvis_gopid' />
    <attribute name='jarvis_name' />
    <attribute name='createdon' />
    <attribute name='jarvis_goplimitout' />
    <attribute name='jarvis_goplimitin' />
    <order attribute='jarvis_name' descending='false' />
    <filter type='and'>
      <condition attribute='statecode' operator='eq' value='0' />
      <condition attribute='jarvis_incident' operator='eq' uitype='incident' value='{0}' />
      <condition attribute='jarvis_dealer' operator='equitype='account' value='{1}' />
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
        }
    }
}
