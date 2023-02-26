using Microsoft.Extensions.Hosting;

namespace Panama.Core.CDC.Interfaces
{
    public interface IBootstrap : IHostedService, IDisposable  
    {
        Task Invoke(CancellationToken cancellationToken);
    }
}
