using Panama.Canal.Interfaces.Sagas;
using Panama.Canal.Models.Sagas;
using Panama.Canal.Sagas;
using Panama.Interfaces;
using Panama.TestApi.Sagas.CreateWeatherForcast.States;

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
            States.Add(_states.Get<CreateWeatherForcastRequested>());
            States.Add(_states.Get<CreateWeatherForcastRequestAnswered>());
            States.Add(_states.Get<CreateWeatherForcastCreated>());
            States.Add(_states.Get<CreateWeatherForcastSuccessNotificationSent>());
            States.Add(_states.Get<CreateWeatherForcastFailed>());
            States.Add(_states.Get<CreateWeatherForcastRollbackRequested>());
            States.Add(_states.Get<CreateWeatherForcastRollbackAnswered>());
            States.Add(_states.Get<CreateWeatherForcastFailedNotificationSent>());
            States.Add(_states.Get<CreateWeatherForcastComplete>());
            States.Add(_states.Get<CreateWeatherForcastPublishResultsComplete>());

            Triggers.Add(_triggers.Get<CreateNewWeatherForcast>());
            Triggers.Add(_triggers.Get<ReviewCreateWeatherForcastAnswer>());
            Triggers.Add(_triggers.Get<SendCreatedWeatherForcastNotification>());
            Triggers.Add(_triggers.Get<RollbackCreateWeatherForcast>());
            Triggers.Add(_triggers.Get<ReviewCreateWeatherForcastRollbackAnswer>());
            Triggers.Add(_triggers.Get<PublishCreateWeatherForcastResults>());

            StateMachine.Configure(_states.Get<NotStarted>())
                .PermitDynamic(_triggers.Get<CreateNewWeatherForcast>(), () => {

                    //TODO: post create weather forcast message on eventbus here..

                    return _states.Get<CreateWeatherForcastRequested>();
                });

            StateMachine.Configure(_states.Get<CreateWeatherForcastRequestAnswered>())
                .PermitDynamic(_triggers.Get<ReviewCreateWeatherForcastAnswer>(), () => {

                    //TODO: determine based on the result on the message if it was successfull or not..

                    var result = true;

                    return result
                        ? _states.Get<CreateWeatherForcastCreated>()
                        : _states.Get<CreateWeatherForcastFailed>();
                });

            StateMachine.Configure(_states.Get<CreateWeatherForcastCreated>())
                .PermitDynamic(_triggers.Get<SendCreatedWeatherForcastNotification>(), () => {

                    //TODO: send success notification here..

                    return _states.Get<CreateWeatherForcastComplete>();
                });

            StateMachine.Configure(_states.Get<CreateWeatherForcastComplete>())
                .PermitDynamic(_triggers.Get<PublishCreateWeatherForcastResults>(), () => {

                    //TODO: publish weather forcast to read models here..

                    return _states.Get<CreateWeatherForcastPublishResultsComplete>();
                });

            StateMachine.Configure(_states.Get<CreateWeatherForcastFailed>())
                .PermitDynamic(_triggers.Get<RollbackCreateWeatherForcast>(), () => {

                    //TODO: rollback weather forcast here..

                    return _states.Get<CreateWeatherForcastRollbackRequested>();
                });

            StateMachine.Configure(_states.Get<CreateWeatherForcastRollbackAnswered>())
                .PermitDynamic(_triggers.Get<ReviewCreateWeatherForcastRollbackAnswer>(), () => {

                    //TODO: send review rollback response here..
                    //but this is a repeatable trasaction so the response doesn't matter in this case
                    //so we just send the failed notification instead

                    return _states.Get<CreateWeatherForcastFailedNotificationSent>();
                });

            StateMachine.Configure(_states.Get<CreateWeatherForcastFailedNotificationSent>())
                .PermitDynamic(_triggers.Get<PublishCreateWeatherForcastResults>(), () => {

                    //TODO: publish weather forcast to read models here..

                    return _states.Get<CreateWeatherForcastPublishResultsComplete>();
                });
        }

        public override async Task Start() => await StateMachine.FireAsync(_triggers.Get<CreateNewWeatherForcast>());
    }
}
