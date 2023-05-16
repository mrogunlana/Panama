using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySqlConnector;
using MySqlConnector.Logging;
using NLog.Extensions.Logging;
using Panama.Canal.Extensions;
using Panama.Canal.Interfaces;
using Panama.Canal.Jobs;
using Panama.Canal.Models.Messaging;
using Panama.Canal.MySQL;
using Panama.Canal.MySQL.Extensions;
using Panama.Canal.MySQL.Models;
using Panama.Canal.Tests.Models;
using Panama.Canal.Tests.Subscriptions;
using Panama.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Context = Panama.Models.Context;

namespace Panama.Canal.Tests.MySQL
{
    [TestClass]
    public class MySqlMessageTests
    {
        private CancellationTokenSource _cts;
        private ServiceCollection _services;
        private IConfigurationRoot _configuration;

        public MySqlMessageTests()
        {
            _services = new ServiceCollection();
            _cts = new CancellationTokenSource();
            _configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.test.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            NLog.Extensions.Logging.ConfigSettingLayoutRenderer.DefaultConfiguration = _configuration;
            MySqlConnectorLogManager.Provider = new MySqlConnector.Logging.NLogLoggerProvider();
            
            Init();
        }

        public virtual void Init()
        {
            _services.AddOptions();
            _services.AddLogging();
            _services.AddSingleton<IServiceCollection>(_ => _services);
            _services.AddSingleton(_configuration);
            _services.AddSingleton<IConfiguration>(_configuration);
        }

        [TestMethod]
        public async Task VerifySucceededStreamingPublishMessageState()
        {
            _services.AddSingleton<State>();
            _services.AddPanama(
                configuration: _configuration,
                setup: options => {
                    options.UseCanal(canal => {
                        canal.UseMySqlStore();
                        canal.UseDefaultBroker();
                    });
                });

            _services.AddLogging(loggingBuilder => {
                // configure Logging with NLog
                loggingBuilder.ClearProviders();
                loggingBuilder.SetMinimumLevel(LogLevel.Trace);
                loggingBuilder.AddNLog(_configuration);
            });

            _cts = new CancellationTokenSource();

            var provider = _services.BuildServiceProvider();
            var bootstrapper = provider.GetRequiredService<IBootstrapper>();

            await bootstrapper.On(_cts.Token);

            var options = provider.GetRequiredService<IOptions<MySqlOptions>>().Value;
            var channels = provider.GetRequiredService<IGenericChannelFactory>();
            var context = new Context(provider);
            var foo = new Foo();
            var id = Guid.NewGuid().ToString();

            using (var connection = new MySqlConnection(options.GetConnectionString()))
            using (var channel = channels.CreateChannel<IDbConnection, IDbTransaction>(connection, _cts.Token))
            {
                var result = await context.Bus()
                    .Id(id)
                    .Data(foo)
                    .Topic("foo.created")
                    .Channel(channel)
                    .Post();

                await channel.Commit();
                await Task.Delay(TimeSpan.FromSeconds(2));

                var state = provider.GetRequiredService<State>();
                var response = state.Data.ToList();

                Assert.IsTrue(response.KvpGet<string, string>("subscription.name").Any(k => k == nameof(FooCreated)));
            }

            await bootstrapper.Off(_cts.Token);
        }

        [TestMethod]
        public async Task VerifySucceededStreamingPublishReplyMessageState()
        {
            _services.AddSingleton<State>();
            _services.AddPanama(
                configuration: _configuration,
                setup: options => {
                    options.UseCanal(canal => {
                        canal.UseMySqlStore();
                        canal.UseDefaultBroker();
                    });
                });

            _services.AddLogging(loggingBuilder => {
                // configure Logging with NLog
                loggingBuilder.ClearProviders();
                loggingBuilder.SetMinimumLevel(LogLevel.Trace);
                loggingBuilder.AddNLog(_configuration);
            });

            _cts = new CancellationTokenSource();

            var provider = _services.BuildServiceProvider();
            var bootstrapper = provider.GetRequiredService<IBootstrapper>();

            await bootstrapper.On(_cts.Token);

            var options = provider.GetRequiredService<IOptions<MySqlOptions>>().Value;
            var channels = provider.GetRequiredService<IGenericChannelFactory>();
            var context = new Context(provider);
            var foo = new Foo();
            var id = Guid.NewGuid().ToString();

            using (var connection = new MySqlConnection(options.GetConnectionString()))
            using (var channel = channels.CreateChannel<IDbConnection, IDbTransaction>(connection, _cts.Token))
            {
                var result = await context.Bus()
                    .Id(id)
                    .Data(foo)
                    .Reply("foo.ack")
                    .Topic("foo.created")
                    .Channel(channel)
                    .Post();

                await channel.Commit();
                await Task.Delay(TimeSpan.FromSeconds(2));

                var state = provider.GetRequiredService<State>();
                var response = state.Data.ToList();

                Assert.IsTrue(response.KvpGet<string, string>("subscription.name").Any(k => k == nameof(FooCreated)));
                Assert.IsTrue(response.KvpGet<string, string>("subscription.name").Any(k => k == nameof(FooAcknowledged)));
            }

            await bootstrapper.Off(_cts.Token);
        }

