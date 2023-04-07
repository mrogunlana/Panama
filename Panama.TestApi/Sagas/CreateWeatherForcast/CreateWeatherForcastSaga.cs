﻿using Panama.Canal.Channels;
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
using Panama.Canal.Sagas.Extensions;

namespace Panama.TestApi.Sagas.CreateWeatherForcast
{
    public class CreateWeatherForcastSaga : StatelessSaga
    {
        private readonly ISagaStateFactory _states;
        private readonly ISagaTriggerFactory _triggers;

        public CreateWeatherForcastSaga(IServiceProvider provider,
            ISagaStateFactory states,
            ISagaTriggerFactory triggers)
            : base(provider)
        {
            _states = states;
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
                    var channels = context.Provider.GetRequiredService<IDefaultChannelFactory>();

                    using (var channel = channels.CreateChannel<DefaultChannel>())
                    {
                        context.Bus()
                            .Data(model)
                            .Channel(channel)
                            .Reply(context.GetReplyTopic())
                            .Token(context.Token)
                            .Topic("weatherforcast.create")
                            .Trigger<ReviewCreateWeatherForcastAnswer>()
                            .Post().GetAwaiter().GetResult();

                        channel.Commit().GetAwaiter().GetResult();
                    }

                    return context.GetState<CreateWeatherForcastRequested>();
                });

            StateMachine.Configure(States.Get<CreateWeatherForcastRequestAnswered>())
                .PermitDynamic(Triggers.Get<ReviewCreateWeatherForcastAnswer>(), (context) =>
                {
                    var message = context.DataGetSingle<Message>();

                    return message.HasException()
                        ? context.GetState<CreateWeatherForcastFailed>()
                        : context.GetState<CreateWeatherForcastCreated>();
                });

            StateMachine.Configure(States.Get<CreateWeatherForcastCreated>())
                .PermitDynamic(Triggers.Get<SendCreatedWeatherForcastNotification>(), (context) =>
                {

                    //TODO: send success notification here..

                    return context.GetState<CreateWeatherForcastComplete>();
                });

            StateMachine.Configure(States.Get<CreateWeatherForcastComplete>())
                .PermitDynamic(Triggers.Get<PublishCreateWeatherForcastResults>(), (context) =>
                {

                    //TODO: publish weather forcast to read models here..

                    return context.GetState<CreateWeatherForcastPublishResultsComplete>();
                });

            StateMachine.Configure(States.Get<CreateWeatherForcastFailed>())
                .PermitDynamic(Triggers.Get<RollbackCreateWeatherForcast>(), (context) =>
                {

                    //TODO: rollback weather forcast here..

                    return context.GetState<CreateWeatherForcastRollbackRequested>();
                });

            StateMachine.Configure(States.Get<CreateWeatherForcastRollbackAnswered>())
                .PermitDynamic(Triggers.Get<ReviewCreateWeatherForcastRollbackAnswer>(), (context) =>
                {

                    //TODO: send review rollback response here..
                    //but this is a repeatable trasaction so the response doesn't matter in this case
                    //so we just send the failed notification instead

                    return context.GetState<CreateWeatherForcastFailedNotificationSent>();
                });

            StateMachine.Configure(States.Get<CreateWeatherForcastFailedNotificationSent>())
                .PermitDynamic(Triggers.Get<PublishCreateWeatherForcastResults>(), (context) =>
                {

                    //TODO: publish weather forcast to read models here..

                    return context.GetState<CreateWeatherForcastPublishResultsComplete>();
                });
        }

        public override async Task Start(IContext context) => await StateMachine.FireAsync(Triggers.Get<CreateNewWeatherForcast>(), context);
    }
}
