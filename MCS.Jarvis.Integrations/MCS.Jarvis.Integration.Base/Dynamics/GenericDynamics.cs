namespace MCS.Jarvis.Integration.Base.Dynamics
{
    using MCS.Jarvis.Integration.Base.Logging;

    /// <summary>
    /// Generic Dynamics class.
    /// </summary>
    public class GenericDynamics
    {
        /// <summary>
        /// dynamics API Client.
        /// </summary>
        private readonly IDynamicsApiClient dynamicsApiClient;

        /// <summary>
        /// logger object.
        /// </summary>
        private readonly ILoggerService logger;

        /// <summary>
        /// Generic Dynamics constructor.
        /// </summary>
        /// <param name="dynamicsApiClient">dynamics API Client.</param>
        /// <param name="logger">logger object.</param>
        public GenericDynamics(IDynamicsApiClient dynamicsApiClient, ILoggerService logger)
        {
            this.dynamicsApiClient = dynamicsApiClient;
            this.logger = logger;
        }
    }
}
