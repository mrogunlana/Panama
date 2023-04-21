using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Panama.Canal.Channels;
using Panama.Canal.Extensions;
using Panama.Canal.Interfaces;
using Panama.Canal.Jobs;
using Panama.Canal.Models;
using Panama.Canal.Tests.Jobs;
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

        [TestInitialize]
        public async Task Init()
        {
            _cts = new CancellationTokenSource();

            //await _bootstrapper.On(_cts.Token);
        }

        [TestCleanup]
        public async Task Cleanup()
        {
            _cts.Cancel();

            //await _bootstrapper.Off(_cts.Token);

            _cts.Dispose();
        }

        [TestMethod]
        public async Task VerifyPost()
        {
            _services.AddPanama(
                configuration: _configuration,
                setup: options => {
                    options.UseCanal(canal => {
                        canal.UseDefaultStore();
                        canal.UseDefaultBroker();
                        canal.UseDefaultScheduler(scheduler => {
                            scheduler.RemoveJob<DelayedPublished>();
                            scheduler.AddJob<DelayedPublished>("* * * * * ?");

                            //add custom jobs to process outbox/inbox messages:
                            scheduler.AddJob<PublishOutbox>("* * * * * ?");
                            scheduler.AddJob<ReceiveInbox>("* * * * * ?");
                        });
                    });
                });

            _cts = new CancellationTokenSource();

            var provider = _services.BuildServiceProvider();
            var bootstrapper = provider.GetRequiredService<IBootstrapper>();



            var channels = provider.GetRequiredService<IDefaultChannelFactory>();
            var context = new Context(provider);
            
            using (var channel = channels.CreateChannel<DefaultChannel>())
            {
                var result = await context.Bus()
                    .Topic("foo.created")
                    .Channel(channel)
                    .Post();

                await Task.Delay(TimeSpan.FromSeconds(2));

                Assert.IsTrue(result.Success);
            }
        }
    }
}