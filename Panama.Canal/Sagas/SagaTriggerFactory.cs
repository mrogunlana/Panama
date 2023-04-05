using Microsoft.Extensions.DependencyInjection;
using Panama.Canal.Interfaces.Sagas;
using Panama.Canal.Models;
using Panama.Models;
using Quartz.Impl.AdoJobStore.Common;

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

        public ISagaTrigger Get(string type)
        {
            var result = Type.GetType(type);
            if (result == null)
                throw new InvalidOperationException($"Header: {Headers.SagaTrigger} type cannot be found.");

            return (ISagaTrigger)_provider.GetRequiredService(result);
        }
    }
}