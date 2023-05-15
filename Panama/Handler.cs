using Microsoft.Extensions.DependencyInjection;
using Panama.Interfaces;
using Panama.Invokers;
using Panama.Models;
using Panama.Resolvers;

namespace Panama
{
    public class Handler : IHandler
    {
        private readonly IServiceProvider _provider;
        private readonly IContext _context;
        private IInvoke<IHandler> _invoker;

        public IContext Context => _context;

        public Handler(IServiceProvider provider)
        {
            _provider = provider;
            _invoker = _provider.GetRequiredService<HandlerInvokerResolver>()(nameof(DefaultInvoker));
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
            _context.Data.Add(_context.Provider.GetRequiredService<Command>());

            return this;
        }
        public IHandler Query<Query>() where Query : IQuery
        {
            _context.Data.Add(_context.Provider.GetRequiredService<Query>());

            return this;
        }
        public IHandler Rollback<Rollback>() where Rollback : IRollback
        {
            _context.Data.Add(_context.Provider.GetRequiredService<Rollback>());

            return this;
        }
        public IHandler Validate<Validate>() where Validate : IValidate
        {
            _context.Data.Add(_context.Provider.GetRequiredService<Validate>());

            return this;
        }

        public IHandler Set<Invoker>() where Invoker : IInvoke<IHandler>
        {
            _invoker = _provider.GetRequiredService<HandlerInvokerResolver>()(typeof(Invoker).Name);
            
            return this;
        }

        public virtual async Task<IResult> Invoke(IContext? context = null)
        {
            return await _invoker.Invoke(_context);
        }
    }
}