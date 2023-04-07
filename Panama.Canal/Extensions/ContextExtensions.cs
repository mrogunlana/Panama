using Microsoft.Extensions.DependencyInjection;
using Panama.Canal.Interfaces;
using Panama.Canal.Models;
using Panama.Canal.Sagas.Stateless.Extensions;
using Panama.Interfaces;

namespace Panama.Canal.Extensions
{
    public static class ContextExtensions
    {
        public static IBus Bus(this IContext context)
        {
            return context.Provider.GetRequiredService<IBus>()
                .Token(context.Token);
        }

        public static SagaContext Saga<T>(this IContext context)
        {
            return new SagaContext(context.Provider, context)
                .Token(context.Token)
                .Type<T>();
        }
    }
}