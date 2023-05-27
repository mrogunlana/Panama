using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Panama.Canal.Models.Descriptors;

namespace Panama.Canal.Tests
{
    [TestClass]
    public class SubscriptionTests
    {
        private IServiceProvider _provider;

        public SubscriptionTests()
        {
            var services = new ServiceCollection();

            services.AddOptions();
            services.AddLogging();
            services.AddSingleton<IServiceCollection>(_ => services);

            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.test.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            services.AddSingleton(configuration);
            services.AddSingleton<IConfiguration>(configuration);

            services.AddPanama(
                configuration: configuration,
                setup: options => {
                    options.UseCanal(canal => {
                        canal.UseDefaultDispatcher();
                        canal.UseDefaultStore();
                        canal.UseDefaultBroker();
                        canal.UseDefaultScheduler();
                    });
                });

            _provider = services.BuildServiceProvider();
        }

        [TestMethod]
        public async Task DetectSubscriptions()
        {
            var initializer = _provider.GetRequiredService<Initializers.Subscriptions>();

            await initializer.Invoke(CancellationToken.None);

            var manager = _provider.GetRequiredService<SubscriberDescriptions>();

            Assert.IsNotNull(manager.Entries);
            Assert.IsTrue(manager.Entries.Count > 0);
        }
    }
}