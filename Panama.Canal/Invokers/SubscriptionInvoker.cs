using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Panama.Canal.Extensions;
using Panama.Canal.Interfaces;
using Panama.Canal.Models;
using Panama.Extensions;
using Panama.Interfaces;
using Panama.Models;

namespace Panama.Canal.Invokers
{
    public class SubscriptionInvoker : IInvoke
    {
        private readonly IBus _bus;
        private readonly ILogger<SubscriptionInvoker> _log;
        private readonly IBootstrap _bootstrapper;
        private readonly IServiceProvider _provider;
        private readonly Subscriptions _subscriptions;

        public SubscriptionInvoker(
              IBus bus
            , IBootstrap bootstrapper
            , IServiceProvider provider
            , Subscriptions subscriptions
            , ILogger<SubscriptionInvoker> log)
        {
            _bus = bus;
            _log = log;
            _provider = provider;
            _bootstrapper = bootstrapper;
            _subscriptions = subscriptions;
        }
        public async Task<IResult> Invoke(IContext? context = null)
        {
            if (context == null)
                throw new ArgumentNullException("Context cannot be located.");

            var message = context.Data.DataGetSingle<InternalMessage>();
            if (message == null)
                throw new InvalidOperationException("Message cannot be located.");

            if (!_bootstrapper.Online)
                throw new InvalidOperationException("Panama Canal has not been started.");

            var metadata = message.GetData<Message>(_provider);
            var data = message.GetData<IList<IModel>>(_provider);
            var group = metadata.GetGroup();
            var instance = metadata.GetInstance();
            var ack = metadata.GetAck();
            var nack = metadata.GetNack();
            var correlationId = metadata.GetCorrelationId();
            var target = Type.GetType(metadata.GetBroker());
            var name = metadata.GetName();
            
            if (target == null)
                throw new InvalidOperationException($"Subscription target: {metadata.GetBroker()} could not be located.");

            try
            {
                var subscriptions = _subscriptions.GetSubscriptions(target, group, name);
                if (subscriptions == null)
                    return new Result().Success();

                foreach (var subscription in subscriptions)
                {
                    var subscriber = (ISubscribe)_provider.GetRequiredService(subscription.Subscriber);
                    if (subscriber == null)
                        throw new InvalidOperationException($"Subscriber: {subscription.Subscriber.Name} could not be located.");

                    var local = new Context(data.AsEnumerable(),
                        id: message.Id,
                        correlationId: message.CorrelationId,
                        provider: _provider,
                        token: context.Token);

                    await subscriber.Event(local).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                var reason = $"Subscription target: {metadata.GetBroker()} failed processing in subscribers.";
                
                _log.LogError(ex, reason);

                if (!string.IsNullOrEmpty(nack))
                    await _provider.GetRequiredService<IBus>()
                        .Instance(instance)
                        .Token(context.Token)
                        .Id(Guid.NewGuid().ToString())
                        .CorrelationId(correlationId)
                        .Topic(nack)
                        .Group(group)
                        .Data(data)
                        .Stream()
                        .Target(target)
                        .Post()
                        .ConfigureAwait(false);

                return new Result()
                    .Fail(reason);
            }

            if (!string.IsNullOrEmpty(ack))
                await _provider.GetRequiredService<IBus>()
                        .Instance(instance)
                        .Token(context.Token)
                        .Id(Guid.NewGuid().ToString())
                        .CorrelationId(correlationId)
                        .Topic(ack)
                        .Group(group)
                        .Data(data)
                        .Stream()
                        .Target(target)
                        .Post()
                        .ConfigureAwait(false);

            return new Result()
                .Success();
        }
    }
}
