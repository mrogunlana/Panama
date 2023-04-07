using Panama.Canal.Sagas.Stateless.Interfaces;
using Panama.Extensions;
using Panama.Interfaces;

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
    }
}