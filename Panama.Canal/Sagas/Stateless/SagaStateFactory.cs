using Microsoft.Extensions.DependencyInjection;
using Panama.Canal.Sagas.Stateless.Interfaces;

namespace Panama.Canal.Sagas.Stateless
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