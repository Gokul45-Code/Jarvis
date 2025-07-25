namespace MCS.Jarvis.Integration.Base.Logging
{
    using System.Text;
    using Microsoft.Extensions.Logging;

    public class LoggerService : ILoggerService
    {
        /// <summary>
        /// Trace writer logger.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoggerTracingService" /> class.
        /// </summary>
        /// <param name="logger">trace writer logger.</param>
        public LoggerService(ILogger logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Writes a trace message to the CRM trace log.
        /// </summary>
        /// <param name="traceMessage">trace message.</param>
        public void LogTrace(string traceMessage)
        {
            this.logger.LogInformation(traceMessage);
        }

        /// <summary>
        /// Writes a warning message to the CRM trace log.
        /// </summary>
        /// <param name="warningMessage">trace message.</param>
        public void LogWarning(string warningMessage)
        {
            this.logger.LogWarning(warningMessage);
        }

        /// <summary>
        /// Logs the Exception raised from the Base Plugin.
        /// </summary>
        /// <param name="ex">Generated exception.</param>
        public void LogException(Exception ex)
        {
            if (ex == null)
            {
                throw new ArgumentNullException("ex", "Null Argument");
            }

            string exceptionDetails = GetExceptionDetails(ex, ex.Message);
            this.logger.LogError(ex, exceptionDetails);
        }

        /// <summary>
        /// Log exception.
        /// </summary>
        /// <param name="ex">exception object.</param>
        /// <param name="logMessage">log exception message.</param>
        public void LogException(Exception ex, string logMessage)
        {
            string exceptionDetails = GetExceptionDetails(ex, logMessage);
            this.logger.LogError(ex, exceptionDetails);
        }

        /// <summary>
        /// Add Custom Dimension.
        /// </summary>
        /// <param name="dimensionProperties">Dimension properties.</param>
        /// <param name="messages">Log message.</param>
        public void AddCustomDimension(Dictionary<string, object> dimensionProperties, params object[] messages)
        {
            using (this.logger.BeginScope(dimensionProperties))
            {
                if (messages != null)
                {
                    foreach (string message in messages)
                    {
                        this.logger.LogInformation(message);
                    }
                }
            }
        }

        /// <summary>
        /// get exception details.
        /// </summary>
        /// <param name="ex">exception object.</param>
        /// <param name="message">exception message.</param>
        /// <returns>return exception details.</returns>
        private static string GetExceptionDetails(Exception ex, string message)
        {
            StringBuilder exceptionBuilder = new StringBuilder();
            exceptionBuilder.Append(message);

            Exception? nextException = ex;
            while (nextException != null)
            {
                exceptionBuilder.AppendLine($"Type: {nextException.GetType().ToString()}");
                exceptionBuilder.AppendLine($"Source: {nextException.Source}");
                exceptionBuilder.AppendLine($"StackTrace: {nextException.StackTrace}");

                nextException = nextException.InnerException;
                if (nextException != null)
                {
                    exceptionBuilder.AppendLine("\r\n ---- Inner Exception ----");
                }
            }

            return exceptionBuilder.ToString();
        }
    }
}
