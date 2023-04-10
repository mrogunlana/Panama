﻿using Panama.Canal.Channels;
using Panama.Canal.Extensions;
using Panama.Canal.Interfaces;
using Panama.Canal.Sagas.Stateless.Extensions;
using Panama.Canal.Sagas.Stateless.Interfaces;
using Panama.Extensions;
using Panama.Interfaces;
using Panama.TestApi.Sagas.CreateWeatherForcast.States;
using Panama.TestApi.Sagas.CreateWeatherForcast.Triggers;

namespace Panama.TestApi.Sagas.CreateWeatherForcast.Events
{
    public class CreateWeatherForcastEvent : ISagaEvent
    {
        public async Task<ISagaState> Execute(IContext context)
        {
            //TODO: post create weather forcast message on eventbus here..
            var model = context.DataGetSingle<WeatherForecast>();
            var channels = context.Provider.GetRequiredService<IDefaultChannelFactory>();

            using (var channel = channels.CreateChannel<DefaultChannel>())
            {
                await context.Bus()
                    .Data(model)
                    .Channel(channel)
                    .Reply(context.GetReplyTopic())
                    .Token(context.Token)
                    .Topic("weatherforcast.create")
                    .Trigger<ReviewCreateWeatherForcastAnswer>()
                    .Post();

                await channel.Commit();
            }

            return context.GetState<CreateWeatherForcastRequested>();
        }
    }
}