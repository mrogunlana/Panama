using Panama.Canal.Extensions;
using Panama.Canal.Interfaces;
using Panama.Canal.Models.Messaging;
using Panama.Interfaces;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;

namespace Panama.Canal.Models.Descriptors
{
    public class SubscriberDescriptions : IModel
    {
        private ConcurrentDictionary<Type, IReadOnlyDictionary<string, ReadOnlyCollection<SubscriberDescriptor>>>? _descriptions;
        public IServiceProvider Provider { get; }

        public ConcurrentDictionary<Type, IReadOnlyDictionary<string, ReadOnlyCollection<SubscriberDescriptor>>>? Entries => _descriptions;

        public SubscriberDescriptions(IServiceProvider provider)
        {
            Provider = provider;
        }

        public void Set(Dictionary<Type, IReadOnlyDictionary<string, ReadOnlyCollection<SubscriberDescriptor>>> descriptions)
        {
            _descriptions = new ConcurrentDictionary<Type, IReadOnlyDictionary<string, ReadOnlyCollection<SubscriberDescriptor>>>(descriptions); ;
        }

        public IReadOnlyDictionary<string, ReadOnlyCollection<SubscriberDescriptor>> GetDescriptions(Type type)
        {
            if (Entries == null)
                throw new InvalidOperationException("Subscriber descriptions are not initialized.");

            var results = Entries[type];
            if (results == null)
                throw new InvalidOperationException($"Subscriber descriptions for target {type?.FullName} cannot be located.");

            return results;
        }

        public IReadOnlyCollection<SubscriberDescriptor> GetDescriptions(string group, Type type)
        {
            if (Entries == null)
                throw new InvalidOperationException("Subscriber descriptions are not initialized.");

            var results = Entries[type];
            if (results == null)
                throw new InvalidOperationException($"Subscriber descriptions for target {type?.FullName} cannot be located.");

            return results[group];
        }

        public IReadOnlyCollection<SubscriberDescriptor> GetDescriptions<T>(string group)
            where T : ITarget
        {
            return GetDescriptions(group, typeof(T));
        }

        public IReadOnlyCollection<SubscriberDescriptor>? GetDescriptions(Type type, string group, string name)
        {
            var result = GetDescriptions(group, type);
            if (result == null)
                return null;

            return new ReadOnlyCollection<SubscriberDescriptor>(result.Where(s => string.Equals(s.Topic, name, StringComparison.OrdinalIgnoreCase)).ToList());
        }

        public bool HasDescriptions(Message message)
        {
            try
            {
                var name = message.GetName();
                var group = message.GetGroup();
                var type = message.GetBrokerType();

                var descriptions = GetDescriptions(type, group, name);
                if (descriptions == null || descriptions.Count == 0)
                    throw new InvalidOperationException($"Subscriber descriptions cannot be found for Target: {type}, Group: {group}, and Topic: {name}.");

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
