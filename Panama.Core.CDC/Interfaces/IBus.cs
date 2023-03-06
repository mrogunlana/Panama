namespace Panama.Core.CDC.Interfaces
{
    public interface IDispatch
    {
        IServiceProvider ServiceProvider { get; }

        AsyncLocal<ITransaction> Transaction { get; }

        Task PublishAsync<T>(string name, T? contentObj, string? callbackName = null,
            CancellationToken cancellationToken = default);

        Task PublishAsync<T>(string name, T? contentObj, IDictionary<string, string?> headers,
            CancellationToken cancellationToken = default);

        Task PublishDelayAsync<T>(TimeSpan delayTime, string name, T? contentObj, IDictionary<string, string?> headers, CancellationToken cancellationToken = default);

        Task PublishDelayAsync<T>(TimeSpan delayTime, string name, T? contentObj, string? callbackName = null, CancellationToken cancellationToken = default);
    }
}
