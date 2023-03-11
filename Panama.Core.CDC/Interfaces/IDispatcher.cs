using Panama.Core.CDC.Models;

namespace Panama.Core.CDC.Interfaces
{
    public interface IDispatcher : IService
    {
        ValueTask Publish(InternalMessage message);

        ValueTask Execute(InternalMessage message, object? descriptor = null);

        ValueTask Schedule(InternalMessage message, DateTime publishTime, object? transaction = null);
    }
}
