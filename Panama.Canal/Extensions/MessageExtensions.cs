using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Panama.Canal.Models;
using Panama.Security.Resolvers;

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

        public static Message AddCorrelationId(this Message message, string value)
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
        public static Message AddMessageGroup(this Message message, string value)
        {
            if (string.IsNullOrEmpty(value))
                return message;

            message.Headers.Add(Headers.Group, value);

            return message;
        }
        public static Message AddMessageBroker(this Message message, string value)
        {
            if (string.IsNullOrEmpty(value))
                return message;

            message.Headers.Add(Headers.Broker, value);

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
        public static Message AddDelayTime(this Message message, DateTime value)
        {
            message.Headers.Add(Headers.Delay, value.ToUniversalTime().ToString());

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
    }
}
