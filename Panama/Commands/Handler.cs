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
        protected readonly ILog _log;
        protected Guid _id = Guid.NewGuid();
        protected readonly IServiceLocator _serviceLocator;

        public List<IModel> Data { get; set; }
        public List<ICommand> Commands { get; set; }
        public List<IValidation> Validators { get; set; }
        public CancellationToken Token { get; set; }

        public Handler(IServiceLocator locator)
        {
            Data = new List<IModel>();
            Commands = new List<ICommand>();
            Validators = new List<IValidation>();
            Token = new CancellationToken();

            _serviceLocator = locator;
            _log = _serviceLocator.Resolve<ILog>();
        }

        private IResult Validate()
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

        private IResult Run()
        {
            var handler = new Stopwatch();
            var rule = new Stopwatch();
            var subject = new Subject(Data, Token);
            try
            {
                handler.Start();

                _log?.LogTrace<Handler>($"Handler (HID:{_id.ToString()}) Start: [{Commands.Count}] Commands Queued.");

                Commands.ForEach(c => {

                    if (Token.IsCancellationRequested)
                        Token.ThrowIfCancellationRequested();

                    rule.Reset();
                    rule.Start();

                    c.Execute(subject);

                    rule.Stop();

                    _log?.LogTrace(c, $"HID:{_id.ToString()}, Command: {c.GetType().Name} Processed in [{rule.Elapsed.ToString(@"hh\:mm\:ss\:fff")}]");
                });

            }
            catch (Exception ex)
            {
                _log?.LogException<Handler>(ex);

                var result = new Result()
                {
                    Success = false
                };
                result.AddMessage($"HID:{_id.ToString()}, Looks like there was a problem with your request.");
                return result;
            }
            finally
            {
                handler.Stop();

                _log?.LogTrace<Handler>($"Handler (HID:{_id.ToString()}) Complete: [{handler.Elapsed.ToString(@"hh\:mm\:ss\:fff")}]");
            }

            return new Result() { Success = true, Data = Data };
        }

        private async Task<IResult> RunAsync()
        {
            var handler = new Stopwatch();
            var rule = new Stopwatch();
            var subject = new Subject(Data, Token);

            try
            {
                handler.Start();

                _log?.LogTrace<Handler>($"Handler (HID:{_id.ToString()}) Start: [{Commands.Count}] Commands Queued.");

                //perform serial execution of commands using unblocking await task in foreach

                foreach (var command in Commands)
                    await Task.Run(() => {

                        if (Token.IsCancellationRequested)
                            Token.ThrowIfCancellationRequested();

                        rule.Reset();
                        rule.Start();

                        command.Execute(subject);

                        rule.Stop();

                        _log?.LogTrace(command, $"HID:{_id.ToString()}, Command: {command.GetType().Name} Processed in [{rule.Elapsed.ToString(@"hh\:mm\:ss\:fff")}]");

                    }, Token);
            }
            catch (Exception ex)
            {
                _log?.LogException<Handler>(ex);

                var result = new Result()
                {
                    Success = false
                };

                result.Cancelled = (ex is OperationCanceledException ||
                                    ex is TaskCanceledException ||
                                    Token.IsCancellationRequested);
                
                if (result.Cancelled)
                    result.AddMessage($"HID:{_id.ToString()}, Looks like there was a cancellation request that caused your request to end prematurely.");
                else
                    result.AddMessage($"HID:{_id.ToString()}, Looks like there was a problem with your request.");
                
                return result;
            }
            finally
            {
                handler.Stop();

                _log?.LogTrace<Handler>($"Handler (HID:{_id.ToString()}) Complete: [{handler.Elapsed.ToString(@"hh\:mm\:ss\:fff")}]");
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

        public IHandler Command<Command>() where Command : ICommand
        {
            Commands.Add(_serviceLocator.Resolve<ICommand>(typeof(Command).Name));

            return this;
        }

        public IHandler Validate<Validator>() where Validator : IValidation
        {
            Validators.Add(_serviceLocator.Resolve<IValidation>(typeof(Validator).Name));

            return this;
        }

        private async Task<IResult> ValidateAsync()
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

                    }, Token);
            }
            catch (Exception ex)
            {
                _log?.LogException<Handler>(ex);

                result.Success = false;
                result.AddMessage($"HID:{_id.ToString()}, Looks like there was a problem with your request.");
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

            //todo: add rollback command
            result = Run();
            if (!result.Success)
                return result;

            return result;
        }

        public async Task<IResult> InvokeAsync()
        {
            IResult result = await ValidateAsync();
            if (!result.Success)
                return result;

            //todo: add rollback command
            result = await RunAsync();
            if (!result.Success)
                return result;

            return result;
        }
    }
}