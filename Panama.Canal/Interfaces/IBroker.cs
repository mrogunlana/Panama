using Panama.Canal.Models;
using Panama.Interfaces;

namespace Panama.Canal.Interfaces
{
    public interface IBroker 
    {
        BrokerOptions Options { get; }
        Task<IResult> Publish(IContext context);
    }
}