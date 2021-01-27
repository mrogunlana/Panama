using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Panama.Core.Commands
{
    public static class HandlerExtensions
    {
        public static List<Action> ToCommandActions(this IHandler handler)
        {
            var result = new List<Action>();
            var subject = new Subject(handler.Data, handler.Token);

            foreach (var command in handler.Commands)
                result.Add(() => { 

                    var rule = new Stopwatch();

                    if (subject.Token.IsCancellationRequested)
                        subject.Token.ThrowIfCancellationRequested();

                    rule.Reset();
                    rule.Start();

                    if (command is ICommandAsync)
                        (command as ICommandAsync)
                            .Execute(subject)
                            .ConfigureAwait(false)
                            .GetAwaiter()
                            .GetResult();
                    else
                        (command as ICommand).Execute(subject);

                    rule.Stop();

                    handler.Log?.LogTrace(command, $"HID:{handler.Id}, Command: {command.GetType().Name} Processed in [{rule.Elapsed.ToString(@"hh\:mm\:ss\:fff")}]");

                });

            return result;

        }

        public static List<Action> ToRollbackActions(this IHandler handler)
        {
            var result = new List<Action>();
            var subject = new Subject(handler.Data, handler.Token);

            foreach (var command in handler.RollbackCommands)
                result.Add(() => {

                    var rule = new Stopwatch();

                    if (subject.Token.IsCancellationRequested)
                        subject.Token.ThrowIfCancellationRequested();

                    rule.Reset();
                    rule.Start();

                    (command as IRollback)
                        .Execute(subject)
                            .ConfigureAwait(false)
                            .GetAwaiter()
                            .GetResult();

                    rule.Stop();

                    handler.Log?.LogTrace(command, $"HID:{handler.Id}, Rollback Command: {command.GetType().Name} Processed in [{rule.Elapsed.ToString(@"hh\:mm\:ss\:fff")}]");

                });

            return result;

        }
    }
}
