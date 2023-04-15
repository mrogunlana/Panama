using Panama.Canal.Extensions;
using Panama.Canal.Interfaces;
using Panama.Interfaces;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;

namespace Panama.Canal.Models
{
    public class ConsumerSubscriptions : IModel
    {
        private ConcurrentDictionary<Type, IReadOnlyDictionary<string, ReadOnlyCollection<Subscription>>>? _subscriptions;
        public IServiceProvider Provider { get; }

        public ConcurrentDictionary<Type, IReadOnlyDictionary<string, ReadOnlyCollection<Subscription>>>? Entries => _subscriptions;

        public ConsumerSubscriptions(IServiceProvider provider)
        {
            Provider = provider;
        }

        public void Set(Dictionary<Type, IReadOnlyDictionary<string, ReadOnlyCollection<Subscription>>> subscriptions)
        {
            _subscriptions = new ConcurrentDictionary<Type, IReadOnlyDictionary<string, ReadOnlyCollection<Subscription>>>(subscriptions); ;
        }

        public IReadOnlyDictionary<string, ReadOnlyCollection<Subscription>> GetSubscriptions(Type type)
        {
            if (Entries == null)
                throw new InvalidOperationException("Subscriptions are not initialized.");

            var results = Entries[type];
            if (results == null)
                throw new InvalidOperationException($"Subscriptions for target {type?.FullName} cannot be located.");

            return results;
        }

        public IReadOnlyCollection<Subscription> GetSubscriptions(string group, Type type)
        {
            if (Entries == null)
                throw new InvalidOperationException("Subscriptions are not initialized.");

            var results = Entries[type];
            if (results == null)
                throw new InvalidOperationException($"Subscriptions for target {type?.FullName} cannot be located.");

            return results[group];
        }

        public IReadOnlyCollection<Subscription> GetSubscriptions<T>(string group)
            where T : ITarget
        {
            return GetSubscriptions(group, typeof(T));
        }

        public IReadOnlyCollection<Subscription>? GetSubscriptions(Type type, string group, string name)
        {
            var result = GetSubscriptions(group, type);
            if (result == null)
                return null;

            return new ReadOnlyCollection<Subscription>(result.Where(s => string.Equals(s.Topic, name, StringComparison.OrdinalIgnoreCase)).ToList());
        }

        public bool HasSubscribers(Message message)
        {
            try
            {
                var name = message.GetName();
                var group = message.GetGroup();
                var type = message.GetBrokerType();

                var subscribers = GetSubscriptions(type, group, name);
                if (subscribers == null || subscribers.Count == 0)
                    throw new InvalidOperationException($"Subscriber cannot be found for Target: {type}, Group: {group}, and Topic: {name}.");

                return true;
            }
            catch (Exception ex)
            {
                message.AddException(ex);

                return false;
            }
        }
    }
}
