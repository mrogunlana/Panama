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