        [TestMethod]
        public async Task VerifyDelayedSucceededStreamingPublishUsingDefaultJob()
        {
            
            _services.AddSingleton<State>();
            _services.AddPanama(
                configuration: _configuration,
                setup: options => {
                    options.UseCanal(canal => {
                        canal.UseMySqlStore();
                        canal.UseDefaultBroker();
                        canal.UseDefaultScheduler((scheduler) => {
                            //scheduler.RemoveJob<DelayedPublished>();
                            //scheduler.AddJob<DelayedPublished>("* * * * * ?");
                        });
                    });
                });
            _services.AddLogging(loggingBuilder => {
                 // configure Logging with NLog
                 loggingBuilder.ClearProviders();
                 loggingBuilder.SetMinimumLevel(LogLevel.Trace);
                 loggingBuilder.AddNLog(_configuration);
             });

            _cts = new CancellationTokenSource();

            var provider = _services.BuildServiceProvider();
            var bootstrapper = provider.GetRequiredService<IBootstrapper>();

            await bootstrapper.On(_cts.Token);

            var options = provider.GetRequiredService<IOptions<MySqlOptions>>().Value;
            var channels = provider.GetRequiredService<IGenericChannelFactory>();
            var context = new Context(provider);
            var foo = new Foo();
            var id = Guid.NewGuid().ToString();
            var now = DateTime.UtcNow;

            using (var connection = new MySqlConnection(options.GetConnectionString()))
            using (var channel = channels.CreateChannel<IDbConnection, IDbTransaction>(connection, _cts.Token))
            {
                var result = await context.Bus()
                    .Id(id)
                    .Data(foo)
                    .Reply("foo.ack")
                    .Topic("foo.created")
                    .Delay(TimeSpan.FromSeconds(75))
                    .Channel(channel)
                    .Post();

                await channel.Commit();
            }

            await Task.Delay(TimeSpan.FromSeconds(200));

            var state = provider.GetRequiredService<State>();
            var response = state.Data.ToList();

            Assert.IsTrue(response.KvpGet<string, string>("subscription.name").Any(k => k == nameof(FooCreated)));
            Assert.IsTrue(response.KvpGet<string, string>("subscription.name").Any(k => k == nameof(FooAcknowledged)));

            var then = response.KvpGetSingle<string, DateTime>("FooCreated.DateTime");

            var delay = (then - now).TotalSeconds;
            var log = provider.GetRequiredService<ILogger<MySqlMessageTests>>();

            log.LogInformation($"Total delayed message processing time in seconds: {delay}");

            await bootstrapper.Off(_cts.Token);
        }

        [TestMethod]
        //[Obsolete]
        public async Task VerifyDelayedPublishJobChangesStateProperly()
        {
            _services.AddSingleton<State>();
            _services.AddPanama(
                configuration: _configuration,
                setup: options => {
                    options.UseCanal(canal => {
                        canal.UseMySqlStore();
                        canal.UseDefaultBroker();
                        canal.UseDefaultScheduler((scheduler) => {
                            scheduler.RemoveJob<DelayedPublished>();
                            scheduler.AddJob<DelayedPublished>("* * * * * ?");
                        });
                    });
                });

            _cts = new CancellationTokenSource();

            var provider = _services.BuildServiceProvider();
            var bootstrapper = provider.GetRequiredService<IBootstrapper>();
            var log = provider.GetRequiredService<ILogger<MySqlMessageTests>>();
            var id = Guid.NewGuid().ToString();
            await bootstrapper.On(_cts.Token);

            var options = provider.GetRequiredService<IOptions<MySqlOptions>>();
            var settings = provider.GetRequiredService<MySqlSettings>();
            var store = provider.GetRequiredService<IStore>();
            var context = new Context(provider);

            var channels = provider.GetRequiredService<IGenericChannelFactory>();
            using (var connection = new MySqlConnection(options.Value.GetConnectionString()))
            using (var channel = channels.CreateChannel<IDbConnection, IDbTransaction>(connection, _cts.Token))
            {
                await context.Bus()
                    .Id(id)
                    .Data(new Foo() { Value = DateTime.UtcNow.ToLongDateString() })
                    .Topic("foo.created")
                    .Reply("foo.ack")
                    .Channel(channel)
                    .Delay(TimeSpan.FromSeconds(75))
                    .Post();

                await channel.Commit();
            }

            log.LogInformation($"Stored temporary message.Id: {id}.");

            await Task.Delay(TimeSpan.FromSeconds(90));

            using (var connection = new MySqlConnection($"Server={options.Value.Host};Port={options.Value.Port};Database={options.Value.Database};Uid={options.Value.Username};Pwd={options.Value.Password};Allow User Variables=True;"))
            {
                if (connection.State == ConnectionState.Closed)
                    await connection.OpenAsync().ConfigureAwait(false);
                using var command = new MySqlCommand($@"

                    SET @_Id = (SELECT _Id FROM `{options.Value.Database}`.`{settings.PublishedTable}` WHERE `__Id` = unhex(md5(@Id)) LIMIT 1);                    

                    SELECT 
                         `_Id`
                        ,`Id` 
                        ,`CorrelationId`
                        ,`Version`
                        ,`Name` 
                        ,`Group` 
                        ,`Content` 
                        ,`Retries` 
                        ,`Created` 
                        ,`Expires` 
                        ,`Status` 
                    FROM `{options.Value.Database}`.`{settings.PublishedTable}`
                    WHERE `_Id` = @_Id;"

                , connection);

                command.Parameters.Add(new MySqlParameter
                {
                    ParameterName = "@Id",
                    DbType = DbType.String,
                    Value = id
                });

                var results = new List<InternalMessage>();
                var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);

                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    var model = settings.GetModel(settings.PublishedTable);
                    for (int i = 0; i < reader.FieldCount; i++)
                        model.SetValue<InternalMessage>(reader.GetName(i), reader.GetValue(i));

                    results.Add(model);
                }

                connection.Close();

                Assert.IsTrue (results.Count > 0);
                Assert.IsTrue(results.First().Status == MessageStatus.Succeeded.ToString());
            }

            await bootstrapper.Off(_cts.Token);
        }
    }
}