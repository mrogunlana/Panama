using Panama.Canal.Interfaces.Sagas;
using Panama.Canal.Models;

namespace Panama.Canal.Sagas
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