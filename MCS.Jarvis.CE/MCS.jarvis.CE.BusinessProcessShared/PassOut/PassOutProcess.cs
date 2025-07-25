// <copyright file="PassOutProcess.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace MCS.Jarvis.CE.BusinessProcessShared.PassOut
{
    using System;
    using System.Collections.Generic;
    using MCS.Jarvis.CE.BusinessProcessesShared.Helpers;
    using Microsoft.Xrm.Sdk;

    /// <summary>
    /// Pass Out Process.
    /// </summary>
    public class PassOutProcess
    {
        /// <summary>
        /// Update Pass Out Date Time Field.
        /// </summary>
        /// <param name="service">Org service.</param>
        /// <param name="passOut">pass Out.</param>
        /// <param name="passOutImg">pass Out Image.</param>
        /// <param name="timeZoneCode">time Zone Code.</param>
        /// <param name="tracingService">tracing Service.</param>
        /// <returns>pass out Entity.</returns>
        public Entity UpdatePassOutDateTimeField(IOrganizationService service, Entity passOut, Entity passOutImg, int? timeZoneCode, ITracingService tracingService)
        {
            var passoutCntrls = new List<string>() { "jarvis_eta", "jarvis_ata", "jarvis_etc", "jarvis_atc" };

            foreach (var targetField in passoutCntrls)
            {
                string passOutTimeField = string.Empty;
                DateTime passOutDateField = DateTime.MinValue;
                tracingService.Trace($"PassOut Attribute : {passOut.Attributes.Count}");
                if (passOut.Attributes.Contains(targetField + "date") || passOut.Attributes.Contains(targetField + "time"))
                {
                    tracingService.Trace($"Enter for {targetField}");

                    //// targetTimeFieldValue or preImageTimeFieldValue
                    if (passOut.Attributes.Contains(targetField + "time") && passOut.Attributes[targetField + "time"] != null)
                    {
                        passOutTimeField = (string)passOut.Attributes[targetField + "time"];
                    }
                    else if (passOutImg != null && passOutImg.Attributes.Contains(targetField + "time") && passOutImg.Attributes[targetField + "time"] != null)
                    {
                        passOutTimeField = (string)passOutImg.Attributes[targetField + "time"];
                    }

                    //// targetDateFieldValue or preImageDateFieldValue
                    if (passOut.Attributes.Contains(targetField + "date") && passOut.Attributes[targetField + "date"] != null)
                    {
                        passOutDateField = (DateTime)passOut.Attributes[targetField + "date"];
                    }
                    else if (passOutImg != null && passOutImg.Attributes.Contains(targetField + "date") && passOutImg.Attributes[targetField + "date"] != null)
                    {
                        passOutDateField = (DateTime)passOutImg.Attributes[targetField + "date"];
                    }

                    tracingService.Trace($"PassOut Date: {passOutDateField}, PassOut Time: {passOutTimeField}");
                    if (passOutDateField != DateTime.MinValue && !string.IsNullOrEmpty(passOutTimeField) && timeZoneCode != null && passOutTimeField.Length == 4)
                    {
                        tracingService.Trace($"{targetField} formate Date: {passOutDateField}, Time:{passOutTimeField}");
                        DateTime passOutDateTimeFomate = new DateTime(passOutDateField.Year, passOutDateField.Month, passOutDateField.Day, Convert.ToInt32(passOutTimeField.Substring(0, 2)), Convert.ToInt32(passOutTimeField.Substring(2, 2)), 00);
                        tracingService.Trace($"{targetField} before Conversion : {passOutDateTimeFomate}");
                        DateTime timeZoneConversion = CrmHelper.RetrieveUTCFromLocalTime(service, passOutDateTimeFomate, timeZoneCode);
                        tracingService.Trace($"{targetField} after Conversion: {timeZoneConversion}");
                        if (passOut.Attributes.Contains(targetField) && passOut.Attributes[targetField] != null && (DateTime)passOut.Attributes[targetField] != timeZoneConversion)
                        {
                            passOut.Attributes[targetField] = timeZoneConversion;
                            tracingService.Trace("Value set");
                        }
                        else if (!passOut.Attributes.Contains(targetField))
                        {
                            passOut.Attributes.Add(targetField, timeZoneConversion);
                        }
                    }
                }
            }

            return passOut;
        }
    }
}