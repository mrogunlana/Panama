using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Panama.Canal.Models;
using Panama.Extensions;
using Panama.Security.Resolvers;
using System.Text;

namespace Panama.Canal.Extensions
{
    public static class TransientMessageExtensions
    {
        public static InternalMessage ToInternal(this TransientMessage message, IServiceProvider provider)
        {
            var resolver = provider.GetService<StringEncryptorResolver>();
            if (resolver == null)
                throw new ArgumentNullException($"{nameof(StringEncryptorResolver)} must be registered to process Messages.");

            var encryptor = resolver(StringEncryptorResolverKey.Base64);
            if (encryptor == null)
                throw new ArgumentNullException($"Base64 encryptor must be registered to process Messages.");

            var encrypted = Encoding.UTF8.GetString(message.Body.ToArray());
            var value = encryptor.FromString(encrypted);
            
            var metadata = JsonConvert.DeserializeObject<Message>(value);
            if (metadata == null)
                throw new InvalidOperationException($"Message could not be located from transient.");

            return metadata.ToInternal(provider);
        }

        public static TransientMessage AddException(this TransientMessage message, string value)
        {
            if (string.IsNullOrEmpty(value))
                return message;

            message.Headers.Add(Headers.Exception, value);

            return message;
        }
        public static TransientMessage AddException(this TransientMessage message, Exception? value)
        {
            if (value == null)
                return message;

            message.Headers.Add(Headers.Exception, $"Type: {value.GetType()}. Message: {value.Message}");

            return message;
        }

        public static TransientMessage RemoveException(this TransientMessage message)
        {
            message.Headers.Remove(Headers.Exception);

            return message;
        }

        public static bool HasException(this TransientMessage message)
        {
            return message.Headers.ContainsKey(Headers.Exception);
        }

        public static string GetId(this TransientMessage message)
        {
            if (message.Headers == null)
                return string.Empty;

            var result = message.Headers[Headers.Id];
            if (result == null)
                return string.Empty;

            return result;
        }

    }
}
