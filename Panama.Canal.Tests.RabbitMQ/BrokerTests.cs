using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySqlConnector.Logging;
using NLog.Extensions.Logging;
using Panama.Canal.Brokers.Interfaces;
using Panama.Canal.Channels;
using Panama.Canal.Extensions;
using Panama.Canal.Interfaces;
using Panama.Canal.RabbitMQ;
using Panama.Canal.RabbitMQ.Models;
using Panama.Canal.Sagas.Extensions;
using Panama.Canal.Sagas.Stateless.Extensions;
using Panama.Canal.Tests.Modules.Models;
using Panama.Canal.Tests.Modules.Sagas.CreateFoo;
using Panama.Canal.Tests.Modules.Sagas.CreateFoo.Events;
using Panama.Canal.Tests.Modules.Sagas.CreateFoo.Exits;
using Panama.Canal.Tests.Modules.Subscriptions;
using Panama.Extensions;
using Panama.Models;
using System.Configuration;

namespace Panama.Canal.Tests.RabbitMQ
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

            NLog.Extensions.Logging.ConfigSettingLayoutRenderer.DefaultConfiguration = configuration;
            MySqlConnectorLogManager.Provider = new MySqlConnector.Logging.NLogLoggerProvider();

            services.AddSingleton(configuration);
            services.AddSingleton<IConfiguration>(configuration);
            services.AddSingleton<State>();

            services.AddLogging(loggingBuilder => {
                // configure Logging with NLog
                loggingBuilder.ClearProviders();
                loggingBuilder.SetMinimumLevel(LogLevel.Trace);
                loggingBuilder.AddNLog(configuration);
            });

            services.AddPanama(
                configuration: configuration,
                setup: options => {
                    options.UseCanal(canal => {
                        canal.UseDefaultStore();
                        canal.UseRabbitMq();
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
        public void VerifyDefaultTarget()
        {
            var factory = _provider.GetRequiredService<ITargetFactory>();
            var target = factory.GetDefaultTarget();

            Assert.AreEqual(target.GetType(), typeof(RabbitMQTarget));
        }
    }
}