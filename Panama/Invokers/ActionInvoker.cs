using Microsoft.Extensions.Logging;
using Panama.Exceptions;
using Panama.Interfaces;
using Panama.Models;
using System.Diagnostics;

namespace Panama.Invokers
{
    public class ActionInvoker<T> : IInvoke<T> where T : IAction
    {
        private readonly ILogger<ActionInvoker<T>> _logger;
        
        public ActionInvoker(ILogger<ActionInvoker<T>> logger)
        {
            _logger = logger;
        }

        public async Task<IResult> Invoke(IContext context)
        {
            var stopwatch = new Stopwatch();
            
            try
            {
                var manifest = context.Data.OfType<T>().ToList();

                stopwatch.Start();

                _logger.LogTrace($"{nameof(T)} Processing (HID:{context.Id}) Start: [{manifest.Count()}] Actions Queued.");

                foreach (var action in manifest)
                {
                    if (context.Token.IsCancellationRequested)
                        context.Token.ThrowIfCancellationRequested();

                    await action
                        .Execute(context)
                        .ConfigureAwait(false);
                }
                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());

                var result = new Result() { Success = false, Data = context.Data };

                result.Cancelled = (ex is OperationCanceledException ||
                                    ex is TaskCanceledException ||
                                    context.Token.IsCancellationRequested);

                if (ex is ValidationException)
                    result.AddMessages(((ValidationException)ex).Messages);
                else if (ex is ServiceException)
                    result.AddMessages(((ServiceException)ex).Messages);
                else if (result.Cancelled)
                    result.AddMessage($"HID:{context.Id}, Looks like there was a cancellation request that caused your request to end prematurely.");
                else
                    result.AddMessage($"HID:{context.Id}, Looks like there was a problem with your request.");

                return result;
            }
            finally
            {
                stopwatch.Stop();

                _logger.LogTrace($"{nameof(T)} Processing (HID:{context.Id}) Complete: [{stopwatch.Elapsed.ToString(@"hh\:mm\:ss\:fff")}]");
            }

            return new Result() { Success = true, Data = context.Data };
        }
    }
}
