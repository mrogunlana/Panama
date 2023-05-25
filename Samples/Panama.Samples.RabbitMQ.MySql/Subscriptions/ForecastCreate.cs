using Panama.Canal.Interfaces;
using Panama.Canal.RabbitMQ.Attributes;
using Panama.Extensions;
using Panama.Interfaces;
using Panama.Samples.RabbitMQ.MySql.Commands;
using Panama.Samples.RabbitMQ.MySql.Models;

namespace Panama.Samples.RabbitMQ.MySql.Subscriptions
{
    [RabbitTopic("forcast.create")]
    public class ForecastCreate : ISubscribe
    {
        private readonly ILogger<ForecastCreate> _log;
        private readonly IServiceProvider _provider;

        public ForecastCreate(
              IServiceProvider provider
            , ILogger<ForecastCreate> log)
        {
            _log = log;
            _provider = provider;
        }
        public async Task Event(IContext context)
        {
            var model = context.DataGetSingle<WeatherForecast>();

            await _provider.GetRequiredService<IHandler>()
                .Add(model)
                .Command<SaveWeatherForecast>()
                .Invoke();
        }
    }
}
