namespace MCS.Jarvis.Integration.Base.Dynamics
{
    using System;
    using System.Globalization;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Dynamics Helper class.
    /// </summary>
    public static class DynamicsApiHelper
    {
        /// <summary>
        /// Get text field value from entity JObject.
        /// </summary>
        /// <param name="jsonObject">jObject value.</param>
        /// <param name="key">key value.</param>
        /// <returns>Text field value.</returns>
        public static string GetStringValueFromJObject(JObject? jsonObject, string key)
        {
            string? result = null;
            JToken? jresult = null;
            if (jsonObject != null && jsonObject.TryGetValue(key, StringComparison.OrdinalIgnoreCase, out jresult) && ((JValue)jresult).Value != null)
            {
                result = Convert.ToString(jresult, CultureInfo.InvariantCulture);
            }

            return result == null ? string.Empty : result;
        }

        /// <summary>
        /// Get boolean field value from entity JObject.
        /// </summary>
        /// <param name="jsonObject">jObject value.</param>
        /// <param name="key">key value.</param>
        /// <returns>Boolean field value.</returns>
        public static bool? GetBoolValueFromJObject(JObject? jsonObject, string key)
        {
            bool? result = null;
            JToken? jresult = null;
            if (jsonObject != null && jsonObject.TryGetValue(key, StringComparison.OrdinalIgnoreCase, out jresult) && ((JValue)jresult).Value != null)
            {
                result = Convert.ToBoolean(jresult, CultureInfo.InvariantCulture);
            }

            return result;
        }

        /// <summary>
        /// Get integer field value from entity JObject.
        /// </summary>
        /// <param name="jsonObject">jObject value.</param>
        /// <param name="key">key value.</param>
        /// <returns>Integer field value.</returns>
        public static int? GetIntegerValueFromJObject(JObject jsonObject, string key)
        {
            int? result = null;
            JToken? jresult = null;
            if (jsonObject != null && jsonObject.TryGetValue(key, StringComparison.OrdinalIgnoreCase, out jresult) && ((JValue)jresult).Value != null)
            {
                result = Convert.ToInt32(jresult, CultureInfo.InvariantCulture);
            }

            return result;
        }

        /// <summary>
        /// Get decimal field value from entity JObject.
        /// </summary>
        /// <param name="jsonObject">jObject value.</param>
        /// <param name="key">key value.</param>
        /// <returns>Decimal field value.</returns>
        public static decimal? GetDecimalValueFromJObject(JObject jsonObject, string key)
        {
            decimal? result = null;
            JToken? jresult = null;
            if (jsonObject != null && jsonObject.TryGetValue(key, StringComparison.OrdinalIgnoreCase, out jresult) && ((JValue)jresult).Value != null)
            {
                result = Convert.ToDecimal(jresult, CultureInfo.InvariantCulture);
            }

            return result;
        }

        /// <summary>
        /// Get Date time field value from entity JObject.
        /// </summary>
        /// <param name="jsonObject">jObject value.</param>
        /// <param name="key">key value.</param>
        /// <returns>Decimal field value.</returns>
        public static DateTime? GetDateTimeValueFromJObject(JObject jsonObject, string key)
        {
            DateTime? result = null;
            JToken? jresult = null;
            if (jsonObject != null && jsonObject.TryGetValue(key, StringComparison.OrdinalIgnoreCase, out jresult) && ((JValue)jresult).Value != null)
            {
                DateTime temp;
                if (DateTime.TryParse(Convert.ToString(jresult, CultureInfo.InvariantCulture), out temp))
                {
                    result = (temp != DateTime.MinValue) ? (DateTime?)temp : null;
                }
            }

            return result;
        }
    }
}
