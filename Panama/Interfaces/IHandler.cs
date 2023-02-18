﻿using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Panama.Core.Interfaces
{
    public interface IHandler
    {
        Guid Id { get; set; }
        ILogger Log { get; }
        IServiceProvider Locator { get; }
        CancellationToken Token { get; set; }
        
        IList<IModel> Data { get; set; }
        IList<IQuery> Queries { get; set; }
        IList<ICommand> Commands { get; set; }
        IList<IRollback> Rollbacks { get; set; }
        IList<IValidate> Validators { get; set; }
        
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
