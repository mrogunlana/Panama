using Panama.Canal.Models;
using Panama.Interfaces;

namespace Panama.Canal.Sagas.Stateless.Interfaces
{
    public interface ISagaFactory
    {
        Task<ISaga> Start<S>(SagaContext context) where S : ISaga;
        Task<ISaga> Get(InternalMessage message);
    }
}