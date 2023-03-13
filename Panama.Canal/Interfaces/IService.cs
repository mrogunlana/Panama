using Microsoft.Extensions.Hosting;

namespace Panama.Canal.Interfaces
{
    public interface IService : IDisposable 
    {
        Task Start(CancellationToken cancellationToken);
    }
}
