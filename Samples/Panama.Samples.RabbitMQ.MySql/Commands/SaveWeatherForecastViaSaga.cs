using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Options;
using Panama.Canal.Extensions;
using Panama.Canal.Interfaces;
using Panama.Canal.MySQL.Models;
using Panama.Canal.Sagas.Extensions;
using Panama.Canal.Sagas.Stateless.Extensions;
using Panama.Extensions;
using Panama.Interfaces;
using Panama.Samples.RabbitMQ.MySql.Models;
using Panama.Samples.RabbitMQ.MySql.Sagas.CreateWeatherForcast;
using Panama.Samples.RabbitMQ.MySQL.Contexts;

namespace Panama.Samples.RabbitMQ.MySql.Commands
{
    public class SaveWeatherForecastViaSaga : ICommand
    {
        private readonly MySqlOptions _options;
        private readonly AppDbContext _context;
        private readonly IGenericChannelFactory _factory;

        public SaveWeatherForecastViaSaga(
            AppDbContext context,
            IOptions<MySqlOptions> options,
            IGenericChannelFactory factory)
        {
            _context = context;
            _factory = factory;
            _options = options.Value;
        }
        public async Task Execute(IContext context)
        {
            var model = context.Data.DataGetSingle<WeatherForecast>();

            using (var channel = _factory.CreateChannel<DatabaseFacade, IDbContextTransaction>(_context.Database, context.Token))
            {
                // start saga
                await context.Saga<CreateWeatherForcastSaga>()
                    .Channel(channel)
                    .Data(model)
                    .Start();

                // commits save transaction, then publish
                await channel.Commit();
            }
        }
    }
}
