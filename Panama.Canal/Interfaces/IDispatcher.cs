using Panama.Canal.Models;

namespace Panama.Canal.Interfaces
{
    public interface IDispatcher : IService
    {
        ValueTask Publish(InternalMessage message);

        ValueTask Execute(InternalMessage message, object? descriptor = null);

        ValueTask Schedule(InternalMessage message, DateTime publishTime, object? transaction = null);
    }
}
