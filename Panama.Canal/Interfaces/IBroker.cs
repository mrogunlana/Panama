using Microsoft.Extensions.ObjectPool;
using Panama.Interfaces;

namespace Panama.Canal.Interfaces
{
    public interface IBroker 
    {
        bool Default { get; set; }
        Type Target { get; }
        IPooledObjectPolicy<IModel> ConnectionPool { get; }
        Task<IResult> Publish(IContext context);
    }
}