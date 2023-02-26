using Panama.Core.Interfaces;
using Panama.Core.Models;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Panama.Core.Invokers
{
    public class InvokeHandler : IInvoke<IHandler> 
    {
        private ILog<InvokeHandler> _log;
        private readonly ILocate _serviceLocator;

        public InvokeHandler(ILocate serviceLocator)
        {
            _serviceLocator = serviceLocator;
            _log = _serviceLocator.Resolve<ILog<InvokeHandler>>(); ;
        }

        public async Task<IResult> Invoke(IHandler handler)
        {
            var stopwatch = new Stopwatch();
            
            try
            {
                stopwatch.Start();

                _log.LogTrace($"Handler (HID:{handler.Id}) Start: [{handler.Manifest.Count()}] Total Actions Queued.");

                var validators = _serviceLocator.Resolve<InvokeActions<IValidate>>();
                var queries = _serviceLocator.Resolve<InvokeActions<IQuery>>();
                var commands = _serviceLocator.Resolve<InvokeActions<ICommand>>();
                var rollbacks = _serviceLocator.Resolve<InvokeActions<IRollback>>();

                var valid = await validators.Invoke(handler);
                if (!valid.Success)
                    return valid;

                var queried = await queries.Invoke(handler);
                if (!queried.Success)
                    return queried;

                var performed = await commands.Invoke(handler);
                if (performed.Success)
                    return performed;

                var compensated = await rollbacks.Invoke(handler);
                return compensated;
            }
            catch (Exception ex)
            {
                _log.LogException(ex);

                var result = new Result() { Success = false, Data = handler.Data };

                result.Cancelled = (ex is OperationCanceledException ||
                                    ex is TaskCanceledException ||
                                    handler.Token.IsCancellationRequested);

                if (result.Cancelled)
                    result.AddMessage($"HID:{handler.Id}, Looks like there was a cancellation request that caused your request to end prematurely.");
                else
                    result.AddMessage($"HID:{handler.Id}, Looks like there was a problem with your request.");

                return result;
            }
            finally
            {
                stopwatch.Stop();

                _log.LogTrace($"Handler (HID:{handler.Id}) Complete: [{stopwatch.Elapsed.ToString(@"hh\:mm\:ss\:fff")}]");
            }
        }
    }
}
