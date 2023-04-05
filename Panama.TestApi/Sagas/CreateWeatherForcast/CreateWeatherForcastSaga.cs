using Panama.Canal.Interfaces;
using Panama.Canal.Interfaces.Sagas;
using Panama.Canal.Models;
using Panama.Canal.Models.Sagas;
using Panama.Canal.Sagas;
using Panama.Interfaces;
using Panama.TestApi.Sagas.CreateWeatherForcast.States;
using Stateless;

namespace Panama.TestApi.Sagas
{
    public class CreateWeatherForcastSaga : Saga
    {
        private readonly IServiceProvider _provider;
        private readonly ISagaStateFactory _states;
        private readonly ISagaTriggerFactory _triggers;

        public CreateWeatherForcastSaga(IServiceProvider provider,
            ISagaStateFactory states,
            ISagaTriggerFactory triggers)
            : base(provider)
        {
            _provider = provider;
            _states = states;
            _triggers = triggers;
        }

        public override Task Configure(IContext context)
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

            Triggers.Add(_triggers.Get<CreateNewWeatherForcast>());
            Triggers.Add(_triggers.Get<ReviewCreateWeatherForcastAnswer>());
            Triggers.Add(_triggers.Get<SendCreatedWeatherForcastNotification>());
            Triggers.Add(_triggers.Get<RollbackCreateWeatherForcast>());
            Triggers.Add(_triggers.Get<ReviewCreateWeatherForcastRollbackAnswer>());
            Triggers.Add(_triggers.Get<SendFailedWeatherForcastNotification>());
            Triggers.Add(_triggers.Get<PublishCreateWeatherForcastResults>());

            StateMachine.Configure(_states.Get<NotStarted>());
        }

        public override Task<Interfaces.IResult> Start()
        {
            throw new NotImplementedException();
        }
    }
}
