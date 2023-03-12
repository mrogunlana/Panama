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
    public class HandlerInvoker : IInvoke<IHandler> 
    {
        private readonly ILogger<HandlerInvoker> _logger;

        public HandlerInvoker(ILogger<HandlerInvoker> logger)
        {
            _logger = logger;
        }

        public async Task<IResult> Invoke(IContext context)
        {
            var stopwatch = new Stopwatch();
         
            try
            {
                var manifest = context.Data.OfType<IAction>();

                stopwatch.Start();

                _logger.LogTrace($"Handler (HID:{context.Id}) Start: [{manifest.Count()}] Total Actions Queued.");

                var valid = await context.Provider
                    .GetService<IInvoke<IValidate>>()
                    .Invoke(context);
                if (!valid.Success)
                    return valid;

                var queried = await context.Provider
                    .GetService<IInvoke<IQuery>>()
                    .Invoke(context);
                if (!queried.Success)
                    return queried;

                var performed = await context.Provider
                    .GetService<IInvoke<ICommand>>()
                    .Invoke(context);
                if (performed.Success)
                    return performed;

                var compensated = await context.Provider
                    .GetService<IInvoke<IRollback>>()
                    .Invoke(context);
                return compensated;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());

                var result = new Result() { Success = false, Data = context.Data };

                result.Cancelled = (ex is OperationCanceledException ||
                                    ex is TaskCanceledException ||
                                    context.Token.IsCancellationRequested);

                if (result.Cancelled)
                    result.AddMessage($"HID:{context.Id}, Looks like there was a cancellation request that caused your request to end prematurely.");
                else
                    result.AddMessage($"HID:{context.Id}, Looks like there was a problem with your request.");

                return result;
            }
            finally
            {
                stopwatch.Stop();

                _logger.LogTrace($"Handler (HID:{context.Id}) Complete: [{stopwatch.Elapsed.ToString(@"hh\:mm\:ss\:fff")}]");
            }
        }
    }
}
