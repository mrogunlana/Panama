using Panama.Canal.Interfaces;
using Panama.Canal.Models;
using System.Threading.Channels;

namespace Panama.Canal.Services
{
    public class Dispatcher : IDispatcher
    {
        private readonly PriorityQueue<InternalMessage, long> _scheduled;

        private readonly IInvokeBrokers _brokers;
        private readonly IInvokeSubscriptions _subscriptions;
        
        private Channel<InternalMessage> _published = default!;
        private Channel<InternalMessage> _received = default!;

        public Dispatcher(
              IInvokeSubscriptions subscriptions
            , IInvokeBrokers brokers)
        {
            _brokers = brokers;
            _subscriptions = subscriptions;
            
            _scheduled = new PriorityQueue<InternalMessage, long>();
        }
        public Task Start(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public ValueTask Execute(InternalMessage message, object? descriptor = null)
        {
            throw new NotImplementedException();
        }

        public ValueTask Publish(InternalMessage message)
        {
            throw new NotImplementedException();
        }

        public ValueTask Schedule(InternalMessage message, DateTime publishTime, object? transaction = null)
        {
            throw new NotImplementedException();
        }
    }
}
