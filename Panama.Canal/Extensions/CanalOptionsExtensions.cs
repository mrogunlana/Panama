using Panama.Canal.Models.Options;
using System.Net;
using System.Text;

namespace Panama.Canal.Extensions
{
    public static class CanalOptionsExtensions
    {
        public static string GetPublishedRetryKey(this CanalOptions options)
        {
            return $"published_retry_{options.Version}";
        }
        public static string GetReceivedRetryKey(this CanalOptions options)
        {
            return $"received_retry_{options.Version}";
        }
        public static string GetInstance(this CanalOptions options)
        {
            var builder = new StringBuilder();

            var host = Dns.GetHostName();
            if (host == null)
                throw new ArgumentException("Dns host name cannot be located.");

            host = host.Length <= 50
                ? host
                : host.Substring(0, 50);

            builder.Append($"{host}_{Guid.NewGuid()}");

            return builder.ToString();
        }

        public static string GetName(this CanalOptions options, string? name = null)
        {
            if (string.IsNullOrEmpty(name))
                return string.Empty;

            var prefix = string.Join(".", new List<string?>() { options.Scope, options.TopicPrefix }.Where(x => !string.IsNullOrEmpty(x)));
            var fqn = string.Join(".", new List<string?>() { prefix, name }.Where(x => !string.IsNullOrEmpty(x)));
            if (string.IsNullOrEmpty(prefix))
                return name;
            if (name.Contains(prefix, StringComparison.OrdinalIgnoreCase))
                return name;

            return fqn;
        }
    }
}
