using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Panama.Canal.Models;
using Panama.Canal.Models.Options;
using Panama.Extensions;
using Panama.Security.Resolvers;
using System.Text;

namespace Panama.Canal.Extensions
{
    public static class InternalMessageExtensions
    {
        public static bool IsContentBase64(this string? message)
        {
            //TODO: Do we need to consider padding?
            //Convert.TryFromBase64String(message.Content.PadRight(message.Content.Length / 4 * 4 + (message.Content.Length % 4 == 0 ? 0 : 4), '='), new Span<byte>(new byte[message.Content.Length]), out _);

            if (message == null)
                return false;

            var buffer = new Span<byte>(new byte[message.Length]);
            return Convert.TryFromBase64String(message, buffer, out int _);
        }

        public static bool IsContentBase64(this InternalMessage message)
        {
            return message.Content.IsContentBase64();
        }

        public static bool IsContentBase64(this object value)
        {
            if (value == null)
                return false;

            var result = value.ToString();
            if (string.IsNullOrEmpty(result))
                return false;

            return result.IsContentBase64();
        }

        public static InternalMessage AddCorrelationId(this InternalMessage message, Message value)
        {
            if (value == null)
                return message;
            if (value.Headers == null)
                return message;
            if (value.Headers.Count == 0)
                return message;

            var result = value.Headers[Headers.CorrelationId];

            if (string.IsNullOrEmpty(result))
                return message;

            message.CorrelationId = result;

            return message;
        }
        public static InternalMessage AddMessageId(this InternalMessage message, Message value)
        {
            if (value ==  null)
                return message;
            if (value.Headers == null)
                return message;
            if (value.Headers.Count == 0)
                return message;

            var result = value.Headers[Headers.Id];

            if (string.IsNullOrEmpty(result))
                return message;

            message.Id = result;

            return message;
        }
        public static InternalMessage AddMessageGroup(this InternalMessage message, Message value)
        {
            if (value == null)
                return message;
            if (value.Headers == null)
                return message;
            if (value.Headers.Count == 0)
                return message;

            var result = value.Headers[Headers.Group];

            if (string.IsNullOrEmpty(result))
                return message;

            message.Group = result;

            return message;
        }
        public static InternalMessage AddMessageBroker(this InternalMessage message, Message value)
        {
            if (value == null)
                return message;
            if (value.Headers == null)
                return message;
            if (value.Headers.Count == 0)
                return message;

            var result = value.Headers[Headers.Broker];

            if (string.IsNullOrEmpty(result))
                return message;

            message.Broker = result;

            return message;
        }
        public static InternalMessage AddMessageName(this InternalMessage message, Message value)
        {
            if (value == null)
                return message;
            if (value.Headers == null)
                return message;
            if (value.Headers.Count == 0)
                return message;

            var result = value.Headers[Headers.Name];

            if (string.IsNullOrEmpty(result))
                return message;

            message.Name = result;

            return message;
        }
        public static InternalMessage AddCreatedTime(this InternalMessage message, Message value)
        {
            if (value == null)
                return message;
            if (value.Headers == null)
                return message;
            if (value.Headers.Count == 0)
                return message;

            var result = value.Headers[Headers.Created];

            if (string.IsNullOrEmpty(result))
                return message;

            message.Created = DateTimeOffset.Parse(result).UtcDateTime;

            return message;
        }
        public static InternalMessage AddData(this InternalMessage message, Message value, IServiceProvider provider)
        {
            var resolver = provider.GetService<StringEncryptorResolver>();
            if (resolver == null)
                throw new ArgumentNullException($"{nameof(StringEncryptorResolver)} must be registered to process Messages.");

            var encryptor = resolver(StringEncryptorResolverKey.Base64);
            if (encryptor == null)
                throw new ArgumentNullException($"Base64 encryptor must be registered to process Messages.");

            if (value == null)
                return message;

            message.Content = encryptor.ToString(JsonConvert.SerializeObject(value, new JsonSerializerSettings {
                TypeNameHandling = TypeNameHandling.All
            }));
            
            return message;
        }
        public static T GetData<T>(this InternalMessage message, IServiceProvider provider)
        {
            var resolver = provider.GetService<StringEncryptorResolver>();
            if (resolver == null)
                throw new ArgumentNullException($"{nameof(StringEncryptorResolver)} must be registered to process Messages.");

            var encryptor = resolver(StringEncryptorResolverKey.Base64);
            if (encryptor == null)
                throw new ArgumentNullException($"Base64 encryptor must be registered to process Messages.");

            var value = encryptor.FromString(message.Content);
            var result = JsonConvert.DeserializeObject<T>(value, new JsonSerializerSettings {
                TypeNameHandling = TypeNameHandling.All
            });
            if (result == null)
                throw new InvalidOperationException($"Message context could not be decoded.");

            return result;
        }
        public static IEnumerable<T> GetData<T>(this IEnumerable<InternalMessage> messages, IServiceProvider provider)
        {
            var result = new List<T>();

            foreach (var message in messages)
                result.Add(message.GetData<T>(provider));

            return result;
        }
        public static InternalMessage ToInternal(this Message message, IServiceProvider provider)
        {
            return new InternalMessage()
                .AddCorrelationId(message)
                .AddMessageId(message)
                .AddMessageName(message)
                .AddMessageGroup(message)
                .AddMessageBroker(message)
                .AddCreatedTime(message)
                .AddData(message, provider);
        }
        public static InternalMessage SetStatus<T>(this InternalMessage message, T value)
            where T : struct
        {
            var result = value.ToString();
            if (result == null)
                throw new InvalidOperationException($"Message status cannot be set using type: {typeof(T)} of {value}");

            message.Status = result;

            return message;
        }
        public static InternalMessage SetRetries(this InternalMessage message, int value)
        {
            message.Retries = value;

            return message;
        }
        public static TransientMessage ToTransient(this InternalMessage message, IServiceProvider provider)
        {
            if (message == null) 
                throw new ArgumentNullException("Internal message could not be located.");

            var metadata = message.GetData<Message>(provider);
            if (metadata == null)
                throw new ArgumentNullException("Message not be built from internal message.");

            var resolver = provider.GetService<StringEncryptorResolver>();
            if (resolver == null)
                throw new ArgumentNullException($"{nameof(StringEncryptorResolver)} must be registered to process Messages.");

            var encryptor = resolver(StringEncryptorResolverKey.Base64);
            if (encryptor == null)
                throw new ArgumentNullException($"Base64 encryptor must be registered to process Messages.");

            var encrypted = message.IsContentBase64() 
                ? message.Content 
                : encryptor.ToString(message.Content) ?? string.Empty;
            var bytes = Encoding.UTF8.GetBytes(encrypted);

            return new TransientMessage(metadata.Headers, bytes);
        }

        public static InternalMessage SetFailedExpiration(this InternalMessage message, IServiceProvider provider, DateTime? value = null)
        {
            var options = provider.GetRequiredService<IOptions<CanalOptions>>();

            message.Expires = ((value ?? DateTime.UtcNow).ToUniversalTime()).AddSeconds(options.Value.FailedMessageExpiredAfter);

            return message;
        }

        public static InternalMessage SetSucceedExpiration(this InternalMessage message, IServiceProvider provider, DateTime? value = null)
        {
            var options = provider.GetRequiredService<IOptions<CanalOptions>>();

            message.Expires = ((value ?? DateTime.UtcNow).ToUniversalTime()).AddSeconds(options.Value.SucceedMessageExpiredAfter);

            return message;
        }
    }
}
