using Panama.Core.Interfaces;

namespace Panama.Core.Messaging.Interfaces
{
    public interface IBroker 
    {
        Task Publish(IContext context);
        
        T GetBroker<T>();
    }
}
