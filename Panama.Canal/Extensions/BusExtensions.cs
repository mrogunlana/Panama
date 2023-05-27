using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Panama.Canal.Interfaces;
using Panama.Canal.Invokers;
using Panama.Canal.Models.Messaging;
using Panama.Canal.Models.Options;
using Panama.Canal.Sagas.Stateless.Interfaces;
using Panama.Interfaces;
using Quartz.Util;

namespace Panama.Canal.Extensions
{
    public static class BusExtensions
    {
        public static IBus Header(this IBus bus, IDictionary<string, string?>? headers = null)
        {
            if (headers == null)
                return bus;

            foreach (var header in headers)
                bus.Context.Headers.TryAdd(header.Key, header.Value);

            return bus;
        }
        public static IBus Header(this IBus bus, string key, string? value)
        {
            if (bus.Context.Headers.TryGetValue(key, out _))
                bus.Context.Headers.Remove(key);

            bus.Context.Headers.TryAdd(key, value);

            return bus;
        }
        public static IBus Topic(this IBus bus, string topic)
        {
            if (bus.Context.Provider == null)
                throw new ArgumentNullException(nameof(bus.Context.Provider));

            var options = bus.Context.Provider.GetRequiredService<IOptions<CanalOptions>>().Value;

            var result = options.GetName(topic);

            if (bus.Context.Headers.TryGetValue(Headers.Name, out _))
                bus.Context.Headers.Remove(Headers.Name);

            bus.Context.Headers.TryAdd(Headers.Name, result);

            return bus;
        }
        public static IBus Type(this IBus bus, string? value = null)
        {
            if (string.IsNullOrEmpty(value))
                return bus;

            if (bus.Context.Headers.ContainsKey(Headers.Type))
                bus.Context.Headers.Remove(Headers.Type);

            bus.Context.Headers.Add(Headers.Type, value);

            return bus;
        }
        public static IBus Reply(this IBus bus, string? ack = null)
        {
            if (ack == null)
                return bus;

            if (bus.Context.Headers.TryGetValue(Headers.Reply, out _))
                bus.Context.Headers.Remove(Headers.Reply);

            bus.Context.Headers.TryAdd(Headers.Reply, ack);

            return bus;
        }
        public static IBus Transaction<T>(this IBus bus, T? current)
        {
            if (current == null)
                return bus;

            bus.Context.Transaction = current;

            return bus;
        }
        public static IBus Token(this IBus bus, CancellationToken? token = null)
        {
            if (token == null)
                return bus;

            bus.Context.Token = token.Value;

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
            if (bus.Context.Headers.TryGetValue(Headers.Broker, out _))
                bus.Context.Headers.Remove(Headers.Broker);

            bus.Context.Headers.TryAdd(Headers.Broker, typeof(T).AssemblyQualifiedName);

            return bus;
        }
        public static IBus Target(this IBus bus, Type? type = null)
        {
            if (type == null)
                return bus;

            if (bus.Context.Headers.TryGetValue(Headers.Broker, out _))
                bus.Context.Headers.Remove(Headers.Broker);

            bus.Context.Headers.TryAdd(Headers.Broker, type.AssemblyQualifiedName);

            return bus;
        }
        public static IBus Delay(this IBus bus, DateTime? value = null)
        {
            if (value == null)
                return bus;

            var result = value ?? DateTime.UtcNow;

            var delay = result.Kind == DateTimeKind.Utc
                ? result
                : result.ToUniversalTime();

            if (bus.Context.Headers.ContainsKey(Headers.Delay))
                bus.Context.Headers.Remove(Headers.Delay);

            bus.Context.Headers.Add(Headers.Delay, delay.ToString());

            return bus;
        }
        public static IBus Delay(this IBus bus, TimeSpan? delay = null)
        {
            if (delay == null)
                return bus;

            if (bus.Context.Headers.ContainsKey(Headers.Delay))
                bus.Context.Headers.Remove(Headers.Delay);

            bus.Context.Headers.Add(Headers.Delay, DateTime.UtcNow.Add(delay.Value).ToString());

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

            if (bus.Context.Headers.TryGetValue(Headers.Group, out _))
                bus.Context.Headers.Remove(Headers.Group);

            bus.Context.Headers.TryAdd(Headers.Group, group);

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

            if (bus.Context.Headers.TryGetValue(Headers.Instance, out _))
                bus.Context.Headers.Remove(Headers.Instance);

            bus.Context.Headers.TryAdd(Headers.Instance, instance);

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
        public static IBus Invoker(this IBus bus, Type type)
        {
            if (bus.Context.Provider == null)
                throw new InvalidOperationException("Service provider could not be located.");

            bus.Context.Invoker = (IInvoke)bus.Context.Provider.GetRequiredService(type);

            return bus;
        }
        public static IBus Invoker(this IBus bus, IInvoke invoker)
        {
            bus.Context.Invoker = invoker;

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

            if (bus.Context.Headers.TryGetValue(Headers.SagaId, out _))
                bus.Context.Headers.Remove(Headers.SagaId);

            bus.Context.Headers.TryAdd(Headers.SagaId, id);

            return bus;
        }
        public static IBus SagaType(this IBus bus, string? type = null)
        {
            if (type == null)
                return bus;

            if (bus.Context.Headers.TryGetValue(Headers.SagaType, out _))
                bus.Context.Headers.Remove(Headers.SagaType);

            bus.Context.Headers.TryAdd(Headers.SagaType, type);

            return bus;
        }

        public static IBus Trigger<T>(this IBus bus)
            where T : ISagaTrigger
        {
            bus.Context.Headers.Add(Headers.SagaTrigger, typeof(T).AssemblyQualifiedName);

            return bus;
        }

        public static IBus State<T>(this IBus bus)
            where T : ISagaState
        {
            bus.Context.Headers.Add(Headers.SagaState, typeof(T).AssemblyQualifiedName);

            return bus;
        }
        public static IBus State(this IBus bus, string? value = null)
        {
            bus.Context.Headers.Add(Headers.SagaState, value);

            return bus;
        }
        public static IBus Trigger(this IBus bus, string? value = null)
        {
            if (value == null)
                return bus;

            bus.Context.Headers.Add(Headers.SagaTrigger, value);

            return bus;
        }
    }
}
