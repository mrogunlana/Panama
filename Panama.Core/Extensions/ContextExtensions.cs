using Panama.Core.Interfaces;

namespace Panama.Core.Extensions
{
    public static class ContextExtensions
    {
        public static IEnumerable<IAction> GetActions<T>(this IContext context)
        {
            var list = context.Data.OfType<IAction>();

            return list
                .Where(x => x.GetType()
                    .GetInterfaces()
                    .Any(i => i.IsGenericType
                        && (i.GetGenericTypeDefinition() == typeof(T))));
        }
        public static IEnumerable<IAction> GetActions(this IContext context, Type type)
        {
            var list = context.Data.OfType<IAction>();

            return list
                .Where(x => x.GetType()
                    .GetInterfaces()
                    .Any(i => i.IsGenericType
                        && (i.GetGenericTypeDefinition() == type)));
        }
    }
}
