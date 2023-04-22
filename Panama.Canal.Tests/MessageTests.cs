using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Panama.Canal.Channels;
using Panama.Canal.Extensions;
using Panama.Canal.Interfaces;
using Panama.Canal.Jobs;
using Panama.Canal.Models;
using Panama.Canal.Models.Options;
using Panama.Canal.Tests.Jobs;
using Panama.Canal.Tests.Models;
using Panama.Models;
using Panama.Security;
using System.Configuration;
using System.Reflection;

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

            _services.AddOptions();
            _services.AddLogging();
            _services.AddSingleton<IServiceCollection>(_ => _services);

            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.test.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            _services.AddSingleton(configuration);
            _services.AddSingleton<IConfiguration>(configuration);
            _configuration = configuration;
            _cts = new CancellationTokenSource();
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
        public async Task VerifyInitialStreamPublishMessageState()
        {
            _services.AddPanama(
                configuration: _configuration,
                setup: options => {
                    options.UseCanal(canal => {
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
                Assert.AreEqual(message.GetName(), "foo.created");
                Assert.AreEqual(message.GetGroup(), options.Value.DefaultGroup);
            }
        }
    }
}