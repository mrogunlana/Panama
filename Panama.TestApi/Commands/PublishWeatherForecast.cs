using Panama.Canal.Channels;
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
                await channel.Post(
                    name: "foo.event",
                    group: "foo",
                    data: models.ToArray(),
                    ack: "foo.event.success",
                    nack: "foo.event.failed");

                await channel.Post<DefaultTarget, PollingInvoker>(
                    name: "bar.event",
                    group: "bar",
                    data: models.ToArray(),
                    ack: "foo.event.success",
                    nack: "foo.event.failed");

                await channel.Commit();
            }
        }
    }
}
