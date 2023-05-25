using Panama.Canal.Models.Messaging;

namespace Panama.Canal.Extensions
{
    public static class DefaultFilterExtensions
    {
        public static IDictionary<string, string?> DefaultFilter(this IDictionary<string, string?> headers)
        {
            if (headers == null)
                throw new InvalidOperationException("Message headers cannot be found.");

            var filters = new List<string> {
                Headers.Id,
                Headers.CorrelationId,
                Headers.Instance,
                Headers.Name,
                Headers.Group,
                Headers.Type,
                Headers.Reply,
                Headers.Broker,
                Headers.Exception,
                Headers.Created,
                Headers.Sent,
                Headers.Delay
            };

            return headers
                .Filter(h => !filters.Contains(h.Key));
        }
    }
}
