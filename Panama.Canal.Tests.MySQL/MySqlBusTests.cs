using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySqlConnector;
using Panama.Canal.Extensions;
using Panama.Canal.Interfaces;
using Panama.Canal.MySQL.Extensions;
using Panama.Canal.MySQL.Models;
using Panama.Canal.Tests.Modules.Models;
using Panama.Canal.Tests.Modules.Subscriptions;
using Panama.Extensions;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Panama.Canal.Tests.MySQL
{
    [TestClass]
    public class MySqlBusTests 
    {
        [TestInitialize]
        public void Init() => Startup._provider!.GetRequiredService<State>().Reset();

        [TestMethod]
        public async Task VerifyPost()
        {
            var options = Startup._provider!.GetRequiredService<IOptions<MySqlOptions>>().Value;
            var channels = Startup._provider!.GetRequiredService<IGenericChannelFactory>();
            var context = new Panama.Models.Context(Startup._provider!);
            
            using (var connection = new MySqlConnection(options.GetConnectionString()))
            using (var channel = channels.CreateChannel<IDbConnection, IDbTransaction>(connection, Startup._cts.Token))
            {
                var result = await context.Bus()
                    .Topic("foo.created")
                    .Channel(channel)
                    .Post();

                await channel.Commit(Startup._cts.Token);
                await Task.Delay(TimeSpan.FromSeconds(2));

                var state = Startup._provider!.GetRequiredService<State>();
                var response = state.Data.ToList();

                Assert.IsTrue(response.KvpGet<string, string>("subscription.name").Any(k => k == nameof(FooCreated)));
            }
        }
    }
}