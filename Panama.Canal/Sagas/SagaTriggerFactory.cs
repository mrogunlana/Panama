using Microsoft.Extensions.DependencyInjection;
using Panama.Canal.Interfaces.Sagas;

namespace Panama.Canal.Sagas
{
    public class SagaTriggerFactory : ISagaTriggerFactory
    {
        private readonly IServiceProvider _provider; 
        public SagaTriggerFactory(IServiceProvider provider)
        {
            _provider = provider;   
        }
        public ISagaTrigger Get<T>() where T : ISagaTrigger
        {
            return _provider.GetRequiredService<T>();
        }
    }
}