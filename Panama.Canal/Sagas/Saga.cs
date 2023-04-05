using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Panama.Canal.Extensions;
using Panama.Canal.Interfaces;
using Panama.Canal.Interfaces.Sagas;
using Panama.Canal.Models;
using Panama.Canal.Models.Sagas;
using Panama.Extensions;
using Panama.Interfaces;
using Panama.Models;
using Stateless;

namespace Panama.Canal.Sagas
{
    public abstract class Saga : ISaga
    {
        private readonly ILogger _log;
        private readonly IStore _store;
        private readonly IServiceProvider _provider;
        private readonly IOptions<CanalOptions> _canalOptions;

        public StateMachine<ISagaState, ISagaTrigger> StateMachine { get; }
        public List<ISagaState> States { get; set; }
        public List<ISagaTrigger> Triggers { get; set; }

        public Saga(IServiceProvider provider)
        {
            _provider = provider;
            _store = provider.GetRequiredService<IStore>();
            _log = provider.GetRequiredService<ILogger<Saga>>();
            _canalOptions = provider.GetRequiredService<IOptions<CanalOptions>>(); ;

            States = new List<ISagaState>();
            Triggers = new List<ISagaTrigger>();

            States.Add(new NotStarted());

            StateMachine = new StateMachine<ISagaState, ISagaTrigger>(States.First());
        }

        public virtual Task<IResult> Continue(SagaContext context)
        {
            StateMachine.OnTransitionCompletedAsync(async (transition) => {
                var i = context.DataGetSingle<InternalMessage>();
                var m = i.GetData<Message>(_provider);
                var e = new SagaEvent();
                e.Id = m.GetSagaId();
                e.Content = i.Content;
                e.CorrelationId = m.GetCorrelationId();
                e.Source = transition?.Source?.ToString() ?? string.Empty;
                e.Destination = transition?.Destination?.ToString() ?? string.Empty;
                e.Expires = (DateTime.UtcNow.ToUniversalTime()).AddSeconds(_canalOptions.Value.SucceedMessageExpiredAfter);

                await _store.StoreSagaEvent(e);
            });

            StateMachine.OnUnhandledTrigger((state, trigger) => {
                var id = context.DataGetSingle<InternalMessage>()
                    .GetData<Message>(_provider)
                    .GetSagaId();
                _log.LogWarning($"Could not locate trigger: {trigger} for state: {state}. Saga Id: {id}");
            });

            var message = context.DataGetSingle<InternalMessage>()
                    .GetData<Message>(_provider);

            Configure(new Context(
                token: context.Token, 
                provider: _provider, 
                correlationId: message.GetCorrelationId()).Add(message));

            StateMachine.Fire(message.GetSagaTrigger(_provider));

            return Task.FromResult(new Result().Success());
        }

        public abstract Task Configure(IContext context);

        public abstract Task<IResult> Start();
    }
}
