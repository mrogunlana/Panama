using Panama.Canal.Sagas.Stateless.Interfaces;
using Panama.Extensions;
using Panama.Interfaces;
using Stateless;

namespace Panama.Canal.Sagas.Stateless.Extensions
{
    public static class ContextExtensions
    {
        public static string GetReplyTopic(this IContext context)
        {
            return context.KvpGetSingle<string, string>("ReplyTopic");
        }

        public static S GetState<S>(this IContext context)
            where S : ISagaState
        {
            return context.DataGetSingle<S>();
        }

        public static StateMachine<ISagaState, ISagaTrigger>.TriggerWithParameters<IContext> GetTrigger<T>(this IContext context)
            where T : ISagaTrigger
        {
            var triggers = context.KvpGetSingle<string, List<StateMachine<ISagaState, ISagaTrigger>.TriggerWithParameters<IContext>>>("Triggers");
            if (triggers == null)
                throw new InvalidOperationException("No triggers can be located.");

            return triggers.Get<T>();
        }
    }
}