using Panama.Canal.Interfaces;
using Panama.Canal.Models;
using Quartz;
using System.Collections.Concurrent;
using System.Threading.Channels;

namespace Panama.Canal.Jobs
{
    [DisallowConcurrentExecution]
    public class Dispatcher : IDispatcher, IJob
    {
        private CancellationTokenSource? _cts;

        private readonly CancellationTokenSource _delay = new();
        private readonly IInvokeBrokers _brokers;
        private readonly IInvokeSubscriptions _subscriptions;
        private readonly PriorityQueue<InternalMessage, DateTime> _scheduled;

        private Channel<InternalMessage> _published = default!;
        private ConcurrentDictionary<string, Channel<InternalMessage>> _received = default!;
        public Dispatcher(
              IInvokeSubscriptions subscriptions
            , IInvokeBrokers brokers)
        {
            var capacity = 500;

            _brokers = brokers;
            _subscriptions = subscriptions;

            _scheduled = new PriorityQueue<InternalMessage, DateTime>();
            _received = new ConcurrentDictionary<string, Channel<InternalMessage>>(1, 2);
            _published = Channel.CreateBounded<InternalMessage>(
                new BoundedChannelOptions(capacity > 5000 ? 5000 : capacity)
                {
                    AllowSynchronousContinuations = true,
                    SingleReader = true,
                    SingleWriter = true,
                    FullMode = BoundedChannelFullMode.Wait
                });
        }
        public Task Execute(IJobExecutionContext context)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            _cts = CancellationTokenSource.CreateLinkedTokenSource(context.CancellationToken, CancellationToken.None);
            _cts.Token.Register(() => _delay.Cancel());

            return Task.CompletedTask;
        }

        public ValueTask Publish(InternalMessage message, CancellationToken? token = null)
        {
            throw new NotImplementedException();
        }

        public ValueTask Execute(InternalMessage message, object? descriptor = null, CancellationToken? token = null)
        {
            throw new NotImplementedException();
        }

        public ValueTask Schedule(InternalMessage message, DateTime delay, object? transaction = null, CancellationToken? token = null)
        {
            throw new NotImplementedException();
        }
    }
}
