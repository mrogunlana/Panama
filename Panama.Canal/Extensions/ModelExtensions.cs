using Microsoft.Extensions.DependencyInjection;
using Panama.Canal.Interfaces;
using Panama.Canal.Models;
using Panama.Extensions;
using Panama.Interfaces;

namespace Panama.Canal.Extensions
{
    public static class ModelExtensions
    {
        public static List<T> PublishGet<T>(this IList<IModel> data) where T : IModel
        {
            var result = new List<T>();
            foreach (var model in data)
                if (model is Publish<T>)
                    result.Add((T)model);
            return result;
        }

        public static T? PublishGetSingle<T>(this IList<IModel> data) where T : IModel
        {
            return data.PublishGet<T>().FirstOrDefault();
        }

        public static void Publish<T>(this IList<IModel> data, T model) where T : IModel
        {
            data.Add(new Publish<T>(model));
        }

        public static void Flush(this IList<IModel> data, IServiceProvider provider, CancellationToken token = default)
        {
            var messages = data.PublishGet<InternalMessage>();
            var dispatcher = provider.GetRequiredService<IDispatcher>();

            foreach (var message in messages)
            {
                var metadata = message.GetData<Message>(provider);
                var delay = metadata.GetDelay();

                if (delay == DateTime.MinValue)
                    dispatcher.Publish(
                        message: message,
                        token: token)
                        .ConfigureAwait(false)
                        .GetAwaiter()
                        .GetResult();
                else
                    dispatcher.Schedule(
                        message: message,
                        delay: delay,
                        token: token)
                        .ConfigureAwait(false)
                        .GetAwaiter()
                        .GetResult();
            }
        }

        public static void AddFiltered<T>(this IList<IModel> data, IResult result)
            where T : IModel
        {
            if (result == null)
                return;
            if (!result.Success)
                return;

            data.AddRange(result.Data.DataGet<IFilter<T>>());
        }
    }
}
