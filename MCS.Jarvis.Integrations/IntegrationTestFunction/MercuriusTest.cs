namespace IntegrationTestFunction
{
    using System;
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

    /// <summary>
    /// Mercurius Test Class.
    /// </summary>
    public class MercuriusTest
    {
        private readonly IConfiguration config;
        private readonly IDynamicsApiClient dynamicsClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="MercuriusTest"/> class.
        /// </summary>
        /// <param name="config">Configuration.</param>
        /// <param name="dynamicsClient">Dynamics Client.</param>
        public MercuriusTest(IConfiguration config, IDynamicsApiClient dynamicsClient)
        {
            this.config = config;
            this.dynamicsClient = dynamicsClient;
        }

        /// <summary>
        /// Test funtion to test Service bus trigger function.
        /// </summary>
        /// <param name="req">Request.</param>
        /// <param name="log">Logger.</param>
        /// <returns>Task.</returns>
        /// <exception cref="ArgumentException">Argument Exception.</exception>
        [FunctionName("MercuriusTest")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            // string myQueueItem = "<SyncHardIndividualProduct xmlns=\"http://www.volvo.com/vda/hardindividualproducttruckbus/1_0\" xmlns:node=\"http://esw.volvo.com/node/2_1\" xmlns:pv=\"http://esw.volvo.com/parametervalue/2_1\" xmlns:prod=\"http://esw.volvo.com/products/2_1\" xmlns:val=\"http://esw.volvo.com/value/1_0\" xmlns:volvo=\"http://www.volvo.com/group/common/1_0\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" serviceID=\"SRV04266\" serviceVersion=\"1.0\" xsi:schemaLocation=\"http://www.volvo.com/vda/hardindividualproducttruckbus/1_0 HardIndividualProductTruckBus_1_0_0_final.xsd\"><volvo:ApplicationArea><volvo:Sender><volvo:LogicalID>VDA</volvo:LogicalID><volvo:TaskID>UPD</volvo:TaskID></volvo:Sender><volvo:CreationDateTime>2023-01-28T15:45:09.530925+01:00</volvo:CreationDateTime><volvo:BODID>VDA-20230128154509530946</volvo:BODID></volvo:ApplicationArea><DataArea><Sync recordSetCount=\"0000001\" recordSetTotal=\"0000001\"><volvo:ActionCriteria>GeneralInformation</volvo:ActionCriteria></Sync><HardIndividualProduct><HardIndividualProductHeader><ResponseStatus><volvo:StatusTypeCode>Informational</volvo:StatusTypeCode><volvo:StatusCode>015</volvo:StatusCode><volvo:StatusText>Vehicle Milage NOT found</volvo:StatusText></ResponseStatus></HardIndividualProductHeader><Identification><ChassisID><ChassisSeries>VDAT</ChassisSeries><ChassisNumber>100001</ChassisNumber></ChassisID><AliasID><VIN>YV2RT60C4HB832880</VIN></AliasID></Identification><CountryOfOperation>NLD</CountryOfOperation><RegistrationNumber>39-BNL-1</RegistrationNumber><DeliveryDate>2020-01-02</DeliveryDate><RetailDate>2020-01-20</RetailDate><VehicleOperationID>41</VehicleOperationID><Brand><BrandID>100</BrandID></Brand><GeneralDescription><ProductClass>24</ProductClass><ProductType>FH 62 TR</ProductType><CompanyCode>T</CompanyCode><AftermarketModel>FH (4)</AftermarketModel><MarketingType>FH13A62R</MarketingType><MainSpecificationWeek>17264</MainSpecificationWeek><BodySpecificationWeek>00000</BodySpecificationWeek></GeneralDescription><SpecialStatus><SpecialInformation>V</SpecialInformation></SpecialStatus><Coverage><CoverageCode>T01  </CoverageCode></Coverage><Coverage><CoverageCode>T05  </CoverageCode></Coverage><Factory><FactoryInvoiceNumber>3001727</FactoryInvoiceNumber><FactoryInvoiceDate>2017-07-06</FactoryInvoiceDate><FactoryOrderYear>70</FactoryOrderYear><FactoryOrderNumber>064262</FactoryOrderNumber><OrderMarket>0001610</OrderMarket><MainAssemblyDate>2017-06-29</MainAssemblyDate><BuildWeek>17264</BuildWeek><MainFactory>002</MainFactory><FinalFactory>002</FinalFactory></Factory><CommercialOrganization><SellingDealerID>010900</SellingDealerID><RepairingDealerID>4567</RepairingDealerID><DeliveryImporter>4567</DeliveryImporter><UserArea><volvo:IntegerValue name=\"RepairingDealerRCID\">456</volvo:IntegerValue></UserArea></CommercialOrganization><EndCustomer><OwningEndCustomerID>C-AVIS-123456</OwningEndCustomerID><UsingEndCustomerID>C-AVIS-123456</UsingEndCustomerID></EndCustomer></HardIndividualProduct></DataArea></SyncHardIndividualProduct>";
             string myQueueItem = "<SyncHardIndividualProduct\r\n    xmlns=\"http://www.volvo.com/vda/hardindividualproducttruckbus/1_0\"\r\n    xmlns:node=\"http://esw.volvo.com/node/2_1\"\r\n    xmlns:pv=\"http://esw.volvo.com/parametervalue/2_1\"\r\n    xmlns:prod=\"http://esw.volvo.com/products/2_1\"\r\n    xmlns:val=\"http://esw.volvo.com/value/1_0\"\r\n    xmlns:volvo=\"http://www.volvo.com/group/common/1_0\"\r\n    xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" serviceID=\"SRV04266\" serviceVersion=\"1.0\" xsi:schemaLocation=\"http://www.volvo.com/vda/hardindividualproducttruckbus/1_0 HardIndividualProductTruckBus_1_0_0_final.xsd\">\r\n    <volvo:ApplicationArea>\r\n        <volvo:Sender>\r\n            <volvo:LogicalID>VDA</volvo:LogicalID>\r\n            <volvo:TaskID>UPD</volvo:TaskID>\r\n        </volvo:Sender>\r\n        <volvo:CreationDateTime>2023-01-28T15:45:09.530925+01:00</volvo:CreationDateTime>\r\n        <volvo:BODID>VDA-20230128154509530946</volvo:BODID>\r\n    </volvo:ApplicationArea>\r\n    <DataArea>\r\n        <Sync recordSetCount=\"0000001\" recordSetTotal=\"0000001\">\r\n            <volvo:ActionCriteria>GeneralInformation</volvo:ActionCriteria>\r\n        </Sync>\r\n        <HardIndividualProduct>\r\n            <HardIndividualProductHeader>\r\n                <ResponseStatus>\r\n                    <volvo:StatusTypeCode>Informational</volvo:StatusTypeCode>\r\n                    <volvo:StatusCode>015</volvo:StatusCode>\r\n                    <volvo:StatusText>Vehicle Milage NOT found</volvo:StatusText>\r\n                </ResponseStatus>\r\n            </HardIndividualProductHeader>\r\n            <Identification>\r\n                <ChassisID>\r\n                    <ChassisSeries>VDAT</ChassisSeries>\r\n                    <ChassisNumber>190021</ChassisNumber>\r\n                </ChassisID>\r\n                <AliasID>\r\n                    <VIN>YV2RT6PC4HB839990</VIN>\r\n                </AliasID>\r\n            </Identification>\r\n            <CountryOfOperation></CountryOfOperation>\r\n            <RegistrationNumber>39-BNL-1</RegistrationNumber>\r\n            <DeliveryDate>2020-01-02</DeliveryDate>\r\n            <RetailDate>2020-01-20</RetailDate>\r\n            <VehicleOperationID>41</VehicleOperationID>\r\n            <Brand>\r\n                <BrandID>100</BrandID>\r\n            </Brand>\r\n            <GeneralDescription>\r\n                <ProductClass>24</ProductClass>\r\n                <ProductType>FH 62 TR</ProductType>\r\n                <CompanyCode>T</CompanyCode>\r\n                <AftermarketModel>FH (4)</AftermarketModel>\r\n                <MarketingType>FH13A62R</MarketingType>\r\n                <MainSpecificationWeek>17264</MainSpecificationWeek>\r\n                <BodySpecificationWeek>00000</BodySpecificationWeek>\r\n            </GeneralDescription>\r\n            <SpecialStatus>\r\n                <SpecialInformation>V</SpecialInformation>\r\n            </SpecialStatus>\r\n            <Coverage>\r\n                <CoverageCode>T01  </CoverageCode>\r\n            </Coverage>\r\n            <Coverage>\r\n                <CoverageCode>T05  </CoverageCode>\r\n            </Coverage>\r\n            <Factory>\r\n                <FactoryInvoiceNumber>3001727</FactoryInvoiceNumber>\r\n                <FactoryInvoiceDate>2017-07-06</FactoryInvoiceDate>\r\n                <FactoryOrderYear>70</FactoryOrderYear>\r\n                <FactoryOrderNumber>064262</FactoryOrderNumber>\r\n                <OrderMarket>0001610</OrderMarket>\r\n                <MainAssemblyDate>2017-06-29</MainAssemblyDate>\r\n                <BuildWeek>17264</BuildWeek>\r\n                <MainFactory>002</MainFactory>\r\n                <FinalFactory>002</FinalFactory>\r\n            </Factory>\r\n            <CommercialOrganization>\r\n                <SellingDealerID>010900</SellingDealerID>\r\n                <RepairingDealerID>4567</RepairingDealerID>\r\n                <DeliveryImporter></DeliveryImporter>\r\n                <UserArea>\r\n                    <volvo:IntegerValue name=\"RepairingDealerRCID\">456</volvo:IntegerValue>\r\n                </UserArea>\r\n            </CommercialOrganization>\r\n            <EndCustomer>\r\n                <OwningEndCustomerID>C-AVIS-123456</OwningEndCustomerID>\r\n            </EndCustomer>\r\n        </HardIndividualProduct>\r\n    </DataArea>\r\n</SyncHardIndividualProduct>";

            // string myQueueItem = "<SyncHardIndividualProduct\r\n    xmlns=\"http://www.volvo.com/vda/hardindividualproducttruckbus/1_0\"\r\n    xmlns:node=\"http://esw.volvo.com/node/2_1\"\r\n    xmlns:pv=\"http://esw.volvo.com/parametervalue/2_1\"\r\n    xmlns:prod=\"http://esw.volvo.com/products/2_1\"\r\n    xmlns:val=\"http://esw.volvo.com/value/1_0\"\r\n    xmlns:volvo=\"http://www.volvo.com/group/common/1_0\"\r\n    xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" serviceID=\"SRV04296\" serviceVersion=\"1.0\" xsi:schemaLocation=\"http://www.volvo.com/vda/hardindividualproducttruckbus/1_0 HardIndividualProductTruckBus_1_0_0_final.xsd\">\r\n    <volvo:ApplicationArea>\r\n        <volvo:Sender>\r\n            <volvo:LogicalID>VDA</volvo:LogicalID>\r\n            <volvo:TaskID>UPD</volvo:TaskID>\r\n        </volvo:Sender>\r\n        <volvo:CreationDateTime>2023-03-15T11:40:33.862398+01:00</volvo:CreationDateTime>\r\n        <volvo:BODID>VDA-20230315114033862423</volvo:BODID>\r\n    </volvo:ApplicationArea>\r\n    <DataArea>\r\n        <Sync recordSetCount=\"0000001\" recordSetTotal=\"0000001\">\r\n            <volvo:ActionCriteria>Variants</volvo:ActionCriteria>\r\n        </Sync>\r\n        <HardIndividualProduct>\r\n            <HardIndividualProductHeader/>\r\n            <Identification>\r\n                <ChassisID>\r\n                    <ChassisSeries>VDAT</ChassisSeries>\r\n                    <ChassisNumber>900801</ChassisNumber>\r\n                </ChassisID>\r\n                <AliasID>\r\n                    <VIN>YV2RTHH4HB832880</VIN>\r\n                </AliasID>\r\n            </Identification>\r\n            <Specification>\r\n                <TechnicalSpecification>\r\n                    <Variant>\r\n                        <VariantFamilyID>6WA</VariantFamilyID>\r\n                        <VariantID>C1X</VariantID>\r\n                    </Variant>\r\n                    <Variant>\r\n                        <VariantFamilyID>APX</VariantFamilyID>\r\n                        <VariantID>Z1X</VariantID>\r\n                    </Variant>\r\n                </TechnicalSpecification>\r\n            </Specification>\r\n        </HardIndividualProduct>\r\n    </DataArea>\r\n</SyncHardIndividualProduct>";
             log.LogInformation($"C# ServiceBus queue trigger function processed message: {myQueueItem}");
#pragma warning disable S2583 // Conditionally executed code should be reachable
             if (!string.IsNullOrEmpty(myQueueItem))
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(myQueueItem);
                ILoggerService logger = new LoggerService(log);
                string jsonData = JsonConvert.SerializeXmlNode(doc.GetElementsByTagName("SyncHardIndividualProduct").Item(0));
                log.LogInformation($"C# ServiceBus queue trigger function processed message: {jsonData}");
                JObject originalJson = JObject.Parse(jsonData);
                log.LogInformation("parse into jobject..");
                string actionCriteria = originalJson["SyncHardIndividualProduct"]["DataArea"]["Sync"]["volvo:ActionCriteria"].ToString();
                if (originalJson["SyncHardIndividualProduct"] != null && originalJson["SyncHardIndividualProduct"]["DataArea"] != null && actionCriteria.ToUpper() == "GeneralInformation".ToUpper())
                {
                    log.LogInformation($"isUpsert: {originalJson["SyncHardIndividualProduct"]["DataArea"]}");
                    this.UpsertVehicle(log, logger, originalJson);
                    return new OkObjectResult("Request body has been processed successfully");
                }
                else if (originalJson["SyncHardIndividualProduct"] != null && originalJson["SyncHardIndividualProduct"]["DataArea"] != null && actionCriteria.ToUpper() == "Variants".ToUpper())
                {
                    log.LogInformation($"isUpsert for Fuel/Power Type from VDA: {originalJson["SyncHardIndividualProduct"]["DataArea"]}");
                    this.UpdateVehicleVariant(log, logger, originalJson);
                    return new OkObjectResult("Request body has been processed successfully");
                }
                else
                {
                    throw new ArgumentException("VDA Integration: Not a valid request Body from VDA ");
                }
            }
            else
            {
                throw new ArgumentException("VDA Integration: No request Body from VDA");
            }
#pragma warning restore S2583 // Conditionally executed code should be reachable

        }

        private void UpsertVehicle(ILogger log, ILoggerService logger, JObject originalJson)
        {
            JObject payLoad = (JObject)originalJson["SyncHardIndividualProduct"]["DataArea"]["HardIndividualProduct"];
            log.LogInformation("payload is ready");
            this.dynamicsClient.SetLoggingReference(logger);
            log.LogInformation("setting logger info in dynamicsclient");
            UpsertVehicleIn casesIn = new UpsertVehicleIn(this.dynamicsClient, logger);
            log.LogInformation("Created the upsertVehicleInbound");
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
                throw new ArgumentException($"Upsert Vehicle: Failed in updating vehicle.");
            }
        }

        private void UpdateVehicleVariant(ILogger log, ILoggerService logger, JObject originalJson)
        {
            JObject payLoad = (JObject)originalJson["SyncHardIndividualProduct"]["DataArea"]["HardIndividualProduct"];
            log.LogInformation("payload is ready");
            this.dynamicsClient.SetLoggingReference(logger);
            log.LogInformation("setting logger info in dynamicsclient");
            UpsertVehicleVariantsIn casesIn = new UpsertVehicleVariantsIn(this.dynamicsClient, logger);
            log.LogInformation("Created the upsertVehicleInbound");
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
                throw new ArgumentException($"Update of Vehicle Variant: Failed in updating vehicle.");
            }
        }
    }
}
