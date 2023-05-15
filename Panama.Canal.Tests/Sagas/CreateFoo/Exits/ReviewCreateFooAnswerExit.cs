using Microsoft.Extensions.DependencyInjection;
using Panama.Canal.Sagas.Stateless.Extensions;
using Panama.Canal.Sagas.Stateless.Interfaces;
using Panama.Canal.Tests.Sagas.CreateFoo.States;
using Panama.Canal.Tests.Sagas.CreateFoo.Triggers;
using Panama.Extensions;
using Panama.Interfaces;
using Panama.Models;

namespace Panama.Canal.Tests.Sagas.CreateFoo.Exits
{
    public class ReviewCreateFooAnswerExit : ISagaStepExit
    {
        public Task<IResult> Execute(IContext context)
        {
            var machine = context.GetStateMachine();
            var destination = context.GetDestination();

            if (destination == context.GetState<CreateFooCreated>())
                machine.Fire(context.GetTrigger<CompleteNewFoo>(), context);
            else if (destination == context.GetState<CreateFooFailed>())
                machine.Fire(context.GetTrigger<RollbackCreateFoo>(), context);
            else
                throw new InvalidOperationException($"Unhandled state transition for: {destination} ");

            var state = context.Provider.GetService<Models.State>();
            if (state == null)
                return Task.FromResult(new Result().Success());

            state.Data.Add(new Kvp<string, string>("saga.exit.name", nameof(ReviewCreateFooAnswerExit)));

            return Task.FromResult(new Result().Success());
        }
    }
}
