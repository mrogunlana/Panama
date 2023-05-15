using Panama.Canal.Interfaces;
using Panama.Canal.Sagas.Models;
using Panama.Interfaces;

namespace Panama.Canal.Sagas.Extensions
{
    public static class SagaContextExtensions
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
    }
}
