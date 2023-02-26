using System.Collections.Generic;
using System.Threading;

namespace Panama.Core.Interfaces
{
    public interface IHandler : IContext, IInvoke
    {
        IList<IAction> Manifest { get; }
        
        IHandler Query<Query>() where Query : IQuery;
        IHandler Command<Command>() where Command : ICommand;
        IHandler Rollback<Rollback>() where Rollback : IRollback;
        IHandler Validate<Validate>() where Validate : IValidate;
        IHandler Add(IModel data);
        IHandler Add(CancellationToken token);
        IHandler Add(params IModel[] data);
        IHandler Add(IEnumerable<IModel> data);
    }
}
