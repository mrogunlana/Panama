using Panama.Canal.Interfaces;
using Panama.Canal.Models;
using Quartz;
using System.Threading.Channels;

namespace Panama.Canal.Jobs
{
    [DisallowConcurrentExecution]
    public class Dispatcher : IDispatcher, IJob
    {
        private CancellationTokenSource? _cts;
        private Channel<InternalMessage> _published = default!;
        private Channel<InternalMessage> _received = default!;

        private readonly IInvokeBrokers _brokers;
        private readonly IInvokeSubscriptions _subscriptions;
        private readonly PriorityQueue<InternalMessage, long> _scheduled;

        public Dispatcher(
              IInvokeSubscriptions subscriptions
            , IInvokeBrokers brokers)
        {
            _brokers = brokers;
            _subscriptions = subscriptions;
            
            _scheduled = new PriorityQueue<InternalMessage, long>();
        }
        public Task Execute(IJobExecutionContext context)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            _cts = CancellationTokenSource.CreateLinkedTokenSource(context.CancellationToken, CancellationToken.None);

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
