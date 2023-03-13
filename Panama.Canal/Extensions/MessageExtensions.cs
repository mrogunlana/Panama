using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Panama.Canal.Models;
using Panama.Security.Resolvers;

namespace Panama.Canal.Extensions
{
    internal static class MessageExtensions
    {
        internal static Message AddHeaders(this Message message, IDictionary<string, string?>? headers)
        {
            if (headers == null)
                return message;

            foreach (var header in headers)
                message.Headers.Add(header.Key, header.Value);

            return message;
        }

        internal static Message AddCorrelationId(this Message message, string value)
        {
            if (string.IsNullOrEmpty(value))
                return message;

            message.Headers.Add(Headers.CorrelationId, value);

            return message;
        }
        internal static Message AddMessageId(this Message message, string value)
        {
            if (string.IsNullOrEmpty(value))
                return message;

            message.Headers.Add(Headers.Id, value);

            return message;
        }
        internal static Message AddMessageName(this Message message, string value)
        {
            if (string.IsNullOrEmpty(value))
                return message;

            message.Headers.Add(Headers.Name, value);

            return message;
        }
        internal static Message AddMessageGroup(this Message message, string value)
        {
            if (string.IsNullOrEmpty(value))
                return message;

            message.Headers.Add(Headers.Group, value);

            return message;
        }
        internal static Message AddMessageTopic(this Message message, string value)
        {
            if (string.IsNullOrEmpty(value))
                return message;

            message.Headers.Add(Headers.Name, value);

            return message;
        }
        internal static Message AddMessageType(this Message message, string value)
        {
            if (string.IsNullOrEmpty(value))
                return message;

            message.Headers.Add(Headers.Type, value);

            return message;
        }
        internal static Message AddAck(this Message message, string? value)
        {
            if (string.IsNullOrEmpty(value))
                return message;

            message.Headers.Add(Headers.Ack, value);

            return message;
        }
        internal static Message AddNack(this Message message, string? value)
        {
            if (string.IsNullOrEmpty(value))
                return message;

            message.Headers.Add(Headers.Nack, value);

            return message;
        }
        internal static Message AddException(this Message message, string value)
        {
            if (string.IsNullOrEmpty(value))
                return message;

            message.Headers.Add(Headers.Exception, value);

            return message;
        }
        internal static Message AddException(this Message message, Exception? value)
        {
            if (value == null)
                return message;

            message.Headers.Add(Headers.Exception, value.Message);

            return message;
        }
        internal static Message AddCreatedTime(this Message message)
        {
            message.Headers.Add(Headers.Created, DateTime.UtcNow.ToString());

            return message;
        }
        internal static Message AddDelayTime(this Message message, TimeSpan value)
        {
            message.Headers.Add(Headers.Delay, value.ToString());

            return message;
        }
        internal static Message AddSentTime(this Message message, DateTime value)
        {
            message.Headers.Add(Headers.Sent, value.ToUniversalTime().ToString());

            return message;
        }
        internal static Message AddData<T>(this Message message, T data)
        {
            message.Value = data;

            return message;
        }
    }
}
