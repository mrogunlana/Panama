using Microsoft.Extensions.Logging;
using Panama.Canal.Extensions;
using Panama.Canal.Interfaces;
using Panama.Canal.Models.Messaging;
using Panama.Canal.Registrars;
using Panama.Canal.Sagas.Extensions;
using Panama.Canal.Sagas.Models;
using Panama.Canal.Sagas.Stateless.Interfaces;
using Panama.Extensions;
using Panama.Interfaces;
using Panama.Models;

namespace Panama.Canal.Processors
{
    public class SagaConsumerProcessor : IProcessor
    {
        private readonly IStore _store;
        private readonly ILogger<SagaConsumerProcessor> _log;
        private readonly ISagaFactory _factory;
        private readonly IServiceProvider _provider;

        public SagaConsumerProcessor(
              IStore store
            , ISagaFactory factory
            , IServiceProvider provider
            , ILogger<SagaConsumerProcessor> log)
        {
            _log = log;
            _store = store;
            _factory = factory;
            _provider = provider;
        }
        public async Task<IResult> Execute(IContext context)
        {
            var message = context.DataGetSingle<InternalMessage>();
            if (message == null)
                throw new InvalidOperationException("Message headers cannot be found.");

            message.SetStatus(MessageStatus.Scheduled);

            var received = await _store.StoreReceivedMessage(
                message: message)
                .ConfigureAwait(false);

            var data = received.GetData<Message>(_provider);
            var saga = _factory.Get(received);
            if (saga == null)
                return new Result().Success().Add($"No saga located for message: {received.Id}, saga: {data.GetSagaType()}");

            return await saga.Continue(new SagaContext(_provider, context)
                .Data(received)
                .Token(context.Token));
        }
    }
}