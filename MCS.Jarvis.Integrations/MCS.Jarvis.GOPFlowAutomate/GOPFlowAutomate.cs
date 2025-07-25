using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using MCS.Jarvis.Integration.Base.Dynamics;
using MCS.Jarvis.Integration.Base.Logging;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using IntegrationProcess.Helper.Constants;
using Newtonsoft.Json.Linq;
using System.Diagnostics.Metrics;
using System.Net.Http;
using System.Threading;
using System.Linq;
using System.Reflection;
using static GOPFlowAutomate.GOPFlow;
using System.Globalization;
using Microsoft.Azure.Amqp.Framing;

namespace GOPFlowAutomate
{
    public class GOPFlow
    {
        private readonly IConfiguration config;
        private readonly IDynamicsApiClient dynamicsClient;
        private string Lineage;
        public List<ConditionLink> flowConditions = new List<ConditionLink>();
        public List<ProcesFlow> procesFlow = new List<ProcesFlow>();
        public const string CaseIncidentnature = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>
  <entity name='jarvis_incidentnature'>
    <attribute name='jarvis_incidentnatureid' />
    <attribute name='jarvis_name' />
    <attribute name='createdon' />
    <attribute name='jarvis_stopgopautoapprovalwhitelist' />
    <attribute name='jarvis_stopgopautoapprovalwarranty' />
    <attribute name='jarvis_stopgopautoapprovalcontract' />
    <attribute name='jarvis_incidenttype' />
    <order attribute='jarvis_name' descending='false' />
    <link-entity name='jarvis_incident_jarvis_incidentnature' from='jarvis_incidentnatureid' to='jarvis_incidentnatureid' visible='false' intersect='true'>
      <link-entity name='incident' from='incidentid' to='incidentid' alias='ak'>
        <filter type='and'>
          <condition attribute='incidentid' operator='eq' uitype='incident' value='{0}' />
        </filter>
      </link-entity>
    </link-entity>
    <link-entity name='jarvis_incidentnature_jarvis_brand' from='jarvis_incidentnatureid' to='jarvis_incidentnatureid' visible='false' intersect='true'>
      <link-entity name='jarvis_brand' from='jarvis_brandid' to='jarvis_brandid' alias='al'>
        <filter type='and'>
          <condition attribute='jarvis_brandid' operator='eq'  uitype='jarvis_brand' value='{1}' />
        </filter>
      </link-entity>
    </link-entity>
  </entity>
</fetch>";

        public const string CaseWarranty = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='jarvis_warranty'>
    <attribute name='jarvis_warrantyid' />
    <attribute name='jarvis_name' />
    <attribute name='createdon' />
    <attribute name='jarvis_gopautoapproved' />
    <attribute name='jarvis_gopamount' />
    <attribute name='transactioncurrencyid' />
    <attribute name='jarvis_country' />
    <attribute name='jarvis_caselocation' />
    <attribute name='jarvis_paymenttype' />
    <order attribute='jarvis_name' descending='false' />
    <filter type='and'>
      <condition attribute='jarvis_country' operator='eq' uitype='jarvis_country' value='{0}' />
      <condition attribute='jarvis_brand' operator='eq'  uitype='jarvis_brand' value='{1}' />
      <condition attribute='jarvis_mileagelimit' operator='ge' value='{2}' />
    <condition attribute='jarvis_warrantyyear' operator='eq' value='{3}' />
      <filter type='or'>
        <condition attribute='jarvis_vehiclemodel' operator='null' />
        <condition attribute='jarvis_vehiclemodel' operator='eq' value='{4}' />
      </filter>
     <filter type='or'>
        <condition attribute='jarvis_caselocation' operator='eq' value='{5}' />
        <condition attribute='jarvis_caselocation' operator='null' />
    </filter>
    </filter>
  </entity>
</fetch>";

        public const string Vehiclecontracts = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='jarvis_vehiclecontract'>
    <attribute name='jarvis_vehiclecontractid' />
    <attribute name='jarvis_name' />
    <attribute name='createdon' />
    <attribute name='jarvis_country' />
    <attribute name='jarvis_onecasecontracttype' />
    <order attribute='modifiedon' descending='false' />
    <filter type='and'>
      <condition attribute='jarvis_vehicle' operator='eq' uitype='jarvis_vehicle' value='{0}' />
<condition attribute='jarvis_onecasecontracttype' operator='not-null' />
      <condition attribute='jarvis_startdate' operator='on-or-before' value='{1}' />
      <condition attribute='jarvis_expiringdate' operator='on-or-after' value='{2}' />
<condition attribute='jarvis_case' operator='eq' uitype='incident' value='{3}' />
      <condition attribute='statecode' operator='eq' value='0' />
 <condition attribute='jarvis_startmileage' operator='le' value='{4}' />
   <condition attribute='jarvis_maxmileage' operator='ge' value='{5}' />

</filter>
  </entity>
</fetch>";

        public const string ExchangeRates = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' no-lock='true' distinct='false'>
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

        public const string DefaultEuroCurrencies = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' no-lock='true' distinct='false'>
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

        public class ProcesFlow
        {
            public int seq;
            public string flow;
            public bool? isSuccessful;

        }
        public class ConditionLink
        {
            public bool warranty;
            public bool contract;
            public bool whitelist;
            public bool isTowingIncluded;
            public bool isTrailerIncluded;
            public string brand;
            public List<ProcesFlow> processList;
            public string lineage;
            public string CaseId;
            public bool disableAutoapprove;
            public string CaseHdCountryId;
            public string MasterDataId;
            public int? statuscode;
        }
        public class GOPInit
        {
            public decimal? Amount;
            public string Currency;
            public int? isApproved;
            //public bool? isApproved;
            public string Contact;
            public int? PaymentType;
            public string CaseId;
            public string dealerId;
            public DateTime approvaldate;
            public int? statuscode;
        }

        public enum GopApproval
        {
            Declined = 334030002,
            Pending = 334030000,
            Approved = 334030001
        }
        public enum Status
        {
            Hasbeensent = 30, // gop status
            Status = 1 // passout status 
        }
        public class PassOutInit
        {
            public string dealerId;
            public string Contact;
            public decimal? Amount;
            public int? statuscode;
            public int? PaymentType;
            public string Currency;
        }

        public GOPFlow(IConfiguration config, IDynamicsApiClient dynamicsClient)
        {
            this.config = config;
            this.dynamicsClient = dynamicsClient;
            // Condition Navigation Link
            // Flow-1 
            procesFlow = new List<ProcesFlow>();
            procesFlow.Add(new ProcesFlow() { seq = 1, flow = "whitelistSubProcess", isSuccessful = null });
            procesFlow.Add(new ProcesFlow() { seq = 2, flow = "warrantySubProcess", isSuccessful = null });
            procesFlow.Add(new ProcesFlow() { seq = 3, flow = "ContractSubProcess", isSuccessful = null });
            procesFlow.Add(new ProcesFlow() { seq = 4, flow = "GOPstandardSubProcess", isSuccessful = null });
            flowConditions.Add(new ConditionLink() { warranty = false, contract = false, whitelist = false, brand = "", processList = procesFlow });
            // Flow-2
            procesFlow = new List<ProcesFlow>();
            procesFlow.Add(new ProcesFlow() { seq = 1, flow = "warrantySubProcess", isSuccessful = null });
            procesFlow.Add(new ProcesFlow() { seq = 2, flow = "GOPstandardSubProcess", isSuccessful = null });
            flowConditions.Add(new ConditionLink() { warranty = false, contract = true, whitelist = true, brand = "", processList = procesFlow });
            // Flow-3
            procesFlow = new List<ProcesFlow>();
            procesFlow.Add(new ProcesFlow() { seq = 1, flow = "ContractSubProcess", isSuccessful = null });
            procesFlow.Add(new ProcesFlow() { seq = 2, flow = "GOPstandardSubProcess", isSuccessful = null });
            flowConditions.Add(new ConditionLink() { warranty = true, contract = false, whitelist = true, brand = "", processList = procesFlow });
            // Flow-4
            procesFlow = new List<ProcesFlow>();
            procesFlow.Add(new ProcesFlow() { seq = 1, flow = "whitelistSubProcess", isSuccessful = null });
            procesFlow.Add(new ProcesFlow() { seq = 2, flow = "GOPstandardSubProcess", isSuccessful = null });
            flowConditions.Add(new ConditionLink() { warranty = true, contract = true, whitelist = false, brand = "", processList = procesFlow });
            // Flow-5
            procesFlow = new List<ProcesFlow>();
            procesFlow.Add(new ProcesFlow() { seq = 1, flow = "warrantySubProcess", isSuccessful = null });
            procesFlow.Add(new ProcesFlow() { seq = 2, flow = "ContractSubProcess", isSuccessful = null });
            procesFlow.Add(new ProcesFlow() { seq = 3, flow = "GOPstandardSubProcess", isSuccessful = null });
            flowConditions.Add(new ConditionLink() { warranty = false, contract = false, whitelist = true, brand = "", processList = procesFlow });
            // Flow-6 
            procesFlow = new List<ProcesFlow>();
            procesFlow.Add(new ProcesFlow() { seq = 1, flow = "whitelistSubProcess", isSuccessful = null });
            procesFlow.Add(new ProcesFlow() { seq = 2, flow = "warrantySubProcess", isSuccessful = null });
            procesFlow.Add(new ProcesFlow() { seq = 3, flow = "GOPstandardSubProcess", isSuccessful = null });
            flowConditions.Add(new ConditionLink() { warranty = false, contract = true, whitelist = false, brand = "", processList = procesFlow });
            // Flow-7 
            procesFlow = new List<ProcesFlow>();
            procesFlow.Add(new ProcesFlow() { seq = 1, flow = "whitelistSubProcess", isSuccessful = null });
            procesFlow.Add(new ProcesFlow() { seq = 2, flow = "ContractSubProcess", isSuccessful = null });
            procesFlow.Add(new ProcesFlow() { seq = 3, flow = "GOPstandardSubProcess", isSuccessful = null });
            flowConditions.Add(new ConditionLink() { warranty = true, contract = false, whitelist = false, brand = "", processList = procesFlow });
            // Flow-8
            procesFlow = new List<ProcesFlow>();
            procesFlow.Add(new ProcesFlow() { seq = 1, flow = "GOPstandardSubProcess", isSuccessful = null });
            flowConditions.Add(new ConditionLink() { warranty = true, contract = true, whitelist = true, brand = "", processList = procesFlow });
        }

