using Panama.Canal.Models;

namespace Panama.Canal.Interfaces
{
    public interface IProcessorFactory
    {
        IProcessor GetProcessor(InternalMessage message);
    }
}