﻿using Panama.Canal.Channels;
using Panama.Canal.Extensions;
using Panama.Canal.Interfaces;
using Panama.Canal.Invokers;
using Panama.Canal.Models;
using Panama.Extensions;
using Panama.Interfaces;
using Panama.TestApi;

namespace Panama.Tests.Commands
{
    public class PublishWeatherForecast : ICommand
    {
        private readonly IDefaultChannelFactory _factory;

        public PublishWeatherForecast(IDefaultChannelFactory factory)
        {
            _factory = factory;
        }
        public async Task Execute(IContext context)
        {
            var models = context.Data.DataGet<WeatherForecast>();

            using (var channel = _factory.CreateChannel<DefaultChannel>())
            {
                await context.Bus()
                    .Channel(channel)
                    .Token(context.Token)
                    .Topic("foo.event")
                    .Group("foo")
                    .Data(models)
                    .Ack("foo.event.success")
                    .Nack("foo.event.failed")
                    .Post();

                await context.Bus()
                    .Channel(channel)
                    .Token(context.Token)
                    .Topic("bar.event")
                    .Group("bar")
                    .Target<DefaultTarget>()
                    .Invoker<PollingPublisherInvoker>()
                    .Data(models)
                    .Ack("bar.event.success")
                    .Nack("bar.event.failed")
                    .Post();

                await channel.Commit();
            }
        }
    }
}
