using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Panama.Canal.Channels;
using Panama.Canal.Extensions;
using Panama.Canal.Interfaces;
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
    public class SagaTests
    {
        private CancellationTokenSource _cts; 
        private IServiceProvider _provider;
        private IBootstrapper _bootstrapper;
        
        public SagaTests()
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
                        canal.UseDefaultDispatcher();
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
        public async Task VerifyFooSaga()
        {
            var channels = _provider.GetRequiredService<IDefaultChannelFactory>();
            var context = new Context(_provider);
            
            using (var channel = channels.CreateChannel<DefaultChannel>())
            {
                await context.Saga<CreateFooSaga>()
                    .Channel(channel)
                    .Data(new Foo())
                    .Start();

                await channel.Commit();

                await Task.Delay(TimeSpan.FromSeconds(2));

                var state = _provider.GetRequiredService<State>();
                var store = _provider.GetRequiredService<Store>();
                var response = state.Data.ToList();

                Assert.IsTrue(response.KvpGet<string, string>("saga.event.name").Count == 3);
                Assert.IsTrue(response.KvpGet<string, string>("saga.exit.name").Count == 1);
                Assert.IsTrue(response.KvpGet<string, string>("subscription.name").Count == 1);

                var steps = new List<string>();

                steps.AddRange(response.KvpGet<string, string>("saga.event.name"));
                steps.AddRange(response.KvpGet<string, string>("saga.exit.name"));
                steps.AddRange(response.KvpGet<string, string>("subscription.name"));

                Assert.IsNotNull(steps.Contains(nameof(CreateFooEvent)));
                Assert.IsNotNull(steps.Contains(nameof(FooCreated)));
                Assert.IsNotNull(steps.Contains(nameof(ReviewCreateFooAnswerEvent)));
                Assert.IsNotNull(steps.Contains(nameof(ReviewCreateFooAnswerExit)));
                Assert.IsNotNull(steps.Contains(nameof(FooCompletedEvent)));
            }
        }
    }
}