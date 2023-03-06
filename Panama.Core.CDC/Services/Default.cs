using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Panama.Core.CDC.Interfaces;
using Panama.Core.CDC.Processors;
using Panama.Core.Models;

namespace Panama.Core.CDC.Services
{
    public class _Default : IService
    {
        private readonly ILogger<_Default> _log;
        private readonly IServiceProvider _provider;
        private readonly ILoggerFactory _factory;

        private CancellationTokenSource _cts;
        private Context _context = default!;
        private Task? _task;
        private bool _disposed;

        public _Default(
            ILogger<_Default> log,
            ILoggerFactory factory,
            IServiceProvider provider)
        {
            _log = log;
            _provider = provider;
            _factory = factory;
            _cts = new CancellationTokenSource();
        }
        public void Dispose()
        {
            if (_disposed) return;

            try
            {
                _disposed = true;
                _log.LogInformation("Shutting down the Panama.Core.CDC processing server...");
                _cts.Cancel();

                _task?.Wait((int)TimeSpan.FromSeconds(10).TotalMilliseconds);
            }
            catch (AggregateException ex)
            {
                var inner = ex.InnerExceptions[0];
                if (!(inner is OperationCanceledException))
                    _log.LogWarning($"Expected an OperationCanceledException, but found '{inner.Message}'.");
            }
            catch (Exception)
            {
                _log.LogWarning("An exception was occurred when disposing.");
            }
            finally
            {
                _log.LogInformation("### Panama.Core.CDC Default Server shutdown!");
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            cancellationToken.Register(() => _cts.Cancel());

            _log.LogDebug("### Panama.Core.CDC Default Server is starting.");

            _context = new Context(_provider, _cts.Token);

            var tasks = new List<IProcess> {
                    _provider.GetRequiredService<DeleteExpiredProcess>(),
                    _provider.GetRequiredService<PublishedRetryProcess>(),
                    _provider.GetRequiredService<ReceivedRetryProcess>()
                }
                .Select(p => new Manager(p, _factory))
                .Select(m => m.Invoke(_context));

            _task = Task.WhenAll(tasks);

            return Task.CompletedTask;
        }
    }
}