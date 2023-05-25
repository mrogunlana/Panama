using Microsoft.Extensions.DependencyInjection;
using Panama.Canal.Sagas.Stateless.Extensions;
using Panama.Canal.Sagas.Stateless.Interfaces;
using Panama.Canal.Tests.Modules.Models;
using Panama.Canal.Tests.Sagas.CreateFoo.States;
using Panama.Interfaces;
using Panama.Models;

namespace Panama.Canal.Tests.Modules.Sagas.CreateFoo.Events
{
    public class FooCompletedEvent : ISagaStepEvent
    {
        public Task<ISagaState> Execute(IContext context)
        {
            ISagaState result = context.GetState<CreateFooComplete>();
            var state = context.Provider.GetService<State>();
            if (state == null)
                return Task.FromResult(result);

            state.Data.Add(new Kvp<string, string>("saga.event.name", nameof(FooCompletedEvent)));

            return Task.FromResult(result);
        }
    }
}
