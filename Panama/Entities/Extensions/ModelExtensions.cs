using System.Collections.Generic;
using System.Linq;

namespace Panama.Core.Entities
{
    public static class ModelExtensions
    {
        public static List<T> DataGet<T>(this List<IModel> data) where T : IModel
        {
            var result = new List<T>();
            foreach (var model in data)
                if (model is T)
                    result.Add((T)model);
            return result;
        }

        public static T DataGetSingle<T>(this List<IModel> data) where T : IModel
        {
            return data.DataGet<T>().FirstOrDefault();
        }

        public static void RemoveAll<T>(this List<IModel> data) where T : IModel
        {
            var delete = data.DataGet<T>();
            foreach (var deleted in delete)
                data.Remove(deleted);
        }

        public static bool Exist<T>(this List<IModel> data) where T : IModel
        {
            var exist = data.DataGet<T>();
            if (exist.Count == 0)
                return false;

            return true;
        }

        public static List<T> KvpGet<T>(this List<IModel> data, string key)
        {
            return data
                .DataGet<KeyValuePair>()
                .Where(x => x.Key == key && x.Value is T)
                .Select(x => (T)x.Value)
                .ToList();
        }

        public static T KvpGetSingle<T>(this List<IModel> data, string key)
        {
            return data.KvpGet<T>(key).FirstOrDefault();
        }
    }
}
