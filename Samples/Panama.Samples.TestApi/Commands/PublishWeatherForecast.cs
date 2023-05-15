using Panama.Canal.Channels;
using Panama.Canal.Extensions;
using Panama.Canal.Interfaces;
using Panama.Canal.Models;
using Panama.Extensions;
using Panama.Interfaces;

namespace Panama.Samples.TestApi.Commands
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
                    .Reply("foo.event.success")
                    .Post();

                await context.Bus()
                    .Channel(channel)
                    .Token(context.Token)
                    .Topic("bar.event")
                    .Group("bar")
                    .Target<DefaultTarget>()
                    .Data(models)
                    .Reply("bar.event.success")
                    .Post();

                await channel.Commit();
            }
        }
    }
}
