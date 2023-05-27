using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySqlConnector.Logging;
using NLog.Extensions.Logging;
using Panama.Canal.Extensions;
using Panama.Canal.Interfaces;
using Panama.Canal.Jobs;
using Panama.Canal.MySQL;
using Panama.Canal.Tests.Modules.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Panama.Canal.Tests.MySQL
{
    [TestClass]
    public class Startup
    {
        public static IServiceProvider? _provider;
        public static ServiceCollection? _services;
        public static IBootstrapper? _bootstrapper;
        public static CancellationTokenSource _cts = new CancellationTokenSource();
        public static IConfigurationRoot? _configuration;

        [AssemblyInitialize]
        public static void Init(TestContext context)
        {
            _services = new ServiceCollection();
            _cts = new CancellationTokenSource();
            _configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.test.json", optional: true)
                .AddEnvironmentVariables()
            .Build();

            NLog.Extensions.Logging.ConfigSettingLayoutRenderer.DefaultConfiguration = _configuration;

            MySqlConnectorLogManager.Provider = new MySqlConnector.Logging.NLogLoggerProvider();

            _services.AddOptions();
            _services.AddLogging();
            _services.AddSingleton<IServiceCollection>(_ => _services);
            _services.AddSingleton(_configuration);
            _services.AddSingleton<IConfiguration>(_configuration);

            _services.AddSingleton<State>();
            _services.AddPanama(
                configuration: _configuration,
                setup: options => {
                    options.UseCanal(canal => {
                        canal.UseMySqlStore();
                        canal.UseDefaultBroker();
                        canal.UseDefaultDispatcher();
                        canal.UseDefaultScheduler((scheduler) => {
                            scheduler.RemoveJob<DelayedPublished>();
                            scheduler.AddJob<DelayedPublished>("* * * * * ?");
                        });
                    });
                });

            _services.AddLogging(loggingBuilder => {
                // configure Logging with NLog
                loggingBuilder.ClearProviders();
                loggingBuilder.SetMinimumLevel(LogLevel.Trace);
                loggingBuilder.AddNLog(_configuration);
            });

            _provider = _services.BuildServiceProvider();

            _bootstrapper = _provider!.GetRequiredService<IBootstrapper>();

            _bootstrapper!.On(_cts.Token).GetAwaiter().GetResult();
        }

        [AssemblyCleanup]
        public static void Cleanup()
        {
            _cts.Cancel();

            _bootstrapper!.Off(_cts.Token).GetAwaiter().GetResult();

            _cts.Dispose();

            Task.Delay(TimeSpan.FromSeconds(1)).GetAwaiter().GetResult();
        }
    }
}
