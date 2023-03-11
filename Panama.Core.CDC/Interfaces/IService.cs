using Microsoft.Extensions.Hosting;

namespace Panama.Core.CDC.Interfaces
{
    public interface IService : IDisposable 
    {
        Task Start(CancellationToken cancellationToken);
    }
}
