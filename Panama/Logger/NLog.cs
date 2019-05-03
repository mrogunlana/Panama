using System;
using NLogger = NLog;

namespace Panama.Logger
{
    public class NLog : ILog
    {
        public void LogException<T>(Exception obj)
        {
            var log = NLogger.LogManager.GetLogger(obj.GetType().FullName);

            log.Log(NLogger.LogLevel.Error, obj);
        }

        public void LogException<T>(string message)
        {
            var log = NLogger.LogManager.GetLogger(typeof(T).FullName);

            log.Log(NLogger.LogLevel.Error, message);
        }

        public void LogInformation<T>(T obj, string message)
        {
            var log = NLogger.LogManager.GetLogger(obj.GetType().FullName);

            log.Log(NLogger.LogLevel.Info, message);
        }

        public void LogInformation<T>(string message)
        {
            var log = NLogger.LogManager.GetLogger(typeof(T).FullName);

            log.Log(NLogger.LogLevel.Info, message);
        }
        public void LogWarning<T>(string message)
        {
            var log = NLogger.LogManager.GetLogger(typeof(T).FullName);

            log.Log(NLogger.LogLevel.Warn, message);
        }
        public void LogTrace<T>(T obj, string message)
        {
            var log = NLogger.LogManager.GetLogger(obj.GetType().FullName);

            log.Log(NLogger.LogLevel.Trace, message);
        }

        public void LogTrace<T>(string message)
        {
            var log = NLogger.LogManager.GetLogger(typeof(T).FullName);

            log.Log(NLogger.LogLevel.Trace, message);
        }
        public void LogDebug<T>(T obj, string message)
        {
            var log = NLogger.LogManager.GetLogger(obj.GetType().FullName);

            log.Log(NLogger.LogLevel.Debug, message);
        }

        public void LogDebug<T>(string message)
        {
            var log = NLogger.LogManager.GetLogger(typeof(T).FullName);
            
            log.Log(NLogger.LogLevel.Debug, message);
        }
    }
}
