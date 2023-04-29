using Microsoft.Extensions.Logging;
using Panama.Interfaces;

namespace Panama.Configuration
{
    public class Logger<T> : ILog<T>
    {
        private readonly ILogger _logger;
        public Logger(ILogger<T> logger)
        {
            _logger = logger;
        }

        public void LogDebug(T obj, string message)
        {
            _logger.LogDebug(message, obj);
        }

        public void LogDebug(string message)
        {
            _logger.LogDebug(message);
        }

        public void LogException(Exception obj)
        {
            _logger.LogError(obj, null);
        }

        public void LogException(string message)
        {
            _logger.LogError(null);
        }

        public void LogInformation(T obj, string message)
        {
            _logger.LogInformation(message, obj);
        }

        public void LogInformation(string message)
        {
            _logger.LogInformation(message);
        }

        public void LogTrace(T obj, string message)
        {
            _logger.LogTrace(message, obj);
        }

        public void LogTrace(string message)
        {
            _logger.LogTrace(message);
        }

        public void LogWarning(string message)
        {
            _logger.LogWarning(message);
        }
    }
}
