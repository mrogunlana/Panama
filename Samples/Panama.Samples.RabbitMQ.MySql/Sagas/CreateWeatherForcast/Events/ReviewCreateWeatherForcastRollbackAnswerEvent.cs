using Panama.Canal.Models.Messaging;
using Panama.Canal.Sagas.Stateless.Extensions;
using Panama.Canal.Sagas.Stateless.Interfaces;
using Panama.Extensions;
using Panama.Interfaces;
using Panama.Samples.RabbitMQ.MySql.Sagas.CreateWeatherForcast.States;

namespace Panama.Samples.RabbitMQ.MySql.Sagas.CreateWeatherForcast.Events
{
    public class ReviewCreateWeatherForcastRollbackAnswerEvent : ISagaStepEvent
    {
        public Task<ISagaState> Execute(IContext context)
        {
            var message = context.DataGetSingle<Message>();

            ISagaState result = context.GetState<CreateWeatherForcastFailedNotificationSent>();

            return Task.FromResult(result);
        }
    }
}
