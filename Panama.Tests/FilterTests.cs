using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Panama.Canal.Extensions;
using Panama.Canal.Models.Messaging;
using Panama.Extensions;
using Panama.Interfaces;
using System;
using System.Collections.Generic;

namespace Panama.Tests
{
    [TestClass]
    public class FilterTests
    {
        private IServiceProvider _provider;

        public FilterTests()
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
        public void VerifyFilterRemoval()
        {
            var data = new List<IModel>();
            var message = new InternalMessage() { Id = Guid.NewGuid().ToString() };

            data.Add(message);
            data.Queue(message);

            var queue = data.QueueGet<InternalMessage>();

            while (queue.Count > 0)
            {
                var value = queue.Dequeue();

                data.Dequeue(value);
                data.Published(value);
            }

            Assert.AreEqual(queue.Count, 0);
            Assert.AreEqual(data.DataGet<InternalMessage>().Count, 1);
            Assert.AreEqual(data.PublishedGet<InternalMessage>().Count, 1);
            Assert.AreEqual(data.QueueGet<InternalMessage>().Count, 0);
        }
    }
}
