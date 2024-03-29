using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLog.Extensions.Logging;
using Panama.Canal.Channels;
using Panama.Canal.Extensions;
using Panama.Canal.Interfaces;
using Panama.Canal.Jobs;
using Panama.Canal.Models;
using Panama.Canal.Models.Messaging;
using Panama.Canal.Models.Options;
using Panama.Canal.Tests.Modules.Models;
using Panama.Canal.Tests.Modules.Subscriptions;
using Panama.Extensions;
using Panama.Models;

namespace Panama.Canal.Tests
{
    [TestClass]
    public class MessageTests
    {
        private CancellationTokenSource _cts; 
        private ServiceCollection _services;
        private IConfigurationRoot _configuration;

        public MessageTests()
        {
            _services = new ServiceCollection();
            _cts = new CancellationTokenSource();
            _configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.test.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            NLog.Extensions.Logging.ConfigSettingLayoutRenderer.DefaultConfiguration = _configuration;

            Init();
        }

        public virtual void Init()
        {
            _services.AddOptions();
            _services.AddLogging();
            _services.AddSingleton<IServiceCollection>(_ => _services);
            _services.AddSingleton(_configuration);
            _services.AddSingleton<IConfiguration>(_configuration);

            _services.AddLogging(loggingBuilder => {
                // configure Logging with NLog
                loggingBuilder.ClearProviders();
                loggingBuilder.SetMinimumLevel(LogLevel.Trace);
                loggingBuilder.AddNLog(_configuration);
            });
        }

        [TestMethod]
        public void VerifyMessageDeserialization()
        {
            _services.AddPanama(_configuration);
            var provider = _services.BuildServiceProvider();

            var message = new Message();
            message.AddData(new Foo());
            var that = message.ToInternal(provider);
            var @this = that.GetData<Message>(provider);

            Assert.AreEqual(message.Value?.To<Foo>()?.Value, @this.Value?.To<Foo>()?.Value);
        }

        [TestMethod]
        public async Task VerifyScheduledPollingPublishMessageState()
        {
            _services.AddPanama(
                configuration: _configuration,
                setup: options => {
                    options.UseCanal(canal => {
                        canal.UseDefaultDispatcher();
                        canal.UseDefaultStore();
                        canal.UseDefaultBroker();
                        canal.UseDefaultScheduler(scheduler => {
                            scheduler.RemoveJob<DelayedPublished>();
                            scheduler.RemoveJob<DeleteExpired>();
                            scheduler.RemoveJob<PublishedRetry>();
                            scheduler.RemoveJob<ReceivedRetry>();
                        });
                    });
                });

            _cts = new CancellationTokenSource();

            var provider = _services.BuildServiceProvider();
            var bootstrapper = provider.GetRequiredService<IBootstrapper>();

            await bootstrapper.On(_cts.Token);

            var options = provider.GetRequiredService<IOptions<CanalOptions>>();
            var channels = provider.GetRequiredService<IDefaultChannelFactory>();
            var context = new Context(provider);
            var foo = new Foo();
            var id = Guid.NewGuid().ToString();

            using (var channel = channels.CreateChannel<DefaultChannel>())
            {
                var result = await context.Bus()
                    .Id(id)
                    .Data(foo)
                    .Topic("foo.created")
                    .Channel(channel)
                    .Post();
                
                var store = provider.GetRequiredService<Store>();

                Assert.IsTrue(store.Published.Count == 1);
                Assert.IsNotNull(store.Published[id]);
                Assert.IsTrue(store.Published[id].Retries == 0);
                Assert.IsNull(store.Published[id].Expires);
                Assert.IsTrue(store.Published[id].Status == MessageStatus.Scheduled.ToString());
                Assert.IsNotNull(store.Published[id].Created);

                var message = store.Published[id].GetData<Message>(provider);

                Assert.IsNotNull(message);
                Assert.AreEqual(message.GetBrokerType(), typeof(DefaultTarget));
                Assert.AreEqual(message.GetBroker(), typeof(DefaultTarget).AssemblyQualifiedName);
                Assert.AreEqual(message.GetName(), options.Value.GetName("foo.created"));
                Assert.AreEqual(message.GetGroup(), options.Value.DefaultGroup);
            }

            await bootstrapper.Off(_cts.Token);
        }

