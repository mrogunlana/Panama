using Microsoft.Extensions.DependencyInjection;
using Panama.Canal.Sagas.Stateless.Interfaces;

namespace Panama.Canal.Sagas.Stateless
{
    public class StatelessSagaStateFactory : ISagaStateFactory
    {
        private readonly IServiceProvider _provider;
        public StatelessSagaStateFactory(IServiceProvider provider)
        {
            _provider = provider;
        }
        public ISagaState Create<T>() where T : ISagaState
        {
            return _provider.GetRequiredService<T>();
        }
    }
}