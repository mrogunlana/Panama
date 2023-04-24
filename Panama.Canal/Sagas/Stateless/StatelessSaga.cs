using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Panama.Canal.Extensions;
using Panama.Canal.Interfaces;
using Panama.Canal.Models;
using Panama.Canal.Models.Options;
using Panama.Canal.Sagas.Interfaces;
using Panama.Canal.Sagas.Models;
using Panama.Canal.Sagas.Stateless.Interfaces;
using Panama.Canal.Sagas.Stateless.Models;
using Panama.Extensions;
using Panama.Interfaces;
using Panama.Models;
using Panama.Security.Resolvers;
using Stateless;

namespace Panama.Canal.Sagas.Stateless
{
    public abstract class StatelessSaga : IStatelessSaga
    {
        private readonly ILogger _log;
        private readonly IStore _store;
        private readonly IServiceProvider _provider;
        private readonly ISagaTriggerFactory _triggers;
        private readonly IOptions<CanalOptions> _canalOptions;
        private readonly StringEncryptorResolver _resolver;

        public StateMachine<ISagaState, ISagaTrigger> StateMachine { get; }

        public List<ISagaState> States { get; set; }
        public List<StateMachine<ISagaState, ISagaTrigger>.TriggerWithParameters<IContext>> Triggers { get; set; }
        public string ReplyTopic { get; }

        public StatelessSaga(IServiceProvider provider)
        {
            _provider = provider;
            _store = provider.GetRequiredService<IStore>();
            _log = provider.GetRequiredService<ILogger<StatelessSaga>>();
            _canalOptions = provider.GetRequiredService<IOptions<CanalOptions>>();
            _triggers = provider.GetRequiredService<ISagaTriggerFactory>();
            _resolver = provider.GetRequiredService<StringEncryptorResolver>();

            States = new List<ISagaState>();
            Triggers = new List<StateMachine<ISagaState, ISagaTrigger>.TriggerWithParameters<IContext>>();
            ReplyTopic = $"{_canalOptions.Value.TopicPrefix}.{GetType().Name}.reply";

            States.Add(new NotStarted());

            StateMachine = new StateMachine<ISagaState, ISagaTrigger>(States.First());
        }

        public virtual Task<IResult> Continue(SagaContext context)
        {
            StateMachine.OnTransitionCompleted((transition) => {
                var i = context.DataGetSingle<InternalMessage>();
                var m = i.GetData<Message>(_provider);
                var e = new SagaEvent();
                e.Id = m.GetSagaId();
                e.Content = i.Content;
                e.CorrelationId = m.GetCorrelationId();
                e.Source = transition?.Source?.ToString() ?? string.Empty;
                e.Destination = transition?.Destination?.ToString() ?? string.Empty;
                e.Expires = DateTime.UtcNow.ToUniversalTime().AddSeconds(_canalOptions.Value.SucceedMessageExpiredAfter);

                _store.StoreSagaEvent(e).GetAwaiter().GetResult();
            });

            StateMachine.OnUnhandledTrigger((state, trigger) => {
                var id = context.DataGetSingle<InternalMessage>()
                    .GetData<Message>(_provider)
                    .GetSagaId();
                _log.LogWarning($"Could not locate trigger: {trigger} for state: {state}. Saga Id: {id}");
            });

            var message = context.DataGetSingle<InternalMessage>()
                    .GetData<Message>(_provider);

            var data = message.GetData<IList<IModel>>();

            if (string.IsNullOrEmpty(message.GetSagaTrigger()))
                return Task.FromResult(new Result().Success());

            Configure(new Context(
                token: context.Token,
                provider: _provider,
                correlationId: message.GetCorrelationId()));

            var local = new Context(
                token: context.Token,
                id: Guid.NewGuid().ToString(),
                correlationId: context.CorrelationId,
                provider: _provider)
                .Add(message)
                .Add(data)
                .Add(States)
                .Add(new Kvp<string, string>("ReplyTopic", ReplyTopic))
                .Add(new Kvp<string, List<StateMachine<ISagaState, ISagaTrigger>.TriggerWithParameters<IContext>>>("Triggers", Triggers));

            StateMachine.Fire(_triggers.Create(message.GetSagaTrigger(), StateMachine), local);

            return Task.FromResult(new Result().Success());
        }

        public abstract void Configure(IContext context);

        public abstract Task Start(IContext context);
    }
}
