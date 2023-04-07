﻿using Panama.Canal.Sagas.Stateless.Extensions;
using Panama.Canal.Sagas.Stateless.Interfaces;
using Panama.Interfaces;
using Panama.TestApi.Sagas.CreateWeatherForcast.States;

namespace Panama.TestApi.Sagas.CreateWeatherForcast.Events
{
    public class RollbackCreateWeatherForcastEvent : ISagaEvent
    {
        public Task<ISagaState> Execute(IContext context)
        {
            //TODO: rollback weather forcast here..

            ISagaState result = context.GetState<CreateWeatherForcastRollbackRequested>();

            return Task.FromResult(result);
        }
    }
}
