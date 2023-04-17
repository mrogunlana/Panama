using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Panama.Canal.Brokers.Interfaces;
using Panama.Canal.Models;

namespace Panama.Canal.Brokers
{
    public class DefaultClient : IBrokerClient
    {
        private readonly string _queue;
        private readonly DefaultOptions _options;
        private readonly IServiceProvider _provider;
        private readonly IDefaultObservable _observable;
        private readonly IList<IDisposable> _subscriptions;

        public Func<TransientMessage, object?, Task>? OnCallback { get; set; }

        public DefaultClient(
              string queue
            , IServiceProvider provider)
        {
            _queue = queue;
            _provider = provider;
            _subscriptions = new List<IDisposable>();
            _observable = provider.GetRequiredService<IDefaultObservable>();
            _options = provider.GetRequiredService<IOptions<DefaultOptions>>().Value;
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
                _subscriptions.Add(_observable.Subscribe(new DefaultSubscriber(topic, this, _provider)));
        }
    }
}
