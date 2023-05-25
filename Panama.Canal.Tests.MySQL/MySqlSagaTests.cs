using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Panama.Canal.Channels;
using Panama.Canal.Extensions;
using Panama.Canal.Interfaces;
using Panama.Canal.Sagas.Extensions;
using Panama.Canal.Sagas.Stateless.Extensions;
using Panama.Canal.Tests.Modules.Models;
using Panama.Canal.Tests.Modules.Sagas.CreateFoo;
using Panama.Canal.Tests.Modules.Sagas.CreateFoo.Events;
using Panama.Canal.Tests.Modules.Sagas.CreateFoo.Exits;
using Panama.Canal.Tests.Modules.Subscriptions;
using Panama.Extensions;
using Panama.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Panama.Canal.Tests.MySQL
{
    [TestClass]
    public class MySqlSagaTests 
    {
        [TestInitialize]
        public void Init() => Startup._provider!.GetRequiredService<State>().Reset();

        [TestMethod]
        public async Task VerifyFooSaga()
        {
            var channels = Startup._provider!.GetRequiredService<IDefaultChannelFactory>();
            var context = new Context(Startup._provider!);

            using (var channel = channels.CreateChannel<DefaultChannel>())
            {
                await context.Saga<CreateFooSaga>()
                    .Channel(channel)
                    .Data(new Foo())
                    .Start();

                await channel.Commit();

                await Task.Delay(TimeSpan.FromSeconds(2));

                var state = Startup._provider!.GetRequiredService<State>();
                var response = state.Data.ToList();

                Assert.IsTrue(response.KvpGet<string, string>("saga.event.name").Count == 3);
                Assert.IsTrue(response.KvpGet<string, string>("saga.exit.name").Count == 1);
                Assert.IsTrue(response.KvpGet<string, string>("subscription.name").Count == 1);

                var steps = new List<string>();

                steps.AddRange(response.KvpGet<string, string>("saga.event.name"));
                steps.AddRange(response.KvpGet<string, string>("saga.exit.name"));
                steps.AddRange(response.KvpGet<string, string>("subscription.name"));

                Assert.IsNotNull(steps.Contains(nameof(CreateFooEvent)));
                Assert.IsNotNull(steps.Contains(nameof(FooCreated)));
                Assert.IsNotNull(steps.Contains(nameof(ReviewCreateFooAnswerEvent)));
                Assert.IsNotNull(steps.Contains(nameof(ReviewCreateFooAnswerExit)));
                Assert.IsNotNull(steps.Contains(nameof(FooCompletedEvent)));
            }
        }
    }
}