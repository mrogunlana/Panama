using Autofac;
using DapperExtensions;
using DapperExtensions.Mapper;
using DapperExtensions.Sql;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Panama.Core.IoC;
using Panama.Core.IoC.Autofac;
using Panama.Core.Logger;
using Panama.Core.Sql;
using Panama.Core.Tests.Models;
using Panama.MySql.Dapper;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace Panama.Core.Tests
{
    [TestClass]
    public class BatchTests
    {
        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<SqlGeneratorImpl>()
               .As<ISqlGenerator>()
               .WithParameter("configuration", new DapperExtensionsConfiguration(typeof(ClassMapper<>), AppDomain.CurrentDomain.GetAssemblies(), new SqlServerDialect()))
               .SingleInstance();

            builder.RegisterType<Logger.NLog>().As<ILog>();
            builder.RegisterType<MySqlQuery>().As<IQuery>();

            ServiceLocator.SetLocator(new AutofacServiceLocator(builder.Build()));
        }

        [TestCleanup]
        public void Cleanup()
        {
            var sql = ServiceLocator.Current.Resolve<IQuery>();

            sql.Execute("delete from User", null);
        }

        [TestMethod]
        public void CanBatchInsert10Users()
        {
            var sql = ServiceLocator.Current.Resolve<IQuery>();
            var users = new List<User>();

            for (int i = 0; i < 10; i++)
                users.Add(new User() {
                    ID = Guid.NewGuid(),
                    Email = $"User{i}@test.com"
                });

            sql.InsertBatch(users);

            var results = sql.ExecuteScalar<int>($"select count(*) from User", null);

            Assert.AreEqual(users.Count, results);
        }
    }
}
