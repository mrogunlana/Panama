using Panama.Interfaces;

namespace Panama.Canal.Extensions
{
    public static class HandlerExtensions
    {
        public static IHandler UseCanal<T>(this IHandler handler)
            where T : IInvoke<IHandler>
        {
            handler.Set<T>();

            return handler;
        }
    }
}
