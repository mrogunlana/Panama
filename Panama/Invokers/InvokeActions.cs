using Panama.Core.Interfaces;
using Panama.Core.Models;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Panama.Core.Invokers
{
    public class InvokeActions<T> : IInvoke<T>
        where T : IAction
    {
        private ILog<InvokeActions<T>> _log;
        
        public InvokeActions(ILog<InvokeActions<T>> log)
        {
            _log = log;
        }

        public async Task<IResult> Invoke(IHandler handler)
        {
            var stopwatch = new Stopwatch();
            
            try
            {
                var manifest = handler.Manifest.OfType<T>();

                stopwatch.Start();

                _log.LogTrace($"{nameof(T)} Processing (HID:{handler.Id}) Start: [{manifest.Count()}] Actions Queued.");

                foreach (var action in manifest)
                {
                    if (handler.Token.IsCancellationRequested)
                        handler.Token.ThrowIfCancellationRequested();

                    await action
                        .Execute(handler)
                        .ConfigureAwait(false);
                }
                
            }
            catch (Exception ex)
            {
                _log.LogException(ex);

                var result = new Result() { Success = false, Data = handler.Data };

                result.Cancelled = (ex is OperationCanceledException ||
                                    ex is TaskCanceledException ||
                                    handler.Token.IsCancellationRequested);

                if (ex is ValidationException)
                    result.AddMessages(((ValidationException)ex).Messages);
                else if (ex is ServiceException)
                    result.AddMessages(((ServiceException)ex).Messages);
                else if (result.Cancelled)
                    result.AddMessage($"HID:{handler.Id}, Looks like there was a cancellation request that caused your request to end prematurely.");
                else
                    result.AddMessage($"HID:{handler.Id}, Looks like there was a problem with your request.");

                return result;
            }
            finally
            {
                stopwatch.Stop();

                _log.LogTrace($"{nameof(T)} Processing (HID:{handler.Id}) Complete: [{stopwatch.Elapsed.ToString(@"hh\:mm\:ss\:fff")}]");
            }

            return new Result() { Success = true, Data = handler.Data };
        }
    }
}
