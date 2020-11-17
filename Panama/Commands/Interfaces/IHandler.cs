using Panama.Core.Entities;
using Panama.Core.IoC;
using Panama.Core.Logger;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Panama.Core.Commands
{
    public interface IHandler
    {
        ILog Log { get; }
        Guid Id { get; }
        IServiceLocator ServiceLocator { get; }
        List<object> Commands { get; set; }
        List<IModel> Data { get; set; }
        List<IValidation> Validators { get; set; }
        CancellationToken Token { get; set; }
        IHandler Command<Command>();
        IHandler Validate<Validator>() where Validator : IValidation;
        IHandler Add(IModel data);
        IHandler Add(params IModel[] data);
        IHandler Add(IEnumerable<IModel> data);
        IResult Invoke();
        Task<IResult> InvokeAsync();
    }
}