        [FunctionName("GOPFlowAutomate")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            ILoggerService logger = new LoggerService(log);
            //string caseId = req.Query["caseId"];

            var reader = new StreamReader(req.Body);
            var data = reader.ReadToEnd();
            dynamic msg = JsonConvert.DeserializeObject(data);
            string caseId = msg.IncidentId;
            log.LogInformation($"Case Id received in request is" + caseId);
            this.dynamicsClient.SetLoggingReference(logger);
            ConditionFlow(caseId);
            string responseMessage = string.IsNullOrEmpty(caseId)
                ? "GOPFlowAutomate unable to execute. Pass the incident Id as body parameter in the http trigger."
                : $"GOPFlowAutomate, {caseId}. executed successfully.";

            return new OkObjectResult(responseMessage);
        }

        #region FLOW-MAPPINGS
        /// <summary>
        /// ConditionFlow - Check from the flowConditions the subprocess to be followed
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private void ConditionFlow(string caseId)
        {
            bool warranty = false;
            bool contract = false;
            bool whitelist = false;
            bool isTowingIncluded = false;
            bool isTrailerIncluded = false;
            try
            {
                var caseObj = this.dynamicsClient.RetrieveEntityById("incidents", caseId);
                if (caseObj != null && caseObj.HasValues)
                {
                    if (caseObj.TryGetValue("_jarvis_vehicle_value", out JToken vehicle))
                    {
                        var casedealer = DynamicsApiHelper.GetStringValueFromJObject(caseObj, "_jarvis_homedealer_value");
                        var vehicleObj = this.dynamicsClient.RetrieveResultSetByFilter("jarvis_vehicles", "_jarvis_brandid_value,_jarvis_countryofoperation_value,jarvis_aftermarketmodel,jarvis_deliverydate,createdon", string.Format("jarvis_vehicleid eq '{0}'", vehicle), true);
                        if (vehicleObj != null)
                        {
                            //#573740-Whitelist in eService to be integrated with OneCase
                            var caseCallerRole = DynamicsApiHelper.GetIntegerValueFromJObject(caseObj, "jarvis_callerrole");
                            // For Caller role = HD
                            if (caseCallerRole != null && caseCallerRole == 3)
                            {
                                var matchedCondition = flowConditions.Where(f => f.warranty.Equals(true) && f.contract.Equals(true) && f.whitelist.Equals(false)).FirstOrDefault();
                                if (matchedCondition != null)
                                {
                                    matchedCondition.lineage = matchedCondition.lineage + ">" + "whitelistSubProcess";
                                    matchedCondition.CaseId = caseId;
                                    if (casedealer != null)
                                    {
                                        var homedealer = this.dynamicsClient.RetrieveEntityById("accounts", casedealer);
                                        if (homedealer != null)
                                        {
                                            matchedCondition.disableAutoapprove = DynamicsApiHelper.GetBoolValueFromJObject(homedealer, "jarvis_gopapprovalstop").GetValueOrDefault();
                                            matchedCondition.CaseHdCountryId = DynamicsApiHelper.GetStringValueFromJObject(homedealer, "_jarvis_address1_country_value");
                                        }
                                    }
                                }
                                var isSucessful = whitelistHDSubProcess(caseObj, vehicleObj?.FirstOrDefault()?.ToObject<dynamic>(), matchedCondition);
                                if (isSucessful != null && isSucessful.GetType().ToString() == "System.Boolean" && (bool)isSucessful == false)
                                {
                                    if (matchedCondition != null)
                                    {
                                        matchedCondition.lineage = matchedCondition.lineage + ">" + "GOPstandardSubProcess";
                                    }
                                    GOPstandardSubProcess(caseObj, vehicleObj?.FirstOrDefault()?.ToObject<dynamic>(), matchedCondition);
                                }
                            }
                            else // For Caller role = RD
                            {
                                #region HDRD&FleetCustomer
                                // Add HD=RD AutoGOP Process US#657623
                                var HDRD = DynamicsApiHelper.GetBoolValueFromJObject(caseObj, "jarvis_hdrd"); // HD=RD
                                var Fleetcustomer = DynamicsApiHelper.GetBoolValueFromJObject(caseObj, "jarvis_fleetcustomer"); // Fleet cut
                                if (HDRD == true || Fleetcustomer == true)
                                {
                                    ConditionLink matchedCondition = new ConditionLink();
                                    matchedCondition.CaseId = caseId;
                                    matchedCondition.lineage = matchedCondition.lineage + ">" + "GOPforHDRDProcess";
                                    matchedCondition.statuscode = (int?)Status.Hasbeensent;
                                    GOPHDRDSubProcess(caseObj, matchedCondition);
                                    PassOutCreationHDRD(caseObj, matchedCondition);
                                }
                                #endregion
                                else
                                {
                                    //var BrandId = vehicle.ToObject<dynamic>().GetValue("jarvis_brandid");

                                    var BrandId = vehicleObj?.FirstOrDefault()?.ToObject<dynamic>().GetValue("_jarvis_brandid_value");
                                    var CountryofOperation = vehicleObj?.FirstOrDefault()?.ToObject<dynamic>().GetValue("_jarvis_countryofoperation_value");

                                    // Get Associated Incident Nature matching the vehicle brand
                                    var incidentNatureList = this.dynamicsClient.RetrieveTopRecordsFromFetchXml("jarvis_incidentnatures", string.Format(GOPFlow.CaseIncidentnature, caseId, BrandId), true);
                                    if (incidentNatureList != null && incidentNatureList.Count > 0)
                                    {
                                        foreach (var incidentnature in incidentNatureList)
                                        {

                                            warranty = warranty || Boolean.Parse(incidentnature.GetValue("jarvis_stopgopautoapprovalwarranty")?.ToString());
                                            contract = contract || Boolean.Parse(incidentnature.GetValue("jarvis_stopgopautoapprovalcontract")?.ToString());
                                            whitelist = whitelist || Boolean.Parse(incidentnature.GetValue("jarvis_stopgopautoapprovalwhitelist")?.ToString());
                                            String IncidentnatureName = incidentnature?.GetValue("jarvis_name")?.ToString();
                                            isTowingIncluded = isTowingIncluded || IncidentnatureName.ToLower().Contains("towing");
                                            isTrailerIncluded = isTrailerIncluded || IncidentnatureName.ToLower().Contains("trailer");

                                        }
                                        // initiate subprocess depending on matched condition
                                        var matchedCondition = flowConditions.Where(f => f.warranty.Equals(warranty) && f.contract.Equals(contract) && f.whitelist.Equals(whitelist)).FirstOrDefault();
                                        if (matchedCondition != null)
                                        {
                                            // Get The Brand Name 
                                            var brandObj = this.dynamicsClient.RetrieveEntityById("jarvis_brands", BrandId.ToString());
                                            if (brandObj != null)
                                            {
                                                //matchedCondition.brand = brandObj.GetValue("jarvis_name")?.ToString();
                                                matchedCondition.brand = DynamicsApiHelper.GetStringValueFromJObject(brandObj, "jarvis_name");
                                            }
                                            if (casedealer != null)
                                            {
                                                var homedealer = this.dynamicsClient.RetrieveEntityById("accounts", casedealer);
                                                if (homedealer != null)
                                                {
                                                    matchedCondition.disableAutoapprove = DynamicsApiHelper.GetBoolValueFromJObject(homedealer, "jarvis_gopapprovalstop").GetValueOrDefault();
                                                    matchedCondition.CaseHdCountryId = DynamicsApiHelper.GetStringValueFromJObject(homedealer, "_jarvis_address1_country_value");
                                                }
                                            }
                                            // Set the Towing and Trailer Included Checks
                                            matchedCondition.isTowingIncluded = isTowingIncluded;
                                            matchedCondition.isTrailerIncluded = isTrailerIncluded;
                                            matchedCondition.CaseId = caseId;
                                            // Call the processes as per sequence
                                            foreach (ProcesFlow GOPProcesFlow in matchedCondition.processList)
                                            {
                                                matchedCondition.lineage = matchedCondition.lineage + ">" + GOPProcesFlow.flow;
                                                MethodInfo mi = this.GetType().GetMethod(GOPProcesFlow.flow);
                                                object[] parameters = new object[] { caseObj, vehicleObj.FirstOrDefault()?.ToObject<dynamic>(), matchedCondition };
                                                var isSucessful = mi.Invoke(this, parameters);
                                                if (isSucessful != null && isSucessful.GetType().ToString() == "System.Boolean" && (bool)isSucessful == true)
                                                {
                                                    break;
                                                }
                                            }
                                        }

                                    }
                                    else
                                    {
                                        // When no Incident Nature Matches Set Default Flow "No,No,No"
                                        var matchedCondition = flowConditions.Where(f => f.warranty.Equals(false) && f.contract.Equals(false) && f.whitelist.Equals(false)).FirstOrDefault();
                                        if (matchedCondition != null)
                                        {
                                            // Get The Brand Name 
                                            var brandObj = this.dynamicsClient.RetrieveEntityById("jarvis_brands", BrandId.ToString());
                                            if (brandObj != null)
                                            {
                                                //matchedCondition.brand = brandObj.GetValue("jarvis_name")?.ToString();
                                                matchedCondition.brand = DynamicsApiHelper.GetStringValueFromJObject(brandObj, "jarvis_name");
                                            }
                                            if (casedealer != null)
                                            {
                                                var homedealer = this.dynamicsClient.RetrieveEntityById("accounts", casedealer);
                                                if (homedealer != null)
                                                {
                                                    matchedCondition.disableAutoapprove = DynamicsApiHelper.GetBoolValueFromJObject(homedealer, "jarvis_gopapprovalstop").GetValueOrDefault();
                                                    matchedCondition.CaseHdCountryId = DynamicsApiHelper.GetStringValueFromJObject(homedealer, "_jarvis_address1_country_value");
                                                }
                                            }

                                            // Set the Towing and Trailer Included Checks
                                            matchedCondition.isTowingIncluded = false;
                                            matchedCondition.isTrailerIncluded = false;
                                            matchedCondition.CaseId = caseId;
                                            // Call the processes as per sequence
                                            foreach (ProcesFlow GOPProcesFlow in matchedCondition.processList)
                                            {
                                                matchedCondition.lineage = matchedCondition.lineage + ">" + GOPProcesFlow.flow;
                                                MethodInfo mi = this.GetType().GetMethod(GOPProcesFlow.flow);
                                                object[] parameters = new object[] { caseObj, vehicleObj?.FirstOrDefault()?.ToObject<dynamic>(), matchedCondition };
                                                var isSucessful = mi.Invoke(this, parameters);
                                                if (isSucessful != null && isSucessful.GetType().ToString() == "System.Boolean" && (bool)isSucessful == true)
                                                {
                                                    break;
                                                }
                                            }
                                        }
                                    }

                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"ConditionFlow:" + ex.Message);
            }
        }

        public bool whitelistSubProcess(dynamic caseObj, dynamic vehicleObj, ConditionLink matchedCondition)
        {
            bool isSucessful = false;
            bool ConditionSatisfied = false;
            try
            {
                var casedealer = DynamicsApiHelper.GetStringValueFromJObject(caseObj, "_jarvis_homedealer_value");
                DateTime caseCreatedDate = DynamicsApiHelper.GetDateTimeValueFromJObject(caseObj, "createdon");
                // get Whitelist for the case Vehicle
                ICollection<JObject> Vehicle_whitelist = this.dynamicsClient.RetrieveResultSetByFilter("jarvis_whitelists", "jarvis_whitelisttype,_jarvis_customer_value,_jarvis_vehicle_value,_jarvis_dealer_value,jarvis_maximumamount,_transactioncurrencyid_value,jarvis_towingincluded,jarvis_trailerincluded,modifiedon,jarvis_validfrom,jarvis_validuntil,jarvis_paymenttype", string.Format("_jarvis_vehicle_value eq '{0}'", DynamicsApiHelper.GetStringValueFromJObject(caseObj, "_jarvis_vehicle_value")), true);
                Vehicle_whitelist = Vehicle_whitelist.Where(w => DynamicsApiHelper.GetDateTimeValueFromJObject(w, "jarvis_validfrom") == null || (caseCreatedDate >= (DateTime)DynamicsApiHelper.GetDateTimeValueFromJObject(w, "jarvis_validfrom"))).ToList();
                Vehicle_whitelist = Vehicle_whitelist.Where(w => DynamicsApiHelper.GetDateTimeValueFromJObject(w, "jarvis_validuntil") == null || (caseCreatedDate <= (DateTime)DynamicsApiHelper.GetDateTimeValueFromJObject(w, "jarvis_validuntil"))).ToList();
                // get Whitelist for the case Customer
                ICollection<JObject> Customer_whitelist = this.dynamicsClient.RetrieveResultSetByFilter("jarvis_whitelists", "jarvis_whitelisttype,_jarvis_customer_value,_jarvis_vehicle_value,_jarvis_dealer_value,jarvis_maximumamount,_transactioncurrencyid_value,jarvis_towingincluded,jarvis_trailerincluded,modifiedon,jarvis_validfrom,jarvis_validuntil,jarvis_paymenttype", string.Format("_jarvis_customer_value eq '{0}'", DynamicsApiHelper.GetStringValueFromJObject(caseObj, "_customerid_value")), true);
                Customer_whitelist = Customer_whitelist.Where(w => DynamicsApiHelper.GetDateTimeValueFromJObject(w, "jarvis_validfrom") == null || (caseCreatedDate >= (DateTime)DynamicsApiHelper.GetDateTimeValueFromJObject(w, "jarvis_validfrom"))).ToList();
                Customer_whitelist = Customer_whitelist.Where(w => DynamicsApiHelper.GetDateTimeValueFromJObject(w, "jarvis_validuntil") == null || (caseCreatedDate <= (DateTime)DynamicsApiHelper.GetDateTimeValueFromJObject(w, "jarvis_validuntil"))).ToList();
                // get Whitelist for the case Home Dealer
                ICollection<JObject> Dealer_whitelist = this.dynamicsClient.RetrieveResultSetByFilter("jarvis_whitelists", "jarvis_whitelisttype,_jarvis_customer_value,_jarvis_vehicle_value,_jarvis_dealer_value,jarvis_maximumamount,_transactioncurrencyid_value,jarvis_towingincluded,jarvis_trailerincluded,modifiedon,jarvis_validfrom,jarvis_validuntil,jarvis_paymenttype", string.Format("_jarvis_dealer_value eq '{0}'", DynamicsApiHelper.GetStringValueFromJObject(caseObj, "_jarvis_homedealer_value")), true);
                Dealer_whitelist = Dealer_whitelist.Where(w => DynamicsApiHelper.GetDateTimeValueFromJObject(w, "jarvis_validfrom") == null || (caseCreatedDate >= (DateTime)DynamicsApiHelper.GetDateTimeValueFromJObject(w, "jarvis_validfrom"))).ToList();
                Dealer_whitelist = Dealer_whitelist.Where(w => DynamicsApiHelper.GetDateTimeValueFromJObject(w, "jarvis_validuntil") == null || (caseCreatedDate <= (DateTime)DynamicsApiHelper.GetDateTimeValueFromJObject(w, "jarvis_validuntil"))).ToList();

                if (Vehicle_whitelist.Count > 0 || Customer_whitelist.Count > 0 || Dealer_whitelist.Count > 0)
                {
                    var whitelistobj = new JObject();

                    // "Vehicle"
                    if (Vehicle_whitelist.Count > 0)
                    {
                        var DealervehicleTypes = Vehicle_whitelist.Where(w => (DynamicsApiHelper.GetStringValueFromJObject(w, "_jarvis_dealer_value") == DynamicsApiHelper.GetStringValueFromJObject(caseObj, "_jarvis_homedealer_value"))).OrderByDescending(d => DynamicsApiHelper.GetDateTimeValueFromJObject(d, "modifiedon")).ToList();
                        // "All Vehicles, Volvo & Renault only"
                        //var Vehicle_whitelist_AllVehicles_volvo_Renault = Vehicle_whitelist.Where(w => (DynamicsApiHelper.GetIntegerValueFromJObject(w, "jarvis_whitelisttype") == 334030001 && DynamicsApiHelper.GetStringValueFromJObject(caseObj, "_customerid_value") == DynamicsApiHelper.GetStringValueFromJObject(w, "_jarvis_customer_value"))).OrderByDescending(d => DynamicsApiHelper.GetDateTimeValueFromJObject(d, "modifiedon")).ToList();
                        // "All Vehicles, All Brands"
                        //var Vehicle_whitelist_AllVehicles_AllBrands = Vehicle_whitelist.Where(w => (DynamicsApiHelper.GetIntegerValueFromJObject(w, "jarvis_whitelisttype") == 334030000 && DynamicsApiHelper.GetStringValueFromJObject(caseObj, "_customerid_value") == DynamicsApiHelper.GetStringValueFromJObject(w, "_jarvis_customer_value"))).OrderByDescending(d => DynamicsApiHelper.GetDateTimeValueFromJObject(d, "modifiedon")).ToList();

                        // Whielist with same vehicle and HD
                        if (DealervehicleTypes.Count > 0)
                        {
                            ConditionSatisfied = true;
                            whitelistobj = DealervehicleTypes.FirstOrDefault();
                        }
                        else
                        {
                            ConditionSatisfied = true;
                            whitelistobj = Vehicle_whitelist.OrderByDescending(d => DynamicsApiHelper.GetDateTimeValueFromJObject(d, "modifiedon")).ToList().FirstOrDefault();
                        }
                        /*else if (Vehicle_whitelist_AllVehicles_volvo_Renault.Count > 0 && (matchedCondition.brand.ToLower().Contains("renault") || matchedCondition.brand.ToLower().Contains("volvo")))
                        {
                            ConditionSatisfied = true;
                            whitelistobj = Vehicle_whitelist_AllVehicles_volvo_Renault.FirstOrDefault();
                        }
                        // "All Vehicles, All Brands"
                        else if (Vehicle_whitelist_AllVehicles_AllBrands.Count > 0)
                        {
                            ConditionSatisfied = true;
                            whitelistobj = Vehicle_whitelist_AllVehicles_AllBrands.FirstOrDefault();
                        }*/

                    }

                    // "Customer"
                    else if (Customer_whitelist.Count > 0)
                    {
                        // "All Vehicles, Volvo & Renault only"
                        var Customer_whitelist_AllVehicles_volvo_Renault = Customer_whitelist.Where(w => (DynamicsApiHelper.GetIntegerValueFromJObject(w, "jarvis_whitelisttype") == 334030001 && DynamicsApiHelper.GetStringValueFromJObject(caseObj, "_customerid_value") == DynamicsApiHelper.GetStringValueFromJObject(w, "_jarvis_customer_value"))).OrderByDescending(d => DynamicsApiHelper.GetDateTimeValueFromJObject(d, "modifiedon")).ToList();
                        // "All Vehicles, All Brands"
                        var Customer_whitelist_AllVehicles_AllBrands = Customer_whitelist.Where(w => (DynamicsApiHelper.GetIntegerValueFromJObject(w, "jarvis_whitelisttype") == 334030000 && DynamicsApiHelper.GetStringValueFromJObject(caseObj, "_customerid_value") == DynamicsApiHelper.GetStringValueFromJObject(w, "_jarvis_customer_value"))).OrderByDescending(d => DynamicsApiHelper.GetDateTimeValueFromJObject(d, "modifiedon")).ToList();

                        if (Customer_whitelist.Count == 1)
                        {
                            ConditionSatisfied = true;
                            whitelistobj = Customer_whitelist.FirstOrDefault();
                        }
                        else if (Customer_whitelist_AllVehicles_volvo_Renault.Count > 0 && (matchedCondition.brand.ToLower().Contains("renault") || matchedCondition.brand.ToLower().Contains("volvo")))
                        {
                            ConditionSatisfied = true;
                            var customerMatchHd = Customer_whitelist_AllVehicles_volvo_Renault.Where(w => (DynamicsApiHelper.GetStringValueFromJObject(w, "_jarvis_dealer_value") == DynamicsApiHelper.GetStringValueFromJObject(caseObj, "_jarvis_homedealer_value"))).OrderByDescending(d => DynamicsApiHelper.GetDateTimeValueFromJObject(d, "modifiedon")).ToList();
                            if (customerMatchHd.Count > 0)
                                whitelistobj = customerMatchHd.FirstOrDefault();
                            else
                                whitelistobj = Customer_whitelist_AllVehicles_volvo_Renault.FirstOrDefault();
                        }
                        // "All Vehicles, All Brands"
                        else if (Customer_whitelist_AllVehicles_AllBrands.Count > 0)
                        {
                            ConditionSatisfied = true;
                            var customerMatchHd = Customer_whitelist_AllVehicles_AllBrands.Where(w => (DynamicsApiHelper.GetStringValueFromJObject(w, "_jarvis_dealer_value") == DynamicsApiHelper.GetStringValueFromJObject(caseObj, "_jarvis_homedealer_value"))).OrderByDescending(d => DynamicsApiHelper.GetDateTimeValueFromJObject(d, "modifiedon")).ToList();
                            if (customerMatchHd.Count > 0)
                                whitelistobj = customerMatchHd.FirstOrDefault();
                            else
                                whitelistobj = Customer_whitelist_AllVehicles_AllBrands.FirstOrDefault();
                        }
                        else
                        {
                            if (Dealer_whitelist.Count > 0)
                            {
                                // "All Customers, All Vehicles"
                                var AllVehicles_AllCustomers = Dealer_whitelist.Where(w => (DynamicsApiHelper.GetIntegerValueFromJObject(w, "jarvis_whitelisttype") == 334030003)).OrderByDescending(d => DynamicsApiHelper.GetDateTimeValueFromJObject(d, "modifiedon")).ToList();
                                if (AllVehicles_AllCustomers.Count > 0)
                                {
                                    ConditionSatisfied = true;
                                    whitelistobj = AllVehicles_AllCustomers.FirstOrDefault();
                                }
                            }
                        }

                    }
                    // "Dealer Condition"
                    else if (Dealer_whitelist.Count > 0)
                    {
                        // "All Customers, All Vehicles"
                        var AllVehicles_AllCustomers = Dealer_whitelist.Where(w => (DynamicsApiHelper.GetIntegerValueFromJObject(w, "jarvis_whitelisttype") == 334030003)).OrderByDescending(d => DynamicsApiHelper.GetDateTimeValueFromJObject(d, "modifiedon")).ToList();
                        if (AllVehicles_AllCustomers.Count > 0)
                        {
                            ConditionSatisfied = true;
                            whitelistobj = AllVehicles_AllCustomers.FirstOrDefault();
                        }
                    }

                    if (ConditionSatisfied)
                    {
                        GOPInit GOPObj = new GOPInit();
                        GOPObj.Amount = DynamicsApiHelper.GetDecimalValueFromJObject(whitelistobj, "jarvis_maximumamount");
                        GOPObj.Currency = DynamicsApiHelper.GetStringValueFromJObject(whitelistobj, "_transactioncurrencyid_value");
                        GOPObj.dealerId = DynamicsApiHelper.GetStringValueFromJObject(whitelistobj, "_jarvis_dealer_value");
                        GOPObj.PaymentType = (DynamicsApiHelper.GetIntegerValueFromJObject(whitelistobj, "jarvis_paymenttype") != null ? DynamicsApiHelper.GetIntegerValueFromJObject(whitelistobj, "jarvis_paymenttype"): 334030006);
                        matchedCondition.lineage += "-" + DynamicsApiHelper.GetStringValueFromJObject(whitelistobj, "jarvis_whitelistid");
                        bool isWhitelistTowingIncluded = (DynamicsApiHelper.GetIntegerValueFromJObject(whitelistobj, "jarvis_towingincluded") == 334030000) ? true : false;
                        bool isWhitelistTrailerIncluded = (DynamicsApiHelper.GetIntegerValueFromJObject(whitelistobj, "jarvis_trailerincluded") == 334030000) ? true : false;
                        if (!matchedCondition.disableAutoapprove)
                        {
                            if (matchedCondition.isTowingIncluded && !matchedCondition.isTrailerIncluded)
                            {
                                if (isWhitelistTowingIncluded)
                                {
                                    GOPObj.isApproved = (int)GopApproval.Approved;
                                    GOPObj.approvaldate = DateTime.UtcNow;
                                    GOPObj.Contact = "Automated GOP via Whitelisting";
                                }
                                else
                                {
                                    GOPObj.isApproved = (int)GopApproval.Pending;
                                }
                                CreateGOP(GOPObj, matchedCondition);
                                return true;
                            }
                            else if (matchedCondition.isTrailerIncluded && !matchedCondition.isTowingIncluded)
                            {
                                if (isWhitelistTrailerIncluded)
                                {
                                    GOPObj.isApproved = (int)GopApproval.Approved;
                                    GOPObj.approvaldate = DateTime.UtcNow;
                                    GOPObj.Contact = "Automated GOP via Whitelisting";
                                }
                                else
                                {
                                    GOPObj.isApproved = (int)GopApproval.Pending;
                                }
                                CreateGOP(GOPObj, matchedCondition);
                                return true;
                            }
                            else if (matchedCondition.isTowingIncluded && matchedCondition.isTrailerIncluded)
                            {
                                if (isWhitelistTrailerIncluded && isWhitelistTowingIncluded)
                                {
                                    GOPObj.isApproved = (int)GopApproval.Approved;
                                    GOPObj.approvaldate = DateTime.UtcNow;
                                    GOPObj.Contact = "Automated GOP via Whitelisting";
                                }
                                else
                                {
                                    GOPObj.isApproved = (int)GopApproval.Pending;
                                }
                                CreateGOP(GOPObj, matchedCondition);
                                return true;
                            }
                            else if (!matchedCondition.isTowingIncluded && !matchedCondition.isTrailerIncluded)
                            {
                                // Bug #602433 - Whitelist GOP not auto-approved
                                GOPObj.isApproved = (int)GopApproval.Approved;
                                GOPObj.approvaldate = DateTime.UtcNow;
                                GOPObj.Contact = "Automated GOP via Whitelisting";

                                CreateGOP(GOPObj, matchedCondition);
                                return true;
                            }
                        }
                        else
                        {
                            GOPObj.isApproved = (int)GopApproval.Pending;
                            CreateGOP(GOPObj, matchedCondition);
                            return true;

                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }

                return ConditionSatisfied;

            }

            catch (Exception ex)
            {
                //throw new ArgumentException($"whitelistSubProcess:" + ex.Message);
                //log.LogInformation($"Exception in whitelistSubProcess " + ex.Message);
                return false;
            }
        }


        public bool warrantySubProcess(dynamic caseObj, dynamic vehicleObj, ConditionLink matchedCondition)
        {
            bool isSucessful = false;
            bool ConditionSatisfied = false;
            double casewarranty = 0;
            try
            {
                // calculate the warranty year of the Vehicle from Delivery Date
                DateTime? vehicleDeliveryDate = (DynamicsApiHelper.GetDateTimeValueFromJObject(vehicleObj, "jarvis_deliverydate") != null) ? (DateTime)DynamicsApiHelper.GetDateTimeValueFromJObject(vehicleObj, "jarvis_deliverydate") : null;
                DateTime caseCreatedDate = DynamicsApiHelper.GetDateTimeValueFromJObject(caseObj, "createdon");
                var BrandId = DynamicsApiHelper.GetStringValueFromJObject(vehicleObj, "_jarvis_brandid_value");
                //var CountryofOperation = DynamicsApiHelper.GetStringValueFromJObject(vehicleObj, "_jarvis_countryofoperation_value");
                var CountryofOperation = matchedCondition.CaseHdCountryId;
                Decimal MileageLimit = (DynamicsApiHelper.GetDecimalValueFromJObject(caseObj, "jarvis_mileage") != null) ? DynamicsApiHelper.GetDecimalValueFromJObject(caseObj, "jarvis_mileage") : 0;
                var VehicleModel = DynamicsApiHelper.GetStringValueFromJObject(vehicleObj, "jarvis_aftermarketmodel");
                var caseLocation = DynamicsApiHelper.GetIntegerValueFromJObject(caseObj, "jarvis_caselocation");
                if (vehicleDeliveryDate != null)
                {
                    casewarranty = Math.Ceiling((caseCreatedDate.Subtract(vehicleDeliveryDate.GetValueOrDefault()).TotalDays) / 365);
                }
                else
                {
                    casewarranty = 500;
                }
                var warrantyList = this.dynamicsClient.RetrieveTopRecordsFromFetchXml("jarvis_warranties", string.Format(GOPFlow.CaseWarranty, CountryofOperation, BrandId, MileageLimit, casewarranty, VehicleModel, caseLocation), true);
                if (warrantyList != null && warrantyList.Count > 0)
                {
                    var warrantyObj = warrantyList[0];
                    ConditionSatisfied = true;
                    /*var caseLocation = DynamicsApiHelper.GetIntegerValueFromJObject(caseObj, "jarvis_caselocation");
                    if ((caseLocation != null) && (caseLocation == 334030002))
                    {
                        if (DynamicsApiHelper.GetIntegerValueFromJObject(warrantyObj, "jarvis_caselocation") == 334030002)
                        {
                            ConditionSatisfied = true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        ConditionSatisfied = true;
                    }*/

                    GOPInit GOPObj = new GOPInit();
                    GOPObj.Amount = DynamicsApiHelper.GetDecimalValueFromJObject(warrantyObj, "jarvis_gopamount");
                    GOPObj.Currency = DynamicsApiHelper.GetStringValueFromJObject(warrantyObj, "_transactioncurrencyid_value");
                    GOPObj.dealerId = DynamicsApiHelper.GetStringValueFromJObject(caseObj, "_jarvis_homedealer_value");
                    matchedCondition.lineage += "-" + DynamicsApiHelper.GetStringValueFromJObject(warrantyObj, "jarvis_warrantyid");
                    bool isAutoApproved = (bool)DynamicsApiHelper.GetBoolValueFromJObject(warrantyObj, "jarvis_gopautoapproved");
                    if (matchedCondition.brand.ToLower().Contains("renault"))
                        GOPObj.PaymentType = (DynamicsApiHelper.GetIntegerValueFromJObject(warrantyObj, "jarvis_paymenttype") != null ? DynamicsApiHelper.GetIntegerValueFromJObject(warrantyObj, "jarvis_paymenttype") : 334030008);
                    else
                        GOPObj.PaymentType = (DynamicsApiHelper.GetIntegerValueFromJObject(warrantyObj, "jarvis_paymenttype") != null ? DynamicsApiHelper.GetIntegerValueFromJObject(warrantyObj, "jarvis_paymenttype") : 334030005); ;
                    if (isAutoApproved && !matchedCondition.disableAutoapprove)
                    {
                        GOPObj.isApproved = (int)GopApproval.Approved;
                        GOPObj.approvaldate = DateTime.UtcNow;
                        GOPObj.Contact = "Automated GOP via Warranty";
                    }
                    else
                    {
                        GOPObj.isApproved = (int)GopApproval.Pending;
                    }
                    CreateGOP(GOPObj, matchedCondition);
                    return true;
                }
                else
                {
                    return false;
                }
                return isSucessful;
            }

            catch (Exception ex)
            {
                return false;
            }
        }

        public bool ContractSubProcess(dynamic caseObj, dynamic vehicleObj, ConditionLink matchedCondition)
        {
            bool isSucessful = false;
            try
            {
                var VehicleId = DynamicsApiHelper.GetStringValueFromJObject(caseObj, "_jarvis_vehicle_value");
                var BrandId = DynamicsApiHelper.GetStringValueFromJObject(vehicleObj, "_jarvis_brandid_value");
                DateTime caseCreatedDate = DynamicsApiHelper.GetDateTimeValueFromJObject(caseObj, "createdon");
                Decimal MileageLimit = (DynamicsApiHelper.GetDecimalValueFromJObject(caseObj, "jarvis_mileage") != null) ? DynamicsApiHelper.GetDecimalValueFromJObject(caseObj, "jarvis_mileage") : 0;
                var contractList = this.dynamicsClient.RetrieveTopRecordsFromFetchXml("jarvis_vehiclecontracts", string.Format(GOPFlow.Vehiclecontracts, VehicleId, caseCreatedDate.ToString("MM/dd/yyyy"), caseCreatedDate.ToString("MM/dd/yyyy"), matchedCondition.CaseId, MileageLimit, MileageLimit), true);
                if (contractList != null && contractList.Count > 0)
                {
                    var vehicleContractObj = contractList[0];
                    var Contracttype = DynamicsApiHelper.GetIntegerValueFromJObject(vehicleContractObj, "jarvis_onecasecontracttype");
                    //var ContractCountry = DynamicsApiHelper.GetStringValueFromJObject(vehicleContractObj, "_jarvis_country_value");
                    var ContractCountry = matchedCondition.CaseHdCountryId;
                    var caseLocation = DynamicsApiHelper.GetIntegerValueFromJObject(caseObj, "jarvis_caselocation");
                    // Get the Associated Gop Contracts
                    ICollection<JObject> GOPcontractList = this.dynamicsClient.RetrieveResultSetByFilter("jarvis_contracts", "jarvis_gopamount,_transactioncurrencyid_value,jarvis_gopautoapproved,jarvis_paymenttype", string.Format("_jarvis_brand_value eq '{0}' and _jarvis_country_value eq '{1}'  and jarvis_caselocation eq {2} and jarvis_type eq {3}", BrandId, ContractCountry, caseLocation, Contracttype), true);
                    if (GOPcontractList != null && GOPcontractList.Count > 0)
                    {
                        var ContractObj = GOPcontractList?.FirstOrDefault();
                        GOPInit GOPObj = new GOPInit();
                        GOPObj.Amount = DynamicsApiHelper.GetDecimalValueFromJObject(ContractObj, "jarvis_gopamount");
                        GOPObj.Currency = DynamicsApiHelper.GetStringValueFromJObject(ContractObj, "_transactioncurrencyid_value");
                        GOPObj.dealerId = DynamicsApiHelper.GetStringValueFromJObject(caseObj, "_jarvis_homedealer_value");
                        matchedCondition.lineage += "-" + DynamicsApiHelper.GetStringValueFromJObject(ContractObj, "jarvis_contractid");
                        bool isAutoApproved = (bool)DynamicsApiHelper.GetBoolValueFromJObject(ContractObj, "jarvis_gopautoapproved");
                        if (matchedCondition.brand.ToLower().Contains("renault"))
                            GOPObj.PaymentType = (DynamicsApiHelper.GetIntegerValueFromJObject(ContractObj, "jarvis_paymenttype") != null ? DynamicsApiHelper.GetIntegerValueFromJObject(ContractObj, "jarvis_paymenttype") : 334030008);
                        else
                            GOPObj.PaymentType = (DynamicsApiHelper.GetIntegerValueFromJObject(ContractObj, "jarvis_paymenttype") != null ? DynamicsApiHelper.GetIntegerValueFromJObject(ContractObj, "jarvis_paymenttype") : 334030001);
                        if (isAutoApproved && !matchedCondition.disableAutoapprove)
                        {
                            GOPObj.isApproved = (int)GopApproval.Approved;
                            GOPObj.approvaldate = DateTime.UtcNow;
                            GOPObj.Contact = "Automated GOP via Contract";
                        }
                        else
                        {
                            GOPObj.isApproved = (int)GopApproval.Pending;
                        }
                        CreateGOP(GOPObj, matchedCondition);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
                return isSucessful;

            }

            catch (Exception ex)
            {
                return false;
            }
        }


        public bool GOPstandardSubProcess(dynamic caseObj, dynamic vehicleObj, ConditionLink matchedCondition)
        {
            bool isSucessful = false;
            try
            {
                /// caseCountry,Vehicle.BrandId and CaseLocation.
                ////var countryofOperation = DynamicsApiHelper.GetStringValueFromJObject(vehicleObj, "_jarvis_countryofoperation_value");
                ////var countryObj = this.dynamicsClient.RetrieveEntityById("jarvis_countries", countryofOperation);
                var BrandId = DynamicsApiHelper.GetStringValueFromJObject(vehicleObj, "_jarvis_brandid_value");
                var caseLocation = DynamicsApiHelper.GetIntegerValueFromJObject(caseObj, "jarvis_caselocation");
                //var caseCountry = DynamicsApiHelper.GetStringValueFromJObject(caseObj, "_jarvis_country_value");
                var caseCountry = matchedCondition.CaseHdCountryId;
                /// Retrieve HD currency for conversion.
                var caseHomeDealer = DynamicsApiHelper.GetStringValueFromJObject(caseObj, "_jarvis_homedealer_value");

                string[] columns = new string[] { "_jarvis_currency_value" };
                var caseHomeDealerObj = this.dynamicsClient.RetrieveEntityById("accounts", caseHomeDealer, columns);
                var homeDealerCurrency = DynamicsApiHelper.GetStringValueFromJObject(caseHomeDealerObj, "_jarvis_currency_value");
                var defaultEuroCurrency = this.dynamicsClient.RetrieveResultSetByFilter("transactioncurrencies", "transactioncurrencyid", "isocurrencycode eq 'EUR' and statecode eq 0").FirstOrDefault();

                if (!string.IsNullOrEmpty(caseCountry) && !string.IsNullOrEmpty(BrandId) && caseLocation != null)
                {
                    //// Retrieved the Gop Standards based on caseCountry, CaseLocation and Vehicle.BrandId. ++sorting by last modifed by
                    ICollection<JObject> gopStandardList = this.dynamicsClient.RetrieveResultSetByFilter("jarvis_standardgops", "_transactioncurrencyid_value,jarvis_paymenttype,jarvis_gopamount&$orderby=_modifiedby_value desc", string.Format("_jarvis_casecountry_value eq {0} and _jarvis_vehiclebrand_value eq {1} and jarvis_caselocation eq {2}", Guid.Parse(caseCountry), Guid.Parse(BrandId), caseLocation));
                    if (gopStandardList != null && gopStandardList.Count > 0)
                    {
                        var gopStandardRecord = gopStandardList.FirstOrDefault();
                        ///CreateGop
                        GOPInit GOPObj = new();
                        Decimal gopStandardAmount = (decimal)((DynamicsApiHelper.GetDecimalValueFromJObject(gopStandardRecord, "jarvis_gopamount") != null) ? DynamicsApiHelper.GetDecimalValueFromJObject(gopStandardRecord, "jarvis_gopamount") : 0);
                        string gopStandardCurrency = DynamicsApiHelper.GetStringValueFromJObject(gopStandardRecord, "_transactioncurrencyid_value");
                        gopStandardAmount *= CurrencyExchange(Guid.Parse(gopStandardCurrency), Guid.Parse(!string.IsNullOrEmpty(homeDealerCurrency) ? homeDealerCurrency : gopStandardCurrency));
                        GOPObj.Amount = gopStandardAmount;
                        GOPObj.Currency = !string.IsNullOrEmpty(homeDealerCurrency) ? homeDealerCurrency : gopStandardCurrency;
                        GOPObj.dealerId = DynamicsApiHelper.GetStringValueFromJObject(caseObj, "_jarvis_homedealer_value");
                        GOPObj.isApproved = (int)GopApproval.Pending;
                        GOPObj.PaymentType = (DynamicsApiHelper.GetIntegerValueFromJObject(gopStandardRecord, "jarvis_paymenttype") != null ? DynamicsApiHelper.GetIntegerValueFromJObject(gopStandardRecord, "jarvis_paymenttype") : 334030003);
                        matchedCondition.lineage += "-" + DynamicsApiHelper.GetStringValueFromJObject(gopStandardRecord, "jarvis_standardgopid");
                        CreateGOP(GOPObj, matchedCondition);
                        return true;
                    }
                    else
                    {
                        ///CreateGop with amount 0.
                        GOPInit GOPObj = new GOPInit
                        {
                            Amount = 0,
                            Currency = !string.IsNullOrEmpty(homeDealerCurrency) ? homeDealerCurrency : DynamicsApiHelper.GetStringValueFromJObject(defaultEuroCurrency, "transactioncurrencyid"),
                            dealerId = DynamicsApiHelper.GetStringValueFromJObject(caseObj, "_jarvis_homedealer_value"),
                            isApproved = (int)GopApproval.Pending,
                            PaymentType = 334030003
                        };
                        CreateGOP(GOPObj, matchedCondition);
                        return true;
                    }
                }
                else
                {
                    //// CreateGop with amount 0.
                    GOPInit GOPObj = new()
                    {
                        Amount = 0,
                        Currency = !string.IsNullOrEmpty(homeDealerCurrency) ? homeDealerCurrency : DynamicsApiHelper.GetStringValueFromJObject(defaultEuroCurrency, "transactioncurrencyid"),
                        dealerId = DynamicsApiHelper.GetStringValueFromJObject(caseObj, "_jarvis_homedealer_value"),
                        isApproved = (int)GopApproval.Pending,
                        PaymentType = 334030003
                    };
                    CreateGOP(GOPObj, matchedCondition);
                    return true;
                }


                ////Old Logic of Gop
                //if (countryObj != null)
                //{
                //    GOPInit GOPObj = new GOPInit();
                //    GOPObj.Amount = DynamicsApiHelper.GetDecimalValueFromJObject(countryObj, "jarvis_gopstandardamount");
                //    GOPObj.Currency = DynamicsApiHelper.GetStringValueFromJObject(countryObj, "_transactioncurrencyid_value");
                //    GOPObj.dealerId = DynamicsApiHelper.GetStringValueFromJObject(caseObj, "_jarvis_homedealer_value");
                //    GOPObj.isApproved = false;
                //    GOPObj.PaymentType = 334030003;
                //    CreateGOP(GOPObj, matchedCondition);
                //    return true;
                //}
                return isSucessful;
            }

            catch (Exception ex)
            {
                throw new ArgumentException($"GOPstandardSubProcess:" + ex.Message);
            }
        }

        #region GOPHDRD
        public void GOPHDRDSubProcess(dynamic caseObj, ConditionLink matchedCondition)
        {
            try
            {
                var HDRD = DynamicsApiHelper.GetBoolValueFromJObject(caseObj, "jarvis_hdrd"); // HD=RD
                var Fleetcustomer = DynamicsApiHelper.GetBoolValueFromJObject(caseObj, "jarvis_fleetcustomer"); // Fleetcus
                var caseHomeDealer = DynamicsApiHelper.GetStringValueFromJObject(caseObj, "_jarvis_homedealer_value");
                string[] columns = new string[] { "_jarvis_currency_value" };
                var caseHomeDealerObj = this.dynamicsClient.RetrieveEntityById("accounts", caseHomeDealer, columns);
                var homeDealerCurrency = DynamicsApiHelper.GetStringValueFromJObject(caseHomeDealerObj, "_jarvis_currency_value");
                var defaultEuroCurrency = this.dynamicsClient.RetrieveResultSetByFilter("transactioncurrencies", "transactioncurrencyid", "isocurrencycode eq 'EUR' and statecode eq 0").FirstOrDefault();

                if (HDRD == true && (Fleetcustomer == true || Fleetcustomer == false))
                {
                    GOPInit GOPObj = new GOPInit
                    {
                        Amount = 0,
                        Currency = !string.IsNullOrEmpty(homeDealerCurrency) ? homeDealerCurrency : DynamicsApiHelper.GetStringValueFromJObject(defaultEuroCurrency, "transactioncurrencyid"),
                        dealerId = DynamicsApiHelper.GetStringValueFromJObject(caseObj, "_jarvis_homedealer_value"),
                        isApproved = (int)GopApproval.Approved,
                        approvaldate = DateTime.UtcNow,
                        PaymentType = 334030007, // HD=RD
                        Contact = "HD=RD",
                        statuscode = matchedCondition.statuscode
                    };
                    CreateGOP(GOPObj, matchedCondition);
                }
                else if (HDRD == false && Fleetcustomer == true)
                {
                    GOPInit GOPObj = new GOPInit
                    {
                        Amount = 0,
                        Currency = !string.IsNullOrEmpty(homeDealerCurrency) ? homeDealerCurrency : DynamicsApiHelper.GetStringValueFromJObject(defaultEuroCurrency, "transactioncurrencyid"),
                        dealerId = DynamicsApiHelper.GetStringValueFromJObject(caseObj, "_jarvis_homedealer_value"),
                        isApproved = (int)GopApproval.Approved,
                        approvaldate = DateTime.UtcNow,
                        PaymentType = 334030009,// National/group Customer
                        Contact = "National/group Customer",
                        statuscode = matchedCondition.statuscode
                    };
                    CreateGOP(GOPObj, matchedCondition);
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"GOPHDRDSubProcess:" + ex.Message);
            }
        }
        #endregion

        public void PassOutCreationHDRD(dynamic caseObj, ConditionLink matchedCondition)
        {
            try
            {
                var HDRD = DynamicsApiHelper.GetBoolValueFromJObject(caseObj, "jarvis_hdrd"); // HD=RD
                var Fleetcustomer = DynamicsApiHelper.GetBoolValueFromJObject(caseObj, "jarvis_fleetcustomer"); // Fleet cut
                var caseHomeDealer = DynamicsApiHelper.GetStringValueFromJObject(caseObj, "_jarvis_homedealer_value");
                string[] columns = new string[] { "_jarvis_currency_value" };
                var caseHomeDealerObj = this.dynamicsClient.RetrieveEntityById("accounts", caseHomeDealer, columns);
                var homeDealerCurrency = DynamicsApiHelper.GetStringValueFromJObject(caseHomeDealerObj, "_jarvis_currency_value");
                var defaultEuroCurrency = this.dynamicsClient.RetrieveResultSetByFilter("transactioncurrencies", "transactioncurrencyid", "isocurrencycode eq 'EUR' and statecode eq 0").FirstOrDefault();
                if (HDRD == true && (Fleetcustomer == true || Fleetcustomer == false))
                {
                    PassOutInit PassOutObj = new PassOutInit
                    {
                        Amount = 0,
                        Currency = !string.IsNullOrEmpty(homeDealerCurrency) ? homeDealerCurrency : DynamicsApiHelper.GetStringValueFromJObject(defaultEuroCurrency, "transactioncurrencyid"),
                        dealerId = DynamicsApiHelper.GetStringValueFromJObject(caseObj, "_jarvis_homedealer_value"),
                        PaymentType = 334030007,// HD=RD
                        Contact = "HD=RD",
                        statuscode = 1
                    };
                    CreatePassOut(PassOutObj, matchedCondition);
                }
                else if (HDRD == false && Fleetcustomer == true)
                {
                    PassOutInit PassOutObj = new PassOutInit
                    {
                        Amount = 0,
                        Currency = !string.IsNullOrEmpty(homeDealerCurrency) ? homeDealerCurrency : DynamicsApiHelper.GetStringValueFromJObject(defaultEuroCurrency, "transactioncurrencyid"),
                        dealerId = DynamicsApiHelper.GetStringValueFromJObject(caseObj, "_jarvis_homedealer_value"),
                        PaymentType = 334030009,// National Group/Customer 
                        Contact = "National Group/Customer",
                        statuscode = 1
                    };
                    CreatePassOut(PassOutObj, matchedCondition);
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"PassOutCreationHDRD:" + ex.Message);
            }
        }

        #region CREATE-PassOut
        private void CreatePassOut(PassOutInit PassOutPayload, ConditionLink matchedCondition)
        {
            try
            {
                if (PassOutPayload != null)
                {
                    JObject payload = new JObject();
                    payload.Add("jarvis_paymenttype", PassOutPayload.PaymentType); //
                    payload.Add("jarvis_goplimitout", PassOutPayload.Amount);
                    payload.Add("jarvis_contact", PassOutPayload.Contact);
                    payload.Add("jarvis_Incident@odata.bind", string.Format(" /{0}({1})", "incidents", matchedCondition.CaseId));
                    payload.Add("jarvis_RepairingDealer@odata.bind", string.Format(" /{0}({1})", "accounts", PassOutPayload.dealerId));
                    payload.Add("transactioncurrencyid@odata.bind", "/transactioncurrencies(" + PassOutPayload.Currency + ")");
                    payload.Add("statuscode", PassOutPayload.statuscode);
                    this.dynamicsClient.CreateEntity("jarvis_passouts", payload);
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"CreatePassOut:" + ex.Message);
            }
        }
        #endregion

        public bool whitelistSubProcess_old(dynamic caseObj, dynamic vehicleObj, ConditionLink matchedCondition)
        {
            bool isSucessful = false;
            bool ConditionSatisfied = false;
            try
            {
                var casedealer = DynamicsApiHelper.GetStringValueFromJObject(caseObj, "_jarvis_homedealer_value");
                ICollection<JObject> whitelist = this.dynamicsClient.RetrieveResultSetByFilter("jarvis_whitelists", "jarvis_whitelisttype,_jarvis_customer_value,_jarvis_vehicle_value,jarvis_maximumamount,_transactioncurrencyid_value,jarvis_towingincluded,jarvis_trailerincluded", string.Format("_jarvis_dealer_value eq '{0}'", casedealer), true);

                if (whitelist != null && whitelist.Count > 0)
                {
                    var whitelistobj = new JObject();
                    // "Vehicle"
                    var vehicleTypes = whitelist.Where(w => (DynamicsApiHelper.GetIntegerValueFromJObject(w, "jarvis_whitelisttype") == 334030002 && DynamicsApiHelper.GetStringValueFromJObject(caseObj, "_jarvis_vehicle_value") == DynamicsApiHelper.GetStringValueFromJObject(w, "_jarvis_vehicle_value"))).ToList();
                    // "All Vehicles, Volvo & Renault only"
                    var AllVehicles_volvo_Renault = whitelist.Where(w => (DynamicsApiHelper.GetIntegerValueFromJObject(w, "jarvis_whitelisttype") == 334030001 && DynamicsApiHelper.GetStringValueFromJObject(caseObj, "_customerid_value") == DynamicsApiHelper.GetStringValueFromJObject(w, "_jarvis_customer_value"))).ToList();
                    // "All Vehicles, All Brands"
                    var AllVehicles_AllBrands = whitelist.Where(w => (DynamicsApiHelper.GetIntegerValueFromJObject(w, "jarvis_whitelisttype") == 334030000 && DynamicsApiHelper.GetStringValueFromJObject(caseObj, "_customerid_value") == DynamicsApiHelper.GetStringValueFromJObject(w, "_jarvis_customer_value"))).ToList();
                    // "All Customers, All Vehicles"
                    var AllVehicles_AllCustomers = whitelist.Where(w => (DynamicsApiHelper.GetIntegerValueFromJObject(w, "jarvis_whitelisttype") == 334030003)).ToList();
                    // "Vehicle"
                    if (vehicleTypes.Count > 0)
                    {
                        ConditionSatisfied = true;
                        whitelistobj = vehicleTypes?.FirstOrDefault();
                    }
                    // "All Vehicles, Volvo & Renault only"
                    else if (AllVehicles_volvo_Renault.Count > 0 && (matchedCondition.brand.ToLower().Contains("renault") || matchedCondition.brand.ToLower().Contains("volvo")))
                    {
                        ConditionSatisfied = true;
                        whitelistobj = AllVehicles_volvo_Renault?.FirstOrDefault();
                    }
                    // "All Vehicles, All Brands"
                    else if (AllVehicles_AllBrands.Count > 0)
                    {
                        ConditionSatisfied = true;
                        whitelistobj = AllVehicles_AllBrands?.FirstOrDefault();
                    }
                    // "All Customers, All Vehicles"
                    else if (AllVehicles_AllCustomers.Count > 0)
                    {
                        ConditionSatisfied = true;
                        whitelistobj = AllVehicles_AllCustomers?.FirstOrDefault();
                    }

                    if (ConditionSatisfied)
                    {
                        GOPInit GOPObj = new GOPInit();
                        GOPObj.Amount = DynamicsApiHelper.GetDecimalValueFromJObject(whitelistobj, "jarvis_maximumamount");
                        GOPObj.Currency = DynamicsApiHelper.GetStringValueFromJObject(whitelistobj, "_transactioncurrencyid_value");
                        GOPObj.dealerId = casedealer;
                        //GOPObj.CaseId =
                        GOPObj.PaymentType = 334030006;
                        bool isWhitelistTowingIncluded = (DynamicsApiHelper.GetIntegerValueFromJObject(whitelistobj, "jarvis_towingincluded") == 334030000) ? true : false;
                        bool isWhitelistTrailerIncluded = (DynamicsApiHelper.GetIntegerValueFromJObject(whitelistobj, "jarvis_trailerincluded") == 334030000) ? true : false;
                        if (matchedCondition.isTowingIncluded && !matchedCondition.isTrailerIncluded)
                        {
                            if (isWhitelistTowingIncluded)
                            {
                                GOPObj.isApproved = (int)GopApproval.Approved;
                                GOPObj.approvaldate = DateTime.UtcNow;
                                GOPObj.Contact = "Automated GOP via Whitelisting";
                            }
                            else
                            {
                                GOPObj.isApproved = (int)GopApproval.Pending;
                            }
                            CreateGOP(GOPObj, matchedCondition);
                            return true;
                        }
                        else if (matchedCondition.isTrailerIncluded && !matchedCondition.isTowingIncluded)
                        {
                            if (isWhitelistTrailerIncluded)
                            {
                                GOPObj.isApproved = (int)GopApproval.Approved;
                                GOPObj.approvaldate = DateTime.UtcNow;
                                GOPObj.Contact = "Automated GOP via Whitelisting";
                            }
                            else
                            {
                                GOPObj.isApproved = (int)GopApproval.Pending;
                            }
                            CreateGOP(GOPObj, matchedCondition);
                            return true;
                        }
                        else if (matchedCondition.isTowingIncluded && matchedCondition.isTrailerIncluded)
                        {
                            if (isWhitelistTrailerIncluded && isWhitelistTowingIncluded)
                            {
                                GOPObj.isApproved = (int)GopApproval.Approved;
                                GOPObj.approvaldate = DateTime.UtcNow;
                                GOPObj.Contact = "Automated GOP via Whitelisting";
                            }
                            else
                            {
                                GOPObj.isApproved = (int)GopApproval.Pending;
                            }
                            CreateGOP(GOPObj, matchedCondition);
                            return true;
                        }
                        else if (!matchedCondition.isTowingIncluded && !matchedCondition.isTrailerIncluded)
                        {
                            GOPObj.isApproved = (int)GopApproval.Approved;
                            GOPObj.approvaldate = DateTime.UtcNow;
                            GOPObj.Contact = "Automated GOP via Whitelisting";
                            CreateGOP(GOPObj, matchedCondition);
                            return true;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }

                return ConditionSatisfied;

            }

            catch (Exception ex)
            {
                //throw new ArgumentException($"whitelistSubProcess:" + ex.Message);
                //log.LogInformation($"Exception in whitelistSubProcess " + ex.Message);
                return false;
            }
        }

        public decimal CurrencyExchange(Guid sourceCurrencyId, Guid targetCurrencyId)
        {
            decimal exchangeValue = 0;
            if (sourceCurrencyId == targetCurrencyId)
            {
                exchangeValue = 1;
            }
            else
            {
                var exchangeList = this.dynamicsClient.RetrieveTopRecordsFromFetchXml("jarvis_exchangerates", string.Format(GOPFlow.ExchangeRates, sourceCurrencyId, targetCurrencyId), true);
                foreach (var exchange in exchangeList)
                {
                    exchangeValue = (decimal)DynamicsApiHelper.GetDecimalValueFromJObject(exchange, "jarvis_value");
                }

            }
            return exchangeValue;
        }

        public bool whitelistHDSubProcess(dynamic caseObj, dynamic vehicleObj, ConditionLink matchedCondition)
        {
            bool isSucessful = false;
            bool ConditionSatisfied = false;
            var brandname = string.Empty;
            try
            {
                var BrandId = DynamicsApiHelper.GetStringValueFromJObject(vehicleObj, "_jarvis_brandid_value");
                var brandObj = this.dynamicsClient.RetrieveEntityById("jarvis_brands", BrandId.ToString());
                if (brandObj != null)
                {
                    brandname = DynamicsApiHelper.GetStringValueFromJObject(brandObj, "jarvis_name");
                }
                var casedealer = DynamicsApiHelper.GetStringValueFromJObject(caseObj, "_jarvis_homedealer_value");
                DateTime caseCreatedDate = DynamicsApiHelper.GetDateTimeValueFromJObject(caseObj, "createdon");
                // get Whitelist for the case Vehicle
                ICollection<JObject> Vehicle_whitelist = this.dynamicsClient.RetrieveResultSetByFilter("jarvis_whitelists", "jarvis_whitelisttype,_jarvis_customer_value,_jarvis_vehicle_value,_jarvis_dealer_value,jarvis_maximumamount,_transactioncurrencyid_value,jarvis_towingincluded,jarvis_trailerincluded,modifiedon,jarvis_validfrom,jarvis_validuntil,jarvis_paymenttype", string.Format("_jarvis_vehicle_value eq '{0}'", DynamicsApiHelper.GetStringValueFromJObject(caseObj, "_jarvis_vehicle_value")), true);
                Vehicle_whitelist = Vehicle_whitelist.Where(w => DynamicsApiHelper.GetDateTimeValueFromJObject(w, "jarvis_validfrom") == null || (caseCreatedDate >= (DateTime)DynamicsApiHelper.GetDateTimeValueFromJObject(w, "jarvis_validfrom"))).ToList();
                Vehicle_whitelist = Vehicle_whitelist.Where(w => DynamicsApiHelper.GetDateTimeValueFromJObject(w, "jarvis_validuntil") == null || (caseCreatedDate <= (DateTime)DynamicsApiHelper.GetDateTimeValueFromJObject(w, "jarvis_validuntil"))).ToList();
                // get Whitelist for the case Customer
                ICollection<JObject> Customer_whitelist = this.dynamicsClient.RetrieveResultSetByFilter("jarvis_whitelists", "jarvis_whitelisttype,_jarvis_customer_value,_jarvis_vehicle_value,_jarvis_dealer_value,jarvis_maximumamount,_transactioncurrencyid_value,jarvis_towingincluded,jarvis_trailerincluded,modifiedon,jarvis_validfrom,jarvis_validuntil,jarvis_paymenttype", string.Format("_jarvis_customer_value eq '{0}'", DynamicsApiHelper.GetStringValueFromJObject(caseObj, "_customerid_value")), true);
                Customer_whitelist = Customer_whitelist.Where(w => DynamicsApiHelper.GetDateTimeValueFromJObject(w, "jarvis_validfrom") == null || (caseCreatedDate >= (DateTime)DynamicsApiHelper.GetDateTimeValueFromJObject(w, "jarvis_validfrom"))).ToList();
                Customer_whitelist = Customer_whitelist.Where(w => DynamicsApiHelper.GetDateTimeValueFromJObject(w, "jarvis_validuntil") == null || (caseCreatedDate <= (DateTime)DynamicsApiHelper.GetDateTimeValueFromJObject(w, "jarvis_validuntil"))).ToList();
                // get Whitelist for the case Home Dealer
                ICollection<JObject> Dealer_whitelist = this.dynamicsClient.RetrieveResultSetByFilter("jarvis_whitelists", "jarvis_whitelisttype,_jarvis_customer_value,_jarvis_vehicle_value,_jarvis_dealer_value,jarvis_maximumamount,_transactioncurrencyid_value,jarvis_towingincluded,jarvis_trailerincluded,modifiedon,jarvis_validfrom,jarvis_validuntil,jarvis_paymenttype", string.Format("_jarvis_dealer_value eq '{0}'", DynamicsApiHelper.GetStringValueFromJObject(caseObj, "_jarvis_homedealer_value")), true);
                Dealer_whitelist = Dealer_whitelist.Where(w => DynamicsApiHelper.GetDateTimeValueFromJObject(w, "jarvis_validfrom") == null || (caseCreatedDate >= (DateTime)DynamicsApiHelper.GetDateTimeValueFromJObject(w, "jarvis_validfrom"))).ToList();
                Dealer_whitelist = Dealer_whitelist.Where(w => DynamicsApiHelper.GetDateTimeValueFromJObject(w, "jarvis_validuntil") == null || (caseCreatedDate <= (DateTime)DynamicsApiHelper.GetDateTimeValueFromJObject(w, "jarvis_validuntil"))).ToList();

                if (Vehicle_whitelist.Count > 0 || Customer_whitelist.Count > 0 || Dealer_whitelist.Count > 0)
                {
                    var whitelistobj = new JObject();

                    // "Vehicle"
                    if (Vehicle_whitelist.Count > 0)
                    {
                        var DealervehicleTypes = Vehicle_whitelist.Where(w => (DynamicsApiHelper.GetStringValueFromJObject(w, "_jarvis_dealer_value") == DynamicsApiHelper.GetStringValueFromJObject(caseObj, "_jarvis_homedealer_value"))).OrderByDescending(d => DynamicsApiHelper.GetDateTimeValueFromJObject(d, "modifiedon")).ToList();
                        // "All Vehicles, Volvo & Renault only"
                        //var Vehicle_whitelist_AllVehicles_volvo_Renault = Vehicle_whitelist.Where(w => (DynamicsApiHelper.GetIntegerValueFromJObject(w, "jarvis_whitelisttype") == 334030001 && DynamicsApiHelper.GetStringValueFromJObject(caseObj, "_customerid_value") == DynamicsApiHelper.GetStringValueFromJObject(w, "_jarvis_customer_value"))).OrderByDescending(d => DynamicsApiHelper.GetDateTimeValueFromJObject(d, "modifiedon")).ToList();
                        // "All Vehicles, All Brands"
                        //var Vehicle_whitelist_AllVehicles_AllBrands = Vehicle_whitelist.Where(w => (DynamicsApiHelper.GetIntegerValueFromJObject(w, "jarvis_whitelisttype") == 334030000 && DynamicsApiHelper.GetStringValueFromJObject(caseObj, "_customerid_value") == DynamicsApiHelper.GetStringValueFromJObject(w, "_jarvis_customer_value"))).OrderByDescending(d => DynamicsApiHelper.GetDateTimeValueFromJObject(d, "modifiedon")).ToList();

                        // Whielist with same vehicle and HD
                        if (DealervehicleTypes.Count > 0)
                        {
                            ConditionSatisfied = true;
                            whitelistobj = DealervehicleTypes?.FirstOrDefault();
                        }
                        else
                        {
                            ConditionSatisfied = true;
                            whitelistobj = Vehicle_whitelist.OrderByDescending(d => DynamicsApiHelper.GetDateTimeValueFromJObject(d, "modifiedon")).ToList().FirstOrDefault();
                        }

                    }

                    // "Customer"
                    else if (Customer_whitelist.Count > 0)
                    {
                        // "All Vehicles, Volvo & Renault only"
                        var Customer_whitelist_AllVehicles_volvo_Renault = Customer_whitelist.Where(w => (DynamicsApiHelper.GetIntegerValueFromJObject(w, "jarvis_whitelisttype") == 334030001 && DynamicsApiHelper.GetStringValueFromJObject(caseObj, "_customerid_value") == DynamicsApiHelper.GetStringValueFromJObject(w, "_jarvis_customer_value"))).OrderByDescending(d => DynamicsApiHelper.GetDateTimeValueFromJObject(d, "modifiedon")).ToList();
                        // "All Vehicles, All Brands"
                        var Customer_whitelist_AllVehicles_AllBrands = Customer_whitelist.Where(w => (DynamicsApiHelper.GetIntegerValueFromJObject(w, "jarvis_whitelisttype") == 334030000 && DynamicsApiHelper.GetStringValueFromJObject(caseObj, "_customerid_value") == DynamicsApiHelper.GetStringValueFromJObject(w, "_jarvis_customer_value"))).OrderByDescending(d => DynamicsApiHelper.GetDateTimeValueFromJObject(d, "modifiedon")).ToList();

                        if (Customer_whitelist.Count == 1)
                        {
                            ConditionSatisfied = true;
                            whitelistobj = Customer_whitelist?.FirstOrDefault();
                        }
                        else if (Customer_whitelist_AllVehicles_volvo_Renault.Count > 0 && (brandname.ToLower().Contains("renault") || brandname.ToLower().Contains("volvo")))
                        {
                            ConditionSatisfied = true;
                            var customerMatchHd = Customer_whitelist_AllVehicles_volvo_Renault.Where(w => (DynamicsApiHelper.GetStringValueFromJObject(w, "_jarvis_dealer_value") == DynamicsApiHelper.GetStringValueFromJObject(caseObj, "_jarvis_homedealer_value"))).OrderByDescending(d => DynamicsApiHelper.GetDateTimeValueFromJObject(d, "modifiedon")).ToList();
                            if (customerMatchHd.Count > 0)
                                whitelistobj = customerMatchHd?.FirstOrDefault();
                            else
                                whitelistobj = Customer_whitelist_AllVehicles_volvo_Renault?.FirstOrDefault();
                        }
                        // "All Vehicles, All Brands"
                        else if (Customer_whitelist_AllVehicles_AllBrands.Count > 0)
                        {
                            ConditionSatisfied = true;
                            var customerMatchHd = Customer_whitelist_AllVehicles_AllBrands.Where(w => (DynamicsApiHelper.GetStringValueFromJObject(w, "_jarvis_dealer_value") == DynamicsApiHelper.GetStringValueFromJObject(caseObj, "_jarvis_homedealer_value"))).OrderByDescending(d => DynamicsApiHelper.GetDateTimeValueFromJObject(d, "modifiedon")).ToList();
                            if (customerMatchHd.Count > 0)
                                whitelistobj = customerMatchHd?.FirstOrDefault();
                            else
                                whitelistobj = Customer_whitelist_AllVehicles_AllBrands?.FirstOrDefault();
                        }
                        else
                        {
                            if (Dealer_whitelist.Count > 0)
                            {
                                // "All Customers, All Vehicles"
                                var AllVehicles_AllCustomers = Dealer_whitelist.Where(w => (DynamicsApiHelper.GetIntegerValueFromJObject(w, "jarvis_whitelisttype") == 334030003)).OrderByDescending(d => DynamicsApiHelper.GetDateTimeValueFromJObject(d, "modifiedon")).ToList();
                                if (AllVehicles_AllCustomers.Count > 0)
                                {
                                    ConditionSatisfied = true;
                                    whitelistobj = AllVehicles_AllCustomers?.FirstOrDefault();
                                }
                            }
                        }

                    }
                    // "Dealer Condition"
                    else if (Dealer_whitelist.Count > 0)
                    {
                        // "All Customers, All Vehicles"
                        var AllVehicles_AllCustomers = Dealer_whitelist.Where(w => (DynamicsApiHelper.GetIntegerValueFromJObject(w, "jarvis_whitelisttype") == 334030003)).OrderByDescending(d => DynamicsApiHelper.GetDateTimeValueFromJObject(d, "modifiedon")).ToList();
                        if (AllVehicles_AllCustomers.Count > 0)
                        {
                            ConditionSatisfied = true;
                            whitelistobj = AllVehicles_AllCustomers?.FirstOrDefault();
                        }
                    }

                    if (ConditionSatisfied)
                    {
                        GOPInit GOPObj = new GOPInit();
                        GOPObj.Amount = DynamicsApiHelper.GetDecimalValueFromJObject(whitelistobj, "jarvis_maximumamount");
                        GOPObj.Currency = DynamicsApiHelper.GetStringValueFromJObject(whitelistobj, "_transactioncurrencyid_value");
                        GOPObj.dealerId = DynamicsApiHelper.GetStringValueFromJObject(whitelistobj, "_jarvis_dealer_value");
                        matchedCondition.lineage += "-" + DynamicsApiHelper.GetStringValueFromJObject(whitelistobj, "jarvis_whitelistid");
                        GOPObj.PaymentType = (DynamicsApiHelper.GetIntegerValueFromJObject(whitelistobj, "jarvis_paymenttype") != null ? DynamicsApiHelper.GetIntegerValueFromJObject(whitelistobj, "jarvis_paymenttype") : 334030006);
                        if (!matchedCondition.disableAutoapprove)
                        {
                            GOPObj.isApproved = (int)GopApproval.Approved;
                            GOPObj.approvaldate = DateTime.UtcNow;
                            GOPObj.Contact = "Automated GOP via Whitelisting";
                        }
                        else
                        {
                            GOPObj.isApproved = (int)GopApproval.Pending;
                        }
                        CreateGOP(GOPObj, matchedCondition);
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }

                return ConditionSatisfied;

            }

            catch (Exception ex)
            {
                //throw new ArgumentException($"whitelistSubProcess:" + ex.Message);
                //log.LogInformation($"Exception in whitelistSubProcess " + ex.Message);
                return false;
            }
        }
        #endregion

        #region CREATE-GOP
        /// <summary>
        /// CreateGOP - Create GOP based on the conditions satisfied
        /// </summary>
        /// <param name="gopPayload"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private void CreateGOP(GOPInit gopPayload, ConditionLink matchedCondition)
        {
            try
            {
                ICollection<JObject> existingGops = this.dynamicsClient.RetrieveResultSetByFilter("jarvis_gops", "jarvis_requesttype", string.Format("_jarvis_incident_value eq '{0}'", matchedCondition.CaseId), true);
                if (gopPayload != null && existingGops.Count < 1)
                {

                    JObject payload = new JObject();
                    payload.Add("jarvis_requesttype", 334030001);
                    payload.Add("jarvis_paymenttype", gopPayload.PaymentType);
                    payload.Add("jarvis_goplimitin", gopPayload.Amount);
                    payload.Add("jarvis_GOPINCurrency@odata.bind", "/transactioncurrencies(" + gopPayload.Currency + ")");
                    payload.Add("jarvis_GOPOUTCurrency@odata.bind", "/transactioncurrencies(" + gopPayload.Currency + ")");
                    payload.Add("jarvis_gopapproval", gopPayload.isApproved);
                    payload.Add("jarvis_contact", gopPayload.Contact);
                    payload.Add("jarvis_Incident@odata.bind", string.Format(" /{0}({1})", "incidents", matchedCondition.CaseId));
                    payload.Add("jarvis_Dealer@odata.bind", string.Format(" /{0}({1})", "accounts", gopPayload.dealerId));
                    payload.Add("jarvis_lineage", matchedCondition.lineage);
                    if(gopPayload.isApproved.Equals((int)GopApproval.Approved))
                    {
                        payload.Add("jarvis_gopapprovaltime", gopPayload.approvaldate);
                    }
                    // Create GOP with Status=HasBeenSent for HD=RD Conditions - US#657623
                    if (gopPayload.statuscode != null)
                    {
                        payload.Add("statuscode", (int?)Status.Hasbeensent);
                    }
                    this.dynamicsClient.CreateEntity("jarvis_gops", payload);
                }

            }
            catch (Exception ex)
            {
                throw new ArgumentException($"CreateGOP:" + ex.Message);
            }
        }
        #endregion
    }
}
