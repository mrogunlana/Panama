using Microsoft.Extensions.DependencyInjection;
using Panama.Canal.Extensions;
using Panama.Canal.Interfaces;
using Panama.Canal.Models;

namespace Panama.Canal.Processors
{
    public class ProcessorFactory : IProcessorFactory
    {
        private readonly IServiceProvider _provider;

        public ProcessorFactory(
            IServiceProvider provider)
        {
            _provider = provider;
        }

        public IProcessor GetProducerProcessor(InternalMessage message)
        {
            return _provider.GetRequiredService<DefaultProducerProcessor>();
        }

        public IProcessor GetConsumerProcessor(InternalMessage message)
        {
            var data = message.GetData<Message>(_provider);
            if (data.IsSagaParticipant())
                return _provider.GetRequiredService<SagaConsumerProcessor>();

            return _provider.GetRequiredService<DefaultConsumerProcessor>();
        }

    }
}