using Panama.Canal.Sagas.Stateless.Interfaces;
using Panama.Extensions;
using Panama.Interfaces;
using Stateless;

namespace Panama.Canal.Sagas.Stateless.Extensions
{
    public static class TransitionExtensions
    {
        public static IContext Context(this StateMachine<ISagaState, ISagaTrigger>.Transition transition, StateMachine<ISagaState, ISagaTrigger> machine)
        {
            if (transition.Parameters == null)
                throw new ArgumentNullException(nameof(transition.Parameters));

            var parameter = transition.Parameters.FirstOrDefault();
            if (parameter  == null)
                throw new InvalidOperationException("Context parameter cannot be located.");

            if (parameter is not IContext)
                throw new InvalidOperationException("Transition parameter must contain context.");

            var context = (IContext)parameter;
            
            context.AddKvp("Destination", transition.Destination);
            context.AddKvp("StateMachine", machine);

            return context;
        }
    }
}