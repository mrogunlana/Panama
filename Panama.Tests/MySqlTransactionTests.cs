using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Panama.Canal.Extensions;
using Panama.Canal.Models;
using Panama.Extensions;
using Panama.Interfaces;
using Panama.Invokers;
using Panama.Security;
using Panama.Service;
using Panama.Tests.Commands;
using Panama.Tests.Commands.EF;
using Panama.Tests.Contexts;
using Panama.Tests.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Panama.Tests
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

            services.AddPanama(domain);
            services.AddPanamaSecurity();

            services.AddDbContext<MySqlDbContext>(options => {
                var connectionString = configuration.GetConnectionString("DefaultConnection");
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
                .Set<ScopedInvoker>()
                .Invoke();

            Assert.IsTrue(save.Success);

            var query = await _provider.GetRequiredService<IHandler>()
                .Query<GetUsers>()
                .Set<ScopedInvoker>()
                .Invoke();

            Assert.IsTrue(query.Success);
            Assert.AreEqual(query.DataGet<User>().Count, 3);
        }

        [TestMethod]
        public async Task VerifyTransactionRollback()
        {
            var delete = await _provider.GetRequiredService<IHandler>()
                .Query<GetUsers>()
                .Command<DeleteUsers>()
                .Set<ScopedInvoker>()
                .Invoke();

            var save = await _provider.GetRequiredService<IHandler>()
                .Command<SaveGeneratedUser>()
                .Command<SaveGeneratedUser>()
                .Command<SaveGeneratedUser>()
                .Command<ThrowException>()
                .Set<ScopedInvoker>()
                .Invoke();

            Assert.IsFalse(save.Success);

            var query = await _provider.GetRequiredService<IHandler>()
                .Query<GetUsers>()
                .Set<ScopedInvoker>()
                .Invoke();

            Assert.IsTrue(query.Success);
            Assert.AreEqual(query.DataGet<User>().Count, 0);
        }
    }
}
