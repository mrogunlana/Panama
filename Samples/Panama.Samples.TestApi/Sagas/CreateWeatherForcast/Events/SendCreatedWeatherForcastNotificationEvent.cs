using Panama.Canal.Sagas.Stateless.Extensions;
using Panama.Canal.Sagas.Stateless.Interfaces;
using Panama.Interfaces;
using Panama.Samples.TestApi.Sagas.CreateWeatherForcast.States;

namespace Panama.Samples.TestApi.Sagas.CreateWeatherForcast.Events
{
    public class SendCreatedWeatherForcastNotificationEvent : ISagaStepEvent
    {
        public Task<ISagaState> Execute(IContext context)
        {
            //TODO: send success notification here..

            ISagaState result = context.GetState<CreateWeatherForcastComplete>();

            return Task.FromResult(result);
        }
    }
}
