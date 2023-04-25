using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Panama.Canal.Brokers.Interfaces;
using Panama.Canal.Models.Messaging;

namespace Panama.Canal.Brokers
{
    public class BrokerClient : IBrokerClient
    {
        private readonly string _queue;
        private readonly BrokerOptions _options;
        private readonly IServiceProvider _provider;
        private readonly IBrokerObservable _observable;
        private readonly IList<IDisposable> _subscriptions;

        public Func<TransientMessage, object?, Task>? OnCallback { get; set; }

        public BrokerClient(
              string queue
            , IServiceProvider provider)
        {
            _queue = queue;
            _provider = provider;
            _subscriptions = new List<IDisposable>();
            _observable = provider.GetRequiredService<IBrokerObservable>();
            _options = provider.GetRequiredService<IOptions<BrokerOptions>>().Value;
        }

        public void Commit(object? sender)
        {
            // ignore
        }

        public void Dispose()
        {
            if (_subscriptions == null)
                return;

            foreach (var subscription in _subscriptions)
                subscription.Dispose();
        }

        public void Listen(TimeSpan timeout, CancellationToken cancellationToken)
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                cancellationToken.WaitHandle.WaitOne(timeout);
            }
        }

        public void Reject(object? sender)
        {
            // ignore
        }

        public void Subscribe(IEnumerable<string> topics)
        {
            foreach (var topic in topics)
                _subscriptions.Add(_observable.Subscribe(new SubscriptionObserver(topic, this, _provider)));
        }
    }
}
