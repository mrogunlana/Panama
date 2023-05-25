using Panama.Canal.Interfaces;
using Panama.Canal.RabbitMQ.Attributes;
using Panama.Interfaces;

namespace Panama.Samples.RabbitMQ.MySql.Subscriptions
{
    [RabbitTopic("forcast.rollback")]
    public class ForecastRollback : ISubscribe
    {
        private readonly ILogger<ForecastRollback> _log;
        private readonly IServiceProvider _provider;

        public ForecastRollback(
              IServiceProvider provider
            , ILogger<ForecastRollback> log)
        {
            _log = log;
            _provider = provider;
        }
        public Task Event(IContext context)
        {
            // rollback forcast here...

            return Task.CompletedTask;
        }
    }
}
