using Panama.Canal.Sagas.Stateless.Extensions;
using Panama.Canal.Sagas.Stateless.Interfaces;
using Panama.Interfaces;
using Panama.TestApi.Sagas.CreateWeatherForcast.States;

namespace Panama.TestApi.Sagas.CreateWeatherForcast.Events
{
    public class PublishCreateWeatherForcastResultsEvent : ISagaStepEvent
    {
        public Task<ISagaState> Execute(IContext context)
        {
            //TODO: publish weather forcast to read models here..

            ISagaState result = context.GetState<CreateWeatherForcastPublishResultsComplete>();

            return Task.FromResult(result);
        }
    }
}
