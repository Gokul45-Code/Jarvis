// <copyright file="CrmHelper.cs" company="Microsoft.">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace MCS.Jarvis.CE.BusinessProcessesShared.Helpers
{
    using System;
    using System.IO;
    using System.Runtime.Serialization.Json;
    using System.Text;
    using Microsoft.Crm.Sdk.Messages;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;
    using Newtonsoft.Json.Linq;
    using static MCS.Jarvis.CE.BusinessProcessesShared.Helpers.Constants;

    /// <summary>
    /// Class for Helper.
    /// </summary>
    public static class CrmHelper
    {
        /// <summary>
        /// Check User Is Mercurius.
        /// </summary>
        /// <param name="targetEntity">Target Entity.</param>
        /// <param name="tracingService">Tracing Service.</param>
        /// <param name="orgservice">Org service.</param>
        /// <returns>Boolean value.</returns>
        public static bool CheckUserIsMercurius(Entity targetEntity, ITracingService tracingService, IOrganizationService orgservice)
        {
            bool isMercuriusUser = false;
            if (targetEntity.Attributes.Contains(General.ModifiedBy) && orgservice != null)
            {
                EntityReference entityReference = targetEntity.GetAttributeValue<EntityReference>(General.ModifiedBy);
                if (entityReference != null)
                {
                    tracingService.Trace("Modified By is present");
                    Entity userRecord = orgservice.Retrieve(entityReference.LogicalName, entityReference.Id, new ColumnSet(Users.fullName));
                    if (userRecord != null && userRecord.Attributes.Contains(Users.fullName))
                    {
                        string userName = userRecord.GetAttributeValue<string>(Users.fullName);
                        //// We are using "MERCURIUS User for siebel integration not send it back to Mercurius system."
                        if (!string.IsNullOrEmpty(userName) && userName.ToString().ToUpper().Contains("MERCURIUS"))
                        {
                            tracingService.Trace("Modified By is Mercurius User only.");
                            return true;
                        }
                    }
                }
            }

            return isMercuriusUser;
        }

        /// <summary>
        /// Check User Is Mercurius.
        /// </summary>
        /// <param name="userId">User Id.</param>
        /// <param name="tracingService">Tracing Service.</param>
        /// <param name="orgservice">Org service.</param>
        /// <returns>Boolean value.</returns>
        public static bool CheckUserIsMercurius(Guid userId, ITracingService tracingService, IOrganizationService orgservice)
        {
            bool isMercuriusUser = false;
            if (userId != Guid.Empty && orgservice != null)
            {
                Entity userRecord = orgservice.Retrieve(Users.logicalName, userId, new ColumnSet(Users.fullName));
                if (userRecord != null && userRecord.Attributes.Contains(Users.fullName))
                {
                    tracingService.Trace("Calling By is present");
                    string userName = userRecord.GetAttributeValue<string>(Users.fullName);

                    //// We are using "MERCURIUS User for siebel integration not send it back to Mercurius system."
                    if (!string.IsNullOrEmpty(userName) && userName.ToString().ToUpper().Contains("MERCURIUS"))
                    {
                        tracingService.Trace("Calling By is Mercurius User only.");
                        return true;
                    }
                }
            }

            return isMercuriusUser;
        }

        /// <summary>
        /// Check User Is System.
        /// </summary>
        /// <param name="userId">User Id.</param>
        /// <param name="tracingService">Tracing Service.</param>
        /// <param name="orgservice">Org service.</param>
        /// <returns>Boolean value.</returns>
        public static bool CheckUserIsSystem(Guid userId, ITracingService tracingService, IOrganizationService orgservice)
        {
            bool isSystemUser = false;
            if (userId != Guid.Empty && orgservice != null)
            {
                Entity userRecord = orgservice.Retrieve(Users.logicalName, userId, new ColumnSet(Users.fullName));
                if (userRecord != null && userRecord.Attributes.Contains(Users.fullName))
                {
                    tracingService.Trace("Calling By is present");
                    string userName = userRecord.GetAttributeValue<string>(Users.fullName);
                    //// We are using "MERCURIUS User for siebel integration not send it back to Mercurius system."
                    if (!string.IsNullOrEmpty(userName) && userName.ToString().ToUpper().Contains("SYSTEM"))
                    {
                        tracingService.Trace("Calling By is Mercurius User only.");
                        return true;
                    }
                }
            }

            return isSystemUser;
        }

        /// <summary>
        /// Check Case Type Is Breakdown.
        /// </summary>
        /// <param name="incidentId">Incident Id.</param>
        /// <param name="tracingService">Tracing Service.</param>
        /// <param name="orgservice">Org service.</param>
        /// <returns>Boolean value.</returns>
        public static bool CheckCaseTypeIsBreakdown(Guid incidentId, ITracingService tracingService, IOrganizationService orgservice)
        {
            bool isCaseTypeBreakdown = false;
            if (incidentId != Guid.Empty && orgservice != null)
            {
                Entity incidentRecord = orgservice.Retrieve(Incident.logicalName, incidentId, new ColumnSet(Incident.casetypecode));
                if (incidentRecord != null && incidentRecord.Attributes.Contains(Incident.casetypecode))
                {
                    tracingService.Trace("Case Type Code is present");
                    OptionSetValue caseTypeCode = (OptionSetValue)incidentRecord.Attributes[Incident.casetypecode];

                    //// We are using "MERCURIUS User for siebel integration not send it back to Mercurius system."
                    if (!string.IsNullOrEmpty(caseTypeCode.Value.ToString()) && caseTypeCode.Value == (int)CaseTypeCode.Breakdown)
                    {
                        tracingService.Trace("Case Type Code is Breakdown only.");
                        return true;
                    }
                }
            }

            return isCaseTypeBreakdown;
        }

        /// <summary>
        /// Check Pre Post Image.
        /// </summary>
        /// <param name="preImage">Pre Image.</param>
        /// <param name="postImage">Post Image.</param>
        /// <param name="tracingService">Tracing Service.</param>
        /// <returns>bool value.</returns>
        public static bool CheckPrePostImage(Entity preImage, Entity postImage, ITracingService tracingService)
        {
            tracingService.Trace("CheckPrePostImage Method Triggered.");
            if (postImage.Attributes.Count > 0)
            {
                tracingService.Trace($"PostImageCount: {postImage.Attributes.Count}");
                tracingService.Trace($"PreImageCount: {preImage.Attributes.Count}");
                foreach (var img in postImage.Attributes)
                {
                    if (!preImage.Attributes.Contains(img.Key.ToString()))
                    {
                        return true;
                    }

                    tracingService.Trace($"postImageKey: {img.Key}");
                    tracingService.Trace($"postImage: {img.Value}");
                    tracingService.Trace($"preImage:  {preImage.Attributes[img.Key]}");
                    tracingService.Trace($"Value: {img.Value.Equals(preImage.Attributes[img.Key])}");

                    if (!img.Value.Equals(preImage.Attributes[img.Key]))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Jarvis To Function.
        /// </summary>
        /// <param name="entity">Entity entity.</param>
        /// <param name="tracingService">Tracing Service.</param>
        /// <param name="unsecureString">UnSecure String.</param>
        /// <param name="url">Url value.</param>
        public static void JarvisToFunction(Entity entity, ITracingService tracingService, string unsecureString, string url)
        {
            AttributesJarvis jarvisFields = new AttributesJarvis
            {
                Id = entity.Id.ToString(),
                TransType = unsecureString,
            };
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(AttributesJarvis));
            MemoryStream memoryStrem = new MemoryStream();
            serializer.WriteObject(memoryStrem, jarvisFields);
            var jsonObject = Encoding.Default.GetString(memoryStrem.ToArray());
            tracingService.Trace("Before: AzureFunctionTrigger");
            Uri serviceURL = new Uri(url);
            BaseWebClient baseWebClient = new BaseWebClient();
            baseWebClient.UploadString(serviceURL, jsonObject, tracingService);
            memoryStrem.Dispose();
            tracingService.Trace("After: AzureFunctionTrigger");
        }

        /// <summary>
        /// [Integration Plugins]JarvisToFunction.
        /// </summary>
        /// <param name="entity">Entity entity.</param>
        /// <param name="entityData">Entity Data.</param>
        /// <param name="tracingService">Tracing Service.</param>
        /// <param name="unsecureString">Unsecure String.</param>
        /// <param name="secureString">Secure String.</param>
        public static void JarvisToFunction(Entity entity, string entityData, ITracingService tracingService, string unsecureString, string secureString)
        {
            MemoryStream memoryStrem = new MemoryStream();
            try
            {
                AttributesJarvis jarvisFields = new AttributesJarvis
                {
                    Id = entity.Id.ToString(),
                    TransType = unsecureString,
                    EntityData = entityData,
                };
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(AttributesJarvis));
                serializer.WriteObject(memoryStrem, jarvisFields);
                var jsonObject = Encoding.Default.GetString(memoryStrem.ToArray());
                tracingService.Trace("Before: AzureFunctionTrigger");
                var appDetails = JObject.Parse(secureString);
                Uri serviceURL = new Uri(appDetails["functionAppUrl"].ToString());
                BaseWebClient baseWebClient = new BaseWebClient();
                baseWebClient.UploadString(serviceURL, jsonObject, tracingService, secureString);
                tracingService.Trace("After: AzureFunctionTrigger");
            }
            catch
            {
            }
            finally
            {
                memoryStrem.Dispose();
            }
        }

        /// <summary>
        /// [Integration Translation Plugins] Jarvis To Function.
        /// </summary>
        /// <param name="entity">Entity entity.</param>
        /// <param name="entityData">Entity Data.</param>
        /// <param name="tracingService">Tracing Service.</param>
        /// <param name="unsecureString">Unsecure String.</param>
        /// <param name="secureString">Secure String.</param>
        /// <param name="translationType">translation Type.</param>
        public static void JarvisToFunction(Entity entity, string entityData, ITracingService tracingService, string unsecureString, string secureString, string translationType)
        {
            MemoryStream memoryStrem = new MemoryStream();
            try
            {
                TransAttributesJarvis jarvisFields = new TransAttributesJarvis
                {
                    Id = entity.Id.ToString(),
                    TransType = unsecureString,
                    EntityData = entityData,
                    TranslationType = translationType,
                };
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(TransAttributesJarvis));
                serializer.WriteObject(memoryStrem, jarvisFields);
                var jsonObject = Encoding.Default.GetString(memoryStrem.ToArray());
                tracingService.Trace("Before: AzureFunctionTrigger");
                var appDetails = JObject.Parse(secureString);
                Uri serviceURL = new Uri(appDetails["functionAppUrl"].ToString());
                BaseWebClient baseWebClient = new BaseWebClient();
                baseWebClient.UploadString(serviceURL, jsonObject, tracingService, secureString);
                tracingService.Trace("After: AzureFunctionTrigger");
            }
            catch
            {
            }
            finally
            {
                memoryStrem.Dispose();
            }
        }

        /// <summary>
        /// [GOP Automate] Jarvis To Gop Function.
        /// </summary>
        /// <param name="entity">Entity entity.</param>
        /// <param name="entityData">Entity Data.</param>
        /// <param name="tracingService">Tracing Service.</param>
        /// <param name="unsecureString">Unsecure String.</param>
        /// <param name="secureString">Secure String.</param>
        public static void JarvisToGopFunction(Entity entity, string entityData, ITracingService tracingService, string unsecureString, string secureString)
        {
            MemoryStream memoryStrem = new MemoryStream();
            try
            {
                GopJarvis jarvisFields = new GopJarvis
                {
                    IncidentId = entity.Id.ToString(),
                };
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(GopJarvis));
                serializer.WriteObject(memoryStrem, jarvisFields);
                var jsonObject = Encoding.Default.GetString(memoryStrem.ToArray());
                tracingService.Trace("Before: AzureFunctionTrigger");
                var appDetails = JObject.Parse(secureString);
                Uri serviceURL = new Uri(appDetails["functionAppUrl"].ToString());
                BaseWebClient baseWebClient = new BaseWebClient();
                baseWebClient.UploadString(serviceURL, jsonObject, tracingService, secureString);
                tracingService.Trace("After: AzureFunctionTrigger");
            }
            catch
            {
            }
            finally
            {
                memoryStrem.Dispose();
            }
        }

        /// <summary>
        /// Get Release Automation Config.
        /// </summary>
        /// <param name="orgService">org Service.</param>
        /// <param name="automation">automation value.</param>
        /// <param name="tracingService">tracing Service.</param>
        /// <returns>config Enabled.</returns>
        public static OptionSetValue GetReleaseAutomationConfig(IOrganizationService orgService, string automation, ITracingService tracingService)
        {
            OptionSetValue configEnabled = new OptionSetValue();
            EntityCollection getJarvisConfig = orgService.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getAutomationConfigs)));
            if (getJarvisConfig != null && getJarvisConfig.Entities.Count > 0)
            {
                if (getJarvisConfig.Entities[0].Attributes.Contains(automation) && getJarvisConfig.Entities[0].Attributes[automation] != null)
                {
                    configEnabled = (OptionSetValue)getJarvisConfig.Entities[0].Attributes[automation];
                }
            }

            return configEnabled;
        }

        /// <summary>
        /// get Automation Config.
        /// </summary>
        /// <param name="orgService">org Service.</param>
        /// <param name="automation">automation value.</param>
        /// <param name="tracingService">tracing Service.</param>
        /// <returns>config Enabled.</returns>
        public static bool GetAutomationConfig(IOrganizationService orgService, string automation, ITracingService tracingService)
        {
            bool configEnabled = false;
            EntityCollection getJarvisConfig = orgService.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getAutomationConfigs)));
            if (getJarvisConfig != null && getJarvisConfig.Entities.Count > 0)
            {
                if (getJarvisConfig.Entities[0].Attributes.Contains(automation) && getJarvisConfig.Entities[0].Attributes[automation] != null)
                {
                    configEnabled = (bool)getJarvisConfig.Entities[0].Attributes[automation];
                }
            }

            return configEnabled;
        }

        /// <summary>
        /// Currency Exchange.
        /// </summary>
        /// <param name="sourceCurrencyId">source CurrencyId.</param>
        /// <param name="targetCurrencyId">target CurrencyId.</param>
        /// <param name="service">Organization Service.</param>
        /// <returns>decimal value.</returns>
        public static decimal CurrencyExchange(Guid sourceCurrencyId, Guid targetCurrencyId, IOrganizationService service)
        {
            decimal exchangeValue = 0;
            if (sourceCurrencyId == targetCurrencyId)
            {
                exchangeValue = 1;
            }
            else
            {
                EntityCollection exchangeRates = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.ExchangeRate, sourceCurrencyId, targetCurrencyId)));

                foreach (var exchangeRate in exchangeRates.Entities)
                {
                    exchangeValue = (decimal)exchangeRate["jarvis_value"];
                }
            }

            return exchangeValue;
        }

        /// <summary>
        /// Retrieve UTC From Local Time.
        /// </summary>
        /// <param name="service">Org service.</param>
        /// <param name="localTime">localTime datetime.</param>
        /// <param name="timeZoneCode">time Zone Code.</param>
        /// <returns>Date Time value.</returns>
        public static DateTime RetrieveUTCFromLocalTime(IOrganizationService service, DateTime localTime, int? timeZoneCode)
        {
            if (!timeZoneCode.HasValue)
            {
                return localTime;
            }

            var request = new UtcTimeFromLocalTimeRequest
            {
                TimeZoneCode = timeZoneCode.Value,
                LocalTime = localTime,
            };

            var response = (UtcTimeFromLocalTimeResponse)service.Execute(request);

            return response.UtcTime;
        }
    }
}
