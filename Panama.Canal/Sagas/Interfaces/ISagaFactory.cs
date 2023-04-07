using Panama.Canal.Models;
using Panama.Canal.Sagas.Interfaces;
using Panama.Canal.Sagas.Models;

namespace Panama.Canal.Sagas.Stateless.Interfaces
{
    public interface ISagaFactory
    {
        Task Start<S>(SagaContext context) where S : ISaga;
        ISaga Get(InternalMessage message);
    }
}