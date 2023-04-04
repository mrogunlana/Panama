using Microsoft.Extensions.DependencyInjection;
using Panama.Canal.Interfaces;
using Panama.Interfaces;

namespace Panama.Canal.Extensions
{
    public static class ContextExtensions
    {
        public static IBus Bus(this IContext context)
        {
            return context.Provider.GetRequiredService<IBus>()
                .Id(Guid.NewGuid().ToString());
        }
    }
}