using Panama.Canal.Interfaces;
using Panama.Canal.RabbitMQ.Attributes;
using Panama.Extensions;
using Panama.Interfaces;
using Panama.Samples.RabbitMQ.MySql.Controllers;
using Panama.Samples.RabbitMQ.MySql.Models;

namespace Panama.Samples.RabbitMQ.MySql.Subscriptions
{
    [RabbitTopic("forecast.created")]
    public class ForecastCreated : ISubscribe
    {
        private readonly ILogger<ForecastCreated> _log;
        private readonly State _state;
        private readonly IServiceProvider _provider;

        public ForecastCreated(
              State state
            , IServiceProvider provider
            , ILogger<ForecastCreated> log)
        {
            _log = log;
            _state = state;
            _provider = provider;
        }
        public Task Event(IContext context)
        {
            var model = context.DataGetSingle<WeatherForecast>();

            _state.Forecasts.TryAdd(model.Summary, model);

            return Task.CompletedTask;
        }
    }
}
