using Panama.Canal.Sagas.Stateless.Extensions;
using Panama.Canal.Sagas.Stateless.Interfaces;
using Panama.Interfaces;
using Panama.TestApi.Sagas.CreateWeatherForcast.States;

namespace Panama.TestApi.Sagas.CreateWeatherForcast.Events
{
    public class ReviewCreateWeatherForcastRollbackAnswerEvent : ISagaEvent
    {
        public Task<ISagaState> Execute(IContext context)
        {
            //TODO: send review rollback response here..
            //but this is a repeatable trasaction so the response doesn't matter in this case
            //so we just send the failed notification instead

            ISagaState result = context.GetState<CreateWeatherForcastFailedNotificationSent>();

            return Task.FromResult(result);
        }
    }
}
