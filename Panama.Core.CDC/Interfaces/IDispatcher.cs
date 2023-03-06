namespace Panama.Core.CDC.Interfaces
{
    public interface IDispatcher : IService
    {
        ValueTask EnqueueToPublish(InternalMessage message);

        ValueTask EnqueueToExecute(InternalMessage message, object? descriptor = null);

        ValueTask EnqueueToScheduler(InternalMessage message, DateTime publishTime, object? transaction = null);
    }
}