        [TestMethod]
        public async Task VerifySucceededPollingPublishMessageState()
        {
            _services.AddPanama(
                configuration: _configuration,
                setup: options => {
                    options.UseCanal(canal => {
                        canal.UseDefaultDispatcher();
                        canal.UseDefaultStore();
                        canal.UseDefaultBroker();
                    });
                });

            _cts = new CancellationTokenSource();

            var provider = _services.BuildServiceProvider();
            var bootstrapper = provider.GetRequiredService<IBootstrapper>();

            await bootstrapper.On(_cts.Token);

            var options = provider.GetRequiredService<IOptions<CanalOptions>>();
            var channels = provider.GetRequiredService<IDefaultChannelFactory>();
            var context = new Context(provider);
            var foo = new Foo();
            var id = Guid.NewGuid().ToString();

            using (var channel = channels.CreateChannel<DefaultChannel>())
            {
                var result = await context.Bus()
                    .Id(id)
                    .Data(foo)
                    .Topic("foo.created")
                    .Channel(channel)
                    .Post();

                var store = provider.GetRequiredService<Store>();

                await channel.Commit();
                await Task.Delay(TimeSpan.FromSeconds(2));

                Assert.IsTrue(store.Published.Count == 1);
                Assert.IsNotNull(store.Published[id]);
                Assert.IsTrue(store.Published[id].Retries == 0);
                Assert.IsNotNull(store.Published[id].Expires);
                Assert.IsTrue(store.Published[id].Status == MessageStatus.Succeeded.ToString());
                Assert.IsNotNull(store.Published[id].Created);

                Assert.IsTrue(store.Received.Count == 1);
                Assert.IsNotNull(store.Received[id]);
                Assert.IsTrue(store.Received[id].Retries == 0);
                Assert.IsNotNull(store.Received[id].Expires);
                Assert.IsTrue(store.Received[id].Status == MessageStatus.Succeeded.ToString());
                Assert.IsNotNull(store.Received[id].Created);

                var message = store.Published[id].GetData<Message>(provider);

                Assert.IsNotNull(message);
                Assert.AreEqual(message.GetBrokerType(), typeof(DefaultTarget));
                Assert.AreEqual(message.GetBroker(), typeof(DefaultTarget).AssemblyQualifiedName);
                Assert.AreEqual(message.GetName(), options.Value.GetName("foo.created"));
                Assert.AreEqual(message.GetGroup(), options.Value.DefaultGroup);
            }

            await bootstrapper.Off(_cts.Token);
        }

