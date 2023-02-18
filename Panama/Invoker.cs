using Microsoft.Extensions.Logging;
using Panama.Core.Interfaces;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Panama.Core
{
    public class Invoker<T> : IInvoke<T> where T : IExecute
    {
        private ILogger _log;
        
        public Invoker(ILogger<Invoker<T>> log)
        {
            _log = log;
        }

        public async Task<IResult> Invoke(IHandler handler)
        {
            var stopwatch = new Stopwatch();
            
            try
            {
                var manifest = handler.Manifest.OfType<T>();
                var context = new Context(handler.Data, handler.Token);

                stopwatch.Start();

                _log.LogTrace($"Handler (HID:{handler.Id}) Start: [{manifest.Count()}] Actions Queued.");

                foreach (var action in manifest)
                    await action.Execute(context)
                        .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, null);

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

                _log.LogTrace($"Handler (HID:{handler.Id}) Complete: [{stopwatch.Elapsed.ToString(@"hh\:mm\:ss\:fff")}]");
            }

            return new Result() { Success = true, Data = handler.Data };
        }
    }
}
