using Panama.Canal.Interfaces;
using System.Collections.ObjectModel;

namespace Panama.Canal.Extensions
{
    public static class DescriptorExtensions
    {
        public static Dictionary<Type, IReadOnlyDictionary<string, ReadOnlyCollection<IDescriptor>>> ToDictionary(this IEnumerable<IDescriptor> descriptions)
        {
            var targets = descriptions.Select(s => s.Target).Distinct();
            var buckets = descriptions.GroupBy(s => s.Target);
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

        public static IReadOnlyDictionary<string, ReadOnlyCollection<IDescriptor>> ToDictionary<T>(this IEnumerable<KeyValuePair<string, ReadOnlyCollection<IDescriptor>>> values)
            where T : ITarget
        {
            var descriptions = new List<IDescriptor>();

            foreach (var value in values)
                descriptions.AddRange(value.Value);

            var dictionary = descriptions.ToDictionary();
            
            return dictionary[typeof(T)];
        }
    }
}
