using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Panama.Extensions;
using Panama.Interfaces;
using Panama.Models;
using System.Diagnostics;
using System.Transactions;

namespace Panama.Invokers
{
    public class ScopedInvoker : IInvoke<IHandler> 
    {
        private readonly IServiceProvider _provider;
        private readonly ILogger<ScopedInvoker> _logger;

        public ScopedInvoker(
              ILogger<ScopedInvoker> logger
            , IServiceProvider provider)
        {
            _logger = logger;
            _provider = provider;
        }

        public async Task<IResult> Invoke(IContext context)
        {
            var stopwatch = new Stopwatch();
         
            try
            {
                var manifest = context.Data.OfType<IAction>().Count();

                stopwatch.Start();

                _logger.LogTrace($"Handler (HID:{context.Id}) Start: [{manifest}] Total Actions Queued.");

                var valid = await context.Provider
                    .GetRequiredService<IInvoke<IValidate>>()
                    .Invoke(context)
                    .ConfigureAwait(false);

                valid.EnsureSuccess();

                var queried = await context.Provider
                    .GetRequiredService<IInvoke<IQuery>>()
                    .Invoke(context)
                    .ConfigureAwait(false);

                queried.EnsureSuccess();

                using (var scope = new TransactionScope(
                      TransactionScopeOption.Required
                    , new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted }))
                {
                    var commands = await context.Provider
                        .GetRequiredService<IInvoke<ICommand>>()
                        .Invoke(context)
                        .ConfigureAwait(false);

                    commands.Complete(scope);

                    if (commands.Success)
                        return commands;
                }

                using (var scope = new TransactionScope(
                      TransactionScopeOption.Required
                    , new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted }))
                {
                    var compensation = await context.Provider
                        .GetRequiredService<IInvoke<IRollback>>()
                        .Invoke(context)
                        .ConfigureAwait(false);

                    compensation.EnsureSuccess();
                    compensation.Complete(scope);

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

                context.Data.RemoveAll<IAction>();

                _logger.LogTrace($"Handler (HID:{context.Id}) Complete: [{stopwatch.Elapsed.ToString(@"hh\:mm\:ss\:fff")}]");
            }
        }
    }
}
