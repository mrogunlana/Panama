using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Panama.Canal.Brokers.Interfaces;
using Panama.Canal.Extensions;
using Panama.Canal.Models;

namespace Panama.Canal.Brokers
{
    public class SubscriptionObserver : IObserver<InternalMessage>
    {
        private readonly string _topic;
        private IDisposable? _unsubscriber;
        private readonly IBrokerClient _client;
        private readonly IServiceProvider _provider;
        private readonly ILogger<SubscriptionObserver> _log;

        public SubscriptionObserver(string topic, IBrokerClient client, IServiceProvider provider)
        {
            _topic = topic;
            _client = client;
            _provider = provider;
            _log = provider.GetRequiredService<ILogger<SubscriptionObserver>>();
        }

        public virtual void OnCompleted() => _log.LogTrace($"Topic {_topic} observation completed.");

        public virtual void OnError(Exception ex) => _log.LogError(ex, $"Topic {_topic} observation error occurred.");

        public virtual void OnNext(InternalMessage message)
        {
            var transient = message.ToTransient(_provider);

            _client?.OnCallback!(transient, message.Id).GetAwaiter().GetResult();
        }
    }
}
