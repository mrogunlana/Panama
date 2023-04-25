using Panama.Canal.Extensions;
using Panama.Canal.Interfaces;
using Panama.Canal.Models.Messaging;
using Panama.Interfaces;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;

namespace Panama.Canal.Models.Descriptors
{
    public class SagaDescriptions : IModel
    {
        private ConcurrentDictionary<Type, IReadOnlyDictionary<string, ReadOnlyCollection<SubscriberDescriptor>>>? _subscriptions;
        public IServiceProvider Provider { get; }

        public ConcurrentDictionary<Type, IReadOnlyDictionary<string, ReadOnlyCollection<SubscriberDescriptor>>>? Entries => _subscriptions;

        public SagaDescriptions(IServiceProvider provider)
        {
            Provider = provider;
        }

        public void Set(Dictionary<Type, IReadOnlyDictionary<string, ReadOnlyCollection<SubscriberDescriptor>>> subscriptions)
        {
            _subscriptions = new ConcurrentDictionary<Type, IReadOnlyDictionary<string, ReadOnlyCollection<SubscriberDescriptor>>>(subscriptions); ;
        }

        public IReadOnlyDictionary<string, ReadOnlyCollection<SubscriberDescriptor>> GetSubscriptions(Type type)
        {
            if (Entries == null)
                throw new InvalidOperationException("Subscriptions are not initialized.");

            var results = Entries[type];
            if (results == null)
                throw new InvalidOperationException($"Subscriptions for target {type?.FullName} cannot be located.");

            return results;
        }

        public IReadOnlyCollection<SubscriberDescriptor> GetSubscriptions(string group, Type type)
        {
            if (Entries == null)
                throw new InvalidOperationException("Subscriptions are not initialized.");

            var results = Entries[type];
            if (results == null)
                throw new InvalidOperationException($"Subscriptions for target {type?.FullName} cannot be located.");

            return results[group];
        }

        public IReadOnlyCollection<SubscriberDescriptor> GetSubscriptions<T>(string group)
            where T : ITarget
        {
            return GetSubscriptions(group, typeof(T));
        }

        public IReadOnlyCollection<SubscriberDescriptor>? GetSubscriptions(Type type, string group, string name)
        {
            var result = GetSubscriptions(group, type);
            if (result == null)
                return null;

            return new ReadOnlyCollection<SubscriberDescriptor>(result.Where(s => string.Equals(s.Topic, name, StringComparison.OrdinalIgnoreCase)).ToList());
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
