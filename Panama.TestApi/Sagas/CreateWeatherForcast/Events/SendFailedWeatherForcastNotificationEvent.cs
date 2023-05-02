using Panama.Canal.Sagas.Stateless.Extensions;
using Panama.Canal.Sagas.Stateless.Interfaces;
using Panama.Interfaces;
using Panama.TestApi.Sagas.CreateWeatherForcast.States;

namespace Panama.TestApi.Sagas.CreateWeatherForcast.Events
{
    public class SendFailedWeatherForcastNotificationEvent : ISagaStepEvent
    {
        public Task<ISagaState> Execute(IContext context)
        {
            //TODO: send success notification here..

            ISagaState result = context.GetState<CreateWeatherForcastComplete>();

            return Task.FromResult(result);
        }
    }
}
