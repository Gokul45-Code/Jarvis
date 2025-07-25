// <copyright file="GetTranslations.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace MCS.Jarvis.CE.Plugins
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;

    /// <summary>
    /// Get Translations.
    /// </summary>
    public class GetTranslations : IPlugin
    {
        /// <summary>
        /// Execute Method.
        /// </summary>
        /// <param name="serviceProvider">service Provider.</param>
        public void Execute(IServiceProvider serviceProvider)
        {
            // Extract the tracing service for use in debugging sandboxed plug-ins.
            ITracingService tracingService =
                (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            // Obtain the execution context from the service provider.
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            if (context.InputParameters.Contains("languageCode") && context.InputParameters["languageCode"] is string)
            {
                tracingService.Trace("Entered");
                string languageCode = (string)context.InputParameters["languageCode"];
                if (context.InputParameters.Contains("inputText") && context.InputParameters["inputText"] is string)
                {
                    string textToTranslate = (string)context.InputParameters["inputText"];
                    string outputTranslateText = (string)context.InputParameters["inputText"];
                    List<string> dictionaryWords = new List<string>();
                    dictionaryWords.Add("VOLVO");
                    dictionaryWords.Add("TRUCK");
                    dictionaryWords.Add("VBC");
                    dictionaryWords.Add("LEVEL");

                    string[] subs = textToTranslate.Split(' ');

                    foreach (string word in subs)
                    {
                        if (dictionaryWords.Contains(word.ToUpper()) == true)
                        {
                            string formatText = "<mstrans:dictionary translation=" + word + "></mstrans:dictionary>";
                            outputTranslateText = outputTranslateText.Replace(word, formatText);
                        }
                    }

                    context.OutputParameters["translatedText"] = outputTranslateText;
                }
            }
        }
    }
}
