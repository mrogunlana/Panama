using Panama.Core.Entities;
using Panama.Core.IoC;
using Panama.Core.Logger;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace Panama.Core.Commands
{
    public static class HandlerExtensions
    {
        public static List<Task> ToCommandTasks(this IHandler handler)
        {
            var result = new List<Task>();
            var subject = new Subject(handler.Data, handler.Token);

            foreach (var command in handler.Commands)
                result.Add(Task.Run(async () => {

                    var rule = new Stopwatch();

                    if (subject.Token.IsCancellationRequested)
                        subject.Token.ThrowIfCancellationRequested();

                    rule.Reset();
                    rule.Start();

                    if (command is ICommandAsync)
                        await (command as ICommandAsync).Execute(subject);
                    else
                        await Task.Run(() => {

                            if (subject.Token.IsCancellationRequested)
                                subject.Token.ThrowIfCancellationRequested();

                            (command as ICommand).Execute(subject);

                        }, subject.Token);

                    rule.Stop();

                    handler.Log?.LogTrace(command, $"HID:{handler.Id}, Command: {command.GetType().Name} Processed in [{rule.Elapsed.ToString(@"hh\:mm\:ss\:fff")}]");

                }, subject.Token));

            return result;

        }

        public static List<Task> ToRollbackTasks(this IHandler handler)
        {
            var result = new List<Task>();
            var subject = new Subject(handler.Data, handler.Token);

            foreach (var command in handler.RollbackCommands)
                result.Add(Task.Run(async () => {

                    var rule = new Stopwatch();

                    if (subject.Token.IsCancellationRequested)
                        subject.Token.ThrowIfCancellationRequested();

                    rule.Reset();
                    rule.Start();

                    await (command as IRollback).Execute(subject);

                    rule.Stop();

                    handler.Log?.LogTrace(command, $"HID:{handler.Id}, Rollback Command: {command.GetType().Name} Processed in [{rule.Elapsed.ToString(@"hh\:mm\:ss\:fff")}]");

                }, subject.Token));

            return result;

        }
    }
}
