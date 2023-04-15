using Panama.Canal.Models;

namespace Panama.Canal.Brokers.Interfaces
{
    public interface IDefaultObservable : IObservable<InternalMessage>
    {
        void Publish(InternalMessage message);
    }
}