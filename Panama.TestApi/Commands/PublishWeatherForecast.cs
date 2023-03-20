using Panama.Extensions;
using Panama.Interfaces;
using Panama.TestApi;
using Panama.Canal.Extensions;

namespace Panama.Tests.Commands
{
    public class PublishWeatherForecast : ICommand
    {
        public async Task Execute(IContext context)
        {
            var models = context.Data.DataGet<WeatherForecast>();

            await context.Bus()
                .Instance("")
                .Topic("")
                .Group("")
                .Data(models)
                .Ack("")
                .Nack("")
                .Publish();
        }
    }
}
