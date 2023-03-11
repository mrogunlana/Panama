using Panama.Core.CDC.Models;
using Panama.Core.Interfaces;

namespace Panama.Core.CDC.Interfaces
{
    public interface IBroker 
    {
        BrokerOptions Options { get; }
        Task<IResult> Publish(IContext context);
    }
}