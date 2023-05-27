using Newtonsoft.Json.Linq;
using Panama.Canal.Models.Messaging;

namespace Panama.Canal.Extensions
{
    public static class HeaderExtensions
    {
        public static IDictionary<string, string?> Filter(this IDictionary<string, string?> headers, Func<KeyValuePair<string, string?>, bool>? predicate = null)
        {
            if (predicate == null)
                return new Dictionary<string, string?>();
            if (headers == null)
                throw new InvalidOperationException("Message headers cannot be found.");

            return headers.Where(predicate).ToDictionary(k => k.Key, k => k.Value);
        }

        public static string? GetName(this IDictionary<string, string?> headers)
        {
            if (headers == null)
                throw new InvalidOperationException("Message headers cannot be found.");

            if (headers.TryGetValue(Headers.Name, out var value))
                return value;

            return null;
        }

        public static string? GetGroup(this IDictionary<string, string?> headers)
        {
            if (headers == null)
                throw new InvalidOperationException("Message headers cannot be found.");

            if (headers.TryGetValue(Headers.Group, out var value))
                return value;

            return null;
        }

        public static string? GetBroker(this IDictionary<string, string?> headers)
        {
            if (headers == null)
                throw new InvalidOperationException("Message headers cannot be found.");

            if (headers.TryGetValue(Headers.Broker, out var value))
                return value;

            return null;
        }

        public static string? GetReply(this IDictionary<string, string?> headers)
        {
            if (headers == null)
                throw new InvalidOperationException("Message headers cannot be found.");

            if (headers.TryGetValue(Headers.Reply, out var value))
                return value;

            return null;
        }

        public static string? GetInstance(this IDictionary<string, string?> headers)
        {
            if (headers == null)
                throw new InvalidOperationException("Message headers cannot be found.");

            if (headers.TryGetValue(Headers.Instance, out var value))
                return value;

            return null;
        }
        public static string? GetSagaType(this IDictionary<string, string?> headers)
        {
            if (headers == null)
                throw new InvalidOperationException("Message headers cannot be found.");

            if (headers.TryGetValue(Headers.SagaType, out var value))
                return value;

            return null;
        }

        public static string? GetSagaId(this IDictionary<string, string?> headers)
        {
            if (headers == null)
                throw new InvalidOperationException("Message headers cannot be found.");

            if (headers.TryGetValue(Headers.SagaId, out var value))
                return value;

            return null;
        }

        public static string? GetDelay(this IDictionary<string, string?> headers)
        {
            if (headers == null)
                throw new InvalidOperationException("Message headers cannot be found.");

            if (headers.TryGetValue(Headers.Delay, out var value))
                return value;

            return null;
        }
    }
}
