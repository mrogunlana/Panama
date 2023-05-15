using Panama.Canal.Models.Messaging;

namespace Panama.Canal.Interfaces
{
    public interface IProcessorFactory
    {
        IProcessor GetProducerProcessor(InternalMessage message);
        IProcessor GetConsumerProcessor(InternalMessage message);
    }
}