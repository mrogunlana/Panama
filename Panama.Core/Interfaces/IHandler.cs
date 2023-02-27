﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Panama.Core.Interfaces
{
    public interface IHandler 
    {
        IList<IAction> Manifest { get; }
        IContext Context { get; }
        IHandler Query<Query>() where Query : IQuery;
        IHandler Command<Command>() where Command : ICommand;
        IHandler Rollback<Rollback>() where Rollback : IRollback;
        IHandler Validate<Validate>() where Validate : IValidate;
        IHandler Add(IModel data);
        IHandler Add(CancellationToken token);
        IHandler Add(params IModel[] data);
        IHandler Add(IEnumerable<IModel> data);

        Task<IResult> Invoke();
    }
}
