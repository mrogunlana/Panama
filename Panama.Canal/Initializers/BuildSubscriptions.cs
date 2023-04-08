using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Panama.Canal.Attributes;
using Panama.Canal.Comparers;
using Panama.Canal.Extensions;
using Panama.Canal.Interfaces;
using Panama.Canal.Models;

namespace Panama.Canal.Initializers
{
    public class BuildSubscriptions : IInitialize
    {
        private readonly IServiceProvider _provider;
        private readonly ILogger<BuildSubscriptions> _log;
        private readonly Models.ConsumerSubscriptions _subscriptions;
        private readonly IOptions<CanalOptions> _options;

        public BuildSubscriptions(
             IServiceProvider provider
           , ILogger<BuildSubscriptions> log
           , IOptions<CanalOptions> options
           , Models.ConsumerSubscriptions subscriptions)
        {
            _log = log;
            _options = options;
            _provider = provider;
            _subscriptions = subscriptions;
        }

        private IEnumerable<Subscription> SetupSubscriptions(IEnumerable<ISubscribe> subscribers)
        {
            var subscriptions = new List<Subscription>();

            if (subscribers == null)
                return subscriptions;
            if (subscribers.Count() == 0)
                return subscriptions;

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
                    topic: topic.Name,
                    group: topic.Group ?? _options.Value.DefaultGroup,
                    subscriber: subscriber.GetType(),
                    target: topic?.Target?.GetType() ?? typeof(DefaultTarget));

                subscriptions.Add(subscription);
            }

            subscriptions = subscriptions
                .Distinct(new SubscriptionComparer(_log))
                .ToList();

            return subscriptions;
        }

        public Task Invoke(CancellationToken token)
        {
            if (token.IsCancellationRequested)
                token.ThrowIfCancellationRequested();

            var subscriptions = SetupSubscriptions(_provider
                .GetServices<ISubscribe>())
                .ToDictionary();

            if (subscriptions == null)
                return Task.CompletedTask;
            if (subscriptions.Count() == 0)
                return Task.CompletedTask;

            _subscriptions.Set(subscriptions);

            return Task.CompletedTask;
        }
    }
}