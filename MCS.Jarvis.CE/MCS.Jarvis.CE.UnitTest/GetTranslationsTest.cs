//-----------------------------------------------------------------------
// <copyright file="GetTranslationsTest.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace MCS.Jarvis.CE.UnitTest
{
    using MCS.Jarvis.CE.Plugins;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.Xrm.Sdk;

    /// <summary>
    /// GetTranslations Test.
    /// </summary>
    [TestClass]
    public class GetTranslationsTest : UnitTestBase
    {
        /// <summary>
        /// Get Translations  Plugin Should Translate Text.
        /// </summary>
        [TestMethod]
        public void GetTranslations_Plugin_Should_Translate_Text()
        {
            this.PluginExecutionContext.InputParametersGet = () => new ParameterCollection
            {
            { "languageCode", "en-US" },
            { "inputText", "This is a VOLVO truck." },
            };

            this.PluginExecutionContext.OutputParametersGet = () => new ParameterCollection();

            GetTranslations plugin = new GetTranslations();
            plugin.Execute(this.ServiceProvider);
        }
    }
}
