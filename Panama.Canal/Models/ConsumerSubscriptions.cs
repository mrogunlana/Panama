using Panama.Interfaces;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;

namespace Panama.Canal.Models
{
    public class ConsumerSubscriptions : IModel
    {
        private ConcurrentDictionary<Type, IReadOnlyDictionary<string, ReadOnlyCollection<Subscription>>>? _subscriptions;

        public ConcurrentDictionary<Type, IReadOnlyDictionary<string, ReadOnlyCollection<Subscription>>>? Entries => _subscriptions;

        public void Set(Dictionary<Type, IReadOnlyDictionary<string, ReadOnlyCollection<Subscription>>> subscriptions)
        {
            _subscriptions = new ConcurrentDictionary<Type, IReadOnlyDictionary<string, ReadOnlyCollection<Subscription>>>(subscriptions); ;
        }
    }
}
