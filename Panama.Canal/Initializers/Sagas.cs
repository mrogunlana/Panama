using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Panama.Canal.Brokers.Interfaces;
using Panama.Canal.Interfaces;
using Panama.Canal.Models.Options;
using Panama.Canal.Sagas.Interfaces;
using Panama.Models;

namespace Panama.Canal.Initializers
{
    public class Sagas : IInitialize
    {
        private readonly IServiceProvider _provider;
        private readonly ILogger<Brokers> _log;
        private readonly IOptions<CanalOptions> _options;

        public Sagas(
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

            //TODO?
            
            return Task.CompletedTask;
        }
    }
}