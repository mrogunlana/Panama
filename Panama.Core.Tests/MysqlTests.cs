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
using System.Transactions;
using KeyValuePair = Panama.Core.Entities.KeyValuePair;

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
            Environment.SetEnvironmentVariable("ASPNETCORE_MYSQL_DATABASE_TEMP", "tempdb", EnvironmentVariableTarget.Process);
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
                .Command<SelectByIdCommand>()
                .Command<ExceptionCommand>()
                .Rollback<RollbackInsertCommand>()
                .InvokeAsync();

            var result = await new Handler(ServiceLocator.Current)
                .Add(token)
                .Add(new User()
                {
                    ID = id
                })
                .Command<SelectByIdCommand>()
                .InvokeAsync();

            var user = result.DataGetSingle<User>();

            Assert.IsNull(user);
        }

        [TestMethod]
        public async Task DoesTransactionScopeAutomaticallyEnlistAmbientConnectionsForRollbackCase()
        {
            var ID = Guid.NewGuid();
            
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                var response = await new Handler(ServiceLocator.Current)
                    .Add(new User() {
                        ID = ID,
                        Email = "test@test.com",
                        FirstName = "John_UPDATED",
                        LastName = "Doe"
                    })
                    .Command<InsertCommand>()
                    .Command<InsertCommandANewRandomUser>()
                    .Command<InsertCommandANewRandomUser>()
                    .Command<InsertCommandANewRandomUser>()
                    .Command<InsertCommandWithRunTimeException>()
                    .InvokeAsync();

                if (response.Success)
                    scope.Complete();
            }

            var result = await new Handler(ServiceLocator.Current)
                .Add(new KeyValuePair("ID", ID))
                .Command<SelectByIdCommand>()
                .InvokeAsync();

            var user = result.DataGetSingle<User>();

            Assert.IsNull(user);
        }

        [TestMethod]
        public async Task DoesTransactionScopeAutomaticallyEnlistAmbientConnectionsForSuccessCase()
        {
            var ID = Guid.NewGuid();

            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                var response = await new Handler(ServiceLocator.Current)
                    .Add(new User()
                    {
                        ID = ID,
                        Email = "test@test.com",
                        FirstName = "John_UPDATED",
                        LastName = "Doe"
                    })
                    .Command<InsertCommand>()
                    .Command<InsertCommandANewRandomUser>()
                    .InvokeAsync();

                if (response.Success)
                    scope.Complete();
            }

            var result = await new Handler(ServiceLocator.Current)
                .Add(new KeyValuePair("ID", ID))
                .Command<SelectByIdCommand>()
                .InvokeAsync();

            var user = result.DataGetSingle<User>();

            Assert.IsNotNull(user);
        }

        [TestMethod]
        public async Task DoesTransactionScopeAutomaticallyEnlistAmbientConnectionsWithBatchInsertForSuccessCase()
        {
            var ID = Guid.NewGuid();

            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                var response = await new Handler(ServiceLocator.Current)
                    .Add(new User()
                    {
                        ID = ID,
                        Email = "test@test.com",
                        FirstName = "John_UPDATED",
                        LastName = "Doe"
                    })
                    .Command<InsertCommand>()
                    .Command<InsertCommandSomeBatchRandomUsers>()
                    .InvokeAsync();

                if (response.Success)
                    scope.Complete();
            }

            var result = await new Handler(ServiceLocator.Current)
                .Add(new KeyValuePair("ID", ID))
                .Command<SelectByIdCommand>()
                .InvokeAsync();

            var user = result.DataGetSingle<User>();

            Assert.IsNotNull(user);
        }

        [TestMethod]
        public async Task DoesTransactionScopeAutomaticallyEnlistAmbientConnectionsWithBatchInsertForRollbackCase()
        {
            var ID = Guid.NewGuid();

            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                var response = await new Handler(ServiceLocator.Current)
                    .Add(new User()
                    {
                        ID = ID,
                        Email = "test@test.com",
                        FirstName = "John_UPDATED",
                        LastName = "Doe"
                    })
                    .Command<InsertCommand>()
                    .Command<InsertCommandSomeBatchRandomUsers>()
                    .Command<InsertCommandWithRunTimeException>()
                    .InvokeAsync();

                if (response.Success)
                    scope.Complete();
            }

            var result = await new Handler(ServiceLocator.Current)
                .Add(new KeyValuePair("ID", ID))
                .Command<SelectByIdCommand>()
                .InvokeAsync();

            var user = result.DataGetSingle<User>();

            Assert.IsNull(user);
        }

        [TestMethod]
        public async Task DoesTransactionScopeAutomaticallyEnlistAmbientConnectionsWithBatchInsertForSuccessCaseUsingNewTransactionHandler()
        {
            var ID = Guid.NewGuid();

            await new TransactionHandler(ServiceLocator.Current)
                .Add(new User()
                {
                    ID = ID,
                    Email = "test@test.com",
                    FirstName = "John_UPDATED",
                    LastName = "Doe"
                })
                .Command<InsertCommand>()
                .Command<InsertCommandSomeBatchRandomUsers>()
                .Command<InsertCommandUsingExecuteSql>()
                .InvokeAsync();

            var result = await new Handler(ServiceLocator.Current)
                .Add(new KeyValuePair("ID", ID))
                .Command<SelectByIdCommand>()
                .InvokeAsync();

            var user = result.DataGetSingle<User>();

            Assert.IsNotNull(user);
        }

        [TestMethod]
        public async Task DoesTransactionScopeAutomaticallyEnlistAmbientConnectionsWithBatchInsertForRollbackCaseUsingNewTransactionHandler()
        {
            var ID = Guid.NewGuid();

            await new TransactionHandler(ServiceLocator.Current)
                .Add(new User()
                {
                    ID = ID,
                    Email = "test@test.com",
                    FirstName = "John_UPDATED",
                    LastName = "Doe"
                })
                .Command<InsertCommand>()
                .Command<InsertCommandSomeBatchRandomUsers>()
                .Command<InsertCommandUsingExecuteSql>()
                .Command<InsertCommandWithRunTimeException>()
                .InvokeAsync();

            var result = await new Handler(ServiceLocator.Current)
                .Add(new KeyValuePair("ID", ID))
                .Command<SelectByIdCommand>()
                .InvokeAsync();

            var user = result.DataGetSingle<User>();

            Assert.IsNull(user);
        }

        [TestMethod]
        public async Task DoesTransactionScopeAutomaticallyEnlistAmbientConnectionsWithBatchInsertForSuccessCaseUsingNewTransactionHandlerAcrossMultipleDatabasesOnSameServer()
        {
            var ID = Guid.NewGuid();

            await new TransactionHandler(ServiceLocator.Current)
                .Add(new User()
                {
                    ID = ID,
                    Email = "test@test.com",
                    FirstName = "John_UPDATED",
                    LastName = "Doe"
                })
                .Command<InsertCommand>()
                .Command<InsertCommandSomeBatchRandomUsersOnDifferentDatabase>()
                .Command<InsertCommandUsingExecuteSql>()
                .InvokeAsync();

            var result = await new Handler(ServiceLocator.Current)
                .Add(new KeyValuePair("ID", ID))
                .Command<SelectByIdCommand>()
                .InvokeAsync();

            var user = result.DataGetSingle<User>();

            Assert.IsNotNull(user);
        }

        [TestMethod]
        public async Task DoesTransactionScopeAutomaticallyEnlistAmbientConnectionsWithBatchInsertForRollbackCaseUsingNewTransactionHandlerAcrossMultipleDatabasesOnSameServer()
        {
            var ID = Guid.NewGuid();

            await new TransactionHandler(ServiceLocator.Current)
                .Add(new User()
                {
                    ID = ID,
                    Email = "test@test.com",
                    FirstName = "John_UPDATED",
                    LastName = "Doe"
                })
                .Command<InsertCommand>()
                .Command<InsertCommandSomeBatchRandomUsers>()
                .Command<InsertCommandSomeBatchRandomUsersOnDifferentDatabase>()
                .Command<InsertCommandUsingExecuteSql>()
                .Command<InsertCommandWithRunTimeException>()
                .InvokeAsync();

            var result = await new Handler(ServiceLocator.Current)
                .Add(new KeyValuePair("ID", ID))
                .Command<SelectByIdCommand>()
                .InvokeAsync();

            var user = result.DataGetSingle<User>();

            Assert.IsNull(user);
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
                .Command<InsertCommand>()
                .Command<SelectReadSingleCommand>()
                .InvokeAsync();


            var user = result.DataGetSingle<User>();

            Assert.IsNotNull(user);
        }

        [TestMethod]
        public async Task CanWeQueryWithReadMultipleSelectStatement()
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
                .Command<InsertCommand>()
                .Command<InsertCommandSomeBatchRandomUsers>()
                .Command<SelectReadMultipleCommand>()
                .InvokeAsync();


            var users = result.DataGet<User>();

            Assert.IsTrue(users.Count > 1);
        }

        [TestMethod]
        public async Task Insert1UnitTest()
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
                .Command<InsertCommand>()
                .InvokeAsync();

            var user = result.DataGetSingle<User>();

            Assert.IsTrue(user._ID > 0);
        }

        [TestMethod]
        public async Task Insert2UnitTest()
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
                .Command<InsertV2Command>()
                .InvokeAsync();

            var user = result.DataGetSingle<User>();

            Assert.IsTrue(user._ID > 0);
        }

        [TestMethod]
        public async Task Insert3UnitTest()
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
                .Command<InsertV3Command>()
                .InvokeAsync();

            var user = result.DataGetSingle<User>();

            Assert.IsTrue(user._ID > 0);
        }

        [TestMethod]
        public async Task ModifyVolitileDataInTransactionScope()
        {
            var prerequisite = await new TransactionHandler(ServiceLocator.Current)
                .Add(new User() {
                    ID = Guid.NewGuid(),
                    Email = "test@test.com",
                    FirstName = "John_UPDATED",
                    LastName = "Doe"
                })
                .Command<SaveCommand>()
                .InvokeAsync();

            var user = prerequisite.DataGetSingle<User>();

            var result = await new TransactionHandler(ServiceLocator.Current)
                .Add(new KeyValuePair("_ID", user._ID))
                .Command<SelectBy_IDCommand>()
                .Command<ModifyUserName>()
                .Command<UpdateCommand>()
                .Command<SelectBy_IDCommand>()
                .Command<ModifyUserName>()
                .Command<UpdateCommand>()
                .InvokeAsync();

            var final = result.DataGetSingle<User>();

            Assert.IsNotNull(final);
        }

        [TestMethod]
        public async Task ModifyNewlyVolitileDataInTransactionScopeShouldFail()
        {
            try
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
                    .Command<InsertV3Command>()
                    .Command<ModifyUserName>()
                    .Command<UpdateCommand>()
                    .InvokeAsync();

                var user = result.DataGet<User>();

                Assert.Fail();
            }
            catch (Exception ex)
            {
                Assert.IsNotNull(ex);
            }
        }

        [TestMethod]
        public async Task DoesNewSaveUserReturnAn_ID()
        {
            var source = new CancellationTokenSource();
            var token = source.Token;
            var handler = await new Handler(ServiceLocator.Current)
                .Add(token)
                .Add(new User()
                {
                    ID = Guid.NewGuid()
                })
                .Command<SaveCommand>()
                .InvokeAsync();

            if (!handler.Success)
                Assert.Fail();

            var user = handler.DataGetSingle<User>();
            if (user == null)
                Assert.Fail();

            Assert.IsTrue(user._ID > 0);
        }

        [TestMethod]
        public async Task DoesInsertUserReturnAn_ID()
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
                .InvokeAsync();

            if (!handler.Success)
                Assert.Fail();

            var user = handler.DataGetSingle<User>();
            if (user == null)
                Assert.Fail();

            Assert.IsTrue(user._ID > 0);
        }

    }
}
