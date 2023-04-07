using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Panama.Canal.Interfaces;
using Panama.Canal.Invokers;
using Panama.Canal.Models;
using Panama.Canal.Sagas.Stateless.Interfaces;
using Panama.Interfaces;
using Quartz.Util;

namespace Panama.Canal.Sagas.Stateless.Extensions
{
    public static class SagaExtensions
    {
        public static SagaContext Type<T>(this SagaContext context)
        {
            context.Type = typeof(T);

            return context;
        }
        public static SagaContext Type(this SagaContext context, Type? type = null)
        {
            if (type == null)
                return context;

            context.Type = type;

            return context;
        }
        public static SagaContext Token(this SagaContext context, CancellationToken token)
        {
            context.Token = token;

            return context;
        }
        public static SagaContext Origin(this SagaContext context, IContext origin)
        {
            context.Origin = origin;

            return context;
        }
        public static SagaContext Channel(this SagaContext context, IChannel? channel = null)
        {
            if (channel == null)
                return context;

            context.Channel = channel;
            context.Transaction = channel.Current;

            return context;
        }
        public static SagaContext Data(this SagaContext context, params IModel[]? models)
        {
            if (models == null)
                return context;

            foreach (var model in models)
                context.Data.Add(model);

            return context;
        }
        public static SagaContext Data(this SagaContext context, IModel model)
        {
            context.Data.Add(model);

            return context;
        }
        public static SagaContext Data(this SagaContext context, IEnumerable<IModel>? models = null)
        {
            if (models == null)
                return context;

            foreach (var model in models)
                context.Data.Add(model);

            return context;
        }
        public static async Task Start(this SagaContext context)
        {
            if (context.Type == null)
                throw new InvalidOperationException($"Saga cannot be located from type: {context?.Type?.Name}");

            var result = context.Provider.GetRequiredService(context.Type);
            if (result is not ISaga)
                throw new InvalidOperationException($"Saga cannot be located from type: {result.GetType().Name}");

            var saga = result as ISaga;
            if (saga == null)
                throw new InvalidOperationException($"Saga cannot be converted from type: {result.GetType().Name}");

            await saga.Start(context);
        }
    }
}
