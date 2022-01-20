using Panama.Core.Entities;
using Panama.Core.IoC;
using Panama.Core.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;

namespace Panama.Core.Commands
{
    public class Handler : IHandler
    {
        protected Execution _processing = Execution.Serial;

        public ILog Log { get; }
        public Guid Id { get; }
        public IServiceLocator ServiceLocator { get; }
        public List<IModel> Data { get; set; }
        public List<object> Commands { get; set; }
        public List<object> RollbackCommands { get; set; }
        public List<IValidation> Validators { get; set; }
        public CancellationToken Token { get; set; }

        public Handler(IServiceLocator locator)
        {
            Data = new List<IModel>();
            Commands = new List<object>();
            RollbackCommands = new List<object>();
            Validators = new List<IValidation>();
            Token = new CancellationToken();
            ServiceLocator = locator;
            Id = Guid.NewGuid();
            Log = ServiceLocator.Resolve<ILog>();
        }

        protected virtual IResult Validate()
        {
            var result = new Result();
            var subject = new Subject(Data, Token);

            foreach (var validator in Validators)
            {
                if (Token.IsCancellationRequested)
                    Token.ThrowIfCancellationRequested();
                
                if (!validator.IsValid(subject))
                    result.AddMessage(validator.Message(subject));
            }
            result.Success = !result.Messages.Any();

            return result;
        }

        protected virtual IResult Run(List<Action> actions)
        {
            var handler = new Stopwatch();
            var rule = new Stopwatch();
            var subject = new Subject(Data, Token);
            try
            {
                handler.Start();

                Log?.LogTrace<Handler>($"Handler (HID:{Id.ToString()}) Start: [{Commands.Count}] Commands Queued.");

                foreach (var action in actions)
                    Task.Run(action)
                        .ConfigureAwait(false)
                        .GetAwaiter()
                        .GetResult();
            }
            catch (Exception ex)
            {
                Log?.LogException<Handler>(ex);

                var result = new Result() { Success = false, Data = Data };

                if (ex is ValidationException)
                    result.AddMessages(((ValidationException)ex).Messages);
                else if (ex is ServiceException)
                    result.AddMessages(((ServiceException)ex).Messages);
                else
                    result.AddMessage($"HID:{Id.ToString()}, Looks like there was a problem with your request.");

                return result;
            }
            finally
            {
                handler.Stop();

                Log?.LogTrace<Handler>($"Handler (HID:{Id.ToString()}) Complete: [{handler.Elapsed.ToString(@"hh\:mm\:ss\:fff")}]");
            }

            return new Result() { Success = true, Data = Data };
        }

        protected virtual async Task<IResult> RunAsync(List<Action> actions)
        {
            var handler = new Stopwatch();

            try
            {
                handler.Start();

                Log?.LogTrace<Handler>($"Handler (HID:{Id.ToString()}) Start: [{Commands.Count}] Commands Queued.");

                switch (_processing)
                {
                    case Execution.Serial:
                        
                        foreach (var action in actions)
                            await Task.Run(action, Token)
                                .ConfigureAwait(false);

                        break;

                    case Execution.Parallel:

                        var tasks = new List<Task>();
                        foreach (var action in actions)
                            tasks.Add(Task.Run(action, Token));

                        await Task.WhenAll(tasks);

                        break;

                    default:

                        throw new Exception($"The execution type for the handler is not found: {_processing}");
                }
            }
            catch (Exception ex)
            {
                Log?.LogException<Handler>(ex);

                var result = new Result() { Success = false, Data = Data };

                result.Cancelled = (ex is OperationCanceledException ||
                                    ex is TaskCanceledException ||
                                    Token.IsCancellationRequested);

                if (ex is ValidationException)
                    result.AddMessages(((ValidationException)ex).Messages);
                else if (ex is ServiceException)
                    result.AddMessages(((ServiceException)ex).Messages);
                else if (result.Cancelled)
                    result.AddMessage($"HID:{Id.ToString()}, Looks like there was a cancellation request that caused your request to end prematurely.");
                else
                    result.AddMessage($"HID:{Id.ToString()}, Looks like there was a problem with your request.");
                
                return result;
            }
            finally
            {
                handler.Stop();

                Log?.LogTrace<Handler>($"Handler (HID:{Id.ToString()}) Complete: [{handler.Elapsed.ToString(@"hh\:mm\:ss\:fff")}]");
            }

            return new Result() { Success = true, Data = Data };
        }

