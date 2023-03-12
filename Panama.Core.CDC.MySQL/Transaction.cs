using Microsoft.Extensions.Options;
using Panama.Core.CDC.Interfaces;
using Panama.Core.CDC.MySQL.Extensions;
using Panama.Core.Interfaces;

namespace Panama.Core.CDC.MySQL
{
    public class Transaction : ITransaction
    {
        public IServiceProvider Provider => throw new NotImplementedException();

        public bool AutoCommit { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public object? DbTransaction { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public Task Commit(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task Rollback(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}