using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Panama.Canal.Models;
using Panama.Security.Resolvers;
using System.Text;

namespace Panama.Canal.Extensions
{
    public static class MessageExtensions
    {
        public static Message AddHeaders(this Message message, IDictionary<string, string?>? headers)
        {
            if (headers == null)
                return message;

            foreach (var header in headers)
                message.Headers.Add(header.Key, header.Value);

            return message;
        }

        public static Message AddCorrelationId(this Message message, string? value)
        {
            if (string.IsNullOrEmpty(value))
                return message;

            message.Headers.Add(Headers.CorrelationId, value);

            return message;
        }
        public static Message AddMessageId(this Message message, string value)
        {
            if (string.IsNullOrEmpty(value))
                return message;

            message.Headers.Add(Headers.Id, value);

            return message;
        }
        public static Message AddMessageName(this Message message, string value)
        {
            if (string.IsNullOrEmpty(value))
                return message;

            message.Headers.Add(Headers.Name, value);

            return message;
        }
        public static Message AddMessageGroup(this Message message, string? value)
        {
            if (string.IsNullOrEmpty(value))
                return message;

            message.Headers.Add(Headers.Group, value);

            return message;
        }
        public static Message AddMessageBroker(this Message message, string? value = null)
        {
            var text = value?.ToString() ?? string.Empty;

            if (string.IsNullOrEmpty(text))
                text = typeof(DefaultTarget).FullName;

            message.Headers.Add(Headers.Broker, text);

            return message;
        }

        public static Message AddMessageInstance(this Message message, string? value = null)
        {
            if (string.IsNullOrEmpty(value))
                return message;

            message.Headers.Add(Headers.Instance, value);

            return message;
        }
        public static Message AddMessageTopic(this Message message, string value)
        {
            if (string.IsNullOrEmpty(value))
                return message;

            message.Headers.Add(Headers.Name, value);

            return message;
        }
        public static Message AddMessageType(this Message message, string value)
        {
            if (string.IsNullOrEmpty(value))
                return message;

            message.Headers.Add(Headers.Type, value);

            return message;
        }
        public static Message AddAck(this Message message, string? value)
        {
            if (string.IsNullOrEmpty(value))
                return message;

            message.Headers.Add(Headers.Ack, value);

            return message;
        }
        public static Message AddNack(this Message message, string? value)
        {
            if (string.IsNullOrEmpty(value))
                return message;

            message.Headers.Add(Headers.Nack, value);

            return message;
        }
        public static Message AddException(this Message message, string value)
        {
            if (string.IsNullOrEmpty(value))
                return message;

            message.Headers.Add(Headers.Exception, value);

            return message;
        }
        public static Message AddException(this Message message, Exception? value)
        {
            if (value == null)
                return message;

            message.Headers.Add(Headers.Exception, value.Message);

            return message;
        }
        public static Message AddCreatedTime(this Message message, DateTime? value = null)
        {
            message.Headers.Add(Headers.Created, value?.ToUniversalTime().ToString() ?? DateTime.UtcNow.ToString());

            return message;
        }
        public static Message AddDelayTime(this Message message, DateTime? value)
        {
            if (value == null)
                return message;

            message.Headers.Add(Headers.Delay, value?.ToUniversalTime().ToString());

            return message;
        }
        public static Message AddSentTime(this Message message, DateTime value)
        {
            message.Headers.Add(Headers.Sent, value.ToUniversalTime().ToString());

            return message;
        }
        public static Message AddData<T>(this Message message, T data)
        {
            message.Value = data;

            return message;
        }

        public static string GetGroup(this Message message)
        {
            if (message.Headers == null)
                throw new InvalidOperationException("Message headers cannot be found.");

            var result = message.Headers[Headers.Group];
            if (result == null)
                throw new InvalidOperationException($"Header: {Headers.Group} cannot be found.");

            return result;
        }
        public static string GetAck(this Message message)
        {
            if (message.Headers == null)
                throw new InvalidOperationException("Message headers cannot be found.");

            var result = message.Headers[Headers.Ack];
            
            return result ?? string.Empty;
        }
        public static string GetNack(this Message message)
        {
            if (message.Headers == null)
                throw new InvalidOperationException("Message headers cannot be found.");

            var result = message.Headers[Headers.Nack];
            
            return result ?? string.Empty;
        }
        public static string GetCorrelationId(this Message message)
        {
            if (message.Headers == null)
                throw new InvalidOperationException("Message headers cannot be found.");

            var result = message.Headers[Headers.CorrelationId];

            return result ?? string.Empty;
        }
        public static string GetName(this Message message)
        {
            if (message.Headers == null)
                throw new InvalidOperationException("Message headers cannot be found.");

            var result = message.Headers[Headers.Name];
            if (result == null)
                throw new InvalidOperationException($"Header: {Headers.Name} cannot be found.");

            return result;
        }
        public static string GetInstance(this Message message)
        {
            if (message.Headers == null)
                throw new InvalidOperationException("Message headers cannot be found.");

            var result = message.Headers[Headers.Instance];

            return result ?? string.Empty;
        }
        public static DateTime GetDelay(this Message message)
        {
            if (message.Headers == null)
                throw new InvalidOperationException("Message headers cannot be found.");

            var result = message.Headers[Headers.Delay];
            if (result == null)
                throw new InvalidOperationException($"Header: {Headers.Delay} cannot be found.");

            if (!DateTime.TryParse(message.Headers[Headers.Delay], out var delay))
                throw new InvalidOperationException($"Header: {Headers.Delay} could not be parsed.");

            return delay;
        }
        public static string GetBroker(this Message message)
        {
            if (message.Headers == null)
                throw new InvalidOperationException("Message headers cannot be found.");

            var result = message.Headers[Headers.Broker];
            if (result == null)
                throw new InvalidOperationException($"Header: {Headers.Broker} cannot be found.");

            return result;
        }

        public static TransientMessage ToTransient(this Message message, IServiceProvider provider)
        {
            if (message == null)
                throw new ArgumentNullException("Message could not be located.");

            var resolver = provider.GetService<StringEncryptorResolver>();
            if (resolver == null)
                throw new ArgumentNullException($"{nameof(StringEncryptorResolver)} must be registered to process Messages.");

            var encryptor = resolver(StringEncryptorResolverKey.Base64);
            if (encryptor == null)
                throw new ArgumentNullException($"Base64 encryptor must be registered to process Messages.");

            var serialized = JsonConvert.SerializeObject(message.Value);
            var encrypted = encryptor.ToString(serialized) ?? string.Empty;
            var bytes = Encoding.UTF8.GetBytes(encrypted);

            return new TransientMessage(message.Headers, bytes);
        }
        public static Message RemoveException(this Message message)
        {
            message.Headers.Remove(Headers.Exception);

            return message;
        }

        public static bool HasException(this Message message)
        {
            return message.Headers.ContainsKey(Headers.Exception);
        }

        public static Message ResetId(this Message message)
        {
            message.Headers[Headers.Id] = Guid.NewGuid().ToString();

            return message;
        }
    }
}
