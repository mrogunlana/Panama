using Autofac;
using Autofac.Features.AttributeFilters;
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
using KeyValuePair = Panama.Core.Entities.KeyValuePair;

namespace Panama.Core.Tests
{
    [TestClass]
    public class AsyncHandlerTests
    {
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
            var builder = new ContainerBuilder();

            builder.RegisterType<Logger.NLog>().As<ILog>();

            //Register all validators -- singletons
            builder.RegisterAssemblyTypes(domain)
                   .Where(t => t.IsAssignableTo<IValidation>())
                   .Named<IValidation>(t => t.Name)
                   .AsImplementedInterfaces()
                   .SingleInstance();

            //Register all commands -- singletons
            builder.RegisterAssemblyTypes(domain)
                   .Where(t => t.IsAssignableTo<ICommand>())
                   .Named<ICommand>(t => t.Name)
                   .AsImplementedInterfaces()
                   .SingleInstance()
                   .WithAttributeFiltering();

            ServiceLocator.SetLocator(new AutofacServiceLocator(builder.Build()));
        }

        [TestMethod]
        public async Task DoesConcurrentCommandsExecuteSerially()
        {
            var handler = await new Handler(ServiceLocator.Current)
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
    }
}
