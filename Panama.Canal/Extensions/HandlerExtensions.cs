using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Panama.Canal.Invokers;
using Panama.Canal.Models;
using Panama.Interfaces;
using Panama.Invokers;
using Panama.Security.Resolvers;

namespace Panama.Canal.Extensions
{
    public static class HandlerExtensions
    {
        public static IHandler UseCanal(this IHandler handler)
        {
            handler.Set<ScopedInvoker>();

            return handler;
        }
        public static IHandler UseCanal<T>(this IHandler handler)
            where T : IInvoke<IHandler>
        {
            handler.Set<T>();

            return handler;
        }
    }
}
