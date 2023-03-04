using Panama.Core.Interfaces;
using Panama.Core.Security.Interfaces;
using System.Reflection;

namespace Panama.Core.CDC.MySQL.Extensions
{
    internal static class ModelExtensions
    {
        internal static void SetValue<T>(this IModel model, string name, object? value)
        {
            var property = typeof(T).GetProperty(name, BindingFlags.Public | BindingFlags.Instance);

            if (property == null)
                return;
            if (!property.CanWrite)
                return;

            var result = Convert.ChangeType(value, property.PropertyType);

            property.SetValue(model, result, null);
        }

        internal static bool IsContentBase64(this _Message message)
        {
            //TODO: Do we need to consider padding?
            //Convert.TryFromBase64String(message.Content.PadRight(message.Content.Length / 4 * 4 + (message.Content.Length % 4 == 0 ? 0 : 4), '='), new Span<byte>(new byte[message.Content.Length]), out _);

            var buffer = new Span<byte>(new byte[message.Content.Length]);
            return Convert.TryFromBase64String(message.Content, buffer, out int _);
        }

        internal static IEnumerable<IModel> DecodeContent(this IEnumerable<IModel> models, IStringEncryptor encryptor)
        {
            var messages = models.OfType<_Message>();

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
