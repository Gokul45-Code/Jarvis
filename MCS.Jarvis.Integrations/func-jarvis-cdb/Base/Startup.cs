// <copyright file="Startup.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using System;
using MCS.Jarvis.Integration.Base.Dynamics;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Func_jarvis_cdb.Base.Startup))]

namespace Func_jarvis_cdb.Base
{
    /// <summary>
    /// Startup.
    /// </summary>
    public class Startup : FunctionsStartup
    {
        /// <summary>
        /// Configure.
        /// </summary>
        /// <param name="builder">Builder.</param>
        /// <exception cref="ArgumentNullException">Argument Null Exception.</exception>
        public override void Configure(IFunctionsHostBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.Services.AddSingleton<IDynamicsApiClient, DynamicsApiClient>().AddHttpClient();
        }

        /// <summary>
        /// ConfigureAppConfiguration.
        /// </summary>
        /// <param name="builder">Builder.</param>
        /// <exception cref="ArgumentNullException">Argument Null Exception.</exception>
        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder), "Null Argument for Configuration");
            }

            builder.ConfigurationBuilder
                .SetBasePath(Environment.CurrentDirectory)
                    .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables()
                    .Build();
        }
    }
}
