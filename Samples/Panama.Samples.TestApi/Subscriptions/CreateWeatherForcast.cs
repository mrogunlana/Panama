using Panama.Canal.Attributes;
using Panama.Canal.Interfaces;
using Panama.Extensions;
using Panama.Interfaces;
using Panama.Models;

namespace Panama.Samples.TestApi.Subscriptions
{
    [DefaultTopic("weatherforcast.create")]
    public class CreateWeatherForcast : ISubscribe
    {
        private readonly ILogger<CreateWeatherForcast> _log;
        private readonly IServiceProvider _provider;

        public CreateWeatherForcast(
              IServiceProvider provider
            , ILogger<CreateWeatherForcast> log)
        {
            _log = log;
            _provider = provider;
        }
        public Task Event(IContext context)
        {
            var kvp = new Kvp<string, string>("subscription.name", nameof(CreateWeatherForcast));

            context.Add(kvp);
            context.Add(new WeatherForecast() { Date = DateTime.UtcNow, Summary = "New Forcast Created!", TemperatureC = 45 });

            _log.LogInformation($"{typeof(CreatedFoo)} subscriber executed.");

            return Task.CompletedTask;
        }
    }
}
