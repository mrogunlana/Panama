using System;

namespace Panama.Core.Interfaces
{
    public interface ILogFactory
    {
        ILog<T> CreateLogger<T>();
    }
}
