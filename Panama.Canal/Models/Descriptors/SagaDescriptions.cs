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
        private ConcurrentDictionary<Type, IReadOnlyDictionary<string, ReadOnlyCollection<IDescriptor>>>? _descriptions;
        public IServiceProvider Provider { get; }

        public ConcurrentDictionary<Type, IReadOnlyDictionary<string, ReadOnlyCollection<IDescriptor>>>? Entries => _descriptions;

        public SagaDescriptions(IServiceProvider provider)
        {
            Provider = provider;
        }

        public void Set(Dictionary<Type, IReadOnlyDictionary<string, ReadOnlyCollection<IDescriptor>>> descriptions)
        {
            _descriptions = new ConcurrentDictionary<Type, IReadOnlyDictionary<string, ReadOnlyCollection<IDescriptor>>>(descriptions);
        }

        public IReadOnlyDictionary<string, ReadOnlyCollection<IDescriptor>> GetDescriptions(Type type)
        {
            if (Entries == null)
                throw new InvalidOperationException("Saga descriptions are not initialized.");

            var results = Entries[type];
            if (results == null)
                throw new InvalidOperationException($"descriptions for target {type?.FullName} cannot be located.");

            return results;
        }

        public IReadOnlyCollection<IDescriptor> GetDescriptions(string group, Type type)
        {
            if (Entries == null)
                throw new InvalidOperationException("Saga descriptions are not initialized.");

            var results = Entries[type];
            if (results == null)
                throw new InvalidOperationException($"Saga descriptions for target {type?.FullName} cannot be located.");

            return results[group];
        }

        public IReadOnlyCollection<IDescriptor> GetDescriptions<T>(string group)
            where T : ITarget
        {
            return GetDescriptions(group, typeof(T));
        }

        public IReadOnlyCollection<IDescriptor>? GetDescriptions(Type type, string group, string name)
        {
            var result = GetDescriptions(group, type);
            if (result == null)
                return null;

            return new ReadOnlyCollection<IDescriptor>(result.Where(s => string.Equals(s.Topic, name, StringComparison.OrdinalIgnoreCase)).ToList());
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
                    throw new InvalidOperationException($"Saga description cannot be found for Target: {type}, Group: {group}, and Topic: {name}.");

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
