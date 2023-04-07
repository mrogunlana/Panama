using Microsoft.Extensions.Logging;
using Panama.Canal.Extensions;
using Panama.Canal.Interfaces;
using Panama.Canal.Models;
using Panama.Canal.Sagas.Stateless.Extensions;
using Panama.Canal.Sagas.Stateless.Interfaces;
using Panama.Extensions;
using Panama.Interfaces;
using Panama.Models;

namespace Panama.Canal.Processors
{
    public class SagaProcessor : IProcessor
    {
        private readonly ILogger<SagaProcessor> _log;
        private readonly ISagaFactory _factory;
        private readonly IServiceProvider _provider;

        public SagaProcessor(
              ISagaFactory factory
            , IServiceProvider provider
            , ILogger<SagaProcessor> log)
        {
            _log = log;
            _factory = factory;
            _provider = provider;
        }
        public async Task<IResult> Execute(IContext context)
        {
            var message = context.DataGetSingle<InternalMessage>();
            if (message == null)
                throw new InvalidOperationException("Message headers cannot be found.");
            
            var data = message.GetData<Message>(_provider);
            var saga = await _factory.Get(message);
            if (saga == null)
                return new Result().Success().Add($"No saga located for message: {message.Id}, saga: {data.GetSagaType()}");

            return await saga.Continue(new SagaContext(_provider, context)
                .Data(message)
                .Token(context.Token));
        }
    }
}