using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Panama.Canal.Brokers.Interfaces;
using Panama.Canal.Channels;
using Panama.Canal.Extensions;
using Panama.Canal.Interfaces;
using Panama.Canal.Models;
using Panama.Canal.Tests.Jobs;
using Panama.Models;
using Panama.Security;
using System.Reflection;

namespace Panama.Canal.Tests
{
    [TestClass]
    public class BrokerTests
    {
        private CancellationTokenSource _cts; 
        private IServiceProvider _provider;
        private IBootstrapper _bootstrapper;

        public BrokerTests()
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
                    options.UseCanal();
                    options.UseDefaultBroker();
                    options.UseDefaultStore();
                });

            _cts = new CancellationTokenSource();
            _provider = services.BuildServiceProvider();
            _bootstrapper = _provider.GetRequiredService<IBootstrapper>();
        }

        

        [TestMethod]
        public void VerifyDefaultTarget()
        {
            var factory = _provider.GetRequiredService<ITargetFactory>();

            var target = factory.GetDefaultTarget();

            Assert.AreEqual(target.GetType(), typeof(DefaultTarget));
        }
    }
}