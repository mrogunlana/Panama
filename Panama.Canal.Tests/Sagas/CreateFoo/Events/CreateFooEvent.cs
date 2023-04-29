using Microsoft.Extensions.DependencyInjection;
using Panama.Canal.Channels;
using Panama.Canal.Extensions;
using Panama.Canal.Interfaces;
using Panama.Canal.Sagas.Stateless.Extensions;
using Panama.Canal.Sagas.Stateless.Interfaces;
using Panama.Canal.Tests.Models;
using Panama.Canal.Tests.Sagas.CreateFoo.States;
using Panama.Canal.Tests.Sagas.CreateFoo.Triggers;
using Panama.Extensions;
using Panama.Interfaces;
using Panama.Models;

namespace Panama.Canal.Tests.Sagas.CreateFoo.Events
{
    public class CreateFooEvent : ISagaStepEvent
    {
        public async Task<ISagaState> Execute(IContext context)
        {
            var model = context.DataGetSingle<Foo>();
            var channels = context.Provider.GetRequiredService<IDefaultChannelFactory>();

            using (var channel = channels.CreateChannel<DefaultChannel>())
            {
                await context.Bus()
                    .Data(model)
                    .Channel(channel)
                    .Token(context.Token)
                    .Topic("foo.created")
                    .Trigger<ReviewCreateFooAnswer>()
                    .State<CreateFooRequestAnswered>()
                .Post();
                await channel.Commit();
            }

            var state = context.Provider.GetService<State>();
            if (state == null)
                return context.GetState<CreateFooRequested>();
            
            state.Data.Add(new Kvp<string, string>("saga.event.name", nameof(CreateFooEvent)));

            return context.GetState<CreateFooRequested>();
        }
    }
}
