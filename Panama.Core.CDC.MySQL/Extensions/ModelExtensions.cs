using MySqlConnector;
using Panama.Core.Interfaces;
using Panama.Core.Security.Interfaces;
using System.Data.Common;
using System.Reflection;

namespace Panama.Core.CDC.MySQL.Extensions
{
    internal static class ModelExtensions
    {
        internal static async Task GetModels<T>(this MySqlDataReader reader, MySqlSettings settings)
            where T : IModel, new()
        {
            if (reader == null)
                return;

            if (settings == null)
                return;

            var results = new List<T>();
            var map = settings.GetMap(table);

            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                var model = new T();
                for (int i = 0; i < reader.FieldCount; i++)
                    model.SetValue<T>(settings.PublishedTableMap[i], reader.GetValue(i));

                results.Add(model);
            }
        }
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

        internal static T Resolve<T>(this IModel model)
            where T : IModel
        {
            if (model == null)
                throw new ArgumentException($"Model must have a value to resolve type: {typeof(T)}.");

            if (model is not T)
                throw new ArgumentException($"Model must be of type: {typeof(T)} to resolve.");

            return (T)model;
        }
    }
}
