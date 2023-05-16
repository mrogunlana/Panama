using Panama.Interfaces;
using System.Collections.Concurrent;

namespace Panama.Samples.RabbitMQ.MySql.Models
{
    public class State : IModel
    {
        public State()
        {
            Forecasts = new ConcurrentDictionary<string, WeatherForecast>();
        }
        public ConcurrentDictionary<string, WeatherForecast> Forecasts { get; set; }
    }
}
