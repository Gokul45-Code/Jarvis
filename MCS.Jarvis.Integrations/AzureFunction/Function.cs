namespace Volvo.Server
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.Azure.WebJobs.Extensions.SignalRService;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.IdentityModel.Logging;
    using Microsoft.IdentityModel.Protocols;
    using Microsoft.IdentityModel.Protocols.OpenIdConnect;
    using Microsoft.IdentityModel.Tokens;
    using Newtonsoft.Json;
    using System;
    using System.Globalization;
    using System.IdentityModel.Tokens.Jwt;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;

    public class MitelHub
    {
        private readonly IConfiguration _configuration;

        public MitelHub(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [FunctionName("Negotiate")]
        public SignalRConnectionInfo ClientConnection(
        [HttpTrigger(AuthorizationLevel.Function)] HttpRequest req, IBinder binder, ILogger log)
        {
            string userId = req.Headers["mitelAgentId"];
            try
            {
                log.LogInformation("Negotiate Azure Function Connected to VolvoSignalRHub.");
                SignalRConnectionInfoAttribute attribute = new SignalRConnectionInfoAttribute
                {
                    HubName = "VolvoSignalRHub",
                    UserId = userId,
                    ConnectionStringSetting = "AzureSignalRConnectionString"
                };
                SignalRConnectionInfo connectionInfo = binder.Bind<SignalRConnectionInfo>(attribute);
                return connectionInfo;
            }
            catch (Exception e)
            {
                log.LogError("Negotiate Azure Function Error occured : " + e.Message);
                SignalRConnectionInfo connectionInfo = new SignalRConnectionInfo();
                return connectionInfo;
            }
        }

        [FunctionName("SendNotification")]
        public async Task<IActionResult> SendNotification(
            [HttpTrigger(AuthorizationLevel.Anonymous, "POST")] HttpRequest req,
            [SignalR(HubName = "VolvoSignalRHub")] IAsyncCollector<SignalRMessage> collector
            , ILogger log)

        {
            try
            {
                bool validToken = false;
                string reqHeader = req.Headers["Authorization"];
                string tenantId = _configuration["TenantId"];
                string aud = _configuration["ClientId"];
                string clientSecret = _configuration["ClientSecret"];

                if (reqHeader != null && reqHeader.StartsWith("Bearer ") && !string.IsNullOrEmpty(tenantId) && !string.IsNullOrEmpty(aud) && !string.IsNullOrEmpty(clientSecret))
                {

                    var token = reqHeader.Trim().Substring("Bearer ".Length).Trim();
                    validToken = await ValidateToken(token, log, tenantId, aud, clientSecret);
                    if (!validToken)
                        return new UnauthorizedObjectResult("Authentication failed !");
                }
                else
                {
                    return new UnauthorizedObjectResult("Authorization Header is missing!");
                }
                var reader = new StreamReader(req.Body);
                var data = reader.ReadToEnd();
                ////log.LogInformation($"data: {data}");
                dynamic msg = JsonConvert.DeserializeObject(data);
                var mitelagentid = msg.AgentID;
                await collector.AddAsync(new SignalRMessage
                {
                    UserId = mitelagentid,
                    Target = "onMitelNotify",
                    Arguments = new[] { msg }
                });
                return mitelagentid != null
                ? (ActionResult)new OkObjectResult($"function SendNotification invoked successfully for agent :{mitelagentid}")
        : new BadRequestObjectResult("Agent is required in the request body");
            }
            catch (Exception e)
            {
                log.LogError($"Azure function SendNotification Error occured : {e.Message}");
                return new BadRequestObjectResult("Error occured : " + e.Message);
            }
        }

        public async Task<bool> ValidateToken(string token, ILogger log, string tenantId, string clientId, string clientSecret)
        {
            IdentityModelEventSource.ShowPII = true;
            var mySecurityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(clientSecret));
            var stsDiscoveryEndpoint = String.Format(CultureInfo.InvariantCulture, "https://login.microsoftonline.com/{0}/.well-known/openid-configuration", tenantId);
            var configManager = new ConfigurationManager<OpenIdConnectConfiguration>(stsDiscoveryEndpoint, new OpenIdConnectConfigurationRetriever());
            var config = await configManager.GetConfigurationAsync();
            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken readToken = tokenHandler.ReadToken(token);
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidAudience = clientId,
                ValidateIssuer = false,
                IssuerSigningKeys = config.SigningKeys,
                IssuerSigningKey = mySecurityKey,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
            };

            try
            {
                tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken validatedToken);
                return true;
            }
            catch (Exception ex)
            {
                log.LogInformation($"Exception: {ex.Message}");
                return false;
            }
        }
    }
}
