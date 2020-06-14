using Panama.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

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
        IHandler Add(params IModel[] data);
        IHandler Add(IList<IModel> data);
        IResult Invoke();
        Task<IResult> InvokeAsync();
    }
}
