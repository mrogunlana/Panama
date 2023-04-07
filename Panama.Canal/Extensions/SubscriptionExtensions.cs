using Microsoft.Extensions.Logging;
using Panama.Canal.Attributes;
using Panama.Canal.Comparers;
using Panama.Canal.Interfaces;
using Panama.Canal.Models;
using System.Collections.ObjectModel;

namespace Panama.Canal.Extensions
{
    public static class SubscriptionExtensions
    {
        public static IEnumerable<Subscription> GetSubscriptions(this IEnumerable<ISubscribe> subscribers, ILogger log)
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
                    topic: topic.Topic,
                    group: topic.Group,
                    subscriber: subscriber.GetType(),
                    target: topic?.Target?.GetType() ?? typeof(DefaultTarget));

                subscriptions.Add(subscription);
            }

            subscriptions = subscriptions
                .Distinct(new SubscriptionComparer(log))
                .ToList();

            return subscriptions;
        }

        public static Dictionary<Type, IReadOnlyDictionary<string, ReadOnlyCollection<Subscription>>> ToDictionary(this IEnumerable<Subscription> subscriptions)
        {
            var targets = subscriptions.Select(s => s.Target).Distinct();
            var buckets = subscriptions.GroupBy(s => s.Target);
            var results = new Dictionary<Type, IReadOnlyDictionary<string, ReadOnlyCollection<Subscription>>>();

            foreach (var bucket in buckets)
            {
                var target = bucket.Key;
                var groups = bucket.ToList().GroupBy(s => s.Group);
                var dictionary = groups.ToDictionary(
                    group => group.Key,
                    group => group
                        .ToList()
                        .AsReadOnly());

                results.TryAdd(target, dictionary);
            }

            return results;
        }

        public static IReadOnlyCollection<Subscription> GetSubscriptions(this Models.Subscriptions subscriptions, string group, Type type)
        {
            if (subscriptions.Entries == null)
                throw new InvalidOperationException("Subscriptions are not initialized.");

            var results = subscriptions.Entries[type];
            if (results == null)
                throw new InvalidOperationException($"Subscriptions for target {type?.FullName} cannot be located.");

            return results[group];
        }

        public static IReadOnlyCollection<Subscription> GetSubscriptions<T>(this Models.Subscriptions subscriptions, string group)
            where T : ITarget
        {
            return GetSubscriptions(subscriptions, group, typeof(T));
        }

        public static IReadOnlyDictionary<string, ReadOnlyCollection<Subscription>> GetSubscriptions(this Models.Subscriptions subscriptions, Type type)
        {
            if (subscriptions.Entries == null)
                throw new InvalidOperationException("Subscriptions are not initialized.");

            var results = subscriptions.Entries[type];
            if (results == null)
                throw new InvalidOperationException($"Subscriptions for target {type?.FullName} cannot be located.");

            return results;
        }

        public static IReadOnlyCollection<Subscription>? GetSubscriptions(this Models.Subscriptions subscriptions, Type type, string group, string name)
        {
            var result = GetSubscriptions(subscriptions, group, type);
            if (result == null)
                return null;

            return new ReadOnlyCollection<Subscription>(result.Where(s => string.Equals(s.Topic, name, StringComparison.OrdinalIgnoreCase)).ToList());
        }
    }
}
