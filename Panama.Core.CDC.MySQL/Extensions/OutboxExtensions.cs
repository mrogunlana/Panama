using Panama.Core.Security.Interfaces;

namespace Panama.Core.CDC.MySQL.Extensions
{
    internal static class OutboxExtensions
    {
        internal static bool IsContentBase64(this Outbox message)
        {
            //TODO: Do we need to consider padding?
            //Convert.TryFromBase64String(message.Content.PadRight(message.Content.Length / 4 * 4 + (message.Content.Length % 4 == 0 ? 0 : 4), '='), new Span<byte>(new byte[message.Content.Length]), out _);

            var buffer = new Span<byte>(new byte[message.Content.Length]);
            return Convert.TryFromBase64String(message.Content, buffer, out int _);
        }

        internal static IEnumerable<Outbox> DecodeContent(this IEnumerable<Outbox> messages, IStringEncryptor encryptor)
        {
            foreach (var message in messages)
            {
                if (!message.IsContentBase64())
                    continue;

                message.Content = encryptor.FromString(message.Content);
            }

            return messages;
        }
    }
}
