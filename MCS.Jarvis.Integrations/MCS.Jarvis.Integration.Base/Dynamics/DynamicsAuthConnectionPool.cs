namespace MCS.Jarvis.Integration.Base.Dynamics
{
    using System.Globalization;
    using Microsoft.Extensions.Configuration;

    public class DynamicsAuthConnectionPool
    {
        /// <summary>
        /// Connection thread lock object.
        /// </summary>
        private readonly object connectionLock = new object();

        /// <summary>
        /// Configuration object.
        /// </summary>
        private readonly IConfiguration config;

        /// <summary>
        /// Authentication tokens.
        /// </summary>
        private IDictionary<string, DateTime> authenticationTokens;

        /// <summary>
        /// Dynamics execution Count.
        /// </summary>
        private int dynamicsExecutionCount;

        private readonly IHttpClientFactory httpClientFactory1;

        /// <summary>
        /// Dynamics authentication connection pool.
        /// </summary>
        /// <param name="config">configuration object.</param>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public DynamicsAuthConnectionPool(IConfiguration config, IHttpClientFactory httpClientFactory)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            this.config = config;
            this.httpClientFactory1 = httpClientFactory;
        }

        /// <summary>
        /// Get Dynamics Authentication Token.
        /// </summary>
        /// <returns></returns>
        public string GetAuthenticationToken()
        {
            lock (this.connectionLock)
            {
                try
                {
                    if (this.authenticationTokens == null)
                    {
                        var isAuthenticateUsingManagedIdentity = Convert.ToBoolean(this.config.GetSection("DynamicsConfig:IsAuthenticateUsingManagedIdentity").Value, CultureInfo.InvariantCulture);
                        if (isAuthenticateUsingManagedIdentity)
                        {
                            ////this.authenticationTokens = DynamicsConnectionService.GetDynamicsTokenUsingManagedIdentity(this.config);
                        }
                        else
                        {
                            this.authenticationTokens = DynamicsConnectionService.GetDynamicsTokenUsingClientCredentials(this.config, this.httpClientFactory1);
                        }
                    }

                    foreach (KeyValuePair<string, DateTime> auth in this.authenticationTokens)
                    {
                        if (DateTime.UtcNow > auth.Value.AddMinutes(-10))
                        {
                            var isAuthenticateUsingManagedIdentity = Convert.ToBoolean(this.config.GetSection("DynamicsConfig:IsAuthenticateUsingManagedIdentity").Value, CultureInfo.InvariantCulture);
                            if (isAuthenticateUsingManagedIdentity)
                            {
                                ////this.authenticationTokens = DynamicsConnectionService.GetDynamicsTokenUsingManagedIdentity(this.config);
                            }
                            else
                            {
                                this.authenticationTokens = DynamicsConnectionService.GetDynamicsTokenUsingClientCredentials(this.config, this.httpClientFactory1);
                            }
                        }
#pragma warning disable S1751 // Refactor the containing loop
                        break;
#pragma warning restore S1751 // Refactor the containing loop
                    }

                    int authConnectionPoolSize = this.authenticationTokens.Count;
                    if (this.dynamicsExecutionCount >= authConnectionPoolSize)
                    {
                        this.dynamicsExecutionCount = 0;
                    }

                    var authenticationToken = this.authenticationTokens.Keys.ToList()[this.dynamicsExecutionCount];
                    this.dynamicsExecutionCount++;
                    return authenticationToken;
                }
                catch (IndexOutOfRangeException)
                {
                    this.dynamicsExecutionCount = 0;
                    return this.GetAuthenticationToken();
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }
    }
}
