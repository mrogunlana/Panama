using Microsoft.Extensions.Logging;
using Panama.Core.Interfaces;
using Panama.Core.Models;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Panama.Core.Invokers
{
    public class InvokeActions : IInvokeAction
    {
        private readonly ILogger<InvokeActions> _logger;
        
        public InvokeActions(ILogger<InvokeActions> logger)
        {
            _logger = logger;
        }

        public async Task<IResult> Invoke<T>(IHandler handler) where T:IAction
        {
            var stopwatch = new Stopwatch();
            
            try
            {
                var manifest = handler.Manifest.OfType<T>();

                stopwatch.Start();

                _logger.LogTrace($"{nameof(T)} Processing (HID:{handler.Context.Id}) Start: [{manifest.Count()}] Actions Queued.");

                foreach (var action in manifest)
                {
                    if (handler.Context.Token.IsCancellationRequested)
                        handler.Context.Token.ThrowIfCancellationRequested();

                    await action
                        .Execute(handler.Context)
                        .ConfigureAwait(false);
                }
                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());

                var result = new Result() { Success = false, Data = handler.Context.Data };

                result.Cancelled = (ex is OperationCanceledException ||
                                    ex is TaskCanceledException ||
                                    handler.Context.Token.IsCancellationRequested);

                if (ex is ValidationException)
                    result.AddMessages(((ValidationException)ex).Messages);
                else if (ex is ServiceException)
                    result.AddMessages(((ServiceException)ex).Messages);
                else if (result.Cancelled)
                    result.AddMessage($"HID:{handler.Context.Id}, Looks like there was a cancellation request that caused your request to end prematurely.");
                else
                    result.AddMessage($"HID:{handler.Context.Id}, Looks like there was a problem with your request.");

                return result;
            }
            finally
            {
                stopwatch.Stop();

                _logger.LogTrace($"{nameof(T)} Processing (HID:{handler.Context.Id}) Complete: [{stopwatch.Elapsed.ToString(@"hh\:mm\:ss\:fff")}]");
            }

            return new Result() { Success = true, Data = handler.Context.Data };
        }
    }
}
