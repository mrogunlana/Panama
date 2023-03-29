using Microsoft.Extensions.DependencyInjection;
using Panama.Canal.Interfaces;
using Panama.Canal.Models;
using Panama.Extensions;
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

            var messages = result.Data.PublishGet<InternalMessage>();
            var dispatcher = provider.GetRequiredService<IDispatcher>();

            foreach (var message in messages)
            {
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
            }

            result.Data.RemoveAll<Publish<InternalMessage>>();
        }
    }
}