        public IHandler Add(IModel data)
        {
            Data.Add(data);

            return this;
        }

        public IHandler Add(params IModel[] data)
        {
            Data.AddRange(data);

            return this;
        }

        public IHandler Add(IEnumerable<IModel> data)
        {
            Data.AddRange(data);

            return this;
        }

        public IHandler Add(CancellationToken token)
        {
            Token = token;

            return this;
        }

        public IHandler Command<Command>()
        {
            if (typeof(Command).GetInterfaces().Contains(typeof(ICommand)))
                Commands.Add(ServiceLocator.Resolve<ICommand>(typeof(Command).Name));
            else if (typeof(Command).GetInterfaces().Contains(typeof(ICommandAsync)))
                Commands.Add(ServiceLocator.Resolve<ICommandAsync>(typeof(Command).Name));
            else
                throw new ArgumentException($"Command type(s): {string.Join(',', typeof(Command)?.GetInterfaces()?.Select(x => x.Name))} are not compatible with supported ICommand and ICommandAsync Interfaces.");

            return this;
        }

        public IHandler Rollback<Rollback>()
        {
            if (typeof(Rollback).GetInterfaces().Contains(typeof(IRollback)))
                RollbackCommands.Add(ServiceLocator.Resolve<IRollback>(typeof(Rollback).Name));
            else
                throw new ArgumentException($"Rollback type(s): {string.Join(',', typeof(Rollback)?.GetInterfaces()?.Select(x => x.Name))} are not compatible with supported the IRollback Interface.");

            return this;
        }

        public IHandler Serial()
        {
            _processing = Execution.Serial;

            return this;
        }

        public IHandler Parallel()
        {
            _processing = Execution.Parallel;

            return this;
        }

        public IHandler Validate<Validator>() where Validator : IValidation
        {
            Validators.Add(ServiceLocator.Resolve<IValidation>(typeof(Validator).Name));

            return this;
        }

        protected virtual async Task<IResult> ValidateAsync()
        {
            var result = new Result();
            var messages = new List<string>();
            var subject = new Subject(Data, Token);

            try
            {
                foreach (var validator in Validators)
                    await Task.Run(() => {

                        if (Token.IsCancellationRequested)
                            Token.ThrowIfCancellationRequested();

                        if (!validator.IsValid(subject))
                            result.AddMessage(validator.Message(subject));

                    }, Token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Log?.LogException<Handler>(ex);

                result.Success = false;
                result.AddMessage($"HID:{Id.ToString()}, Looks like there was a problem with your request.");
                return result;
            }

            result.Success = !result.Messages.Any();

            return result;
        }

        public IResult Invoke()
        {
            IResult result = Validate();
            if (!result.Success)
                return result;

            result = Run(this.ToCommandActions());
            if (result.Success) 
                return result;

            var rollback = Run(this.ToRollbackActions());
            foreach (var message in rollback.Messages)
                result.AddMessage(message);

            return result;
        }

        public async Task<IResult> InvokeAsync()
        {
            IResult result = await ValidateAsync();
            if (!result.Success)
                return result;

            result = await RunAsync(this.ToCommandActions());
            if (result.Success)
                return result;

            var rollback = await RunAsync(this.ToRollbackActions());
            foreach (var message in rollback.Messages)
                result.AddMessage(message);

            return result;
        }
    }
}