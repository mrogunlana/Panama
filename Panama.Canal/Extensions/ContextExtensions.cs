using Microsoft.Extensions.DependencyInjection;
using Panama.Canal.Interfaces;
using Panama.Canal.Sagas.Extensions;
using Panama.Canal.Sagas.Models;
using Panama.Extensions;
using Panama.Interfaces;

namespace Panama.Canal.Extensions
{
    public static class ContextExtensions
    {
        public static IBus Bus(this IContext context)
        {
            return context.Provider.GetRequiredService<IMarina>()
                .GetBus(context.Token)
                .Reply(context.KvpGetSingle<string, string>("ReplyTopic"));
        }

        public static SagaContext Saga<T>(this IContext context)
        {
            return new SagaContext(context.Provider, context)
                .Token(context.Token)
                .Type<T>();
        }
    }
}