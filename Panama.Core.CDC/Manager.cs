using Panama.Core.CDC.Interfaces;
using Panama.Core.Interfaces;

namespace Panama.Core.CDC
{
    public class Manager : IManage
    {
        private readonly IProcess _process;
        private readonly ILog<Manager> _log;

        public Manager(
              IProcess process
            , ILogFactory factory)
        {
            _process = process;
            _log = factory.CreateLogger<Manager>();
        }
        public async Task Invoke(IContext context)
        {
            while (!context.Token.IsCancellationRequested)
            {
                try
                {
                    await _process.Invoke(context).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    //ignore
                }
                catch (Exception)
                {
                    _log.LogWarning($"Processor '{nameof(_process)}' failed. Retrying...");

                    await Task.Delay(TimeSpan.FromSeconds(2), context.Token).ConfigureAwait(false);
                }
            }
        }
    }
}
