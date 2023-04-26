using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Panama.Canal.Channels;
using Panama.Canal.Extensions;
using Panama.Canal.Interfaces;
using Panama.Canal.Sagas.Extensions;
using Panama.Canal.Sagas.Stateless.Extensions;
using Panama.Canal.Tests.Models;
using Panama.Canal.Tests.Sagas.CreateFoo;
using Panama.Canal.Tests.Subscriptions;
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
                        canal.UseDefaultStore();
                        canal.UseDefaultBroker();
                        canal.UseDefaultScheduler();
                    });
                });

            _cts = new CancellationTokenSource();
            _provider = services.BuildServiceProvider();
            _bootstrapper = _provider.GetRequiredService<IBootstrapper>();
        }

        //[TestInitialize]
        public async Task Init()
        {
            _provider.GetRequiredService<State>().Data.Clear();
            _cts = new CancellationTokenSource();

            await _bootstrapper.On(_cts.Token);
        }

        //[TestCleanup]
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
            _cts = new CancellationTokenSource();

            await _bootstrapper.On(_cts.Token);

            var channels = _provider.GetRequiredService<IDefaultChannelFactory>();
            var context = new Context(_provider);
            
            using (var channel = channels.CreateChannel<DefaultChannel>())
            {
                await context.Saga<CreateFooSaga>()
                    .Channel(channel)
                    .Data(new Foo())
                    .Start();

                await channel.Commit();

                await Task.Delay(TimeSpan.FromMinutes(60));

                var state = _provider.GetRequiredService<State>();
                var response = state.Data.ToList();

                Assert.IsTrue(response.KvpGet<string, string>("saga.event.name").Count == 3);
                Assert.IsTrue(response.KvpGet<string, string>("subscription.name").Count == 2);

                _cts.Cancel();

                await _bootstrapper.Off(_cts.Token);

                _cts.Dispose();
            }
        }
    }
}