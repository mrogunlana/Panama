using Panama.Core.Interfaces;
using System.Collections.Generic;

namespace Panama.Core.Extensions
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
    }
}
