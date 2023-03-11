using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Panama.Core.Tests.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Panama.Core.Service;
using Panama.Core.Interfaces;
using Panama.Core.Models;

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
            _serviceProvider = services.BuildServiceProvider();
        }

        [TestMethod]
        public async Task DoesConcurrentCommandsExecuteSerially()
        {
            var result = await new Handler(_serviceProvider.GetService<IInvokeHandler<IHandler>>(), _serviceProvider)
                .Command<SerialCommand1>()
                .Command<SerialCommand2>()
                .Command<SerialCommand3>()
                .Invoke();

            if (result.Data.Count == 0)
                Assert.Fail();

            var _1 = (result.Data[0] as Kvp<string, int>);
            var _2 = (result.Data[1] as Kvp<string, int>);
            var _3 = (result.Data[2] as Kvp<string, int>);

            if (_1.Value != 1)
                Assert.Fail();
            if (_2.Value != 2)
                Assert.Fail();
            if (_3.Value != 3)
                Assert.Fail();

            Assert.IsTrue(true);
        }

        [TestMethod]
        public async Task DoesConcurrentAsyncCommandsExecuteSerially()
        {
            var result = await new Handler(_serviceProvider.GetService<IInvokeHandler<IHandler>>(), _serviceProvider)
                .Command<AsyncSerialCommand1>()
                .Command<AsyncSerialCommand2>()
                .Command<AsyncSerialCommand3>()
                .Invoke();

            if (result.Data.Count == 0)
                Assert.Fail();

            var _1 = (result.Data[0] as Kvp<string, int>);
            var _2 = (result.Data[1] as Kvp<string, int>);
            var _3 = (result.Data[2] as Kvp<string, int>);

            if (_1.Value != 1)
                Assert.Fail();
            if (_2.Value != 2)
                Assert.Fail();
            if (_3.Value != 3)
                Assert.Fail();

            Assert.IsTrue(true);
        }

        [TestMethod]
        public async Task DoesAsyncandNonAsyncCommandsPlayNicelyTogether()
        {
            var result = await new Handler(_serviceProvider.GetService<IInvokeHandler<IHandler>>(), _serviceProvider)
                .Command<AsyncSerialCommand1>()
                .Command<AsyncSerialCommand2>()
                .Command<AsyncSerialCommand3>()
                .Command<SerialCommand4>()
                .Command<SerialCommand5>()
                .Invoke();

            if (result.Data.Count == 0)
                Assert.Fail();

            var _1 = (result.Data[0] as Kvp<string, int>);
            var _2 = (result.Data[1] as Kvp<string, int>);
            var _3 = (result.Data[2] as Kvp<string, int>);
            var _4 = (result.Data[3] as Kvp<string, int>);
            var _5 = (result.Data[4] as Kvp<string, int>);

            if (_1.Value != 1)
                Assert.Fail();
            if (_2.Value != 2)
                Assert.Fail();
            if (_3.Value != 3)
                Assert.Fail();
            if (_4.Value != 4)
                Assert.Fail();
            if (_5.Value != 5)
                Assert.Fail();

            Assert.IsTrue(true);
        }
    }
}
