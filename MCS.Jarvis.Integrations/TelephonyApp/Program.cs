// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace TelephonyIntegration
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Build the host
            var host = CreateWebHostBuilder(args).Build();

            // Run the app
            host.Run();


        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();


    }
}
