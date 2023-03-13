using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Panama.Core.Extensions;
using Panama.Core.Interfaces;
using Panama.Core.Models;
using System.Diagnostics;
using System.Transactions;

namespace Panama.Core.Invokers
{
    public class ScopedInvoker : IInvoke<IHandler> 
    {
        private readonly ILogger<DefaultInvoker> _logger;

        public ScopedInvoker(ILogger<DefaultInvoker> logger)
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
                    .GetRequiredService<IInvoke<IValidate>>()
                    .Invoke(context);

                valid.EnsureSuccess();

                var queried = await context.Provider
                    .GetRequiredService<IInvoke<IQuery>>()
                    .Invoke(context);

                queried.EnsureSuccess();

                using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    var commands = await context.Provider
                        .GetRequiredService<IInvoke<ICommand>>()
                        .Invoke(context);

                    commands.ContinueWith(_ => scope.Complete());
                    if (commands.Success)
                        return commands;
                }

                using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    var compensation = await context.Provider
                        .GetRequiredService<IInvoke<IRollback>>()
                        .Invoke(context);

                    compensation.ContinueWith(o => scope.Complete());
                    compensation.EnsureSuccess();

                    return compensation;
                }
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
