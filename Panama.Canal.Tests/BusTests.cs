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
using Panama.Canal.Channels;
using Panama.Models;
using Panama.Canal.Extensions;

namespace Panama.Canal.Tests
{
    [TestClass]
    public class BusTests
    {
        private IServiceProvider _provider;

        public BusTests()
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

            services.AddPanama(domain);
            services.AddPanamaCanal(configuration, domain);
            services.AddPanamaSecurity();

            _provider = services.BuildServiceProvider();
        }

        [TestMethod]
        public async Task VerifyPost()
        {
            var channels = _provider.GetRequiredService<IDefaultChannelFactory>();
            var context = new Context(_provider);
            
            using (var channel = channels.CreateChannel<DefaultChannel>())
            {
                var result = await context.Bus()
                    .Topic("foo.created")
                    .Channel(channel)
                    .Post();

                Assert.IsTrue(result.Success);
            }
        }
    }
}