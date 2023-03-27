using Autofac;
using DapperExtensions;
using DapperExtensions.Mapper;
using DapperExtensions.Sql;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Panama.Tests
{
    [TestClass]
    public class HandlerTests
    {
        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_MYSQL_SERVER", "localhost", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("ASPNETCORE_MYSQL_PORT", "3309", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("ASPNETCORE_MYSQL_DATABASE", "panama-core", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("ASPNETCORE_MYSQL_USER", "panama-db", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("ASPNETCORE_MYSQL_PASSWORD", "abc123", EnvironmentVariableTarget.Process);

            var builder = new ContainerBuilder();
            builder.RegisterType<SqlGeneratorImpl>()
               .As<ISqlGenerator>()
               .WithParameter("configuration", new DapperExtensionsConfiguration(typeof(ClassMapper<>), AppDomain.CurrentDomain.GetAssemblies(), new SqlServerDialect()))
               .SingleInstance();

        }

        [TestCleanup]
        public void Cleanup()
        {
            
        }

        
    }
}
