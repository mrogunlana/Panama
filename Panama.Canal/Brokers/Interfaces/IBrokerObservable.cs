using Panama.Canal.Models.Messaging;

namespace Panama.Canal.Brokers.Interfaces
{
    public interface IBrokerObservable : IObservable<InternalMessage>
    {
        void Publish(InternalMessage message);
    }
}