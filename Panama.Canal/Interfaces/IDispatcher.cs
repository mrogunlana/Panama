using Panama.Canal.Models;

namespace Panama.Canal.Interfaces
{
    public interface IDispatcher 
    {
        ValueTask Publish(InternalMessage message, CancellationToken? token = null);

        ValueTask Execute(InternalMessage message, object? descriptor = null, CancellationToken? token = null);

        ValueTask Schedule(InternalMessage message, DateTime delay, object? transaction = null, CancellationToken? token = null);
    }
}
