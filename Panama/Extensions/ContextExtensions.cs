using Panama.Interfaces;
using Panama.Models;

namespace Panama.Extensions
{
    public static class ContextExtensions
    {
        public static IEnumerable<IAction> GetActions<T>(this IContext context)
        {
            var list = context.Data.OfType<IAction>().ToList();

            return list
                .Where(x => x.GetType()
                    .GetInterfaces()
                    .Any(i => i.IsGenericType
                        && (i.GetGenericTypeDefinition() == typeof(T))));
        }
        public static IEnumerable<IAction> GetActions(this IContext context, Type type)
        {
            var list = context.Data.OfType<IAction>().ToList();

            return list
                .Where(x => x.GetType()
                    .GetInterfaces()
                    .Any(i => i.IsGenericType
                        && (i.GetGenericTypeDefinition() == type)));
        }
        public static List<T> DataGet<T>(this IContext context) where T : IModel
        {
            return context.Data.DataGet<T>();
        }

        public static T DataGetSingle<T>(this IContext context) where T : IModel
        {
            return context.Data.DataGetSingle<T>();
        }
        public static List<V> KvpGet<K, V>(this IContext context, K key)
        {
            return context.Data.KvpGet<K, V>(key);
        }

        public static V KvpGetSingle<K, V>(this IContext context, K key)
        {
            return context.Data.KvpGetSingle<K, V>(key);
        }

        public static IContext Add(this IContext context, IEnumerable<IModel>? data)
        {
            if (data == null)
                return context;

            context.Data.AddRange(data);

            return context;
        }
        public static IContext Add(this IContext context, params IModel[] data)
        {
            if (data == null)
                return context;

            context.Data.AddRange(data);

            return context;
        }
        public static IContext Token(this IContext context, CancellationToken token)
        {
            context.Token = token;

            return context;
        }

        public static IContext AddKvp<K, V>(this IContext context, K key, V value)
        {
            context.Data.Add(new Kvp<K, V>(key, value));

            return context;
        }
        public static List<T> SnapshotGet<T>(this IContext context) where T : IModel
        {
            return context.Data.SnapshotGet<T>();
        }

        public static T SnapshotGetSingle<T>(this IContext context) where T : IModel
        {
            return context.Data.SnapshotGetSingle<T>();
        }

        public static void Snapshot<T>(this IContext context, T model) where T : IModel
        {
            context.Data.Add(new Snapshot<T>(model));
        }
    }
}
