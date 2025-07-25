// <copyright file="Startup.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using System;
using MCS.Jarvis.Integration.Base.Dynamics;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(SiebelDB_001_Jarvis.Base.Startup))]

namespace SiebelDB_001_Jarvis.Base
{
    /// <summary>
    /// Case Startup class.
    /// </summary>
    public class Startup : FunctionsStartup
    {
        /// <summary>
        /// Configuration Method.
        /// </summary>
        /// <param name="builder">Builder.</param>
        /// <exception cref="ArgumentNullException">Argument Exception.</exception>
        public override void Configure(IFunctionsHostBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.Services.AddSingleton<IDynamicsApiClient, DynamicsApiClient>().AddHttpClient();
        }

        /// <summary>
        /// Configure App Configuration.
        /// </summary>
        /// <param name="builder">Builder.</param>
        /// <exception cref="ArgumentNullException">Argument Exception.</exception>
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
