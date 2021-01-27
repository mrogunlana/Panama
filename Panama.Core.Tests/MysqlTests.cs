using Autofac;
using Autofac.Features.AttributeFilters;
using DapperExtensions;
using DapperExtensions.Mapper;
using DapperExtensions.Sql;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Panama.Core.Commands;
using Panama.Core.Entities;
using Panama.Core.IoC;
using Panama.Core.IoC.Autofac;
using Panama.Core.Logger;
using Panama.Core.MySql.Dapper;
using Panama.Core.MySql.Dapper.Interfaces;
using Panama.Core.Tests.Commands;
using Panama.Core.Tests.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Panama.Core.Tests
{
    [TestClass]
    public class MysqlTests
    {
        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            DapperExtensions.DapperExtensions.SqlDialect = new MySqlDialect();

            Environment.SetEnvironmentVariable("ASPNETCORE_MYSQL_SERVER", "localhost", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("ASPNETCORE_MYSQL_PORT", "3309", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("ASPNETCORE_MYSQL_DATABASE", "panama-core", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("ASPNETCORE_MYSQL_USER", "panama-db", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("ASPNETCORE_MYSQL_PASSWORD", "abc123", EnvironmentVariableTarget.Process);

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

            builder.RegisterType<SqlGeneratorImpl>()
               .As<ISqlGenerator>()
               .WithParameter("configuration", new DapperExtensionsConfiguration(typeof(ClassMapper<>), AppDomain.CurrentDomain.GetAssemblies(), new MySqlDialect()))
               .SingleInstance();

            builder.RegisterType<Logger.NLog>().As<ILog>();
            builder.RegisterType<MySqlQuery>().As<IMySqlQuery>();

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

            //Register all commands -- singletons
            builder.RegisterAssemblyTypes(domain)
                   .Where(t => t.IsAssignableTo<ICommandAsync>())
                   .Named<ICommandAsync>(t => t.Name)
                   .AsImplementedInterfaces()
                   .SingleInstance()
                   .WithAttributeFiltering();

            //Register all commands -- singletons
            builder.RegisterAssemblyTypes(domain)
                   .Where(t => t.IsAssignableTo<IRollback>())
                   .Named<IRollback>(t => t.Name)
                   .AsImplementedInterfaces()
                   .SingleInstance()
                   .WithAttributeFiltering();

            ServiceLocator.SetLocator(new AutofacServiceLocator(builder.Build()));
        }

        [TestMethod]
        public async Task DoesNewUpdateWithDefinitionWork()
        {
            var source = new CancellationTokenSource();
            var token = source.Token;
            var handler = await new Handler(ServiceLocator.Current)
                .Add(token)
                .Add(new User() { 
                    ID = Guid.NewGuid(),  
                    Email = "test@test.com",
                    FirstName = "John_UPDATED",
                    LastName = "Doe"
                })
                .Command<UpdateCommand>()
                .InvokeAsync();

            if (handler.Success)
                Assert.IsTrue(true);
            else
                Assert.Fail();
        }

        [TestMethod]
        public async Task DoesNewInsertWithDefinitionWork()
        {
            var source = new CancellationTokenSource();
            var token = source.Token;
            var handler = await new Handler(ServiceLocator.Current)
                .Add(token)
                .Add(new User() {
                    ID = Guid.NewGuid()
                })
                .Command<InsertCommand>()
                .InvokeAsync();

            if (handler.Success)
                Assert.IsTrue(true);
            else
                Assert.Fail();
        }

        [TestMethod]
        public async Task DoesCancellationStopLongRunningDatabaseTask()
        {
            var source = new CancellationTokenSource();
            source.CancelAfter(TimeSpan.FromSeconds(5));

            var result = await new Handler(ServiceLocator.Current)
                .Add(source.Token)
                .Command<LongRunningDatabaseCommand>()
                .InvokeAsync();

            if (result.Cancelled)
                Assert.IsTrue(true);
            else
                Assert.Fail();
        }

        [TestMethod]
        public async Task DoesCancellationInterruptSeriesOfLongRunningDatabaseTasks()
        {
            var source = new CancellationTokenSource();
            source.CancelAfter(TimeSpan.FromSeconds(15));

            var result = await new Handler(ServiceLocator.Current)
                .Add(source.Token)
                .Command<LongRunningDatabaseCommand>()
                .Command<LongRunningDatabaseCommand>()
                .Command<LongRunningDatabaseCommand>()
                .InvokeAsync();

            if (result.Cancelled)
                Assert.IsTrue(true);
            else
                Assert.Fail();
        }

        [TestMethod]
        public async Task DoesNewDeleteWithDefinitionWork()
        {
            var source = new CancellationTokenSource();
            var token = source.Token;
            var handler = await new Handler(ServiceLocator.Current)
                .Add(token)
                .Add(new User()
                {
                    ID = Guid.NewGuid()
                })
                .Command<InsertCommand>()
                .Command<SelectCommand>()
                .Command<DeleteCommand>()
                .InvokeAsync();

            if (handler.Success)
                Assert.IsTrue(true);
            else
                Assert.Fail();
        }

        [TestMethod]
        public async Task DoesSimpleRollbackCommandWork()
        {
            var id = Guid.NewGuid();
            var source = new CancellationTokenSource();
            var token = source.Token;
            var handler = await new Handler(ServiceLocator.Current)
                .Add(token)
                .Add(new User()
                {
                    ID = id
                })
                .Command<InsertCommand>()
                .Command<ExceptionCommand>()
                .Rollback<RollbackInsertCommand>()
                .InvokeAsync();

            var result = await new Handler(ServiceLocator.Current)
                .Add(token)
                .Add(new User()
                {
                    ID = id
                })
                .Command<SelectCommand>()
                .InvokeAsync();

            var user = result.DataGet<User>();

            Assert.IsNotNull(user);
        }
    }
}
