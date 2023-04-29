using Microsoft.Extensions.DependencyInjection;
using Panama.Canal.Sagas.Models;
using Panama.Canal.Sagas.Stateless.Interfaces;
using Panama.Extensions;
using Panama.Interfaces;
using Panama.Models;
using Stateless;

namespace Panama.Canal.Sagas.Stateless.Extensions
{
    public static class SagaContextExtensions
    {
        public static async Task Start(this SagaContext context)
        {
            if (context.Type == null)
                throw new InvalidOperationException($"Saga cannot be located from type: {context?.Type?.Name}");

            var result = context.Provider.GetRequiredService(context.Type);
            if (result is not IStatelessSaga)
                throw new InvalidOperationException($"Saga cannot be located from type: {result.GetType().Name}");

            var saga = result as IStatelessSaga;
            if (saga == null)
                throw new InvalidOperationException($"Saga cannot be converted from type: {result.GetType().Name}");

            var local = new Context(
                token: context.Token,
                id: Guid.NewGuid().ToString(),
                correlationId: context.CorrelationId,
                provider: context.Provider)
            .Add(context.Data)
                .Add(new Kvp<string, string>("CorrelationId", context.CorrelationId))
                .Add(new Kvp<string, string>("SagaId", Guid.NewGuid().ToString()))
                .Add(new Kvp<string, string>("SagaType", saga.GetType().AssemblyQualifiedName!))
                .Add(new Kvp<string, string>("ReplyTopic", saga.ReplyTopic))
                .Add(new Kvp<string, List<StateMachine<ISagaState, ISagaTrigger>.TriggerWithParameters<IContext>>>("Triggers", saga.Triggers))
                .Add(new Kvp<string, List<ISagaState>>("States", saga.States));

            saga.Init(local);
            saga.Configure(local);

            await saga.Start(local);
        }
    }
}
