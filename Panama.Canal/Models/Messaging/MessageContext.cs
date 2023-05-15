using Panama.Models;

namespace Panama.Canal.Models.Messaging
{
    public class MessageContext : Context
    {
        public MessageContext(
              InternalMessage data
            , CancellationToken? token = null)
            : base(token)
        {
            Data.Add(data);

            Id = data.Id;
            CorrelationId = data.CorrelationId;
        }

        public MessageContext(
              InternalMessage data
            , IServiceProvider provider
            , CancellationToken? token = null)
            : base(provider, token)
        {
            Data.Add(data);

            Id = data.Id;
            CorrelationId = data.CorrelationId;
        }
    }
}
