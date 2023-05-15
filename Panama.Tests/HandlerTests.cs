using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Panama.Extensions;
using Panama.Interfaces;
using Panama.Invokers;
using Panama.Tests.Commands;
using System;
using System.Threading.Tasks;

namespace Panama.Tests
{
    [TestClass]
    public class HandlerTests
    {
        private IServiceProvider _provider;

        public HandlerTests()
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

            services.AddPanama(configuration);

            _provider = services.BuildServiceProvider();
        }

        [TestMethod]
        public void VerifyEnvironmentVariables()
        {
            var configuration = _provider.GetRequiredService<IConfiguration>();

            var user = configuration.GetValue<string>("ASPNETCORE_MYSQL_USER");
            var server = configuration.GetValue<string>("ASPNETCORE_MYSQL_SERVER");
            var database = configuration.GetValue<string>("ASPNETCORE_MYSQL_DATABASE");
            var port = configuration.GetValue<int>("ASPNETCORE_MYSQL_PORT");
            var password = configuration.GetValue<string>("ASPNETCORE_MYSQL_PASSWORD");
            var environment = configuration.GetValue<string>("ASPNETCORE_ENVIRONMENT");

            Assert.IsNotNull(user);
            Assert.IsNotNull(server);
            Assert.IsNotNull(database);
            Assert.IsNotNull(port);
            Assert.IsNotNull(password);
            Assert.IsNotNull(environment);
        }

        [TestMethod]
        public async Task VerifyAmbientTransactionScope()
        {
            var handler = _provider.GetRequiredService<IHandler>();

            var result = await handler
                .Command<VerifyAmbientTransaction>()
                .Set<ScopedInvoker>()
                .Invoke();

            Assert.IsTrue(result.Success);
            Assert.IsTrue(result.KvpGetSingle<string, bool>("HasTransaction"));
        }

        [TestMethod]
        public async Task VerifyDefaultTransactionScope()
        {
            var handler = _provider.GetRequiredService<IHandler>();

            var result = await handler
                .Command<VerifyAmbientTransaction>()
                .Set<DefaultInvoker>()
                .Invoke();

            Assert.IsTrue(result.Success);
            Assert.IsFalse(result.KvpGetSingle<string, bool>("HasTransaction"));
        }
    }
}
