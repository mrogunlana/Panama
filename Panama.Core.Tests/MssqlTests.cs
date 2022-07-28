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
using Panama.Core.Tests.Commands;
using Panama.Core.Tests.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using KeyValuePair = Panama.Core.Entities.KeyValuePair;

namespace Panama.Core.Tests
{
    [TestClass]
    public class MssqlTests
    {
        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            DapperExtensions.DapperExtensions.SqlDialect = new SqlServerDialect();

            Environment.SetEnvironmentVariable("ASPNETCORE_MSSQL_SERVER", "localhost", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("ASPNETCORE_MSSQL_PORT", "1433", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("ASPNETCORE_MSSQL_DATABASE", "logdb", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("ASPNETCORE_MSSQL_DATABASE_TEMP", "tempdb", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("ASPNETCORE_MSSQL_USER", "serviceuser", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("ASPNETCORE_MSSQL_PASSWORD", "Password1", EnvironmentVariableTarget.Process);

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
               .WithParameter("configuration", new DapperExtensionsConfiguration(typeof(ClassMapper<>), AppDomain.CurrentDomain.GetAssemblies(), new SqlServerDialect()))
               .SingleInstance();

            builder.RegisterType<Logger.NLog>().As<ILog>();
            builder.RegisterType<SqlServerQuery>().As<IQuery>();

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

            ServiceLocator.SetLocator(new AutofacServiceLocator(builder.Build()));
        }

        [TestMethod]
        public async Task CanWeQueryWithReadSingleSelectStatement()
        {
            var result = await new Handler(ServiceLocator.Current)
                .Add(new Log()
                {
                    MachineName = "replica-5",
                    Level = "Trace",
                    Message = "Yes, it works",
                    CallSite = "Panama.Core.Logger.NLog.LogTrace",
                    Logger = "Panama.Core.Commands.Handler",
                })
                .Command<InsertLogEntryCommand>()
                .Command<SelectReadSingleLogEntryCommand>()
                .InvokeAsync();


            var model = result.DataGetSingle<Log>();

            Assert.IsNotNull(model);
        }
    }
}
