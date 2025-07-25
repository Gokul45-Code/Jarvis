namespace MCS.Jarvis.Integration.Base.Logging
{
    public interface ILoggerService
    {
        /// <summary>
        /// The LogTrace.
        /// </summary>
        /// <param name="traceMessage">The traceMessage<see cref="string"/>.</param>
        void LogTrace(string traceMessage);

        /// <summary>
        /// The Log warning.
        /// </summary>
        /// <param name="warningMessage">The warning message<see cref="string"/>.</param>
        void LogWarning(string warningMessage);

        /// <summary>
        /// The LogException.
        /// </summary>
        /// <param name="ex">The ex<see cref="Exception"/>.</param>
        void LogException(Exception ex);

        /// <summary>
        /// Log exception.
        /// </summary>
        /// <param name="ex">exception object.</param>
        /// <param name="logMessage">log exception message.</param>
        void LogException(Exception ex, string logMessage);

        /// <summary>
        /// Add Custom Dimension.
        /// </summary>
        /// <param name="dimensionProperties">Dimension properties.</param>
        /// <param name="messages">Log messages.</param>
        void AddCustomDimension(Dictionary<string, object> dimensionProperties, params object[] messages);
    }
}
