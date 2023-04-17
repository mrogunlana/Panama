using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Panama.Canal.Brokers.Interfaces;
using Panama.Canal.Interfaces;
using Panama.Canal.Models.Options;
using Panama.Models;

namespace Panama.Canal.Initializers
{
    public class Brokers : IInitialize
    {
        private readonly IServiceProvider _provider;
        private readonly ILogger<Brokers> _log;
        private readonly IOptions<CanalOptions> _options;

        public Brokers(
             IServiceProvider provider
           , ILogger<Brokers> log
           , IOptions<CanalOptions> options)
        {
            _log = log;
            _options = options;
            _provider = provider;
        }

        public Task Invoke(CancellationToken token)
        {
            if (token.IsCancellationRequested)
                token.ThrowIfCancellationRequested();

            var processes = _provider
                .GetServices<IBrokerProcess>();

            if (processes == null)
                return Task.CompletedTask;
            if (processes.Count() == 0)
                return Task.CompletedTask;

            foreach (var process in processes)
                process.Start(new Context(token));

            return Task.CompletedTask;
        }
    }
}