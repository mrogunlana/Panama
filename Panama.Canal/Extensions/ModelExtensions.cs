﻿using Panama.Interfaces;

namespace Panama.Canal.Extensions
{
    public static class ModelExtensions
    {
        public static Queue<T> QueueGet<T>(this IList<IModel> data) where T : IModel
        {
            var result = new List<T>();
            foreach (var model in data)
                if (model is Models.Filters.Queued<T>)
                    result.Add(((Models.Filters.Queued<T>)model).Value);
            return new Queue<T>(result);
        }

        public static void Dequeue<T>(this IList<IModel> data, T model) where T : IModel
        {
            var value = data
                .AsQueryable()
                .OfType<Models.Filters.Queued<T>>()
                .Where(x => x.Value.Equals(model))
                .FirstOrDefault();

            if (value == null)
                throw new InvalidOperationException($"IModel of type: {typeof(T).Name} could not be dequeued from the context.");

            data.Remove(value);
        }

        public static void Queue<T>(this IList<IModel> data, T model) where T : IModel
        {
            data.Add(new Models.Filters.Queued<T>(model));
        }

        public static List<T> PublishedGet<T>(this IList<IModel> data) where T : IModel
        {
            var result = new List<T>();
            foreach (var model in data)
                if (model is Models.Filters.Published<T>)
                    result.Add(((Models.Filters.Published<T>)model).Value);
            return result;
        }

        public static T? PublishedGetSingle<T>(this IList<IModel> data) where T : IModel
        {
            return data.PublishedGet<T>().FirstOrDefault();
        }

        public static void Published<T>(this IList<IModel> data, T model) where T : IModel
        {
            data.Add(new Models.Filters.Published<T>(model));
        }
    }
}
