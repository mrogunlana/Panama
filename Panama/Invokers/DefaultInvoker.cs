﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Panama.Extensions;
using Panama.Interfaces;
using Panama.Models;
using System.Diagnostics;

namespace Panama.Invokers
{
    public class DefaultInvoker : IInvoke<IHandler> 
    {
        private readonly ILogger<DefaultInvoker> _logger;

        public DefaultInvoker(ILogger<DefaultInvoker> logger)
        {
            _logger = logger;
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

                var commands = await context.Provider
                    .GetRequiredService<IInvoke<ICommand>>()
                    .Invoke(context)
                    .ConfigureAwait(false);

                if (commands.Success)
                    return commands;

                var compensation = await context.Provider
                    .GetRequiredService<IInvoke<IRollback>>()
                    .Invoke(context)
                    .ConfigureAwait(false);

                return compensation;
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
