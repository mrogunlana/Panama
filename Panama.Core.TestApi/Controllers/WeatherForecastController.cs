using Microsoft.AspNetCore.Mvc;
using Panama.Core.Extensions;
using Panama.Core.Interfaces;
using Panama.Core.Invokers;
using Panama.Core.Tests.Commands;

namespace Panama.Core.TestApi.Controllers
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
                .Set<ScopedInvoker>()
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
                .Invoke();

            return result.Data.DataGet<WeatherForecast>();
        }
    }
}