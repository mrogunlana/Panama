using Microsoft.AspNetCore.Mvc;
using Panama.Core.Interfaces;
using Panama.Core.Invokers;
using Panama.Core.Tests.Commands;

namespace Panama.Core.TestApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

        private readonly Microsoft.Extensions.Logging.ILogger<WeatherForecastController> _logger;
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
                .Command<SerialCommand1>()
                .Invoke();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
}