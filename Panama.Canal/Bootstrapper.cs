using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Panama.Canal.Extensions;
using Panama.Canal.Interfaces;
using Panama.Canal.Models;
using Quartz;
using Quartz.Spi;

namespace Panama.Canal
{
    public class Bootstrapper : IHostedService, IBootstrap
    {
        private bool _off;
        private CancellationTokenSource? _cts;
        private IScheduler _scheduler = default!;
        private IEnumerable<IInitialize> _initializers = default!;

        private readonly ILogger<Bootstrapper> _log;
        private readonly IServiceProvider _provider;
        private readonly IJobFactory _jobFactory;
        private readonly IEnumerable<Job> _schedules;
        private readonly ISchedulerFactory _schedulerFactory;
        
        public bool Online => !_cts?.IsCancellationRequested ?? false;
        public IScheduler Scheduler => _scheduler;

        public Bootstrapper(ISchedulerFactory schedulerFactory
            , IJobFactory jobFactory
            , ILogger<Bootstrapper> log
            , IServiceProvider provider
            , IEnumerable<Job> schedules)
        {
            _log = log;
            _provider = provider;
            _schedules = schedules;
            _jobFactory = jobFactory;
            _schedulerFactory = schedulerFactory;
        }

        private async Task Initialize()
        {
            foreach (var initialize in _initializers)
            {
                try
                {
                    _cts!.Token.ThrowIfCancellationRequested();

                    await initialize.Invoke(_cts!.Token);
                }
                catch (Exception ex)
                {
                    if (ex is InvalidOperationException) throw;

                    _log.LogError(ex, "Initializing the processors!");
                }
            }
        }

        public async Task On(CancellationToken cancellationToken)
        {
            if (_cts != null)
            {
                _log.LogInformation("### Panama Canal background task is already started!");

                return;
            }

            _log.LogDebug("### Panama Canal Server is starting.");

            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _initializers = _provider.GetServices<IInitialize>();
            
            await Initialize().ConfigureAwait(false);

            _scheduler = await _schedulerFactory
                .GetScheduler(cancellationToken)
                .ConfigureAwait(false);

            _scheduler.JobFactory = _jobFactory;

            foreach (var schedule in _schedules)
            {
                var job = schedule.CreateJob();
                var trigger = schedule.CreateTrigger();

                await _scheduler
                    .ScheduleJob(job, trigger, cancellationToken)
                    .ConfigureAwait(false);
            }

            await _scheduler
                .Start(cancellationToken)
                .ConfigureAwait(false);

            _off = false;
            _log.LogInformation("### Panama Canal Server started!");
        }

        public async Task Off()
        {
            if (_off) 
                return;
            if (_scheduler == null)
                return;

            _cts?.Cancel();

            await _scheduler
                    .Shutdown(_cts?.Token ?? CancellationToken.None)
                    .ConfigureAwait(false);

            _cts?.Dispose();
            _cts = null;
            _off = true;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await On(cancellationToken).ConfigureAwait(false);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_scheduler == null)
                return;

            _cts?.Cancel();

            await _scheduler
                .Shutdown(cancellationToken)
                .ConfigureAwait(false);

            _cts = null;
        }
    }
}
