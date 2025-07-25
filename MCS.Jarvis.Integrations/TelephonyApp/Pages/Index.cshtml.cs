// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using TelephonyIntegration;

namespace TelephonyIntegration.Pages
{
    public class IndexModel : PageModel
    {
        public string accessToken { get; set; }
        public void OnGet()
        {
            //        accessToken = Program.GetAccessToken();
            //        var configuration = new ConfigurationBuilder()
            //.AddJsonFile("appsettings.json", true, true)
            //.Build();
            //var authorityUrl = configuration["authorityUrl"] + configuration["tenantId"];
            //var confidentialClient = ConfidentialClientApplicationBuilder

            //       .Create(configuration["clientId"])

            //       .WithClientSecret(configuration["clientSecret"])

            //       .WithAuthority(new Uri(authorityUrl))

            //       .Build();


            //var scopes = new List<string> { configuration["clientId"] + "/.default" };
            //var accessTokenRequest = confidentialClient.AcquireTokenForClient(scopes);

            //accessToken = configuration["clientId"];
            ViewData["BearerToken"] = HttpContext.Items["Token"];
            //return View();
        }

    }
}
