using System;
using NLogger = NLog;

namespace Panama.Core.Logger
{
    public class NLog : ILog
    {
        //private readonly Guid _id;

        //public NLog()
        //{
        //    _id = new Guid();
        //}
        public void LogException<T>(Exception obj)
        {
            //TODO: Maybe this could work in conjunction with Autofac scoping
            //to provide Log per Handler Request??
            //var log = NLogger.LogManager.GetLogger(obj.GetType().FullName);
            //var info = new NLogger.LogEventInfo(NLogger.LogLevel.Error, log.Name, obj.Message);

            //info.Properties["LogId"] = _id.ToString();

            //log.Log(info);

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
