﻿using Microsoft.Extensions.Options;
using MySqlConnector;
using Panama.Canal.Extensions;
using Panama.Canal.Interfaces;
using Panama.Canal.MySQL.Extensions;
using Panama.Canal.MySQL.Models;
using Panama.Canal.Sagas.Stateless.Extensions;
using Panama.Canal.Sagas.Stateless.Interfaces;
using Panama.Extensions;
using Panama.Interfaces;
using Panama.Samples.RabbitMQ.MySql.Models;
using Panama.Samples.RabbitMQ.MySql.Sagas.CreateWeatherForcast.States;
using Panama.Samples.RabbitMQ.MySql.Sagas.CreateWeatherForcast.Triggers;
using System.Data;

namespace Panama.Samples.RabbitMQ.MySql.Sagas.CreateWeatherForcast.Events
{
    public class CreateWeatherForcastEvent : ISagaStepEvent
    {
        public async Task<ISagaState> Execute(IContext context)
        {
            var model = context.DataGetSingle<WeatherForecast>();
            var channels = context.Provider.GetRequiredService<IGenericChannelFactory>();
            var options = context.Provider.GetRequiredService<IOptions<MySqlOptions>>().Value;

            using (var connection = new MySqlConnection(options.GetConnectionString()))
            using (var channel = channels.CreateChannel<IDbConnection, IDbTransaction>(connection, context.Token))
            {
                await context.Bus()
                    .Data(model)
                    .Channel(channel)
                    .Token(context.Token)
                    .Topic("forcast.create")
                    .Trigger<ReviewCreateWeatherForcastAnswer>()
                    .Post();

                await channel.Commit();
            }

            return context.GetState<CreateWeatherForcastRequested>();
        }
    }
}
