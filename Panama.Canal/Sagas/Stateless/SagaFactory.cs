using Panama.Canal.Models;
using Panama.Canal.Sagas.Stateless.Interfaces;

namespace Panama.Canal.Sagas.Stateless
{
    public class SagaFactory : ISagaFactory
    {
        public Task<ISaga> Get(InternalMessage message)
        {
            throw new NotImplementedException();
        }

        public Task<ISaga> Start<S>(SagaContext context) where S : ISaga
        {
            throw new NotImplementedException();
        }
    }
}