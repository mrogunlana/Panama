using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Panama.Canal.Interfaces;
using Panama.Security;
using System.Reflection;
using Panama.Canal.Initializers;
using Panama.Interfaces;
using Panama.Canal.Sagas.Stateless.Interfaces;
using Panama.Canal.Models;
using Panama.Canal.Tests.Subscriptions;

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

            var assemblies = new List<Assembly>();

            // domain built like so to overcome .net core .dll discovery issue 
            // within container:
            assemblies.Add(Assembly.GetExecutingAssembly());
            assemblies.AddRange(AppDomain.CurrentDomain.GetAssemblies());
            assemblies.AddRange(Assembly
                .GetExecutingAssembly()
                .GetReferencedAssemblies()
                .Select(x => Assembly.Load(x))
                .ToList());

            var domain = assemblies.ToArray();

            services.AddPanama(
                assemblies: domain,
                configuration: configuration,
                setup: options => {
                    options.UseCanal();
                    options.UseDefaultBroker();
                    options.UseDefaultStore();
                });

            _provider = services.BuildServiceProvider();
        }

        [TestMethod]
        public async Task DetectSubscriptions()
        {
            var initializer = _provider.GetRequiredService<BuildSubscriptions>();

            await initializer.Invoke(CancellationToken.None);

            var manager = _provider.GetRequiredService<ConsumerSubscriptions>();

            Assert.IsNotNull(manager.Entries);
            Assert.IsTrue(manager.Entries.Count > 0);
        }
    }
}