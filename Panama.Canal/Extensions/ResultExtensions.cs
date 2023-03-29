using Microsoft.Extensions.DependencyInjection;
using Panama.Canal.Interfaces;
using Panama.Canal.Models;
using Panama.Interfaces;

namespace Panama.Canal.Extensions
{
    public static class ResultExtensions
    {
        public static void Flush(this IResult result, IServiceProvider provider, CancellationToken token = default)
        {
            if (result == null)
                return;
            if (!result.Success)
                return;

            var messages = result.Data.QueueGet<InternalMessage>();
            var dispatcher = provider.GetRequiredService<IDispatcher>();

            while (messages.Count > 0)
            {
                var message = messages.Dequeue();
                var metadata = message.GetData<Message>(provider);
                var delay = metadata.GetDelay();

                if (delay == DateTime.MinValue)
                    dispatcher.Publish(
                        message: message,
                        token: token)
                        .ConfigureAwait(false)
                        .GetAwaiter()
                        .GetResult();
                else
                    dispatcher.Schedule(
                        message: message,
                        delay: delay,
                        token: token)
                        .ConfigureAwait(false)
                        .GetAwaiter()
                        .GetResult();

                result.Data.Dequeue(message);
                result.Data.Published(message);
            }
        }

        public static IResult Published<T>(this IResult result, T model)
            where T : IModel
        {
            result.Data.Published(model);

            return result;
        }
    }
}
