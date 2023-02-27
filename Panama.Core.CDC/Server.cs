using Panama.Core.CDC.Interfaces;
using Panama.Core.Interfaces;
using Panama.Core.Models;

namespace Panama.Core.CDC
{
    public class Server : IServer
    {
        private readonly ILog<Server> _log;
        private readonly ILocate _locator;
        private readonly ILogFactory _factory;

        private CancellationTokenSource _cts;
        private Context _context = default!;
        private Task? _task;
        private bool _disposed;

        public Server(
            ILog<Server> log,
            ILogFactory factory,
            ILocate locator)
        {
            _log = log;
            _locator = locator;
            _factory = factory;
            _cts = new CancellationTokenSource();
        }
        public void Dispose()
        {
            if (_disposed) return;

            try
            {
                _disposed = true;
                _log.LogInformation("Shutting down the processing server...");
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

            _context = new Context(_locator, _cts.Token);

            var processors = _locator.ResolveList<IProcess>();

            var tasks = _locator
                .ResolveList<IProcess>()
                .Select(p => new Manager(p, _factory))
                .Select(m => m.Invoke(_context));

            _task = Task.WhenAll(tasks);

            return Task.CompletedTask;
        }
    }
}