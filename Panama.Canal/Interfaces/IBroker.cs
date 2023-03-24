using Microsoft.Extensions.ObjectPool;
using Panama.Canal.Models;
using Panama.Interfaces;

namespace Panama.Canal.Interfaces
{
    public interface IBroker 
    {
        IPooledObjectPolicy<IModel> ConnectionPool { get; }
        Task<IResult> Publish(IContext context);
    }
}