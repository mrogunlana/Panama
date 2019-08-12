using Panama.Core.Entities;
using System.Collections.Generic;

namespace Panama.Core.Commands
{
    public interface IHandler
    {
        List<ICommand> Commands { get; set; }
        List<IModel> Data { get; set; }
        List<IValidation> Validators { get; set; }
        IHandler Command<Command>() where Command : ICommand;
        IHandler Validate<Validator>() where Validator : IValidation;
        IHandler Add(IModel data);
        IResult Invoke();
    }
}
