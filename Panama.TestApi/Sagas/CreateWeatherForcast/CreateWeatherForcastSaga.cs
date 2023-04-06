using Panama.Canal.Interfaces.Sagas;
using Panama.Canal.Models.Sagas;
using Panama.Canal.Sagas;
using Panama.Interfaces;
using Panama.TestApi.Sagas.CreateWeatherForcast.States;
using Panama.Canal.Sagas.Extensions;

namespace Panama.TestApi.Sagas
{
    public class CreateWeatherForcastSaga : Saga
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
                .PermitDynamic(Triggers.Get<CreateNewWeatherForcast>(), (IContext context) => {

                    //TODO: post create weather forcast message on eventbus here..

                    return States.Get<CreateWeatherForcastRequested>();
                });

            StateMachine.Configure(States.Get<CreateWeatherForcastRequestAnswered>())
                .PermitDynamic(Triggers.Get<ReviewCreateWeatherForcastAnswer>(), (IContext context) => {

                    //TODO: determine based on the result on the message if it was successfull or not..

                    var result = true;

                    return result
                        ? States.Get<CreateWeatherForcastCreated>()
                        : States.Get<CreateWeatherForcastFailed>();
                });

            StateMachine.Configure(States.Get<CreateWeatherForcastCreated>())
                .PermitDynamic(Triggers.Get<SendCreatedWeatherForcastNotification>(), (IContext context) => {

                    //TODO: send success notification here..

                    return States.Get<CreateWeatherForcastComplete>();
                });

            StateMachine.Configure(States.Get<CreateWeatherForcastComplete>())
                .PermitDynamic(Triggers.Get<PublishCreateWeatherForcastResults>(), (IContext context) => {

                    //TODO: publish weather forcast to read models here..

                    return States.Get<CreateWeatherForcastPublishResultsComplete>();
                });

            StateMachine.Configure(States.Get<CreateWeatherForcastFailed>())
                .PermitDynamic(Triggers.Get<RollbackCreateWeatherForcast>(), (IContext context) => {

                    //TODO: rollback weather forcast here..

                    return States.Get<CreateWeatherForcastRollbackRequested>();
                });

            StateMachine.Configure(States.Get<CreateWeatherForcastRollbackAnswered>())
                .PermitDynamic(Triggers.Get<ReviewCreateWeatherForcastRollbackAnswer>(), (IContext context) => {

                    //TODO: send review rollback response here..
                    //but this is a repeatable trasaction so the response doesn't matter in this case
                    //so we just send the failed notification instead

                    return States.Get<CreateWeatherForcastFailedNotificationSent>();
                });

            StateMachine.Configure(States.Get<CreateWeatherForcastFailedNotificationSent>())
                .PermitDynamic(Triggers.Get<PublishCreateWeatherForcastResults>(), (IContext context) => {

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
