using Panama.Core.CDC.Interfaces;

namespace Panama.Core.CDC.Services
{
    public class Dispatcher : IDispatcher
    {
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public ValueTask EnqueueToExecute(InternalMessage message, object? descriptor = null)
        {
            throw new NotImplementedException();
        }

        public ValueTask EnqueueToPublish(InternalMessage message)
        {
            throw new NotImplementedException();
        }

        public ValueTask EnqueueToScheduler(InternalMessage message, DateTime publishTime, object? transaction = null)
        {
            throw new NotImplementedException();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
