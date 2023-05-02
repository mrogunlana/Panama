using Panama.Canal.Sagas.Stateless.Interfaces;
using Panama.Interfaces;
using Panama.Models;
using Panama.Canal.Sagas.Stateless.Extensions;
using Panama.Extensions;
using Panama.TestApi.Sagas.CreateWeatherForcast.States;
using Panama.TestApi.Sagas.CreateWeatherForcast.Triggers;

namespace Panama.TestApi.Sagas.CreateWeatherForcast.Exits
{
    public class ReviewCreateWeatherForcastRollbackAnswerExit : ISagaStepExit
    {
        public Task<Interfaces.IResult> Execute(IContext context)
        {
            var machine = context.GetStateMachine();
            var destination = context.GetDestination();

            if (destination == context.GetState<CreateWeatherForcastFailedNotificationSent>())
                machine.Fire(context.GetTrigger<SendCreatedWeatherForcastNotification>(), context);
            else
                throw new InvalidOperationException($"Unhandled state transition for: {destination} ");

            
            return Task.FromResult(new Result().Success());
        }
    }
}
