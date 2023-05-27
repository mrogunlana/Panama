using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Panama.Canal.Channels;
using Panama.Canal.Extensions;
using Panama.Canal.Interfaces;
using Panama.Canal.Models.Messaging;
using Panama.Canal.Sagas.Extensions;
using Panama.Canal.Sagas.Stateless.Extensions;
using Panama.Canal.Tests.Modules.Models;
using Panama.Canal.Tests.Modules.Sagas.CreateFoo;
using Panama.Canal.Tests.Modules.Sagas.CreateFoo.Events;
using Panama.Canal.Tests.Modules.Sagas.CreateFoo.Exits;
using Panama.Canal.Tests.Modules.Subscriptions;
using Panama.Extensions;
using Panama.Models;

namespace Panama.Canal.Tests
{
    [TestClass]
    public class HeaderTests
    {
        private CancellationTokenSource _cts; 
        private IServiceProvider _provider;
        private IBootstrapper _bootstrapper;
        
        public HeaderTests()
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
            services.AddSingleton<State>();

            services.AddPanama(
                configuration: configuration,
                setup: options => {
                    options.UseCanal(canal => {
                        canal.UseDefaultStore();
                        canal.UseDefaultBroker();
                        canal.UseDefaultScheduler();
                    });
                });

            _cts = new CancellationTokenSource();
            _provider = services.BuildServiceProvider();
            _bootstrapper = _provider.GetRequiredService<IBootstrapper>();
        }

        [TestInitialize]
        public async Task Init()
        {
            _provider.GetRequiredService<State>().Data.Clear();
            _cts = new CancellationTokenSource();

            await _bootstrapper.On(_cts.Token);
        }

        [TestCleanup]
        public async Task Cleanup()
        {
            _provider.GetRequiredService<State>().Data.Clear();
            _cts.Cancel();

            await _bootstrapper.Off(_cts.Token);

            _cts.Dispose();
        }

        [TestMethod]
        public void VerifyHeaderFilters()
        {
            var message = new Message()
                .AddCorrelationId(Guid.NewGuid().ToString())
                .AddCreatedTime()
                .AddGroup("test.group")
                .AddMessageTopic("test.topic")
                .AddMessageId(Guid.NewGuid().ToString())
                .AddType(GetType().AssemblyQualifiedName)
                .AddReply("test.reply")
                .AddSentTime(DateTime.UtcNow.AddSeconds(1))
                .AddDelayTime(DateTime.UtcNow.AddSeconds(5));
            
            var result = message.Headers.DefaultFilter();

            Assert.AreEqual(1, result.Count);
        }
    }
}