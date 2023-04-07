using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Panama.Canal.Extensions;
using Panama.Canal.Models;
using Panama.Extensions;
using Panama.Interfaces;
using Panama.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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

            var assemblies = new List<Assembly>();

            // domain built like so to overcome .net core .dll discovery issue 
            // within container:
            assemblies.Add(Assembly.GetExecutingAssembly());
            assemblies.AddRange(AppDomain.CurrentDomain.GetAssemblies());
            assemblies.AddRange(Assembly
                .GetExecutingAssembly()
                .GetReferencedAssemblies()
                .Select(x => Assembly.Load(x))
                .ToList());

            var domain = assemblies.ToArray();

            services.AddPanama(domain);
            services.AddPanamaSecurity();

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
