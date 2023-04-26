using Panama.Interfaces;
using Panama.Models;
using System.Transactions;

namespace Panama.Extensions
{
    public static class ResultExtensions
    {
        public static List<T> DataGet<T>(this IResult result) where T : IModel
        {
            return result.Data.DataGet<T>();
        }

        public static T DataGetSingle<T>(this IResult result) where T : IModel
        {
            return result.Data.DataGetSingle<T>();
        }
        public static List<V> KvpGet<K, V>(this IResult result, K key)
        {
            return result.Data.KvpGet<K, V>(key);
        }

        public static V KvpGetSingle<K, V>(this IResult result, K key)
        {
            return result.Data.KvpGetSingle<K, V>(key);
        }

        public static void RemoveAll<T>(this IResult result) where T : IModel
        {
            result.Data.RemoveAll<T>();
        }

        public static bool Exist<T>(this IResult result) where T : IModel
        {
            return result.Data.Exist<T>();
        }

        public static void EnsureSuccess(this IResult result)
        {
            if (result == null)
                throw new Exception("Results cannot be located.");
            if (!result.Success)
                throw new Exception($"Results failed for these reasons: {string.Join(";", result.Messages)}.");
        }

        public static IResult Success(this IResult result)
        {
            result.Success = true;

            return result;
        }
        public static IResult Fail(this IResult result)
        {
            result.Success = false;

            return result;
        }
        public static IResult Cancel(this IResult result)
        {
            result.Success = false;
            result.Cancelled = true;

            return result;
        }
        public static IResult Fail(this IResult result, params string[] messages)
        {
            result.Success = false;

            foreach (var message in messages)
                result.AddMessage(message);

            return result;
        }

        public static IResult Fail(this IResult result, IEnumerable<string> messages)
        {
            return result.Fail(messages.ToArray());
        }

        public static IResult Add(this IResult result, IModel model)
        {
            result.Data.Add(model);

            return result;
        }
        public static IResult AddKvp<K, V>(this IResult result, K key, V value)
        {
            result.Data.Add(new Kvp<K, V>(key, value));

            return result;
        }
        public static IResult Add(this IResult result, params IModel[] models)
        {
            foreach (var model in models)
                result.Data.Add(model);

            return result;
        }
        public static IResult Add(this IResult result, IEnumerable<IModel>? models)
        {
            if (models == null)
                return result;

            foreach (var model in models)
                result.Data.Add(model);

            return result;
        }

        public static void Complete(this IResult result, TransactionScope scope)
        {
            if (result == null)
                return;
            if (!result.Success)
                return;

            scope.Complete();
        }

        public static IResult Add(this IResult result, string message)
        {
            result.Messages.Add(message);

            return result;
        }
    }
}
