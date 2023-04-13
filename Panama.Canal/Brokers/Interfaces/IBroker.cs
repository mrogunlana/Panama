using Microsoft.Extensions.ObjectPool;
using Panama.Interfaces;

namespace Panama.Canal.Brokers.Interfaces
{
    public interface IBroker
    {
        IBrokerOptions Options { get; }

        Type Target { get; }
        Task<IResult> Publish(IContext context);
    }
}