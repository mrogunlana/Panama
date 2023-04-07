using Panama.Canal.Sagas.Stateless;
using Panama.Canal.Sagas.Stateless.Extensions;
using Panama.Canal.Sagas.Stateless.Interfaces;
using Panama.Canal.Sagas.Stateless.Models;
using Panama.Interfaces;
using Panama.TestApi.Sagas.CreateWeatherForcast.Events;
using Panama.TestApi.Sagas.CreateWeatherForcast.States;
using Panama.TestApi.Sagas.CreateWeatherForcast.Triggers;

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
                .PermitDynamic(Triggers.Get<CreateNewWeatherForcast>(), (context) => {
                    return context.ExecuteEvent<CreateWeatherForcastEvent>();
                });

            StateMachine.Configure(States.Get<CreateWeatherForcastRequestAnswered>())
                .PermitDynamic(Triggers.Get<ReviewCreateWeatherForcastAnswer>(), (context) => {
                    return context.ExecuteEvent<ReviewCreateWeatherForcastAnswerEvent>();
                });

            StateMachine.Configure(States.Get<CreateWeatherForcastCreated>())
                .PermitDynamic(Triggers.Get<SendCreatedWeatherForcastNotification>(), (context) => {
                    return context.ExecuteEvent<SendCreatedWeatherForcastNotificationEvent>();
                });

            StateMachine.Configure(States.Get<CreateWeatherForcastComplete>())
                .PermitDynamic(Triggers.Get<PublishCreateWeatherForcastResults>(), (context) => {
                    return context.ExecuteEvent<SendCreatedWeatherForcastNotificationEvent>();
                });

            StateMachine.Configure(States.Get<CreateWeatherForcastFailed>())
                .PermitDynamic(Triggers.Get<RollbackCreateWeatherForcast>(), (context) => {
                    return context.ExecuteEvent<SendCreatedWeatherForcastNotificationEvent>();
                });

            StateMachine.Configure(States.Get<CreateWeatherForcastRollbackAnswered>())
                .PermitDynamic(Triggers.Get<ReviewCreateWeatherForcastRollbackAnswer>(), (context) => {
                    return context.ExecuteEvent<ReviewCreateWeatherForcastRollbackAnswerEvent>();
                });

            StateMachine.Configure(States.Get<CreateWeatherForcastFailedNotificationSent>())
                .PermitDynamic(Triggers.Get<PublishCreateWeatherForcastResults>(), (context) => {
                    return context.ExecuteEvent<PublishCreateWeatherForcastResultsEvent>();
                });
        }

        public override async Task Start(IContext context) => await StateMachine.FireAsync(Triggers.Get<CreateNewWeatherForcast>(), context);
    }
}
