using Microsoft.Extensions.DependencyInjection;
using Panama.Canal.Interfaces.Sagas;

namespace Panama.Canal.Sagas
{
    public class SagaStateFactory : ISagaStateFactory
    {
        private readonly IServiceProvider _provider; 
        public SagaStateFactory(IServiceProvider provider)
        {
            _provider = provider;   
        }
        public ISagaState Create<T>() where T : ISagaState
        {
            return _provider.GetRequiredService<T>();
        }
    }
}