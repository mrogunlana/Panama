using Microsoft.Extensions.Hosting;
using Quartz;

namespace Panama.Canal.Interfaces
{
    public interface IBootstrap : IHostedService
    {
        bool Active { get; }
        IScheduler Scheduler { get; }
        Task On(CancellationToken cancellationToken);
        Task Off();
    }
}
