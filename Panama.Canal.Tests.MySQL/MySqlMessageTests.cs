using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySqlConnector;
using Panama.Canal.Extensions;
using Panama.Canal.Interfaces;
using Panama.Canal.Jobs;
using Panama.Canal.Models.Options;
using Panama.Canal.MySQL;
using Panama.Canal.MySQL.Extensions;
using Panama.Canal.MySQL.Models;
using Panama.Canal.Tests.Models;
using Panama.Canal.Tests.Subscriptions;
using Panama.Extensions;
using Panama.Models;
using Quartz.Impl.AdoJobStore.Common;
using System;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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

            await Task.Delay(TimeSpan.FromHours(90));

            var state = provider.GetRequiredService<State>();
            var response = state.Data.ToList();

            Assert.IsTrue(response.KvpGet<string, string>("subscription.name").Any(k => k == nameof(FooCreated)));
            Assert.IsTrue(response.KvpGet<string, string>("subscription.name").Any(k => k == nameof(FooAcknowledged)));

            var then = response.KvpGetSingle<string, DateTime>("FooCreated.DateTime");

            var delay = (then - now).TotalSeconds;

            Assert.IsTrue(delay > 70);
            Assert.IsTrue(delay < 80);

            await bootstrapper.Off(_cts.Token);
        }
    }
}