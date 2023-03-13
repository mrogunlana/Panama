using Microsoft.Extensions.Options;
using Panama.Canal.Interfaces;
using Panama.Canal.MySQL.Extensions;
using Panama.Interfaces;

namespace Panama.Canal.MySQL
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