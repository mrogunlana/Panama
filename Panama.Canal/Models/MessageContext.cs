using Panama.Models;

namespace Panama.Canal.Models
{
    public class MessageContext : Context
    {
        public MessageContext(
              InternalMessage data
            , IServiceProvider? provider = null
            , CancellationToken? token = null)
            : base(provider, token)
        {
            Data.Add(data);

            Id = data.Id;
            CorrelationId = data.CorrelationId;
        }
    }
}
