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
    }
}
