using Panama.Canal.Channels;
using Panama.Canal.Extensions;
using Panama.Canal.Interfaces;
using Panama.Canal.Models;
using Panama.Canal.Sagas.Stateless;
using Panama.Canal.Sagas.Stateless.Extensions;
using Panama.Canal.Sagas.Stateless.Interfaces;
using Panama.Canal.Sagas.Stateless.Models;
using Panama.Extensions;
using Panama.Interfaces;
using Panama.TestApi.Sagas.CreateWeatherForcast.States;
using Panama.TestApi.Sagas.CreateWeatherForcast.Triggers;

namespace Panama.TestApi.Sagas.CreateWeatherForcast
{
    public class CreateWeatherForcastSaga : StatelessSaga
    {
        private readonly ISagaStateFactory _states;
        private readonly ISagaTriggerFactory _triggers;
        private readonly IDefaultChannelFactory _channels;

        public CreateWeatherForcastSaga(IServiceProvider provider,
            ISagaStateFactory states,
            ISagaTriggerFactory triggers,
            IDefaultChannelFactory channels)
            : base(provider)
        {
            _states = states;
            _channels = channels;
            _triggers = triggers;
        }

        public override void Configure(IContext context)
        {
            States.Add(_states.Create<CreateWeatherForcastRequested>());
            States.Add(_states.Create<CreateWeatherForcastRequestAnswered>());
            States.Add(_states.Create<CreateWeatherForcastCreated>());
            States.Add(_states.Create<CreateWeatherForcastSuccessNotificationSent>());
            States.Add(_states.Create<CreateWeatherForcastFailed>());
            States.Add(_states.Create<CreateWeatherForcastRollbackRequested>());
            States.Add(_states.Create<CreateWeatherForcastRollbackAnswered>());
            States.Add(_states.Create<CreateWeatherForcastFailedNotificationSent>());
            States.Add(_states.Create<CreateWeatherForcastComplete>());
            States.Add(_states.Create<CreateWeatherForcastPublishResultsComplete>());

            Triggers.Add(_triggers.Create<CreateNewWeatherForcast>(StateMachine));
            Triggers.Add(_triggers.Create<ReviewCreateWeatherForcastAnswer>(StateMachine));
            Triggers.Add(_triggers.Create<SendCreatedWeatherForcastNotification>(StateMachine));
            Triggers.Add(_triggers.Create<RollbackCreateWeatherForcast>(StateMachine));
            Triggers.Add(_triggers.Create<ReviewCreateWeatherForcastRollbackAnswer>(StateMachine));
            Triggers.Add(_triggers.Create<PublishCreateWeatherForcastResults>(StateMachine));

            StateMachine.Configure(States.Get<NotStarted>())
                .PermitDynamic(Triggers.Get<CreateNewWeatherForcast>(), (context) =>
                {

                    //TODO: post create weather forcast message on eventbus here..
                    var model = context.DataGetSingle<WeatherForecast>();

                    using (var channel = _channels.CreateChannel<DefaultChannel>())
                    {
                        context.Bus()
                            .Data(model)
                            .Channel(channel)
                            .Reply(ReplyTopic)
                            .Token(context.Token)
                            .Topic("weatherforcast.create")
                            .Trigger<ReviewCreateWeatherForcastAnswer>()
                            .Post().GetAwaiter().GetResult();

                        channel.Commit().GetAwaiter().GetResult();
                    }

                    return States.Get<CreateWeatherForcastRequested>();
                });

            StateMachine.Configure(States.Get<CreateWeatherForcastRequestAnswered>())
                .PermitDynamic(Triggers.Get<ReviewCreateWeatherForcastAnswer>(), (context) =>
                {

                    var message = context.DataGetSingle<Message>();

                    return message.HasException()
                        ? States.Get<CreateWeatherForcastFailed>()
                        : States.Get<CreateWeatherForcastCreated>();
                });

            StateMachine.Configure(States.Get<CreateWeatherForcastCreated>())
                .PermitDynamic(Triggers.Get<SendCreatedWeatherForcastNotification>(), (context) =>
                {

                    //TODO: send success notification here..

                    return States.Get<CreateWeatherForcastComplete>();
                });

            StateMachine.Configure(States.Get<CreateWeatherForcastComplete>())
                .PermitDynamic(Triggers.Get<PublishCreateWeatherForcastResults>(), (context) =>
                {

                    //TODO: publish weather forcast to read models here..

                    return States.Get<CreateWeatherForcastPublishResultsComplete>();
                });

            StateMachine.Configure(States.Get<CreateWeatherForcastFailed>())
                .PermitDynamic(Triggers.Get<RollbackCreateWeatherForcast>(), (context) =>
                {

                    //TODO: rollback weather forcast here..

                    return States.Get<CreateWeatherForcastRollbackRequested>();
                });

            StateMachine.Configure(States.Get<CreateWeatherForcastRollbackAnswered>())
                .PermitDynamic(Triggers.Get<ReviewCreateWeatherForcastRollbackAnswer>(), (context) =>
                {

                    //TODO: send review rollback response here..
                    //but this is a repeatable trasaction so the response doesn't matter in this case
                    //so we just send the failed notification instead

                    return States.Get<CreateWeatherForcastFailedNotificationSent>();
                });

            StateMachine.Configure(States.Get<CreateWeatherForcastFailedNotificationSent>())
                .PermitDynamic(Triggers.Get<PublishCreateWeatherForcastResults>(), (context) =>
                {

                    //TODO: publish weather forcast to read models here..

                    return States.Get<CreateWeatherForcastPublishResultsComplete>();
                });
        }

        public override async Task Start(IContext context)
        {
            Configure(context);

            await StateMachine.FireAsync(Triggers.Get<CreateNewWeatherForcast>(), context);
        }
    }
}
