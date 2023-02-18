﻿using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Panama.Core.Interfaces
{
    public interface IHandler
    {
        Guid Id { get; }
        ILogger Log { get; }
        IServiceProvider Locator { get; }
        CancellationToken Token { get; }
        
        IList<IModel> Data { get; }
        IList<IQuery> Queries { get; }
        IList<ICommand> Commands { get; }
        IList<IRollback> Rollbacks { get; }
        IList<IValidate> Validators { get; }
        
        IHandler Query<Query>() where Query : IQuery;
        IHandler Command<Command>() where Command : ICommand;
        IHandler Rollback<Rollback>() where Rollback : IRollback;
        IHandler Validate<Validate>() where Validate : IValidate;
        IHandler Add(IModel data);
        IHandler Add(CancellationToken token);
        IHandler Add(params IModel[] data);
        IHandler Add(IEnumerable<IModel> data);
        IHandler Add(Guid id);

        Task<IResult> Invoke();
    }
}
