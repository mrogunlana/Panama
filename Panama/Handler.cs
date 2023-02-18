using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Panama.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

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
        public IList<IExecute> Manifest { get; }

        public Handler(IServiceProvider serviceProvider)
        {
            _id = Guid.NewGuid();
            _token = CancellationToken.None;
            
            Locator = serviceProvider;

            Data = new List<IModel>();
            Manifest = new List<IExecute>();

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
        public IHandler Command<Command>() where Command : ICommand
        {
            Manifest.Add(Locator.GetService<Command>());

            return this;
        }
        public IHandler Query<Query>() where Query : IQuery
        {
            Manifest.Add(Locator.GetService<Query>());

            return this;
        }
        public IHandler Rollback<Rollback>() where Rollback : IRollback
        {
            Manifest.Add(Locator.GetService<Rollback>());

            return this;
        }
        public IHandler Validate<Validate>() where Validate : IValidate
        {
            Manifest.Add(Locator.GetService<Validate>());

            return this;
        }
        
        public async Task<IResult> Invoke()
        {
            var validators = Locator.GetService<Invoker<IValidate>>();
            var queries = Locator.GetService<Invoker<IQuery>>();
            var commands = Locator.GetService<Invoker<ICommand>>();
            var rollbacks = Locator.GetService<Invoker<IRollback>>();

            var valid = await validators.Invoke(this);
            if (!valid.Success)
                return valid;

            var queried = await queries.Invoke(this);
            if (!queried.Success)
                return queried;

            var performed = await commands.Invoke(this);
            if (performed.Success)
                return performed;

            var compensated = await rollbacks.Invoke(this);
            return compensated;
        }
    }
}