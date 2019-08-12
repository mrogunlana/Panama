using System;

namespace Panama.Core.Logger
{
    public interface ILog
    {
        void LogInformation<T>(T obj, string message);
        void LogInformation<T>(string message);
        void LogWarning<T>(string message);
        void LogException<T>(Exception obj);
        void LogException<T>(string message);
        void LogTrace<T>(T obj, string message);
        void LogTrace<T>(string message);
        void LogDebug<T>(T obj, string message);
        void LogDebug<T>(string message);
    }
}
