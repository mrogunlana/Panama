using Microsoft.AspNetCore.Mvc;
using Panama.Canal.Extensions;
using Panama.Extensions;
using Panama.Interfaces;
using Panama.Invokers;
using Panama.Tests.Commands;


namespace Panama.TestApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IServiceProvider _provider;

        public WeatherForecastController(ILogger<WeatherForecastController> logger
            , IServiceProvider provider)
        {
            _logger = logger;
            _provider = provider;
        }

        [HttpGet("/")]
        public async Task<IEnumerable<WeatherForecast>> GetAsync()
        {
            var result = await _provider
                .GetRequiredService<IHandler>()
                .AddKvp(1, "Freezing")
                .AddKvp(2, "Bracing")
                .AddKvp(3, "Chilly")
                .AddKvp(4, "Cool")
                .AddKvp(5, "Mild")
                .AddKvp(6, "Warm")
                .AddKvp(7, "Balmy")
                .AddKvp(8, "Hot")
                .AddKvp(9, "Sweltering")
                .AddKvp(10, "Scorching")
                .Command<SerialCommand1>()
                .Command<AddRandomWeatherForecast>()
                .Command<PublishWeatherForecast>()
                .Invoke();

            return result.Data.DataGet<WeatherForecast>();
        }
    }
}