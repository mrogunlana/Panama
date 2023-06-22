using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Options;
using Panama.Canal.Extensions;
using Panama.Canal.Interfaces;
using Panama.Canal.MySQL.Models;
using Panama.Extensions;
using Panama.Interfaces;
using Panama.Samples.RabbitMQ.MySql.Models;
using Panama.Samples.RabbitMQ.MySQL.Contexts;

namespace Panama.Samples.RabbitMQ.MySql.Commands
{
    public class SaveWeatherForecast : ICommand
    {
        private readonly MySqlOptions _options;
        private readonly AppDbContext _context;
        private readonly IGenericChannelFactory _factory;

        public SaveWeatherForecast(
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
            var models = context.Data.DataGet<WeatherForecast>();

            using (var channel = _factory.CreateChannel<DatabaseFacade, IDbContextTransaction>(_context.Database, context.Token))
            {
                foreach (var model in models)
                {
                    _context.Add(model);

                    // save forcast to database
                    await _context.SaveChangesAsync();
                }

                // publish new model
                await context.Bus()
                    .Channel(channel)
                    .Token(context.Token)
                    .Topic("forecast.created")
                    .Data(models)
                    .Post();

                // commits save transaction, then publish
                await channel.Commit();
            }
        }
    }
}
