using Microsoft.Extensions.Hosting;
using Quartz;

namespace Panama.Canal.Interfaces
{
    public interface IBootstrap : IHostedService
    {
        bool Online { get; }
        IScheduler Scheduler { get; }
        Task On(CancellationToken cancellationToken);
        Task Off();
    }
}
