﻿using Panama.Canal.Sagas.Stateless.Interfaces;
using Panama.Interfaces;
using Panama.Models;
using Panama.Canal.Sagas.Stateless.Extensions;
using Panama.Extensions;
using Panama.Samples.RabbitMQ.MySql.Sagas.CreateWeatherForcast.States;
using Panama.Samples.RabbitMQ.MySql.Sagas.CreateWeatherForcast.Triggers;

namespace Panama.Samples.RabbitMQ.MySql.Sagas.CreateWeatherForcast.Exits
{
    public class RollbackCreateWeatherForcastEventExit : ISagaStepExit
    {
        public Task<Interfaces.IResult> Execute(IContext context)
        {
            var machine = context.GetStateMachine();
            var destination = context.GetDestination();

            if (destination == context.GetState<CreateWeatherForcastRollbackRequested>())
                machine.Fire(context.GetTrigger<ReviewCreateWeatherForcastRollbackAnswer>(), context);
            else
                throw new InvalidOperationException($"Unhandled state transition for: {destination} ");

            
            return Task.FromResult(new Result().Success());
        }
    }
}
