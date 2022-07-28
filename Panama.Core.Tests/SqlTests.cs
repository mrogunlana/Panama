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
using Panama.Core.Sql;
using Panama.Core.Sql.Dapper;
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
    public class SqlTests
    {
        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_MSSQL_SERVER", "localhost", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("ASPNETCORE_MSSQL_PORT", "1433", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("ASPNETCORE_MSSQL_DATABASE", "panama", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("ASPNETCORE_MSSQL_USER", "sa", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("ASPNETCORE_MSSQL_PASSWORD", "Jf4UZh4Lz64AbqbG", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("LOGDB_CONNECTION_STRING", "Server=127.0.0.1;Port=1433;Database=logdb;User Id=sa;Password=Jf4UZh4Lz64AbqbG;", EnvironmentVariableTarget.Process);

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
               .WithParameter("configuration", new DapperExtensionsConfiguration())
               .SingleInstance();

            builder.RegisterType<Logger.NLog>().As<ILog>();
            builder.RegisterType<SqlQueryAsync>().As<IQueryAsync>();

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
        public async Task CanWeQueryWithReadSingleSelectStatement()
        {
            var ID = Guid.NewGuid();

            var result = await new TransactionHandler(ServiceLocator.Current)
                .Add(new User()
                {
                    ID = ID,
                    Email = "test@test.com",
                    FirstName = "John_UPDATED",
                    LastName = "Doe"
                })
                .Command<InsertCommandUsingMssql>()
                .Command<SelectReadSingleCommandUsingMssql>()
                .InvokeAsync();


            var user = result.DataGetSingle<User>();

            Assert.IsNotNull(user);
        }
    }
}
