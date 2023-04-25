using Panama.Canal.Extensions;
using Panama.Canal.Models.Messaging;
using Panama.Canal.Sagas.Stateless.Extensions;
using Panama.Canal.Sagas.Stateless.Interfaces;
using Panama.Extensions;
using Panama.Interfaces;
using Panama.TestApi.Sagas.CreateWeatherForcast.States;

namespace Panama.TestApi.Sagas.CreateWeatherForcast.Events
{
    public class ReviewCreateWeatherForcastAnswerEvent : ISagaEvent
    {
        public Task<ISagaState> Execute(IContext context)
        {
            var message = context.DataGetSingle<Message>();

            ISagaState result = message.HasException()
                ? context.GetState<CreateWeatherForcastFailed>()
                : context.GetState<CreateWeatherForcastCreated>();

            return Task.FromResult(result);
        }
    }
}
