using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Panama.Canal.Models.Descriptors;
using Panama.Canal.Models.Messaging;
using Panama.Extensions;
using Panama.Interfaces;
using Panama.Models;
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

        public static Message ToMessage(this TransientMessage message, IServiceProvider provider)
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

            return metadata;
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

        public static IResult TryGetModels(this TransientMessage message, IServiceProvider provider)
        {
            try
            {
                if (provider == null)
                    throw new ArgumentNullException(nameof(IServiceProvider));

                message.RemoveException();

                var resolver = provider.GetService<StringEncryptorResolver>();
                if (resolver == null)
                    throw new ArgumentNullException($"{nameof(StringEncryptorResolver)} must be registered to process Messages.");

                var encryptor = resolver(StringEncryptorResolverKey.Base64);
                if (encryptor == null)
                    throw new ArgumentNullException($"Base64 encryptor must be registered to process Messages.");

                var body = Encoding.UTF8.GetString(message.Body.ToArray());
                var result = encryptor.FromString(body);
                var external = JsonConvert.DeserializeObject<Message>(result, new JsonSerializerSettings() {
                    TypeNameHandling = TypeNameHandling.All
                });

                if (external == null)
                    throw new InvalidOperationException($"Message ID: {message.Headers[Headers.Id]} could not be located.");
                
                var local = external.ToInternal(provider);
                if (local == null)
                    throw new InvalidOperationException($"Interal message ID: {message.Headers[Headers.Id]} could not be located.");

                var data = external.GetData<IList<IModel>>();
                
                external.RemoveException();

                var subscriptions = provider.GetRequiredService<SubscriberDescriptions>().HasDescriptions(external);
                if (subscriptions == false)
                    throw new InvalidCastException($"No subscribers can be found for message ID: {message.Headers[Headers.Id]}.");

                return new Result()
                    .Success()
                    .Add(local)
                    .Add(data)
                    .Add(external)
                    .Add(message);
            }
            catch (Exception ex)
            {
                message.AddException(ex);

                var external = new Message(message.Headers, Encoding.UTF8.GetString(message.Body.ToArray()))
                        .AddCreatedTime()
                        .AddException(ex)
                        .AddMessageId(Guid.NewGuid().ToString());

                return new Result()
                    .Fail()
                    .Add(message)
                    .Add(external)
                    .Add(external.ToInternal(provider));
            }
        }
    }
}
