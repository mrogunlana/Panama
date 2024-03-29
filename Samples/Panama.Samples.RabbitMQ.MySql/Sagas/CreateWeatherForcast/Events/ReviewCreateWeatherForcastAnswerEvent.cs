﻿using Panama.Canal.Extensions;
using Panama.Canal.Models.Messaging;
using Panama.Canal.Sagas.Stateless.Extensions;
using Panama.Canal.Sagas.Stateless.Interfaces;
using Panama.Extensions;
using Panama.Interfaces;
using Panama.Samples.RabbitMQ.MySql.Sagas.CreateWeatherForcast.States;

namespace Panama.Samples.RabbitMQ.MySql.Sagas.CreateWeatherForcast.Events
{
    public class ReviewCreateWeatherForcastAnswerEvent : ISagaStepEvent
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
