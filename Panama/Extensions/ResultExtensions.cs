using Panama.Interfaces;

namespace Panama.Extensions
{
    public static class ResultExtensions
    {
        public static List<T> DataGet<T>(this IResult result) where T : IModel
        {
            return result.DataGet<T>();
        }

        public static T DataGetSingle<T>(this IResult result) where T : IModel
        {
            return result.DataGetSingle<T>();
        }
        public static List<T> KvpGet<T>(this IResult result, string key)
        {
            return result.KvpGet<T>(key);
        }

        public static T KvpGetSingle<T>(this IResult result, string key)
        {
            return result.KvpGetSingle<T>(key);
        }

        public static void RemoveAll<T>(this IResult result) where T : IModel
        {
            result.RemoveAll<T>();
        }

        public static bool Exist<T>(this IResult result) where T : IModel
        {
            return result.Exist<T>();
        }

        public static void ContinueWith(this IResult result, Action<IResult> action)
        {
            if (result == null)
                return;
            if (!result.Success)
                return;

            action.Invoke(result);
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
        public static IResult Add(this IResult result, IModel model)
        {
            result.Data.Add(model);

            return result;
        }
        public static IResult Add(this IResult result, params IModel[] models)
        {
            foreach (var model in models)
                result.Data.Add(model);

            return result;
        }
        public static IResult Add(this IResult result, IEnumerable<IModel> models)
        {
            foreach (var model in models)
                result.Data.Add(model);

            return result;
        }
    }
}
