using Panama.Core.Entities;
using Panama.Core.IoC;
using Panama.Core.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;

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

        public Handler(IServiceLocator locator)
        {
            Data = new List<IModel>();
            Commands = new List<ICommand>();
            Validators = new List<IValidation>();

            _serviceLocator = locator;
            _log = _serviceLocator.Resolve<ILog>();
        }

        private IResult Validate()
        {
            var result = new Result();

            foreach (var validator in Validators)
            {
                if (!validator.IsValid(Data))
                    result.AddMessage(validator.Message());
            }
            result.Success = !result.Messages.Any();

            return result;
        }

        private IResult Run()
        {
            var handler = new Stopwatch();
            var rule = new Stopwatch();

            try
            {
                handler.Start();

                _log?.LogTrace<Handler>($"Handler (HID:{_id.ToString()}) Start: [{Commands.Count}] Commands Queued.");

                Commands.ForEach(c => {

                    rule.Reset();
                    rule.Start();

                    c.Execute(Data);

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

            try
            {
                handler.Start();

                _log?.LogTrace<Handler>($"Handler (HID:{_id.ToString()}) Start: [{Commands.Count}] Commands Queued.");

                //perform serial execution of commands using unblocking await task in foreach

                foreach (var command in Commands)
                    await Task.Run(() => {

                        rule.Reset();
                        rule.Start();

                        command.Execute(Data);

                        rule.Stop();

                        _log?.LogTrace(command, $"HID:{_id.ToString()}, Command: {command.GetType().Name} Processed in [{rule.Elapsed.ToString(@"hh\:mm\:ss\:fff")}]");

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

        public IHandler Add(IModel data)
        {
            Data.Add(data);

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

            foreach (var validator in Validators)
                await Task.Run(() => {
                    if (!validator.IsValid(Data))
                        result.AddMessage(validator.Message());
                });

            foreach (var message in messages)
                result.AddMessage(message);

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