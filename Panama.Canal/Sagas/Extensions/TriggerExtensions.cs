using Panama.Canal.Interfaces;
using Panama.Canal.Interfaces.Sagas;
using Panama.Canal.Models;
using Panama.Interfaces;
using Stateless;

namespace Panama.Canal.Sagas.Extensions
{
    public static class TriggerExtensions
    {
        public static StateMachine<ISagaState, ISagaTrigger>.TriggerWithParameters<IContext> Get<T>(this List<StateMachine<ISagaState, ISagaTrigger>.TriggerWithParameters<IContext>> triggers)
            where T : ISagaTrigger
        {
            if (triggers == null)
                throw new ArgumentNullException(nameof(triggers));

            var result = triggers.Where(x => x.Trigger is T).FirstOrDefault();
            if (result == null)
                throw new InvalidOperationException($"Trigger of type: {typeof(T).Name} could not be located.");

            return result;
        }
    }
}
