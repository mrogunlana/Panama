using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Panama.Core.Interfaces;
using Panama.Core.Models;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Panama.Core.Invokers
{
    public class InvokeHandler : IInvokeResult<IHandler> 
    {
        private readonly ILogger<InvokeHandler> _logger;
        private IInvokeAction invoker { get; set; }

        public InvokeHandler(ILogger<InvokeHandler> logger, IInvokeAction _invoker)
        {
            invoker = _invoker;
            _logger = logger;
        }

        public async Task<IResult> Invoke(IHandler handler)
        {
            var stopwatch = new Stopwatch();
         
            try
            {
                stopwatch.Start();

                _logger.LogTrace($"Handler (HID:{handler.Context.Id}) Start: [{handler.Manifest.Count()}] Total Actions Queued.");


                var valid = await invoker.Invoke<IValidate>(handler);
                if (!valid.Success)
                    return valid;

                var queried = await invoker.Invoke<IQuery>(handler);
                if (!queried.Success)
                    return queried;

                var performed = await invoker.Invoke<ICommand>(handler);
                if (performed.Success)
                    return performed;

                var compensated = await invoker.Invoke<IRollback>(handler);
                return compensated;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());

                var result = new Result() { Success = false, Data = handler.Context.Data };

                result.Cancelled = (ex is OperationCanceledException ||
                                    ex is TaskCanceledException ||
                                    handler.Context.Token.IsCancellationRequested);

                if (result.Cancelled)
                    result.AddMessage($"HID:{handler.Context.Id}, Looks like there was a cancellation request that caused your request to end prematurely.");
                else
                    result.AddMessage($"HID:{handler.Context.Id}, Looks like there was a problem with your request.");

                return result;
            }
            finally
            {
                stopwatch.Stop();

                _logger.LogTrace($"Handler (HID:{handler.Context.Id}) Complete: [{stopwatch.Elapsed.ToString(@"hh\:mm\:ss\:fff")}]");
            }
        }
    }
}
