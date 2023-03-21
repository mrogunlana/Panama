using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Panama.Canal.Models;
using Panama.Extensions;
using Panama.Security.Resolvers;

namespace Panama.Canal.Extensions
{
    public static class InternalMessageExtensions
    {
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
            if (value.Value == null)
                return message;

            message.Content = encryptor.ToString(JsonConvert.SerializeObject(value.Value));

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
            var result = JsonConvert.DeserializeObject<T>(value);
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
            var result = nameof(value).ToString();

            message.Status = result;

            return message;
        }
    }
}
