using Microsoft.Extensions.DependencyInjection;
using Panama.Canal.Extensions;
using Panama.Canal.Models;
using Panama.Canal.Sagas.Interfaces;
using Panama.Canal.Sagas.Models;
using Panama.Canal.Sagas.Stateless.Interfaces;

namespace Panama.Canal.Sagas.Stateless
{
    public class StatelessSagaFactory : ISagaFactory
    {
        private readonly IServiceProvider _provider;

        public StatelessSagaFactory(IServiceProvider provider)
        {
            _provider = provider;
        }
        public ISaga Get(InternalMessage message)
        {
            var local = message.GetData<Message>(_provider);
            var type = Type.GetType(local.GetSagaType());
            if (type == null)
                throw new InvalidOperationException($"Saga type: {local.GetSagaType()} could not be located.");
            
            var result = _provider.GetRequiredService(type);
            
            return (IStatelessSaga)result;
        }

        public async Task Start<S>(SagaContext context)
            where S : ISaga
        {
            var result = context.Provider.GetRequiredService<S>();
            if (result == null || result is not IStatelessSaga)
                throw new InvalidOperationException($"Saga cannot be located from type: {typeof(S).GetType().Name}");

            await result.Start(context);
        }
    }
}