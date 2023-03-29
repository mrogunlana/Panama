using Microsoft.Extensions.DependencyInjection;
using Panama.Canal.Interfaces;
using Panama.Canal.Invokers;
using Panama.Interfaces;

namespace Panama.Canal.Extensions
{
    public static class BusExtensions
    {
        public static IBus Origin(this IBus bus, IContext origin)
        {
            bus.Context.Origin = origin;

            return bus.Context.Current;
        }
        public static IBus Header(this IBus bus, string key, string? value)
        {
            bus.Context.Headers.Add(key, value);

            return bus.Context.Current;
        }
        public static IBus Topic(this IBus bus, string topic)
        {
            bus.Context.Name = topic;

            return bus.Context.Current;
        }
        public static IBus Ack(this IBus bus, string ack)
        {
            bus.Context.Ack = ack;

            return bus.Context.Current;
        }
        public static IBus Nack(this IBus bus, string nack)
        {
            bus.Context.Nack = nack;

            return bus.Context.Current;
        }
        public static IBus Token(this IBus bus, CancellationToken token)
        {
            bus.Context.Token = token;

            return bus.Context.Current;
        }
        public static IBus CorrelationId(this IBus bus, string id)
        {
            bus.Context.CorrelationId = id;

            return bus.Context.Current;
        }
        public static IBus Group(this IBus bus, string group)
        {
            bus.Context.Group = group;

            return bus.Context.Current;
        }
        public static IBus Data(this IBus bus, params IModel[] models)
        {
            foreach (var model in models)
                bus.Context.Data.Add(model);

            return bus.Context.Current;
        }
        public static IBus Data(this IBus bus, IModel model)
        {
            bus.Context.Data.Add(model);


            return bus.Context.Current;
        }
        public static IBus Data(this IBus bus, IEnumerable<IModel> models)
        {
            foreach (var model in models)
                bus.Context.Data.Add(model);

            return bus.Context.Current;
        }
        public static IBus Instance(this IBus bus, string instance)
        {
            bus.Context.Instance = instance;

            return bus.Context.Current;
        }
        public static IBus Invoker<T>(this IBus bus)
            where T : IInvoke
        {
            if (bus.Context.Provider == null)
                throw new InvalidOperationException("Service provider could not be located.");

            bus.Context.Invoker = bus.Context.Provider.GetRequiredService<T>();

            return bus.Context.Current;
        }
        public static IBus Polling(this IBus bus)
        {
            bus.Invoker<PollingInvoker>();

            return bus.Context.Current;
        }

        public static IBus Stream(this IBus bus)
        {
            bus.Invoker<StreamInvoker>();

            return bus.Context.Current;
        }
    }
}
