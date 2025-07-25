namespace IntegrationTestFunction
{
    using System;
    using System.Globalization;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Xml;
    using IntegrationProcess;
    using MCS.Jarvis.Integration.Base.Dynamics;
    using MCS.Jarvis.Integration.Base.Logging;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class SiebelTestFunction : ExponentialBackoffRetryAttribute
    {
        private readonly IConfiguration config;
        private readonly IDynamicsApiClient dynamicsClient;
        private readonly int maxRetryCount;

        public SiebelTestFunction(IConfiguration config, IDynamicsApiClient dynamicsClient) : base(config.GetValue<int>("MaxRetryCount"), config.GetValue<string>("FirstRetryInterval"), config.GetValue<string>("MaxRetryInterval"))
        {
            this.config = config;
            this.dynamicsClient = dynamicsClient;
        }

        [FunctionName("SiebelTestFunction")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
#pragma warning disable S125 // Sections of code should not be commented out
            // string myQueueItem = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n<!-- edited with XMLSpy v2011 rel. 3 sp1 (http://www.altova.com) by VOLVO INFORMATION TECH AB (VOLVO INFORMATION TECH AB) -->\r\n<SiebelMessage MessageId=\"ca147478-bae9-4c4b-a77b-d7166fe8ae36\" eventTimestamp=\"2022-11-07 08:52:30\" TransType=\"Mercurius.Event.CreateCase\" MessageType=\"Integration Object\" IntObjectName=\"Case Detail Info JARVIS\" IntObjectFormat=\"Siebel Hierarchical\">\r\n\t<ListOfServiceRequestBreakdownArgus>\r\n\t\t<ServiceRequestBdArgus>\r\n            <CommunicationTypeARGUS>WEB</CommunicationTypeARGUS>\t\r\n\t\t\t<CaseNumberARGUS>234574Z33</CaseNumberARGUS>\r\n\t\t\t<CaseNumberJarvis></CaseNumberJarvis>\t\t\t\r\n\t\t\t<Status>000</Status>\r\n\t\t\t<CaseOpenAtARGUS>02/09/2022 09:09:20</CaseOpenAtARGUS>\r\n\t\t\t<BrandARGUS>VOLVO TRUCK&amp;BUS</BrandARGUS>\r\n\t\t\t<CallerNameARGUS>MandyTest</CallerNameARGUS>\r\n\t\t\t<CallbacknumberARGUS>+46739026228</CallbacknumberARGUS>\r\n\t\t\t<CallerLanguageARGUS>FRENCH</CallerLanguageARGUS>\r\n\t\t\t<CallerRelationTypeARGUS>CUSTOMER</CallerRelationTypeARGUS>\r\n\t\t\t<DriverNameARGUS>Tom</DriverNameARGUS>\r\n\t\t\t<DriverPhoneARGUS>+46</DriverPhoneARGUS>\r\n\t\t\t<DriverLanguageARGUS>fRENCH</DriverLanguageARGUS>\r\n\t\t\t<VINSerialBDARGUS>223</VINSerialBDARGUS>\r\n\t\t\t<RegistrationBDARGUS>M-MS 7205</RegistrationBDARGUS>\r\n\t\t\t<RegTrailerBDARGUS/>\r\n\t\t    <LoadCargoBDARGUS/>\r\n            <MileageUnitsBDARGUS>KM</MileageUnitsBDARGUS>\r\n\t\t\t<MileageBDARGUS/>\t\t\t\r\n\t\t\t<LocationBDARGUS>We are close to Helsingborg and having Issues</LocationBDARGUS>\r\n\t\t\t<ReportedBreakDownCountry>SWEDEN</ReportedBreakDownCountry>\r\n\t\t\t<CustomerNumARGUS>C-AVIS-123456</CustomerNumARGUS>\r\n\t\t\t<HomeDealerIdBDARGUS>6183984778</HomeDealerIdBDARGUS>\r\n\t\t\t<CreatorPartnerFullNameARGUS>adil@vovlo.com</CreatorPartnerFullNameARGUS>\t\r\n\t\t</ServiceRequestBdArgus>\r\n</ListOfServiceRequestBreakdownArgus>\r\n</SiebelMessage>";
            // string myQueueItem = "<!--  edited with XMLSpy v2011 rel. 3 sp1 (http://www.altova.com) by VOLVO INFORMATION TECH AB (VOLVO INFORMATION TECH AB)  -->\r\n<SiebelMessage MessageId=\"ca147478-bae9-4c4b-a77b-d7166fe8ae36\" EventTimestamp=\"03/24/2021 23:04:54\" TransType=\"Mercurius.Event.ETAUpdate\" MessageType=\"Integration Object\" IntObjectName=\"Mercurius Case Detail Info\" IntObjectFormat=\"Siebel Hierarchical\">\r\n<ListOfServiceRequestBreakdownArgus>\r\n<ServiceRequestBdArgus>\r\n<CaseNumberARGUS>447574Z38</CaseNumberARGUS>\r\n<CaseNumberJarvis/>\r\n<ListOfSRBdPassoutLoginfoArgus>\r\n<SRBdPassoutLoginfoArgus>\r\n<HomeDealerIdBDARGUS>C-AVIS-123456</HomeDealerIdBDARGUS>\r\n<HomeDealerCountryBDARGUS>GERMANY</HomeDealerCountryBDARGUS>\r\n<HomeDealerBrandBDARGUS>RENAULT</HomeDealerBrandBDARGUS>\r\n<ETADateTimeBDARGUS>03/23/2023 09:11:07</ETADateTimeBDARGUS>\r\n<UpdatedByFullName>Rupa</UpdatedByFullName>\r\n</SRBdPassoutLoginfoArgus>\r\n<SRBdPassoutLoginfoArgus>\r\n<HomeDealerIdBDARGUS>12421</HomeDealerIdBDARGUS>\r\n<HomeDealerCountryBDARGUS>GERMANY</HomeDealerCountryBDARGUS>\r\n<HomeDealerBrandBDARGUS>RENAULT</HomeDealerBrandBDARGUS>\r\n<ETADateTimeBDARGUS>12/23/2022 09:11:07</ETADateTimeBDARGUS>\r\n<UpdatedByFullName>Rupa</UpdatedByFullName>\r\n</SRBdPassoutLoginfoArgus>\r\n</ListOfSRBdPassoutLoginfoArgus>\r\n</ServiceRequestBdArgus>\r\n</ListOfServiceRequestBreakdownArgus>\r\n</SiebelMessage>";
            // string myQueueItem = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n<!-- edited with XMLSpy v2011 rel. 3 sp1 (http://www.altova.com) by VOLVO INFORMATION TECH AB (VOLVO INFORMATION TECH AB) -->\r\n<SiebelMessage MessageId=\"ca147478-bae9-4c4b-a77b-d7166fe8ae36\" EventTimestamp=\"03/24/2021 23:04:54\" TransType=\"Mercurius.Event.ETAUpdate\" MessageType=\"Integration Object\" IntObjectName=\"Mercurius Case Detail Info ARGUS\" IntObjectFormat=\"Siebel Hierarchical\" >\r\n\t\t<ListOfServiceRequestBreakdownArgus>\r\n\t\t<ServiceRequestBdArgus>\r\n           \t<CaseNumberARGUS>2022001841</CaseNumberARGUS>\r\n\t\t    <CaseNumberJarvis>DEV-01276-K7H3Q</CaseNumberJarvis>\t\t\t\r\n\t\t\t<ListOfSRBdPassoutLoginfoArgus>\r\n\t\t\t\t<SRBdPassoutLoginfoArgus>\r\n\t\t\t\t\t<HomeDealerIdBDARGUS>C-AVIS-123456</HomeDealerIdBDARGUS>\r\n\t\t\t\t\t<HomeDealerCountryBDARGUS>GERMANY</HomeDealerCountryBDARGUS>\r\n\t\t\t\t\t<HomeDealerCountryCodeBDARGUS>DE</HomeDealerCountryCodeBDARGUS>\r\n\t\t\t\t\t<HomeDealerBrandBDARGUS>RENAULT</HomeDealerBrandBDARGUS>\r\n\t\t\t\t\t<ETADateTimeBDARGUS>03/23/2018 09:11:07</ETADateTimeBDARGUS>\r\n\t\t\t\t\t<UpdatedByMailId>abc@volvo.com</UpdatedByMailId>\r\n\t\t\t\t\t<UpdatedByLogin>KJOHN</UpdatedByLogin>\t\t\r\n\t\t\t\t</SRBdPassoutLoginfoArgus>\r\n\t\t\t</ListOfSRBdPassoutLoginfoArgus>\r\n\t\t</ServiceRequestBdArgus>\r\n\t</ListOfServiceRequestBreakdownArgus>\r\n</SiebelMessage>";
            // string myQueueItem = "<!--  edited with XMLSpy v2011 rel. 3 sp1 (http://www.altova.com) by VOLVO INFORMATION TECH AB (VOLVO INFORMATION TECH AB)  -->\r\n<SiebelMessage MessageId=\"ca147478-bae9-4c4b-a77b-d7166fe8ae36\" EventTimestamp=\"03/24/2021 23:04:54\" TransType=\"Mercurius.Event.ATCUpdate\" MessageType=\"Integration Object\" IntObjectName=\"Mercurius Case Detail Info\" IntObjectFormat=\"Siebel Hierarchical\">\r\n<ListOfServiceRequestBreakdownArgus>\r\n<ServiceRequestBdArgus>\r\n<CaseNumberARGUS>447574Z38</CaseNumberARGUS>\r\n<CaseNumberJarvis/>\r\n<ListOfSRBdPassoutLoginfoArgus>\r\n<SRBdPassoutLoginfoArgus>\r\n<HomeDealerIdBDARGUS>C-AVIS-123456</HomeDealerIdBDARGUS>\r\n<HomeDealerCountryBDARGUS>GERMANY</HomeDealerCountryBDARGUS>\r\n<HomeDealerBrandBDARGUS>RENAULT</HomeDealerBrandBDARGUS>\r\n<ATCDateTimeBDARGUS>03/23/2023 09:11:07</ATCDateTimeBDARGUS>\r\n<UpdatedByFullName>Rupa</UpdatedByFullName>\r\n</SRBdPassoutLoginfoArgus>\r\n<SRBdPassoutLoginfoArgus>\r\n<HomeDealerIdBDARGUS>12421</HomeDealerIdBDARGUS>\r\n<HomeDealerCountryBDARGUS>GERMANY</HomeDealerCountryBDARGUS>\r\n<HomeDealerBrandBDARGUS>RENAULT</HomeDealerBrandBDARGUS>\r\n<ATCDateTimeBDARGUS>12/23/2022 09:11:07</ATCDateTimeBDARGUS>\r\n<UpdatedByFullName>Rupa</UpdatedByFullName>\r\n</SRBdPassoutLoginfoArgus>\r\n</ListOfSRBdPassoutLoginfoArgus>\r\n</ServiceRequestBdArgus>\r\n</ListOfServiceRequestBreakdownArgus>\r\n</SiebelMessage>";
            // string myQueueItem = "<!--  edited with XMLSpy v2011 rel. 3 sp1 (http://www.altova.com) by VOLVO INFORMATION TECH AB (VOLVO INFORMATION TECH AB)  -->\r\n<SiebelMessage MessageId=\"ca147478-bae9-4c4b-a77b-d7166fe8ae36\" EventTimestamp=\"03/24/2021 23:04:54\" TransType=\"Mercurius.Event.ATAUpdate\" MessageType=\"Integration Object\" IntObjectName=\"Mercurius Case Detail Info\" IntObjectFormat=\"Siebel Hierarchical\">\r\n<ListOfServiceRequestBreakdownArgus>\r\n<ServiceRequestBdArgus>\r\n<CaseNumberARGUS>447574Z38</CaseNumberARGUS>\r\n<CaseNumberJarvis/>\r\n<ListOfSRBdPassoutLoginfoArgus>\r\n<SRBdPassoutLoginfoArgus>\r\n<HomeDealerIdBDARGUS>C-AVIS-123456</HomeDealerIdBDARGUS>\r\n<HomeDealerCountryBDARGUS>GERMANY</HomeDealerCountryBDARGUS>\r\n<HomeDealerBrandBDARGUS>RENAULT</HomeDealerBrandBDARGUS>\r\n<ATADateTimeBDARGUS>03/23/2023 09:11:07</ATADateTimeBDARGUS>\r\n<UpdatedByFullName>Rupa</UpdatedByFullName>\r\n</SRBdPassoutLoginfoArgus>\r\n<SRBdPassoutLoginfoArgus>\r\n<HomeDealerIdBDARGUS>12421</HomeDealerIdBDARGUS>\r\n<HomeDealerCountryBDARGUS>GERMANY</HomeDealerCountryBDARGUS>\r\n<HomeDealerBrandBDARGUS>RENAULT</HomeDealerBrandBDARGUS>\r\n<ATADateTimeBDARGUS>12/23/2022 09:11:07</ATADateTimeBDARGUS>\r\n<UpdatedByFullName>Rupa</UpdatedByFullName>\r\n</SRBdPassoutLoginfoArgus>\r\n</ListOfSRBdPassoutLoginfoArgus>\r\n</ServiceRequestBdArgus>\r\n</ListOfServiceRequestBreakdownArgus>\r\n</SiebelMessage>";
            // string myQueueItem = "<?xml version=\"1.0\" encoding=\"UTF-8\"?> <!-- edited with XMLSpy v2011 rel. 3 sp1 (http://www.altova.com) by VOLVO INFORMATION TECH AB (VOLVO INFORMATION TECH AB) --> <SiebelMessage MessageId=\"ca147478-bae9-4c4b-a77b-d7166fe8ae36\" EventTimestamp=\"03/24/2021 23:04:54\" TransType=\"Mercurius.Event.ATCUpdate\" MessageType=\"Integration Object\" IntObjectName=\"Mercurius Case Detail Info ARGUS\" IntObjectFormat=\"Siebel Hierarchical\"> <ListOfServiceRequestBreakdownArgus> <ServiceRequestBdArgus> <CaseNumberARGUS>8751777203</CaseNumberARGUS> <CaseNumberJarvis></CaseNumberJarvis> <ListOfSRBdPassoutLoginfoArgus> <SRBdPassoutLoginfoArgus> <HomeDealerIdBDARGUS>6183984778</HomeDealerIdBDARGUS> <HomeDealerCountryBDARGUS>GERMANY</HomeDealerCountryBDARGUS> <HomeDealerBrandBDARGUS>RENAULT</HomeDealerBrandBDARGUS> <ATCDateTimeBDARGUS>03/25/2020 23:04:54</ATCDateTimeBDARGUS> <UpdatedByFullName>ADIL</UpdatedByFullName> </SRBdPassoutLoginfoArgus> </ListOfSRBdPassoutLoginfoArgus> </ServiceRequestBdArgus> </ListOfServiceRequestBreakdownArgus> </SiebelMessage>";
            // string myQueueItem = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n<!-- edited with XMLSpy v2011 rel. 3 sp1 (http://www.altova.com) by VOLVO INFORMATION TECH AB (VOLVO INFORMATION TECH AB) -->\r\n<SiebelMessage MessageId=\"ca147478-bae9-4c4b-a77b-d7166fe8ae36\" EventTimestamp=\"03/24/2021 23:04:54\" TransType=\"Mercurius.Event.DelayedETAUpdate\" MessageType=\"Integration Object\" IntObjectName=\"Mercurius Case Detail Info ARGUS\" IntObjectFormat=\"Siebel Hierarchical\">\r\n\t<ListOfServiceRequestBreakdownArgus>\r\n\t\t<ServiceRequestBdArgus>\r\n         \t<CaseNumberARGUS>447574Z38</CaseNumberARGUS>\r\n\t\t    <CaseNumberJarvis></CaseNumberJarvis>\t\t\t\r\n\t\t\t<ListOfSRBdPassoutLoginfoArgus>\r\n\t\t\t\t<SRBdPassoutLoginfoArgus>\r\n\t\t\t\t\t<HomeDealerIdBDARGUS>12421</HomeDealerIdBDARGUS>\r\n\t\t\t\t\t<HomeDealerBrandBDARGUS>RENAULT</HomeDealerBrandBDARGUS>\r\n\t\t\t\t\t<HomeDealerCountryBDARGUS>GERMANY</HomeDealerCountryBDARGUS>\r\n\t\t\t\t\t<UpdatedByFullName>Rupa</UpdatedByFullName>\t\r\n\t\t\t\t\t<ListOfSRBreakdownDelayedEtaArgus>\r\n\t\t\t\t\t\t<SRBreakdownDelayedEtaArgus>\r\n\t\t\t\t\t\t\t<ETADelayedDateBDARGUS>03/27/2018 09:11:07</ETADelayedDateBDARGUS>\r\n\t\t\t\t\t\t</SRBreakdownDelayedEtaArgus>\r\n\t\t\t\t\t</ListOfSRBreakdownDelayedEtaArgus>\r\n\t\t\t\t</SRBdPassoutLoginfoArgus><SRBdPassoutLoginfoArgus>\r\n\t\t\t\t\t<HomeDealerIdBDARGUS>C-AVIS-123456</HomeDealerIdBDARGUS>\r\n\t\t\t\t\t<HomeDealerBrandBDARGUS>RENAULT</HomeDealerBrandBDARGUS>\r\n\t\t\t\t\t<HomeDealerCountryBDARGUS>GERMANY</HomeDealerCountryBDARGUS>\r\n\t\t\t\t\t<UpdatedByFullName>ADIL</UpdatedByFullName>\t\r\n\t\t\t\t\t<ListOfSRBreakdownDelayedEtaArgus>\r\n\t\t\t\t\t\t<SRBreakdownDelayedEtaArgus>\r\n\t\t\t\t\t\t\t<ETADelayedDateBDARGUS>03/27/2022 09:11:07</ETADelayedDateBDARGUS>\r\n\t\t\t\t\t\t</SRBreakdownDelayedEtaArgus>\r\n\t\t\t\t\t</ListOfSRBreakdownDelayedEtaArgus>\r\n\t\t\t\t</SRBdPassoutLoginfoArgus>\r\n\t\t\t</ListOfSRBdPassoutLoginfoArgus>\r\n\t\t</ServiceRequestBdArgus>\r\n\t</ListOfServiceRequestBreakdownArgus>\r\n</SiebelMessage>";
            // string myQueueItem = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n<SiebelMessage MessageId=\"ca147478-bae9-4c4b-a77b-d7166fe8ae36\" EventTimestamp=\"01/09/2022 10:06:20\" TransType=\"Mercurius.Event.UpdateCase\" MessageType=\"Integration Object\" IntObjectName=\"Mercurius Case Detail Info ARGUS\" IntObjectFormat=\"Siebel Hierarchical\">\r\n\t<ListOfServiceRequestBreakdownArgus>\r\n\t\t<ServiceRequestBdArgus>\r\n\t\t\t<CommunicationTypeARGUS>WEB</CommunicationTypeARGUS>\r\n\t\t\t<CustomerRefNumber>XXXX</CustomerRefNumber>\r\n\t\t\t<CaseNumberARGUS>23457Z33</CaseNumberARGUS>\r\n\t\t\t<CaseNumberJarvis>DEV-01455-Z3B0Z</CaseNumberJarvis>\r\n\t\t\t<Status>030</Status>\r\n\t\t\t<BrandARGUS>VOLVO TRUCK&amp;BUS</BrandARGUS>\r\n\t\t\t<CallerNameARGUS>Adil</CallerNameARGUS>\r\n\t\t\t<CallbacknumberARGUS>+46739026239</CallbacknumberARGUS>\r\n\t\t\t<CallerLanguageARGUS>DUTCH</CallerLanguageARGUS>\r\n\t\t\t<CallerRelationTypeARGUS>CUSTOMER</CallerRelationTypeARGUS>\r\n\t\t\t<DriverNameARGUS>Tomasz</DriverNameARGUS>\r\n\t\t\t<DriverPhoneARGUS>+46123456789</DriverPhoneARGUS>\r\n\t\t\t<DriverLanguageARGUS>DUTCH</DriverLanguageARGUS>\r\n\t\t\t<HomeDealerIdBDARGUS>C-AVIS-123456</HomeDealerIdBDARGUS>\r\n\t\t\t<RegistrationBDARGUS>07C7749</RegistrationBDARGUS>\r\n\t\t\t<ChassisSeriesBDARGUS>B</ChassisSeriesBDARGUS>\r\n\t\t\t<ChassisNumberBDARGUS>451247</ChassisNumberBDARGUS>\r\n\t\t\t<RegTrailerBDARGUS>PFZ495</RegTrailerBDARGUS>\r\n\t\t\t<LoadCargoBDARGUS>load 20t</LoadCargoBDARGUS>\r\n\t\t\t<SpecialIndicatorBDARGUS>LNG</SpecialIndicatorBDARGUS>\r\n\t\t\t<MileageUnitsBDARGUS>KM</MileageUnitsBDARGUS>\r\n\t\t\t<MileageBDARGUS>23456</MileageBDARGUS>\r\n\t\t\t<LocationBDARGUS>Helsingborg Road</LocationBDARGUS>\r\n\t\t\t<ReportedBreakDownCountry>SWEDEN</ReportedBreakDownCountry>\r\n\t\t\t<PersonalLongitudeBDARGUS>-12.34</PersonalLongitudeBDARGUS>\r\n\t\t\t<PersonalLatitudeBDARGUS>34.56</PersonalLatitudeBDARGUS>\r\n\t\t\t<CustomerNumARGUS>C-AVIS-123456</CustomerNumARGUS>\r\n\t\t\t<ListOfSRBdGopInLoginfoArgus>\r\n\t\t\t\t<SRBdGopInLoginfoArgus>\r\n\t\t\t\t\t<HomeDealerNameBDARGUS>VOLVO HD TEST</HomeDealerNameBDARGUS>\r\n\t\t\t\t\t<HomeDealerIdBDARGUS>11112231</HomeDealerIdBDARGUS>\r\n\t\t\t\t\t<HomeDealerBrandBDARGUS>VOLVO TRUCK&amp;BUS</HomeDealerBrandBDARGUS>\r\n\t\t\t\t\t<Type>GOP-IN</Type>\r\n\t\t\t\t\t<Id>1-123AN</Id>\r\n\t\t\t\t\t<Id_JARVIS>GOP-0001001-T2D2T</Id_JARVIS>\r\n\t\t\t\t\t<ObservationTypeBDARGUS>Unable to contact GOP person update</ObservationTypeBDARGUS>\r\n\t\t\t\t\t<ContactNameARGUS>Stacy</ContactNameARGUS>\r\n\t\t\t\t\t<LimitInAmountBDARGUS>36853</LimitInAmountBDARGUS>\r\n\t\t\t\t\t<LimitInCurrencyBDARGUS>EUR</LimitInCurrencyBDARGUS>\r\n\t\t\t\t\t<LimitOutAmountBDARGUS>652</LimitOutAmountBDARGUS>\r\n\t\t\t\t\t<LimitOutCurrencyBDARGUS>EUR</LimitOutCurrencyBDARGUS>\r\n\t\t\t\t\t<PaymentTypeBDARGUS>CASH</PaymentTypeBDARGUS>\r\n\t\t\t\t\t<ValidatedTimeARGUS>03/24/2021 07:04:54</ValidatedTimeARGUS>\r\n\t\t\t\t\t<ValidatedByNameARGUS>BDNORMAL</ValidatedByNameARGUS>\r\n\t\t\t\t</SRBdGopInLoginfoArgus>\r\n\t\t\t\t<SRBdGopInLoginfoArgus>\r\n\t\t\t\t\t<HomeDealerNameBDARGUS>VOLVO HD TEST</HomeDealerNameBDARGUS>\r\n\t\t\t\t\t<HomeDealerIdBDARGUS>C-AVIS-123456</HomeDealerIdBDARGUS>\r\n\t\t\t\t\t<HomeDealerBrandBDARGUS>VOLVO TRUCK&amp;BUS</HomeDealerBrandBDARGUS>\r\n\t\t\t\t\t<Type>GOP-IN</Type>\r\n\t\t\t\t\t<Id>1-13Xt</Id>\r\n\t\t\t\t\t<Id_JARVIS></Id_JARVIS>\r\n\t\t\t\t\t<ObservationTypeBDARGUS>Unable to contact GOP person</ObservationTypeBDARGUS>\r\n\t\t\t\t\t<ContactNameARGUS>Stacy</ContactNameARGUS>\r\n\t\t\t\t\t<LimitInAmountBDARGUS>36853</LimitInAmountBDARGUS>\r\n\t\t\t\t\t<LimitInCurrencyBDARGUS>EUR</LimitInCurrencyBDARGUS>\r\n\t\t\t\t\t<LimitOutAmountBDARGUS>652</LimitOutAmountBDARGUS>\r\n\t\t\t\t\t<LimitOutCurrencyBDARGUS>EUR</LimitOutCurrencyBDARGUS>\r\n\t\t\t\t\t<PaymentTypeBDARGUS>CASH</PaymentTypeBDARGUS>\r\n\t\t\t\t\t<ValidatedTimeARGUS>03/24/2021 07:04:54</ValidatedTimeARGUS>\r\n\t\t\t\t\t<ValidatedByNameARGUS>BDNORMAL</ValidatedByNameARGUS>\r\n\t\t\t\t</SRBdGopInLoginfoArgus>\r\n\t\t\t</ListOfSRBdGopInLoginfoArgus>\r\n\t\t\t<ListOfSRBdPassoutLoginfoArgus>\r\n\t\t\t\t<SRBdPassoutLoginfoArgus>\r\n\t\t\t\t\t<HomeDealerNameBDARGUS>VALERIO CANEZ S.A.</HomeDealerNameBDARGUS>\r\n\t\t\t\t\t<HomeDealerIdBDARGUS>C-AVIS-123456</HomeDealerIdBDARGUS>\r\n\t\t\t\t\t<HomeDealerBrandBDARGUS>VOLVO TRUCK&amp;BUS</HomeDealerBrandBDARGUS>\r\n\t\t\t\t\t<Type>PASS OUT</Type>\r\n\t\t\t\t\t<Id>1-145B9Y</Id>\r\n\t\t\t\t\t<Id_JARVIS>PASS-0001001-K4R2C2</Id_JARVIS>\r\n\t\t\t\t\t<ObservationTypeBDARGUS>Dealer refuses to attend update34</ObservationTypeBDARGUS>\r\n\t\t\t\t\t<PassOutAmountBDARGUS>10</PassOutAmountBDARGUS>\r\n\t\t\t\t\t<PassOutAmountCurrencyBDARGUS>EUR</PassOutAmountCurrencyBDARGUS>\r\n\t\t\t\t\t<PaymentTypeBDARGUS>AUTO GOP</PaymentTypeBDARGUS>\r\n\t\t\t\t\t<ValidatedByNameARGUS>BDNORMAL</ValidatedByNameARGUS>\r\n\t\t\t\t</SRBdPassoutLoginfoArgus>\r\n\t\t\t</ListOfSRBdPassoutLoginfoArgus>\r\n\t\t</ServiceRequestBdArgus>\r\n\t</ListOfServiceRequestBreakdownArgus>\r\n</SiebelMessage>";
            // string myQueueItem = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n<!-- edited with XMLSpy v2011 rel. 3 sp1 (http://www.altova.com) by VOLVO INFORMATION TECH AB (VOLVO INFORMATION TECH AB) -->\r\n<SiebelMessage MessageId=\"ca147478-bae9-4c4b-a77b-d7166fe8ae36\" EventTimestamp=\"03/24/2021 23:04:54\" TransType=\"Mercurius.Event.DelayedETAUpdate\" MessageType=\"Integration Object\" IntObjectName=\"Mercurius Case Detail Info ARGUS\" IntObjectFormat=\"Siebel Hierarchical\">\r\n<ListOfServiceRequestBreakdownArgus>\r\n\t\t<ServiceRequestBdArgus>\r\n         \t<CaseNumberARGUS>2022001841dd</CaseNumberARGUS>\r\n\t\t    <CaseNumberJarvis>DEV-01276-K7H3Q</CaseNumberJarvis>\t\t\t\r\n\t\t\t<ListOfSRBdPassoutLoginfoArgus>\r\n\t\t\t\t<SRBdPassoutLoginfoArgus>\r\n\t\t\t\t\t<HomeDealerIdBDARGUS>C-AVIS-123456</HomeDealerIdBDARGUS>\r\n\t\t\t\t\t<HomeDealerBrandBDARGUS>RENAULT</HomeDealerBrandBDARGUS>\r\n\t\t\t\t\t<HomeDealerCountryBDARGUS>GERMANY</HomeDealerCountryBDARGUS>\r\n\t\t\t\t\t<HomeDealerCountryCodeBDARGUS>DE</HomeDealerCountryCodeBDARGUS>\r\n\t\t\t\t\t<UpdatedByMailId>abc@volvo.com</UpdatedByMailId>\r\n\t\t\t\t\t<UpdatedByLogin>KJOHN</UpdatedByLogin>\t\r\n\t\t\t\t\t<ETADelayedDateTimeBDARGUS>03/27/2018 09:11:07</ETADelayedDateTimeBDARGUS>\r\n\t\t\t\t\t<ETADelayedReason>Delayed due to parts unavailability</ETADelayedReason>\t\t\t\t\t\r\n\t\t\t\t</SRBdPassoutLoginfoArgus>\r\n\t\t\t</ListOfSRBdPassoutLoginfoArgus>\r\n\t\t</ServiceRequestBdArgus>\r\n\t</ListOfServiceRequestBreakdownArgus>\r\n</SiebelMessage>\r\n";
            // string myQueueItem = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n<SiebelMessage MessageId=\"1-ZKL\" TransType=\"AddRemark\" MessageType=\"Integration Object\" IntObjectName=\"Mercurius Case Detail Info ARGUS\" IntObjectFormat=\"Siebel Hierarchical\">\r\n\t<ListOfServiceRequestBreakdownArgus>\r\n\t\t<ServiceRequestBdArgus>\r\n\t\t\t<CaseNumberARGUS>2021000281</CaseNumberARGUS>\r\n          <CaseNumberJARVIS>DEV-01276-K7H3Q</CaseNumberJARVIS>\r\n\t\t\t<CustomerRefNumber>XXXX</CustomerRefNumber>\r\n\t\t\t<ListOfServiceRequestRemark>\r\n\t\t\t\t<ServiceRequestRemark>\r\n\t\t\t\t\t<Comment>Diagnose: INFO - Part Fixed temporarily</Comment>\r\n\t\t\t\t\t<Audience>Public</Audience>\r\n\t\t\t\t\t<Status>010</Status>\r\n\t\t\t\t\t<Created>11/23/2021 11:45:18</Created>\r\n\t\t\t\t\t<CreatorAliasARGUS>BDNORMAL</CreatorAliasARGUS>\r\n\t\t\t\t</ServiceRequestRemark>\r\n\t\t\t</ListOfServiceRequestRemark>\r\n\t\t</ServiceRequestBdArgus>\r\n\t</ListOfServiceRequestBreakdownArgus>\r\n</SiebelMessage>";
            // string myQueueItem = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n<SiebelMessage MessageId=\"1-ZKL\" TransType =\"AddRemark\" MessageType=\"Integration Object\" IntObjectName=\"Mercurius Case Detail Info ARGUS\" IntObjectFormat=\"Siebel Hierarchical\">\r\n\t<ListOfServiceRequestBreakdownArgus>\r\n\t\t<ServiceRequestBdArgus>\r\n\t\t\t<CaseNumberARGUS>2022001841</CaseNumberARGUS>\r\n          <CaseNumberJARVIS></CaseNumberJARVIS>\r\n\t\t\t<CustomerRefNumber>XXXX</CustomerRefNumber>\r\n\t\t\t<ListOfServiceRequestRemark>\r\n\t\t\t\t<ServiceRequestRemark>\r\n\t\t\t\t\t<Comment>Diagnose: INFO - Part Fixed temporarily</Comment>\r\n\t\t\t\t\t<Audience>Public</Audience>\r\n\t\t\t\t\t<Status>010</Status>\r\n\t\t\t\t\t<Created>11/23/2021 11:45:18</Created>\r\n\t\t\t\t\t<CreatorAliasARGUS>BDNORMAL</CreatorAliasARGUS>\r\n\t\t\t\t</ServiceRequestRemark>\r\n              <ServiceRequestRemark>\r\n\t\t\t\t\t<Comment>Diagnose: INFO 2 - Part Fixed temporarily</Comment>\r\n\t\t\t\t\t<Audience>Public</Audience>\r\n\t\t\t\t\t<Status>010</Status>\r\n\t\t\t\t\t<Created>11/23/2021 11:45:18</Created>\r\n\t\t\t\t\t<CreatorAliasARGUS>BDNORMAL</CreatorAliasARGUS>\r\n\t\t\t\t</ServiceRequestRemark>\r\n\t\t\t</ListOfServiceRequestRemark>\r\n\t\t</ServiceRequestBdArgus>\r\n\t</ListOfServiceRequestBreakdownArgus>\r\n</SiebelMessage>";
            //  string myQueueItem = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n<!-- edited with XMLSpy v2011 rel. 3 sp1 (http://www.altova.com) by VOLVO INFORMATION TECH AB (VOLVO INFORMATION TECH AB) -->\r\n<SiebelMessage MessageId=\"ca147478-bae9-4c4b-a77b-d7166fe8ae36\" EventTimestamp=\"03/24/2022 23:04:54\" TransType=\"Mercurius.Event.AddReportFault1\" MessageType=\"Integration Object\" IntObjectName=\"Mercurius Case Detail Info ARGUS\" IntObjectFormat=\"Siebel Hierarchical\">\r\n    <ListOfServiceRequestBreakdownArgus>\r\n        <ServiceRequestBdArgus>    \r\n            <CaseNumberARGUS>59492427472</CaseNumberARGUS>\r\n            <CaseNumberJARVIS></CaseNumberJARVIS>\r\n            <ListOfSRBdReportFaultArgus>\r\n                <SRBdReportFaultArgus>\r\n                    <Created>03/06/2018 11:41:24</Created>\r\n                    <Comments>Issue with Truck, No possible speed in Up hill updated</Comments>\r\n                    <Language>ENGLISH</Language>\r\n                    <CreatedByLogin>MERCURIUS</CreatedByLogin>\r\n                    <CreatedByMailId>merc@noemail.com</CreatedByMailId>\r\n                </SRBdReportFaultArgus>\r\n            </ListOfSRBdReportFaultArgus>\r\n        </ServiceRequestBdArgus>\r\n    </ListOfServiceRequestBreakdownArgus>\r\n</SiebelMessage>";
            // string myQueueItem = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n<!-- edited with XMLSpy v2011 rel. 3 sp1 (http://www.altova.com) by VOLVO INFORMATION TECH AB (VOLVO INFORMATION TECH AB) -->\r\n<SiebelMessage MessageId=\"ca147478-bae9-4c4b-a77b-d7166fe8ae36\" EventTimestamp=\"03/24/2021 23:04:54\" TransType=\"Mercurius.Event.AddReportFault2\" MessageType=\"Integration Object\" IntObjectName=\"Mercurius Case Detail Info ARGUS\" IntObjectFormat=\"Siebel Hierarchical\">\r\n\t<ListOfServiceRequestBreakdownArgus>\r\n\t\t<ServiceRequestBdArgus>\t\r\n\t\t    <CaseNumberARGUS>59492427472</CaseNumberARGUS>\r\n\t\t\t<CaseNumberJARVIS></CaseNumberJARVIS>\r\n\t\t\t<ListOfSRBdReportFaultArgus>\r\n\t\t\t\t<SRBdReportFaultArgus>\r\n\t\t\t\t\t<Created>03/06/2018 11:41:24</Created>\r\n\t\t\t\t\t<Comments>Issue with Truck, No possible speed in Uphill by rupa</Comments>\r\n\t\t\t\t\t<Language>Portuguese</Language> <LanguageCode>FIN</LanguageCode>\r\n\t\t\t\t\t<CreatedByLogin>MERCURIUS</CreatedByLogin>\r\n\t\t\t\t\t<CreatedByMailId>merc@noemail.com</CreatedByMailId>\r\n\t\t\t\t</SRBdReportFaultArgus>\r\n\t\t\t</ListOfSRBdReportFaultArgus>\r\n\t\t</ServiceRequestBdArgus>\r\n\t</ListOfServiceRequestBreakdownArgus>\r\n</SiebelMessage>";
            // string myQueueItem = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n<!-- edited with XMLSpy v2011 rel. 3 sp1 (http://www.altova.com) by VOLVO INFORMATION TECH AB (VOLVO INFORMATION TECH AB) -->\r\n<SiebelMessage MessageId=\"ca147478-bae9-4c4b-a77b-d7166fe8ae36\" EventTimestamp=\"03/24/2021 23:04:54\" TransType=\"Mercurius.Event.AddReportFault2\" MessageType=\"Integration Object\" IntObjectName=\"Mercurius Case Detail Info ARGUS\" IntObjectFormat=\"Siebel Hierarchical\">\r\n\t<ListOfServiceRequestBreakdownArgus>\r\n\t\t<ServiceRequestBdArgus>\t\r\n\t\t    <CaseNumberARGUS>2022001841d</CaseNumberARGUS>\r\n\t\t\t<CaseNumberJARVIS>DEV-01410-B5R5X</CaseNumberJARVIS>\r\n\t\t\t<ListOfSRBdReportFaultArgus>\r\n\t\t\t\t<SRBdReportFaultArgus>\r\n\t\t\t\t\t<Created>03/06/2018 11:41:24</Created>\r\n\t\t\t\t\t<Comments>Issue with Truck, No possible speed in Uphill-Dutch</Comments>\r\n\t\t\t\t\t<Language>Dutch</Language>\r\n\t\t\t\t\t<CreatedByLogin>MERCURIUS</CreatedByLogin>\r\n\t\t\t\t\t<CreatedByMailId>merc@noemail.com</CreatedByMailId>\r\n\t\t\t\t</SRBdReportFaultArgus>\r\n\t\t\t\t<SRBdReportFaultArgus>\r\n\t\t\t\t\t<Created>03/06/2018 11:41:24</Created>\r\n\t\t\t\t\t<Comments>Problème avec le camion, pas de vitesse possible en montée</Comments>\r\n\t\t\t\t\t<Language>French</Language>\r\n\t\t\t\t\t<CreatedByLogin>MERCURIUS</CreatedByLogin>\r\n\t\t\t\t\t<CreatedByMailId>merc@noemail.com</CreatedByMailId>\r\n\t\t\t\t</SRBdReportFaultArgus>\r\n\t\t\t\t<SRBdReportFaultArgus>\r\n\t\t\t\t\t<Created>03/06/2018 11:41:24</Created>\r\n\t\t\t\t\t<Comments>Problème avec le camion, pas de vitesse possible en montée</Comments>\r\n\t\t\t\t\t<Language>US English</Language>\r\n\t\t\t\t\t<CreatedByLogin>MERCURIUS</CreatedByLogin>\r\n\t\t\t\t\t<CreatedByMailId>merc@noemail.com</CreatedByMailId>\r\n\t\t\t\t</SRBdReportFaultArgus>\r\n\t\t\t</ListOfSRBdReportFaultArgus>\r\n\t\t</ServiceRequestBdArgus>\r\n\t</ListOfServiceRequestBreakdownArgus>\r\n</SiebelMessage>";
            // string myQueueItem = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n<SiebelMessage MessageId=\"1-128FGX\" EventTimestamp=\"\" TransType=\"Mercurius.Event.AddGOP\" MessageType=\"Integration Object\" IntObjectName=\"Mercurius Case Detail Info ARGUS\" IntObjectFormat=\"Siebel Hierarchical\">\r\n    <ListOfServiceRequestBreakdownArgus>\r\n        <ServiceRequestBdArgus>\r\n            <CaseNumberARGUS>59492427472</CaseNumberARGUS>\r\n            <CaseNumberJARVIS></CaseNumberJARVIS>\r\n            <ListOfSRBdGopInLoginfoArgus>\r\n                <SRBdGopInLoginfoArgus>\r\n                    <TDIPartner>0001610</TDIPartner> \r\n                    <TDIMarket>0001610</TDIMarket> \r\n                    <HomeDealerBrandBDARGUS>RENAULT</HomeDealerBrandBDARGUS>\r\n                    <HomeDealerCountryBDARGUS>GERMANY</HomeDealerCountryBDARGUS>\r\n                    <HomeDealerCountryCodeBDARGUS>DE</HomeDealerCountryCodeBDARGUS>\r\n                    <Type>GOP-IN</Type>\r\n                    <Id>29-123PG</Id>\r\n                    <ObservationTypeBDARGUS>Unable to contact GOP person</ObservationTypeBDARGUS>\r\n                    <ContactNameARGUS>Stacy</ContactNameARGUS>\r\n                    <LimitInAmountBDARGUS>36853</LimitInAmountBDARGUS>\r\n                    <LimitInCurrencyBDARGUS>EUR</LimitInCurrencyBDARGUS>\r\n                    <LimitOutAmountBDARGUS>652</LimitOutAmountBDARGUS>\r\n                    <LimitOutCurrencyBDARGUS>EUR</LimitOutCurrencyBDARGUS>\r\n                    <PaymentTypeBDARGUS>CASH</PaymentTypeBDARGUS>\r\n                    <ValidatedTimeARGUS>03/24/2021 07:04:54</ValidatedTimeARGUS>\r\n                    <ValidatedByLogin>BDNORMAL</ValidatedByLogin>\r\n                    <ValidatedByMailId>abc@volvo.com</ValidatedByMailId>\r\n                    <HomeDealerNotUsedFlg>N</HomeDealerNotUsedFlg>\r\n                </SRBdGopInLoginfoArgus>\r\n            </ListOfSRBdGopInLoginfoArgus>\r\n        </ServiceRequestBdArgus>\r\n    </ListOfServiceRequestBreakdownArgus>\r\n</SiebelMessage>";
            // string myQueueItem = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n<SiebelMessage MessageId=\"1-128FGX\" EventTimestamp=\"01/09/2022 10:06:20\" TransType=\"Mercurius.Event.PassOut\" MessageType=\"Integration Object\" IntObjectName=\"Mercurius Case Detail Info ARGUS\" IntObjectFormat=\"Siebel Hierarchical\">\r\n\t<ListOfServiceRequestBreakdownArgus>\r\n\t\t<ServiceRequestBdArgus>\t\t\t\r\n\t\t\t<CaseNumberARGUS>2022340041</CaseNumberARGUS>\r\n\t\t\t<CaseNumberJARVIS>DEV-01419-R6R7P</CaseNumberJARVIS>\r\n\t\t\t<ListOfSRBdPassoutLoginfoArgus>\r\n\t\t\t\t<SRBdPassoutLoginfoArgus>\r\n\t\t\t\t\t<HomeDealerIdBDARGUS>87684357</HomeDealerIdBDARGUS>\r\n\t\t\t\t\t<HomeDealerCountryBDARGUS>GERMANY</HomeDealerCountryBDARGUS>\r\n\t\t\t\t\t<HomeDealerCountryCodeBDARGUS>DE</HomeDealerCountryCodeBDARGUS>\r\n\t\t\t\t\t<HomeDealerBrandBDARGUS>VOLVO TRUCK&amp;BUS</HomeDealerBrandBDARGUS>\r\n\t\t\t\t\t<Type>PASS OUT</Type>\r\n\t\t\t\t\t<Id>5-145BXY</Id>\r\n\t\t\t\t\t<ObservationTypeBDARGUS>Dealer refuses to attend</ObservationTypeBDARGUS>\r\n\t\t\t\t\t<ContactNameARGUS>Vasquez</ContactNameARGUS>\r\n\t\t\t\t\t<PassOutAmountBDARGUS>199</PassOutAmountBDARGUS>\r\n\t\t\t\t\t<PassOutAmountCurrencyBDARGUS>EUR</PassOutAmountCurrencyBDARGUS>\r\n\t\t\t\t\t<PaymentTypeBDARGUS>AUTO GOP</PaymentTypeBDARGUS>\r\n\t\t\t\t\t<ValidatedTimeARGUS>03/24/2021 07:04:54</ValidatedTimeARGUS>\r\n\t\t\t\t\t<ValidatedByLogin>BDNORMAL</ValidatedByLogin>\r\n\t\t\t\t\t<ValidatedByMailId>abc@volvo.com</ValidatedByMailId>\r\n\t\t\t\t</SRBdPassoutLoginfoArgus>\r\n\t\t\t</ListOfSRBdPassoutLoginfoArgus>\r\n\t\t</ServiceRequestBdArgus>\r\n\t</ListOfServiceRequestBreakdownArgus>\r\n</SiebelMessage>";
            // string myQueueItem = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n<SiebelMessage MessageId=\"ca147478-bae9-4c4b-a77b-d7166fe8ae36\" EventTimestamp=\"01/09/2022 10:06:20\" TransType=\"Mercurius.Event.UpdateCase\" MessageType=\"Integration Object\" IntObjectName=\"Mercurius Case Detail Info ARGUS\" IntObjectFormat=\"Siebel Hierarchical\">\r\n    <ListOfServiceRequestBreakdownArgus>\r\n        <ServiceRequestBdArgus>\r\n            <CommunicationTypeARGUS>PHONE</CommunicationTypeARGUS>\r\n            <CaseNumberARGUS>2745431841</CaseNumberARGUS>\r\n            <CaseNumberJARVIS></CaseNumberJARVIS>\r\n            <Status>050</Status>\r\n            <BrandARGUS>VOLVO TRUCK&amp;BUS</BrandARGUS>\r\n            <CallerNameARGUS>Rupa</CallerNameARGUS>\r\n            <CallbacknumberARGUS>+46739026239</CallbacknumberARGUS>\r\n            <CallerLanguageARGUS>GERMAN</CallerLanguageARGUS>\r\n            <CallerLanguageCodeARGUS>DEU</CallerLanguageCodeARGUS>\r\n            <CallerRelationTypeARGUS>CUSTOMER</CallerRelationTypeARGUS>\r\n            <DriverNameARGUS>Tomasz</DriverNameARGUS>\r\n            <DriverLanguageARGUS>DUTCH</DriverLanguageARGUS>\r\n            <DriverLanguageCodeARGUS>NLD</DriverLanguageCodeARGUS>\r\n           <TDIPartner>0001610</TDIPartner>\r\n\t\t\t<TDIMarket>0001610</TDIMarket>\r\n            <HomeDealerCountryBDARGUS>GERMANY</HomeDealerCountryBDARGUS>\r\n            <HomeDealerCountryCodeBDARGUS>DE</HomeDealerCountryCodeBDARGUS>\r\n            <HomeDealerBrandBDARGUS>RENAULT</HomeDealerBrandBDARGUS>\r\n            <VINSerialBDARGUS>223</VINSerialBDARGUS>\r\n            <RegistrationBDARGUS>07C7749</RegistrationBDARGUS>\r\n            <RegTrailerBDARGUS>PFZ495</RegTrailerBDARGUS>\r\n            <LoadCargoBDARGUS>load 20t</LoadCargoBDARGUS>\r\n            <MileageUnitsBDARGUS>MILES</MileageUnitsBDARGUS>\r\n            <CustomerInstructions>Brakes not working Properly</CustomerInstructions>\r\n            <MileageBDARGUS>23456</MileageBDARGUS>\r\n            <LocationBDARGUS>Helsingborg Road</LocationBDARGUS>\r\n            <ReportedBreakDownCountry>GERMANY</ReportedBreakDownCountry>\r\n            <ReportedBreakDownCountryCode>DE</ReportedBreakDownCountryCode>\r\n            <PersonalLongitudeBDARGUS>12.34</PersonalLongitudeBDARGUS>\r\n            <PersonalLatitudeBDARGUS>34.56</PersonalLatitudeBDARGUS>\r\n            <CustomerNumARGUS>1610-018100</CustomerNumARGUS>\r\n            <UpdatedByLogin>MERCURIUS</UpdatedByLogin>\r\n            <UpdatedByMailId>merc@volvo.com</UpdatedByMailId>\r\n        </ServiceRequestBdArgus>\r\n    </ListOfServiceRequestBreakdownArgus>\r\n</SiebelMessage>";
            // string myQueueItem = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n<!-- edited with XMLSpy v2011 rel. 3 sp1 (http://www.altova.com) by VOLVO INFORMATION TECH AB (VOLVO INFORMATION TECH AB) -->\r\n<SiebelMessage MessageId=\"ca147478-bae9-4c4b-a77b-d7166fe8ae36\" EventTimestamp=\"01/09/2022 10:06:20\" TransType=\"Mercurius.Event.CreateCase\" MessageType=\"Integration Object\" IntObjectName=\"Mercurius Case Detail Info ARGUS\" IntObjectFormat=\"Siebel Hierarchical\">\r\n\t<ListOfServiceRequestBreakdownArgus>\r\n\t\t<ServiceRequestBdArgus>\r\n            <CommunicationTypeARGUS>WEB</CommunicationTypeARGUS>\t\r\n\t\t\t<CaseNumberARGUS>2345766433</CaseNumberARGUS>\r\n\t\t\t<CaseNumberJarvis></CaseNumberJarvis>\t\t\t\r\n\t\t\t<Status>000</Status>\r\n\t\t\t<CaseOpenAtARGUS>02/08/2022 09:09:20</CaseOpenAtARGUS>\r\n\t\t\t<BrandARGUS>VOLVO TRUCK&amp;BUS</BrandARGUS>\r\n\t\t\t<CallerNameARGUS>MandyTest</CallerNameARGUS>\r\n\t\t\t<CallbacknumberARGUS>+46739026228</CallbacknumberARGUS>\r\n\t\t\t<CallerLanguageARGUS>DUTCH</CallerLanguageARGUS>\r\n\t\t\t<CallerLanguageCodeARGUS>DEU</CallerLanguageCodeARGUS>\r\n\t\t\t<CallerRelationTypeARGUS>CUSTOMER</CallerRelationTypeARGUS><AssistanceARGUS>BD Delayed</AssistanceARGUS>\r\n\t\t\t<DriverNameARGUS>Tom</DriverNameARGUS>\r\n\t\t\t<DriverPhoneARGUS>+46</DriverPhoneARGUS>\r\n\t\t\t<DriverLanguageARGUS>DUTCH</DriverLanguageARGUS>\r\n\t\t\t<DriverLanguageCodeARGUS>DEU</DriverLanguageCodeARGUS>\r\n\t\t\t<VINSerialBDARGUS>VF644AGE000010631</VINSerialBDARGUS>\r\n\t\t\t<RegistrationBDARGUS>07C7749</RegistrationBDARGUS>\r\n\t\t\t<RegTrailerBDARGUS/>\r\n\t\t    <LoadCargoBDARGUS/>\r\n            <MileageUnitsBDARGUS>KM</MileageUnitsBDARGUS>\r\n\t\t\t<MileageBDARGUS/>\t\t\t\r\n\t\t\t<LocationBDARGUS>We are close to Helsingborg and having Issues</LocationBDARGUS>\r\n\t\t\t<CustomerInstructions>Expandys UH EXPANDYS 1+3 MAXI 450000KM  23/09/2013 22/09/2017</CustomerInstructions>\r\n\t\t\t<ReportedBreakDownCountry>SWEDEN</ReportedBreakDownCountry>\r\n\t\t\t<ReportedBreakDownCountryCode>DE</ReportedBreakDownCountryCode>\r\n\t\t\t<CustomerNumARGUS>1610-018100</CustomerNumARGUS>\r\n\t\t\t<HomeDealerIdBDARGUS>1610-018100</HomeDealerIdBDARGUS>\r\n\t\t\t<CreatedByLogin>MERCURIUS</CreatedByLogin>\r\n\t\t\t<CreatedByMailId>mercurius@noemail.com</CreatedByMailId>\r\n\t\t</ServiceRequestBdArgus>\r\n</ListOfServiceRequestBreakdownArgus>\r\n</SiebelMessage>\r\n\r\n";
            // string myQueueItem = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n<SiebelMessage MessageId=\"1-ZKL\" TransType=\"Mercurius.Event.RepairInfo2\" MessageType=\"Integration Object\" IntObjectName=\"Mercurius Case Detail Info ARGUS\" IntObjectFormat=\"Siebel Hierarchical\">\r\n\t<ListOfServiceRequestBreakdownArgus>\r\n\t\t<ServiceRequestBdArgus>\r\n\t\t\t<MercuriusIntegrationRule>Mercurius Integration Rule</MercuriusIntegrationRule>\r\n\t\t\t<CaseNumberARGUS>1212TR</CaseNumberARGUS>\r\n\t\t\t<CustomerRefNumber>XXXX</CustomerRefNumber>\r\n\t\t\t<ListOfSRBdPassoutLoginfoArgus>\r\n\t\t\t\t<SRBdPassoutLoginfoArgus>\r\n\t\t\t\t\t<HomeDealerNameBDARGUS>VISA</HomeDealerNameBDARGUS>\r\n\t\t\t\t\t<HomeDealerIdBDARGUS>101003</HomeDealerIdBDARGUS>\r\n\t\t\t\t\t<HomeDealerCountryBDARGUS>DENMARK</HomeDealerCountryBDARGUS>\r\n\t\t\t\t\t<HomeDealerCountryCodeBDARGUS>DK</HomeDealerCountryCodeBDARGUS>\r\n\t\t\t\t\t<HomeDealerBrandBDARGUS>VOLVO TRUCK&amp;BUS</HomeDealerBrandBDARGUS>\r\n\t\t\t\t\t<ListOfSRBreakdownRepairinfoArgus>\r\n\t\t\t\t\t\t<SRBreakdownRepairinfoArgus>\r\n\t\t\t\t\t\t\t<Comment>Diagnose: INFO - Part Available: No - Parts: PARTS INFO</Comment>\r\n\t\t\t\t\t\t\t<Language>DANISH</Language>\r\n\t\t\t\t\t\t\t<LanguageCode>DAN</LanguageCode>\r\n\t\t\t\t\t\t\t<Audience>Public</Audience>\r\n\t\t\t\t\t\t\t<Type>REPAIR INFO</Type>\r\n\t\t\t\t\t\t\t<HomeDealerIdBDARGUS>C-AVIS-123456</HomeDealerIdBDARGUS>\r\n\t\t\t\t\t\t\t<HomeDealerCountryBDARGUS>DENMARK</HomeDealerCountryBDARGUS>\r\n\t\t\t\t\t\t\t<HomeDealerCountryCodeBDARGUS>DK</HomeDealerCountryCodeBDARGUS>\r\n\t\t\t\t\t\t\t<HomeDealerBrandBDARGUS>VOLVO TRUCK&amp;BUS</HomeDealerBrandBDARGUS>\r\n\t\t\t\t\t\t\t<Created>11/23/2021 10:45:18</Created>\r\n\t\t\t\t\t\t\t<CreatorPartnerFullNameARGUS>STANDARD MERCURIUS</CreatorPartnerFullNameARGUS>\r\n\t\t\t\t\t\t</SRBreakdownRepairinfoArgus>\r\n\t\t\t\t\t</ListOfSRBreakdownRepairinfoArgus>\r\n\t\t\t\t</SRBdPassoutLoginfoArgus>\r\n\t\t\t</ListOfSRBdPassoutLoginfoArgus>\r\n\t\t</ServiceRequestBdArgus>\r\n\t</ListOfServiceRequestBreakdownArgus>\r\n</SiebelMessage>";
            // string myQueueItem = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n<SiebelMessage MessageId=\"1-ZKL\" TransType=\"Mercurius.Event.RepairInfo1\" MessageType=\"Integration Object\" IntObjectName=\"Mercurius Case Detail Info ARGUS\" IntObjectFormat=\"Siebel Hierarchical\">\r\n\t<ListOfServiceRequestBreakdownArgus>\r\n\t\t<ServiceRequestBdArgus>\r\n\t\t\t<MercuriusIntegrationRule>Mercurius Integration Rule</MercuriusIntegrationRule>\r\n\t\t\t<CaseNumberARGUS>1212TR</CaseNumberARGUS>\r\n\t\t\t<CustomerRefNumber>XXXX</CustomerRefNumber>\r\n\t\t\t<ListOfSRBdPassoutLoginfoArgus>\r\n\t\t\t\t<SRBdPassoutLoginfoArgus>\r\n\t\t\t\t\t<HomeDealerNameBDARGUS>VISA</HomeDealerNameBDARGUS>\r\n\t\t\t\t\t<HomeDealerIdBDARGUS>101003</HomeDealerIdBDARGUS>\r\n\t\t\t\t\t<HomeDealerCountryBDARGUS>DENMARK</HomeDealerCountryBDARGUS>\r\n\t\t\t\t\t<HomeDealerCountryCodeBDARGUS>DK</HomeDealerCountryCodeBDARGUS>\r\n\t\t\t\t\t<HomeDealerBrandBDARGUS>VOLVO TRUCK&amp;BUS</HomeDealerBrandBDARGUS>\r\n\t\t\t\t\t<ListOfSRBreakdownRepairinfoArgus>\r\n\t\t\t\t\t\t<SRBreakdownRepairinfoArgus>\r\n\t\t\t\t\t\t\t<Comment>Diagnose: test updated - Part Available: No - Parts: PARTS INFO</Comment>\r\n\t\t\t\t\t\t\t<Language>DANISH</Language>\r\n\t\t\t\t\t\t\t<LanguageCode>DEU</LanguageCode>\r\n\t\t\t\t\t\t\t<Audience>Public</Audience>\r\n\t\t\t\t\t\t\t<Type>TOWING INFO</Type>\r\n\t\t\t\t\t\t\t<HomeDealerIdBDARGUS>C-AVIS-123456</HomeDealerIdBDARGUS>\r\n\t\t\t\t\t\t\t<HomeDealerCountryBDARGUS>DENMARK</HomeDealerCountryBDARGUS>\r\n\t\t\t\t\t\t\t<HomeDealerCountryCodeBDARGUS>DK</HomeDealerCountryCodeBDARGUS>\r\n\t\t\t\t\t\t\t<HomeDealerBrandBDARGUS>VOLVO TRUCK&amp;BUS</HomeDealerBrandBDARGUS>\r\n\t\t\t\t\t\t\t<Created>11/23/2021 10:45:18</Created>\r\n\t\t\t\t\t\t\t<CreatorPartnerFullNameARGUS>STANDARD MERCURIUS</CreatorPartnerFullNameARGUS>\r\n\t\t\t\t\t\t</SRBreakdownRepairinfoArgus>\r\n\t\t\t\t\t</ListOfSRBreakdownRepairinfoArgus>\r\n\t\t\t\t</SRBdPassoutLoginfoArgus>\r\n\t\t\t</ListOfSRBdPassoutLoginfoArgus>\r\n\t\t</ServiceRequestBdArgus>\r\n\t</ListOfServiceRequestBreakdownArgus>\r\n</SiebelMessage>";
            // string myQueueItem = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n<SiebelMessage MessageId=\"1-128FGX\" EventTimestamp=\"01/09/2022 10:06:26\" TransType=\"Mercurius.Event.PassOut\" MessageType=\"Integration Object\" IntObjectName=\"Mercurius Case Detail Info ARGUS\" IntObjectFormat=\"Siebel Hierarchical\">\r\n    <ListOfServiceRequestBreakdownArgus>\r\n        <ServiceRequestBdArgus>\r\n            <CaseNumberARGUS>59492427472</CaseNumberARGUS>\r\n            <CaseNumberJARVIS></CaseNumberJARVIS>\r\n            <ListOfSRBdPassoutLoginfoArgus>\r\n                <SRBdPassoutLoginfoArgus>\r\n                    <TDIPartner>0001610</TDIPartner>\r\n                    <TDIMarket>0001610</TDIMarket>\r\n                    <HomeDealerIdBDARGUS>1001046163</HomeDealerIdBDARGUS>\r\n                    <HomeDealerCountryBDARGUS>GERMANY</HomeDealerCountryBDARGUS>\r\n                    <HomeDealerCountryCodeBDARGUS>DE</HomeDealerCountryCodeBDARGUS>\r\n                    <HomeDealerBrandBDARGUS>VOLVO</HomeDealerBrandBDARGUS>\r\n                    <Type>PASS OUT</Type>\r\n                    <Id>29--915BXY</Id>\r\n                    <ObservationTypeBDARGUS>Dealer refuses to attend</ObservationTypeBDARGUS>\r\n                    <ContactNameARGUS>Vasquez</ContactNameARGUS>\r\n                    <PassOutAmountBDARGUS>10</PassOutAmountBDARGUS>\r\n                    <PassOutAmountCurrencyBDARGUS>EUR</PassOutAmountCurrencyBDARGUS>\r\n                    <PaymentTypeBDARGUS>AUTO GOP</PaymentTypeBDARGUS>\r\n                    <ValidatedTimeARGUS>03/24/2021 07:04:54</ValidatedTimeARGUS>\r\n                    <ValidatedByLogin>MERCURIUS</ValidatedByLogin>\r\n                    <ValidatedByMailId>mercurius@noemail.com</ValidatedByMailId>\r\n                    <HomeDealerNotUsedFlg>N</HomeDealerNotUsedFlg>\r\n                </SRBdPassoutLoginfoArgus>\r\n            </ListOfSRBdPassoutLoginfoArgus>\r\n        </ServiceRequestBdArgus>\r\n    </ListOfServiceRequestBreakdownArgus>\r\n</SiebelMessage>";
            // string myQueueItem = "<SiebelMessage MessageId=\"ca147478-bae9-4c4b-a77b-d7166fe8ae36\" EventTimestamp=\"03/24/2021 23:04:54\" TransType=\"Mercurius.Event.DelayedETAUpdate1\" MessageType=\"Integration Object\" IntObjectName=\"Mercurius Case Detail Info ARGUS\" IntObjectFormat=\"Siebel Hierarchical\">\r\n<ListOfServiceRequestBreakdownArgus>\r\n\t\t<ServiceRequestBdArgus>\r\n         \t<CaseNumberARGUS>2745431841</CaseNumberARGUS>\r\n\t\t    <CaseNumberJARVIS>DEV-01821-Q9B4C</CaseNumberJARVIS>\t\t\t\r\n\t\t\t<ListOfSRBdPassoutLoginfoArgus>\r\n\t\t\t\t<SRBdPassoutLoginfoArgus>\r\n\t\t\t\t\t<HomeDealerIdBDARGUS>1000656200</HomeDealerIdBDARGUS>\r\n\t\t\t\t\t<HomeDealerBrandBDARGUS>RENAULT</HomeDealerBrandBDARGUS>\r\n\t\t\t\t\t<HomeDealerCountryBDARGUS>GERMANY</HomeDealerCountryBDARGUS>\r\n\t\t\t\t\t<HomeDealerCountryCodeBDARGUS>DE</HomeDealerCountryCodeBDARGUS>\r\n\t\t\t\t\t<ListOfSrBreakdownEtaLogArgus>\r\n\t\t\t\t\t\t<SrBreakdownEtaLogArgus>\r\n\t\t\t\t\t\t\t<ETADelayedDateBDARGUS>03/05/2018 18:30:00</ETADelayedDateBDARGUS>\r\n\t\t\t\t\t\t\t<Comments>delayed due to traffic</Comments>\r\n\t\t\t\t\t\t\t<Language>FINNISH</Language>\r\n\t\t\t\t\t\t\t<LanguageCode>FIN</LanguageCode>\r\n\t\t\t\t\t\t\t<UpdatedByMailId>mercurius@noemail.com</UpdatedByMailId>\r\n\t\t\t\t\t        <UpdatedByLogin>MERCURIUS</UpdatedByLogin>\t\r\n\t\t\t\t\t\t</SrBreakdownEtaLogArgus>\r\n\t\t\t\t\t</ListOfSrBreakdownEtaLogArgus>\t\t\t\t\t\r\n\t\t\t\t</SRBdPassoutLoginfoArgus>\r\n\t\t\t</ListOfSRBdPassoutLoginfoArgus>\r\n\t\t</ServiceRequestBdArgus>\r\n\t</ListOfServiceRequestBreakdownArgus>\r\n</SiebelMessage>";
            // string myQueueItem = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n<!-- edited with XMLSpy v2011 rel. 3 sp1 (http://www.altova.com) by VOLVO INFORMATION TECH AB (VOLVO INFORMATION TECH AB) -->\r\n<SiebelMessage MessageId=\"ca147478-bae9-4c4b-a77b-d7166fe8ae36\" EventTimestamp=\"03/24/2023 23:04:54\" TransType=\"Mercurius.Event.DelayedETAUpdate2\" MessageType=\"Integration Object\" IntObjectName=\"Mercurius Case Detail Info ARGUS\" IntObjectFormat=\"Siebel Hierarchical\">\r\n    <ListOfServiceRequestBreakdownArgus>\r\n        <ServiceRequestBdArgus>         \r\n            <CaseNumberARGUS>59492427472</CaseNumberARGUS>    \r\n            <CaseNumberJARVIS></CaseNumberJARVIS>\r\n            <ListOfSRBdPassoutLoginfoArgus>\r\n                <SRBdPassoutLoginfoArgus>\r\n                    <HomeDealerIdBDARGUS>1000656200</HomeDealerIdBDARGUS>\r\n                    <TDIPartner>0001610</TDIPartner> \r\n                    <TDIMarket>0001610</TDIMarket> \r\n                    <HomeDealerBrandBDARGUS>RENAULT</HomeDealerBrandBDARGUS>\r\n                    <HomeDealerCountryBDARGUS>GERMANY</HomeDealerCountryBDARGUS>\r\n                    <HomeDealerCountryCodeBDARGUS>DE</HomeDealerCountryCodeBDARGUS>\r\n                    <ListOfSrBreakdownEtaLogArgus>\r\n                        <SrBreakdownEtaLogArgus>\r\n                            <ETADelayedDateBDARGUS>03/05/2019 18:30:00</ETADelayedDateBDARGUS>\r\n                            <Comments>Vivek delayed due to traffic</Comments>\r\n                            <Language>DUTCH</Language>\r\n                            <LanguageCode>NLD</LanguageCode>\r\n                            <UpdatedByMailId>mercurius@noemail.com</UpdatedByMailId>        \r\n                            <UpdatedByLogin>MERCURIUS</UpdatedByLogin>\r\n                        </SrBreakdownEtaLogArgus>\r\n                    </ListOfSrBreakdownEtaLogArgus>\r\n                </SRBdPassoutLoginfoArgus>\r\n            </ListOfSRBdPassoutLoginfoArgus>\r\n        </ServiceRequestBdArgus>\r\n    </ListOfServiceRequestBreakdownArgus>\r\n</SiebelMessage>";
            // string myQueueItem = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n<SiebelMessage MessageId=\"1-ZKL\" TransType=\"Mercurius.Event.RepairInfo2\" MessageType=\"Integration Object\" IntObjectName=\"Mercurius Case Detail Info ARGUS\" IntObjectFormat=\"Siebel Hierarchical\">\r\n\t<ListOfServiceRequestBreakdownArgus>\r\n\t\t<ServiceRequestBdArgus>\r\n\t\t\t<CaseNumberARGUS>0987654312</CaseNumberARGUS>\r\n\t\t\t<CaseNumberJARVIS>DEV-01289-K2X1Y</CaseNumberJARVIS>\r\n\t\t\t<ListOfSRBdPassoutLoginfoArgus>\r\n\t\t\t\t<SRBdPassoutLoginfoArgus>\r\n\t\t\t\t\t<TDIPartner>0001610</TDIPartner> \r\n                    <TDIMarket>0001610</TDIMarket> \r\n\t\t\t\t\t<HomeDealerCountryBDARGUS>DENMARK</HomeDealerCountryBDARGUS>\r\n\t\t\t\t\t<HomeDealerCountryCodeBDARGUS>DK</HomeDealerCountryCodeBDARGUS>\r\n\t\t\t\t\t<HomeDealerBrandBDARGUS>VOLVO TRUCK&amp;BUS</HomeDealerBrandBDARGUS>     \r\n\t\t\t\t\t<ListOfSRBreakdownRepairinfoArgus>\r\n\t\t\t\t\t\t<SRBreakdownRepairinfoArgus>\r\n\t\t\t\t\t\t\t<Comment>Diagnose: INFO - Part Available: No - Parts: PARTS INFO</Comment>\r\n\t\t\t\t\t\t\t<Language>US English</Language>\r\n\t\t\t\t\t\t\t<LanguageCode>ENU</LanguageCode>\r\n\t\t\t\t\t\t\t<Audience>Public</Audience>\r\n\t\t\t\t\t\t\t<Type>REPAIR INFO</Type>\r\n\t\t\t\t\t\t\t<UpdatedByMailId>mercurius@noemail.com</UpdatedByMailId>\r\n\t\t\t\t\t        <UpdatedByLogin>MERCURIUS</UpdatedByLogin>\t\r\n\t\t\t\t\t\t</SRBreakdownRepairinfoArgus>\r\n\t\t\t\t\t</ListOfSRBreakdownRepairinfoArgus>\r\n\t\t\t\t</SRBdPassoutLoginfoArgus>\r\n\t\t\t</ListOfSRBdPassoutLoginfoArgus>\r\n\t\t</ServiceRequestBdArgus>\r\n\t</ListOfServiceRequestBreakdownArgus>\r\n</SiebelMessage>";
            //  string myQueueItem = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n<SiebelMessage MessageId=\"ca147478-bae9-4c4b-a77b-d7166fe8ae36\" EventTimestamp=\"01/09/2022 10:06:20\" TransType=\"Mercurius.Event.AddGOP+\" MessageType=\"Integration Object\" IntObjectName=\"Mercurius Case Detail Info ARGUS\" IntObjectFormat=\"Siebel Hierarchical\">\r\n    <ListOfServiceRequestBreakdownArgus>\r\n        <ServiceRequestBdArgus>\r\n            <CaseNumberARGUS>284574Z33</CaseNumberARGUS>\r\n            <CaseNumberJARVIS></CaseNumberJARVIS>\r\n            <ListOfSRBreakdownGopPlusArgus>\r\n                <SRBreakdownGopPlusArgus>\r\n                    <Id>X6-123</Id>\r\n                    <IdJARVIS></IdJARVIS>\r\n                    <HomeDealerIdBDARGUS>101003</HomeDealerIdBDARGUS>\r\n                    <HomeDealerCountryBDARGUS>DENMARK</HomeDealerCountryBDARGUS>\r\n                    <HomeDealerCountryCodeBDARGUS>DK</HomeDealerCountryCodeBDARGUS>\r\n                    <HomeDealerBrandBDARGUS>VOLVO TRUCK&amp;BUS</HomeDealerBrandBDARGUS>\r\n                    <TDIPartner>0001610</TDIPartner>\r\n                    <TDIMarket>0001610</TDIMarket>\r\n                    <Comment>Issue with Truck</Comment>\r\n     <PaymentTypeBDARGUS>CASH</PaymentTypeBDARGUS>               <GOPValueBDARGUS>1000.00</GOPValueBDARGUS>\r\n                    <GOPCurrencyBDARGUS>EUR</GOPCurrencyBDARGUS>\r\n                    <Type>GOP+ HD</Type>\r\n                    <Language>ENGLISH</Language>\r\n                    <LanguageCode>ENU</LanguageCode>\r\n                    <HDApprovedARGUS>Y</HDApprovedARGUS>\r\n                    <HDContactARGUS>JT</HDContactARGUS>\r\n                    <GOPContactBDARGUS/>\r\n                    <UpdatedByLogin>MERCURIUS</UpdatedByLogin>\r\n                    <UpdatedByMailId>MERCURIUS@noemail.com</UpdatedByMailId>\r\n                </SRBreakdownGopPlusArgus>\r\n            </ListOfSRBreakdownGopPlusArgus>\r\n        </ServiceRequestBdArgus>\r\n    </ListOfServiceRequestBreakdownArgus>\r\n</SiebelMessage>\r\n";
            // string myQueueItem = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n<SiebelMessage MessageId=\"ca147478-bae9-4c4b-a77b-d7166fe8ae36\" EventTimestamp=\"01/09/2022 10:06:20\" TransType=\"Mercurius.Event.AddGOP+\" MessageType=\"Integration Object\" IntObjectName=\"Mercurius Case Detail Info ARGUS\" IntObjectFormat=\"Siebel Hierarchical\">\r\n    <ListOfServiceRequestBreakdownArgus>\r\n        <ServiceRequestBdArgus>\r\n            <CaseNumberARGUS>284574Z33</CaseNumberARGUS>\r\n            <CaseNumberJARVIS></CaseNumberJARVIS>\r\n            <ListOfSRBreakdownGopPlusArgus>\r\n                <SRBreakdownGopPlusArgus>\r\n                    <Id>X3-123</Id>\r\n                    <IdJARVIS></IdJARVIS>\r\n                    <HomeDealerIdBDARGUS>101003</HomeDealerIdBDARGUS>\r\n                    <HomeDealerCountryBDARGUS>DENMARK</HomeDealerCountryBDARGUS>\r\n                    <HomeDealerCountryCodeBDARGUS>DK</HomeDealerCountryCodeBDARGUS>\r\n                    <HomeDealerBrandBDARGUS>VOLVO TRUCK&amp;BUS</HomeDealerBrandBDARGUS>\r\n                    <TDIPartner>0001610</TDIPartner>\r\n                    <TDIMarket>0001610</TDIMarket>\r\n                    <Comment>Issue with Truck</Comment>\r\n                    <GOPValueBDARGUS>800.00</GOPValueBDARGUS>\r\n                    <GOPCurrencyBDARGUS>EUR</GOPCurrencyBDARGUS>\r\n                    <Type>GOP+</Type>\r\n                    <Language>ENGLISH</Language>\r\n                    <LanguageCode>ENU</LanguageCode>\r\n                    <PaymentTypeBDARGUS>CASH</PaymentTypeBDARGUS>\r\n                    <HDApprovedARGUS>N</HDApprovedARGUS>\r\n                    <HDContactARGUS>JT</HDContactARGUS>\r\n                    <GOPContactBDARGUS>JT</GOPContactBDARGUS>\r\n                    <UpdatedByLogin>MERCURIUS</UpdatedByLogin>\r\n                    <UpdatedByMailId>MERCURIUS@noemail.com</UpdatedByMailId>\r\n                </SRBreakdownGopPlusArgus>\r\n            </ListOfSRBreakdownGopPlusArgus>\r\n        </ServiceRequestBdArgus>\r\n    </ListOfServiceRequestBreakdownArgus>\r\n</SiebelMessage>\r\n";
            // string myQueueItem = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n<SiebelMessage EventTimestamp=\"04/30/2023 09:44:31\" MessageId=\"1-2S6PU0\" TransType=\"Mercurius.Event.ATAUpdate\" IntObjectName=\"Mercurius Case Detail Info ARGUS\" MessageType=\"Integration Object\" IntObjectFormat=\"Siebel Hierarchical\">\r\n    <ListOfServiceRequestBreakdownArgus>\r\n        <ServiceRequestBdArgus>\r\n            <CaseNumberARGUS>59492427472</CaseNumberARGUS>\r\n            <CaseNumberJARVIS></CaseNumberJARVIS>\r\n            <ListOfSRBdPassoutLoginfoArgus>\r\n                <SRBdPassoutLoginfoArgus>\r\n                    <HomeDealerCountryCodeBDARGUS>SLOVAKIA</HomeDealerCountryCodeBDARGUS>\r\n                    <HomeDealerCountryBDARGUS>SLOVAKIA</HomeDealerCountryBDARGUS>\r\n                    <HomeDealerIdBDARGUS>311529</HomeDealerIdBDARGUS>\r\n                    <ValidatedByLogin>MERCURIUS</ValidatedByLogin>\r\n                    <ValidatedByMailId>Mercurius@noemail.com</ValidatedByMailId>\r\n                    <ATADateTimeBDARGUS>04/30/2023 09:44:00</ATADateTimeBDARGUS>\r\n                    <HomeDealerBrandBDARGUS>RENAULT</HomeDealerBrandBDARGUS>\r\n                    <TDIPartner>0001610</TDIPartner>\r\n                    <TDIMarket>0001610</TDIMarket>\r\n                </SRBdPassoutLoginfoArgus>\r\n            </ListOfSRBdPassoutLoginfoArgus>\r\n        </ServiceRequestBdArgus>\r\n    </ListOfServiceRequestBreakdownArgus>\r\n</SiebelMessage>";
            // string myQueueItem = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n<SiebelMessage EventTimestamp=\"04/30/2023 12:44:31\" MessageId=\"1-2S6PU0\" TransType=\"Mercurius.Event.GPSETAUpdate\" IntObjectName=\"Mercurius Case Detail Info ARGUS\" MessageType=\"Integration Object\" IntObjectFormat=\"Siebel Hierarchical\">\r\n    <ListOfServiceRequestBreakdownArgus>\r\n        <ServiceRequestBdArgus>\r\n            <CaseNumberARGUS>59492427472</CaseNumberARGUS>\r\n            <CaseNumberJARVIS></CaseNumberJARVIS>\r\n            <ListOfSRBdPassoutLoginfoArgus>\r\n                <SRBdPassoutLoginfoArgus>\r\n                    <HomeDealerCountryCodeBDARGUS>SLOVAKIA</HomeDealerCountryCodeBDARGUS>\r\n                    <HomeDealerCountryBDARGUS>SLOVAKIA</HomeDealerCountryBDARGUS>\r\n                    <HomeDealerIdBDARGUS>311529</HomeDealerIdBDARGUS>\r\n                    <ValidatedByLogin>MERCURIUS</ValidatedByLogin>\r\n                    <ValidatedByMailId>Mercurius@noemail.com</ValidatedByMailId>\r\n                    <GPSETAARGUS>04/30/2023 12:44:00</GPSETAARGUS>\r\n                    <HomeDealerBrandBDARGUS>RENAULT</HomeDealerBrandBDARGUS>\r\n                    <TDIPartner>0001610</TDIPartner>\r\n                    <TDIMarket>0001610</TDIMarket>\r\n                </SRBdPassoutLoginfoArgus>\r\n            </ListOfSRBdPassoutLoginfoArgus>\r\n        </ServiceRequestBdArgus>\r\n    </ListOfServiceRequestBreakdownArgus>\r\n</SiebelMessage>";
            // string myQueueItem = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n<SiebelMessage MessageId=\"ca147478-bae9-4c4b-a77b-d7166fe8ae36\" EventTimestamp=\"01/10/2023 12:06:40\" TransType=\"Mercurius.Event.AddGOP+\" MessageType=\"Integration Object\" IntObjectName=\"Mercurius Case Detail Info ARGUS\" IntObjectFormat=\"Siebel Hierarchical\">\r\n    <ListOfServiceRequestBreakdownArgus>\r\n        <ServiceRequestBdArgus>\r\n            <CaseNumberARGUS>59492427472</CaseNumberARGUS>\r\n            <CaseNumberJARVIS></CaseNumberJARVIS>\r\n            <ListOfSRBreakdownGopPlusArgus>\r\n                <SRBreakdownGopPlusArgus>\r\n                    <Id>27-Z125</Id>\r\n                    <IdJARVIS></IdJARVIS>\r\n                    <HomeDealerIdBDARGUS>101003</HomeDealerIdBDARGUS>\r\n                    <HomeDealerCountryBDARGUS>DENMARK</HomeDealerCountryBDARGUS>\r\n                    <HomeDealerCountryCodeBDARGUS>DK</HomeDealerCountryCodeBDARGUS>\r\n                    <HomeDealerBrandBDARGUS>VOLVO</HomeDealerBrandBDARGUS>\r\n                    <TDIPartner>0001610</TDIPartner>\r\n                    <TDIMarket>0001610</TDIMarket>\r\n                    <Comment>Issue with Truck</Comment>\r\n                    <GOPValueBDARGUS>80.00</GOPValueBDARGUS>\r\n                    <GOPCurrencyBDARGUS>EUR</GOPCurrencyBDARGUS>\r\n                    <Type>GOP+</Type>\r\n                    <Language>ENGLISH</Language>\r\n                    <LanguageCode>ENU</LanguageCode>\r\n                    <PaymentTypeBDARGUS>CASH</PaymentTypeBDARGUS>\r\n                    <HDApprovedARGUS>N</HDApprovedARGUS>\r\n                    <HDContactARGUS>JT</HDContactARGUS>\r\n                    <GOPContactBDARGUS>JT</GOPContactBDARGUS>\r\n                    <UpdatedByLogin>MERCURIUS</UpdatedByLogin>\r\n                    <UpdatedByMailId>MERCURIUS@noemail.com</UpdatedByMailId>\r\n                </SRBreakdownGopPlusArgus>\r\n            </ListOfSRBreakdownGopPlusArgus>\r\n        </ServiceRequestBdArgus>\r\n    </ListOfServiceRequestBreakdownArgus>\r\n</SiebelMessage>";
            // string myQueueItem = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n<SiebelMessage MessageId=\"1-ZKL\" EventTimestamp=\"04/09/2023 10:06:20\" TransType=\"Mercurius.Event.JobEnd1\" MessageType=\"Integration Object\" IntObjectName=\"Mercurius Case Detail Info ARGUS\" IntObjectFormat=\"Siebel Hierarchical\">\r\n\t<ListOfServiceRequestBreakdownArgus>\r\n\t\t<ServiceRequestBdArgus>\r\n\t\t\t<CaseNumberARGUS>2023000918</CaseNumberARGUS>\r\n\t\t\t<CaseNumberJARVIS>XXXX</CaseNumberJARVIS>\r\n\t\t\t<ActualMileageBDARGUS>359</ActualMileageBDARGUS>\r\n\t\t\t<ActualMileageUnitsBDARGUS>KM</ActualMileageUnitsBDARGUS>\r\n\t\t\t<TrailerRepairFlagBDARGUS>Y</TrailerRepairFlagBDARGUS>\r\n\t\t\t<ListOfSRBdPassoutLoginfoArgus>\r\n\t\t\t\t<SRBdPassoutLoginfoArgus>\r\n\t\t\t\t\t<HomeDealerIdBDARGUS>923231</HomeDealerIdBDARGUS>\r\n\t\t\t\t\t<HomeDealerCountryBDARGUS>DENMARK</HomeDealerCountryBDARGUS>\r\n\t\t\t\t\t<HomeDealerCountryCodeBDARGUS>DK</HomeDealerCountryCodeBDARGUS>\r\n\t\t\t\t\t<HomeDealerBrandBDARGUS>VOLVO</HomeDealerBrandBDARGUS>\r\n\t\t\t\t\t<TDIPartner>0001610</TDIPartner> \r\n\t\t\t\t\t<TDIMarket>0001610</TDIMarket> \r\n\t\t\t\t\t<ListOfSRBreakdownJobEndDetailsArgus>\r\n\t\t\t\t\t\t<SRBreakdownJobEndDetailsArgus>\r\n\t\t\t\t\t\t\t<Comment>HELLO JOB END Detail</Comment>\r\n\t\t\t\t\t\t\t<Language>FINISH</Language>\r\n\t\t\t\t\t\t\t<LanguageCode>FIN</LanguageCode>\r\n\t\t\t\t\t\t\t<Type>TEMP REPAIR</Type>\r\n\t\t\t\t\t\t\t<Audience>Public</Audience>\r\n                            <UpdatedByMailId>mercurius@noemail.com</UpdatedByMailId>\r\n\t\t\t\t\t        <UpdatedByLogin>MERCURIUS</UpdatedByLogin>\t\r\n\t\t\t\t\t\t</SRBreakdownJobEndDetailsArgus>\r\n\t\t\t\t\t</ListOfSRBreakdownJobEndDetailsArgus>\r\n\t\t\t\t</SRBdPassoutLoginfoArgus>\r\n\t\t\t</ListOfSRBdPassoutLoginfoArgus>\r\n\t\t</ServiceRequestBdArgus>\r\n\t</ListOfServiceRequestBreakdownArgus>\r\n</SiebelMessage>";
            // string myQueueItem = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n<SiebelMessage MessageId=\"1-ZKL\" EventTimestamp=\"01/09/2023 11:06:20\" TransType=\"Mercurius.Event.JobEnd1\" MessageType=\"Integration Object\" IntObjectName=\"Mercurius Case Detail Info ARGUS\" IntObjectFormat=\"Siebel Hierarchical\">\r\n\t<ListOfServiceRequestBreakdownArgus>\r\n\t\t<ServiceRequestBdArgus>\r\n\t\t\t<CaseNumberARGUS>59492427472</CaseNumberARGUS>\r\n\t\t\t<CaseNumberJARVIS>XXXX</CaseNumberJARVIS>\r\n\t\t\t<ActualMileageBDARGUS>350</ActualMileageBDARGUS>\r\n\t\t\t<ActualMileageUnitsBDARGUS>KM</ActualMileageUnitsBDARGUS>\r\n\t\t\t<TrailerRepairFlagBDARGUS>Y</TrailerRepairFlagBDARGUS>\r\n\t\t\t<ListOfSRBdPassoutLoginfoArgus>\r\n\t\t\t\t<SRBdPassoutLoginfoArgus>\r\n\t\t\t\t\t<HomeDealerIdBDARGUS>923231</HomeDealerIdBDARGUS>\r\n\t\t\t\t\t<HomeDealerCountryBDARGUS>DENMARK</HomeDealerCountryBDARGUS>\r\n\t\t\t\t\t<HomeDealerCountryCodeBDARGUS>DK</HomeDealerCountryCodeBDARGUS>\r\n\t\t\t\t\t<HomeDealerBrandBDARGUS>VOLVO</HomeDealerBrandBDARGUS>\r\n\t\t\t\t\t<TDIPartner>0001610</TDIPartner> \r\n\t\t\t\t\t<TDIMarket>0001610</TDIMarket> \r\n\t\t\t\t\t<ListOfSRBreakdownJobEndDetailsArgus>\r\n\t\t\t\t\t\t<SRBreakdownJobEndDetailsArgus>\r\n\t\t\t\t\t\t\t<Comment>HELLO JOB END Detail to create</Comment>\r\n\t\t\t\t\t\t\t<Language>FINISH</Language>\r\n\t\t\t\t\t\t\t<LanguageCode>FIN</LanguageCode>\r\n\t\t\t\t\t\t\t<Type>ACTUAL CAUSE/FAULT</Type>\r\n\t\t\t\t\t\t\t<Audience>Public</Audience>\r\n                            <UpdatedByMailId>mercurius@noemail.com</UpdatedByMailId>\r\n\t\t\t\t\t        <UpdatedByLogin>MERCURIUS</UpdatedByLogin>\t\r\n\t\t\t\t\t\t</SRBreakdownJobEndDetailsArgus>\r\n\t\t\t\t\t</ListOfSRBreakdownJobEndDetailsArgus>\r\n\t\t\t\t</SRBdPassoutLoginfoArgus>\r\n\t\t\t</ListOfSRBdPassoutLoginfoArgus>\r\n\t\t</ServiceRequestBdArgus>\r\n\t</ListOfServiceRequestBreakdownArgus>\r\n</SiebelMessage>";
            // string myQueueItem = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n<SiebelMessage MessageId=\"1-ZKL\" EventTimestamp=\"01/09/2023 10:06:20\" TransType=\"Mercurius.Event.JobEnd2\" MessageType=\"Integration Object\" IntObjectName=\"Mercurius Case Detail Info ARGUS\" IntObjectFormat=\"Siebel Hierarchical\">\r\n\t<ListOfServiceRequestBreakdownArgus>\r\n\t\t<ServiceRequestBdArgus>\r\n\t\t\t<CaseNumberARGUS>59492427472</CaseNumberARGUS>\r\n\t\t\t<CaseNumberJARVIS>XXXX</CaseNumberJARVIS>\r\n\t\t\t<ActualMileageBDARGUS>333</ActualMileageBDARGUS>\r\n\t\t\t<ActualMileageUnitsBDARGUS>KM</ActualMileageUnitsBDARGUS>\r\n\t\t\t<TrailerRepairFlagBDARGUS>Y</TrailerRepairFlagBDARGUS>\r\n\t\t\t<ListOfSRBdPassoutLoginfoArgus>\r\n\t\t\t\t<SRBdPassoutLoginfoArgus>\r\n\t\t\t\t\t<HomeDealerIdBDARGUS>923231</HomeDealerIdBDARGUS>\r\n\t\t\t\t\t<HomeDealerCountryBDARGUS>DENMARK</HomeDealerCountryBDARGUS>\r\n\t\t\t\t\t<HomeDealerCountryCodeBDARGUS>DK</HomeDealerCountryCodeBDARGUS>\r\n\t\t\t\t\t<HomeDealerBrandBDARGUS>VOLVO TRUCK&amp;BUS</HomeDealerBrandBDARGUS>\r\n\t\t\t\t\t<TDIPartner>0001610</TDIPartner> \r\n\t\t\t\t\t<TDIMarket>0001610</TDIMarket> \r\n\t\t\t\t\t<ListOfSRBreakdownJobEndDetailsArgus>\r\n\t\t\t\t\t\t<SRBreakdownJobEndDetailsArgus>\r\n\t\t\t\t\t\t\t<Comment>HALLO JOB ENDE Detail update to german</Comment>\r\n\t\t\t\t\t\t\t<Language>FINISH</Language>\r\n\t\t\t\t\t\t\t<LanguageCode>POL</LanguageCode>\r\n\t\t\t\t\t\t\t<Type>TEMP REPAIR</Type>\r\n\t\t\t\t\t\t\t<Audience>Public</Audience>\r\n                            <UpdatedByMailId>mercurius@noemail.com</UpdatedByMailId>\r\n\t\t\t\t\t        <UpdatedByLogin>MERCURIUS</UpdatedByLogin>\t\r\n\t\t\t\t\t\t</SRBreakdownJobEndDetailsArgus>\r\n\t\t\t\t\t</ListOfSRBreakdownJobEndDetailsArgus>\r\n\t\t\t\t</SRBdPassoutLoginfoArgus>\r\n\t\t\t</ListOfSRBdPassoutLoginfoArgus>\r\n\t\t</ServiceRequestBdArgus>\r\n\t</ListOfServiceRequestBreakdownArgus>\r\n</SiebelMessage>";
            // string myQueueItem = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n<SiebelMessage MessageId=\"ca147478-bae9-4c4b-a77b-d7166fe8ae36\" EventTimestamp=\"01/09/2022 10:06:20\" TransType=\"Mercurius.Event.SaveExitMonitorHis\" MessageType=\"Integration Object\" IntObjectName=\"Mercurius Case Detail Info ARGUS\" IntObjectFormat=\"Siebel Hierarchical\">\r\n    <ListOfServiceRequestBreakdownArgus>\r\n        <ServiceRequestBdArgus>\r\n            <CaseNumberARGUS>2023000851</CaseNumberARGUS>\r\n            <CaseNumberJARVIS></CaseNumberJARVIS>\r\n            <ListOfActionBdMonitorHistoryArgus>\r\n                <ActionBdMonitorHistoryArgus>\r\n                    <FUComment>123456sdhash</FUComment>\r\n                    <FUDateARGUS>04/11/2022 05:18:39</FUDateARGUS>\r\n                    <SpecialAttentionARGUS>N</SpecialAttentionARGUS>\r\n                    <UpdatedByLogin>MERCURIUS</UpdatedByLogin>\r\n                    <UpdatedByMailId>MERCURIUS@noemail.com</UpdatedByMailId>\r\n                </ActionBdMonitorHistoryArgus>\r\n            </ListOfActionBdMonitorHistoryArgus>\r\n        </ServiceRequestBdArgus>\r\n    </ListOfServiceRequestBreakdownArgus>\r\n</SiebelMessage>\t\t\t\t\t\t\t\r\n";
            // string myQueueItem = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n<SiebelMessage MessageId=\"ca147478-bae9-4c4b-a77b-d7166fe8ae36\" EventTimestamp=\"01/09/2022 10:06:20\" TransType=\"Mercurius.Event.AddAttachment\" MessageType=\"Integration Object\" IntObjectName=\"Mercurius Case Detail Info ARGUS\" IntObjectFormat=\"Siebel Hierarchical\">\t\t\t\t\t\t\r\n\t<ListOfServiceRequestBreakdownArgus>\r\n\t\t<ServiceRequestBdArgus>\r\n\t\t\t<CaseNumberARGUS>2021000281</CaseNumberARGUS>\r\n\t\t\t<CaseNumberJARVIS>DEV-01009-R7Y2J2</CaseNumberJARVIS>\r\n\t\t\t<ListOfServiceRequestBdAttachmentArgus>\r\n\t\t\t\t<ServiceRequestBdAttachmentArgus>\r\n                    <ActivityFileName>test file</ActivityFileName>\r\n\t\t\t\t\t<ActivityFileSize>17436</ActivityFileSize>\r\n\t\t\t\t\t<ActivityFileExt>xlsx</ActivityFileExt>\t\t\t\t\r\n\t\t\t\t    <SRStatusBDARGUS>020</SRStatusBDARGUS>\r\n                    <Audience>Internal</Audience>\t\t\t\t    \r\n\t\t\t\t\t<ActivityComments>Diagnose: INFO - Part Fixed temporarily</ActivityComments>\r\n\t\t\t\t\t<HomeDealerIdBDARGUS/>\r\n\t\t\t\t\t<HomeDealerCountryBDARGUS/>\r\n\t\t\t\t\t<HomeDealerCountryCodeBDARGUS/>\r\n\t\t\t\t\t<HomeDealerBrandBDARGUS/>\r\n\t\t\t\t\t<TDIPartner/> \r\n\t\t\t\t\t<TDIMarket/>\t\t\t\t\r\n\t\t\t        <SourceARGUS>WEB</SourceARGUS>\r\n\t\t\t\t\t<MCARGUS>HD</MCARGUS>\r\n                    <MCCountryARGUS>SWEDEN</MCCountryARGUS>\r\n\t\t\t\t\t<MCCountryCodeARGUS>SV</MCCountryCodeARGUS>\r\n\t\t\t\t    <UpdatedByMailId>ESERVICE@noemail.com</UpdatedByMailId>\r\n\t\t\t\t\t<UpdatedByLogin>ESERVICE</UpdatedByLogin>\t\r\n\t\t\t\t\t<AttachmentId AttachmentIsTextData=\"false\" Extension=\"xlsx\" ContentId=\"1-2SDO7Z\">UEsDBBQABgAIAAAAIQCO3CYMmQEAALMGAAATAAgCW0NvbnRlbnRfVHlwZXNdLnhtbCCiBAIooAACAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAADEVctOwzAQvCPxD5GvqHHhgBBq2gOPIyABH+DG28Rq/JB3C+3fs3ZLhVBpFbUSlzxs78zsrHY9mixtV3xARONdJS7LoSjA1V4b11Ti/UAAYACAAAACEAE8QsE8IAAABCAQAAIwAAAAAAAAAAAAAAAACmPmIAeGwvd29ya3NoZWV0cy9fcmVscy9zaGVldDIueG1sLnJlbHNQSwECLQAUAAYACAAAACEALyzzyL4AAAAkAQAAIwAAAAAAAAAAAAAAAACpP2IAeGwvZHJhd2luZ3MvX3JlbHMvZHJhd2luZzEueG1sLnJlbHNQSwECLQAUAAYACAAAACEAz+BHtMIBAAAsFQAAJwAAAAAAAAAAAAAAAACoQGIAeGwvcHJpbnRlclNldHRpbmdzL3ByaW50ZXJTZXR0aW5nczEuYmluUEsBAi0AFAAGAAgAAAAhAM/gR7TCAQAALBUAACcAAAAAAAAAAAAAAAAAr0JiAHhsL3ByaW50ZXJTZXR0aW5ncy9wcmludGVyU2V0dGluZ3MyLmJpblBLAQItABQABgAIAAAAIQBs9oQYVgEAAIsCAAARAAAAAAAAAAAAAAAAALZEYgBkb2NQcm9wcy9jb3JlLnhtbFBLAQItABQABgAIAAAAIQD+bMpcjQEAACwDAAAQAAAAAAAAAAAAAAAAAENHYgBkb2NQcm9wcy9hcHAueG1sUEsBAi0AFAAGAAgAAAAhAFjcGt+5AQAA7QUAABMAAAAAAAAAAAAAAAAABkpiAGRvY1Byb3BzL2N1c3RvbS54bWxQSwUGAAAAABMAEwArBQAA+ExiAAAA</AttachmentId>\t\t\r\n\t\t\t\t</ServiceRequestBdAttachmentArgus>\r\n\t\t\t</ListOfServiceRequestBdAttachmentArgus>\r\n\t\t</ServiceRequestBdArgus>\r\n\t</ListOfServiceRequestBreakdownArgus>\r\n</SiebelMessage>";
            string myQueueItem = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n<SiebelMessage MessageId=\"ca147478-bae9-4c4b-a77b-d7166fe8ae36\" EventTimestamp=\"06/06/2023 15:40:40\" TransType=\"Mercurius.Event.UpdateCase\" MessageType=\"Integration Object\" IntObjectName=\"Mercurius Case Detail Info ARGUS\" IntObjectFormat=\"Siebel Hierarchical\">\r\n    <ListOfServiceRequestBreakdownArgus>\r\n        <ServiceRequestBdArgus>\r\n            <CommunicationTypeARGUS>PHONE</CommunicationTypeARGUS>\r\n            <CaseNumberARGUS>2323OPZ35</CaseNumberARGUS>\r\n            <CaseNumberJARVIS>DEV-02664-Y9M2G</CaseNumberJARVIS>\r\n            <Status>020</Status>\r\n            <BrandARGUS>Renault</BrandARGUS>\r\n            <AssistanceARGUS>BD Immediate</AssistanceARGUS>\r\n            <AssistanceType2ARGUS>Other</AssistanceType2ARGUS>\r\n            <CallerNameARGUS>Adil</CallerNameARGUS>\r\n            <CallbacknumberARGUS>+46739026239</CallbacknumberARGUS>\r\n            <CallerLanguageARGUS>GERMAN</CallerLanguageARGUS>\r\n            <CallerLanguageCodeARGUS>DEU</CallerLanguageCodeARGUS>\r\n            <CallerRelationTypeARGUS>CUSTOMER</CallerRelationTypeARGUS>\r\n            <DriverNameARGUS>TomaszT</DriverNameARGUS>\r\n            <DriverLanguageARGUS>FRENCH</DriverLanguageARGUS>\r\n            <DriverLanguageCodeARGUS>FRA</DriverLanguageCodeARGUS>\r\n            <TDIPartner>0001610</TDIPartner>\r\n            <TDIMarket>0001610</TDIMarket>\r\n            <HomeDealerCountryBDARGUS>GERMANY</HomeDealerCountryBDARGUS>\r\n            <HomeDealerCountryCodeBDARGUS>DE</HomeDealerCountryCodeBDARGUS>\r\n            <HomeDealerBrandBDARGUS>RENAULT</HomeDealerBrandBDARGUS>\r\n            <VINSerialBDARGUS>VF644AGE000010631</VINSerialBDARGUS>\r\n            <RegistrationBDARGUS>07C7749</RegistrationBDARGUS>\r\n            <RegTrailerBDARGUS>PFZ495</RegTrailerBDARGUS>\r\n            <LoadCargoBDARGUS>load 20t</LoadCargoBDARGUS>\r\n            <MileageUnitsBDARGUS>KM</MileageUnitsBDARGUS>\r\n            <CustomerInstructions>Brakes not working Properly</CustomerInstructions>\r\n            <MileageBDARGUS>23456</MileageBDARGUS>\r\n            <LocationBDARGUS>Helsingborg Road</LocationBDARGUS>\r\n            <ReportedBreakDownCountry>GERMANY</ReportedBreakDownCountry>\r\n            <ReportedBreakDownCountryCode>DE</ReportedBreakDownCountryCode>\r\n            <PersonalLongitudeBDARGUS>12.34</PersonalLongitudeBDARGUS>\r\n            <PersonalLatitudeBDARGUS>34.56</PersonalLatitudeBDARGUS>\r\n            <CustomerNumARGUS>C-AVIS-123456</CustomerNumARGUS>\r\n            <UpdatedByLogin>MERCURIUS</UpdatedByLogin>\r\n            <UpdatedByMailId>mercuris@volvo.com</UpdatedByMailId>\r\n            <ForceCloseReason>(&lt;=20)  Customer called in error</ForceCloseReason>\r\n            <ForcedCloseType>Donot charge any CCP</ForcedCloseType>\r\n            <ForcedClosedARGUS>N</ForcedClosedARGUS>\r\n            <RequestWorkerLoginId></RequestWorkerLoginId>\r\n            <RequestWorkerBDARGUS>mercurius@noemail.com</RequestWorkerBDARGUS>\r\n        </ServiceRequestBdArgus>\r\n    </ListOfServiceRequestBreakdownArgus>\r\n</SiebelMessage>\t\t\t\t\t\t\r\n";

            // string myQueueItem = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n<SiebelMessage MessageId=\"ca147478-bae9-4c4b-a77b-d7166fe8ae36\" EventTimestamp=\"03/24/2021 23:04:54\" TransType=\"Mercurius.Event.CustWhitelist\" MessageType=\"Integration Object\" IntObjectName=\"Mercurius Account Breakdown ARGUS\" IntObjectFormat=\"Siebel Hierarchical\">\r\n    <ListOfAccountBreakdownArgus>\r\n        <Account>\r\n            <BreakDownCustomerDealerID>C-AVIS-123456</BreakDownCustomerDealerID>\r\n            <FinancialType>WHITELIST</FinancialType>\r\n            <CustomerFSDealerNum>C-AVIS-123456</CustomerFSDealerNum>\r\n            <CustomerFSDealerBrand>RENAULT</CustomerFSDealerBrand>\r\n            <CustomerFSTDIPartner>0001610</CustomerFSTDIPartner>\r\n            <CustomerFSTDIMarket>0001610</CustomerFSTDIMarket>\r\n            <OrderLimit>922337203685477</OrderLimit>\r\n            <OrderLimitCurrency>EUR</OrderLimitCurrency>\r\n            <OrderreferenceNum>222</OrderreferenceNum>\r\n            <FSStartTime>03/05/2018 18:30:00</FSStartTime>\r\n            <FSExpiryDate>03/05/2026 18:30:00</FSExpiryDate>\r\n            <Natinternat>National &amp; International</Natinternat>\r\n            <Updated>03/05/2018 18:30:00</Updated>\r\n            <UpdatedByLogin>ESERVICE</UpdatedByLogin>\r\n            <UpdatedByMailId>ESERVICE@noemail.com</UpdatedByMailId>\r\n        </Account>\r\n    </ListOfAccountBreakdownArgus>\r\n</SiebelMessage>\r\n";
            // string myQueueItem = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n<SiebelMessage MessageId=\"ca147478-bae9-4c4b-a77b-d7166fe8ae36\" EventTimestamp=\"01/09/2022 10:06:20\" TransType=\"Mercurius.Event.VINWhitelist\" MessageType=\"Integration Object\" IntObjectName=\"Mercurius Vehicles Unit Info ARGUS\" IntObjectFormat=\"Siebel Hierarchical\">\r\n\t<ListOfVehiclesUnitArgus>\r\n\t\t<VehicleUnitsBdArgus>\r\n\t\t\t<SerialNumber>VF644AGE000010833</SerialNumber>\r\n\t\t\t<ChassisNumber>909634</ChassisNumber>\r\n\t\t\t<ChassisSeries>9</ChassisSeries>\r\n\t\t\t<ListOfVehicleUnitFinancialArgus>\r\n\t\t\t\t<VehicleUnitFinancialArgus>\r\n\t\t\t\t\t<FinancialType>WHITELIST</FinancialType>\r\n\t\t\t\t\t<DealerNumber>318240</DealerNumber>\r\n\t\t\t\t\t<DealerBrand>RENAULT</DealerBrand>\r\n\t\t\t\t\t<DealerTDIPartner>0001610</DealerTDIPartner>\r\n\t\t\t\t\t<DealerTDIMarket>0001610</DealerTDIMarket>\r\n\t\t\t\t\t<Orderlimit>1000</Orderlimit>\r\n\t\t\t\t\t<OrderCurrency>EUR</OrderCurrency>\r\n\t\t\t\t\t<Orderreference>Temp</Orderreference>\r\n\t\t\t\t\t<StartDate>03/23/2018 09:11:07</StartDate>\r\n\t\t\t\t\t<ExpiryDate>03/23/2020 09:11:07</ExpiryDate>\r\n\t\t\t\t\t<Nationalinternational>National &amp; International</Nationalinternational>\r\n\t\t\t\t\t<Updated>03/05/2018 18:30:00</Updated>\r\n\t\t\t\t\t<UpdatedByLogin>ESERVICE</UpdatedByLogin>\r\n\t\t\t\t\t<UpdatedByMailId>ESERVICE@noemail.com</UpdatedByMailId>\r\n\t\t\t\t</VehicleUnitFinancialArgus>\r\n\t\t\t</ListOfVehicleUnitFinancialArgus>\r\n\t\t</VehicleUnitsBdArgus>\r\n\t</ListOfVehiclesUnitArgus>\r\n</SiebelMessage>";
            log.LogInformation("C# HTTP trigger function processed a request.");
#pragma warning restore S125 // Sections of code should not be commented out
            ILoggerService logger = new LoggerService(log);
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(myQueueItem);
            string json = JsonConvert.SerializeXmlNode(doc.GetElementsByTagName("SiebelMessage").Item(0));
            JObject originalJson = JObject.Parse(json);
            log.LogInformation("parse into jobject..");
            string transType = originalJson["SiebelMessage"]["@TransType"].ToString();
            if (originalJson["SiebelMessage"] != null && originalJson["SiebelMessage"]["ListOfServiceRequestBreakdownArgus"] != null && transType.ToString().ToUpper() == "Mercurius.Event.CreateCase".ToUpper())
            {
                log.LogInformation($"isCreate: {transType.ToUpper()}");
                JObject payLoad = (JObject)originalJson["SiebelMessage"]["ListOfServiceRequestBreakdownArgus"]["ServiceRequestBdArgus"];
                log.LogInformation("payload is ready");
                this.dynamicsClient.SetLoggingReference(logger);
                log.LogInformation("setting logger info in dynamicsclient");
                CreateCasesIn casesIn = new CreateCasesIn(this.dynamicsClient, logger);
                log.LogInformation("Created the upsertDealersin");
                HttpResponseMessage result = await casesIn.IntegrationProcessAsync(payLoad);
                log.LogInformation("triggered the method.");
                if (result.IsSuccessStatusCode)
                {
#pragma warning disable S6422 // Calls to "async" methods should not be blocking in Azure Functions
                    log.LogInformation($"Message body : {result.StatusCode}, Message Content:{result.Content.ReadAsStringAsync().GetAwaiter().GetResult()}");
#pragma warning restore S6422 // Calls to "async" methods should not be blocking in Azure Functions
                    return new OkObjectResult("Request body has been processed successfully");
                }
            }
            else if (originalJson["SiebelMessage"] != null && originalJson["SiebelMessage"]["ListOfServiceRequestBreakdownArgus"] != null &&
                (transType.ToUpper() == "Mercurius.Event.ETAUpdate".ToUpper() || transType.ToUpper() == "Mercurius.Event.ETCUpdate".ToUpper() ||
                transType.ToUpper() == "Mercurius.Event.ATCUpdate".ToUpper() || transType.ToUpper() == "Mercurius.Event.ATAUpdate".ToUpper() ||
                transType.ToUpper() == "Mercurius.Event.GPSETAUpdate".ToUpper()))
            {
                if (originalJson.SelectToken("SiebelMessage.@EventTimestamp") == null || string.IsNullOrEmpty(originalJson.SelectToken("SiebelMessage.@EventTimestamp").ToString()))
                {
                    log.LogError("EventTimestamp is a mandatory field.");
                    throw new ArgumentException("EventTimestamp is a mandatory field.");
                }

                log.LogInformation($"isUpdate: {transType.ToUpper()}");
                this.UpdateEtaEtcAtcAta(log, logger, originalJson, transType);
                return new OkObjectResult("Request body has been processed successfully");
            }
            else if (originalJson["SiebelMessage"] != null && originalJson["SiebelMessage"]["ListOfServiceRequestBreakdownArgus"] != null && transType.ToUpper() == "Mercurius.Event.UpdateCase".ToUpper())
            {
                if (originalJson.SelectToken("SiebelMessage.@EventTimestamp") == null || string.IsNullOrEmpty(originalJson.SelectToken("SiebelMessage.@EventTimestamp").ToString()))
                {
                    log.LogError("EventTimestamp is a mandatory field.");
                    throw new ArgumentException("EventTimestamp is a mandatory field.");
                }

                log.LogInformation($"isUpdate: {originalJson["SiebelMessage"]["@TransType"].ToString().ToUpper()}");
                this.UpdateCase(log, logger, originalJson);
            }
            else if (originalJson["SiebelMessage"] != null && originalJson["SiebelMessage"]["ListOfServiceRequestBreakdownArgus"] != null &&
               transType.ToUpper() == "AddRemark".ToUpper())
            {
                log.LogInformation($"isAddRemark: {transType.ToUpper()}");
                JObject payLoad = (JObject)originalJson["SiebelMessage"]["ListOfServiceRequestBreakdownArgus"]["ServiceRequestBdArgus"];
                string caseNumberArgus = payLoad["CaseNumberARGUS"].ToString();
                string caseNumberJarvis = payLoad["CaseNumberJARVIS"]?.ToString();
                var remarkList = payLoad["ListOfServiceRequestRemark"]["ServiceRequestRemark"];
                log.LogInformation("payload is ready");
                this.dynamicsClient.SetLoggingReference(logger);
                log.LogInformation("setting logger info in dynamicsclient");
                if (remarkList.GetType() == typeof(JObject))
                {
                    AddRemarkInbound addRemarks = new AddRemarkInbound(this.dynamicsClient, logger);
                    log.LogInformation("Add remark inbound");
                    var result = addRemarks.IntegrationProcessAsync(JObject.Parse(remarkList.ToString()), caseNumberArgus, caseNumberJarvis);
                    log.LogInformation("triggered the method.");
#pragma warning disable S6422 // Calls to "async" methods should not be blocking in Azure Functions
                    if (result.Result.IsSuccessStatusCode)
                    {
                        log.LogInformation($"Message body : {result.Result.StatusCode}, Message Content:{result.Result.Content.ReadAsStringAsync().GetAwaiter().GetResult()}");
#pragma warning restore S6422 // Calls to "async" methods should not be blocking in Azure Functions
                    }
                    else
                    {
                        throw new ArgumentException($"Add Remark: Failed in adding remark with case number " + caseNumberArgus);
                    }
                }
                else
                {
                    bool isSuccess = true;
                    foreach (var item in remarkList)
                    {
                        AddRemarkInbound addRemarks = new AddRemarkInbound(this.dynamicsClient, logger);
                        log.LogInformation("Add remark inbound");
                        var result = addRemarks.IntegrationProcessAsync(JObject.Parse(item.ToString()), caseNumberArgus, caseNumberJarvis);
                        log.LogInformation("triggered the method.");
#pragma warning disable S6422 // Calls to "async" methods should not be blocking in Azure Functions
                        if (result.Result.IsSuccessStatusCode)
                        {
                            log.LogInformation($"Message body : {result.Result.StatusCode}, Message Content:{result.Result.Content.ReadAsStringAsync().GetAwaiter().GetResult()}");
#pragma warning restore S6422 // Calls to "async" methods should not be blocking in Azure Functions
                        }
                        else
                        {
                            isSuccess = false;
                        }
                    }

                    if (!isSuccess)
                    {
                        throw new ArgumentException($"Add remark: Failed in adding remark with case number " + caseNumberArgus);
                    }
                }
            }
            else if (originalJson["SiebelMessage"] != null && originalJson["SiebelMessage"]["ListOfServiceRequestBreakdownArgus"] != null &&
               (transType.ToUpper() == "Mercurius.Event.AddReportFault1".ToUpper() || transType.ToUpper() == "Mercurius.Event.AddReportFault2".ToUpper()))
            {
                if (originalJson.SelectToken("SiebelMessage.@EventTimestamp") == null || string.IsNullOrEmpty(originalJson.SelectToken("SiebelMessage.@EventTimestamp").ToString()))
                {
                    log.LogError("EventTimestamp is a mandatory field.");
                    throw new ArgumentException("EventTimestamp is a mandatory field.");
                }

                log.LogInformation($"isAddReportFault: {transType.ToUpper()}");
                this.AddReportFault(log, logger, originalJson, transType);
            }
            else if (originalJson["SiebelMessage"] != null && originalJson["SiebelMessage"]["ListOfServiceRequestBreakdownArgus"] != null && transType.ToUpper() == "Mercurius.Event.AddGOP".ToUpper())
            {
                if (originalJson.SelectToken("SiebelMessage.@EventTimestamp") == null || string.IsNullOrEmpty(originalJson.SelectToken("SiebelMessage.@EventTimestamp").ToString()))
                {
                    log.LogError("EventTimestamp is a mandatory field.");
                    throw new ArgumentException("EventTimestamp is a mandatory field.");
                }

                log.LogInformation($"isUpdate: {originalJson["SiebelMessage"]["@TransType"].ToString().ToUpper()}");
                this.AddGOP(log, logger, originalJson);
            }
            else if (originalJson["SiebelMessage"] != null && originalJson["SiebelMessage"]["ListOfServiceRequestBreakdownArgus"] != null && transType.ToUpper() == "Mercurius.Event.PassOut".ToUpper())
            {
                if (originalJson.SelectToken("SiebelMessage.@EventTimestamp") == null || string.IsNullOrEmpty(originalJson.SelectToken("SiebelMessage.@EventTimestamp").ToString()))
                {
                    log.LogError("EventTimestamp is a mandatory field.");
                    throw new ArgumentException("EventTimestamp is a mandatory field.");
                }

                log.LogInformation($"isAddPassout: {originalJson["SiebelMessage"]["@TransType"].ToString().ToUpper()}");
                this.AddPassout(log, logger, originalJson);
            }
            else if (originalJson["SiebelMessage"] != null && originalJson["SiebelMessage"]["ListOfServiceRequestBreakdownArgus"] != null &&
                (transType.ToUpper() == "Mercurius.Event.RepairInfo1".ToUpper() || transType.ToUpper() == "Mercurius.Event.RepairInfo2".ToUpper()))
            {
                log.LogInformation($"isAddRepairInfo: {transType.ToUpper()}");
                this.AddRepairInfo(log, logger, originalJson, transType);
            }
            //// Adding DelayedETAUpdateLogic.
            else if (originalJson["SiebelMessage"] != null && originalJson["SiebelMessage"]["ListOfServiceRequestBreakdownArgus"] != null && (transType.ToUpper() == "Mercurius.Event.DelayedETAUpdate1".ToUpper() || transType.ToUpper() == "Mercurius.Event.DelayedETAUpdate2".ToUpper()))
            {
                if (originalJson.SelectToken("SiebelMessage.@EventTimestamp") == null || string.IsNullOrEmpty(originalJson.SelectToken("SiebelMessage.@EventTimestamp").ToString()))
                {
                    log.LogError("EventTimestamp is a mandatory field.");
                    throw new ArgumentException("EventTimestamp is a mandatory field.");
                }

                log.LogInformation($"isDelayedETAUpdate: {transType.ToUpper()}");
                this.DelayedETA(log, logger, originalJson, transType);
            }
            else if (originalJson["SiebelMessage"] != null && originalJson["SiebelMessage"]["ListOfServiceRequestBreakdownArgus"] != null && transType.ToUpper() == "Mercurius.Event.AddGOP+".ToUpper())
            {
                if (originalJson.SelectToken("SiebelMessage.@EventTimestamp") == null || string.IsNullOrEmpty(originalJson.SelectToken("SiebelMessage.@EventTimestamp").ToString()))
                {
                    log.LogError("EventTimestamp is a mandatory field.");
                    throw new ArgumentException("EventTimestamp is a mandatory field.");
                }

                log.LogInformation($"isAddGOP+: {originalJson["SiebelMessage"]["@TransType"].ToString().ToUpper()}");
                this.AddGOPPlus(log, logger, originalJson);
            }
            else if (originalJson["SiebelMessage"] != null && originalJson["SiebelMessage"]["ListOfServiceRequestBreakdownArgus"] != null &&
                (transType.ToUpper() == "Mercurius.Event.JobEnd1".ToUpper() || transType.ToUpper() == "Mercurius.Event.JobEnd2".ToUpper()))
            {
                if (originalJson.SelectToken("SiebelMessage.@EventTimestamp") == null || string.IsNullOrEmpty(originalJson.SelectToken("SiebelMessage.@EventTimestamp").ToString()))
                {
                    log.LogError("EventTimestamp is a mandatory field.");
                    throw new ArgumentException("EventTimestamp is a mandatory field.");
                }

                log.LogInformation($"isAddJodEndDetails: {transType.ToUpper()}");
                this.AddJobEndDetails(log, logger, originalJson, transType);
            }
            else if (originalJson["SiebelMessage"] != null && originalJson["SiebelMessage"]["ListOfServiceRequestBreakdownArgus"] != null && transType.ToUpper() == "Mercurius.Event.SaveExitMonitorHis".ToUpper())
            {
                log.LogInformation($"isAddSaveExitMonitor: {originalJson["SiebelMessage"]["@TransType"].ToString().ToUpper()}");
                this.AddCaseMonitorAction(log, logger, originalJson);
                return new OkObjectResult("successfully created");
            }
            else if (originalJson["SiebelMessage"] != null && originalJson["SiebelMessage"]["ListOfServiceRequestBreakdownArgus"] != null && transType.ToUpper() == "Mercurius.Event.AddAttachment".ToUpper())
            {
                log.LogInformation($"AddAttachment: {originalJson["SiebelMessage"]["@TransType"].ToString().ToUpper()}");
                this.AddAttachment(log, logger, originalJson, transType);
            }
            else if (originalJson["SiebelMessage"] != null && originalJson["SiebelMessage"]["ListOfAccountBreakdownArgus"] != null &&
                transType.ToUpper() == "Mercurius.Event.CustWhitelist".ToUpper())
            {
                log.LogInformation($"isCustomerWhitelist: {transType.ToUpper()}");
                this.AddCustomerWhitelist(log, logger, originalJson);
            }
            else if (originalJson["SiebelMessage"] != null && originalJson["SiebelMessage"]["ListOfVehiclesUnitArgus"] != null && originalJson["SiebelMessage"]["ListOfVehiclesUnitArgus"]["VehicleUnitsBdArgus"] != null &&
                transType.ToUpper() == "Mercurius.Event.VINWhitelist".ToUpper())
            {
                log.LogInformation($"isVINWhitelist: {transType.ToUpper()}");
                this.AddVINWhitelist(log, logger, originalJson);
            }

            throw new ArgumentException("CTDI Integration 3: No request Body from CTDI");
        }

        private void UpdateCase(ILogger log, ILoggerService logger, JObject originalJson)
        {
            DateTime eventTimestamp = DateTime.ParseExact(originalJson.SelectToken("SiebelMessage.@EventTimestamp").ToString(), "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
            JObject payLoad = (JObject)originalJson["SiebelMessage"]["ListOfServiceRequestBreakdownArgus"]["ServiceRequestBdArgus"];
            log.LogInformation("payload is ready");
            this.dynamicsClient.SetLoggingReference(logger);
            log.LogInformation("setting logger info in dynamicsclient");
            if (payLoad != null && !string.IsNullOrEmpty(eventTimestamp.ToString()) && payLoad.ContainsKey("ForcedClosedARGUS") && payLoad["ForcedClosedARGUS"].ToString().ToUpper() == "Y")
            {
                ForceCloseCaseInbound casesIn = new ForceCloseCaseInbound(this.dynamicsClient, logger);
                log.LogInformation("Created the upsertCasein");
                var result = casesIn.IntegrationProcessAsync(payLoad);
                log.LogInformation("triggered the method.");
#pragma warning disable S6422 // Calls to "async" methods should not be blocking in Azure Functions
                if (result.Result.IsSuccessStatusCode)
                {
                    log.LogInformation($"Message body : {result.Result.StatusCode}, Message Content:{result.Result.Content.ReadAsStringAsync().GetAwaiter().GetResult()}");
#pragma warning restore S6422 // Calls to "async" methods should not be blocking in Azure Functions
                }
                else
                {
                    throw new ArgumentException($"Update case: Failed to Force Close Case.");
                }
            }
            else if (payLoad != null && !string.IsNullOrEmpty(eventTimestamp.ToString()))
            {
                UpdateCaseInbound casesIn = new UpdateCaseInbound(this.dynamicsClient, logger);
                log.LogInformation("Created the upsertCasein");
                var result = casesIn.IntegrationProcessAsync(payLoad, eventTimestamp);
                log.LogInformation("triggered the method.");
#pragma warning disable S6422 // Calls to "async" methods should not be blocking in Azure Functions
                if (result.Result.IsSuccessStatusCode)
                {
                    log.LogInformation($"Message body : {result.Result.StatusCode}, Message Content:{result.Result.Content.ReadAsStringAsync().GetAwaiter().GetResult()}");
#pragma warning restore S6422 // Calls to "async" methods should not be blocking in Azure Functions
                }
                else
                {
                    throw new ArgumentException($"Update case: Failed in updating case.");
                }
            }
            else
            {
                throw new ArgumentException("Invalid Payload or EventTimestamp.");
            }
        }

        private void UpdateEtaEtcAtcAta(ILogger log, ILoggerService logger, JObject originalJson, string transType)
        {
            DateTime eventTimestamp = DateTime.Parse(originalJson.SelectToken("SiebelMessage.@EventTimestamp").ToString());
            JObject payLoad = (JObject)originalJson["SiebelMessage"]["ListOfServiceRequestBreakdownArgus"]["ServiceRequestBdArgus"];
            string caseNumberArgus = payLoad["CaseNumberARGUS"].ToString();
            string caseNumberJarvis = payLoad["CaseNumberJARVIS"]?.ToString();
            var passoutList = payLoad["ListOfSRBdPassoutLoginfoArgus"]["SRBdPassoutLoginfoArgus"];
            log.LogInformation("payload is ready");
            this.dynamicsClient.SetLoggingReference(logger);
            log.LogInformation("setting logger info in dynamicsclient");
            if (passoutList.GetType() == typeof(JObject))
            {
                UpsertPassout casesIn = new UpsertPassout(this.dynamicsClient, this.config, logger);
                log.LogInformation("Created the upsertCase inbound");
                var result = casesIn.IntegrationProcessAsync(JObject.Parse(passoutList.ToString()), caseNumberArgus, transType, caseNumberJarvis, eventTimestamp);
                log.LogInformation("triggered the method.");
#pragma warning disable S6422 // Calls to "async" methods should not be blocking in Azure Functions
                if (result.Result.IsSuccessStatusCode)
                {
                    log.LogInformation($"Message body : {result.Result.StatusCode}, Message Content:{result.Result.Content.ReadAsStringAsync().GetAwaiter().GetResult()}");
#pragma warning restore S6422 // Calls to "async" methods should not be blocking in Azure Functions
                }
                else
                {
                    throw new ArgumentException($"Update case: Failed in updating case with case number " + caseNumberArgus + " " + caseNumberJarvis);
                }
            }
            else
            {
                bool isSuccess = true;
                foreach (var item in passoutList)
                {
                    UpsertPassout casesIn = new UpsertPassout(this.dynamicsClient, this.config, logger);
                    log.LogInformation("Created the upsertCase inbound");
                    var result = casesIn.IntegrationProcessAsync(JObject.Parse(item.ToString()), caseNumberArgus, transType, caseNumberJarvis, eventTimestamp);
                    log.LogInformation("triggered the method.");
#pragma warning disable S6422 // Calls to "async" methods should not be blocking in Azure Functions
                    if (result.Result.IsSuccessStatusCode)
                    {
                        log.LogInformation($"Message body : {result.Result.StatusCode}, Message Content:{result.Result.Content.ReadAsStringAsync().GetAwaiter().GetResult()}");
#pragma warning restore S6422 // Calls to "async" methods should not be blocking in Azure Functions
                    }
                    else
                    {
                        isSuccess = false;
                    }
                }

                if (!isSuccess)
                {
                    throw new ArgumentException($"Upsert case: Failed in Upsert case with case number " + caseNumberArgus + " " + caseNumberJarvis);
                }
            }
        }

        private void AddCaseMonitorAction(ILogger log, ILoggerService logger, JObject originalJson)
        {
            JObject payLoad = (JObject)originalJson["SiebelMessage"]["ListOfServiceRequestBreakdownArgus"]["ServiceRequestBdArgus"];
            log.LogInformation("payload is ready");
            string caseNumberArgus = payLoad["CaseNumberARGUS"].ToString();
            string caseNumberJarvis = payLoad["CaseNumberJARVIS"]?.ToString();
            var caseMonitorData = payLoad["ListOfActionBdMonitorHistoryArgus"]["ActionBdMonitorHistoryArgus"];
            this.dynamicsClient.SetLoggingReference(logger);
            log.LogInformation("setting logger info in dynamicsclient");
            AddCaseMonitorActionInbound casesIn = new AddCaseMonitorActionInbound(this.dynamicsClient, logger);
            log.LogInformation("Create the Add Case Monitor Action");
            var result = casesIn.IntegrationProcessAsync(JObject.Parse(caseMonitorData.ToString()), caseNumberArgus, caseNumberJarvis);
            log.LogInformation("triggered the method.");
#pragma warning disable S6422 // Calls to "async" methods should not be blocking in Azure Functions
            if (result.Result.IsSuccessStatusCode)
            {
                log.LogInformation($"Message body : {result.Result.StatusCode}, Message Content:{result.Result.Content.ReadAsStringAsync().GetAwaiter().GetResult()}");
#pragma warning restore S6422 // Calls to "async" methods should not be blocking in Azure Functions
            }
            else
            {
                throw new ArgumentException($"Add Case Monitor Action: Failed in adding case monitor action.");
            }
        }

        private void AddCustomerWhitelist(ILogger log, ILoggerService logger, JObject originalJson)
        {
            JObject payLoad = (JObject)originalJson["SiebelMessage"]["ListOfAccountBreakdownArgus"]["Account"];
            log.LogInformation("payload is ready");
            this.dynamicsClient.SetLoggingReference(logger);
            log.LogInformation("setting logger info in dynamicsclient");
            AddCustomerWhitelist addCustomerWhitelist = new AddCustomerWhitelist(this.dynamicsClient, logger);
            log.LogInformation("Add Customer Whitelist inbound");
            var result = addCustomerWhitelist.IntegrationProcessAsync(JObject.Parse(payLoad.ToString()));
            log.LogInformation("triggered the method.");
#pragma warning disable S6422 // Calls to "async" methods should not be blocking in Azure Functions
            if (result.Result.IsSuccessStatusCode)
            {
                log.LogInformation($"Message body : {result.Result.StatusCode}, Message Content:{result.Result.Content.ReadAsStringAsync().GetAwaiter().GetResult()}");
#pragma warning restore S6422 // Calls to "async" methods should not be blocking in Azure Functions
            }
            else
            {
                throw new ArgumentException($"Add Customer Whitelist: Failed in adding Customer Whitelist.");
            }
        }

        private void AddReportFault(ILogger log, ILoggerService logger, JObject originalJson, string transType)
        {
            DateTime eventTimestamp = DateTime.Parse(originalJson.SelectToken("SiebelMessage.@EventTimestamp").ToString());
            JObject payLoad = (JObject)originalJson["SiebelMessage"]["ListOfServiceRequestBreakdownArgus"]["ServiceRequestBdArgus"];
            string caseNumberArgus = payLoad["CaseNumberARGUS"].ToString();
            string caseNumberJarvis = payLoad["CaseNumberJARVIS"]?.ToString();
            var reportFaultList = payLoad["ListOfSRBdReportFaultArgus"]["SRBdReportFaultArgus"];
            log.LogInformation("payload is ready");
            this.dynamicsClient.SetLoggingReference(logger);
            log.LogInformation("setting logger info in dynamicsclient");
            if (reportFaultList.GetType() == typeof(JObject))
            {
                AddReportFaultInbound addReportFault = new AddReportFaultInbound(this.dynamicsClient, logger);
                log.LogInformation("Add ReportFault inbound");
                var result = addReportFault.IntegrationProcessAsync(JObject.Parse(reportFaultList.ToString()), caseNumberArgus, caseNumberJarvis, transType, eventTimestamp);
                log.LogInformation("triggered the method.");
#pragma warning disable S6422 // Calls to "async" methods should not be blocking in Azure Functions
                if (result.Result.IsSuccessStatusCode)
                {
                    log.LogInformation($"Message body : {result.Result.StatusCode}, Message Content:{result.Result.Content.ReadAsStringAsync().GetAwaiter().GetResult()}");
#pragma warning restore S6422 // Calls to "async" methods should not be blocking in Azure Functions
                }
                else
                {
                    throw new ArgumentException($"Add ReportFault: Failed in adding ReportFault with case number " + caseNumberArgus);
                }
            }
            else
            {
                bool isSuccess = true;
                foreach (var item in reportFaultList)
                {
                    AddReportFaultInbound addReportFault = new AddReportFaultInbound(this.dynamicsClient, logger);
                    log.LogInformation("Add ReportFault inbound");
                    var result = addReportFault.IntegrationProcessAsync(JObject.Parse(item.ToString()), caseNumberArgus, caseNumberJarvis, transType, eventTimestamp);
                    log.LogInformation("triggered the method.");
#pragma warning disable S6422 // Calls to "async" methods should not be blocking in Azure Functions
                    if (result.Result.IsSuccessStatusCode)
                    {
                        log.LogInformation($"Message body : {result.Result.StatusCode}, Message Content:{result.Result.Content.ReadAsStringAsync().GetAwaiter().GetResult()}");
#pragma warning restore S6422 // Calls to "async" methods should not be blocking in Azure Functions
                    }
                    else
                    {
                        isSuccess = false;
                    }
                }

                if (!isSuccess)
                {
                    throw new ArgumentException($"Add ReportFault: Failed in adding ReportFault with case number " + caseNumberArgus);
                }
            }
        }

        private void AddGOPPlus(ILogger log, ILoggerService logger, JObject originalJson)
        {
            DateTime eventTimestamp = DateTime.Parse(originalJson.SelectToken("SiebelMessage.@EventTimestamp").ToString());
            JObject payLoad = (JObject)originalJson["SiebelMessage"]["ListOfServiceRequestBreakdownArgus"]["ServiceRequestBdArgus"];
            log.LogInformation("payload is ready");
            string caseNumberArgus = payLoad["CaseNumberARGUS"].ToString();
            string caseNumberJarvis = payLoad["CaseNumberJARVIS"]?.ToString();
            var gopData = payLoad["ListOfSRBreakdownGopPlusArgus"]["SRBreakdownGopPlusArgus"];
            this.dynamicsClient.SetLoggingReference(logger);
            log.LogInformation("setting logger info in dynamicsclient");
            AddGopPlusInbound casesIn = new AddGopPlusInbound(this.dynamicsClient, logger);
            log.LogInformation("Created the upsert GOP+");
            var result = casesIn.IntegrationProcessAsync(JObject.Parse(gopData.ToString()), caseNumberArgus, caseNumberJarvis, eventTimestamp);
            log.LogInformation("triggered the method.");
#pragma warning disable S6422 // Calls to "async" methods should not be blocking in Azure Functions
            if (result.Result.IsSuccessStatusCode)
            {
                log.LogInformation($"Message body : {result.Result.StatusCode}, Message Content:{result.Result.Content.ReadAsStringAsync().GetAwaiter().GetResult()}");
#pragma warning restore S6422 // Calls to "async" methods should not be blocking in Azure Functions
            }
            else
            {
                throw new ArgumentException($"Update case: Failed in updating case.");
            }
        }

        private void AddJobEndDetails(ILogger log, ILoggerService logger, JObject originalJson, string transType)
        {
            DateTime eventTimestamp = DateTime.Parse(originalJson.SelectToken("SiebelMessage.@EventTimestamp").ToString());
            JObject payLoad = (JObject)originalJson["SiebelMessage"]["ListOfServiceRequestBreakdownArgus"]["ServiceRequestBdArgus"];
            string caseNumberArgus = string.Empty;
            string caseNumberJarvis = string.Empty;

            if (payLoad.HasValues)
            {
                caseNumberArgus = payLoad["CaseNumberARGUS"].ToString();
                caseNumberJarvis = payLoad["CaseNumberJARVIS"]?.ToString();
                log.LogInformation("payload is ready");
                this.dynamicsClient.SetLoggingReference(logger);
                log.LogInformation("setting logger info in dynamicsclient");
                AddJobEndDetailsInbound addJobEndDetails = new AddJobEndDetailsInbound(this.dynamicsClient, logger);
                log.LogInformation("Add Job End Details inbound");
                var result = addJobEndDetails.IntegrationProcessAsync(payLoad, caseNumberArgus, caseNumberJarvis, transType, eventTimestamp);
                log.LogInformation("triggered the method.");
#pragma warning disable S6422 // Calls to "async" methods should not be blocking in Azure Functions
                if (result.Result.IsSuccessStatusCode)
                {
                    log.LogInformation($"Message body : {result.Result.StatusCode}, Message Content:{result.Result.Content.ReadAsStringAsync().GetAwaiter().GetResult()}");
#pragma warning restore S6422 // Calls to "async" methods should not be blocking in Azure Functions
                }
                else
                {
                    throw new ArgumentException($"Add JobEndDetails: Failed in adding JobEndDetails with case number " + caseNumberArgus);
                }
            }
            else
            {
                throw new ArgumentException($"Add JobEndDetails: Failed in adding JobEndDetails because no payload for the case number " + caseNumberArgus);
            }
        }

        private void AddVINWhitelist(ILogger log, ILoggerService logger, JObject originalJson)
        {
            JObject payLoad = (JObject)originalJson["SiebelMessage"]["ListOfVehiclesUnitArgus"]["VehicleUnitsBdArgus"];
            log.LogInformation("payload is ready");
            string vinNumber = payLoad["SerialNumber"]?.ToString();
            var vehicleUnit = payLoad["ListOfVehicleUnitFinancialArgus"]["VehicleUnitFinancialArgus"];
            this.dynamicsClient.SetLoggingReference(logger);
            log.LogInformation("setting logger info in dynamicsclient");
            AddVINWhitelist addVinWhitelist = new AddVINWhitelist(this.dynamicsClient, logger);
            log.LogInformation("Add Customer Whitelist inbound");
            var result = addVinWhitelist.IntegrationProcessAsync(JObject.Parse(vehicleUnit.ToString()), vinNumber);
            log.LogInformation("triggered the method.");
#pragma warning disable S6422 // Calls to "async" methods should not be blocking in Azure Functions
            if (result.Result.IsSuccessStatusCode)
            {
                log.LogInformation($"Message body : {result.Result.StatusCode}, Message Content:{result.Result.Content.ReadAsStringAsync().GetAwaiter().GetResult()}");
#pragma warning restore S6422 // Calls to "async" methods should not be blocking in Azure Functions
            }
            else
            {
                throw new ArgumentException($"Add VIN Whitelist: Failed in adding VIN Whitelist.");
            }
        }

        private void AddRepairInfo(ILogger log, ILoggerService logger, JObject originalJson, string transType)
        {
            JObject payLoad = (JObject)originalJson["SiebelMessage"]["ListOfServiceRequestBreakdownArgus"]["ServiceRequestBdArgus"];
            string caseNumberArgus = payLoad["CaseNumberARGUS"].ToString();
            string caseNumberJarvis = payLoad["CaseNumberJARVIS"]?.ToString();
            var passOutPayload = payLoad["ListOfSRBdPassoutLoginfoArgus"]["SRBdPassoutLoginfoArgus"];
            JObject repairInfo;
            if (passOutPayload.HasValues)
            {
                repairInfo = JObject.Parse(passOutPayload["ListOfSRBreakdownRepairinfoArgus"]["SRBreakdownRepairinfoArgus"].ToString());
                if (repairInfo.HasValues)
                {
                    log.LogInformation("payload is ready");
                    this.dynamicsClient.SetLoggingReference(logger);
                    log.LogInformation("setting logger info in dynamicsclient");
                    AddRepairInfoInbound addRepairInfo = new AddRepairInfoInbound(this.dynamicsClient, logger);
                    log.LogInformation("Add ReportFault inbound");
                    var result = addRepairInfo.IntegrationProcessAsync(repairInfo, JObject.Parse(passOutPayload.ToString()), caseNumberArgus, caseNumberJarvis, transType);
                    log.LogInformation("triggered the method.");
#pragma warning disable S6422 // Calls to "async" methods should not be blocking in Azure Functions
                    if (result.Result.IsSuccessStatusCode)
                    {
                        log.LogInformation($"Message body : {result.Result.StatusCode}, Message Content:{result.Result.Content.ReadAsStringAsync().GetAwaiter().GetResult()}");
#pragma warning restore S6422 // Calls to "async" methods should not be blocking in Azure Functions
                    }
                    else
                    {
                        throw new ArgumentException($"Add RepairInfo: Failed in adding RepairInfo with case number " + caseNumberArgus);
                    }
                }
                else
                {
                    throw new ArgumentException($"Add RepairInfo: No Repair info data in the payload for the case number " + caseNumberArgus);
                }
            }
            else
            {
                throw new ArgumentException($"Add AddRepairInfo: Failed in adding RepairInfo because no PassOut payload for the case number " + caseNumberArgus);
            }
        }

        private void AddGOP(ILogger log, ILoggerService logger, JObject originalJson)
        {
            DateTime eventTimestamp = DateTime.Parse(originalJson.SelectToken("SiebelMessage.@EventTimestamp").ToString());
            JObject payLoad = (JObject)originalJson["SiebelMessage"]["ListOfServiceRequestBreakdownArgus"]["ServiceRequestBdArgus"];
            log.LogInformation("payload is ready");
            string caseNumberArgus = payLoad["CaseNumberARGUS"].ToString();
            string caseNumberJarvis = payLoad["CaseNumberJARVIS"]?.ToString();
            var gopData = payLoad["ListOfSRBdGopInLoginfoArgus"]["SRBdGopInLoginfoArgus"];
            this.dynamicsClient.SetLoggingReference(logger);
            log.LogInformation("setting logger info in dynamicsclient");
            AddGopInbound casesIn = new AddGopInbound(this.dynamicsClient, logger);
            log.LogInformation("Created the add GOP");
            var result = casesIn.IntegrationProcessAsync(JObject.Parse(gopData.ToString()), caseNumberArgus, caseNumberJarvis, eventTimestamp);
            log.LogInformation("triggered the method.");
#pragma warning disable S6422 // Calls to "async" methods should not be blocking in Azure Functions
            if (result.Result.IsSuccessStatusCode)
            {
                log.LogInformation($"Message body : {result.Result.StatusCode}, Message Content:{result.Result.Content.ReadAsStringAsync().GetAwaiter().GetResult()}");
#pragma warning restore S6422 // Calls to "async" methods should not be blocking in Azure Functions
            }
            else
            {
                throw new ArgumentException($"Update case: Failed in updating case.");
            }
        }

        private void AddPassout(ILogger log, ILoggerService logger, JObject originalJson)
        {
            DateTime eventTimestamp = DateTime.Parse(originalJson.SelectToken("SiebelMessage.@EventTimestamp").ToString());
            JObject payLoad = (JObject)originalJson["SiebelMessage"]["ListOfServiceRequestBreakdownArgus"]["ServiceRequestBdArgus"];
            log.LogInformation("payload is ready");
            string caseNumberArgus = payLoad["CaseNumberARGUS"].ToString();
            string caseNumberJarvis = payLoad["CaseNumberJARVIS"]?.ToString();
            var passoutData = payLoad["ListOfSRBdPassoutLoginfoArgus"]["SRBdPassoutLoginfoArgus"];
            this.dynamicsClient.SetLoggingReference(logger);
            log.LogInformation("setting logger info in dynamicsclient");
            AddPassoutInbound casesIn = new AddPassoutInbound(this.dynamicsClient, logger);
            log.LogInformation("Created the upsertDealersin");
            var result = casesIn.IntegrationProcessAsync(JObject.Parse(passoutData.ToString()), caseNumberArgus, caseNumberJarvis, eventTimestamp);
            log.LogInformation("triggered the method.");
#pragma warning disable S6422 // Calls to "async" methods should not be blocking in Azure Functions
            if (result.Result.IsSuccessStatusCode)
            {
                log.LogInformation($"Message body : {result.Result.StatusCode}, Message Content:{result.Result.Content.ReadAsStringAsync().GetAwaiter().GetResult()}");
#pragma warning restore S6422 // Calls to "async" methods should not be blocking in Azure Functions
            }
            else
            {
                throw new ArgumentException($"Update case: Failed in updating case.");
            }
        }

        private void DelayedETA(ILogger log, ILoggerService logger, JObject originalJson, string transType)
        {
            DateTime eventTimestamp = DateTime.Parse(originalJson.SelectToken("SiebelMessage.@EventTimestamp").ToString());
            JObject payLoad = (JObject)originalJson["SiebelMessage"]["ListOfServiceRequestBreakdownArgus"]["ServiceRequestBdArgus"];
            string caseNumberArgus = payLoad["CaseNumberARGUS"].ToString();
            string caseNumberJarvis = payLoad["CaseNumberJARVIS"]?.ToString();
            var passOutPayload = payLoad["ListOfSRBdPassoutLoginfoArgus"]["SRBdPassoutLoginfoArgus"];
            JObject delayedEtaPayload;
            if (passOutPayload.HasValues)
            {
                delayedEtaPayload = JObject.Parse(passOutPayload["ListOfSrBreakdownEtaLogArgus"]["SrBreakdownEtaLogArgus"].ToString());

                log.LogInformation("payload is ready");
                this.dynamicsClient.SetLoggingReference(logger);
                log.LogInformation("setting logger info in dynamicsclient");
                if (delayedEtaPayload.HasValues)
                {
                    DelayedEtaInbound delayedEta = new DelayedEtaInbound(this.dynamicsClient, logger);
                    log.LogInformation("Add DealyedETA inbound");
                    var result = delayedEta.IntegrationProcessAsync(JObject.Parse(passOutPayload.ToString()), delayedEtaPayload, caseNumberArgus, caseNumberJarvis, transType, eventTimestamp);
                    log.LogInformation("triggered the method.");
#pragma warning disable S6422 // Calls to "async" methods should not be blocking in Azure Functions
                    if (result.Result.IsSuccessStatusCode)
                    {
                        log.LogInformation($"Message body : {result.Result.StatusCode}, Message Content:{result.Result.Content.ReadAsStringAsync().GetAwaiter().GetResult()}");
#pragma warning restore S6422 // Calls to "async" methods should not be blocking in Azure Functions
                    }
                    else
                    {
                        throw new ArgumentException($"Add DealyedETA: Failed in adding DelayedETA with case number " + caseNumberArgus);
                    }
                }
            }
            else
            {
                throw new ArgumentException($"Add DealyedETA: Failed in adding DelayedETA with case number " + caseNumberArgus);
            }
        }

        private void AddAttachment(ILogger log, ILoggerService logger, JObject originalJson, string transType)
        {
            JObject payLoad = (JObject)originalJson["SiebelMessage"]["ListOfServiceRequestBreakdownArgus"]["ServiceRequestBdArgus"];
            string caseNumberArgus = payLoad["CaseNumberARGUS"].ToString();
            string caseNumberJarvis = payLoad["CaseNumberJARVIS"]?.ToString();
            var attachmentPayload = payLoad["ListOfServiceRequestBdAttachmentArgus"]["ServiceRequestBdAttachmentArgus"];
            if (attachmentPayload.HasValues)
            {
                log.LogInformation("payload is ready");
                this.dynamicsClient.SetLoggingReference(logger);
                log.LogInformation("setting logger info in dynamicsclient");

                AddAttachmentInbound addAttachment = new AddAttachmentInbound(this.dynamicsClient, logger);
                log.LogInformation("Add Attachment inbound");
                var result = addAttachment.IntegrationProcessAsync(JObject.Parse(attachmentPayload.ToString()), caseNumberArgus, caseNumberJarvis);
                log.LogInformation("triggered the method.");
#pragma warning disable S6422 // Calls to "async" methods should not be blocking in Azure Functions
                if (result.Result.IsSuccessStatusCode)
                {
                    log.LogInformation($"Message body : {result.Result.StatusCode}, Message Content:{result.Result.Content.ReadAsStringAsync().GetAwaiter().GetResult()}");
#pragma warning restore S6422 // Calls to "async" methods should not be blocking in Azure Functions
                }
                else
                {
                    throw new ArgumentException($"Add DealyedETA: Failed in adding DelayedETA with case number " + caseNumberArgus);
                }
            }
            else
            {
                throw new ArgumentException($"Add DealyedETA: Failed in adding DelayedETA with case number " + caseNumberArgus);
            }
        }
    }
}
