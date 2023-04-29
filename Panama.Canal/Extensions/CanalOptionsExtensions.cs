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
    }
}
