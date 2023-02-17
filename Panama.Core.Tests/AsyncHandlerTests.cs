using Autofac;
using Autofac.Features.AttributeFilters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Panama.Core.Commands;
using Panama.Core.IoC;
using Panama.Core.IoC.Autofac;
using Panama.Core.Logger;
using Panama.Core.MySql.Dapper;
using Panama.Core.Tests.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Panama.Core.Service;
using KeyValuePair = Panama.Core.Entities.KeyValuePair;
using Panama.Core.MySql.Dapper.Interfaces;
using DapperExtensions.Sql;

namespace Panama.Core.Tests
{
    [TestClass]
    public class AsyncHandlerTests
    {
        private static IServiceProvider _serviceProvider { get; set; }
        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
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
            var services = new ServiceCollection();
            services.AddPanama(assemblies);
            services.AddSingleton<ILog, Logger.NLog>();
            _serviceProvider = services.BuildServiceProvider();
        }

        [TestMethod]
        public async Task DoesConcurrentCommandsExecuteSerially()
        {
            var handler = await new Handler(_serviceProvider, _serviceProvider.GetService<ILog>())
                .Command<SerialCommand1>()
                .Command<SerialCommand2>()
                .Command<SerialCommand3>()
                .InvokeAsync();

            if (handler.Data.Count == 0)
                Assert.Fail();

            var _1 = (handler.Data[0] as KeyValuePair);
            var _2 = (handler.Data[1] as KeyValuePair);
            var _3 = (handler.Data[2] as KeyValuePair);

            if (_1.Value.ToString() != "1")
                Assert.Fail();
            if (_2.Value.ToString() != "2")
                Assert.Fail();
            if (_3.Value.ToString() != "3")
                Assert.Fail();

            Assert.IsTrue(true);
        }

        [TestMethod]
        public async Task DoesConcurrentAsyncCommandsExecuteSerially()
        {
            var handler = await new Handler(_serviceProvider, _serviceProvider.GetService<ILog>())
                .Command<AsyncSerialCommand1>()
                .Command<AsyncSerialCommand2>()
                .Command<AsyncSerialCommand3>()
                .InvokeAsync();

            if (handler.Data.Count == 0)
                Assert.Fail();

            var _1 = (handler.Data[0] as KeyValuePair);
            var _2 = (handler.Data[1] as KeyValuePair);
            var _3 = (handler.Data[2] as KeyValuePair);

            if (_1.Value.ToString() != "1")
                Assert.Fail();
            if (_2.Value.ToString() != "2")
                Assert.Fail();
            if (_3.Value.ToString() != "3")
                Assert.Fail();

            Assert.IsTrue(true);
        }

        [TestMethod]
        public async Task DoesAsyncandNonAsyncCommandsPlayNicelyTogether()
        {
            var handler = await new Handler(_serviceProvider, _serviceProvider.GetService<ILog>())
                .Command<AsyncSerialCommand1>()
                .Command<AsyncSerialCommand2>()
                .Command<AsyncSerialCommand3>()
                .Command<SerialCommand4>()
                .Command<SerialCommand5>()
                .InvokeAsync();

            if (handler.Data.Count == 0)
                Assert.Fail();

            var _1 = (handler.Data[0] as KeyValuePair);
            var _2 = (handler.Data[1] as KeyValuePair);
            var _3 = (handler.Data[2] as KeyValuePair);
            var _4 = (handler.Data[3] as KeyValuePair);
            var _5 = (handler.Data[4] as KeyValuePair);

            if (_1.Value.ToString() != "1")
                Assert.Fail();
            if (_2.Value.ToString() != "2")
                Assert.Fail();
            if (_3.Value.ToString() != "3")
                Assert.Fail();
            if (_4.Value.ToString() != "4")
                Assert.Fail();
            if (_5.Value.ToString() != "5")
                Assert.Fail();

            Assert.IsTrue(true);
        }
    }
}
