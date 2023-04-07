using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Panama.Canal.Interfaces;
using Panama.Canal.Interfaces.Sagas;
using Panama.Canal.Invokers;
using Panama.Canal.Models;
using Panama.Interfaces;
using Quartz.Util;

namespace Panama.Canal.Extensions
{
    public static class BusExtensions
    {
        public static IBus Origin(this IBus bus, IContext origin)
        {
            bus.Context.Origin = origin;

            return bus;
        }
        public static IBus Header(this IBus bus, IDictionary<string, string?>? headers = null)
        {
            if (headers == null)
                return bus;

            foreach (var header in headers)
                bus.Context.Headers.Add(header.Key, header.Value);

            return bus;
        }
        public static IBus Header(this IBus bus, string key, string? value)
        {
            bus.Context.Headers.Add(key, value);

            return bus;
        }
        public static IBus Topic(this IBus bus, string topic)
        {
            bus.Context.Name = topic;

            return bus;
        }
        public static IBus Reply(this IBus bus, string? ack = null)
        {
            if (ack == null)
                return bus;

            bus.Context.Reply = ack;

            return bus;
        }
        public static IBus Transaction<T>(this IBus bus, T? current)
        {
            if (current == null)
                return bus;

            bus.Context.Transaction = current;

            return bus;
        }
        public static IBus Token(this IBus bus, CancellationToken token)
        {
            bus.Context.Token = token;

            return bus;
        }
        public static IBus CorrelationId(this IBus bus, string? id = null)
        {
            if (id == null)
                return bus;

            bus.Context.CorrelationId = id;

            return bus;
        }
        public static IBus Target<T>(this IBus bus)
            where T : ITarget
        {
            bus.Context.Target = typeof(T);

            return bus;
        }
        public static IBus Target(this IBus bus, Type? type = null)
        {
            if (type == null)
                return bus;

            bus.Context.Target = type;

            return bus;
        }
        public static IBus Delay(this IBus bus, DateTime? delay = null)
        {
            if (delay == null)
                return bus;

            bus.Context.Delay = delay.Value;

            return bus;
        }
        public static IBus Channel(this IBus bus, IChannel? channel = null)
        {
            if (channel == null)
                return bus;

            bus.Context.Channel = channel;
            bus.Context.Transaction = channel.Current;

            return bus;
        }
        public static IBus Group(this IBus bus, string? group = null)
        {
            if (group == null)
                return bus;

            bus.Context.Group = group;

            return bus;
        }
        public static IBus Id(this IBus bus, string? id = null)
        {
            if (id == null)
                return bus;

            bus.Context.Id = id;

            return bus;
        }
        public static IBus Data(this IBus bus, params IModel[]? models)
        {
            if (models == null)
                return bus; 

            foreach (var model in models)
                bus.Context.Data.Add(model);

            return bus;
        }
        public static IBus Data(this IBus bus, IModel model)
        {
            bus.Context.Data.Add(model);


            return bus;
        }
        public static IBus Data(this IBus bus, IEnumerable<IModel>? models = null)
        {
            if (models == null)
                return bus;

            foreach (var model in models)
                bus.Context.Data.Add(model);

            return bus;
        }
        public static IBus Instance(this IBus bus, string? instance = null)
        {
            if (instance == null)
                return bus;

            bus.Context.Instance = instance;

            return bus;
        }
        public static IBus Invoker<T>(this IBus bus)
            where T : IInvoke
        {
            if (bus.Context.Provider == null)
                throw new InvalidOperationException("Service provider could not be located.");

            bus.Context.Invoker = bus.Context.Provider.GetRequiredService<T>();

            return bus;
        }
        public static IBus Polling(this IBus bus)
        {
            bus.Invoker<PollingPublisherInvoker>();

            return bus;
        }

        public static IBus Stream(this IBus bus)
        {
            bus.Invoker<OutboxInvoker>();

            return bus;
        }

        public static IBus SagaId(this IBus bus, string? id = null)
        {
            if (id == null)
                return bus;

            bus.Context.SagaId = id;

            return bus;
        }
        public static IBus SagaType(this IBus bus, string? type = null)
        {
            if (type == null)
                return bus;

            bus.Context.SagaType = type;

            return bus;
        }

        public static IBus Trigger<T>(this IBus bus)
            where T : ISagaTrigger
        {
            bus.Context.Headers.Add(Headers.SagaTrigger, typeof(T).GetType().AssemblyQualifiedName);

            return bus;
        }
    }
}
