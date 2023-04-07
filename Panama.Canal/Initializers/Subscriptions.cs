using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Panama.Canal.Extensions;
using Panama.Canal.Interfaces;
using Panama.Canal.Models;

namespace Panama.Canal.Initializers
{
    internal class Subscriptions : IInitialize
    {
        private readonly IServiceProvider _provider;
        private readonly ILogger<Subscriptions> _log;
        private readonly Models.Subscriptions _subscriptions;
        private readonly IOptions<CanalOptions> _options;

        public Subscriptions(
             IServiceProvider provider
           , ILogger<Subscriptions> log
           , IOptions<CanalOptions> options
           , Models.Subscriptions subscriptions)
        {
            _log = log;
            _options = options;
            _provider = provider;
            _subscriptions = subscriptions;
        }

        public Task Invoke(CancellationToken token)
        {
            if (token.IsCancellationRequested)
                token.ThrowIfCancellationRequested();

            var subscriptions = _provider
                .GetServices<ISubscribe>()
                .GetSubscriptions(_log)
                .ToDictionary();

            if (subscriptions == null)
                return Task.CompletedTask;
            if (subscriptions.Count() == 0)
                return Task.CompletedTask;

            _subscriptions.Set(subscriptions);

            return Task.CompletedTask;
        }
    }
}