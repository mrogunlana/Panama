using Panama.Core.IoC;
using Panama.Core.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Panama.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace Panama.Core
{
    public class Handler : IHandler
    {
        private Guid _id;
        private CancellationToken _token;

        public Guid Id => _id;
        public ILogger Log { get; }
        public CancellationToken Token => _token;
        public IServiceProvider Locator { get; }
        public IList<IModel> Data { get; }
        public IList<IQuery> Queries { get; }
        public IList<ICommand> Commands { get; }
        public IList<IRollback> Rollbacks { get; }
        public IList<IValidate> Validators { get; }

        public Handler(IServiceProvider serviceProvider)
        {
            _id = Guid.NewGuid();
            _token = CancellationToken.None;
            
            Locator = serviceProvider;

            Data = new List<IModel>();
            Queries = new List<IQuery>();
            Commands = new List<ICommand>();
            Rollbacks = new List<IRollback>();
            Validators = new List<IValidate>();

            Log = serviceProvider.GetService<ILogger<Handler>>();
        }

        public IHandler Add(IModel data)
        {
            Data.Add(data);

            return this;
        }
        public IHandler Add(CancellationToken token)
        {
            _token = token;

            return this;
        }
        public IHandler Add(params IModel[] data)
        {
            foreach (var model in data)
                Data.Add(model);

            return this;
        }
        public IHandler Add(IEnumerable<IModel> data)
        {
            foreach (var model in data)
                Data.Add(model);

            return this;
        }
        public IHandler Add(Guid id)
        {
            _id = id;

            return this; 
        }
        
        public IHandler Command<Command>() where Command : ICommand
        {
            Commands.Add(Locator.GetService<Command>());

            return this;
        }
        public IHandler Query<Query>() where Query : IQuery
        {
            Queries.Add(Locator.GetService<Query>());

            return this;
        }
        public IHandler Rollback<Rollback>() where Rollback : IRollback
        {
            Rollbacks.Add(Locator.GetService<Rollback>());

            return this;
        }
        public IHandler Validate<Validate>() where Validate : IValidate
        {
            Validators.Add(Locator.GetService<Validate>());

            return this;
        }
        
        public Task<IResult> Invoke()
        {
            throw new NotImplementedException();
        }
    }
}