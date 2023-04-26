using Panama.Canal.Sagas.Stateless;
using Panama.Canal.Sagas.Stateless.Extensions;
using Panama.Canal.Sagas.Stateless.Interfaces;
using Panama.Canal.Sagas.Stateless.Models;
using Panama.Canal.Tests.Sagas.CreateFoo.Events;
using Panama.Canal.Tests.Sagas.CreateFoo.States;
using Panama.Canal.Tests.Sagas.CreateFoo.Triggers;
using Panama.Extensions;
using Panama.Interfaces;
using Stateless;

namespace Panama.Canal.Tests.Sagas.CreateFoo
{
    public class CreateFooSaga : StatelessSaga
    {
        private readonly ISagaStateFactory _states;
        private readonly ISagaTriggerFactory _triggers;

        public CreateFooSaga(IServiceProvider provider,
            ISagaStateFactory states,
            ISagaTriggerFactory triggers)
            : base(provider)
        {
            _states = states;
            _triggers = triggers;
        }

        public override void Init(IContext context)
        {
            States.Add(_states.Create<CreateFooRequested>());
            States.Add(_states.Create<CreateFooRequestAnswered>());
            States.Add(_states.Create<CreateFooCreated>());
            States.Add(_states.Create<CreateFooFailed>());
            States.Add(_states.Create<CreateFooRollbackRequested>());
            States.Add(_states.Create<CreateFooRollbackAnswered>());
            States.Add(_states.Create<CreateFooComplete>());

            Triggers.Add(_triggers.Create<CreateNewFoo>(StateMachine));
            Triggers.Add(_triggers.Create<ReviewCreateFooAnswer>(StateMachine));
            Triggers.Add(_triggers.Create<RollbackCreateFoo>(StateMachine));
            Triggers.Add(_triggers.Create<CompleteNewFoo>(StateMachine));
        }

        public override void Configure(IContext context)
        {
            base.Configure(context);

            StateMachine.Configure(States.Get<NotStarted>())
                .PermitDynamic(Triggers.Get<CreateNewFoo>(), (context) => {
                    return context.ExecuteEvent<CreateFooEvent>();
                });

            StateMachine.Configure(States.Get<CreateFooRequestAnswered>())
                .Permit(Triggers.GetUnderlyingTrigger<ReviewCreateFooAnswer>(), States.Get<CreateFooRollbackAnswered>())
                .OnEntryFrom(Triggers.Get<ReviewCreateFooAnswer>(), (context) => {
                    var result = context.ExecuteEntry<ReviewCreateFooAnswerEntry>();
                    var trigger = result.TriggerGetSingle("Trigger");
                    
                    StateMachine.Fire(trigger, context);
                })
                .PermitDynamic(Triggers.Get<CompleteNewFoo>(), (context) => {
                    return States.Get<CreateFooComplete>();
                })
                .PermitDynamic(Triggers.Get<RollbackCreateFoo>(), (context) => {
                    return context.ExecuteEvent<RollbackFooEvent>();
                });
                //.OnEntryFrom(Triggers.Get<ReviewCreateFooAnswer>(), email => OnAssigned(email));
                //.PermitIf(Trigger.CallDialled, State.Ringing, () => IsValidNumber)
                //.PermitIf(Trigger.CallDialled, State.Beeping, () => !IsValidNumber)
                //.PermitDynamic(Triggers.Get<ReviewCreateFooAnswer>(), (context) => {
                //    return context.ExecuteEvent<ReviewCreateFooAnswerEvent>();
                //})
                //.OnExit(() => {  });

            //StateMachine.Configure(States.Get<CreateFooCreated>())
            //    .PermitDynamic(Triggers.Get<CompleteNewFoo>(), (context) => {
            //        return States.Get<CreateFooComplete>();
            //    });

            //StateMachine.Configure(States.Get<CreateFooFailed>())
            //    .PermitDynamic(Triggers.Get<RollbackCreateFoo>(), (context) => {
            //        return context.ExecuteEvent<RollbackFooEvent>();
            //    });
        }

        public override async Task Start(IContext context)
        {
            try
            {
                await StateMachine.FireAsync(Triggers.Get<CreateNewFoo>(), context);
            }
            catch (Exception ex)
            {
                var e = ex;
                throw;
            }
        }
    }
}
