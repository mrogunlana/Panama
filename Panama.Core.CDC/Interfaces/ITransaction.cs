namespace Panama.Core.CDC.Interfaces
{
    public interface ITransaction
    {
        IServiceProvider Provider { get; }

        bool AutoCommit { get; set; }

        object? DbTransaction { get; set; }

        Task Commit(CancellationToken cancellationToken = default);

        Task Rollback(CancellationToken cancellationToken = default);
    }
}
