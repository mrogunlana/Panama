using Microsoft.Extensions.Options;
using MySqlConnector;
using Panama.Canal.Extensions;
using Panama.Canal.Interfaces;
using Panama.Canal.MySQL.Extensions;
using Panama.Canal.MySQL.Models;
using Panama.Extensions;
using Panama.Interfaces;
using Panama.Samples.RabbitMQ.MySql.Models;
using System.Data;

namespace Panama.Samples.RabbitMQ.MySql.Commands
{
    public class SaveWeatherForecast : ICommand
    {
        private readonly MySqlOptions _options;
        private readonly IGenericChannelFactory _factory;

        public SaveWeatherForecast(
            IOptions<MySqlOptions> options,
            IGenericChannelFactory factory)
        {
            _factory = factory;
            _options = options.Value;
        }
        public async Task Execute(IContext context)
        {
            var model = context.Data.DataGetSingle<WeatherForecast>();
            context.Snapshot(model);
            using (var connection = new MySqlConnection(_options.GetConnectionString()))
            using (var channel = _factory.CreateChannel<IDbConnection, IDbTransaction>(connection, context.Token))
            {
                // publish new model
                await context.Bus()
                    .Channel(channel)
                    .Token(context.Token)
                    .Topic("forecast.created")
                    .Data(model)
                    .Post();

                await channel.Commit();
            }
        }
    }
}
