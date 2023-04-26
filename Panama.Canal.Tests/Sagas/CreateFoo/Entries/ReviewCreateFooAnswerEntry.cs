using Microsoft.Extensions.DependencyInjection;
using Panama.Canal.Extensions;
using Panama.Canal.Models.Messaging;
using Panama.Canal.Sagas.Stateless.Extensions;
using Panama.Canal.Sagas.Stateless.Interfaces;
using Panama.Canal.Tests.Models;
using Panama.Canal.Tests.Sagas.CreateFoo.States;
using Panama.Canal.Tests.Sagas.CreateFoo.Triggers;
using Panama.Extensions;
using Panama.Interfaces;
using Panama.Models;

namespace Panama.Canal.Tests.Sagas.CreateFoo.Events
{
    public class ReviewCreateFooAnswerEntry : ISagaEntry
    {
        public Task<IResult> Execute(IContext context)
        {
            var message = context.DataGetSingle<Message>();

            var trigger = message.HasException()
                ? context.GetTrigger<RollbackCreateFoo>()
                : context.GetTrigger<CompleteNewFoo>();

            var result = new Result()
                .Success()
                .AddKvp("Trigger", trigger);

            var state = context.Provider.GetService<State>();
            if (state == null)
                return Task.FromResult(result);

            state.Data.Add(new Kvp<string, string>("saga.event.name", nameof(ReviewCreateFooAnswerEvent)));

            return Task.FromResult(result);
        }
    }
}
