using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Panama.Canal.Interfaces;
using Panama.Canal.Models;
using Panama.Interfaces;

namespace Panama.Canal.Invokers
{
    public class PublishedInvokerFactory : IInvokeFactory
    {
        private readonly IServiceProvider _provider;
        private readonly CanalOptions _options;

        public PublishedInvokerFactory(
            IServiceProvider provider,
            IOptions<CanalOptions> options)
        {
            _provider = provider;
            _options = options.Value;
        }
        public IInvoke GetInvoker()
        {
            if (_options.ProcessingType == ProcessingType.None)
                throw new InvalidOperationException("Canal processing type should be set in the startup registration.");

            switch (_options.ProcessingType)
            {
                case ProcessingType.Stream:
                    return _provider.GetRequiredService<OutboxInvoker>();
                case ProcessingType.Poll:
                    return _provider.GetRequiredService<PollingPublisherInvoker>();
                default:
                    throw new InvalidOperationException($"Processing type {_options.ProcessingType} not supported.");
            }
        }
    }
}
