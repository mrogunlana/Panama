using Microsoft.Extensions.Hosting;

namespace Panama.Canal.Interfaces
{
    public interface IBootstrap : IHostedService, IDisposable  
    {
        bool IsActive { get; }
        Task Invoke(CancellationToken cancellationToken);
    }
}
