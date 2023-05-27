using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Panama.Samples.TestConsole.Models
{
    public class WeatherForecast
    {
        public long _Id { get; set; }
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public DateTime Date { get; set; }

        public int TemperatureC { get; set; }

        public string? Summary { get; set; }
        public DateTime Created { get; set; } = DateTime.Now;
    }
}
