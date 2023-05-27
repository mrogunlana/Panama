using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MySqlConnector.Logging;
using NLog.Extensions.Logging;
using Panama.Canal;
using Panama.Canal.Extensions;
using Panama.Samples.TestConsole.Jobs;

namespace Panama.Samples.TestConsole
{
    internal class Program
    {
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(builder => {
                    builder.Sources.Clear();

                    var configuration = new ConfigurationBuilder()
                        .AddEnvironmentVariables()
                        .AddCommandLine(args)
                        .AddJsonFile("appsettings.test.json")
                        .Build();

                    builder.AddConfiguration(configuration);
                })
                .ConfigureServices((context, services) => {
                    
                    NLog.Extensions.Logging.ConfigSettingLayoutRenderer.DefaultConfiguration = context.Configuration;

                    MySqlConnectorLogManager.Provider = new MySqlConnector.Logging.NLogLoggerProvider();

                    services.AddOptions();
                    services.AddLogging();
                    services.AddSingleton<IServiceCollection>(_ => services);
                    services.AddSingleton(context.Configuration);
                    services.AddSingleton<IConfiguration>(context.Configuration);

                    services.AddPanama(
                        configuration: context.Configuration,
                        setup: options => {
                            options.UseCanal(canal => {
                                canal.UseDefaultScheduler((scheduler) => {
                                    scheduler.ClearJobs();
                                    scheduler.AddJob<Publish10MessagesEverySecond>("* * * * * ?");
                                });
                            });
                        });

                    services.AddLogging(loggingBuilder => {
                        // configure Logging with NLog
                        loggingBuilder.ClearProviders();
                        loggingBuilder.SetMinimumLevel(LogLevel.Trace);
                        loggingBuilder.AddNLog(context.Configuration);
                    });
                });

        public static void Main(string[] args) => CreateHostBuilder(args).Build().Run();
    }
}