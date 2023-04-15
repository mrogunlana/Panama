using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Panama.Canal.Extensions;
using Panama.Canal.Invokers;
using Panama.Canal.Models;
using Panama.Extensions;
using Panama.Models;

namespace Panama.Canal.Brokers
{
    public class DefaultSubscriber : IObserver<InternalMessage>
    {
        private readonly string _topic;
        private IDisposable? _unsubscriber;
        private readonly IServiceProvider _provider;
        private readonly ILogger<DefaultSubscriber> _log;

        public DefaultSubscriber(string topic, IServiceProvider provider)
        {
            _topic = topic;
            _provider = provider;
            _log = provider.GetRequiredService<ILogger<DefaultSubscriber>>();
        }

        public virtual void OnCompleted() => _log.LogTrace($"Topic {_topic} observation completed.");

        public virtual void OnError(Exception ex) => _log.LogError(ex, $"Topic {_topic} observation error occurred.");

        public virtual void OnNext(InternalMessage message)
        {
            var local = message.GetData<Message>(_provider);
            if (local == null)
                throw new InvalidOperationException($"Observered external message cannot be located. Message ID: {message.Id}.");

            var invoker = _provider.GetRequiredService<ReceivedInvokerFactory>().GetInvoker();
            var context = new Context(
                correlationId: local.GetCorrelationId(),
                provider: _provider)
                    .Add(message)
                    .Add(local);

            invoker.Invoke(context);
        }
    }
}
