using Panama.Canal.Interfaces;
using Panama.Canal.Models;
using Panama.Canal.Sagas.Stateless.Interfaces;
using Panama.Interfaces;
using Panama.Models;
using Stateless;

namespace Panama.Canal.Sagas.Stateless.Extensions
{
    public static class TriggerExtensions
    {
        public static StateMachine<ISagaState, ISagaTrigger>.TriggerWithParameters<IContext> Get<T>(this List<StateMachine<ISagaState, ISagaTrigger>.TriggerWithParameters<IContext>> triggers)
            where T : ISagaTrigger
        {
            var result = triggers.Get(typeof(T));
            if (result == null)
                throw new InvalidOperationException($"Trigger of type: {typeof(T)} cannot be located.");

            return result;
        }

        public static StateMachine<ISagaState, ISagaTrigger>.TriggerWithParameters<IContext>? Get(this List<StateMachine<ISagaState, ISagaTrigger>.TriggerWithParameters<IContext>> triggers, Type? type)
        {
            if (type == null)
                return null;
            if (triggers == null)
                throw new ArgumentNullException(nameof(triggers));

            var trigger = triggers.Where(x => x.Trigger.GetType() == type).FirstOrDefault();

            return trigger;
        }

        public static ISagaTrigger GetUnderlyingTrigger<T>(this List<StateMachine<ISagaState, ISagaTrigger>.TriggerWithParameters<IContext>> triggers)
            where T : ISagaTrigger
        {
            var trigger = triggers.Get(typeof(T));
            if (trigger == null)
                throw new InvalidOperationException($"Trigger of type: {typeof(T)} cannot be located.");

            return trigger.Trigger;
        }
    }
}
