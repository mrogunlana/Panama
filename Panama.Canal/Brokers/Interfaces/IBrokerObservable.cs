using Panama.Canal.Models;

namespace Panama.Canal.Brokers.Interfaces
{
    public interface IBrokerObservable : IObservable<InternalMessage>
    {
        void Publish(InternalMessage message);
    }
}