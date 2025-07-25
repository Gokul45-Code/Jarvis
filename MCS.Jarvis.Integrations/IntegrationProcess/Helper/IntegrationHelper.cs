// <copyright file="IntegrationHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace MCS.Jarvis.IntegrationProcess.Helper
{
    using System.Globalization;
    using global::IntegrationProcess.Helper.Constants;
    using MCS.Jarvis.Integration.Base.Dynamics;
    using MCS.Jarvis.Integration.Base.Logging;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Integration Helper Class.
    /// </summary>
    public class IntegrationHelper
    {
        /// <summary>
        /// dynamics API Client.
        /// </summary>
        private readonly IDynamicsApiClient dynamicsApiClient;

        /// <summary>
        /// logger object.
        /// </summary>
        private readonly ILoggerService logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="IntegrationHelper"/> class.
        /// Generic Dynamics constructor.
        /// </summary>
        /// <param name="dynamicsApiClient">dynamics API Client.</param>
        /// <param name="logger">logger object.</param>
        public IntegrationHelper(IDynamicsApiClient dynamicsApiClient, ILoggerService logger)
        {
            this.dynamicsApiClient = dynamicsApiClient;
            this.logger = logger;
        }

        /// <summary>
        /// LookupValueFromTarget - Return guid and true for the record matched from Jarvis else return false.
        /// </summary>
        /// <param name="retrieveList">Retrieve List of Master data.</param>
        /// <param name="sourcePayload">Source Payload.</param>
        /// <param name="sourceField">Source field.</param>
        /// <param name="targetLookupEntity">Target Lookup Entity.</param>
        /// <param name="targetFieldSchema">Target Field Schema.</param>
        /// <param name="targetEntityName">Target Entity Name.</param>
        /// <returns>Id of the attribute.</returns>
        public (bool, JToken? value) LookupValueFromTarget(Dictionary<string, JArray?> retrieveList, JObject? sourcePayload, string sourceField, string targetLookupEntity, string targetFieldSchema, string targetEntityName)
        {
            try
            {
                var matchMasterRecord = retrieveList.First(item => item.Key.ToUpper().Contains(targetLookupEntity.ToUpper()));

                if (!string.IsNullOrEmpty(sourceField) && !string.IsNullOrEmpty(targetFieldSchema) && !string.IsNullOrEmpty(targetEntityName) && matchMasterRecord.Key != null)
                {
                    var matachingrecord = matchMasterRecord.Value?.ToList();
                    if ((sourceField.Split(",").Length > 1 && targetFieldSchema.Split(",").Length > 1)
                    && sourceField.Split(",").Length == targetFieldSchema.Split(",").Length)
                    {
                        for (var i = 0; i < sourceField.Split(",").Length; i++)
                        {
                            if (matachingrecord != null && matachingrecord.Count > 0)
                            {
                                var sourceValue = sourcePayload.SelectToken(sourceField.Split(",")[i]) != null ? sourcePayload.SelectToken(sourceField.Split(",")[i])?.Value<JToken>() : string.Empty;
                                if (!string.IsNullOrEmpty(sourceValue?.ToString()))
                                {
                                    var matchrecord = matachingrecord.Where(item => item[targetFieldSchema.Split(",")[i]]?.ToString().ToUpper().Replace("-", string.Empty) == sourceValue.ToString().ToUpper().Replace("-", string.Empty)).ToList();
                                    matachingrecord = matchrecord;
                                }
                            }
                            else
                            {
                                break;
                            }
                        }

                        if (matachingrecord != null && matachingrecord.Count > 0)
                        {
                            JToken? jToken = matachingrecord.Select(item => item[targetEntityName.ToLower()]).First();
                            return (true, jToken);
                        }
                    }
                    else
                    {
                        var sourceValue = sourcePayload.SelectToken(sourceField).Value<JToken>();
                        if (!string.IsNullOrEmpty(sourceValue.ToString()) && matachingrecord != null && matachingrecord.Count > 0)
                        {
                            var matchRecord = matachingrecord.Where(item => item[targetFieldSchema].ToString().ToUpper().Replace("-", string.Empty) == sourceValue.ToString().ToUpper().Replace("-", string.Empty)).ToList();

                            if (matchRecord != null && matchRecord.Count > 0)
                            {
                                JToken? jToken = matchRecord.Select(item => item[targetEntityName.ToLower()]).First();
                                return (true, jToken);
                            }
                        }
                    }
                }
                else
                {
                    this.logger.LogException(new ArgumentException("Not a Valid Configuration to match and retrive value from source"));
                    throw new ArgumentException($"Not a Valid Configuration to match and retrive value from source");
                }

                return (false, string.Empty);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($" Retrieving Lookup Value from Jarvis entity {targetLookupEntity} Failed with Error Message - " + ex.Message);
            }
        }

        /// <summary>
        /// SubgridValueFromTarget - Get related records from Target.
        /// </summary>
        /// <param name="retrieveList">Retrieve List of Master data.</param>
        /// <param name="targetLookupEntity">Target Lookup Entity.</param>
        /// <param name="targetRelationshipName">Target Relatiomship Name.</param>
        /// <returns>Id of the attribute.</returns>
        public JToken? SubgridValueFromTarget(Dictionary<string, JArray?> retrieveList, string targetLookupEntity, string targetRelationshipName)
        {
            try
            {
                if (targetLookupEntity != null && targetRelationshipName != null)
                {
                    var record = retrieveList.First(item => item.Key.ToUpper() == targetLookupEntity.ToUpper()).Value;
                    if (record != null && record.Count > 0)
                    {
                        record.First().ToObject<JObject>().TryGetValue(targetRelationshipName, StringComparison.OrdinalIgnoreCase, out JToken? value);
                        return value;
                    }
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                throw new ArgumentException($" Retrieving Subgrid Value From Entity Failed with Error Message - " + ex.Message);
            }
        }

        /// <summary>
        /// SetFieldMapping - Set Payload Field Mapping.
        /// </summary>
        /// <param name="currentPayload">Current Payload.</param>
        /// <param name="fieldMappings">Field Mapping.</param>
        /// <param name="retrieveList">Retrieve List.</param>
        /// <returns>JObject of return payload.</returns>
        public JObject SetFieldMapping(JObject? currentPayload, JToken? fieldMappings, Dictionary<string, JArray?> retrieveList)
        {
            try
            {
                JObject payload = new JObject();

                foreach (JObject fieldMapping in fieldMappings.Values<JObject>())
                {
                    var sourceFieldSchema = DynamicsApiHelper.GetStringValueFromJObject(fieldMapping, Constants.SourceFieldSchema);
                    var targetFieldSchema = DynamicsApiHelper.GetStringValueFromJObject(fieldMapping, Constants.TargetFieldSchema);
                    var targetFieldDataType = DynamicsApiHelper.GetStringValueFromJObject(fieldMapping, Constants.TargetFieldDataType).ToUpper();
                    var targetLookupFieldValue = DynamicsApiHelper.GetStringValueFromJObject(fieldMapping, Constants.TargetLookupFieldValue);
                    var targetFieldLookupEntity = DynamicsApiHelper.GetStringValueFromJObject(fieldMapping, Constants.TargetFieldLookupEntity);
                    var targetLookupEntity = DynamicsApiHelper.GetStringValueFromJObject(fieldMapping, Constants.TargetLookupEntity);
                    var targetAlternateKeys = DynamicsApiHelper.GetStringValueFromJObject(fieldMapping, Constants.TargetAlternateKeys);
                    fieldMapping.TryGetValue(Constants.OptionSetMapping, StringComparison.OrdinalIgnoreCase, out JToken? optionSetMapping);

                    // Framing.
                    if (!string.IsNullOrEmpty(targetFieldSchema) && !string.IsNullOrEmpty(sourceFieldSchema) && !string.IsNullOrEmpty(targetFieldDataType) && currentPayload != null)
                    {
                        var sourceValue = currentPayload.SelectToken(sourceFieldSchema.Split(",")[0])?.Value<JToken>();
                        if ((sourceValue != null) || (!string.IsNullOrEmpty(targetLookupEntity) && !string.IsNullOrEmpty(targetFieldLookupEntity)))
                        {
                            switch (targetFieldDataType)
                            {
                                case "TEXT":
                                    {
                                        payload.Add(targetFieldSchema.ToString(), Convert.ToString(sourceValue.Value<JToken>(), CultureInfo.InvariantCulture));
                                        break;
                                    }

                                case "DECIMAL":
                                    {
                                        if (!string.IsNullOrEmpty(sourceValue.Value<JToken>().ToString()))
                                        {
                                            payload.Add(targetFieldSchema, Convert.ToDecimal(sourceValue.Value<JToken>(), CultureInfo.InvariantCulture));
                                        }
                                        else
                                        {
                                            payload.Add(targetFieldSchema, null);
                                        }

                                        break;
                                    }

                                case "INTEGER":
                                    {
                                        if (!string.IsNullOrEmpty(sourceValue.Value<JToken>().ToString()))
                                        {
                                            payload.Add(targetFieldSchema, Convert.ToInt32(sourceValue.Value<JToken>(), CultureInfo.InvariantCulture));
                                        }
                                        else
                                        {
                                            payload.Add(targetFieldSchema, null);
                                        }

                                        break;
                                    }

                                case "BIGINTEGER":
                                    {
                                        if (!string.IsNullOrEmpty(sourceValue.Value<JToken>().ToString()))
                                        {
                                            payload.Add(targetFieldSchema, Convert.ToInt64(sourceValue.Value<JToken>(), CultureInfo.InvariantCulture));
                                        }
                                        else
                                        {
                                            payload.Add(targetFieldSchema, null);
                                        }

                                        break;
                                    }

                                case "TWOOPTION":
                                    {
                                        payload.Add(targetFieldSchema, Convert.ToBoolean(sourceValue.Value<JToken>(), CultureInfo.InvariantCulture));
                                        break;
                                    }

                                case "LOOKUP":
                                    {
                                        var lookupErrorFlag = false;
                                        if (targetLookupEntity != null && targetAlternateKeys != null && targetFieldLookupEntity != null && targetLookupFieldValue != null)
                                        {
                                            if ((sourceFieldSchema.Split(",").Length > 1 && targetAlternateKeys.Split(",").Length > 1)
                                               && sourceFieldSchema.Split(",").Length == targetAlternateKeys.Split(",").Length)
                                            {
                                                for (var i = 0; i < sourceFieldSchema.Split(",").Length; i++)
                                                {
                                                    var lookupSourceValue = currentPayload.SelectToken(sourceFieldSchema.Split(",")[i]) != null ? currentPayload.SelectToken(sourceFieldSchema.Split(",")[i]).Value<JToken>() : string.Empty;
                                                    if (string.IsNullOrEmpty(lookupSourceValue.ToString()))
                                                    {
                                                        lookupErrorFlag = true;
                                                        break;
                                                    }
                                                }

                                                if (!lookupErrorFlag)
                                                {
                                                    var lookupResult = this.LookupValueFromTarget(retrieveList, currentPayload, sourceFieldSchema, targetLookupEntity, targetAlternateKeys, targetLookupFieldValue);
                                                    if (lookupResult.Item1)
                                                    {
                                                        JToken token = string.Format("/{0}({1})", targetFieldLookupEntity, lookupResult.value);
                                                        payload.Add(string.Concat(targetFieldSchema, "@odata.bind"), token);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                var lookupSourceValue = currentPayload.SelectToken(sourceFieldSchema.Split(",")[0])?.Value<JToken>();
                                                if (lookupSourceValue != null && !string.IsNullOrEmpty(lookupSourceValue.ToString()))
                                                {
                                                    var lookupResult = this.LookupValueFromTarget(retrieveList, currentPayload, sourceFieldSchema, targetLookupEntity, targetAlternateKeys, targetLookupFieldValue);
                                                    if (lookupResult.Item1)
                                                    {
                                                        JToken token = string.Format("/{0}({1})", targetFieldLookupEntity, lookupResult.value);
                                                        payload.Add(string.Concat(targetFieldSchema, "@odata.bind"), token);
                                                    }
                                                }
                                            }

                                            break;
                                        }
                                        else
                                        {
                                            this.logger.LogException(new Exception($"Configuration for Field Mapping{targetFieldSchema} is Incorrect"));
                                            throw new ArgumentException($"Configuration for Field Mapping{targetFieldSchema} is Incorrect");
                                        }
                                    }

                                case "DATETIME":
                                    {
                                        if (!string.IsNullOrEmpty(sourceValue.Value<JToken>().ToString()))
                                        {
                                            payload.Add(targetFieldSchema, Convert.ToDateTime(sourceValue.Value<JToken>(), CultureInfo.InvariantCulture));
                                        }
                                        else
                                        {
                                            payload.Add(targetFieldSchema, null);
                                        }

                                        break;
                                    }

                                case "OPTIONSET":
                                    {
                                        if (!string.IsNullOrEmpty(sourceValue.Value<JToken>().ToString()) && optionSetMapping != null)
                                        {
                                            var optiosetResult = this.OptionSetFromTarget(optionSetMapping?.ToObject<JObject>(), sourceValue.Value<JToken>().ToString());
                                            if (optiosetResult.Item1)
                                            {
                                                payload.Add(targetFieldSchema, Convert.ToInt32(optiosetResult.value, CultureInfo.InvariantCulture));
                                            }
                                        }
                                        else
                                        {
                                            payload.Add(targetFieldSchema, null);
                                        }

                                        break;
                                    }

                                case "BOOLEAN":
                                    {
                                        if (sourceValue != null && !string.IsNullOrEmpty(sourceValue.ToString()))
                                        {
                                            switch (sourceValue.ToString().ToLower())
                                            {
                                                case "y":
                                                    {
                                                        payload.Add(targetFieldSchema, true);
                                                        break;
                                                    }

                                                case "n":
                                                    {
                                                        payload.Add(targetFieldSchema, false);
                                                        break;
                                                    }

                                                default:
                                                    {
                                                        this.logger.LogException(new Exception($"Boolean Value {sourceValue} is not valid: - {sourceFieldSchema}"));
                                                        throw new ArgumentException($"Boolean Value {sourceValue} is not valid: - {sourceFieldSchema}");
                                                    }
                                            }
                                        }

                                        break;
                                    }

                                case "DATE":
                                    {
                                        if (!string.IsNullOrEmpty(sourceValue.Value<JToken>().ToString()))
                                        {
                                            var timeZoneCode = DynamicsApiHelper.GetStringValueFromJObject(fieldMapping, Constants.TimeZoneCode);
                                            if (timeZoneCode != null && !string.IsNullOrEmpty(timeZoneCode))
                                            {
                                                TimeZoneInfo gmtPlus1TimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneCode);
                                                payload.Add(targetFieldSchema, TimeZoneInfo.ConvertTimeFromUtc(Convert.ToDateTime(sourceValue.Value<JToken>(), CultureInfo.InvariantCulture), gmtPlus1TimeZone).Date);
                                            }
                                            else
                                            {
                                                payload.Add(targetFieldSchema, Convert.ToDateTime(sourceValue.Value<JToken>(), CultureInfo.InvariantCulture).Date);
                                            }
                                        }
                                        else
                                        {
                                            payload.Add(targetFieldSchema, null);
                                        }

                                        break;
                                    }

                                case "TIME":
                                    {
                                        if (!string.IsNullOrEmpty(sourceValue.Value<JToken>().ToString()))
                                        {
                                            var timeZoneCode = DynamicsApiHelper.GetStringValueFromJObject(fieldMapping, Constants.TimeZoneCode);
                                            if (timeZoneCode != null && !string.IsNullOrEmpty(timeZoneCode))
                                            {
                                                TimeZoneInfo gmtPlus1TimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneCode);
                                                payload.Add(targetFieldSchema, TimeZoneInfo.ConvertTimeFromUtc(Convert.ToDateTime(sourceValue.Value<JToken>(), CultureInfo.InvariantCulture), gmtPlus1TimeZone).ToString("HH:mm", CultureInfo.InvariantCulture));
                                            }
                                            else
                                            {
                                                payload.Add(targetFieldSchema, Convert.ToDateTime(sourceValue.Value<JToken>(), CultureInfo.InvariantCulture).ToString("HH:mm", CultureInfo.InvariantCulture));
                                            }
                                        }
                                        else
                                        {
                                            payload.Add(targetFieldSchema, null);
                                        }

                                        break;
                                    }
                                case "LOOKUPWITHDEFAULT":
                                    {
                                        var lookupErrorFlag = false;
                                        if (targetLookupEntity != null && targetAlternateKeys != null && targetFieldLookupEntity != null && targetLookupFieldValue != null)
                                        {
                                            if ((sourceFieldSchema.Split(",").Length > 1 && targetAlternateKeys.Split(",").Length > 1)
                                               && sourceFieldSchema.Split(",").Length == targetAlternateKeys.Split(",").Length)
                                            {
                                                for (var i = 0; i < sourceFieldSchema.Split(",").Length; i++)
                                                {
                                                    var lookupSourceValue = currentPayload.SelectToken(sourceFieldSchema.Split(",")[i]) != null ? currentPayload.SelectToken(sourceFieldSchema.Split(",")[i]).Value<JToken>() : string.Empty;
                                                    if (string.IsNullOrEmpty(lookupSourceValue.ToString()))
                                                    {
                                                        lookupErrorFlag = true;
                                                        break;
                                                    }
                                                }

                                                if (!lookupErrorFlag)
                                                {
                                                    var lookupResult = this.LookupValueFromTarget(retrieveList, currentPayload, sourceFieldSchema, targetLookupEntity, targetAlternateKeys, targetLookupFieldValue);
                                                    if (lookupResult.Item1)
                                                    {
                                                        JToken token = string.Format("/{0}({1})", targetFieldLookupEntity, lookupResult.value);
                                                        payload.Add(string.Concat(targetFieldSchema, "@odata.bind"), token);
                                                    }
                                                    else
                                                    {
                                                        var defaultValue = DynamicsApiHelper.GetStringValueFromJObject(fieldMapping, Constants.DefaultValue);
                                                        if (!string.IsNullOrEmpty(defaultValue))
                                                        {
                                                            JToken token = string.Format("/{0}({1})", targetFieldLookupEntity, defaultValue);
                                                            payload.Add(string.Concat(targetFieldSchema, "@odata.bind"), token);
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                var lookupSourceValue = currentPayload.SelectToken(sourceFieldSchema.Split(",")[0])?.Value<JToken>();
                                                if (lookupSourceValue != null && !string.IsNullOrEmpty(lookupSourceValue.ToString()))
                                                {
                                                    var lookupResult = this.LookupValueFromTarget(retrieveList, currentPayload, sourceFieldSchema, targetLookupEntity, targetAlternateKeys, targetLookupFieldValue);
                                                    if (lookupResult.Item1)
                                                    {
                                                        JToken token = string.Format("/{0}({1})", targetFieldLookupEntity, lookupResult.value);
                                                        payload.Add(string.Concat(targetFieldSchema, "@odata.bind"), token);
                                                    }
                                                    else
                                                    {
                                                        var defaultValue = DynamicsApiHelper.GetStringValueFromJObject(fieldMapping, Constants.DefaultValue);
                                                        if (!string.IsNullOrEmpty(defaultValue))
                                                        {
                                                            JToken token = string.Format("/{0}({1})", targetFieldLookupEntity, defaultValue);
                                                            payload.Add(string.Concat(targetFieldSchema, "@odata.bind"), token);
                                                        }
                                                    }
                                                }
                                            }

                                            break;
                                        }
                                        else
                                        {
                                            this.logger.LogException(new Exception($"Configuration for Field Mapping{targetFieldSchema} is Incorrect"));
                                            throw new ArgumentException($"Configuration for Field Mapping{targetFieldSchema} is Incorrect");
                                        }
                                    }

                                default: break;
                            }
                        }
                    }
                }

                if (payload.Count > 0)
                {
                    return payload;
                }
                else
                {
                    throw new ArgumentException($"Payload formation didnt happend for the {fieldMappings}");
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException($" Set Field Mapping For Payload Failed with Error Message - " + ex.Message);
            }
        }

        /// <summary>
        /// SetFieldMappingWithOverWritable - Set Payload Field Mapping.
        /// </summary>
        /// <param name="currentPayload">Current Payload.</param>
        /// <param name="fieldMappings">Field Mapping.</param>
        /// <param name="retrieveList">Retrieve List.</param>
        /// <returns>JObject of return payload.</returns>
        public JObject SetFieldMappingWithOverWritable(JObject? currentPayload, JToken? fieldMappings, Dictionary<string, JArray?> retrieveList)
        {
            try
            {
                JObject payload = new JObject();

                foreach (JObject fieldMapping in fieldMappings.Values<JObject>())
                {
                    var sourceFieldSchema = DynamicsApiHelper.GetStringValueFromJObject(fieldMapping, Constants.SourceFieldSchema);
                    var targetFieldSchema = DynamicsApiHelper.GetStringValueFromJObject(fieldMapping, Constants.TargetFieldSchema);
                    var targetFieldDataType = DynamicsApiHelper.GetStringValueFromJObject(fieldMapping, Constants.TargetFieldDataType).ToUpper();
                    var targetLookupFieldValue = DynamicsApiHelper.GetStringValueFromJObject(fieldMapping, Constants.TargetLookupFieldValue);
                    var targetFieldLookupEntity = DynamicsApiHelper.GetStringValueFromJObject(fieldMapping, Constants.TargetFieldLookupEntity);
                    var targetLookupEntity = DynamicsApiHelper.GetStringValueFromJObject(fieldMapping, Constants.TargetLookupEntity);
                    var targetAlternateKeys = DynamicsApiHelper.GetStringValueFromJObject(fieldMapping, Constants.TargetAlternateKeys);
                    fieldMapping.TryGetValue(Constants.OptionSetMapping, StringComparison.OrdinalIgnoreCase, out JToken optionSetMapping);
                    bool? isOverwritable = DynamicsApiHelper.GetBoolValueFromJObject(fieldMapping, Constants.IsOverwritable);

                    // Framing.
                    if (!string.IsNullOrEmpty(targetFieldSchema) && !string.IsNullOrEmpty(sourceFieldSchema) && !string.IsNullOrEmpty(targetFieldDataType) && currentPayload != null)
                    {
                        var sourceValue = currentPayload.SelectToken(sourceFieldSchema.Split(",")[0])?.Value<JToken>();
                        switch (targetFieldDataType)
                            {
                                case "TEXT":
                                    {
                                        if (sourceValue != null && !string.IsNullOrEmpty(sourceValue.Value<JToken>().ToString()))
                                        {
                                            payload.Add(targetFieldSchema.ToString(), Convert.ToString(sourceValue.Value<JToken>(), CultureInfo.InvariantCulture));
                                        }
                                        else if (isOverwritable != null && isOverwritable == true)
                                        {
                                            payload.Add(targetFieldSchema.ToString(), string.Empty);
                                        }

                                        break;
                                    }

                                case "DECIMAL":
                                    {
                                        if (sourceValue != null && !string.IsNullOrEmpty(sourceValue.Value<JToken>().ToString()))
                                        {
                                            payload.Add(targetFieldSchema, Convert.ToDecimal(sourceValue.Value<JToken>(), CultureInfo.InvariantCulture));
                                        }
                                        else if (isOverwritable != null && isOverwritable == true)
                                        {
                                            payload.Add(targetFieldSchema, null);
                                        }

                                        break;
                                    }

                                case "INTEGER":
                                    {
                                        if (sourceValue != null && !string.IsNullOrEmpty(sourceValue.Value<JToken>().ToString()))
                                        {
                                            payload.Add(targetFieldSchema, Convert.ToInt32(sourceValue.Value<JToken>(), CultureInfo.InvariantCulture));
                                        }
                                        else if (isOverwritable != null && isOverwritable == true)
                                        {
                                            payload.Add(targetFieldSchema, null);
                                        }

                                        break;
                                    }

                                case "BIGINTEGER":
                                    {
                                        if (sourceValue != null && !string.IsNullOrEmpty(sourceValue.Value<JToken>().ToString()))
                                        {
                                            payload.Add(targetFieldSchema, Convert.ToInt64(sourceValue.Value<JToken>(), CultureInfo.InvariantCulture));
                                        }
                                        else if (isOverwritable != null && isOverwritable == true)
                                        {
                                            payload.Add(targetFieldSchema, null);
                                        }

                                        break;
                                    }

                                case "TWOOPTION":
                                    {
                                        payload.Add(targetFieldSchema, Convert.ToBoolean(sourceValue.Value<JToken>(), CultureInfo.InvariantCulture));
                                        break;
                                    }

                                case "LOOKUP":
                                    {
                                        var lookupErrorFlag = false;
                                        if (targetLookupEntity != null && targetAlternateKeys != null && targetFieldLookupEntity != null && targetLookupFieldValue != null)
                                        {
                                            if ((sourceFieldSchema.Split(",").Length > 1 && targetAlternateKeys.Split(",").Length > 1)
                                               && sourceFieldSchema.Split(",").Length == targetAlternateKeys.Split(",").Length)
                                            {
                                                for (var i = 0; i < sourceFieldSchema.Split(",").Length; i++)
                                                {
                                                    var lookupSourceValue = currentPayload.SelectToken(sourceFieldSchema.Split(",")[i]) != null ? currentPayload.SelectToken(sourceFieldSchema.Split(",")[i]).Value<JToken>() : string.Empty;
                                                    if (string.IsNullOrEmpty(lookupSourceValue.ToString()))
                                                    {
                                                        lookupErrorFlag = true;
                                                        break;
                                                    }
                                                }

                                                if (!lookupErrorFlag)
                                                {
                                                    var lookupResult = this.LookupValueFromTarget(retrieveList, currentPayload, sourceFieldSchema, targetLookupEntity, targetAlternateKeys, targetLookupFieldValue);
                                                    if (lookupResult.Item1)
                                                    {
                                                        JToken token = string.Format("/{0}({1})", targetFieldLookupEntity, lookupResult.value);
                                                        payload.Add(string.Concat(targetFieldSchema, "@odata.bind"), token);
                                                    }
                                                    else if (isOverwritable != null && isOverwritable == true)
                                                    {
                                                        payload.Add(targetFieldSchema, null);
                                                    }
                                                }
                                                else if (isOverwritable != null && isOverwritable == true)
                                                {
                                                    payload.Add(targetFieldSchema, null);
                                                }
                                            }
                                            else
                                            {
                                                var lookupSourceValue = currentPayload.SelectToken(sourceFieldSchema.Split(",")[0])?.Value<JToken>();
                                                if (lookupSourceValue != null && !string.IsNullOrEmpty(lookupSourceValue.ToString()))
                                                {
                                                    var lookupResult = this.LookupValueFromTarget(retrieveList, currentPayload, sourceFieldSchema, targetLookupEntity, targetAlternateKeys, targetLookupFieldValue);
                                                    if (lookupResult.Item1)
                                                    {
                                                        JToken token = string.Format("/{0}({1})", targetFieldLookupEntity, lookupResult.value);
                                                        payload.Add(string.Concat(targetFieldSchema, "@odata.bind"), token);
                                                    }
                                                    else if (isOverwritable != null && isOverwritable == true)
                                                    {
                                                        payload.Add(targetFieldSchema, null);
                                                    }
                                                }
                                                else if (isOverwritable != null && isOverwritable == true)
                                                {
                                                    payload.Add(targetFieldSchema, null);
                                                }
                                        }

                                            break;
                                        }
                                        else
                                        {
                                            this.logger.LogException(new Exception($"Configuration for Field Mapping{targetFieldSchema} is Incorrect"));
                                            throw new ArgumentException($"Configuration for Field Mapping{targetFieldSchema} is Incorrect");
                                        }
                                    }

                                case "DATETIME":
                                    {
                                        if (sourceValue != null && !string.IsNullOrEmpty(sourceValue.Value<JToken>().ToString()))
                                        {
                                            payload.Add(targetFieldSchema, Convert.ToDateTime(sourceValue.Value<JToken>(), CultureInfo.InvariantCulture));
                                        }
                                        else if (isOverwritable != null && isOverwritable == true)
                                        {
                                            payload.Add(targetFieldSchema, null);
                                        }

                                        break;
                                    }

                                case "OPTIONSET":
                                    {
                                        if (sourceValue != null && !string.IsNullOrEmpty(sourceValue.Value<JToken>().ToString()) && optionSetMapping.HasValues)
                                        {
                                            var optiosetResult = this.OptionSetFromTarget(optionSetMapping.ToObject<JObject>(), sourceValue.Value<JToken>().ToString());
                                            if (optiosetResult.Item1)
                                            {
                                                payload.Add(targetFieldSchema, Convert.ToInt32(optiosetResult.value, CultureInfo.InvariantCulture));
                                            }
                                        }
                                        else if (isOverwritable != null && isOverwritable == true)
                                        {
                                            payload.Add(targetFieldSchema, null);
                                        }

                                        break;
                                    }

                                case "BOOLEAN":
                                    {
                                        if (sourceValue != null && !string.IsNullOrEmpty(sourceValue.ToString()))
                                        {
                                            switch (sourceValue.ToString().ToLower())
                                            {
                                                case "y":
                                                    {
                                                        payload.Add(targetFieldSchema, true);
                                                        break;
                                                    }

                                                case "n":
                                                    {
                                                        payload.Add(targetFieldSchema, false);
                                                        break;
                                                    }

                                                default:
                                                    {
                                                        if (isOverwritable != null && isOverwritable == true)
                                                        {
                                                            payload.Add(targetFieldSchema, null);
                                                        }

                                                        break;
                                                    }
                                            }
                                        }
                                        else if (isOverwritable != null && isOverwritable == true)
                                        {
                                            payload.Add(targetFieldSchema, null);
                                        }

                                        break;
                                    }

                                case "DATE":
                                    {
                                        if (sourceValue != null && !string.IsNullOrEmpty(sourceValue.Value<JToken>().ToString()))
                                        {
                                            payload.Add(targetFieldSchema, Convert.ToDateTime(sourceValue.Value<JToken>(), CultureInfo.InvariantCulture).Date);
                                        }
                                        else if (isOverwritable != null && isOverwritable == true)
                                        {
                                            payload.Add(targetFieldSchema, null);
                                        }

                                        break;
                                    }

                                case "TIME":
                                    {
                                        if (sourceValue != null && !string.IsNullOrEmpty(sourceValue.Value<JToken>().ToString()))
                                        {
                                            payload.Add(targetFieldSchema, Convert.ToDateTime(sourceValue.Value<JToken>(), CultureInfo.InvariantCulture).ToString("HH:mm", CultureInfo.InvariantCulture));
                                        }
                                        else if (isOverwritable != null && isOverwritable == true)
                                        {
                                            payload.Add(targetFieldSchema, null);
                                        }

                                        break;
                                    }

                                default: break;
                            }
                    }
                }

                if (payload.Count > 0)
                {
                    return payload;
                }
                else
                {
                    throw new ArgumentException($"Payload formation didnt happend for the {fieldMappings}");
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException($" Set Field Mapping For Payload Failed with Error Message - " + ex.Message);
            }
        }

        /// <summary>
        /// ValidationSetFieldMapping - Validate Field Mapping.
        /// </summary>
        /// <param name="currentPayload">Current Payload to validate.</param>
        /// <param name="entity">Entity.</param>
        /// <param name="retrieveList">Retrieve List of Master data.</param>
        /// <param name="payloadTag">Pay Load Tag.</param>
        public void ValidateSetFieldMapping(JObject? currentPayload, JToken entity, Dictionary<string, JArray?> retrieveList, string? payloadTag = null)
        {
            try
            {
                foreach (JObject fieldMapping in entity.SelectToken("fieldMappings").Values<JObject>())
                {
                    var sourceFieldSchema = DynamicsApiHelper.GetStringValueFromJObject(fieldMapping, Constants.SourceFieldSchema);
                    var targetFieldSchema = DynamicsApiHelper.GetStringValueFromJObject(fieldMapping, Constants.TargetFieldSchema);
                    var targetFieldDataType = DynamicsApiHelper.GetStringValueFromJObject(fieldMapping, Constants.TargetFieldDataType).ToUpper();
                    var targetLookupFieldValue = DynamicsApiHelper.GetStringValueFromJObject(fieldMapping, Constants.TargetLookupFieldValue);
                    var targetLookupEntity = DynamicsApiHelper.GetStringValueFromJObject(fieldMapping, Constants.TargetLookupEntity);
                    var targetAlternateKeys = DynamicsApiHelper.GetStringValueFromJObject(fieldMapping, Constants.TargetAlternateKeys);
                    fieldMapping.TryGetValue(Constants.OptionSetMapping, StringComparison.OrdinalIgnoreCase, out JToken optionSetMapping);
                    bool? isRequired = DynamicsApiHelper.GetBoolValueFromJObject(fieldMapping, Constants.IsRequired);

                    // Validation for required fields
                    if (targetFieldSchema != null && targetFieldDataType != null)
                    {
                        if (isRequired != null && isRequired == true && fieldMapping.ContainsKey(Constants.IsRequired) && fieldMapping.GetValue(Constants.IsRequired).Value<bool>()
                            && fieldMapping.ContainsKey(Constants.SourceFieldSchema))
                        {
                            if ((sourceFieldSchema.Split(",").Length > 1 && targetAlternateKeys.Split(",").Length > 1)
                                   && sourceFieldSchema.Split(",").Length == targetAlternateKeys.Split(",").Length)
                            {
                                for (var i = 0; i < sourceFieldSchema.Split(",").Length; i++)
                                {
                                    var sourceValue = currentPayload.SelectToken(sourceFieldSchema.Split(",")[i]) != null ? currentPayload.SelectToken(sourceFieldSchema.Split(",")[i]).Value<JToken>() : string.Empty;
                                    if (string.IsNullOrEmpty(sourceValue.ToString()))
                                    {
                                        this.logger.LogException(new Exception($"Value {sourceValue} for Required Field from configuration is missing in the payload: - {payloadTag} {sourceFieldSchema.Split(",")[i]}"));
                                        throw new ArgumentException($"Value {sourceValue} for Required Field from configuration is missing in the payload: - {payloadTag} {sourceFieldSchema.Split(",")[i]}");
                                    }
                                }
                            }
                            else if (currentPayload.SelectToken(sourceFieldSchema) == null || currentPayload.SelectToken(sourceFieldSchema).ToString() == string.Empty)
                            {
                                this.logger.LogException(new Exception($"Value for Required Field from configuration is missing in the payload:-{payloadTag} {sourceFieldSchema}"));
                                throw new ArgumentException($"Value for Required Field from configuration is missing in the payload:-{payloadTag} {sourceFieldSchema}");
                            }
                        }

                        // Jarvis master data validation
                        switch (targetFieldDataType)
                        {
                            case "LOOKUP":
                                {
                                    var lookupErrorFlag = false;
                                    if (targetAlternateKeys != null && targetLookupEntity != null)
                                    {
                                        if ((sourceFieldSchema.Split(",").Length > 1 && targetAlternateKeys.Split(",").Length > 1)
                                           && sourceFieldSchema.Split(",").Length == targetAlternateKeys.Split(",").Length)
                                        {
                                            for (var i = 0; i < sourceFieldSchema.Split(",").Length; i++)
                                            {
                                                var sourceValue = currentPayload.SelectToken(sourceFieldSchema.Split(",")[i]) != null ? currentPayload.SelectToken(sourceFieldSchema.Split(",")[i]).Value<JToken>() : string.Empty;
                                                if (string.IsNullOrEmpty(sourceValue.ToString()))
                                                {
                                                    lookupErrorFlag = true;
                                                    break;
                                                }
                                            }

                                            if (!lookupErrorFlag)
                                            {
                                                var sourceValue = currentPayload.SelectToken(sourceFieldSchema.Split(",")[0])?.Value<JToken>();
                                                var lookupResult = this.LookupValueFromTarget(retrieveList, currentPayload, sourceFieldSchema, targetLookupEntity, targetAlternateKeys, targetLookupFieldValue);
                                                if (!lookupResult.Item1)
                                                {
                                                    this.logger.LogException(new Exception($"Lookup Value combination for {sourceValue} is not present in Jarvis:- {payloadTag} {targetLookupEntity} - {sourceFieldSchema}"));
                                                    throw new ArgumentException($"Lookup Value combination for {sourceValue} is not present in Jarvis:-{payloadTag}  {targetLookupEntity} - {sourceFieldSchema}");
                                                }
                                            }
                                        }
                                        else
                                        {
                                            var sourceValue = currentPayload.SelectToken(sourceFieldSchema.Split(",")[0])?.Value<JToken>();
                                            if (sourceValue != null && !string.IsNullOrEmpty(sourceValue.ToString()))
                                            {
                                                var lookupResult = this.LookupValueFromTarget(retrieveList, currentPayload, sourceFieldSchema, targetLookupEntity, targetAlternateKeys, targetLookupFieldValue);
                                                if (!lookupResult.Item1)
                                                {
                                                    this.logger.LogException(new Exception($"Lookup Value {sourceValue} is not present in Jarvis:- {payloadTag} {targetLookupEntity} - {sourceFieldSchema.Split(",")[0]}"));
                                                    throw new ArgumentException($"Lookup Value {sourceValue} is not present in Jarvis:-{payloadTag}  {targetLookupEntity} - {sourceFieldSchema.Split(",")[0]}");
                                                }
                                            }
                                        }
                                    }

                                    break;
                                }

                            case "OPTIONSET":
                                {
                                    var optionSetValue = currentPayload.GetValue(fieldMapping.GetValue(Constants.SourceFieldSchema).Value<string>())?.ToString();
                                    if (!string.IsNullOrEmpty(optionSetValue) && optionSetMapping.HasValues)
                                    {
                                        var optiosetResult = this.OptionSetFromTarget(optionSetMapping.ToObject<JObject>(), optionSetValue);
                                        if (!optiosetResult.Item1)
                                        {
                                            this.logger.LogException(new Exception($"Optionset Value {optionSetValue} is not present in Jarvis: - {payloadTag} {targetLookupEntity} - {sourceFieldSchema}"));
                                            throw new ArgumentException($"Optionset Value {optionSetValue} is not present in Jarvis: - {payloadTag} {targetLookupEntity} - {sourceFieldSchema}");
                                        }
                                    }

                                    break;
                                }

                            case "BOOLEAN":
                                {
                                    var sourceValue = currentPayload.SelectToken(sourceFieldSchema)?.Value<JToken>();
                                    if (sourceValue != null && !string.IsNullOrEmpty(sourceValue.ToString()))
                                    {
                                        switch (sourceValue.ToString().ToLower())
                                        {
                                            case "y": break;
                                            case "n": break;
                                            default:
                                                {
                                                    this.logger.LogException(new Exception($"Boolean Value {sourceValue} is not valid: - {sourceFieldSchema}"));
                                                    throw new ArgumentException($"Boolean Value {sourceValue} is not valid: - {sourceFieldSchema}");
                                                }
                                        }
                                    }

                                    break;
                                }
                        }
                    }
                    else
                    {
                        throw new ArgumentException($"Payload formation didnt happend for the {entity}");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException($" Validation Failed with Error Message - " + ex.Message);
            }
        }

        /// <summary>
        /// ValidateSetFieldMappingWithOverWritable - Validate Field Mapping for VDA Vehicle.
        /// </summary>
        /// <param name="currentPayload">Current Payload to validate.</param>
        /// <param name="entity">Entity.</param>
        /// <param name="retrieveList">Retrieve List of Master data.</param>
        /// <param name="payloadTag">Pay Load Tag.</param>
        public void ValidateSetFieldMappingWithOverWritable(JObject currentPayload, JToken entity, Dictionary<string, JArray?> retrieveList, string? payloadTag = null)
        {
            try
            {
                foreach (JObject fieldMapping in entity.SelectToken("fieldMappings").Values<JObject>())
                {
                    var sourceFieldSchema = DynamicsApiHelper.GetStringValueFromJObject(fieldMapping, Constants.SourceFieldSchema);
                    var targetFieldSchema = DynamicsApiHelper.GetStringValueFromJObject(fieldMapping, Constants.TargetFieldSchema);
                    var targetFieldDataType = DynamicsApiHelper.GetStringValueFromJObject(fieldMapping, Constants.TargetFieldDataType).ToUpper();
                    var targetLookupFieldValue = DynamicsApiHelper.GetStringValueFromJObject(fieldMapping, Constants.TargetLookupFieldValue);
                    var targetLookupEntity = DynamicsApiHelper.GetStringValueFromJObject(fieldMapping, Constants.TargetLookupEntity);
                    var targetAlternateKeys = DynamicsApiHelper.GetStringValueFromJObject(fieldMapping, Constants.TargetAlternateKeys);
                    fieldMapping.TryGetValue(Constants.OptionSetMapping, StringComparison.OrdinalIgnoreCase, out JToken optionSetMapping);
                    bool? isRequired = DynamicsApiHelper.GetBoolValueFromJObject(fieldMapping, Constants.IsRequired);
                    bool? isOverwritable = DynamicsApiHelper.GetBoolValueFromJObject(fieldMapping, Constants.IsOverwritable);

                    // Validation for required fields
                    if (targetFieldSchema != null && targetFieldDataType != null)
                    {
                        if (isRequired != null && isRequired == true && fieldMapping.ContainsKey(Constants.IsRequired) && fieldMapping.GetValue(Constants.IsRequired).Value<bool>()
                            && fieldMapping.ContainsKey(Constants.SourceFieldSchema))
                        {
                            if ((sourceFieldSchema.Split(",").Length > 1 && targetAlternateKeys.Split(",").Length > 1)
                                   && sourceFieldSchema.Split(",").Length == targetAlternateKeys.Split(",").Length)
                            {
                                for (var i = 0; i < sourceFieldSchema.Split(",").Length; i++)
                                {
                                    var sourceValue = currentPayload.SelectToken(sourceFieldSchema.Split(",")[i]) != null ? currentPayload.SelectToken(sourceFieldSchema.Split(",")[i]).Value<JToken>() : string.Empty;
                                    if (string.IsNullOrEmpty(sourceValue.ToString()))
                                    {
                                        this.logger.LogException(new Exception($"Value {sourceValue} for Required Field from configuration is missing in the payload: - {payloadTag} {sourceFieldSchema.Split(",")[i]}"));
                                        throw new ArgumentException($"Value {sourceValue} for Required Field from configuration is missing in the payload: - {payloadTag} {sourceFieldSchema.Split(",")[i]}");
                                    }
                                }
                            }
                            else if (currentPayload.SelectToken(sourceFieldSchema) == null || currentPayload.SelectToken(sourceFieldSchema).ToString() == string.Empty)
                            {
                                this.logger.LogException(new Exception($"Value for Required Field from configuration is missing in the payload:-{payloadTag} {sourceFieldSchema}"));
                                throw new ArgumentException($"Value for Required Field from configuration is missing in the payload:-{payloadTag} {sourceFieldSchema}");
                            }
                        }

                        // Jarvis master data validation
                        switch (targetFieldDataType)
                        {
                            case "LOOKUP":
                                {
                                    var lookupErrorFlag = false;
                                    if (targetAlternateKeys != null && targetLookupEntity != null)
                                    {
                                        if ((sourceFieldSchema.Split(",").Length > 1 && targetAlternateKeys.Split(",").Length > 1)
                                           && sourceFieldSchema.Split(",").Length == targetAlternateKeys.Split(",").Length && (isOverwritable == null || isOverwritable == false))
                                        {
                                            for (var i = 0; i < sourceFieldSchema.Split(",").Length; i++)
                                            {
                                                var sourceValue = currentPayload.SelectToken(sourceFieldSchema.Split(",")[i]) != null ? currentPayload.SelectToken(sourceFieldSchema.Split(",")[i]).Value<JToken>() : string.Empty;
                                                if (string.IsNullOrEmpty(sourceValue.ToString()))
                                                {
                                                    lookupErrorFlag = true;
                                                    break;
                                                }
                                            }

                                            if (!lookupErrorFlag)
                                            {
                                                var sourceValue = currentPayload.SelectToken(sourceFieldSchema.Split(",")[0])?.Value<JToken>();
                                                var lookupResult = this.LookupValueFromTarget(retrieveList, currentPayload, sourceFieldSchema, targetLookupEntity, targetAlternateKeys, targetLookupFieldValue);
                                                if (!lookupResult.Item1)
                                                {
                                                    this.logger.LogException(new Exception($"Lookup Value combination for {sourceValue} is not present in Jarvis:- {payloadTag} {targetLookupEntity} - {sourceFieldSchema}"));
                                                    throw new ArgumentException($"Lookup Value combination for {sourceValue} is not present in Jarvis:-{payloadTag}  {targetLookupEntity} - {sourceFieldSchema}");
                                                }
                                            }
                                        }
                                        else
                                        {
                                            var sourceValue = currentPayload.SelectToken(sourceFieldSchema.Split(",")[0])?.Value<JToken>();
                                            if (sourceValue != null && !string.IsNullOrEmpty(sourceValue.ToString()) && (isOverwritable == null || isOverwritable == false))
                                            {
                                                var lookupResult = this.LookupValueFromTarget(retrieveList, currentPayload, sourceFieldSchema, targetLookupEntity, targetAlternateKeys, targetLookupFieldValue);
                                                if (!lookupResult.Item1)
                                                {
                                                    this.logger.LogException(new Exception($"Lookup Value {sourceValue} is not present in Jarvis:- {payloadTag} {targetLookupEntity} - {sourceFieldSchema.Split(",")[0]}"));
                                                    throw new ArgumentException($"Lookup Value {sourceValue} is not present in Jarvis:-{payloadTag}  {targetLookupEntity} - {sourceFieldSchema.Split(",")[0]}");
                                                }
                                            }
                                        }
                                    }

                                    break;
                                }

                            case "OPTIONSET":
                                {
                                    var optionSetValue = currentPayload.GetValue(fieldMapping.GetValue(Constants.SourceFieldSchema).Value<string>())?.ToString();
                                    if (!string.IsNullOrEmpty(optionSetValue) && optionSetMapping.HasValues && (isOverwritable == null || isOverwritable == false))
                                    {
                                        var optiosetResult = this.OptionSetFromTarget(optionSetMapping.ToObject<JObject>(), optionSetValue);
                                        if (!optiosetResult.Item1)
                                        {
                                            this.logger.LogException(new Exception($"Optionset Value {optionSetValue} is not present in Jarvis: - {payloadTag} {targetLookupEntity} - {sourceFieldSchema}"));
                                            throw new ArgumentException($"Optionset Value {optionSetValue} is not present in Jarvis: - {payloadTag} {targetLookupEntity} - {sourceFieldSchema}");
                                        }
                                    }

                                    break;
                                }

                            case "BOOLEAN":
                                {
                                    var sourceValue = currentPayload.SelectToken(sourceFieldSchema)?.Value<JToken>();
                                    if (sourceValue != null && !string.IsNullOrEmpty(sourceValue.ToString()) && (isOverwritable == null || isOverwritable == false))
                                    {
                                        switch (sourceValue.ToString().ToLower())
                                        {
                                            case "y": break;
                                            case "n": break;
                                            default:
                                                {
                                                    this.logger.LogException(new Exception($"Boolean Value {sourceValue} is not valid: - {sourceFieldSchema}"));
                                                    throw new ArgumentException($"Boolean Value {sourceValue} is not valid: - {sourceFieldSchema}");
                                                }
                                        }
                                    }

                                    break;
                                }
                        }
                    }
                    else
                    {
                        throw new ArgumentException($"Payload formation didnt happend for the {entity}");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException($" Validation Failed with Error Message - " + ex.Message);
            }
        }

        /// <summary>
        /// OptionSetFromTarget - Return option set value and true for field found in Jarvis else return false.
        /// </summary>
        /// <param name="optionsetMapping">Option Set Mapping.</param>
        /// <param name="optionsetValue">Option set Value.</param>
        /// <returns>Optionset Id.</returns>
        /// <exception cref="ArgumentException">Argument Exception.</exception>
        public (bool, JToken? value) OptionSetFromTarget(JObject? optionsetMapping, string optionsetValue)
        {
            try
            {
                optionsetMapping.ToObject<JObject>().TryGetValue(optionsetValue, StringComparison.OrdinalIgnoreCase, out JToken value);
                if (value != null)
                {
                    return (true, value);
                }

                return (false, null);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($" Validating Option Set From Target Failed with Error Message - " + ex.Message);
            }
        }

        /// <summary>
        /// Set Field Maping for Outbound to frame Payload.
        /// </summary>
        /// <param name="currentPayload">Current Payload.</param>
        /// <param name="fieldMappings">Field Mapping.</param>
        /// <param name="retrieveList">Retrieve List.</param>
        /// <returns>Payload Jobject.</returns>
        /// <exception cref="ArgumentException">Argument Exception.</exception>
        public JObject SetFieldMappingOutbound(JObject? currentPayload, JToken fieldMappings, Dictionary<string, JArray?> retrieveList)
        {
            JObject payload = new JObject();

            foreach (JObject fieldMapping in fieldMappings.Values<JObject>())
            {
                var sourceFieldSchema = DynamicsApiHelper.GetStringValueFromJObject(fieldMapping, "sourceFieldSchema");
                var targetFieldSchema = DynamicsApiHelper.GetStringValueFromJObject(fieldMapping, "targetFieldSchema");
                var sourceFieldDataType = DynamicsApiHelper.GetStringValueFromJObject(fieldMapping, "sourceFieldDataType").ToUpper();
                var sourceLookupEntity = DynamicsApiHelper.GetStringValueFromJObject(fieldMapping, "sourceLookupEntity");
                var sourceLookupKeys = DynamicsApiHelper.GetStringValueFromJObject(fieldMapping, "sourceLookupKeys");
                var sourceLookupField = DynamicsApiHelper.GetStringValueFromJObject(fieldMapping, "sourceLookupField");
                var defaultValue = DynamicsApiHelper.GetStringValueFromJObject(fieldMapping, "DefaultValue");
                fieldMapping.TryGetValue("OptionSetMapping", StringComparison.OrdinalIgnoreCase, out JToken optionSetMapping);

                if ((!string.IsNullOrEmpty(sourceFieldSchema) && !string.IsNullOrEmpty(targetFieldSchema) && !string.IsNullOrEmpty(sourceFieldDataType)) || fieldMapping.ContainsKey("DefaultValue"))
                {
                    var sourceValue = currentPayload.SelectToken(sourceFieldSchema);
                    if ((sourceValue != null) || (!string.IsNullOrEmpty(sourceLookupEntity)) || fieldMapping.ContainsKey("DefaultValue"))
                    {
                        switch (sourceFieldDataType)
                        {
                            case "TEXT":
                                payload.Add(targetFieldSchema.ToString(), Convert.ToString(sourceValue.Value<JToken>(), CultureInfo.InvariantCulture));
                                break;
                            case "DECIMAL":
                                {
                                    if (!string.IsNullOrEmpty(sourceValue.Value<JToken>().ToString()))
                                    {
                                        payload.Add(targetFieldSchema, Convert.ToDecimal(sourceValue.Value<JToken>(), CultureInfo.InvariantCulture));
                                    }
                                    else
                                    {
                                        payload.Add(targetFieldSchema, null);
                                    }

                                    break;
                                }

                            case "DATETIME":
                                {
                                    if (!string.IsNullOrEmpty(sourceValue.Value<JToken>().ToString()))
                                    {
                                        payload.Add(targetFieldSchema, Convert.ToDateTime(sourceValue.Value<JToken>(), CultureInfo.InvariantCulture).ToString("MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture));
                                    }
                                    else
                                    {
                                        payload.Add(targetFieldSchema, null);
                                    }

                                    break;
                                }

                            case "DATE":
                                {
                                    if (!string.IsNullOrEmpty(sourceValue.Value<JToken>().ToString()))
                                    {
                                        payload.Add(targetFieldSchema, Convert.ToDateTime(sourceValue.Value<JToken>(), CultureInfo.InvariantCulture).ToString("MM/dd/yyyy", CultureInfo.InvariantCulture));
                                    }
                                    else
                                    {
                                        payload.Add(targetFieldSchema, null);
                                    }

                                    break;
                                }

                            case "TIME":
                                {
                                    if (!string.IsNullOrEmpty(sourceValue.Value<JToken>().ToString()))
                                    {
                                        payload.Add(targetFieldSchema, Convert.ToDateTime(sourceValue.Value<JToken>(), CultureInfo.InvariantCulture).ToString("HH:mm:ss", CultureInfo.InvariantCulture));
                                    }
                                    else
                                    {
                                        payload.Add(targetFieldSchema, "00:00:00");
                                    }

                                    break;
                                }

                            case "OPTIONSET":
                                {
                                    if (!string.IsNullOrEmpty(sourceValue.Value<JToken>().ToString()) && optionSetMapping.HasValues)
                                    {
                                        var optiosetResult = this.OptionSetFromTarget(optionSetMapping.ToObject<JObject>(), sourceValue.Value<JToken>().ToString());
                                        if (optiosetResult.Item1)
                                        {
                                            payload.Add(targetFieldSchema, Convert.ToString(optiosetResult.value, CultureInfo.InvariantCulture));
                                        }
                                    }
                                    else
                                    {
                                        payload.Add(targetFieldSchema, null);
                                    }

                                    break;
                                }

                            case "LOOKUP":
                                {
                                    if (sourceLookupEntity != null && sourceLookupKeys != null && sourceLookupField != null)
                                    {
                                        if (sourceValue != null && sourceValue.ToString() != string.Empty)
                                        {
                                            var lookupResult = this.LookupValueFromTarget(retrieveList, currentPayload, sourceFieldSchema, sourceLookupEntity, sourceLookupKeys, sourceLookupField);
                                            if (lookupResult.Item1)
                                            {
                                                payload.Add(targetFieldSchema, Convert.ToString(lookupResult.value, CultureInfo.InvariantCulture));
                                            }
                                            else
                                            {
                                                payload.Add(targetFieldSchema, null);
                                            }
                                        }
                                        else
                                        {
                                            payload.Add(targetFieldSchema, null);
                                        }

                                        break;
                                    }
                                    else
                                    {
                                        this.logger.LogException(new Exception($"Configuration for Field Mapping{targetFieldSchema} is Incorrect"));
                                        throw new ArgumentException($"Configuration for Field Mapping{targetFieldSchema} is Incorrect");
                                    }
                                }

                            case "DEFAULT":
                                {
                                    payload.Add(targetFieldSchema, defaultValue);
                                    break;
                                }

                            default: break;
                        }
                    }
                }
            }

            return payload;
        }

        /// <summary>
        /// Validate Field Mapping for outbound before framing payload.
        /// </summary>
        /// <param name="currentPayload">Current Payload.</param>
        /// <param name="fieldMappings">Field Mapping.</param>
        /// <exception cref="ArgumentException">Argument Exception.</exception>
        public void ValidateSetFieldMappingOutbound(JObject? currentPayload, JToken fieldMappings)
        {
            foreach (JObject fieldMapping in fieldMappings.Values<JObject>())
            {
                var sourceFieldSchema = DynamicsApiHelper.GetStringValueFromJObject(fieldMapping, "sourceFieldSchema");
                var targetFieldSchema = DynamicsApiHelper.GetStringValueFromJObject(fieldMapping, "targetFieldSchema");
                var sourceFieldDataType = DynamicsApiHelper.GetStringValueFromJObject(fieldMapping, "sourceFieldDataType").ToUpper();
                bool? isRequired = DynamicsApiHelper.GetBoolValueFromJObject(fieldMapping, "isRequired");
                fieldMapping.TryGetValue("OptionSetMapping", StringComparison.OrdinalIgnoreCase, out JToken optionSetMapping);

                // DefaultValue Not required for Validation.
                if (fieldMapping.ContainsKey("DefaultValue"))
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(sourceFieldSchema) && !string.IsNullOrEmpty(targetFieldSchema) && !string.IsNullOrEmpty(sourceFieldDataType))
                {
                    if (isRequired != null && isRequired == true && fieldMapping.ContainsKey("isRequired") && fieldMapping.GetValue("isRequired").Value<bool>()
                                        && fieldMapping.ContainsKey("sourceFieldSchema") && (currentPayload.SelectToken(sourceFieldSchema) == null || currentPayload.SelectToken(sourceFieldSchema).ToString() == string.Empty))
                    {
                        this.logger.LogTrace($"Value for Required Field from configuraiton is missing in the paylaod :{sourceFieldSchema}");
                        //throw new ArgumentException($"Value for Required Field from configuraiton is missing in the paylaod :{sourceFieldSchema}");
                    }

                    if (sourceFieldDataType == "OPTIONSET")
                    {
                        var optionSetValue = currentPayload.SelectToken(fieldMapping.GetValue("sourceFieldSchema").Value<string>()).ToString();
                        if (!string.IsNullOrEmpty(optionSetValue) && optionSetMapping.HasValues)
                        {
                            var optiosetResult = this.OptionSetFromTarget(optionSetMapping.ToObject<JObject>(), optionSetValue);
                            if (!optiosetResult.Item1)
                            {
                                this.logger.LogTrace($"Optionset Value {optionSetValue} is not present in Jarvis for {targetFieldSchema} - {sourceFieldSchema}");
                                //throw new ArgumentException($"Optionset Value {optionSetValue} is not present in Jarvis for {targetFieldSchema} - {sourceFieldSchema}");
                            }
                        }
                    }
                }
                else
                {
                    throw new ArgumentException($"Payload formation didnt happend for the {targetFieldSchema}");
                }
            }
        }

        /// <summary>
        /// Return Key Payload for BusinessPartnerBrands.
        /// </summary>
        /// <param name="brandsMappings">Brand Mapping.</param>
        /// <param name="currentPayload">Current Payload.</param>
        /// <param name="retrieveList">retrieve List.</param>
        /// <param name="bpResponsibleUnit">Responsible Unit.</param>
        /// <param name="bpRetailCountry">Retail Country.</param>
        /// <returns>Id of the attribute.</returns>
        /// <exception cref="ArgumentException">Argument Exception.</exception>
        public (string, JObject) BrandsToBusinessPartnerBrands(JObject? brandsMappings, JObject currentPayload, Dictionary<string, JArray?> retrieveList, string? bpResponsibleUnit, string? bpRetailCountry)
        {
            try
            {
                JObject payload = new JObject();
                string bpBrandRecordKey = string.Empty;
                string? sourceEntity = DynamicsApiHelper.GetStringValueFromJObject(brandsMappings, Constants.SourceEntityName);
                string? sourceField = DynamicsApiHelper.GetStringValueFromJObject(brandsMappings, Constants.SourceFieldSchema);
                string? targetLookupEntity = DynamicsApiHelper.GetStringValueFromJObject(brandsMappings, Constants.TargetLookupEntity);
                string? targetLookupEntityName = DynamicsApiHelper.GetStringValueFromJObject(brandsMappings, Constants.TargetEntityId);
                string? targetFieldSchema = DynamicsApiHelper.GetStringValueFromJObject(brandsMappings, Constants.TargetLookupFieldSchema);
                string? targetRelationship = DynamicsApiHelper.GetStringValueFromJObject(brandsMappings, Constants.TargetRelationshipName);
                bool? isRequired = DynamicsApiHelper.GetBoolValueFromJObject(brandsMappings, Constants.IsRequired);

                if (!string.IsNullOrEmpty(sourceEntity) && !string.IsNullOrEmpty(sourceField) && !string.IsNullOrEmpty(targetLookupEntity) && !string.IsNullOrEmpty(targetLookupEntityName) && !string.IsNullOrEmpty(targetFieldSchema) && !string.IsNullOrEmpty(targetRelationship))
                {
                    var sourceValue = currentPayload.SelectToken(sourceField);
                    if (isRequired.HasValue && isRequired.Value && sourceValue != null && sourceValue.ToString() != string.Empty)
                    {
                        var retrieveAccounts = retrieveList.First(item => item.Key.ToUpper().Contains(sourceEntity.ToUpper()));
                        brandsMappings.TryGetValue(Constants.JarvisFieldMappings, out JToken fieldMapping);
                        if (retrieveAccounts.Key != null && fieldMapping != null)
                        {
                            var accountPayload = retrieveAccounts.Value.FirstOrDefault(item => item[Constants.JarvisResponsableunitid] != null && item[Constants.JarvisResponsableunitid].ToString().ToUpper() == bpResponsibleUnit.ToString().ToUpper() && item[Constants.JarvisRetailcountryid] != null && item[Constants.JarvisRetailcountryid].ToString().ToUpper() == bpRetailCountry.ToString().ToUpper());
                            if (accountPayload != null && accountPayload.HasValues)
                            {
                                string accountId = accountPayload.SelectToken(Constants.AccountID).ToString();
                                string accountNumber = accountPayload.SelectToken(Constants.Accountnumber).ToString();
                                Dictionary<string, JArray> retrieveAccount = new ()
                                {
                                    { Constants.Accounts, new JArray(accountPayload.ToObject<JObject>()) },
                                };
                                JToken? bpBrandsList = this.SubgridValueFromTarget(retrieveAccount, sourceEntity, targetRelationship);

                                if (bpBrandsList != null && bpBrandsList.Any())
                                {
                                    retrieveList.Add(Constants.JarvisBusinesspartnerbrandses, bpBrandsList.Value<JArray>());

                                    var bpBrandRecord = this.LookupValueBpBrandFromTarget(retrieveList, currentPayload, sourceField, targetLookupEntity, targetFieldSchema, targetLookupEntityName);
                                    if (bpBrandRecord.Item1 && bpBrandRecord.value != null)
                                    {
                                        bpBrandRecordKey = bpBrandRecord.value.ToString();
                                        payload.Add(CtdiConstants.JarvisStateCode, 0);

                                        return (bpBrandRecordKey, payload);
                                    }
                                }

                                bpBrandRecordKey = Guid.NewGuid().ToString();
                                this.ValidateSetFieldMapping(currentPayload, brandsMappings, retrieveList);
                                payload = this.SetFieldMapping(currentPayload, fieldMapping, retrieveList);
                                payload.Add(CtdiConstants.JarvisBusinessPartnerId, accountNumber);
                                JToken accountToken = string.Format("/{0}({1})", sourceEntity, accountId);
                                payload.Add(CtdiConstants.JarvisBusinessPartnerOdataBind, accountToken);
                                JToken? token = this.SetOwnerTeam(retrieveList, brandsMappings);
                                if (token != null)
                                {
                                    payload.Add(CtdiConstants.OwnerId, token);
                                }
                            }
                        }
                    }
                    else
                    {
                        this.logger.LogException(new Exception($"Value for Required Field from configuration is missing in the payload:-{sourceField}"));
                        throw new ArgumentException($"Value for Required Field from configuration is missing in the payload:-{sourceField}");
                    }
                }
                else
                {
                    this.logger.LogException(new Exception($"CTDI Dealers Configuration is not perfect"));
                    throw new ArgumentException($"CTDI Dealers Configuration is not perfect");
                }

                return (bpBrandRecordKey, payload);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"BrandsToBusinessPartnerBrands:" + ex.Message);
            }
        }

        /// <summary>
        /// Retrieve lookup Business Partner Brands from Target.
        /// </summary>
        /// <param name="retrieveList">Retrieve List of Master data.</param>
        /// <param name="sourcePayload">Source Payload.</param>
        /// <param name="sourceField">Source Field.</param>
        /// <param name="targetLookupEntity">Target Lookup Entity.</param>
        /// <param name="targetFieldSchema">Target Field Schema.</param>
        /// <param name="targetEntityName">Target Entity Name.</param>
        /// <returns>JToken id of the attribute.</returns>
        /// <exception cref="ArgumentException">Argument Exception.</exception>
        public (bool, JToken? value) LookupValueBpBrandFromTarget(Dictionary<string, JArray> retrieveList, JObject sourcePayload, string sourceField, string targetLookupEntity, string targetFieldSchema, string targetEntityName)
        {
            try
            {
                var matchMasterRecord = retrieveList.First(item => item.Key.ToUpper().Contains(targetLookupEntity.ToUpper()));

                if (!string.IsNullOrEmpty(sourceField) && !string.IsNullOrEmpty(targetFieldSchema) && !string.IsNullOrEmpty(targetEntityName) && matchMasterRecord.Key != null)
                {
                    var matachingrecord = matchMasterRecord.Value.ToList();

                    var sourceValue = sourcePayload.SelectToken(sourceField);
                    if (!string.IsNullOrEmpty(sourceValue.ToString()) && matachingrecord != null && matachingrecord.Count > 0)
                    {
                        var matchRecord = matachingrecord.Where(item => item.SelectToken(targetFieldSchema).ToString().ToUpper() == sourceValue.ToString().ToUpper()).ToList();

                        if (matchRecord != null && matchRecord.Count > 0)
                        {
                            JToken? jToken = matchRecord.Select(item => item[targetEntityName.ToLower()]).First();
                            return (true, jToken);
                        }
                    }
                }
                else
                {
                    this.logger.LogException(new ArgumentException("Not a Valid Configuration to match and retrive value from source"));
                    throw new ArgumentException($"Not a Valid Configuration to match and retrive value from source");
                }

                return (false, string.Empty);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($" Retrieving Lookup Value from Jarvis entity {targetLookupEntity} Failed with Error Message - " + ex.Message);
            }
        }

        /// <summary>
        /// Set Owner Team Based on Mapping from Integration Configuration.
        /// </summary>
        /// <param name="retrieveList">Retrieve List of Master Data.</param>
        /// <param name="mappings">Mapping.</param>
        /// <returns>JToken Id of the attribute.</returns>
        /// <exception cref="ArgumentException">Argument Exception.</exception>
        public JToken? SetOwnerTeam(Dictionary<string, JArray> retrieveList, JObject mappings)
        {
            try
            {
                // retreive team from jarvis for framing owner
                JToken? jarvisTeams = retrieveList.First(item => item.Key.ToUpper() == Constants.Teams.ToUpper()).Value;
                if (jarvisTeams != null)
                {
                    string teamJarvis = DynamicsApiHelper.GetStringValueFromJObject(mappings, CtdiConstants.OwnerTeam);
                    var matachingRecordTeams = jarvisTeams.ToList();
                    var matchRecord = matachingRecordTeams.Where(item => item[CtdiConstants.Name].ToString() == teamJarvis.ToString());
                    JToken? teamIdJarvis = matchRecord.Select(item => item[CtdiConstants.TeamId]).First();
                    JToken token = string.Format("/{0}({1})", CtdiConstants.Teams, teamIdJarvis.ToString());
                    return token;
                }
                else
                {
                    throw new ArgumentException($"Team Configuration is Missing Please Update Integration Processs Code.");
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException($" Set Owner Team Failed with Error Message - " + ex.Message);
            }
        }

        /// <summary>
        /// Converting Plugin ImageContext into a Entity Object.
        /// </summary>
        /// <param name="entityData">Entity Data.</param>
        /// <returns>Jobject for the entity.</returns>
        /// <exception cref="ArgumentException">Argument Exception.</exception>
        public JObject ConvertContextToObject(string entityData)
        {
            try
            {
                JArray entityDataArray = JArray.Parse(entityData);
                JObject keyValue = new JObject();
                foreach (var entity in entityDataArray)
                {
                    JObject values = entity.ToObject<JObject>();
                    if (values.HasValues)
                    {
                        var value = values.SelectToken("Value");
                        var key = values.SelectToken("Key");
                        switch (values.SelectToken("Value").Type.ToString().ToUpper())
                        {
                            case "STRING" or "BOOLEAN" or "FLOAT" or "INT" or "DATE":
                                {
                                    value = values.SelectToken("Value");
                                    break;
                                }

                            case "OBJECT":
                                {
                                    if (values.SelectToken("Value.Value") != null)
                                    {
                                        value = values.SelectToken("Value.Value");
                                    }
                                    else
                                    {
                                        value = values.SelectToken("Value.Id");
                                        key = $"_{key}_value";
                                    }

                                    break;
                                }
                        }

                        keyValue.Add(key.ToString(), value);
                    }
                }

                return keyValue;
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Failed in Convert CRM Payload into Object with Error Message - " + ex.Message);
            }
        }
    }
}
