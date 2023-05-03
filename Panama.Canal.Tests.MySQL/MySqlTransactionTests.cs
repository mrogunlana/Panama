using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Panama.Extensions;
using Panama.Interfaces;
using Panama.Invokers;
using Panama.Models;
using Panama.Canal.Tests.MySQL.Commands;
using Panama.Canal.Tests.MySQL.Commands.EF;
using Panama.Canal.Tests.MySQL.Contexts;
using Panama.Canal.Tests.MySQL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Panama.Canal.Tests.MySQL
{
    [TestClass]
    public class MySqlTransactionTests
    {
        private IServiceProvider _provider;

        public MySqlTransactionTests()
        {
            var services = new ServiceCollection();

            services.AddOptions();
            services.AddLogging();
            services.AddSingleton<IServiceCollection>(_ => services);

            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.test.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            services.AddSingleton(configuration);
            services.AddSingleton<IConfiguration>(configuration);

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

            services.AddPanama(
                assemblies: domain,
                configuration: configuration);

            services.AddDbContext<AppDbContext>(options =>
            {
                var connectionString = configuration.GetConnectionString("MySql");
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
                    .LogTo(Console.WriteLine, LogLevel.Information)
                    .EnableDetailedErrors();
            });

            _provider = services.BuildServiceProvider();
        }

        [TestMethod]
        public async Task VerifyTransactionScope()
        {
            var save = await _provider.GetRequiredService<IHandler>()
                .Command<SaveGeneratedUser>()
                .Command<SaveGeneratedUser>()
                .Command<SaveGeneratedUser>()
                .Command<SaveGeneratedUserInline>()
                .Command<SaveGeneratedSetting>()
                .Set<ScopedInvoker>()
                .Invoke();

            Assert.IsTrue(save.Success);

            var query = await _provider.GetRequiredService<IHandler>()
                .Add(save.DataGet<User>().Select(x => new Kvp<string, Guid>("ID", x.ID)))
                .Query<GetUsers>()
                .Set<ScopedInvoker>()
                .Invoke();

            Assert.IsTrue(query.Success);
            Assert.AreEqual(query.DataGet<User>().Count, 4);

            await _provider.GetRequiredService<IHandler>()
                .Command<DeleteAllUsers>()
                .Invoke();
        }

        [TestMethod]
        public async Task VerifyTransactionRollback()
        {
            var save = await _provider.GetRequiredService<IHandler>()
                .Command<SaveGeneratedUser>()
                .Command<SaveGeneratedUser>()
                .Command<SaveGeneratedUser>()
                .Command<ThrowException>()
                .Set<ScopedInvoker>()
                .Invoke();

            Assert.IsTrue(save.Success);

            var query = await _provider.GetRequiredService<IHandler>()
                .Add(save.DataGet<User>().Select(x => new Kvp<string, Guid>("ID", x.ID)))
                .Query<GetUsers>()
                .Set<ScopedInvoker>()
                .Invoke();

            Assert.IsTrue(query.Success);
            Assert.AreEqual(query.DataGet<User>().Count, 0);

            await _provider.GetRequiredService<IHandler>()
                .Command<DeleteAllUsers>()
                .Invoke();

        }
    }
}
