using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Panama.Canal.Brokers.Interfaces;
using Panama.Canal.Comparers;
using Panama.Canal.Extensions;
using Panama.Canal.Interfaces;
using Panama.Canal.Models.Descriptors;
using Panama.Canal.Models.Options;
using Panama.Canal.Sagas.Interfaces;

namespace Panama.Canal.Initializers
{
    public class Sagas : IInitialize
    {
        private readonly ILogger<Sagas> _log;
        private readonly ITargetFactory _factory;
        private readonly IServiceProvider _provider;
        private readonly IOptions<CanalOptions> _options;
        private readonly SagaDescriptions _descriptions;

        public Sagas(
             ITargetFactory factory
           , IServiceProvider provider
           , ILogger<Sagas> log
           , IOptions<CanalOptions> options
           , SagaDescriptions descriptions)
        {
            _log = log;
            _factory = factory;
            _options = options;
            _provider = provider;
            _descriptions = descriptions;
        }

        private IEnumerable<IDescriptor> SetupDescriptions(IEnumerable<ISaga> sagas)
        {
            var descriptions = new List<IDescriptor>();

            if (sagas == null)
                return descriptions;
            if (sagas.Count() == 0)
                return descriptions;

            foreach (var saga in sagas)
            {
                var subscription = new SagaDescriptor(
                    topic: saga.ReplyTopic,
                    group: saga.ReplyGroup ?? _options.Value.DefaultGroup,
                    saga: saga.GetType(),
                    target: saga?.Target ?? _factory.GetDefaultTarget().GetType());

                descriptions.Add(subscription);
            }

            descriptions = descriptions
                .Distinct(new DescriptorComparer(_log))
                .ToList();

            return descriptions;
        }


        public Task Invoke(CancellationToken token)
        {
            if (token.IsCancellationRequested)
                token.ThrowIfCancellationRequested();

            var descriptions = SetupDescriptions(_provider
                .GetServices<ISaga>())
                .ToDictionary();

            if (descriptions == null)
                return Task.CompletedTask;
            if (descriptions.Count() == 0)
                return Task.CompletedTask;

            _descriptions.Set(descriptions);

            return Task.CompletedTask;
        }
    }
}