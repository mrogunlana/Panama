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
        private readonly IInvoke<IHandler> _invoker; 
        private readonly IServiceProvider _provider;
        private readonly IContext _context;
        
        public Handler(IInvoke<IHandler> invoker, IServiceProvider provider)
        {
            _provider = provider;
            _invoker = invoker;
            _context = new Context(CancellationToken.None, Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), provider);
        }

        public IHandler Add(IModel data)
        {
            _context.Data.Add(data);

            return this;
        }
        public IHandler Add(CancellationToken token)
        {
            _context.Token = token;

            return this;
        }
        public IHandler Add(params IModel[] data)
        {
            foreach (var model in data)
                _context.Data.Add(model);

            return this;
        }
        public IHandler Add(IEnumerable<IModel> data)
        {
            foreach (var model in data)
                _context.Data.Add(model);

            return this;
        }
        public IHandler Command<Command>() where Command : ICommand
        {
            _context.Data.Add(_provider.GetService<Command>());

            return this;
        }
        public IHandler Query<Query>() where Query : IQuery
        {
            _context.Data.Add(_provider.GetService<Query>());

            return this;
        }
        public IHandler Rollback<Rollback>() where Rollback : IRollback
        {
            _context.Data.Add(_provider.GetService<Rollback>());

            return this;
        }
        public IHandler Validate<Validate>() where Validate : IValidate
        {
            _context.Data.Add(_provider.GetService<Validate>());

            return this;
        }
        
        public virtual async Task<IResult> Invoke(IContext context = null)
        {
            return await _invoker.Invoke(_context);
        }
    }
}