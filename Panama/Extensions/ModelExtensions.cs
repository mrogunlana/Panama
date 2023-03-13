using Newtonsoft.Json;
using Panama.Interfaces;
using Panama.Models;
using System.Collections.Generic;
using System.Linq;

namespace Panama.Extensions
{
    public static class ModelExtensions
    {
        public static List<T> DataGet<T>(this IList<IModel> data) where T : IModel
        {
            var result = new List<T>();
            foreach (var model in data)
                if (model is T)
                    result.Add((T)model);
            return result;
        }

        public static T DataGetSingle<T>(this IList<IModel> data) where T : IModel
        {
            return data.DataGet<T>().FirstOrDefault();
        }

        public static void RemoveAll<T>(this IList<IModel> data) where T : IModel
        {
            var delete = data.DataGet<T>();
            foreach (var deleted in delete)
                data.Remove(deleted);
        }

        public static bool Exist<T>(this IList<IModel> data) where T : IModel
        {
            var exist = data.DataGet<T>();
            if (exist.Count == 0)
                return false;

            return true;
        }

        public static List<Kvp<K, V>> KvpGet<K, V>(this IList<IModel> data)
        {
            return data
                .DataGet<Kvp<K, V>>()
                .Select(x => x)
                .ToList();
        }

        public static List<V> KvpGet<K, V>(this IList<IModel> data, K key)
        {
            return data
                .DataGet<Kvp<K,V>>()
                .Where(x => EqualityComparer<K>.Default.Equals(x.Key, key) && x.Value is V)
                .Select(x => x.Value)
                .ToList();
        }

        public static V KvpGetSingle<K, V>(this IList<IModel> data, K key)
        {
            return data.KvpGet<K, V>(key).FirstOrDefault();
        }

        public static T Copy<T>(this IModel data)
        {
            var copy = JsonConvert.SerializeObject(data);

            var result = JsonConvert.DeserializeObject<T>(copy);

            if (result == null)
                throw new Exception($"IModel data type: {typeof(T)} cannot be copied.");

            return result;
        }

        public static List<T> SnapshotGet<T>(this IList<IModel> data) where T : IModel
        {
            var result = new List<T>();
            foreach (var model in data)
                if (model is ISnapshot<T>)
                    result.Add((T)model);
            return result;
        }

        public static T SnapshotGetSingle<T>(this IList<IModel> data) where T : IModel
        {
            return data.SnapshotGet<T>().FirstOrDefault();
        }

        public static void Snapshot<T>(this IList<IModel> data, T model) where T : IModel
        {
            data.Add(new Snapshot<T>(model));
        }

        public static List<Kvp<K, V>> SnapshotGet<K, V>(this IList<IModel> data)
        {
            return data
                .DataGet<Snapshot<Kvp<K, V>>>()
                .Select(x => x.Value)
                .ToList();
        }

        public static List<V> SnapshotGet<K, V>(this IList<IModel> data, K key)
        {
            return data
                .DataGet<Snapshot<Kvp<K, V>>>()
                .Where(x => EqualityComparer<K>.Default.Equals(x.Value.Key, key) && x.Value.Value is V)
                .Select(x => x.Value.Value)
                .ToList();
        }

        public static V SnapshotGetSingle<K, V>(this IList<IModel> data, K key)
        {
            return data.SnapshotGet<K, V>(key).FirstOrDefault();
        }

        public static void AddRange(this IList<IModel> data, params IModel[] models)
        {
            foreach (var model in models)
                data.Add(model);
        }
        public static void AddRange(this IList<IModel> data, IEnumerable<IModel> models)
        {
            foreach (var model in models)
                data.Add(model);
        }
    }
}
