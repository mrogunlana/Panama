using Microsoft.Extensions.DependencyInjection;
using Panama.Core.Interfaces;
using Panama.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Panama.Core
{
    public class Handler : IHandler
    {
        private IInvokeHandler<IHandler> Invoker; 
        public IList<IAction> Manifest { get; }
        private IServiceProvider provider { get; set; }

        public IContext Context { get; }
        
        public Handler(IInvokeHandler<IHandler> invoker, IServiceProvider serviceProvider)
        {
            provider = serviceProvider;
            Manifest = new List<IAction>();
            Invoker = invoker;
            Context = new Context(CancellationToken.None, Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
        }

        public IHandler Add(IModel data)
        {
            Context.Data.Add(data);

            return this;
        }
        public IHandler Add(CancellationToken token)
        {
            Context.Token = token;

            return this;
        }
        public IHandler Add(params IModel[] data)
        {
            foreach (var model in data)
                Context.Data.Add(model);

            return this;
        }
        public IHandler Add(IEnumerable<IModel> data)
        {
            foreach (var model in data)
                Context.Data.Add(model);

            return this;
        }
        public IHandler Command<Command>() where Command : ICommand
        {
            Manifest.Add(provider.GetService<Command>());

            return this;
        }
        public IHandler Query<Query>() where Query : IQuery
        {
            Manifest.Add(provider.GetService<Query>());

            return this;
        }
        public IHandler Rollback<Rollback>() where Rollback : IRollback
        {
            Manifest.Add(provider.GetService<Rollback>());

            return this;
        }
        public IHandler Validate<Validate>() where Validate : IValidate
        {
            Manifest.Add(provider.GetService<Validate>());

            return this;
        }
        
        public virtual async Task<IResult> Invoke()
        {
            return await Invoker.Invoke(this);
        }
    }
}