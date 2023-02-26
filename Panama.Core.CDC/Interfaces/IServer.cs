using Microsoft.Extensions.Hosting;

namespace Panama.Core.CDC.Interfaces
{
    public interface IServer : IDisposable 
    {
        Task StartAsync(CancellationToken cancellationToken);
    }
}
