using Microsoft.AspNetCore.Mvc;
using Panama.Canal.Models.Descriptors;
using Panama.Interfaces;
using Panama.Samples.RabbitMQ.MySql.Commands;
using Panama.Samples.RabbitMQ.MySql.Models;
using System.Collections.Concurrent;

namespace Panama.Samples.RabbitMQ.MySql.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly IServiceProvider _provider;
        private readonly State _state;
        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(
            State state,
            IServiceProvider provider,
            ILogger<WeatherForecastController> logger)
        {
            _state = state;
            _logger = logger;
            _provider = provider;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            var state = _provider.GetRequiredService<State>();
            if (state == null)
                return new List<WeatherForecast>(); 

            return state.Forecasts.Values.ToArray();
        }

        [HttpPost]
        public async Task<Interfaces.IResult> Post([FromBody]WeatherForecast forecast)
        {
            var test = _provider.GetRequiredService<SubscriberDescriptions>();

            return await _provider.GetRequiredService<IHandler>()
                .Add(forecast)
                .Command<SaveWeatherForecast>()
                .Invoke();
        }
    }
}