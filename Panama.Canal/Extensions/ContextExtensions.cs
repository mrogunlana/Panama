using Microsoft.Extensions.DependencyInjection;
using Panama.Canal.Interfaces;
using Panama.Canal.Models;
using Panama.Interfaces;

namespace Panama.Canal.Extensions
{
    public static class ContextExtensions
    {
        public static IBus Bus(this IContext context, Type target)
        {
            if (context.Provider == null)
                throw new InvalidOperationException("Service provider cannot be located.");

            var bus = context.Provider.GetRequiredService<IBus>();

            bus.CorrelationId(context.CorrelationId);
            bus.Token(context.Token);
            bus.Header(Headers.Id, context.Id);
            bus.Header(Headers.CorrelationId, context.CorrelationId);
            bus.Origin(context);

            bus.Context.CorrelationId = context.CorrelationId;
            bus.Context.Token = context.Token;
            bus.Context.Target = target;

            return bus;
        }

        public static IBus Bus(this IContext context)
        {
            return Bus(context, typeof(DefaultTarget));
        }
        public static IBus Bus<T>(this IContext context)
            where T : ITarget
        {
            return Bus(context, typeof(T));
        }
    }
}
