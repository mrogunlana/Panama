using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Panama.Canal.Extensions;
using Panama.Canal.Interfaces;
using Panama.Canal.Models.Messaging;
using Panama.Canal.Models.Options;
using Panama.Canal.Sagas.Models;
using Panama.Canal.Sagas.Stateless.Extensions;
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
        private StateMachine<ISagaState, ISagaTrigger> _machine;
        public StateMachine<ISagaState, ISagaTrigger> StateMachine => _machine;

        public List<ISagaState> States { get; set; }
        public List<StateMachine<ISagaState, ISagaTrigger>.TriggerWithParameters<IContext>> Triggers { get; set; }
        public string ReplyTopic { get; }
        public string ReplyGroup { get; set; }
        public virtual Type? Target { get; }

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
            ReplyTopic = string.Join(".", new List<string>() { _canalOptions.Value.TopicPrefix ?? string.Empty, GetType().Name, "reply" }.Where(x => !string.IsNullOrEmpty(x)));
            ReplyGroup = _canalOptions.Value.DefaultGroup;
            States.Add(new NotStarted());

            _machine = new StateMachine<ISagaState, ISagaTrigger>(States.First());
        }

        public abstract void Init(IContext context);

        public virtual Task<IResult> Continue(SagaContext context)
        {
            var message = context.DataGetSingle<InternalMessage>()
                    .GetData<Message>(_provider);

            if (string.IsNullOrEmpty(message.GetSagaTrigger()))
                return Task.FromResult(new Result().Success());
            
            Init(context);

            _machine = new StateMachine<ISagaState, ISagaTrigger>(States.Get(message.GetSagaStateType()) ?? States.First());

            var local = new Context(
                token: context.Token,
                id: Guid.NewGuid().ToString(),
                correlationId: message.GetCorrelationId(),
                provider: _provider)
                .Add(message)
                .Add(context.DataGetSingle<InternalMessage>())
                .Add(message.GetData<IList<IModel>>())
                .Add(new Kvp<string, string>("CorrelationId", message.GetCorrelationId()))
                .Add(new Kvp<string, string>("SagaId", message.GetSagaId()))
                .Add(new Kvp<string, string>("SagaType", GetType().AssemblyQualifiedName!))
                .Add(new Kvp<string, string>("ReplyTopic", ReplyTopic))
                .Add(new Kvp<string, List<StateMachine<ISagaState, ISagaTrigger>.TriggerWithParameters<IContext>>>("Triggers", Triggers))
                .Add(new Kvp<string, List<ISagaState>>("States", States));

            Configure(local);

            var trigger = Triggers.Get(message.GetSagaTriggerType());
            if (trigger == null)
                return Task.FromResult(new Result().Success());

            StateMachine.Fire(trigger, local);

            return Task.FromResult(new Result().Success());
        }

        public virtual void Configure(IContext context)
        {
            StateMachine.OnTransitionCompleted((transition) => {
                var id = context.KvpGetSingle<string, string>("SagaId");
                var correlationId = context.KvpGetSingle<string, string>("CorrelationId");
                var e = new SagaEvent();

                var i = context?.DataGetSingle<InternalMessage>();

                e.Id = id;
                e.Content = i?.Content;
                e.CorrelationId = correlationId;
                e.Source = transition?.Source?.GetType().AssemblyQualifiedName ?? string.Empty;
                e.Destination = transition?.Destination?.GetType().AssemblyQualifiedName ?? string.Empty;
                e.Expires = DateTime.UtcNow.AddSeconds(_canalOptions.Value.SucceedMessageExpiredAfter);
                e.Trigger = transition?.Trigger.GetType().AssemblyQualifiedName ?? string.Empty;

                _store.StoreSagaEvent(e).GetAwaiter().GetResult();
            });

            StateMachine.OnUnhandledTrigger((state, trigger) => {
                var id = context.DataGetSingle<InternalMessage>()
                    .GetData<Message>(_provider)
                    .GetSagaId();
                _log.LogWarning($"Could not locate trigger: {trigger} for state: {state}. Saga Id: {id}");
            });
        }

        public abstract Task Start(IContext context);
    }
}
