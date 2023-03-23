using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Panama.Canal.Attributes;
using Panama.Canal.Comparers;
using Panama.Canal.Interfaces;
using Panama.Canal.Models;

namespace Panama.Canal.Intializers
{
    internal class Subscriptions : IInitialize
    {
        private readonly IServiceProvider _provider;
        private readonly ILogger<Subscriptions> _log;
        private readonly Models.Subscriptions _subscriptions;
        private readonly IOptions<CanalOptions> _options;

        public Subscriptions(
             IServiceProvider provider
           , ILogger<Subscriptions> log
           , IOptions<CanalOptions> options
           , Models.Subscriptions subscriptions)
        {
            _log = log;
            _options = options;
            _provider = provider;
            _subscriptions = subscriptions;
        }
         
        public Task Invoke(CancellationToken token)
        {
            if (token.IsCancellationRequested)
                token.ThrowIfCancellationRequested();

            var subscribers = _provider.GetServices<ISubscribe>();
            if (subscribers == null)
                return Task.CompletedTask;
            if (subscribers.Count() == 0)
                return Task.CompletedTask;

            foreach (var subscriber in subscribers)
            {
                var attribute = Attribute
                    .GetCustomAttribute(
                        subscriber.GetType(), 
                        typeof(TopicAttribute));

                if (attribute == null)
                    throw new InvalidOperationException($"Topic attribute could not be found on Subscription: {subscriber.GetType().Name}.");

                var topic = (TopicAttribute)attribute;
                if (topic == null)
                    throw new InvalidOperationException($"Subscription: {subscriber.GetType().Name} needs a Topic attribute.");

                var subscription = new Subscription(
                    topic: topic.Topic,
                    group: topic.Group,
                    subscriber: subscriber.GetType(),
                    broker: topic.Broker);

                _subscriptions.Subscribers.Add(subscription);
            }

            _subscriptions.Subscribers = _subscriptions.Subscribers
                .Distinct(new SubscriptionComparer(_log))
                .ToList();

            return Task.CompletedTask;
        }
    }
}