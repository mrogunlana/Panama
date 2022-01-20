using Panama.Core.Entities;
using Panama.Core.IoC;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Transactions;

namespace Panama.Core.Commands
{
    public class TransactionHandler : Handler
    {
        public TransactionHandler(IServiceLocator locator) : base(locator) { }

        protected override IResult Run(List<Action> actions)
        {
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                var result = base.Run(actions);

                if (Token.IsCancellationRequested)
                    return result;

                if (result.Success)
                    scope.Complete();

                return result;
            }
        }

        protected override async Task<IResult> RunAsync(List<Action> actions)
        {
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                var result = await base.RunAsync(actions);

                if (Token.IsCancellationRequested)
                    return result;

                if (result.Success)
                    scope.Complete();

                return result;
            }
        }

        protected override IResult Validate()
        {
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                var result = base.Validate();

                if (Token.IsCancellationRequested)
                    return result;

                if (result.Success)
                    scope.Complete();

                return result;
            }
        }

        protected override async Task<IResult> ValidateAsync()
        {
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                var result = await base.ValidateAsync();

                if (Token.IsCancellationRequested)
                    return result;

                if (result.Success)
                    scope.Complete();

                return result;
            }
        }
    }
}
