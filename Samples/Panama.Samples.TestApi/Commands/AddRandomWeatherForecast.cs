using Panama.Extensions;
using Panama.Interfaces;

namespace Panama.Samples.TestApi.Commands
{
    public class AddRandomWeatherForecast : ICommand
    {
        public async Task Execute(IContext context)
        {
            var kvp = context.Data.KvpGetSingle<string, int>("test");
            var snapshot = context.Data.SnapshotGet<string, int>("test");
            var list = context.Data.KvpGet<int, string>();

            context.Data.AddRange(Enumerable
                .Range(1, 5)
                .Select(index => new WeatherForecast
                {
                    Date = DateTime.UtcNow.AddDays(index),
                    TemperatureC = Random.Shared.Next(-20, 55),
                    Summary = list[Random.Shared.Next(list.Count)].Value
                }));

            //wait 5 seconds before next command...
            await Task.Delay(2000);
        }
    }
}
