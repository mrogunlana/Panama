using System;

namespace Panama.Interfaces
{
    public interface ILog<T>
    {
        void LogInformation(T obj, string message);
        void LogInformation(string message);
        void LogWarning(string message);
        void LogException(Exception obj);
        void LogException(string message);
        void LogTrace(T obj, string message);
        void LogTrace(string message);
        void LogDebug(T obj, string message);
        void LogDebug(string message);
    }
}
