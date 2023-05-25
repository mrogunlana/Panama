using Panama.Canal.Interfaces;
using Panama.Canal.RabbitMQ.Attributes;
using Panama.Interfaces;

namespace Panama.Samples.RabbitMQ.MySql.Subscriptions
{
    [RabbitTopic("forcast.completed")]
    public class ForecastCompleted : ISubscribe
    {
        private readonly ILogger<ForecastCreate> _log;
        private readonly IServiceProvider _provider;

        public ForecastCompleted(
              IServiceProvider provider
            , ILogger<ForecastCreate> log)
        {
            _log = log;
            _provider = provider;
        }
        public Task Event(IContext context)
        {
            // TODO: publish completion of saga forcast to this topic

            return Task.CompletedTask;
        }
    }
}
