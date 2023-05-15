using Microsoft.Extensions.DependencyInjection;
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
            var states = context.KvpGetSingle<string, List<ISagaState>>("States")?.Select(x => x as IModel)?.ToList();
            if (states == null)
                throw new InvalidOperationException($"States cannot be located");

            return states.DataGetSingle<S>();
        }

        public static StateMachine<ISagaState, ISagaTrigger>.TriggerWithParameters<IContext> GetTrigger<T>(this IContext context)
            where T : ISagaTrigger
        {
            var triggers = context.KvpGetSingle<string, List<StateMachine<ISagaState, ISagaTrigger>.TriggerWithParameters<IContext>>>("Triggers");
            if (triggers == null)
                throw new InvalidOperationException("No triggers can be located.");

            return triggers.Get<T>();
        }

        public static StateMachine<ISagaState, ISagaTrigger>.TriggerWithParameters<IContext>? TriggerGetSingle(this IResult result, string key)
        {
            var trigger = result.KvpGetSingle<string, StateMachine<ISagaState, ISagaTrigger>.TriggerWithParameters<IContext>>(key);
            if (trigger == null)
                return null;

            return trigger;
        }

        public static ISagaState ExecuteEvent<E>(this IContext context)
            where E : ISagaStepEvent
        {
            var @event = context.Provider.GetRequiredService<E>();

            return @event.Execute(context).GetAwaiter().GetResult();
        }

        public static IResult ExecuteEntry<E>(this IContext context)
            where E : ISagaStepEntry
        {
            var @event = context.Provider.GetRequiredService<E>();

            return @event.Execute(context).GetAwaiter().GetResult();
        }

        public static IResult ExecuteExit<E>(this IContext context)
            where E : ISagaStepExit
        {
            var @event = context.Provider.GetRequiredService<E>();

            return @event.Execute(context).GetAwaiter().GetResult();
        }

        public static StateMachine<ISagaState, ISagaTrigger> GetStateMachine(this IContext context)
        {
            return context.KvpGetSingle<string, StateMachine<ISagaState, ISagaTrigger>>("StateMachine");
        }

        public static ISagaState GetDestination(this IContext context)
        {
            return context.KvpGetSingle<string, ISagaState>("Destination");
        }
    }
}