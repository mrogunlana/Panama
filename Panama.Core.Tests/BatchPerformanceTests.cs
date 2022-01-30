using Autofac;
using Autofac.Features.AttributeFilters;
using DapperExtensions;
using DapperExtensions.Mapper;
using DapperExtensions.Sql;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Panama.Core.Commands;
using Panama.Core.IoC;
using Panama.Core.IoC.Autofac;
using Panama.Core.Logger;
using Panama.Core.MySql.Dapper;
using Panama.Core.MySql.Dapper.Interfaces;
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
    public class BatchPerformanceTests
    {
        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            DapperExtensions.DapperExtensions.SqlDialect = new MySqlDialect();

            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();

            // add all config values to environment variables
            foreach (var env in config.AsEnumerable())
                Environment.SetEnvironmentVariable(env.Key, config.GetValue<string>(env.Key));

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

            //Register convenience Csv client
            builder.RegisterType<MockCsvClient>().As<ICsvClient>().SingleInstance();

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
        public async Task ImportCsvDataAndPersistUsingMySqlConnector()
        {
            var result = await new Handler(ServiceLocator.Current)
                .Add(new KeyValuePair("Filename", @"data\test-data-100k.csv"))
                .Add(new KeyValuePair("Batch", 100))
                .Command<Get100kTestDataFromCsvAsModels>()
                .Command<InsertBatchCsvDataUsingMySqlConnector>()
                .InvokeAsync();

            Assert.IsTrue(result.Success);
        }

        [TestMethod]
        public async Task ImportCsvDataAndPersistUsingMySqlData()
        {
            var result = await new Handler(ServiceLocator.Current)
                .Add(new KeyValuePair("Filename", @"data\test-data-100k.csv"))
                .Add(new KeyValuePair("Batch", 100))
                .Command<Get100kTestDataFromCsvAsModels>()
                .Command<InsertBatchCsvDataUsingMySqlData>()
                .InvokeAsync();

            Assert.IsTrue(result.Success);
        }

        [TestMethod]
        public async Task ImportCsvDataAndPersistUsingPanamaCoreMySqlDapperLibrary()
        {
            var result = await new Handler(ServiceLocator.Current)
                .Add(new KeyValuePair("Filename", @"data\test-data-100k.csv"))
                .Add(new KeyValuePair("Batch", 100))
                .Command<Get100kTestDataFromCsvAsModels>()
                .Command<InsertBatchCsvDataUsingPanamaCoreMySqlDapperLibrary>()
                .InvokeAsync();

            Assert.IsTrue(result.Success);
        }
    }
}
