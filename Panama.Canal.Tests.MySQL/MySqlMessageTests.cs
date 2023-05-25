using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySqlConnector;
using Panama.Canal.Extensions;
using Panama.Canal.Interfaces;
using Panama.Canal.Models.Messaging;
using Panama.Canal.MySQL.Extensions;
using Panama.Canal.MySQL.Models;
using Panama.Canal.Tests.Modules.Models;
using Panama.Canal.Tests.Modules.Subscriptions;
using Panama.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Context = Panama.Models.Context;

namespace Panama.Canal.Tests.MySQL
{
    [TestClass]
    public class MySqlMessageTests 
    {
        [TestInitialize]
        public void Init() => Startup._provider!.GetRequiredService<State>().Reset();
        
        [TestMethod]
        public async Task VerifySucceededStreamingPublishMessageState()
        {
            var options = Startup._provider!.GetRequiredService<IOptions<MySqlOptions>>().Value;
            var channels = Startup._provider!.GetRequiredService<IGenericChannelFactory>();
            var context = new Context(Startup._provider!);
            var foo = new Foo();
            var id = Guid.NewGuid().ToString();

            using (var connection = new MySqlConnection(options.GetConnectionString()))
            using (var channel = channels.CreateChannel<IDbConnection, IDbTransaction>(connection, Startup._cts.Token))
            {
                var result = await context.Bus()
                    .Id(id)
                    .Data(foo)
                    .Topic("foo.created")
                    .Channel(channel)
                    .Post();

                await channel.Commit();
                await Task.Delay(TimeSpan.FromSeconds(2));

                var state = Startup._provider!.GetRequiredService<State>();
                var response = state.Data.ToList();

                Assert.IsTrue(response.KvpGet<string, string>("subscription.name").Any(k => k == nameof(FooCreated)));
            }
        }

        [TestMethod]
        public async Task VerifySucceededStreamingPublishReplyMessageState()
        {
            var options = Startup._provider!.GetRequiredService<IOptions<MySqlOptions>>().Value;
            var channels = Startup._provider!.GetRequiredService<IGenericChannelFactory>();
            var context = new Context(Startup._provider!);
            var foo = new Foo();
            var id = Guid.NewGuid().ToString();

            using (var connection = new MySqlConnection(options.GetConnectionString()))
            using (var channel = channels.CreateChannel<IDbConnection, IDbTransaction>(connection, Startup._cts.Token))
            {
                var result = await context.Bus()
                    .Id(id)
                    .Data(foo)
                    .Reply("foo.ack")
                    .Topic("foo.created")
                    .Channel(channel)
                    .Post();

                await channel.Commit();
                await Task.Delay(TimeSpan.FromSeconds(2));

                var state = Startup._provider!.GetRequiredService<State>();
                var response = state.Data.ToList();

                Assert.IsTrue(response.KvpGet<string, string>("subscription.name").Any(k => k == nameof(FooCreated)));
                Assert.IsTrue(response.KvpGet<string, string>("subscription.name").Any(k => k == nameof(FooAcknowledged)));
            }
        }

        [TestMethod]
        public async Task VerifyDelayedSucceededStreamingPublishUsingDefaultJob()
        {
            var options = Startup._provider!.GetRequiredService<IOptions<MySqlOptions>>().Value;
            var channels = Startup._provider!.GetRequiredService<IGenericChannelFactory>();
            var context = new Context(Startup._provider!);
            var foo = new Foo();
            var id = Guid.NewGuid().ToString();
            var now = DateTime.UtcNow;

            using (var connection = new MySqlConnection(options.GetConnectionString()))
            using (var channel = channels.CreateChannel<IDbConnection, IDbTransaction>(connection, Startup._cts.Token))
            {
                var result = await context.Bus()
                    .Id(id)
                    .Data(foo)
                    .Reply("foo.ack")
                    .Topic("foo.created")
                    .Delay(TimeSpan.FromSeconds(75))
                    .Channel(channel)
                    .Post();

                await channel.Commit();
            }

            await Task.Delay(TimeSpan.FromSeconds(90));

            var state = Startup._provider!.GetRequiredService<State>();
            var response = state.Data.ToList();

            Assert.IsTrue(response.KvpGet<string, string>("subscription.name").Any(k => k == nameof(FooCreated)));
            Assert.IsTrue(response.KvpGet<string, string>("subscription.name").Any(k => k == nameof(FooAcknowledged)));

            var then = response.KvpGetSingle<string, DateTime>("FooCreated.DateTime");

            var delay = (then - now).TotalSeconds;
            var log = Startup._provider!.GetRequiredService<ILogger<MySqlMessageTests>>();

            log.LogInformation($"Total delayed message processing time in seconds: {delay}");
        }

        [TestMethod]
        public async Task VerifyDelayedPublishJobChangesStateProperly()
        {
            var log = Startup._provider!.GetRequiredService<ILogger<MySqlMessageTests>>();
            var id = Guid.NewGuid().ToString();

            var options = Startup._provider!.GetRequiredService<IOptions<MySqlOptions>>();
            var settings = Startup._provider!.GetRequiredService<MySqlSettings>();
            var store = Startup._provider!.GetRequiredService<IStore>();
            var context = new Context(Startup._provider!);

            var channels = Startup._provider!.GetRequiredService<IGenericChannelFactory>();
            using (var connection = new MySqlConnection(options.Value.GetConnectionString()))
            using (var channel = channels.CreateChannel<IDbConnection, IDbTransaction>(connection, Startup._cts.Token))
            {
                await context.Bus()
                    .Id(id)
                    .Data(new Foo() { Value = DateTime.UtcNow.ToLongDateString() })
                    .Topic("foo.created")
                    .Reply("foo.ack")
                    .Channel(channel)
                    .Delay(TimeSpan.FromSeconds(75))
                    .Post();

                await channel.Commit();
            }

            log.LogInformation($"Stored temporary message.Id: {id}.");

            await Task.Delay(TimeSpan.FromSeconds(90));

            using (var connection = new MySqlConnection($"Server={options.Value.Host};Port={options.Value.Port};Database={options.Value.Database};Uid={options.Value.Username};Pwd={options.Value.Password};Allow User Variables=True;"))
            {
                if (connection.State == ConnectionState.Closed)
                    await connection.OpenAsync().ConfigureAwait(false);
                using var command = new MySqlCommand($@"

                    SET @_Id = (SELECT _Id FROM `{options.Value.Database}`.`{settings.PublishedTable}` WHERE `__Id` = unhex(md5(@Id)) LIMIT 1);                    

                    SELECT 
                         `_Id`
                        ,`Id` 
                        ,`CorrelationId`
                        ,`Version`
                        ,`Name` 
                        ,`Group` 
                        ,`Content` 
                        ,`Retries` 
                        ,`Created` 
                        ,`Expires` 
                        ,`Status` 
                    FROM `{options.Value.Database}`.`{settings.PublishedTable}`
                    WHERE `_Id` = @_Id;"

                , connection);

                command.Parameters.Add(new MySqlParameter
                {
                    ParameterName = "@Id",
                    DbType = DbType.String,
                    Value = id
                });

                var results = new List<InternalMessage>();
                var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);

                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    var model = settings.GetModel(settings.PublishedTable);
                    for (int i = 0; i < reader.FieldCount; i++)
                        model.SetValue<InternalMessage>(reader.GetName(i), reader.GetValue(i));

                    results.Add(model);
                }

                connection.Close();

                Assert.IsTrue (results.Count > 0);
                Assert.IsTrue(results.First().Status == MessageStatus.Succeeded.ToString());
            }
        }
    }
}