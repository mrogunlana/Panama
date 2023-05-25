using Panama.Interfaces;

namespace Panama.Samples.RabbitMQ.MySql.Models
{
    public class WeatherForecast : IModel
    {
        public long _Id { get; set; }
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public DateTime Date { get; set; }

        public int TemperatureC { get; set; }

        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

        public string? Summary { get; set; }
        public DateTime Created { get; set; } = DateTime.Now;
    }
}