using Microsoft.Extensions.Logging;
using Panama.Canal.Attributes;
using Panama.Canal.Comparers;
using Panama.Canal.Interfaces;
using Panama.Canal.Models.Descriptors;
using System.Collections.ObjectModel;

namespace Panama.Canal.Extensions
{
    public static class SubscriptionExtensions
    {
        public static Dictionary<Type, IReadOnlyDictionary<string, ReadOnlyCollection<IDescriptor>>> ToDictionary(this IEnumerable<IDescriptor> subscriptions)
        {
            var targets = subscriptions.Select(s => s.Target).Distinct();
            var buckets = subscriptions.GroupBy(s => s.Target);
            var results = new Dictionary<Type, IReadOnlyDictionary<string, ReadOnlyCollection<IDescriptor>>>();

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
    }
}
