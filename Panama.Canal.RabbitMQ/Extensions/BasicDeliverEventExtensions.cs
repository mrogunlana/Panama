using RabbitMQ.Client.Events;
using System.Text;

namespace Panama.Canal.RabbitMQ.Extensions
{
    internal static class BasicDeliverEventExtensions
    {
        internal static Dictionary<string, string?> GetHeaders(this BasicDeliverEventArgs args)
        {
            var headers = new Dictionary<string, string?>();

            if (args == null)
                throw new ArgumentNullException(nameof(args));
            if (args.BasicProperties == null)
                return headers;
            if (args.BasicProperties.Headers == null)
                return headers;

            foreach (var header in args.BasicProperties.Headers)
            {
                if (header.Value is byte[] val)
                    headers.Add(header.Key, Encoding.UTF8.GetString(val));
                else
                    headers.Add(header.Key, header.Value?.ToString());
            }

            return headers;
        }
    }
}