        [TestMethod]
        public async Task VerifySucceededPollingPublishReplyMessageState()
        {
            _services.AddSingleton<State>();
            _services.AddPanama(
                configuration: _configuration,
                setup: options => {
                    options.UseCanal(canal => {
                        canal.UseDefaultDispatcher();
                        canal.UseDefaultStore();
                        canal.UseDefaultBroker();
                    });
                });

            _cts = new CancellationTokenSource();

            var provider = _services.BuildServiceProvider();
            var bootstrapper = provider.GetRequiredService<IBootstrapper>();

            await bootstrapper.On(_cts.Token);

            var options = provider.GetRequiredService<IOptions<CanalOptions>>();
            var channels = provider.GetRequiredService<IDefaultChannelFactory>();
            var context = new Context(provider);
            var foo = new Foo();
            var id = Guid.NewGuid().ToString();

            using (var channel = channels.CreateChannel<DefaultChannel>())
            {
                var result = await context.Bus()
                    .Id(id)
                    .Data(foo)
                    .Reply("foo.ack")
                    .Topic("foo.created")
                    .Channel(channel)
                    .Post();

                var store = provider.GetRequiredService<Store>();

                await channel.Commit();
                await Task.Delay(TimeSpan.FromSeconds(2));

                Assert.IsTrue(store.Published.Count == 2);
                Assert.IsNotNull(store.Published[id]);
                Assert.IsTrue(store.Published[id].Retries == 0);
                Assert.IsNotNull(store.Published[id].Expires);
                Assert.IsTrue(store.Published[id].Status == MessageStatus.Succeeded.ToString());
                Assert.IsNotNull(store.Published[id].Created);

                Assert.IsTrue(store.Received.Count == 2);
                Assert.IsNotNull(store.Received[id]);
                Assert.IsTrue(store.Received[id].Retries == 0);
                Assert.IsNotNull(store.Received[id].Expires);
                Assert.IsTrue(store.Received[id].Status == MessageStatus.Succeeded.ToString());
                Assert.IsNotNull(store.Received[id].Created);

                var message = store.Published[id].GetData<Message>(provider);

                Assert.IsNotNull(message);
                Assert.AreEqual(message.GetBrokerType(), typeof(DefaultTarget));
                Assert.AreEqual(message.GetBroker(), typeof(DefaultTarget).AssemblyQualifiedName);
                Assert.AreEqual(message.GetName(), options.Value.GetName("foo.created"));
                Assert.AreEqual(message.GetGroup(), options.Value.DefaultGroup);

                var state = provider.GetRequiredService<State>();
                var response = state.Data.ToList();

                Assert.IsTrue(response.KvpGet<string, string>("subscription.name").Any(k => k == nameof(FooCreated)));
                Assert.IsTrue(response.KvpGet<string, string>("subscription.name").Any(k => k == nameof(FooAcknowledged)));
            }

            await bootstrapper.Off(_cts.Token);
        }

        [TestMethod]
        public async Task VerifyDelayedSucceededPollingPublishUsingDefaultJob()
        {
            _services.AddSingleton<State>();
            _services.AddPanama(
                configuration: _configuration,
                setup: options => {
                    options.UseCanal(canal => {
                        canal.UseDefaultDispatcher();
                        canal.UseDefaultStore();
                        canal.UseDefaultBroker();
                        canal.UseDefaultScheduler();
                    });
                });

            _cts = new CancellationTokenSource();

            var provider = _services.BuildServiceProvider();
            var bootstrapper = provider.GetRequiredService<IBootstrapper>();

            await bootstrapper.On(_cts.Token);

            var options = provider.GetRequiredService<IOptions<CanalOptions>>();
            var channels = provider.GetRequiredService<IDefaultChannelFactory>();
            var context = new Context(provider);
            var foo = new Foo();
            var id = Guid.NewGuid().ToString();
            var now = DateTime.UtcNow;
            using (var channel = channels.CreateChannel<DefaultChannel>())
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

            var store = provider.GetRequiredService<Store>();
            
            await Task.Delay(TimeSpan.FromSeconds(100));

            Assert.IsTrue(store.Published.Count == 2);
            Assert.IsNotNull(store.Published[id]);
            Assert.IsTrue(store.Published[id].Retries == 0);
            Assert.IsNotNull(store.Published[id].Expires);
            Assert.IsTrue(store.Published[id].Status == MessageStatus.Succeeded.ToString());
            Assert.IsNotNull(store.Published[id].Created);

            Assert.IsTrue(store.Received.Count == 2);
            Assert.IsNotNull(store.Received[id]);
            Assert.IsTrue(store.Received[id].Retries == 0);
            Assert.IsNotNull(store.Received[id].Expires);
            Assert.IsTrue(store.Received[id].Status == MessageStatus.Succeeded.ToString());
            Assert.IsNotNull(store.Received[id].Created);

            var message = store.Published[id].GetData<Message>(provider);

            Assert.IsNotNull(message);
            Assert.AreEqual(message.GetBrokerType(), typeof(DefaultTarget));
            Assert.AreEqual(message.GetBroker(), typeof(DefaultTarget).AssemblyQualifiedName);
            Assert.AreEqual(message.GetName(), options.Value.GetName("foo.created"));
            Assert.AreEqual(message.GetGroup(), options.Value.DefaultGroup);

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