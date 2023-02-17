using Autofac;
using Autofac.Features.AttributeFilters;
using DapperExtensions;
using DapperExtensions.Mapper;
using DapperExtensions.Sql;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Panama.Core.Commands;
using Panama.Core.Entities;
using Panama.Core.IoC;
using Panama.Core.IoC.Autofac;
using Panama.Core.Logger;
using Panama.Core.MySql.Dapper;
using Panama.Core.MySql.Dapper.Interfaces;
using Panama.Core.Service;
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
        private static IServiceProvider _serviceProvider { get; set; }
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
            var services = new ServiceCollection();
            services.AddPanama(assemblies);
            services.AddSingleton<ISqlGenerator, SqlGeneratorImpl>();
            services.AddSingleton<IDapperExtensionsConfiguration, DapperExtensionsConfiguration>();
            services.AddSingleton<IQueryAsync, SqlQueryAsync>();
            services.AddSingleton<ILog, Logger.NLog>();
            _serviceProvider = services.BuildServiceProvider();
        }

        [TestMethod]
        public async Task CanWeQueryWithReadSingleSelectStatement()
        {
            var ID = Guid.NewGuid();
        }
    }
}
