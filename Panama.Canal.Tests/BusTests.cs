using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
    public class BusTests
    {
        private CancellationTokenSource _cts; 
        private IServiceProvider _provider;
        private IBootstrapper _bootstrapper;

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

            //add custom jobs to process outbox/inbox messages:
            services.AddSingleton<PublishOutbox>();
            services.AddSingleton<ReceiveInbox>();
            services.AddSingleton(new Job(
                type: typeof(PublishOutbox),
                expression: "0/1 * * * * ?"));
            services.AddSingleton(new Job(
                type: typeof(ReceiveInbox),
                expression: "0/1 * * * * ?"));

            _cts = new CancellationTokenSource();
            _provider = services.BuildServiceProvider();
            _bootstrapper = _provider.GetRequiredService<IBootstrapper>();
        }

        [TestInitialize]
        public async Task Init()
        {
            _cts = new CancellationTokenSource();

            await _bootstrapper.On(_cts.Token);
        }

        [TestCleanup]
        public async Task Cleanup()
        {
            _cts.Cancel();

            await _bootstrapper.Off(_cts.Token);

            _cts.Dispose();
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