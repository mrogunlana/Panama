using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Panama.Core.CDC.Interfaces;
using Panama.Core.Interfaces;

namespace Panama.Core.CDC
{
    public class Bootstrapper : BackgroundService, IBootstrap
    {
        private readonly IServiceProvider _provider;
        private readonly ILogger<Bootstrapper> _log;

        private CancellationTokenSource? _cts;
        private IEnumerable<IServer> _servers = default!;
        private bool _disposed;

        public bool IsActive => !_cts?.IsCancellationRequested ?? false;

        public Bootstrapper(
              IServiceProvider provider
            , ILogger<Bootstrapper> log)
        {
            _provider = provider;
            _log = log;
        }

        private async Task StartProcessors()
        {
            foreach (var server in _servers)
            {
                try
                {
                    _cts!.Token.ThrowIfCancellationRequested();

                    await server.StartAsync(_cts!.Token);
                }
                catch (OperationCanceledException)
                {
                    // ignore
                }
                catch (Exception ex)
                {
                    _log.LogError(ex, $"Starting {nameof(server)} throws an exception: {ex.Message}");
                }
            }
        }

        public override void Dispose()
        {
            if (_disposed) return;

            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
            _disposed = true;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Invoke(stoppingToken).ConfigureAwait(false);
        }

        public  override async Task StopAsync(CancellationToken cancellationToken)
        {
            _cts?.Cancel();

            await base.StopAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task Invoke(CancellationToken cancellationToken)
        {
            if (_cts != null)
            {
                _log.LogInformation("### Panama.Core.CDC background task is already started!");

                return;
            }

            _log.LogDebug("### Panama.Core.CDC Server is starting.");

            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            _servers = _provider.GetServices<IServer>();

            _cts.Token.Register(() =>
            {
                _log.LogDebug("### Panama.Core.CDC Server is stopping.");

                foreach (var server in _servers)
                {
                    try
                    {
                        server.Dispose();
                    }
                    catch (OperationCanceledException ex)
                    {
                        _log.LogWarning($"Expected an OperationCanceledException, but found '{ex.Message}'.");
                    }
                }
            });

            await StartProcessors().ConfigureAwait(false);

            _disposed = false;
            _log.LogInformation("### Panama.Core.CDC Server started!");
        }
    }
}
