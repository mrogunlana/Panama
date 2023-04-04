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

        public IProcessor GetProcessor(InternalMessage message)
        {
            var data = message.GetData<Message>(_provider);
            if (data.IsSagaParticipant())
                return _provider.GetRequiredService<SagaProcessor>();

            return _provider.GetRequiredService<DefaultProcessor>();
        }
    }
}