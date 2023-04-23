using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Panama.Canal.Interfaces;
using Panama.Canal.Models;
using Panama.Canal.Sagas.Stateless.Interfaces;
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

            if (message.Headers.ContainsKey(Headers.CorrelationId))
                message.Headers.Remove(Headers.CorrelationId);

            message.Headers.Add(Headers.CorrelationId, value);

            return message;
        }
        public static Message AddMessageId(this Message message, string value)
        {
            if (string.IsNullOrEmpty(value))
                return message;

            if (message.Headers.ContainsKey(Headers.Id))
                message.Headers.Remove(Headers.Id);

            message.Headers.Add(Headers.Id, value);

            return message;
        }
        public static Message AddMessageName(this Message message, string value)
        {
            if (string.IsNullOrEmpty(value))
                return message;

            if (message.Headers.ContainsKey(Headers.Name))
                message.Headers.Remove(Headers.Name);

            message.Headers.Add(Headers.Name, value);

            return message;
        }
        public static Message AddMessageGroup(this Message message, string? value)
        {
            if (string.IsNullOrEmpty(value))
                return message;

            if (message.Headers.ContainsKey(Headers.Group))
                message.Headers.Remove(Headers.Group);

            message.Headers.Add(Headers.Group, value);

            return message;
        }
        public static Message AddMessageBroker(this Message message, string? value = null)
        {
            var text = value?.ToString() ?? string.Empty;

            if (string.IsNullOrEmpty(text))
                text = typeof(DefaultTarget).AssemblyQualifiedName;

            if (message.Headers.ContainsKey(Headers.Broker))
                message.Headers.Remove(Headers.Broker);

            message.Headers.Add(Headers.Broker, text);

            return message;
        }

        public static Message AddMessageInstance(this Message message, string? value = null)
        {
            if (string.IsNullOrEmpty(value))
                return message;

            if (message.Headers.ContainsKey(Headers.Instance))
                message.Headers.Remove(Headers.Instance);

            message.Headers.Add(Headers.Instance, value);

            return message;
        }
        public static Message AddMessageTopic(this Message message, string value)
        {
            if (string.IsNullOrEmpty(value))
                return message;

            if (message.Headers.ContainsKey(Headers.Name))
                message.Headers.Remove(Headers.Name);

            message.Headers.Add(Headers.Name, value);

            return message;
        }
        public static Message AddMessageType(this Message message, string value)
        {
            if (string.IsNullOrEmpty(value))
                return message;

            if (message.Headers.ContainsKey(Headers.Type))
                message.Headers.Remove(Headers.Type);

            message.Headers.Add(Headers.Type, value);

            return message;
        }
        public static Message AddReply(this Message message, string? value)
        {
            if (string.IsNullOrEmpty(value))
                return message;
            
            if (message.Headers.ContainsKey(Headers.Reply))
                message.Headers.Remove(Headers.Reply);

            message.Headers.Add(Headers.Reply, value);

            return message;
        }
        public static Message AddTrigger<T>(this Message message, T value)
            where T : ISagaTrigger
        {
            if (value == null)
                return message;

            if (message.Headers.ContainsKey(Headers.SagaTrigger))
                message.Headers.Remove(Headers.SagaTrigger);

            message.Headers.Add(Headers.SagaTrigger, value.GetType().AssemblyQualifiedName);

            return message;
        }
        public static Message AddException(this Message message, string value)
        {
            if (string.IsNullOrEmpty(value))
                return message;

            if (message.Headers.ContainsKey(Headers.Exception))
                message.Headers.Remove(Headers.Exception);

            message.Headers.Add(Headers.Exception, value);

            return message;
        }
        public static Message AddException(this Message message, Exception? value)
        {
            if (value == null)
                return message;

            if (message.Headers.ContainsKey(Headers.Exception))
                message.Headers.Remove(Headers.Exception);

            message.Headers.Add(Headers.Exception, value.Message);

            return message;
        }
        public static Message AddCreatedTime(this Message message, DateTime? value = null)
        {
            if (message.Headers.ContainsKey(Headers.Created))
                message.Headers.Remove(Headers.Created);

            message.Headers.Add(Headers.Created, value?.ToUniversalTime().ToString() ?? DateTime.UtcNow.ToString());

            return message;
        }
        public static Message AddDelayTime(this Message message, DateTime? value)
        {
            if (value == null)
                return message;
            if (value == DateTime.MinValue)
                return message;

            if (message.Headers.ContainsKey(Headers.Delay))
                message.Headers.Remove(Headers.Delay);
            
            message.Headers.Add(Headers.Delay, value?.ToUniversalTime().ToString());

            return message;
        }
        public static Message AddSentTime(this Message message, DateTime value)
        {
            if (message.Headers.ContainsKey(Headers.Sent))
                message.Headers.Remove(Headers.Sent);

            message.Headers.Add(Headers.Sent, value.ToUniversalTime().ToString());

            return message;
        }
        public static Message AddData<T>(this Message message, T data)
        {
            message.Value = data;

            return message;
        }
        public static T? GetData<T>(this Message message)
        {
            if (message.Value is T)
                return (T)message.Value;

            return default;
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
        public static string GetReply(this Message message)
        {
            if (message.Headers == null)
                throw new InvalidOperationException("Message headers cannot be found.");

            message.Headers.TryGetValue(Headers.Reply, out var result);

            return result ?? string.Empty;
        }
        public static bool HasReply(this Message message)
        {
            if (string.IsNullOrEmpty(GetReply(message)))
                return false;

            return true;
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

            message.Headers.TryGetValue(Headers.Instance, out var result);

            return result ?? string.Empty;
        }
        public static DateTime GetDelay(this Message message)
        {
            if (message.Headers == null)
                throw new InvalidOperationException("Message headers cannot be found.");

            if (!message.Headers.TryGetValue(Headers.Delay, out var result))
                return DateTime.MinValue;

            if (!DateTime.TryParse(result, out var delay))
                throw new InvalidOperationException($"Header: {Headers.Delay} could not be parsed.");

            return delay;
        }
        public static string GetBroker(this Message message)
        {
            if (message.Headers == null)
                throw new InvalidOperationException("Message headers cannot be found.");

            message.Headers.TryGetValue(Headers.Broker, out var result);
            
            if (result == null)
                throw new InvalidOperationException($"Header: {Headers.Broker} cannot be found.");

            return result;
        }

        public static Type GetBrokerType(this Message message)
        {
            if (message.Headers == null)
                throw new InvalidOperationException("Message headers cannot be found.");
          
            var result = Type.GetType(message.GetBroker());
            if (result == null)
                throw new InvalidOperationException("Broker type cannot be located.");

            return result;
        }

        public static string GetSagaType(this Message message)
        {
            if (message.Headers == null)
                throw new InvalidOperationException("Message headers cannot be found.");

            message.Headers.TryGetValue(Headers.SagaType, out var result);
            if (result == null)
                return string.Empty;

            return result;
        }

        public static string GetSagaId(this Message message)
        {
            if (message.Headers == null)
                throw new InvalidOperationException("Message headers cannot be found.");

            message.Headers.TryGetValue(Headers.SagaId, out var result);

            if (result == null)
                throw new InvalidOperationException($"Header: {Headers.SagaId} cannot be found.");

            return result;
        }

        public static string GetSagaTrigger(this Message message)
        {
            if (message.Headers == null)
                throw new InvalidOperationException("Message headers cannot be found.");

            message.Headers.TryGetValue(Headers.SagaTrigger, out var result);

            if (result == null)
                throw new InvalidOperationException($"Header: {Headers.SagaTrigger} cannot be found.");

            return result;
        }

        public static bool IsSagaReply(this Message message)
        {
            var type = message.GetSagaType();
            var name = message.GetName();

            if (string.IsNullOrEmpty(type))
                return false;
            if (string.IsNullOrEmpty(name))
                return false;
            if (!name.Contains(type, StringComparison.OrdinalIgnoreCase))
                return false;

            return true;
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
