using Microsoft.Extensions.DependencyInjection;
using Panama.Canal.Extensions;
using Panama.Canal.Models.Messaging;
using Panama.Canal.Sagas.Stateless.Extensions;
using Panama.Canal.Sagas.Stateless.Interfaces;
using Panama.Canal.Tests.Modules.Models;
using Panama.Canal.Tests.Sagas.CreateFoo.States;
using Panama.Extensions;
using Panama.Interfaces;
using Panama.Models;

namespace Panama.Canal.Tests.Modules.Sagas.CreateFoo.Events
{
    public class ReviewCreateFooAnswerEvent : ISagaStepEvent
    {
        public Task<ISagaState> Execute(IContext context)
        {
            var message = context.DataGetSingle<Message>();

            ISagaState result = message.HasException()
                ? context.GetState<CreateFooFailed>()
                : context.GetState<CreateFooCreated>();

            var state = context.Provider.GetService<State>();
            if (state == null)
                return Task.FromResult(result);

            state.Data.Add(new Kvp<string, string>("saga.event.name", nameof(ReviewCreateFooAnswerEvent)));

            return Task.FromResult(result);
        }
    }
}
