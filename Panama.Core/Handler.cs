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
        private Guid _correlationId;
        private CancellationToken _token;
        private IInvokeResult<IHandler> _invoker; 

        public Guid Id => _id;
        public Guid CorrelationId => _correlationId;
        public CancellationToken Token => _token;
        public IList<IModel> Data { get; }
        public IList<IAction> Manifest { get; }

        public ILocate Locator { get; }

        public Handler(ILocate serviceLocator)
        {
            _id = Guid.NewGuid();
            _token = CancellationToken.None;
            _invoker = serviceLocator.Resolve<IInvokeResult<IHandler>>();

            Data = new List<IModel>();
            Manifest = new List<IAction>();
            Locator = serviceLocator;
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
            Manifest.Add(Locator.Resolve<Command>());

            return this;
        }
        public IHandler Query<Query>() where Query : IQuery
        {
            Manifest.Add(Locator.Resolve<Query>());

            return this;
        }
        public IHandler Rollback<Rollback>() where Rollback : IRollback
        {
            Manifest.Add(Locator.Resolve<Rollback>());

            return this;
        }
        public IHandler Validate<Validate>() where Validate : IValidate
        {
            Manifest.Add(Locator.Resolve<Validate>());

            return this;
        }
        
        public virtual async Task<IResult> Invoke()
        {
            return await _invoker.Invoke(this);
        }
    }
}