using Microsoft.Extensions.Hosting;

namespace Panama.Core.CDC.Interfaces
{
    public interface IBootstrap : IHostedService, IDisposable  
    {
        bool IsActive { get; }
        Task Invoke(CancellationToken cancellationToken);
    }
}
