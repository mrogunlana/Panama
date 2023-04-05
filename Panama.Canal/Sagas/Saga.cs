using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Panama.Canal.Extensions;
using Panama.Canal.Interfaces;
using Panama.Canal.Models;
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

        public StateMachine<string, string>? StateMachine { get; private set;  }
        
        public Saga(
            ILogger log,
            IStore store,
            IServiceProvider provider,
            IOptions<CanalOptions> canalOptions)
        {
            _log = log;
            _store = store;
            _provider = provider;
            _canalOptions = canalOptions;
        }

        public Task<IResult> Continue(SagaContext context)
        {
            StateMachine = new StateMachine<string, string>(context
                .DataGetSingle<InternalMessage>()
                .GetData<Message>(_provider)
                .GetName());

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

            Configure();

            return Task.FromResult(new Result().Success());
        }

        public abstract Task Configure();
    }
}
